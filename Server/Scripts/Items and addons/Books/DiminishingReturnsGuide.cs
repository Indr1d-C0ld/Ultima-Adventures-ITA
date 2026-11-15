using System;
using Server;

namespace Server.Items
{
        public class DiminishingReturnsGuide : BaseBook
        {
                public static readonly BookContent Content = new BookContent
                        (
                                "Diminishing Returns", "Archivist of Adventure",
                                new BookPageInfo
                                (
                                        "Diminishing Returns",
                                        "on Lodoria",
                                        "",
                                        "This primer explains",
                                        "how bonuses that push",
                                        "stats and resists past",
                                        "normal values taper",
                                        "off on this shard."
                                ),
                                new BookPageInfo
                                (
                                        "The server uses a",
                                        "weighted curve in",
                                        "AdventuresFunctions.",
                                        "Each bonus is split",
                                        "into ten steps, then",
                                        "every step shrinks as",
                                        "you approach the",
                                        "maximum allowed value."
                                ),
                                new BookPageInfo
                                (
                                        "Hits, stamina, and",
                                        "mana from items are",
                                        "run through this",
                                        "curve before being",
                                        "added. The Stat Bonus",
                                        "Cap is 115, so heavy",
                                        "stacking is softened",
                                        "instead of exploding."
                                ),
                                new BookPageInfo
                                (
                                        "Strength, dexterity,",
                                        "and intelligence item",
                                        "bonuses also pass",
                                        "through the same",
                                        "function. You still",
                                        "gain the first points",
                                        "fully, but later points",
                                        "contribute less."
                                ),
                                new BookPageInfo
                                (
                                        "Resistances follow",
                                        "their own caps. Most",
                                        "adventurers can reach",
                                        "70 in each school; a",
                                        "curse drags non-",
                                        "physical caps to 60,",
                                        "elves add +5 energy,",
                                        "and Rock Flesh fixes"
                                ),
                                new BookPageInfo
                                (
                                        "physical at 90.",
                                        "Game masters ignore",
                                        "these limits. Because",
                                        "caps are hard, no",
                                        "diminishing curve is",
                                        "needed for resists."
                                ),
                                new BookPageInfo
                                (
                                        "Outgoing weapon",
                                        "damage also uses",
                                        "diminishing returns",
                                        "after all bonuses are",
                                        "counted. Avatars are",
                                        "smoothed toward 200,",
                                        "other players toward",
                                        "150, and creatures to"
                                ),
                                new BookPageInfo
                                (
                                        "225. This keeps big",
                                        "damage spikes under",
                                        "control while still",
                                        "rewarding investment",
                                        "in skills and gear."
                                ),
                                new BookPageInfo
                                (
                                        "Practical advice:",
                                        "- Expect strong early",
                                        "returns as you stack",
                                        "bonuses.",
                                        "- Past the midpoint,",
                                        "each extra point adds",
                                        "less than the last.",
                                        "- Plan around the"
                                ),
                                new BookPageInfo
                                (
                                        "caps noted above so",
                                        "you do not overbuild",
                                        "in one place while",
                                        "missing gains in",
                                        "another."
                                )
                        );

                public override BookContent DefaultContent{ get{ return Content; } }

                [Constructable]
                public DiminishingReturnsGuide() : base( 0xFF1, false )
                {
                }

                public DiminishingReturnsGuide( Serial serial ) : base( serial )
                {
                }

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );

                        writer.Write( (int)0 ); // version
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );

                        int version = reader.ReadInt();
                }
        }
}
