using System;
using System.Collections.Generic;
using Server.Network;
using Server.Items;
using Server.Mobiles;
using Server.Misc;
using Server.Targeting;

namespace Server.Spells.Chivalry
{
	public class HolyLightSpell : PaladinSpell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Holy Light", "Augus Luminos",
				-1,
				9002
			);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1.75 ); } }

		public override double RequiredSkill{ get{ return 55.0; } }
		public override int RequiredMana{ get{ return 10; } }
		public override int RequiredTithing{ get{ return 10; } }
		public override int MantraNumber{ get{ return 1060724; } } // Augus Luminos
		public override bool BlocksMovement{ get{ return false; } }

		public HolyLightSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override bool DelayedDamage{ get{ return false; } }

		public override void OnCast()
		{
			if ( CheckSequence() )
			{
				List<Mobile> targets = new List<Mobile>();
				List<Mobile> tauntTargets = new List<Mobile>();

				foreach ( Mobile m in Caster.GetMobilesInRange( 3 ) )
					if ( Caster != m && SpellHelper.ValidIndirectTarget( Caster, m ) && Caster.CanBeHarmful( m, false ) )
						targets.Add( m );

				foreach ( Mobile m in Caster.GetMobilesInRange( 6 ) )
					if ( Caster != m && SpellHelper.ValidIndirectTarget( Caster, m ) && Caster.CanBeHarmful( m, false ) )
						tauntTargets.Add( m );

				Caster.PlaySound( 0x212 );
				Caster.PlaySound( 0x206 );

				Effects.SendLocationParticles( EffectItem.Create( Caster.Location, Caster.Map, EffectItem.DefaultDuration ), 0x376A, 1, 29, 0x47D, 2, 9962, 0 );
				Effects.SendLocationParticles( EffectItem.Create( new Point3D( Caster.X, Caster.Y, Caster.Z - 7 ), Caster.Map, EffectItem.DefaultDuration ), 0x37C4, 1, 29, 0x47D, 2, 9502, 0 );

				double chivalryScale = GetChivalryScaling( Caster );

				int baseSdiBonus = AosAttributes.GetValue( Caster, AosAttribute.SpellDamage );
				int sdiCap = MyServerSettings.RealSpellDamageCap();

				for ( int i = 0; i < targets.Count; ++i )
				{
					Mobile m = targets[i];

					int damage = ComputePowerValue( 10 ) + Utility.RandomMinMax( 0, 2 );
					int maxDamage = (int)Math.Round( 24 * chivalryScale );

					// TODO: Should caps be applied?
					if ( damage < 8 )
						damage = 8;
					else if ( damage > maxDamage )
						damage = maxDamage;

					int sdiBonus = baseSdiBonus;

					if ( sdiBonus > sdiCap )
						sdiBonus = sdiCap;

					if ( Caster.Player && m.Player )
					{
						int pvpCap = MyServerSettings.SpellDamageIncreaseVsPlayers();

						if ( pvpCap > 0 && sdiBonus > pvpCap )
							sdiBonus = pvpCap;
					}
					else if ( !m.Player )
					{
						int pvmCap = MyServerSettings.SpellDamageIncreaseVsMonsters();

						if ( pvmCap > 0 && sdiBonus > pvmCap )
							sdiBonus = pvmCap;
					}

					damage = AOS.Scale( damage, 100 + sdiBonus );

					Caster.DoHarmful( m );
					SpellHelper.Damage( this, m, damage, 0, 0, 0, 0, 100 );
				}

				for ( int i = 0; i < tauntTargets.Count; ++i )
				{
					BaseCreature creature = tauntTargets[i] as BaseCreature;

					if ( creature != null )
						creature.ApplyTaunt( Caster, TimeSpan.FromSeconds( 6.0 ) );
				}
			}

			FinishSequence();
		}
	}
}
