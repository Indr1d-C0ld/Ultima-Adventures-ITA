using System;
using Server;

namespace Server.Items
{
        public class BankVendorGuide : BaseBook
        {
                public static readonly BookContent Content = new BookContent
                        (
                                "Bank Vendor Ledgers", "The Royal Treasurer",
                                new BookPageInfo
                                (
                                        "Random NPCs will",
                                        "now buy supplies from",
                                        "player-run vendors."

                                ),
                                new BookPageInfo
                                (
                                        "Vendors stocked with",
                                        "tools, reagents, or",
                                        "provisions are most",
                                        "attractive to clerks",
                                        "who need to restock",
                                        "the vault. Prices",
                                        "should be fair, but",
                                        "profit is expected."
                                ),
                                new BookPageInfo
                                (
                                        "Keep enough gold on",
                                        "hand in your vendor's",
                                        "till so rent can",
                                        "be made. Bank NPCs pay ",
                                        "directly from their",
                                        "ledgers, leaving coin",
                                        "with your vendor as",
                                        "soon as they buy."
                                ),
                                new BookPageInfo
                                (
                                        "  Restock daily. NPC",
                                        "interest drops if a",
                                        "vendor looks empty or",
                                        "sells only trinkets.",
                                        "Label shelves clearly",
                                        "and place bulk goods",
                                        "near the entrance for",
                                        "easy counting."
                                )
                        );

                public override BookContent DefaultContent{ get{ return Content; } }

                [Constructable]
                public BankVendorGuide() : base( Utility.Random( 0xFEF, 2 ), false )
                {
                }

                public BankVendorGuide( Serial serial ) : base( serial )
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
