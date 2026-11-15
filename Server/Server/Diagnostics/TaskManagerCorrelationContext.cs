using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Server.Diagnostics
{
	public static class TaskManagerCorrelationContext
	{
		private const long RecentWindowMs = 60000;

		private static readonly object m_Lock = new object();
		private static readonly Dictionary<string, RecentContext> m_RecentEnds = new Dictionary<string, RecentContext>();
		private static long m_NextRunId;

		[ThreadStatic]
		private static long t_RunId;

		[ThreadStatic]
		private static string t_TaskName;

		[ThreadStatic]
		private static string t_TimerName;

		[ThreadStatic]
		private static string t_PhaseName;

		[ThreadStatic]
		private static bool t_Startup;

		[ThreadStatic]
		private static DateTime t_StartUtc;

		[ThreadStatic]
		private static string t_TargetType;

		[ThreadStatic]
		private static string t_TargetAction;

		[ThreadStatic]
		private static string t_TargetSpawnerKind;

		[ThreadStatic]
		private static string t_TargetEntryKey;

		[ThreadStatic]
		private static string t_TargetRegionKey;

		[ThreadStatic]
		private static string t_TargetDefinitionKey;

		public sealed class DirectContext
		{
			public long RunId;
			public string TaskName;
			public string TimerName;
			public string PhaseName;
			public bool Startup;
			public DateTime StartUtc;
			public string TargetType;
			public string TargetAction;
			public string TargetSpawnerKind;
			public string TargetEntryKey;
			public string TargetRegionKey;
			public string TargetDefinitionKey;

			public bool Active
			{
				get { return RunId > 0 && !String.IsNullOrWhiteSpace(TaskName); }
			}

			public string DirectOrigin
			{
				get
				{
					if (!Active)
						return "none";

					string phase = !String.IsNullOrWhiteSpace(PhaseName) ? "/" + PhaseName : String.Empty;
					return String.Format("{0}{1}/runId={2}", TaskName, phase, RunId);
				}
			}

			public string DirectOriginWithTimer
			{
				get
				{
					if (!Active)
						return "none";

					string timer = !String.IsNullOrWhiteSpace(TimerName) ? "/" + TimerName : String.Empty;
					string phase = !String.IsNullOrWhiteSpace(PhaseName) ? "/" + PhaseName : String.Empty;
					return String.Format("{0}{1}{2}/runId={3}", TaskName, timer, phase, RunId);
				}
			}

			public string TargetDescription
			{
				get
				{
					if (String.IsNullOrWhiteSpace(TargetType))
						return "none";

					return String.Format("{0} action={1} spawner={2} entry={3} region={4} definition={5}",
						TargetType,
						CleanForOutput(TargetAction),
						CleanForOutput(TargetSpawnerKind),
						CleanForOutput(TargetEntryKey),
						CleanForOutput(TargetRegionKey),
						CleanForOutput(TargetDefinitionKey));
				}
			}

			public DirectContext Clone()
			{
				return (DirectContext)MemberwiseClone();
			}
		}

		public sealed class RecentContext
		{
			public long RunId;
			public string TaskName;
			public string TimerName;
			public string PhaseName;
			public bool Startup;
			public long EndTimestamp;
			public DateTime EndUtc;

			public string Key
			{
				get
				{
					string phase = !String.IsNullOrWhiteSpace(PhaseName) ? ":" + PhaseName : String.Empty;
					return String.Format("{0}{1}/runId={2}", CleanForOutput(TaskName), phase, RunId);
				}
			}

			public RecentContext Clone()
			{
				return (RecentContext)MemberwiseClone();
			}
		}

		public static long BeginTask(string taskName, string timerName, bool startup)
		{
			long runId = Interlocked.Increment(ref m_NextRunId);

			ResumeTask(runId, taskName, timerName, startup, DateTime.UtcNow);

			return runId;
		}

		public static void ResumeTask(long runId, string taskName, string timerName, bool startup, DateTime startUtc)
		{
			t_RunId = runId;
			t_TaskName = Clean(taskName);
			t_TimerName = Clean(timerName);
			t_PhaseName = null;
			t_Startup = startup;
			t_StartUtc = startUtc;
			ClearTarget();
		}

		public static void SuspendTask(string taskName)
		{
			string task = Clean(taskName);

			if (!String.Equals(t_TaskName, task, StringComparison.Ordinal))
				return;

			t_RunId = 0;
			t_TaskName = null;
			t_TimerName = null;
			t_PhaseName = null;
			t_Startup = false;
			t_StartUtc = DateTime.MinValue;
			ClearTarget();
		}

		public static void StartPhase(string taskName, string phaseName)
		{
			if (String.IsNullOrWhiteSpace(t_TaskName))
				t_TaskName = Clean(taskName);

			t_PhaseName = Clean(phaseName);
			ClearTarget();
		}

		public static void EndPhase(string taskName, string phaseName)
		{
			string task = Clean(taskName);
			string phase = Clean(phaseName);

			if (!String.IsNullOrWhiteSpace(task) && !String.IsNullOrWhiteSpace(phase))
				RecordRecent(task, phase);

			if (String.Equals(t_TaskName, task, StringComparison.Ordinal) && String.Equals(t_PhaseName, phase, StringComparison.Ordinal))
			{
				t_PhaseName = null;
				ClearTarget();
			}
		}

		public static void EndTask(string taskName)
		{
			string task = Clean(taskName);

			if (!String.IsNullOrWhiteSpace(task))
				RecordRecent(task, null);

			if (String.Equals(t_TaskName, task, StringComparison.Ordinal))
			{
				t_RunId = 0;
				t_TaskName = null;
				t_TimerName = null;
				t_PhaseName = null;
				t_Startup = false;
				t_StartUtc = DateTime.MinValue;
				ClearTarget();
			}
		}

		public static void BeginTarget(string targetType, string action, string spawnerKind, string entryKey, string regionKey, string definitionKey)
		{
			t_TargetType = Clean(targetType);
			t_TargetAction = Clean(action);
			t_TargetSpawnerKind = Clean(spawnerKind);
			t_TargetEntryKey = Clean(entryKey);
			t_TargetRegionKey = Clean(regionKey);
			t_TargetDefinitionKey = Clean(definitionKey);
		}

		public static void EndTarget()
		{
			ClearTarget();
		}

		public static DirectContext GetDirectContextSnapshot()
		{
			DirectContext context = new DirectContext();
			context.RunId = t_RunId;
			context.TaskName = t_TaskName;
			context.TimerName = t_TimerName;
			context.PhaseName = t_PhaseName;
			context.Startup = t_Startup;
			context.StartUtc = t_StartUtc;
			context.TargetType = t_TargetType;
			context.TargetAction = t_TargetAction;
			context.TargetSpawnerKind = t_TargetSpawnerKind;
			context.TargetEntryKey = t_TargetEntryKey;
			context.TargetRegionKey = t_TargetRegionKey;
			context.TargetDefinitionKey = t_TargetDefinitionKey;
			return context;
		}

		public static string GetDirectOriginLabel()
		{
			return GetDirectContextSnapshot().DirectOrigin;
		}

		public static string[] GetRecentCorrelationLabels()
		{
			List<string> labels = new List<string>();
			long now = Stopwatch.GetTimestamp();
			List<string> expired = null;

			lock (m_Lock)
			{
				foreach (KeyValuePair<string, RecentContext> kv in m_RecentEnds)
				{
					long elapsed = GetElapsedMilliseconds(now, kv.Value.EndTimestamp);

					if (elapsed > RecentWindowMs)
					{
						if (expired == null)
							expired = new List<string>();

						expired.Add(kv.Key);
						continue;
					}

					labels.Add("after " + kv.Value.Key + " +" + GetWindowLabel(elapsed));
				}

				if (expired != null)
				{
					for (int i = 0; i < expired.Count; ++i)
						m_RecentEnds.Remove(expired[i]);
				}
			}

			if (labels.Count == 0)
				labels.Add("none");

			return labels.ToArray();
		}

		public static string[] GetCorrelationLabels()
		{
			List<string> labels = new List<string>();
			DirectContext direct = GetDirectContextSnapshot();

			if (direct.Active)
			{
				labels.Add("direct " + direct.TaskName + "/runId=" + direct.RunId.ToString());

				if (!String.IsNullOrWhiteSpace(direct.PhaseName))
					labels.Add("direct " + direct.TaskName + ":" + direct.PhaseName + "/runId=" + direct.RunId.ToString());
			}

			string[] recent = GetRecentCorrelationLabels();

			for (int i = 0; i < recent.Length; ++i)
				labels.Add("recent " + recent[i]);

			if (labels.Count == 0)
				labels.Add("none");

			return labels.ToArray();
		}

		public static string GetCurrentContext()
		{
			DirectContext direct = GetDirectContextSnapshot();

			if (direct.Active)
				return direct.DirectOrigin;

			return "none";
		}

		private static void RecordRecent(string task, string phase)
		{
			long runId = t_RunId;

			if (runId <= 0)
				return;

			RecentContext context = new RecentContext();
			context.RunId = runId;
			context.TaskName = Clean(task);
			context.TimerName = t_TimerName;
			context.PhaseName = Clean(phase);
			context.Startup = t_Startup;
			context.EndTimestamp = Stopwatch.GetTimestamp();
			context.EndUtc = DateTime.UtcNow;

			lock (m_Lock)
				m_RecentEnds[context.Key] = context;
		}

		private static void ClearTarget()
		{
			t_TargetType = null;
			t_TargetAction = null;
			t_TargetSpawnerKind = null;
			t_TargetEntryKey = null;
			t_TargetRegionKey = null;
			t_TargetDefinitionKey = null;
		}

		private static string Clean(string value)
		{
			return value != null ? value.Trim() : String.Empty;
		}

		private static string CleanForOutput(string value)
		{
			if (String.IsNullOrWhiteSpace(value))
				return "none";

			return value.Trim();
		}

		private static long GetElapsedMilliseconds(long now, long start)
		{
			return (now - start) * 1000 / Stopwatch.Frequency;
		}

		private static string GetWindowLabel(long elapsedMs)
		{
			if (elapsedMs <= 1000)
				return "1s";

			if (elapsedMs <= 5000)
				return "5s";

			if (elapsedMs <= 15000)
				return "15s";

			if (elapsedMs <= 30000)
				return "30s";

			return "60s";
		}
	}
}
