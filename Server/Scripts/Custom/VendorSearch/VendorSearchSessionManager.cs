using System;
using System.Collections.Generic;

namespace Server.Custom.VendorSearch
{
    internal interface IVendorSearchClock
    {
        DateTime UtcNow { get; }
    }

    internal sealed class VendorSearchSystemClock : IVendorSearchClock
    {
        public DateTime UtcNow { get { return DateTime.UtcNow; } }
    }

    internal sealed class VendorSearchThrottle
    {
        public DateTime NextSearch;
        public DateTime NextLoadMore;
        public DateTime NextMap;
    }

    internal sealed class VendorSearchSessionStore
    {
        private readonly Dictionary<Serial, VendorSearchSession> m_Sessions =
            new Dictionary<Serial, VendorSearchSession>();
        private readonly Dictionary<Serial, VendorSearchThrottle> m_Throttles =
            new Dictionary<Serial, VendorSearchThrottle>();
        private readonly IVendorSearchClock m_Clock;
        private int m_NextSessionId;

        public VendorSearchSessionStore( IVendorSearchClock clock )
        {
            m_Clock = clock ?? new VendorSearchSystemClock();
        }

        public int Count { get { return m_Sessions.Count; } }
        public DateTime UtcNow { get { return m_Clock.UtcNow; } }

        public bool TryBeginSearch( Serial playerSerial, out string message )
        {
            VendorSearchThrottle throttle = GetThrottle( playerSerial );

            if ( UtcNow < throttle.NextSearch )
            {
                message = "Please wait a moment before running another vendor search.";
                return false;
            }

            throttle.NextSearch = UtcNow + VendorSearchLimits.SearchCooldown;
            message = String.Empty;
            return true;
        }

        public VendorSearchSession Set( Serial playerSerial, VendorSearchQuery query, VendorSearchExecutionResult result )
        {
            if ( query == null )
                query = new VendorSearchQuery();

            if ( result == null )
                result = new VendorSearchExecutionResult(
                    new List<Serial>(),
                    new VendorSearchSummary(),
                    false );

            VendorSearchSession session = new VendorSearchSession(
                NextSessionId(),
                playerSerial,
                query.Clone(),
                result.ItemSerials,
                result.Summary,
                result.Truncated,
                UtcNow );

            m_Sessions[playerSerial] = session;
            return session;
        }

        public bool TryGet( Serial playerSerial, int sessionId, out VendorSearchSession session )
        {
            if ( !m_Sessions.TryGetValue( playerSerial, out session ) || session.Id != sessionId )
            {
                session = null;
                return false;
            }

            if ( session.LastAccess < UtcNow - VendorSearchLimits.SessionIdleTimeout )
            {
                m_Sessions.Remove( playerSerial );
                m_Throttles.Remove( playerSerial );
                session = null;
                return false;
            }

            session.Touch( UtcNow );
            return true;
        }

        public VendorSearchSession GetCurrent( Serial playerSerial )
        {
            VendorSearchSession session;

            if ( !m_Sessions.TryGetValue( playerSerial, out session ) )
                return null;

            return TryGet( playerSerial, session.Id, out session ) ? session : null;
        }

        public bool TryLoadMore( Serial playerSerial, int sessionId, out string message )
        {
            VendorSearchSession session;

            if ( !TryGet( playerSerial, sessionId, out session ) )
            {
                message = "The vendor search session has expired.";
                return false;
            }

            VendorSearchThrottle throttle = GetThrottle( playerSerial );

            if ( UtcNow < throttle.NextLoadMore )
            {
                message = "Please wait a moment before loading more results.";
                return false;
            }

            throttle.NextLoadMore = UtcNow + VendorSearchLimits.LoadMoreCooldown;
            message = String.Empty;
            return session.ExposeMore( VendorSearchLimits.LoadMoreIncrement );
        }

        public bool TryBeginMap( Serial playerSerial, out string message )
        {
            VendorSearchThrottle throttle = GetThrottle( playerSerial );

            if ( UtcNow < throttle.NextMap )
            {
                message = "Please wait a moment before requesting another vendor map.";
                return false;
            }

            throttle.NextMap = UtcNow + VendorSearchLimits.MapActionCooldown;
            message = String.Empty;
            return true;
        }

        public void RemoveResult( Serial playerSerial, int sessionId, Serial itemSerial )
        {
            VendorSearchSession session;

            if ( TryGet( playerSerial, sessionId, out session ) )
                session.Remove( itemSerial );
        }

        public void Remove( Serial playerSerial )
        {
            m_Sessions.Remove( playerSerial );
            m_Throttles.Remove( playerSerial );
        }

