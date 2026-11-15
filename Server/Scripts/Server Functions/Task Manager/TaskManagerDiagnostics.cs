using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Server.Items;
using Server.Network;

namespace Server.Diagnostics
{
	public static class TaskManagerDiagnostics
	{
		private const long SummaryInterval = 60000;
		private const long SlowTaskThreshold = 50;
		private const int MaxTaskStats = 64;
		private const bool VerboseTaskManagerPhaseLogging = false;

		private static readonly object m_Lock = new object();
		private static readonly Dictionary<string, TaskStat> m_Current = new Dictionary<string, TaskStat>();
		private static readonly Dictionary<string, TaskStat> m_Last = new Dictionary<string, TaskStat>();
		private static readonly Dictionary<string, TaskSample> m_LatestByTask = new Dictionary<string, TaskSample>();
		private static TaskSample m_Worst;
		private static long m_NextSummary;

		public sealed class Context
		{
			internal string TaskName;
			internal string TimerName;
			internal long RunId;
			internal bool Startup;
			internal DateTime StartedUtc;
			internal long StartTimestamp;
			internal string CurrentPhase;
			internal long PhaseStartTimestamp;
			internal int OnlineCount;
			internal long ScannedCount;
			internal long DeletedCount;
			internal long ModifiedCount;
			internal long CreatedCount;
			private Dictionary<string, PhaseStat> m_Phases = new Dictionary<string, PhaseStat>();

			internal Dictionary<string, PhaseStat> Phases
			{
				get { return m_Phases; }
			}
		}

		private sealed class TaskStat
		{
			public string Name;
			public long Count;
			public long StartupCount;
			public long SlowCount;
			public long TotalMs;
			public long MaxMs;
			public long LastMs;
			public long ScannedCount;
			public long DeletedCount;
			public long ModifiedCount;
			public long CreatedCount;
			public long ErrorCount;

			public void Add(TaskSample sample)
			{
				++Count;

				if (sample.Startup)
					++StartupCount;

				TotalMs += sample.TotalMs;
				LastMs = sample.TotalMs;

				if (sample.TotalMs > MaxMs)
					MaxMs = sample.TotalMs;

				if (sample.TotalMs >= SlowTaskThreshold)
					++SlowCount;

				ScannedCount += sample.ScannedCount;
				DeletedCount += sample.DeletedCount;
				ModifiedCount += sample.ModifiedCount;
				CreatedCount += sample.CreatedCount;
				ErrorCount += sample.ErrorCount;
			}

			public TaskStat Clone()
			{
				TaskStat copy = new TaskStat();
				copy.Name = Name;
				copy.Count = Count;
				copy.StartupCount = StartupCount;
				copy.SlowCount = SlowCount;
				copy.TotalMs = TotalMs;
				copy.MaxMs = MaxMs;
				copy.LastMs = LastMs;
				copy.ScannedCount = ScannedCount;
				copy.DeletedCount = DeletedCount;
				copy.ModifiedCount = ModifiedCount;
				copy.CreatedCount = CreatedCount;
				copy.ErrorCount = ErrorCount;
				return copy;
			}
		}

		internal sealed class PhaseStat
		{
			public string Name;
			public long Count;
			public long TotalMs;
			public long MaxMs;
			public long CheckedCount;
			public long ChangedCount;
			public long DeletedCount;
			public long CreatedCount;
			public long ErrorCount;
			public bool CanChunkCandidate;
			public bool CanRegistryCandidate;
			public string Notes;

			public void Add(long elapsed)
			{
				++Count;
				TotalMs += elapsed;

				if (elapsed > MaxMs)
					MaxMs = elapsed;
			}

			public PhaseStat Clone()
			{
				PhaseStat copy = new PhaseStat();
				copy.Name = Name;
				copy.Count = Count;
				copy.TotalMs = TotalMs;
				copy.MaxMs = MaxMs;
				copy.CheckedCount = CheckedCount;
				copy.ChangedCount = ChangedCount;
				copy.DeletedCount = DeletedCount;
				copy.CreatedCount = CreatedCount;
				copy.ErrorCount = ErrorCount;
				copy.CanChunkCandidate = CanChunkCandidate;
				copy.CanRegistryCandidate = CanRegistryCandidate;
				copy.Notes = Notes;
				return copy;
			}
		}

