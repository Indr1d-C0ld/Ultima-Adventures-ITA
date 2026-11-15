namespace Server.Items
{
	[Flipable(0x2c7e, 0x1e3f)]
	public class CoinPouch : RucksackBase
	{
		public override string FailureMessage { get { return "This rucksack is for coins, gemstones, and gold nuggets."; } }

		[Constructable]
		public CoinPouch() : base("coin pouch")
		{
			Hue = 0x31;
		}

		public override bool CanAdd(Mobile from, Item item)
		{
			return base.CanAdd(from, item)
				&& (
					item is Gold
					|| item is DDCopper
					|| item is DDSilver
					|| item is DDJewels
					|| item is DDXormite
					|| item is DDGemstones
					|| item is DDGoldNuggets
				);
		}

		public CoinPouch(Serial serial) : base(serial)
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
			if (version == 0) { MaxItems = 20; }
			if (version < 2) { MaxItems = 50; }
		}
	}
}