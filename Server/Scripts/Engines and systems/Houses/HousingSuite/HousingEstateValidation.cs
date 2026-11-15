using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Server.Items;

namespace Server.Multis
{
	internal sealed class HousingMoveEntitySet
	{
		private sealed class ReferenceComparer : IEqualityComparer<IEntity>
		{
			public bool Equals( IEntity left, IEntity right )
			{
				return Object.ReferenceEquals( left, right );
			}

			public int GetHashCode( IEntity entity )
			{
				return RuntimeHelpers.GetHashCode( entity );
			}
		}

		private static readonly FieldInfo m_TrashField = typeof( BaseHouse ).GetField( "m_Trash", BindingFlags.Instance | BindingFlags.NonPublic );
		private readonly Dictionary<IEntity, bool> m_Identities = new Dictionary<IEntity, bool>( new ReferenceComparer() );
		private readonly List<IEntity> m_Entities = new List<IEntity>();

		public IList<IEntity> Entities { get { return m_Entities; } }

		public bool Contains( IEntity entity )
		{
			return entity != null && m_Identities.ContainsKey( entity );
		}

		public static HousingMoveEntitySet Capture( IList<HouseFoundation> houses )
		{
			HousingMoveEntitySet result = new HousingMoveEntitySet();

			if ( houses != null )
			{
				for ( int i = 0; i < houses.Count; ++i )
					result.AddHouse( houses[i] );
			}

			return result;
		}

		public static HousingMoveEntitySet Capture( IList<BaseHouse> houses )
		{
			HousingMoveEntitySet result = new HousingMoveEntitySet();

			if ( houses != null )
			{
				for ( int i = 0; i < houses.Count; ++i )
					result.AddHouse( houses[i] );
			}

			return result;
		}

		private void AddHouse( BaseHouse house )
		{
			if ( house == null || house.Deleted )
				return;

			AddEntity( house );
			AddEntity( house.Sign );
			AddItems( house.Doors );
			AddItems( house.LockDowns );
			AddItems( house.VendorRentalContracts );
			AddSecures( house.Secures );
			AddItems( house.Addons );
			AddEntities( house.PlayerVendors );
			AddEntities( house.PlayerBarkeepers );
			AddEntity( house.MovingCrate );

			HouseFoundation foundation = house as HouseFoundation;

			if ( foundation != null )
			{
				AddEntity( foundation.SignHanger );
				AddEntity( foundation.Signpost );
				AddItems( foundation.Fixtures );
			}

			if ( m_TrashField != null )
				AddEntity( m_TrashField.GetValue( house ) as IEntity );
		}

		private void AddSecures( IList secures )
		{
			if ( secures == null )
				return;

			for ( int i = 0; i < secures.Count; ++i )
			{
				SecureInfo info = secures[i] as SecureInfo;

				if ( info != null )
					AddEntity( info.Item );
			}
		}

		private void AddItems( IList items )
		{
			if ( items == null )
				return;

			for ( int i = 0; i < items.Count; ++i )
				AddEntity( items[i] as Item );
		}

		private void AddEntities( IList entities )
		{
			if ( entities == null )
				return;

			for ( int i = 0; i < entities.Count; ++i )
				AddEntity( entities[i] as IEntity );
		}

		private void AddEntity( IEntity entity )
		{
			if ( entity == null || m_Identities.ContainsKey( entity ) )
				return;

			Item item = entity as Item;
			Mobile mobile = entity as Mobile;

			if ( ( item != null && item.Deleted ) || ( mobile != null && mobile.Deleted ) )
				return;

			m_Identities.Add( entity, true );
			m_Entities.Add( entity );

			if ( item != null )
			{
				BaseAddon addon = item as BaseAddon;

				if ( addon != null )
				{
					AddItems( addon.Components );
					AddItems( addon.Items );
				}

				AddItems( item.Items );
			}
			else if ( mobile != null )
			{
				AddItems( mobile.Items );
			}
		}
	}

