using System;
using System.Globalization;
using System.Text;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Custom.VendorSearch
{
    internal enum VendorSearchAdminTab
    {
        Overview,
        Index,
        Sessions,
        DailyRebuild,
        StaleListings,
        Help
    }

    public sealed class VendorSearchAdminGump : Gump
    {
        private const int BackgroundWidth = 820;
        private const int BackgroundHeight = 620;
        private const int TabButtonBase = 10;
        private const int RebuildButtonID = 100;
        private const int ClearSessionsButtonID = 101;
        private const int CleanupButtonID = 102;
        private const int ToggleDailyButtonID = 103;
        private const int InspectButtonID = 104;

        private static readonly string[] m_TabNames = new string[]
        {
            "Overview",
            "Index",
            "Sessions",
            "Daily",
            "Stale",
            "Help"
        };

        private readonly Mobile m_From;
        private readonly VendorSearchAdminTab m_Tab;
        private readonly string m_Details;

        public static void Open( Mobile from )
        {
            if ( from == null || from.Deleted || from.AccessLevel < AccessLevel.GameMaster )
                return;

            from.CloseGump( typeof( VendorSearchAdminGump ) );
            from.SendGump( new VendorSearchAdminGump( from, VendorSearchAdminTab.Overview, null ) );
        }

        internal static void OpenInspection( Mobile from, PlayerVendor vendor )
        {
            if ( from == null || vendor == null || from.AccessLevel < AccessLevel.GameMaster )
                return;

            StringBuilder text = new StringBuilder();
            VendorSearchRecord[] snapshot = VendorSearchIndex.GetSnapshot();
            int listingCount = 0;

            for ( int i = 0; i < snapshot.Length; i++ )
            {
                if ( snapshot[i].VendorSerial == vendor.Serial )
                    listingCount++;
            }

            text.AppendFormat( "<BASEFONT COLOR=#F7D774>Player Vendor {0}</BASEFONT><br>", vendor.Serial );
            text.AppendFormat( "Name: {0}<br>", Escape( vendor.Name ) );
            text.AppendFormat( "Shop: {0}<br>", Escape( vendor.ShopName ) );
            text.AppendFormat( "Deleted: {0}<br>", vendor.Deleted );
            text.AppendFormat( "Index listings: {0:N0}<br>", listingCount );
            text.AppendFormat( "Vendor valid: {0}<br>", VendorSearchValidation.IsVendorValid( vendor ) );
            text.AppendFormat( "Accessible to staff viewer: {0}<br>", VendorSearchValidation.IsVendorAccessible( from, vendor ) );
            text.AppendFormat( "Placeholder active: {0}<br>", vendor.Placeholder != null );
            text.AppendFormat( "House present: {0}<br>", vendor.House != null );

            if ( vendor.House != null )
            {
                text.AppendFormat( "Inside house: {0}<br>", vendor.House.IsInside( vendor ) );
                text.AppendFormat( "House map matches: {0}<br>", vendor.House.Map == vendor.Map );
            }

            text.AppendFormat(
                "Location: {0} [{1}] {2}, {3}, {4}<br>",
                Escape( vendor.Map == null ? String.Empty : vendor.Map.Name ),
                Escape( vendor.Region == null ? String.Empty : vendor.Region.Name ),
                vendor.X,
                vendor.Y,
                vendor.Z );

            ShowInspection( from, text.ToString() );
        }

        internal static void OpenInspection( Mobile from, Item item )
        {
            if ( from == null || item == null || from.AccessLevel < AccessLevel.GameMaster )
                return;

            StringBuilder text = new StringBuilder();
            VendorSearchRecord record;
            bool indexed = VendorSearchIndex.TryGet( item.Serial, out record );
            PlayerVendor vendor = null;

            if ( indexed )
                vendor = World.FindMobile( record.VendorSerial ) as PlayerVendor;

            if ( vendor == null )
                vendor = item.RootParent as PlayerVendor;

            bool listingValid = vendor != null
                && VendorSearchValidation.IsVendorValid( vendor )
                && VendorSearchValidation.IsListingStillValid( vendor, item );
            VendorItem vendorItem = vendor == null ? null : vendor.GetVendorItem( item );

            text.AppendFormat( "<BASEFONT COLOR=#F7D774>Item {0}</BASEFONT><br>", item.Serial );
            text.AppendFormat( "Name: {0}<br>", Escape( VendorSearchCatalog.ResolveItemName( item ) ) );
            text.AppendFormat( "Type: {0}<br>", Escape( item.GetType().FullName ) );
            text.AppendFormat( "Deleted: {0}<br>", item.Deleted );
            text.AppendFormat( "Indexed: {0}<br>", indexed );
            text.AppendFormat( "Live listing valid: {0}<br>", listingValid );
            text.AppendFormat( "Amount: {0:N0}<br>", Math.Max( 1, item.Amount ) );

            if ( vendorItem != null )
                text.AppendFormat( "Live price: {0:N0}<br>", vendorItem.Price );

            if ( record != null )
            {
                text.AppendFormat( "Indexed vendor serial: {0}<br>", record.VendorSerial );
                text.AppendFormat( "Indexed price: {0:N0}<br>", record.Price );
                text.AppendFormat( "Indexed amount: {0:N0}<br>", record.Amount );
            }

            if ( vendor != null )
            {
                text.AppendFormat( "Vendor: {0} ({1})<br>", Escape( vendor.Name ), vendor.Serial );
                text.AppendFormat( "Vendor valid: {0}<br>", VendorSearchValidation.IsVendorValid( vendor ) );
                text.AppendFormat( "Accessible to staff viewer: {0}<br>", VendorSearchValidation.IsVendorAccessible( from, vendor ) );
                text.AppendFormat(
                    "Location: {0} {1}, {2}, {3}<br>",
                    Escape( vendor.Map == null ? String.Empty : vendor.Map.Name ),
                    vendor.X,
                    vendor.Y,
                    vendor.Z );
            }

            ShowInspection( from, text.ToString() );
        }

        private static void ShowInspection( Mobile from, string details )
        {
            from.CloseGump( typeof( VendorSearchAdminGump ) );
            from.SendGump( new VendorSearchAdminGump( from, VendorSearchAdminTab.Index, details ) );
        }

        internal VendorSearchAdminGump( Mobile from, VendorSearchAdminTab tab, string details )
            : base( 60, 60 )
        {
            m_From = from;
            m_Tab = tab;
            m_Details = details;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            Build();
        }

        private void Build()
        {
            AddPage( 0 );
            AddBackground( 0, 0, BackgroundWidth, BackgroundHeight, 9270 );
            AddImageTiled( 20, 20, BackgroundWidth - 40, BackgroundHeight - 40, 2624 );
            AddAlphaRegion( 20, 20, BackgroundWidth - 40, BackgroundHeight - 40 );
            AddHtml(
                0,
                30,
                BackgroundWidth,
                32,
                "<CENTER><BASEFONT COLOR=#FFFFFF>Player Vendor Search Administration</BASEFONT></CENTER>",
                false,
                false );

            for ( int i = 0; i < m_TabNames.Length; i++ )
            {
                int x = 30 + ( i * 126 );
                bool selected = (int)m_Tab == i;

                AddButton(
                    x,
                    70,
                    selected ? 4006 : 4005,
                    selected ? 4006 : 4007,
                    TabButtonBase + i,
                    GumpButtonType.Reply,
                    0 );
                AddHtml(
                    x + 34,
                    74,
                    88,
                    20,
                    String.Format(
                        "<BASEFONT COLOR={0}>{1}</BASEFONT>",
                        selected ? "#C5FFAE" : "#FFFFFF",
                        m_TabNames[i] ),
                    false,
                    false );
            }

            AddImageTiled( 30, 105, 760, 440, 2624 );

            switch ( m_Tab )
            {
                case VendorSearchAdminTab.Index:
                    DrawIndex();
                    break;
                case VendorSearchAdminTab.Sessions:
                    DrawSessions();
                    break;
                case VendorSearchAdminTab.DailyRebuild:
                    DrawDaily();
                    break;
                case VendorSearchAdminTab.StaleListings:
                    DrawStale();
                    break;
                case VendorSearchAdminTab.Help:
                    DrawHelp();
                    break;
                default:
                    DrawOverview();
                    break;
            }

            if ( m_Tab != VendorSearchAdminTab.Index && !String.IsNullOrEmpty( m_Details ) )
                AddHtml( 55, 505, 710, 34, m_Details, false, false );
        }

        private void DrawOverview()
        {
            VendorSearchIndexStats stats = VendorSearchIndex.GetStats();
            string html = String.Format(
                CultureInfo.InvariantCulture,
                "<BASEFONT COLOR=#F7D774>Current State</BASEFONT><br><br>"
                + "Ready: {0}<br>Rebuilding: {1}<br>Cleaning: {2}<br>"
                + "Indexed records: {3:N0}<br>Indexed vendors: {4:N0}<br>"
                + "Snapshot version: {5:N0}<br>Stale records removed: {6:N0}<br>"
                + "Active sessions: {7:N0}<br>Daily rebuild enabled: {8}<br><br>"
                + "Last rebuild reason: {9}<br>Started: {10}<br>Completed: {11}",
                stats.Ready,
                stats.Rebuilding,
                stats.Cleaning,
                stats.RecordCount,
                stats.VendorCount,
                stats.SnapshotVersion,
                stats.StaleRemoved,
                VendorSearchSessionManager.SessionCount,
                VendorSearchMaintenance.DailyEnabled,
                Escape( stats.LastRebuildReason ),
                FormatDate( stats.LastRebuildStarted ),
                FormatDate( stats.LastRebuildCompleted ) );

            AddHtml( 55, 130, 710, 380, html, false, true );
        }

        private void DrawIndex()
        {
            AddHtml(
                55,
                128,
                710,
                52,
                "<BASEFONT COLOR=#FFFFFF>Rebuild creates a staging index in bounded slices, then swaps it atomically. Inspection is read-only and may be used by Game Masters.</BASEFONT>",
                false,
                false );

            AddActionButton( 55, 190, RebuildButtonID, "Rebuild Index" );
            AddActionButton( 245, 190, InspectButtonID, "Inspect Vendor / Item" );

            if ( !String.IsNullOrEmpty( m_Details ) )
                AddHtml( 55, 235, 710, 285, m_Details, false, true );
        }

        private void DrawSessions()
        {
            AddHtml(
                55,
                130,
                710,
                100,
                String.Format(
                    CultureInfo.InvariantCulture,
                    "<BASEFONT COLOR=#FFFFFF>Active player search sessions: {0:N0}<br><br>Each player holds at most one serial-only snapshot. Sessions expire after 10 minutes of inactivity.</BASEFONT>",
                    VendorSearchSessionManager.SessionCount ),
                false,
                false );
            AddActionButton( 55, 250, ClearSessionsButtonID, "Clear All Sessions" );
        }

        private void DrawDaily()
        {
            AddHtml(
                55,
                130,
                710,
                120,
                "<BASEFONT COLOR=#FFFFFF>Daily rebuild is "
                    + ( VendorSearchMaintenance.DailyEnabled ? "enabled" : "disabled" )
                    + ". The setting is runtime-only and returns to enabled after a server restart. Busy or saving states defer the daily run.</BASEFONT>",
                false,
                false );
            AddActionButton(
                55,
                270,
                ToggleDailyButtonID,
                VendorSearchMaintenance.DailyEnabled ? "Disable Daily Rebuild" : "Enable Daily Rebuild" );
        }

        private void DrawStale()
        {
            AddHtml(
                55,
                130,
                710,
                120,
                "<BASEFONT COLOR=#FFFFFF>Cleanup validates only cached item serials and processes 250 records per slice. It does not scan the world and does not remove listings merely because one viewer is banned from a house.</BASEFONT>",
                false,
                false );
            AddActionButton( 55, 270, CleanupButtonID, "Start Stale Cleanup" );
        }

        private void DrawHelp()
        {
            string html =
                "<BASEFONT COLOR=#F7D774>Player command</BASEFONT><br>"
                + "<BASEFONT COLOR=#FFFFFF>[VendorSearch opens the player-only search tool. Results provide inline details and map creation only; remote buying and backpack access are not available.</BASEFONT><br><br>"
                + "<BASEFONT COLOR=#F7D774>Search behavior</BASEFONT><br>"
                + "<BASEFONT COLOR=#FFFFFF>Players may filter by name, vendor/shop, region, facet, price, per-item price, stack amount, listing date, category, and subcategory. Sorts include price, listing age, vendor, item name, and stack amount.</BASEFONT><br><br>"
                + "<BASEFONT COLOR=#F7D774>Safety limits</BASEFONT><br>"
                + "<BASEFONT COLOR=#FFFFFF>Search has a 1-second cooldown; Load More has a 3-second cooldown. Sessions expire after 10 minutes idle. The first 2,000 results are exposed, with 2,000-result Load More steps and a 100,000-result browsing ceiling.</BASEFONT><br><br>"
                + "<BASEFONT COLOR=#F7D774>Maintenance</BASEFONT><br>"
                + "<BASEFONT COLOR=#FFFFFF>The index rebuilds after world load and daily by default. The daily toggle is runtime-only. Rebuilds are sliced and preserve the previous active index if staging fails.</BASEFONT><br><br>"
                + "<BASEFONT COLOR=#F7D774>Item tooltips</BASEFONT><br>"
                + "<BASEFONT COLOR=#FFFFFF>Stock RunUO clients attach live serial properties to the rendered item art, not to an arbitrary larger hover rectangle. The art is centered in a fixed square, but the genuine tooltip hover area remains the visible art.</BASEFONT>";

            AddHtml( 55, 125, 710, 395, html, false, true );
        }

        private void AddActionButton( int x, int y, int buttonID, string label )
        {
            AddButton( x, y, 4005, 4007, buttonID, GumpButtonType.Reply, 0 );
            AddHtml(
                x + 36,
                y + 4,
                180,
                20,
                "<BASEFONT COLOR=#FFFFFF>" + Escape( label ) + "</BASEFONT>",
                false,
                false );
        }

        public override void OnResponse( NetState sender, RelayInfo info )
        {
            if ( sender == null
                || sender.Mobile != m_From
                || m_From == null
                || m_From.Deleted
                || m_From.AccessLevel < AccessLevel.GameMaster
                || info == null )
                return;

            int buttonID = info.ButtonID;

            if ( buttonID == 0 )
                return;

            if ( buttonID >= TabButtonBase
                && buttonID < TabButtonBase + m_TabNames.Length )
            {
                Reopen( (VendorSearchAdminTab)( buttonID - TabButtonBase ), null );
                return;
            }

            switch ( buttonID )
            {
                case RebuildButtonID:
                    if ( !RequireAdministrator() )
                        return;

                    Reopen(
                        VendorSearchMaintenance.BeginRebuild( "administrator" )
                            ? "Vendor search rebuild started."
                            : "A rebuild or cleanup is already running, or the world is saving." );
                    return;
                case ClearSessionsButtonID:
                    if ( !RequireAdministrator() )
                        return;

                    VendorSearchSessionManager.ClearAll();
                    Reopen( "All vendor search sessions were cleared." );
                    return;
                case CleanupButtonID:
                    if ( !RequireAdministrator() )
                        return;

                    Reopen(
                        VendorSearchMaintenance.BeginCleanup()
                            ? "Stale-listing cleanup started."
                            : "A cleanup or rebuild is already running." );
                    return;
                case ToggleDailyButtonID:
                    if ( !RequireAdministrator() )
                        return;

                    VendorSearchMaintenance.SetDailyEnabled( !VendorSearchMaintenance.DailyEnabled );
                    Reopen(
                        "Daily rebuild is now "
                            + ( VendorSearchMaintenance.DailyEnabled ? "enabled." : "disabled." ) );
                    return;
                case InspectButtonID:
                    m_From.Target = new VendorSearchInspectTarget( m_From );
                    m_From.SendMessage( "Target a player vendor or one of its listed items." );
                    return;
                default:
                    Reopen( null );
                    return;
            }
        }

        private bool RequireAdministrator()
        {
            if ( m_From.AccessLevel >= AccessLevel.Administrator )
                return true;

            Reopen( "Administrator access is required for that action." );
            return false;
        }

        private void Reopen( string message )
        {
            string details = String.IsNullOrEmpty( message )
                ? m_Details
                : "<BASEFONT COLOR=#FFCC66>" + Escape( message ) + "</BASEFONT>";
            Reopen( m_Tab, details );
        }

        private void Reopen( VendorSearchAdminTab tab, string details )
        {
            m_From.CloseGump( typeof( VendorSearchAdminGump ) );
            m_From.SendGump( new VendorSearchAdminGump( m_From, tab, details ) );
        }

        private static string Escape( string value )
        {
            return Utility.FixHtml( value ?? String.Empty );
        }

        private static string FormatDate( DateTime value )
        {
            return value == DateTime.MinValue
                ? "Never"
                : value.ToString( "u", CultureInfo.InvariantCulture );
        }

        private sealed class VendorSearchInspectTarget : Target
        {
            private readonly Mobile m_From;

            public VendorSearchInspectTarget( Mobile from )
                : base( -1, false, TargetFlags.None )
            {
                m_From = from;
            }

            protected override void OnTarget( Mobile from, object targeted )
            {
                if ( from != m_From
                    || m_From == null
                    || m_From.Deleted
                    || m_From.AccessLevel < AccessLevel.GameMaster )
                    return;

                PlayerVendor vendor = targeted as PlayerVendor;
                Item item = targeted as Item;

                if ( vendor != null )
                {
                    OpenInspection( m_From, vendor );
                    return;
                }

                if ( item != null )
                {
                    OpenInspection( m_From, item );
                    return;
                }

                m_From.SendMessage( "Target a player vendor or one of its listed items." );
                m_From.SendGump(
                    new VendorSearchAdminGump(
                        m_From,
                        VendorSearchAdminTab.Index,
                        null ) );
            }
        }
    }
}
