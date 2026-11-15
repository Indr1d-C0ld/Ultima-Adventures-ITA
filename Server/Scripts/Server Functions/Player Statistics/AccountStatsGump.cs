using System;
using System.Collections.Generic;
using System.Globalization;
using Server;
using Server.Accounting;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.AccountStatistics;

namespace Server.Gumps
{
	public class AccountStatsGump : Gump
	{
		private const int LabelHue = 0x480;
		private const int ValueHue = 0x481;
		private const int ButtonRefresh = 1;

		private readonly Account m_Account;
		private readonly AccountStatsData m_Data;

		public AccountStatsGump(Mobile from, Account account) : base(50, 50)
		{
			m_Account = account;
			m_Data = AccountStatsProvider.Gather(account);

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			BuildGump(from);
		}

		private void BuildGump(Mobile from)
		{
			AddPage(0);
			AddBackground(0, 0, 620, 430, 9270);
			AddLabel(20, 15, ValueHue, "Account Statistics");

			if (m_Data == null)
			{
				AddLabel(20, 45, LabelHue, "No account data available.");
				return;
			}

			AddLabel(20, 45, LabelHue, "Account:");
			AddLabel(120, 45, ValueHue, string.Format("{0} ({1})", m_Data.AccountName, m_Data.AccessLevel));

			int y = 70;
			AddLabel(20, y, LabelHue, "Created:");
			AddLabel(120, y, ValueHue, FormatDate(m_Data.Created));
			y += 20;

			AddLabel(20, y, LabelHue, "Last Login:");
			AddLabel(120, y, ValueHue, FormatDate(m_Data.LastLogin));
			y += 20;

			AddLabel(20, y, LabelHue, "Total Game Time:");
			AddLabel(160, y, ValueHue, FormatTimeSpan(m_Data.TotalGameTime));
			y += 20;

			AddLabel(20, y, LabelHue, "Characters:");
			AddLabel(120, y, ValueHue, m_Data.CharacterCount.ToString(CultureInfo.InvariantCulture));
			y += 30;

			AddLabel(20, y, LabelHue, "Backpack Gold:");
			AddLabel(160, y, ValueHue, FormatGold(m_Data.TotalBackpackGold));
			y += 20;

			if (m_Data.AccountGoldEnabled)
			{
				AddLabel(20, y, LabelHue, "Account Currency:");
				AddLabel(170, y, ValueHue, string.Format("{0} gold / {1} plat", FormatGold(m_Data.AccountGold), FormatGold(m_Data.AccountPlat)));
				y += 20;
			}
			else
			{
				AddLabel(20, y, LabelHue, "Bank Gold:");
				AddLabel(120, y, ValueHue, FormatGold(m_Data.TotalBankGold));
				y += 20;
			}

			AddLabel(20, y, LabelHue, "Vendor Holdings:");
			AddLabel(160, y, ValueHue, FormatGold(m_Data.VendorGold));
			y += 20;

			AddLabel(20, y, LabelHue, "Total Gold Value:");
			AddLabel(160, y, ValueHue, FormatGold(m_Data.TotalGoldValue));
			y += 30;

			AddLabel(20, y, LabelHue, "Total Skill Value:");
			AddLabel(160, y, ValueHue, FormatSkillValue(m_Data.TotalSkillValue));
			y += 20;

			if (m_Data.HasSkillCapTiers)
			{
				AddLabel(20, y, LabelHue, "Skill Cap Tiers:");
				y += 20;

				foreach (int tier in new[] { 105, 110, 115, 120, 125 })
				{
					int count;
					m_Data.SkillCapBuckets.TryGetValue(tier, out count);
					AddLabel(40, y, LabelHue, string.Format("{0}:", tier));
					AddLabel(90, y, ValueHue, count.ToString(CultureInfo.InvariantCulture));
					y += 20;
				}
			}
			else
			{
				AddLabel(20, y, LabelHue, "Skill Cap Tiers:");
				AddLabel(160, y, ValueHue, "No 105-125 tiers found; see per-character caps.");
				y += 20;
			}

			AddButton(520, 15, 0xFAB, 0xFAD, ButtonRefresh, GumpButtonType.Reply, 0);
			AddLabel(555, 18, LabelHue, "Refresh");

			if (m_Data.CharacterCount > 0)
			{
				AddButton(520, 45, 0x15E1, 0x15E5, 0, GumpButtonType.Page, 1);
				AddLabel(555, 48, LabelHue, "Characters");
			}

			for (int i = 0; i < m_Data.Characters.Count; i++)
			{
				BuildCharacterPage(i + 1, i);
			}
		}

