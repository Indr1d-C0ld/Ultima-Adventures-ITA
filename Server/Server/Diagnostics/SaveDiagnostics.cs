/***************************************************************************
 *                         SaveDiagnostics.cs
 *                         ------------------
 *   Lightweight attribution for world saves and AutoSave wrapper work.
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Server.Guilds;
using Server.Network;

namespace Server.Diagnostics
{
	public enum SaveDiagnosticMode
	{
		Aggregate,
		Deep
	}

	public static class SaveDiagnostics
	{
		private const int SaveSlowSerializeThresholdMs = 25;
		private const int SaveVerySlowSerializeThresholdMs = 50;
		private const int SaveTypeStatsMaxEntries = 128;
		private const string ConfigPath = "Config/SaveDiagnostics.cfg";

		private static readonly object m_Lock = new object();

		private static SaveDiagnosticMode m_ConfiguredMode = SaveDiagnosticMode.Deep;
		private static bool m_DeepNextSaveRequested;
		private static bool m_ConfigurationLoaded;
		private static string m_ConfigurationError;
		private static string m_LastConfigurationSaveError;
		private static bool m_Initialized;
		private static bool m_ConfigurationWarningReported;

		private static SaveSample m_Active;
		private static SaveSample m_Last;
		private static SaveSample m_LastAuto;
		private static SaveSample m_LastManual;
		private static SaveSample m_Worst;

		private static PendingAutoSave m_PendingAutoSave;

		private static long m_SaveCount;
		private static long m_AutoSaveCount;
		private static long m_ManualSaveCount;
		private static long m_TotalMs;
		private static long m_AutoTotalMs;
		private static long m_ManualTotalMs;

		public sealed class Snapshot
		{
			public SaveDiagnosticMode ConfiguredMode;
			public SaveDiagnosticMode EffectiveMode;
			public bool DeepNextSaveRequested;
			public bool SaveActive;
			public string ActiveMode;
			public bool ConfigurationLoaded;
			public string ConfigurationPath;
			public string ConfigurationError;
			public string ConfigurationSaveError;
			public long SaveCount;
			public bool LastAvailable;
			public SaveDiagnosticMode LastMode;
			public long LastTotalMs;
			public long LastStrategyMs;
			public long LastItemsMs;
			public long LastMobilesMs;
			public long LastGuildsMs;
			public long LastDeepProfiledEntities;
			public long LastAggregateOnlyEntities;
			public string LastError;
		}

		private sealed class PendingAutoSave
		{
			public bool AutoSave;
			public bool PermitBackgroundWrite;
			public long PreWorldWaitMs;
			public long BackupMs;
			public long GraveCleanupMs;
			public int GraveCleanupDeleted;
			public DateTime MarkedUtc;
		}

		private sealed class SaveSample
		{
			public DateTime StartedUtc;
			public DateTime EndedUtc;
			public SaveDiagnosticMode Mode;
			public bool AutoSave;
			public bool ManualSave;
			public bool Message;
			public bool PermitBackgroundWrite;
			public string Strategy;
			public string Error;
			public int OnlineCount;
			public int ItemCount;
			public int MobileCount;
			public int GuildCount;
			public int ItemsSaved;
			public int MobilesSaved;
			public int GuildsSaved;
			public long ItemBytes;
			public long MobileBytes;
			public long GuildBytes;
			public long TotalMs;
			public long WorldSaveMs;
			public long PreWorldWaitMs;
			public long BackupMs;
			public long GraveCleanupMs;
			public int GraveCleanupDeleted;
			public long FlushPauseMs;
			public long WorldWaitMs;
			public long DirectoryEnsureMs;
			public long BeforeWorldSaveMs;
			public long StrategyMs;
			public long ItemsMs;
			public long ItemsOpenMs;
			public long ItemsSerializeMs;
			public long ItemsTypeDbMs;
			public long ItemsCloseMs;
			public long MobilesMs;
			public long MobilesOpenMs;
			public long MobilesSerializeMs;
			public long MobilesTypeDbMs;
			public long MobilesCloseMs;
			public long GuildsMs;
			public long GuildsOpenMs;
			public long GuildsSerializeMs;
			public long GuildsCloseMs;
			public long WorldSaveEventMs;
			public long ProcessSafetyQueuesMs;
			public long ProcessDecayMs;
			public long AfterWorldSaveMs;
			public long NetResumeMs;
			public long OtherMs;
			public long DeepProfiledEntities;
			public long AggregateOnlyEntities;
			public bool DetailedTypeStatsCollected;
			public bool DetailedWorstSampleCollected;
			public TypeStat[] ItemTypeStats;
			public TypeStat[] MobileTypeStats;
			public TypeStat[] GuildTypeStats;
			public SerializeSample WorstSerialize;

			public SaveSample Clone()
			{
				SaveSample copy = (SaveSample)MemberwiseClone();
				copy.ItemTypeStats = CloneTypeStats(ItemTypeStats);
				copy.MobileTypeStats = CloneTypeStats(MobileTypeStats);
				copy.GuildTypeStats = CloneTypeStats(GuildTypeStats);
				copy.WorstSerialize = WorstSerialize != null ? WorstSerialize.Clone() : null;
				return copy;
			}
		}

		internal sealed class TypeStat
		{
			public string Kind;
			public string TypeName;
			public long Count;
			public long TotalMs;
			public long MaxMs;
			public long SlowCount;
			public long VerySlowCount;

			public void Add(long elapsedMs)
			{
				++Count;
				TotalMs += elapsedMs;

				if (elapsedMs > MaxMs)
					MaxMs = elapsedMs;

				if (elapsedMs >= SaveSlowSerializeThresholdMs)
					++SlowCount;

				if (elapsedMs >= SaveVerySlowSerializeThresholdMs)
					++VerySlowCount;
			}

			public TypeStat Clone()
			{
				TypeStat copy = new TypeStat();
				copy.Kind = Kind;
				copy.TypeName = TypeName;
				copy.Count = Count;
				copy.TotalMs = TotalMs;
				copy.MaxMs = MaxMs;
				copy.SlowCount = SlowCount;
				copy.VerySlowCount = VerySlowCount;
				return copy;
			}
		}

		internal sealed class SerializeSample
		{
			public string Kind;
			public string TypeName;
			public string Name;
			public Serial Serial;
			public long ElapsedMs;

			public SerializeSample Clone()
			{
				return (SerializeSample)MemberwiseClone();
			}
		}

		public sealed class TypeCollector
		{
			internal string m_Kind;
			private Dictionary<string, TypeStat> m_Stats = new Dictionary<string, TypeStat>();
			private SerializeSample m_Worst;

			public TypeCollector(string kind)
			{
				m_Kind = kind != null ? kind : "unknown";
			}

			public void Record(Type type, string name, Serial serial, long elapsedMs)
			{
				if (elapsedMs < 0)
					elapsedMs = 0;

				string typeName = type != null ? type.FullName : "unknown";
				TypeStat stat;

				if (!m_Stats.TryGetValue(typeName, out stat))
				{
					if (m_Stats.Count < SaveTypeStatsMaxEntries)
					{
						stat = new TypeStat();
						stat.Kind = m_Kind;
						stat.TypeName = typeName;
						m_Stats[typeName] = stat;
					}
				}

				if (stat != null)
					stat.Add(elapsedMs);

				if (m_Worst == null || elapsedMs > m_Worst.ElapsedMs)
				{
					m_Worst = new SerializeSample();
					m_Worst.Kind = m_Kind;
					m_Worst.TypeName = typeName;
					m_Worst.Name = name != null ? name : String.Empty;
					m_Worst.Serial = serial;
					m_Worst.ElapsedMs = elapsedMs;
				}
			}

			internal TypeStat[] GetStats()
			{
				TypeStat[] stats = new TypeStat[m_Stats.Count];
				int index = 0;

				foreach (TypeStat stat in m_Stats.Values)
					stats[index++] = stat.Clone();

				return stats;
			}

			internal SerializeSample GetWorst()
			{
				return m_Worst != null ? m_Worst.Clone() : null;
			}

		}

		static SaveDiagnostics()
		{
			LoadConfiguration();
		}

		public static void Initialize()
		{
			string warning = null;

			lock (m_Lock)
			{
				if (!m_Initialized)
					m_Initialized = true;

				if (!m_ConfigurationWarningReported && !String.IsNullOrEmpty(m_ConfigurationError))
				{
					warning = m_ConfigurationError;
					m_ConfigurationWarningReported = true;
				}
			}

			if (!String.IsNullOrEmpty(warning))
				DiagnosticConsole.WriteLine("[SaveDiagnostics]", DiagnosticSeverity.Warn, "Configuration warning: {0}", warning);
		}

		public static SaveDiagnosticMode ConfiguredMode
		{
			get { lock (m_Lock) { return m_ConfiguredMode; } }
		}

		public static SaveDiagnosticMode EffectiveMode
		{
			get
			{
				lock (m_Lock)
				{
					if (m_Active != null)
						return m_Active.Mode;

					return m_DeepNextSaveRequested ? SaveDiagnosticMode.Deep : m_ConfiguredMode;
				}
			}
		}

		public static bool DeepNextSaveRequested
		{
			get { lock (m_Lock) { return m_DeepNextSaveRequested; } }
		}

		public static bool IsDeepProfilingActive
		{
			get
			{
				lock (m_Lock)
					return m_Active != null && m_Active.Mode == SaveDiagnosticMode.Deep;
			}
		}

		public static string ConfigurationError
		{
			get
			{
				lock (m_Lock)
					return !String.IsNullOrEmpty(m_ConfigurationError) ? m_ConfigurationError : "none";
			}
		}

		public static string ConfigurationSaveError
		{
			get
			{
				lock (m_Lock)
					return !String.IsNullOrEmpty(m_LastConfigurationSaveError) ? m_LastConfigurationSaveError : "none";
			}
		}

		public static string ConfigurationPath
		{
			get
			{
				try { return Path.GetFullPath(ConfigPath); }
				catch { return ConfigPath; }
			}
		}

		public static Snapshot GetSnapshot()
		{
			Snapshot snapshot = new Snapshot();

			lock (m_Lock)
			{
				snapshot.ConfiguredMode = m_ConfiguredMode;
				snapshot.EffectiveMode = m_Active != null
					? m_Active.Mode
					: (m_DeepNextSaveRequested ? SaveDiagnosticMode.Deep : m_ConfiguredMode);
				snapshot.DeepNextSaveRequested = m_DeepNextSaveRequested;
				snapshot.SaveActive = m_Active != null;
				snapshot.ActiveMode = m_Active != null ? m_Active.Mode.ToString() : "none";
				snapshot.ConfigurationLoaded = m_ConfigurationLoaded;
				snapshot.ConfigurationError = !String.IsNullOrEmpty(m_ConfigurationError) ? m_ConfigurationError : "none";
				snapshot.ConfigurationSaveError = !String.IsNullOrEmpty(m_LastConfigurationSaveError) ? m_LastConfigurationSaveError : "none";
				snapshot.SaveCount = m_SaveCount;

				if (m_Last != null)
				{
					snapshot.LastAvailable = true;
					snapshot.LastMode = m_Last.Mode;
					snapshot.LastTotalMs = m_Last.TotalMs;
					snapshot.LastStrategyMs = m_Last.StrategyMs;
					snapshot.LastItemsMs = m_Last.ItemsMs;
					snapshot.LastMobilesMs = m_Last.MobilesMs;
					snapshot.LastGuildsMs = m_Last.GuildsMs;
					snapshot.LastDeepProfiledEntities = m_Last.DeepProfiledEntities;
					snapshot.LastAggregateOnlyEntities = m_Last.AggregateOnlyEntities;
					snapshot.LastError = !String.IsNullOrEmpty(m_Last.Error) ? m_Last.Error : "none";
				}
				else
				{
					snapshot.LastError = "none";
				}
			}

			snapshot.ConfigurationPath = ConfigurationPath;
			return snapshot;
		}

		public static bool SetConfiguredMode(SaveDiagnosticMode mode, out string message)
		{
			lock (m_Lock)
			{
				m_ConfiguredMode = mode;

				if (mode == SaveDiagnosticMode.Deep)
					m_DeepNextSaveRequested = false;
			}

			bool saved = SaveConfiguration();

			if (saved)
			{
				message = String.Format("Save diagnostics persistent mode set to {0}. Active saves are not interrupted.", mode);
				return true;
			}

			message = String.Format("Save diagnostics mode set to {0} in memory, but configuration write failed: {1}", mode, GetLastConfigurationSaveError());
			return false;
		}

		public static bool RequestDeepNextSave(out string message)
		{
			lock (m_Lock)
			{
				if (m_ConfiguredMode == SaveDiagnosticMode.Deep)
				{
					message = "Persistent mode is already Deep; one-shot Deep is unnecessary.";
					return false;
				}

				if (m_DeepNextSaveRequested)
				{
					message = "One-shot Deep profiling is already requested for the next save.";
					return false;
				}

				m_DeepNextSaveRequested = true;
				message = "One-shot Deep profiling requested for the next save. Persistent mode remains Aggregate.";
				return true;
			}
		}

		public static void Reset()
		{
			lock (m_Lock)
			{
				m_Last = null;
				m_LastAuto = null;
				m_LastManual = null;
				m_Worst = null;
				m_SaveCount = 0;
				m_AutoSaveCount = 0;
				m_ManualSaveCount = 0;
				m_TotalMs = 0;
				m_AutoTotalMs = 0;
				m_ManualTotalMs = 0;
			}
		}

		private static string GetLastConfigurationSaveError()
		{
			lock (m_Lock)
				return !String.IsNullOrEmpty(m_LastConfigurationSaveError) ? m_LastConfigurationSaveError : "unknown";
		}

		private static void LoadConfiguration()
		{
			SaveDiagnosticMode mode = SaveDiagnosticMode.Deep;
			bool loaded = false;
			string error = null;

			try
			{
				if (File.Exists(ConfigPath))
				{
					string[] lines = File.ReadAllLines(ConfigPath);

					for (int i = 0; i < lines.Length; ++i)
					{
						string line = lines[i] != null ? lines[i].Trim() : String.Empty;

						if (line.Length == 0 || line.StartsWith("#"))
							continue;

						int equals = line.IndexOf('=');

						if (equals < 0)
							continue;

						string key = line.Substring(0, equals).Trim();
						string value = line.Substring(equals + 1).Trim();

						if (String.Equals(key, "Mode", StringComparison.OrdinalIgnoreCase))
						{
							SaveDiagnosticMode parsed;

							if (TryParseMode(value, out parsed))
							{
								mode = parsed;
								loaded = true;
							}
							else
							{
								error = String.Format("Invalid SaveDiagnostics Mode '{0}', defaulting to Deep.", value);
								mode = SaveDiagnosticMode.Deep;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				error = ex.GetType().Name + ": " + ex.Message;
				mode = SaveDiagnosticMode.Deep;
			}

			m_ConfiguredMode = mode;
			m_ConfigurationLoaded = loaded;
			m_ConfigurationError = error;
		}

		private static bool SaveConfiguration()
		{
			SaveDiagnosticMode mode;

			lock (m_Lock)
				mode = m_ConfiguredMode;

			try
			{
				string directory = Path.GetDirectoryName(ConfigPath);

				if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				string tempPath = ConfigPath + ".tmp";

				using (StreamWriter writer = new StreamWriter(tempPath, false))
				{
					writer.WriteLine("# Save diagnostics configuration");
					writer.WriteLine("# Mode=Deep preserves per-entity profiling; Mode=Aggregate keeps low-overhead phase timing.");
					writer.WriteLine("Mode={0}", mode);
				}

				if (File.Exists(ConfigPath))
					File.Delete(ConfigPath);

				File.Move(tempPath, ConfigPath);

				lock (m_Lock)
				{
					m_ConfigurationLoaded = true;
					m_ConfigurationError = null;
					m_LastConfigurationSaveError = null;
				}

				return true;
			}
			catch (Exception ex)
			{
				lock (m_Lock)
					m_LastConfigurationSaveError = ex.GetType().Name + ": " + ex.Message;

				return false;
			}
		}

		private static bool TryParseMode(string value, out SaveDiagnosticMode mode)
		{
			if (String.Equals(value, "Aggregate", StringComparison.OrdinalIgnoreCase))
			{
				mode = SaveDiagnosticMode.Aggregate;
				return true;
			}

			if (String.Equals(value, "Deep", StringComparison.OrdinalIgnoreCase))
			{
				mode = SaveDiagnosticMode.Deep;
				return true;
			}

			mode = SaveDiagnosticMode.Deep;
			return false;
		}

		public static long GetTimestamp()
		{
			return Stopwatch.GetTimestamp();
		}

		public static long GetElapsedMilliseconds(long start)
		{
			return (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
		}

		public static void MarkNextAutoSave(bool permitBackgroundWrite, long preWorldWaitMs, long backupMs, long graveCleanupMs, int graveCleanupDeleted)
		{
			MarkNextSaveWrapper(true, permitBackgroundWrite, preWorldWaitMs, backupMs, graveCleanupMs, graveCleanupDeleted);
		}

		public static void MarkNextSaveWrapper(bool autoSave, bool permitBackgroundWrite, long preWorldWaitMs, long backupMs, long graveCleanupMs, int graveCleanupDeleted)
		{
			lock (m_Lock)
			{
				m_PendingAutoSave = new PendingAutoSave();
				m_PendingAutoSave.AutoSave = autoSave;
				m_PendingAutoSave.PermitBackgroundWrite = permitBackgroundWrite;
				m_PendingAutoSave.PreWorldWaitMs = preWorldWaitMs;
				m_PendingAutoSave.BackupMs = backupMs;
				m_PendingAutoSave.GraveCleanupMs = graveCleanupMs;
				m_PendingAutoSave.GraveCleanupDeleted = graveCleanupDeleted;
				m_PendingAutoSave.MarkedUtc = DateTime.UtcNow;
			}
		}

		public static void BeginSave(bool message, bool permitBackgroundWrite, string strategyName)
		{
			SaveSample sample = new SaveSample();
			PendingAutoSave pending = null;
			SaveDiagnosticMode mode;

			lock (m_Lock)
			{
				pending = m_PendingAutoSave;
				m_PendingAutoSave = null;
				mode = m_DeepNextSaveRequested ? SaveDiagnosticMode.Deep : m_ConfiguredMode;
				m_DeepNextSaveRequested = false;
			}

			sample.StartedUtc = DateTime.UtcNow;
			sample.Mode = mode;
			sample.Message = message;
			sample.PermitBackgroundWrite = permitBackgroundWrite;
			sample.Strategy = strategyName != null ? strategyName : "unknown";
			sample.OnlineCount = NetState.Instances.Count;
			sample.ItemCount = World.Items != null ? World.Items.Count : 0;
			sample.MobileCount = World.Mobiles != null ? World.Mobiles.Count : 0;
			sample.GuildCount = BaseGuild.List != null ? BaseGuild.List.Count : 0;

			if (pending != null)
			{
				sample.AutoSave = pending.AutoSave;
				sample.ManualSave = !pending.AutoSave;
				sample.PreWorldWaitMs = pending.PreWorldWaitMs;
				sample.BackupMs = pending.BackupMs;
				sample.GraveCleanupMs = pending.GraveCleanupMs;
				sample.GraveCleanupDeleted = pending.GraveCleanupDeleted;
			}
			else
			{
				sample.ManualSave = true;
			}

			lock (m_Lock)
				m_Active = sample;
		}

		public static void RecordPhase(string phase, long elapsedMs)
		{
			if (elapsedMs < 0)
				elapsedMs = 0;

			lock (m_Lock)
			{
				if (m_Active == null)
					return;

				AddPhase(m_Active, phase, elapsedMs);
			}
		}

		public static void RecordEntityBatch(string kind, int count, long bytes)
		{
			lock (m_Lock)
			{
				if (m_Active == null)
					return;

				if (kind == "items")
				{
					m_Active.ItemsSaved += count;
					m_Active.ItemBytes += bytes;
				}
				else if (kind == "mobiles")
				{
					m_Active.MobilesSaved += count;
					m_Active.MobileBytes += bytes;
				}
				else if (kind == "guilds")
				{
					m_Active.GuildsSaved += count;
					m_Active.GuildBytes += bytes;
				}
			}
		}

		public static void RecordEntityProfilingCounts(long deepProfiledEntities, long aggregateOnlyEntities)
		{
			if (deepProfiledEntities < 0)
				deepProfiledEntities = 0;

			if (aggregateOnlyEntities < 0)
				aggregateOnlyEntities = 0;

			lock (m_Lock)
			{
				if (m_Active == null)
					return;

				m_Active.DeepProfiledEntities += deepProfiledEntities;
				m_Active.AggregateOnlyEntities += aggregateOnlyEntities;
			}
		}

		public static void RecordTypeStats(TypeCollector collector)
		{
			if (collector == null)
				return;

			TypeStat[] stats = collector.GetStats();
			SerializeSample worst = collector.GetWorst();

			lock (m_Lock)
			{
				if (m_Active == null)
					return;

				if (collector.m_Kind == "items")
					m_Active.ItemTypeStats = stats;
				else if (collector.m_Kind == "mobiles")
					m_Active.MobileTypeStats = stats;
				else if (collector.m_Kind == "guilds")
					m_Active.GuildTypeStats = stats;

				if (stats != null && stats.Length > 0)
					m_Active.DetailedTypeStatsCollected = true;

				if (worst != null && (m_Active.WorstSerialize == null || worst.ElapsedMs > m_Active.WorstSerialize.ElapsedMs))
				{
					m_Active.WorstSerialize = worst.Clone();
					m_Active.DetailedWorstSampleCollected = true;
				}
			}
		}

		public static void RecordError(Exception ex)
		{
			lock (m_Lock)
			{
				if (m_Active != null)
					m_Active.Error = ex != null ? ex.GetType().FullName + ": " + ex.Message : "unknown";
			}
		}

		public static void EndSave(long worldSaveMs)
		{
			SaveSample sample;

			lock (m_Lock)
			{
				sample = m_Active;
				m_Active = null;
			}

			if (sample == null)
				return;

			sample.EndedUtc = DateTime.UtcNow;
			sample.WorldSaveMs = worldSaveMs;
			sample.TotalMs = worldSaveMs + sample.PreWorldWaitMs + sample.BackupMs + sample.GraveCleanupMs;
			sample.OtherMs = ComputeOtherMs(sample);

			lock (m_Lock)
			{
				++m_SaveCount;
				m_TotalMs += sample.TotalMs;
				m_Last = sample.Clone();

				if (sample.AutoSave)
				{
					++m_AutoSaveCount;
					m_AutoTotalMs += sample.TotalMs;
					m_LastAuto = sample.Clone();
				}
				else
				{
					++m_ManualSaveCount;
					m_ManualTotalMs += sample.TotalMs;
					m_LastManual = sample.Clone();
				}

				if (m_Worst == null || sample.TotalMs > m_Worst.TotalMs)
					m_Worst = sample.Clone();
			}
		}

		private static TypeStat[] CloneTypeStats(TypeStat[] stats)
		{
			if (stats == null)
				return null;

			TypeStat[] copy = new TypeStat[stats.Length];

			for (int i = 0; i < stats.Length; ++i)
				copy[i] = stats[i] != null ? stats[i].Clone() : null;

			return copy;
		}

		private static void AddPhase(SaveSample sample, string phase, long elapsedMs)
		{
			switch (phase)
			{
				case "flushPause": sample.FlushPauseMs += elapsedMs; break;
				case "worldWait": sample.WorldWaitMs += elapsedMs; break;
				case "directoryEnsure": sample.DirectoryEnsureMs += elapsedMs; break;
				case "beforeWorldSave": sample.BeforeWorldSaveMs += elapsedMs; break;
				case "strategy": sample.StrategyMs += elapsedMs; break;
				case "items": sample.ItemsMs += elapsedMs; break;
				case "itemsOpen": sample.ItemsOpenMs += elapsedMs; break;
				case "itemsSerialize": sample.ItemsSerializeMs += elapsedMs; break;
				case "itemsTypeDb": sample.ItemsTypeDbMs += elapsedMs; break;
				case "itemsClose": sample.ItemsCloseMs += elapsedMs; break;
				case "mobiles": sample.MobilesMs += elapsedMs; break;
				case "mobilesOpen": sample.MobilesOpenMs += elapsedMs; break;
				case "mobilesSerialize": sample.MobilesSerializeMs += elapsedMs; break;
				case "mobilesTypeDb": sample.MobilesTypeDbMs += elapsedMs; break;
				case "mobilesClose": sample.MobilesCloseMs += elapsedMs; break;
				case "guilds": sample.GuildsMs += elapsedMs; break;
				case "guildsOpen": sample.GuildsOpenMs += elapsedMs; break;
				case "guildsSerialize": sample.GuildsSerializeMs += elapsedMs; break;
				case "guildsClose": sample.GuildsCloseMs += elapsedMs; break;
				case "worldSaveEvent": sample.WorldSaveEventMs += elapsedMs; break;
				case "processSafetyQueues": sample.ProcessSafetyQueuesMs += elapsedMs; break;
				case "processDecay": sample.ProcessDecayMs += elapsedMs; break;
				case "afterWorldSave": sample.AfterWorldSaveMs += elapsedMs; break;
				case "netResume": sample.NetResumeMs += elapsedMs; break;
			}
		}

		private static long ComputeOtherMs(SaveSample sample)
		{
			long known =
				sample.PreWorldWaitMs +
				sample.BackupMs +
				sample.GraveCleanupMs +
				sample.FlushPauseMs +
				sample.WorldWaitMs +
				sample.DirectoryEnsureMs +
				sample.BeforeWorldSaveMs +
				sample.StrategyMs +
				sample.WorldSaveEventMs +
				sample.ProcessSafetyQueuesMs +
				sample.ProcessDecayMs +
				sample.AfterWorldSaveMs +
				sample.NetResumeMs;

			long other = sample.TotalMs - known;
			return other > 0 ? other : 0;
		}

		private static string GetTopPhase(SaveSample sample)
		{
			if (sample == null)
				return "none";

			string name = "none";
			long ms = 0;

			ConsiderPhase(ref name, ref ms, "backup", sample.BackupMs);
			ConsiderPhase(ref name, ref ms, "graveCleanup", sample.GraveCleanupMs);
			ConsiderPhase(ref name, ref ms, "flushPause", sample.FlushPauseMs);
			ConsiderPhase(ref name, ref ms, "worldWait", sample.WorldWaitMs);
			ConsiderPhase(ref name, ref ms, "directoryEnsure", sample.DirectoryEnsureMs);
			ConsiderPhase(ref name, ref ms, "beforeWorldSave", sample.BeforeWorldSaveMs);
			ConsiderPhase(ref name, ref ms, "strategy", sample.StrategyMs);
			ConsiderPhase(ref name, ref ms, "items", sample.ItemsMs);
			ConsiderPhase(ref name, ref ms, "mobiles", sample.MobilesMs);
			ConsiderPhase(ref name, ref ms, "guilds", sample.GuildsMs);
			ConsiderPhase(ref name, ref ms, "worldSaveEvent", sample.WorldSaveEventMs);
			ConsiderPhase(ref name, ref ms, "processSafetyQueues", sample.ProcessSafetyQueuesMs);
			ConsiderPhase(ref name, ref ms, "processDecay", sample.ProcessDecayMs);
			ConsiderPhase(ref name, ref ms, "afterWorldSave", sample.AfterWorldSaveMs);
			ConsiderPhase(ref name, ref ms, "netResume", sample.NetResumeMs);
			ConsiderPhase(ref name, ref ms, "other", sample.OtherMs);

			return String.Format("{0}:{1}ms", name, ms);
		}

		private static void ConsiderPhase(ref string name, ref long ms, string candidateName, long candidateMs)
		{
			if (candidateMs > ms)
			{
				name = candidateName;
				ms = candidateMs;
			}
		}

		public static void WriteStats(TextWriter writer)
		{
			if (writer == null)
				return;

			SaveSample active;
			SaveSample last;
			SaveSample lastAuto;
			SaveSample lastManual;
			SaveSample worst;
			long saves;
			long autoSaves;
			long manualSaves;
			long totalMs;
			long autoTotalMs;
			long manualTotalMs;
			SaveDiagnosticMode configuredMode;
			SaveDiagnosticMode effectiveMode;
			bool deepNextSaveRequested;
			bool configurationLoaded;
			string configurationError;
			string configurationSaveError;

			lock (m_Lock)
			{
				active = m_Active != null ? m_Active.Clone() : null;
				last = m_Last != null ? m_Last.Clone() : null;
				lastAuto = m_LastAuto != null ? m_LastAuto.Clone() : null;
				lastManual = m_LastManual != null ? m_LastManual.Clone() : null;
				worst = m_Worst != null ? m_Worst.Clone() : null;
				saves = m_SaveCount;
				autoSaves = m_AutoSaveCount;
				manualSaves = m_ManualSaveCount;
				totalMs = m_TotalMs;
				autoTotalMs = m_AutoTotalMs;
				manualTotalMs = m_ManualTotalMs;
				configuredMode = m_ConfiguredMode;
				effectiveMode = m_Active != null ? m_Active.Mode : (m_DeepNextSaveRequested ? SaveDiagnosticMode.Deep : m_ConfiguredMode);
				deepNextSaveRequested = m_DeepNextSaveRequested;
				configurationLoaded = m_ConfigurationLoaded;
				configurationError = m_ConfigurationError;
				configurationSaveError = m_LastConfigurationSaveError;
			}

			writer.WriteLine("SaveStats: saves={0}, autoSaves={1}, manualSaves={2}, avg={3}ms, avgAuto={4}ms, avgManual={5}ms",
				saves,
				autoSaves,
				manualSaves,
				saves > 0 ? totalMs / saves : 0,
				autoSaves > 0 ? autoTotalMs / autoSaves : 0,
				manualSaves > 0 ? manualTotalMs / manualSaves : 0);

			writer.WriteLine("SaveDiagnostics: configuredMode={0}, effectiveMode={1}, deepNextSaveRequested={2}, activeSaveMode={3}, configurationLoaded={4}, configurationPath={5}, configurationError={6}, configurationSaveError={7}",
				configuredMode,
				effectiveMode,
				deepNextSaveRequested,
				active != null ? active.Mode.ToString() : "none",
				configurationLoaded,
				ConfigurationPath,
				configurationError != null ? configurationError : "none",
				configurationSaveError != null ? configurationSaveError : "none");

			if (active != null)
				WriteSample(writer, "active", active);

			WriteSample(writer, "last", last);
			WriteSample(writer, "lastAuto", lastAuto);
			WriteSample(writer, "lastManual", lastManual);
			WriteSample(writer, "worst", worst);
		}

		private static void WriteSample(TextWriter writer, string label, SaveSample sample)
		{
			if (sample == null)
			{
				writer.WriteLine("SaveStats {0}: unavailable", label);
				return;
			}

			writer.WriteLine("SaveStats {0}: total={1}ms world={2}ms auto={3} manual={4} mode={5} strategy={6} permitBackgroundWrite={7} online={8} items={9}/{10} mobiles={11}/{12} guilds={13}/{14} bytes(item/mobile/guild)={15}/{16}/{17} topPhase={18} startedUtc={19} completedUtc={20} error={21}",
				label,
				sample.TotalMs,
				sample.WorldSaveMs,
				sample.AutoSave,
				sample.ManualSave,
				sample.Mode,
				sample.Strategy,
				sample.PermitBackgroundWrite,
				sample.OnlineCount,
				sample.ItemsSaved,
				sample.ItemCount,
				sample.MobilesSaved,
				sample.MobileCount,
				sample.GuildsSaved,
				sample.GuildCount,
				sample.ItemBytes,
				sample.MobileBytes,
				sample.GuildBytes,
				GetTopPhase(sample),
				sample.StartedUtc,
				sample.EndedUtc,
				sample.Error != null ? sample.Error : "none");

			writer.WriteLine("  profiling: activeMode={0} deepProfiledEntities={1} aggregateOnlyEntities={2} detailedTypeStatsCollected={3} detailedWorstSampleCollected={4}",
				sample.Mode,
				sample.DeepProfiledEntities,
				sample.AggregateOnlyEntities,
				sample.DetailedTypeStatsCollected,
				sample.DetailedWorstSampleCollected);

			if (sample.Mode == SaveDiagnosticMode.Aggregate)
				writer.WriteLine("  Per-entity deep profiling disabled for this save.");
			else
				writer.WriteLine("  Per-entity deep profiling was active for this save.");

			writer.WriteLine("  wrapper: preWorldWait={0}ms backup={1}ms graveCleanup={2}ms graveDeleted={3}",
				sample.PreWorldWaitMs,
				sample.BackupMs,
				sample.GraveCleanupMs,
				sample.GraveCleanupDeleted);

			writer.WriteLine("  world: flushPause={0}ms worldWait={1}ms directoryEnsure={2}ms beforeWorldSave={3}ms strategy={4}ms worldSaveEvent={5}ms processSafetyQueues={6}ms processDecay={7}ms afterWorldSave={8}ms netResume={9}ms other={10}ms",
				sample.FlushPauseMs,
				sample.WorldWaitMs,
				sample.DirectoryEnsureMs,
				sample.BeforeWorldSaveMs,
				sample.StrategyMs,
				sample.WorldSaveEventMs,
				sample.ProcessSafetyQueuesMs,
				sample.ProcessDecayMs,
				sample.AfterWorldSaveMs,
				sample.NetResumeMs,
				sample.OtherMs);

			writer.WriteLine("  strategyDetail: items={0}ms(open={1}, serialize={2}, typeDb={3}, close={4}) mobiles={5}ms(open={6}, serialize={7}, typeDb={8}, close={9}) guilds={10}ms(open={11}, serialize={12}, close={13})",
				sample.ItemsMs,
				sample.ItemsOpenMs,
				sample.ItemsSerializeMs,
				sample.ItemsTypeDbMs,
				sample.ItemsCloseMs,
				sample.MobilesMs,
				sample.MobilesOpenMs,
				sample.MobilesSerializeMs,
				sample.MobilesTypeDbMs,
				sample.MobilesCloseMs,
				sample.GuildsMs,
				sample.GuildsOpenMs,
				sample.GuildsSerializeMs,
				sample.GuildsCloseMs);

			if (sample.Mode == SaveDiagnosticMode.Aggregate)
				writer.WriteLine("  worstSerialize: Not collected: SaveDiagnosticMode=Aggregate");
			else if (sample.WorstSerialize != null)
				writer.WriteLine("  worstSerialize: kind={0} type={1} serial={2} name=\"{3}\" elapsed={4}ms", sample.WorstSerialize.Kind, sample.WorstSerialize.TypeName, sample.WorstSerialize.Serial, sample.WorstSerialize.Name, sample.WorstSerialize.ElapsedMs);
			else
				writer.WriteLine("  worstSerialize: unavailable");

			WriteTypeStats(writer, "itemTypes", sample.ItemTypeStats, sample.Mode);
			WriteTypeStats(writer, "mobileTypes", sample.MobileTypeStats, sample.Mode);
			WriteTypeStats(writer, "guildTypes", sample.GuildTypeStats, sample.Mode);
		}

		private static void WriteTypeStats(TextWriter writer, string label, TypeStat[] stats, SaveDiagnosticMode mode)
		{
			if (mode == SaveDiagnosticMode.Aggregate)
			{
				writer.WriteLine("  {0}: Not collected: SaveDiagnosticMode=Aggregate", label);
				return;
			}

			if (stats == null || stats.Length == 0)
			{
				writer.WriteLine("  {0}: unavailable", label);
				return;
			}

			TypeStat[] byTotal = CloneTypeStats(stats);
			TypeStat[] byMax = CloneTypeStats(stats);
			TypeStat[] byCount = CloneTypeStats(stats);

			Array.Sort(byTotal, delegate(TypeStat a, TypeStat b)
			{
				if (a == null && b == null)
					return 0;
				if (a == null)
					return 1;
				if (b == null)
					return -1;
				return b.TotalMs.CompareTo(a.TotalMs);
			});

			Array.Sort(byMax, delegate(TypeStat a, TypeStat b)
			{
				if (a == null && b == null)
					return 0;
				if (a == null)
					return 1;
				if (b == null)
					return -1;
				return b.MaxMs.CompareTo(a.MaxMs);
			});

			Array.Sort(byCount, delegate(TypeStat a, TypeStat b)
			{
				if (a == null && b == null)
					return 0;
				if (a == null)
					return 1;
				if (b == null)
					return -1;
				return b.Count.CompareTo(a.Count);
			});

			writer.WriteLine("  {0}: tracked={1} topTotal={2} topMax={3} topCount={4}",
				label,
				stats.Length,
				FormatTypeStat(GetFirstTypeStat(byTotal)),
				FormatTypeStat(GetFirstTypeStat(byMax)),
				FormatTypeStat(GetFirstTypeStat(byCount)));

			int count = Math.Min(5, byTotal.Length);
			for (int i = 0; i < count; ++i)
			{
				if (byTotal[i] != null)
					writer.WriteLine("    total#{0}: {1}", i + 1, FormatTypeStat(byTotal[i]));
			}
		}

		private static TypeStat GetFirstTypeStat(TypeStat[] stats)
		{
			if (stats == null)
				return null;

			for (int i = 0; i < stats.Length; ++i)
			{
				if (stats[i] != null)
					return stats[i];
			}

			return null;
		}

		private static string FormatTypeStat(TypeStat stat)
		{
			if (stat == null)
				return "none";

			long avg = stat.Count > 0 ? stat.TotalMs / stat.Count : 0;
			return String.Format("{0} count={1} total={2}ms max={3}ms avg={4}ms slow25={5} slow50={6}", stat.TypeName, stat.Count, stat.TotalMs, stat.MaxMs, avg, stat.SlowCount, stat.VerySlowCount);
		}
	}
}
