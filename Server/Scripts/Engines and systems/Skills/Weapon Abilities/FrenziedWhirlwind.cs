using System;
using System.Collections;
using Server;
using Server.Spells;
using Server.Engines.PartySystem;

namespace Server.Items
{
	/// <summary>
	/// A quick attack to all enemies in range of your weapon that causes damage over time.
	/// </summary>
	public class FrenziedWhirlwind : WeaponAbility
	{
		public FrenziedWhirlwind()
		{
		}

		public override bool CheckSkills( Mobile from )
		{
			return base.CheckSkills( from );
		}

		public override int BaseMana { get { return 30; } }

		private static Hashtable m_Registry = new Hashtable();
		public static Hashtable Registry { get { return m_Registry; } }

		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			if( !Validate( attacker ) )	//Mana check after check that there are targets
				return;

			ClearCurrentAbility( attacker );

			Map map = attacker.Map;

			if( map == null )
				return;

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;

			if( weapon == null )
				return;

                        ArrayList targets = new ArrayList();

                        TryAddTarget( attacker, defender, weapon, targets, false );

                        foreach( Mobile m in attacker.GetMobilesInRange( 2 ) )
                        {
                                if( m == attacker || m == defender )
                                        continue;

                                TryAddTarget( attacker, m, weapon, targets, true );
                        }

                        if( targets.Count > 0 )
			{
				if( !CheckMana( attacker, true ) )
					return;

				attacker.FixedEffect( 0x3728, 10, 15 );
				attacker.PlaySound( 0x2A1 );

				// 5-15 damage
				int amount = (int)(10.0 * ((Math.Max( attacker.Skills[SkillName.Tactics].Value, attacker.Skills[SkillName.Anatomy].Value ) - 50.0) / 70.0 + 5));

				for( int i = 0; i < targets.Count; ++i )
				{
					Mobile m = (Mobile)targets[i];
					attacker.DoHarmful( m, true );

                                        DotEffect effect = Registry[m] as DotEffect;
                                        int stackCount = 0;

                                        if( effect != null )
                                        {
                                                effect.Timer.Stop();
                                                Registry.Remove( m );
                                                stackCount = Math.Min( effect.StackCount + 1, 3 );
                                        }

                                        double multiplier = 1.0 + (0.15 * stackCount);
                                        int totalDamage = (int)Math.Round( amount * multiplier );

                                        InternalTimer t = new InternalTimer( attacker, m, totalDamage );
                                        t.Start();
                                        Registry[m] = new DotEffect( t, stackCount );
				}

				Timer.DelayCall( TimeSpan.FromSeconds( 2.0 ), new TimerStateCallback( RepeatEffect ), attacker );
			}
		}

                private static void TryAddTarget( Mobile attacker, Mobile target, BaseWeapon weapon, ArrayList targets, bool checkIndirect )
                {
                        if( target == null || target.Deleted || target.Map != attacker.Map || !target.Alive )
                                return;

                        if( target == attacker )
                                return;

                        if( checkIndirect && !SpellHelper.ValidIndirectTarget( attacker, target ) )
                                return;

                        if( !attacker.CanSee( target ) || !attacker.CanBeHarmful( target ) )
                                return;

                        if( !attacker.InRange( target, weapon.MaxRange ) )
                                return;

                        if( !attacker.InLOS( target ) )
                                return;

                        if( !targets.Contains( target ) )
                                targets.Add( target );
                }

                private void RepeatEffect( object state )
                {
                        Mobile attacker = (Mobile)state;

                        attacker.FixedEffect( 0x3728, 10, 15 );
			attacker.PlaySound( 0x2A1 );
		}

		private class InternalTimer : Timer
		{
			private Mobile m_Attacker;
			private Mobile m_Defender;
			private double m_DamageRemaining;
			private double m_DamageToDo;

			private readonly double DamagePerTick;

			public InternalTimer( Mobile attacker, Mobile defender, int totalDamage )
				: base( TimeSpan.Zero, TimeSpan.FromSeconds( 0.25 ), 12 )	// 3 seconds at .25 seconds apart = 12.  Confirm delay inbetween of .25 each.
			{
				m_Attacker = attacker;
				m_Defender = defender;

				m_DamageRemaining = (double)totalDamage;
				DamagePerTick = (double)totalDamage / 12 + 0.01;

				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				if( !m_Defender.Alive || m_DamageRemaining <= 0 )
				{
                                        Stop();
                                        RemoveEffect( m_Defender, this );
                                        return;
                                }

				m_DamageRemaining -= DamagePerTick;
				m_DamageToDo += DamagePerTick;

				if( m_DamageRemaining <= 0 && m_DamageToDo < 1 )
					m_DamageToDo = 1.0; //Confirm this 'round up' at the end

				int damage = (int)m_DamageToDo;

				if( damage > 0 )
				{
					m_Defender.Damage( damage, m_Attacker );
					m_DamageToDo -= damage;
				}

                                if( !m_Defender.Alive || m_DamageRemaining <= 0 )
                                {
                                        Stop();
                                        RemoveEffect( m_Defender, this );
                                }
                        }
                }

                private static void RemoveEffect( Mobile defender, InternalTimer timer )
                {
                        DotEffect effect = Registry[defender] as DotEffect;

                        if( effect != null && effect.Timer == timer )
                                Registry.Remove( defender );
                }

                private class DotEffect
                {
                        public InternalTimer Timer { get; private set; }
                        public int StackCount { get; private set; }

                        public DotEffect( InternalTimer timer, int stackCount )
                        {
                                Timer = timer;
                                StackCount = stackCount;
                        }
                }
        }
}