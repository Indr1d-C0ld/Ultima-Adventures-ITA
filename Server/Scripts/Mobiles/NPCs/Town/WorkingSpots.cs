using System;
using Server;
using Server.Mobiles;
using Server.Misc;
using System.Collections.Generic;
using System.Collections;
using Server.Diagnostics;
using Server.Regions;

namespace Server.Items
{
	public class WorkingSpots : Item
	{
		private static readonly TimeSpan PopulationSliceDelay = TimeSpan.FromMilliseconds( 25.0 );
		private const long PopulationSliceBudgetMs = 25;
		private static DateTime m_NextSpawnErrorLog;
		private static int m_SpawnErrorSuppressed;
		private static PopulationRun m_ActivePopulation;

		private enum PopulationStage
		{
			ScanWorkingSpots,
			CleanupSpots,
			SelectCandidates,
			SetupSpawn,
			Complete
		}

		private sealed class PopulationRun
		{
			public PopulationStage Stage;
			public ArrayList Spots;
			public ArrayList Candidates;
			public int Index;
			public int WorldItemsScanned;
			public TaskManagerDiagnostics.Context Diagnostics;
			public Action<Exception> Completion;
			public long PreparationMs;
			public long SetupWorkMs;
			public long MaxChunkMs;
			public int ChunkCount;
			public long WallStart;
		}

		private sealed class PopulationTimer : Timer
		{
			private readonly PopulationRun m_Run;
			private readonly string m_Label;

			public PopulationTimer( PopulationRun run ) : base( PopulationSliceDelay )
			{
				m_Run = run;
				m_Label = GetPopulationStageName( run != null ? run.Stage : PopulationStage.Complete );
				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				long callbackStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
				string callbackKind = m_Label;

				ProcessPopulationSlice( m_Run );

				long callbackElapsed = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( callbackStart );
				TaskManagerDailyAttributionDiagnostics.RecordVillageChunk( callbackElapsed );
				TaskManagerDailyAttributionDiagnostics.RecordStagedCallback( ToString(), callbackKind, callbackElapsed );
			}

			public override string ToString()
			{
				return GetType().FullName + " / TaskManagerDaily populateVillages:" + m_Label;
			}
		}

		public static bool PopulationInProgress
		{
			get { return m_ActivePopulation != null; }
		}

		[Constructable]
		public WorkingSpots() : base( 0x1B7B )
		{
			Name = "working spot";
			Visible = false;
			Movable = false;
		}

		public static void PopulateVillages()
		{
			PopulateVillages( null );
		}

		public static void PopulateVillages( TaskManagerDiagnostics.Context diag )
		{
			long populateStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			try
			{
				ArrayList candidates = PreparePopulation( diag );
				SpawnCandidatesSynchronously( candidates, diag );
			}
			catch ( Exception ex )
			{
				TaskManagerDailyAttributionDiagnostics.RecordError( "PopulateVillages", ex );
				throw;
			}
			finally
			{
				TaskManagerDailyAttributionDiagnostics.RecordGroup( "PopulateVillages", populateStart );
			}
		}

		public static bool BeginPopulateVillages( TaskManagerDiagnostics.Context diag, Action<Exception> completion )
		{
			if ( m_ActivePopulation != null )
				return false;

			PopulationRun run = new PopulationRun();
			run.Stage = PopulationStage.ScanWorkingSpots;
			run.Spots = new ArrayList();
			run.Candidates = new ArrayList();
			run.Diagnostics = diag;
			run.Completion = completion;
			run.WallStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			m_ActivePopulation = run;

			TaskManagerDailyAttributionDiagnostics.MarkStaged();
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "staged", 1 );
			TaskManagerDiagnostics.MarkChunkCandidate( diag, "active Daily-only ordered village preparation and setupSpawn work pump; one setup spawn per callback" );

			try
			{
				PopulationTimer timer = new PopulationTimer( run );
				timer.Start();
			}
			catch
			{
				m_ActivePopulation = null;
				throw;
			}

