using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Items;
using Server.Multis;

namespace Server.Commands
{
    public static class LockdownHouseAll
    {
        public static void Initialize()
        {
            CommandSystem.Register("LockdownHouseAll", AccessLevel.GameMaster, new CommandEventHandler(OnCommand));
            CommandSystem.Register("LockAllHouseItems", AccessLevel.GameMaster, new CommandEventHandler(OnCommand));
        }

        [Usage("LockdownHouseAll")]
        [Description("Locks down all movable world items inside the house you are standing in.")]
        private static void OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            BaseHouse house = BaseHouse.FindHouseAt(from);

            if (house == null)
            {
                from.SendMessage("You must be standing inside a house to use this command.");
                return;
            }

            int scanned = 0;
            int locked = 0;
            int skipped = 0;

            List<Item> candidates = new List<Item>();

            foreach (Item item in World.Items.Values)
            {
                if (item == null || item.Deleted)
                    continue;

                if (item.Map != house.Map || item.Parent != null)
                    continue;

                if (!house.IsInside(item))
                    continue;

                scanned++;

                if (!item.Movable || item.IsLockedDown || item.IsSecure || item is HouseSign || item is Static)
                {
                    skipped++;
                    continue;
                }

                candidates.Add(item);
            }

            foreach (Item item in candidates)
            {
                int before = house.LockDownCount;
                house.SetLockdown(item, true);

                if (house.IsLockedDown(item) || house.LockDownCount > before)
                    locked++;
                else
                    skipped++;
            }

            from.SendMessage("House lockdown complete. Scanned: {0}, Locked down: {1}, Skipped: {2}.", scanned, locked, skipped);
        }
    }
}
