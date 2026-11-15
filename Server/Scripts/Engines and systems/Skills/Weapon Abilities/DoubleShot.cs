using System;
using Server;

namespace Server.Items
{
	/// <summary>
	/// Send two arrows flying at your opponent if you're mounted.
	/// </summary>
	public class DoubleShot : WeaponAbility
	{
		public DoubleShot()
		{
		}

		public override int BaseMana { get { return 30; } }

		public override bool CheckSkills( Mobile from )
		{
			return base.CheckSkills( from );
		}

                private const double BonusDamageScalar = 1.30;

                public override void OnHit( Mobile attacker, Mobile defender, int damage )
                {
                        Use( attacker, defender, true );
                }

                public override void OnMiss( Mobile attacker, Mobile defender )
                {
                        Use( attacker, defender, false );
                }

		public override bool Validate( Mobile from )
		{
			if( base.Validate( from ) )
			{
				//if( from.Mounted )
					return true;
				//else
				//{
				//	from.SendLocalizedMessage( 1070770 ); // You can only execute this attack while mounted!
				//	ClearCurrentAbility( from );
				//}
			}

			return false;
		}

		public void Use( Mobile attacker, Mobile defender, bool firstHit )
		{
			if( !Validate( attacker ) || !CheckMana( attacker, true ) || attacker.Weapon == null )	//sanity
				return;

			ClearCurrentAbility( attacker );

			attacker.SendLocalizedMessage( 1063348 ); // You launch two shots at once!
			defender.SendLocalizedMessage( 1063349 ); // You're attacked with a barrage of shots!

			defender.FixedParticles( 0x37B9, 1, 19, 0x251D, EffectLayer.Waist );

			IWeapon weapon = attacker.Weapon;

			if ( weapon == null )
				return;

			if ( firstHit )
			{
				BaseWeapon baseWeapon = weapon as BaseWeapon;

				if ( baseWeapon != null )
				{
					baseWeapon.OnSwing( attacker, defender, BonusDamageScalar );
					return;
				}
			}

			weapon.OnSwing( attacker, defender );
		}
	}
}
