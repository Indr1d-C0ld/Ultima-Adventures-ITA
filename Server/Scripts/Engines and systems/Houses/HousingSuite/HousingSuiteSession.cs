using System;
using System.Collections.Generic;

namespace Server.Multis
{
	public enum HousingSuiteView
	{
		Dashboard,
		HouseSelection,
		MoveSourceSelection,
		DestinationSelection,
		MoveReview,
		Paused
	}

	public sealed class HousingSuiteSession
	{
		private const int MaximumSelectedHouses = 256;
		private static readonly Dictionary<Serial, HousingSuiteSession> m_Sessions = new Dictionary<Serial, HousingSuiteSession>();
		private static int m_NextSessionToken;

		internal static int ActiveCount { get { return m_Sessions.Count; } }

		private readonly Serial m_ActorSerial;
		private readonly int m_SessionToken;
		private readonly List<Serial> m_SelectedHouseSerials;
		private int m_StateVersion;
		private Serial m_SelectedEstateSerial;
		private HousingSuiteView m_View;
		private int m_ExpectedEstateRevision;
		private bool m_HasPendingDestination;
		private Point3D m_PendingDestination;
		private int m_PendingDestinationMapIndex;
		private int m_PendingDeltaX;
		private int m_PendingDeltaY;
		private int m_PendingDeltaZ;
		private bool m_Force;
		private int m_ConfirmationToken;
		private int m_NearbyRange;
		private int m_NextMovementUndoToken;
		private HousingSuiteMovementUndoRecord m_PendingMovementUndo;

		public Serial ActorSerial { get { return m_ActorSerial; } }
		public int SessionToken { get { return m_SessionToken; } }
		public int StateVersion { get { return m_StateVersion; } }
		public int SelectedHouseCount { get { return m_SelectedHouseSerials.Count; } }
		public Serial SelectedEstateSerial { get { return m_SelectedEstateSerial; } }
		public int ExpectedEstateRevision { get { return m_ExpectedEstateRevision; } }
		public bool HasPendingDestination { get { return m_HasPendingDestination; } }
		public Point3D PendingDestination { get { return m_PendingDestination; } }
		public int PendingDestinationMapIndex { get { return m_PendingDestinationMapIndex; } }
		public int PendingDeltaX { get { return m_PendingDeltaX; } }
		public int PendingDeltaY { get { return m_PendingDeltaY; } }
		public int PendingDeltaZ { get { return m_PendingDeltaZ; } }
		public int NearbyRange { get { return m_NearbyRange; } }
		public HousingSuiteMovementUndoRecord PendingMovementUndo { get { return m_PendingMovementUndo; } }

		public IList<Serial> SelectedHouseSerials
		{
			get { return new List<Serial>( m_SelectedHouseSerials ).AsReadOnly(); }
		}

		public HousingSuiteView View
		{
			get { return m_View; }
			set
			{
				if ( m_View != value )
				{
					m_View = value;
					Touch();
				}
			}
		}

		public bool Force
		{
			get { return m_Force; }
			set
			{
				if ( m_Force != value )
				{
					m_Force = value;
					Touch();
				}
			}
		}

		public int ConfirmationToken
		{
			get { return m_ConfirmationToken; }
			set
			{
				if ( m_ConfirmationToken != value )
				{
					m_ConfirmationToken = value;
					Touch();
				}
			}
		}

		private HousingSuiteSession( Serial actorSerial, int sessionToken )
		{
			m_ActorSerial = actorSerial;
			m_SessionToken = sessionToken;
			m_SelectedHouseSerials = new List<Serial>();
			m_SelectedEstateSerial = Serial.MinusOne;
			m_View = HousingSuiteView.Dashboard;
			m_PendingDestinationMapIndex = -1;
			m_NearbyRange = HousingSuiteCommand.DefaultNearbyRange;
		}

		public static HousingSuiteSession Get( Mobile actor )
		{
			if ( !IsCurrentStaffActor( actor ) )
				return null;

			HousingSuiteSession session;

			if ( !m_Sessions.TryGetValue( actor.Serial, out session ) )
			{
				session = new HousingSuiteSession( actor.Serial, NextSessionToken() );
				m_Sessions.Add( actor.Serial, session );
			}

			return session;
		}

		public static HousingSuiteSession Find( Mobile actor )
		{
			if ( actor == null )
				return null;

			if ( !IsCurrentStaffActor( actor ) )
			{
				m_Sessions.Remove( actor.Serial );
				return null;
			}

			HousingSuiteSession session;
			return m_Sessions.TryGetValue( actor.Serial, out session ) ? session : null;
		}

		public static void Close( Mobile actor )
		{
			if ( actor != null )
				m_Sessions.Remove( actor.Serial );
		}

		internal static void Close( Serial actorSerial, int sessionToken )
		{
			HousingSuiteSession session;

			if ( m_Sessions.TryGetValue( actorSerial, out session ) && session.SessionToken == sessionToken )
				m_Sessions.Remove( actorSerial );
		}

		internal static bool IsCurrentStaffActor( Mobile actor )
		{
			return actor != null
				&& !actor.Deleted
				&& actor.AccessLevel >= AccessLevel.GameMaster
				&& World.FindMobile( actor.Serial ) == actor;
		}

		public Serial GetSelectedHouseSerial( int index )
		{
			if ( index < 0 || index >= m_SelectedHouseSerials.Count )
				throw new ArgumentOutOfRangeException( "index" );

			return m_SelectedHouseSerials[index];
		}