		private sealed class TaskSample
		{
			public string TaskName;
			public string TimerName;
			public long RunId;
			public bool Startup;
			public DateTime StartedUtc;
			public long TotalMs;
			public int OnlineCount;
			public long ScannedCount;
			public long DeletedCount;
			public long ModifiedCount;
			public long CreatedCount;
			public long ErrorCount;
			public Dictionary<string, PhaseStat> Phases = new Dictionary<string, PhaseStat>();

			public TaskSample Clone()
			{
				TaskSample copy = new TaskSample();
				copy.TaskName = TaskName;
				copy.TimerName = TimerName;
				copy.RunId = RunId;
				copy.Startup = Startup;
				copy.StartedUtc = StartedUtc;
				copy.TotalMs = TotalMs;
				copy.OnlineCount = OnlineCount;
				copy.ScannedCount = ScannedCount;
				copy.DeletedCount = DeletedCount;
				copy.ModifiedCount = ModifiedCount;
				copy.CreatedCount = CreatedCount;
				copy.ErrorCount = ErrorCount;

				foreach (KeyValuePair<string, PhaseStat> kv in Phases)
					copy.Phases[kv.Key] = kv.Value.Clone();

				return copy;
			}
		}

		public static Context BeginRun(string taskName, string timerName, bool startup)
		{
			Context context = new Context();
			context.TaskName = taskName;
			context.TimerName = timerName;
			context.RunId = TaskManagerCorrelationContext.BeginTask(taskName, timerName, startup);
			context.Startup = startup;
			context.StartedUtc = DateTime.UtcNow;
			context.StartTimestamp = Stopwatch.GetTimestamp();
			context.OnlineCount = NetState.Instances.Count;
			return context;
		}

		public static long GetTimestamp()
		{
			return Stopwatch.GetTimestamp();
		}

