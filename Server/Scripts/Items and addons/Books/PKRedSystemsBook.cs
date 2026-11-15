using System;
using Server;

namespace Server.Items
{
        public class PKRedSystemsBook : BaseBook
        {
                public static readonly BookContent Content = new BookContent
                        (
                                "PK and Red Systems", "Archivist of Adventure",
                                new BookPageInfo
                                (
                                        "This book explains",
                                        "how PK and Red",
                                        "systems shape our",
                                        "realm.",
                                        "They were designed",
                                        "out of nostalgia",
                                        "for the perilous",
                                        "days of old."
                                ),
                                new BookPageInfo
                                (
                                        "We remember when",
                                        "open combat made",
                                        "travel thrilling.",
                                        "That feeling is",
                                        "recreated here to",
                                        "honor memories of",
                                        "Ultima's wilder",
                                        "frontiers."
                                ),
                                new BookPageInfo
                                (
                                        "PK marks those who",
                                        "choose lawless",
                                        "paths. It invites",
                                        "risk and asks",
                                        "players to weigh",
                                        "glory against the",
                                        "burden of infamy.",
                                        "Justice watches all."
                                ),
                                new BookPageInfo
                                (
                                        "Red status follows",
                                        "murder. A name in",
                                        "crimson warns the",
                                        "world that danger",
                                        "is near. It raises",
                                        "tension even when",
                                        "steel stays sheathed.",
                                        "Your choices echo."
                                ),
                                new BookPageInfo
                                (
                                        "These systems make",
                                        "every journey a",
                                        "decision. Do you",
                                        "travel armed? Do",
                                        "you bring friends",
                                        "to share risk?",
                                        "Tension turns even",
                                        "simple errands epic."
                                ),
                                new BookPageInfo
                                (
                                        "By embracing risk",
                                        "we keep adventures",
                                        "alive. Nostalgia,",
                                        "dread, and hope",
                                        "blend into stories",
                                        "worth retelling.",
                                        "Carry this guide",
                                        "and travel wisely."
                                )
                        );

                public override BookContent DefaultContent{ get{ return Content; } }

                [Constructable]
                public PKRedSystemsBook() : base( 0xFF1, false )
                {
                }

                public PKRedSystemsBook( Serial serial ) : base( serial )
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