		public bool AddSelectedHouse( Serial houseSerial )
		{
			if ( houseSerial == Serial.MinusOne || m_SelectedHouseSerials.Count >= MaximumSelectedHouses )
				return false;

			for ( int i = 0; i < m_SelectedHouseSerials.Count; ++i )
			{
				if ( m_SelectedHouseSerials[i] == houseSerial )
					return false;
			}

			m_SelectedHouseSerials.Add( houseSerial );
			Touch();
			return true;
		}

		public bool RemoveSelectedHouse( Serial houseSerial )
		{
			for ( int i = 0; i < m_SelectedHouseSerials.Count; ++i )
			{
				if ( m_SelectedHouseSerials[i] != houseSerial )
					continue;

				m_SelectedHouseSerials.RemoveAt( i );
				Touch();
				ClearSelectedEstate();
				ClearPendingDestination();
				return true;
			}

			return false;
		}

		public bool TrySetNearbyRange( int range )
		{
			if ( range < HousingSuiteCommand.MinimumNearbyRange || range > HousingSuiteCommand.MaximumNearbyRange )
				return false;

			if ( m_NearbyRange != range )
			{
				m_NearbyRange = range;
				Touch();
			}

			return true;
		}

		public void ClearSelectedHouses()
		{
			if ( m_SelectedHouseSerials.Count > 0 )
			{
				m_SelectedHouseSerials.Clear();
				Touch();
			}

			ClearSelectedEstate();
			ClearPendingDestination();
		}

		public void SetSelectedEstate( Serial estateSerial, int expectedRevision )
		{
			int revision = expectedRevision < 0 ? 0 : expectedRevision;

			if ( m_SelectedEstateSerial != estateSerial || m_ExpectedEstateRevision != revision )
			{
				m_SelectedEstateSerial = estateSerial;
				m_ExpectedEstateRevision = revision;
				Touch();
			}
		}

		public void ClearSelectedEstate()
		{
			if ( m_SelectedEstateSerial != Serial.MinusOne || m_ExpectedEstateRevision != 0 )
			{
				m_SelectedEstateSerial = Serial.MinusOne;
				m_ExpectedEstateRevision = 0;
				Touch();
			}
		}

		public void SetPendingDestination( Point3D destination, int mapIndex )
		{
			SetPendingDestination( destination, mapIndex, 0, 0, 0 );
		}

		public void SetPendingDestination( Point3D destination, int mapIndex, int deltaX, int deltaY, int deltaZ )
		{
			if ( !m_HasPendingDestination
				|| m_PendingDestination != destination
				|| m_PendingDestinationMapIndex != mapIndex
				|| m_PendingDeltaX != deltaX
				|| m_PendingDeltaY != deltaY
				|| m_PendingDeltaZ != deltaZ )
			{
				m_HasPendingDestination = true;
				m_PendingDestination = destination;
				m_PendingDestinationMapIndex = mapIndex;
				m_PendingDeltaX = deltaX;
				m_PendingDeltaY = deltaY;
				m_PendingDeltaZ = deltaZ;
				Touch();
			}
		}

		public void ClearPendingDestination()
		{
			if ( m_HasPendingDestination
				|| m_PendingDestination != Point3D.Zero
				|| m_PendingDestinationMapIndex != -1
				|| m_PendingDeltaX != 0
				|| m_PendingDeltaY != 0
				|| m_PendingDeltaZ != 0 )
			{
				m_HasPendingDestination = false;
				m_PendingDestination = Point3D.Zero;
				m_PendingDestinationMapIndex = -1;
				m_PendingDeltaX = 0;
				m_PendingDeltaY = 0;
				m_PendingDeltaZ = 0;
				Touch();
			}
		}

		internal int IssueMovementUndoToken()
		{
			if ( m_NextMovementUndoToken == Int32.MaxValue )
				m_NextMovementUndoToken = 0;

			return ++m_NextMovementUndoToken;
		}

		public bool SetPendingMovementUndo( HousingSuiteMovementUndoRecord record )
		{
			if ( record == null || record.ActorSerial != m_ActorSerial || record.SessionToken != m_SessionToken )
				return false;

			if ( m_PendingMovementUndo != record )
			{
				m_PendingMovementUndo = record;
				Touch();
			}

			return true;
		}

		public HousingSuiteMovementUndoRecord ConsumePendingMovementUndo( int expectedToken )
		{
			HousingSuiteMovementUndoRecord record = m_PendingMovementUndo;

			if ( record == null || record.Token != expectedToken )
				return null;

			m_PendingMovementUndo = null;
			Touch();
			return record;
		}

		public bool ClearPendingMovementUndo( int expectedToken )
		{
			return ConsumePendingMovementUndo( expectedToken ) != null;
		}

		public void ClearPendingMovementUndo()
		{
			if ( m_PendingMovementUndo != null )
			{
				m_PendingMovementUndo = null;
				Touch();
			}
		}

		private void Touch()
		{
			if ( m_StateVersion == Int32.MaxValue )
				m_StateVersion = 0;
			else
				++m_StateVersion;
		}

		private static int NextSessionToken()
		{
			if ( m_NextSessionToken == Int32.MaxValue )
				m_NextSessionToken = 0;

			return ++m_NextSessionToken;
		}
	}
}
