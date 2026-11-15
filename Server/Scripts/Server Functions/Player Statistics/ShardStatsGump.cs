// Integration notes: place in Scripts/Server Functions/Player Statistics. Uses ShardStatsProvider cache (Accounts.GetAccounts()).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Server;
using Server.AccountStatistics;
using Server.Gumps;
using Server.Network;

namespace Server.Gumps
{
	public class ShardStatsGump : Gump
	{
		public enum LeaderboardSort
		{
			TotalGold,
			VendorGold,
			TotalSkill,
			TotalGameTime
		}

		private const int LabelHue = 0x480;
		private const int ValueHue = 0x481;
		private const int RowsPerPage = 15;
		private const int LeaderboardTopN = 50;
		private const int BackgroundWidth = 1030;
		private const int BackgroundHeight = 660;
		private const int HeaderHeight = 40;
		private const int BackgroundPadding = 20;
		private const int ButtonRefresh = 1;
		private const int ButtonOverview = 2;
		private const int ButtonLeaderboards = 3;
		private const int ButtonSkillCaps = 4;
		private const int ButtonSkills = 5;
		private const int ButtonAccounts = 6;
		private const int AccountToggleBase = 2000;
		private const int ButtonAccountsPrev = 60;
		private const int ButtonAccountsNext = 61;
		private const int ButtonAccountsSearch = 62;
		private const int AccountsSearchEntry = 1;

		private readonly LeaderboardSort m_Sort;
		private readonly int m_Page;
		private readonly int m_ListPage;
		private readonly int m_SkillPage;
		private readonly int m_AccountPage;
		private readonly SkillName? m_SelectedSkill;
		private readonly bool m_ForceRefresh;
		private readonly string m_AccountSearch;

		public ShardStatsGump(LeaderboardSort sort, int page, int listPage, int skillPage, int accountPage, string accountSearch, SkillName? selectedSkill, bool forceRefresh) : base(40, 40)
		{
			m_Sort = sort;
			m_Page = page;
			m_ListPage = listPage;
			m_SkillPage = skillPage;
			m_AccountPage = accountPage;
			m_AccountSearch = accountSearch ?? string.Empty;
			m_SelectedSkill = selectedSkill;
			m_ForceRefresh = forceRefresh;

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			BuildGump();
		}

		private void BuildGump()
		{
			ShardStatsSnapshot snapshot = ShardStatsProvider.GetSnapshot(m_ForceRefresh);

			AddPage(0);
			AddBackground(0, 0, BackgroundWidth, BackgroundHeight, 9270);
			AddImageTiled(BackgroundPadding, BackgroundPadding, BackgroundWidth - (BackgroundPadding * 2), BackgroundHeight - (BackgroundPadding * 2), 2624);
			AddAlphaRegion(BackgroundPadding, BackgroundPadding, BackgroundWidth - (BackgroundPadding * 2), BackgroundHeight - (BackgroundPadding * 2));
			AddHtml(0, 32, BackgroundWidth, HeaderHeight, "<CENTER><BASEFONT COLOR=#FFFFFF>Shard Analytics</BASEFONT></CENTER>", false, false);
			AddButton(BackgroundWidth - 190, 22, 0xFAB, 0xFAD, ButtonRefresh, GumpButtonType.Reply, 0);
			AddLabel(BackgroundWidth - 155, 26, LabelHue, "Refresh");

			AddButton(20, 45, 0x15E1, 0x15E5, ButtonOverview, GumpButtonType.Reply, 0);
			AddLabel(50, 48, LabelHue, "Overview");
			AddButton(140, 45, 0x15E1, 0x15E5, ButtonLeaderboards, GumpButtonType.Reply, 0);
			AddLabel(170, 48, LabelHue, "Leaderboards");
			AddButton(300, 45, 0x15E1, 0x15E5, ButtonSkillCaps, GumpButtonType.Reply, 0);
			AddLabel(330, 48, LabelHue, "Skill Caps");
			AddButton(430, 45, 0x15E1, 0x15E5, ButtonSkills, GumpButtonType.Reply, 0);
			AddLabel(460, 48, LabelHue, "Skills");
			AddButton(520, 45, 0x15E1, 0x15E5, ButtonAccounts, GumpButtonType.Reply, 0);
			AddLabel(550, 48, LabelHue, "Accounts");

			if (snapshot == null)
			{
				AddLabel(20, 80, LabelHue, "No snapshot available.");
				return;
			}

			AddLabel(BackgroundWidth - 380, 45, LabelHue, "Last Build:");
			AddLabel(BackgroundWidth - 290, 45, ValueHue, snapshot.BuiltUtc.ToString("g"));

			switch (m_Page)
			{
				case 1:
					BuildLeaderboards(snapshot);
					break;
				case 2:
					BuildSkillCaps(snapshot);
					break;
					case 3:
						BuildSkillThresholds(snapshot);
						break;
					case 4:
						BuildAccounts(snapshot);
						break;
					default:
						BuildOverview(snapshot);
						break;
			}
		}

