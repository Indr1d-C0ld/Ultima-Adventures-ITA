using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using Server.Commands;
using Server.Diagnostics;

namespace Server.Regions
{
	public class SpawnEntry : ISpawner
	{
		public static readonly TimeSpan DefaultMinSpawnTime = TimeSpan.FromMinutes( 2.0 );
		public static readonly TimeSpan DefaultMaxSpawnTime = TimeSpan.FromMinutes( 5.0 );

		private static Hashtable m_Table = new Hashtable();

		public static Hashtable Table{ get{ return m_Table; } }


		// When a creature's AI is deactivated (PlayerRangeSensitive optimization) does it return home?
		public bool ReturnOnDeactivate{ get{ return true; } }

		// Are creatures unlinked on taming (true) or should they also go out of the region (false)?
		public bool UnlinkOnTaming{ get{ return false; } }

		// Are unlinked and untamed creatures removed after 20 hours?
		public bool RemoveIfUntamed{ get{ return true; } }


		public static readonly Direction InvalidDirection = Direction.Running;

		private int m_ID;
		private BaseRegion m_Region;
		private Point3D m_Home;
		private int m_Range;
		private Direction m_Direction;
		private SpawnDefinition m_Definition;
		private List<ISpawnable> m_SpawnedObjects;
		private int m_Max;
		private TimeSpan m_MinSpawnTime;
		private TimeSpan m_MaxSpawnTime;
		private bool m_Running;

		private DateTime m_NextSpawn;
		private Timer m_SpawnTimer;

		public int ID{ get{ return m_ID; } }
		public BaseRegion Region{ get{ return m_Region; } }
		public Point3D HomeLocation{ get{ return m_Home; } }
		public int HomeRange{ get{ return m_Range; } }
		public Direction Direction{ get{ return m_Direction; } }
		public SpawnDefinition Definition{ get{ return m_Definition; } }
		public List<ISpawnable> SpawnedObjects{ get{ return m_SpawnedObjects; } }
		public int Max{ get{ return m_Max; } }
		public TimeSpan MinSpawnTime{ get{ return m_MinSpawnTime; } }
		public TimeSpan MaxSpawnTime{ get{ return m_MaxSpawnTime; } }
		public bool Running{ get{ return m_Running; } }
		public string DiagnosticsEntryKey{ get{ return GetDiagnosticsEntryKey(); } }
		public string DiagnosticsRegionKey{ get{ return GetDiagnosticsRegionKey(); } }
		public string DiagnosticsDefinitionKey{ get{ return GetDiagnosticsDefinitionKey(); } }
		public string DiagnosticsEntryDescription{ get{ return GetDiagnosticsEntryDescription(); } }
		public string DiagnosticsRegionDescription{ get{ return GetDiagnosticsRegionDescription(); } }
		public string DiagnosticsDefinitionDescription{ get{ return GetDiagnosticsDefinitionDescription(); } }

		public bool Complete{ get{ return m_SpawnedObjects.Count >= m_Max; } }
		public bool Spawning{ get{ return m_Running && !this.Complete; } }

		public SpawnEntry( int id, BaseRegion region, Point3D home, int range, Direction direction, SpawnDefinition definition, int max, TimeSpan minSpawnTime, TimeSpan maxSpawnTime )
		{
			m_ID = id;
			m_Region = region;
			m_Home = home;
			m_Range = range;
			m_Direction = direction;
			m_Definition = definition;
			m_SpawnedObjects = new List<ISpawnable>();
			m_Max = max;
			m_MinSpawnTime = minSpawnTime;
			m_MaxSpawnTime = maxSpawnTime;
			m_Running = false;

			if ( m_Table.Contains( id ) )
				Console.WriteLine( "Warning: double SpawnEntry ID '{0}'", id );
			else
				m_Table[id] = this;
		}

		public Point3D RandomSpawnLocation( int spawnHeight, bool land, bool water )
		{
			return m_Region.RandomSpawnLocation( spawnHeight, land, water, m_Home, m_Range );
		}

		public void Start()
		{
			if ( m_Running )
				return;

			m_Running = true;
			CheckTimer( SpawnEntryDiagnostics.ScheduleReason.Start, null, 0, m_SpawnedObjects.Count, m_SpawnedObjects.Count );
		}

