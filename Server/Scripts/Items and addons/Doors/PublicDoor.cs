using System;
using Server;
using Server.Commands;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Regions;

namespace Server.Items
{
        public class PublicDoor : Item
        {
                public Point3D m_PointDest;
                public Map m_MapDest;

                static PublicDoor()
                {
                        EventSink.OpenDoorMacroUsed += EventSink_OpenDoorMacroUsed;
                }

                [CommandProperty( AccessLevel.GameMaster )]
                public Point3D PointDest
                {
                        get { return m_PointDest; }
                        set { m_PointDest = value; InvalidateProperties(); }
                }

                [CommandProperty( AccessLevel.GameMaster )]
                public Map MapDest
                {
                        get { return m_MapDest; }
                        set { m_MapDest = value; InvalidateProperties(); }
                }

                [Constructable]
                public PublicDoor() : base( 0x676 )
                {
                        Name = "door";
                        Weight = 10;
                        Movable = false;
                        Hue = 0;
                        m_PointDest = new Point3D( 0, 0, 0 );
                        m_MapDest = Map.Trammel;
                }

                public override bool HandlesOnMovement
                {
                        get { return true; }
                }

                public override bool OnMoveOver( Mobile m )
                {
                        if ( m is PlayerMobile )
                        {
                                Use( m );
                                return false;
                        }

                        return base.OnMoveOver( m );
                }

                public override void OnDoubleClick( Mobile m )
                {
                        if ( m.InRange( GetWorldLocation(), 2 ) )
                        {
                                Use( m );
                        }
                        else
                        {
                                m.SendLocalizedMessage( 502138 ); // That is too far away for you to use
                        }
                }

                public override void OnDoubleClickDead( Mobile m )
                {
                        if ( m.InRange( GetWorldLocation(), 2 ) )
                        {
                                Use( m );
                        }
                        else
                        {
                                m.SendLocalizedMessage( 502138 ); // That is too far away for you to use
                        }
                }

                public bool Use( Mobile from )
                {
                        if ( from == null )
                                return false;

                        if ( m_MapDest == null )
                                m_MapDest = Map;

                        bool teleported = false;

                        if ( from is PlayerMobile )
                        {
                                PlayerMobile pm = (PlayerMobile)from;

                                if ( pm.PauseDoor )
                                        return false;

                                teleported = DoPublicDoor( pm );

                                if ( teleported )
                                {
                                        pm.PauseDoor = true;
                                        Timer.DelayCall( TimeSpan.FromSeconds( 1.0 ), new TimerStateCallback( UnPause ), pm );
                                        from.PlaySound( DoorSound( ItemID ) );
                                }
                        }
                        else
                        {
                                teleported = DoPublicDoor( from );
                        }

                        return teleported;
                }

                private static void UnPause( object state )
                {
                        if ( state is PlayerMobile )
                                ((PlayerMobile)state).PauseDoor = false;
                }

                private static void EventSink_OpenDoorMacroUsed( OpenDoorMacroEventArgs args )
                {
                        Mobile m = args.Mobile;

                        if ( m == null || m.Map == null )
                                return;

                        int x = m.X, y = m.Y;

                        switch ( m.Direction & Direction.Mask )
                        {
                                case Direction.North:      --y; break;
                                case Direction.Right: ++x; --y; break;
                                case Direction.East:  ++x;      break;
                                case Direction.Down:  ++x; ++y; break;
                                case Direction.South:      ++y; break;
                                case Direction.Left:  --x; ++y; break;
                                case Direction.West:  --x;      break;
                                case Direction.Up:    --x; --y; break;
                        }

                        Sector sector = m.Map.GetSector( x, y );

                        foreach ( Item item in sector.Items )
                        {
                                if ( item.Location.X == x && item.Location.Y == y && (item.Z + item.ItemData.Height) > m.Z && (m.Z + 16) > item.Z && item is PublicDoor && m.CanSee( item ) && m.InLOS( item ) )
                                {
                                        if ( m.CheckAlive() )
                                        {
                                                m.SendLocalizedMessage( 500024 ); // Opening door...
                                                ((PublicDoor)item).Use( m );
                                        }

                                        break;
                                }
                        }
                }

