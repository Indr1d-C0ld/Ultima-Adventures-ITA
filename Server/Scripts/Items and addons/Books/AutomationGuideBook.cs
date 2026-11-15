using System;
using Server;

namespace Server.Items
{
        public class AutomationGuideBook : BaseBook
        {
                public static readonly BookContent Content = new BookContent
                        (
                                "Automation of Tasks", "Compiled by the Tinker",
                                new BookPageInfo
                                (
                                        "Adventures automation helps ",
                                        "you repeat gathering and",
                                        "crafting actions through",
                                        "short spoken commands.",
                                        "Use this guide to learn how ",
                                        "to trigger and stop tasks. ",
                                        "Automation respects your ",
                                        "current skills and tools."
                                ),
                                new BookPageInfo
                                (
                                        "Begin with the phrases",
                                        "\"I wish to make\",",
                                        "\"I wish to start\", or",
                                        "\"I wish to have\".",
                                        "Follow them with an action ",
                                        "keyword to pick a task.",
                                        "Saying \"I wish to make list\" ",
                                        "lists every action available. "
                                ),
                                new BookPageInfo
                                (
                                        "Gathering tasks include:",
                                        "- Fishing with a pole in hand.",
                                        "- Mining with a pick or shovel.",
                                        "- Lumberjacking with an axe.",
                                        "- Skinning using knife and",
                                        "  scissors carried in pack.",
                                        "- Milling wheat near a mill.",
                                        ""
                                ),
                                new BookPageInfo
                                (
                                        "Crafting and prep tasks:",
                                        "- Making dough while near",
                                        "  water using cooking tools.",
                                        "- Baking bread beside an",
                                        "  oven with the same tools.",
                                        "- Smelting ore beside a forge.",
                                        "- Cutting logs at a sawmill.",
                                        ""
                                ),
                                new BookPageInfo
                                (
                                        "Automation remembers your ",
                                        "tools and location before",
                                        "starting, then repeats the ",
                                        "task after each delay.",
                                        "Be sure you have materials ",
                                        "and are close enough to",
                                        "your target or workshop",
                                        "before you begin."
                                ),
                                new BookPageInfo
                                (
                                        "To stop any task say",
                                        "\"I wish to stop\".",
                                        "Moving, removing tools, or",
                                        "running out of resources",
                                        "will also end the routine",
                                        "and return control to you. ",
                                        "Stay alert while tasks run ",
                                        "to avoid unwanted trouble. "
                                ),
                                new BookPageInfo
                                (
                                        "Special words:",
                                        "- \"I wish to strip\" removes",
                                        "  gear to ready you for work.",
                                        "- \"I wish to sacrifice\" pays",
                                        "  homage when your balance",
                                        "  is steady or above.",
                                        "Use these phrases exactly",
                                        "to guide the automation item."
                                ),
                                new BookPageInfo
                                (
                                        "Special words: Good luck!"

                                )
                        );

                public override BookContent DefaultContent{ get{ return Content; } }

                [Constructable]
                public AutomationGuideBook() : base( 0xFF1, false )
                {
                        Hue = 0x489;
                }

                public AutomationGuideBook( Serial serial ) : base( serial )
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
