/***************************************************************************
 *                     SetLocationDiagnostics.cs
 *                     -------------------------
 *   Attribution for Mobile.SetLocation costs, especially movement-driven calls.
 ***************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using Server.Accounting;

namespace Server.Diagnostics
{
	public static class SetLocationDiagnostics
	{
		private const long SummaryInterval = 30000;
		private const long SlowSetLocationThreshold = 150;

		private static readonly object m_Lock = new object();
		private static readonly WindowStats m_Current = new WindowStats();
		private static readonly WindowStats m_Last = new WindowStats();
		private static long m_NextSummary;

		[ThreadStatic]
		private static MovementContext m_MovementContext;

		private sealed class MovementContext
		{
			public Mobile Mobile;
			public Point3D Start;
			public Point3D Target;
			public Direction Direction;
		}

		public struct SetLocationSample
		{
			public Mobile Mobile;
			public Map Map;
			public Point3D StartLocation;
			public Point3D EndLocation;
			public Point3D TargetLocation;
			public Direction Direction;
			public bool IsTeleport;
			public bool Movement;
			public bool Detailed;
			public bool HasNetState;
			public long TotalMs;

			public long PrecheckMs;
			public long AssignLocationMs;
			public long UpdateRegionMs;
			public long BankBoxCloseMs;
			public long ValidateTradesMs;
			public long MapOnMoveMs;
			public long TeleportUpdateMs;
			public long OldClientsRangeMs;
			public long OldClientsRemoveMs;
			public long OurStateGetObjectsMs;
			public long OurStateItemLoopMs;
			public long OurStateMobileLoopMs;
			public long OtherClientsGetRangeMs;
			public long OtherClientsLoopMs;
			public long OldClientLOSMs;
			public long NewClientLOSMs;
			public long CanSeeMs;
			public long MobileIncomingMs;
			public long HealthbarPacketMs;
			public long OPLPacketMs;
			public long RemovePacketMs;
			public long BondedPacketMs;
			public long OnLocationChangeMs;
			public long RegionOnLocationChangedMs;
			public long OtherMs;

			public int OldClientsInRangeCount;
			public int RemovePacketsSent;
			public int ObjectsInRangeCount;
			public int ItemsConsidered;
			public int ItemsSentToSelf;
			public int MobilesConsidered;
			public int MobileIncomingSentToOthers;
			public int MobileIncomingSentToSelf;
			public int HealthbarPacketsSent;
			public int OPLPacketsSent;
			public int BondedPacketsSent;
			public int LineOfSightChecks;
			public int InLOSChecks;
			public int CanSeeChecks;
			public int OurStateBranchCount;
			public int NoNetStateBranchCount;
		}

		private sealed class WindowStats
		{
			public int Calls;
			public int MovementCalls;
			public int TeleportCalls;
			public int SlowCalls;
			public int SlowMovementCalls;
			public long TotalMs;
			public long MaxMs;
			public long LastMs;

			public long PrecheckMs;
			public long AssignLocationMs;
			public long UpdateRegionMs;
			public long BankBoxCloseMs;
			public long ValidateTradesMs;
			public long MapOnMoveMs;
			public long TeleportUpdateMs;
			public long OldClientsRangeMs;
			public long OldClientsRemoveMs;
			public long OurStateGetObjectsMs;
			public long OurStateItemLoopMs;
			public long OurStateMobileLoopMs;
			public long OtherClientsGetRangeMs;
			public long OtherClientsLoopMs;
			public long OldClientLOSMs;
			public long NewClientLOSMs;
			public long CanSeeMs;
			public long MobileIncomingMs;
			public long HealthbarPacketMs;
			public long OPLPacketMs;
			public long RemovePacketMs;
			public long BondedPacketMs;
			public long OnLocationChangeMs;
			public long RegionOnLocationChangedMs;
			public long OtherMs;

			public long MaxPhaseMs;
			public string MaxPhaseName;
			public string WorstMobile;
			public string WorstAccount;
			public string WorstMap;
			public Point3D WorstStart;
			public Point3D WorstEnd;
			public Point3D WorstTarget;
			public Direction WorstDirection;
			public bool WorstMovement;
			public bool WorstTeleport;
			public string WorstTopPhase;
			public long WorstTopPhaseMs;

			public long OldClientsInRangeCount;
			public long RemovePacketsSent;
			public long ObjectsInRangeCount;
			public long ItemsConsidered;
			public long ItemsSentToSelf;
			public long MobilesConsidered;
			public long MobileIncomingSentToOthers;
			public long MobileIncomingSentToSelf;
			public long HealthbarPacketsSent;
			public long OPLPacketsSent;
			public long BondedPacketsSent;
			public long LineOfSightChecks;
			public long InLOSChecks;
			public long CanSeeChecks;
			public long OurStateBranchCount;
			public long NoNetStateBranchCount;

			public void Reset()
			{
				Calls = 0;
				MovementCalls = 0;
				TeleportCalls = 0;
				SlowCalls = 0;
				SlowMovementCalls = 0;
				TotalMs = 0;
				MaxMs = 0;
				LastMs = 0;

				PrecheckMs = 0;
				AssignLocationMs = 0;
				UpdateRegionMs = 0;
				BankBoxCloseMs = 0;
				ValidateTradesMs = 0;
				MapOnMoveMs = 0;
				TeleportUpdateMs = 0;
				OldClientsRangeMs = 0;
				OldClientsRemoveMs = 0;
				OurStateGetObjectsMs = 0;
				OurStateItemLoopMs = 0;
				OurStateMobileLoopMs = 0;
				OtherClientsGetRangeMs = 0;
				OtherClientsLoopMs = 0;
				OldClientLOSMs = 0;
				NewClientLOSMs = 0;
				CanSeeMs = 0;
				MobileIncomingMs = 0;
				HealthbarPacketMs = 0;
				OPLPacketMs = 0;
				RemovePacketMs = 0;
				BondedPacketMs = 0;
				OnLocationChangeMs = 0;
				RegionOnLocationChangedMs = 0;
				OtherMs = 0;

				MaxPhaseMs = 0;
				MaxPhaseName = null;
				WorstMobile = null;
				WorstAccount = null;
				WorstMap = null;
				WorstStart = Point3D.Zero;
				WorstEnd = Point3D.Zero;
				WorstTarget = Point3D.Zero;
				WorstDirection = Direction.North;
				WorstMovement = false;
				WorstTeleport = false;
				WorstTopPhase = null;
				WorstTopPhaseMs = 0;

				OldClientsInRangeCount = 0;
				RemovePacketsSent = 0;
				ObjectsInRangeCount = 0;
				ItemsConsidered = 0;
				ItemsSentToSelf = 0;
				MobilesConsidered = 0;
				MobileIncomingSentToOthers = 0;
				MobileIncomingSentToSelf = 0;
				HealthbarPacketsSent = 0;
				OPLPacketsSent = 0;
				BondedPacketsSent = 0;
				LineOfSightChecks = 0;
				InLOSChecks = 0;
				CanSeeChecks = 0;
				OurStateBranchCount = 0;
				NoNetStateBranchCount = 0;
			}

			public void CopyFrom( WindowStats other )
			{
				Calls = other.Calls;
				MovementCalls = other.MovementCalls;
				TeleportCalls = other.TeleportCalls;
				SlowCalls = other.SlowCalls;
				SlowMovementCalls = other.SlowMovementCalls;
				TotalMs = other.TotalMs;
				MaxMs = other.MaxMs;
				LastMs = other.LastMs;

				PrecheckMs = other.PrecheckMs;
				AssignLocationMs = other.AssignLocationMs;
				UpdateRegionMs = other.UpdateRegionMs;
				BankBoxCloseMs = other.BankBoxCloseMs;
				ValidateTradesMs = other.ValidateTradesMs;
				MapOnMoveMs = other.MapOnMoveMs;
				TeleportUpdateMs = other.TeleportUpdateMs;
				OldClientsRangeMs = other.OldClientsRangeMs;
				OldClientsRemoveMs = other.OldClientsRemoveMs;
				OurStateGetObjectsMs = other.OurStateGetObjectsMs;
				OurStateItemLoopMs = other.OurStateItemLoopMs;
				OurStateMobileLoopMs = other.OurStateMobileLoopMs;
				OtherClientsGetRangeMs = other.OtherClientsGetRangeMs;
				OtherClientsLoopMs = other.OtherClientsLoopMs;
				OldClientLOSMs = other.OldClientLOSMs;
				NewClientLOSMs = other.NewClientLOSMs;
				CanSeeMs = other.CanSeeMs;
				MobileIncomingMs = other.MobileIncomingMs;
				HealthbarPacketMs = other.HealthbarPacketMs;
				OPLPacketMs = other.OPLPacketMs;
				RemovePacketMs = other.RemovePacketMs;
				BondedPacketMs = other.BondedPacketMs;
				OnLocationChangeMs = other.OnLocationChangeMs;
				RegionOnLocationChangedMs = other.RegionOnLocationChangedMs;
				OtherMs = other.OtherMs;

				MaxPhaseMs = other.MaxPhaseMs;
				MaxPhaseName = other.MaxPhaseName;
				WorstMobile = other.WorstMobile;
				WorstAccount = other.WorstAccount;
				WorstMap = other.WorstMap;
				WorstStart = other.WorstStart;
				WorstEnd = other.WorstEnd;
				WorstTarget = other.WorstTarget;
				WorstDirection = other.WorstDirection;
				WorstMovement = other.WorstMovement;
				WorstTeleport = other.WorstTeleport;
				WorstTopPhase = other.WorstTopPhase;
				WorstTopPhaseMs = other.WorstTopPhaseMs;

				OldClientsInRangeCount = other.OldClientsInRangeCount;
				RemovePacketsSent = other.RemovePacketsSent;
				ObjectsInRangeCount = other.ObjectsInRangeCount;
				ItemsConsidered = other.ItemsConsidered;
				ItemsSentToSelf = other.ItemsSentToSelf;
				MobilesConsidered = other.MobilesConsidered;
				MobileIncomingSentToOthers = other.MobileIncomingSentToOthers;
				MobileIncomingSentToSelf = other.MobileIncomingSentToSelf;
				HealthbarPacketsSent = other.HealthbarPacketsSent;
				OPLPacketsSent = other.OPLPacketsSent;
				BondedPacketsSent = other.BondedPacketsSent;
				LineOfSightChecks = other.LineOfSightChecks;
				InLOSChecks = other.InLOSChecks;
				CanSeeChecks = other.CanSeeChecks;
				OurStateBranchCount = other.OurStateBranchCount;
				NoNetStateBranchCount = other.NoNetStateBranchCount;
			}
		}

		public static void BeginMoveSetLocationContext( Mobile mobile, Point3D start, Point3D target, Direction direction )
		{
			MovementContext context = m_MovementContext;

			if ( context == null )
				context = m_MovementContext = new MovementContext();

			context.Mobile = mobile;
			context.Start = start;
			context.Target = target;
			context.Direction = direction;
		}

		public static void EndMoveSetLocationContext( Mobile mobile )
		{
			MovementContext context = m_MovementContext;

			if ( context != null && context.Mobile == mobile )
				m_MovementContext = null;
		}

		public static bool IsMovementSetLocation( Mobile mobile )
		{
			MovementContext context = m_MovementContext;

			return ( context != null && context.Mobile == mobile );
		}

		public static long GetTimestamp()
		{
			return Stopwatch.GetTimestamp();
		}

		public static long GetElapsedMilliseconds( long start )
		{
			return ( Stopwatch.GetTimestamp() - start ) * 1000 / Stopwatch.Frequency;
		}

		public static SetLocationSample CreateSample( Mobile mobile, Point3D newLocation, bool isTeleport )
		{
			SetLocationSample sample = new SetLocationSample();
			MovementContext context = m_MovementContext;

			sample.Mobile = mobile;
			sample.Map = mobile != null ? mobile.Map : null;
			sample.StartLocation = mobile != null ? mobile.Location : Point3D.Zero;
			sample.EndLocation = sample.StartLocation;
			sample.TargetLocation = newLocation;
			sample.Direction = context != null ? context.Direction : Direction.North;
			sample.IsTeleport = isTeleport;
			sample.Movement = ( context != null && context.Mobile == mobile );
			sample.HasNetState = ( mobile != null && mobile.NetState != null );
			sample.Detailed = ( sample.Movement || sample.IsTeleport || sample.HasNetState );

			return sample;
		}

		public static void Record( ref SetLocationSample sample, long start, Point3D endLocation )
		{
			sample.TotalMs = GetElapsedMilliseconds( start );
			sample.EndLocation = endLocation;
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

				if ( sample.IsTeleport )
					++m_Current.TeleportCalls;

				if ( sample.TotalMs > m_Current.MaxMs )
				{
					m_Current.MaxMs = sample.TotalMs;
					SetWorstContext( m_Current, ref sample );
				}

				if ( sample.TotalMs >= SlowSetLocationThreshold )
				{
					++m_Current.SlowCalls;

					if ( sample.Movement )
						++m_Current.SlowMovementCalls;

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
					logSummary = ( m_Current.SlowMovementCalls > 0 );
					m_Current.Reset();
				}
			}

			if ( logSlow )
				LogSlowSetLocation( ref sample );

			if ( logSummary )
				LogWindow( m_Last );
		}

		private static long GetAccountedMs( ref SetLocationSample sample )
		{
			return sample.PrecheckMs +
				sample.AssignLocationMs +
				sample.UpdateRegionMs +
				sample.BankBoxCloseMs +
				sample.ValidateTradesMs +
				sample.MapOnMoveMs +
				sample.TeleportUpdateMs +
				sample.OldClientsRangeMs +
				sample.OldClientsRemoveMs +
				sample.OurStateGetObjectsMs +
				sample.OurStateItemLoopMs +
				sample.OurStateMobileLoopMs +
				sample.OtherClientsGetRangeMs +
				sample.OtherClientsLoopMs +
				sample.OnLocationChangeMs +
				sample.RegionOnLocationChangedMs;
		}

		private static void AddPhases( WindowStats stats, ref SetLocationSample sample )
		{
			stats.PrecheckMs += sample.PrecheckMs;
			stats.AssignLocationMs += sample.AssignLocationMs;
			stats.UpdateRegionMs += sample.UpdateRegionMs;
			stats.BankBoxCloseMs += sample.BankBoxCloseMs;
			stats.ValidateTradesMs += sample.ValidateTradesMs;
			stats.MapOnMoveMs += sample.MapOnMoveMs;
			stats.TeleportUpdateMs += sample.TeleportUpdateMs;
			stats.OldClientsRangeMs += sample.OldClientsRangeMs;
			stats.OldClientsRemoveMs += sample.OldClientsRemoveMs;
			stats.OurStateGetObjectsMs += sample.OurStateGetObjectsMs;
			stats.OurStateItemLoopMs += sample.OurStateItemLoopMs;
			stats.OurStateMobileLoopMs += sample.OurStateMobileLoopMs;
			stats.OtherClientsGetRangeMs += sample.OtherClientsGetRangeMs;
			stats.OtherClientsLoopMs += sample.OtherClientsLoopMs;
			stats.OldClientLOSMs += sample.OldClientLOSMs;
			stats.NewClientLOSMs += sample.NewClientLOSMs;
			stats.CanSeeMs += sample.CanSeeMs;
			stats.MobileIncomingMs += sample.MobileIncomingMs;
			stats.HealthbarPacketMs += sample.HealthbarPacketMs;
			stats.OPLPacketMs += sample.OPLPacketMs;
			stats.RemovePacketMs += sample.RemovePacketMs;
			stats.BondedPacketMs += sample.BondedPacketMs;
			stats.OnLocationChangeMs += sample.OnLocationChangeMs;
			stats.RegionOnLocationChangedMs += sample.RegionOnLocationChangedMs;
			stats.OtherMs += sample.OtherMs;
		}

		private static void AddCounts( WindowStats stats, ref SetLocationSample sample )
		{
			stats.OldClientsInRangeCount += sample.OldClientsInRangeCount;
			stats.RemovePacketsSent += sample.RemovePacketsSent;
			stats.ObjectsInRangeCount += sample.ObjectsInRangeCount;
			stats.ItemsConsidered += sample.ItemsConsidered;
			stats.ItemsSentToSelf += sample.ItemsSentToSelf;
			stats.MobilesConsidered += sample.MobilesConsidered;
			stats.MobileIncomingSentToOthers += sample.MobileIncomingSentToOthers;
			stats.MobileIncomingSentToSelf += sample.MobileIncomingSentToSelf;
			stats.HealthbarPacketsSent += sample.HealthbarPacketsSent;
			stats.OPLPacketsSent += sample.OPLPacketsSent;
			stats.BondedPacketsSent += sample.BondedPacketsSent;
			stats.LineOfSightChecks += sample.LineOfSightChecks;
			stats.InLOSChecks += sample.InLOSChecks;
			stats.CanSeeChecks += sample.CanSeeChecks;
			stats.OurStateBranchCount += sample.OurStateBranchCount;
			stats.NoNetStateBranchCount += sample.NoNetStateBranchCount;
		}

		private static void UpdateMaxPhase( WindowStats stats, ref SetLocationSample sample )
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

		private static void SetWorstContext( WindowStats stats, ref SetLocationSample sample )
		{
			string topName;
			long topMs;

			GetTopPhase( ref sample, out topName, out topMs );

			stats.WorstMobile = GetMobileText( sample.Mobile );
			stats.WorstAccount = GetAccountText( sample.Mobile );
			stats.WorstMap = sample.Map != null ? sample.Map.ToString() : "null";
			stats.WorstStart = sample.StartLocation;
			stats.WorstEnd = sample.EndLocation;
			stats.WorstTarget = sample.TargetLocation;
			stats.WorstDirection = sample.Direction;
			stats.WorstMovement = sample.Movement;
			stats.WorstTeleport = sample.IsTeleport;
			stats.WorstTopPhase = topName;
			stats.WorstTopPhaseMs = topMs;
		}

		private static void GetTopPhase( ref SetLocationSample sample, out string name, out long ms )
		{
			name = "none";
			ms = 0;

			ConsiderPhase( "precheck", sample.PrecheckMs, ref name, ref ms );
			ConsiderPhase( "assign", sample.AssignLocationMs, ref name, ref ms );
			ConsiderPhase( "updateRegion", sample.UpdateRegionMs, ref name, ref ms );
			ConsiderPhase( "bankBox", sample.BankBoxCloseMs, ref name, ref ms );
			ConsiderPhase( "trades", sample.ValidateTradesMs, ref name, ref ms );
			ConsiderPhase( "mapOnMove", sample.MapOnMoveMs, ref name, ref ms );
			ConsiderPhase( "teleportUpdate", sample.TeleportUpdateMs, ref name, ref ms );
			ConsiderPhase( "oldClientRange", sample.OldClientsRangeMs, ref name, ref ms );
			ConsiderPhase( "oldClientRemove", sample.OldClientsRemoveMs, ref name, ref ms );
			ConsiderPhase( "getObjects", sample.OurStateGetObjectsMs, ref name, ref ms );
			ConsiderPhase( "itemLoop", sample.OurStateItemLoopMs, ref name, ref ms );
			ConsiderPhase( "mobileLoop", sample.OurStateMobileLoopMs, ref name, ref ms );
			ConsiderPhase( "otherClientRange", sample.OtherClientsGetRangeMs, ref name, ref ms );
			ConsiderPhase( "otherClientLoop", sample.OtherClientsLoopMs, ref name, ref ms );
			ConsiderPhase( "onLocation", sample.OnLocationChangeMs, ref name, ref ms );
			ConsiderPhase( "regionChanged", sample.RegionOnLocationChangedMs, ref name, ref ms );
			ConsiderPhase( "other", sample.OtherMs, ref name, ref ms );
		}

		private static string GetTopTotalPhase( WindowStats stats )
		{
			string name = "none";
			long ms = 0;

			ConsiderPhase( "precheck", stats.PrecheckMs, ref name, ref ms );
			ConsiderPhase( "assign", stats.AssignLocationMs, ref name, ref ms );
			ConsiderPhase( "updateRegion", stats.UpdateRegionMs, ref name, ref ms );
			ConsiderPhase( "bankBox", stats.BankBoxCloseMs, ref name, ref ms );
			ConsiderPhase( "trades", stats.ValidateTradesMs, ref name, ref ms );
			ConsiderPhase( "mapOnMove", stats.MapOnMoveMs, ref name, ref ms );
			ConsiderPhase( "teleportUpdate", stats.TeleportUpdateMs, ref name, ref ms );
			ConsiderPhase( "oldClientRange", stats.OldClientsRangeMs, ref name, ref ms );
			ConsiderPhase( "oldClientRemove", stats.OldClientsRemoveMs, ref name, ref ms );
			ConsiderPhase( "getObjects", stats.OurStateGetObjectsMs, ref name, ref ms );
			ConsiderPhase( "itemLoop", stats.OurStateItemLoopMs, ref name, ref ms );
			ConsiderPhase( "mobileLoop", stats.OurStateMobileLoopMs, ref name, ref ms );
			ConsiderPhase( "otherClientRange", stats.OtherClientsGetRangeMs, ref name, ref ms );
			ConsiderPhase( "otherClientLoop", stats.OtherClientsLoopMs, ref name, ref ms );
			ConsiderPhase( "onLocation", stats.OnLocationChangeMs, ref name, ref ms );
			ConsiderPhase( "regionChanged", stats.RegionOnLocationChangedMs, ref name, ref ms );
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

		private static void LogSlowSetLocation( ref SetLocationSample sample )
		{
			string topName;
			long topMs;

			GetTopPhase( ref sample, out topName, out topMs );

			DiagnosticConsole.WriteParsedLine( "[Lag] SetLocation slow {0}ms mobile={1} account={2} map={3} start={4} end={5} target={6} movement={7} teleport={8} top={9}:{10}ms",
				sample.TotalMs,
				GetMobileText( sample.Mobile ),
				GetAccountText( sample.Mobile ),
				sample.Map,
				sample.StartLocation,
				sample.EndLocation,
				sample.TargetLocation,
				sample.Movement,
				sample.IsTeleport,
				topName,
				topMs );

			DiagnosticConsole.WriteParsedLine( "[Lag] SetLocation detail updateRegion={0} mapOnMove={1} oldClients={2} removeOld={3} getObjects={4} itemLoop={5} mobileLoop={6} otherClients={7} onLocation={8} regionChanged={9} canSee={10} oldLos={11} newLos={12} incoming={13} health={14} opl={15} remove={16} other={17}; objects={18} items={19} mobiles={20} itemsSent={21} mobileSelf={22} mobileOther={23} los={24} canSeeChecks={25}",
				sample.UpdateRegionMs,
				sample.MapOnMoveMs,
				sample.OldClientsRangeMs,
				sample.OldClientsRemoveMs,
				sample.OurStateGetObjectsMs,
				sample.OurStateItemLoopMs,
				sample.OurStateMobileLoopMs,
				sample.OtherClientsLoopMs,
				sample.OnLocationChangeMs,
				sample.RegionOnLocationChangedMs,
				sample.CanSeeMs,
				sample.OldClientLOSMs,
				sample.NewClientLOSMs,
				sample.MobileIncomingMs,
				sample.HealthbarPacketMs,
				sample.OPLPacketMs,
				sample.RemovePacketMs,
				sample.OtherMs,
				sample.ObjectsInRangeCount,
				sample.ItemsConsidered,
				sample.MobilesConsidered,
				sample.ItemsSentToSelf,
				sample.MobileIncomingSentToSelf,
				sample.MobileIncomingSentToOthers,
				sample.InLOSChecks + sample.LineOfSightChecks,
				sample.CanSeeChecks );
		}

		private static void LogWindow( WindowStats stats )
		{
			long avg = stats.Calls > 0 ? stats.TotalMs / stats.Calls : 0;
			long objectsAvg = stats.Calls > 0 ? stats.ObjectsInRangeCount / stats.Calls : 0;
			long itemsAvg = stats.Calls > 0 ? stats.ItemsConsidered / stats.Calls : 0;
			long mobilesAvg = stats.Calls > 0 ? stats.MobilesConsidered / stats.Calls : 0;
			long oldClientsAvg = stats.Calls > 0 ? stats.OldClientsInRangeCount / stats.Calls : 0;

			DiagnosticConsole.WriteParsedLine( "[Lag] SetLocationStats 30s: calls={0} avg={1}ms max={2}ms movementCalls={3} teleportCalls={4} slow={5} updateRegion={6}ms mapOnMove={7}ms oldClients={8}ms getObjects={9}ms itemLoop={10}ms mobileLoop={11}ms regionChanged={12}ms topTotal={13} topMax={14}:{15}ms",
				stats.Calls,
				avg,
				stats.MaxMs,
				stats.MovementCalls,
				stats.TeleportCalls,
				stats.SlowCalls,
				stats.UpdateRegionMs,
				stats.MapOnMoveMs,
				stats.OldClientsRangeMs + stats.OldClientsRemoveMs,
				stats.OurStateGetObjectsMs,
				stats.OurStateItemLoopMs,
				stats.OurStateMobileLoopMs,
				stats.RegionOnLocationChangedMs,
				GetTopTotalPhase( stats ),
				stats.MaxPhaseName,
				stats.MaxPhaseMs );

			DiagnosticConsole.WriteParsedLine( "[Lag] SetLocationCounts 30s: objectsAvg={0} itemsAvg={1} mobilesAvg={2} oldClientsAvg={3} removePackets={4} itemsSent={5} mobileIncomingSelf={6} mobileIncomingOthers={7} losChecks={8} canSeeChecks={9} oplPackets={10} healthbars={11} worst={12} account={13} map={14} start={15} end={16} movement={17} teleport={18} top={19}:{20}ms",
				objectsAvg,
				itemsAvg,
				mobilesAvg,
				oldClientsAvg,
				stats.RemovePacketsSent,
				stats.ItemsSentToSelf,
				stats.MobileIncomingSentToSelf,
				stats.MobileIncomingSentToOthers,
				stats.InLOSChecks + stats.LineOfSightChecks,
				stats.CanSeeChecks,
				stats.OPLPacketsSent,
				stats.HealthbarPacketsSent,
				stats.WorstMobile,
				stats.WorstAccount,
				stats.WorstMap,
				stats.WorstStart,
				stats.WorstEnd,
				stats.WorstMovement,
				stats.WorstTeleport,
				stats.WorstTopPhase,
				stats.WorstTopPhaseMs );
		}

		public static void WriteStats( TextWriter writer )
		{
			if ( writer == null )
				return;

			lock ( m_Lock )
			{
				WriteWindow( writer, "Current SetLocation window", m_Current );
				WriteWindow( writer, "Last SetLocation window", m_Last );
			}
		}

		private static void WriteWindow( TextWriter writer, string label, WindowStats stats )
		{
			long avg = stats.Calls > 0 ? stats.TotalMs / stats.Calls : 0;
			long objectsAvg = stats.Calls > 0 ? stats.ObjectsInRangeCount / stats.Calls : 0;
			long itemsAvg = stats.Calls > 0 ? stats.ItemsConsidered / stats.Calls : 0;
			long mobilesAvg = stats.Calls > 0 ? stats.MobilesConsidered / stats.Calls : 0;
			long oldClientsAvg = stats.Calls > 0 ? stats.OldClientsInRangeCount / stats.Calls : 0;

			writer.WriteLine( "{0}: calls={1} movement={2} teleport={3} slow={4}/{5} avg={6}ms max={7}ms last={8}ms topTotal={9} topMax={10}:{11}ms",
				label,
				stats.Calls,
				stats.MovementCalls,
				stats.TeleportCalls,
				stats.SlowCalls,
				stats.SlowMovementCalls,
				avg,
				stats.MaxMs,
				stats.LastMs,
				GetTopTotalPhase( stats ),
				stats.MaxPhaseName,
				stats.MaxPhaseMs );

			writer.WriteLine( "  phases total: pre={0} assign={1} updateRegion={2} bankBox={3} trades={4} mapOnMove={5} teleportUpdate={6} oldRange={7} oldRemove={8} getObjects={9} itemLoop={10} mobileLoop={11} otherRange={12} otherLoop={13} onLocation={14} regionChanged={15} other={16}",
				stats.PrecheckMs,
				stats.AssignLocationMs,
				stats.UpdateRegionMs,
				stats.BankBoxCloseMs,
				stats.ValidateTradesMs,
				stats.MapOnMoveMs,
				stats.TeleportUpdateMs,
				stats.OldClientsRangeMs,
				stats.OldClientsRemoveMs,
				stats.OurStateGetObjectsMs,
				stats.OurStateItemLoopMs,
				stats.OurStateMobileLoopMs,
				stats.OtherClientsGetRangeMs,
				stats.OtherClientsLoopMs,
				stats.OnLocationChangeMs,
				stats.RegionOnLocationChangedMs,
				stats.OtherMs );

			writer.WriteLine( "  subphases total: canSee={0} oldLos={1} newLos={2} incoming={3} health={4} opl={5} remove={6} bonded={7}",
				stats.CanSeeMs,
				stats.OldClientLOSMs,
				stats.NewClientLOSMs,
				stats.MobileIncomingMs,
				stats.HealthbarPacketMs,
				stats.OPLPacketMs,
				stats.RemovePacketMs,
				stats.BondedPacketMs );

			writer.WriteLine( "  averages/counts: objectsAvg={0} itemsAvg={1} mobilesAvg={2} oldClientsAvg={3} removePackets={4} itemsSent={5} mobileIncomingSelf={6} mobileIncomingOthers={7} healthbars={8} oplPackets={9} bondedPackets={10} losChecks={11} lineOfSightChecks={12} canSeeChecks={13} ourStateBranches={14} noStateBranches={15}",
				objectsAvg,
				itemsAvg,
				mobilesAvg,
				oldClientsAvg,
				stats.RemovePacketsSent,
				stats.ItemsSentToSelf,
				stats.MobileIncomingSentToSelf,
				stats.MobileIncomingSentToOthers,
				stats.HealthbarPacketsSent,
				stats.OPLPacketsSent,
				stats.BondedPacketsSent,
				stats.InLOSChecks,
				stats.LineOfSightChecks,
				stats.CanSeeChecks,
				stats.OurStateBranchCount,
				stats.NoNetStateBranchCount );

			writer.WriteLine( "  worst: mobile={0} account={1} map={2} start={3} end={4} target={5} dir={6} movement={7} teleport={8} top={9}:{10}ms",
				stats.WorstMobile,
				stats.WorstAccount,
				stats.WorstMap,
				stats.WorstStart,
				stats.WorstEnd,
				stats.WorstTarget,
				stats.WorstDirection,
				stats.WorstMovement,
				stats.WorstTeleport,
				stats.WorstTopPhase,
				stats.WorstTopPhaseMs );
		}
	}
}
