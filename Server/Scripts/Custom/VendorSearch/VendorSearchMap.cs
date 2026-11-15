using System;
using System.Collections.Generic;
using Server;
using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Network;

namespace Server.Custom.VendorSearch
{
                internal sealed class VendorSearchMapLeaseStore
                {
                        private readonly Dictionary<Serial, Serial> m_ByOwner =
                                new Dictionary<Serial, Serial>();

                        public void Set(Serial ownerSerial, Serial mapSerial)
                        {
                                if (ownerSerial.IsValid && mapSerial.IsValid)
                                        m_ByOwner[ownerSerial] = mapSerial;
                        }

                        public bool TryGet(Serial ownerSerial, out Serial mapSerial)
                        {
                                return m_ByOwner.TryGetValue(ownerSerial, out mapSerial);
                        }

                        public void Remove(Serial ownerSerial, Serial mapSerial)
                        {
                                Serial current;

                                if (m_ByOwner.TryGetValue(ownerSerial, out current) && current == mapSerial)
                                        m_ByOwner.Remove(ownerSerial);
                        }
                }

                internal static class VendorSearchMapLeasePolicy
                {
                        public static bool CanReuse(Serial requestedOwner, Serial storedOwner, Serial rootOwner)
                        {
                                return requestedOwner.IsValid
                                        && requestedOwner == storedOwner
                                        && (!rootOwner.IsValid || rootOwner == requestedOwner);
                        }

                        public static bool CanLift(Serial storedOwner, Serial lifter, bool isStaff)
                        {
                                return !storedOwner.IsValid
                                        || isStaff
                                        || storedOwner == lifter;
                        }

                        public static bool CanDiscard( Serial storedOwner, Serial actor, bool isTrashContainer )
                        {
                                return isTrashContainer
                                        && storedOwner.IsValid
                                        && storedOwner == actor;
                        }

                        public static bool CanRequestDelete( Serial storedOwner, Serial actor, bool isInBackpack )
                        {
                                return isInBackpack
                                        && storedOwner.IsValid
                                        && storedOwner == actor;
                        }
                }

		public class VendorSearchMap : MapItem
		{
                private static readonly VendorSearchMapLeaseStore m_Leases =
                        new VendorSearchMapLeaseStore();

                private Point3D m_Target;
                private Map m_TargetMap;
                private string m_VendorName;
                private string m_ShopName;
                private string m_FacetName;
                private bool m_CanDisplayMap;
                private Serial m_OwnerSerial;

                [CommandProperty( AccessLevel.GameMaster )]
                public Serial OwnerSerial
                {
                        get
                        {
                                return m_OwnerSerial;
                        }
                }

                public override bool Nontransferable
                {
                        get
                        {
                                return m_OwnerSerial.IsValid;
                        }
                }

                public override void HandleInvalidTransfer( Mobile from )
                {
                        if ( from != null )
                                from.SendMessage( "This vendor map is assigned to you and cannot be transferred." );
                }

                public override bool OnDragLift( Mobile from )
                {
                        if ( from == null
                                || !VendorSearchMapLeasePolicy.CanLift(
                                        m_OwnerSerial,
                                        from.Serial,
                                        from.AccessLevel >= AccessLevel.GameMaster ) )
                        {
                                if ( from != null )
                                        from.SendMessage( "That vendor map belongs to another player." );

                                return false;
                        }

                        return base.OnDragLift( from );
                }

                private static bool IsTrashContainer( Item target )
                {
                        return target is TrashBarrel || target is TrashChest;
                }

                public override bool OnDroppedInto( Mobile from, Container target, Point3D p )
                {
                        bool canDiscard = from != null
                                && VendorSearchMapLeasePolicy.CanDiscard(
                                        m_OwnerSerial,
                                        from.Serial,
                                        IsTrashContainer( target ) );

                        if ( !canDiscard )
                                return base.OnDroppedInto( from, target, p );

                        if ( !from.OnDroppedItemInto( this, target, p ) )
                                return false;

                        return target.OnDragDropInto( from, this, p );
                }

