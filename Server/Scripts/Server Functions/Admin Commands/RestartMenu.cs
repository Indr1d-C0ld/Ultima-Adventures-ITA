using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Misc;
using Server.Network;

namespace Server.Gumps
{
        public class RestartMenuGump : Gump
        {
                private static readonly HashSet<NetState> m_OpenStates = new HashSet<NetState>();

                private const int MessageEntryID = 1;
                private const int MinutesEntryID = 2;
                private const int SecondsEntryID = 3;
                private const int ShutdownSwitchID = 1;

                private readonly string m_Message;

                public static void SendTo( NetState state, string message )
                {
                        if ( state == null || state.Mobile == null )
                                return;

                        state.Mobile.CloseGump( typeof( RestartMenuGump ) );
                        state.Mobile.SendGump( new RestartMenuGump( state, message ) );

                        lock ( m_OpenStates )
                        {
                                m_OpenStates.Add( state );
                        }
                }

                public RestartMenuGump( NetState owner, string message ) : base( 50, 50 )
                {
                        m_Message = message ?? String.Empty;

                        Closable = true;
                        Disposable = true;
                        Dragable = true;
                        Resizable = false;

                        TimeSpan remaining;
                        bool active = AutoRestart.TryGetRemaining( out remaining );
                        bool restart = AutoRestart.ShouldRestart;

                        AddPage( 0 );
                        AddBackground( 0, 0, 520, 360, 9270 );
                        AddAlphaRegion( 10, 10, 500, 340 );

                        AddLabel( 30, 25, 0x34, "Server Restart Menu" );

                        AddLabel( 30, 60, 0x481, "Announcement" );
                        AddHtml( 30, 80, 460, 60, "<body bgcolor='#1A1A1A'></body>", false, false );
                        AddTextEntry( 40, 100, 440, 30, 0x481, MessageEntryID, m_Message );

                        int totalSeconds = 300;

                        if ( active )
                        {
                                totalSeconds = Math.Max( 1, (int)Math.Ceiling( remaining.TotalSeconds ) );
                                AddLabel( 30, 150, 0x34, String.Format( "Current countdown: {0} ({1})", FormatTime( totalSeconds ), restart ? "restart" : "shutdown" ) );
                        }
                        else
                        {
                                AddLabel( 30, 150, 0x34, "No active restart countdown." );
                        }

                        int minutes = totalSeconds / 60;
                        int seconds = totalSeconds % 60;

                        AddLabel( 30, 190, 0x481, "Minutes" );
                        AddHtml( 30, 210, 80, 24, "<body bgcolor='#1A1A1A'></body>", false, false );
                        AddTextEntry( 35, 212, 70, 20, 0x481, MinutesEntryID, minutes.ToString() );

                        AddLabel( 140, 190, 0x481, "Seconds" );
                        AddHtml( 140, 210, 80, 24, "<body bgcolor='#1A1A1A'></body>", false, false );
                        AddTextEntry( 145, 212, 70, 20, 0x481, SecondsEntryID, seconds.ToString() );

                        bool shutdownSelected = !restart;
                        AddCheck( 250, 210, 210, 211, shutdownSelected, ShutdownSwitchID );
                        AddLabel( 280, 212, 0x34, "Save and shutdown instead of restart" );

                        AddButton( 30, 260, 247, 248, 1, GumpButtonType.Reply, 0 );
                        AddLabel( 95, 260, 0x34, active ? "Update countdown" : "Start countdown" );

                        if ( active )
                        {
                                AddButton( 270, 260, 4014, 4016, 2, GumpButtonType.Reply, 0 );
                                AddLabel( 330, 260, 0x22, "Stop countdown" );
                        }
                }

