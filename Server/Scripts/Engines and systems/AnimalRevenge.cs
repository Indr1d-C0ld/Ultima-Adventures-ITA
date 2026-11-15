using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;

namespace Server.Engines
{
    public static class AnimalRevengeSystem
    {
        private class KillInfo
        {
            public int Count;
            public int Body;
            public int Hue;
            public int Sound;
        }

        private static readonly Dictionary<PlayerMobile, KillInfo> m_Kills = new Dictionary<PlayerMobile, KillInfo>();

        // Number of animal kills required before the avenger appears
        public static readonly int KillThreshold = 100;

        public static void OnAnimalKilled(BaseCreature creature, Mobile killer)
        {
            if (creature == null || killer == null)
                return;

            // only track true animals and ignore the avenger itself
            if (!creature.Body.IsAnimal || creature is AnimalAvenger)
                return;

            if (killer is BaseCreature)
            {
                Mobile master = ((BaseCreature)killer).GetMaster();
                if (master != null)
                    killer = master;
            }

            PlayerMobile pm = killer as PlayerMobile;

            if (pm == null)
                return;

            KillInfo info;
            if (!m_Kills.TryGetValue(pm, out info))
                m_Kills[pm] = info = new KillInfo();

            info.Count++;
            info.Body = creature.BodyValue;
            info.Hue = creature.Hue;
            info.Sound = creature.BaseSoundID;

            if (info.Count >= KillThreshold)
            {
                SpawnAvenger(pm, info.Body, info.Hue, info.Sound);
                info.Count = 0; // reset after spawning
            }
        }

        private static void SpawnAvenger(PlayerMobile pm, int body, int hue, int sound)
        {
            if (pm == null || pm.Map == null)
                return;

            Point3D loc = GetSpawnLocation(pm.Location, pm.Map, 10);

            AnimalAvenger avenger = new AnimalAvenger(body, hue, sound);
            avenger.MoveToWorld(loc, pm.Map);
            avenger.Combatant = pm;
            avenger.FocusMob = pm;

            pm.SendMessage("The wild seeks revenge for your deeds!");
        }

        private static Point3D GetSpawnLocation(Point3D center, Map map, int distance)
        {
            Point3D loc = center;

            for (int i = 0; i < 10; i++)
            {
                double angle = Utility.RandomDouble() * Math.PI * 2;
                int x = center.X + (int)Math.Round(Math.Cos(angle) * distance);
                int y = center.Y + (int)Math.Round(Math.Sin(angle) * distance);
                int z = map.GetAverageZ(x, y);

                if (map.CanSpawnMobile(x, y, z))
                    return new Point3D(x, y, z);
            }

            return center;
        }
    }
}

