using System;
using System.IO;
using System.Text;
using Server.Diagnostics;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;
using Server.Network;

namespace Server.Commands
{
	public class TaskManagerTestGump : Gump
	{
		private const string ColorGood = "63EF00";
		private const string ColorBad = "DE103A";
		internal const int HueGood = 63;
		internal const int HueBad = 133;

		internal const int PageDashboard = 1;
		private const int PageOfficial = 2;
		private const int PageFocused = 3;
		private const int PageLongRun = 4;
		internal const int PageReports = 5;
		private const int PageSettings = 6;
		private const int PageHelp = 7;
		internal const int PageOperations = 8;
		internal const int PageNetwork = 9;

		private const int ButtonClose = 1;
		private const int ButtonRefresh = 2;
		private const int ButtonCancel = 3;
		private const int ButtonReset = 4;
		private const int ButtonOverrideSettling = 5;
		private const int ButtonProgress = 6;

		private const int ButtonTabBase = 100;
		private const int ButtonOfficial = 201;
		private const int ButtonDaily = 202;
		private const int Button150 = 203;
		private const int Button250 = 204;
		private const int ButtonHourly = 205;
		private const int Button200 = 206;
		private const int ButtonLagReportOnly = 207;
		private const int ButtonBalanceValidate = 208;
		private const int ButtonDailyAttribution = 209;
		private const int ButtonDyePreview = 210;

		private const int ButtonLongRunStart = 301;
		private const int ButtonLongRunStop = 302;
		private const int ButtonLongRunDuration = 303;
		private const int ButtonLongRunInterval = 304;
		private const int ButtonLongRunStartup = 305;
		private const int ButtonLongRunTaskWindows = 306;
		private const int ButtonLongRunAutoSave = 307;
		private const int ButtonLongRunSummary = 308;

		private const int ButtonGenerateLagReport = 401;
		private const int ButtonGenerateSummary = 402;
		private const int ButtonClearReportHistory = 403;

		private const int ButtonSaveAggregate = 601;
		private const int ButtonSaveDeep = 602;
		private const int ButtonSaveDeepNext = 603;
		private const int ButtonSaveReset = 604;
		private const int ButtonConsoleFlush = 605;
		private const int ButtonConsoleRotate = 606;
		private const int ButtonRuneBagStatus = 607;
		private const int ButtonRuneBagReset = 608;
		private const int ButtonNetFlightStatus = 609;
		private const int ButtonNetFlightEnable = 610;

		private const int ButtonDailyMinus = 501;
		private const int ButtonDailyPlus = 502;
		private const int Button150Minus = 503;
		private const int Button150Plus = 504;
		private const int Button250Minus = 505;
		private const int Button250Plus = 506;
		private const int ButtonHourlyMinus = 507;
		private const int ButtonHourlyPlus = 508;
		private const int Button200Minus = 509;
		private const int Button200Plus = 510;
		private const int ButtonToggleHourly = 511;
		private const int ButtonToggle200 = 512;
		private const int ButtonToggleValidate = 513;
		private const int ButtonToggleAutoReports = 514;
		private const int ButtonToggleAutoSummary = 515;

		private readonly int m_Page;

		public TaskManagerTestGump() : this( PageDashboard )
		{
		}

		public TaskManagerTestGump( int page ) : base( 20, 20 )
		{
			if ( page < PageDashboard || page > PageNetwork )
				page = PageDashboard;

			m_Page = page;
			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			TaskManagerTestController.Snapshot snapshot = TaskManagerTestController.GetSnapshot();

			AddPage( 0 );
			AddBackground( 0, 0, 1040, 900, 9270 );
			AddAlphaRegion( 12, 12, 1016, 876 );
			AddHtml( 24, 18, 992, 28, Html( "<center><big>Performance Test Suite</big></center>" ), false, false );
			AddNavigation();
			AddFooter( snapshot );

			AddPage( 1 );

			if ( m_Page == PageOfficial )
				AddOfficialTests( snapshot );
			else if ( m_Page == PageFocused )
				AddFocusedTests( snapshot );
			else if ( m_Page == PageLongRun )
				AddLongRun( snapshot );
			else if ( m_Page == PageReports )
				AddReports( snapshot );
			else if ( m_Page == PageOperations )
				AddOperations();
			else if ( m_Page == PageNetwork )
				AddNetworkDiagnostics();
			else if ( m_Page == PageSettings )
				AddSettings( snapshot );
			else if ( m_Page == PageHelp )
				AddHelp();
			else
				AddDashboard( snapshot );
		}

		private void AddNavigation()
		{
			string[] labels = new string[] { "Dashboard", "Official", "Focused", "Long-Run", "Reports", "Network", "Operations", "Settings", "Help" };
			int[] pages = new int[] { PageDashboard, PageOfficial, PageFocused, PageLongRun, PageReports, PageNetwork, PageOperations, PageSettings, PageHelp };
			int[] positions = new int[] { 24, 132, 226, 320, 420, 514, 611, 729, 829 };

			for ( int i = 0; i < labels.Length; ++i )
			{
				AddButton( positions[i], 54, 4005, 4007, ButtonTabBase + pages[i], GumpButtonType.Reply, 0 );
				AddLabel( positions[i] + 31, 56, m_Page == pages[i] ? HueGood : 1152, labels[i] );
			}
		}

		private void AddFooter( TaskManagerTestController.Snapshot snapshot )
		{
			AddImageTiled( 24, 840, 992, 1, 2624 );
			AddButtonWithLabel( 30, 858, ButtonRefresh, "Refresh", 1152 );
			AddButtonWithLabel( 150, 858, ButtonGenerateLagReport, "LagReport", 1152 );
			AddButtonWithLabel( 290, 858, ButtonProgress, "Progress", 1152 );
			AddButtonWithLabel( 420, 858, ButtonCancel, "Cancel", 1152 );
			AddButtonWithLabel( 540, 858, ButtonReset, "Reset", 1152 );
			AddButtonWithLabel( 650, 858, ButtonClose, "Close", 1152 );
		}

		private void AddDashboard( TaskManagerTestController.Snapshot snapshot )
		{
			AddHtml( 24, 90, 992, 44, BuildStateBanner( snapshot ), false, false );
			AddPanel( 24, 145, 486, 300, "Suite Status", BuildStatusText( snapshot ) );
			AddPanel( 530, 145, 486, 300, "Health Snapshot", BuildHealthText( snapshot ) );
			AddPanel( 24, 460, 486, 355, "Latest Results", BuildResultsText( snapshot ) );
			AddPanel( 530, 460, 486, 355, "Progress and Output", BuildProgressText( snapshot ) );
		}

		private void AddOfficialTests( TaskManagerTestController.Snapshot snapshot )
		{
			AddPanel( 24, 100, 992, 230, "Official Test Modules", 
				"v3.16 Confirmation runs daily, 150-minute SpawnEntry, 250-minute, and optional smoke tests.<br><br>"
				+ "v3.17 Daily Attribution runs one forced daily path, captures the new deep attribution, settles, and writes a focused summary.<br><br>"
				+ "<basefont color=#FFCC66>This uses real maintenance paths and mutates world state. Daily and 250-minute runs update their normal scheduling state.</basefont>" );

			AddButtonWithLabel( 50, 350, ButtonOfficial, "Start Official v3.16 Confirmation", HueGood );
			AddButtonWithLabel( 530, 350, ButtonDailyAttribution, "Start Official v3.17 Daily Attribution", HueGood );
			AddPanel( 24, 395, 486, 340, "Current Options",
				String.Format( "{0}<br>{1}<br>{2}<br>{3}<br>{4}",
					ColorOnOff( "Hourly smoke test", snapshot.IncludeHourly, true ),
					ColorOnOff( "200-minute smoke test", snapshot.Include200, true ),
					ColorOnOff( "BalancePortalValidate", snapshot.IncludeBalancePortalValidate, false ),
					ColorOnOff( "Checkpoint LagReports", snapshot.AutoGenerateLagReportEveryStep, true ),
					ColorOnOff( "Final summary", snapshot.AutoGenerateFinalSummary, true ) ) );
			AddPanel( 530, 395, 486, 340, "Safety",
				"Only one suite or manual TaskManager test may run at once.<br><br>The suite refuses to start during World.Saving or while the settling guard is active.<br><br>BalancePortalValidate remains opt-in and requires a separate warning confirmation." );
		}

