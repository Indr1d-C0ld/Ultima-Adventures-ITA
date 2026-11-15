using System;
using System.Collections.Generic;
using System.IO;
using Server.Diagnostics;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;

namespace Server.Commands
{
	public static class TaskManagerTestController
	{
		private const int MaxEvents = 80;
		private const int MaxReports = 64;
		private const int MaxDisplayedReports = 20;
		private const int MaxRecentModuleResults = 16;
		private const string OutputRoot = "Logs\\LagDiagnostics\\PerformanceTests";

		private const long BalancePortalPassMs = 500;
		private const long BalancePortalFailMs = 2000;
		private const long TaskManagerDailyPassMs = 5000;
		private const long TaskManagerDailyFailMs = 8000;
		private const long DailyWallClockPassMs = 60000;
		private const long DailyWallClockFailMs = 120000;
		private const long TaskManager150PassMs = 250;
		private const long TaskManager150FailMs = 1000;
		private const long TaskManager250PassMs = 500;
		private const long TaskManager250FailMs = 2000;
		private const long PopulateCitiesPassMs = 1000;
		private const long PopulateCitiesFailMs = 4000;
		private const long PopulateVillagesSetupSpawnPassMs = 1000;
		private const long PopulateVillagesSetupSpawnFailMs = 4000;
		private const long PlantGardensPassMs = 250;
		private const long PlantGardensFailMs = 1500;
		private const long SpreadOutCreatureCleanupPassMs = 250;
		private const long SpreadOutCreatureCleanupFailMs = 1500;
		private const long DailyStagedCallbackPassMs = 100;
		private const long DailyStagedCallbackFailMs = 250;
		private const long DailyStagedCallbackCriticalMs = 1000;
		private const long DailyStagedCallbackCountWarn = 3000;
		private const long DailyStagedCallbackCountFail = 5000;
		private const long SpawnEntryCallbackWorkWarnMs = 1000;
		private const long SpawnEntryExpectedDirectSchedulesMin = 150;
		private const long SpawnEntryExpectedDirectSchedulesMax = 250;
		private const long PremiumSpawnerOldestReadyWarnMs = 15000;
		private const long PremiumSpawnerOldestReadyFailMs = 30000;
		private const long PremiumSpawnerSingleEntryPassMs = 100;
		private const long PremiumSpawnerSingleEntryFailMs = 250;
		private const long PremiumSpawnerConstructorPassMs = 75;
		private const long PremiumSpawnerConstructorFailMs = 150;

		private static readonly object m_Lock = new object();
		private static readonly List<string> m_Events = new List<string>();
		private static readonly List<string> m_ReportFiles = new List<string>();
		private static readonly List<string> m_RecentModuleResults = new List<string>();
		private static readonly List<TestResult> m_LastResults = new List<TestResult>();
		private static readonly Dictionary<string, ManualSample> m_LastManualByFamily = new Dictionary<string, ManualSample>();

		private static TestState m_State = TestState.Idle;
		private static string m_ActiveTestName = "none";
		private static TestModule m_ActiveModule = TestModule.LagReportOnly;
		private static string m_ActivePhase = "Idle";
		private static string m_NextStep = "none";
		private static List<TestStep> m_Plan = new List<TestStep>();
		private static int m_StepIndex;
		private static Mobile m_Admin;
		private static string m_AdminName = "none";
		private static string m_AccountName = "none";
		private static DateTime m_StartedUtc = DateTime.MinValue;
		private static DateTime m_StartedLocal = DateTime.MinValue;
		private static DateTime m_LastUpdateUtc = DateTime.MinValue;
		private static string m_CurrentFamily = "none";
		private static string m_LastCompletedStep = "none";
		private static string m_LastReportPath = "none";
		private static string m_LastSummaryPath = "none";
		private static string m_OutputFolder = "none";
		private static string m_LastError = "none";
		private static string m_OverallResult = "INFO";
		private static bool m_CancelRequested;
		private static int m_GumpPage = 1;

		private static bool m_InFlight;
		private static string m_InFlightFamily = "none";
		private static string m_InFlightCommand = "none";
		private static string m_InFlightAdmin = "none";
		private static string m_InFlightAccount = "none";
		private static DateTime m_InFlightUtc = DateTime.MinValue;
		private static DateTime m_InFlightLocal = DateTime.MinValue;

		private static bool m_Settling;
		private static string m_SettlingFamily = "none";
		private static string m_SettlingReason = "none";
		private static DateTime m_SettlingUntilUtc = DateTime.MinValue;
		private static DateTime m_SettlingUntilLocal = DateTime.MinValue;

		private static ManualSample m_LastManual;
		private static TestTimer m_TestTimer;
		private static RefreshTimer m_RefreshTimer;
		private static LongRunTimer m_LongRunTimer;

		private static bool m_IncludeHourly = true;
		private static bool m_Include200 = true;
		private static bool m_IncludeBalancePortalValidate;
		private static bool m_AutoGenerateLagReportEveryStep = true;
		private static bool m_AutoGenerateFinalSummary = true;
		private static int m_DailySettlingSeconds = 90;
		private static int m_150SettlingSeconds = 45;
		private static int m_250SettlingSeconds = 60;
		private static int m_HourlySettlingSeconds = 30;
		private static int m_200SettlingSeconds = 30;
		private static int m_LongRunDurationMinutes = 30;
		private static int m_LongRunIntervalMinutes = 5;
		private static bool m_LongRunCaptureStartupMarker = true;
		private static bool m_LongRunCaptureTaskWindows = true;
		private static bool m_LongRunCaptureAutoSaveWindows = true;
		private static bool m_LongRunEndSummary = true;

		private static bool m_LongRunActive;
		private static DateTime m_LongRunStartedUtc = DateTime.MinValue;
		private static DateTime m_LongRunStartedLocal = DateTime.MinValue;
		private static DateTime m_LongRunEndUtc = DateTime.MinValue;
		private static DateTime m_LongRunEndLocal = DateTime.MinValue;
		private static DateTime m_LongRunNextCaptureUtc = DateTime.MinValue;
		private static int m_LongRunCaptures;
		private static int m_LongRunErrors;
		private static string m_LongRunLastError = "none";
		private static long m_BaselineSpawnDirectSchedules;
		private static long m_BaselineSpawnUnknownCallbacks;
		private static long m_BaselineSpawnCallbackMs;
		private static TaskManagerDailyAttributionDiagnostics.PerformanceSummary m_CurrentTestDailyAttribution;
		private static string m_CurrentTestDailyAttributionModule = "none";

		private enum TestState
		{
			Idle,
			Running,
			Settling,
			CapturingLagReport,
			WaitingForAdmin,
			Complete,
			Failed,
			Cancelled
		}

		private struct ScheduleState
		{
			public bool Available;
			public DateTime LastRunUtc;
			public DateTime NextRunUtc;
		}

		private sealed class ManualSample
		{
			public string Family;
			public string Command;
			public string Admin;
			public string Account;
			public bool Forced;
			public DateTime StartedUtc;
			public DateTime StartedLocal;
			public long ElapsedMs;
			public string Result;
			public string Error;
			public string OldSchedule;
			public string NewSchedule;

			public ManualSample Clone()
			{
				ManualSample copy = new ManualSample();
				copy.Family = Family;
				copy.Command = Command;
				copy.Admin = Admin;
				copy.Account = Account;
				copy.Forced = Forced;
				copy.StartedUtc = StartedUtc;
				copy.StartedLocal = StartedLocal;
				copy.ElapsedMs = ElapsedMs;
				copy.Result = Result;
				copy.Error = Error;
				copy.OldSchedule = OldSchedule;
				copy.NewSchedule = NewSchedule;
				return copy;
			}
		}

		private delegate void ManualExecutionCompleted( bool success, Exception error );

		private sealed class ManualExecution
		{
			public Mobile From;
			public string Family;
			public string Command;
			public string Admin;
			public string Account;
			public bool Forced;
			public DateTime StartedUtc;
			public DateTime StartedLocal;
			public long StartedTimestamp;
			public string OldSchedule;
			public ManualExecutionCompleted Completion;
			public bool Finished;
		}

		public sealed class Snapshot
		{
			public string State;
			public string ActiveTestName;
			public string ActiveModule;
			public string ActivePhase;
			public string NextStep;
			public int StepNumber;
			public int StepCount;
			public int CurrentStepIndex;
			public string[] PlanSteps;
			public string CurrentFamily;
			public bool InFlight;
			public string InFlightCommand;
			public string InFlightAdmin;
			public bool Settling;
			public string SettlingFamily;
			public string SettlingReason;
			public DateTime SettlingUntilUtc;
			public DateTime SettlingUntilLocal;
			public int SettlingSecondsRemaining;
			public string AdminName;
			public string AccountName;
			public DateTime StartedUtc;
			public DateTime StartedLocal;
			public long ElapsedSeconds;
			public bool CancelRequested;
			public string LastCompletedStep;
			public string LastReportPath;
			public string LastSummaryPath;
			public string OutputFolder;
			public string LastError;
			public string OverallResult;
			public string LastManualCommand;
			public string LastManualResult;
			public long LastManualElapsedMs;
			public bool IncludeHourly;
			public bool Include200;
			public bool IncludeBalancePortalValidate;
			public bool AutoGenerateLagReportEveryStep;
			public bool AutoGenerateFinalSummary;
			public int DailySettlingSeconds;
			public int Settling150Seconds;
			public int Settling250Seconds;
			public int HourlySettlingSeconds;
			public int Settling200Seconds;
			public int LongRunDurationMinutes;
			public int LongRunIntervalMinutes;
			public bool LongRunCaptureStartupMarker;
			public bool LongRunCaptureTaskWindows;
			public bool LongRunCaptureAutoSaveWindows;
			public bool LongRunEndSummary;
			public bool LongRunActive;
			public DateTime LongRunStartedUtc;
			public DateTime LongRunStartedLocal;
			public DateTime LongRunEndUtc;
			public DateTime LongRunEndLocal;
			public DateTime LongRunNextCaptureUtc;
			public long LongRunRemainingSeconds;
			public int LongRunCaptures;
			public int LongRunErrors;
			public string LongRunLastError;
			public int GumpPage;
			public string[] RecentEvents;
			public string[] ReportFiles;
			public string[] RecentModuleResults;
			public TestResult[] Results;
		}

		public static bool IncludeHourly
		{
			get { lock ( m_Lock ) { return m_IncludeHourly; } }
			set { lock ( m_Lock ) { m_IncludeHourly = value; } }
		}

		public static bool Include200
		{
			get { lock ( m_Lock ) { return m_Include200; } }
			set { lock ( m_Lock ) { m_Include200 = value; } }
		}

		public static bool IncludeBalancePortalValidate
		{
			get { lock ( m_Lock ) { return m_IncludeBalancePortalValidate; } }
			set { lock ( m_Lock ) { m_IncludeBalancePortalValidate = value; } }
		}

		public static bool AutoGenerateLagReportEveryStep
		{
			get { lock ( m_Lock ) { return m_AutoGenerateLagReportEveryStep; } }
			set { lock ( m_Lock ) { m_AutoGenerateLagReportEveryStep = value; } }
		}

		public static bool AutoGenerateFinalSummary
		{
			get { lock ( m_Lock ) { return m_AutoGenerateFinalSummary; } }
			set { lock ( m_Lock ) { m_AutoGenerateFinalSummary = value; } }
		}

		public static bool CanRunBalancePortalValidate
		{
			get { lock ( m_Lock ) { return m_IncludeBalancePortalValidate; } }
		}

		public static void AdjustSettlingSeconds( string family, int delta )
		{
			lock ( m_Lock )
			{
				if ( family == "daily" )
					m_DailySettlingSeconds = Clamp( m_DailySettlingSeconds + delta, 0, 600 );
				else if ( family == "150" )
					m_150SettlingSeconds = Clamp( m_150SettlingSeconds + delta, 0, 600 );
				else if ( family == "250" )
					m_250SettlingSeconds = Clamp( m_250SettlingSeconds + delta, 0, 600 );
				else if ( family == "hourly" )
					m_HourlySettlingSeconds = Clamp( m_HourlySettlingSeconds + delta, 0, 600 );
				else if ( family == "200" )
					m_200SettlingSeconds = Clamp( m_200SettlingSeconds + delta, 0, 600 );
			}
		}

		public static void CycleLongRunDuration()
		{
			int[] values = new int[] { 30, 60, 120, 360, 480, 720, 1440 };

			lock ( m_Lock )
				m_LongRunDurationMinutes = NextChoice( values, m_LongRunDurationMinutes );
		}

		public static void CycleLongRunInterval()
		{
			int[] values = new int[] { 1, 2, 5, 10 };

			lock ( m_Lock )
				m_LongRunIntervalMinutes = NextChoice( values, m_LongRunIntervalMinutes );
		}

		public static void ToggleLongRunStartupMarker()
		{
			lock ( m_Lock )
				m_LongRunCaptureStartupMarker = !m_LongRunCaptureStartupMarker;
		}

		public static void ToggleLongRunTaskWindows()
		{
			lock ( m_Lock )
				m_LongRunCaptureTaskWindows = !m_LongRunCaptureTaskWindows;
		}

		public static void ToggleLongRunAutoSaveWindows()
		{
			lock ( m_Lock )
				m_LongRunCaptureAutoSaveWindows = !m_LongRunCaptureAutoSaveWindows;
		}

		public static void ToggleLongRunEndSummary()
		{
			lock ( m_Lock )
				m_LongRunEndSummary = !m_LongRunEndSummary;
		}

		private static int NextChoice( int[] values, int current )
		{
			for ( int i = 0; i < values.Length; ++i )
			{
				if ( values[i] == current )
					return values[( i + 1 ) % values.Length];
			}

			return values[0];
		}

		private static int Clamp( int value, int min, int max )
		{
			if ( value < min )
				return min;

			if ( value > max )
				return max;

			return value;
		}

		public static void OpenGump( Mobile from )
		{
			OpenGump( from, 0 );
		}

		public static void OpenGump( Mobile from, int page )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			lock ( m_Lock )
			{
				if ( page >= TaskManagerTestGump.PageDashboard && page <= TaskManagerTestGump.PageNetwork )
					m_GumpPage = page;

				page = m_GumpPage;
			}

			from.CloseGump( typeof( TaskManagerTestGump ) );
			from.SendGump( new TaskManagerTestGump( page ) );
			RefreshProgressGump( from );
		}

		public static void OpenProgressGump( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			from.CloseGump( typeof( PerformanceTestProgressGump ) );
			from.SendGump( new PerformanceTestProgressGump() );
		}

		private static void OpenDashboardAndProgress( Mobile from )
		{
			lock ( m_Lock )
				m_GumpPage = TaskManagerTestGump.PageDashboard;

			OpenGump( from, TaskManagerTestGump.PageDashboard );
			OpenProgressGump( from );
		}

		public static bool StartOfficial( Mobile from )
		{
			return StartPlan( from, BuildOfficialPlan() );
		}

