using Server;
using System;
using Server.Items;
using Server.Multis;
using Server.Targeting;
using Server.Mobiles;
using Server.AllHues;

namespace Server.Items
{
	public class AllDyeTubsWeapon : DyeTub
	{
		private int i_charges;
		private int TheHue = 0;
		private int m_DyedHue;
		private bool m_Redyable = false;
		private bool m_Charged = false;
		private bool m_AllowPack = true;

		[CommandProperty( AccessLevel.GameMaster )]
		public bool AllowPack
		{
			get	{return m_AllowPack;}
			set	{m_AllowPack = value;}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Charged
		{
			get	{return m_Charged;}
			set	{m_Charged = value;}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Charges
		{
			get { return i_charges; }
			set { i_charges = value; InvalidateProperties(); }
		}

		[Constructable]
		public AllDyeTubsWeapon(  )
		{
			Name = "Weapon Dye Tub";
			Weight = 5.0;
			Hue = TheHue;
			DyedHue = TheHue;
			Charges = 100;
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
			list.Add( "Price Per Item Dyed: 500 Gold" );

			if ( Charged )
			{
				list.Add( 1060658, "Uses Remaining \t{0}", this.Charges );
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( this.IsChildOf( from.Backpack ) )
			{
				DoPack( from );
			}
			else
			{
				DoOut( from );
			}
		}

		public void DoPack( Mobile from )
		{
			if( AllowPack )
			{
				DoOut( from );
			}
			else
			{
				from.SendMessage("The dye tub cannot be in your pack.");
			}
		}

		public void DoOut ( Mobile from )
		{
			if ( from.InRange( this.GetWorldLocation(), 1 ) )
			{
				from.SendMessage( "Select the item to dye" );
				from.Target = new AllDyeTubsWeaponTarget( this );
			}
			else
			{
				from.SendLocalizedMessage( 500446 ); // That is too far away.
			}
		}

		public AllDyeTubsWeapon( Serial serial ) : base( serial ){}

		public override bool CanBeginPreview( Mobile from, out string failureMessage )
		{
			if ( !base.CanBeginPreview( from, out failureMessage ) )
				return false;

			if ( Charged && Charges <= 0 )
			{
				failureMessage = "That dye tub has no charges remaining.";
				return false;
			}

			if ( IsChildOf( from.Backpack ) && !AllowPack )
			{
				failureMessage = "The dye tub cannot be in your pack.";
				return false;
			}

			return true;
		}

		public override bool CanPreviewTarget( Mobile from, Item target, int selectedHue, out string failureMessage )
		{
			failureMessage = null;

			if ( !( target is BaseWeapon ) )
			{
				failureMessage = "That item cannot be dyed.";
				return false;
			}

			if ( !DyePreviewUtility.IsOwnedGear( from, target ) )
			{
				failureMessage = "The item must be equipped or in your pack.";
				return false;
			}

			if ( !from.InRange( GetWorldLocation(), 1 ) )
			{
				failureMessage = "That is too far away.";
				return false;
			}

			return true;
		}

		public override bool CanApply( Mobile from, Item target, int selectedHue, out string failureMessage )
		{
			if ( !CanBeginPreview( from, out failureMessage )
				|| !CanPreviewTarget( from, target, selectedHue, out failureMessage ) )
				return false;

			if ( from.Backpack == null || from.Backpack.GetAmount( typeof( Gold ) ) < 500 )
			{
				failureMessage = "You need 500 gold in your pack.";
				return false;
			}

			return true;
		}

		public override bool ApplyPermanentDye( Mobile from, Item target, int selectedHue )
		{
			string failureMessage;

			if ( !CanApply( from, target, selectedHue, out failureMessage )
				|| !from.Backpack.ConsumeTotal( typeof( Gold ), 500 ) )
			{
				if ( from != null && !String.IsNullOrEmpty( failureMessage ) )
					from.SendMessage( failureMessage );

				return false;
			}

			target.Hue = selectedHue;

			if ( Charged )
			{
				if ( Charges <= 1 )
					Delete();
				else
					Charges = Charges - 1;
			}

			from.PlaySound( 0x23F );
			return true;
		}

		public override bool IsSourceStillValid( Mobile from )
		{
			string failureMessage;
			return CanBeginPreview( from, out failureMessage );
		}

		public override string GetConsumptionMessage()
		{
			return Charged
				? "Applying costs 500 gold and uses one tub charge."
				: "Applying costs 500 gold.";
		}

		public override void BeginTargeting( Mobile from )
		{
			DoOut( from );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version

			writer.Write( (int) m_DyedHue);
			writer.Write( (int) i_charges );

			writer.Write( (bool) m_Redyable );
			writer.Write( (bool) m_Charged );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			m_DyedHue = reader.ReadInt();
			i_charges = reader.ReadInt();

			m_Redyable = reader.ReadBool();
			m_Charged = reader.ReadBool();
		}

		public class AllDyeTubsWeaponTarget : Target
		{
			private AllDyeTubsWeapon m_Tub;

			public AllDyeTubsWeaponTarget( AllDyeTubsWeapon dyetub ) : base( 12, false, TargetFlags.None )
			{
				m_Tub = dyetub;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is Item )
				{
					Item item = (Item) targeted;

					if ( item is BaseWeapon )
					{
						DyePreviewSession.Begin( from, m_Tub, item );
					}
					else
					{
						from.SendMessage("That item cannot be dyed.");
					}
				}
				else
				{
					from.SendMessage("You cannot dye that.");
				}
			}
		}
	}
}
