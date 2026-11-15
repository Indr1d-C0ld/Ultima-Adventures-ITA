using System;
using Server;
using Server.Commands;
using Server.Mobiles;

namespace Server.Commands
{
    public class RollCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("roll", AccessLevel.Player, new CommandEventHandler(OnRoll));
        }

        private static void OnRoll(CommandEventArgs e)
        {
            int min = 0;
            int max = 100;

            string argument = e.ArgString.Trim();

            if (!string.IsNullOrEmpty(argument))
            {
                string[] range = argument.Split('-');

                if (range.Length != 2 || !int.TryParse(range[0], out min) || !int.TryParse(range[1], out max))
                {
                    e.Mobile.SendMessage("Usage: [roll or [roll <min>-<max>");
                    return;
                }
            }

            if (min > max)
            {
                int swap = min;
                min = max;
                max = swap;
            }

            int result = Utility.RandomMinMax(min, max);

            e.Mobile.Emote("*rolls a {0} ({1}-{2})*", result, min, max);
        }
    }
}
