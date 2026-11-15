using Server.Items;
using Server.Mobiles;
using Server.Misc;

namespace Server.Spells
{
	public static class SpellbookManaDisplay
	{
		public static int GetAdjustedManaCost(Mobile from, int baseMana)
		{
			if (from == null)
				return baseMana;

			double scalar = 1.0;
			int lmc = AosAttributes.GetValue(from, AosAttribute.LowerManaCost);
			int lmcCap = MyServerSettings.LowerManaCostCap();
			int lrc = AosAttributes.GetValue(from, AosAttribute.LowerRegCost);
			int lrcCap = MyServerSettings.LowerReagentCostCap();

			if (from is PlayerMobile && ((PlayerMobile)from).Sorcerer())
				lmcCap = 100;

			if (lrc > lrcCap)
				lrc = lrcCap;

			scalar += (double)lrc / 200;

			lmc = AdventuresFunctions.DiminishingReturns(lmc, lmcCap);

			if (lmc > lmcCap)
				lmc = lmcCap;

			scalar -= (double)lmc / 100;

			if (scalar <= 0.1)
				scalar = 0.1;

			return (int)(baseMana * scalar);
		}

		public static string FormatManaCost(Mobile from, int baseMana)
		{
			return string.Format("{0} ({1})", baseMana, GetAdjustedManaCost(from, baseMana));
		}
	}
}
