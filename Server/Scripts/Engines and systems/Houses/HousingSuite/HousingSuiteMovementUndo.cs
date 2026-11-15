using System;
using System.Collections.Generic;

namespace Server.Multis
{
	public enum HousingSuiteMovementUndoOperation
	{
		MoveHouse,
		MoveEstate,
		ElevateHouse,
		ElevateEstate
	}

	public sealed class HousingSuiteMovementUndoRecord
	{
		private readonly Serial m_ActorSerial;
		private readonly int m_SessionToken;
		private readonly int m_Token;
		private readonly bool m_MoveEstate;
		private readonly Serial[] m_AffectedHouseSerials;
		private readonly Serial m_EstateSerial;
		private readonly int m_EstateRevision;
		private readonly int m_SourceMapIndex;
		private readonly Point3D m_SourceAnchor;
		private readonly int m_DestinationMapIndex;
		private readonly Point3D m_DestinationAnchor;
		private readonly int m_DeltaX;
		private readonly int m_DeltaY;
		private readonly int m_DeltaZ;
		private readonly bool m_Force;
		private readonly HousingSuiteHouseFingerprint[] m_PostFingerprints;
		private readonly HousingSuiteMovementUndoOperation m_Operation;
		private readonly int m_SourceEstateRevision;

		public Serial ActorSerial { get { return m_ActorSerial; } }
		public int SessionToken { get { return m_SessionToken; } }
		public int Token { get { return m_Token; } }
		public bool MoveEstate { get { return m_MoveEstate; } }
		public Serial[] AffectedHouseSerials { get { return CopySerials( m_AffectedHouseSerials ); } }
		public Serial EstateSerial { get { return m_EstateSerial; } }
		public int EstateRevision { get { return m_EstateRevision; } }
		public int SourceMapIndex { get { return m_SourceMapIndex; } }
		public Point3D SourceAnchor { get { return m_SourceAnchor; } }
		public int DestinationMapIndex { get { return m_DestinationMapIndex; } }
		public Point3D DestinationAnchor { get { return m_DestinationAnchor; } }
		public int DeltaX { get { return m_DeltaX; } }
		public int DeltaY { get { return m_DeltaY; } }
		public int DeltaZ { get { return m_DeltaZ; } }
		public bool Force { get { return m_Force; } }
		public HousingSuiteMovementUndoOperation Operation { get { return m_Operation; } }
		public int SourceEstateRevision { get { return m_SourceEstateRevision; } }
		public int ResultingEstateRevision { get { return m_EstateRevision; } }
		internal HousingSuiteHouseFingerprint[] PostFingerprints { get { return CopyFingerprints( m_PostFingerprints ); } }

		internal HousingSuiteMovementUndoRecord( HousingSuiteSession session, bool moveEstate, Serial[] affectedHouseSerials, Serial estateSerial, int estateRevision, int sourceMapIndex, Point3D sourceAnchor, int destinationMapIndex, Point3D destinationAnchor, int deltaX, int deltaY, int deltaZ, bool force, HousingSuiteHouseFingerprint[] postFingerprints )
			: this( session, deltaX == 0 && deltaY == 0 && deltaZ != 0
				? ( moveEstate ? HousingSuiteMovementUndoOperation.ElevateEstate : HousingSuiteMovementUndoOperation.ElevateHouse )
				: ( moveEstate ? HousingSuiteMovementUndoOperation.MoveEstate : HousingSuiteMovementUndoOperation.MoveHouse ),
				moveEstate, affectedHouseSerials, estateSerial, estateRevision > 0 ? estateRevision - 1 : 0, estateRevision, sourceMapIndex, sourceAnchor, destinationMapIndex, destinationAnchor, deltaX, deltaY, deltaZ, force, postFingerprints )
		{
		}

