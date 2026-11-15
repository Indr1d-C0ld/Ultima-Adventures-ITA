using System;
using System.Collections;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.DeathKnight
{
	public class ShieldOfHateSpell : DeathKnightSpell
	{
		private static SpellInfo m_Info = new SpellInfo( "Shield of Hate", "Bael Odi", 236, 9011 );
		private static Hashtable m_Table = new Hashtable();

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1 ); } }
		public override int RequiredTithing { get { return 77; } }
		public override double RequiredSkill { get { return 60.0; } }
		public override int RequiredMana { get { return 48; } }

		public ShieldOfHateSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) )
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.

			if ( CheckBSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );

				ShieldOfHateState state = GetState( m );
				bool wasActive = state != null;
				TimeSpan duration = TimeSpan.FromSeconds( GetKarmaPower( Caster ) );
				TimedBuffRefreshDecision decision = TimedBuffRefresh.Decide( state != null, state == null ? 0 : state.Strength, 100, false );

				if ( decision == TimedBuffRefreshDecision.RefreshExisting )
				{
					state.Refresh();
				}
				else
				{
					if ( state != null )
						RemoveState( m, state );

					ResistanceMod[] mods = new ResistanceMod[]
					{
						new ResistanceMod( ResistanceType.Fire, 0 ),
						new ResistanceMod( ResistanceType.Poison, 0 ),
						new ResistanceMod( ResistanceType.Cold, 0 ),
						new ResistanceMod( ResistanceType.Physical, 100 )
					};

					state = new ShieldOfHateState( m, Caster, 100, duration, mods );
					m_Table[m] = state;
					state.Apply( m );
				}

				if ( !wasActive )
					m.SendMessage( "You feel hatred shielding you from physical harm." );

				m.PlaySound( 0x5C0 );
				m.FixedParticles( 0x376A, 1, 29, 9961, 1152, 0, EffectLayer.Waist );
				BuffInfo buffInfo = new BuffInfo( BuffIcon.ShieldOfHate, 1151498, 1151499, state.Duration, m, "100" );
				state.SetBuffInfo( buffInfo );
				BuffInfo.AddBuff( m, buffInfo );
			}

			FinishSequence();
		}

		private static ShieldOfHateState GetState( Mobile m )
		{
			if ( m == null )
				return null;

			ShieldOfHateState state = m_Table[m] as ShieldOfHateState;

			if ( state == null )
			{
				m_Table.Remove( m );
				return null;
			}

			if ( m.Deleted )
				RemoveState( m, state );

			return m_Table[m] == state ? state : null;
		}

		private static void RemoveState( Mobile m, ShieldOfHateState state )
		{
			if ( state == null )
				return;

			if ( m == null || m_Table[m] != state )
			{
				state.Stop();
				return;
			}

			m_Table.Remove( m );
			state.Stop();
			state.Remove( m );
			BuffInfo.RemoveBuff( m, state.BuffInfo );
		}

		private static void ExpireState( TimedBuffState timedState )
		{
			ShieldOfHateState state = timedState == null ? null : timedState.Owner as ShieldOfHateState;

			if ( state != null && state.Target != null && m_Table[state.Target] == state )
			{
				state.Target.SendMessage( "The shield around you dissipates..." );
				state.Target.PlaySound( 488 );
			}

			RemoveState( state == null ? null : state.Target, state );
		}

		public static bool RemoveCurse( Mobile m )
		{
			ShieldOfHateState state = GetState( m );

			if ( state == null )
				return false;

			m.SendMessage( "The shield around you dissipates..." );
			m.PlaySound( 488 );
			RemoveState( m, state );
			return true;
		}

		public static bool UnderEffect( Mobile m )
		{
			return GetState( m ) != null;
		}

		private sealed class ShieldOfHateState
		{
			private Mobile m_Target;
			private ResistanceMod[] m_Mods;
			private TimedBuffState m_State;
			private BuffInfo m_BuffInfo;

			public Mobile Target { get { return m_Target; } }
			public int Strength { get { return 100; } }
			public TimeSpan Duration { get { return m_State.FullDuration; } }
			public BuffInfo BuffInfo { get { return m_BuffInfo; } }

			public ShieldOfHateState( Mobile target, Mobile source, int strength, TimeSpan duration, ResistanceMod[] mods )
			{
				m_Target = target;
				m_Mods = mods;
				m_State = new TimedBuffState( this, source, strength, duration, ExpireState );
			}

			public void Refresh()
			{
				m_State.Refresh();
			}

			public void SetBuffInfo( BuffInfo buffInfo )
			{
				m_BuffInfo = buffInfo;
			}

			public void Stop()
			{
				m_State.Stop();
			}

			public void Apply( Mobile m )
			{
				for ( int i = 0; i < m_Mods.Length; ++i )
					m.AddResistanceMod( m_Mods[i] );
			}

			public void Remove( Mobile m )
			{
				for ( int i = 0; i < m_Mods.Length; ++i )
					m.RemoveResistanceMod( m_Mods[i] );
			}
		}

		private class InternalTarget : Target
		{
			private ShieldOfHateSpell m_Owner;

			public InternalTarget( ShieldOfHateSpell owner ) : base( 12, false, TargetFlags.Beneficial )
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
	}
}
