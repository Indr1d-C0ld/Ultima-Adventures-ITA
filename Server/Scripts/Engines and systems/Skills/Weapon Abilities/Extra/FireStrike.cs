using System;
using System.Collections.Generic;
using Server;

namespace Server.Items
{
        public class FireStrike : WeaponAbility
        {
                private const double DotChance = 1.00;
                private const int DotDurationSeconds = 10;
                private const int DotMaxStacks = 3;
                private const int DotFireDamagePercent = 100; // Ensure DoT ticks are treated entirely as fire damage
                private const double DotDamageScalar = 1.5; // Adjust this to increase or decrease total DoT damage

                private const int ResistanceDurationSeconds = 20;
                private const int ResistanceMaxStacks = 3;
                private const int FireResistancePenaltyPerStack = -10;

                private static readonly Dictionary<Mobile, FireResistanceEffectInfo> s_ResistanceEffects = new Dictionary<Mobile, FireResistanceEffectInfo>();
                private static readonly Dictionary<Mobile, FireDotEffectInfo> s_DotEffects = new Dictionary<Mobile, FireDotEffectInfo>();

                public FireStrike(){}

                public override int BaseMana { get { return 20; } }

                public override void OnHit(Mobile attacker, Mobile defender, int damage)
                {
                        if (!Validate(attacker) || !CheckMana(attacker, true))
                                return;

                        ClearCurrentAbility(attacker);

                        if (attacker != null)
                                attacker.SendMessage("You strike them with burning force!");

                        if (defender != null)
                                defender.SendMessage("You were struck with burning force!");

                        int bonusDamage = 0;
                        BaseWeapon weapon = attacker.Weapon as BaseWeapon;

                        if (weapon != null)
                        {
                                Skill skill = attacker.Skills[weapon.Skill];
                                double reqSkill = GetRequiredSkill(attacker);
                                double skillToUse = 0.0;

                                if (skill != null)
                                        skillToUse = skill.Value;

                                if (weapon.WeaponAttributes.UseBestSkill > 0)
                                {
                                        double best = skillToUse;

                                        double swords = attacker.Skills[SkillName.Swords].Value;
                                        double macing = attacker.Skills[SkillName.Macing].Value;
                                        double fencing = attacker.Skills[SkillName.Fencing].Value;
                                        double lumber = attacker.Skills[SkillName.Lumberjacking].Value;

                                        if (swords >= reqSkill && swords > best)
                                                best = swords;
                                        if (macing >= reqSkill && macing > best)
                                                best = macing;
                                        if (fencing >= reqSkill && fencing > best)
                                                best = fencing;
                                        if (lumber >= reqSkill && lumber > best)
                                                best = lumber;

                                        skillToUse = best;
                                }

                                bonusDamage = Utility.RandomMinMax(15, 35) + (int)(skillToUse / 20);
                                AOS.Damage(defender, attacker, bonusDamage, true, 0, 100, 0, 0, 0);
                        }

                        int totalDamage = Math.Max(1, damage + bonusDamage);

                        if (defender != null)
                        {
                                ApplyResistanceDebuff(attacker, defender);

                                if (Utility.RandomDouble() < DotChance)
                                        ApplyDot(attacker, defender, totalDamage);
                        }

                        base.OnHit(attacker, defender, damage);
                }

                private static void ApplyResistanceDebuff(Mobile attacker, Mobile defender)
                {
                        if (defender == null || defender.Deleted)
                                return;

                        FireResistanceEffectInfo info;

                        if (!s_ResistanceEffects.TryGetValue(defender, out info))
                        {
                                info = new FireResistanceEffectInfo(defender);
                                s_ResistanceEffects[defender] = info;
                        }

                        if (info.StackCount < ResistanceMaxStacks)
                                info.StackCount++;

                        info.RemainingTicks = ResistanceDurationSeconds;

                        if (info.Timer == null)
                        {
                                info.Timer = new FireResistanceTimer(info);
                                info.Timer.Start();
                        }

                        UpdateResistance(info);
                        UpdateFireStrikeBuff(info.Defender);

                        if (attacker != null)
                                attacker.SendMessage("Your strike weakens their resistance to fire!");

                        if (defender.Alive)
                                defender.SendMessage("You feel your resistance to fire wane!");
                }

