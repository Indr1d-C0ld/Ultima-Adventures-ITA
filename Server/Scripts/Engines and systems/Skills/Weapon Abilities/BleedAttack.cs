using System;
using System.Collections;
using Server.Mobiles;
using Server.Spells.Necromancy;
using Server.Network;
using Server.Spells;

namespace Server.Items
{
	/// <summary>
	/// Make your opponent bleed profusely with this wicked use of your weapon.
	/// When successful, the target will bleed for several seconds, taking damage as time passes for up to ten seconds.
	/// The rate of damage slows down as time passes, and the blood loss can be completely staunched with the use of bandages. 
	/// </summary>
	public class BleedAttack : WeaponAbility
	{
		private static readonly TimeSpan BuffDuration = TimeSpan.FromSeconds( 14.0 );

		public BleedAttack()
		{
		}

		public override int BaseMana{ get{ return 30; } }

		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
				return;

			ClearCurrentAbility( attacker );

			// Necromancers under Lich or Wraith Form are immune to Bleed Attacks.
			TransformContext context = TransformationSpellHelper.GetContext( defender );

			if ( (context != null && ( context.Type == typeof( LichFormSpell ) || context.Type == typeof( WraithFormSpell ))) ||
				(defender is BaseCreature && ((BaseCreature)defender).BleedImmune) )
			{
				attacker.SendLocalizedMessage( 1062052 ); // Your target is not affected by the bleed attack!
				return;
			}

			attacker.SendLocalizedMessage( 1060159 ); // Your target is bleeding!
			defender.SendLocalizedMessage( 1060160 ); // You are bleeding!

			if ( defender is PlayerMobile )
			{
				defender.LocalOverheadMessage( MessageType.Regular, 0x21, 1060757 ); // You are bleeding profusely
				defender.NonlocalOverheadMessage( MessageType.Regular, 0x21, 1060758, defender.Name ); // ~1_NAME~ is bleeding profusely
			}

			defender.PlaySound( 0x133 );
			defender.FixedParticles( 0x377A, 244, 25, 9950, 31, 0, EffectLayer.Waist );

                        BeginBleed( defender, attacker, damage );
                }

                private static Hashtable m_Table = new Hashtable();

		public static bool IsBleeding( Mobile m )
		{
			return m_Table.Contains( m );
		}

                public static void BeginBleed( Mobile m, Mobile from )
                {
                        BeginBleed( m, from, 0 );
                }

                public static void BeginBleed( Mobile m, Mobile from, int damage )
                {
                        Timer t = (Timer)m_Table[m];

                        if ( t != null )
                                t.Stop();

                        t = new InternalTimer( from, m, damage );
                        m_Table[m] = t;

					PlayerMobile player = m as PlayerMobile;

					if ( player != null )
						player.AddBuff( new BuffInfo( BuffIcon.Bleed, 1075829, 1075830, BuffDuration, player ) );

                        t.Start();
                }

                public static void DoBleed( Mobile m, Mobile from, int amount )
                {
                        if ( m.Alive )
                        {
                                int damage = amount;

                                if ( damage < 0 )
                                        damage = 0;

                                if ( !m.Player )
                                        damage *= 2;

                                m.PlaySound( 0x133 );

                                if ( damage > 0 )
                                        m.Damage( damage, from ); // Direct true damage

                                Blood blood = new Blood();

                                blood.ItemID = Utility.Random( 0x122A, 5 );

                                blood.MoveToWorld( m.Location, m.Map );
                        }
                        else
                        {
                                EndBleed( m, false );
                        }
		}

		public static void EndBleed( Mobile m, bool message )
		{
			Timer t = (Timer)m_Table[m];
			PlayerMobile player = m as PlayerMobile;

			if ( t == null )
			{
				if ( player != null )
					player.RemoveBuff( BuffIcon.Bleed );

				return;
			}

			t.Stop();
			m_Table.Remove( m );

			if ( player != null )
				player.RemoveBuff( BuffIcon.Bleed );

			if ( message )
				m.SendLocalizedMessage( 1060167 ); // The bleeding wounds have healed, you are no longer bleeding!
		}

		private class InternalTimer : Timer
		{
                        private Mobile m_From;
                        private Mobile m_Mobile;
                        private int m_Count;
                        private int[] m_TickDamages;

                        private static readonly int[] s_TickWeights = new int[] { 5, 5, 10, 15, 20, 20, 25 };
                        private const int DefaultBaseDamage = 35;

                        public InternalTimer( Mobile from, Mobile m, int damage ) : base( TimeSpan.FromSeconds( 2.0 ), TimeSpan.FromSeconds( 2.0 ) )
                        {
                                m_From = from;
                                m_Mobile = m;
                                m_TickDamages = CalculateTickDamages( damage );
                                Priority = TimerPriority.TwoFiftyMS;
                        }

