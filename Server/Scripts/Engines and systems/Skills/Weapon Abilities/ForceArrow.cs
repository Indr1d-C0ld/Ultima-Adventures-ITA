// $Id: //depot/c%23/RunUO Core Scripts/RunUO Core Scripts/Items/Weapons/Abilities/ForceArrow.cs#2 $

using System;
using Server;
using System.Collections;
using Server.Spells;

namespace Server.Items
{
	public class ForceArrow : WeaponAbility
	{
		public ForceArrow()
		{
		}

                public override int BaseMana { get { return 20; } }
                public override double DamageScalar { get { return 1.3; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true))
				return;

			ClearCurrentAbility(attacker);

			attacker.SendLocalizedMessage(1074381); // You fire an arrow of pure force.
			defender.SendLocalizedMessage(1074382); // You are struck by a force arrow!

                        if (Utility.RandomDouble() >= attacker.Skills[SkillName.Anatomy].Value / 600)
                        {
                                defender.Warmode = false;
                                ApplyKnockback(attacker, defender, 2);
                                //attacker.SendMessage("Mobile forget who are attacking.");
                        }
                        DoLowerDefense(attacker, defender);
                }

                public virtual void DoLowerDefense(Mobile from, Mobile defender)
                {
                        if (HitLower.ApplyDefense(defender))
			{
				defender.PlaySound(0x28E);
                                Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0x23, 3);
                        }
                }

                private void ApplyKnockback(Mobile attacker, Mobile defender, int distance)
                {
                        if (attacker == null || defender == null)
                                return;

                        Map map = defender.Map;

                        if (map == null || map == Map.Internal)
                                return;

                        int dx = defender.X - attacker.X;
                        int dy = defender.Y - attacker.Y;

                        if (dx == 0 && dy == 0)
                                return;

                        int stepX = Math.Sign(dx);
                        int stepY = Math.Sign(dy);

                        Point3D start = defender.Location;
                        Point3D target = start;

                        for (int i = 1; i <= distance; ++i)
                        {
                                int newX = start.X + (stepX * i);
                                int newY = start.Y + (stepY * i);
                                int newZ = defender.Z;

                                Point3D candidate = new Point3D(newX, newY, newZ);

                                if (!map.CanSpawnMobile(newX, newY, newZ) || SpellHelper.CheckMulti(candidate, map))
                                {
                                        newZ = map.GetAverageZ(newX, newY);
                                        candidate = new Point3D(newX, newY, newZ);

                                        if (!map.CanSpawnMobile(newX, newY, newZ) || SpellHelper.CheckMulti(candidate, map))
                                                break;
                                }

                                target = candidate;
                        }

                        if (target != start)
                                defender.MoveToWorld(target, map);
                }
        }
}
