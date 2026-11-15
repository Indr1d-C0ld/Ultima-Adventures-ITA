using System;
using Server.Targeting;
using Server.Network;
using Server;

namespace Server.Spells.First
{
	public class NightSightSpell : MagerySpell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Night Sight", "In Lor",
				236,
				9031,
				Reagent.SulfurousAsh,
				Reagent.SpidersSilk
			);

		public override SpellCircle Circle { get { return SpellCircle.First; } }

		public NightSightSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new NightSightTarget( this );
		}

		private class NightSightTarget : Target
		{
			private Spell m_Spell;

			public NightSightTarget( Spell spell ) : base( 12, false, TargetFlags.Beneficial )
			{
				m_Spell = spell;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is Mobile && m_Spell.CheckBSequence( (Mobile) targeted ) )
				{
					Mobile targ = (Mobile)targeted;

					SpellHelper.Turn( m_Spell.Caster, targ );

					double skill = Core.AOS ? targ.Skills[SkillName.Magery].Value : from.Skills[SkillName.Magery].Value;
					int level = (int)( LightCycle.DungeonLevel * ( skill / 100.0 ) );

					if ( level < 0 )
						level = 0;
					else if ( level > LightCycle.DungeonLevel )
						level = LightCycle.DungeonLevel;

					if ( LightCycle.ApplyNightSight( targ, level ) )
					{

						targ.FixedParticles( 0x376A, 9, 32, 5007, Server.Items.CharacterDatabase.GetMySpellHue( from, 0 ), 0, EffectLayer.Waist );
						targ.PlaySound( 0x1E3 );
					}
					else
					{
						from.SendMessage( "{0} already have nightsight.", from == targ ? "You" : "They" );
					}
				}

				m_Spell.FinishSequence();
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Spell.FinishSequence();
			}
		}
	}
}
