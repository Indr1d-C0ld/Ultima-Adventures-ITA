using System;
using Server;

namespace Server.Items
{
    public abstract class BaseGuildGuideBook : BaseBook
    {
        protected BaseGuildGuideBook()
            : base(Utility.Random(0xFF0, 2), false)
        {
        }

        public BaseGuildGuideBook(Serial serial)
            : base(serial)
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

    public class WizardsGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Wizards Guild Primer", "Archivist Kord",
            new BookPageInfo(
                "Welcome, aspirant.",
                "",
                "The Wizards Guild is",
                "the home of those",
                "who weave the Arcane",
                "Arts in Sosaria.",
                "Read on to learn of",
                "your obligations."),
            new BookPageInfo(
                "Joining requires a",
                "one time tithe of",
                "two thousand gold.",
                "Pay it directly to",
                "a guildmaster.",
                "Keep four hundred",
                "gold nearby for ring",
                "replacement requests."),
            new BookPageInfo(
                "Your guild ring marks",
                "you as kin and grants",
                "clarity of mind.",
                "+10 Magery, Eval Int",
                "and Meditation bolster",
                "your studies while it",
                "rests upon your hand.",
                "Never trade it away."),
            new BookPageInfo(
                "Guild mages share",
                "runes, reagents, and",
                "research openly.",
                "Aid fellow members,",
                "answer summons, and",
                "respect the Codex.",
                "Misuse of magics or",
                "neglect loses favor."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public WizardsGuildGuide()
        {
        }

        public WizardsGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class WarriorsGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Warriors Guild Ledger", "Captain Ersk",
            new BookPageInfo(
                "Steel binds us all.",
                "The Warriors Guild",
                "recruits any fighter",
                "of proven courage.",
                "Within these leaves",
                "learn our oath and",
                "the path to service.",
                ""),
            new BookPageInfo(
                "Bring two thousand",
                "gold to the guild",
                "quartermaster when",
                "you seek induction.",
                "Stay solvent with",
                "four hundred gold",
                "should your ring",
                "need replacement."),
            new BookPageInfo(
                "Wear the ring at all",
                "times. Its enchant",
                "strengthens arm and",
                "instinct: +10 skill",
                "in Fencing, Macing,",
                "Parrying, Swordplay",
                "and Tactics alike.",
                "Guard it fiercely."),
            new BookPageInfo(
                "Aid fellow warriors",
                "who call for steel.",
                "Keep weapons honed,",
                "train daily, and",
                "answer the city's",
                "banners when raised.",
                "Cowards find their",
                "membership revoked."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public WarriorsGuildGuide()
        {
        }

        public WarriorsGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class ThievesGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Thieves Guild Brief", "Mistress Lyra",
            new BookPageInfo(
                "Keep this hidden.",
                "The Thieves Guild",
                "accepts only those",
                "of nimble hands",
                "and shadowed hearts.",
                "Study these tenets",
                "before you petition",
                "the guildmaster."),
            new BookPageInfo(
                "Two thousand gold",
                "buys your oath.",
                "Arrive alone and",
                "leave no witnesses.",
                "Have four hundred",
                "gold ready should",
                "the ring be lost",
                "or confiscated."),
            new BookPageInfo(
                "Our rings are forged",
                "for silence: +10 to",
                "Hiding, Lockpicks,",
                "Snooping, Stealing",
                "and Stealth alike.",
                "The enchant ends",
                "the moment it leaves",
                "a guild hand."),
            new BookPageInfo(
                "Never speak names,",
                "never betray a job,",
                "and never draw",
                "heat to the safe",
                "houses. Share intel",
                "through dead drops",
                "and tithe from each",
                "successful haul."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public ThievesGuildGuide()
        {
        }

        public ThievesGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class RangersGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Rangers Field Notes", "Warden Selis",
            new BookPageInfo(
                "Walk softly, friend.",
                "The Rangers Guild",
                "guards Sosaria's",
                "wild borders.",
                "Read and remember",
                "what we expect of",
                "every sworn bow.",
                ""),
            new BookPageInfo(
                "A tithe of two",
                "thousand gold seals",
                "your pledge. Visit",
                "a ranger guildhall",
                "and pay in person.",
                "Four hundred more",
                "replaces a ring",
                "lost in the wilds."),
            new BookPageInfo(
                "Our rings sharpen",
                "instinct: +25 to",
                "Camping and",
                "Tracking arts,",
                "guiding you through",
                "night and storm.",
                "Never leave it",
                "in a city chest."),
            new BookPageInfo(
                "Answer every call",
                "to scout, heal the",
                "land, and respect",
                "the balance.",
                "Share trail signs",
                "with guildmates",
                "and keep poachers",
                "far from our wards."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public RangersGuildGuide()
        {
        }

        public RangersGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class HealersGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Healers Charter", "Sister Maren",
            new BookPageInfo(
                "Mercy binds us.",
                "The Healers Guild",
                "tends every soul",
                "without judgement.",
                "This charter records",
                "your duties and",
                "the blessings of",
                "membership."),
            new BookPageInfo(
                "Two thousand gold",
                "is rendered upon",
                "acceptance. Keep",
                "four hundred gold",
                "aside in case your",
                "ring is misplaced",
                "and must be",
                "consecrated anew."),
            new BookPageInfo(
                "While worn, the",
                "ring grants steady",
                "hands: +15 to",
                "Anatomy, Healing",
                "and Veterinary",
                "skills to aid",
                "patients of every",
                "stripe."),
            new BookPageInfo(
                "Respond to calls",
                "for aid, stock",
                "bandages in the",
                "hall, and pass on",
                "healing lore.",
                "Profiteering or",
                "neglect earns swift",
                "discipline."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public HealersGuildGuide()
        {
        }

        public HealersGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class MinersGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Miners Ledger", "Foreman Joric",
            new BookPageInfo(
                "Pick and lantern",
                "alike fall under",
                "the Miners Guild.",
                "Within lies the",
                "contract every",
                "prospector swears",
                "before descending",
                "underground."),
            new BookPageInfo(
                "Two thousand gold",
                "secures your",
                "membership, paid",
                "to the guildmaster.",
                "Four hundred more",
                "covers the cost",
                "of forging a",
                "replacement ring."),
            new BookPageInfo(
                "The ring strengthens",
                "arm and aim,",
                "granting +30 skill",
                "to all Mining",
                "endeavors. Keep it",
                "clear of slag and",
                "never lend it",
                "to outsiders."),
            new BookPageInfo(
                "Report new veins,",
                "share smelting",
                "techniques, and",
                "follow safety",
                "codes. Abandoned",
                "shafts or unpaid",
                "dues draw the ire",
                "of the foremen."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public MinersGuildGuide()
        {
        }

        public MinersGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class MerchantsGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Merchants Ledger", "Broker Thalen",
            new BookPageInfo(
                "Prosper in order.",
                "The Merchants",
                "Guild regulates",
                "trade routes and",
                "market stalls.",
                "Study these laws",
                "before hanging",
                "our sigil."),
            new BookPageInfo(
                "Tender two thousand",
                "gold for entrance",
                "and remember that",
                "four hundred gold",
                "replaces a ring",
                "should thieves or",
                "storms claim it",
                "from your hand."),
            new BookPageInfo(
                "The ring sharpens",
                "appraisal: +15 to",
                "Item ID, Arms Lore",
                "and Taste ID,",
                "ensuring fair",
                "contracts and",
                "profitable trades.",
                "Guard it closely."),
            new BookPageInfo(
                "Honor guild tariffs,",
                "share caravan",
                "warnings, and",
                "support fellow",
                "merchants. Price",
                "gouging or fraud",
                "earns swift",
                "expulsion."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public MerchantsGuildGuide()
        {
        }

        public MerchantsGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class TinkersGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Tinkerers Codex", "Artificer Dell",
            new BookPageInfo(
                "Gears and ideas",
                "turn together in",
                "the Tinkers Guild.",
                "This codex records",
                "every novice's",
                "pledge and the",
                "aid our craft",
                "provides."),
            new BookPageInfo(
                "Two thousand gold",
                "commits you to",
                "the hall. Retain",
                "four hundred gold",
                "for ring reforging",
                "should accidents",
                "claim the original",
                "during work."),
            new BookPageInfo(
                "Your ring grants",
                "steady focus:",
                "+30 Tinkering",
                "skill while worn.",
                "Never lend it",
                "as collateral nor",
                "store it in",
                "untested devices."),
            new BookPageInfo(
                "Share schematics,",
                "teach apprentices,",
                "and mark hazardous",
                "experiments clearly.",
                "Sabotage or",
                "reckless misuse",
                "invites guild",
                "censure."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public TinkersGuildGuide()
        {
        }

        public TinkersGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class TailorsGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Tailors Pattern", "Mistress Elowen",
            new BookPageInfo(
                "Thread and honor",
                "bind the Tailors",
                "Guild. This pattern",
                "details our pledge",
                "and the boons",
                "woven into each",
                "member's ring.",
                ""),
            new BookPageInfo(
                "Present two thousand",
                "gold when you",
                "apply. Keep four",
                "hundred set aside",
                "to replace a ring",
                "lost to dye vats",
                "or careless",
                "clients."),
            new BookPageInfo(
                "The ring grants",
                "+30 Tailoring",
                "skill, guiding",
                "needle and shears",
                "for flawless",
                "stitchwork. Wear",
                "it whenever you",
                "take measurements."),
            new BookPageInfo(
                "Maintain quality",
                "ledgers, teach",
                "proper fits, and",
                "share rare cloth",
                "suppliers with",
                "guild artisans.",
                "Fraudulent labels",
                "mean expulsion."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public TailorsGuildGuide()
        {
        }

        public TailorsGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class SailorsGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Sailors Almanac", "Bosun Grell",
            new BookPageInfo(
                "The Sailors Guild",
                "charts every tide",
                "and tempest. This",
                "almanac prepares",
                "new hands for",
                "life beneath our",
                "colors at sea.",
                ""),
            new BookPageInfo(
                "Render two thousand",
                "gold to the dock",
                "captain to enlist.",
                "Keep four hundred",
                "gold ready if your",
                "ring sinks or",
                "the kraken",
                "claims it."),
            new BookPageInfo(
                "The ring grants",
                "+30 skill in",
                "Fishing, steadying",
                "line and net in",
                "storm or calm.",
                "Secure it with",
                "cord lest it",
                "wash overboard."),
            new BookPageInfo(
                "Share shoal charts,",
                "answer distress",
                "flags, and keep",
                "ships seaworthy.",
                "Smuggling outside",
                "guild sanction",
                "ends your berth",
                "swiftly."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public SailorsGuildGuide()
        {
        }

        public SailorsGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class BardsGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Bards Songbook", "Maestro Piers",
            new BookPageInfo(
                "Music carries",
                "Sosaria's memory.",
                "The Bards Guild",
                "entrusts you with",
                "its chorus. Read",
                "these verses to",
                "learn the duties",
                "of a maestro."),
            new BookPageInfo(
                "Contribute two",
                "thousand gold when",
                "you take the stage.",
                "A further four",
                "hundred replaces",
                "any ring lost",
                "between taverns",
                "or travels."),
            new BookPageInfo(
                "The ring attunes",
                "your art: +10 to",
                "Discordance,",
                "Musicianship,",
                "Peacemaking and",
                "Provocation.",
                "Guard it from",
                "jealous hands."),
            new BookPageInfo(
                "Share new songs,",
                "lead community",
                "gatherings, and",
                "calm unrest",
                "when called.",
                "Mockery of the",
                "guild's good name",
                "bars you forever."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public BardsGuildGuide()
        {
        }

        public BardsGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class BlacksmithsGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Smiths Charter", "Hammerlord Branik",
            new BookPageInfo(
                "By anvil and oath",
                "the Blacksmiths",
                "Guild forges",
                "Sosaria's steel.",
                "This charter marks",
                "your responsibilities",
                "and the boon",
                "of membership."),
            new BookPageInfo(
                "Two thousand gold",
                "is due upon",
                "acceptance. A",
                "reserve of four",
                "hundred gold covers",
                "a new ring if",
                "the forge consumes",
                "the last."),
            new BookPageInfo(
                "The ring grants",
                "+20 skill to",
                "Blacksmithing",
                "and Arms Lore,",
                "steadied by years",
                "of tradition.",
                "Do not trade it",
                "for ore."),
            new BookPageInfo(
                "Share alloy",
                "secrets only with",
                "guild kin, keep",
                "shops safe, and",
                "answer militia",
                "contracts promptly.",
                "Neglecting orders",
                "brands you oathless."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public BlacksmithsGuildGuide()
        {
        }

        public BlacksmithsGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class NecromancersGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Black Magic Manual", "Magus Vorl",
            new BookPageInfo(
                "Tread the shadowed",
                "path with purpose.",
                "The Necromancers",
                "Guild binds dark",
                "arts beneath strict",
                "covenants. Read",
                "and obey each",
                "clause."),
            new BookPageInfo(
                "Two thousand gold",
                "opens our sanctum.",
                "Keep four hundred",
                "gold available",
                "to reclaim a ring",
                "should it be",
                "lost to ritual",
                "mishap."),
            new BookPageInfo(
                "The ring focuses",
                "forbidden study:",
                "+10 to Alchemy,",
                "Forensics,",
                "Necromancy and",
                "Spirit Speak.",
                "Do not let it",
                "touch holy flame."),
            new BookPageInfo(
                "Respect grave",
                "sites, silence",
                "secrets, and obey",
                "council edicts.",
                "Unleashed horrors",
                "bring hunters and",
                "banishment alike."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public NecromancersGuildGuide()
        {
        }

        public NecromancersGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class AlchemistsGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Alchemists Notes", "Master Quill",
            new BookPageInfo(
                "Distillation of",
                "knowledge defines",
                "the Alchemists",
                "Guild. These notes",
                "outline the compact",
                "all new members",
                "sign with the",
                "council."),
            new BookPageInfo(
                "Deliver two thousand",
                "gold for entry.",
                "Keep four hundred",
                "gold ready to",
                "replace a ring",
                "lost to fumes",
                "or volatile",
                "brews."),
            new BookPageInfo(
                "Our rings refine",
                "the senses: +15",
                "to Alchemy,",
                "Cooking and",
                "Taste ID, ensuring",
                "every draught",
                "is balanced.",
                "Guard it well."),
            new BookPageInfo(
                "Share formulas",
                "within the hall,",
                "label reagents,",
                "and cleanse labs",
                "after use. Rogue",
                "experiments bring",
                "disciplinary",
                "transmutation."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public AlchemistsGuildGuide()
        {
        }

        public AlchemistsGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class DruidsGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Druids Grove Lore", "Elder Sylra",
            new BookPageInfo(
                "Welcome to the",
                "Circle. The Druids",
                "Guild keeps the",
                "green paths",
                "of Sosaria whole.",
                "This lore records",
                "your vows and",
                "blessings."),
            new BookPageInfo(
                "Offer two thousand",
                "gold to join our",
                "groves. Hold four",
                "hundred more to",
                "rebind a ring",
                "if the wilds",
                "claim it from",
                "your hand."),
            new BookPageInfo(
                "The ring nurtures",
                "your craft: +5",
                "Alchemy and",
                "Cooking, +10 to",
                "Animal Lore,",
                "Taming, Herding",
                "and Veterinary",
                "arts."),
            new BookPageInfo(
                "Tend sacred sites,",
                "answer nature's",
                "omens, and never",
                "exploit beasts.",
                "Balance broken",
                "must swiftly",
                "be restored."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public DruidsGuildGuide()
        {
        }

        public DruidsGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class ArchersGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Archers Range Log", "Fletcher Nyra",
            new BookPageInfo(
                "Every arrow loosed",
                "in guild name",
                "flies true.",
                "These pages teach",
                "the duties of",
                "the Archers",
                "Guild and the",
                "ring's favor."),
            new BookPageInfo(
                "Provide two thousand",
                "gold when you",
                "enlist. Keep four",
                "hundred gold on",
                "hand to replace",
                "a ring lost to",
                "skirmish or",
                "target practice."),
            new BookPageInfo(
                "Wearing it grants",
                "+10 Archery,",
                "+20 Fletching",
                "and +10 Tactics",
                "skill. Never let",
                "an untested archer",
                "borrow the",
                "enchantment."),
            new BookPageInfo(
                "Share arrow",
                "patterns, defend",
                "the realm's walls,",
                "and honor range",
                "safety. Reckless",
                "shots earn",
                "swift expulsion."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public ArchersGuildGuide()
        {
        }

        public ArchersGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class CarpentersGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Carpenters Log", "Sawyer Branna",
            new BookPageInfo(
                "Timbercraft lives",
                "through the",
                "Carpenters Guild.",
                "This log explains",
                "our promises",
                "and the ring",
                "bestowed upon",
                "members."),
            new BookPageInfo(
                "Pay two thousand",
                "gold to the guild",
                "steward. Hold four",
                "hundred to replace",
                "any ring lost",
                "to sawdust or",
                "river barges.",
                ""),
            new BookPageInfo(
                "The ring grants",
                "+20 Carpentry",
                "and Lumberjacking",
                "skill, steadying",
                "hand and eye",
                "for every cut.",
                "Do not pawn it",
                "for timber."),
            new BookPageInfo(
                "Maintain sturdy",
                "housing, teach",
                "joinery tricks,",
                "and replant felled",
                "groves. Neglect",
                "invites guild",
                "reprisal."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public CarpentersGuildGuide()
        {
        }

        public CarpentersGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class CartographersGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Cartographers Atlas", "Surveyor Penn",
            new BookPageInfo(
                "Map the unknown",
                "for the",
                "Cartographers",
                "Guild. This atlas",
                "details our code",
                "and the enchanted",
                "ring bestowed",
                "upon you."),
            new BookPageInfo(
                "Present two thousand",
                "gold to join the",
                "chart hall. Keep",
                "four hundred gold",
                "to replace a ring",
                "lost to storm",
                "or spilled ink.",
                ""),
            new BookPageInfo(
                "Our ring grants",
                "+30 Cartography",
                "skill, steadying",
                "hand and compass",
                "alike. Store it",
                "in a dry case",
                "when not worn.",
                ""),
            new BookPageInfo(
                "Share accurate",
                "updates, mark",
                "hazards clearly,",
                "and honor the",
                "guild seal.",
                "False maps earn",
                "swift exile."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public CartographersGuildGuide()
        {
        }

        public CartographersGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class LibrariansGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Librarians Index", "Curator Miri",
            new BookPageInfo(
                "Guard every tome",
                "under the Librarians",
                "Guild banner.",
                "This index records",
                "our creed and",
                "the ring's",
                "scholarly aid.",
                ""),
            new BookPageInfo(
                "Submit two thousand",
                "gold for admittance.",
                "Retain four hundred",
                "gold to replace",
                "your ring if",
                "misplaced among",
                "stacks or lent",
                "in error."),
            new BookPageInfo(
                "The ring grants",
                "+20 Item ID",
                "and Inscription",
                "skill, letting",
                "you catalogue",
                "and scribe",
                "with mastery.",
                ""),
            new BookPageInfo(
                "Preserve texts,",
                "share indexes,",
                "and aid scholars",
                "seeking lore.",
                "Selling banned",
                "scrolls ends",
                "your tenure."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public LibrariansGuildGuide()
        {
        }

        public LibrariansGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class CulinariansGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Culinary Charter", "Chef Amand",
            new BookPageInfo(
                "Flavor and service",
                "define the",
                "Culinary Guild.",
                "This charter",
                "details the duties",
                "and delicious",
                "benefits awaiting",
                "members."),
            new BookPageInfo(
                "Provide two thousand",
                "gold at initiation.",
                "Keep four hundred",
                "gold for replacing",
                "a ring lost amid",
                "kitchens or",
                "feasts gone awry.",
                ""),
            new BookPageInfo(
                "The ring grants",
                "+20 Cooking",
                "and Taste ID",
                "skill, perfecting",
                "seasoning and",
                "presentation.",
                "Do not use it",
                "as a napkin."),
            new BookPageInfo(
                "Share recipes,",
                "source fresh",
                "produce, and aid",
                "guild feasts.",
                "Poisoning patrons",
                "brings permanent",
                "banishment."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public CulinariansGuildGuide()
        {
        }

        public CulinariansGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class AssassinsGuildGuide : BaseGuildGuideBook
    {
        public static readonly BookContent Content = new BookContent(
            "Assassins Edict", "Shade Ravel",
            new BookPageInfo(
                "Silence and",
                "precision define",
                "the Assassins",
                "Guild. These",
                "pages outline",
                "the contract",
                "binding every",
                "blade."),
            new BookPageInfo(
                "Two thousand gold",
                "secures the oath.",
                "Keep four hundred",
                "gold for ring",
                "replacement if",
                "missions demand",
                "discarding your",
                "token."),
            new BookPageInfo(
                "The ring sharpens",
                "deadly arts: +10",
                "Fencing, Hiding",
                "and Stealth,",
                "with +15 to",
                "Poisoning.",
                "Lose it and you",
                "lose our trust."),
            new BookPageInfo(
                "Honor contracts,",
                "strike with",
                "discretion, and",
                "never reveal",
                "clients. Failure",
                "means hunters",
                "will reclaim",
                "your life."),
            new BookPageInfo(
                "Guild vendors favor",
                "those bearing rings.",
                "Expect better coin",
                "for goods you sell",
                "and lighter prices",
                "on vital supplies.",
                "Show your guild ring",
                "to access private stock."),
            new BookPageInfo(
                "Should you wish to",
                "depart, speak with",
                "the guildmaster to",
                "resign honorably.",
                "They will reclaim",
                "your guild ring and",
                "record your change",
                "within the ledger."),
            new BookPageInfo(
                "Each new allegiance",
                "demands greater coin.",
                "The next guild asks",
                "no less than four",
                "thousand pieces of",
                "gold, and the toll",
                "doubles each time",
                "you trade your oath."));

        [Constructable]
        public AssassinsGuildGuide()
        {
        }

        public AssassinsGuildGuide(Serial serial)
            : base(serial)
        {
        }

        public override BookContent DefaultContent
        {
            get { return Content; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }
}