	public static class HousingEstateValidation
	{
		public static HousingValidationResult ValidateMove( Mobile actor, IList<HouseFoundation> houses, Map map, int deltaX, int deltaY, int deltaZ, bool force )
		{
			List<BaseHouse> baseHouses = new List<BaseHouse>( houses == null ? 0 : houses.Count );

			if ( houses != null )
			{
				for ( int i = 0; i < houses.Count; ++i )
					baseHouses.Add( houses[i] );
			}

			return ValidateMove( actor, baseHouses, map, deltaX, deltaY, deltaZ, force );
		}

		public static HousingValidationResult ValidateMove( Mobile actor, IList<BaseHouse> houses, Map map, int deltaX, int deltaY, int deltaZ, bool force )
		{
			if ( force && actor != null && !actor.Deleted && actor.AccessLevel < AccessLevel.GameMaster )
				return Invalid( "This operation requires " + AccessLevel.GameMaster + " access." );

			Point3D[] destinations;
			HousingValidationResult hardValidation = ValidateMoveHard( actor, houses, map, deltaX, deltaY, deltaZ, out destinations );

			if ( !hardValidation.IsValid )
				return hardValidation;

			if ( force )
				return HousingValidationResult.Valid;

			HousingMoveEntitySet movingEntities = HousingMoveEntitySet.Capture( houses );

			for ( int i = 0; i < houses.Count; ++i )
			{
				BaseHouse house = houses[i];
				ArrayList occupants;
				HousePlacementResult placement = HousePlacement.CheckMove( actor, map, house.ItemID, destinations[i], movingEntities, out occupants );

				if ( placement != HousePlacementResult.Valid )
					return Invalid( "House " + house.Serial + " has an invalid movement destination: " + placement + "." );

				if ( occupants != null && occupants.Count > 0 )
					return Invalid( "House " + house.Serial + " has an occupied movement destination." );

				HousingValidationResult actualComponents = ValidateActualComponents( actor, house, map, destinations[i], movingEntities );

				if ( !actualComponents.IsValid )
					return actualComponents;
			}

			return HousingValidationResult.Valid;
		}

		public static HousingValidationResult ValidateMoveHard( Mobile actor, IList<BaseHouse> houses, Map map, int deltaX, int deltaY, int deltaZ )
		{
			Point3D[] ignored;
			return ValidateMoveHard( actor, houses, map, deltaX, deltaY, deltaZ, out ignored );
		}

		internal static HousingValidationResult ValidateMoveHard( Mobile actor, IList<HouseFoundation> houses, Map map, int deltaX, int deltaY, int deltaZ )
		{
			List<BaseHouse> baseHouses = new List<BaseHouse>( houses == null ? 0 : houses.Count );

			if ( houses != null )
			{
				for ( int i = 0; i < houses.Count; ++i )
					baseHouses.Add( houses[i] );
			}

			return ValidateMoveHard( actor, baseHouses, map, deltaX, deltaY, deltaZ );
		}

