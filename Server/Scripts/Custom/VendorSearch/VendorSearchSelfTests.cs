using System;
using System.Collections.Generic;
using System.Text;
using Server.Items;

namespace Server.Custom.VendorSearch
{
    internal sealed class VendorSearchFakeClock : IVendorSearchClock
    {
        public DateTime UtcNow { get; set; }
    }

    internal sealed class VendorSearchTestReport
    {
        private readonly StringBuilder m_Details = new StringBuilder();

        public int Passed { get; private set; }
        public int Failed { get; private set; }
        public string Details { get { return m_Details.ToString(); } }

        public void Pass( string name )
        {
            Passed++;
            m_Details.AppendLine( "PASS: " + name );
        }

        public void Fail( string name, string message )
        {
            Failed++;
            m_Details.AppendLine( "FAIL: " + name + " - " + message );
        }
    }

    internal static class VendorSearchSelfTests
    {
        public static VendorSearchTestReport Run()
        {
            VendorSearchTestReport report = new VendorSearchTestReport();

            RunTest( report, "session window test", delegate { TestSessionWindow( report ); } );
            RunTest( report, "session edge test", delegate { TestSessionEdges( report ); } );
            RunTest( report, "query clone test", delegate { TestQueryClone( report ); } );
            RunTest( report, "record property values test", delegate { TestRecordPropertyValues( report ); } );
            RunTest( report, "gear property filters test", delegate { TestGearPropertyFilters( report ); } );
            RunTest( report, "test runner resilience test", delegate { TestRunTestContinues( report ); } );
            RunTest( report, "category definitions test", delegate { TestCategoryDefinitions( report ); } );
            RunTest( report, "multi-category count test", delegate { TestCategoryCountsCanRepresentMultipleCategories( report ); } );
            RunTest( report, "index store test", delegate { TestIndexStore( report ); } );
            RunTest( report, "dirty vendor removal test", delegate { TestDirtyVendorRemoval( report ); } );
            RunTest( report, "query filters test", delegate { TestQueryFilters( report ); } );
            RunTest( report, "query sorts test", delegate { TestQuerySorts( report ); } );
            RunTest( report, "query summary test", delegate { TestQuerySummaryAndStableSort( report ); } );
            RunTest( report, "query category counts test", delegate { TestCategoryCountsPrecedeSelection( report ); } );
            RunTest( report, "query safety cap test", delegate { TestQuerySafetyCap( report ); } );
            RunTest( report, "session store test", delegate { TestSessionStoreCooldownAndExpiry( report ); } );
            RunTest( report, "map lease store test", delegate { TestMapLeaseStore( report ); } );
            RunTest( report, "map lease ownership test", delegate { TestMapLeaseOwnership( report ); } );
            RunTest( report, "map cooldown test", delegate { TestMapCooldown( report ); } );
            RunTest( report, "page row capture test", delegate { TestPageRowCapture( report ); } );
            RunTest( report, "merged drop refresh policy test", delegate { TestMergedDropRefreshPolicy( report ); } );
            RunTest( report, "vendor placement policy test", delegate { TestVendorPlacementPolicy( report ); } );
            RunTest( report, "startup coordinator test", delegate { TestStartupCoordinator( report ); } );

            return report;
        }

        private static void TestSessionWindow( VendorSearchTestReport report )
        {
            List<Serial> ids = new List<Serial>();

            for ( int i = 1; i <= 4500; i++ )
                ids.Add( (Serial)( 0x40000000 + i ) );

            VendorSearchSession session = new VendorSearchSession(
                1,
                (Serial)1,
                new VendorSearchQuery(),
                ids,
                new VendorSearchSummary(),
                false,
                new DateTime( 2026, 1, 1, 12, 0, 0, DateTimeKind.Utc ) );

            bool first = session.ExposeMore( 2000 );

            Assert( report, "session initial window", first && session.VisibleResultCount == 4000 );

            bool second = session.ExposeMore( 2000 );

            Assert( report, "session final window", second && session.VisibleResultCount == 4500 );
            Assert( report, "session has no further window", !session.ExposeMore( 2000 ) );
        }

