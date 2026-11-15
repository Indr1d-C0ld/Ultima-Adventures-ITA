using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Multis
{
	public class HousingEstateController : Item
	{
		private const int MaximumRecentRevisions = 10;
		private const int MaximumSerializedMembers = 4096;

		private HousingEstateHealth m_Health;
		private string m_DegradedReason;
		private int m_CurrentRevision;
		private List<Serial> m_MemberSerials;
		private List<HouseFoundation> m_Members;
		private HousingEstateSnapshot m_OriginalSnapshot;
		private List<HousingEstateSnapshot> m_RecentRevisions;

		[CommandProperty( AccessLevel.GameMaster )]
		public HousingEstateHealth Health { get { return m_Health; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public string DegradedReason { get { return m_DegradedReason; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int CurrentRevision { get { return m_CurrentRevision; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int MemberCount { get { return m_MemberSerials == null ? 0 : m_MemberSerials.Count; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int RecentRevisionCount { get { return m_RecentRevisions == null ? 0 : m_RecentRevisions.Count; } }

		public HousingEstateSnapshot OriginalSnapshot { get { return m_OriginalSnapshot; } }

		public HousingEstateController( IList<HouseFoundation> members, HousingEstateSnapshot originalSnapshot )
			: base( 1 )
		{
			m_Health = HousingEstateHealth.Healthy;
			m_DegradedReason = String.Empty;
			m_MemberSerials = new List<Serial>();
			m_Members = new List<HouseFoundation>();
			m_RecentRevisions = new List<HousingEstateSnapshot>( MaximumRecentRevisions );
			m_OriginalSnapshot = originalSnapshot;
			m_CurrentRevision = originalSnapshot == null ? 0 : originalSnapshot.Revision;

			if ( members != null )
			{
				for ( int i = 0; i < members.Count; ++i )
				{
					m_MemberSerials.Add( members[i] == null ? Serial.MinusOne : members[i].Serial );
					m_Members.Add( members[i] );
				}
			}

			Name = "housing estate controller";
			Visible = false;
			Movable = false;
			Internalize();
		}

		public HousingEstateController( Serial serial )
			: base( serial )
		{
		}

		public HouseFoundation GetMember( int index )
		{
			if ( index < 0 || index >= MemberCount )
				throw new ArgumentOutOfRangeException( "index" );

			HouseFoundation member = m_Members[index];

			if ( member == null && m_MemberSerials[index] != Serial.MinusOne )
			{
				member = World.FindItem( m_MemberSerials[index] ) as HouseFoundation;
				m_Members[index] = member;
			}

			return member;
		}

		internal Serial GetMemberSerial( int index )
		{
			if ( index < 0 || index >= MemberCount )
				throw new ArgumentOutOfRangeException( "index" );

			return m_MemberSerials[index];
		}

		public HousingEstateSnapshot GetRecentRevision( int index )
		{
			if ( index < 0 || index >= RecentRevisionCount )
				throw new ArgumentOutOfRangeException( "index" );

			return m_RecentRevisions[index];
		}

		internal HousingEstateSnapshot FindRevision( int revision )
		{
			HousingEstateSnapshot found = null;

			if ( m_OriginalSnapshot != null && m_OriginalSnapshot.Revision == revision )
				found = m_OriginalSnapshot;

			for ( int i = 0; i < RecentRevisionCount; ++i )
			{
				HousingEstateSnapshot candidate = m_RecentRevisions[i];

				if ( candidate == null || candidate.Revision != revision )
					continue;

				if ( found != null )
					return null;

				found = candidate;
			}

			return found;
		}

		public void AddRevision( HousingEstateSnapshot snapshot )
		{
			if ( snapshot == null )
			{
				MarkDegraded( "A null estate revision was rejected." );
				return;
			}

			int discardCount = HousingEstateHistory.CalculateDiscardCount( m_RecentRevisions.Count, 1, MaximumRecentRevisions );

			while ( discardCount-- > 0 )
				m_RecentRevisions.RemoveAt( 0 );

			m_RecentRevisions.Add( snapshot );
			++m_CurrentRevision;
		}

		internal bool ContainsMember( HouseFoundation house )
		{
			if ( house == null || m_MemberSerials == null )
				return false;

			for ( int i = 0; i < m_MemberSerials.Count; ++i )
			{
				if ( m_MemberSerials[i] == house.Serial )
					return true;
			}

			return false;
		}

		internal List<HouseFoundation> CopyMembers()
		{
			List<HouseFoundation> members = new List<HouseFoundation>( MemberCount );

			for ( int i = 0; i < MemberCount; ++i )
				members.Add( GetMember( i ) );

			return members;
		}

		internal HousingValidationResult ValidateMembers()
		{
			if ( Deleted )
				return Invalid( "The estate controller is deleted." );

			if ( m_MemberSerials == null || m_Members == null || m_MemberSerials.Count == 0 || m_MemberSerials.Count != m_Members.Count )
				return Invalid( "The estate has no members." );

			if ( m_OriginalSnapshot == null )
				return Invalid( "The estate has no original snapshot." );

			if ( m_OriginalSnapshot.HouseCount != m_MemberSerials.Count )
				return Invalid( "The estate member list does not match its original snapshot." );

			for ( int i = 0; i < m_MemberSerials.Count; ++i )
			{
				HouseFoundation member = GetMember( i );

				if ( member == null || member.Deleted )
					return Invalid( "The estate contains a missing or deleted member." );

				if ( m_MemberSerials[i] != member.Serial || m_OriginalSnapshot.GetHouse( i ).FoundationSerial != member.Serial )
					return Invalid( "The estate member order does not match its original snapshot." );

				for ( int j = 0; j < i; ++j )
				{
					if ( m_MemberSerials[j] == member.Serial )
						return Invalid( "The estate contains a duplicate member." );
				}
			}

			return m_OriginalSnapshot.ValidateLiveReferences();
		}

		internal void MarkDegraded( string reason )
		{
			m_Health = HousingEstateHealth.Degraded;
			m_DegradedReason = String.IsNullOrEmpty( reason ) ? "Estate validation failed." : reason;
		}

		internal void RestoreHealth( HousingEstateHealth health, string reason )
		{
			m_Health = health;
			m_DegradedReason = reason ?? String.Empty;
		}

		public override void OnAfterDelete()
		{
			HousingEstateRegistry.Unregister( this );
			base.OnAfterDelete();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version
			writer.Write( (int) m_Health );
			writer.Write( m_DegradedReason );
			writer.Write( m_CurrentRevision );
			writer.Write( MemberCount );

			for ( int i = 0; i < MemberCount; ++i )
			{
				writer.Write( (int) m_MemberSerials[i] );
				writer.Write( m_Members[i] );
			}

			writer.Write( m_OriginalSnapshot != null );

			if ( m_OriginalSnapshot != null )
				m_OriginalSnapshot.Serialize( writer );

			writer.Write( RecentRevisionCount );

			for ( int i = 0; i < RecentRevisionCount; ++i )
				m_RecentRevisions[i].Serialize( writer );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			m_Health = HousingEstateHealth.Degraded;
			m_DegradedReason = "Estate controller data was not loaded.";
			m_MemberSerials = new List<Serial>();
			m_Members = new List<HouseFoundation>();
			m_RecentRevisions = new List<HousingEstateSnapshot>( MaximumRecentRevisions );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
				case 0:
				{
					m_Health = (HousingEstateHealth) reader.ReadInt();
					m_DegradedReason = reader.ReadString() ?? String.Empty;
					m_CurrentRevision = reader.ReadInt();

					int memberCount = reader.ReadInt();

					if ( memberCount < 0 || memberCount > MaximumSerializedMembers )
						throw new InvalidDataException( "Invalid housing estate member count." );

					for ( int i = 0; i < memberCount; ++i )
					{
						Serial serial = version >= 1 ? (Serial) reader.ReadInt() : Serial.MinusOne;
						HouseFoundation member = reader.ReadItem() as HouseFoundation;
						m_MemberSerials.Add( version >= 1 ? serial : ( member == null ? Serial.MinusOne : member.Serial ) );
						m_Members.Add( member );
					}

					if ( reader.ReadBool() )
						m_OriginalSnapshot = new HousingEstateSnapshot( reader );

					int revisionCount = reader.ReadInt();

					if ( revisionCount < 0 || revisionCount > MaximumRecentRevisions )
						throw new InvalidDataException( "Invalid housing estate revision count." );

					for ( int i = 0; i < revisionCount; ++i )
						m_RecentRevisions.Add( new HousingEstateSnapshot( reader ) );

					break;
				}
				default:
				{
					throw new InvalidDataException( "Unsupported housing estate controller version." );
				}
			}

			Visible = false;
			Movable = false;
			Timer.DelayCall( TimeSpan.Zero, new TimerCallback( RegisterAfterLoad ) );
		}

		private void RegisterAfterLoad()
		{
			if ( Deleted )
				return;

			string error;

			if ( !HousingEstateRegistry.Register( this, out error ) )
				MarkDegraded( error );
		}

		private static HousingValidationResult Invalid( string message )
		{
			return new HousingValidationResult( false, message );
		}
	}
}
