using System;
using Server;
using System.Collections;
using System.Collections.Generic;
using Server.Misc;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    public class DwarvenForge : Item, IChopable
	{
        [Constructable]
        public DwarvenForge() : base( 0x5442 )
		{
            Name = "dwarven forge";
			Weight = 100.0;
			Light = LightType.Empty;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
				from.SendMessage( "You can only ignite this if it is secure in a home." );
			else if ( !from.InRange( GetWorldLocation(), 2 ) )
				from.SendMessage( "You will need to get closer to ignite that." );
			else if ( Movable )
				from.SendMessage( "You can only ignite this if it is secure in a home." );
			else
			{
				if ( ItemID == 0x5442 ){ 	ItemID = 0x5443; Light = LightType.Circle225; 	from.SendSound( 0x208 ); }
				else { 						ItemID = 0x5442; Light = LightType.Empty; 		from.SendSound( 0x208 ); }
			}
		}

		public override bool OnDragLift( Mobile from )
		{
			if ( ItemID != 0x5442 ){ 		ItemID = 0x5442; Light = LightType.Empty; 		from.SendSound( 0x208 ); }

			return true;
		}

		public override bool OnDroppedToWorld( Mobile from, Point3D p )
		{
			if ( !ForgeAndSawmillHelper.CheckWorldDrop( from, this, p ) )
				return false;

			return base.OnDroppedToWorld( from, p );
		}

		public void OnChop( Mobile from )
		{
			if ( !from.InRange( GetWorldLocation(), 3 ) )
			{
				from.SendLocalizedMessage( 500446 ); // That is too far away.
				return;
			}

			BaseHouse house = BaseHouse.FindHouseAt( this );

			if ( house == null || !house.IsCoOwner( from ) )
				return;

			if ( ItemID != 0x5442 )
			{
				ItemID = 0x5442;
				Light = LightType.Empty;
				from.SendSound( 0x208 );
			}

			if ( house.IsLockedDown( this ) )
				house.SetLockdown( this, false );

			Movable = true;
			from.AddToBackpack( this );
			from.SendMessage( "You pick up the dwarven forge." );
		}

        public DwarvenForge( Serial serial ) : base( serial )
		{
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
    }
}
