using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Spells;

namespace Server.Spells.Research
{
	public class ResearchRockFlesh : ResearchSpell
	{
		public override int spellIndex { get { return 10; } }
		public int CirclePower = 5;
		public static int spellID = 10;

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 0.5 ); } }
		public override double RequiredSkill { get { return (double)Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 8 ) ); } }
		public override int RequiredMana { get { return Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 7 ) ); } }

		private static SpellInfo m_Info = new SpellInfo(
			Server.Misc.Research.SpellInformation( spellID, 2 ),
			Server.Misc.Research.CapsCast( Server.Misc.Research.SpellInformation( spellID, 4 ) ),
			236,
			9011
		);

		private static Hashtable m_Table = new Hashtable();

		public ResearchRockFlesh( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public static bool UnderEffect( Mobile m )
		{
			return m_Table[m] != null;
		}

		public static void RemoveEffect( Mobile m )
		{
			RemoveState( m, (RockFleshState)m_Table[m], true );
		}

		public override void OnCast()
		{
			TimeSpan duration = TimeSpan.FromSeconds( DamagingSkill( Caster ) * 4 );
			RockFleshState state = (RockFleshState)m_Table[Caster];

			if ( state != null )
			{
				TimedBuffRefreshDecision decision = TimedBuffRefresh.Decide( true, state.Strength, 90, false );

				if ( decision == TimedBuffRefreshDecision.RefreshExisting )
				{
					state.Refresh( duration );
					BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.SpellFocusingBuff, 1151432, 1151433, duration, Caster, state.Strength.ToString() ) );
					FinishSequence();
					return;
				}

				RemoveState( Caster, state, false );
			}

			state = new RockFleshState( Caster, duration );
			m_Table[Caster] = state;
			state.Apply();

			if ( !Caster.BeginAction( typeof( ResearchRockFlesh ) ) )
			{
				RemoveState( Caster, state, false );
				FinishSequence();
				return;
			}

			BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.SpellFocusingBuff, 1151432, 1151433, duration, Caster, state.Strength.ToString() ) );

			if ( Caster.Mount != null )
				Caster.Mount.Rider = null;

			Caster.SendMessage( "Your flesh turns to stone." );

			Point3D hands = new Point3D( Caster.X + 1, Caster.Y + 1, Caster.Z + 8 );
			Effects.SendLocationParticles(
				EffectItem.Create( hands, Caster.Map, EffectItem.DefaultDuration ),
				0x3837,
				9,
				32,
				Server.Items.CharacterDatabase.GetMySpellHue( Caster, 0xB7F ),
				0,
				5022,
				0
			);
			Caster.PlaySound( 0x65A );
			Server.Misc.Research.ConsumeScroll( Caster, true, spellID, false );
			KarmaMod( Caster, (int)RequiredSkill + RequiredMana );

			FinishSequence();
		}

		private static void RemoveState( Mobile m, RockFleshState state, bool notify )
		{
			if ( m == null || state == null || m_Table[m] != state )
				return;

			m_Table.Remove( m );
			state.Stop();
			state.Remove();
			m.EndAction( typeof( ResearchRockFlesh ) );
			BuffInfo.RemoveBuff( m, BuffIcon.SpellFocusingBuff );

			if ( notify )
			{
				m.SendMessage( "Your flesh turns back to normal." );

				Point3D hands = new Point3D( m.X + 1, m.Y + 1, m.Z + 8 );
				Effects.SendLocationParticles(
					EffectItem.Create( hands, m.Map, EffectItem.DefaultDuration ),
					0x3837,
					9,
					32,
					Server.Items.CharacterDatabase.GetMySpellHue( m, 0xB7F ),
					0,
					5022,
					0
				);
				m.PlaySound( 0x65A );
			}
		}

		private sealed class RockFleshState
		{
			private Mobile m_Target;
			private ResistanceMod m_Mod;
			private Body m_OriginalBodyMod;
			private int m_OriginalHueMod;
			private TimedBuffState m_State;

			public int Strength { get { return m_Mod.Offset; } }

			public RockFleshState( Mobile target, TimeSpan duration )
			{
				m_Target = target;
				m_Mod = new ResistanceMod( ResistanceType.Physical, 90 );
				m_OriginalBodyMod = target.BodyMod;
				m_OriginalHueMod = target.HueMod;
				m_State = new TimedBuffState( this, target, m_Mod.Offset, duration, Expire );
			}

			public void Apply()
			{
				m_Target.AddResistanceMod( m_Mod );
				m_Target.BodyMod = 14;
				m_Target.HueMod = 0xB80;
			}

			public void Remove()
			{
				m_Target.RemoveResistanceMod( m_Mod );

				if ( m_Target.BodyMod == 14 )
					m_Target.BodyMod = m_OriginalBodyMod;

				if ( m_Target.HueMod == 0xB80 )
					m_Target.HueMod = m_OriginalHueMod;
			}

			public void Refresh( TimeSpan duration )
			{
				m_State.Replace( m_Target, Strength, duration, Expire );
			}

			public void Stop()
			{
				m_State.Stop();
			}

			private void Expire( TimedBuffState ignored )
			{
				if ( m_Table[m_Target] != this )
					return;

				RemoveState( m_Target, this, true );
			}
		}
	}
}
