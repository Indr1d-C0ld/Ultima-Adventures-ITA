using System;
using System.Collections.Generic;

namespace Server.Custom.VendorSearch
{
    internal interface IVendorSearchRecordGate
    {
        bool Include( VendorSearchRecord record );
    }

    internal sealed class VendorSearchRecordGate : IVendorSearchRecordGate
    {
        public static readonly VendorSearchRecordGate AllowAll = new VendorSearchRecordGate();

        public bool Include( VendorSearchRecord record )
        {
            return record != null;
        }
    }

    internal static class VendorSearchQueryEngine
    {
        internal static VendorSearchExecutionResult Execute( VendorSearchRecord[] records, VendorSearchQuery query, IVendorSearchRecordGate gate )
        {
            if ( records == null )
                records = new VendorSearchRecord[0];

            if ( query == null )
                query = new VendorSearchQuery();

            VendorSearchCatalog.NormalizeQuery( query );

            List<VendorSearchRecord> matches = new List<VendorSearchRecord>();
            VendorSearchSummary summary = new VendorSearchSummary();

            for ( int i = 0; i < records.Length; i++ )
            {
                VendorSearchRecord record = records[i];

                if ( !MatchesCoreFilters( record, query ) )
                    continue;

                if ( gate != null && !gate.Include( record ) )
                    continue;

                if ( !VendorSearchPropertyRegistry.Matches( record, query.PropertyFilters ) )
                    continue;

                VendorSearchCatalog.AddCategoryCounts( record, summary.CategoryCounts );

                if ( !MatchesSelectedCategory( record, query ) )
                    continue;

                matches.Add( record );
            }

            matches.Sort( CreateComparison( query.Sort ) );
            FillSummary( matches, summary );

            int browsable = Math.Min( matches.Count, VendorSearchLimits.MaximumSessionResults );
            List<Serial> serials = new List<Serial>( browsable );

            for ( int i = 0; i < browsable; i++ )
                serials.Add( matches[i].ItemSerial );

            summary.TotalMatches = matches.Count;
            summary.BrowsableMatches = browsable;

            return new VendorSearchExecutionResult( serials, summary, matches.Count > browsable );
        }

        private static bool MatchesCoreFilters( VendorSearchRecord record, VendorSearchQuery query )
        {
            if ( record == null || record.Price < 0 )
                return false;

            string term = ( query.SearchTerm ?? String.Empty ).Trim().ToLowerInvariant();

            if ( term.Length > 0 )
            {
                if ( query.TextMode == VendorSearchTextMode.Exact )
                {
                    if ( !String.Equals(
                        record.DisplayName,
                        query.SearchTerm.Trim(),
                        StringComparison.OrdinalIgnoreCase ) )
                        return false;
                }
                else if ( String.IsNullOrEmpty( record.SearchText )
                    || record.SearchText.IndexOf( term, StringComparison.Ordinal ) < 0 )
                {
                    return false;
                }
            }

            string vendorTerm = ( query.VendorName ?? String.Empty ).Trim();

            if ( vendorTerm.Length > 0
                && IndexOfIgnoreCase( record.VendorName, vendorTerm ) < 0
                && IndexOfIgnoreCase( record.ShopName, vendorTerm ) < 0 )
                return false;

            string regionTerm = ( query.RegionTerm ?? String.Empty ).Trim();

            if ( regionTerm.Length > 0
                && IndexOfIgnoreCase( record.RegionName, regionTerm ) < 0
                && IndexOfIgnoreCase( record.FacetName, regionTerm ) < 0 )
                return false;

            if ( query.MapID.HasValue
                && ( record.VendorMap == null || record.VendorMap.MapID != query.MapID.Value ) )
                return false;

            if ( query.MinPrice.HasValue && record.Price < query.MinPrice.Value )
                return false;
            if ( query.MaxPrice.HasValue && record.Price > query.MaxPrice.Value )
                return false;
            if ( query.MinPricePerItem.HasValue && record.PricePerItem < query.MinPricePerItem.Value )
                return false;
            if ( query.MaxPricePerItem.HasValue && record.PricePerItem > query.MaxPricePerItem.Value )
                return false;
            if ( query.MinStackAmount.HasValue && record.Amount < query.MinStackAmount.Value )
                return false;
            if ( query.CreatedAfter.HasValue && record.Created < query.CreatedAfter.Value )
                return false;
            if ( query.HideZeroPrice && record.Price == 0 )
                return false;

            return true;
        }

