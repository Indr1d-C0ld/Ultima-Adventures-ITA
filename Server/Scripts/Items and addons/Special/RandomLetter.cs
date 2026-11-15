using System;
using Server.Network;
using Server.Prompts;
using Server.Items;
using Server.Targeting;

namespace Server.Items
{
 
    public class RandomLetter : Item // Create the item class which is derived from the base item class
    {
        public const int MinValue = 21951;
        public const int MaxValue = 21976;
        
        public override string DefaultName
        {
            get { return "a Forged Letter"; }
        }


        [Constructable]
        public RandomLetter()
            : base( MinValue )
        {
			ItemID = Utility.RandomMinMax( MinValue, MaxValue );
            Weight = 5.0;
			Hue = 1153;
        }

        public RandomLetter(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            if (version == 1)
            {
			    int lettertype = reader.ReadInt();
            }
        }

        public override bool DisplayLootType { get { return false; } }

    }
}