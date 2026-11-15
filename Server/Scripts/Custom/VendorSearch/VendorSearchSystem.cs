using System;
using Server.Commands;

namespace Server.Custom.VendorSearch
{
    internal sealed class VendorSearchStartupCoordinator
    {
        private bool m_Initialized;

        public bool Initialize( Action initializeSystems, Action startMaintenance )
        {
            if ( m_Initialized )
                return false;

            m_Initialized = true;

            if ( initializeSystems != null )
                initializeSystems();

            if ( startMaintenance != null )
                startMaintenance();

            return true;
        }
    }

    public static class VendorSearchSystem
    {
        private static readonly VendorSearchStartupCoordinator m_Startup =
            new VendorSearchStartupCoordinator();

        public static void Initialize()
        {
            m_Startup.Initialize(
                new Action( InitializeSystems ),
                new Action( VendorSearchMaintenance.StartAfterWorldLoad ) );
        }

        private static void InitializeSystems()
        {
            CommandSystem.Register(
                "VendorSearch",
                AccessLevel.Player,
                new CommandEventHandler( OnPlayerCommand ) );
            CommandSystem.Register(
                "VendorSearchAdmin",
                AccessLevel.GameMaster,
                new CommandEventHandler( OnAdminCommand ) );
            VendorSearchSessionManager.Initialize();
        }

        private static void OnPlayerCommand( CommandEventArgs e )
        {
            if ( e != null )
                OpenNewSearch( e.Mobile, CreateDefaultQuery() );
        }

        private static void OnAdminCommand( CommandEventArgs e )
        {
            if ( e != null )
                VendorSearchAdminGump.Open( e.Mobile );
        }

        public static void OpenNewSearch( Mobile from, VendorSearchQuery query )
        {
            if ( from == null )
                return;

            if ( !VendorSearchIndex.Ready )
            {
                from.SendMessage(
                    "The player vendor search index is still building. Please try again shortly." );
                return;
            }

            VendorSearchSession session;
            string message;

            if ( !VendorSearchSessionManager.TryCreate(
                from,
                query,
                out session,
                out message ) )
            {
                if ( !String.IsNullOrEmpty( message ) )
                    from.SendMessage( message );

                VendorSearchSession current =
                    VendorSearchSessionManager.GetCurrent( from );

                if ( current != null )
                    OpenSession( from, current.Id );

                return;
            }

            OpenSession( from, session.Id );
        }

        public static void OpenVendorSearch( Mobile from, VendorSearchQuery query )
        {
            OpenNewSearch( from, query ?? CreateDefaultQuery() );
        }

        public static void OpenSession( Mobile from, int sessionId )
        {
            if ( from == null )
                return;

            from.CloseGump( typeof( VendorSearchGump ) );
            from.SendGump( new VendorSearchGump( from, sessionId ) );
        }

        public static VendorSearchQuery CreateDefaultQuery()
        {
            return new VendorSearchQuery
            {
                CategoryKey = "all",
                SubcategoryKey = "all",
                Sort = VendorSearchSort.PriceLowToHigh,
                TextMode = VendorSearchTextMode.Contains,
                Page = 0
            };
        }
    }
}