		public void Stop()
		{
			if ( !m_Running )
				return;

			m_Running = false;
			CheckTimer( SpawnEntryDiagnostics.ScheduleReason.Other, null, 0, m_SpawnedObjects.Count, m_SpawnedObjects.Count );
		}

		private void Spawn()
		{
			ISpawnable spawn = m_Definition.Spawn(this);

			if ( spawn != null )
				Add( spawn );
		}

		private void Add( ISpawnable spawn )
		{
			m_SpawnedObjects.Add( spawn );

			spawn.Spawner = this;

			if ( spawn is BaseCreature )
				((BaseCreature)spawn).RemoveIfUntamed = this.RemoveIfUntamed;
		}

		void ISpawner.Remove( ISpawnable spawn )
		{
			int beforeCount = m_SpawnedObjects.Count;
			string removedType = spawn != null ? spawn.GetType().FullName : null;
			int removedSerial = 0;

			if ( spawn != null )
				removedSerial = spawn.Serial;

			m_SpawnedObjects.Remove( spawn );

			CheckTimer( SpawnEntryDiagnostics.ScheduleReason.SpawnedObjectRemoved, removedType, removedSerial, beforeCount, m_SpawnedObjects.Count );
		}

		private TimeSpan RandomTime()
		{
			int min = (int) m_MinSpawnTime.TotalSeconds;
			int max = (int) m_MaxSpawnTime.TotalSeconds;

			int rand = Utility.RandomMinMax( min, max );
			return TimeSpan.FromSeconds( rand );
		}

		private void CheckTimer()
		{
			CheckTimer( SpawnEntryDiagnostics.ScheduleReason.Other, null, 0, m_SpawnedObjects.Count, m_SpawnedObjects.Count );
		}

		private void CheckTimer( SpawnEntryDiagnostics.ScheduleReason reason, string removedTargetType, int removedSerial, int beforeSpawnedCount, int afterSpawnedCount )
		{
			if ( this.Spawning )
			{
				if ( m_SpawnTimer == null )
				{
					TimeSpan time = RandomTime();
					m_SpawnTimer = Timer.DelayCall( time, new TimerCallback( TimerCallback ) );
					m_NextSpawn = DateTime.UtcNow + time;
					SpawnEntryDiagnostics.RecordSchedule( GetDiagnosticsEntryKey(), GetDiagnosticsEntryDescription(), GetDiagnosticsRegionKey(), GetDiagnosticsRegionDescription(), GetDiagnosticsEntryType(), GetDiagnosticsDefinitionKey(), GetDiagnosticsDefinitionDescription(), (long)time.TotalMilliseconds, reason, removedTargetType, removedSerial, false, true, beforeSpawnedCount, afterSpawnedCount, m_Max );
				}
			}
			else if ( m_SpawnTimer != null )
			{
				m_SpawnTimer.Stop();
				m_SpawnTimer = null;
			}
		}

		private void TimerCallback()
		{
			long diagnosticsStart = SpawnEntryDiagnostics.GetTimestamp();
			int beforeSpawnedCount = m_SpawnedObjects.Count;
			int requestedAmount = -1;
			int afterSpawnedCount = beforeSpawnedCount;
			bool callbackRescheduled = false;
			long nextDelayMs = -1;

			try
			{
				int amount = Math.Max( (m_Max - m_SpawnedObjects.Count) / 3, 1 );
				requestedAmount = amount;

				for ( int i = 0; i < amount; i++ )
					Spawn();

				m_SpawnTimer = null;
				afterSpawnedCount = m_SpawnedObjects.Count;
				CheckTimer( SpawnEntryDiagnostics.ScheduleReason.TimerCallbackReschedule, null, 0, beforeSpawnedCount, afterSpawnedCount );
				callbackRescheduled = m_SpawnTimer != null;

				if ( callbackRescheduled )
				{
					nextDelayMs = (long)( m_NextSpawn - DateTime.UtcNow ).TotalMilliseconds;

					if ( nextDelayMs < 0 )
						nextDelayMs = 0;
				}
			}
			finally
			{
				afterSpawnedCount = m_SpawnedObjects.Count;
				SpawnEntryDiagnostics.RecordCallback( GetDiagnosticsEntryKey(), GetDiagnosticsEntryDescription(), GetDiagnosticsRegionKey(), GetDiagnosticsRegionDescription(), GetDiagnosticsEntryType(), GetDiagnosticsDefinitionKey(), GetDiagnosticsDefinitionDescription(), beforeSpawnedCount, requestedAmount, afterSpawnedCount, afterSpawnedCount - beforeSpawnedCount, afterSpawnedCount, m_Max, m_Running, this.Spawning, callbackRescheduled, nextDelayMs, SpawnEntryDiagnostics.GetElapsedMilliseconds( diagnosticsStart ) );
			}
		}

