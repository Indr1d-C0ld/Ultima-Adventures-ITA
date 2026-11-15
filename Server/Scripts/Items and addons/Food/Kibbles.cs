using System;
using Server;
using Server.Mobiles;

namespace Server.Items
{
        public abstract class BaseKibble : Food
        {
		protected BaseKibble(int itemID, string name, int hue = 0)
			: base(1, itemID)
		{
			Name = name;
			Hue = hue;
			Stackable = true;
			Weight = 1.0;
			FillFactor = 20;
		}

                public BaseKibble(Serial serial)
                        : base(serial)
                {
                }

                public abstract FoodType KibbleType { get; }

                public override void Serialize(GenericWriter writer)
                {
                        base.Serialize(writer);

                        writer.Write((int)0); // version for legacy saves
                }

                public override void Deserialize(GenericReader reader)
                {
                        base.Deserialize(reader);

                        if (!reader.End())
                        {
                                reader.ReadInt(); // legacy version (discarded)
                        }
                }

                public override bool Eat(Mobile from)
                {
                        if (from != null && from.Player)
                                from.SendMessage("This kibble is meant for pets, not people.");

                        return false;
                }

                public virtual void OnFed(Mobile feeder, BaseCreature pet)
                {
                        if (pet != null)
                        {
				pet.Hunger = 20;
				pet.Loyalty = BaseCreature.MaxLoyalty;
				pet.InvalidateProperties();

				pet.PlaySound(Utility.Random(0x3A, 3));

				if (pet.Body.IsAnimal)
					pet.Animate(3, 5, 1, true, false, 0);
				else if (pet.Body.IsMonster)
					pet.Animate(17, 5, 1, true, false, 0);

				if (feeder != null)
					pet.SayTo(feeder, 502060); // Your pet looks happier.
			}

			if (feeder != null)
				feeder.SendMessage("Your pet is completely satisfied.");

			if (Amount > 1)
				Amount--;
			else
				Delete();
		}

        }

        public class MeatKibble : BaseKibble
        {
		[Constructable]
		public MeatKibble() : base(0x097B, "meat kibble", 0x66D)
		{
		}

		public MeatKibble(Serial serial) : base(serial)
		{
		}

                public override FoodType KibbleType
                {
                        get { return FoodType.Meat; }
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

        public class FishKibble : BaseKibble
        {
		[Constructable]
		public FishKibble() : base(0x09CC, "fish kibble", 0x5A9)
		{
		}

		public FishKibble(Serial serial) : base(serial)
		{
		}

                public override FoodType KibbleType
                {
                        get { return FoodType.Fish; }
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

        public class FruitsAndVegiesKibble : BaseKibble
        {
		[Constructable]
		public FruitsAndVegiesKibble() : base(0x09D0, "fruit and veggie kibble", 0x58B)
		{
		}

		public FruitsAndVegiesKibble(Serial serial) : base(serial)
		{
		}

                public override FoodType KibbleType
                {
                        get { return FoodType.FruitsAndVegies; }
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

        public class GrainsAndHayKibble : BaseKibble
        {
		[Constructable]
		public GrainsAndHayKibble() : base(0x103B, "grain and hay kibble", 0x482)
		{
		}

		public GrainsAndHayKibble(Serial serial) : base(serial)
		{
		}

                public override FoodType KibbleType
                {
                        get { return FoodType.GrainsAndHay; }
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

        public class EggKibble : BaseKibble
        {
		[Constructable]
		public EggKibble() : base(0x09B5, "egg kibble", 0x47E)
		{
		}

		public EggKibble(Serial serial) : base(serial)
		{
		}

                public override FoodType KibbleType
                {
                        get { return FoodType.Eggs; }
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

        public class GoldKibble : BaseKibble
        {
		[Constructable]
		public GoldKibble() : base(0x0EED, "golden kibble", 0x8A5)
		{
		}

		public GoldKibble(Serial serial) : base(serial)
		{
		}

                public override FoodType KibbleType
                {
                        get { return FoodType.Gold; }
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

        public class FireKibble : BaseKibble
        {
		[Constructable]
		public FireKibble() : base(0x0F8C, "ember kibble", 0x489)
		{
		}

		public FireKibble(Serial serial) : base(serial)
		{
		}

                public override FoodType KibbleType
                {
                        get { return FoodType.Fire; }
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

        public class GemKibble : BaseKibble
        {
		[Constructable]
		public GemKibble() : base(0x0F16, "gemmed kibble", 0x8A7)
		{
		}

		public GemKibble(Serial serial) : base(serial)
		{
		}

                public override FoodType KibbleType
                {
                        get { return FoodType.Gems; }
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

	public class NoxKibble : BaseKibble
	{
		[Constructable]
		public NoxKibble() : base(0x0F88, "noxious kibble", 0x455)
		{
		}

		public NoxKibble(Serial serial) : base(serial)
		{
		}

                public override FoodType KibbleType
                {
                        get { return FoodType.Nox; }
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

        public class SeaKibble : BaseKibble
        {
		[Constructable]
		public SeaKibble() : base(0x0DD6, "sea-salted kibble", 0x5E9)
		{
		}

		public SeaKibble(Serial serial) : base(serial)
		{
		}

                public override FoodType KibbleType
                {
                        get { return FoodType.Sea; }
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

        public class MoonKibble : BaseKibble
        {
		[Constructable]
		public MoonKibble() : base(0x0F8B, "moonlit kibble", 0x482)
		{
		}

		public MoonKibble(Serial serial) : base(serial)
		{
		}

                public override FoodType KibbleType
                {
                        get { return FoodType.Moon; }
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