                public override bool OnDroppedOnto( Mobile from, Item target )
                {
                        bool canDiscard = from != null
                                && VendorSearchMapLeasePolicy.CanDiscard(
                                        m_OwnerSerial,
                                        from.Serial,
                                        IsTrashContainer( target ) );

                        if ( !canDiscard )
                                return base.OnDroppedOnto( from, target );

                        if ( Deleted || from.Deleted || target.Deleted
                                || from.Map != target.Map || from.Map == null || target.Map == null )
                                return false;

                        if ( from.AccessLevel < AccessLevel.GameMaster
                                && !from.InRange( target.GetWorldLocation(), 2 ) )
                                return false;

                        if ( !from.CanSee( target ) || !from.InLOS( target ) )
                                return false;

                        if ( !target.IsAccessibleTo( from ) )
                                return false;

                        if ( !from.OnDroppedItemOnto( this, target ) )
                                return false;

                        return target.OnDragDrop( from, this );
                }

		[CommandProperty( AccessLevel.GameMaster )]
                public Point3D Target
                {
                        get
                        {
                                return m_Target;
                        }
                        set
                        {
                                m_Target = value;
                        }
                }

		[CommandProperty( AccessLevel.GameMaster )]
                public Map TargetMap
                {
                        get
                        {
                                return m_TargetMap;
                        }
                        set
                        {
                                m_TargetMap = value;
                        }
                }

		[CommandProperty( AccessLevel.GameMaster )]
                public string VendorName
                {
                        get
                        {
                                return m_VendorName;
                        }
                        set
                        {
                                m_VendorName = value;
                        }
                }

		[CommandProperty( AccessLevel.GameMaster )]
                public string ShopName
                {
                        get
                        {
                                return m_ShopName;
                        }
                        set
                        {
                                m_ShopName = value;
                        }
                }

		[CommandProperty( AccessLevel.GameMaster )]
                public string FacetName
                {
                        get
                        {
                                return m_FacetName;
                        }
                        set
                        {
                                m_FacetName = value;
                        }
                }

                public bool CanDisplayMap
                {
                        get
                        {
                                return m_CanDisplayMap;
                        }
                }

		[Constructable]
                public VendorSearchMap()
                {
                        Name = "vendor map";
                        Protected = true;
                        LootType = LootType.Regular;
                        m_CanDisplayMap = true;
                }

		public VendorSearchMap( Serial serial ) : base( serial )
		{
		}

                public static VendorSearchMap Acquire(Mobile owner)
                {
                        if (owner == null || owner.Deleted)
                                return null;

                        Serial mapSerial;
                        VendorSearchMap map = null;

                        if (m_Leases.TryGet(owner.Serial, out mapSerial))
                                map = World.FindItem(mapSerial) as VendorSearchMap;

                        if (map != null && !map.Deleted)
                        {
                                Mobile rootOwner = map.RootParent as Mobile;
                                Serial rootOwnerSerial = rootOwner == null ? Serial.Zero : rootOwner.Serial;
                                bool containedOutsideOwner =
                                        map.Parent != null && rootOwnerSerial == Serial.Zero;

                                if (containedOutsideOwner
                                        || !VendorSearchMapLeasePolicy.CanReuse(
                                                owner.Serial,
                                                map.m_OwnerSerial,
                                                rootOwnerSerial))
                                {
                                        m_Leases.Remove(owner.Serial, map.Serial);
                                        map.m_OwnerSerial = Serial.Zero;
                                        map = null;
                                }
                        }

                        if (map == null || map.Deleted)
                        {
                                m_Leases.Remove(owner.Serial, mapSerial);
                                map = new VendorSearchMap();
                                map.m_OwnerSerial = owner.Serial;
                                m_Leases.Set(owner.Serial, map.Serial);
                        }

                        return map;
                }

