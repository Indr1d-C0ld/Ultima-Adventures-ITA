//Engine r53
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Server;
using Server.Commands;
using Server.Diagnostics;
using Server.Items;
using Server.Misc;
using Server.Network;
using Server.Regions;
using CPA = Server.CommandPropertyAttribute;

namespace Server.Mobiles
{
	public class PremiumSpawner : Item
	{
		private int m_Team;
		private int m_HomeRange;          // = old SpawnRange
		private int m_WalkingRange = -1;  // = old HomeRange
		private int m_SpawnID = 1;
		private int m_Count;
		private int m_CountA;
		private int m_CountB;
		private int m_CountC;
                private int m_CountD;
                private int m_CountE;
                private TimeSpan m_MinDelay;
                private TimeSpan m_MaxDelay;
                private Direction m_SpawnDirection = Direction.South;
                private List<string> m_CreaturesName; // creatures to be spawned
                private List<IEntity> m_Creatures;    // spawned creatures
                private List<string> m_CreaturesNameA;
                private List<IEntity> m_CreaturesA;
		private List<string> m_CreaturesNameB;
		private List<IEntity> m_CreaturesB;
		private List<string> m_CreaturesNameC;
		private List<IEntity> m_CreaturesC;
		private List<string> m_CreaturesNameD;
		private List<IEntity> m_CreaturesD;
		private List<string> m_CreaturesNameE;
                private List<IEntity> m_CreaturesE;
                private DateTime m_End;
                private InternalTimer m_Timer;
		private bool m_Running;
                private bool m_Group;
                private WayPoint m_WayPoint;
                private const long SlowLogThreshold = 50;
                private const long DetailedSlowThreshold = 100;
                private const long ConstructorSlowThreshold = 75;
                private const long SlowLogInterval = 30000;
                private static long m_NextSlowLog;
                private static readonly bool EnablePremiumSpawnerScheduler = true;
                private static readonly bool EnablePremiumSpawnerBudgeting = true;
                private static readonly bool EnablePremiumSpawnerPromotionBudget = true;
                private static readonly bool EnablePremiumSpawnerAdaptiveProcessing = true;
                private static readonly bool EnablePremiumSpawnerAdaptivePromotion = true;
                private static readonly bool EnablePremiumSpawnerStartupCatchupJitter = true;
                private const int PremiumSpawnerSchedulerIntervalMs = 500;
                private const int PremiumSpawnerBaseProcessPerTick = 25;
                private const int PremiumSpawnerBacklogProcessPerTick1 = 50;
                private const int PremiumSpawnerBacklogProcessPerTick2 = 75;
                private const int PremiumSpawnerBacklogProcessPerTick3 = 100;
                private const int PremiumSpawnerBacklogReadyThreshold1 = 500;
                private const int PremiumSpawnerBacklogReadyThreshold2 = 1000;
                private const int PremiumSpawnerBacklogReadyThreshold3 = 1500;
                private const int PremiumSpawnerOldestReadyThresholdMs1 = 10000;
                private const int PremiumSpawnerOldestReadyThresholdMs2 = 20000;
                private const int PremiumSpawnerOldestReadyThresholdMs3 = 30000;
                private const int PremiumSpawnerBaseBudgetMsPerTick = 25;
                private const int PremiumSpawnerBacklogBudgetMsPerTick1 = 35;
                private const int PremiumSpawnerBacklogBudgetMsPerTick2 = 45;
                private const int PremiumSpawnerBacklogBudgetMsPerTick3 = 50;
                private const int PremiumSpawnerMaxPromotePerTickNormal = 100;
                private const int PremiumSpawnerMaxPromotePerTickWhenReadyBacklogged = 50;
                private const int PremiumSpawnerMaxPromotePerTickWhenReadySevere = 25;
                private const int PremiumSpawnerReadyBacklogPromotionThrottle = 1000;
                private const int PremiumSpawnerReadySevereBacklogPromotionThrottle = 1500;
                private const int PremiumSpawnerSchedulerSoftLagMs = 75;
                private const int PremiumSpawnerSchedulerHardLagMs = 100;
                private const int PremiumSpawnerTargetOldestReadyMs = 10000;
                private const int PremiumSpawnerWarningOldestReadyMs = 30000;
                private const int PremiumSpawnerTargetReadyBacklog = 500;
                private const int PremiumSpawnerMaxReadyQueueWarning = 500;
                private const int PremiumSpawnerMaxFutureQueueWarning = 15000;
                private const int PremiumSpawnerStartupCatchupWindowSeconds = 300;
                private const int PremiumSpawnerStartupCatchupMinDelaySeconds = 5;
                private const long StatsSummaryInterval = 60000;
                private const int MaxSpawnerStats = 4096;
                private const int MaxConstructorTypeStats = 128;
                private const int ConstructorTypeTopCount = 10;
                private static readonly object m_StatsLock = new object();
                private static Dictionary<int, SpawnerStat> m_CurrentStats = new Dictionary<int, SpawnerStat>();
                private static Dictionary<int, SpawnerStat> m_LastStats = new Dictionary<int, SpawnerStat>();
                private static Dictionary<string, ConstructorTypeStat> m_CurrentConstructorTypeStats = new Dictionary<string, ConstructorTypeStat>(StringComparer.Ordinal);
                private static Dictionary<string, ConstructorTypeStat> m_LastConstructorTypeStats = new Dictionary<string, ConstructorTypeStat>(StringComparer.Ordinal);
                private static Dictionary<string, ConstructorTypeStat> m_AllTimeConstructorTypeStats = new Dictionary<string, ConstructorTypeStat>(StringComparer.Ordinal);
                private static SortedDictionary<long, List<PremiumSpawner>> m_SchedulerFuture = new SortedDictionary<long, List<PremiumSpawner>>();
                private static Dictionary<int, long> m_SchedulerDueBySerial = new Dictionary<int, long>();
                private static Dictionary<int, PremiumSpawner> m_SchedulerSpawnerBySerial = new Dictionary<int, PremiumSpawner>();
                private static Queue<PremiumSpawner> m_SchedulerReady = new Queue<PremiumSpawner>();
                private static Dictionary<int, PremiumSpawner> m_SchedulerReadyQueued = new Dictionary<int, PremiumSpawner>();
                private static SpawnerStat m_WorstSpawnerSample = new SpawnerStat();
                private static SpawnerStat m_TopLateSpawner = new SpawnerStat();
                private static SpawnerStat m_TopZeroNextDelaySpawner = new SpawnerStat();
                private static SlowSpawnSample m_WorstSingleEntrySample = new SlowSpawnSample();
                private static SlowSpawnSample m_WorstSingleSpawnSample = new SlowSpawnSample();
                private static SlowSpawnSample m_WorstSlowNoOpSample = new SlowSpawnSample();
                private static SlowSpawnSample m_WorstConstructorSample = new SlowSpawnSample();
                private static SpawnerStat m_TopRepeatedSlowSpawner = new SpawnerStat();
                private static SchedulerWindowStats m_CurrentSchedulerStats = new SchedulerWindowStats();
                private static SchedulerWindowStats m_LastSchedulerStats = new SchedulerWindowStats();
                private static PremiumSpawnerSchedulerTimer m_SchedulerTimer;
                private static long m_NextStatsSummary;
                private static long m_NextSchedulerWarning;
                private static long m_SchedulerBudgetDeferredDueTotal;
                private static long m_SchedulerProcessedTotal;
                private static long m_SchedulerDuplicatePreventedTotal;
                private static long m_SchedulerInvalidRemovedTotal;
                private static long m_SchedulerTicksTotal;
                private static long m_SchedulerRescheduledTotal;
                private static long m_SchedulerOldInternalTimerCallbacksTotal;
                private static long m_SchedulerMaxReadyQueueTotal;
                private static long m_SchedulerMaxFutureCountTotal;
                private static long m_SchedulerPromotionBudgetHitTotal;
                private static long m_SchedulerBudgetDeferredEventsTotal;
                private static long m_SchedulerMaxUnpromotedDueTotal;
                private static long m_SchedulerMaxOldestReadyAgeMs;
                private static long m_SchedulerMaxOldestUnpromotedDueAgeMs;
                private static long m_StartupCatchupAssignedTotal;
                private static long m_StartupCatchupDelayTotalMs;
                private static long m_StartupCatchupMaxDelayAssignedMs;
                private static long m_SlowEntryYieldCountTotal;
                private static long m_SingleSlowEntryBudgetBreaksTotal;
                private static long m_MaxSingleEntryMsTotal;
                private static int m_MaxSingleEntrySerial;
                private static string m_MaxSingleEntryMap = "none";
                private static Point3D m_MaxSingleEntryLocation;
                private static int m_ProcessedBeforeSlowEntry;
                private static int m_ProcessedAfterSlowEntry;
                private static long m_LastSchedulerTickMsForAdaptive;
                private static long m_NextAdaptiveLagWarning;
                private static int m_LastReadyCountForDrainRate = -1;
                private static long m_LastReadyDrainRateTick;

                [ThreadStatic]
                private static SpawnCallTrace m_ThreadSpawnTrace;

                private class SpawnerStat
                {
                        public int Serial;
                        public string Map;
                        public Point3D Location;
                        public long Count;
                        public long SlowCount;
                        public long TotalMs;
                        public long MaxMs;
                        public long LastMs;
                        public long SpawnedTotal;
                        public long TotalDoTimerMs;
                        public long TotalRegionMs;
                        public long TotalDefragMs;
                        public long TotalSpawnMs;
                        public long MaxDoTimerMs;
                        public long MaxRegionMs;
                        public long MaxDefragMs;
                        public long MaxSpawnMs;
                        public long NearbyCheckedTotal;
                        public long DeferredCount;
                        public long ProcessedFromSchedulerCount;
                        public long DuplicateEnqueueAvoidedCount;
                        public long LatenessTotalMs;
                        public long LatenessCount;
                        public long MaxLatenessMs;
                        public long LastNextDelayMs;
                        public long ZeroNextDelayCount;
                        public long ZeroNextDelayRepeatedCount;
                        public int MaxNearbyChecked;
                        public int LastTracked;
                        public int LastTrackedBefore;
                        public int LastTrackedAfter;
                        public int LastTrackedDelta;
                        public int LastCategories;
                        public int LastNearbyChecked;
                        public int LastSpawned;
                        public long ConstructedTotal;
                        public long AcceptedIntoSpawnerListTotal;
                        public long MovedToWorldTotal;
                        public long DeletedAfterConstructionTotal;
                        public long RejectedByRegionTotal;
                        public long NoObjectConstructedTotal;
                        public bool LastDeferred;
                        public string LastSlowBreakdown;

                        public void CopyFrom(SpawnerStat other)
                        {
                                Serial = other.Serial;
                                Map = other.Map;
                                Location = other.Location;
                                Count = other.Count;
                                SlowCount = other.SlowCount;
                                TotalMs = other.TotalMs;
                                MaxMs = other.MaxMs;
                                LastMs = other.LastMs;
                                SpawnedTotal = other.SpawnedTotal;
                                TotalDoTimerMs = other.TotalDoTimerMs;
                                TotalRegionMs = other.TotalRegionMs;
                                TotalDefragMs = other.TotalDefragMs;
                                TotalSpawnMs = other.TotalSpawnMs;
                                MaxDoTimerMs = other.MaxDoTimerMs;
                                MaxRegionMs = other.MaxRegionMs;
                                MaxDefragMs = other.MaxDefragMs;
                                MaxSpawnMs = other.MaxSpawnMs;
                                NearbyCheckedTotal = other.NearbyCheckedTotal;
                                DeferredCount = other.DeferredCount;
                                ProcessedFromSchedulerCount = other.ProcessedFromSchedulerCount;
                                DuplicateEnqueueAvoidedCount = other.DuplicateEnqueueAvoidedCount;
                                LatenessTotalMs = other.LatenessTotalMs;
                                LatenessCount = other.LatenessCount;
                                MaxLatenessMs = other.MaxLatenessMs;
                                LastNextDelayMs = other.LastNextDelayMs;
                                ZeroNextDelayCount = other.ZeroNextDelayCount;
                                ZeroNextDelayRepeatedCount = other.ZeroNextDelayRepeatedCount;
                                MaxNearbyChecked = other.MaxNearbyChecked;
                                LastTracked = other.LastTracked;
                                LastTrackedBefore = other.LastTrackedBefore;
                                LastTrackedAfter = other.LastTrackedAfter;
                                LastTrackedDelta = other.LastTrackedDelta;
                                LastCategories = other.LastCategories;
                                LastNearbyChecked = other.LastNearbyChecked;
                                LastSpawned = other.LastSpawned;
                                ConstructedTotal = other.ConstructedTotal;
                                AcceptedIntoSpawnerListTotal = other.AcceptedIntoSpawnerListTotal;
                                MovedToWorldTotal = other.MovedToWorldTotal;
                                DeletedAfterConstructionTotal = other.DeletedAfterConstructionTotal;
                                RejectedByRegionTotal = other.RejectedByRegionTotal;
                                NoObjectConstructedTotal = other.NoObjectConstructedTotal;
                                LastDeferred = other.LastDeferred;
                                LastSlowBreakdown = other.LastSlowBreakdown;
                        }
                }

                private sealed class SpawnCallTrace
                {
                        public PremiumSpawner Owner;
                        public bool Active;
                        public string RegionName;
                        public string CurrentCategory;
                        public string SelectedType;
                        public string ConstructedType;
                        public string WorstConstructorType;
                        public string WorstCategory;
                        public string Error;
                        public int CategoriesInspected;
                        public int EntriesInspected;
                        public int CandidateTypesConsidered;
                        public int SpawnAttempts;
                        public int RetryAttempts;
                        public int LocationSearchCount;
                        public int ConstructedCount;
                        public int AcceptedIntoSpawnerListCount;
                        public int MovedToWorldCount;
                        public int DeletedAfterConstructionCount;
                        public int RejectedByRegionCount;
                        public int NoObjectConstructedCount;
                        public long ArgumentParseMs;
                        public long TypeLookupMs;
                        public long ReflectionSetupMs;
                        public long ConstructorMs;
                        public long ConstructorMaxMs;
                        public long SpawnPointSearchMs;
                        public long OnBeforeSpawnMs;
                        public long MoveToWorldMs;
                        public long SetupMs;
                        public long RegionValidationMs;
                        public long OnAfterSpawnMs;
                        public long InternalDefragMs;
                        public long CategoryMaxMs;
                        public long CurrentAttemptStart;
                        public int CurrentConstructorIndex;
                        public bool HasCitizenConstructorTrace;
                        public CitizenConstructorSample WorstCitizenConstructorTrace;
                        public List<ConstructorInvocationTrace> ConstructorInvocations;

                        public void Reset(PremiumSpawner owner)
                        {
                                Owner = owner;
                                Active = owner != null;
                                RegionName = "unknown";
                                CurrentCategory = "none";
                                SelectedType = "none";
                                ConstructedType = "none";
                                WorstConstructorType = "none";
                                WorstCategory = "none";
                                Error = "none";
                                CategoriesInspected = 0;
                                EntriesInspected = 0;
                                CandidateTypesConsidered = 0;
                                SpawnAttempts = 0;
                                RetryAttempts = 0;
                                LocationSearchCount = 0;
                                ConstructedCount = 0;
                                AcceptedIntoSpawnerListCount = 0;
                                MovedToWorldCount = 0;
                                DeletedAfterConstructionCount = 0;
                                RejectedByRegionCount = 0;
                                NoObjectConstructedCount = 0;
                                ArgumentParseMs = 0;
                                TypeLookupMs = 0;
                                ReflectionSetupMs = 0;
                                ConstructorMs = 0;
                                ConstructorMaxMs = 0;
                                SpawnPointSearchMs = 0;
                                OnBeforeSpawnMs = 0;
                                MoveToWorldMs = 0;
                                SetupMs = 0;
                                RegionValidationMs = 0;
                                OnAfterSpawnMs = 0;
                                InternalDefragMs = 0;
                                CategoryMaxMs = 0;
                                CurrentAttemptStart = 0;
                                CurrentConstructorIndex = -1;
                                HasCitizenConstructorTrace = false;
                                WorstCitizenConstructorTrace = new CitizenConstructorSample();

                                if (ConstructorInvocations == null)
                                        ConstructorInvocations = new List<ConstructorInvocationTrace>(16);
                                else
                                        ConstructorInvocations.Clear();
                        }

                        public void Clear()
                        {
                                Owner = null;
                                Active = false;
                        }
                }

                private struct ConstructorInvocationTrace
                {
                        public string TypeName;
                        public long ConstructorMs;
                        public long SpawnCallMs;
                        public bool Constructed;
                        public bool AcceptedIntoSpawnerList;
                        public bool MovedToWorld;
                        public bool DeletedAfterConstruction;
                        public bool RejectedByRegion;
                        public bool HasCitizenConstructorTrace;
                        public CitizenConstructorSample CitizenConstructorTrace;
                        public string Error;
                }

                private sealed class ConstructorTypeStat
                {
                        public string TypeName;
                        public long Count;
                        public long SlowCount;
                        public long ConstructorTotalMs;
                        public long ConstructorMaxMs;
                        public long SpawnCallTotalMs;
                        public long SpawnCallMaxMs;
                        public long ConstructedCount;
                        public long AcceptedIntoSpawnerListCount;
                        public long MovedToWorldCount;
                        public long DeletedAfterConstructionCount;
                        public long RejectedByRegionCount;
                        public long RetainedTrackedCount;
                        public long PositiveTrackedDeltaCallCount;
                        public long NoPositiveTrackedDeltaCount;
                        public long NoPositiveTrackedDeltaConstructorTotalMs;
                        public int SlowestSpawnerSerial;
                        public string SlowestSpawnerMap;
                        public Point3D SlowestSpawnerLocation;
                        public int LastSpawnerSerial;
                        public string LastSpawnerMap;
                        public Point3D LastSpawnerLocation;
                        public long CitizenTraceCount;
                        public long[] CitizenPhaseTotals = new long[CitizenConstructorDiagnostics.PhaseCount];
                        public long[] CitizenPhaseMax = new long[CitizenConstructorDiagnostics.PhaseCount];
                        public string WorstSample;

                        public void CopyFrom(ConstructorTypeStat source)
                        {
                                if (source == null)
                                        return;

                                TypeName = source.TypeName;
                                Count = source.Count;
                                SlowCount = source.SlowCount;
                                ConstructorTotalMs = source.ConstructorTotalMs;
                                ConstructorMaxMs = source.ConstructorMaxMs;
                                SpawnCallTotalMs = source.SpawnCallTotalMs;
                                SpawnCallMaxMs = source.SpawnCallMaxMs;
                                ConstructedCount = source.ConstructedCount;
                                AcceptedIntoSpawnerListCount = source.AcceptedIntoSpawnerListCount;
                                MovedToWorldCount = source.MovedToWorldCount;
                                DeletedAfterConstructionCount = source.DeletedAfterConstructionCount;
                                RejectedByRegionCount = source.RejectedByRegionCount;
                                RetainedTrackedCount = source.RetainedTrackedCount;
                                PositiveTrackedDeltaCallCount = source.PositiveTrackedDeltaCallCount;
                                NoPositiveTrackedDeltaCount = source.NoPositiveTrackedDeltaCount;
                                NoPositiveTrackedDeltaConstructorTotalMs = source.NoPositiveTrackedDeltaConstructorTotalMs;
                                SlowestSpawnerSerial = source.SlowestSpawnerSerial;
                                SlowestSpawnerMap = source.SlowestSpawnerMap;
                                SlowestSpawnerLocation = source.SlowestSpawnerLocation;
                                LastSpawnerSerial = source.LastSpawnerSerial;
                                LastSpawnerMap = source.LastSpawnerMap;
                                LastSpawnerLocation = source.LastSpawnerLocation;
                                CitizenTraceCount = source.CitizenTraceCount;
                                Array.Copy(source.CitizenPhaseTotals, CitizenPhaseTotals, CitizenConstructorDiagnostics.PhaseCount);
                                Array.Copy(source.CitizenPhaseMax, CitizenPhaseMax, CitizenConstructorDiagnostics.PhaseCount);
                                WorstSample = source.WorstSample;
                        }
                }

                private sealed class SlowSpawnSample
                {
                        public int Serial;
                        public string Map;
                        public Point3D Location;
                        public string RegionName;
                        public long TotalMs;
                        public long DoTimerMs;
                        public long RegionMs;
                        public long DefragMs;
                        public long SpawnMs;
                        public int ConfiguredCategories;
                        public int CategoriesInspected;
                        public int EntriesInspected;
                        public int CandidateTypesConsidered;
                        public string SelectedType;
                        public string ConstructedType;
                        public string WorstConstructorType;
                        public string WorstCategory;
                        public long ArgumentParseMs;
                        public long TypeLookupMs;
                        public long ReflectionSetupMs;
                        public long ConstructorMs;
                        public long ConstructorMaxMs;
                        public long SpawnPointSearchMs;
                        public int LocationSearchCount;
                        public long OnBeforeSpawnMs;
                        public long MoveToWorldMs;
                        public long SetupMs;
                        public long RegionValidationMs;
                        public long OnAfterSpawnMs;
                        public long InternalDefragMs;
                        public long CategoryMaxMs;
                        public int Attempts;
                        public int Retries;
                        public int Spawned;
                        public int TrackedBefore;
                        public int TrackedAfter;
                        public int TrackedDelta;
                        public int ConstructedCount;
                        public int AcceptedIntoSpawnerListCount;
                        public int MovedToWorldCount;
                        public int DeletedAfterConstructionCount;
                        public int RejectedByRegionCount;
                        public int NoObjectConstructedCount;
                        public int NearbyChecked;
                        public long NextDelayMs;
                        public string Error;

                        public void CopyFrom(PremiumSpawner spawner, SpawnCallTrace trace, long totalMs, long doTimerMs, long regionMs, long defragMs, long spawnMs, int categories, int spawned, int trackedBefore, int trackedAfter, int nearbyChecked, TimeSpan nextDelay)
                        {
                                Serial = spawner != null ? spawner.Serial.Value : 0;
                                Map = spawner != null && spawner.Map != null ? spawner.Map.ToString() : "null";
                                Location = spawner != null ? spawner.Location : new Point3D(0, 0, 0);
                                RegionName = trace != null ? trace.RegionName : "unknown";
                                TotalMs = totalMs;
                                DoTimerMs = doTimerMs;
                                RegionMs = regionMs;
                                DefragMs = defragMs;
                                SpawnMs = spawnMs;
                                ConfiguredCategories = categories;
                                CategoriesInspected = trace != null ? trace.CategoriesInspected : 0;
                                EntriesInspected = trace != null ? trace.EntriesInspected : 0;
                                CandidateTypesConsidered = trace != null ? trace.CandidateTypesConsidered : 0;
                                SelectedType = trace != null ? trace.SelectedType : "none";
                                ConstructedType = trace != null ? trace.ConstructedType : "none";
                                WorstConstructorType = trace != null ? trace.WorstConstructorType : "none";
                                WorstCategory = trace != null ? trace.WorstCategory : "none";
                                ArgumentParseMs = trace != null ? trace.ArgumentParseMs : 0;
                                TypeLookupMs = trace != null ? trace.TypeLookupMs : 0;
                                ReflectionSetupMs = trace != null ? trace.ReflectionSetupMs : 0;
                                ConstructorMs = trace != null ? trace.ConstructorMs : 0;
                                ConstructorMaxMs = trace != null ? trace.ConstructorMaxMs : 0;
                                SpawnPointSearchMs = trace != null ? trace.SpawnPointSearchMs : 0;
                                LocationSearchCount = trace != null ? trace.LocationSearchCount : 0;
                                OnBeforeSpawnMs = trace != null ? trace.OnBeforeSpawnMs : 0;
                                MoveToWorldMs = trace != null ? trace.MoveToWorldMs : 0;
                                SetupMs = trace != null ? trace.SetupMs : 0;
                                RegionValidationMs = trace != null ? trace.RegionValidationMs : 0;
                                OnAfterSpawnMs = trace != null ? trace.OnAfterSpawnMs : 0;
                                InternalDefragMs = trace != null ? trace.InternalDefragMs : 0;
                                CategoryMaxMs = trace != null ? trace.CategoryMaxMs : 0;
                                Attempts = trace != null ? trace.SpawnAttempts : 0;
                                Retries = trace != null ? trace.RetryAttempts : 0;
                                Spawned = spawned;
                                TrackedBefore = trackedBefore;
                                TrackedAfter = trackedAfter;
                                TrackedDelta = trackedAfter - trackedBefore;
                                ConstructedCount = trace != null ? trace.ConstructedCount : 0;
                                AcceptedIntoSpawnerListCount = trace != null ? trace.AcceptedIntoSpawnerListCount : 0;
                                MovedToWorldCount = trace != null ? trace.MovedToWorldCount : 0;
                                DeletedAfterConstructionCount = trace != null ? trace.DeletedAfterConstructionCount : 0;
                                RejectedByRegionCount = trace != null ? trace.RejectedByRegionCount : 0;
                                NoObjectConstructedCount = trace != null ? trace.NoObjectConstructedCount : 0;
                                NearbyChecked = nearbyChecked;
                                NextDelayMs = (long)nextDelay.TotalMilliseconds;
                                Error = trace != null ? trace.Error : "none";
                        }
                }

                private class SchedulerWindowStats
                {
                        public long SchedulerTicks;
                        public long SchedulerTickTotalMs;
                        public long SchedulerTickMaxMs;
                        public long SchedulerTickLastMs;
                        public long PromotedDue;
                        public long PromotedDueThisTick;
                        public long ProcessedDue;
                        public long ProcessedDueThisTick;
                        public long BudgetDeferredDue;
                        public long NewlyBudgetDeferredThisTick;
                        public long BudgetDeferredEvents;
                        public long BudgetDeferredSpawnerCount;
                        public long ReadyBacklogCarriedCurrent;
                        public long ReadyBacklogCarriedMax;
                        public long OldestReadyAgeMs;
                        public long OldestReadyMaxAgeMs;
                        public long UnpromotedDueCurrent;
                        public long UnpromotedDueMax;
                        public long OldestUnpromotedDueAgeMs;
                        public long MaxPromotionDelayMs;
                        public long PromotionBudgetHitCount;
                        public long PromotionBudgetHitTotal;
                        public long InvalidRemoved;
                        public long DuplicatePrevented;
                        public long Rescheduled;
                        public long OldInternalTimerCallbacks;
                        public long SpawnWorkElapsedMs;
                        public long SpawnWorkMaxMs;
                        public long LatenessTotalMs;
                        public long LatenessMaxMs;
                        public long LatenessSamples;
                        public long MaxReadyQueue;
                        public long MaxFutureCount;
                        public long SlowCalls;
                        public long ZeroNextDelayCount;
                        public long ZeroNextDelayProcessed;
                        public long ZeroNextDelayRescheduled;
                        public long ZeroNextDelayRepeatedCount;
                        public long StartupCatchupAssigned;
                        public long StartupCatchupDelayTotalMs;
                        public long StartupCatchupMaxDelayAssignedMs;
                        public long AdaptiveLevelLast;
                        public long AdaptiveLevelMax;
                        public long AdaptiveLevel0Count;
                        public long AdaptiveLevel1Count;
                        public long AdaptiveLevel2Count;
                        public long AdaptiveLevel3Count;
                        public long AdaptiveBudgetHitCount;
                        public long AdaptiveBudgetMsUsedLast;
                        public long AdaptiveProcessLimitLast;
                        public long AdaptiveBudgetLimitMsLast;
                        public long AdaptivePromotionLimitLast;
                        public long AdaptiveProcessingLimitedByMsBudget;
                        public long AdaptiveProcessingLimitedByCountBudget;
                        public long OldestReadyTotalMs;
                        public long OldestReadySamples;
                        public long OldestReadyTicksOverTarget;
                        public long ReadyBacklogOverTargetCurrent;
                        public long ReadyBacklogOverTargetMax;
                        public long BacklogDrainRatePerMinute;
                        public long SlowEntryYieldCount;
                        public long SingleSlowEntryBudgetBreaks;
                        public long MaxSingleEntryMs;
                        public int MaxSingleEntrySerial;
                        public string MaxSingleEntryMap;
                        public Point3D MaxSingleEntryLocation;
                        public long ProcessedBeforeSlowEntry;
                        public long ProcessedAfterSlowEntry;

