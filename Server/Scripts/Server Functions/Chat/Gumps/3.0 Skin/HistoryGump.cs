using System;
using System.Collections;
using System.Text;
using Server;
using Server.Gumps;

namespace Knives.Chat3
{
    public class HistoryGump : GumpPlus
    {
        private static readonly Hashtable s_States = new Hashtable();
        private readonly Channel c_Channel;
        private readonly DisplayState c_State;

        private sealed class DisplayState
        {
            public int Width = 520;
            public int Height = 430;
        }

        public HistoryGump(Mobile m, Channel c)
            : base(m, 70, 60)
        {
            m.CloseGump(typeof(HistoryGump));

            c_Channel = c;

            c_State = (DisplayState)s_States[m];
            if (c_State == null)
            {
                c_State = new DisplayState();
                s_States[m] = c_State;
            }

        }

        protected override void BuildGump()
        {
            int width = Math.Max(420, Math.Min(900, c_State.Width));
            int height = Math.Max(300, Math.Min(700, c_State.Height));
            c_State.Width = width;
            c_State.Height = height;

            const int padding = 12;
            const int titleY = 14;
            int contentX = 20;
            int contentY = 88;
            int contentWidth = width - 40;
            int contentHeight = height - 112;

            AddBackground(0, 0, width, height, 9270);
            AddImageTiled(padding, padding, width - (padding * 2), height - (padding * 2), 2624);
            AddAlphaRegion(padding, padding, width - (padding * 2), height - (padding * 2));

            string title = String.Format("<CENTER><BASEFONT COLOR=#FFFFFF>{0} {1}</BASEFONT></CENTER>", Utility.FixHtml(c_Channel.NameFor(Owner)), General.Local(206));
            AddHtml(0, titleY, width, 24, title, false, false);

            AddButton(22, 46, 4005, 4007, "Refresh", new GumpCallback(Refresh));
            AddHtml(56, 48, 80, 20, "<BASEFONT COLOR=#9DC6FF>Refresh</BASEFONT>", false, false);

            AddButton(148, 46, 4014, 4016, "Smaller", new GumpCallback(Smaller));
            AddHtml(182, 48, 65, 20, "<BASEFONT COLOR=#9DC6FF>Smaller</BASEFONT>", false, false);
            AddButton(252, 46, 4005, 4007, "Larger", new GumpCallback(Larger));
            AddHtml(286, 48, 55, 20, "<BASEFONT COLOR=#9DC6FF>Larger</BASEFONT>", false, false);


            AddHtml(contentX, 64, contentWidth, 20, "<BASEFONT COLOR=#9DC6FF>Most recent public chat messages</BASEFONT>", false, false);

            StringBuilder txt = new StringBuilder();
            ArrayList list = c_Channel.GetHistory(Owner);

            for (int i = list.Count - 1; i >= 0; --i)
            {
                Channel.ChatHistory ch = (Channel.ChatHistory)list[i];
                txt.AppendFormat("<BASEFONT COLOR=#FFFF99>{0}</BASEFONT>: {1}<BR>", Utility.FixHtml(ch.Mobile.RawName), Utility.FixHtml(ch.Txt));
            }

            AddImageTiled(contentX, contentY, contentWidth, contentHeight, 2624);
            AddAlphaRegion(contentX, contentY, contentWidth, contentHeight);
            AddHtml(contentX + 8, contentY + 8, contentWidth - 16, contentHeight - 16, txt.ToString(), false, true);
        }

        private void Refresh()
        {
            NewGump();
        }

        private void Smaller()
        {
            c_State.Width -= 80;
            c_State.Height -= 60;
            NewGump();
        }

        private void Larger()
        {
            c_State.Width += 80;
            c_State.Height += 60;
            NewGump();
        }

        public static void RefreshChannelViewers(Channel channel)
        {
            if (channel == null)
                return;

            foreach (Data data in Data.Datas.Values)
            {
                Mobile mobile = data.Mobile;

                if (mobile == null || mobile.Deleted || mobile.NetState == null)
                    continue;

                DisplayState state = (DisplayState)s_States[mobile];
                if (state == null)
                    continue;

                bool hasOpenHistory = false;
                foreach (Gump g in mobile.NetState.Gumps)
                {
                    HistoryGump history = g as HistoryGump;
                    if (history != null && history.c_Channel == channel)
                    {
                        hasOpenHistory = true;
                        break;
                    }
                }

                if (hasOpenHistory)
                    new HistoryGump(mobile, channel);
            }
        }
    }
}
