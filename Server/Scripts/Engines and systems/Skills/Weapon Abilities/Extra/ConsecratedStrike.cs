using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;

namespace Server.Items
{
        public class ConsecratedStrike : WeaponAbility
        {
                private const int HitsRequiredForDebuff = 3;
                private const int ResistanceDebuffAmount = -20;
                private static readonly TimeSpan ResistanceDebuffDuration = TimeSpan.FromSeconds( 10.0 );

                private static readonly Dictionary<Mobile, HitTracker> m_HitTrackers = new Dictionary<Mobile, HitTracker>();
                private static readonly Dictionary<Mobile, ResistanceDebuffInfo> m_Debuffs = new Dictionary<Mobile, ResistanceDebuffInfo>();

                public ConsecratedStrike(){}
                public override int BaseMana { get { return 20; } }
                public override double DamageScalar { get { return 1.5; } }

                public override bool OnBeforeSwing(Mobile attacker, Mobile defender, bool validate)
                {
                        if (validate && (!Validate(attacker) || !CheckMana(attacker, false))) return false;
                        else return true;
                }

                public override void OnHit(Mobile attacker, Mobile defender, int damage)
                {
                        if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
                                return;

                        ClearCurrentAbility(attacker);
                        attacker.SendMessage("You hit them with the highest possible damage!");
                        defender.PlaySound(0x56);

                        HandleConsecutiveHit(attacker, defender);
                }

                private static void HandleConsecutiveHit( Mobile attacker, Mobile defender )
                {
                        if ( attacker == null || defender == null )
                                return;

                        HitTracker tracker;

                        if ( !m_HitTrackers.TryGetValue( attacker, out tracker ) )
                        {
                                tracker = new HitTracker( defender );
                                m_HitTrackers[attacker] = tracker;
                        }

                        if ( tracker.Defender != defender )
                        {
                                tracker.Defender = defender;
                                tracker.HitCount = 0;
                        }

                        tracker.HitCount++;

                        if ( tracker.HitCount >= HitsRequiredForDebuff )
                        {
                                tracker.HitCount = 0;
                                ApplyResistanceDebuff( attacker, defender );
                        }
                }

                private static void ApplyResistanceDebuff( Mobile attacker, Mobile defender )
                {
                        if ( defender == null )
                                return;

                        ResistanceType resistanceType = GetLowestResistanceType( defender );

                        ResistanceDebuffInfo info;

                        if ( !m_Debuffs.TryGetValue( defender, out info ) )
                        {
                                info = new ResistanceDebuffInfo( defender );
                                m_Debuffs[defender] = info;
                        }

                        info.Apply( resistanceType );

                        if ( attacker != null )
                                attacker.SendMessage( "Your consecrated strikes weaken their {0} resistance!", resistanceType.ToString().ToLower() );

                        defender.SendMessage( "Your {0} resistance is weakened by consecrated strikes!", resistanceType.ToString().ToLower() );
                }

                private static ResistanceType GetLowestResistanceType( Mobile defender )
                {
                        ResistanceType lowestType = ResistanceType.Physical;
                        int lowestValue = defender.GetResistance( lowestType );

                        foreach ( ResistanceType type in Enum.GetValues( typeof( ResistanceType ) ) )
                        {
                                int value = defender.GetResistance( type );

                                if ( value < lowestValue )
                                {
                                        lowestValue = value;
                                        lowestType = type;
                                }
                        }

                        return lowestType;
                }

                private class HitTracker
                {
                        public HitTracker( Mobile defender )
                        {
                                Defender = defender;
                        }

                        public Mobile Defender { get; set; }
                        public int HitCount { get; set; }
                }

                private class ResistanceDebuffInfo
                {
                        private Timer m_Timer;

                        public ResistanceDebuffInfo( Mobile defender )
                        {
                                Defender = defender;
                        }

                        public Mobile Defender { get; private set; }
                        public ResistanceMod Mod { get; private set; }

                        public void Apply( ResistanceType type )
                        {
                                if ( Defender == null )
                                        return;

                                if ( Mod != null )
                                {
                                        Defender.RemoveResistanceMod( Mod );
                                        Mod = null;
                                }

                                Mod = new ResistanceMod( type, ResistanceDebuffAmount );
                                Defender.AddResistanceMod( Mod );

                                if ( m_Timer != null )
                                        m_Timer.Stop();

                                m_Timer = new ExpireTimer( Defender );
                                m_Timer.Start();
                                BuffInfo.AddBuff( Defender, new BuffInfo( BuffIcon.ConsecratedStrike, 1151484, 1151485, ResistanceDebuffDuration, Defender, Math.Abs( ResistanceDebuffAmount ).ToString() ) );
                        }

                        public void Clear()
                        {
                                if ( Defender == null )
                                        return;

                                if ( m_Timer != null )
                                {
                                        m_Timer.Stop();
                                        m_Timer = null;
                                }

                                if ( Mod != null )
                                {
                                        Defender.RemoveResistanceMod( Mod );
                                        Mod = null;
                                }

                                BuffInfo.RemoveBuff( Defender, BuffIcon.ConsecratedStrike );
                        }

                        private class ExpireTimer : Timer
                        {
                                private readonly Mobile m_Defender;

                                public ExpireTimer( Mobile defender ) : base( ResistanceDebuffDuration )
                                {
                                        m_Defender = defender;
                                }

                                protected override void OnTick()
                                {
                                        if ( m_Defender == null )
                                                return;

                                        ResistanceDebuffInfo info;

                                        if ( m_Debuffs.TryGetValue( m_Defender, out info ) )
                                        {
                                                info.Clear();
                                                m_Debuffs.Remove( m_Defender );
                                                m_Defender.SendMessage( "The consecrated strike's debuff fades." );
                                        }
                                }
                        }
                }
        }
}