                private bool DoPublicDoor( Mobile m )
                {
                        if ( !(m is PlayerMobile) )
                                return false;

                        PlayerMobile pc = (PlayerMobile)m;
                        CharacterDatabase db = CharacterDatabase.GetDB( m );

                        string sPublicDoor = pc.CharacterPublicDoor;

                        if ( string.IsNullOrEmpty( sPublicDoor ) && db != null )
                        {
                                sPublicDoor = db.CharacterPublicDoor;
                                pc.CharacterPublicDoor = sPublicDoor;
                        }

                        int mX = 0;
                        int mY = 0;
                        int mZ = 0;
                        Map mWorld = null;
                        string mZone = "";

                        if ( !string.IsNullOrEmpty( sPublicDoor ) )
                        {
                                string[] sPublicDoors = sPublicDoor.Split( '#' );

                                for ( int i = 0; i < sPublicDoors.Length; ++i )
                                {
                                        string exits = sPublicDoors[i];

                                        switch ( i )
                                        {
                                                case 0: int.TryParse( exits, out mX ); break;
                                                case 1: int.TryParse( exits, out mY ); break;
                                                case 2: int.TryParse( exits, out mZ ); break;
                                                case 3:
                                                        try { mWorld = Map.Parse( exits ); }
                                                        catch { }

                                                        if ( mWorld == null )
                                                                mWorld = Map.Trammel;

                                                        break;
                                                case 4:
                                                        mZone = exits;
                                                        break;
                                        }
                                }
                        }

                        string regionName = Worlds.GetRegionName( m.Map, m.Location );

                        if ( regionName == "the Dojo" || regionName == "the Thieves Guild" || regionName == "the Camping Tent" || regionName == "the Dungeon Room" || regionName == "the Black Magic Guild" )
                        {
                                m.Hidden = true;
                        }

                        if ( regionName == "the Basement" && m.Region is PublicRegion && BasementDoor.HatchAtOtherEnd( m ) )
                        {
                                ClearCharacterPublicDoor( pc, db );
                                Point3D loc = new Point3D( mX, mY, mZ );
                                PublicTeleport( m, loc, mWorld, mZone, "exit" );
                                return true;
                        }

                        if ( regionName != "the Basement" && m.Region is PublicRegion && !string.IsNullOrEmpty( sPublicDoor ) )
                        {
                                ClearCharacterPublicDoor( pc, db );
                                Point3D loc = new Point3D( mX, mY, mZ );
                                PublicTeleport( m, loc, mWorld, mZone, "exit" );
                                return true;
                        }

                        if ( m.Region is PublicRegion )
                        {
                                ClearCharacterPublicDoor( pc, db );

                                Point3D loc = new Point3D( 1832, 755, 0 );
                                string failPlace = "the Building";
                                Map failMap = Map.Trammel;

                                if ( regionName == "the Bank" )
                                {
                                        loc = new Point3D( 1830, 768, 0 );
                                        failPlace = "the Bank";
                                }
                                else if ( regionName == "the Tavern" )
                                {
                                        loc = new Point3D( 1831, 758, 12 );
                                        failPlace = "the Inn";
                                }
                                else if ( regionName == "the Dojo" )
                                {
                                        loc = new Point3D( 1831, 758, 12 );
                                        failPlace = "the Inn";
                                }
                                else if ( regionName == "the Port" )
                                {
                                        loc = new Point3D( 1831, 758, 12 );
                                        failPlace = "the PortThat ";
                                }
                                else if ( regionName == "the Black Magic Guild" )
                                {
                                        loc = new Point3D( 2243, 251, 0 );
                                        failPlace = "the Black Magic Guild";
                                }
                                else if ( regionName == "the Wizards Guild" )
                                {
                                        loc = new Point3D( 2832, 1873, 55 );
                                        failPlace = "the Wizards Guild";
                                }
                                else if ( regionName == "the Thieves Guild" )
                                {
                                        loc = new Point3D( 3315, 2059, 40 );
                                        failPlace = "the Thieves Guild";
                                }
                                else if ( regionName == "the Basement" )
                                {
                                        loc = new Point3D( 1831, 758, 12 );
                                        failPlace = "the Basement";
                                }

                                PublicTeleport( m, loc, failMap, failPlace, "exit" );
                                return true;
                        }

                        if ( Name == "the Wizards Guild" && pc.NpcGuild != NpcGuild.MagesGuild )
                        {
                                m.SendMessage( "Only those of the Wizards Guild may enter!" );
                                return false;
                        }

                        string sX = m.X.ToString();
                        string sY = m.Y.ToString();
                        string sZ = m.Z.ToString();
                        string sMap = Worlds.GetMyMapString( m.Map );
                        string sZone = Name;

                        if ( Name == "oak shelf" )
                                sZone = "the Thieves Guild";
                        else if ( Name == "trapdoor" )
                                sZone = "the Thieves Guild";
                        else if ( Name == "camping tent" )
                                sZone = "the Camping Tent";

                        string record = sX + "#" + sY + "#" + sZ + "#" + sMap + "#" + sZone;

                        pc.CharacterPublicDoor = record;

                        if ( db != null )
                                db.CharacterPublicDoor = record;

                        PublicTeleport( m, m_PointDest, m_MapDest, sZone, "enter" );
                        return true;
                }

                private static void ClearCharacterPublicDoor( PlayerMobile pc, CharacterDatabase db )
                {
                        if ( pc != null )
                                pc.CharacterPublicDoor = null;

                        if ( db != null )
                                db.CharacterPublicDoor = null;
                }

                public override void AddNameProperties( ObjectPropertyList list )
                {
                        base.AddNameProperties( list );

                        if ( ItemID >= 0x1ED9 && ItemID <= 0x1EFC )
                                list.Add( 1070722, "Magical Doorway" );
                }

                public static int DoorSound( int door )
                {
                        int sound = 234;

                        if ( door == 0x12DC || door == 0x12E6 || door == 0x608 || door == 0x6727 || door == 0x6728 )
                                sound = 0x057;
                        else if ( door >= 0x675 && door <= 0x694 )
                                sound = 236;
                        else if ( door >= 8172 && door <= 8188 )
                                sound = 236;
                        else if ( door == 8197 )
                                sound = 236;
                        else if ( door >= 0x6C5 && door <= 0x6D4 )
                                sound = 236;
                        else if ( door == 0x695 || door == 0x697 || door == 0x69F || door == 0x69D )
                                sound = 235;
                        else if ( door >= 0x1ED9 && door <= 0x1EFC )
                                sound = 0x1FE;
                        else if ( door == 0x3F38 )
                                sound = 0x1FE;

                        return sound;
                }

                public static void PublicTeleport( Mobile m, Point3D loc, Map map, string zone, string direction )
                {
                        BaseCreature.TeleportPets( m, loc, map, false );
                        m.MoveToWorld( loc, map );
                        LoggingFunctions.LogRegions( m, zone, direction );
                }

                public PublicDoor( Serial serial ) : base( serial )
                {
                }

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );
                        writer.Write( (int) 0 ); // version
                        writer.Write( m_PointDest );
                        writer.Write( m_MapDest );
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );
                        int version = reader.ReadInt();
                        m_PointDest = reader.ReadPoint3D();
                        m_MapDest = reader.ReadMap();
                }
        }
}
