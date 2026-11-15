namespace Server.Items
{
    public class TrappedPouch : Pouch
    {
        private int TrappedHue = 33; // 0x21 Red

        [Constructable]
        public TrappedPouch() : base()
        {
            Weight = 1.0;
            Hue = TrappedHue;
            TrapLevel = 100;
            TrapPower = 0;
            TrapType = TrapType.DartTrap;
            Name = "trapped pouch";
        }

        public TrappedPouch(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override bool ExecuteTrap(Mobile from)
        {
            bool result = base.ExecuteTrap(from);
            if (result)
            {
                Name = "pouch";
                if (Hue == TrappedHue)
                    Hue = 0;
            }

            return result;
        }
    }
}