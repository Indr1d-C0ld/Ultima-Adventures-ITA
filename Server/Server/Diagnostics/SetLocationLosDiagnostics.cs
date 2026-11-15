/***************************************************************************
 *                 SetLocationLosDiagnostics.cs
 *                 ----------------------------
 *   Attribution for rare Mobile.SetLocation LOS/canSee outliers.
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Server.Accounting;

namespace Server.Diagnostics
{
	public static class SetLocationLosDiagnostics
	{
		private const long SummaryInterval = 60000;
		private const long SlowCheckThreshold = 100;
		private const long RecordCheckThreshold = 25;
		private const int MaxTypeStats = 128;

		private static readonly object m_Lock = new object();
		private static readonly WindowStats m_Current = new WindowStats();
		private static readonly WindowStats m_Last = new WindowStats();
		private static readonly WindowStats m_WorstEver = new WindowStats();
		private static long m_NextSummary;

		private sealed class TypeStat
		{
			public string Name;
			public long Count;
			public long TotalMs;
			public long MaxMs;
			public long OldLosMs;
			public long NewLosMs;
			public long CanSeeMs;
			public long LineOfSightMs;
			public long OldLosMaxMs;
			public long NewLosMaxMs;
			public long CanSeeMaxMs;
			public long LineOfSightMaxMs;

			public void Add( string phase, long elapsed )
			{
				++Count;
				TotalMs += elapsed;

				if ( elapsed > MaxMs )
					MaxMs = elapsed;

				if ( phase == "oldLos" )
				{
					OldLosMs += elapsed;
					if ( elapsed > OldLosMaxMs )
						OldLosMaxMs = elapsed;
				}
				else if ( phase == "newLos" )
				{
					NewLosMs += elapsed;
					if ( elapsed > NewLosMaxMs )
						NewLosMaxMs = elapsed;
				}
				else if ( phase == "canSee" )
				{
					CanSeeMs += elapsed;
					if ( elapsed > CanSeeMaxMs )
						CanSeeMaxMs = elapsed;
				}
				else if ( phase == "oldLineOfSight" )
				{
					LineOfSightMs += elapsed;
					if ( elapsed > LineOfSightMaxMs )
						LineOfSightMaxMs = elapsed;
				}
			}
		}

		private sealed class WindowStats
		{
			public long Checks;
			public long SlowChecks;
			public long TotalMs;
			public long MaxMs;
			public long OldLosMs;
			public long NewLosMs;
			public long CanSeeMs;
			public long LineOfSightMs;
			public long OldLosMaxMs;
			public long NewLosMaxMs;
			public long CanSeeMaxMs;
			public long LineOfSightMaxMs;
			public string WorstPhase;
			public string WorstTargetType;
			public string WorstTarget;
			public string WorstTargetAccount;
			public string WorstTargetMap;
			public Point3D WorstTargetLocation;
			public string WorstMoverType;
			public string WorstMover;
			public string WorstMoverAccount;
			public string WorstMoverMap;
			public Point3D WorstMoverOldLocation;
			public Point3D WorstMoverLocation;
			public bool WorstAITimerContext;
			public bool WorstPlayerMovementContext;
			public DateTime WorstTime;
			public long WorstTick;

			public Dictionary<string, TypeStat> TargetTypes = new Dictionary<string, TypeStat>();
			public Dictionary<string, TypeStat> MoverTypes = new Dictionary<string, TypeStat>();
			public Dictionary<string, TypeStat> PhaseTypes = new Dictionary<string, TypeStat>();

			public void Reset()
			{
				Checks = 0;
				SlowChecks = 0;
				TotalMs = 0;
				MaxMs = 0;
				OldLosMs = 0;
				NewLosMs = 0;
				CanSeeMs = 0;
				LineOfSightMs = 0;
				OldLosMaxMs = 0;
				NewLosMaxMs = 0;
				CanSeeMaxMs = 0;
				LineOfSightMaxMs = 0;
				WorstPhase = null;
				WorstTargetType = null;
				WorstTarget = null;
				WorstTargetAccount = null;
				WorstTargetMap = null;
				WorstTargetLocation = Point3D.Zero;
				WorstMoverType = null;
				WorstMover = null;
				WorstMoverAccount = null;
				WorstMoverMap = null;
				WorstMoverOldLocation = Point3D.Zero;
				WorstMoverLocation = Point3D.Zero;
				WorstAITimerContext = false;
				WorstPlayerMovementContext = false;
				WorstTime = DateTime.MinValue;
				WorstTick = 0;
				TargetTypes.Clear();
				MoverTypes.Clear();
				PhaseTypes.Clear();
			}

			public void CopyFrom( WindowStats other )
			{
				Checks = other.Checks;
				SlowChecks = other.SlowChecks;
				TotalMs = other.TotalMs;
				MaxMs = other.MaxMs;
				OldLosMs = other.OldLosMs;
				NewLosMs = other.NewLosMs;
				CanSeeMs = other.CanSeeMs;
				LineOfSightMs = other.LineOfSightMs;
				OldLosMaxMs = other.OldLosMaxMs;
				NewLosMaxMs = other.NewLosMaxMs;
				CanSeeMaxMs = other.CanSeeMaxMs;
				LineOfSightMaxMs = other.LineOfSightMaxMs;
				WorstPhase = other.WorstPhase;
				WorstTargetType = other.WorstTargetType;
				WorstTarget = other.WorstTarget;
				WorstTargetAccount = other.WorstTargetAccount;
				WorstTargetMap = other.WorstTargetMap;
				WorstTargetLocation = other.WorstTargetLocation;
				WorstMoverType = other.WorstMoverType;
				WorstMover = other.WorstMover;
				WorstMoverAccount = other.WorstMoverAccount;
				WorstMoverMap = other.WorstMoverMap;
				WorstMoverOldLocation = other.WorstMoverOldLocation;
				WorstMoverLocation = other.WorstMoverLocation;
				WorstAITimerContext = other.WorstAITimerContext;
				WorstPlayerMovementContext = other.WorstPlayerMovementContext;
				WorstTime = other.WorstTime;
				WorstTick = other.WorstTick;

				TargetTypes.Clear();

				CopyStats( other.TargetTypes, TargetTypes );
				CopyStats( other.MoverTypes, MoverTypes );
				CopyStats( other.PhaseTypes, PhaseTypes );
			}
		}

		public static void Record( Mobile mover, Mobile target, string phase, long elapsed, Point3D oldLocation )
		{
			if ( mover == null || target == null || elapsed < RecordCheckThreshold )
				return;

			string targetType = GetTypeName( target );
			string moverType = GetTypeName( mover );
			bool slow = ( elapsed >= SlowCheckThreshold );
			bool logSummary = false;

			lock ( m_Lock )
			{
				AddSample( m_Current, mover, target, targetType, moverType, phase, elapsed, oldLocation );

				if ( elapsed > m_WorstEver.MaxMs )
				{
					SetWorstSample( m_WorstEver, mover, target, targetType, moverType, phase, elapsed, oldLocation );
					m_WorstEver.Checks = 1;
					m_WorstEver.SlowChecks = slow ? 1 : 0;
					m_WorstEver.TotalMs = elapsed;
					AddPhaseTotal( m_WorstEver, phase, elapsed );
				}

				long now = Core.TickCount;

				if ( m_NextSummary == 0 )
				{
					m_NextSummary = now + SummaryInterval;
				}
				else if ( now >= m_NextSummary )
				{
					m_NextSummary = now + SummaryInterval;
					m_Last.CopyFrom( m_Current );
					logSummary = ( m_Current.SlowChecks > 0 );
					m_Current.Reset();
				}
			}

			if ( slow )
				LogSlowCheck( mover, target, targetType, phase, elapsed, oldLocation );

			if ( logSummary )
				LogWindow( m_Last );
		}

		private static void AddSample( WindowStats stats, Mobile mover, Mobile target, string targetType, string moverType, string phase, long elapsed, Point3D oldLocation )
		{
			++stats.Checks;
			stats.TotalMs += elapsed;

			if ( elapsed >= SlowCheckThreshold )
				++stats.SlowChecks;

			AddPhaseTotal( stats, phase, elapsed );

			TypeStat stat;

			if ( !stats.TargetTypes.TryGetValue( targetType, out stat ) )
			{
				if ( stats.TargetTypes.Count < MaxTypeStats )
				{
					stat = new TypeStat();
					stat.Name = targetType;
					stats.TargetTypes[targetType] = stat;
				}
			}

			if ( stat != null )
				stat.Add( phase, elapsed );

			AddTypeStat( stats.MoverTypes, moverType, phase, elapsed );
			AddTypeStat( stats.PhaseTypes, phase, phase, elapsed );

			if ( elapsed > stats.MaxMs )
				SetWorstSample( stats, mover, target, targetType, moverType, phase, elapsed, oldLocation );
		}

		private static void AddPhaseTotal( WindowStats stats, string phase, long elapsed )
		{
			if ( phase == "oldLos" )
			{
				stats.OldLosMs += elapsed;
				if ( elapsed > stats.OldLosMaxMs )
					stats.OldLosMaxMs = elapsed;
			}
			else if ( phase == "newLos" )
			{
				stats.NewLosMs += elapsed;
				if ( elapsed > stats.NewLosMaxMs )
					stats.NewLosMaxMs = elapsed;
			}
			else if ( phase == "canSee" )
			{
				stats.CanSeeMs += elapsed;
				if ( elapsed > stats.CanSeeMaxMs )
					stats.CanSeeMaxMs = elapsed;
			}
			else if ( phase == "oldLineOfSight" )
			{
				stats.LineOfSightMs += elapsed;
				if ( elapsed > stats.LineOfSightMaxMs )
					stats.LineOfSightMaxMs = elapsed;
			}
		}

		private static void AddTypeStat( Dictionary<string, TypeStat> table, string name, string phase, long elapsed )
		{
			TypeStat stat;

			if ( !table.TryGetValue( name, out stat ) )
			{
				if ( table.Count >= MaxTypeStats )
					return;

				stat = new TypeStat();
				stat.Name = name;
				table[name] = stat;
			}

			stat.Add( phase, elapsed );
		}

		private static void CopyStats( Dictionary<string, TypeStat> from, Dictionary<string, TypeStat> to )
		{
			to.Clear();

			foreach ( TypeStat source in from.Values )
			{
				TypeStat copy = new TypeStat();
				copy.Name = source.Name;
				copy.Count = source.Count;
				copy.TotalMs = source.TotalMs;
				copy.MaxMs = source.MaxMs;
				copy.OldLosMs = source.OldLosMs;
				copy.NewLosMs = source.NewLosMs;
				copy.CanSeeMs = source.CanSeeMs;
				copy.LineOfSightMs = source.LineOfSightMs;
				copy.OldLosMaxMs = source.OldLosMaxMs;
				copy.NewLosMaxMs = source.NewLosMaxMs;
				copy.CanSeeMaxMs = source.CanSeeMaxMs;
				copy.LineOfSightMaxMs = source.LineOfSightMaxMs;
				to[copy.Name] = copy;
			}
		}

		private static void SetWorstSample( WindowStats stats, Mobile mover, Mobile target, string targetType, string moverType, string phase, long elapsed, Point3D oldLocation )
		{
			stats.MaxMs = elapsed;
			stats.WorstPhase = phase;
			stats.WorstTargetType = targetType;
			stats.WorstTarget = GetMobileText( target );
			stats.WorstTargetAccount = GetAccountText( target );
			stats.WorstTargetMap = target.Map != null ? target.Map.ToString() : "null";
			stats.WorstTargetLocation = target.Location;
			stats.WorstMoverType = moverType;
			stats.WorstMover = GetMobileText( mover );
			stats.WorstMoverAccount = GetAccountText( mover );
			stats.WorstMoverMap = mover.Map != null ? mover.Map.ToString() : "null";
			stats.WorstMoverOldLocation = oldLocation;
			stats.WorstMoverLocation = mover.Location;
			stats.WorstAITimerContext = OnMovementDiagnostics.IsAITimerContext;
			stats.WorstPlayerMovementContext = ( mover.NetState != null );
			stats.WorstTime = DateTime.UtcNow;
			stats.WorstTick = Core.TickCount;
		}

		private static string GetTypeName( Mobile mobile )
		{
			Type type = mobile != null ? mobile.GetType() : null;

			return type != null ? type.FullName : "null";
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

		private static TypeStat GetTopByTotal( Dictionary<string, TypeStat> table )
		{
			TypeStat best = null;

			foreach ( TypeStat stat in table.Values )
			{
				if ( best == null || stat.TotalMs > best.TotalMs )
					best = stat;
			}

			return best;
		}

		private static TypeStat GetTopByMax( Dictionary<string, TypeStat> table )
		{
			TypeStat best = null;

			foreach ( TypeStat stat in table.Values )
			{
				if ( best == null || stat.MaxMs > best.MaxMs )
					best = stat;
			}

			return best;
		}

		private static string FormatStat( TypeStat stat )
		{
			if ( stat == null )
				return "none";

			long avg = stat.Count > 0 ? stat.TotalMs / stat.Count : 0;

			return String.Format( "{0} count={1} total={2}ms max={3}ms avg={4}ms oldLos={5}/{6}ms newLos={7}/{8}ms canSee={9}/{10}ms lineOfSight={11}/{12}ms",
				stat.Name,
				stat.Count,
				stat.TotalMs,
				stat.MaxMs,
				avg,
				stat.OldLosMs,
				stat.OldLosMaxMs,
				stat.NewLosMs,
				stat.NewLosMaxMs,
				stat.CanSeeMs,
				stat.CanSeeMaxMs,
				stat.LineOfSightMs,
				stat.LineOfSightMaxMs );
		}

		private static void LogSlowCheck( Mobile mover, Mobile target, string targetType, string phase, long elapsed, Point3D oldLocation )
		{
			DiagnosticConsole.WriteParsedLine( "[Lag] SetLocation LOS slow {0}ms mover={1} {2} acct={3} target={4} {5} acct={6} phase={7} map={8} moverOld={9} moverLoc={10} targetLoc={11} aiTimer={12} playerMovement={13}",
				elapsed,
				GetTypeName( mover ),
				GetMobileText( mover ),
				GetAccountText( mover ),
				targetType,
				GetMobileText( target ),
				GetAccountText( target ),
				phase,
				mover != null ? mover.Map : null,
				oldLocation,
				mover != null ? mover.Location : Point3D.Zero,
				target != null ? target.Location : Point3D.Zero,
				OnMovementDiagnostics.IsAITimerContext,
				mover != null && mover.NetState != null );
		}

		private static void LogWindow( WindowStats stats )
		{
			DiagnosticConsole.WriteParsedLine( "[Lag] SetLocationLosStats 60s: checks={0} slow={1} total={2}ms max={3}ms oldLos={4}ms newLos={5}ms canSee={6}ms lineOfSight={7}ms topTotal={8}",
				stats.Checks,
				stats.SlowChecks,
				stats.TotalMs,
				stats.MaxMs,
				stats.OldLosMs,
				stats.NewLosMs,
				stats.CanSeeMs,
				stats.LineOfSightMs,
				FormatStat( GetTopByTotal( stats.TargetTypes ) ) );
		}

		private static void WriteWindow( TextWriter writer, string label, WindowStats stats )
		{
			writer.WriteLine( "SetLocationLOS {0}: checks={1}, slow={2}, total={3}ms, max={4}ms, oldLos={5}/{6}ms, newLos={7}/{8}ms, canSee={9}/{10}ms, lineOfSight={11}/{12}ms",
				label,
				stats.Checks,
				stats.SlowChecks,
				stats.TotalMs,
				stats.MaxMs,
				stats.OldLosMs,
				stats.OldLosMaxMs,
				stats.NewLosMs,
				stats.NewLosMaxMs,
				stats.CanSeeMs,
				stats.CanSeeMaxMs,
				stats.LineOfSightMs,
				stats.LineOfSightMaxMs );

			writer.WriteLine( "  topTargetTotal: {0}", FormatStat( GetTopByTotal( stats.TargetTypes ) ) );
			writer.WriteLine( "  topTargetMax: {0}", FormatStat( GetTopByMax( stats.TargetTypes ) ) );
			writer.WriteLine( "  topMoverTotal: {0}", FormatStat( GetTopByTotal( stats.MoverTypes ) ) );
			writer.WriteLine( "  topMoverMax: {0}", FormatStat( GetTopByMax( stats.MoverTypes ) ) );
			writer.WriteLine( "  phaseTotals: {0}", FormatStat( GetTopByTotal( stats.PhaseTypes ) ) );
			writer.WriteLine( "  worst: elapsed={0}ms phase={1} utc={2} tick={3} mover={4} {5} acct={6} old={7} loc={8}/{9} target={10} {11} acct={12} loc={13}/{14} aiTimer={15} playerMovement={16}",
				stats.MaxMs,
				stats.WorstPhase,
				stats.WorstTime,
				stats.WorstTick,
				stats.WorstMoverType,
				stats.WorstMover,
				stats.WorstMoverAccount,
				stats.WorstMoverOldLocation,
				stats.WorstMoverMap,
				stats.WorstMoverLocation,
				stats.WorstTargetType,
				stats.WorstTarget,
				stats.WorstTargetAccount,
				stats.WorstTargetMap,
				stats.WorstTargetLocation,
				stats.WorstAITimerContext,
				stats.WorstPlayerMovementContext );
		}

		public static void WriteStats( TextWriter writer )
		{
			if ( writer == null )
				return;

			WindowStats current = new WindowStats();
			WindowStats last = new WindowStats();
			WindowStats worstEver = new WindowStats();

			lock ( m_Lock )
			{
				current.CopyFrom( m_Current );
				last.CopyFrom( m_Last );
				worstEver.CopyFrom( m_WorstEver );
			}

			WriteWindow( writer, "current", current );
			WriteWindow( writer, "last", last );
			WriteWindow( writer, "worstEver", worstEver );
		}
	}
}
