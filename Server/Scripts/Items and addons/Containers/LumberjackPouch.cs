namespace Server.Items
{
	[Flipable(0x1C10, 0x1CC6)]
	public class LumberjackPouch : RucksackBase
	{
		public override string FailureMessage { get { return "This rucksack is for logs."; } }

		[Constructable]
		public LumberjackPouch() : base("lumberjacks rucksack")
		{
			Hue = 0x95;
		}

		public override bool CanAdd(Mobile from, Item item)
		{
			return base.CanAdd(from, item) && item is BaseLog;
		}

		public LumberjackPouch(Serial serial) : base(serial)
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