		private void AddFocusedTests( TaskManagerTestController.Snapshot snapshot )
		{
			AddPanel( 24, 100, 992, 100, "Focused Test Modules", "Each module uses the same controller, guards, real TaskManager path, checkpoint reports, and PASS/WARN/FAIL evaluation as the official suite." );

			AddActionBox( 24, 215, 318, 170, "Daily Only", "Runs daily maintenance, captures BalancePortal diagnostics, then settles.", ButtonDaily );
			AddActionBox( 361, 215, 318, 170, "150 SpawnEntry Only", "Runs the known SpawnEntry-producing path and waits for callback chains.", Button150 );
			AddActionBox( 698, 215, 318, 170, "250 Only", "Runs the forced 250-minute path and captures settled diagnostics.", Button250 );
			AddActionBox( 24, 400, 318, 170, "Hourly Only", "Runs the hourly maintenance smoke test.", ButtonHourly );
			AddActionBox( 361, 400, 318, 170, "200 Only", "Runs the 200-minute maintenance smoke test.", Button200 );
			AddActionBox( 698, 400, 318, 170, "LagReport Only", "Captures one LagReport and summary without mutating world state.", ButtonLagReportOnly );
			AddActionBox( 24, 585, 318, 150, "Dye Preview Tests", "Runs temporary functional checks for preview, cancel, apply, and stale responses.", ButtonDyePreview );
			AddActionBox( 361, 585, 318, 150, "BalancePortalValidate", "Read-only but intentionally expensive old full-scan comparison.", ButtonBalanceValidate );
			AddPanel( 698, 585, 318, 150, "Settling Guard", BuildSettlingGuardText( snapshot ) );
			AddButtonWithLabel( 870, 690, ButtonOverrideSettling, "Override", 1152 );
		}

		private void AddLongRun( TaskManagerTestController.Snapshot snapshot )
		{
			AddPanel( 24, 100, 992, 175, "Long-Run Diagnostics",
				"Captures unattended LagReports at a fixed interval. It does not run TaskManager commands, BalancePortalValidate, saves, or restarts.<br><br>"
				+ String.Format( "{0} duration={1}m interval={2}m captures={3} {4}",
					ColorLabeledValue( "active", snapshot.LongRunActive.ToString(), snapshot.LongRunActive ? "66CCFF" : "CCCCCC" ),
					snapshot.LongRunDurationMinutes,
					snapshot.LongRunIntervalMinutes,
					snapshot.LongRunCaptures,
					ColorLabeledValue( "errors", snapshot.LongRunErrors.ToString(), snapshot.LongRunErrors == 0 ? ColorGood : ColorBad ) ) );

			AddPanel( 24, 290, 486, 280, "Schedule",
				String.Format( "Started UTC: {0}<br>Target end UTC: {1}<br>Remaining: {2}<br>Next capture UTC: {3}<br>Latest file: {4}",
					ColorOptionalValue( FormatDate( snapshot.LongRunStartedUtc ) ),
					ColorOptionalValue( FormatDate( snapshot.LongRunEndUtc ) ),
					ColorCountdown( snapshot.LongRunRemainingSeconds ),
					ColorOptionalValue( FormatDate( snapshot.LongRunNextCaptureUtc ) ),
					ColorOptionalValue( MiddleEllipsis( snapshot.LastReportPath, 64 ) ) ) );

			AddPanel( 530, 290, 486, 280, "Capture Options",
				String.Format( "{0}<br>{1}<br>{2}<br>{3}<br><br>These marker options are diagnostic annotations; they do not hook or alter natural timers.",
					ColorOnOff( "Startup marker", snapshot.LongRunCaptureStartupMarker, true ),
					ColorOnOff( "TaskManager windows", snapshot.LongRunCaptureTaskWindows, true ),
					ColorOnOff( "AutoSave windows", snapshot.LongRunCaptureAutoSaveWindows, true ),
					ColorOnOff( "End summary", snapshot.LongRunEndSummary, true ) ) );

			AddButtonWithLabel( 42, 600, ButtonLongRunDuration, "Duration: " + snapshot.LongRunDurationMinutes + "m", 1152 );
			AddButtonWithLabel( 245, 600, ButtonLongRunInterval, "Interval: " + snapshot.LongRunIntervalMinutes + "m", 1152 );
			AddButtonWithLabel( 455, 600, ButtonLongRunStart, "Start Long-Run", HueGood );
			AddButtonWithLabel( 680, 600, ButtonLongRunStop, "Stop Long-Run", 1152 );

			AddButtonWithLabel( 42, 650, ButtonLongRunStartup, "Startup Marker: " + OnOff( snapshot.LongRunCaptureStartupMarker ), snapshot.LongRunCaptureStartupMarker ? HueGood : HueBad );
			AddButtonWithLabel( 270, 650, ButtonLongRunTaskWindows, "Task Windows: " + OnOff( snapshot.LongRunCaptureTaskWindows ), snapshot.LongRunCaptureTaskWindows ? HueGood : HueBad );
			AddButtonWithLabel( 500, 650, ButtonLongRunAutoSave, "AutoSave Windows: " + OnOff( snapshot.LongRunCaptureAutoSaveWindows ), snapshot.LongRunCaptureAutoSaveWindows ? HueGood : HueBad );
			AddButtonWithLabel( 760, 650, ButtonLongRunSummary, "Summary: " + OnOff( snapshot.LongRunEndSummary ), snapshot.LongRunEndSummary ? HueGood : HueBad );
		}

		private void AddReports( TaskManagerTestController.Snapshot snapshot )
		{
			AddPanel( 24, 100, 486, 170, "Report Output",
				String.Format( "Output folder:<br>{0}<br>Latest LagReport:<br>{1}<br>Latest summary:<br>{2}",
					ColorOptionalValue( MiddleEllipsis( snapshot.OutputFolder, 54 ) ),
					ColorOptionalValue( MiddleEllipsis( snapshot.LastReportPath, 54 ) ),
					ColorOptionalValue( MiddleEllipsis( snapshot.LastSummaryPath, 54 ) ) ) );

			AddPanel( 530, 100, 486, 170, "Operations Health",
				"Save, Console Transcript, and RuneBag health and controls are on Operations.<br><br>Verbose details remain in LagReport and generated summaries." );

			AddButtonWithLabel( 42, 288, ButtonGenerateLagReport, "Generate LagReport", HueGood );
			AddButtonWithLabel( 290, 288, ButtonGenerateSummary, "Generate Summary", 1152 );
			AddButtonWithLabel( 535, 288, ButtonClearReportHistory, "Clear History", 1152 );

			AddPanel( 24, 330, 992, 250, "Daily Attribution", BuildDailyAttributionReportText() );

			StringBuilder reports = new StringBuilder();
			int start = Math.Max( 0, snapshot.ReportFiles.Length - 5 );

			if ( start >= snapshot.ReportFiles.Length )
				reports.Append( "No generated reports are currently displayed." );
			else
			{
				for ( int i = start; i < snapshot.ReportFiles.Length; ++i )
					reports.AppendFormat( "{0}. {1}<br>", i + 1, ColorOptionalValue( MiddleEllipsis( snapshot.ReportFiles[i], 112 ) ) );
			}

			AddPanel( 24, 595, 992, 225, "Recent Generated Reports", reports.ToString() );
		}

		private void AddOperations()
		{
			SaveDiagnostics.Snapshot save = SaveDiagnostics.GetSnapshot();
			ConsoleTranscript.Snapshot console = ConsoleTranscript.GetSnapshot();
			RuneBagDiagnostics.Snapshot runeBag = RuneBagDiagnostics.GetSnapshot();

			AddPanel( 24, 100, 486, 270, "Save Diagnostics", BuildSaveDiagnosticsText( save ) );
			AddPanel( 530, 100, 486, 270, "Console Transcript", BuildConsoleTranscriptText( console ) );

			AddButtonWithLabel( 42, 385, ButtonSaveAggregate, "Save Aggregate", 1152 );
			AddButtonWithLabel( 190, 385, ButtonSaveDeep, "Save Deep", 1152 );
			AddButtonWithLabel( 305, 385, ButtonSaveDeepNext, "Deep Next", 1152 );
			AddButtonWithLabel( 430, 385, ButtonSaveReset, "Save Reset", 1152 );
			AddButtonWithLabel( 625, 385, ButtonConsoleFlush, "Flush Log", 1152 );
			AddButtonWithLabel( 790, 385, ButtonConsoleRotate, "Rotate Log", 1152 );

			AddPanel( 24, 435, 992, 335, "RuneBag Diagnostics", BuildRuneBagDiagnosticsText( runeBag ) );
			AddButtonWithLabel( 42, 790, ButtonRuneBagReset, "RuneBag Reset", 1152 );
		}

		private void AddNetworkDiagnostics()
		{
			NetFlightDiagnosticsSnapshot network = NetFlightRecorder.GetDiagnosticsSnapshot();

			AddPanel( 24, 100, 992, 125, "Automatic Per-Player Network Capture",
				"Every connected player is monitored independently. Bounded rolling packet history is dumped automatically for stalls, backlogs, packet storms, integrity failures, and disconnect/relog events.<br>"
				+ "This is passive diagnostics only. It does not disconnect, throttle, repair, or otherwise alter a player session." );

			AddButtonWithLabel( 42, 240, ButtonNetFlightStatus, "Detailed Status", 1152 );

			if ( network != null && !network.Enabled )
				AddButtonWithLabel( 245, 240, ButtonNetFlightEnable, "Enable Capture", HueGood );

			AddPanel( 24, 280, 486, 245, "Recorder Health", BuildNetFlightHealthText( network ) );
			AddPanel( 530, 280, 486, 245, "Most Concerning Sessions", BuildNetFlightSessionText( network ) );
			AddPanel( 24, 540, 992, 280, "Recent Per-Player Dumps", BuildNetFlightRecentDumpText( network ) );
		}

