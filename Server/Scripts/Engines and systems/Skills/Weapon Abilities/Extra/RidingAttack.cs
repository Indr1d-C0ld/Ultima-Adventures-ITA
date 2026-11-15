using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
	public class RidingAttack : WeaponAbility
	{
		private const int RequiredRunTiles = 10;

		private static readonly Dictionary<Mobile, RidingAttackContext> m_RunData = new Dictionary<Mobile, RidingAttackContext>();

		static RidingAttack()
		{
			EventSink.Movement += OnMovement;
		}

		public RidingAttack(){}
		public override int BaseMana { get { return 20; } }
		public override bool CheckSkills( Mobile from )
		{
			return base.CheckSkills( from );
		}

		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			if( !attacker.Mounted )
			{
				ResetContext( attacker );
				attacker.SendMessage( "You must be mounted to use this ability!" );
				ClearCurrentAbility( attacker );
				return;
			}

			if( !Validate( attacker ) || !CheckMana( attacker, true ) )
			{
				ResetContext( attacker );
				return;
			}

			int tilesRan = GetAndResetRunTiles( attacker );

			ClearCurrentAbility( attacker );

			bool hasPiercing;
			int amount = 10 + (int)(10.0 * (attacker.Skills[SkillName.Chivalry].Value) / 70.0 + 5);
			int scaledAmount = ScaleDamage( amount, tilesRan, out hasPiercing );

			if( defender.Mounted )
			{
				Mobile mount = defender.Mount as Mobile;
				BaseMount.Dismount( defender );

				if( mount != null )
					DealDamage( mount, attacker, scaledAmount, hasPiercing );
				else
					DealDamage( defender, attacker, scaledAmount, hasPiercing );
			}
			else
			{
				DealDamage( defender, attacker, scaledAmount, hasPiercing );
				if( Server.Items.ParalyzingBlow.IsImmune( defender ) )
				{
					attacker.SendLocalizedMessage( 1070804 );
					defender.SendLocalizedMessage( 1070813 );
				}
				else
				{
					defender.Paralyze( TimeSpan.FromSeconds( 4.0 ) );
					Server.Items.ParalyzingBlow.BeginImmunity( defender, Server.Items.ParalyzingBlow.FreezeDelayDuration );
				}
			}
		}

		private static void OnMovement( MovementEventArgs e )
		{
			Mobile mobile = e.Mobile;

			if( mobile == null || mobile.Deleted )
				return;

			if( WeaponAbility.GetCurrentAbility( mobile ) == WeaponAbility.RidingAttack )
			{
				RidingAttackContext context;

				if( !m_RunData.TryGetValue( mobile, out context ) )
				{
					context = new RidingAttackContext();
					m_RunData[mobile] = context;
				}

				context.RecordMovement( e.Direction );
			}
			else if( m_RunData.ContainsKey( mobile ) )
			{
				m_RunData.Remove( mobile );
			}
		}

		private static void ResetContext( Mobile mobile )
		{
			if( mobile == null )
				return;

			m_RunData.Remove( mobile );
		}

		private static int GetAndResetRunTiles( Mobile mobile )
		{
			RidingAttackContext context;

			if( mobile != null && m_RunData.TryGetValue( mobile, out context ) )
			{
				m_RunData.Remove( mobile );
				return context.TilesRan;
			}

			return 0;
		}

		private static int ScaleDamage( int baseDamage, int tilesRan, out bool hasPiercing )
		{
			int clampedTiles = Math.Max( 0, Math.Min( tilesRan, RequiredRunTiles ) );
			hasPiercing = ( clampedTiles >= RequiredRunTiles );

			double scalar = 1.0 + ( clampedTiles / (double)RequiredRunTiles );
			int scaled = (int)Math.Round( baseDamage * scalar, MidpointRounding.AwayFromZero );

			return Math.Max( 1, scaled );
		}

		private static void DealDamage( Mobile target, Mobile attacker, int damage, bool hasPiercing )
		{
			if( target == null || target.Deleted || damage <= 0 )
				return;

			if( hasPiercing )
				AOS.Damage( target, attacker, damage, false, 50, 0, 0, 0, 0, 0, 50, false, false, false );
			else
				AOS.Damage( target, attacker, damage, 100, 0, 0, 0, 0 );
		}

		private class RidingAttackContext
		{
			public int TilesRan { get; private set; }

			public void RecordMovement( Direction direction )
			{
				if( ( direction & Direction.Running ) == 0 || TilesRan >= RequiredRunTiles )
					return;

				TilesRan++;
			}
		}
	}
}
