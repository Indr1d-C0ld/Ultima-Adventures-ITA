using System;
using Server;

namespace Server.Items
{
	public class ElementalStrike : WeaponAbility
	{
		private const string ColdDexModName = "ElementalStrikeColdDex";

		public ElementalStrike()
		{
		}

		public override int BaseMana { get { return 20; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true))
				return;

			ClearCurrentAbility(attacker);

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

				if (attacker.Skills[SkillName.Swords].Value >= reqSkill)
					skilltouse2 = attacker.Skills[SkillName.Swords].Value;

				if (attacker.Skills[SkillName.Macing].Value >= reqSkill && attacker.Skills[SkillName.Macing].Value > skilltouse2)
					skilltouse2 = attacker.Skills[SkillName.Macing].Value;

				if (attacker.Skills[SkillName.Fencing].Value >= reqSkill && attacker.Skills[SkillName.Fencing].Value > skilltouse2)
					skilltouse2 = attacker.Skills[SkillName.Fencing].Value;

				if (attacker.Skills[SkillName.Lumberjacking].Value >= reqSkill && attacker.Skills[SkillName.Lumberjacking].Value > skilltouse2)
					skilltouse2 = attacker.Skills[SkillName.Lumberjacking].Value;

				if (skilltouse2 > skilltouse)
					skilltouse = skilltouse2;
			}

			int baseDamage = Utility.RandomMinMax(15, 35) + (int)(skilltouse / 20);

			if (baseDamage < 1)
				baseDamage = 1;

			int elementalDamage = (int)Math.Round(baseDamage * 1.5);

			if (elementalDamage < 1)
				elementalDamage = 1;

			switch (Utility.Random(4))
			{
				case 0:
					ApplyFireEffect(attacker, defender, elementalDamage);
					break;
				case 1:
					ApplyColdEffect(attacker, defender, elementalDamage);
					break;
				case 2:
					ApplyPoisonEffect(attacker, defender, elementalDamage);
					break;
				default:
					ApplyEnergyEffect(attacker, defender, elementalDamage);
					break;
			}

			base.OnHit(attacker, defender, damage);
		}

		private static void ApplyFireEffect(Mobile attacker, Mobile defender, int damage)
		{
			AOS.Damage(defender, attacker, damage, true, 0, 100, 0, 0, 0);
			attacker.SendMessage("Your elemental strike engulfs your foe in flames!");
			defender.SendMessage("You are scorched by chaotic fire!");

			BuffInfo buff = null;

			if (defender.Player)
			{
				buff = new BuffInfo(BuffIcon.ElementalStrikeBurn, 1151526, 1151527, TimeSpan.FromSeconds(3.0), defender);
				BuffInfo.AddBuff(defender, buff);
			}

			new FireDoTTimer(attacker, defender, Math.Max(1, (int)Math.Round(damage * 0.1)), buff).Start();
		}

		private static void ApplyColdEffect(Mobile attacker, Mobile defender, int damage)
		{
			AOS.Damage(defender, attacker, damage, true, 0, 0, 100, 0, 0);
			attacker.SendMessage("Your elemental strike freezes your foe in place!");
			defender.SendMessage("Biting cold saps your speed!");

			defender.RemoveStatMod(ColdDexModName);
			int dexLoss = (int)Math.Ceiling(defender.RawDex * 0.7);

			if (dexLoss < 1)
				dexLoss = 1;

			defender.AddStatMod(new StatMod(StatType.Dex, ColdDexModName, -dexLoss, TimeSpan.FromSeconds(3.0)));

			if (defender.Player)
				BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.ElementalStrikeCold, 1151518, 1151519, TimeSpan.FromSeconds(3.0), defender, dexLoss.ToString()));
		}

		private static void ApplyPoisonEffect(Mobile attacker, Mobile defender, int damage)
		{
			AOS.Damage(defender, attacker, damage, true, 0, 0, 0, 100, 0);
			attacker.SendMessage("Toxic energy courses through your strike!");
			defender.SendMessage("Venom seeps into your veins!");

			int poisoningSkill = attacker.Skills[SkillName.Poisoning].Fixed;
			int poisonLevel = poisoningSkill / 200;

			if (poisonLevel > 4)
				poisonLevel = 4;

			if (poisonLevel < 0)
				poisonLevel = 0;

			Poison poison = Poison.GetPoison(poisonLevel);

			if (poison != null)
				defender.ApplyPoison(attacker, poison);
		}

		private static void ApplyEnergyEffect(Mobile attacker, Mobile defender, int damage)
		{
			AOS.Damage(defender, attacker, damage, true, 0, 0, 0, 0, 100);
			attacker.SendMessage("Raw energy lashes out, stunning your foe!");
			defender.SendMessage("Chaotic energy paralyzes you!");
			defender.Paralyze(TimeSpan.FromSeconds(1.0));
		}

		private class FireDoTTimer : Timer
		{
			private readonly Mobile m_Attacker;
			private readonly Mobile m_Defender;
			private readonly int m_TickDamage;
			private readonly BuffInfo m_Buff;
			private int m_Ticks;

			public FireDoTTimer(Mobile attacker, Mobile defender, int tickDamage, BuffInfo buff) : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), 3)
			{
				m_Attacker = attacker;
				m_Defender = defender;
				m_TickDamage = tickDamage;
				m_Buff = buff;
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				if (m_Defender.Deleted || !m_Defender.Alive)
				{
					BuffInfo.RemoveBuff(m_Defender, m_Buff);
					Stop();
					return;
				}

				m_Ticks++;
				AOS.Damage(m_Defender, m_Attacker, m_TickDamage, true, 0, 100, 0, 0, 0);

				if (m_Ticks >= 3)
					Stop();
			}
		}
	}
}
