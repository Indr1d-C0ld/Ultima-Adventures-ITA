// Integration notes: place in Scripts/Server Functions/Player Statistics. Uses Accounts.GetAccounts() for enumeration.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Accounting;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

namespace Server.AccountStatistics
{
	public class ShardStatsSnapshot
	{
		public DateTime BuiltUtc { get; set; }
		public int AccountCount { get; set; }
		public int CharacterCount { get; set; }
		public int ActiveOnlineCount { get; set; }
		public long TotalGoldValue { get; set; }
		public long TotalBackpackGold { get; set; }
		public long TotalBankGold { get; set; }
		public long TotalVendorGold { get; set; }
		public long TotalAccountCurrencyValue { get; set; }
		public long TotalSkillValue { get; set; }
		public long TotalFame { get; set; }
		public long TotalKarma { get; set; }
		public long TotalKills { get; set; }
		public long TotalShortTermMurders { get; set; }
		public Dictionary<int, int> SkillCapTierCounts { get; set; }
		public Dictionary<int, int> SkillCapRawHistogram { get; set; }
		public List<AccountLeaderboardRow> Accounts { get; set; }
		public List<CharacterKillRow> TopKillsCharacters { get; set; }
		public Dictionary<SkillName, SkillThresholdStats> SkillStats { get; set; }
	}

	public class AccountLeaderboardRow
	{
		public string AccountName { get; set; }
		public AccessLevel AccessLevel { get; set; }
		public int CharacterCount { get; set; }
		public long TotalGoldValue { get; set; }
		public long VendorGold { get; set; }
		public long TotalSkillValue { get; set; }
		public TimeSpan TotalGameTime { get; set; }
	}

	public class SkillThresholdStats
	{
		public SkillName Skill { get; set; }
		public Dictionary<int, int> ThresholdCounts { get; set; }
		public List<CharacterSkillRow> TopCharacters { get; set; }
	}

	public class CharacterSkillRow
	{
		public string CharacterName { get; set; }
		public string AccountName { get; set; }
		public double SkillValue { get; set; }
		public int SkillBaseFixedPoint { get; set; }
		public int SkillsCap { get; set; }
		public long TotalGold { get; set; }
	}

	public class CharacterKillRow
	{
		public string CharacterName { get; set; }
		public string AccountName { get; set; }
		public int Kills { get; set; }
		public int ShortTermMurders { get; set; }
		public int Fame { get; set; }
		public int Karma { get; set; }
	}

	public class AccountToggleRow
	{
		public string AccountName { get; set; }
		public AccessLevel AccessLevel { get; set; }
		public bool IsExcluded { get; set; }
	}

	public static class ShardStatsProvider
	{
		private static readonly bool ExcludeStaffAccounts = true;
		private static readonly int[] SkillCapTiers = { 105, 110, 115, 120, 125 };
		private static readonly int[] SkillThresholds = { 100, 105, 110, 115, 120, 125 };
		private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5.0);
		private const int TopN = 50;
		private const int SkillCapBucketSize = 100; // 10.0 skill points in fixed-point
		private static readonly object SyncRoot = new object();
		private static readonly HashSet<string> ExcludedAccounts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private static ShardStatsSnapshot m_LastSnapshot;
		private static DateTime m_LastBuildUtc;
		private static bool m_IsBuilding;

		public static ShardStatsSnapshot GetSnapshot(bool forceRefresh)
		{
			lock (SyncRoot)
			{
				if (m_IsBuilding)
					return m_LastSnapshot;

				if (!forceRefresh && m_LastSnapshot != null && DateTime.UtcNow - m_LastBuildUtc < CacheTtl)
					return m_LastSnapshot;

				m_IsBuilding = true;
			}

			try
			{
				ShardStatsSnapshot snapshot = BuildSnapshot();
				lock (SyncRoot)
				{
					m_LastSnapshot = snapshot;
					m_LastBuildUtc = snapshot.BuiltUtc;
				}
				return snapshot;
			}
			finally
			{
				lock (SyncRoot)
				{
					m_IsBuilding = false;
				}
			}
		}

		public static bool IsBuilding
		{
			get { return m_IsBuilding; }
		}

