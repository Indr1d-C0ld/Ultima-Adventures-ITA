// $Id: //depot/c%23/RunUO Core Scripts/RunUO Core Scripts/Items/Weapons/Abilities/PsychicAttack.cs#2 $

using System;
using Server;
using System.Collections.Generic;
using Server.Spells;

namespace Server.Items
{
	public class PsychicAttack : WeaponAbility
	{
		private const int BacklashDamage = 25;
		private static readonly TimeSpan RestoreDelay = TimeSpan.FromSeconds(5.0);
		private static readonly TimeSpan BacklashDuration = TimeSpan.FromSeconds(5.0);
		private static readonly TimeSpan BuffDuration = RestoreDelay + BacklashDuration;

		private static readonly Dictionary<Mobile, PsychicDebuffContext> m_DebuffTable = new Dictionary<Mobile, PsychicDebuffContext>();

		public PsychicAttack()
		{
		}

		public override int BaseMana { get { return 30; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true))
				return;

			ClearCurrentAbility(attacker);

			attacker.SendLocalizedMessage(1074383); // Your shot sends forth a wave of psychic energy.
			defender.SendLocalizedMessage(1074384); // Your mind is attacked by psychic force!

			//defender.Mana -= Utility.Random( (int)attacker.Skills[SkillName.Anatomy].Value/10, (int)attacker.Skills[SkillName.Anatomy].Value/5 );

			int toDrain = defender.Mana;

			if (toDrain < 0)
				toDrain = 0;

			PsychicDebuffContext context;

			if (!m_DebuffTable.TryGetValue(defender, out context))
			{
				context = new PsychicDebuffContext(defender);
				m_DebuffTable[defender] = context;
			}
			else
			{
				toDrain = 0;
			}

			defender.FixedParticles(0x3789, 10, 25, 5032, EffectLayer.Head);
			defender.PlaySound(0x1F8);

			context.Apply(attacker, toDrain);
			BuffInfo.AddBuff( defender, new BuffInfo( BuffIcon.PsychicAttack, 1028864, 1028864, BuffDuration, defender ) );
		}

		private class PsychicDebuffContext
		{
			private readonly Mobile m_Defender;
			private Mobile m_Attacker;
			private int m_ManaToRestore;
			private Timer m_RestoreTimer;
			private Timer m_BacklashTimer;
			private SpellWatcher m_Watcher;
			private Spell m_LastSpell;
			private bool m_BacklashActive;

			public PsychicDebuffContext(Mobile defender)
			{
				m_Defender = defender;
			}

			public void Apply(Mobile attacker, int manaDrained)
			{
				m_Attacker = attacker;

				StopBacklash();

				if (manaDrained > 0)
				{
					manaDrained = Math.Min(manaDrained, m_Defender.Mana);

					if (manaDrained > 0)
					{
						m_ManaToRestore = manaDrained;
						m_Defender.Mana -= manaDrained;
					}
				}

				RestartRestoreTimer();
			}

			private void RestartRestoreTimer()
			{
				Timer old = m_RestoreTimer;

				m_RestoreTimer = new RestoreTimer(this);
				m_RestoreTimer.Start();

				if (old != null)
					old.Stop();
			}

			public void Restore()
			{
				StopRestoreTimer();

				if (m_Defender.Deleted || !m_Defender.Alive || m_Defender.IsDeadBondedPet)
				{
					m_ManaToRestore = 0;
					End();
					return;
				}

				if (m_ManaToRestore > 0)
				{
					m_Defender.Mana += m_ManaToRestore;
					m_Defender.FixedEffect(0x3779, 10, 25);
					m_Defender.PlaySound(0x28E);
				}

				m_ManaToRestore = 0;

				BeginBacklash();
			}

			private void StopRestoreTimer()
			{
				if (m_RestoreTimer != null)
				{
					m_RestoreTimer.Stop();
					m_RestoreTimer = null;
				}
			}

			private void BeginBacklash()
			{
				m_BacklashActive = true;
				EnsureWatcher();

				Timer old = m_BacklashTimer;

				m_BacklashTimer = new BacklashTimer(this);
				m_BacklashTimer.Start();

				if (old != null)
					old.Stop();
			}

			private void StopBacklash()
			{
				m_BacklashActive = false;
				StopWatcher();

				if (m_BacklashTimer != null)
				{
					m_BacklashTimer.Stop();
					m_BacklashTimer = null;
				}
			}

			private void EnsureWatcher()
			{
				if (m_Watcher == null)
				{
					m_Watcher = new SpellWatcher(this);
				}

				m_Watcher.Start();
			}

			private void StopWatcher()
			{
				if (m_Watcher != null)
				{
					m_Watcher.Stop();
					m_Watcher = null;
				}
			}

			public void End()
			{
				StopRestoreTimer();
				StopBacklash();

				m_LastSpell = null;
				m_Attacker = null;
				m_BacklashActive = false;

				m_DebuffTable.Remove(m_Defender);
				BuffInfo.RemoveBuff( m_Defender, BuffIcon.PsychicAttack );
			}

			public void OnSpellTick()
			{
				if (m_Defender.Deleted)
				{
					End();
					return;
				}

				if (!m_Defender.Alive || m_Defender.IsDeadBondedPet)
				{
					End();
					return;
				}

				if (!m_BacklashActive)
					return;

				Spell spell = m_Defender.Spell as Spell;

				if (spell == null || !spell.IsCasting)
				{
					m_LastSpell = null;
					return;
				}

				if (spell == m_LastSpell)
					return;

				m_LastSpell = spell;

				TriggerBacklash(spell);
			}

			private void TriggerBacklash(Spell spell)
			{
				if (m_Defender == null || !m_BacklashActive)
					return;

				m_Defender.SendMessage("A psychic backlash wracks your mind as you attempt to cast!");
				m_Defender.FixedParticles(0x374A, 10, 15, 5028, EffectLayer.Head);
				m_Defender.PlaySound(0x1F8);
				m_Defender.Damage(BacklashDamage, m_Attacker);

				if (spell != null)
					spell.Disturb(DisturbType.Hurt);
			}

			private class RestoreTimer : Timer
			{
				private readonly PsychicDebuffContext m_Context;

				public RestoreTimer(PsychicDebuffContext context) : base(RestoreDelay)
				{
					m_Context = context;
					Priority = TimerPriority.TwoFiftyMS;
				}

				protected override void OnTick()
				{
					m_Context.Restore();
				}
			}

			private class BacklashTimer : Timer
			{
				private readonly PsychicDebuffContext m_Context;

				public BacklashTimer(PsychicDebuffContext context) : base(BacklashDuration)
				{
					m_Context = context;
					Priority = TimerPriority.TwoFiftyMS;
				}

				protected override void OnTick()
				{
					m_Context.End();
				}
			}

			private class SpellWatcher : Timer
			{
				private readonly PsychicDebuffContext m_Context;

				public SpellWatcher(PsychicDebuffContext context) : base(TimeSpan.Zero, TimeSpan.FromMilliseconds(150.0))
				{
					m_Context = context;
					Priority = TimerPriority.TwentyFiveMS;
				}

				protected override void OnTick()
				{
					m_Context.OnSpellTick();
				}
			}
		}
	}
}
