// $Id: //depot/c%23/RunUO Core Scripts/RunUO Core Scripts/Items/Weapons/Abilities/SerpentArrow.cs#2 $

using System;
using Server;
using System.Collections;
using System.Collections.Generic;

namespace Server.Items
{
        public class SerpentArrow : WeaponAbility
        {
                private static readonly TimeSpan PoisonResistDebuffDuration = TimeSpan.FromSeconds( 10.0 );
                private static readonly Dictionary<Mobile, PoisonResistDebuffTimer> m_PoisonDebuffs = new Dictionary<Mobile, PoisonResistDebuffTimer>();

                public SerpentArrow()
                {
                }

		public override int BaseMana { get { return 30; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true))
				return;

			ClearCurrentAbility(attacker);


			attacker.SendMessage("You poisoned your target.");
			defender.SendMessage("You've been poisoned.");

			int level;

                        if (Core.AOS)
                        {
                                int total = attacker.Skills.Poisoning.Fixed;

                                if (total >= 1000)
                                        level = 3;
                                else if (total > 850)
                                        level = 2;
                                else if (total > 650)
                                        level = 1;
                                else
                                        level = 0;
                        }
                        else
                        {
				double total = attacker.Skills[SkillName.Poisoning].Value;

				double dist = attacker.GetDistanceToSqrt(defender);

				if (dist >= 3.0)
					total -= (dist - 3.0) * 10.0;

				if (total >= 200.0 && 1 > Utility.Random(10))
					level = 3;
				else if (total > (Core.AOS ? 170.1 : 170.0))
					level = 2;
				else if (total > (Core.AOS ? 130.1 : 130.0))
					level = 1;
				else
					level = 0;
			}

                        ApplyPoisonResistanceDebuff(attacker, defender);

                        defender.ApplyPoison(attacker, Poison.GetPoison(level));

                        defender.FixedParticles(0x374A, 10, 15, 5021, EffectLayer.Waist);
                        defender.PlaySound(0x474);
                }

                private static void ApplyPoisonResistanceDebuff(Mobile attacker, Mobile defender)
                {
                        if (attacker == null || defender == null || attacker.Deleted || defender.Deleted)
                                return;

                        Skill poisoning = attacker.Skills[SkillName.Poisoning];

                        if (poisoning == null || poisoning.Base <= 110.0)
                                return;

                        int currentResistance = defender.PoisonResistance;

                        if (currentResistance <= 0)
                                return;

                        int reduction = (int)Math.Ceiling(currentResistance * 0.30);

                        if (reduction < 1)
                                return;

                        if (reduction > currentResistance)
                                reduction = currentResistance;

                        PoisonResistDebuffTimer existing;

                        if (m_PoisonDebuffs.TryGetValue(defender, out existing))
                                existing.End(false);

                        ResistanceMod mod = new ResistanceMod(ResistanceType.Poison, -reduction);
                        defender.AddResistanceMod(mod);

                        PoisonResistDebuffTimer timer = new PoisonResistDebuffTimer(defender, mod, PoisonResistDebuffDuration);
                        timer.Start();
                        m_PoisonDebuffs[defender] = timer;
                        BuffInfo.AddBuff( defender, new BuffInfo( BuffIcon.SerpentArrowDebuff, 1151472, 1151473, PoisonResistDebuffDuration, defender, reduction.ToString() ) );

                        attacker.SendMessage("Your serpent arrow weakens your foe's resistance to poison!");
                        defender.SendMessage("Your resistance to poison has been weakened!");
                }

                private class PoisonResistDebuffTimer : Timer
                {
                        private readonly Mobile m_Target;
                        private readonly ResistanceMod m_Mod;
                        private readonly DateTime m_Expire;
                        private bool m_Ended;

                        public PoisonResistDebuffTimer(Mobile target, ResistanceMod mod, TimeSpan duration) : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
                        {
                                m_Target = target;
                                m_Mod = mod;
                                m_Expire = DateTime.UtcNow + duration;
                                Priority = TimerPriority.OneSecond;
                        }

                        protected override void OnTick()
                        {
                                if (m_Ended)
                                        return;

                                if (m_Target == null || m_Target.Deleted || !m_Target.Alive || DateTime.UtcNow >= m_Expire || m_Target.Poison == null)
                                        End(true);
                        }

                        public void End(bool sendMessage)
                        {
                                if (m_Ended)
                                        return;

                                m_Ended = true;
                                Stop();

                                if (m_Target != null && !m_Target.Deleted)
                                        m_Target.RemoveResistanceMod(m_Mod);

                                m_PoisonDebuffs.Remove(m_Target);
						BuffInfo.RemoveBuff( m_Target, BuffIcon.SerpentArrowDebuff );

                                if (sendMessage && m_Target != null && !m_Target.Deleted && m_Target.Alive)
                                        m_Target.SendMessage("Your resistance to poison has recovered.");
                        }
                }
        }
}