		private static string BuildNetFlightHealthText( NetFlightDiagnosticsSnapshot network )
		{
			if ( network == null )
				return "NetFlight diagnostics: Unavailable";

			return String.Format(
				"{0}<br>Sessions: {1} / healthy {2} / warning {3} / critical {4}<br>"
				+ "Dumps: total {5} / automatic {6} / disconnect {7} / manual {8}<br>"
				+ "Flush: queue {9} / dropped {10} / snapshot dropped {11} / lag {12}ms<br>"
				+ "Writer: format {13}ms / write {14}ms / error {15}<br>Output: {16}",
				ColorBoolHealthyWhenTrue( "Automatic capture", network.Enabled ),
				network.ConnectedSessions,
				ColorValue( network.HealthySessions.ToString(), ColorGood ),
				ColorValue( network.WarningSessions.ToString(), network.WarningSessions > 0 ? "FFCC66" : ColorGood ),
				ColorValue( network.CriticalSessions.ToString(), network.CriticalSessions > 0 ? ColorBad : ColorGood ),
				network.TotalDumps,
				network.AutomaticDumps,
				network.DisconnectDumps,
				network.ManualDumps,
				network.FlushQueueDepth,
				ColorValue( network.FlushDropped.ToString(), network.FlushDropped == 0 ? ColorGood : ColorBad ),
				ColorValue( network.SnapshotDropped.ToString(), network.SnapshotDropped == 0 ? ColorGood : ColorBad ),
				network.FlushLagMs,
				network.LastFormatDurationMs,
				network.LastWriteDurationMs,
				ColorError( network.LastWriterError ),
				ColorOptionalValue( MiddleEllipsis( network.OutputFolder, 48 ) ) );
		}

		private static string BuildNetFlightSessionText( NetFlightDiagnosticsSnapshot network )
		{
			if ( network == null || network.Clients == null || network.Clients.Length == 0 )
				return "No connected player sessions.";

			StringBuilder text = new StringBuilder( 768 );
			int take = Math.Min( 4, network.Clients.Length );

			for ( int i = 0; i < take; ++i )
			{
				NetFlightClientDiagnosticsSnapshot client = network.Clients[i];
				string healthColor = client.HealthScore >= 2 ? ColorBad : ( client.HealthScore == 1 ? "FFCC66" : ColorGood );
				text.AppendFormat(
					"{0} {1} / {2} / session {3}<br>pending {4}, receive {5}, send age {6}, in/out {7}/{8}, UltimaLive {9}/{10}<br>",
					ColorValue( client.Health ?? "UNKNOWN", healthColor ),
					Encode( ShortSingleLineText( client.Character, 22 ) ),
					Encode( ShortSingleLineText( client.Account, 20 ) ),
					client.SessionId,
					FormatCompactBytes( client.PendingBytes ),
					FormatCompactBytes( client.ReceiveQueueBytes ),
					client.SendCompletionAgeMs < 0 ? "n/a" : client.SendCompletionAgeMs + "ms",
					client.InboundPackets10s,
					client.OutboundPackets10s,
					client.InboundUltimaLive60s,
					client.OutboundUltimaLive60s );
			}

			if ( network.Clients.Length > take )
				text.AppendFormat( "{0} more connected sessions are included in Detailed Status and LagReport.", network.Clients.Length - take );

			return text.ToString();
		}

		private static string BuildNetFlightRecentDumpText( NetFlightDiagnosticsSnapshot network )
		{
			if ( network == null || network.RecentDumpPaths == null || network.RecentDumpPaths.Length == 0 )
				return "No NetFlight dumps have been generated since this server start.";

			StringBuilder text = new StringBuilder( 768 );
			int start = Math.Max( 0, network.RecentDumpPaths.Length - 7 );

			for ( int i = start; i < network.RecentDumpPaths.Length; ++i )
				text.AppendFormat( "{0}. {1}<br>", i + 1, ColorOptionalValue( MiddleEllipsis( network.RecentDumpPaths[i], 112 ) ) );

			text.Append( "Full per-player packet windows and metrics are stored in these plain-text files.<br>" );
			text.Append( "Files contain account names, character names, IP addresses, packet identifiers, and exception details. Sanitize before public sharing." );
			return text.ToString();
		}

		private static string BuildSaveDiagnosticsText( SaveDiagnostics.Snapshot save )
		{
			if ( save == null )
				return "Save diagnostics: Unavailable";

			StringBuilder text = new StringBuilder( 512 );
			text.AppendFormat( "Modes: {0} / {1} | {2}/{3}<br>", ColorValue( save.ConfiguredMode.ToString(), "CCCCCC" ), ColorValue( save.EffectiveMode.ToString(), "CCCCCC" ), ColorBoolHealthyWhenFalse( "Active", save.SaveActive ), ColorOptionalValue( save.ActiveMode ) );
			text.AppendFormat( "{0} | {1}<br>", ColorLabeledValue( "Deep Next", save.DeepNextSaveRequested ? "requested" : "none", save.DeepNextSaveRequested ? "FFCC66" : "CCCCCC" ), ColorBoolHealthyWhenTrue( "Config load", save.ConfigurationLoaded ) );
			text.AppendFormat( "Config load error: {0} | Config write error: {1}<br>", ColorError( ShortSingleLineText( save.ConfigurationError, 12 ) ), ColorError( ShortSingleLineText( save.ConfigurationSaveError, 12 ) ) );
			text.AppendFormat( "Config path: {0} | Saves: {1}<br>", ColorOptionalValue( MiddleEllipsis( save.ConfigurationPath, 28 ) ), FormatCompactCount( save.SaveCount ) );

			if ( !save.LastAvailable )
			{
				text.Append( "Last save: Not sampled<br>" );
			}
			else
			{
				text.AppendFormat( "Last: {0} / {1} ms<br>", ColorValue( save.LastMode.ToString(), "CCCCCC" ), FormatCompactCount( save.LastTotalMs ) );
				text.AppendFormat( "Timing S/I/M/G (ms): {0}/{1}/{2}/{3}<br>", FormatCompactCount( save.LastStrategyMs ), FormatCompactCount( save.LastItemsMs ), FormatCompactCount( save.LastMobilesMs ), FormatCompactCount( save.LastGuildsMs ) );
				text.AppendFormat( "Entities deep/aggregate: {0}/{1}<br>", FormatCompactCount( save.LastDeepProfiledEntities ), FormatCompactCount( save.LastAggregateOnlyEntities ) );
			}

			text.AppendFormat( "Last error: {0}", ColorError( ShortSingleLineText( save.LastError, 28 ) ) );
			return text.ToString();
		}

		private static string BuildConsoleTranscriptText( ConsoleTranscript.Snapshot console )
		{
			if ( console == null )
				return "Console transcript: Unavailable";

			return String.Format(
				"{0} / {1} / {2}<br>File: {3} / part: {4}<br>Written: {5}<br>"
				+ "Queue: {6} chunks/{7}; peak: {8} chunks/{9}<br>"
				+ "Dropped: {10} chunks/{11}; write errors: {12}<br>Last error: {13}<br>"
				+ "Last flush UTC: {14} / flush/rotate {15}/{16}<br>"
				+ "Privacy: IP, account, character name, local path, and exception/error details; sanitize before sharing.",
				ColorBoolHealthyWhenTrue( "Enabled", console.Enabled ),
				ColorBoolHealthyWhenTrue( "Initialized", console.Initialized ),
				ColorBoolHealthyWhenTrue( "Writer alive", console.WriterAlive ),
				ColorOptionalValue( MiddleEllipsis( console.CurrentFile, 28 ) ),
				console.CurrentPart,
				FormatCompactBytes( console.BytesWritten ),
				FormatCompactCount( console.QueuedChunks ),
				FormatCompactBytes( console.QueuedBytes ),
				FormatCompactCount( console.MaxQueuedChunks ),
				FormatCompactBytes( console.MaxQueuedBytes ),
				ColorValue( FormatCompactCount( console.DroppedChunks ), console.DroppedChunks == 0 ? ColorGood : ColorBad ),
				ColorValue( FormatCompactBytes( console.DroppedBytes ), console.DroppedBytes == 0 ? ColorGood : ColorBad ),
				ColorValue( FormatCompactCount( console.WriteErrors ), console.WriteErrors == 0 ? ColorGood : ColorBad ),
				ColorError( ShortText( console.LastError, 32 ) ),
				ColorOptionalValue( FormatUtc( console.LastFlushUtc ) ),
				FormatCompactCount( console.FlushCount ),
				FormatCompactCount( console.Rotations ) );
		}

