using System;
using Server;

namespace Server.Items
{
        public class UAFarmPatches : BaseBook
        {
                public static readonly BookContent Content = new BookContent
                        (
                                "UA Farm Patches", "Sage Amaro",
                                new BookPageInfo
                                (
                                        "Within these pages",
                                        "I record the lore of",
                                        "our patchwork farms,",
                                        "so every settler",
                                        "may coax harvests",
                                        "from the restless",
                                        "soil of Ultima",
                                        "Adventures."
                                ),
                                new BookPageInfo
                                (
                                        "A patch is a plot",
                                        "of tilled earth",
                                        "bounded by low",
                                        "ridges. Builders",
                                        "mark them with",
                                        "stakes; farmers",
                                        "measure them by",
                                        "steps of ten."
                                ),
                                new BookPageInfo
                                (
                                        "A healthy patch",
                                        "rests before first",
                                        "sowing. Let the",
                                        "grass die, break",
                                        "clods with a hoe,",
                                        "and mix compost",
                                        "until dark loam",
                                        "forms."
                                ),
                                new BookPageInfo
                                (
                                        "Seeds of grain",
                                        "love shallow beds;",
                                        "roots like onions",
                                        "crave deeper folds.",
                                        "Keep each furrow",
                                        "one hand apart",
                                        "so growth will",
                                        "breathe."
                                ),
                                new BookPageInfo
                                (
                                        "Water with the",
                                        "dawn, never at",
                                        "noon. The sun of",
                                        "Britannia steals",
                                        "moisture quick,",
                                        "and wet leaves invite",
                                        "rot by dusk."
                                ),
                                new BookPageInfo
                                (
                                        "Every third harvest",
                                        "rest the patch with",
                                        "peas or beans. Their",
                                        "roots fix air into",
                                        "the soil, gifting",
                                        "fertility to the",
                                        "next grain."
                                ),
                                new BookPageInfo
                                (
                                        "Watch for grubs",
                                        "and beetles. A",
                                        "ring of garlic",
                                        "bulbs at the edge",
                                        "keeps most from",
                                        "burrowing, while",
                                        "hens devour the",
                                        "rest."
                                ),
                                new BookPageInfo
                                (
                                        "Storms can flood",
                                        "a careless plot.",
                                        "Cut spill trenches",
                                        "on the low side",
                                        "so excess water",
                                        "escapes and does",
                                        "not drown the",
                                        "roots."
                                ),
                                new BookPageInfo
                                (
                                        "Harvest when seed",
                                        "heads bend heavy",
                                        "and stalks ring",
                                        "hollow when tapped.",
                                        "Cut just above",
                                        "the first joint",
                                        "to spare the",
                                        "crown."
                                ),
                                new BookPageInfo
                                (
                                        "Store sheaves in",
                                        "dry lofts. Damp",
                                        "grain molds within",
                                        "a week, while dry",
                                        "grain sleeps safe",
                                        "for seasons. A",
                                        "little patience",
                                        "saves plenty."
                                ),
                                new BookPageInfo
                                (
                                        "In colder moons,",
                                        "layer straw across",
                                        "bare beds. Frost",
                                        "will crack clods",
                                        "and return softened",
                                        "earth when spring",
                                        "arrives."
                                ),
                                new BookPageInfo
                                (
                                        "Share ditches,",
                                        "tools, and seed",
                                        "caches with your",
                                        "neighbors. A patch",
                                        "thrives fastest",
                                        "where the village",
                                        "thrives too."
                                ),
                                new BookPageInfo
                                (
                                        "Should blight take",
                                        "hold, burn the",
                                        "stubble and salt",
                                        "the ashes lightly.",
                                        "Rest the soil a",
                                        "season; better a",
                                        "pause than empty",
                                        "granaries."
                                )
                        );

                public override BookContent DefaultContent{ get{ return Content; } }

                [Constructable]
                public UAFarmPatches() : base( Utility.Random( 0xFEF, 2 ), false )
                {
                }

                public UAFarmPatches( Serial serial ) : base( serial )
                {
                }

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );

                        writer.WriteEncodedInt( (int) 0 ); // version
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );

                        int version = reader.ReadEncodedInt();
                }
        }
}
