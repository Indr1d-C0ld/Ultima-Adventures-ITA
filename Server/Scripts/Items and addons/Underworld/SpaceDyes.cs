using Server;
using System;
using System.Collections;
using Server.Network;
using Server.Targeting;
using Server.Prompts;
using Server.Misc;

namespace Server.Items
{
	public class SpaceDyes : Item, IDyePreviewSource
	{
		[Constructable]
		public SpaceDyes() : this( 1 )
		{
		}

		[Constructable]
		public SpaceDyes( int amount ) : base( 0x1FDD )
		{
			Weight = 0.01;
			Stackable = true;
			Amount = amount;
			Name = "space-age dye"; 
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );
			list.Add( 1070722, "Used To Dye Almost Anything" );
		}

		public override void OnDoubleClick( Mobile from )
		{
			BeginTargeting( from );
		}

		public Item SourceItem { get { return this; } }
		public int PreviewHue { get { return Hue; } }

		public bool CanBeginPreview( Mobile from, out string failureMessage )
		{
			failureMessage = null;

			if ( Deleted || Amount < 1 || from == null || from.Backpack == null || !IsChildOf( from.Backpack ) )
			{
				failureMessage = "The dye must be in your backpack to use it.";
				return false;
			}

			return true;
		}

		public bool CanPreviewTarget( Mobile from, Item target, int selectedHue, out string failureMessage )
		{
			failureMessage = null;

			if ( !DyePreviewUtility.IsOwnedGear( from, target ) )
			{
				failureMessage = "You may only dye equipment you are wearing or carrying in your backpack.";
				return false;
			}

			if ( target.Stackable || target.ItemID == 8702 || target.ItemID == 4011 )
			{
				failureMessage = "You cannot dye that.";
				return false;
			}

			return true;
		}

		public bool CanApply( Mobile from, Item target, int selectedHue, out string failureMessage )
		{
			return CanBeginPreview( from, out failureMessage )
				&& CanPreviewTarget( from, target, selectedHue, out failureMessage );
		}

		public bool ApplyPermanentDye( Mobile from, Item target, int selectedHue )
		{
			string failureMessage;

			if ( !CanApply( from, target, selectedHue, out failureMessage ) )
			{
				if ( from != null && !String.IsNullOrEmpty( failureMessage ) )
					from.SendMessage( failureMessage );

				return false;
			}

			target.Hue = selectedHue;
			from.RevealingAction();
			from.PlaySound( 0x23E );
			from.AddToBackpack( new Bottle() );
			Consume();
			return true;
		}

		public bool IsSourceStillValid( Mobile from )
		{
			string failureMessage;
			return CanBeginPreview( from, out failureMessage );
		}

		public string GetSourceDisplayName()
		{
			return !String.IsNullOrEmpty( Name ) ? Name : "space-age dye";
		}

		public string GetConsumptionMessage()
		{
			return "Applying this dye will consume one dye.";
		}

		public void BeginTargeting( Mobile from )
		{
			string failureMessage;

			if ( !CanBeginPreview( from, out failureMessage ) )
			{
				if ( from != null )
					from.SendLocalizedMessage( 1060640 );

				return;
			}

			from.SendMessage( "What do you want to use the dye on?" );
			from.Target = new DyeTarget( this );
		}

		private class DyeTarget : Target
		{
			private SpaceDyes m_Dye;

			public DyeTarget( SpaceDyes tube ) : base( 1, false, TargetFlags.None )
			{
				m_Dye = tube;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is Item )
				{
					Item iDye = targeted as Item;

					if ( DyePreviewUtility.IsWearableGear( iDye ) )
					{
						DyePreviewSession.Begin( from, m_Dye, iDye );
					}
					else if ( !iDye.IsChildOf( from.Backpack ) )
					{
						from.SendMessage( "You can only dye things in your pack." );
					}
					else if ( ( iDye.Stackable == true ) || ( iDye.ItemID == 8702 ) || ( iDye.ItemID == 4011 ) )
					{
						from.SendMessage( "You cannot dye that." );
					}
					else if ( iDye.IsChildOf( from.Backpack ) )
					{
                        if (targeted is MagicPigment)
                        {
                            MagicPigment pigment = (MagicPigment)targeted;
                            if (pigment.ApplyHue(from, m_Dye.Hue, 0x23E))
                            {
                                from.AddToBackpack(new Bottle());
                                m_Dye.Consume();
                            }
                        }
						else if ( targeted is PaintPaletteBase )
						{
							PaintPaletteBase pigment = (PaintPaletteBase)targeted;
							if (pigment.ApplyHue(from, m_Dye.Hue, 0x23E))
							{
								from.AddToBackpack( new Bottle() );
								m_Dye.Consume();
							}
						}
                        else
                        {
                            iDye.Hue = m_Dye.Hue;
                            from.RevealingAction();
                            from.PlaySound(0x23E);
                            from.AddToBackpack(new Bottle());
                            m_Dye.Consume();
                        }
					}
					else
					{
						from.SendMessage( "You cannot dye that with this." );
					}
				}
				else
				{
					from.SendMessage( "You cannot dye that with this." );
				}
			}
		}

		public SpaceDyes( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( ( int) 1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
            if (version == 0)
			{
				reader.ReadInt(); // Vial hue
			}
		}
	}
}
