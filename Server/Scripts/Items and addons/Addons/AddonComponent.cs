using System;
using Server;
using Server.Multis;
using Server.Targeting;

namespace Server.Items
{
	public static class ForgeAndSawmillHelper
	{
		public static bool IsRestrictedPlacementItem( Item item )
		{
			if ( item == null )
				return false;

			if ( item.Name == "saw mill" &&
				( item.ItemID == 1928 || item.ItemID == 4525 || item.ItemID == 7130 || item.ItemID == 4530 || item.ItemID == 7127 ) )
				return true;

			if ( item.GetType().IsDefined( typeof( Server.Engines.Craft.ForgeAttribute ), false ) )
				return true;

			if ( item is DwarvenForge )
				return true;

			int itemID = item.ItemID;

			return ( itemID == 4017 || (itemID >= 0x10DE && itemID <= 0x10E0) || (itemID >= 6522 && itemID <= 6569) || itemID == 0x2DD8 );
		}

		public static bool CheckWorldDrop( Mobile from, Item item, Point3D p )
		{
			if ( !IsRestrictedPlacementItem( item ) )
				return true;

			BaseHouse house = BaseHouse.FindHouseAt( p, from.Map, p.Z );

			if ( house != null )
				return true;

			from.SendMessage( "Forges and saw mills can only be dropped inside a player house." );
			return false;
		}

		public static bool TryHandleDragDrop( Mobile from, object target, Item dropped )
		{
			if ( from == null || target == null || dropped == null )
				return false;

			BaseOre ore = dropped as BaseOre;
			if ( ore != null && IsForgeTarget( target, false ) )
				return ore.Smelt( from, target, ore.Amount );

			BaseLog log = dropped as BaseLog;
			if ( log != null && BaseLog.IsSawmill( target ) )
				return log.Cut( from, target, log.Amount );

			return false;
		}

		public static bool IsForgeTarget( object target, bool consumeCharge )
		{
			AddonComponent component = target as AddonComponent;

			if ( component != null && component.Addon != null && component.Map != null && component.Map != Map.Internal )
			{
				int itemID = component.ItemID;

				if ( itemID == 4017 || (itemID >= 0x10DE && itemID <= 0x10E0) || (itemID >= 6522 && itemID <= 6569) || itemID == 0x2DD8 )
					return true;
			}

			return Server.Engines.Craft.DefBlacksmithy.IsForge( target, consumeCharge );
		}

		public static bool IsItemDragDropTarget( Item dropped, Item target )
		{
			if ( dropped == null || target == null )
				return false;

			BaseOre ore = dropped as BaseOre;
			if ( ore != null )
				return IsForgeTarget( target, false );

			BaseLog log = dropped as BaseLog;
			if ( log != null )
				return BaseLog.IsSawmill( target );

			return false;
		}

		public static bool TryHandleItemDragDrop( Mobile from, Item target, Item dropped, Point3D location )
		{
			if ( !IsItemDragDropTarget( dropped, target ) )
				return false;

			if ( from == null || from.Deleted || target.Deleted || dropped.Deleted ||
				from.Map == null || target.Map == null || dropped.Map != Map.Internal || from.Map != target.Map )
				return false;

			if ( from.AccessLevel < AccessLevel.GameMaster && !from.InRange( target.GetWorldLocation(), 2 ) )
				return false;

			// Item.DropToItem also applies a line-of-sight collision test. Forge and sawmill
			// graphics are commonly impassable or multi-piece, so use the same range-based
			// behavior as the static-tile route while retaining visibility and house access.
			if ( !from.CanSee( target ) || !target.IsAccessibleTo( from ) )
				return false;

			object root = target.RootParent;
			if ( root is Mobile && !((Mobile)root).CheckNonlocalDrop( from, dropped, target ) )
				return false;

			if ( dropped.Nontransferable && from.Player && target != from.Backpack && from.AccessLevel < AccessLevel.GameMaster )
				return false;

			if ( !from.OnDroppedItemToItem( dropped, target, location ) )
				return false;

			if ( !from.OnDroppedItemOnto( dropped, target ) )
				return false;

			return TryHandleDragDrop( from, target, dropped );
		}