                        public void Reset()
                        {
                                SchedulerTicks = 0;
                                SchedulerTickTotalMs = 0;
                                SchedulerTickMaxMs = 0;
                                SchedulerTickLastMs = 0;
                                PromotedDue = 0;
                                PromotedDueThisTick = 0;
                                ProcessedDue = 0;
                                ProcessedDueThisTick = 0;
                                BudgetDeferredDue = 0;
                                NewlyBudgetDeferredThisTick = 0;
                                BudgetDeferredEvents = 0;
                                BudgetDeferredSpawnerCount = 0;
                                ReadyBacklogCarriedCurrent = 0;
                                ReadyBacklogCarriedMax = 0;
                                OldestReadyAgeMs = 0;
                                OldestReadyMaxAgeMs = 0;
                                UnpromotedDueCurrent = 0;
                                UnpromotedDueMax = 0;
                                OldestUnpromotedDueAgeMs = 0;
                                MaxPromotionDelayMs = 0;
                                PromotionBudgetHitCount = 0;
                                PromotionBudgetHitTotal = 0;
                                InvalidRemoved = 0;
                                DuplicatePrevented = 0;
                                Rescheduled = 0;
                                OldInternalTimerCallbacks = 0;
                                SpawnWorkElapsedMs = 0;
                                SpawnWorkMaxMs = 0;
                                LatenessTotalMs = 0;
                                LatenessMaxMs = 0;
                                LatenessSamples = 0;
                                MaxReadyQueue = 0;
                                MaxFutureCount = 0;
                                SlowCalls = 0;
                                ZeroNextDelayCount = 0;
                                ZeroNextDelayProcessed = 0;
                                ZeroNextDelayRescheduled = 0;
                                ZeroNextDelayRepeatedCount = 0;
                                StartupCatchupAssigned = 0;
                                StartupCatchupDelayTotalMs = 0;
                                StartupCatchupMaxDelayAssignedMs = 0;
                                AdaptiveLevelLast = 0;
                                AdaptiveLevelMax = 0;
                                AdaptiveLevel0Count = 0;
                                AdaptiveLevel1Count = 0;
                                AdaptiveLevel2Count = 0;
                                AdaptiveLevel3Count = 0;
                                AdaptiveBudgetHitCount = 0;
                                AdaptiveBudgetMsUsedLast = 0;
                                AdaptiveProcessLimitLast = 0;
                                AdaptiveBudgetLimitMsLast = 0;
                                AdaptivePromotionLimitLast = 0;
                                AdaptiveProcessingLimitedByMsBudget = 0;
                                AdaptiveProcessingLimitedByCountBudget = 0;
                                OldestReadyTotalMs = 0;
                                OldestReadySamples = 0;
                                OldestReadyTicksOverTarget = 0;
                                ReadyBacklogOverTargetCurrent = 0;
                                ReadyBacklogOverTargetMax = 0;
                                BacklogDrainRatePerMinute = 0;
                                SlowEntryYieldCount = 0;
                                SingleSlowEntryBudgetBreaks = 0;
                                MaxSingleEntryMs = 0;
                                MaxSingleEntrySerial = 0;
                                MaxSingleEntryMap = "none";
                                MaxSingleEntryLocation = new Point3D(0, 0, 0);
                                ProcessedBeforeSlowEntry = 0;
                                ProcessedAfterSlowEntry = 0;
                        }

                        public void CopyFrom(SchedulerWindowStats other)
                        {
                                SchedulerTicks = other.SchedulerTicks;
                                SchedulerTickTotalMs = other.SchedulerTickTotalMs;
                                SchedulerTickMaxMs = other.SchedulerTickMaxMs;
                                SchedulerTickLastMs = other.SchedulerTickLastMs;
                                PromotedDue = other.PromotedDue;
                                PromotedDueThisTick = other.PromotedDueThisTick;
                                ProcessedDue = other.ProcessedDue;
                                ProcessedDueThisTick = other.ProcessedDueThisTick;
                                BudgetDeferredDue = other.BudgetDeferredDue;
                                NewlyBudgetDeferredThisTick = other.NewlyBudgetDeferredThisTick;
                                BudgetDeferredEvents = other.BudgetDeferredEvents;
                                BudgetDeferredSpawnerCount = other.BudgetDeferredSpawnerCount;
                                ReadyBacklogCarriedCurrent = other.ReadyBacklogCarriedCurrent;
                                ReadyBacklogCarriedMax = other.ReadyBacklogCarriedMax;
                                OldestReadyAgeMs = other.OldestReadyAgeMs;
                                OldestReadyMaxAgeMs = other.OldestReadyMaxAgeMs;
                                UnpromotedDueCurrent = other.UnpromotedDueCurrent;
                                UnpromotedDueMax = other.UnpromotedDueMax;
                                OldestUnpromotedDueAgeMs = other.OldestUnpromotedDueAgeMs;
                                MaxPromotionDelayMs = other.MaxPromotionDelayMs;
                                PromotionBudgetHitCount = other.PromotionBudgetHitCount;
                                PromotionBudgetHitTotal = other.PromotionBudgetHitTotal;
                                InvalidRemoved = other.InvalidRemoved;
                                DuplicatePrevented = other.DuplicatePrevented;
                                Rescheduled = other.Rescheduled;
                                OldInternalTimerCallbacks = other.OldInternalTimerCallbacks;
                                SpawnWorkElapsedMs = other.SpawnWorkElapsedMs;
                                SpawnWorkMaxMs = other.SpawnWorkMaxMs;
                                LatenessTotalMs = other.LatenessTotalMs;
                                LatenessMaxMs = other.LatenessMaxMs;
                                LatenessSamples = other.LatenessSamples;
                                MaxReadyQueue = other.MaxReadyQueue;
                                MaxFutureCount = other.MaxFutureCount;
                                SlowCalls = other.SlowCalls;
                                ZeroNextDelayCount = other.ZeroNextDelayCount;
                                ZeroNextDelayProcessed = other.ZeroNextDelayProcessed;
                                ZeroNextDelayRescheduled = other.ZeroNextDelayRescheduled;
                                ZeroNextDelayRepeatedCount = other.ZeroNextDelayRepeatedCount;
                                StartupCatchupAssigned = other.StartupCatchupAssigned;
                                StartupCatchupDelayTotalMs = other.StartupCatchupDelayTotalMs;
                                StartupCatchupMaxDelayAssignedMs = other.StartupCatchupMaxDelayAssignedMs;
                                AdaptiveLevelLast = other.AdaptiveLevelLast;
                                AdaptiveLevelMax = other.AdaptiveLevelMax;
                                AdaptiveLevel0Count = other.AdaptiveLevel0Count;
                                AdaptiveLevel1Count = other.AdaptiveLevel1Count;
                                AdaptiveLevel2Count = other.AdaptiveLevel2Count;
                                AdaptiveLevel3Count = other.AdaptiveLevel3Count;
                                AdaptiveBudgetHitCount = other.AdaptiveBudgetHitCount;
                                AdaptiveBudgetMsUsedLast = other.AdaptiveBudgetMsUsedLast;
                                AdaptiveProcessLimitLast = other.AdaptiveProcessLimitLast;
                                AdaptiveBudgetLimitMsLast = other.AdaptiveBudgetLimitMsLast;
                                AdaptivePromotionLimitLast = other.AdaptivePromotionLimitLast;
                                AdaptiveProcessingLimitedByMsBudget = other.AdaptiveProcessingLimitedByMsBudget;
                                AdaptiveProcessingLimitedByCountBudget = other.AdaptiveProcessingLimitedByCountBudget;
                                OldestReadyTotalMs = other.OldestReadyTotalMs;
                                OldestReadySamples = other.OldestReadySamples;
                                OldestReadyTicksOverTarget = other.OldestReadyTicksOverTarget;
                                ReadyBacklogOverTargetCurrent = other.ReadyBacklogOverTargetCurrent;
                                ReadyBacklogOverTargetMax = other.ReadyBacklogOverTargetMax;
                                BacklogDrainRatePerMinute = other.BacklogDrainRatePerMinute;
                                SlowEntryYieldCount = other.SlowEntryYieldCount;
                                SingleSlowEntryBudgetBreaks = other.SingleSlowEntryBudgetBreaks;
                                MaxSingleEntryMs = other.MaxSingleEntryMs;
                                MaxSingleEntrySerial = other.MaxSingleEntrySerial;
                                MaxSingleEntryMap = other.MaxSingleEntryMap;
                                MaxSingleEntryLocation = other.MaxSingleEntryLocation;
                                ProcessedBeforeSlowEntry = other.ProcessedBeforeSlowEntry;
                                ProcessedAfterSlowEntry = other.ProcessedAfterSlowEntry;
                        }
                }

                private static bool ShouldTreatAsControlled(BaseCreature creature)
                {
                        if (creature == null)
                                return true;

                        BaseChild child = creature as BaseChild;

                        if (child != null)
                        {
                                if (child.ControlMaster == null)
                                        return false;

                                if (child.childtarget != null)
                                        return false;
                        }

                        return true;
                }

		public bool IsFull{ get{ return ( m_Creatures != null && m_Creatures.Count >= m_Count ); } }
		public bool IsFulla{ get{ return ( m_CreaturesA != null && m_CreaturesA.Count >= m_CountA ); } }
		public bool IsFullb{ get{ return ( m_CreaturesB != null && m_CreaturesB.Count >= m_CountB ); } }
		public bool IsFullc{ get{ return ( m_CreaturesC != null && m_CreaturesC.Count >= m_CountC ); } }
		public bool IsFulld{ get{ return ( m_CreaturesD != null && m_CreaturesD.Count >= m_CountD ); } }
		public bool IsFulle{ get{ return ( m_CreaturesE != null && m_CreaturesE.Count >= m_CountE ); } }
		
		//public override bool HandlesOnMovement{ get{ return MyServerSettings.EnableAmbientSoundEffects(); } } //FINAL
		//private DateTime m_NextSound;	
		//public DateTime NextSound{ get{ return m_NextSound; } set{ m_NextSound = value; } }
		
		public List<string> CreaturesName
		{
			get { return m_CreaturesName; }
			set
			{
				m_CreaturesName = value;
				if ( m_CreaturesName.Count < 1 )
					Stop();

				InvalidateProperties();
			}
		}

		public List<string> SubSpawnerA
		{
			get { return m_CreaturesNameA; }
			set
			{
				m_CreaturesNameA = value;
				if ( m_CreaturesNameA.Count < 1 )
					Stop();

				InvalidateProperties();
			}
		}

		public List<string> SubSpawnerB
		{
			get { return m_CreaturesNameB; }
			set
			{
				m_CreaturesNameB = value;
				if ( m_CreaturesNameB.Count < 1 )
					Stop();

				InvalidateProperties();
			}
		}

		public List<string> SubSpawnerC
		{
			get { return m_CreaturesNameC; }
			set
			{
				m_CreaturesNameC = value;
				if ( m_CreaturesNameC.Count < 1 )
					Stop();

				InvalidateProperties();
			}
		}

		public List<string> SubSpawnerD
		{
			get { return m_CreaturesNameD; }
			set
			{
				m_CreaturesNameD = value;
				if ( m_CreaturesNameD.Count < 1 )
					Stop();

				InvalidateProperties();
			}
		}

		public List<string> SubSpawnerE
		{
			get { return m_CreaturesNameE; }
			set
			{
				m_CreaturesNameE = value;
				if ( m_CreaturesNameE.Count < 1 )
					Stop();

				InvalidateProperties();
			}
		}

		public virtual int CreaturesNameCount { get { return m_CreaturesName.Count; } }
		public virtual int CreaturesNameCountA { get { return m_CreaturesNameA.Count; } }
		public virtual int CreaturesNameCountB { get { return m_CreaturesNameB.Count; } }
		public virtual int CreaturesNameCountC { get { return m_CreaturesNameC.Count; } }
		public virtual int CreaturesNameCountD { get { return m_CreaturesNameD.Count; } }
		public virtual int CreaturesNameCountE { get { return m_CreaturesNameE.Count; } }

		public override void OnAfterDuped( Item newItem )
		{
			PremiumSpawner s = newItem as PremiumSpawner;

			if ( s == null )
				return;

			s.m_CreaturesName = new List<string>( m_CreaturesName );
			s.m_CreaturesNameA = new List<string>( m_CreaturesNameA );
			s.m_CreaturesNameB = new List<string>( m_CreaturesNameB );
			s.m_CreaturesNameC = new List<string>( m_CreaturesNameC );
			s.m_CreaturesNameD = new List<string>( m_CreaturesNameD );
			s.m_CreaturesNameE = new List<string>( m_CreaturesNameE );
		}
		
