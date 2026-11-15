using System;
using Server.Mobiles;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
	[CorpseName( "a human corpse" )]
	public class PortalNpcBoss : BaseCreature
	{
		[Constructable]
		public PortalNpcBoss() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
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

			SetStr( 90, 55 );
			SetDex( 90, 65 );
			SetInt( 90, 65 );

			SetHits( 60, 100 );
			SetMana( 40 );

			SetDamage( 12, 18 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 40, 50 );

			SetSkill( SkillName.Fencing, 70.0, 80.5 );
			SetSkill( SkillName.Macing, 70.0, 80.5 );
			SetSkill( SkillName.MagicResist, 70.0, 80.5 );
			SetSkill( SkillName.Swords, 70.0, 80.5 );
			SetSkill( SkillName.Tactics, 70.0, 80.5 );
			SetSkill( SkillName.Wrestling, 70.0, 80.5 );

			Fame = 0;
			Karma = 0;

			VirtualArmor = 40;

			Tamable = true;
			ControlSlots = 2;
			MinTameSkill = 100;
			
			double roll = Utility.RandomDouble(); // Generates a random number between 0.0 and 1.0

			if ( roll < 0.05 ) // 5% (Low) chance to drop a special book
			{
				switch ( Utility.Random( 4 ) )
				{
					case 0: PackItem( new BookOfChivalry() ); break;
					case 1: PackItem( new NecromancerSpellbook() ); break;
					case 2: PackItem( new BookOfNinjitsu() ); break;
					case 3: PackItem( new BookOfBushido() ); break;
				}
			}
			else if ( roll < 0.45 ) // 30% (Medium) chance to drop a spellbook or weapon
			{
				PackItem( new Spellbook() );
			}
			// The remaining 60% chance (from 0.40 to 1.0) will result in dropping nothing.


			switch ( Utility.Random( 3 ) )
			{
				case 1: AddItem( new Dagger() ); break;
				case 2: AddItem( new Cleaver() ); break;
				case 3: AddItem( new Club() ); break;
			}

			AddItem( new Shirt( RandomThings.GetRandomColor(0) ));
			AddItem( new Boots( Utility.RandomNeutralHue() ) );
			AddItem( new ShortPants( RandomThings.GetRandomColor(0) ) );
			Utility.AssignRandomHair( this );
		}

		public override bool AlwaysAttackable{ get{ return true; } }

		public PortalNpcBoss( Serial serial ) : base( serial )
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