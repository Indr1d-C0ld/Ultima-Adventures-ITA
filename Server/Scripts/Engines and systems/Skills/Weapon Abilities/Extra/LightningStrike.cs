using System;
using System.Collections.Generic;
using Server;
using Server.Spells;

namespace Server.Items
{
        public class LightningStriker : WeaponAbility
        {
                private const double ParalyzeChance = 0.25;
                private static readonly TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(3.0);
                private static readonly TimeSpan ComboWindow = TimeSpan.FromSeconds(6.0);
                private static readonly TimeSpan OverloadDebuffDuration = TimeSpan.FromSeconds(8.0);
                private const int OverloadRange = 5;

                private static readonly Dictionary<Mobile, ComboState> m_ComboStates = new Dictionary<Mobile, ComboState>();
                private static readonly Dictionary<Mobile, OverloadDebuffTimer> m_OverloadDebuffs = new Dictionary<Mobile, OverloadDebuffTimer>();

                private class ComboState
                {
                        public int Count;
                        public DateTime LastStrike;
                }

                private class OverloadDebuffTimer : Timer
                {
                        private readonly Mobile m_Target;
                        private readonly ResistanceMod m_Mod;

                        public OverloadDebuffTimer(Mobile target, ResistanceMod mod)
                                : base(OverloadDebuffDuration)
                        {
                                m_Target = target;
                                m_Mod = mod;
                                Priority = TimerPriority.TwoFiftyMS;
                        }

                        public void Expire()
                        {
                                m_Target.RemoveResistanceMod(m_Mod);
                                Stop();
                                m_OverloadDebuffs.Remove(m_Target);
                                BuffInfo.RemoveBuff( m_Target, BuffIcon.LightningStrikeDebuff );
                        }

                        protected override void OnTick()
                        {
                                m_Target.SendMessage("Your energy resistance stabilizes as the overload fades.");
                                Expire();
                        }
                }

                public LightningStriker(){}

                public override int BaseMana { get { return 20; } }

                public override void OnHit(Mobile attacker, Mobile defender, int damage)
                {
                        if (!Validate(attacker) || !CheckMana(attacker, true)) return;
                        ClearCurrentAbility(attacker);
                        attacker.SendMessage("You strike them with lightning force!");
                        defender.SendMessage("You where struck with lightning force!");

                        BaseWeapon weapon = attacker.Weapon as BaseWeapon;
                        if (weapon == null) return;
                        Skill skill = attacker.Skills[weapon.Skill];
                        double reqSkill = GetRequiredSkill(attacker);
                        double skilltouse = 0.0;
                        if (skill != null) skilltouse = skill.Value;
                        if (weapon.WeaponAttributes.UseBestSkill > 0)
                        {
                                double skilltouse2 = 0.0;
                                if ( attacker.Skills[SkillName.Swords].Value >= reqSkill ) skilltouse2 = attacker.Skills[SkillName.Swords].Value;
                                if ( attacker.Skills[SkillName.Macing].Value >= reqSkill && attacker.Skills[SkillName.Macing].Value > skilltouse2 ) skilltouse2 = attacker.Skills[SkillName.Macing].Value;
                                if ( attacker.Skills[SkillName.Fencing].Value >= reqSkill && attacker.Skills[SkillName.Fencing].Value > skilltouse2 ) skilltouse2 = attacker.Skills[SkillName.Fencing].Value;
                                if ( attacker.Skills[SkillName.Lumberjacking].Value >= reqSkill && attacker.Skills[SkillName.Lumberjacking].Value > skilltouse2 ) skilltouse2 = attacker.Skills[SkillName.Lumberjacking].Value;
                                if (skilltouse2 > skilltouse) skilltouse = skilltouse2;
                        }
                        int todam = (int)(skilltouse / 20);
                        AOS.Damage( defender, attacker, (Utility.RandomMinMax(15, 35) + todam), true, 0, 0, 0, 0, 100 );

                        if (Utility.RandomDouble() <= ParalyzeChance)
                        {
                                defender.Paralyze(ParalyzeDuration);
                                defender.FixedEffect(0x376A, 10, 16);
                                defender.PlaySound(0x204);
                                attacker.SendMessage("Your lightning strike leaves {0} momentarily paralyzed!", defender.Name);
                                defender.SendMessage("The lightning seizes your muscles, leaving you paralyzed!");
                        }

                        HandleCombo(attacker, defender);

                        base.OnHit(attacker, defender, damage);
                }

