namespace Server.Items
{
	[Flipable(0x1C10, 0x1CC6)]
	public class AlchemyPouch : RucksackBase
	{
		public override string FailureMessage { get { return "This rucksack is for reagents, empty jars, and empty bottles."; } }

		[Constructable]
		public AlchemyPouch() : base("alchemy rucksack")
		{
			Hue = 0x89F;
		}

		public override bool CanAdd(Mobile from, Item item)
		{
			return base.CanAdd(from, item)
				&& (
					Server.Misc.MaterialInfo.IsReagent(item)
					|| item is Bottle
					|| item is Jar
				);
		}

		public AlchemyPouch(Serial serial) : base(serial)
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