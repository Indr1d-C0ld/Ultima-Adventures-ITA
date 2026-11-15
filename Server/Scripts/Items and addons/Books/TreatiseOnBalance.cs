using System;

namespace Server.Items
{
    public class TreatiseOnBalance : BlueBook
    {
        public static readonly BookContent Content = new BookContent(
            "Treatise on Balance", "Time Scholar",
            new BookPageInfo(
                "Treatise on Balance",
                "by a Time Scholar",
                "",
                "The Aether Globe holds",
                "the Balance Level, a",
                "scale between Order",
                "and Chaos. 50,000 is",
                "neutral; low means Order."),
            new BookPageInfo(
                "High levels (toward",
                "100,000) mean Chaos is",
                "strong. Low levels",
                "(near zero) show Order.",
                "As the number shifts,",
                "world color and guardian",
                "messages on the globe",
                "change to warn you."),
            new BookPageInfo(
                "Avatars pledge to",
                "Order or Chaos through",
                "the Time Lord. Balance",
                "Status marks you as",
                "Order (positive), Chaos",
                "(negative), or Observer",
                "(zero). Observers still",
                "shape the world, but"),
            new BookPageInfo(
                "their influence is",
                "halved when quests",
                "report to the Globe.",
                "Only pledged Avatars",
                "gain leaderboard ranks",
                "and bonus damage when",
                "their side leads.",
                ""),
            new BookPageInfo(
                "Double click any Globe",
                "to see standings if",
                "you are pledged.",
                "The property list shows",
                "whether Order or Chaos",
                "leads and by how many",
                "points. Colors fade",
                "from blue (Order) to red"),
            new BookPageInfo(
                "as Chaos rises.",
                "",
                "Balance Level also",
                "drives the Aether",
                "curse. Travel by recall",
                "or gate while burdened",
                "by Karma risks fame,",
                "karma, and skill loss."),
            new BookPageInfo(
                "Good karma uses the",
                "current Balance directly;",
                "evil karma uses its",
                "inverse, so the side in",
                "power is safer. Crossing",
                "facets raises the risk,",
                "and Champions are spared",
                "the curse."),
            new BookPageInfo(
                "Quests and actions can",
                "move the Balance.",
                "Gold rewards convert to",
                "influence; good deeds",
                "reduce the Balance",
                "toward Order, wicked",
                "acts raise it toward",
                "Chaos."),
            new BookPageInfo(
                "The Globe adjusts by",
                "daily ticks as well.",
                "Recent swings set the",
                "rate of change and",
                "investment returns.",
                "When swings are steady,",
                "neutrality pulls back;",
                "wild surges amplify",
                "future shifts."),
            new BookPageInfo(
                "Death penalties and",
                "resurrection costs draw",
                "on Karma and the",
                "Balance Level. When your",
                "side leads, penalties",
                "can ease; when it is",
                "losing, skills and gold",
                "fall harder."),
            new BookPageInfo(
                "Avatars who push far",
                "enough become Chaos or",
                "Order champions, gaining",
                "extra damage when their",
                "side dominates. Track",
                "your Balance Effect and",
                "use the Globe often to",
                "guide your path."));

        [Constructable]
        public TreatiseOnBalance()
            : base(false)
        {
        }

        public TreatiseOnBalance(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get
            {
                return Content;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}
