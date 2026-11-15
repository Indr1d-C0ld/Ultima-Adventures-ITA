using System;
using System.IO;
using Server.Items;
using Server.Network;
using System.Collections.Generic;
using System.Collections;
using Server.Mobiles;
using Server.Misc;
using Server.Targeting;
using Server.Multis;
using Server.Regions;
using Server.Diagnostics;

namespace Server.Items
{
	public partial class TaskManagerDaily : Item
	{
		
		private static DateTime lastrun = DateTime.UtcNow;
		private static readonly object m_RunLock = new object();
		private static DailyRunState m_ActiveRun;
		private static SpreadOutCleanupRun m_ActiveSpreadOutCleanup;
		private static long m_OverlapRefused;
		private static DateTime m_LastOverlapRefusedUtc;

		public delegate void DailyRunCompleted( bool success, Exception error, long elapsedMs );

		private sealed class DailyRunState
		{
			public TaskManagerDiagnostics.Context Diagnostics;
			public long AttributionStart;
			public bool AttributionStarted;
			public string TimerName;
			public bool Startup;
			public bool ForceDaily;
			public long StartedTimestamp;
			public DailyRunCompleted Completion;
			public bool Finished;
			public DailyPipelinePhase Phase;
			public Exception PendingError;
			public bool WaitingForStealingPedestals;
			public bool WaitingForSpreadOutCleanup;
			public bool WaitingForGardens;
			public bool WaitingForCities;
			public ArrayList InvestmentBankBoxes;
			public ArrayList InvestmentStones;
			public ArrayList InvestmentBones;
			public PlayerMobile GoodChampion;
			public PlayerMobile EvilChampion;
		}

		private enum DailyPipelinePhase
		{
			AfterVillages,
			QuestSearchCrates,
			StealingPedestals,
			SpreadOutCleanup,
			DrinkerCleanup,
			InvestmentsAndBalance,
			InvestmentItemScan,
			InvestmentPayout,
			InvestmentStoneBonePopulate,
			BalanceAndChampions,
			ChampionAnnouncements,
			InfectionInvasionCheck,
			CleanupRun,
			PlantGardens,
			BeginPopulateCities,
			Finalize
		}

		private static readonly TimeSpan DailyPhaseDelay = TimeSpan.FromMilliseconds( 25.0 );
		private static readonly TimeSpan SpreadOutCleanupDelay = TimeSpan.FromMilliseconds( 25.0 );
		private const long SpreadOutCleanupBudgetMs = 35;

		private enum SpreadOutCleanupStage
		{
			ScanMobiles,
			CreateMissingGargoyles,
			DeleteInternalInfected,
			ScanShips,
			ClearShips,
			DeleteTargets,
			MoveExodus,
			MoveSerpents,
			MoveGargoyles,
			DeleteHealers,
			Complete
		}

		private sealed class SpreadOutCleanupRun
		{
			public DailyRunState DailyState;
			public SpreadOutCleanupStage Stage;
			public ArrayList Targets = new ArrayList();
			public ArrayList Healers = new ArrayList();
			public ArrayList Exodus = new ArrayList();
			public ArrayList Serpents = new ArrayList();
			public ArrayList Gargoyles = new ArrayList();
			public ArrayList InternalInfected = new ArrayList();
			public ArrayList Ships = new ArrayList();
			public int Index;
			public int GargoyleCreateIndex;
			public long WallStart;
			public long WorkMs;
		}

		private sealed class SpreadOutCleanupTimer : Timer
		{
			private readonly SpreadOutCleanupRun m_Run;
			private readonly string m_Label;

			public SpreadOutCleanupTimer( SpreadOutCleanupRun run ) : base( SpreadOutCleanupDelay )
			{
				m_Run = run;
				m_Label = GetSpreadOutCleanupStageName( run != null ? run.Stage : SpreadOutCleanupStage.Complete );
				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				RunSpreadOutCleanupCallback( m_Run, ToString(), m_Label );
			}

			public override string ToString()
			{
				return GetType().FullName + " / TaskManagerDaily spreadOutCreatureCleanup:" + m_Label;
			}
		}

		private sealed class DailyPhaseTimer : Timer
		{
			private readonly DailyRunState m_State;
			private readonly DailyPipelinePhase m_Phase;

			public DailyPhaseTimer( DailyRunState state, DailyPipelinePhase phase ) : base( DailyPhaseDelay )
			{
				m_State = state;
				m_Phase = phase;
				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				RunDailyPhaseCallback( m_State, m_Phase, ToString() );
			}

			public override string ToString()
			{
				return GetType().FullName + " / TaskManagerDaily " + GetDailyPhaseName( m_Phase );
			}
		}

		public static DateTime LastRun { get { return lastrun; } }
		public static long OverlapRefusedCount { get { lock ( m_RunLock ) return m_OverlapRefused; } }
		public static DateTime LastOverlapRefusedUtc { get { lock ( m_RunLock ) return m_LastOverlapRefusedUtc; } }

		public static bool RunInProgress
		{
			get
			{
				lock ( m_RunLock )
					return m_ActiveRun != null;
			}
		}

		public static string ActiveRunDescription
		{
			get
			{
				lock ( m_RunLock )
					return m_ActiveRun != null
						? String.Format( "{0} / {1}", m_ActiveRun.TimerName, GetDailyPhaseName( m_ActiveRun.Phase ) )
						: "none";
			}
		}

		public static DateTime NextRunTime
		{
			get { return lastrun + TimeSpan.FromHours( 23.0 ); }
		}

		[Constructable]
		public TaskManagerDaily () : base( 0x0EDE )
		{
			Movable = false;
			Name = "Task Manager Daily";
			Visible = false;
			lastrun = DateTime.UtcNow;
			TaskTimer thisTimer = new TaskTimer( this ); 
			thisTimer.Start(); 
		}

        public TaskManagerDaily(Serial serial) : base(serial)
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
            writer.Write( (int) 0 ); // version
			writer.Write( (DateTime)lastrun );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
            int version = reader.ReadInt();
			lastrun = reader.ReadDateTime();
			
			if ( Server.Misc.MyServerSettings.RunRoutinesAtStartup() )
			{
				FirstTimer thisTimer = new FirstTimer( this ); 
				thisTimer.Start();
			}
		}

		public class TaskTimer : Timer 
		{
			private Item i_item; 
			public TaskTimer( Item task ) : base( TimeSpan.FromMinutes( 1440 ) )
			{ 
				Priority = TimerPriority.OneMinute; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				TaskTimer thisTimer = new TaskTimer( i_item ); 
				thisTimer.Start(); 
				RunThis( "TaskManagerDaily+TaskTimer", false );
			} 
		}

		public class FirstTimer : Timer 
		{ 
			private Item i_item; 
			public FirstTimer( Item task ) : base( TimeSpan.FromSeconds( 10.0 ) )
			{ 
				Priority = TimerPriority.OneSecond; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				TaskTimer thisTimer = new TaskTimer( i_item ); 
				thisTimer.Start(); 
				RunThis( "TaskManagerDaily+FirstTimer", true );
			} 
		}

		private static ArrayList GetBalancePlayers( out int playerScanCount )
		{
			ArrayList players = new ArrayList();
			playerScanCount = 0;

			foreach ( Mobile player in World.Mobiles.Values )
			{
				++playerScanCount;

				if ( player is PlayerMobile && player.AccessLevel == AccessLevel.Player  )
				{
					if ( player != null && ((PlayerMobile)player).BalanceEffect != 0 && ((PlayerMobile)player).BalanceStatus != 0 )
					{
						players.Add( player );
					}
				}
			}

			return players;
		}

		private static ArrayList GetOrderedBalancePortals( out int worldItemsScanned, out int portalGoodCandidates, out int portalEvilCandidates )
		{
			ArrayList portals = new ArrayList();
			worldItemsScanned = 0;
			portalGoodCandidates = 0;
			portalEvilCandidates = 0;

			// Per-run only: preserves World.Items.Values order without introducing a persistent registry or hooks.
			foreach ( Item itm in World.Items.Values )
			{
				++worldItemsScanned;

				if ( itm == null || itm.Deleted )
					continue;

				if ( itm is PortalGood )
				{
					portals.Add( itm );
					++portalGoodCandidates;
				}
				else if ( itm is PortalEvil )
				{
					portals.Add( itm );
					++portalEvilCandidates;
				}
			}

			return portals;
		}

		private static string GetDiagnosticSerial( Mobile mobile )
		{
			return mobile != null ? mobile.Serial.ToString() : "null";
		}

		private static void AddOwnedBalancePortal( ArrayList matches, Item itm, PlayerMobile player )
		{
			if ( matches == null || player == null || itm == null || itm.Deleted )
				return;

			if ( itm is PortalGood && ((PortalGood)itm).Owner == (Mobile)player )
			{
				matches.Add( itm );
			}
			else if ( itm is PortalEvil && ((PortalEvil)itm).Owner == (Mobile)player )
			{
				matches.Add( itm );
			}
		}

