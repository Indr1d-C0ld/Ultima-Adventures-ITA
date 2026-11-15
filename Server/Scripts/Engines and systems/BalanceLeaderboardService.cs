using System;
using System.Collections.Generic;
using System.Linq;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public static class BalanceLeaderboardService
    {
        public static void Initialize()
        {
            AetherGlobe.UpdateAvatars();
        }

        public static void UpdateMobile(PlayerMobile pm)
        {
            if (pm == null)
                return;

            string canonicalName = pm.RawName;

            if (string.IsNullOrEmpty(canonicalName))
                canonicalName = pm.Name;

            // remove existing entries
            RemoveByName(AetherGlobe.ChaosAvatars, canonicalName);
            RemoveByName(AetherGlobe.OrderAvatars, canonicalName);

            if (!pm.Avatar || pm.BalanceStatus == 0)
                return;

            int bp = Math.Abs(pm.BalanceEffect);
            SortedDictionary<int, string> dict = pm.BalanceStatus < 0 ? AetherGlobe.ChaosAvatars : AetherGlobe.OrderAvatars;

            while (dict.ContainsKey(bp))
                bp++;

            dict[bp] = canonicalName;
        }

        private static void RemoveByName(SortedDictionary<int, string> dict, string name)
        {
            foreach (var entry in dict.Where(kvp => kvp.Value == name).ToList())
            {
                dict.Remove(entry.Key);
            }
        }
    }
}
