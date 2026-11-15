using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Server.Multis
{
	public sealed class HousingHouseSnapshot
	{
		private const int MaximumDesignStateBytes = 32 * 1024 * 1024;
		private const int MaximumAccessEntries = 4096;

		private readonly Serial m_FoundationSerial;
		private readonly string m_TypeName;
		private readonly Map m_Map;
		private readonly Point3D m_Location;
		private readonly Mobile m_Owner;
		private readonly bool m_Public;
		private readonly Mobile[] m_CoOwners;
		private readonly Mobile[] m_Friends;
		private readonly Mobile[] m_Access;
		private readonly Mobile[] m_Bans;
		private readonly int m_MaxLockDowns;
		private readonly int m_MaxSecures;
		private readonly bool m_RestrictDecay;
		private readonly int m_LastRevision;

		private DesignState m_CurrentState;
		private DesignState m_DesignState;
		private DesignState m_BackupState;
		private byte[] m_CurrentStateData;
		private byte[] m_DesignStateData;
		private byte[] m_BackupStateData;
		private string m_StateError;

		public Serial FoundationSerial { get { return m_FoundationSerial; } }
		public string TypeName { get { return m_TypeName; } }
		public Map Map { get { return m_Map; } }
		public Point3D Location { get { return m_Location; } }
		internal bool RestrictDecay { get { return m_RestrictDecay; } }

		internal string GetDesignRevisionSummary()
		{
			HouseFoundation foundation = World.FindItem( m_FoundationSerial ) as HouseFoundation;

			if ( ( m_CurrentState == null || m_DesignState == null || m_BackupState == null )
				&& ( foundation == null || foundation.Deleted || !EnsureDesignStates( foundation ) ) )
				return "current r? | design r? | backup r? (captured design state unavailable)";

			return String.Format( "current r{0} | design r{1} | backup r{2}", m_CurrentState.Revision, m_DesignState.Revision, m_BackupState.Revision );
		}

		internal HousingHouseSnapshot( HouseFoundation foundation )
		{
			if ( foundation == null || foundation.Deleted )
				throw new ArgumentException( "A live house foundation is required.", "foundation" );

			m_FoundationSerial = foundation.Serial;
			m_TypeName = foundation.GetType().FullName;
			m_Map = foundation.Map;
			m_Location = foundation.Location;
			m_Owner = foundation.Owner;
			m_Public = foundation.Public;
			m_CoOwners = CopyMobiles( foundation.CoOwners );
			m_Friends = CopyMobiles( foundation.Friends );
			m_Access = CopyMobiles( foundation.Access );
			m_Bans = CopyMobiles( foundation.Bans );
			m_MaxLockDowns = foundation.MaxLockDowns;
			m_MaxSecures = foundation.MaxSecures;
			m_RestrictDecay = foundation.RestrictDecay;
			m_LastRevision = foundation.LastRevision;
			m_CurrentState = new DesignState( foundation.CurrentState );
			m_DesignState = new DesignState( foundation.DesignState );
			m_BackupState = new DesignState( foundation.BackupState );
		}

		internal HousingHouseSnapshot( GenericReader reader )
		{
			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_FoundationSerial = (Serial) reader.ReadInt();
					m_TypeName = reader.ReadString() ?? String.Empty;
					m_Map = reader.ReadMap();
					m_Location = reader.ReadPoint3D();
					m_Owner = reader.ReadMobile();
					m_Public = reader.ReadBool();
					m_CoOwners = ReadMobiles( reader );
					m_Friends = ReadMobiles( reader );
					m_Access = ReadMobiles( reader );
					m_Bans = ReadMobiles( reader );
					m_MaxLockDowns = reader.ReadInt();
					m_MaxSecures = reader.ReadInt();
					m_RestrictDecay = reader.ReadBool();
					m_LastRevision = reader.ReadInt();
					m_CurrentStateData = ReadDesignStateData( reader );
					m_DesignStateData = ReadDesignStateData( reader );
					m_BackupStateData = ReadDesignStateData( reader );
					break;
				}
				default:
				{
					throw new InvalidDataException( "Unsupported housing house snapshot version." );
				}
			}

			HouseFoundation foundation = World.FindItem( m_FoundationSerial ) as HouseFoundation;

			if ( foundation != null && !foundation.Deleted )
				EnsureDesignStates( foundation );
		}

		internal void Serialize( GenericWriter writer )
		{
			writer.Write( (int) 0 ); // version
			writer.Write( (int) m_FoundationSerial );
			writer.Write( m_TypeName );
			writer.Write( m_Map );
			writer.Write( m_Location );
			writer.Write( m_Owner );
			writer.Write( m_Public );
			WriteMobiles( writer, m_CoOwners );
			WriteMobiles( writer, m_Friends );
			WriteMobiles( writer, m_Access );
			WriteMobiles( writer, m_Bans );
			writer.Write( m_MaxLockDowns );
			writer.Write( m_MaxSecures );
			writer.Write( m_RestrictDecay );
			writer.Write( m_LastRevision );
			WriteDesignState( writer, m_CurrentState, m_CurrentStateData );
			WriteDesignState( writer, m_DesignState, m_DesignStateData );
			WriteDesignState( writer, m_BackupState, m_BackupStateData );
		}

		internal HousingValidationResult ValidateLiveReference( out HouseFoundation foundation )
		{
			foundation = null;

			Item item = World.FindItem( m_FoundationSerial );

			if ( item == null )
				return Invalid( "Estate member " + m_FoundationSerial + " is missing." );

			if ( item.Deleted )
				return Invalid( "Estate member " + m_FoundationSerial + " is deleted." );

			foundation = item as HouseFoundation;

			if ( foundation == null )
				return Invalid( "Estate member " + m_FoundationSerial + " is not a house foundation." );

			if ( foundation.GetType().FullName != m_TypeName )
			{
				foundation = null;
				return Invalid( "Estate member " + m_FoundationSerial + " has the wrong house type." );
			}

			if ( m_Map == null )
			{
				foundation = null;
				return Invalid( "Estate member " + m_FoundationSerial + " has no captured map." );
			}

			if ( !EnsureDesignStates( foundation ) )
			{
				foundation = null;
				return Invalid( "Estate member " + m_FoundationSerial + " has invalid design state: " + m_StateError );
			}

			return HousingValidationResult.Valid;
		}

		internal HousingValidationResult ValidateRestoreReference( out HouseFoundation foundation )
		{
			HousingValidationResult validation = ValidateLiveReference( out foundation );

			if ( !validation.IsValid )
				return validation;

			if ( m_Map == Map.Internal )
				return Invalid( "Estate member " + m_FoundationSerial + " has an invalid restore map." );

			if ( m_Owner == null || m_Owner.Deleted )
				return Invalid( "Estate member " + m_FoundationSerial + " has an invalid captured owner." );

			if ( foundation.Owner != m_Owner )
				return Invalid( "Estate member " + m_FoundationSerial + " has a different owner." );

			if ( !MobilesAreLive( m_CoOwners ) || !MobilesAreLive( m_Friends ) || !MobilesAreLive( m_Access ) || !MobilesAreLive( m_Bans ) )
				return Invalid( "Estate member " + m_FoundationSerial + " has an invalid captured access reference." );

			return HousingValidationResult.Valid;
		}

		internal HousingValidationResult ValidateRestoreDestination( HouseFoundation foundation, IList<BaseHouse> ignoredHouses )
		{
			if ( !MatchesFoundationIdentity( foundation ) )
				return Invalid( "Estate member " + m_FoundationSerial + " has an incompatible restore identity." );

			ArrayList toMove;
			HousePlacementResult placement = HousePlacement.CheckRestore( m_Owner, m_Map, foundation.ItemID, m_Location, ignoredHouses, out toMove );

			if ( placement != HousePlacementResult.Valid )
				return Invalid( "Estate member " + m_FoundationSerial + " has an invalid restore destination: " + placement + "." );

			if ( toMove != null && toMove.Count > 0 )
				return Invalid( "Estate member " + m_FoundationSerial + " has an occupied restore destination." );

			return HousingValidationResult.Valid;
		}

		internal bool MatchesFoundationIdentity( HouseFoundation foundation )
		{
			return foundation != null && !foundation.Deleted && foundation.Serial == m_FoundationSerial && foundation.GetType().FullName == m_TypeName;
		}

		internal Rectangle2D GetRestorePlot()
		{
			MultiComponentList components = m_CurrentState.Components;
			return new Rectangle2D( m_Location.X + components.Min.X, m_Location.Y + components.Min.Y, components.Width, components.Height );
		}

		internal bool MatchesLiveState( HouseFoundation foundation )
		{
			if ( !MatchesFoundationIdentity( foundation ) || !EnsureDesignStates( foundation ) )
				return false;

			return foundation.Map == m_Map
				&& foundation.Location == m_Location
				&& foundation.Owner == m_Owner
				&& foundation.Public == m_Public
				&& MobileListsEqual( foundation.CoOwners, m_CoOwners )
				&& MobileListsEqual( foundation.Friends, m_Friends )
				&& MobileListsEqual( foundation.Access, m_Access )
				&& MobileListsEqual( foundation.Bans, m_Bans )
				&& foundation.MaxLockDowns == m_MaxLockDowns
				&& foundation.MaxSecures == m_MaxSecures
				&& foundation.RestrictDecay == m_RestrictDecay
				&& foundation.LastRevision == m_LastRevision
				&& DesignStatesEqual( foundation.CurrentState, m_CurrentState )
				&& DesignStatesEqual( foundation.DesignState, m_DesignState )
				&& DesignStatesEqual( foundation.BackupState, m_BackupState );
		}

		internal void Restore( HouseFoundation foundation, Mobile actor, bool refreshDesignFixtures )
		{
			if ( foundation.Map != m_Map )
				foundation.MoveToWorld( m_Location, m_Map );
			else
				foundation.Location = m_Location;

			if ( foundation.Owner != m_Owner )
				foundation.Owner = m_Owner;

			foundation.Public = m_Public;
			foundation.CoOwners = CreateMobileList( m_CoOwners );
			foundation.Friends = CreateMobileList( m_Friends );
			foundation.Access = CreateMobileList( m_Access );
			foundation.Bans = CreateMobileList( m_Bans );
			foundation.MaxLockDowns = m_MaxLockDowns;
			foundation.MaxSecures = m_MaxSecures;
			foundation.RestrictDecay = m_RestrictDecay;
			foundation.LastRevision = m_LastRevision;

			if ( refreshDesignFixtures )
				foundation.ClearFixtures( m_Owner );

			foundation.CurrentState = new DesignState( m_CurrentState );
			foundation.DesignState = new DesignState( m_DesignState );
			foundation.BackupState = new DesignState( m_BackupState );

			if ( refreshDesignFixtures )
			{
				foundation.AddFixtures( m_Owner, foundation.CurrentState.Fixtures );
				foundation.CheckSignpost();
			}

			foundation.UpdateRegion();
			foundation.Delta( ItemDelta.Update );
			foundation.ProcessDelta();
		}

		private static bool MobilesAreLive( Mobile[] mobiles )
		{
			for ( int i = 0; i < mobiles.Length; ++i )
			{
				if ( mobiles[i] == null || mobiles[i].Deleted )
					return false;
			}

			return true;
		}

		private static bool MobileListsEqual( ArrayList actual, Mobile[] expected )
		{
			if ( actual == null || actual.Count != expected.Length )
				return false;

			for ( int i = 0; i < expected.Length; ++i )
			{
				if ( actual[i] != expected[i] )
					return false;
			}

			return true;
		}

		private static bool DesignStatesEqual( DesignState actual, DesignState expected )
		{
			if ( actual == null || expected == null )
				return actual == expected;

			byte[] actualData = SerializeDesignState( actual );
			byte[] expectedData = SerializeDesignState( expected );

			if ( actualData.Length != expectedData.Length )
				return false;

			for ( int i = 0; i < actualData.Length; ++i )
			{
				if ( actualData[i] != expectedData[i] )
					return false;
			}

			return true;
		}

		private static byte[] SerializeDesignState( DesignState state )
		{
			MemoryStream stream = new MemoryStream();
			BinaryFileWriter writer = new BinaryFileWriter( stream, true );
			state.Serialize( writer );
			writer.Flush();
			byte[] data = stream.ToArray();
			stream.Close();
			return data;
		}

		private bool EnsureDesignStates( HouseFoundation foundation )
		{
			if ( m_CurrentState != null && m_DesignState != null && m_BackupState != null )
				return true;

			try
			{
				m_CurrentState = DeserializeDesignState( foundation, m_CurrentState, ref m_CurrentStateData );
				m_DesignState = DeserializeDesignState( foundation, m_DesignState, ref m_DesignStateData );
				m_BackupState = DeserializeDesignState( foundation, m_BackupState, ref m_BackupStateData );

				if ( m_CurrentState == null || m_DesignState == null || m_BackupState == null )
				{
					m_StateError = "one or more design states are missing";
					return false;
				}

				m_StateError = String.Empty;
				return true;
			}
			catch ( Exception exception )
			{
				m_StateError = exception.GetType().Name + ": " + exception.Message;
				return false;
			}
		}

		private static DesignState DeserializeDesignState( HouseFoundation foundation, DesignState state, ref byte[] data )
		{
			if ( state != null )
				return state;

			if ( data == null || data.Length == 0 )
				return null;

			MemoryStream stream = new MemoryStream( data, false );
			BinaryReader binaryReader = new BinaryReader( stream );
			DesignState restored = new DesignState( foundation, new BinaryFileReader( binaryReader ) );
			binaryReader.Close();
			data = null;
			return restored;
		}

		private static void WriteDesignState( GenericWriter writer, DesignState state, byte[] existingData )
		{
			byte[] data = existingData;

			if ( state != null )
			{
				MemoryStream stream = new MemoryStream();
				BinaryFileWriter stateWriter = new BinaryFileWriter( stream, true );
				state.Serialize( stateWriter );
				stateWriter.Flush();
				data = stream.ToArray();
				stream.Close();
			}

			int length = data == null ? 0 : data.Length;
			writer.Write( length );

			for ( int i = 0; i < length; ++i )
				writer.Write( data[i] );
		}

		private static byte[] ReadDesignStateData( GenericReader reader )
		{
			int length = reader.ReadInt();

			if ( length < 0 || length > MaximumDesignStateBytes )
				throw new InvalidDataException( "Invalid housing design-state length." );

			byte[] data = new byte[length];

			for ( int i = 0; i < length; ++i )
				data[i] = reader.ReadByte();

			return data;
		}

		private static Mobile[] CopyMobiles( ArrayList source )
		{
			if ( source == null || source.Count == 0 )
				return new Mobile[0];

			List<Mobile> mobiles = new List<Mobile>( source.Count );

			for ( int i = 0; i < source.Count; ++i )
			{
				Mobile mobile = source[i] as Mobile;

				if ( mobile != null && !mobile.Deleted )
					mobiles.Add( mobile );
			}

			return mobiles.ToArray();
		}

		private static ArrayList CreateMobileList( Mobile[] mobiles )
		{
			ArrayList list = new ArrayList( mobiles.Length );

			for ( int i = 0; i < mobiles.Length; ++i )
			{
				Mobile mobile = mobiles[i];

				if ( mobile != null && !mobile.Deleted )
					list.Add( mobile );
			}

			return list;
		}

		private static void WriteMobiles( GenericWriter writer, Mobile[] mobiles )
		{
			writer.Write( mobiles.Length );

			for ( int i = 0; i < mobiles.Length; ++i )
				writer.Write( mobiles[i] );
		}

		private static Mobile[] ReadMobiles( GenericReader reader )
		{
			int count = reader.ReadInt();

			if ( count < 0 || count > MaximumAccessEntries )
				throw new InvalidDataException( "Invalid housing access-list length." );

			Mobile[] mobiles = new Mobile[count];

			for ( int i = 0; i < count; ++i )
				mobiles[i] = reader.ReadMobile();

			return mobiles;
		}

		private static HousingValidationResult Invalid( string message )
		{
			return new HousingValidationResult( false, message );
		}
	}

	public sealed class HousingEstateSnapshot
	{
		private const int MaximumSnapshotHouses = 4096;

		private readonly HousingEstateOperation m_Operation;
		private readonly Serial m_ActorSerial;
		private readonly string m_ActorName;
		private readonly int m_Revision;
		private readonly DateTime m_CapturedAtUtc;
		private readonly HousingHouseSnapshot[] m_Houses;

		public HousingEstateOperation Operation { get { return m_Operation; } }
		public Serial ActorSerial { get { return m_ActorSerial; } }
		public Mobile Actor { get { return World.FindMobile( m_ActorSerial ); } }
		public string ActorName { get { return m_ActorName; } }
		public int Revision { get { return m_Revision; } }
		public DateTime CapturedAtUtc { get { return m_CapturedAtUtc; } }
		public int HouseCount { get { return m_Houses.Length; } }

		private HousingEstateSnapshot( IList<HouseFoundation> houses, HousingEstateOperation operation, Mobile actor, int revision )
		{
			if ( houses == null )
				throw new ArgumentNullException( "houses" );

			m_Operation = operation;
			m_ActorSerial = actor == null ? Serial.MinusOne : actor.Serial;
			m_ActorName = actor == null ? String.Empty : ( actor.Name ?? String.Empty );
			m_Revision = revision;
			m_CapturedAtUtc = DateTime.UtcNow;
			m_Houses = new HousingHouseSnapshot[houses.Count];

			for ( int i = 0; i < houses.Count; ++i )
				m_Houses[i] = new HousingHouseSnapshot( houses[i] );
		}

		public HousingEstateSnapshot( GenericReader reader )
		{
			if ( reader == null )
				throw new ArgumentNullException( "reader" );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_Operation = (HousingEstateOperation) reader.ReadInt();
					m_ActorSerial = (Serial) reader.ReadInt();
					m_ActorName = reader.ReadString() ?? String.Empty;
					m_Revision = reader.ReadInt();
					m_CapturedAtUtc = reader.ReadDateTime();

					int count = reader.ReadInt();

					if ( count < 0 || count > MaximumSnapshotHouses )
						throw new InvalidDataException( "Invalid housing estate snapshot member count." );

					m_Houses = new HousingHouseSnapshot[count];

					for ( int i = 0; i < count; ++i )
						m_Houses[i] = new HousingHouseSnapshot( reader );

					break;
				}
				default:
				{
					throw new InvalidDataException( "Unsupported housing estate snapshot version." );
				}
			}
		}

		public static HousingEstateSnapshot Capture( IList<HouseFoundation> houses, HousingEstateOperation operation, Mobile actor, int revision )
		{
			return new HousingEstateSnapshot( houses, operation, actor, revision );
		}

		public HousingHouseSnapshot GetHouse( int index )
		{
			if ( index < 0 || index >= m_Houses.Length )
				throw new ArgumentOutOfRangeException( "index" );

			return m_Houses[index];
		}

		public HousingValidationResult ValidateLiveReferences()
		{
			for ( int i = 0; i < m_Houses.Length; ++i )
			{
				HouseFoundation foundation;
				HousingValidationResult result = m_Houses[i].ValidateLiveReference( out foundation );

				if ( !result.IsValid )
					return result;
			}

			return HousingValidationResult.Valid;
		}

		internal HousingValidationResult ValidateRestoreReferences()
		{
			for ( int i = 0; i < m_Houses.Length; ++i )
			{
				HouseFoundation foundation;
				HousingValidationResult result = m_Houses[i].ValidateRestoreReference( out foundation );

				if ( !result.IsValid )
					return result;
			}

			return HousingValidationResult.Valid;
		}

		internal HousingValidationResult ValidateRestoreDestinations( IList<HouseFoundation> members )
		{
			if ( members == null || members.Count != m_Houses.Length )
				return new HousingValidationResult( false, "The restore destination membership is incompatible." );

			List<BaseHouse> ignoredHouses = new List<BaseHouse>( members.Count );

			for ( int i = 0; i < members.Count; ++i )
				ignoredHouses.Add( members[i] );

			for ( int i = 0; i < m_Houses.Length; ++i )
			{
				HousingValidationResult validation = m_Houses[i].ValidateRestoreDestination( members[i], ignoredHouses );

				if ( !validation.IsValid )
					return validation;

				Rectangle2D plot = m_Houses[i].GetRestorePlot();

				for ( int j = 0; j < i; ++j )
				{
					Rectangle2D other = m_Houses[j].GetRestorePlot();

					if ( plot.X < other.X + other.Width && other.X < plot.X + plot.Width && plot.Y < other.Y + other.Height && other.Y < plot.Y + plot.Height )
						return new HousingValidationResult( false, "Two estate members have overlapping restore destinations." );
				}
			}

			return HousingValidationResult.Valid;
		}

		internal bool MatchesMembership( IList<HouseFoundation> members )
		{
			if ( members == null || members.Count != m_Houses.Length )
				return false;

			for ( int i = 0; i < m_Houses.Length; ++i )
			{
				if ( !m_Houses[i].MatchesFoundationIdentity( members[i] ) )
					return false;
			}

			return true;
		}

		internal HousingValidationResult RestoreDecayLiveState()
		{
			HouseFoundation[] foundations = new HouseFoundation[m_Houses.Length];

			for ( int i = 0; i < m_Houses.Length; ++i )
			{
				HousingValidationResult result = m_Houses[i].ValidateLiveReference( out foundations[i] );

				if ( !result.IsValid )
					return result;
			}

			for ( int i = 0; i < m_Houses.Length; ++i )
				foundations[i].RestrictDecay = m_Houses[i].RestrictDecay;

			return HousingValidationResult.Valid;
		}

		internal bool TryRestoreDecay( HouseFoundation foundation, out string error )
		{
			error = String.Empty;

			if ( foundation == null || foundation.Deleted )
			{
				error = "The estate member is missing or deleted.";
				return false;
			}

			for ( int i = 0; i < m_Houses.Length; ++i )
			{
				if ( m_Houses[i].FoundationSerial != foundation.Serial )
					continue;

				if ( !m_Houses[i].MatchesFoundationIdentity( foundation ) )
				{
					error = "The surviving estate member has the wrong house type.";
					return false;
				}

				foundation.RestrictDecay = m_Houses[i].RestrictDecay;
				return true;
			}

			error = "The surviving estate member has no original snapshot entry.";
			return false;
		}

		internal bool VerifyLiveState()
		{
			for ( int i = 0; i < m_Houses.Length; ++i )
			{
				HouseFoundation foundation;

				if ( !m_Houses[i].ValidateLiveReference( out foundation ).IsValid || !m_Houses[i].MatchesLiveState( foundation ) )
					return false;
			}

			return true;
		}

		public HousingValidationResult RestoreLiveState()
		{
			return RestoreLiveState( null, false );
		}

		internal HousingValidationResult RestoreLiveState( Mobile actor )
		{
			return RestoreLiveState( actor, true );
		}

		private HousingValidationResult RestoreLiveState( Mobile actor, bool refreshDesignFixtures )
		{
			HouseFoundation[] foundations = new HouseFoundation[m_Houses.Length];

			for ( int i = 0; i < m_Houses.Length; ++i )
			{
				HousingValidationResult result = m_Houses[i].ValidateLiveReference( out foundations[i] );

				if ( !result.IsValid )
					return result;
			}

			for ( int i = 0; i < m_Houses.Length; ++i )
				m_Houses[i].Restore( foundations[i], actor, refreshDesignFixtures );

			return HousingValidationResult.Valid;
		}

		public void Serialize( GenericWriter writer )
		{
			if ( writer == null )
				throw new ArgumentNullException( "writer" );

			writer.Write( (int) 0 ); // version
			writer.Write( (int) m_Operation );
			writer.Write( (int) m_ActorSerial );
			writer.Write( m_ActorName );
			writer.Write( m_Revision );
			writer.Write( m_CapturedAtUtc );
			writer.Write( m_Houses.Length );

			for ( int i = 0; i < m_Houses.Length; ++i )
				m_Houses[i].Serialize( writer );
		}
	}
}