		internal HousingSuiteMovementUndoRecord( HousingSuiteSession session, HousingSuiteMovementUndoOperation operation, bool moveEstate, Serial[] affectedHouseSerials, Serial estateSerial, int sourceEstateRevision, int resultingEstateRevision, int sourceMapIndex, Point3D sourceAnchor, int destinationMapIndex, Point3D destinationAnchor, int deltaX, int deltaY, int deltaZ, bool force, HousingSuiteHouseFingerprint[] postFingerprints )
		{
			if ( session == null )
				throw new ArgumentNullException( "session" );

			if ( affectedHouseSerials == null || postFingerprints == null || affectedHouseSerials.Length == 0 || affectedHouseSerials.Length != postFingerprints.Length )
				throw new ArgumentException( "Movement undo requires matching affected-house and fingerprint arrays." );

			m_ActorSerial = session.ActorSerial;
			m_SessionToken = session.SessionToken;
			m_Token = session.IssueMovementUndoToken();
			m_MoveEstate = moveEstate;
			m_Operation = operation;
			m_AffectedHouseSerials = CopySerials( affectedHouseSerials );
			m_EstateSerial = estateSerial;
			m_SourceEstateRevision = sourceEstateRevision;
			m_EstateRevision = resultingEstateRevision;
			m_SourceMapIndex = sourceMapIndex;
			m_SourceAnchor = sourceAnchor;
			m_DestinationMapIndex = destinationMapIndex;
			m_DestinationAnchor = destinationAnchor;
			m_DeltaX = deltaX;
			m_DeltaY = deltaY;
			m_DeltaZ = deltaZ;
			m_Force = force;
			m_PostFingerprints = CopyFingerprints( postFingerprints );
		}

		internal Serial GetAffectedHouseSerial( int index )
		{
			return m_AffectedHouseSerials[index];
		}

		internal int AffectedHouseCount { get { return m_AffectedHouseSerials.Length; } }
		internal HousingSuiteHouseFingerprint[] CopyPostFingerprints() { return CopyFingerprints( m_PostFingerprints ); }

		private static Serial[] CopySerials( Serial[] values )
		{
			Serial[] copy = new Serial[values.Length];
			Array.Copy( values, copy, values.Length );
			return copy;
		}

		private static HousingSuiteHouseFingerprint[] CopyFingerprints( HousingSuiteHouseFingerprint[] values )
		{
			HousingSuiteHouseFingerprint[] copy = new HousingSuiteHouseFingerprint[values.Length];
			Array.Copy( values, copy, values.Length );
			return copy;
		}
	}

	public static class HousingSuiteMovementUndo
	{
		internal static bool TryCreateAfterMovement( HousingSuiteSession session, HousingSuiteMovementUndoOperation operation, bool moveEstate, IList<BaseHouse> affected, Serial estateSerial, int sourceEstateRevision, Map sourceMap, Point3D sourceAnchor, Map destinationMap, Point3D destinationAnchor, int deltaX, int deltaY, int deltaZ, bool force, out HousingSuiteMovementUndoRecord record, out string error )
		{
			record = null;
			error = String.Empty;

			if ( session == null || affected == null || affected.Count == 0 || !HousingSuiteCommand.IsValidWorldMap( sourceMap ) || !HousingSuiteCommand.IsValidWorldMap( destinationMap ) )
			{
				error = "The completed movement could not be captured for undo.";
				return false;
			}

			Serial[] serials = new Serial[affected.Count];

			for ( int i = 0; i < affected.Count; ++i )
			{
				BaseHouse house = affected[i];

				if ( house == null || house.Deleted || World.FindItem( house.Serial ) != house )
				{
					error = "An affected house could not be resolved after movement.";
					return false;
				}

				serials[i] = house.Serial;
			}

			int estateRevision = 0;

			if ( estateSerial != Server.Serial.MinusOne )
			{
				HousingEstateController estate = World.FindItem( estateSerial ) as HousingEstateController;

				if ( estate == null || estate.Deleted || estate.Health != HousingEstateHealth.Healthy )
				{
					error = "The estate could not be resolved after movement.";
					return false;
				}

				estateRevision = estate.CurrentRevision;
			}

			record = new HousingSuiteMovementUndoRecord( session, operation, moveEstate, serials, estateSerial, sourceEstateRevision, estateRevision, sourceMap.MapIndex, sourceAnchor, destinationMap.MapIndex, destinationAnchor, deltaX, deltaY, deltaZ, force, HousingSuiteHouseFingerprint.CaptureAll( affected ) );
			return true;
		}

