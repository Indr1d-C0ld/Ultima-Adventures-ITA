using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.VendorSearch
{
    internal static class VendorSearchLimits
    {
        internal const int ResultsPerPage = 5;
        internal const int InitialVisibleResults = 2000;
        internal const int LoadMoreIncrement = 2000;
        internal const int MaximumSessionResults = 100000;
        internal const int RebuildVendorsPerSlice = 20;
        internal const int CleanupRecordsPerSlice = 250;
        internal const int MaximumSearchTextLength = 64;
        internal const int MaximumFilterTextLength = 40;
        internal static readonly TimeSpan SearchCooldown = TimeSpan.FromSeconds( 1.0 );
        internal static readonly TimeSpan LoadMoreCooldown = TimeSpan.FromSeconds( 3.0 );
        internal static readonly TimeSpan MapActionCooldown = TimeSpan.FromSeconds( 3.0 );
        internal static readonly TimeSpan SessionIdleTimeout = TimeSpan.FromMinutes( 10.0 );
        internal static readonly TimeSpan SessionSweepInterval = TimeSpan.FromMinutes( 5.0 );
        internal static readonly TimeSpan DailyRebuildInterval = TimeSpan.FromDays( 1.0 );
    }

    [Flags]
    public enum VendorSearchCategoryFlags
    {
        None = 0,
        Weapons = 1,
        Armor = 2,
        Magic = 4,
        Clothing = 8,
        Jewelry = 16,
        Resources = 32,
        Consumables = 64,
        Misc = 128,
        All = Weapons | Armor | Magic | Clothing | Jewelry | Resources | Consumables | Misc
    }

    [Flags]
    public enum VendorSearchSubcategoryFlags : ulong
    {
        None = 0,
        Swords = 1UL << 0,
        Axes = 1UL << 1,
        Maces = 1UL << 2,
        Polearms = 1UL << 3,
        Daggers = 1UL << 4,
        Ranged = 1UL << 5,
        OffHand = 1UL << 6,
        Plate = 1UL << 7,
        Chain = 1UL << 8,
        LeatherArmor = 1UL << 9,
        Bone = 1UL << 10,
        Shields = 1UL << 11,
        Spellbooks = 1UL << 12,
        Scrolls = 1UL << 13,
        WandsAndStaves = 1UL << 14,
        MagicReagents = 1UL << 15,
        Runebooks = 1UL << 16,
        Hats = 1UL << 17,
        Robes = 1UL << 18,
        Shirts = 1UL << 19,
        Pants = 1UL << 20,
        Footwear = 1UL << 21,
        Cloaks = 1UL << 22,
        Rings = 1UL << 23,
        Bracelets = 1UL << 24,
        Necklaces = 1UL << 25,
        Earrings = 1UL << 26,
        Trinkets = 1UL << 27,
        Metals = 1UL << 28,
        Wood = 1UL << 29,
        LeatherResources = 1UL << 30,
        Scales = 1UL << 31,
        ResourceReagents = 1UL << 32,
        Potions = 1UL << 33,
        FoodAndDrink = 1UL << 34,
        Bandages = 1UL << 35
    }

    public enum VendorSearchSort
    {
        PriceLowToHigh,
        PriceHighToLow,
        NewestListed,
        OldestListed,
        VendorName,
        ItemName,
        StackAmount
    }

    public enum VendorSearchTextMode
    {
        Contains,
        Exact
    }

    public enum VendorSearchAdvancedPage
    {
        General,
        WeaponEffects
    }

    public sealed class VendorSearchPropertyFilter
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public sealed class VendorSearchRecord
    {
        private IDictionary<string, string> m_PropertyValues;

        public Serial VendorSerial { get; internal set; }
        public Serial ItemSerial { get; internal set; }
        public Type ItemType { get; internal set; }
        public int ItemID { get; internal set; }
        public int Hue { get; internal set; }
        public string DisplayName { get; internal set; }
        public string SearchText { get; internal set; }
        public string Description { get; internal set; }
        public string VendorName { get; internal set; }
        public string ShopName { get; internal set; }
        public Map VendorMap { get; internal set; }
        public Point3D VendorLocation { get; internal set; }
        public string RegionName { get; internal set; }
        public string FacetName { get; internal set; }
        public VendorSearchCategoryFlags CategoryFlags { get; internal set; }
        public VendorSearchSubcategoryFlags SubcategoryFlags { get; internal set; }
        public int Price { get; internal set; }
        public int Amount { get; internal set; }
        public double PricePerItem { get; internal set; }
        public DateTime Created { get; internal set; }
        public bool Stackable { get; internal set; }

        internal void SetPropertyValues( IDictionary<string, string> values )
        {
            if ( values == null || values.Count == 0 )
            {
                m_PropertyValues = null;
                return;
            }

            m_PropertyValues = new Dictionary<string, string>( values, StringComparer.OrdinalIgnoreCase );
        }

        internal bool TryGetPropertyValue( string key, out string value )
        {
            value = null;

            return key != null
                && m_PropertyValues != null
                && m_PropertyValues.TryGetValue( key, out value );
        }
    }

    public sealed class VendorSearchQuery
    {
        public string SearchTerm { get; set; }
        public string VendorName { get; set; }
        public string RegionTerm { get; set; }
        public int? MapID { get; set; }
        public string CategoryKey { get; set; }
        public string SubcategoryKey { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public double? MinPricePerItem { get; set; }
        public double? MaxPricePerItem { get; set; }
        public int? MinStackAmount { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public bool HideZeroPrice { get; set; }
        public bool ShowAdvanced { get; set; }
        public VendorSearchAdvancedPage AdvancedPage { get; set; }
        public VendorSearchTextMode TextMode { get; set; }
        public VendorSearchSort Sort { get; set; }
        public int Page { get; set; }
        public List<VendorSearchPropertyFilter> PropertyFilters { get; set; }

        public VendorSearchQuery Clone()
        {
            VendorSearchQuery clone = (VendorSearchQuery)MemberwiseClone();

            if ( PropertyFilters != null )
            {
                clone.PropertyFilters = new List<VendorSearchPropertyFilter>( PropertyFilters.Count );

                for ( int i = 0; i < PropertyFilters.Count; i++ )
                {
                    VendorSearchPropertyFilter filter = PropertyFilters[i];

                    if ( filter == null )
                    {
                        clone.PropertyFilters.Add( null );
                    }
                    else
                    {
                        clone.PropertyFilters.Add( new VendorSearchPropertyFilter
                        {
                            Key = filter.Key,
                            Value = filter.Value
                        } );
                    }
                }
            }

            return clone;
        }
    }

    public sealed class VendorSearchSummary
    {
        public VendorSearchSummary()
        {
            CategoryCounts = new Dictionary<string, int>( StringComparer.OrdinalIgnoreCase );
        }

        public int TotalMatches { get; internal set; }
        public int BrowsableMatches { get; internal set; }
        public long TotalAvailableAmount { get; internal set; }
        public int CheapestPrice { get; internal set; }
        public int HighestPrice { get; internal set; }
        public double AveragePrice { get; internal set; }
        public double MedianPrice { get; internal set; }
        public Dictionary<string, int> CategoryCounts { get; private set; }
    }

    public sealed class VendorSearchExecutionResult
    {
        public VendorSearchExecutionResult( List<Serial> itemSerials, VendorSearchSummary summary, bool truncated )
        {
            ItemSerials = itemSerials;
            Summary = summary;
            Truncated = truncated;
        }

        public List<Serial> ItemSerials { get; private set; }
        public VendorSearchSummary Summary { get; private set; }
        public bool Truncated { get; private set; }
    }

    public sealed class VendorSearchSession
    {
        private readonly List<Serial> m_ItemSerials;

        public VendorSearchSession( int id, Serial playerSerial, VendorSearchQuery query, List<Serial> itemSerials, VendorSearchSummary summary, bool truncated, DateTime now )
        {
            Id = id;
            PlayerSerial = playerSerial;
            Query = query;
            m_ItemSerials = itemSerials ?? new List<Serial>();
            Summary = summary ?? new VendorSearchSummary();
            Truncated = truncated;
            VisibleResultCount = Math.Min( VendorSearchLimits.InitialVisibleResults, m_ItemSerials.Count );
            LastAccess = now;
        }

        public int Id { get; private set; }
        public Serial PlayerSerial { get; private set; }
        public VendorSearchQuery Query { get; private set; }
        public VendorSearchSummary Summary { get; private set; }
        public bool Truncated { get; private set; }
        public int VisibleResultCount { get; private set; }
        public DateTime LastAccess { get; private set; }
        public int ResultCount { get { return m_ItemSerials.Count; } }
        public bool HasMore { get { return VisibleResultCount < m_ItemSerials.Count; } }

        public Serial GetItemSerial( int index )
        {
            if ( index < 0 || index >= VisibleResultCount || index >= m_ItemSerials.Count )
                return Serial.Zero;

            return m_ItemSerials[index];
        }

        public Serial[] CapturePage( int page, int pageSize )
        {
            if ( page < 0 || pageSize <= 0 )
                return new Serial[0];

            int start = page * pageSize;

            if ( start < 0 || start >= VisibleResultCount || start >= m_ItemSerials.Count )
                return new Serial[0];

            int count = Math.Min(
                pageSize,
                Math.Min( VisibleResultCount, m_ItemSerials.Count ) - start );
            Serial[] serials = new Serial[count];

            for ( int i = 0; i < count; i++ )
                serials[i] = m_ItemSerials[start + i];

            return serials;
        }

        public bool Remove( Serial itemSerial )
        {
            bool removed = m_ItemSerials.Remove( itemSerial );

            if ( removed && VisibleResultCount > m_ItemSerials.Count )
                VisibleResultCount = m_ItemSerials.Count;

            return removed;
        }

        public bool ExposeMore( int amount )
        {
            if ( amount <= 0 || !HasMore )
                return false;

            VisibleResultCount = Math.Min( m_ItemSerials.Count, VisibleResultCount + amount );
            return true;
        }

        public void Touch( DateTime now )
        {
            LastAccess = now;
        }
    }

    public sealed class VendorSearchLiveResult
    {
        public VendorSearchRecord Record { get; internal set; }
        public PlayerVendor Vendor { get; internal set; }
        public VendorItem VendorItem { get; internal set; }
        public Item Item { get; internal set; }
        public int Price { get; internal set; }
        public int Amount { get; internal set; }
        public double PricePerItem { get; internal set; }
    }

    public sealed class VendorSearchIndexStats
    {
        public int RecordCount { get; internal set; }
        public int VendorCount { get; internal set; }
        public int SnapshotVersion { get; internal set; }
        public int StaleRemoved { get; internal set; }
        public bool Ready { get; internal set; }
        public bool Rebuilding { get; internal set; }
        public bool Cleaning { get; internal set; }
        public DateTime LastRebuildStarted { get; internal set; }
        public DateTime LastRebuildCompleted { get; internal set; }
        public string LastRebuildReason { get; internal set; }
    }
}
