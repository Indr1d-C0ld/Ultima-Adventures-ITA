using System;
using System.Collections.Generic;
using Server;
using Server.Network;
using Server.Commands;
using Server.Items;

namespace Server
{
	public class LightCycle
	{
		private static Dictionary<Serial, NightSightTimer> m_NightSightTimers = new Dictionary<Serial, NightSightTimer>();

                public const int DayLevel = 0;
                public const int NightLevel = 26;
                public const int DungeonLevel = 28;
                public const int CaveLevel = 28;
                public const int JailLevel = 20;

                private static int m_LevelOverride = int.MinValue;

                public static int LevelOverride
                {
                        get{ return m_LevelOverride; }
                        set
                        {
                                m_LevelOverride = value;

                                RefreshAllLights();
                        }
                }

		public static void Initialize()
		{
                        new LightCycleTimer().Start();
                        EventSink.Login += new LoginEventHandler( OnLogin );

                        CommandSystem.Register( "GlobalLight", AccessLevel.GameMaster, new CommandEventHandler( Light_OnCommand ) );
                        CommandSystem.Register( "SetTime", AccessLevel.Administrator, new CommandEventHandler( SetTime_OnCommand ) );
                }

		[Usage( "GlobalLight <value>" )]
		[Description( "Sets the current global light level." )]
                private static void Light_OnCommand( CommandEventArgs e )
                {
                        if ( e.Length >= 1 )
                        {
                                LevelOverride = e.GetInt32( 0 );
				e.Mobile.SendMessage( "Global light level override has been changed to {0}.", m_LevelOverride );
			}
			else
			{
                                LevelOverride = int.MinValue;
                                e.Mobile.SendMessage( "Global light level override has been cleared." );
                        }
                }

                [Usage( "SetTime <hour> [minute]" )]
                [Description( "Sets the shard's time of day using a 24-hour clock." )]
                private static void SetTime_OnCommand( CommandEventArgs e )
                {
                        if ( e.Length < 1 )
                        {
                                e.Mobile.SendMessage( "Format: SetTime <hour> [minute]" );
                                return;
                        }

                        int normalizedHours, normalizedMinutes;

                        Clock.SetTimeOfDay( e.Mobile, e.GetInt32( 0 ), e.Length > 1 ? e.GetInt32( 1 ) : 0, out normalizedHours, out normalizedMinutes );

                        RefreshAllLights();

                        e.Mobile.SendMessage( "Time set to {0:D2}:{1:D2} shard-wide.", normalizedHours, normalizedMinutes );
                }

		public static void OnLogin( LoginEventArgs args )
		{
			Mobile m = args.Mobile;

			m.CheckLightLevels( true );
		}

                public static int ComputeLevelFor( Mobile from )
                {
                        if ( m_LevelOverride > int.MinValue )
                                return m_LevelOverride;

                        Map map = from.Map;

                        if ( map == null )
                                map = Map.Trammel;

                        if ( Server.Misc.Worlds.GetRegionName( map, from.Location ) == "the Underworld" )
                                return DungeonLevel;

                        int hours, minutes;

                        Server.Items.Clock.GetTime( map, from.X, from.Y, out hours, out minutes );

                        return ComputeLevelForTime( hours, minutes );
                }

                public static int ComputeLevelForTime( int hours, int minutes )
                {
                        /*
                         * Gradually transition light using a full-day cosine curve so dawn/dusk blend naturally.
                         * Brightness is 0 at midnight, 1 at noon, then mapped into light levels where lower is brighter.
                         *
                         * Nights hold at maximum darkness from 12:00 AM through 4:59 AM for a consistent overnight plateau,
                         * then ease out with a 5:00–6:00 AM smoothstep blend into the daytime curve so there is no sharp drop
                         * at dawn.
                         */

                        double minutesOfDay = (hours * 60.0) + minutes;

                        if ( minutesOfDay < (5 * 60.0) )
                                return NightLevel;

                        double radians = (minutesOfDay / (24.0 * 60.0) * Math.PI * 2.0) - Math.PI; // shift so noon is brightest
                        double brightness = 0.5 + (Math.Cos( radians ) * 0.5); // 0 (midnight) -> 1 (noon)

                        double baseLevel = DayLevel + ((1.0 - brightness) * (NightLevel - DayLevel));

                        if ( minutesOfDay <= (6 * 60.0) )
                        {
                                double blend = (minutesOfDay - (5 * 60.0)) / 60.0; // 0 at 5:00, 1 at 6:00
                                double easedBlend = blend * blend * (3.0 - (2.0 * blend)); // smoothstep easing

                                return (int)Math.Round( (NightLevel * (1.0 - easedBlend)) + (baseLevel * easedBlend) );
                        }

                        return (int)Math.Round( baseLevel );
                }

