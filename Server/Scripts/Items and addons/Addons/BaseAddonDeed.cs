using System;
using System.Collections.Generic;
using Server;
using Server.Multis;
using Server.Targeting;

using Server.Commands;
using Server.Commands.Generic;
using Server.Network;

namespace Server.Items
{
        [Flipable( 0x14F0, 0x14EF )]
        public abstract class BaseAddonDeed : Item
        {
                public abstract BaseAddon Addon{ get; }

                private AddonPlacementPreview m_Preview;

                protected virtual bool RequiresHousePlacement
                {
                        get { return false; }
                }

                protected virtual bool CanPlaceInHouse( Mobile from, BaseHouse house )
                {
                        if ( !RequiresHousePlacement )
                                return true;

                        if ( house == null )
                        {
                                from.SendMessage( "This can only be placed inside a player house." );
                                return false;
                        }

                        if ( !house.IsCoOwner( from ) && !house.IsOwner( from ) )
                        {
                                from.SendMessage( "You can only place this inside a player house where you are a co-owner." );
                                return false;
                        }

                        return true;
                }

		public BaseAddonDeed() : base( 0x14F0 )
		{
			Weight = 1.0;
		}

		public BaseAddonDeed( Serial serial ) : base( serial )
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

			if ( Weight == 0.0 )
				Weight = 1.0;
		}

                public override void OnDoubleClick( Mobile from )
                {
                        if ( IsChildOf( from.Backpack ) )
                                from.Target = new InternalTarget( this );
                        else
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
		}

                private class InternalTarget : Target
                {
                        private BaseAddonDeed m_Deed;

                        public InternalTarget( BaseAddonDeed deed ) : base( -1, true, TargetFlags.None )
			{
				m_Deed = deed;

				CheckLOS = false;
			}

                        protected override void OnTarget( Mobile from, object targeted )
                        {
                                AddonPlacementPreview preview = targeted as AddonPlacementPreview;

                                if ( preview == null )
                                {
                                        AddonPreviewGhost ghost = targeted as AddonPreviewGhost;

                                        if ( ghost != null )
                                                preview = ghost.Preview;
                                }

                                if ( preview != null && preview.Deed == m_Deed )
                                {
                                        m_Deed.CommitPlacement( from, preview );
                                        return;
                                }

                                IPoint3D p = targeted as IPoint3D;
                                Point3D p2 = new Point3D( p);
                                Point3D fromloc = new Point3D( from);

                                Map map = from.Map;

                                if ( p == null || map == null || m_Deed.Deleted )
                                        return;

                                m_Deed.ClearPreview();

				if ( m_Deed.IsChildOf( from.Backpack ) )
				{
					BaseAddon addon = m_Deed.Addon;

					Server.Spells.SpellHelper.GetSurfaceTop( ref p );

                                        BaseHouse house = BaseHouse.FindHouseAt( p2, map, p2.Z );

                                        if ( !m_Deed.CanPlaceInHouse( from, house ) )
                                        {
                                                addon.Delete();
                                                return;
                                        }

                                        AddonFitResult res = addon.CouldFit( p, map, from, ref house );

                                        if ( !(Utility.InRange( p2, fromloc, 10 )) )
                                        {
                                                from.SendMessage( 0,  "This is too far away." );
						return;
					}

                                        m_Deed.m_Preview = new AddonPlacementPreview( m_Deed, addon, new Point3D( p ), map, house, res );
                                        addon.Delete();
                                        from.Target = new InternalTarget( m_Deed );

                                        if ( res == AddonFitResult.Valid )
                                                from.SendMessage( "A ghost preview has been placed. Target it again to confirm placement." );
                                        else if ( res == AddonFitResult.Blocked )
                                                from.SendMessage( "Preview placed, but red highlights show blocked tiles." );
                                        else if ( res == AddonFitResult.DoorTooClose )
                                                from.SendMessage( "Preview shows a door conflict. Reposition before confirming." );
                                        else if ( res == AddonFitResult.NoWall )
                                                from.SendLocalizedMessage( 500268 ); // This object needs to be mounted on something.
                                        else
                                                from.SendMessage( "Preview placed. Resolve the highlighted issue before confirming." );
                                }
                                else
                                {
                                        from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
                                }
                        }

                        protected override void OnTargetCancel( Mobile from, TargetCancelType cancelType )
                        {
                                base.OnTargetCancel( from, cancelType );

                                m_Deed.ClearPreview();
                        }
                }

                internal void ClearPreview( AddonPlacementPreview preview = null )
                {
                        if ( preview != null )
                        {
                                if ( m_Preview != preview )
                                        return;

                                m_Preview = null;

                                if ( !preview.Deleted )
                                        preview.Delete();

                                return;
                        }

                        if ( m_Preview != null )
                        {
                                var temp = m_Preview;
                                m_Preview = null;

                                if ( !temp.Deleted )
                                        temp.Delete();
                        }
                }