                public void Configure(Point3D target, Map targetMap, string vendorName, string shopName, string facetName)
                {
                        m_Target = target;
                        m_TargetMap = targetMap;
                        m_VendorName = vendorName;
                        m_ShopName = shopName;

                        Map map = targetMap ?? Map.Trammel;
                        string normalizedWorld = NormalizeWorldName(map, target, facetName);
                        string displayFacet = facetName;

                        if (string.IsNullOrEmpty(displayFacet))
                        {
                                displayFacet = normalizedWorld;
                        }
                        else
                        {
                                string lowerDisplay = displayFacet.ToLowerInvariant();
                                string lowerWorld = normalizedWorld.ToLowerInvariant();

                                if (lowerWorld.Length > 0 && lowerDisplay.IndexOf(lowerWorld) < 0)
                                        displayFacet = string.Format("{0} ({1})", displayFacet, normalizedWorld);
                        }

                        m_FacetName = displayFacet;

                        m_CanDisplayMap = SupportsFacetMap(map) && HasFacetDimensions(map);

                        if (m_CanDisplayMap)
                        {
                                int mapWidth = 0;
                                int mapHeight = 0;

                                if (map != null)
                                {
                                        int index = map.MapID;

                                        if (index >= 0 && index < Map.Maps.Length)
                                        {
                                                Map layout = Map.Maps[index];

                                                if (layout != null)
                                                {
                                                        mapWidth = layout.Width;
                                                        mapHeight = layout.Height;
                                                }
                                        }
                                }

                                const int viewPixelWidth = 300;
                                const int viewPixelHeight = 300;
                                const int viewWorldWidth = 600;
                                const int viewWorldHeight = 600;

                                int mapMinX = 0;
                                int mapMinY = 0;
                                int mapMaxX = mapWidth;
                                int mapMaxY = mapHeight;

                                int worldMinX = int.MinValue;
                                int worldMinY = int.MinValue;
                                ApplyWorldMinimums(normalizedWorld, ref worldMinX, ref worldMinY);

                                if (worldMinX != int.MinValue)
                                        mapMinX = Math.Max(mapMinX, worldMinX);

                                if (worldMinY != int.MinValue)
                                        mapMinY = Math.Max(mapMinY, worldMinY);

                                int worldMaxX = int.MaxValue;
                                int worldMaxY = int.MaxValue;
                                ApplyWorldMaximums(normalizedWorld, ref worldMaxX, ref worldMaxY);

                                if (worldMaxX != int.MaxValue)
                                        mapMaxX = Math.Min(mapMaxX, worldMaxX);

                                if (worldMaxY != int.MaxValue)
                                        mapMaxY = Math.Min(mapMaxY, worldMaxY);

                                int availableWidth = mapMaxX - mapMinX;
                                int availableHeight = mapMaxY - mapMinY;

                                if (availableWidth <= 0 || availableHeight <= 0)
                                {
                                        m_CanDisplayMap = false;
                                }
                                else
                                {
                                        int desiredWidth = viewWorldWidth;
                                        int desiredHeight = viewWorldHeight;

                                        if (desiredWidth > availableWidth)
                                                desiredWidth = availableWidth;

                                        if (desiredHeight > availableHeight)
                                                desiredHeight = availableHeight;

                                        int minX = target.X - (desiredWidth / 2);
                                        int minY = target.Y - (desiredHeight / 2);

                                        if (minX < mapMinX)
                                                minX = mapMinX;

                                        int maxStartX = mapMaxX - desiredWidth;

                                        if (maxStartX < mapMinX)
                                                maxStartX = mapMinX;

                                        if (minX > maxStartX)
                                                minX = maxStartX;

                                        if (minY < mapMinY)
                                                minY = mapMinY;

                                        int maxStartY = mapMaxY - desiredHeight;

                                        if (maxStartY < mapMinY)
                                                maxStartY = mapMinY;

                                        if (minY > maxStartY)
                                                minY = maxStartY;

                                        int maxX = minX + desiredWidth;
                                        int maxY = minY + desiredHeight;

                                        int worldWidth = maxX - minX;
                                        int worldHeight = maxY - minY;

                                        if (worldWidth <= 0 || worldHeight <= 0)
                                        {
                                                m_CanDisplayMap = false;
                                        }
                                        else
                                        {
                                                Width = viewPixelWidth;
                                                Height = viewPixelHeight;
                                                DisplayMap = map;

                                                Bounds = new Rectangle2D(minX, minY, worldWidth, worldHeight);
                                        }
                                }

                        }

                        if (!m_CanDisplayMap)
                        {
                                Width = 200;
                                Height = 200;
                                DisplayMap = null;
                                Bounds = new Rectangle2D(0, 0, Width, Height);
                        }

                        Name = string.Format(m_CanDisplayMap ? "Vendor Map: {0} ({1})" : "Vendor Map: {0} ({1}, coords only)", vendorName, m_FacetName);

                        ClearPins();

                        if (m_CanDisplayMap)
                                AddWorldPin(target.X, target.Y);

                        InvalidateProperties();
                }

