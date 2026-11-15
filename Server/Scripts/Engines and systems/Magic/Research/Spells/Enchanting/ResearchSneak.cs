using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Spells;
using Server.Targeting;

namespace Server.Spells.Research
{
	public class ResearchSneak : ResearchSpell
	{
		public override int spellIndex { get { return 3; } }
		public int CirclePower = 1;
		public static int spellID = 3;
		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 0.75 ); } }
		public override double RequiredSkill { get { return (double)Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 8 ) ); } }
		public override int RequiredMana { get { return Int32.Parse( Server.Misc.Research.SpellInformation( spellIndex, 7 ) ); } }
		private static SpellInfo m_Info = new SpellInfo( Server.Misc.Research.SpellInformation( spellID, 2 ), Server.Misc.Research.CapsCast( Server.Misc.Research.SpellInformation( spellID, 4 ) ), 212, 9061 );
		private static Hashtable m_Table = new Hashtable();

		public ResearchSneak( Mobile caster, Item scroll ) : base( caster, scroll, m_Info ) { }
		public override void OnCast() { Caster.Target = new InternalTarget( this ); }
		public static bool HasEffect( Mobile m ) { return m_Table[m] != null; }
		public static void RemoveEffect( Mobile m ) { RemoveState( m, (SneakState)m_Table[m] ); }

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) ) Caster.SendLocalizedMessage( 500237 );
			else if ( CheckBSequence( m ) )
			{
				int hiding = Math.Max( 0, 100 - (int)m.Skills[SkillName.Hiding].Base );
				int stealth = Math.Max( 0, 100 - (int)m.Skills[SkillName.Stealth].Base );
				TimeSpan duration = TimeSpan.FromSeconds( Math.Max( 120.0, Math.Min( 480.0, DamagingSkill( Caster ) * 2 ) ) );
				SneakState state = (SneakState)m_Table[m];

				if ( state != null && !state.CanReplace( hiding, stealth ) )
				{
					state.Refresh();
					ApplyVisuals( m );
					BuffInfo.AddBuff( m, new BuffInfo( BuffIcon.SpellFocusingDebuff, 1151434, 1151435, duration, m, String.Format( "{0}\t{1}", hiding, stealth ) ) );
					Server.Misc.Research.ConsumeScroll( Caster, true, spellIndex, false );
				}
				else
				{
					if ( state != null ) RemoveState( m, state );
					if ( !m.BeginAction( typeof( ResearchSneak ) ) ) { FinishSequence(); return; }
					state = new SneakState( m, Caster, hiding, stealth, duration );
					m_Table[m] = state;
					state.Apply();
					m.Hidden = true;
					ApplyVisuals( m );
					BuffInfo.AddBuff( m, new BuffInfo( BuffIcon.SpellFocusingDebuff, 1151434, 1151435, duration, m, String.Format( "{0}\t{1}", hiding, stealth ) ) );
					Server.Misc.Research.ConsumeScroll( Caster, true, spellIndex, false );
				}
			}
			FinishSequence();
		}

		private void ApplyVisuals( Mobile m ) { SpellHelper.Turn( Caster, m ); Effects.SendLocationParticles( EffectItem.Create( m.Location, m.Map, EffectItem.DefaultDuration ), 0x3728, 8, 20, 0, 0, 5042, 0 ); Effects.PlaySound( m, m.Map, 0x201 ); }
		private static void RemoveState( Mobile m, SneakState state ) { if ( m == null || state == null || m_Table[m] != state ) return; m_Table.Remove( m ); state.Stop(); state.Remove(); m.EndAction( typeof( ResearchSneak ) ); m.Hidden = false; BuffInfo.RemoveBuff( m, BuffIcon.SpellFocusingDebuff ); }

		private sealed class SneakState
		{
			private Mobile m_Target;
			private SkillMod m_HidingMod;
			private SkillMod m_StealthMod;
			private int m_HidingBonus, m_StealthBonus;
			private TimedBuffState m_State;
			public SneakState( Mobile target, Mobile source, int hiding, int stealth, TimeSpan duration ) { m_Target = target; m_HidingBonus = hiding; m_StealthBonus = stealth; m_HidingMod = new DefaultSkillMod( SkillName.Hiding, true, hiding ); m_StealthMod = new DefaultSkillMod( SkillName.Stealth, true, stealth ); m_State = new TimedBuffState( this, source, hiding + stealth, duration, Expire ); }
			public bool CanReplace( int hiding, int stealth ) { return hiding >= m_HidingBonus && stealth >= m_StealthBonus; }
			public void Apply() { m_Target.AddSkillMod( m_HidingMod ); m_Target.AddSkillMod( m_StealthMod ); }
			public void Remove() { m_Target.RemoveSkillMod( m_HidingMod ); m_Target.RemoveSkillMod( m_StealthMod ); }
			public void Refresh() { m_State.Refresh(); }
			public void Stop() { m_State.Stop(); }
			private void Expire( TimedBuffState ignored ) { if ( m_Table[m_Target] != this ) return; RemoveState( m_Target, this ); }
		}

		private class InternalTarget : Target { private ResearchSneak m_Owner; public InternalTarget( ResearchSneak owner ) : base( Core.ML ? 10 : 12, false, TargetFlags.Beneficial ) { m_Owner = owner; } protected override void OnTarget( Mobile from, object o ) { if ( o is Mobile ) m_Owner.Target( (Mobile)o ); } protected override void OnTargetFinish( Mobile from ) { m_Owner.FinishSequence(); } }
	}
}
