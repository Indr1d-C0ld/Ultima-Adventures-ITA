using System;
using System.Collections;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Items;
using Server.Targeting;
using Server.Gumps;
using Server.Spells;
using Server.Misc;

namespace Server.Spells.Research
{
	public class ResearchEndureCold : ResearchSpell
	{
		public override int spellIndex { get { return 12; } }
		public int CirclePower = 2;
		public static int spellID = 12;
		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 0.75 ); } }
		public override double RequiredSkill{ get{ return (double)(Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 8 ))); } }
		public override int RequiredMana{ get{ return Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 7 )); } }

		private static SpellInfo m_Info = new SpellInfo(
				Server.Misc.Research.SpellInformation( spellID, 2 ),
				Server.Misc.Research.CapsCast( Server.Misc.Research.SpellInformation( spellID, 4 ) ),
				236,
				9011
			);

		public ResearchEndureCold( Mobile caster, Item scroll) : base( caster, scroll, m_Info )
		{
		}

		private static Hashtable m_Table = new Hashtable();

		private static void RemoveEffect( Mobile m, EffectState expected, bool sendMessage )
		{
			EffectState state = (EffectState)m_Table[m];

			if ( state == null || ( expected != null && state != expected ) )
				return;

			m_Table.Remove( m );

			if ( state.Timer != null )
				state.Timer.Stop();

			m.RemoveResistanceMod( state.Mod );
			BuffInfo.RemoveBuff( m, BuffIcon.RageFocusingDebuff );

			if ( sendMessage )
			{
				m.SendMessage( "The cold protection effect has worn off." );
				m.PlaySound( 0x1F8 );
			}
		}

        public override void OnCast()
        {
			if( CheckSequence() )
			{
				bool success = false;

				ArrayList targets = new ArrayList();

				foreach ( Mobile m in Caster.GetMobilesInRange( 3 ) )
				{
					if ( Caster.CanBeBeneficial( m, false, true ) && !(m is Golem) )
						targets.Add( m );
				}

				for ( int i = 0; i < targets.Count; ++i )
				{
					Mobile m = (Mobile)targets[i];

					TimeSpan duration = TimeSpan.FromSeconds( (double)(DamagingSkill( Caster ) * 4) ); 
                    int amount = (int)(DamagingSkill( Caster ) / 8);
					EffectState state = (EffectState)m_Table[m];
					if ( state != null && TimedBuffRefresh.Decide( true, state.Strength, amount, false ) == TimedBuffRefreshDecision.RefreshExisting ) { state.Refresh(); BuffInfo.AddBuff( m, new BuffInfo( BuffIcon.RageFocusingDebuff, 1151436, 1151437, state.Duration, m, state.Strength.ToString() ) ); success = true; continue; }
					RemoveEffect( m, null, false );
					ExpireTimer timer;

					m.SendMessage( "Your resistance to cold has increased." );
					ResistanceMod mod1 = new ResistanceMod( ResistanceType.Cold, + amount, true );
					m.AddResistanceMod( mod1 );
					timer = new ExpireTimer( m, duration );
					state = new EffectState( m, Caster, amount, duration, mod1, timer );
					m_Table[m] = state;
					timer.State = state;
					BuffInfo.AddBuff( m, new BuffInfo( BuffIcon.RageFocusingDebuff, 1151436, 1151437, duration, m, amount.ToString() ) );

					m.PlaySound( 0x1E9 );
					m.FixedParticles( 0x375A, 9, 20, 5016, Server.Items.CharacterDatabase.GetMySpellHue( Caster, 0 ), 0, EffectLayer.Waist );

					timer.Start();
					success = true;
				}

				if ( success ){ Server.Misc.Research.ConsumeScroll( Caster, true, spellIndex, false ); }
			}
			FinishSequence();
		}

		private class EffectState
		{
			public ResistanceMod Mod { get; private set; }
			public ExpireTimer Timer { get; set; }
			public Mobile Target { get; private set; }
			public Mobile Source { get; private set; }
			public int Strength { get; private set; }
			public TimeSpan Duration { get; private set; }

			public EffectState( Mobile target, Mobile source, int strength, TimeSpan duration, ResistanceMod mod, ExpireTimer timer )
			{
				Target = target; Source = source; Strength = strength; Duration = duration; Mod = mod; Timer = timer;
			}
			public void Refresh() { Timer.Stop(); Timer = new ExpireTimer( Target, Duration ); Timer.State = this; Timer.Start(); }
		}

		private class ExpireTimer : Timer
		{
			private Mobile m_Mobile;
			private EffectState m_State;
			public EffectState State { set { m_State = value; } }

			public ExpireTimer( Mobile m, TimeSpan delay ) : base( delay )
			{
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				if ( m_Mobile == null || m_State == null || m_Table[m_Mobile] != m_State || m_State.Timer != this )
					return;

				RemoveEffect( m_Mobile, m_State, true );
			}
		}
	}
}
