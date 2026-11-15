using System;
using System.IO;
using Server;
using Server.Commands;
using System.Collections;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Misc;
using Server.Items;
using Server.Targeting;
using Server.Network;
using Server.Diagnostics;

namespace Server.Misc
{
	public class AutoSave : Timer
	{
		private static TimeSpan m_Delay = TimeSpan.FromMinutes( MyServerSettings.ServerSaveMinutes() );
		private static TimeSpan m_Warning = TimeSpan.Zero;

		public static void Initialize()
		{
			new AutoSave().Start();
			CommandSystem.Register( "SetSaves", AccessLevel.Administrator, new CommandEventHandler( SetSaves_OnCommand ) );
		}

		private static bool m_SavesEnabled = true;

		public static bool SavesEnabled
		{
			get{ return m_SavesEnabled; }
			set{ m_SavesEnabled = value; }
		}

		[Usage( "SetSaves <true | false>" )]
		[Description( "Enables or disables automatic shard saving." )]
		public static void SetSaves_OnCommand( CommandEventArgs e )
		{
			if ( e.Length == 1 )
			{
				m_SavesEnabled = e.GetBoolean( 0 );
				e.Mobile.SendMessage( "Saves have been {0}.", m_SavesEnabled ? "enabled" : "disabled" );
			}
			else
			{
				e.Mobile.SendMessage( "Format: SetSaves <true | false>" );
			}
		}

		public AutoSave() : base( m_Delay - m_Warning, m_Delay )
		{
			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			if ( !m_SavesEnabled || AutoRestart.Restarting )
				return;

			if ( m_Warning == TimeSpan.Zero )
			{
				long cleanupStart = SaveDiagnostics.GetTimestamp();
				int cleanupDeleted = 0;

				// CLEAN UP ANY GRAVE DISTURBED MONSTERS OR TREASURE MAP MONSTERS
				ArrayList monsters = new ArrayList();
				foreach ( Mobile m in World.Mobiles.Values )
				{
					if ( m is BaseCreature )
					{
						BaseCreature bc = (BaseCreature)m;
						if ( ( bc.ControlSlots == 666 ) && ( m.Combatant == null ) )
							monsters.Add( bc );
					}
				}
				for ( int i = 0; i < monsters.Count; ++i )
				{
					Mobile ridof = ( Mobile )monsters[ i ];
					Effects.SendLocationParticles( EffectItem.Create( ridof.Location, ridof.Map, EffectItem.DefaultDuration ), 0x3728, 8, 20, 5042 );
					Effects.PlaySound( ridof, ridof.Map, 0x201 );
					ridof.Delete();
					++cleanupDeleted;
				}

				Save( true, false, SaveDiagnostics.GetElapsedMilliseconds( cleanupStart ), cleanupDeleted, true );
			}
			else
			{
				int s = (int)m_Warning.TotalSeconds;
				int m = s / 60;
				s %= 60;

				if ( m > 0 && s > 0 )
					World.Broadcast( 0x35, true, "The world will save in {0} minute{1} and {2} second{3}.", m, m != 1 ? "s" : "", s, s != 1 ? "s" : "" );
				else if ( m > 0 )
					World.Broadcast( 0x35, true, "The world will save in {0} minute{1}.", m, m != 1 ? "s" : "" );
				else
					World.Broadcast( 0x35, true, "The world will save in {0} second{1}.", s, s != 1 ? "s" : "" );

				Timer.DelayCall( m_Warning, new TimerCallback( Save ) );
			}
		}

                public static void Save()
                {
                        AutoSave.Save( false );
                }

                public static void Save( bool permitBackgroundWrite )
                {
                        Save( permitBackgroundWrite, false );
                }

                public static void SaveDuringRestart()
                {
                        Save( false, true );
                }

                private static void Save( bool permitBackgroundWrite, bool ignoreRestartFlag )
                {
                        Save( permitBackgroundWrite, ignoreRestartFlag, 0, 0, false );
                }

                private static void Save( bool permitBackgroundWrite, bool ignoreRestartFlag, long graveCleanupMs, int graveCleanupDeleted, bool scheduledAutoSave )
                {
                        if ( AutoRestart.Restarting && !ignoreRestartFlag )
                                return;

			long waitStart = SaveDiagnostics.GetTimestamp();
                        World.WaitForWriteCompletion();
			long preWorldWaitMs = SaveDiagnostics.GetElapsedMilliseconds( waitStart );

			long backupMs = 0;
			long backupStart = SaveDiagnostics.GetTimestamp();

                        try{ Backup(); }
                        catch ( Exception e ) { Console.WriteLine("WARNING: Automatic backup FAILED: {0}", e); }
			finally { backupMs = SaveDiagnostics.GetElapsedMilliseconds( backupStart ); }

			SaveDiagnostics.MarkNextSaveWrapper( scheduledAutoSave, permitBackgroundWrite, preWorldWaitMs, backupMs, graveCleanupMs, graveCleanupDeleted );
                        World.Save( true, permitBackgroundWrite );
                }

                private const double BackupRetentionDays = 3.0;

                private static readonly string[] UnitsWords = new string[]
                        {
                                "",
                                "One",
                                "Two",
                                "Three",
                                "Four",
                                "Five",
                                "Six",
                                "Seven",
                                "Eight",
                                "Nine"
                        };

