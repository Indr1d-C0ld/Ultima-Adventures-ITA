using System;
using System.Collections.Generic;
using System.Globalization;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Custom.VendorSearch
{
    public sealed class VendorSearchGump : Gump
    {
        private const int BackgroundWidth = 1180;
        private const int BackgroundHeight = 860;
        private const int InnerPadding = 20;
        private const int FilterPanelX = 35;
        private const int FilterPanelY = 135;
        private const int FilterPanelWidth = 285;
        private const int FilterPanelHeight = 660;
        private const int ResultsPanelX = 340;
        private const int ResultsPanelY = 135;
        private const int ResultsPanelWidth = 805;
        private const int ResultRowHeight = 122;
        private const int ItemSquareSize = 88;

        private const int SearchButtonID = 1;
        private const int PreviousButtonID = 2;
        private const int NextButtonID = 3;
        private const int LoadMoreButtonID = 4;
        private const int SortButtonID = 5;
        private const int AdvancedButtonID = 6;
        private const int TextModeButtonID = 7;
        private const int ZeroPriceButtonID = 8;
        private const int MapFilterButtonID = 9;
        private const int AdvancedCategoryButtonID = 10;
        private const int AdvancedSubcategoryButtonID = 11;
        private const int ClearStatsButtonID = 12;
        private const int SpellChannelingButtonID = 20;
        private const int MageArmorButtonID = 21;
        private const int ExceptionalButtonID = 22;
        private const int IdentifiedButtonID = 23;
        private const int GeneralStatsButtonID = 24;
        private const int WeaponEffectsButtonID = 25;
        private const int CategoryButtonBase = 100;
        private const int SubcategoryButtonBase = 200;
        private const int MapButtonBase = 1000;

        private const int SearchEntryID = 1;
        private const int VendorEntryID = 2;
        private const int RegionEntryID = 3;
        private const int MinPriceEntryID = 4;
        private const int MaxPriceEntryID = 5;
        private const int MinPerItemEntryID = 6;
        private const int MaxPerItemEntryID = 7;
        private const int MinStackEntryID = 8;
        private const int CreatedAfterEntryID = 9;
        private const int MaterialEntryID = 100;
        private const int SlayerEntryID = 101;
        private const int EnhancePotionsEntryID = 102;

        private sealed class PropertyEntryDefinition
        {
            public PropertyEntryDefinition( int entryID, string key, string label )
            {
                EntryID = entryID;
                Key = key;
                Label = label;
            }

            public int EntryID;
            public string Key;
            public string Label;
        }

        private static readonly PropertyEntryDefinition[] m_GeneralPropertyEntries =
            new PropertyEntryDefinition[]
            {
                new PropertyEntryDefinition( 110, VendorSearchPropertyKeys.WeaponDamage, "Damage increase" ),
                new PropertyEntryDefinition( 111, VendorSearchPropertyKeys.HitChance, "Hit chance" ),
                new PropertyEntryDefinition( 112, VendorSearchPropertyKeys.DefenseChance, "Defense chance" ),
                new PropertyEntryDefinition( 113, VendorSearchPropertyKeys.SwingSpeed, "Swing speed" ),
                new PropertyEntryDefinition( 115, VendorSearchPropertyKeys.SelfRepair, "Self repair" ),
                new PropertyEntryDefinition( 116, VendorSearchPropertyKeys.Durability, "Max durability" ),
                new PropertyEntryDefinition( 117, VendorSearchPropertyKeys.LowerStatRequirement, "Lower stat req." ),
                new PropertyEntryDefinition( 118, VendorSearchPropertyKeys.SpellDamage, "Spell damage" ),
                new PropertyEntryDefinition( 119, VendorSearchPropertyKeys.FasterCasting, "Faster casting" ),
                new PropertyEntryDefinition( 120, VendorSearchPropertyKeys.FasterCastRecovery, "Cast recovery" ),
                new PropertyEntryDefinition( 121, VendorSearchPropertyKeys.LowerManaCost, "Lower mana cost" ),
                new PropertyEntryDefinition( 122, VendorSearchPropertyKeys.LowerReagentCost, "Lower reagent cost" ),
                new PropertyEntryDefinition( 123, VendorSearchPropertyKeys.Luck, "Luck" ),
                new PropertyEntryDefinition( 124, VendorSearchPropertyKeys.BonusStrength, "Strength bonus" ),
                new PropertyEntryDefinition( 125, VendorSearchPropertyKeys.BonusDexterity, "Dexterity bonus" ),
                new PropertyEntryDefinition( 126, VendorSearchPropertyKeys.BonusIntelligence, "Intelligence bonus" ),
                new PropertyEntryDefinition( 127, VendorSearchPropertyKeys.BonusHits, "Hit point bonus" ),
                new PropertyEntryDefinition( 128, VendorSearchPropertyKeys.BonusStamina, "Stamina bonus" ),
                new PropertyEntryDefinition( 129, VendorSearchPropertyKeys.BonusMana, "Mana bonus" ),
                new PropertyEntryDefinition( 130, VendorSearchPropertyKeys.RegenHits, "Hit point regen" ),
                new PropertyEntryDefinition( 131, VendorSearchPropertyKeys.RegenStamina, "Stamina regen" ),
                new PropertyEntryDefinition( 132, VendorSearchPropertyKeys.RegenMana, "Mana regen" ),
                new PropertyEntryDefinition( 133, VendorSearchPropertyKeys.PhysicalResist, "Physical resist" ),
                new PropertyEntryDefinition( 134, VendorSearchPropertyKeys.FireResist, "Fire resist" ),
                new PropertyEntryDefinition( 135, VendorSearchPropertyKeys.ColdResist, "Cold resist" ),
                new PropertyEntryDefinition( 136, VendorSearchPropertyKeys.PoisonResist, "Poison resist" ),
                new PropertyEntryDefinition( 137, VendorSearchPropertyKeys.EnergyResist, "Energy resist" )
            };

        private static readonly PropertyEntryDefinition[] m_WeaponPropertyEntries =
            new PropertyEntryDefinition[]
            {
                new PropertyEntryDefinition( 200, VendorSearchPropertyKeys.WeaponPhysicalDamage, "Physical damage %" ),
                new PropertyEntryDefinition( 201, VendorSearchPropertyKeys.WeaponFireDamage, "Fire damage %" ),
                new PropertyEntryDefinition( 202, VendorSearchPropertyKeys.WeaponColdDamage, "Cold damage %" ),
                new PropertyEntryDefinition( 203, VendorSearchPropertyKeys.WeaponPoisonDamage, "Poison damage %" ),
                new PropertyEntryDefinition( 204, VendorSearchPropertyKeys.WeaponEnergyDamage, "Energy damage %" ),
                new PropertyEntryDefinition( 205, VendorSearchPropertyKeys.WeaponChaosDamage, "Chaos damage %" ),
                new PropertyEntryDefinition( 206, VendorSearchPropertyKeys.WeaponDirectDamage, "Direct damage %" ),
                new PropertyEntryDefinition( 207, VendorSearchPropertyKeys.HitMagicArrow, "Hit magic arrow" ),
                new PropertyEntryDefinition( 208, VendorSearchPropertyKeys.HitHarm, "Hit harm" ),
                new PropertyEntryDefinition( 209, VendorSearchPropertyKeys.HitFireball, "Hit fireball" ),
                new PropertyEntryDefinition( 210, VendorSearchPropertyKeys.HitLightning, "Hit lightning" ),
                new PropertyEntryDefinition( 211, VendorSearchPropertyKeys.HitDispel, "Hit dispel" ),
                new PropertyEntryDefinition( 212, VendorSearchPropertyKeys.HitPhysicalArea, "Physical area" ),
                new PropertyEntryDefinition( 213, VendorSearchPropertyKeys.HitFireArea, "Fire area" ),
                new PropertyEntryDefinition( 214, VendorSearchPropertyKeys.HitColdArea, "Cold area" ),
                new PropertyEntryDefinition( 215, VendorSearchPropertyKeys.HitPoisonArea, "Poison area" ),
                new PropertyEntryDefinition( 216, VendorSearchPropertyKeys.HitEnergyArea, "Energy area" ),
                new PropertyEntryDefinition( 217, VendorSearchPropertyKeys.HitLeechHits, "Hit life leech" ),
                new PropertyEntryDefinition( 218, VendorSearchPropertyKeys.HitLeechStamina, "Hit stamina leech" ),
                new PropertyEntryDefinition( 219, VendorSearchPropertyKeys.HitLeechMana, "Hit mana leech" ),
                new PropertyEntryDefinition( 220, VendorSearchPropertyKeys.HitLowerAttack, "Hit lower attack" ),
                new PropertyEntryDefinition( 221, VendorSearchPropertyKeys.HitLowerDefense, "Hit lower defense" )
            };

        private readonly Mobile m_From;
        private readonly int m_SessionId;
        private VendorSearchSession m_Session;
        private VendorSearchQuery m_Query;
        private Serial[] m_RowItemSerials = new Serial[0];

        public VendorSearchGump( Mobile from, int sessionId )
            : base( 35, 40 )
        {
            if ( from == null )
                throw new ArgumentNullException( "from" );

            m_From = from;
            m_SessionId = sessionId;

            VendorSearchSession session;

            if ( !VendorSearchSessionManager.TryGet( from, sessionId, out session ) )
                return;

            m_Session = session;
            m_Query = session.Query.Clone();

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            Build();
        }

        private void Build()
        {
            VendorSearchCatalog.NormalizeQuery( m_Query );

            int pageCount = GetPageCount( m_Session.VisibleResultCount );

            if ( m_Query.Page >= pageCount )
                m_Query.Page = pageCount - 1;

            if ( m_Query.Page < 0 )
                m_Query.Page = 0;

            m_Session.Query.Page = m_Query.Page;

            AddPage( 0 );
            AddBackground( 0, 0, BackgroundWidth, BackgroundHeight, 9270 );
            AddImageTiled(
                InnerPadding,
                InnerPadding,
                BackgroundWidth - ( InnerPadding * 2 ),
                BackgroundHeight - ( InnerPadding * 2 ),
                2624 );
            AddAlphaRegion(
                InnerPadding,
                InnerPadding,
                BackgroundWidth - ( InnerPadding * 2 ),
                BackgroundHeight - ( InnerPadding * 2 ) );

            AddHtml(
                0,
                32,
                BackgroundWidth,
                35,
                "<CENTER><BASEFONT COLOR=#FFFFFF>Player Vendor Search</BASEFONT></CENTER>",
                false,
                false );

            DrawHeader( pageCount );
            DrawFilterPanel();

            if ( !m_Query.ShowAdvanced )
                DrawResults( pageCount );
        }

        private void DrawHeader( int pageCount )
        {
            AddHtml( 40, 73, 65, 22, "<BASEFONT COLOR=#F7D774>Search</BASEFONT>", false, false );
            AddEntryBackground( 105, 70, 330, 24 );
            AddTextEntry( 109, 72, 322, 20, 0x34, SearchEntryID, m_Query.SearchTerm ?? String.Empty );

            AddButton( 448, 69, 4005, 4007, SearchButtonID, GumpButtonType.Reply, 0 );
            AddHtml( 484, 73, 52, 22, "<BASEFONT COLOR=#FFFFFF>Find</BASEFONT>", false, false );

            AddButton( 555, 69, 4005, 4007, SortButtonID, GumpButtonType.Reply, 0 );
            AddHtml(
                591,
                73,
                190,
                22,
                String.Format(
                    "<BASEFONT COLOR=#FFFFFF>Sort: {0}</BASEFONT>",
                    Utility.FixHtml( GetSortLabel( m_Query.Sort ) ) ),
                false,
                false );

            AddButton( 800, 69, 4005, 4007, AdvancedButtonID, GumpButtonType.Reply, 0 );
            AddHtml(
                836,
                73,
                150,
                22,
                String.Format(
                    "<BASEFONT COLOR=#FFFFFF>{0} Filters</BASEFONT>",
                    m_Query.ShowAdvanced ? "Basic" : "Advanced" ),
                false,
                false );

            string summary;

            if ( m_Query.ShowAdvanced )
            {
                summary = String.Format(
                    CultureInfo.InvariantCulture,
                    "Advanced item filters   Active stat filters: {0:N0}",
                    GetActivePropertyFilterCount( m_Query ) );
            }
            else
            {
                summary = String.Format(
                    CultureInfo.InvariantCulture,
                    "Results: {0:N0}   Browsing: {1:N0}   Page: {2}/{3}   Price snapshot: {4:N0} / {5:N0} avg / {6:N0} median / {7:N0}",
                    m_Session.Summary.TotalMatches,
                    m_Session.VisibleResultCount,
                    m_Session.VisibleResultCount == 0 ? 0 : m_Query.Page + 1,
                    pageCount,
                    m_Session.Summary.CheapestPrice,
                    m_Session.Summary.AveragePrice,
                    m_Session.Summary.MedianPrice,
                    m_Session.Summary.HighestPrice );
            }

            AddHtml(
                40,
                103,
                1095,
                22,
                "<BASEFONT COLOR=#9DC6FF>" + summary + "</BASEFONT>",
                false,
                false );

            if ( m_Session.Truncated && !m_Query.ShowAdvanced )
            {
                AddHtml(
                    40,
                    120,
                    1095,
                    18,
                    "<BASEFONT COLOR=#FFCC66>The result list reached the 100,000-listing safety limit. Refine the filters for complete browsing.</BASEFONT>",
                    false,
                    false );
            }
        }

        private void DrawFilterPanel()
        {
            if ( m_Query.ShowAdvanced )
            {
                AddImageTiled(
                    FilterPanelX,
                    FilterPanelY,
                    BackgroundWidth - ( FilterPanelX * 2 ),
                    FilterPanelHeight,
                    2624 );
                AddAlphaRegion(
                    FilterPanelX,
                    FilterPanelY,
                    BackgroundWidth - ( FilterPanelX * 2 ),
                    FilterPanelHeight );
                DrawAdvancedFilters();
            }
            else
            {
                AddImageTiled( FilterPanelX, FilterPanelY, FilterPanelWidth, FilterPanelHeight, 2624 );
                AddAlphaRegion( FilterPanelX, FilterPanelY, FilterPanelWidth, FilterPanelHeight );
                DrawBasicFilters();
            }
        }

        private void DrawBasicFilters()
        {
            AddHtml( 50, 150, 245, 24, "<BASEFONT COLOR=#F7D774>Categories</BASEFONT>", false, false );

            IList<VendorSearchCategoryDefinition> categories = VendorSearchCatalog.Categories;
            int categoryY = 178;

            for ( int i = 0; i < categories.Count; i++ )
            {
                VendorSearchCategoryDefinition category = categories[i];
                bool selected = String.Equals( category.Key, m_Query.CategoryKey, StringComparison.OrdinalIgnoreCase );
                int count;
                m_Session.Summary.CategoryCounts.TryGetValue( category.Key, out count );

                AddButton(
                    50,
                    categoryY,
                    selected ? 4006 : 4005,
                    selected ? 4006 : 4007,
                    CategoryButtonBase + i,
                    GumpButtonType.Reply,
                    0 );
                AddHtml(
                    86,
                    categoryY + 3,
                    210,
                    20,
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "<BASEFONT COLOR={0}>{1} ({2:N0})</BASEFONT>",
                        selected ? "#C5FFAE" : "#FFFFFF",
                        Utility.FixHtml( category.Name ),
                        count ),
                    false,
                    false );

                categoryY += 24;
            }

            VendorSearchCategoryDefinition current = VendorSearchCatalog.FindCategory( m_Query.CategoryKey );

            if ( current == null )
                return;

            int subcategoryHeaderY = 420;
            AddHtml( 50, subcategoryHeaderY, 245, 24, "<BASEFONT COLOR=#F7D774>Subcategories</BASEFONT>", false, false );

            int subcategoryY = subcategoryHeaderY + 28;

            for ( int i = 0; i < current.Subcategories.Count; i++ )
            {
                VendorSearchSubcategoryDefinition subcategory = current.Subcategories[i];
                bool selected = String.Equals(
                    subcategory.Key,
                    m_Query.SubcategoryKey,
                    StringComparison.OrdinalIgnoreCase );

                AddButton(
                    50,
                    subcategoryY,
                    selected ? 4006 : 4005,
                    selected ? 4006 : 4007,
                    SubcategoryButtonBase + i,
                    GumpButtonType.Reply,
                    0 );
                AddHtml(
                    86,
                    subcategoryY + 3,
                    210,
                    20,
                    String.Format(
                        "<BASEFONT COLOR={0}>{1}</BASEFONT>",
                        selected ? "#C5FFAE" : "#FFFFFF",
                        Utility.FixHtml( subcategory.Name ) ),
                    false,
                    false );

                subcategoryY += 24;
            }
        }

        private void DrawAdvancedFilters()
        {
            int generalX = 50;
            int generalWidth = 285;

            AddHtml( generalX, 150, generalWidth, 24, "<BASEFONT COLOR=#F7D774>Listing Filters</BASEFONT>", false, false );
            AddField( "Vendor / shop", VendorEntryID, m_Query.VendorName, generalX, 178, generalWidth );
            AddField( "Region / city", RegionEntryID, m_Query.RegionTerm, generalX, 228, generalWidth );

            AddAdvancedChoice(
                generalX,
                280,
                "Category",
                AdvancedCategoryButtonID,
                GetCategoryLabel( m_Query.CategoryKey ) );
            AddAdvancedChoice(
                generalX,
                312,
                "Subcategory",
                AdvancedSubcategoryButtonID,
                GetSubcategoryLabel( m_Query.CategoryKey, m_Query.SubcategoryKey ) );
            AddAdvancedChoice(
                generalX,
                344,
                "Facet",
                MapFilterButtonID,
                GetMapLabel( m_Query.MapID ) );

            AddHtml( generalX, 382, generalWidth, 20, "<BASEFONT COLOR=#FFFFFF>Total price range</BASEFONT>", false, false );
            AddEntryBackground( generalX, 402, 128, 24 );
            AddTextEntry( generalX + 4, 404, 120, 20, 0x34, MinPriceEntryID, FormatNullable( m_Query.MinPrice ) );
            AddEntryBackground( generalX + 157, 402, 128, 24 );
            AddTextEntry( generalX + 161, 404, 120, 20, 0x34, MaxPriceEntryID, FormatNullable( m_Query.MaxPrice ) );

            AddHtml( generalX, 438, generalWidth, 20, "<BASEFONT COLOR=#FFFFFF>Per-item price range</BASEFONT>", false, false );
            AddEntryBackground( generalX, 458, 128, 24 );
            AddTextEntry( generalX + 4, 460, 120, 20, 0x34, MinPerItemEntryID, FormatNullable( m_Query.MinPricePerItem ) );
            AddEntryBackground( generalX + 157, 458, 128, 24 );
            AddTextEntry( generalX + 161, 460, 120, 20, 0x34, MaxPerItemEntryID, FormatNullable( m_Query.MaxPricePerItem ) );

            AddCompactField(
                "Min stack",
                MinStackEntryID,
                FormatNullable( m_Query.MinStackAmount ),
                generalX,
                502,
                generalWidth );
            AddCompactField(
                "Listed after",
                CreatedAfterEntryID,
                m_Query.CreatedAfter.HasValue
                    ? m_Query.CreatedAfter.Value.ToString( "yyyy-MM-dd", CultureInfo.InvariantCulture )
                    : String.Empty,
                generalX,
                536,
                generalWidth,
                112 );

            AddAdvancedToggle(
                generalX,
                580,
                TextModeButtonID,
                "Name match",
                m_Query.TextMode == VendorSearchTextMode.Exact ? "Exact" : "Contains" );
            AddAdvancedToggle(
                generalX,
                612,
                ZeroPriceButtonID,
                "Zero-price",
                m_Query.HideZeroPrice ? "Hidden" : "Shown" );

            AddHtml(
                generalX,
                662,
                generalWidth,
                70,
                "<BASEFONT COLOR=#9DC6FF>All numeric gear fields are minimum values. Leave a field blank to disable it.</BASEFONT>",
                false,
                true );

            DrawPropertyFilters();
        }

        private void DrawPropertyFilters()
        {
            int startX = 365;
            int sectionWidth = 775;
            PropertyEntryDefinition[] entries = GetPropertyEntries( m_Query.AdvancedPage );

            AddHtml( startX, 150, sectionWidth, 24, "<BASEFONT COLOR=#F7D774>Item Attributes and Stats</BASEFONT>", false, false );

            AddPropertyPageButton(
                startX,
                178,
                GeneralStatsButtonID,
                "General",
                m_Query.AdvancedPage == VendorSearchAdvancedPage.General );
            AddPropertyPageButton(
                startX + 125,
                178,
                WeaponEffectsButtonID,
                "Weapon Effects",
                m_Query.AdvancedPage == VendorSearchAdvancedPage.WeaponEffects );

            AddButton( startX + 520, 178, 4005, 4007, ClearStatsButtonID, GumpButtonType.Reply, 0 );
            AddHtml( startX + 556, 182, 170, 20, "<BASEFONT COLOR=#FFFFFF>Clear item filters</BASEFONT>", false, false );

            AddCompactField(
                "Material",
                MaterialEntryID,
                GetPropertyFilterValue( m_Query, VendorSearchPropertyKeys.Material ),
                startX,
                212,
                245,
                150 );
            AddCompactField(
                "Slayer",
                SlayerEntryID,
                GetPropertyFilterValue( m_Query, VendorSearchPropertyKeys.Slayer ),
                startX + 255,
                212,
                245,
                150 );
            AddCompactField(
                "Enhance P.",
                EnhancePotionsEntryID,
                GetPropertyFilterValue( m_Query, VendorSearchPropertyKeys.EnhancePotions ),
                startX + 510,
                212,
                245,
                150 );

            int[] columnX = new int[] { startX, startX + 255, startX + 510 };
            int rowsPerColumn = ( entries.Length + 2 ) / 3;

            for ( int i = 0; i < entries.Length; i++ )
            {
                int column = i / rowsPerColumn;
                int row = i % rowsPerColumn;

                AddCompactField(
                    entries[i].Label,
                    entries[i].EntryID,
                    GetPropertyFilterValue( m_Query, entries[i].Key ),
                    columnX[column],
                    256 + ( row * 34 ),
                    245 );
            }

            AddRequiredPropertyToggle(
                startX,
                580,
                SpellChannelingButtonID,
                "Spell channeling",
                VendorSearchPropertyKeys.SpellChanneling );
            AddRequiredPropertyToggle(
                startX + 255,
                580,
                MageArmorButtonID,
                "Mage armor",
                VendorSearchPropertyKeys.MageArmor );
            AddRequiredPropertyToggle(
                startX + 510,
                580,
                ExceptionalButtonID,
                "Exceptional",
                VendorSearchPropertyKeys.Exceptional );
            AddRequiredPropertyToggle(
                startX,
                612,
                IdentifiedButtonID,
                "Identified",
                VendorSearchPropertyKeys.Identified );

            AddHtml(
                startX,
                662,
                sectionWidth,
                70,
                "<BASEFONT COLOR=#9DC6FF>Damage type and hit-effect values are minimum percentages. Multiple filled fields must all match the same listing.</BASEFONT>",
                false,
                true );
        }

        private void AddPropertyPageButton(
            int x,
            int y,
            int buttonID,
            string label,
            bool selected )
        {
            AddButton(
                x,
                y,
                selected ? 4006 : 4005,
                selected ? 4006 : 4007,
                buttonID,
                GumpButtonType.Reply,
                0 );
            AddHtml(
                x + 36,
                y + 3,
                115,
                20,
                String.Format(
                    "<BASEFONT COLOR={0}>{1}</BASEFONT>",
                    selected ? "#C5FFAE" : "#FFFFFF",
                    Utility.FixHtml( label ) ),
                false,
                false );
        }

        private void DrawResults( int pageCount )
        {
            AddHtml( ResultsPanelX, ResultsPanelY + 7, 300, 24, "<BASEFONT COLOR=#F7D774>Listings</BASEFONT>", false, false );
            AddHtml(
                ResultsPanelX + ResultsPanelWidth - 250,
                ResultsPanelY + 7,
                250,
                24,
                String.Format(
                    CultureInfo.InvariantCulture,
                    "<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFFFFF>Page {0} of {1}</BASEFONT></DIV>",
                    m_Session.VisibleResultCount == 0 ? 0 : m_Query.Page + 1,
                    pageCount ),
                false,
                false );

            int rowStartY = 176;
            List<Serial> staleOnPage = new List<Serial>();
            int drawn = 0;
            m_RowItemSerials = m_Session.CapturePage(
                m_Query.Page,
                VendorSearchLimits.ResultsPerPage );

            for ( int row = 0; row < m_RowItemSerials.Length; row++ )
            {
                Serial itemSerial = m_RowItemSerials[row];

                VendorSearchLiveResult live;
                string message;

                if ( !VendorSearchValidation.TryResolve( m_From, itemSerial, out live, out message ) )
                {
                    staleOnPage.Add( itemSerial );
                    continue;
                }

                DrawResultRow( live, row, rowStartY + ( row * ResultRowHeight ) );
                drawn++;
            }

            for ( int i = 0; i < staleOnPage.Count; i++ )
                VendorSearchSessionManager.RemoveResult( m_From, m_SessionId, staleOnPage[i] );

            if ( drawn == 0 )
            {
                AddHtml(
                    ResultsPanelX,
                    rowStartY + 20,
                    ResultsPanelWidth,
                    40,
                    "<CENTER><BASEFONT COLOR=#FFFFFF>No available listings matched these filters.</BASEFONT></CENTER>",
                    false,
                    false );
            }

            int pagerY = 805;

            if ( m_Query.Page > 0 )
            {
                AddButton( ResultsPanelX, pagerY, 4014, 4015, PreviousButtonID, GumpButtonType.Reply, 0 );
                AddHtml( ResultsPanelX + 34, pagerY + 3, 90, 20, "<BASEFONT COLOR=#FFFFFF>Previous</BASEFONT>", false, false );
            }

            if ( m_Query.Page < pageCount - 1 )
            {
                AddButton( ResultsPanelX + 150, pagerY, 4005, 4007, NextButtonID, GumpButtonType.Reply, 0 );
                AddHtml( ResultsPanelX + 184, pagerY + 3, 70, 20, "<BASEFONT COLOR=#FFFFFF>Next</BASEFONT>", false, false );
            }

            if ( m_Session.HasMore )
            {
                AddButton( ResultsPanelX + 300, pagerY, 4005, 4007, LoadMoreButtonID, GumpButtonType.Reply, 0 );
                AddHtml(
                    ResultsPanelX + 334,
                    pagerY + 3,
                    210,
                    20,
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "<BASEFONT COLOR=#FFFFFF>Load {0:N0} More</BASEFONT>",
                        VendorSearchLimits.LoadMoreIncrement ),
                    false,
                    false );
            }
        }

        private void DrawResultRow( VendorSearchLiveResult live, int row, int y )
        {
            int rowHeight = ResultRowHeight - 8;
            AddImageTiled( ResultsPanelX, y, ResultsPanelWidth, rowHeight, 2624 );
            AddAlphaRegion( ResultsPanelX, y, ResultsPanelWidth, rowHeight );

            int artX = ResultsPanelX + 12;
            int artY = y + 13;
            AddCenteredItem( live.Item, artX, artY, ItemSquareSize );

            string itemName = VendorSearchCatalog.ResolveItemName( live.Item );
            string vendorName = String.IsNullOrEmpty( live.Vendor.Name ) ? live.Vendor.RawName : live.Vendor.Name;
            string shopName = live.Vendor.ShopName ?? String.Empty;
            string vendorLine = String.IsNullOrEmpty( shopName )
                ? vendorName
                : vendorName + " - " + shopName;
            string regionName = live.Record.RegionName ?? String.Empty;
            string facetName = live.Record.FacetName ?? String.Empty;
            string description = live.Record.Description ?? String.Empty;
            int textX = artX + ItemSquareSize + 12;
            int rightX = ResultsPanelX + ResultsPanelWidth - 198;

            AddHtml(
                textX,
                y + 8,
                475,
                22,
                "<BASEFONT COLOR=#C5FFAE>" + Utility.FixHtml( itemName ) + "</BASEFONT>",
                false,
                false );
            AddHtml(
                textX,
                y + 30,
                475,
                20,
                "<BASEFONT COLOR=#FFFFFF>Vendor: " + Utility.FixHtml( vendorLine ) + "</BASEFONT>",
                false,
                false );
            AddHtml(
                textX,
                y + 51,
                475,
                20,
                String.Format(
                    CultureInfo.InvariantCulture,
                    "<BASEFONT COLOR=#F7D774>Price: {0:N0} gold</BASEFONT>",
                    live.Price ),
                false,
                false );
            AddHtml(
                textX,
                y + 72,
                475,
                20,
                String.Format(
                    CultureInfo.InvariantCulture,
                    "<BASEFONT COLOR=#9DC6FF>{0} [{1}] at {2}, {3}, {4}</BASEFONT>",
                    Utility.FixHtml( regionName ),
                    Utility.FixHtml( facetName ),
                    live.Vendor.X,
                    live.Vendor.Y,
                    live.Vendor.Z ),
                false,
                false );

            if ( !String.IsNullOrEmpty( description ) )
            {
                AddHtml(
                    textX,
                    y + 92,
                    475,
                    20,
                    "<BASEFONT COLOR=#D8D8D8>" + Utility.FixHtml( description ) + "</BASEFONT>",
                    false,
                    false );
            }

            AddHtml(
                rightX,
                y + 12,
                185,
                20,
                "<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFFFFF>Listed "
                    + live.Record.Created.ToString( "yyyy-MM-dd", CultureInfo.InvariantCulture )
                    + "</BASEFONT></DIV>",
                false,
                false );

            if ( live.Item.Stackable && live.Amount > 1 )
            {
                AddHtml(
                    rightX,
                    y + 35,
                    185,
                    20,
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFFFFF>Amount: {0:N0}</BASEFONT></DIV>",
                        live.Amount ),
                    false,
                    false );
                AddHtml(
                    rightX,
                    y + 56,
                    185,
                    20,
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFFFFF>{0:N2} each</BASEFONT></DIV>",
                        live.PricePerItem ),
                    false,
                    false );
            }

            AddButton(
                ResultsPanelX + ResultsPanelWidth - 96,
                y + 82,
                4005,
                4007,
                MapButtonBase + row,
                GumpButtonType.Reply,
                0 );
            AddHtml(
                ResultsPanelX + ResultsPanelWidth - 60,
                y + 86,
                50,
                20,
                "<BASEFONT COLOR=#FFFFFF>Map</BASEFONT>",
                false,
                false );
        }

        private void AddCenteredItem( Item item, int x, int y, int size )
        {
            AddImageTiled( x, y, size, size, 9304 );

            int itemID = item.ItemID;
            int maskedID = itemID & TileData.MaxItemValue;
            Rectangle2D bounds = new Rectangle2D( 0, 0, 44, 44 );

            if ( ItemBounds.Table != null && maskedID >= 0 && maskedID < ItemBounds.Table.Length )
                bounds = ItemBounds.Table[maskedID];

            int width = bounds.Width > 0 ? bounds.Width : 44;
            int height = bounds.Height > 0 ? bounds.Height : 44;
            int itemX = x + ( size / 2 ) - ( width / 2 ) - bounds.X;
            int itemY = y + ( size / 2 ) - ( height / 2 ) - bounds.Y;

            itemX = Math.Max( x - bounds.X, Math.Min( itemX, x + size - width - bounds.X ) );
            itemY = Math.Max( y - bounds.Y, Math.Min( itemY, y + size - height - bounds.Y ) );

            AddItem( itemX, itemY, itemID, item.Hue );
            AddItemProperty( item.Serial );
            item.SendPropertiesTo( m_From );
        }

        public override void OnResponse( NetState sender, RelayInfo info )
        {
            VendorSearchSession session;

            if ( m_From == null
                || sender == null
                || sender.Mobile != m_From
                || info == null
                || !VendorSearchSessionManager.TryGet( m_From, m_SessionId, out session ) )
                return;

            int buttonID = info.ButtonID;

            if ( buttonID == 0 )
                return;

            if ( buttonID >= MapButtonBase
                && buttonID < MapButtonBase + VendorSearchLimits.ResultsPerPage )
            {
                HandleMapButton( session, buttonID - MapButtonBase );
                return;
            }

            switch ( buttonID )
            {
                case PreviousButtonID:
                    session.Query.Page = Math.Max( 0, session.Query.Page - 1 );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case NextButtonID:
                    session.Query.Page = Math.Min(
                        GetPageCount( session.VisibleResultCount ) - 1,
                        session.Query.Page + 1 );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case LoadMoreButtonID:
                    HandleLoadMore( session );
                    return;
            }

            VendorSearchQuery query;
            string error;

            if ( !TryBuildQueryFromEntries( info, session.Query, out query, out error ) )
            {
                if ( !String.IsNullOrEmpty( error ) )
                    m_From.SendMessage( error );

                VendorSearchSystem.OpenSession( m_From, session.Id );
                return;
            }

            if ( buttonID >= CategoryButtonBase
                && buttonID < CategoryButtonBase + VendorSearchCatalog.Categories.Count
                && !query.ShowAdvanced )
            {
                VendorSearchCategoryDefinition category =
                    VendorSearchCatalog.Categories[buttonID - CategoryButtonBase];
                query.CategoryKey = category.Key;
                query.SubcategoryKey = category.Subcategories[0].Key;
                query.Page = 0;
                VendorSearchSystem.OpenNewSearch( m_From, query );
                return;
            }

            VendorSearchCategoryDefinition current = VendorSearchCatalog.FindCategory( query.CategoryKey );

            if ( current != null
                && buttonID >= SubcategoryButtonBase
                && buttonID < SubcategoryButtonBase + current.Subcategories.Count
                && !query.ShowAdvanced )
            {
                query.SubcategoryKey = current.Subcategories[buttonID - SubcategoryButtonBase].Key;
                query.Page = 0;
                VendorSearchSystem.OpenNewSearch( m_From, query );
                return;
            }

            switch ( buttonID )
            {
                case SearchButtonID:
                    query.Page = 0;
                    query.ShowAdvanced = false;
                    VendorSearchSystem.OpenNewSearch( m_From, query );
                    return;
                case SortButtonID:
                    query.Sort = NextSort( query.Sort );
                    query.Page = 0;

                    if ( query.ShowAdvanced )
                    {
                        ApplyDraftQuery( query, session.Query );
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    VendorSearchSystem.OpenNewSearch( m_From, query );
                    return;
                case AdvancedButtonID:
                    query.ShowAdvanced = !query.ShowAdvanced;
                    query.Page = 0;

                    if ( query.ShowAdvanced )
                    {
                        ApplyDraftQuery( query, session.Query );
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                    }
                    else
                    {
                        VendorSearchSystem.OpenNewSearch( m_From, query );
                    }

                    return;
                case TextModeButtonID:
                    if ( !query.ShowAdvanced )
                    {
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    query.TextMode = query.TextMode == VendorSearchTextMode.Exact
                        ? VendorSearchTextMode.Contains
                        : VendorSearchTextMode.Exact;
                    query.Page = 0;
                    ApplyDraftQuery( query, session.Query );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case ZeroPriceButtonID:
                    if ( !query.ShowAdvanced )
                    {
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    query.HideZeroPrice = !query.HideZeroPrice;
                    query.Page = 0;
                    ApplyDraftQuery( query, session.Query );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case MapFilterButtonID:
                    if ( !query.ShowAdvanced )
                    {
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    query.MapID = GetNextMapID( query.MapID );
                    query.Page = 0;
                    ApplyDraftQuery( query, session.Query );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case AdvancedCategoryButtonID:
                    if ( !query.ShowAdvanced )
                    {
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    CycleAdvancedCategory( query );
                    ApplyDraftQuery( query, session.Query );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case AdvancedSubcategoryButtonID:
                    if ( !query.ShowAdvanced )
                    {
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    CycleAdvancedSubcategory( query );
                    ApplyDraftQuery( query, session.Query );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case ClearStatsButtonID:
                    if ( !query.ShowAdvanced )
                    {
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    query.PropertyFilters = new List<VendorSearchPropertyFilter>();
                    ApplyDraftQuery( query, session.Query );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case GeneralStatsButtonID:
                    if ( !query.ShowAdvanced )
                    {
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    query.AdvancedPage = VendorSearchAdvancedPage.General;
                    ApplyDraftQuery( query, session.Query );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case WeaponEffectsButtonID:
                    if ( !query.ShowAdvanced )
                    {
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    query.AdvancedPage = VendorSearchAdvancedPage.WeaponEffects;
                    ApplyDraftQuery( query, session.Query );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case SpellChannelingButtonID:
                    if ( !query.ShowAdvanced )
                    {
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    ToggleRequiredProperty( query, VendorSearchPropertyKeys.SpellChanneling );
                    ApplyDraftQuery( query, session.Query );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case MageArmorButtonID:
                    if ( !query.ShowAdvanced )
                    {
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    ToggleRequiredProperty( query, VendorSearchPropertyKeys.MageArmor );
                    ApplyDraftQuery( query, session.Query );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case ExceptionalButtonID:
                    if ( !query.ShowAdvanced )
                    {
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    ToggleRequiredProperty( query, VendorSearchPropertyKeys.Exceptional );
                    ApplyDraftQuery( query, session.Query );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                case IdentifiedButtonID:
                    if ( !query.ShowAdvanced )
                    {
                        VendorSearchSystem.OpenSession( m_From, session.Id );
                        return;
                    }

                    ToggleRequiredProperty( query, VendorSearchPropertyKeys.Identified );
                    ApplyDraftQuery( query, session.Query );
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
                default:
                    VendorSearchSystem.OpenSession( m_From, session.Id );
                    return;
            }
        }

        private void HandleMapButton( VendorSearchSession session, int row )
        {
            Serial itemSerial = row >= 0 && row < m_RowItemSerials.Length
                ? m_RowItemSerials[row]
                : Server.Serial.Zero;
            string message;

            if ( itemSerial == Server.Serial.Zero )
            {
                VendorSearchSystem.OpenSession( m_From, session.Id );
                return;
            }

            if ( !VendorSearchSessionManager.TryBeginMap( m_From, out message ) )
            {
                if ( !String.IsNullOrEmpty( message ) )
                    m_From.SendMessage( message );

                VendorSearchSystem.OpenSession( m_From, session.Id );
                return;
            }

            if ( !VendorSearchValidation.TryCreateMap( m_From, itemSerial, out message ) )
                VendorSearchSessionManager.RemoveResult( m_From, session.Id, itemSerial );

            if ( !String.IsNullOrEmpty( message ) )
                m_From.SendMessage( message );

            VendorSearchSystem.OpenSession( m_From, session.Id );
        }

        private void HandleLoadMore( VendorSearchSession session )
        {
            string message;

            if ( !VendorSearchSessionManager.TryLoadMore( m_From, session.Id, out message )
                && !String.IsNullOrEmpty( message ) )
                m_From.SendMessage( message );

            VendorSearchSystem.OpenSession( m_From, session.Id );
        }

        private bool TryBuildQueryFromEntries( RelayInfo info, VendorSearchQuery source, out VendorSearchQuery query, out string error )
        {
            query = source == null ? new VendorSearchQuery() : source.Clone();
            error = String.Empty;
            query.SearchTerm = GetText( info, SearchEntryID );

            if ( !query.ShowAdvanced )
            {
                query.Page = 0;
                VendorSearchCatalog.NormalizeQuery( query );
                return true;
            }

            query.VendorName = GetText( info, VendorEntryID );
            query.RegionTerm = GetText( info, RegionEntryID );

            int? minPrice;

            if ( !TryParseInt( info, MinPriceEntryID, out minPrice ) )
            {
                error = "Minimum price must be a non-negative whole number.";
                return false;
            }

            int? maxPrice;

            if ( !TryParseInt( info, MaxPriceEntryID, out maxPrice ) )
            {
                error = "Maximum price must be a non-negative whole number.";
                return false;
            }

            double? minPricePerItem;

            if ( !TryParseDouble( info, MinPerItemEntryID, out minPricePerItem ) )
            {
                error = "Minimum per-item price must be a non-negative number.";
                return false;
            }

            double? maxPricePerItem;

            if ( !TryParseDouble( info, MaxPerItemEntryID, out maxPricePerItem ) )
            {
                error = "Maximum per-item price must be a non-negative number.";
                return false;
            }

            int? minStackAmount;

            if ( !TryParseInt( info, MinStackEntryID, out minStackAmount ) )
            {
                error = "Minimum stack amount must be a non-negative whole number.";
                return false;
            }

            DateTime? createdAfter;

            if ( !TryParseDate( info, CreatedAfterEntryID, out createdAfter ) )
            {
                error = "Listed-after date must use yyyy-MM-dd.";
                return false;
            }

            List<VendorSearchPropertyFilter> propertyFilters;

            if ( !TryBuildPropertyFilters( info, query, out propertyFilters, out error ) )
                return false;

            query.MinPrice = minPrice;
            query.MaxPrice = maxPrice;
            query.MinPricePerItem = minPricePerItem;
            query.MaxPricePerItem = maxPricePerItem;
            query.MinStackAmount = minStackAmount;
            query.CreatedAfter = createdAfter;
            query.PropertyFilters = propertyFilters;

            if ( query.MinPrice.HasValue
                && query.MaxPrice.HasValue
                && query.MinPrice.Value > query.MaxPrice.Value )
            {
                error = "Minimum price cannot be greater than maximum price.";
                return false;
            }

            if ( query.MinPricePerItem.HasValue
                && query.MaxPricePerItem.HasValue
                && query.MinPricePerItem.Value > query.MaxPricePerItem.Value )
            {
                error = "Minimum per-item price cannot be greater than maximum per-item price.";
                return false;
            }

            query.Page = 0;
            VendorSearchCatalog.NormalizeQuery( query );
            return true;
        }

        private static bool TryBuildPropertyFilters(
            RelayInfo info,
            VendorSearchQuery source,
            out List<VendorSearchPropertyFilter> filters,
            out string error )
        {
            filters = new List<VendorSearchPropertyFilter>();
            error = String.Empty;
            PropertyEntryDefinition[] entries = GetPropertyEntries( source.AdvancedPage );

            PreserveUneditedPropertyFilters( source, filters, entries );

            string material = LimitFilterText( GetText( info, MaterialEntryID ) );

            if ( material.Length > 0 )
                AddPropertyFilter( filters, VendorSearchPropertyKeys.Material, material );

            string slayer = LimitFilterText( GetText( info, SlayerEntryID ) );

            if ( slayer.Length > 0 )
                AddPropertyFilter( filters, VendorSearchPropertyKeys.Slayer, slayer );

            string enhancePotionsText = GetText( info, EnhancePotionsEntryID );

            if ( enhancePotionsText.Length > 0 )
            {
                int enhancePotions;

                if ( !Int32.TryParse(
                    enhancePotionsText,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out enhancePotions )
                    || enhancePotions < 0 )
                {
                    error = "Enhance potions must be a non-negative whole number.";
                    return false;
                }

                AddPropertyFilter(
                    filters,
                    VendorSearchPropertyKeys.EnhancePotions,
                    enhancePotions.ToString( CultureInfo.InvariantCulture ) );
            }

            for ( int i = 0; i < entries.Length; i++ )
            {
                string text = GetText( info, entries[i].EntryID );

                if ( text.Length == 0 )
                    continue;

                int value;

                if ( !Int32.TryParse(
                    text,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value )
                    || value < 0 )
                {
                    error = entries[i].Label + " must be a non-negative whole number.";
                    return false;
                }

                AddPropertyFilter(
                    filters,
                    entries[i].Key,
                    value.ToString( CultureInfo.InvariantCulture ) );
            }

            PreserveRequiredProperty( source, filters, VendorSearchPropertyKeys.SpellChanneling );
            PreserveRequiredProperty( source, filters, VendorSearchPropertyKeys.MageArmor );
            PreserveRequiredProperty( source, filters, VendorSearchPropertyKeys.Exceptional );
            PreserveRequiredProperty( source, filters, VendorSearchPropertyKeys.Identified );
            return true;
        }

        private static void PreserveUneditedPropertyFilters(
            VendorSearchQuery source,
            List<VendorSearchPropertyFilter> filters,
            PropertyEntryDefinition[] editedEntries )
        {
            if ( source == null || source.PropertyFilters == null || filters == null )
                return;

            for ( int i = 0; i < source.PropertyFilters.Count; i++ )
            {
                VendorSearchPropertyFilter filter = source.PropertyFilters[i];

                if ( filter == null
                    || IsCommonPropertyKey( filter.Key )
                    || ContainsPropertyKey( editedEntries, filter.Key ) )
                    continue;

                AddPropertyFilter( filters, filter.Key, filter.Value );
            }
        }

        private static bool IsCommonPropertyKey( string key )
        {
            return String.Equals( key, VendorSearchPropertyKeys.Material, StringComparison.OrdinalIgnoreCase )
                || String.Equals( key, VendorSearchPropertyKeys.Slayer, StringComparison.OrdinalIgnoreCase )
                || String.Equals( key, VendorSearchPropertyKeys.EnhancePotions, StringComparison.OrdinalIgnoreCase )
                || String.Equals( key, VendorSearchPropertyKeys.SpellChanneling, StringComparison.OrdinalIgnoreCase )
                || String.Equals( key, VendorSearchPropertyKeys.MageArmor, StringComparison.OrdinalIgnoreCase )
                || String.Equals( key, VendorSearchPropertyKeys.Exceptional, StringComparison.OrdinalIgnoreCase )
                || String.Equals( key, VendorSearchPropertyKeys.Identified, StringComparison.OrdinalIgnoreCase );
        }

        private static bool ContainsPropertyKey(
            PropertyEntryDefinition[] entries,
            string key )
        {
            if ( entries == null || String.IsNullOrEmpty( key ) )
                return false;

            for ( int i = 0; i < entries.Length; i++ )
            {
                if ( String.Equals( entries[i].Key, key, StringComparison.OrdinalIgnoreCase ) )
                    return true;
            }

            return false;
        }

        private static bool TryParseInt( RelayInfo info, int entryID, out int? value )
        {
            string text = GetText( info, entryID );

            if ( text.Length == 0 )
            {
                value = null;
                return true;
            }

            int parsed;

            if ( Int32.TryParse( text, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed )
                && parsed >= 0 )
            {
                value = parsed;
                return true;
            }

            value = null;
            return false;
        }

        private static bool TryParseDouble( RelayInfo info, int entryID, out double? value )
        {
            string text = GetText( info, entryID );

            if ( text.Length == 0 )
            {
                value = null;
                return true;
            }

            double parsed;

            if ( Double.TryParse( text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed )
                && !Double.IsNaN( parsed )
                && !Double.IsInfinity( parsed )
                && parsed >= 0.0 )
            {
                value = parsed;
                return true;
            }

            value = null;
            return false;
        }

        private static bool TryParseDate( RelayInfo info, int entryID, out DateTime? value )
        {
            string text = GetText( info, entryID );

            if ( text.Length == 0 )
            {
                value = null;
                return true;
            }

            DateTime parsed;

            if ( DateTime.TryParseExact(
                text,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsed ) )
            {
                value = parsed;
                return true;
            }

            value = null;
            return false;
        }

        private static string GetText( RelayInfo info, int entryID )
        {
            TextRelay relay = info.GetTextEntry( entryID );

            if ( relay == null || relay.Text == null )
                return String.Empty;

            return relay.Text.Trim();
        }

        private static string LimitFilterText( string value )
        {
            if ( String.IsNullOrEmpty( value ) )
                return String.Empty;

            string trimmed = value.Trim();

            return trimmed.Length <= VendorSearchLimits.MaximumFilterTextLength
                ? trimmed
                : trimmed.Substring( 0, VendorSearchLimits.MaximumFilterTextLength );
        }

        private void AddField( string label, int entryID, string value, int x, int y, int width )
        {
            AddHtml(
                x,
                y,
                width,
                20,
                "<BASEFONT COLOR=#FFFFFF>" + Utility.FixHtml( label ) + "</BASEFONT>",
                false,
                false );
            AddEntryBackground( x, y + 20, width, 24 );
            AddTextEntry( x + 4, y + 22, width - 8, 20, 0x34, entryID, value ?? String.Empty );
        }

        private void AddCompactField( string label, int entryID, string value, int x, int y, int width )
        {
            AddCompactField( label, entryID, value, x, y, width, 78 );
        }

        private void AddCompactField(
            string label,
            int entryID,
            string value,
            int x,
            int y,
            int width,
            int entryWidth )
        {
            entryWidth = Math.Max( 50, Math.Min( entryWidth, width - 60 ) );

            AddHtml(
                x,
                y + 3,
                width - entryWidth - 8,
                20,
                "<BASEFONT COLOR=#FFFFFF>" + Utility.FixHtml( label ) + "</BASEFONT>",
                false,
                false );
            AddEntryBackground( x + width - entryWidth, y, entryWidth, 24 );
            AddTextEntry(
                x + width - entryWidth + 4,
                y + 2,
                entryWidth - 8,
                20,
                0x34,
                entryID,
                value ?? String.Empty );
        }

        private void AddAdvancedChoice( int x, int y, string label, int buttonID, string value )
        {
            AddHtml(
                x,
                y + 3,
                105,
                20,
                "<BASEFONT COLOR=#FFFFFF>" + Utility.FixHtml( label ) + "</BASEFONT>",
                false,
                false );
            AddButton( x + 108, y, 4005, 4007, buttonID, GumpButtonType.Reply, 0 );
            AddHtml(
                x + 144,
                y + 3,
                140,
                20,
                "<BASEFONT COLOR=#C5FFAE>" + Utility.FixHtml( value ) + "</BASEFONT>",
                false,
                false );
        }

        private void AddAdvancedToggle( int x, int y, int buttonID, string label, string value )
        {
            AddButton( x, y, 4005, 4007, buttonID, GumpButtonType.Reply, 0 );
            AddHtml(
                x + 36,
                y + 3,
                245,
                20,
                "<BASEFONT COLOR=#FFFFFF>"
                    + Utility.FixHtml( label )
                    + ": <BASEFONT COLOR=#C5FFAE>"
                    + Utility.FixHtml( value )
                    + "</BASEFONT>",
                false,
                false );
        }

        private void AddRequiredPropertyToggle( int x, int y, int buttonID, string label, string key )
        {
            bool required = HasPropertyFilter( m_Query, key );

            AddButton(
                x,
                y,
                required ? 4006 : 4005,
                required ? 4006 : 4007,
                buttonID,
                GumpButtonType.Reply,
                0 );
            AddHtml(
                x + 36,
                y + 3,
                200,
                20,
                String.Format(
                    "<BASEFONT COLOR={0}>{1}: {2}</BASEFONT>",
                    required ? "#C5FFAE" : "#FFFFFF",
                    Utility.FixHtml( label ),
                    required ? "Required" : "Any" ),
                false,
                false );
        }

        private void AddEntryBackground( int x, int y, int width, int height )
        {
            AddImageTiled( x, y, width, height, 9354 );
        }

        private static int GetActivePropertyFilterCount( VendorSearchQuery query )
        {
            return query == null || query.PropertyFilters == null
                ? 0
                : query.PropertyFilters.Count;
        }

        private static PropertyEntryDefinition[] GetPropertyEntries(
            VendorSearchAdvancedPage page )
        {
            return page == VendorSearchAdvancedPage.WeaponEffects
                ? m_WeaponPropertyEntries
                : m_GeneralPropertyEntries;
        }

        private static string GetPropertyFilterValue( VendorSearchQuery query, string key )
        {
            if ( query == null || query.PropertyFilters == null || String.IsNullOrEmpty( key ) )
                return String.Empty;

            for ( int i = 0; i < query.PropertyFilters.Count; i++ )
            {
                VendorSearchPropertyFilter filter = query.PropertyFilters[i];

                if ( filter != null
                    && String.Equals( filter.Key, key, StringComparison.OrdinalIgnoreCase ) )
                    return filter.Value ?? String.Empty;
            }

            return String.Empty;
        }

        private static bool HasPropertyFilter( VendorSearchQuery query, string key )
        {
            return GetPropertyFilterValue( query, key ).Length > 0;
        }

        private static void AddPropertyFilter(
            List<VendorSearchPropertyFilter> filters,
            string key,
            string value )
        {
            if ( filters == null || String.IsNullOrEmpty( key ) || String.IsNullOrEmpty( value ) )
                return;

            filters.Add( new VendorSearchPropertyFilter { Key = key, Value = value } );
        }

        private static void PreserveRequiredProperty(
            VendorSearchQuery source,
            List<VendorSearchPropertyFilter> filters,
            string key )
        {
            if ( HasPropertyFilter( source, key ) )
                AddPropertyFilter( filters, key, "1" );
        }

        private static void ToggleRequiredProperty( VendorSearchQuery query, string key )
        {
            if ( query == null || String.IsNullOrEmpty( key ) )
                return;

            if ( query.PropertyFilters == null )
                query.PropertyFilters = new List<VendorSearchPropertyFilter>();

            for ( int i = query.PropertyFilters.Count - 1; i >= 0; i-- )
            {
                VendorSearchPropertyFilter filter = query.PropertyFilters[i];

                if ( filter != null
                    && String.Equals( filter.Key, key, StringComparison.OrdinalIgnoreCase ) )
                {
                    query.PropertyFilters.RemoveAt( i );
                    return;
                }
            }

            AddPropertyFilter( query.PropertyFilters, key, "1" );
        }

        private static void ApplyDraftQuery( VendorSearchQuery source, VendorSearchQuery target )
        {
            if ( source == null || target == null )
                return;

            target.SearchTerm = source.SearchTerm;
            target.VendorName = source.VendorName;
            target.RegionTerm = source.RegionTerm;
            target.MapID = source.MapID;
            target.CategoryKey = source.CategoryKey;
            target.SubcategoryKey = source.SubcategoryKey;
            target.MinPrice = source.MinPrice;
            target.MaxPrice = source.MaxPrice;
            target.MinPricePerItem = source.MinPricePerItem;
            target.MaxPricePerItem = source.MaxPricePerItem;
            target.MinStackAmount = source.MinStackAmount;
            target.CreatedAfter = source.CreatedAfter;
            target.HideZeroPrice = source.HideZeroPrice;
            target.ShowAdvanced = source.ShowAdvanced;
            target.AdvancedPage = source.AdvancedPage;
            target.TextMode = source.TextMode;
            target.Sort = source.Sort;
            target.Page = source.Page;
            target.PropertyFilters = ClonePropertyFilters( source.PropertyFilters );
        }

        private static List<VendorSearchPropertyFilter> ClonePropertyFilters(
            List<VendorSearchPropertyFilter> source )
        {
            if ( source == null )
                return null;

            List<VendorSearchPropertyFilter> clone =
                new List<VendorSearchPropertyFilter>( source.Count );

            for ( int i = 0; i < source.Count; i++ )
            {
                VendorSearchPropertyFilter filter = source[i];

                if ( filter == null )
                    continue;

                clone.Add(
                    new VendorSearchPropertyFilter
                    {
                        Key = filter.Key,
                        Value = filter.Value
                    } );
            }

            return clone;
        }

        private static int GetPageCount( int visibleCount )
        {
            if ( visibleCount <= 0 )
                return 1;

            return ( visibleCount + VendorSearchLimits.ResultsPerPage - 1 )
                / VendorSearchLimits.ResultsPerPage;
        }

        private static VendorSearchSort NextSort( VendorSearchSort sort )
        {
            int value = (int)sort + 1;

            if ( value > (int)VendorSearchSort.StackAmount )
                value = (int)VendorSearchSort.PriceLowToHigh;

            return (VendorSearchSort)value;
        }

        private static string GetSortLabel( VendorSearchSort sort )
        {
            switch ( sort )
            {
                case VendorSearchSort.PriceHighToLow:
                    return "Price High-Low";
                case VendorSearchSort.NewestListed:
                    return "Newest Listed";
                case VendorSearchSort.OldestListed:
                    return "Oldest Listed";
                case VendorSearchSort.VendorName:
                    return "Vendor Name";
                case VendorSearchSort.ItemName:
                    return "Item Name A-Z";
                case VendorSearchSort.StackAmount:
                    return "Stack Amount";
                default:
                    return "Price Low-High";
            }
        }

        private static string FormatNullable( int? value )
        {
            return value.HasValue
                ? value.Value.ToString( CultureInfo.InvariantCulture )
                : String.Empty;
        }

        private static string FormatNullable( double? value )
        {
            return value.HasValue
                ? value.Value.ToString( "0.##", CultureInfo.InvariantCulture )
                : String.Empty;
        }

        private static void CycleAdvancedCategory( VendorSearchQuery query )
        {
            if ( query == null || VendorSearchCatalog.Categories.Count == 0 )
                return;

            int currentIndex = 0;

            for ( int i = 0; i < VendorSearchCatalog.Categories.Count; i++ )
            {
                if ( String.Equals(
                    VendorSearchCatalog.Categories[i].Key,
                    query.CategoryKey,
                    StringComparison.OrdinalIgnoreCase ) )
                {
                    currentIndex = i;
                    break;
                }
            }

            VendorSearchCategoryDefinition next =
                VendorSearchCatalog.Categories[( currentIndex + 1 ) % VendorSearchCatalog.Categories.Count];
            query.CategoryKey = next.Key;
            query.SubcategoryKey = next.Subcategories.Count == 0
                ? "all"
                : next.Subcategories[0].Key;
            query.Page = 0;
        }

        private static void CycleAdvancedSubcategory( VendorSearchQuery query )
        {
            if ( query == null )
                return;

            VendorSearchCategoryDefinition category =
                VendorSearchCatalog.FindCategory( query.CategoryKey );

            if ( category == null || category.Subcategories.Count == 0 )
                return;

            int currentIndex = 0;

            for ( int i = 0; i < category.Subcategories.Count; i++ )
            {
                if ( String.Equals(
                    category.Subcategories[i].Key,
                    query.SubcategoryKey,
                    StringComparison.OrdinalIgnoreCase ) )
                {
                    currentIndex = i;
                    break;
                }
            }

            query.SubcategoryKey =
                category.Subcategories[( currentIndex + 1 ) % category.Subcategories.Count].Key;
            query.Page = 0;
        }

        private static string GetCategoryLabel( string categoryKey )
        {
            VendorSearchCategoryDefinition category =
                VendorSearchCatalog.FindCategory( categoryKey );

            return category == null ? "All" : category.Name;
        }

        private static string GetSubcategoryLabel( string categoryKey, string subcategoryKey )
        {
            VendorSearchSubcategoryDefinition subcategory =
                VendorSearchCatalog.FindSubcategory( categoryKey, subcategoryKey );

            return subcategory == null ? "All" : subcategory.Name;
        }

        private static int? GetNextMapID( int? currentMapID )
        {
            int currentIndex = GetMapChoiceIndex( currentMapID );

            for ( int offset = 1; offset <= 7; offset++ )
            {
                int nextIndex = ( currentIndex + offset ) % 7;
                Map map = GetMapChoice( nextIndex );

                if ( nextIndex == 0 )
                    return null;

                if ( map != null )
                    return map.MapID;
            }

            return null;
        }

        private static string GetMapLabel( int? mapID )
        {
            int index = GetMapChoiceIndex( mapID );

            switch ( index )
            {
                case 1: return "Trammel";
                case 2: return "Felucca";
                case 3: return "Ilshenar";
                case 4: return "Malas";
                case 5: return "Tokuno";
                case 6: return "Ter Mur";
                default: return "Any";
            }
        }

        private static int GetMapChoiceIndex( int? mapID )
        {
            if ( !mapID.HasValue )
                return 0;

            for ( int i = 1; i < 7; i++ )
            {
                Map map = GetMapChoice( i );

                if ( map != null && map.MapID == mapID.Value )
                    return i;
            }

            return 0;
        }

        private static Map GetMapChoice( int index )
        {
            switch ( index )
            {
                case 1: return Map.Trammel;
                case 2: return Map.Felucca;
                case 3: return Map.Ilshenar;
                case 4: return Map.Malas;
                case 5: return Map.Tokuno;
                case 6: return Map.TerMur;
                default: return null;
            }
        }
    }
}