        private static void TestSessionEdges( VendorSearchTestReport report )
        {
            List<Serial> ids = new List<Serial>();

            for ( int i = 1; i <= 2500; i++ )
                ids.Add( (Serial)( 0x40000000 + i ) );

            DateTime initialAccess = new DateTime( 2026, 2, 1, 8, 0, 0, DateTimeKind.Utc );
            DateTime updatedAccess = new DateTime( 2026, 2, 1, 8, 5, 0, DateTimeKind.Utc );
            VendorSearchSession session = new VendorSearchSession(
                2,
                (Serial)2,
                new VendorSearchQuery(),
                ids,
                new VendorSearchSummary(),
                false,
                initialAccess );

            Assert( report, "session hides results outside visible window", session.GetItemSerial( 2000 ) == Serial.Zero );
            Assert( report, "session rejects zero exposure", !session.ExposeMore( 0 ) );
            Assert( report, "session rejects negative exposure", !session.ExposeMore( -1 ) );

            List<Serial> removableIds = new List<Serial>();
            removableIds.Add( (Serial)0x40001000 );

            VendorSearchSession removableSession = new VendorSearchSession(
                3,
                (Serial)3,
                new VendorSearchQuery(),
                removableIds,
                new VendorSearchSummary(),
                false,
                initialAccess );

            bool removed = removableSession.Remove( removableSession.GetItemSerial( 0 ) );

            Assert( report, "session removes visible result", removed && removableSession.ResultCount == 0 );
            Assert( report, "session visible count stays bounded", removableSession.VisibleResultCount == 0 );

            session.Touch( updatedAccess );

            Assert( report, "session touch updates access", session.LastAccess == updatedAccess );
        }

        private static void TestQueryClone( VendorSearchTestReport report )
        {
            VendorSearchQuery source = new VendorSearchQuery();
            source.SearchTerm = "ingot";
            source.MinPrice = 25;
            source.Sort = VendorSearchSort.PriceHighToLow;
            source.PropertyFilters = new List<VendorSearchPropertyFilter>();
            source.PropertyFilters.Add( new VendorSearchPropertyFilter { Key = "material", Value = "iron" } );
            source.PropertyFilters.Add( null );

            VendorSearchQuery clone = source.Clone();
            clone.SearchTerm = "board";
            clone.PropertyFilters[0].Value = "oak";

            Assert( report, "query clone is independent", source.SearchTerm == "ingot" );
            Assert( report, "query clone keeps values", clone.MinPrice == 25 && clone.Sort == VendorSearchSort.PriceHighToLow );
            Assert( report, "query clone copies filters", source.PropertyFilters[0].Value == "iron" );
            Assert( report, "query clone preserves null filters", clone.PropertyFilters.Count == 2 && clone.PropertyFilters[1] == null );
        }

        private static void TestRecordPropertyValues( VendorSearchTestReport report )
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            values["Material"] = "iron";

            VendorSearchRecord record = new VendorSearchRecord();
            record.SetPropertyValues( values );

            values["Material"] = "oak";

            string value;
            bool found = record.TryGetPropertyValue( "material", out value );

            Assert( report, "record property values are copied", found && value == "iron" );
            Assert( report, "record property lookup is case insensitive", record.TryGetPropertyValue( "MATERIAL", out value ) && value == "iron" );
        }