		private static long UpdateBalancePortalFingerprint( long hash, Item item )
		{
			unchecked
			{
				int value = item != null ? item.Serial.Value : 0;
				return ( hash * 1099511628211L ) ^ value;
			}
		}

		public static void WriteBalancePortalValidation( TextWriter writer )
		{
			if ( writer == null )
				return;

			writer.WriteLine( "BalancePortalValidate warning: this command intentionally compares against the old full-scan path." );
			writer.WriteLine( "BalancePortalValidate warning: it may take several seconds and should not be run during timed performance windows unless explicitly marked." );

			long start = TaskManagerDiagnostics.GetTimestamp();
			int playerScanCount;
			ArrayList players = GetBalancePlayers( out playerScanCount );
			int worldItemsScanned;
			int portalGoodCandidates;
			int portalEvilCandidates;
			ArrayList candidates = GetOrderedBalancePortals( out worldItemsScanned, out portalGoodCandidates, out portalEvilCandidates );
			long matchingPortals = 0;
			long mismatches = 0;
			string firstMismatch = null;
			long originalFingerprint = 1469598103934665603L;
			long candidateFingerprint = 1469598103934665603L;

			for ( int i = 0; i < players.Count; ++i )
			{
				PlayerMobile player = (PlayerMobile)players[ i ];
				ArrayList originalMatches = new ArrayList();
				ArrayList candidateMatches = new ArrayList();

				foreach ( Item itm in World.Items.Values )
					AddOwnedBalancePortal( originalMatches, itm, player );

				for ( int j = 0; j < candidates.Count; ++j )
					AddOwnedBalancePortal( candidateMatches, candidates[ j ] as Item, player );

				matchingPortals += originalMatches.Count;

				for ( int j = 0; j < originalMatches.Count; ++j )
					originalFingerprint = UpdateBalancePortalFingerprint( originalFingerprint, originalMatches[ j ] as Item );

				for ( int j = 0; j < candidateMatches.Count; ++j )
					candidateFingerprint = UpdateBalancePortalFingerprint( candidateFingerprint, candidateMatches[ j ] as Item );

				if ( originalMatches.Count != candidateMatches.Count )
				{
					++mismatches;

					if ( firstMismatch == null )
						firstMismatch = String.Format( "playerIndex={0} player={1} originalCount={2} candidateCount={3}", i, GetDiagnosticSerial( player ), originalMatches.Count, candidateMatches.Count );

					continue;
				}

				for ( int j = 0; j < originalMatches.Count; ++j )
				{
					Item original = originalMatches[ j ] as Item;
					Item candidate = candidateMatches[ j ] as Item;

					if ( original != candidate )
					{
						++mismatches;

						if ( firstMismatch == null )
						{
							string originalSerial = original != null ? original.Serial.ToString() : "null";
							string candidateSerial = candidate != null ? candidate.Serial.ToString() : "null";
							firstMismatch = String.Format( "playerIndex={0} player={1} matchIndex={2} original={3} candidate={4}", i, GetDiagnosticSerial( player ), j, originalSerial, candidateSerial );
						}

						break;
					}
				}
			}

			long elapsed = TaskManagerDiagnostics.GetElapsedMilliseconds( start );

			BalancePortalDiagnostics.RecordValidation(
				elapsed,
				playerScanCount,
				players.Count,
				worldItemsScanned,
				candidates.Count,
				matchingPortals,
				mismatches,
				firstMismatch,
				originalFingerprint,
				candidateFingerprint );

			writer.WriteLine( "BalancePortalValidate: elapsed={0}ms playersChecked={1} applicablePlayers={2} worldItemsScanned={3} portalCandidates={4} goodCandidates={5} evilCandidates={6} matchingPortals={7} mismatches={8} originalFingerprint={9} candidateFingerprint={10} firstMismatch={11}",
				elapsed,
				playerScanCount,
				players.Count,
				worldItemsScanned,
				candidates.Count,
				portalGoodCandidates,
				portalEvilCandidates,
				matchingPortals,
				mismatches,
				originalFingerprint,
				candidateFingerprint,
				firstMismatch != null ? firstMismatch : "none" );
		}

		public static void RunThis( string timerName, bool startup )
		{
			RunThis( timerName, startup, false );
		}

		public static void RunThis( string timerName, bool startup, bool forceDaily )
		{
			if ( !StartRun( timerName, startup, forceDaily, null ) )
				DiagnosticConsole.WriteLine( "[TaskManagerDaily]", DiagnosticSeverity.Warn, "Run refused because another Daily run is active: {0}", ActiveRunDescription );
		}

		public static bool StartRun( string timerName, bool startup, bool forceDaily, DailyRunCompleted completion )
		{
			DailyRunState state = new DailyRunState();
			state.TimerName = timerName;
			state.Startup = startup;
			state.ForceDaily = forceDaily;
			state.StartedTimestamp = TaskManagerDiagnostics.GetTimestamp();
			state.Completion = completion;

			lock ( m_RunLock )
			{
				if ( m_ActiveRun != null )
				{
					++m_OverlapRefused;
					m_LastOverlapRefusedUtc = DateTime.UtcNow;
					return false;
				}

				m_ActiveRun = state;
			}

			try
			{
				state.Diagnostics = TaskManagerDiagnostics.BeginRun( "TaskManagerDaily", timerName, startup );
				state.AttributionStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();

				try
				{
					TaskManagerDailyAttributionDiagnostics.BeginRun( state.Diagnostics.RunId, startup, state.Diagnostics.StartedUtc, timerName, forceDaily );
					TaskManagerDailyAttributionDiagnostics.MarkVillagesBeforeDueCheck();
					state.AttributionStarted = true;
				}
				catch ( Exception ex )
				{
					DiagnosticConsole.WriteLine( "[Lag]", DiagnosticSeverity.Error, "TaskManagerDaily attribution start failed: {0}: {1}", ex.GetType().Name, ex.Message );
				}

				TaskManagerDiagnostics.StartPhase( state.Diagnostics, "populateVillages" );
				DiagnosticConsole.WriteLine(
					"[TaskManagerDaily]",
					DiagnosticSeverity.Info,
					"staged run start timer={0} startup={1} forced={2} runId={3}",
					timerName,
					startup,
					forceDaily,
					state.Diagnostics.RunId );

				if ( !Server.Items.WorkingSpots.BeginPopulateVillages(
					state.Diagnostics,
					delegate( Exception error )
					{
						QueueDailyPhase( state, DailyPipelinePhase.AfterVillages, error );
					} ) )
				{
					throw new InvalidOperationException( "WorkingSpots population is already in progress." );
				}

				TaskManagerDiagnostics.SuspendRun( state.Diagnostics );
				return true;
			}
			catch ( Exception ex )
			{
				FinishRun( state, ex );

				if ( completion == null )
					throw;

				return false;
			}
		}

		private static void QueueDailyPhase( DailyRunState state, DailyPipelinePhase phase, Exception pendingError )
		{
			if ( state == null || state.Finished )
				return;

			// Heavy Daily phases always cross a timer boundary so completion work cannot
			// become part of the preceding population callback.
			state.Phase = phase;

			if ( pendingError != null )
				state.PendingError = pendingError;

			try
			{
				DailyPhaseTimer timer = new DailyPhaseTimer( state, phase );
				timer.Start();
			}
			catch ( Exception ex )
			{
				state.PendingError = ex;
				FinishRun( state, ex );
			}
		}

		private static void RunDailyPhaseCallback( DailyRunState state, DailyPipelinePhase phase, string callbackName )
		{
			if ( state == null || state.Finished )
				return;

			bool complete = false;
			bool scheduleNext = false;
			Exception error = null;
			long callbackStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			string phaseName = GetDailyPhaseName( phase );

			TaskManagerDiagnostics.ResumeRun( state.Diagnostics );
			TaskManagerDailyAttributionDiagnostics.SetCurrentPhase( phaseName );

			try
			{
				if ( state.PendingError != null )
				throw state.PendingError;

				switch ( phase )
				{
					case DailyPipelinePhase.AfterVillages:
					case DailyPipelinePhase.QuestSearchCrates:
					case DailyPipelinePhase.DrinkerCleanup:
					case DailyPipelinePhase.InvestmentsAndBalance:
					case DailyPipelinePhase.InvestmentItemScan:
					case DailyPipelinePhase.InvestmentPayout:
					case DailyPipelinePhase.InvestmentStoneBonePopulate:
					case DailyPipelinePhase.BalanceAndChampions:
					case DailyPipelinePhase.ChampionAnnouncements:
					case DailyPipelinePhase.InfectionInvasionCheck:
					case DailyPipelinePhase.CleanupRun:
						complete = ContinueAfterVillages( state, out error );
						scheduleNext = !complete && error == null;
						break;
					case DailyPipelinePhase.StealingPedestals:
						BeginStealingPedestalsPhase( state );
						break;
					case DailyPipelinePhase.SpreadOutCleanup:
						BeginSpreadOutCleanupPhase( state );
						break;
					case DailyPipelinePhase.PlantGardens:
						BeginPlantGardensPhase( state );
						break;
					case DailyPipelinePhase.BeginPopulateCities:
						BeginPopulateCitiesPhase( state );
						break;
					case DailyPipelinePhase.Finalize:
						RunFinalDailyPhase( state );
						complete = true;
						break;
				}
			}
			catch ( Exception ex )
			{
				error = ex;
				complete = true;
			}
			finally
			{
				long callbackElapsed = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( callbackStart );
				TaskManagerDailyAttributionDiagnostics.RecordStagedCallback( callbackName, GetDailyPhaseKind( phase ), callbackElapsed );
				TaskManagerDiagnostics.SuspendRun( state.Diagnostics );
			}

			if ( error != null )
				state.PendingError = error;

			if ( complete )
			{
				FinishRun( state, error );

				if ( error != null && state.Completion == null )
					throw error;

				return;
			}

			if ( scheduleNext )
				QueueDailyPhase( state, state.Phase, null );
		}