		private static string BuildRuneBagDiagnosticsText( RuneBagDiagnostics.Snapshot runeBag )
		{
			if ( runeBag == null )
				return "RuneBag diagnostics: Unavailable";

			return String.Format(
				"Attempts: {0:N0} / allowed: {1:N0}<br>"
				+ "Throttled: {2} total<br>"
				+ "Throttle breakdown: cooldown {3:N0} / casting {4:N0} / targeting {5:N0} / in progress {6:N0}<br>"
				+ "Invalid: {7:N0} / failed normal checks: {8:N0}<br>"
				+ "Casts: started {9:N0} / failed {10:N0}<br>"
				+ "Completion: {11}<br>"
				+ "Timing: total {12:N0}ms / average {13:N0}ms / maximum {14:N0}ms / slow {15:N0}<br>"
				+ "Active guards: {16:N0} / throttle states: {17:N0}<br>"
				+ "Tracked: players {18:N0} / items {19:N0}<br>"
				+ "Pruned states: {20:N0} / errors: {21}<br>"
				+ "Last throttle UTC: {22}",
				runeBag.Attempts,
				runeBag.Allowed,
				ColorValue( runeBag.Throttled.ToString( "N0" ), runeBag.Throttled == 0 ? "CCCCCC" : "FFCC66" ),
				runeBag.ThrottledByCooldown,
				runeBag.ThrottledAlreadyCasting,
				runeBag.ThrottledAlreadyTargeting,
				runeBag.ThrottledInProgress,
				runeBag.InvalidState,
				runeBag.FailedNormalChecks,
				runeBag.CastsStarted,
				runeBag.CastsFailed,
				ColorAvailability( "Unavailable" ),
				runeBag.TotalMs,
				runeBag.AverageMs,
				runeBag.MaxMs,
				runeBag.SlowCalls,
				runeBag.ActiveGuards,
				runeBag.ThrottleTableSize,
				runeBag.TrackedPlayers,
				runeBag.TrackedItems,
				runeBag.StaleEntriesPruned,
				ColorValue( runeBag.DiagnosticErrors.ToString( "N0" ), runeBag.DiagnosticErrors == 0 ? ColorGood : ColorBad ),
				ColorOptionalValue( FormatDate( runeBag.LastThrottleUtc ) ) );
		}

		private static string BuildDailyAttributionReportText()
		{
			TaskManagerDailyAttributionDiagnostics.PerformanceSummary daily = TaskManagerDailyAttributionDiagnostics.GetPerformanceSummary();

			if ( !daily.Available )
				return "Daily attribution: Not sampled";

			return String.Format(
				"Run ID: {0} / {1} / {2} / total wall {3}ms<br>Top: {4} {5}ms<br>"
				+ "Actual staged callbacks: {6} / max {7}ms / {8} / {9}<br>"
				+ "Village callbacks: {10} / max {11}ms / wall {12}ms<br>"
				+ "City callbacks: {13} / max {14}ms / wall {15}ms<br>"
				+ "Garden callbacks: {16} / max {17}ms / wall {18}ms; placement {19} / max {20}ms / wall {21}ms<br>"
				+ "Main totals: village setup {22}ms / constructor {23}ms / other {24}ms / cities {25}ms / gardens {26}ms<br>"
				+ "Other phases: spread cleanup {27}ms / stealing pedestals {28}ms<br>"
				+ "Full attribution is available in LagReport and the generated summary.",
				daily.RunId,
				!String.IsNullOrWhiteSpace( daily.CompletionMode ) ? daily.CompletionMode : "unknown",
				!String.IsNullOrWhiteSpace( daily.CompletionState ) ? daily.CompletionState : "unknown",
				daily.TotalMs,
				!String.IsNullOrWhiteSpace( daily.TopGroup ) ? daily.TopGroup : "none",
				daily.TopGroupMs,
				daily.StagedCallbackCount,
				daily.StagedCallbackMaxMs,
				!String.IsNullOrWhiteSpace( daily.StagedCallbackMaxKind ) ? daily.StagedCallbackMaxKind : "none",
				!String.IsNullOrWhiteSpace( daily.StagedCallbackMaxName ) ? MiddleEllipsis( daily.StagedCallbackMaxName, 56 ) : "none",
				daily.VillageChunkCount,
				daily.VillageChunkMaxMs,
				daily.VillageStagedWallMs,
				daily.CityChunkCount,
				daily.CityChunkMaxMs,
				daily.CityStagedWallMs,
				daily.GardenChunkCount,
				daily.GardenChunkMaxMs,
				daily.GardenStagedWallMs,
				daily.GardenPlacementCallbackCount,
				daily.GardenPlacementCallbackMaxMs,
				daily.GardenPlacementWallMs,
				daily.PopulateVillagesSetupSpawnMs,
				daily.VillageConstructorMs,
				daily.VillageNonConstructorMs,
				daily.PopulateCitiesMs,
				daily.PlantGardensMs,
				daily.SpreadOutCreatureCleanupMs,
				daily.StealingPedestalsMs );
		}

		private void AddSettings( TaskManagerTestController.Snapshot snapshot )
		{
			AddPanel( 24, 100, 992, 85, "In-Memory Settings", "Settings reset on reboot and do not change TaskManager, SpawnEntry, or save behavior." );
			AddSettingRow( 30, 205, "Daily settling", snapshot.DailySettlingSeconds, ButtonDailyMinus, ButtonDailyPlus );
			AddSettingRow( 30, 245, "150 settling", snapshot.Settling150Seconds, Button150Minus, Button150Plus );
			AddSettingRow( 30, 285, "250 settling", snapshot.Settling250Seconds, Button250Minus, Button250Plus );
			AddSettingRow( 30, 325, "Hourly settling", snapshot.HourlySettlingSeconds, ButtonHourlyMinus, ButtonHourlyPlus );
			AddSettingRow( 30, 365, "200 settling", snapshot.Settling200Seconds, Button200Minus, Button200Plus );

			AddToggleRow( 530, 205, "Include hourly smoke test", snapshot.IncludeHourly, true, ButtonToggleHourly );
			AddToggleRow( 530, 245, "Include 200-minute smoke test", snapshot.Include200, true, ButtonToggle200 );
			AddToggleRow( 530, 285, "Include BalancePortalValidate", snapshot.IncludeBalancePortalValidate, false, ButtonToggleValidate );
			AddToggleRow( 530, 325, "LagReport at checkpoints", snapshot.AutoGenerateLagReportEveryStep, true, ButtonToggleAutoReports );
			AddToggleRow( 530, 365, "Generate final summary", snapshot.AutoGenerateFinalSummary, true, ButtonToggleAutoSummary );

			AddPanel( 24, 425, 992, 310, "Defaults and Limits",
				"Settling adjustments use 5-second steps and remain bounded from 0 to 600 seconds.<br><br>"
				+ "Long-run duration choices: 30m, 1h, 2h, 6h, 8h, 12h, 24h.<br>"
				+ "Long-run interval choices: 1m, 2m, 5m, 10m.<br><br>"
				+ "<basefont color=#FFCC66>BalancePortalValidate selection still requires an explicit confirmation before execution.</basefont>" );
		}

		private void AddHelp()
		{
			AddPanel( 24, 100, 486, 635, "Safety Warnings",
				"Administrator-only.<br><br>"
				+ "Manual TaskManager tests mutate world state.<br><br>"
				+ "Daily updates LastRun and shifts the next natural daily run.<br><br>"
				+ "Repeated manual daily runs are not identical benchmarks.<br><br>"
				+ "The 150-minute test causes downstream SpawnEntry callbacks for roughly 20-30 seconds.<br><br>"
				+ "BalancePortalValidate intentionally recreates the old expensive full scan.<br><br>"
				+ "Cancellation cannot interrupt a synchronous task already executing." );
			AddPanel( 530, 100, 486, 635, "Operating Notes",
				"Long-run diagnostics can generate many small files.<br><br>"
				+ "The suite never automatically saves or restarts the server.<br><br>"
				+ "Save and restart validation remain manual.<br><br>"
				+ "Automatic per-player network capture and recent NetFlight files are shown on Network.<br><br>"
				+ "Save Diagnostics, Console Transcript, and RuneBag controls live on Operations.<br><br>"
				+ "Closing this Gump does not stop an active test. Reopen it with [TestSuite.<br><br>"
				+ "Performance Test Suite commands share this controller's in-flight and settling guards.<br><br>"
				+ "Generated reports contain detailed diagnostics; this Gump intentionally stays compact." );
		}

		private void AddPanel( int x, int y, int width, int height, string title, string body )
		{
			AddImageTiled( x, y, width, height, 2624 );
			AddAlphaRegion( x + 6, y + 6, width - 12, height - 12 );
			AddLabel( x + 16, y + 10, 1152, title );
			AddHtml( x + 16, y + 42, width - 32, height - 62, Html( body ), false, false );
		}