        private static void TestGearPropertyFilters( VendorSearchTestReport report )
        {
            VendorSearchRecord record = CreateRecord( 90, "Test Sword", 100 );
            Dictionary<string, string> values = new Dictionary<string, string>();
            values[VendorSearchPropertyKeys.WeaponDamage] = "35";
            values[VendorSearchPropertyKeys.Material] = "Dull Copper";
            values[VendorSearchPropertyKeys.Exceptional] = "1";
            values[VendorSearchPropertyKeys.WeaponFireDamage] = "40";
            values[VendorSearchPropertyKeys.HitFireball] = "45";
            values[VendorSearchPropertyKeys.HitFireArea] = "30";
            record.SetPropertyValues( values );

            VendorSearchQuery matching = new VendorSearchQuery();
            matching.PropertyFilters = new List<VendorSearchPropertyFilter>();
            matching.PropertyFilters.Add(
                new VendorSearchPropertyFilter
                {
                    Key = VendorSearchPropertyKeys.WeaponDamage,
                    Value = "30"
                } );
            matching.PropertyFilters.Add(
                new VendorSearchPropertyFilter
                {
                    Key = VendorSearchPropertyKeys.Material,
                    Value = "copper"
                } );
            matching.PropertyFilters.Add(
                new VendorSearchPropertyFilter
                {
                    Key = VendorSearchPropertyKeys.Exceptional,
                    Value = "1"
                } );
            matching.PropertyFilters.Add(
                new VendorSearchPropertyFilter
                {
                    Key = VendorSearchPropertyKeys.WeaponFireDamage,
                    Value = "40"
                } );
            matching.PropertyFilters.Add(
                new VendorSearchPropertyFilter
                {
                    Key = VendorSearchPropertyKeys.HitFireball,
                    Value = "40"
                } );
            matching.PropertyFilters.Add(
                new VendorSearchPropertyFilter
                {
                    Key = VendorSearchPropertyKeys.HitFireArea,
                    Value = "25"
                } );

            VendorSearchQuery tooHigh = matching.Clone();
            tooHigh.PropertyFilters[0].Value = "40";

            Assert( report, "gear numeric and text filters match", Execute( new VendorSearchRecord[] { record }, matching ).Summary.TotalMatches == 1 );
            Assert( report, "gear minimum filter rejects lower value", Execute( new VendorSearchRecord[] { record }, tooHigh ).Summary.TotalMatches == 0 );
        }

        private static void TestRunTestContinues( VendorSearchTestReport report )
        {
            VendorSearchTestReport isolated = new VendorSearchTestReport();

            RunTest( isolated, "expected exception", delegate { throw new InvalidOperationException( "expected" ); } );
            RunTest( isolated, "continued test", delegate { isolated.Pass( "continued assertion" ); } );

            Assert( report, "test runner reports exceptions", isolated.Failed == 1 );
            Assert( report, "test runner continues after exceptions", isolated.Passed == 1 );
        }

        private static void TestCategoryDefinitions( VendorSearchTestReport report )
        {
            IList<VendorSearchCategoryDefinition> categories = VendorSearchCatalog.Categories;

            Assert( report, "catalog starts with all", categories.Count > 0 && categories[0].Key == "all" );
            Assert( report, "catalog excludes npc pets", VendorSearchCatalog.FindCategory( "pets" ) == null );
            Assert( report, "catalog has resource reagents", VendorSearchCatalog.FindSubcategory( "resources", "reagents" ) != null );
        }

        private static void TestCategoryCountsCanRepresentMultipleCategories( VendorSearchTestReport report )
        {
            VendorSearchRecord record = CreateRecord( 1, "Black Pearl", 50 );
            record.CategoryFlags = VendorSearchCategoryFlags.Magic | VendorSearchCategoryFlags.Resources;

            Dictionary<string, int> counts = new Dictionary<string, int>( StringComparer.OrdinalIgnoreCase );
            VendorSearchCatalog.AddCategoryCounts( record, counts );

            Assert( report, "multi-category all count", counts["all"] == 1 );
            Assert( report, "multi-category magic count", counts["magic"] == 1 );
            Assert( report, "multi-category resource count", counts["resources"] == 1 );
        }

        private static VendorSearchRecord CreateRecord( int id, string name, int price )
        {
            return new VendorSearchRecord
            {
                VendorSerial = (Serial)( 1000 + id ),
                ItemSerial = (Serial)( 0x40000000 + id ),
                ItemType = typeof( Gold ),
                ItemID = 0xEED,
                DisplayName = name,
                SearchText = name.ToLowerInvariant(),
                VendorName = "Vendor " + id,
                ShopName = "Shop " + id,
                VendorMap = Map.Trammel,
                VendorLocation = new Point3D( id, id, 0 ),
                RegionName = "Britain",
                FacetName = "Sosaria",
                CategoryFlags = VendorSearchCategoryFlags.Misc,
                Price = price,
                Amount = 1,
                PricePerItem = price,
                Created = new DateTime( 2026, 1, 1 ).AddMinutes( id )
            };
        }