		private void BuildOverview(ShardStatsSnapshot snapshot)
		{
			int y = 90;
			AddSectionHeader(20, y, 320, "Shard Totals");
			y += 24;
			AddLabel(20, y, LabelHue, "Accounts:");
			AddLabel(140, y, ValueHue, snapshot.AccountCount.ToString(CultureInfo.InvariantCulture));
			y += 20;
			AddLabel(20, y, LabelHue, "Characters:");
			AddLabel(140, y, ValueHue, snapshot.CharacterCount.ToString(CultureInfo.InvariantCulture));
			y += 20;
			AddLabel(20, y, LabelHue, "Online Now:");
			AddLabel(140, y, ValueHue, snapshot.ActiveOnlineCount.ToString(CultureInfo.InvariantCulture));
			y += 30;

			AddLabel(20, y, LabelHue, "Total Gold:");
			AddLabel(140, y, ValueHue, FormatGold(snapshot.TotalGoldValue));
			y += 20;
			AddLabel(20, y, LabelHue, "Backpack Gold:");
			AddLabel(140, y, ValueHue, FormatGold(snapshot.TotalBackpackGold));
			y += 20;
			AddLabel(20, y, LabelHue, "Bank Gold:");
			AddLabel(140, y, ValueHue, FormatGold(snapshot.TotalBankGold));
			y += 20;
			AddLabel(20, y, LabelHue, "Account Currency:");
			AddLabel(160, y, ValueHue, FormatGold(snapshot.TotalAccountCurrencyValue));
			y += 20;
			AddLabel(20, y, LabelHue, "Vendor Gold:");
			AddLabel(140, y, ValueHue, FormatGold(snapshot.TotalVendorGold));
			y += 30;

			AddLabel(20, y, LabelHue, "Total Skill Value:");
			AddLabel(160, y, ValueHue, FormatSkillValue(snapshot.TotalSkillValue));
			y += 20;
			AddLabel(20, y, LabelHue, "Total Fame/Karma:");
			AddLabel(180, y, ValueHue, string.Format("{0:N0} / {1:N0}", snapshot.TotalFame, snapshot.TotalKarma));
			y += 20;
			AddLabel(20, y, LabelHue, "Total Kills/ST:");
			AddLabel(180, y, ValueHue, string.Format("{0:N0} / {1:N0}", snapshot.TotalKills, snapshot.TotalShortTermMurders));

			BuildTopKills(snapshot, 420, 90);
		}

		private void BuildTopKills(ShardStatsSnapshot snapshot, int x, int y)
		{
			AddSectionHeader(x, y, 320, "Top Murder Counts");
			y += 24;

			if (snapshot.TopKillsCharacters.Count == 0)
			{
				AddLabel(x, y, ValueHue, "No murder stats.");
				return;
			}

			int max = Math.Min(RowsPerPage, snapshot.TopKillsCharacters.Count);
			for (int i = 0; i < max; i++)
			{
				CharacterKillRow row = snapshot.TopKillsCharacters[i];
				AddLabel(x, y, ValueHue, string.Format("{0} ({1})", row.CharacterName, row.AccountName));
				AddLabel(x + 220, y, ValueHue, string.Format("{0:N0}", row.Kills));
				y += 18;
			}
		}

