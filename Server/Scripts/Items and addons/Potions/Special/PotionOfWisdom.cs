using Server;
using System;
using System.Collections;
using Server.Network;
using Server.Targeting;
using Server.Prompts;
using Server.Misc;


namespace Server.Items
{
	public class PotionOfWisdom : Item
	{
		[Constructable]
		public PotionOfWisdom() : this( 1 )
		{
		}


		[Constructable]
		public PotionOfWisdom( int amount ) : base( 0x2827 )
		{
			Weight = 0.01;
			Stackable = true;
			Amount = amount;
			Hue = 0xB9E;
			Name = "potion of wisdom";
		}


		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1060640 ); // The item must be in your backpack to use it.
			}
			else if ( from.StatCap > from.RawStatTotal )
			{
				int up = GetGainAmount( from, true );

				from.RawInt = from.RawInt + up;
				from.SendMessage( "This potions makes you feel smarter!" );
				Server.Items.BasePotion.PlayDrinkEffect( from );
				this.Consume();
			}
			else if ( from.RawStatTotal == from.StatCap && ApplyCappedStatGain( from, GetGainAmount( from, false ) ) > 0 )
			{
				from.SendMessage( "This potions makes you feel smarter!" );
				Server.Items.BasePotion.PlayDrinkEffect( from );
				this.Consume();
			}
			else
			{
				from.SendMessage( "This potion would have no effect on you." );
			}
		}

		private static int GetGainAmount( Mobile from, bool limitToAvailablePoints )
		{
			int up = 1;
			int chance = (int)( Utility.RandomMinMax( 1, 100 ) + ( Server.Items.BasePotion.EnhancePotions( from ) / 2 ) );

			if ( chance >= 98 ){ up = 5; }
			else if ( chance >= 87 ){ up = 4; }
			else if ( chance >= 75 ){ up = 3; }
			else if ( chance >= 50 ){ up = 2; }

			if ( limitToAvailablePoints )
				up = AvailPoints( from, up );

			return up;
		}

		internal static int ApplyCappedStatGain( Mobile from, int amount )
		{
			return SkillCheck.ApplyCappedStatGain( from, SkillCheck.Stat.Int, amount );
		}


		public static int AvailPoints( Mobile from, int val )
		{
			int points = from.StatCap - ( from.RawStr + from.RawInt + from.RawDex );


			if ( val > points ){ val = points; }


			return val;
		}


		public PotionOfWisdom( Serial serial ) : base( serial )
		{
		}


		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( ( int) 0 ); // version
		}


		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