                private static void UpdateResistance(FireResistanceEffectInfo info)
                {
                        if (info == null || info.Defender == null || info.Defender.Deleted)
                                return;

                        if (info.StackCount <= 0)
                        {
                                if (info.FireResistMod != null)
                                {
                                        info.Defender.RemoveResistanceMod(info.FireResistMod);
                                        info.FireResistMod = null;
                                }

                                info.LastAppliedPenalty = 0;
                                return;
                        }

                        int desiredPenalty = FireResistancePenaltyPerStack * info.StackCount;

                        if (info.FireResistMod == null)
                        {
                                info.FireResistMod = new ResistanceMod(ResistanceType.Fire, desiredPenalty);
                                info.Defender.AddResistanceMod(info.FireResistMod);
                                info.LastAppliedPenalty = desiredPenalty;

                                if (info.Defender.Alive)
                                        info.Defender.SendMessage(String.Format("Your fire resistance is reduced by {0}%!", -desiredPenalty));
                        }
                        else
                        {
                                info.FireResistMod.Offset = desiredPenalty;

                                if (info.LastAppliedPenalty != desiredPenalty)
                                {
                                        info.LastAppliedPenalty = desiredPenalty;

                                        if (info.Defender.Alive)
                                                info.Defender.SendMessage(String.Format("Your fire resistance is reduced by {0}%!", -desiredPenalty));
                                }
                        }
                }

                private static void ClearResistanceEffect(FireResistanceEffectInfo info, bool sendMessage)
                {
                        if (info == null)
                                return;

                        if (info.Timer != null)
                        {
                                info.Timer.Stop();
                                info.Timer = null;
                        }

                        if (info.Defender != null)
                        {
                                if (info.FireResistMod != null)
                                {
                                        info.Defender.RemoveResistanceMod(info.FireResistMod);
                                        info.FireResistMod = null;
                                }

                                if (sendMessage && info.Defender.Alive)
                                        info.Defender.SendMessage("You regain your resistance to fire.");

                                s_ResistanceEffects.Remove(info.Defender);
                        }

                        info.StackCount = 0;
                        info.RemainingTicks = 0;
                        info.LastAppliedPenalty = 0;
						UpdateFireStrikeBuff( info.Defender );
                }

                private static void ApplyDot(Mobile attacker, Mobile defender, int totalDamage)
                {
                        if (defender == null || defender.Deleted)
                                return;

                        FireDotEffectInfo info;

                        if (!s_DotEffects.TryGetValue(defender, out info))
                        {
                                info = new FireDotEffectInfo(defender);
                                s_DotEffects[defender] = info;
                        }

                        if (info.Stacks.Count >= DotMaxStacks)
                        {
                                FireDotStack replace = info.Stacks[0];

                                for (int i = 1; i < info.Stacks.Count; i++)
                                {
                                        if (info.Stacks[i].RemainingTicks < replace.RemainingTicks)
                                                replace = info.Stacks[i];
                                }

                                info.Stacks.Remove(replace);
                        }

                        info.Stacks.Add(new FireDotStack(attacker, totalDamage));

                        if (info.Timer == null)
                        {
                                info.Timer = new FireDotTimer(info);
                                info.Timer.Start();
                        }

                        if (attacker != null)
                                attacker.SendMessage("Your strike ignites them in lingering flames!");

                        if (defender.Alive)
                                defender.SendMessage("Flames continue to scorch you!");

                        UpdateFireStrikeBuff( defender );
                }

                private static void ClearDotEffect(FireDotEffectInfo info)
                {
                        if (info == null)
                                return;

                        if (info.Timer != null)
                        {
                                info.Timer.Stop();
                                info.Timer = null;
                        }

                        if (info.Defender != null)
                                s_DotEffects.Remove(info.Defender);

                        if (info.Defender != null)
                                UpdateFireStrikeBuff( info.Defender );
                }

                private static void UpdateFireStrikeBuff( Mobile defender )
                {
                        if ( defender == null || defender.Deleted )
                                return;

                        if ( !defender.Alive )
                        {
                                BuffInfo.RemoveBuff( defender, BuffIcon.FireStrike );
                                return;
                        }

                        FireResistanceEffectInfo resistance;
                        FireDotEffectInfo dot;
                        int remainingSeconds = 0;
                        int stackCount = 0;

                        if ( s_ResistanceEffects.TryGetValue( defender, out resistance ) )
                        {
                                remainingSeconds = Math.Max( remainingSeconds, resistance.RemainingTicks );
                                stackCount = resistance.StackCount;
                        }

                        if ( s_DotEffects.TryGetValue( defender, out dot ) )
                        {
                                for ( int i = 0; i < dot.Stacks.Count; ++i )
                                        remainingSeconds = Math.Max( remainingSeconds, dot.Stacks[i].RemainingTicks );

                                if ( stackCount == 0 )
                                        stackCount = dot.Stacks.Count;
                        }

                        if ( remainingSeconds <= 0 )
                        {
                                BuffInfo.RemoveBuff( defender, BuffIcon.FireStrike );
                                return;
                        }

                        BuffInfo.AddBuff( defender, new BuffInfo( BuffIcon.FireStrike, 1151478, 1151479, TimeSpan.FromSeconds( remainingSeconds ), defender, stackCount.ToString() ) );
                }