		public static bool TryHandleStaticDragDrop( Mobile from, Item dropped, Point3D location )
		{
			if ( from == null || dropped == null || from.Map == null || !from.InRange( location, 2 ) )
				return false;

			BaseOre ore = dropped as BaseOre;
			BaseLog log = dropped as BaseLog;
			IPooledEnumerable<Item> items = from.Map.GetItemsInRange( location, 1 );

			foreach ( Item target in items )
			{
				if ( ore != null && IsForgeTarget( target, false ) )
				{
					TryHandleDragDrop( from, target, dropped );
					items.Free();
					return true;
				}

				if ( log != null && BaseLog.IsSawmill( target ) )
				{
					TryHandleDragDrop( from, target, dropped );
					items.Free();
					return true;
				}
			}

			items.Free();
			StaticTile[] tiles = from.Map.Tiles.GetStaticTiles( location.X, location.Y, true );

			for ( int i = 0; i < tiles.Length; ++i )
			{
				StaticTile tile = tiles[i];
				StaticTarget target = new StaticTarget( new Point3D( location.X, location.Y, tile.Z ), tile.ID );

				if ( ore != null && IsForgeTarget( target, false ) )
				{
					TryHandleDragDrop( from, target, dropped );
					return true;
				}

				if ( log != null && BaseLog.IsSawmill( target ) )
				{
					TryHandleDragDrop( from, target, dropped );
					return true;
				}
			}

			return false;
		}
	}

	[Server.Engines.Craft.Anvil]
	public class AnvilComponent : AddonComponent
	{
		[Constructable]
		public AnvilComponent( int itemID ) : base( itemID )
		{
		}

		public AnvilComponent( Serial serial ) : base( serial )
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

	[Server.Engines.Craft.Forge]
	public class ForgeComponent : AddonComponent
	{
		[Constructable]
		public ForgeComponent( int itemID ) : base( itemID )
		{
		}

		public ForgeComponent( Serial serial ) : base( serial )
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

	public class LocalizedAddonComponent : AddonComponent
	{
		private int m_LabelNumber;

		[CommandProperty( AccessLevel.GameMaster )]
		public int Number
		{
			get{ return m_LabelNumber; }
			set{ m_LabelNumber = value; InvalidateProperties(); }
		}

		public override int LabelNumber{ get{ return m_LabelNumber; } }

		[Constructable]
		public LocalizedAddonComponent( int itemID, int labelNumber ) : base( itemID )
		{
			m_LabelNumber = labelNumber;
		}

		public LocalizedAddonComponent( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( (int) m_LabelNumber );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_LabelNumber = reader.ReadInt();
					break;
				}
			}
		}
	}

	public class AddonComponent : Item, IChopable
	{
		private Point3D m_Offset;
		private BaseAddon m_Addon;

