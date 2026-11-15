using System;
using Server.Commands;
using Server.Items;

namespace Server.Commands
{
        public class DailyTimerCommand
        {
                public static void Initialize()
                {
                        CommandSystem.Register( "dailytimer", AccessLevel.Player, new CommandEventHandler( DailyTimer_OnCommand ) );
                }

                [Usage( "dailytimer" )]
                [Description( "Shows the time remaining until the next hourly and daily resets." )]
                public static void DailyTimer_OnCommand( CommandEventArgs e )
                {
                        DateTime now = DateTime.UtcNow;

                        TimeSpan hourlyRemaining = TaskManager.NextRunTime - now;
                        TimeSpan dailyRemaining = TaskManagerDaily.NextRunTime - now;

                        e.Mobile.SendMessage( "Hourly reset in {0}.", FormatTime( hourlyRemaining ) );
                        e.Mobile.SendMessage( "Daily reset in {0}.", FormatTime( dailyRemaining ) );
                }

                private static string FormatTime( TimeSpan span )
                {
                        if ( span < TimeSpan.Zero )
                        {
                                span = TimeSpan.Zero;
                        }

                        int hours = (int)Math.Floor( span.TotalHours );

                        return String.Format( "{0:D2}:{1:D2}:{2:D2}", hours, span.Minutes, span.Seconds );
                }
        }
}