		private static string GetDailyPhaseName( DailyPipelinePhase phase )
		{
			switch ( phase )
			{
				case DailyPipelinePhase.AfterVillages: return "dailyPreamble";
				case DailyPipelinePhase.QuestSearchCrates: return "questSearchCrates";
				case DailyPipelinePhase.StealingPedestals: return "stealingPedestals";
				case DailyPipelinePhase.SpreadOutCleanup: return "spreadOutCreatureCleanup";
				case DailyPipelinePhase.DrinkerCleanup: return "drinkerCleanup";
				case DailyPipelinePhase.InvestmentsAndBalance: return "investmentsAndBalance";
				case DailyPipelinePhase.InvestmentItemScan: return "investmentItemScan";
				case DailyPipelinePhase.InvestmentPayout: return "investmentPayout";
				case DailyPipelinePhase.InvestmentStoneBonePopulate: return "stoneBonePopulate";
				case DailyPipelinePhase.BalanceAndChampions: return "balanceAndChampions";
				case DailyPipelinePhase.ChampionAnnouncements: return "championAnnouncements";
				case DailyPipelinePhase.InfectionInvasionCheck: return "infectionInvasionCheck";
				case DailyPipelinePhase.CleanupRun: return "cleanupRun";
				case DailyPipelinePhase.PlantGardens: return "plantGardens";
				case DailyPipelinePhase.BeginPopulateCities: return "populateCities:start";
				default: return "dailyFinalization";
			}
		}

		private static string GetDailyPhaseKind( DailyPipelinePhase phase )
		{
			switch ( phase )
			{
				case DailyPipelinePhase.AfterVillages: return "continuation";
				case DailyPipelinePhase.QuestSearchCrates: return "phase";
				case DailyPipelinePhase.StealingPedestals: return "phase";
				case DailyPipelinePhase.SpreadOutCleanup: return "cleanup";
				case DailyPipelinePhase.DrinkerCleanup: return "cleanup";
				case DailyPipelinePhase.InvestmentsAndBalance: return "phase";
				case DailyPipelinePhase.InvestmentItemScan: return "phase";
				case DailyPipelinePhase.InvestmentPayout: return "phase";
				case DailyPipelinePhase.InvestmentStoneBonePopulate: return "phase";
				case DailyPipelinePhase.BalanceAndChampions: return "phase";
				case DailyPipelinePhase.ChampionAnnouncements: return "phase";
				case DailyPipelinePhase.InfectionInvasionCheck: return "phase";
				case DailyPipelinePhase.CleanupRun: return "cleanup";
				case DailyPipelinePhase.PlantGardens: return "phase";
				case DailyPipelinePhase.BeginPopulateCities: return "phaseTransition";
				default: return "finalization";
			}
		}

