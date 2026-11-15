using System;
using System.Collections.Generic;

namespace Server.Multis
{
	public static class HousingEstateRegistry
	{
		private const string TransferBlockedMessage = "This house belongs to a combined estate. Split the estate before transferring ownership.";
		private const string DemolitionBlockedMessage = "This house belongs to a combined estate. Split the estate before demolishing or resizing it.";
		private static readonly Dictionary<HouseFoundation, HousingEstateController> m_Estates = new Dictionary<HouseFoundation, HousingEstateController>();

		public static bool TryGetEstate( HouseFoundation house, out HousingEstateController estate )
		{
			estate = null;

			if ( house == null )
				return false;

			HousingEstateController found;

			if ( !m_Estates.TryGetValue( house, out found ) )
				return false;

			if ( house.Deleted )
			{
				m_Estates.Remove( house );

				if ( found != null && !found.Deleted )
					found.MarkDegraded( "An estate member was deleted." );

				return false;
			}

			if ( found == null || found.Deleted || !found.ContainsMember( house ) )
			{
				m_Estates.Remove( house );
				return false;
			}

			estate = found;
			return true;
		}

		public static bool Register( HousingEstateController estate, out string error )
		{
			error = String.Empty;

			if ( estate == null )
			{
				error = "The estate controller is missing.";
				return false;
			}

			HousingValidationResult validation = estate.ValidateMembers();

			if ( !validation.IsValid )
			{
				error = validation.Message;
				estate.MarkDegraded( error );
				return false;
			}

			List<HouseFoundation> staleMembers = null;

			for ( int i = 0; i < estate.MemberCount; ++i )
			{
				HouseFoundation member = estate.GetMember( i );
				HousingEstateController existing;

				if ( !m_Estates.TryGetValue( member, out existing ) )
					continue;

				if ( existing == null || existing.Deleted || !existing.ContainsMember( member ) )
				{
					if ( staleMembers == null )
						staleMembers = new List<HouseFoundation>();

					staleMembers.Add( member );
					continue;
				}

				if ( existing != estate )
				{
					error = "An estate member is already registered to another controller.";
					estate.MarkDegraded( error );
					return false;
				}
			}

			if ( staleMembers != null )
			{
				for ( int i = 0; i < staleMembers.Count; ++i )
				{
					HouseFoundation member = staleMembers[i];
					HousingEstateController existing;

					if ( m_Estates.TryGetValue( member, out existing ) && ( existing == null || existing.Deleted || !existing.ContainsMember( member ) ) )
						m_Estates.Remove( member );
				}
			}

			for ( int i = 0; i < estate.MemberCount; ++i )
				m_Estates[estate.GetMember( i )] = estate;

			return true;
		}

		public static void Unregister( HousingEstateController estate )
		{
			if ( estate == null )
				return;

			for ( int i = 0; i < estate.MemberCount; ++i )
			{
				HouseFoundation member = estate.GetMember( i );

				if ( member == null )
					continue;

				HousingEstateController existing;

				if ( m_Estates.TryGetValue( member, out existing ) && existing == estate )
					m_Estates.Remove( member );
			}
		}

		internal static bool IsRegisteredTo( HouseFoundation house, HousingEstateController estate )
		{
			if ( house == null || estate == null )
				return false;

			HousingEstateController existing;
			return m_Estates.TryGetValue( house, out existing ) && existing == estate;
		}

		internal static void UnregisterMember( HouseFoundation house, HousingEstateController estate )
		{
			if ( house == null || estate == null )
				return;

			HousingEstateController existing;

			if ( m_Estates.TryGetValue( house, out existing ) && existing == estate )
				m_Estates.Remove( house );
		}

		internal static bool RestoreOwnedMapping( HouseFoundation house, HousingEstateController estate, out string error )
		{
			error = String.Empty;

			if ( house == null || house.Deleted || estate == null || estate.Deleted )
			{
				error = "The saved estate mapping references a deleted object.";
				return false;
			}

			HousingEstateController existing;

			if ( !m_Estates.TryGetValue( house, out existing ) || existing == null || existing.Deleted || !existing.ContainsMember( house ) )
			{
				m_Estates[house] = estate;
				return true;
			}

			if ( existing == estate )
				return true;

			error = "A surviving member is now owned by another estate controller.";
			return false;
		}

		public static void OnHouseDeleted( BaseHouse house )
		{
			HouseFoundation foundation = house as HouseFoundation;

			if ( foundation == null )
				return;

			try
			{
				HousingEstateController estate;

				if ( m_Estates.TryGetValue( foundation, out estate ) )
				{
					m_Estates.Remove( foundation );

					if ( estate != null && !estate.Deleted )
						estate.MarkDegraded( "An estate member was deleted." );
				}
			}
			catch ( Exception exception )
			{
				Server.Scripts.Commands.LogHelper.LogException( exception, "HousingEstateRegistry.OnHouseDeleted failed for house " + foundation.Serial + "." );
			}
		}

