using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
	public class AchillesStrike : WeaponAbility
	{
		public AchillesStrike(){}

		public override int BaseMana { get { return 20; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true)) return;
			ClearCurrentAbility(attacker);
			attacker.SendMessage("You damage their Achilles tendon!");
			defender.SendMessage("Your Achilles tendon was hurt!");

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;
			if (weapon == null) return;
			Skill skill = attacker.Skills[weapon.Skill];
			double reqSkill = GetRequiredSkill(attacker);
			double skilltouse = 0.0;
			if (skill != null) skilltouse = skill.Value;
			if (weapon.WeaponAttributes.UseBestSkill > 0)
			{
				double skilltouse2 = 0.0;
				if ( attacker.Skills[SkillName.Swords].Value >= reqSkill ) skilltouse2 = attacker.Skills[SkillName.Swords].Value;
				if ( attacker.Skills[SkillName.Macing].Value >= reqSkill && attacker.Skills[SkillName.Macing].Value > skilltouse2 ) skilltouse2 = attacker.Skills[SkillName.Macing].Value;
				if ( attacker.Skills[SkillName.Fencing].Value >= reqSkill && attacker.Skills[SkillName.Fencing].Value > skilltouse2 ) skilltouse2 = attacker.Skills[SkillName.Fencing].Value;
				if ( attacker.Skills[SkillName.Lumberjacking].Value >= reqSkill && attacker.Skills[SkillName.Lumberjacking].Value > skilltouse2 ) skilltouse2 = attacker.Skills[SkillName.Lumberjacking].Value;
				if (skilltouse2 > skilltouse) skilltouse = skilltouse2;
			}
                        int todam = (int)(skilltouse / 20);
                        double rootDuration = (Utility.RandomDouble() * todam / 2) + 4.0; // Base duration increased by 1 second
                        ApplyAchillesEffect( attacker, defender, TimeSpan.FromSeconds( rootDuration ) );

                        double baseDamage = Utility.RandomMinMax( 5, 25 ) + todam;
                        int totalDamage = (int)Math.Round( baseDamage * 1.3 );

                        AOS.Damage( defender, attacker, totalDamage, true, 100, 0, 0, 0, 0 );
                        base.OnHit(attacker, defender, damage);
                }

                internal static bool HasHitBonus( Mobile attacker, Mobile defender )
                {
                        if ( attacker == null || defender == null )
                                return false;

                        AchillesStrikeInfo info;

                        if ( m_AchillesEffects.TryGetValue( defender, out info ) )
                                return info.IsRooted && info.Attacker == attacker;

                        return false;
                }

                internal static int GetHitBonus()
                {
                        return AchillesHitChanceBonus;
                }

                private static void ApplyAchillesEffect( Mobile attacker, Mobile defender, TimeSpan duration )
                {
                        if ( attacker == null || defender == null )
                                return;

                        AchillesStrikeInfo info;

                        if ( !m_AchillesEffects.TryGetValue( defender, out info ) )
                        {
                                info = new AchillesStrikeInfo( defender );
                                m_AchillesEffects[defender] = info;
			}

                        info.Apply( attacker, duration );
			BuffInfo.AddBuff( defender, new BuffInfo( BuffIcon.AchillesStrike, 1151494, 1151495, duration + AchillesSlowDuration, defender ) );
                }

                private static void RemoveAchilles( Mobile defender, bool fromSlowTimer )
                {
                        if ( defender == null )
                                return;

                        AchillesStrikeInfo info;

                        if ( m_AchillesEffects.TryGetValue( defender, out info ) )
                        {
			info.Clear( fromSlowTimer );
			m_AchillesEffects.Remove( defender );
			BuffInfo.RemoveBuff( defender, BuffIcon.AchillesStrike );
                        }
                }

                private const int AchillesHitChanceBonus = 15;
                private static readonly TimeSpan AchillesSlowDuration = TimeSpan.FromSeconds( 7.0 );
                private static readonly Dictionary<Mobile, AchillesStrikeInfo> m_AchillesEffects = new Dictionary<Mobile, AchillesStrikeInfo>();

                private sealed class AchillesStrikeInfo
                {
                        private readonly Mobile m_Defender;
                        private Mobile m_Attacker;
                        private Timer m_RootTimer;
                        private Timer m_SlowTimer;
                        private bool m_IsRooted;
                        private bool m_PlayerSpeedForced;
                        private bool m_CreatureSpeedStored;
                        private double m_CreatureOriginalSpeed;
                        private bool m_CantWalkApplied;

                        public Mobile Attacker{ get{ return m_Attacker; } }
                        public bool IsRooted{ get{ return m_IsRooted; } }

                        public AchillesStrikeInfo( Mobile defender )
                        {
                                m_Defender = defender;
                        }

                        public void Apply( Mobile attacker, TimeSpan duration )
                        {
                                if ( m_Defender == null )
                                        return;

                                m_Attacker = attacker;
                                ResetSlowState( true );

                                if ( !m_Defender.CantWalk )
                                {
                                        m_Defender.CantWalk = true;
                                        m_CantWalkApplied = true;
                                }
                                else if ( !m_IsRooted )
                                {
                                        m_CantWalkApplied = false;
                                }

                                m_IsRooted = true;

                                if ( m_RootTimer != null )
                                {
                                        m_RootTimer.Stop();
                                        m_RootTimer = null;
                                }

                                m_RootTimer = new RootTimer( this, duration );
                                m_RootTimer.Start();
                        }

                        public void OnRootExpired()
                        {
                                m_RootTimer = null;

                                if ( m_Defender == null || m_Defender.Deleted )
                                {
                                        RemoveAchilles( m_Defender, false );
                                        return;
                                }

                                if ( m_CantWalkApplied )
                                {
                                        m_Defender.CantWalk = false;
                                        m_CantWalkApplied = false;
                                }

                                m_IsRooted = false;
                                ApplySlow();
                        }

                        public void OnSlowExpired()
                        {
                                ResetSlowState( false );
                                RemoveAchilles( m_Defender, true );
                        }

                        public void Clear( bool fromSlowTimer )
                        {
                                if ( m_RootTimer != null )
                                {
                                        m_RootTimer.Stop();
                                        m_RootTimer = null;
                                }

                                if ( m_IsRooted && m_CantWalkApplied && m_Defender != null )
                                {
                                        m_Defender.CantWalk = false;
                                }

                                m_IsRooted = false;
                                m_CantWalkApplied = false;

                                ResetSlowState( !fromSlowTimer );

                                m_Attacker = null;
                        }

                        private void ApplySlow()
                        {
                                if ( m_Defender == null || m_Defender.Deleted )
                                {
                                        RemoveAchilles( m_Defender, false );
                                        return;
                                }

                                ResetSlowState( true );

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
                                                m_CreatureOriginalSpeed = creature.CurrentSpeed;
                                                m_CreatureSpeedStored = true;
                                                creature.CurrentSpeed = creature.PassiveSpeed;
                                        }
                                }

                                if ( m_SlowTimer != null )
                                {
                                        m_SlowTimer.Stop();
                                        m_SlowTimer = null;
                                }

                                m_SlowTimer = new SlowTimer( this, AchillesSlowDuration );
                                m_SlowTimer.Start();
                        }

                        private void ResetSlowState( bool stopTimer )
                        {
                                if ( m_SlowTimer != null )
                                {
                                        if ( stopTimer )
                                                m_SlowTimer.Stop();

                                        m_SlowTimer = null;
                                }

                                if ( m_PlayerSpeedForced && m_Defender != null && m_Defender.NetState != null )
                                        m_Defender.Send( SpeedControl.Disable );

                                if ( m_CreatureSpeedStored )
                                {
                                        BaseCreature creature = m_Defender as BaseCreature;

                                        if ( creature != null )
                                                creature.CurrentSpeed = m_CreatureOriginalSpeed;
                                }

                                m_PlayerSpeedForced = false;
                                m_CreatureSpeedStored = false;
                        }
                }

                private sealed class RootTimer : Timer
                {
                        private readonly AchillesStrikeInfo m_Info;

                        public RootTimer( AchillesStrikeInfo info, TimeSpan duration ) : base( duration )
                        {
                                m_Info = info;
                                Priority = TimerPriority.TwentyFiveMS;
                        }

                        protected override void OnTick()
                        {
                                if ( m_Info != null )
                                        m_Info.OnRootExpired();
                        }
                }

                private sealed class SlowTimer : Timer
                {
                        private readonly AchillesStrikeInfo m_Info;

                        public SlowTimer( AchillesStrikeInfo info, TimeSpan duration ) : base( duration )
                        {
                                m_Info = info;
                                Priority = TimerPriority.OneSecond;
                        }

                        protected override void OnTick()
                        {
                                if ( m_Info != null )
                                        m_Info.OnSlowExpired();
                        }
                }
        }
}