        private static void TestIndexStore( VendorSearchTestReport report )
        {
            VendorSearchIndexStore store = new VendorSearchIndexStore();
            VendorSearchRecord first = CreateRecord( 1, "Board", 100 );
            VendorSearchRecord replacement = CreateRecord( 1, "Board", 80 );
            VendorSearchRecord second = CreateRecord( 2, "Ingot", 50 );
            second.VendorSerial = first.VendorSerial;

            store.Upsert( first );
            VendorSearchRecord[] snapshotOne = store.GetSnapshot();
            VendorSearchRecord[] snapshotTwo = store.GetSnapshot();

            Assert( report, "store snapshot reused while clean", Object.ReferenceEquals( snapshotOne, snapshotTwo ) );

            store.Upsert( replacement );
            store.Upsert( second );

            VendorSearchRecord found;
            Assert( report, "store replaces item record", store.TryGet( first.ItemSerial, out found ) && found.Price == 80 );
            Assert( report, "store tracks vendor entries", store.RecordCount == 2 && store.VendorCount == 1 );
            Assert( report, "store removes vendor entries", store.RemoveVendor( first.VendorSerial ) == 2 && store.RecordCount == 0 );
        }

        private static void TestVendorPlacementPolicy( VendorSearchTestReport report )
        {
            Assert(
                report,
                "world vendor placement is valid without a house",
                VendorSearchValidation.IsVendorPlacementValid( Map.Trammel, false, false ) );
            Assert(
                report,
                "internal vendor placement is invalid",
                !VendorSearchValidation.IsVendorPlacementValid( Map.Internal, false, false ) );
            Assert(
                report,
                "deleted vendor placement is invalid",
                !VendorSearchValidation.IsVendorPlacementValid( Map.Trammel, true, false ) );
            Assert(
                report,
                "placeholder vendor placement is invalid",
                !VendorSearchValidation.IsVendorPlacementValid( Map.Trammel, false, true ) );
        }

        private static void TestDirtyVendorRemoval( VendorSearchTestReport report )
        {
            VendorSearchIndexStore store = new VendorSearchIndexStore();
            VendorSearchRecord record = CreateRecord( 20, "Board", 10 );
            record.VendorSerial = (Serial)0x3FFFFFF0;
            store.Upsert( record );

            VendorSearchMaintenance.ReplayDirtyVendors( store, new Serial[] { record.VendorSerial } );

            Assert( report, "dirty missing vendor removed from staging", store.RecordCount == 0 );
        }