		private void AddActionBox( int x, int y, int width, int height, string title, string body, int buttonId )
		{
			AddImageTiled( x, y, width, height, 2624 );
			AddAlphaRegion( x + 6, y + 6, width - 12, height - 12 );
			AddLabel( x + 16, y + 10, 1152, title );
			AddHtml( x + 16, y + 42, width - 32, height - 94, Html( body ), false, false );
			AddButton( x + 16, y + height - 38, 4005, 4007, buttonId, GumpButtonType.Reply, 0 );
			AddLabel( x + 48, y + height - 36, HueGood, "Start" );
		}

		private void AddSettingRow( int x, int y, string label, int seconds, int minusButton, int plusButton )
		{
			AddLabel( x, y + 2, HueGood, label );
			AddButton( x + 210, y, 4014, 4016, minusButton, GumpButtonType.Reply, 0 );
			AddLabel( x + 250, y + 2, HueGood, seconds + "s" );
			AddButton( x + 315, y, 4005, 4007, plusButton, GumpButtonType.Reply, 0 );
		}

		private void AddToggleRow( int x, int y, string label, bool enabled, bool selectedIsGood, int buttonId )
		{
			AddButton( x, y, enabled ? 2154 : 2151, enabled ? 2151 : 2154, buttonId, GumpButtonType.Reply, 0 );
			AddHtml( x + 34, y, 440, 22, ColorOnOff( label, enabled, selectedIsGood ), false, false );
		}

		private void AddButtonWithLabel( int x, int y, int buttonId, string text, int hue )
		{
			AddButton( x, y, 4005, 4007, buttonId, GumpButtonType.Reply, 0 );
			AddLabel( x + 32, y + 2, hue, text );
		}

		private static string BuildStatusText( TaskManagerTestController.Snapshot snapshot )
		{
			return String.Format(
				"State: {0}<br>Module: {1}<br>Phase: {2}<br>Step: {3}/{4}<br>Current run ID: {5}<br>Elapsed: {6}<br>{7}<br>{8}<br>{9}",
				ColorState( snapshot.State, snapshot.LongRunActive ),
				ColorOptionalValue( snapshot.ActiveTestName ),
				ColorOptionalValue( snapshot.ActivePhase ),
				snapshot.StepNumber,
				snapshot.StepCount,
				ColorAvailability( GetCurrentRunId( snapshot.CurrentFamily ) ),
				FormatDuration( snapshot.ElapsedSeconds ),
				ColorBoolHealthyWhenFalse( "In flight", snapshot.InFlight ),
				ColorBoolHealthyWhenFalse( "Settling", snapshot.Settling ),
				ColorBoolHealthyWhenFalse( "World saving", World.Saving ) );
		}

		private static string GetCurrentRunId( string family )
		{
			string taskName = null;

			if ( family == "daily" )
				taskName = "TaskManagerDaily";
			else if ( family == "150" )
				taskName = "TaskManager150Min";
			else if ( family == "200" )
				taskName = "TaskManager200Min";
			else if ( family == "250" )
				taskName = "TaskManager250Min";
			else if ( family == "hourly" )
				taskName = "TaskManager";

			if ( taskName == null )
				return "Unavailable";

			long runId;
			long duration;
			string timerName;
			DateTime startedUtc;
			string topPhase;

			return TaskManagerDiagnostics.TryGetLatestRunInfo( taskName, out runId, out duration, out timerName, out startedUtc, out topPhase ) ? runId.ToString() : "Unavailable";
		}

		private static string BuildHealthText( TaskManagerTestController.Snapshot snapshot )
		{
			BalancePortalDiagnostics.Sample balance;
			bool hasBalance = BalancePortalDiagnostics.TryGetLatestSample( out balance );
			SpawnEntryDiagnostics.PerformanceSummary spawn = SpawnEntryDiagnostics.GetPerformanceTestSummary();
			long oldCallbacks;
			long oldestReady;
			PremiumSpawner.GetPerformanceTestSummary( out oldCallbacks, out oldestReady );
			int physicalScans;
			long collectMs;
			Statistics.GetPerformanceTestSummary( out physicalScans, out collectMs );
			long discordTimeouts;
			long discordErrors;
			DiscordBridge.GetPerformanceTestSummary( out discordTimeouts, out discordErrors );
			string taskTop = GetLatestTaskManagerTop();
			TaskManagerDailyAttributionDiagnostics.PerformanceSummary daily = TaskManagerDailyAttributionDiagnostics.GetPerformanceSummary();
			string dailyTop = daily.Available
				? String.Format(
					"Daily wall: {0}ms / callbacks: {1}<br>Daily top: {2} {3}ms<br>Actual max: {4}ms / Village: {5}ms / City: {6}ms / Garden placement: {7}ms",
					daily.TotalMs,
					daily.StagedCallbackCount,
					!String.IsNullOrWhiteSpace( daily.TopGroup ) ? daily.TopGroup : "none",
					daily.TopGroupMs,
					daily.StagedCallbackMaxMs,
					daily.VillageChunkMaxMs,
					daily.CityChunkMaxMs,
					daily.GardenPlacementCallbackCount > 0 ? daily.GardenPlacementCallbackMaxMs : daily.GardenChunkMaxMs )
				: "Daily attribution: Not sampled";

			string balanceText = hasBalance ? ColorMetricGoodWarnFail( "BalancePortal", balance.TotalPhaseMs, 500, 2000, "ms" ) : ColorLabeledValue( "BalancePortal", "Unavailable", "CCCCCC" );
			string spawnText = String.Format( "Spawn chains: {0} active / {1} complete / {2} tracked", spawn.ActiveOriginChains, spawn.CompletedOriginChains, spawn.TrackedOriginChains );

			return String.Format(
				"{0}<br>Timer.Slice max: {1}ms<br>TaskManager latest top: {2}<br>{3}<br>Last manual: {4}, {5}ms<br>{6}<br>{7}<br>{8}<br>{9}<br>{10}",
				ColorMetricGoodWarnFail( "Partial sends", NetState.PartialSends, 0, Int64.MaxValue, String.Empty ),
				Timer.MaxSliceDuration,
				ColorOptionalValue( taskTop ),
				ColorOptionalValue( dailyTop ),
				ColorOptionalValue( snapshot.LastManualCommand ),
				snapshot.LastManualElapsedMs,
				balanceText,
				spawnText,
				ColorPremiumSpawner( oldestReady, oldCallbacks ),
				ColorCollectStats( collectMs, physicalScans ),
				ColorDiscord( discordTimeouts, discordErrors ) );
		}

		private static string GetLatestTaskManagerTop()
		{
			string[] names = new string[] { "TaskManagerDaily", "TaskManager150Min", "TaskManager200Min", "TaskManager250Min", "TaskManager" };
			string topName = "Unavailable";
			string topPhase = "Unavailable";
			long topMs = -1;

			for ( int i = 0; i < names.Length; ++i )
			{
				long runId;
				long duration;
				string timerName;
				DateTime startedUtc;
				string phase;

				if ( TaskManagerDiagnostics.TryGetLatestRunInfo( names[i], out runId, out duration, out timerName, out startedUtc, out phase ) && duration > topMs )
				{
					topName = names[i];
					topPhase = phase;
					topMs = duration;
				}
			}

			return topMs >= 0 ? String.Format( "{0} {1}ms ({2})", topName, topMs, topPhase ) : "Unavailable";
		}

		private static string BuildResultsText( TaskManagerTestController.Snapshot snapshot )
		{
			if ( snapshot.Results == null || snapshot.Results.Length == 0 )
				return "No evaluated module result is available yet.";

			StringBuilder text = new StringBuilder();
			text.AppendFormat( "Overall: {0}<br><br>", ColorResult( snapshot.OverallResult ) );

			AppendResult( text, FindResult( snapshot.Results, "Partial sends" ) );
			AppendResult( text, FindResult( snapshot.Results, "balanceChampionAndPortals" ) );
			AppendResult( text, FindResult( snapshot.Results, "TaskManagerDaily" ) );
			AppendResult( text, FindResult( snapshot.Results, "TaskManager150Min" ) );
			AppendResult( text, FindResult( snapshot.Results, "TaskManager250Min" ) );
			AppendSpawnEntryResult( text, snapshot.Results );
			AppendResult( text, FindResult( snapshot.Results, "PremiumSpawner max constructor" ) );
			AppendResult( text, FindResult( snapshot.Results, "PremiumSpawner top constructor type by total" ) );
			text.Append( "<br>Full evaluation is in the generated summary file." );

			return text.ToString();
		}

		private static string BuildProgressText( TaskManagerTestController.Snapshot snapshot )
		{
			return String.Format(
				"Last completed: {0}<br>Next step: {1}<br>Settling remaining: {2}<br>Last error: {3}<br><br>Latest report: {4}<br>Latest summary: {5}<br>Output folder: {6}",
				ColorOptionalValue( snapshot.LastCompletedStep ),
				ColorOptionalValue( snapshot.NextStep ),
				ColorCountdownSeconds( snapshot.SettlingSecondsRemaining ),
				ColorError( snapshot.LastError ),
				ColorOptionalValue( FileNameOnly( snapshot.LastReportPath ) ),
				ColorOptionalValue( FileNameOnly( snapshot.LastSummaryPath ) ),
				ColorOptionalValue( FileNameOnly( snapshot.OutputFolder ) ) );
		}

