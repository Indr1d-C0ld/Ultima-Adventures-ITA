using System;
using System.Text;
using Server.Gumps;
using Server.Network;

namespace Server.Commands
{
	public class PerformanceTestProgressGump : Gump
	{
		private const string ColorGood = "63EF00";
		private const string ColorBad = "DE103A";

		private const int ButtonRefresh = 1;
		private const int ButtonDashboard = 2;
		private const int ButtonCancel = 3;
		private const int ButtonClose = 4;

		public PerformanceTestProgressGump() : base( 580, 24 )
		{
			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			TaskManagerTestController.Snapshot snapshot = TaskManagerTestController.GetSnapshot();

			AddPage( 0 );
			AddBackground( 0, 0, 500, 730, 9270 );
			AddAlphaRegion( 12, 12, 476, 706 );
			AddHtml( 24, 18, 452, 28, "<basefont color=#FFFFFF><center><big>Performance Test Progress</big></center></basefont>", false, false );
			AddHtml( 24, 52, 452, 34, BuildStateBanner( snapshot ), false, false );

			AddImageTiled( 22, 94, 456, 250, 2624 );
			AddAlphaRegion( 28, 100, 444, 238 );
			AddHtml( 40, 110, 420, 218, BuildSummary( snapshot ), false, false );

			AddImageTiled( 22, 358, 456, 310, 2624 );
			AddAlphaRegion( 28, 364, 444, 298 );
			AddLabel( 40, 374, 1152, "Plan Progress" );
			AddHtml( 40, 404, 420, 244, BuildStepChecklist( snapshot ), false, false );

			AddImageTiled( 22, 684, 456, 1, 2624 );
			AddButtonWithLabel( 28, 700, ButtonRefresh, "Refresh", 1152 );
			AddButtonWithLabel( 138, 700, ButtonDashboard, "Dashboard", 1152 );
			AddButtonWithLabel( 268, 700, ButtonCancel, "Cancel", 1152 );
			AddButtonWithLabel( 378, 700, ButtonClose, "Close", 1152 );
		}

		private static string BuildSummary( TaskManagerTestController.Snapshot snapshot )
		{
			long remaining = snapshot.Settling
				? snapshot.SettlingSecondsRemaining
				: ( snapshot.LongRunActive ? GetLongRunNextCaptureSeconds( snapshot ) : 0 );

			return String.Format(
				"<basefont color=#FFFFFF>Module: {0}<br>Phase: {1}<br>Step: {2}/{3} | Elapsed: {4}<br>Remaining: {5}<br>Last complete: {6}<br>Next: {7}<br>Overall: {8}<br>Last error: {9}</basefont>",
				ColorOptionalValue( snapshot.ActiveTestName ),
				ColorOptionalValue( ShortText( snapshot.ActivePhase, 58 ) ),
				snapshot.StepNumber,
				snapshot.StepCount,
				FormatDuration( snapshot.ElapsedSeconds ),
				ColorCountdown( remaining ),
				ColorOptionalValue( ShortText( snapshot.LastCompletedStep, 48 ) ),
				ColorOptionalValue( ShortText( snapshot.NextStep, 48 ) ),
				ColorValue( snapshot.OverallResult, GetResultColor( snapshot.OverallResult ) ),
				ColorValue( ShortText( snapshot.LastError, 50 ), IsClearError( snapshot.LastError ) ? ColorGood : ColorBad ) );
		}

		private static string BuildStepChecklist( TaskManagerTestController.Snapshot snapshot )
		{
			if ( snapshot.PlanSteps == null || snapshot.PlanSteps.Length == 0 )
			{
				if ( snapshot.LongRunActive )
					return String.Format( "<basefont color=#FFFFFF>{0} Long-run capture {1}<br>Next capture in {2}</basefont>",
						ColorValue( ">", "66CCFF" ),
						snapshot.LongRunCaptures + 1,
						ColorCountdown( GetLongRunNextCaptureSeconds( snapshot ) ) );

				return "<basefont color=#FFFFFF>No active modular plan.</basefont>";
			}

			int current = snapshot.CurrentStepIndex;
			int start = Math.Max( 0, current - 3 );
			int end = Math.Min( snapshot.PlanSteps.Length, current + 6 );
			StringBuilder text = new StringBuilder();

			if ( start > 0 )
				text.AppendFormat( "<basefont color=#FFFFFF>... {0} earlier step(s)</basefont><br>", start );

			for ( int i = start; i < end; ++i )
			{
				string marker = ".";
				string color = "888888";

				if ( snapshot.State == "Complete" || i < current )
				{
					marker = "DONE";
					color = ColorGood;
				}
				else if ( i == current )
				{
					if ( snapshot.State == "Failed" )
					{
						marker = "X";
						color = ColorBad;
					}
					else if ( snapshot.State == "Cancelled" )
					{
						marker = "STOP";
						color = "FFCC66";
					}
					else if ( snapshot.Settling )
					{
						marker = "WAIT";
						color = "FFCC66";
					}
					else
					{
						marker = "&gt;";
						color = "66CCFF";
					}
				}

				text.AppendFormat( "<basefont color=#FFFFFF>{0} {1}. {2}</basefont><br>",
					ColorValue( marker, color ),
					i + 1,
					Encode( ShortText( snapshot.PlanSteps[i], 50 ) ) );
			}

			if ( end < snapshot.PlanSteps.Length )
				text.AppendFormat( "<basefont color=#FFFFFF>... {0} more step(s)</basefont>", snapshot.PlanSteps.Length - end );

			return text.ToString();
		}

