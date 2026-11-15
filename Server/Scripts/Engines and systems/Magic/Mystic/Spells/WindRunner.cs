using System;
using System.Collections;
using Server.Network;
using Server.Items;
using Server.Mobiles;

namespace Server.Spells.Mystic
{
	public class WindRunner : MysticSpell
	{
		private static SpellInfo m_Info = new SpellInfo( "Wind Runner", "Beh Ra Mu Ahm", 269, 0 );
		private static Hashtable m_Table = new Hashtable();

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3 ); } }
		public override int RequiredTithing { get { return 250; } }
		public override double RequiredSkill { get { return 70.0; } }
		public override int RequiredMana { get { return 50; } }
		public override int MysticSpellCircle { get { return 2; } }

		public WindRunner( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public static bool UnderEffect( Mobile m )
		{
			return GetState( m ) != null;
		}

		public static void RemoveEffect( Mobile m )
		{
			RemoveState( m, GetState( m ), true );
		}

		public static bool StopTimer( Mobile m )
		{
			WindRunnerState state = GetState( m );

			if ( state == null )
				return false;

			if ( m_Table[m] != state )
				return false;

			m_Table.Remove( m );
			state.Stop();
			BuffInfo.RemoveBuff( m, BuffIcon.Rage );
			return true;
		}

		private static WindRunnerState GetState( Mobile m )
		{
			if ( m == null )
				return null;

			WindRunnerState state = m_Table[m] as WindRunnerState;

			if ( state == null )
			{
				m_Table.Remove( m );
				return null;
			}

			if ( m.Deleted )
				RemoveState( m, state, false );

			return m_Table[m] == state ? state : null;
		}

		private static void RemoveState( Mobile m, WindRunnerState state, bool releaseSpeed )
		{
			if ( state == null )
				return;

			if ( m == null || m_Table[m] != state )
			{
				state.Stop();
				BuffInfo.RemoveBuff( m == null ? state.Target : m, BuffIcon.Rage );
				return;
			}

			m_Table.Remove( m );
			state.Stop();
			BuffInfo.RemoveBuff( m, BuffIcon.Rage );

			if ( releaseSpeed && !m.Deleted )
			{
				m.Send( SpeedControl.Disable );
				m.EndAction( typeof( WindRunner ) );
				m.PlaySound( 0x64C ); // Cleansing winds
				m.SendMessage( "You feel the wind around you dissipate" );
			}
		}

		private static void ExpireState( TimedBuffState timedState )
		{
			WindRunnerState state = timedState == null ? null : timedState.Owner as WindRunnerState;
			RemoveState( state == null ? null : state.Target, state, true );
		}

		public override void OnCast()
		{
			Item shoes = Caster.FindItemOnLayer( Layer.Shoes );

			if ( Caster.Mounted )
			{
				Caster.SendMessage( "You cannot use this ability while on a mount!" );
			}
			else if ( shoes is BootsofHermes )
			{
				Caster.SendMessage( "You cannot use this ability while wearing those magical boots!" );
			}
			else if ( Caster is PlayerMobile && ( (PlayerMobile)Caster ).SoulBound && ( (PlayerMobile)Caster ).sbmasterspeed )
			{
				Caster.SendMessage( "You are already moving with speed!" );
			}
			else if ( CheckSequence() )
			{
				WindRunnerState state = GetState( Caster );
				TimeSpan duration = TimeSpan.FromSeconds( (int)( Caster.Skills[SkillName.Wrestling].Value * 5 ) );
				TimedBuffRefreshDecision decision = TimedBuffRefresh.Decide( state != null, state == null ? 0 : state.Strength, 1, false );

				if ( decision == TimedBuffRefreshDecision.RefreshExisting )
				{
					state.Refresh();
				}
				else
				{
					if ( state != null )
						StopTimer( Caster );

					state = new WindRunnerState( Caster, 1, duration );
					m_Table[Caster] = state;
				}

				BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.Rage, 1151454, 1151455, state.FullDuration, Caster ) );

				Caster.Send( SpeedControl.MountSpeed );
				Caster.BeginAction( typeof( WindRunner ) );
				Point3D air = new Point3D( Caster.X + 1, Caster.Y + 1, Caster.Z + 5 );
				Effects.SendLocationParticles( EffectItem.Create( air, Caster.Map, EffectItem.DefaultDuration ), 0x2007, 9, 32, 0, 0, 5022, 0 );
				Caster.PlaySound( 0x64F ); // Hailstorm
			}

			FinishSequence();
		}

		private sealed class WindRunnerState
		{
			private Mobile m_Target;
			private TimedBuffState m_State;

			public Mobile Target { get { return m_Target; } }
			public int Strength { get { return 1; } }
			public TimeSpan FullDuration { get { return m_State.FullDuration; } }

			public WindRunnerState( Mobile target, int strength, TimeSpan duration )
			{
				m_Target = target;
				m_State = new TimedBuffState( this, target, strength, duration, ExpireState );
			}

			public void Refresh()
			{
				m_State.Refresh();
			}

			public void Stop()
			{
				m_State.Stop();
			}
		}
	}
}