		private static string BuildSettlingGuardText( TaskManagerTestController.Snapshot snapshot )
		{
			return String.Format( "{0}<br>Family: {1}<br>Remaining: {2}<br>{3}",
				ColorBoolHealthyWhenFalse( "Active", snapshot.Settling ),
				ColorOptionalValue( snapshot.SettlingFamily ),
				ColorCountdownSeconds( snapshot.SettlingSecondsRemaining ),
				ColorOptionalValue( snapshot.SettlingReason ) );
		}

		private static string BuildStateBanner( TaskManagerTestController.Snapshot snapshot )
		{
			string text;

			if ( snapshot.Settling )
			{
				text = String.Format( "{0} - DOWNSTREAM TIMERS ARE STILL RUNNING - {1}",
					ColorValue( "SETTLING", "FFCC66" ),
					ColorCountdownSeconds( snapshot.SettlingSecondsRemaining ) );
			}
			else if ( snapshot.State == "CapturingLagReport" )
			{
				text = ColorValue( "CAPTURING REPORT", "66CCFF" );
			}
			else if ( snapshot.LongRunActive )
			{
				text = ColorValue( "LONG-RUN ACTIVE", "66CCFF" ) + " - NEXT CAPTURE " + ColorCountdown( GetLongRunNextCaptureSeconds( snapshot ) );
			}
			else if ( snapshot.State == "Running" || snapshot.InFlight )
			{
				text = ColorValue( "TEST RUNNING", "66CCFF" ) + " - DO NOT START ANOTHER TEST";
			}
			else if ( snapshot.State == "Complete" )
			{
				text = "TEST COMPLETE - OVERALL " + ColorResult( snapshot.OverallResult );
			}
			else if ( snapshot.State == "Failed" )
			{
				text = ColorValue( "TEST FAILED", ColorBad ) + " - " + Encode( ShortText( snapshot.LastError, 80 ) );
			}
			else if ( snapshot.State == "Cancelled" )
			{
				text = ColorValue( "TEST CANCELLED", "FFCC66" ) + " - NO ROLLBACK WAS PERFORMED";
			}
			else
			{
				text = ColorValue( "IDLE", "CCCCCC" );
			}

			return "<basefont color=#FFFFFF><center><big>" + text + "</big></center></basefont>";
		}

		private static string Html( string text )
		{
			return "<basefont color=#FFFFFF>" + ( text ?? String.Empty );
		}

		private static string Encode( string text )
		{
			if ( String.IsNullOrEmpty( text ) )
				return "none";

			return text.Replace( "&", "&amp;" ).Replace( "<", "&lt;" ).Replace( ">", "&gt;" );
		}

		private static string ShortText( string text, int max )
		{
			if ( String.IsNullOrEmpty( text ) )
				return "none";

			if ( text.Length <= max )
				return text;

			return text.Substring( 0, max - 3 ) + "...";
		}

		private static string ShortSingleLineText( string text, int max )
		{
			if ( !String.IsNullOrEmpty( text ) )
				text = text.Replace( '\r', ' ' ).Replace( '\n', ' ' );

			return ShortText( text, max );
		}

		private static string FileNameOnly( string path )
		{
			if ( String.IsNullOrEmpty( path ) || path == "none" )
				return "none";

			try
			{
				string name = Path.GetFileName( path.TrimEnd( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar ) );
				return !String.IsNullOrEmpty( name ) ? name : path;
			}
			catch
			{
				return path;
			}
		}

		private static string MiddleEllipsis( string path, int max )
		{
			if ( String.IsNullOrEmpty( path ) )
				return "none";

			if ( path.Length <= max )
				return path;

			string fileName = FileNameOnly( path );
			int prefixLength = max - fileName.Length - 4;

			if ( prefixLength < 8 )
				return ShortText( path, max );

			return path.Substring( 0, prefixLength ) + "...\\" + fileName;
		}

		private static string OnOff( bool value )
		{
			return value ? "ON" : "off";
		}

		private static string FormatDate( DateTime value )
		{
			return value == DateTime.MinValue ? "Unavailable" : value.ToString();
		}

		private static string FormatUtc( DateTime value )
		{
			return value == DateTime.MinValue ? "Unavailable" : value.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm" );
		}

		private static string FormatDuration( long seconds )
		{
			if ( seconds < 0 )
				seconds = 0;

			TimeSpan duration = TimeSpan.FromSeconds( seconds );
			return String.Format( "{0:00}:{1:00}:{2:00}", (int)duration.TotalHours, duration.Minutes, duration.Seconds );
		}

		private static string FormatCompactCount( long value )
		{
			if ( value < 0 )
				value = 0;

			if ( value < 1000 )
				return value.ToString();
			if ( value < 1000000 )
				return String.Format( "{0:0.#}K", value / 1000.0 );
			if ( value < 1000000000 )
				return String.Format( "{0:0.#}M", value / 1000000.0 );
			if ( value < 1000000000000L )
				return String.Format( "{0:0.#}G", value / 1000000000.0 );

			return String.Format( "{0:0.#}T", value / 1000000000000.0 );
		}

		private static string FormatCompactBytes( long value )
		{
			if ( value < 0 )
				value = 0;

			if ( value < 1024 )
				return value + " B";
			if ( value < 1048576 )
				return String.Format( "{0:0.#} KB", value / 1024.0 );
			if ( value < 1073741824 )
				return String.Format( "{0:0.#} MB", value / 1048576.0 );
			if ( value < 1099511627776L )
				return String.Format( "{0:0.#} GB", value / 1073741824.0 );

			return String.Format( "{0:0.#} TB", value / 1099511627776.0 );
		}

		private static string ColorValue( string value, string color )
		{
			return String.Format( "<basefont color=#{0}>{1}</basefont>", color, Encode( value ) );
		}

		private static string ColorLabeledValue( string label, string value, string color )
		{
			return Encode( label ) + ": " + ColorValue( value, color );
		}

		private static string ColorBoolHealthyWhenFalse( string label, bool value )
		{
			return ColorLabeledValue( label, value.ToString(), value ? ColorBad : ColorGood );
		}

		private static string ColorBoolHealthyWhenTrue( string label, bool value )
		{
			return ColorLabeledValue( label, value.ToString(), value ? ColorGood : ColorBad );
		}

		private static string ColorOnOff( string label, bool value, bool selectedIsGood )
		{
			string color = value ? ColorGood : ColorBad;
			return ColorValue( label + ": " + OnOff( value ), color );
		}

		private static string ColorState( string state, bool longRunActive )
		{
			string display = longRunActive ? "LONG-RUN ACTIVE" : ( state ?? "Idle" ).ToUpperInvariant();
			return ColorValue( display, longRunActive ? "66CCFF" : GetStateColor( state ) );
		}

		private static string ColorAvailability( string value )
		{
			bool unavailable = String.IsNullOrEmpty( value ) || String.Equals( value, "Unavailable", StringComparison.OrdinalIgnoreCase );
			return ColorValue( unavailable ? "Unavailable" : value, unavailable ? "CCCCCC" : ColorGood );
		}

		private static string ColorOptionalValue( string value )
		{
			if ( String.IsNullOrEmpty( value ) || String.Equals( value, "none", StringComparison.OrdinalIgnoreCase ) )
				return ColorValue( "none", "CCCCCC" );

			if ( String.Equals( value, "Unavailable", StringComparison.OrdinalIgnoreCase ) )
				return ColorValue( "Unavailable", "CCCCCC" );

			if ( String.Equals( value, "Available", StringComparison.OrdinalIgnoreCase ) )
				return ColorValue( "Available", ColorGood );

			return Encode( value );
		}

		private static string GetStateColor( string state )
		{
			if ( state == "Complete" )
				return ColorGood;
			if ( state == "Failed" )
				return ColorBad;
			if ( state == "Cancelled" || state == "Settling" || state == "WaitingForAdmin" )
				return "FFCC66";
			if ( state == "Running" || state == "CapturingLagReport" )
				return "66CCFF";
			return "CCCCCC";
		}

		private static string ColorResult( string result )
		{
			return ColorValue( result ?? "INFO", GetResultColor( result ) );
		}

		private static string ColorMetricGoodWarnFail( string label, long value, long passLimit, long failLimit, string suffix )
		{
			string color = value <= passLimit ? ColorGood : ( value > failLimit ? ColorBad : "FFCC66" );
			return ColorLabeledValue( label, value + suffix, color );
		}

		private static string ColorError( string error )
		{
			bool clear = String.IsNullOrEmpty( error ) || String.Equals( error, "none", StringComparison.OrdinalIgnoreCase );
			return ColorValue( clear ? "none" : ShortText( error, 68 ), clear ? ColorGood : ColorBad );
		}

