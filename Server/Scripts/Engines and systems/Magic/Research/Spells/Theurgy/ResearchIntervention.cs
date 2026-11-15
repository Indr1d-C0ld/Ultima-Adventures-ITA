using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Spells;

namespace Server.Spells.Research
{
	public class ResearchIntervention : ResearchSpell
	{
		public override int spellIndex { get { return 47; } }
		public int CirclePower = 6;
		public static int spellID = 47;

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1.5 ); } }
		public override double RequiredSkill { get { return (double)Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 8 ) ); } }
		public override int RequiredMana { get { return Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 7 ) ); } }

		private static SpellInfo m_Info = new SpellInfo(
			Server.Misc.Research.SpellInformation( spellID, 2 ),
			Server.Misc.Research.CapsCast( Server.Misc.Research.SpellInformation( spellID, 4 ) ),
			242,
			9012
		);

		private static Hashtable m_Table = new Hashtable();

		public ResearchIntervention( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			if ( CheckSequence() )
			{
				int resistanceOffset = (int)( DamagingSkill( Caster ) / 5 );
				int absorption = (int)( DamagingSkill( Caster ) / 2 );
				TimeSpan duration = TimeSpan.FromMinutes( (int)( DamagingSkill( Caster ) / 12 ) );
				InterventionState state = (InterventionState)m_Table[Caster];

				if ( state == null )
				{
					state = new InterventionState( Caster );
					m_Table[Caster] = state;
				}

				state.ApplyOrRefreshResistance( resistanceOffset, duration );
				state.ApplyReflectionIfAvailable( absorption, duration );
				state.RefreshBuff();

				Caster.PlaySound( 0x5C9 );

				Point3D wings = new Point3D( Caster.X + 1, Caster.Y + 1, Caster.Z + 18 );
				Effects.SendLocationEffect( wings, Caster.Map, 0x3FE5, 30, 10, 0, 0 );
				Server.Misc.Research.ConsumeScroll( Caster, true, spellID, false );
			}

			FinishSequence();
		}

		public static void EndArmor( Mobile m )
		{
			if ( m == null )
				return;

			RemoveState( m, (InterventionState)m_Table[m] );
		}

		public static void NotifyAbsorptionConsumed( Mobile m, int amount )
		{
			InterventionState state = m == null ? null : (InterventionState)m_Table[m];

			if ( state != null )
				state.NotifyAbsorptionConsumed( amount );
		}

		private static void RemoveState( Mobile m, InterventionState state )
		{
			if ( m == null || state == null || m_Table[m] != state )
				return;

			m_Table.Remove( m );
			state.Stop();
			state.Remove();
			m.PlaySound( 0x1F8 );
			BuffInfo.RemoveBuff( m, BuffIcon.TribulationCaster );
		}

		private sealed class InterventionState
		{
			private Mobile m_Target;
			private ResistanceMod[] m_Mods;
			private int m_RemainingAbsorption;
			private TimedBuffState m_ResistanceState;
			private TimedBuffState m_ReflectionState;

			public InterventionState( Mobile target )
			{
				m_Target = target;
			}

			public void ApplyOrRefreshResistance( int offset, TimeSpan duration )
			{
				if ( m_ResistanceState != null && m_Mods != null && offset <= m_Mods[0].Offset )
				{
					m_ResistanceState.Refresh();
					return;
				}

				RemoveResistance();

				m_Mods = new ResistanceMod[]
				{
					new ResistanceMod( ResistanceType.Physical, offset ),
					new ResistanceMod( ResistanceType.Fire, offset ),
					new ResistanceMod( ResistanceType.Cold, offset ),
					new ResistanceMod( ResistanceType.Poison, offset ),
					new ResistanceMod( ResistanceType.Energy, offset )
				};

				for ( int i = 0; i < m_Mods.Length; ++i )
					m_Target.AddResistanceMod( m_Mods[i] );

				m_ResistanceState = new TimedBuffState( this, m_Target, offset, duration, ExpireResistance );
			}

			public void ApplyReflectionIfAvailable( int absorption, TimeSpan duration )
			{
				if ( m_ReflectionState != null )
					return;

				m_RemainingAbsorption = absorption;
				m_Target.MagicDamageAbsorb += m_RemainingAbsorption;
				m_ReflectionState = new TimedBuffState( this, m_Target, absorption, duration, ExpireReflection );
			}

			public void NotifyAbsorptionConsumed( int amount )
			{
				if ( amount <= 0 || m_ReflectionState == null )
					return;

				m_RemainingAbsorption = Math.Max( 0, m_RemainingAbsorption - amount );

				if ( m_RemainingAbsorption == 0 )
				{
					m_ReflectionState.Stop();
					m_ReflectionState = null;
					RemoveIfEmpty();
				}

				if ( m_Table[m_Target] == this )
					RefreshBuff();
			}

			public void Remove()
			{
				Mobile m = m_Target;

				RemoveResistance();

				if ( m_RemainingAbsorption > 0 )
					m.MagicDamageAbsorb = Math.Max( 0, m.MagicDamageAbsorb - m_RemainingAbsorption );

				m_RemainingAbsorption = 0;
				m_ReflectionState = null;
			}

			public void Stop()
			{
				if ( m_ResistanceState != null )
					m_ResistanceState.Stop();

				if ( m_ReflectionState != null )
					m_ReflectionState.Stop();
			}

			private void RemoveResistance()
			{
				if ( m_ResistanceState != null )
				{
					m_ResistanceState.Stop();
					m_ResistanceState = null;
				}

				if ( m_Mods == null )
					return;

				for ( int i = 0; i < m_Mods.Length; ++i )
					m_Target.RemoveResistanceMod( m_Mods[i] );

				m_Mods = null;
			}

			private void ExpireResistance( TimedBuffState state )
			{
				if ( m_ResistanceState != state )
					return;

				RemoveResistance();
				RemoveIfEmpty();

				if ( m_Table[m_Target] == this )
					RefreshBuff();
			}

			private void ExpireReflection( TimedBuffState state )
			{
				if ( m_ReflectionState != state )
					return;

				m_ReflectionState = null;

				if ( m_RemainingAbsorption > 0 )
					m_Target.MagicDamageAbsorb = Math.Max( 0, m_Target.MagicDamageAbsorb - m_RemainingAbsorption );

				m_RemainingAbsorption = 0;
				RemoveIfEmpty();

				if ( m_Table[m_Target] == this )
					RefreshBuff();
			}

			public void RefreshBuff()
			{
				DateTime now = DateTime.UtcNow;
				TimeSpan duration = TimeSpan.Zero;

				if ( m_ResistanceState != null )
					duration = m_ResistanceState.Expires - now;

				if ( m_ReflectionState != null )
				{
					TimeSpan reflectionDuration = m_ReflectionState.Expires - now;

					if ( reflectionDuration > duration )
						duration = reflectionDuration;
				}

				if ( duration <= TimeSpan.Zero )
					return;

				int resistanceOffset = m_Mods == null || m_Mods.Length == 0 ? 0 : m_Mods[0].Offset;
				string args = String.Format( "{0}\t{1}", resistanceOffset, m_RemainingAbsorption );

				BuffInfo.AddBuff( m_Target, new BuffInfo( BuffIcon.TribulationCaster, 1151442, 1151443, duration, m_Target, args ) );
			}

			private void RemoveIfEmpty()
			{
				if ( m_ResistanceState != null || m_ReflectionState != null || m_Table[m_Target] != this )
					return;

				m_Table.Remove( m_Target );
				m_Target.PlaySound( 0x1F8 );
				BuffInfo.RemoveBuff( m_Target, BuffIcon.TribulationCaster );
			}
		}
	}
}
