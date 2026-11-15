using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Custom.VendorSearch
{
    public static class VendorSearchMaintenance
    {
        private static RebuildTimer m_RebuildTimer;
        private static CleanupTimer m_CleanupTimer;
        private static Timer m_DailyTimer;
        private static bool m_DailyEnabled = true;
        private static DateTime m_LastStarted;
        private static DateTime m_LastCompleted;
        private static string m_LastReason = String.Empty;

        public static bool DailyEnabled { get { return m_DailyEnabled; } }
        public static bool Rebuilding { get { return m_RebuildTimer != null; } }
        public static bool Cleaning { get { return m_CleanupTimer != null; } }

        public static void StartAfterWorldLoad()
        {
            BeginRebuild( "startup" );
            ScheduleDaily( VendorSearchLimits.DailyRebuildInterval );
        }

        public static bool BeginRebuild( string reason )
        {
            if ( Rebuilding || Cleaning || World.Saving )
                return false;

            List<Serial> vendorSerials = new List<Serial>();

            foreach ( Mobile mobile in World.Mobiles.Values )
            {
                if ( mobile is PlayerVendor && !mobile.Deleted )
                    vendorSerials.Add( mobile.Serial );
            }

            VendorSearchIndex.BeginStaging();
            m_LastStarted = DateTime.UtcNow;
            m_LastReason = reason ?? String.Empty;
            Console.WriteLine(
                "Vendor search rebuild started ({0}): {1:N0} player vendors queued.",
                m_LastReason,
                vendorSerials.Count );
            m_RebuildTimer = new RebuildTimer( vendorSerials );
            m_RebuildTimer.Start();
            return true;
        }

        public static void SetDailyEnabled( bool enabled )
        {
            m_DailyEnabled = enabled;
        }

        public static bool BeginCleanup()
        {
            return BeginCleanup( new Func<Serial, bool>( VendorSearchValidation.IsListingStillValid ) );
        }

        internal static void ReplayDirtyVendors( VendorSearchIndexStore staging, IEnumerable<Serial> dirtySerials )
        {
            if ( staging == null || dirtySerials == null )
                return;

            foreach ( Serial vendorSerial in dirtySerials )
            {
                staging.RemoveVendor( vendorSerial );

                if ( World.Mobiles == null )
                    continue;

                PlayerVendor vendor = World.FindMobile( vendorSerial ) as PlayerVendor;

                if ( vendor != null && !vendor.Deleted )
                    VendorSearchIndex.RefreshVendorInto( staging, vendor );
            }
        }

        internal static bool BeginCleanup( Func<Serial, bool> isValid )
        {
            if ( Cleaning || Rebuilding || isValid == null )
                return false;

            VendorSearchRecord[] snapshot = VendorSearchIndex.GetSnapshot();
            List<Serial> serials = new List<Serial>( snapshot.Length );

            for ( int i = 0; i < snapshot.Length; i++ )
                serials.Add( snapshot[i].ItemSerial );

            m_CleanupTimer = new CleanupTimer( serials, isValid );
            m_CleanupTimer.Start();
            return true;
        }

        internal static void PopulateStats( VendorSearchIndexStats stats )
        {
            if ( stats == null )
                return;

            stats.Rebuilding = Rebuilding;
            stats.Cleaning = Cleaning;
            stats.LastRebuildStarted = m_LastStarted;
            stats.LastRebuildCompleted = m_LastCompleted;
            stats.LastRebuildReason = m_LastReason;
        }

        private static void ScheduleDaily( TimeSpan delay )
        {
            if ( m_DailyTimer != null )
                m_DailyTimer.Stop();

            m_DailyTimer = Timer.DelayCall( delay, new TimerCallback( OnDailyTimer ) );
        }

        private static void OnDailyTimer()
        {
            m_DailyTimer = null;

            if ( !m_DailyEnabled )
            {
                ScheduleDaily( VendorSearchLimits.DailyRebuildInterval );
                return;
            }

            if ( World.Saving || Rebuilding || Cleaning )
            {
                ScheduleDaily( TimeSpan.FromMinutes( 10.0 ) );
                return;
            }

            BeginRebuild( "daily" );
            ScheduleDaily( VendorSearchLimits.DailyRebuildInterval );
        }

        private sealed class RebuildTimer : Timer
        {
            private readonly List<Serial> m_VendorSerials;
            private readonly VendorSearchIndexStore m_Staging;
            private int m_Index;
            private bool m_Failed;

            public RebuildTimer( List<Serial> vendorSerials )
                : base( TimeSpan.Zero, TimeSpan.FromMilliseconds( 50.0 ) )
            {
                m_VendorSerials = vendorSerials ?? new List<Serial>();
                m_Staging = new VendorSearchIndexStore();
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                int end = Math.Min(
                    m_Index + VendorSearchLimits.RebuildVendorsPerSlice,
                    m_VendorSerials.Count );

                while ( m_Index < end )
                {
                    Serial vendorSerial = m_VendorSerials[m_Index++];

                    try
                    {
                        PlayerVendor vendor = World.FindMobile( vendorSerial ) as PlayerVendor;

                        if ( vendor != null && !vendor.Deleted )
                            VendorSearchIndex.RefreshVendorInto( m_Staging, vendor );
                    }
                    catch ( Exception ex )
                    {
                        m_Failed = true;
                        Console.WriteLine( "Vendor search rebuild failed for vendor {0}: {1}", vendorSerial, ex );
                    }
                }

                if ( m_Index < m_VendorSerials.Count )
                    return;

                try
                {
                    if ( !m_Failed )
                        ReplayDirtyVendors( m_Staging, VendorSearchIndex.GetDirtyVendorSerials() );

                    if ( !m_Failed )
                    {
                        VendorSearchIndex.ReplaceActiveStore( m_Staging );
                        m_LastCompleted = DateTime.UtcNow;
                        Console.WriteLine(
                            "Vendor search rebuild completed: {0:N0} listings from {1:N0} vendors in {2}.",
                            m_Staging.RecordCount,
                            m_Staging.VendorCount,
                            ( m_LastCompleted - m_LastStarted ).ToString() );
                    }
                    else
                    {
                        Console.WriteLine(
                            "Vendor search rebuild aborted after {0}; the previous index remains active.",
                            ( DateTime.UtcNow - m_LastStarted ).ToString() );
                    }
                }
                catch ( Exception ex )
                {
                    Console.WriteLine(
                        "Vendor search rebuild finalization failed; the previous index remains active: {0}",
                        ex );
                }
                finally
                {
                    VendorSearchIndex.EndStaging();
                    m_RebuildTimer = null;
                    Stop();
                }
            }
        }

        private sealed class CleanupTimer : Timer
        {
            private readonly List<Serial> m_Serials;
            private readonly Func<Serial, bool> m_IsValid;
            private int m_Index;

            public CleanupTimer( List<Serial> serials, Func<Serial, bool> isValid )
                : base( TimeSpan.Zero, TimeSpan.FromMilliseconds( 50.0 ) )
            {
                m_Serials = serials;
                m_IsValid = isValid;
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                int end = Math.Min(
                    m_Index + VendorSearchLimits.CleanupRecordsPerSlice,
                    m_Serials.Count );

                while ( m_Index < end )
                {
                    Serial itemSerial = m_Serials[m_Index++];

                    try
                    {
                        if ( !m_IsValid( itemSerial ) )
                            VendorSearchIndex.RemoveStaleListing( itemSerial );
                    }
                    catch ( Exception ex )
                    {
                        Console.WriteLine( "Vendor search cleanup failed for item {0}: {1}", itemSerial, ex );
                    }
                }

                if ( m_Index >= m_Serials.Count )
                {
                    m_CleanupTimer = null;
                    Stop();
                }
            }
        }
    }
}
