using System;
using System.Collections.Generic;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Custom.VendorSearch
{
    public static class VendorSearchValidation
    {
        public static bool IsVendorValid( PlayerVendor vendor )
        {
            if ( vendor == null )
                return false;

            return IsVendorPlacementValid(
                vendor.Map,
                vendor.Deleted,
                vendor.Placeholder != null );
        }

        public static bool IsVendorAccessible( Mobile from, PlayerVendor vendor )
        {
            if ( !IsVendorValid( vendor ) )
                return false;

            if ( from != null
                && !vendor.IsOwner( from )
                && vendor.House != null
                && vendor.House.Map == vendor.Map
                && vendor.House.IsInside( vendor )
                && vendor.House.IsBanned( from ) )
                return false;

            return true;
        }

        internal static bool IsVendorPlacementValid( Map map, bool deleted, bool placeholderActive )
        {
            return !deleted
                && map != null
                && map != Map.Internal
                && !placeholderActive;
        }

        public static bool IsListingStillValid( PlayerVendor vendor, Item item )
        {
            if ( vendor == null || item == null || item.Deleted || vendor.Backpack == null )
                return false;

            if ( !item.IsChildOf( vendor.Backpack ) )
                return false;

            VendorItem vendorItem = vendor.GetVendorItem( item );
            return vendorItem != null
                && vendorItem.Valid
                && vendorItem.IsForSale
                && vendorItem.Item == item;
        }

        public static bool IsListingStillValid( Serial itemSerial )
        {
            VendorSearchRecord record;

            if ( !VendorSearchIndex.TryGet( itemSerial, out record ) )
                return false;

            PlayerVendor vendor = World.FindMobile( record.VendorSerial ) as PlayerVendor;

            return IsVendorValid( vendor )
                && IsListingStillValid( vendor, World.FindItem( itemSerial ) );
        }

        public static bool TryResolve( Mobile from, Serial itemSerial, out VendorSearchLiveResult result, out string message )
        {
            result = null;
            message = String.Empty;

            if ( from == null || from.Deleted )
                return false;

            VendorSearchRecord record;

            if ( !VendorSearchIndex.TryGet( itemSerial, out record ) )
            {
                message = "That listing is no longer available.";
                return false;
            }

            PlayerVendor vendor = World.FindMobile( record.VendorSerial ) as PlayerVendor;
            Item item = World.FindItem( itemSerial );

            if ( !IsVendorValid( vendor ) )
            {
                VendorSearchIndex.RemoveStaleListing( itemSerial );
                message = "That listing is no longer available.";
                return false;
            }

            if ( !IsVendorAccessible( from, vendor ) )
            {
                message = "You cannot access that vendor.";
                return false;
            }

            if ( !IsListingStillValid( vendor, item ) )
            {
                VendorSearchIndex.RemoveStaleListing( itemSerial );
                message = "That listing is no longer available.";
                return false;
            }

            VendorItem vendorItem = vendor.GetVendorItem( item );
            int amount = Math.Max( 1, item.Amount );
            bool stackable = item.Stackable && amount > 0;

            result = new VendorSearchLiveResult
            {
                Record = record,
                Vendor = vendor,
                VendorItem = vendorItem,
                Item = item,
                Price = vendorItem.Price,
                Amount = amount,
                PricePerItem = stackable
                    ? Math.Round( (double)vendorItem.Price / amount, 2 )
                    : vendorItem.Price
            };

            return true;
        }

        public static bool TryCreateMap( Mobile from, Serial itemSerial, out string message )
        {
            if ( from == null || from.Deleted )
            {
                message = String.Empty;
                return false;
            }

            VendorSearchLiveResult live;

            if ( !TryResolve( from, itemSerial, out live, out message ) )
                return false;

            PlayerVendor vendor = live.Vendor;
            string vendorName = String.IsNullOrEmpty( vendor.Name ) ? "Vendor" : vendor.Name;
            string facet = Worlds.GetMyWorld( vendor.Map, vendor.Location, vendor.X, vendor.Y );
            VendorSearchMap map = VendorSearchMap.Acquire( from );

            if ( map == null )
            {
                message = "A vendor map could not be created.";
                return false;
            }

            map.Configure( vendor.Location, vendor.Map, vendorName, vendor.ShopName, facet );

            if ( !from.PlaceInBackpack( map ) )
                map.MoveToWorld( from.Location, from.Map );

            message = map.CanDisplayMap
                ? "A vendor map has been created."
                : "The facet cannot be displayed on the client map; use the listed coordinates.";

            return true;
        }
    }

    internal sealed class VendorSearchViewerGate : IVendorSearchRecordGate
    {
        private readonly Mobile m_From;
        private readonly Dictionary<Serial, bool> m_VendorAccess =
            new Dictionary<Serial, bool>();
        private readonly Dictionary<Serial, PlayerVendor> m_Vendors =
            new Dictionary<Serial, PlayerVendor>();
        private readonly List<Serial> m_StaleItems = new List<Serial>();

        public VendorSearchViewerGate( Mobile from )
        {
            m_From = from;
        }

        public bool Include( VendorSearchRecord record )
        {
            bool allowed;

            if ( !m_VendorAccess.TryGetValue( record.VendorSerial, out allowed ) )
            {
                PlayerVendor vendor = World.FindMobile( record.VendorSerial ) as PlayerVendor;
                m_Vendors[record.VendorSerial] = vendor;
                allowed = VendorSearchValidation.IsVendorAccessible( m_From, vendor );
                m_VendorAccess[record.VendorSerial] = allowed;
            }

            if ( !allowed )
                return false;

            Item item = World.FindItem( record.ItemSerial );
            PlayerVendor liveVendor;
            m_Vendors.TryGetValue( record.VendorSerial, out liveVendor );

            if ( !VendorSearchValidation.IsListingStillValid( liveVendor, item ) )
            {
                m_StaleItems.Add( record.ItemSerial );
                return false;
            }

            return true;
        }

        public void RemoveDiscoveredStaleListings()
        {
            for ( int i = 0; i < m_StaleItems.Count; i++ )
                VendorSearchIndex.RemoveStaleListing( m_StaleItems[i] );
        }
    }
}
