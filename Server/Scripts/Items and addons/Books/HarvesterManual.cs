using System;

namespace Server.Items
{
    public class HarvesterManual : BaseBook
    {
        public static readonly BookContent Content = new BookContent(
            "Field Manual: Harvester Systems",
            "Quartermaster Corps",
            new BookPageInfo(
                "Overview",
                "Harvesters automate",
                "resource work or karmic",
                "balance rituals.",
                "Types 1-3 mine, log, or",
                "flay hides.",
                "Types 4-9 & 20 brew or ",
                "press supplies."),
            new BookPageInfo(
                "Building with a",
                "Device Kit",
                "Drop 1000 boards,",
                "ingots, or cut leather",
                "and 500 gears inside.",
                "Material decides rig:",
                "boards for lumber,",
                "ingots for mining,"),
            new BookPageInfo(
                "leather for hides.",
                "Quality follows the",
                "material point value:",
                "poor, standard, or",
                "perfect builds.",
                "Double-click the charged",
                "kit to assemble the",
                "finished device."),
            new BookPageInfo(
                "Placement Rules",
                "Set the device on the",
                "ground to claim it.",
                "Mining rigs need cave",
                "floor or mountainside.",
                "Lumber rigs need forest",
                "or jungle terrain.",
                "Hide rigs need swamp."),
            new BookPageInfo(
                "Balance spikes belong",
                "in dungeons.",
                "Barrels and presses may",
                "sit almost anywhere,",
                "even indoors.",
                "Keep harvesters outside",
                "city limits and away",
                "from dungeon homes."),
            new BookPageInfo(
                "Do not place near houses ",
                "or other active rigs",
                "inside their control",
                "radius; higher quality",
                "machines claim larger",
                "areas.",
                string.Empty,
                string.Empty),
            new BookPageInfo(
                "Fuel & Operation",
                "Ore, lumber, and hide",
                "rigs run on copper.",
                "Balance spikes draw",
                "power from silver and",
                "your karma.",
                "Barrels accept supplies",
                "like apples, hops,"),
            new BookPageInfo(
                "grapes, honey, milk,",
                "body parts, or wheat",
                "depending on recipe.",
                "Once claimed, devices",
                "show remaining runtime",
                "and let builders collect",
                "finished goods or fuel.",
                string.Empty)
        );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public HarvesterManual() : base(0xFF0, false)
        {
            Weight = 1.0;
        }

        public HarvesterManual(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
