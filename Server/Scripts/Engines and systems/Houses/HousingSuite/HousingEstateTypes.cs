using System;

namespace Server.Multis
{
	public enum HousingEstateOperation
	{
		Combine,
		Split,
		MoveHouse,
		MoveEstate,
		ElevateHouse,
		ElevateEstate,
		Restore,
		DesignCommit,
		ForceDissolve
	}

	public enum HousingEstateHealth
	{
		Healthy,
		Degraded
	}

	public sealed class HousingValidationResult
	{
		public static readonly HousingValidationResult Valid = new HousingValidationResult( true, String.Empty );

		public bool IsValid { get; private set; }
		public string Message { get; private set; }

		public HousingValidationResult( bool isValid, string message )
		{
			IsValid = isValid;
			Message = message ?? String.Empty;
		}
	}

	public struct HousingEstateCapacityTotals
	{
		public int MaxStorage, CurrentStorage, MaxLockdowns, CurrentLockdowns, MaxVendors, CurrentVendors;

		public static int Clamp( long value )
		{
			return value < 0 ? 0 : ( value > Int32.MaxValue ? Int32.MaxValue : (int) value );
		}
	}

	public static class HousingEstateHistory
	{
		public static int CalculateDiscardCount( int currentCount, int incomingCount, int maximumCount )
		{
			if ( currentCount < 0 )
				currentCount = 0;

			if ( incomingCount < 0 )
				incomingCount = 0;

			if ( maximumCount < 0 )
				maximumCount = 0;

			long overflow = (long) currentCount + incomingCount - maximumCount;
			return overflow <= 0 ? 0 : ( overflow > currentCount ? currentCount : (int) overflow );
		}
	}

	public static class HousingEstateGeometry
	{
		public static bool AreConnected( Rectangle2D left, Rectangle2D right, int maximumGap )
		{
			long leftStartX = left.X;
			long leftStartY = left.Y;
			long leftEndX = leftStartX + (long) left.Width;
			long leftEndY = leftStartY + (long) left.Height;
			long rightStartX = right.X;
			long rightStartY = right.Y;
			long rightEndX = rightStartX + (long) right.Width;
			long rightEndY = rightStartY + (long) right.Height;
			long gap = maximumGap < 0 ? 0L : (long) maximumGap;

			return ( RangesOverlap( leftStartY, leftEndY, rightStartY, rightEndY ) && RangesWithinGap( leftStartX, leftEndX, rightStartX, rightEndX, gap ) )
				|| ( RangesOverlap( leftStartX, leftEndX, rightStartX, rightEndX ) && RangesWithinGap( leftStartY, leftEndY, rightStartY, rightEndY, gap ) );
		}

		private static bool RangesOverlap( long leftStart, long leftEnd, long rightStart, long rightEnd )
		{
			return leftStart < rightEnd && rightStart < leftEnd;
		}

		private static bool RangesWithinGap( long leftStart, long leftEnd, long rightStart, long rightEnd, long maximumGap )
		{
			if ( leftEnd < rightStart )
				return rightStart - leftEnd <= maximumGap;

			if ( rightEnd < leftStart )
				return leftStart - rightEnd <= maximumGap;

			return true;
		}
	}
}