		private void BuildLeaderboards(ShardStatsSnapshot snapshot)
		{
			AddSectionHeader(20, 86, 700, "Leaderboards");
			AddLabel(20, 110, LabelHue, "Sort:");
			AddButton(70, 108, 0x15E1, 0x15E5, 20, GumpButtonType.Reply, 0);
			AddLabel(100, 110, LabelHue, "Total Gold");
			AddButton(200, 108, 0x15E1, 0x15E5, 21, GumpButtonType.Reply, 0);
			AddLabel(230, 110, LabelHue, "Vendor Gold");
			AddButton(340, 108, 0x15E1, 0x15E5, 22, GumpButtonType.Reply, 0);
			AddLabel(370, 110, LabelHue, "Total Skill");
			AddButton(470, 108, 0x15E1, 0x15E5, 23, GumpButtonType.Reply, 0);
			AddLabel(500, 110, LabelHue, "Playtime");

			List<AccountLeaderboardRow> sorted = SortAccounts(snapshot.Accounts, m_Sort);
			int start = m_ListPage * RowsPerPage;
			int end = Math.Min(start + RowsPerPage, sorted.Count);

			int y = 150;
			AddLabel(20, y, LabelHue, "Account");
			AddLabel(220, y, LabelHue, "Chars");
			AddLabel(280, y, LabelHue, "Gold");
			AddLabel(400, y, LabelHue, "Vendor");
			AddLabel(500, y, LabelHue, "Skills");
			AddLabel(620, y, LabelHue, "Time");
			y += 20;

			for (int i = start; i < end; i++)
			{
				AccountLeaderboardRow row = sorted[i];
				AddLabel(20, y, ValueHue, row.AccountName);
				AddLabel(220, y, ValueHue, row.CharacterCount.ToString(CultureInfo.InvariantCulture));
				AddLabel(280, y, ValueHue, FormatGold(row.TotalGoldValue));
				AddLabel(400, y, ValueHue, FormatGold(row.VendorGold));
				AddLabel(500, y, ValueHue, FormatSkillValue(row.TotalSkillValue));
				AddLabel(620, y, ValueHue, FormatTimeSpan(row.TotalGameTime));
				y += 18;
			}

			BuildPager(sorted.Count, m_ListPage, BackgroundWidth - 120, 560, 30, 31);
		}

		private void BuildSkillCaps(ShardStatsSnapshot snapshot)
		{
			int y = 90;
			AddSectionHeader(20, y, 420, "Skill Cap Distribution");
			y += 24;
			int tierTotal = 0;
			foreach (int tier in new[] { 105, 110, 115, 120, 125 })
			{
				if (snapshot.SkillCapTierCounts.ContainsKey(tier))
					tierTotal += snapshot.SkillCapTierCounts[tier];
			}

			if (tierTotal > 0)
			{
				AddLabel(20, y, LabelHue, "Skill Cap Tiers:");
				y += 20;

				foreach (int tier in new[] { 105, 110, 115, 120, 125 })
				{
					int count = snapshot.SkillCapTierCounts.ContainsKey(tier) ? snapshot.SkillCapTierCounts[tier] : 0;
					AddLabel(40, y, LabelHue, string.Format("{0}:", tier));
					AddLabel(100, y, ValueHue, count.ToString(CultureInfo.InvariantCulture));
					y += 18;
				}

				y += 20;
			}
			else
			{
				AddLabel(20, y, LabelHue, "Skill Cap Tiers:");
				AddLabel(160, y, ValueHue, "No 105-125 tiers detected; showing raw cap distribution.");
				y += 30;
			}

			AddLabel(20, y, LabelHue, "Raw Skill Cap Histogram:");
			y += 20;

			List<int> caps = snapshot.SkillCapRawHistogram.Keys.OrderBy(key => key).ToList();
			int column = 0;
			int baseX = 20;
			int baseY = y;
			int rowsPerColumn = 12;

			for (int i = 0; i < caps.Count; i++)
			{
				int cap = caps[i];
				int count = snapshot.SkillCapRawHistogram[cap];
				int row = i % rowsPerColumn;
				column = i / rowsPerColumn;
				int x = baseX + (column * 200);
				int rowY = baseY + (row * 18);
				AddLabel(x, rowY, LabelHue, string.Format("{0}:", FormatSkillValue(cap)));
				AddLabel(x + 90, rowY, ValueHue, count.ToString(CultureInfo.InvariantCulture));
			}
		}

		private void BuildSkillThresholds(ShardStatsSnapshot snapshot)
		{
			SkillName selectedSkill = m_SelectedSkill ?? SkillName.Alchemy;
			SkillThresholdStats stats = snapshot.SkillStats[selectedSkill];

			AddSectionHeader(20, 86, 420, "Skill Thresholds");
			AddLabel(20, 110, LabelHue, "Select Skill:");
			BuildSkillPicker(selectedSkill, 20, 130);

			int y = 130;
			int listX = 732;
			AddLabel(listX, y, LabelHue, string.Format("{0} Thresholds", selectedSkill));
			y += 20;
			foreach (int threshold in new[] { 100, 105, 110, 115, 120, 125 })
			{
				int count = stats.ThresholdCounts[threshold];
				AddLabel(listX, y, LabelHue, string.Format(">= {0}:", threshold));
				AddLabel(listX + 80, y, ValueHue, count.ToString(CultureInfo.InvariantCulture));
				y += 18;
			}

			AddLabel(listX, y + 10, LabelHue, "Top Characters:");
			int listY = y + 30;
			int start = m_SkillPage * RowsPerPage;
			int end = Math.Min(start + RowsPerPage, stats.TopCharacters.Count);

			for (int i = start; i < end; i++)
			{
				CharacterSkillRow row = stats.TopCharacters[i];
				AddLabel(listX, listY, ValueHue, string.Format("{0} ({1})", row.CharacterName, row.AccountName));
				AddLabel(listX + 230, listY, ValueHue, row.SkillValue.ToString("F1", CultureInfo.InvariantCulture));
				listY += 18;
			}

			BuildPager(stats.TopCharacters.Count, m_SkillPage, BackgroundWidth - 120, 560, 40, 41);
		}