			return true;
		}

		private static ArrayList PreparePopulation( TaskManagerDiagnostics.Context diag )
		{
			ArrayList spots = new ArrayList();
			int itemsScanned = 0;
			TaskManagerDiagnostics.StartPhase( diag, "populateVillages:scanWorkingSpots" );
			long scanStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();

			foreach ( Item item in World.Items.Values )
			{
				++itemsScanned;

				if ( item is WorkingSpots )
					spots.Add( item );
			}

			TaskManagerDailyAttributionDiagnostics.RecordSubphase( "PopulateVillages", "scanWorkingSpots", scanStart );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "worldItemsScanned", itemsScanned );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "workingSpotsFound", spots.Count );
			TaskManagerDiagnostics.AddScanned( diag, itemsScanned );
			TaskManagerDiagnostics.MarkRegistryCandidate( diag, "WorkingSpots full item scan" );

			TaskManagerDiagnostics.StartPhase( diag, "populateVillages:cleanupSpots" );
			TaskManagerDiagnostics.AddScanned( diag, spots.Count );
			long cleanupStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();

			for ( int i = 0; i < spots.Count; ++i )
				Cleanup( (Item)spots[i], diag );

			TaskManagerDailyAttributionDiagnostics.RecordSubphase( "PopulateVillages", "cleanupSpots", cleanupStart );
			return SelectSetupCandidates( spots, diag, itemsScanned, true );
		}

		public static int Cleanup( Item spot )
		{
			return Cleanup( spot, null );
		}

		public static int Cleanup( Item spot, TaskManagerDiagnostics.Context diag )
		{
			ArrayList clean = new ArrayList();
			int itemDeletes = 0;
			int mobileDeletes = 0;
			foreach ( Item item in spot.GetItemsInRange( 0 ) )
			{
				if ( !(item is WorkingSpots) && item.Location == spot.Location )
				{
					clean.Add( item );
				}
			}
			foreach ( Mobile mobile in spot.GetMobilesInRange( 0 ) )
			{
				if ( mobile.Location == spot.Location && (
					mobile is SpellCritter || 
					mobile is TradesmanAlchemist || 
					mobile is TradesmanLeather || 
					mobile is TradesmanLogger || 
					mobile is TradesmanLumber || 
					mobile is TradesmanMiner || 
					mobile is TradesmanSmelter || 
					mobile is TradesmanCook || 
					mobile is TradesmanSmith || 
					mobile is TrainingBow || 
					mobile is TrainingFishing || 
					mobile is TrainingMagery || 
					mobile is TrainingSingle || 
					mobile is Warriors ) )
						clean.Add( mobile );
			}
			for ( int i = 0; i < clean.Count; ++i )
			{
				if ( clean[i] is Item ){ ((Item)clean[i]).Delete(); ++itemDeletes; }
				else if ( clean[i] is Mobile ){ ((Mobile)clean[i]).Delete(); ++mobileDeletes; }
			}
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "itemsDeleted", itemDeletes );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "mobilesDeleted", mobileDeletes );
			TaskManagerDiagnostics.AddDeleted( diag, clean.Count );
			return clean.Count;
		}

		public static void Setup()
		{
			Setup( null );
		}

		public static void Setup( TaskManagerDiagnostics.Context diag )
		{
			ArrayList spots = new ArrayList();
			int itemsScanned = 0;
			TaskManagerDiagnostics.StartPhase( diag, "populateVillages:setupWorldScan" );
			long setupScanStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();

			foreach ( Item spot in World.Items.Values )
			{
				++itemsScanned;
				WorkingSpots workingSpot = spot as WorkingSpots;

				if ( workingSpot != null )
					spots.Add( workingSpot );
			}

			TaskManagerDailyAttributionDiagnostics.RecordSubphase( "PopulateVillages", "setupScan", setupScanStart );
			TaskManagerDiagnostics.AddScanned( diag, itemsScanned );
			TaskManagerDiagnostics.MarkRegistryCandidate( diag, "WorkingSpots setup full item scan" );
			ArrayList candidates = SelectSetupCandidates( spots, diag, itemsScanned, false );
			SpawnCandidatesSynchronously( candidates, diag );
		}

		private static ArrayList SelectSetupCandidates( ArrayList spots, TaskManagerDiagnostics.Context diag, int worldItemsScanned, bool reusedPopulationScan )
		{
			ArrayList candidates = new ArrayList();
			TaskManagerDiagnostics.StartPhase( diag, "populateVillages:setupScan" );
			long setupScanStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();

			for ( int i = 0; i < spots.Count; ++i )
			{
				WorkingSpots workingSpot = spots[i] as WorkingSpots;

				if ( IsSetupCandidate( workingSpot ) )
					candidates.Add( workingSpot );
			}

			TaskManagerDailyAttributionDiagnostics.RecordSubphase( "PopulateVillages", reusedPopulationScan ? "setupCandidateSelection" : "setupCandidateSelectionAfterScan", setupScanStart );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "setupItemsScanned", reusedPopulationScan ? spots.Count : worldItemsScanned );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "setupWorkingSpotsChecked", spots.Count );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "setupCandidates", candidates.Count );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "setupSkipped", spots.Count - candidates.Count );
			TaskManagerDiagnostics.AddScanned( diag, spots.Count );

			if ( reusedPopulationScan )
			{
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "setupWorldScansAvoided", 1 );
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "setupWorldItemChecksAvoided", Math.Max( 0, worldItemsScanned - spots.Count ) );
			}

			return candidates;
		}

		private static void SpawnCandidatesSynchronously( ArrayList spots, TaskManagerDiagnostics.Context diag )
		{
			TaskManagerDiagnostics.StartPhase( diag, "populateVillages:setupSpawn" );
			TaskManagerDiagnostics.AddScanned( diag, spots.Count );
			long setupSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();

			for ( int i = 0; i < spots.Count; ++i )
				ProcessSetupCandidate( (Item)spots[i], diag );

			TaskManagerDailyAttributionDiagnostics.RecordGroup( "PopulateVillages setupSpawn", setupSpawnStart );
			TaskManagerDiagnostics.MarkChunkCandidate( diag, "startup/recurring village repopulation can be chunked later" );
		}

		private static void ProcessPopulationSlice( PopulationRun run )
		{
			if ( run == null || run != m_ActivePopulation )
				return;

			Exception error = null;
			long chunkStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			int startIndex = run.Index;
			PopulationStage startStage = run.Stage;
			int operations = 0;

			TaskManagerDiagnostics.ResumeRun( run.Diagnostics );

			try
			{
				while ( run == m_ActivePopulation )
				{
					TaskManagerDailyAttributionDiagnostics.SetCurrentPhase( "populateVillages:" + GetPopulationStageName( run.Stage ) );
					PopulationStage operationStage = run.Stage;

					switch ( run.Stage )
					{
						case PopulationStage.ScanWorkingSpots:
							ProcessPopulationScan( run );
							break;
						case PopulationStage.CleanupSpots:
							ProcessPopulationCleanup( run );
							break;
						case PopulationStage.SelectCandidates:
							ProcessCandidateSelection( run );
							break;
						case PopulationStage.SetupSpawn:
							ProcessSetupSpawn( run );
							break;
						case PopulationStage.Complete:
							CompletePopulationRun( run, null );
							break;
					}

					++operations;

					if ( run != m_ActivePopulation
						|| ( operationStage != run.Stage && run.Stage == PopulationStage.SetupSpawn )
						|| TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( chunkStart ) >= PopulationSliceBudgetMs )
						break;
				}
			}
			catch ( Exception ex )
			{
				error = ex;
				TaskManagerDailyAttributionDiagnostics.RecordError( "PopulateVillages setupSpawn", ex );
			}

			long chunkElapsed = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( chunkStart );
			++run.ChunkCount;

			if ( chunkElapsed > run.MaxChunkMs )
				run.MaxChunkMs = chunkElapsed;

			if ( startStage == PopulationStage.SetupSpawn )
			{
				run.SetupWorkMs += chunkElapsed;
				TaskManagerDailyAttributionDiagnostics.RecordDetail( "PopulateVillages setupSpawn", "chunkSlice", "setupSpawn", chunkElapsed );
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "chunks", 1 );
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "chunkCandidates", Math.Max( 0, run.Index - startIndex ) );
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "chunkOperations", operations );
			}
			else
			{
				run.PreparationMs += chunkElapsed;
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "preparationChunkOperations", operations );
			}

			TaskManagerDiagnostics.SuspendRun( run.Diagnostics );

			if ( error != null )
			{
				CompletePopulationRun( run, error );
				return;
			}

			if ( run != m_ActivePopulation )
				return;

			try
			{
				PopulationTimer timer = new PopulationTimer( run );
				timer.Start();
			}
			catch ( Exception ex )
			{
				TaskManagerDailyAttributionDiagnostics.RecordError( "PopulateVillages setupSpawn", ex );
				CompletePopulationRun( run, ex );
			}
		}

		private static void ProcessPopulationScan( PopulationRun run )
		{
			TaskManagerDiagnostics.StartPhase( run.Diagnostics, "populateVillages:scanWorkingSpots" );
			long scanStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();

			foreach ( Item item in World.Items.Values )
			{
				++run.WorldItemsScanned;

				if ( item is WorkingSpots )
					run.Spots.Add( item );
			}

			TaskManagerDailyAttributionDiagnostics.RecordSubphase( "PopulateVillages", "scanWorkingSpots", scanStart );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "worldItemsScanned", run.WorldItemsScanned );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "workingSpotsFound", run.Spots.Count );
			TaskManagerDiagnostics.AddScanned( run.Diagnostics, run.WorldItemsScanned );
			TaskManagerDiagnostics.MarkRegistryCandidate( run.Diagnostics, "WorkingSpots full item scan" );
			run.Stage = PopulationStage.CleanupSpots;
			run.Index = 0;
		}

		private static void ProcessPopulationCleanup( PopulationRun run )
		{
			TaskManagerDiagnostics.StartPhase( run.Diagnostics, "populateVillages:cleanupSpots" );

			if ( run.Index < run.Spots.Count )
			{
				long cleanupStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
				Cleanup( ( Item )run.Spots[run.Index], run.Diagnostics );
				TaskManagerDailyAttributionDiagnostics.RecordSubphase( "PopulateVillages", "cleanupSpot", cleanupStart );
				++run.Index;
			}

			if ( run.Index >= run.Spots.Count )
			{
				TaskManagerDiagnostics.AddScanned( run.Diagnostics, run.Spots.Count );
				run.Stage = PopulationStage.SelectCandidates;
				run.Index = 0;
			}
		}

		private static void ProcessCandidateSelection( PopulationRun run )
		{
			TaskManagerDiagnostics.StartPhase( run.Diagnostics, "populateVillages:setupScan" );

			long selectionStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();

			if ( run.Index < run.Spots.Count )
			{
				WorkingSpots spot = run.Spots[run.Index] as WorkingSpots;

				if ( IsSetupCandidate( spot ) )
					run.Candidates.Add( spot );

				++run.Index;
			}

			TaskManagerDailyAttributionDiagnostics.RecordSubphase( "PopulateVillages", "setupCandidateSelection", selectionStart );

			if ( run.Index >= run.Spots.Count )
			{
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "setupItemsScanned", run.Spots.Count );
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "setupWorkingSpotsChecked", run.Spots.Count );
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "setupCandidates", run.Candidates.Count );
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "setupSkipped", run.Spots.Count - run.Candidates.Count );
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "setupWorldScansAvoided", 1 );
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages", "setupWorldItemChecksAvoided", Math.Max( 0, run.WorldItemsScanned - run.Spots.Count ) );
				TaskManagerDiagnostics.AddScanned( run.Diagnostics, run.Spots.Count );
				run.Stage = PopulationStage.SetupSpawn;
				run.Index = 0;
			}
		}

		private static void ProcessSetupSpawn( PopulationRun run )
		{
			TaskManagerDiagnostics.StartPhase( run.Diagnostics, "populateVillages:setupSpawn" );

			if ( run.Index < run.Candidates.Count )
			{
				ProcessSetupCandidate( ( Item )run.Candidates[run.Index], run.Diagnostics );
				++run.Index;
			}

			if ( run.Index >= run.Candidates.Count )
				run.Stage = PopulationStage.Complete;
		}

		private static string GetPopulationStageName( PopulationStage stage )
		{
			switch ( stage )
			{
				case PopulationStage.ScanWorkingSpots: return "scanWorkingSpots";
				case PopulationStage.CleanupSpots: return "cleanupSpots";
				case PopulationStage.SelectCandidates: return "setupCandidateSelection";
				case PopulationStage.SetupSpawn: return "setupSpawn";
				default: return "finalization";
			}
		}

		private static void CompletePopulationRun( PopulationRun run, Exception error )
		{
			long wallMs = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( run.WallStart );
			TaskManagerDailyAttributionDiagnostics.RecordGroupElapsed( "PopulateVillages setupSpawn", run.SetupWorkMs );
			TaskManagerDailyAttributionDiagnostics.RecordGroupElapsed( "PopulateVillages", run.PreparationMs + run.SetupWorkMs );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "stagedWallMs", wallMs );
			TaskManagerDailyAttributionDiagnostics.CompleteVillageStaging( wallMs );
			m_ActivePopulation = null;

			if ( run.Completion != null )
				run.Completion( error );
		}

		private static void ProcessSetupCandidate( Item spot, TaskManagerDiagnostics.Context diag )
		{
			long spotStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			int created = SpawnAtSpot( spot );
			long elapsed = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( spotStart );
			TaskManagerDiagnostics.AddCreated( diag, created );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "spotsProcessed", 1 );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", created > 0 ? "spotsCreated" : "spotsSkipped", 1 );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "mobilesCreated", created );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "moveToWorldCalls", created );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "homeSetupCalls", created );
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "onAfterSpawnCalls", created );
			RecordSetupSpot( spot, elapsed );

			if ( elapsed > 50 )
			{
				TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "heavySingleOperations", 1 );
				TaskManagerDailyAttributionDiagnostics.RecordDetail(
					"PopulateVillages setupSpawn",
					"heavySingleOperation",
					GetSpawnedTypeName( spot != null ? spot.Name : null ),
					elapsed );
			}
		}

		private static bool IsSetupCandidate( WorkingSpots spot )
		{
			if ( spot == null || spot.Deleted || Utility.RandomMinMax(1,4) == 1 )
				return false;

			bool infected = false;
			String reg = Worlds.GetMyWorld( spot.Map, spot.Location, spot.X, spot.Y );
			if (reg != null && AdventuresFunctions.RegionIsInfected( reg ) )
			{
				foreach ( Mobile m in spot.GetMobilesInRange( 20 ) )
				{
					if (m is BaseCreature)
					{
						if ( ((BaseCreature)m).CanInfect && !infected)
							infected = true;
					}
				}
			}
			return !infected;
		}

		private static int SpawnAtSpot( Item spot )
		{
			string errorNote;
			return SpawnAtSpot( spot, out errorNote );
		}

		private static int SpawnAtSpot( Item spot, out string errorNote )
		{
			errorNote = null;

			try
			{
				return SpawnAtSpotCore( spot );
			}
			catch ( Exception ex )
			{
				errorNote = GetSpawnErrorNote( spot, ex );
				TaskManagerDailyAttributionDiagnostics.RecordError( "PopulateVillages setupSpawn", ex );
				LogSpawnError( errorNote );
				return 0;
			}
		}

		private static string GetSpawnErrorNote( Item spot, Exception ex )
		{
			string spotName = spot != null ? spot.Name : "null";
			string map = spot != null && spot.Map != null ? spot.Map.ToString() : "null";
			string loc = spot != null ? spot.Location.ToString() : "null";
			string error = ex != null ? ex.GetType().Name : "unknown";

			return "spawnError=" + error + ", spot=" + spotName + ", map=" + map + ", loc=" + loc;
		}

		private static void LogSpawnError( string note )
		{
			if ( DateTime.UtcNow < m_NextSpawnErrorLog )
			{
				++m_SpawnErrorSuppressed;
				return;
			}

			if ( m_SpawnErrorSuppressed > 0 )
			{
				DiagnosticConsole.WriteLine( "[Lag]", DiagnosticSeverity.Warn, "TaskManagerDaily populateVillages spawn errors suppressed={0}", m_SpawnErrorSuppressed );
				m_SpawnErrorSuppressed = 0;
			}

			DiagnosticConsole.WriteLine( "[Lag]", DiagnosticSeverity.Error, "TaskManagerDaily populateVillages spawn failed: {0}", note );
			m_NextSpawnErrorLog = DateTime.UtcNow + TimeSpan.FromMinutes( 1.0 );
		}

		private static int SpawnAtSpotCore( Item spot )
		{
			if ( spot == null || spot.Deleted )
				return 0;

			if ( spot.Name == "alchemist" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new TradesmanAlchemist(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 2 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}
			else if ( spot.Name == "archer" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new TrainingBow(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 6 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}
			else if ( spot.Name == "fighter" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new TrainingSingle(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 1 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}
			else if ( spot.Name == "fisherman" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new TrainingFishing(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 6 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}
			else if ( spot.Name == "lumber" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new TradesmanLumber(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 2 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}
			else if ( spot.Name == "lumberjack" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new TradesmanLogger(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 1 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}
			else if ( spot.Name == "miner" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new TradesmanMiner(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 1 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}
			else if ( spot.Name == "smelter" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new TradesmanSmelter(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 1 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}
			else if ( spot.Name == "smith" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new TradesmanSmith(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 1 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}
			else if ( spot.Name == "tanner" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new TradesmanLeather(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 1 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}
			else if ( spot.Name == "cook" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new TradesmanCook(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 1 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}
			else if ( spot.Name == "warrior" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new Warriors(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 1 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}
			else if ( spot.Name == "wizard" )
			{
				long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Mobile citizen = new TrainingMagery(); RecordSetupOperation( "constructor", citizen, constructorStart );
				long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.MoveToWorld( spot.Location, spot.Map ); RecordSetupOperation( "moveToWorld", citizen, moveStart );
				long homeStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); ((BaseCreature)citizen).Home = spot.Location; ((BaseCreature)citizen).RangeHome = 0; RecordSetupOperation( "homeSetup", citizen, homeStart );
				long afterSpawnStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); citizen.OnAfterSpawn(); RecordSetupOperation( "onAfterSpawn", citizen, afterSpawnStart );
				long addStart = TaskManagerDailyAttributionDiagnostics.BeginTiming(); Add( citizen, spot, 4 ); RecordSetupOperation( "workingSpotsAdd", citizen, addStart ); return 1;
			}

			return 0;
		}

		private static void RecordSetupOperation( string operation, Mobile mobile, long startedTimestamp )
		{
			long elapsed = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( startedTimestamp );
			string type = mobile != null ? mobile.GetType().Name : "null";
			TaskManagerDailyAttributionDiagnostics.RecordSubphaseElapsed( "PopulateVillages setupSpawn", operation, elapsed );
			TaskManagerDailyAttributionDiagnostics.RecordDetail( "PopulateVillages setupSpawn", operation + "Type", type, elapsed );
		}

		private static void RecordSetupSpot( Item spot, long elapsedMs )
		{
			try
			{
				string spotName = spot != null && !String.IsNullOrWhiteSpace( spot.Name ) ? spot.Name : "unknown";
				string map = spot != null && spot.Map != null ? spot.Map.ToString() : "null";
				string location = spot != null ? spot.Location.ToString() : "null";
				string spawnedType = GetSpawnedTypeName( spotName );
				Region region = spot != null ? Region.Find( spot.Location, spot.Map ) : null;
				string regionName = region != null && !String.IsNullOrWhiteSpace( region.Name ) ? region.Name : "unknown";
				string description = String.Format( "spot={0} spawnedType={1} map={2} region={3} loc={4}", spotName, spawnedType, map, regionName, location );

				TaskManagerDailyAttributionDiagnostics.RecordDetail( "PopulateVillages setupSpawn", "spot", spotName, elapsedMs );
				TaskManagerDailyAttributionDiagnostics.RecordDetail( "PopulateVillages setupSpawn", "spawnedType", spawnedType, elapsedMs );
				TaskManagerDailyAttributionDiagnostics.RecordDetail( "PopulateVillages setupSpawn", "map", map, elapsedMs );
				TaskManagerDailyAttributionDiagnostics.RecordDetail( "PopulateVillages setupSpawn", "region", regionName, elapsedMs );
				TaskManagerDailyAttributionDiagnostics.RecordDetail( "PopulateVillages setupSpawn", "setupMethod", "SpawnAtSpot", elapsedMs );
				TaskManagerDailyAttributionDiagnostics.RecordWorst( "PopulateVillages setupSpawn", description, elapsedMs );
			}
			catch ( Exception ex )
			{
				TaskManagerDailyAttributionDiagnostics.RecordError( "PopulateVillages setupSpawn", ex );
			}
		}

		private static string GetSpawnedTypeName( string spotName )
		{
			if ( spotName == "alchemist" ) return "TradesmanAlchemist";
			if ( spotName == "archer" ) return "TrainingBow";
			if ( spotName == "fighter" ) return "TrainingSingle";
			if ( spotName == "fisherman" ) return "TrainingFishing";
			if ( spotName == "lumber" ) return "TradesmanLumber";
			if ( spotName == "lumberjack" ) return "TradesmanLogger";
			if ( spotName == "miner" ) return "TradesmanMiner";
			if ( spotName == "smelter" ) return "TradesmanSmelter";
			if ( spotName == "smith" ) return "TradesmanSmith";
			if ( spotName == "tanner" ) return "TradesmanLeather";
			if ( spotName == "cook" ) return "TradesmanCook";
			if ( spotName == "warrior" ) return "Warriors";
			if ( spotName == "wizard" ) return "TrainingMagery";
			return "none";
		}

		public static void Add( Mobile citizen, Item spots, int range )
		{
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "workingSpotsAddCalls", 1 );
			bool hasCitizen = false;

			ArrayList items = new ArrayList();
			foreach ( Item item in spots.GetItemsInRange( range ) )
			{
				if ( item is WorkingSpots && item != spots )
				{
					hasCitizen = true;
					items.Add( item );
				}
			}
			TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "accessorySpotsScanned", items.Count );
			for ( int i = 0; i < items.Count; ++i )
			{
				if ( hasCitizen )
				{
					Item spot = ( Item )items[ i ];
					long accessoryStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
					Item createdItem = null;
					Mobile createdMobile = null;

					if ( spot.Name == "anvil" && ( citizen is TradesmanSmelter  || citizen is TradesmanSmith ) ){ createdItem = new AnvilHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Weight = -2.0; }
					else if ( spot.Name == "dummy" && citizen is TrainingSingle ){ createdItem = new TrainingDummy(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Weight = -2.0; }
					else if ( spot.Name == "forge" && ( citizen is TradesmanSmelter  || citizen is TradesmanSmith ) ){ createdItem = new ForgeHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Weight = -2.0; }
					else if ( spot.Name == "hidden anvil" && ( citizen is TradesmanSmelter  || citizen is TradesmanSmith ) ){ createdItem = new AnvilHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Visible = false; createdItem.Weight = -2.0; }
					else if ( spot.Name == "hidden forge" && ( citizen is TradesmanSmelter  || citizen is TradesmanSmith ) ){ createdItem = new ForgeHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Visible = false; createdItem.Weight = -2.0; }
					else if ( spot.Name == "hidden pan" && ( citizen is TradesmanCook ) ){ createdItem = new StoveHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Visible = false; createdItem.Weight = -2.0; }
					else if ( spot.Name == "hidden hide" && citizen is TradesmanLeather ){ createdItem = new LeatherHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Visible = false; createdItem.Weight = -2.0; }
					else if ( spot.Name == "hide" && citizen is TradesmanLeather ){ createdItem = new LeatherHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Weight = -2.0; }
					else if ( spot.Name == "pentagram" && citizen is TrainingMagery ){ createdItem = new MagicHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Visible = false; createdItem.OnAfterSpawn(); createdItem.Weight = -2.0; }
					else if ( spot.Name == "potion" && citizen is TradesmanAlchemist ){ createdItem = new CauldronHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Weight = -2.0; }
					else if ( spot.Name == "rock" && citizen is TradesmanMiner ){ createdItem = new RockHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Visible = false; createdItem.Weight = -2.0; }
					else if ( spot.Name == "saw" && citizen is TradesmanLumber ){ createdItem = new SawHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Visible = false; createdItem.Weight = -2.0; }
					else if ( spot.Name == "pan" && citizen is TradesmanCook ){ createdItem = new StoveHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Visible = false; createdItem.Weight = -2.0; }
					else if ( spot.Name == "target" && citizen is TrainingBow ){ createdItem = new ArcheryButte(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Weight = -2.0; }
					else if ( spot.Name == "tree" && citizen is TradesmanLogger ){ createdItem = new TreeHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Visible = false; createdItem.Weight = -2.0; }
					else if ( spot.Name == "water" && citizen is TrainingFishing ){ createdItem = new WaterHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Visible = false; createdItem.Weight = -2.0; }
					else if ( spot.Name == "knight" && citizen is Warriors ){ createdMobile = new Warriors(); createdMobile.MoveToWorld( spot.Location, spot.Map ); ((BaseCreature)createdMobile).Home = spot.Location; ((BaseCreature)createdMobile).RangeHome = 0; createdMobile.OnAfterSpawn(); }
					else if ( spot.Name == "hidden pan" && citizen is TradesmanCook ){ createdItem = new StoveHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Visible = false; createdItem.Weight = -2.0; }
					else if ( spot.Name == "pan" && citizen is TradesmanCook ){ createdItem = new StoveHit(); createdItem.MoveToWorld( spot.Location, spot.Map ); createdItem.Weight = -2.0; }

					long accessoryElapsed = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( accessoryStart );

					if ( createdItem != null )
					{
						TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "itemsCreated", 1 );
						TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "moveToWorldCalls", 1 );
						TaskManagerDailyAttributionDiagnostics.RecordDetail( "PopulateVillages setupSpawn", "accessoryType", createdItem.GetType().Name, accessoryElapsed );
					}
					else if ( createdMobile != null )
					{
						TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "mobilesCreated", 1 );
						TaskManagerDailyAttributionDiagnostics.AddCount( "PopulateVillages setupSpawn", "moveToWorldCalls", 1 );
						TaskManagerDailyAttributionDiagnostics.RecordDetail( "PopulateVillages setupSpawn", "accessoryType", createdMobile.GetType().Name, accessoryElapsed );
					}
				}
			}
			//if ( !hasCitizen && spots.Name != "fighter" && spots.Name != "archer" ){ Console.WriteLine("" + spots.Name + " " + spots.X + ", " + spots.Y + ""); }
		}

		public WorkingSpots( Serial serial ) : base( serial )
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
}
