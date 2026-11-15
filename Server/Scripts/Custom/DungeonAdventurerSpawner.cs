using System;
using System.Collections.Generic;
using Server;
using Server.Misc;
using Server.Mobiles;
using Server.Regions;
using Server.Spells;

namespace Server.Custom
{
    public static class DungeonAdventurerSpawner
    {
        private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes( 5.0 );
        private const int MaxAdventurersPerDungeon = 5;
        private const int MaxTotalAdventurers = 500;
        private const int RegionsPerSweep = 40;
        private const double SpawnChancePerRegion = 0.40;
        private const int SpawnHeightAllowance = 20;

        private static readonly List<Region> m_DungeonRegions = new List<Region>();
        private static readonly string[] m_AmbientDialogue =
        {
            "Stay alert down here!",
            "I swear I heard something...",
            "Another day, another dungeon."
        };
        private static Timer m_Timer;

        public static void Initialize()
        {
            if ( m_Timer != null )
                return;

            RebuildDungeonRegionCache();

            m_Timer = new InternalTimer();
            m_Timer.Start();

            PerformSweep();
        }

        private static void PerformSweep()
        {
            RebuildDungeonRegionCache();

            if ( m_DungeonRegions.Count == 0 )
                return;

            int total = CountAllAdventurers();

            if ( total >= MaxTotalAdventurers )
                return;

            int startIndex = Utility.Random( m_DungeonRegions.Count );
            int regionsToProcess = Math.Min( RegionsPerSweep, m_DungeonRegions.Count );

            for ( int i = 0; i < regionsToProcess && total < MaxTotalAdventurers; ++i )
            {
                Region region = m_DungeonRegions[(startIndex + i) % m_DungeonRegions.Count];

                if ( region == null || region.Map == null || region.Map == Map.Internal )
                    continue;

                if ( Utility.RandomDouble() > SpawnChancePerRegion )
                    continue;

                int existing = CountAdventurersInRegion( region );

                if ( existing >= MaxAdventurersPerDungeon )
                    continue;

                if ( TrySpawnAdventurer( region ) )
                    total++;
            }
        }

        private static void RebuildDungeonRegionCache()
        {
            m_DungeonRegions.Clear();

            foreach ( Region region in Region.Regions )
            {
                if ( region == null || region.Map == null || region.Map == Map.Internal )
                    continue;

                if ( IsDungeonRegion( region ) )
                    m_DungeonRegions.Add( region );
            }
        }

        private static bool IsDungeonRegion( Region region )
        {
            return region is DungeonRegion || region is BardDungeonRegion;
        }

        private static int CountAllAdventurers()
        {
            int count = 0;

            foreach ( Mobile mobile in World.Mobiles.Values )
            {
                Adventurers adventurer = mobile as Adventurers;

                if ( adventurer != null && !adventurer.Deleted && adventurer.Alive && adventurer.Map != null && adventurer.Map != Map.Internal )
                    count++;
            }

            return count;
        }

        private static int CountAdventurersInRegion( Region region )
        {
            int count = 0;

            List<Mobile> mobiles = region.GetMobiles();

            for ( int i = 0; i < mobiles.Count; ++i )
            {
                Adventurers adventurer = mobiles[i] as Adventurers;

                if ( adventurer != null && !adventurer.Deleted && adventurer.Alive )
                    count++;
            }

            return count;
        }

        private static bool TrySpawnAdventurer( Region region )
        {
            if ( region == null || region.Map == null )
                return false;

            BaseRegion baseRegion = region as BaseRegion;

            if ( baseRegion == null )
                return false;

            Point3D random = baseRegion.RandomSpawnLocation( SpawnHeightAllowance, true, false, Point3D.Zero, 0 );

            if ( random == Point3D.Zero )
                return false;

            Point3D location = random;

            if ( !SpellHelper.FindValidSpawnLocation( region.Map, ref location, true ) )
                return false;

            Adventurers adventurer = new Adventurers();
            adventurer.MoveToWorld( location, region.Map );
            adventurer.Home = location;
            adventurer.RangeHome = 12;

            if ( Utility.RandomDouble() < 0.25 && m_AmbientDialogue.Length > 0 )
            {
                adventurer.Say( m_AmbientDialogue[Utility.Random( m_AmbientDialogue.Length )] );
            }

            return true;
        }

        private sealed class InternalTimer : Timer
        {
            public InternalTimer() : base( TimeSpan.FromMinutes( 1.0 ), SweepInterval )
            {
                Priority = TimerPriority.OneMinute;
            }

            protected override void OnTick()
            {
                PerformSweep();
            }
        }
    }
}