                public override void OnResponse( NetState sender, RelayInfo info )
                {
                        if ( info.ButtonID == 0 )
                        {
                                Unregister( sender );
                                return;
                        }

                        if ( info.ButtonID == 2 )
                        {
                                if ( AutoRestart.CancelCountdown( sender.Mobile ) )
                                        sender.Mobile.SendMessage( 0x35, "Countdown stopped." );

                                SendTo( sender, m_Message );
                                return;
                        }

                        string message = m_Message;

                        TextRelay textEntry = info.GetTextEntry( MessageEntryID );
                        if ( textEntry != null )
                                message = textEntry.Text.Trim();

                        int minutes = 0;
                        int seconds = 0;

                        TextRelay minutesEntry = info.GetTextEntry( MinutesEntryID );
                        if ( minutesEntry != null && !Int32.TryParse( minutesEntry.Text, out minutes ) )
                                minutes = 0;

                        TextRelay secondsEntry = info.GetTextEntry( SecondsEntryID );
                        if ( secondsEntry != null && !Int32.TryParse( secondsEntry.Text, out seconds ) )
                                seconds = 0;

                        if ( minutes < 0 || seconds < 0 )
                        {
                                sender.Mobile.SendMessage( 33, "Please enter a valid, non-negative time." );
                                SendTo( sender, message );
                                return;
                        }

                        int totalSeconds = (minutes * 60) + seconds;

                        if ( totalSeconds < 1 )
                        {
                                sender.Mobile.SendMessage( 33, "Please enter at least one second before restart." );
                                SendTo( sender, message );
                                return;
                        }

                        TimeSpan delay = TimeSpan.FromSeconds( totalSeconds );
                        bool restart = !info.IsSwitched( ShutdownSwitchID );

                        if ( AutoRestart.StartCountdown( delay, message, restart, sender.Mobile ) )
                                sender.Mobile.SendMessage( 0x35, "Countdown set for {0} ({1}).", FormatTime( totalSeconds ), restart ? "restart" : "shutdown" );

                        SendTo( sender, message );
                }

                public static void RefreshActiveGumps( string message )
                {
                        List<NetState> states;

                        lock ( m_OpenStates )
                        {
                                if ( m_OpenStates.Count == 0 )
                                        return;

                                states = new List<NetState>( m_OpenStates );
                        }

                        foreach ( NetState state in states )
                        {
                                if ( state == null || state.Mobile == null || state.Socket == null || !state.Running )
                                {
                                        Unregister( state );
                                        continue;
                                }

                                SendTo( state, message );
                        }
                }

                public static void CloseActiveGumps()
                {
                        List<NetState> states;

                        lock ( m_OpenStates )
                        {
                                if ( m_OpenStates.Count == 0 )
                                        return;

                                states = new List<NetState>( m_OpenStates );
                                m_OpenStates.Clear();
                        }

                        foreach ( NetState state in states )
                        {
                                if ( state != null && state.Mobile != null )
                                        state.Mobile.CloseGump( typeof( RestartMenuGump ) );
                        }
                }

                private static void Unregister( NetState state )
                {
                        if ( state == null )
                                return;

                        lock ( m_OpenStates )
                        {
                                m_OpenStates.Remove( state );
                        }
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
        }

        public class RestartNotifyGump : Gump
        {
                private static readonly HashSet<NetState> m_OpenStates = new HashSet<NetState>();

                private const string DarkGrey = "#1A1A1A";
                private const string EntryText = "#FFFFFF";

                private readonly string m_Message;
                private readonly bool m_Restart;
                private readonly int m_TotalSeconds;

                public static void SendToAll( string message, TimeSpan remaining, bool restart )
                {
                        foreach ( NetState state in NetState.Instances )
                        {
                                if ( state == null || state.Mobile == null || state.Socket == null || !state.Running )
                                        continue;

                                SendTo( state, message, remaining, restart );
                        }
                }

                public static void SendTo( NetState state, string message, TimeSpan remaining, bool restart )
                {
                        if ( state == null || state.Mobile == null )
                                return;

                        state.Mobile.CloseGump( typeof( RestartNotifyGump ) );
                        state.Mobile.SendGump( new RestartNotifyGump( message, remaining, restart ) );

                        lock ( m_OpenStates )
                        {
                                m_OpenStates.Add( state );
                        }
                }