		private void BuildCharacterPage(int page, int index)
		{
			CharacterStatsData character = m_Data.Characters[index];
			AddPage(page);
			AddBackground(0, 0, 620, 430, 9270);

			AddLabel(20, 15, ValueHue, string.Format("Character {0} of {1}", index + 1, m_Data.CharacterCount));
			AddLabel(20, 45, LabelHue, "Name:");
			AddLabel(120, 45, ValueHue, character.Name ?? "(unknown)");

			AddLabel(20, 65, LabelHue, "Serial:");
			AddLabel(120, 65, ValueHue, character.Serial.ToString());

			AddLabel(20, 85, LabelHue, "Map:");
			AddLabel(120, 85, ValueHue, character.Map != null ? character.Map.Name : "Unknown");

			AddLabel(20, 105, LabelHue, "Location:");
			AddLabel(120, 105, ValueHue, string.Format("{0}, {1}, {2}", character.Location.X, character.Location.Y, character.Location.Z));

			AddLabel(20, 125, LabelHue, "Online:");
			AddLabel(120, 125, ValueHue, character.Online ? "Yes" : "No");

			AddLabel(20, 155, LabelHue, "Last Login:");
			AddLabel(120, 155, ValueHue, FormatDate(character.LastLogin));

			AddLabel(20, 175, LabelHue, "Game Time:");
			AddLabel(120, 175, ValueHue, FormatTimeSpan(character.GameTime));

			AddLabel(20, 205, LabelHue, "Skills Total:");
			AddLabel(140, 205, ValueHue, FormatSkillValue(character.SkillsTotal));

			AddLabel(20, 225, LabelHue, "Skills Cap:");
			AddLabel(140, 225, ValueHue, FormatSkillValue(character.SkillsCap));

			AddLabel(20, 255, LabelHue, "Backpack Gold:");
			AddLabel(160, 255, ValueHue, FormatGold(character.BackpackGold));

			AddLabel(20, 275, LabelHue, character.BankGoldShared ? "Bank Gold (shared):" : "Bank Gold:");
			AddLabel(170, 275, ValueHue, FormatGold(character.BankGold));

			AddLabel(20, 295, LabelHue, "Fame/Karma:");
			AddLabel(140, 295, ValueHue, string.Format("{0:N0} / {1:N0}", character.Fame, character.Karma));

			AddLabel(20, 315, LabelHue, "Kills / Deaths:");
			AddLabel(170, 315, ValueHue, string.Format("{0:N0} / {1:N0}", character.Kills, character.Deaths));

			AddLabel(20, 335, LabelHue, "Short Term Murders:");
			AddLabel(170, 335, ValueHue, string.Format("{0:N0}", character.ShortTermMurders));

			AddButton(520, 15, 0x15E3, 0x15E7, 0, GumpButtonType.Page, 0);
			AddLabel(555, 18, LabelHue, "Summary");

			if (index > 0)
			{
				AddButton(520, 45, 0x15E3, 0x15E7, 0, GumpButtonType.Page, page - 1);
				AddLabel(555, 48, LabelHue, "Prev");
			}

			if (index < m_Data.CharacterCount - 1)
			{
				AddButton(520, 75, 0x15E1, 0x15E5, 0, GumpButtonType.Page, page + 1);
				AddLabel(555, 78, LabelHue, "Next");
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == ButtonRefresh && sender != null && sender.Mobile != null)
			{
				sender.Mobile.SendGump(new AccountStatsGump(sender.Mobile, m_Account));
			}
		}

		private static string FormatDate(DateTime value)
		{
			if (value == DateTime.MinValue)
				return "Unknown";

			return value.ToString("g");
		}

		private static string FormatTimeSpan(TimeSpan span)
		{
			if (span <= TimeSpan.Zero)
				return "0h";

			int days = span.Days;
			int hours = span.Hours;
			int minutes = span.Minutes;

			if (days > 0)
				return string.Format("{0}d {1}h {2}m", days, hours, minutes);

			return string.Format("{0}h {1}m", hours, minutes);
		}

		private static string FormatGold(long value)
		{
			return value.ToString("N0", CultureInfo.InvariantCulture);
		}

		private static string FormatSkillValue(long value)
		{
			return (value / 10.0).ToString("F1", CultureInfo.InvariantCulture);
		}
	}
}
