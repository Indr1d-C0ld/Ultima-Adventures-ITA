using System;
using Server.Network;
using Server.Items;
using Server.Misc;

namespace Server.Items
{
	public class MessageInABottle : Item
	{
		public static int GetRandomLevel()
		{
			if ( Core.AOS && 1 > Utility.Random( 25 ) )
				return 4; // ancient

			return Utility.RandomMinMax( 1, 3 );
		}

		public override int LabelNumber{ get{ return 1041080; } } // a message in a bottle

		private Map m_TargetMap;
		private int m_Level;
		private bool m_HasDeterministicLocation;
		private Map m_DeterministicMap;
		private Point3D m_DeterministicLocation;
		public string MapWorld;

		[CommandProperty( AccessLevel.GameMaster )]
		public Map TargetMap
		{
			get{ return m_TargetMap; }
			set{ m_TargetMap = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Level
		{
			get{ return m_Level; }
			set{ m_Level = Math.Max( 1, Math.Min( value, 4 ) ); }
		}

		[CommandProperty(AccessLevel.Owner)]
		public string Map_World { get { return MapWorld; } set { MapWorld = value; InvalidateProperties(); } }

		[Constructable]
		public MessageInABottle( Map map, int level, Point3D location, int x, int y ) : base( 0x12AD )
		{
			if ( level < 1 ){ level = GetRandomLevel(); }

			MapWorld = Worlds.GetMyWorld( map, location, x, y );
			Weight = 1.0;
			m_TargetMap = map;
			m_Level = level;
		}

		public MessageInABottle( Serial serial ) : base( serial )
		{
		}

		public void SetDeterministicLocation( Map map, Point3D location )
		{
			if ( map == null || map == Map.Internal )
				return;

			m_HasDeterministicLocation = true;
			m_DeterministicMap = map;
			m_DeterministicLocation = location;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 4 ); // version
			writer.Write( (int) m_Level );
			writer.Write( m_TargetMap );
            writer.Write( MapWorld );

			writer.Write( m_HasDeterministicLocation );

			if ( m_HasDeterministicLocation )
			{
				writer.Write( m_DeterministicMap );
				writer.Write( m_DeterministicLocation );
			}
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_Level = reader.ReadInt();
			m_TargetMap = reader.ReadMap();
            MapWorld = reader.ReadString();

			if ( version >= 4 )
			{
				m_HasDeterministicLocation = reader.ReadBool();

				if ( m_HasDeterministicLocation )
				{
					m_DeterministicMap = reader.ReadMap();
					m_DeterministicLocation = reader.ReadPoint3D();
				}
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
			{
				SOS sos = new SOS( MapWorld, m_Level );

				if ( m_HasDeterministicLocation && m_DeterministicMap != null && m_DeterministicMap != Map.Internal )
				{
					sos.TargetMap = m_DeterministicMap;
					sos.TargetLocation = m_DeterministicLocation;
				}

				Consume();
				from.AddToBackpack( sos );
				from.LocalOverheadMessage( Network.MessageType.Regular, 0x3B2, 501891 ); // You extract the message from the bottle.
			}
			else
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
		}
	}
}
