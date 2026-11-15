using System;
using Server;

namespace Server.Items
{
	public abstract class BaseReagent : Item
	{
		public override double DefaultWeight
		{
			get { return 0.1; }
		}

		public BaseReagent( int itemID ) : this( itemID, 1 )
		{
		}

		public BaseReagent( int itemID, int amount ) : base( itemID )
		{
			Stackable = true;
			Amount = amount;
		}

		public BaseReagent( Serial serial ) : base( serial )
		{
		}

		public override void OnAdded( IEntity parent )
		{
			base.OnAdded( parent );

			if ( parent is ReagentBelt )
				return;

			Mobile from = RootParent as Mobile;

			if ( from == null || from.Backpack == null )
				return;

			Container container = parent as Container;

			if ( container == null || !container.IsChildOf( from.Backpack ) )
				return;

			ReagentBelt belt = ReagentBelt.FindReagentBelt( from );

			if ( belt == null )
				return;

			belt.TryAutoLoot( from, this );
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