                private static bool SupportsFacetMap( Map map )
                {
                        if ( map == null || map == Map.Internal )
                                return false;

                        return map == Map.Trammel
                                || map == Map.Felucca
                                || map == Map.Ilshenar
                                || map == Map.Malas
                                || map == Map.Tokuno
                                || map == Map.TerMur;
                }

                public override void DisplayTo( Mobile from )
                {
                        if ( !m_CanDisplayMap )
                        {
                                if ( from != null )
                                        from.SendMessage( "This map cannot be displayed on your client. Use the listed coordinates to find the vendor." );

                                return;
                        }

                        base.DisplayTo( from );
                }

                private static bool HasFacetDimensions( Map map )
                {
                        if ( map == null )
                                return false;

                        int index = map.MapID;

                        if ( index < 0 || index >= Map.Maps.Length )
                                return false;

                        Map target = Map.Maps[index];

                        return target != null && target.Width > 0 && target.Height > 0;
                }

                private static string NormalizeWorldName( Map map, Point3D target, string facetName )
                {
                        string world = Worlds.GetMyWorld( map, target, target.X, target.Y );

                        if ( !string.IsNullOrEmpty( facetName ) )
                                world = facetName;

                        if ( string.IsNullOrEmpty( world ) )
                        {
                                if ( map == Map.Trammel )
                                        return "the Land of Sosaria";
                                if ( map == Map.Felucca )
                                        return "the Land of Lodoria";
                                if ( map == Map.Ilshenar )
                                        return "DarkMoor";
                                if ( map == Map.Malas )
                                        return "the Serpent Island";
                                if ( map == Map.Tokuno )
                                        return "the Isles of Dread";
                                if ( map == Map.TerMur )
                                        return "the Savaged Empire";

                                return "Unknown";
                        }

                        if ( world == "the Town of Skara Brae" || world == "the Moon of Luna" )
                                return "the Land of Sosaria";

                        if ( world == "the Bottle World of Kuldar" )
                                return "the Serpent Island";

                        string lower = world.ToLowerInvariant();

                        if ( lower.IndexOf( "bank" ) >= 0 || lower.IndexOf( "britannia" ) >= 0 || lower.IndexOf( "britain" ) >= 0 )
                                return "the Land of Sosaria";

                        return world;
                }

                private static void ApplyWorldMinimums( string world, ref int minX, ref int minY )
                {
                        if ( string.IsNullOrEmpty( world ) )
                                return;

                        if ( world == "the Land of Ambrosia" )
                        {
                                if ( minX < 5122 )
                                        minX = 5122;
                                if ( minY < 3036 )
                                        minY = 3036;
                        }
                        else if ( world == "the Island of Umber Veil" )
                        {
                                if ( minX < 699 )
                                        minX = 699;
                                if ( minY < 3129 )
                                        minY = 3129;
                        }
                        else if ( world == "the Bottle World of Kuldar" )
                        {
                                if ( minX < 6127 )
                                        minX = 6127;
                                if ( minY < 828 )
                                        minY = 828;
                        }
                        else if ( world == "the Savaged Empire" )
                        {
                                if ( minX < 136 )
                                        minX = 136;
                                if ( minY < 8 )
                                        minY = 8;
                        }
                }

