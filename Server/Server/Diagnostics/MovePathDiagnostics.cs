/***************************************************************************
 *                       MovePathDiagnostics.cs
 *                       ----------------------
 *   Low-overhead attribution for Mobile.Move hot-path costs.
 ***************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using Server.Accounting;

namespace Server.Diagnostics
{
	public static class MovePathDiagnostics
	{
		public const string ReasonDeleted = "Deleted";
		public const string ReasonSpellBlocked = "SpellBlocked";
		public const string ReasonFrozenParalyzed = "FrozenParalyzed";
		public const string ReasonCheckMovementFailed = "CheckMovementFailed";
		public const string ReasonMoveOffMobileBlocked = "MoveOffMobileBlocked";
		public const string ReasonMoveOffItemBlocked = "MoveOffItemBlocked";
		public const string ReasonMoveOverMobileBlocked = "MoveOverMobileBlocked";
		public const string ReasonMoveOverItemBlocked = "MoveOverItemBlocked";
		public const string ReasonRegionBlocked = "RegionBlocked";
		public const string ReasonInternalOnMoveBlocked = "InternalOnMoveBlocked";
		public const string ReasonFastwalkBlocked = "FastwalkBlocked";
		public const string ReasonNoMap = "NoMap";
		public const string ReasonSuccess = "Success";
		public const string ReasonDirectionOnly = "DirectionOnly";

		private const long SummaryInterval = 30000;
		private const long SlowMoveThreshold = 150;

		private static readonly object m_Lock = new object();
		private static readonly WindowStats m_Current = new WindowStats();
		private static readonly WindowStats m_Last = new WindowStats();
		private static long m_NextSummary;

		public struct MovePathSample
		{
			public Mobile Mobile;
			public Map Map;
			public Point3D StartLocation;
			public Point3D EndLocation;
			public Direction Direction;
			public bool Accepted;
			public bool AITimerContext;
			public string RejectedReason;
			public long TotalMs;

			public long PrecheckMs;
			public long CheckMovementMs;
			public long CoordinateMs;
			public long SectorLookupMs;
			public long MoveOffMobileMs;
			public long MoveOffItemMs;
			public long MoveOverMobileMs;
			public long MoveOverItemMs;
			public long RegionCanMoveMs;
			public long InternalOnMoveMs;
			public long FastwalkMs;
			public long DisruptiveActionMs;
			public long AckSendMs;
			public long SetLocationMs;
			public long SetDirectionMs;
			public long GetObjectsInRangeMs;
			public long BuildMoveListsMs;
			public long SendMovingPacketsMs;
			public long VisibilityMs;
			public long NotorietyMs;
			public long MovingPacketSendMs;
			public long OnMovementMobileMs;
			public long OnMovementItemMs;
			public long OnAfterMoveMs;
			public long OtherMs;

			public int OldSectorMobileCount;
			public int OldSectorItemCount;
			public int NewSectorMobileCount;
			public int NewSectorItemCount;
			public int ObjectsInRangeCount;
			public int MoveClientCount;
			public int MoveListCount;
			public int MobileOnMovementCount;
			public int ItemOnMovementCount;
			public int HandlesOnMovementItemCount;
			public int VisibilityChecks;
			public int CanSeeChecks;
			public int InLOSChecks;
			public int MobileMovingPacketsSent;
		}

		private sealed class WindowStats
		{
			public int Moves;
			public int Accepted;
			public int Rejected;
			public int SlowMoves;
			public long TotalMs;
			public long MaxMs;
			public long LastMs;

			public long PrecheckMs;
			public long CheckMovementMs;
			public long CoordinateMs;
			public long SectorLookupMs;
			public long MoveOffMobileMs;
			public long MoveOffItemMs;
			public long MoveOverMobileMs;
			public long MoveOverItemMs;
			public long RegionCanMoveMs;
			public long InternalOnMoveMs;
			public long FastwalkMs;
			public long DisruptiveActionMs;
			public long AckSendMs;
			public long SetLocationMs;
			public long SetDirectionMs;
			public long GetObjectsInRangeMs;
			public long BuildMoveListsMs;
			public long SendMovingPacketsMs;
			public long VisibilityMs;
			public long NotorietyMs;
			public long MovingPacketSendMs;
			public long OnMovementMobileMs;
			public long OnMovementItemMs;
			public long OnAfterMoveMs;
			public long OtherMs;

			public long MaxPhaseMs;
			public string MaxPhaseName;
			public string WorstMobile;
			public string WorstAccount;
			public string WorstMap;
			public Point3D WorstStart;
			public Point3D WorstEnd;
			public Direction WorstDirection;
			public bool WorstAccepted;
			public bool WorstAITimerContext;
			public string WorstReason;
			public string WorstTopPhase;
			public long WorstTopPhaseMs;

			public long OldSectorMobileCount;
			public long OldSectorItemCount;
			public long NewSectorMobileCount;
			public long NewSectorItemCount;
			public long ObjectsInRangeCount;
			public long MoveClientCount;
			public long MoveListCount;
			public long MobileOnMovementCount;
			public long ItemOnMovementCount;
			public long HandlesOnMovementItemCount;
			public long VisibilityChecks;
			public long CanSeeChecks;
			public long InLOSChecks;
			public long MobileMovingPacketsSent;

			public void Reset()
			{
				Moves = 0;
				Accepted = 0;
				Rejected = 0;
				SlowMoves = 0;
				TotalMs = 0;
				MaxMs = 0;
				LastMs = 0;

				PrecheckMs = 0;
				CheckMovementMs = 0;
				CoordinateMs = 0;
				SectorLookupMs = 0;
				MoveOffMobileMs = 0;
				MoveOffItemMs = 0;
				MoveOverMobileMs = 0;
				MoveOverItemMs = 0;
				RegionCanMoveMs = 0;
				InternalOnMoveMs = 0;
				FastwalkMs = 0;
				DisruptiveActionMs = 0;
				AckSendMs = 0;
				SetLocationMs = 0;
				SetDirectionMs = 0;
				GetObjectsInRangeMs = 0;
				BuildMoveListsMs = 0;
				SendMovingPacketsMs = 0;
				VisibilityMs = 0;
				NotorietyMs = 0;
				MovingPacketSendMs = 0;
				OnMovementMobileMs = 0;
				OnMovementItemMs = 0;
				OnAfterMoveMs = 0;
				OtherMs = 0;

				MaxPhaseMs = 0;
				MaxPhaseName = null;
				WorstMobile = null;
				WorstAccount = null;
				WorstMap = null;
				WorstStart = Point3D.Zero;
				WorstEnd = Point3D.Zero;
				WorstDirection = Direction.North;
				WorstAccepted = false;
				WorstAITimerContext = false;
				WorstReason = null;
				WorstTopPhase = null;
				WorstTopPhaseMs = 0;

				OldSectorMobileCount = 0;
				OldSectorItemCount = 0;
				NewSectorMobileCount = 0;
				NewSectorItemCount = 0;
				ObjectsInRangeCount = 0;
				MoveClientCount = 0;
				MoveListCount = 0;
				MobileOnMovementCount = 0;
				ItemOnMovementCount = 0;
				HandlesOnMovementItemCount = 0;
				VisibilityChecks = 0;
				CanSeeChecks = 0;
				InLOSChecks = 0;
				MobileMovingPacketsSent = 0;
			}

			public void CopyFrom( WindowStats other )
			{
				Moves = other.Moves;
				Accepted = other.Accepted;
				Rejected = other.Rejected;
				SlowMoves = other.SlowMoves;
				TotalMs = other.TotalMs;
				MaxMs = other.MaxMs;
				LastMs = other.LastMs;

				PrecheckMs = other.PrecheckMs;
				CheckMovementMs = other.CheckMovementMs;
				CoordinateMs = other.CoordinateMs;
				SectorLookupMs = other.SectorLookupMs;
				MoveOffMobileMs = other.MoveOffMobileMs;
				MoveOffItemMs = other.MoveOffItemMs;
				MoveOverMobileMs = other.MoveOverMobileMs;
				MoveOverItemMs = other.MoveOverItemMs;
				RegionCanMoveMs = other.RegionCanMoveMs;
				InternalOnMoveMs = other.InternalOnMoveMs;
				FastwalkMs = other.FastwalkMs;
				DisruptiveActionMs = other.DisruptiveActionMs;
				AckSendMs = other.AckSendMs;
				SetLocationMs = other.SetLocationMs;
				SetDirectionMs = other.SetDirectionMs;
				GetObjectsInRangeMs = other.GetObjectsInRangeMs;
				BuildMoveListsMs = other.BuildMoveListsMs;
				SendMovingPacketsMs = other.SendMovingPacketsMs;
				VisibilityMs = other.VisibilityMs;
				NotorietyMs = other.NotorietyMs;
				MovingPacketSendMs = other.MovingPacketSendMs;
				OnMovementMobileMs = other.OnMovementMobileMs;
				OnMovementItemMs = other.OnMovementItemMs;
				OnAfterMoveMs = other.OnAfterMoveMs;
				OtherMs = other.OtherMs;

				MaxPhaseMs = other.MaxPhaseMs;
				MaxPhaseName = other.MaxPhaseName;
				WorstMobile = other.WorstMobile;
				WorstAccount = other.WorstAccount;
				WorstMap = other.WorstMap;
				WorstStart = other.WorstStart;
				WorstEnd = other.WorstEnd;
				WorstDirection = other.WorstDirection;
				WorstAccepted = other.WorstAccepted;
				WorstAITimerContext = other.WorstAITimerContext;
				WorstReason = other.WorstReason;
				WorstTopPhase = other.WorstTopPhase;
				WorstTopPhaseMs = other.WorstTopPhaseMs;

				OldSectorMobileCount = other.OldSectorMobileCount;
				OldSectorItemCount = other.OldSectorItemCount;
				NewSectorMobileCount = other.NewSectorMobileCount;
				NewSectorItemCount = other.NewSectorItemCount;
				ObjectsInRangeCount = other.ObjectsInRangeCount;
				MoveClientCount = other.MoveClientCount;
				MoveListCount = other.MoveListCount;
				MobileOnMovementCount = other.MobileOnMovementCount;
				ItemOnMovementCount = other.ItemOnMovementCount;
				HandlesOnMovementItemCount = other.HandlesOnMovementItemCount;
				VisibilityChecks = other.VisibilityChecks;
				CanSeeChecks = other.CanSeeChecks;
				InLOSChecks = other.InLOSChecks;
				MobileMovingPacketsSent = other.MobileMovingPacketsSent;
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

		public static MovePathSample CreateSample( Mobile mobile, Direction direction, Point3D startLocation )
		{
			MovePathSample sample = new MovePathSample();

			sample.Mobile = mobile;
			sample.Map = mobile != null ? mobile.Map : null;
			sample.StartLocation = startLocation;
			sample.EndLocation = startLocation;
			sample.Direction = direction;
			sample.AITimerContext = OnMovementDiagnostics.IsAITimerContext;
			sample.RejectedReason = ReasonSuccess;

			return sample;
		}

		public static void RecordRejected( ref MovePathSample sample, string reason, long start, Point3D endLocation )
		{
			sample.Accepted = false;
			sample.RejectedReason = reason;
			RecordFinished( ref sample, start, endLocation );
		}

		public static void RecordAccepted( ref MovePathSample sample, string reason, long start, Point3D endLocation )
		{
			sample.Accepted = true;
			sample.RejectedReason = reason;
			RecordFinished( ref sample, start, endLocation );
		}

		private static void RecordFinished( ref MovePathSample sample, long start, Point3D endLocation )
		{
			sample.TotalMs = GetElapsedMilliseconds( start );
			sample.EndLocation = endLocation;
			sample.OtherMs = sample.TotalMs - GetAccountedMs( ref sample );

			if ( sample.OtherMs < 0 )
				sample.OtherMs = 0;

			Record( ref sample );
		}

		private static long GetAccountedMs( ref MovePathSample sample )
		{
			return sample.PrecheckMs +
				sample.CheckMovementMs +
				sample.CoordinateMs +
				sample.SectorLookupMs +
				sample.MoveOffMobileMs +
				sample.MoveOffItemMs +
				sample.MoveOverMobileMs +
				sample.MoveOverItemMs +
				sample.RegionCanMoveMs +
				sample.InternalOnMoveMs +
				sample.FastwalkMs +
				sample.DisruptiveActionMs +
				sample.AckSendMs +
				sample.SetLocationMs +
				sample.SetDirectionMs +
				sample.GetObjectsInRangeMs +
				sample.BuildMoveListsMs +
				sample.SendMovingPacketsMs +
				sample.OnMovementMobileMs +
				sample.OnMovementItemMs +
				sample.OnAfterMoveMs;
		}

		private static void Record( ref MovePathSample sample )
		{
			bool logSlow = false;
			bool logSummary = false;

			lock ( m_Lock )
			{
				++m_Current.Moves;
				m_Current.TotalMs += sample.TotalMs;
				m_Current.LastMs = sample.TotalMs;

				if ( sample.Accepted )
					++m_Current.Accepted;
				else
					++m_Current.Rejected;

				if ( sample.TotalMs > m_Current.MaxMs )
				{
					m_Current.MaxMs = sample.TotalMs;
					SetWorstContext( m_Current, ref sample );
				}

				if ( sample.TotalMs >= SlowMoveThreshold )
				{
					++m_Current.SlowMoves;
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
					logSummary = ( m_Current.Moves > 0 && m_Current.SlowMoves > 0 );
					m_Current.Reset();
				}
			}

			if ( logSlow )
				LogSlowMove( ref sample );

			if ( logSummary )
				LogWindow( m_Last );
		}

		private static void AddPhases( WindowStats stats, ref MovePathSample sample )
		{
			stats.PrecheckMs += sample.PrecheckMs;
			stats.CheckMovementMs += sample.CheckMovementMs;
			stats.CoordinateMs += sample.CoordinateMs;
			stats.SectorLookupMs += sample.SectorLookupMs;
			stats.MoveOffMobileMs += sample.MoveOffMobileMs;
			stats.MoveOffItemMs += sample.MoveOffItemMs;
			stats.MoveOverMobileMs += sample.MoveOverMobileMs;
			stats.MoveOverItemMs += sample.MoveOverItemMs;
			stats.RegionCanMoveMs += sample.RegionCanMoveMs;
			stats.InternalOnMoveMs += sample.InternalOnMoveMs;
			stats.FastwalkMs += sample.FastwalkMs;
			stats.DisruptiveActionMs += sample.DisruptiveActionMs;
			stats.AckSendMs += sample.AckSendMs;
			stats.SetLocationMs += sample.SetLocationMs;
			stats.SetDirectionMs += sample.SetDirectionMs;
			stats.GetObjectsInRangeMs += sample.GetObjectsInRangeMs;
			stats.BuildMoveListsMs += sample.BuildMoveListsMs;
			stats.SendMovingPacketsMs += sample.SendMovingPacketsMs;
			stats.VisibilityMs += sample.VisibilityMs;
			stats.NotorietyMs += sample.NotorietyMs;
			stats.MovingPacketSendMs += sample.MovingPacketSendMs;
			stats.OnMovementMobileMs += sample.OnMovementMobileMs;
			stats.OnMovementItemMs += sample.OnMovementItemMs;
			stats.OnAfterMoveMs += sample.OnAfterMoveMs;
			stats.OtherMs += sample.OtherMs;
		}

		private static void AddCounts( WindowStats stats, ref MovePathSample sample )
		{
			stats.OldSectorMobileCount += sample.OldSectorMobileCount;
			stats.OldSectorItemCount += sample.OldSectorItemCount;
			stats.NewSectorMobileCount += sample.NewSectorMobileCount;
			stats.NewSectorItemCount += sample.NewSectorItemCount;
			stats.ObjectsInRangeCount += sample.ObjectsInRangeCount;
			stats.MoveClientCount += sample.MoveClientCount;
			stats.MoveListCount += sample.MoveListCount;
			stats.MobileOnMovementCount += sample.MobileOnMovementCount;
			stats.ItemOnMovementCount += sample.ItemOnMovementCount;
			stats.HandlesOnMovementItemCount += sample.HandlesOnMovementItemCount;
			stats.VisibilityChecks += sample.VisibilityChecks;
			stats.CanSeeChecks += sample.CanSeeChecks;
			stats.InLOSChecks += sample.InLOSChecks;
			stats.MobileMovingPacketsSent += sample.MobileMovingPacketsSent;
		}

		private static void UpdateMaxPhase( WindowStats stats, ref MovePathSample sample )
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

		private static void SetWorstContext( WindowStats stats, ref MovePathSample sample )
		{
			string topName;
			long topMs;

			GetTopPhase( ref sample, out topName, out topMs );

			stats.WorstMobile = GetMobileText( sample.Mobile );
			stats.WorstAccount = GetAccountText( sample.Mobile );
			stats.WorstMap = sample.Map != null ? sample.Map.ToString() : "null";
			stats.WorstStart = sample.StartLocation;
			stats.WorstEnd = sample.EndLocation;
			stats.WorstDirection = sample.Direction;
			stats.WorstAccepted = sample.Accepted;
			stats.WorstAITimerContext = sample.AITimerContext;
			stats.WorstReason = sample.RejectedReason;
			stats.WorstTopPhase = topName;
			stats.WorstTopPhaseMs = topMs;
		}

		private static void GetTopPhase( ref MovePathSample sample, out string name, out long ms )
		{
			name = "none";
			ms = 0;

			ConsiderPhase( "precheck", sample.PrecheckMs, ref name, ref ms );
			ConsiderPhase( "check", sample.CheckMovementMs, ref name, ref ms );
			ConsiderPhase( "coordinate", sample.CoordinateMs, ref name, ref ms );
			ConsiderPhase( "sector", sample.SectorLookupMs, ref name, ref ms );
			ConsiderPhase( "moveOffMobile", sample.MoveOffMobileMs, ref name, ref ms );
			ConsiderPhase( "moveOffItem", sample.MoveOffItemMs, ref name, ref ms );
			ConsiderPhase( "moveOverMobile", sample.MoveOverMobileMs, ref name, ref ms );
			ConsiderPhase( "moveOverItem", sample.MoveOverItemMs, ref name, ref ms );
			ConsiderPhase( "region", sample.RegionCanMoveMs, ref name, ref ms );
			ConsiderPhase( "internal", sample.InternalOnMoveMs, ref name, ref ms );
			ConsiderPhase( "fastwalk", sample.FastwalkMs, ref name, ref ms );
			ConsiderPhase( "disrupt", sample.DisruptiveActionMs, ref name, ref ms );
			ConsiderPhase( "ack", sample.AckSendMs, ref name, ref ms );
			ConsiderPhase( "setLoc", sample.SetLocationMs, ref name, ref ms );
			ConsiderPhase( "setDir", sample.SetDirectionMs, ref name, ref ms );
			ConsiderPhase( "getRange", sample.GetObjectsInRangeMs, ref name, ref ms );
			ConsiderPhase( "buildLists", sample.BuildMoveListsMs, ref name, ref ms );
			ConsiderPhase( "sendPackets", sample.SendMovingPacketsMs, ref name, ref ms );
			ConsiderPhase( "onMoveMob", sample.OnMovementMobileMs, ref name, ref ms );
			ConsiderPhase( "onMoveItem", sample.OnMovementItemMs, ref name, ref ms );
			ConsiderPhase( "after", sample.OnAfterMoveMs, ref name, ref ms );
			ConsiderPhase( "other", sample.OtherMs, ref name, ref ms );
		}

		private static void ConsiderPhase( string phaseName, long phaseMs, ref string name, ref long ms )
		{
			if ( phaseMs > ms )
			{
				name = phaseName;
				ms = phaseMs;
			}
		}

		private static string GetTopTotalPhase( WindowStats stats )
		{
			string name = "none";
			long ms = 0;

			ConsiderPhase( "precheck", stats.PrecheckMs, ref name, ref ms );
			ConsiderPhase( "check", stats.CheckMovementMs, ref name, ref ms );
			ConsiderPhase( "coordinate", stats.CoordinateMs, ref name, ref ms );
			ConsiderPhase( "sector", stats.SectorLookupMs, ref name, ref ms );
			ConsiderPhase( "moveOffMobile", stats.MoveOffMobileMs, ref name, ref ms );
			ConsiderPhase( "moveOffItem", stats.MoveOffItemMs, ref name, ref ms );
			ConsiderPhase( "moveOverMobile", stats.MoveOverMobileMs, ref name, ref ms );
			ConsiderPhase( "moveOverItem", stats.MoveOverItemMs, ref name, ref ms );
			ConsiderPhase( "region", stats.RegionCanMoveMs, ref name, ref ms );
			ConsiderPhase( "internal", stats.InternalOnMoveMs, ref name, ref ms );
			ConsiderPhase( "fastwalk", stats.FastwalkMs, ref name, ref ms );
			ConsiderPhase( "disrupt", stats.DisruptiveActionMs, ref name, ref ms );
			ConsiderPhase( "ack", stats.AckSendMs, ref name, ref ms );
			ConsiderPhase( "setLoc", stats.SetLocationMs, ref name, ref ms );
			ConsiderPhase( "setDir", stats.SetDirectionMs, ref name, ref ms );
			ConsiderPhase( "getRange", stats.GetObjectsInRangeMs, ref name, ref ms );
			ConsiderPhase( "buildLists", stats.BuildMoveListsMs, ref name, ref ms );
			ConsiderPhase( "sendPackets", stats.SendMovingPacketsMs, ref name, ref ms );
			ConsiderPhase( "onMoveMob", stats.OnMovementMobileMs, ref name, ref ms );
			ConsiderPhase( "onMoveItem", stats.OnMovementItemMs, ref name, ref ms );
			ConsiderPhase( "after", stats.OnAfterMoveMs, ref name, ref ms );
			ConsiderPhase( "other", stats.OtherMs, ref name, ref ms );

			return String.Format( "{0}:{1}ms", name, ms );
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

		private static void LogSlowMove( ref MovePathSample sample )
		{
			string topName;
			long topMs;

			GetTopPhase( ref sample, out topName, out topMs );

			DiagnosticConsole.WriteParsedLine( "[Lag] MovePath slow {0}ms mobile={1} account={2} map={3} start={4} end={5} dir={6} accepted={7} reason={8} top={9}:{10}ms",
				sample.TotalMs,
				GetMobileText( sample.Mobile ),
				GetAccountText( sample.Mobile ),
				sample.Map,
				sample.StartLocation,
				sample.EndLocation,
				sample.Direction,
				sample.Accepted,
				sample.RejectedReason,
				topName,
				topMs );

			DiagnosticConsole.WriteParsedLine( "[Lag] MovePath detail check={0} sector={1} moveOffM={2} moveOffI={3} moveOverM={4} moveOverI={5} region={6} internal={7} fastwalk={8} ack={9} setLoc={10} getRange={11} build={12} sendPackets={13} visibility={14} noto={15} packetSend={16} onMoveMob={17} onMoveItem={18} after={19} other={20}; objects={21} clients={22} moveList={23} losChecks={24} packetsSent={25}",
				sample.CheckMovementMs,
				sample.SectorLookupMs,
				sample.MoveOffMobileMs,
				sample.MoveOffItemMs,
				sample.MoveOverMobileMs,
				sample.MoveOverItemMs,
				sample.RegionCanMoveMs,
				sample.InternalOnMoveMs,
				sample.FastwalkMs,
				sample.AckSendMs,
				sample.SetLocationMs,
				sample.GetObjectsInRangeMs,
				sample.BuildMoveListsMs,
				sample.SendMovingPacketsMs,
				sample.VisibilityMs,
				sample.NotorietyMs,
				sample.MovingPacketSendMs,
				sample.OnMovementMobileMs,
				sample.OnMovementItemMs,
				sample.OnAfterMoveMs,
				sample.OtherMs,
				sample.ObjectsInRangeCount,
				sample.MoveClientCount,
				sample.MoveListCount,
				sample.InLOSChecks,
				sample.MobileMovingPacketsSent );
		}

		private static void LogWindow( WindowStats stats )
		{
			long avg = stats.Moves > 0 ? stats.TotalMs / stats.Moves : 0;
			long objectsAvg = stats.Moves > 0 ? stats.ObjectsInRangeCount / stats.Moves : 0;
			long clientsAvg = stats.Moves > 0 ? stats.MoveClientCount / stats.Moves : 0;
			long callbacksAvg = stats.Moves > 0 ? ( stats.MobileOnMovementCount + stats.ItemOnMovementCount ) / stats.Moves : 0;

			DiagnosticConsole.WriteParsedLine( "[Lag] MovePathStats 30s: moves={0} avg={1}ms max={2}ms accepted={3} rejected={4} slow={5} check={6}ms sector={7}ms region={8}ms internal={9}ms getRange={10}ms buildLists={11}ms visibility={12}ms sendPackets={13}ms onMovement={14}ms topTotal={15} topMax={16}:{17}ms",
				stats.Moves,
				avg,
				stats.MaxMs,
				stats.Accepted,
				stats.Rejected,
				stats.SlowMoves,
				stats.CheckMovementMs,
				stats.SectorLookupMs,
				stats.RegionCanMoveMs,
				stats.InternalOnMoveMs,
				stats.GetObjectsInRangeMs,
				stats.BuildMoveListsMs,
				stats.VisibilityMs,
				stats.SendMovingPacketsMs,
				stats.OnMovementMobileMs + stats.OnMovementItemMs,
				GetTopTotalPhase( stats ),
				stats.MaxPhaseName,
				stats.MaxPhaseMs );

			DiagnosticConsole.WriteParsedLine( "[Lag] MovePathCounts 30s: objectsAvg={0} clientsAvg={1} callbacksAvg={2} packetsSent={3} losChecks={4} canSeeChecks={5} itemMovement={6} mobileMovement={7} worst={8} account={9} map={10} start={11} end={12} dir={13} accepted={14} reason={15} aiTimer={16} top={17}:{18}ms",
				objectsAvg,
				clientsAvg,
				callbacksAvg,
				stats.MobileMovingPacketsSent,
				stats.InLOSChecks,
				stats.CanSeeChecks,
				stats.ItemOnMovementCount,
				stats.MobileOnMovementCount,
				stats.WorstMobile,
				stats.WorstAccount,
				stats.WorstMap,
				stats.WorstStart,
				stats.WorstEnd,
				stats.WorstDirection,
				stats.WorstAccepted,
				stats.WorstReason,
				stats.WorstAITimerContext,
				stats.WorstTopPhase,
				stats.WorstTopPhaseMs );
		}

		public static void WriteStats( TextWriter writer )
		{
			if ( writer == null )
				return;

			lock ( m_Lock )
			{
				WriteWindow( writer, "Current MovePath window", m_Current );
				WriteWindow( writer, "Last MovePath window", m_Last );
			}
		}

		private static void WriteWindow( TextWriter writer, string label, WindowStats stats )
		{
			long avg = stats.Moves > 0 ? stats.TotalMs / stats.Moves : 0;
			long objectsAvg = stats.Moves > 0 ? stats.ObjectsInRangeCount / stats.Moves : 0;
			long clientsAvg = stats.Moves > 0 ? stats.MoveClientCount / stats.Moves : 0;
			long callbacksAvg = stats.Moves > 0 ? ( stats.MobileOnMovementCount + stats.ItemOnMovementCount ) / stats.Moves : 0;

			writer.WriteLine( "{0}: moves={1} avg={2}ms max={3}ms last={4}ms accepted={5} rejected={6} slow={7}",
				label,
				stats.Moves,
				avg,
				stats.MaxMs,
				stats.LastMs,
				stats.Accepted,
				stats.Rejected,
				stats.SlowMoves );

			writer.WriteLine( "  phases total: pre={0} check={1} coord={2} sector={3} offM={4} offI={5} overM={6} overI={7} region={8} internal={9} fastwalk={10} disrupt={11} ack={12} setLoc={13} setDir={14} getRange={15} build={16} sendPackets={17} visibility={18} notoriety={19} packetSend={20} onMoveMob={21} onMoveItem={22} after={23} other={24}",
				stats.PrecheckMs,
				stats.CheckMovementMs,
				stats.CoordinateMs,
				stats.SectorLookupMs,
				stats.MoveOffMobileMs,
				stats.MoveOffItemMs,
				stats.MoveOverMobileMs,
				stats.MoveOverItemMs,
				stats.RegionCanMoveMs,
				stats.InternalOnMoveMs,
				stats.FastwalkMs,
				stats.DisruptiveActionMs,
				stats.AckSendMs,
				stats.SetLocationMs,
				stats.SetDirectionMs,
				stats.GetObjectsInRangeMs,
				stats.BuildMoveListsMs,
				stats.SendMovingPacketsMs,
				stats.VisibilityMs,
				stats.NotorietyMs,
				stats.MovingPacketSendMs,
				stats.OnMovementMobileMs,
				stats.OnMovementItemMs,
				stats.OnAfterMoveMs,
				stats.OtherMs );

			writer.WriteLine( "  averages/counts: objectsAvg={0} clientsAvg={1} callbacksAvg={2} moveListTotal={3} packetsSent={4} visibilityChecks={5} canSeeChecks={6} losChecks={7} itemMovement={8} mobileMovement={9} topTotal={10} topMax={11}:{12}ms",
				objectsAvg,
				clientsAvg,
				callbacksAvg,
				stats.MoveListCount,
				stats.MobileMovingPacketsSent,
				stats.VisibilityChecks,
				stats.CanSeeChecks,
				stats.InLOSChecks,
				stats.ItemOnMovementCount,
				stats.MobileOnMovementCount,
				GetTopTotalPhase( stats ),
				stats.MaxPhaseName,
				stats.MaxPhaseMs );

			writer.WriteLine( "  worst: mobile={0} account={1} map={2} start={3} end={4} dir={5} accepted={6} reason={7} aiTimer={8} top={9}:{10}ms",
				stats.WorstMobile,
				stats.WorstAccount,
				stats.WorstMap,
				stats.WorstStart,
				stats.WorstEnd,
				stats.WorstDirection,
				stats.WorstAccepted,
				stats.WorstReason,
				stats.WorstAITimerContext,
				stats.WorstTopPhase,
				stats.WorstTopPhaseMs );
		}
	}
}
