/***************************************************************************
 *                    OnMovementDiagnostics.cs
 *                    ------------------------
 *   Attribution for expensive Mobile.OnMovement observer callbacks.
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Server.Accounting;

namespace Server.Diagnostics
{
	public static class OnMovementDiagnostics
	{
		private const long SummaryInterval = 30000;
		private const long SlowCallbackThreshold = 50;
		private const int MaxTypeStats = 128;

		private static readonly object m_Lock = new object();
		private static readonly WindowStats m_Current = new WindowStats();
		private static readonly WindowStats m_Last = new WindowStats();
		private static readonly WindowStats m_WorstEver = new WindowStats();
		private static long m_NextSummary;

		[ThreadStatic]
		private static int m_AITimerDepth;

		private sealed class TypeStat
		{
			public string Name;
			public long Count;
			public long TotalMs;
			public long MaxMs;
			public long SlowCount;

			public void Add( long elapsed )
			{
				++Count;
				TotalMs += elapsed;

				if ( elapsed > MaxMs )
					MaxMs = elapsed;

				if ( elapsed >= SlowCallbackThreshold )
					++SlowCount;
			}
		}

		private sealed class WindowStats
		{
			public long Callbacks;
			public long SlowCallbacks;
			public long TotalMs;
			public long MaxMs;
			public long AITimerContextCallbacks;
			public long PlayerMoverCallbacks;
			public long PlayerPacketMoverCallbacks;
			public long NPCMoverCallbacks;
			public string WorstObserver;
			public string WorstObserverType;
			public string WorstObserverAccount;
			public string WorstObserverMap;
			public Point3D WorstObserverLocation;
			public string WorstMover;
			public string WorstMoverType;
			public string WorstMoverAccount;
			public string WorstMoverMap;
			public Point3D WorstMoverLocation;
			public Point3D WorstOldLocation;
			public bool WorstAITimerContext;
			public DateTime WorstTime;
			public long WorstTick;

			public Dictionary<string, TypeStat> ObserverTypes = new Dictionary<string, TypeStat>();
			public Dictionary<string, TypeStat> MoverTypes = new Dictionary<string, TypeStat>();
			public Dictionary<string, TypeStat> PairTypes = new Dictionary<string, TypeStat>();

			public void Reset()
			{
				Callbacks = 0;
				SlowCallbacks = 0;
				TotalMs = 0;
				MaxMs = 0;
				AITimerContextCallbacks = 0;
				PlayerMoverCallbacks = 0;
				PlayerPacketMoverCallbacks = 0;
				NPCMoverCallbacks = 0;
				WorstObserver = null;
				WorstObserverType = null;
				WorstObserverAccount = null;
				WorstObserverMap = null;
				WorstObserverLocation = Point3D.Zero;
				WorstMover = null;
				WorstMoverType = null;
				WorstMoverAccount = null;
				WorstMoverMap = null;
				WorstMoverLocation = Point3D.Zero;
				WorstOldLocation = Point3D.Zero;
				WorstAITimerContext = false;
				WorstTime = DateTime.MinValue;
				WorstTick = 0;
				ObserverTypes.Clear();
				MoverTypes.Clear();
				PairTypes.Clear();
			}

			public void CopyFrom( WindowStats other )
			{
				Callbacks = other.Callbacks;
				SlowCallbacks = other.SlowCallbacks;
				TotalMs = other.TotalMs;
				MaxMs = other.MaxMs;
				AITimerContextCallbacks = other.AITimerContextCallbacks;
				PlayerMoverCallbacks = other.PlayerMoverCallbacks;
				PlayerPacketMoverCallbacks = other.PlayerPacketMoverCallbacks;
				NPCMoverCallbacks = other.NPCMoverCallbacks;
				WorstObserver = other.WorstObserver;
				WorstObserverType = other.WorstObserverType;
				WorstObserverAccount = other.WorstObserverAccount;
				WorstObserverMap = other.WorstObserverMap;
				WorstObserverLocation = other.WorstObserverLocation;
				WorstMover = other.WorstMover;
				WorstMoverType = other.WorstMoverType;
				WorstMoverAccount = other.WorstMoverAccount;
				WorstMoverMap = other.WorstMoverMap;
				WorstMoverLocation = other.WorstMoverLocation;
				WorstOldLocation = other.WorstOldLocation;
				WorstAITimerContext = other.WorstAITimerContext;
				WorstTime = other.WorstTime;
				WorstTick = other.WorstTick;

				CopyStats( other.ObserverTypes, ObserverTypes );
				CopyStats( other.MoverTypes, MoverTypes );
				CopyStats( other.PairTypes, PairTypes );
			}
		}

		public static void BeginAITimer()
		{
			++m_AITimerDepth;
		}

		public static void EndAITimer()
		{
			if ( m_AITimerDepth > 0 )
				--m_AITimerDepth;
		}

		public static bool IsAITimerContext
		{
			get{ return m_AITimerDepth > 0; }
		}

		public static long GetTimestamp()
		{
			return Stopwatch.GetTimestamp();
		}

		public static long GetElapsedMilliseconds( long start )
		{
			return ( Stopwatch.GetTimestamp() - start ) * 1000 / Stopwatch.Frequency;
		}

		public static void Record( Mobile observer, Mobile mover, Point3D oldLocation, long elapsed )
		{
			if ( observer == null || mover == null )
				return;

			bool slow = ( elapsed >= SlowCallbackThreshold );
			bool logSummary = false;

			string observerType = GetTypeName( observer );
			string moverType = GetTypeName( mover );
			string pairType = String.Format( "{0}<-{1}", observerType, moverType );
			bool aiContext = IsAITimerContext;

			lock ( m_Lock )
			{
				AddSample( m_Current, observer, mover, oldLocation, elapsed, observerType, moverType, pairType, aiContext );

				if ( elapsed > m_WorstEver.MaxMs )
				{
					SetWorstSample( m_WorstEver, observer, mover, oldLocation, elapsed, observerType, moverType, aiContext );
					m_WorstEver.Callbacks = 1;
					m_WorstEver.SlowCallbacks = slow ? 1 : 0;
					m_WorstEver.TotalMs = elapsed;
					m_WorstEver.AITimerContextCallbacks = aiContext ? 1 : 0;
					m_WorstEver.PlayerMoverCallbacks = mover.Player ? 1 : 0;
					m_WorstEver.PlayerPacketMoverCallbacks = mover.NetState != null ? 1 : 0;
					m_WorstEver.NPCMoverCallbacks = mover.Player ? 0 : 1;
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
					logSummary = ( m_Current.SlowCallbacks > 0 );
					m_Current.Reset();
				}
			}

			if ( slow )
				LogSlowCallback( observer, mover, oldLocation, elapsed, aiContext );

			if ( logSummary )
				LogWindow( m_Last );
		}

		private static void AddSample( WindowStats stats, Mobile observer, Mobile mover, Point3D oldLocation, long elapsed, string observerType, string moverType, string pairType, bool aiContext )
		{
			++stats.Callbacks;
			stats.TotalMs += elapsed;

			if ( elapsed >= SlowCallbackThreshold )
				++stats.SlowCallbacks;

			if ( aiContext )
				++stats.AITimerContextCallbacks;

			if ( mover.Player )
				++stats.PlayerMoverCallbacks;
			else
				++stats.NPCMoverCallbacks;

			if ( mover.NetState != null )
				++stats.PlayerPacketMoverCallbacks;

			AddTypeStat( stats.ObserverTypes, observerType, elapsed );
			AddTypeStat( stats.MoverTypes, moverType, elapsed );
			AddTypeStat( stats.PairTypes, pairType, elapsed );

			if ( elapsed > stats.MaxMs )
				SetWorstSample( stats, observer, mover, oldLocation, elapsed, observerType, moverType, aiContext );
		}

		private static void SetWorstSample( WindowStats stats, Mobile observer, Mobile mover, Point3D oldLocation, long elapsed, string observerType, string moverType, bool aiContext )
		{
			stats.MaxMs = elapsed;
			stats.WorstObserver = GetMobileText( observer );
			stats.WorstObserverType = observerType;
			stats.WorstObserverAccount = GetAccountText( observer );
			stats.WorstObserverMap = observer.Map != null ? observer.Map.ToString() : "null";
			stats.WorstObserverLocation = observer.Location;
			stats.WorstMover = GetMobileText( mover );
			stats.WorstMoverType = moverType;
			stats.WorstMoverAccount = GetAccountText( mover );
			stats.WorstMoverMap = mover.Map != null ? mover.Map.ToString() : "null";
			stats.WorstMoverLocation = mover.Location;
			stats.WorstOldLocation = oldLocation;
			stats.WorstAITimerContext = aiContext;
			stats.WorstTime = DateTime.UtcNow;
			stats.WorstTick = Core.TickCount;
		}

		private static void AddTypeStat( Dictionary<string, TypeStat> table, string name, long elapsed )
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

			stat.Add( elapsed );
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
				copy.SlowCount = source.SlowCount;
				to[copy.Name] = copy;
			}
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

		private static void LogSlowCallback( Mobile observer, Mobile mover, Point3D oldLocation, long elapsed, bool aiContext )
		{
			DiagnosticConsole.WriteParsedLine( "[Lag] OnMovement callback {0}ms observer={1} {2} observerAcct={3} mover={4} {5} moverAcct={6} map={7} observerLoc={8} moverLoc={9} oldMoverLoc={10} aiTimer={11}",
				elapsed,
				GetTypeName( observer ),
				GetMobileText( observer ),
				GetAccountText( observer ),
				GetTypeName( mover ),
				GetMobileText( mover ),
				GetAccountText( mover ),
				mover != null ? mover.Map : null,
				observer != null ? observer.Location : Point3D.Zero,
				mover != null ? mover.Location : Point3D.Zero,
				oldLocation,
				aiContext );
		}

		private static void LogWindow( WindowStats stats )
		{
			TypeStat topObserver = GetTopByTotal( stats.ObserverTypes );
			TypeStat topMover = GetTopByTotal( stats.MoverTypes );
			TypeStat topPair = GetTopByTotal( stats.PairTypes );

			DiagnosticConsole.WriteParsedLine( "[Lag] OnMovementStats 30s: callbacks={0} slow={1} total={2}ms max={3}ms aiContext={4} playerMovers={5} npcMovers={6}",
				stats.Callbacks,
				stats.SlowCallbacks,
				stats.TotalMs,
				stats.MaxMs,
				stats.AITimerContextCallbacks,
				stats.PlayerMoverCallbacks,
				stats.NPCMoverCallbacks );

			DiagnosticConsole.WriteParsedLine( "[Lag] OnMovementTop 30s: observer={0} mover={1} pair={2}",
				FormatStat( topObserver ),
				FormatStat( topMover ),
				FormatStat( topPair ) );
		}

		private static string FormatStat( TypeStat stat )
		{
			if ( stat == null )
				return "none";

			long avg = stat.Count > 0 ? stat.TotalMs / stat.Count : 0;

			return String.Format( "{0} count={1} total={2}ms max={3}ms avg={4}ms slow={5}", stat.Name, stat.Count, stat.TotalMs, stat.MaxMs, avg, stat.SlowCount );
		}

		private static void WriteWindow( TextWriter writer, string label, WindowStats stats )
		{
			long avg = stats.Callbacks > 0 ? stats.TotalMs / stats.Callbacks : 0;

			writer.WriteLine( "OnMovement {0}: callbacks={1}, slow={2}, total={3}ms, avg={4}ms, max={5}ms, aiContext={6}, playerMovers={7}, npcMovers={8}",
				label,
				stats.Callbacks,
				stats.SlowCallbacks,
				stats.TotalMs,
				avg,
				stats.MaxMs,
				stats.AITimerContextCallbacks,
				stats.PlayerMoverCallbacks,
				stats.NPCMoverCallbacks );
			writer.WriteLine( "  context: playerPacketMovers={0}", stats.PlayerPacketMoverCallbacks );

			writer.WriteLine( "  topObserverTotal: {0}", FormatStat( GetTopByTotal( stats.ObserverTypes ) ) );
			writer.WriteLine( "  topObserverMax: {0}", FormatStat( GetTopByMax( stats.ObserverTypes ) ) );
			writer.WriteLine( "  topMoverTotal: {0}", FormatStat( GetTopByTotal( stats.MoverTypes ) ) );
			writer.WriteLine( "  topMoverMax: {0}", FormatStat( GetTopByMax( stats.MoverTypes ) ) );
			writer.WriteLine( "  topPairByTotal: {0}", FormatStat( GetTopByTotal( stats.PairTypes ) ) );
			writer.WriteLine( "  topPairByMax: {0}", FormatStat( GetTopByMax( stats.PairTypes ) ) );
			writer.WriteLine( "  exactWorstPair: {0}<-{1}", stats.WorstObserverType, stats.WorstMoverType );
			writer.WriteLine( "  exactWorst: elapsed={0}ms utc={1} tick={2} observer={3} {4} acct={5} loc={6}/{7} mover={8} {9} acct={10} loc={11}/{12} old={13} aiTimer={14}",
				stats.MaxMs,
				stats.WorstTime,
				stats.WorstTick,
				stats.WorstObserverType,
				stats.WorstObserver,
				stats.WorstObserverAccount,
				stats.WorstObserverMap,
				stats.WorstObserverLocation,
				stats.WorstMoverType,
				stats.WorstMover,
				stats.WorstMoverAccount,
				stats.WorstMoverMap,
				stats.WorstMoverLocation,
				stats.WorstOldLocation,
				stats.WorstAITimerContext );
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
