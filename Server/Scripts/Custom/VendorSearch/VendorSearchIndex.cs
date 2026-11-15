using System;
using System.Collections.Generic;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Custom.VendorSearch
{
    internal static class VendorSearchMutationPolicy
    {
        public static bool ShouldRefreshAfterDrop( bool droppedDeleted )
        {
            return droppedDeleted;
        }
    }

    internal sealed class VendorSearchIndexStore
    {
        private readonly Dictionary<Serial, VendorSearchRecord> m_ByItem =
            new Dictionary<Serial, VendorSearchRecord>();
        private readonly Dictionary<Serial, HashSet<Serial>> m_ByVendor =
            new Dictionary<Serial, HashSet<Serial>>();
        private VendorSearchRecord[] m_Snapshot;
        private bool m_SnapshotDirty = true;
        private int m_Version;

        public int RecordCount { get { return m_ByItem.Count; } }
        public int VendorCount { get { return m_ByVendor.Count; } }
        public int Version { get { return m_Version; } }

        public void Upsert( VendorSearchRecord record )
        {
            if ( record == null || !record.ItemSerial.IsValid || !record.VendorSerial.IsValid )
                return;

            VendorSearchRecord existing;

            if ( m_ByItem.TryGetValue( record.ItemSerial, out existing ) )
                RemoveVendorItemReference( existing.VendorSerial, record.ItemSerial );

            m_ByItem[record.ItemSerial] = record;

            HashSet<Serial> items;

            if ( !m_ByVendor.TryGetValue( record.VendorSerial, out items ) )
            {
                items = new HashSet<Serial>();
                m_ByVendor[record.VendorSerial] = items;
            }

            items.Add( record.ItemSerial );
            MarkChanged();
        }

        public bool TryGet( Serial itemSerial, out VendorSearchRecord record )
        {
            return m_ByItem.TryGetValue( itemSerial, out record );
        }

        public bool RemoveItem( Serial itemSerial )
        {
            VendorSearchRecord record;

            if ( !m_ByItem.TryGetValue( itemSerial, out record ) )
                return false;

            m_ByItem.Remove( itemSerial );
            RemoveVendorItemReference( record.VendorSerial, itemSerial );
            MarkChanged();
            return true;
        }

        public int RemoveVendor( Serial vendorSerial )
        {
            HashSet<Serial> items;

            if ( !m_ByVendor.TryGetValue( vendorSerial, out items ) )
                return 0;

            Serial[] itemSerials = new Serial[items.Count];
            items.CopyTo( itemSerials );

            for ( int i = 0; i < itemSerials.Length; i++ )
                m_ByItem.Remove( itemSerials[i] );

            m_ByVendor.Remove( vendorSerial );
            MarkChanged();
            return itemSerials.Length;
        }

        public VendorSearchRecord[] GetSnapshot()
        {
            if ( !m_SnapshotDirty && m_Snapshot != null )
                return m_Snapshot;

            m_Snapshot = new VendorSearchRecord[m_ByItem.Count];
            m_ByItem.Values.CopyTo( m_Snapshot, 0 );
            m_SnapshotDirty = false;
            return m_Snapshot;
        }

        private void RemoveVendorItemReference( Serial vendorSerial, Serial itemSerial )
        {
            HashSet<Serial> items;

            if ( !m_ByVendor.TryGetValue( vendorSerial, out items ) )
                return;

            items.Remove( itemSerial );

            if ( items.Count == 0 )
                m_ByVendor.Remove( vendorSerial );
        }

        private void MarkChanged()
        {
            m_Version++;
            m_SnapshotDirty = true;
        }
    }

    internal static class VendorSearchRecordFactory
    {
        public static VendorSearchRecord Create( PlayerVendor vendor, VendorItem vendorItem )
        {
            if ( !VendorSearchValidation.IsVendorValid( vendor )
                || vendorItem == null
                || !vendorItem.Valid
                || !vendorItem.IsForSale )
                return null;

            Item item = vendorItem.Item;

            if ( item == null || item.Deleted || vendor.Backpack == null || !item.IsChildOf( vendor.Backpack ) )
                return null;

            int amount = Math.Max( 1, item.Amount );
            bool stackable = item.Stackable && amount > 0;
            double pricePerItem = stackable
                ? Math.Round( (double)vendorItem.Price / amount, 2 )
                : vendorItem.Price;
            string displayName = VendorSearchCatalog.ResolveItemName( item );
            string vendorName = String.IsNullOrEmpty( vendor.Name ) ? vendor.RawName : vendor.Name;
            string region = Worlds.GetRegionName( vendor.Map, vendor.Location );
            string facet = Worlds.GetMyWorld( vendor.Map, vendor.Location, vendor.X, vendor.Y );
            VendorSearchCategoryFlags categories;
            VendorSearchSubcategoryFlags subcategories;

            VendorSearchCatalog.Classify( item, out categories, out subcategories );

            VendorSearchRecord record = new VendorSearchRecord
            {
                VendorSerial = vendor.Serial,
                ItemSerial = item.Serial,
                ItemType = item.GetType(),
                ItemID = item.ItemID,
                Hue = item.Hue,
                DisplayName = displayName,
                Description = Limit( vendorItem.Description, 160 ),
                VendorName = Limit( vendorName, 40 ),
                ShopName = Limit( vendor.ShopName, 40 ),
                VendorMap = vendor.Map,
                VendorLocation = vendor.Location,
                RegionName = Limit( region, 60 ),
                FacetName = Limit( facet, 60 ),
                CategoryFlags = categories,
                SubcategoryFlags = subcategories,
                Price = vendorItem.Price,
                Amount = amount,
                PricePerItem = pricePerItem,
                Created = vendorItem.Created,
                Stackable = stackable,
                SearchText = VendorSearchCatalog.BuildSearchText(
                    item,
                    displayName,
                    vendorItem.Description,
                    vendorName,
                    vendor.ShopName,
                    region,
                    facet )
            };

            record.SetPropertyValues( VendorSearchPropertyRegistry.BuildValues( item ) );
            return record;
        }

        private static string Limit( string value, int maximumLength )
        {
            if ( String.IsNullOrEmpty( value ) || value.Length <= maximumLength )
                return value ?? String.Empty;

            return value.Substring( 0, maximumLength );
        }
    }

    public static class VendorSearchIndex
    {
        private static VendorSearchIndexStore m_Active = new VendorSearchIndexStore();
        private static readonly HashSet<Serial> m_DirtyVendors = new HashSet<Serial>();
        private static bool m_Ready;
        private static bool m_Staging;
        private static int m_StaleRemoved;

        public static bool Ready { get { return m_Ready; } }

        public static VendorSearchRecord[] GetSnapshot()
        {
            return m_Active.GetSnapshot();
        }

        public static bool TryGet( Serial itemSerial, out VendorSearchRecord record )
        {
            return m_Active.TryGet( itemSerial, out record );
        }

        public static void UpdateListing( PlayerVendor vendor, VendorItem vendorItem )
        {
            VendorSearchRecord record = VendorSearchRecordFactory.Create( vendor, vendorItem );

            if ( record == null )
            {
                if ( vendorItem != null && vendorItem.Item != null )
                    RemoveListing( vendorItem.Item.Serial );

                MarkVendorDirty( vendor == null ? Serial.Zero : vendor.Serial );
                return;
            }

            m_Active.Upsert( record );
            MarkVendorDirty( record.VendorSerial );
        }

        public static void RemoveListing( Serial itemSerial )
        {
            VendorSearchRecord existing;

            if ( m_Active.TryGet( itemSerial, out existing ) )
                MarkVendorDirty( existing.VendorSerial );

            m_Active.RemoveItem( itemSerial );
        }

        public static void RemoveStaleListing( Serial itemSerial )
        {
            VendorSearchRecord existing;

            if ( m_Active.TryGet( itemSerial, out existing ) )
                MarkVendorDirty( existing.VendorSerial );

            if ( m_Active.RemoveItem( itemSerial ) )
                m_StaleRemoved++;
        }

        public static void RemoveVendor( Serial vendorSerial )
        {
            m_Active.RemoveVendor( vendorSerial );
            MarkVendorDirty( vendorSerial );
        }

        public static void RefreshVendor( PlayerVendor vendor )
        {
            if ( vendor == null )
                return;

            RefreshVendorInto( m_Active, vendor );
            MarkVendorDirty( vendor.Serial );
        }

        internal static void RefreshVendorInto( VendorSearchIndexStore store, PlayerVendor vendor )
        {
            if ( store == null || vendor == null )
                return;

            store.RemoveVendor( vendor.Serial );

            if ( vendor.Deleted )
                return;

            foreach ( VendorItem vendorItem in vendor.EnumerateVendorItems() )
            {
                VendorSearchRecord record = VendorSearchRecordFactory.Create( vendor, vendorItem );

                if ( record != null )
                    store.Upsert( record );
            }
        }

        internal static void ReplaceActiveStore( VendorSearchIndexStore store )
        {
            if ( store == null )
                return;

            m_Active = store;
            m_Ready = true;
        }

        internal static void BeginStaging()
        {
            m_DirtyVendors.Clear();
            m_Staging = true;
        }

        internal static Serial[] GetDirtyVendorSerials()
        {
            Serial[] serials = new Serial[m_DirtyVendors.Count];
            m_DirtyVendors.CopyTo( serials );
            return serials;
        }

        internal static void EndStaging()
        {
            m_DirtyVendors.Clear();
            m_Staging = false;
        }

        public static VendorSearchIndexStats GetStats()
        {
            VendorSearchIndexStats stats = new VendorSearchIndexStats();
            stats.RecordCount = m_Active.RecordCount;
            stats.VendorCount = m_Active.VendorCount;
            stats.SnapshotVersion = m_Active.Version;
            stats.StaleRemoved = m_StaleRemoved;
            stats.Ready = m_Ready;
            VendorSearchMaintenance.PopulateStats( stats );
            return stats;
        }

        private static void MarkVendorDirty( Serial vendorSerial )
        {
            if ( m_Staging && vendorSerial.IsValid )
                m_DirtyVendors.Add( vendorSerial );
        }
    }
}
