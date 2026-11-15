using System;
using System.Collections;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Spells.Herbalist
{
	public class WoodlandProtectionSpell : HerbalistSpell
	{
		private static SpellInfo m_Info = new SpellInfo( "", "", 239, 9021 );
		public override int HerbalistSpellCircle{ get{ return 4; } }
		public override double CastDelay{ get{ return 1.0; } }
		public override double RequiredSkill{ get{ return 25.0; } }
		public override int RequiredMana{ get{ return 0; } }
		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1.0 ); } }

		private static Hashtable m_Table = new Hashtable();
		
		public WoodlandProtectionSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public static bool HasEffect( Mobile m )
		{
			return ( m_Table[m] != null );
		}
		
		public static void RemoveEffect( Mobile m )
		{
			WoodlandProtectionState state = (WoodlandProtectionState)m_Table[m];

			if ( state != null )
				state.Remove();
		}
		
		public override void OnCast()
		{
			WoodlandProtectionState state = (WoodlandProtectionState)m_Table[Caster];

			if ( state == null && !Caster.CanBeginAction( typeof( WoodlandProtectionSpell ) ) )
			{
				Caster.SendLocalizedMessage( 1005559 );
			}
			
			else if ( CheckSequence() )
			{
				int MyResist = (int)(Caster.Skills[SkillName.AnimalTaming].Value) / 5;

				double span = ( Caster.Skills[SkillName.AnimalLore].Value / 2 ) * 60;
				TimeSpan duration = TimeSpan.FromSeconds( (int)span );
				TimedBuffRefreshDecision decision = TimedBuffRefresh.Decide( state != null, state != null ? state.Strength : 0, MyResist, false );

				if ( decision == TimedBuffRefreshDecision.RefreshExisting )
				{
					state.Refresh();
					BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.HowlOfCacophony, 1151464, 1151465, state.FullDuration, Caster, MyResist.ToString() ) );
				}
				else
				{
					if ( state != null )
						state.Remove();

					ApplyEffect( Caster, MyResist, duration );
				}

				Caster.PlaySound( 0x19 );
				Effects.SendLocationParticles( Caster, 0xC87, 9, 10, 5025 );
			}
			FinishSequence();
		}

		private static void ApplyEffect( Mobile mobile, int resistance, TimeSpan duration )
		{
			ResistanceMod[] mods = new ResistanceMod[]
			{
				new ResistanceMod( ResistanceType.Physical, resistance ),
				new ResistanceMod( ResistanceType.Fire, resistance ),
				new ResistanceMod( ResistanceType.Cold, resistance ),
				new ResistanceMod( ResistanceType.Poison, resistance ),
				new ResistanceMod( ResistanceType.Energy, resistance )
			};

			WoodlandProtectionState state = new WoodlandProtectionState( mobile, resistance, duration, mods );
			m_Table[mobile] = state;
			state.Start();
			BuffInfo.AddBuff( mobile, new BuffInfo( BuffIcon.HowlOfCacophony, 1151464, 1151465, duration, mobile, resistance.ToString() ) );
		}

		private sealed class WoodlandProtectionState
		{
			private Mobile m_Target;
			private int m_Strength;
			private ResistanceMod[] m_Mods;
			private TimedBuffState m_State;

			public int Strength { get { return m_Strength; } }
			public TimeSpan FullDuration { get { return m_State.FullDuration; } }

			public WoodlandProtectionState( Mobile target, int strength, TimeSpan duration, ResistanceMod[] mods )
			{
				m_Target = target;
				m_Strength = strength;
				m_Mods = mods;
				m_State = new TimedBuffState( this, target, strength, duration, new TimedBuffExpireCallback( Expire ) );
			}

			public void Start()
			{
				for ( int i = 0; i < m_Mods.Length; ++i )
					m_Target.AddResistanceMod( m_Mods[i] );

				m_Target.BeginAction( typeof( WoodlandProtectionSpell ) );
			}

			public void Refresh()
			{
				m_State.Refresh();
			}

			public void Remove()
			{
				if ( m_Table[m_Target] != this )
					return;

				m_Table.Remove( m_Target );
				m_State.Stop();
				BuffInfo.RemoveBuff( m_Target, BuffIcon.HowlOfCacophony );

				for ( int i = 0; i < m_Mods.Length; ++i )
					m_Target.RemoveResistanceMod( m_Mods[i] );

				m_Target.EndAction( typeof( WoodlandProtectionSpell ) );
			}

			private void Expire( TimedBuffState state )
			{
				if ( m_Table[m_Target] != state.Owner )
					return;

				m_Target.PrivateOverheadMessage( MessageType.Regular, 0x14C, false, "Your woodland protection has worn off", m_Target.NetState );
				Remove();
			}
		}
	}
}