                private static readonly string[] TensWords = new string[]
                        {
                                "",
                                "Ten",
                                "Twenty",
                                "Thirty",
                                "Forty",
                                "Fifty",
                                "Sixty",
                                "Seventy",
                                "Eighty",
                                "Ninety"
                        };

                private static readonly string[] OrdinalUnits = new string[]
                        {
                                "",
                                "First",
                                "Second",
                                "Third",
                                "Fourth",
                                "Fifth",
                                "Sixth",
                                "Seventh",
                                "Eighth",
                                "Ninth",
                                "Tenth",
                                "Eleventh",
                                "Twelfth",
                                "Thirteenth",
                                "Fourteenth",
                                "Fifteenth",
                                "Sixteenth",
                                "Seventeenth",
                                "Eighteenth",
                                "Nineteenth",
                                "Twentieth"
                        };

                private static readonly string[] OrdinalTens = new string[]
                        {
                                "",
                                "Tenth",
                                "Twentieth",
                                "Thirtieth",
                                "Fortieth",
                                "Fiftieth",
                                "Sixtieth",
                                "Seventieth",
                                "Eightieth",
                                "Ninetieth"
                        };

                private static readonly string[] m_Backups = CreateBackupLabels();

                private static void Backup()
                {
                        if ( m_Backups.Length == 0 )
                                return;

			string root = Path.Combine( Core.BaseDirectory, "Backups/Automatic" );

			if ( !Directory.Exists( root ) )
				Directory.CreateDirectory( root );

			string[] existing = Directory.GetDirectories( root );

			for ( int i = 0; i < m_Backups.Length; ++i )
			{
				DirectoryInfo dir = Match( existing, m_Backups[i] );

				if ( dir == null )
					continue;

				if ( i > 0 )
				{
					string timeStamp = FindTimeStamp( dir.Name );

					if ( timeStamp != null )
					{
						try{ dir.MoveTo( FormatDirectory( root, m_Backups[i - 1], timeStamp ) ); }
						catch{}
					}
				}
				else
				{
					try{ dir.Delete( true ); }
					catch{}
				}
			}

			string saves = Path.Combine( Core.BaseDirectory, "Saves" );

			if ( Directory.Exists( saves ) )
				Directory.Move( saves, FormatDirectory( root, m_Backups[m_Backups.Length - 1], GetTimeStamp() ) );
		}

		private static DirectoryInfo Match( string[] paths, string match )
		{
			for ( int i = 0; i < paths.Length; ++i )
			{
				DirectoryInfo info = new DirectoryInfo( paths[i] );

				if ( info.Name.StartsWith( match ) )
					return info;
			}

			return null;
		}

		private static string FormatDirectory( string root, string name, string timeStamp )
		{
			return Path.Combine( root, String.Format( "{0} ({1})", name, timeStamp ) );
		}

                private static string FindTimeStamp( string input )
                {
			int start = input.IndexOf( '(' );

			if ( start >= 0 )
			{
				int end = input.IndexOf( ')', ++start );

				if ( end >= start )
					return input.Substring( start, end-start );
			}

                        return null;
                }

                private static string[] CreateBackupLabels()
                {
                        double minutesPerSave = MyServerSettings.ServerSaveMinutes();

                        if ( minutesPerSave <= 0.0 )
                                minutesPerSave = 20.0; // Fallback to the default configuration

                        int count = (int)Math.Ceiling( TimeSpan.FromDays( BackupRetentionDays ).TotalMinutes / minutesPerSave );

                        if ( count < 1 )
                                count = 1;

                        string[] labels = new string[count];

                        if ( count == 1 )
                        {
                                labels[0] = "Most Recent";
                                return labels;
                        }

                        for ( int i = 0; i < count - 1; ++i )
                        {
                                int backupNumber = count - i;
                                labels[i] = String.Format( "{0} Backup", GetOrdinalWord( backupNumber ) );
                        }

                        labels[count - 1] = "Most Recent";

                        return labels;
                }

                private static string GetOrdinalWord( int number )
                {
                        if ( number <= 0 )
                                return number.ToString();

                        if ( number <= 20 )
                                return OrdinalUnits[number];

                        if ( number < 100 )
                        {
                                int tens = number / 10;
                                int units = number % 10;

                                if ( units == 0 )
                                        return OrdinalTens[tens];

                                return String.Format( "{0} {1}", TensWords[tens], OrdinalUnits[units] );
                        }

                        if ( number < 1000 )
                        {
                                int hundreds = number / 100;
                                int remainder = number % 100;

                                if ( remainder == 0 )
                                        return String.Format( "{0} Hundredth", UnitsWords[hundreds] );

                                return String.Format( "{0} Hundred {1}", UnitsWords[hundreds], GetOrdinalWord( remainder ) );
                        }

                        return number.ToString();
                }

                private static string GetTimeStamp()
		{
			DateTime now = DateTime.UtcNow;

			return String.Format( "{0}-{1}-{2} {3}-{4:D2}-{5:D2}",
					now.Day,
					now.Month,
					now.Year,
					now.Hour,
					now.Minute,
					now.Second
				);
		}
	}
}
