using System;
using System.Collections.Generic;
using Server.Commands;

namespace Server.Multis
{
	public static class HousingEstateService
	{
		public static bool TryCombine( Mobile actor, IList<Serial> memberSerials, out HousingEstateController estate, out string error )
		{
			estate = null;
			error = String.Empty;

			if ( !ValidateActor( actor, AccessLevel.GameMaster, out error ) )
			{
				Log( actor, HousingEstateOperation.Combine, Serial.MinusOne, -1, false, error );
				return false;
			}

			List<HouseFoundation> members;

			if ( !ResolveMembers( memberSerials, out members, out error ) )
			{
				Log( actor, HousingEstateOperation.Combine, Serial.MinusOne, -1, false, error );
				return false;
			}

			HousingValidationResult validation = HousingEstateValidation.ValidateCombine( members );

			if ( !validation.IsValid )
			{
				error = validation.Message;
				Log( actor, HousingEstateOperation.Combine, Serial.MinusOne, -1, false, error );
				return false;
			}

			HousingEstateSnapshot original = null;
			HousingEstateController attempted = null;

			try
			{
				original = HousingEstateSnapshot.Capture( members, HousingEstateOperation.Combine, actor, 0 );
				attempted = new HousingEstateController( members, original );

				for ( int i = 0; i < members.Count; ++i )
					members[i].RestrictDecay = true;

				if ( !HousingEstateRegistry.Register( attempted, out error ) )
					throw new InvalidOperationException( error );

				if ( !VerifyRegisteredMembersAndCapacity( attempted, members, out error ) )
					throw new InvalidOperationException( error );

				HousingEstateSnapshot after = HousingEstateSnapshot.Capture( members, HousingEstateOperation.Combine, actor, attempted.CurrentRevision + 1 );
				attempted.AddRevision( after );

				if ( attempted.CurrentRevision != 1 || attempted.RecentRevisionCount != 1 || attempted.GetRecentRevision( 0 ) != after )
					throw new InvalidOperationException( "The combine revision was not appended exactly once." );

				estate = attempted;
				Log( actor, HousingEstateOperation.Combine, estate.Serial, after.Revision, true, "combined members=" + FormatMemberSerials( members ) );
				return true;
			}
			catch ( Exception exception )
			{
				error = ConciseReason( exception );

				string cleanupError;

				if ( !RollbackCombine( attempted, original, members, out cleanupError ) )
				{
					error = error + " Rollback verification failed: " + cleanupError;
					MarkRollbackFailure( attempted, HousingEstateOperation.Combine, cleanupError );
				}

				Serial attemptedSerial = attempted == null ? Serial.MinusOne : attempted.Serial;
				error += " members=" + FormatMemberSerials( members );
				Log( actor, HousingEstateOperation.Combine, attemptedSerial, -1, false, error );
				return false;
			}
		}

		public static bool TrySplit( Mobile actor, Serial estateSerial, out string error )
		{
			error = String.Empty;

			if ( !ValidateActor( actor, AccessLevel.GameMaster, out error ) )
			{
				Log( actor, HousingEstateOperation.Split, estateSerial, -1, false, error );
				return false;
			}

			HousingEstateController estate;

			if ( !ResolveHealthyEstate( estateSerial, out estate, out error ) )
			{
				Log( actor, HousingEstateOperation.Split, estateSerial, -1, false, error );
				return false;
			}

			HousingValidationResult validation = HousingEstateValidation.ValidateSplit( estate );

			if ( !validation.IsValid )
			{
				error = validation.Message;
				Log( actor, HousingEstateOperation.Split, estateSerial, estate.CurrentRevision, false, error );
				return false;
			}

			List<HouseFoundation> members = null;
			HousingEstateSnapshot rollback = null;
			HousingEstateHealth previousHealth = estate.Health;
			string previousReason = estate.DegradedReason;

			try
			{
				members = estate.CopyMembers();
				rollback = HousingEstateSnapshot.Capture( members, HousingEstateOperation.Split, actor, estate.CurrentRevision );
				validation = estate.OriginalSnapshot.RestoreDecayLiveState();

				if ( !validation.IsValid )
					throw new InvalidOperationException( validation.Message );

				HousingEstateRegistry.Unregister( estate );

				if ( HasOwnedMapping( estate, members ) )
					throw new InvalidOperationException( "One or more estate mappings remained after split unregister." );

				estate.Delete();

				if ( !estate.Deleted )
					throw new InvalidOperationException( "The estate controller was not deleted." );

				Log( actor, HousingEstateOperation.Split, estateSerial, rollback.Revision, true, "split members=" + FormatMemberSerials( members ) );
				return true;
			}
			catch ( Exception exception )
			{
				error = ConciseReason( exception );

				if ( rollback != null && members != null && !estate.Deleted )
				{
					string rollbackError;

					if ( !RollbackSplit( estate, members, rollback, previousHealth, previousReason, out rollbackError ) )
					{
						error = error + " Rollback verification failed: " + rollbackError;
						MarkRollbackFailure( estate, HousingEstateOperation.Split, rollbackError );
					}
				}

				error += " members=" + FormatMemberSerials( members );
				Log( actor, HousingEstateOperation.Split, estateSerial, rollback == null ? estate.CurrentRevision : rollback.Revision, false, error );
				return false;
			}
		}

