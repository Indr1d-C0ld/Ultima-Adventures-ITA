/***************************************************************************
 *                   PlayerLocationDiagnostics.cs
 *                   ----------------------------
 *   Attribution for PlayerMobile.OnLocationChange movement costs.
 ***************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using Server.Accounting;

namespace Server.Diagnostics
{
	public static class PlayerLocationDiagnostics
	{
		private const long SummaryInterval = 30000;
		private const long SlowPlayerLocationThreshold = 100;
		private const long MeaningfulWindowThreshold = 1000;

		private static readonly object m_Lock = new object();
		private static readonly WindowStats m_Current = new WindowStats();
		private static readonly WindowStats m_Last = new WindowStats();
		private static long m_NextSummary;

		public struct PlayerLocationSample
		{
			public Mobile Mobile;
			public Map Map;
			public Point3D StartLocation;
			public Point3D EndLocation;
			public bool Movement;
			public long TotalMs;

			public long RegionNameMs;
			public long MainRegionTransitionMs;
			public long RegionMusicMs;
			public long SoulboundPartyMs;
			public long HouseLookupMs;
			public long HouseRefreshMs;
			public long RegionFindMs;
			public long NoMountRegionMs;
			public long IsBeingFastMs;
			public long GetLastMountedMs;
			public long DismountPlayerMs;
			public long AllowMagicSpeedMs;
			public long SpeedDisableMs;
			public long SpeedEnableMs;
			public long SpellSpeedContextMs;
			public long SpellSpeedRemoveMs;
			public long HenchmanMs;
			public long CheckLightMs;
			public long DesignContextMs;
			public long DesignRelocateMs;
			public long OtherMs;

			public int RegionNameCalls;
			public int RegionChangedCount;
			public int MainRegionEnterCount;
			public int HouseLookupCount;
			public int HouseRefreshCount;
			public int RegionFindCount;
			public int NoMountChecks;
			public int IsBeingFastCalls;
			public int FastCheckSkippedCount;
			public int GetLastMountedCalls;
			public int LastMountSkippedCount;
			public int LastMountAttemptCount;
			public int LastMountPendingCount;
			public int LastMountWorldScanAvoidedCount;
			public int AllowMagicSpeedChecks;
			public int SpeedDisableCount;
			public int SpeedEnableCount;
			public int HenchmanMountChecks;
			public int LightChecks;
			public int DesignContextChecks;
			public int DesignRelocations;
			public int SameRegionCount;
			public int HouseNullCount;
			public int NoSpeedChangeCount;
		}

		private sealed class WindowStats
		{
			public int Calls;
			public int MovementCalls;
			public int SlowCalls;
			public long TotalMs;
			public long MaxMs;
			public long LastMs;

			public long RegionNameMs;
			public long MainRegionTransitionMs;
			public long RegionMusicMs;
			public long SoulboundPartyMs;
			public long HouseLookupMs;
			public long HouseRefreshMs;
			public long RegionFindMs;
			public long NoMountRegionMs;
			public long IsBeingFastMs;
			public long GetLastMountedMs;
			public long DismountPlayerMs;
			public long AllowMagicSpeedMs;
			public long SpeedDisableMs;
			public long SpeedEnableMs;
			public long SpellSpeedContextMs;
			public long SpellSpeedRemoveMs;
			public long HenchmanMs;
			public long CheckLightMs;
			public long DesignContextMs;
			public long DesignRelocateMs;
			public long OtherMs;

			public int RegionNameCalls;
			public int RegionChangedCount;
			public int MainRegionEnterCount;
			public int HouseLookupCount;
			public int HouseRefreshCount;
			public int RegionFindCount;
			public int NoMountChecks;
			public int IsBeingFastCalls;
			public int FastCheckSkippedCount;
			public int GetLastMountedCalls;
			public int LastMountSkippedCount;
			public int LastMountAttemptCount;
			public int LastMountPendingCount;
			public int LastMountWorldScanAvoidedCount;
			public int AllowMagicSpeedChecks;
			public int SpeedDisableCount;
			public int SpeedEnableCount;
			public int HenchmanMountChecks;
			public int LightChecks;
			public int DesignContextChecks;
			public int DesignRelocations;
			public int SameRegionCount;
			public int HouseNullCount;
			public int NoSpeedChangeCount;

			public string MaxPhaseName;
			public long MaxPhaseMs;
			public string WorstMobile;
			public string WorstAccount;
			public string WorstMap;
			public Point3D WorstStart;
			public Point3D WorstEnd;
			public bool WorstMovement;
			public string WorstTopPhase;
			public long WorstTopPhaseMs;

			public void Reset()
			{
				Calls = 0;
				MovementCalls = 0;
				SlowCalls = 0;
				TotalMs = 0;
				MaxMs = 0;
				LastMs = 0;

				RegionNameMs = 0;
				MainRegionTransitionMs = 0;
				RegionMusicMs = 0;
				SoulboundPartyMs = 0;
				HouseLookupMs = 0;
				HouseRefreshMs = 0;
				RegionFindMs = 0;
				NoMountRegionMs = 0;
				IsBeingFastMs = 0;
				GetLastMountedMs = 0;
				DismountPlayerMs = 0;
				AllowMagicSpeedMs = 0;
				SpeedDisableMs = 0;
				SpeedEnableMs = 0;
				SpellSpeedContextMs = 0;
				SpellSpeedRemoveMs = 0;
				HenchmanMs = 0;
				CheckLightMs = 0;
				DesignContextMs = 0;
				DesignRelocateMs = 0;
				OtherMs = 0;

				RegionNameCalls = 0;
				RegionChangedCount = 0;
				MainRegionEnterCount = 0;
				HouseLookupCount = 0;
				HouseRefreshCount = 0;
				RegionFindCount = 0;
				NoMountChecks = 0;
				IsBeingFastCalls = 0;
				FastCheckSkippedCount = 0;
				GetLastMountedCalls = 0;
				LastMountSkippedCount = 0;
				LastMountAttemptCount = 0;
				LastMountPendingCount = 0;
				LastMountWorldScanAvoidedCount = 0;
				AllowMagicSpeedChecks = 0;
				SpeedDisableCount = 0;
				SpeedEnableCount = 0;
				HenchmanMountChecks = 0;
				LightChecks = 0;
				DesignContextChecks = 0;
				DesignRelocations = 0;
				SameRegionCount = 0;
				HouseNullCount = 0;
				NoSpeedChangeCount = 0;

				MaxPhaseName = null;
				MaxPhaseMs = 0;
				WorstMobile = null;
				WorstAccount = null;
				WorstMap = null;
				WorstStart = Point3D.Zero;
				WorstEnd = Point3D.Zero;
				WorstMovement = false;
				WorstTopPhase = null;
				WorstTopPhaseMs = 0;
			}

			public void CopyFrom( WindowStats other )
			{
				Calls = other.Calls;
				MovementCalls = other.MovementCalls;
				SlowCalls = other.SlowCalls;
				TotalMs = other.TotalMs;
				MaxMs = other.MaxMs;
				LastMs = other.LastMs;

				RegionNameMs = other.RegionNameMs;
				MainRegionTransitionMs = other.MainRegionTransitionMs;
				RegionMusicMs = other.RegionMusicMs;
				SoulboundPartyMs = other.SoulboundPartyMs;
				HouseLookupMs = other.HouseLookupMs;
				HouseRefreshMs = other.HouseRefreshMs;
				RegionFindMs = other.RegionFindMs;
				NoMountRegionMs = other.NoMountRegionMs;
				IsBeingFastMs = other.IsBeingFastMs;
				GetLastMountedMs = other.GetLastMountedMs;
				DismountPlayerMs = other.DismountPlayerMs;
				AllowMagicSpeedMs = other.AllowMagicSpeedMs;
				SpeedDisableMs = other.SpeedDisableMs;
				SpeedEnableMs = other.SpeedEnableMs;
				SpellSpeedContextMs = other.SpellSpeedContextMs;
				SpellSpeedRemoveMs = other.SpellSpeedRemoveMs;
				HenchmanMs = other.HenchmanMs;
				CheckLightMs = other.CheckLightMs;
				DesignContextMs = other.DesignContextMs;
				DesignRelocateMs = other.DesignRelocateMs;
				OtherMs = other.OtherMs;

				RegionNameCalls = other.RegionNameCalls;
				RegionChangedCount = other.RegionChangedCount;
				MainRegionEnterCount = other.MainRegionEnterCount;
				HouseLookupCount = other.HouseLookupCount;
				HouseRefreshCount = other.HouseRefreshCount;
				RegionFindCount = other.RegionFindCount;
				NoMountChecks = other.NoMountChecks;
				IsBeingFastCalls = other.IsBeingFastCalls;
				FastCheckSkippedCount = other.FastCheckSkippedCount;
				GetLastMountedCalls = other.GetLastMountedCalls;
				LastMountSkippedCount = other.LastMountSkippedCount;
				LastMountAttemptCount = other.LastMountAttemptCount;
				LastMountPendingCount = other.LastMountPendingCount;
				LastMountWorldScanAvoidedCount = other.LastMountWorldScanAvoidedCount;
				AllowMagicSpeedChecks = other.AllowMagicSpeedChecks;
				SpeedDisableCount = other.SpeedDisableCount;
				SpeedEnableCount = other.SpeedEnableCount;
				HenchmanMountChecks = other.HenchmanMountChecks;
				LightChecks = other.LightChecks;
				DesignContextChecks = other.DesignContextChecks;
				DesignRelocations = other.DesignRelocations;
				SameRegionCount = other.SameRegionCount;
				HouseNullCount = other.HouseNullCount;
				NoSpeedChangeCount = other.NoSpeedChangeCount;

				MaxPhaseName = other.MaxPhaseName;
				MaxPhaseMs = other.MaxPhaseMs;
				WorstMobile = other.WorstMobile;
				WorstAccount = other.WorstAccount;
				WorstMap = other.WorstMap;
				WorstStart = other.WorstStart;
				WorstEnd = other.WorstEnd;
				WorstMovement = other.WorstMovement;
				WorstTopPhase = other.WorstTopPhase;
				WorstTopPhaseMs = other.WorstTopPhaseMs;
			}
		}

		public static long GetTimestamp()
		{
			return Stopwatch.GetTimestamp();
		}

		public static long GetElapsedMilliseconds( long start )
		{
			return ( Stopwatch.GetTimestamp() - start ) * 1000 / Stopwatch.Frequency;
		}

		public static PlayerLocationSample CreateSample( Mobile mobile, Point3D oldLocation )
		{
			PlayerLocationSample sample = new PlayerLocationSample();

			sample.Mobile = mobile;
			sample.Map = mobile != null ? mobile.Map : null;
			sample.StartLocation = oldLocation;
			sample.EndLocation = mobile != null ? mobile.Location : Point3D.Zero;
			sample.Movement = SetLocationDiagnostics.IsMovementSetLocation( mobile );

			return sample;
		}

		public static void Record( ref PlayerLocationSample sample, long start )
		{
			sample.TotalMs = GetElapsedMilliseconds( start );
			sample.EndLocation = sample.Mobile != null ? sample.Mobile.Location : sample.EndLocation;
			sample.OtherMs = sample.TotalMs - GetAccountedMs( ref sample );

			if ( sample.OtherMs < 0 )
				sample.OtherMs = 0;

			bool logSlow = false;
			bool logSummary = false;

			lock ( m_Lock )
			{
				++m_Current.Calls;
				m_Current.TotalMs += sample.TotalMs;
				m_Current.LastMs = sample.TotalMs;

				if ( sample.Movement )
					++m_Current.MovementCalls;

				if ( sample.TotalMs > m_Current.MaxMs )
				{
					m_Current.MaxMs = sample.TotalMs;
					SetWorstContext( m_Current, ref sample );
				}

				if ( sample.TotalMs >= SlowPlayerLocationThreshold )
				{
					++m_Current.SlowCalls;
					logSlow = true;
				}

				AddPhases( m_Current, ref sample );
				AddCounts( m_Current, ref sample );
				UpdateMaxPhase( m_Current, ref sample );

				long now = Core.TickCount;

				if ( m_NextSummary == 0 )
				{
					m_NextSummary = now + SummaryInterval;
				}
				else if ( now >= m_NextSummary )
				{
					m_NextSummary = now + SummaryInterval;
					m_Last.CopyFrom( m_Current );
					logSummary = ( m_Current.TotalMs >= MeaningfulWindowThreshold || m_Current.SlowCalls > 0 );
					m_Current.Reset();
				}
			}

			if ( logSlow )
				LogSlowPlayerLocation( ref sample );

			if ( logSummary )
				LogWindow( m_Last );
		}

		private static long GetAccountedMs( ref PlayerLocationSample sample )
		{
			return sample.RegionNameMs +
				sample.MainRegionTransitionMs +
				sample.RegionMusicMs +
				sample.SoulboundPartyMs +
				sample.HouseLookupMs +
				sample.HouseRefreshMs +
				sample.RegionFindMs +
				sample.NoMountRegionMs +
				sample.IsBeingFastMs +
				sample.GetLastMountedMs +
				sample.DismountPlayerMs +
				sample.AllowMagicSpeedMs +
				sample.SpeedDisableMs +
				sample.SpeedEnableMs +
				sample.SpellSpeedContextMs +
				sample.SpellSpeedRemoveMs +
				sample.HenchmanMs +
				sample.CheckLightMs +
				sample.DesignContextMs +
				sample.DesignRelocateMs;
		}

		private static void AddPhases( WindowStats stats, ref PlayerLocationSample sample )
		{
			stats.RegionNameMs += sample.RegionNameMs;
			stats.MainRegionTransitionMs += sample.MainRegionTransitionMs;
			stats.RegionMusicMs += sample.RegionMusicMs;
			stats.SoulboundPartyMs += sample.SoulboundPartyMs;
			stats.HouseLookupMs += sample.HouseLookupMs;
			stats.HouseRefreshMs += sample.HouseRefreshMs;
			stats.RegionFindMs += sample.RegionFindMs;
			stats.NoMountRegionMs += sample.NoMountRegionMs;
			stats.IsBeingFastMs += sample.IsBeingFastMs;
			stats.GetLastMountedMs += sample.GetLastMountedMs;
			stats.DismountPlayerMs += sample.DismountPlayerMs;
			stats.AllowMagicSpeedMs += sample.AllowMagicSpeedMs;
			stats.SpeedDisableMs += sample.SpeedDisableMs;
			stats.SpeedEnableMs += sample.SpeedEnableMs;
			stats.SpellSpeedContextMs += sample.SpellSpeedContextMs;
			stats.SpellSpeedRemoveMs += sample.SpellSpeedRemoveMs;
			stats.HenchmanMs += sample.HenchmanMs;
			stats.CheckLightMs += sample.CheckLightMs;
			stats.DesignContextMs += sample.DesignContextMs;
			stats.DesignRelocateMs += sample.DesignRelocateMs;
			stats.OtherMs += sample.OtherMs;
		}

		private static void AddCounts( WindowStats stats, ref PlayerLocationSample sample )
		{
			stats.RegionNameCalls += sample.RegionNameCalls;
			stats.RegionChangedCount += sample.RegionChangedCount;
			stats.MainRegionEnterCount += sample.MainRegionEnterCount;
			stats.HouseLookupCount += sample.HouseLookupCount;
			stats.HouseRefreshCount += sample.HouseRefreshCount;
			stats.RegionFindCount += sample.RegionFindCount;
			stats.NoMountChecks += sample.NoMountChecks;
			stats.IsBeingFastCalls += sample.IsBeingFastCalls;
			stats.FastCheckSkippedCount += sample.FastCheckSkippedCount;
			stats.GetLastMountedCalls += sample.GetLastMountedCalls;
			stats.LastMountSkippedCount += sample.LastMountSkippedCount;
			stats.LastMountAttemptCount += sample.LastMountAttemptCount;
			stats.LastMountPendingCount += sample.LastMountPendingCount;
			stats.LastMountWorldScanAvoidedCount += sample.LastMountWorldScanAvoidedCount;
			stats.AllowMagicSpeedChecks += sample.AllowMagicSpeedChecks;
			stats.SpeedDisableCount += sample.SpeedDisableCount;
			stats.SpeedEnableCount += sample.SpeedEnableCount;
			stats.HenchmanMountChecks += sample.HenchmanMountChecks;
			stats.LightChecks += sample.LightChecks;
			stats.DesignContextChecks += sample.DesignContextChecks;
			stats.DesignRelocations += sample.DesignRelocations;
			stats.SameRegionCount += sample.SameRegionCount;
			stats.HouseNullCount += sample.HouseNullCount;
			stats.NoSpeedChangeCount += sample.NoSpeedChangeCount;
		}

		private static void UpdateMaxPhase( WindowStats stats, ref PlayerLocationSample sample )
		{
			string name;
			long ms;

			GetTopPhase( ref sample, out name, out ms );

			if ( ms > stats.MaxPhaseMs )
			{
				stats.MaxPhaseMs = ms;
				stats.MaxPhaseName = name;
			}
		}

		private static void SetWorstContext( WindowStats stats, ref PlayerLocationSample sample )
		{
			string topName;
			long topMs;

			GetTopPhase( ref sample, out topName, out topMs );

			stats.WorstMobile = GetMobileText( sample.Mobile );
			stats.WorstAccount = GetAccountText( sample.Mobile );
			stats.WorstMap = sample.Map != null ? sample.Map.ToString() : "null";
			stats.WorstStart = sample.StartLocation;
			stats.WorstEnd = sample.EndLocation;
			stats.WorstMovement = sample.Movement;
			stats.WorstTopPhase = topName;
			stats.WorstTopPhaseMs = topMs;
		}

		private static void GetTopPhase( ref PlayerLocationSample sample, out string name, out long ms )
		{
			name = "none";
			ms = 0;

			ConsiderPhase( "regionName", sample.RegionNameMs, ref name, ref ms );
			ConsiderPhase( "mainRegion", sample.MainRegionTransitionMs, ref name, ref ms );
			ConsiderPhase( "music", sample.RegionMusicMs, ref name, ref ms );
			ConsiderPhase( "soulbound", sample.SoulboundPartyMs, ref name, ref ms );
			ConsiderPhase( "houseLookup", sample.HouseLookupMs, ref name, ref ms );
			ConsiderPhase( "houseRefresh", sample.HouseRefreshMs, ref name, ref ms );
			ConsiderPhase( "regionFind", sample.RegionFindMs, ref name, ref ms );
			ConsiderPhase( "noMount", sample.NoMountRegionMs, ref name, ref ms );
			ConsiderPhase( "isBeingFast", sample.IsBeingFastMs, ref name, ref ms );
			ConsiderPhase( "getLastMounted", sample.GetLastMountedMs, ref name, ref ms );
			ConsiderPhase( "dismount", sample.DismountPlayerMs, ref name, ref ms );
			ConsiderPhase( "allowSpeed", sample.AllowMagicSpeedMs, ref name, ref ms );
			ConsiderPhase( "speedDisable", sample.SpeedDisableMs, ref name, ref ms );
			ConsiderPhase( "speedEnable", sample.SpeedEnableMs, ref name, ref ms );
			ConsiderPhase( "speedContext", sample.SpellSpeedContextMs, ref name, ref ms );
			ConsiderPhase( "speedRemove", sample.SpellSpeedRemoveMs, ref name, ref ms );
			ConsiderPhase( "henchman", sample.HenchmanMs, ref name, ref ms );
			ConsiderPhase( "light", sample.CheckLightMs, ref name, ref ms );
			ConsiderPhase( "designContext", sample.DesignContextMs, ref name, ref ms );
			ConsiderPhase( "designRelocate", sample.DesignRelocateMs, ref name, ref ms );
			ConsiderPhase( "other", sample.OtherMs, ref name, ref ms );
		}

		private static string GetTopTotalPhase( WindowStats stats )
		{
			string name = "none";
			long ms = 0;

			ConsiderPhase( "regionName", stats.RegionNameMs, ref name, ref ms );
			ConsiderPhase( "mainRegion", stats.MainRegionTransitionMs, ref name, ref ms );
			ConsiderPhase( "music", stats.RegionMusicMs, ref name, ref ms );
			ConsiderPhase( "soulbound", stats.SoulboundPartyMs, ref name, ref ms );
			ConsiderPhase( "houseLookup", stats.HouseLookupMs, ref name, ref ms );
			ConsiderPhase( "houseRefresh", stats.HouseRefreshMs, ref name, ref ms );
			ConsiderPhase( "regionFind", stats.RegionFindMs, ref name, ref ms );
			ConsiderPhase( "noMount", stats.NoMountRegionMs, ref name, ref ms );
			ConsiderPhase( "isBeingFast", stats.IsBeingFastMs, ref name, ref ms );
			ConsiderPhase( "getLastMounted", stats.GetLastMountedMs, ref name, ref ms );
			ConsiderPhase( "dismount", stats.DismountPlayerMs, ref name, ref ms );
			ConsiderPhase( "allowSpeed", stats.AllowMagicSpeedMs, ref name, ref ms );
			ConsiderPhase( "speedDisable", stats.SpeedDisableMs, ref name, ref ms );
			ConsiderPhase( "speedEnable", stats.SpeedEnableMs, ref name, ref ms );
			ConsiderPhase( "speedContext", stats.SpellSpeedContextMs, ref name, ref ms );
			ConsiderPhase( "speedRemove", stats.SpellSpeedRemoveMs, ref name, ref ms );
			ConsiderPhase( "henchman", stats.HenchmanMs, ref name, ref ms );
			ConsiderPhase( "light", stats.CheckLightMs, ref name, ref ms );
			ConsiderPhase( "designContext", stats.DesignContextMs, ref name, ref ms );
			ConsiderPhase( "designRelocate", stats.DesignRelocateMs, ref name, ref ms );
			ConsiderPhase( "other", stats.OtherMs, ref name, ref ms );

			return String.Format( "{0}:{1}ms", name, ms );
		}

		private static void ConsiderPhase( string phaseName, long phaseMs, ref string name, ref long ms )
		{
			if ( phaseMs > ms )
			{
				name = phaseName;
				ms = phaseMs;
			}
		}

		private static string GetMobileText( Mobile mobile )
		{
			if ( mobile == null )
				return "null";

			return String.Format( "{0} \"{1}\"", mobile.Serial, mobile.Name );
		}

		private static string GetAccountText( Mobile mobile )
		{
			if ( mobile == null || mobile.Account == null )
				return "null";

			IAccount account = mobile.Account;

			return account.Username;
		}

		private static void LogSlowPlayerLocation( ref PlayerLocationSample sample )
		{
			string topName;
			long topMs;

			GetTopPhase( ref sample, out topName, out topMs );

			DiagnosticConsole.WriteParsedLine( "[Lag] PlayerLocation slow {0}ms mobile={1} account={2} map={3} start={4} end={5} movement={6} top={7}:{8}ms",
				sample.TotalMs,
				GetMobileText( sample.Mobile ),
				GetAccountText( sample.Mobile ),
				sample.Map,
				sample.StartLocation,
				sample.EndLocation,
				sample.Movement,
				topName,
				topMs );

			DiagnosticConsole.WriteParsedLine( "[Lag] PlayerLocation detail regionName={0} main={1} music={2} soulbound={3} houseLookup={4} houseRefresh={5} regionFind={6} noMount={7} isBeingFast={8} getLastMounted={9} dismount={10} allowSpeed={11} speedDisable={12} speedEnable={13} speedContext={14} speedRemove={15} henchman={16} light={17} design={18} designMove={19} other={20}",
				sample.RegionNameMs,
				sample.MainRegionTransitionMs,
				sample.RegionMusicMs,
				sample.SoulboundPartyMs,
				sample.HouseLookupMs,
				sample.HouseRefreshMs,
				sample.RegionFindMs,
				sample.NoMountRegionMs,
				sample.IsBeingFastMs,
				sample.GetLastMountedMs,
				sample.DismountPlayerMs,
				sample.AllowMagicSpeedMs,
				sample.SpeedDisableMs,
				sample.SpeedEnableMs,
				sample.SpellSpeedContextMs,
				sample.SpellSpeedRemoveMs,
				sample.HenchmanMs,
				sample.CheckLightMs,
				sample.DesignContextMs,
				sample.DesignRelocateMs,
				sample.OtherMs );
		}

		private static void LogWindow( WindowStats stats )
		{
			long avg = stats.Calls > 0 ? stats.TotalMs / stats.Calls : 0;
			int speedChanges = stats.SpeedDisableCount + stats.SpeedEnableCount;

			DiagnosticConsole.WriteParsedLine( "[Lag] PlayerLocationStats 30s: calls={0} movement={1} avg={2}ms max={3}ms slow={4} topTotal={5} topMax={6}:{7}ms regionName={8}ms houseLookup={9}ms noMount={10}ms isBeingFast={11}ms getLastMounted={12}ms speed={13}ms light={14}ms design={15}ms other={16}ms",
				stats.Calls,
				stats.MovementCalls,
				avg,
				stats.MaxMs,
				stats.SlowCalls,
				GetTopTotalPhase( stats ),
				stats.MaxPhaseName,
				stats.MaxPhaseMs,
				stats.RegionNameMs,
				stats.HouseLookupMs,
				stats.NoMountRegionMs,
				stats.IsBeingFastMs,
				stats.GetLastMountedMs,
				stats.AllowMagicSpeedMs + stats.SpeedDisableMs + stats.SpeedEnableMs + stats.SpellSpeedContextMs + stats.SpellSpeedRemoveMs,
				stats.CheckLightMs,
				stats.DesignContextMs + stats.DesignRelocateMs,
				stats.OtherMs );

			DiagnosticConsole.WriteParsedLine( "[Lag] PlayerLocationCounts 30s: regionNameCalls={0} sameRegion={1} regionChanged={2} mainEnter={3} houseLookups={4} houseNull={5} houseRefresh={6} regionFinds={7} noMountChecks={8} isBeingFastCalls={9} fastSkipped={10} lastMountPending={11} lastMountAttempts={12} lastMountSkipped={13} lastMountWorldScanAvoided={14} allowSpeedChecks={15} speedChanges={16} lightChecks={17} designChecks={18} designRelocations={19} noSpeedChange={20} worst={21} account={22} map={23} start={24} end={25} top={26}:{27}ms",
				stats.RegionNameCalls,
				stats.SameRegionCount,
				stats.RegionChangedCount,
				stats.MainRegionEnterCount,
				stats.HouseLookupCount,
				stats.HouseNullCount,
				stats.HouseRefreshCount,
				stats.RegionFindCount,
				stats.NoMountChecks,
				stats.IsBeingFastCalls,
				stats.FastCheckSkippedCount,
				stats.LastMountPendingCount,
				stats.LastMountAttemptCount,
				stats.LastMountSkippedCount,
				stats.LastMountWorldScanAvoidedCount,
				stats.AllowMagicSpeedChecks,
				speedChanges,
				stats.LightChecks,
				stats.DesignContextChecks,
				stats.DesignRelocations,
				stats.NoSpeedChangeCount,
				stats.WorstMobile,
				stats.WorstAccount,
				stats.WorstMap,
				stats.WorstStart,
				stats.WorstEnd,
				stats.WorstTopPhase,
				stats.WorstTopPhaseMs );
		}

		public static void WriteStats( TextWriter writer )
		{
			if ( writer == null )
				return;

			lock ( m_Lock )
			{
				WriteWindow( writer, "Current PlayerLocation window", m_Current );
				WriteWindow( writer, "Last PlayerLocation window", m_Last );
			}
		}

		private static void WriteWindow( TextWriter writer, string label, WindowStats stats )
		{
			long avg = stats.Calls > 0 ? stats.TotalMs / stats.Calls : 0;
			int speedChanges = stats.SpeedDisableCount + stats.SpeedEnableCount;

			writer.WriteLine( "{0}: calls={1} movement={2} slow={3} avg={4}ms max={5}ms last={6}ms topTotal={7} topMax={8}:{9}ms",
				label,
				stats.Calls,
				stats.MovementCalls,
				stats.SlowCalls,
				avg,
				stats.MaxMs,
				stats.LastMs,
				GetTopTotalPhase( stats ),
				stats.MaxPhaseName,
				stats.MaxPhaseMs );

			writer.WriteLine( "  phases total: regionName={0} main={1} music={2} soulbound={3} houseLookup={4} houseRefresh={5} regionFind={6} noMount={7} isBeingFast={8} getLastMounted={9} dismount={10} allowSpeed={11} speedDisable={12} speedEnable={13} speedContext={14} speedRemove={15} henchman={16} light={17} designContext={18} designRelocate={19} other={20}",
				stats.RegionNameMs,
				stats.MainRegionTransitionMs,
				stats.RegionMusicMs,
				stats.SoulboundPartyMs,
				stats.HouseLookupMs,
				stats.HouseRefreshMs,
				stats.RegionFindMs,
				stats.NoMountRegionMs,
				stats.IsBeingFastMs,
				stats.GetLastMountedMs,
				stats.DismountPlayerMs,
				stats.AllowMagicSpeedMs,
				stats.SpeedDisableMs,
				stats.SpeedEnableMs,
				stats.SpellSpeedContextMs,
				stats.SpellSpeedRemoveMs,
				stats.HenchmanMs,
				stats.CheckLightMs,
				stats.DesignContextMs,
				stats.DesignRelocateMs,
				stats.OtherMs );

			writer.WriteLine( "  counts: regionNameCalls={0} sameRegion={1} regionChanged={2} mainEnter={3} houseLookups={4} houseNull={5} houseRefresh={6} regionFinds={7} noMountChecks={8} isBeingFastCalls={9} fastSkipped={10} getLastMountedCalls={11} lastMountPending={12} lastMountAttempts={13} lastMountSkipped={14} lastMountWorldScanAvoided={15} allowSpeedChecks={16} speedDisable={17} speedEnable={18} speedChanges={19} henchmanChecks={20} lightChecks={21} designChecks={22} designRelocations={23} noSpeedChange={24}",
				stats.RegionNameCalls,
				stats.SameRegionCount,
				stats.RegionChangedCount,
				stats.MainRegionEnterCount,
				stats.HouseLookupCount,
				stats.HouseNullCount,
				stats.HouseRefreshCount,
				stats.RegionFindCount,
				stats.NoMountChecks,
				stats.IsBeingFastCalls,
				stats.FastCheckSkippedCount,
				stats.GetLastMountedCalls,
				stats.LastMountPendingCount,
				stats.LastMountAttemptCount,
				stats.LastMountSkippedCount,
				stats.LastMountWorldScanAvoidedCount,
				stats.AllowMagicSpeedChecks,
				stats.SpeedDisableCount,
				stats.SpeedEnableCount,
				speedChanges,
				stats.HenchmanMountChecks,
				stats.LightChecks,
				stats.DesignContextChecks,
				stats.DesignRelocations,
				stats.NoSpeedChangeCount );

			writer.WriteLine( "  worst: mobile={0} account={1} map={2} start={3} end={4} movement={5} top={6}:{7}ms",
				stats.WorstMobile,
				stats.WorstAccount,
				stats.WorstMap,
				stats.WorstStart,
				stats.WorstEnd,
				stats.WorstMovement,
				stats.WorstTopPhase,
				stats.WorstTopPhaseMs );
		}
	}
}
