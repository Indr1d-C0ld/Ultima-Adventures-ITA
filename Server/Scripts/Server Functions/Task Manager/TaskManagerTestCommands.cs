using System;
using System.IO;
using Server.Diagnostics;

namespace Server.Commands
{
	public static class TaskManagerTestCommands
	{
		public static void Initialize()
		{
			CommandSystem.Register( "TestSuite", AccessLevel.Administrator, new CommandEventHandler( TestSuite_OnCommand ) );
			CommandSystem.Register( "TaskManagerTest", AccessLevel.Administrator, new CommandEventHandler( TaskManagerTest_OnCommand ) );
			CommandSystem.Register( "TaskManagerTestGump", AccessLevel.Administrator, new CommandEventHandler( PerformanceTestGump_OnCommand ) );
			CommandSystem.Register( "TMTest", AccessLevel.Administrator, new CommandEventHandler( PerformanceTestGump_OnCommand ) );
			CommandSystem.Register( "PerfTest", AccessLevel.Administrator, new CommandEventHandler( PerformanceTest_OnCommand ) );
			CommandSystem.Register( "PerfTestGump", AccessLevel.Administrator, new CommandEventHandler( PerformanceTestGump_OnCommand ) );
		}

		[Usage( "TestSuite <status|gump|progress|help|network ...|save ...|console ...|runebag ...|hourly confirm|150 confirm|200 confirm|250 confirm|daily confirm|longrun ...>" )]
		[Description( "Opens and controls the Administrator-only unified Performance Test Suite." )]
		private static void TestSuite_OnCommand( CommandEventArgs e )
		{
			HandleCommand( e, true );
		}

		private static void TaskManagerTest_OnCommand( CommandEventArgs e )
		{
			SendDeprecatedAliasNotice( e );
			HandleCommand( e, true );
		}

		private static void PerformanceTest_OnCommand( CommandEventArgs e )
		{
			SendDeprecatedAliasNotice( e );
			HandleCommand( e, true );
		}

		private static void HandleCommand( CommandEventArgs e, bool openOnEmpty )
		{
			if ( e == null || e.Mobile == null || e.Mobile.AccessLevel < AccessLevel.Administrator )
				return;

			if ( e.Length == 0 )
			{
				if ( openOnEmpty )
					TaskManagerTestController.OpenGump( e.Mobile );
				else
					WriteHelp( e.Mobile );

				return;
			}

			string action = e.GetString( 0 );

			if ( IsArg( action, "help" ) )
			{
				WriteHelp( e.Mobile );
			}
			else if ( IsArg( action, "gump" ) )
			{
				TaskManagerTestController.OpenGump( e.Mobile );
			}
			else if ( IsArg( action, "progress" ) )
			{
				TaskManagerTestController.OpenProgressGump( e.Mobile );
			}
			else if ( IsArg( action, "status" ) || IsArg( action, "diagnostics" ) )
			{
				WriteStatus( e.Mobile );
			}
			else if ( IsArg( action, "longrun" ) )
			{
				HandleLongRun( e );
			}
			else if ( IsArg( action, "save" ) || IsArg( action, "savediagnostics" ) || IsArg( action, "savestats" ) )
			{
				HandleSaveDiagnostics( e );
			}
			else if ( IsArg( action, "console" ) || IsArg( action, "consolelog" ) )
			{
				HandleConsoleTranscript( e );
			}
			else if ( IsArg( action, "runebag" ) )
			{
				HandleRuneBagDiagnostics( e );
			}
			else if ( IsArg( action, "network" ) || IsArg( action, "netflight" ) )
			{
				HandleNetworkDiagnostics( e );
			}
			else if ( IsManualFamily( action ) )
			{
				if ( e.Length < 2 || !IsArg( e.GetString( 1 ), "confirm" ) )
				{
					SendLine( e.Mobile, "Performance Test refused: mutating TaskManager commands require the literal word 'confirm'." );
					SendLine( e.Mobile, "These commands run real maintenance logic and may delete, create, respawn, broadcast, and update last-run state." );
					SendLine( e.Mobile, "Example: [TestSuite daily confirm" );
					SendLine( e.Mobile, "Open [TestSuite for the guided testing suite." );
					return;
				}

				TaskManagerTestController.RunManualCommand( e.Mobile, NormalizeFamily( action ) );
			}
			else
			{
				SendLine( e.Mobile, "Unknown Performance Test subcommand." );
				WriteHelp( e.Mobile );
			}
		}