		private static HousingValidationResult ValidateMoveHard( Mobile actor, IList<BaseHouse> houses, Map map, int deltaX, int deltaY, int deltaZ, out Point3D[] destinations )
		{
			destinations = null;

			if ( actor == null || actor.Deleted )
				return Invalid( "A live staff actor is required." );

			if ( actor.AccessLevel < AccessLevel.GameMaster )
				return Invalid( "This operation requires " + AccessLevel.GameMaster + " access." );

			if ( houses == null || houses.Count == 0 )
				return Invalid( "No houses were selected for movement." );

			if ( !IsValidWorldMap( map ) )
				return Invalid( "The destination map is invalid." );

			destinations = new Point3D[houses.Count];
			Rectangle2D[] plots = new Rectangle2D[houses.Count];
			Map sourceMap = null;

			for ( int i = 0; i < houses.Count; ++i )
			{
				BaseHouse house = houses[i];

				if ( house == null )
					return Invalid( "The movement selection contains a missing house." );

				if ( house.Deleted || World.FindItem( house.Serial ) != house )
					return Invalid( "House " + house.Serial + " is missing or deleted." );

				for ( int j = 0; j < i; ++j )
				{
					if ( houses[j] == house || houses[j].Serial == house.Serial )
						return Invalid( "House " + house.Serial + " was selected more than once." );
				}

				if ( !IsValidWorldMap( house.Map ) )
					return Invalid( "House " + house.Serial + " is on an invalid source map." );

				if ( i == 0 )
					sourceMap = house.Map;
				else if ( house.Map != sourceMap )
					return Invalid( "All selected houses must share one source map." );

				HouseFoundation foundation = house as HouseFoundation;

				if ( foundation != null && ( !foundation.IsActive || foundation.Customizer != null ) )
					return Invalid( "House " + house.Serial + " has an active customization session." );

				MultiComponentList components = house.Components;

				if ( components == null || components.Width <= 0 || components.Height <= 0 )
					return Invalid( "House " + house.Serial + " has invalid component dimensions." );

				long destinationX = (long) house.X + deltaX;
				long destinationY = (long) house.Y + deltaY;
				long destinationZ = (long) house.Z + deltaZ;

				if ( destinationX < Int32.MinValue || destinationX > Int32.MaxValue || destinationY < Int32.MinValue || destinationY > Int32.MaxValue || destinationZ < Int32.MinValue || destinationZ > Int32.MaxValue )
					return Invalid( "House " + house.Serial + " destination coordinates overflow." );

				if ( destinationZ < -128 || destinationZ > 127 )
					return Invalid( "House " + house.Serial + " destination Z must be between -128 and 127." );

				long plotX = destinationX + components.Min.X;
				long plotY = destinationY + components.Min.Y;
				long plotEndX = plotX + components.Width;
				long plotEndY = plotY + components.Height;

				if ( plotX < 0 || plotY < 0 || plotEndX > map.Width || plotEndY > map.Height || plotEndX <= plotX || plotEndY <= plotY )
					return Invalid( "House " + house.Serial + " destination components are outside the map bounds." );

				destinations[i] = new Point3D( (int) destinationX, (int) destinationY, (int) destinationZ );
				plots[i] = new Rectangle2D( (int) plotX, (int) plotY, components.Width, components.Height );

				for ( int j = 0; j < i; ++j )
				{
					if ( RectanglesOverlap( plots[i], plots[j] ) )
						return Invalid( "Two selected houses have overlapping movement destinations." );
				}
			}

			return HousingValidationResult.Valid;
		}

		public static bool IsAllowedElevationDelta( int deltaZ )
		{
			return deltaZ >= -100 && deltaZ <= 100 && deltaZ != 0;
		}

		internal static HousingValidationResult ValidateConnectedEstate( IList<HouseFoundation> houses )
		{
			if ( houses == null || houses.Count == 0 )
				return Invalid( "The estate has no live members." );

			Rectangle2D[] plots = new Rectangle2D[houses.Count];
			Map map = null;
			Mobile owner = null;

			for ( int i = 0; i < houses.Count; ++i )
			{
				HouseFoundation house = houses[i];

				if ( house == null || house.Deleted || World.FindItem( house.Serial ) != house )
					return Invalid( "The estate contains a missing or deleted member." );

				for ( int j = 0; j < i; ++j )
				{
					if ( houses[j] == house || houses[j].Serial == house.Serial )
						return Invalid( "The estate contains a duplicate member." );
				}

				if ( !IsValidWorldMap( house.Map ) )
					return Invalid( "Estate member " + house.Serial + " is on an invalid map." );

				if ( i == 0 )
					map = house.Map;
				else if ( house.Map != map )
					return Invalid( "Estate members are not on one map." );

				if ( house.Owner == null || house.Owner.Deleted )
					return Invalid( "Estate member " + house.Serial + " has no valid owner." );

				if ( i == 0 )
					owner = house.Owner;
				else if ( house.Owner != owner )
					return Invalid( "Estate members do not share one owner." );

				if ( !house.IsActive || house.Customizer != null )
					return Invalid( "Estate member " + house.Serial + " has an active customization session." );

				if ( house.Z < -128 || house.Z > 127 )
					return Invalid( "Estate member " + house.Serial + " has an invalid foundation Z." );

				MultiComponentList components = house.Components;

				if ( components == null || components.Width <= 0 || components.Height <= 0 )
					return Invalid( "Estate member " + house.Serial + " has invalid component dimensions." );

				long plotX = (long) house.X + components.Min.X;
				long plotY = (long) house.Y + components.Min.Y;
				long plotEndX = plotX + components.Width;
				long plotEndY = plotY + components.Height;

				if ( plotX < 0 || plotY < 0 || plotEndX > map.Width || plotEndY > map.Height || plotEndX <= plotX || plotEndY <= plotY )
					return Invalid( "Estate member " + house.Serial + " has components outside the map bounds." );

				plots[i] = new Rectangle2D( (int) plotX, (int) plotY, components.Width, components.Height );
			}

			bool[] visited = new bool[houses.Count];
			int[] queue = new int[houses.Count];
			int queueStart = 0;
			int queueEnd = 0;
			int visitedCount = 1;

			visited[0] = true;
			queue[queueEnd++] = 0;

			while ( queueStart < queueEnd )
			{
				int current = queue[queueStart++];

				for ( int i = 0; i < plots.Length; ++i )
				{
					if ( visited[i] || !HousingEstateGeometry.AreConnected( plots[current], plots[i], 1 ) )
						continue;

					visited[i] = true;
					queue[queueEnd++] = i;
					++visitedCount;
				}
			}

			return visitedCount == houses.Count ? HousingValidationResult.Valid : Invalid( "The moved house plots do not form one connected estate." );
		}