		private static string ColorPremiumSpawner( long oldestReady, long oldCallbacks )
		{
			string color = oldCallbacks > 0 || oldestReady > 30000 ? ColorBad : ( oldestReady > 15000 ? "FFCC66" : ColorGood );
			return "Premium: oldestReady=" + ColorValue( oldestReady + "ms", color ) + ", oldCallbacks=" + ColorValue( oldCallbacks.ToString(), color );
		}

		private static string ColorCollectStats( long collectMs, int physicalScans )
		{
			string color = physicalScans == 0 ? ColorGood : "FFCC66";
			return "CollectStats: " + ColorValue( collectMs + "ms", color ) + ", physicalItemScans=" + ColorValue( physicalScans.ToString(), color );
		}

		private static string ColorDiscord( long timeouts, long errors )
		{
			string color = timeouts == 0 && errors == 0 ? ColorGood : "FFCC66";
			return "Discord: timeouts=" + ColorValue( timeouts.ToString(), color ) + ", errors=" + ColorValue( errors.ToString(), color );
		}

		private static string ColorCountdown( long seconds )
		{
			return ColorValue( FormatDuration( seconds ), "FFCC66" );
		}

		private static string ColorCountdownSeconds( long seconds )
		{
			return ColorValue( Math.Max( 0, seconds ) + "s", "FFCC66" );
		}

		private static TestResult FindResult( TestResult[] results, string name )
		{
			if ( results == null )
				return null;

			for ( int i = 0; i < results.Length; ++i )
			{
				if ( results[i] != null && String.Equals( results[i].Name, name, StringComparison.OrdinalIgnoreCase ) )
					return results[i];
			}

			return null;
		}

		private static void AppendResult( StringBuilder text, TestResult result )
		{
			if ( text == null || result == null )
				return;

			text.AppendFormat( "{0} {1}: {2}<br>",
				ColorValue( result.Level.ToString().ToUpperInvariant(), GetResultColor( result.Level.ToString() ) ),
				Encode( result.Name ),
				Encode( result.MeasuredValue ) );
		}

		private static void AppendSpawnEntryResult( StringBuilder text, TestResult[] results )
		{
			TestResult direct = FindResult( results, "SpawnEntry direct schedules" );
			TestResult callback = FindResult( results, "SpawnEntry callback work" );
			TestResult unknown = FindResult( results, "SpawnEntry unknown-origin callbacks" );

			if ( direct == null && callback == null && unknown == null )
				return;

			TestResultLevel level = TestResultLevel.Info;

			if ( direct != null && direct.Level > level )
				level = direct.Level;
			if ( callback != null && callback.Level > level )
				level = callback.Level;
			if ( unknown != null && unknown.Level > level )
				level = unknown.Level;

			text.AppendFormat( "{0} SpawnEntry: direct={1}, callback={2}, unknown={3}<br>",
				ColorValue( level.ToString().ToUpperInvariant(), GetResultColor( level.ToString() ) ),
				direct != null ? Encode( direct.MeasuredValue ) : "n/a",
				callback != null ? Encode( callback.MeasuredValue ) : "n/a",
				unknown != null ? Encode( unknown.MeasuredValue ) : "n/a" );
		}

		private static long GetLongRunNextCaptureSeconds( TaskManagerTestController.Snapshot snapshot )
		{
			if ( snapshot == null || snapshot.LongRunNextCaptureUtc == DateTime.MinValue )
				return 0;

			TimeSpan remaining = snapshot.LongRunNextCaptureUtc - DateTime.UtcNow;
			return remaining > TimeSpan.Zero ? (long)Math.Ceiling( remaining.TotalSeconds ) : 0;
		}

		private static string GetResultColor( string result )
		{
			if ( String.Equals( result, "PASS", StringComparison.OrdinalIgnoreCase ) )
				return ColorGood;
			if ( String.Equals( result, "WARN", StringComparison.OrdinalIgnoreCase ) )
				return "FFCC66";
			if ( String.Equals( result, "FAIL", StringComparison.OrdinalIgnoreCase ) )
				return ColorBad;
			return "CCCCCC";
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender != null ? sender.Mobile : null;

			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			int button = info.ButtonID;

			if ( button >= ButtonTabBase + PageDashboard && button <= ButtonTabBase + PageNetwork )
			{
				TaskManagerTestController.OpenGump( from, button - ButtonTabBase );
				return;
			}

			switch ( button )
			{
				case ButtonOfficial:
					from.SendGump( new PerformanceTestStartConfirmGump( ButtonOfficial, "Official v3.16 Confirmation" ) );
				break;
				case ButtonDailyAttribution:
					from.SendGump( new PerformanceTestStartConfirmGump( ButtonDailyAttribution, "Official v3.17 Daily Attribution" ) );
					break;
				case ButtonDaily:
					from.SendGump( new PerformanceTestStartConfirmGump( ButtonDaily, "Daily Only" ) );
					break;
				case Button150:
					from.SendGump( new PerformanceTestStartConfirmGump( Button150, "150 SpawnEntry Only" ) );
					break;
				case Button250:
					from.SendGump( new PerformanceTestStartConfirmGump( Button250, "250 Only" ) );
					break;
				case ButtonHourly:
					from.SendGump( new PerformanceTestStartConfirmGump( ButtonHourly, "Hourly Only" ) );
					break;
				case Button200:
					from.SendGump( new PerformanceTestStartConfirmGump( Button200, "200 Only" ) );
					break;
				case ButtonLagReportOnly:
					TaskManagerTestController.StartLagReportOnly( from );
					break;
				case ButtonBalanceValidate:
					from.SendGump( new PerformanceTestBalanceValidateConfirmGump( true ) );
					break;
				case ButtonDyePreview:
					TaskManagerTestController.RunDyePreviewTests( from );
					break;
				case ButtonLongRunStart:
					if ( !TaskManagerTestController.StartLongRun( from, TaskManagerTestController.GetSnapshot().LongRunDurationMinutes, TaskManagerTestController.GetSnapshot().LongRunIntervalMinutes ) )
						TaskManagerTestController.OpenGump( from, PageLongRun );
					break;
				case ButtonLongRunStop:
					from.SendGump( new PerformanceTestLongRunStopConfirmGump() );
					break;
				case ButtonLongRunDuration:
					TaskManagerTestController.CycleLongRunDuration();
					TaskManagerTestController.OpenGump( from, PageLongRun );
					break;
				case ButtonLongRunInterval:
					TaskManagerTestController.CycleLongRunInterval();
					TaskManagerTestController.OpenGump( from, PageLongRun );
					break;
				case ButtonLongRunStartup:
					TaskManagerTestController.ToggleLongRunStartupMarker();
					TaskManagerTestController.OpenGump( from, PageLongRun );
					break;
				case ButtonLongRunTaskWindows:
					TaskManagerTestController.ToggleLongRunTaskWindows();
					TaskManagerTestController.OpenGump( from, PageLongRun );
					break;
				case ButtonLongRunAutoSave:
					TaskManagerTestController.ToggleLongRunAutoSaveWindows();
					TaskManagerTestController.OpenGump( from, PageLongRun );
					break;
				case ButtonLongRunSummary:
					TaskManagerTestController.ToggleLongRunEndSummary();
					TaskManagerTestController.OpenGump( from, PageLongRun );
					break;
				case ButtonGenerateLagReport:
					TaskManagerTestController.GenerateLagReportNow( from );
					break;
				case ButtonGenerateSummary:
					TaskManagerTestController.GenerateSummaryNow( from );
					break;
				case ButtonClearReportHistory:
					TaskManagerTestController.ClearDisplayedReportHistory( from );
					break;
				case ButtonSaveAggregate:
					TaskManagerTestController.SetSaveDiagnosticsMode( from, SaveDiagnosticMode.Aggregate );
					break;
				case ButtonSaveDeep:
					TaskManagerTestController.SetSaveDiagnosticsMode( from, SaveDiagnosticMode.Deep );
					break;
				case ButtonSaveDeepNext:
					TaskManagerTestController.RequestDeepNextSave( from );
					break;
				case ButtonSaveReset:
					TaskManagerTestController.ResetSaveDiagnostics( from );
					break;
				case ButtonConsoleFlush:
					TaskManagerTestController.FlushConsoleTranscript( from );
					break;
				case ButtonConsoleRotate:
					TaskManagerTestController.RotateConsoleTranscript( from );
					break;
				case ButtonRuneBagStatus:
					TaskManagerTestController.OpenGump( from, PageOperations );
					break;
				case ButtonRuneBagReset:
					TaskManagerTestController.ResetRuneBagDiagnostics( from );
					break;
				case ButtonNetFlightStatus:
					TaskManagerTestController.ShowNetFlightDiagnosticsStatus( from );
					TaskManagerTestController.OpenGump( from, PageNetwork );
					break;
				case ButtonNetFlightEnable:
					TaskManagerTestController.SetNetFlightDiagnosticsEnabled( from, true );
					break;
				case ButtonDailyMinus:
					AdjustSetting( from, "daily", -5 );
					break;
				case ButtonDailyPlus:
					AdjustSetting( from, "daily", 5 );
					break;
				case Button150Minus:
					AdjustSetting( from, "150", -5 );
					break;
				case Button150Plus:
					AdjustSetting( from, "150", 5 );
					break;
				case Button250Minus:
					AdjustSetting( from, "250", -5 );
					break;
				case Button250Plus:
					AdjustSetting( from, "250", 5 );
					break;
				case ButtonHourlyMinus:
					AdjustSetting( from, "hourly", -5 );
					break;
				case ButtonHourlyPlus:
					AdjustSetting( from, "hourly", 5 );
					break;
				case Button200Minus:
					AdjustSetting( from, "200", -5 );
					break;
				case Button200Plus:
					AdjustSetting( from, "200", 5 );
					break;
				case ButtonToggleHourly:
					TaskManagerTestController.IncludeHourly = !TaskManagerTestController.IncludeHourly;
					TaskManagerTestController.OpenGump( from, PageSettings );
					break;
				case ButtonToggle200:
					TaskManagerTestController.Include200 = !TaskManagerTestController.Include200;
					TaskManagerTestController.OpenGump( from, PageSettings );
					break;
				case ButtonToggleValidate:
					if ( TaskManagerTestController.IncludeBalancePortalValidate )
					{
						TaskManagerTestController.IncludeBalancePortalValidate = false;
						TaskManagerTestController.OpenGump( from, PageSettings );
					}
					else
					{
						from.SendGump( new PerformanceTestBalanceValidateConfirmGump( false ) );
					}
					break;
				case ButtonToggleAutoReports:
					TaskManagerTestController.AutoGenerateLagReportEveryStep = !TaskManagerTestController.AutoGenerateLagReportEveryStep;
					TaskManagerTestController.OpenGump( from, PageSettings );
					break;
				case ButtonToggleAutoSummary:
					TaskManagerTestController.AutoGenerateFinalSummary = !TaskManagerTestController.AutoGenerateFinalSummary;
					TaskManagerTestController.OpenGump( from, PageSettings );
					break;
				case ButtonRefresh:
					TaskManagerTestController.OpenGump( from, m_Page );
					break;
				case ButtonProgress:
					TaskManagerTestController.OpenProgressGump( from );
					break;
				case ButtonCancel:
					TaskManagerTestController.Cancel( from );
					break;
				case ButtonReset:
					TaskManagerTestController.ResetDisplay( from );
					break;
				case ButtonOverrideSettling:
					from.SendGump( new PerformanceTestOverrideSettlingGump() );
					break;
			}
		}

