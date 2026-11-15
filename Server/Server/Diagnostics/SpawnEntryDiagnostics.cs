using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Server.Diagnostics
{
	public static class SpawnEntryDiagnostics
	{
		public enum ScheduleReason
		{
			Start,
			SpawnedObjectRemoved,
			TimerCallbackReschedule,
			DeserializeRestore,
			Respawn,
			Other
		}

		private const long WindowMs = 60000;
		private const int SpawnEntrySlowThresholdMs = 25;
		private const int SpawnEntryVerySlowThresholdMs = 50;
		private const int SpawnEntryMaxTrackedEntries = 256;
		private const int SpawnEntryMaxTrackedRegions = 128;
		private const int SpawnEntryMaxTrackedTypes = 128;
		private const int SpawnEntryMaxTrackedDefinitions = 128;
		private const int SpawnEntryMaxTrackedScheduleDetails = 256;
		private const int SpawnEntryMaxTrackedCorrelations = 64;
		private const int SpawnEntryMaxTrackedPerCorrelation = 32;
		private const int SpawnEntryMaxRecentSamples = 32;
		private const int SpawnEntryNearImmediateScheduleMs = 1000;
		private const long SpawnEntrySlowLogIntervalMs = 30000;
		private const long SpawnEntrySliceThresholdMs = 250;
		private const int MaxTrackedOriginChains = 4096;
		private const int OriginChainTtlMinutes = 5;
		private const int MaxBurstSummaries = 16;
		private const int MaxBurstDetailEntries = 128;

		private static readonly object m_Lock = new object();
		private static WindowStats m_Current = new WindowStats();
		private static WindowStats m_Last = new WindowStats();
		private static WindowStats m_Total = new WindowStats();

		private static Dictionary<string, DetailStat> m_CurrentEntries = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_LastEntries = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_TotalEntries = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_CurrentRegions = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_LastRegions = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_TotalRegions = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_CurrentTypes = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_LastTypes = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_TotalTypes = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_CurrentDefinitions = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_LastDefinitions = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_TotalDefinitions = new Dictionary<string, DetailStat>();

		private static Dictionary<string, DetailStat> m_CurrentScheduleEntries = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_LastScheduleEntries = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_TotalScheduleEntries = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_CurrentScheduleRegions = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_LastScheduleRegions = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_TotalScheduleRegions = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_CurrentScheduleDefinitions = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_LastScheduleDefinitions = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_TotalScheduleDefinitions = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_CurrentScheduleReasons = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_LastScheduleReasons = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_TotalScheduleReasons = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_CurrentScheduleRemovedTypes = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_LastScheduleRemovedTypes = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_TotalScheduleRemovedTypes = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_CurrentScheduleDirectOrigins = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_LastScheduleDirectOrigins = new Dictionary<string, DetailStat>();
		private static Dictionary<string, DetailStat> m_TotalScheduleDirectOrigins = new Dictionary<string, DetailStat>();

		private static Dictionary<string, CorrelationStat> m_CurrentRecentCorrelations = new Dictionary<string, CorrelationStat>();
		private static Dictionary<string, CorrelationStat> m_LastRecentCorrelations = new Dictionary<string, CorrelationStat>();
		private static Dictionary<string, CorrelationStat> m_TotalRecentCorrelations = new Dictionary<string, CorrelationStat>();

		private static Dictionary<string, OriginChain> m_OriginChains = new Dictionary<string, OriginChain>();
		private static Dictionary<long, BurstSummary> m_Bursts = new Dictionary<long, BurstSummary>();
		private static List<long> m_BurstOrder = new List<long>();
		private static Queue<Sample> m_RecentSlowSamples = new Queue<Sample>();
		private static long m_NextWindow;
		private static long m_NextSlowLog;
		private static long m_DiagnosticErrors;
		private static long m_NextSliceId;
		private static SliceSample m_LastSpawnEntrySlice;
		private static SliceSample m_MaxSpawnEntrySlice;

		[ThreadStatic]
		private static SliceAccumulator t_CurrentSlice;

		private sealed class WindowStats
		{
			public long CallbackCount;
			public long TotalMs;
			public long MaxMs;
			public long SlowCount;
			public long VerySlowCount;
			public long ScheduleCount;
			public long ImmediateScheduleCount;
			public long NearImmediateScheduleCount;
			public long TotalScheduleDelayMs;
			public long DirectOriginScheduleCount;
			public long RecentCorrelationScheduleCount;
			public long NoOriginScheduleCount;
			public long OriginChainCallbackCount;
			public long NoOriginCallbackCount;
			public Sample Worst;

			public WindowStats Clone()
			{
				WindowStats copy = new WindowStats();
				copy.CallbackCount = CallbackCount;
				copy.TotalMs = TotalMs;
				copy.MaxMs = MaxMs;
				copy.SlowCount = SlowCount;
				copy.VerySlowCount = VerySlowCount;
				copy.ScheduleCount = ScheduleCount;
				copy.ImmediateScheduleCount = ImmediateScheduleCount;
				copy.NearImmediateScheduleCount = NearImmediateScheduleCount;
				copy.TotalScheduleDelayMs = TotalScheduleDelayMs;
				copy.DirectOriginScheduleCount = DirectOriginScheduleCount;
				copy.RecentCorrelationScheduleCount = RecentCorrelationScheduleCount;
				copy.NoOriginScheduleCount = NoOriginScheduleCount;
				copy.OriginChainCallbackCount = OriginChainCallbackCount;
				copy.NoOriginCallbackCount = NoOriginCallbackCount;
				copy.Worst = Worst != null ? Worst.Clone() : null;
				return copy;
			}

			public void Clear()
			{
				CallbackCount = 0;
				TotalMs = 0;
				MaxMs = 0;
				SlowCount = 0;
				VerySlowCount = 0;
				ScheduleCount = 0;
				ImmediateScheduleCount = 0;
				NearImmediateScheduleCount = 0;
				TotalScheduleDelayMs = 0;
				DirectOriginScheduleCount = 0;
				RecentCorrelationScheduleCount = 0;
				NoOriginScheduleCount = 0;
				OriginChainCallbackCount = 0;
				NoOriginCallbackCount = 0;
				Worst = null;
			}

			public void AddCallback(Sample sample)
			{
				++CallbackCount;
				TotalMs += sample.ElapsedMs;

				if (sample.ElapsedMs > MaxMs)
					MaxMs = sample.ElapsedMs;

				if (sample.ElapsedMs >= SpawnEntrySlowThresholdMs)
					++SlowCount;

				if (sample.ElapsedMs >= SpawnEntryVerySlowThresholdMs)
					++VerySlowCount;

				if (sample.OriginRunId > 0)
					++OriginChainCallbackCount;
				else
					++NoOriginCallbackCount;

				if (Worst == null || sample.ElapsedMs > Worst.ElapsedMs)
					Worst = sample.Clone();
			}

			public void AddSchedule(long delayMs, bool directOrigin, bool recentCorrelation)
			{
				++ScheduleCount;
				TotalScheduleDelayMs += delayMs;

				if (delayMs <= 0)
					++ImmediateScheduleCount;

				if (delayMs <= SpawnEntryNearImmediateScheduleMs)
					++NearImmediateScheduleCount;

				if (directOrigin)
					++DirectOriginScheduleCount;
				else if (recentCorrelation)
					++RecentCorrelationScheduleCount;
				else
					++NoOriginScheduleCount;
			}
		}

		public sealed class PerformanceSummary
		{
			public long TotalCallbackMs;
			public long DirectSchedules;
			public long UnknownOriginCallbacks;
			public long LatestBurstSchedules;
			public long LatestBurstCallbacks;
			public long LatestBurstCallbackMs;
			public long LatestBurstRunId;
			public string LatestBurstTask;
			public int ActiveOriginChains;
			public int CompletedOriginChains;
			public int TrackedOriginChains;
		}

		private sealed class DetailStat
		{
			public string Key;
			public string Description;
			public long Count;
			public long TotalMs;
			public long MaxMs;
			public long SlowCount;
			public long VerySlowCount;
			public long LastMs;
			public DateTime LastUtc;

			public DetailStat Clone()
			{
				DetailStat copy = new DetailStat();
				copy.Key = Key;
				copy.Description = Description;
				copy.Count = Count;
				copy.TotalMs = TotalMs;
				copy.MaxMs = MaxMs;
				copy.SlowCount = SlowCount;
				copy.VerySlowCount = VerySlowCount;
				copy.LastMs = LastMs;
				copy.LastUtc = LastUtc;
				return copy;
			}

			public void Add(long elapsedMs)
			{
				++Count;
				TotalMs += elapsedMs;
				LastMs = elapsedMs;
				LastUtc = DateTime.UtcNow;

				if (elapsedMs > MaxMs)
					MaxMs = elapsedMs;

				if (elapsedMs >= SpawnEntrySlowThresholdMs)
					++SlowCount;

				if (elapsedMs >= SpawnEntryVerySlowThresholdMs)
					++VerySlowCount;
			}
		}

		private sealed class CorrelationStat
		{
			public string Label;
			public WindowStats Stats = new WindowStats();
			public Dictionary<string, DetailStat> Entries = new Dictionary<string, DetailStat>();
			public Dictionary<string, DetailStat> Regions = new Dictionary<string, DetailStat>();
			public Dictionary<string, DetailStat> Types = new Dictionary<string, DetailStat>();

			public CorrelationStat Clone()
			{
				CorrelationStat copy = new CorrelationStat();
				copy.Label = Label;
				copy.Stats = Stats.Clone();
				copy.Entries = CloneDetails(Entries);
				copy.Regions = CloneDetails(Regions);
				copy.Types = CloneDetails(Types);
				return copy;
			}
		}

		private sealed class Sample
		{
			public DateTime TimeUtc;
			public long ElapsedMs;
			public string Entry;
			public string Region;
			public string EntryType;
			public string Definition;
			public int BeforeSpawnedCount;
			public int RequestedAmount;
			public int SpawnedCount;
			public int MaxCount;
			public int SpawnedThisCallback;
			public bool Running;
			public bool Spawning;
			public bool CallbackRescheduled;
			public long NextDelayMs;
			public long OriginRunId;
			public string DirectOrigin;
			public string OriginChain;
			public string RecentCorrelation;
			public string ScheduleReason;
			public string RemovedTargetType;

			public Sample Clone()
			{
				return (Sample)MemberwiseClone();
			}
		}

		private sealed class ScheduleSample
		{
			public DateTime TimeUtc;
			public long DelayMs;
			public string EntryKey;
			public string Entry;
			public string RegionKey;
			public string Region;
			public string DefinitionKey;
			public string Definition;
			public string EntryType;
			public ScheduleReason Reason;
			public string RemovedTargetType;
			public int RemovedSerial;
			public bool TimerAlreadyExisted;
			public bool TimerCreated;
			public int BeforeSpawnedCount;
			public int AfterSpawnedCount;
			public int MaxCount;
			public long OriginRunId;
			public string DirectOrigin;
			public string RecentCorrelation;
			public string TargetAction;
		}

		private sealed class SliceAccumulator
		{
			public long Id;
			public long Count;
			public long TotalMs;
			public long SlowCount;
			public Sample Worst;
			public Dictionary<string, DetailStat> Entries = new Dictionary<string, DetailStat>();
			public Dictionary<string, DetailStat> Regions = new Dictionary<string, DetailStat>();
			public Dictionary<string, DetailStat> Types = new Dictionary<string, DetailStat>();
			public Dictionary<string, DetailStat> Origins = new Dictionary<string, DetailStat>();
			public Dictionary<long, SliceRunStat> Runs = new Dictionary<long, SliceRunStat>();
		}

		private sealed class SliceRunStat
		{
			public long RunId;
			public long CallbackCount;
			public long TotalMs;
			public long MaxMs;
		}

		private sealed class SliceSample
		{
			public DateTime TimeUtc;
			public long SliceMs;
			public int QueueCount;
			public int ProcessedCount;
			public long NamedMs;
			public long OtherMs;
			public long SpawnEntryMs;
			public long CallbackCount;
			public long SlowCount;
			public string TopEntryTotal;
			public string TopEntryMax;
			public string TopRegionTotal;
			public string TopTypeTotal;
			public string TopOriginTotal;
			public Sample Worst;

			public SliceSample Clone()
			{
				SliceSample copy = (SliceSample)MemberwiseClone();
				copy.Worst = Worst != null ? Worst.Clone() : null;
				return copy;
			}
		}

		private sealed class OriginChain
		{
			public string EntryKey;
			public string EntryDescription;
			public string RegionKey;
			public string RegionDescription;
			public string DefinitionKey;
			public string DefinitionDescription;
			public long RunId;
			public string TaskName;
			public string TimerName;
			public string PhaseName;
			public string ScheduleReason;
			public string RemovedTargetType;
			public string TargetAction;
			public DateTime ScheduleUtc;
			public DateTime FirstCallbackUtc;
			public DateTime LastCallbackUtc;
			public DateTime LastTouchedUtc;
			public long Schedules;
			public long CallbackCount;
			public long CallbackMs;
			public long MaxCallbackMs;
			public bool Completed;
			public int FinalSpawnedCount;
			public int MaxCount;

			public string OriginLabel
			{
				get
				{
					if (RunId <= 0)
						return "none";

					string phase = !String.IsNullOrWhiteSpace(PhaseName) ? "/" + PhaseName : String.Empty;
					return String.Format("{0}{1}/runId={2}", CleanOutput(TaskName), phase, RunId);
				}
			}

			public OriginChain Clone()
			{
				return (OriginChain)MemberwiseClone();
			}
		}

		private sealed class ProjectionStats
		{
			public string Label;
			public int SpreadMs;
			public long Count;
			public long TotalAddedMs;
			public long MaxAddedMs;
			public Dictionary<int, long> Bucket250 = new Dictionary<int, long>();
			public Dictionary<int, long> Bucket1000 = new Dictionary<int, long>();

			public ProjectionStats Clone()
			{
				ProjectionStats copy = new ProjectionStats();
				copy.Label = Label;
				copy.SpreadMs = SpreadMs;
				copy.Count = Count;
				copy.TotalAddedMs = TotalAddedMs;
				copy.MaxAddedMs = MaxAddedMs;
				copy.Bucket250 = CloneLongDictionary(Bucket250);
				copy.Bucket1000 = CloneLongDictionary(Bucket1000);
				return copy;
			}

			public void Add(long actualOffsetMs, long addedDelayMs)
			{
				if (actualOffsetMs < 0)
					actualOffsetMs = 0;

				if (addedDelayMs < 0)
					addedDelayMs = 0;

				long projectedOffset = actualOffsetMs + addedDelayMs;
				++Count;
				TotalAddedMs += addedDelayMs;

				if (addedDelayMs > MaxAddedMs)
					MaxAddedMs = addedDelayMs;

				AddBucket(Bucket250, (int)(projectedOffset / 250));
				AddBucket(Bucket1000, (int)(projectedOffset / 1000));
			}
		}

		private sealed class BurstSummary
		{
			public long RunId;
			public string TaskName;
			public string TimerName;
			public string PhaseName;
			public bool Startup;
			public DateTime StartUtc;
			public DateTime EndUtc;
			public long Schedules;
			public long Callbacks;
			public long CallbackMs;
			public long MaxCallbackMs;
			public long SlowCallbacks;
			public long MaxCallbacksInSlice;
			public long MaxSpawnEntryMsInSlice;
			public long MaxSliceMs;
			public long EntriesCompleteAfter1s;
			public long EntriesCompleteAfter5s;
			public long EntriesCompleteAfter10s;
			public long EntriesIncompleteAfter30s;
			public string ExactWorstEntry;
			public Dictionary<string, DetailStat> Entries = new Dictionary<string, DetailStat>();
			public Dictionary<string, DetailStat> Regions = new Dictionary<string, DetailStat>();
			public Dictionary<string, DetailStat> Definitions = new Dictionary<string, DetailStat>();
			public Dictionary<string, DetailStat> RemovedTargetTypes = new Dictionary<string, DetailStat>();
			public Dictionary<string, DetailStat> TargetTypes = new Dictionary<string, DetailStat>();
			public Dictionary<string, DetailStat> TargetActions = new Dictionary<string, DetailStat>();
			public ProjectionStats ActualProjection = NewProjection("actual", 0);
			public ProjectionStats Spread5Projection = NewProjection("dryRun5s", 5000);
			public ProjectionStats Spread10Projection = NewProjection("dryRun10s", 10000);
			public ProjectionStats Spread15Projection = NewProjection("dryRun15s", 15000);

			public BurstSummary Clone()
			{
				BurstSummary copy = (BurstSummary)MemberwiseClone();
				copy.Entries = CloneDetails(Entries);
				copy.Regions = CloneDetails(Regions);
				copy.Definitions = CloneDetails(Definitions);
				copy.RemovedTargetTypes = CloneDetails(RemovedTargetTypes);
				copy.TargetTypes = CloneDetails(TargetTypes);
				copy.TargetActions = CloneDetails(TargetActions);
				copy.ActualProjection = ActualProjection.Clone();
				copy.Spread5Projection = Spread5Projection.Clone();
				copy.Spread10Projection = Spread10Projection.Clone();
				copy.Spread15Projection = Spread15Projection.Clone();
				return copy;
			}
		}

		public static long GetTimestamp()
		{
			return Stopwatch.GetTimestamp();
		}

		public static long GetElapsedMilliseconds(long start)
		{
			return (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
		}

		public static long BeginTimerSlice()
		{
			long id = Interlocked.Increment(ref m_NextSliceId);
			SliceAccumulator slice = new SliceAccumulator();
			slice.Id = id;
			t_CurrentSlice = slice;
			return id;
		}

		public static void RecordTimerSlice(long sliceId, long sliceMs, int queueCount, int processedCount, long namedMs, long otherMs)
		{
			try
			{
				SliceAccumulator slice = t_CurrentSlice;

				if (slice == null || slice.Id != sliceId || slice.Count == 0 || sliceMs < SpawnEntrySliceThresholdMs)
					return;

				SliceSample sample = new SliceSample();
				sample.TimeUtc = DateTime.UtcNow;
				sample.SliceMs = sliceMs;
				sample.QueueCount = queueCount;
				sample.ProcessedCount = processedCount;
				sample.NamedMs = namedMs;
				sample.OtherMs = otherMs;
				sample.SpawnEntryMs = slice.TotalMs;
				sample.CallbackCount = slice.Count;
				sample.SlowCount = slice.SlowCount;
				sample.TopEntryTotal = FormatDetail(GetTopDetail(slice.Entries, "total"));
				sample.TopEntryMax = FormatDetail(GetTopDetail(slice.Entries, "max"));
				sample.TopRegionTotal = FormatDetail(GetTopDetail(slice.Regions, "total"));
				sample.TopTypeTotal = FormatDetail(GetTopDetail(slice.Types, "total"));
				sample.TopOriginTotal = FormatDetail(GetTopDetail(slice.Origins, "total"));
				sample.Worst = slice.Worst != null ? slice.Worst.Clone() : null;

				lock (m_Lock)
				{
					foreach (SliceRunStat run in slice.Runs.Values)
					{
						BurstSummary burst;

						if (run.RunId > 0 && m_Bursts.TryGetValue(run.RunId, out burst))
						{
							if (run.CallbackCount > burst.MaxCallbacksInSlice)
								burst.MaxCallbacksInSlice = run.CallbackCount;

							if (run.TotalMs > burst.MaxSpawnEntryMsInSlice)
								burst.MaxSpawnEntryMsInSlice = run.TotalMs;

							if (sliceMs > burst.MaxSliceMs)
								burst.MaxSliceMs = sliceMs;
						}
					}

					m_LastSpawnEntrySlice = sample.Clone();

					if (m_MaxSpawnEntrySlice == null || sample.SliceMs > m_MaxSpawnEntrySlice.SliceMs)
						m_MaxSpawnEntrySlice = sample.Clone();
				}
			}
			catch
			{
				Interlocked.Increment(ref m_DiagnosticErrors);
			}
		}

		public static void EndTimerSlice(long sliceId)
		{
			SliceAccumulator slice = t_CurrentSlice;

			if (slice != null && slice.Id == sliceId)
				t_CurrentSlice = null;
		}

		public static void RecordTaskManager150Target(string targetType, string action, string spawnerKind, string entryKey, string regionKey, string definitionKey)
		{
			try
			{
				TaskManagerCorrelationContext.DirectContext direct = TaskManagerCorrelationContext.GetDirectContextSnapshot();

				if (!direct.Active || !String.Equals(direct.TaskName, "TaskManager150Min", StringComparison.Ordinal))
					return;

				targetType = CleanKey(targetType, "targetType");
				action = CleanKey(action, "action");
				spawnerKind = CleanKey(spawnerKind, "spawner");

				lock (m_Lock)
				{
					BurstSummary burst = GetOrCreateBurst(direct);
					AddDetail(burst.TargetTypes, MaxBurstDetailEntries, targetType, targetType + " spawner=" + spawnerKind, 0);
					AddDetail(burst.TargetActions, MaxBurstDetailEntries, action, action, 0);
				}
			}
			catch
			{
				Interlocked.Increment(ref m_DiagnosticErrors);
			}
		}

		public static void RecordSchedule(string entryKey, string entryDescription, string regionKey, string regionDescription, string entryType, string definitionKey, string definitionDescription, long delayMs)
		{
			RecordSchedule(entryKey, entryDescription, regionKey, regionDescription, entryType, definitionKey, definitionDescription, delayMs, ScheduleReason.Other, null, 0, false, true, -1, -1, -1);
		}

		public static void RecordSchedule(string entryKey, string entryDescription, string regionKey, string regionDescription, string entryType, string definitionKey, string definitionDescription, long delayMs, ScheduleReason reason, string removedTargetType, int removedSerial, bool timerAlreadyExisted, bool timerCreated, int beforeSpawnedCount, int afterSpawnedCount, int maxCount)
		{
			try
			{
				if (delayMs < 0)
					delayMs = 0;

				entryKey = CleanKey(entryKey, "entry");
				regionKey = CleanKey(regionKey, "region");
				entryType = CleanKey(entryType, "entryType");
				definitionKey = CleanKey(definitionKey, "definition");
				entryDescription = CleanDescription(entryDescription, entryKey);
				regionDescription = CleanDescription(regionDescription, regionKey);
				definitionDescription = CleanDescription(definitionDescription, definitionKey);
				removedTargetType = CleanKey(removedTargetType, "none");

				TaskManagerCorrelationContext.DirectContext direct = TaskManagerCorrelationContext.GetDirectContextSnapshot();
				string[] recentLabels = TaskManagerCorrelationContext.GetRecentCorrelationLabels();
				bool directOrigin = direct.Active;
				bool recentCorrelation = HasUsefulRecentCorrelation(recentLabels);

				if (removedTargetType == "none" && direct.Active && !String.IsNullOrWhiteSpace(direct.TargetType))
					removedTargetType = direct.TargetType;

				ScheduleSample sample = new ScheduleSample();
				sample.TimeUtc = DateTime.UtcNow;
				sample.DelayMs = delayMs;
				sample.EntryKey = entryKey;
				sample.Entry = entryDescription;
				sample.RegionKey = regionKey;
				sample.Region = regionDescription;
				sample.EntryType = entryType;
				sample.DefinitionKey = definitionKey;
				sample.Definition = definitionDescription;
				sample.Reason = reason;
				sample.RemovedTargetType = removedTargetType;
				sample.RemovedSerial = removedSerial;
				sample.TimerAlreadyExisted = timerAlreadyExisted;
				sample.TimerCreated = timerCreated;
				sample.BeforeSpawnedCount = beforeSpawnedCount;
				sample.AfterSpawnedCount = afterSpawnedCount;
				sample.MaxCount = maxCount;
				sample.DirectOrigin = direct.Active ? direct.DirectOrigin : "none";
				sample.RecentCorrelation = JoinLabels(recentLabels);
				sample.TargetAction = direct.Active && !String.IsNullOrWhiteSpace(direct.TargetAction) ? direct.TargetAction : "none";

				lock (m_Lock)
				{
					RotateIfNeeded();
					CleanupOriginChains(DateTime.UtcNow);

					OriginChain chain = null;

					if (directOrigin)
						chain = CreateOrUpdateOriginChain(sample, direct);
					else
						m_OriginChains.TryGetValue(entryKey, out chain);

					if (chain != null)
					{
						++chain.Schedules;
						chain.LastTouchedUtc = sample.TimeUtc;
						sample.OriginRunId = chain.RunId;
					}

					m_Current.AddSchedule(delayMs, directOrigin, recentCorrelation);
					m_Total.AddSchedule(delayMs, directOrigin, recentCorrelation);
					AddScheduleToDetails(m_CurrentScheduleEntries, m_CurrentScheduleRegions, m_CurrentScheduleDefinitions, m_CurrentScheduleReasons, m_CurrentScheduleRemovedTypes, m_CurrentScheduleDirectOrigins, sample);
					AddScheduleToDetails(m_TotalScheduleEntries, m_TotalScheduleRegions, m_TotalScheduleDefinitions, m_TotalScheduleReasons, m_TotalScheduleRemovedTypes, m_TotalScheduleDirectOrigins, sample);
					AddScheduleCorrelations(m_CurrentRecentCorrelations, recentLabels, delayMs);
					AddScheduleCorrelations(m_TotalRecentCorrelations, recentLabels, delayMs);

					BurstSummary burst = null;

					if (sample.OriginRunId > 0 && m_Bursts.TryGetValue(sample.OriginRunId, out burst))
						AddScheduleToBurst(burst, sample);
					else if (direct.Active)
						AddScheduleToBurst(GetOrCreateBurst(direct), sample);
				}
			}
			catch
			{
				Interlocked.Increment(ref m_DiagnosticErrors);
			}
		}

		public static void RecordCallback(string entryKey, string entryDescription, string regionKey, string regionDescription, string entryType, string definitionKey, string definitionDescription, int spawnedCount, int maxCount, bool running, bool spawning, long elapsedMs)
		{
			RecordCallback(entryKey, entryDescription, regionKey, regionDescription, entryType, definitionKey, definitionDescription, spawnedCount, -1, spawnedCount, spawnedCount, 0, maxCount, running, spawning, false, -1, elapsedMs);
		}

		public static void RecordCallback(string entryKey, string entryDescription, string regionKey, string regionDescription, string entryType, string definitionKey, string definitionDescription, int beforeSpawnedCount, int requestedAmount, int afterSpawnedCount, int spawnedThisCallback, int spawnedCount, int maxCount, bool running, bool spawning, bool callbackRescheduled, long nextDelayMs, long elapsedMs)
		{
			try
			{
				if (elapsedMs < 0)
					elapsedMs = 0;

				entryKey = CleanKey(entryKey, "entry");
				regionKey = CleanKey(regionKey, "region");
				entryType = CleanKey(entryType, "entryType");
				definitionKey = CleanKey(definitionKey, "definition");
				entryDescription = CleanDescription(entryDescription, entryKey);
				regionDescription = CleanDescription(regionDescription, regionKey);
				definitionDescription = CleanDescription(definitionDescription, definitionKey);

				string[] recentLabels = TaskManagerCorrelationContext.GetRecentCorrelationLabels();
				DateTime now = DateTime.UtcNow;

				Sample sample = new Sample();
				sample.TimeUtc = now;
				sample.ElapsedMs = elapsedMs;
				sample.Entry = entryDescription;
				sample.Region = regionDescription;
				sample.EntryType = entryType;
				sample.Definition = definitionDescription;
				sample.BeforeSpawnedCount = beforeSpawnedCount;
				sample.RequestedAmount = requestedAmount;
				sample.SpawnedCount = spawnedCount;
				sample.MaxCount = maxCount;
				sample.SpawnedThisCallback = spawnedThisCallback;
				sample.Running = running;
				sample.Spawning = spawning;
				sample.CallbackRescheduled = callbackRescheduled;
				sample.NextDelayMs = nextDelayMs;
				sample.RecentCorrelation = JoinLabels(recentLabels);
				sample.DirectOrigin = "none";
				sample.OriginChain = "none";
				sample.ScheduleReason = "none";
				sample.RemovedTargetType = "none";

				lock (m_Lock)
				{
					RotateIfNeeded();
					CleanupOriginChains(now);

					OriginChain chain;

					if (m_OriginChains.TryGetValue(entryKey, out chain))
					{
						sample.OriginRunId = chain.RunId;
						sample.DirectOrigin = chain.OriginLabel;
						sample.OriginChain = FormatChainBrief(chain);
						sample.ScheduleReason = chain.ScheduleReason;
						sample.RemovedTargetType = chain.RemovedTargetType;

						RecordCallbackToChain(chain, sample, spawnedCount, maxCount, running, spawning);
					}

					AddCallbackToWindow(m_Current, m_CurrentEntries, m_CurrentRegions, m_CurrentTypes, m_CurrentDefinitions, m_CurrentRecentCorrelations, sample, entryKey, entryDescription, regionKey, regionDescription, entryType, definitionKey, definitionDescription, recentLabels);
					AddCallbackToWindow(m_Total, m_TotalEntries, m_TotalRegions, m_TotalTypes, m_TotalDefinitions, m_TotalRecentCorrelations, sample, entryKey, entryDescription, regionKey, regionDescription, entryType, definitionKey, definitionDescription, recentLabels);

					if (elapsedMs >= SpawnEntrySlowThresholdMs)
					{
						if (m_RecentSlowSamples.Count >= SpawnEntryMaxRecentSamples)
							m_RecentSlowSamples.Dequeue();

						m_RecentSlowSamples.Enqueue(sample.Clone());
					}
				}

				RecordSliceCallback(sample, entryKey, entryDescription, regionKey, regionDescription, entryType);
				LogSlowCallback(sample);
			}
			catch
			{
				Interlocked.Increment(ref m_DiagnosticErrors);
			}
		}

		public static void Reset()
		{
			lock (m_Lock)
			{
				m_Current.Clear();
				m_Last.Clear();
				m_Total.Clear();
				ClearCallbackDetails();
				ClearScheduleDetails();
				m_CurrentRecentCorrelations.Clear();
				m_LastRecentCorrelations.Clear();
				m_TotalRecentCorrelations.Clear();
				m_OriginChains.Clear();
				m_Bursts.Clear();
				m_BurstOrder.Clear();
				m_RecentSlowSamples.Clear();
				m_LastSpawnEntrySlice = null;
				m_MaxSpawnEntrySlice = null;
				m_NextWindow = 0;
				m_DiagnosticErrors = 0;
			}
		}

		public static void WriteStats(TextWriter writer)
		{
			if (writer == null)
				return;

			WindowStats current;
			WindowStats last;
			WindowStats total;
			DetailStat[] currentEntries;
			DetailStat[] lastEntries;
			DetailStat[] totalEntries;
			DetailStat[] currentRegions;
			DetailStat[] lastRegions;
			DetailStat[] totalRegions;
			DetailStat[] currentTypes;
			DetailStat[] lastTypes;
			DetailStat[] totalTypes;
			DetailStat[] currentDefinitions;
			DetailStat[] lastDefinitions;
			DetailStat[] totalDefinitions;
			DetailStat[] currentScheduleEntries;
			DetailStat[] lastScheduleEntries;
			DetailStat[] totalScheduleEntries;
			DetailStat[] currentScheduleRegions;
			DetailStat[] lastScheduleRegions;
			DetailStat[] totalScheduleRegions;
			DetailStat[] currentScheduleDefinitions;
			DetailStat[] lastScheduleDefinitions;
			DetailStat[] totalScheduleDefinitions;
			DetailStat[] currentScheduleReasons;
			DetailStat[] lastScheduleReasons;
			DetailStat[] totalScheduleReasons;
			DetailStat[] currentScheduleRemovedTypes;
			DetailStat[] lastScheduleRemovedTypes;
			DetailStat[] totalScheduleRemovedTypes;
			DetailStat[] currentScheduleDirectOrigins;
			DetailStat[] lastScheduleDirectOrigins;
			DetailStat[] totalScheduleDirectOrigins;
			CorrelationStat[] currentRecentCorrelations;
			CorrelationStat[] lastRecentCorrelations;
			CorrelationStat[] totalRecentCorrelations;
			Sample[] recentSlow;
			SliceSample lastSlice;
			SliceSample maxSlice;
			long diagnosticErrors;
			int activeChains;
			int completedChains;
			int trackedChains;

			lock (m_Lock)
			{
				RotateIfNeeded();
				CleanupOriginChains(DateTime.UtcNow);
				current = m_Current.Clone();
				last = m_Last.Clone();
				total = m_Total.Clone();
				currentEntries = CopyDetailArray(m_CurrentEntries);
				lastEntries = CopyDetailArray(m_LastEntries);
				totalEntries = CopyDetailArray(m_TotalEntries);
				currentRegions = CopyDetailArray(m_CurrentRegions);
				lastRegions = CopyDetailArray(m_LastRegions);
				totalRegions = CopyDetailArray(m_TotalRegions);
				currentTypes = CopyDetailArray(m_CurrentTypes);
				lastTypes = CopyDetailArray(m_LastTypes);
				totalTypes = CopyDetailArray(m_TotalTypes);
				currentDefinitions = CopyDetailArray(m_CurrentDefinitions);
				lastDefinitions = CopyDetailArray(m_LastDefinitions);
				totalDefinitions = CopyDetailArray(m_TotalDefinitions);
				currentScheduleEntries = CopyDetailArray(m_CurrentScheduleEntries);
				lastScheduleEntries = CopyDetailArray(m_LastScheduleEntries);
				totalScheduleEntries = CopyDetailArray(m_TotalScheduleEntries);
				currentScheduleRegions = CopyDetailArray(m_CurrentScheduleRegions);
				lastScheduleRegions = CopyDetailArray(m_LastScheduleRegions);
				totalScheduleRegions = CopyDetailArray(m_TotalScheduleRegions);
				currentScheduleDefinitions = CopyDetailArray(m_CurrentScheduleDefinitions);
				lastScheduleDefinitions = CopyDetailArray(m_LastScheduleDefinitions);
				totalScheduleDefinitions = CopyDetailArray(m_TotalScheduleDefinitions);
				currentScheduleReasons = CopyDetailArray(m_CurrentScheduleReasons);
				lastScheduleReasons = CopyDetailArray(m_LastScheduleReasons);
				totalScheduleReasons = CopyDetailArray(m_TotalScheduleReasons);
				currentScheduleRemovedTypes = CopyDetailArray(m_CurrentScheduleRemovedTypes);
				lastScheduleRemovedTypes = CopyDetailArray(m_LastScheduleRemovedTypes);
				totalScheduleRemovedTypes = CopyDetailArray(m_TotalScheduleRemovedTypes);
				currentScheduleDirectOrigins = CopyDetailArray(m_CurrentScheduleDirectOrigins);
				lastScheduleDirectOrigins = CopyDetailArray(m_LastScheduleDirectOrigins);
				totalScheduleDirectOrigins = CopyDetailArray(m_TotalScheduleDirectOrigins);
				currentRecentCorrelations = CopyCorrelationArray(m_CurrentRecentCorrelations);
				lastRecentCorrelations = CopyCorrelationArray(m_LastRecentCorrelations);
				totalRecentCorrelations = CopyCorrelationArray(m_TotalRecentCorrelations);
				recentSlow = m_RecentSlowSamples.ToArray();
				lastSlice = m_LastSpawnEntrySlice != null ? m_LastSpawnEntrySlice.Clone() : null;
				maxSlice = m_MaxSpawnEntrySlice != null ? m_MaxSpawnEntrySlice.Clone() : null;
				diagnosticErrors = m_DiagnosticErrors;
				GetOriginChainCountsNoLock(out activeChains, out completedChains, out trackedChains);
			}

			WriteWindow(writer, "current", current);
			WriteWindow(writer, "last", last);
			WriteWindow(writer, "sinceBoot", total);
			writer.WriteLine("SpawnEntry attribution status: subphases=Skipped because passive attribution was unsafe scheduling=enabled scheduleReasons=enabled directOrigins=enabled originChains=enabled burstSummaries=enabled dryRunProjections=enabled timerSliceAccumulation=enabled activeOriginChains={0} completedOriginChains={1} trackedOriginChains={2} diagnosticErrors={3}", activeChains, completedChains, trackedChains, diagnosticErrors);
			WriteCallbackTopScope(writer, "current", currentEntries, currentRegions, currentTypes, currentDefinitions, current.Worst);
			WriteCallbackTopScope(writer, "last", lastEntries, lastRegions, lastTypes, lastDefinitions, last.Worst);
			WriteCallbackTopScope(writer, "sinceBoot", totalEntries, totalRegions, totalTypes, totalDefinitions, total.Worst);
			WriteScheduleTopScope(writer, "current", currentScheduleEntries, currentScheduleRegions, currentScheduleDefinitions, currentScheduleReasons, currentScheduleRemovedTypes, currentScheduleDirectOrigins);
			WriteScheduleTopScope(writer, "last", lastScheduleEntries, lastScheduleRegions, lastScheduleDefinitions, lastScheduleReasons, lastScheduleRemovedTypes, lastScheduleDirectOrigins);
			WriteScheduleTopScope(writer, "sinceBoot", totalScheduleEntries, totalScheduleRegions, totalScheduleDefinitions, totalScheduleReasons, totalScheduleRemovedTypes, totalScheduleDirectOrigins);
			WriteCorrelations(writer, "recent current", currentRecentCorrelations);
			WriteCorrelations(writer, "recent last", lastRecentCorrelations);
			WriteCorrelations(writer, "recent sinceBoot", totalRecentCorrelations);
			WriteSlice(writer, "last", lastSlice);
			WriteSlice(writer, "max", maxSlice);
			WriteRecentSlow(writer, recentSlow);
		}

		public static void WriteBurstStats(TextWriter writer)
		{
			if (writer == null)
				return;

			BurstSummary[] bursts;
			int activeChains;
			int completedChains;
			int trackedChains;
			long diagnosticErrors;

			lock (m_Lock)
			{
				CleanupOriginChains(DateTime.UtcNow);
				bursts = CopyBurstArray();
				GetOriginChainCountsNoLock(out activeChains, out completedChains, out trackedChains);
				diagnosticErrors = m_DiagnosticErrors;
			}

			Array.Sort(bursts, delegate(BurstSummary a, BurstSummary b)
			{
				if (a == null && b == null)
					return 0;

				if (a == null)
					return 1;

				if (b == null)
					return -1;

				return b.RunId.CompareTo(a.RunId);
			});

			writer.WriteLine("SpawnEntryBurstStats: recentBursts={0} activeOriginChains={1} completedOriginChains={2} trackedOriginChains={3} diagnosticErrors={4}", bursts.Length, activeChains, completedChains, trackedChains, diagnosticErrors);

			int count = Math.Min(8, bursts.Length);

			for (int i = 0; i < count; ++i)
				WriteBurst(writer, i + 1, bursts[i]);
		}

		public static PerformanceSummary GetPerformanceTestSummary()
		{
			PerformanceSummary summary = new PerformanceSummary();

			lock (m_Lock)
			{
				CleanupOriginChains(DateTime.UtcNow);
				summary.TotalCallbackMs = m_Total.TotalMs;
				summary.DirectSchedules = m_Total.DirectOriginScheduleCount;
				summary.UnknownOriginCallbacks = m_Total.NoOriginCallbackCount;
				GetOriginChainCountsNoLock(out summary.ActiveOriginChains, out summary.CompletedOriginChains, out summary.TrackedOriginChains);

				BurstSummary latest = null;

				for (int i = 0; i < m_BurstOrder.Count; ++i)
				{
					BurstSummary candidate;

					if (m_Bursts.TryGetValue(m_BurstOrder[i], out candidate) && candidate != null && (latest == null || candidate.RunId > latest.RunId))
						latest = candidate;
				}

				if (latest != null)
				{
					summary.LatestBurstSchedules = latest.Schedules;
					summary.LatestBurstCallbacks = latest.Callbacks;
					summary.LatestBurstCallbackMs = latest.CallbackMs;
					summary.LatestBurstRunId = latest.RunId;
					summary.LatestBurstTask = latest.TaskName;
				}
			}

			return summary;
		}

		private static void GetOriginChainCountsNoLock(out int active, out int completed, out int tracked)
		{
			active = 0;
			completed = 0;
			tracked = m_OriginChains.Count;

			foreach (OriginChain chain in m_OriginChains.Values)
			{
				if (chain != null && chain.Completed)
					++completed;
				else
					++active;
			}
		}

		private static void AddScheduleToDetails(Dictionary<string, DetailStat> entries, Dictionary<string, DetailStat> regions, Dictionary<string, DetailStat> definitions, Dictionary<string, DetailStat> reasons, Dictionary<string, DetailStat> removedTypes, Dictionary<string, DetailStat> directOrigins, ScheduleSample sample)
		{
			AddDetail(entries, SpawnEntryMaxTrackedScheduleDetails, sample.EntryKey, sample.Entry, sample.DelayMs);
			AddDetail(regions, SpawnEntryMaxTrackedRegions, sample.RegionKey, sample.Region, sample.DelayMs);
			AddDetail(definitions, SpawnEntryMaxTrackedDefinitions, sample.DefinitionKey, sample.Definition, sample.DelayMs);
			AddDetail(reasons, SpawnEntryMaxTrackedTypes, sample.Reason.ToString(), sample.Reason.ToString(), sample.DelayMs);
			AddDetail(removedTypes, SpawnEntryMaxTrackedTypes, sample.RemovedTargetType, sample.RemovedTargetType, sample.DelayMs);

			if (!String.IsNullOrWhiteSpace(sample.DirectOrigin) && sample.DirectOrigin != "none")
				AddDetail(directOrigins, SpawnEntryMaxTrackedCorrelations, sample.DirectOrigin, sample.DirectOrigin, sample.DelayMs);
		}

		private static OriginChain CreateOrUpdateOriginChain(ScheduleSample sample, TaskManagerCorrelationContext.DirectContext direct)
		{
			OriginChain chain;

			if (!m_OriginChains.TryGetValue(sample.EntryKey, out chain))
			{
				if (m_OriginChains.Count >= MaxTrackedOriginChains)
					RemoveOldestOriginChain();

				chain = new OriginChain();
				chain.EntryKey = sample.EntryKey;
				m_OriginChains[sample.EntryKey] = chain;
			}

			chain.EntryDescription = sample.Entry;
			chain.RegionKey = sample.RegionKey;
			chain.RegionDescription = sample.Region;
			chain.DefinitionKey = sample.DefinitionKey;
			chain.DefinitionDescription = sample.Definition;
			chain.RunId = direct.RunId;
			chain.TaskName = direct.TaskName;
			chain.TimerName = direct.TimerName;
			chain.PhaseName = direct.PhaseName;
			chain.ScheduleReason = sample.Reason.ToString();
			chain.RemovedTargetType = sample.RemovedTargetType;
			chain.TargetAction = !String.IsNullOrWhiteSpace(direct.TargetAction) ? direct.TargetAction : sample.TargetAction;
			chain.ScheduleUtc = sample.TimeUtc;
			chain.LastTouchedUtc = sample.TimeUtc;
			chain.Completed = false;
			chain.FinalSpawnedCount = sample.AfterSpawnedCount;
			chain.MaxCount = sample.MaxCount;

			GetOrCreateBurst(direct);
			return chain;
		}

		private static void RecordCallbackToChain(OriginChain chain, Sample sample, int spawnedCount, int maxCount, bool running, bool spawning)
		{
			if (chain == null)
				return;

			++chain.CallbackCount;
			chain.CallbackMs += sample.ElapsedMs;

			if (sample.ElapsedMs > chain.MaxCallbackMs)
				chain.MaxCallbackMs = sample.ElapsedMs;

			if (chain.FirstCallbackUtc == DateTime.MinValue)
				chain.FirstCallbackUtc = sample.TimeUtc;

			chain.LastCallbackUtc = sample.TimeUtc;
			chain.LastTouchedUtc = sample.TimeUtc;
			chain.FinalSpawnedCount = spawnedCount;
			chain.MaxCount = maxCount;

			BurstSummary burst;

			if (m_Bursts.TryGetValue(chain.RunId, out burst))
				AddCallbackToBurst(burst, chain, sample, running, spawning);

			if (!spawning || !running || (maxCount >= 0 && spawnedCount >= maxCount))
				chain.Completed = true;
		}

		private static BurstSummary GetOrCreateBurst(TaskManagerCorrelationContext.DirectContext direct)
		{
			BurstSummary burst;

			if (m_Bursts.TryGetValue(direct.RunId, out burst))
				return burst;

			if (m_Bursts.Count >= MaxBurstSummaries && m_BurstOrder.Count > 0)
			{
				long oldRunId = m_BurstOrder[0];
				m_BurstOrder.RemoveAt(0);
				m_Bursts.Remove(oldRunId);
			}

			burst = new BurstSummary();
			burst.RunId = direct.RunId;
			burst.TaskName = direct.TaskName;
			burst.TimerName = direct.TimerName;
			burst.PhaseName = direct.PhaseName;
			burst.Startup = direct.Startup;
			burst.StartUtc = DateTime.UtcNow;
			burst.EndUtc = burst.StartUtc;
			m_Bursts[burst.RunId] = burst;
			m_BurstOrder.Add(burst.RunId);
			return burst;
		}

		private static void AddScheduleToBurst(BurstSummary burst, ScheduleSample sample)
		{
			if (burst == null)
				return;

			if (burst.StartUtc == DateTime.MinValue)
				burst.StartUtc = sample.TimeUtc;

			if (sample.TimeUtc > burst.EndUtc)
				burst.EndUtc = sample.TimeUtc;

			++burst.Schedules;
			AddDetail(burst.Entries, MaxBurstDetailEntries, sample.EntryKey, sample.Entry, 0);
			AddDetail(burst.Regions, MaxBurstDetailEntries, sample.RegionKey, sample.Region, 0);
			AddDetail(burst.Definitions, MaxBurstDetailEntries, sample.DefinitionKey, sample.Definition, 0);
			AddDetail(burst.RemovedTargetTypes, MaxBurstDetailEntries, sample.RemovedTargetType, sample.RemovedTargetType, 0);

			if (String.Equals(burst.TaskName, "TaskManager150Min", StringComparison.Ordinal) && String.Equals(burst.PhaseName, "processTargets", StringComparison.Ordinal))
				AddDryRunProjections(burst, sample);
		}

		private static void AddCallbackToBurst(BurstSummary burst, OriginChain chain, Sample sample, bool running, bool spawning)
		{
			if (burst == null)
				return;

			if (sample.TimeUtc > burst.EndUtc)
				burst.EndUtc = sample.TimeUtc;

			++burst.Callbacks;
			burst.CallbackMs += sample.ElapsedMs;

			if (sample.ElapsedMs > burst.MaxCallbackMs)
			{
				burst.MaxCallbackMs = sample.ElapsedMs;
				burst.ExactWorstEntry = FormatSample(sample);
			}

			if (sample.ElapsedMs >= SpawnEntrySlowThresholdMs)
				++burst.SlowCallbacks;

			AddDetail(burst.Entries, MaxBurstDetailEntries, chain.EntryKey, chain.EntryDescription, sample.ElapsedMs);
			AddDetail(burst.Regions, MaxBurstDetailEntries, chain.RegionKey, chain.RegionDescription, sample.ElapsedMs);
			AddDetail(burst.Definitions, MaxBurstDetailEntries, chain.DefinitionKey, chain.DefinitionDescription, sample.ElapsedMs);
			AddDetail(burst.RemovedTargetTypes, MaxBurstDetailEntries, chain.RemovedTargetType, chain.RemovedTargetType, 0);

			if (!spawning || !running || (sample.MaxCount >= 0 && sample.SpawnedCount >= sample.MaxCount))
			{
				long elapsed = (long)(sample.TimeUtc - chain.ScheduleUtc).TotalMilliseconds;

				if (elapsed <= 1000)
					++burst.EntriesCompleteAfter1s;

				if (elapsed <= 5000)
					++burst.EntriesCompleteAfter5s;

				if (elapsed <= 10000)
					++burst.EntriesCompleteAfter10s;

				if (elapsed > 30000)
					++burst.EntriesIncompleteAfter30s;
			}
		}

		private static void AddDryRunProjections(BurstSummary burst, ScheduleSample sample)
		{
			long actualOffsetMs = (long)(sample.TimeUtc - burst.StartUtc).TotalMilliseconds + sample.DelayMs;
			string hashKey = burst.RunId.ToString() + "|" + sample.EntryKey + "|" + sample.DefinitionKey + "|" + burst.Schedules.ToString();
			int hash = StableHash(hashKey);

			burst.ActualProjection.Add(actualOffsetMs, 0);
			burst.Spread5Projection.Add(actualOffsetMs, hash % 5000);
			burst.Spread10Projection.Add(actualOffsetMs, hash % 10000);
			burst.Spread15Projection.Add(actualOffsetMs, hash % 15000);
		}

		private static void AddCallbackToWindow(WindowStats window, Dictionary<string, DetailStat> entries, Dictionary<string, DetailStat> regions, Dictionary<string, DetailStat> types, Dictionary<string, DetailStat> definitions, Dictionary<string, CorrelationStat> recentCorrelations, Sample sample, string entryKey, string entryDescription, string regionKey, string regionDescription, string entryType, string definitionKey, string definitionDescription, string[] recentLabels)
		{
			window.AddCallback(sample);
			AddDetail(entries, SpawnEntryMaxTrackedEntries, entryKey, entryDescription, sample.ElapsedMs);
			AddDetail(regions, SpawnEntryMaxTrackedRegions, regionKey, regionDescription, sample.ElapsedMs);
			AddDetail(types, SpawnEntryMaxTrackedTypes, entryType, entryType, sample.ElapsedMs);
			AddDetail(definitions, SpawnEntryMaxTrackedDefinitions, definitionKey, definitionDescription, sample.ElapsedMs);
			AddCallbackCorrelations(recentCorrelations, recentLabels, sample, entryKey, entryDescription, regionKey, regionDescription, entryType);
		}

		private static void AddCallbackCorrelations(Dictionary<string, CorrelationStat> correlations, string[] labels, Sample sample, string entryKey, string entryDescription, string regionKey, string regionDescription, string entryType)
		{
			if (labels == null)
				return;

			for (int i = 0; i < labels.Length; ++i)
			{
				if (labels[i] == "none")
					continue;

				CorrelationStat stat = GetOrCreateCorrelation(correlations, labels[i]);

				if (stat == null)
					continue;

				stat.Stats.AddCallback(sample);
				AddDetail(stat.Entries, SpawnEntryMaxTrackedPerCorrelation, entryKey, entryDescription, sample.ElapsedMs);
				AddDetail(stat.Regions, SpawnEntryMaxTrackedPerCorrelation, regionKey, regionDescription, sample.ElapsedMs);
				AddDetail(stat.Types, SpawnEntryMaxTrackedPerCorrelation, entryType, entryType, sample.ElapsedMs);
			}
		}

		private static void AddScheduleCorrelations(Dictionary<string, CorrelationStat> correlations, string[] labels, long delayMs)
		{
			if (labels == null)
				return;

			for (int i = 0; i < labels.Length; ++i)
			{
				if (labels[i] == "none")
					continue;

				CorrelationStat stat = GetOrCreateCorrelation(correlations, labels[i]);

				if (stat != null)
					stat.Stats.AddSchedule(delayMs, false, true);
			}
		}

		private static CorrelationStat GetOrCreateCorrelation(Dictionary<string, CorrelationStat> correlations, string label)
		{
			label = CleanKey(label, "none");
			CorrelationStat stat;

			if (!correlations.TryGetValue(label, out stat))
			{
				if (correlations.Count >= SpawnEntryMaxTrackedCorrelations)
					return null;

				stat = new CorrelationStat();
				stat.Label = label;
				correlations[label] = stat;
			}

			return stat;
		}

		private static void RecordSliceCallback(Sample sample, string entryKey, string entryDescription, string regionKey, string regionDescription, string entryType)
		{
			SliceAccumulator slice = t_CurrentSlice;

			if (slice == null)
				return;

			++slice.Count;
			slice.TotalMs += sample.ElapsedMs;

			if (sample.ElapsedMs >= SpawnEntrySlowThresholdMs)
				++slice.SlowCount;

			if (slice.Worst == null || sample.ElapsedMs > slice.Worst.ElapsedMs)
				slice.Worst = sample.Clone();

			AddDetail(slice.Entries, SpawnEntryMaxTrackedPerCorrelation, entryKey, entryDescription, sample.ElapsedMs);
			AddDetail(slice.Regions, SpawnEntryMaxTrackedPerCorrelation, regionKey, regionDescription, sample.ElapsedMs);
			AddDetail(slice.Types, SpawnEntryMaxTrackedPerCorrelation, entryType, entryType, sample.ElapsedMs);

			if (!String.IsNullOrWhiteSpace(sample.DirectOrigin) && sample.DirectOrigin != "none")
				AddDetail(slice.Origins, SpawnEntryMaxTrackedPerCorrelation, sample.DirectOrigin, sample.DirectOrigin, sample.ElapsedMs);

			if (sample.OriginRunId > 0)
			{
				SliceRunStat run;

				if (!slice.Runs.TryGetValue(sample.OriginRunId, out run))
				{
					run = new SliceRunStat();
					run.RunId = sample.OriginRunId;
					slice.Runs[sample.OriginRunId] = run;
				}

				++run.CallbackCount;
				run.TotalMs += sample.ElapsedMs;

				if (sample.ElapsedMs > run.MaxMs)
					run.MaxMs = sample.ElapsedMs;
			}
		}

		private static void LogSlowCallback(Sample sample)
		{
			if (sample == null || sample.ElapsedMs < SpawnEntryVerySlowThresholdMs)
				return;

			long now = Core.TickCount;

			if (now < Interlocked.Read(ref m_NextSlowLog))
				return;

			Interlocked.Exchange(ref m_NextSlowLog, now + SpawnEntrySlowLogIntervalMs);
			DiagnosticConsole.WriteParsedLine(String.Format("[Lag] SpawnEntry.TimerCallback slow {0}ms entry={1} region={2} definition={3} directOrigin={4} recentCorrelation={5}", sample.ElapsedMs, sample.Entry, sample.Region, sample.Definition, sample.DirectOrigin, sample.RecentCorrelation));
		}

		private static void RotateIfNeeded()
		{
			long now = Core.TickCount;

			if (m_NextWindow == 0)
			{
				m_NextWindow = now + WindowMs;
				return;
			}

			if (now < m_NextWindow)
				return;

			m_NextWindow = now + WindowMs;
			m_Last = m_Current.Clone();
			m_Current.Clear();
			m_LastEntries = CloneDetails(m_CurrentEntries);
			m_CurrentEntries.Clear();
			m_LastRegions = CloneDetails(m_CurrentRegions);
			m_CurrentRegions.Clear();
			m_LastTypes = CloneDetails(m_CurrentTypes);
			m_CurrentTypes.Clear();
			m_LastDefinitions = CloneDetails(m_CurrentDefinitions);
			m_CurrentDefinitions.Clear();
			m_LastScheduleEntries = CloneDetails(m_CurrentScheduleEntries);
			m_CurrentScheduleEntries.Clear();
			m_LastScheduleRegions = CloneDetails(m_CurrentScheduleRegions);
			m_CurrentScheduleRegions.Clear();
			m_LastScheduleDefinitions = CloneDetails(m_CurrentScheduleDefinitions);
			m_CurrentScheduleDefinitions.Clear();
			m_LastScheduleReasons = CloneDetails(m_CurrentScheduleReasons);
			m_CurrentScheduleReasons.Clear();
			m_LastScheduleRemovedTypes = CloneDetails(m_CurrentScheduleRemovedTypes);
			m_CurrentScheduleRemovedTypes.Clear();
			m_LastScheduleDirectOrigins = CloneDetails(m_CurrentScheduleDirectOrigins);
			m_CurrentScheduleDirectOrigins.Clear();
			m_LastRecentCorrelations = CloneCorrelations(m_CurrentRecentCorrelations);
			m_CurrentRecentCorrelations.Clear();
		}

		private static void CleanupOriginChains(DateTime now)
		{
			List<string> remove = null;

			foreach (KeyValuePair<string, OriginChain> kv in m_OriginChains)
			{
				if ((now - kv.Value.LastTouchedUtc).TotalMinutes >= OriginChainTtlMinutes)
				{
					if (remove == null)
						remove = new List<string>();

					remove.Add(kv.Key);
				}
			}

			if (remove != null)
			{
				for (int i = 0; i < remove.Count; ++i)
					m_OriginChains.Remove(remove[i]);
			}
		}

		private static void RemoveOldestOriginChain()
		{
			string oldestKey = null;
			DateTime oldest = DateTime.MaxValue;

			foreach (KeyValuePair<string, OriginChain> kv in m_OriginChains)
			{
				if (kv.Value.LastTouchedUtc < oldest)
				{
					oldest = kv.Value.LastTouchedUtc;
					oldestKey = kv.Key;
				}
			}

			if (oldestKey != null)
				m_OriginChains.Remove(oldestKey);
		}

		private static void AddDetail(Dictionary<string, DetailStat> stats, int maxEntries, string key, string description, long elapsedMs)
		{
			DetailStat stat;

			key = CleanKey(key, "none");
			description = CleanDescription(description, key);

			if (!stats.TryGetValue(key, out stat))
			{
				if (stats.Count >= maxEntries)
					return;

				stat = new DetailStat();
				stat.Key = key;
				stat.Description = description;
				stats[key] = stat;
			}

			stat.Add(elapsedMs);
		}

		private static Dictionary<string, DetailStat> CloneDetails(Dictionary<string, DetailStat> source)
		{
			Dictionary<string, DetailStat> copy = new Dictionary<string, DetailStat>();

			foreach (KeyValuePair<string, DetailStat> kv in source)
				copy[kv.Key] = kv.Value.Clone();

			return copy;
		}

		private static Dictionary<int, long> CloneLongDictionary(Dictionary<int, long> source)
		{
			Dictionary<int, long> copy = new Dictionary<int, long>();

			foreach (KeyValuePair<int, long> kv in source)
				copy[kv.Key] = kv.Value;

			return copy;
		}

		private static Dictionary<string, CorrelationStat> CloneCorrelations(Dictionary<string, CorrelationStat> source)
		{
			Dictionary<string, CorrelationStat> copy = new Dictionary<string, CorrelationStat>();

			foreach (KeyValuePair<string, CorrelationStat> kv in source)
				copy[kv.Key] = kv.Value.Clone();

			return copy;
		}

		private static DetailStat[] CopyDetailArray(Dictionary<string, DetailStat> source)
		{
			DetailStat[] stats = new DetailStat[source.Count];
			int index = 0;

			foreach (DetailStat stat in source.Values)
				stats[index++] = stat.Clone();

			return stats;
		}

		private static CorrelationStat[] CopyCorrelationArray(Dictionary<string, CorrelationStat> source)
		{
			CorrelationStat[] stats = new CorrelationStat[source.Count];
			int index = 0;

			foreach (CorrelationStat stat in source.Values)
				stats[index++] = stat.Clone();

			return stats;
		}

		private static BurstSummary[] CopyBurstArray()
		{
			BurstSummary[] bursts = new BurstSummary[m_Bursts.Count];
			int index = 0;

			foreach (BurstSummary burst in m_Bursts.Values)
				bursts[index++] = burst.Clone();

			return bursts;
		}

		private static DetailStat GetTopDetail(Dictionary<string, DetailStat> stats, string mode)
		{
			DetailStat best = null;

			foreach (DetailStat stat in stats.Values)
				best = BetterDetail(best, stat, mode);

			return best;
		}

		private static DetailStat GetTopDetail(DetailStat[] stats, string mode)
		{
			DetailStat best = null;

			if (stats == null)
				return null;

			for (int i = 0; i < stats.Length; ++i)
				best = BetterDetail(best, stats[i], mode);

			return best;
		}

		private static DetailStat BetterDetail(DetailStat best, DetailStat stat, string mode)
		{
			if (stat == null)
				return best;

			if (best == null)
				return stat;

			if (mode == "total" && stat.TotalMs > best.TotalMs)
				return stat;

			if (mode == "max" && stat.MaxMs > best.MaxMs)
				return stat;

			if (mode == "count" && stat.Count > best.Count)
				return stat;

			return best;
		}

		private static void WriteWindow(TextWriter writer, string label, WindowStats stats)
		{
			long avg = stats.CallbackCount > 0 ? stats.TotalMs / stats.CallbackCount : 0;
			long avgScheduleDelay = stats.ScheduleCount > 0 ? stats.TotalScheduleDelayMs / stats.ScheduleCount : 0;

			writer.WriteLine("SpawnEntryStats {0}: callbacks={1} total={2}ms max={3}ms avg={4}ms slow={5} verySlow={6} schedules={7} immediateSchedules={8} nearImmediateSchedules={9} avgScheduleDelayMs={10} directSchedules={11} recentOnlySchedules={12} noOriginSchedules={13} originChainCallbacks={14} noOriginCallbacks={15}",
				label,
				stats.CallbackCount,
				stats.TotalMs,
				stats.MaxMs,
				avg,
				stats.SlowCount,
				stats.VerySlowCount,
				stats.ScheduleCount,
				stats.ImmediateScheduleCount,
				stats.NearImmediateScheduleCount,
				avgScheduleDelay,
				stats.DirectOriginScheduleCount,
				stats.RecentCorrelationScheduleCount,
				stats.NoOriginScheduleCount,
				stats.OriginChainCallbackCount,
				stats.NoOriginCallbackCount);
		}

		private static void WriteCallbackTopScope(TextWriter writer, string label, DetailStat[] entries, DetailStat[] regions, DetailStat[] types, DetailStat[] definitions, Sample worst)
		{
			writer.WriteLine("SpawnEntry callback tops {0}: entryTotal={1}", label, FormatDetail(GetTopDetail(entries, "total")));
			writer.WriteLine("SpawnEntry callback tops {0}: entryMax={1}", label, FormatDetail(GetTopDetail(entries, "max")));
			writer.WriteLine("SpawnEntry callback tops {0}: entryCount={1}", label, FormatDetail(GetTopDetail(entries, "count")));
			writer.WriteLine("SpawnEntry callback tops {0}: regionTotal={1}", label, FormatDetail(GetTopDetail(regions, "total")));
			writer.WriteLine("SpawnEntry callback tops {0}: regionMax={1}", label, FormatDetail(GetTopDetail(regions, "max")));
			writer.WriteLine("SpawnEntry callback tops {0}: regionCount={1}", label, FormatDetail(GetTopDetail(regions, "count")));
			writer.WriteLine("SpawnEntry callback tops {0}: entryTypeTotal={1}", label, FormatDetail(GetTopDetail(types, "total")));
			writer.WriteLine("SpawnEntry callback tops {0}: entryTypeMax={1}", label, FormatDetail(GetTopDetail(types, "max")));
			writer.WriteLine("SpawnEntry callback tops {0}: entryTypeCount={1}", label, FormatDetail(GetTopDetail(types, "count")));
			writer.WriteLine("SpawnEntry callback tops {0}: definitionTotal={1}", label, FormatDetail(GetTopDetail(definitions, "total")));
			writer.WriteLine("SpawnEntry callback tops {0}: definitionMax={1}", label, FormatDetail(GetTopDetail(definitions, "max")));
			writer.WriteLine("SpawnEntry callback tops {0}: definitionCount={1}", label, FormatDetail(GetTopDetail(definitions, "count")));
			writer.WriteLine("SpawnEntry callback tops {0}: exactWorst={1}", label, FormatSample(worst));
		}

		private static void WriteScheduleTopScope(TextWriter writer, string label, DetailStat[] entries, DetailStat[] regions, DetailStat[] definitions, DetailStat[] reasons, DetailStat[] removedTypes, DetailStat[] directOrigins)
		{
			writer.WriteLine("SpawnEntry schedule tops {0}: entryCount={1}", label, FormatDetail(GetTopDetail(entries, "count")));
			writer.WriteLine("SpawnEntry schedule tops {0}: regionCount={1}", label, FormatDetail(GetTopDetail(regions, "count")));
			writer.WriteLine("SpawnEntry schedule tops {0}: definitionCount={1}", label, FormatDetail(GetTopDetail(definitions, "count")));
			writer.WriteLine("SpawnEntry schedule tops {0}: reasonCount={1}", label, FormatDetail(GetTopDetail(reasons, "count")));
			writer.WriteLine("SpawnEntry schedule tops {0}: removedTargetTypeCount={1}", label, FormatDetail(GetTopDetail(removedTypes, "count")));
			writer.WriteLine("SpawnEntry schedule tops {0}: directOriginCount={1}", label, FormatDetail(GetTopDetail(directOrigins, "count")));
		}

		private static void WriteCorrelations(TextWriter writer, string label, CorrelationStat[] stats)
		{
			writer.WriteLine("SpawnEntry recent correlations {0}: entries={1}", label, stats.Length);

			Array.Sort(stats, delegate(CorrelationStat a, CorrelationStat b)
			{
				if (a == null && b == null)
					return 0;

				if (a == null)
					return 1;

				if (b == null)
					return -1;

				long aScore = a.Stats.TotalMs + a.Stats.ScheduleCount;
				long bScore = b.Stats.TotalMs + b.Stats.ScheduleCount;
				return bScore.CompareTo(aScore);
			});

			int count = Math.Min(12, stats.Length);

			for (int i = 0; i < count; ++i)
			{
				CorrelationStat stat = stats[i];

				if (stat == null)
					continue;

				long avg = stat.Stats.CallbackCount > 0 ? stat.Stats.TotalMs / stat.Stats.CallbackCount : 0;
				writer.WriteLine("  recentCorrelation#{0}: {1} callbacks={2} total={3}ms max={4}ms avg={5}ms slow={6} schedules={7} nearImmediateSchedules={8} topEntryTotal={9} topEntryMax={10} topRegionTotal={11} topTypeTotal={12} worst={13}",
					i + 1,
					stat.Label,
					stat.Stats.CallbackCount,
					stat.Stats.TotalMs,
					stat.Stats.MaxMs,
					avg,
					stat.Stats.SlowCount,
					stat.Stats.ScheduleCount,
					stat.Stats.NearImmediateScheduleCount,
					FormatDetail(GetTopDetail(stat.Entries, "total")),
					FormatDetail(GetTopDetail(stat.Entries, "max")),
					FormatDetail(GetTopDetail(stat.Regions, "total")),
					FormatDetail(GetTopDetail(stat.Types, "total")),
					FormatSample(stat.Stats.Worst));
			}
		}

		private static void WriteSlice(TextWriter writer, string label, SliceSample sample)
		{
			writer.Write("SpawnEntrySlice {0}: ", label);

			if (sample == null)
			{
				writer.WriteLine("Unavailable");
				return;
			}

			writer.WriteLine("slice={0}ms queue={1} processed={2} named={3}ms other={4}ms spawnEntry={5}ms callbacks={6} slow={7} topEntryTotal={8} topEntryMax={9} topRegionTotal={10} topTypeTotal={11} topDirectOrigin={12} worst={13} timeUtc={14}",
				sample.SliceMs,
				sample.QueueCount,
				sample.ProcessedCount,
				sample.NamedMs,
				sample.OtherMs,
				sample.SpawnEntryMs,
				sample.CallbackCount,
				sample.SlowCount,
				sample.TopEntryTotal,
				sample.TopEntryMax,
				sample.TopRegionTotal,
				sample.TopTypeTotal,
				sample.TopOriginTotal,
				FormatSample(sample.Worst),
				sample.TimeUtc);
		}

		private static void WriteRecentSlow(TextWriter writer, Sample[] samples)
		{
			writer.WriteLine("SpawnEntry recent slow samples: count={0}", samples.Length);

			int count = Math.Min(SpawnEntryMaxRecentSamples, samples.Length);

			for (int i = 0; i < count; ++i)
				writer.WriteLine("  slow#{0}: {1}", i + 1, FormatSample(samples[i]));
		}

		private static void WriteBurst(TextWriter writer, int index, BurstSummary burst)
		{
			if (burst == null)
				return;

			long duration = burst.EndUtc >= burst.StartUtc ? (long)(burst.EndUtc - burst.StartUtc).TotalMilliseconds : 0;
			writer.WriteLine("  burst#{0}: runId={1} origin={2}/{3} timer={4} startup={5} schedules={6} callbacks={7} callbackMs={8} maxCallback={9}ms slow={10} duration={11}ms distinctEntries={12} distinctRegions={13} distinctDefinitions={14} distinctRemovedTypes={15} maxSliceMs={16} maxSpawnEntryMsInSlice={17} maxCallbacksInSlice={18}",
				index,
				burst.RunId,
				CleanOutput(burst.TaskName),
				CleanOutput(burst.PhaseName),
				CleanOutput(burst.TimerName),
				burst.Startup,
				burst.Schedules,
				burst.Callbacks,
				burst.CallbackMs,
				burst.MaxCallbackMs,
				burst.SlowCallbacks,
				duration,
				burst.Entries.Count,
				burst.Regions.Count,
				burst.Definitions.Count,
				burst.RemovedTargetTypes.Count,
				burst.MaxSliceMs,
				burst.MaxSpawnEntryMsInSlice,
				burst.MaxCallbacksInSlice);
			writer.WriteLine("    completion: <=1s={0} <=5s={1} <=10s={2} >30s={3}", burst.EntriesCompleteAfter1s, burst.EntriesCompleteAfter5s, burst.EntriesCompleteAfter10s, burst.EntriesIncompleteAfter30s);
			writer.WriteLine("    topDefinitionByCallbacks={0}", FormatDetail(GetTopDetail(burst.Definitions, "total")));
			writer.WriteLine("    topRemovedTargetTypeBySchedules={0}", FormatDetail(GetTopDetail(burst.RemovedTargetTypes, "count")));
			writer.WriteLine("    topRegionByCallbackTime={0}", FormatDetail(GetTopDetail(burst.Regions, "total")));
			writer.WriteLine("    topTargetTypeProcessed={0}", FormatDetail(GetTopDetail(burst.TargetTypes, "count")));
			writer.WriteLine("    topTargetActionProcessed={0}", FormatDetail(GetTopDetail(burst.TargetActions, "count")));
			writer.WriteLine("    exactWorstEntry={0}", !String.IsNullOrWhiteSpace(burst.ExactWorstEntry) ? burst.ExactWorstEntry : "none");
			WriteProjection(writer, "    ", burst.ActualProjection, null);
			WriteProjection(writer, "    ", burst.Spread5Projection, burst.ActualProjection);
			WriteProjection(writer, "    ", burst.Spread10Projection, burst.ActualProjection);
			WriteProjection(writer, "    ", burst.Spread15Projection, burst.ActualProjection);
		}

		private static void WriteProjection(TextWriter writer, string prefix, ProjectionStats projection, ProjectionStats actual)
		{
			long peak250 = GetPeak(projection.Bucket250);
			long peak1000 = GetPeak(projection.Bucket1000);
			long avg250 = GetAverageActiveBucket(projection.Bucket250);
			long avg1000 = GetAverageActiveBucket(projection.Bucket1000);
			long avgAdded = projection.Count > 0 ? projection.TotalAddedMs / projection.Count : 0;
			long actualPeak250 = actual != null ? GetPeak(actual.Bucket250) : peak250;
			long actualPeak1000 = actual != null ? GetPeak(actual.Bucket1000) : peak1000;
			long reduction250 = actualPeak250 > 0 ? ((actualPeak250 - peak250) * 100) / actualPeak250 : 0;
			long reduction1000 = actualPeak1000 > 0 ? ((actualPeak1000 - peak1000) * 100) / actualPeak1000 : 0;

			writer.WriteLine("{0}DryRun {1}: schedules={2} actualPeak250ms={3} projectedPeak250ms={4} actualPeak1s={5} projectedPeak1s={6} avg250msBucket={7} avg1sBucket={8} peak250ReductionPct={9} peak1sReductionPct={10} spreadDurationMs={11} avgAddedDelayMs={12} maxAddedDelayMs={13}",
				prefix,
				projection.Label,
				projection.Count,
				actualPeak250,
				peak250,
				actualPeak1000,
				peak1000,
				avg250,
				avg1000,
				reduction250,
				reduction1000,
				projection.SpreadMs,
				avgAdded,
				projection.MaxAddedMs);
		}

		private static string FormatDetail(DetailStat stat)
		{
			if (stat == null)
				return "none";

			long avg = stat.Count > 0 ? stat.TotalMs / stat.Count : 0;
			return String.Format("{0} total={1}ms count={2} slow={3} verySlow={4} max={5}ms avg={6}ms last={7}ms lastUtc={8}", stat.Description, stat.TotalMs, stat.Count, stat.SlowCount, stat.VerySlowCount, stat.MaxMs, avg, stat.LastMs, stat.LastUtc);
		}

		private static string FormatSample(Sample sample)
		{
			if (sample == null)
				return "none";

			return String.Format("{0}ms entry={1} region={2} entryType={3} definition={4} before={5} requested={6} spawnedThisCallback={7} spawned={8}/{9} running={10} spawning={11} callbackRescheduled={12} nextDelayMs={13} directOrigin={14} originChain={15} recentCorrelation={16} scheduleReason={17} removedTargetType={18} utc={19}",
				sample.ElapsedMs,
				sample.Entry,
				sample.Region,
				sample.EntryType,
				sample.Definition,
				sample.BeforeSpawnedCount,
				sample.RequestedAmount,
				sample.SpawnedThisCallback,
				sample.SpawnedCount,
				sample.MaxCount,
				sample.Running,
				sample.Spawning,
				sample.CallbackRescheduled,
				sample.NextDelayMs,
				sample.DirectOrigin,
				sample.OriginChain,
				sample.RecentCorrelation,
				sample.ScheduleReason,
				sample.RemovedTargetType,
				sample.TimeUtc);
		}

		private static string FormatChainBrief(OriginChain chain)
		{
			if (chain == null)
				return "none";

			return String.Format("{0} reason={1} removedType={2} schedules={3} callbacks={4} callbackMs={5} complete={6}",
				chain.OriginLabel,
				chain.ScheduleReason,
				chain.RemovedTargetType,
				chain.Schedules,
				chain.CallbackCount,
				chain.CallbackMs,
				chain.Completed);
		}

		private static string JoinLabels(string[] labels)
		{
			if (labels == null || labels.Length == 0)
				return "none";

			string value = labels[0];

			for (int i = 1; i < labels.Length; ++i)
				value += ", " + labels[i];

			return value;
		}

		private static bool HasUsefulRecentCorrelation(string[] labels)
		{
			if (labels == null)
				return false;

			for (int i = 0; i < labels.Length; ++i)
			{
				if (!String.IsNullOrWhiteSpace(labels[i]) && labels[i] != "none")
					return true;
			}

			return false;
		}

		private static void ClearCallbackDetails()
		{
			m_CurrentEntries.Clear();
			m_LastEntries.Clear();
			m_TotalEntries.Clear();
			m_CurrentRegions.Clear();
			m_LastRegions.Clear();
			m_TotalRegions.Clear();
			m_CurrentTypes.Clear();
			m_LastTypes.Clear();
			m_TotalTypes.Clear();
			m_CurrentDefinitions.Clear();
			m_LastDefinitions.Clear();
			m_TotalDefinitions.Clear();
		}

		private static void ClearScheduleDetails()
		{
			m_CurrentScheduleEntries.Clear();
			m_LastScheduleEntries.Clear();
			m_TotalScheduleEntries.Clear();
			m_CurrentScheduleRegions.Clear();
			m_LastScheduleRegions.Clear();
			m_TotalScheduleRegions.Clear();
			m_CurrentScheduleDefinitions.Clear();
			m_LastScheduleDefinitions.Clear();
			m_TotalScheduleDefinitions.Clear();
			m_CurrentScheduleReasons.Clear();
			m_LastScheduleReasons.Clear();
			m_TotalScheduleReasons.Clear();
			m_CurrentScheduleRemovedTypes.Clear();
			m_LastScheduleRemovedTypes.Clear();
			m_TotalScheduleRemovedTypes.Clear();
			m_CurrentScheduleDirectOrigins.Clear();
			m_LastScheduleDirectOrigins.Clear();
			m_TotalScheduleDirectOrigins.Clear();
		}

		private static int StableHash(string value)
		{
			unchecked
			{
				int hash = 23;

				if (value != null)
				{
					for (int i = 0; i < value.Length; ++i)
						hash = (hash * 31) + value[i];
				}

				return hash & 0x7FFFFFFF;
			}
		}

		private static ProjectionStats NewProjection(string label, int spreadMs)
		{
			ProjectionStats projection = new ProjectionStats();
			projection.Label = label;
			projection.SpreadMs = spreadMs;
			return projection;
		}

		private static void AddBucket(Dictionary<int, long> buckets, int bucket)
		{
			long value;

			if (!buckets.TryGetValue(bucket, out value))
				value = 0;

			buckets[bucket] = value + 1;
		}

		private static long GetPeak(Dictionary<int, long> buckets)
		{
			long peak = 0;

			foreach (long count in buckets.Values)
			{
				if (count > peak)
					peak = count;
			}

			return peak;
		}

		private static long GetAverageActiveBucket(Dictionary<int, long> buckets)
		{
			if (buckets.Count == 0)
				return 0;

			long total = 0;

			foreach (long count in buckets.Values)
				total += count;

			return total / buckets.Count;
		}

		private static string CleanKey(string value, string fallback)
		{
			if (String.IsNullOrWhiteSpace(value))
				return fallback;

			return value.Trim();
		}

		private static string CleanDescription(string value, string fallback)
		{
			if (String.IsNullOrWhiteSpace(value))
				return fallback;

			return value.Trim();
		}

		private static string CleanOutput(string value)
		{
			if (String.IsNullOrWhiteSpace(value))
				return "none";

			return value.Trim();
		}
	}
}