		public static bool StartDailyAttribution( Mobile from )
		{
			TestPlan plan = new TestPlan( "Official v3.17 Daily Attribution", TestModule.OfficialV317DailyAttribution );
			AddPreflight( plan );
			AddRunTask( plan, "daily", "Run daily manual forced" );
			AddLagReport( plan, "daily-attribution-immediate" );
			plan.Add( new TestStep( TestStepType.CaptureStats, "Capture TaskManagerDaily attribution" ) );
			AddWait( plan, "daily", GetSettlingSeconds( "daily" ), "Daily settling after attribution run" );
			AddLagReport( plan, "daily-attribution-settled" );
			AddSummary( plan );
			return StartPlan( from, plan );
		}

		public static bool StartDailyOnly( Mobile from )
		{
			TestPlan plan = new TestPlan( "Daily Only", TestModule.DailyOnly );
			AddPreflight( plan );
			AddRunTask( plan, "daily", "Run daily manual forced" );
			AddLagReport( plan, "daily-immediate" );
			AddWait( plan, "daily", GetSettlingSeconds( "daily" ), "Daily settling after manual run" );
			AddLagReport( plan, "daily-settled" );
			AddSummary( plan );
			return StartPlan( from, plan );
		}

		public static bool Start150Only( Mobile from )
		{
			TestPlan plan = new TestPlan( "150 SpawnEntry Only", TestModule.SpawnEntry150Only );
			AddPreflight( plan );
			AddRunTask( plan, "150", "Run 150 SpawnEntry manual" );
			AddLagReport( plan, "150-immediate" );
			AddWait( plan, "150", GetSettlingSeconds( "150" ), "SpawnEntry callback settling after 150-minute run" );
			AddLagReport( plan, "150-settled" );
			AddSummary( plan );
			return StartPlan( from, plan );
		}

		public static bool Start250Only( Mobile from )
		{
			TestPlan plan = new TestPlan( "250 Only", TestModule.TaskManager250Only );
			AddPreflight( plan );
			AddRunTask( plan, "250", "Run 250 manual forced" );
			AddLagReport( plan, "250-immediate" );
			AddWait( plan, "250", GetSettlingSeconds( "250" ), "250-minute settling after manual run" );
			AddLagReport( plan, "250-settled" );
			AddSummary( plan );
			return StartPlan( from, plan );
		}

		public static bool StartHourlyOnly( Mobile from )
		{
			TestPlan plan = new TestPlan( "Hourly Only", TestModule.TaskManagerHourlyOnly );
			AddPreflight( plan );
			AddRunTask( plan, "hourly", "Run hourly manual" );
			AddWait( plan, "hourly", GetSettlingSeconds( "hourly" ), "Hourly settling after manual run" );
			AddLagReport( plan, "hourly-settled" );
			AddSummary( plan );
			return StartPlan( from, plan );
		}

		public static bool Start200Only( Mobile from )
		{
			TestPlan plan = new TestPlan( "200 Only", TestModule.TaskManager200Only );
			AddPreflight( plan );
			AddRunTask( plan, "200", "Run 200-minute manual" );
			AddWait( plan, "200", GetSettlingSeconds( "200" ), "200-minute settling after manual run" );
			AddLagReport( plan, "200-settled" );
			AddSummary( plan );
			return StartPlan( from, plan );
		}

		public static bool StartLagReportOnly( Mobile from )
		{
			TestPlan plan = new TestPlan( "LagReport Only", TestModule.LagReportOnly );
			AddPreflight( plan );
			AddLagReport( plan, "manual-module" );
			AddSummary( plan );
			return StartPlan( from, plan );
		}

		public static bool StartBalancePortalValidateOnly( Mobile from )
		{
			TestPlan plan = new TestPlan( "BalancePortalValidate Only", TestModule.BalancePortalValidateOnly );
			AddPreflight( plan );
			TestStep validate = new TestStep( TestStepType.RunBalancePortalValidate, "Run BalancePortalValidate" );
			validate.Description = "Read-only comparison against the intentionally expensive old full-scan selection path.";
			validate.RequiresConfirmation = true;
			plan.Add( validate );
			AddLagReport( plan, "balanceportalvalidate" );
			AddSummary( plan );
			return StartPlan( from, plan );
		}

		public static bool RunDyePreviewTests( Mobile from )
		{
			string reason;

			if ( !CanStart( from, false, out reason ) )
			{
				SendLine( from, reason );
				return false;
			}

			DateTime startedUtc = DateTime.UtcNow;
			DateTime startedLocal = DateTime.Now;
			TestResult[] testResults = DyePreviewTestSuite.Run();
			List<TestResult> results = new List<TestResult>( testResults );
			TestResultLevel overall = GetOverallResult( results );

			lock ( m_Lock )
			{
				m_State = overall == TestResultLevel.Fail ? TestState.Failed : TestState.Complete;
				m_ActiveTestName = "Dye Preview Functional Tests";
				m_ActiveModule = TestModule.DyePreview;
				m_ActivePhase = "Complete";
				m_NextStep = "none";
				m_Plan.Clear();
				m_StepIndex = 0;
				m_Admin = from;
				m_AdminName = GetMobileName( from );
				m_AccountName = GetAccountName( from );
				m_StartedUtc = startedUtc;
				m_StartedLocal = startedLocal;
				m_LastUpdateUtc = DateTime.UtcNow;
				m_CurrentFamily = "dye-preview";
				m_LastCompletedStep = "Run dye preview functional tests";
				m_LastError = overall == TestResultLevel.Fail ? "One or more dye preview tests failed." : "none";
				m_OverallResult = overall.ToString().ToUpperInvariant();
				m_CancelRequested = false;
				m_Events.Clear();
				m_LastResults.Clear();
				m_LastResults.AddRange( results );
				m_RecentModuleResults.Add( String.Format( "{0:u} {1}: {2}", DateTime.UtcNow, m_ActiveTestName, m_OverallResult ) );

				while ( m_RecentModuleResults.Count > MaxRecentModuleResults )
					m_RecentModuleResults.RemoveAt( 0 );
			}

			AddEvent( String.Format( "Ran dye preview functional tests by {0}: {1}", GetMobileName( from ), m_OverallResult ) );
			OpenGump( from, 1 );
			return overall != TestResultLevel.Fail;
		}

		private static TestPlan BuildOfficialPlan()
		{
			bool includeHourly;
			bool include200;
			bool includeValidate;

			lock ( m_Lock )
			{
				includeHourly = m_IncludeHourly;
				include200 = m_Include200;
				includeValidate = m_IncludeBalancePortalValidate;
			}

			TestPlan plan = new TestPlan( "Official v3.16 Confirmation", TestModule.OfficialV316 );
			AddPreflight( plan );
			AddRunTask( plan, "daily", "Run daily manual forced" );
			AddLagReport( plan, "daily-immediate" );
			AddWait( plan, "daily", GetSettlingSeconds( "daily" ), "Daily settling after manual run" );
			AddLagReport( plan, "daily-settled" );
			AddRunTask( plan, "150", "Run 150 SpawnEntry manual" );
			AddLagReport( plan, "150-immediate" );
			AddWait( plan, "150", GetSettlingSeconds( "150" ), "SpawnEntry callback settling after 150-minute run" );
			AddLagReport( plan, "150-settled" );
			AddRunTask( plan, "250", "Run 250 manual forced" );
			AddLagReport( plan, "250-immediate" );
			AddWait( plan, "250", GetSettlingSeconds( "250" ), "250-minute settling after manual run" );
			AddLagReport( plan, "250-settled" );

			if ( includeHourly )
			{
				AddRunTask( plan, "hourly", "Run hourly smoke test" );
				AddWait( plan, "hourly", GetSettlingSeconds( "hourly" ), "Hourly smoke-test settling" );
				AddLagReport( plan, "hourly-settled" );
			}

			if ( include200 )
			{
				AddRunTask( plan, "200", "Run 200-minute smoke test" );
				AddWait( plan, "200", GetSettlingSeconds( "200" ), "200-minute smoke-test settling" );
				AddLagReport( plan, "200-settled" );
			}

			if ( includeValidate )
			{
				TestStep validate = new TestStep( TestStepType.RunBalancePortalValidate, "Run BalancePortalValidate" );
				validate.RequiresConfirmation = true;
				plan.Add( validate );
				AddLagReport( plan, "balanceportalvalidate" );
			}

			AddLagReport( plan, "final" );
			AddSummary( plan );
			return plan;
		}

		private static void AddPreflight( TestPlan plan )
		{
			plan.Add( new TestStep( TestStepType.Preflight, "Preflight" ) );
			AddLagReport( plan, "preflight" );
		}

		private static void AddRunTask( TestPlan plan, string family, string name )
		{
			TestStep step = new TestStep( GetTaskStepType( family ), name );
			step.Family = family;
			step.MutatesWorldState = true;
			step.RequiresConfirmation = true;
			plan.Add( step );
		}

		private static TestStepType GetTaskStepType( string family )
		{
			if ( family == "hourly" )
				return TestStepType.RunTaskManagerHourly;
			if ( family == "150" )
				return TestStepType.RunTaskManager150;
			if ( family == "200" )
				return TestStepType.RunTaskManager200;
			if ( family == "250" )
				return TestStepType.RunTaskManager250;
			return TestStepType.RunTaskManagerDaily;
		}

		private static bool IsTaskStep( TestStepType type )
		{
			return type == TestStepType.RunTaskManagerHourly
				|| type == TestStepType.RunTaskManager150
				|| type == TestStepType.RunTaskManager200
				|| type == TestStepType.RunTaskManager250
				|| type == TestStepType.RunTaskManagerDaily;
		}

		private static void AddLagReport( TestPlan plan, string label )
		{
			TestStep step = new TestStep( TestStepType.CaptureLagReport, "Capture LagReport " + label );
			step.ReportLabel = label;
			plan.Add( step );
		}

		private static void AddWait( TestPlan plan, string family, int seconds, string name )
		{
			TestStep step = new TestStep( TestStepType.WaitSettling, name );
			step.Family = family;
			step.SettlingSeconds = seconds;
			step.EstimatedSeconds = seconds;
			plan.Add( step );
		}

		private static void AddSummary( TestPlan plan )
		{
			plan.Add( new TestStep( TestStepType.Summary, "Write combined summary" ) );
		}

		private static bool StartPlan( Mobile from, TestPlan plan )
		{
			string reason;

			if ( !CanStart( from, true, out reason ) )
			{
				SendLine( from, reason );
				return false;
			}

			DateTime nowUtc = DateTime.UtcNow;
			DateTime nowLocal = DateTime.Now;
			string stamp = nowLocal.ToString( "yyyyMMdd-HHmmss" );
			string folder = Path.GetFullPath( Path.Combine( OutputRoot, String.Format( "PerformanceTest-v31604-{0}", stamp ) ) );
			Directory.CreateDirectory( folder );
			SpawnEntryDiagnostics.PerformanceSummary spawnBaseline = SpawnEntryDiagnostics.GetPerformanceTestSummary();

			lock ( m_Lock )
			{
				m_State = TestState.Running;
				m_ActiveTestName = plan.Name;
				m_ActiveModule = plan.Module;
				m_ActivePhase = "Starting";
				m_NextStep = plan.Steps.Count > 0 ? plan.Steps[0].Name : "none";
				m_Plan = new List<TestStep>( plan.Steps );
				m_StepIndex = 0;
				m_Admin = from;
				m_AdminName = GetMobileName( from );
				m_AccountName = GetAccountName( from );
				m_StartedUtc = nowUtc;
				m_StartedLocal = nowLocal;
				m_LastUpdateUtc = nowUtc;
				m_CurrentFamily = "none";
				m_LastCompletedStep = "none";
				m_LastReportPath = "none";
				m_LastSummaryPath = "none";
				m_OutputFolder = folder;
				m_LastError = "none";
				m_OverallResult = "INFO";
				m_CancelRequested = false;
				m_Events.Clear();
				m_ReportFiles.Clear();
				m_LastResults.Clear();
				m_BaselineSpawnDirectSchedules = spawnBaseline.DirectSchedules;
				m_BaselineSpawnUnknownCallbacks = spawnBaseline.UnknownOriginCallbacks;
				m_BaselineSpawnCallbackMs = spawnBaseline.TotalCallbackMs;
				m_CurrentTestDailyAttribution = null;
				m_CurrentTestDailyAttributionModule = "none";
			}

			AddEvent( String.Format( "Started {0} by {1} account={2}", plan.Name, GetMobileName( from ), GetAccountName( from ) ) );
			EnsureRefreshTimer();
			ScheduleNextStep( TimeSpan.FromMilliseconds( 250.0 ) );
			OpenDashboardAndProgress( from );
			return true;
		}

		public static bool GenerateLagReportNow( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return false;

			string folder = Path.GetFullPath( OutputRoot );
			Directory.CreateDirectory( folder );
			string fileName = String.Format( "PerformanceTest-v31604-{0}-manual-LagReport", DateTime.Now.ToString( "yyyyMMdd-HHmmss" ) );
			string path = LagDiagnostics.GenerateLagReport( from, folder, fileName );

			lock ( m_Lock )
			{
				m_LastReportPath = path;
				AddReportPathLocked( path );
				m_OutputFolder = folder;
				m_LastUpdateUtc = DateTime.UtcNow;
			}

			AddEvent( "Generated manual LagReport: " + path );
			SendLine( from, "LagReport written to: " + path );
			OpenGump( from );
			return true;
		}

		public static bool GenerateSummaryNow( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return false;

			try
			{
				EvaluateCurrentTest();
				WriteSummaryFile( "manual snapshot" );
				SendLine( from, "Performance Test summary written to: " + GetSnapshot().LastSummaryPath );
				OpenGump( from, TaskManagerTestGump.PageReports );
				return true;
			}
			catch ( Exception ex )
			{
				SendLine( from, String.Format( "Performance Test summary failed: {0}: {1}", ex.GetType().FullName, ex.Message ) );
				return false;
			}
		}

		public static void ClearDisplayedReportHistory( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			lock ( m_Lock )
			{
				m_ReportFiles.Clear();
				m_LastUpdateUtc = DateTime.UtcNow;
			}

			AddEvent( "Cleared displayed report history; files were not deleted." );
			OpenGump( from, TaskManagerTestGump.PageReports );
		}

		public static void ShowSaveDiagnosticsStatus( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			using ( StringWriter writer = new StringWriter() )
			{
				SaveDiagnostics.WriteStats( writer );
				SendLines( from, writer );
			}
		}

