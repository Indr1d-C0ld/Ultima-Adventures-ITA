using System;

namespace Server.Spells
{
    public static class SkillScaledAreaRange
    {
        public static int Calculate(double skillValue)
        {
            if (Double.IsNaN(skillValue) || skillValue < 20.0)
                return 3;

            if (skillValue < 40.0)
                return 4;

            if (skillValue < 60.0)
                return 5;

            if (skillValue < 80.0)
                return 6;

            if (skillValue < 100.0)
                return 7;

            if (skillValue < 110.0)
                return 8;

            if (skillValue < 120.0)
                return 9;

            if (skillValue < 125.0)
                return 10;

            return 11;
        }
    }
}