                private void CommitPlacement( Mobile from, AddonPlacementPreview preview )
                {
                        if ( preview == null )
                                return;

                        if ( !IsChildOf( from.Backpack ) )
                        {
                                from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
                                return;
                        }

                        Point3D targetLocation = preview.TargetLocation;
                        Map targetMap = preview.Map;

                        // Remove the preview first so its ghost tiles don't block CanFit checks for players
                        if ( !preview.Deleted )
                                preview.Delete();

                        m_Preview = null;

                        BaseAddon addon = Addon;

                        BaseHouse house = BaseHouse.FindHouseAt( targetLocation, targetMap, targetLocation.Z );

                        if ( !CanPlaceInHouse( from, house ) )
                        {
                                addon.Delete();
                                return;
                        }

                        AddonFitResult res = addon.CouldFit( targetLocation, targetMap, from, ref house );

                        if ( !(Utility.InRange( targetLocation, from.Location, 10 )) )
                        {
                                from.SendMessage( 0, "This is too far away." );
                                addon.Delete();
                                return;
                        }

                        if ( res == AddonFitResult.Valid )
                        {
                                addon.MoveToWorld( targetLocation, targetMap );

                                if ( house == null )
                                        Delete();

                                if ( addon is BaseHarvestPatchAddon || addon is BaseFruitTreeAddon )
                                        addon.Owner = from;

                                if ( house != null )
                                {
                                        Delete();
                                        house.Addons.Add( addon );
                                }
                        }
                        else if ( res == AddonFitResult.Blocked )
                                from.SendLocalizedMessage( 500269 ); // You cannot build that there.
                        else if ( res == AddonFitResult.DoorTooClose )
                                from.SendLocalizedMessage( 500271 ); // You cannot build near the door.
                        else if ( res == AddonFitResult.NoWall )
                                from.SendLocalizedMessage( 500268 ); // This object needs to be mounted on something.

                        if ( res != AddonFitResult.Valid )
                                addon.Delete();
                }
        }
        public class AddonPreviewGhost : Static
        {
                public AddonPlacementPreview Preview{ get; private set; }

                public AddonPreviewGhost( AddonPlacementPreview preview, int itemID ) : base( itemID )
                {
                        Preview = preview;
                }

                public AddonPreviewGhost( Serial serial ) : base( serial )
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

                        Delete();
                }
        }

        public class AddonPlacementPreview : Item
        {
                private const int ValidHue = 1266;
                private const int BlockedHue = 33;
                private const int WallHue = 2413;
                private const int DoorHue = 1359;

                private readonly List<Item> m_Tiles;
                private readonly Timer m_Timer;

                public BaseAddonDeed Deed{ get; private set; }
                public Point3D TargetLocation{ get; private set; }

                public AddonPlacementPreview( BaseAddonDeed deed, BaseAddon addon, Point3D location, Map map, BaseHouse house, AddonFitResult result ) : base( 1 )
                {
                        Deed = deed;
                        TargetLocation = location;

                        Movable = false;
                        Visible = false;

                        m_Tiles = new List<Item>();

                        MoveToWorld( location, map );

                        if ( addon != null && addon.Components != null )
                        {
                                foreach ( AddonComponent component in addon.Components )
                                {
                                        if ( component == null )
                                                continue;

                                        AddonPreviewGhost ghost = new AddonPreviewGhost( this, component.ItemID );
                                        ghost.Movable = false;
                                        ghost.Visible = true;
                                        ghost.Hue = GetHue( component, location, map, house );
                                        ghost.Name = result == AddonFitResult.Valid ? "Placement preview" : "Placement conflict";
                                        ghost.MoveToWorld( new Point3D( location.X + component.Offset.X, location.Y + component.Offset.Y, location.Z + component.Offset.Z ), map );

                                        m_Tiles.Add( ghost );
                                }
                        }

                        m_Timer = Timer.DelayCall( TimeSpan.FromSeconds( 30.0 ), Delete );
                }

                public AddonPlacementPreview( Serial serial ) : base( serial )
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

                        Delete();
                }

                private int GetHue( AddonComponent component, Point3D origin, Map map, BaseHouse house )
                {
                        Point3D componentLoc = new Point3D( origin.X + component.Offset.X, origin.Y + component.Offset.Y, origin.Z + component.Offset.Z );

                        if ( component.NeedsWall && !BaseAddon.IsWall( componentLoc.X + component.WallPosition.X, componentLoc.Y + component.WallPosition.Y, componentLoc.Z + component.WallPosition.Z, map ) )
                                return WallHue;

                        if ( IsDoorConflict( house, component, origin ) )
                                return DoorHue;

                        bool blocked = !map.CanFit( componentLoc.X, componentLoc.Y, componentLoc.Z, component.ItemData.Height, false, true, ( component.Z == 0 ) );

                        return blocked ? BlockedHue : ValidHue;
                }

                private static bool IsDoorConflict( BaseHouse house, AddonComponent component, Point3D origin )
                {
                        if ( house == null )
                                return false;

                        Point3D addonLoc = new Point3D( origin.X + component.Offset.X, origin.Y + component.Offset.Y, origin.Z + component.Offset.Z );
                        int addonHeight = component.ItemData.CalcHeight;

                        foreach ( BaseDoor door in house.Doors )
                        {
                                Point3D doorLoc = door.GetWorldLocation();
                                int doorHeight = door.ItemData.CalcHeight;

                                if ( Utility.InRange( doorLoc, addonLoc, 1 ) && ( addonLoc.Z == doorLoc.Z || ( (addonLoc.Z + addonHeight) > doorLoc.Z && (doorLoc.Z + doorHeight) > addonLoc.Z ) ) )
                                        return true;
                        }

                        return false;
                }

                public override void OnAfterDelete()
                {
                        base.OnAfterDelete();

                        if ( m_Timer != null )
                                m_Timer.Stop();

                        if ( m_Tiles != null )
                        {
                                foreach ( Item tile in m_Tiles )
                                        tile.Delete();
                        }

                        if ( Deed != null )
                                Deed.ClearPreview( this );
                }
        }
}