                private static void ApplyWorldMaximums( string world, ref int maxX, ref int maxY )
                {
                        if ( string.IsNullOrEmpty( world ) )
                                return;

                        if ( world == "the Land of Ambrosia" )
                        {
                                if ( maxX > 6126 )
                                        maxX = 6126;
                                if ( maxY > 4095 )
                                        maxY = 4095;
                        }
                        else if ( world == "the Island of Umber Veil" )
                        {
                                if ( maxX > 2272 )
                                        maxX = 2272;
                                if ( maxY > 4095 )
                                        maxY = 4095;
                        }
                        else if ( world == "the Bottle World of Kuldar" )
                        {
                                if ( maxX > 7167 )
                                        maxX = 7167;
                                if ( maxY > 2742 )
                                        maxY = 2742;
                        }
                        else if ( world == "the Land of Lodoria" )
                        {
                                if ( maxX > 5119 )
                                        maxX = 5119;
                                if ( maxY > 4095 )
                                        maxY = 4095;
                        }
                        else if ( world == "the Land of Sosaria" )
                        {
                                if ( maxX > 5118 )
                                        maxX = 5118;
                                if ( maxY > 3126 )
                                        maxY = 3126;
                        }
                        else if ( world == "the Underworld" )
                        {
                                if ( maxX > 1581 )
                                        maxX = 1581;
                                if ( maxY > 1599 )
                                        maxY = 1599;
                        }
                        else if ( world == "the Serpent Island" )
                        {
                                if ( maxX > 1869 )
                                        maxX = 1869;
                                if ( maxY > 2046 )
                                        maxY = 2046;
                        }
                        else if ( world == "the Isles of Dread" )
                        {
                                if ( maxX > 1446 )
                                        maxX = 1446;
                                if ( maxY > 1446 )
                                        maxY = 1446;
                        }
                        else if ( world == "the Savaged Empire" )
                        {
                                if ( maxX > 1159 )
                                        maxX = 1159;
                                if ( maxY > 1791 )
                                        maxY = 1791;
                        }
                        else if ( world == "the Midlands" )
                        {
                                if ( maxX > 1151 )
                                        maxX = 1151;
                                if ( maxY > 1535 )
                                        maxY = 1535;
                        }
                }

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( !string.IsNullOrEmpty( m_ShopName ) )
				list.Add( "Shop: {0}", m_ShopName );

			if ( !string.IsNullOrEmpty( m_VendorName ) )
				list.Add( "Vendor: {0}", m_VendorName );

                        if ( !string.IsNullOrEmpty( m_FacetName ) )
                                list.Add( "Facet: {0}", m_FacetName );

                        list.Add( "Location: {0},{1}", m_Target.X, m_Target.Y );

                        if ( !m_CanDisplayMap )
                                list.Add( "Map display unavailable for this facet." );
                }