		public static IList<AccountToggleRow> GetAccountRows(bool includeStaff)
		{
			List<AccountToggleRow> rows = new List<AccountToggleRow>();

			foreach (IAccount accountEntry in Accounts.GetAccounts())
			{
				Account account = accountEntry as Account;
				if (account == null)
					continue;

				if (!includeStaff && account.AccessLevel >= AccessLevel.Counselor)
					continue;

				bool excluded = IsExcludedAccount(account.Username);
				rows.Add(new AccountToggleRow
				{
					AccountName = account.Username,
					AccessLevel = account.AccessLevel,
					IsExcluded = excluded
				});
			}

			rows.Sort((left, right) => string.Compare(left.AccountName, right.AccountName, StringComparison.OrdinalIgnoreCase));
			return rows;
		}

		public static bool ToggleAccountExcluded(string accountName)
		{
			if (string.IsNullOrWhiteSpace(accountName))
				return false;

			lock (SyncRoot)
			{
				if (ExcludedAccounts.Contains(accountName))
				{
					ExcludedAccounts.Remove(accountName);
					return false;
				}

				ExcludedAccounts.Add(accountName);
				return true;
			}
		}

		public static DateTime LastBuildUtc
		{
			get { return m_LastBuildUtc; }
		}

		private static ShardStatsSnapshot BuildSnapshot()
		{
			ShardStatsSnapshot snapshot = new ShardStatsSnapshot
			{
				BuiltUtc = DateTime.UtcNow,
				SkillCapTierCounts = new Dictionary<int, int>(),
				SkillCapRawHistogram = new Dictionary<int, int>(),
				Accounts = new List<AccountLeaderboardRow>(),
				TopKillsCharacters = new List<CharacterKillRow>(),
				SkillStats = CreateSkillStats()
			};

			foreach (IAccount accountEntry in Accounts.GetAccounts())
			{
				Account account = accountEntry as Account;
				if (account == null)
					continue;

				if (ExcludeStaff(account))
					continue;

				AccountTotals accountTotals = GatherAccountTotals(account, snapshot);

				snapshot.AccountCount++;
				snapshot.CharacterCount += accountTotals.CharacterCount;
				snapshot.ActiveOnlineCount += accountTotals.OnlineCount;
				snapshot.TotalGoldValue += accountTotals.TotalGoldValue;
				snapshot.TotalBackpackGold += accountTotals.TotalBackpackGold;
				snapshot.TotalBankGold += accountTotals.TotalBankGold;
				snapshot.TotalVendorGold += accountTotals.VendorGold;
				snapshot.TotalAccountCurrencyValue += accountTotals.AccountCurrencyValue;
				snapshot.TotalSkillValue += accountTotals.TotalSkillValue;
				snapshot.TotalFame += accountTotals.TotalFame;
				snapshot.TotalKarma += accountTotals.TotalKarma;
				snapshot.TotalKills += accountTotals.TotalKills;
				snapshot.TotalShortTermMurders += accountTotals.TotalShortTermMurders;

				snapshot.Accounts.Add(new AccountLeaderboardRow
				{
					AccountName = account.Username,
					AccessLevel = account.AccessLevel,
					CharacterCount = accountTotals.CharacterCount,
					TotalGoldValue = accountTotals.TotalGoldValue,
					VendorGold = accountTotals.VendorGold,
					TotalSkillValue = accountTotals.TotalSkillValue,
					TotalGameTime = account.TotalGameTime
				});
			}

			return snapshot;
		}

		private static Dictionary<SkillName, SkillThresholdStats> CreateSkillStats()
		{
			Dictionary<SkillName, SkillThresholdStats> stats = new Dictionary<SkillName, SkillThresholdStats>();

			foreach (SkillName skill in Enum.GetValues(typeof(SkillName)))
			{
				stats[skill] = new SkillThresholdStats
				{
					Skill = skill,
					ThresholdCounts = SkillThresholds.ToDictionary(t => t, t => 0),
					TopCharacters = new List<CharacterSkillRow>()
				};
			}

			return stats;
		}

		private static bool ExcludeStaff(Account account)
		{
			if (!ExcludeStaffAccounts)
				return false;

			if (account.AccessLevel >= AccessLevel.Counselor)
				return true;

			for (int i = 0; i < account.Length; i++)
			{
				Mobile mobile = account[i];
				if (mobile != null && mobile.AccessLevel >= AccessLevel.Counselor)
					return true;
			}

			if (IsExcludedAccount(account.Username))
				return true;

			return false;
		}

