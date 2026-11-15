using System;
using Server;
using System.Collections;

namespace Server.Items
{
	/// <summary>
	/// Attack with increased damage with additional damage over time.
	/// </summary>
	public class TalonStrike : WeaponAbility
	{
		private static Hashtable m_Registry = new Hashtable();
		public static Hashtable Registry { get { return m_Registry; } }

		public TalonStrike()
		{
		}

		public override int BaseMana { get { return 30; } }
		public override double DamageScalar { get { return 1.4; } }

		public override bool CheckSkills( Mobile from )
		{
			return base.CheckSkills( from );
		}

		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			if( Registry.Contains( defender ) || !Validate( attacker ) || !CheckMana( attacker, true ) )
				return;

			ClearCurrentAbility( attacker );

			attacker.SendLocalizedMessage( 1063358 ); // You deliver a talon strike!
			defender.SendLocalizedMessage( 1063359 ); // Your attacker delivers a talon strike!

			defender.FixedParticles( 0x373A, 1, 17, 0x26BC, 0x662, 0, EffectLayer.Waist );

			double tactics = attacker.Skills[SkillName.Tactics].Value;
			double clampedTactics = Math.Max( 0.0, Math.Min( 125.0, tactics ) );
			double anatomy = attacker.Skills[SkillName.Anatomy].Value;
			double clampedAnatomy = Math.Max( 0.0, Math.Min( 125.0, anatomy ) );

			double baseDamage = 10.0 * (tactics - 50.0) / 70.0 + 5;
			double anatomyBonus = 1.0 + 0.25 * (clampedAnatomy / 125.0);
			int totalDamage = (int)Math.Max( 1.0, Math.Round( baseDamage * anatomyBonus ) );

			double totalDuration = 2.0 + (clampedTactics / 125.0) * 4.0; // 2 - 6 seconds based on tactics skill

			Timer t = new InternalTimer( defender, totalDamage, attacker, totalDuration );  //Damage over time

			t.Start();

			Registry.Add( defender, t );
			BuffInfo.AddBuff( defender, new BuffInfo( BuffIcon.TalonStrike, 1028856, 1028856, TimeSpan.FromSeconds( totalDuration ), defender ) );
		}

		private class InternalTimer : Timer
		{
			private Mobile m_Defender;
			private double m_DamageRemaining;
			private double m_DamageToDo;
			private Mobile m_Attacker;

			private readonly double DamagePerTick;
			private const double FlatDamagePerTick = 1.0;

			public InternalTimer( Mobile defender, int totalDamage, Mobile attacker, double totalDuration )
				: base( TimeSpan.Zero, TimeSpan.FromSeconds( 0.25 ), Math.Max( 1, (int)Math.Round( totalDuration / 0.25 ) ) )
			{
				m_Defender = defender;
				m_DamageRemaining = (double)totalDamage;
				Priority = TimerPriority.TwentyFiveMS;

				m_Attacker = attacker;

				int totalTicks = Math.Max( 1, (int)Math.Round( totalDuration / 0.25 ) );
				DamagePerTick = (double)totalDamage / totalTicks + .01;
			}

			protected override void OnTick()
			{
				if( !m_Defender.Alive || m_DamageRemaining <= 0 )
				{
					Stop();
					Server.Items.TalonStrike.Registry.Remove( m_Defender );
					BuffInfo.RemoveBuff( m_Defender, BuffIcon.TalonStrike );
					return;
				}

				m_DamageRemaining -= DamagePerTick;
				m_DamageToDo += DamagePerTick + FlatDamagePerTick;

				if( m_DamageRemaining <= 0 && m_DamageToDo < 1 )
					m_DamageToDo = 1.0; //Confirm this 'round up' at the end

				int damage = (int)m_DamageToDo;

                                if( damage > 0 )
                                {
                                        m_Defender.Damage( damage, m_Attacker, false );
                                        m_DamageToDo -= damage;
                                }

				if( !m_Defender.Alive || m_DamageRemaining <= 0 )
				{
					Stop();
					Server.Items.TalonStrike.Registry.Remove( m_Defender );
					BuffInfo.RemoveBuff( m_Defender, BuffIcon.TalonStrike );
				}
			}
		}
	}
}