                private static void HandleCombo(Mobile attacker, Mobile defender)
                {
                        if (attacker == null || defender == null)
                                return;

                        ComboState state;

                        if (!m_ComboStates.TryGetValue(attacker, out state))
                        {
                                state = new ComboState();
                                m_ComboStates[attacker] = state;
                        }

                        if ((DateTime.UtcNow - state.LastStrike) > ComboWindow)
                                state.Count = 0;

                        state.LastStrike = DateTime.UtcNow;
                        state.Count++;

                        if (state.Count >= 3)
                        {
                                state.Count = 0;
                                TriggerOverload(attacker, defender);
                        }
                }

                private static void TriggerOverload(Mobile attacker, Mobile defender)
                {
                        if (attacker == null || defender == null)
                                return;

                        Map map = defender.Map;

                        if (map == null)
                                return;

                        List<Mobile> targets = new List<Mobile>();
                        targets.Add(defender);

                        foreach (Mobile m in defender.GetMobilesInRange(OverloadRange))
                        {
                                if (m == null || m == defender || m == attacker)
                                        continue;

                                if (m.Deleted || !m.Alive || m.Map != map)
                                        continue;

                                if (!attacker.CanSee(m) || !attacker.CanBeHarmful(m) || !SpellHelper.ValidIndirectTarget(attacker, m))
                                        continue;

                                targets.Add(m);
                        }

                        if (targets.Count == 0)
                                return;

                        attacker.SendMessage("Your successive lightning strikes overload and arc to nearby foes!");

                        foreach (Mobile target in targets)
                        {
                                if (target == null || target.Deleted)
                                        continue;

                                attacker.DoHarmful(target);
                                target.BoltEffect(0);
                                target.PlaySound(0x29);

                                int reduction = GetEnergyReduction(target);

                                if (reduction > 0)
                                        ApplyOverloadDebuff(target, reduction);

                                AOS.Damage(target, attacker, Utility.RandomMinMax(12, 24), true, 0, 0, 0, 0, 100);

                                if (target == defender)
                                        target.SendMessage("The lightning overload cascades through you, weakening your energy resistance!");
                                else
                                        target.SendMessage("Lightning arcs from {0}{1} strike, shocking you and weakening your energy resistance!", attacker.Name, attacker.Name.ToLower().EndsWith("s") ? "'" : "'s");
                        }
                }

                private static int GetEnergyReduction(Mobile target)
                {
                        if (target == null)
                                return 0;

                        int energyResistance = Math.Max(0, target.EnergyResistance);
                        double reduction = energyResistance * 0.25;

                        return (int)Math.Ceiling(reduction);
                }

                private static void ApplyOverloadDebuff(Mobile target, int reduction)
                {
                        if (target == null)
                                return;

                        OverloadDebuffTimer timer;

                        if (m_OverloadDebuffs.TryGetValue(target, out timer))
                        {
                                timer.Expire();
                        }

                        ResistanceMod mod = new ResistanceMod(ResistanceType.Energy, -reduction);
                        target.AddResistanceMod(mod);

                        timer = new OverloadDebuffTimer(target, mod);
                        timer.Start();
                        m_OverloadDebuffs[target] = timer;
                        BuffInfo.AddBuff( target, new BuffInfo( BuffIcon.LightningStrikeDebuff, 1151482, 1151483, OverloadDebuffDuration, target, reduction.ToString() ) );
                }
        }
}
