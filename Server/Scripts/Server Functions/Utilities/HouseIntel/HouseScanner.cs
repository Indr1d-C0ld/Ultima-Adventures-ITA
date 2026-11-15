using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Multis;

namespace Server.Utilities
{
	public class HouseScanResult
	{
		public int TotalItems;
		public int LockedDownCount;
		public int SecuredCount;
		public int ContainerCount;
		public int VendorCount;
		public DateTime LastComputed;
	}

	public static class HouseScanner
	{
		private static readonly Dictionary<Serial, HouseScanResult> m_Cache = new Dictionary<Serial, HouseScanResult>();

		public static HouseScanResult GetStats( BaseHouse house, bool force )
		{
			if ( house == null || house.Deleted )
				return new HouseScanResult { LastComputed = DateTime.UtcNow };

			HouseScanResult cached;
			if ( !force && m_Cache.TryGetValue( house.Serial, out cached ) )
			{
				if ( DateTime.UtcNow - cached.LastComputed <= HouseIntelConfig.CacheDuration )
					return cached;
			}

			HouseScanResult fresh = ScanHouse( house );
			m_Cache[house.Serial] = fresh;
			return fresh;
		}

		public static IEnumerable<Item> EnumerateRootItems( BaseHouse house )
		{
			if ( house == null || house.Deleted || house.Map == null || house.Map == Map.Internal )
				yield break;

			Point2D start = new Point2D( house.X + house.Components.Min.X, house.Y + house.Components.Min.Y );
			Point2D end = new Point2D( house.X + house.Components.Max.X + 1, house.Y + house.Components.Max.Y + 1 );
			Rectangle2D rect = new Rectangle2D( start, end );

			IPooledEnumerable eable = house.Map.GetItemsInBounds( rect );

			foreach ( Item item in eable )
			{
				if ( item == null || item.Deleted )
					continue;

				if ( item.RootParent != null )
					continue;

				if ( !house.IsInside( item ) )
					continue;

				yield return item;
			}

			eable.Free();
		}

		private static HouseScanResult ScanHouse( BaseHouse house )
		{
			HouseScanResult result = new HouseScanResult
			{
				LastComputed = DateTime.UtcNow
			};

			HashSet<Serial> visited = new HashSet<Serial>();

			foreach ( Item root in EnumerateRootItems( house ) )
			{
				if ( IsExcludedItem( house, root ) )
					continue;

				ScanItemTree( house, root, result, visited );
			}

			result.VendorCount = CountVendors( house );

			return result;
		}

		private static void ScanItemTree( BaseHouse house, Item item, HouseScanResult result, HashSet<Serial> visited )
		{
			if ( item == null || item.Deleted )
				return;

			if ( visited.Contains( item.Serial ) )
				return;

			visited.Add( item.Serial );

			if ( IsExcludedItem( house, item ) )
				return;

			result.TotalItems++;

			if ( item is Container )
				result.ContainerCount++;

			if ( house.IsLockedDown( item ) )
				result.LockedDownCount++;

			if ( house.IsSecure( item ) )
				result.SecuredCount++;

			List<Item> items = item.Items;

			for ( int i = 0; i < items.Count; ++i )
			{
				ScanItemTree( house, items[i], result, visited );
			}
		}

		private static int CountVendors( BaseHouse house )
		{
			if ( house == null )
				return 0;

			int count = 0;

			if ( house.PlayerVendors != null )
				count += house.PlayerVendors.Count;

			if ( house.InternalizedVendors != null )
				count += house.InternalizedVendors.Count;

			return count;
		}

		public static bool IsExcludedItem( BaseHouse house, Item item )
		{
			if ( item == null )
				return true;

			if ( item == house.MovingCrate )
				return true;

			if ( item is HouseSign )
				return true;

			if ( item is BaseHouseDoor || item is BaseDoor )
				return true;

			if ( item is BaseAddon || item is AddonComponent )
				return true;

			return false;
		}
	}
}
