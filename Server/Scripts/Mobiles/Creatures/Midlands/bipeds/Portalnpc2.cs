using System;
using Server.Mobiles;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
	[CorpseName( "a human corpse" )]
	public class PortalNpc2 : BaseCreature
	{
		[Constructable]
		public PortalNpc2() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Hue = Server.Misc.RandomThings.GetRandomSkinColor();

			if ( this.Female = Utility.RandomBool() )
			{
				Body = 0x191;
				Name = NameList.RandomName( "female" );
			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName( "male" );
			}

			SetStr( 50, 55 );
			SetDex( 60, 65 );
			SetInt( 60, 65 );

			SetHits( 30, 50 );
			SetMana( 20 );

			SetDamage( 8, 14 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 20, 30 );

			SetSkill( SkillName.Fencing, 50.0, 60.5 );
			SetSkill( SkillName.Macing, 50.0, 60.5 );
			SetSkill( SkillName.MagicResist, 50.0, 60.5 );
			SetSkill( SkillName.Swords, 50.0, 60.5 );
			SetSkill( SkillName.Tactics, 50.0, 60.5 );
			SetSkill( SkillName.Wrestling, 50.0, 60.5 );

			Fame = 0;
			Karma = 0;

			VirtualArmor = 22;

			Tamable = true;
			ControlSlots = 2;
			MinTameSkill = 30;
			
			switch ( Utility.Random( 7 ))
			{
				case 0: PackItem( Loot.RandomPotion() ); break;
				case 1: PackItem( Loot.RandomScroll( 0, 15, SpellbookType.Regular ) ); break;
				case 2: PackItem( Loot.RandomPotion() );  break;
			}

			switch ( Utility.Random( 4 ))
			{
				case 0: AddItem( new Shirt( RandomThings.GetRandomColor(0) )); break;
				case 1: AddItem( new Boots( Utility.RandomNeutralHue() ) ); break;
				case 2: AddItem( new ShortPants( RandomThings.GetRandomColor(0) ) ); break;
			}
			
			Utility.AssignRandomHair( this );
		}

		public override bool AlwaysAttackable{ get{ return true; } }

		public PortalNpc2( Serial serial ) : base( serial )
                      {
                      }
					  
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}