		[CommandProperty( AccessLevel.GameMaster )]
		public int Count
		{
			get { return m_Count; }
			set { m_Count = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int CountA
		{
			get { return m_CountA; }
			set { m_CountA = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int CountB
		{
			get { return m_CountB; }
			set { m_CountB = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
                public int CountC
                {
                        get { return m_CountC; }
                        set { m_CountC = value; InvalidateProperties(); }
                }

		[CommandProperty( AccessLevel.GameMaster )]
		public int CountD
		{
			get { return m_CountD; }
			set { m_CountD = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int CountE
		{
			get { return m_CountE; }
			set { m_CountE = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
                public WayPoint WayPoint
                {
                        get
                        {
                                return m_WayPoint;
                        }
                        set
                        {
                                m_WayPoint = value;
                        }
                }

                [CommandProperty( AccessLevel.GameMaster )]
                public Direction SpawnDirection
                {
                        get
                        {
                                return m_SpawnDirection;
                        }
                        set
                        {
                                m_SpawnDirection = value;
                        }
                }

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Running
		{
			get { return m_Running; }
			set
			{
				if ( value )
					Start();
				else
					Stop();

				InvalidateProperties();
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int HomeRange
		{
			get { return m_HomeRange; }
			set { m_HomeRange = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )] 
		public int WalkingRange 
		{ 
		   get { return m_WalkingRange; } 
		   set { m_WalkingRange = value; InvalidateProperties(); } 
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int SpawnID
		{
			get { return m_SpawnID; }
			set { m_SpawnID = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Team
		{
			get { return m_Team; }
			set { m_Team = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan MinDelay
		{
			get { return m_MinDelay; }
			set { m_MinDelay = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan MaxDelay
		{
			get { return m_MaxDelay; }
			set { m_MaxDelay = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan NextSpawn
		{
			get
			{
				if ( m_Running )
					return m_End - DateTime.UtcNow;
				else
					return TimeSpan.FromSeconds( 0 );
			}
			set
			{
				Start();
				DoTimer( value );
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Group
		{
			get { return m_Group; }
			set { m_Group = value; InvalidateProperties(); }
		}

		[Constructable]
		public PremiumSpawner( int amount, int subamountA, int subamountB, int subamountC, int subamountD, int subamountE, int spawnid, int minDelay, int maxDelay, int team, int homeRange, int walkingRange, string creatureName, string creatureNameA, string creatureNameB, string creatureNameC, string creatureNameD, string creatureNameE ) : base( 0x1f13 )
		{
			List<string> creaturesName = new List<string>();
			creaturesName.Add( creatureName );

			List<string> creatureNameAA = new List<string>();
			creaturesName.Add( creatureNameA );

			List<string> creatureNameBB = new List<string>();
			creaturesName.Add( creatureNameB );

			List<string> creatureNameCC = new List<string>();
			creaturesName.Add( creatureNameC );

			List<string> creatureNameDD = new List<string>();
			creaturesName.Add( creatureNameD );

			List<string> creatureNameEE = new List<string>();
			creaturesName.Add( creatureNameE );

			InitSpawn( amount, subamountA, subamountB, subamountC, subamountD, subamountE, spawnid, TimeSpan.FromMinutes( minDelay ), TimeSpan.FromMinutes( maxDelay ), team, homeRange, walkingRange, creaturesName, creatureNameAA, creatureNameBB, creatureNameCC, creatureNameDD, creatureNameEE );
		}

		[Constructable]
		public PremiumSpawner( string creatureName ) : base( 0x1f13 )
		{
			List<string> creaturesName = new List<string>();
			creaturesName.Add( creatureName );

			List<string> creatureNameAA = new List<string>();
			List<string> creatureNameBB = new List<string>();
			List<string> creatureNameCC = new List<string>();
			List<string> creatureNameDD = new List<string>();
			List<string> creatureNameEE = new List<string>();

			InitSpawn( 1, 0, 0, 0, 0, 0, 1, TimeSpan.FromMinutes( 5 ), TimeSpan.FromMinutes( 10 ), 0, 4, -1, creaturesName, creatureNameAA, creatureNameBB, creatureNameCC, creatureNameDD, creatureNameEE );
		}

		[Constructable]
		public PremiumSpawner() : base( 0x1f13 )
		{
			List<string> creaturesName = new List<string>();

			List<string> creatureNameAA = new List<string>();
			List<string> creatureNameBB = new List<string>();
			List<string> creatureNameCC = new List<string>();
			List<string> creatureNameDD = new List<string>();
			List<string> creatureNameEE = new List<string>();

			InitSpawn( 1, 0, 0, 0, 0, 0, 1, TimeSpan.FromMinutes( 5 ), TimeSpan.FromMinutes( 10 ), 0, 4, -1, creaturesName, creatureNameAA, creatureNameBB, creatureNameCC, creatureNameDD, creatureNameEE );
		}

		public PremiumSpawner( int amount, int subamountA, int subamountB, int subamountC, int subamountD, int subamountE, int spawnid, TimeSpan minDelay, TimeSpan maxDelay, int team, int homeRange, int walkingRange, List<string> creaturesName, List<string> creatureNameAA, List<string> creatureNameBB, List<string> creatureNameCC, List<string> creatureNameDD, List<string> creatureNameEE )
			: base( 0x1f13 )
		{
			InitSpawn( amount, subamountA, subamountB, subamountC, subamountD, subamountE, spawnid, minDelay, maxDelay, team, homeRange, walkingRange, creaturesName, creatureNameAA, creatureNameBB, creatureNameCC, creatureNameDD, creatureNameEE );
		}

		public override string DefaultName
		{
			get { return "PremiumSpawner"; }
		}

		public void InitSpawn( int amount, int subamountA, int subamountB, int subamountC, int subamountD, int subamountE, int SpawnID, TimeSpan minDelay, TimeSpan maxDelay, int team, int homeRange, int walkingRange, List<string> creaturesName, List<string> creatureNameAA, List<string> creatureNameBB, List<string> creatureNameCC, List<string> creatureNameDD, List<string> creatureNameEE )
		{
			Name = "PremiumSpawner";
			m_SpawnID = SpawnID;
			Visible = false;
			Movable = false;
			m_Running = true;
			m_Group = false;
			m_MinDelay = minDelay;
			m_MaxDelay = maxDelay;
			m_Count = amount;
			m_CountA = subamountA;
			m_CountB = subamountB;
			m_CountC = subamountC;
			m_CountD = subamountD;
			m_CountE = subamountE;
			m_Team = team;
			m_HomeRange = homeRange;
			m_WalkingRange = walkingRange;
			m_CreaturesName = creaturesName;
			m_CreaturesNameA = creatureNameAA;
			m_CreaturesNameB = creatureNameBB;
			m_CreaturesNameC = creatureNameCC;
			m_CreaturesNameD = creatureNameDD;
			m_CreaturesNameE = creatureNameEE;
			m_Creatures = new List<IEntity>();
			m_CreaturesA = new List<IEntity>();
			m_CreaturesB = new List<IEntity>();
			m_CreaturesC = new List<IEntity>();
			m_CreaturesD = new List<IEntity>();
			m_CreaturesE = new List<IEntity>();
			DoTimer( TimeSpan.FromSeconds( 1 ) );
		}
			
		public PremiumSpawner( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.AccessLevel < AccessLevel.GameMaster )
				return;

			PremiumSpawnerGump g = new PremiumSpawnerGump( this );
			from.SendGump( g );
		}



		//public override void OnMovement( Mobile m, Point3D oldLocation )
		//{
			/*
			if( m is PlayerMobile && MyServerSettings.EnableDungeonSoundEffects() )
			{
				if ( DateTime.UtcNow >= m_NextSound && Utility.InRange( m.Location, this.Location, 10 ) )
				{
					if ( Utility.RandomDouble() > 0.80 )
					{
						int sound = HiddenChest.DungeonSounds( this );	
						m.PlaySound( sound );	
					}
					m_NextSound = (DateTime.UtcNow + TimeSpan.FromSeconds( 60 ));	
				}
			}
			
				else if ( Server.Misc.Worlds.IsFireDungeon( this.Location, this.Map ) ){			category = "fire"; }
				else if ( Server.Misc.Worlds.IsIceDungeon( this.Location, this.Map ) ){				category = "snow"; }
				else if ( Server.Misc.Worlds.IsSeaDungeon( this.Location, this.Map ) ){				category = "sea"; }
				else if ( Server.Misc.Worlds.TestTile ( this.Map, this.X, this.Y, "dirt" ) 
						&& Server.Misc.Worlds.TestMountain ( this.Map, this.X, this.Y, 15 ) ){		category = "mountain"; }
				else if ( Server.Misc.Worlds.TestMountain ( this.Map, this.X, this.Y, 10 ) ){ 		category = "mountain"; }
				else if ( Server.Misc.Worlds.TestOcean ( this.Map, this.X, this.Y, 15 ) ){ 			category = "sea"; }
				else if ( Server.Misc.Worlds.TestTile ( this.Map, this.X, this.Y, "snow" ) ){		category = "snow"; }
				else if ( Server.Misc.Worlds.TestTile ( this.Map, this.X, this.Y, "cave" ) ){		category = "dungeon"; if ( Utility.RandomBool() ){ category = "mountain"; } }
				else if ( Server.Misc.Worlds.TestTile ( this.Map, this.X, this.Y, "swamp" ) ){		category = "swamp"; }
				else if ( Server.Misc.Worlds.TestTile ( this.Map, this.X, this.Y, "jungle" ) ){		category = "jungle"; }
				else if ( Server.Misc.Worlds.TestTile ( this.Map, this.X, this.Y, "forest" ) ){		category = "forest"; }
				else if ( Server.Misc.Worlds.TestTile ( this.Map, this.X, this.Y, "sand" ) ){		category = "sand"; }
				else if ( Server.Misc.Worlds.TestMountain ( this.Map, this.X, this.Y, 15 ) ){ 		category = "mountain"; }
				else if ( reg.IsPartOf( typeof( DungeonRegion ) ) ){								category = "dungeon"; }
			*/
		//}


		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( m_Running )
			{
				list.Add( 1060742 ); // active

				list.Add( 1060656, m_Count.ToString() );
				list.Add( 1061169, m_HomeRange.ToString() );
				list.Add( 1060658, "walking range\t{0}", m_WalkingRange );

				list.Add( 1060663, "SpawnID\t{0}", m_SpawnID.ToString() );

//				list.Add( 1060659, "group\t{0}", m_Group );
//				list.Add( 1060660, "team\t{0}", m_Team );
				list.Add( 1060661, "speed\t{0} to {1}", m_MinDelay, m_MaxDelay );

				for ( int i = 0; i < 2 && i < m_CreaturesName.Count; ++i )
					list.Add( 1060662 + i, "{0}\t{1}", m_CreaturesName[i], CountCreatures( m_CreaturesName[i] ) );
			}
			else
			{
				list.Add( 1060743 ); // inactive
			}
		}

		public override void OnSingleClick( Mobile from )
		{
			base.OnSingleClick( from );

			if ( m_Running )
				LabelTo( from, "[Running]" );
			else
				LabelTo( from, "[Off]" );
		}

		public void Start()
		{
			if ( !m_Running )
			{
				if ( m_CreaturesName.Count > 0 || m_CreaturesNameA.Count > 0 || m_CreaturesNameB.Count > 0 || m_CreaturesNameC.Count > 0 || m_CreaturesNameD.Count > 0 || m_CreaturesNameE.Count > 0 )
				{
					m_Running = true;
					DoTimer();
				}
			}
		}

		public void Stop()
		{
			if ( m_Running )
			{
				UnschedulePremiumSpawner( this );

				if ( m_Timer != null )
				{
					m_Timer.Stop();
					m_Timer = null;
				}

				m_Running = false;
			}
		}

		public static string ParseType( string s )
		{
			return s.Split( null, 2 )[0];
		}

		public void Defrag()
		{
			bool removed = false;

			for ( int i = 0; i < m_Creatures.Count; ++i )
			{
				IEntity e = m_Creatures[i];

				if ( e is Item )
				{
					Item item = (Item)e;

					if ( item.Deleted || item.Parent != null )
					{
						m_Creatures.RemoveAt( i );
						--i;
						removed = true;
					}
				}
				else if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					if ( m.Deleted )
					{
						m_Creatures.RemoveAt( i );
						--i;
						removed = true;
					}
					else if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
                                                if ( (bc.Controlled && ShouldTreatAsControlled( bc )) || bc.IsStabled )
						{
							m_Creatures.RemoveAt( i );
							--i;
							removed = true;
						}
					}
				}
				else
				{
					m_Creatures.RemoveAt( i );
					--i;
					removed = true;
				}
			}

			for ( int i = 0; i < m_CreaturesA.Count; ++i )
			{
				IEntity e = m_CreaturesA[i];

				if ( e is Item )
				{
					Item item = (Item)e;

					if ( item.Deleted || item.Parent != null )
					{
						m_CreaturesA.RemoveAt( i );
						--i;
						removed = true;
					}
				}
				else if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					if ( m.Deleted )
					{
						m_CreaturesA.RemoveAt( i );
						--i;
						removed = true;
					}
					else if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
                                                if ( (bc.Controlled && ShouldTreatAsControlled( bc )) || bc.IsStabled )
						{
							m_CreaturesA.RemoveAt( i );
							--i;
							removed = true;
						}
					}
				}
				else
				{
					m_CreaturesA.RemoveAt( i );
					--i;
					removed = true;
				}
			}

			for ( int i = 0; i < m_CreaturesB.Count; ++i )
			{
				IEntity e = m_CreaturesB[i];

				if ( e is Item )
				{
					Item item = (Item)e;

					if ( item.Deleted || item.Parent != null )
					{
						m_CreaturesB.RemoveAt( i );
						--i;
						removed = true;
					}
				}
				else if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					if ( m.Deleted )
					{
						m_CreaturesB.RemoveAt( i );
						--i;
						removed = true;
					}
					else if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
                                                if ( (bc.Controlled && ShouldTreatAsControlled( bc )) || bc.IsStabled )
						{
							m_CreaturesB.RemoveAt( i );
							--i;
							removed = true;
						}
					}
				}
				else
				{
					m_CreaturesB.RemoveAt( i );
					--i;
					removed = true;
				}
			}

			for ( int i = 0; i < m_CreaturesC.Count; ++i )
			{
				IEntity e = m_CreaturesC[i];

				if ( e is Item )
				{
					Item item = (Item)e;

					if ( item.Deleted || item.Parent != null )
					{
						m_CreaturesC.RemoveAt( i );
						--i;
						removed = true;
					}
				}
				else if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					if ( m.Deleted )
					{
						m_CreaturesC.RemoveAt( i );
						--i;
						removed = true;
					}
					else if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
                                                if ( (bc.Controlled && ShouldTreatAsControlled( bc )) || bc.IsStabled )
						{
							m_CreaturesC.RemoveAt( i );
							--i;
							removed = true;
						}
					}
				}
				else
				{
					m_CreaturesC.RemoveAt( i );
					--i;
					removed = true;
				}
			}

			for ( int i = 0; i < m_CreaturesD.Count; ++i )
			{
				IEntity e = m_CreaturesD[i];

				if ( e is Item )
				{
					Item item = (Item)e;

					if ( item.Deleted || item.Parent != null )
					{
						m_CreaturesD.RemoveAt( i );
						--i;
						removed = true;
					}
				}
				else if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					if ( m.Deleted )
					{
						m_CreaturesD.RemoveAt( i );
						--i;
						removed = true;
					}
					else if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
                                                if ( (bc.Controlled && ShouldTreatAsControlled( bc )) || bc.IsStabled )
						{
							m_CreaturesD.RemoveAt( i );
							--i;
							removed = true;
						}
					}
				}
				else
				{
					m_CreaturesD.RemoveAt( i );
					--i;
					removed = true;
				}
			}

			for ( int i = 0; i < m_CreaturesE.Count; ++i )
			{
				IEntity e = m_CreaturesE[i];

				if ( e is Item )
				{
					Item item = (Item)e;

					if ( item.Deleted || item.Parent != null )
					{
						m_CreaturesE.RemoveAt( i );
						--i;
						removed = true;
					}
				}
				else if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					if ( m.Deleted )
					{
						m_CreaturesE.RemoveAt( i );
						--i;
						removed = true;
					}
					else if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
                                                if ( (bc.Controlled && ShouldTreatAsControlled( bc )) || bc.IsStabled )
						{
							m_CreaturesE.RemoveAt( i );
							--i;
							removed = true;
						}
					}
				}
				else
				{
					m_CreaturesE.RemoveAt( i );
					--i;
					removed = true;
				}
			}

			if ( removed )
				InvalidateProperties();
		}

		private static long GetElapsedMilliseconds(long start)
		{
			return (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
		}

		private int GetTrackedCreatureCount()
		{
			int count = 0;

			if (m_Creatures != null)
				count += m_Creatures.Count;

			if (m_CreaturesA != null)
				count += m_CreaturesA.Count;

			if (m_CreaturesB != null)
				count += m_CreaturesB.Count;

			if (m_CreaturesC != null)
				count += m_CreaturesC.Count;

			if (m_CreaturesD != null)
				count += m_CreaturesD.Count;

			if (m_CreaturesE != null)
				count += m_CreaturesE.Count;

			return count;
		}

		private int GetConfiguredCategoryCount()
		{
			int count = 0;

			if (m_CreaturesName != null && m_CreaturesName.Count > 0)
				++count;

			if (m_CreaturesNameA != null && m_CreaturesNameA.Count > 0)
				++count;

			if (m_CreaturesNameB != null && m_CreaturesNameB.Count > 0)
				++count;

			if (m_CreaturesNameC != null && m_CreaturesNameC.Count > 0)
				++count;

			if (m_CreaturesNameD != null && m_CreaturesNameD.Count > 0)
				++count;

			if (m_CreaturesNameE != null && m_CreaturesNameE.Count > 0)
				++count;

			return count;
		}

                private SpawnCallTrace BeginSpawnTrace()
                {
                        SpawnCallTrace trace = m_ThreadSpawnTrace;

                        if (trace == null)
                        {
                                trace = new SpawnCallTrace();
                                m_ThreadSpawnTrace = trace;
                        }
                        else if (trace.Active)
                        {
                                return null;
                        }

                        trace.Reset(this);
                        return trace;
                }

                private static SpawnCallTrace GetSpawnTrace()
                {
                        SpawnCallTrace trace = m_ThreadSpawnTrace;
                        return trace != null && trace.Active ? trace : null;
                }

                private SpawnCallTrace GetOwnedSpawnTrace()
                {
                        SpawnCallTrace trace = GetSpawnTrace();
                        return trace != null && trace.Owner == this ? trace : null;
                }

                private static void EndSpawnTrace(SpawnCallTrace trace)
                {
                        if (trace != null)
                                trace.Clear();
                }

                private long BeginCategoryTrace(string category)
                {
                        SpawnCallTrace trace = GetOwnedSpawnTrace();

                        if (trace == null)
                                return 0;

                        trace.CurrentCategory = category;
                        ++trace.CategoriesInspected;
                        ++trace.EntriesInspected;
                        return Stopwatch.GetTimestamp();
                }

                private void EndCategoryTrace(string category, long started)
                {
                        if (started == 0)
                                return;

                        SpawnCallTrace trace = GetOwnedSpawnTrace();

                        if (trace == null)
                                return;

                        long elapsed = GetElapsedMilliseconds(started);

                        if (elapsed > trace.CategoryMaxMs)
                        {
                                trace.CategoryMaxMs = elapsed;
                                trace.WorstCategory = category;
                        }
                }

                private void TraceSpawnAttempt()
                {
                        SpawnCallTrace trace = GetOwnedSpawnTrace();

                        if (trace == null)
                                return;

                        ++trace.SpawnAttempts;

                        if (trace.SpawnAttempts > trace.EntriesInspected)
                                ++trace.RetryAttempts;

                        FinishCurrentSpawnAttempt(trace);
                        trace.CurrentAttemptStart = Stopwatch.GetTimestamp();
                        trace.CurrentConstructorIndex = -1;
                }

                private static void TraceConstructorInvocation(Type type, long constructorElapsed, bool constructed, CitizenConstructorSample citizenTrace)
                {
                        SpawnCallTrace trace = GetSpawnTrace();

                        if (trace == null)
                                return;

                        ConstructorInvocationTrace invocation = new ConstructorInvocationTrace();
                        invocation.TypeName = type != null ? type.FullName : "unknown";
                        invocation.ConstructorMs = constructorElapsed;
                        invocation.Constructed = constructed;
                        invocation.HasCitizenConstructorTrace = !String.IsNullOrEmpty(citizenTrace.TypeName);
                        invocation.CitizenConstructorTrace = citizenTrace;
                        invocation.Error = constructed ? "none" : trace.Error;
                        trace.ConstructorInvocations.Add(invocation);
                        trace.CurrentConstructorIndex = trace.ConstructorInvocations.Count - 1;

                        if (invocation.HasCitizenConstructorTrace
                                && (!trace.HasCitizenConstructorTrace || citizenTrace.TotalMs > trace.WorstCitizenConstructorTrace.TotalMs))
                        {
                                trace.HasCitizenConstructorTrace = true;
                                trace.WorstCitizenConstructorTrace = citizenTrace;
                        }

                        if (constructed)
                                ++trace.ConstructedCount;
                }

                private void TraceAcceptedIntoSpawnerList()
                {
                        SpawnCallTrace trace = GetOwnedSpawnTrace();

                        if (trace == null || trace.CurrentConstructorIndex < 0 || trace.CurrentConstructorIndex >= trace.ConstructorInvocations.Count)
                                return;

                        ConstructorInvocationTrace invocation = trace.ConstructorInvocations[trace.CurrentConstructorIndex];

                        if (!invocation.AcceptedIntoSpawnerList)
                        {
                                invocation.AcceptedIntoSpawnerList = true;
                                ++trace.AcceptedIntoSpawnerListCount;
                                trace.ConstructorInvocations[trace.CurrentConstructorIndex] = invocation;
                        }
                }

                private static void TraceMovedToWorld(SpawnCallTrace trace)
                {
                        if (trace == null || trace.CurrentConstructorIndex < 0 || trace.CurrentConstructorIndex >= trace.ConstructorInvocations.Count)
                                return;

                        ConstructorInvocationTrace invocation = trace.ConstructorInvocations[trace.CurrentConstructorIndex];

                        if (!invocation.MovedToWorld)
                        {
                                invocation.MovedToWorld = true;
                                ++trace.MovedToWorldCount;
                                trace.ConstructorInvocations[trace.CurrentConstructorIndex] = invocation;
                        }
                }

                private void DeleteRejectedByRegionWithTrace(IEntity entity)
                {
                        SpawnCallTrace trace = GetOwnedSpawnTrace();

                        if (entity != null)
                                entity.Delete();

                        TraceDeletedAfterConstruction(trace, true);
                        FinishCurrentSpawnAttempt(trace);
                }

                private static void TraceDeletedAfterConstruction(SpawnCallTrace trace, bool rejectedByRegion)
                {
                        if (trace == null || trace.CurrentConstructorIndex < 0 || trace.CurrentConstructorIndex >= trace.ConstructorInvocations.Count)
                                return;

                        ConstructorInvocationTrace invocation = trace.ConstructorInvocations[trace.CurrentConstructorIndex];

                        if (!invocation.DeletedAfterConstruction)
                        {
                                invocation.DeletedAfterConstruction = true;
                                ++trace.DeletedAfterConstructionCount;
                        }

                        if (rejectedByRegion && !invocation.RejectedByRegion)
                        {
                                invocation.RejectedByRegion = true;
                                ++trace.RejectedByRegionCount;
                        }

                        trace.ConstructorInvocations[trace.CurrentConstructorIndex] = invocation;
                }

                private void TraceUnacceptedOrNoObject()
                {
                        SpawnCallTrace trace = GetOwnedSpawnTrace();

                        if (trace == null)
                                return;

                        if (trace.CurrentConstructorIndex < 0
                                || trace.CurrentConstructorIndex >= trace.ConstructorInvocations.Count
                                || !trace.ConstructorInvocations[trace.CurrentConstructorIndex].Constructed)
                                ++trace.NoObjectConstructedCount;

                        FinishCurrentSpawnAttempt(trace);
                }

                private static void FinishCurrentSpawnAttempt(SpawnCallTrace trace)
                {
                        if (trace == null || trace.CurrentAttemptStart == 0)
                                return;

                        if (trace.CurrentConstructorIndex >= 0 && trace.CurrentConstructorIndex < trace.ConstructorInvocations.Count)
                        {
                                ConstructorInvocationTrace invocation = trace.ConstructorInvocations[trace.CurrentConstructorIndex];
                                invocation.SpawnCallMs = GetElapsedMilliseconds(trace.CurrentAttemptStart);
                                invocation.Error = trace.Error;
                                trace.ConstructorInvocations[trace.CurrentConstructorIndex] = invocation;
                        }

                        trace.CurrentAttemptStart = 0;
                        trace.CurrentConstructorIndex = -1;
                }

                private void DefragWithTrace()
                {
                        SpawnCallTrace trace = GetOwnedSpawnTrace();

                        if (trace == null)
                        {
                                Defrag();
                                return;
                        }

                        long started = Stopwatch.GetTimestamp();
                        Defrag();
                        trace.InternalDefragMs += GetElapsedMilliseconds(started);
                }

                private Point3D GetSpawnPositionWithTrace()
                {
                        SpawnCallTrace trace = GetOwnedSpawnTrace();

                        if (trace == null)
                                return GetSpawnPosition();

                        long started = Stopwatch.GetTimestamp();
                        Point3D location = GetSpawnPosition();
                        trace.SpawnPointSearchMs += GetElapsedMilliseconds(started);
                        ++trace.LocationSearchCount;
                        return location;
                }

                private Point3D GetSpawnSpotLandDungeonWithTrace()
                {
                        SpawnCallTrace trace = GetOwnedSpawnTrace();

                        if (trace == null)
                                return GetSpawnSpotLandDungeon();

                        long started = Stopwatch.GetTimestamp();
                        Point3D location = GetSpawnSpotLandDungeon();
                        trace.SpawnPointSearchMs += GetElapsedMilliseconds(started);
                        ++trace.LocationSearchCount;
                        return location;
                }

                private void OnBeforeSpawnWithTrace(IEntity entity, Point3D location, Map map)
                {
                        SpawnCallTrace trace = GetOwnedSpawnTrace();

                        if (trace == null)
                        {
                                if (entity is Mobile)
                                        ((Mobile)entity).OnBeforeSpawn(location, map);
                                else if (entity is Item)
                                        ((Item)entity).OnBeforeSpawn(location, map);

                                return;
                        }

                        long started = Stopwatch.GetTimestamp();

                        if (entity is Mobile)
                                ((Mobile)entity).OnBeforeSpawn(location, map);
                        else if (entity is Item)
                                ((Item)entity).OnBeforeSpawn(location, map);

                        trace.OnBeforeSpawnMs += GetElapsedMilliseconds(started);
                }

                private void MoveToWorldWithTrace(IEntity entity, Point3D location, Map map)
                {
                        SpawnCallTrace trace = GetOwnedSpawnTrace();

                        if (trace == null)
                        {
                                if (entity is Mobile)
                                        ((Mobile)entity).MoveToWorld(location, map);
                                else if (entity is Item)
                                        ((Item)entity).MoveToWorld(location, map);

                                return;
                        }

                        long started = Stopwatch.GetTimestamp();

                        if (entity is Mobile)
                                ((Mobile)entity).MoveToWorld(location, map);
                        else if (entity is Item)
                                ((Item)entity).MoveToWorld(location, map);

                        trace.MoveToWorldMs += GetElapsedMilliseconds(started);
                        TraceMovedToWorld(trace);

                        if (entity != null && entity.Deleted)
                                TraceDeletedAfterConstruction(trace, false);
                }

                private void OnAfterSpawnWithTrace(IEntity entity)
                {
                        SpawnCallTrace trace = GetOwnedSpawnTrace();

                        if (trace == null)
                        {
                                if (entity is BaseCreature)
                                        ((BaseCreature)entity).OnAfterSpawn(Location, Map);
                                else if (entity is Mobile)
                                        ((Mobile)entity).OnAfterSpawn();
                                else if (entity is Item)
                                        ((Item)entity).OnAfterSpawn();

                                return;
                        }

                        long started = Stopwatch.GetTimestamp();

                        if (entity is BaseCreature)
                                ((BaseCreature)entity).OnAfterSpawn(Location, Map);
                        else if (entity is Mobile)
                                ((Mobile)entity).OnAfterSpawn();
                        else if (entity is Item)
                                ((Item)entity).OnAfterSpawn();

                        trace.OnAfterSpawnMs += GetElapsedMilliseconds(started);

                        if (entity != null && entity.Deleted)
                                TraceDeletedAfterConstruction(trace, false);

                        FinishCurrentSpawnAttempt(trace);
                }

                private bool IsOutsideSpawnerRegionWithTrace(IEntity entity)
                {
                        SpawnCallTrace trace = GetOwnedSpawnTrace();
                        long started = trace != null ? Stopwatch.GetTimestamp() : 0;
                        bool outside = false;

                        if (entity is Mobile)
                        {
                                Mobile mobile = (Mobile)entity;
                                outside = Region.Find(Location, Map) != Region.Find(mobile.Location, mobile.Map);
                        }
                        else if (entity is Item)
                        {
                                Item item = (Item)entity;
                                outside = Region.Find(Location, Map) != Region.Find(item.Location, item.Map);
                        }

                        if (trace != null)
                                trace.RegionValidationMs += GetElapsedMilliseconds(started);

                        return outside;
                }

                private static void CopyStats(Dictionary<int, SpawnerStat> from, Dictionary<int, SpawnerStat> to)
                {
                        to.Clear();

                        foreach (SpawnerStat source in from.Values)
                        {
                                SpawnerStat copy = new SpawnerStat();
                                copy.CopyFrom(source);
                                to[copy.Serial] = copy;
                        }
                }

                private static SpawnerStat GetCurrentStatNoLock(PremiumSpawner spawner)
                {
                        if (spawner == null)
                                return null;

                        int serial = spawner.Serial.Value;
                        SpawnerStat stat;

                        if (!m_CurrentStats.TryGetValue(serial, out stat))
                        {
                                if (m_CurrentStats.Count >= MaxSpawnerStats)
                                        return null;

                                stat = new SpawnerStat();
                                stat.Serial = serial;
                                m_CurrentStats[serial] = stat;
                        }

                        return stat;
                }

                private static SpawnerStat GetTopStat(Dictionary<int, SpawnerStat> stats, string mode)
                {
                        SpawnerStat best = null;

                        foreach (SpawnerStat stat in stats.Values)
                        {
                                if (best == null)
                                {
                                        best = stat;
                                        continue;
                                }

                                if (mode == "total" && stat.TotalMs > best.TotalMs)
                                        best = stat;
                                else if (mode == "max" && stat.MaxMs > best.MaxMs)
                                        best = stat;
                                else if (mode == "count" && stat.Count > best.Count)
                                        best = stat;
                                else if (mode == "deferred" && stat.DeferredCount > best.DeferredCount)
                                        best = stat;
                                else if (mode == "late" && stat.MaxLatenessMs > best.MaxLatenessMs)
                                        best = stat;
                                else if (mode == "zeroDelay" && stat.ZeroNextDelayCount > best.ZeroNextDelayCount)
                                        best = stat;
                        }

                        return best;
                }

                private static bool HasSlowStats(Dictionary<int, SpawnerStat> stats)
                {
                        foreach (SpawnerStat stat in stats.Values)
                        {
                                if (stat.SlowCount > 0)
                                        return true;
                        }

                        return false;
                }

                private static void GetGlobalTotals(Dictionary<int, SpawnerStat> stats, out long count, out long slowCount, out long totalMs, out long maxMs, out long deferred)
                {
                        count = 0;
                        slowCount = 0;
                        totalMs = 0;
                        maxMs = 0;
                        deferred = 0;

                        foreach (SpawnerStat stat in stats.Values)
                        {
                                count += stat.Count;
                                slowCount += stat.SlowCount;
                                totalMs += stat.TotalMs;
                                deferred += stat.DeferredCount;

                                if (stat.MaxMs > maxMs)
                                        maxMs = stat.MaxMs;
                        }
                }

                private static string FormatStat(SpawnerStat stat)
                {
                        if (stat == null || (stat.Count == 0 && stat.LatenessCount == 0))
                                return "none";

                        long avg = stat.Count > 0 ? stat.TotalMs / stat.Count : 0;
                        long avgNearby = stat.Count > 0 ? stat.NearbyCheckedTotal / stat.Count : 0;
                        long avgLateness = stat.LatenessCount > 0 ? stat.LatenessTotalMs / stat.LatenessCount : 0;

                        return String.Format("0x{0:X} at {1}/{2} count={3} slow={4} total={5}ms max={6}ms avg={7}ms last={8}ms tracked={9} trackedBefore={33} trackedAfter={34} trackedDelta={35} categories={10} spawned={11} spawnedMeaning=positiveTrackedDeltaTotal lastSpawned={12} constructedTotal={36} acceptedIntoSpawnerListTotal={37} movedToWorldTotal={38} deletedAfterConstructionTotal={39} rejectedByRegionTotal={40} noObjectConstructedTotal={41} budgetDeferredDue={13} processedDue={14} duplicatePrevented={15} nextDelay={16}ms zeroNextDelay={17} zeroRepeated={18} latenessAvg={19}ms latenessMax={20}ms doTimer={21}/{22}ms region={23}/{24}ms defrag={25}/{26}ms spawn={27}/{28}ms nearbyTotal={29} nearbyMax={30} nearbyAvg={31} breakdown={32}",
                                stat.Serial,
                                stat.Map,
                                stat.Location,
                                stat.Count,
                                stat.SlowCount,
                                stat.TotalMs,
                                stat.MaxMs,
                                avg,
                                stat.LastMs,
                                stat.LastTracked,
                                stat.LastCategories,
                                stat.SpawnedTotal,
                                stat.LastSpawned,
                                stat.DeferredCount,
                                stat.ProcessedFromSchedulerCount,
                                stat.DuplicateEnqueueAvoidedCount,
                                stat.LastNextDelayMs,
                                stat.ZeroNextDelayCount,
                                stat.ZeroNextDelayRepeatedCount,
                                avgLateness,
                                stat.MaxLatenessMs,
                                stat.TotalDoTimerMs,
                                stat.MaxDoTimerMs,
                                stat.TotalRegionMs,
                                stat.MaxRegionMs,
                                stat.TotalDefragMs,
                                stat.MaxDefragMs,
                                stat.TotalSpawnMs,
                                stat.MaxSpawnMs,
                                stat.NearbyCheckedTotal,
                                stat.MaxNearbyChecked,
                                avgNearby,
                                !String.IsNullOrEmpty(stat.LastSlowBreakdown) ? stat.LastSlowBreakdown : "none",
                                stat.LastTrackedBefore,
                                stat.LastTrackedAfter,
                                stat.LastTrackedDelta,
                                stat.ConstructedTotal,
                                stat.AcceptedIntoSpawnerListTotal,
                                stat.MovedToWorldTotal,
                                stat.DeletedAfterConstructionTotal,
                                stat.RejectedByRegionTotal,
                                stat.NoObjectConstructedTotal);
                }

                private static string FormatSpawnBreakdown(SpawnCallTrace trace)
                {
                        if (trace == null)
                                return "unavailable";

                        return String.Format(
                                "categoriesInspected:{0},entriesInspected:{1},candidates:{2},selected:{3},constructedType:{4},constructedCount:{5},acceptedIntoSpawnerListCount:{6},movedToWorldCount:{7},deletedAfterConstructionCount:{8},rejectedByRegionCount:{9},noObjectConstructedCount:{10},parse:{11}ms,typeLookup:{12}ms,reflection:{13}ms,constructor:{14}ms,constructorMax:{15}ms,worstConstructor:{16},locationSearch:{17}ms/locationChecks:{18},before:{19}ms,move:{20}ms,setup:{21}ms,regionValidation:{22}ms,after:{23}ms,internalDefrag:{24}ms,attempts:{25},retries:{26},categoryMax:{27}ms/worstCategory:{28},error:{29},citizenConstructor:{30}",
                                trace.CategoriesInspected,
                                trace.EntriesInspected,
                                trace.CandidateTypesConsidered,
                                trace.SelectedType,
                                trace.ConstructedType,
                                trace.ConstructedCount,
                                trace.AcceptedIntoSpawnerListCount,
                                trace.MovedToWorldCount,
                                trace.DeletedAfterConstructionCount,
                                trace.RejectedByRegionCount,
                                trace.NoObjectConstructedCount,
                                trace.ArgumentParseMs,
                                trace.TypeLookupMs,
                                trace.ReflectionSetupMs,
                                trace.ConstructorMs,
                                trace.ConstructorMaxMs,
                                trace.WorstConstructorType,
                                trace.SpawnPointSearchMs,
                                trace.LocationSearchCount,
                                trace.OnBeforeSpawnMs,
                                trace.MoveToWorldMs,
                                trace.SetupMs,
                                trace.RegionValidationMs,
                                trace.OnAfterSpawnMs,
                                trace.InternalDefragMs,
                                trace.SpawnAttempts,
                                trace.RetryAttempts,
                                trace.CategoryMaxMs,
                                trace.WorstCategory,
                                trace.Error,
                                trace.HasCitizenConstructorTrace ? CitizenConstructorDiagnostics.FormatCompact(trace.WorstCitizenConstructorTrace) : "Not sampled");
                }

                private static string FormatSlowSample(SlowSpawnSample sample)
                {
                        if (sample == null || sample.TotalMs <= 0)
                                return "none";

                        long attributedSpawnMs = sample.ArgumentParseMs + sample.TypeLookupMs + sample.ReflectionSetupMs + sample.ConstructorMs
                                + sample.SpawnPointSearchMs + sample.OnBeforeSpawnMs + sample.MoveToWorldMs + sample.SetupMs
                                + sample.RegionValidationMs + sample.OnAfterSpawnMs + sample.InternalDefragMs;
                        long otherSpawnMs = sample.SpawnMs > attributedSpawnMs ? sample.SpawnMs - attributedSpawnMs : 0;

                        return String.Format(
                                "0x{0:X} at {1}/{2} region={3} total={4}ms doTimer={5}ms regionCheck={6}ms defrag={7}ms spawn={8}ms categories={9}/{10} entries={11} candidates={12} selected={13} constructedType={14} constructedCount={15} acceptedIntoSpawnerListCount={16} movedToWorldCount={17} deletedAfterConstructionCount={18} rejectedByRegionCount={19} noObjectConstructedCount={20} parse={21}ms typeLookup={22}ms reflection={23}ms constructor={24}ms constructorMax={25}ms worstConstructor={26} constructorIncludesEntitySetup=True locationSearch={27}ms/{28} before={29}ms move={30}ms setup={31}ms regionValidation={32}ms after={33}ms internalDefrag={34}ms otherSpawn={35}ms categoryMax={36}ms worstCategory={37} attempts={38} retries={39} spawned={40} spawnedMeaning=positiveTrackedDelta trackedBefore={41} trackedAfter={42} trackedDelta={43} nearbyChecked={44} nextDelay={45}ms error={46}",
                                sample.Serial,
                                sample.Map,
                                sample.Location,
                                sample.RegionName,
                                sample.TotalMs,
                                sample.DoTimerMs,
                                sample.RegionMs,
                                sample.DefragMs,
                                sample.SpawnMs,
                                sample.CategoriesInspected,
                                sample.ConfiguredCategories,
                                sample.EntriesInspected,
                                sample.CandidateTypesConsidered,
                                sample.SelectedType,
                                sample.ConstructedType,
                                sample.ConstructedCount,
                                sample.AcceptedIntoSpawnerListCount,
                                sample.MovedToWorldCount,
                                sample.DeletedAfterConstructionCount,
                                sample.RejectedByRegionCount,
                                sample.NoObjectConstructedCount,
                                sample.ArgumentParseMs,
                                sample.TypeLookupMs,
                                sample.ReflectionSetupMs,
                                sample.ConstructorMs,
                                sample.ConstructorMaxMs,
                                sample.WorstConstructorType,
                                sample.SpawnPointSearchMs,
                                sample.LocationSearchCount,
                                sample.OnBeforeSpawnMs,
                                sample.MoveToWorldMs,
                                sample.SetupMs,
                                sample.RegionValidationMs,
                                sample.OnAfterSpawnMs,
                                sample.InternalDefragMs,
                                otherSpawnMs,
                                sample.CategoryMaxMs,
                                sample.WorstCategory,
                                sample.Attempts,
                                sample.Retries,
                                sample.Spawned,
                                sample.TrackedBefore,
                                sample.TrackedAfter,
                                sample.TrackedDelta,
                                sample.NearbyChecked,
                                sample.NextDelayMs,
                                sample.Error);
                }

                private static void LogStatsWindow()
                {
                        SpawnerStat topTotal;
                        SpawnerStat topMax;
                        SpawnerStat topCount;

                        lock (m_StatsLock)
                        {
                                topTotal = GetTopStat(m_LastStats, "total");
                                topMax = GetTopStat(m_LastStats, "max");
                                topCount = GetTopStat(m_LastStats, "count");
                        }

                        long count;
                        long slow;
                        long total;
                        long max;
                        long deferred;

                        lock (m_StatsLock)
                        {
                                GetGlobalTotals(m_LastStats, out count, out slow, out total, out max, out deferred);
                        }

                        DiagnosticConsole.WriteParsedLine(String.Format("[Lag] PremiumSpawnerStats 60s: calls={0} slow={1} budgetDeferredDue={2} total={3}ms max={4}ms topTotal={5}", count, slow, deferred, total, max, FormatStat(topTotal)));
                        DiagnosticConsole.WriteParsedLine(String.Format("[Lag] PremiumSpawnerStats 60s: topMax={0}", FormatStat(topMax)));
                        DiagnosticConsole.WriteParsedLine(String.Format("[Lag] PremiumSpawnerStats 60s: topCount={0}", FormatStat(topCount)));
                }

                private static void EnsurePremiumSpawnerSchedulerNoLock()
                {
                        if (m_SchedulerTimer != null)
                                return;

                        m_SchedulerTimer = new PremiumSpawnerSchedulerTimer();
                        m_SchedulerTimer.Start();
                }

                private static long GetPremiumSpawnerDueBucket(DateTime dueTime)
                {
                        return dueTime.Ticks / TimeSpan.TicksPerSecond;
                }

                private static void RecordStartupCatchupDelay(long delayMs)
                {
                        lock (m_StatsLock)
                        {
                                ++m_CurrentSchedulerStats.StartupCatchupAssigned;
                                m_CurrentSchedulerStats.StartupCatchupDelayTotalMs += delayMs;

                                if (delayMs > m_CurrentSchedulerStats.StartupCatchupMaxDelayAssignedMs)
                                        m_CurrentSchedulerStats.StartupCatchupMaxDelayAssignedMs = delayMs;

                                ++m_StartupCatchupAssignedTotal;
                                m_StartupCatchupDelayTotalMs += delayMs;

                                if (delayMs > m_StartupCatchupMaxDelayAssignedMs)
                                        m_StartupCatchupMaxDelayAssignedMs = delayMs;
                        }
                }

                private static void GetUnpromotedDueStatsNoLock(long nowBucket, out int count, out long oldestAgeMs)
                {
                        count = 0;
                        oldestAgeMs = 0;
                        DateTime now = DateTime.UtcNow;

                        foreach (KeyValuePair<long, List<PremiumSpawner>> kvp in m_SchedulerFuture)
                        {
                                if (kvp.Key > nowBucket)
                                        break;

                                List<PremiumSpawner> bucket = kvp.Value;

                                if (bucket == null)
                                        continue;

                                for (int i = 0; i < bucket.Count; ++i)
                                {
                                        PremiumSpawner spawner = bucket[i];

                                        if (!IsValidScheduledSpawner(spawner))
                                                continue;

                                        long scheduledBucket;
                                        PremiumSpawner scheduledSpawner;

                                        if (!m_SchedulerDueBySerial.TryGetValue(spawner.Serial.Value, out scheduledBucket) || scheduledBucket != kvp.Key)
                                                continue;

                                        if (!m_SchedulerSpawnerBySerial.TryGetValue(spawner.Serial.Value, out scheduledSpawner) || scheduledSpawner != spawner)
                                                continue;

                                        ++count;

                                        if (spawner.m_End < now)
                                        {
                                                long age = (long)(now - spawner.m_End).TotalMilliseconds;

                                                if (age > oldestAgeMs)
                                                        oldestAgeMs = age;
                                        }
                                }
                        }
                }

                private static long GetOldestReadyAgeMsNoLock()
                {
                        long oldestAgeMs = 0;
                        DateTime now = DateTime.UtcNow;

                        foreach (PremiumSpawner spawner in m_SchedulerReadyQueued.Values)
                        {
                                if (!IsValidScheduledSpawner(spawner))
                                        continue;

                                if (spawner.m_End < now)
                                {
                                        long age = (long)(now - spawner.m_End).TotalMilliseconds;

                                        if (age > oldestAgeMs)
                                                oldestAgeMs = age;
                                }
                        }

                        return oldestAgeMs;
                }

                private static int GetPremiumSpawnerPromotionLimitNoLock()
                {
                        if (!EnablePremiumSpawnerPromotionBudget)
                                return Int32.MaxValue;

                        if (!EnablePremiumSpawnerAdaptivePromotion)
                                return PremiumSpawnerMaxPromotePerTickNormal;

                        int readyCount = m_SchedulerReadyQueued.Count;

                        if (readyCount >= PremiumSpawnerReadySevereBacklogPromotionThrottle)
                                return PremiumSpawnerMaxPromotePerTickWhenReadySevere;

                        if (readyCount >= PremiumSpawnerReadyBacklogPromotionThrottle)
                                return PremiumSpawnerMaxPromotePerTickWhenReadyBacklogged;

                        return PremiumSpawnerMaxPromotePerTickNormal;
                }

                private static void GetPremiumSpawnerProcessingLimitsNoLock(int readyCount, long oldestReadyAgeMs, out int adaptiveLevel, out int processLimit, out int budgetLimitMs)
                {
                        adaptiveLevel = 0;

                        if (EnablePremiumSpawnerAdaptiveProcessing)
                        {
                                if (readyCount > PremiumSpawnerBacklogReadyThreshold3 || oldestReadyAgeMs > PremiumSpawnerOldestReadyThresholdMs3)
                                        adaptiveLevel = 3;
                                else if (readyCount > PremiumSpawnerBacklogReadyThreshold2 || oldestReadyAgeMs > PremiumSpawnerOldestReadyThresholdMs2)
                                        adaptiveLevel = 2;
                                else if (readyCount > PremiumSpawnerBacklogReadyThreshold1 || oldestReadyAgeMs > PremiumSpawnerOldestReadyThresholdMs1)
                                        adaptiveLevel = 1;
                        }

                        if (m_LastSchedulerTickMsForAdaptive >= PremiumSpawnerSchedulerHardLagMs)
                        {
                                adaptiveLevel = 0;
                        }
                        else if (m_LastSchedulerTickMsForAdaptive >= PremiumSpawnerSchedulerSoftLagMs && adaptiveLevel > 0)
                        {
                                --adaptiveLevel;
                        }

                        switch (adaptiveLevel)
                        {
                                case 3:
                                        processLimit = PremiumSpawnerBacklogProcessPerTick3;
                                        budgetLimitMs = PremiumSpawnerBacklogBudgetMsPerTick3;
                                        break;
                                case 2:
                                        processLimit = PremiumSpawnerBacklogProcessPerTick2;
                                        budgetLimitMs = PremiumSpawnerBacklogBudgetMsPerTick2;
                                        break;
                                case 1:
                                        processLimit = PremiumSpawnerBacklogProcessPerTick1;
                                        budgetLimitMs = PremiumSpawnerBacklogBudgetMsPerTick1;
                                        break;
                                default:
                                        processLimit = PremiumSpawnerBaseProcessPerTick;
                                        budgetLimitMs = PremiumSpawnerBaseBudgetMsPerTick;
                                        break;
                        }
                }

                private static void RecordAdaptiveSelectionNoLock(int adaptiveLevel, int processLimit, int budgetLimitMs)
                {
                        m_CurrentSchedulerStats.AdaptiveLevelLast = adaptiveLevel;

                        if (adaptiveLevel > m_CurrentSchedulerStats.AdaptiveLevelMax)
                                m_CurrentSchedulerStats.AdaptiveLevelMax = adaptiveLevel;

                        if (adaptiveLevel == 3)
                                ++m_CurrentSchedulerStats.AdaptiveLevel3Count;
                        else if (adaptiveLevel == 2)
                                ++m_CurrentSchedulerStats.AdaptiveLevel2Count;
                        else if (adaptiveLevel == 1)
                                ++m_CurrentSchedulerStats.AdaptiveLevel1Count;
                        else
                                ++m_CurrentSchedulerStats.AdaptiveLevel0Count;

                        m_CurrentSchedulerStats.AdaptiveProcessLimitLast = processLimit;
                        m_CurrentSchedulerStats.AdaptiveBudgetLimitMsLast = budgetLimitMs;
                }

                private static long UpdateReadyBacklogDrainRateNoLock(int readyCount)
                {
                        long now = Core.TickCount;
                        long rate = 0;

                        if (m_LastReadyCountForDrainRate >= 0 && now > m_LastReadyDrainRateTick)
                        {
                                int drained = m_LastReadyCountForDrainRate - readyCount;

                                if (drained > 0)
                                        rate = (long)drained * 60000 / (now - m_LastReadyDrainRateTick);
                        }

                        m_LastReadyCountForDrainRate = readyCount;
                        m_LastReadyDrainRateTick = now;

                        return rate;
                }

                private static void SchedulePremiumSpawner(PremiumSpawner spawner, DateTime dueTime, string reason)
                {
                        if (spawner == null)
                                return;

                        lock (m_StatsLock)
                        {
                                if (spawner == null || spawner.Deleted || !spawner.m_Running)
                                {
                                        ++m_CurrentSchedulerStats.InvalidRemoved;
                                        ++m_SchedulerInvalidRemovedTotal;
                                        return;
                                }

                                int serial = spawner.Serial.Value;
                                long dueBucket = GetPremiumSpawnerDueBucket(dueTime);
                                long oldDueBucket;

                                if (m_SchedulerDueBySerial.TryGetValue(serial, out oldDueBucket))
                                {
                                        if (oldDueBucket == dueBucket)
                                        {
                                                ++m_CurrentSchedulerStats.DuplicatePrevented;
                                                ++m_SchedulerDuplicatePreventedTotal;

                                                SpawnerStat duplicateStat = GetCurrentStatNoLock(spawner);

                                                if (duplicateStat != null)
                                                        ++duplicateStat.DuplicateEnqueueAvoidedCount;

                                                EnsurePremiumSpawnerSchedulerNoLock();
                                                return;
                                        }

                                        ++m_CurrentSchedulerStats.Rescheduled;
                                        ++m_SchedulerRescheduledTotal;
                                }

                                if (m_SchedulerReadyQueued.ContainsKey(serial))
                                {
                                        m_SchedulerReadyQueued.Remove(serial);
                                        ++m_CurrentSchedulerStats.Rescheduled;
                                        ++m_SchedulerRescheduledTotal;
                                }

                                List<PremiumSpawner> bucket;

                                if (!m_SchedulerFuture.TryGetValue(dueBucket, out bucket))
                                {
                                        bucket = new List<PremiumSpawner>();
                                        m_SchedulerFuture[dueBucket] = bucket;
                                }

                                bucket.Add(spawner);
                                m_SchedulerDueBySerial[serial] = dueBucket;
                                m_SchedulerSpawnerBySerial[serial] = spawner;

                                int futureCount = m_SchedulerDueBySerial.Count;

                                if (futureCount > m_CurrentSchedulerStats.MaxFutureCount)
                                        m_CurrentSchedulerStats.MaxFutureCount = futureCount;

                                if (futureCount > m_SchedulerMaxFutureCountTotal)
                                        m_SchedulerMaxFutureCountTotal = futureCount;

                                EnsurePremiumSpawnerSchedulerNoLock();
                        }
                }

                private static void UnschedulePremiumSpawner(PremiumSpawner spawner)
                {
                        if (spawner == null)
                                return;

                        lock (m_StatsLock)
                        {
                                int serial = spawner.Serial.Value;
                                m_SchedulerDueBySerial.Remove(serial);
                                m_SchedulerSpawnerBySerial.Remove(serial);
                                m_SchedulerReadyQueued.Remove(serial);
                        }
                }

                private static void RecordSchedulerInvalidRemoved()
                {
                        lock (m_StatsLock)
                        {
                                ++m_CurrentSchedulerStats.InvalidRemoved;
                                ++m_SchedulerInvalidRemovedTotal;
                        }
                }

                private static bool IsValidScheduledSpawner(PremiumSpawner spawner)
                {
                        return (spawner != null && !spawner.Deleted && spawner.m_Running);
                }

                private static int PromoteDuePremiumSpawnersNoLock(out int unpromotedDue, out long oldestUnpromotedDueAgeMs)
                {
                        int promoted = 0;
                        long nowBucket = GetPremiumSpawnerDueBucket(DateTime.UtcNow);
                        int promoteLimit = GetPremiumSpawnerPromotionLimitNoLock();
                        bool promotionBudgetHit = false;

                        unpromotedDue = 0;
                        oldestUnpromotedDueAgeMs = 0;
                        m_CurrentSchedulerStats.AdaptivePromotionLimitLast = promoteLimit == Int32.MaxValue ? 0 : promoteLimit;

                        while (m_SchedulerFuture.Count > 0)
                        {
                                long firstBucket = 0;
                                List<PremiumSpawner> bucket = null;
                                bool found = false;

                                foreach (KeyValuePair<long, List<PremiumSpawner>> kvp in m_SchedulerFuture)
                                {
                                        firstBucket = kvp.Key;
                                        bucket = kvp.Value;
                                        found = true;
                                        break;
                                }

                                if (!found || firstBucket > nowBucket)
                                        break;

                                m_SchedulerFuture.Remove(firstBucket);

                                if (bucket == null)
                                        continue;

                                List<PremiumSpawner> remaining = null;

                                for (int i = 0; i < bucket.Count; ++i)
                                {
                                        PremiumSpawner spawner = bucket[i];

                                        if (promoted >= promoteLimit)
                                        {
                                                if (remaining == null)
                                                        remaining = new List<PremiumSpawner>();

                                                remaining.Add(spawner);
                                                promotionBudgetHit = true;
                                                continue;
                                        }

                                        if (!IsValidScheduledSpawner(spawner))
                                        {
                                                if (spawner != null)
                                                {
                                                        long staleBucket;
                                                        int staleSerial = spawner.Serial.Value;

                                                        if (m_SchedulerDueBySerial.TryGetValue(staleSerial, out staleBucket) && staleBucket == firstBucket)
                                                        {
                                                                m_SchedulerDueBySerial.Remove(staleSerial);
                                                                m_SchedulerSpawnerBySerial.Remove(staleSerial);
                                                        }
                                                }

                                                ++m_CurrentSchedulerStats.InvalidRemoved;
                                                ++m_SchedulerInvalidRemovedTotal;
                                                continue;
                                        }

                                        int serial = spawner.Serial.Value;
                                        long scheduledBucket;
                                        PremiumSpawner scheduledSpawner;

                                        if (!m_SchedulerDueBySerial.TryGetValue(serial, out scheduledBucket) || scheduledBucket != firstBucket)
                                                continue;

                                        if (!m_SchedulerSpawnerBySerial.TryGetValue(serial, out scheduledSpawner) || scheduledSpawner != spawner)
                                                continue;

                                        m_SchedulerDueBySerial.Remove(serial);
                                        m_SchedulerSpawnerBySerial.Remove(serial);

                                        if (m_SchedulerReadyQueued.ContainsKey(serial))
                                        {
                                                ++m_CurrentSchedulerStats.DuplicatePrevented;
                                                ++m_SchedulerDuplicatePreventedTotal;
                                                continue;
                                        }

                                        m_SchedulerReady.Enqueue(spawner);
                                        m_SchedulerReadyQueued[serial] = spawner;
                                        ++promoted;

                                        int readyCount = m_SchedulerReadyQueued.Count;

                                        if (readyCount > m_CurrentSchedulerStats.MaxReadyQueue)
                                                m_CurrentSchedulerStats.MaxReadyQueue = readyCount;

                                        if (readyCount > m_SchedulerMaxReadyQueueTotal)
                                                m_SchedulerMaxReadyQueueTotal = readyCount;
                                }

                                if (remaining != null && remaining.Count > 0)
                                {
                                        m_SchedulerFuture[firstBucket] = remaining;
                                        break;
                                }
                        }

                        if (promotionBudgetHit)
                        {
                                ++m_CurrentSchedulerStats.PromotionBudgetHitCount;
                                ++m_CurrentSchedulerStats.PromotionBudgetHitTotal;
                                ++m_SchedulerPromotionBudgetHitTotal;
                        }

                        m_CurrentSchedulerStats.PromotedDue += promoted;
                        m_CurrentSchedulerStats.PromotedDueThisTick = promoted;

                        GetUnpromotedDueStatsNoLock(nowBucket, out unpromotedDue, out oldestUnpromotedDueAgeMs);

                        m_CurrentSchedulerStats.UnpromotedDueCurrent = unpromotedDue;

                        if (unpromotedDue > m_CurrentSchedulerStats.UnpromotedDueMax)
                                m_CurrentSchedulerStats.UnpromotedDueMax = unpromotedDue;

                        if (unpromotedDue > m_SchedulerMaxUnpromotedDueTotal)
                                m_SchedulerMaxUnpromotedDueTotal = unpromotedDue;

                        m_CurrentSchedulerStats.OldestUnpromotedDueAgeMs = oldestUnpromotedDueAgeMs;

                        if (oldestUnpromotedDueAgeMs > m_CurrentSchedulerStats.MaxPromotionDelayMs)
                                m_CurrentSchedulerStats.MaxPromotionDelayMs = oldestUnpromotedDueAgeMs;

                        if (oldestUnpromotedDueAgeMs > m_SchedulerMaxOldestUnpromotedDueAgeMs)
                                m_SchedulerMaxOldestUnpromotedDueAgeMs = oldestUnpromotedDueAgeMs;

                        return promoted;
                }

                private static void RecordSpawnerLateness(PremiumSpawner spawner, long latenessMs)
                {
                        if (spawner == null || latenessMs <= 0)
                                return;

                        lock (m_StatsLock)
                        {
                                SpawnerStat stat = GetCurrentStatNoLock(spawner);

                                if (stat == null)
                                        return;

                                stat.LatenessTotalMs += latenessMs;
                                ++stat.LatenessCount;

                                if (latenessMs > stat.MaxLatenessMs)
                                        stat.MaxLatenessMs = latenessMs;

                                m_CurrentSchedulerStats.LatenessTotalMs += latenessMs;
                                ++m_CurrentSchedulerStats.LatenessSamples;

                                if (latenessMs > m_CurrentSchedulerStats.LatenessMaxMs)
                                        m_CurrentSchedulerStats.LatenessMaxMs = latenessMs;

                                if (latenessMs > m_TopLateSpawner.MaxLatenessMs)
                                {
                                        stat.Map = spawner.Map != null ? spawner.Map.ToString() : "null";
                                        stat.Location = spawner.Location;
                                        m_TopLateSpawner.CopyFrom(stat);
                                }
                        }
                }

                private static void DrainPremiumSpawnerScheduler()
                {
                        long start = Stopwatch.GetTimestamp();
                        int processedThisTick = 0;
                        int promoted = 0;
                        int unpromotedDue = 0;
                        long oldestUnpromotedDueAgeMs = 0;
                        int newlyBudgetDeferredThisTick = 0;
                        bool budgetHit = false;
                        int adaptiveLevel = 0;
                        int processLimit = PremiumSpawnerBaseProcessPerTick;
                        int budgetLimitMs = PremiumSpawnerBaseBudgetMsPerTick;
                        bool logWarning = false;
                        bool logAdaptiveLagWarning = false;
                        int readyCountForWarning = 0;
                        int futureCountForWarning = 0;
                        int unpromotedDueForWarning = 0;
                        long oldestReadyAgeForWarning = 0;
                        long oldestUnpromotedDueAgeForWarning = 0;
                        long schedulerElapsedForLagWarning = 0;

                        lock (m_StatsLock)
                        {
                                ++m_CurrentSchedulerStats.SchedulerTicks;
                                ++m_SchedulerTicksTotal;
                                promoted = PromoteDuePremiumSpawnersNoLock(out unpromotedDue, out oldestUnpromotedDueAgeMs);

                                int readyCount = m_SchedulerReadyQueued.Count;
                                long oldestReadyAgeMs = GetOldestReadyAgeMsNoLock();

                                GetPremiumSpawnerProcessingLimitsNoLock(readyCount, oldestReadyAgeMs, out adaptiveLevel, out processLimit, out budgetLimitMs);
                                RecordAdaptiveSelectionNoLock(adaptiveLevel, processLimit, budgetLimitMs);
                        }

                        while (true)
                        {
                                if (processedThisTick >= processLimit)
                                {
                                        budgetHit = true;
                                        break;
                                }

                                if (EnablePremiumSpawnerBudgeting && GetElapsedMilliseconds(start) >= budgetLimitMs)
                                {
                                        budgetHit = true;
                                        break;
                                }

                                PremiumSpawner spawner = null;

                                lock (m_StatsLock)
                                {
                                        if (m_SchedulerReady.Count == 0)
                                                break;

                                        spawner = m_SchedulerReady.Dequeue();

                                        if (spawner != null)
                                        {
                                                int serial = spawner.Serial.Value;
                                                PremiumSpawner readySpawner;

                                                if (!m_SchedulerReadyQueued.TryGetValue(serial, out readySpawner) || readySpawner != spawner)
                                                {
                                                        spawner = null;
                                                }
                                                else
                                                {
                                                        m_SchedulerReadyQueued.Remove(serial);

                                                        if (m_SchedulerDueBySerial.ContainsKey(serial))
                                                                spawner = null;
                                                }
                                        }
                                }

                                if (spawner == null || spawner.Deleted || !spawner.m_Running)
                                {
                                        if (spawner != null)
                                                RecordSchedulerInvalidRemoved();

                                        continue;
                                }

                                long latenessMs = 0;

                                if (spawner.m_End < DateTime.UtcNow)
                                        latenessMs = (long)(DateTime.UtcNow - spawner.m_End).TotalMilliseconds;

                                RecordSpawnerLateness(spawner, latenessMs);
                                int processedBeforeEntry = processedThisTick;
                                long entryStart = Stopwatch.GetTimestamp();
                                ++processedThisTick;
                                spawner.OnTick(true);
                                long entryElapsed = GetElapsedMilliseconds(entryStart);

                                if (EnablePremiumSpawnerBudgeting && entryElapsed >= budgetLimitMs)
                                {
                                        RecordSlowEntryYield(spawner, entryElapsed, processedBeforeEntry);
                                        budgetHit = true;
                                        break;
                                }
                        }

                        lock (m_StatsLock)
                        {
                                int readyCount = m_SchedulerReadyQueued.Count;
                                int futureCount = m_SchedulerDueBySerial.Count;
                                long elapsed = GetElapsedMilliseconds(start);
                                long oldestReadyAgeMs = GetOldestReadyAgeMsNoLock();
                                bool limitedByMsBudget = (readyCount > 0 && budgetHit && EnablePremiumSpawnerBudgeting && elapsed >= budgetLimitMs);
                                bool limitedByCountBudget = (readyCount > 0 && budgetHit && processedThisTick >= processLimit);
                                long backlogDrainRate = UpdateReadyBacklogDrainRateNoLock(readyCount);

                                if (readyCount > 0 && EnablePremiumSpawnerBudgeting && budgetHit)
                                {
                                        newlyBudgetDeferredThisTick = promoted > processedThisTick ? promoted - processedThisTick : 0;

                                        ++m_CurrentSchedulerStats.BudgetDeferredEvents;
                                        ++m_SchedulerBudgetDeferredEventsTotal;
                                        m_CurrentSchedulerStats.NewlyBudgetDeferredThisTick = newlyBudgetDeferredThisTick;
                                        m_CurrentSchedulerStats.BudgetDeferredDue += newlyBudgetDeferredThisTick;
                                        m_CurrentSchedulerStats.BudgetDeferredSpawnerCount += newlyBudgetDeferredThisTick;
                                        m_SchedulerBudgetDeferredDueTotal += newlyBudgetDeferredThisTick;
                                        ++m_CurrentSchedulerStats.AdaptiveBudgetHitCount;
                                }
                                else
                                {
                                        m_CurrentSchedulerStats.NewlyBudgetDeferredThisTick = 0;
                                }

                                m_CurrentSchedulerStats.ProcessedDueThisTick = processedThisTick;
                                m_CurrentSchedulerStats.ReadyBacklogCarriedCurrent = readyCount;

                                if (readyCount > m_CurrentSchedulerStats.ReadyBacklogCarriedMax)
                                        m_CurrentSchedulerStats.ReadyBacklogCarriedMax = readyCount;

                                m_CurrentSchedulerStats.OldestReadyAgeMs = oldestReadyAgeMs;

                                if (oldestReadyAgeMs > m_CurrentSchedulerStats.OldestReadyMaxAgeMs)
                                        m_CurrentSchedulerStats.OldestReadyMaxAgeMs = oldestReadyAgeMs;

                                if (oldestReadyAgeMs > m_SchedulerMaxOldestReadyAgeMs)
                                        m_SchedulerMaxOldestReadyAgeMs = oldestReadyAgeMs;

                                m_CurrentSchedulerStats.OldestReadyTotalMs += oldestReadyAgeMs;
                                ++m_CurrentSchedulerStats.OldestReadySamples;

                                if (oldestReadyAgeMs > PremiumSpawnerTargetOldestReadyMs)
                                        ++m_CurrentSchedulerStats.OldestReadyTicksOverTarget;

                                m_CurrentSchedulerStats.ReadyBacklogOverTargetCurrent = readyCount > PremiumSpawnerTargetReadyBacklog ? readyCount - PremiumSpawnerTargetReadyBacklog : 0;

                                if (m_CurrentSchedulerStats.ReadyBacklogOverTargetCurrent > m_CurrentSchedulerStats.ReadyBacklogOverTargetMax)
                                        m_CurrentSchedulerStats.ReadyBacklogOverTargetMax = m_CurrentSchedulerStats.ReadyBacklogOverTargetCurrent;

                                m_CurrentSchedulerStats.BacklogDrainRatePerMinute = backlogDrainRate;
                                m_CurrentSchedulerStats.AdaptiveBudgetMsUsedLast = elapsed;

                                if (limitedByMsBudget)
                                        ++m_CurrentSchedulerStats.AdaptiveProcessingLimitedByMsBudget;

                                if (limitedByCountBudget)
                                        ++m_CurrentSchedulerStats.AdaptiveProcessingLimitedByCountBudget;

                                m_CurrentSchedulerStats.SchedulerTickLastMs = elapsed;
                                m_CurrentSchedulerStats.SchedulerTickTotalMs += elapsed;
                                m_LastSchedulerTickMsForAdaptive = elapsed;

                                if (elapsed > m_CurrentSchedulerStats.SchedulerTickMaxMs)
                                        m_CurrentSchedulerStats.SchedulerTickMaxMs = elapsed;

                                if (readyCount > m_CurrentSchedulerStats.MaxReadyQueue)
                                        m_CurrentSchedulerStats.MaxReadyQueue = readyCount;

                                if (futureCount > m_CurrentSchedulerStats.MaxFutureCount)
                                        m_CurrentSchedulerStats.MaxFutureCount = futureCount;

                                if (readyCount > m_SchedulerMaxReadyQueueTotal)
                                        m_SchedulerMaxReadyQueueTotal = readyCount;

                                if (futureCount > m_SchedulerMaxFutureCountTotal)
                                        m_SchedulerMaxFutureCountTotal = futureCount;

                                long now = Core.TickCount;

                                if (elapsed >= PremiumSpawnerSchedulerHardLagMs && now >= m_NextAdaptiveLagWarning)
                                {
                                        m_NextAdaptiveLagWarning = now + StatsSummaryInterval;
                                        schedulerElapsedForLagWarning = elapsed;
                                        logAdaptiveLagWarning = true;
                                }

                                if ((readyCount >= PremiumSpawnerMaxReadyQueueWarning || futureCount >= PremiumSpawnerMaxFutureQueueWarning) && now >= m_NextSchedulerWarning)
                                {
                                        m_NextSchedulerWarning = now + StatsSummaryInterval;
                                        readyCountForWarning = readyCount;
                                        futureCountForWarning = futureCount;
                                        unpromotedDueForWarning = unpromotedDue;
                                        oldestReadyAgeForWarning = oldestReadyAgeMs;
                                        oldestUnpromotedDueAgeForWarning = oldestUnpromotedDueAgeMs;
                                        logWarning = true;
                                }

                                m_SchedulerTimer = null;

                                if (readyCount > 0 || futureCount > 0)
                                        EnsurePremiumSpawnerSchedulerNoLock();
                        }

                        if (logWarning)
                                DiagnosticConsole.WriteParsedLine(String.Format("[Lag] PremiumSpawner scheduler backlog: readyCount={0} futureCount={1} unpromotedDue={2} promotedDue={3} processedDue={4} adaptiveLevel={5} processLimit={6} budgetLimit={7}ms newlyBudgetDeferred={8} oldestReady={9}ms oldestUnpromoted={10}ms", readyCountForWarning, futureCountForWarning, unpromotedDueForWarning, promoted, processedThisTick, adaptiveLevel, processLimit, budgetLimitMs, newlyBudgetDeferredThisTick, oldestReadyAgeForWarning, oldestUnpromotedDueAgeForWarning));

                        if (logAdaptiveLagWarning)
                                DiagnosticConsole.WriteParsedLine(String.Format("[Lag] PremiumSpawner scheduler tick took {0}ms; adaptiveLevel={1}, processLimit={2}, budgetLimit={3}ms, processedDue={4}", schedulerElapsedForLagWarning, adaptiveLevel, processLimit, budgetLimitMs, processedThisTick));
                }

                private static void RecordSlowEntryYield(PremiumSpawner spawner, long entryElapsed, int processedBeforeEntry)
                {
                        lock (m_StatsLock)
                        {
                                ++m_CurrentSchedulerStats.SlowEntryYieldCount;
                                ++m_CurrentSchedulerStats.SingleSlowEntryBudgetBreaks;
                                ++m_SlowEntryYieldCountTotal;
                                ++m_SingleSlowEntryBudgetBreaksTotal;
                                m_CurrentSchedulerStats.ProcessedBeforeSlowEntry = processedBeforeEntry;
                                m_CurrentSchedulerStats.ProcessedAfterSlowEntry = 0;
                                m_ProcessedBeforeSlowEntry = processedBeforeEntry;
                                m_ProcessedAfterSlowEntry = 0;

                                if (entryElapsed > m_CurrentSchedulerStats.MaxSingleEntryMs)
                                {
                                        m_CurrentSchedulerStats.MaxSingleEntryMs = entryElapsed;
                                        m_CurrentSchedulerStats.MaxSingleEntrySerial = spawner != null ? spawner.Serial.Value : 0;
                                        m_CurrentSchedulerStats.MaxSingleEntryMap = spawner != null && spawner.Map != null ? spawner.Map.ToString() : "null";
                                        m_CurrentSchedulerStats.MaxSingleEntryLocation = spawner != null ? spawner.Location : new Point3D(0, 0, 0);
                                }

                                if (entryElapsed > m_MaxSingleEntryMsTotal)
                                {
                                        m_MaxSingleEntryMsTotal = entryElapsed;
                                        m_MaxSingleEntrySerial = spawner != null ? spawner.Serial.Value : 0;
                                        m_MaxSingleEntryMap = spawner != null && spawner.Map != null ? spawner.Map.ToString() : "null";
                                        m_MaxSingleEntryLocation = spawner != null ? spawner.Location : new Point3D(0, 0, 0);
                                }
                        }
                }

                private static void CopySchedulerStats(SchedulerWindowStats from, SchedulerWindowStats to)
                {
                        to.CopyFrom(from);
                }

                private static void CopyConstructorTypeStats(Dictionary<string, ConstructorTypeStat> from, Dictionary<string, ConstructorTypeStat> to)
                {
                        to.Clear();

                        foreach (ConstructorTypeStat source in from.Values)
                        {
                                ConstructorTypeStat copy = new ConstructorTypeStat();
                                copy.CopyFrom(source);
                                to[copy.TypeName] = copy;
                        }
                }

                private static ConstructorTypeStat GetConstructorTypeStatNoLock(Dictionary<string, ConstructorTypeStat> stats, string typeName)
                {
                        if (stats == null)
                                return null;

                        if (String.IsNullOrEmpty(typeName))
                                typeName = "unknown";

                        ConstructorTypeStat stat;

                        if (!stats.TryGetValue(typeName, out stat))
                        {
                                if (stats.Count >= MaxConstructorTypeStats)
                                        typeName = "(other)";

                                if (!stats.TryGetValue(typeName, out stat))
                                {
                                        stat = new ConstructorTypeStat();
                                        stat.TypeName = typeName;
                                        stats[typeName] = stat;
                                }
                        }

                        return stat;
                }

                private static void RecordConstructorTypeStatsNoLock(PremiumSpawner spawner, SpawnCallTrace trace, int trackedDelta)
                {
                        if (trace == null || trace.ConstructorInvocations == null || trace.ConstructorInvocations.Count == 0)
                                return;

                        for (int i = 0; i < trace.ConstructorInvocations.Count; ++i)
                        {
                                ConstructorInvocationTrace invocation = trace.ConstructorInvocations[i];
                                RecordConstructorTypeStatNoLock(m_CurrentConstructorTypeStats, spawner, invocation, trackedDelta);
                                RecordConstructorTypeStatNoLock(m_AllTimeConstructorTypeStats, spawner, invocation, trackedDelta);
                        }
                }

                private static void RecordConstructorTypeStatNoLock(Dictionary<string, ConstructorTypeStat> stats, PremiumSpawner spawner, ConstructorInvocationTrace invocation, int trackedDelta)
                {
                        ConstructorTypeStat stat = GetConstructorTypeStatNoLock(stats, invocation.TypeName);

                        if (stat == null)
                                return;

                        ++stat.Count;
                        stat.ConstructorTotalMs += invocation.ConstructorMs;
                        stat.SpawnCallTotalMs += invocation.SpawnCallMs;

                        if (invocation.ConstructorMs >= ConstructorSlowThreshold)
                                ++stat.SlowCount;

                        if (invocation.Constructed)
                                ++stat.ConstructedCount;

                        if (invocation.AcceptedIntoSpawnerList)
                                ++stat.AcceptedIntoSpawnerListCount;

                        if (invocation.MovedToWorld)
                                ++stat.MovedToWorldCount;

                        if (invocation.DeletedAfterConstruction)
                                ++stat.DeletedAfterConstructionCount;

                        if (invocation.RejectedByRegion)
                                ++stat.RejectedByRegionCount;

                        if (invocation.AcceptedIntoSpawnerList && !invocation.DeletedAfterConstruction)
                                ++stat.RetainedTrackedCount;

                        if (trackedDelta > 0)
                        {
                                ++stat.PositiveTrackedDeltaCallCount;
                        }
                        else
                        {
                                ++stat.NoPositiveTrackedDeltaCount;
                                stat.NoPositiveTrackedDeltaConstructorTotalMs += invocation.ConstructorMs;
                        }

                        if (invocation.HasCitizenConstructorTrace)
                        {
                                ++stat.CitizenTraceCount;

                                for (int i = 0; i < CitizenConstructorDiagnostics.PhaseCount; ++i)
                                {
                                        long phaseMs = invocation.CitizenConstructorTrace.GetPhaseMilliseconds((CitizenConstructorPhase)i);
                                        stat.CitizenPhaseTotals[i] += phaseMs;

                                        if (phaseMs > stat.CitizenPhaseMax[i])
                                                stat.CitizenPhaseMax[i] = phaseMs;
                                }
                        }

                        stat.LastSpawnerSerial = spawner != null ? spawner.Serial.Value : 0;
                        stat.LastSpawnerMap = spawner != null && spawner.Map != null ? spawner.Map.ToString() : "null";
                        stat.LastSpawnerLocation = spawner != null ? spawner.Location : new Point3D(0, 0, 0);

                        if (invocation.ConstructorMs > stat.ConstructorMaxMs)
                        {
                                stat.ConstructorMaxMs = invocation.ConstructorMs;
                                stat.SlowestSpawnerSerial = stat.LastSpawnerSerial;
                                stat.SlowestSpawnerMap = stat.LastSpawnerMap;
                                stat.SlowestSpawnerLocation = stat.LastSpawnerLocation;
                                stat.WorstSample = FormatConstructorInvocation(invocation, trackedDelta);
                        }

                        if (invocation.SpawnCallMs > stat.SpawnCallMaxMs)
                                stat.SpawnCallMaxMs = invocation.SpawnCallMs;
                }

                private static string FormatConstructorInvocation(ConstructorInvocationTrace invocation, int trackedDelta)
                {
                        return String.Format(
                                "type={0},constructor={1}ms,spawnCall={2}ms,constructed={3},acceptedIntoSpawnerList={4},movedToWorld={5},deletedAfterConstruction={6},rejectedByRegion={7},trackedDelta={8},error={9},citizenConstructor={10}",
                                invocation.TypeName,
                                invocation.ConstructorMs,
                                invocation.SpawnCallMs,
                                invocation.Constructed,
                                invocation.AcceptedIntoSpawnerList,
                                invocation.MovedToWorld,
                                invocation.DeletedAfterConstruction,
                                invocation.RejectedByRegion,
                                trackedDelta,
                                invocation.Error,
                                invocation.HasCitizenConstructorTrace ? CitizenConstructorDiagnostics.FormatCompact(invocation.CitizenConstructorTrace) : "Not sampled");
                }

                private static void RecordStats(PremiumSpawner spawner, long elapsed, long doTimerElapsed, long regionElapsed, long defragElapsed, long spawnElapsed, int trackedBefore, int trackedAfter, int categories, int spawned, int nearbyChecked, bool deferred, bool fromScheduler, TimeSpan nextDelay, SpawnCallTrace trace)
                {
                        bool logSummary = false;
                        FinishCurrentSpawnAttempt(trace);

                        lock (m_StatsLock)
                        {
                                SpawnerStat stat = GetCurrentStatNoLock(spawner);

                                if (!fromScheduler)
                                {
                                        ++m_CurrentSchedulerStats.OldInternalTimerCallbacks;
                                        ++m_SchedulerOldInternalTimerCallbacksTotal;
                                }
                                else if (!deferred)
                                {
                                        ++m_CurrentSchedulerStats.ProcessedDue;
                                        ++m_SchedulerProcessedTotal;
                                        m_CurrentSchedulerStats.SpawnWorkElapsedMs += elapsed;

                                        if (elapsed > m_CurrentSchedulerStats.SpawnWorkMaxMs)
                                                m_CurrentSchedulerStats.SpawnWorkMaxMs = elapsed;
                                }

                                if (elapsed >= SlowLogThreshold)
                                        ++m_CurrentSchedulerStats.SlowCalls;

                                if (stat != null)
                                {
                                        ++stat.Count;
                                        stat.TotalMs += elapsed;
                                        stat.LastMs = elapsed;
                                        stat.Map = spawner.Map != null ? spawner.Map.ToString() : "null";
                                        stat.Location = spawner.Location;
                                        stat.LastTracked = trackedAfter;
                                        stat.LastTrackedBefore = trackedBefore;
                                        stat.LastTrackedAfter = trackedAfter;
                                        stat.LastTrackedDelta = trackedAfter - trackedBefore;
                                        stat.LastCategories = categories;
                                        stat.LastNearbyChecked = nearbyChecked;
                                        stat.LastSpawned = spawned;
                                        stat.LastDeferred = deferred;
                                        stat.LastSlowBreakdown = elapsed >= DetailedSlowThreshold ? FormatSpawnBreakdown(trace) : stat.LastSlowBreakdown;
                                        long previousNextDelayMs = stat.LastNextDelayMs;
                                        stat.LastNextDelayMs = (long)nextDelay.TotalMilliseconds;

                                        if (nextDelay <= TimeSpan.Zero)
                                        {
                                                ++stat.ZeroNextDelayCount;
                                                ++m_CurrentSchedulerStats.ZeroNextDelayCount;

                                                if (stat.Count > 1 && previousNextDelayMs <= 0)
                                                {
                                                        ++stat.ZeroNextDelayRepeatedCount;
                                                        ++m_CurrentSchedulerStats.ZeroNextDelayRepeatedCount;
                                                }

                                                if (fromScheduler)
                                                        ++m_CurrentSchedulerStats.ZeroNextDelayProcessed;
                                                else
                                                        ++m_CurrentSchedulerStats.ZeroNextDelayRescheduled;

                                                if (stat.ZeroNextDelayCount > m_TopZeroNextDelaySpawner.ZeroNextDelayCount)
                                                        m_TopZeroNextDelaySpawner.CopyFrom(stat);
                                        }

                                        stat.SpawnedTotal += spawned;
                                        stat.ConstructedTotal += trace != null ? trace.ConstructedCount : 0;
                                        stat.AcceptedIntoSpawnerListTotal += trace != null ? trace.AcceptedIntoSpawnerListCount : 0;
                                        stat.MovedToWorldTotal += trace != null ? trace.MovedToWorldCount : 0;
                                        stat.DeletedAfterConstructionTotal += trace != null ? trace.DeletedAfterConstructionCount : 0;
                                        stat.RejectedByRegionTotal += trace != null ? trace.RejectedByRegionCount : 0;
                                        stat.NoObjectConstructedTotal += trace != null ? trace.NoObjectConstructedCount : 0;
                                        stat.TotalDoTimerMs += doTimerElapsed;
                                        stat.TotalRegionMs += regionElapsed;
                                        stat.TotalDefragMs += defragElapsed;
                                        stat.TotalSpawnMs += spawnElapsed;
                                        stat.NearbyCheckedTotal += nearbyChecked;

                                        if (deferred)
                                                ++stat.DeferredCount;
                                        else if (fromScheduler)
                                                ++stat.ProcessedFromSchedulerCount;

                                        if (elapsed >= SlowLogThreshold)
                                                ++stat.SlowCount;

                                        if (elapsed > stat.MaxMs)
                                                stat.MaxMs = elapsed;

                                        if (doTimerElapsed > stat.MaxDoTimerMs)
                                                stat.MaxDoTimerMs = doTimerElapsed;

                                        if (regionElapsed > stat.MaxRegionMs)
                                                stat.MaxRegionMs = regionElapsed;

                                        if (defragElapsed > stat.MaxDefragMs)
                                                stat.MaxDefragMs = defragElapsed;

                                        if (spawnElapsed > stat.MaxSpawnMs)
                                                stat.MaxSpawnMs = spawnElapsed;

                                        if (nearbyChecked > stat.MaxNearbyChecked)
                                                stat.MaxNearbyChecked = nearbyChecked;

                                        if (elapsed > m_WorstSpawnerSample.MaxMs)
                                                m_WorstSpawnerSample.CopyFrom(stat);

                                        if (stat.SlowCount > m_TopRepeatedSlowSpawner.SlowCount)
                                                m_TopRepeatedSlowSpawner.CopyFrom(stat);
                                }

                                if (elapsed >= DetailedSlowThreshold)
                                {
                                        SlowSpawnSample sample = new SlowSpawnSample();
                                        sample.CopyFrom(spawner, trace, elapsed, doTimerElapsed, regionElapsed, defragElapsed, spawnElapsed, categories, spawned, trackedBefore, trackedAfter, nearbyChecked, nextDelay);

                                        if (sample.TotalMs > m_WorstSingleEntrySample.TotalMs)
                                                m_WorstSingleEntrySample = sample;

                                        if (sample.SpawnMs > m_WorstSingleSpawnSample.SpawnMs)
                                                m_WorstSingleSpawnSample = sample;

                                        if (spawned == 0 && sample.TotalMs > m_WorstSlowNoOpSample.TotalMs)
                                                m_WorstSlowNoOpSample = sample;

                                        if (sample.ConstructorMaxMs > m_WorstConstructorSample.ConstructorMaxMs)
                                                m_WorstConstructorSample = sample;
                                }

                                RecordConstructorTypeStatsNoLock(spawner, trace, trackedAfter - trackedBefore);

                                long now = Core.TickCount;

                                if (m_NextStatsSummary == 0)
                                {
                                        m_NextStatsSummary = now + StatsSummaryInterval;
                                }
                                else if (now >= m_NextStatsSummary)
                                {
                                        m_NextStatsSummary = now + StatsSummaryInterval;
                                        CopyStats(m_CurrentStats, m_LastStats);
                                        CopyConstructorTypeStats(m_CurrentConstructorTypeStats, m_LastConstructorTypeStats);
                                        CopySchedulerStats(m_CurrentSchedulerStats, m_LastSchedulerStats);
                                        logSummary = HasSlowStats(m_CurrentStats);
                                        m_CurrentStats.Clear();
                                        m_CurrentConstructorTypeStats.Clear();
                                        m_CurrentSchedulerStats.Reset();
                                }
                        }

                        if (logSummary)
                                LogStatsWindow();
                }

                private static void WriteConstructorTypeStats(TextWriter writer, string label, Dictionary<string, ConstructorTypeStat> stats)
                {
                        writer.WriteLine("PremiumSpawner constructorTypes {0}: types={1}", label, stats != null ? stats.Count : 0);

                        if (stats == null || stats.Count == 0)
                        {
                                writer.WriteLine("PremiumSpawner constructorTypes {0}: Not sampled", label);
                                return;
                        }

                        WriteConstructorTypeTop(writer, label, "topTotal", GetSortedConstructorTypes(stats, "total"));
                        WriteConstructorTypeTop(writer, label, "topMax", GetSortedConstructorTypes(stats, "max"));

                        if (String.Equals(label, "allTime", StringComparison.Ordinal))
                        {
                                WriteConstructorTypeTop(writer, label, "topCount", GetSortedConstructorTypes(stats, "count"));
                                WriteConstructorTypeTop(writer, label, "topNoPositiveTrackedDelta", GetSortedConstructorTypes(stats, "noPositiveTrackedDelta"));
                        }
                }

                private static void WriteConstructorTypeTop(TextWriter writer, string label, string mode, List<ConstructorTypeStat> stats)
                {
                        int count = Math.Min(ConstructorTypeTopCount, stats.Count);

                        if (count == 0)
                        {
                                writer.WriteLine("PremiumSpawner constructorTypes {0} {1}: none", label, mode);
                                return;
                        }

                        for (int i = 0; i < count; ++i)
                                writer.WriteLine("PremiumSpawner constructorTypes {0} {1} #{2}: {3}", label, mode, i + 1, FormatConstructorTypeStat(stats[i]));
                }

                private static List<ConstructorTypeStat> GetSortedConstructorTypes(Dictionary<string, ConstructorTypeStat> stats, string mode)
                {
                        List<ConstructorTypeStat> list = new List<ConstructorTypeStat>();

                        if (stats != null)
                        {
                                foreach (ConstructorTypeStat stat in stats.Values)
                                {
                                        if (mode == "noPositiveTrackedDelta" && stat.NoPositiveTrackedDeltaCount <= 0)
                                                continue;

                                        list.Add(stat);
                                }
                        }

                        list.Sort(delegate(ConstructorTypeStat left, ConstructorTypeStat right)
                        {
                                long leftValue;
                                long rightValue;

                                if (mode == "max")
                                {
                                        leftValue = left.ConstructorMaxMs;
                                        rightValue = right.ConstructorMaxMs;
                                }
                                else if (mode == "count")
                                {
                                        leftValue = left.Count;
                                        rightValue = right.Count;
                                }
                                else if (mode == "noPositiveTrackedDelta")
                                {
                                        leftValue = left.NoPositiveTrackedDeltaConstructorTotalMs;
                                        rightValue = right.NoPositiveTrackedDeltaConstructorTotalMs;
                                }
                                else
                                {
                                        leftValue = left.ConstructorTotalMs;
                                        rightValue = right.ConstructorTotalMs;
                                }

                                int compare = rightValue.CompareTo(leftValue);
                                return compare != 0 ? compare : String.Compare(left.TypeName, right.TypeName, StringComparison.Ordinal);
                        });

                        return list;
                }

                private static string FormatConstructorTypeStat(ConstructorTypeStat stat)
                {
                        if (stat == null || stat.Count <= 0)
                                return "none";

                        long constructorAvg = stat.ConstructorTotalMs / stat.Count;
                        long spawnCallAvg = stat.SpawnCallTotalMs / stat.Count;
                        int citizenTopTotal = GetTopCitizenPhaseIndex(stat.CitizenPhaseTotals, false);
                        int citizenTopMax = GetTopCitizenPhaseIndex(stat.CitizenPhaseMax, false);
                        int citizenSetupTopTotal = GetTopCitizenPhaseIndex(stat.CitizenPhaseTotals, true);
                        int citizenSetupTopMax = GetTopCitizenPhaseIndex(stat.CitizenPhaseMax, true);

                        return String.Format(
                                "type={0} count={1} slow={2} constructorTotal={3}ms constructorMax={4}ms constructorAvg={5}ms spawnCallTotal={6}ms spawnCallMax={7}ms spawnCallAvg={8}ms constructed={9} acceptedIntoSpawnerList={10} movedToWorld={11} deletedAfterConstruction={12} rejectedByRegion={13} retainedTracked={14} spawnedPositiveTrackedDeltaCalls={15} noPositiveTrackedDelta={16}/{17}ms citizenTraces={18} citizenTopSubphaseTotal={19}:{20}ms citizenTopSubphaseMax={21}:{22}ms citizenSetupTopTotal={23}:{24}ms citizenSetupTopMax={25}:{26}ms slowestSpawner=0x{27:X}@{28}/{29} lastSpawner=0x{30:X}@{31}/{32} worst={33}",
                                stat.TypeName,
                                stat.Count,
                                stat.SlowCount,
                                stat.ConstructorTotalMs,
                                stat.ConstructorMaxMs,
                                constructorAvg,
                                stat.SpawnCallTotalMs,
                                stat.SpawnCallMaxMs,
                                spawnCallAvg,
                                stat.ConstructedCount,
                                stat.AcceptedIntoSpawnerListCount,
                                stat.MovedToWorldCount,
                                stat.DeletedAfterConstructionCount,
                                stat.RejectedByRegionCount,
                                stat.RetainedTrackedCount,
                                stat.PositiveTrackedDeltaCallCount,
                                stat.NoPositiveTrackedDeltaCount,
                                stat.NoPositiveTrackedDeltaConstructorTotalMs,
                                stat.CitizenTraceCount,
                                citizenTopTotal >= 0 ? CitizenConstructorDiagnostics.GetPhaseName((CitizenConstructorPhase)citizenTopTotal) : "Not sampled",
                                citizenTopTotal >= 0 ? stat.CitizenPhaseTotals[citizenTopTotal] : 0,
                                citizenTopMax >= 0 ? CitizenConstructorDiagnostics.GetPhaseName((CitizenConstructorPhase)citizenTopMax) : "Not sampled",
                                citizenTopMax >= 0 ? stat.CitizenPhaseMax[citizenTopMax] : 0,
                                citizenSetupTopTotal >= 0 ? CitizenConstructorDiagnostics.GetPhaseName((CitizenConstructorPhase)citizenSetupTopTotal) : "Not sampled",
                                citizenSetupTopTotal >= 0 ? stat.CitizenPhaseTotals[citizenSetupTopTotal] : 0,
                                citizenSetupTopMax >= 0 ? CitizenConstructorDiagnostics.GetPhaseName((CitizenConstructorPhase)citizenSetupTopMax) : "Not sampled",
                                citizenSetupTopMax >= 0 ? stat.CitizenPhaseMax[citizenSetupTopMax] : 0,
                                stat.SlowestSpawnerSerial,
                                stat.SlowestSpawnerMap,
                                stat.SlowestSpawnerLocation,
                                stat.LastSpawnerSerial,
                                stat.LastSpawnerMap,
                                stat.LastSpawnerLocation,
                                !String.IsNullOrEmpty(stat.WorstSample) ? stat.WorstSample : "none");
                }

                private static int GetTopCitizenPhaseIndex(long[] values, bool setupOnly)
                {
                        if (values == null)
                                return -1;

                        int best = -1;
                        long bestValue = -1;

                        for (int i = 0; i < CitizenConstructorDiagnostics.PhaseCount; ++i)
                        {
                                CitizenConstructorPhase phase = (CitizenConstructorPhase)i;
                                bool setupDetail = phase >= CitizenConstructorPhase.BackpackReplace
                                        && phase <= CitizenConstructorPhase.PhraseRumorGeneration;

                                if (setupOnly != setupDetail)
                                        continue;

                                if (values[i] > bestValue)
                                {
                                        best = i;
                                        bestValue = values[i];
                                }
                        }

                        return bestValue > 0 ? best : -1;
                }

                public static void WriteStats(TextWriter writer)
                {
                        if (writer == null)
                                return;

                        Dictionary<int, SpawnerStat> current = new Dictionary<int, SpawnerStat>();
                        Dictionary<int, SpawnerStat> last = new Dictionary<int, SpawnerStat>();
                        Dictionary<string, ConstructorTypeStat> currentConstructorTypes = new Dictionary<string, ConstructorTypeStat>(StringComparer.Ordinal);
                        Dictionary<string, ConstructorTypeStat> lastConstructorTypes = new Dictionary<string, ConstructorTypeStat>(StringComparer.Ordinal);
                        Dictionary<string, ConstructorTypeStat> allTimeConstructorTypes = new Dictionary<string, ConstructorTypeStat>(StringComparer.Ordinal);
                        SpawnerStat worst = new SpawnerStat();
                        SpawnerStat topLate = new SpawnerStat();
                        SpawnerStat topZeroDelay = new SpawnerStat();
                        SchedulerWindowStats currentScheduler = new SchedulerWindowStats();
                        SchedulerWindowStats lastScheduler = new SchedulerWindowStats();
                        int readyCount;
                        int futureCount;
                        long schedulerProcessed;
                        long schedulerBudgetDeferredDue;
                        long schedulerDuplicates;
                        long schedulerInvalid;
                        long schedulerTicks;
                        long schedulerRescheduled;
                        long oldInternalTimerCallbacks;
                        long maxReadyQueue;
                        long maxFutureCount;
                        long promotionBudgetHits;
                        long budgetDeferredEvents;
                        long maxUnpromotedDue;
                        long maxOldestReadyAgeMs;
                        long maxOldestUnpromotedDueAgeMs;
                        long startupCatchupAssigned;
                        long startupCatchupDelayTotalMs;
                        long startupCatchupMaxDelayAssignedMs;

                        lock (m_StatsLock)
                        {
                                CopyStats(m_CurrentStats, current);
                                CopyStats(m_LastStats, last);
                                CopyConstructorTypeStats(m_CurrentConstructorTypeStats, currentConstructorTypes);
                                CopyConstructorTypeStats(m_LastConstructorTypeStats, lastConstructorTypes);
                                CopyConstructorTypeStats(m_AllTimeConstructorTypeStats, allTimeConstructorTypes);
                                worst.CopyFrom(m_WorstSpawnerSample);
                                topLate.CopyFrom(m_TopLateSpawner);
                                topZeroDelay.CopyFrom(m_TopZeroNextDelaySpawner);
                                currentScheduler.CopyFrom(m_CurrentSchedulerStats);
                                lastScheduler.CopyFrom(m_LastSchedulerStats);
                                readyCount = m_SchedulerReadyQueued.Count;
                                futureCount = m_SchedulerDueBySerial.Count;
                                schedulerProcessed = m_SchedulerProcessedTotal;
                                schedulerBudgetDeferredDue = m_SchedulerBudgetDeferredDueTotal;
                                schedulerDuplicates = m_SchedulerDuplicatePreventedTotal;
                                schedulerInvalid = m_SchedulerInvalidRemovedTotal;
                                schedulerTicks = m_SchedulerTicksTotal;
                                schedulerRescheduled = m_SchedulerRescheduledTotal;
                                oldInternalTimerCallbacks = m_SchedulerOldInternalTimerCallbacksTotal;
                                maxReadyQueue = m_SchedulerMaxReadyQueueTotal;
                                maxFutureCount = m_SchedulerMaxFutureCountTotal;
                                promotionBudgetHits = m_SchedulerPromotionBudgetHitTotal;
                                budgetDeferredEvents = m_SchedulerBudgetDeferredEventsTotal;
                                maxUnpromotedDue = m_SchedulerMaxUnpromotedDueTotal;
                                maxOldestReadyAgeMs = m_SchedulerMaxOldestReadyAgeMs;
                                maxOldestUnpromotedDueAgeMs = m_SchedulerMaxOldestUnpromotedDueAgeMs;
                                startupCatchupAssigned = m_StartupCatchupAssignedTotal;
                                startupCatchupDelayTotalMs = m_StartupCatchupDelayTotalMs;
                                startupCatchupMaxDelayAssignedMs = m_StartupCatchupMaxDelayAssignedMs;
                        }

                        WriteStatsWindow(writer, "current", current);
                        WriteStatsWindow(writer, "last", last);
                        writer.WriteLine("PremiumSpawner worstSample: {0}", FormatStat(worst));
                        writer.WriteLine("PremiumSpawner topLateSpawner: {0}", FormatStat(topLate));
                        writer.WriteLine("PremiumSpawner topZeroNextDelaySpawner: {0}", FormatStat(topZeroDelay));
                        WriteSingleEntryStats(writer);
                        WriteConstructorTypeStats(writer, "current", currentConstructorTypes);
                        WriteConstructorTypeStats(writer, "last", lastConstructorTypes);
                        WriteConstructorTypeStats(writer, "allTime", allTimeConstructorTypes);
                        CitizenConstructorDiagnostics.WriteStats(writer);
                        writer.WriteLine("PremiumSpawner scheduler: schedulerEnabled={0}, budgetEnabled={1}, promotionBudgetEnabled={2}, adaptiveProcessingEnabled={3}, adaptivePromotionEnabled={4}, startupCatchupEnabled={5}, interval={6}ms, maxPromoteNormal={7}, baseProcessPerTick={8}, baseBudgetMsPerTick={9}, readyCount={10}, futureCount={11}, maxReadyQueue={12}, maxFutureCount={13}",
                                EnablePremiumSpawnerScheduler,
                                EnablePremiumSpawnerBudgeting,
                                EnablePremiumSpawnerPromotionBudget,
                                EnablePremiumSpawnerAdaptiveProcessing,
                                EnablePremiumSpawnerAdaptivePromotion,
                                EnablePremiumSpawnerStartupCatchupJitter,
                                PremiumSpawnerSchedulerIntervalMs,
                                PremiumSpawnerMaxPromotePerTickNormal,
                                PremiumSpawnerBaseProcessPerTick,
                                PremiumSpawnerBaseBudgetMsPerTick,
                                readyCount,
                                futureCount,
                                maxReadyQueue,
                                maxFutureCount);
                        writer.WriteLine("PremiumSpawner scheduler totals: processedDue={0}, budgetDeferredDue={1}, budgetDeferredEvents={2}, promotionBudgetHits={3}, duplicatePrevented={4}, invalidRemoved={5}, rescheduled={6}, schedulerTicks={7}, oldInternalTimerCallbacks={8}, maxUnpromotedDue={9}, maxOldestReadyAgeMs={10}, maxOldestUnpromotedDueAgeMs={11}",
                                schedulerProcessed,
                                schedulerBudgetDeferredDue,
                                budgetDeferredEvents,
                                promotionBudgetHits,
                                schedulerDuplicates,
                                schedulerInvalid,
                                schedulerRescheduled,
                                schedulerTicks,
                                oldInternalTimerCallbacks,
                                maxUnpromotedDue,
                                maxOldestReadyAgeMs,
                                maxOldestUnpromotedDueAgeMs);
                        writer.WriteLine("PremiumSpawner startupCatchup: assigned={0}, avgDelay={1}ms, maxDelay={2}ms, window={3}s, minDelay={4}s",
                                startupCatchupAssigned,
                                startupCatchupAssigned > 0 ? startupCatchupDelayTotalMs / startupCatchupAssigned : 0,
                                startupCatchupMaxDelayAssignedMs,
                                PremiumSpawnerStartupCatchupWindowSeconds,
                                PremiumSpawnerStartupCatchupMinDelaySeconds);
                        WriteSchedulerWindow(writer, "current", currentScheduler);
                        WriteSchedulerWindow(writer, "last", lastScheduler);
                }

                public static void WriteBudgetStats(TextWriter writer)
                {
                        WriteSchedulerStats(writer);
                }

                public static void GetPerformanceTestSummary(out long oldInternalTimerCallbacks, out long oldestReadyAgeMs)
                {
                        long maxSingleEntryMs;
                        long maxSingleSpawnMs;
                        GetPerformanceTestSummary(out oldInternalTimerCallbacks, out oldestReadyAgeMs, out maxSingleEntryMs, out maxSingleSpawnMs);
                }

                public static void GetPerformanceTestSummary(out long oldInternalTimerCallbacks, out long oldestReadyAgeMs, out long maxSingleEntryMs, out long maxSingleSpawnMs)
                {
                        lock (m_StatsLock)
                        {
                                oldInternalTimerCallbacks = m_SchedulerOldInternalTimerCallbacksTotal;
                                oldestReadyAgeMs = m_CurrentSchedulerStats.SchedulerTicks > 0 ? m_CurrentSchedulerStats.OldestReadyAgeMs : m_LastSchedulerStats.OldestReadyAgeMs;
                                maxSingleEntryMs = Math.Max(m_WorstSpawnerSample.MaxMs, m_WorstSingleEntrySample.TotalMs);
                                maxSingleSpawnMs = Math.Max(m_WorstSpawnerSample.MaxSpawnMs, m_WorstSingleSpawnSample.SpawnMs);
                        }
                }

                public static void GetConstructorPerformanceTestSummary(out string topMaxType, out long topMaxMs, out string topTotalType, out long topTotalMs, out long topTotalCount)
                {
                        lock (m_StatsLock)
                        {
                                ConstructorTypeStat topMax = GetTopConstructorTypeNoLock(m_AllTimeConstructorTypeStats, "max");
                                ConstructorTypeStat topTotal = GetTopConstructorTypeNoLock(m_AllTimeConstructorTypeStats, "total");

                                topMaxType = topMax != null ? topMax.TypeName : "Not sampled";
                                topMaxMs = topMax != null ? topMax.ConstructorMaxMs : 0;
                                topTotalType = topTotal != null ? topTotal.TypeName : "Not sampled";
                                topTotalMs = topTotal != null ? topTotal.ConstructorTotalMs : 0;
                                topTotalCount = topTotal != null ? topTotal.Count : 0;
                        }
                }

                private static ConstructorTypeStat GetTopConstructorTypeNoLock(Dictionary<string, ConstructorTypeStat> stats, string mode)
                {
                        ConstructorTypeStat best = null;

                        if (stats == null)
                                return null;

                        foreach (ConstructorTypeStat stat in stats.Values)
                        {
                                if (best == null
                                        || (mode == "max" && stat.ConstructorMaxMs > best.ConstructorMaxMs)
                                        || (mode != "max" && stat.ConstructorTotalMs > best.ConstructorTotalMs))
                                        best = stat;
                        }

                        return best;
                }

                public static void WriteSchedulerStats(TextWriter writer)
                {
                        if (writer == null)
                                return;

                        SchedulerWindowStats current = new SchedulerWindowStats();
                        SchedulerWindowStats last = new SchedulerWindowStats();
                        int readyCount;
                        int futureCount;
                        long schedulerProcessed;
                        long budgetDeferredDue;
                        long duplicatePrevented;
                        long invalidRemoved;
                        long schedulerTicks;
                        long rescheduled;
                        long oldInternalTimerCallbacks;
                        long maxReadyQueue;
                        long maxFutureCount;
                        long promotionBudgetHits;
                        long budgetDeferredEvents;
                        long maxUnpromotedDue;
                        long maxOldestReadyAgeMs;
                        long maxOldestUnpromotedDueAgeMs;
                        long startupCatchupAssigned;
                        long startupCatchupDelayTotalMs;
                        long startupCatchupMaxDelayAssignedMs;
                        SpawnerStat topLate;
                        SpawnerStat topZeroDelay;

                        lock (m_StatsLock)
                        {
                                current.CopyFrom(m_CurrentSchedulerStats);
                                last.CopyFrom(m_LastSchedulerStats);
                                readyCount = m_SchedulerReadyQueued.Count;
                                futureCount = m_SchedulerDueBySerial.Count;
                                schedulerProcessed = m_SchedulerProcessedTotal;
                                budgetDeferredDue = m_SchedulerBudgetDeferredDueTotal;
                                duplicatePrevented = m_SchedulerDuplicatePreventedTotal;
                                invalidRemoved = m_SchedulerInvalidRemovedTotal;
                                schedulerTicks = m_SchedulerTicksTotal;
                                rescheduled = m_SchedulerRescheduledTotal;
                                oldInternalTimerCallbacks = m_SchedulerOldInternalTimerCallbacksTotal;
                                maxReadyQueue = m_SchedulerMaxReadyQueueTotal;
                                maxFutureCount = m_SchedulerMaxFutureCountTotal;
                                promotionBudgetHits = m_SchedulerPromotionBudgetHitTotal;
                                budgetDeferredEvents = m_SchedulerBudgetDeferredEventsTotal;
                                maxUnpromotedDue = m_SchedulerMaxUnpromotedDueTotal;
                                maxOldestReadyAgeMs = m_SchedulerMaxOldestReadyAgeMs;
                                maxOldestUnpromotedDueAgeMs = m_SchedulerMaxOldestUnpromotedDueAgeMs;
                                startupCatchupAssigned = m_StartupCatchupAssignedTotal;
                                startupCatchupDelayTotalMs = m_StartupCatchupDelayTotalMs;
                                startupCatchupMaxDelayAssignedMs = m_StartupCatchupMaxDelayAssignedMs;
                                topLate = new SpawnerStat();
                                topLate.CopyFrom(m_TopLateSpawner);
                                topZeroDelay = new SpawnerStat();
                                topZeroDelay.CopyFrom(m_TopZeroNextDelaySpawner);
                        }

                        writer.WriteLine("PremiumSpawnerScheduler: schedulerEnabled={0}, budgetEnabled={1}, promotionBudgetEnabled={2}, adaptiveProcessingEnabled={3}, adaptivePromotionEnabled={4}, startupCatchupEnabled={5}, interval={6}ms, maxPromoteNormal={7}, baseProcessPerTick={8}, baseBudgetMsPerTick={9}, readyCount={10}, futureCount={11}, maxReadyQueue={12}, maxFutureCount={13}",
                                EnablePremiumSpawnerScheduler,
                                EnablePremiumSpawnerBudgeting,
                                EnablePremiumSpawnerPromotionBudget,
                                EnablePremiumSpawnerAdaptiveProcessing,
                                EnablePremiumSpawnerAdaptivePromotion,
                                EnablePremiumSpawnerStartupCatchupJitter,
                                PremiumSpawnerSchedulerIntervalMs,
                                PremiumSpawnerMaxPromotePerTickNormal,
                                PremiumSpawnerBaseProcessPerTick,
                                PremiumSpawnerBaseBudgetMsPerTick,
                                readyCount,
                                futureCount,
                                maxReadyQueue,
                                maxFutureCount);
                        writer.WriteLine("  totals: processedDue={0}, budgetDeferredDue={1}, budgetDeferredEvents={2}, promotionBudgetHits={3}, duplicatePrevented={4}, invalidRemoved={5}, rescheduled={6}, schedulerTicks={7}, oldInternalTimerCallbacks={8}, maxUnpromotedDue={9}, maxOldestReadyAgeMs={10}, maxOldestUnpromotedDueAgeMs={11}",
                                schedulerProcessed,
                                budgetDeferredDue,
                                budgetDeferredEvents,
                                promotionBudgetHits,
                                duplicatePrevented,
                                invalidRemoved,
                                rescheduled,
                                schedulerTicks,
                                oldInternalTimerCallbacks,
                                maxUnpromotedDue,
                                maxOldestReadyAgeMs,
                                maxOldestUnpromotedDueAgeMs);
                        writer.WriteLine("  startupCatchup: assigned={0}, avgDelay={1}ms, maxDelay={2}ms, window={3}s, minDelay={4}s",
                                startupCatchupAssigned,
                                startupCatchupAssigned > 0 ? startupCatchupDelayTotalMs / startupCatchupAssigned : 0,
                                startupCatchupMaxDelayAssignedMs,
                                PremiumSpawnerStartupCatchupWindowSeconds,
                                PremiumSpawnerStartupCatchupMinDelaySeconds);
                        writer.WriteLine("  topLateSpawner: {0}", FormatStat(topLate));
                        writer.WriteLine("  topZeroNextDelaySpawner: {0}", FormatStat(topZeroDelay));
                        WriteSingleEntryStats(writer);
                        WriteSchedulerWindow(writer, "current", current);
                        WriteSchedulerWindow(writer, "last", last);
                }

                private static void WriteSingleEntryStats(TextWriter writer)
                {
                        if (writer == null)
                                return;

                        SlowSpawnSample worstEntry;
                        SlowSpawnSample worstSpawn;
                        SlowSpawnSample worstNoOp;
                        SlowSpawnSample worstConstructor;
                        SpawnerStat topRepeated = new SpawnerStat();
                        long slowEntryYieldCount;
                        long singleSlowEntryBudgetBreaks;
                        long maxSingleEntryMs;
                        int maxSingleEntrySerial;
                        string maxSingleEntryMap;
                        Point3D maxSingleEntryLocation;
                        int processedBeforeSlowEntry;
                        int processedAfterSlowEntry;
                        long measuredMaxEntryMs;
                        long measuredMaxSpawnMs;

                        lock (m_StatsLock)
                        {
                                worstEntry = m_WorstSingleEntrySample;
                                worstSpawn = m_WorstSingleSpawnSample;
                                worstNoOp = m_WorstSlowNoOpSample;
                                worstConstructor = m_WorstConstructorSample;
                                topRepeated.CopyFrom(m_TopRepeatedSlowSpawner);
                                slowEntryYieldCount = m_SlowEntryYieldCountTotal;
                                singleSlowEntryBudgetBreaks = m_SingleSlowEntryBudgetBreaksTotal;
                                maxSingleEntryMs = m_MaxSingleEntryMsTotal;
                                maxSingleEntrySerial = m_MaxSingleEntrySerial;
                                maxSingleEntryMap = m_MaxSingleEntryMap;
                                maxSingleEntryLocation = m_MaxSingleEntryLocation;
                                processedBeforeSlowEntry = m_ProcessedBeforeSlowEntry;
                                processedAfterSlowEntry = m_ProcessedAfterSlowEntry;
                                measuredMaxEntryMs = Math.Max(m_WorstSpawnerSample.MaxMs, m_WorstSingleEntrySample.TotalMs);
                                measuredMaxSpawnMs = Math.Max(m_WorstSpawnerSample.MaxSpawnMs, m_WorstSingleSpawnSample.SpawnMs);
                        }

                        writer.WriteLine("PremiumSpawner singleEntrySummary: maxSingleEntryMs={0}, maxSingleEntrySpawnMs={1}, detailedThreshold={2}ms", measuredMaxEntryMs, measuredMaxSpawnMs, DetailedSlowThreshold);
                        writer.WriteLine(
                                "PremiumSpawner maxSingleEntry: {0}",
                                (worstEntry == null || worstEntry.TotalMs <= 0) && measuredMaxEntryMs > 0
                                        ? "No detailed slow sample retained; max came from constructor type stats"
                                        : FormatSlowSample(worstEntry));
                        writer.WriteLine("PremiumSpawner maxSingleSpawn: {0}", FormatSlowSample(worstSpawn));
                        writer.WriteLine("PremiumSpawner slowNoOp spawned=0: {0}", FormatSlowSample(worstNoOp));
                        writer.WriteLine("PremiumSpawner worstConstructor: {0}", FormatSlowSample(worstConstructor));
                        writer.WriteLine("PremiumSpawner topRepeatedSlowSpawner: {0}", FormatStat(topRepeated));
                        writer.WriteLine(
                                "PremiumSpawner singleEntryScheduler: slowEntryYieldCount={0}, singleSlowEntryBudgetBreaks={1}, maxSingleEntryMs={2}, maxSingleEntrySerial=0x{3:X}, maxSingleEntryMap={4}, maxSingleEntryLocation={5}, processedBeforeSlowEntry={6}, processedAfterSlowEntry={7}",
                                slowEntryYieldCount,
                                singleSlowEntryBudgetBreaks,
                                maxSingleEntryMs,
                                maxSingleEntrySerial,
                                maxSingleEntryMap,
                                maxSingleEntryLocation,
                                processedBeforeSlowEntry,
                                processedAfterSlowEntry);
                }

                private static void WriteSchedulerWindow(TextWriter writer, string label, SchedulerWindowStats stats)
                {
                        long schedulerAvg = stats.SchedulerTicks > 0 ? stats.SchedulerTickTotalMs / stats.SchedulerTicks : 0;
                        long spawnAvg = stats.ProcessedDue > 0 ? stats.SpawnWorkElapsedMs / stats.ProcessedDue : 0;
                        long lateAvg = stats.LatenessSamples > 0 ? stats.LatenessTotalMs / stats.LatenessSamples : 0;
                        long averageOldestReady = stats.OldestReadySamples > 0 ? stats.OldestReadyTotalMs / stats.OldestReadySamples : 0;
                        long percentTicksOverTarget = stats.SchedulerTicks > 0 ? stats.OldestReadyTicksOverTarget * 100 / stats.SchedulerTicks : 0;
                        bool oldestReadyOverTarget = stats.OldestReadyAgeMs > PremiumSpawnerTargetOldestReadyMs;
                        long oldestReadyOverTargetMs = oldestReadyOverTarget ? stats.OldestReadyAgeMs - PremiumSpawnerTargetOldestReadyMs : 0;

                        writer.WriteLine("PremiumSpawnerScheduler {0}: schedulerTicks={1}, schedulerTickLast={2}ms, schedulerTickAvg={3}ms, schedulerTickMax={4}ms, promotedDue={5}, promotedThisTick={6}, processedDue={7}, processedThisTick={8}, budgetDeferredDue={9}, newlyBudgetDeferredThisTick={10}, budgetDeferredEvents={11}, budgetDeferredSpawnerCount={12}, readyBacklogCurrent={13}, readyBacklogMax={14}, oldestReadyAgeMs={15}, oldestReadyMaxAgeMs={16}, unpromotedDueCurrent={17}, unpromotedDueMax={18}, oldestUnpromotedDueAgeMs={19}, maxPromotionDelayMs={20}, promotionBudgetHits={21}, promotionBudgetHitTotal={22}, maxReadyQueue={23}, maxFutureCount={24}, invalidRemoved={25}, duplicatePrevented={26}, rescheduled={27}, oldInternalTimerCallbacks={28}, spawnWorkTotal={29}ms, spawnWorkAvg={30}ms, spawnWorkMax={31}ms, avgLatenessMs={32}, maxLatenessMs={33}, zeroNextDelay={34}, zeroNextDelayProcessed={35}, zeroNextDelayRescheduled={36}, startupCatchupAssigned={37}, startupCatchupAvgDelayMs={38}, startupCatchupMaxDelayMs={39}, slowCalls={40}",
                                label,
                                stats.SchedulerTicks,
                                stats.SchedulerTickLastMs,
                                schedulerAvg,
                                stats.SchedulerTickMaxMs,
                                stats.PromotedDue,
                                stats.PromotedDueThisTick,
                                stats.ProcessedDue,
                                stats.ProcessedDueThisTick,
                                stats.BudgetDeferredDue,
                                stats.NewlyBudgetDeferredThisTick,
                                stats.BudgetDeferredEvents,
                                stats.BudgetDeferredSpawnerCount,
                                stats.ReadyBacklogCarriedCurrent,
                                stats.ReadyBacklogCarriedMax,
                                stats.OldestReadyAgeMs,
                                stats.OldestReadyMaxAgeMs,
                                stats.UnpromotedDueCurrent,
                                stats.UnpromotedDueMax,
                                stats.OldestUnpromotedDueAgeMs,
                                stats.MaxPromotionDelayMs,
                                stats.PromotionBudgetHitCount,
                                stats.PromotionBudgetHitTotal,
                                stats.MaxReadyQueue,
                                stats.MaxFutureCount,
                                stats.InvalidRemoved,
                                stats.DuplicatePrevented,
                                stats.Rescheduled,
                                stats.OldInternalTimerCallbacks,
                                stats.SpawnWorkElapsedMs,
                                spawnAvg,
                                stats.SpawnWorkMaxMs,
                                lateAvg,
                                stats.LatenessMaxMs,
                                stats.ZeroNextDelayCount,
                                stats.ZeroNextDelayProcessed,
                                stats.ZeroNextDelayRescheduled,
                                stats.StartupCatchupAssigned,
                                stats.StartupCatchupAssigned > 0 ? stats.StartupCatchupDelayTotalMs / stats.StartupCatchupAssigned : 0,
                                stats.StartupCatchupMaxDelayAssignedMs,
                                stats.SlowCalls);
                        writer.WriteLine("PremiumSpawner target {0}: targetOldestReadyMs={1}, warningOldestReadyMs={2}, oldestReadyMs={3}, oldestReadyOverTarget={4}, oldestReadyOverTargetMs={5}, averageOldestReadyMs={6}, maxOldestReadyMs={7}, percentTicksOverTarget={8}, readyBacklogTarget={9}, readyBacklogOverTarget={10}, readyBacklogOverTargetMax={11}, backlogDrainRatePerMinute={12}",
                                label,
                                PremiumSpawnerTargetOldestReadyMs,
                                PremiumSpawnerWarningOldestReadyMs,
                                stats.OldestReadyAgeMs,
                                oldestReadyOverTarget,
                                oldestReadyOverTargetMs,
                                averageOldestReady,
                                stats.OldestReadyMaxAgeMs,
                                percentTicksOverTarget,
                                PremiumSpawnerTargetReadyBacklog,
                                stats.ReadyBacklogOverTargetCurrent,
                                stats.ReadyBacklogOverTargetMax,
                                stats.BacklogDrainRatePerMinute);
                        writer.WriteLine("PremiumSpawner adaptive {0}: adaptiveLevelLast={1}, adaptiveLevelMax={2}, adaptiveLevelCounts=base:{3}/l1:{4}/l2:{5}/l3:{6}, adaptiveBudgetMsUsedLast={7}, adaptiveProcessLimit={8}, adaptiveBudgetLimitMs={9}, adaptivePromotionLimit={10}, adaptiveBudgetHitCount={11}, adaptiveProcessingLimitedByMsBudget={12}, adaptiveProcessingLimitedByCountBudget={13}, softLagMs={14}, hardLagMs={15}",
                                label,
                                stats.AdaptiveLevelLast,
                                stats.AdaptiveLevelMax,
                                stats.AdaptiveLevel0Count,
                                stats.AdaptiveLevel1Count,
                                stats.AdaptiveLevel2Count,
                                stats.AdaptiveLevel3Count,
                                stats.AdaptiveBudgetMsUsedLast,
                                stats.AdaptiveProcessLimitLast,
                                stats.AdaptiveBudgetLimitMsLast,
                                stats.AdaptivePromotionLimitLast,
                                stats.AdaptiveBudgetHitCount,
                                stats.AdaptiveProcessingLimitedByMsBudget,
                                stats.AdaptiveProcessingLimitedByCountBudget,
                                PremiumSpawnerSchedulerSoftLagMs,
                                PremiumSpawnerSchedulerHardLagMs);
                        writer.WriteLine("PremiumSpawner zeroDelay {0}: zeroNextDelayCount={1}, zeroNextDelayRepeatedCount={2}, zeroDelayProcessedThisWindow={3}, zeroDelayRescheduledThisWindow={4}",
                                label,
                                stats.ZeroNextDelayCount,
                                stats.ZeroNextDelayRepeatedCount,
                                stats.ZeroNextDelayProcessed,
                                stats.ZeroNextDelayRescheduled);
                        writer.WriteLine(
                                "PremiumSpawner singleEntry {0}: slowEntryYieldCount={1}, singleSlowEntryBudgetBreaks={2}, maxSingleEntryMs={3}, maxSingleEntrySerial=0x{4:X}, maxSingleEntryMap={5}, maxSingleEntryLocation={6}, processedBeforeSlowEntry={7}, processedAfterSlowEntry={8}",
                                label,
                                stats.SlowEntryYieldCount,
                                stats.SingleSlowEntryBudgetBreaks,
                                stats.MaxSingleEntryMs,
                                stats.MaxSingleEntrySerial,
                                !String.IsNullOrEmpty(stats.MaxSingleEntryMap) ? stats.MaxSingleEntryMap : "none",
                                stats.MaxSingleEntryLocation,
                                stats.ProcessedBeforeSlowEntry,
                                stats.ProcessedAfterSlowEntry);
                }

                private static void WriteStatsWindow(TextWriter writer, string label, Dictionary<int, SpawnerStat> stats)
                {
                        long count;
                        long slow;
                        long total;
                        long max;
                        long avg;
                        long deferred;

                        GetGlobalTotals(stats, out count, out slow, out total, out max, out deferred);
                        avg = count > 0 ? total / count : 0;

                        writer.WriteLine("PremiumSpawner {0}: entries={1}, calls={2}, slow={3}, budgetDeferredDue={4}, total={5}ms, avg={6}ms, max={7}ms", label, stats.Count, count, slow, deferred, total, avg, max);
                        writer.WriteLine("PremiumSpawner {0}: topTotal={1}", label, FormatStat(GetTopStat(stats, "total")));
                        writer.WriteLine("PremiumSpawner {0}: topMax={1}", label, FormatStat(GetTopStat(stats, "max")));
                        writer.WriteLine("PremiumSpawner {0}: topCount={1}", label, FormatStat(GetTopStat(stats, "count")));
                        writer.WriteLine("PremiumSpawner {0}: topBudgetDeferredDue={1}", label, FormatStat(GetTopStat(stats, "deferred")));
                        writer.WriteLine("PremiumSpawner {0}: topLate={1}", label, FormatStat(GetTopStat(stats, "late")));
                        writer.WriteLine("PremiumSpawner {0}: topZeroNextDelay={1}", label, FormatStat(GetTopStat(stats, "zeroDelay")));
                }

		public void OnTick()
		{
			if ( EnablePremiumSpawnerScheduler )
			{
				m_End = DateTime.UtcNow;
				SchedulePremiumSpawner( this, DateTime.UtcNow, "manual" );
			}
			else
			{
				OnTick(false);
			}
		}

		private void OnTick(bool fromScheduler)
		{
			long start = Stopwatch.GetTimestamp();
			SpawnCallTrace trace = BeginSpawnTrace();
			int trackedBefore = GetTrackedCreatureCount();
			int nearbyChecked = 0;
			bool regionCheck = false;
			bool deferred = false;
			int categories = GetConfiguredCategoryCount();
			long doTimerElapsed = 0;
			long regionElapsed = 0;
			long defragElapsed = 0;
			long spawnElapsed = 0;

			try
			{
				long phaseStart = Stopwatch.GetTimestamp();
				DoTimer();
				doTimerElapsed = GetElapsedMilliseconds(phaseStart);

				if (World.Saving || this.Map == null )
					return;

				phaseStart = Stopwatch.GetTimestamp();
				String reg = Worlds.GetMyWorld( this.Map, this.Location, this.X, this.Y );

				if ( trace != null )
					trace.RegionName = !String.IsNullOrEmpty( reg ) ? reg : "none";

				if (reg != null && AdventuresFunctions.RegionIsInfected( reg ) )
				{
					regionCheck = true;
					bool infected = false;

					foreach ( Mobile m in this.GetMobilesInRange( 30 ) )
					{
						++nearbyChecked;

						if (m is BaseCreature)
						{
							if ( ((BaseCreature)m).CanInfect && !infected)
								infected = true;
						}
					}
					if (infected)
						return;
				}
				regionElapsed = GetElapsedMilliseconds(phaseStart);

				if ( m_Group )
				{
					phaseStart = Stopwatch.GetTimestamp();
					Defrag();
					defragElapsed = GetElapsedMilliseconds(phaseStart);

					if  ( m_Creatures.Count == 0 || m_CreaturesA.Count == 0 || m_CreaturesB.Count == 0 || m_CreaturesC.Count == 0 || m_CreaturesD.Count == 0 || m_CreaturesE.Count == 0 )
					{
						phaseStart = Stopwatch.GetTimestamp();
						Respawn();
						spawnElapsed = GetElapsedMilliseconds(phaseStart);
					}
					else
					{
						return;
					}
				}
				else
				{
					phaseStart = Stopwatch.GetTimestamp();
					Spawn();
					SpawnA();
					SpawnB();
					SpawnC();
					SpawnD();
					SpawnE();
					spawnElapsed = GetElapsedMilliseconds(phaseStart);
				}
			}
			catch ( Exception ex )
			{
				if ( trace != null )
					trace.Error = ex.GetType().Name + ": " + ex.Message;

				throw;
			}
			finally
			{
				long elapsed = GetElapsedMilliseconds(start);
				int trackedAfter = GetTrackedCreatureCount();
				int spawned = trackedAfter > trackedBefore ? trackedAfter - trackedBefore : 0;
				TimeSpan nextDelay = m_End > DateTime.UtcNow ? m_End - DateTime.UtcNow : TimeSpan.Zero;

				RecordStats(this, elapsed, doTimerElapsed, regionElapsed, defragElapsed, spawnElapsed, trackedBefore, trackedAfter, categories, spawned, nearbyChecked, deferred, fromScheduler, nextDelay, trace);

				if (!deferred && elapsed >= SlowLogThreshold)
				{
					long now = Core.TickCount;

					if (now >= m_NextSlowLog)
					{
						m_NextSlowLog = now + SlowLogInterval;

						DiagnosticConsole.WriteParsedLine(String.Format("[Lag] PremiumSpawner 0x{0:X} at {1}/{2} took {3}ms; doTimer={4}ms region={5}ms defrag={6}ms spawn={7}ms tracked={8}, trackedBefore={9}, trackedAfter={10}, trackedDelta={11}, spawned={12}, categories={13}, nearbyChecked={14}, regionCheck={15}, nextDelay={16}, breakdown={17}", Serial.Value, Map, Location, elapsed, doTimerElapsed, regionElapsed, defragElapsed, spawnElapsed, trackedAfter, trackedBefore, trackedAfter, trackedAfter - trackedBefore, spawned, categories, nearbyChecked, regionCheck, nextDelay, FormatSpawnBreakdown(trace)));
					}
				}

				EndSpawnTrace( trace );
			}
		}

		public static void Reconfigure( PremiumSpawner spawner, bool respawn )
		{
			if ( spawner.SpawnID == 8888 )
			{
				spawner.Count = ( int )( spawner.CountE * 0.15 );
					if ( Utility.RandomMinMax( 1, 10 ) == 1 ){ spawner.Count = ( int )( spawner.CountE * 0.20 ); }
					if ( spawner.Count < 1 ){ spawner.Count = 1; }

				spawner.CountA = ( int )( spawner.CountE * 0.12 );
					if ( spawner.CountA < 1 && Utility.RandomMinMax( 1, 5 ) == 1 ){ spawner.CountA = 1; }

				spawner.CountB = ( int )( spawner.CountE * 0.09 );
					if ( spawner.CountB < 1 && Utility.RandomMinMax( 1, 5 ) == 1 ){ spawner.CountB = 1; }

				spawner.CountC = ( int )( spawner.CountE * 0.06 );
					if ( spawner.CountC < 1 && Utility.RandomMinMax( 1, 5 ) == 1 ){ spawner.CountC = 1; }

				spawner.CountD = ( int )( spawner.CountE * 0.03 );
					if ( spawner.CountD < 1 && Utility.RandomMinMax( 1, 5 ) == 1 ){ spawner.CountD = 1; }

				if ( respawn ){ spawner.Respawn(); }
				else
				{
					spawner.RemoveCreatures();
					spawner.RemoveCreaturesA();
					spawner.RemoveCreaturesB();
					spawner.RemoveCreaturesC();
					spawner.RemoveCreaturesD();
					spawner.RemoveCreaturesE();
				}
			}
		}

		public static void SpreadOut( Mobile m )
		{
						if ( m is BaseVendor || m is Adventurers || m is Jedi || m is Syth )
			{
				///////////////// SPREAD WANDERING HEALERS AROUND THE LAND /////////////////
				if ( m.X >= 0 && m.Y >= 0 && m.X <= 6 && m.Y <= 6 && m.Map == Map.Felucca ){ m.Location = Worlds.GetRandomLocation( "the Land of Lodoria", "land" ); m.WhisperHue = 911; }
				else if ( m.X >= 0 && m.Y >= 0 && m.X <= 6 && m.Y <= 6 && m.Map == Map.Trammel ){ m.Location = Worlds.GetRandomLocation( "the Land of Sosaria", "land" ); m.WhisperHue = 911; }
				else if ( m.X >= 0 && m.Y >= 0 && m.X <= 6 && m.Y <= 6 && m.Map == Map.Malas ){ m.Location = Worlds.GetRandomLocation( "the Serpent Island", "land" ); m.WhisperHue = 911; }
				else if ( m.X >= 0 && m.Y >= 0 && m.X <= 6 && m.Y <= 6 && m.Map == Map.Tokuno ){ m.Location = Worlds.GetRandomLocation( "the Isles of Dread", "land" ); m.WhisperHue = 911; }
				else if ( m.X >= 1125 && m.Y >= 298 && m.X <= 1131 && m.Y <= 305 && m.Map == Map.TerMur ){ m.Location = Worlds.GetRandomLocation( "the Savaged Empire", "land" ); m.WhisperHue = 911; }
				else if ( m.X >= 5457 && m.Y >= 3300 && m.X <= 5459 && m.Y <= 3302 && m.Map == Map.Trammel ){ m.Location = Worlds.GetRandomLocation( "the Land of Ambrosia", "land" ); m.WhisperHue = 911; }
				else if ( m.X >= 608 && m.Y >= 4090 && m.X <= 704 && m.Y <= 4096 && m.Map == Map.Trammel ){ m.Location = Worlds.GetRandomLocation( "the Island of Umber Veil", "land" ); m.WhisperHue = 911; }
				else if ( m.X >= 6126 && m.Y >= 827 && m.X <= 6132 && m.Y <= 833 && m.Map == Map.Trammel ){ m.Location = Worlds.GetRandomLocation( "the Bottle World of Kuldar", "land" ); m.WhisperHue = 911; }
				else if ( m.X == 3 && m.Y == 3 && m.Map == Map.Ilshenar ){ m.Location = Worlds.GetRandomLocation( "the Underworld", "land" ); m.WhisperHue = 911; }
				else if ( m.X == 4 && m.Y == 4 && m.Map == Map.Ilshenar ){ m.Location = Worlds.GetRandomLocation( "the Underworld", "land" ); m.WhisperHue = 911; }
			}
			else
			{
				///////////////// SPREAD SEA SPAWNS OVER THE OCEANS AROUND THE LAND /////////////////
				if ( m.X >= 0 && m.Y >= 0 && m.X <= 6 && m.Y <= 6 && m.Map == Map.Felucca ){ m.Location = Worlds.GetRandomLocation( "the Land of Lodoria", "sea" ); m.WhisperHue = 999; }
				else if ( m.X >= 0 && m.Y >= 0 && m.X <= 6 && m.Y <= 6 && m.Map == Map.Trammel ){ m.Location = Worlds.GetRandomLocation( "the Land of Sosaria", "sea" ); m.WhisperHue = 999; }
				else if ( m.X >= 0 && m.Y >= 0 && m.X <= 6 && m.Y <= 6 && m.Map == Map.Malas ){ m.Location = Worlds.GetRandomLocation( "the Serpent Island", "sea" ); m.WhisperHue = 999; }
				else if ( m.X >= 0 && m.Y >= 0 && m.X <= 6 && m.Y <= 6 && m.Map == Map.Tokuno ){ m.Location = Worlds.GetRandomLocation( "the Isles of Dread", "sea" ); m.WhisperHue = 999; }
				else if ( m.X >= 1125 && m.Y >= 298 && m.X <= 1131 && m.Y <= 305 && m.Map == Map.TerMur ){ m.Location = Worlds.GetRandomLocation( "the Savaged Empire", "sea" ); m.WhisperHue = 999; }
				else if ( m.X >= 5457 && m.Y >= 3300 && m.X <= 5459 && m.Y <= 3302 && m.Map == Map.Trammel ){ m.Location = Worlds.GetRandomLocation( "the Land of Ambrosia", "sea" ); m.WhisperHue = 999; }
				else if ( m.X >= 608 && m.Y >= 4090 && m.X <= 704 && m.Y <= 4096 && m.Map == Map.Trammel ){ m.Location = Worlds.GetRandomLocation( "the Island of Umber Veil", "sea" ); m.WhisperHue = 999; }
				else if ( m.X >= 6126 && m.Y >= 827 && m.X <= 6132 && m.Y <= 833 && m.Map == Map.Trammel ){ m.Location = Worlds.GetRandomLocation( "the Bottle World of Kuldar", "sea" ); m.WhisperHue = 999; }
				else if ( m.X == 3 && m.Y == 3 && m.Map == Map.Ilshenar ){ m.Location = Worlds.GetRandomLocation( "the Underworld", "sea" ); m.WhisperHue = 999; }
				else if ( m.X == 4 && m.Y == 4 && m.Map == Map.Ilshenar ){ m.Location = Worlds.GetRandomLocation( "the Underworld", "land" ); m.WhisperHue = 911; }

				if ( m.WhisperHue == 999 && ( m is Balron || m is Daemon || m is Dragons || m is Wyrms ) ){ m.CanSwim = true; }
			}
		}

		public static void SpreadItems( Item i )
		{
			if ( i is LandChest || i is StrangePortal )
			{
				///////////////// SPREAD CORPSES AND PORTALS AROUND THE LAND /////////////////
				if ( i.X >= 0 && i.Y >= 0 && i.X <= 6 && i.Y <= 6 && i.Map == Map.Felucca ){ i.Location = Worlds.GetRandomLocation( "the Land of Lodoria", "land" ); }
				else if ( i.X >= 0 && i.Y >= 0 && i.X <= 6 && i.Y <= 6 && i.Map == Map.Trammel ){ i.Location = Worlds.GetRandomLocation( "the Land of Sosaria", "land" ); }
				else if ( i.X >= 0 && i.Y >= 0 && i.X <= 6 && i.Y <= 6 && i.Map == Map.Malas ){ i.Location = Worlds.GetRandomLocation( "the Serpent Island", "land" ); }
				else if ( i.X >= 0 && i.Y >= 0 && i.X <= 6 && i.Y <= 6 && i.Map == Map.Tokuno ){ i.Location = Worlds.GetRandomLocation( "the Isles of Dread", "land" ); }
				else if ( i.X >= 1125 && i.Y >= 298 && i.X <= 1131 && i.Y <= 305 && i.Map == Map.TerMur ){ i.Location = Worlds.GetRandomLocation( "the Savaged Empire", "land" ); }
				else if ( i.X >= 5457 && i.Y >= 3300 && i.X <= 5459 && i.Y <= 3302 && i.Map == Map.Trammel ){ i.Location = Worlds.GetRandomLocation( "the Land of Ambrosia", "land" ); }
				else if ( i.X >= 608 && i.Y >= 4090 && i.X <= 704 && i.Y <= 4096 && i.Map == Map.Trammel ){ i.Location = Worlds.GetRandomLocation( "the Island of Umber Veil", "land" ); }
				else if ( i.X == 4 && i.Y == 4 && i.Map == Map.Ilshenar ){ i.Location = Worlds.GetRandomLocation( "the Underworld", "land" ); }
			}
			else if ( i is WaterChest )
			{
				///////////////// SPREAD BOATS OVER THE OCEANS /////////////////
				if ( i.X >= 0 && i.Y >= 0 && i.X <= 6 && i.Y <= 6 && i.Map == Map.Felucca ){ i.Location = Worlds.GetRandomLocation( "the Land of Lodoria", "sea" ); }
				else if ( i.X >= 0 && i.Y >= 0 && i.X <= 6 && i.Y <= 6 && i.Map == Map.Trammel ){ i.Location = Worlds.GetRandomLocation( "the Land of Sosaria", "sea" ); }
				else if ( i.X >= 0 && i.Y >= 0 && i.X <= 6 && i.Y <= 6 && i.Map == Map.Malas ){ i.Location = Worlds.GetRandomLocation( "the Serpent Island", "sea" ); }
				else if ( i.X >= 0 && i.Y >= 0 && i.X <= 6 && i.Y <= 6 && i.Map == Map.Tokuno ){ i.Location = Worlds.GetRandomLocation( "the Isles of Dread", "sea" ); }
				else if ( i.X >= 1125 && i.Y >= 298 && i.X <= 1131 && i.Y <= 305 && i.Map == Map.TerMur ){ i.Location = Worlds.GetRandomLocation( "the Savaged Empire", "sea" ); }
				else if ( i.X >= 5457 && i.Y >= 3300 && i.X <= 5459 && i.Y <= 3302 && i.Map == Map.Trammel ){ i.Location = Worlds.GetRandomLocation( "the Land of Ambrosia", "sea" ); }
				else if ( i.X >= 608 && i.Y >= 4090 && i.X <= 704 && i.Y <= 4096 && i.Map == Map.Trammel ){ i.Location = Worlds.GetRandomLocation( "the Island of Umber Veil", "sea" ); }
				else if ( i.X >= 6126 && i.Y >= 827 && i.X <= 6132 && i.Y <= 833 && i.Map == Map.Trammel ){ i.Location = Worlds.GetRandomLocation( "the Bottle World of Kuldar", "sea" ); }
				else if ( i.X == 3 && i.Y == 3 && i.Map == Map.Ilshenar ){ i.Location = Worlds.GetRandomLocation( "the Underworld", "sea" ); }
			}
		}
		
		public void Respawn() // remove all creatures and spawn all again
		{
			RemoveCreatures();
			RemoveCreaturesA();
			RemoveCreaturesB();
			RemoveCreaturesC();
			RemoveCreaturesD();
			RemoveCreaturesE();

			for ( int i = 0; i < m_Count; i++ )
				Spawn();
			for ( int i = 0; i < m_CountA; i++ )
				SpawnA();
			for ( int i = 0; i < m_CountB; i++ )
				SpawnB();
			for ( int i = 0; i < m_CountC; i++ )
				SpawnC();
			for ( int i = 0; i < m_CountD; i++ )
				SpawnD();
			for ( int i = 0; i < m_CountE; i++ )
				SpawnE();
		}
		
		public void Spawn()
		{
			if ( CreaturesNameCount > 0 )
			{
				long started = BeginCategoryTrace( "primary" );

				try
				{
					Spawn( Utility.Random( CreaturesNameCount ) );
				}
				finally
				{
					EndCategoryTrace( "primary", started );
				}
			}
		}

		public void SpawnA()
		{
			if ( CreaturesNameCountA > 0 )
			{
				long started = BeginCategoryTrace( "A" );

				try
				{
					SpawnA( Utility.Random( CreaturesNameCountA ) );
				}
				finally
				{
					EndCategoryTrace( "A", started );
				}
			}
		}

		public void SpawnB()
		{
			if ( CreaturesNameCountB > 0 )
			{
				long started = BeginCategoryTrace( "B" );

				try
				{
					SpawnB( Utility.Random( CreaturesNameCountB ) );
				}
				finally
				{
					EndCategoryTrace( "B", started );
				}
			}
		}

		public void SpawnC()
		{
			if ( CreaturesNameCountC > 0 )
			{
				long started = BeginCategoryTrace( "C" );

				try
				{
					SpawnC( Utility.Random( CreaturesNameCountC ) );
				}
				finally
				{
					EndCategoryTrace( "C", started );
				}
			}
		}

		public void SpawnD()
		{
			if ( CreaturesNameCountD > 0 )
			{
				long started = BeginCategoryTrace( "D" );

				try
				{
					SpawnD( Utility.Random( CreaturesNameCountD ) );
				}
				finally
				{
					EndCategoryTrace( "D", started );
				}
			}
		}

		public void SpawnE()
		{
			if ( CreaturesNameCountE > 0 )
			{
				long started = BeginCategoryTrace( "E" );

				try
				{
					SpawnE( Utility.Random( CreaturesNameCountE ) );
				}
				finally
				{
					EndCategoryTrace( "E", started );
				}
			}
		}

		public void Spawn( string creatureName )
		{
			for ( int i = 0; i < m_CreaturesName.Count; i++ )
			{
				if ( m_CreaturesName[i] == creatureName )
				{
					Spawn( i );
					break;
				}
			}
		}

		public void SpawnA( string creatureNameA )
		{
			for ( int i = 0; i < m_CreaturesNameA.Count; i++ )
			{
				if ( (string)m_CreaturesNameA[i] == creatureNameA )
				{
					SpawnA( i );
					break;
				}
			}
		}

		public void SpawnB( string creatureNameB )
		{
			for ( int i = 0; i < m_CreaturesNameB.Count; i++ )
			{
				if ( (string)m_CreaturesNameB[i] == creatureNameB )
				{
					SpawnB( i );
					break;
				}
			}
		}

		public void SpawnC( string creatureNameC )
		{
			for ( int i = 0; i < m_CreaturesNameC.Count; i++ )
			{
				if ( (string)m_CreaturesNameC[i] == creatureNameC )
				{
					SpawnC( i );
					break;
				}
			}
		}

		public void SpawnD( string creatureNameD )
		{
			for ( int i = 0; i < m_CreaturesNameD.Count; i++ )
			{
				if ( (string)m_CreaturesNameD[i] == creatureNameD )
				{
					SpawnD( i );
					break;
				}
			}
		}

		public void SpawnE( string creatureNameE )
		{
			for ( int i = 0; i < m_CreaturesNameE.Count; i++ )
			{
				if ( (string)m_CreaturesNameE[i] == creatureNameE )
				{
					SpawnE( i );
					break;
				}
			}
		}


		private string[] ResolveSpawnArgs( string spawnDef )
		{
			string[] args = CommandSystem.Split( spawnDef );
			string baseName = ParseType( spawnDef );
			Type baseType = ScriptCompiler.FindTypeByName( baseName );

			if ( Utility.RandomMinMax( 1, 4 ) == 1 )
			{
				if ( baseType == typeof( Dragons ) || Insensitive.Equals( baseName, "Dragon" ) || Insensitive.Equals( baseName, "Dragons" ) )
					args[0] = "RidingDragon";
				else if ( baseType == typeof( WhiteTiger ) || Insensitive.Equals( baseName, "WhiteTiger" ) )
					args[0] = "WhiteTigerRiding";
			}

			return args;
		}

		protected virtual IEntity CreateSpawnedObject( int index )
		{
			return CreateSpawnedObject( m_CreaturesName, index );
		}

		protected virtual IEntity CreateSpawnedObjectA( int index )
		{
			return CreateSpawnedObject( m_CreaturesNameA, index );
		}

		protected virtual IEntity CreateSpawnedObjectB( int index )
		{
			return CreateSpawnedObject( m_CreaturesNameB, index );
		}

		protected virtual IEntity CreateSpawnedObjectC( int index )
		{
			return CreateSpawnedObject( m_CreaturesNameC, index );
		}

		protected virtual IEntity CreateSpawnedObjectD( int index )
		{
			return CreateSpawnedObject( m_CreaturesNameD, index );
		}

		protected virtual IEntity CreateSpawnedObjectE( int index )
		{
			return CreateSpawnedObject( m_CreaturesNameE, index );
		}

		private IEntity CreateSpawnedObject( List<string> names, int index )
		{
			if ( names == null || index < 0 || index >= names.Count )
				return null;

			SpawnCallTrace trace = GetOwnedSpawnTrace();
			string[] spawnArgs;
			long started = trace != null ? Stopwatch.GetTimestamp() : 0;

			try
			{
				spawnArgs = ResolveSpawnArgs( names[index] );
			}
			catch ( Exception ex )
			{
				if ( trace != null )
					trace.Error = ex.GetType().Name + ": " + ex.Message;

				return null;
			}

			if ( trace != null )
			{
				trace.ArgumentParseMs += GetElapsedMilliseconds( started );
				trace.SelectedType = spawnArgs != null && spawnArgs.Length > 0 ? spawnArgs[0] : "none";
				++trace.CandidateTypesConsidered;
			}

			if ( spawnArgs == null || spawnArgs.Length == 0 )
				return null;

			started = trace != null ? Stopwatch.GetTimestamp() : 0;
			Type type = ScriptCompiler.FindTypeByName( spawnArgs[0] );

			if ( trace != null )
				trace.TypeLookupMs += GetElapsedMilliseconds( started );

			if ( type == null )
				return null;

			try
			{
				return Build( spawnArgs, type );
			}
			catch ( Exception ex )
			{
				if ( trace != null )
					trace.Error = ex.GetType().Name + ": " + ex.Message;
			}

			return null;
		}

		public static IEntity Build( string[] args )
		{
			if ( args == null || args.Length == 0 )
				return null;

			Type type = ScriptCompiler.FindTypeByName( args[0] );
			return Build( args, type );
		}

		private static IEntity Build( string[] args, Type type )
		{
			if ( args == null || args.Length == 0 || type == null )
				return null;

			SpawnCallTrace trace = GetSpawnTrace();
			long reflectionStart = trace != null ? Stopwatch.GetTimestamp() : 0;
			Add.FixArgs( ref args );

			string[,] props = null;

			for ( int i = 0; i < args.Length; ++i )
			{
				if ( Insensitive.Equals( args[i], "set" ) )
				{
					int remains = args.Length - i - 1;

					if ( remains >= 2 )
					{
						props = new string[remains / 2, 2];

						remains /= 2;

						for ( int j = 0; j < remains; ++j )
						{
							props[j, 0] = args[i + (j * 2) + 1];
							props[j, 1] = args[i + (j * 2) + 2];
						}

						Add.FixSetString( ref args, i );
					}

					break;
				}
			}

			if ( !Add.IsEntity( type ) ) {
				return null;
			}

			PropertyInfo[] realProps = null;

			if ( props != null )
			{
				realProps = new PropertyInfo[props.GetLength( 0 )];

				PropertyInfo[] allProps = type.GetProperties( BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public );

				for ( int i = 0; i < realProps.Length; ++i )
				{
					PropertyInfo thisProp = null;

					string propName = props[i, 0];

					for ( int j = 0; thisProp == null && j < allProps.Length; ++j )
					{
						if ( Insensitive.Equals( propName, allProps[j].Name ) )
							thisProp = allProps[j];
					}

					if ( thisProp != null )
					{
						CPA attr = Properties.GetCPA( thisProp );

						if ( attr != null && AccessLevel.GameMaster >= attr.WriteLevel && thisProp.CanWrite && !attr.ReadOnly )
							realProps[i] = thisProp;
					}
				}
			}

			ConstructorInfo[] ctors = type.GetConstructors();

			if ( trace != null )
				trace.ReflectionSetupMs += GetElapsedMilliseconds( reflectionStart );

			for ( int i = 0; i < ctors.Length; ++i )
			{
				ConstructorInfo ctor = ctors[i];

				if ( !Add.IsConstructable( ctor, AccessLevel.GameMaster ) )
					continue;

				ParameterInfo[] paramList = ctor.GetParameters();

				if ( args.Length == paramList.Length )
				{
					object[] paramValues = Add.ParseValues( paramList, args );

					if ( paramValues == null )
						continue;

					long constructorStart = trace != null ? Stopwatch.GetTimestamp() : 0;
					object built = null;
					bool citizenTraceStarted = trace != null
						&& typeof( Citizens ).IsAssignableFrom( type )
						&& CitizenConstructorDiagnostics.Begin( type.FullName, "PremiumSpawner" );

					try
					{
						built = ctor.Invoke( paramValues );
					}
					finally
					{
						if ( trace != null )
						{
							long constructorElapsed = GetElapsedMilliseconds( constructorStart );
							trace.ConstructorMs += constructorElapsed;

							if ( constructorElapsed > trace.ConstructorMaxMs )
							{
								trace.ConstructorMaxMs = constructorElapsed;
								trace.WorstConstructorType = type.FullName;
							}

							CitizenConstructorSample citizenTrace = citizenTraceStarted
								? CitizenConstructorDiagnostics.End( constructorElapsed, built != null ? "none" : "constructor failed" )
								: new CitizenConstructorSample();
							TraceConstructorInvocation( type, constructorElapsed, built != null, citizenTrace );
						}
						else if ( citizenTraceStarted )
						{
							CitizenConstructorDiagnostics.Abandon();
						}
					}

					if ( built != null && realProps != null )
					{
						long propertyStart = trace != null ? Stopwatch.GetTimestamp() : 0;

						for ( int j = 0; j < realProps.Length; ++j )
						{
							if ( realProps[j] == null )
								continue;

							string result = Properties.InternalSetValue( built, realProps[j], props[j, 1] );
						}

						if ( trace != null )
							trace.ReflectionSetupMs += GetElapsedMilliseconds( propertyStart );
					}

					if ( trace != null && built != null )
						trace.ConstructedType = built.GetType().FullName;

					return (IEntity)built;
				}
			}

			return null;
		}

		public void Spawn( int index )
		{
			Map map = Map;

			if ( map == null || map == Map.Internal || CreaturesNameCount == 0 || index >= CreaturesNameCount || Parent != null )
				return;

			DefragWithTrace();

			if ( m_Creatures.Count >= m_Count )
				return;

			int amount = 1;

			if (m_Creatures.Count == (m_Count-1) || m_Count == 1)
			{
				Region reg = Region.Find( this.Location, this.Map );
				if ( (reg.IsPartOf( typeof(BardDungeonRegion) ) || reg.IsPartOf( typeof( DungeonRegion ) ) ) && Utility.RandomDouble() > 0.93 )
				{
					double bleh = Utility.RandomDouble();
					if (m_Count <= 4)
					{
						if (bleh >= 0.85)
							amount += Utility.RandomMinMax(2,4);
						else
							amount += Utility.RandomMinMax(1,3);
					}
					else
					{
							amount += Utility.RandomMinMax(1,2);
					}

					amount = ClampSpawns(m_Creatures, amount);
				}
			}

			while (amount > 0)
			{
				amount -= 1;

				TraceSpawnAttempt();

				IEntity ent = CreateSpawnedObject( index );

				if ( ent is Mobile )
				{
					Mobile m = (Mobile)ent;

					if ( ( m is Mangar || m is Exodus || m is Surtaz || m is Vulcrum || m is Arachnar || m is CaddelliteDragon || (m is BaseCreature && ( ((BaseCreature)m).HitsMaxSeed > 1000 || m.Fame > 15000 ) ) ) && amount > 0)
						amount = 0;

					m_Creatures.Add( m );
					TraceAcceptedIntoSpawnerList();
					
					Point3D loc = ( m is BaseVendor ? this.Location : GetSpawnPositionWithTrace() );
					
									if ( m is WanderingHealer || m is Adventurers || m is Jedi )
					{
						loc = GetSpawnPositionWithTrace();
					}
					else if ( m is Syth && SpawnID == 9999 )
					{
						loc = GetSpawnSpotLandDungeonWithTrace();
					}

                                        OnBeforeSpawnWithTrace( m, loc, map );
                                        InvalidateProperties();

                                        MoveToWorldWithTrace( m, loc, map );

                                        m.Direction = m_SpawnDirection;

                                        if ( m is BaseCreature )
                                        {
                                                BaseCreature c = (BaseCreature)m;
						
						if( m_WalkingRange >= 0 )
							c.RangeHome = m_WalkingRange;
						else
							c.RangeHome = m_HomeRange;

						c.CurrentWayPoint = m_WayPoint;

						if ( m_Team > 0 )
							c.Team = m_Team;

						c.Home = loc;

						Server.Misc.MorphingTime.SetGender( m );

						if ( IsOutsideSpawnerRegionWithTrace( m ) ){ DeleteRejectedByRegionWithTrace( m ); DefragWithTrace(); return; } // REMOVE IF NOT IN SAME REGION
						c.SpawnerSpawned = true;
					}

					OnAfterSpawnWithTrace( m );

					if (m != null && m.Map != null)
					{
						//check for bugged enemies spawning at 0,0,0
						Point3D loccheck = m.Location;
						if (loccheck.X == 0 && loccheck.Y == 0)
						{
							Console.WriteLine( m + " spawned at 0,0 at map " + m.Map + " from spawner at " + this.Location );
						}
					}
				}
				else if ( ent is Item && m_Creatures.Count < m_Count )
				{
					Item item = (Item)ent;

					m_Creatures.Add( item );
					TraceAcceptedIntoSpawnerList();

					Point3D loc = GetSpawnPositionWithTrace();

					OnBeforeSpawnWithTrace( item, loc, map );
					InvalidateProperties();

					MoveToWorldWithTrace( item, loc, map );

					if ( IsOutsideSpawnerRegionWithTrace( item ) ){ DeleteRejectedByRegionWithTrace( item ); DefragWithTrace(); return; } // REMOVE IF NOT IN SAME REGION

					OnAfterSpawnWithTrace( item );
				}
				else
				{
					TraceUnacceptedOrNoObject();
				}
			}
		}

		public void SpawnA( int index )
		{
			Map map = Map;

			if ( map == null || map == Map.Internal || CreaturesNameCountA == 0 || index >= CreaturesNameCountA || Parent != null )
				return;

			DefragWithTrace();

			if ( m_CreaturesA.Count >= m_CountA )
				return;

			int amount = 1;

			if (m_CreaturesA.Count == (m_CountA-1) || m_CountA == 1)
			{
				Region reg = Region.Find( this.Location, this.Map );
				if ( (reg.IsPartOf( typeof( BardDungeonRegion ) ) || reg.IsPartOf( typeof( DungeonRegion ) ) ) && Utility.RandomDouble() > 0.93 )
				{
					double bleh = Utility.RandomDouble();
					if (m_CountA <= 4)
					{
						if (bleh >= 0.85)
							amount += Utility.RandomMinMax(2,3);
						else
							amount += Utility.RandomMinMax(1,2);
					}
					else
					{
							amount += Utility.RandomMinMax(1,2);
					}

					amount = ClampSpawns(m_CreaturesA, amount);
				}
			}

			while (amount > 0)
			{
				amount -= 1;

				TraceSpawnAttempt();

				IEntity ent = CreateSpawnedObjectA( index );

				if ( ent is Mobile )
				{
					Mobile m = (Mobile)ent;

					if ( ( m is Mangar || m is Exodus || m is Surtaz || m is Vulcrum || m is Arachnar || m is CaddelliteDragon || (m is BaseCreature && ( ((BaseCreature)m).HitsMaxSeed > 1000 || m.Fame > 15000 ) ) ) && amount > 0)
						amount = 0;

					m_CreaturesA.Add( m );
					TraceAcceptedIntoSpawnerList();
					

					Point3D loc = ( m is BaseVendor ? this.Location : GetSpawnPositionWithTrace() );
					
					if ( m is WanderingHealer || m is Adventurers || m is Jedi )
					{
						loc = GetSpawnPositionWithTrace();
					}
					else if ( m is Syth && SpawnID == 9999 )
					{
						loc = GetSpawnSpotLandDungeonWithTrace();
					}

                                        OnBeforeSpawnWithTrace( m, loc, map );
                                        InvalidateProperties();

                                        MoveToWorldWithTrace( m, loc, map );

                                        m.Direction = m_SpawnDirection;

                                        if ( m is BaseCreature )
                                        {
                                                BaseCreature c = (BaseCreature)m;
						
						if( m_WalkingRange >= 0 )
							c.RangeHome = m_WalkingRange;
						else
							c.RangeHome = m_HomeRange;

						c.CurrentWayPoint = m_WayPoint;

						if ( m_Team > 0 )
							c.Team = m_Team;

						c.Home = loc;

						Server.Misc.MorphingTime.SetGender( m );

						if ( IsOutsideSpawnerRegionWithTrace( m ) ){ DeleteRejectedByRegionWithTrace( m ); DefragWithTrace(); return; } // REMOVE IF NOT IN SAME REGION
						c.SpawnerSpawned = true;
					}
					
					OnAfterSpawnWithTrace( m );

					if (m != null && m.Map != null)
					{
						//check for bugged enemies spawning at 0,0,0
						Point3D loccheck = m.Location;
						if (loccheck.X == 0 && loccheck.Y == 0)
						{
							Console.WriteLine( m + " spawned at 0,0 at map " + m.Map + " from spawner at " + this.Location );
						}
					}
				}
				else if ( ent is Item && m_CreaturesA.Count < m_CountA )
				{
					Item item = (Item)ent;

					m_CreaturesA.Add( item );
					TraceAcceptedIntoSpawnerList();

					Point3D loc = GetSpawnPositionWithTrace();

					OnBeforeSpawnWithTrace( item, loc, map );
					InvalidateProperties();

					MoveToWorldWithTrace( item, loc, map );

					if ( IsOutsideSpawnerRegionWithTrace( item ) ){ DeleteRejectedByRegionWithTrace( item ); DefragWithTrace(); return; } // REMOVE IF NOT IN SAME REGION

					OnAfterSpawnWithTrace( item );
				}
				else
				{
					TraceUnacceptedOrNoObject();
				}
			}
		}

        public void SpawnB( int index )
		{
			Map map = Map;

			if ( map == null || map == Map.Internal || CreaturesNameCountB == 0 || index >= CreaturesNameCountB || Parent != null )
				return;

			DefragWithTrace();

			if ( m_CreaturesB.Count >= m_CountB )
				return;

			int amount = 1;

			if (m_CreaturesB.Count == (m_CountB-1) || m_CountB == 1)
			{
				Region reg = Region.Find( this.Location, this.Map );
				if ( ( reg.IsPartOf( typeof( BardDungeonRegion ) ) || reg.IsPartOf( typeof( DungeonRegion ) ) ) && Utility.RandomDouble() > 0.93 )
				{
					double bleh = Utility.RandomDouble();
					if (m_CountB <= 4)
					{
						if (bleh >= 0.85)
							amount += Utility.RandomMinMax(2,3);
						else
							amount += Utility.RandomMinMax(1,2);
					}
					else
					{
							amount += Utility.RandomMinMax(1,2);
					}

					amount = ClampSpawns(m_CreaturesB, amount);
				}
			}

			while (amount > 0)
			{
				amount -= 1;

				TraceSpawnAttempt();

				IEntity ent = CreateSpawnedObjectB( index );

				if ( ent is Mobile )
				{
					Mobile m = (Mobile)ent;

					m_CreaturesB.Add( m );
					TraceAcceptedIntoSpawnerList();
					
					if ( ( m is Mangar || m is Exodus || m is Surtaz || m is Vulcrum || m is Arachnar || m is CaddelliteDragon || (m is BaseCreature && ( ((BaseCreature)m).HitsMaxSeed > 1000 || m.Fame > 15000 ) ) ) && amount > 0)
						amount = 0;

					Point3D loc = ( m is BaseVendor ? this.Location : GetSpawnPositionWithTrace() );
					
									if ( m is WanderingHealer || m is Adventurers || m is Jedi )
					{
						loc = GetSpawnPositionWithTrace();
					}
					else if ( m is Syth && SpawnID == 9999 )
					{
						loc = GetSpawnSpotLandDungeonWithTrace();
					}

                                        OnBeforeSpawnWithTrace( m, loc, map );
                                        InvalidateProperties();

                                        MoveToWorldWithTrace( m, loc, map );

                                        m.Direction = m_SpawnDirection;

                                        if ( m is BaseCreature )
                                        {
                                                BaseCreature c = (BaseCreature)m;
						
						if( m_WalkingRange >= 0 )
							c.RangeHome = m_WalkingRange;
						else
							c.RangeHome = m_HomeRange;

						c.CurrentWayPoint = m_WayPoint;

						if ( m_Team > 0 )
							c.Team = m_Team;

						c.Home = loc;

						Server.Misc.MorphingTime.SetGender( m );

						if ( IsOutsideSpawnerRegionWithTrace( m ) ){ DeleteRejectedByRegionWithTrace( m ); DefragWithTrace(); return; } // REMOVE IF NOT IN SAME REGION
						c.SpawnerSpawned = true;
					}

					OnAfterSpawnWithTrace( m );

					if (m != null && m.Map != null)
					{
						//check for bugged enemies spawning at 0,0,0
						Point3D loccheck = m.Location;
						if (loccheck.X == 0 && loccheck.Y == 0)
						{
							Console.WriteLine( m + " spawned at 0,0 at map " + m.Map + " from spawner at " + this.Location );
						}
					}
				}
				else if ( ent is Item && m_CreaturesB.Count < m_CountB )
				{
					Item item = (Item)ent;

					m_CreaturesB.Add( item );
					TraceAcceptedIntoSpawnerList();

					Point3D loc = GetSpawnPositionWithTrace();

					OnBeforeSpawnWithTrace( item, loc, map );
					InvalidateProperties();

					MoveToWorldWithTrace( item, loc, map );

					if ( IsOutsideSpawnerRegionWithTrace( item ) ){ DeleteRejectedByRegionWithTrace( item ); DefragWithTrace(); return; } // REMOVE IF NOT IN SAME REGION

					OnAfterSpawnWithTrace( item );
				}
				else
				{
					TraceUnacceptedOrNoObject();
				}
			}
		}

		public void SpawnC( int index )
		{
			Map map = Map;

			if ( map == null || map == Map.Internal || CreaturesNameCountC == 0 || index >= CreaturesNameCountC || Parent != null )
				return;

			DefragWithTrace();

			if ( m_CreaturesC.Count >= m_CountC )
				return;


			TraceSpawnAttempt();


			IEntity ent = CreateSpawnedObjectC( index );

			if ( ent is Mobile )
			{
				Mobile m = (Mobile)ent;

				m_CreaturesC.Add( m );
				TraceAcceptedIntoSpawnerList();

				Point3D loc = ( m is BaseVendor ? this.Location : GetSpawnPositionWithTrace() );
				
				if ( m is WanderingHealer || m is Adventurers || m is Jedi )
				{
					loc = GetSpawnPositionWithTrace();
				}
				else if ( m is Syth && SpawnID == 9999 )
				{
					loc = GetSpawnSpotLandDungeonWithTrace();
				}

                                OnBeforeSpawnWithTrace( m, loc, map );
                                InvalidateProperties();

                                MoveToWorldWithTrace( m, loc, map );

                                m.Direction = m_SpawnDirection;

                                if ( m is BaseCreature )
                                {
                                        BaseCreature c = (BaseCreature)m;
					
					if( m_WalkingRange >= 0 )
						c.RangeHome = m_WalkingRange;
					else
						c.RangeHome = m_HomeRange;

					c.CurrentWayPoint = m_WayPoint;

					if ( m_Team > 0 )
						c.Team = m_Team;

					c.Home = loc;

					Server.Misc.MorphingTime.SetGender( m );

					if ( IsOutsideSpawnerRegionWithTrace( m ) ){ DeleteRejectedByRegionWithTrace( m ); DefragWithTrace(); return; } // REMOVE IF NOT IN SAME REGION
					c.SpawnerSpawned = true;
				}

				OnAfterSpawnWithTrace( m );

				//check for bugged enemies spawning at 0,0,0
				if (m != null && m.Map != null)
				{
					Point3D loccheck = m.Location;
					if (loccheck.X == 0 && loccheck.Y == 0)
					{
						Console.WriteLine( m + " spawned at 0,0 at map " + m.Map + " from spawner at " + this.Location );
					}
				}

			}
			else if ( ent is Item )
			{
				Item item = (Item)ent;

				m_CreaturesC.Add( item );
				TraceAcceptedIntoSpawnerList();

				Point3D loc = GetSpawnPositionWithTrace();

				OnBeforeSpawnWithTrace( item, loc, map );
				InvalidateProperties();

				MoveToWorldWithTrace( item, loc, map );

				if ( IsOutsideSpawnerRegionWithTrace( item ) ){ DeleteRejectedByRegionWithTrace( item ); DefragWithTrace(); return; } // REMOVE IF NOT IN SAME REGION

				OnAfterSpawnWithTrace( item );
			}
			else
			{
				TraceUnacceptedOrNoObject();
			}
		}

		public void SpawnD( int index )
		{
			Map map = Map;

			if ( map == null || map == Map.Internal || CreaturesNameCountD == 0 || index >= CreaturesNameCountD || Parent != null )
				return;

			DefragWithTrace();

			if ( m_CreaturesD.Count >= m_CountD )
				return;


			TraceSpawnAttempt();


			IEntity ent = CreateSpawnedObjectD( index );

			if ( ent is Mobile )
			{
				Mobile m = (Mobile)ent;

				m_CreaturesD.Add( m );
				TraceAcceptedIntoSpawnerList();

				Point3D loc = ( m is BaseVendor ? this.Location : GetSpawnPositionWithTrace() );
				
				if ( m is WanderingHealer || m is Adventurers || m is Jedi )
				{
					loc = GetSpawnPositionWithTrace();
				}
				else if ( m is Syth && SpawnID == 9999 )
				{
					loc = GetSpawnSpotLandDungeonWithTrace();
				}

                                OnBeforeSpawnWithTrace( m, loc, map );
                                InvalidateProperties();

                                MoveToWorldWithTrace( m, loc, map );

                                m.Direction = m_SpawnDirection;

                                if ( m is BaseCreature )
                                {
                                        BaseCreature c = (BaseCreature)m;
					
					if( m_WalkingRange >= 0 )
						c.RangeHome = m_WalkingRange;
					else
						c.RangeHome = m_HomeRange;

					c.CurrentWayPoint = m_WayPoint;

					if ( m_Team > 0 )
						c.Team = m_Team;

					c.Home = loc;

					Server.Misc.MorphingTime.SetGender( m );

					if ( IsOutsideSpawnerRegionWithTrace( m ) ){ DeleteRejectedByRegionWithTrace( m ); DefragWithTrace(); return; } // REMOVE IF NOT IN SAME REGION
					c.SpawnerSpawned = true;
				}

				OnAfterSpawnWithTrace( m );
			}
			else if ( ent is Item )
			{
				Item item = (Item)ent;

				m_CreaturesD.Add( item );
				TraceAcceptedIntoSpawnerList();

				Point3D loc = GetSpawnPositionWithTrace();

				OnBeforeSpawnWithTrace( item, loc, map );
				InvalidateProperties();

				MoveToWorldWithTrace( item, loc, map );

				if ( IsOutsideSpawnerRegionWithTrace( item ) ){ DeleteRejectedByRegionWithTrace( item ); DefragWithTrace(); return; } // REMOVE IF NOT IN SAME REGION

				OnAfterSpawnWithTrace( item );
			}
			else
			{
				TraceUnacceptedOrNoObject();
			}
		}

		public void SpawnE( int index )
		{
			Map map = Map;

			if ( map == null || map == Map.Internal || CreaturesNameCountE == 0 || index >= CreaturesNameCountE || Parent != null )
				return;

			DefragWithTrace();

			if ( m_CreaturesE.Count >= m_CountE )
				return;


			TraceSpawnAttempt();


			IEntity ent = CreateSpawnedObjectE( index );

			if ( ent is Mobile )
			{
				Mobile m = (Mobile)ent;

				m_CreaturesE.Add( m );
				TraceAcceptedIntoSpawnerList();
				
				Point3D loc = ( m is BaseVendor ? this.Location : GetSpawnPositionWithTrace() );
				
				if ( m is WanderingHealer || m is Adventurers || m is Jedi )
				{
					loc = GetSpawnPositionWithTrace();
				}
				else if ( m is Syth && SpawnID == 9999 )
				{
					loc = GetSpawnSpotLandDungeonWithTrace();
				}

                                OnBeforeSpawnWithTrace( m, loc, map );
                                InvalidateProperties();

                                MoveToWorldWithTrace( m, loc, map );

                                m.Direction = m_SpawnDirection;

                                if ( m is BaseCreature )
                                {
                                        BaseCreature c = (BaseCreature)m;
					
					if( m_WalkingRange >= 0 )
						c.RangeHome = m_WalkingRange;
					else
						c.RangeHome = m_HomeRange;

					c.CurrentWayPoint = m_WayPoint;

					if ( m_Team > 0 )
						c.Team = m_Team;

					c.Home = loc;

					Server.Misc.MorphingTime.SetGender( m );

					if ( IsOutsideSpawnerRegionWithTrace( m ) ){ DeleteRejectedByRegionWithTrace( m ); DefragWithTrace(); return; } // REMOVE IF NOT IN SAME REGION
					c.SpawnerSpawned = true;
				}

				OnAfterSpawnWithTrace( m );
			}
			else if ( ent is Item )
			{
				Item item = (Item)ent;

				m_CreaturesE.Add( item );
				TraceAcceptedIntoSpawnerList();

				Point3D loc = GetSpawnPositionWithTrace();

				OnBeforeSpawnWithTrace( item, loc, map );
				InvalidateProperties();

				MoveToWorldWithTrace( item, loc, map );

				if ( IsOutsideSpawnerRegionWithTrace( item ) ){ DeleteRejectedByRegionWithTrace( item ); DefragWithTrace(); return; } // REMOVE IF NOT IN SAME REGION

				OnAfterSpawnWithTrace( item );
			}
			else
			{
				TraceUnacceptedOrNoObject();
			}
		}

		public Point3D GetSpawnPosition()
		{
			Map map = Map;

			if ( map == null )
				return Location;

			// Try 10 times to find a Spawnable location.
			for ( int i = 0; i < 10; i++ )
			{
				int x, y;

				if ( m_HomeRange > 0 ) {
					x = Location.X + (Utility.Random( (m_HomeRange * 2) + 1 ) - m_HomeRange);
					y = Location.Y + (Utility.Random( (m_HomeRange * 2) + 1 ) - m_HomeRange);
				} else {
					x = Location.X;
					y = Location.Y;
				}

				int z = Map.GetAverageZ( x, y );

				Region regSpawner = Region.Find( this.Location, this.Map );
				Region regSpawn1 = Region.Find( new Point3D(x, y, this.Z), this.Map );
				Region regSpawn2 = Region.Find( new Point3D(x, y, z), this.Map );

				if ( Map.CanSpawnMobile( new Point2D( x, y ), this.Z ) && regSpawner == regSpawn1 )
					return new Point3D( x, y, this.Z );
				else if ( Map.CanSpawnMobile( new Point2D( x, y ), z ) && regSpawner == regSpawn2 )
					return new Point3D( x, y, z );
			}

			return this.Location;
		}

        public Point3D GetSpawnSpotLandDungeon()
        {
			if ( Utility.RandomBool() )
			{
				Point3D dungeonLocation = Server.Misc.Worlds.GetRandomDungeonSpot( Map );

				if ( dungeonLocation != Point3D.Zero && Map != null && Map != Map.Internal && Map.CanSpawnMobile( dungeonLocation ) )
					return dungeonLocation;
			}

			Point3D landLocation = Server.Misc.Worlds.GetRandomLocation( Server.Misc.Worlds.GetMyWorld( Map, Location, X, Y ), "land" );

			if ( landLocation != Point3D.Zero && Map != null && Map != Map.Internal && Map.CanSpawnMobile( landLocation ) )
				return landLocation;

			return GetSpawnPosition();
		}

		public void DoTimer()
		{
			if ( !m_Running )
				return;

			int minSeconds = (int)m_MinDelay.TotalSeconds;
			int maxSeconds = (int)m_MaxDelay.TotalSeconds;

			TimeSpan delay = TimeSpan.FromSeconds( Utility.RandomMinMax( minSeconds, maxSeconds ) );
			DoTimer( delay );
		}

		public void DoTimer( TimeSpan delay )
		{
			DoTimer( delay, false );
		}

		private void DoTimer( TimeSpan delay, bool startupCatchup )
		{
			if ( !m_Running )
				return;

			DateTime now = DateTime.UtcNow;
			DateTime dueTime = now + delay;

			if ( startupCatchup && EnablePremiumSpawnerScheduler && EnablePremiumSpawnerStartupCatchupJitter && dueTime <= now + TimeSpan.FromSeconds( 1.0 ) )
			{
				int minDelay = PremiumSpawnerStartupCatchupMinDelaySeconds;
				int maxDelay = PremiumSpawnerStartupCatchupWindowSeconds;

				if ( maxDelay < minDelay )
					maxDelay = minDelay;

				int catchupDelaySeconds = Utility.RandomMinMax( minDelay, maxDelay );
				delay = TimeSpan.FromSeconds( catchupDelaySeconds );
				dueTime = now + delay;
				RecordStartupCatchupDelay( (long)delay.TotalMilliseconds );
			}

			m_End = dueTime;

			if ( m_Timer != null )
			{
				m_Timer.Stop();
				m_Timer = null;
			}

			if ( EnablePremiumSpawnerScheduler )
			{
				SchedulePremiumSpawner( this, dueTime, startupCatchup ? "startupCatchup" : "timer" );
				return;
			}

			m_Timer = new InternalTimer( this, delay );
			m_Timer.Start();
		}

        private static int ClampSpawns(List<IEntity> creatures, int amount)
        {
			BaseCreature firstCreature = creatures.FirstOrDefault(c => c is BaseCreature) as BaseCreature;
			bool hasBreathAttack = firstCreature != null && firstCreature.HasBreath;
			if (!hasBreathAttack) return amount; // Can still split

			const int Max_Breath_Attacker_Spawns = 3;
			return Max_Breath_Attacker_Spawns < creatures.Count 
				? Math.Min(amount, Max_Breath_Attacker_Spawns - creatures.Count) // Allow no more than +3 in this slot
				: 1; // Let it spawn the original
        }

		private class PremiumSpawnerSchedulerTimer : Timer
		{
			public PremiumSpawnerSchedulerTimer() : base(TimeSpan.FromMilliseconds(PremiumSpawnerSchedulerIntervalMs))
			{
				Priority = TimerPriority.TwoFiftyMS;
			}

			protected override void OnTick()
			{
				DrainPremiumSpawnerScheduler();
			}
		}

		private class InternalTimer : Timer
		{
			private PremiumSpawner m_PremiumSpawner;

			public InternalTimer( PremiumSpawner spawner, TimeSpan delay ) : base( delay )
			{
				if ( spawner.IsFull || spawner.IsFulla || spawner.IsFullb || spawner.IsFullc || spawner.IsFulld || spawner.IsFulle )
					Priority = TimerPriority.FiveSeconds;
				else
					Priority = TimerPriority.OneSecond;

				m_PremiumSpawner = spawner;
			}

			protected override void OnTick()
			{
				if ( m_PremiumSpawner != null )
					if ( !m_PremiumSpawner.Deleted )
					{
						if ( EnablePremiumSpawnerScheduler )
						{
							lock (m_StatsLock)
							{
								++m_CurrentSchedulerStats.OldInternalTimerCallbacks;
								++m_SchedulerOldInternalTimerCallbacksTotal;
							}

							SchedulePremiumSpawner( m_PremiumSpawner, DateTime.UtcNow, "legacyInternalTimer" );
						}
						else
						{
							m_PremiumSpawner.OnTick(false);
						}
					}
			}
		}

		public int CountCreatures( string creatureName )
		{
			Defrag();

			int count = 0;

			for ( int i = 0; i < m_Creatures.Count; ++i )
				if ( Insensitive.Equals( creatureName, m_Creatures[i].GetType().Name ) )
					++count;

			return count;
		}

		public int CountCreaturesA( string creatureNameA )
		{
			Defrag();

			int count = 0;

			for ( int i = 0; i < m_CreaturesA.Count; ++i )
				if ( Insensitive.Equals( creatureNameA, m_CreaturesA[i].GetType().Name ) )
					++count;

			return count;
		}

		public int CountCreaturesB( string creatureNameB )
		{
			Defrag();

			int count = 0;

			for ( int i = 0; i < m_CreaturesB.Count; ++i )
				if ( Insensitive.Equals( creatureNameB, m_CreaturesB[i].GetType().Name ) )
					++count;

			return count;
		}

		public int CountCreaturesC( string creatureNameC )
		{
			Defrag();

			int count = 0;

			for ( int i = 0; i < m_CreaturesC.Count; ++i )
				if ( Insensitive.Equals( creatureNameC, m_CreaturesC[i].GetType().Name ) )
					++count;

			return count;
		}

		public int CountCreaturesD( string creatureNameD )
		{
			Defrag();

			int count = 0;

			for ( int i = 0; i < m_CreaturesD.Count; ++i )
				if ( Insensitive.Equals( creatureNameD, m_CreaturesD[i].GetType().Name ) )
					++count;

			return count;
		}

		public int CountCreaturesE( string creatureNameE )
		{
			Defrag();

			int count = 0;

			for ( int i = 0; i < m_CreaturesE.Count; ++i )
				if ( Insensitive.Equals( creatureNameE, m_CreaturesE[i].GetType().Name ) )
					++count;

			return count;
		}

		public void RemoveCreatures( string creatureName )
		{
			Defrag();

			for ( int i = 0; i < m_Creatures.Count; ++i )
			{
				IEntity e = m_Creatures[i];

				if ( Insensitive.Equals( creatureName, e.GetType().Name ) )
						e.Delete();
			}

			InvalidateProperties();
		}

		public void RemoveCreaturesA( string creatureNameA )
		{
			Defrag();

			for ( int i = 0; i < m_CreaturesA.Count; ++i )
			{
				IEntity e = m_CreaturesA[i];

				if ( Insensitive.Equals( creatureNameA, e.GetType().Name ) )
						e.Delete();
			}

			InvalidateProperties();
		}

		public void RemoveCreaturesB( string creatureNameB )
		{
			Defrag();

			for ( int i = 0; i < m_CreaturesB.Count; ++i )
			{
				IEntity e = m_CreaturesB[i];

				if ( Insensitive.Equals( creatureNameB, e.GetType().Name ) )
						e.Delete();
			}

			InvalidateProperties();
		}

		public void RemoveCreaturesC( string creatureNameC )
		{
			Defrag();

			for ( int i = 0; i < m_CreaturesC.Count; ++i )
			{
				IEntity e = m_CreaturesC[i];

				if ( Insensitive.Equals( creatureNameC, e.GetType().Name ) )
						e.Delete();
			}

			InvalidateProperties();
		}

		public void RemoveCreaturesD( string creatureNameD )
		{
			Defrag();

			for ( int i = 0; i < m_CreaturesD.Count; ++i )
			{
				IEntity e = m_CreaturesD[i];

				if ( Insensitive.Equals( creatureNameD, e.GetType().Name ) )
						e.Delete();
			}

			InvalidateProperties();
		}

		public void RemoveCreaturesE( string creatureNameE )
		{
			Defrag();

			for ( int i = 0; i < m_CreaturesE.Count; ++i )
			{
				IEntity e = m_CreaturesE[i];

				if ( Insensitive.Equals( creatureNameE, e.GetType().Name ) )
						e.Delete();
			}

			InvalidateProperties();
		}
		
		public void RemoveCreatures()
		{
			Defrag();

			for ( int i = 0; i < m_Creatures.Count; ++i )
				m_Creatures[i].Delete();

			InvalidateProperties();
		}

		public void RemoveCreaturesA()
		{
			Defrag();

			for ( int i = 0; i < m_CreaturesA.Count; ++i )
				m_CreaturesA[i].Delete();

			InvalidateProperties();
		}

		public void RemoveCreaturesB()
		{
			Defrag();

			for ( int i = 0; i < m_CreaturesB.Count; ++i )
				m_CreaturesB[i].Delete();

			InvalidateProperties();
		}

		public void RemoveCreaturesC()
		{
			Defrag();

			for ( int i = 0; i < m_CreaturesC.Count; ++i )
				m_CreaturesC[i].Delete();

			InvalidateProperties();
		}

		public void RemoveCreaturesD()
		{
			Defrag();

			for ( int i = 0; i < m_CreaturesD.Count; ++i )
				m_CreaturesD[i].Delete();

			InvalidateProperties();
		}

		public void RemoveCreaturesE()
		{
			Defrag();

			for ( int i = 0; i < m_CreaturesE.Count; ++i )
				m_CreaturesE[i].Delete();

			InvalidateProperties();
		}

		private void NormalizeCreatureHomes( List<IEntity> creatures )
		{
			if ( creatures == null )
				return;

			for ( int i = 0; i < creatures.Count; ++i )
			{
				IEntity e = creatures[i];
				BaseCreature creature = e as BaseCreature;

				if ( creature == null )
					continue;

				if ( creature.Home == Location )
					creature.Home = creature.Location;
			}
		}

		public void BringToHome()
		{
			Defrag();

			for ( int i = 0; i < m_Creatures.Count; ++i )
			{
				IEntity e = m_Creatures[i];

				if ( e is Mobile )
				{
					Mobile m = (Mobile)e;

					m.MoveToWorld( Location, Map );
				}
				else if ( e is Item )
				{
					Item item = (Item)e;

					item.MoveToWorld( Location, Map );
				}
			}

			for ( int i = 0; i < m_CreaturesA.Count; ++i )
			{
				object o = m_CreaturesA[i];

				if ( o is Mobile )
				{
					Mobile m = (Mobile)o;

					m.MoveToWorld( Location, Map );
				}
				else if ( o is Item )
				{
					Item item = (Item)o;

					item.MoveToWorld( Location, Map );
				}
			}

			for ( int i = 0; i < m_CreaturesB.Count; ++i )
			{
				object o = m_CreaturesB[i];

				if ( o is Mobile )
				{
					Mobile m = (Mobile)o;

					m.MoveToWorld( Location, Map );
				}
				else if ( o is Item )
				{
					Item item = (Item)o;

					item.MoveToWorld( Location, Map );
				}
			}

			for ( int i = 0; i < m_CreaturesC.Count; ++i )
			{
				object o = m_CreaturesC[i];

				if ( o is Mobile )
				{
					Mobile m = (Mobile)o;

					m.MoveToWorld( Location, Map );
				}
				else if ( o is Item )
				{
					Item item = (Item)o;

					item.MoveToWorld( Location, Map );
				}
			}

			for ( int i = 0; i < m_CreaturesD.Count; ++i )
			{
				object o = m_CreaturesD[i];

				if ( o is Mobile )
				{
					Mobile m = (Mobile)o;

					m.MoveToWorld( Location, Map );
				}
				else if ( o is Item )
				{
					Item item = (Item)o;

					item.MoveToWorld( Location, Map );
				}
			}

			for ( int i = 0; i < m_CreaturesE.Count; ++i )
			{
				object o = m_CreaturesE[i];

				if ( o is Mobile )
				{
					Mobile m = (Mobile)o;

					m.MoveToWorld( Location, Map );
				}
				else if ( o is Item )
				{
					Item item = (Item)o;

					item.MoveToWorld( Location, Map );
				}
			}

		}

		public override void OnDelete()
		{
			base.OnDelete();
			UnschedulePremiumSpawner( this );

			RemoveCreatures();
			RemoveCreaturesA();
			RemoveCreaturesB();
			RemoveCreaturesC();
			RemoveCreaturesD();
			RemoveCreaturesE();
			if ( m_Timer != null )
			{
				m_Timer.Stop();
				m_Timer = null;
			}
		}
		
public override void Serialize( GenericWriter writer )
{
base.Serialize( writer );

writer.Write( (int) 5 ); // version
writer.Write( m_WalkingRange );

writer.Write( m_SpawnID );
writer.Write( (int)m_SpawnDirection );
writer.Write( m_CountA );
writer.Write( m_CountB );
writer.Write( m_CountC );
writer.Write( m_CountD );
writer.Write( m_CountE );

			writer.Write( m_WayPoint );

			writer.Write( m_Group );

			writer.Write( m_MinDelay );
			writer.Write( m_MaxDelay );
			writer.Write( m_Count );
			writer.Write( m_Team );
			writer.Write( m_HomeRange );
			writer.Write( m_Running );
			
			if ( m_Running )
				writer.WriteDeltaTime( m_End );

			writer.Write( m_CreaturesName.Count );

			for ( int i = 0; i < m_CreaturesName.Count; ++i )
				writer.Write( m_CreaturesName[i] );

			writer.Write( m_CreaturesNameA.Count );

			for ( int i = 0; i < m_CreaturesNameA.Count; ++i )
				writer.Write( (string)m_CreaturesNameA[i] );

			writer.Write( m_CreaturesNameB.Count );

			for ( int i = 0; i < m_CreaturesNameB.Count; ++i )
				writer.Write( (string)m_CreaturesNameB[i] );

			writer.Write( m_CreaturesNameC.Count );

			for ( int i = 0; i < m_CreaturesNameC.Count; ++i )
				writer.Write( (string)m_CreaturesNameC[i] );

			writer.Write( m_CreaturesNameD.Count );

			for ( int i = 0; i < m_CreaturesNameD.Count; ++i )
				writer.Write( (string)m_CreaturesNameD[i] );

			writer.Write( m_CreaturesNameE.Count );

			for ( int i = 0; i < m_CreaturesNameE.Count; ++i )
				writer.Write( (string)m_CreaturesNameE[i] );

			writer.Write( m_Creatures.Count );

			for ( int i = 0; i < m_Creatures.Count; ++i )
			{
				IEntity e = m_Creatures[i];

				if ( e is Item )
					writer.Write( (Item)e );
				else if ( e is Mobile )
					writer.Write( (Mobile)e );
				else
					writer.Write( Serial.MinusOne );
			}

			writer.Write( m_CreaturesA.Count );

			for ( int i = 0; i < m_CreaturesA.Count; ++i )
			{
				IEntity e = m_CreaturesA[i];

				if ( e is Item )
					writer.Write( (Item)e );
				else if ( e is Mobile )
					writer.Write( (Mobile)e );
				else
					writer.Write( Serial.MinusOne );
			}

			writer.Write( m_CreaturesB.Count );

			for ( int i = 0; i < m_CreaturesB.Count; ++i )
			{
				IEntity e = m_CreaturesB[i];

				if ( e is Item )
					writer.Write( (Item)e );
				else if ( e is Mobile )
					writer.Write( (Mobile)e );
				else
					writer.Write( Serial.MinusOne );
			}

			writer.Write( m_CreaturesC.Count );

			for ( int i = 0; i < m_CreaturesC.Count; ++i )
			{
				IEntity e = m_CreaturesC[i];

				if ( e is Item )
					writer.Write( (Item)e );
				else if ( e is Mobile )
					writer.Write( (Mobile)e );
				else
					writer.Write( Serial.MinusOne );
			}

			writer.Write( m_CreaturesD.Count );

			for ( int i = 0; i < m_CreaturesD.Count; ++i )
			{
				IEntity e = m_CreaturesD[i];

				if ( e is Item )
					writer.Write( (Item)e );
				else if ( e is Mobile )
					writer.Write( (Mobile)e );
				else
					writer.Write( Serial.MinusOne );
			}

			writer.Write( m_CreaturesE.Count );

			for ( int i = 0; i < m_CreaturesE.Count; ++i )
			{
				IEntity e = m_CreaturesE[i];

				if ( e is Item )
					writer.Write( (Item)e );
				else if ( e is Mobile )
					writer.Write( (Mobile)e );
				else
					writer.Write( Serial.MinusOne );
			}

		}

		private static WarnTimer m_WarnTimer;

public override void Deserialize( GenericReader reader )
{
base.Deserialize( reader );

int version = reader.ReadInt();

switch ( version )
{
case 5:
{
m_WalkingRange = reader.ReadInt();
m_SpawnID = reader.ReadInt();
m_SpawnDirection = (Direction)reader.ReadInt();
m_CountA = reader.ReadInt();
m_CountB = reader.ReadInt();
m_CountC = reader.ReadInt();
m_CountD = reader.ReadInt();
m_CountE = reader.ReadInt();

goto case 3;
}
case 4:
{
m_WalkingRange = reader.ReadInt();
m_SpawnID = reader.ReadInt();
m_CountA = reader.ReadInt();
m_CountB = reader.ReadInt();
m_CountC = reader.ReadInt();
m_CountD = reader.ReadInt();
m_CountE = reader.ReadInt();

goto case 3;
}
				case 3:
				case 2:
				{
					m_WayPoint = reader.ReadItem() as WayPoint;

					goto case 1;
				}

				case 1:
				{
					m_Group = reader.ReadBool();
					
					goto case 0;
				}

				case 0:
				{
					m_MinDelay = reader.ReadTimeSpan();
					m_MaxDelay = reader.ReadTimeSpan();
					m_Count = reader.ReadInt();
					m_Team = reader.ReadInt();
					m_HomeRange = reader.ReadInt();
					m_Running = reader.ReadBool();

					TimeSpan ts = TimeSpan.Zero;

					if ( m_Running )
						ts = reader.ReadDeltaTime() - DateTime.UtcNow;
					
					int size = reader.ReadInt();
					m_CreaturesName = new List<string>( size );
					for ( int i = 0; i < size; ++i )
					{
						string creatureString = reader.ReadString();

						m_CreaturesName.Add( creatureString );
						string typeName = ParseType( creatureString );

						if ( ScriptCompiler.FindTypeByName( typeName ) == null )
						{
							if ( m_WarnTimer == null )
								m_WarnTimer = new WarnTimer();

							m_WarnTimer.Add( Location, Map, typeName );
						}
					}

					int sizeA = reader.ReadInt();
					m_CreaturesNameA = new List<string>( sizeA );
					for ( int i = 0; i < sizeA; ++i )
					{
						string creatureString = reader.ReadString();

						m_CreaturesNameA.Add( creatureString );
						string typeName = ParseType( creatureString );

						if ( ScriptCompiler.FindTypeByName( typeName ) == null )
						{
							if ( m_WarnTimer == null )
								m_WarnTimer = new WarnTimer();

							m_WarnTimer.Add( Location, Map, typeName );
						}
					}

					int sizeB = reader.ReadInt();
					m_CreaturesNameB = new List<string>( sizeB );
					for ( int i = 0; i < sizeB; ++i )
					{
						string creatureString = reader.ReadString();

						m_CreaturesNameB.Add( creatureString );
						string typeName = ParseType( creatureString );

						if ( ScriptCompiler.FindTypeByName( typeName ) == null )
						{
							if ( m_WarnTimer == null )
								m_WarnTimer = new WarnTimer();

							m_WarnTimer.Add( Location, Map, typeName );
						}
					}

					int sizeC = reader.ReadInt();
					m_CreaturesNameC = new List<string>( sizeC );
					for ( int i = 0; i < sizeC; ++i )
					{
						string creatureString = reader.ReadString();

						m_CreaturesNameC.Add( creatureString );
						string typeName = ParseType( creatureString );

						if ( ScriptCompiler.FindTypeByName( typeName ) == null )
						{
							if ( m_WarnTimer == null )
								m_WarnTimer = new WarnTimer();

							m_WarnTimer.Add( Location, Map, typeName );
						}
					}

					int sizeD = reader.ReadInt();
					m_CreaturesNameD = new List<string>( sizeD );
					for ( int i = 0; i < sizeD; ++i )
					{
						string creatureString = reader.ReadString();

						m_CreaturesNameD.Add( creatureString );
						string typeName = ParseType( creatureString );

						if ( ScriptCompiler.FindTypeByName( typeName ) == null )
						{
							if ( m_WarnTimer == null )
								m_WarnTimer = new WarnTimer();

							m_WarnTimer.Add( Location, Map, typeName );
						}
					}

					int sizeE = reader.ReadInt();
					m_CreaturesNameE = new List<string>( sizeE );
					for ( int i = 0; i < sizeE; ++i )
					{
						string creatureString = reader.ReadString();

						m_CreaturesNameE.Add( creatureString );
						string typeName = ParseType( creatureString );

						if ( ScriptCompiler.FindTypeByName( typeName ) == null )
						{
							if ( m_WarnTimer == null )
								m_WarnTimer = new WarnTimer();

							m_WarnTimer.Add( Location, Map, typeName );
						}
					}

					int count = reader.ReadInt();
					m_Creatures = new List<IEntity>( count );
					for ( int i = 0; i < count; ++i )
					{
						IEntity e = World.FindEntity( reader.ReadInt() );

						if ( e != null )
							m_Creatures.Add( e );
					}

					int countA = reader.ReadInt();
					m_CreaturesA = new List<IEntity>( countA );
					for ( int i = 0; i < countA; ++i )
					{
						IEntity e = World.FindEntity( reader.ReadInt() );

						if ( e != null )
							m_CreaturesA.Add( e );
					}

					int countB = reader.ReadInt();
					m_CreaturesB = new List<IEntity>( countB );
					for ( int i = 0; i < countB; ++i )
					{
						IEntity e = World.FindEntity( reader.ReadInt() );

						if ( e != null )
							m_CreaturesB.Add( e );
					}

					int countC = reader.ReadInt();
					m_CreaturesC = new List<IEntity>( countC );
					for ( int i = 0; i < countC; ++i )
					{
						IEntity e = World.FindEntity( reader.ReadInt() );

						if ( e != null )
							m_CreaturesC.Add( e );
					}

					int countD = reader.ReadInt();
					m_CreaturesD = new List<IEntity>( countD );
					for ( int i = 0; i < countD; ++i )
					{
						IEntity e = World.FindEntity( reader.ReadInt() );

						if ( e != null )
							m_CreaturesD.Add( e );
					}

					int countE = reader.ReadInt();
					m_CreaturesE = new List<IEntity>( countE );
					for ( int i = 0; i < countE; ++i )
					{
						IEntity e = World.FindEntity( reader.ReadInt() );

						if ( e != null )
							m_CreaturesE.Add( e );
					}

					if ( m_Running )
						DoTimer( ts, true );

					break;
				}
			}

			NormalizeCreatureHomes( m_Creatures );
			NormalizeCreatureHomes( m_CreaturesA );
			NormalizeCreatureHomes( m_CreaturesB );
			NormalizeCreatureHomes( m_CreaturesC );
			NormalizeCreatureHomes( m_CreaturesD );
			NormalizeCreatureHomes( m_CreaturesE );

			if ( version < 3 && Weight == 0 )
				Weight = -1;
		}

		private class WarnTimer : Timer
		{
			private List<WarnEntry> m_List;

			private class WarnEntry
			{
				public Point3D m_Point;
				public Map m_Map;
				public string m_Name;

				public WarnEntry( Point3D p, Map map, string name )
				{
					m_Point = p;
					m_Map = map;
					m_Name = name;
				}
			}

			public WarnTimer() : base( TimeSpan.FromSeconds( 1.0 ) )
			{
				m_List = new List<WarnEntry>();
				Start();
			}

			public void Add( Point3D p, Map map, string name )
			{
				m_List.Add( new WarnEntry( p, map, name ) );
			}

			protected override void OnTick()
			{
				try
				{
					Console.WriteLine( "Warning: {0} bad spawns detected, logged: 'PremiumBadspawn.log'", m_List.Count );

					using ( StreamWriter op = new StreamWriter( "PremiumBadspawn.log", true ) )
					{
						op.WriteLine( "# Bad spawns : {0}", DateTime.UtcNow );
						op.WriteLine( "# Format: X Y Z F Name" );
						op.WriteLine();

						foreach ( WarnEntry e in m_List )
							op.WriteLine( "{0}\t{1}\t{2}\t{3}\t{4}", e.m_Point.X, e.m_Point.Y, e.m_Point.Z, e.m_Map, e.m_Name );

						op.WriteLine();
						op.WriteLine();
					}
				}
				catch
				{
				}
			}
		}
	}
}