		private static HousingValidationResult ValidateActualComponents( Mobile actor, BaseHouse house, Map map, Point3D destination, HousingMoveEntitySet movingEntities )
		{
			MultiComponentList components = house.Components;

			for ( int x = 0; x < components.Width; ++x )
			{
				for ( int y = 0; y < components.Height; ++y )
				{
					StaticTile[] addTiles = components.Tiles[x][y];

					if ( addTiles == null || addTiles.Length == 0 )
						continue;

					int tileX = destination.X + components.Min.X + x;
					int tileY = destination.Y + components.Min.Y + y;
					Point3D testPoint = new Point3D( tileX, tileY, destination.Z );

					if ( !Region.Find( testPoint, map ).AllowHousing( actor, testPoint ) )
						return Invalid( "House " + house.Serial + " has a forbidden housing region in its destination footprint." );

					int landStartZ = 0;
					int landAverageZ = 0;
					int landTopZ = 0;
					map.GetAverageZ( tileX, tileY, ref landStartZ, ref landAverageZ, ref landTopZ );
					StaticTile[] staticTiles = map.Tiles.GetStaticTiles( tileX, tileY, true );
					Sector sector = map.GetSector( tileX, tileY );

					for ( int i = 0; i < addTiles.Length; ++i )
					{
						StaticTile addTile = addTiles[i];

						if ( addTile.ID == 0x1 )
							continue;

						ItemData addData = TileData.ItemTable[addTile.ID & TileData.MaxItemValue];
						int addZ = destination.Z + addTile.Z;
						int addTop = addZ + addTile.Height;

						if ( ( addData.Flags & TileFlag.Surface ) != 0 )
							addTop += 16;

						if ( addTop > landStartZ && landAverageZ > addZ )
							return Invalid( "House " + house.Serial + " destination collides with terrain." );

						for ( int j = 0; j < staticTiles.Length; ++j )
						{
							StaticTile staticTile = staticTiles[j];
							ItemData staticData = TileData.ItemTable[staticTile.ID & TileData.MaxItemValue];

							if ( IsBlocking( staticData ) && OverlapsZ( addZ, addTop, staticTile.Z, staticTile.Z + staticData.CalcHeight ) )
								return Invalid( "House " + house.Serial + " destination collides with a static tile." );
						}

						for ( int j = 0; j < sector.Items.Count; ++j )
						{
							Item item = sector.Items[j];

							if ( item == null || item.Deleted || !item.Visible || item.X != tileX || item.Y != tileY || movingEntities.Contains( item ) )
								continue;

							ItemData itemData = item.ItemData;

							if ( OverlapsZ( addZ, addTop, item.Z, item.Z + itemData.CalcHeight ) && ( item.Movable || IsBlocking( itemData ) ) )
								return Invalid( "House " + house.Serial + " destination collides with an item." );
						}

						for ( int j = 0; j < sector.Mobiles.Count; ++j )
						{
							Mobile mobile = sector.Mobiles[j];

							if ( mobile != null && !mobile.Deleted && mobile.X == tileX && mobile.Y == tileY && !movingEntities.Contains( mobile ) && OverlapsZ( addZ, addTop, mobile.Z, mobile.Z + 16 ) )
								return Invalid( "House " + house.Serial + " destination is occupied by a mobile." );
						}

						for ( int j = 0; j < sector.Multis.Count; ++j )
						{
							BaseMulti multi = sector.Multis[j];

							if ( multi == null || multi.Deleted || multi.Map != map || movingEntities.Contains( multi ) )
								continue;

							MultiComponentList otherComponents = multi.Components;
							int otherX = tileX - ( multi.X + otherComponents.Min.X );
							int otherY = tileY - ( multi.Y + otherComponents.Min.Y );

							if ( otherX < 0 || otherY < 0 || otherX >= otherComponents.Width || otherY >= otherComponents.Height )
								continue;

							StaticTile[] otherTiles = otherComponents.Tiles[otherX][otherY];

							for ( int k = 0; k < otherTiles.Length; ++k )
							{
								StaticTile otherTile = otherTiles[k];
								ItemData otherData = TileData.ItemTable[otherTile.ID & TileData.MaxItemValue];
								int otherZ = multi.Z + otherTile.Z;

								if ( IsBlocking( otherData ) && OverlapsZ( addZ, addTop, otherZ, otherZ + otherData.CalcHeight ) )
									return Invalid( "House " + house.Serial + " destination collides with a multi." );
							}
						}
					}
				}
			}

			return HousingValidationResult.Valid;
		}