                private class FireResistanceEffectInfo
                {
                        public Mobile Defender { get; private set; }
                        public int StackCount { get; set; }
                        public int RemainingTicks { get; set; }
                        public FireResistanceTimer Timer { get; set; }
                        public ResistanceMod FireResistMod { get; set; }
                        public int LastAppliedPenalty { get; set; }

                        public FireResistanceEffectInfo(Mobile defender)
                        {
                                Defender = defender;
                        }
                }

                private class FireResistanceTimer : Timer
                {
                        private readonly FireResistanceEffectInfo m_Info;

                        public FireResistanceTimer(FireResistanceEffectInfo info) : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
                        {
                                m_Info = info;
                                Priority = TimerPriority.TwoFiftyMS;
                        }

                        protected override void OnTick()
                        {
                                Mobile defender = m_Info.Defender;

                                if (defender == null || defender.Deleted)
                                {
                                        ClearResistanceEffect(m_Info, false);
                                        return;
                                }

                                if (!defender.Alive)
                                {
                                        ClearResistanceEffect(m_Info, false);
                                        return;
                                }

                                m_Info.RemainingTicks--;

                                if (m_Info.RemainingTicks <= 0)
                                {
                                        ClearResistanceEffect(m_Info, true);
                                        return;
                                }

                                UpdateResistance(m_Info);
                        }
                }

                private class FireDotEffectInfo
                {
                        public Mobile Defender { get; private set; }
                        public List<FireDotStack> Stacks { get; private set; }
                        public FireDotTimer Timer { get; set; }

                        public FireDotEffectInfo(Mobile defender)
                        {
                                Defender = defender;
                                Stacks = new List<FireDotStack>();
                        }
                }

                private class FireDotStack
                {
                        public Mobile Attacker { get; private set; }
                        public int RemainingTicks { get; set; }
                        public double DamagePerTick { get; private set; }
                        public double PendingDamage { get; set; }

                        public FireDotStack(Mobile attacker, int totalDamage)
                        {
                                Attacker = attacker;
                                RemainingTicks = DotDurationSeconds;
                                DamagePerTick = Math.Max(0.1, (totalDamage * DotDamageScalar) / DotDurationSeconds);
                        }
                }

                private class FireDotTimer : Timer
                {
                        private readonly FireDotEffectInfo m_Info;

                        public FireDotTimer(FireDotEffectInfo info) : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
                        {
                                m_Info = info;
                                Priority = TimerPriority.TwoFiftyMS;
                        }

                        protected override void OnTick()
                        {
                                Mobile defender = m_Info.Defender;

                                if (defender == null || defender.Deleted)
                                {
                                        ClearDotEffect(m_Info);
                                        return;
                                }

                                if (!defender.Alive)
                                {
                                        ClearDotEffect(m_Info);
                                        return;
                                }

                                for (int i = m_Info.Stacks.Count - 1; i >= 0; --i)
                                {
                                        FireDotStack stack = m_Info.Stacks[i];

                                        if (stack.RemainingTicks <= 0)
                                        {
                                                m_Info.Stacks.RemoveAt(i);
                                                continue;
                                        }

                                        stack.PendingDamage += stack.DamagePerTick;
                                        int damageToDeal = (int)stack.PendingDamage;

                                        if (damageToDeal > 0)
                                        {
                                                stack.PendingDamage -= damageToDeal;
                                                AOS.Damage(defender, stack.Attacker, damageToDeal, 0, DotFireDamagePercent, 0, 0, 0);
                                        }

                                        stack.RemainingTicks--;

                                        if (stack.RemainingTicks <= 0)
                                                m_Info.Stacks.RemoveAt(i);
                                }

                                if (m_Info.Stacks.Count == 0)
                                {
                                        ClearDotEffect(m_Info);
                                }
                        }
                }
        }
}
