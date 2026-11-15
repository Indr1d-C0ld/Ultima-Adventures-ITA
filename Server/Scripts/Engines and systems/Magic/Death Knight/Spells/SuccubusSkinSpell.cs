using System;
using System.Collections;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;

namespace Server.Spells.DeathKnight
{
	public class SuccubusSkinSpell : DeathKnightSpell
	{
		private static SpellInfo m_Info = new SpellInfo( "Succubus Skin", "Erinyes Carnem", 236, 9011 );
		private static Hashtable m_Table = new Hashtable();

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3 ); } }
		public override int RequiredTithing { get { return 49; } }
		public override int RequiredMana { get { return 32; } }
		public override double RequiredSkill { get { return 35.0; } }

		public SuccubusSkinSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
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

		private static SuccubusSkinState GetState( Mobile m )
		{
			if ( m == null )
				return null;

			SuccubusSkinState state = m_Table[m] as SuccubusSkinState;

			if ( state == null )
			{
				m_Table.Remove( m );
				return null;
			}

			if ( m.Deleted )
				RemoveState( state );

			return m_Table[m] == state ? state : null;
		}

		private static void RemoveState( SuccubusSkinState state )
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
			BuffInfo.RemoveBuff( m, state.BuffInfo );
		}

		private static void ExpireState( TimedBuffState timedState )
		{
			RemoveState( timedState == null ? null : timedState.Owner as SuccubusSkinState );
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

				int healAmount = Server.Misc.MyServerSettings.PlayerLevelMod( Utility.RandomMinMax( 5, 10 ), m );
				TimeSpan duration = TimeSpan.FromSeconds( GetKarmaPower( Caster ) );
				SuccubusSkinState state = GetState( m );
				TimedBuffRefreshDecision decision = TimedBuffRefresh.Decide( state != null, state == null ? 0 : state.HealAmount, healAmount, false );

				if ( decision == TimedBuffRefreshDecision.RefreshExisting )
				{
					state.Refresh();
				}
				else
				{
					if ( state != null )
						RemoveState( state );

					state = new SuccubusSkinState( m, Caster, healAmount, duration );
					m_Table[m] = state;
					state.Tick();
					state.StartTickTimer();
				}

				m.PlaySound( 0x202 );
				m.FixedParticles( 0x3779, 1, 46, 9502, 5, 3, EffectLayer.Waist );
				m.SendMessage( "Your skin changes, causing your wounds to heal faster." );
				BuffInfo buffInfo = new BuffInfo( BuffIcon.CorpseSkin, 1044123, 1044118, state.Duration, m );
				state.SetBuffInfo( buffInfo );
				BuffInfo.AddBuff( m, buffInfo );
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private SuccubusSkinSpell m_Owner;

			public InternalTarget( SuccubusSkinSpell owner ) : base( 12, false, TargetFlags.Beneficial )
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

		private sealed class SuccubusSkinState
		{
			private Mobile m_Target;
			private Mobile m_Source;
			private int m_HealAmount;
			private TimedBuffState m_State;
			private Timer m_TickTimer;
			private BuffInfo m_BuffInfo;

			public Mobile Target { get { return m_Target; } }
			public Mobile Source { get { return m_Source; } }
			public int HealAmount { get { return m_HealAmount; } }
			public TimeSpan Duration { get { return m_State.FullDuration; } }
			public BuffInfo BuffInfo { get { return m_BuffInfo; } }

			public SuccubusSkinState( Mobile target, Mobile source, int healAmount, TimeSpan duration )
			{
				m_Target = target;
				m_Source = source;
				m_HealAmount = healAmount;
				m_State = new TimedBuffState( this, source, healAmount, duration, ExpireState );
			}

			public void SetBuffInfo( BuffInfo buffInfo )
			{
				m_BuffInfo = buffInfo;
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
				m_Target.FixedParticles( 0x3779, 1, 46, 9502, 5, 3, EffectLayer.Waist );
			}

			private sealed class InternalTimer : Timer
			{
				private SuccubusSkinState m_Owner;

				public InternalTimer( SuccubusSkinState owner ) : base( TimeSpan.FromSeconds( 4 ), TimeSpan.FromSeconds( 4 ) )
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