		private static bool IsValidWorldMap( Map map )
		{
			if ( map == null || map == Map.Internal || map.Width <= 0 || map.Height <= 0 )
				return false;

			int index = map.MapIndex;
			return index >= 0 && index < Map.Maps.Length && Map.Maps[index] == map;
		}

		private static bool RectanglesOverlap( Rectangle2D left, Rectangle2D right )
		{
			return (long) left.X < (long) right.X + right.Width
				&& (long) right.X < (long) left.X + left.Width
				&& (long) left.Y < (long) right.Y + right.Height
				&& (long) right.Y < (long) left.Y + left.Height;
		}

		private static bool IsBlocking( ItemData data )
		{
			return data.Impassable || ( data.Surface && ( data.Flags & TileFlag.Background ) == 0 );
		}

		private static bool OverlapsZ( int leftBottom, int leftTop, int rightBottom, int rightTop )
		{
			return leftTop > rightBottom && rightTop > leftBottom;
		}

		public static HousingValidationResult ValidateCombine( IList<HouseFoundation> houses )
		{
			if ( houses == null )
				return Invalid( "No houses were selected." );

			if ( houses.Count < 2 )
				return Invalid( "Select at least two houses to combine." );

			Rectangle2D[] plots = new Rectangle2D[houses.Count];
			Map map = null;
			Mobile owner = null;

			for ( int i = 0; i < houses.Count; ++i )
			{
				HouseFoundation house = houses[i];

				if ( house == null )
					return Invalid( "The selection contains a missing house." );

				if ( house.Deleted )
					return Invalid( "House " + house.Serial + " is deleted." );

				for ( int j = 0; j < i; ++j )
				{
					if ( Object.ReferenceEquals( houses[j], house ) )
						return Invalid( "House " + house.Serial + " was selected more than once." );
				}

				if ( house.Map == null || house.Map == Map.Internal )
					return Invalid( "House " + house.Serial + " is on an invalid map." );

				if ( i == 0 )
					map = house.Map;
				else if ( house.Map != map )
					return Invalid( "House " + house.Serial + " is on a different map." );

				if ( house.Owner == null || house.Owner.Deleted )
					return Invalid( "House " + house.Serial + " has no valid owner." );

				if ( i == 0 )
					owner = house.Owner;
				else if ( house.Owner != owner )
					return Invalid( "House " + house.Serial + " has a different owner." );

				if ( !house.IsActive || house.Customizer != null )
					return Invalid( "House " + house.Serial + " has an active customization session." );

				HousingEstateController existing;

				if ( HousingEstateRegistry.TryGetEstate( house, out existing ) )
					return Invalid( "House " + house.Serial + " already belongs to an estate." );

				MultiComponentList components = house.Components;

				if ( components == null || components.Width <= 0 || components.Height <= 0 )
					return Invalid( "House " + house.Serial + " has invalid plot dimensions." );

				long plotX = (long) house.X + components.Min.X;
				long plotY = (long) house.Y + components.Min.Y;

				if ( plotX < Int32.MinValue || plotX > Int32.MaxValue || plotY < Int32.MinValue || plotY > Int32.MaxValue )
					return Invalid( "House " + house.Serial + " has invalid plot coordinates." );

				plots[i] = new Rectangle2D( (int) plotX, (int) plotY, components.Width, components.Height );
			}

			bool[] visited = new bool[houses.Count];
			int[] queue = new int[houses.Count];
			int queueStart = 0;
			int queueEnd = 0;
			int visitedCount = 1;

			visited[0] = true;
			queue[queueEnd++] = 0;

			while ( queueStart < queueEnd )
			{
				int current = queue[queueStart++];

				for ( int i = 0; i < plots.Length; ++i )
				{
					if ( visited[i] || !HousingEstateGeometry.AreConnected( plots[current], plots[i], 1 ) )
						continue;

					visited[i] = true;
					queue[queueEnd++] = i;
					++visitedCount;
				}
			}

			if ( visitedCount != houses.Count )
				return Invalid( "The selected house plots do not form one connected estate." );

			return HousingValidationResult.Valid;
		}

