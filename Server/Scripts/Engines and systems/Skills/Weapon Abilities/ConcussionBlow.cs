using System;
using Server.Network;

namespace Server.Items
{
	/// <summary>
	/// This devastating strike is most effective against those who are in good health and whose reserves of mana are low, or vice versa.
	/// </summary>
	public class ConcussionBlow : WeaponAbility
	{
		public ConcussionBlow()
		{
		}

		public override int BaseMana{ get{ return 20; } }

		public override bool OnBeforeDamage( Mobile attacker, Mobile defender )
		{
			if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
				return false;

			ClearCurrentAbility( attacker );

			attacker.SendLocalizedMessage( 1060165 ); // You have delivered a concussion!
			defender.SendLocalizedMessage( 1060166 ); // You feel disoriented!

			defender.PlaySound( 0x213 );
			defender.FixedParticles( 0x377A, 1, 32, 9949, 1153, 0, EffectLayer.Head );

			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( defender.X, defender.Y, defender.Z + 10 ), defender.Map ), new Entity( Serial.Zero, new Point3D( defender.X, defender.Y, defender.Z + 20 ), defender.Map ), 0x36FE, 1, 0, false, false, 1133, 3, 9501, 1, 0, EffectLayer.Waist, 0x100 );

                        int damage = 10; // Base damage is 10.
                        double difference = 0.0;

                        if ( defender.HitsMax > 0 )
                        {
                                double hitsPercent = ( (double)defender.Hits / (double)defender.HitsMax ) * 100.0;

                                double manaPercent = 0;

                                if ( defender.ManaMax > 0 )
                                        manaPercent = ( (double)defender.Mana / (double)defender.ManaMax ) * 100.0;

                                difference = Math.Abs( hitsPercent - manaPercent );

                                int baseBonus = Math.Min( (int)( difference / 4.0 ), 20 );
                                int intensifiedBonus = (int)Math.Ceiling( baseBonus * 1.25 );

                                damage += intensifiedBonus;
                        }

                        if ( difference > 50.0 )
                        {
                                defender.Animate( 21, 6, 1, true, false, 0 );
                                defender.LocalOverheadMessage( MessageType.Emote, 0x3B2, true, "*stumbles around in confusion*" );

                                if ( !defender.Paralyzed )
                                        defender.Paralyze( TimeSpan.FromSeconds( 1.5 ) );
                        }

                        // Total damage is 10 + (0~25) = 10~35, physical, non-resistable.

			defender.Damage( damage, attacker );

			return true;
		}
	}
}