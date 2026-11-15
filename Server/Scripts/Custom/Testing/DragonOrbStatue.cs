using System;
using Server; 
using Server.Misc;
using Server.Multis;

namespace Server.Items
{
	[Flipable( 0x544D, 0x544E )]
	public class DragonOrbStatue : Item, IChopable
	{
		[Constructable]
		public DragonOrbStatue() : base( 0x544D )
		{
			Name = "Statue of " + NameList.RandomName( "dragon" );
			Light = LightType.Circle225;
			Weight = 100.0;
			Hue = Misc.RandomThings.GetRandomColor(0);
		}

		public override bool VerifyMove( Mobile from )
		{
			if ( !Movable && Parent == null && from != null && !from.Deleted )
			{
				BaseHouse house = BaseHouse.FindHouseAt( this );

				if ( house != null && house.IsCoOwner( from ) )
					from.SendMessage( "This statue is locked down. Use the house release command, Homeowner Tools, or an axe to pick it up." );
			}

			return base.VerifyMove( from );
		}

		public void OnChop( Mobile from )
		{
			if ( from == null || from.Deleted || !from.Alive )
				return;

			if ( Parent != null || Map == null || Map == Map.Internal || from.Map != Map || !from.InRange( GetWorldLocation(), 3 ) )
			{
				from.SendLocalizedMessage( 500446 ); // That is too far away.
				return;
			}

			BaseHouse house = BaseHouse.FindHouseAt( this );

			if ( house == null || !house.IsCoOwner( from ) )
				return;

			if ( house.IsLockedDown( this ) )
				house.SetLockdown( this, false );
			else if ( house.IsSecure( this ) )
				house.ReleaseSecure( from, this );

			Movable = true;
			from.AddToBackpack( this );
			from.SendMessage( "You pick up the dragon orb statue." );
		}

		public DragonOrbStatue( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