		private void BuildAccounts(ShardStatsSnapshot snapshot)
		{
			AddSectionHeader(20, 86, 700, "Account Exclusions");

			AddLabel(20, 110, LabelHue, "Search:");
			AddTextEntry(80, 108, 200, 20, ValueHue, AccountsSearchEntry, m_AccountSearch);
			AddButton(290, 108, 0x15E1, 0x15E5, ButtonAccountsSearch, GumpButtonType.Reply, 0);
			AddLabel(315, 110, LabelHue, "Go");

			List<AccountToggleRow> accounts = GetFilteredAccounts(m_AccountSearch);
			int start = m_AccountPage * RowsPerPage;
			int end = Math.Min(start + RowsPerPage, accounts.Count);

			int y = 130;
			AddLabel(20, y, LabelHue, "Account");
			AddLabel(260, y, LabelHue, "Access");
			AddLabel(380, y, LabelHue, "Status");
			AddLabel(520, y, LabelHue, "Toggle");
			y += 20;

			if (accounts.Count == 0)
			{
				AddLabel(20, y, ValueHue, "No accounts matched.");
				return;
			}

			for (int i = start; i < end; i++)
			{
				AccountToggleRow row = accounts[i];
				bool isStaff = row.AccessLevel >= AccessLevel.Counselor;
				string status = isStaff ? "Staff (excluded)" : (row.IsExcluded ? "Excluded" : "Included");

				AddLabel(20, y, ValueHue, row.AccountName);
				AddLabel(260, y, ValueHue, row.AccessLevel.ToString());
				AddLabel(380, y, ValueHue, status);

				if (!isStaff)
				{
					int buttonId = AccountToggleBase + i;
					AddButton(520, y, 0x15E1, 0x15E5, buttonId, GumpButtonType.Reply, 0);
					AddLabel(545, y, LabelHue, row.IsExcluded ? "Include" : "Exclude");
				}
				else
				{
					AddLabel(520, y, LabelHue, "N/A");
				}

				y += 18;
			}

			BuildPager(accounts.Count, m_AccountPage, BackgroundWidth - 120, 560, ButtonAccountsPrev, ButtonAccountsNext);
		}

		private void BuildSkillPicker(SkillName selected, int x, int y)
		{
			SkillName[] skills = Enum.GetValues(typeof(SkillName)).Cast<SkillName>().ToArray();
			int rows = 10;
			int columns = (int)Math.Ceiling(skills.Length / (double)rows);
			int index = 0;

			for (int col = 0; col < columns; col++)
			{
				for (int row = 0; row < rows && index < skills.Length; row++)
				{
					SkillName skill = skills[index++];
					int buttonId = 100 + (int)skill;
					int rowY = y + (row * 18);
					int colX = x + (col * 120);
					AddButton(colX, rowY, 0x15E1, 0x15E5, buttonId, GumpButtonType.Reply, 0);
					int hue = skill == selected ? ValueHue : LabelHue;
					AddLabel(colX + 20, rowY, hue, skill.ToString());
				}
			}
		}

		private void BuildPager(int totalCount, int page, int x, int y, int prevButtonId, int nextButtonId)
		{
			if (totalCount <= RowsPerPage)
				return;

			if (page > 0)
			{
				AddButton(x - 60, y, 0x15E3, 0x15E7, prevButtonId, GumpButtonType.Reply, 0);
				AddLabel(x - 35, y + 2, LabelHue, "Prev");
			}

			if ((page + 1) * RowsPerPage < totalCount)
			{
				AddButton(x, y, 0x15E1, 0x15E5, nextButtonId, GumpButtonType.Reply, 0);
				AddLabel(x + 25, y + 2, LabelHue, "Next");
			}
		}

		private void AddSectionHeader(int x, int y, int width, string text)
		{
			AddHtml(x, y, width, 20, string.Format("<BODY BGCOLOR=#2B2B2B><BASEFONT COLOR=#C5FFAE>{0}</BASEFONT></BODY>", text), false, false);
		}