		private static bool ContinueAfterVillages( DailyRunState state, out Exception error )
		{
			error = null;
			TaskManagerDiagnostics.Context diag = state.Diagnostics;
			bool startup = state.Startup;

			try
			{
				if ( state.Phase == DailyPipelinePhase.AfterVillages )
				{
					bool due = state.ForceDaily || DateTime.UtcNow > ( lastrun + TimeSpan.FromHours( 23.0 ) );
					TaskManagerDailyAttributionDiagnostics.MarkDueDecision( due );

					if ( !due )
					{
						TaskManagerDiagnostics.StartPhase( diag, "notDue" );
						return true;
					}

					World.Broadcast( 0x35, true, "Executing daily routine" ); // Let the players know lag is incoming
					Console.WriteLine( "Begin Daily Tasks" );
					LoggingFunctions.LogServer( "Start - Arrange Quest Search Crates" );
					state.Phase = DailyPipelinePhase.QuestSearchCrates;
					return false;
				}

				if ( state.Phase == DailyPipelinePhase.QuestSearchCrates )
				{
					///// MOVE THE SEARCH PEDESTALS //////////////////////////////////////
					TaskManagerDiagnostics.StartPhase( diag, "questSearchCrates" );
					BuildQuests.SearchCreate();
					LoggingFunctions.LogServer( "Done - Arrange Quest Search Crates" );
					LoggingFunctions.LogServer( "Start - Change Stealing Pedestals" );
					state.Phase = DailyPipelinePhase.StealingPedestals;
					return false;
				}

				if ( state.Phase == DailyPipelinePhase.DrinkerCleanup )
				{
				ArrayList drinkers = new ArrayList();
				TaskManagerDiagnostics.StartPhase( diag, "drinkerScan" );
				int drinkerScanCount = 0;
				foreach ( Mobile drunk in World.Mobiles.Values )
				{
				++drinkerScanCount;
				if ( drunk is AdventurerWest || drunk is AdventurerSouth || drunk is AdventurerNorth || drunk is AdventurerEast || drunk is TavernPatronWest || drunk is TavernPatronSouth || drunk is TavernPatronNorth || drunk is TavernPatronEast )
				{
					if ( drunk != null )
					{
						drinkers.Add( drunk );
					}
				}
				}
				TaskManagerDiagnostics.AddScanned( diag, drinkerScanCount );

				TaskManagerDiagnostics.StartPhase( diag, "drinkerCleanup" );
				for ( int i = 0; i < drinkers.Count; ++i )
				{
					Mobile drunk = ( Mobile )drinkers[ i ];
					Effects.SendLocationParticles( EffectItem.Create( drunk.Location, drunk.Map, EffectItem.DefaultDuration ), 0x3728, 8, 20, 5042 );
					Effects.PlaySound( drunk, drunk.Map, 0x201 );
					drunk.Delete();
				}
				TaskManagerDiagnostics.AddDeleted( diag, drinkers.Count );
				state.Phase = DailyPipelinePhase.InvestmentsAndBalance;
				return false;
				}

				if ( state.Phase == DailyPipelinePhase.InvestmentsAndBalance )
				{
					Console.WriteLine( "investment payout" );
					World.Broadcast( 0, true, "Investment bags have been calculated for the day!" );
					TaskManagerDiagnostics.StartPhase( diag, "aetherCurse" );
					long aetherCurseStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
					AetherGlobe.ChangeCurse( 0 );
					long aetherCurseMs = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( aetherCurseStart );
					TaskManagerDailyAttributionDiagnostics.RecordSubphaseElapsed( "InvestmentsAndBalance", "aetherCurse", aetherCurseMs );
					TaskManagerDailyAttributionDiagnostics.RecordGroupElapsed( "InvestmentsAndBalance", aetherCurseMs );
					state.InvestmentBankBoxes = new ArrayList();
					state.InvestmentStones = new ArrayList();
					state.InvestmentBones = new ArrayList();
					state.Phase = DailyPipelinePhase.InvestmentItemScan;
					return false;
				}

				if ( state.Phase == DailyPipelinePhase.InvestmentItemScan )
				{
					TaskManagerDiagnostics.StartPhase( diag, "investmentItemScan" );
					TaskManagerDiagnostics.MarkRegistryCandidate( diag, "daily investment/tombstone/bone full item scan" );
					long investmentScanStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
					int investmentScanCount = 0;

					foreach( Item bb in World.Items.Values )
					{
						++investmentScanCount;

						if ( bb is BankBox )
							state.InvestmentBankBoxes.Add( bb );

						if ( (bb is TombStone || bb is Corpse) && bb.RootParentEntity == null )
						{
							Region region = Region.Find( bb.Location, bb.Map );

							if (bb.Map != null && bb.Map != Map.Internal && !(region.IsPartOf( typeof( BardTownRegion ) ) ) && !(region.IsPartOf( typeof( BardDungeonRegion ) )) )
								state.InvestmentStones.Add(bb);
						}

						if (bb is EssenceBones)
							state.InvestmentBones.Add(bb);
					}

					long investmentScanMs = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( investmentScanStart );
					TaskManagerDiagnostics.AddScanned( diag, investmentScanCount );
					TaskManagerDailyAttributionDiagnostics.RecordSubphaseElapsed( "InvestmentsAndBalance", "investmentItemScan", investmentScanMs );
					TaskManagerDailyAttributionDiagnostics.RecordGroupElapsed( "InvestmentsAndBalance", investmentScanMs );
					TaskManagerDailyAttributionDiagnostics.AddCount( "InvestmentsAndBalance", "itemsScanned", investmentScanCount );
					state.Phase = DailyPipelinePhase.InvestmentPayout;
					return false;
				}

				if ( state.Phase == DailyPipelinePhase.InvestmentPayout )
				{
					TaskManagerDiagnostics.StartPhase( diag, "investmentPayout" );
					long investmentPayoutStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();

					foreach( BankBox ibb in state.InvestmentBankBoxes )
					{
						double interest = 0;
						double evilinterest = 0;

						foreach( Item item in ibb.Items )
						{
							if( item is InterestBag )
							{
								List<Item> ItemsInBag = item.Items;

								for( int z = 0; z < ItemsInBag.Count; z++ )
								{
									Item inBag = ItemsInBag[z];

									if( inBag is InvestmentCheck )
									{
										if (Utility.RandomMinMax(1, 5000) == 69)
										{
											((InvestmentCheck)inBag).Worth = 0;
											LoggingFunctions.LogInvestments( ibb.Owner, ((InvestmentCheck)inBag).Worth, false );
										}
										else
										{
											interest = ((InvestmentCheck)inBag).Worth * AetherGlobe.rateofreturn;
											((InvestmentCheck)inBag).Worth += (int)interest;

											if (interest > 0)
												LoggingFunctions.LogInvestments( ibb.Owner, (int)interest, true );
											else
												LoggingFunctions.LogInvestments( ibb.Owner, (int)interest, false );
										}
									}
								}
							}

							if( item is EvilInterestBag )
							{
								List<Item> ItemsInBag = item.Items;

								for( int z = 0; z < ItemsInBag.Count; z++ )
								{
									Item inBag = ItemsInBag[z];

									if( inBag is InvestmentCheck )
									{
										if (AetherGlobe.rateofreturn > 0)
											evilinterest = -(AetherGlobe.rateofreturn);
										else if (AetherGlobe.rateofreturn < 0)
											evilinterest = Math.Abs(AetherGlobe.rateofreturn);

										interest = ((InvestmentCheck)inBag).Worth * evilinterest;
										((InvestmentCheck)inBag).Worth += (int)interest;

										if (interest > 0)
											LoggingFunctions.LogInvestments( ibb.Owner, (int)interest, true );
										else
											LoggingFunctions.LogInvestments( ibb.Owner, (int)interest, false );
									}
								}
							}
						}
					}

					long investmentPayoutMs = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( investmentPayoutStart );
					TaskManagerDailyAttributionDiagnostics.RecordSubphaseElapsed( "InvestmentsAndBalance", "investmentPayout", investmentPayoutMs );
					TaskManagerDailyAttributionDiagnostics.RecordGroupElapsed( "InvestmentsAndBalance", investmentPayoutMs );
					state.Phase = DailyPipelinePhase.InvestmentStoneBonePopulate;
					return false;
				}

				if ( state.Phase == DailyPipelinePhase.InvestmentStoneBonePopulate )
				{
					TaskManagerDiagnostics.StartPhase( diag, "stoneBonePopulate" );
					long stoneBoneStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();

					foreach( Item stone in state.InvestmentStones )
					{
						Region reg = Region.Find( stone.Location, stone.Map );
						BaseHouse house = BaseHouse.FindHouseAt( stone.Location, stone.Map, 16 );

						if ( Utility.RandomMinMax(1, 200) == 69 && house == null && !(reg.IsPartOf( typeof( SafeRegion )) || reg.IsPartOf( typeof ( PublicRegion )) || reg.IsPartOf( typeof ( ProtectedRegion )) || reg.IsPartOf( typeof ( HouseRegion )) ) )
							AdventuresFunctions.PopulateStones( stone );
					}

					foreach( EssenceBones bns in state.InvestmentBones )
						AdventuresFunctions.PopulateBones(bns);

					long stoneBoneMs = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( stoneBoneStart );
					TaskManagerDailyAttributionDiagnostics.RecordSubphaseElapsed( "InvestmentsAndBalance", "stoneBonePopulate", stoneBoneMs );
					TaskManagerDailyAttributionDiagnostics.RecordGroupElapsed( "InvestmentsAndBalance", stoneBoneMs );
					TaskManagerDailyAttributionDiagnostics.AddCount( "InvestmentsAndBalance", "bankBoxes", state.InvestmentBankBoxes.Count );
					TaskManagerDailyAttributionDiagnostics.AddCount( "InvestmentsAndBalance", "stones", state.InvestmentStones.Count );
					TaskManagerDailyAttributionDiagnostics.AddCount( "InvestmentsAndBalance", "bones", state.InvestmentBones.Count );
					state.InvestmentBankBoxes = null;
					state.InvestmentStones = null;
					state.InvestmentBones = null;
					state.Phase = DailyPipelinePhase.BalanceAndChampions;
					return false;
				}

				if ( state.Phase == DailyPipelinePhase.BalanceAndChampions )
				{
				long balanceGroupStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
				//Decay BalanceEffect and crown champion
				World.Broadcast( 0, true, "Balance Champions have been chosen!" );
				Console.WriteLine( "Balance Champs chosen" );
				state.GoodChampion = null;
				state.EvilChampion = null;
				
/*				AetherGlobe.GoodChamp = null;
				AetherGlobe.EvilChamp = null;*/
				
				TaskManagerDiagnostics.StartPhase( diag, "balancePlayerScan" );
				int playerScanCount;
				ArrayList players = GetBalancePlayers( out playerScanCount );
				TaskManagerDiagnostics.AddScanned( diag, playerScanCount );

				TaskManagerDiagnostics.StartPhase( diag, "balanceChampionAndPortals" );
				TaskManagerDiagnostics.MarkRegistryCandidate( diag, "per-run ordered portal candidate list active; no persistent registry or hooks" );
				TaskManagerDiagnostics.MarkChunkCandidate( diag, "balance champion and portal upkeep can be split after portal registry" );
				long balanceStart = TaskManagerDiagnostics.GetTimestamp();
				BalancePortalDiagnostics.Sample balanceSample = new BalancePortalDiagnostics.Sample();
				balanceSample.RunId = diag != null ? diag.RunId : 0;
				balanceSample.Startup = startup;
				balanceSample.StartedUtc = DateTime.UtcNow;
				balanceSample.PlayersChecked = playerScanCount;
				balanceSample.ApplicablePlayers = players.Count;
				balanceSample.ValidationStatus = "Not run";
				int balanceWorldItemsScanned = 0;
				int balancePortalGoodCandidates = 0;
				int balancePortalEvilCandidates = 0;
				ArrayList balancePortals = new ArrayList();

				if ( players.Count > 0 )
				{
					long candidateBuildStart = TaskManagerDiagnostics.GetTimestamp();
					balancePortals = GetOrderedBalancePortals( out balanceWorldItemsScanned, out balancePortalGoodCandidates, out balancePortalEvilCandidates );
					balanceSample.CandidateBuildMs = TaskManagerDiagnostics.GetElapsedMilliseconds( candidateBuildStart );
				}

				balanceSample.WorldItemsScanned = balanceWorldItemsScanned;
				balanceSample.PortalCandidates = balancePortals.Count;
				balanceSample.PortalGoodCandidates = balancePortalGoodCandidates;
				balanceSample.PortalEvilCandidates = balancePortalEvilCandidates;
				long balancePortalChanges = 0;
				for ( int i = 0; i < players.Count; ++i )
				{
					
						PlayerMobile m = (PlayerMobile)players[ i ];
						long playerCandidateIterations = 0;
						long playerMatchedPortals = 0;

						if ((Mobile)m == AetherGlobe.GoodChamp || (Mobile)m == AetherGlobe.EvilChamp)
						{
							double loss = (double)m.BalanceEffect * 0.10;
							if (m.SoulBound)
								loss = (double)m.BalanceEffect * 0.05;
								
							m.BalanceEffect -= (int)Math.Ceiling(loss); // decay the amount
							++balanceSample.BalanceEffectCalls;
						}

						// count portal decay values
						for ( int j = 0; j < balancePortals.Count; ++j )
						{
							Item itm = balancePortals[ j ] as Item;
							++balanceSample.CandidatePortalIterations;
							++playerCandidateIterations;

							if ( itm == null || itm.Deleted )
								continue;

							if ( itm is PortalGood && ((PortalGood)itm).Owner == (Mobile)m )
							{
								PortalGood portal = (PortalGood)itm;
								int decayupkeep = (int)( 0.03  * (double)m.BalanceEffect);
								++balanceSample.PortalsMatched;
								++balanceSample.PortalGoodMatched;
								++playerMatchedPortals;

								if (decayupkeep < 250) //player doesnt have enough bp to maintain this shard
								{
									if ( !portal.Sleep )
										++balanceSample.SleepTransitions;

									portal.Sleep = true;
									++balancePortalChanges;
								}
								else if (portal.Sleep && decayupkeep > 250)
								{
									portal.Sleep = false;
									m.BalanceEffect -= decayupkeep;
									++balanceSample.SleepTransitions;
									++balanceSample.BalanceEffectCalls;
									++balancePortalChanges;
								}
								else 
								{
									m.BalanceEffect -= decayupkeep;
									++balanceSample.BalanceEffectCalls;
									++balancePortalChanges;
								}
							}
							else if ( itm is PortalEvil && ((PortalEvil)itm).Owner == (Mobile)m )
							{
								PortalEvil portal = (PortalEvil)itm;
								int decayupkeep = (int)( 0.03  * (double)m.BalanceEffect);
								++balanceSample.PortalsMatched;
								++balanceSample.PortalEvilMatched;
								++playerMatchedPortals;

								if (decayupkeep < 250) //player doesnt have enough bp to maintain this shard
								{
									if ( !portal.Sleep )
										++balanceSample.SleepTransitions;

									portal.Sleep = true;
									++balancePortalChanges;
								}
								else if (portal.Sleep && decayupkeep > 250)
								{
									portal.Sleep = false;
									m.BalanceEffect -= decayupkeep;
									++balanceSample.SleepTransitions;
									++balanceSample.BalanceEffectCalls;
									++balancePortalChanges;
								}
								else 
								{
									m.BalanceEffect -= decayupkeep;
									++balanceSample.BalanceEffectCalls;
									++balancePortalChanges;
								}
							}
						}

						if ( playerCandidateIterations > balanceSample.WorstPlayerCandidateIterations )
						{
							balanceSample.WorstPlayerCandidateIterations = playerCandidateIterations;
							balanceSample.WorstPlayerIterationMatches = playerMatchedPortals;
							balanceSample.WorstPlayerByIterations = GetDiagnosticSerial( m );
						}

						if ( playerMatchedPortals > balanceSample.WorstPlayerMatchedPortals )
						{
							balanceSample.WorstPlayerMatchedPortals = playerMatchedPortals;
							balanceSample.WorstPlayerMatchedIterations = playerCandidateIterations;
							balanceSample.WorstPlayerByMatchedPortals = GetDiagnosticSerial( m );
						}
								
						if (m.BalanceEffect < 0 && m.Avatar && m.BalanceStatus != 0 && m.Kills == 0 ) //contributed to good
						{
							if (state.GoodChampion == null )
							{
								
								state.GoodChampion = m;
							}
							else if (m.BalanceEffect < state.GoodChampion.BalanceEffect )
							{
								
								state.GoodChampion = m;
							}
						}
						else if (m.BalanceEffect > 0 && m.Avatar && m.BalanceStatus != 0 ) //contributed to evil
						{
							
							if (state.EvilChampion == null)
							{
								
								state.EvilChampion = m;
							}
							else if (m.BalanceEffect > state.EvilChampion.BalanceEffect)
							{
								state.EvilChampion = m;
								
							}
						}
				}
				balanceSample.OriginalEquivalentChecks = balanceSample.ApplicablePlayers * balanceSample.WorldItemsScanned;
				balanceSample.OptimizedActualChecks = balanceSample.WorldItemsScanned + balanceSample.CandidatePortalIterations;
				balanceSample.AvoidedChecks = balanceSample.OriginalEquivalentChecks - balanceSample.OptimizedActualChecks;

				if ( balanceSample.AvoidedChecks < 0 )
					balanceSample.AvoidedChecks = 0;

				balanceSample.TotalPhaseMs = TaskManagerDiagnostics.GetElapsedMilliseconds( balanceStart );
				BalancePortalDiagnostics.RecordRun( balanceSample );
				TaskManagerDiagnostics.AddScanned( diag, balanceSample.OptimizedActualChecks );
				TaskManagerDiagnostics.AddModified( diag, balancePortalChanges );
				TaskManagerDailyAttributionDiagnostics.RecordSubphaseElapsed( "BalanceAndChampions", "balanceRun", balanceSample.TotalPhaseMs );
				TaskManagerDailyAttributionDiagnostics.RecordGroup( "BalanceAndChampions", balanceGroupStart );
				state.Phase = DailyPipelinePhase.ChampionAnnouncements;
				return false;
				}

				if ( state.Phase == DailyPipelinePhase.ChampionAnnouncements )
				{
				long championAnnouncementStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
				if (state.GoodChampion != null && state.GoodChampion != AetherGlobe.GoodChamp)
				{
					AetherGlobe.GoodChamp = (Mobile)state.GoodChampion;
					state.GoodChampion.InvalidateProperties();
				}
				if (state.EvilChampion != null && state.EvilChampion != AetherGlobe.EvilChamp)
				{
					AetherGlobe.EvilChamp = (Mobile)state.EvilChampion;
					state.EvilChampion.InvalidateProperties();
				}

				PlayerMobile announcedGoodChamp = AetherGlobe.GoodChamp as PlayerMobile;
				PlayerMobile announcedEvilChamp = AetherGlobe.EvilChamp as PlayerMobile;

				if (announcedGoodChamp != null)
				{
					string goodMessage = announcedGoodChamp.Name + " valiantly fights for the side of Good";
					World.Broadcast( 0, true, goodMessage );
					DiscordBridge.ReportGameEvent( "Balance Champion (Good): " + announcedGoodChamp.Name, "Logging Adventures" );
				}

				if (announcedEvilChamp != null)
				{
					string evilMessage = announcedEvilChamp.Name + " continues to cause anarchy";
					World.Broadcast( 0, true, evilMessage );
					DiscordBridge.ReportGameEvent( "Balance Champion (Evil): " + announcedEvilChamp.Name, "Logging Adventures" );
				}
				TaskManagerDailyAttributionDiagnostics.RecordSubphase( "BalanceAndChampions", "championAnnouncements", championAnnouncementStart );
				TaskManagerDailyAttributionDiagnostics.RecordGroupElapsed(
					"BalanceAndChampions",
					TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( championAnnouncementStart ) );
				state.Phase = DailyPipelinePhase.InfectionInvasionCheck;
				return false;
				}

				if ( state.Phase == DailyPipelinePhase.InfectionInvasionCheck )
				{
				long infectionGroupStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
				long infectionStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
				AdventuresFunctions.CheckInfection();
				TaskManagerDailyAttributionDiagnostics.RecordSubphase( "BalanceAndChampions", "infectionCheck", infectionStart );
				if (AetherGlobe.invasionstage == 1)
				{
					TaskManagerDiagnostics.StartPhase( diag, "invasionRoutine" );
					long invasionStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
					AdventuresFunctions.InvasionRoutine();
					TaskManagerDailyAttributionDiagnostics.RecordSubphase( "BalanceAndChampions", "invasionRoutine", invasionStart );
				}
				TaskManagerDailyAttributionDiagnostics.RecordGroup( "BalanceAndChampions", infectionGroupStart );
				state.Phase = DailyPipelinePhase.CleanupRun;
				return false;
				}

				if ( state.Phase == DailyPipelinePhase.CleanupRun )
				{
				TaskManagerDiagnostics.StartPhase( diag, "cleanupRun" );
				TaskManagerDiagnostics.MarkChunkCandidate( diag, "cleanup helper should be profiled/chunked separately" );
				Cleanup.Run();
				state.Phase = DailyPipelinePhase.PlantGardens;
				return false;
				}
			}
			catch ( Exception ex )
			{
				error = ex;
			}

			return true;
		}

