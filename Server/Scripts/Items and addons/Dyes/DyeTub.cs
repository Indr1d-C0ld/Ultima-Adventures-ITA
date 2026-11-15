using System;
using System.Collections.Generic;
using Server;
using Server.Multis;
using Server.Targeting;
using Server.ContextMenus;
using Server.Gumps;
using Server.Multis.Deeds;

namespace Server.Items
{
	public interface IDyable
	{
		bool Dye( Mobile from, DyeTub sender );
	}

	public class DyeTub : Item, ISecurable, IDyePreviewSource
	{
		private bool m_Redyable;
		private int m_DyedHue;
		private SecureLevel m_SecureLevel;

		public virtual CustomHuePicker CustomHuePicker{ get{ return null; } }

		public virtual bool AllowRunebooks
		{
			get{ return false; }
		}

		public virtual bool AllowFurniture
		{
			get{ return false; }
		}

		public virtual bool AllowStatuettes
		{
			get{ return false; }
		}

		public virtual bool AllowLeather
		{
			get{ return false; }
		}

		public virtual bool AllowArmor
		{
			get{ return false; }
		}

		public virtual bool AllowDyables
		{
			get{ return true; }
		}

		public virtual bool AllowWeapons
		{
			get{ return false; }
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version
			
			writer.Write( (int)m_SecureLevel );
			writer.Write( (bool) m_Redyable );
			writer.Write( (int) m_DyedHue );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
				{
					m_SecureLevel = (SecureLevel)reader.ReadInt();
					goto case 0;
				}
				case 0:
				{
					m_Redyable = reader.ReadBool();
					m_DyedHue = reader.ReadInt();

					break;
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Redyable
		{
			get
			{
				return m_Redyable;
			}
			set
			{
				m_Redyable = value;
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int DyedHue
		{
			get
			{
				return m_DyedHue;
			}
			set
			{
				if ( m_Redyable )
				{
					m_DyedHue = value;
					Hue = value;
				}
			}
		}
		
		[CommandProperty( AccessLevel.GameMaster )]
		public SecureLevel Level
		{
			get
			{
				return m_SecureLevel;
			}
			set
			{
				m_SecureLevel = value;
			}
		}

		[Constructable] 
		public DyeTub() : base( 0xFAB )
		{
			Weight = 10.0;
			m_Redyable = true;
		}
		
		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list )
		{
			base.GetContextMenuEntries( from, list );
			SetSecureLevelEntry.AddTo( from, this, list );
		}

		public DyeTub( Serial serial ) : base( serial )
		{
		}

		public Item SourceItem
		{
			get { return this; }
		}

		public virtual int PreviewHue
		{
			get { return DyedHue; }
		}

		public virtual bool CanBeginPreview( Mobile from, out string failureMessage )
		{
			failureMessage = null;

			if ( Deleted || from == null || from.Deleted )
			{
				failureMessage = "That dye source is no longer available.";
				return false;
			}

			if ( !from.InRange( GetWorldLocation(), 1 ) )
			{
				failureMessage = "That is too far away.";
				return false;
			}

			return true;
		}

		public virtual bool CanPreviewTarget( Mobile from, Item target, int selectedHue, out string failureMessage )
		{
			failureMessage = null;

			if ( !DyePreviewUtility.IsOwnedGear( from, target ) )
			{
				failureMessage = "You may only preview dye on equipment you are wearing or carrying in your backpack.";
				return false;
			}

			if ( !from.InRange( GetWorldLocation(), 1 ) || !from.InRange( target.GetWorldLocation(), 1 ) )
			{
				failureMessage = "That is too far away.";
				return false;
			}

			if ( target is IDyable && AllowDyables )
			{
				BaseClothing clothing = target as BaseClothing;

				if ( clothing != null && !clothing.CanPreviewDye( from, this ) )
				{
					failureMessage = "That item cannot be dyed with this source.";
					return false;
				}

				return true;
			}

			if ( IsSpecialDirectTarget( target ) )
			{
				if ( !target.Movable )
				{
					failureMessage = "That item cannot be dyed.";
					return false;
				}

				return true;
			}

			if ( target is BaseArmor && ( Server.Misc.MaterialInfo.IsAnyKindOfClothItem( target ) || target is ElvenBoots ) && AllowLeather )
			{
				if ( !target.Movable )
				{
					failureMessage = "That item cannot be dyed.";
					return false;
				}

				return true;
			}

			if ( target is BaseArmor
				&& ( Server.Misc.MaterialInfo.IsAnyKindOfMetalItem( target ) || Server.Misc.MaterialInfo.IsAnyKindOfWoodItem( target ) )
				&& AllowArmor )
			{
				if ( !target.Movable )
				{
					failureMessage = "That item cannot be dyed.";
					return false;
				}

				return true;
			}

			if ( target is BaseWeapon && AllowWeapons )
			{
				if ( !target.Movable )
				{
					failureMessage = "That item cannot be dyed.";
					return false;
				}

				return true;
			}

			failureMessage = "That item cannot be dyed with this source.";
			return false;
		}

		public virtual bool CanApply( Mobile from, Item target, int selectedHue, out string failureMessage )
		{
			return CanBeginPreview( from, out failureMessage )
				&& CanPreviewTarget( from, target, selectedHue, out failureMessage );
		}

		public virtual bool ApplyPermanentDye( Mobile from, Item target, int selectedHue )
		{
			string failureMessage;

			if ( !CanApply( from, target, selectedHue, out failureMessage ) )
			{
				if ( from != null && !String.IsNullOrEmpty( failureMessage ) )
					from.SendMessage( failureMessage );

				return false;
			}

			if ( target is IDyable && AllowDyables )
			{
				if ( ((IDyable)target).Dye( from, this ) )
				{
					if ( target.Hue != selectedHue )
						target.Hue = selectedHue;

					from.PlaySound( 0x23E );
					return true;
				}

				return false;
			}

			if ( IsSpecialDirectTarget( target )
				|| ( target is BaseArmor && ( Server.Misc.MaterialInfo.IsAnyKindOfClothItem( target ) || target is ElvenBoots ) && AllowLeather )
				|| ( target is BaseArmor
					&& ( Server.Misc.MaterialInfo.IsAnyKindOfMetalItem( target ) || Server.Misc.MaterialInfo.IsAnyKindOfWoodItem( target ) )
					&& AllowArmor )
				|| ( target is BaseWeapon && AllowWeapons ) )
			{
				target.Hue = selectedHue;
				from.PlaySound( 0x23E );
				return true;
			}

			return false;
		}

		public virtual bool IsSourceStillValid( Mobile from )
		{
			string failureMessage;
			return CanBeginPreview( from, out failureMessage );
		}

		public virtual string GetSourceDisplayName()
		{
			return !String.IsNullOrEmpty( Name ) ? Name : GetType().Name;
		}

		public virtual string GetConsumptionMessage()
		{
			return null;
		}

		public virtual void BeginTargeting( Mobile from )
		{
			string failureMessage;

			if ( !CanBeginPreview( from, out failureMessage ) )
			{
				if ( from != null && !String.IsNullOrEmpty( failureMessage ) )
					from.SendMessage( failureMessage );

				return;
			}

			from.SendLocalizedMessage( TargetMessage );
			from.Target = new InternalTarget( this );
		}

		private static bool IsSpecialDirectTarget( Item item )
		{
			return item is ILevelable
				|| item is IGiftable
				|| item is RibbonTree
				|| item is RibbonTreeSmall
				|| item is DragonOrbStatue
				|| item is BaseBoatDeed
				|| item is HouseDeed
				|| item is HousePlacementTool
				|| item is BaseDockedBoat
				|| item is MagicCloak
				|| item is RandomLetter
				|| item is MagicHat
				|| item is Pillows
				|| item is ResearchBag
				|| item is ColoredWallTorch
				|| item is MagicPigment
				|| item is PaintPaletteBase
				|| item is BaseQuiver
				|| item is MagicBelt
				|| item is MagicSash
				|| item is MagicBoots
				|| item is MagicRobe
				|| item is HairDyeBottle
				|| item is HairDyePotion
				|| item is BagOfTricks
				|| item is IThrowingGloves
				|| item is IPugilistGloves;
		}

		// Select the clothing to dye.
		public virtual int TargetMessage{ get{ return 500859; } }

		// You can not dye that.
		public virtual int FailMessage{ get{ return 1042083; } }

		public override void OnDoubleClick( Mobile from )
		{
			BeginTargeting( from );
		}

		private class InternalTarget : Target
		{
			private DyeTub m_Tub;

			public InternalTarget( DyeTub tub ) : base( 1, false, TargetFlags.None )
			{
				m_Tub = tub;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is Item )
				{
					Item item = (Item)targeted;

					if ( item is IDyable && m_Tub.AllowDyables )
					{
						if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
							from.SendLocalizedMessage( 500446 ); // That is too far away.
						else if ( DyePreviewUtility.IsWearableGear( item ) )
							DyePreviewSession.Begin( from, m_Tub, item );
						else if ( ((IDyable)item).Dye( from, m_Tub ) )
							from.PlaySound( 0x23E );
					}
					else if ( FurnitureAttribute.Check( item ) )
					{
						if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
						{
							from.SendLocalizedMessage( 500446 ); // That is too far away.
						}
						else
						{
							bool okay = ( item.IsChildOf( from.Backpack ) );

							if ( !okay )
							{
								if ( item.Parent == null )
								{
									BaseHouse house = BaseHouse.FindHouseAt( item );

									if ( house == null || ( !house.IsLockedDown( item ) && !house.IsSecure( item ) ) )
										from.SendLocalizedMessage( 501022 ); // Furniture must be locked down to paint it.
									else if ( !house.IsCoOwner( from ) )
										from.SendLocalizedMessage( 501023 ); // You must be the owner to use this item.
									else
										okay = true;
								}
								else
								{
									from.SendLocalizedMessage( 1048135 ); // The furniture must be in your backpack to be painted.
								}
							}

							if ( okay )
							{
								item.Hue = m_Tub.DyedHue;
								from.PlaySound( 0x23E );
							}
						}
					}
					else if ( (item is Runebook || item is RecallRune ) && m_Tub.AllowRunebooks )
					{
						if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
						{
							from.SendLocalizedMessage( 500446 ); // That is too far away.
						}
						else if ( !item.Movable )
						{
							from.SendLocalizedMessage( 1049776 ); // You cannot dye runes or runebooks that are locked down.
						}
						else
						{
							item.Hue = m_Tub.DyedHue;
							from.PlaySound( 0x23E );
						}
					}
					else if ( IsSpecialDirectTarget( item ) )
					{
						if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
						{
							from.SendLocalizedMessage( 500446 ); // That is too far away.
						}
						else if ( !item.Movable )
						{
							from.SendLocalizedMessage( 1042083 ); // You cannot dye that.
						}
						else if ( DyePreviewUtility.IsWearableGear( item ) )
							DyePreviewSession.Begin( from, m_Tub, item );
						else if ( item is MagicPigment )
						{
							MagicPigment pigment = (MagicPigment)item;
							pigment.ApplyHue(from, m_Tub.DyedHue, 0x23E);
						}
						else if ( item is PaintPaletteBase )
						{
							PaintPaletteBase pigment = (PaintPaletteBase)item;
							pigment.ApplyHue(from, m_Tub.DyedHue, 0x23E);
						}
						else
						{
							item.Hue = m_Tub.DyedHue;
							from.PlaySound( 0x23E );
						}

						if ( ( item is BaseBoatDeed || item is BaseDockedBoat ) && item.Hue < 1 ){ item.Hue = 0x5BE; }
					}
					else if ( item is DDRelicRugAddonDeed )
					{
						if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
						{
							from.SendLocalizedMessage( 500446 ); // That is too far away.
						}
						else if ( !item.Movable )
						{
							from.SendLocalizedMessage( 1042083 ); // You cannot dye that.
						}
						else
						{
							item.Hue = m_Tub.DyedHue;
							DDRelicRugAddonDeed relic = (DDRelicRugAddonDeed)item;
							relic.RelicColor = m_Tub.DyedHue;
							from.PlaySound( 0x23E );
						}
					}
					else if ( item is HenchmanFamiliarItem || item is WaxPainting || item is WaxSculptors || item is WaxSculptorsD || item is WaxSculptorsE || item is ColorCandleShort || item is ColorCandleLong )
					{
						if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
						{
							from.SendLocalizedMessage( 500446 ); // That is too far away.
						}
						else if ( !item.Movable )
						{
							from.SendLocalizedMessage( 1042083 ); // You cannot dye that.
						}
						else
						{
							item.Hue = m_Tub.DyedHue;
							from.PlaySound( 0x23E );
						}
					}
					else if ( ( item is MyTentEastAddonDeed ) || ( item is MyTentSouthAddonDeed ) )
					{
						if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
						{
							from.SendLocalizedMessage( 500446 ); // That is too far away.
						}
						else if ( !item.Movable )
						{
							from.SendLocalizedMessage( 1042083 ); // You cannot dye that.
						}
						else
						{
							item.Hue = m_Tub.DyedHue;
							if ( item is MyTentEastAddonDeed ) { MyTentEastAddonDeed tent = (MyTentEastAddonDeed)item; tent.TentColor = m_Tub.DyedHue; }
							else { MyTentSouthAddonDeed tent = (MyTentSouthAddonDeed)item; tent.TentColor = m_Tub.DyedHue; }
							from.PlaySound( 0x23E );
						}
					}
					else if ( ( item is MyCircusTentEastAddonDeed ) || ( item is MyCircusTentSouthAddonDeed ) )
					{
						if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
						{
							from.SendLocalizedMessage( 500446 ); // That is too far away.
						}
						else if ( !item.Movable )
						{
							from.SendLocalizedMessage( 1042083 ); // You cannot dye that.
						}
						else
						{
							item.Hue = m_Tub.DyedHue;
							if ( item is MyCircusTentEastAddonDeed ) { MyCircusTentEastAddonDeed tent = (MyCircusTentEastAddonDeed)item; tent.TentColor1 = m_Tub.DyedHue; }
							else { MyCircusTentSouthAddonDeed tent = (MyCircusTentSouthAddonDeed)item; tent.TentColor1 = m_Tub.DyedHue; }
							from.PlaySound( 0x23E );
						}
					}
					else if ( item is MonsterStatuette && m_Tub.AllowStatuettes )
					{
						if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
						{
							from.SendLocalizedMessage( 500446 ); // That is too far away.
						}
						else if ( !item.Movable )
						{
							from.SendLocalizedMessage( 1049779 ); // You cannot dye statuettes that are locked down.
						}
						else
						{
							item.Hue = m_Tub.DyedHue;
							from.PlaySound( 0x23E );
						}
					}
					else if ( item is BaseArmor && ( Server.Misc.MaterialInfo.IsAnyKindOfClothItem( item ) || item is ElvenBoots ) && m_Tub.AllowLeather )
					{
						if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
						{
							from.SendLocalizedMessage( 500446 ); // That is too far away.
						}
						else if ( !item.Movable )
						{
							from.SendLocalizedMessage( 1042419 ); // You may not dye leather items which are locked down.
						}
						else if ( DyePreviewUtility.IsWearableGear( item ) )
							DyePreviewSession.Begin( from, m_Tub, item );
						else
						{
							item.Hue = m_Tub.DyedHue;
							from.PlaySound( 0x23E );
						}
					}
					else if ( item is BaseArmor && ( Server.Misc.MaterialInfo.IsAnyKindOfMetalItem( item ) || Server.Misc.MaterialInfo.IsAnyKindOfWoodItem( item ) ) && m_Tub.AllowArmor )
					{
						if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
						{
							from.SendLocalizedMessage( 500446 ); // That is too far away.
						}
						else if ( !item.Movable )
						{
							from.SendLocalizedMessage( 1042083 ); // You cannot dye that.
						}
						else if ( DyePreviewUtility.IsWearableGear( item ) )
							DyePreviewSession.Begin( from, m_Tub, item );
						else
						{
							item.Hue = m_Tub.DyedHue;
							from.PlaySound( 0x23E );
						}
					}
					else if ( item is BaseWeapon && m_Tub.AllowWeapons )
					{
						if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
						{
							from.SendLocalizedMessage( 500446 ); // That is too far away.
						}
						else if ( !item.Movable )
						{
							from.SendLocalizedMessage( 1042083 ); // You cannot dye that.
						}
						else if ( DyePreviewUtility.IsWearableGear( item ) )
							DyePreviewSession.Begin( from, m_Tub, item );
						else
						{
							item.Hue = m_Tub.DyedHue;
							from.PlaySound( 0x23E );
						}
					}
					else
					{
						from.SendLocalizedMessage( m_Tub.FailMessage );
					}
				}
				else
				{
					from.SendLocalizedMessage( m_Tub.FailMessage );
				}
			}
		}
	}
}