		public static bool TryMoveHouse( Mobile actor, Serial houseSerial, Map destinationMap, Point3D destination, bool force, out string error )
		{
			error = String.Empty;
			Point3D source = Point3D.Zero;
			Map sourceMap = null;
			int deltaX = 0;
			int deltaY = 0;
			int deltaZ = 0;

			if ( !ValidateActor( actor, AccessLevel.GameMaster, out error ) )
			{
				LogMove( actor, HousingEstateOperation.MoveHouse, houseSerial, sourceMap, source, destinationMap, destination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			BaseHouse selectedHouse;

			if ( !ResolveHouse( houseSerial, out selectedHouse, out error ) )
			{
				LogMove( actor, HousingEstateOperation.MoveHouse, houseSerial, sourceMap, source, destinationMap, destination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			HouseFoundation house = selectedHouse as HouseFoundation;

			if ( house == null )
				return TryMoveStandaloneHouse( actor, selectedHouse, destinationMap, destination, force, out error );

			source = house.Location;
			sourceMap = house.Map;

			if ( !TryGetDelta( source, destination, out deltaX, out deltaY, out deltaZ, out error ) )
			{
				LogMove( actor, HousingEstateOperation.MoveHouse, houseSerial, sourceMap, source, destinationMap, destination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			HousingEstateController estate = null;
			HousingEstateController registered;
			List<HouseFoundation> transactionMembers;

			if ( HousingEstateRegistry.TryGetEstate( house, out registered ) )
			{
				if ( !ResolveHealthyEstate( registered.Serial, out estate, out error ) || estate != registered )
				{
					if ( String.IsNullOrEmpty( error ) )
						error = "The house has invalid estate membership.";

					LogMove( actor, HousingEstateOperation.MoveHouse, houseSerial, sourceMap, source, destinationMap, destination, deltaX, deltaY, deltaZ, force, false, error );
					return false;
				}

				transactionMembers = estate.CopyMembers();
			}
			else
			{
				transactionMembers = new List<HouseFoundation>( 1 );
				transactionMembers.Add( house );
			}

			if ( estate != null )
			{
				HousingValidationResult currentIntegrity = HousingEstateValidation.ValidateConnectedEstate( transactionMembers );

				if ( !currentIntegrity.IsValid )
				{
					error = currentIntegrity.Message;
					LogMove( actor, HousingEstateOperation.MoveHouse, houseSerial, sourceMap, source, destinationMap, destination, deltaX, deltaY, deltaZ, force, false, error );
					return false;
				}
			}

			List<HouseFoundation> selection = new List<HouseFoundation>( 1 );
			selection.Add( house );
			HousingValidationResult validation = HousingEstateValidation.ValidateMove( actor, selection, destinationMap, deltaX, deltaY, deltaZ, force );

			if ( !validation.IsValid )
			{
				error = validation.Message;
				LogMove( actor, HousingEstateOperation.MoveHouse, houseSerial, sourceMap, source, destinationMap, destination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			HousingEstateCapacityTotals expectedTotals = new HousingEstateCapacityTotals();

			if ( estate != null && !HousingEstateRegistry.TryGetCapacityTotals( transactionMembers[0], out expectedTotals ) )
			{
				error = "Estate capacity could not be captured before movement.";
				LogMove( actor, HousingEstateOperation.MoveHouse, houseSerial, sourceMap, source, destinationMap, destination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			int previousRevision = estate == null ? 0 : estate.CurrentRevision;
			int previousRevisionCount = estate == null ? 0 : estate.RecentRevisionCount;
			HousingEstateSnapshot rollback = null;
			MoveTransactionState moveStates = null;

			try
			{
				rollback = HousingEstateSnapshot.Capture( transactionMembers, HousingEstateOperation.MoveHouse, actor, previousRevision );
				moveStates = CaptureMoveStates( transactionMembers );
				MarkMoveStarted( moveStates.Houses, house );
				house.MoveToWorld( destination, destinationMap );

				if ( house.Map != destinationMap || house.Location != destination )
					throw new InvalidOperationException( "The house did not reach its requested destination." );

				HousingValidationResult finalPlacement = HousingEstateValidation.ValidateMoveHard( actor, selection, destinationMap, 0, 0, 0 );

				if ( !finalPlacement.IsValid )
					throw new InvalidOperationException( "Post-movement validation failed: " + finalPlacement.Message );

				if ( estate != null )
				{
					if ( !VerifyEstateAfterMove( estate, transactionMembers, expectedTotals, out error ) )
						throw new InvalidOperationException( error );
				}
				else if ( HousingEstateRegistry.TryGetEstate( house, out registered ) )
				{
					throw new InvalidOperationException( "Standalone movement unexpectedly created estate membership." );
				}

				HousingEstateSnapshot after = HousingEstateSnapshot.Capture( transactionMembers, HousingEstateOperation.MoveHouse, actor, previousRevision + 1 );

				if ( !after.VerifyLiveState() )
					throw new InvalidOperationException( "The moved house state did not verify before revision capture." );

				if ( estate != null )
					estate.AddRevision( after );

				LogMove( actor, HousingEstateOperation.MoveHouse, houseSerial, sourceMap, source, destinationMap, destination, deltaX, deltaY, deltaZ, force, true, estate == null ? "moved standalone house" : "moved estate member as revision " + after.Revision );
				return true;
			}
			catch ( Exception exception )
			{
				error = ConciseReason( exception );

				if ( rollback != null && moveStates != null )
				{
					string rollbackError;

					if ( !RollbackMove( actor, estate, transactionMembers, moveStates, rollback, expectedTotals, previousRevision, previousRevisionCount, out rollbackError ) )
					{
						error += " Rollback verification failed: " + rollbackError;
						MarkRollbackFailure( estate, HousingEstateOperation.MoveHouse, rollbackError );
					}
				}

				LogMove( actor, HousingEstateOperation.MoveHouse, houseSerial, sourceMap, source, destinationMap, destination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}
		}

		public static bool TryMoveEstate( Mobile actor, Serial estateSerial, Map destinationMap, Point3D anchorDestination, bool force, out string error )
		{
			error = String.Empty;
			Point3D source = Point3D.Zero;
			Map sourceMap = null;
			int deltaX = 0;
			int deltaY = 0;
			int deltaZ = 0;

			if ( !ValidateActor( actor, AccessLevel.GameMaster, out error ) )
			{
				LogMove( actor, HousingEstateOperation.MoveEstate, estateSerial, sourceMap, source, destinationMap, anchorDestination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			HousingEstateController estate;

			if ( !ResolveHealthyEstate( estateSerial, out estate, out error ) )
			{
				LogMove( actor, HousingEstateOperation.MoveEstate, estateSerial, sourceMap, source, destinationMap, anchorDestination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			List<HouseFoundation> members = estate.CopyMembers();

			if ( members.Count == 0 || members[0] == null )
			{
				error = "The estate has no canonical anchor member.";
				error += " members=" + FormatMemberSerials( members );
				LogMove( actor, HousingEstateOperation.MoveEstate, estateSerial, sourceMap, source, destinationMap, anchorDestination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			source = members[0].Location;
			sourceMap = members[0].Map;
			HousingValidationResult currentIntegrity = HousingEstateValidation.ValidateConnectedEstate( members );

			if ( !currentIntegrity.IsValid )
			{
				error = currentIntegrity.Message;
				error += " members=" + FormatMemberSerials( members );
				LogMove( actor, HousingEstateOperation.MoveEstate, estateSerial, sourceMap, source, destinationMap, anchorDestination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			if ( !TryGetDelta( source, anchorDestination, out deltaX, out deltaY, out deltaZ, out error ) )
			{
				error += " members=" + FormatMemberSerials( members );
				LogMove( actor, HousingEstateOperation.MoveEstate, estateSerial, sourceMap, source, destinationMap, anchorDestination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			HousingValidationResult validation = HousingEstateValidation.ValidateMove( actor, members, destinationMap, deltaX, deltaY, deltaZ, force );

			if ( !validation.IsValid )
			{
				error = validation.Message;
				error += " members=" + FormatMemberSerials( members );
				LogMove( actor, HousingEstateOperation.MoveEstate, estateSerial, sourceMap, source, destinationMap, anchorDestination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			HousingEstateCapacityTotals expectedTotals;

			if ( !HousingEstateRegistry.TryGetCapacityTotals( members[0], out expectedTotals ) )
			{
				error = "Estate capacity could not be captured before movement.";
				error += " members=" + FormatMemberSerials( members );
				LogMove( actor, HousingEstateOperation.MoveEstate, estateSerial, sourceMap, source, destinationMap, anchorDestination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			Point3D[] destinations = new Point3D[members.Count];

			for ( int i = 0; i < members.Count; ++i )
			{
				if ( !TryApplyDelta( members[i].Location, deltaX, deltaY, deltaZ, out destinations[i] ) )
				{
					error = "An estate member destination overflowed after preflight.";
					error += " members=" + FormatMemberSerials( members );
					LogMove( actor, HousingEstateOperation.MoveEstate, estateSerial, sourceMap, source, destinationMap, anchorDestination, deltaX, deltaY, deltaZ, force, false, error );
					return false;
				}
			}

			int previousRevision = estate.CurrentRevision;
			int previousRevisionCount = estate.RecentRevisionCount;
			HousingEstateSnapshot rollback = null;
			MoveTransactionState moveStates = null;

			try
			{
				rollback = HousingEstateSnapshot.Capture( members, HousingEstateOperation.MoveEstate, actor, previousRevision );
				moveStates = CaptureMoveStates( members );

				for ( int i = 0; i < members.Count; ++i )
				{
					moveStates.Houses[i].MoveStarted = true;
					members[i].MoveToWorld( destinations[i], destinationMap );
				}

				HousingValidationResult finalPlacement = HousingEstateValidation.ValidateMoveHard( actor, members, destinationMap, 0, 0, 0 );

				if ( !finalPlacement.IsValid )
					throw new InvalidOperationException( "Post-movement validation failed: " + finalPlacement.Message );

				for ( int i = 0; i < members.Count; ++i )
				{
					if ( members[i].Map != destinationMap || members[i].Location != destinations[i] )
						throw new InvalidOperationException( "Estate member " + members[i].Serial + " did not reach its requested destination." );
				}

				if ( !VerifyEstateAfterMove( estate, members, expectedTotals, out error ) )
					throw new InvalidOperationException( error );

				HousingEstateSnapshot after = HousingEstateSnapshot.Capture( members, HousingEstateOperation.MoveEstate, actor, previousRevision + 1 );

				if ( !after.VerifyLiveState() )
					throw new InvalidOperationException( "The moved estate state did not verify before revision capture." );

				estate.AddRevision( after );
				LogMove( actor, HousingEstateOperation.MoveEstate, estateSerial, sourceMap, source, destinationMap, anchorDestination, deltaX, deltaY, deltaZ, force, true, "members=" + FormatMemberSerials( members ) + " revision=" + after.Revision );
				return true;
			}
			catch ( Exception exception )
			{
				error = ConciseReason( exception );

				if ( rollback != null && moveStates != null )
				{
					string rollbackError;

					if ( !RollbackMove( actor, estate, members, moveStates, rollback, expectedTotals, previousRevision, previousRevisionCount, out rollbackError ) )
					{
						error += " Rollback verification failed: " + rollbackError;
						MarkRollbackFailure( estate, HousingEstateOperation.MoveEstate, rollbackError );
					}
				}

				error += " members=" + FormatMemberSerials( members );
				LogMove( actor, HousingEstateOperation.MoveEstate, estateSerial, sourceMap, source, destinationMap, anchorDestination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}
		}

		private static bool TryMoveStandaloneHouse( Mobile actor, BaseHouse house, Map destinationMap, Point3D destination, bool force, out string error )
		{
			error = String.Empty;
			Point3D source = house.Location;
			Map sourceMap = house.Map;
			int deltaX;
			int deltaY;
			int deltaZ;

			if ( !TryGetDelta( source, destination, out deltaX, out deltaY, out deltaZ, out error ) )
			{
				LogMove( actor, HousingEstateOperation.MoveHouse, house.Serial, sourceMap, source, destinationMap, destination, 0, 0, 0, force, false, error );
				return false;
			}

			List<BaseHouse> houses = new List<BaseHouse>( 1 );
			houses.Add( house );
			HousingValidationResult validation = HousingEstateValidation.ValidateMove( actor, houses, destinationMap, deltaX, deltaY, deltaZ, force );

			if ( !validation.IsValid )
			{
				error = validation.Message;
				LogMove( actor, HousingEstateOperation.MoveHouse, house.Serial, sourceMap, source, destinationMap, destination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}

			MoveTransactionState moveStates = CaptureMoveStates( houses );

			try
			{
				MarkMoveStarted( moveStates.Houses, house );
				house.MoveToWorld( destination, destinationMap );

				if ( house.Map != destinationMap || house.Location != destination )
					throw new InvalidOperationException( "The house did not reach its requested destination." );

				validation = HousingEstateValidation.ValidateMoveHard( actor, houses, destinationMap, 0, 0, 0 );

				if ( !validation.IsValid )
					throw new InvalidOperationException( "Post-movement validation failed: " + validation.Message );

				LogMove( actor, HousingEstateOperation.MoveHouse, house.Serial, sourceMap, source, destinationMap, destination, deltaX, deltaY, deltaZ, force, true, "moved standalone house" );
				return true;
			}
			catch ( Exception exception )
			{
				error = ConciseReason( exception );
				string rollbackError;

				if ( !RollbackStandaloneMove( moveStates, out rollbackError ) )
					error += " Rollback verification failed: " + rollbackError;

				LogMove( actor, HousingEstateOperation.MoveHouse, house.Serial, sourceMap, source, destinationMap, destination, deltaX, deltaY, deltaZ, force, false, error );
				return false;
			}
		}

		public static bool TryRestore( Mobile actor, Serial estateSerial, int revision, out string error )
		{
			error = String.Empty;

			if ( !ValidateActor( actor, AccessLevel.GameMaster, out error ) )
			{
				Log( actor, HousingEstateOperation.Restore, estateSerial, revision, false, error );
				return false;
			}

			HousingEstateController estate;

			if ( !ResolveHealthyEstate( estateSerial, out estate, out error ) )
			{
				Log( actor, HousingEstateOperation.Restore, estateSerial, revision, false, error );
				return false;
			}

			HousingEstateSnapshot selected = estate.FindRevision( revision );

			if ( selected == null )
			{
				error = "The requested estate revision is unavailable or ambiguous.";
				Log( actor, HousingEstateOperation.Restore, estateSerial, revision, false, error );
				return false;
			}

			List<HouseFoundation> members = null;
			HousingEstateSnapshot rollback = null;
			HousingEstateHealth previousHealth = estate.Health;
			string previousReason = estate.DegradedReason;
			int previousRevision = estate.CurrentRevision;
			bool mutationStarted = false;

			try
			{
				members = estate.CopyMembers();
				HousingValidationResult validation = selected.ValidateRestoreReferences();

				if ( !validation.IsValid )
				{
					error = validation.Message;
					Log( actor, HousingEstateOperation.Restore, estateSerial, revision, false, error + " members=" + FormatMemberSerials( members ) );
					return false;
				}

				if ( !selected.MatchesMembership( members ) )
				{
					error = "The requested revision has incompatible estate membership.";
					Log( actor, HousingEstateOperation.Restore, estateSerial, revision, false, error + " members=" + FormatMemberSerials( members ) );
					return false;
				}

				validation = selected.ValidateRestoreDestinations( members );

				if ( !validation.IsValid )
				{
					error = validation.Message;
					Log( actor, HousingEstateOperation.Restore, estateSerial, revision, false, error + " members=" + FormatMemberSerials( members ) );
					return false;
				}

				rollback = HousingEstateSnapshot.Capture( members, HousingEstateOperation.Restore, actor, estate.CurrentRevision );
				mutationStarted = true;
				validation = selected.RestoreLiveState( actor );

				if ( !validation.IsValid )
					throw new InvalidOperationException( validation.Message );

				if ( !selected.VerifyLiveState() )
					throw new InvalidOperationException( "The restored estate state did not match the selected revision." );

				if ( !VerifyRegisteredMembers( estate, members ) )
					throw new InvalidOperationException( "Estate registry membership changed during restore." );

				HousingEstateSnapshot after = HousingEstateSnapshot.Capture( members, HousingEstateOperation.Restore, actor, previousRevision + 1 );
				estate.AddRevision( after );

				Log( actor, HousingEstateOperation.Restore, estateSerial, revision, true, "members=" + FormatMemberSerials( members ) + " restoredAsRevision=" + after.Revision );
				return true;
			}
			catch ( Exception exception )
			{
				error = ConciseReason( exception );

				if ( mutationStarted && rollback != null && members != null )
				{
					string rollbackError;

					if ( !RollbackRestore( estate, members, rollback, previousHealth, previousReason, out rollbackError ) )
					{
						error = error + " Rollback verification failed: " + rollbackError;
						MarkRollbackFailure( estate, HousingEstateOperation.Restore, rollbackError );
					}
				}

				error += " members=" + FormatMemberSerials( members );
				Log( actor, HousingEstateOperation.Restore, estateSerial, revision, false, error );
				return false;
			}
		}

		public static bool TryForceDissolve( Mobile actor, Serial estateSerial, out string error )
		{
			error = String.Empty;

			if ( !ValidateActor( actor, AccessLevel.Administrator, out error ) )
			{
				Log( actor, HousingEstateOperation.ForceDissolve, estateSerial, -1, false, error );
				return false;
			}

			HousingEstateController estate;

			if ( !ResolveEstate( estateSerial, out estate, out error ) )
			{
				Log( actor, HousingEstateOperation.ForceDissolve, estateSerial, -1, false, error );
				return false;
			}

			if ( estate.Health != HousingEstateHealth.Degraded )
			{
				error = "Only a degraded estate may be force dissolved.";
				Log( actor, HousingEstateOperation.ForceDissolve, estateSerial, estate.CurrentRevision, false, error );
				return false;
			}

			List<ForceDissolveMemberState> survivingStates = null;
			HousingEstateSnapshot rollback = null;

			try
			{
				survivingStates = CaptureForceDissolveState( actor, estate, estateSerial );
				List<HouseFoundation> survivingFoundations = new List<HouseFoundation>( survivingStates.Count );

				for ( int i = 0; i < survivingStates.Count; ++i )
					survivingFoundations.Add( survivingStates[i].Foundation );

				rollback = HousingEstateSnapshot.Capture( survivingFoundations, HousingEstateOperation.ForceDissolve, actor, estate.CurrentRevision );

				for ( int i = 0; i < survivingStates.Count; ++i )
				{
					HouseFoundation foundation = survivingStates[i].Foundation;

					HousingEstateRegistry.UnregisterMember( foundation, estate );

					string decayError = String.Empty;

					if ( estate.OriginalSnapshot == null || !estate.OriginalSnapshot.TryRestoreDecay( foundation, out decayError ) )
						LogSkippedMember( actor, estateSerial, foundation.Serial, String.IsNullOrEmpty( decayError ) ? "original decay state unavailable" : decayError );
				}

				HousingEstateRegistry.Unregister( estate );

				for ( int i = 0; i < survivingStates.Count; ++i )
				{
					HouseFoundation surviving = survivingStates[i].Foundation;

					if ( surviving != null && HousingEstateRegistry.IsRegisteredTo( surviving, estate ) )
						throw new InvalidOperationException( "A surviving estate mapping could not be removed." );
				}

				estate.Delete();

				if ( !estate.Deleted )
					throw new InvalidOperationException( "The degraded estate controller was not deleted." );

				Log( actor, HousingEstateOperation.ForceDissolve, estateSerial, estate.CurrentRevision, true, "members=" + FormatForceMemberSerials( survivingStates ) + " degraded estate dissolved" );
				return true;
			}
			catch ( Exception exception )
			{
				error = ConciseReason( exception );

				if ( !estate.Deleted && rollback != null && survivingStates != null )
				{
					string rollbackError;

					if ( !RollbackForceDissolve( estate, survivingStates, rollback, out rollbackError ) )
					{
						error += " Rollback verification failed: " + rollbackError;

						string cleanupError;

						if ( !CompleteForceDissolveCleanup( actor, estate, estateSerial, survivingStates, out cleanupError ) )
						{
							error += " Deterministic cleanup failed: " + cleanupError;
							MarkRollbackFailure( estate, HousingEstateOperation.ForceDissolve, rollbackError + "; " + cleanupError );
						}
					}
				}

				error += " members=" + FormatForceMemberSerials( survivingStates );
				Log( actor, HousingEstateOperation.ForceDissolve, estateSerial, estate.CurrentRevision, false, error );
				return false;
			}
		}

		private sealed class HouseMoveState
		{
			public BaseHouse House;
			public Map Map;
			public Point3D Location;
			public bool MoveStarted;
		}

		private sealed class MoveEntityState
		{
			public IEntity Entity;
			public Map Map;
			public Point3D Location;
			public IEntity Parent;
			public bool AllowDeletedOnRollback;
		}

		private sealed class MoveTransactionState
		{
			public List<HouseMoveState> Houses;
			public List<MoveEntityState> Entities;
		}

		private static bool ResolveHouse( Serial houseSerial, out BaseHouse house, out string error )
		{
			house = null;
			error = String.Empty;
			Item item = World.FindItem( houseSerial );

			if ( item == null || item.Deleted )
			{
				error = "The house foundation is missing or deleted.";
				return false;
			}

			house = item as BaseHouse;

			if ( house == null )
			{
				error = "The supplied serial is not a house.";
				return false;
			}

			return true;
		}

		private static bool TryGetDelta( Point3D source, Point3D destination, out int deltaX, out int deltaY, out int deltaZ, out string error )
		{
			deltaX = 0;
			deltaY = 0;
			deltaZ = 0;
			error = String.Empty;
			long x = (long) destination.X - source.X;
			long y = (long) destination.Y - source.Y;
			long z = (long) destination.Z - source.Z;

			if ( x < Int32.MinValue || x > Int32.MaxValue || y < Int32.MinValue || y > Int32.MaxValue || z < Int32.MinValue || z > Int32.MaxValue )
			{
				error = "The requested movement delta overflows supported coordinates.";
				return false;
			}

			deltaX = (int) x;
			deltaY = (int) y;
			deltaZ = (int) z;
			return true;
		}

		private static bool TryApplyDelta( Point3D source, int deltaX, int deltaY, int deltaZ, out Point3D destination )
		{
			long x = (long) source.X + deltaX;
			long y = (long) source.Y + deltaY;
			long z = (long) source.Z + deltaZ;

			if ( x < Int32.MinValue || x > Int32.MaxValue || y < Int32.MinValue || y > Int32.MaxValue || z < Int32.MinValue || z > Int32.MaxValue )
			{
				destination = Point3D.Zero;
				return false;
			}

			destination = new Point3D( (int) x, (int) y, (int) z );
			return true;
		}

		private static MoveTransactionState CaptureMoveStates( IList<HouseFoundation> members )
		{
			List<HouseMoveState> states = new List<HouseMoveState>( members.Count );

			for ( int i = 0; i < members.Count; ++i )
			{
				HouseMoveState state = new HouseMoveState();
				state.House = members[i];
				state.Map = members[i].Map;
				state.Location = members[i].Location;
				states.Add( state );
			}

			HousingMoveEntitySet entitySet = HousingMoveEntitySet.Capture( members );
			List<MoveEntityState> entityStates = new List<MoveEntityState>( entitySet.Entities.Count );

			for ( int i = 0; i < entitySet.Entities.Count; ++i )
			{
				IEntity entity = entitySet.Entities[i];
				MoveEntityState entityState = new MoveEntityState();
				entityState.Entity = entity;
				entityState.Map = entity.Map;
				entityState.Location = entity.Location;

				Item item = entity as Item;
				MovingCrate movingCrate = item as MovingCrate;

				if ( item != null )
					entityState.Parent = item.Parent;

				entityState.AllowDeletedOnRollback = movingCrate != null && movingCrate.Items.Count == 0;

				entityStates.Add( entityState );
			}

			MoveTransactionState transaction = new MoveTransactionState();
			transaction.Houses = states;
			transaction.Entities = entityStates;
			return transaction;
		}

		private static MoveTransactionState CaptureMoveStates( IList<BaseHouse> houses )
		{
			List<HouseMoveState> states = new List<HouseMoveState>( houses.Count );

			for ( int i = 0; i < houses.Count; ++i )
			{
				HouseMoveState state = new HouseMoveState();
				state.House = houses[i];
				state.Map = houses[i].Map;
				state.Location = houses[i].Location;
				states.Add( state );
			}

			HousingMoveEntitySet entitySet = HousingMoveEntitySet.Capture( houses );
			List<MoveEntityState> entityStates = CaptureEntityStates( entitySet );
			MoveTransactionState transaction = new MoveTransactionState();
			transaction.Houses = states;
			transaction.Entities = entityStates;
			return transaction;
		}

		private static List<MoveEntityState> CaptureEntityStates( HousingMoveEntitySet entitySet )
		{
			List<MoveEntityState> entityStates = new List<MoveEntityState>( entitySet.Entities.Count );

			for ( int i = 0; i < entitySet.Entities.Count; ++i )
			{
				IEntity entity = entitySet.Entities[i];
				MoveEntityState entityState = new MoveEntityState();
				entityState.Entity = entity;
				entityState.Map = entity.Map;
				entityState.Location = entity.Location;

				Item item = entity as Item;
				MovingCrate movingCrate = item as MovingCrate;

				if ( item != null )
					entityState.Parent = item.Parent;

				entityState.AllowDeletedOnRollback = movingCrate != null && movingCrate.Items.Count == 0;
				entityStates.Add( entityState );
			}

			return entityStates;
		}

		private static void MarkMoveStarted( IList<HouseMoveState> states, BaseHouse house )
		{
			for ( int i = 0; i < states.Count; ++i )
			{
				if ( states[i].House == house )
				{
					states[i].MoveStarted = true;
					return;
				}
			}
		}

		private static bool VerifyEstateAfterMove( HousingEstateController estate, List<HouseFoundation> members, HousingEstateCapacityTotals expectedTotals, out string error )
		{
			error = String.Empty;
			HousingValidationResult connected = HousingEstateValidation.ValidateConnectedEstate( members );

			if ( !connected.IsValid )
			{
				error = connected.Message;
				return false;
			}

			if ( !VerifyRegisteredMembersAndCapacity( estate, members, out error ) )
				return false;

			HousingEstateCapacityTotals currentTotals;

			if ( !HousingEstateRegistry.TryGetCapacityTotals( members[0], out currentTotals ) || !CapacityEquals( expectedTotals, currentTotals ) )
			{
				error = "Estate capacity totals changed during movement.";
				return false;
			}

			return true;
		}

		private static void RestoreMoveEntity( MoveEntityState state )
		{
			if ( state == null || state.Entity == null )
				throw new InvalidOperationException( "A captured movement entity is missing." );

			Item item = state.Entity as Item;
			Mobile mobile = state.Entity as Mobile;

			if ( item != null )
			{
				if ( item.Deleted )
				{
					if ( state.AllowDeletedOnRollback )
						return;

					throw new InvalidOperationException( "Captured item " + item.Serial + " was deleted." );
				}

				if ( state.Parent != null )
				{
					Item parentItem = state.Parent as Item;
					Mobile parentMobile = state.Parent as Mobile;

					if ( ( parentItem != null && parentItem.Deleted ) || ( parentMobile != null && parentMobile.Deleted ) )
						throw new InvalidOperationException( "Captured parent for item " + item.Serial + " was deleted." );

					if ( !Object.ReferenceEquals( item.Parent, state.Parent ) )
					{
						if ( parentItem != null )
							parentItem.AddItem( item );
						else if ( parentMobile != null )
							parentMobile.AddItem( item );
						else
							throw new InvalidOperationException( "Captured item parent has an unsupported type." );
					}

					if ( item.Map != state.Map )
						item.Map = state.Map;

					if ( item.Location != state.Location )
						item.Location = state.Location;
				}
				else if ( item.Parent != null || item.Map != state.Map || item.Location != state.Location )
				{
					item.MoveToWorld( state.Location, state.Map );
				}
			}
			else if ( mobile != null )
			{
				if ( mobile.Deleted )
					throw new InvalidOperationException( "Captured mobile " + mobile.Serial + " was deleted." );

				if ( mobile.Map != state.Map || mobile.Location != state.Location )
					mobile.MoveToWorld( state.Location, state.Map );
			}
			else
			{
				throw new InvalidOperationException( "Captured entity has an unsupported type." );
			}
		}

		private static bool VerifyMoveEntity( MoveEntityState state )
		{
			if ( state == null || state.Entity == null )
				return false;

			Item item = state.Entity as Item;

			if ( item != null )
			{
				if ( item.Deleted && state.AllowDeletedOnRollback )
					return true;

				return !item.Deleted && Object.ReferenceEquals( item.Parent, state.Parent ) && item.Map == state.Map && item.Location == state.Location;
			}

			Mobile mobile = state.Entity as Mobile;

			return mobile != null && !mobile.Deleted && mobile.Map == state.Map && mobile.Location == state.Location;
		}

		private static bool RollbackMove( Mobile actor, HousingEstateController estate, IList<HouseFoundation> members, MoveTransactionState transaction, HousingEstateSnapshot rollback, HousingEstateCapacityTotals expectedTotals, int previousRevision, int previousRevisionCount, out string error )
		{
			error = String.Empty;
			IList<HouseMoveState> states = transaction.Houses;

			for ( int i = states.Count - 1; i >= 0; --i )
			{
				HouseMoveState state = states[i];

				if ( !state.MoveStarted )
					continue;

				try
				{
					if ( state.House == null || state.House.Deleted )
						AppendRollbackError( ref error, "a moved house became missing or deleted" );
					else
						state.House.MoveToWorld( state.Location, state.Map );
				}
				catch ( Exception exception )
				{
					AppendRollbackError( ref error, "reverse movement failed: " + ConciseReason( exception ) );
				}
			}

			for ( int i = 0; i < transaction.Entities.Count; ++i )
			{
				MoveEntityState state = transaction.Entities[i];

				try
				{
					RestoreMoveEntity( state );
				}
				catch ( Exception exception )
				{
					AppendRollbackError( ref error, "subordinate restore failed: " + ConciseReason( exception ) );
				}
			}

			try
			{
				for ( int i = 0; i < transaction.Entities.Count; ++i )
				{
					if ( !VerifyMoveEntity( transaction.Entities[i] ) )
						AppendRollbackError( ref error, "an exact movement entity did not return to its captured state" );
				}

				if ( !rollback.VerifyLiveState() )
				{
					HousingValidationResult restore = rollback.RestoreLiveState( actor );

					if ( !restore.IsValid )
						AppendRollbackError( ref error, "snapshot restore failed: " + restore.Message );
				}
			}
			catch ( Exception exception )
			{
				AppendRollbackError( ref error, "snapshot restore failed: " + ConciseReason( exception ) );
			}

			if ( estate != null )
			{
				for ( int i = 0; i < members.Count; ++i )
				{
					if ( HousingEstateRegistry.IsRegisteredTo( members[i], estate ) )
						continue;

					string mappingError;

					if ( !HousingEstateRegistry.RestoreOwnedMapping( members[i], estate, out mappingError ) )
						AppendRollbackError( ref error, "registry restore failed: " + mappingError );
				}
			}

			try
			{
				if ( !rollback.VerifyLiveState() )
					AppendRollbackError( ref error, "the original movement snapshot did not verify" );

				if ( estate != null )
				{
					string verificationError;

					if ( !VerifyEstateAfterMove( estate, new List<HouseFoundation>( members ), expectedTotals, out verificationError ) )
						AppendRollbackError( ref error, verificationError );

					if ( estate.CurrentRevision != previousRevision || estate.RecentRevisionCount != previousRevisionCount )
						AppendRollbackError( ref error, "the estate revision ledger changed during failed movement" );
				}
				else
				{
					HousingEstateController unexpected;

					if ( members.Count != 1 || HousingEstateRegistry.TryGetEstate( members[0], out unexpected ) )
						AppendRollbackError( ref error, "standalone rollback has unexpected estate membership" );
				}
			}
			catch ( Exception exception )
			{
				AppendRollbackError( ref error, "rollback verification failed: " + ConciseReason( exception ) );
			}

			return error.Length == 0;
		}

		private static bool RollbackStandaloneMove( MoveTransactionState transaction, out string error )
		{
			error = String.Empty;

			for ( int i = transaction.Houses.Count - 1; i >= 0; --i )
			{
				HouseMoveState state = transaction.Houses[i];

				if ( !state.MoveStarted )
					continue;

				try
				{
					if ( state.House == null || state.House.Deleted )
						AppendRollbackError( ref error, "a moved standalone house became missing or deleted" );
					else
						state.House.MoveToWorld( state.Location, state.Map );
				}
				catch ( Exception exception )
				{
					AppendRollbackError( ref error, "reverse standalone movement failed: " + ConciseReason( exception ) );
				}
			}

			for ( int i = 0; i < transaction.Entities.Count; ++i )
			{
				try
				{
					RestoreMoveEntity( transaction.Entities[i] );
				}
				catch ( Exception exception )
				{
					AppendRollbackError( ref error, "standalone subordinate restore failed: " + ConciseReason( exception ) );
				}
			}

			for ( int i = 0; i < transaction.Houses.Count; ++i )
			{
				HouseMoveState state = transaction.Houses[i];

				if ( state.House == null || state.House.Deleted || state.House.Map != state.Map || state.House.Location != state.Location )
					AppendRollbackError( ref error, "a standalone house did not return to its captured state" );
			}

			for ( int i = 0; i < transaction.Entities.Count; ++i )
			{
				if ( !VerifyMoveEntity( transaction.Entities[i] ) )
					AppendRollbackError( ref error, "a standalone movement entity did not return to its captured state" );
			}

			return error.Length == 0;
		}

		private static void MarkRollbackFailure( HousingEstateController estate, HousingEstateOperation operation, string rollbackError )
		{
			if ( estate != null && !estate.Deleted )
				estate.MarkDegraded( operation + " rollback verification failed: " + ( rollbackError ?? String.Empty ) );
		}

		private sealed class ForceDissolveMemberState
		{
			public HouseFoundation Foundation;
			public bool WasMapped;
		}

		private static List<ForceDissolveMemberState> CaptureForceDissolveState( Mobile actor, HousingEstateController estate, Serial estateSerial )
		{
			List<ForceDissolveMemberState> states = new List<ForceDissolveMemberState>();

			for ( int i = 0; i < estate.MemberCount; ++i )
			{
				Serial memberSerial = estate.GetMemberSerial( i );
				Item item = World.FindItem( memberSerial );

				if ( item == null || item.Deleted )
				{
					LogSkippedMember( actor, estateSerial, memberSerial, "missing or deleted" );
					continue;
				}

				HouseFoundation foundation = item as HouseFoundation;

				if ( foundation == null )
				{
					LogSkippedMember( actor, estateSerial, memberSerial, "wrong item type" );
					continue;
				}

				bool duplicate = false;

				for ( int j = 0; j < states.Count; ++j )
				{
					if ( states[j].Foundation == foundation )
					{
						duplicate = true;
						break;
					}
				}

				if ( duplicate )
					continue;

				ForceDissolveMemberState state = new ForceDissolveMemberState();
				state.Foundation = foundation;
				state.WasMapped = HousingEstateRegistry.IsRegisteredTo( foundation, estate );
				states.Add( state );
			}

			return states;
		}

		private static bool ValidateActor( Mobile actor, AccessLevel minimumAccess, out string error )
		{
			error = String.Empty;

			if ( actor == null || actor.Deleted )
			{
				error = "A live staff actor is required.";
				return false;
			}

			if ( actor.AccessLevel < minimumAccess )
			{
				error = "This operation requires " + minimumAccess + " access.";
				return false;
			}

			return true;
		}

		private static bool ResolveMembers( IList<Serial> memberSerials, out List<HouseFoundation> members, out string error )
		{
			members = new List<HouseFoundation>();
			error = String.Empty;

			if ( memberSerials == null )
			{
				error = "No house serials were supplied.";
				return false;
			}

			for ( int i = 0; i < memberSerials.Count; ++i )
			{
				Serial serial = memberSerials[i];

				for ( int j = 0; j < i; ++j )
				{
					if ( memberSerials[j] == serial )
					{
						error = "House serial " + serial + " was supplied more than once.";
						return false;
					}
				}

				Item item = World.FindItem( serial );

				if ( item == null || item.Deleted )
				{
					error = "House serial " + serial + " is missing or deleted.";
					return false;
				}

				HouseFoundation foundation = item as HouseFoundation;

				if ( foundation == null )
				{
					error = "Item " + serial + " is not a house foundation.";
					return false;
				}

				members.Add( foundation );
			}

			members.Sort( CompareFoundations );
			return true;
		}

		private static int CompareFoundations( HouseFoundation left, HouseFoundation right )
		{
			return left.Serial.Value.CompareTo( right.Serial.Value );
		}

		private static bool ResolveEstate( Serial estateSerial, out HousingEstateController estate, out string error )
		{
			estate = null;
			error = String.Empty;
			Item item = World.FindItem( estateSerial );

			if ( item == null || item.Deleted )
			{
				error = "The estate controller is missing or deleted.";
				return false;
			}

			estate = item as HousingEstateController;

			if ( estate == null )
			{
				error = "The supplied serial is not an estate controller.";
				return false;
			}

			return true;
		}

		private static bool ResolveHealthyEstate( Serial estateSerial, out HousingEstateController estate, out string error )
		{
			if ( !ResolveEstate( estateSerial, out estate, out error ) )
				return false;

			if ( estate.Health != HousingEstateHealth.Healthy )
			{
				error = String.IsNullOrEmpty( estate.DegradedReason ) ? "The estate is degraded." : estate.DegradedReason;
				return false;
			}

			HousingValidationResult validation = estate.ValidateMembers();

			if ( !validation.IsValid )
			{
				error = validation.Message;
				estate.MarkDegraded( error );
				return false;
			}

			List<HouseFoundation> members = estate.CopyMembers();

			if ( !VerifyRegisteredMembers( estate, members ) )
			{
				error = "The estate has invalid registry membership.";
				estate.MarkDegraded( error );
				return false;
			}

			return true;
		}

		private static bool VerifyRegisteredMembersAndCapacity( HousingEstateController estate, List<HouseFoundation> members, out string error )
		{
			error = String.Empty;

			if ( !VerifyRegisteredMembers( estate, members ) )
			{
				error = "One or more houses were not registered to the new estate.";
				return false;
			}

			HousingEstateCapacityTotals expected;

			if ( members.Count == 0 || !HousingEstateRegistry.TryGetCapacityTotals( members[0], out expected ) )
			{
				error = "Pooled estate capacity could not be obtained.";
				return false;
			}

			for ( int i = 1; i < members.Count; ++i )
			{
				HousingEstateCapacityTotals actual;

				if ( !HousingEstateRegistry.TryGetCapacityTotals( members[i], out actual ) || !CapacityEquals( expected, actual ) )
				{
					error = "Pooled estate capacity was inconsistent between members.";
					return false;
				}
			}

			return true;
		}

		private static bool VerifyRegisteredMembers( HousingEstateController estate, IList<HouseFoundation> members )
		{
			for ( int i = 0; i < members.Count; ++i )
			{
				if ( !HousingEstateRegistry.IsRegisteredTo( members[i], estate ) )
					return false;
			}

			return true;
		}

		private static bool HasOwnedMapping( HousingEstateController estate, IList<HouseFoundation> members )
		{
			for ( int i = 0; i < members.Count; ++i )
			{
				if ( HousingEstateRegistry.IsRegisteredTo( members[i], estate ) )
					return true;
			}

			return false;
		}

		private static bool CapacityEquals( HousingEstateCapacityTotals left, HousingEstateCapacityTotals right )
		{
			return left.MaxStorage == right.MaxStorage
				&& left.CurrentStorage == right.CurrentStorage
				&& left.MaxLockdowns == right.MaxLockdowns
				&& left.CurrentLockdowns == right.CurrentLockdowns
				&& left.MaxVendors == right.MaxVendors
				&& left.CurrentVendors == right.CurrentVendors;
		}

		private static bool RollbackCombine( HousingEstateController estate, HousingEstateSnapshot original, IList<HouseFoundation> members, out string error )
		{
			error = String.Empty;

			try
			{
				if ( estate != null )
					HousingEstateRegistry.Unregister( estate );
			}
			catch ( Exception exception )
			{
				AppendRollbackError( ref error, "unregister failed: " + ConciseReason( exception ) );
			}

			try
			{
				if ( original != null )
				{
					HousingValidationResult restore = original.RestoreLiveState();

					if ( !restore.IsValid )
						AppendRollbackError( ref error, "snapshot restore failed: " + restore.Message );
				}
			}
			catch ( Exception exception )
			{
				AppendRollbackError( ref error, "snapshot restore failed: " + ConciseReason( exception ) );
			}

			try
			{
				if ( estate != null && !estate.Deleted )
					estate.Delete();
			}
			catch ( Exception exception )
			{
				AppendRollbackError( ref error, "controller delete failed: " + ConciseReason( exception ) );
			}

			try
			{
				if ( estate != null && ( !estate.Deleted || ( members != null && HasOwnedMapping( estate, members ) ) ) )
					AppendRollbackError( ref error, "the incomplete controller or one of its mappings survived cleanup" );
			}
			catch ( Exception exception )
			{
				AppendRollbackError( ref error, "cleanup verification failed: " + ConciseReason( exception ) );
			}

			try
			{
				if ( original != null && !original.VerifyLiveState() )
					AppendRollbackError( ref error, "the original house state was not restored" );
			}
			catch ( Exception exception )
			{
				AppendRollbackError( ref error, "state verification failed: " + ConciseReason( exception ) );
			}

			return error.Length == 0;
		}

		private static void AppendRollbackError( ref string error, string detail )
		{
			if ( String.IsNullOrEmpty( detail ) )
				return;

			if ( error.Length > 0 )
				error += "; ";

			error += detail;
		}

		private static bool RollbackSplit( HousingEstateController estate, IList<HouseFoundation> members, HousingEstateSnapshot rollback, HousingEstateHealth health, string reason, out string error )
		{
			error = String.Empty;

			try
			{
				HousingValidationResult restore = rollback.RestoreLiveState();

				if ( !restore.IsValid )
				{
					error = restore.Message;
					return false;
				}

				if ( !HousingEstateRegistry.Register( estate, out error ) )
					return false;

				if ( !rollback.VerifyLiveState() || !VerifyRegisteredMembers( estate, members ) )
				{
					error = "The split rollback state or registry membership did not verify.";
					return false;
				}

				estate.RestoreHealth( health, reason );
				return true;
			}
			catch ( Exception exception )
			{
				error = ConciseReason( exception );
				return false;
			}
		}

		private static bool RollbackRestore( HousingEstateController estate, IList<HouseFoundation> members, HousingEstateSnapshot rollback, HousingEstateHealth health, string reason, out string error )
		{
			error = String.Empty;

			try
			{
				HousingValidationResult restore = rollback.RestoreLiveState( rollback.Actor );

				if ( !restore.IsValid )
				{
					error = restore.Message;
					return false;
				}

				if ( !rollback.VerifyLiveState() || !VerifyRegisteredMembers( estate, members ) )
				{
					error = "The restore rollback state or registry membership did not verify.";
					return false;
				}

				estate.RestoreHealth( health, reason );
				return true;
			}
			catch ( Exception exception )
			{
				error = ConciseReason( exception );
				return false;
			}
		}

		private static bool RollbackForceDissolve( HousingEstateController estate, IList<ForceDissolveMemberState> states, HousingEstateSnapshot rollback, out string error )
		{
			error = String.Empty;

			try
			{
				HousingValidationResult restore = rollback.RestoreLiveState();

				if ( !restore.IsValid )
					AppendRollbackError( ref error, restore.Message );

				for ( int i = 0; i < states.Count; ++i )
				{
					ForceDissolveMemberState state = states[i];

					if ( !state.WasMapped )
						continue;

					string mappingError;

					if ( !HousingEstateRegistry.RestoreOwnedMapping( state.Foundation, estate, out mappingError ) )
						AppendRollbackError( ref error, mappingError );
				}

				if ( !rollback.VerifyLiveState() )
					AppendRollbackError( ref error, "the surviving state did not verify" );

				for ( int i = 0; i < states.Count; ++i )
				{
					if ( states[i].WasMapped && !HousingEstateRegistry.IsRegisteredTo( states[i].Foundation, estate ) )
						AppendRollbackError( ref error, "a saved registry mapping was not restored" );
				}
			}
			catch ( Exception exception )
			{
				AppendRollbackError( ref error, ConciseReason( exception ) );
			}

			return error.Length == 0;
		}

		private static bool CompleteForceDissolveCleanup( Mobile actor, HousingEstateController estate, Serial estateSerial, IList<ForceDissolveMemberState> states, out string error )
		{
			error = String.Empty;

			for ( int i = 0; i < states.Count; ++i )
			{
				HouseFoundation foundation = states[i].Foundation;

				try
				{
					HousingEstateRegistry.UnregisterMember( foundation, estate );

					string decayError = String.Empty;

					if ( estate.OriginalSnapshot == null || !estate.OriginalSnapshot.TryRestoreDecay( foundation, out decayError ) )
						LogSkippedMember( actor, estateSerial, foundation.Serial, String.IsNullOrEmpty( decayError ) ? "original decay state unavailable during cleanup" : decayError );
				}
				catch ( Exception exception )
				{
					AppendRollbackError( ref error, "member cleanup failed: " + ConciseReason( exception ) );
				}
			}

			try
			{
				HousingEstateRegistry.Unregister( estate );
			}
			catch ( Exception exception )
			{
				AppendRollbackError( ref error, "registry cleanup failed: " + ConciseReason( exception ) );
			}

			try
			{
				if ( !estate.Deleted )
					estate.Delete();
			}
			catch ( Exception exception )
			{
				AppendRollbackError( ref error, "controller cleanup failed: " + ConciseReason( exception ) );
			}

			if ( !estate.Deleted )
				AppendRollbackError( ref error, "the degraded controller survived cleanup" );

			for ( int i = 0; i < states.Count; ++i )
			{
				if ( HousingEstateRegistry.IsRegisteredTo( states[i].Foundation, estate ) )
					AppendRollbackError( ref error, "an owned mapping survived cleanup" );
			}

			return error.Length == 0;
		}

		private static string ConciseReason( Exception exception )
		{
			string reason = exception == null ? "Unknown transaction failure." : exception.GetType().Name + ": " + exception.Message;
			reason = reason.Replace( '\r', ' ' ).Replace( '\n', ' ' );

			if ( reason.Length > 240 )
				reason = reason.Substring( 0, 240 );

			return reason;
		}

		private static void Log( Mobile actor, HousingEstateOperation operation, Serial estateSerial, int revision, bool success, string reason )
		{
			if ( actor == null || actor.Deleted )
				return;

			CommandLogging.WriteLine( actor, "{0} {1} HousingEstate operation={2} estate={3} revision={4} Force={5} outcome={6} reason={7}", actor.AccessLevel, CommandLogging.Format( actor ), operation, estateSerial, revision, operation == HousingEstateOperation.ForceDissolve, success ? "success" : "failure", reason ?? String.Empty );
		}

		private static string FormatMemberSerials( IList<HouseFoundation> members )
		{
			if ( members == null || members.Count == 0 )
				return "[]";

			string value = "[";

			for ( int i = 0; i < members.Count; ++i )
			{
				if ( i > 0 )
					value += ",";

				value += members[i] == null ? "null" : members[i].Serial.ToString();
			}

			return value + "]";
		}

		private static string FormatForceMemberSerials( IList<ForceDissolveMemberState> states )
		{
			if ( states == null || states.Count == 0 )
				return "[]";

			string value = "[";

			for ( int i = 0; i < states.Count; ++i )
			{
				if ( i > 0 )
					value += ",";

				value += states[i].Foundation == null ? "null" : states[i].Foundation.Serial.ToString();
			}

			return value + "]";
		}

		private static void LogMove( Mobile actor, HousingEstateOperation operation, Serial serial, Map sourceMap, Point3D source, Map destinationMap, Point3D destination, int deltaX, int deltaY, int deltaZ, bool force, bool success, string reason )
		{
			if ( actor == null || actor.Deleted )
				return;

			string sourceName = sourceMap == null ? "null" : sourceMap.ToString();
			string destinationName = destinationMap == null ? "null" : destinationMap.ToString();
			CommandLogging.WriteLine( actor, "{0} {1} HousingEstate operation={2} serial={3} sourceMap={4} source={5} destinationMap={6} destination={7} delta=({8},{9},{10}) Force={11} outcome={12} reason={13}", actor.AccessLevel, CommandLogging.Format( actor ), operation, serial, sourceName, source, destinationName, destination, deltaX, deltaY, deltaZ, force, success ? "success" : "failure", reason ?? String.Empty );
		}

		private static void LogSkippedMember( Mobile actor, Serial estateSerial, Serial memberSerial, string reason )
		{
			if ( actor == null || actor.Deleted )
				return;

			CommandLogging.WriteLine( actor, "{0} {1} HousingEstate operation={2} estate={3} member={4} outcome=skip reason={5}", actor.AccessLevel, CommandLogging.Format( actor ), HousingEstateOperation.ForceDissolve, estateSerial, memberSerial, reason ?? String.Empty );
		}
	}
}
