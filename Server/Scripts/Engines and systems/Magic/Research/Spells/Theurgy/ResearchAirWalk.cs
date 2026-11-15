using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Spells;

namespace Server.Spells.Research
{
	public class ResearchAirWalk : ResearchSpell
	{
		public override int spellIndex { get { return 55; } }
		public int CirclePower = 7;
		public static int spellID = 55;

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1.0 ); } }
		public override double RequiredSkill { get { return (double)Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 8 ) ); } }
		public override int RequiredMana { get { return Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 7 ) ); } }

		private static SpellInfo m_Info = new SpellInfo(
			Server.Misc.Research.SpellInformation( spellID, 2 ),
			Server.Misc.Research.CapsCast( Server.Misc.Research.SpellInformation( spellID, 4 ) ),
			203,
			9031
		);

		private static Hashtable m_Table = new Hashtable();

		public ResearchAirWalk( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public static bool UnderEffect( Mobile m )
		{
			return m_Table[m] != null;
		}

		public static void RemoveEffect( Mobile m )
		{
			RemoveState( m, (AirWalkState)m_Table[m] );
		}

		public override void OnCast()
		{
			TimeSpan duration = TimeSpan.FromSeconds( GetDurationSeconds() );
			AirWalkState state = (AirWalkState)m_Table[Caster];

			if ( state != null )
			{
				TimedBuffRefreshDecision decision = TimedBuffRefresh.Decide( true, state.Strength, 1, false );

				if ( decision == TimedBuffRefreshDecision.RefreshExisting )
				{
					state.Refresh( duration );
				}
				else
				{
					RemoveState( Caster, state );
					state = null;
				}
			}

			if ( state == null )
			{
				Mobile m = Caster;

				if ( !m.BeginAction( typeof( ResearchAirWalk ) ) )
				{
					FinishSequence();
					return;
				}

				state = new AirWalkState( m, duration );
				m_Table[m] = state;
			}

			BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.Warding, 1151440, 1151441, duration, Caster ) );

			Point3D air = new Point3D( Caster.X + 1, Caster.Y + 1, Caster.Z + 5 );
			Effects.SendLocationParticles(
				EffectItem.Create( air, Caster.Map, EffectItem.DefaultDuration ),
				0x2007,
				9,
				32,
				Server.Items.CharacterDatabase.GetMySpellHue( Caster, 0 ),
				0,
				5022,
				0
			);
			Caster.PlaySound( 0x014 );
			Server.Misc.Research.ConsumeScroll( Caster, true, spellID, false );
			Caster.PublicOverheadMessage( Network.MessageType.Emote, Caster.EmoteHue, false, "*Raises off the ground*" );

			FinishSequence();
		}

		private int GetDurationSeconds()
		{
			double x = 2 * DamagingSkill( Caster );
			return (int)( 0.0066 * ( x * x ) - 1.543 * x + 325 );
		}

		private static void RemoveState( Mobile m, AirWalkState state )
		{
			if ( m == null || state == null || m_Table[m] != state )
				return;

			m_Table.Remove( m );
			state.Stop();
			m.PublicOverheadMessage( Network.MessageType.Emote, m.EmoteHue, false, "*Lowers gently to the ground*" );
			m.PlaySound( 0x014 );
			m.EndAction( typeof( ResearchAirWalk ) );
			BuffInfo.RemoveBuff( m, BuffIcon.Warding );
		}

		private sealed class AirWalkState
		{
			private Mobile m_Target;
			private TimedBuffState m_State;

			public int Strength { get { return 1; } }

			public AirWalkState( Mobile target, TimeSpan duration )
			{
				m_Target = target;
				m_State = new TimedBuffState( this, target, 1, duration, Expire );
			}

			public void Refresh( TimeSpan duration )
			{
				m_State.Replace( m_Target, 1, duration, Expire );
			}

			public void Stop()
			{
				m_State.Stop();
			}

			private void Expire( TimedBuffState ignored )
			{
				if ( m_Table[m_Target] != this )
					return;

				RemoveState( m_Target, this );
			}
		}
	}
}