        private static void TestQueryFilters( VendorSearchTestReport report )
        {
            Map matchingMap = new Map( 77, 77, 77, 16, 16, 0, "Vendor Search Test", MapRules.None );
            VendorSearchRecord record = CreateRecord( 30, "Iron Ingot", 100 );
            record.SearchText = "iron ingot britain smith royal forge britain sosaria";
            record.VendorName = "Britain Smith";
            record.ShopName = "Royal Forge";
            record.VendorMap = matchingMap;
            record.Amount = 10;
            record.PricePerItem = 10.0;
            record.CategoryFlags = VendorSearchCategoryFlags.Resources;
            record.SubcategoryFlags = VendorSearchSubcategoryFlags.Metals;
            record.Created = new DateTime( 2026, 1, 10 );

            VendorSearchRecord zero = CreateRecord( 31, "Free Sample", 0 );
            VendorSearchRecord[] records = new VendorSearchRecord[] { record, zero };

            VendorSearchQuery contains = new VendorSearchQuery
            {
                SearchTerm = "iron",
                VendorName = "smith",
                RegionTerm = "sosaria",
                MapID = matchingMap.MapID,
                MinPrice = 90,
                MaxPrice = 110,
                MinPricePerItem = 9.0,
                MaxPricePerItem = 11.0,
                MinStackAmount = 10,
                CreatedAfter = new DateTime( 2026, 1, 9 ),
                CategoryKey = "resources",
                SubcategoryKey = "metals"
            };

            Assert( report, "query contains and core filters", Execute( records, contains ).Summary.TotalMatches == 1 );

            VendorSearchQuery exact = contains.Clone();
            exact.TextMode = VendorSearchTextMode.Exact;
            exact.SearchTerm = "iron ingot";

            Assert( report, "query exact match", Execute( records, exact ).Summary.TotalMatches == 1 );

            exact.SearchTerm = "iron";
            Assert( report, "query exact rejects partial", Execute( records, exact ).Summary.TotalMatches == 0 );

            VendorSearchQuery hideZero = new VendorSearchQuery { HideZeroPrice = true };
            Assert( report, "query hides zero price", Execute( records, hideZero ).Summary.TotalMatches == 1 );

            VendorSearchQuery wrongSubcategory = contains.Clone();
            wrongSubcategory.SubcategoryKey = "wood";
            Assert( report, "query filters subcategory", Execute( records, wrongSubcategory ).Summary.TotalMatches == 0 );

            VendorSearchQuery wrongMap = contains.Clone();
            wrongMap.MapID = matchingMap.MapID + 1;
            Assert( report, "query filters map", Execute( records, wrongMap ).Summary.TotalMatches == 0 );
        }

        private static void TestQuerySorts( VendorSearchTestReport report )
        {
            VendorSearchRecord a = CreateRecord( 3, "Board", 90 );
            a.VendorName = "Zulu";
            a.Amount = 5;
            a.PricePerItem = 18.0;
            a.Created = new DateTime( 2026, 1, 3 );

            VendorSearchRecord b = CreateRecord( 1, "Ingot", 30 );
            b.VendorName = "Alpha";
            b.Amount = 10;
            b.PricePerItem = 3.0;
            b.Created = new DateTime( 2026, 1, 1 );

            VendorSearchRecord c = CreateRecord( 2, "Apple", 60 );
            c.VendorName = "Middle";
            c.Amount = 10;
            c.PricePerItem = 4.0;
            c.Created = new DateTime( 2026, 1, 2 );

            VendorSearchRecord[] records = new VendorSearchRecord[] { a, b, c };

            AssertFirst( report, "sort price low", records, VendorSearchSort.PriceLowToHigh, b.ItemSerial );
            AssertFirst( report, "sort price high", records, VendorSearchSort.PriceHighToLow, a.ItemSerial );
            AssertFirst( report, "sort newest", records, VendorSearchSort.NewestListed, a.ItemSerial );
            AssertFirst( report, "sort oldest", records, VendorSearchSort.OldestListed, b.ItemSerial );
            AssertFirst( report, "sort vendor", records, VendorSearchSort.VendorName, b.ItemSerial );
            AssertFirst( report, "sort item", records, VendorSearchSort.ItemName, c.ItemSerial );
            AssertFirst( report, "sort stack amount", records, VendorSearchSort.StackAmount, b.ItemSerial );

            VendorSearchRecord tieHigh = CreateRecord( 5, "Tie", 50 );
            VendorSearchRecord tieLow = CreateRecord( 4, "Tie", 50 );
            VendorSearchExecutionResult tieResult = Execute(
                new VendorSearchRecord[] { tieHigh, tieLow },
                new VendorSearchQuery { Sort = VendorSearchSort.PriceLowToHigh } );

            Assert( report, "sort uses serial tie break", tieResult.ItemSerials[0] == tieLow.ItemSerial );
        }

