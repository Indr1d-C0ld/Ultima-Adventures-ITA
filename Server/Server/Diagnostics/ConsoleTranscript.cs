using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Server.Diagnostics
{
	public static class ConsoleTranscript
	{
		private const bool DefaultEnabled = true;
		private const string OutputDirectory = "Logs/Console";
		private const long MaxPartBytes = 64L * 1024L * 1024L;
		private const int RetentionDays = 14;
		private const int MaxRetainedFiles = 100;
		private const int FlushIntervalMs = 1500;
		private const int MaxQueuedChunks = 16384;
		private const long MaxQueuedBytes = 12L * 1024L * 1024L;

		private static readonly object m_Lock = new object();
		private static readonly Queue<string> m_Queue = new Queue<string>();
		private static readonly AutoResetEvent m_Signal = new AutoResetEvent(false);
		private static readonly Encoding m_Utf8 = new UTF8Encoding(false);
		private static readonly List<Action<string>> m_Observers = new List<Action<string>>();

		private static bool m_Enabled = DefaultEnabled;
		private static bool m_Initialized;
		private static bool m_Accepting;
		private static bool m_StopRequested;
		private static bool m_FlushRequested;
		private static bool m_RotateRequested;
		private static Thread m_WriterThread;
		private static TextWriter m_OriginalOut;
		private static TextWriter m_OriginalError;
		private static StreamWriter m_Writer;
		private static string m_BootStamp;
		private static string m_CurrentFile;
		private static int m_CurrentPart = 1;
		private static long m_CurrentPartBytes;
		private static DateTime m_StartedUtc;
		private static DateTime m_LastFlushUtc;
		private static DateTime m_LastDropWarningUtc;
		private static string m_LastError;

		private static long m_EnqueueCalls;
		private static long m_EnqueuedChunks;
		private static long m_EnqueuedBytes;
		private static int m_QueuedChunks;
		private static long m_QueuedBytes;
		private static int m_MaxQueuedChunks;
		private static long m_MaxQueuedBytes;
		private static long m_DroppedChunks;
		private static long m_DroppedBytes;
		private static long m_BytesWritten;
		private static long m_WriteCalls;
		private static long m_TotalWriteMs;
		private static long m_MaxWriteMs;
		private static long m_FlushCount;
		private static long m_TotalFlushMs;
		private static long m_MaxFlushMs;
		private static long m_Rotations;
		private static long m_WriteErrors;

		public static bool Initialized { get { lock (m_Lock) { return m_Initialized; } } }

		public sealed class Snapshot
		{
			public bool Enabled;
			public bool Initialized;
			public bool WriterAlive;
			public string CurrentFile;
			public int CurrentPart;
			public long BytesWritten;
			public int QueuedChunks;
			public long QueuedBytes;
			public int MaxQueuedChunks;
			public long MaxQueuedBytes;
			public long DroppedChunks;
			public long DroppedBytes;
			public long WriteErrors;
			public string LastError;
			public DateTime StartedUtc;
			public DateTime LastFlushUtc;
			public long FlushCount;
			public long Rotations;
		}

		public static void Initialize()
		{
			lock (m_Lock)
			{
				if (m_Initialized)
					return;

				m_Initialized = true;
				m_Enabled = DefaultEnabled;

				if (!m_Enabled)
					return;

				try
				{
					Directory.CreateDirectory(OutputDirectory);
					CleanupRetention();

					m_OriginalOut = Console.Out;
					m_OriginalError = Console.Error;
					m_StartedUtc = DateTime.UtcNow;
					m_BootStamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
					OpenWriterLocked(1);

					m_Accepting = true;
					m_WriterThread = new Thread(WriterLoop);
					m_WriterThread.Name = "Console Transcript";
					m_WriterThread.IsBackground = true;
					m_WriterThread.Start();

					Console.SetOut(new TeeTextWriter(m_OriginalOut));
					Console.SetError(new TeeTextWriter(m_OriginalError));
				}
				catch (Exception ex)
				{
					m_LastError = ex.GetType().Name + ": " + ex.Message;
					m_WriteErrors++;
					m_Accepting = false;

					WriteInternalWarning("Console transcript initialization failed: " + m_LastError);
				}
			}
		}

		public static bool RegisterObserver(Action<string> observer)
		{
			if (observer == null)
				return false;

			lock (m_Lock)
			{
				if (!m_Initialized || !m_Enabled)
					return false;

				for (int i = 0; i < m_Observers.Count; ++i)
				{
					if (m_Observers[i] == observer)
						return true;
				}

				m_Observers.Add(observer);
				return true;
			}
		}

		public static Snapshot GetSnapshot()
		{
			Snapshot snapshot = new Snapshot();

			lock (m_Lock)
			{
				snapshot.Enabled = m_Enabled;
				snapshot.Initialized = m_Initialized;
				snapshot.WriterAlive = m_WriterThread != null && m_WriterThread.IsAlive;
				snapshot.CurrentFile = !String.IsNullOrEmpty(m_CurrentFile) ? Path.GetFullPath(m_CurrentFile) : "none";
				snapshot.CurrentPart = m_CurrentPart;
				snapshot.BytesWritten = m_BytesWritten;
				snapshot.QueuedChunks = m_QueuedChunks;
				snapshot.QueuedBytes = m_QueuedBytes;
				snapshot.MaxQueuedChunks = m_MaxQueuedChunks;
				snapshot.MaxQueuedBytes = m_MaxQueuedBytes;
				snapshot.DroppedChunks = m_DroppedChunks;
				snapshot.DroppedBytes = m_DroppedBytes;
				snapshot.WriteErrors = m_WriteErrors;
				snapshot.LastError = !String.IsNullOrEmpty(m_LastError) ? m_LastError : "none";
				snapshot.StartedUtc = m_StartedUtc;
				snapshot.LastFlushUtc = m_LastFlushUtc;
				snapshot.FlushCount = m_FlushCount;
				snapshot.Rotations = m_Rotations;
			}

			return snapshot;
		}

		public static void RequestFlush()
		{
			lock (m_Lock)
				m_FlushRequested = true;

			m_Signal.Set();
		}

		public static bool FlushNow(int timeoutMs)
		{
			DateTime start = DateTime.UtcNow;
			RequestFlush();

			while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
			{
				lock (m_Lock)
				{
					if (m_QueuedChunks == 0 && !m_FlushRequested)
						return true;
				}

				Thread.Sleep(25);
			}

			return false;
		}

		public static bool RotateNow(int timeoutMs)
		{
			long rotations;

			lock (m_Lock)
			{
				rotations = m_Rotations;
				m_RotateRequested = true;
			}

			m_Signal.Set();

			DateTime start = DateTime.UtcNow;

			while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
			{
				lock (m_Lock)
				{
					if (m_Rotations > rotations && !m_RotateRequested)
						return true;
				}

				Thread.Sleep(25);
			}

			return false;
		}

		public static void Shutdown()
		{
			Thread thread;

			lock (m_Lock)
			{
				if (!m_Initialized || !m_Enabled || m_StopRequested)
					return;

				EnqueueLocked(String.Format("[ConsoleTranscript] End UTC={0:u} local={1}", DateTime.UtcNow, DateTime.Now), true);
				m_Accepting = false;
				m_StopRequested = true;
				m_FlushRequested = true;
				thread = m_WriterThread;
			}

			m_Signal.Set();

			if (thread != null)
				thread.Join(3000);

			CloseWriter();
		}

		public static void EmergencyFlush(int timeoutMs)
		{
			FlushNow(timeoutMs);
		}

		internal static void Enqueue(string text)
		{
			if (String.IsNullOrEmpty(text))
				return;

			Action<string>[] observers = null;

			lock (m_Lock)
			{
				if (m_Observers.Count > 0)
				{
					observers = new Action<string>[m_Observers.Count];
					m_Observers.CopyTo(observers);
				}

				if (!m_Accepting || !m_Enabled)
					goto notify;

				EnqueueLocked(text, false);
			}

		notify:
			if (observers != null)
			{
				for (int i = 0; i < observers.Length; ++i)
				{
					try { observers[i](text); }
					catch { }
				}
			}
		}

		private static void EnqueueLocked(string text, bool force)
		{
			long bytes = m_Utf8.GetByteCount(text);
			m_EnqueueCalls++;

			if (!force && (m_Queue.Count >= MaxQueuedChunks || m_QueuedBytes + bytes > MaxQueuedBytes))
			{
				m_DroppedChunks++;
				m_DroppedBytes += bytes;
				WarnDropLocked();
				return;
			}

			m_Queue.Enqueue(text);
			m_QueuedChunks = m_Queue.Count;
			m_QueuedBytes += bytes;
			m_EnqueuedChunks++;
			m_EnqueuedBytes += bytes;

			if (m_QueuedChunks > m_MaxQueuedChunks)
				m_MaxQueuedChunks = m_QueuedChunks;

			if (m_QueuedBytes > m_MaxQueuedBytes)
				m_MaxQueuedBytes = m_QueuedBytes;

			m_Signal.Set();
		}

		private static void WarnDropLocked()
		{
			if ((DateTime.UtcNow - m_LastDropWarningUtc).TotalSeconds < 30.0)
				return;

			m_LastDropWarningUtc = DateTime.UtcNow;
			WriteInternalWarning(String.Format("Console transcript queue full; droppedChunks={0} droppedBytes={1}", m_DroppedChunks, m_DroppedBytes));
		}

		private static void WriterLoop()
		{
			DateTime lastFlush = DateTime.UtcNow;

			while (true)
			{
				m_Signal.WaitOne(FlushIntervalMs);
				DrainQueue();

				bool rotate;
				bool flush;
				bool stop;

				lock (m_Lock)
				{
					rotate = m_RotateRequested;
					flush = m_FlushRequested;
					stop = m_StopRequested;
				}

				if (rotate)
					RotateWriter();

				if (flush || (DateTime.UtcNow - lastFlush).TotalMilliseconds >= FlushIntervalMs)
				{
					FlushWriter();
					lastFlush = DateTime.UtcNow;
				}

				if (stop)
				{
					DrainQueue();
					FlushWriter();
					return;
				}
			}
		}

		private static void DrainQueue()
		{
			while (true)
			{
				string text;
				long bytes;

				lock (m_Lock)
				{
					if (m_Queue.Count == 0)
						return;

					text = m_Queue.Dequeue();
					bytes = m_Utf8.GetByteCount(text);
					m_QueuedChunks = m_Queue.Count;
					m_QueuedBytes -= bytes;

					if (m_QueuedBytes < 0)
						m_QueuedBytes = 0;
				}

				WriteChunk(text, bytes);
			}
		}

		private static void WriteChunk(string text, long bytes)
		{
			StreamWriter writer;

			lock (m_Lock)
				writer = m_Writer;

			if (writer == null)
				return;

			long start = Stopwatch.GetTimestamp();

			try
			{
				writer.Write(text);

				long elapsed = (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;

				lock (m_Lock)
				{
					m_WriteCalls++;
					m_BytesWritten += bytes;
					m_CurrentPartBytes += bytes;
					m_TotalWriteMs += elapsed;

					if (elapsed > m_MaxWriteMs)
						m_MaxWriteMs = elapsed;

					if (m_CurrentPartBytes >= MaxPartBytes)
						m_RotateRequested = true;
				}
			}
			catch (Exception ex)
			{
				lock (m_Lock)
				{
					m_WriteErrors++;
					m_LastError = ex.GetType().Name + ": " + ex.Message;
				}
			}
		}

		private static void FlushWriter()
		{
			StreamWriter writer;

			lock (m_Lock)
				writer = m_Writer;

			if (writer == null)
				return;

			long start = Stopwatch.GetTimestamp();

			try
			{
				writer.Flush();
				long elapsed = (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;

				lock (m_Lock)
				{
					m_FlushRequested = false;
					m_LastFlushUtc = DateTime.UtcNow;
					m_FlushCount++;
					m_TotalFlushMs += elapsed;

					if (elapsed > m_MaxFlushMs)
						m_MaxFlushMs = elapsed;
				}
			}
			catch (Exception ex)
			{
				lock (m_Lock)
				{
					m_FlushRequested = false;
					m_WriteErrors++;
					m_LastError = ex.GetType().Name + ": " + ex.Message;
				}
			}
		}

		private static void RotateWriter()
		{
			lock (m_Lock)
			{
				m_RotateRequested = false;

				try
				{
					if (m_Writer != null)
					{
						m_Writer.Flush();
						m_Writer.Close();
						m_Writer = null;
					}

					OpenWriterLocked(m_CurrentPart + 1);
					m_Rotations++;
				}
				catch (Exception ex)
				{
					m_WriteErrors++;
					m_LastError = ex.GetType().Name + ": " + ex.Message;
				}
			}
		}

		private static void OpenWriterLocked(int part)
		{
			m_CurrentPart = part < 1 ? 1 : part;
			m_CurrentPartBytes = 0;
			m_CurrentFile = Path.Combine(OutputDirectory, FormatFileName(m_CurrentPart));

			FileStream stream = new FileStream(m_CurrentFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			m_Writer = new StreamWriter(stream, m_Utf8);
			m_Writer.AutoFlush = false;

			m_Writer.WriteLine("ConsoleTranscript format=1");
			m_Writer.WriteLine("startedUtc={0:u}", m_StartedUtc);
			m_Writer.WriteLine("startedLocal={0}", DateTime.Now);

			try { m_Writer.WriteLine("processId={0}", Process.GetCurrentProcess().Id); }
			catch { m_Writer.WriteLine("processId=unavailable"); }

			m_Writer.WriteLine("stdout and stderr are mirrored into this file as plain text.");
			m_Writer.WriteLine("Privacy reminder: console output may include account names, character names, IP addresses, local paths, and exception details.");
			m_Writer.WriteLine();
			m_Writer.Flush();
			m_LastFlushUtc = DateTime.UtcNow;
		}

		private static string FormatFileName(int part)
		{
			if (part <= 1)
				return String.Format("Console-{0}.txt", m_BootStamp);

			return String.Format("Console-{0}-part{1:000}.txt", m_BootStamp, part);
		}

		private static void CloseWriter()
		{
			lock (m_Lock)
			{
				try
				{
					if (m_Writer != null)
					{
						m_Writer.Flush();
						m_Writer.Close();
						m_Writer = null;
					}
				}
				catch
				{
				}
			}
		}

		private static void CleanupRetention()
		{
			try
			{
				string[] files = Directory.GetFiles(OutputDirectory, "Console-*.txt");
				DateTime cutoff = DateTime.UtcNow.AddDays(-RetentionDays);
				List<FileInfo> retained = new List<FileInfo>();

				for (int i = 0; i < files.Length; ++i)
				{
					FileInfo info = new FileInfo(files[i]);

					if (info.LastWriteTimeUtc < cutoff)
					{
						try { info.Delete(); }
						catch { retained.Add(info); }
					}
					else
					{
						retained.Add(info);
					}
				}

				retained.Sort(delegate(FileInfo a, FileInfo b)
				{
					return b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc);
				});

				for (int i = MaxRetainedFiles; i < retained.Count; ++i)
				{
					try { retained[i].Delete(); }
					catch { }
				}
			}
			catch (Exception ex)
			{
				m_LastError = "Retention cleanup failed: " + ex.GetType().Name + ": " + ex.Message;
				m_WriteErrors++;
			}
		}

		private static void WriteInternalWarning(string message)
		{
			try
			{
				TextWriter writer = m_OriginalError != null ? m_OriginalError : m_OriginalOut;

				if (writer != null)
					writer.WriteLine("[ConsoleTranscript] " + message);
			}
			catch
			{
			}
		}

		public static void WriteStats(TextWriter writer)
		{
			if (writer == null)
				return;

			lock (m_Lock)
			{
				writer.WriteLine("ConsoleTranscript: enabled={0}, initialized={1}, currentFile={2}, startedUtc={3:u}, part={4}, bytesWritten={5}, queuedChunks={6}, queuedBytes={7}, maxQueuedChunks={8}, maxQueuedBytes={9}, droppedChunks={10}, droppedBytes={11}, writeErrors={12}, lastError={13}, lastFlushUtc={14:u}, writerAlive={15}",
					m_Enabled,
					m_Initialized,
					!String.IsNullOrEmpty(m_CurrentFile) ? Path.GetFullPath(m_CurrentFile) : "none",
					m_StartedUtc,
					m_CurrentPart,
					m_BytesWritten,
					m_QueuedChunks,
					m_QueuedBytes,
					m_MaxQueuedChunks,
					m_MaxQueuedBytes,
					m_DroppedChunks,
					m_DroppedBytes,
					m_WriteErrors,
					!String.IsNullOrEmpty(m_LastError) ? m_LastError : "none",
					m_LastFlushUtc,
					m_WriterThread != null && m_WriterThread.IsAlive);

				writer.WriteLine("  enqueueCalls={0} enqueuedChunks={1} enqueuedBytes={2} writeCalls={3} totalWriteMs={4} maxWriteMs={5} flushCount={6} totalFlushMs={7} maxFlushMs={8} rotations={9}",
					m_EnqueueCalls,
					m_EnqueuedChunks,
					m_EnqueuedBytes,
					m_WriteCalls,
					m_TotalWriteMs,
					m_MaxWriteMs,
					m_FlushCount,
					m_TotalFlushMs,
					m_MaxFlushMs,
					m_Rotations);
			}
		}

		private sealed class TeeTextWriter : TextWriter
		{
			private readonly TextWriter m_Primary;

			public TeeTextWriter(TextWriter primary)
			{
				m_Primary = primary;
			}

			public override Encoding Encoding
			{
				get { return m_Primary != null ? m_Primary.Encoding : Encoding.Default; }
			}

			public override void Write(char value)
			{
				if (m_Primary != null)
					m_Primary.Write(value);

				ConsoleTranscript.Enqueue(value.ToString());
			}

			public override void Write(string value)
			{
				if (m_Primary != null)
					m_Primary.Write(value);

				ConsoleTranscript.Enqueue(value);
			}

			public override void Write(char[] buffer, int index, int count)
			{
				if (m_Primary != null)
					m_Primary.Write(buffer, index, count);

				if (buffer != null && count > 0)
					ConsoleTranscript.Enqueue(new String(buffer, index, count));
			}

			public override void WriteLine()
			{
				if (m_Primary != null)
					m_Primary.WriteLine();

				ConsoleTranscript.Enqueue(Environment.NewLine);
			}

			public override void WriteLine(string value)
			{
				if (m_Primary != null)
					m_Primary.WriteLine(value);

				ConsoleTranscript.Enqueue((value != null ? value : String.Empty) + Environment.NewLine);
			}

			public override void Flush()
			{
				if (m_Primary != null)
					m_Primary.Flush();

				ConsoleTranscript.RequestFlush();
			}
		}
	}
}