		private static void HandleLongRun( CommandEventArgs e )
		{
			if ( e.Length < 2 )
			{
				SendLine( e.Mobile, "Usage: [TestSuite longrun <start minutes intervalMinutes confirm|stop confirm|status>" );
				return;
			}

			string action = e.GetString( 1 );

			if ( IsArg( action, "status" ) )
			{
				WriteStatus( e.Mobile );
				return;
			}

			if ( IsArg( action, "stop" ) )
			{
				if ( e.Length < 3 || !IsArg( e.GetString( 2 ), "confirm" ) )
				{
					SendLine( e.Mobile, "Stopping a long-run session requires: [TestSuite longrun stop confirm" );
					return;
				}

				TaskManagerTestController.StopLongRun( e.Mobile );
				return;
			}

			if ( IsArg( action, "start" ) )
			{
				int duration;
				int interval;

				if ( e.Length < 5
					|| !Int32.TryParse( e.GetString( 2 ), out duration )
					|| !Int32.TryParse( e.GetString( 3 ), out interval )
					|| !IsArg( e.GetString( 4 ), "confirm" ) )
				{
					SendLine( e.Mobile, "Usage: [TestSuite longrun start <minutes> <intervalMinutes> confirm" );
					return;
				}

				TaskManagerTestController.StartLongRun( e.Mobile, duration, interval );
				return;
			}

			SendLine( e.Mobile, "Unknown long-run subcommand." );
		}

		private static void HandleSaveDiagnostics( CommandEventArgs e )
		{
			if ( e.Length < 2 || IsArg( e.GetString( 1 ), "status" ) )
			{
				TaskManagerTestController.ShowSaveDiagnosticsStatus( e.Mobile );
				return;
			}

			string action = e.GetString( 1 );

			if ( IsArg( action, "mode" ) )
			{
				if ( e.Length < 3 )
				{
					SendLine( e.Mobile, "Usage: [TestSuite save mode aggregate|deep" );
					return;
				}

				if ( IsArg( e.GetString( 2 ), "aggregate" ) )
				{
					TaskManagerTestController.SetSaveDiagnosticsMode( e.Mobile, SaveDiagnosticMode.Aggregate );
					return;
				}

				if ( IsArg( e.GetString( 2 ), "deep" ) )
				{
					TaskManagerTestController.SetSaveDiagnosticsMode( e.Mobile, SaveDiagnosticMode.Deep );
					return;
				}

				SendLine( e.Mobile, "Usage: [TestSuite save mode aggregate|deep" );
				return;
			}

			if ( IsArg( action, "deep" ) && e.Length >= 3 && IsArg( e.GetString( 2 ), "next" ) )
			{
				TaskManagerTestController.RequestDeepNextSave( e.Mobile );
				return;
			}

			if ( IsArg( action, "reset" ) )
			{
				TaskManagerTestController.ResetSaveDiagnostics( e.Mobile );
				return;
			}

			if ( IsArg( action, "help" ) )
			{
				SendLine( e.Mobile, "Save diagnostics: [TestSuite save status | mode aggregate | mode deep | deep next | reset" );
				return;
			}

			SendLine( e.Mobile, "Unknown save diagnostics subcommand. Use [TestSuite save help." );
		}

		private static void HandleConsoleTranscript( CommandEventArgs e )
		{
			if ( e.Length < 2 || IsArg( e.GetString( 1 ), "status" ) )
			{
				TaskManagerTestController.ShowConsoleTranscriptStatus( e.Mobile );
				return;
			}

			string action = e.GetString( 1 );

			if ( IsArg( action, "flush" ) )
			{
				TaskManagerTestController.FlushConsoleTranscript( e.Mobile );
				return;
			}

			if ( IsArg( action, "rotate" ) )
			{
				TaskManagerTestController.RotateConsoleTranscript( e.Mobile );
				return;
			}

			if ( IsArg( action, "help" ) )
			{
				SendLine( e.Mobile, "Console transcript: [TestSuite console status | flush | rotate" );
				return;
			}

			SendLine( e.Mobile, "Unknown console transcript subcommand. Use [TestSuite console help." );
		}

		private static void HandleRuneBagDiagnostics( CommandEventArgs e )
		{
			if ( e.Length < 2 || IsArg( e.GetString( 1 ), "status" ) )
			{
				TaskManagerTestController.ShowRuneBagDiagnosticsStatus( e.Mobile );
				return;
			}

			if ( IsArg( e.GetString( 1 ), "reset" ) )
			{
				TaskManagerTestController.ResetRuneBagDiagnostics( e.Mobile );
				return;
			}

			if ( IsArg( e.GetString( 1 ), "help" ) )
			{
				SendLine( e.Mobile, "Rune bag diagnostics: [TestSuite runebag status | reset" );
				return;
			}

			SendLine( e.Mobile, "Unknown rune bag diagnostics subcommand. Use [TestSuite runebag help." );
		}

