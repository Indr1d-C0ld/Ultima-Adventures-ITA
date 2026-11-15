using System;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Network;

namespace Server.Misc
{
        public class AutoRestart : Timer
        {
                public static bool Enabled = false; // is the script enabled?

                private static readonly TimeSpan DefaultRestartDelay = TimeSpan.FromMinutes( 5.0 );

                private static AutoRestart m_Timer;
                private static bool m_Restarting;
                private static DateTime m_RestartTime;
                private static int m_LastAnnounceSeconds = -1;
                private static string m_Message = String.Empty;
                private static bool m_ShouldRestart = true;
                private static bool m_NotifiedPlayers;
                private static DateTime m_LastPlayerNotify;

                public static bool Restarting
                {
                        get{ return m_Restarting; }
                }

                public static string CurrentMessage
                {
                        get { return m_Message; }
                }

                public static void Initialize()
                {
                        CommandSystem.Register( "Restart", AccessLevel.Administrator, new CommandEventHandler( Restart_OnCommand ) );
                        CommandSystem.Register( "RestartMenu", AccessLevel.Administrator, new CommandEventHandler( RestartMenu_OnCommand ) );

                        EventSink.Login += OnLogin;

                        if ( m_Timer == null )
                        {
                                m_Timer = new AutoRestart();
                                m_Timer.Start();
                        }
                }

                private static void RestartMenu_OnCommand( CommandEventArgs e )
                {
                        Server.Gumps.RestartMenuGump.SendTo( e.Mobile.NetState, m_Message );
                }

                private static void Restart_OnCommand( CommandEventArgs e )
                {
                        if ( StartCountdown( DefaultRestartDelay, m_Message, e.Mobile ) )
                                e.Mobile.SendMessage( "Restart countdown started for {0} minutes.", (int)Math.Ceiling( DefaultRestartDelay.TotalMinutes ) );
                }

                public static bool StartCountdown( TimeSpan delay, string message )
                {
                        return StartCountdown( delay, message, true, null );
                }

                public static bool StartCountdown( TimeSpan delay, string message, bool restart )
                {
                        return StartCountdown( delay, message, restart, null );
                }

                public static bool StartCountdown( TimeSpan delay, string message, Mobile issuer )
                {
                        return StartCountdown( delay, message, true, issuer );
                }

                public static bool StartCountdown( TimeSpan delay, string message, bool restart, Mobile issuer )
                {
                        if ( delay < TimeSpan.FromSeconds( 1.0 ) )
                        {
                                if ( issuer != null )
                                        issuer.SendMessage( 33, "Delay must be at least 1 second." );

                                return false;
                        }

                        Enabled = true;
                        m_Restarting = true;
                        m_RestartTime = DateTime.UtcNow + delay;
                        m_Message = message ?? String.Empty;
                        m_ShouldRestart = restart;
                        m_LastAnnounceSeconds = -1;
                        m_NotifiedPlayers = false;
                        m_LastPlayerNotify = DateTime.MinValue;

                        int announceSeconds = (int)Math.Ceiling( delay.TotalSeconds );

                        string scheduledMessage = BuildMessage( String.Format( "Server {0} scheduled in {1}.", GetActionName(), FormatTime( announceSeconds ) ), announceSeconds );
                        World.Broadcast( 0x22, true, scheduledMessage );
                        DiscordBridge.ReportRestartCountdownStarted( scheduledMessage );

                        if ( issuer != null )
                                issuer.SendMessage( 0x35, "Countdown updated: {0}.", GetActionName() );

                        SendRestartGumpToPlayers( delay );

                        return true;
                }

                public static bool CancelCountdown( Mobile issuer )
                {
                        if ( !m_Restarting )
                        {
                                if ( issuer != null )
                                        issuer.SendMessage( 33, "No active countdown to stop." );

                                return false;
                        }

                        m_Restarting = false;
                        m_LastAnnounceSeconds = -1;
                        m_NotifiedPlayers = false;

                        World.Broadcast( 0x22, true, BuildMessage( String.Format( "Server {0} cancelled.", GetActionName() ) ) );

                        RestartMenuGump.CloseActiveGumps();
                        RestartNotifyGump.CloseActiveGumps();

                        if ( issuer != null )
                                issuer.SendMessage( 0x35, "Countdown cancelled." );

                        return true;
                }

                public static bool TryGetRemaining( out TimeSpan remaining )
                {
                        if ( m_Restarting )
                        {
                                remaining = m_RestartTime - DateTime.UtcNow;
                                if ( remaining < TimeSpan.Zero )
                                        remaining = TimeSpan.Zero;

                                return true;
                        }

                        remaining = TimeSpan.Zero;
                        return false;
                }

                public static bool ShouldRestart
                {
                        get { return m_ShouldRestart; }
                }

