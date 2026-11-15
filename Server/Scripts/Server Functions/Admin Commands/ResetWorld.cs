using System.Collections.Generic;
using System.Linq;
using Knives.TownHouses;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

namespace Server.Commands
{
    public class ResetWorldCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("ClearHouses", AccessLevel.Administrator, new CommandEventHandler(ClearHouses));
            CommandSystem.Register("ClearVendors", AccessLevel.Administrator, new CommandEventHandler(ClearVendors));
            CommandSystem.Register("ResetWorld", AccessLevel.Administrator, new CommandEventHandler(ResetWorld));
        }

        public static void ClearHouses(CommandEventArgs args)
        {
            if (args.Mobile != null)
            {
                args.Mobile.SendGump(new ConfirmationGump(args.Mobile, "This will clear ALL houses and boats that are not marked as Ageless. Are you absolutely certain?", () => ClearHousesInternal()));
            }
            else
            {
                ClearHousesInternal();
            }
        }

        public static void ClearVendors(CommandEventArgs args)
        {
            if (args.Mobile != null)
            {
                args.Mobile.SendGump(new ConfirmationGump(args.Mobile, "This will clear ALL Player Vendors. Are you absolutely certain?", () => ClearVendorsInternal()));
            }
            else
            {
                ClearVendorsInternal();
            }
        }

        public static void ResetWorld(CommandEventArgs args)
        {
            if (args.Mobile != null)
            {
                args.Mobile.SendGump(new ConfirmationGump(args.Mobile, "This will clear ALL Player Vendors and houses/boats that are not marked as Ageless. Are you absolutely certain?", () =>
                {
                    ClearHousesInternal();
                    ClearVendorsInternal();
                }));
            }
            else
            {
                ClearHousesInternal();
                ClearVendorsInternal();
            }
        }

        private static void ClearVendorsInternal()
        {
            // Clean up lingering items
            World.Mobiles.Values
                .Where(mobile => mobile is PlayerVendor)
                .ToList()
                .ForEach(item => item.Delete());
        }

        private static void ClearHousesInternal()
        {
            // Warning: This does not work on Mobiles in houses

            var toDelete = new List<IEntity>();
            foreach (var entity in World.Items.Values.Where(x => x is BaseHouse).Cast<BaseHouse>().ToList())
            {
                // Keep ageless townhomes
                if (entity is TownHouse && entity.DecayType == DecayType.Ageless) continue;

                var entities = entity.GetHouseEntities();

                // Keep any house with a LinkedGate
                // if (entities.Any(i => i is LinkedGate)) continue;

                entity.GetMobiles().Where(mobile => (mobile is PlayerMobile) == false).ToList().ForEach(mobile => mobile.Delete());
                entity.Items.ForEach(item => item.Delete());

                entities.ForEach(e => e.Delete());

                if (entity is TownHouse) continue;

                toDelete.Add(entity);
            }

            // Delete all items in townhome boundaries
            foreach (var entity in World.Items.Values.Where(item => item is TownHouseSign).Cast<TownHouseSign>().ToList())
            {
                if (entity.Blocks.Count < 1) continue;

                int minX = ((Rectangle2D)entity.Blocks[0]).Start.X;
                int minY = ((Rectangle2D)entity.Blocks[0]).Start.Y;
                int maxX = ((Rectangle2D)entity.Blocks[0]).End.X;
                int maxY = ((Rectangle2D)entity.Blocks[0]).End.Y;

                foreach (Rectangle2D rect in entity.Blocks)
                {
                    if (rect.Start.X < minX) minX = rect.Start.X;
                    if (rect.Start.Y < minY) minY = rect.Start.Y;
                    if (rect.End.X > maxX) maxX = rect.End.X;
                    if (rect.End.Y > maxY) maxY = rect.End.Y;
                }

                var enumerable = entity.Map.GetObjectsInBounds(new Rectangle2D(minX, minY, maxX - minX, maxY - minY));
                foreach (var e in enumerable)
                {
                    var item = e as Item;
                    if (item == null || item is Static || item is TownHouseSign || item is BaseDoor) continue;

                    if (item.Parent == null && !item.IsLockedDown && !item.IsSecure)
                    {
                        item.Delete();
                    }
                }
                enumerable.Free();
            }

            // Clean up lingering items
            World.Items.Values
                .Where(item => item.Parent == null && (item is TownHouse) == false && !item.IsLockedDown && !item.IsSecure && BaseHouse.FindHouseAt(item) != null)
                .ToList()
                .ForEach(item => item.Delete());

            toDelete.ForEach(e => e.Delete());

            foreach (var entity in World.Items.Values.Where(x => x is BaseBoat).Cast<BaseBoat>().ToList())
            {
                if (entity.Ageless) continue;

                entity.GetMovingEntities().ForEach(e => e.Delete());
                entity.Items.ForEach(item => item.Delete());
                entity.Delete();
            }
        }
    }
}