		public static HousingValidationResult ValidateSplit( HousingEstateController estate )
		{
			if ( estate == null )
				return Invalid( "The estate controller is missing." );

			if ( estate.Deleted )
				return Invalid( "The estate controller is deleted." );

			if ( estate.Health != HousingEstateHealth.Healthy )
				return Invalid( String.IsNullOrEmpty( estate.DegradedReason ) ? "The estate is degraded." : estate.DegradedReason );

			HousingValidationResult validation = estate.ValidateMembers();

			if ( !validation.IsValid )
			{
				estate.MarkDegraded( validation.Message );
				return validation;
			}

			for ( int i = 0; i < estate.MemberCount; ++i )
			{
				HouseFoundation member = estate.GetMember( i );
				HousingEstateController registered;

				if ( !HousingEstateRegistry.TryGetEstate( member, out registered ) || registered != estate )
				{
					string message = "Estate member " + member.Serial + " has invalid registry membership.";
					estate.MarkDegraded( message );
					return Invalid( message );
				}

				int fromSecures;
				int fromVendors;
				int fromLockdowns;
				int fromMovingCrate;
				int currentStorage = member.GetLocalAosCurSecures( out fromSecures, out fromVendors, out fromLockdowns, out fromMovingCrate );
				int maximumStorage = member.GetLocalAosMaxSecures();

				if ( currentStorage > maximumStorage )
					return OverCapacity( member, "storage", (long) currentStorage - maximumStorage );

				int currentLockdowns = member.GetLocalAosCurLockdowns();
				int maximumLockdowns = member.GetLocalAosMaxLockdowns();

				if ( currentLockdowns > maximumLockdowns )
					return OverCapacity( member, "lockdown", (long) currentLockdowns - maximumLockdowns );

				int currentVendors = member.GetLocalNewVendorSystemCurVendors();
				int maximumVendors = member.GetLocalNewVendorSystemMaxVendors();

				if ( currentVendors > maximumVendors )
					return OverCapacity( member, "vendor", (long) currentVendors - maximumVendors );
			}

			return HousingValidationResult.Valid;
		}

		private static HousingValidationResult OverCapacity( HouseFoundation house, string category, long overage )
		{
			return Invalid( "House " + house.Serial + " exceeds its local " + category + " capacity by " + overage + "." );
		}

		private static HousingValidationResult Invalid( string message )
		{
			return new HousingValidationResult( false, message );
		}
	}
}
