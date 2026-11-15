using System;
using System.Collections;
using System.Text;
using Server;
using Server.Mobiles;
using Server.Targeting;
using Server.Items;
using Server.Network;

namespace Server.SkillHandlers
{
	public class ForensicEvaluation
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Forensics].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			m.Target = new ForensicTarget();
			m.RevealingAction();

			m.SendLocalizedMessage( 501000 ); // Show me the crime.

			return TimeSpan.FromSeconds( 1.0 );
		}

		private static void ReportLastTouched( Mobile from, Item item )
		{
			if ( item.LastTouched != "" )
				from.SendMessage( "The last person to touch this item was " + item.LastTouched );
		}

		public class ForensicTarget : Target
		{
			public ForensicTarget() : base( 10, false, TargetFlags.None )
			{
			}

			protected override void OnTarget(Mobile from, object target)
			{
				if (target is Mobile)
				{
					if (from.CheckTargetSkill(SkillName.Forensics, target, 40.0, 100.0))
					{
						if (target is PlayerMobile && ((PlayerMobile)target).NpcGuild == NpcGuild.ThievesGuild)
							from.SendLocalizedMessage(501004);//That individual is a thief!
						else
							from.SendLocalizedMessage(501003);//You notice nothing unusual.
					}
					else
					{
						from.SendLocalizedMessage(501001);//You cannot determain anything useful.
					}
				}
				else if (target is Coffer)
				{
					Coffer coffer = (Coffer)target;

					if (coffer.CofferRobbed > 0)
					{
						from.SendMessage("It seems that " + coffer.CofferRobber + " has robbed this coffer of it's gold!");
					}
					else
					{
						from.SendMessage("That coffer has not been emptied by thieves.");
					}
				}
				else if (target is LandChest && LandChest.isBody(((Item)target).ItemID))
				{
					from.SendMessage("This adventurer looks to have been slain by some wild animal.");
				}
				else if (target is LandChest && !LandChest.isBody(((Item)target).ItemID))
				{
					from.SendMessage("For some reason, this wagon was left behind.");
				}
				else if (target is EssenceBones)
				{
					EssenceBones bones = (EssenceBones)target;

					// Require 100.0+ Forensics to reveal
					if (from.Skills[SkillName.Forensics].Value >= 110.0)
					{
						bones.Identified = true;         // permanently reveal going forward
						bones.InvalidateProperties();    // refresh property list

						if (bones.Mob != null)
						{
							from.PrivateOverheadMessage(
								MessageType.Regular, 0x3B2, false,
								"These bones belonged to a " + bones.Mob.Name + ".",
								from.NetState
							);
						}
						else
						{
							from.PrivateOverheadMessage(
								MessageType.Regular, 0x3B2, false,
								"You identify the essence, but the specific origin cannot be determined.",
								from.NetState
							);
						}
					}
					else
					{
						from.PrivateOverheadMessage(
							MessageType.Regular, 0x3B2, false,
							"You sense something unusual about these bones, but can't determine what.",
							from.NetState
						);
					}

					ReportLastTouched( from, bones );
				}
				else if (target is WaterChest)
				{
					from.SendMessage("Maybe the owner of this boat fell into the sea and drowned.");
				}
				else if (target is SunkenShip)
				{
					from.SendMessage("This ship looks as though it seen better days.");
				}
				else if (target is Corpse)
				{
					if (from.CheckTargetSkill(SkillName.Forensics, target, 0.0, 100.0))
					{
						Corpse c = (Corpse)target;

						if (c.m_Forensicist != null)
							from.SendLocalizedMessage(1042750, c.m_Forensicist); // The forensicist  ~1_NAME~ has already discovered that:
						else
							c.m_Forensicist = from.Name;

						if (((Body)c.Amount).IsHuman)
							from.SendLocalizedMessage(1042751, (c.Killer == null ? "no one" : c.Killer.Name));//This person was killed by ~1_KILLER_NAME~

                                                if (c.Looters.Count > 0)
                                                {
                                                        StringBuilder sb = new StringBuilder();
                                                        for (int i = 0; i < c.Looters.Count; i++)
                                                        {
                                                                if (i > 0)
                                                                        sb.Append(", ");
                                                                sb.Append(((Mobile)c.Looters[i]).Name);
                                                        }

                                                        from.SendLocalizedMessage(1042752, sb.ToString());//This body has been distrubed by ~1_PLAYER_NAMES~
                                                }
                                                else
                                                {
                                                        from.SendLocalizedMessage(501002);//The corpse has not be desecrated.
                                                }

                                                double skill = from.Skills[SkillName.Forensics].Value;
                                                if (skill >= 60.0)
                                                {
                                                        from.SendMessage("Time of death: " + c.TimeOfDeath.ToString());
                                                }

                                                if (skill >= 90.0)
                                                {
                                                        if (c.Criminals != null && c.Criminals.Count > 0)
                                                        {
                                                                StringBuilder csb = new StringBuilder();
                                                                for (int i = 0; i < c.Criminals.Count; i++)
                                                                {
                                                                        if (i > 0)
                                                                                csb.Append(", ");
                                                                        csb.Append(c.Criminals[i].Name);
                                                                }

                                                                from.SendMessage("Criminal flag(s): " + csb.ToString());
                                                        }
                                                        else
                                                        {
                                                                from.SendMessage("No criminal activity has been recorded.");
                                                        }
                                                }
					}
					else
					{
						from.SendLocalizedMessage(501001);//You cannot determine anything useful.
					}
				}
				else if (target is ILockpickable)
				{
					ILockpickable p = (ILockpickable)target;
					if (p.Picker != null)
						from.SendLocalizedMessage(1042749, p.Picker.Name);//This lock was opened by ~1_PICKER_NAME~
					else
						from.SendLocalizedMessage(501003);//You notice nothing unusual.
				}
				else if ( target is Item )
					ReportLastTouched( from, (Item)target );
					
			}
		}
	}
}
