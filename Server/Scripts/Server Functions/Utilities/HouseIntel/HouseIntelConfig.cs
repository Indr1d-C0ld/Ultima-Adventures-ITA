using System;
using Server;

namespace Server.Utilities
{
	public static class HouseIntelConfig
	{
		public static AccessLevel RequiredAccessLevel = AccessLevel.Administrator;
		public static TimeSpan CacheDuration = TimeSpan.FromMinutes(10.0);
		public static int DefaultInactiveDays = 60;
		public static bool IncludeAssociatedActivity = true;
		public static bool AllowPacking = true;
		public static bool AllowSubnetMatching = false;
		public static int MaxLoginHistoryEntries = 50;

		public static string AdminEscrowAccount = "houseintel";
		public static string AdminEscrowContainerName = "House Intel Escrow";
	}
}
