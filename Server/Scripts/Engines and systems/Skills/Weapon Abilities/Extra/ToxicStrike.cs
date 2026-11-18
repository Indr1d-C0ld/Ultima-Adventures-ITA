using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Spells;

namespace Server.Items
{
        public class ToxicStrike : WeaponAbility
        {
                private const int MaxStacks = 3;
                private const int ResistancePenalty = -10;
                private static readonly TimeSpan DebuffDuration = TimeSpan.FromSeconds(20.0);

                private static readonly Dictionary<Mobile, DebuffEntry> m_DebuffTable = new Dictionary<Mobile, DebuffEntry>();
                private static readonly Dictionary<Mobile, int> m_HitCounter = new Dictionary<Mobile, int>();

                public ToxicStrike(){}

                public override int BaseMana { get { return 20; } }

                public override void OnHit(Mobile attacker, Mobile defender, int damage)
                {
                        if (!Validate(attacker) || !CheckMana(attacker, true) || defender == null)
                                return;

                        ClearCurrentAbility(attacker);
                        attacker.SendMessage("You hurt them with a sickly blow!");
                        defender.SendMessage("You feel sickly from the blow!");

                        BaseWeapon weapon = attacker.Weapon as BaseWeapon;
                        if (weapon == null)
                                return;

                        Skill skill = attacker.Skills[weapon.Skill];
                        double reqSkill = GetRequiredSkill(attacker);
                        double skilltouse = 0.0;
                        if (skill != null)
                                skilltouse = skill.Value;

                        if (weapon.WeaponAttributes.UseBestSkill > 0)
                        {
                                double skilltouse2 = 0.0;
                                if ( attacker.Skills[SkillName.Swords].Value >= reqSkill ) skilltouse2 = attacker.Skills[SkillName.Swords].Value;
                                if ( attacker.Skills[SkillName.Macing].Value >= reqSkill && attacker.Skills[SkillName.Macing].Value > skilltouse2 ) skilltouse2 = attacker.Skills[SkillName.Macing].Value;
                                if ( attacker.Skills[SkillName.Fencing].Value >= reqSkill && attacker.Skills[SkillName.Fencing].Value > skilltouse2 ) skilltouse2 = attacker.Skills[SkillName.Fencing].Value;
                                if ( attacker.Skills[SkillName.Lumberjacking].Value >= reqSkill && attacker.Skills[SkillName.Lumberjacking].Value > skilltouse2 ) skilltouse2 = attacker.Skills[SkillName.Lumberjacking].Value;
                                if (skilltouse2 > skilltouse)
                                        skilltouse = skilltouse2;
                        }

                        int todam = (int)(skilltouse / 20);
                        AOS.Damage( defender, attacker, (Utility.RandomMinMax(15, 35) + todam), true, 0, 0, 0, 100, 0 );

                        ApplyPoisonDebuff(defender);
                        HandlePoisonNova(attacker, defender);

                        base.OnHit(attacker, defender, damage);
                }

                private static void ApplyPoisonDebuff(Mobile defender)
                {
                        if (defender == null || defender.Deleted)
                                return;

                        DebuffEntry entry;

                        if (!m_DebuffTable.TryGetValue(defender, out entry))
                        {
                                entry = new DebuffEntry(defender);
                                m_DebuffTable[defender] = entry;
                        }

                        if (entry.StackCount < MaxStacks)
                                entry.AddStack();
                        else
                                entry.RefreshStacks();

                        BuffInfo.AddBuff( defender, new BuffInfo( BuffIcon.ToxicStrike, 1151474, 1151475, DebuffDuration, defender, entry.StackCount.ToString() ) );
                }

                private static void HandlePoisonNova(Mobile attacker, Mobile defender)
                {
                        if (attacker == null || attacker.Deleted)
                                return;

                        int current;
                        if (!m_HitCounter.TryGetValue(attacker, out current))
                                current = 0;

                        current++;

                        if (current >= 3)
                        {
                                CreatePoisonNova(attacker, defender);
                                current = 0;
                        }

                        if (current == 0)
                                m_HitCounter.Remove(attacker);
                        else
                                m_HitCounter[attacker] = current;
                }

