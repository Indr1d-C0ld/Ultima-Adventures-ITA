using System;
using System.IO;
using Server.Commands;
using Server.Diagnostics;
using Server.Engines.XmlSpawner2;
using Server.Mobiles;
using Server.Network;
using Server.OneTime.Events;

namespace Server.Misc
{
	public class LagDiagnostics
	{
		private delegate void StatsWriter( TextWriter writer );

		public static void Initialize()
		{
			CommandSystem.Register( "LagReport", AccessLevel.Administrator, new CommandEventHandler( LagReport_OnCommand ) );
			CommandSystem.Register( "LagStatus", AccessLevel.Administrator, new CommandEventHandler( LagStatus_OnCommand ) );
			CommandSystem.Register( "TimerStats", AccessLevel.Administrator, new CommandEventHandler( TimerStats_OnCommand ) );
			CommandSystem.Register( "SpawnEntryStats", AccessLevel.Administrator, new CommandEventHandler( SpawnEntryStats_OnCommand ) );
			CommandSystem.Register( "SpawnEntryStatsReset", AccessLevel.Administrator, new CommandEventHandler( SpawnEntryStatsReset_OnCommand ) );
			CommandSystem.Register( "SpawnEntryBurstStats", AccessLevel.Administrator, new CommandEventHandler( SpawnEntryBurstStats_OnCommand ) );
			CommandSystem.Register( "RuneBagStats", AccessLevel.Administrator, new CommandEventHandler( RuneBagStats_OnCommand ) );
			CommandSystem.Register( "RuneBagStatsReset", AccessLevel.Administrator, new CommandEventHandler( RuneBagStatsReset_OnCommand ) );
			CommandSystem.Register( "SaveStats", AccessLevel.Administrator, new CommandEventHandler( SaveStats_OnCommand ) );
			CommandSystem.Register( "SaveDiagnostics", AccessLevel.Administrator, new CommandEventHandler( SaveStats_OnCommand ) );
			CommandSystem.Register( "ConsoleLog", AccessLevel.Administrator, new CommandEventHandler( ConsoleLog_OnCommand ) );
			CommandSystem.Register( "TaskManagerStats", AccessLevel.Administrator, new CommandEventHandler( TaskManagerStats_OnCommand ) );
			CommandSystem.Register( "TaskManagerDailyAttributionStats", AccessLevel.Administrator, new CommandEventHandler( TaskManagerDailyAttributionStats_OnCommand ) );
			CommandSystem.Register( "TaskManagerDailyAttributionStatsReset", AccessLevel.Administrator, new CommandEventHandler( TaskManagerDailyAttributionStatsReset_OnCommand ) );
			CommandSystem.Register( "BalancePortalStats", AccessLevel.Administrator, new CommandEventHandler( BalancePortalStats_OnCommand ) );
			CommandSystem.Register( "BalancePortalValidate", AccessLevel.Administrator, new CommandEventHandler( BalancePortalValidate_OnCommand ) );
			CommandSystem.Register( "PacketStats", AccessLevel.Administrator, new CommandEventHandler( PacketStats_OnCommand ) );
			CommandSystem.Register( "ThrottleStats", AccessLevel.Administrator, new CommandEventHandler( ThrottleStats_OnCommand ) );
			CommandSystem.Register( "MovementStats", AccessLevel.Administrator, new CommandEventHandler( MovementStats_OnCommand ) );
			CommandSystem.Register( "MovePathStats", AccessLevel.Administrator, new CommandEventHandler( MovePathStats_OnCommand ) );
			CommandSystem.Register( "SetLocationStats", AccessLevel.Administrator, new CommandEventHandler( SetLocationStats_OnCommand ) );
			CommandSystem.Register( "SetLocationLosStats", AccessLevel.Administrator, new CommandEventHandler( SetLocationLosStats_OnCommand ) );
			CommandSystem.Register( "PlayerLocationStats", AccessLevel.Administrator, new CommandEventHandler( PlayerLocationStats_OnCommand ) );
			CommandSystem.Register( "OnMovementStats", AccessLevel.Administrator, new CommandEventHandler( OnMovementStats_OnCommand ) );
			CommandSystem.Register( "XmlAttachStats", AccessLevel.Administrator, new CommandEventHandler( XmlAttachStats_OnCommand ) );
			CommandSystem.Register( "StatusPageStats", AccessLevel.Administrator, new CommandEventHandler( StatusPageStats_OnCommand ) );
			CommandSystem.Register( "CollectStatsStats", AccessLevel.Administrator, new CommandEventHandler( CollectStatsStats_OnCommand ) );
			CommandSystem.Register( "PremiumSpawnerStats", AccessLevel.Administrator, new CommandEventHandler( PremiumSpawnerStats_OnCommand ) );
			CommandSystem.Register( "PremiumSpawnerBudgetStats", AccessLevel.Administrator, new CommandEventHandler( PremiumSpawnerBudgetStats_OnCommand ) );
			CommandSystem.Register( "PremiumSpawnerSchedulerStats", AccessLevel.Administrator, new CommandEventHandler( PremiumSpawnerSchedulerStats_OnCommand ) );
			CommandSystem.Register( "StatisticsRegistryStats", AccessLevel.Administrator, new CommandEventHandler( StatisticsRegistryStats_OnCommand ) );
			CommandSystem.Register( "CollectStatsRebuild", AccessLevel.Administrator, new CommandEventHandler( CollectStatsRebuild_OnCommand ) );
			CommandSystem.Register( "OneTimeStats", AccessLevel.Administrator, new CommandEventHandler( OneTimeStats_OnCommand ) );
			CommandSystem.Register( "OneTimeRebuild", AccessLevel.Administrator, new CommandEventHandler( OneTimeRebuild_OnCommand ) );
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

		private static void WriteLagStatus( TextWriter writer )
		{
			writer.WriteLine( "Online: {0}", NetState.Instances.Count );
			writer.WriteLine( "Partial sends: {0}", NetState.PartialSends );
			writer.WriteLine( "Timer.Slice last={0}ms max={1}ms queueLast={2} queueMax={3}", Timer.LastSliceDuration, Timer.MaxSliceDuration, Timer.LastQueueCountAtSlice, Timer.MaxQueueCountAtSlice );
		}

		private static void WriteSection( TextWriter writer, string title, StatsWriter statsWriter )
		{
			writer.WriteLine();
			writer.WriteLine( "================================================================================" );
			writer.WriteLine( title );
			writer.WriteLine( "================================================================================" );

			try
			{
				statsWriter( writer );
			}
			catch ( Exception ex )
			{
				writer.WriteLine( "ERROR while writing {0}: {1}", title, ex );
			}
		}

		public static void WriteLagReport( TextWriter writer, Mobile requestedBy )
		{
			writer.WriteLine( "Ultima Adventures Lag Diagnostics Report" );
			writer.WriteLine( "Generated local: {0}", DateTime.Now );
			writer.WriteLine( "Generated UTC: {0}", DateTime.UtcNow );
			writer.WriteLine( "Requested by: {0}", requestedBy != null ? requestedBy.RawName : "Console" );
			writer.WriteLine( "Online: {0}", NetState.Instances.Count );

			// Diagnostic integration rule:
			// New stats commands must also write through LagReport. New diagnostic tests and
			// reports must also be integrated into the Performance Test Suite. Standalone
			// commands are convenience wrappers, never the primary reporting path.
			WriteSection( writer, "LagStatus", WriteLagStatus );
			WriteSection( writer, "TimerStats", Timer.WriteTimerStats );
			WriteSection( writer, "TimerSliceBreakdown", Timer.WriteTimerSliceBreakdown );
			WriteSection( writer, "SpawnEntryStats", SpawnEntryDiagnostics.WriteStats );
			WriteSection( writer, "SpawnEntryBurstStats", SpawnEntryDiagnostics.WriteBurstStats );
			WriteSection( writer, "RuneBagStats", RuneBagDiagnostics.WriteStats );
			WriteSection( writer, "DiscordBridgeStats", DiscordBridge.WriteStats );
			WriteSection( writer, "SaveStats", SaveDiagnostics.WriteStats );
			WriteSection( writer, "ConsoleTranscript", ConsoleTranscript.WriteStats );
			WriteSection( writer, "TaskManagerStats", TaskManagerDiagnostics.WriteStats );
			WriteSection( writer, "TaskManagerDailyAttributionStats", TaskManagerDailyAttributionDiagnostics.WriteStats );
			WriteSection( writer, "BalancePortalStats", BalancePortalDiagnostics.WriteStats );
			WriteSection( writer, "PerformanceTest / TaskManagerTest", TaskManagerTestController.WriteStatus );
			WriteSection( writer, "NetFlightStats", NetFlightRecorder.WriteDiagnostics );
			WriteSection( writer, "PacketStats", MessagePump.WritePacketStats );
			WriteSection( writer, "ThrottleStats", MessagePump.WriteThrottleStats );
			WriteSection( writer, "MovementStats", PacketHandlers.WriteMovementStats );
			WriteSection( writer, "MovePathStats", MovePathDiagnostics.WriteStats );
			WriteSection( writer, "SetLocationStats", SetLocationDiagnostics.WriteStats );
			WriteSection( writer, "SetLocationLosStats", SetLocationLosDiagnostics.WriteStats );
			WriteSection( writer, "PlayerLocationStats", PlayerLocationDiagnostics.WriteStats );
			WriteSection( writer, "XmlAttachStats", XmlAttach.WriteMovementStats );
			WriteSection( writer, "OnMovementStats", OnMovementDiagnostics.WriteStats );
			WriteSection( writer, "OneTimeStats", OneTimeEventHelper.WriteStats );
			WriteSection( writer, "PremiumSpawnerStats", PremiumSpawner.WriteStats );
			WriteSection( writer, "PremiumSpawnerSchedulerStats", PremiumSpawner.WriteSchedulerStats );
			WriteSection( writer, "StatusPageStats", StatusPage.WriteStats );
			WriteSection( writer, "CollectStatsStats", Statistics.WriteStats );
			WriteSection( writer, "PremiumSpawnerBudgetStats", PremiumSpawner.WriteBudgetStats );
			WriteSection( writer, "StatisticsRegistryStats", Statistics.WriteRegistryStats );
			WriteSection( writer, "BaseAIStats", WriteBaseAIStatsUnavailable );
		}

		public static string GenerateLagReport( Mobile requestedBy, string directory, string fileNameBase )
		{
			if ( String.IsNullOrWhiteSpace( directory ) )
				directory = Path.Combine( "Logs", "LagDiagnostics" );

			if ( String.IsNullOrWhiteSpace( fileNameBase ) )
				fileNameBase = String.Format( "LagReport-{0:yyyyMMdd-HHmmss}", DateTime.Now );

			Directory.CreateDirectory( directory );

			string path = GetUniqueReportPath( directory, fileNameBase );

			using ( StreamWriter writer = new StreamWriter( path, false ) )
			{
				WriteLagReport( writer, requestedBy );
			}

			return Path.GetFullPath( path );
		}

		private static string GetUniqueReportPath( string directory, string fileNameBase )
		{
			string safeBase = fileNameBase;

			foreach ( char c in Path.GetInvalidFileNameChars() )
				safeBase = safeBase.Replace( c, '_' );

			string path = Path.Combine( directory, safeBase + ".txt" );

			if ( !File.Exists( path ) )
				return path;

			for ( int i = 1; i < 1000; ++i )
			{
				path = Path.Combine( directory, String.Format( "{0}-{1}.txt", safeBase, i ) );

				if ( !File.Exists( path ) )
					return path;
			}

			return Path.Combine( directory, String.Format( "{0}-{1}.txt", safeBase, Guid.NewGuid().ToString( "N" ) ) );
		}

		private static void WriteBaseAIStatsUnavailable( TextWriter writer )
		{
			writer.WriteLine( "Not implemented. BaseAI correlation is currently included as aiTimerContext in MovePathStats, OnMovementStats, and SetLocationLosStats." );
		}

		private static void LagReport_OnCommand( CommandEventArgs e )
		{
			string directory = Path.Combine( "Logs", "LagDiagnostics" );
			string fullPath = GenerateLagReport( e.Mobile, directory, String.Format( "LagReport-{0:yyyyMMdd-HHmmss}", DateTime.Now ) );

			SendLine( e.Mobile, String.Format( "Lag diagnostics report written to: {0}", fullPath ) );
		}

		private static void LagStatus_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				WriteLagStatus( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void TimerStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				Timer.WriteTimerStats( writer );
				Timer.WriteTimerSliceBreakdown( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void SaveStats_OnCommand( CommandEventArgs e )
		{
			if ( e.Length > 0 )
			{
				string action = e.GetString( 0 );

				if ( IsArg( action, "status" ) )
				{
					using ( StringWriter writer = new StringWriter() )
					{
						SaveDiagnostics.WriteStats( writer );
						SendLines( e.Mobile, writer );
					}

					return;
				}

				if ( IsArg( action, "mode" ) )
				{
					if ( e.Length < 2 )
					{
						SendLine( e.Mobile, "Usage: [SaveStats mode aggregate|deep" );
						return;
					}

					SaveDiagnosticMode mode;

					if ( IsArg( e.GetString( 1 ), "aggregate" ) )
						mode = SaveDiagnosticMode.Aggregate;
					else if ( IsArg( e.GetString( 1 ), "deep" ) )
						mode = SaveDiagnosticMode.Deep;
					else
					{
						SendLine( e.Mobile, "Usage: [SaveStats mode aggregate|deep" );
						return;
					}

					string message;
					SaveDiagnostics.SetConfiguredMode( mode, out message );
					SendLine( e.Mobile, message );
					return;
				}

				if ( IsArg( action, "deep" ) && e.Length >= 2 && IsArg( e.GetString( 1 ), "next" ) )
				{
					string message;
					SaveDiagnostics.RequestDeepNextSave( out message );
					SendLine( e.Mobile, message );
					return;
				}

				if ( IsArg( action, "reset" ) )
				{
					SaveDiagnostics.Reset();
					SendLine( e.Mobile, "Save diagnostic counters and samples reset. Persistent mode was not changed." );
					return;
				}

				if ( IsArg( action, "help" ) )
				{
					SendLine( e.Mobile, "Save diagnostics: [SaveStats status | mode aggregate | mode deep | deep next | reset" );
					return;
				}

				SendLine( e.Mobile, "Unknown SaveStats action. Use [SaveStats help." );
				return;
			}

			using ( StringWriter writer = new StringWriter() )
			{
				SaveDiagnostics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void ConsoleLog_OnCommand( CommandEventArgs e )
		{
			if ( e.Length == 0 || IsArg( e.GetString( 0 ), "status" ) )
			{
				using ( StringWriter writer = new StringWriter() )
				{
					ConsoleTranscript.WriteStats( writer );
					writer.WriteLine( "Warning: console transcript files may contain IP addresses, account names, character names, local paths, and exception details. Sanitize before sharing publicly." );
					SendLines( e.Mobile, writer );
				}

				return;
			}

			string action = e.GetString( 0 );

			if ( IsArg( action, "flush" ) )
			{
				bool completed = ConsoleTranscript.FlushNow( 3000 );
				SendLine( e.Mobile, completed ? "Console transcript flush completed." : "Console transcript flush requested; timeout reached before confirmation." );
				return;
			}

			if ( IsArg( action, "rotate" ) )
			{
				bool completed = ConsoleTranscript.RotateNow( 3000 );
				SendLine( e.Mobile, completed ? "Console transcript rotated to the next part." : "Console transcript rotation requested; timeout reached before confirmation." );
				return;
			}

			if ( IsArg( action, "help" ) )
			{
				SendLine( e.Mobile, "ConsoleLog commands: [ConsoleLog status | flush | rotate" );
				return;
			}

			SendLine( e.Mobile, "Unknown ConsoleLog action. Use [ConsoleLog help." );
		}

		private static bool IsArg( string value, string expected )
		{
			return String.Equals( value, expected, StringComparison.OrdinalIgnoreCase );
		}

		private static void SpawnEntryStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				SpawnEntryDiagnostics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void SpawnEntryStatsReset_OnCommand( CommandEventArgs e )
		{
			SpawnEntryDiagnostics.Reset();
			SendLine( e.Mobile, "SpawnEntryStats reset." );
		}

		private static void SpawnEntryBurstStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				SpawnEntryDiagnostics.WriteBurstStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void RuneBagStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				RuneBagDiagnostics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void RuneBagStatsReset_OnCommand( CommandEventArgs e )
		{
			RuneBagDiagnostics.Reset();
			SendLine( e.Mobile, "RuneBagStats reset. Active throttle guards were preserved." );
		}

		private static void TaskManagerStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				TaskManagerDiagnostics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void TaskManagerDailyAttributionStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				TaskManagerDailyAttributionDiagnostics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void TaskManagerDailyAttributionStatsReset_OnCommand( CommandEventArgs e )
		{
			TaskManagerDailyAttributionDiagnostics.Reset();
			SendLine( e.Mobile, "TaskManagerDailyAttributionStats reset." );
		}

		private static void BalancePortalStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				BalancePortalDiagnostics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void BalancePortalValidate_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				Server.Items.TaskManagerDaily.WriteBalancePortalValidation( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void PacketStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				MessagePump.WritePacketStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void ThrottleStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				MessagePump.WriteThrottleStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void MovementStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				PacketHandlers.WriteMovementStats( writer );
				MovePathDiagnostics.WriteStats( writer );
				SetLocationDiagnostics.WriteStats( writer );
				SetLocationLosDiagnostics.WriteStats( writer );
				PlayerLocationDiagnostics.WriteStats( writer );
				OnMovementDiagnostics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void MovePathStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				MovePathDiagnostics.WriteStats( writer );
				SetLocationDiagnostics.WriteStats( writer );
				SetLocationLosDiagnostics.WriteStats( writer );
				PlayerLocationDiagnostics.WriteStats( writer );
				OnMovementDiagnostics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void SetLocationStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				SetLocationDiagnostics.WriteStats( writer );
				SetLocationLosDiagnostics.WriteStats( writer );
				PlayerLocationDiagnostics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void SetLocationLosStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				SetLocationLosDiagnostics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void PlayerLocationStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				PlayerLocationDiagnostics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void OnMovementStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				OnMovementDiagnostics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void XmlAttachStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				XmlAttach.WriteMovementStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void StatusPageStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				StatusPage.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void CollectStatsStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				Statistics.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void PremiumSpawnerStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				PremiumSpawner.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void PremiumSpawnerBudgetStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				PremiumSpawner.WriteBudgetStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void PremiumSpawnerSchedulerStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				PremiumSpawner.WriteSchedulerStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void StatisticsRegistryStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				Statistics.WriteRegistryStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void CollectStatsRebuild_OnCommand( CommandEventArgs e )
		{
			Statistics.RebuildStatsRegistry();

			using ( StringWriter writer = new StringWriter() )
			{
				Statistics.WriteRegistryStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void OneTimeStats_OnCommand( CommandEventArgs e )
		{
			using ( StringWriter writer = new StringWriter() )
			{
				OneTimeEventHelper.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}

		private static void OneTimeRebuild_OnCommand( CommandEventArgs e )
		{
			OneTimeEventHelper.RebuildRegistry();

			using ( StringWriter writer = new StringWriter() )
			{
				OneTimeEventHelper.WriteStats( writer );
				SendLines( e.Mobile, writer );
			}
		}
	}
}
