using System;
using Server;

namespace Server.Items
{
    public class MultiSpawnsGuide : BaseBook
    {
        public static readonly BookContent Content = new BookContent
        (
            "Multi-Spawns Guide", "Shard Archivist",

            new BookPageInfo
            (
                "Multi-spawns use the",
                "Premium Spawner engine",
                "to rebuild the world",
                "from .map files in",
                "Data/Monsters. Each",
                "line can drive six",
                "\"fake\" spawners with",
                "shared settings."
            ),
            new BookPageInfo
            (
                "Maps are plain text",
                "lists. A line that",
                "starts with * uses",
                "| to split fields",
                "and stores creature",
                "lists, location,",
                "timers, and counts",
                "for every slot."
            ),
            new BookPageInfo
            (
                "Creature slots one",
                "through six hold",
                "multiple entries",
                "separated by colons",
                "to add randomness",
                "per slot. Empty",
                "slots are ignored",
                "when counts are 0."
            ),
            new BookPageInfo
            (
                "After the lists you",
                "set X, Y, Z and a",
                "facet id (0 for",
                "Felucca+Trammel,",
                "1 Felucca, 2",
                "Trammel, 3 Ilshenar,",
                "4 Malas, 5 Tokuno)"
            ),
            new BookPageInfo
            (
                "Next are min and",
                "max respawn in",
                "minutes, walk and",
                "home ranges, and",
                "the SpawnID used",
                "when saving or",
                "unloading groups",
                "of related spawns."
            ),
            new BookPageInfo
            (
                "Six trailing numbers",
                "set the creature",
                "counts for each slot",
                "in order. Keep",
                "HomeRange at or",
                "below WalkRange and",
                "record coordinates",
                "on safe tiles."
            ),
            new BookPageInfo
            (
                "Use [spawngen",
                "MapName.map to read",
                "a file, [spawngen",
                "save to snapshot",
                "live spawners,",
                "[spawngen remove",
                "to wipe them, or",
                "[spawnrem per facet."
            ),
            new BookPageInfo
            (
                "Premium spawners",
                "also support",
                "override entries",
                "(map, id, min, max)",
                "to quickly retarget",
                "whole sections of",
                "a list while",
                "keeping counts."
            )
        );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public MultiSpawnsGuide() : base(0xFF2, false)
        {
            Hue = 1150;
        }

        public MultiSpawnsGuide(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }
}