		private static bool IsExcludedAccount(string accountName)
		{
			lock (SyncRoot)
			{
				return ExcludedAccounts.Contains(accountName);
			}
		}

		private static AccountTotals GatherAccountTotals(Account account, ShardStatsSnapshot snapshot)
		{
			AccountTotals totals = new AccountTotals();
			long totalBackpackGold = 0;
			long totalBankGold = 0;
			long totalSkillValue = 0;
			int onlineCount = 0;
			int characterCount = 0;
			long totalFame = 0;
			long totalKarma = 0;
			long totalKills = 0;
			long totalShortTerm = 0;

			for (int i = 0; i < account.Length; i++)
			{
				Mobile mobile = account[i];
				if (mobile == null)
					continue;

				characterCount++;

				if (mobile.NetState != null)
					onlineCount++;

				int skillCapTier = GetSkillCapTier(mobile.Skills != null ? mobile.Skills.Cap : 0);
				if (skillCapTier > 0)
					Increment(snapshot.SkillCapTierCounts, skillCapTier);

				int rawCap = mobile.Skills != null ? mobile.Skills.Cap : 0;
				Increment(snapshot.SkillCapRawHistogram, BucketSkillCap(rawCap));

				int skillsTotal = mobile.Skills != null ? mobile.Skills.Total : 0;
				totalSkillValue += skillsTotal;

				long backpackGold = GetContainerCurrency(mobile.Backpack);
				totalBackpackGold += backpackGold;

				long bankGold = 0;
				if (!AccountGold.Enabled)
				{
					bankGold = GetContainerCurrency(mobile.FindBankNoCreate());
					totalBankGold += bankGold;
				}

				UpdateSkillStats(snapshot.SkillStats, mobile, account.Username, backpackGold, bankGold);
				UpdateKills(snapshot.TopKillsCharacters, mobile, account.Username);

				totalFame += mobile.Fame;
				totalKarma += mobile.Karma;
				totalKills += mobile.Kills;
				totalShortTerm += mobile.ShortTermMurders;
			}

			long accountCurrencyValue = 0;
			if (AccountGold.Enabled)
			{
				int gold;
				double totalGold;
				int plat;
				double totalPlat;
				account.GetBalance(out gold, out totalGold, out plat, out totalPlat);
				accountCurrencyValue = gold + ((long)plat * AccountGold.CurrencyThreshold);
			}

			long vendorGold = GetVendorHoldings(account);

			totals.CharacterCount = characterCount;
			totals.OnlineCount = onlineCount;
			totals.TotalBackpackGold = totalBackpackGold;
			totals.TotalBankGold = totalBankGold;
			totals.AccountCurrencyValue = accountCurrencyValue;
			totals.VendorGold = vendorGold;
			totals.TotalSkillValue = totalSkillValue;
			totals.TotalGoldValue = totalBackpackGold + vendorGold + (AccountGold.Enabled ? accountCurrencyValue : totalBankGold);
			totals.TotalFame = totalFame;
			totals.TotalKarma = totalKarma;
			totals.TotalKills = totalKills;
			totals.TotalShortTermMurders = totalShortTerm;

			return totals;
		}

		private static void UpdateSkillStats(Dictionary<SkillName, SkillThresholdStats> stats, Mobile mobile, string accountName, long backpackGold, long bankGold)
		{
			if (mobile.Skills == null)
				return;

			long totalGold = backpackGold + bankGold;

			foreach (KeyValuePair<SkillName, SkillThresholdStats> entry in stats)
			{
				Skill skill = mobile.Skills[entry.Key];
				if (skill == null)
					continue;

				int baseFixed = skill.BaseFixedPoint;
				foreach (int threshold in SkillThresholds)
				{
					if (baseFixed >= threshold * 10)
						entry.Value.ThresholdCounts[threshold]++;
				}

				if (baseFixed > 0)
				{
					InsertTopSkill(entry.Value.TopCharacters, new CharacterSkillRow
					{
						CharacterName = mobile.Name,
						AccountName = accountName,
						SkillBaseFixedPoint = baseFixed,
						SkillValue = baseFixed / 10.0,
						SkillsCap = mobile.Skills.Cap,
						TotalGold = totalGold
					});
				}
			}
		}

