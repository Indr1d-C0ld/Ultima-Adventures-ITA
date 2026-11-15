using System;
using System.Collections;
using Server.Items;
using Server.Mobiles;
using Server.Spells;
using Server.Spells.Seventh;

namespace Server.Spells.HolyMan
{
	public class SanctifySpell : HolyManSpell
	{
		private static SpellInfo m_Info = new SpellInfo( "Sanctify", "Benedicite", 266, 9040 );
		private static Hashtable m_Table = new Hashtable();

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3 ); } }
		public override int RequiredTithing { get { return 60; } }
		public override double RequiredSkill { get { return 30.0; } }
		public override int RequiredMana { get { return 15; } }

		public SanctifySpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public static bool HasEffect( Mobile m )
		{
			return GetState( m ) != null;
		}

		public static void RemoveEffect( Mobile m )
		{
			RemoveState( m, GetState( m ) );
		}

		private static SanctifyState GetState( Mobile m )
		{
			if ( m == null )
				return null;

			SanctifyState state = m_Table[m] as SanctifyState;

			if ( state == null )
			{
				m_Table.Remove( m );
				return null;
			}

			if ( m.Deleted )
				RemoveState( m, state );

			return m_Table[m] == state ? state : null;
		}

		private static void RemoveState( Mobile m, SanctifyState state )
		{
			if ( state == null )
				return;

			if ( m == null || m_Table[m] != state )
			{
				state.Stop();
				BuffInfo.RemoveBuff( m == null ? state.Target : m, BuffIcon.ManaPhase );
				return;
			}

			m_Table.Remove( m );
			state.Stop();
			BuffInfo.RemoveBuff( m, BuffIcon.ManaPhase );

			if ( !m.Deleted )
			{
				state.RemoveModifiers( m );
				m.EndAction( typeof( SanctifySpell ) );
			}
		}

		private static void ExpireState( TimedBuffState timedState )
		{
			SanctifyState state = timedState == null ? null : timedState.Owner as SanctifyState;
			RemoveState( state == null ? null : state.Target, state );
		}

		public override bool CheckCast( Mobile caster )
		{
			if ( !TransformationSpellHelper.CheckCast( Caster, this ) )
				return false;

			return base.CheckCast( caster );
		}

		public override void OnCast()
		{
			TransformationSpellHelper.OnCast( Caster, this );

			if ( CheckSequence() )
			{
				int modify = (int)( ( Caster.Skills[SkillName.Healing].Value / 25 ) + ( Caster.Skills[SkillName.SpiritSpeak].Value / 25 ) );
				modify = Server.Misc.MyServerSettings.PlayerLevelMod( modify, Caster );

				TimeSpan duration = TimeSpan.FromSeconds( (int)( Caster.Skills[SkillName.Healing].Value + ( Caster.Skills[SkillName.SpiritSpeak].Value / 2 ) ) );
				SanctifyState state = GetState( Caster );
				TimedBuffRefreshDecision decision = TimedBuffRefresh.Decide( state != null, state == null ? 0 : state.Strength, modify, false );

				if ( decision == TimedBuffRefreshDecision.RefreshExisting )
				{
					state.Refresh();
					BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.ManaPhase, 1151450, 1151451, state.FullDuration, Caster, modify.ToString() ) );
				}
				else
				{
					if ( state != null )
						RemoveState( Caster, state );

					StatMod[] statMods = new StatMod[]
					{
						new StatMod( StatType.Str, "[] Str Offset", modify, TimeSpan.Zero ),
						new StatMod( StatType.Dex, "[] Dex Offset", modify, TimeSpan.Zero ),
						new StatMod( StatType.Int, "[] Int Offset", modify, TimeSpan.Zero )
					};
					SkillMod[] skillMods = new SkillMod[]
					{
						new DefaultSkillMod( SkillName.Parry, true, modify ),
						new DefaultSkillMod( SkillName.Tactics, true, modify ),
						new DefaultSkillMod( SkillName.Anatomy, true, modify )
					};

					state = new SanctifyState( Caster, Caster, modify, duration, statMods, skillMods );
					m_Table[Caster] = state;
					state.ApplyModifiers( Caster );
					Caster.BeginAction( typeof( SanctifySpell ) );
					BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.ManaPhase, 1151450, 1151451, duration, Caster, modify.ToString() ) );
				}

				Caster.PlaySound( 0x5C9 );
				Point3D wings = new Point3D( Caster.X + 1, Caster.Y + 1, Caster.Z + 18 );
				Effects.SendLocationEffect( wings, Caster.Map, 0x3FE5, 30, 10, 0, 0 );
			}

			FinishSequence();
		}

		private sealed class SanctifyState
		{
			private Mobile m_Target;
			private int m_Strength;
			private StatMod[] m_StatMods;
			private SkillMod[] m_SkillMods;
			private TimedBuffState m_State;

			public Mobile Target { get { return m_Target; } }
			public int Strength { get { return m_Strength; } }
			public TimeSpan FullDuration { get { return m_State.FullDuration; } }

			public SanctifyState( Mobile target, Mobile source, int strength, TimeSpan duration, StatMod[] statMods, SkillMod[] skillMods )
			{
				m_Target = target;
				m_Strength = strength;
				m_StatMods = statMods;
				m_SkillMods = skillMods;
				m_State = new TimedBuffState( this, source, strength, duration, ExpireState );
			}

			public void Refresh()
			{
				m_State.Refresh();
			}

			public void Stop()
			{
				m_State.Stop();
			}

			public void ApplyModifiers( Mobile m )
			{
				for ( int i = 0; i < m_StatMods.Length; ++i )
					m.AddStatMod( m_StatMods[i] );

				for ( int i = 0; i < m_SkillMods.Length; ++i )
					m.AddSkillMod( m_SkillMods[i] );
			}

			public void RemoveModifiers( Mobile m )
			{
				for ( int i = 0; i < m_StatMods.Length; ++i )
				{
					if ( m.GetStatMod( m_StatMods[i].Name ) == m_StatMods[i] )
						m.RemoveStatMod( m_StatMods[i].Name );
				}

				for ( int i = 0; i < m_SkillMods.Length; ++i )
					m.RemoveSkillMod( m_SkillMods[i] );
			}
		}
	}
}
