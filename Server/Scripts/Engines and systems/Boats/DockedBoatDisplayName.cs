using System;

namespace Server.Multis
{
	internal static class DockedBoatDisplayName
	{
		public const string SmallBoat = "Small Boat";
		public const string SmallDragonShip = "Small Dragon Ship";
		public const string MediumBoat = "Medium Boat";
		public const string MediumDragonShip = "Medium Dragon Ship";
		public const string LargeBoat = "Large Boat";
		public const string LargeDragonShip = "Large Dragon Ship";

		public static string Format( string shipName, string modelTypeName )
		{
			if ( modelTypeName == null )
				return null;

			if ( String.IsNullOrEmpty( shipName ) )
				return modelTypeName;

			return shipName + " \u2014 " + modelTypeName;
		}
	}
}
