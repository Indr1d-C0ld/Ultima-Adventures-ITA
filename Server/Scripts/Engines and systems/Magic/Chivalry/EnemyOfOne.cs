using System;
using System.Collections;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Items;
using Server.Targeting;

namespace Server.Spells.Chivalry
{
	public class EnemyOfOneSpell : PaladinSpell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Enemy of One", "Forul Solum",
				-1,
				9002
			);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 0.5 ); } }

		public override double RequiredSkill{ get{ return 45.0; } }
		public override int RequiredMana{ get{ return 20; } }
		public override int RequiredTithing{ get{ return 10; } }
		public override int MantraNumber{ get{ return 1060723; } } // Forul Solum
		public override bool BlocksMovement{ get{ return false; } }

		public EnemyOfOneSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			if ( UnderEffect( Caster ) )
			{
				PlayEffects();
				RemoveEffect( Caster );
			}
			else if ( CheckSequence() )
			{
				PlayEffects();

				double delay = (double)ComputePowerValue( 1 ) / 60;

				// TODO: Should caps be applied?
				if ( delay < 1.5 )
					delay = 1.5;
				else if ( delay > 3.5 )
					delay = 3.5;

				ExpireTimer timer = new ExpireTimer( Caster, TimeSpan.FromMinutes( delay ) );
				m_Table[Caster] = timer;
				timer.Start();

				if ( Caster is PlayerMobile )
				{
					((PlayerMobile)Caster).EnemyOfOneType = null;
					((PlayerMobile)Caster).WaitingForEnemy = true;

					BuffInfo.AddBuff ( Caster, new BuffInfo ( BuffIcon.EnemyOfOne, 1075653, 1044111, TimeSpan.FromMinutes ( delay ), Caster ) );
				}
			}

			FinishSequence();
		}

		private void PlayEffects()
		{
			Caster.PlaySound( 0x0F5 );
			Caster.PlaySound( 0x1ED );
			Caster.FixedParticles( 0x375A, 1, 30, 9966, 33, 2, EffectLayer.Head );
			Caster.FixedParticles( 0x37B9, 1, 30, 9502, 43, 3, EffectLayer.Head );
		}

		private static Hashtable m_Table = new Hashtable();

		public static bool UnderEffect( Mobile m )
		{
			if ( m == null )
				return false;

			Timer timer = m_Table[m] as Timer;

			if ( timer == null )
			{
				m_Table.Remove( m );
				return false;
			}

			return true;
		}

		public static void RemoveEffect( Mobile m )
		{
			if ( m == null )
				return;

			RemoveEffect( m, m_Table[m] as Timer );
		}

		private static void RemoveEffect( Mobile m, Timer timer )
		{
			if ( m == null || timer == null || m_Table[m] != timer )
				return;

			m_Table.Remove( m );
			timer.Stop();
			m.PlaySound( 0x1F8 );

			if ( m is PlayerMobile )
			{
				PlayerMobile player = (PlayerMobile)m;
				player.EnemyOfOneType = null;
				player.WaitingForEnemy = false;
			}

			BuffInfo.RemoveBuff( m, BuffIcon.EnemyOfOne );
		}

		private class ExpireTimer : Timer
		{
			private Mobile m_Mobile;

			public ExpireTimer( Mobile mobile, TimeSpan delay ) : base( delay )
			{
				m_Mobile = mobile;
			}

			protected override void OnTick()
			{
				RemoveEffect( m_Mobile, this );
			}
		}
	}
}