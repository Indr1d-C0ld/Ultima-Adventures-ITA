using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Server.Diagnostics
{
	public enum RuneBagPhase
	{
		EntryGuard,
		ContainerScan,
		RuneSelection,
		SpellCreation,
		TargetCreation,
		CastStart,
		GumpBuild,
		Other
	}

	public static class RuneBagDiagnostics
	{
		private const int CooldownMilliseconds = 250;
		private const int InProgressTimeoutMilliseconds = 10000;
		private const int SlowCallMilliseconds = 50;
		private const int MaxGuardStates = 512;
		private const int MaxPlayerStats = 128;
		private const int MaxItemStats = 256;
		private static readonly TimeSpan StateRetention = TimeSpan.FromMinutes( 30.0 );
		private static readonly TimeSpan StatsRetention = TimeSpan.FromHours( 2.0 );
		private static readonly TimeSpan PruneInterval = TimeSpan.FromMinutes( 1.0 );

		private static readonly object m_Lock = new object();
		private static readonly Dictionary<int, GuardState> m_States = new Dictionary<int, GuardState>();
		private static readonly Dictionary<int, EntityStat> m_PlayerStats = new Dictionary<int, EntityStat>();
		private static readonly Dictionary<int, EntityStat> m_ItemStats = new Dictionary<int, EntityStat>();

		private static DateTime m_LastPruneUtc;
		private static long m_NextAttemptId;
		private static long m_Attempts;
		private static long m_Allowed;
		private static long m_ThrottledByCooldown;
		private static long m_ThrottledAlreadyCasting;
		private static long m_ThrottledAlreadyTargeting;
		private static long m_ThrottledInProgress;
		private static long m_InvalidState;
		private static long m_FailedNormalChecks;
		private static long m_SpellCreated;
		private static long m_TargetCreated;
		private static long m_GumpCreated;
		private static long m_CastsStarted;
		private static long m_CastsFailed;
		private static long m_TotalMs;
		private static long m_MaxMs;
		private static long m_SlowCalls;
		private static long m_DiagnosticErrors;
		private static long m_StaleEntriesPruned;
		private static DateTime m_LastThrottleUtc;
		private static Sample m_LastSample;
		private static Sample m_WorstSample;

		private sealed class GuardState
		{
			public DateTime LastAllowedUtc;
			public DateTime LastSeenUtc;
			public DateTime InProgressUtc;
			public bool InProgress;
			public long ActiveAttemptId;
		}

		private sealed class EntityStat
		{
			public int Serial;
			public string Name;
			public string Account;
			public long Attempts;
			public long Throttles;
			public DateTime LastSeenUtc;

			public EntityStat Clone()
			{
				EntityStat copy = new EntityStat();
				copy.Serial = Serial;
				copy.Name = Name;
				copy.Account = Account;
				copy.Attempts = Attempts;
				copy.Throttles = Throttles;
				copy.LastSeenUtc = LastSeenUtc;
				return copy;
			}
		}

		public sealed class Attempt
		{
			internal long Id;
			internal long StartedTimestamp;
			internal DateTime StartedUtc;
			internal int PlayerSerial;
			internal int ItemSerial;
			internal string PlayerName;
			internal string AccountName;
			internal string Source;
			internal long EntryGuardMs;
			internal long ContainerScanMs;
			internal long RuneSelectionMs;
			internal long SpellCreationMs;
			internal long TargetCreationMs;
			internal long CastStartMs;
			internal long GumpBuildMs;
			internal long OtherMs;
			internal bool SpellCreated;
			internal bool TargetCreated;
			internal bool GumpCreated;
			internal bool CastStarted;
			internal bool CastFailed;
			internal bool FailedNormalCheck;
			internal bool InvalidState;
			internal string Error;
			internal bool Completed;
		}

		private sealed class Sample
		{
			public DateTime StartedUtc;
			public int PlayerSerial;
			public int ItemSerial;
			public string PlayerName;
			public string AccountName;
			public string Source;
			public long TotalMs;
			public string WorstPhase;
			public long WorstPhaseMs;
			public long EntryGuardMs;
			public long ContainerScanMs;
			public long RuneSelectionMs;
			public long SpellCreationMs;
			public long TargetCreationMs;
			public long CastStartMs;
			public long GumpBuildMs;
			public long OtherMs;
			public bool SpellCreated;
			public bool CastStarted;
			public bool CastFailed;
			public string Error;

			public Sample Clone()
			{
				return (Sample)MemberwiseClone();
			}
		}

		public sealed class Snapshot
		{
			public long Attempts;
			public long Allowed;
			public long Throttled;
			public long ThrottledByCooldown;
			public long ThrottledAlreadyCasting;
			public long ThrottledAlreadyTargeting;
			public long ThrottledInProgress;
			public long InvalidState;
			public long FailedNormalChecks;
			public long CastsStarted;
			public long CastsFailed;
			public long TotalMs;
			public long AverageMs;
			public long StaleEntriesPruned;
			public DateTime LastThrottleUtc;
			public int ActiveGuards;
			public int TrackedPlayers;
			public int TrackedItems;
			public long MaxMs;
			public long SlowCalls;
			public int ThrottleTableSize;
			public long DiagnosticErrors;
		}

		public static int CooldownMs
		{
			get { return CooldownMilliseconds; }
		}

		public static long GetTimestamp()
		{
			return Stopwatch.GetTimestamp();
		}

		public static long GetElapsedMilliseconds( long startedTimestamp )
		{
			if ( startedTimestamp <= 0 )
				return 0;

			long elapsed = Stopwatch.GetTimestamp() - startedTimestamp;

			if ( elapsed <= 0 )
				return 0;

			return elapsed * 1000L / Stopwatch.Frequency;
		}

		public static bool TryBegin( Mobile player, Item bag, string source, out Attempt attempt )
		{
			long guardStart = Stopwatch.GetTimestamp();
			attempt = null;
			DateTime now = DateTime.UtcNow;
			int playerSerial = player != null ? player.Serial.Value : 0;
			int itemSerial = bag != null ? bag.Serial.Value : 0;
			string playerName = GetPlayerName( player );
			string accountName = GetAccountName( player );
			bool invalid = player == null || bag == null || player.Deleted || bag.Deleted || !player.Alive || player.Backpack == null || !bag.IsChildOf( player.Backpack );

			lock ( m_Lock )
			{
				PruneLocked( now );
				++m_Attempts;
				EntityStat playerStat = UpdateEntityLocked( m_PlayerStats, MaxPlayerStats, playerSerial, playerName, accountName, now );
				EntityStat itemStat = UpdateEntityLocked( m_ItemStats, MaxItemStats, itemSerial, "rune bag", "none", now );

				if ( invalid )
				{
					++m_InvalidState;
					return false;
				}

				GuardState state;

				if ( !m_States.TryGetValue( playerSerial, out state ) )
				{
					if ( m_States.Count >= MaxGuardStates )
					{
						RemoveOldestStateLocked();
						++m_StaleEntriesPruned;
					}

					state = new GuardState();
					m_States[playerSerial] = state;
				}

				state.LastSeenUtc = now;

				if ( state.InProgress && ( now - state.InProgressUtc ).TotalMilliseconds >= InProgressTimeoutMilliseconds )
				{
					state.InProgress = false;
					state.ActiveAttemptId = 0;
					++m_StaleEntriesPruned;
				}

				bool throttled = false;

				if ( state.InProgress )
				{
					++m_ThrottledInProgress;
					throttled = true;
				}
				else if ( state.LastAllowedUtc != DateTime.MinValue && ( now - state.LastAllowedUtc ).TotalMilliseconds < CooldownMilliseconds )
				{
					++m_ThrottledByCooldown;
					throttled = true;
				}
				else if ( player.Spell != null && player.Spell.IsCasting )
				{
					++m_ThrottledAlreadyCasting;
					throttled = true;
				}
				else if ( player.Target != null )
				{
					++m_ThrottledAlreadyTargeting;
					throttled = true;
				}
				else if ( player.Spell != null )
				{
					++m_ThrottledInProgress;
					throttled = true;
				}

				if ( throttled )
				{
					++playerStat.Throttles;
					++itemStat.Throttles;
					m_LastThrottleUtc = now;
				}
				else
				{
					++m_Allowed;
					state.LastAllowedUtc = now;
					state.InProgress = true;
					state.InProgressUtc = now;
					state.ActiveAttemptId = ++m_NextAttemptId;

					attempt = new Attempt();
					attempt.Id = state.ActiveAttemptId;
					attempt.StartedTimestamp = guardStart;
					attempt.StartedUtc = now;
					attempt.PlayerSerial = playerSerial;
					attempt.ItemSerial = itemSerial;
					attempt.PlayerName = playerName;
					attempt.AccountName = accountName;
					attempt.Source = !String.IsNullOrEmpty( source ) ? source : "OnDoubleClick";
					attempt.EntryGuardMs = GetElapsedMilliseconds( guardStart );
					return true;
				}
			}

			return false;
		}

		public static void RecordPhase( Attempt attempt, RuneBagPhase phase, long elapsedMs )
		{
			if ( attempt == null || attempt.Completed || elapsedMs < 0 )
				return;

			switch ( phase )
			{
				case RuneBagPhase.EntryGuard: attempt.EntryGuardMs += elapsedMs; break;
				case RuneBagPhase.ContainerScan: attempt.ContainerScanMs += elapsedMs; break;
				case RuneBagPhase.RuneSelection: attempt.RuneSelectionMs += elapsedMs; break;
				case RuneBagPhase.SpellCreation: attempt.SpellCreationMs += elapsedMs; break;
				case RuneBagPhase.TargetCreation: attempt.TargetCreationMs += elapsedMs; break;
				case RuneBagPhase.CastStart: attempt.CastStartMs += elapsedMs; break;
				case RuneBagPhase.GumpBuild: attempt.GumpBuildMs += elapsedMs; break;
				default: attempt.OtherMs += elapsedMs; break;
			}
		}

		public static void MarkSpellCreated( Attempt attempt )
		{
			if ( attempt != null )
				attempt.SpellCreated = true;
		}

		public static void MarkTargetCreated( Attempt attempt )
		{
			if ( attempt != null )
				attempt.TargetCreated = true;
		}

		public static void MarkGumpCreated( Attempt attempt )
		{
			if ( attempt != null )
				attempt.GumpCreated = true;
		}

		public static void MarkCastResult( Attempt attempt, bool started )
		{
			if ( attempt == null )
				return;

			attempt.CastStarted = started;
			attempt.CastFailed = !started;
		}

		public static void MarkFailedNormalCheck( Attempt attempt )
		{
			if ( attempt != null )
				attempt.FailedNormalCheck = true;
		}

		public static void MarkInvalidState( Attempt attempt )
		{
			if ( attempt != null )
				attempt.InvalidState = true;
		}

		public static void MarkError( Attempt attempt, Exception error )
		{
			if ( attempt == null )
				return;

			attempt.Error = error != null ? error.GetType().Name + ": " + error.Message : "unknown";
		}

		public static void Complete( Attempt attempt )
		{
			if ( attempt == null || attempt.Completed )
				return;

			attempt.Completed = true;
			long totalMs = GetElapsedMilliseconds( attempt.StartedTimestamp );
			long measured = attempt.EntryGuardMs + attempt.ContainerScanMs + attempt.RuneSelectionMs + attempt.SpellCreationMs + attempt.TargetCreationMs + attempt.CastStartMs + attempt.GumpBuildMs + attempt.OtherMs;

			if ( totalMs > measured )
				attempt.OtherMs += totalMs - measured;

			Sample sample = BuildSample( attempt, totalMs );

			lock ( m_Lock )
			{
				GuardState state;

				if ( m_States.TryGetValue( attempt.PlayerSerial, out state ) && state.ActiveAttemptId == attempt.Id )
				{
					state.InProgress = false;
					state.ActiveAttemptId = 0;
					state.LastSeenUtc = DateTime.UtcNow;
				}

				m_TotalMs += totalMs;
				if ( totalMs > m_MaxMs )
					m_MaxMs = totalMs;
				if ( totalMs >= SlowCallMilliseconds )
					++m_SlowCalls;
				if ( attempt.InvalidState )
					++m_InvalidState;
				if ( attempt.FailedNormalCheck )
					++m_FailedNormalChecks;
				if ( attempt.SpellCreated )
					++m_SpellCreated;
				if ( attempt.TargetCreated )
					++m_TargetCreated;
				if ( attempt.GumpCreated )
					++m_GumpCreated;
				if ( attempt.CastStarted )
					++m_CastsStarted;
				if ( attempt.CastFailed )
					++m_CastsFailed;
				if ( !String.IsNullOrEmpty( attempt.Error ) )
					++m_DiagnosticErrors;

				m_LastSample = sample;

				if ( m_WorstSample == null || sample.TotalMs > m_WorstSample.TotalMs )
					m_WorstSample = sample.Clone();
			}
		}

		public static Snapshot GetSnapshot()
		{
			Snapshot snapshot = new Snapshot();
			DateTime now = DateTime.UtcNow;

			lock ( m_Lock )
			{
				snapshot.Attempts = m_Attempts;
				snapshot.Allowed = m_Allowed;
				snapshot.Throttled = GetThrottleTotalLocked();
				snapshot.ThrottledByCooldown = m_ThrottledByCooldown;
				snapshot.ThrottledAlreadyCasting = m_ThrottledAlreadyCasting;
				snapshot.ThrottledAlreadyTargeting = m_ThrottledAlreadyTargeting;
				snapshot.ThrottledInProgress = m_ThrottledInProgress;
				snapshot.InvalidState = m_InvalidState;
				snapshot.FailedNormalChecks = m_FailedNormalChecks;
				snapshot.CastsStarted = m_CastsStarted;
				snapshot.CastsFailed = m_CastsFailed;
				snapshot.TotalMs = m_TotalMs;
				snapshot.AverageMs = m_Allowed > 0 ? m_TotalMs / m_Allowed : 0;
				snapshot.StaleEntriesPruned = m_StaleEntriesPruned;
				snapshot.LastThrottleUtc = m_LastThrottleUtc;
				snapshot.ActiveGuards = 0;

				foreach ( GuardState state in m_States.Values )
				{
					if ( state.InProgress && ( now - state.InProgressUtc ).TotalMilliseconds < InProgressTimeoutMilliseconds )
						++snapshot.ActiveGuards;
				}

				snapshot.TrackedPlayers = m_PlayerStats.Count;
				snapshot.TrackedItems = m_ItemStats.Count;
				snapshot.MaxMs = m_MaxMs;
				snapshot.SlowCalls = m_SlowCalls;
				snapshot.ThrottleTableSize = m_States.Count;
				snapshot.DiagnosticErrors = m_DiagnosticErrors;
			}

			return snapshot;
		}

		public static void Reset()
		{
			lock ( m_Lock )
			{
				m_PlayerStats.Clear();
				m_ItemStats.Clear();
				m_Attempts = 0;
				m_Allowed = 0;
				m_ThrottledByCooldown = 0;
				m_ThrottledAlreadyCasting = 0;
				m_ThrottledAlreadyTargeting = 0;
				m_ThrottledInProgress = 0;
				m_InvalidState = 0;
				m_FailedNormalChecks = 0;
				m_SpellCreated = 0;
				m_TargetCreated = 0;
				m_GumpCreated = 0;
				m_CastsStarted = 0;
				m_CastsFailed = 0;
				m_TotalMs = 0;
				m_MaxMs = 0;
				m_SlowCalls = 0;
				m_DiagnosticErrors = 0;
				m_StaleEntriesPruned = 0;
				m_LastThrottleUtc = DateTime.MinValue;
				m_LastSample = null;
				m_WorstSample = null;
			}
		}

		public static void WriteStats( TextWriter writer )
		{
			if ( writer == null )
				return;

			Sample last;
			Sample worst;
			List<EntityStat> players;
			List<EntityStat> items;
			long attempts;
			long allowed;
			long cooldown;
			long casting;
			long targeting;
			long inProgress;
			long invalid;
			long failedChecks;
			long spellCreated;
			long targetCreated;
			long gumpCreated;
			long castsStarted;
			long castsFailed;
			long totalMs;
			long maxMs;
			long slowCalls;
			long errors;
			long pruned;
			DateTime lastThrottle;
			int stateCount;

			lock ( m_Lock )
			{
				last = m_LastSample != null ? m_LastSample.Clone() : null;
				worst = m_WorstSample != null ? m_WorstSample.Clone() : null;
				players = CloneStatsLocked( m_PlayerStats );
				items = CloneStatsLocked( m_ItemStats );
				attempts = m_Attempts;
				allowed = m_Allowed;
				cooldown = m_ThrottledByCooldown;
				casting = m_ThrottledAlreadyCasting;
				targeting = m_ThrottledAlreadyTargeting;
				inProgress = m_ThrottledInProgress;
				invalid = m_InvalidState;
				failedChecks = m_FailedNormalChecks;
				spellCreated = m_SpellCreated;
				targetCreated = m_TargetCreated;
				gumpCreated = m_GumpCreated;
				castsStarted = m_CastsStarted;
				castsFailed = m_CastsFailed;
				totalMs = m_TotalMs;
				maxMs = m_MaxMs;
				slowCalls = m_SlowCalls;
				errors = m_DiagnosticErrors;
				pruned = m_StaleEntriesPruned;
				lastThrottle = m_LastThrottleUtc;
				stateCount = m_States.Count;
			}

			long avgMs = allowed > 0 ? totalMs / allowed : 0;
			writer.WriteLine( "RuneBagStats sinceBoot: cooldown={0}ms attempts={1} allowed={2} throttledByCooldown={3} throttledAlreadyCasting={4} throttledAlreadyTargeting={5} throttledInProgress={6} invalidState={7} failedNormalChecks={8} spellCreated={9} directTargetCreated={10} directGumpCreated={11} castsStarted={12} castsCompleted=Unavailable castsFailed={13} total={14}ms max={15}ms avg={16}ms slowCalls={17} throttleTableSize={18} staleEntriesPruned={19} diagnosticErrors={20} lastThrottleUtc={21}",
				CooldownMilliseconds, attempts, allowed, cooldown, casting, targeting, inProgress, invalid, failedChecks, spellCreated, targetCreated, gumpCreated, castsStarted, castsFailed, totalMs, maxMs, avgMs, slowCalls, stateCount, pruned, errors, FormatDate( lastThrottle ) );
			writer.WriteLine( "  note=RuneBag creates no Gump or Target directly; targets and cast completion remain owned by the normal spell lifecycle." );
			WriteSample( writer, "last", last );
			WriteSample( writer, "worst", worst );
			WriteTop( writer, "topPlayersByAttempts", players, false );
			WriteTop( writer, "topPlayersByThrottles", players, true );
			WriteTop( writer, "topItemsByAttempts", items, false );
			WriteTop( writer, "topItemsByThrottles", items, true );
		}

		private static Sample BuildSample( Attempt attempt, long totalMs )
		{
			Sample sample = new Sample();
			sample.StartedUtc = attempt.StartedUtc;
			sample.PlayerSerial = attempt.PlayerSerial;
			sample.ItemSerial = attempt.ItemSerial;
			sample.PlayerName = attempt.PlayerName;
			sample.AccountName = attempt.AccountName;
			sample.Source = attempt.Source;
			sample.TotalMs = totalMs;
			sample.EntryGuardMs = attempt.EntryGuardMs;
			sample.ContainerScanMs = attempt.ContainerScanMs;
			sample.RuneSelectionMs = attempt.RuneSelectionMs;
			sample.SpellCreationMs = attempt.SpellCreationMs;
			sample.TargetCreationMs = attempt.TargetCreationMs;
			sample.CastStartMs = attempt.CastStartMs;
			sample.GumpBuildMs = attempt.GumpBuildMs;
			sample.OtherMs = attempt.OtherMs;
			sample.SpellCreated = attempt.SpellCreated;
			sample.CastStarted = attempt.CastStarted;
			sample.CastFailed = attempt.CastFailed;
			sample.Error = !String.IsNullOrEmpty( attempt.Error ) ? attempt.Error : "none";
			sample.WorstPhase = "entryGuard";
			sample.WorstPhaseMs = sample.EntryGuardMs;
			SetWorstPhase( sample, "containerScan", sample.ContainerScanMs );
			SetWorstPhase( sample, "runeSelection", sample.RuneSelectionMs );
			SetWorstPhase( sample, "spellCreation", sample.SpellCreationMs );
			SetWorstPhase( sample, "targetCreation", sample.TargetCreationMs );
			SetWorstPhase( sample, "castStart", sample.CastStartMs );
			SetWorstPhase( sample, "gumpBuild", sample.GumpBuildMs );
			SetWorstPhase( sample, "other", sample.OtherMs );
			return sample;
		}

		private static void SetWorstPhase( Sample sample, string phase, long elapsedMs )
		{
			if ( elapsedMs > sample.WorstPhaseMs )
			{
				sample.WorstPhase = phase;
				sample.WorstPhaseMs = elapsedMs;
			}
		}

		private static void WriteSample( TextWriter writer, string label, Sample sample )
		{
			if ( sample == null )
			{
				writer.WriteLine( "RuneBagStats {0}: Not sampled", label );
				return;
			}

			writer.WriteLine( "RuneBagStats {0}: startedUtc={1} source={2} player=0x{3:X} name={4} account={5} bag=0x{6:X} total={7}ms worstPhase={8} worstPhaseMs={9} spellCreated={10} castStarted={11} castFailed={12} error={13}",
				label, sample.StartedUtc, sample.Source, sample.PlayerSerial, Safe( sample.PlayerName ), Safe( sample.AccountName ), sample.ItemSerial, sample.TotalMs, sample.WorstPhase, sample.WorstPhaseMs, sample.SpellCreated, sample.CastStarted, sample.CastFailed, Safe( sample.Error ) );
			writer.WriteLine( "  phases: entryGuard={0}ms containerScan={1}ms runeSelection={2}ms spellCreation={3}ms targetCreation={4}ms castStart={5}ms gumpBuild={6}ms other={7}ms",
				sample.EntryGuardMs, sample.ContainerScanMs, sample.RuneSelectionMs, sample.SpellCreationMs, sample.TargetCreationMs, sample.CastStartMs, sample.GumpBuildMs, sample.OtherMs );
		}

		private static void WriteTop( TextWriter writer, string label, List<EntityStat> stats, bool byThrottles )
		{
			stats.Sort( delegate( EntityStat left, EntityStat right )
			{
				long leftValue = byThrottles ? left.Throttles : left.Attempts;
				long rightValue = byThrottles ? right.Throttles : right.Attempts;
				int result = rightValue.CompareTo( leftValue );
				return result != 0 ? result : right.Attempts.CompareTo( left.Attempts );
			} );

			writer.Write( "  {0}:", label );
			int written = 0;

			for ( int i = 0; i < stats.Count && written < 5; ++i )
			{
				EntityStat stat = stats[i];
				long value = byThrottles ? stat.Throttles : stat.Attempts;

				if ( value <= 0 )
					continue;

				writer.Write( " {0}0x{1:X}/{2} attempts={3} throttles={4}", written > 0 ? "; " : " ", stat.Serial, Safe( stat.Name ), stat.Attempts, stat.Throttles );
				++written;
			}

			if ( written == 0 )
				writer.Write( " none" );

			writer.WriteLine();
		}

		private static EntityStat UpdateEntityLocked( Dictionary<int, EntityStat> stats, int maxCount, int serial, string name, string account, DateTime now )
		{
			EntityStat stat;

			if ( !stats.TryGetValue( serial, out stat ) )
			{
				if ( stats.Count >= maxCount )
					RemoveOldestLocked( stats );

				stat = new EntityStat();
				stat.Serial = serial;
				stats[serial] = stat;
			}

			stat.Name = !String.IsNullOrEmpty( name ) ? name : "unknown";
			stat.Account = !String.IsNullOrEmpty( account ) ? account : "none";
			stat.LastSeenUtc = now;
			++stat.Attempts;
			return stat;
		}

		private static void RemoveOldestLocked( Dictionary<int, EntityStat> stats )
		{
			int oldestKey = 0;
			DateTime oldest = DateTime.MaxValue;
			bool found = false;

			foreach ( KeyValuePair<int, EntityStat> entry in stats )
			{
				if ( entry.Value.LastSeenUtc < oldest )
				{
					oldest = entry.Value.LastSeenUtc;
					oldestKey = entry.Key;
					found = true;
				}
			}

			if ( found )
				stats.Remove( oldestKey );
		}

		private static void PruneLocked( DateTime now )
		{
			if ( m_LastPruneUtc != DateTime.MinValue && now - m_LastPruneUtc < PruneInterval )
				return;

			m_LastPruneUtc = now;
			m_StaleEntriesPruned += RemoveStaleStatesLocked( now );
			RemoveStaleStatsLocked( m_PlayerStats, now );
			RemoveStaleStatsLocked( m_ItemStats, now );
		}

		private static int RemoveStaleStatesLocked( DateTime now )
		{
			List<int> remove = null;

			foreach ( KeyValuePair<int, GuardState> entry in m_States )
			{
				if ( entry.Value.InProgress && ( now - entry.Value.InProgressUtc ).TotalMilliseconds >= InProgressTimeoutMilliseconds )
				{
					entry.Value.InProgress = false;
					entry.Value.ActiveAttemptId = 0;
				}

				if ( !entry.Value.InProgress && now - entry.Value.LastSeenUtc >= StateRetention )
				{
					if ( remove == null )
						remove = new List<int>();

					remove.Add( entry.Key );
				}
			}

			if ( remove == null )
				return 0;

			for ( int i = 0; i < remove.Count; ++i )
				m_States.Remove( remove[i] );

			return remove.Count;
		}

		private static void RemoveOldestStateLocked()
		{
			int oldestKey = 0;
			DateTime oldest = DateTime.MaxValue;
			bool found = false;

			foreach ( KeyValuePair<int, GuardState> entry in m_States )
			{
				if ( !entry.Value.InProgress && entry.Value.LastSeenUtc < oldest )
				{
					oldest = entry.Value.LastSeenUtc;
					oldestKey = entry.Key;
					found = true;
				}
			}

			if ( !found )
			{
				foreach ( KeyValuePair<int, GuardState> entry in m_States )
				{
					if ( entry.Value.LastSeenUtc < oldest )
					{
						oldest = entry.Value.LastSeenUtc;
						oldestKey = entry.Key;
						found = true;
					}
				}
			}

			if ( found )
				m_States.Remove( oldestKey );
		}

		private static void RemoveStaleStatsLocked( Dictionary<int, EntityStat> stats, DateTime now )
		{
			List<int> remove = null;

			foreach ( KeyValuePair<int, EntityStat> entry in stats )
			{
				if ( now - entry.Value.LastSeenUtc >= StatsRetention )
				{
					if ( remove == null )
						remove = new List<int>();

					remove.Add( entry.Key );
				}
			}

			if ( remove == null )
				return;

			for ( int i = 0; i < remove.Count; ++i )
				stats.Remove( remove[i] );
		}

		private static List<EntityStat> CloneStatsLocked( Dictionary<int, EntityStat> stats )
		{
			List<EntityStat> copy = new List<EntityStat>( stats.Count );

			foreach ( EntityStat stat in stats.Values )
				copy.Add( stat.Clone() );

			return copy;
		}

		private static long GetThrottleTotalLocked()
		{
			return m_ThrottledByCooldown + m_ThrottledAlreadyCasting + m_ThrottledAlreadyTargeting + m_ThrottledInProgress;
		}

		private static string GetPlayerName( Mobile player )
		{
			return player != null && !String.IsNullOrEmpty( player.Name ) ? player.Name : "unknown";
		}

		private static string GetAccountName( Mobile player )
		{
			return player != null && player.Account != null && !String.IsNullOrEmpty( player.Account.Username ) ? player.Account.Username : "none";
		}

		private static string Safe( string value )
		{
			return !String.IsNullOrEmpty( value ) ? value.Replace( '\r', ' ' ).Replace( '\n', ' ' ) : "none";
		}

		private static string FormatDate( DateTime value )
		{
			return value == DateTime.MinValue ? "Unavailable" : value.ToString( "u" );
		}
	}
}
