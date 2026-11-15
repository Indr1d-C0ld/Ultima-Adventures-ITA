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
	public class AccountStatsData
	{
		public Account Account { get; set; }
		public string AccountName { get; set; }
		public AccessLevel AccessLevel { get; set; }
		public DateTime Created { get; set; }
		public DateTime LastLogin { get; set; }
		public TimeSpan TotalGameTime { get; set; }
		public bool AccountGoldEnabled { get; set; }
		public int CurrencyThreshold { get; set; }
		public int CharacterCount { get; set; }
		public long TotalSkillValue { get; set; }
		public long TotalBackpackGold { get; set; }
		public long TotalBankGold { get; set; }
		public long AccountGoldValue { get; set; }
		public int AccountGold { get; set; }
		public int AccountPlat { get; set; }
		public long VendorGold { get; set; }
		public long TotalGoldValue { get; set; }
		public bool HasSkillCapTiers { get; set; }
		public Dictionary<int, int> SkillCapBuckets { get; set; }
		public List<CharacterStatsData> Characters { get; set; }
	}

	public class CharacterStatsData
	{
		public Mobile Mobile { get; set; }
		public string Name { get; set; }
		public Serial Serial { get; set; }
		public Map Map { get; set; }
		public Point3D Location { get; set; }
		public bool Online { get; set; }
		public int SkillsTotal { get; set; }
		public int SkillsCap { get; set; }
		public int SkillCapTier { get; set; }
		public long BackpackGold { get; set; }
		public long BankGold { get; set; }
		public long TotalGold { get; set; }
		public bool BankGoldShared { get; set; }
		public DateTime LastLogin { get; set; }
		public TimeSpan GameTime { get; set; }
		public int Fame { get; set; }
		public int Karma { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }
		public int ShortTermMurders { get; set; }
	}

	public static class AccountStatsProvider
	{
		private static readonly int[] SkillCapTiers = { 105, 110, 115, 120, 125 };

		// Found signals (data already tracked):
		// FOUND VIA SEARCH: Scripts/Server Functions/Accounting/Account.cs (Account.Created, Account.LastLogin, Account.TotalGameTime)
		// FOUND VIA SEARCH: Scripts/Mobiles/PlayerMobile.cs (PlayerMobile.LastLogin, PlayerMobile.GameTime)
		// FOUND VIA SEARCH: Server/Skills.cs (Skills.Total, Skills.Cap)
		// FOUND VIA SEARCH: Server/Mobile.cs (Kills, ShortTermMurders, Fame, Karma)
		// FOUND VIA SEARCH: Server/Items/Container.cs & Server/Items/Containers.cs (BankBox.TotalGold, Container.GetAmount)
		// FOUND VIA SEARCH: Server/IAccount.cs (AccountGold.Enabled, IAccount.TotalGold, IAccount.TotalPlat)
		// FOUND VIA SEARCH: Scripts/Engines and systems/PlayerVendor.cs (PlayerVendor.Owner, HoldGold, BankAccount)
		// FOUND VIA SEARCH: Scripts/Engines and systems/Houses/BaseHouse.cs (BaseHouse.GetHouses, BaseHouse.PlayerVendors)
		// FOUND VIA SEARCH: Scripts/Items and addons/Misc/BankCheck.cs, InvestmentCheck.cs, CurrencyBars.cs (Worth-based currency)
		public static AccountStatsData Gather(Account account)
		{
			if (account == null)
				return null;

			AccountStatsData data = new AccountStatsData
			{
				Account = account,
				AccountName = account.Username,
				AccessLevel = account.AccessLevel,
				Created = account.Created,
				LastLogin = account.LastLogin,
				TotalGameTime = account.TotalGameTime,
				AccountGoldEnabled = AccountGold.Enabled,
				CurrencyThreshold = AccountGold.CurrencyThreshold,
				SkillCapBuckets = new Dictionary<int, int>(),
				Characters = new List<CharacterStatsData>()
			};

			long totalSkillValue = 0;
			long totalBackpackGold = 0;
			long totalBankGold = 0;

			for (int i = 0; i < account.Length; i++)
			{
				Mobile mobile = account[i];
				if (mobile == null)
					continue;

				PlayerMobile player = mobile as PlayerMobile;
				CharacterStatsData character = new CharacterStatsData
				{
					Mobile = mobile,
					Name = mobile.Name,
					Serial = mobile.Serial,
					Map = mobile.Map,
					Location = mobile.Location,
					Online = mobile.NetState != null,
					SkillsTotal = mobile.Skills != null ? mobile.Skills.Total : 0,
					SkillsCap = mobile.Skills != null ? mobile.Skills.Cap : 0,
					Fame = mobile.Fame,
					Karma = mobile.Karma,
					Kills = mobile.Kills,
					Deaths = player != null ? player.Deaths : 0,
					ShortTermMurders = mobile.ShortTermMurders,
					LastLogin = player != null ? player.LastLogin : DateTime.MinValue,
					GameTime = player != null ? player.GameTime : TimeSpan.Zero,
					BankGoldShared = AccountGold.Enabled
				};

				character.SkillCapTier = GetSkillCapTier(character.SkillsCap);
				if (character.SkillCapTier > 0)
				{
					int current;
					data.SkillCapBuckets.TryGetValue(character.SkillCapTier, out current);
					data.SkillCapBuckets[character.SkillCapTier] = current + 1;
				}

				character.BackpackGold = GetContainerCurrency(mobile.Backpack);
				totalBackpackGold += character.BackpackGold;

				if (!AccountGold.Enabled)
				{
					character.BankGold = GetContainerCurrency(mobile.FindBankNoCreate());
					totalBankGold += character.BankGold;
				}
				else
				{
					BankBox bank = mobile.FindBankNoCreate();
					character.BankGold = bank != null ? bank.TotalGold : 0;
				}

				character.TotalGold = character.BackpackGold + character.BankGold;
				data.Characters.Add(character);

				totalSkillValue += character.SkillsTotal;
			}

			data.CharacterCount = data.Characters.Count;
			data.TotalSkillValue = totalSkillValue;
			data.TotalBackpackGold = totalBackpackGold;
			data.TotalBankGold = totalBankGold;
			data.HasSkillCapTiers = data.SkillCapBuckets.Count > 0;

			int accountGold;
			double accountTotalGold;
			int accountPlat;
			double accountTotalPlat;
			account.GetBalance(out accountGold, out accountTotalGold, out accountPlat, out accountTotalPlat);

			data.AccountGold = accountGold;
			data.AccountPlat = accountPlat;
			data.AccountGoldValue = accountGold + ((long)accountPlat * data.CurrencyThreshold);

			data.VendorGold = GetVendorHoldings(account);

			data.TotalGoldValue = data.TotalBackpackGold + data.VendorGold + (data.AccountGoldEnabled ? data.AccountGoldValue : data.TotalBankGold);

			return data;
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
	}
}
