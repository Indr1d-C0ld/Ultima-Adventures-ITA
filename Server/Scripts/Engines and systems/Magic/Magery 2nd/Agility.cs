using System;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.Second
{
	public class AgilitySpell : MagerySpell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Agility", "Ex Uus",
				212,
				9061,
				Reagent.Bloodmoss,
				Reagent.MandrakeRoot
			);

		public override SpellCircle Circle { get { return SpellCircle.Second; } }

		public AgilitySpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if ( CheckBSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );

				int bonus = SpellHelper.GetOffset( Caster, m, StatType.Dex, false );
				TimeSpan length = SpellHelper.GetDuration( Caster, m );
				int appliedBonus;
				TimeSpan appliedDuration;

				SpellHelper.AddStatBonus( Caster, m, StatType.Dex, bonus, length, out appliedBonus, out appliedDuration );

				m.FixedParticles( 0x375A, 10, 15, 5010, Server.Items.CharacterDatabase.GetMySpellHue( Caster, 0 ), 0, EffectLayer.Waist );
				m.PlaySound( 0x1e7 );

				BuffInfo.AddBuff( m, new BuffInfo( BuffIcon.Agility, 1075841, appliedDuration, m, appliedBonus.ToString() ) );
			}

			FinishSequence();
		}

		public class InternalTarget : Target
		{
			private AgilitySpell m_Owner;

			public InternalTarget( AgilitySpell owner ) : base( Core.ML ? 10 : 12, false, TargetFlags.Beneficial )
			{
				m_Owner = owner;
			} 

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
				{
					m_Owner.Target( (Mobile)o );
				}
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}