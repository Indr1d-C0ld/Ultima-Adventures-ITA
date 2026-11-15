using System;
using System.Collections.Generic;

namespace Server.Commands
{
	public enum TestModule
	{
		OfficialV316,
		OfficialV317DailyAttribution,
		DailyOnly,
		SpawnEntry150Only,
		TaskManager250Only,
		TaskManagerHourlyOnly,
		TaskManager200Only,
		LagReportOnly,
		BalancePortalValidateOnly,
		DyePreview,
		LongRunDiagnostics
	}

	public enum TestStepType
	{
		Preflight,
		CaptureLagReport,
		CaptureStats,
		RunTaskManagerHourly,
		RunTaskManager150,
		RunTaskManager200,
		RunTaskManager250,
		RunTaskManagerDaily,
		WaitSettling,
		RunBalancePortalValidate,
		RunManualSavePromptOnly,
		LongRunSample,
		Summary
	}

	public enum TestResultLevel
	{
		Info,
		Pass,
		Warn,
		Fail
	}

	public sealed class TestResult
	{
		public string Name;
		public TestResultLevel Level;
		public string Reason;
		public string MeasuredValue;
		public string Threshold;

		public TestResult Clone()
		{
			TestResult copy = new TestResult();
			copy.Name = Name;
			copy.Level = Level;
			copy.Reason = Reason;
			copy.MeasuredValue = MeasuredValue;
			copy.Threshold = Threshold;
			return copy;
		}

		public override string ToString()
		{
			return String.Format(
				"{0}: {1} measured={2} threshold={3} reason={4}",
				Level,
				Name,
				!String.IsNullOrWhiteSpace( MeasuredValue ) ? MeasuredValue : "Unavailable",
				!String.IsNullOrWhiteSpace( Threshold ) ? Threshold : "Informational",
				!String.IsNullOrWhiteSpace( Reason ) ? Reason : "none" );
		}
	}

	public sealed class TestStep
	{
		public string Name;
		public string Description;
		public TestStepType Type;
		public string Family;
		public string ReportLabel;
		public int EstimatedSeconds;
		public int SettlingSeconds;
		public bool MutatesWorldState;
		public bool RequiresConfirmation;
		public bool CaptureLagReportBefore;
		public bool CaptureLagReportAfter;

		public TestStep( TestStepType type, string name )
		{
			Type = type;
			Name = name;
			Description = name;
			Family = "none";
			ReportLabel = "checkpoint";
		}
	}

	public sealed class TestPlan
	{
		public string Name;
		public TestModule Module;
		public bool MutatesWorldState;
		public bool RequiresConfirmation;
		public readonly List<TestStep> Steps = new List<TestStep>();

		public TestPlan( string name, TestModule module )
		{
			Name = name;
			Module = module;
		}

		public void Add( TestStep step )
		{
			if ( step == null )
				return;

			Steps.Add( step );

			if ( step.MutatesWorldState )
				MutatesWorldState = true;

			if ( step.RequiresConfirmation )
				RequiresConfirmation = true;
		}
	}
}