		private static void UpdateKills(List<CharacterKillRow> list, Mobile mobile, string accountName)
		{
			if (mobile.Kills <= 0 && mobile.ShortTermMurders <= 0)
				return;

			InsertTopKills(list, new CharacterKillRow
			{
				CharacterName = mobile.Name,
				AccountName = accountName,
				Kills = mobile.Kills,
				ShortTermMurders = mobile.ShortTermMurders,
				Fame = mobile.Fame,
				Karma = mobile.Karma
			});
		}

		private static int GetSkillCapTier(int cap)
		{
			if (cap <= 0)
				return 0;

			int capTenths = cap / 10;
			if (SkillCapTiers.Contains(capTenths))
				return capTenths;

			int capHundredths = cap / 100;
			if (SkillCapTiers.Contains(capHundredths))
				return capHundredths;

			return 0;
		}

		private static int BucketSkillCap(int cap)
		{
			if (cap <= 0)
				return 0;

			return (cap / SkillCapBucketSize) * SkillCapBucketSize;
		}

		private static long GetContainerCurrency(Container container)
		{
			if (container == null)
				return 0;

			long total = container.GetAmount(typeof(Gold), true);

			foreach (BankCheck check in container.FindItemsByType<BankCheck>(true))
				total += check.Worth;

			foreach (InvestmentCheck check in container.FindItemsByType<InvestmentCheck>(true))
				total += check.Worth;

			foreach (BaseCurrencyBar bar in container.FindItemsByType<BaseCurrencyBar>(true))
				total += bar.Worth;

			return total;
		}

		private static long GetVendorHoldings(Account account)
		{
			long total = 0;
			HashSet<Serial> vendors = new HashSet<Serial>();

			for (int i = 0; i < account.Length; i++)
			{
				Mobile mobile = account[i];
				if (mobile == null)
					continue;

				List<BaseHouse> houses = BaseHouse.GetHouses(mobile);
				for (int h = 0; h < houses.Count; h++)
				{
					BaseHouse house = houses[h];
					if (house == null)
						continue;

					ArrayList list = house.PlayerVendors;
					for (int v = 0; v < list.Count; v++)
					{
						PlayerVendor vendor = list[v] as PlayerVendor;
						if (vendor == null)
							continue;

						Account vendorAccount = vendor.Owner != null ? vendor.Owner.Account as Account : null;
						if (vendorAccount == null || vendorAccount != account)
							continue;

						if (!vendors.Add(vendor.Serial))
							continue;

						total += vendor.HoldGold + vendor.BankAccount;
					}
				}
			}

			return total;
		}

		private static void Increment(Dictionary<int, int> dict, int key)
		{
			int current;
			dict.TryGetValue(key, out current);
			dict[key] = current + 1;
		}

		private static void InsertTopSkill(List<CharacterSkillRow> list, CharacterSkillRow entry)
		{
			int index = list.FindIndex(row => entry.SkillBaseFixedPoint > row.SkillBaseFixedPoint);
			if (index < 0)
				list.Add(entry);
			else
				list.Insert(index, entry);

			if (list.Count > TopN)
				list.RemoveAt(list.Count - 1);
		}

		private static void InsertTopKills(List<CharacterKillRow> list, CharacterKillRow entry)
		{
			int index = list.FindIndex(row => entry.Kills > row.Kills);
			if (index < 0)
				list.Add(entry);
			else
				list.Insert(index, entry);

			if (list.Count > TopN)
				list.RemoveAt(list.Count - 1);
		}

		private class AccountTotals
		{
			public int CharacterCount { get; set; }
			public int OnlineCount { get; set; }
			public long TotalBackpackGold { get; set; }
			public long TotalBankGold { get; set; }
			public long AccountCurrencyValue { get; set; }
			public long VendorGold { get; set; }
			public long TotalGoldValue { get; set; }
			public long TotalSkillValue { get; set; }
			public long TotalFame { get; set; }
			public long TotalKarma { get; set; }
			public long TotalKills { get; set; }
			public long TotalShortTermMurders { get; set; }
		}
	}
}