		[CommandProperty( AccessLevel.GameMaster )]
		public BaseAddon Addon
		{
			get
			{
				return m_Addon;
			}
			set
			{
				m_Addon = value;
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Point3D Offset
		{
			get
			{
				return m_Offset;
			}
			set
			{
				m_Offset = value;
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool PreventAxePullUp
		{
			get
			{
				return ( m_Addon != null && m_Addon.PreventAxePullUp );
			}
			set
			{
				if ( m_Addon != null )
					m_Addon.PreventAxePullUp = value;
			}
		}

		[Hue, CommandProperty( AccessLevel.GameMaster )]
		public override int Hue
		{
			get
			{
				return base.Hue;
			}
			set
			{
				base.Hue = value;

				if ( m_Addon != null && m_Addon.ShareHue )
					m_Addon.Hue = value;
			}
		}

		public virtual bool NeedsWall{ get{ return false; } }
		public virtual Point3D WallPosition{ get{ return Point3D.Zero; } }

		public override int GetPacketFlags()
		{
			int flags = base.GetPacketFlags();

			// The client requires the world-item target flag in addition to the
			// Container tiledata flag before it sends this component's serial.
			// OnDragLift keeps the component server-side immovable.
			if ( ForgeAndSawmillHelper.IsRestrictedPlacementItem( this ) )
				flags |= 0x20;

			return flags;
		}

		[Constructable]
		public AddonComponent( int itemID ) : base( itemID )
		{
			Movable = false;
			ApplyLightTo( this );
		}

		public AddonComponent( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( m_Addon != null )
				m_Addon.OnComponentUsed( this, from );
		}

		public override bool OnDragLift( Mobile from )
		{
			if ( ForgeAndSawmillHelper.IsRestrictedPlacementItem( this ) )
				return false;

			return base.OnDragLift( from );
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if ( ForgeAndSawmillHelper.TryHandleDragDrop( from, this, dropped ) )
				return true;

			return base.OnDragDrop( from, dropped );
		}

		public void OnChop( Mobile from )
		{
			if ( m_Addon != null && from.InRange( GetWorldLocation(), 3 ) )
				m_Addon.OnChop( from );
			else
				from.SendLocalizedMessage( 500446 ); // That is too far away.
		}

		public override void OnLocationChange( Point3D old )
		{
			if ( m_Addon != null )
				m_Addon.Location = new Point3D( X - m_Offset.X, Y - m_Offset.Y, Z - m_Offset.Z );
		}

		public override void OnMapChange()
		{
			if ( m_Addon != null )
				m_Addon.Map = Map;
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if ( m_Addon != null )
				m_Addon.Delete();
		}

		public override bool OnDroppedToWorld( Mobile from, Point3D p )
		{
			if ( !ForgeAndSawmillHelper.CheckWorldDrop( from, this, p ) )
				return false;

			return base.OnDroppedToWorld( from, p );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

			writer.Write( m_Addon );
			writer.Write( m_Offset );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
				case 0:
				{
					m_Addon = reader.ReadItem() as BaseAddon;
					m_Offset = reader.ReadPoint3D();

					if ( m_Addon != null )
						m_Addon.OnComponentLoaded( this );

					ApplyLightTo( this );

					break;
				}
			}

			if ( version < 1 && Weight == 0 )
				Weight = -1;
		}

		public static void ApplyLightTo( Item item )
		{
			if ( (item.ItemData.Flags & TileFlag.LightSource) == 0 )
				return; // not a light source

			int itemID = item.ItemID;

			for ( int i = 0; i < m_Entries.Length; ++i )
			{
				LightEntry entry = m_Entries[i];
				int[] toMatch = entry.m_ItemIDs;
				bool contains = false;

				for ( int j = 0; !contains && j < toMatch.Length; ++j )
					contains = ( itemID == toMatch[j] );

				if ( contains )
				{
					item.Light = entry.m_Light;
					return;
				}
			}
		}

		private static LightEntry[] m_Entries = new LightEntry[]
			{
				new LightEntry( LightType.WestSmall, 1122, 1123, 1124, 1141, 1142, 1143, 1144, 1145, 1146, 2347, 2359, 2360, 2361, 2362, 2363, 2364, 2387, 2388, 2389, 2390, 2391, 2392 ),
				new LightEntry( LightType.NorthSmall, 1131, 1133, 1134, 1147, 1148, 1149, 1150, 1151, 1152, 2352, 2373, 2374, 2375, 2376, 2377, 2378, 2401, 2402, 2403, 2404, 2405, 2406 ),
				new LightEntry( LightType.Circle300, 6526, 6538, 6571 ),
				new LightEntry( LightType.Circle150, 5703, 6587 )
			};

		private class LightEntry
		{
			public LightType m_Light;
			public int[] m_ItemIDs;

			public LightEntry( LightType light, params int[] itemIDs )
			{
				m_Light = light;
				m_ItemIDs = itemIDs;
			}
		}
	}
}