        private static bool MatchesSelectedCategory( VendorSearchRecord record, VendorSearchQuery query )
        {
            VendorSearchCategoryDefinition category = VendorSearchCatalog.FindCategory( query.CategoryKey );

            if ( category == null || category.Key == "all" )
                return true;

            if ( ( record.CategoryFlags & category.Flag ) == 0 )
                return false;

            VendorSearchSubcategoryDefinition subcategory =
                VendorSearchCatalog.FindSubcategory( category.Key, query.SubcategoryKey );

            return subcategory == null
                || subcategory.Flag == VendorSearchSubcategoryFlags.None
                || ( record.SubcategoryFlags & subcategory.Flag ) != 0;
        }

        private static Comparison<VendorSearchRecord> CreateComparison( VendorSearchSort sort )
        {
            switch ( sort )
            {
                case VendorSearchSort.PriceHighToLow:
                    return delegate( VendorSearchRecord x, VendorSearchRecord y )
                    {
                        int result = y.Price.CompareTo( x.Price );
                        return result != 0 ? result : CompareTie( x, y );
                    };
                case VendorSearchSort.NewestListed:
                    return delegate( VendorSearchRecord x, VendorSearchRecord y )
                    {
                        int result = y.Created.CompareTo( x.Created );
                        return result != 0 ? result : CompareTie( x, y );
                    };
                case VendorSearchSort.OldestListed:
                    return delegate( VendorSearchRecord x, VendorSearchRecord y )
                    {
                        int result = x.Created.CompareTo( y.Created );
                        return result != 0 ? result : CompareTie( x, y );
                    };
                case VendorSearchSort.VendorName:
                    return delegate( VendorSearchRecord x, VendorSearchRecord y )
                    {
                        int result = String.Compare(
                            x.VendorName ?? String.Empty,
                            y.VendorName ?? String.Empty,
                            StringComparison.OrdinalIgnoreCase );
                        return result != 0 ? result : CompareTie( x, y );
                    };
                case VendorSearchSort.ItemName:
                    return delegate( VendorSearchRecord x, VendorSearchRecord y )
                    {
                        int result = String.Compare(
                            x.DisplayName ?? String.Empty,
                            y.DisplayName ?? String.Empty,
                            StringComparison.OrdinalIgnoreCase );
                        return result != 0 ? result : CompareTie( x, y );
                    };
                case VendorSearchSort.StackAmount:
                    return delegate( VendorSearchRecord x, VendorSearchRecord y )
                    {
                        int result = y.Amount.CompareTo( x.Amount );

                        if ( result == 0 )
                            result = x.PricePerItem.CompareTo( y.PricePerItem );

                        return result != 0 ? result : CompareTie( x, y );
                    };
                default:
                    return delegate( VendorSearchRecord x, VendorSearchRecord y )
                    {
                        int result = x.Price.CompareTo( y.Price );
                        return result != 0 ? result : CompareTie( x, y );
                    };
            }
        }

        private static int CompareTie( VendorSearchRecord x, VendorSearchRecord y )
        {
            return x.ItemSerial.Value.CompareTo( y.ItemSerial.Value );
        }

        private static int IndexOfIgnoreCase( string source, string value )
        {
            if ( String.IsNullOrEmpty( source ) || String.IsNullOrEmpty( value ) )
                return -1;

            return source.IndexOf( value, StringComparison.OrdinalIgnoreCase );
        }

        private static void FillSummary( List<VendorSearchRecord> matches, VendorSearchSummary summary )
        {
            if ( matches.Count == 0 )
                return;

            List<int> prices = new List<int>( matches.Count );
            long totalPrice = 0;
            long totalAmount = 0;
            int cheapest = Int32.MaxValue;
            int highest = Int32.MinValue;

            for ( int i = 0; i < matches.Count; i++ )
            {
                VendorSearchRecord record = matches[i];
                prices.Add( record.Price );
                totalPrice += record.Price;
                totalAmount += Math.Max( 1, record.Amount );
                cheapest = Math.Min( cheapest, record.Price );
                highest = Math.Max( highest, record.Price );
            }

            prices.Sort();

            int middle = prices.Count / 2;
            double median = ( prices.Count & 1 ) == 1
                ? prices[middle]
                : ( (long)prices[middle - 1] + prices[middle] ) / 2.0;

            summary.CheapestPrice = cheapest;
            summary.HighestPrice = highest;
            summary.AveragePrice = (double)totalPrice / matches.Count;
            summary.MedianPrice = median;
            summary.TotalAvailableAmount = totalAmount;
        }
    }
}
