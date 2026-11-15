using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Server;
using Server.Commands;
using Server.Diagnostics;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Misc
{
    public static class DiscordBridge
    {
        private static readonly string ConfigPath = Path.Combine("Data", "discordbridge.cfg");
        private static readonly string BridgeRootPath = Path.Combine("Data", "DiscordBridge");
        private static readonly string OutgoingPath = Path.Combine(BridgeRootPath, "outgoing");
        private static readonly string IncomingPath = Path.Combine(BridgeRootPath, "incoming");
        private static readonly string ProcessingPath = Path.Combine(BridgeRootPath, "processing");
        private static readonly string FailedPath = Path.Combine(BridgeRootPath, "failed");
        private static readonly string ArchivePath = Path.Combine(BridgeRootPath, "archive");
        private static readonly string StatePath = Path.Combine(BridgeRootPath, "state");

        private static DateTime m_ServerStartedUtc;
        private static string m_PendingShutdownSource;
        private static readonly string ConsoleLogPath = Path.Combine("Logs", "Console.log");
        private static readonly object ConsoleCaptureLock = new object();
        private static bool m_ConsoleCaptureInitialized;
        private static TextWriter m_OriginalConsoleOut;
        private static StreamWriter m_ConsoleLogWriter;
        private static readonly Queue<string> m_RecentConsoleLines = new Queue<string>();
        private static readonly StringBuilder m_ConsoleLineBuffer = new StringBuilder();

        private static readonly string UptimeHistoryPath = Path.Combine(StatePath, "uptime-history.log");
        private static readonly List<UptimeInterval> m_UptimeHistory = new List<UptimeInterval>();
        private static bool m_SessionUptimeRecorded;

        private static readonly TimeSpan CommandPollInterval = TimeSpan.FromMilliseconds(500.0);
        private const int MaxIncomingFilesPerPoll = 100;
        private static readonly bool EnableDiscordBridgeSafePolling = true;
        private const int DiscordBridgePollTimeoutMs = 2000;
        private const int DiscordBridgeMainThreadDrainBudgetMs = 10;
        private const int DiscordBridgeMaxCommandsPerTick = 10;
        private const bool DiscordBridgeSkipIfPollInFlight = true;
        private const int DiscordBridgeMaxQueuedCommands = 500;

        private static bool m_Enabled;
        private static string m_BotToken;
        private static string m_CommandChannelId;
        private static string m_EventWebhookUrl;
        private static string m_PlayerEventWebhookUrl;
        private static string m_ChatWebhookUrl;
        private static string m_CommandWebhookUrl;
        private static string m_CommandPrefix;
        private static HashSet<string> m_StaffUserIds = new HashSet<string>();

        private static readonly object m_PollLock = new object();
        private static readonly Queue<QueuedDiscordCommand> m_QueuedCommands = new Queue<QueuedDiscordCommand>();
        private static bool m_PollInFlight;
        private static long m_PollsStarted;
        private static long m_PollsCompleted;
        private static long m_PollsSkippedInFlight;
        private static long m_PollsTimedOut;
        private static long m_PollErrors;
        private static long m_TotalPollMs;
        private static long m_LastPollMs;
        private static long m_MaxPollMs;
        private static long m_MainThreadDrainLastMs;
        private static long m_MainThreadDrainMaxMs;
        private static long m_CommandsQueued;
        private static long m_CommandsDrained;
        private static long m_CommandsDropped;
        private static long m_LastQueuedCount;
        private static long m_LastDrainedCount;
        private static string m_LastError = String.Empty;
        private static DateTime m_LastSuccessUtc;
        private static DateTime m_LastTimeoutUtc;

        private sealed class QueuedDiscordCommand
        {
            public IncomingEnvelope Envelope;
            public string ProcessingPath;
        }

        public static void Initialize()
        {
            // TLS 1.2 compatibility retained for transitional safety.
            // Runtime Discord transport is now file-queue based (no outbound HTTP needed here).
            // ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // TLS 1.2

            m_ServerStartedUtc = DateTime.UtcNow;
            m_PendingShutdownSource = String.Empty;
            m_SessionUptimeRecorded = false;

            LoadConfig();
            EnsureQueueDirectories();
            LoadUptimeHistory();

            EventSink.Login += OnLogin;
            EventSink.Logout += OnLogout;
            EventSink.CharacterCreated += OnCharacterCreated;
            EventSink.PlayerDeath += OnPlayerDeath;
            EventSink.ServerStarted += OnServerStarted;
            EventSink.Shutdown += OnShutdown;
            EventSink.Crashed += OnCrashed;

            CommandSystem.Register("DiscordStatus", AccessLevel.Administrator, OnDiscordStatusCommand);
            CommandSystem.Register("DiscordReload", AccessLevel.Administrator, OnDiscordReloadCommand);
            CommandSystem.Register("DiscordBridgeStats", AccessLevel.Administrator, OnDiscordBridgeStatsCommand);

            if (CanPollCommands())
            {
                Timer.DelayCall(TimeSpan.FromSeconds(2.0), CommandPollInterval, PollDiscordCommands);
            }
        }

        public static void ReportChat(Mobile from, string text, MessageType type)
        {
            if (!m_Enabled || from == null || String.IsNullOrWhiteSpace(text))
            {
                return;
            }

            text = text.Trim();

            if (text.StartsWith("["))
            {
                return;
            }

            if (IsStaffMobile(from))
            {
                return;
            }

            if (type != MessageType.Regular && type != MessageType.Emote && type != MessageType.Whisper && type != MessageType.Yell)
            {
                return;
            }

            if (type == MessageType.Emote)
            {
                SendChatToDiscord(from.Name + " " + text);
                return;
            }

            SendChatToDiscord(from.Name + ": " + text);
        }

        public static void ReportGameEvent(string eventText, string logCategory)
        {
            if (!m_Enabled || String.IsNullOrWhiteSpace(eventText))
            {
                return;
            }

            string cleaned = eventText;
            int split = cleaned.IndexOf('#');
            if (split >= 0)
            {
                cleaned = cleaned.Substring(0, split);
            }

            cleaned = cleaned.Trim();
            if (cleaned.StartsWith("["))
            {
                return;
            }

            string lowerCategory = (logCategory ?? String.Empty).ToLowerInvariant();

            string lowerText = cleaned.ToLowerInvariant();

            if (lowerText.Contains("puzzlebox") && lowerText.Contains("trap"))
            {
                return;
            }

            if (lowerText.Contains("stone face") && lowerText.Contains("trap"))
            {
                return;
            }

            if (lowerText.Contains("mushroom") && lowerText.Contains("trap"))
            {
                return;
            }

            if (lowerCategory.Contains("battles") || lowerText.Contains("boss"))
            {
                SendEventToDiscord(cleaned);
            }
            else if (lowerCategory.Contains("deaths") || lowerText.Contains("died"))
            {
                SendEventToDiscord(cleaned);
            }
            else if (lowerCategory.Contains("quests") || lowerText.Contains("quest") || lowerText.Contains("completed"))
            {
                SendEventToDiscord(cleaned);
            }
            else if (lowerCategory.Contains("adventures") || lowerCategory.Contains("journey") || lowerText.Contains("journey"))
            {
                SendEventToDiscord(cleaned);
            }
            else if (lowerText.Contains("trap"))
            {
                SendEventToDiscord(cleaned);
            }
            else if (lowerText.Contains("artifact") || lowerText.Contains("loot"))
            {
                SendEventToDiscord(cleaned);
            }
        }

        public static void ReportRestartCountdownStarted(string noticeMessage)
        {
            if (!m_Enabled || String.IsNullOrWhiteSpace(noticeMessage))
            {
                return;
            }

            SetPendingShutdownSource("restart countdown");
            SendEventToDiscord("🔔 " + noticeMessage);
            SendCommandToDiscord(GetStatusMessage("📊"));
        }

        public static void ReportRestartCountdownMinuteUpdate(string noticeMessage)
        {
            if (!m_Enabled || String.IsNullOrWhiteSpace(noticeMessage))
            {
                return;
            }

            SendEventToDiscord("⏱️ " + noticeMessage);
        }

        public static void ReportConsoleShutdownOrRestart(bool restart, bool saveRequested)
        {
            if (!m_Enabled)
            {
                return;
            }

            string action = restart ? "restart" : "shutdown";
            string saveText = saveRequested ? "with save" : "without save";

            SetPendingShutdownSource("admin command");
            SendEventToDiscord(String.Format("🖥️ Console command initiated server {0} ({1}).", action, saveText));
        }

        private static void OnLogin(LoginEventArgs e)
        {
            // Login announcements are relayed through existing realm-access logging
            // (e.g. "X has entered the realm (Online: N)") to avoid duplicate join notices.
        }

        private static void OnLogout(LogoutEventArgs e)
        {
            // Logout announcements are relayed through existing realm-access logging
            // (e.g. "X has left the realm (Online: N)") to avoid duplicate leave notices.
        }

        private static void OnCharacterCreated(CharacterCreatedEventArgs e)
        {
            if (e != null)
            {
                if (e.Mobile != null && IsStaffMobile(e.Mobile))
                {
                    return;
                }

                if (IsStaffAccount(e.Account))
                {
                    return;
                }

                SendPlayerEventToDiscord("New character created: " + e.Name);
            }
        }

        private static void OnPlayerDeath(PlayerDeathEventArgs e)
        {
            if (e.Mobile != null && e.Mobile.Player)
            {
                if (IsStaffMobile(e.Mobile))
                {
                    return;
                }

                SendPlayerEventToDiscord("Player death: " + e.Mobile.Name);
            }
        }

        private static void OnShutdown(ShutdownEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(m_PendingShutdownSource))
            {
                SendEventToDiscord("🖥️ Direct server console shutdown/restart command detected.");
            }

            RecordCurrentSessionUptime();
            SendEventToDiscord("🛑 Server is shutting down.");
            m_PendingShutdownSource = String.Empty;
        }

        private static void OnServerStarted()
        {
            SendEventToDiscord("✅ Server is fully up and loaded.");
        }

        private static void OnCrashed(CrashedEventArgs e)
        {
            RecordCurrentSessionUptime();
            SendEventToDiscord("🚨 Server crash detected. Process is going down.");
        }

        private static void PollDiscordCommands()
        {
            if (!m_Enabled)
            {
                return;
            }

            if (EnableDiscordBridgeSafePolling)
            {
                StartDiscordPollWorker();
                DrainQueuedDiscordCommands();
                return;
            }

            List<string> files;

            try
            {
                EnsureQueueDirectories();
                files = EnumerateOldestFiles(IncomingPath, "*.json", MaxIncomingFilesPerPoll);
            }
            catch (Exception ex)
            {
                DiagnosticConsole.WriteLine("[DiscordBridge]", DiagnosticSeverity.Error, "Incoming queue scan error: {0}", ex.Message);
                return;
            }

            if (files == null || files.Count == 0)
            {
                return;
            }

            for (int i = 0; i < files.Count; i++)
            {
                ProcessIncomingFile(files[i]);
            }
        }

        private static void StartDiscordPollWorker()
        {
            lock (m_PollLock)
            {
                if (m_PollInFlight)
                {
                    if (DiscordBridgeSkipIfPollInFlight)
                        ++m_PollsSkippedInFlight;

                    return;
                }

                m_PollInFlight = true;
                ++m_PollsStarted;
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback(PollDiscordCommandsWorker));
        }

        private static void PollDiscordCommandsWorker(object state)
        {
            long start = Stopwatch.GetTimestamp();
            bool timedOut = false;
            bool hadError = false;
            int queued = 0;
            int dropped = 0;
            string lastError = String.Empty;

            try
            {
                EnsureQueueDirectories();
                List<string> files = EnumerateOldestFiles(IncomingPath, "*.json", MaxIncomingFilesPerPoll);

                for (int i = 0; i < files.Count; ++i)
                {
                    if (GetElapsedMilliseconds(start) >= DiscordBridgePollTimeoutMs)
                    {
                        timedOut = true;
                        break;
                    }

                    QueuedDiscordCommand command;
                    if (TryPrepareIncomingCommand(files[i], out command, ref lastError))
                    {
                        if (QueueDiscordCommand(command))
                        {
                            ++queued;
                        }
                        else
                        {
                            ++dropped;
                            MoveProcessingFile(command.ProcessingPath, FailedPath, ".drop");
                        }
                    }
                    else if (!String.IsNullOrWhiteSpace(lastError))
                    {
                        hadError = true;
                    }
                }
            }
            catch (Exception ex)
            {
                hadError = true;
                lastError = ex.GetType().Name + ": " + ex.Message;
            }
            finally
            {
                long elapsed = GetElapsedMilliseconds(start);
                RecordPollFinished(elapsed, timedOut, hadError, queued, dropped, lastError);
            }
        }

        private static bool TryPrepareIncomingCommand(string incomingPath, out QueuedDiscordCommand command, ref string lastError)
        {
            command = null;

            string fileName = Path.GetFileName(incomingPath);
            string processingPath = Path.Combine(ProcessingPath, fileName);

            try
            {
                if (File.Exists(processingPath))
                {
                    processingPath = Path.Combine(ProcessingPath, Guid.NewGuid().ToString("N") + "_" + fileName);
                }

                File.Move(incomingPath, processingPath);
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception ex)
            {
                lastError = "move: " + ex.Message;
                return false;
            }

            try
            {
                string raw = File.ReadAllText(processingPath, Encoding.UTF8);
                IncomingEnvelope envelope;
                if (!TryParseIncomingEnvelope(raw, out envelope))
                {
                    MoveProcessingFile(processingPath, FailedPath, ".bad");
                    return false;
                }

                command = new QueuedDiscordCommand();
                command.Envelope = envelope;
                command.ProcessingPath = processingPath;
                return true;
            }
            catch (IOException ex)
            {
                lastError = "read: " + ex.Message;
                MoveProcessingFile(processingPath, FailedPath, ".io");
                return false;
            }
            catch (Exception ex)
            {
                lastError = "parse: " + ex.Message;
                MoveProcessingFile(processingPath, FailedPath, ".err");
                return false;
            }
        }

        private static bool QueueDiscordCommand(QueuedDiscordCommand command)
        {
            if (command == null)
                return false;

            lock (m_PollLock)
            {
                if (m_QueuedCommands.Count >= DiscordBridgeMaxQueuedCommands)
                {
                    return false;
                }

                m_QueuedCommands.Enqueue(command);
                ++m_CommandsQueued;
                return true;
            }
        }

        private static void DrainQueuedDiscordCommands()
        {
            long start = Stopwatch.GetTimestamp();
            int drained = 0;

            while (drained < DiscordBridgeMaxCommandsPerTick)
            {
                if (drained > 0 && GetElapsedMilliseconds(start) >= DiscordBridgeMainThreadDrainBudgetMs)
                    break;

                QueuedDiscordCommand command = null;

                lock (m_PollLock)
                {
                    if (m_QueuedCommands.Count == 0)
                        break;

                    command = m_QueuedCommands.Dequeue();
                }

                try
                {
                    IncomingEnvelope envelope = command.Envelope;
                    HandleDiscordCommand(envelope.Content.Trim(), envelope.AuthorId, envelope.AuthorName, envelope.ChannelIsStaff, envelope.ChannelId);
                    MoveProcessingFile(command.ProcessingPath, ArchivePath, ".ok");
                }
                catch (Exception ex)
                {
                    RecordPollError("drain: " + ex.Message);
                    DiagnosticConsole.WriteLine("[DiscordBridge]", DiagnosticSeverity.Error, "Incoming command drain error: {0}", ex.Message);
                    MoveProcessingFile(command.ProcessingPath, FailedPath, ".err");
                }

                ++drained;
            }

            long elapsed = GetElapsedMilliseconds(start);

            lock (m_PollLock)
            {
                m_MainThreadDrainLastMs = elapsed;
                if (elapsed > m_MainThreadDrainMaxMs)
                    m_MainThreadDrainMaxMs = elapsed;

                m_LastDrainedCount = drained;
                m_CommandsDrained += drained;
            }
        }

        private static void RecordPollFinished(long elapsed, bool timedOut, bool hadError, int queued, int dropped, string lastError)
        {
            lock (m_PollLock)
            {
                m_PollInFlight = false;
                ++m_PollsCompleted;
                m_LastPollMs = elapsed;
                m_TotalPollMs += elapsed;
                m_LastQueuedCount = queued;

                if (elapsed > m_MaxPollMs)
                    m_MaxPollMs = elapsed;

                if (timedOut)
                {
                    ++m_PollsTimedOut;
                    m_LastTimeoutUtc = DateTime.UtcNow;
                }

                if (hadError)
                    ++m_PollErrors;

                if (dropped > 0)
                    m_CommandsDropped += dropped;

                if (!String.IsNullOrWhiteSpace(lastError))
                    m_LastError = lastError;

                if (!timedOut && !hadError)
                    m_LastSuccessUtc = DateTime.UtcNow;
            }
        }

        private static void RecordPollError(string message)
        {
            lock (m_PollLock)
            {
                ++m_PollErrors;
                m_LastError = message;
            }
        }

        private static long GetElapsedMilliseconds(long start)
        {
            return (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
        }

        private static bool CanPollCommands()
        {
            return m_Enabled;
        }

        private static void HandleDiscordCommand(string content, string authorId, string authorName, bool isStaffChannel, string channelId)
        {
            if (String.IsNullOrWhiteSpace(content))
            {
                return;
            }

            string trimmed = content.Trim();

            if (!trimmed.StartsWith(m_CommandPrefix))
            {
                return;
            }

            string cmdText = trimmed.Substring(m_CommandPrefix.Length).Trim();
            if (String.IsNullOrWhiteSpace(cmdText))
            {
                return;
            }

            string cmd = cmdText.ToLowerInvariant();
            string cmdArg = String.Empty;
            int split = cmd.IndexOf(' ');
            if (split >= 0)
            {
                cmdArg = cmd.Substring(split + 1).Trim();
                cmd = cmd.Substring(0, split).Trim();
            }

            Action<string> sendReply = delegate(string message)
            {
                SendCommandToDiscord(message, channelId);
            };

            if (cmd == "commands")
            {
                sendReply(GetCommandsHelpMessage(isStaffChannel));
                return;
            }

            if (cmd == "help")
            {
                sendReply(GetDetailedHelpMessage(cmdArg, isStaffChannel));
                return;
            }

            if (cmd == "status")
            {
                sendReply(GetStatusMessage("🟢"));
                return;
            }

            if (cmd == "whois")
            {
                sendReply(GetWhoIsMessage());
                return;
            }

            if (cmd == "cooldown" || cmd == "cooldowns")
            {
                sendReply(GetCooldownMessage());
                return;
            }

            if (cmd == "ratings")
            {
                sendReply(GetRatingsMessage());
                return;
            }

            bool isStaffCommand = (cmd == "save" || cmd == "printlog" || cmd == "reset" || cmd == "restart");
            if (isStaffCommand && !isStaffChannel)
            {
                sendReply("Staff commands are only available in staff command channels.");
                return;
            }

            if (!IsStaffUser(authorId))
            {
                sendReply("You are not authorized for staff commands. User: " + FormatActorName(authorName, authorId));
                return;
            }

            if (cmd == "save")
            {
                try
                {
                    sendReply("Save requested from Discord by " + FormatActorName(authorName, authorId) + ".");
                    AutoSave.Save();
                    sendReply("Save completed.");
                }
                catch (Exception ex)
                {
                    sendReply("Save failed: " + ex.Message);
                }
            }
            else if (cmd == "printlog")
            {
                sendReply(GetPrintLogMessage());
            }
            else if (cmd == "reset" || cmd == "restart")
            {
                SetPendingShutdownSource("discord command");
                sendReply("Restart requested from Discord by " + FormatActorName(authorName, authorId) + ".");
                sendReply(GetStatusMessage("🔄"));
                Timer.DelayCall(TimeSpan.FromSeconds(2.0), delegate { Core.Kill(true); });
            }
            else
            {
                sendReply("Unknown command: " + cmd + " (try: commands, help, status, whois, cooldown, ratings" + (isStaffChannel ? ", save, printlog, restart" : "") + ")");
            }
        }

        private static bool IsStaffUser(string authorId)
        {
            if (String.IsNullOrWhiteSpace(authorId))
            {
                return false;
            }

            return m_StaffUserIds.Contains(NormalizeId(authorId));
        }

        private static bool IsStaffMobile(Mobile mobile)
        {
            return mobile != null && mobile.AccessLevel > AccessLevel.Player;
        }

        private static bool IsStaffAccount(Server.Accounting.IAccount account)
        {
            return account != null && account.AccessLevel > AccessLevel.Player;
        }

        private static string NormalizeId(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return String.Empty;
            }

            StringBuilder sb = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c >= '0' && c <= '9')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static void SendEventToDiscord(string message)
        {
            EnqueueOutgoingMessage("event", message, "game_event");
        }

        private static void SendPlayerEventToDiscord(string message)
        {
            EnqueueOutgoingMessage("player_event", message, "player_event");
        }

        private static void SendChatToDiscord(string message)
        {
            EnqueueOutgoingMessage("chat", message, "chat");
        }

        private static void SendCommandToDiscord(string message)
        {
            SendCommandToDiscord(message, String.Empty);
        }

        private static void SendCommandToDiscord(string message, string channelId)
        {
            string source = "command";
            if (!String.IsNullOrWhiteSpace(channelId))
            {
                source = "command_channel:" + channelId;
            }

            EnqueueOutgoingMessage("command", message, source);
        }

        private static void EnqueueOutgoingMessage(string channelType, string message, string source)
        {
            if (!m_Enabled || String.IsNullOrWhiteSpace(channelType) || String.IsNullOrWhiteSpace(message))
            {
                return;
            }

            try
            {
                EnsureQueueDirectories();

                OutgoingEnvelope envelope = new OutgoingEnvelope();
                envelope.Id = Guid.NewGuid().ToString("N");
                envelope.CreatedUtc = DateTime.UtcNow;
                envelope.ChannelType = channelType;
                envelope.Content = message;
                envelope.Attempts = 0;
                envelope.Source = source;

                string ts = envelope.CreatedUtc.ToString("yyyyMMddHHmmssfff");
                string fileName = ts + "_" + envelope.Id + ".json";
                string tmpPath = Path.Combine(OutgoingPath, fileName + ".tmp");
                string finalPath = Path.Combine(OutgoingPath, fileName);

                File.WriteAllText(tmpPath, SerializeOutgoingEnvelope(envelope), Encoding.UTF8);
                File.Move(tmpPath, finalPath);
            }
            catch (Exception ex)
            {
                DiagnosticConsole.WriteLine("[DiscordBridge]", DiagnosticSeverity.Error, "Failed to enqueue outgoing message: {0}", ex.Message);
            }
        }

        private static int GetOnlinePlayerCount()
        {
            int count = 0;
            foreach (NetState ns in NetState.Instances)
            {
                if (ns != null && ns.Mobile != null && ns.Mobile.Player)
                {
                    if (!IsStaffMobile(ns.Mobile))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static List<string> GetOnlinePlayerNames()
        {
            List<string> names = new List<string>();

            foreach (NetState ns in NetState.Instances)
            {
                if (ns == null || ns.Mobile == null || !ns.Mobile.Player)
                {
                    continue;
                }

                if (IsStaffMobile(ns.Mobile))
                {
                    continue;
                }

                string name = ns.Mobile.Name;
                if (String.IsNullOrWhiteSpace(name))
                {
                    name = "(unknown)";
                }

                if (!names.Contains(name))
                {
                    names.Add(name);
                }
            }

            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        private static string FormatUptime(TimeSpan span)
        {
            return String.Format("{0}d {1}h {2}m", (int)span.TotalDays, span.Hours, span.Minutes);
        }

        private static string GetStatusMessage(string prefixEmoji)
        {
            string baseLine = String.Format(
                "{0} Server online | Players: {1} | Uptime: {2} | Avg CPS: {3:0.00}",
                prefixEmoji,
                GetOnlinePlayerCount(),
                FormatUptime(DateTime.UtcNow - m_ServerStartedUtc),
                Core.AverageCPS
            );

            return baseLine + "\n" + GetExtendedUptimeSummary();
        }

        private static string GetExtendedUptimeSummary()
        {
            DateTime now = DateTime.UtcNow;

            DateTime monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime yearStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime fiveYearStart = now.AddYears(-5);

            TimeSpan monthUp = GetWindowUptime(monthStart, now);
            TimeSpan yearUp = GetWindowUptime(yearStart, now);
            TimeSpan fiveYearUp = GetWindowUptime(fiveYearStart, now);

            TimeSpan monthWindow = now - monthStart;
            TimeSpan yearWindow = now - yearStart;
            TimeSpan fiveYearWindow = now - fiveYearStart;

            return String.Format(
                "Uptime calc | Month: {0} ({1:0.00}%) | Year: {2} ({3:0.00}%) | 5Y: {4} ({5:0.00}%)",
                FormatUptime(monthUp),
                GetUptimePercent(monthUp, monthWindow),
                FormatUptime(yearUp),
                GetUptimePercent(yearUp, yearWindow),
                FormatUptime(fiveYearUp),
                GetUptimePercent(fiveYearUp, fiveYearWindow)
            );
        }

        private static TimeSpan GetWindowUptime(DateTime windowStartUtc, DateTime nowUtc)
        {
            if (nowUtc <= windowStartUtc)
            {
                return TimeSpan.Zero;
            }

            long totalTicks = 0;

            for (int i = 0; i < m_UptimeHistory.Count; i++)
            {
                totalTicks += GetOverlapTicks(m_UptimeHistory[i].StartUtc, m_UptimeHistory[i].EndUtc, windowStartUtc, nowUtc);
            }

            totalTicks += GetOverlapTicks(m_ServerStartedUtc, nowUtc, windowStartUtc, nowUtc);

            if (totalTicks <= 0)
            {
                return TimeSpan.Zero;
            }

            return TimeSpan.FromTicks(totalTicks);
        }


        private static long GetOverlapTicks(DateTime intervalStartUtc, DateTime intervalEndUtc, DateTime windowStartUtc, DateTime windowEndUtc)
        {
            DateTime start = intervalStartUtc > windowStartUtc ? intervalStartUtc : windowStartUtc;
            DateTime end = intervalEndUtc < windowEndUtc ? intervalEndUtc : windowEndUtc;

            if (end <= start)
            {
                return 0;
            }

            return (end - start).Ticks;
        }

        private static void LoadUptimeHistory()
        {
            m_UptimeHistory.Clear();

            try
            {
                Directory.CreateDirectory(StatePath);

                if (!File.Exists(UptimeHistoryPath))
                {
                    return;
                }

                string[] lines = File.ReadAllLines(UptimeHistoryPath);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (String.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] parts = line.Split('|');
                    if (parts.Length != 2)
                    {
                        continue;
                    }

                    long startTicks;
                    long endTicks;

                    if (!long.TryParse(parts[0], out startTicks) || !long.TryParse(parts[1], out endTicks))
                    {
                        continue;
                    }

                    DateTime startUtc = new DateTime(startTicks, DateTimeKind.Utc);
                    DateTime endUtc = new DateTime(endTicks, DateTimeKind.Utc);

                    if (endUtc > startUtc)
                    {
                        m_UptimeHistory.Add(new UptimeInterval(startUtc, endUtc));
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticConsole.WriteLine("[DiscordBridge]", DiagnosticSeverity.Error, "Failed to load uptime history: {0}", ex.Message);
            }
        }

        private static void RecordCurrentSessionUptime()
        {
            if (m_SessionUptimeRecorded)
            {
                return;
            }

            DateTime nowUtc = DateTime.UtcNow;

            if (nowUtc <= m_ServerStartedUtc)
            {
                return;
            }

            UptimeInterval interval = new UptimeInterval(m_ServerStartedUtc, nowUtc);
            m_UptimeHistory.Add(interval);

            try
            {
                Directory.CreateDirectory(StatePath);
                File.AppendAllText(UptimeHistoryPath, interval.StartUtc.Ticks + "|" + interval.EndUtc.Ticks + Environment.NewLine);
                m_SessionUptimeRecorded = true;
            }
            catch (Exception ex)
            {
                DiagnosticConsole.WriteLine("[DiscordBridge]", DiagnosticSeverity.Error, "Failed to persist uptime history: {0}", ex.Message);
            }
        }

        private struct UptimeInterval
        {
            public readonly DateTime StartUtc;
            public readonly DateTime EndUtc;

            public UptimeInterval(DateTime startUtc, DateTime endUtc)
            {
                StartUtc = startUtc;
                EndUtc = endUtc;
            }
        }
        private static double GetUptimePercent(TimeSpan uptime, TimeSpan window)
        {
            if (window.TotalSeconds <= 0.0)
            {
                return 0.0;
            }

            return (uptime.TotalSeconds / window.TotalSeconds) * 100.0;
        }

        private static string GetWhoIsMessage()
        {
            List<string> names = GetOnlinePlayerNames();
            if (names.Count == 0)
            {
                return "Online players (excluding staff): none";
            }

            const int maxNames = 30;
            int visibleCount = Math.Min(names.Count, maxNames);
            string shown = String.Join(", ", names.GetRange(0, visibleCount).ToArray());

            if (names.Count > maxNames)
            {
                shown += String.Format(", ... (+{0} more)", names.Count - maxNames);
            }

            return String.Format("Online players (excluding staff) [{0}]: {1}", names.Count, shown);
        }

        private static string GetCooldownMessage()
        {
            DateTime now = DateTime.UtcNow;

            TimeSpan lottery = LadyLuck.NextDrawingTimeUtc - now;
            TimeSpan hourly = TaskManager.NextRunTime - now;
            TimeSpan daily = TaskManagerDaily.NextRunTime - now;

            if (lottery < TimeSpan.Zero)
            {
                lottery = TimeSpan.Zero;
            }

            if (hourly < TimeSpan.Zero)
            {
                hourly = TimeSpan.Zero;
            }

            if (daily < TimeSpan.Zero)
            {
                daily = TimeSpan.Zero;
            }

            string restartPart = "Restart countdown: inactive";
            TimeSpan restartRemaining;
            if (AutoRestart.TryGetRemaining(out restartRemaining))
            {
                restartPart = "Restart countdown: " + FormatClock(restartRemaining);
            }

            return String.Format(
                "Cooldowns | Lottery draw: {0} | Hourly reset: {1} | Daily reset: {2} | {3}",
                FormatClock(lottery),
                FormatClock(hourly),
                FormatClock(daily),
                restartPart
            );
        }

        private static string GetCommandsHelpMessage(bool includeStaffCommands)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available Discord bridge commands:");
            sb.AppendLine("• " + m_CommandPrefix + " commands");
            sb.AppendLine("• " + m_CommandPrefix + " help [command]");
            sb.AppendLine("• " + m_CommandPrefix + " status");
            sb.AppendLine("• " + m_CommandPrefix + " whois");
            sb.AppendLine("• " + m_CommandPrefix + " cooldown");
            sb.AppendLine("• " + m_CommandPrefix + " ratings");

            if (includeStaffCommands)
            {
                sb.AppendLine("• " + m_CommandPrefix + " save (staff only)");
                sb.AppendLine("• " + m_CommandPrefix + " printlog (staff only)");
                sb.AppendLine("• " + m_CommandPrefix + " restart | " + m_CommandPrefix + " reset (staff only)");
            }

            return sb.ToString().TrimEnd();
        }

        private static string GetDetailedHelpMessage(string query, bool includeStaffCommands)
        {
            List<string> commandDetails = new List<string>();
            commandDetails.Add(m_CommandPrefix + " commands — Shows a highlighted list of all available Discord bridge commands.");
            commandDetails.Add(m_CommandPrefix + " help [command] — Shows detailed help for all commands, or only commands whose name/description matches your search text.");
            commandDetails.Add(m_CommandPrefix + " status — Shows server alive status, online non-staff player count, uptime, and average CPS.");
            commandDetails.Add(m_CommandPrefix + " whois — Lists online player character names, excluding in-game staff accounts.");
            commandDetails.Add(m_CommandPrefix + " cooldown — Shows remaining timers for lottery draw, hourly reset, daily reset, and restart countdown (if active).");
            commandDetails.Add(m_CommandPrefix + " ratings — Shows current avatar leaderboard snapshots for Good (Order) and Evil (Chaos).");

            if (includeStaffCommands)
            {
                commandDetails.Add(m_CommandPrefix + " save — Staff only. Runs an immediate world save and reports completion/failure.");
                commandDetails.Add(m_CommandPrefix + " printlog — Staff only. Reads the last 10 lines from the server console log file and returns them.");
                commandDetails.Add(m_CommandPrefix + " restart / " + m_CommandPrefix + " reset — Staff only. Announces restart request, posts status snapshot, then restarts the shard.");
            }

            if (String.IsNullOrWhiteSpace(query))
            {
                return String.Format(
                    "Help usage: `{0} help <command-or-keyword>` to search command help. Try `{0} commands` to list all commands. Example: `{0} help cooldown`.",
                    m_CommandPrefix
                );
            }

            string q = query.ToLowerInvariant();
            List<string> matches = new List<string>();

            for (int i = 0; i < commandDetails.Count; i++)
            {
                string detail = commandDetails[i];
                if (detail.ToLowerInvariant().Contains(q))
                {
                    matches.Add(detail);
                }
            }

            if (matches.Count == 0)
            {
                return "No help results for \"" + query + "\". Try " + m_CommandPrefix + " commands to see all commands.";
            }

            return "Help results for \"" + query + "\":\n" + String.Join("\n", matches.ToArray());
        }

        private static string GetRatingsMessage()
        {
            AetherGlobe.UpdateAvatars();

            List<string> goodEntries = BuildRatingEntries(AetherGlobe.OrderAvatars, 5);
            List<string> evilEntries = BuildRatingEntries(AetherGlobe.ChaosAvatars, 5);

            int rows = 5;
            int goodWidth = "none".Length;

            for (int i = 0; i < goodEntries.Count; i++)
            {
                if (goodEntries[i].Length > goodWidth)
                {
                    goodWidth = goodEntries[i].Length;
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Ratings | Good (Order): | Evil (Chaos):");

            for (int i = 0; i < rows; i++)
            {
                string left = i < goodEntries.Count ? goodEntries[i] : "none";
                string right = i < evilEntries.Count ? evilEntries[i] : "none";

                sb.Append(left.PadRight(goodWidth + 4));
                sb.Append(" | ");
                sb.AppendLine(right);
            }

            return "Here they are!\n```\n" + sb.ToString().TrimEnd() + "```\n" + GetBalanceSummaryMessage();
        }

        private static string GetBalanceSummaryMessage()
        {
            int balanceLevel = AetherGlobe.BalanceLevel;
            int evilPoints = balanceLevel - 50000;
            int goodPoints = 50000 - balanceLevel;

            string shiftLine;
            if (evilPoints > 0)
            {
                shiftLine = "Balance shift: Chaos by " + evilPoints + " points.";
            }
            else if (goodPoints > 0)
            {
                shiftLine = "Balance shift: Order by " + goodPoints + " points.";
            }
            else
            {
                shiftLine = "Balance shift: Neutral (0 points).";
            }

            string bonusLine;
            if (balanceLevel > 51000)
            {
                int bonus = (int)((((double)balanceLevel - 50000.0) / 50000.0) * 0.17 * 100.0);
                bonusLine = "Damage bonus: Evil Avatars +" + bonus + "%";
            }
            else if (balanceLevel < 49000)
            {
                int bonus = (int)((((double)(50000 - balanceLevel)) / 50000.0) * 0.17 * 100.0);
                bonusLine = "Damage bonus: Good Avatars +" + bonus + "%";
            }
            else
            {
                bonusLine = "Damage bonus: Neutral (0%).";
            }

            return shiftLine + "\n" + bonusLine;
        }

        private static void SetPendingShutdownSource(string source)
        {
            m_PendingShutdownSource = source ?? String.Empty;
        }

        private static List<string> BuildRatingEntries(SortedDictionary<int, string> ratings, int maxEntries)
        {
            List<string> entries = new List<string>();

            int rank = 0;
            foreach (KeyValuePair<int, string> kvp in ratings)
            {
                if (rank >= maxEntries)
                {
                    break;
                }

                rank++;
                entries.Add(String.Format("{0}. {1} ({2})", rank, kvp.Value, kvp.Key));
            }

            if (entries.Count == 0)
            {
                entries.Add("none");
            }

            return entries;
        }

        private static string GetPrintLogMessage()
        {
            EnsureConsoleCaptureInitialized();

            string[] bufferedLines;
            lock (ConsoleCaptureLock)
            {
                bufferedLines = m_RecentConsoleLines.ToArray();
            }

            if (bufferedLines.Length > 0)
            {
                int takeBuffered = Math.Min(10, bufferedLines.Length);
                int startBuffered = bufferedLines.Length - takeBuffered;

                StringBuilder bufferedOutput = new StringBuilder();
                bufferedOutput.AppendLine("Last " + takeBuffered + " lines from live console capture:");
                bufferedOutput.AppendLine("```");
                for (int i = startBuffered; i < bufferedLines.Length; i++)
                {
                    bufferedOutput.AppendLine(bufferedLines[i]);
                }
                bufferedOutput.Append("```");
                return bufferedOutput.ToString();
            }

            string[] candidates = new string[]
            {
                Path.Combine("Logs", "Console.log"),
                Path.Combine("Logs", "console.log"),
                "Console.log",
                "console.log",
                Path.Combine("Logs", "Server.log"),
                Path.Combine("Logs", "server.log")
            };

            string foundPath = null;
            for (int i = 0; i < candidates.Length; i++)
            {
                if (File.Exists(candidates[i]))
                {
                    foundPath = candidates[i];
                    break;
                }
            }

            if (String.IsNullOrWhiteSpace(foundPath))
            {
                return "PrintLog: console log file was not found. Checked: " + String.Join(", ", candidates);
            }

            try
            {
                string[] lines = ReadLastLinesFromFile(foundPath, 10);
                if (lines.Length == 0)
                {
                    return "PrintLog: `" + foundPath + "` is empty.";
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Last " + lines.Length + " lines from `" + foundPath + "`:");
                sb.AppendLine("```");
                for (int i = 0; i < lines.Length; i++)
                {
                    sb.AppendLine(lines[i]);
                }
                sb.Append("```");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return "PrintLog failed: " + ex.Message;
            }
        }

        private static string[] ReadLastLinesFromFile(string path, int maxLines)
        {
            if (maxLines <= 0)
            {
                return new string[0];
            }

            Queue<string> lines = new Queue<string>();
            const int maxTailBytes = 65536;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                long start = fs.Length > maxTailBytes ? fs.Length - maxTailBytes : 0;
                fs.Seek(start, SeekOrigin.Begin);

                using (StreamReader sr = new StreamReader(fs))
                {
                    if (start > 0)
                    {
                        sr.ReadLine(); // discard partial first line from the tail window
                    }

                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lines.Enqueue(line);

                        while (lines.Count > maxLines)
                        {
                            lines.Dequeue();
                        }
                    }
                }
            }

            string[] result = new string[lines.Count];
            lines.CopyTo(result, 0);
            return result;
        }

        private static void EnsureConsoleCaptureInitialized()
        {
            lock (ConsoleCaptureLock)
            {
                if (m_ConsoleCaptureInitialized)
                {
                    return;
                }

                try
                {
                    if (ConsoleTranscript.RegisterObserver(CaptureConsoleText))
                    {
                        m_ConsoleCaptureInitialized = true;
                        DiagnosticConsole.WriteLine("[DiscordBridge]", DiagnosticSeverity.Good, "Console capture subscribed to ConsoleTranscript.");
                        return;
                    }

                    string directory = Path.GetDirectoryName(ConsoleLogPath);
                    if (!String.IsNullOrWhiteSpace(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    m_OriginalConsoleOut = Console.Out;
                    m_ConsoleLogWriter = new StreamWriter(new FileStream(ConsoleLogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
                    m_ConsoleLogWriter.AutoFlush = true;

                    Console.SetOut(new TeeTextWriter(m_OriginalConsoleOut, m_ConsoleLogWriter, CaptureConsoleText));
                    m_ConsoleCaptureInitialized = true;

                    DiagnosticConsole.WriteLine("[DiscordBridge]", DiagnosticSeverity.Good, "Console log capture initialized: {0}", ConsoleLogPath);
                }
                catch (Exception ex)
                {
                    DiagnosticConsole.WriteLine("[DiscordBridge]", DiagnosticSeverity.Error, "Failed to initialize console capture: {0}", ex.Message);
                }
            }
        }

        private static void CaptureConsoleText(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return;
            }

            lock (ConsoleCaptureLock)
            {
                m_ConsoleLineBuffer.Append(text);

                while (true)
                {
                    int newlineIndex = m_ConsoleLineBuffer.ToString().IndexOf('\n');
                    if (newlineIndex < 0)
                    {
                        break;
                    }

                    string line = m_ConsoleLineBuffer.ToString(0, newlineIndex).TrimEnd('\r');
                    m_ConsoleLineBuffer.Remove(0, newlineIndex + 1);

                    m_RecentConsoleLines.Enqueue(line);
                    while (m_RecentConsoleLines.Count > 500)
                    {
                        m_RecentConsoleLines.Dequeue();
                    }
                }
            }
        }

        private sealed class TeeTextWriter : TextWriter
        {
            private readonly TextWriter _primary;
            private readonly TextWriter _secondary;
            private readonly Action<string> _capture;

            public TeeTextWriter(TextWriter primary, TextWriter secondary, Action<string> capture)
            {
                _primary = primary;
                _secondary = secondary;
                _capture = capture;
            }

            public override Encoding Encoding
            {
                get { return _primary.Encoding; }
            }

            public override void Write(char value)
            {
                _primary.Write(value);
                _secondary.Write(value);
                _capture(value.ToString());
            }

            public override void Write(string value)
            {
                _primary.Write(value);
                _secondary.Write(value);
                _capture(value);
            }

            public override void WriteLine(string value)
            {
                _primary.WriteLine(value);
                _secondary.WriteLine(value);
                _capture((value ?? String.Empty) + Environment.NewLine);
            }

            public override void Flush()
            {
                _primary.Flush();
                _secondary.Flush();
            }
        }

        private static string FormatClock(TimeSpan span)
        {
            if (span < TimeSpan.Zero)
            {
                span = TimeSpan.Zero;
            }

            int hours = (int)Math.Floor(span.TotalHours);
            return String.Format("{0:D2}:{1:D2}:{2:D2}", hours, span.Minutes, span.Seconds);
        }

        private static void LoadConfig()
        {
            m_Enabled = false;
            m_BotToken = String.Empty;
            m_CommandChannelId = String.Empty;
            m_EventWebhookUrl = String.Empty;
            m_PlayerEventWebhookUrl = String.Empty;
            m_ChatWebhookUrl = String.Empty;
            m_CommandWebhookUrl = String.Empty;
            m_CommandPrefix = "!uo";
            m_StaffUserIds = new HashSet<string>();

            EnsureConfigExists();

            string legacyListenChannelId = String.Empty;
            string legacyNotifyWebhookUrl = String.Empty;

            string[] lines = File.ReadAllLines(ConfigPath);
            foreach (string raw in lines)
            {
                if (String.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                string line = raw.Trim();
                if (line.StartsWith("#"))
                {
                    continue;
                }

                int idx = line.IndexOf('=');
                if (idx < 1)
                {
                    continue;
                }

                string key = line.Substring(0, idx).Trim().ToLowerInvariant();
                string value = line.Substring(idx + 1).Trim();

                if (key == "enabled")
                {
                    m_Enabled = (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase));
                }
                else if (key == "bot_token")
                {
                    m_BotToken = value;
                }
                else if (key == "command_channel_id")
                {
                    m_CommandChannelId = value;
                }
                else if (key.StartsWith("command_channel_") && key.EndsWith("_id"))
                {
                    // Phase 3+ config supports multiple command channels.
                    // Keep the first configured channel for lightweight shard status display.
                    if (String.IsNullOrWhiteSpace(m_CommandChannelId) && !String.IsNullOrWhiteSpace(value))
                    {
                        m_CommandChannelId = value;
                    }
                }
                else if (key == "event_webhook_url")
                {
                    m_EventWebhookUrl = value;
                }
                else if (key == "player_event_webhook_url")
                {
                    m_PlayerEventWebhookUrl = value;
                }
                else if (key == "chat_webhook_url")
                {
                    m_ChatWebhookUrl = value;
                }
                else if (key == "command_webhook_url")
                {
                    m_CommandWebhookUrl = value;
                }
                else if (key == "command_prefix")
                {
                    m_CommandPrefix = String.IsNullOrWhiteSpace(value) ? "!uo" : value;
                }
                else if (key == "staff_user_ids")
                {
                    string[] ids = value.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < ids.Length; i++)
                    {
                        string normalized = NormalizeId(ids[i]);
                        if (!String.IsNullOrWhiteSpace(normalized))
                        {
                            m_StaffUserIds.Add(normalized);
                        }
                    }
                }
                else if (key == "listen_channel_id")
                {
                    legacyListenChannelId = value;
                }
                else if (key == "notify_webhook_url")
                {
                    legacyNotifyWebhookUrl = value;
                }
            }

            // Legacy compatibility
            if (String.IsNullOrWhiteSpace(m_CommandChannelId) && !String.IsNullOrWhiteSpace(legacyListenChannelId))
            {
                m_CommandChannelId = legacyListenChannelId;
            }

            if (String.IsNullOrWhiteSpace(m_EventWebhookUrl) && !String.IsNullOrWhiteSpace(legacyNotifyWebhookUrl))
            {
                m_EventWebhookUrl = legacyNotifyWebhookUrl;
            }

            if (String.IsNullOrWhiteSpace(m_CommandWebhookUrl))
            {
                m_CommandWebhookUrl = m_EventWebhookUrl;
            }
            if (String.IsNullOrWhiteSpace(m_PlayerEventWebhookUrl))
            {
                m_PlayerEventWebhookUrl = m_EventWebhookUrl;
            }
            if (String.IsNullOrWhiteSpace(m_ChatWebhookUrl))
            {
                m_ChatWebhookUrl = m_EventWebhookUrl;
            }

            // legacy fields retained in cfg for compatibility with older deployments;
            // they are unused by phase 1 local file queue transport.
        }

        private static void EnsureConfigExists()
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            if (File.Exists(ConfigPath))
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Discord bridge config");
            sb.AppendLine("# Set enabled=true after filling values");
            sb.AppendLine("enabled=false");
            sb.AppendLine("# Legacy network transport settings retained for compatibility; shard now uses local queue folders.");
            sb.AppendLine("bot_token=");
            sb.AppendLine("# Legacy single command channel id (still supported):");
            sb.AppendLine("command_channel_id=");
            sb.AppendLine("# Preferred multi-channel command config (worker consumes these):");
            sb.AppendLine("# command_channel_<n>_id = Discord channel id");
            sb.AppendLine("# command_channel_<n>_staff = true for staff command channels, false for user command channels");
            sb.AppendLine("command_channel_1_id=");
            sb.AppendLine("command_channel_1_staff=true");
            sb.AppendLine("command_channel_1_webhook_url=");
            sb.AppendLine("command_channel_2_id=");
            sb.AppendLine("command_channel_2_staff=false");
            sb.AppendLine("command_channel_2_webhook_url=");
            sb.AppendLine("command_channel_3_id=");
            sb.AppendLine("command_channel_3_staff=false");
            sb.AppendLine("command_channel_3_webhook_url=");
            sb.AppendLine("# Legacy output webhook values (external worker can still use these)");
            sb.AppendLine("event_webhook_url=");
            sb.AppendLine("# Optional: dedicated webhook for player lifecycle events (join/leave/death/new character)");
            sb.AppendLine("player_event_webhook_url=");
            sb.AppendLine("# Optional: dedicated webhook for player chat relay");
            sb.AppendLine("chat_webhook_url=");
            sb.AppendLine("# Optional: command replies to a separate webhook/channel");
            sb.AppendLine("command_webhook_url=");
            sb.AppendLine("command_prefix=!uo");
            sb.AppendLine("# Comma-separated Discord user IDs allowed for save/reset/restart");
            sb.AppendLine("staff_user_ids=");
            sb.AppendLine("# Legacy keys still supported: listen_channel_id, notify_webhook_url");

            File.WriteAllText(ConfigPath, sb.ToString());
        }

        private static string EscapeJson(string value)
        {
            string normalized = (value ?? String.Empty)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");

            return normalized
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n");
        }

        private static void EnsureQueueDirectories()
        {
            Directory.CreateDirectory(BridgeRootPath);
            Directory.CreateDirectory(OutgoingPath);
            Directory.CreateDirectory(IncomingPath);
            Directory.CreateDirectory(ProcessingPath);
            Directory.CreateDirectory(FailedPath);
            Directory.CreateDirectory(ArchivePath);
            Directory.CreateDirectory(StatePath);
        }

        private static void ProcessIncomingFile(string incomingPath)
        {
            string fileName = Path.GetFileName(incomingPath);
            string processingPath = Path.Combine(ProcessingPath, fileName);

            try
            {
                if (File.Exists(processingPath))
                {
                    processingPath = Path.Combine(ProcessingPath, Guid.NewGuid().ToString("N") + "_" + fileName);
                }

                File.Move(incomingPath, processingPath);
            }
            catch (IOException)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            catch (Exception ex)
            {
                DiagnosticConsole.WriteLine("[DiscordBridge]", DiagnosticSeverity.Error, "Failed to move incoming file to processing: {0}", ex.Message);
                return;
            }

            try
            {
                string raw = File.ReadAllText(processingPath, Encoding.UTF8);
                IncomingEnvelope envelope;
                if (!TryParseIncomingEnvelope(raw, out envelope))
                {
                    MoveProcessingFile(processingPath, FailedPath, ".bad");
                    return;
                }

                HandleDiscordCommand(envelope.Content.Trim(), envelope.AuthorId, envelope.AuthorName, envelope.ChannelIsStaff, envelope.ChannelId);
                MoveProcessingFile(processingPath, ArchivePath, ".ok");
            }
            catch (IOException)
            {
                MoveProcessingFile(processingPath, FailedPath, ".io");
            }
            catch (Exception ex)
            {
                DiagnosticConsole.WriteLine("[DiscordBridge]", DiagnosticSeverity.Error, "Incoming file processing error: {0}", ex.Message);
                MoveProcessingFile(processingPath, FailedPath, ".err");
            }
        }

        private static void MoveProcessingFile(string sourcePath, string targetDirectory, string suffix)
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(sourcePath);
                string ext = Path.GetExtension(sourcePath);
                string dateSegment = DateTime.UtcNow.ToString("yyyy-MM-dd");
                string datedTargetDirectory = Path.Combine(targetDirectory, dateSegment);
                Directory.CreateDirectory(datedTargetDirectory);
                string targetPath = Path.Combine(datedTargetDirectory, name + suffix + ext);

                if (File.Exists(targetPath))
                {
                    targetPath = Path.Combine(datedTargetDirectory, name + suffix + "_" + DateTime.UtcNow.Ticks.ToString() + ext);
                }

                File.Move(sourcePath, targetPath);
            }
            catch (Exception ex)
            {
                DiagnosticConsole.WriteLine("[DiscordBridge]", DiagnosticSeverity.Error, "Failed to move processed file: {0}", ex.Message);
            }
        }

        private static bool TryParseIncomingEnvelope(string raw, out IncomingEnvelope envelope)
        {
            envelope = new IncomingEnvelope();

            if (String.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            envelope.Id = ExtractJsonField(raw, "id");
            envelope.AuthorId = NormalizeId(ExtractJsonField(raw, "authorId"));
            envelope.AuthorName = ExtractJsonField(raw, "authorName");
            envelope.Content = ExtractJsonField(raw, "content");
            envelope.ChannelId = ExtractJsonField(raw, "channelId");
            string channelIsStaffRaw = ExtractJsonField(raw, "channelIsStaff");
            envelope.ChannelIsStaff = channelIsStaffRaw == "1" || channelIsStaffRaw.Equals("true", StringComparison.OrdinalIgnoreCase);

            if (String.IsNullOrWhiteSpace(envelope.Content))
            {
                envelope.Content = ExtractJsonField(raw, "raw");
            }

            if (String.IsNullOrWhiteSpace(envelope.Content))
            {
                return false;
            }

            return true;
        }

        private static string SerializeOutgoingEnvelope(OutgoingEnvelope envelope)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            sb.Append("\"id\":\"").Append(EscapeJson(envelope.Id)).Append("\",");
            sb.Append("\"createdUtc\":\"").Append(envelope.CreatedUtc.ToString("o")).Append("\",");
            sb.Append("\"channelType\":\"").Append(EscapeJson(envelope.ChannelType)).Append("\",");
            sb.Append("\"content\":\"").Append(EscapeJson(envelope.Content)).Append("\",");
            sb.Append("\"attempts\":").Append(envelope.Attempts);
            if (!String.IsNullOrWhiteSpace(envelope.Source))
            {
                sb.Append(",\"source\":\"").Append(EscapeJson(envelope.Source)).Append("\"");
            }
            sb.Append('}');

            return sb.ToString();
        }

        private static string ExtractJsonField(string json, string field)
        {
            Match m = Regex.Match(json, "\\\"" + Regex.Escape(field) + "\\\"\\s*:\\s*\\\"(?<v>(?:\\\\.|[^\\\"])*)\\\"", RegexOptions.Singleline);
            if (!m.Success)
            {
                return String.Empty;
            }

            return Regex.Unescape(m.Groups["v"].Value);
        }

        private static List<string> EnumerateOldestFiles(string directory, string searchPattern, int maxCount)
        {
            List<string> selected = new List<string>();

            if (maxCount <= 0 || !Directory.Exists(directory))
            {
                return selected;
            }

            foreach (string file in Directory.EnumerateFiles(directory, searchPattern))
            {
                DateTime created;
                try
                {
                    created = File.GetCreationTimeUtc(file);
                }
                catch
                {
                    continue;
                }

                int insertIndex = selected.Count;
                while (insertIndex > 0)
                {
                    string compareFile = selected[insertIndex - 1];
                    DateTime compareCreated;

                    try
                    {
                        compareCreated = File.GetCreationTimeUtc(compareFile);
                    }
                    catch
                    {
                        compareCreated = DateTime.MaxValue;
                    }

                    if (created >= compareCreated)
                    {
                        break;
                    }

                    insertIndex--;
                }

                selected.Insert(insertIndex, file);

                if (selected.Count > maxCount)
                {
                    selected.RemoveAt(selected.Count - 1);
                }
            }

            return selected;
        }

        private static int SafeCountFiles(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    return 0;
                }

                int count = 0;
                foreach (string file in Directory.EnumerateFiles(directory, "*.json", SearchOption.AllDirectories))
                {
                    if (!String.IsNullOrWhiteSpace(file))
                    {
                        count++;
                    }
                }

                return count;
            }
            catch
            {
                return -1;
            }
        }

        private static string GetOldestAgeSummary(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    return "n/a";
                }

                DateTime? oldest = null;
                foreach (string file in Directory.EnumerateFiles(directory, "*.json"))
                {
                    DateTime created;
                    try
                    {
                        created = File.GetCreationTimeUtc(file);
                    }
                    catch
                    {
                        continue;
                    }

                    if (!oldest.HasValue || created < oldest.Value)
                    {
                        oldest = created;
                    }
                }

                if (!oldest.HasValue)
                {
                    return "none";
                }

                TimeSpan age = DateTime.UtcNow - oldest.Value;
                if (age < TimeSpan.Zero)
                {
                    age = TimeSpan.Zero;
                }

                return ((int)age.TotalMinutes).ToString() + "m";
            }
            catch
            {
                return "n/a";
            }
        }

        private static string FormatActorName(string authorName, string authorId)
        {
            if (!String.IsNullOrWhiteSpace(authorName))
            {
                return authorName;
            }

            return "user " + authorId;
        }

        private class IncomingEnvelope
        {
            public string Id;
            public string AuthorId;
            public string AuthorName;
            public string Content;
            public string ChannelId;
            public bool ChannelIsStaff;
        }

        private class OutgoingEnvelope
        {
            public string Id;
            public DateTime CreatedUtc;
            public string ChannelType;
            public string Content;
            public int Attempts;
            public string Source;
        }

        [Usage("DiscordStatus")]
        [Description("Shows current Discord bridge runtime status.")]
        private static void OnDiscordStatusCommand(CommandEventArgs e)
        {
            int incomingCount = SafeCountFiles(IncomingPath);
            int outgoingCount = SafeCountFiles(OutgoingPath);
            int processingCount = SafeCountFiles(ProcessingPath);
            int failedCount = SafeCountFiles(FailedPath);
            int archiveCount = SafeCountFiles(ArchivePath);

            e.Mobile.SendMessage("DiscordBridge enabled: {0}", m_Enabled);
            e.Mobile.SendMessage("Transport mode: local disk queue");
            e.Mobile.SendMessage("Incoming folder: {0}", IncomingPath);
            e.Mobile.SendMessage("Outgoing folder: {0}", OutgoingPath);
            e.Mobile.SendMessage("Queue counts | in:{0} out:{1} proc:{2} failed:{3} arch:{4}", incomingCount, outgoingCount, processingCount, failedCount, archiveCount);
            e.Mobile.SendMessage("Queue oldest | in:{0} out:{1}", GetOldestAgeSummary(IncomingPath), GetOldestAgeSummary(OutgoingPath));
            e.Mobile.SendMessage("Staff users configured: {0}", m_StaffUserIds.Count);
            e.Mobile.SendMessage("Online players now: {0}", GetOnlinePlayerCount());
        }

        public static void WriteStats(TextWriter writer)
        {
            if (writer == null)
            {
                return;
            }

            bool inFlight;
            int queuedCommands;
            long pollsStarted;
            long pollsCompleted;
            long pollsSkipped;
            long pollsTimedOut;
            long pollErrors;
            long totalPollMs;
            long lastPollMs;
            long maxPollMs;
            long drainLastMs;
            long drainMaxMs;
            long commandsQueued;
            long commandsDrained;
            long commandsDropped;
            long lastQueuedCount;
            long lastDrainedCount;
            string lastError;
            DateTime lastSuccessUtc;
            DateTime lastTimeoutUtc;

            lock (m_PollLock)
            {
                inFlight = m_PollInFlight;
                queuedCommands = m_QueuedCommands.Count;
                pollsStarted = m_PollsStarted;
                pollsCompleted = m_PollsCompleted;
                pollsSkipped = m_PollsSkippedInFlight;
                pollsTimedOut = m_PollsTimedOut;
                pollErrors = m_PollErrors;
                totalPollMs = m_TotalPollMs;
                lastPollMs = m_LastPollMs;
                maxPollMs = m_MaxPollMs;
                drainLastMs = m_MainThreadDrainLastMs;
                drainMaxMs = m_MainThreadDrainMaxMs;
                commandsQueued = m_CommandsQueued;
                commandsDrained = m_CommandsDrained;
                commandsDropped = m_CommandsDropped;
                lastQueuedCount = m_LastQueuedCount;
                lastDrainedCount = m_LastDrainedCount;
                lastError = m_LastError;
                lastSuccessUtc = m_LastSuccessUtc;
                lastTimeoutUtc = m_LastTimeoutUtc;
            }

            long avgPollMs = pollsCompleted > 0 ? totalPollMs / pollsCompleted : 0;

            writer.WriteLine("DiscordBridgeStats: enabled={0}, safePolling={1}, inFlight={2}, queuedCommands={3}, pollInterval={4}ms, pollTimeout={5}ms, drainBudget={6}ms, maxCommandsPerTick={7}",
                m_Enabled,
                EnableDiscordBridgeSafePolling,
                inFlight,
                queuedCommands,
                (int)CommandPollInterval.TotalMilliseconds,
                DiscordBridgePollTimeoutMs,
                DiscordBridgeMainThreadDrainBudgetMs,
                DiscordBridgeMaxCommandsPerTick);

            writer.WriteLine("  polls: started={0}, completed={1}, skippedInFlight={2}, timedOut={3}, errors={4}, last={5}ms, max={6}ms, avg={7}ms, lastSuccessUtc={8}, lastTimeoutUtc={9}",
                pollsStarted,
                pollsCompleted,
                pollsSkipped,
                pollsTimedOut,
                pollErrors,
                lastPollMs,
                maxPollMs,
                avgPollMs,
                lastSuccessUtc,
                lastTimeoutUtc);

            writer.WriteLine("  commands: queuedTotal={0}, drainedTotal={1}, dropped={2}, queuedNow={3}, lastWorkerQueued={4}, lastDrainCount={5}, mainThreadDrainLast={6}ms, mainThreadDrainMax={7}ms",
                commandsQueued,
                commandsDrained,
                commandsDropped,
                queuedCommands,
                lastQueuedCount,
                lastDrainedCount,
                drainLastMs,
                drainMaxMs);

            writer.WriteLine("  queues: incoming={0}, outgoing={1}, processing={2}, failed={3}, archive={4}, oldestIncoming={5}, oldestOutgoing={6}",
                SafeCountFiles(IncomingPath),
                SafeCountFiles(OutgoingPath),
                SafeCountFiles(ProcessingPath),
                SafeCountFiles(FailedPath),
                SafeCountFiles(ArchivePath),
                GetOldestAgeSummary(IncomingPath),
                GetOldestAgeSummary(OutgoingPath));

            writer.WriteLine("  lastError={0}", !String.IsNullOrWhiteSpace(lastError) ? lastError : "none");
        }

        public static void GetPerformanceTestSummary(out long timeouts, out long errors)
        {
            lock (m_PollLock)
            {
                timeouts = m_PollsTimedOut;
                errors = m_PollErrors;
            }
        }

        [Usage("DiscordBridgeStats")]
        [Description("Shows Discord bridge polling and queue diagnostics.")]
        private static void OnDiscordBridgeStatsCommand(CommandEventArgs e)
        {
            using (StringWriter writer = new StringWriter())
            {
                WriteStats(writer);

                using (StringReader reader = new StringReader(writer.ToString()))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        e.Mobile.SendMessage(line);
                    }
                }
            }
        }

        [Usage("DiscordReload")]
        [Description("Reloads Discord bridge config from Data/discordbridge.cfg.")]
        private static void OnDiscordReloadCommand(CommandEventArgs e)
        {
            LoadConfig();
            e.Mobile.SendMessage("DiscordBridge config reloaded. Enabled={0}", m_Enabled);
        }
    }
}
