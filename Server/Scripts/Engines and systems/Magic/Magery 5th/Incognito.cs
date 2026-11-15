using System;
using System.Collections;
using Server;
using Server.Mobiles;
using Server.Misc;
using Server.Items;
using Server.Gumps;
using Server.Spells;
using Server.Spells.Seventh;

namespace Server.Spells.Fifth
{
	public class IncognitoSpell : MagerySpell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Incognito", "Kal In Ex",
				206,
				9002,
				Reagent.Bloodmoss,
				Reagent.Garlic,
				Reagent.Nightshade
			);

		public override SpellCircle Circle { get { return SpellCircle.Fifth; } }

		public IncognitoSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override bool CheckCast(Mobile caster)
        {
            if (!base.CheckCast(caster))
                return false;

			if ( Caster.BodyMod == 183 || Caster.BodyMod == 184 )
			{
				Caster.SendLocalizedMessage( 1042402 ); // You cannot use incognito while wearing body paint
				return false;
			}

			return true;
		}

		public override void OnCast()
		{
			if ( Caster.BodyMod == 183 || Caster.BodyMod == 184 )
			{
				Caster.SendLocalizedMessage( 1042402 ); // You cannot use incognito while wearing body paint
			}
			else if ( DisguiseTimers.IsDisguised( Caster ) )
			{
				Caster.SendLocalizedMessage( 1061631 ); // You can't do that while disguised.
			}
			else if ( !Caster.CanBeginAction( typeof( PolymorphSpell ) ) || Caster.IsBodyMod )
			{
				DoFizzle();
			}
			else if ( CheckSequence() )
			{
				bool refresh = !Caster.CanBeginAction( typeof( IncognitoSpell ) );

				if ( refresh || Caster.BeginAction( typeof( IncognitoSpell ) ) )
				{
					if ( !refresh )
					{
						DisguiseTimers.StopTimer( Caster );

						Caster.HueMod = Caster.Race.RandomSkinHue();
						Caster.NameMod = Caster.Female ? NameList.RandomName( "female" ) : NameList.RandomName( "male" );

						PlayerMobile pm = Caster as PlayerMobile;

						if ( pm != null && pm.Race != null )
						{
							pm.SetHairMods( pm.Race.RandomHair( pm.Female ), pm.Race.RandomFacialHair( pm.Female ) );
							pm.HairHue = pm.Race.RandomHairHue();
							pm.FacialHairHue = pm.Race.RandomHairHue();
						}

						Caster.FixedParticles( 0x373A, 10, 15, 5036, Server.Items.CharacterDatabase.GetMySpellHue( Caster, 0 ), 0, EffectLayer.Head );
						Caster.PlaySound( 0x3BD );

						BaseArmor.ValidateMobile( Caster );
						BaseClothing.ValidateMobile( Caster );
					}

                                       StopTimer( Caster, false );


					int timeVal = ((6 * Caster.Skills.Magery.Fixed) / 50) + 1;

					if( timeVal > 144 )
						timeVal = 144;

					TimeSpan length = TimeSpan.FromSeconds( timeVal );


					Timer t = new InternalTimer( Caster, length );

					m_Timers[Caster] = t;

					t.Start();

					BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.Disguised, 1075821, 1075820, length, Caster ) );

				}
				else
				{
					Caster.SendLocalizedMessage( 1079022 ); // You're already incognitoed!
				}
			}

			FinishSequence();
		}

		public static Hashtable m_Timers = new Hashtable();

               public static bool StopTimer( Mobile m )
               {
                       return StopTimer( m, true );
               }

               public static bool StopTimer( Mobile m, bool releaseEffect )
               {
                       Timer t = (Timer)m_Timers[m];

                       if ( t != null )
                       {
                               t.Stop();
                               m_Timers.Remove( m );
                       }

                       if ( releaseEffect )
                       {
                               if ( !m.CanBeginAction( typeof( IncognitoSpell ) ) )
                               {
                                       ClearIncognito( m );
                               }
                               else
                               {
									BuffInfo.RemoveBuff( m, BuffIcon.Disguised );
                               }
                       }

                       return ( t != null );
               }

		private static void ClearIncognito( Mobile m )
		{
			if ( m is PlayerMobile )
				((PlayerMobile)m).SetHairMods( -1, -1 );

			m.BodyMod = 0;
			m.HueMod = -1;
			m.NameMod = null;

			BaseArmor.ValidateMobile( m );
			BaseClothing.ValidateMobile( m );

			BuffInfo.RemoveBuff( m, BuffIcon.Disguised );
			m.EndAction( typeof( IncognitoSpell ) );
		}

		private static int[] m_HairIDs = new int[]
			{
				0x2044, 0x2045, 0x2046,
				0x203C, 0x203B, 0x203D,
				0x2047, 0x2048, 0x2049,
				0x204A, 0x0000
			};

		private static int[] m_BeardIDs = new int[]
			{
				0x203E, 0x203F, 0x2040,
				0x2041, 0x204B, 0x204C,
				0x204D, 0x0000
			};

		private class InternalTimer : Timer
		{
			private Mobile m_Owner;

			public InternalTimer( Mobile owner, TimeSpan length ) : base( length )
			{
				m_Owner = owner;

				/*
				int val = ((6 * owner.Skills.Magery.Fixed) / 50) + 1;

				if ( val > 144 )
					val = 144;

				Delay = TimeSpan.FromSeconds( val );
				 * */
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				if ( m_Timers[m_Owner] != this )
					return;

				StopTimer( m_Owner );
			}
		}
	}
}