        private static void TestQuerySummaryAndStableSort( VendorSearchTestReport report )
        {
            VendorSearchRecord a = CreateRecord( 3, "Iron Ingot", 90 );
            a.Amount = 10;
            a.PricePerItem = 9.0;
            a.CategoryFlags = VendorSearchCategoryFlags.Resources;
            a.SubcategoryFlags = VendorSearchSubcategoryFlags.Metals;

            VendorSearchRecord b = CreateRecord( 1, "Iron Ingot", 30 );
            b.Amount = 5;
            b.PricePerItem = 6.0;
            b.CategoryFlags = VendorSearchCategoryFlags.Resources;
            b.SubcategoryFlags = VendorSearchSubcategoryFlags.Metals;

            VendorSearchRecord c = CreateRecord( 2, "Iron Ingot", 60 );
            c.Amount = 20;
            c.PricePerItem = 3.0;
            c.CategoryFlags = VendorSearchCategoryFlags.Resources;
            c.SubcategoryFlags = VendorSearchSubcategoryFlags.Metals;

            VendorSearchQuery query = new VendorSearchQuery
            {
                SearchTerm = "iron",
                CategoryKey = "resources",
                SubcategoryKey = "metals",
                Sort = VendorSearchSort.PriceLowToHigh
            };

            VendorSearchExecutionResult result = Execute( new VendorSearchRecord[] { a, b, c }, query );

            Assert( report, "query price sort", result.ItemSerials[0] == b.ItemSerial && result.ItemSerials[2] == a.ItemSerial );
            Assert( report, "query cheapest", result.Summary.CheapestPrice == 30 );
            Assert( report, "query average", Math.Abs( result.Summary.AveragePrice - 60.0 ) < 0.001 );
            Assert( report, "query median", Math.Abs( result.Summary.MedianPrice - 60.0 ) < 0.001 );
            Assert( report, "query highest", result.Summary.HighestPrice == 90 );
            Assert( report, "query total amount", result.Summary.TotalAvailableAmount == 35 );
        }

        private static void TestCategoryCountsPrecedeSelection( VendorSearchTestReport report )
        {
            VendorSearchRecord resource = CreateRecord( 40, "Board", 10 );
            resource.CategoryFlags = VendorSearchCategoryFlags.Resources;
            resource.SubcategoryFlags = VendorSearchSubcategoryFlags.Wood;

            VendorSearchRecord magic = CreateRecord( 41, "Spellbook", 20 );
            magic.CategoryFlags = VendorSearchCategoryFlags.Magic;
            magic.SubcategoryFlags = VendorSearchSubcategoryFlags.Spellbooks;

            VendorSearchExecutionResult result = Execute(
                new VendorSearchRecord[] { resource, magic },
                new VendorSearchQuery { CategoryKey = "resources", SubcategoryKey = "all" } );

            Assert( report, "category selection limits matches", result.Summary.TotalMatches == 1 );
            Assert( report, "category counts include all core matches",
                result.Summary.CategoryCounts["resources"] == 1
                && result.Summary.CategoryCounts["magic"] == 1
                && result.Summary.CategoryCounts["all"] == 2 );
        }

        private static void TestQuerySafetyCap( VendorSearchTestReport report )
        {
            VendorSearchRecord record = CreateRecord( 50, "Board", 10 );
            VendorSearchRecord[] records = new VendorSearchRecord[VendorSearchLimits.MaximumSessionResults + 1];

            for ( int i = 0; i < records.Length; i++ )
                records[i] = record;

            VendorSearchExecutionResult result = Execute( records, new VendorSearchQuery() );

            Assert( report, "query preserves full count at safety cap", result.Summary.TotalMatches == records.Length );
            Assert( report, "query caps browsable results", result.Summary.BrowsableMatches == VendorSearchLimits.MaximumSessionResults );
            Assert( report, "query marks safety truncation", result.Truncated && result.ItemSerials.Count == VendorSearchLimits.MaximumSessionResults );
        }

        private static VendorSearchExecutionResult Execute( VendorSearchRecord[] records, VendorSearchQuery query )
        {
            return VendorSearchQueryEngine.Execute( records, query, VendorSearchRecordGate.AllowAll );
        }

        private static void AssertFirst( VendorSearchTestReport report, string name, VendorSearchRecord[] records, VendorSearchSort sort, Serial expected )
        {
            VendorSearchExecutionResult result = Execute( records, new VendorSearchQuery { Sort = sort } );
            Assert( report, name, result.ItemSerials.Count > 0 && result.ItemSerials[0] == expected );
        }

