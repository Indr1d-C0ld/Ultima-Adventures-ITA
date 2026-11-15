using System;
using Server;

namespace Server.Items
{
        public class SoulboundPrimer : BaseBook
        {
                public static readonly BookContent Content = new BookContent(
                        "The Soulbound Primer", "Stone, Master Soulbound",
                        new BookPageInfo(
                                "SOULBOUND BASICS",
                                "",
                                "Soulbound souls live",
                                "only once. Death wipes",
                                "all memories and sends",
                                "you back as a new",
                                "person with no gear.",
                                "Rewards await the"
                        ),
                        new BookPageInfo(
                                "brave who endure this",
                                "path. Those who face",
                                "it gain unique items",
                                "and adventures that",
                                "regular souls never",
                                "touch.",
                                ""
                        ),
                        new BookPageInfo(
                                "TO BEGIN",
                                "Create a new character",
                                "and say yes when asked",
                                "to become soulbound.",
                                "Speak with the Time",
                                "Lord near the new",
                                "soul globe and ask",
                                "about 'soulbound'.",
                                "Say 'unam animam'",
                                "to accept the life",
                                "and claim your maps."
                        ),
                        new BookPageInfo(
                                "The maps mark each",
                                "entrance to the Hall",
                                "of Legends across",
                                "the lands. Seek the",
                                "Hall early; a guide",
                                "within teaches what",
                                "awaits our kind.",
                                ""
                        ),
                        new BookPageInfo(
                                "YOUR SOUL'S VAULT",
                                "Every soulbound is",
                                "tied to a phylactery,",
                                "your soul's vault.",
                                "Keep it in your pack",
                                "and double-click to",
                                "focus its power.",
                                "Temporary ones cannot"
                        ),
                        new BookPageInfo(
                                "channel essence, but",
                                "a true phylactery",
                                "stores essences that",
                                "shape your stats and",
                                "abilities. Guide the",
                                "essences well.",
                                ""
                        ),
                        new BookPageInfo(
                                "TRAVELING WITH OTHERS ",
                                " ",
                                "Soulbound parties earn",
                                "shared strength. When",
                                "all members are bound",
                                "by soul, choose a role",
                                "buff from the party",
                                "window to add extra",
                                "essence power."
                        ),
                        new BookPageInfo(
                                "The phylactery weighs",
                                "the nearby soulbound",
                                "allies in the same",
                                "region; more bonded",
                                "friends mean stronger",
                                "temporary buffs to",
                                "your essences.",
                                ""
                        )
                );

                public override BookContent DefaultContent { get { return Content; } }

                [Constructable]
                public SoulboundPrimer() : base(0xFF4, false)
                {
                }

                public SoulboundPrimer( Serial serial ) : base( serial )
                {
                }

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );

                        writer.WriteEncodedInt( (int)0 ); // version
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );

                        int version = reader.ReadEncodedInt();
                }
        }
}