		public static long GetElapsedMilliseconds(long start)
		{
			return (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
		}

		public static void StartPhase(Context context, string phase)
		{
			if (context == null)
				return;

			EndCurrentPhase(context);
			context.CurrentPhase = phase;
			context.PhaseStartTimestamp = Stopwatch.GetTimestamp();
			TaskManagerCorrelationContext.StartPhase(context.TaskName, phase);
		}

		public static void ResumeRun(Context context)
		{
			if (context == null)
				return;

			TaskManagerCorrelationContext.ResumeTask(
				context.RunId,
				context.TaskName,
				context.TimerName,
				context.Startup,
				context.StartedUtc);
		}

		public static void SuspendRun(Context context)
		{
			if (context == null)
				return;

			EndCurrentPhase(context);
			TaskManagerCorrelationContext.SuspendTask(context.TaskName);
		}

		public static void AddScanned(Context context, long count)
		{
			if (context != null && count > 0)
			{
				context.ScannedCount += count;
				GetOrCreateCurrentPhase(context).CheckedCount += count;
			}
		}

		public static void AddDeleted(Context context, long count)
		{
			if (context != null && count > 0)
			{
				context.DeletedCount += count;
				GetOrCreateCurrentPhase(context).DeletedCount += count;
			}
		}

		public static void AddModified(Context context, long count)
		{
			if (context != null && count > 0)
			{
				context.ModifiedCount += count;
				GetOrCreateCurrentPhase(context).ChangedCount += count;
			}
		}

		public static void AddCreated(Context context, long count)
		{
			if (context != null && count > 0)
			{
				context.CreatedCount += count;
				GetOrCreateCurrentPhase(context).CreatedCount += count;
			}
		}

		public static void MarkChunkCandidate(Context context, string notes)
		{
			if (context == null)
				return;

			PhaseStat phase = GetOrCreateCurrentPhase(context);
			phase.CanChunkCandidate = true;
			phase.Notes = AppendNote(phase.Notes, notes);
		}

		public static void MarkRegistryCandidate(Context context, string notes)
		{
			if (context == null)
				return;

			PhaseStat phase = GetOrCreateCurrentPhase(context);
			phase.CanRegistryCandidate = true;
			phase.Notes = AppendNote(phase.Notes, notes);
		}

		private static string AppendNote(string existing, string note)
		{
			if (String.IsNullOrWhiteSpace(note))
				return existing;

			if (String.IsNullOrWhiteSpace(existing))
				return note;

			return existing + "; " + note;
		}

		private static PhaseStat GetOrCreateCurrentPhase(Context context)
		{
			string phaseName = context.CurrentPhase != null ? context.CurrentPhase : "unphased";
			PhaseStat stat;

			if (!context.Phases.TryGetValue(phaseName, out stat))
			{
				stat = new PhaseStat();
				stat.Name = phaseName;
				context.Phases[phaseName] = stat;
			}

			return stat;
		}

		public static void EndRun(Context context)
		{
			if (context == null)
				return;

			EndCurrentPhase(context);

			TaskSample sample = new TaskSample();
			sample.TaskName = context.TaskName;
			sample.TimerName = context.TimerName;
			sample.RunId = context.RunId;
			sample.Startup = context.Startup;
			sample.StartedUtc = context.StartedUtc;
			sample.TotalMs = GetElapsedMilliseconds(context.StartTimestamp);
			sample.OnlineCount = context.OnlineCount;
			sample.ScannedCount = context.ScannedCount;
			sample.DeletedCount = context.DeletedCount;
			sample.ModifiedCount = context.ModifiedCount;
			sample.CreatedCount = context.CreatedCount;

			foreach (KeyValuePair<string, PhaseStat> kv in context.Phases)
				sample.Phases[kv.Key] = kv.Value.Clone();

			TaskManagerCorrelationContext.EndTask(context.TaskName);
			RecordSample(sample);
		}

		private static void EndCurrentPhase(Context context)
		{
			if (context.CurrentPhase == null)
				return;

			long elapsed = GetElapsedMilliseconds(context.PhaseStartTimestamp);
			PhaseStat stat;

			if (!context.Phases.TryGetValue(context.CurrentPhase, out stat))
			{
				stat = new PhaseStat();
				stat.Name = context.CurrentPhase;
				context.Phases[context.CurrentPhase] = stat;
			}

			stat.Add(elapsed);
			TaskManagerCorrelationContext.EndPhase(context.TaskName, context.CurrentPhase);
			context.CurrentPhase = null;
			context.PhaseStartTimestamp = 0;
		}

		private static void RecordSample(TaskSample sample)
		{
			bool rotate = false;
			bool logSummary = false;

			lock (m_Lock)
			{
				TaskStat stat;

				if (!m_Current.TryGetValue(sample.TimerName, out stat))
				{
					if (m_Current.Count >= MaxTaskStats)
						return;

					stat = new TaskStat();
					stat.Name = sample.TimerName;
					m_Current[sample.TimerName] = stat;
				}

				stat.Add(sample);
				m_LatestByTask[sample.TaskName] = sample.Clone();

				if (m_Worst == null || sample.TotalMs > m_Worst.TotalMs)
					m_Worst = sample.Clone();

				long now = Core.TickCount;

				if (m_NextSummary == 0)
					m_NextSummary = now + SummaryInterval;
				else if (now >= m_NextSummary)
				{
					m_NextSummary = now + SummaryInterval;
					rotate = true;
					logSummary = HasSlowTasks(m_Current);
				}

				if (rotate)
				{
					CopyStats(m_Current, m_Last);
					m_Current.Clear();
				}
			}

			if (sample.TotalMs >= SlowTaskThreshold)
				DiagnosticConsole.WriteParsedLine(String.Format("[Lag] TaskManager {0} took {1}ms startup={2} runId={3} topPhase={4}", sample.TimerName, sample.TotalMs, sample.Startup, sample.RunId, GetTopPhase(sample)));

			if (logSummary)
				LogSummary();
		}

		private static void CopyStats(Dictionary<string, TaskStat> from, Dictionary<string, TaskStat> to)
		{
			to.Clear();

			foreach (TaskStat stat in from.Values)
				to[stat.Name] = stat.Clone();
		}

		private static bool HasSlowTasks(Dictionary<string, TaskStat> stats)
		{
			foreach (TaskStat stat in stats.Values)
			{
				if (stat.SlowCount > 0)
					return true;
			}

			return false;
		}

		private static TaskStat GetTopStat(Dictionary<string, TaskStat> stats, string mode)
		{
			TaskStat best = null;

			foreach (TaskStat stat in stats.Values)
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
			}

			return best;
		}