		private string GetDiagnosticsEntryKey()
		{
			return "SpawnEntry#" + m_ID.ToString();
		}

		private string GetDiagnosticsEntryDescription()
		{
			return String.Format( "id={0} region={1} map={2} home={3} range={4} spawned={5}/{6}",
				m_ID,
				GetRegionName(),
				GetMapName(),
				m_Home,
				m_Range,
				m_SpawnedObjects != null ? m_SpawnedObjects.Count : 0,
				m_Max );
		}

		private string GetDiagnosticsRegionKey()
		{
			return String.Format( "{0}:{1}:{2}", GetRegionType(), GetRegionName(), GetMapName() );
		}

		private string GetDiagnosticsRegionDescription()
		{
			return String.Format( "{0} name={1} map={2}", GetRegionType(), GetRegionName(), GetMapName() );
		}

		private string GetDiagnosticsEntryType()
		{
			return GetType().FullName;
		}

		private string GetDiagnosticsDefinitionKey()
		{
			if ( m_Definition == null )
				return "definition=null";

			SpawnTreasureChest chest = m_Definition as SpawnTreasureChest;

			if ( chest != null )
				return String.Format( "treasureChest:{0}:{1}", chest.ItemID, chest.Level );

			SpawnGroup group = m_Definition as SpawnGroup;

			if ( group != null )
				return "group:" + group.Name;

			SpawnType spawnType = m_Definition as SpawnType;

			if ( spawnType != null && spawnType.Type != null )
				return "type:" + spawnType.Type.FullName;

			return "definitionType:" + m_Definition.GetType().FullName;
		}

		private string GetDiagnosticsDefinitionDescription()
		{
			if ( m_Definition == null )
				return "definition=null";

			SpawnTreasureChest chest = m_Definition as SpawnTreasureChest;

			if ( chest != null )
				return String.Format( "treasureChest itemID={0} level={1}", chest.ItemID, chest.Level );

			SpawnGroup group = m_Definition as SpawnGroup;

			if ( group != null )
				return "group name=" + group.Name;

			SpawnType spawnType = m_Definition as SpawnType;

			if ( spawnType != null && spawnType.Type != null )
				return String.Format( "{0} type={1}", m_Definition.GetType().FullName, spawnType.Type.FullName );

			return m_Definition.GetType().FullName;
		}

		private string GetRegionName()
		{
			return m_Region != null && m_Region.Name != null ? m_Region.Name : "null";
		}

		private string GetRegionType()
		{
			return m_Region != null ? m_Region.GetType().FullName : "null";
		}

		private string GetMapName()
		{
			return m_Region != null && m_Region.Map != null ? m_Region.Map.ToString() : "null";
		}

		public void DeleteSpawnedObjects()
		{
			InternalDeleteSpawnedObjects();

			m_Running = false;
			CheckTimer( SpawnEntryDiagnostics.ScheduleReason.Other, null, 0, m_SpawnedObjects.Count, m_SpawnedObjects.Count );
		}

		private void InternalDeleteSpawnedObjects()
		{
			foreach ( ISpawnable spawnable in m_SpawnedObjects )
			{
				spawnable.Spawner = null;

				bool uncontrolled = !(spawnable is BaseCreature) || !((BaseCreature)spawnable).Controlled;

				if( uncontrolled )
					spawnable.Delete();
			}

			m_SpawnedObjects.Clear();
		}

		public void Respawn()
		{
			InternalDeleteSpawnedObjects();

			for ( int i = 0; !this.Complete && i < m_Max; i++ )
				Spawn();

			m_Running = true;
			CheckTimer( SpawnEntryDiagnostics.ScheduleReason.Respawn, null, 0, m_SpawnedObjects.Count, m_SpawnedObjects.Count );
		}

		public void Delete()
		{
			m_Max = 0;
			InternalDeleteSpawnedObjects();

			if ( m_SpawnTimer != null )
			{
				m_SpawnTimer.Stop();
				m_SpawnTimer = null;
			}

			if ( m_Table[m_ID] == this )
				m_Table.Remove( m_ID );
		}

