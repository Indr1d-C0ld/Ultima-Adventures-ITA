using System;
using System.Collections;
using Server;

namespace Knives.Chat3
{
    public class Public : Channel
    {
        public Public() : base("Public")
        {
            Commands.Add("chat");
            Commands.Add("c");
            Commands.Add("cshow");
            NewChars = true;

            Register(this);
        }

        public override void UpdateHistory(Mobile m)
        {
            while (History.Count > 100)
                History.RemoveAt(0);
        }
    }
}