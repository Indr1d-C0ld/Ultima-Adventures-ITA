using System;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
	public class PackHorseEquipmentDeed : Item
	{
		private const double RequiredTailoring = 0.0;
		private const double RequiredTaming = 30.0;

		[Constructable]
		public PackHorseEquipmentDeed() : base( 0x14F0 )
		{
			Weight = 1.0;
			Name = "a pack horse equipment deed";
		}

		public PackHorseEquipmentDeed( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
				return;
			}

			if ( from.Skills[SkillName.Tailoring].Base < RequiredTailoring || from.Skills[SkillName.AnimalTaming].Base < RequiredTaming )
			{
				from.SendMessage( "You lack the skill to use this deed." );
				return;
			}

			from.SendMessage( "Select the horse you wish to outfit." );
			from.Target = new PackHorseEquipmentTarget( this );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

		private class PackHorseEquipmentTarget : Target
		{
			private readonly PackHorseEquipmentDeed m_Deed;

			public PackHorseEquipmentTarget( PackHorseEquipmentDeed deed ) : base( 1, false, TargetFlags.None )
			{
				m_Deed = deed;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_Deed.Deleted )
					return;

				if ( !m_Deed.IsChildOf( from.Backpack ) )
				{
					from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
					return;
				}

				if ( targeted is PackHorse )
				{
					from.SendMessage( "That is already a pack horse." );
					return;
				}

				if ( !( targeted is Horse ) )
				{
					from.SendMessage( "That is not a horse." );
					return;
				}

				Horse horse = (Horse)targeted;

				if ( horse.ControlMaster != from || !horse.Controlled )
				{
					from.SendMessage( "That horse does not belong to you." );
					return;
				}

				if ( horse.Summoned )
				{
					from.SendMessage( "You cannot outfit a summoned horse." );
					return;
				}

				BaseMount mount = horse as BaseMount;
				if ( mount != null && mount.Rider != null )
				{
					from.SendMessage( "You must dismount the horse first." );
					return;
				}

				Map map = horse.Map;
				Point3D location = horse.Location;
				string horseName = horse.Name;
				int horseHue = horse.Hue;
				bool wasBonded = horse.IsBonded;

				horse.Delete();

				PackHorse packHorse = new PackHorse();
				packHorse.MoveToWorld( location, map );
				packHorse.ControlMaster = from;
				packHorse.Controlled = true;
				packHorse.ControlOrder = OrderType.Follow;
				packHorse.IsBonded = wasBonded;
				packHorse.Hue = horseHue;

				if ( !string.IsNullOrEmpty( horseName ) && horseName != "a horse" && horseName != "a greater horse" )
					packHorse.Name = horseName;
				else
					packHorse.Name = "a pack horse";

				from.SendMessage( "Your horse is now outfitted as a pack horse." );
				m_Deed.Delete();
			}
		}
	}
}