		public void Serialize( GenericWriter writer )
		{
			writer.Write( (int) m_SpawnedObjects.Count );

			for ( int i = 0; i < m_SpawnedObjects.Count; i++ )
			{
				ISpawnable spawn = m_SpawnedObjects[i];

				int serial = spawn.Serial;

				writer.Write( (int) serial );
			}

			writer.Write( (bool) m_Running );

			if ( m_SpawnTimer != null )
			{
				writer.Write( true );
				writer.WriteDeltaTime( (DateTime) m_NextSpawn );
			}
			else
			{
				writer.Write( false );
			}
		}

		public void Deserialize( GenericReader reader, int version )
		{
			int count = reader.ReadInt();

			for ( int i = 0; i < count; i++ )
			{
				int serial = reader.ReadInt();
				ISpawnable spawnableEntity = World.FindEntity( serial ) as ISpawnable;

				if (spawnableEntity != null)
					Add(spawnableEntity);
			}

			m_Running = reader.ReadBool();

			if ( reader.ReadBool() )
			{
				m_NextSpawn = reader.ReadDeltaTime();

				if ( this.Spawning )
				{
					if ( m_SpawnTimer != null )
						m_SpawnTimer.Stop();

					TimeSpan delay = m_NextSpawn - DateTime.UtcNow;
					TimeSpan actualDelay = delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
					m_SpawnTimer = Timer.DelayCall( actualDelay, new TimerCallback( TimerCallback ) ); 
					SpawnEntryDiagnostics.RecordSchedule( GetDiagnosticsEntryKey(), GetDiagnosticsEntryDescription(), GetDiagnosticsRegionKey(), GetDiagnosticsRegionDescription(), GetDiagnosticsEntryType(), GetDiagnosticsDefinitionKey(), GetDiagnosticsDefinitionDescription(), (long)actualDelay.TotalMilliseconds, SpawnEntryDiagnostics.ScheduleReason.DeserializeRestore, null, 0, false, true, m_SpawnedObjects.Count, m_SpawnedObjects.Count, m_Max );
				}
			}

			CheckTimer();
		}

		private static List<IEntity> m_RemoveList;

		public static void Remove( GenericReader reader, int version )
		{
			int count = reader.ReadInt();

			for ( int i = 0; i < count; i++ )
			{
				int serial = reader.ReadInt();
				IEntity entity = World.FindEntity( serial );

				if ( entity != null )
				{
					if ( m_RemoveList == null )
						m_RemoveList = new List<IEntity>();

					m_RemoveList.Add( entity );
				}
			}

			reader.ReadBool(); // m_Running

			if ( reader.ReadBool() )
				reader.ReadDeltaTime(); // m_NextSpawn
		}

		public static void Initialize()
		{
			if ( m_RemoveList != null )
			{
				foreach ( IEntity ent in m_RemoveList )
				{
					ent.Delete();
				}

				m_RemoveList = null;
			}

			SpawnPersistence.EnsureExistence();

			CommandSystem.Register( "RespawnAllRegions", AccessLevel.Administrator, new CommandEventHandler( RespawnAllRegions_OnCommand ) );
			CommandSystem.Register( "RespawnRegion", AccessLevel.GameMaster, new CommandEventHandler( RespawnRegion_OnCommand ) );
			CommandSystem.Register( "DelAllRegionSpawns", AccessLevel.Administrator, new CommandEventHandler( DelAllRegionSpawns_OnCommand ) );
			CommandSystem.Register( "DelRegionSpawns", AccessLevel.GameMaster, new CommandEventHandler( DelRegionSpawns_OnCommand ) );
			CommandSystem.Register( "StartAllRegionSpawns", AccessLevel.Administrator, new CommandEventHandler( StartAllRegionSpawns_OnCommand ) );
			CommandSystem.Register( "StartRegionSpawns", AccessLevel.GameMaster, new CommandEventHandler( StartRegionSpawns_OnCommand ) );
			CommandSystem.Register( "StopAllRegionSpawns", AccessLevel.Administrator, new CommandEventHandler( StopAllRegionSpawns_OnCommand ) );
			CommandSystem.Register( "StopRegionSpawns", AccessLevel.GameMaster, new CommandEventHandler( StopRegionSpawns_OnCommand ) );
		}