                        protected override void OnTick()
                        {
                                if ( m_Count >= m_TickDamages.Length )
                                {
                                        EndBleed( m_Mobile, true );
                                        return;
                                }

                                int damage = m_TickDamages[m_Count];

                                if ( damage > 0 )
                                        DoBleed( m_Mobile, m_From, damage );
                                else if ( m_Mobile.Alive )
                                        m_Mobile.PlaySound( 0x133 );

                                m_Count++;

                                if ( m_Count >= m_TickDamages.Length )
                                        EndBleed( m_Mobile, true );
                        }

                        private int[] CalculateTickDamages( int damage )
                        {
                                int[] ticks = new int[s_TickWeights.Length];

                                int effectiveDamage = (damage > 0) ? RemoveStealthDamageBonus( damage ) : DefaultBaseDamage;

                                double anatomySkill = 0.0;

                                if ( m_From != null && m_From != m_Mobile )
                                        anatomySkill = m_From.Skills[SkillName.Anatomy].Value;

                                double anatomyScalar = 0.6 + (anatomySkill / 200.0);

                                if ( anatomyScalar < 0.1 )
                                        anatomyScalar = 0.1;

                                int totalDamage = (int)Math.Round( effectiveDamage * anatomyScalar );

                                if ( totalDamage < 1 )
                                        totalDamage = 1;

                                double remainingDamage = totalDamage;
                                double remainingWeight = 0;

                                for ( int i = 0; i < s_TickWeights.Length; i++ )
                                        remainingWeight += s_TickWeights[i];

                                for ( int i = 0; i < s_TickWeights.Length; i++ )
                                {
                                        if ( remainingDamage <= 0 )
                                        {
                                                ticks[i] = 0;
                                                continue;
                                        }

                                        if ( i == s_TickWeights.Length - 1 )
                                        {
                                                ticks[i] = (int)Math.Round( remainingDamage );
                                                remainingDamage = 0;
                                                continue;
                                        }

                                        double weight = s_TickWeights[i];
                                        double portion = remainingDamage * (weight / remainingWeight);
                                        int tickDamage = (int)Math.Floor( portion );

                                        if ( tickDamage < 0 )
                                                tickDamage = 0;

                                        if ( tickDamage > remainingDamage )
                                                tickDamage = (int)Math.Floor( remainingDamage );

                                        ticks[i] = tickDamage;
                                        remainingDamage -= tickDamage;
                                        remainingWeight -= weight;
                                }

                                int assigned = 0;

                                for ( int i = 0; i < ticks.Length; i++ )
                                        assigned += ticks[i];

                                int difference = totalDamage - assigned;

                                if ( difference != 0 )
                                {
                                        if ( difference > 0 )
                                        {
                                                ticks[ticks.Length - 1] += difference;
                                        }
                                        else
                                        {
                                                difference = -difference;

                                                for ( int i = ticks.Length - 1; i >= 0 && difference > 0; i-- )
                                                {
                                                        if ( ticks[i] <= 0 )
                                                                continue;

                                                        int reduction = Math.Min( ticks[i], difference );
                                                        ticks[i] -= reduction;
                                                        difference -= reduction;
                                                }
                                        }
                                }

                                return ticks;
                        }

                        private int RemoveStealthDamageBonus( int damage )
                        {
                                if ( damage <= 0 )
                                        return damage;

                                PlayerMobile pm = m_From as PlayerMobile;

                                if ( pm == null )
                                        return damage;

                                double stealthScalar = pm.LastSneakAttackDamageScalar;

                                if ( stealthScalar <= 0.0 )
                                        return damage;

                                int baseDamage = (int)Math.Floor( damage / (1.0 + stealthScalar) );

                                if ( baseDamage < 0 )
                                        baseDamage = 0;

                                int recomputed = baseDamage + (int)Math.Floor( baseDamage * stealthScalar );

                                while ( recomputed < damage && baseDamage < damage )
                                {
                                        baseDamage++;
                                        recomputed = baseDamage + (int)Math.Floor( baseDamage * stealthScalar );
                                }

                                while ( recomputed > damage && baseDamage > 0 )
                                {
                                        baseDamage--;
                                        recomputed = baseDamage + (int)Math.Floor( baseDamage * stealthScalar );
                                }

                                pm.LastSneakAttackDamageScalar = 0.0;

                                if ( baseDamage < 0 )
                                        baseDamage = 0;
                                else if ( baseDamage > damage )
                                        baseDamage = damage;

                                return baseDamage;
                        }
                }
        }
}
