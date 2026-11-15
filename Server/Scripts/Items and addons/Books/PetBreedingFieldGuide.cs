using System;
using Server;

namespace Server.Items
{
    public class PetBreedingFieldGuide : BaseBook
    {
        public static readonly BookContent Content = new BookContent
        (
            "Field Guide to Pet Breeding", "Adventurers' Guild",

            new BookPageInfo
            (
                "Welcome, tamer. This",
                "guide explains how the",
                "animal breeding service",
                "works in our lands so",
                "your stable can grow",
                "stronger generations",
                "of pets.",
                ""
            ),
            new BookPageInfo
            (
                "Where to begin:",
                "Seek out any NPC",
                "titled \"the animal",
                "breeder\" (they use",
                "the Jako breeder",
                "service). Right-click",
                "and choose \"Breed\"",
                "or say \"I wish to"
            ),
            new BookPageInfo
            (
                "breed animals\" while",
                "nearby. The breeder",
                "will only help if",
                "your Animal Taming",
                "skill is at least",
                "55.0.",
                "",
                ""
            ),
            new BookPageInfo
            (
                "Choosing parents:",
                "* Two tameable pets",
                "  of the SAME species",
                "  are required.",
                "* They must be",
                "  opposite sexes.",
                "* Levels must be",
                "  within five ranks"
            ),
            new BookPageInfo
            (
                "  of each other.",
                "* Each must be at",
                "  least mating level",
                "  10 and not on a",
                "  breeding cooldown.",
                "* Paragons, humanoids",
                "  and untamed beasts",
                "  are refused.",
                ""
            ),
            new BookPageInfo
            (
                "Starting a pairing:",
                "Use the breeder's",
                "contract gump to pick",
                "the female and male.",
                "If two different",
                "owners are involved,",
                "both must accept the",
                "proposal.",
                ""
            ),
            new BookPageInfo
            (
                "Costs and waiting:",
                "The fee is Level1 x",
                "Level2 x 50 gold",
                "(doubled when both",
                "pets belong to the",
                "same owner). The",
                "breeder keeps both",
                "parents until the",
                "cooldown ends",
                "(2-3 days for males"
            ),
            new BookPageInfo
            (
                "and 3-7 days for",
                "females depending on",
                "their tier).",
                "",
                "When the timer is",
                "done you receive a",
                "parent ticket. Give",
                "it back to the",
                "breeder to reclaim",
                "your pet." 
            ),
            new BookPageInfo
            (
                "The owner of the",
                "female also receives",
                "the baby deed if the",
                "attempt succeeds",
                "(twins are rare!).",
                "Both parents get a",
                "refund if breeding",
                "fails.",
                ""
            ),
            new BookPageInfo
            (
                "Passing on power:",
                "Offspring inherit",
                "stats from either",
                "parent, with a chance",
                "to keep the best",
                "strength, resists,",
                "and damage rolls.",
                "Special hues and",
                "levels can also rise",
                "from strong parents."
            ),
            new BookPageInfo
            (
                "Quick tips:",
                "* Check stable gumps",
                "  for breeding status",
                "  before pairing.",
                "* Keep tickets safe;",
                "  they are the ONLY",
                "  way to retrieve",
                "  parents and",
                "  newborns.",
                "* Both parents stay"
            ),
            new BookPageInfo
            (
                "  with the breeder",
                "  until you redeem",
                "  their tickets.",
                "* Train pets close in",
                "  level to avoid",
                "  rejection.",
                "",
                "Good luck raising",
                "the next legend!",
                ""
            )
        );

        public override BookContent DefaultContent { get { return Content; } }

        [Constructable]
        public PetBreedingFieldGuide() : base(0xFEF, false)
        {
            Hue = 1150;
        }

        public PetBreedingFieldGuide(Serial serial) : base(serial)
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
            int version = reader.ReadEncodedInt();
        }
    }
}
