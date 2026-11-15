// Integration notes: place in Scripts/Server Functions/Admin Commands. Uses Accounts.GetAccounts() to enumerate accounts.
using System;
using Server;
using Server.AccountStatistics;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Commands
{
	public class ShardStatsCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register("ShardStats", AccessLevel.Administrator, new CommandEventHandler(OnCommand));
		}

		[Usage("ShardStats [refresh|topgold|skill <SkillName>]")]
		[Description("Shows shard-wide analytics and leaderboards.")]
		private static void OnCommand(CommandEventArgs e)
		{
			bool forceRefresh = false;
			ShardStatsGump.LeaderboardSort sort = ShardStatsGump.LeaderboardSort.TotalGold;
			SkillName? skill = null;
			int page = 0;

			if (e.Length > 0)
			{
				string arg = e.GetString(0);
				if (Insensitive.Equals(arg, "refresh"))
				{
					forceRefresh = true;
				}
				else if (Insensitive.Equals(arg, "topgold"))
				{
					page = 1;
					sort = ShardStatsGump.LeaderboardSort.TotalGold;
				}
				else if (Insensitive.Equals(arg, "skill") && e.Length > 1)
				{
					string skillName = e.GetString(1);
					SkillName parsed;
					if (Enum.TryParse(skillName, true, out parsed))
					{
						skill = parsed;
						page = 3;
					}
					else
					{
						e.Mobile.SendMessage("Unknown skill '{0}'.", skillName);
						return;
					}
				}
			}

			e.Mobile.CloseGump(typeof(ShardStatsGump));
			e.Mobile.SendGump(new ShardStatsGump(sort, page, 0, 0, 0, string.Empty, skill, forceRefresh));
		}
	}
}
