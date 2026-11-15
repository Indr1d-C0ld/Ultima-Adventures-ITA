using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Server.Diagnostics;

namespace Server.OneTime.Events
{
	public static class OneTimeEventHelper
	{
		private const long SlowLogThreshold = 50;
		private const long SlowLogInterval = 30000;
		private const int StatSlots = 16;

		private static readonly object m_SyncRoot = new object();
		private static readonly ReferenceComparer m_Comparer = new ReferenceComparer();

		private static Dictionary<int, List<IOneTime>> m_RegistryByType = new Dictionary<int, List<IOneTime>>();
		private static Dictionary<IOneTime, int> m_RegisteredTypes = new Dictionary<IOneTime, int>( m_Comparer );

		private static long[] m_LastElapsedByType = new long[StatSlots];
		private static long[] m_MaxElapsedByType = new long[StatSlots];
		private static int[] m_LastCallbacksByType = new int[StatSlots];
		private static int[] m_MaxCallbacksByType = new int[StatSlots];
		private static int[] m_LastRegisteredByType = new int[StatSlots];
		private static int[] m_LastStaleRemovedByType = new int[StatSlots];
		private static int[] m_LastRebucketedByType = new int[StatSlots];
		private static long[] m_NextSlowLogByType = new long[StatSlots];

		private static int m_TotalRegistered;
		private static int m_LastRebuildWorldItems;
		private static int m_LastRebuildWorldMobiles;
		private static int m_LastRebuildRegistered;
		private static long m_LastRebuildElapsed;
		private static DateTime m_LastRebuildTime;

		private sealed class ReferenceComparer : IEqualityComparer<IOneTime>
		{
			public bool Equals( IOneTime x, IOneTime y )
			{
				return Object.ReferenceEquals( x, y );
			}

			public int GetHashCode( IOneTime obj )
			{
				return RuntimeHelpers.GetHashCode( obj );
			}
		}

		public static void Initialize()
		{
			World.ItemAdded += OnItemAdded;
			World.ItemRemoved += OnItemRemoved;
			World.MobileAdded += OnMobileAdded;
			World.MobileRemoved += OnMobileRemoved;
		}

		private static void OnItemAdded( Item item )
		{
			IOneTime oneTime = item as IOneTime;

			if ( oneTime != null )
				Register( oneTime );
		}

		private static void OnItemRemoved( Item item )
		{
			IOneTime oneTime = item as IOneTime;

			if ( oneTime != null )
				Unregister( oneTime );
		}

		private static void OnMobileAdded( Mobile mobile )
		{
			IOneTime oneTime = mobile as IOneTime;

			if ( oneTime != null )
				Register( oneTime );
		}

		private static void OnMobileRemoved( Mobile mobile )
		{
			IOneTime oneTime = mobile as IOneTime;

			if ( oneTime != null )
				Unregister( oneTime );
		}

		private static long GetElapsedMilliseconds( long start )
		{
			return ( Stopwatch.GetTimestamp() - start ) * 1000 / Stopwatch.Frequency;
		}

		private static bool IsValidStatType( int type )
		{
			return type >= 0 && type < StatSlots;
		}

		private static string GetTypeName( int type )
		{
			switch ( type )
			{
				case 1: return "Tick";
				case 2: return "Millisecond";
				case 3: return "Second";
				case 4: return "Minute";
				case 5: return "Hour";
				case 6: return "Day";
			}

			return type.ToString();
		}

		public static void Register( IOneTime obj )
		{
			if ( obj == null )
				return;

			lock ( m_SyncRoot )
				RegisterLocked( obj, obj.OneTimeType );
		}

		public static void Unregister( IOneTime obj )
		{
			if ( obj == null )
				return;

			lock ( m_SyncRoot )
				UnregisterLocked( obj );
		}

		public static void UpdateRegistration( IOneTime obj )
		{
			Register( obj );
		}

		public static void Rebucket( IOneTime obj, int oldType, int newType )
		{
			if ( obj == null )
				return;

			lock ( m_SyncRoot )
			{
				RemoveFromBucketLocked( obj, oldType );
				m_RegisteredTypes[obj] = newType;
				AddToBucketLocked( obj, newType );
			}
		}

