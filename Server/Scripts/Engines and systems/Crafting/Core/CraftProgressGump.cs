using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Items;

namespace Server.Engines.Craft
{
    public class CraftProgressGump : Gump
    {
        private readonly Mobile m_From;
        private readonly CraftSystem m_System;
        private readonly CraftItem m_Item;
        private readonly BaseTool m_Tool;

        private const int StartButton = 1;
        private const int PauseButton = 2;
        private const int StopButton = 3;

        public CraftProgressGump( Mobile from, CraftSystem system, CraftItem item, BaseTool tool ) : base( 200, 200 )
        {
            m_From = from;
            m_System = system;
            m_Item = item;
            m_Tool = ( tool != null && !tool.Deleted ) ? tool : null;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage( 0 );
            AddBackground( 0, 0, 320, 240, 5054 );
            AddImageTiled( 10, 10, 300, 20, 2624 );
            AddAlphaRegion( 10, 10, 300, 220 );

            CraftItem.CustomCraftStatus status = new CraftItem.CustomCraftStatus();
            bool hasStatus = ( item != null ) && CraftItem.TryGetCustomCraftStatus( from, item, out status );

            if ( item != null )
            {
                if ( item.NameNumber > 0 )
                    AddHtmlLocalized( 20, 14, 270, 18, item.NameNumber, 0xFFFFFF, false, false );
                else
                    AddLabelCropped( 20, 14, 270, 18, 0xFFFFFF, ResolveTitle( item ) );
            }
            else
            {
                AddLabelCropped( 20, 14, 270, 18, 0x7FFF, "Crafting Progress" );
            }

            if ( !hasStatus )
            {
                AddLabel( 20, 60, 0x480, "No custom crafting queue is active." );
                return;
            }

            int crafted = Math.Max( status.Total - status.Remaining, 0 );

            AddStat( 20, 50, "Total", status.Total > 0 ? status.Total.ToString() : "-" );
            AddStat( 20, 70, "Crafted", crafted.ToString() );
            AddStat( 20, 90, "Succeeded", status.Successes.ToString() );
            AddStat( 20, 110, "Failed", status.Failures.ToString() );
            AddStat( 20, 130, "Exceptional", status.Exceptional.ToString() );

            string state = GetStateLabel( status );
            AddLabel( 20, 150, 0x21, string.Format( "Status: {0}", state ) );

            if ( status.StopReason != null && status.StopReason.Length > 0 )
            {
                AddLabelCropped( 20, 170, 280, 18, 0x21, status.StopReason );
            }

            AddButton( 20, 200, 4005, 4007, StartButton, GumpButtonType.Reply, 0 );
            AddLabel( 55, 200, 0x34, "Start" );
            AddButton( 120, 200, 4005, 4007, PauseButton, GumpButtonType.Reply, 0 );
            AddLabel( 155, 200, 0x34, "Pause" );
            AddButton( 220, 200, 4017, 4019, StopButton, GumpButtonType.Reply, 0 );
            AddLabel( 255, 200, 0x21, "Stop" );
        }

        private void AddStat( int x, int y, string label, string value )
        {
            AddLabel( x, y, 0x480, label + ":" );
            AddLabel( x + 140, y, 0x34, value );
        }

        private string GetStateLabel( CraftItem.CustomCraftStatus status )
        {
            if ( status.Completed && status.Successes + status.Failures > 0 )
                return "Completed";

            if ( status.Stopped && status.Remaining <= 0 )
                return "Stopped";

            if ( status.Paused )
                return "Paused";

            if ( status.IsCrafting )
                return "In Progress";

            return status.Active ? "Ready" : "Idle";
        }

        private string ResolveTitle( CraftItem item )
        {
            if ( item == null )
                return "Crafting Progress";

            if ( !string.IsNullOrEmpty( item.NameString ) )
                return item.NameString;

            if ( item.NameNumber > 0 )
                return string.Format( "#{0}", item.NameNumber );

            return item.ItemType != null ? item.ItemType.Name : "Crafting Progress";
        }

        public override void OnResponse( NetState sender, RelayInfo info )
        {
            Mobile from = sender != null ? sender.Mobile : null;

            if ( from == null || m_Item == null )
                return;

            int buttonID = info != null ? info.ButtonID : 0;

            switch ( buttonID )
            {
                case StartButton:
                    CraftItem.ResumeCustomCraft( from, m_System, m_Item, m_Tool );
                    break;
                case PauseButton:
                    CraftItem.SetCustomCraftPaused( from, m_Item, true );
                    break;
                case StopButton:
                    CraftItem.StopCustomCraft( from, m_Item );
                    break;
            }

            if ( buttonID != 0 )
                Show( from, m_System, m_Item, m_Tool, true );
        }

        public static void Show( Mobile from, CraftSystem system, CraftItem item, BaseTool tool, bool forceOpen )
        {
            if ( from == null || item == null )
                return;

            if ( !forceOpen && !from.HasGump( typeof( CraftProgressGump ) ) )
                return;

            from.CloseGump( typeof( CraftProgressGump ) );
            from.SendGump( new CraftProgressGump( from, system, item, tool ) );
        }

        public static void Close( Mobile from )
        {
            if ( from != null )
                from.CloseGump( typeof( CraftProgressGump ) );
        }
    }
}