		private static void AdjustSetting( Mobile from, string family, int delta )
		{
			TaskManagerTestController.AdjustSettlingSeconds( family, delta );
			TaskManagerTestController.OpenGump( from, PageSettings );
		}

		public static void StartConfirmedAction( Mobile from, int action )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			bool started = false;

			switch ( action )
			{
				case ButtonOfficial:
					started = TaskManagerTestController.StartOfficial( from );
					break;
				case ButtonDailyAttribution:
					started = TaskManagerTestController.StartDailyAttribution( from );
					break;
				case ButtonDaily:
					started = TaskManagerTestController.StartDailyOnly( from );
					break;
				case Button150:
					started = TaskManagerTestController.Start150Only( from );
					break;
				case Button250:
					started = TaskManagerTestController.Start250Only( from );
					break;
				case ButtonHourly:
					started = TaskManagerTestController.StartHourlyOnly( from );
					break;
				case Button200:
					started = TaskManagerTestController.Start200Only( from );
					break;
			}

			if ( !started )
				TaskManagerTestController.OpenGump( from );
		}
	}

	public class PerformanceTestStartConfirmGump : Gump
	{
		private readonly int m_Action;

		public PerformanceTestStartConfirmGump( int action, string name ) : base( 150, 120 )
		{
			m_Action = action;
			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 560, 280, 9270 );
			AddAlphaRegion( 16, 16, 528, 248 );
			AddHtml( 28, 24, 504, 28, "<basefont color=#FFCC66><center><big>Confirm Performance Test</big></center>", false, false );
			AddHtml( 28, 66, 504, 132,
				"<basefont color=#FFFFFF>You are about to start <basefont color=#FFCC66>" + Encode( name ) + "</basefont>.<br><br>"
				+ "This uses real maintenance logic and may delete, create, respawn, broadcast, and update LastRun state. It cannot be interrupted while a synchronous task is executing.</basefont>",
				false, false );
			AddButton( 80, 220, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddLabel( 114, 222, TaskManagerTestGump.HueGood, "Confirm and Start" );
			AddButton( 360, 220, 4005, 4007, 2, GumpButtonType.Reply, 0 );
			AddLabel( 394, 222, 1152, "Cancel" );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender != null ? sender.Mobile : null;

			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			if ( info.ButtonID == 1 )
				TaskManagerTestGump.StartConfirmedAction( from, m_Action );
			else
				TaskManagerTestController.OpenGump( from );
		}

		private static string Encode( string text )
		{
			return String.IsNullOrEmpty( text ) ? "test" : text.Replace( "&", "&amp;" ).Replace( "<", "&lt;" ).Replace( ">", "&gt;" );
		}
	}

	public class PerformanceTestBalanceValidateConfirmGump : Gump
	{
		private readonly bool m_RunNow;

		public PerformanceTestBalanceValidateConfirmGump( bool runNow ) : base( 150, 120 )
		{
			m_RunNow = runNow;
			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 580, 300, 9270 );
			AddAlphaRegion( 16, 16, 548, 268 );
			AddHtml( 28, 24, 524, 28, "<basefont color=#DE103A><center><big>Confirm BalancePortalValidate</big></center>", false, false );
			AddHtml( 28, 66, 524, 150,
				"<basefont color=#FFFFFF>This command intentionally compares against the old full-scan path.<br><br>"
				+ "It may take 5-6+ seconds. Do not run it during timed performance windows unless intentionally marked. Generated output is labeled BEGIN/END BALANCEPORTALVALIDATE.</basefont>",
				false, false );
			AddButton( 70, 240, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddLabel( 104, 242, runNow ? TaskManagerTestGump.HueGood : 53, runNow ? "Confirm and Run" : "Include in Official Test" );
			AddButton( 385, 240, 4005, 4007, 2, GumpButtonType.Reply, 0 );
			AddLabel( 419, 242, 1152, "Cancel" );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender != null ? sender.Mobile : null;

			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			if ( info.ButtonID == 1 )
			{
				if ( m_RunNow )
				{
					if ( !TaskManagerTestController.StartBalancePortalValidateOnly( from ) )
						TaskManagerTestController.OpenGump( from );
				}
				else
				{
					TaskManagerTestController.IncludeBalancePortalValidate = true;
					TaskManagerTestController.OpenGump( from, 6 );
				}
			}
			else
			{
				TaskManagerTestController.OpenGump( from );
			}
		}
	}

	public class PerformanceTestLongRunStopConfirmGump : Gump
	{
		public PerformanceTestLongRunStopConfirmGump() : base( 180, 160 )
		{
			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 500, 210, 9270 );
			AddAlphaRegion( 16, 16, 468, 178 );
			AddHtml( 28, 30, 444, 92, "<basefont color=#FFFFFF>Stop the active diagnostics-only long-run session? Existing report files remain on disk and the summary will record that an administrator stopped the run.</basefont>", false, false );
			AddButton( 70, 150, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddLabel( 104, 152, TaskManagerTestGump.HueBad, "Confirm Stop" );
			AddButton( 320, 150, 4005, 4007, 2, GumpButtonType.Reply, 0 );
			AddLabel( 354, 152, 1152, "Cancel" );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender != null ? sender.Mobile : null;

			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			if ( info.ButtonID == 1 )
				TaskManagerTestController.StopLongRun( from );
			else
				TaskManagerTestController.OpenGump( from, 4 );
		}
	}

	public class PerformanceTestOverrideSettlingGump : Gump
	{
		public PerformanceTestOverrideSettlingGump() : base( 150, 120 )
		{
			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 560, 260, 9270 );
			AddAlphaRegion( 16, 16, 528, 228 );
			AddHtml( 28, 24, 504, 28, "<basefont color=#DE103A><center><big>Override Settling Guard</big></center>", false, false );
			AddHtml( 28, 66, 504, 116,
				"<basefont color=#FFFFFF>The settling guard allows downstream timers, especially SpawnEntry callbacks after a 150-minute run, to finish before another test starts. Override only when you intentionally accept overlapping diagnostic windows.</basefont>",
				false, false );
			AddButton( 70, 205, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddLabel( 104, 207, TaskManagerTestGump.HueBad, "Override Guard" );
			AddButton( 360, 205, 4005, 4007, 2, GumpButtonType.Reply, 0 );
			AddLabel( 394, 207, 1152, "Cancel" );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender != null ? sender.Mobile : null;

			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			if ( info.ButtonID == 1 )
				TaskManagerTestController.OverrideSettlingGuard( from );
			else
				TaskManagerTestController.OpenGump( from );
		}
	}
}
