using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
	public sealed class PowerScrollBuyInfoComparer : IComparer<GenericBuyInfo>
	{
		public static readonly PowerScrollBuyInfoComparer Instance = new PowerScrollBuyInfoComparer();

		private PowerScrollBuyInfoComparer()
		{
		}

		public int Compare(GenericBuyInfo left, GenericBuyInfo right)
		{
			string leftName = GetTypeName(left);
			string rightName = GetTypeName(right);

			return string.Compare(leftName, rightName, StringComparison.OrdinalIgnoreCase);
		}

		private static string GetTypeName(GenericBuyInfo info)
		{
			if (info == null || info.Type == null)
				return string.Empty;

			return info.Type.Name ?? string.Empty;
		}
	}
}
