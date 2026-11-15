using System;
using System.Globalization;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
    public abstract class BaseCurrencyBar : Item
    {
        private int m_Worth;

        protected BaseCurrencyBar(int itemID, int worth, double weight) : base(itemID)
        {
            m_Worth = worth;
            Weight = weight;
            Stackable = false;
            Name = DefaultName;
        }

        public BaseCurrencyBar(Serial serial) : base(serial)
        {
        }

        protected abstract string DefaultName { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Worth
        {
            get
            {
                return m_Worth;
            }
            set
            {
                m_Worth = value;
                InvalidateProperties();
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            string worth = Worth.ToString("N0", CultureInfo.GetCultureInfo("en-US"));
            list.Add(1060738, worth); // value: ~1_val~
        }

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);

            LabelTo(from, string.Format("value: {0:N0}", Worth));
        }

        public override void OnDoubleClick(Mobile from)
        {
            BankBox box = from != null ? from.FindBankNoCreate() : null;

            if (box == null || !IsChildOf(box))
            {
                if (from != null)
                    from.SendLocalizedMessage(1047026); // That must be in your bank box to use it.
                return;
            }

            int remaining = Worth;
            int deposited = 0;

            Delete();

            while (remaining > 60000)
            {
                Gold gold = new Gold(60000);

                if (box.TryDropItem(from, gold, false))
                {
                    remaining -= 60000;
                    deposited += 60000;
                }
                else
                {
                    gold.Delete();
                    if (from != null)
                        from.AddToBackpack(new BankCheck(remaining));
                    remaining = 0;
                    break;
                }
            }

            if (remaining > 0)
            {
                Gold gold = new Gold(remaining);

                if (box.TryDropItem(from, gold, false))
                {
                    deposited += remaining;
                }
                else
                {
                    gold.Delete();
                    if (from != null)
                        from.AddToBackpack(new BankCheck(remaining));
                }
            }

            if (from != null)
            {
                from.SendLocalizedMessage(1042672, true, " " + deposited.ToString()); // Gold was deposited in your account:
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
            writer.Write(m_Worth);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            m_Worth = reader.ReadInt();

            if (Name == null)
                Name = DefaultName;
        }
    }

    public class GoldBar : BaseCurrencyBar
    {
        [Constructable]
        public GoldBar() : base(0x1BEC, 60000, 25.0)
        {
        }

        public GoldBar(Serial serial) : base(serial)
        {
        }

        protected override string DefaultName
        {
            get { return "gold bar"; }
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

    public class GoldBarStack3 : BaseCurrencyBar
    {
        [Constructable]
        public GoldBarStack3() : base(0x1BED, 60000 * 3, 75.0)
        {
        }

        public GoldBarStack3(Serial serial) : base(serial)
        {
        }

        protected override string DefaultName
        {
            get { return "stack of 3 gold bars"; }
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

    public class GoldBarStack5 : BaseCurrencyBar
    {
        [Constructable]
        public GoldBarStack5() : base(0x1BEE, 60000 * 5, 125.0)
        {
        }

        public GoldBarStack5(Serial serial) : base(serial)
        {
        }

        protected override string DefaultName
        {
            get { return "stack of 5 gold bars"; }
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

    public class SilverBar : BaseCurrencyBar
    {
        [Constructable]
        public SilverBar() : base(0x1BF8, 60000, 25.0)
        {
        }

        public SilverBar(Serial serial) : base(serial)
        {
        }

        protected override string DefaultName
        {
            get { return "silver bar"; }
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

    public class SilverBarStack3 : BaseCurrencyBar
    {
        [Constructable]
        public SilverBarStack3() : base(0x1BF9, 60000 * 3, 75.0)
        {
        }

        public SilverBarStack3(Serial serial) : base(serial)
        {
        }

        protected override string DefaultName
        {
            get { return "stack of 3 silver bars"; }
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

    public class SilverBarStack5 : BaseCurrencyBar
    {
        [Constructable]
        public SilverBarStack5() : base(0x1BFA, 60000 * 5, 125.0)
        {
        }

        public SilverBarStack5(Serial serial) : base(serial)
        {
        }

        protected override string DefaultName
        {
            get { return "stack of 5 silver bars"; }
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

    public class CopperBar : BaseCurrencyBar
    {
        [Constructable]
        public CopperBar() : base(0x1BE6, 60000, 25.0)
        {
        }

        public CopperBar(Serial serial) : base(serial)
        {
        }

        protected override string DefaultName
        {
            get { return "copper bar"; }
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

    public class CopperBarStack3 : BaseCurrencyBar
    {
        [Constructable]
        public CopperBarStack3() : base(0x1BE7, 60000 * 3, 75.0)
        {
        }

        public CopperBarStack3(Serial serial) : base(serial)
        {
        }

        protected override string DefaultName
        {
            get { return "stack of 3 copper bars"; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            ItemID = 0x1BE7;
        }
    }

    public class CopperBarStack5 : BaseCurrencyBar
    {
        [Constructable]
        public CopperBarStack5() : base(0x1BE8, 60000 * 5, 125.0)
        {
        }

        public CopperBarStack5(Serial serial) : base(serial)
        {
        }

        protected override string DefaultName
        {
            get { return "stack of 5 copper bars"; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            ItemID = 0x1BE8;
        }
    }
}
