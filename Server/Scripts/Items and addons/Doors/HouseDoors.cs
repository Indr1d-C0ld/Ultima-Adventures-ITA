using System;
using System.Collections;
using Server;
using Server.Multis;
using Server.Gumps;
using System.Collections.Generic;
using Server.ContextMenus;

namespace Server.Items
{
	public class MetalHouseDoor : BaseHouseDoor
	{
		[Constructable]
		public MetalHouseDoor( DoorFacing facing ) : base( facing, 0x675 + (2 * (int)facing), 0x676 + (2 * (int)facing), 0xEC, 0xF3, BaseDoor.GetOffset( facing ) )
		{
		}

		public MetalHouseDoor( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer ) // Default Serialize method
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader ) // Default Deserialize method
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class DarkWoodHouseDoor : BaseHouseDoor
	{
		[Constructable]
		public DarkWoodHouseDoor( DoorFacing facing ) : base( facing, 0x6A5 + (2 * (int)facing), 0x6A6 + (2 * (int)facing), 0xEA, 0xF1, BaseDoor.GetOffset( facing ) )
		{
		}

		public DarkWoodHouseDoor( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer ) // Default Serialize method
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader ) // Default Deserialize method
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class GenericHouseDoor : BaseHouseDoor
	{
		[Constructable]
		public GenericHouseDoor( DoorFacing facing, int baseItemID, int openedSound, int closedSound ) : this( facing, baseItemID, openedSound, closedSound, true )
		{
		}

		[Constructable]
		public GenericHouseDoor( DoorFacing facing, int baseItemID, int openedSound, int closedSound, bool autoAdjust )
			: base( facing, baseItemID + (autoAdjust ? (2 * (int)facing) : 0), baseItemID + 1 + (autoAdjust ? (2 * (int)facing) : 0), openedSound, closedSound, BaseDoor.GetOffset( facing ) )
		{
		}

		public GenericHouseDoor( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer ) // Default Serialize method
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader ) // Default Deserialize method
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public abstract class BaseHouseDoor : BaseDoor, ISecurable
	{
		private DoorFacing m_Facing;
		private SecureLevel m_Level;
		private bool m_CloseAfterPassage;
		private Timer m_CloseAfterPassageTimer;

		[CommandProperty( AccessLevel.GameMaster )]
		public DoorFacing Facing
		{
			get{ return m_Facing; }
			set{ m_Facing = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public SecureLevel Level
		{
			get{ return m_Level; }
			set{ m_Level = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool CloseAfterPassage
		{
			get{ return m_CloseAfterPassage; }
			set{ m_CloseAfterPassage = value; }
		}

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list )
		{
			base.GetContextMenuEntries( from, list );
			SetSecureLevelEntry.AddTo( from, this, list );
		}

		public BaseHouseDoor( DoorFacing facing, int closedID, int openedID, int openedSound, int closedSound, Point3D offset ) : base( closedID, openedID, openedSound, closedSound, offset )
		{
			m_Facing = facing;
			m_Level = SecureLevel.Anyone;
		}

		public BaseHouse FindHouse()
		{
			Point3D loc;

			if ( Open )
				loc = new Point3D( X - Offset.X, Y - Offset.Y, Z - Offset.Z );
			else
				loc = this.Location;

			return BaseHouse.FindHouseAt( loc, Map, 20 );
		}

		public bool CheckAccess( Mobile m )
		{
			BaseHouse house = FindHouse();

			if ( house == null )
				return false;

			if ( !house.IsAosRules )
				return true;

			if ( house.Public ? house.IsBanned( m ) : !house.HasAccess( m ) )
				return false;

			return house.HasSecureAccess( m, m_Level );
		}

		public override void OnOpened( Mobile from )
		{
			BaseHouse house = FindHouse();

			if ( house != null && house.IsFriend( from ) && from.AccessLevel == AccessLevel.Player && house.RefreshDecay() )
				from.SendLocalizedMessage( 1043293 ); // Your house's age and contents have been refreshed.

			if ( house != null && house.Public && !house.IsFriend( from ) )
				house.Visits++;

			StartCloseAfterPassage( from );
		}

		public override void OnClosed( Mobile from )
		{
			if ( m_CloseAfterPassageTimer != null )
			{
				m_CloseAfterPassageTimer.Stop();
				m_CloseAfterPassageTimer = null;
			}
		}

		public override bool UseLocks()
		{
			BaseHouse house = FindHouse();

			return ( house == null || !house.IsAosRules );
		}

		public override void Use( Mobile from )
		{
			if ( !CheckAccess( from ) )
				from.SendLocalizedMessage( 1061637 ); // You are not allowed to access this.
			else
				base.Use( from );
		}

		public BaseHouseDoor( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 2 ); // version

			writer.Write( (int) m_Level );

			writer.Write( (int) m_Facing );

			writer.Write( m_CloseAfterPassage );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 2:
					m_Level = (SecureLevel)reader.ReadInt();
					m_Facing = (DoorFacing)reader.ReadInt();
					m_CloseAfterPassage = reader.ReadBool();
					break;
				case 1:
					m_Level = (SecureLevel)reader.ReadInt();
					m_Facing = (DoorFacing)reader.ReadInt();
					break;
				case 0:
					m_Level = SecureLevel.Anyone;
					m_Facing = (DoorFacing)reader.ReadInt();
					break;
			}
		}

		public override bool IsInside( Mobile from )
		{
			return IsInsideAt( from, GetClosedLocation() );
		}

		private Point3D GetClosedLocation()
		{
			if ( Open )
				return new Point3D( X - Offset.X, Y - Offset.Y, Z - Offset.Z );

			return Location;
		}

		private bool IsInsideAt( Mobile from, Point3D baseLocation )
		{
			int x,y,w,h;

			const int r = 2;
			const int bs = r*2+1;
			const int ss = r+1;

			switch ( m_Facing )
			{
				case DoorFacing.WestCW:
				case DoorFacing.EastCCW: x = -r; y = -r; w = bs; h = ss; break;

				case DoorFacing.EastCW: 
				case DoorFacing.WestCCW: x = -r; y = 0; w = bs; h = ss; break;

				case DoorFacing.SouthCW:
				case DoorFacing.NorthCCW: x = -r; y = -r; w = ss; h = bs; break;

				case DoorFacing.NorthCW:
				case DoorFacing.SouthCCW: x = 0; y = -r; w = ss; h = bs; break;

				//No way to test the 'insideness' of SE Sliding doors on OSI, so leaving them default to false until furthur information gained

				default: return false;
			}

			int rx = from.X - baseLocation.X;
			int ry = from.Y - baseLocation.Y;
			int az = Math.Abs( from.Z - baseLocation.Z );

			return ( rx >= x && rx < (x+w) && ry >= y && ry < (y+h) && az <= 4 );
		}

		private void StartCloseAfterPassage( Mobile from )
		{
			if ( !m_CloseAfterPassage || from == null )
				return;

			if ( m_CloseAfterPassageTimer != null )
				m_CloseAfterPassageTimer.Stop();

			Point3D closedLocation = GetClosedLocation();
			bool wasInside = IsInsideAt( from, closedLocation );

			m_CloseAfterPassageTimer = new CloseAfterPassageTimer( this, from, closedLocation, wasInside );
			m_CloseAfterPassageTimer.Start();
		}

		private class CloseAfterPassageTimer : Timer
		{
			private readonly BaseHouseDoor m_Door;
			private readonly Mobile m_Mobile;
			private readonly Point3D m_ClosedLocation;
			private readonly bool m_WasInside;
			private bool m_PassedThrough;
			private DateTime m_PassedThroughAt;

			public CloseAfterPassageTimer( BaseHouseDoor door, Mobile mobile, Point3D closedLocation, bool wasInside )
				: base( TimeSpan.FromMilliseconds( 250.0 ), TimeSpan.FromMilliseconds( 250.0 ) )
			{
				Priority = TimerPriority.TwoFiftyMS;
				m_Door = door;
				m_Mobile = mobile;
				m_ClosedLocation = closedLocation;
				m_WasInside = wasInside;
			}

			protected override void OnTick()
			{
				if ( m_Door.Deleted || !m_Door.Open || m_Mobile == null || m_Mobile.Deleted )
				{
					Stop();
					return;
				}

				if ( m_Mobile.Map != m_Door.Map )
				{
					Stop();
					return;
				}

				bool isInside = m_Door.IsInsideAt( m_Mobile, m_ClosedLocation );
				if ( isInside != m_WasInside )
				{
					m_PassedThrough = true;
					if ( m_PassedThroughAt == DateTime.MinValue )
						m_PassedThroughAt = DateTime.UtcNow;
				}

				if ( m_PassedThrough )
				{
					if ( m_Door.IsFreeToClose() )
					{
						m_Door.Open = false;

						BaseDoor link = m_Door.Link;
						if ( link != null && link.Open )
							link.Open = false;

						Stop();
						return;
					}

					if ( DateTime.UtcNow >= m_PassedThroughAt + TimeSpan.FromSeconds( 0.5 ) )
					{
						m_Door.Open = false;

						BaseDoor link = m_Door.Link;
						if ( link != null && link.Open )
							link.Open = false;

						Stop();
						return;
					}
				}
			}
		}
	}
}