                private static void CreatePoisonNova(Mobile attacker, Mobile defender)
                {
                        if (defender == null || defender.Deleted || defender.Map == null)
                                return;

                        Mobile check = defender;

                        if (check == null || check.Deleted)
                                check = attacker;

                        Map map = defender.Map;
                        if (check != null && MonsterSplatter.TooMuchSplatter(check))
                                return;

                        Point3D start = defender.Location;
                        int hue = 0x3F;
                        int effectId = 0x36D4;
                        int speed = 7;
                        HashSet<Mobile> hitTargets = new HashSet<Mobile>();

                        Point3D[] directions = new Point3D[]
                        {
                                new Point3D(0, -1, 0),
                                new Point3D(1, -1, 0),
                                new Point3D(1, 0, 0),
                                new Point3D(1, 1, 0),
                                new Point3D(0, 1, 0),
                                new Point3D(-1, 1, 0),
                                new Point3D(-1, 0, 0),
                                new Point3D(-1, -1, 0)
                        };

                        for (int i = 0; i < directions.Length; i++)
                        {
                                int endX = start.X + (directions[i].X * 8);
                                int endY = start.Y + (directions[i].Y * 8);
                                Point3D end = new Point3D(endX, endY, start.Z);

                                Effects.SendMovingEffect(
                                        new Entity(Serial.Zero, start, map),
                                        new Entity(Serial.Zero, end, map),
                                        effectId,
                                        speed,
                                        0,
                                        false,
                                        false,
                                        hue,
                                        0);

                                for (int step = 1; step <= 8; step++)
                                {
                                        int checkX = start.X + (directions[i].X * step);
                                        int checkY = start.Y + (directions[i].Y * step);
                                        Point3D checkPoint = new Point3D(checkX, checkY, start.Z);

                                        IPooledEnumerable eable = map.GetMobilesInRange(checkPoint, 0);
                                        foreach (Mobile target in eable)
                                        {
                                                if (target == null || target.Deleted || target == attacker)
                                                        continue;

                                                if (hitTargets.Contains(target))
                                                        continue;

                                                if (!SpellHelper.ValidIndirectTarget(attacker, target) || !attacker.CanBeHarmful(target, false))
                                                        continue;

                                                hitTargets.Add(target);
                                                attacker.DoHarmful(target);

                                                int damage = Utility.RandomMinMax(20, 40);
                                                AOS.Damage(target, attacker, damage, true, 0, 0, 0, 100, 0);

                                                int poisonLevel = Math.Min( 4, attacker.Skills[SkillName.Poisoning].Fixed / 200 ); // corretto: a 100+ dava il livello 5 inesistente (nessun veleno)
                                                Poison poison = Poison.GetPoison(poisonLevel);
                                                if (poison != null)
                                                        target.ApplyPoison(attacker, poison);
                                        }
                                        eable.Free();
                                }
                        }

                        attacker.SendMessage("Poison erupts outward in a noxious nova!");
                }

                private class DebuffEntry
                {
                        private readonly Mobile m_Defender;
                        private readonly List<ExpireTimer> m_Timers = new List<ExpireTimer>();

                        public DebuffEntry(Mobile defender)
                        {
                                m_Defender = defender;
                        }

                        public int StackCount
                        {
                                get { return m_Timers.Count; }
                        }

                        public void AddStack()
                        {
                                if (m_Defender == null || m_Defender.Deleted)
                                        return;

                                ResistanceMod mod = new ResistanceMod(ResistanceType.Poison, ResistancePenalty);
                                m_Defender.AddResistanceMod(mod);

                                ExpireTimer timer = new ExpireTimer(this, mod);
                                timer.Start();
                                m_Timers.Add(timer);
                        }

                        public void RefreshStacks()
                        {
                                for (int i = 0; i < m_Timers.Count; i++)
                                {
                                        m_Timers[i].Refresh();
                                }
                        }

                        public void Expire(ExpireTimer timer)
                        {
                                if (timer == null)
                                        return;

                                if (m_Defender != null && !m_Defender.Deleted)
                                        m_Defender.RemoveResistanceMod(timer.Mod);

                                timer.Stop();
                                m_Timers.Remove(timer);

                                if (m_Timers.Count == 0)
                                {
                                        m_DebuffTable.Remove(m_Defender);
                                        BuffInfo.RemoveBuff( m_Defender, BuffIcon.ToxicStrike );
                                }
                        }
                }

                private class ExpireTimer : Timer
                {
                        private readonly DebuffEntry m_Entry;

                        public ResistanceMod Mod { get; private set; }

                        public ExpireTimer(DebuffEntry entry, ResistanceMod mod)
                                : base(DebuffDuration)
                        {
                                m_Entry = entry;
                                Mod = mod;
                                Priority = TimerPriority.TwoFiftyMS;
                        }

                        protected override void OnTick()
                        {
                                m_Entry.Expire(this);
                        }

                        public void Refresh()
                        {
                                Stop();
                                Delay = DebuffDuration;
                                Start();
                        }
                }
        }
}
