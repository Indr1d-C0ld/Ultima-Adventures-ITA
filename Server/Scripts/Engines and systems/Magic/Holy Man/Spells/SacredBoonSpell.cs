using System;
using System.Collections;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;
using Server.Spells;

namespace Server.Spells.HolyMan
{
	public class SacredBoonSpell : HolyManSpell
	{
		private static SpellInfo m_Info = new SpellInfo( "Sacred Boon", "Sacrum Munus", 266, 9040 );
		private static Hashtable m_Table = new Hashtable();

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 4 ); } }
		public override int RequiredTithing { get { return 100; } }
		public override double RequiredSkill { get { return 60.0; } }
		public override int RequiredMana { get { return 40; } }

		public SacredBoonSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public static bool HasEffect( Mobile m )
		{
			return GetState( m ) != null;
		}

		public static void RemoveEffect( Mobile m )
		{
			RemoveState( GetState( m ) );
		}

		private static SacredBoonState GetState( Mobile m )
		{
			if ( m == null )
				return null;

			SacredBoonState state = m_Table[m] as SacredBoonState;

			if ( state == null )
			{
				m_Table.Remove( m );
				return null;
			}

			if ( m.Deleted )
				RemoveState( state );

			return m_Table[m] == state ? state : null;
		}

		private static void RemoveState( SacredBoonState state )
		{
			if ( state == null )
				return;

			Mobile m = state.Target;

			if ( m == null || m_Table[m] != state )
			{
				state.Stop();
				return;
			}

			m_Table.Remove( m );
			state.Stop();
		}

		private static void ExpireState( TimedBuffState timedState )
		{
			RemoveState( timedState == null ? null : timedState.Owner as SacredBoonState );
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) )
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.

			if ( CheckBSequence( m, false ) )
			{
				SpellHelper.Turn( Caster, m );

				int healAmount = GetHealAmount( Caster, m );
				TimeSpan duration = TimeSpan.FromSeconds( 30.0 );
				SacredBoonState state = GetState( m );
				TimedBuffRefreshDecision decision = TimedBuffRefresh.Decide( state != null, state == null ? 0 : state.HealAmount, healAmount, false );

				if ( decision == TimedBuffRefreshDecision.RefreshExisting )
				{
					state.Refresh();
				}
				else
				{
					if ( state != null )
						RemoveState( state );

					state = new SacredBoonState( m, Caster, healAmount, duration );
					m_Table[m] = state;
					state.Tick();
					state.StartTickTimer();
				}

				m.PlaySound( 0x202 );
				m.FixedParticles( 0x376A, 1, 62, 9923, 3, 3, EffectLayer.Waist );
				m.FixedParticles( 0x3779, 1, 46, 9502, 5, 3, EffectLayer.Waist );
				m.SendMessage( "A holy aura surrounds you causing your wounds to heal faster." );
			}

			FinishSequence();
		}

		private static int GetHealAmount( Mobile source, Mobile m )
		{
			double heal = 5 + ( source.Skills[SkillName.Healing].Value / 15.0 ) + ( source.Skills[SkillName.SpiritSpeak].Value / 15.0 );
			return Server.Misc.MyServerSettings.PlayerLevelMod( (int)heal, m );
		}

		private class InternalTarget : Target
		{
			private SacredBoonSpell m_Owner;

			public InternalTarget( SacredBoonSpell owner ) : base( 12, false, TargetFlags.Beneficial )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
					m_Owner.Target( (Mobile)o );
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}

		private sealed class SacredBoonState
		{
			private Mobile m_Target;
			private Mobile m_Source;
			private int m_HealAmount;
			private TimedBuffState m_State;
			private Timer m_TickTimer;

			public Mobile Target { get { return m_Target; } }
			public Mobile Source { get { return m_Source; } }
			public int HealAmount { get { return m_HealAmount; } }

			public SacredBoonState( Mobile target, Mobile source, int healAmount, TimeSpan duration )
			{
				m_Target = target;
				m_Source = source;
				m_HealAmount = healAmount;
				m_State = new TimedBuffState( this, source, healAmount, duration, ExpireState );
			}

			public void Refresh()
			{
				m_State.Refresh();
			}

			public void Stop()
			{
				if ( m_TickTimer != null )
				{
					m_TickTimer.Stop();
					m_TickTimer = null;
				}

				m_State.Stop();
			}

			public void StartTickTimer()
			{
				if ( m_TickTimer != null || m_Target == null || m_Table[m_Target] != this )
					return;

				m_TickTimer = new InternalTimer( this );
				m_TickTimer.Start();
			}

			public void Tick()
			{
				if ( m_Target == null || m_Table[m_Target] != this )
				{
					Stop();
					return;
				}

				if ( m_Target.Deleted )
				{
					RemoveState( this );
					return;
				}

				if ( !m_Target.CheckAlive() )
				{
					RemoveState( this );
					return;
				}

				m_Target.Heal( m_HealAmount );
				m_Target.PlaySound( 0x202 );
				m_Target.FixedParticles( 0x376A, 1, 62, 9923, 3, 3, EffectLayer.Waist );
				m_Target.FixedParticles( 0x3779, 1, 46, 9502, 5, 3, EffectLayer.Waist );
			}

			private sealed class InternalTimer : Timer
			{
				private SacredBoonState m_Owner;

				public InternalTimer( SacredBoonState owner ) : base( TimeSpan.FromSeconds( 4 ), TimeSpan.FromSeconds( 4 ) )
				{
					m_Owner = owner;
				}

				protected override void OnTick()
				{
					m_Owner.Tick();
				}
			}
		}
	}
}
