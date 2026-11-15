using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Server.Diagnostics
{
	public static class TaskManagerDailyAttributionDiagnostics
	{
		private const int MaxGroups = 16;
		private const int MaxCountersPerGroup = 64;
		private const int MaxSubphasesPerGroup = 32;
		private const int MaxDetailsPerGroup = 128;
		private const long SlowGroupThresholdMs = 250;

		private static readonly object m_Lock = new object();

		[ThreadStatic]
		private static RunSample m_Active;

		private static RunSample m_Current;
		private static RunSample m_Last;
		private static RunSample m_Worst;
		private static RunSample m_SinceBoot = new RunSample();

		public sealed class PerformanceSummary
		{
			public bool Available;
			public long RunId;
			public DateTime StartedUtc;
			public string TimerName;
			public bool Startup;
			public bool Forced;
			public bool VillagesBeforeDueCheck;
			public bool DueEvaluated;
			public bool Due;
			public string CompletionMode;
			public string CompletionState;
			public long TotalMs;
			public long VillageChunkCount;
			public long VillageChunkTotalMs;
			public long VillageChunkMaxMs;
			public long VillageStagedWallMs;
			public long CityChunkCount;
			public long CityChunkTotalMs;
			public long CityChunkMaxMs;
			public long CityStagedWallMs;
			public long GardenChunkCount;
			public long GardenChunkTotalMs;
			public long GardenChunkMaxMs;
			public long GardenStagedWallMs;
			public long GardenPlacementCallbackCount;
			public long GardenPlacementCallbackTotalMs;
			public long GardenPlacementCallbackMaxMs;
			public long GardenPlacementWallMs;
			public long SpreadOutCleanupCallbackCount;
			public long SpreadOutCleanupCallbackTotalMs;
			public long SpreadOutCleanupCallbackMaxMs;
			public long SpreadOutCleanupWallMs;
			public string SpreadOutCleanupCallbackMaxName;
			public string SpreadOutCleanupCallbackMaxKind;
			public long StealingPedestalCallbackCount;
			public long StealingPedestalCallbackTotalMs;
			public long StealingPedestalCallbackMaxMs;
			public long StealingPedestalWallMs;
			public string StealingPedestalCallbackMaxName;
			public string StealingPedestalCallbackMaxKind;
			public long StagedCallbackCount;
			public long StagedCallbackTotalMs;
			public long StagedCallbackMaxMs;
			public string StagedCallbackMaxName;
			public string StagedCallbackMaxKind;
			public string CurrentPhase;
			public long VillageConstructorMs;
			public long VillageNonConstructorMs;
			public long PopulateVillagesMs;
			public long PopulateVillagesSetupSpawnMs;
			public long PopulateCitiesMs;
			public long PlantGardensMs;
			public long SpreadOutCreatureCleanupMs;
			public long StealingPedestalsMs;
			public long InvestmentAetherCurseMs;
			public long InvestmentItemScanMs;
			public long InvestmentPayoutMs;
			public long InvestmentStoneBonePopulateMs;
			public long BalanceRunMs;
			public long ChampionAnnouncementMs;
			public long InfectionCheckMs;
			public long InvasionRoutineMs;
			public string TopGroup;
			public long TopGroupMs;
		}

		private sealed class RunSample
		{
			public long RunId;
			public long RunCount;
			public bool Startup;
			public bool Forced;
			public DateTime StartedUtc;
			public long StartedTimestamp;
			public string TimerName;
			public bool VillagesBeforeDueCheck;
			public bool DueEvaluated;
			public bool Due;
			public string CompletionMode;
			public string CompletionState;
			public long VillageChunkCount;
			public long VillageChunkTotalMs;
			public long VillageChunkMaxMs;
			public long VillageStagedWallMs;
			public long CityChunkCount;
			public long CityChunkTotalMs;
			public long CityChunkMaxMs;
			public long CityStagedWallMs;
			public long GardenChunkCount;
			public long GardenChunkTotalMs;
			public long GardenChunkMaxMs;
			public long GardenStagedWallMs;
			public long GardenPlacementCallbackCount;
			public long GardenPlacementCallbackTotalMs;
			public long GardenPlacementCallbackMaxMs;
			public long GardenPlacementWallMs;
			public long SpreadOutCleanupCallbackCount;
			public long SpreadOutCleanupCallbackTotalMs;
			public long SpreadOutCleanupCallbackMaxMs;
			public long SpreadOutCleanupWallMs;
			public string SpreadOutCleanupCallbackMaxName;
			public string SpreadOutCleanupCallbackMaxKind;
			public long StealingPedestalCallbackCount;
			public long StealingPedestalCallbackTotalMs;
			public long StealingPedestalCallbackMaxMs;
			public long StealingPedestalWallMs;
			public string StealingPedestalCallbackMaxName;
			public string StealingPedestalCallbackMaxKind;
			public long StagedCallbackCount;
			public long StagedCallbackTotalMs;
			public long StagedCallbackMaxMs;
			public string StagedCallbackMaxName;
			public string StagedCallbackMaxKind;
			public string CurrentPhase;
			public long TotalMs;
			public long ErrorCount;
			public readonly Dictionary<string, GroupStat> Groups = new Dictionary<string, GroupStat>();

			public RunSample Clone()
			{
				RunSample copy = new RunSample();
				copy.RunId = RunId;
				copy.RunCount = RunCount;
				copy.Startup = Startup;
				copy.Forced = Forced;
				copy.StartedUtc = StartedUtc;
				copy.StartedTimestamp = StartedTimestamp;
				copy.TimerName = TimerName;
				copy.VillagesBeforeDueCheck = VillagesBeforeDueCheck;
				copy.DueEvaluated = DueEvaluated;
				copy.Due = Due;
				copy.CompletionMode = CompletionMode;
				copy.CompletionState = CompletionState;
				copy.VillageChunkCount = VillageChunkCount;
				copy.VillageChunkTotalMs = VillageChunkTotalMs;
				copy.VillageChunkMaxMs = VillageChunkMaxMs;
				copy.VillageStagedWallMs = VillageStagedWallMs;
				copy.CityChunkCount = CityChunkCount;
				copy.CityChunkTotalMs = CityChunkTotalMs;
				copy.CityChunkMaxMs = CityChunkMaxMs;
				copy.CityStagedWallMs = CityStagedWallMs;
				copy.GardenChunkCount = GardenChunkCount;
				copy.GardenChunkTotalMs = GardenChunkTotalMs;
				copy.GardenChunkMaxMs = GardenChunkMaxMs;
				copy.GardenStagedWallMs = GardenStagedWallMs;
				copy.GardenPlacementCallbackCount = GardenPlacementCallbackCount;
				copy.GardenPlacementCallbackTotalMs = GardenPlacementCallbackTotalMs;
				copy.GardenPlacementCallbackMaxMs = GardenPlacementCallbackMaxMs;
				copy.GardenPlacementWallMs = GardenPlacementWallMs;
				copy.SpreadOutCleanupCallbackCount = SpreadOutCleanupCallbackCount;
				copy.SpreadOutCleanupCallbackTotalMs = SpreadOutCleanupCallbackTotalMs;
				copy.SpreadOutCleanupCallbackMaxMs = SpreadOutCleanupCallbackMaxMs;
				copy.SpreadOutCleanupWallMs = SpreadOutCleanupWallMs;
				copy.SpreadOutCleanupCallbackMaxName = SpreadOutCleanupCallbackMaxName;
				copy.SpreadOutCleanupCallbackMaxKind = SpreadOutCleanupCallbackMaxKind;
				copy.StealingPedestalCallbackCount = StealingPedestalCallbackCount;
				copy.StealingPedestalCallbackTotalMs = StealingPedestalCallbackTotalMs;
				copy.StealingPedestalCallbackMaxMs = StealingPedestalCallbackMaxMs;
				copy.StealingPedestalWallMs = StealingPedestalWallMs;
				copy.StealingPedestalCallbackMaxName = StealingPedestalCallbackMaxName;
				copy.StealingPedestalCallbackMaxKind = StealingPedestalCallbackMaxKind;
				copy.StagedCallbackCount = StagedCallbackCount;
				copy.StagedCallbackTotalMs = StagedCallbackTotalMs;
				copy.StagedCallbackMaxMs = StagedCallbackMaxMs;
				copy.StagedCallbackMaxName = StagedCallbackMaxName;
				copy.StagedCallbackMaxKind = StagedCallbackMaxKind;
				copy.CurrentPhase = CurrentPhase;
				copy.TotalMs = TotalMs;
				copy.ErrorCount = ErrorCount;

				foreach ( KeyValuePair<string, GroupStat> entry in Groups )
					copy.Groups[entry.Key] = entry.Value.Clone();

				return copy;
			}
		}

		private sealed class GroupStat
		{
			public string Name;
			public long Calls;
			public long TotalMs;
			public long MaxMs;
			public long LastMs;
			public long SlowCount;
			public long ErrorCount;
			public string WorstDescription;
			public long WorstDescriptionMs;
			public readonly Dictionary<string, long> Counters = new Dictionary<string, long>();
			public readonly Dictionary<string, DetailStat> Subphases = new Dictionary<string, DetailStat>();
			public readonly Dictionary<string, DetailStat> Details = new Dictionary<string, DetailStat>();

			public GroupStat Clone()
			{
				GroupStat copy = new GroupStat();
				copy.Name = Name;
				copy.Calls = Calls;
				copy.TotalMs = TotalMs;
				copy.MaxMs = MaxMs;
				copy.LastMs = LastMs;
				copy.SlowCount = SlowCount;
				copy.ErrorCount = ErrorCount;
				copy.WorstDescription = WorstDescription;
				copy.WorstDescriptionMs = WorstDescriptionMs;

				foreach ( KeyValuePair<string, long> entry in Counters )
					copy.Counters[entry.Key] = entry.Value;

				foreach ( KeyValuePair<string, DetailStat> entry in Subphases )
					copy.Subphases[entry.Key] = entry.Value.Clone();

				foreach ( KeyValuePair<string, DetailStat> entry in Details )
					copy.Details[entry.Key] = entry.Value.Clone();

				return copy;
			}
		}

		private sealed class DetailStat
		{
			public string Name;
			public long Count;
			public long TotalMs;
			public long MaxMs;
			public long LastMs;

			public DetailStat Clone()
			{
				DetailStat copy = new DetailStat();
				copy.Name = Name;
				copy.Count = Count;
				copy.TotalMs = TotalMs;
				copy.MaxMs = MaxMs;
				copy.LastMs = LastMs;
				return copy;
			}
		}

		public static void BeginRun( long runId, bool startup, DateTime startedUtc )
		{
			BeginRun( runId, startup, startedUtc, "unknown", false );
		}

		public static void BeginRun( long runId, bool startup, DateTime startedUtc, string timerName, bool forced )
		{
			RunSample sample = new RunSample();
			sample.RunId = runId;
			sample.RunCount = 1;
			sample.Startup = startup;
			sample.Forced = forced;
			sample.StartedUtc = startedUtc;
			sample.StartedTimestamp = Stopwatch.GetTimestamp();
			sample.TimerName = !String.IsNullOrWhiteSpace( timerName ) ? timerName : "unknown";
			sample.CompletionMode = "synchronous";
			sample.CompletionState = "in progress";
			m_Active = sample;
		}

		public static void EndRun( long startedTimestamp )
		{
			EndRun( startedTimestamp, "complete" );
		}

		public static void EndRun( long startedTimestamp, string completionState )
		{
			RunSample sample = m_Active;
			m_Active = null;

			if ( sample == null )
				return;

			sample.TotalMs = ElapsedMilliseconds( startedTimestamp );
			sample.CompletionState = !String.IsNullOrWhiteSpace( completionState ) ? completionState : "complete";

			lock ( m_Lock )
			{
				m_Last = m_Current != null ? m_Current.Clone() : null;
				m_Current = sample.Clone();

				if ( m_Worst == null || sample.TotalMs > m_Worst.TotalMs )
					m_Worst = sample.Clone();

				MergeRun( m_SinceBoot, sample );
			}
		}

		public static void MarkVillagesBeforeDueCheck()
		{
			if ( m_Active != null )
				m_Active.VillagesBeforeDueCheck = true;
		}

		public static void MarkDueDecision( bool due )
		{
			if ( m_Active == null )
				return;

			m_Active.DueEvaluated = true;
			m_Active.Due = due;
		}

		public static void MarkStaged()
		{
			if ( m_Active != null )
				m_Active.CompletionMode = "staged";
		}

		public static void RecordVillageChunk( long elapsedMs )
		{
			if ( m_Active == null )
				return;

			++m_Active.VillageChunkCount;
			m_Active.VillageChunkTotalMs += elapsedMs;

			if ( elapsedMs > m_Active.VillageChunkMaxMs )
				m_Active.VillageChunkMaxMs = elapsedMs;
		}

		public static void CompleteVillageStaging( long wallMs )
		{
			if ( m_Active != null )
				m_Active.VillageStagedWallMs = wallMs;
		}

		public static void SetCurrentPhase( string phase )
		{
			if ( m_Active != null )
				m_Active.CurrentPhase = !String.IsNullOrWhiteSpace( phase ) ? phase : "unknown";
		}

		public static void RecordCityChunk( long elapsedMs )
		{
			if ( m_Active == null )
				return;

			++m_Active.CityChunkCount;
			m_Active.CityChunkTotalMs += elapsedMs;

			if ( elapsedMs > m_Active.CityChunkMaxMs )
				m_Active.CityChunkMaxMs = elapsedMs;
		}

		public static void CompleteCityStaging( long wallMs )
		{
			if ( m_Active != null )
				m_Active.CityStagedWallMs = wallMs;
		}

		public static void RecordGardenChunk( long elapsedMs )
		{
			if ( m_Active == null )
				return;

			++m_Active.GardenChunkCount;
			m_Active.GardenChunkTotalMs += elapsedMs;

			if ( elapsedMs > m_Active.GardenChunkMaxMs )
				m_Active.GardenChunkMaxMs = elapsedMs;
		}

		public static void CompleteGardenStaging( long wallMs )
		{
			if ( m_Active != null )
				m_Active.GardenStagedWallMs = wallMs;
		}

		public static void RecordGardenPlacementCallback( long elapsedMs )
		{
			if ( m_Active == null )
				return;

			++m_Active.GardenPlacementCallbackCount;
			m_Active.GardenPlacementCallbackTotalMs += elapsedMs;

			if ( elapsedMs > m_Active.GardenPlacementCallbackMaxMs )
				m_Active.GardenPlacementCallbackMaxMs = elapsedMs;
		}

		public static void CompleteGardenPlacementStaging( long wallMs )
		{
			if ( m_Active != null )
				m_Active.GardenPlacementWallMs = wallMs;
		}

		public static void RecordSpreadOutCleanupCallback( string callbackName, string callbackKind, long elapsedMs )
		{
			RunSample sample = m_Active;

			if ( sample == null )
				return;

			++sample.SpreadOutCleanupCallbackCount;
			sample.SpreadOutCleanupCallbackTotalMs += elapsedMs;

			if ( elapsedMs > sample.SpreadOutCleanupCallbackMaxMs )
			{
				sample.SpreadOutCleanupCallbackMaxMs = elapsedMs;
				sample.SpreadOutCleanupCallbackMaxName = SafeName( callbackName );
				sample.SpreadOutCleanupCallbackMaxKind = SafeName( callbackKind );
			}

			AddCount( "SpreadOutCreatureCleanup", SafeName( callbackKind ) + "Callbacks", 1 );
			RecordDetail( "SpreadOutCreatureCleanup", "callback", callbackKind, elapsedMs );
		}

		public static void CompleteSpreadOutCleanupStaging( long wallMs )
		{
			if ( m_Active != null )
				m_Active.SpreadOutCleanupWallMs = wallMs;
		}

		public static void RecordStealingPedestalCallback( string callbackName, string callbackKind, long elapsedMs )
		{
			RunSample sample = m_Active;

			if ( sample == null )
				return;

			++sample.StealingPedestalCallbackCount;
			sample.StealingPedestalCallbackTotalMs += elapsedMs;

			if ( elapsedMs > sample.StealingPedestalCallbackMaxMs )
			{
				sample.StealingPedestalCallbackMaxMs = elapsedMs;
				sample.StealingPedestalCallbackMaxName = SafeName( callbackName );
				sample.StealingPedestalCallbackMaxKind = SafeName( callbackKind );
			}
		}

		public static void CompleteStealingPedestalStaging( long wallMs )
		{
			if ( m_Active != null )
				m_Active.StealingPedestalWallMs = wallMs;
		}

		public static void RecordStagedCallback( string callbackName, string callbackKind, long elapsedMs )
		{
			RunSample sample = m_Active;

			if ( sample == null )
				return;

			++sample.StagedCallbackCount;
			sample.StagedCallbackTotalMs += elapsedMs;

			if ( elapsedMs > sample.StagedCallbackMaxMs )
			{
				sample.StagedCallbackMaxMs = elapsedMs;
				sample.StagedCallbackMaxName = SafeName( callbackName );
				sample.StagedCallbackMaxKind = SafeName( callbackKind );
			}

			RecordDetail( "Daily staged callbacks", "phase", callbackName, elapsedMs );
			RecordWorst(
				"Daily staged callbacks",
				String.Format( "{0} kind={1}", callbackName, callbackKind ),
				elapsedMs );
		}

		public static long BeginTiming()
		{
			return Stopwatch.GetTimestamp();
		}

		public static long ElapsedMilliseconds( long startedTimestamp )
		{
			if ( startedTimestamp <= 0 )
				return 0;

			return ( Stopwatch.GetTimestamp() - startedTimestamp ) * 1000 / Stopwatch.Frequency;
		}

		public static void RecordGroup( string groupName, long startedTimestamp )
		{
			RecordGroupElapsed( groupName, ElapsedMilliseconds( startedTimestamp ) );
		}

		public static void RecordGroupElapsed( string groupName, long elapsedMs )
		{
			GroupStat group = GetActiveGroup( groupName );

			if ( group == null )
				return;

			++group.Calls;
			group.TotalMs += elapsedMs;
			group.LastMs = elapsedMs;

			if ( elapsedMs > group.MaxMs )
				group.MaxMs = elapsedMs;

			if ( elapsedMs >= SlowGroupThresholdMs )
				++group.SlowCount;
		}

		public static void RecordSubphase( string groupName, string subphaseName, long startedTimestamp )
		{
			RecordSubphaseElapsed( groupName, subphaseName, ElapsedMilliseconds( startedTimestamp ) );
		}

		public static void RecordSubphaseElapsed( string groupName, string subphaseName, long elapsedMs )
		{
			GroupStat group = GetActiveGroup( groupName );

			if ( group == null )
				return;

			AddDetail( group.Subphases, MaxSubphasesPerGroup, subphaseName, elapsedMs );
		}

		public static void RecordDetail( string groupName, string category, string key, long elapsedMs )
		{
			GroupStat group = GetActiveGroup( groupName );

			if ( group == null )
				return;

			string detailName = SafeName( category ) + ":" + SafeName( key );
			AddDetail( group.Details, MaxDetailsPerGroup, detailName, elapsedMs );
		}

		public static void AddCount( string groupName, string counterName, long count )
		{
			if ( count == 0 )
				return;

			GroupStat group = GetActiveGroup( groupName );

			if ( group == null )
				return;

			long value;

			if ( group.Counters.TryGetValue( counterName, out value ) )
			{
				group.Counters[counterName] = value + count;
			}
			else if ( group.Counters.Count < MaxCountersPerGroup )
			{
				group.Counters[counterName] = count;
			}
			else
			{
				if ( group.Counters.TryGetValue( "droppedCounters", out value ) )
					group.Counters["droppedCounters"] = value + 1;
			}
		}

		public static void RecordWorst( string groupName, string description, long elapsedMs )
		{
			GroupStat group = GetActiveGroup( groupName );

			if ( group == null || elapsedMs <= group.WorstDescriptionMs )
				return;

			group.WorstDescriptionMs = elapsedMs;
			group.WorstDescription = SafeName( description );
		}

		public static void RecordError( string groupName, Exception error )
		{
			RunSample sample = m_Active;
			GroupStat group = GetActiveGroup( groupName );

			if ( sample != null )
				++sample.ErrorCount;

			if ( group == null )
				return;

			++group.ErrorCount;

			if ( error != null )
				RecordDetail( groupName, "exception", error.GetType().FullName, 0 );
		}

		public static PerformanceSummary GetPerformanceSummary()
		{
			RunSample sample;

			lock ( m_Lock )
				sample = m_Current != null ? m_Current.Clone() : null;

			PerformanceSummary summary = new PerformanceSummary();

			if ( sample == null )
				return summary;

			summary.Available = true;
			summary.RunId = sample.RunId;
			summary.StartedUtc = sample.StartedUtc;
			summary.TimerName = sample.TimerName;
			summary.Startup = sample.Startup;
			summary.Forced = sample.Forced;
			summary.VillagesBeforeDueCheck = sample.VillagesBeforeDueCheck;
			summary.DueEvaluated = sample.DueEvaluated;
			summary.Due = sample.Due;
			summary.CompletionMode = sample.CompletionMode;
			summary.CompletionState = sample.CompletionState;
			summary.TotalMs = sample.TotalMs;
			summary.VillageChunkCount = sample.VillageChunkCount;
			summary.VillageChunkTotalMs = sample.VillageChunkTotalMs;
			summary.VillageChunkMaxMs = sample.VillageChunkMaxMs;
			summary.VillageStagedWallMs = sample.VillageStagedWallMs;
			summary.CityChunkCount = sample.CityChunkCount;
			summary.CityChunkTotalMs = sample.CityChunkTotalMs;
			summary.CityChunkMaxMs = sample.CityChunkMaxMs;
			summary.CityStagedWallMs = sample.CityStagedWallMs;
			summary.GardenChunkCount = sample.GardenChunkCount;
			summary.GardenChunkTotalMs = sample.GardenChunkTotalMs;
			summary.GardenChunkMaxMs = sample.GardenChunkMaxMs;
			summary.GardenStagedWallMs = sample.GardenStagedWallMs;
			summary.GardenPlacementCallbackCount = sample.GardenPlacementCallbackCount;
			summary.GardenPlacementCallbackTotalMs = sample.GardenPlacementCallbackTotalMs;
			summary.GardenPlacementCallbackMaxMs = sample.GardenPlacementCallbackMaxMs;
			summary.GardenPlacementWallMs = sample.GardenPlacementWallMs;
			summary.SpreadOutCleanupCallbackCount = sample.SpreadOutCleanupCallbackCount;
			summary.SpreadOutCleanupCallbackTotalMs = sample.SpreadOutCleanupCallbackTotalMs;
			summary.SpreadOutCleanupCallbackMaxMs = sample.SpreadOutCleanupCallbackMaxMs;
			summary.SpreadOutCleanupWallMs = sample.SpreadOutCleanupWallMs;
			summary.SpreadOutCleanupCallbackMaxName = sample.SpreadOutCleanupCallbackMaxName;
			summary.SpreadOutCleanupCallbackMaxKind = sample.SpreadOutCleanupCallbackMaxKind;
			summary.StealingPedestalCallbackCount = sample.StealingPedestalCallbackCount;
			summary.StealingPedestalCallbackTotalMs = sample.StealingPedestalCallbackTotalMs;
			summary.StealingPedestalCallbackMaxMs = sample.StealingPedestalCallbackMaxMs;
			summary.StealingPedestalWallMs = sample.StealingPedestalWallMs;
			summary.StealingPedestalCallbackMaxName = sample.StealingPedestalCallbackMaxName;
			summary.StealingPedestalCallbackMaxKind = sample.StealingPedestalCallbackMaxKind;
			summary.StagedCallbackCount = sample.StagedCallbackCount;
			summary.StagedCallbackTotalMs = sample.StagedCallbackTotalMs;
			summary.StagedCallbackMaxMs = sample.StagedCallbackMaxMs;
			summary.StagedCallbackMaxName = sample.StagedCallbackMaxName;
			summary.StagedCallbackMaxKind = sample.StagedCallbackMaxKind;
			summary.CurrentPhase = sample.CurrentPhase;
			summary.PopulateVillagesMs = GetGroupMilliseconds( sample, "PopulateVillages" );
			summary.PopulateVillagesSetupSpawnMs = GetGroupMilliseconds( sample, "PopulateVillages setupSpawn" );
			summary.VillageConstructorMs = GetSubphaseMilliseconds( sample, "PopulateVillages setupSpawn", "constructor" );
			summary.VillageNonConstructorMs = Math.Max( 0, summary.PopulateVillagesSetupSpawnMs - summary.VillageConstructorMs );
			summary.PopulateCitiesMs = GetGroupMilliseconds( sample, "PopulateCities" );
			summary.PlantGardensMs = GetGroupMilliseconds( sample, "PlantGardens" );
			summary.SpreadOutCreatureCleanupMs = GetGroupMilliseconds( sample, "SpreadOutCreatureCleanup" );
			summary.StealingPedestalsMs = GetGroupMilliseconds( sample, "StealingPedestals" );
			summary.InvestmentAetherCurseMs = GetSubphaseMilliseconds( sample, "InvestmentsAndBalance", "aetherCurse" );
			summary.InvestmentItemScanMs = GetSubphaseMilliseconds( sample, "InvestmentsAndBalance", "investmentItemScan" );
			summary.InvestmentPayoutMs = GetSubphaseMilliseconds( sample, "InvestmentsAndBalance", "investmentPayout" );
			summary.InvestmentStoneBonePopulateMs = GetSubphaseMilliseconds( sample, "InvestmentsAndBalance", "stoneBonePopulate" );
			summary.BalanceRunMs = GetSubphaseMilliseconds( sample, "BalanceAndChampions", "balanceRun" );
			summary.ChampionAnnouncementMs = GetSubphaseMilliseconds( sample, "BalanceAndChampions", "championAnnouncements" );
			summary.InfectionCheckMs = GetSubphaseMilliseconds( sample, "BalanceAndChampions", "infectionCheck" );
			summary.InvasionRoutineMs = GetSubphaseMilliseconds( sample, "BalanceAndChampions", "invasionRoutine" );

			GroupStat top = GetTopGroup( sample );

			if ( top != null )
			{
				summary.TopGroup = top.Name;
				summary.TopGroupMs = top.TotalMs;
			}

			return summary;
		}

		public static void Reset()
		{
			lock ( m_Lock )
			{
				m_Current = null;
				m_Last = null;
				m_Worst = null;
				m_SinceBoot = new RunSample();
			}

			m_Active = null;
		}

		public static void WriteStats( TextWriter writer )
		{
			if ( writer == null )
				return;

			RunSample current;
			RunSample last;
			RunSample worst;
			RunSample sinceBoot;
			RunSample active = m_Active != null ? m_Active.Clone() : null;

			if ( active != null )
				active.TotalMs = ElapsedMilliseconds( active.StartedTimestamp );

			lock ( m_Lock )
			{
				current = m_Current != null ? m_Current.Clone() : null;
				last = m_Last != null ? m_Last.Clone() : null;
				worst = m_Worst != null ? m_Worst.Clone() : null;
				sinceBoot = m_SinceBoot.Clone();
			}

			WriteSummary( writer, active != null ? active : current );
			WriteRun( writer, "active", active );
			WriteRun( writer, "current", current );
			WriteRun( writer, "last", last );
			WriteRun( writer, "worst", worst );
			WriteRun( writer, "sinceBoot", sinceBoot.Groups.Count > 0 ? sinceBoot : null );
		}

		public static void WriteSummary( TextWriter writer )
		{
			if ( writer == null )
				return;

			RunSample current;

			lock ( m_Lock )
				current = m_Current != null ? m_Current.Clone() : null;

			WriteSummary( writer, current );
		}

		private static void WriteSummary( TextWriter writer, RunSample sample )
		{
			if ( sample == null )
			{
				writer.WriteLine( "TaskManagerDaily Attribution: Not sampled" );
				return;
			}

			GroupStat topTotal = GetTopGroup( sample );
			GroupStat topMaximum = GetTopGroupByMaximum( sample );
			string topSubphaseTotalGroup;
			string topSubphaseMaximumGroup;
			DetailStat topSubphaseTotal = GetTopSubphase( sample, false, out topSubphaseTotalGroup );
			DetailStat topSubphaseMaximum = GetTopSubphase( sample, true, out topSubphaseMaximumGroup );
			GroupStat worstGroup = GetWorstSampleGroup( sample );
			DetailStat topCreatedType = GetTopCategorizedDetail(
				sample,
				new string[] { "spawnedType", "constructorType", "accessoryType", "plantType" } );
			DetailStat topRegionMap = GetTopCategorizedDetail(
				sample,
				new string[] { "region", "map", "dragonRiderMap" } );

			writer.WriteLine(
				"TaskManagerDaily Attribution: runId={0} timer={1} startup={2} forced={3} startedUtc={4} total={5}ms mode={6} state={7} phase={8} villagesBeforeDueCheck={9} dueEvaluated={10} due={11} errors={12}",
				sample.RunId,
				sample.TimerName,
				sample.Startup,
				sample.Forced,
				sample.StartedUtc,
				sample.TotalMs,
				!String.IsNullOrWhiteSpace( sample.CompletionMode ) ? sample.CompletionMode : "unknown",
				!String.IsNullOrWhiteSpace( sample.CompletionState ) ? sample.CompletionState : "unknown",
				!String.IsNullOrWhiteSpace( sample.CurrentPhase ) ? sample.CurrentPhase : "none",
				sample.VillagesBeforeDueCheck,
				sample.DueEvaluated,
				sample.Due,
				sample.ErrorCount );
			writer.WriteLine(
				"  villageCallbacks: callbacks={0} work={1}ms maxCallback={2}ms avgCallback={3}ms stagedWall={4}ms constructor={5}ms nonConstructor={6}ms",
				sample.VillageChunkCount,
				sample.VillageChunkTotalMs,
				sample.VillageChunkMaxMs,
				sample.VillageChunkCount > 0 ? sample.VillageChunkTotalMs / sample.VillageChunkCount : 0,
				sample.VillageStagedWallMs,
				GetSubphaseMilliseconds( sample, "PopulateVillages setupSpawn", "constructor" ),
				Math.Max( 0, GetGroupMilliseconds( sample, "PopulateVillages setupSpawn" ) - GetSubphaseMilliseconds( sample, "PopulateVillages setupSpawn", "constructor" ) ) );
			writer.WriteLine(
				"  cityStaging: callbacks={0} work={1}ms maxCallback={2}ms avgCallback={3}ms stagedWall={4}ms",
				sample.CityChunkCount,
				sample.CityChunkTotalMs,
				sample.CityChunkMaxMs,
				sample.CityChunkCount > 0 ? sample.CityChunkTotalMs / sample.CityChunkCount : 0,
				sample.CityStagedWallMs );
			writer.WriteLine(
				"  gardenStaging: callbacks={0} work={1}ms maxCallback={2}ms avgCallback={3}ms stagedWall={4}ms",
				sample.GardenChunkCount,
				sample.GardenChunkTotalMs,
				sample.GardenChunkMaxMs,
				sample.GardenChunkCount > 0 ? sample.GardenChunkTotalMs / sample.GardenChunkCount : 0,
				sample.GardenStagedWallMs );
			writer.WriteLine(
				"  gardenPlacement: entries={0}/{1} randomRolls={2} plantsCreated={3} callbacks={4} work={5}ms maxCallback={6}ms avgCallback={7}ms wall={8}ms heavyOutliers={9}",
				GetCounter( sample, "PlantGardens", "placementEntriesProcessed" ),
				GetCounter( sample, "PlantGardens", "placementEntriesTotal" ),
				GetCounter( sample, "PlantGardens", "randomRollsPerformed" ),
				GetCounter( sample, "PlantGardens", "plantsPlaced" ),
				sample.GardenPlacementCallbackCount,
				sample.GardenPlacementCallbackTotalMs,
				sample.GardenPlacementCallbackMaxMs,
				sample.GardenPlacementCallbackCount > 0 ? sample.GardenPlacementCallbackTotalMs / sample.GardenPlacementCallbackCount : 0,
				sample.GardenPlacementWallMs,
				GetCounter( sample, "PlantGardens", "heavySingleOperations" ) );
			WriteSpreadOutCleanupStaging( writer, sample );
			writer.WriteLine(
				"  stealingPedestalStaging: callbacks={0} work={1}ms maxCallback={2}ms avgCallback={3}ms stagedWall={4}ms callback={5} kind={6} entries={7} created={8} deleted={9} scans={10} randomPlacements={11}",
				sample.StealingPedestalCallbackCount,
				sample.StealingPedestalCallbackTotalMs,
				sample.StealingPedestalCallbackMaxMs,
				sample.StealingPedestalCallbackCount > 0 ? sample.StealingPedestalCallbackTotalMs / sample.StealingPedestalCallbackCount : 0,
				sample.StealingPedestalWallMs,
				!String.IsNullOrWhiteSpace( sample.StealingPedestalCallbackMaxName ) ? sample.StealingPedestalCallbackMaxName : "none",
				!String.IsNullOrWhiteSpace( sample.StealingPedestalCallbackMaxKind ) ? sample.StealingPedestalCallbackMaxKind : "none",
				GetCounter( sample, "StealingPedestals", "pedestalEntriesProcessed" ),
				GetCounter( sample, "StealingPedestals", "pedestalsCreated" ),
				GetCounter( sample, "StealingPedestals", "pedestalsDeleted" ),
				GetCounter( sample, "StealingPedestals", "itemsScanned" ),
				GetCounter( sample, "StealingPedestals", "randomPlacements" ) );
			writer.WriteLine(
				"  actualStagedCallbacks: count={0} total={1}ms max={2}ms avg={3}ms callback={4} kind={5}",
				sample.StagedCallbackCount,
				sample.StagedCallbackTotalMs,
				sample.StagedCallbackMaxMs,
				sample.StagedCallbackCount > 0 ? sample.StagedCallbackTotalMs / sample.StagedCallbackCount : 0,
				!String.IsNullOrWhiteSpace( sample.StagedCallbackMaxName ) ? sample.StagedCallbackMaxName : "none",
				!String.IsNullOrWhiteSpace( sample.StagedCallbackMaxKind ) ? sample.StagedCallbackMaxKind : "none" );
			writer.WriteLine(
				"  leaders: topGroupTotal={0} topGroupMax={1} topSubphaseTotal={2} topSubphaseMax={3}",
				FormatGroupSummary( topTotal, false ),
				FormatGroupSummary( topMaximum, true ),
				FormatQualifiedDetailSummary( topSubphaseTotalGroup, topSubphaseTotal, false ),
				FormatQualifiedDetailSummary( topSubphaseMaximumGroup, topSubphaseMaximum, true ) );

			WriteGroupSummary( writer, sample, "PopulateVillages setupSpawn" );
			WriteGroupSummary( writer, sample, "PopulateCities" );
			WriteGroupSummary( writer, sample, "Daily staged callbacks" );
			WriteGroupSummary( writer, sample, "PlantGardens" );
			WriteGroupSummary( writer, sample, "SpreadOutCreatureCleanup" );
			WriteGroupSummary( writer, sample, "StealingPedestals" );
			WriteGroupSummary( writer, sample, "InvestmentsAndBalance" );
			WriteGroupSummary( writer, sample, "BalanceAndChampions" );

			writer.WriteLine(
				"  exactWorst={0} topCreatedType={1} topRegionMap={2}",
				worstGroup != null && !String.IsNullOrWhiteSpace( worstGroup.WorstDescription )
					? String.Format( "{0}/{1}:{2}ms", worstGroup.Name, worstGroup.WorstDescription, worstGroup.WorstDescriptionMs )
					: "none",
				FormatDetailSummary( topCreatedType, false ),
				FormatDetailSummary( topRegionMap, false ) );
			writer.WriteLine( "  note: subphase totals are passive attribution; remaining phase time is reported as other/unattributed." );
		}

		private static void WriteGroupSummary( TextWriter writer, RunSample sample, string groupName )
		{
			GroupStat group;

			if ( sample == null || !sample.Groups.TryGetValue( groupName, out group ) )
			{
				writer.WriteLine( "  {0}: Not sampled", groupName );
				return;
			}

			long attributedMs = 0;

			foreach ( DetailStat subphase in group.Subphases.Values )
				attributedMs += subphase.TotalMs;

			long unattributedMs = group.TotalMs > attributedMs ? group.TotalMs - attributedMs : 0;
			writer.WriteLine(
				"  {0}: total={1}ms max={2}ms attributed={3}ms other/unattributed={4}ms topSubphaseTotal={5} topSubphaseMax={6} errors={7}",
				group.Name,
				group.TotalMs,
				group.MaxMs,
				attributedMs,
				unattributedMs,
				FormatDetailSummary( GetTopDetail( group.Subphases, false ), false ),
				FormatDetailSummary( GetTopDetail( group.Subphases, true ), true ),
				group.ErrorCount );
		}

		private static GroupStat GetActiveGroup( string groupName )
		{
			RunSample sample = m_Active;

			if ( sample == null || String.IsNullOrWhiteSpace( groupName ) )
				return null;

			GroupStat group;

			if ( sample.Groups.TryGetValue( groupName, out group ) )
				return group;

			if ( sample.Groups.Count >= MaxGroups )
				return null;

			group = new GroupStat();
			group.Name = groupName;
			sample.Groups[groupName] = group;
			return group;
		}

		private static void AddDetail( Dictionary<string, DetailStat> details, int maximum, string name, long elapsedMs )
		{
			if ( String.IsNullOrWhiteSpace( name ) )
				name = "unknown";

			DetailStat detail;

			if ( !details.TryGetValue( name, out detail ) )
			{
				if ( details.Count >= maximum )
					return;

				detail = new DetailStat();
				detail.Name = name;
				details[name] = detail;
			}

			++detail.Count;
			detail.TotalMs += elapsedMs;
			detail.LastMs = elapsedMs;

			if ( elapsedMs > detail.MaxMs )
				detail.MaxMs = elapsedMs;
		}

		private static void MergeRun( RunSample totals, RunSample sample )
		{
			++totals.RunCount;
			totals.TotalMs += sample.TotalMs;
			totals.ErrorCount += sample.ErrorCount;
			totals.VillageChunkCount += sample.VillageChunkCount;
			totals.VillageChunkTotalMs += sample.VillageChunkTotalMs;
			totals.VillageStagedWallMs += sample.VillageStagedWallMs;
			totals.CityChunkCount += sample.CityChunkCount;
			totals.CityChunkTotalMs += sample.CityChunkTotalMs;
			totals.CityStagedWallMs += sample.CityStagedWallMs;
			totals.GardenChunkCount += sample.GardenChunkCount;
			totals.GardenChunkTotalMs += sample.GardenChunkTotalMs;
			totals.GardenStagedWallMs += sample.GardenStagedWallMs;
			totals.GardenPlacementCallbackCount += sample.GardenPlacementCallbackCount;
			totals.GardenPlacementCallbackTotalMs += sample.GardenPlacementCallbackTotalMs;
			totals.GardenPlacementWallMs += sample.GardenPlacementWallMs;
			totals.SpreadOutCleanupCallbackCount += sample.SpreadOutCleanupCallbackCount;
			totals.SpreadOutCleanupCallbackTotalMs += sample.SpreadOutCleanupCallbackTotalMs;
			totals.SpreadOutCleanupWallMs += sample.SpreadOutCleanupWallMs;
			totals.StealingPedestalCallbackCount += sample.StealingPedestalCallbackCount;
			totals.StealingPedestalCallbackTotalMs += sample.StealingPedestalCallbackTotalMs;
			totals.StealingPedestalWallMs += sample.StealingPedestalWallMs;
			totals.StagedCallbackCount += sample.StagedCallbackCount;
			totals.StagedCallbackTotalMs += sample.StagedCallbackTotalMs;
			totals.CompletionMode = sample.CompletionMode;
			totals.CompletionState = sample.CompletionState;
			totals.CurrentPhase = sample.CurrentPhase;
			totals.TimerName = sample.TimerName;
			totals.Startup = sample.Startup;
			totals.Forced = sample.Forced;
			totals.VillagesBeforeDueCheck = sample.VillagesBeforeDueCheck;
			totals.DueEvaluated = sample.DueEvaluated;
			totals.Due = sample.Due;

			if ( sample.VillageChunkMaxMs > totals.VillageChunkMaxMs )
				totals.VillageChunkMaxMs = sample.VillageChunkMaxMs;

			if ( sample.CityChunkMaxMs > totals.CityChunkMaxMs )
				totals.CityChunkMaxMs = sample.CityChunkMaxMs;

			if ( sample.GardenChunkMaxMs > totals.GardenChunkMaxMs )
				totals.GardenChunkMaxMs = sample.GardenChunkMaxMs;

			if ( sample.GardenPlacementCallbackMaxMs > totals.GardenPlacementCallbackMaxMs )
				totals.GardenPlacementCallbackMaxMs = sample.GardenPlacementCallbackMaxMs;

			if ( sample.SpreadOutCleanupCallbackMaxMs > totals.SpreadOutCleanupCallbackMaxMs )
			{
				totals.SpreadOutCleanupCallbackMaxMs = sample.SpreadOutCleanupCallbackMaxMs;
				totals.SpreadOutCleanupCallbackMaxName = sample.SpreadOutCleanupCallbackMaxName;
				totals.SpreadOutCleanupCallbackMaxKind = sample.SpreadOutCleanupCallbackMaxKind;
			}

			if ( sample.StealingPedestalCallbackMaxMs > totals.StealingPedestalCallbackMaxMs )
			{
				totals.StealingPedestalCallbackMaxMs = sample.StealingPedestalCallbackMaxMs;
				totals.StealingPedestalCallbackMaxName = sample.StealingPedestalCallbackMaxName;
				totals.StealingPedestalCallbackMaxKind = sample.StealingPedestalCallbackMaxKind;
			}

			if ( sample.StagedCallbackMaxMs > totals.StagedCallbackMaxMs )
			{
				totals.StagedCallbackMaxMs = sample.StagedCallbackMaxMs;
				totals.StagedCallbackMaxName = sample.StagedCallbackMaxName;
				totals.StagedCallbackMaxKind = sample.StagedCallbackMaxKind;
			}

			foreach ( KeyValuePair<string, GroupStat> entry in sample.Groups )
			{
				GroupStat target;

				if ( !totals.Groups.TryGetValue( entry.Key, out target ) )
				{
					if ( totals.Groups.Count >= MaxGroups )
						continue;

					target = new GroupStat();
					target.Name = entry.Key;
					totals.Groups[entry.Key] = target;
				}

				MergeGroup( target, entry.Value );
			}
		}

		private static void MergeGroup( GroupStat target, GroupStat source )
		{
			target.Calls += source.Calls;
			target.TotalMs += source.TotalMs;
			target.LastMs = source.LastMs;
			target.SlowCount += source.SlowCount;
			target.ErrorCount += source.ErrorCount;

			if ( source.MaxMs > target.MaxMs )
				target.MaxMs = source.MaxMs;

			if ( source.WorstDescriptionMs > target.WorstDescriptionMs )
			{
				target.WorstDescriptionMs = source.WorstDescriptionMs;
				target.WorstDescription = source.WorstDescription;
			}

			foreach ( KeyValuePair<string, long> entry in source.Counters )
			{
				long value;

				if ( target.Counters.TryGetValue( entry.Key, out value ) )
					target.Counters[entry.Key] = value + entry.Value;
				else if ( target.Counters.Count < MaxCountersPerGroup )
					target.Counters[entry.Key] = entry.Value;
			}

			MergeDetails( target.Subphases, source.Subphases, MaxSubphasesPerGroup );
			MergeDetails( target.Details, source.Details, MaxDetailsPerGroup );
		}

		private static void MergeDetails( Dictionary<string, DetailStat> target, Dictionary<string, DetailStat> source, int maximum )
		{
			foreach ( KeyValuePair<string, DetailStat> entry in source )
			{
				DetailStat detail;

				if ( !target.TryGetValue( entry.Key, out detail ) )
				{
					if ( target.Count >= maximum )
						continue;

					detail = new DetailStat();
					detail.Name = entry.Key;
					target[entry.Key] = detail;
				}

				detail.Count += entry.Value.Count;
				detail.TotalMs += entry.Value.TotalMs;
				detail.LastMs = entry.Value.LastMs;

				if ( entry.Value.MaxMs > detail.MaxMs )
					detail.MaxMs = entry.Value.MaxMs;
			}
		}

		private static void WriteRun( TextWriter writer, string label, RunSample sample )
		{
			if ( sample == null )
			{
				writer.WriteLine( "TaskManagerDailyAttributionStats {0}: Not sampled", label );
				return;
			}

			GroupStat top = GetTopGroup( sample );
			writer.WriteLine(
				"TaskManagerDailyAttributionStats {0}: runId={1} runs={2} timer={3} startup={4} forced={5} startedUtc={6} total={7}ms mode={8} state={9} phase={10} villagesBeforeDueCheck={11} dueEvaluated={12} due={13} groups={14} errors={15} top={16}",
				label,
				sample.RunId,
				sample.RunCount,
				!String.IsNullOrWhiteSpace( sample.TimerName ) ? sample.TimerName : "unknown",
				sample.Startup,
				sample.Forced,
				sample.StartedUtc,
				sample.TotalMs,
				!String.IsNullOrWhiteSpace( sample.CompletionMode ) ? sample.CompletionMode : "unknown",
				!String.IsNullOrWhiteSpace( sample.CompletionState ) ? sample.CompletionState : "unknown",
				!String.IsNullOrWhiteSpace( sample.CurrentPhase ) ? sample.CurrentPhase : "none",
				sample.VillagesBeforeDueCheck,
				sample.DueEvaluated,
				sample.Due,
				sample.Groups.Count,
				sample.ErrorCount,
				top != null ? String.Format( "{0}:{1}ms", top.Name, top.TotalMs ) : "none" );
			writer.WriteLine(
				"  villageCallbacks: callbacks={0} work={1}ms maxCallback={2}ms avgCallback={3}ms stagedWall={4}ms constructor={5}ms nonConstructor={6}ms",
				sample.VillageChunkCount,
				sample.VillageChunkTotalMs,
				sample.VillageChunkMaxMs,
				sample.VillageChunkCount > 0 ? sample.VillageChunkTotalMs / sample.VillageChunkCount : 0,
				sample.VillageStagedWallMs,
				GetSubphaseMilliseconds( sample, "PopulateVillages setupSpawn", "constructor" ),
				Math.Max( 0, GetGroupMilliseconds( sample, "PopulateVillages setupSpawn" ) - GetSubphaseMilliseconds( sample, "PopulateVillages setupSpawn", "constructor" ) ) );
			writer.WriteLine(
				"  cityStaging: callbacks={0} work={1}ms maxCallback={2}ms avgCallback={3}ms stagedWall={4}ms",
				sample.CityChunkCount,
				sample.CityChunkTotalMs,
				sample.CityChunkMaxMs,
				sample.CityChunkCount > 0 ? sample.CityChunkTotalMs / sample.CityChunkCount : 0,
				sample.CityStagedWallMs );
			writer.WriteLine(
				"  gardenStaging: callbacks={0} work={1}ms maxCallback={2}ms avgCallback={3}ms stagedWall={4}ms",
				sample.GardenChunkCount,
				sample.GardenChunkTotalMs,
				sample.GardenChunkMaxMs,
				sample.GardenChunkCount > 0 ? sample.GardenChunkTotalMs / sample.GardenChunkCount : 0,
				sample.GardenStagedWallMs );
			writer.WriteLine(
				"  gardenPlacement: entries={0}/{1} randomRolls={2} plantsCreated={3} callbacks={4} work={5}ms maxCallback={6}ms avgCallback={7}ms wall={8}ms heavyOutliers={9}",
				GetCounter( sample, "PlantGardens", "placementEntriesProcessed" ),
				GetCounter( sample, "PlantGardens", "placementEntriesTotal" ),
				GetCounter( sample, "PlantGardens", "randomRollsPerformed" ),
				GetCounter( sample, "PlantGardens", "plantsPlaced" ),
				sample.GardenPlacementCallbackCount,
				sample.GardenPlacementCallbackTotalMs,
				sample.GardenPlacementCallbackMaxMs,
				sample.GardenPlacementCallbackCount > 0 ? sample.GardenPlacementCallbackTotalMs / sample.GardenPlacementCallbackCount : 0,
				sample.GardenPlacementWallMs,
				GetCounter( sample, "PlantGardens", "heavySingleOperations" ) );
			WriteSpreadOutCleanupStaging( writer, sample );
			writer.WriteLine(
				"  actualStagedCallbacks: count={0} total={1}ms max={2}ms avg={3}ms callback={4} kind={5}",
				sample.StagedCallbackCount,
				sample.StagedCallbackTotalMs,
				sample.StagedCallbackMaxMs,
				sample.StagedCallbackCount > 0 ? sample.StagedCallbackTotalMs / sample.StagedCallbackCount : 0,
				!String.IsNullOrWhiteSpace( sample.StagedCallbackMaxName ) ? sample.StagedCallbackMaxName : "none",
				!String.IsNullOrWhiteSpace( sample.StagedCallbackMaxKind ) ? sample.StagedCallbackMaxKind : "none" );

			GroupStat[] groups = new GroupStat[sample.Groups.Count];
			sample.Groups.Values.CopyTo( groups, 0 );
			Array.Sort( groups, CompareGroups );

			for ( int i = 0; i < groups.Length; ++i )
				WriteGroup( writer, groups[i] );
		}

		private static void WriteSpreadOutCleanupStaging( TextWriter writer, RunSample sample )
		{
			writer.WriteLine(
				"  spreadOutCleanupStaging: callbacks={0} work={1}ms maxCallback={2}ms avgCallback={3}ms wall={4}ms callback={5} kind={6} clearShipUnits={7} targetUnits={8} healerUnits={9} skipped={10}",
				sample.SpreadOutCleanupCallbackCount,
				sample.SpreadOutCleanupCallbackTotalMs,
				sample.SpreadOutCleanupCallbackMaxMs,
				sample.SpreadOutCleanupCallbackCount > 0 ? sample.SpreadOutCleanupCallbackTotalMs / sample.SpreadOutCleanupCallbackCount : 0,
				sample.SpreadOutCleanupWallMs,
				!String.IsNullOrWhiteSpace( sample.SpreadOutCleanupCallbackMaxName ) ? sample.SpreadOutCleanupCallbackMaxName : "none",
				!String.IsNullOrWhiteSpace( sample.SpreadOutCleanupCallbackMaxKind ) ? sample.SpreadOutCleanupCallbackMaxKind : "none",
				GetCounter( sample, "SpreadOutCreatureCleanup", "clearShipProcessed" ),
				GetCounter( sample, "SpreadOutCreatureCleanup", "deleteTargetsWorkUnits" ),
				GetCounter( sample, "SpreadOutCreatureCleanup", "deleteHealersWorkUnits" ),
				GetCounter( sample, "SpreadOutCreatureCleanup", "alreadyDeletedSkipped" ) );
		}

		private static void WriteGroup( TextWriter writer, GroupStat group )
		{
			if ( group == null )
				return;

			long average = group.Calls > 0 ? group.TotalMs / group.Calls : 0;
			writer.WriteLine(
				"  {0}: calls={1} total={2}ms max={3}ms avg={4}ms last={5}ms slow={6} errors={7} worst={8}:{9}ms",
				group.Name,
				group.Calls,
				group.TotalMs,
				group.MaxMs,
				average,
				group.LastMs,
				group.SlowCount,
				group.ErrorCount,
				!String.IsNullOrWhiteSpace( group.WorstDescription ) ? group.WorstDescription : "none",
				group.WorstDescriptionMs );

			long attributedMs = 0;

			foreach ( DetailStat subphase in group.Subphases.Values )
				attributedMs += subphase.TotalMs;

			long unattributedMs = group.TotalMs > attributedMs ? group.TotalMs - attributedMs : 0;
			DetailStat topSubphaseTotal = GetTopDetail( group.Subphases, false );
			DetailStat topSubphaseMax = GetTopDetail( group.Subphases, true );
			writer.WriteLine(
				"    attribution: attributed={0}ms other/unattributed={1}ms topSubphaseTotal={2} topSubphaseMax={3}",
				attributedMs,
				unattributedMs,
				FormatDetailSummary( topSubphaseTotal, false ),
				FormatDetailSummary( topSubphaseMax, true ) );

			WriteCounters( writer, group.Counters );
			WriteDetails( writer, "subphase", group.Subphases, 12 );
			WriteDetailLeaders( writer, group.Details );
			WriteDetails( writer, "detail", group.Details, 12 );
		}

		private static void WriteCounters( TextWriter writer, Dictionary<string, long> counters )
		{
			if ( counters.Count == 0 )
			{
				writer.WriteLine( "    counters: none" );
				return;
			}

			string[] names = new string[counters.Count];
			counters.Keys.CopyTo( names, 0 );
			Array.Sort( names, StringComparer.Ordinal );

			writer.Write( "    counters:" );

			for ( int i = 0; i < names.Length; ++i )
				writer.Write( " {0}={1}", names[i], counters[names[i]] );

			writer.WriteLine();
		}

		private static void WriteDetails( TextWriter writer, string label, Dictionary<string, DetailStat> details, int maximum )
		{
			if ( details.Count == 0 )
			{
				writer.WriteLine( "    {0}: none", label );
				return;
			}

			DetailStat[] values = new DetailStat[details.Count];
			details.Values.CopyTo( values, 0 );
			Array.Sort( values, CompareDetails );

			int count = Math.Min( maximum, values.Length );

			for ( int i = 0; i < count; ++i )
			{
				DetailStat detail = values[i];
				writer.WriteLine(
					"    {0}#{1}: {2} count={3} total={4}ms max={5}ms last={6}ms",
					label,
					i + 1,
					detail.Name,
					detail.Count,
					detail.TotalMs,
					detail.MaxMs,
					detail.LastMs );
			}
		}

		private static void WriteDetailLeaders( TextWriter writer, Dictionary<string, DetailStat> details )
		{
			if ( details.Count == 0 )
				return;

			List<string> categories = new List<string>();

			foreach ( string name in details.Keys )
			{
				int separator = name.IndexOf( ':' );
				string category = separator > 0 ? name.Substring( 0, separator ) : "detail";

				if ( !categories.Contains( category ) && categories.Count < 16 )
					categories.Add( category );
			}

			categories.Sort( StringComparer.Ordinal );

			for ( int i = 0; i < categories.Count; ++i )
			{
				string prefix = categories[i] + ":";
				DetailStat topTotal = null;
				DetailStat topMax = null;
				DetailStat topCount = null;

				foreach ( KeyValuePair<string, DetailStat> entry in details )
				{
					if ( !entry.Key.StartsWith( prefix, StringComparison.Ordinal ) )
						continue;

					DetailStat detail = entry.Value;

					if ( topTotal == null || detail.TotalMs > topTotal.TotalMs )
						topTotal = detail;

					if ( topMax == null || detail.MaxMs > topMax.MaxMs )
						topMax = detail;

					if ( topCount == null || detail.Count > topCount.Count )
						topCount = detail;
				}

				writer.WriteLine(
					"    leaders {0}: total={1} max={2} count={3}",
					categories[i],
					FormatDetailSummary( topTotal, false ),
					FormatDetailSummary( topMax, true ),
					FormatCountSummary( topCount ) );
			}
		}

		private static GroupStat GetTopGroup( RunSample sample )
		{
			GroupStat top = null;

			if ( sample == null )
				return null;

			foreach ( GroupStat group in sample.Groups.Values )
			{
				if ( top == null || group.TotalMs > top.TotalMs )
					top = group;
			}

			return top;
		}

		private static GroupStat GetTopGroupByMaximum( RunSample sample )
		{
			GroupStat top = null;

			if ( sample == null )
				return null;

			foreach ( GroupStat group in sample.Groups.Values )
			{
				if ( top == null || group.MaxMs > top.MaxMs )
					top = group;
			}

			return top;
		}

		private static GroupStat GetWorstSampleGroup( RunSample sample )
		{
			GroupStat worst = null;

			if ( sample == null )
				return null;

			foreach ( GroupStat group in sample.Groups.Values )
			{
				if ( worst == null || group.WorstDescriptionMs > worst.WorstDescriptionMs )
					worst = group;
			}

			return worst != null && worst.WorstDescriptionMs > 0 ? worst : null;
		}

		private static DetailStat GetTopSubphase( RunSample sample, bool byMaximum, out string groupName )
		{
			DetailStat top = null;
			groupName = null;

			if ( sample == null )
				return null;

			foreach ( GroupStat group in sample.Groups.Values )
			{
				DetailStat candidate = GetTopDetail( group.Subphases, byMaximum );

				if ( candidate == null )
					continue;

				long candidateValue = byMaximum ? candidate.MaxMs : candidate.TotalMs;
				long topValue = top != null ? ( byMaximum ? top.MaxMs : top.TotalMs ) : -1;

				if ( top == null || candidateValue > topValue )
				{
					top = candidate;
					groupName = group.Name;
				}
			}

			return top;
		}

		private static DetailStat GetTopCategorizedDetail( RunSample sample, string[] categories )
		{
			DetailStat top = null;

			if ( sample == null || categories == null )
				return null;

			foreach ( GroupStat group in sample.Groups.Values )
			{
				foreach ( KeyValuePair<string, DetailStat> entry in group.Details )
				{
					bool matches = false;

					for ( int i = 0; i < categories.Length; ++i )
					{
						if ( entry.Key.StartsWith( categories[i] + ":", StringComparison.Ordinal ) )
						{
							matches = true;
							break;
						}
					}

					if ( matches && ( top == null || entry.Value.TotalMs > top.TotalMs ) )
						top = entry.Value;
				}
			}

			return top;
		}

		private static long GetGroupMilliseconds( RunSample sample, string groupName )
		{
			GroupStat group;
			return sample.Groups.TryGetValue( groupName, out group ) ? group.LastMs : 0;
		}

		private static long GetCounter( RunSample sample, string groupName, string counterName )
		{
			GroupStat group;
			long value;

			return sample != null
				&& sample.Groups.TryGetValue( groupName, out group )
				&& group.Counters.TryGetValue( counterName, out value )
					? value
					: 0;
		}

		private static long GetSubphaseMilliseconds( RunSample sample, string groupName, string subphaseName )
		{
			GroupStat group;
			DetailStat subphase;

			if ( sample == null
				|| !sample.Groups.TryGetValue( groupName, out group )
				|| !group.Subphases.TryGetValue( subphaseName, out subphase ) )
				return 0;

			return subphase.TotalMs;
		}

		private static int CompareGroups( GroupStat left, GroupStat right )
		{
			if ( left == null && right == null )
				return 0;

			if ( left == null )
				return 1;

			if ( right == null )
				return -1;

			return right.TotalMs.CompareTo( left.TotalMs );
		}

		private static int CompareDetails( DetailStat left, DetailStat right )
		{
			if ( left == null && right == null )
				return 0;

			if ( left == null )
				return 1;

			if ( right == null )
				return -1;

			int total = right.TotalMs.CompareTo( left.TotalMs );
			return total != 0 ? total : right.Count.CompareTo( left.Count );
		}

		private static DetailStat GetTopDetail( Dictionary<string, DetailStat> details, bool byMaximum )
		{
			DetailStat top = null;

			foreach ( DetailStat detail in details.Values )
			{
				if ( top == null )
				{
					top = detail;
					continue;
				}

				if ( byMaximum )
				{
					if ( detail.MaxMs > top.MaxMs )
						top = detail;
				}
				else if ( detail.TotalMs > top.TotalMs )
				{
					top = detail;
				}
			}

			return top;
		}

		private static string FormatDetailSummary( DetailStat detail, bool byMaximum )
		{
			if ( detail == null )
				return "none";

			return String.Format( "{0}:{1}ms", detail.Name, byMaximum ? detail.MaxMs : detail.TotalMs );
		}

		private static string FormatGroupSummary( GroupStat group, bool byMaximum )
		{
			if ( group == null )
				return "none";

			return String.Format( "{0}:{1}ms", group.Name, byMaximum ? group.MaxMs : group.TotalMs );
		}

		private static string FormatQualifiedDetailSummary( string groupName, DetailStat detail, bool byMaximum )
		{
			if ( detail == null )
				return "none";

			return String.Format(
				"{0}/{1}:{2}ms",
				!String.IsNullOrWhiteSpace( groupName ) ? groupName : "unknown",
				detail.Name,
				byMaximum ? detail.MaxMs : detail.TotalMs );
		}

		private static string FormatCountSummary( DetailStat detail )
		{
			if ( detail == null )
				return "none";

			return String.Format( "{0}:{1}", detail.Name, detail.Count );
		}

		private static string SafeName( string value )
		{
			if ( String.IsNullOrWhiteSpace( value ) )
				return "unknown";

			value = value.Replace( '\r', ' ' ).Replace( '\n', ' ' );
			return value.Length <= 180 ? value : value.Substring( 0, 180 );
		}
	}
}
