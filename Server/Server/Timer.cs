/***************************************************************************
 *                                 Timer.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id$
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Server.Diagnostics;

namespace Server
{
	public enum TimerPriority
	{
		EveryTick,
		TenMS,
		TwentyFiveMS,
		FiftyMS,
		TwoFiftyMS,
		OneSecond,
		FiveSeconds,
		OneMinute
	}

	public delegate void TimerCallback();
	public delegate void TimerStateCallback( object state );
	public delegate void TimerStateCallback<T>( T state );

	public class Timer
	{
		private long m_Next;
		private long m_Delay;
		private long m_Interval;
		private bool m_Running;
		private int m_Index, m_Count;
		private TimerPriority m_Priority;
		private List<Timer> m_List;
		private bool m_PrioritySet;

		private static string FormatDelegate( Delegate callback )
		{
			if ( callback == null )
				return "null";

			return String.Format( "{0}.{1}", callback.Method.DeclaringType.FullName, callback.Method.Name );
		}

		public static void DumpInfo( TextWriter tw )
		{
			TimerThread.DumpInfo( tw );
		}

		public TimerPriority Priority
		{
			get
			{
				return m_Priority;
			}
			set
			{
				if ( !m_PrioritySet )
					m_PrioritySet = true;

				if ( m_Priority != value )
				{
					m_Priority = value;

					if ( m_Running )
						TimerThread.PriorityChange( this, (int)m_Priority );
				}
			}
		}

		public DateTime Next
		{
			// Obnoxious
			get { return DateTime.UtcNow + TimeSpan.FromMilliseconds(m_Next-Core.TickCount); }
		}

		public TimeSpan Delay
		{
			get { return TimeSpan.FromMilliseconds(m_Delay); }
			set { m_Delay = (long)value.TotalMilliseconds; }
		}

		public TimeSpan Interval
		{
			get { return TimeSpan.FromMilliseconds(m_Interval); }
			set { m_Interval = (long)value.TotalMilliseconds; }
		}

		public bool Running
		{
			get { return m_Running; }
			set {
				if ( value ) {
					Start();
				} else {
					Stop();
				}
			}
		}

		public TimerProfile GetProfile()
		{
			if ( !Core.Profiling ) {
				return null;
			}

			string name = ToString();

			if ( name == null ) {
				name = "null";
			}

			return TimerProfile.Acquire( name );
		}

		public class TimerThread
		{
			private static Dictionary<Timer,TimerChangeEntry> m_Changed = new Dictionary<Timer,TimerChangeEntry>();

			private static long[] m_NextPriorities = new long[8];
			private static long[] m_PriorityDelays = new long[8]
			{
				0,
				10,
				25,
				50,
				250,
				1000,
				5000,
				60000
			};

			private static List<Timer>[] m_Timers = new List<Timer>[8]
			{
				new List<Timer>(),
				new List<Timer>(),
				new List<Timer>(),
				new List<Timer>(),
				new List<Timer>(),
				new List<Timer>(),
				new List<Timer>(),
				new List<Timer>(),
			};

			public static void DumpInfo( TextWriter tw )
			{
				for ( int i = 0; i < 8; ++i )
				{
					tw.WriteLine( "Priority: {0}", (TimerPriority)i );
					tw.WriteLine();

					Dictionary<string, List<Timer>> hash = new Dictionary<string, List<Timer>>();

					for ( int j = 0; j < m_Timers[i].Count; ++j )
					{
						Timer t = m_Timers[i][j];

						string key = t.ToString();

						List<Timer> list;
						hash.TryGetValue( key, out list );

						if ( list == null )
							hash[key] = list = new List<Timer>();

						list.Add( t );
					}

					foreach ( KeyValuePair<string, List<Timer>> kv in hash )
					{
						string key = kv.Key;
						List<Timer> list = kv.Value;

						tw.WriteLine( "Type: {0}; Count: {1}; Percent: {2}%", key, list.Count, (int)(100 * (list.Count / (double)m_Timers[i].Count)) );
					}

					tw.WriteLine();
					tw.WriteLine();
				}
			}

			private class TimerChangeEntry
			{
				public Timer m_Timer;
				public int m_NewIndex;
				public bool m_IsAdd;

				private TimerChangeEntry( Timer t, int newIndex, bool isAdd )
				{
					m_Timer = t;
					m_NewIndex = newIndex;
					m_IsAdd = isAdd;
				}

				public void Free()
				{
					lock (m_InstancePool) {
						if (m_InstancePool.Count < 200) // Arbitrary
							m_InstancePool.Enqueue( this );
					}
				}

				private static Queue<TimerChangeEntry> m_InstancePool = new Queue<TimerChangeEntry>();

				public static TimerChangeEntry GetInstance( Timer t, int newIndex, bool isAdd )
				{
					TimerChangeEntry e = null;

					lock (m_InstancePool) {
						if ( m_InstancePool.Count > 0 ) {
						e = m_InstancePool.Dequeue();
						}
					}

					if (e != null) {
							e.m_Timer = t;
							e.m_NewIndex = newIndex;
							e.m_IsAdd = isAdd;
					} else {
						e = new TimerChangeEntry( t, newIndex, isAdd );
					}

					return e;
				}
			}

			public TimerThread()
			{
			}

			public static void Change( Timer t, int newIndex, bool isAdd )
			{
				lock (m_Changed)
					m_Changed[t] = TimerChangeEntry.GetInstance(t, newIndex, isAdd);
				m_Signal.Set();
			}

			public static void AddTimer( Timer t )
			{
				Change( t, (int)t.Priority, true );
			}

			public static void PriorityChange( Timer t, int newPrio )
			{
				Change( t, newPrio, false );
			}

			public static void RemoveTimer( Timer t )
			{
				Change( t, -1, false );
			}

			private static void ProcessChanged()
			{
				lock (m_Changed) {
					long curTicks = Core.TickCount;

					foreach (TimerChangeEntry tce in m_Changed.Values) {
					Timer timer = tce.m_Timer;
					int newIndex = tce.m_NewIndex;

					if ( timer.m_List != null )
						timer.m_List.Remove( timer );

						if (tce.m_IsAdd) {
							timer.m_Next = curTicks + timer.m_Delay;
						timer.m_Index = 0;
					}

						if (newIndex >= 0) {
						timer.m_List = m_Timers[newIndex];
						timer.m_List.Add( timer );
						} else {
						timer.m_List = null;
					}

					tce.Free();
				}

					m_Changed.Clear();
				}
			}

			private static AutoResetEvent m_Signal = new AutoResetEvent( false );
			public static void Set() { m_Signal.Set(); }

			public void TimerMain()
			{
				long now;
				int i, j;
				bool loaded;

				while ( !Core.Closing )
				{
					if (World.Loading || World.Saving)
					{
						m_Signal.WaitOne(1, false);
						continue;
					}

					ProcessChanged();

					loaded = false;

					for ( i = 0; i < m_Timers.Length; i++)
					{
						now = Core.TickCount;
						if ( now < m_NextPriorities[i] )
							break;

						m_NextPriorities[i] = now + m_PriorityDelays[i];

						for ( j = 0; j < m_Timers[i].Count; j++)
						{
							Timer t = m_Timers[i][j];

							if ( !t.m_Queued && now > t.m_Next )
							{
								t.m_Queued = true;

								lock ( m_Queue )
									m_Queue.Enqueue( t );

								loaded = true;
									
								if ( t.m_Count != 0 && (++t.m_Index >= t.m_Count) )
								{
									t.Stop();
								}
								else
								{
									t.m_Next = now + t.m_Interval;
								}
							}
						}
					}

					if ( loaded )
						Core.Set();

					m_Signal.WaitOne(1, false);
				}
			}
		}

		private static Queue<Timer> m_Queue = new Queue<Timer>();
		private static int m_BreakCount = 20000;
		private const long SlowTimerCallbackThreshold = 50;
		private const long SlowTimerSliceThreshold = 200;
		private const long SlowTimerSliceLogInterval = 30000;
		private const long TimerSliceEntryThreshold = 10;
		private const int MaxTimerSliceEntries = 8;
		private static long m_LastSliceDuration;
		private static long m_MaxSliceDuration;
		private static int m_LastQueueCountAtSlice;
		private static int m_MaxQueueCountAtSlice;
		private static long m_NextSlowSliceLog;
		private static object m_TimerSliceBreakdownLock = new object();
		private static TimerSliceSample m_LastSlowTimerSlice;
		private static TimerSliceSample m_MaxSlowTimerSlice;
		private static object m_TimerStatsLock = new object();
		private static Dictionary<string, TimerStat> m_CurrentTimerStats = new Dictionary<string, TimerStat>();
		private static Dictionary<string, TimerStat> m_LastTimerStats = new Dictionary<string, TimerStat>();
		private static long m_NextTimerStatsSummary;
		private const long TimerStatsSummaryInterval = 60000;
		private const int MaxTimerStats = 128;

		private class TimerStat
		{
			public string Name;
			public string Priority;
			public bool Repeating;
			public TimeSpan Delay;
			public TimeSpan Interval;
			public long Count;
			public long SlowCount;
			public long MaxElapsed;
			public long TotalElapsed;
			public long LastElapsed;
			public DateTime LastOccurrence;
			public string SampleTimerType;

			public void CopyFrom( TimerStat other )
			{
				Name = other.Name;
				Priority = other.Priority;
				Repeating = other.Repeating;
				Delay = other.Delay;
				Interval = other.Interval;
				Count = other.Count;
				SlowCount = other.SlowCount;
				MaxElapsed = other.MaxElapsed;
				TotalElapsed = other.TotalElapsed;
				LastElapsed = other.LastElapsed;
				LastOccurrence = other.LastOccurrence;
				SampleTimerType = other.SampleTimerType;
			}
		}

		private sealed class TimerSliceEntry
		{
			public string Name;
			public long Elapsed;
		}

		private sealed class TimerSliceSample
		{
			public DateTime TimeUtc;
			public long SliceElapsed;
			public int QueueCount;
			public int ProcessedCount;
			public long NamedElapsed;
			public long OtherElapsed;
			public TimerSliceEntry[] Entries;
			public int EntryCount;

			public TimerSliceSample Clone()
			{
				TimerSliceSample copy = new TimerSliceSample();
				copy.TimeUtc = TimeUtc;
				copy.SliceElapsed = SliceElapsed;
				copy.QueueCount = QueueCount;
				copy.ProcessedCount = ProcessedCount;
				copy.NamedElapsed = NamedElapsed;
				copy.OtherElapsed = OtherElapsed;
				copy.EntryCount = EntryCount;

				if (Entries != null)
				{
					copy.Entries = new TimerSliceEntry[Entries.Length];

					for (int i = 0; i < Entries.Length; ++i)
					{
						if (Entries[i] == null)
							continue;

						copy.Entries[i] = new TimerSliceEntry();
						copy.Entries[i].Name = Entries[i].Name;
						copy.Entries[i].Elapsed = Entries[i].Elapsed;
					}
				}

				return copy;
			}
		}

		public static int BreakCount{ get{ return m_BreakCount; } set{ m_BreakCount = value; } }

		private static int m_QueueCountAtSlice;

		private bool m_Queued;

		public static long LastSliceDuration{ get{ return Interlocked.Read( ref m_LastSliceDuration ); } }
		public static long MaxSliceDuration{ get{ return Interlocked.Read( ref m_MaxSliceDuration ); } }
		public static int LastQueueCountAtSlice{ get{ return m_LastQueueCountAtSlice; } }
		public static int MaxQueueCountAtSlice{ get{ return m_MaxQueueCountAtSlice; } }

		private static long GetElapsedMilliseconds( long start )
		{
			return ( Stopwatch.GetTimestamp() - start ) * 1000 / Stopwatch.Frequency;
		}

		private static void SetMax( ref long target, long value )
		{
			long current;

			do
			{
				current = Interlocked.Read( ref target );

				if ( value <= current )
					return;
			}
			while ( Interlocked.CompareExchange( ref target, value, current ) != current );
		}

		private static string FormatTimer( Timer timer )
		{
			if ( timer == null )
				return "null";

			string name = timer.ToString();

			return name != null ? name : timer.GetType().FullName;
		}

		private static void RecordTimerStats( Timer timer, long elapsed )
		{
			RecordTimerStats( FormatTimer( timer ), timer, elapsed, elapsed >= SlowTimerCallbackThreshold );
		}

		private static void RecordTimerStats( string name, Timer timer, long elapsed, bool slow )
		{
			bool logSummary = false;

			lock ( m_TimerStatsLock )
			{
				TimerStat stat;

				if ( !m_CurrentTimerStats.TryGetValue( name, out stat ) )
				{
					if ( m_CurrentTimerStats.Count >= MaxTimerStats )
						return;

					stat = new TimerStat();
					stat.Name = name;
					m_CurrentTimerStats[name] = stat;
				}

				++stat.Count;
				stat.TotalElapsed += elapsed;
				stat.LastElapsed = elapsed;
				stat.LastOccurrence = DateTime.UtcNow;

				if ( slow )
					++stat.SlowCount;

				if ( elapsed > stat.MaxElapsed )
					stat.MaxElapsed = elapsed;

				if ( timer != null )
				{
					stat.Priority = timer.Priority.ToString();
					stat.Repeating = ( timer.m_Count == 0 || timer.m_Count > 1 );
					stat.Delay = timer.Delay;
					stat.Interval = timer.Interval;
					stat.SampleTimerType = timer.GetType().FullName;
				}

				long now = Core.TickCount;

				if ( m_NextTimerStatsSummary == 0 )
				{
					m_NextTimerStatsSummary = now + TimerStatsSummaryInterval;
				}
				else if ( now >= m_NextTimerStatsSummary )
				{
					m_NextTimerStatsSummary = now + TimerStatsSummaryInterval;
					CopyTimerStats( m_CurrentTimerStats, m_LastTimerStats );
					logSummary = HasSlowTimerStats( m_CurrentTimerStats );
					m_CurrentTimerStats.Clear();
				}
			}

			if ( logSummary )
				LogTimerStatsWindow();
		}

		private static void LogSlowTimerCallback( Timer timer, long elapsed )
		{
			DiagnosticConsole.WriteParsedLine( String.Format( "[Lag] Timer callback {0} took {1}ms (Priority={2}, Repeating={3}, Delay={4}, Interval={5})", FormatTimer( timer ), elapsed, timer.Priority, ( timer.m_Count == 0 || timer.m_Count > 1 ), timer.Delay, timer.Interval ) );
		}

		private static string FormatDelegate( Delegate callback, object target )
		{
			if ( callback == null )
				return "null";

			if ( callback.Method != null )
			{
				Type declaringType = callback.Method.DeclaringType;
				string typeName = declaringType != null ? declaringType.FullName : "unknown";
				string targetName = target != null ? target.GetType().FullName : null;

				if ( targetName != null && targetName != typeName )
					return String.Format( "{0}.{1} target={2}", typeName, callback.Method.Name, targetName );

				return String.Format( "{0}.{1}", typeName, callback.Method.Name );
			}

			return callback.ToString();
		}

		private static void LogSlowDelayCall( Delegate callback, object target, Timer timer, long elapsed )
		{
			string callbackName = FormatDelegate( callback, target );

			RecordTimerStats( callbackName, timer, elapsed, true );
			DiagnosticConsole.WriteParsedLine( String.Format( "[Lag] DelayCall callback {0} took {1}ms (TimerPriority={2})", callbackName, elapsed, timer.Priority ) );
		}

		private static void CopyTimerStats( Dictionary<string, TimerStat> from, Dictionary<string, TimerStat> to )
		{
			to.Clear();

			foreach ( TimerStat source in from.Values )
			{
				TimerStat copy = new TimerStat();
				copy.CopyFrom( source );
				to[copy.Name] = copy;
			}
		}

		private static bool HasSlowTimerStats( Dictionary<string, TimerStat> stats )
		{
			foreach ( TimerStat stat in stats.Values )
			{
				if ( stat.SlowCount > 0 )
					return true;
			}

			return false;
		}

		private static TimerStat GetTopTimerStat( Dictionary<string, TimerStat> stats, string mode )
		{
			TimerStat best = null;

			foreach ( TimerStat stat in stats.Values )
			{
				if ( best == null )
				{
					best = stat;
					continue;
				}

				if ( mode == "total" && stat.TotalElapsed > best.TotalElapsed )
					best = stat;
				else if ( mode == "max" && stat.MaxElapsed > best.MaxElapsed )
					best = stat;
				else if ( mode == "count" && stat.Count > best.Count )
					best = stat;
			}

			return best;
		}

		private static string FormatTimerStat( TimerStat stat )
		{
			if ( stat == null )
				return "none";

			long avg = stat.Count > 0 ? stat.TotalElapsed / stat.Count : 0;

			return String.Format( "{0} total={1}ms count={2} slow={3} max={4}ms avg={5}ms last={6}ms priority={7} repeating={8}",
				stat.Name,
				stat.TotalElapsed,
				stat.Count,
				stat.SlowCount,
				stat.MaxElapsed,
				avg,
				stat.LastElapsed,
				stat.Priority,
				stat.Repeating );
		}

		private static void AddTimerSliceEntry( ref TimerSliceEntry[] entries, ref int count, string name, long elapsed )
		{
			if ( entries == null )
				entries = new TimerSliceEntry[MaxTimerSliceEntries];

			if ( count < MaxTimerSliceEntries )
			{
				entries[count] = new TimerSliceEntry();
				entries[count].Name = name;
				entries[count].Elapsed = elapsed;
				++count;
				return;
			}

			int smallest = 0;

			for ( int i = 1; i < count; ++i )
			{
				if ( entries[i].Elapsed < entries[smallest].Elapsed )
					smallest = i;
			}

			if ( elapsed > entries[smallest].Elapsed )
			{
				entries[smallest].Name = name;
				entries[smallest].Elapsed = elapsed;
			}
		}

		private static TimerSliceSample RecordTimerSliceBreakdown( long sliceElapsed, int queueCount, int processedCount, TimerSliceEntry[] entries, int entryCount, long namedElapsed )
		{
			TimerSliceSample sample = new TimerSliceSample();
			sample.TimeUtc = DateTime.UtcNow;
			sample.SliceElapsed = sliceElapsed;
			sample.QueueCount = queueCount;
			sample.ProcessedCount = processedCount;
			sample.NamedElapsed = namedElapsed;
			sample.OtherElapsed = sliceElapsed > namedElapsed ? sliceElapsed - namedElapsed : 0;
			sample.EntryCount = entryCount;

			if ( entries != null )
			{
				sample.Entries = new TimerSliceEntry[entryCount];

				for ( int i = 0; i < entryCount; ++i )
				{
					sample.Entries[i] = new TimerSliceEntry();
					sample.Entries[i].Name = entries[i].Name;
					sample.Entries[i].Elapsed = entries[i].Elapsed;
				}

				Array.Sort( sample.Entries, delegate( TimerSliceEntry a, TimerSliceEntry b )
				{
					if ( a == null && b == null )
						return 0;

					if ( a == null )
						return 1;

					if ( b == null )
						return -1;

					return b.Elapsed.CompareTo( a.Elapsed );
				} );
			}

			lock ( m_TimerSliceBreakdownLock )
			{
				m_LastSlowTimerSlice = sample.Clone();

				if ( m_MaxSlowTimerSlice == null || sample.SliceElapsed > m_MaxSlowTimerSlice.SliceElapsed )
					m_MaxSlowTimerSlice = sample.Clone();
			}

			return sample;
		}

		private static string FormatTimerSliceSample( TimerSliceSample sample )
		{
			if ( sample == null )
				return "none";

			StringWriter writer = new StringWriter();
			WriteTimerSliceSampleLine( writer, sample );
			return writer.ToString();
		}

		private static void WriteTimerSliceSampleLine( TextWriter writer, TimerSliceSample sample )
		{
			writer.Write( "slice={0}ms queue={1} processed={2} named={3}ms other={4}ms",
				sample.SliceElapsed,
				sample.QueueCount,
				sample.ProcessedCount,
				sample.NamedElapsed,
				sample.OtherElapsed );

			if ( sample.Entries != null && sample.EntryCount > 0 )
			{
				writer.Write( " top=" );

				for ( int i = 0; i < sample.EntryCount; ++i )
				{
					if ( sample.Entries[i] == null )
						continue;

					if ( i > 0 )
						writer.Write( "; " );

					writer.Write( "{0}:{1}ms", sample.Entries[i].Name, sample.Entries[i].Elapsed );
				}
			}
		}

		public static void WriteTimerSliceBreakdown( TextWriter writer )
		{
			TimerSliceSample last;
			TimerSliceSample max;

			lock ( m_TimerSliceBreakdownLock )
			{
				last = m_LastSlowTimerSlice != null ? m_LastSlowTimerSlice.Clone() : null;
				max = m_MaxSlowTimerSlice != null ? m_MaxSlowTimerSlice.Clone() : null;
			}

			writer.Write( "TimerSliceBreakdown last: " );

			if ( last == null )
				writer.WriteLine( "Unavailable" );
			else
			{
				WriteTimerSliceSampleLine( writer, last );
				writer.WriteLine( " timeUtc={0}", last.TimeUtc );
			}

			writer.Write( "TimerSliceBreakdown max: " );

			if ( max == null )
				writer.WriteLine( "Unavailable" );
			else
			{
				WriteTimerSliceSampleLine( writer, max );
				writer.WriteLine( " timeUtc={0}", max.TimeUtc );
			}
		}

		private static void LogTimerStatsWindow()
		{
			TimerStat total;
			TimerStat max;
			TimerStat count;

			lock ( m_TimerStatsLock )
			{
				total = GetTopTimerStat( m_LastTimerStats, "total" );
				max = GetTopTimerStat( m_LastTimerStats, "max" );
				count = GetTopTimerStat( m_LastTimerStats, "count" );
			}

			DiagnosticConsole.WriteParsedLine( String.Format( "[Lag] TimerStats 60s: topTotal={0}", FormatTimerStat( total ) ) );
			DiagnosticConsole.WriteParsedLine( String.Format( "[Lag] TimerStats 60s: topMax={0} topCount={1}", FormatTimerStat( max ), FormatTimerStat( count ) ) );
		}

		public static void WriteTimerStats( TextWriter writer )
		{
			writer.WriteLine( "Timer.Slice last={0}ms max={1}ms queueLast={2} queueMax={3}", LastSliceDuration, MaxSliceDuration, LastQueueCountAtSlice, MaxQueueCountAtSlice );

			TimerStat[] current;
			TimerStat[] last;

			lock ( m_TimerStatsLock )
			{
				current = new TimerStat[m_CurrentTimerStats.Count];
				m_CurrentTimerStats.Values.CopyTo( current, 0 );
				last = new TimerStat[m_LastTimerStats.Count];
				m_LastTimerStats.Values.CopyTo( last, 0 );
			}

			WriteTimerStatsWindow( writer, "current", current );
			WriteTimerStatsWindow( writer, "last", last );
		}

		private static void WriteTimerStatsWindow( TextWriter writer, string label, TimerStat[] stats )
		{
			writer.WriteLine( "TimerStats {0}: entries={1}", label, stats.Length );
			WriteSortedTimerStats( writer, "  topTotal", CopyTimerStatsArray( stats ), "total" );
			WriteSortedTimerStats( writer, "  topMax", CopyTimerStatsArray( stats ), "max" );
		}

		private static TimerStat[] CopyTimerStatsArray( TimerStat[] stats )
		{
			TimerStat[] copy = new TimerStat[stats.Length];
			Array.Copy( stats, copy, stats.Length );
			return copy;
		}

		private static void WriteSortedTimerStats( TextWriter writer, string title, TimerStat[] stats, string mode )
		{
			writer.WriteLine( title );
			Array.Sort( stats, delegate( TimerStat a, TimerStat b )
			{
				if ( a == null && b == null )
					return 0;

				if ( a == null )
					return 1;

				if ( b == null )
					return -1;

				if ( mode == "total" )
					return b.TotalElapsed.CompareTo( a.TotalElapsed );

				return b.MaxElapsed.CompareTo( a.MaxElapsed );
			} );

			int count = Math.Min( 10, stats.Length );

			for ( int i = 0; i < count; ++i )
			{
				TimerStat stat = stats[i];

				if ( stat == null )
					continue;

				writer.WriteLine( "{0}. {1}", i + 1, FormatTimerStat( stat ) );
			}
		}

		public static void Slice()
		{
			long spawnEntrySliceId = SpawnEntryDiagnostics.BeginTimerSlice();
			long sliceStart = Stopwatch.GetTimestamp();
			int queueCount = 0;
			TimerSliceEntry[] sliceEntries = null;
			int sliceEntryCount = 0;
			long sliceNamedElapsed = 0;
			int processedCount = 0;

			lock ( m_Queue )
			{
				m_QueueCountAtSlice = m_Queue.Count;
				m_LastQueueCountAtSlice = m_QueueCountAtSlice;

				if ( m_QueueCountAtSlice > m_MaxQueueCountAtSlice )
					m_MaxQueueCountAtSlice = m_QueueCountAtSlice;

				queueCount = m_QueueCountAtSlice;

				int index = 0;

				while ( index < m_BreakCount && m_Queue.Count != 0 )
				{
					Timer t = m_Queue.Dequeue();
					TimerProfile prof = t.GetProfile();

					if ( prof != null ) {
						prof.Start();
					}

					long callbackStart = Stopwatch.GetTimestamp();

					t.OnTick();

					long callbackElapsed = GetElapsedMilliseconds( callbackStart );
					RecordTimerStats( t, callbackElapsed );

					if ( callbackElapsed >= TimerSliceEntryThreshold )
					{
						sliceNamedElapsed += callbackElapsed;
						AddTimerSliceEntry( ref sliceEntries, ref sliceEntryCount, FormatTimer( t ), callbackElapsed );
					}

					if ( callbackElapsed >= SlowTimerCallbackThreshold )
						LogSlowTimerCallback( t, callbackElapsed );

					t.m_Queued = false;
					++index;
					processedCount = index;

					if ( prof != null ) {
						prof.Finish();
					}
				}
			}

			long sliceElapsed = GetElapsedMilliseconds( sliceStart );
			Interlocked.Exchange( ref m_LastSliceDuration, sliceElapsed );
			SetMax( ref m_MaxSliceDuration, sliceElapsed );

			if ( sliceElapsed >= SlowTimerSliceThreshold )
			{
				long now = Core.TickCount;
				TimerSliceSample sample = RecordTimerSliceBreakdown( sliceElapsed, queueCount, processedCount, sliceEntries, sliceEntryCount, sliceNamedElapsed );
				SpawnEntryDiagnostics.RecordTimerSlice( spawnEntrySliceId, sliceElapsed, queueCount, processedCount, sliceNamedElapsed, sample.OtherElapsed );

				if ( now >= m_NextSlowSliceLog )
				{
					m_NextSlowSliceLog = now + SlowTimerSliceLogInterval;
					DiagnosticConsole.WriteParsedLine( String.Format( "[Lag] TimerSliceBreakdown {0}", FormatTimerSliceSample( sample ) ) );
				}
			}

			SpawnEntryDiagnostics.EndTimerSlice( spawnEntrySliceId );
		}

		public Timer( TimeSpan delay ) : this( delay, TimeSpan.Zero, 1 )
		{
		}

		public Timer( TimeSpan delay, TimeSpan interval ) : this( delay, interval, 0 )
		{
		}

		public virtual bool DefRegCreation
		{
			get{ return true; }
		}

		public void RegCreation()
		{
			TimerProfile prof = GetProfile();

			if ( prof != null ) {
				prof.Created++;
			}
		}

		public Timer( TimeSpan delay, TimeSpan interval, int count )
		{
			m_Delay = (long)delay.TotalMilliseconds;
			m_Interval = (long)interval.TotalMilliseconds;
			m_Count = count;

			if ( !m_PrioritySet ) {
				if ( count == 1 ) {
					m_Priority = ComputePriority( delay );
				} else {
					m_Priority = ComputePriority( interval );
				}
				m_PrioritySet = true;
			}

			if ( DefRegCreation )
				RegCreation();
		}

		public override string ToString()
		{
			return GetType().FullName;
		}

		public static TimerPriority ComputePriority( TimeSpan ts )
		{
			if ( ts >= TimeSpan.FromMinutes( 1.0 ) )
				return TimerPriority.FiveSeconds;

			if ( ts >= TimeSpan.FromSeconds( 10.0 ) )
				return TimerPriority.OneSecond;

			if ( ts >= TimeSpan.FromSeconds( 5.0 ) )
				return TimerPriority.TwoFiftyMS;

			if ( ts >= TimeSpan.FromSeconds( 2.5 ) )
				return TimerPriority.FiftyMS;

			if ( ts >= TimeSpan.FromSeconds( 1.0 ) )
				return TimerPriority.TwentyFiveMS;

			if ( ts >= TimeSpan.FromSeconds( 0.5 ) )
				return TimerPriority.TenMS;

			return TimerPriority.EveryTick;
		}

		#region DelayCall(..)

		public static Timer DelayCall( TimerCallback callback )
		{
			return DelayCall( TimeSpan.Zero, TimeSpan.Zero, 1, callback );
		}

		public static Timer DelayCall( TimeSpan delay, TimerCallback callback )
		{
			return DelayCall( delay, TimeSpan.Zero, 1, callback );
		}

		public static Timer DelayCall( TimeSpan delay, TimeSpan interval, TimerCallback callback )
		{
			return DelayCall( delay, interval, 0, callback );
		}

		public static Timer DelayCall( TimeSpan delay, TimeSpan interval, int count, TimerCallback callback )
		{
			Timer t = new DelayCallTimer( delay, interval, count, callback );

			if ( count == 1 )
				t.Priority = ComputePriority( delay );
			else
				t.Priority = ComputePriority( interval );

			t.Start();

			return t;
		}

		public static Timer DelayCall( TimerStateCallback callback, object state )
		{
			return DelayCall( TimeSpan.Zero, TimeSpan.Zero, 1, callback, state );
		}

		public static Timer DelayCall( TimeSpan delay, TimerStateCallback callback, object state )
		{
			return DelayCall( delay, TimeSpan.Zero, 1, callback, state );
		}

		public static Timer DelayCall( TimeSpan delay, TimeSpan interval, TimerStateCallback callback, object state )
		{
			return DelayCall( delay, interval, 0, callback, state );
		}

		public static Timer DelayCall( TimeSpan delay, TimeSpan interval, int count, TimerStateCallback callback, object state )
		{
			Timer t = new DelayStateCallTimer( delay, interval, count, callback, state );

			if ( count == 1 )
				t.Priority = ComputePriority( delay );
			else
				t.Priority = ComputePriority( interval );

			t.Start();

			return t;
		}
		#endregion

		#region DelayCall<T>(..)
		public static Timer DelayCall<T>( TimerStateCallback<T> callback, T state )
		{
			return DelayCall( TimeSpan.Zero, TimeSpan.Zero, 1, callback, state );
		}

		public static Timer DelayCall<T>( TimeSpan delay, TimerStateCallback<T> callback, T state )
		{
			return DelayCall( delay, TimeSpan.Zero, 1, callback, state );
		}

		public static Timer DelayCall<T>( TimeSpan delay, TimeSpan interval, TimerStateCallback<T> callback, T state )
		{
			return DelayCall( delay, interval, 0, callback, state );
		}

		public static Timer DelayCall<T>( TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T> callback, T state )
		{
			Timer t = new DelayStateCallTimer<T>( delay, interval, count, callback, state );

			if( count == 1 )
				t.Priority = ComputePriority( delay );
			else
				t.Priority = ComputePriority( interval );

			t.Start();

			return t;
		}
		#endregion

		#region DelayCall Timers
		private class DelayCallTimer : Timer
		{
			private TimerCallback m_Callback;

			public TimerCallback Callback{ get{ return m_Callback; } }

			public override bool DefRegCreation{ get{ return false; } }

			public DelayCallTimer( TimeSpan delay, TimeSpan interval, int count, TimerCallback callback ) : base( delay, interval, count )
			{
				m_Callback = callback;
				RegCreation();
			}

			protected override void OnTick()
			{
				if ( m_Callback != null )
				{
					long start = Stopwatch.GetTimestamp();

					try
					{
						m_Callback();
					}
					finally
					{
						long elapsed = GetElapsedMilliseconds( start );

						if ( elapsed >= SlowTimerCallbackThreshold )
							LogSlowDelayCall( m_Callback, m_Callback.Target, this, elapsed );
					}
				}
			}

			public override string ToString()
			{
				return String.Format( "DelayCallTimer[{0}]", FormatDelegate( m_Callback ) );
			}
		}

		private class DelayStateCallTimer : Timer
		{
			private TimerStateCallback m_Callback;
			private object m_State;

			public TimerStateCallback Callback{ get{ return m_Callback; } }

			public override bool DefRegCreation{ get{ return false; } }

			public DelayStateCallTimer( TimeSpan delay, TimeSpan interval, int count, TimerStateCallback callback, object state ) : base( delay, interval, count )
			{
				m_Callback = callback;
				m_State = state;

				RegCreation();
			}

			protected override void OnTick()
			{
				if ( m_Callback != null )
				{
					long start = Stopwatch.GetTimestamp();

					try
					{
						m_Callback( m_State );
					}
					finally
					{
						long elapsed = GetElapsedMilliseconds( start );

						if ( elapsed >= SlowTimerCallbackThreshold )
							LogSlowDelayCall( m_Callback, m_Callback.Target, this, elapsed );
					}
				}
			}

			public override string ToString()
			{
				return String.Format( "DelayStateCall[{0}]", FormatDelegate( m_Callback ) );
			}
		}

		private class DelayStateCallTimer<T> : Timer
		{
			private TimerStateCallback<T> m_Callback;
			private T m_State;

			public TimerStateCallback<T> Callback { get { return m_Callback; } }

			public override bool DefRegCreation { get { return false; } }

			public DelayStateCallTimer( TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T> callback, T state )
				: base( delay, interval, count )
			{
				m_Callback = callback;
				m_State = state;

				RegCreation();
			}

			protected override void OnTick()
			{
				if( m_Callback != null )
				{
					long start = Stopwatch.GetTimestamp();

					try
					{
						m_Callback( m_State );
					}
					finally
					{
						long elapsed = GetElapsedMilliseconds( start );

						if ( elapsed >= SlowTimerCallbackThreshold )
							LogSlowDelayCall( m_Callback, m_Callback.Target, this, elapsed );
					}
				}
			}

			public override string ToString()
			{
				return String.Format( "DelayStateCall[{0}]", FormatDelegate( m_Callback ) );
			}
		}
		#endregion

		public void Start()
		{
			if ( !m_Running )
			{
				m_Running = true;
				TimerThread.AddTimer( this );

				TimerProfile prof = GetProfile();

				if ( prof != null ) {
					prof.Started++;
				}
			}
		}

		public void Stop()
		{
			if ( m_Running )
			{
				m_Running = false;
				TimerThread.RemoveTimer( this );

				TimerProfile prof = GetProfile();

				if ( prof != null ) {
					prof.Stopped++;
				}
			}
		}

		protected virtual void OnTick()
		{
		}
	}
}