		public static bool TryUndo( Mobile actor, HousingSuiteSession session, int token, out string error, out bool hasEstateHistory )
		{
			error = String.Empty;
			hasEstateHistory = false;

			HousingSuiteSession currentSession = HousingSuiteSession.Find( actor );

			if ( session == null || currentSession != session || actor == null || session.ActorSerial != actor.Serial )
			{
				error = "The HousingSuite session is no longer current.";
				return false;
			}

			HousingSuiteMovementUndoRecord pending = session.PendingMovementUndo;

			if ( pending == null || pending.Token != token || pending.ActorSerial != actor.Serial || pending.SessionToken != session.SessionToken )
			{
				error = "This movement undo is stale or already handled.";
				return false;
			}

			hasEstateHistory = pending.EstateSerial != Server.Serial.MinusOne;

			HousingSuiteMovementUndoRecord record = session.ConsumePendingMovementUndo( token );

			if ( record == null )
			{
				error = "This movement undo is stale or already handled.";
				return false;
			}

			Map sourceMap = HousingSuiteCommand.ResolveWorldMap( record.SourceMapIndex );
			Map destinationMap = HousingSuiteCommand.ResolveWorldMap( record.DestinationMapIndex );

			if ( sourceMap == null || destinationMap == null || !HasConsistentDelta( record ) )
			{
				error = "The recorded movement coordinates are invalid.";
				return false;
			}

			List<BaseHouse> houses = new List<BaseHouse>( record.AffectedHouseCount );

			for ( int i = 0; i < record.AffectedHouseCount; ++i )
			{
				BaseHouse house = World.FindItem( record.GetAffectedHouseSerial( i ) ) as BaseHouse;

				if ( house == null || house.Deleted || World.FindItem( house.Serial ) != house )
				{
					error = "An affected house is missing or deleted.";
					return false;
				}

				houses.Add( house );
			}

			if ( houses[0].Map != destinationMap || houses[0].Location != record.DestinationAnchor || !HousingSuiteHouseFingerprint.MatchesAll( houses, record.CopyPostFingerprints() ) )
			{
				error = "An affected house changed after the movement.";
				return false;
			}

			HousingEstateController estate;

			if ( !ValidateEstateState( record, houses, out estate, out error ) )
				return false;

			bool success = record.MoveEstate
				? HousingEstateService.TryMoveEstate( actor, record.EstateSerial, sourceMap, record.SourceAnchor, record.Force, out error )
				: HousingEstateService.TryMoveHouse( actor, record.GetAffectedHouseSerial( 0 ), sourceMap, record.SourceAnchor, record.Force, out error );

			if ( success && estate != null )
				session.SetSelectedEstate( estate.Serial, estate.CurrentRevision );
			else if ( !success && String.IsNullOrEmpty( error ) )
				error = "The inverse movement was rejected.";

			return success;
		}

		private static bool ValidateEstateState( HousingSuiteMovementUndoRecord record, IList<BaseHouse> houses, out HousingEstateController estate, out string error )
		{
			estate = null;
			error = String.Empty;

			if ( record.EstateSerial == Server.Serial.MinusOne )
			{
				if ( record.MoveEstate || record.EstateRevision != 0 || houses.Count != 1 )
				{
					error = "The standalone movement record is inconsistent.";
					return false;
				}

				HouseFoundation foundation = houses[0] as HouseFoundation;
				HousingEstateController registered;

				if ( foundation != null && HousingEstateRegistry.TryGetEstate( foundation, out registered ) )
				{
					error = "The house gained estate membership after movement.";
					return false;
				}

				return true;
			}

			estate = World.FindItem( record.EstateSerial ) as HousingEstateController;

			if ( estate == null || estate.Deleted || estate.Health != HousingEstateHealth.Healthy || estate.CurrentRevision != record.EstateRevision )
			{
				error = "The estate revision or health changed after movement.";
				return false;
			}

			for ( int i = 0; i < houses.Count; ++i )
			{
				HouseFoundation member = houses[i] as HouseFoundation;
				HousingEstateController registered;

				if ( member == null || !HousingEstateRegistry.TryGetEstate( member, out registered ) || registered != estate )
				{
					error = "Estate membership changed after movement.";
					return false;
				}
			}

			if ( record.MoveEstate )
			{
				List<HouseFoundation> members = estate.CopyMembers();

				if ( members.Count != houses.Count )
				{
					error = "Estate membership changed after movement.";
					return false;
				}

				for ( int i = 0; i < members.Count; ++i )
				{
					if ( members[i] != houses[i] )
					{
						error = "Estate member order changed after movement.";
						return false;
					}
				}
			}
			else if ( houses.Count != 1 )
			{
				error = "A one-house undo contains multiple affected houses.";
				return false;
			}

			return true;
		}

		private static bool HasConsistentDelta( HousingSuiteMovementUndoRecord record )
		{
			return (long) record.SourceAnchor.X + record.DeltaX == record.DestinationAnchor.X
				&& (long) record.SourceAnchor.Y + record.DeltaY == record.DestinationAnchor.Y
				&& (long) record.SourceAnchor.Z + record.DeltaZ == record.DestinationAnchor.Z;
		}
	}
}
