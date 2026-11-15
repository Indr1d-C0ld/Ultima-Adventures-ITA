using System;
using System.Collections;
using Server;
using Server.Misc;

namespace Server.Items
{
	public class DeathBlow : WeaponAbility
	{
		private const int DamageBuffBonus = 20;
		private static readonly TimeSpan DamageBuffDuration = TimeSpan.FromSeconds( 10.0 );

		private static readonly Hashtable m_DamageBuffTable = new Hashtable();

                private class DamageBuffInfo
                {
                        private readonly Mobile m_From;

                        public Mobile From
                        {
                                get { return m_From; }
                        }
                        public Timer Timer { get; set; }

                        public DamageBuffInfo( Mobile from )
                        {
                                m_From = from;
                        }
                }

		public DeathBlow(){}

		public override int BaseMana { get { return 50; } }

		public override bool OnBeforeSwing(Mobile attacker, Mobile defender, bool validate)
		{
			if (validate && (!Validate(attacker) || !CheckMana(attacker, false))) return false;
			else return true;
		}

		public override bool OnBeforeDamage(Mobile attacker, Mobile defender)
		{
			if (!CheckMana(attacker, true)) return false;
			ClearCurrentAbility(attacker);
			attacker.SendMessage("You strike a deadly blow!");
			defender.SendMessage("You were struck with a deadly blow!");
			defender.PlaySound(0x213);
			defender.FixedParticles(0x377A, 1, 32, 9949, 1153, 0, EffectLayer.Head);
			Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 10), defender.Map), new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 20), defender.Map), 0x36FE, 1, 0, false, false, 1133, 3, 9501, 1, 0, EffectLayer.Waist, 0x100);
			int damage = 0;
			BaseWeapon weapon = attacker.Weapon as BaseWeapon;
			Skill skill = attacker.Skills[weapon.Skill];
			int skLevel = 1;

			if (skill != null) 
				skLevel = (int)(skill.Value / 150) +1;

			int phys, fire, cold, pois, nrgy, chaos, direct;
			weapon.GetDamageTypes( attacker, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct );
			int maxd = (int)(weapon.MaxDamage);

			int resource = (int)(weapon.Resource);

			if (resource >=  400) 
				resource = 10;
			else if (resource >=  300) 
				resource = 6;
			else if (resource >=  200) 
				resource = 3;
			else if (resource >=  100) 
				resource = 2;
			if (resource >  20) 
				resource = 1;
			if (resource <  1) 
				resource = 1;


			damage = AdventuresFunctions.DiminishingReturns( (int)(( resource * skLevel * maxd) / 2), 200, 10 );
			AOS.Damage( defender, attacker, damage, phys, fire, cold, pois, nrgy );

			if ( attacker != null && attacker.Player )
				ApplyDamageBuff( attacker );

			return true;
		}

		public static bool TryGetDamageBonus( Mobile from, ref int damageBonus )
		{
			if ( from == null )
				return false;

			DamageBuffInfo info = m_DamageBuffTable[from] as DamageBuffInfo;

			if ( info == null )
				return false;

			damageBonus = DamageBuffBonus;
			return true;
		}

		private static void ApplyDamageBuff( Mobile from )
		{
			if ( from == null || from.Deleted )
				return;

			DamageBuffInfo info = m_DamageBuffTable[from] as DamageBuffInfo;
			bool isNew = (info == null);

			if ( isNew )
			{
				info = new DamageBuffInfo( from );
				m_DamageBuffTable[from] = info;
			}
			else if ( info.Timer != null )
			{
				info.Timer.Stop();
			}

			info.Timer = Timer.DelayCall( DamageBuffDuration, EndDamageBuff, info );
			BuffInfo.AddBuff( from, new BuffInfo( BuffIcon.DeathBlow, 1151486, 1151487, DamageBuffDuration, from, DamageBuffBonus.ToString() ) );

			if ( info.Timer != null )
				info.Timer.Priority = TimerPriority.OneSecond;

			if ( isNew )
				from.SendMessage( "Your deathblow empowers you, increasing your damage!" );
			else
				from.SendMessage( "Your deathblow empowerment is renewed." );

			from.Delta( MobileDelta.WeaponDamage );
		}

		private static void EndDamageBuff( object state )
		{
			DamageBuffInfo info = state as DamageBuffInfo;

			if ( info == null )
				return;

			m_DamageBuffTable.Remove( info.From );
			BuffInfo.RemoveBuff( info.From, BuffIcon.DeathBlow );

			if ( info.From != null && !info.From.Deleted )
			{
				info.From.SendMessage( "The power from your deathblow fades." );
				info.From.Delta( MobileDelta.WeaponDamage );
			}

			if ( info.Timer != null )
			{
				info.Timer.Stop();
				info.Timer = null;
			}
		}
	}
}