		private static BaseRegion GetCommandData( CommandEventArgs args )
		{
			Mobile from = args.Mobile;

			Region reg;
			if ( args.Length == 0 )
			{
				reg = from.Region;
			}
			else
			{
				string name = args.GetString( 0 );
				//reg = (Region) from.Map.Regions[name];

				if ( !from.Map.Regions.TryGetValue( name, out reg ) )
				{
					from.SendMessage( "Could not find region '{0}'.", name );
					return null;
				}
			}

			BaseRegion br = reg as BaseRegion;

			if ( br == null || br.Spawns == null )
			{
				from.SendMessage( "There are no spawners in region '{0}'.", reg );
				return null;
			}

			return br;
		}

		[Usage( "RespawnAllRegions" )]
		[Description( "Respawns all regions and sets the spawners as running." )]
		public static void RespawnAllRegions_OnCommand( CommandEventArgs args )
		{
			foreach ( SpawnEntry entry in m_Table.Values )
			{
				entry.Respawn();
			}

			args.Mobile.SendMessage( "All regions have respawned." );
		}

		[Usage( "RespawnRegion [<region name>]" )]
		[Description( "Respawns the region in which you are (or that you provided) and sets the spawners as running." )]
		private static void RespawnRegion_OnCommand( CommandEventArgs args )
		{
			BaseRegion region = GetCommandData( args );

			if ( region == null )
				return;

			for ( int i = 0; i < region.Spawns.Length; i++ )
				region.Spawns[i].Respawn();

			args.Mobile.SendMessage( "Region '{0}' has respawned.", region );
		}

		[Usage( "DelAllRegionSpawns" )]
		[Description( "Deletes all spawned objects of every regions and sets the spawners as not running." )]
		private static void DelAllRegionSpawns_OnCommand( CommandEventArgs args )
		{
			foreach ( SpawnEntry entry in m_Table.Values )
			{
				entry.DeleteSpawnedObjects();
			}

			args.Mobile.SendMessage( "All region spawned objects have been deleted." );
		}

		[Usage( "DelRegionSpawns [<region name>]" )]
		[Description( "Deletes all spawned objects of the region in which you are (or that you provided) and sets the spawners as not running." )]
		private static void DelRegionSpawns_OnCommand( CommandEventArgs args )
		{
			BaseRegion region = GetCommandData( args );

			if ( region == null )
				return;

			for ( int i = 0; i < region.Spawns.Length; i++ )
				region.Spawns[i].DeleteSpawnedObjects();

			args.Mobile.SendMessage( "Spawned objects of region '{0}' have been deleted.", region );
		}

		[Usage( "StartAllRegionSpawns" )]
		[Description( "Sets the region spawners of all regions as running." )]
		private static void StartAllRegionSpawns_OnCommand( CommandEventArgs args )
		{
			foreach ( SpawnEntry entry in m_Table.Values )
			{
				entry.Start();
			}

			args.Mobile.SendMessage( "All region spawners have started." );
		}

		[Usage( "StartRegionSpawns [<region name>]" )]
		[Description( "Sets the region spawners of the region in which you are (or that you provided) as running." )]
		private static void StartRegionSpawns_OnCommand( CommandEventArgs args )
		{
			BaseRegion region = GetCommandData( args );

			if ( region == null )
				return;

			for ( int i = 0; i < region.Spawns.Length; i++ )
				region.Spawns[i].Start();

			args.Mobile.SendMessage( "Spawners of region '{0}' have started.", region );
		}

		[Usage( "StopAllRegionSpawns" )]
		[Description( "Sets the region spawners of all regions as not running." )]
		private static void StopAllRegionSpawns_OnCommand( CommandEventArgs args )
		{
			foreach ( SpawnEntry entry in m_Table.Values )
			{
				entry.Stop();
			}

			args.Mobile.SendMessage( "All region spawners have stopped." );
		}

		[Usage( "StopRegionSpawns [<region name>]" )]
		[Description( "Sets the region spawners of the region in which you are (or that you provided) as not running." )]
		private static void StopRegionSpawns_OnCommand( CommandEventArgs args )
		{
			BaseRegion region = GetCommandData( args );

			if ( region == null )
				return;

			for ( int i = 0; i < region.Spawns.Length; i++ )
				region.Spawns[i].Stop();

			args.Mobile.SendMessage( "Spawners of region '{0}' have stopped.", region );
		}
	}
}
