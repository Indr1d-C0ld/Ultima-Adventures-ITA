using System;
using Server;

namespace Server.Items
{
        public class HouseBuildingGuide : BaseBook
        {
                public static readonly BookContent Content = new BookContent
                        (
                                "On Housebuilding", "Architect Primus",
                                new BookPageInfo
                                (
                                        "Housebuilding in",
                                        "Ultima Adventures",
                                        "begins with a deed",
                                        "and a clear stretch",
                                        "of land. Set the",
                                        "footprint, confirm",
                                        "placement, and claim",
                                        "the plot as owner."
                                ),
                                new BookPageInfo
                                (
                                        "Two paths exist:",
                                        "pre-built deeds and",
                                        "custom construction.",
                                        "Both raise sturdy",
                                        "foundations, yet they",
                                        "differ in who swings",
                                        "the hammer and who",
                                        "bears the blisters."
                                ),
                                new BookPageInfo
                                (
                                        "Pre-built homes call",
                                        "on hired crews who",
                                        "finish walls and roofs",
                                        "the moment the deed",
                                        "hits the ground. You",
                                        "pay coin for their",
                                        "labor and materials,",
                                        "and the house appears."
                                ),
                                new BookPageInfo
                                (
                                        "Custom houses use",
                                        "your own hands. The",
                                        "tool marks a build",
                                        "area around the plot",
                                        "where floors, walls,",
                                        "and trim may rise",
                                        "one tile at a time",
                                        "within its bounds."
                                ),
                                new BookPageInfo
                                (
                                        "Bring resources:",
                                        "boards for timber,",
                                        "stone blocks for",
                                        "foundations, and",
                                        "other craftables",
                                        "the design calls for.",
                                        "The gump tallies",
                                        "what you feed it."
                                ),
                                new BookPageInfo
                                (
                                        "A humble cottage",
                                        "usually starts with",
                                        "about 200 boards and",
                                        "100 stone, enough to",
                                        "lay a floor and set",
                                        "supporting walls.",
                                        "Larger halls need",
                                        "more of each."
                                ),
                                new BookPageInfo
                                (
                                        "A skill limit guards",
                                        "custom plots to stop",
                                        "endless dirt pads from",
                                        "sprawling across",
                                        "the world. When you",
                                        "reach the cap you",
                                        "cannot expand a",
                                        "custom foundation."
                                ),
                                new BookPageInfo
                                (
                                        "Lore wise, that limit",
                                        "marks the strain of",
                                        "digging and hauling",
                                        "stone by yourself. A",
                                        "pre-built deed is",
                                        "simply you hiring",
                                        "others to do the",
                                        "heavy lifting."
                                )
                        );

                public override BookContent DefaultContent{ get{ return Content; } }

                [Constructable]
                public HouseBuildingGuide() : base( 0xFF4, false )
                {
                }

                public HouseBuildingGuide( Serial serial ) : base( serial )
                {
                }

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );

                        writer.Write( (int) 0 );
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );

                        int version = reader.ReadInt();
                }
        }
}
