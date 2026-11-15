using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
        public class FreezeStrike : WeaponAbility
        {
                private const int MaxStacks = 3;
                private const int ColdResistPenaltyPerStack = -10;
                private const string DexModName = "FreezeStrikeDexPenalty";
                private static readonly TimeSpan DebuffDuration = TimeSpan.FromSeconds( 20.0 );
                private const double RegenDelayScalarPerStack = 1.0; // additional regen delay applied per stack

                private static readonly Dictionary<Mobile, FreezeStrikeDebuffInfo> m_Debuffs = new Dictionary<Mobile, FreezeStrikeDebuffInfo>();

                public FreezeStrike()
                {
                }

                public override int BaseMana{ get{ return 20; } }

                public override void OnHit( Mobile attacker, Mobile defender, int damage )
                {
                        if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
                                return;

                        ClearCurrentAbility( attacker );
                        attacker.SendMessage( "You strike them with chilling force!" );
                        defender.SendMessage( "You where struck with chilling force!" );

                        BaseWeapon weapon = attacker.Weapon as BaseWeapon;
                        if ( weapon == null )
                                return;

                        Skill skill = attacker.Skills[weapon.Skill];
                        double reqSkill = GetRequiredSkill( attacker );
                        double skilltouse = 0.0;

                        if ( skill != null )
                                skilltouse = skill.Value;

                        if ( weapon.WeaponAttributes.UseBestSkill > 0 )
                        {
                                double skilltouse2 = 0.0;
                                if ( attacker.Skills[SkillName.Swords].Value >= reqSkill ) skilltouse2 = attacker.Skills[SkillName.Swords].Value;
                                if ( attacker.Skills[SkillName.Macing].Value >= reqSkill && attacker.Skills[SkillName.Macing].Value > skilltouse2 ) skilltouse2 = attacker.Skills[SkillName.Macing].Value;
                                if ( attacker.Skills[SkillName.Fencing].Value >= reqSkill && attacker.Skills[SkillName.Fencing].Value > skilltouse2 ) skilltouse2 = attacker.Skills[SkillName.Fencing].Value;
                                if ( attacker.Skills[SkillName.Lumberjacking].Value >= reqSkill && attacker.Skills[SkillName.Lumberjacking].Value > skilltouse2 ) skilltouse2 = attacker.Skills[SkillName.Lumberjacking].Value;
                                if ( skilltouse2 > skilltouse )
                                        skilltouse = skilltouse2;
                        }

                        int todam = (int)( skilltouse / 20 );
                        AOS.Damage( defender, attacker, ( Utility.RandomMinMax( 15, 35 ) + todam ), true, 0, 0, 100, 0, 0 );

                        ApplyDebuff( attacker, defender );

                        base.OnHit( attacker, defender, damage );
                }

                private static void ApplyDebuff( Mobile attacker, Mobile defender )
                {
                        if ( defender == null )
                                return;

                        FreezeStrikeDebuffInfo info;

                        if ( !m_Debuffs.TryGetValue( defender, out info ) )
                        {
                                info = new FreezeStrikeDebuffInfo( defender );
                                m_Debuffs[defender] = info;
                        }

                        bool chillSurge = info.AddStack();
                        BuffInfo.AddBuff( defender, new BuffInfo( BuffIcon.FreezeStrike, 1151480, 1151481, DebuffDuration, defender, info.StackCount.ToString() ) );

                        attacker.SendMessage( "Your chilling strikes sap their defenses ({0}/{1})!", info.StackCount, MaxStacks );
                        defender.SendMessage( "The chill from the strike grows ({0}/{1})!", info.StackCount, MaxStacks );

                        if ( chillSurge )
                        {
                                Effects.PlaySound( defender.Location, defender.Map, 0x10B );
                                attacker.SendMessage( "Your foe is overwhelmed by the cold!" );
                                defender.SendMessage( "The chilling force slows you to a crawl!" );
                        }
                }

                public static void ClearDebuff( Mobile defender )
                {
                        if ( defender == null )
                                return;

                        FreezeStrikeDebuffInfo info;

                        if ( !m_Debuffs.TryGetValue( defender, out info ) )
                                return;

                        info.Clear();
                        m_Debuffs.Remove( defender );
                        BuffInfo.RemoveBuff( defender, BuffIcon.FreezeStrike );
                }

                public static double GetRegenScalar( Mobile defender )
                {
                        FreezeStrikeDebuffInfo info;

                        if ( defender != null && m_Debuffs.TryGetValue( defender, out info ) )
                                return info.GetRegenScalar();

                        return 1.0;
                }

                private class FreezeStrikeDebuffInfo
                {
                        private readonly Mobile m_Defender;
                        private ResistanceMod m_ColdResistMod;
                        private Timer m_Timer;
                        private bool m_PlayerSpeedForced;
                        private bool m_CreatureSpeedStored;
                        private double m_CreatureOriginalSpeed;
                        private bool m_IsChilled;

                        public int StackCount { get; private set; }
                        public bool IsChilled { get { return m_IsChilled; } }

                        public FreezeStrikeDebuffInfo( Mobile defender )
                        {
                                m_Defender = defender;
                        }

                        public bool AddStack()
                        {
                                if ( StackCount < MaxStacks )
                                        StackCount++;

                                UpdateEffects();
                                return StackCount >= MaxStacks;
                        }

                        private void UpdateEffects()
                        {
                                if ( m_Defender == null )
                                        return;

                                if ( StackCount > 0 )
                                {
                                        if ( m_ColdResistMod == null )
                                        {
                                                m_ColdResistMod = new ResistanceMod( ResistanceType.Cold, ColdResistPenaltyPerStack * StackCount );
                                                m_Defender.AddResistanceMod( m_ColdResistMod );
                                        }
                                        else
                                        {
                                                m_ColdResistMod.Offset = ColdResistPenaltyPerStack * StackCount;
                                        }

                                        ApplyDexDebuff();
                                }
                                else
                                {
                                        if ( m_ColdResistMod != null )
                                        {
                                                m_Defender.RemoveResistanceMod( m_ColdResistMod );
                                                m_ColdResistMod = null;
                                        }

                                        m_Defender.RemoveStatMod( DexModName );
                                }

                                bool shouldChill = StackCount >= MaxStacks;

                                if ( shouldChill )
                                {
                                        ApplySpeedDebuff();
                                }
                                else
                                {
                                        RemoveSpeedDebuff();
                                }

                                m_IsChilled = shouldChill;

                                if ( m_Timer != null )
                                        m_Timer.Stop();

                                m_Timer = new DebuffTimer( m_Defender );
                                m_Timer.Start();
                        }

                        public void Clear()
                        {
                                if ( m_Timer != null )
                                {
                                        m_Timer.Stop();
                                        m_Timer = null;
                                }

                                if ( m_ColdResistMod != null )
                                {
                                        m_Defender.RemoveResistanceMod( m_ColdResistMod );
                                        m_ColdResistMod = null;
                                }

                                m_Defender.RemoveStatMod( DexModName );
                                RemoveSpeedDebuff();

                                StackCount = 0;
                                m_IsChilled = false;
                        }

                        public double GetRegenScalar()
                        {
                                if ( StackCount <= 0 )
                                        return 1.0;

                                return 1.0 + ( RegenDelayScalarPerStack * StackCount );
                        }

                        private void ApplySpeedDebuff()
                        {
                                if ( m_Defender.NetState != null )
                                {
                                        m_Defender.Send( SpeedControl.WalkSpeed );
                                        m_PlayerSpeedForced = true;
                                }
                                else
                                {
                                        BaseCreature creature = m_Defender as BaseCreature;

                                        if ( creature != null )
                                        {
                                                if ( !m_CreatureSpeedStored )
                                                {
                                                        m_CreatureOriginalSpeed = creature.CurrentSpeed;
                                                        m_CreatureSpeedStored = true;
                                                }

                                                creature.CurrentSpeed = creature.PassiveSpeed;
                                        }
                                }
                        }

                        private void RemoveSpeedDebuff()
                        {
                                if ( m_Defender == null )
                                        return;

                                if ( m_PlayerSpeedForced && m_Defender.NetState != null )
                                        m_Defender.Send( SpeedControl.Disable );

                                BaseCreature creature = m_Defender as BaseCreature;

                                if ( m_CreatureSpeedStored && creature != null )
                                        creature.CurrentSpeed = m_CreatureOriginalSpeed;

                                m_PlayerSpeedForced = false;
                                m_CreatureSpeedStored = false;
                        }

                        private void ApplyDexDebuff()
                        {
                                m_Defender.RemoveStatMod( DexModName );

                                double ratio = (double)StackCount / MaxStacks;

                                if ( ratio > 1.0 )
                                        ratio = 1.0;

                                int dexLoss = (int)Math.Ceiling( m_Defender.RawDex * 0.5 * ratio );

                                int minimumLoss = StackCount;

                                if ( dexLoss < minimumLoss )
                                        dexLoss = minimumLoss;

                                m_Defender.AddStatMod( new StatMod( StatType.Dex, DexModName, -dexLoss, DebuffDuration ) );
                        }
                }

                private class DebuffTimer : Timer
                {
                        private readonly Mobile m_Target;

                        public DebuffTimer( Mobile defender ) : base( DebuffDuration )
                        {
                                m_Target = defender;
                                Priority = TimerPriority.OneSecond;
                        }

                        protected override void OnTick()
                        {
                                ClearDebuff( m_Target );
                        }
                }
        }
}