                public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list )
                {
                        base.GetContextMenuEntries( from, list );

                        Container backpack = from == null ? null : from.Backpack;
                        bool isInBackpack = backpack != null && IsChildOf( backpack );

                        if ( !Deleted && from != null
                                && VendorSearchMapLeasePolicy.CanRequestDelete(
                                        m_OwnerSerial,
                                        from.Serial,
                                        isInBackpack ) )
                        {
                                list.Add( new DeleteVendorMapEntry( Serial ) );
                        }
                }

                private sealed class DeleteVendorMapEntry : ContextMenuEntry
                {
                        private readonly Serial m_MapSerial;

                        public DeleteVendorMapEntry( Serial mapSerial ) : base( 1011403, 2 )
                        {
                                m_MapSerial = mapSerial;
                        }

                        public override void OnClick()
                        {
                                Mobile from = Owner == null ? null : Owner.From;
                                VendorSearchMap map = World.FindItem( m_MapSerial ) as VendorSearchMap;
                                Container backpack = from == null ? null : from.Backpack;
                                bool isInBackpack = map != null
                                        && backpack != null
                                        && map.IsChildOf( backpack );

                                if ( from == null || from.Deleted || map == null || map.Deleted
                                        || !VendorSearchMapLeasePolicy.CanRequestDelete(
                                                map.m_OwnerSerial,
                                                from.Serial,
                                                isInBackpack ) )
                                {
                                        if ( from != null )
                                                from.SendMessage( "That vendor map can no longer be deleted." );

                                        return;
                                }

                                from.CloseGump( typeof( VendorSearchMapDeleteGump ) );
                                from.SendGump( new VendorSearchMapDeleteGump( map.Serial ) );
                        }
                }

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );

                        writer.Write( 2 );
                        writer.Write( m_CanDisplayMap );
                        writer.Write( m_Target );
                        writer.Write( m_TargetMap );
                        writer.Write( m_VendorName );
                        writer.Write( m_ShopName );
                        writer.Write( m_FacetName );
                        writer.Write( m_OwnerSerial );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

                        int version = reader.ReadInt();

                        if ( version >= 1 )
                                m_CanDisplayMap = reader.ReadBool();
                        else
                                m_CanDisplayMap = true;

                        m_Target = reader.ReadPoint3D();
                        m_TargetMap = reader.ReadMap();
                        m_VendorName = reader.ReadString();
                        m_ShopName = reader.ReadString();
                        m_FacetName = reader.ReadString();

                        if ( version >= 2 )
                        {
                                m_OwnerSerial = reader.ReadInt();

                                if ( m_OwnerSerial.IsValid )
                                        m_Leases.Set( m_OwnerSerial, Serial );
                        }
                }

                public override void OnDelete()
                {
                        if ( m_OwnerSerial.IsValid )
                                m_Leases.Remove( m_OwnerSerial, Serial );

                        base.OnDelete();
                }
        }

        public sealed class VendorSearchMapDeleteGump : Gump
        {
                private readonly Serial m_MapSerial;

                public VendorSearchMapDeleteGump( Serial mapSerial ) : base( 120, 80 )
                {
                        m_MapSerial = mapSerial;

                        Closable = true;
                        Disposable = true;
                        Dragable = true;
                        Resizable = false;

                        AddPage( 0 );
                        AddBackground( 0, 0, 360, 190, 9270 );
                        AddAlphaRegion( 12, 12, 336, 166 );
                        AddHtml(
                                25,
                                22,
                                310,
                                28,
                                "<CENTER><BASEFONT COLOR=#FFFFFF><BIG>Delete Vendor Map</BIG></BASEFONT></CENTER>",
                                false,
                                false );
                        AddHtml(
                                32,
                                62,
                                296,
                                58,
                                "<BASEFONT COLOR=#FFFFFF>This permanently deletes this vendor map. You can create another map from vendor search later.</BASEFONT>",
                                false,
                                false );

                        AddButton( 62, 142, 247, 248, 1, GumpButtonType.Reply, 0 );
                        AddButton( 220, 142, 241, 242, 0, GumpButtonType.Reply, 0 );
                }

                public override void OnResponse( NetState sender, RelayInfo info )
                {
                        if ( sender == null || info == null || info.ButtonID != 1 )
                                return;

                        Mobile from = sender.Mobile;
                        VendorSearchMap map = World.FindItem( m_MapSerial ) as VendorSearchMap;
                        Container backpack = from == null ? null : from.Backpack;
                        bool isInBackpack = map != null
                                && backpack != null
                                && map.IsChildOf( backpack );

                        if ( from == null || from.Deleted || map == null || map.Deleted
                                || !VendorSearchMapLeasePolicy.CanRequestDelete(
                                        map.OwnerSerial,
                                        from.Serial,
                                        isInBackpack ) )
                        {
                                if ( from != null )
                                        from.SendMessage( "That vendor map can no longer be deleted." );

                                return;
                        }

                        map.Delete();
                        from.SendMessage( "You delete the vendor map." );
                }
        }
}