		private static void BeginStealingPedestalsPhase( DailyRunState state )
		{
			if ( state == null || state.Finished )
				return;

			TaskManagerDiagnostics.StartPhase( state.Diagnostics, "stealingPedestals" );
			TaskManagerDiagnostics.MarkChunkCandidate( state.Diagnostics, "active Daily-only ordered pedestal rebuild uses a 35ms timer budget across standard, essence, and special work units" );
			state.WaitingForStealingPedestals = true;

			if ( !BuildSteadPeds.BeginCreateStealPeds(
				state.Diagnostics,
				delegate( Exception error )
				{
					state.WaitingForStealingPedestals = false;
					LoggingFunctions.LogServer( "Done - Change Stealing Pedestals" );
					QueueDailyPhase( state, DailyPipelinePhase.SpreadOutCleanup, error );
				} ) )
			{
				state.WaitingForStealingPedestals = false;
				throw new InvalidOperationException( "Stealing pedestal rebuild is already in progress." );
			}
		}

		private static void BeginSpreadOutCleanupPhase( DailyRunState state )
		{
			if ( state == null || state.Finished )
				return;

			if ( m_ActiveSpreadOutCleanup != null )
				throw new InvalidOperationException( "Spread-out creature cleanup is already in progress." );

			LoggingFunctions.LogServer( "Start - Remove Spread Out Monsters, Drinkers, And Healers" );
			TaskManagerDiagnostics.StartPhase( state.Diagnostics, "spreadOutCreatureCleanup" );
			TaskManagerDiagnostics.MarkChunkCandidate( state.Diagnostics, "active Daily-only ordered spread-out creature and NPC ship cleanup pump" );

			SpreadOutCleanupRun run = new SpreadOutCleanupRun();
			run.DailyState = state;
			run.Stage = SpreadOutCleanupStage.ScanMobiles;
			run.WallStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			m_ActiveSpreadOutCleanup = run;
			state.WaitingForSpreadOutCleanup = true;

			TaskManagerDailyAttributionDiagnostics.MarkStaged();
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "staged", 1 );