		public static void RecordDesignCommit( HouseFoundation house, Mobile actor )
		{
			if ( house == null || house.Deleted )
				return;

			HousingEstateController estate = null;

			try
			{
				if ( !TryGetEstate( house, out estate ) || estate.Health != HousingEstateHealth.Healthy )
					return;

				List<HouseFoundation> members = estate.CopyMembers();
				HousingEstateSnapshot snapshot = HousingEstateSnapshot.Capture( members, HousingEstateOperation.DesignCommit, actor, estate.CurrentRevision + 1 );
				estate.AddRevision( snapshot );
			}
			catch ( Exception exception )
			{
				if ( estate != null && !estate.Deleted )
					estate.MarkDegraded( "Design revision capture failed: " + exception.Message );

				Server.Scripts.Commands.LogHelper.LogException( exception, "HousingEstateRegistry.RecordDesignCommit failed for house " + house.Serial + "." );
			}
		}

		public static bool TryGetCapacityTotals( BaseHouse house, out HousingEstateCapacityTotals totals )
		{
			int fromSecures;
			int fromVendors;
			int fromLockdowns;
			int fromMovingCrate;

			return TryGetCapacityTotals( house, out totals, out fromSecures, out fromVendors, out fromLockdowns, out fromMovingCrate );
		}

		internal static bool TryGetCapacityTotals( BaseHouse house, out HousingEstateCapacityTotals totals, out int fromSecures, out int fromVendors, out int fromLockdowns, out int fromMovingCrate )
		{
			totals = new HousingEstateCapacityTotals();
			fromSecures = 0;
			fromVendors = 0;
			fromLockdowns = 0;
			fromMovingCrate = 0;

			HouseFoundation foundation = house as HouseFoundation;
			HousingEstateController estate;

			if ( foundation == null || !TryGetEstate( foundation, out estate ) || estate.Health != HousingEstateHealth.Healthy )
				return false;

			HousingValidationResult validation = estate.ValidateMembers();

			if ( !validation.IsValid )
			{
				estate.MarkDegraded( validation.Message );
				return false;
			}

			long maxStorage = 0;
			long currentStorage = 0;
			long currentFromSecures = 0;
			long currentFromVendors = 0;
			long currentFromLockdowns = 0;
			long currentFromMovingCrate = 0;
			long maxLockdowns = 0;
			long currentLockdowns = 0;
			long maxVendors = 0;
			long currentVendors = 0;

			for ( int i = 0; i < estate.MemberCount; ++i )
			{
				HouseFoundation member = estate.GetMember( i );
				HousingEstateController registered;

				if ( member == null || member.Deleted || !m_Estates.TryGetValue( member, out registered ) || registered != estate )
				{
					estate.MarkDegraded( "An estate member is missing, deleted, or has invalid registry membership." );
					return false;
				}

				int localFromSecures;
				int localFromVendors;
				int localFromLockdowns;
				int localFromMovingCrate;

				maxStorage += member.GetLocalAosMaxSecures();
				currentStorage += member.GetLocalAosCurSecures( out localFromSecures, out localFromVendors, out localFromLockdowns, out localFromMovingCrate );
				currentFromSecures += localFromSecures;
				currentFromVendors += localFromVendors;
				currentFromLockdowns += localFromLockdowns;
				currentFromMovingCrate += localFromMovingCrate;
				maxLockdowns += member.GetLocalAosMaxLockdowns();
				currentLockdowns += member.GetLocalAosCurLockdowns();
				maxVendors += member.GetLocalNewVendorSystemMaxVendors();
				currentVendors += member.GetLocalNewVendorSystemCurVendors();
			}

			totals.MaxStorage = HousingEstateCapacityTotals.Clamp( maxStorage );
			totals.CurrentStorage = HousingEstateCapacityTotals.Clamp( currentStorage );
			totals.MaxLockdowns = HousingEstateCapacityTotals.Clamp( maxLockdowns );
			totals.CurrentLockdowns = HousingEstateCapacityTotals.Clamp( currentLockdowns );
			totals.MaxVendors = HousingEstateCapacityTotals.Clamp( maxVendors );
			totals.CurrentVendors = HousingEstateCapacityTotals.Clamp( currentVendors );
			fromSecures = HousingEstateCapacityTotals.Clamp( currentFromSecures );
			fromVendors = HousingEstateCapacityTotals.Clamp( currentFromVendors );
			fromLockdowns = HousingEstateCapacityTotals.Clamp( currentFromLockdowns );
			fromMovingCrate = HousingEstateCapacityTotals.Clamp( currentFromMovingCrate );
			return true;
		}

		public static bool CanTransfer( BaseHouse house, Mobile actor, out string error )
		{
			error = String.Empty;

			if ( !IsRegisteredMember( house ) )
				return true;

			error = TransferBlockedMessage;
			return false;
		}

		public static bool CanDemolish( BaseHouse house, Mobile actor, out string error )
		{
			error = String.Empty;

			if ( !IsRegisteredMember( house ) )
				return true;

			error = DemolitionBlockedMessage;
			return false;
		}

		private static bool IsRegisteredMember( BaseHouse house )
		{
			HouseFoundation foundation = house as HouseFoundation;
			HousingEstateController estate;
			return foundation != null && TryGetEstate( foundation, out estate );
		}
	}
}
