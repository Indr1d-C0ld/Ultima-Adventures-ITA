using System;
using System.Collections.Generic;
using System.Text;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Help
{
        public class BugReportCommand
        {
                public static void Initialize()
                {
                        CommandSystem.Register( "bug", AccessLevel.Player, new CommandEventHandler( Bug_OnCommand ) );
                }

                [Usage( "bug" )]
                [Description( "Opens the bug report form." )]
                private static void Bug_OnCommand( CommandEventArgs e )
                {
                        e.Mobile.SendGump( new BugReportGump( e.Mobile ) );
                }
        }

        public class BugReportGump : Gump
        {
                private const int Background = 9270;
                private const int InputHue = 1152;

                private Mobile m_From;

                public BugReportGump( Mobile from ) : base( 50, 50 )
                {
                        m_From = from;

                        from.CloseGump( typeof( BugReportGump ) );

                        AddPage( 0 );
                        AddBackground( 0, 0, 500, 360, Background );

                        AddLabel( 20, 20, InputHue, "Bug report" );
                        AddLabel( 20, 50, InputHue, "Describe what happened (required):" );
                        AddBackground( 20, 70, 460, 120, 3000 );
                        AddTextEntry( 30, 80, 440, 100, InputHue, 1, "" );

                        AddLabel( 20, 200, InputHue, "Screenshot link (optional):" );
                        AddBackground( 20, 220, 460, 24, 3000 );
                        AddTextEntry( 24, 222, 452, 20, InputHue, 2, "" );

                        AddLabel( 20, 252, InputHue, "Your current location will be included automatically." );

                        AddCheck( 24, 278, 210, 211, false, 3 );
                        AddLabel( 45, 278, InputHue, "Include recent journal (speech)" );

                        AddCheck( 24, 303, 210, 211, false, 4 );
                        AddLabel( 45, 303, InputHue, "I consent to share this report with staff" );

                        AddButton( 320, 320, 4005, 4007, 1, GumpButtonType.Reply, 0 );
                        AddLabel( 355, 320, InputHue, "Submit" );

                        AddButton( 120, 320, 4017, 4019, 0, GumpButtonType.Reply, 0 );
                        AddLabel( 155, 320, InputHue, "Cancel" );
                }

                public override void OnResponse( NetState sender, RelayInfo info )
                {
                        if ( info.ButtonID == 0 )
                                return;

                        TextRelay descriptionEntry = info.GetTextEntry( 1 );
                        string description = descriptionEntry != null ? descriptionEntry.Text.Trim() : String.Empty;

                        if ( description.Length < 5 )
                        {
                                m_From.SendMessage( 0x22, "Please provide a short description of the bug." );
                                m_From.SendGump( new BugReportGump( m_From ) );
                                return;
                        }

                        if ( !info.IsSwitched( 4 ) )
                        {
                                m_From.SendMessage( 0x22, "You must consent before we can forward this to staff." );
                                m_From.SendGump( new BugReportGump( m_From ) );
                                return;
                        }

                        TextRelay screenshotEntry = info.GetTextEntry( 2 );
                        string screenshot = screenshotEntry != null ? screenshotEntry.Text.Trim() : String.Empty;

                        bool includeSpeechLog = info.IsSwitched( 3 );

                        List<SpeechLogEntry> speechLog = includeSpeechLog ? GetRecentSpeech( m_From as PlayerMobile ) : null;

                        string compiled = BuildMessage( m_From, description, screenshot, includeSpeechLog, speechLog );

                        PageQueue.Enqueue( new PageEntry( m_From, compiled, PageType.Bug, speechLog, true ) );

                        m_From.SendMessage( 0x35, "Thank you. Your bug report has been sent to the staff queue." );
                }

                private static string BuildMessage( Mobile from, string description, string screenshot, bool includeSpeechLog, List<SpeechLogEntry> speechLog )
                {
                        StringBuilder builder = new StringBuilder();

                        builder.Append( "<b>Bug report submitted via [bug]</b><br><br>" );
                        builder.Append( "<u>Description:</u><br>" );
                        builder.Append( Utility.FixHtml( description ) );
                        builder.Append( "<br><br>" );

                        builder.Append( "<u>Location:</u><br>" );
                        builder.Append( Utility.FixHtml( FormatLocation( from ) ) );
                        builder.Append( "<br><br>" );

                        if ( screenshot.Length > 0 )
                        {
                                string safeScreenshot = Utility.FixHtml( screenshot );
                                builder.Append( "<u>Screenshot:</u><br>" );
                                builder.AppendFormat( "<a href=\"{0}\" target=\"_blank\">{0}</a><br><br>", safeScreenshot );
                        }

                        builder.Append( "<u>Journal snippets:</u><br>" );

                        if ( includeSpeechLog && speechLog != null && speechLog.Count > 0 )
                        {
                                for ( int i = 0; i < speechLog.Count; ++i )
                                {
                                        SpeechLogEntry entry = speechLog[i];
                                        string fromName = entry.From != null ? Utility.FixHtml( entry.From.Name ) : "Unknown";
                                        string speech = Utility.FixHtml( entry.Speech );

                                        builder.AppendFormat( "[{0}] {1}: {2}<br>", entry.Created.ToShortTimeString(), fromName, speech );
                                }
                        }
                        else if ( includeSpeechLog )
                        {
                                builder.Append( "Requested, but no recent speech was available." );
                        }
                        else
                        {
                                builder.Append( "Player did not include journal information." );
                        }

                        return builder.ToString();
                }

                private static string FormatLocation( Mobile from )
                {
                        if ( from == null )
                                return "Unknown";

                        string map = ( from.Map != null && from.Map != Map.Internal ) ? from.Map.Name : "Unknown map";
                        string region = from.Region != null ? from.Region.Name : "No region";

                        return String.Format( "{0} ({1}, {2}, {3}) in {4}", region, from.X, from.Y, from.Z, map );
                }

                private static List<SpeechLogEntry> GetRecentSpeech( PlayerMobile player )
                {
                        if ( player == null || player.SpeechLog == null )
                                return null;

                        List<SpeechLogEntry> entries = new List<SpeechLogEntry>();

                        foreach ( SpeechLogEntry entry in player.SpeechLog )
                                entries.Add( entry );

                        int startIndex = entries.Count > 10 ? entries.Count - 10 : 0;
                        return entries.GetRange( startIndex, entries.Count - startIndex );
                }
        }
}
