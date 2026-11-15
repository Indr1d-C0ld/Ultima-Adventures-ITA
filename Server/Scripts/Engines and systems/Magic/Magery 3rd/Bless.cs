using System;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;
using Server.Spells.Fourth;

namespace Server.Spells.Third
{
	public class BlessSpell : MagerySpell
	{
		private const string BlessStateName = "[Magic] Bless State";

		private static SpellInfo m_Info = new SpellInfo(
				"Bless", "Rel Sanct",
				203,
				9061,
				Reagent.Garlic,
				Reagent.MandrakeRoot
			);

		public override SpellCircle Circle { get { return SpellCircle.Third; } }

		public BlessSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if ( CheckBSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );
				
				if (m is PlayerMobile && CurseSpell.UnderEffect( m ))
				{
					if (Utility.RandomBool())
					{
						CurseSpell.RemoveEffect(m);
                        BuffInfo.RemoveBuff(m, BuffIcon.Curse);
						m.SendMessage("You are no longer cursed!");
						m.UpdateResistances();
					}
					else
						m.SendMessage("You try to remove your curse, but fail!");
				}
					
				else 
				{

							double scalar = SpellHelper.GetOffsetScalar( Caster, m, false );
							TimeSpan length = SpellHelper.GetDuration( Caster, m );

							double bonusMultiplier = 1.0;

							if ( Caster is PlayerMobile && ((PlayerMobile)Caster).Sorcerer() )
							{
								bonusMultiplier += (Caster.Skills[SkillName.Magery].Value + Caster.Skills[SkillName.EvalInt].Value) / 180.0;
								length += TimeSpan.FromSeconds( 120 );
							}

							double adjustedScalar = scalar * bonusMultiplier;

							if ( Core.AOS && !SpellHelper.DisableSkillCheck )
								Caster.CheckSkill( SkillName.EvalInt, 0.0, 120.0 );

							int strBonus;
							int dexBonus;
							int intBonus;

							if ( Core.AOS )
							{
								strBonus = SpellHelper.GetStatOffset( m, StatType.Str, adjustedScalar );
								dexBonus = SpellHelper.GetStatOffset( m, StatType.Dex, adjustedScalar );
								intBonus = SpellHelper.GetStatOffset( m, StatType.Int, adjustedScalar );
							}
							else
							{
								bool oldDisableSkillCheck = SpellHelper.DisableSkillCheck;

								strBonus = (int)(SpellHelper.GetOffset( Caster, m, StatType.Str, false ) * bonusMultiplier);
								SpellHelper.DisableSkillCheck = true;
								dexBonus = (int)(SpellHelper.GetOffset( Caster, m, StatType.Dex, false ) * bonusMultiplier);
								intBonus = (int)(SpellHelper.GetOffset( Caster, m, StatType.Int, false ) * bonusMultiplier);
								SpellHelper.DisableSkillCheck = oldDisableSkillCheck;
							}

							int strAppliedBonus;
							int dexAppliedBonus;
							int intAppliedBonus;
							TimeSpan strAppliedDuration;
							TimeSpan dexAppliedDuration;
							TimeSpan intAppliedDuration;

							SpellHelper.AddStatBonus( Caster, m, StatType.Str, strBonus, length, out strAppliedBonus, out strAppliedDuration );
							SpellHelper.AddStatBonus( Caster, m, StatType.Dex, dexBonus, length, out dexAppliedBonus, out dexAppliedDuration );
							SpellHelper.AddStatBonus( Caster, m, StatType.Int, intBonus, length, out intAppliedBonus, out intAppliedDuration );

							m.FixedParticles( 0x373A, 10, 15, 5018, Server.Items.CharacterDatabase.GetMySpellHue( Caster, 0 ), 0, EffectLayer.Waist );
							m.PlaySound( 0x1EA );

							TimeSpan blessDuration = GetLongestDuration( strAppliedDuration, dexAppliedDuration, intAppliedDuration );

							if ( m is BaseCreature )
								ApplyCreatureBlessState( (BaseCreature)m, blessDuration );

							string args = String.Format( "{0}\t{1}\t{2}", strAppliedBonus, dexAppliedBonus, intAppliedBonus );

							BuffInfo.AddBuff( m, new BuffInfo( BuffIcon.Bless, 1075847, 1075848, blessDuration, m, args ) );
						}
			}

			FinishSequence();
		}

		private static TimeSpan GetLongestDuration( TimeSpan first, TimeSpan second, TimeSpan third )
		{
			if ( second > first )
				first = second;

			if ( third > first )
				first = third;

			return first;
		}

		private static void ApplyCreatureBlessState( BaseCreature creature, TimeSpan duration )
		{
			BlessStateMod oldState = creature.GetStatMod( BlessStateName ) as BlessStateMod;

			if ( oldState != null )
				oldState.Stop();

			BlessStateMod state = new BlessStateMod( creature, duration );
			creature.AddStatMod( state );
			creature.IsBlessed = true;
			state.Start();
		}

		private sealed class BlessStateMod : StatMod
		{
			private BaseCreature m_Creature;
			private BlessedStateTimer m_Timer;

			public BlessStateMod( BaseCreature creature, TimeSpan duration )
				: base( StatType.Str, BlessStateName, 0, TimeSpan.Zero )
			{
				m_Creature = creature;
				m_Timer = new BlessedStateTimer( this, duration );
			}

			public void Start()
			{
				m_Timer.Start();
			}

			public void Stop()
			{
				if ( m_Timer != null )
				{
					m_Timer.Stop();
					m_Timer = null;
				}
			}

			public void Expire()
			{
				m_Timer = null;

				if ( !m_Creature.Deleted && m_Creature.GetStatMod( BlessStateName ) == this )
				{
					m_Creature.RemoveStatMod( BlessStateName );
					m_Creature.IsBlessed = false;
				}
			}
		}

		private sealed class BlessedStateTimer : Timer
		{
			private BlessStateMod m_State;

			public BlessedStateTimer( BlessStateMod state, TimeSpan duration )
				: base( duration )
			{
				m_State = state;
			}

			protected override void OnTick()
			{
				m_State.Expire();
			}
		}

		public class InternalTarget : Target
		{
			private BlessSpell m_Owner;

			public InternalTarget( BlessSpell owner ) : base( Core.ML ? 10 : 12, false, TargetFlags.Beneficial )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
				{
					m_Owner.Target( (Mobile)o );
				}
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