		public static void RebuildRegistry()
		{
			long start = Stopwatch.GetTimestamp();

			Dictionary<int, List<IOneTime>> registry = new Dictionary<int, List<IOneTime>>();
			Dictionary<IOneTime, int> registered = new Dictionary<IOneTime, int>( m_Comparer );

			int worldItems = World.Items.Count;
			int worldMobiles = World.Mobiles.Count;
			int count = 0;

			foreach ( Item item in World.Items.Values )
			{
				if ( item == null || item.Deleted )
					continue;

				IOneTime oneTime = item as IOneTime;

				if ( oneTime != null )
				{
					AddToRegistry( registry, registered, oneTime, oneTime.OneTimeType );
					++count;
				}
			}

			foreach ( Mobile mobile in World.Mobiles.Values )
			{
				if ( mobile == null || mobile.Deleted )
					continue;

				IOneTime oneTime = mobile as IOneTime;

				if ( oneTime != null )
				{
					AddToRegistry( registry, registered, oneTime, oneTime.OneTimeType );
					++count;
				}
			}

			lock ( m_SyncRoot )
			{
				m_RegistryByType = registry;
				m_RegisteredTypes = registered;
				m_TotalRegistered = registered.Count;
				m_LastRebuildWorldItems = worldItems;
				m_LastRebuildWorldMobiles = worldMobiles;
				m_LastRebuildRegistered = count;
				m_LastRebuildElapsed = GetElapsedMilliseconds( start );
				m_LastRebuildTime = DateTime.UtcNow;
			}

			Console.WriteLine( "OneTime registry rebuilt: registered={0}, worldItems={1}, worldMobiles={2}, elapsed={3}ms", count, worldItems, worldMobiles, m_LastRebuildElapsed );
		}

		private static void AddToRegistry( Dictionary<int, List<IOneTime>> registry, Dictionary<IOneTime, int> registered, IOneTime obj, int type )
		{
			if ( registered.ContainsKey( obj ) )
				return;

			registered[obj] = type;

			List<IOneTime> list;

			if ( !registry.TryGetValue( type, out list ) )
				registry[type] = list = new List<IOneTime>();

			list.Add( obj );
		}

		private static void RegisterLocked( IOneTime obj, int type )
		{
			int oldType;

			if ( m_RegisteredTypes.TryGetValue( obj, out oldType ) )
			{
				if ( oldType == type )
					return;

				RemoveFromBucketLocked( obj, oldType );
				m_RegisteredTypes[obj] = type;
				AddToBucketLocked( obj, type );
				return;
			}

			m_RegisteredTypes[obj] = type;
			AddToBucketLocked( obj, type );
			++m_TotalRegistered;
		}

		private static void UnregisterLocked( IOneTime obj )
		{
			int oldType;

			if ( !m_RegisteredTypes.TryGetValue( obj, out oldType ) )
				return;

			m_RegisteredTypes.Remove( obj );
			RemoveFromBucketLocked( obj, oldType );

			if ( m_TotalRegistered > 0 )
				--m_TotalRegistered;
		}

		private static void AddToBucketLocked( IOneTime obj, int type )
		{
			List<IOneTime> list;

			if ( !m_RegistryByType.TryGetValue( type, out list ) )
				m_RegistryByType[type] = list = new List<IOneTime>();

			list.Add( obj );
		}

		private static void RemoveFromBucketLocked( IOneTime obj, int type )
		{
			List<IOneTime> list;

			if ( !m_RegistryByType.TryGetValue( type, out list ) )
				return;

			for ( int i = list.Count - 1; i >= 0; --i )
			{
				if ( Object.ReferenceEquals( list[i], obj ) )
				{
					list.RemoveAt( i );
					break;
				}
			}
		}

		private static bool IsDeleted( IOneTime obj )
		{
			Item item = obj as Item;

			if ( item != null )
				return item.Deleted;

			Mobile mobile = obj as Mobile;

			if ( mobile != null )
				return mobile.Deleted;

			return false;
		}

		private static bool IsInternalMap( IOneTime obj )
		{
			Item item = obj as Item;

			if ( item != null )
				return item.Map == Map.Internal;

			Mobile mobile = obj as Mobile;

			if ( mobile != null )
				return mobile.Map == Map.Internal;

			return false;
		}