		private static void HandleNetworkDiagnostics( CommandEventArgs e )
		{
			if ( e.Length < 2 || IsArg( e.GetString( 1 ), "status" ) )
			{
				TaskManagerTestController.ShowNetFlightDiagnosticsStatus( e.Mobile );
				return;
			}

			string action = e.GetString( 1 );

			if ( IsArg( action, "on" ) || IsArg( action, "enable" ) )
			{
				TaskManagerTestController.SetNetFlightDiagnosticsEnabled( e.Mobile, true );
				return;
			}

			if ( IsArg( action, "off" ) || IsArg( action, "disable" ) )
			{
				if ( e.Length < 3 || !IsArg( e.GetString( 2 ), "confirm" ) )
				{
					SendLine( e.Mobile, "Disabling automatic per-player network capture requires: [TestSuite network off confirm" );
					return;
				}

				TaskManagerTestController.SetNetFlightDiagnosticsEnabled( e.Mobile, false );
				return;
			}

			if ( IsArg( action, "help" ) )
			{
				SendLine( e.Mobile, "Network diagnostics: [TestSuite network status | on | off confirm" );
				SendLine( e.Mobile, "Capture is automatic and per connected player; no manual player targeting is required." );
				return;
			}

			SendLine( e.Mobile, "Unknown network diagnostics subcommand. Use [TestSuite network help." );
		}

		private static void PerformanceTestGump_OnCommand( CommandEventArgs e )
		{
			SendDeprecatedAliasNotice( e );
			HandleCommand( e, true );
		}

		private static bool IsManualFamily( string action )
		{
			return IsArg( action, "hourly" ) || IsArg( action, "150" ) || IsArg( action, "200" ) || IsArg( action, "250" ) || IsArg( action, "daily" );
		}

		private static string NormalizeFamily( string action )
		{
			if ( IsArg( action, "hourly" ) )
				return "hourly";

			if ( IsArg( action, "150" ) )
				return "150";

			if ( IsArg( action, "200" ) )
				return "200";

			if ( IsArg( action, "250" ) )
				return "250";

			return "daily";
		}

		private static bool IsArg( string value, string expected )
		{
			return String.Equals( value, expected, StringComparison.OrdinalIgnoreCase );
		}

		private static void WriteHelp( Mobile from )
		{
			SendLine( from, "Performance Test Suite usage:" );
			SendLine( from, "[TestSuite opens the Gump; operational diagnostics are on the Operations tab." );
			SendLine( from, "Verbose Network/Save/Console/RuneBag status is available through [TestSuite network|save|console|runebag status." );
			SendLine( from, "  [TestSuite" );
			SendLine( from, "  [TestSuite gump" );
			SendLine( from, "  [TestSuite progress" );
			SendLine( from, "  [TestSuite status" );
			SendLine( from, "  [TestSuite network status" );
			SendLine( from, "  [TestSuite network on" );
			SendLine( from, "  [TestSuite network off confirm" );
			SendLine( from, "  [TestSuite save status" );
			SendLine( from, "  [TestSuite save mode aggregate" );
			SendLine( from, "  [TestSuite save mode deep" );
			SendLine( from, "  [TestSuite save deep next" );
			SendLine( from, "  [TestSuite console status" );
			SendLine( from, "  [TestSuite console flush" );
			SendLine( from, "  [TestSuite console rotate" );
			SendLine( from, "  [TestSuite runebag status" );
			SendLine( from, "  [TestSuite runebag reset" );
			SendLine( from, "  [TestSuite hourly confirm" );
			SendLine( from, "  [TestSuite 150 confirm" );
			SendLine( from, "  [TestSuite 200 confirm" );
			SendLine( from, "  [TestSuite 250 confirm" );
			SendLine( from, "  [TestSuite daily confirm" );
			SendLine( from, "  [TestSuite longrun start <minutes> <intervalMinutes> confirm" );
			SendLine( from, "  [TestSuite longrun stop confirm" );
			SendLine( from, "  [TestSuite longrun status" );
			SendLine( from, "Mutating commands require confirm and refuse while World.Saving, another test is active, or the settling guard is active." );
			SendLine( from, "Long-run diagnostics only captures reports; it does not run mutating TaskManager tasks." );
		}

		private static void SendDeprecatedAliasNotice( CommandEventArgs e )
		{
			if ( e != null && e.Mobile != null && e.Mobile.AccessLevel >= AccessLevel.Administrator )
				SendLine( e.Mobile, "This command is deprecated; use [TestSuite." );
		}

		private static void WriteStatus( Mobile from )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				TaskManagerTestController.WriteStatus( writer );
				SendLines( from, writer );
			}
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
			using ( StringReader reader = new StringReader( writer.ToString() ) )
			{
				string line;

				while ( ( line = reader.ReadLine() ) != null )
					SendLine( from, line );
			}
		}
	}
}