		private static string BuildStateBanner( TaskManagerTestController.Snapshot snapshot )
		{
			string state = snapshot.State == "CapturingLagReport"
				? "CAPTURING REPORT"
				: ( snapshot.LongRunActive ? "LONG-RUN ACTIVE" : ( snapshot.State ?? "Idle" ).ToUpperInvariant() );
			string color = GetStateColor( snapshot.State, snapshot.LongRunActive );
			string suffix = String.Empty;

			if ( snapshot.Settling )
			{
				state = "SETTLING";
				color = "FFCC66";
				suffix = " - " + ColorCountdownSeconds( snapshot.SettlingSecondsRemaining );
			}
			else if ( snapshot.LongRunActive )
				suffix = " - " + ColorCountdown( GetLongRunNextCaptureSeconds( snapshot ) );

			return "<basefont color=#FFFFFF><center><big>" + ColorValue( state, color ) + suffix + "</big></center></basefont>";
		}

		private static bool IsActive( TaskManagerTestController.Snapshot snapshot )
		{
			return snapshot != null && ( snapshot.State == "Running" || snapshot.State == "Settling" || snapshot.State == "CapturingLagReport" || snapshot.InFlight || snapshot.LongRunActive );
		}

		private static bool IsClearError( string error )
		{
			return String.IsNullOrEmpty( error ) || String.Equals( error, "none", StringComparison.OrdinalIgnoreCase );
		}

		private static long GetLongRunNextCaptureSeconds( TaskManagerTestController.Snapshot snapshot )
		{
			if ( snapshot == null || snapshot.LongRunNextCaptureUtc == DateTime.MinValue )
				return 0;

			TimeSpan remaining = snapshot.LongRunNextCaptureUtc - DateTime.UtcNow;
			return remaining > TimeSpan.Zero ? (long)Math.Ceiling( remaining.TotalSeconds ) : 0;
		}

		private static string GetStateColor( string state, bool longRunActive )
		{
			if ( longRunActive || state == "Running" || state == "CapturingLagReport" )
				return "66CCFF";
			if ( state == "Complete" )
				return ColorGood;
			if ( state == "Failed" )
				return ColorBad;
			if ( state == "Settling" || state == "Cancelled" || state == "WaitingForAdmin" )
				return "FFCC66";
			return "CCCCCC";
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

		private static string ColorValue( string value, string color )
		{
			return String.Format( "<basefont color=#{0}>{1}</basefont>", color, Encode( value ) );
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

		private static string ColorCountdown( long seconds )
		{
			return ColorValue( FormatDuration( seconds ), "FFCC66" );
		}

		private static string ColorCountdownSeconds( long seconds )
		{
			return ColorValue( Math.Max( 0, seconds ) + "s", "FFCC66" );
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

		private static string FormatDuration( long seconds )
		{
			if ( seconds < 0 )
				seconds = 0;

			TimeSpan duration = TimeSpan.FromSeconds( seconds );
			return String.Format( "{0:00}:{1:00}:{2:00}", (int)duration.TotalHours, duration.Minutes, duration.Seconds );
		}

		private void AddButtonWithLabel( int x, int y, int buttonId, string text, int hue )
		{
			AddButton( x, y, 4005, 4007, buttonId, GumpButtonType.Reply, 0 );
			AddLabel( x + 30, y + 2, hue, text );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender != null ? sender.Mobile : null;

			if ( from == null || from.AccessLevel < AccessLevel.Administrator )
				return;

			switch ( info.ButtonID )
			{
				case ButtonRefresh:
					TaskManagerTestController.OpenProgressGump( from );
					break;
				case ButtonDashboard:
					TaskManagerTestController.OpenGump( from, TaskManagerTestGump.PageDashboard );
					TaskManagerTestController.OpenProgressGump( from );
					break;
				case ButtonCancel:
					TaskManagerTestController.Cancel( from );
					break;
			}
		}
	}
}