		private static void UpdateStats( int type, long elapsed, int registered, int callbacks, int staleRemoved, int rebucketed )
		{
			if ( !IsValidStatType( type ) )
				return;

			m_LastElapsedByType[type] = elapsed;
			m_LastRegisteredByType[type] = registered;
			m_LastCallbacksByType[type] = callbacks;
			m_LastStaleRemovedByType[type] = staleRemoved;
			m_LastRebucketedByType[type] = rebucketed;

			if ( elapsed > m_MaxElapsedByType[type] )
				m_MaxElapsedByType[type] = elapsed;

			if ( callbacks > m_MaxCallbacksByType[type] )
				m_MaxCallbacksByType[type] = callbacks;
		}

		public static void SendIOneTime( int type )
		{
			long start = Stopwatch.GetTimestamp();
			int callbacks = 0;
			int staleRemoved = 0;
			int rebucketed = 0;
			int registered = 0;
			int totalRegistered;
			List<IOneTime> snapshot = null;

			lock ( m_SyncRoot )
			{
				List<IOneTime> bucket;

				if ( m_RegistryByType.TryGetValue( type, out bucket ) )
				{
					registered = bucket.Count;
					snapshot = new List<IOneTime>( bucket );
				}

				totalRegistered = m_TotalRegistered;
			}

			if ( snapshot != null )
			{
				for ( int i = 0; i < snapshot.Count; ++i )
				{
					IOneTime oneTime = snapshot[i];

					if ( oneTime == null || IsDeleted( oneTime ) )
					{
						Unregister( oneTime );
						++staleRemoved;
						continue;
					}

					int currentType = oneTime.OneTimeType;

					if ( currentType != type )
					{
						Rebucket( oneTime, type, currentType );
						++rebucketed;
						continue;
					}

					if ( IsInternalMap( oneTime ) )
						continue;

					oneTime.OneTimeTick();
					++callbacks;
				}
			}

			long elapsed = GetElapsedMilliseconds( start );
			UpdateStats( type, elapsed, registered, callbacks, staleRemoved, rebucketed );

			if ( elapsed >= SlowLogThreshold )
			{
				long now = Core.TickCount;

				if ( !IsValidStatType( type ) || now >= m_NextSlowLogByType[type] )
				{
					if ( IsValidStatType( type ) )
						m_NextSlowLogByType[type] = now + SlowLogInterval;

					DiagnosticConsole.WriteParsedLine( "[Lag] OneTime SendIOneTime type={0}({1}) took {2}ms; registered={3}, callbacks={4}, staleRemoved={5}, rebucketed={6}, totalRegistered={7}", GetTypeName( type ), type, elapsed, registered, callbacks, staleRemoved, rebucketed, totalRegistered );
				}
			}
		}

		public static void WriteStats( TextWriter writer )
		{
			if ( writer == null )
				return;

			lock ( m_SyncRoot )
			{
				writer.WriteLine( "OneTime registry: totalRegistered={0}, lastRebuildRegistered={1}, worldItems={2}, worldMobiles={3}, rebuildElapsed={4}ms, rebuildUtc={5:u}", m_TotalRegistered, m_LastRebuildRegistered, m_LastRebuildWorldItems, m_LastRebuildWorldMobiles, m_LastRebuildElapsed, m_LastRebuildTime );

				for ( int i = 1; i <= 6; ++i )
				{
					int bucketCount = 0;
					List<IOneTime> bucket;

					if ( m_RegistryByType.TryGetValue( i, out bucket ) )
						bucketCount = bucket.Count;

					writer.WriteLine( "{0}({1}): registered={2}, lastRegistered={3}, last={4}ms max={5}ms callbacks={6}/{7} staleRemoved={8} rebucketed={9}",
						GetTypeName( i ),
						i,
						bucketCount,
						m_LastRegisteredByType[i],
						m_LastElapsedByType[i],
						m_MaxElapsedByType[i],
						m_LastCallbacksByType[i],
						m_MaxCallbacksByType[i],
						m_LastStaleRemovedByType[i],
						m_LastRebucketedByType[i] );
				}
			}
		}
	}
}