                public RestartNotifyGump( string message, TimeSpan remaining, bool restart ) : base( 50, 50 )
                {
                        m_Message = AutoRestart.FormatAnnouncement( message ?? String.Empty, remaining );
                        m_Restart = restart;
                        m_TotalSeconds = Math.Max( 0, (int)Math.Ceiling( remaining.TotalSeconds ) );

                        Closable = true;
                        Disposable = true;
                        Dragable = true;
                        Resizable = false;

                        AddPage( 0 );
                        AddBackground( 0, 0, 520, 300, 9270 );
                        AddAlphaRegion( 10, 10, 500, 280 );

                        AddLabel( 30, 25, 0x34, String.Format( "Server {0} Notice", restart ? "Restart" : "Shutdown" ) );

                        AddLabel( 30, 60, 0x481, restart ? "The server is preparing to restart." : "The server is preparing to shut down." );

                        AddLabel( 30, 90, 0x481, "Announcement" );
                        AddHtml( 30, 110, 460, 60, "<body bgcolor='#1A1A1A'></body>", false, false );
                        AddHtml( 40, 122, 440, 36, FormatAnnouncement(), false, false );

                        AddLabel( 30, 170, 0x481, "Time remaining" );
                        AddHtml( 30, 190, 200, 24, "<body bgcolor='#1A1A1A'></body>", false, false );
                        AddHtml( 35, 192, 190, 20, FormatTime( m_TotalSeconds ), false, false );

                        AddButton( 300, 190, 247, 248, 1, GumpButtonType.Reply, 0 );
                        AddLabel( 365, 192, 0x34, "OK" );
                }

                public override void OnResponse( NetState sender, RelayInfo info )
                {
                        Unregister( sender );
                }

                public static void RefreshActiveGumps( string message )
                {
                        List<NetState> states;
                        TimeSpan remaining;
                        bool active = AutoRestart.TryGetRemaining( out remaining );
                        bool restart = AutoRestart.ShouldRestart;

                        lock ( m_OpenStates )
                        {
                                if ( m_OpenStates.Count == 0 )
                                        return;

                                states = new List<NetState>( m_OpenStates );
                        }

                        if ( !active )
                        {
                                foreach ( NetState state in states )
                                {
                                        Unregister( state );
                                }

                                return;
                        }

                        foreach ( NetState state in states )
                        {
                                if ( state == null || state.Mobile == null || state.Socket == null || !state.Running )
                                {
                                        Unregister( state );
                                        continue;
                                }

                                SendTo( state, message, remaining, restart );
                        }
                }

                public static void CloseActiveGumps()
                {
                        List<NetState> states;

                        lock ( m_OpenStates )
                        {
                                if ( m_OpenStates.Count == 0 )
                                        return;

                                states = new List<NetState>( m_OpenStates );
                                m_OpenStates.Clear();
                        }

                        foreach ( NetState state in states )
                        {
                                if ( state != null && state.Mobile != null )
                                        state.Mobile.CloseGump( typeof( RestartNotifyGump ) );
                        }
                }

                private static void Unregister( NetState state )
                {
                        if ( state == null )
                                return;

                        lock ( m_OpenStates )
                        {
                                m_OpenStates.Remove( state );
                        }
                }

                private string FormatAnnouncement()
                {
                        if ( String.IsNullOrWhiteSpace( m_Message ) )
                                return String.Format( "<BASEFONT COLOR={0}>No announcement provided.</BASEFONT>", EntryText );

                        return String.Format( "<BASEFONT COLOR={0}>{1}</BASEFONT>", EntryText, m_Message );
                }

                private static string FormatTime( int totalSeconds )
                {
                        int minutes = totalSeconds / 60;
                        int seconds = totalSeconds % 60;

                        string formatted;

                        if ( minutes > 0 && seconds > 0 )
                                formatted = String.Format( "{0} minute{1} {2} second{3}", minutes, minutes == 1 ? "" : "s", seconds, seconds == 1 ? "" : "s" );
                        else if ( minutes > 0 )
                                formatted = String.Format( "{0} minute{1}", minutes, minutes == 1 ? "" : "s" );
                        else
                                formatted = String.Format( "{0} second{1}", seconds, seconds == 1 ? "" : "s" );

                        return String.Format( "<BASEFONT COLOR={0}>{1}</BASEFONT>", EntryText, formatted );
                }
        }
}
