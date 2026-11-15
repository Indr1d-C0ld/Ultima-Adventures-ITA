using System;

namespace Server.Items
{
	/// <summary>
	/// Available on some crossbows, this special move allows archers to fire while on the move.
	/// This shot is somewhat less accurate than normal, but the ability to fire while running is a clear advantage.
	/// </summary>
        public class MovingShot : WeaponAbility
        {
                private const int BaseAccuracyPenalty = -25;
                private const double BaseDamageBonus = 1.5;

                private int m_ScaledAccuracyPenalty = BaseAccuracyPenalty;
                private double m_ScaledDamageScalar = BaseDamageBonus;

                public MovingShot()
                {
                }

                public override int BaseMana{ get{ return 15; } }
                public override int AccuracyBonus{ get{ return m_ScaledAccuracyPenalty; } }
                public override double DamageScalar{ get{ return m_ScaledDamageScalar; } }

                public override bool OnBeforeSwing( Mobile attacker, Mobile defender )
                {
                        if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
                                return false;

                        BaseWeapon weapon = attacker.Weapon as BaseWeapon;

                        double skillValue = 0.0;

                        if ( weapon != null )
                                skillValue = attacker.Skills[weapon.Skill].Value;

                        // Clamp the weapon skill to the 0-125 range so the penalty scales predictably.
                        double clampedSkill = Math.Max( 0.0, Math.Min( 125.0, skillValue ) );
                        double scaledPenalty = BaseAccuracyPenalty + (clampedSkill / 125.0) * Math.Abs( BaseAccuracyPenalty );

                        m_ScaledAccuracyPenalty = (int)Math.Round( Math.Min( 0.0, scaledPenalty ), MidpointRounding.AwayFromZero );

                        double velocityBonus = 0.0;

                        BaseRanged rangedWeapon = weapon as BaseRanged;

                        if ( rangedWeapon != null && rangedWeapon.Velocity > 0 )
                        {
                                int velocity = Math.Min( rangedWeapon.Velocity, 50 );
                                velocityBonus = velocity * 0.01;
                        }

                        m_ScaledDamageScalar = BaseDamageBonus + velocityBonus;

                        return true;
                }
		
		public override void OnMiss( Mobile attacker, Mobile defender )
		{
			//Validates in OnSwing for accuracy scalar

			ClearCurrentAbility( attacker );

			attacker.SendLocalizedMessage( 1060089 ); // You fail to execute your special move
		}

		public override bool ValidatesDuringHit { get { return false; } }

		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			//Validates in OnSwing for accuracy scalar

			ClearCurrentAbility( attacker );

			attacker.SendLocalizedMessage( 1060216 ); // Your shot was successful
		}
	}
}
