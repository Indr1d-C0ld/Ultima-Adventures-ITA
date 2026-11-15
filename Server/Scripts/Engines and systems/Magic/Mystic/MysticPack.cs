using System;

namespace Server.Items
{
	[Flipable(0x1C10, 0x1CC6)]
	public class MysticPack : RucksackBase
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner { get; private set; }

		public override string FailureMessage { get { return "You cannot seem to open the rucksack."; } }

		[Constructable]
		public MysticPack() : base("monk's rucksack")
		{
			MaxItems = 20;
			Hue = 2422;
		}

		public void SetOwner(Mobile from)
		{
			if (Owner != null) return;

			Owner = from;
			var wrestling = Math.Max(0, Owner.Skills[SkillName.Wrestling].Base - 100);
			MaxItems = 20
				+ (int)wrestling // +1 slots per 1 skill 
				+ 5 * (int)(wrestling / 25); // +5 slots at 125
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			if (Owner != null) { list.Add(1070722, "Belongs to " + Owner.Name + ""); }
		}

		public override bool CanAdd(Mobile from, Item item)
		{
			return base.CanAdd(from, item)
				&& Owner == from
				&& from.Skills[SkillName.Wrestling].Value >= 100
				&& Server.Misc.GetPlayerInfo.isMonk(from)
				&& (item is MysticSpellbook || item is MysticMonkRobe || item is MonkRobe) == false;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (Owner == null && Server.Misc.GetPlayerInfo.isMonk(from))
				SetOwner(from);

			if (CanAdd(from, null))
			{
				Open(from);
				return;
			}

			from.SendMessage(FailureMessage);
		}

		public MysticPack(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)1); // version
			writer.Write((Mobile)Owner);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
			Owner = reader.ReadMobile();

			if (version == 0)
				MaxItems = 50; // Legacy bags are set to the max amount
		}
	}
}