		public static void SetSaveDiagnosticsMode( Mobile from, SaveDiagnosticMode mode )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			string message;
			SaveDiagnostics.SetConfiguredMode( mode, out message );
			SendLine( from, message );
			AddEvent( String.Format( "SaveDiagnostics mode set to {0} by {1}.", mode, GetMobileName( from ) ) );
			OpenGump( from, TaskManagerTestGump.PageOperations );
		}

		public static void RequestDeepNextSave( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			string message;
			SaveDiagnostics.RequestDeepNextSave( out message );
			SendLine( from, message );
			AddEvent( "Requested one-shot Deep save diagnostics by " + GetMobileName( from ) + "." );
			OpenGump( from, TaskManagerTestGump.PageOperations );
		}

		public static void ResetSaveDiagnostics( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			SaveDiagnostics.Reset();
			SendLine( from, "Save diagnostic counters and samples reset. Persistent mode was not changed." );
			AddEvent( "Reset SaveDiagnostics display counters by " + GetMobileName( from ) + "." );
			OpenGump( from, TaskManagerTestGump.PageOperations );
		}

		public static void ShowConsoleTranscriptStatus( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			using ( StringWriter writer = new StringWriter() )
			{
				ConsoleTranscript.WriteStats( writer );
				writer.WriteLine( "Warning: console transcript files may contain IP addresses, account names, character names, local paths, and exception details. Sanitize before sharing publicly." );
				SendLines( from, writer );
			}
		}

		public static void FlushConsoleTranscript( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			bool completed = ConsoleTranscript.FlushNow( 3000 );
			SendLine( from, completed ? "Console transcript flush completed." : "Console transcript flush requested; timeout reached before confirmation." );
			AddEvent( String.Format( "Console transcript flush requested by {0}; completed={1}.", GetMobileName( from ), completed ) );
			OpenGump( from, TaskManagerTestGump.PageOperations );
		}

		public static void RotateConsoleTranscript( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			bool completed = ConsoleTranscript.RotateNow( 3000 );
			SendLine( from, completed ? "Console transcript rotated to the next part." : "Console transcript rotation requested; timeout reached before confirmation." );
			AddEvent( String.Format( "Console transcript rotation requested by {0}; completed={1}.", GetMobileName( from ), completed ) );
			OpenGump( from, TaskManagerTestGump.PageOperations );
		}

		public static void ShowRuneBagDiagnosticsStatus( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			using ( StringWriter writer = new StringWriter() )
			{
				RuneBagDiagnostics.WriteStats( writer );
				SendLines( from, writer );
			}
		}

		public static void ResetRuneBagDiagnostics( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			RuneBagDiagnostics.Reset();
			SendLine( from, "RuneBagStats reset. Active throttle guards were preserved." );
			AddEvent( "Reset RuneBagStats by " + GetMobileName( from ) + "." );
			OpenGump( from, TaskManagerTestGump.PageOperations );
		}

		public static void ShowNetFlightDiagnosticsStatus( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			using ( StringWriter writer = new StringWriter() )
			{
				NetFlightRecorder.WriteDiagnostics( writer );
				SendLines( from, writer );
			}
		}

		public static void SetNetFlightDiagnosticsEnabled( Mobile from, bool enabled )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			NetFlightRecorder.SetEnabled( enabled );
			AddEvent( String.Format( "NetFlight automatic per-player capture {0} by {1}.", enabled ? "enabled" : "disabled", GetMobileName( from ) ) );
			SendLine( from, enabled
				? "NetFlight automatic per-player capture enabled."
				: "NetFlight automatic per-player capture disabled; no new packet evidence will be recorded." );
			OpenGump( from, TaskManagerTestGump.PageNetwork );
		}

		public static bool StartLongRun( Mobile from, int durationMinutes, int intervalMinutes )
		{
			string reason;

			if ( !CanStart( from, true, out reason ) )
			{
				SendLine( from, reason );
				return false;
			}

			if ( durationMinutes < 1 || durationMinutes > 1440 )
			{
				SendLine( from, "Performance Test long-run duration must be between 1 and 1440 minutes." );
				return false;
			}

			if ( intervalMinutes < 1 || intervalMinutes > 60 || intervalMinutes > durationMinutes )
			{
				SendLine( from, "Performance Test long-run interval must be between 1 and 60 minutes and no longer than the duration." );
				return false;
			}

			DateTime nowUtc = DateTime.UtcNow;
			DateTime nowLocal = DateTime.Now;
			string stamp = nowLocal.ToString( "yyyyMMdd-HHmmss" );
			string folder = Path.GetFullPath( Path.Combine( OutputRoot, String.Format( "PerformanceTest-v31604-longrun-{0}", stamp ) ) );
			Directory.CreateDirectory( folder );
			SpawnEntryDiagnostics.PerformanceSummary spawnBaseline = SpawnEntryDiagnostics.GetPerformanceTestSummary();
			bool captureStartup;

			lock ( m_Lock )
			{
				m_LongRunActive = true;
				m_LongRunDurationMinutes = durationMinutes;
				m_LongRunIntervalMinutes = intervalMinutes;
				m_LongRunStartedUtc = nowUtc;
				m_LongRunStartedLocal = nowLocal;
				m_LongRunEndUtc = nowUtc + TimeSpan.FromMinutes( durationMinutes );
				m_LongRunEndLocal = nowLocal + TimeSpan.FromMinutes( durationMinutes );
				captureStartup = m_LongRunCaptureStartupMarker;
				m_LongRunNextCaptureUtc = captureStartup ? nowUtc : nowUtc + TimeSpan.FromMinutes( intervalMinutes );
				m_LongRunCaptures = 0;
				m_LongRunErrors = 0;
				m_LongRunLastError = "none";
				m_State = TestState.Running;
				m_ActiveTestName = "Long-Run Diagnostics";
				m_ActiveModule = TestModule.LongRunDiagnostics;
				m_ActivePhase = captureStartup ? "Capturing startup marker" : "Waiting for next long-run capture";
				m_NextStep = captureStartup ? "Startup LagReport capture" : "Long-run LagReport capture";
				m_Plan.Clear();
				m_StepIndex = 0;
				m_Admin = from;
				m_AdminName = GetMobileName( from );
				m_AccountName = GetAccountName( from );
				m_StartedUtc = nowUtc;
				m_StartedLocal = nowLocal;
				m_LastUpdateUtc = nowUtc;
				m_CurrentFamily = "diagnostics-only";
				m_LastCompletedStep = "none";
				m_LastReportPath = "none";
				m_LastSummaryPath = "none";
				m_OutputFolder = folder;
				m_LastError = "none";
				m_OverallResult = "INFO";
				m_CancelRequested = false;
				m_Events.Clear();
				m_ReportFiles.Clear();
				m_LastResults.Clear();
				m_BaselineSpawnDirectSchedules = spawnBaseline.DirectSchedules;
				m_BaselineSpawnUnknownCallbacks = spawnBaseline.UnknownOriginCallbacks;
				m_BaselineSpawnCallbackMs = spawnBaseline.TotalCallbackMs;
			}

			AddEvent( String.Format( "Started long-run diagnostics duration={0}m interval={1}m by {2}", durationMinutes, intervalMinutes, GetMobileName( from ) ) );
			EnsureRefreshTimer();
			ScheduleLongRunCapture( captureStartup ? TimeSpan.FromMilliseconds( 250.0 ) : TimeSpan.FromMinutes( intervalMinutes ) );
			OpenDashboardAndProgress( from );
			return true;
		}

		public static void StopLongRun( Mobile from )
		{
			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			bool active;

			lock ( m_Lock )
				active = m_LongRunActive;

			if ( !active )
			{
				SendLine( from, "Performance Test: no long-run diagnostics session is active." );
				return;
			}

			FinishLongRun( "stopped by administrator", true );
			OpenGump( from, 4 );
		}

		private static void ScheduleLongRunCapture( TimeSpan delay )
		{
			lock ( m_Lock )
			{
				if ( m_LongRunTimer != null )
					m_LongRunTimer.Stop();

				m_LongRunTimer = new LongRunTimer( delay );
				m_LongRunTimer.Start();
			}
		}

		private static void ProcessLongRunCapture()
		{
			bool active;
			DateTime endUtc;
			int intervalMinutes;

			lock ( m_Lock )
			{
				m_LongRunTimer = null;
				active = m_LongRunActive;
				endUtc = m_LongRunEndUtc;
				intervalMinutes = m_LongRunIntervalMinutes;
			}

			if ( !active )
				return;

			if ( DateTime.UtcNow >= endUtc )
			{
				FinishLongRun( "complete", false );
				return;
			}

			try
			{
				CaptureLongRunLagReport();
			}
			catch ( Exception ex )
			{
				lock ( m_Lock )
				{
					++m_LongRunErrors;
					m_LongRunLastError = String.Format( "{0}: {1}", ex.GetType().FullName, ex.Message );
					m_LastError = m_LongRunLastError;
				}

				AddEvent( "Long-run capture error: " + m_LongRunLastError );
			}

			lock ( m_Lock )
			{
				if ( !m_LongRunActive )
					return;

				m_ActivePhase = "Waiting for next long-run capture";
				m_NextStep = "Long-run LagReport capture";
				m_LongRunNextCaptureUtc = DateTime.UtcNow + TimeSpan.FromMinutes( intervalMinutes );
				m_LastUpdateUtc = DateTime.UtcNow;
			}

			TimeSpan remaining = endUtc - DateTime.UtcNow;

			if ( remaining <= TimeSpan.Zero )
				FinishLongRun( "complete", false );
			else
				ScheduleLongRunCapture( remaining < TimeSpan.FromMinutes( intervalMinutes ) ? remaining : TimeSpan.FromMinutes( intervalMinutes ) );
		}

		private static void CaptureLongRunLagReport()
		{
			string folder;
			string stamp;
			int captureNumber;

			lock ( m_Lock )
			{
				m_State = TestState.CapturingLagReport;
				m_ActivePhase = "Capturing long-run LagReport";
				folder = m_OutputFolder;
				captureNumber = m_LongRunCaptures + 1;
				stamp = m_LongRunStartedLocal.ToString( "yyyyMMdd-HHmmss" );
			}

			string fileName = String.Format( "PerformanceTest-v31604-longrun-{0}-capture{1:000}-LagReport", stamp, captureNumber );
			string path = LagDiagnostics.GenerateLagReport( GetAdmin(), folder, fileName );

			lock ( m_Lock )
			{
				++m_LongRunCaptures;
				m_LastReportPath = path;
				AddReportPathLocked( path );
				m_State = TestState.Running;
				m_LastCompletedStep = "Long-run capture " + m_LongRunCaptures;
				m_LastUpdateUtc = DateTime.UtcNow;
			}

			AddEvent( "Captured long-run LagReport: " + path );
			RefreshGump();
		}

		private static void FinishLongRun( string result, bool stopped )
		{
			bool writeSummary;

			lock ( m_Lock )
			{
				if ( !m_LongRunActive )
					return;

				m_LongRunActive = false;
				m_LongRunNextCaptureUtc = DateTime.MinValue;

				if ( m_LongRunTimer != null )
				{
					m_LongRunTimer.Stop();
					m_LongRunTimer = null;
				}

				m_State = stopped ? TestState.Cancelled : TestState.Complete;
				m_ActivePhase = stopped ? "Long-run stopped by administrator" : "Long-run complete";
				m_NextStep = "none";
				m_LastCompletedStep = m_ActivePhase;
				m_LastUpdateUtc = DateTime.UtcNow;
				writeSummary = m_LongRunEndSummary && m_AutoGenerateFinalSummary;
			}

			AddEvent( "Long-run diagnostics " + result + "." );
			EvaluateCurrentTest();

			if ( writeSummary )
			{
				try
				{
					WriteSummaryFile( "long-run " + result );
				}
				catch ( Exception ex )
				{
					lock ( m_Lock )
					{
						++m_LongRunErrors;
						m_LongRunLastError = String.Format( "{0}: {1}", ex.GetType().FullName, ex.Message );
						m_LastError = m_LongRunLastError;
					}
				}
			}

			RefreshGump();
		}

		private sealed class LongRunTimer : Timer
		{
			public LongRunTimer( TimeSpan delay ) : base( delay )
			{
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				ProcessLongRunCapture();
			}

			public override string ToString()
			{
				return GetType().FullName + " / Long-Run LagReport";
			}
		}

		public static bool RunManualCommand( Mobile from, string family )
		{
			string reason;

			if ( !CanStart( from, false, out reason ) )
			{
				SendLine( from, reason );
				return false;
			}

			return RunTaskInternal(
				from,
				family,
				true,
				delegate( bool success, Exception error )
				{
					if ( success )
					{
						StartSettlingGuard( family, GetSettlingSeconds( family ), "Manual command settling guard" );
						OpenDashboardAndProgress( from );
					}
				} );
		}

		public static void Cancel( Mobile from )
		{
			bool longRun;
			bool cancelNow = false;

			lock ( m_Lock )
				longRun = m_LongRunActive;

			if ( longRun )
			{
				FinishLongRun( "cancelled by administrator", true );
				OpenGump( from );
				return;
			}

			lock ( m_Lock )
			{
				if ( m_State == TestState.Idle || m_State == TestState.Complete || m_State == TestState.Failed || m_State == TestState.Cancelled )
				{
					SendLine( from, "TaskManagerTest: no automated test is running." );
					return;
				}

				m_CancelRequested = true;

				if ( m_InFlight )
				{
					m_ActivePhase = "Cancellation requested; waiting for current task to finish";
				}
				else
				{
					if ( m_TestTimer != null )
					{
						m_TestTimer.Stop();
						m_TestTimer = null;
					}

					FinishCancelledLocked( "Cancelled after current step." );
					cancelNow = true;
				}

				m_LastUpdateUtc = DateTime.UtcNow;
			}

			AddEvent( "Cancellation requested by " + GetMobileName( from ) );

			if ( cancelNow )
			{
				try
				{
					EvaluateCurrentTest();
					WriteSummaryFile( "cancelled" );
				}
				catch ( Exception ex )
				{
					lock ( m_Lock )
						m_LastError = "Cancelled-summary error: " + ex.Message;
				}
			}

			OpenGump( from );
		}

		public static void ResetDisplay( Mobile from )
		{
			lock ( m_Lock )
			{
				if ( m_InFlight )
				{
					SendLine( from, "TaskManagerTest: cannot reset while a task is in flight." );
					return;
				}

				if ( m_State == TestState.Running || m_State == TestState.CapturingLagReport )
				{
					SendLine( from, "TaskManagerTest: cancel the active test before resetting display." );
					return;
				}

				m_State = TestState.Idle;
				m_ActiveTestName = "none";
				m_ActiveModule = TestModule.LagReportOnly;
				m_ActivePhase = "Idle";
				m_NextStep = "none";
				m_Plan.Clear();
				m_StepIndex = 0;
				m_CurrentFamily = "none";
				m_LastError = "none";
				m_OverallResult = "INFO";
				m_CancelRequested = false;
				m_LastResults.Clear();
				m_LastUpdateUtc = DateTime.UtcNow;
			}

			AddEvent( "Display reset by " + GetMobileName( from ) );
			OpenGump( from );
		}

		public static void OverrideSettlingGuard( Mobile from )
		{
			lock ( m_Lock )
			{
				if ( !m_Settling )
				{
					SendLine( from, "TaskManagerTest: no settling guard is active." );
					return;
				}

				AddEventLocked( String.Format( "Settling guard overridden by {0}; family={1} untilUtc={2}", GetMobileName( from ), m_SettlingFamily, m_SettlingUntilUtc ) );
				m_Settling = false;
				m_SettlingFamily = "none";
				m_SettlingReason = "none";
				m_SettlingUntilUtc = DateTime.MinValue;
				m_SettlingUntilLocal = DateTime.MinValue;
				m_LastUpdateUtc = DateTime.UtcNow;
			}

			DiagnosticConsole.WriteLine( "[TaskManagerTest]", DiagnosticSeverity.Warn, "Settling guard overridden by {0} account={1}", GetMobileName( from ), GetAccountName( from ) );
			OpenGump( from );
		}

		private static void ProcessNextStep()
		{
			TestStep step = null;
			bool cancelledBeforeStep = false;
			bool completedWithoutStep = false;

			lock ( m_Lock )
			{
				if ( m_CancelRequested )
				{
					FinishCancelledLocked( "Cancelled before next step." );
					cancelledBeforeStep = true;
				}
				else if ( m_StepIndex >= m_Plan.Count )
				{
					FinishCompleteLocked();
					completedWithoutStep = true;
				}
				else
				{
					step = m_Plan[m_StepIndex];
					m_State = step.Type == TestStepType.CaptureLagReport ? TestState.CapturingLagReport : TestState.Running;
					m_ActivePhase = step.Name;
					m_NextStep = m_StepIndex + 1 < m_Plan.Count ? m_Plan[m_StepIndex + 1].Name : "none";
					m_CurrentFamily = !String.IsNullOrWhiteSpace( step.Family ) ? step.Family : "none";
					m_LastUpdateUtc = DateTime.UtcNow;
				}
			}

			if ( cancelledBeforeStep )
			{
				try
				{
					EvaluateCurrentTest();
					WriteSummaryFile( "cancelled" );
				}
				catch ( Exception ex )
				{
					lock ( m_Lock )
						m_LastError = "Cancelled-summary error: " + ex.Message;
				}

				RefreshGump();
				return;
			}

			if ( completedWithoutStep )
			{
				RefreshGump();
				return;
			}

			RefreshGump();

			try
			{
				if ( step.Type == TestStepType.Preflight )
				{
					RunPreflight();
					CompleteStep( step.Name );
					ScheduleNextStep( TimeSpan.FromMilliseconds( 250.0 ) );
				}
				else if ( IsTaskStep( step.Type ) )
				{
					bool accepted = RunTaskInternal(
						GetAdmin(),
						step.Family,
						false,
						delegate( bool success, Exception error )
						{
							if ( !success )
							{
								FailTest( error != null ? error : new InvalidOperationException( "TaskManager manual step failed." ) );
								return;
							}

							CompleteStep( step.Name );

							if ( IsCancellationRequested() )
								CancelAfterStep();
							else
								ScheduleNextStep( TimeSpan.FromMilliseconds( 250.0 ) );
						} );

					if ( !accepted )
						return;
				}
				else if ( step.Type == TestStepType.CaptureLagReport )
				{
					bool capture;

					lock ( m_Lock )
						capture = m_AutoGenerateLagReportEveryStep || m_ActiveModule == TestModule.LagReportOnly;

					if ( capture )
						CaptureLagReport( step.ReportLabel );
					else
						AddEvent( "Skipped automatic LagReport by in-memory setting: " + step.ReportLabel );

					CompleteStep( step.Name );
					ScheduleNextStep( TimeSpan.FromMilliseconds( 250.0 ) );
				}
				else if ( step.Type == TestStepType.WaitSettling )
				{
					StartSettlingGuard( step.Family, step.SettlingSeconds, step.Name );
					CompleteStep( step.Name );
					ScheduleNextStep( TimeSpan.FromSeconds( step.SettlingSeconds ) );
				}
				else if ( step.Type == TestStepType.RunBalancePortalValidate )
				{
					RunBalancePortalValidate();
					CompleteStep( step.Name );
					ScheduleNextStep( TimeSpan.FromMilliseconds( 250.0 ) );
				}
				else if ( step.Type == TestStepType.CaptureStats )
				{
					AddEvent( "Captured in-memory diagnostics checkpoint: " + step.Name );
					CompleteStep( step.Name );
					ScheduleNextStep( TimeSpan.FromMilliseconds( 250.0 ) );
				}
				else if ( step.Type == TestStepType.RunManualSavePromptOnly )
				{
					SendLine( GetAdmin(), "Performance Test reminder: manual save/restart validation is required. The suite will not save or restart automatically." );
					CompleteStep( step.Name );
					ScheduleNextStep( TimeSpan.FromMilliseconds( 250.0 ) );
				}
				else if ( step.Type == TestStepType.LongRunSample )
				{
					CaptureLagReport( step.ReportLabel );
					CompleteStep( step.Name );
					ScheduleNextStep( TimeSpan.FromMilliseconds( 250.0 ) );
				}
				else if ( step.Type == TestStepType.Summary )
				{
					bool writeSummary;

					lock ( m_Lock )
						writeSummary = m_AutoGenerateFinalSummary;

					EvaluateCurrentTest();

					if ( writeSummary )
						WriteSummaryFile( "complete" );
					else
						AddEvent( "Skipped automatic final summary by in-memory setting." );

					CompleteStep( step.Name );

					lock ( m_Lock )
					{
						FinishCompleteLocked();
					}

					RefreshGump();
				}
			}
			catch ( Exception ex )
			{
				FailTest( ex );
			}
		}

		private static void RunPreflight()
		{
			if ( World.Saving )
				throw new InvalidOperationException( "World is saving during preflight." );

			AddEvent( String.Format( "Preflight ok. partialSends={0} timerMax={1}ms", NetState.PartialSends, Timer.MaxSliceDuration ) );
		}

		private static bool RunTaskInternal( Mobile from, string family, bool commandInitiated, ManualExecutionCompleted completion )
		{
			if ( World.Saving )
			{
				SendLine( from, "TaskManagerTest refused: the world is currently saving. Nothing was queued." );

				if ( completion != null )
					completion( false, new InvalidOperationException( "The world is currently saving." ) );

				return false;
			}

			string command = GetManualCommandName( family );
			string admin = GetMobileName( from );
			string account = GetAccountName( from );
			bool forced = IsForcedFamily( family );
			ScheduleState oldSchedule = GetSchedule( family );
			string oldScheduleText = FormatSchedule( oldSchedule );

			if ( !BeginManual( family, command, admin, account ) )
			{
				SendLine( from, String.Format( "TaskManagerTest refused: {0} is already running, started by {1} at UTC {2}.", m_InFlightCommand, m_InFlightAdmin, m_InFlightUtc ) );

				if ( completion != null )
					completion( false, new InvalidOperationException( "Another TaskManager test is already in flight." ) );

				return false;
			}

			ManualExecution execution = new ManualExecution();
			execution.From = from;
			execution.Family = family;
			execution.Command = command;
			execution.Admin = admin;
			execution.Account = account;
			execution.Forced = forced;
			execution.StartedUtc = DateTime.UtcNow;
			execution.StartedLocal = DateTime.Now;
			execution.StartedTimestamp = TaskManagerDiagnostics.GetTimestamp();
			execution.OldSchedule = oldScheduleText;
			execution.Completion = completion;

			DiagnosticConsole.WriteLine( "[TaskManagerTest]", DiagnosticSeverity.Info, "START command={0} manual=True forced={1} admin={2} account={3} utc={4} local={5} oldSchedule={6}", command, forced, admin, account, execution.StartedUtc, execution.StartedLocal, oldScheduleText );
			SendLine( from, String.Format( "TaskManagerTest starting {0}. manual=True forced={1}", command, forced ) );
			SendLine( from, String.Format( "Old schedule: {0}", oldScheduleText ) );
			EnsureRefreshTimer();

			if ( family == "daily" )
			{
				bool started;

				try
				{
					started = Server.Items.TaskManagerDaily.StartRun(
						"TaskManagerDaily+ManualCommand",
						false,
						true,
						delegate( bool success, Exception error, long elapsedMs )
						{
							FinishManualExecution( execution, success ? null : error, commandInitiated );
						} );
				}
				catch ( Exception ex )
				{
					FinishManualExecution( execution, ex, commandInitiated );
					return false;
				}

				if ( !started && !execution.Finished )
				{
					FinishManualExecution( execution, new InvalidOperationException( "TaskManagerDaily is already in progress." ), commandInitiated );
					return false;
				}

				return started;
			}

			try
			{
				InvokeTask( family );
				FinishManualExecution( execution, null, commandInitiated );
				return true;
			}
			catch ( Exception ex )
			{
				FinishManualExecution( execution, ex, commandInitiated );
				return false;
			}
		}

		private static void FinishManualExecution( ManualExecution execution, Exception error, bool commandInitiated )
		{
			if ( execution == null || execution.Finished )
				return;

			execution.Finished = true;
			long elapsed = TaskManagerDiagnostics.GetElapsedMilliseconds( execution.StartedTimestamp );
			ScheduleState newSchedule = GetSchedule( execution.Family );
			string newScheduleText = FormatSchedule( newSchedule );
			string result = error == null ? "complete" : "error";
			string errorText = error == null ? "none" : String.Format( "{0}: {1}", error.GetType().FullName, error.Message );

			if ( error != null )
				DiagnosticConsole.WriteLine( "[TaskManagerTest]", DiagnosticSeverity.Error, "ERROR command={0} admin={1} account={2} error={3}", execution.Command, execution.Admin, execution.Account, errorText );

			RecordManualResult(
				execution.Family,
				execution.Command,
				execution.Admin,
				execution.Account,
				execution.Forced,
				execution.StartedUtc,
				execution.StartedLocal,
				elapsed,
				result,
				errorText,
				execution.OldSchedule,
				newScheduleText );

			if ( error == null && execution.Family == "daily" )
				CaptureCurrentTestDailyAttribution( execution );

			EndManual();

			DiagnosticConsole.WriteLine(
				"[TaskManagerTest]",
				error == null ? DiagnosticSeverity.Good : DiagnosticSeverity.Error,
				"END command={0} manual=True forced={1} admin={2} account={3} elapsed={4}ms result={5} newSchedule={6}",
				execution.Command,
				execution.Forced,
				execution.Admin,
				execution.Account,
				elapsed,
				result,
				newScheduleText );

			if ( error == null )
				SendLine( execution.From, String.Format( "TaskManagerTest {0} complete in {1}ms.", execution.Command, elapsed ) );
			else
				SendLine( execution.From, String.Format( "TaskManagerTest {0} failed after {1}ms: {2}", execution.Command, elapsed, errorText ) );

			SendLine( execution.From, String.Format( "New schedule: {0}", newScheduleText ) );
			AddEvent( String.Format( "Ran {0} elapsed={1}ms result={2} commandInitiated={3}", execution.Command, elapsed, result, commandInitiated ) );

			if ( execution.Completion != null )
				execution.Completion( error == null, error );
		}

		private static void CaptureCurrentTestDailyAttribution( ManualExecution execution )
		{
			if ( execution == null )
				return;

			TaskManagerDailyAttributionDiagnostics.PerformanceSummary summary = TaskManagerDailyAttributionDiagnostics.GetPerformanceSummary();
			string module = "none";
			bool captured = false;

			lock ( m_Lock )
			{
				if ( summary.Available
					&& summary.RunId > 0
					&& String.Equals( summary.TimerName, "TaskManagerDaily+ManualCommand", StringComparison.Ordinal )
					&& summary.StartedUtc >= execution.StartedUtc
					&& m_StartedUtc != DateTime.MinValue
					&& execution.StartedUtc >= m_StartedUtc )
				{
					m_CurrentTestDailyAttribution = summary;
					m_CurrentTestDailyAttributionModule = m_ActiveModule.ToString();
					module = m_CurrentTestDailyAttributionModule;
					captured = true;
				}
			}

			if ( captured )
				AddEvent( String.Format( "Captured current-test Daily attribution runId={0} startedUtc={1} timer={2} module={3}", summary.RunId, summary.StartedUtc, summary.TimerName, module ) );
		}

		private static bool TryGetCurrentTestDailyAttribution( out TaskManagerDailyAttributionDiagnostics.PerformanceSummary summary, out string module )
		{
			lock ( m_Lock )
			{
				summary = m_CurrentTestDailyAttribution;
				module = m_CurrentTestDailyAttributionModule;
				return summary != null && summary.Available;
			}
		}

		private static void InvokeTask( string family )
		{
			if ( family == "hourly" )
			{
				Server.Items.TaskManager.RunThis( "TaskManager+ManualCommand", false );
			}
			else if ( family == "150" )
			{
				Server.Items.TaskManager150Min.RunThis( false, "TaskManager150Min+ManualCommand", false );
			}
			else if ( family == "200" )
			{
				Server.Items.TaskManager200Min.RunThis( false, "TaskManager200Min+ManualCommand", false );
			}
			else if ( family == "250" )
			{
				Server.Items.TaskManager250Min.RunThis( false, "TaskManager250Min+ManualCommand", false, true );
			}
			else if ( family == "daily" )
			{
				throw new InvalidOperationException( "Daily manual runs must use the staged completion path." );
			}
		}

		private static void CaptureLagReport( string label )
		{
			string folder;
			string fileName;
			int step;

			lock ( m_Lock )
			{
				folder = m_OutputFolder;
				step = m_StepIndex + 1;
				fileName = String.Format( "PerformanceTest-v31604-{0}-step{1:00}-{2}-LagReport",
					m_StartedLocal != DateTime.MinValue ? m_StartedLocal.ToString( "yyyyMMdd-HHmmss" ) : DateTime.Now.ToString( "yyyyMMdd-HHmmss" ),
					step,
					!String.IsNullOrWhiteSpace( label ) ? label : "checkpoint" );
			}

			if ( String.IsNullOrWhiteSpace( folder ) || folder == "none" )
				folder = Path.GetFullPath( OutputRoot );

			string path = LagDiagnostics.GenerateLagReport( GetAdmin(), folder, fileName );

			lock ( m_Lock )
			{
				m_LastReportPath = path;
				AddReportPathLocked( path );
				m_LastUpdateUtc = DateTime.UtcNow;
			}

			AddEvent( "Captured LagReport: " + path );
		}

		private static void RunBalancePortalValidate()
		{
			string folder;
			string fileName;

			lock ( m_Lock )
			{
				folder = m_OutputFolder;
				fileName = String.Format( "PerformanceTest-v31604-{0}-BalancePortalValidate",
					m_StartedLocal != DateTime.MinValue ? m_StartedLocal.ToString( "yyyyMMdd-HHmmss" ) : DateTime.Now.ToString( "yyyyMMdd-HHmmss" ) );
			}

			if ( String.IsNullOrWhiteSpace( folder ) || folder == "none" )
				folder = Path.GetFullPath( OutputRoot );

			Directory.CreateDirectory( folder );
			string path = GetUniqueTextPath( folder, fileName );

			using ( StreamWriter writer = new StreamWriter( path, false ) )
			{
				writer.WriteLine( "BEGIN BALANCEPORTALVALIDATE" );
				writer.WriteLine( "This intentionally runs the old expensive full-scan comparison and is excluded from normal performance timing." );
				Server.Items.TaskManagerDaily.WriteBalancePortalValidation( writer );
				writer.WriteLine( "END BALANCEPORTALVALIDATE" );
			}

			path = Path.GetFullPath( path );

			lock ( m_Lock )
			{
				AddReportPathLocked( path );
				m_LastReportPath = path;
				m_LastUpdateUtc = DateTime.UtcNow;
			}

			AddEvent( "Captured BalancePortalValidate: " + path );
		}

		private static void EvaluateCurrentTest()
		{
			List<TestResult> results = new List<TestResult>();
			AddResult( results, "Partial sends", NetState.PartialSends == 0 ? TestResultLevel.Pass : TestResultLevel.Warn,
				NetState.PartialSends == 0 ? "No partial sends observed." : "Partial sends were observed; inspect PacketStats and network logs.",
				NetState.PartialSends.ToString(), "PASS = 0" );

			BalancePortalDiagnostics.Sample balance;

			if ( BalancePortalDiagnostics.TryGetLatestSample( out balance ) )
				AddDurationResult( results, "balanceChampionAndPortals", balance.TotalPhaseMs, BalancePortalPassMs, BalancePortalFailMs );
			else
				AddResult( results, "balanceChampionAndPortals", TestResultLevel.Info, "No BalancePortal sample is available.", "Unavailable", "PASS <= 500ms; FAIL > 2000ms" );

			AddManualDurationResult( results, "daily", "TaskManagerDaily", TaskManagerDailyPassMs, TaskManagerDailyFailMs );
			AddManualDurationResult( results, "150", "TaskManager150Min", TaskManager150PassMs, TaskManager150FailMs );
			AddManualDurationResult( results, "250", "TaskManager250Min", TaskManager250PassMs, TaskManager250FailMs );
			AddDailyAttributionResults( results );

			SpawnEntryDiagnostics.PerformanceSummary spawn = SpawnEntryDiagnostics.GetPerformanceTestSummary();
			long directSchedules = Math.Max( 0, spawn.DirectSchedules - m_BaselineSpawnDirectSchedules );
			long unknownCallbacks = Math.Max( 0, spawn.UnknownOriginCallbacks - m_BaselineSpawnUnknownCallbacks );
			long callbackMs = Math.Max( 0, spawn.TotalCallbackMs - m_BaselineSpawnCallbackMs );
			ManualSample manual150 = GetLastManual( "150" );

			if ( IsSampleInCurrentTest( manual150 ) )
			{
				TestResultLevel scheduleLevel = directSchedules >= SpawnEntryExpectedDirectSchedulesMin && directSchedules <= SpawnEntryExpectedDirectSchedulesMax ? TestResultLevel.Pass : TestResultLevel.Warn;
				AddResult( results, "SpawnEntry direct schedules", scheduleLevel,
					scheduleLevel == TestResultLevel.Pass ? "Direct TaskManager150 schedule count is inside the expected diagnostic range." : "Direct TaskManager150 schedule count is outside the expected range.",
					directSchedules.ToString(), "PASS = 150-250" );
				AddResult( results, "SpawnEntry callback work", callbackMs > SpawnEntryCallbackWorkWarnMs ? TestResultLevel.Warn : TestResultLevel.Pass,
					callbackMs > SpawnEntryCallbackWorkWarnMs ? "Measured SpawnEntry callback work exceeded the warning threshold." : "Measured SpawnEntry callback work stayed within the current threshold.",
					callbackMs + "ms", "WARN > 1000ms" );
			}
			else
			{
				AddResult( results, "SpawnEntry direct schedules", TestResultLevel.Info, "This module did not run TaskManager150.", "Not sampled", "PASS = 150-250 during a 150 test" );
			}

			AddResult( results, "SpawnEntry unknown-origin callbacks", unknownCallbacks == 0 ? TestResultLevel.Pass : TestResultLevel.Fail,
				unknownCallbacks == 0 ? "Every callback observed during the module had a known origin." : "Unknown-origin callbacks were observed.",
				unknownCallbacks.ToString(), "FAIL > 0" );

			int physicalItemScans;
			long collectMs;
			Statistics.GetPerformanceTestSummary( out physicalItemScans, out collectMs );
			AddResult( results, "CollectStats physical item scans", physicalItemScans == 0 ? TestResultLevel.Pass : TestResultLevel.Warn,
				physicalItemScans == 0 ? "CollectStats used the optimized registry path." : "CollectStats reported a physical item scan.",
				physicalItemScans.ToString(), "PASS = 0" );

			long oldInternalCallbacks;
			long oldestReadyMs;
			long maxSingleEntryMs;
			long maxSingleSpawnMs;
			PremiumSpawner.GetPerformanceTestSummary( out oldInternalCallbacks, out oldestReadyMs, out maxSingleEntryMs, out maxSingleSpawnMs );
			AddResult( results, "PremiumSpawner old internal callbacks", oldInternalCallbacks == 0 ? TestResultLevel.Pass : TestResultLevel.Warn,
				oldInternalCallbacks == 0 ? "The old per-spawner timer path remained inactive." : "Old internal timer callbacks were observed.",
				oldInternalCallbacks.ToString(), "PASS = 0" );

			TestResultLevel premiumReadyLevel = oldestReadyMs > PremiumSpawnerOldestReadyFailMs
				? TestResultLevel.Fail
				: ( oldestReadyMs > PremiumSpawnerOldestReadyWarnMs ? TestResultLevel.Warn : TestResultLevel.Pass );
			AddResult( results, "PremiumSpawner oldest ready", premiumReadyLevel,
				premiumReadyLevel == TestResultLevel.Pass ? "Oldest-ready age is within target." : "Oldest-ready age exceeded the configured performance threshold.",
				oldestReadyMs + "ms", "WARN > 15000ms; FAIL > 30000ms" );

			AddPremiumSpawnerSingleCallResult( results, "PremiumSpawner max single entry", maxSingleEntryMs );
			AddPremiumSpawnerSingleCallResult( results, "PremiumSpawner max single spawn subphase", maxSingleSpawnMs );

			string topConstructorMaxType;
			long topConstructorMaxMs;
			string topConstructorTotalType;
			long topConstructorTotalMs;
			long topConstructorTotalCount;
			PremiumSpawner.GetConstructorPerformanceTestSummary(
				out topConstructorMaxType,
				out topConstructorMaxMs,
				out topConstructorTotalType,
				out topConstructorTotalMs,
				out topConstructorTotalCount );
			AddPremiumSpawnerConstructorResult( results, topConstructorMaxType, topConstructorMaxMs );
			AddResult( results, "PremiumSpawner top constructor type by max", TestResultLevel.Info,
				"Highest retained single constructor sample by concrete constructed type.",
				String.Format( "{0} {1}ms", topConstructorMaxType, topConstructorMaxMs ),
				"Informational; constructor evaluation shown separately" );
			AddResult( results, "PremiumSpawner top constructor type by total", TestResultLevel.Info,
				"Highest retained aggregate constructor time by concrete constructed type.",
				String.Format( "{0} total={1}ms count={2} avg={3}ms", topConstructorTotalType, topConstructorTotalMs, topConstructorTotalCount, topConstructorTotalCount > 0 ? topConstructorTotalMs / topConstructorTotalCount : 0 ),
				"Informational" );

			string citizenConstructorType;
			long citizenConstructorMaxMs;
			string citizenTopPhase;
			long citizenTopPhaseMs;
			string citizenTopSetupPhase;
			long citizenTopSetupPhaseMs;
			CitizenConstructorDiagnostics.GetPerformanceSummary(
				out citizenConstructorType,
				out citizenConstructorMaxMs,
				out citizenTopPhase,
				out citizenTopPhaseMs,
				out citizenTopSetupPhase,
				out citizenTopSetupPhaseMs );
			AddResult( results, "Citizen constructor top subphase", TestResultLevel.Info,
				"Highest retained Citizen-family constructor and its aggregate major subphase.",
				String.Format( "{0} max={1}ms top={2}:{3}ms", citizenConstructorType, citizenConstructorMaxMs, citizenTopPhase, citizenTopPhaseMs ),
				"Informational; PremiumSpawner-created Citizen-family mobiles only" );
			AddResult( results, "Citizen constructor setup detail", TestResultLevel.Info,
				"Highest retained detailed SetupCitizen subphase.",
				String.Format( "{0}:{1}ms", citizenTopSetupPhase, citizenTopSetupPhaseMs ),
				"Informational" );

			string citizenPhraseMaxHelper;
			long citizenPhraseMaxMs;
			string citizenPhraseTotalHelper;
			long citizenPhraseTotalMs;
			string citizenPhraseCountHelper;
			long citizenPhraseHelperCalls;
			string citizenPhraseWorstSample;
			CitizenConstructorDiagnostics.GetPhrasePerformanceSummary(
				out citizenPhraseMaxHelper,
				out citizenPhraseMaxMs,
				out citizenPhraseTotalHelper,
				out citizenPhraseTotalMs,
				out citizenPhraseCountHelper,
				out citizenPhraseHelperCalls,
				out citizenPhraseWorstSample );

			TestResultLevel citizenPhraseLevel = TestResultLevel.Info;
			string citizenPhraseReason = "No Citizen phrase/rumor nested helper sample is available.";

			if ( citizenPhraseMaxHelper != "Not sampled" )
			{
				citizenPhraseLevel = citizenPhraseMaxMs <= 25 ? TestResultLevel.Pass : ( citizenPhraseMaxMs <= 75 ? TestResultLevel.Warn : TestResultLevel.Fail );
				citizenPhraseReason = citizenPhraseMaxMs <= 25
					? "Citizen phrase/rumor nested helper maximum is within the diagnostic target."
					: ( citizenPhraseMaxMs <= 75
						? "Citizen phrase/rumor nested helper maximum is above the preferred target."
						: "Citizen phrase/rumor nested helper maximum exceeds the diagnostic failure threshold." );
			}

			AddResult( results, "Citizen phrase/rumor nested helper max", citizenPhraseLevel,
				citizenPhraseReason,
				citizenPhraseMaxHelper == "Not sampled" ? "Not sampled" : String.Format( "{0}:{1}ms", citizenPhraseMaxHelper, citizenPhraseMaxMs ),
				"PASS <= 25ms; WARN <= 75ms; FAIL > 75ms" );
			AddResult( results, "Citizen phrase/rumor nested helper totals", TestResultLevel.Info,
				"Highest retained nested helper by exclusive total and call count.",
				String.Format( "total={0}:{1}ms count={2}:{3}", citizenPhraseTotalHelper, citizenPhraseTotalMs, citizenPhraseCountHelper, citizenPhraseHelperCalls ),
				"Informational" );
			AddResult( results, "Citizen phrase/rumor exact worst sample", TestResultLevel.Info,
				"Exact retained Citizen constructor context for the worst sample.",
				citizenPhraseWorstSample,
				"Informational; no live Mobile or Item references retained" );

			long discordTimeouts;
			long discordErrors;
			DiscordBridge.GetPerformanceTestSummary( out discordTimeouts, out discordErrors );
			AddResult( results, "DiscordBridge polling", discordTimeouts == 0 && discordErrors == 0 ? TestResultLevel.Pass : TestResultLevel.Warn,
				discordTimeouts == 0 && discordErrors == 0 ? "No Discord polling timeout or error has been recorded." : "Discord polling recorded a timeout or error.",
				String.Format( "timeouts={0}, errors={1}", discordTimeouts, discordErrors ), "PASS = both 0" );

			AddResult( results, "AutoSave", TestResultLevel.Info, "AutoSave is informational; inspect SaveStats for timing.", "SaveDiagnostics mode=" + SaveDiagnostics.EffectiveMode, "Informational only; mode does not affect suite pass/fail" );

			lock ( m_Lock )
			{
				m_LastResults.Clear();
				m_LastResults.AddRange( results );
				m_OverallResult = GetOverallResult( results ).ToString().ToUpperInvariant();
				m_RecentModuleResults.Add( String.Format( "{0:u} {1}: {2}", DateTime.UtcNow, m_ActiveTestName, m_OverallResult ) );

				while ( m_RecentModuleResults.Count > MaxRecentModuleResults )
					m_RecentModuleResults.RemoveAt( 0 );
			}
		}

		private static void AddPremiumSpawnerSingleCallResult( List<TestResult> results, string name, long elapsedMs )
		{
			TestResultLevel level = elapsedMs > PremiumSpawnerSingleEntryFailMs
				? TestResultLevel.Fail
				: ( elapsedMs > PremiumSpawnerSingleEntryPassMs ? TestResultLevel.Warn : TestResultLevel.Pass );

			AddResult(
				results,
				name,
				level,
				level == TestResultLevel.Pass
					? "No retained PremiumSpawner sample exceeded the 100ms pass threshold."
					: ( elapsedMs > 1000
						? "A retained PremiumSpawner sample exceeded 1000ms and is critical."
						: ( level == TestResultLevel.Warn
							? "A retained PremiumSpawner sample exceeded 100ms."
							: "A retained PremiumSpawner sample exceeded 250ms." ) ),
				elapsedMs + "ms",
				"PASS <= 100ms; WARN <= 250ms; FAIL > 250ms; CRITICAL > 1000ms" );
		}

		private static void AddPremiumSpawnerConstructorResult( List<TestResult> results, string typeName, long elapsedMs )
		{
			TestResultLevel level = elapsedMs > PremiumSpawnerConstructorFailMs
				? TestResultLevel.Fail
				: ( elapsedMs > PremiumSpawnerConstructorPassMs ? TestResultLevel.Warn : TestResultLevel.Pass );

			AddResult(
				results,
				"PremiumSpawner max constructor",
				level,
				elapsedMs <= 0
					? "No PremiumSpawner constructor sample has been recorded."
					: ( elapsedMs > 1000
						? "A PremiumSpawner constructor exceeded 1000ms and is critical."
						: ( level == TestResultLevel.Pass
							? "The maximum PremiumSpawner constructor stayed within the 75ms pass threshold."
							: ( level == TestResultLevel.Warn
								? "The maximum PremiumSpawner constructor exceeded 75ms."
								: "The maximum PremiumSpawner constructor exceeded 150ms." ) ) ),
				String.Format( "{0} {1}ms", typeName, elapsedMs ),
				"PASS <= 75ms; WARN <= 150ms; FAIL > 150ms; CRITICAL > 1000ms" );
		}

		private static void AddManualDurationResult( List<TestResult> results, string family, string name, long passMs, long failMs )
		{
			ManualSample sample = GetLastManual( family );

			if ( !IsSampleInCurrentTest( sample ) )
			{
				AddResult( results, name, TestResultLevel.Info, "This module did not run this TaskManager family.", "Not sampled", String.Format( "PASS <= {0}ms; FAIL > {1}ms", passMs, failMs ) );
				return;
			}

			if ( family == "daily" )
			{
				TaskManagerDailyAttributionDiagnostics.PerformanceSummary daily;
				string dailyModule;

				if ( TryGetCurrentTestDailyAttribution( out daily, out dailyModule )
					&& !String.IsNullOrWhiteSpace( dailyModule )
					&& String.Equals( daily.CompletionMode, "staged", StringComparison.OrdinalIgnoreCase ) )
				{
					TestResultLevel wallLevel = sample.ElapsedMs > DailyWallClockFailMs
						? TestResultLevel.Fail
						: ( sample.ElapsedMs > DailyWallClockPassMs ? TestResultLevel.Warn : TestResultLevel.Pass );
					AddResult(
						results,
						name + " wall-clock",
						wallLevel,
						wallLevel == TestResultLevel.Pass
							? "Staged Daily completed within the wall-clock target."
							: ( wallLevel == TestResultLevel.Warn
								? "Staged Daily exceeded the 60-second wall-clock target."
								: "Staged Daily exceeded the 120-second wall-clock failure threshold." ),
						sample.ElapsedMs + "ms wall",
						"PASS <= 60000ms; WARN <= 120000ms; FAIL > 120000ms" );
					return;
				}
			}

			AddDurationResult( results, name, sample.ElapsedMs, passMs, failMs );
		}

		private static void AddDailyAttributionResults( List<TestResult> results )
		{
			TaskManagerDailyAttributionDiagnostics.PerformanceSummary summary;
			string module;
			bool sampled = TryGetCurrentTestDailyAttribution( out summary, out module );

			if ( !sampled )
			{
				AddResult( results, "Daily top offender", TestResultLevel.Info, "This module did not capture a TaskManagerDaily attribution sample.", "Not sampled", "Informational" );
				AddResult( results, "PopulateCities attribution", TestResultLevel.Info, "No current-test sample is available.", "Not sampled", "PASS <= 1000ms; severe > 4000ms" );
				AddResult( results, "PopulateVillages setupSpawn attribution", TestResultLevel.Info, "No current-test sample is available.", "Not sampled", "PASS <= 1000ms; severe > 4000ms" );
				AddResult( results, "PlantGardens attribution", TestResultLevel.Info, "No current-test sample is available.", "Not sampled", "PASS <= 250ms; severe > 1500ms" );
				AddResult( results, "SpreadOutCreatureCleanup attribution", TestResultLevel.Info, "No current-test sample is available.", "Not sampled", "PASS <= 250ms; severe > 1500ms" );
				AddResult( results, "SpreadOutCreatureCleanup max actual callback", TestResultLevel.Info, "No current-test sample is available.", "Not sampled", "PASS <= 100ms; WARN <= 250ms; FAIL > 250ms" );
				return;
			}

			AddResult(
				results,
				"Daily attribution sample",
				TestResultLevel.Info,
				"Bound to the exact manual Daily run completed by this test module.",
				String.Format( "runId={0}, startedUtc={1}, timer={2}, module={3}", summary.RunId, summary.StartedUtc, summary.TimerName, module ),
				"Current test module only" );

			AddResult(
				results,
				"Daily top offender",
				TestResultLevel.Info,
				"Latest diagnostics-only TaskManagerDaily attribution.",
				String.Format( "{0} {1}ms", !String.IsNullOrWhiteSpace( summary.TopGroup ) ? summary.TopGroup : "none", summary.TopGroupMs ),
				"Informational" );
			AddAttributionDurationResult( results, "PopulateCities attribution", summary.PopulateCitiesMs, PopulateCitiesPassMs, PopulateCitiesFailMs );
			AddAttributionDurationResult( results, "PopulateVillages setupSpawn attribution", summary.PopulateVillagesSetupSpawnMs, PopulateVillagesSetupSpawnPassMs, PopulateVillagesSetupSpawnFailMs );
			AddDailyCallbackResult(
				results,
				"PopulateVillages max actual callback",
				summary.VillageChunkMaxMs,
				String.Format( "callbacks={0}, wall={1}ms", summary.VillageChunkCount, summary.VillageStagedWallMs ) );
			AddDailyCallbackResult(
				results,
				"PopulateCities max actual callback",
				summary.CityChunkMaxMs,
				String.Format( "callbacks={0}, wall={1}ms", summary.CityChunkCount, summary.CityStagedWallMs ) );
			AddDailyCallbackResult(
				results,
				"PlantGardens max actual callback",
				summary.GardenPlacementCallbackCount > 0 ? summary.GardenPlacementCallbackMaxMs : summary.GardenChunkMaxMs,
				summary.GardenPlacementCallbackCount > 0
					? String.Format(
						"placementCallbacks={0}, placementWall={1}ms, allGardenCallbacks={2}",
						summary.GardenPlacementCallbackCount,
						summary.GardenPlacementWallMs,
						summary.GardenChunkCount )
					: String.Format( "callbacks={0}, wall={1}ms", summary.GardenChunkCount, summary.GardenStagedWallMs ) );
			AddDailyCallbackResult(
				results,
				"SpreadOutCreatureCleanup max actual callback",
				summary.SpreadOutCleanupCallbackMaxMs,
				String.Format(
					"callbacks={0}, wall={1}ms, kind={2}, callback={3}",
					summary.SpreadOutCleanupCallbackCount,
					summary.SpreadOutCleanupWallMs,
					!String.IsNullOrWhiteSpace( summary.SpreadOutCleanupCallbackMaxKind ) ? summary.SpreadOutCleanupCallbackMaxKind : "none",
					!String.IsNullOrWhiteSpace( summary.SpreadOutCleanupCallbackMaxName ) ? summary.SpreadOutCleanupCallbackMaxName : "none" ) );
			AddDailyCallbackResult(
				results,
				"StealingPedestals max actual callback",
				summary.StealingPedestalCallbackMaxMs,
				String.Format(
					"callbacks={0}, wall={1}ms, kind={2}, callback={3}",
					summary.StealingPedestalCallbackCount,
					summary.StealingPedestalWallMs,
					!String.IsNullOrWhiteSpace( summary.StealingPedestalCallbackMaxKind ) ? summary.StealingPedestalCallbackMaxKind : "none",
					!String.IsNullOrWhiteSpace( summary.StealingPedestalCallbackMaxName ) ? summary.StealingPedestalCallbackMaxName : "none" ) );
			AddResult(
				results,
				"InvestmentsAndBalance phase split",
				TestResultLevel.Info,
				"Investment, balance, champion announcement, and infection/invasion work run on separate behavior-equivalent DailyPhaseTimer ticks.",
				String.Format(
					"curse={0}ms scan={1}ms payout={2}ms stones/bones={3}ms balance={4}ms announcements={5}ms infection={6}ms invasion={7}ms",
					summary.InvestmentAetherCurseMs,
					summary.InvestmentItemScanMs,
					summary.InvestmentPayoutMs,
					summary.InvestmentStoneBonePopulateMs,
					summary.BalanceRunMs,
					summary.ChampionAnnouncementMs,
					summary.InfectionCheckMs,
					summary.InvasionRoutineMs ),
				"Informational; individual Daily callbacks use the normal 100/250/1000ms thresholds" );
			TestResultLevel callbackLevel = summary.StagedCallbackMaxMs > DailyStagedCallbackFailMs
				? TestResultLevel.Fail
				: ( summary.StagedCallbackMaxMs > DailyStagedCallbackPassMs ? TestResultLevel.Warn : TestResultLevel.Pass );
			AddResult(
				results,
				"Daily staged callback actual maximum",
				callbackLevel,
				callbackLevel == TestResultLevel.Pass
					? "No complete Daily staged timer callback exceeded the 100ms pass threshold."
					: ( callbackLevel == TestResultLevel.Warn
						? "A complete Daily staged timer callback exceeded 100ms."
						: ( summary.StagedCallbackMaxMs > DailyStagedCallbackCriticalMs
							? "A complete Daily staged timer callback exceeded 1000ms and is critical."
							: "A complete Daily staged timer callback exceeded 250ms." ) ),
				String.Format(
					"{0}ms / {1} / {2}",
					summary.StagedCallbackMaxMs,
					!String.IsNullOrWhiteSpace( summary.StagedCallbackMaxKind ) ? summary.StagedCallbackMaxKind : "unknown",
					!String.IsNullOrWhiteSpace( summary.StagedCallbackMaxName ) ? summary.StagedCallbackMaxName : "unknown" ),
				"PASS <= 100ms; WARN <= 250ms; FAIL > 250ms; CRITICAL > 1000ms" );
			TestResultLevel callbackCountLevel = summary.StagedCallbackCount > DailyStagedCallbackCountFail
				? TestResultLevel.Fail
				: ( summary.StagedCallbackCount > DailyStagedCallbackCountWarn ? TestResultLevel.Warn : TestResultLevel.Pass );
			AddResult(
				results,
				"Daily staged callback count",
				callbackCountLevel,
				callbackCountLevel == TestResultLevel.Pass
					? "Daily completed without excessive timer callback churn."
					: ( callbackCountLevel == TestResultLevel.Warn
						? "Daily used more than 3000 staged callbacks."
						: "Daily used more than 5000 staged callbacks." ),
				summary.StagedCallbackCount.ToString(),
				"PASS <= 3000; WARN <= 5000; FAIL > 5000" );
			AddAttributionDurationResult( results, "PlantGardens attribution", summary.PlantGardensMs, PlantGardensPassMs, PlantGardensFailMs );
			AddAttributionDurationResult( results, "SpreadOutCreatureCleanup attribution", summary.SpreadOutCreatureCleanupMs, SpreadOutCreatureCleanupPassMs, SpreadOutCreatureCleanupFailMs );
		}

		private static void AddDailyCallbackResult( List<TestResult> results, string name, long callbackMs, string details )
		{
			TestResultLevel level = callbackMs > DailyStagedCallbackFailMs
				? TestResultLevel.Fail
				: ( callbackMs > DailyStagedCallbackPassMs ? TestResultLevel.Warn : TestResultLevel.Pass );
			string reason = level == TestResultLevel.Pass
				? "The largest complete timer callback stayed within the 100ms pass threshold."
				: ( level == TestResultLevel.Warn
					? "A complete timer callback exceeded 100ms; inspect heavy single-operation diagnostics."
					: ( callbackMs > DailyStagedCallbackCriticalMs
						? "A complete timer callback exceeded 1000ms and is critical."
						: "A complete timer callback exceeded 250ms." ) );

			AddResult(
				results,
				name,
				level,
				reason,
				String.Format( "{0}ms, {1}", callbackMs, details ),
				"PASS <= 100ms; WARN <= 250ms; FAIL > 250ms; CRITICAL > 1000ms" );
		}

		private static void AddAttributionDurationResult( List<TestResult> results, string name, long valueMs, long passMs, long severeMs )
		{
			TestResultLevel level = valueMs <= passMs ? TestResultLevel.Pass : TestResultLevel.Warn;
			string reason;

			if ( valueMs <= passMs )
				reason = "Duration is within the attribution guidance threshold.";
			else if ( valueMs > severeMs )
				reason = "Duration exceeded the severe diagnostic threshold; attribution remains guidance-only and does not fail the suite.";
			else
				reason = "Duration exceeded the attribution guidance threshold.";

			AddResult( results, name, level, reason, valueMs + "ms", String.Format( "PASS <= {0}ms; severe > {1}ms", passMs, severeMs ) );
		}

		private static bool IsSampleInCurrentTest( ManualSample sample )
		{
			if ( sample == null )
				return false;

			lock ( m_Lock )
				return m_StartedUtc != DateTime.MinValue && sample.StartedUtc >= m_StartedUtc;
		}

		private static void AddDurationResult( List<TestResult> results, string name, long valueMs, long passMs, long failMs )
		{
			TestResultLevel level = valueMs > failMs ? TestResultLevel.Fail : ( valueMs > passMs ? TestResultLevel.Warn : TestResultLevel.Pass );
			string reason = level == TestResultLevel.Pass ? "Duration is within the pass threshold." : ( level == TestResultLevel.Warn ? "Duration exceeded the pass threshold." : "Duration exceeded the fail threshold." );
			AddResult( results, name, level, reason, valueMs + "ms", String.Format( "PASS <= {0}ms; FAIL > {1}ms", passMs, failMs ) );
		}

		private static void AddResult( List<TestResult> results, string name, TestResultLevel level, string reason, string measured, string threshold )
		{
			TestResult result = new TestResult();
			result.Name = name;
			result.Level = level;
			result.Reason = reason;
			result.MeasuredValue = measured;
			result.Threshold = threshold;
			results.Add( result );
		}

		private static TestResultLevel GetOverallResult( List<TestResult> results )
		{
			TestResultLevel overall = TestResultLevel.Pass;

			for ( int i = 0; i < results.Count; ++i )
			{
				if ( results[i].Level == TestResultLevel.Fail )
					return TestResultLevel.Fail;

				if ( results[i].Level == TestResultLevel.Warn )
					overall = TestResultLevel.Warn;
			}

			return overall;
		}

		private static void WriteSummaryFile( string result )
		{
			string folder;
			string fileName;
			string[] events;
			string[] reports;
			Snapshot snapshot;
			TaskManagerDailyAttributionDiagnostics.PerformanceSummary dailyAttribution;
			string dailyAttributionModule;
			bool hasDailyAttribution = TryGetCurrentTestDailyAttribution( out dailyAttribution, out dailyAttributionModule );

			lock ( m_Lock )
			{
				folder = m_OutputFolder;
				fileName = String.Format( "PerformanceTest-v321-{0}-summary",
					m_StartedLocal != DateTime.MinValue ? m_StartedLocal.ToString( "yyyyMMdd-HHmmss" ) : DateTime.Now.ToString( "yyyyMMdd-HHmmss" ) );
				events = m_Events.ToArray();
				reports = m_ReportFiles.ToArray();
			}

			if ( String.IsNullOrWhiteSpace( folder ) || folder == "none" )
				folder = Path.GetFullPath( OutputRoot );

			Directory.CreateDirectory( folder );
			string path = GetUniqueTextPath( folder, fileName );
			snapshot = GetSnapshot();

			using ( StreamWriter writer = new StreamWriter( path, false ) )
			{
				writer.WriteLine( "Performance Test Suite v3.25 Summary" );
				writer.WriteLine( "Result: {0}", result );
				writer.WriteLine( "Evaluation: {0}", snapshot.OverallResult );
				writer.WriteLine( "Test: {0}", snapshot.ActiveTestName );
				writer.WriteLine(
					"Current-test Daily attribution: {0}",
					hasDailyAttribution
						? String.Format( "runId={0} startedUtc={1} timer={2} module={3}", dailyAttribution.RunId, dailyAttribution.StartedUtc, dailyAttribution.TimerName, dailyAttributionModule )
						: "Not sampled" );
				writer.WriteLine( "Admin: {0} account={1}", snapshot.AdminName, snapshot.AccountName );
				writer.WriteLine( "Start UTC: {0}", snapshot.StartedUtc );
				writer.WriteLine( "Start local: {0}", snapshot.StartedLocal );
				writer.WriteLine( "End UTC: {0}", DateTime.UtcNow );
				writer.WriteLine( "End local: {0}", DateTime.Now );
				writer.WriteLine( "Long-run: active={0} duration={1}m interval={2}m captures={3} errors={4} startupMarker={5} taskWindows={6} autoSaveWindows={7} endSummary={8}",
					snapshot.LongRunActive,
					snapshot.LongRunDurationMinutes,
					snapshot.LongRunIntervalMinutes,
					snapshot.LongRunCaptures,
					snapshot.LongRunErrors,
					snapshot.LongRunCaptureStartupMarker,
					snapshot.LongRunCaptureTaskWindows,
					snapshot.LongRunCaptureAutoSaveWindows,
					snapshot.LongRunEndSummary );
				writer.WriteLine( "Warnings: manual tests mutate world state; daily and 250 shift normal last-run state; cancellation cannot interrupt a synchronous task already running." );
				writer.WriteLine();
				writer.WriteLine( "PASS / WARN / FAIL evaluation:" );

				for ( int i = 0; i < snapshot.Results.Length; ++i )
					writer.WriteLine( "  {0}", snapshot.Results[i] );

				writer.WriteLine();
				writer.WriteLine( "Generated files:" );

				for ( int i = 0; i < reports.Length; ++i )
					writer.WriteLine( "  {0}", reports[i] );

				writer.WriteLine();
				writer.WriteLine( "Timeline:" );

				for ( int i = 0; i < events.Length; ++i )
					writer.WriteLine( "  {0}", events[i] );

				writer.WriteLine();
				TaskManagerDiagnostics.WriteStats( writer );
				writer.WriteLine();
				writer.WriteLine( "TaskManagerDaily Attribution" );
				writer.WriteLine( "--------------------------------------------------------------------------------" );
				TaskManagerDailyAttributionDiagnostics.WriteStats( writer );
				writer.WriteLine();
				BalancePortalDiagnostics.WriteStats( writer );
				writer.WriteLine();
				SpawnEntryDiagnostics.WriteStats( writer );
				writer.WriteLine();
				SpawnEntryDiagnostics.WriteBurstStats( writer );
				writer.WriteLine();
				PremiumSpawner.WriteSchedulerStats( writer );
				writer.WriteLine();
				Statistics.WriteStats( writer );
				writer.WriteLine();
				StatusPage.WriteStats( writer );
				writer.WriteLine();
				DiscordBridge.WriteStats( writer );
				writer.WriteLine();
				RuneBagDiagnostics.WriteStats( writer );
				writer.WriteLine();
				NetFlightRecorder.WriteDiagnostics( writer );
				writer.WriteLine();
				SaveDiagnostics.WriteStats( writer );
				writer.WriteLine();
				ConsoleTranscript.WriteStats( writer );
			}

			path = Path.GetFullPath( path );

			lock ( m_Lock )
			{
				AddReportPathLocked( path );
				m_LastReportPath = path;
				m_LastSummaryPath = path;
				m_LastUpdateUtc = DateTime.UtcNow;
			}

			AddEvent( "Wrote summary: " + path );
		}

		private static string GetUniqueTextPath( string folder, string fileNameBase )
		{
			string safeBase = fileNameBase;

			foreach ( char c in Path.GetInvalidFileNameChars() )
				safeBase = safeBase.Replace( c, '_' );

			string path = Path.Combine( folder, safeBase + ".txt" );

			if ( !File.Exists( path ) )
				return path;

			for ( int i = 1; i < 1000; ++i )
			{
				path = Path.Combine( folder, String.Format( "{0}-{1}.txt", safeBase, i ) );

				if ( !File.Exists( path ) )
					return path;
			}

			return Path.Combine( folder, String.Format( "{0}-{1}.txt", safeBase, Guid.NewGuid().ToString( "N" ) ) );
		}

		private static bool CanStart( Mobile from, bool automated, out string reason )
		{
			reason = null;

			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
			{
				reason = "TaskManagerTest refused: Administrator access required.";
				return false;
			}

			if ( World.Saving )
			{
				reason = "TaskManagerTest refused: the world is currently saving. Nothing was queued.";
				return false;
			}

			if ( Server.Items.TaskManagerDaily.RunInProgress )
			{
				reason = String.Format( "Performance Test refused: TaskManagerDaily staged population is already active ({0}). Open [TestSuite for status.", Server.Items.TaskManagerDaily.ActiveRunDescription );
				return false;
			}

			lock ( m_Lock )
			{
				if ( m_LongRunActive )
				{
					reason = String.Format( "Performance Test refused: long-run diagnostics is active; next capture UTC {0}. Open [TestSuite for status.", m_LongRunNextCaptureUtc );
					return false;
				}

				if ( m_InFlight )
				{
					reason = String.Format( "Performance Test refused: {0} is already in flight by {1}. Open [TestSuite for status.", m_InFlightCommand, m_InFlightAdmin );
					return false;
				}

				if ( m_State == TestState.Running || m_State == TestState.CapturingLagReport || m_State == TestState.WaitingForAdmin )
				{
					reason = String.Format( "Performance Test refused: automated test '{0}' is active. Open [TestSuite for status.", m_ActiveTestName );
					return false;
				}

				if ( m_Settling && GetSettlingSecondsRemainingLocked() > 0 )
				{
					reason = String.Format( "Performance Test refused: settling guard active for {0}, {1}s remaining ({2}). Open [TestSuite for status.", m_SettlingFamily, GetSettlingSecondsRemainingLocked(), m_SettlingReason );
					return false;
				}
			}

			return true;
		}

		private static bool BeginManual( string family, string command, string admin, string account )
		{
			lock ( m_Lock )
			{
				if ( m_InFlight )
					return false;

				m_InFlight = true;
				m_InFlightFamily = family;
				m_InFlightCommand = command;
				m_InFlightAdmin = admin;
				m_InFlightAccount = account;
				m_InFlightUtc = DateTime.UtcNow;
				m_InFlightLocal = DateTime.Now;
				m_LastUpdateUtc = DateTime.UtcNow;
				return true;
			}
		}

		private static void EndManual()
		{
			lock ( m_Lock )
			{
				m_InFlight = false;
				m_InFlightFamily = "none";
				m_InFlightCommand = "none";
				m_InFlightAdmin = "none";
				m_InFlightAccount = "none";
				m_InFlightUtc = DateTime.MinValue;
				m_InFlightLocal = DateTime.MinValue;
				m_LastUpdateUtc = DateTime.UtcNow;
			}
		}

		private static void RecordManualResult( string family, string command, string admin, string account, bool forced, DateTime startedUtc, DateTime startedLocal, long elapsed, string result, string error, string oldSchedule, string newSchedule )
		{
			ManualSample sample = new ManualSample();
			sample.Family = family;
			sample.Command = command;
			sample.Admin = admin;
			sample.Account = account;
			sample.Forced = forced;
			sample.StartedUtc = startedUtc;
			sample.StartedLocal = startedLocal;
			sample.ElapsedMs = elapsed;
			sample.Result = result;
			sample.Error = error;
			sample.OldSchedule = oldSchedule;
			sample.NewSchedule = newSchedule;

			lock ( m_Lock )
			{
				m_LastManual = sample.Clone();
				m_LastManualByFamily[family] = sample.Clone();
				m_LastUpdateUtc = DateTime.UtcNow;
			}
		}

		private static void StartSettlingGuard( string family, int seconds, string reason )
		{
			if ( seconds <= 0 )
				return;

			lock ( m_Lock )
			{
				m_Settling = true;
				m_SettlingFamily = family;
				m_SettlingReason = reason;
				m_SettlingUntilUtc = DateTime.UtcNow + TimeSpan.FromSeconds( seconds );
				m_SettlingUntilLocal = DateTime.Now + TimeSpan.FromSeconds( seconds );

				if ( m_State == TestState.Running || m_State == TestState.CapturingLagReport )
					m_State = TestState.Settling;

				m_LastUpdateUtc = DateTime.UtcNow;
			}

			AddEvent( String.Format( "Settling {0}s for {1}: {2}", seconds, family, reason ) );
			EnsureRefreshTimer();
		}

		private static void ClearExpiredSettlingLocked()
		{
			if ( m_Settling && DateTime.UtcNow >= m_SettlingUntilUtc )
			{
				m_Settling = false;
				m_SettlingFamily = "none";
				m_SettlingReason = "none";
				m_SettlingUntilUtc = DateTime.MinValue;
				m_SettlingUntilLocal = DateTime.MinValue;
			}
		}

		private static int GetSettlingSeconds( string family )
		{
			lock ( m_Lock )
			{
				if ( family == "daily" )
					return m_DailySettlingSeconds;

				if ( family == "150" )
					return m_150SettlingSeconds;

				if ( family == "250" )
					return m_250SettlingSeconds;

				if ( family == "hourly" )
					return m_HourlySettlingSeconds;

				if ( family == "200" )
					return m_200SettlingSeconds;
			}

			return 0;
		}

		private static int GetSettlingSecondsRemainingLocked()
		{
			ClearExpiredSettlingLocked();

			if ( !m_Settling )
				return 0;

			TimeSpan remaining = m_SettlingUntilUtc - DateTime.UtcNow;

			if ( remaining <= TimeSpan.Zero )
				return 0;

			return (int)Math.Ceiling( remaining.TotalSeconds );
		}

		private static void CompleteStep( string stepName )
		{
			lock ( m_Lock )
			{
				m_LastCompletedStep = stepName;
				++m_StepIndex;
				m_LastUpdateUtc = DateTime.UtcNow;

				if ( m_Settling && DateTime.UtcNow >= m_SettlingUntilUtc )
					ClearExpiredSettlingLocked();
			}

			AddEvent( "Completed step: " + stepName );
			RefreshGump();
		}

		private static void CancelAfterStep()
		{
			lock ( m_Lock )
			{
				FinishCancelledLocked( "Cancelled after current step." );
			}

			try
			{
				EvaluateCurrentTest();
				WriteSummaryFile( "cancelled" );
			}
			catch ( Exception ex )
			{
				lock ( m_Lock )
					m_LastError = "Cancelled-summary error: " + ex.Message;
			}

			RefreshGump();
		}

		private static bool IsCancellationRequested()
		{
			lock ( m_Lock )
				return m_CancelRequested;
		}

		private static void FinishCancelledLocked( string reason )
		{
			m_State = TestState.Cancelled;
			m_ActivePhase = reason;
			m_NextStep = "none";
			m_LastError = reason;
			m_CancelRequested = false;
			m_LastUpdateUtc = DateTime.UtcNow;
			AddEventLocked( reason );
		}

		private static void FinishCompleteLocked()
		{
			ClearExpiredSettlingLocked();
			m_State = TestState.Complete;
			m_ActivePhase = "Complete";
			m_NextStep = "none";
			m_CurrentFamily = "none";
			m_LastUpdateUtc = DateTime.UtcNow;
			AddEventLocked( "Test complete." );
		}

		private static void FailTest( Exception ex )
		{
			lock ( m_Lock )
			{
				m_State = TestState.Failed;
				m_ActivePhase = "Failed";
				m_NextStep = "none";
				m_LastError = ex != null ? String.Format( "{0}: {1}", ex.GetType().FullName, ex.Message ) : "unknown";
				m_LastUpdateUtc = DateTime.UtcNow;
				AddEventLocked( "Failed: " + m_LastError );
			}

			try
			{
				EvaluateCurrentTest();
				WriteSummaryFile( "failed" );
			}
			catch ( Exception summaryError )
			{
				lock ( m_Lock )
					m_LastError = m_LastError + " | failed-summary error: " + summaryError.Message;
			}

			RefreshGump();
		}

		private static void ScheduleNextStep( TimeSpan delay )
		{
			lock ( m_Lock )
			{
				if ( m_TestTimer != null )
					m_TestTimer.Stop();

				m_TestTimer = new TestTimer( delay, GetNextTimerLabelLocked() );
				m_TestTimer.Start();
			}
		}

		private static string GetNextTimerLabelLocked()
		{
			if ( m_StepIndex < 0 || m_StepIndex >= m_Plan.Count )
				return "Complete";

			TestStep step = m_Plan[m_StepIndex];

			if ( IsTaskStep( step.Type ) )
				return GetFriendlyFamilyName( step.Family ) + " ManualCommand";

			return step.Name;
		}

		private static string GetFriendlyFamilyName( string family )
		{
			if ( family == "daily" )
				return "Daily";
			if ( family == "150" )
				return "150";
			if ( family == "200" )
				return "200";
			if ( family == "250" )
				return "250";
			if ( family == "hourly" )
				return "Hourly";
			return "Performance Test";
		}

		private sealed class TestTimer : Timer
		{
			private readonly string m_FriendlyLabel;

			public TestTimer( TimeSpan delay, string friendlyLabel ) : base( delay )
			{
				m_FriendlyLabel = friendlyLabel;
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				ProcessNextStep();
			}

			public override string ToString()
			{
				return GetType().FullName + " / " + ( !String.IsNullOrWhiteSpace( m_FriendlyLabel ) ? m_FriendlyLabel : "Performance Test Step" );
			}
		}

		private sealed class RefreshTimer : Timer
		{
			public RefreshTimer() : base( TimeSpan.FromSeconds( 1.0 ) )
			{
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				bool keepGoing = false;

				lock ( m_Lock )
				{
					ClearExpiredSettlingLocked();
					keepGoing = ( m_State == TestState.Running || m_State == TestState.Settling || m_State == TestState.CapturingLagReport || m_InFlight || m_Settling || m_LongRunActive );
					m_RefreshTimer = null;
				}

				RefreshGumps();

				if ( keepGoing )
					EnsureRefreshTimer();
			}
		}

		private static void EnsureRefreshTimer()
		{
			lock ( m_Lock )
			{
				if ( m_RefreshTimer != null )
					return;

				m_RefreshTimer = new RefreshTimer();
				m_RefreshTimer.Start();
			}
		}

		private static void RefreshGump()
		{
			RefreshGumps();
		}

		private static void RefreshGumps()
		{
			Mobile admin = GetAdmin();

			if ( admin != null && admin.AccessLevel >= AccessLevel.Administrator )
			{
				if ( admin.HasGump( typeof( TaskManagerTestGump ) ) )
				{
					admin.CloseGump( typeof( TaskManagerTestGump ) );
					admin.SendGump( new TaskManagerTestGump( m_GumpPage ) );
				}

				RefreshProgressGump( admin );
			}
		}

		private static void RefreshProgressGump( Mobile admin )
		{
			if ( admin == null || admin.AccessLevel < AccessLevel.Administrator || !admin.HasGump( typeof( PerformanceTestProgressGump ) ) )
				return;

			admin.CloseGump( typeof( PerformanceTestProgressGump ) );
			admin.SendGump( new PerformanceTestProgressGump() );
		}

		private static Mobile GetAdmin()
		{
			lock ( m_Lock )
				return m_Admin;
		}

		public static Snapshot GetSnapshot()
		{
			lock ( m_Lock )
			{
				ClearExpiredSettlingLocked();

				Snapshot snapshot = new Snapshot();
				snapshot.State = m_State.ToString();
				snapshot.ActiveTestName = m_ActiveTestName;
				snapshot.ActiveModule = m_ActiveModule.ToString();
				snapshot.ActivePhase = m_ActivePhase;
				snapshot.NextStep = m_NextStep;
				int currentStepIndex = m_StepIndex;

				if ( m_State == TestState.Settling && currentStepIndex > 0 )
					--currentStepIndex;

				if ( m_Plan.Count > 0 )
					currentStepIndex = Math.Max( 0, Math.Min( currentStepIndex, m_Plan.Count - 1 ) );
				else
					currentStepIndex = -1;

				snapshot.CurrentStepIndex = currentStepIndex;
				snapshot.StepNumber = currentStepIndex >= 0 ? currentStepIndex + 1 : 0;
				snapshot.StepCount = m_Plan.Count;
				snapshot.PlanSteps = new string[m_Plan.Count];

				for ( int i = 0; i < m_Plan.Count; ++i )
					snapshot.PlanSteps[i] = m_Plan[i] != null ? m_Plan[i].Name : "Unnamed step";

				snapshot.CurrentFamily = m_CurrentFamily;
				snapshot.InFlight = m_InFlight;
				snapshot.InFlightCommand = m_InFlightCommand;
				snapshot.InFlightAdmin = m_InFlightAdmin;
				snapshot.Settling = m_Settling;
				snapshot.SettlingFamily = m_SettlingFamily;
				snapshot.SettlingReason = m_SettlingReason;
				snapshot.SettlingUntilUtc = m_SettlingUntilUtc;
				snapshot.SettlingUntilLocal = m_SettlingUntilLocal;
				snapshot.SettlingSecondsRemaining = GetSettlingSecondsRemainingLocked();
				snapshot.AdminName = m_AdminName;
				snapshot.AccountName = m_AccountName;
				snapshot.StartedUtc = m_StartedUtc;
				snapshot.StartedLocal = m_StartedLocal;
				snapshot.ElapsedSeconds = m_StartedUtc != DateTime.MinValue ? (long)( DateTime.UtcNow - m_StartedUtc ).TotalSeconds : 0;
				snapshot.CancelRequested = m_CancelRequested;
				snapshot.LastCompletedStep = m_LastCompletedStep;
				snapshot.LastReportPath = m_LastReportPath;
				snapshot.LastSummaryPath = m_LastSummaryPath;
				snapshot.OutputFolder = m_OutputFolder;
				snapshot.LastError = m_LastError;
				snapshot.OverallResult = m_OverallResult;
				snapshot.LastManualCommand = m_LastManual != null ? m_LastManual.Command : "none";
				snapshot.LastManualResult = m_LastManual != null ? m_LastManual.Result : "none";
				snapshot.LastManualElapsedMs = m_LastManual != null ? m_LastManual.ElapsedMs : 0;
				snapshot.IncludeHourly = m_IncludeHourly;
				snapshot.Include200 = m_Include200;
				snapshot.IncludeBalancePortalValidate = m_IncludeBalancePortalValidate;
				snapshot.AutoGenerateLagReportEveryStep = m_AutoGenerateLagReportEveryStep;
				snapshot.AutoGenerateFinalSummary = m_AutoGenerateFinalSummary;
				snapshot.DailySettlingSeconds = m_DailySettlingSeconds;
				snapshot.Settling150Seconds = m_150SettlingSeconds;
				snapshot.Settling250Seconds = m_250SettlingSeconds;
				snapshot.HourlySettlingSeconds = m_HourlySettlingSeconds;
				snapshot.Settling200Seconds = m_200SettlingSeconds;
				snapshot.LongRunDurationMinutes = m_LongRunDurationMinutes;
				snapshot.LongRunIntervalMinutes = m_LongRunIntervalMinutes;
				snapshot.LongRunCaptureStartupMarker = m_LongRunCaptureStartupMarker;
				snapshot.LongRunCaptureTaskWindows = m_LongRunCaptureTaskWindows;
				snapshot.LongRunCaptureAutoSaveWindows = m_LongRunCaptureAutoSaveWindows;
				snapshot.LongRunEndSummary = m_LongRunEndSummary;
				snapshot.LongRunActive = m_LongRunActive;
				snapshot.LongRunStartedUtc = m_LongRunStartedUtc;
				snapshot.LongRunStartedLocal = m_LongRunStartedLocal;
				snapshot.LongRunEndUtc = m_LongRunEndUtc;
				snapshot.LongRunEndLocal = m_LongRunEndLocal;
				snapshot.LongRunNextCaptureUtc = m_LongRunNextCaptureUtc;
				snapshot.LongRunRemainingSeconds = m_LongRunActive && m_LongRunEndUtc > DateTime.UtcNow ? (long)( m_LongRunEndUtc - DateTime.UtcNow ).TotalSeconds : 0;
				snapshot.LongRunCaptures = m_LongRunCaptures;
				snapshot.LongRunErrors = m_LongRunErrors;
				snapshot.LongRunLastError = m_LongRunLastError;
				snapshot.GumpPage = m_GumpPage;
				snapshot.RecentEvents = m_Events.ToArray();
				snapshot.ReportFiles = CopyRecentReportsLocked();
				snapshot.RecentModuleResults = m_RecentModuleResults.ToArray();
				snapshot.Results = new TestResult[m_LastResults.Count];

				for ( int i = 0; i < m_LastResults.Count; ++i )
					snapshot.Results[i] = m_LastResults[i].Clone();

				return snapshot;
			}
		}

		private static string[] CopyRecentReportsLocked()
		{
			int count = Math.Min( MaxDisplayedReports, m_ReportFiles.Count );
			string[] reports = new string[count];
			int start = m_ReportFiles.Count - count;

			for ( int i = 0; i < count; ++i )
				reports[i] = m_ReportFiles[start + i];

			return reports;
		}

		public static void WriteStatus( TextWriter writer )
		{
			if ( writer == null )
				return;

			Snapshot snapshot = GetSnapshot();

			writer.WriteLine( "PerformanceTest / TaskManagerTest: controllerState={0} activeModule={1} activeTest={2} activePhase={3} step={4}/{5} inFlight={6} inFlightCommand={7} settling={8} settlingFamily={9} settlingRemaining={10}s settlingUntilUtc={11} settlingUntilLocal={12} currentAdmin={13} account={14} startedUtc={15} startedLocal={16} elapsed={17}s cancelRequested={18}",
				snapshot.State,
				snapshot.ActiveModule,
				snapshot.ActiveTestName,
				snapshot.ActivePhase,
				snapshot.StepNumber,
				snapshot.StepCount,
				snapshot.InFlight,
				snapshot.InFlightCommand,
				snapshot.Settling,
				snapshot.SettlingFamily,
				snapshot.SettlingSecondsRemaining,
				snapshot.SettlingUntilUtc,
				snapshot.SettlingUntilLocal,
				snapshot.AdminName,
				snapshot.AccountName,
				snapshot.StartedUtc,
				snapshot.StartedLocal,
				snapshot.ElapsedSeconds,
				snapshot.CancelRequested );
			writer.WriteLine( "PerformanceTest output: folder={0} lastReport={1} latestSummary={2} lastCompletedStep={3} overall={4} lastError={5}", snapshot.OutputFolder, snapshot.LastReportPath, snapshot.LastSummaryPath, snapshot.LastCompletedStep, snapshot.OverallResult, snapshot.LastError );
			writer.WriteLine( "PerformanceTest progress: nextStep={0}", snapshot.NextStep );
			writer.WriteLine(
				"PerformanceTest dailyStaging: inProgress={0} activeRun={1} overlapRefused={2} lastOverlapRefusedUtc={3}",
				Server.Items.TaskManagerDaily.RunInProgress,
				Server.Items.TaskManagerDaily.ActiveRunDescription,
				Server.Items.TaskManagerDaily.OverlapRefusedCount,
				Server.Items.TaskManagerDaily.LastOverlapRefusedUtc );
			writer.WriteLine( "PerformanceTest longRun: active={0} startedUtc={1} targetEndUtc={2} remaining={3}s interval={4}m nextCaptureUtc={5} captures={6} errors={7} lastError={8}", snapshot.LongRunActive, snapshot.LongRunStartedUtc, snapshot.LongRunEndUtc, snapshot.LongRunRemainingSeconds, snapshot.LongRunIntervalMinutes, snapshot.LongRunNextCaptureUtc, snapshot.LongRunCaptures, snapshot.LongRunErrors, snapshot.LongRunLastError );
			writer.WriteLine( "PerformanceTest lastManual: command={0} result={1} elapsed={2}ms", snapshot.LastManualCommand, snapshot.LastManualResult, snapshot.LastManualElapsedMs );
			writer.WriteLine( "PerformanceTest options: includeHourly={0} include200={1} includeBalancePortalValidate={2} autoLagReport={3} autoSummary={4}", snapshot.IncludeHourly, snapshot.Include200, snapshot.IncludeBalancePortalValidate, snapshot.AutoGenerateLagReportEveryStep, snapshot.AutoGenerateFinalSummary );

			NetFlightDiagnosticsSnapshot network = NetFlightRecorder.GetDiagnosticsSnapshot();
			writer.WriteLine( "PerformanceTest network: enabled={0} connected={1} healthy={2} warning={3} critical={4} dumps={5} disconnectDumps={6} flushDropped={7} snapshotDropped={8} lastError={9}",
				network.Enabled,
				network.ConnectedSessions,
				network.HealthySessions,
				network.WarningSessions,
				network.CriticalSessions,
				network.TotalDumps,
				network.DisconnectDumps,
				network.FlushDropped,
				network.SnapshotDropped,
				network.LastWriterError );

			RuneBagDiagnostics.Snapshot runeBag = RuneBagDiagnostics.GetSnapshot();
			writer.WriteLine( "PerformanceTest runeBag: cooldown={0}ms attempts={1} allowed={2} throttled={3} max={4}ms slowCalls={5} throttleTableSize={6} diagnosticErrors={7}",
				RuneBagDiagnostics.CooldownMs,
				runeBag.Attempts,
				runeBag.Allowed,
				runeBag.Throttled,
				runeBag.MaxMs,
				runeBag.SlowCalls,
				runeBag.ThrottleTableSize,
				runeBag.DiagnosticErrors );

			WriteFamilyStatus( writer, "hourly", "TaskManager" );
			WriteFamilyStatus( writer, "150", "TaskManager150Min" );
			WriteFamilyStatus( writer, "200", "TaskManager200Min" );
			WriteFamilyStatus( writer, "250", "TaskManager250Min" );
			WriteFamilyStatus( writer, "daily", "TaskManagerDaily" );

			writer.WriteLine( "PerformanceTest evaluation: count={0} overall={1}", snapshot.Results.Length, snapshot.OverallResult );

			for ( int i = 0; i < snapshot.Results.Length; ++i )
				writer.WriteLine( "  result#{0}: {1}", i + 1, snapshot.Results[i] );

			writer.WriteLine( "PerformanceTest recent module results: count={0}", snapshot.RecentModuleResults.Length );

			for ( int i = Math.Max( 0, snapshot.RecentModuleResults.Length - 8 ); i < snapshot.RecentModuleResults.Length; ++i )
				writer.WriteLine( "  module#{0}: {1}", i + 1, snapshot.RecentModuleResults[i] );

			writer.WriteLine( "PerformanceTest reports: count={0}", snapshot.ReportFiles.Length );

			for ( int i = Math.Max( 0, snapshot.ReportFiles.Length - 8 ); i < snapshot.ReportFiles.Length; ++i )
				writer.WriteLine( "  report#{0}: {1}", i + 1, snapshot.ReportFiles[i] );

			writer.WriteLine( "PerformanceTest recent events: count={0}", snapshot.RecentEvents.Length );

			for ( int i = Math.Max( 0, snapshot.RecentEvents.Length - 12 ); i < snapshot.RecentEvents.Length; ++i )
				writer.WriteLine( "  event#{0}: {1}", i + 1, snapshot.RecentEvents[i] );
		}

		private static void WriteFamilyStatus( TextWriter writer, string family, string taskName )
		{
			long runId;
			long duration;
			string timerName;
			DateTime startedUtc;
			string topPhase;
			bool hasDiag = TaskManagerDiagnostics.TryGetLatestRunInfo( taskName, out runId, out duration, out timerName, out startedUtc, out topPhase );
			ManualSample manual = GetLastManual( family );

			writer.WriteLine( "TaskManagerTest {0}: schedule={1} timerActive=Unavailable latestRunId={2} latestDurationMs={3} latestTimer={4} latestTopPhase={5} latestStartedUtc={6} lastManualDurationMs={7} lastManualResult={8} lastError={9}",
				family,
				FormatSchedule( GetSchedule( family ) ),
				hasDiag ? runId.ToString() : "Unavailable",
				hasDiag ? duration.ToString() : "Unavailable",
				hasDiag ? timerName : "Unavailable",
				hasDiag ? topPhase : "Unavailable",
				hasDiag ? startedUtc.ToString() : "Unavailable",
				manual != null ? manual.ElapsedMs.ToString() : "Unavailable",
				manual != null ? manual.Result : "Unavailable",
				manual != null ? manual.Error : "Unavailable" );
		}

		private static ManualSample GetLastManual( string family )
		{
			lock ( m_Lock )
			{
				ManualSample sample;

				if ( m_LastManualByFamily.TryGetValue( family, out sample ) && sample != null )
					return sample.Clone();
			}

			return null;
		}

		private static string GetManualCommandName( string family )
		{
			if ( family == "hourly" )
				return "TaskManager+ManualCommand";

			if ( family == "150" )
				return "TaskManager150Min+ManualCommand";

			if ( family == "200" )
				return "TaskManager200Min+ManualCommand";

			if ( family == "250" )
				return "TaskManager250Min+ManualCommand";

			return "TaskManagerDaily+ManualCommand";
		}

		private static bool IsForcedFamily( string family )
		{
			return family == "daily" || family == "250";
		}

		private static ScheduleState GetSchedule( string family )
		{
			ScheduleState state = new ScheduleState();

			if ( family == "hourly" )
			{
				state.Available = true;
				state.LastRunUtc = Server.Items.TaskManager.LastRun;
				state.NextRunUtc = Server.Items.TaskManager.NextRunTime;
			}
			else if ( family == "250" )
			{
				state.Available = true;
				state.LastRunUtc = Server.Items.TaskManager250Min.LastRun;
				state.NextRunUtc = Server.Items.TaskManager250Min.NextRunTime;
			}
			else if ( family == "daily" )
			{
				state.Available = true;
				state.LastRunUtc = Server.Items.TaskManagerDaily.LastRun;
				state.NextRunUtc = Server.Items.TaskManagerDaily.NextRunTime;
			}

			return state;
		}

		private static string FormatSchedule( ScheduleState state )
		{
			if ( !state.Available )
				return "LastRun=Unavailable NextRun=Unavailable dueNow=Unavailable timeUntilDue=Unavailable";

			DateTime now = DateTime.UtcNow;
			TimeSpan until = state.NextRunUtc - now;
			bool due = until <= TimeSpan.Zero;

			if ( due )
				until = TimeSpan.Zero;

			return String.Format( "LastRunUtc={0} LastRunLocal={1} NextRunUtc={2} NextRunLocal={3} dueNow={4} timeUntilDue={5}",
				state.LastRunUtc,
				ToLocal( state.LastRunUtc ),
				state.NextRunUtc,
				ToLocal( state.NextRunUtc ),
				due,
				until );
		}

		private static DateTime ToLocal( DateTime utc )
		{
			if ( utc == DateTime.MinValue )
				return utc;

			return DateTime.SpecifyKind( utc, DateTimeKind.Utc ).ToLocalTime();
		}

		private static string GetMobileName( Mobile from )
		{
			if ( from == null )
				return "null";

			if ( !String.IsNullOrWhiteSpace( from.RawName ) )
				return from.RawName;

			if ( !String.IsNullOrWhiteSpace( from.Name ) )
				return from.Name;

			return from.Serial.ToString();
		}

		private static string GetAccountName( Mobile from )
		{
			if ( from != null && from.Account != null && !String.IsNullOrWhiteSpace( from.Account.Username ) )
				return from.Account.Username;

			return "none";
		}

		private static void AddReportPathLocked( string path )
		{
			m_ReportFiles.Add( path );

			while ( m_ReportFiles.Count > MaxReports )
				m_ReportFiles.RemoveAt( 0 );
		}

		private static void AddEvent( string message )
		{
			lock ( m_Lock )
				AddEventLocked( message );
		}

		private static void AddEventLocked( string message )
		{
			m_Events.Add( String.Format( "{0:u} {1}", DateTime.UtcNow, message ) );

			while ( m_Events.Count > MaxEvents )
				m_Events.RemoveAt( 0 );
		}

		private static void SendLine( Mobile from, string line )
		{
			if ( from != null )
				from.SendMessage( line );
			else
				DiagnosticConsole.WriteParsedLine( line );
		}

		private static void SendLines( Mobile from, StringWriter writer )
		{
			if ( writer == null )
				return;

			using ( StringReader reader = new StringReader( writer.ToString() ) )
			{
				string line;

				while ( ( line = reader.ReadLine() ) != null )
					SendLine( from, line );
			}
		}
	}
}