        private static void TestSessionStoreCooldownAndExpiry( VendorSearchTestReport report )
        {
            VendorSearchFakeClock clock = new VendorSearchFakeClock();
            clock.UtcNow = new DateTime( 2026, 6, 14, 12, 0, 0, DateTimeKind.Utc );

            VendorSearchSessionStore store = new VendorSearchSessionStore( clock );
            Serial playerSerial = (Serial)1234;
            string message;

            Assert( report, "session first search allowed", store.TryBeginSearch( playerSerial, out message ) );
            Assert( report, "session search cooldown", !store.TryBeginSearch( playerSerial, out message ) );

            clock.UtcNow = clock.UtcNow.AddMilliseconds( 999.0 );
            Assert( report, "session search cooldown holds before one second", !store.TryBeginSearch( playerSerial, out message ) );

            clock.UtcNow = clock.UtcNow.AddMilliseconds( 1.0 );
            Assert( report, "session search cooldown expires at one second", store.TryBeginSearch( playerSerial, out message ) );

            List<Serial> ids = new List<Serial>();

            for ( int i = 0; i < 2500; i++ )
                ids.Add( (Serial)( 0x40010000 + i ) );

            VendorSearchExecutionResult execution = new VendorSearchExecutionResult(
                ids,
                new VendorSearchSummary(),
                false );

            VendorSearchSession first = store.Set( playerSerial, new VendorSearchQuery(), execution );
            VendorSearchSession replacement = store.Set( playerSerial, new VendorSearchQuery(), execution );
            VendorSearchSession found;

            Assert( report, "session replacement keeps one active", store.Count == 1 );
            Assert( report, "session old id rejected", !store.TryGet( playerSerial, first.Id, out found ) );
            Assert( report, "session current id accepted", store.TryGet( playerSerial, replacement.Id, out found ) );

            Assert( report, "session load more allowed", store.TryLoadMore( playerSerial, replacement.Id, out message ) );
            Assert( report, "session load more cooldown", !store.TryLoadMore( playerSerial, replacement.Id, out message ) );

            DateTime previousAccess = replacement.LastAccess;
            clock.UtcNow = clock.UtcNow.AddMinutes( 1.0 );
            Assert( report, "session page access succeeds", store.TryGet( playerSerial, replacement.Id, out found ) );
            Assert( report, "session page access touches idle time", replacement.LastAccess > previousAccess );

            clock.UtcNow = clock.UtcNow.AddMinutes( 11.0 );
            Assert( report, "session idle expiry", store.SweepExpired() == 1 && store.Count == 0 );
        }

        private static void TestMapLeaseStore( VendorSearchTestReport report )
        {
            VendorSearchMapLeaseStore store = new VendorSearchMapLeaseStore();
            Serial owner = (Serial)0x00000021;
            Serial firstMap = (Serial)0x40000021;
            Serial secondMap = (Serial)0x40000022;
            Serial found;

            store.Set( owner, firstMap );
            Assert( report, "map lease stores owner map", store.TryGet( owner, out found ) && found == firstMap );

            store.Set( owner, secondMap );
            Assert( report, "map lease replaces owner map", store.TryGet( owner, out found ) && found == secondMap );

            store.Remove( owner, firstMap );
            Assert( report, "map lease ignores stale removal", store.TryGet( owner, out found ) && found == secondMap );

            store.Remove( owner, secondMap );
            Assert( report, "map lease removes current map", !store.TryGet( owner, out found ) );
        }

        private static void TestMapCooldown( VendorSearchTestReport report )
        {
            VendorSearchFakeClock clock = new VendorSearchFakeClock
            {
                UtcNow = new DateTime( 2026, 1, 1, 12, 0, 0, DateTimeKind.Utc )
            };
            VendorSearchSessionStore store = new VendorSearchSessionStore( clock );
            Serial player = (Serial)0x00000022;
            string message;

            Assert( report, "session first map action allowed", store.TryBeginMap( player, out message ) );
            Assert( report, "session map action cooldown", !store.TryBeginMap( player, out message ) );

            clock.UtcNow += VendorSearchLimits.MapActionCooldown;
            Assert( report, "session map cooldown expires", store.TryBeginMap( player, out message ) );
        }

