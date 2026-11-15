#if NETFLIGHT_DIAGNOSTICS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Server.Network
{
        [Flags]
        internal enum NetFlightEventFlags : byte
        {
                None = 0x00,
                Inbound = 0x01,
                Outbound = 0x02,
                Integrity = 0x04,
                Movement = 0x08,
                Trigger = 0x10
        }

        public enum NetFlightInfoCode : byte
        {
                None = 0,
                MovementDenied,
                MovementNoChange,
                HandlerException,
                ParseException,
                DecodeException,
                UnknownPacket,
                InvalidLength,
                LengthMismatch,
                Storm,
                Manual
        }

        [Flags]
        internal enum NetFlightTriggerReason
        {
                None = 0x00,
                Backlog = 0x01,
                Stall = 0x02,
                Movement = 0x04,
                Integrity = 0x08,
                PacketStorm = 0x10,
                MovementStuckWhileTrafficContinues = 0x20,
                DecodeRatioSpike = 0x40,
                MovementWedgeAlive = 0x80,
                StormThenWedge = 0x100
        }

        public sealed class NetFlightClientDiagnosticsSnapshot
        {
                public int SessionId;
                public string Account;
                public string Character;
                public string Ip;
                public DateTime ConnectedUtc;
                public int ConnectedSeconds;
                public int PendingBytes;
                public int PeakPendingBytes;
                public int ReceiveQueueBytes;
                public int SendCompletionAgeMs;
                public int EnqueueAgeMs;
                public int InboundAgeMs;
                public int OutboundAgeMs;
                public int InboundPackets1s;
                public int InboundPackets10s;
                public int OutboundPackets1s;
                public int OutboundPackets10s;
                public int InboundUltimaLive10s;
                public int InboundUltimaLive60s;
                public int OutboundUltimaLive10s;
                public int OutboundUltimaLive60s;
                public int OutstandingMovement;
                public int IntegrityErrors;
                public int StallAgeMs;
                public int EventCount;
                public int HealthScore;
                public string Health;
                public string LastDumpReason;
                public int LastDumpAgeMs;
                public bool SnapshotBusy;
        }

        public sealed class NetFlightDiagnosticsSnapshot
        {
                public bool Enabled;
                public int ConnectedSessions;
                public int HealthySessions;
                public int WarningSessions;
                public int CriticalSessions;
                public int RecentDumpCount;
                public int TotalDumps;
                public int AutomaticDumps;
                public int DisconnectDumps;
                public int ManualDumps;
                public int FlushQueueDepth;
                public int FlushDropped;
                public int FlushLagMs;
                public int SnapshotDropped;
                public int LastWriteDurationMs;
                public int LastFormatDurationMs;
                public string LastWriterError;
                public string OutputFolder;
                public NetFlightClientDiagnosticsSnapshot[] Clients;
                public string[] RecentDumpPaths;
        }

        internal struct NetFlightEvent
        {
                public int Tick;
                public short Length;
                public byte PacketId;
                public NetFlightEventFlags Flags;
                public NetFlightInfoCode Info;
        }

        internal sealed class NetFlightSnapshot
        {
                public string Path;
                public int SessionId;
                public string Account;
                public string Character;
                public string Ip;
                public DateTime ConnectedUtc;
                public int Tick;
                public NetFlightTriggerReason Reasons;
                public bool Retriggered;
                public string ManualReason;
                public bool WarmupActive;
                public int PendingBytes;
                public int PeakPending;
                public int StallStartTick;
                public int StallSpikeCount;
                public int LastStallSpikeTick;
                public int PendingDumpTick;
                public NetFlightTriggerReason PendingReasons;
                public int CooldownUntil;

                public int MovementRequests;
                public int MovementDenied;
                public int MovementNoChange;
                public int LastMovementChangeTick;
                public int LastMovementInboundTick;
                public int LastMovementResponseTick;
                public MoveHistory[] Outstanding;
                public int OutstandingCount;
                public int StuckWindowStartTick;
                public int StuckOutboundPackets;
                public int StuckOutboundBytes;
                public int[] StuckTopCounts;
                public int[] StuckTopBytes;

                public int[] OutCounts1;
                public int[] OutBytes1;
                public int[] OutCounts10;
                public int[] OutBytes10;
                public int[] OutCounts60;
                public int[] OutBytes60;
                public int[] InCounts1;
                public int[] InBytes1;
                public int[] InCounts10;
                public int[] InBytes10;
                public int[] InCounts60;
                public int[] InBytes60;

                public int[] MoveReqCounts10;
                public int[] MoveReqCounts60;
                public int[] MoveRespCounts10;
                public int[] MoveRespCounts60;

                public int DecodePreBytes10s;
                public int DecodePostBytes10s;
                public int DecodeCalls10s;
                public int DecodeExceptionCount;

                public byte LastBadPacketId;
                public int LastBadDeclared;
                public int LastBadAvailable;
                public byte[] LastBadPrefix;
                public int LastBadPrefixLength;
                public string LastIntegrityMessage;
                public int IntegrityExceptions;
                public int IntegrityUnknown;
                public int IntegrityInvalidLength;
                public int IntegrityDeclaredLengthMismatch;
                public int IntegrityDecode;
                public IntegrityRecord[] IntegrityRecords;
                public int IntegrityRecordCount;

                public int[] SendCompletions;
                public int[] SendCompletionBytes;
                public int[] SendCompletionsLong;
                public int[] SendCompletionBytesLong;
                public int[] SendEnqueued;
                public int[] SendEnqueuedBytes;
                public int LastSendDrainTick;
                public int LastEnqueueTick;
                public int SendCompletionIndex;
                public int SendCompletionLongIndex;
                public int SendEnqueueIndex;

                public int LastInboundTick;
                public int LastInboundNonKeepaliveTick;
                public int LastOutboundTick;
                public int ReceiveQueueBytes;

                public NetFlightEvent[] Events;
                public int EventCount;

                public BurstSample[] BurstSamples;
                public int BurstSampleCount;

                public int FlushDepth;
                public int FlushDropped;
                public int FlushLagMs;
                public long HotPathContended;
                public long HotPathDropped;
                public long HotPathSampleTicks;
                public long HotPathSampleCount;
                public int SnapshotDropped;
                public int LastWriteDurationMs;
                public int LastFormatDurationMs;
                public string LastWriterError;
                public bool RecordInboundCountsOnly;
                public bool RecordOutboundCountsOnly;
                public bool RecordEventRing;
                public int EventSampleRate;
        }

        internal struct MoveHistory
        {
                public int Tick;
                public byte Direction;
                public byte Sequence;
                public byte ResponseId;
                public bool Responded;
                public int ResponseTick;
        }

        internal struct IntegrityRecord
        {
                public int Tick;
                public byte PacketId;
                public int DeclaredLength;
                public int AvailableLength;
                public byte[] PrefixBytes;
                public int PrefixLength;
                public NetFlightInfoCode Info;
                public string Message;
        }

        internal struct BurstSample
        {
                public byte PacketId;
                public short Length;
                public byte[] Prefix;
                public int PrefixLength;
        }

        internal sealed class PacketRateWindow
        {
                private readonly int _seconds;
                private readonly int[][] _counts;
                private readonly int[][] _bytes;
                private readonly int[] _totals;
                private readonly int[] _byteTotals;
                private int _index;
                private int _secondStamp;

                public PacketRateWindow(int seconds)
                {
                        _seconds = Math.Max(1, seconds);
                        _counts = new int[_seconds][];
                        _bytes = new int[_seconds][];
                        _totals = new int[_seconds];
                        _byteTotals = new int[_seconds];

                        for (int i = 0; i < _seconds; ++i)
                        {
                                _counts[i] = new int[256];
                                _bytes[i] = new int[256];
                        }

                        _secondStamp = (int)(Core.TickCount / 1000);
                }

                private void Advance(int secondStamp)
                {
                        if (secondStamp == _secondStamp)
                                return;

                        int diff = secondStamp - _secondStamp;

                        if (diff < 0)
                                diff = 0;

                        if (diff > _seconds)
                                diff = _seconds;

                        for (int i = 0; i < diff; ++i)
                        {
                                _index = (_index + 1) % _seconds;
                                Array.Clear(_counts[_index], 0, 256);
                                Array.Clear(_bytes[_index], 0, 256);
                                _totals[_index] = 0;
                                _byteTotals[_index] = 0;
                        }

                        _secondStamp = secondStamp;
                }

                private void EnsureCurrent(int tickMs)
                {
                        int stamp = tickMs / 1000;
                        Advance(stamp);
                }

                public void Add(int tickMs, byte packetId, int length)
                {
                        EnsureCurrent(tickMs);

                        _counts[_index][packetId]++;
                        _bytes[_index][packetId] += length;
                        _totals[_index]++;
                        _byteTotals[_index] += length;
                }

                public int GetTotalPackets(int seconds, int tickMs)
                {
                        EnsureCurrent(tickMs);
                        int span = Math.Min(Math.Max(seconds, 1), _seconds);
                        int total = 0;

                        for (int i = 0; i < span; ++i)
                        {
                                int bucket = (_index - i + _seconds) % _seconds;
                                total += _totals[bucket];
                        }

                        return total;
                }

                public int GetTotalBytes(int seconds, int tickMs)
                {
                        EnsureCurrent(tickMs);
                        int span = Math.Min(Math.Max(seconds, 1), _seconds);
                        int total = 0;

                        for (int i = 0; i < span; ++i)
                        {
                                int bucket = (_index - i + _seconds) % _seconds;
                                total += _byteTotals[bucket];
                        }

                        return total;
                }

                public void Aggregate(int seconds, int tickMs, int[] counts, int[] bytes)
                {
                        EnsureCurrent(tickMs);
                        Array.Clear(counts, 0, counts.Length);
                        Array.Clear(bytes, 0, bytes.Length);

                        int span = Math.Min(Math.Max(seconds, 1), _seconds);

                        for (int i = 0; i < span; ++i)
                        {
                                int bucket = (_index - i + _seconds) % _seconds;

                                int[] srcCount = _counts[bucket];
                                int[] srcBytes = _bytes[bucket];

                                for (int j = 0; j < 256; ++j)
                                {
                                        counts[j] += srcCount[j];
                                        bytes[j] += srcBytes[j];
                                }
                        }
                }
        }

        // NOTE: This tool is designed to be non-invasive; all disk I/O and heavy formatting
        // are deferred to a background flusher. Hot-path hooks only mutate simple counters
        // and ring buffers with no allocations.
        public sealed class NetFlightRecorder
        {
                // Unified monotonic time source; all ages are derived from this and clamped in Age().
                private static int Now()
                {
                        return (int)Core.TickCount;
                }

                private static int Age(int now, long then)
                {
                        if (then <= 0)
                                return -1;

                        int delta = now - (int)then;
                        return delta < 0 ? 0 : delta;
                }

                private static void PollAll()
                {
                        NetFlightRecorder[] snapshot;

                        lock (_recordersLock)
                        {
                                if (_recorders.Count == 0)
                                        return;

                                snapshot = _recorders.ToArray();
                        }

                        for (int i = 0; i < snapshot.Length; ++i)
                        {
                                NetFlightRecorder recorder = snapshot[i];
                                if (recorder != null)
                                        recorder.Poll();
                        }
                }

                private const int EventCapacity = 32768;
                private const int RingWindowMs = 30000;
                private const int FlushQueueCapacity = 32;
                private const int FlushQueueHardDrop = 64;
                private const int BacklogThreshold = 256 * 1024;
                private const int BacklogDurationMs = 1000;
                private const int StallDurationMs = 1500;
                private const int StallCorroboratedPending = 64 * 1024;
                private const int PerClientCooldownMs = 120000;
                private const int GlobalCooldownWindowMs = 600000;
                private const int GlobalCooldownCap = 10;
                private const int PacketStormThreshold = 450;
                private const int PacketStormSingleThreshold = 260;
                private const int PacketStormSustainMs = 1000;
                private const int WarmupIgnoreMs = 8000;

                private const int MovementOutstandingCap = 64;
                private const int MovementOutstandingMs = 1500;
                private const int MovementOutstandingBurstMs = 3200;
                private const int MovementStuckTrafficSampleMs = 3000;
                private const int MovementStuckMinRequests = 4;
                private const int MovementAliveKeepaliveId = 0xF0;
                private const int MovementAliveInboundThreshold = 3;
                private const int MovementAliveReqThreshold10s = 3;
                private const int MovementAliveReqThreshold60s = 6;
                private const int MovementAliveLastPosMs = 15000;
                private const int MovementWedgePendingLimit = 64 * 1024;
                private const int MovementWedgeSustainMs = 1000;
                private const int MovementStuckSustainMs = 1000;
                private const int MovementHistoryCap = 32;

                private const int TopWindowLongSeconds = 60;

                private const int BurstSampleDurationMs = 3000;
                private const int BurstSampleHexBytes = 24;
                private const int BurstSampleRate = 8; // 1 out of N packets
                private const int FullCaptureDurationMs = 3000;

                private const int IntegrityRecordCap = 32;

                private const int RecentDumpPathCapacity = 20;

                private const double DecodeRatioSpikeThreshold = 3.5; // post/pre ratio
                private const int DecodeExceptionClusterMs = 10000;
                private const int DecodeExceptionClusterCount = 3;

                private static readonly Queue<long> _globalDumps = new Queue<long>();
                private static readonly object _globalLock = new object();
                private static readonly Queue<NetFlightSnapshot> _flushQueue = new Queue<NetFlightSnapshot>(FlushQueueCapacity);
                private static readonly AutoResetEvent _flushSignal = new AutoResetEvent(false);
                private static readonly Thread _flushThread;
                private static readonly List<NetFlightRecorder> _recorders = new List<NetFlightRecorder>();
                private static readonly object _recordersLock = new object();
                private static readonly Queue<string> _recentDumpPaths = new Queue<string>(RecentDumpPathCapacity);
                private static readonly object _historyLock = new object();
                private static readonly Timer _pollTimer;
                private static volatile bool _enabled = true;
                private static volatile bool _movementTrackingEnabled = true;
                private static volatile bool _integrityEnabled = true;
                private static volatile bool _burstSamplingEnabled = true;
                private static volatile bool _recordInboundCountsOnly;
                private static volatile bool _recordOutboundCountsOnly;
                private static volatile bool _recordEventRing = true;
                private static volatile int _eventSampleRate = 1;
                private static volatile int _hotPathSampleRate = 500;
                private static int _flushDropped;
                private static int _flushLagMs;
                private static int _snapshotDropped;
                private static int _lastWriteDurationMs;
                private static int _lastFormatDurationMs;
                private static string _lastWriterError;
                private static int _nextSessionId;
                private static int _dumpSequence;
                private static int _totalDumps;
                private static int _automaticDumps;
                private static int _disconnectDumps;
                private static int _manualDumps;

                static NetFlightRecorder()
                {
                        _flushThread = new Thread(FlushWorker)
                        {
                                IsBackground = true,
                                Name = "NetFlightFlush"
                        };
                        _flushThread.Start();

                        _pollTimer = Timer.DelayCall(TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250), PollAll);
                        _pollTimer.Priority = TimerPriority.TwoFiftyMS;
                }

                private readonly NetFlightEvent[] _events = new NetFlightEvent[EventCapacity];
                private int _eventCount;
                private int _eventIndex;

                private readonly object _syncRoot = new object();

                private readonly PacketRateWindow _outbound = new PacketRateWindow(10);
                private readonly PacketRateWindow _outboundLong = new PacketRateWindow(TopWindowLongSeconds);
                private readonly PacketRateWindow _inbound = new PacketRateWindow(10);
                private readonly PacketRateWindow _inboundLong = new PacketRateWindow(TopWindowLongSeconds);
                private readonly PacketRateWindow _movementReqWindow = new PacketRateWindow(TopWindowLongSeconds);
                private readonly PacketRateWindow _movementRespWindow = new PacketRateWindow(TopWindowLongSeconds);
                private readonly int[] _scratchCounts = new int[256];
                private readonly int[] _scratchBytes = new int[256];

                private readonly NetState _owner;
                private readonly int _sessionId;
                private readonly DateTime _connectedUtc;
                private bool _detached;

                private int _triggerDirty;
                private int _fullSampleUntil;
                private int _eventSampleCounter;
                private int _hotPathSampleCounter;
                private long _hotPathContended;
                private long _hotPathDropped;
                private long _hotPathSampleTicks;
                private long _hotPathSampleCount;

                private int _pendingBytes;
                private int _peakPending;
                private long _backlogStartTick;
                private long _stallStartTick;
                private long _lastSendDrainTick;
                private long _lastStormTick;
                private long _packetStormStartTick;
                private long _movementWedgeStartTick;
                private long _movementStuckStartTick;
                private int _stallSpikeCount;
                private long _lastStallSpikeTick;
                private long _lastEnqueueTick;

                private readonly MoveHistory[] _moveOutstanding = new MoveHistory[MovementOutstandingCap];
                private int _moveOutstandingCount;
                private int _moveOutstandingIndex;
                private readonly MoveHistory[] _moveHistory = new MoveHistory[MovementHistoryCap];
                private int _moveHistoryCount;
                private int _moveHistoryIndex;
                private int _lastMoveSeq;
                private int _lastMovementResponseTick;
                private int _lastNonMovementOutboundTick;
                private int _lastMovementInboundTick;
                private int _lastInboundTick;
                private int _lastInboundNonKeepaliveTick;
                private int _lastOutboundTick;
                private long _stuckWindowStartTick;
                private readonly int[] _stuckTopCounts = new int[256];
                private readonly int[] _stuckTopBytes = new int[256];
                private int _stuckOutboundBytes;
                private int _stuckOutboundPackets;

                private readonly IntegrityRecord[] _integrityRecords = new IntegrityRecord[IntegrityRecordCap];
                private int _integrityRecordCount;
                private int _integrityRecordIndex;
                private byte _lastBadPacketId;
                private int _lastBadDeclared;
                private int _lastBadAvailable;
                private byte[] _lastBadPrefix;
                private int _lastBadPrefixLength;

                private int _decodePreBytes10s;
                private int _decodePostBytes10s;
                private int _decodeCalls10s;
                private int _decodeWindowStartTick;
                private int _decodeExceptionCount;
                private int _decodeExceptionClusterTick;

                private readonly int[] _sendCompletions = new int[10];
                private readonly int[] _sendCompletionBytes = new int[10];
                private int _sendCompletionIndex;
                private int _sendCompletionStamp;
                private readonly int[] _sendCompletionsLong = new int[60];
                private readonly int[] _sendCompletionBytesLong = new int[60];
                private int _sendCompletionLongStamp;
                private int _sendCompletionLongIndex;
                private readonly int[] _sendEnqueued = new int[60];
                private readonly int[] _sendEnqueuedBytes = new int[60];
                private int _sendEnqueueStamp;
                private int _sendEnqueueIndex;

                private long _cooldownUntil;
                private bool _pendingDump;
                private bool _retriggered;
                private NetFlightTriggerReason _pendingReasons;
                private NetFlightTriggerReason _pendingImmediateReasons;
                private NetFlightTriggerReason _postTriggerReasons;
                private long _postTriggerDumpTick;

                private long _lastMovementTick;
                private long _movementBurstStart;
                private long _lastMovementChangeTick;
                private int _movementRequests;
                private int _movementDenied;
                private int _movementNoChange;

                private int _integrityExceptions;
                private int _integrityUnknown;
                private int _integrityInvalidLength;
                private int _integrityDeclaredLengthMismatch;
                private int _integrityDecode;
                private string _lastIntegrityMessage;

                private long _lastDumpTick;
                private string _lastDumpReason;

                private readonly int _startTick;
                private int _warmupUntil;

                private long _burstSampleUntil;
                private int _burstSampleCounter;
                private readonly List<BurstSample> _burstSamples = new List<BurstSample>(16);

                public NetFlightRecorder(NetState owner)
                {
                        _owner = owner;
                        _sessionId = Interlocked.Increment(ref _nextSessionId);
                        _connectedUtc = DateTime.UtcNow;
                        _startTick = Now();
                        _warmupUntil = _startTick + WarmupIgnoreMs;
                        _lastSendDrainTick = Now();
                        _sendCompletionStamp = Now() / 1000;
                        _sendCompletionLongStamp = _sendCompletionStamp;
                        _sendEnqueueStamp = _sendCompletionStamp;
                        _decodeWindowStartTick = Now();

                        lock (_recordersLock)
                                _recorders.Add(this);
                }

                public void Detach()
                {
                        if (_detached)
                                return;

                        _detached = true;
                        RecordDisconnect("socket-closed");

                        lock (_recordersLock)
                                _recorders.Remove(this);
                }

                public void RecordDisconnect(string reason)
                {
                        if (!_enabled)
                                return;

                        int tick = Now();

                        lock (_syncRoot)
                        {
                                bool establishedSession = Age(tick, _startTick) >= WarmupIgnoreMs;
                                bool diagnosticState = _pendingBytes > 0 || _stallStartTick != 0 || _pendingDump
                                        || _integrityExceptions > 0 || _integrityUnknown > 0 || _integrityInvalidLength > 0
                                        || _integrityDeclaredLengthMismatch > 0 || _integrityDecode > 0;

                                if (!establishedSession && !diagnosticState)
                                        return;

                                WriteDump(tick, NetFlightTriggerReason.None, false, "disconnect-" + (reason ?? "unknown"));
                        }
                }

                // Hot-path hooks use TryEnter to avoid ever blocking network threads.
                private bool TryEnterHotPath(out long sampleStart)
                {
                        if (!Monitor.TryEnter(_syncRoot))
                        {
                                Interlocked.Increment(ref _hotPathContended);
                                Interlocked.Increment(ref _hotPathDropped);
                                sampleStart = 0;
                                return false;
                        }

                        sampleStart = 0;
                        int rate = _hotPathSampleRate;
                        if (rate > 0 && (Interlocked.Increment(ref _hotPathSampleCounter) % rate) == 0)
                                sampleStart = Stopwatch.GetTimestamp();

                        return true;
                }

                private void ExitHotPath(long sampleStart)
                {
                        if (sampleStart != 0)
                        {
                                long elapsed = Stopwatch.GetTimestamp() - sampleStart;
                                Interlocked.Add(ref _hotPathSampleTicks, elapsed);
                                Interlocked.Increment(ref _hotPathSampleCount);
                        }

                        Monitor.Exit(_syncRoot);
                }

                private void MarkDirty()
                {
                        Interlocked.Exchange(ref _triggerDirty, 1);
                }

                public void Poll()
                {
                        if (!_enabled)
                                return;

                        if (Interlocked.Exchange(ref _triggerDirty, 0) == 0 && !_pendingDump && _postTriggerDumpTick == 0)
                                return;

                        if (!Monitor.TryEnter(_syncRoot))
                        {
                                Interlocked.Increment(ref _hotPathContended);
                                return;
                        }

                        try
                        {
                                int tick = Now();

                                CheckPending(tick);
                                CheckCooldownRelease(tick);
                                CheckPostTriggerDump(tick);

                                if (_pendingImmediateReasons != NetFlightTriggerReason.None)
                                {
                                        HandleTrigger(_pendingImmediateReasons, tick);
                                        _pendingImmediateReasons = NetFlightTriggerReason.None;
                                }

                                CheckTriggers(tick);
                        }
                        finally
                        {
                                Monitor.Exit(_syncRoot);
                        }
                }

                private static void CleanupGlobal(long tick)
                {
                        lock (_globalLock)
                        {
                                while (_globalDumps.Count > 0 && Age((int)tick, _globalDumps.Peek()) > GlobalCooldownWindowMs)
                                {
                                        _globalDumps.Dequeue();
                                }
                        }
                }

                private static bool CanPassGlobal(long tick)
                {
                        CleanupGlobal(tick);

                        lock (_globalLock)
                        {
                                if (_globalDumps.Count >= GlobalCooldownCap)
                                        return false;

                                _globalDumps.Enqueue(tick);
                                return true;
                        }
                }

                public void RecordSend(byte packetId, int length, int pendingBytes, byte[] prefix = null)
                {
                        if (!_enabled)
                                return;

                        int tick = Now();

                        long sampleStart;
                        if (!TryEnterHotPath(out sampleStart))
                                return;

                        try
                        {
                                _pendingBytes = pendingBytes;
                                _peakPending = Math.Max(_peakPending, pendingBytes);
                                _lastEnqueueTick = tick;
                                _lastOutboundTick = tick;

                                if (_recordEventRing && !_recordOutboundCountsOnly && ShouldRecordEvent(tick))
                                        AddEvent(tick, packetId, length, NetFlightEventFlags.Outbound, NetFlightInfoCode.None);
                                _outbound.Add(tick, packetId, length);
                                _outboundLong.Add(tick, packetId, length);
                                TrackEnqueue(length, tick);

                                if (packetId == 0x21 || packetId == 0x22 || packetId == 0x23)
                                {
                                        _lastMovementResponseTick = tick;
                                        MarkMovementResponded(packetId);
                                        _movementRespWindow.Add(tick, packetId, length);
                                }
                                else
                                {
                                        _lastNonMovementOutboundTick = tick;
                                }

                                if (_stuckWindowStartTick != 0)
                                {
                                        _stuckOutboundPackets++;
                                        _stuckOutboundBytes += length;
                                        if (packetId != 0x21 && packetId != 0x22 && packetId != 0x23)
                                        {
                                                _stuckTopCounts[packetId]++;
                                                _stuckTopBytes[packetId] += length;
                                        }
                                }

                                TrySampleBurst(packetId, length, prefix);
                                CheckPending(tick);
                        }
                        finally
                        {
                                ExitHotPath(sampleStart);
                        }

                        MarkDirty();
                }

                public void RecordSendCompletion(int bytesSent, int pendingBytes)
                {
                        if (!_enabled)
                                return;

                        int tick = Now();

                        long sampleStart;
                        if (!TryEnterHotPath(out sampleStart))
                                return;

                        try
                        {
                                _pendingBytes = pendingBytes;
                                _peakPending = Math.Max(_peakPending, pendingBytes);
                                _lastSendDrainTick = tick;

                                int stamp = tick / 1000;
                                if (stamp != _sendCompletionStamp)
                                {
                                        int diff = stamp - _sendCompletionStamp;
                                        if (diff < 0)
                                                diff = 0;
                                        if (diff > 10)
                                                diff = 10;
                                        for (int i = 0; i < diff; ++i)
                                        {
                                                _sendCompletionIndex = (_sendCompletionIndex + 1) % 10;
                                                _sendCompletions[_sendCompletionIndex] = 0;
                                                _sendCompletionBytes[_sendCompletionIndex] = 0;
                                        }
                                        _sendCompletionStamp = stamp;
                                }

                                _sendCompletions[_sendCompletionIndex]++;
                                _sendCompletionBytes[_sendCompletionIndex] += Math.Max(0, bytesSent);

                                if (stamp != _sendCompletionLongStamp)
                                {
                                        int diff = stamp - _sendCompletionLongStamp;
                                        if (diff > 60)
                                                diff = 60;
                                        for (int i = 0; i < diff; ++i)
                                        {
                                                _sendCompletionLongIndex = (_sendCompletionLongIndex + 1) % 60;
                                                _sendCompletionsLong[_sendCompletionLongIndex] = 0;
                                                _sendCompletionBytesLong[_sendCompletionLongIndex] = 0;
                                        }
                                        _sendCompletionLongStamp = stamp;
                                }
                                _sendCompletionsLong[_sendCompletionLongIndex]++;
                                _sendCompletionBytesLong[_sendCompletionLongIndex] += Math.Max(0, bytesSent);

                                if (pendingBytes <= 0)
                                        _stallStartTick = 0;

                                CheckPending(tick);
                        }
                        finally
                        {
                                ExitHotPath(sampleStart);
                        }

                        MarkDirty();
                }

                public void RecordReceive(byte packetId, int length)
                {
                        if (!_enabled)
                                return;

                        int tick = Now();

                        long sampleStart;
                        if (!TryEnterHotPath(out sampleStart))
                                return;

                        try
                        {
                                if (_recordEventRing && !_recordInboundCountsOnly && ShouldRecordEvent(tick))
                                        AddEvent(tick, packetId, length, NetFlightEventFlags.Inbound, NetFlightInfoCode.None);
                                _inbound.Add(tick, packetId, length);
                                _inboundLong.Add(tick, packetId, length);
                                _lastInboundTick = tick;
                                if (packetId != MovementAliveKeepaliveId)
                                        _lastInboundNonKeepaliveTick = tick;
                                if (packetId == 0x02)
                                {
                                        _lastMovementInboundTick = tick;
                                }
                        }
                        finally
                        {
                                ExitHotPath(sampleStart);
                        }

                        MarkDirty();
                }

                public void RecordMovementRequest(bool denied, bool moved)
                {
                        if (!_enabled || !_movementTrackingEnabled)
                                return;

                        RecordMovementRequest(denied, moved, 0, 0);
                }

                public void RecordMovementRequest(bool denied, bool moved, byte direction, int seq)
                {
                        if (!_enabled || !_movementTrackingEnabled)
                                return;

                        int tick = Now();

                        long sampleStart;
                        if (!TryEnterHotPath(out sampleStart))
                                return;

                        try
                        {
                                if (_movementBurstStart == 0 || Age(tick, _lastMovementTick) > 1000)
                                {
                                        _movementBurstStart = tick;
                                        _movementRequests = 0;
                                        _movementDenied = 0;
                                        _movementNoChange = 0;
                                }

                                _movementRequests++;
                                _lastMovementTick = tick;
                                _lastMovementInboundTick = tick;
                                _lastMoveSeq = seq;

                                if (denied)
                                        _movementDenied++;

                                if (!moved)
                                {
                                        _movementNoChange++;
                                }
                                else
                                {
                                        _lastMovementChangeTick = tick;
                                }

                                NetFlightInfoCode info = denied ? NetFlightInfoCode.MovementDenied : (moved ? NetFlightInfoCode.None : NetFlightInfoCode.MovementNoChange);
                                if (_recordEventRing && !_recordInboundCountsOnly && ShouldRecordEvent(tick))
                                        AddEvent(tick, 0x02, 0, NetFlightEventFlags.Inbound | NetFlightEventFlags.Movement, info);
                                _movementReqWindow.Add(tick, 0x02, 0);
                                TrackOutstandingMove(tick, direction, (byte)seq);
                                EnsureStuckWindow(tick);
                        }
                        finally
                        {
                                ExitHotPath(sampleStart);
                        }

                        MarkDirty();
                }

                public void RecordIntegrity(NetFlightInfoCode kind, byte packetId, int length, string message, int declaredLen = -1, int availableLen = -1, byte[] prefix = null, int prefixLength = 0)
                {
                        if (!_enabled || !_integrityEnabled)
                                return;

                        int tick = Now();

                        long sampleStart;
                        if (!TryEnterHotPath(out sampleStart))
                                return;

                        try
                        {
                                if (_recordEventRing && !_recordInboundCountsOnly && ShouldRecordEvent(tick))
                                        AddEvent(tick, packetId, (short)length, NetFlightEventFlags.Inbound | NetFlightEventFlags.Integrity, kind);
                                AppendIntegrityRecord(tick, packetId, declaredLen, availableLen, prefix, prefixLength, kind, message);

                                switch (kind)
                                {
                                        case NetFlightInfoCode.HandlerException:
                                        case NetFlightInfoCode.ParseException:
                                                _integrityExceptions++;
                                                break;
                                        case NetFlightInfoCode.UnknownPacket:
                                                _integrityUnknown++;
                                                break;
                                        case NetFlightInfoCode.InvalidLength:
                                                _integrityInvalidLength++;
                                                break;
                                        case NetFlightInfoCode.LengthMismatch:
                                                _integrityDeclaredLengthMismatch++;
                                                break;
                                        case NetFlightInfoCode.DecodeException:
                                                _integrityDecode++;
                                                break;
                                }

                                if (!string.IsNullOrEmpty(message))
                                        _lastIntegrityMessage = message;

                                _pendingImmediateReasons |= NetFlightTriggerReason.Integrity;
                        }
                        finally
                        {
                                ExitHotPath(sampleStart);
                        }

                        MarkDirty();
                }

                public void ForceDump(string reason)
                {
                        if (!_enabled)
                                return;

                        int tick = Now();

                        lock (_syncRoot)
                        {
                                if (!CanPassGlobal(tick))
                                        return;

                                AddEvent(tick, 0x00, 0, NetFlightEventFlags.Trigger, NetFlightInfoCode.Manual);
                                StartBurstSampling(tick);
                                WriteDump(tick, NetFlightTriggerReason.Integrity, true, reason);
                                _cooldownUntil = tick + PerClientCooldownMs;
                        }
                }

                public string DescribeStatus()
                {
                        int tick = Now();
                        int seconds = _cooldownUntil > tick ? (int)((_cooldownUntil - tick) / 1000) : 0;

                        return string.Format("cooldown:{0}s pending:{1} retrigger:{2} outstanding:{3} stuck:{4} lastDumpMsAgo:{5} lastReason:{6}", seconds, _pendingDump, _retriggered, _moveOutstandingCount, _stuckWindowStartTick != 0, Age(tick, _lastDumpTick), _lastDumpReason ?? "none");
                }

                public string DescribeOverhead()
                {
                        long contended = Interlocked.Read(ref _hotPathContended);
                        long dropped = Interlocked.Read(ref _hotPathDropped);
                        long samples = Interlocked.Read(ref _hotPathSampleCount);
                        long ticks = Interlocked.Read(ref _hotPathSampleTicks);
                        double avgUs = samples == 0 ? 0 : (ticks * 1000000.0 / Stopwatch.Frequency / samples);

                        return string.Format("contended:{0} dropped:{1} avgUs:{2:F1}", contended, dropped, avgUs);
                }

                public static int GetRecentDumpCount()
                {
                        long tick = Now();
                        CleanupGlobal(tick);

                        lock (_globalLock)
                                return _globalDumps.Count;
                }

                public int GetOutboundPackets(int seconds)
                {
                        lock (_syncRoot)
                                return _outbound.GetTotalPackets(seconds, Now());
                }

                private void AddEvent(int tick, byte packetId, int length, NetFlightEventFlags flags, NetFlightInfoCode info)
                {
                        _events[_eventIndex].Tick = tick;
                        _events[_eventIndex].PacketId = packetId;
                        _events[_eventIndex].Length = (short)Math.Min(short.MaxValue, Math.Max(short.MinValue, length));
                        _events[_eventIndex].Flags = flags;
                        _events[_eventIndex].Info = info;

                        _eventIndex = (_eventIndex + 1) % EventCapacity;
                        if (_eventCount < EventCapacity)
                                _eventCount++;
                }

                private bool ShouldRecordEvent(int tick)
                {
                        if (!_recordEventRing)
                                return false;

                        if (_fullSampleUntil != 0)
                        {
                                if (Age(tick, _fullSampleUntil) > 0)
                                        _fullSampleUntil = 0;
                                else
                                        return true;
                        }

                        int rate = _eventSampleRate;
                        if (rate <= 1)
                                return true;

                        return (Interlocked.Increment(ref _eventSampleCounter) % rate) == 0;
                }

                private void CheckPending(int tick)
                {
                        if (_pendingBytes >= BacklogThreshold)
                        {
                                if (_backlogStartTick == 0)
                                        _backlogStartTick = tick;
                        }
                        else
                        {
                                _backlogStartTick = 0;
                        }

                        if (_pendingBytes > 0)
                        {
                                if (_stallStartTick == 0)
                                        _stallStartTick = _lastSendDrainTick;
                        }
                        else
                        {
                                if (_stallStartTick != 0)
                                {
                                        _stallSpikeCount++;
                                        _lastStallSpikeTick = tick;
                                }

                                _stallStartTick = 0;
                        }
                }

                private void CheckTriggers(int tick)
                {
                        bool warmup = InWarmup(tick);

                        if (_moveOutstandingCount == 0)
                                _stuckWindowStartTick = 0;

                        NetFlightTriggerReason reasons = NetFlightTriggerReason.None;

                        if (_backlogStartTick != 0 && Age(tick, _backlogStartTick) > BacklogDurationMs)
                                reasons |= NetFlightTriggerReason.Backlog;

                        if (_stallStartTick != 0 && Age(tick, _stallStartTick) > StallDurationMs)
                        {
                                if (IsStallCorroborated(tick))
                                        reasons |= NetFlightTriggerReason.Stall;
                        }

                        if (!warmup && IsMovementAnomaly(tick))
                                reasons |= NetFlightTriggerReason.Movement;

                        if (!warmup)
                        {
                                bool stuck = IsMovementStuck(tick);
                                if (stuck)
                                {
                                        if (_movementStuckStartTick == 0)
                                                _movementStuckStartTick = tick;
                                        if (Age(tick, _movementStuckStartTick) >= MovementStuckSustainMs)
                                                reasons |= NetFlightTriggerReason.MovementStuckWhileTrafficContinues;
                                }
                                else
                                {
                                        _movementStuckStartTick = 0;
                                }

                                bool wedge = IsMovementWedgeAlive(tick);
                                if (wedge)
                                {
                                        if (_movementWedgeStartTick == 0)
                                                _movementWedgeStartTick = tick;
                                        if (Age(tick, _movementWedgeStartTick) >= MovementWedgeSustainMs)
                                                reasons |= NetFlightTriggerReason.MovementWedgeAlive;
                                }
                                else
                                {
                                        _movementWedgeStartTick = 0;
                                }

                                if (IsStormThenWedge(tick))
                                        reasons |= NetFlightTriggerReason.StormThenWedge;

                                bool storm = IsPacketStorm(tick);
                                if (storm)
                                {
                                        if (_packetStormStartTick == 0)
                                                _packetStormStartTick = tick;
                                        if (Age(tick, _packetStormStartTick) >= PacketStormSustainMs)
                                                reasons |= NetFlightTriggerReason.PacketStorm;
                                }
                                else
                                {
                                        _packetStormStartTick = 0;
                                }
                        }

                        if (!warmup && IsDecodeSpike(tick))
                                reasons |= NetFlightTriggerReason.DecodeRatioSpike;

                        if (reasons != NetFlightTriggerReason.None)
                                HandleTrigger(reasons, tick);
                }

                private bool InWarmup(int tick)
                {
                        if (_warmupUntil == 0)
                                return false;

                        if (_lastMovementChangeTick != 0)
                        {
                                _warmupUntil = 0;
                                return false;
                        }

                        if (tick < _warmupUntil)
                                return true;

                        _warmupUntil = 0;
                        return false;
                }

                private bool IsPacketStorm(int tick)
                {
                        int total1 = _outbound.GetTotalPackets(1, tick);
                        int total10 = _outbound.GetTotalPackets(10, tick);

                        if (total1 > PacketStormThreshold)
                        {
                                _lastStormTick = tick;
                                return true;
                        }

                        if (total10 / 10 > PacketStormThreshold)
                        {
                                _lastStormTick = tick;
                                return true;
                        }

                        int[] counts = _scratchCounts;
                        int[] bytes = _scratchBytes;
                        _outbound.Aggregate(1, tick, counts, bytes);

                        for (int i = 0; i < counts.Length; ++i)
                        {
                                if (counts[i] > PacketStormSingleThreshold)
                                {
                                        _lastStormTick = tick;
                                        return true;
                                }
                        }

                        return false;
                }

                private bool IsMovementAnomaly(int tick)
                {
                        if (_movementBurstStart == 0)
                                return false;

                        if (Age(tick, _lastMovementTick) > 750)
                                return false;

                        if (Age(tick, _movementBurstStart) < 3000)
                                return false;

                        bool noChange = _movementNoChange > (_movementRequests / 2) && (Age(tick, _lastMovementChangeTick) > 2500);
                        bool deniedHeavy = _movementDenied >= 3 && _movementDenied >= (_movementRequests / 3);

                        return noChange || deniedHeavy;
                }

                private bool IsMovementStuck(int tick)
                {
                        int oldestTick;
                        int outstanding = GetOutstandingCount(tick, out oldestTick);

                        if (outstanding == 0)
                                return false;

                        int age = Age(tick, oldestTick);
                        if (age < MovementOutstandingMs)
                                return false;

                        bool trafficContinues = _lastNonMovementOutboundTick != 0 && Age(tick, _lastNonMovementOutboundTick) < MovementStuckTrafficSampleMs;
                        if (!trafficContinues)
                                return false;

                        EnsureStuckWindow(tick);
                        return outstanding >= MovementStuckMinRequests || age > MovementOutstandingBurstMs;
                }

                private bool IsMovementWedgeAlive(int tick)
                {
                        int req10 = GetMovementRequests(10, tick);
                        int req60 = GetMovementRequests(60, tick);

                        if (req10 < MovementAliveReqThreshold10s && req60 < MovementAliveReqThreshold60s)
                                return false;

                        bool inboundAlive = _inbound.GetTotalPackets(10, tick) >= MovementAliveInboundThreshold || (_lastInboundTick != 0 && Age(tick, _lastInboundTick) < 10000);
                        bool keepaliveAlive = _lastInboundNonKeepaliveTick != 0 && Age(tick, _lastInboundNonKeepaliveTick) < 20000;

                        if (!inboundAlive && !keepaliveAlive)
                                return false;

                        int lastPosAge = Age(tick, _lastMovementChangeTick);
                        if (lastPosAge >= 0 && lastPosAge < MovementAliveLastPosMs)
                                return false;

                        int denied10 = GetMovementResponses(10, tick, 0x22);
                        int oldestOutstandingTick;
                        bool outstanding = GetOutstandingCount(tick, out oldestOutstandingTick) > 0;

                        if (!outstanding && denied10 == 0)
                                return false;

                        if (_pendingBytes >= MovementWedgePendingLimit)
                                return false;

                        EnsureStuckWindow(tick);
                        return true;
                }

                private bool IsStormThenWedge(int tick)
                {
                        if (_lastStormTick == 0 || Age(tick, _lastStormTick) > 60000)
                                return false;

                        return IsMovementWedgeAlive(tick);
                }

                private bool IsStallCorroborated(int tick)
                {
                        if (_pendingBytes >= StallCorroboratedPending)
                                return true;

                        int send10 = SumLast(_sendCompletions, _sendCompletionIndex, 10);
                        int enq10 = SumLast(_sendEnqueued, _sendEnqueueIndex, 10);

                        if (send10 == 0 && enq10 > 0)
                                return true;

                        return IsMovementWedgeAlive(tick);
                }

                private bool IsDecodeSpike(int tick)
                {
                        if (_decodeCalls10s == 0)
                                return false;

                        double ratio = _decodePreBytes10s == 0 ? 0 : (double)_decodePostBytes10s / Math.Max(1, _decodePreBytes10s);
                        if (ratio > DecodeRatioSpikeThreshold)
                                return true;

                        if (_decodeExceptionClusterTick != 0 && Age(tick, _decodeExceptionClusterTick) < DecodeExceptionClusterMs && _decodeExceptionCount >= DecodeExceptionClusterCount)
                                return true;

                        return false;
                }

                private void HandleTrigger(NetFlightTriggerReason reasons, int tick)
                {
                        StartBurstSampling(tick);
                        _fullSampleUntil = tick + FullCaptureDurationMs;

                        if (_cooldownUntil > tick)
                        {
                                _pendingDump = true;
                                _pendingReasons |= reasons;
                                _retriggered = true;
                                return;
                        }

                        if (!CanPassGlobal(tick))
                        {
                                _pendingDump = true;
                                _pendingReasons |= reasons;
                                _cooldownUntil = Math.Max(_cooldownUntil, tick + PerClientCooldownMs);
                                return;
                        }

                        WriteDump(tick, reasons, _retriggered, null);
                        _postTriggerReasons |= reasons;
                        _postTriggerDumpTick = tick + BurstSampleDurationMs;
                        _retriggered = false;
                        _pendingDump = false;
                        _pendingReasons = NetFlightTriggerReason.None;
                        _cooldownUntil = tick + PerClientCooldownMs;
                }

                private void CheckPostTriggerDump(int tick)
                {
                        if (_postTriggerDumpTick == 0 || tick < _postTriggerDumpTick)
                                return;

                        NetFlightTriggerReason reasons = _postTriggerReasons;
                        _postTriggerDumpTick = 0;
                        _postTriggerReasons = NetFlightTriggerReason.None;

                        if (reasons != NetFlightTriggerReason.None)
                                WriteDump(tick, reasons, true, null);
                }

                private int GetMovementRequests(int seconds, int tick)
                {
                        _movementReqWindow.Aggregate(seconds, tick, _scratchCounts, _scratchBytes);
                        return _scratchCounts[0x02];
                }

                private int GetMovementResponses(int seconds, int tick, byte responseId)
                {
                        _movementRespWindow.Aggregate(seconds, tick, _scratchCounts, _scratchBytes);
                        return _scratchCounts[responseId];
                }

                private int GetOutstandingCount(int tick, out int oldestTick)
                {
                        CompactOutstanding(tick);

                        oldestTick = 0;
                        int count = 0;

                        for (int i = 0; i < _moveOutstandingCount; ++i)
                        {
                                int idx = (_moveOutstandingIndex - i - 1 + MovementOutstandingCap) % MovementOutstandingCap;
                                MoveHistory entry = _moveOutstanding[idx];
                                if (entry.Responded)
                                        continue;

                                count++;
                                oldestTick = entry.Tick;
                        }

                        return count;
                }

                private static int SumLast(int[] values, int index, int count)
                {
                        int total = 0;

                        for (int i = 0; i < count; ++i)
                        {
                                int idx = (index - i + values.Length) % values.Length;
                                total += values[idx];
                        }

                        return total;
                }

                private void TrackOutstandingMove(int tick, byte direction, byte seq)
                {
                        _moveOutstanding[_moveOutstandingIndex].Tick = tick;
                        _moveOutstanding[_moveOutstandingIndex].Direction = direction;
                        _moveOutstanding[_moveOutstandingIndex].Sequence = seq;
                        _moveOutstanding[_moveOutstandingIndex].Responded = false;
                        _moveOutstanding[_moveOutstandingIndex].ResponseId = 0;
                        _moveOutstanding[_moveOutstandingIndex].ResponseTick = 0;

                        _moveOutstandingIndex = (_moveOutstandingIndex + 1) % MovementOutstandingCap;
                        if (_moveOutstandingCount < MovementOutstandingCap)
                                _moveOutstandingCount++;

                        CompactOutstanding(tick);

                        _moveHistory[_moveHistoryIndex] = _moveOutstanding[_moveOutstandingIndex == 0 ? MovementOutstandingCap - 1 : _moveOutstandingIndex - 1];
                        _moveHistoryIndex = (_moveHistoryIndex + 1) % MovementHistoryCap;
                        if (_moveHistoryCount < MovementHistoryCap)
                                _moveHistoryCount++;
                }

                private void EnsureStuckWindow(int tick)
                {
                        if (_stuckWindowStartTick == 0)
                        {
                                _stuckWindowStartTick = tick;
                                Array.Clear(_stuckTopCounts, 0, _stuckTopCounts.Length);
                                Array.Clear(_stuckTopBytes, 0, _stuckTopBytes.Length);
                                _stuckOutboundBytes = 0;
                                _stuckOutboundPackets = 0;
                        }
                }

                private void MarkMovementResponded(byte responseId)
                {
                        int now = Now();
                        int count = _moveOutstandingCount;
                        for (int i = 0; i < count; ++i)
                        {
                                int idx = (_moveOutstandingIndex - i - 1 + MovementOutstandingCap) % MovementOutstandingCap;
                                if (!_moveOutstanding[idx].Responded)
                                {
                                        _moveOutstanding[idx].Responded = true;
                                        _moveOutstanding[idx].ResponseId = responseId;
                                        _moveOutstanding[idx].ResponseTick = now;
                                        break;
                                }
                        }

                        for (int i = 0; i < _moveHistoryCount; ++i)
                        {
                                int idx = (_moveHistoryIndex - i - 1 + MovementHistoryCap) % MovementHistoryCap;
                                if (!_moveHistory[idx].Responded)
                                {
                                        _moveHistory[idx].Responded = true;
                                        _moveHistory[idx].ResponseId = responseId;
                                        _moveHistory[idx].ResponseTick = now;
                                        break;
                                }
                        }

                        CompactOutstanding(now);
                }

                private void CompactOutstanding(int tick)
                {
                        while (_moveOutstandingCount > 0)
                        {
                                int oldestIndex = (_moveOutstandingIndex - _moveOutstandingCount + MovementOutstandingCap) % MovementOutstandingCap;
                                MoveHistory oldest = _moveOutstanding[oldestIndex];

                                if ((oldest.Responded && oldest.ResponseTick != 0) || Age(tick, oldest.Tick) > MovementOutstandingBurstMs)
                                {
                                        _moveOutstandingCount--;
                                }
                                else
                                {
                                        break;
                                }
                        }
                }

                private void AppendIntegrityRecord(int tick, byte packetId, int declaredLen, int availableLen, byte[] prefixBytes, int prefixLength, NetFlightInfoCode info, string message)
                {
                        _lastBadPacketId = packetId;
                        _lastBadDeclared = declaredLen;
                        _lastBadAvailable = availableLen;
                        if (prefixBytes != null && prefixLength > 0)
                        {
                                byte[] copy = new byte[prefixLength];
                                Buffer.BlockCopy(prefixBytes, 0, copy, 0, prefixLength);
                                _lastBadPrefix = copy;
                                _lastBadPrefixLength = prefixLength;
                                prefixBytes = copy;
                        }

                        _integrityRecords[_integrityRecordIndex].Tick = tick;
                        _integrityRecords[_integrityRecordIndex].PacketId = packetId;
                        _integrityRecords[_integrityRecordIndex].DeclaredLength = declaredLen;
                        _integrityRecords[_integrityRecordIndex].AvailableLength = availableLen;
                        _integrityRecords[_integrityRecordIndex].PrefixBytes = prefixBytes;
                        _integrityRecords[_integrityRecordIndex].PrefixLength = prefixLength;
                        _integrityRecords[_integrityRecordIndex].Info = info;
                        _integrityRecords[_integrityRecordIndex].Message = message;

                        _integrityRecordIndex = (_integrityRecordIndex + 1) % IntegrityRecordCap;
                        if (_integrityRecordCount < IntegrityRecordCap)
                                _integrityRecordCount++;
                }

                public void RecordDecode(int preBytes, int postBytes, bool compressionEnabled, bool success, byte[] prefix, string message)
                {
                        if (!_enabled)
                                return;

                        int tick = Now();

                        long sampleStart;
                        if (!TryEnterHotPath(out sampleStart))
                                return;

                        try
                        {
                                if (Age(tick, _decodeWindowStartTick) > 10000)
                                {
                                        _decodeWindowStartTick = tick;
                                        _decodeCalls10s = 0;
                                        _decodePreBytes10s = 0;
                                        _decodePostBytes10s = 0;
                                }

                                _decodeCalls10s++;
                                _decodePreBytes10s += preBytes;
                                _decodePostBytes10s += postBytes;

                                if (!success)
                                {
                                        _decodeExceptionCount++;
                                        if (_decodeExceptionClusterTick == 0)
                                                _decodeExceptionClusterTick = tick;
                                }

                                if (!success && prefix != null && prefix.Length > 0)
                                {
                                        int take = Math.Min(prefix.Length, BurstSampleHexBytes);
                                        AppendIntegrityRecord(tick, prefix[0], preBytes, postBytes, prefix, take, NetFlightInfoCode.DecodeException, message);
                                }
                        }
                        finally
                        {
                                ExitHotPath(sampleStart);
                        }

                        MarkDirty();
                }

                private void TrackEnqueue(int length, int tick)
                {
                        int stamp = tick / 1000;
                        if (stamp != _sendEnqueueStamp)
                        {
                                int diff = stamp - _sendEnqueueStamp;
                                if (diff > 60)
                                        diff = 60;
                                for (int i = 0; i < diff; ++i)
                                {
                                        _sendEnqueueIndex = (_sendEnqueueIndex + 1) % 60;
                                        _sendEnqueued[_sendEnqueueIndex] = 0;
                                        _sendEnqueuedBytes[_sendEnqueueIndex] = 0;
                                }
                                _sendEnqueueStamp = stamp;
                        }
                        _sendEnqueued[_sendEnqueueIndex]++;
                        _sendEnqueuedBytes[_sendEnqueueIndex] += Math.Max(0, length);
                }

                private void StartBurstSampling(int tick)
                {
                        if (!_burstSamplingEnabled)
                        {
                                _burstSampleUntil = 0;
                                _burstSamples.Clear();
                                return;
                        }

                        _burstSampleCounter = 0;
                        _burstSamples.Clear();
                        _burstSampleUntil = tick + BurstSampleDurationMs;
                }

                private void TrySampleBurst(byte packetId, int length, byte[] prefix)
                {
                        if (!_burstSamplingEnabled)
                                return;

                        int now = Now();
                        if (_burstSampleUntil == 0 || now > _burstSampleUntil)
                                return;

                        _burstSampleCounter++;
                        if (_burstSampleCounter % BurstSampleRate != 0)
                                return;

                        BurstSample sample = new BurstSample
                        {
                                PacketId = packetId,
                                Length = (short)length,
                                Prefix = null,
                                PrefixLength = 0
                        };

                        if (prefix != null && prefix.Length > 0)
                        {
                                int take = Math.Min(prefix.Length, BurstSampleHexBytes);
                                sample.Prefix = new byte[take];
                                sample.PrefixLength = take;
                                Buffer.BlockCopy(prefix, 0, sample.Prefix, 0, take);
                        }

                        _burstSamples.Add(sample);
                }

                private void CheckCooldownRelease(int tick)
                {
                        if (_pendingDump && _cooldownUntil != 0 && tick >= _cooldownUntil)
                        {
                                if (!CanPassGlobal(tick))
                                        return;

                                WriteDump(tick, _pendingReasons, true, null);
                                _pendingDump = false;
                                _retriggered = false;
                                _pendingReasons = NetFlightTriggerReason.None;
                                _cooldownUntil = tick + PerClientCooldownMs;
                        }
                }

                private void WriteDump(int tick, NetFlightTriggerReason reasons, bool retriggered, string manualReason)
                {
                        if (!_enabled)
                                return;

                        NetFlightSnapshot snapshot = BuildSnapshot(tick, reasons, retriggered, manualReason);
                        if (snapshot == null)
                                return;

                        if (!EnqueueFlush(snapshot))
                                return;

                        _lastDumpTick = tick;
                        _lastDumpReason = manualReason ?? reasons.ToString();

                        Interlocked.Increment(ref _totalDumps);

                        if (!string.IsNullOrEmpty(manualReason) && manualReason.StartsWith("disconnect-", StringComparison.OrdinalIgnoreCase))
                                Interlocked.Increment(ref _disconnectDumps);
                        else if (!string.IsNullOrEmpty(manualReason))
                                Interlocked.Increment(ref _manualDumps);
                        else
                                Interlocked.Increment(ref _automaticDumps);
                }

                private NetFlightSnapshot BuildSnapshot(int tick, NetFlightTriggerReason reasons, bool retriggered, string manualReason)
                {
                        NetFlightSnapshot snap = new NetFlightSnapshot();

                        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff'Z'");
                        string account = _owner.Account != null ? _owner.Account.Username : "none";
                        string name = _owner.Mobile != null ? _owner.Mobile.RawName : "none";
                        string ip = _owner.Address != null ? _owner.Address.ToString() : "0.0.0.0";
                        string reasonText = manualReason ?? reasons.ToString();

                        string safeAccount = SafePath(account);
                        string safeName = SafePath(name);
                        string safeReason = SafePath(reasonText);
                        string safeIp = SafePath(ip);

                        string directory = Path.Combine("Logs", "NetFlight");
                        int dumpSequence = Interlocked.Increment(ref _dumpSequence);
                        string fileName = string.Format("{0}_sid-{1}_seq-{2}_acct-{3}_char-{4}_ip-{5}_reason-{6}.log", timestamp, _sessionId, dumpSequence, safeAccount, safeName, safeIp, safeReason);

                        snap.Path = Path.Combine(directory, fileName);
                        snap.SessionId = _sessionId;
                        snap.Account = account;
                        snap.Character = name;
                        snap.Ip = ip;
                        snap.ConnectedUtc = _connectedUtc;
                        snap.Tick = tick;
                        snap.Reasons = reasons;
                        snap.Retriggered = retriggered || _retriggered;
                        snap.ManualReason = manualReason;
                        snap.WarmupActive = InWarmup(tick);
                        snap.PendingBytes = _pendingBytes;
                        snap.PeakPending = _peakPending;
                        snap.StallStartTick = (int)_stallStartTick;
                        snap.StallSpikeCount = _stallSpikeCount;
                        snap.LastStallSpikeTick = (int)_lastStallSpikeTick;
                        snap.PendingDumpTick = _pendingDump ? tick : 0;
                        snap.PendingReasons = _pendingReasons;
                        snap.CooldownUntil = (int)_cooldownUntil;

                        snap.MovementRequests = _movementRequests;
                        snap.MovementDenied = _movementDenied;
                        snap.MovementNoChange = _movementNoChange;
                        snap.LastMovementChangeTick = (int)_lastMovementChangeTick;
                        snap.LastMovementInboundTick = _lastMovementInboundTick;
                        snap.LastMovementResponseTick = _lastMovementResponseTick;
                        snap.StuckWindowStartTick = (int)_stuckWindowStartTick;
                        snap.StuckOutboundPackets = _stuckOutboundPackets;
                        snap.StuckOutboundBytes = _stuckOutboundBytes;

                        snap.StuckTopCounts = new int[256];
                        snap.StuckTopBytes = new int[256];
                        Array.Copy(_stuckTopCounts, snap.StuckTopCounts, 256);
                        Array.Copy(_stuckTopBytes, snap.StuckTopBytes, 256);

                        snap.OutCounts1 = new int[256];
                        snap.OutBytes1 = new int[256];
                        snap.OutCounts10 = new int[256];
                        snap.OutBytes10 = new int[256];
                        snap.OutCounts60 = new int[256];
                        snap.OutBytes60 = new int[256];
                        snap.InCounts1 = new int[256];
                        snap.InBytes1 = new int[256];
                        snap.InCounts10 = new int[256];
                        snap.InBytes10 = new int[256];
                        snap.InCounts60 = new int[256];
                        snap.InBytes60 = new int[256];

                        _outbound.Aggregate(1, tick, snap.OutCounts1, snap.OutBytes1);
                        _outbound.Aggregate(10, tick, snap.OutCounts10, snap.OutBytes10);
                        _outboundLong.Aggregate(TopWindowLongSeconds, tick, snap.OutCounts60, snap.OutBytes60);
                        _inbound.Aggregate(1, tick, snap.InCounts1, snap.InBytes1);
                        _inbound.Aggregate(10, tick, snap.InCounts10, snap.InBytes10);
                        _inboundLong.Aggregate(TopWindowLongSeconds, tick, snap.InCounts60, snap.InBytes60);

                        snap.MoveReqCounts10 = new int[256];
                        snap.MoveReqCounts60 = new int[256];
                        snap.MoveRespCounts10 = new int[256];
                        snap.MoveRespCounts60 = new int[256];
                        _movementReqWindow.Aggregate(10, tick, snap.MoveReqCounts10, new int[256]);
                        _movementReqWindow.Aggregate(60, tick, snap.MoveReqCounts60, new int[256]);
                        _movementRespWindow.Aggregate(10, tick, snap.MoveRespCounts10, new int[256]);
                        _movementRespWindow.Aggregate(60, tick, snap.MoveRespCounts60, new int[256]);

                        snap.OutstandingCount = _moveOutstandingCount;
                        snap.Outstanding = new MoveHistory[_moveOutstandingCount];
                        for (int i = 0; i < _moveOutstandingCount; ++i)
                        {
                                int idx = (_moveOutstandingIndex - _moveOutstandingCount + i + MovementOutstandingCap) % MovementOutstandingCap;
                                snap.Outstanding[i] = _moveOutstanding[idx];
                        }

                        snap.DecodePreBytes10s = _decodePreBytes10s;
                        snap.DecodePostBytes10s = _decodePostBytes10s;
                        snap.DecodeCalls10s = _decodeCalls10s;
                        snap.DecodeExceptionCount = _decodeExceptionCount;

                        snap.LastBadPacketId = _lastBadPacketId;
                        snap.LastBadDeclared = _lastBadDeclared;
                        snap.LastBadAvailable = _lastBadAvailable;
                        snap.LastBadPrefix = _lastBadPrefix;
                        snap.LastBadPrefixLength = _lastBadPrefixLength;
                        snap.LastIntegrityMessage = _lastIntegrityMessage;
                        snap.IntegrityExceptions = _integrityExceptions;
                        snap.IntegrityUnknown = _integrityUnknown;
                        snap.IntegrityInvalidLength = _integrityInvalidLength;
                        snap.IntegrityDeclaredLengthMismatch = _integrityDeclaredLengthMismatch;
                        snap.IntegrityDecode = _integrityDecode;

                        snap.IntegrityRecordCount = _integrityRecordCount;
                        snap.IntegrityRecords = new IntegrityRecord[_integrityRecordCount];
                        for (int i = 0; i < _integrityRecordCount; ++i)
                        {
                                int idx = (_integrityRecordIndex - _integrityRecordCount + i + IntegrityRecordCap) % IntegrityRecordCap;
                                snap.IntegrityRecords[i] = _integrityRecords[idx];
                        }

                        snap.SendCompletions = new int[10];
                        snap.SendCompletionBytes = new int[10];
                        snap.SendCompletionsLong = new int[60];
                        snap.SendCompletionBytesLong = new int[60];
                        snap.SendEnqueued = new int[60];
                        snap.SendEnqueuedBytes = new int[60];
                        Array.Copy(_sendCompletions, snap.SendCompletions, 10);
                        Array.Copy(_sendCompletionBytes, snap.SendCompletionBytes, 10);
                        Array.Copy(_sendCompletionsLong, snap.SendCompletionsLong, 60);
                        Array.Copy(_sendCompletionBytesLong, snap.SendCompletionBytesLong, 60);
                        Array.Copy(_sendEnqueued, snap.SendEnqueued, 60);
                        Array.Copy(_sendEnqueuedBytes, snap.SendEnqueuedBytes, 60);
                        snap.LastSendDrainTick = (int)_lastSendDrainTick;
                        snap.LastEnqueueTick = (int)_lastEnqueueTick;
                        snap.SendCompletionIndex = _sendCompletionIndex;
                        snap.SendCompletionLongIndex = _sendCompletionLongIndex;
                        snap.SendEnqueueIndex = _sendEnqueueIndex;

                        snap.LastInboundTick = _lastInboundTick;
                        snap.LastInboundNonKeepaliveTick = _lastInboundNonKeepaliveTick;
                        snap.LastOutboundTick = _lastOutboundTick;
                        snap.ReceiveQueueBytes = GetReceiveQueueBytes();

                        int eventStart = _eventCount == EventCapacity ? _eventIndex : 0;
                        int eventCopyCount = 0;

                        for (int i = 0; i < _eventCount; ++i)
                        {
                                int slot = (eventStart + i) % EventCapacity;
                                int eventAge = Age(tick, _events[slot].Tick);

                                if (eventAge >= 0 && eventAge <= RingWindowMs)
                                        eventCopyCount++;
                        }

                        snap.EventCount = eventCopyCount;
                        snap.Events = new NetFlightEvent[eventCopyCount];
                        int eventCopyIndex = 0;

                        for (int i = 0; i < _eventCount; ++i)
                        {
                                int slot = (eventStart + i) % EventCapacity;
                                int eventAge = Age(tick, _events[slot].Tick);

                                if (eventAge >= 0 && eventAge <= RingWindowMs)
                                        snap.Events[eventCopyIndex++] = _events[slot];
                        }

                        snap.BurstSampleCount = _burstSamples.Count;
                        if (snap.BurstSampleCount > 0)
                        {
                                snap.BurstSamples = new BurstSample[snap.BurstSampleCount];
                                for (int i = 0; i < snap.BurstSampleCount; ++i)
                                        snap.BurstSamples[i] = _burstSamples[i];
                        }

                        lock (_flushQueue)
                        {
                                snap.FlushDepth = _flushQueue.Count;
                                snap.FlushDropped = _flushDropped;
                                snap.FlushLagMs = _flushLagMs;
                        }

                        snap.HotPathContended = Interlocked.Read(ref _hotPathContended);
                        snap.HotPathDropped = Interlocked.Read(ref _hotPathDropped);
                        snap.HotPathSampleCount = Interlocked.Read(ref _hotPathSampleCount);
                        snap.HotPathSampleTicks = Interlocked.Read(ref _hotPathSampleTicks);
                        snap.SnapshotDropped = _snapshotDropped;
                        snap.LastWriteDurationMs = _lastWriteDurationMs;
                        snap.LastFormatDurationMs = _lastFormatDurationMs;
                        snap.LastWriterError = _lastWriterError;
                        snap.RecordInboundCountsOnly = _recordInboundCountsOnly;
                        snap.RecordOutboundCountsOnly = _recordOutboundCountsOnly;
                        snap.RecordEventRing = _recordEventRing;
                        snap.EventSampleRate = _eventSampleRate;

                        return snap;
                }

                private static bool EnqueueFlush(NetFlightSnapshot snapshot)
                {
                        lock (_flushQueue)
                        {
                                if (_flushQueue.Count >= FlushQueueHardDrop)
                                {
                                        _snapshotDropped++;
                                        return false;
                                }

                                if (_flushQueue.Count >= FlushQueueCapacity)
                                {
                                        _flushQueue.Dequeue();
                                        _snapshotDropped++;
                                }

                                _flushQueue.Enqueue(snapshot);
                        }

                        lock (_historyLock)
                        {
                                while (_recentDumpPaths.Count >= RecentDumpPathCapacity)
                                        _recentDumpPaths.Dequeue();

                                _recentDumpPaths.Enqueue(GetFullPath(snapshot.Path));
                        }

                        _flushSignal.Set();
                        return true;
                }

                private static string FormatSnapshot(NetFlightSnapshot snap)
                {
                        StringBuilder builder = new StringBuilder(4096);
                        using (StringWriter writer = new StringWriter(builder))
                        {
                                WriteHeaderSnapshot(writer, snap);
                                WriteBodySnapshot(writer, snap);
                        }

                        return builder.ToString();
                }

                private static int SumCounts(int[] counts)
                {
                        int total = 0;
                        for (int i = 0; i < counts.Length; ++i)
                                total += counts[i];
                        return total;
                }

                private static int SumBytes(int[] bytes)
                {
                        int total = 0;
                        for (int i = 0; i < bytes.Length; ++i)
                                total += bytes[i];
                        return total;
                }

                private static void WriteHeaderSnapshot(TextWriter writer, NetFlightSnapshot snap)
                {
                        writer.WriteLine("# NetFlightRecorder Dump");
                        writer.WriteLine("timestampUtc={0}", DateTime.UtcNow.ToString("O"));
                        writer.WriteLine("sessionId={0}", snap.SessionId);
                        writer.WriteLine("connectedUtc={0:O}", snap.ConnectedUtc);
                        writer.WriteLine("account={0}", snap.Account ?? "(none)");
                        writer.WriteLine("character={0}", snap.Character ?? "(none)");
                        writer.WriteLine("ip={0}", snap.Ip ?? "(none)");
                        writer.WriteLine("reason={0}", snap.ManualReason ?? snap.Reasons.ToString());
                        writer.WriteLine("retriggered={0}", snap.Retriggered);

                        writer.WriteLine("health.enabled={0}", _enabled);
                        writer.WriteLine("health.flushQueueDepth={0}", snap.FlushDepth);
                        writer.WriteLine("health.flushDropped={0}", snap.FlushDropped);
                        writer.WriteLine("health.flushLagMs={0}", snap.FlushLagMs);
                        writer.WriteLine("health.warmupActive={0}", snap.WarmupActive);
                        writer.WriteLine("health.snapshotDropped={0}", snap.SnapshotDropped);
                        writer.WriteLine("health.lastWriterError={0}", string.IsNullOrEmpty(snap.LastWriterError) ? "none" : snap.LastWriterError);
                        writer.WriteLine("health.lastWriteDurationMs={0}", snap.LastWriteDurationMs);
                        writer.WriteLine("health.lastFormatDurationMs={0}", snap.LastFormatDurationMs);

                        double avgUs = snap.HotPathSampleCount == 0 ? 0 : (snap.HotPathSampleTicks * 1000000.0 / Stopwatch.Frequency / snap.HotPathSampleCount);
                        writer.WriteLine("health.hotPathContended={0}", snap.HotPathContended);
                        writer.WriteLine("health.hotPathDropped={0}", snap.HotPathDropped);
                        writer.WriteLine("health.hotPathAvgUs={0:F2}", avgUs);
                        writer.WriteLine("health.recordEventRing={0}", snap.RecordEventRing);
                        writer.WriteLine("health.recordInboundCountsOnly={0}", snap.RecordInboundCountsOnly);
                        writer.WriteLine("health.recordOutboundCountsOnly={0}", snap.RecordOutboundCountsOnly);
                        writer.WriteLine("health.eventSampleRate={0}", snap.EventSampleRate);

                        int moveReq10 = snap.MoveReqCounts10[0x02];
                        int moveReq60 = snap.MoveReqCounts60[0x02];
                        int moveResp10 = snap.MoveRespCounts10[0x21] + snap.MoveRespCounts10[0x22] + snap.MoveRespCounts10[0x23];
                        int moveResp60 = snap.MoveRespCounts60[0x21] + snap.MoveRespCounts60[0x22] + snap.MoveRespCounts60[0x23];

                        string topOut10s = TopToString(snap.OutCounts10, snap.OutBytes10, 3);

                        writer.WriteLine("SIG reason={0} stall={1:F3} pending={2} in10sPkts={3} out10sPkts={4} out10sBytes={5} moveReq10s={6} moveResp10s={7} deniedStreak={8} noChangeStreak={9} lastPosChangeMs={10} integrityErrs={11} topOut10s={12}", snap.ManualReason ?? snap.Reasons.ToString(), snap.StallStartTick == 0 ? 0 : Age(snap.Tick, snap.StallStartTick) / 1000.0, snap.PendingBytes, SumCounts(snap.InCounts10), SumCounts(snap.OutCounts10), SumBytes(snap.OutBytes10), moveReq10, moveResp10, snap.MovementDenied, snap.MovementNoChange, Age(snap.Tick, snap.LastMovementChangeTick), snap.IntegrityExceptions + snap.IntegrityInvalidLength + snap.IntegrityDeclaredLengthMismatch + snap.IntegrityUnknown + snap.IntegrityDecode, topOut10s);
                        writer.WriteLine("pendingBytes={0}", snap.PendingBytes);
                        writer.WriteLine("peakPendingBytes={0}", snap.PeakPending);
                        writer.WriteLine("receiveQueueBytes={0}", snap.ReceiveQueueBytes);

                        double stallSeconds = 0;
                        if (snap.StallStartTick != 0)
                                stallSeconds = Age(snap.Tick, snap.StallStartTick) / 1000.0;

                        writer.WriteLine("stallSeconds={0:F3}", stallSeconds);
                        writer.WriteLine("stallSpikes={0}", snap.StallSpikeCount);

                        writer.WriteLine("outbound.packets1s={0}", SumCounts(snap.OutCounts1));
                        writer.WriteLine("outbound.packets10s={0}", SumCounts(snap.OutCounts10));
                        writer.WriteLine("outbound.bytes1s={0}", SumBytes(snap.OutBytes1));
                        writer.WriteLine("outbound.bytes10s={0}", SumBytes(snap.OutBytes10));
                        writer.WriteLine("inbound.packets1s={0}", SumCounts(snap.InCounts1));
                        writer.WriteLine("inbound.packets10s={0}", SumCounts(snap.InCounts10));
                        writer.WriteLine("inbound.bytes1s={0}", SumBytes(snap.InBytes1));
                        writer.WriteLine("inbound.bytes10s={0}", SumBytes(snap.InBytes10));

                        writer.WriteLine("movement.requests={0}", snap.MovementRequests);
                        writer.WriteLine("movement.denied={0}", snap.MovementDenied);
                        writer.WriteLine("movement.noChange={0}", snap.MovementNoChange);
                        writer.WriteLine("movement.lastChangeMsAgo={0}", Age(snap.Tick, snap.LastMovementChangeTick));
                        writer.WriteLine("movement.req10s={0}", moveReq10);
                        writer.WriteLine("movement.req60s={0}", moveReq60);
                        writer.WriteLine("movement.resp10s={0}", moveResp10);
                        writer.WriteLine("movement.resp60s={0}", moveResp60);
                        writer.WriteLine("movement.outstanding={0}", snap.OutstandingCount);
                        writer.WriteLine("movement.lastReqMsAgo={0}", Age(snap.Tick, snap.LastMovementInboundTick));
                        writer.WriteLine("movement.lastRespMsAgo={0}", Age(snap.Tick, snap.LastMovementResponseTick));

                        writer.WriteLine("integrity.exceptions={0}", snap.IntegrityExceptions);
                        writer.WriteLine("integrity.unknownPackets={0}", snap.IntegrityUnknown);
                        writer.WriteLine("integrity.invalidLength={0}", snap.IntegrityInvalidLength);
                        writer.WriteLine("integrity.lengthMismatch={0}", snap.IntegrityDeclaredLengthMismatch);
                        writer.WriteLine("integrity.decode={0}", snap.IntegrityDecode);

                        if (!string.IsNullOrEmpty(snap.LastIntegrityMessage))
                                writer.WriteLine("integrity.lastMessage={0}", snap.LastIntegrityMessage);

                        WriteTopPackets(writer, "outbound.top1s", snap.OutCounts1, snap.OutBytes1, 3);
                        WriteTopPackets(writer, "outbound.top10s", snap.OutCounts10, snap.OutBytes10, 3);
                        WriteTopPackets(writer, "outbound.top60s", snap.OutCounts60, snap.OutBytes60, 5);
                        WriteTopPackets(writer, "inbound.top1s", snap.InCounts1, snap.InBytes1, 3);
                        WriteTopPackets(writer, "inbound.top10s", snap.InCounts10, snap.InBytes10, 3);
                        WriteTopPackets(writer, "inbound.top60s", snap.InCounts60, snap.InBytes60, 5);

                        writer.WriteLine();
                        writer.WriteLine("# MovementCorrelation");
                        writer.WriteLine("movement.outstanding={0}", snap.OutstandingCount);
                        writer.WriteLine("movement.oldestOutstandingMs={0}", snap.OutstandingCount == 0 ? 0 : Age(snap.Tick, snap.Outstanding[0].Tick));
                        writer.WriteLine("movement.lastMoveReqMsAgo={0}", Age(snap.Tick, snap.LastMovementInboundTick));
                        writer.WriteLine("movement.lastMoveRespMsAgo={0}", Age(snap.Tick, snap.LastMovementResponseTick));

                        int outstandingSlots = Math.Min(snap.OutstandingCount, 8);
                        for (int i = 0; i < outstandingSlots; ++i)
                        {
                                MoveHistory mv = snap.Outstanding[i];
                                writer.WriteLine("movement.outstanding[{0}]=ageMs:{1} dir:{2} seq:{3} responded:{4} respId:{5}", i, Age(snap.Tick, mv.Tick), mv.Direction, mv.Sequence, mv.Responded, mv.ResponseId);
                        }

                        writer.WriteLine();
                        writer.WriteLine("# TrafficDuringStuck");
                        writer.WriteLine("stuck.active={0}", snap.StuckWindowStartTick != 0);
                        writer.WriteLine("stuck.durationMs={0}", snap.StuckWindowStartTick == 0 ? 0 : Age(snap.Tick, snap.StuckWindowStartTick));
                        writer.WriteLine("stuck.outPackets={0}", snap.StuckOutboundPackets);
                        writer.WriteLine("stuck.outBytes={0}", snap.StuckOutboundBytes);
                        WriteTopPackets(writer, "stuck.top", snap.StuckTopCounts, snap.StuckTopBytes, 5);

                        writer.WriteLine();
                        writer.WriteLine("# DecodeStats");
                        writer.WriteLine("decode.preBytes10s={0}", snap.DecodePreBytes10s);
                        writer.WriteLine("decode.postBytes10s={0}", snap.DecodePostBytes10s);
                        writer.WriteLine("decode.calls10s={0}", snap.DecodeCalls10s);
                        double ratio = snap.DecodePreBytes10s == 0 ? 0 : (double)snap.DecodePostBytes10s / Math.Max(1, snap.DecodePreBytes10s);
                        writer.WriteLine("decode.ratio10s={0:F3}", ratio);
                        writer.WriteLine("decode.exceptionCount10m={0}", snap.DecodeExceptionCount);

                        writer.WriteLine();
                        writer.WriteLine("# IntegrityAnomalies");
                        writer.WriteLine("integrity.lastBadPacketId=0x{0:X2}", snap.LastBadPacketId);
                        writer.WriteLine("integrity.lastBadDeclaredLen={0}", snap.LastBadDeclared);
                        writer.WriteLine("integrity.lastBadAvailLen={0}", snap.LastBadAvailable);
                        string lastBadPrefix = FormatHexPrefix(snap.LastBadPrefix, snap.LastBadPrefixLength);
                        if (!string.IsNullOrEmpty(lastBadPrefix))
                                writer.WriteLine("integrity.lastBadHexPrefix={0}", lastBadPrefix);

                        for (int i = 0; i < snap.IntegrityRecordCount; ++i)
                        {
                                IntegrityRecord rec = snap.IntegrityRecords[i];
                                writer.Write("integrity.records[{0}]=ageMs:{1} pid:0x{2:X2} declared:{3} avail:{4} info:{5}", i, Age(snap.Tick, rec.Tick), rec.PacketId, rec.DeclaredLength, rec.AvailableLength, rec.Info);
                                string hex = FormatHexPrefix(rec.PrefixBytes, rec.PrefixLength);
                                if (!string.IsNullOrEmpty(hex))
                                        writer.Write(" hex:{0}", hex);
                                if (!string.IsNullOrEmpty(rec.Message))
                                        writer.Write(" msg:{0}", rec.Message);
                                writer.WriteLine();
                        }

                        writer.WriteLine();
                        writer.WriteLine("# SendDrainStats");
                        int send1s = 0, send10s = 0, sendBytes10s = 0;
                        for (int i = 0; i < 10; ++i)
                        {
                                int idx = (snap.SendCompletionIndex - i + 10) % 10;
                                send10s += snap.SendCompletions[idx];
                                sendBytes10s += snap.SendCompletionBytes[idx];
                                if (i == 0)
                                        send1s += snap.SendCompletions[idx];
                        }
                        writer.WriteLine("send.completions1s={0}", send1s);
                        writer.WriteLine("send.completions10s={0}", send10s);
                        writer.WriteLine("send.avgBytesPerCompletion10s={0}", send10s == 0 ? 0 : sendBytes10s / Math.Max(1, send10s));
                        writer.WriteLine("send.completions60s={0}", SumLast(snap.SendCompletionsLong, snap.SendCompletionLongIndex, 60));
                        writer.WriteLine("send.bytes60s={0}", SumLast(snap.SendCompletionBytesLong, snap.SendCompletionLongIndex, 60));
                        writer.WriteLine("send.enqueued60s={0}", SumLast(snap.SendEnqueued, snap.SendEnqueueIndex, 60));
                        writer.WriteLine("send.enqueuedBytes60s={0}", SumLast(snap.SendEnqueuedBytes, snap.SendEnqueueIndex, 60));
                        writer.WriteLine("send.lastCompletionMsAgo={0}", Age(snap.Tick, snap.LastSendDrainTick));
                        writer.WriteLine("send.lastEnqueueMsAgo={0}", Age(snap.Tick, snap.LastEnqueueTick));

                        writer.WriteLine();
                        writer.WriteLine("# AliveIndicators");
                        writer.WriteLine("alive.lastInboundMsAgo={0}", Age(snap.Tick, snap.LastInboundTick));
                        writer.WriteLine("alive.lastInboundNonKeepaliveMsAgo={0}", Age(snap.Tick, snap.LastInboundNonKeepaliveTick));
                        writer.WriteLine("alive.lastOutboundMsAgo={0}", Age(snap.Tick, snap.LastOutboundTick));

                        writer.WriteLine();
                        writer.WriteLine("# BurstSamples");
                        if (snap.BurstSamples != null)
                        {
                                for (int i = 0; i < snap.BurstSamples.Length; ++i)
                                {
                                        BurstSample sample = snap.BurstSamples[i];
                                        string hex = FormatHexPrefix(sample.Prefix, sample.PrefixLength);
                                        if (!string.IsNullOrEmpty(hex))
                                                writer.WriteLine("burst[{0}]=pid:0x{1:X2} len:{2} hex:{3}", i, sample.PacketId, sample.Length, hex);
                                        else
                                                writer.WriteLine("burst[{0}]=pid:0x{1:X2} len:{2}", i, sample.PacketId, sample.Length);
                                }
                        }
                }

                private static void WriteBodySnapshot(TextWriter writer, NetFlightSnapshot snap)
                {
                        writer.WriteLine();
                        writer.WriteLine("# Events (newest last)");

                        for (int i = 0; i < snap.EventCount; ++i)
                        {
                                NetFlightEvent evt = snap.Events[i];
                                double age = Age(snap.Tick, evt.Tick) / 1000.0;
                                writer.Write("-@{0:F3}s dir={1} pid=0x{2:X2} len={3}", age, DirectionFor(evt.Flags), evt.PacketId, evt.Length);

                                if (evt.Info != NetFlightInfoCode.None)
                                        writer.Write(" info={0}", evt.Info);

                                if ((evt.Flags & NetFlightEventFlags.Integrity) != 0)
                                        writer.Write(" integrity=1");

                                if ((evt.Flags & NetFlightEventFlags.Movement) != 0)
                                        writer.Write(" movement=1");

                                writer.WriteLine();
                        }
                }

                private void WriteHeader(TextWriter writer, int tick, NetFlightTriggerReason reasons, bool retriggered, string manualReason)
                {
                        writer.WriteLine("# NetFlightRecorder Dump");
                        writer.WriteLine("timestampUtc={0}", DateTime.UtcNow.ToString("O"));
                        writer.WriteLine("account={0}", _owner.Account != null ? _owner.Account.Username : "(none)");
                        writer.WriteLine("character={0}", _owner.Mobile != null ? _owner.Mobile.RawName : "(none)");
                        writer.WriteLine("ip={0}", _owner.Address);
                        writer.WriteLine("reason={0}", manualReason ?? reasons.ToString());
                        writer.WriteLine("retriggered={0}", retriggered || _retriggered);

                        int flushDepth;
                        lock (_flushQueue)
                                flushDepth = _flushQueue.Count;

                        writer.WriteLine("health.enabled={0}", _enabled);
                        writer.WriteLine("health.flushQueueDepth={0}", flushDepth);
                        writer.WriteLine("health.flushDropped={0}", _flushDropped);
                        writer.WriteLine("health.flushLagMs={0}", _flushLagMs);
                        writer.WriteLine("health.warmupActive={0}", InWarmup(tick));

                        int moveReq10 = GetMovementRequests(10, tick);
                        int moveReq60 = GetMovementRequests(60, tick);
                        int moveResp10 = GetMovementResponses(10, tick, 0x21) + GetMovementResponses(10, tick, 0x22) + GetMovementResponses(10, tick, 0x23);
                        int moveResp60 = GetMovementResponses(60, tick, 0x21) + GetMovementResponses(60, tick, 0x22) + GetMovementResponses(60, tick, 0x23);

                        _outbound.Aggregate(10, tick, _scratchCounts, _scratchBytes);
                        string topOut10s = TopToString(_scratchCounts, _scratchBytes, 3);

                        writer.WriteLine("SIG reason={0} stall={1:F3} pending={2} in10sPkts={3} out10sPkts={4} out10sBytes={5} moveReq10s={6} moveResp10s={7} deniedStreak={8} noChangeStreak={9} lastPosChangeMs={10} integrityErrs={11} topOut10s={12}", manualReason ?? reasons.ToString(), _stallStartTick == 0 ? 0 : Age(tick, _stallStartTick) / 1000.0, _pendingBytes, _inbound.GetTotalPackets(10, tick), _outbound.GetTotalPackets(10, tick), _outbound.GetTotalBytes(10, tick), moveReq10, moveResp10, _movementDenied, _movementNoChange, Age(tick, _lastMovementChangeTick), _integrityExceptions + _integrityInvalidLength + _integrityDeclaredLengthMismatch + _integrityUnknown + _integrityDecode, topOut10s);
                        writer.WriteLine("pendingBytes={0}", _pendingBytes);
                        writer.WriteLine("peakPendingBytes={0}", _peakPending);

                        double stallSeconds = 0;
                        if (_stallStartTick != 0)
                                stallSeconds = Age(tick, _stallStartTick) / 1000.0;

                        writer.WriteLine("stallSeconds={0:F3}", stallSeconds);
                        writer.WriteLine("stallSpikes={0}", _stallSpikeCount);

                        writer.WriteLine("outbound.packets1s={0}", _outbound.GetTotalPackets(1, tick));
                        writer.WriteLine("outbound.packets10s={0}", _outbound.GetTotalPackets(10, tick));
                        writer.WriteLine("outbound.bytes1s={0}", _outbound.GetTotalBytes(1, tick));
                        writer.WriteLine("outbound.bytes10s={0}", _outbound.GetTotalBytes(10, tick));
                        writer.WriteLine("inbound.packets1s={0}", _inbound.GetTotalPackets(1, tick));
                        writer.WriteLine("inbound.packets10s={0}", _inbound.GetTotalPackets(10, tick));
                        writer.WriteLine("inbound.bytes1s={0}", _inbound.GetTotalBytes(1, tick));
                        writer.WriteLine("inbound.bytes10s={0}", _inbound.GetTotalBytes(10, tick));

                        writer.WriteLine("movement.requests={0}", _movementRequests);
                        writer.WriteLine("movement.denied={0}", _movementDenied);
                        writer.WriteLine("movement.noChange={0}", _movementNoChange);
                        writer.WriteLine("movement.lastChangeMsAgo={0}", Age(tick, _lastMovementChangeTick));
                        writer.WriteLine("movement.req10s={0}", moveReq10);
                        writer.WriteLine("movement.req60s={0}", moveReq60);
                        writer.WriteLine("movement.resp10s={0}", moveResp10);
                        writer.WriteLine("movement.resp60s={0}", moveResp60);
                        int oldestOutstanding;
                        int outstandingCount = GetOutstandingCount(tick, out oldestOutstanding);
                        writer.WriteLine("movement.outstanding={0}", outstandingCount);
                        writer.WriteLine("movement.lastReqMsAgo={0}", Age(tick, _lastMovementInboundTick));
                        writer.WriteLine("movement.lastRespMsAgo={0}", Age(tick, _lastMovementResponseTick));

                        writer.WriteLine("integrity.exceptions={0}", _integrityExceptions);
                        writer.WriteLine("integrity.unknownPackets={0}", _integrityUnknown);
                        writer.WriteLine("integrity.invalidLength={0}", _integrityInvalidLength);
                        writer.WriteLine("integrity.lengthMismatch={0}", _integrityDeclaredLengthMismatch);
                        writer.WriteLine("integrity.decode={0}", _integrityDecode);

                        if (!string.IsNullOrEmpty(_lastIntegrityMessage))
                                writer.WriteLine("integrity.lastMessage={0}", _lastIntegrityMessage);

                        int[] counts = _scratchCounts;
                        int[] bytes = _scratchBytes;

                        _outbound.Aggregate(1, tick, counts, bytes);
                        WriteTopPackets(writer, "outbound.top1s", counts, bytes, 3);

                        _outbound.Aggregate(10, tick, counts, bytes);
                        WriteTopPackets(writer, "outbound.top10s", counts, bytes, 3);

                        _outboundLong.Aggregate(TopWindowLongSeconds, tick, counts, bytes);
                        WriteTopPackets(writer, "outbound.top60s", counts, bytes, 5);

                        _inbound.Aggregate(1, tick, counts, bytes);
                        WriteTopPackets(writer, "inbound.top1s", counts, bytes, 3);

                        _inbound.Aggregate(10, tick, counts, bytes);
                        WriteTopPackets(writer, "inbound.top10s", counts, bytes, 3);

                        _inboundLong.Aggregate(TopWindowLongSeconds, tick, counts, bytes);
                        WriteTopPackets(writer, "inbound.top60s", counts, bytes, 5);

                        writer.WriteLine();
                        writer.WriteLine("# MovementCorrelation");
                        writer.WriteLine("movement.outstanding={0}", outstandingCount);
                        writer.WriteLine("movement.oldestOutstandingMs={0}", outstandingCount == 0 ? 0 : Age(tick, oldestOutstanding));
                        writer.WriteLine("movement.lastMoveReqMsAgo={0}", Age(tick, _lastMovementInboundTick));
                        writer.WriteLine("movement.lastMoveRespMsAgo={0}", Age(tick, _lastMovementResponseTick));

                        int outstandingSlots = Math.Min(_moveOutstandingCount, 8);
                        for (int i = 0; i < outstandingSlots; ++i)
                        {
                                int idx = (_moveOutstandingIndex - _moveOutstandingCount + i + MovementOutstandingCap) % MovementOutstandingCap;
                                MoveHistory mv = _moveOutstanding[idx];
                                writer.WriteLine("movement.outstanding[{0}]=ageMs:{1} dir:{2} seq:{3} responded:{4} respId:{5}", i, Age(tick, mv.Tick), mv.Direction, mv.Sequence, mv.Responded, mv.ResponseId);
                        }

                        writer.WriteLine();
                        writer.WriteLine("# TrafficDuringStuck");
                        writer.WriteLine("stuck.active={0}", _stuckWindowStartTick != 0);
                        writer.WriteLine("stuck.durationMs={0}", _stuckWindowStartTick == 0 ? 0 : Age(tick, _stuckWindowStartTick));
                        writer.WriteLine("stuck.outPackets={0}", _stuckOutboundPackets);
                        writer.WriteLine("stuck.outBytes={0}", _stuckOutboundBytes);
                        WriteTopPackets(writer, "stuck.top", _stuckTopCounts, _stuckTopBytes, 5);

                        writer.WriteLine();
                        writer.WriteLine("# DecodeStats");
                        writer.WriteLine("decode.preBytes10s={0}", _decodePreBytes10s);
                        writer.WriteLine("decode.postBytes10s={0}", _decodePostBytes10s);
                        writer.WriteLine("decode.calls10s={0}", _decodeCalls10s);
                        double ratio = _decodePreBytes10s == 0 ? 0 : (double)_decodePostBytes10s / Math.Max(1, _decodePreBytes10s);
                        writer.WriteLine("decode.ratio10s={0:F3}", ratio);
                        writer.WriteLine("decode.exceptionCount10m={0}", _decodeExceptionCount);

                        writer.WriteLine();
                        writer.WriteLine("# IntegrityAnomalies");
                        writer.WriteLine("integrity.lastBadPacketId=0x{0:X2}", _lastBadPacketId);
                        writer.WriteLine("integrity.lastBadDeclaredLen={0}", _lastBadDeclared);
                        writer.WriteLine("integrity.lastBadAvailLen={0}", _lastBadAvailable);
                        string lastBadPrefix = FormatHexPrefix(_lastBadPrefix, _lastBadPrefixLength);
                        if (!string.IsNullOrEmpty(lastBadPrefix))
                                writer.WriteLine("integrity.lastBadHexPrefix={0}", lastBadPrefix);

                        for (int i = 0; i < _integrityRecordCount; ++i)
                        {
                                int idx = (_integrityRecordIndex - _integrityRecordCount + i + IntegrityRecordCap) % IntegrityRecordCap;
                                IntegrityRecord rec = _integrityRecords[idx];
                                writer.Write("integrity.records[{0}]=ageMs:{1} pid:0x{2:X2} declared:{3} avail:{4} info:{5}", i, Age(tick, rec.Tick), rec.PacketId, rec.DeclaredLength, rec.AvailableLength, rec.Info);
                                string hex = FormatHexPrefix(rec.PrefixBytes, rec.PrefixLength);
                                if (!string.IsNullOrEmpty(hex))
                                        writer.Write(" hex:{0}", hex);
                                if (!string.IsNullOrEmpty(rec.Message))
                                        writer.Write(" msg:{0}", rec.Message);
                                writer.WriteLine();
                        }

                        writer.WriteLine();
                        writer.WriteLine("# SendDrainStats");
                        int send1s = 0, send10s = 0, sendBytes10s = 0;
                        for (int i = 0; i < 10; ++i)
                        {
                                int idx = (_sendCompletionIndex - i + 10) % 10;
                                send10s += _sendCompletions[idx];
                                sendBytes10s += _sendCompletionBytes[idx];
                                if (i == 0)
                                        send1s += _sendCompletions[idx];
                        }
                        writer.WriteLine("send.completions1s={0}", send1s);
                        writer.WriteLine("send.completions10s={0}", send10s);
                        writer.WriteLine("send.avgBytesPerCompletion10s={0}", send10s == 0 ? 0 : sendBytes10s / send10s);
                        writer.WriteLine("send.lastCompletionMsAgo={0}", Age(tick, _lastSendDrainTick));
                        writer.WriteLine("send.lastEnqueueMsAgo={0}", Age(tick, _lastEnqueueTick));
                        writer.WriteLine("send.completions60s={0}", SumLast(_sendCompletionsLong, _sendCompletionLongIndex, 60));
                        writer.WriteLine("send.bytes60s={0}", SumLast(_sendCompletionBytesLong, _sendCompletionLongIndex, 60));
                        writer.WriteLine("send.enqueued60s={0}", SumLast(_sendEnqueued, _sendEnqueueIndex, 60));
                        writer.WriteLine("send.enqueuedBytes60s={0}", SumLast(_sendEnqueuedBytes, _sendEnqueueIndex, 60));

                        writer.WriteLine();
                        writer.WriteLine("# MovementCorrelation");
                        writer.WriteLine("movement.oldestOutstandingMs={0}", _moveOutstandingCount == 0 ? -1 : Age(tick, _moveOutstanding[(_moveOutstandingIndex - _moveOutstandingCount + MovementOutstandingCap) % MovementOutstandingCap].Tick));
                        writer.WriteLine("movement.lastPosChangeMsAgo={0}", Age(tick, _lastMovementChangeTick));
                        writer.WriteLine("movement.deniedStreak={0}", _movementDenied);
                        writer.WriteLine("movement.noChangeStreak={0}", _movementNoChange);
                        int pairs = Math.Min(10, _moveHistoryCount);
                        for (int i = 0; i < pairs; ++i)
                        {
                                int idx = (_moveHistoryIndex - i - 1 + MovementHistoryCap) % MovementHistoryCap;
                                MoveHistory h = _moveHistory[idx];
                                writer.Write("movement.pair[{0}]=ageMs:{1} dir:{2} seq:{3} resp:0x{4:X2} respAge:{5}", i, Age(tick, h.Tick), h.Direction, h.Sequence, h.ResponseId, h.Responded ? Age(tick, h.ResponseTick) : -1);
                                writer.WriteLine();
                        }

                        writer.WriteLine();
                        writer.WriteLine("# AliveIndicators");
                        writer.WriteLine("alive.lastInboundMsAgo={0}", Age(tick, _lastInboundTick));
                        writer.WriteLine("alive.lastInboundNonKeepaliveMsAgo={0}", Age(tick, _lastInboundNonKeepaliveTick));
                        writer.WriteLine("alive.lastOutboundMsAgo={0}", Age(tick, _lastOutboundTick));

                        if (_burstSamples.Count > 0)
                        {
                                writer.WriteLine();
                                writer.WriteLine("# BurstSamples");
                                for (int i = 0; i < _burstSamples.Count; ++i)
                                {
                                        BurstSample sample = _burstSamples[i];
                                        string hex = FormatHexPrefix(sample.Prefix, sample.PrefixLength);
                                        if (!string.IsNullOrEmpty(hex))
                                                writer.WriteLine("burst[{0}]=pid:0x{1:X2} len:{2} hex:{3}", i, sample.PacketId, sample.Length, hex);
                                        else
                                                writer.WriteLine("burst[{0}]=pid:0x{1:X2} len:{2}", i, sample.PacketId, sample.Length);
                                }
                        }

                        writer.WriteLine();
                }

                private static void WriteTopPackets(TextWriter writer, string label, int[] counts, int[] bytes, int take)
                {
                        List<Tuple<int, int, int>> top = new List<Tuple<int, int, int>>(256);

                        for (int i = 0; i < counts.Length; ++i)
                        {
                                if (counts[i] > 0)
                                        top.Add(Tuple.Create(i, counts[i], bytes[i]));
                        }

                        top.Sort((a, b) => b.Item2.CompareTo(a.Item2));

                        writer.Write(label);
                        writer.Write('=');

                        for (int i = 0; i < top.Count && i < take; ++i)
                        {
                                if (i > 0)
                                        writer.Write(',');

                                writer.Write("0x{0:X2}:{1}/{2}", top[i].Item1, top[i].Item2, top[i].Item3);
                        }

                        writer.WriteLine();
                }

                private static string TopToString(int[] counts, int[] bytes, int take)
                {
                        List<Tuple<int, int, int>> top = new List<Tuple<int, int, int>>(256);
                        for (int i = 0; i < counts.Length; ++i)
                        {
                                if (counts[i] > 0)
                                        top.Add(Tuple.Create(i, counts[i], bytes[i]));
                        }

                        top.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < top.Count && i < take; ++i)
                        {
                                if (i > 0)
                                        sb.Append(',');
                                sb.AppendFormat("0x{0:X2}:{1}:{2}", top[i].Item1, top[i].Item2, top[i].Item3);
                        }

                        return sb.ToString();
                }

                private static string FormatHexPrefix(byte[] bytes, int length)
                {
                        if (bytes == null || length <= 0)
                                return null;

                        int take = Math.Min(bytes.Length, length);
                        StringBuilder sb = new StringBuilder(take * 2);
                        for (int i = 0; i < take; ++i)
                                sb.Append(bytes[i].ToString("X2"));

                        return sb.ToString();
                }

                private void WriteBody(TextWriter writer, int tick, bool extendWindow)
                {
                        int window = extendWindow ? Math.Max(RingWindowMs, PerClientCooldownMs) : RingWindowMs;
                        int cutoff = tick - window;
                        int count = _eventCount;
                        int index = _eventCount == EventCapacity ? _eventIndex : 0;

                        writer.WriteLine("# Events (newest last)");

                        for (int i = 0; i < count; ++i)
                        {
                                int slot = (index + i) % EventCapacity;
                                NetFlightEvent evt = _events[slot];

                                if (evt.Tick < cutoff)
                                        continue;

                                double age = Age(tick, evt.Tick) / 1000.0;
                                writer.Write("-@{0:F3}s dir={1} pid=0x{2:X2} len={3}", age, DirectionFor(evt.Flags), evt.PacketId, evt.Length);

                                if (evt.Info != NetFlightInfoCode.None)
                                        writer.Write(" info={0}", evt.Info);

                                if ((evt.Flags & NetFlightEventFlags.Integrity) != 0)
                                        writer.Write(" integrity=1");

                                if ((evt.Flags & NetFlightEventFlags.Movement) != 0)
                                        writer.Write(" movement=1");

                                writer.WriteLine();
                        }
                }

                private static void FlushWorker()
                {
                        while (true)
                        {
                                _flushSignal.WaitOne();

                                while (true)
                                {
                                        NetFlightSnapshot snapshot = null;

                                        lock (_flushQueue)
                                        {
                                                if (_flushQueue.Count > 0)
                                                        snapshot = _flushQueue.Dequeue();
                                        }

                                        if (snapshot == null)
                                                break;

                                        try
                                        {
                                                string directory = Path.GetDirectoryName(snapshot.Path);
                                                if (!string.IsNullOrEmpty(directory))
                                                        Directory.CreateDirectory(directory);

                                                Stopwatch sw = Stopwatch.StartNew();
                                                string content = FormatSnapshot(snapshot);
                                                sw.Stop();
                                                _lastFormatDurationMs = (int)sw.ElapsedMilliseconds;

                                                sw.Restart();
                                                using (StreamWriter writer = new StreamWriter(snapshot.Path, false, Encoding.UTF8))
                                                        writer.Write(content);
                                                sw.Stop();
                                                _lastWriteDurationMs = (int)sw.ElapsedMilliseconds;

                                                _flushLagMs = Math.Max(0, Now() - snapshot.Tick);
                                                _lastWriterError = null;
                                        }
                                        catch (Exception ex)
                                        {
                                                _flushDropped++;
                                                _lastWriterError = ex.Message;
                                        }
                                }
                        }
                }

                private int GetReceiveQueueBytes()
                {
                        ByteQueue buffer = _owner != null ? _owner.Buffer : null;
                        return buffer != null ? Math.Max(0, buffer.Length) : 0;
                }

                private NetFlightClientDiagnosticsSnapshot GetClientDiagnostics(int tick)
                {
                        NetFlightClientDiagnosticsSnapshot snapshot = new NetFlightClientDiagnosticsSnapshot();
                        snapshot.SessionId = _sessionId;
                        snapshot.Account = _owner != null && _owner.Account != null ? _owner.Account.Username : "none";
                        snapshot.Character = _owner != null && _owner.Mobile != null ? _owner.Mobile.RawName : "none";
                        snapshot.Ip = _owner != null && _owner.Address != null ? _owner.Address.ToString() : "0.0.0.0";
                        snapshot.ConnectedUtc = _connectedUtc;
                        snapshot.ConnectedSeconds = Math.Max(0, Age(tick, _startTick) / 1000);

                        if (!Monitor.TryEnter(_syncRoot))
                        {
                                snapshot.Health = "BUSY";
                                snapshot.SnapshotBusy = true;
                                return snapshot;
                        }

                        try
                        {
                                int[] counts = new int[256];
                                int[] bytes = new int[256];

                                snapshot.PendingBytes = _pendingBytes;
                                snapshot.PeakPendingBytes = _peakPending;
                                snapshot.ReceiveQueueBytes = GetReceiveQueueBytes();
                                snapshot.SendCompletionAgeMs = Age(tick, _lastSendDrainTick);
                                snapshot.EnqueueAgeMs = Age(tick, _lastEnqueueTick);
                                snapshot.InboundAgeMs = Age(tick, _lastInboundTick);
                                snapshot.OutboundAgeMs = Age(tick, _lastOutboundTick);
                                snapshot.InboundPackets1s = _inbound.GetTotalPackets(1, tick);
                                snapshot.InboundPackets10s = _inbound.GetTotalPackets(10, tick);
                                snapshot.OutboundPackets1s = _outbound.GetTotalPackets(1, tick);
                                snapshot.OutboundPackets10s = _outbound.GetTotalPackets(10, tick);

                                _inbound.Aggregate(10, tick, counts, bytes);
                                snapshot.InboundUltimaLive10s = counts[0x3F];
                                _inboundLong.Aggregate(60, tick, counts, bytes);
                                snapshot.InboundUltimaLive60s = counts[0x3F];
                                _outbound.Aggregate(10, tick, counts, bytes);
                                snapshot.OutboundUltimaLive10s = counts[0x3F];
                                _outboundLong.Aggregate(60, tick, counts, bytes);
                                snapshot.OutboundUltimaLive60s = counts[0x3F];

                                snapshot.OutstandingMovement = _moveOutstandingCount;
                                snapshot.IntegrityErrors = _integrityExceptions + _integrityUnknown + _integrityInvalidLength
                                        + _integrityDeclaredLengthMismatch + _integrityDecode;
                                snapshot.StallAgeMs = _stallStartTick != 0 ? Age(tick, _stallStartTick) : 0;
                                snapshot.EventCount = _eventCount;
                                snapshot.LastDumpReason = _lastDumpReason ?? "none";
                                snapshot.LastDumpAgeMs = Age(tick, _lastDumpTick);

                                bool critical = snapshot.PendingBytes >= BacklogThreshold
                                        || snapshot.StallAgeMs >= StallDurationMs
                                        || snapshot.ReceiveQueueBytes >= 65536
                                        || snapshot.IntegrityErrors > 0;
                                bool warning = snapshot.PendingBytes >= 65536
                                        || snapshot.StallAgeMs >= 500
                                        || snapshot.ReceiveQueueBytes >= 16384
                                        || snapshot.OutboundPackets1s > PacketStormThreshold;

                                snapshot.HealthScore = critical ? 2 : (warning ? 1 : 0);
                                snapshot.Health = critical ? "CRITICAL" : (warning ? "WARN" : "HEALTHY");
                        }
                        finally
                        {
                                Monitor.Exit(_syncRoot);
                        }

                        return snapshot;
                }

                public static NetFlightDiagnosticsSnapshot GetDiagnosticsSnapshot()
                {
                        int tick = Now();
                        NetFlightRecorder[] recorders;

                        lock (_recordersLock)
                                recorders = _recorders.ToArray();

                        List<NetFlightClientDiagnosticsSnapshot> clients = new List<NetFlightClientDiagnosticsSnapshot>(recorders.Length);

                        for (int i = 0; i < recorders.Length; ++i)
                        {
                                NetFlightRecorder recorder = recorders[i];
                                if (recorder != null && !recorder._detached)
                                        clients.Add(recorder.GetClientDiagnostics(tick));
                        }

                        clients.Sort(delegate(NetFlightClientDiagnosticsSnapshot left, NetFlightClientDiagnosticsSnapshot right)
                        {
                                int score = right.HealthScore.CompareTo(left.HealthScore);
                                if (score != 0)
                                        return score;

                                int pending = right.PendingBytes.CompareTo(left.PendingBytes);
                                if (pending != 0)
                                        return pending;

                                return string.Compare(left.Character, right.Character, StringComparison.OrdinalIgnoreCase);
                        });

                        NetFlightDiagnosticsSnapshot snapshot = new NetFlightDiagnosticsSnapshot();
                        snapshot.Enabled = _enabled;
                        snapshot.ConnectedSessions = clients.Count;

                        for (int i = 0; i < clients.Count; ++i)
                        {
                                if (clients[i].HealthScore >= 2)
                                        snapshot.CriticalSessions++;
                                else if (clients[i].HealthScore == 1 || clients[i].SnapshotBusy)
                                        snapshot.WarningSessions++;
                                else
                                        snapshot.HealthySessions++;
                        }

                        lock (_flushQueue)
                                snapshot.FlushQueueDepth = _flushQueue.Count;

                        snapshot.FlushDropped = _flushDropped;
                        snapshot.FlushLagMs = _flushLagMs;
                        snapshot.SnapshotDropped = _snapshotDropped;
                        snapshot.LastWriteDurationMs = _lastWriteDurationMs;
                        snapshot.LastFormatDurationMs = _lastFormatDurationMs;
                        snapshot.LastWriterError = _lastWriterError ?? "none";
                        snapshot.TotalDumps = _totalDumps;
                        snapshot.AutomaticDumps = _automaticDumps;
                        snapshot.DisconnectDumps = _disconnectDumps;
                        snapshot.ManualDumps = _manualDumps;
                        snapshot.OutputFolder = GetFullPath(Path.Combine("Logs", "NetFlight"));
                        snapshot.Clients = clients.ToArray();

                        lock (_historyLock)
                        {
                                snapshot.RecentDumpPaths = _recentDumpPaths.ToArray();
                                snapshot.RecentDumpCount = snapshot.RecentDumpPaths.Length;
                        }

                        return snapshot;
                }

                public static void WriteDiagnostics(TextWriter writer)
                {
                        if (writer == null)
                                return;

                        NetFlightDiagnosticsSnapshot snapshot = GetDiagnosticsSnapshot();
                        writer.WriteLine("NetFlightStats enabled={0} connected={1} healthy={2} warning={3} critical={4}",
                                snapshot.Enabled,
                                snapshot.ConnectedSessions,
                                snapshot.HealthySessions,
                                snapshot.WarningSessions,
                                snapshot.CriticalSessions);
                        writer.WriteLine("NetFlightStats dumps total={0} automatic={1} disconnect={2} manual={3} recentDisplayed={4}",
                                snapshot.TotalDumps,
                                snapshot.AutomaticDumps,
                                snapshot.DisconnectDumps,
                                snapshot.ManualDumps,
                                snapshot.RecentDumpCount);
                        writer.WriteLine("NetFlightStats flush queue={0} dropped={1} lagMs={2} snapshotDropped={3} lastFormatMs={4} lastWriteMs={5} lastError={6}",
                                snapshot.FlushQueueDepth,
                                snapshot.FlushDropped,
                                snapshot.FlushLagMs,
                                snapshot.SnapshotDropped,
                                snapshot.LastFormatDurationMs,
                                snapshot.LastWriteDurationMs,
                                snapshot.LastWriterError);
                        writer.WriteLine("NetFlightStats outputFolder={0}", snapshot.OutputFolder);
                        writer.WriteLine("NetFlightStats privacy: files contain account names, character names, IP addresses, packet identifiers, and exception details; sanitize before public sharing.");

                        if (snapshot.Clients == null || snapshot.Clients.Length == 0)
                        {
                                writer.WriteLine("NetFlightStats clients: No connected sessions.");
                        }
                        else
                        {
                                for (int i = 0; i < snapshot.Clients.Length; ++i)
                                {
                                        NetFlightClientDiagnosticsSnapshot client = snapshot.Clients[i];
                                        writer.WriteLine(
                                                "NetFlightClient session={0} health={1} account={2} character={3} ip={4} connectedSeconds={5} pendingBytes={6} peakPendingBytes={7} receiveQueueBytes={8} sendCompletionAgeMs={9} enqueueAgeMs={10} inboundAgeMs={11} outboundAgeMs={12} in1s={13} in10s={14} out1s={15} out10s={16} ultimaLiveIn10s={17} ultimaLiveIn60s={18} ultimaLiveOut10s={19} ultimaLiveOut60s={20} movementOutstanding={21} integrityErrors={22} stallAgeMs={23} events={24} lastDumpAgeMs={25} lastDumpReason={26} snapshotBusy={27}",
                                                client.SessionId,
                                                client.Health,
                                                client.Account,
                                                client.Character,
                                                client.Ip,
                                                client.ConnectedSeconds,
                                                client.PendingBytes,
                                                client.PeakPendingBytes,
                                                client.ReceiveQueueBytes,
                                                client.SendCompletionAgeMs,
                                                client.EnqueueAgeMs,
                                                client.InboundAgeMs,
                                                client.OutboundAgeMs,
                                                client.InboundPackets1s,
                                                client.InboundPackets10s,
                                                client.OutboundPackets1s,
                                                client.OutboundPackets10s,
                                                client.InboundUltimaLive10s,
                                                client.InboundUltimaLive60s,
                                                client.OutboundUltimaLive10s,
                                                client.OutboundUltimaLive60s,
                                                client.OutstandingMovement,
                                                client.IntegrityErrors,
                                                client.StallAgeMs,
                                                client.EventCount,
                                                client.LastDumpAgeMs,
                                                client.LastDumpReason,
                                                client.SnapshotBusy);
                                }
                        }

                        if (snapshot.RecentDumpPaths == null || snapshot.RecentDumpPaths.Length == 0)
                        {
                                writer.WriteLine("NetFlightStats recentDumps: none");
                        }
                        else
                        {
                                for (int i = 0; i < snapshot.RecentDumpPaths.Length; ++i)
                                        writer.WriteLine("NetFlightDump[{0}]={1}", i + 1, snapshot.RecentDumpPaths[i]);
                        }
                }

                public static void SetEnabled(bool enabled)
                {
                        _enabled = enabled;
                }

                public static void SetMovementTracking(bool enabled)
                {
                        _movementTrackingEnabled = enabled;
                }

                public static void SetIntegrityCapture(bool enabled)
                {
                        _integrityEnabled = enabled;
                }

                public static void SetBurstSampling(bool enabled)
                {
                        _burstSamplingEnabled = enabled;
                }

                public static void SetRecordInboundCountsOnly(bool enabled)
                {
                        _recordInboundCountsOnly = enabled;
                }

                public static void SetRecordOutboundCountsOnly(bool enabled)
                {
                        _recordOutboundCountsOnly = enabled;
                }

                public static void SetRecordEventRing(bool enabled)
                {
                        _recordEventRing = enabled;
                }

                public static void SetEventSampleRate(int rate)
                {
                        _eventSampleRate = Math.Max(1, rate);
                }

                public static string GetFlushStatus()
                {
                        int depth;
                        lock (_flushQueue)
                                depth = _flushQueue.Count;

                        return string.Format("enabled:{0} queue:{1} dropped:{2} lagMs:{3} snapshotsDropped:{4} lastWriteMs:{5} lastError:{6}", _enabled, depth, _flushDropped, _flushLagMs, _snapshotDropped, _lastWriteDurationMs, _lastWriterError ?? "none");
                }

                public static string GetSettingsStatus()
                {
                        return string.Format("eventRing:{0} inboundCountsOnly:{1} outboundCountsOnly:{2} sampleRate:{3} movement:{4} integrity:{5} burst:{6}", _recordEventRing, _recordInboundCountsOnly, _recordOutboundCountsOnly, _eventSampleRate, _movementTrackingEnabled, _integrityEnabled, _burstSamplingEnabled);
                }

                private static string DirectionFor(NetFlightEventFlags flags)
                {
                        if ((flags & NetFlightEventFlags.Outbound) != 0)
                                return "out";

                        if ((flags & NetFlightEventFlags.Inbound) != 0)
                                return "in";

                        return "n/a";
                }

                private static string SafePath(string value)
                {
                        if (string.IsNullOrEmpty(value))
                                return "none";

                        foreach (char c in Path.GetInvalidFileNameChars())
                                value = value.Replace(c, '_');

                        return value.Replace(' ', '_');
                }

                private static string GetFullPath(string path)
                {
                        try
                        {
                                return Path.GetFullPath(path);
                        }
                        catch
                        {
                                return path;
                        }
                }

        }
}
#endif