		private static List<AccountLeaderboardRow> SortAccounts(List<AccountLeaderboardRow> accounts, LeaderboardSort sort)
		{
			IEnumerable<AccountLeaderboardRow> query;

			switch (sort)
			{
				case LeaderboardSort.VendorGold:
					query = accounts.OrderByDescending(row => row.VendorGold);
					break;
				case LeaderboardSort.TotalSkill:
					query = accounts.OrderByDescending(row => row.TotalSkillValue);
					break;
				case LeaderboardSort.TotalGameTime:
					query = accounts.OrderByDescending(row => row.TotalGameTime);
					break;
				case LeaderboardSort.TotalGold:
				default:
					query = accounts.OrderByDescending(row => row.TotalGoldValue);
					break;
			}

			return query.Take(LeaderboardTopN).ToList();
		}

		private static List<AccountToggleRow> GetFilteredAccounts(string search)
		{
			IList<AccountToggleRow> accounts = ShardStatsProvider.GetAccountRows(true);

			if (string.IsNullOrWhiteSpace(search))
				return accounts.ToList();

			string term = search.Trim();
			return accounts.Where(row => row.AccountName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			int buttonId = info.ButtonID;
			if (buttonId == 0)
				return;

			int page = m_Page;
			int listPage = m_ListPage;
			int skillPage = m_SkillPage;
			int accountPage = m_AccountPage;
			LeaderboardSort sort = m_Sort;
			SkillName? selectedSkill = m_SelectedSkill;
			bool forceRefresh = false;
			string accountSearch = m_AccountSearch;

			switch (buttonId)
			{
				case ButtonRefresh:
					forceRefresh = true;
					break;
				case ButtonOverview:
					page = 0;
					break;
				case ButtonLeaderboards:
					page = 1;
					break;
				case ButtonSkillCaps:
					page = 2;
					break;
					case ButtonSkills:
						page = 3;
						break;
					case ButtonAccounts:
						page = 4;
						break;
				case 20:
					sort = LeaderboardSort.TotalGold;
					page = 1;
					listPage = 0;
					break;
				case 21:
					sort = LeaderboardSort.VendorGold;
					page = 1;
					listPage = 0;
					break;
				case 22:
					sort = LeaderboardSort.TotalSkill;
					page = 1;
					listPage = 0;
					break;
				case 23:
					sort = LeaderboardSort.TotalGameTime;
					page = 1;
					listPage = 0;
					break;
				case 30:
					listPage = Math.Max(0, listPage - 1);
					page = 1;
					break;
				case 31:
					listPage += 1;
					page = 1;
					break;
					case 40:
						skillPage = Math.Max(0, skillPage - 1);
						page = 3;
						break;
					case 41:
						skillPage += 1;
						page = 3;
						break;
					case ButtonAccountsPrev:
						accountPage = Math.Max(0, accountPage - 1);
						page = 4;
						break;
					case ButtonAccountsNext:
						accountPage += 1;
						page = 4;
						break;
					case ButtonAccountsSearch:
						TextRelay searchEntry = info.GetTextEntry(AccountsSearchEntry);
						accountSearch = searchEntry != null ? searchEntry.Text.Trim() : string.Empty;
						accountPage = 0;
						page = 4;
						break;
					default:
						if (buttonId >= AccountToggleBase)
						{
							int index = buttonId - AccountToggleBase;
							List<AccountToggleRow> accounts = GetFilteredAccounts(accountSearch);
							if (index >= 0 && index < accounts.Count)
							{
								ShardStatsProvider.ToggleAccountExcluded(accounts[index].AccountName);
								forceRefresh = true;
							}

							page = 4;
							break;
						}

						if (buttonId >= 100)
						{
							int skillValue = buttonId - 100;
							if (Enum.IsDefined(typeof(SkillName), skillValue))
							{
								selectedSkill = (SkillName)skillValue;
								skillPage = 0;
								page = 3;
							}
						}
						break;
				}

			sender.Mobile.SendGump(new ShardStatsGump(sort, page, listPage, skillPage, accountPage, accountSearch, selectedSkill, forceRefresh));
		}

		private static string FormatGold(long value)
		{
			return value.ToString("N0", CultureInfo.InvariantCulture);
		}

		private static string FormatSkillValue(long value)
		{
			return (value / 10.0).ToString("F1", CultureInfo.InvariantCulture);
		}

		private static string FormatTimeSpan(TimeSpan span)
		{
			if (span <= TimeSpan.Zero)
				return "0h";

			if (span.Days > 0)
				return string.Format("{0}d {1}h", span.Days, span.Hours);

			return string.Format("{0}h {1}m", span.Hours, span.Minutes);
		}
	}
}