		private static string FormatStat(TaskStat stat)
		{
			if (stat == null)
				return "none";

			long avg = stat.Count > 0 ? stat.TotalMs / stat.Count : 0;
			return String.Format("{0} total={1}ms count={2} startup={3} slow={4} max={5}ms avg={6}ms last={7}ms checked={8} deleted={9} changed={10} created={11} errors={12}", stat.Name, stat.TotalMs, stat.Count, stat.StartupCount, stat.SlowCount, stat.MaxMs, avg, stat.LastMs, stat.ScannedCount, stat.DeletedCount, stat.ModifiedCount, stat.CreatedCount, stat.ErrorCount);
		}

		private static string GetTopPhase(TaskSample sample)
		{
			PhaseStat top = null;

			foreach (PhaseStat phase in sample.Phases.Values)
			{
				if (top == null || phase.TotalMs > top.TotalMs)
					top = phase;
			}

			return top != null ? String.Format("{0}:{1}ms", top.Name, top.TotalMs) : "none";
		}

		private static void LogSummary()
		{
			TaskStat topTotal;
			TaskStat topMax;

			lock (m_Lock)
			{
				topTotal = GetTopStat(m_Last, "total");
				topMax = GetTopStat(m_Last, "max");
			}

			DiagnosticConsole.WriteParsedLine(String.Format("[Lag] TaskManagerStats 60s: topTotal={0}", FormatStat(topTotal)));
			DiagnosticConsole.WriteParsedLine(String.Format("[Lag] TaskManagerStats 60s: topMax={0}", FormatStat(topMax)));
		}

		public static void WriteStats(TextWriter writer)
		{
			if (writer == null)
				return;

			TaskStat[] current;
			TaskStat[] last;
			TaskSample[] latest;
			TaskSample worst;

			lock (m_Lock)
			{
				current = new TaskStat[m_Current.Count];
				m_Current.Values.CopyTo(current, 0);
				last = new TaskStat[m_Last.Count];
				m_Last.Values.CopyTo(last, 0);
				latest = new TaskSample[m_LatestByTask.Count];
				m_LatestByTask.Values.CopyTo(latest, 0);
				worst = m_Worst != null ? m_Worst.Clone() : null;
			}

			WriteWindow(writer, "current", current);
			WriteWindow(writer, "last", last);
			WriteLatest(writer, latest);
			WriteSample(writer, "worst", worst);
		}

		public static bool TryGetLatestRunInfo(string taskName, out long runId, out long totalMs, out string timerName, out DateTime startedUtc, out string topPhase)
		{
			runId = 0;
			totalMs = 0;
			timerName = null;
			startedUtc = DateTime.MinValue;
			topPhase = null;

			if (String.IsNullOrWhiteSpace(taskName))
				return false;

			lock (m_Lock)
			{
				TaskSample sample;

				if (!m_LatestByTask.TryGetValue(taskName, out sample) || sample == null)
					return false;

				runId = sample.RunId;
				totalMs = sample.TotalMs;
				timerName = sample.TimerName;
				startedUtc = sample.StartedUtc;
				topPhase = GetTopPhase(sample);
				return true;
			}
		}