                private AutoRestart() : base( TimeSpan.FromSeconds( 1.0 ), TimeSpan.FromSeconds( 1.0 ) )
                {
                        Priority = TimerPriority.OneSecond;
                }

                protected override void OnTick()
                {
                        if ( !Enabled || !m_Restarting )
                                return;

                        TimeSpan remaining = m_RestartTime - DateTime.UtcNow;

                        if ( remaining < TimeSpan.Zero )
                                remaining = TimeSpan.Zero;

                        int seconds = (int)Math.Ceiling( remaining.TotalSeconds );

                        if ( seconds != m_LastAnnounceSeconds && ShouldAnnounce( seconds ) )
                        {
                                m_LastAnnounceSeconds = seconds;
                                string announceMessage = BuildMessage( String.Format( "Server {0} in {1}.", GetActionName(), FormatTime( seconds ) ), seconds );
                                World.Broadcast( 0x22, true, announceMessage );

                                if ( seconds > 0 && (seconds % 60) == 0 )
                                        DiscordBridge.ReportRestartCountdownMinuteUpdate( announceMessage );
                        }

                        if ( !m_NotifiedPlayers )
                        {
                                SendRestartGumpToPlayers( remaining );
                                m_NotifiedPlayers = true;
                                m_LastPlayerNotify = DateTime.UtcNow;
                        }
                        else if ( DateTime.UtcNow - m_LastPlayerNotify >= TimeSpan.FromMinutes( 1.0 ) )
                        {
                                SendRestartGumpToPlayers( remaining );
                                m_LastPlayerNotify = DateTime.UtcNow;
                        }

                        Server.Gumps.RestartMenuGump.RefreshActiveGumps( m_Message );
                        Server.Gumps.RestartNotifyGump.RefreshActiveGumps( m_Message );

                        if ( remaining <= TimeSpan.Zero )
                        {
                                World.Broadcast( 0x35, true, BuildMessage( String.Format( "Server {0} now!", GetActionVerb() ) ) );
                                AutoSave.SaveDuringRestart();
                                Core.Kill( m_ShouldRestart );
                        }
                }

                private static void OnLogin( LoginEventArgs e )
                {
                        if ( e == null || e.Mobile == null )
                                return;

                        if ( !m_Restarting )
                                return;

                        TimeSpan remaining = m_RestartTime - DateTime.UtcNow;

                        if ( remaining < TimeSpan.Zero )
                                remaining = TimeSpan.Zero;

                        Server.Gumps.RestartNotifyGump.SendTo( e.Mobile.NetState, m_Message, remaining, m_ShouldRestart );
                }

                private static void SendRestartGumpToPlayers( TimeSpan remaining )
                {
                        Server.Gumps.RestartNotifyGump.SendToAll( m_Message, remaining, m_ShouldRestart );
                }

                private static bool ShouldAnnounce( int seconds )
                {
                        if ( seconds <= 15 )
                                return true;

                        if ( seconds == 60 || seconds == 45 || seconds == 30 )
                                return true;

                        if ( seconds % 60 == 0 )
                                return true;

                        return false;
                }

                private static string BuildMessage( string prefix, int? seconds = null )
                {
                        string message = m_Message;

                        if ( seconds.HasValue )
                                message = FormatAnnouncement( message, TimeSpan.FromSeconds( seconds.Value ) );
                        else if ( m_Restarting )
                                message = FormatAnnouncement( message, m_RestartTime - DateTime.UtcNow );
                        else
                                message = FormatAnnouncement( message, TimeSpan.Zero );

                        if ( !String.IsNullOrWhiteSpace( message ) )
                                return String.Format( "{0} {1}", prefix, message );

                        return prefix;
                }

                private static string GetActionName()
                {
                        return m_ShouldRestart ? "restart" : "shutdown";
                }

                private static string GetActionVerb()
                {
                        return m_ShouldRestart ? "restarting" : "shutting down";
                }

                private static string FormatTime( int totalSeconds )
                {
                        int minutes = totalSeconds / 60;
                        int seconds = totalSeconds % 60;

                        if ( minutes > 0 && seconds > 0 )
                                return String.Format( "{0} minute{1} {2} second{3}", minutes, minutes == 1 ? "" : "s", seconds, seconds == 1 ? "" : "s" );

                        if ( minutes > 0 )
                                return String.Format( "{0} minute{1}", minutes, minutes == 1 ? "" : "s" );

                        return String.Format( "{0} second{1}", seconds, seconds == 1 ? "" : "s" );
                }

                public static string FormatAnnouncement( string message, TimeSpan remaining )
                {
                        if ( String.IsNullOrWhiteSpace( message ) )
                                return message;

                        int seconds = (int)Math.Max( 0, Math.Ceiling( remaining.TotalSeconds ) );

                        return message.Replace( "[[time]]", FormatTime( seconds ) );
                }
        }
}