        public void Clear()
        {
            m_Sessions.Clear();
            m_Throttles.Clear();
        }

        public int SweepExpired()
        {
            DateTime cutoff = UtcNow - VendorSearchLimits.SessionIdleTimeout;
            List<Serial> expired = new List<Serial>();

            foreach ( KeyValuePair<Serial, VendorSearchSession> pair in m_Sessions )
            {
                if ( pair.Value.LastAccess < cutoff )
                    expired.Add( pair.Key );
            }

            for ( int i = 0; i < expired.Count; i++ )
            {
                m_Sessions.Remove( expired[i] );
                m_Throttles.Remove( expired[i] );
            }

            return expired.Count;
        }

        private VendorSearchThrottle GetThrottle( Serial playerSerial )
        {
            VendorSearchThrottle throttle;

            if ( !m_Throttles.TryGetValue( playerSerial, out throttle ) )
            {
                throttle = new VendorSearchThrottle();
                m_Throttles[playerSerial] = throttle;
            }

            return throttle;
        }

        private int NextSessionId()
        {
            if ( m_NextSessionId == Int32.MaxValue )
                m_NextSessionId = 0;

            return ++m_NextSessionId;
        }
    }

    public static class VendorSearchSessionManager
    {
        private static readonly VendorSearchSessionStore m_State =
            new VendorSearchSessionStore( new VendorSearchSystemClock() );
        private static Timer m_SweepTimer;
        private static bool m_Initialized;

        public static int SessionCount { get { return m_State.Count; } }

        public static void Initialize()
        {
            if ( m_Initialized )
                return;

            m_Initialized = true;
            EventSink.Logout += new LogoutEventHandler( OnLogout );
            EventSink.Disconnected += new DisconnectedEventHandler( OnDisconnected );
            m_SweepTimer = Timer.DelayCall(
                VendorSearchLimits.SessionSweepInterval,
                VendorSearchLimits.SessionSweepInterval,
                new TimerCallback( SweepExpiredCallback ) );
        }

        public static bool TryCreate( Mobile from, VendorSearchQuery query, out VendorSearchSession session, out string message )
        {
            session = null;
            message = String.Empty;

            if ( from == null || from.Deleted )
                return false;

            if ( query == null )
                query = new VendorSearchQuery();

            if ( !m_State.TryBeginSearch( from.Serial, out message ) )
                return false;

            VendorSearchExecutionResult result;
            VendorSearchViewerGate gate = new VendorSearchViewerGate( from );

            try
            {
                result = VendorSearchQueryEngine.Execute(
                    VendorSearchIndex.GetSnapshot(),
                    query,
                    gate );
                gate.RemoveDiscoveredStaleListings();
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "Vendor search failed for {0}: {1}", from.Serial, ex );
                message = "The vendor search could not be completed. Please try again.";
                return false;
            }

            session = m_State.Set( from.Serial, query, result );
            return true;
        }

        public static bool TryGet( Mobile from, int sessionId, out VendorSearchSession session )
        {
            session = null;
            return from != null
                && !from.Deleted
                && m_State.TryGet( from.Serial, sessionId, out session );
        }

        public static VendorSearchSession GetCurrent( Mobile from )
        {
            return from == null || from.Deleted ? null : m_State.GetCurrent( from.Serial );
        }

        public static bool TryLoadMore( Mobile from, int sessionId, out string message )
        {
            if ( from == null || from.Deleted )
            {
                message = String.Empty;
                return false;
            }

            return m_State.TryLoadMore( from.Serial, sessionId, out message );
        }

        public static bool TryBeginMap( Mobile from, out string message )
        {
            if ( from == null || from.Deleted )
            {
                message = String.Empty;
                return false;
            }

            return m_State.TryBeginMap( from.Serial, out message );
        }

        public static void RemoveResult( Mobile from, int sessionId, Serial itemSerial )
        {
            if ( from != null && !from.Deleted )
                m_State.RemoveResult( from.Serial, sessionId, itemSerial );
        }

        public static void Remove( Serial playerSerial )
        {
            m_State.Remove( playerSerial );
        }

        public static void ClearAll()
        {
            m_State.Clear();
        }

        public static int SweepExpired()
        {
            return m_State.SweepExpired();
        }

        private static void SweepExpiredCallback()
        {
            m_State.SweepExpired();
        }

        private static void OnLogout( LogoutEventArgs e )
        {
            if ( e != null && e.Mobile != null )
                m_State.Remove( e.Mobile.Serial );
        }

        private static void OnDisconnected( DisconnectedEventArgs e )
        {
            if ( e != null && e.Mobile != null )
                m_State.Remove( e.Mobile.Serial );
        }
    }
}