        private static void TestMapLeaseOwnership( VendorSearchTestReport report )
        {
            Serial owner = (Serial)0x00000024;
            Serial other = (Serial)0x00000025;

            Assert(
                report,
                "map lease reuses world map",
                VendorSearchMapLeasePolicy.CanReuse( owner, owner, Serial.Zero ) );
            Assert(
                report,
                "map lease reuses owner-held map",
                VendorSearchMapLeasePolicy.CanReuse( owner, owner, owner ) );
            Assert(
                report,
                "map lease rejects another holder",
                !VendorSearchMapLeasePolicy.CanReuse( owner, owner, other ) );
            Assert(
                report,
                "map lease rejects wrong stored owner",
                !VendorSearchMapLeasePolicy.CanReuse( owner, other, Serial.Zero ) );
            Assert(
                report,
                "map lease owner can lift",
                VendorSearchMapLeasePolicy.CanLift( owner, owner, false ) );
            Assert(
                report,
                "map lease rejects another lifter",
                !VendorSearchMapLeasePolicy.CanLift( owner, other, false ) );
            Assert(
                report,
                "map lease permits staff lifter",
                VendorSearchMapLeasePolicy.CanLift( owner, other, true ) );
            Assert(
                report,
                "unleased map permits normal lift",
                VendorSearchMapLeasePolicy.CanLift( Serial.Zero, other, false ) );
        }

        private static void TestPageRowCapture( VendorSearchTestReport report )
        {
            List<Serial> ids = new List<Serial>();

            for ( int i = 1; i <= 6; i++ )
                ids.Add( (Serial)( 0x40000030 + i ) );

            VendorSearchSession session = new VendorSearchSession(
                1,
                (Serial)0x00000023,
                new VendorSearchQuery(),
                ids,
                new VendorSearchSummary(),
                false,
                new DateTime( 2026, 1, 1, 12, 0, 0, DateTimeKind.Utc ) );
            Serial[] rows = session.CapturePage( 0, 5 );
            Serial removed = rows[0];
            Serial second = rows[1];
            Serial third = rows[2];

            session.Remove( removed );

            Assert( report, "page row capture keeps original target", rows.Length == 5 && rows[1] == second );
            Assert( report, "page row capture is independent", session.GetItemSerial( 1 ) == third );
        }

        private static void TestMergedDropRefreshPolicy( VendorSearchTestReport report )
        {
            Assert(
                report,
                "merged drop requires vendor refresh",
                VendorSearchMutationPolicy.ShouldRefreshAfterDrop( true ) );
            Assert(
                report,
                "normal drop uses listing update",
                !VendorSearchMutationPolicy.ShouldRefreshAfterDrop( false ) );
        }

        private static void TestStartupCoordinator( VendorSearchTestReport report )
        {
            VendorSearchStartupCoordinator coordinator = new VendorSearchStartupCoordinator();
            int initializeCalls = 0;
            int maintenanceCalls = 0;

            bool first = coordinator.Initialize(
                delegate { initializeCalls++; },
                delegate { maintenanceCalls++; } );
            bool second = coordinator.Initialize(
                delegate { initializeCalls++; },
                delegate { maintenanceCalls++; } );

            Assert(
                report,
                "startup coordinator initializes once",
                first && !second && initializeCalls == 1 );
            Assert(
                report,
                "startup coordinator starts maintenance once",
                maintenanceCalls == 1 );
        }

        private static void RunTest( VendorSearchTestReport report, string name, Action test )
        {
            try
            {
                test();
            }
            catch ( Exception ex )
            {
                report.Fail( name, ex.Message );
            }
        }

        private static void Assert( VendorSearchTestReport report, string name, bool condition )
        {
            if ( condition )
                report.Pass( name );
            else
                report.Fail( name, "assertion was false" );
        }
    }
}