			try
			{
				SpreadOutCleanupTimer timer = new SpreadOutCleanupTimer( run );
				timer.Start();
			}
			catch
			{
				state.WaitingForSpreadOutCleanup = false;
				m_ActiveSpreadOutCleanup = null;
				throw;
			}
		}

		private static void RunSpreadOutCleanupCallback( SpreadOutCleanupRun run, string callbackName, string callbackKind )
		{
			if ( run == null || run != m_ActiveSpreadOutCleanup )
				return;

			long callbackStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			bool complete = false;
			Exception error = null;

			TaskManagerDiagnostics.ResumeRun( run.DailyState.Diagnostics );
			TaskManagerDiagnostics.StartPhase( run.DailyState.Diagnostics, "spreadOutCreatureCleanup:" + callbackKind );
			TaskManagerDailyAttributionDiagnostics.SetCurrentPhase( "spreadOutCreatureCleanup:" + callbackKind );

			try
			{
				complete = ProcessSpreadOutCleanupSlice( run );
			}
			catch ( Exception ex )
			{
				error = ex;
				TaskManagerDailyAttributionDiagnostics.RecordError( "SpreadOutCreatureCleanup", ex );
			}
			finally
			{
				long callbackElapsed = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( callbackStart );
				run.WorkMs += callbackElapsed;
				TaskManagerDailyAttributionDiagnostics.RecordSpreadOutCleanupCallback( callbackName, callbackKind, callbackElapsed );
				TaskManagerDailyAttributionDiagnostics.RecordStagedCallback( callbackName, "cleanup", callbackElapsed );
				TaskManagerDiagnostics.SuspendRun( run.DailyState.Diagnostics );
			}

			if ( error != null || complete )
			{
				CompleteSpreadOutCleanupRun( run, error );
				return;
			}

			try
			{
				SpreadOutCleanupTimer timer = new SpreadOutCleanupTimer( run );
				timer.Start();
			}
			catch ( Exception ex )
			{
				TaskManagerDailyAttributionDiagnostics.RecordError( "SpreadOutCreatureCleanup", ex );
				CompleteSpreadOutCleanupRun( run, ex );
			}
		}

		private static bool ProcessSpreadOutCleanupSlice( SpreadOutCleanupRun run )
		{
			long sliceStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			int operations = 0;

			while ( run == m_ActiveSpreadOutCleanup )
			{
				SpreadOutCleanupStage stage = run.Stage;

				if ( !ProcessSpreadOutCleanupOperation( run ) )
					return true;

				++operations;

				if ( run.Stage != stage
					|| TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( sliceStart ) >= SpreadOutCleanupBudgetMs )
					break;
			}

			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "callbackOperations", operations );
			return false;
		}

		private static bool ProcessSpreadOutCleanupOperation( SpreadOutCleanupRun run )
		{
			switch ( run.Stage )
			{
				case SpreadOutCleanupStage.ScanMobiles:
					ScanSpreadOutMobiles( run );
					break;
				case SpreadOutCleanupStage.CreateMissingGargoyles:
					CreateMissingSpreadOutGargoyle( run );
					break;
				case SpreadOutCleanupStage.DeleteInternalInfected:
					ProcessSpreadOutMobile( run, run.InternalInfected, "deleteInternalInfected" );
					break;
				case SpreadOutCleanupStage.ScanShips:
					ScanSpreadOutShips( run );
					break;
				case SpreadOutCleanupStage.ClearShips:
					ProcessSpreadOutShip( run );
					break;
				case SpreadOutCleanupStage.DeleteTargets:
					ProcessSpreadOutMobile( run, run.Targets, "deleteTarget" );
					break;
				case SpreadOutCleanupStage.MoveExodus:
					ProcessSpreadOutMobile( run, run.Exodus, "moveExodus" );
					break;
				case SpreadOutCleanupStage.MoveSerpents:
					ProcessSpreadOutMobile( run, run.Serpents, "moveSerpent" );
					break;
				case SpreadOutCleanupStage.MoveGargoyles:
					ProcessSpreadOutMobile( run, run.Gargoyles, "moveGargoyle" );
					break;
				case SpreadOutCleanupStage.DeleteHealers:
					ProcessSpreadOutMobile( run, run.Healers, "deleteHealer" );
					break;
				case SpreadOutCleanupStage.Complete:
					return false;
			}

			return true;
		}

		private static void ScanSpreadOutMobiles( SpreadOutCleanupRun run )
		{
			TaskManagerDiagnostics.StartPhase( run.DailyState.Diagnostics, "spreadOutCreatureScan" );
			long scanStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			int scanned = 0;

			foreach ( Mobile creature in World.Mobiles.Values )
			{
				++scanned;

				if ( creature is CodexGargoyleA || creature is CodexGargoyleB )
				{
					run.Gargoyles.Add( creature );
				}
				else if ( creature is BaseCreature && ( ( BaseCreature )creature ).CanInfect && creature.Map == Map.Internal )
				{
					run.InternalInfected.Add( creature );
				}
				else if ( creature.WhisperHue == 999 || creature.WhisperHue == 911 )
				{
					if ( creature is Adventurers || creature is WanderingHealer || creature is Courier || creature is Syth || creature is Jedi )
						run.Healers.Add( creature );
					else if ( creature is Exodus )
						run.Exodus.Add( creature );
					else if ( creature is Jormungandr )
						run.Serpents.Add( creature );
					else
						run.Targets.Add( creature );
				}
			}

			TaskManagerDailyAttributionDiagnostics.RecordSubphase( "SpreadOutCreatureCleanup", "scan", scanStart );
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "mobilesScanned", scanned );
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "internalInfectedEligible", run.InternalInfected.Count );
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "targetsEligible", run.Targets.Count );
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "exodusEligible", run.Exodus.Count );
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "serpentsEligible", run.Serpents.Count );
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "gargoylesEligible", run.Gargoyles.Count );
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "healersEligible", run.Healers.Count );
			TaskManagerDiagnostics.AddScanned( run.DailyState.Diagnostics, scanned );
			AdvanceSpreadOutCleanupStage( run, SpreadOutCleanupStage.CreateMissingGargoyles );
		}

		private static void CreateMissingSpreadOutGargoyle( SpreadOutCleanupRun run )
		{
			if ( run.Gargoyles.Count > 0 || run.GargoyleCreateIndex >= 2 )
			{
				AdvanceSpreadOutCleanupStage( run, SpreadOutCleanupStage.DeleteInternalInfected );
				return;
			}

			long constructorStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			Mobile gargoyle = run.GargoyleCreateIndex == 0 ? ( Mobile )new CodexGargoyleA() : new CodexGargoyleB();
			TaskManagerDailyAttributionDiagnostics.RecordSubphase( "SpreadOutCreatureCleanup", "gargoyleConstructor", constructorStart );
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "creaturesCreated", 1 );

			long moveStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			Worlds.MoveToRandomDungeon( gargoyle );
			RecordSpreadOutAction( "createdGargoyleMove", gargoyle, moveStart );
			RecordHeavySpreadOutOperation( "createdGargoyleMove", moveStart );
			++run.GargoyleCreateIndex;
		}

		private static void ScanSpreadOutShips( SpreadOutCleanupRun run )
		{
			long scanStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			run.Ships = Server.Multis.BaseBoat.GetClearShipTargets();
			TaskManagerDailyAttributionDiagnostics.RecordSubphase( "SpreadOutCreatureCleanup", "clearShipScan", scanStart );
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "clearShipWorkUnits", run.Ships.Count );
			AdvanceSpreadOutCleanupStage( run, SpreadOutCleanupStage.ClearShips );
		}

		private static void ProcessSpreadOutShip( SpreadOutCleanupRun run )
		{
			if ( run.Index >= run.Ships.Count )
			{
				AdvanceSpreadOutCleanupStage( run, SpreadOutCleanupStage.DeleteTargets );
				return;
			}

			Item ship = run.Ships[run.Index++] as Item;

			if ( ship == null || ship.Deleted )
			{
				TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "alreadyDeletedSkipped", 1 );
				return;
			}

			ItemAttribution identity = CaptureItemAttribution( ship );
			long actionStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();

			if ( Server.Multis.BaseBoat.ClearShipTarget( ship ) )
				TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "clearShipProcessed", 1 );

			long elapsed = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( actionStart );
			TaskManagerDailyAttributionDiagnostics.RecordSubphaseElapsed( "SpreadOutCreatureCleanup", "clearShip", elapsed );
			TaskManagerDailyAttributionDiagnostics.RecordDetail( "SpreadOutCreatureCleanup", "shipType", identity.Type, elapsed );
			TaskManagerDailyAttributionDiagnostics.RecordDetail( "SpreadOutCreatureCleanup", "map", identity.Map, elapsed );
			TaskManagerDailyAttributionDiagnostics.RecordDetail( "SpreadOutCreatureCleanup", "region", identity.Region, elapsed );
			TaskManagerDailyAttributionDiagnostics.RecordWorst( "SpreadOutCreatureCleanup", "clearShip " + identity.Description, elapsed );
			RecordHeavySpreadOutOperationElapsed( "clearShip " + identity.Description, elapsed );
		}

		private static void ProcessSpreadOutMobile( SpreadOutCleanupRun run, ArrayList list, string action )
		{
			if ( run.Index >= list.Count )
			{
				AdvanceSpreadOutCleanupStage( run, GetNextSpreadOutCleanupStage( run.Stage ) );
				return;
			}

			Mobile creature = list[run.Index++] as Mobile;

			if ( creature == null || creature.Deleted )
			{
				TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "alreadyDeletedSkipped", 1 );
				return;
			}

			long actionStart = TaskManagerDailyAttributionDiagnostics.BeginTiming();
			MobileAttribution identity = CaptureMobileAttribution( creature );

			if ( action == "deleteInternalInfected" )
			{
				creature.Delete();
				RecordSpreadOutDeletion( run, "deleteInternalInfected" );
			}
			else if ( action == "deleteTarget" )
			{
				if ( !creature.Hidden )
				{
					if ( creature.WhisperHue == 911 )
					{
						Effects.SendLocationEffect( creature.Location, creature.Map, 0x3400, 60, 0x6E4, 0 );
						Effects.PlaySound( creature.Location, creature.Map, 0x108 );
					}
					else
					{
						creature.PlaySound( 0x026 );
						Effects.SendLocationEffect( creature.Location, creature.Map, 0x352D, 16, 4 );
					}
				}

				creature.Delete();
				RecordSpreadOutDeletion( run, "deleteTargets" );
			}
			else if ( action == "moveExodus" )
			{
				Server.Misc.IntelligentAction.BurnAway( creature );
				Worlds.MoveToRandomDungeon( creature );
				Server.Misc.IntelligentAction.BurnAway( creature );
				RecordSpreadOutMove( "moveExodus" );
			}
			else if ( action == "moveSerpent" )
			{
				creature.PlaySound( 0x026 );
				Effects.SendLocationEffect( creature.Location, creature.Map, 0x352D, 16, 4 );
				Worlds.MoveToRandomOcean( creature );
				creature.PlaySound( 0x026 );
				Effects.SendLocationEffect( creature.Location, creature.Map, 0x352D, 16, 4 );
				RecordSpreadOutMove( "moveSerpents" );
			}
			else if ( action == "moveGargoyle" )
			{
				Server.Misc.IntelligentAction.BurnAway( creature );
				Worlds.MoveToRandomDungeon( creature );
				Server.Misc.IntelligentAction.BurnAway( creature );
				RecordSpreadOutMove( "moveGargoyles" );
			}
			else if ( action == "deleteHealer" )
			{
				Effects.SendLocationParticles( EffectItem.Create( creature.Location, creature.Map, EffectItem.DefaultDuration ), 0x3728, 10, 10, 2023 );
				creature.PlaySound( 0x1FE );
				creature.Delete();
				RecordSpreadOutDeletion( run, "deleteHealers" );
			}

			long elapsed = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( actionStart );
			TaskManagerDailyAttributionDiagnostics.RecordSubphaseElapsed( "SpreadOutCreatureCleanup", GetSpreadOutSubphaseName( action ), elapsed );
			RecordSpreadOutAction( action, identity, actionStart );
			RecordHeavySpreadOutOperationElapsed( action + " " + identity.Description, elapsed );
		}

		private static void RecordSpreadOutDeletion( SpreadOutCleanupRun run, string subphase )
		{
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "creaturesDeleted", 1 );
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", subphase + "WorkUnits", 1 );
			TaskManagerDiagnostics.AddDeleted( run.DailyState.Diagnostics, 1 );
		}

		private static void RecordSpreadOutMove( string subphase )
		{
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "creaturesMoved", 1 );
			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", subphase + "WorkUnits", 1 );
		}

		private static void RecordHeavySpreadOutOperation( string description, long startedTimestamp )
		{
			RecordHeavySpreadOutOperationElapsed( description, TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( startedTimestamp ) );
		}

		private static void RecordHeavySpreadOutOperationElapsed( string description, long elapsed )
		{
			if ( elapsed < SpreadOutCleanupBudgetMs )
				return;

			TaskManagerDailyAttributionDiagnostics.AddCount( "SpreadOutCreatureCleanup", "heavySingleOperations", 1 );
			TaskManagerDailyAttributionDiagnostics.RecordDetail( "SpreadOutCreatureCleanup", "heavyOperation", description, elapsed );
		}

		private static void AdvanceSpreadOutCleanupStage( SpreadOutCleanupRun run, SpreadOutCleanupStage stage )
		{
			run.Stage = stage;
			run.Index = 0;
		}

		private static SpreadOutCleanupStage GetNextSpreadOutCleanupStage( SpreadOutCleanupStage stage )
		{
			switch ( stage )
			{
				case SpreadOutCleanupStage.DeleteInternalInfected: return SpreadOutCleanupStage.ScanShips;
				case SpreadOutCleanupStage.DeleteTargets: return SpreadOutCleanupStage.MoveExodus;
				case SpreadOutCleanupStage.MoveExodus: return SpreadOutCleanupStage.MoveSerpents;
				case SpreadOutCleanupStage.MoveSerpents: return SpreadOutCleanupStage.MoveGargoyles;
				case SpreadOutCleanupStage.MoveGargoyles: return SpreadOutCleanupStage.DeleteHealers;
				default: return SpreadOutCleanupStage.Complete;
			}
		}

		private static string GetSpreadOutSubphaseName( string action )
		{
			if ( action == "deleteTarget" )
				return "deleteTargets";
			if ( action == "moveExodus" )
				return "moveExodus";
			if ( action == "moveSerpent" )
				return "moveSerpents";
			if ( action == "moveGargoyle" )
				return "moveGargoyles";
			if ( action == "deleteHealer" )
				return "deleteHealers";

			return "deleteInternalInfected";
		}

		private static string GetSpreadOutCleanupStageName( SpreadOutCleanupStage stage )
		{
			switch ( stage )
			{
				case SpreadOutCleanupStage.ScanMobiles: return "scan";
				case SpreadOutCleanupStage.CreateMissingGargoyles: return "createGargoyles";
				case SpreadOutCleanupStage.DeleteInternalInfected: return "deleteInternalInfected";
				case SpreadOutCleanupStage.ScanShips: return "clearShipScan";
				case SpreadOutCleanupStage.ClearShips: return "clearShip";
				case SpreadOutCleanupStage.DeleteTargets: return "deleteTargets";
				case SpreadOutCleanupStage.MoveExodus: return "moveExodus";
				case SpreadOutCleanupStage.MoveSerpents: return "moveSerpents";
				case SpreadOutCleanupStage.MoveGargoyles: return "moveGargoyles";
				case SpreadOutCleanupStage.DeleteHealers: return "deleteHealers";
				default: return "complete";
			}
		}

		private static void CompleteSpreadOutCleanupRun( SpreadOutCleanupRun run, Exception error )
		{
			if ( run == null || run != m_ActiveSpreadOutCleanup )
				return;

			m_ActiveSpreadOutCleanup = null;
			long wallMs = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( run.WallStart );
			TaskManagerDailyAttributionDiagnostics.CompleteSpreadOutCleanupStaging( wallMs );
			TaskManagerDailyAttributionDiagnostics.RecordGroupElapsed( "SpreadOutCreatureCleanup", run.WorkMs );

			DailyRunState state = run.DailyState;
			state.WaitingForSpreadOutCleanup = false;

			run.Targets.Clear();
			run.Healers.Clear();
			run.Exodus.Clear();
			run.Serpents.Clear();
			run.Gargoyles.Clear();
			run.InternalInfected.Clear();
			run.Ships.Clear();

			QueueDailyPhase( state, DailyPipelinePhase.DrinkerCleanup, error );
		}

		private static void BeginPlantGardensPhase( DailyRunState state )
		{
			TaskManagerDiagnostics.StartPhase( state.Diagnostics, "plantGardens" );
			TaskManagerDiagnostics.MarkChunkCandidate( state.Diagnostics, "active Daily-only garden cleanup and placement work pump" );
			state.WaitingForGardens = true;

			if ( !Farms.BeginPlantGardens(
				state.Diagnostics,
				delegate( Exception error )
				{
					state.WaitingForGardens = false;
					QueueDailyPhase( state, DailyPipelinePhase.BeginPopulateCities, error );
				} ) )
			{
				state.WaitingForGardens = false;
				throw new InvalidOperationException( "Garden population is already in progress." );
			}
		}

		private static void BeginPopulateCitiesPhase( DailyRunState state )
		{
			TaskManagerDiagnostics.StartPhase( state.Diagnostics, "populateCities" );
			TaskManagerDiagnostics.MarkChunkCandidate( state.Diagnostics, "active Daily-only city population work pump; one citizen or dragon rider per constructor callback" );
			state.WaitingForCities = true;

			if ( !Server.Mobiles.Citizens.BeginPopulateCities(
				state.Diagnostics,
				delegate( Exception error )
				{
					state.WaitingForCities = false;
					QueueDailyPhase( state, DailyPipelinePhase.Finalize, error );
				} ) )
			{
				state.WaitingForCities = false;
				throw new InvalidOperationException( "Citizen population is already in progress." );
			}
		}

		private static void RunFinalDailyPhase( DailyRunState state )
		{
			TaskManagerDiagnostics.Context diag = state.Diagnostics;
			TaskManagerDiagnostics.StartPhase( diag, "wipeThiefFlags" );
			ThiefGuildmaster.WipeFlaggedList();
			TaskManagerDiagnostics.StartPhase( diag, "cleanupInternalObjects" );
			Server.Misc.AdventuresFunctions.CleanupInternalObjects( null, true );
			TaskManagerDiagnostics.StartPhase( diag, "oldCharCleanup" );
			Server.Misc.AdventuresFunctions.OldCharCleanup();
			TaskManagerDiagnostics.StartPhase( diag, "releaseOldHarvesters" );
			Server.Misc.AdventuresFunctions.ReleaseOldHarvesters();
			LoggingFunctions.LogServer( "Done - Remove Spread Out Monsters, Drinkers, And Healers" );
			Console.WriteLine( "End Daily Tasks" );
			lastrun = DateTime.UtcNow;
		}

		private static void FinishRun( DailyRunState state, Exception error )
		{
			if ( state == null )
				return;

			lock ( m_RunLock )
			{
				if ( state.Finished )
					return;

				state.Finished = true;
			}

			try
			{
				if ( state.AttributionStarted )
					TaskManagerDailyAttributionDiagnostics.EndRun( state.AttributionStart, error == null ? "complete" : "failed" );
			}
			catch ( Exception ex )
			{
				DiagnosticConsole.WriteLine( "[Lag]", DiagnosticSeverity.Error, "TaskManagerDaily attribution finalization failed: {0}: {1}", ex.GetType().Name, ex.Message );
			}
			finally
			{
				state.InvestmentBankBoxes = null;
				state.InvestmentStones = null;
				state.InvestmentBones = null;
				state.GoodChampion = null;
				state.EvilChampion = null;

				if ( state.Diagnostics != null )
				{
					TaskManagerDiagnostics.ResumeRun( state.Diagnostics );
					TaskManagerDiagnostics.EndRun( state.Diagnostics );
				}

				lock ( m_RunLock )
				{
					if ( m_ActiveRun == state )
						m_ActiveRun = null;
				}
			}

			long elapsed = TaskManagerDiagnostics.GetElapsedMilliseconds( state.StartedTimestamp );
			TaskManagerDailyAttributionDiagnostics.PerformanceSummary summary = TaskManagerDailyAttributionDiagnostics.GetPerformanceSummary();

			if ( error != null )
				DiagnosticConsole.WriteLine( "[TaskManagerDaily]", DiagnosticSeverity.Error, "run failed timer={0} elapsed={1}ms error={2}: {3}", state.TimerName, elapsed, error.GetType().Name, error.Message );
			else
				DiagnosticConsole.WriteLine(
					"[TaskManagerDaily]",
					GetCompletionSeverity( elapsed, summary ),
					"run complete timer={0} elapsed={1}ms stagedCallbacks={2} maxCallback={3}ms maxCallbackName={4}",
					state.TimerName,
					elapsed,
					summary.Available ? summary.StagedCallbackCount : 0,
					summary.Available ? summary.StagedCallbackMaxMs : 0,
					summary.Available && !String.IsNullOrWhiteSpace( summary.StagedCallbackMaxName ) ? summary.StagedCallbackMaxName : "none" );

			if ( state.Completion != null )
				state.Completion( error == null, error, elapsed );
		}

		private static DiagnosticSeverity GetCompletionSeverity( long elapsed, TaskManagerDailyAttributionDiagnostics.PerformanceSummary summary )
		{
			long maximumCallback = summary != null && summary.Available ? summary.StagedCallbackMaxMs : 0;

			if ( maximumCallback > 1000 )
				return DiagnosticSeverity.Critical;

			if ( maximumCallback > 250 || elapsed > 120000 )
				return DiagnosticSeverity.Error;

			if ( maximumCallback > 100 || elapsed > 60000 )
				return DiagnosticSeverity.Warn;

			return DiagnosticSeverity.Good;
		}

		private struct MobileAttribution
		{
			public string Type;
			public string Map;
			public string Region;
			public string Description;
		}

		private struct ItemAttribution
		{
			public string Type;
			public string Map;
			public string Region;
			public string Description;
		}

		private static void RecordSpreadOutAction( string action, Mobile mobile, long startedTimestamp )
		{
			RecordSpreadOutAction( action, CaptureMobileAttribution( mobile ), startedTimestamp );
		}

		private static void RecordSpreadOutAction( string action, MobileAttribution identity, long startedTimestamp )
		{
			long elapsed = TaskManagerDailyAttributionDiagnostics.ElapsedMilliseconds( startedTimestamp );
			TaskManagerDailyAttributionDiagnostics.RecordDetail( "SpreadOutCreatureCleanup", "action", action, elapsed );
			TaskManagerDailyAttributionDiagnostics.RecordDetail( "SpreadOutCreatureCleanup", "mobileType", identity.Type, elapsed );
			TaskManagerDailyAttributionDiagnostics.RecordDetail( "SpreadOutCreatureCleanup", "map", identity.Map, elapsed );
			TaskManagerDailyAttributionDiagnostics.RecordDetail( "SpreadOutCreatureCleanup", "region", identity.Region, elapsed );
			TaskManagerDailyAttributionDiagnostics.RecordWorst( "SpreadOutCreatureCleanup", action + " " + identity.Description, elapsed );
		}

		private static MobileAttribution CaptureMobileAttribution( Mobile mobile )
		{
			MobileAttribution identity = new MobileAttribution();

			if ( mobile == null )
			{
				identity.Type = "null";
				identity.Map = "null";
				identity.Region = "unknown";
				identity.Description = "null";
				return identity;
			}

			try
			{
				identity.Type = mobile.GetType().Name;
				identity.Map = mobile.Map != null ? mobile.Map.ToString() : "null";
				identity.Region = mobile.Region != null && !String.IsNullOrWhiteSpace( mobile.Region.Name ) ? mobile.Region.Name : "unknown";
				identity.Description = String.Format( "{0} map={1} region={2} loc={3}", identity.Type, identity.Map, identity.Region, mobile.Location );
			}
			catch ( Exception ex )
			{
				identity.Type = mobile.GetType().Name;
				identity.Map = "unavailable";
				identity.Region = "unavailable";
				identity.Description = identity.Type + " identifiers unavailable";
				TaskManagerDailyAttributionDiagnostics.RecordError( "SpreadOutCreatureCleanup", ex );
			}

			return identity;
		}

		private static ItemAttribution CaptureItemAttribution( Item item )
		{
			ItemAttribution identity = new ItemAttribution();

			if ( item == null )
			{
				identity.Type = "null";
				identity.Map = "null";
				identity.Region = "unknown";
				identity.Description = "null";
				return identity;
			}

			try
			{
				identity.Type = item.GetType().Name;
				identity.Map = item.Map != null ? item.Map.ToString() : "null";
				Region region = item.Map != null ? Region.Find( item.Location, item.Map ) : null;
				identity.Region = region != null && !String.IsNullOrWhiteSpace( region.Name ) ? region.Name : "unknown";
				identity.Description = String.Format( "{0} map={1} region={2} loc={3}", identity.Type, identity.Map, identity.Region, item.Location );
			}
			catch ( Exception ex )
			{
				identity.Type = item.GetType().Name;
				identity.Map = "unavailable";
				identity.Region = "unavailable";
				identity.Description = identity.Type + " identifiers unavailable";
				TaskManagerDailyAttributionDiagnostics.RecordError( "SpreadOutCreatureCleanup", ex );
			}

			return identity;
		}

	}
}