		private class LightCycleTimer : Timer
		{
			public LightCycleTimer() : base( TimeSpan.FromSeconds( 0 ), TimeSpan.FromSeconds( 5.0 ) )
			{
				Priority = TimerPriority.FiveSeconds;
			}

                        protected override void OnTick()
                        {
                                for ( int i = 0; i < NetState.Instances.Count; ++i )
                                {
                                        NetState ns = NetState.Instances[i];
                                        Mobile m = ns.Mobile;

                                        if ( m != null )
                                                m.CheckLightLevels( false );
                                }
                        }
                }

                private static void RefreshAllLights()
                {
                        for ( int i = 0; i < NetState.Instances.Count; ++i )
                        {
                                NetState ns = NetState.Instances[i];
                                Mobile m = ns.Mobile;

                                if ( m != null )
                                        m.CheckLightLevels( false );
                        }
                }

		public static bool ApplyNightSight( Mobile mobile, int level )
		{
			if ( mobile == null || mobile.Deleted )
				return false;

			NightSightTimer current;

			if ( m_NightSightTimers.TryGetValue( mobile.Serial, out current ) )
			{
				current.Stop();
				m_NightSightTimers.Remove( mobile.Serial );
				mobile.EndAction( typeof( LightCycle ) );
			}

			if ( !mobile.BeginAction( typeof( LightCycle ) ) )
				return false;

			if ( level < 0 )
				level = 0;
			else if ( level > DungeonLevel )
				level = DungeonLevel;

			TimeSpan duration = TimeSpan.FromMinutes( Utility.Random( 15, 25 ) );
			NightSightTimer timer = new NightSightTimer( mobile, duration, true );

			m_NightSightTimers[mobile.Serial] = timer;
			mobile.LightLevel = level;
			mobile.CheckLightLevels( true );
			BuffInfo.RemoveBuff( mobile, BuffIcon.NightSight );
			BuffInfo.AddBuff( mobile, new BuffInfo( BuffIcon.NightSight, 1075643, duration, mobile ) );
			timer.Start();

			return true;
		}

                public class NightSightTimer : Timer
                {
			private Mobile m_Owner;
			private bool m_Tracked;

			public NightSightTimer( Mobile owner ) : this( owner, TimeSpan.FromMinutes( Utility.Random( 15, 25 ) ), false )
			{
			}

			public NightSightTimer( Mobile owner, TimeSpan duration ) : this( owner, duration, false )
			{
			}

			public NightSightTimer( Mobile owner, TimeSpan duration, bool tracked ) : base( duration )
			{
				m_Owner = owner;
				m_Tracked = tracked;
				Priority = TimerPriority.OneMinute;
			}

			protected override void OnTick()
			{
				if ( m_Owner == null )
					return;

				if ( m_Tracked )
				{
					NightSightTimer current;

					if ( !m_NightSightTimers.TryGetValue( m_Owner.Serial, out current ) || current != this )
						return;

					m_NightSightTimers.Remove( m_Owner.Serial );
				}

				m_Owner.EndAction( typeof( LightCycle ) );

				if ( !m_Owner.Deleted )
				{
					m_Owner.LightLevel = 0;
					BuffInfo.RemoveBuff( m_Owner, BuffIcon.NightSight );
				}
			}
		}
	}
}