		private static void WriteWindow(TextWriter writer, string label, TaskStat[] stats)
		{
			writer.WriteLine("TaskManagerStats {0}: entries={1} topTotal={2} topMax={3}",
				label,
				stats.Length,
				FormatStat(GetTopStat(ToDictionary(stats), "total")),
				FormatStat(GetTopStat(ToDictionary(stats), "max")));

			Array.Sort(stats, delegate(TaskStat a, TaskStat b)
			{
				if (a == null && b == null)
					return 0;

				if (a == null)
					return 1;

				if (b == null)
					return -1;

				return b.TotalMs.CompareTo(a.TotalMs);
			});

			int count = Math.Min(10, stats.Length);

			for (int i = 0; i < count; ++i)
				writer.WriteLine("  total#{0}: {1}", i + 1, FormatStat(stats[i]));
		}

		private static Dictionary<string, TaskStat> ToDictionary(TaskStat[] stats)
		{
			Dictionary<string, TaskStat> dict = new Dictionary<string, TaskStat>();

			for (int i = 0; i < stats.Length; ++i)
			{
				if (stats[i] != null)
					dict[stats[i].Name] = stats[i];
			}

			return dict;
		}

		private static void WriteLatest(TextWriter writer, TaskSample[] latest)
		{
			writer.WriteLine("TaskManager latest samples: count={0}", latest.Length);

			Array.Sort(latest, delegate(TaskSample a, TaskSample b)
			{
				if (a == null && b == null)
					return 0;

				if (a == null)
					return 1;

				if (b == null)
					return -1;

				return String.Compare(a.TaskName, b.TaskName, StringComparison.Ordinal);
			});

			for (int i = 0; i < latest.Length; ++i)
				WriteSample(writer, latest[i] != null ? latest[i].TaskName : "latest", latest[i]);
		}

		private static void WriteSample(TextWriter writer, string label, TaskSample sample)
		{
			if (sample == null)
			{
				writer.WriteLine("TaskManager sample {0}: unavailable", label);
				return;
			}

			writer.WriteLine("TaskManager sample {0}: timer={1} runId={2} total={3}ms startup={4} online={5} checked={6} deleted={7} changed={8} created={9} errors={10} topPhase={11} startedUtc={12}",
				label,
				sample.TimerName,
				sample.RunId,
				sample.TotalMs,
				sample.Startup,
				sample.OnlineCount,
				sample.ScannedCount,
				sample.DeletedCount,
				sample.ModifiedCount,
				sample.CreatedCount,
				sample.ErrorCount,
				GetTopPhase(sample),
				sample.StartedUtc);

			WritePhases(writer, sample);
		}

		private static void WritePhases(TextWriter writer, TaskSample sample)
		{
			PhaseStat[] phases = new PhaseStat[sample.Phases.Count];
			sample.Phases.Values.CopyTo(phases, 0);

			Array.Sort(phases, delegate(PhaseStat a, PhaseStat b)
			{
				if (a == null && b == null)
					return 0;

				if (a == null)
					return 1;

				if (b == null)
					return -1;

				return b.TotalMs.CompareTo(a.TotalMs);
			});

			for (int i = 0; i < phases.Length; ++i)
			{
				PhaseStat phase = phases[i];

				if (phase == null)
					continue;

				writer.WriteLine("  phase {0}: total={1}ms count={2} max={3}ms checked={4} changed={5} deleted={6} created={7} errors={8} chunkCandidate={9} registryCandidate={10} notes={11}",
					phase.Name,
					phase.TotalMs,
					phase.Count,
					phase.MaxMs,
					phase.CheckedCount,
					phase.ChangedCount,
					phase.DeletedCount,
					phase.CreatedCount,
					phase.ErrorCount,
					phase.CanChunkCandidate,
					phase.CanRegistryCandidate,
					!String.IsNullOrWhiteSpace(phase.Notes) ? phase.Notes : "none");
			}
		}
	}
}
