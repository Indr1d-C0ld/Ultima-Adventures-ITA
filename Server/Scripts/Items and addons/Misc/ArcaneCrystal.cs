using System;
using Server.Targeting;

namespace Server.Items
{
	public class ArcaneCrystal : Item
	{
		public override string DefaultName
		{
			get { return "arcane crystal"; }
		}

		[Constructable]
		public ArcaneCrystal() : base( 0x1F19 )
		{
			Stackable = true;
			Weight = 1.0;
			Hue = ArcaneGem.DefaultArcaneHue;
		}

		public ArcaneCrystal( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1060640 );
			}
			else
			{
				from.SendMessage( "What do you want to use the arcane crystal on?" );
				from.Target = new ArcaneCrystalTarget( this );
			}
		}

		private class ArcaneCrystalTarget : Target
		{
			private ArcaneCrystal m_Crystal;

			public ArcaneCrystalTarget( ArcaneCrystal crystal ) : base( 1, false, TargetFlags.None )
			{
				m_Crystal = crystal;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_Crystal == null || m_Crystal.Deleted )
					return;

				if ( !m_Crystal.IsChildOf( from.Backpack ) )
				{
					from.SendLocalizedMessage( 1060640 );
					return;
				}

				if ( targeted is GolemPorterItem )
				{
					((GolemPorterItem)targeted).RechargeWithArcaneCrystal( from, m_Crystal );
				}
				else
				{
					from.SendMessage( "You don't think that will really do anything." );
				}
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
