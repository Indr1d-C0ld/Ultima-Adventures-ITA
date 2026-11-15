using System;
using Server;

namespace Server.Items
{
        public class BulkOrdersAndShoppesBook : BaseBook
        {
                public static readonly BookContent Content = new BookContent
                        (
                                "Bulk Orders & Shoppes", "Trade Minister",
                                new BookPageInfo
                                (
                                        "  Bulk Order Deeds",
                                        "are guild contracts",
                                        "granted to shoppe",
                                        "keepers that prove",
                                        "reliable. Complete",
                                        "tasks for customers",
                                        "and the guild will",
                                        "notice your work."
                                ),
                                new BookPageInfo
                                (
                                        "  Every finished",
                                        "order raises shoppe",
                                        "reputation. Starting",
                                        "at 2,000 reputation,",
                                        "each completion has a",
                                        "chance near one",
                                        "percent, rising to",
                                        "three as favor grows."
                                ),
                                new BookPageInfo
                                (
                                        "  Guild clerks send",
                                        "large deeds for the",
                                        "smith, bowyer,",
                                        "carpenter, and tailor",
                                        "shoppes. They bundle",
                                        "several small jobs",
                                        "into one heavy order",
                                        "to prove expertise."
                                ),
                                new BookPageInfo
                                (
                                        "  Small deeds list one",
                                        "item, material, and",
                                        "quality. Large deeds",
                                        "link matching smalls",
                                        "into a set. Craft the",
                                        "pieces, double check",
                                        "counts, and combine",
                                        "them carefully."
                                ),
                                new BookPageInfo
                                (
                                        "  Return filled deeds",
                                        "to the same vendor. They",
                                        "check quality and pay",
                                        "gold, favor, and a",
                                        "chance at trade rewards",
                                        "ranging from resources",
                                        "to tools or style",
                                        "notes for artisans."
                                ),
                                new BookPageInfo
                                (
                                        "  Keep spare deeds in",
                                        "a book or chest so",
                                        "you can match sets.",
                                        "Stock materials before",
                                        "starting large orders,",
                                        "and keep taking shoppe",
                                        "tasks to hold favor",
                                        "between big hauls."
                                )
                        );

                public override BookContent DefaultContent{ get{ return Content; } }

                [Constructable]
                public BulkOrdersAndShoppesBook() : base( Utility.Random( 0xFEF, 2 ), false )
                {
                }

                public BulkOrdersAndShoppesBook( Serial serial ) : base( serial )
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
