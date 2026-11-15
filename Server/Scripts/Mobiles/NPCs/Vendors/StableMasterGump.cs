using System;
using System.Collections.Generic;
using System.Text;
using Custom.Jerbal.Jako;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
    public class StableMasterGump : Gump
    {
        private const int PetsPerPage = 3;
        private const int PurchaseButtonID = 1;
        private const int PrevPageButtonID = 2;
        private const int NextPageButtonID = 3;
        private const int SettingsButtonID = 4;
        private const int SearchButtonID = 5;
        private const int FavoritesButtonID = 6;
        private const int CustomListsButtonID = 7;
        private const int SearchTextEntryID = 1;
        private const int PetButtonBase = 1000;
        private const int FavoritePetButtonBase = 2000;
        private const int AddToListButtonBase = 3000;
        private const int ComparePetButtonBase = 6000;

        private const double DefaultScale = 1.0;
        private const double CompactScale = 0.75;

        private const string DefaultStatLabelColor = "#C5FFAE";
        private const string DefaultStatValueColor = "#FFFFFF";
        private const string PhysicalColor = "#C5C5C5";
        private const string FireColor = "#FF6B3A";
        private const string ColdColor = "#66C7FF";
        private const string PoisonColor = "#7FEA7F";
        private const string EnergyColor = "#C592FF";

        private readonly Mobile m_From;
        private readonly IList<BaseCreature> m_List;
        private readonly int m_Page;
        private readonly StableMasterGumpOptions m_Options;
        private readonly string m_Title;
        private readonly StableStatDisplayFlags m_DisplayFlags;
        private readonly string m_SearchTerm;
        private readonly StableMasterLayout m_Layout;

        internal sealed class StableMasterLayout
        {
            private const int BaseBackgroundPadding = 20;
            private const int BaseHeaderToEntryOffset = 150 * 2;
            private const int BaseFooterPadding = 30 * 2;
            private const int BaseBackgroundWidth = 600 * 2;
            private const int BaseEntryX = 40;
            private const int BaseDetailTextOffset = 10;
            private const int BaseClaimReservedWidth = 90;
            private const int BaseDetailToPreviewGap = 13;
            private const int BasePetPreviewAreaWidth = 320;
            private const int BasePetPreviewInnerPadding = 0;
            private const int BaseClaimButtonWidth = 20;
            private const int BaseClaimButtonPadding = 10;
            private const int BaseClaimLabelOffset = 60;
            private const int BaseSettingsButtonXOffset = 220;
            private const int BaseSettingsButtonY = 32;
            private const int BaseSettingsLabelOffsetX = 38;
            private const int BaseSettingsLabelOffsetY = 2;
            private const int BaseSearchLabelX = 40;
            private const int BaseSearchLabelY = 98;
            private const int BaseSearchEntryOffsetX = 70;
            private const int BaseSearchEntryY = 96;
            private const int BaseSearchEntryWidth = 260;
            private const int BaseSearchEntryHeight = 20;
            private const int BaseSearchButtonOffsetX = 10;
            private const int BaseSearchButtonY = 96;
            private const int BaseSearchLabelButtonOffsetX = 38;
            private const int BaseSearchLabelButtonOffsetY = 2;
            private const int BaseFavoritesButtonXOffset = 360;
            private const int BaseCustomListsButtonXOffset = 220;
            private const int BaseTopListButtonY = 116;
            private const int BaseTopListLabelOffsetX = 38;
            private const int BaseTopListLabelOffsetY = 2;
            private const int BaseCompareButtonOffset = 100;
            private const int BaseFavoriteActionButtonXOffset = -110;
            private const int BaseAddListActionButtonXOffset = 0;
            private const int BaseActionLabelOffsetX = 38;
            private const int BaseDetailLineHeight = 13;
            private const int BaseDetailLineCount = 24; // expanded summary rows, bonding timers, stat lines, top/bottom padding buffers
            private const int BaseDetailTopPadding = 50;
            private const int BaseDetailBottomPadding = 24;
            private const int BaseEntrySpacingPadding = 10;
            private const int BasePetPreviewYOffset = 50;
            private const int BaseEntryTitleOffset = 10;
            private const int BaseEntryNameOffset = 30;
            private const int BaseEntryButtonLabelOffset = 12;
            private const int BaseExpansionX = 520;
            private const int BaseExpansionY = 46;
            private const int BaseExpansionWidth = 300;
            private const int BaseExpansionHeight = 40;
            private const int BaseExpansionButtonY = 86;
            private const int BaseExpansionLabelX = 558;
            private const int BaseExpansionLabelY = 88;
            private const int BaseSummaryX = 40;
            private const int BaseSummaryWidth = 500;
            private const int BaseSummaryHeight = 20;
            private const int BaseSummaryLineOneY = 58;
            private const int BaseSummaryLineTwoY = 78;
            private const int BaseTitleY = 32;
            private const int BaseTitleHeight = 40;
            private const int BasePageLabelX = 40;
            private const int BasePageLabelY = 118;
            private const int BasePrevPageButtonX = 210;
            private const int BaseNextPageButtonX = 270;
            private const int BasePageButtonY = 116;

            public StableMasterLayout( double scale )
            {
                Scale = scale;
                IsCompact = Math.Abs( scale - CompactScale ) < 0.0001;

                BackgroundPadding = ScaleValue( BaseBackgroundPadding );
                HeaderToEntryOffset = ScaleValue( BaseHeaderToEntryOffset );
                FooterPadding = ScaleValue( BaseFooterPadding );
                BackgroundWidth = ScaleValue( BaseBackgroundWidth );
                EntryX = ScaleValue( BaseEntryX );
                DetailTextOffset = ScaleValue( BaseDetailTextOffset );
                ClaimReservedWidth = ScaleValue( BaseClaimReservedWidth );
                DetailToPreviewGap = ScaleValue( BaseDetailToPreviewGap );
                PetPreviewAreaWidth = ScaleValue( BasePetPreviewAreaWidth );
                PetPreviewInnerPadding = ScaleValue( BasePetPreviewInnerPadding );
                ClaimButtonWidth = ScaleValue( BaseClaimButtonWidth );
                ClaimButtonPadding = ScaleValue( BaseClaimButtonPadding );
                ClaimLabelOffset = ScaleValue( BaseClaimLabelOffset );
                SettingsButtonX = BackgroundWidth - ScaleValue( BaseSettingsButtonXOffset );
                SettingsButtonY = ScaleValue( BaseSettingsButtonY );
                SettingsLabelX = SettingsButtonX + ScaleValue( BaseSettingsLabelOffsetX );
                SettingsLabelY = SettingsButtonY + ScaleValue( BaseSettingsLabelOffsetY );
                SearchLabelX = ScaleValue( BaseSearchLabelX );
                SearchLabelY = ScaleValue( BaseSearchLabelY );
                SearchEntryX = SearchLabelX + ScaleValue( BaseSearchEntryOffsetX );
                SearchEntryY = ScaleValue( BaseSearchEntryY );
                SearchEntryWidth = ScaleValue( BaseSearchEntryWidth );
                SearchEntryHeight = ScaleValue( BaseSearchEntryHeight );
                SearchButtonX = SearchEntryX + SearchEntryWidth + ScaleValue( BaseSearchButtonOffsetX );
                SearchButtonY = ScaleValue( BaseSearchButtonY );
                SearchLabelButtonX = SearchButtonX + ScaleValue( BaseSearchLabelButtonOffsetX );
                SearchLabelButtonY = SearchButtonY + ScaleValue( BaseSearchLabelButtonOffsetY );
                FavoritesButtonX = BackgroundWidth - ScaleValue( BaseFavoritesButtonXOffset );
                CustomListsButtonX = BackgroundWidth - ScaleValue( BaseCustomListsButtonXOffset );
                TopListButtonY = ScaleValue( BaseTopListButtonY );
                FavoritesLabelX = FavoritesButtonX + ScaleValue( BaseTopListLabelOffsetX );
                CustomListsLabelX = CustomListsButtonX + ScaleValue( BaseTopListLabelOffsetX );
                TopListLabelY = TopListButtonY + ScaleValue( BaseTopListLabelOffsetY );
                CompareButtonOffset = ScaleValue( BaseCompareButtonOffset );
                DetailLineHeight = ScaleValue( BaseDetailLineHeight );
                DetailLineCount = BaseDetailLineCount;
                DetailTopPadding = ScaleValue( BaseDetailTopPadding );
                DetailBottomPadding = ScaleValue( BaseDetailBottomPadding );
                EntrySpacingPadding = ScaleValue( BaseEntrySpacingPadding );
                PetPreviewYOff = ScaleValue( BasePetPreviewYOffset );
                EntryTitleOffset = ScaleValue( BaseEntryTitleOffset );
                EntryNameOffset = ScaleValue( BaseEntryNameOffset );
                EntryButtonLabelOffset = ScaleValue( BaseEntryButtonLabelOffset );
                ExpansionX = ScaleValue( BaseExpansionX );
                ExpansionY = ScaleValue( BaseExpansionY );
                ExpansionWidth = ScaleValue( BaseExpansionWidth );
                ExpansionHeight = ScaleValue( BaseExpansionHeight );
                ExpansionButtonY = ScaleValue( BaseExpansionButtonY );
                ExpansionLabelX = ScaleValue( BaseExpansionLabelX );
                ExpansionLabelY = ScaleValue( BaseExpansionLabelY );

                if ( IsCompact )
                {
                    int compactExpansionYOffset = ScaleValue( 10 );

                    ExpansionButtonY += compactExpansionYOffset;
                    ExpansionLabelY += compactExpansionYOffset;
                }
                SummaryX = ScaleValue( BaseSummaryX );
                SummaryWidth = ScaleValue( BaseSummaryWidth );
                SummaryHeight = ScaleValue( BaseSummaryHeight );
                SummaryLineOneY = ScaleValue( BaseSummaryLineOneY );
                SummaryLineTwoY = ScaleValue( BaseSummaryLineTwoY );
                TitleY = ScaleValue( BaseTitleY );
                TitleHeight = ScaleValue( BaseTitleHeight );
                PageLabelX = ScaleValue( BasePageLabelX );
                PageLabelY = ScaleValue( BasePageLabelY );
                PrevPageButtonX = ScaleValue( BasePrevPageButtonX );
                NextPageButtonX = ScaleValue( BaseNextPageButtonX );
                PageButtonY = ScaleValue( BasePageButtonY );

                DetailLineHeight = Math.Max( 1, DetailLineHeight );
                DetailTopPadding = Math.Max( 1, DetailTopPadding );
                DetailBottomPadding = Math.Max( 1, DetailBottomPadding );

                EntryHeight = DetailTopPadding + (DetailLineCount * DetailLineHeight) + DetailBottomPadding;
                EntrySpacing = EntryHeight + EntrySpacingPadding;
                EntryWidth = BackgroundWidth - (EntryX * 2);
                DetailTextX = EntryX + DetailTextOffset;
                DetailTextWidth = EntryWidth - ClaimReservedWidth - DetailToPreviewGap - PetPreviewAreaWidth;
                PetPreviewLeft = DetailTextX + DetailTextWidth + DetailToPreviewGap;
                PetPreviewHeight = EntryHeight - DetailTopPadding - DetailBottomPadding;
                ClaimButtonX = PetPreviewLeft + PetPreviewAreaWidth - ClaimButtonWidth - ClaimButtonPadding;
                ClaimLabelX = ClaimButtonX - ClaimLabelOffset;
                CompareButtonX = ClaimButtonX - CompareButtonOffset;
                CompareLabelX = CompareButtonX - ClaimLabelOffset;
                FavoritePetButtonX = PetPreviewLeft + ScaleValue( BaseFavoriteActionButtonXOffset );
                FavoritePetLabelX = FavoritePetButtonX + ScaleValue( BaseActionLabelOffsetX );
                AddListButtonX = PetPreviewLeft + ScaleValue( BaseAddListActionButtonXOffset );
                AddListLabelX = AddListButtonX + ScaleValue( BaseActionLabelOffsetX );
                BackgroundHeight = HeaderToEntryOffset + (PetsPerPage * EntryHeight) + ((PetsPerPage - 1) * EntrySpacingPadding) + FooterPadding;
            }

            public double Scale { get; private set; }
            public int BackgroundPadding { get; private set; }
            public int HeaderToEntryOffset { get; private set; }
            public int FooterPadding { get; private set; }
            public int BackgroundWidth { get; private set; }
            public int BackgroundHeight { get; private set; }
            public int EntryX { get; private set; }
            public int EntryWidth { get; private set; }
            public int DetailTextOffset { get; private set; }
            public int DetailTextX { get; private set; }
            public int ClaimReservedWidth { get; private set; }
            public int DetailToPreviewGap { get; private set; }
            public int PetPreviewAreaWidth { get; private set; }
            public int PetPreviewLeft { get; private set; }
            public int PetPreviewInnerPadding { get; private set; }
            public int ClaimButtonWidth { get; private set; }
            public int ClaimButtonPadding { get; private set; }
            public int ClaimLabelOffset { get; private set; }
            public int PetPreviewYOff { get; private set; }
            public int PetPreviewHeight { get; private set; }
            public int ClaimButtonX { get; private set; }
            public int ClaimLabelX { get; private set; }
            public int CompareButtonOffset { get; private set; }
            public int CompareButtonX { get; private set; }
            public int CompareLabelX { get; private set; }
            public int FavoritePetButtonX { get; private set; }
            public int FavoritePetLabelX { get; private set; }
            public int AddListButtonX { get; private set; }
            public int AddListLabelX { get; private set; }
            public int SettingsButtonX { get; private set; }
            public int SettingsButtonY { get; private set; }
            public int SettingsLabelX { get; private set; }
            public int SettingsLabelY { get; private set; }
            public int SearchLabelX { get; private set; }
            public int SearchLabelY { get; private set; }
            public int SearchEntryX { get; private set; }
            public int SearchEntryY { get; private set; }
            public int SearchEntryWidth { get; private set; }
            public int SearchEntryHeight { get; private set; }
            public int SearchButtonX { get; private set; }
            public int SearchButtonY { get; private set; }
            public int SearchLabelButtonX { get; private set; }
            public int SearchLabelButtonY { get; private set; }
            public int FavoritesButtonX { get; private set; }
            public int CustomListsButtonX { get; private set; }
            public int TopListButtonY { get; private set; }
            public int FavoritesLabelX { get; private set; }
            public int CustomListsLabelX { get; private set; }
            public int TopListLabelY { get; private set; }
            public int DetailLineHeight { get; private set; }
            public int DetailLineCount { get; private set; }
            public int DetailTopPadding { get; private set; }
            public int DetailBottomPadding { get; private set; }
            public int EntryHeight { get; private set; }
            public int EntrySpacingPadding { get; private set; }
            public int EntrySpacing { get; private set; }
            public int DetailTextWidth { get; private set; }
            public int EntryTitleOffset { get; private set; }
            public int EntryNameOffset { get; private set; }
            public int EntryButtonLabelOffset { get; private set; }
            public int ExpansionX { get; private set; }
            public int ExpansionY { get; private set; }
            public int ExpansionWidth { get; private set; }
            public int ExpansionHeight { get; private set; }
            public int ExpansionButtonY { get; private set; }
            public int ExpansionLabelX { get; private set; }
            public int ExpansionLabelY { get; private set; }
            public int SummaryX { get; private set; }
            public int SummaryWidth { get; private set; }
            public int SummaryHeight { get; private set; }
            public int SummaryLineOneY { get; private set; }
            public int SummaryLineTwoY { get; private set; }
            public int TitleY { get; private set; }
            public int TitleHeight { get; private set; }
            public int PageLabelX { get; private set; }
            public int PageLabelY { get; private set; }
            public int PrevPageButtonX { get; private set; }
            public int NextPageButtonX { get; private set; }
            public int PageButtonY { get; private set; }
            public bool IsCompact { get; private set; }

            public static int ScaleValue( double scale, int value )
            {
                return (int)Math.Round( value * scale );
            }

            public int ScaleValue( int value )
            {
                return ScaleValue( Scale, value );
            }
        }

        private static double ResolveScale( Mobile mobile )
        {
            PlayerMobile pm = mobile as PlayerMobile;

            if ( pm != null && pm.UseSmallStableMasterGump )
                return CompactScale;

            return DefaultScale;
        }

        internal static int ScaleOffset( Mobile mobile, int value )
        {
            return StableMasterLayout.ScaleValue( ResolveScale( mobile ), value );
        }

        internal static StableMasterLayout CreateLayout( Mobile mobile )
        {
            return new StableMasterLayout( ResolveScale( mobile ) );
        }

        public StableMasterGump( string title, Mobile from, IList<BaseCreature> list, int page, StableMasterGumpOptions options ) : base( ScaleOffset( from, 35 ), ScaleOffset( from, 40 ) )
        {
            if ( from == null )
                throw new ArgumentNullException( "from" );
            if ( list == null )
                throw new ArgumentNullException( "list" );
            if ( options == null )
                throw new ArgumentNullException( "options" );
            if ( options.CapacityResolver == null )
                throw new ArgumentException( "CapacityResolver must be provided", "options" );
            if ( options.ReopenHandler == null )
                throw new ArgumentException( "ReopenHandler must be provided", "options" );
            if ( options.ClaimHandler == null )
                throw new ArgumentException( "ClaimHandler must be provided", "options" );

            m_Title = String.IsNullOrEmpty( title ) ? "Stable Overview" : title;
            m_From = from;
            m_List = list;
            m_Options = options;
            m_DisplayFlags = GetDisplayFlags( from );
            m_SearchTerm = GetSearchTerm( from );
            m_Layout = CreateLayout( from );

            from.CloseGump( typeof( StableMasterGump ) );

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            int totalPages = Math.Max( 1, (int)Math.Ceiling( list.Count / (double)PetsPerPage ) );
            m_Page = Math.Max( 0, Math.Min( page, totalPages - 1 ) );

            AddPage( 0 );
            AddBackground( 0, 0, m_Layout.BackgroundWidth, m_Layout.BackgroundHeight, 9270 );
            AddImageTiled( m_Layout.BackgroundPadding, m_Layout.BackgroundPadding, m_Layout.BackgroundWidth - (m_Layout.BackgroundPadding * 2), m_Layout.BackgroundHeight - (m_Layout.BackgroundPadding * 2), 2624 );
            AddAlphaRegion( m_Layout.BackgroundPadding, m_Layout.BackgroundPadding, m_Layout.BackgroundWidth - (m_Layout.BackgroundPadding * 2), m_Layout.BackgroundHeight - (m_Layout.BackgroundPadding * 2) );

            string titleHtml = String.Format(
                "<CENTER><BASEFONT COLOR=#FFFFFF>{0}</BASEFONT></CENTER>",
                Utility.FixHtml( m_Title ) );

            AddHtml( 0, m_Layout.TitleY, m_Layout.BackgroundWidth, m_Layout.TitleHeight, titleHtml, false, false );

            if ( from is PlayerMobile )
            {
                AddButton( m_Layout.SettingsButtonX, m_Layout.SettingsButtonY, 4005, 4007, SettingsButtonID, GumpButtonType.Reply, 0 );
                AddLabel( m_Layout.SettingsLabelX, m_Layout.SettingsLabelY, 0x34, "Settings" );
            }

            int capacity = options.CapacityResolver( from );
            int weeklyCost = list.Count * options.WeeklyCostPerPet;

            AddHtml( m_Layout.SummaryX, m_Layout.SummaryLineOneY, m_Layout.SummaryWidth, m_Layout.SummaryHeight, String.Format( "<BASEFONT COLOR=#FFFFFF>{0} of {1} pets currently stabled</BASEFONT>", list.Count, capacity ), false, false );
            AddHtml( m_Layout.SummaryX, m_Layout.SummaryLineTwoY, m_Layout.SummaryWidth, m_Layout.SummaryHeight, String.Format( "<BASEFONT COLOR=#9DC6FF>Weekly cost: {0} × {1} = {2:N0} gold</BASEFONT>", list.Count, options.WeeklyCostPerPet, weeklyCost ), false, false );

            AddLabel( m_Layout.SearchLabelX, m_Layout.SearchLabelY, 0x34, "Search" );
            AddTextEntry( m_Layout.SearchEntryX, m_Layout.SearchEntryY, m_Layout.SearchEntryWidth, m_Layout.SearchEntryHeight, 0x34, SearchTextEntryID, GetSearchEntryText( m_SearchTerm ) );
            AddButton( m_Layout.SearchButtonX, m_Layout.SearchButtonY, 4005, 4007, SearchButtonID, GumpButtonType.Reply, 0 );
            AddLabel( m_Layout.SearchLabelButtonX, m_Layout.SearchLabelButtonY, 0x34, "Find" );

            PlayerMobile pm = from as PlayerMobile;
            if ( pm != null )
            {
                PruneStableListState( pm );

                AddButton( m_Layout.FavoritesButtonX, m_Layout.TopListButtonY, 4005, 4007, FavoritesButtonID, GumpButtonType.Reply, 0 );
                AddLabel( m_Layout.FavoritesLabelX, m_Layout.TopListLabelY, 0x34, "Favorites" );

                AddButton( m_Layout.CustomListsButtonX, m_Layout.TopListButtonY, 4005, 4007, CustomListsButtonID, GumpButtonType.Reply, 0 );
                AddLabel( m_Layout.CustomListsLabelX, m_Layout.TopListLabelY, 0x34, "Listings" );
            }

            if ( pm != null && options.DisplayExpansionSection )
                AddExpansionSection( pm );

            AddLabel( m_Layout.PageLabelX, m_Layout.PageLabelY, 0x34, String.Format( "Page {0} of {1}", m_Page + 1, totalPages ) );
            if ( m_Page > 0 )
                AddButton( m_Layout.PrevPageButtonX, m_Layout.PageButtonY, 4014, 4016, PrevPageButtonID, GumpButtonType.Reply, 0 );
            if ( m_Page + 1 < totalPages )
                AddButton( m_Layout.NextPageButtonX, m_Layout.PageButtonY, 4005, 4007, NextPageButtonID, GumpButtonType.Reply, 0 );

            int startIndex = m_Page * PetsPerPage;

            for ( int i = 0; i < PetsPerPage; ++i )
            {
                int petIndex = startIndex + i;
                if ( petIndex >= m_List.Count )
                    break;

                BaseCreature pet = m_List[petIndex];
                int entryY = m_Layout.HeaderToEntryOffset + (i * m_Layout.EntrySpacing);

                DrawPetEntry( pet, petIndex, entryY );
            }
        }

        public override void OnResponse( NetState sender, RelayInfo info )
        {
            int buttonID = info.ButtonID;

            if ( buttonID == 0 )
                return;

            if ( buttonID == SettingsButtonID )
            {
                if ( m_Options != null && m_Options.ReopenHandler != null )
                    m_Options.ReopenHandler( m_From, m_Page );
                else
                    m_From.SendGump( new StableMasterGump( m_Title, m_From, m_List, m_Page, m_Options ) );

                PlayerMobile pmSettings = m_From as PlayerMobile;

                if ( pmSettings != null )
                    pmSettings.SendGump( new StableMasterSettingsGump( m_Title, pmSettings, m_Options, m_Page ) );

                return;
            }

            if ( buttonID == SearchButtonID )
            {
                HandleSearchResponse( info );
                return;
            }

            if ( buttonID == FavoritesButtonID )
            {
                ReopenMainGump();
                StablePetCollectionGump.ShowFavorites( m_Title, m_From, m_Options, 0, StableGumpReturnContext.Main( m_Page ) );
                return;
            }

            if ( buttonID == CustomListsButtonID )
            {
                ReopenMainGump();
                StableCustomListsGump.Show( m_Title, m_From, m_Options, 0, StableGumpReturnContext.Main( m_Page ) );
                return;
            }

            if ( buttonID == PurchaseButtonID )
            {
                if ( m_Options.TryPurchaseExpansion != null && m_Options.TryPurchaseExpansion( m_From as PlayerMobile ) )
                {
                    m_Options.ReopenHandler( m_From, m_Page );
                }
                else
                {
                    m_From.SendGump( new StableMasterGump( m_Title, m_From, m_List, m_Page, m_Options ) );
                }

                return;
            }

            if ( buttonID == PrevPageButtonID )
            {
                m_Options.ReopenHandler( m_From, Math.Max( m_Page - 1, 0 ) );
                return;
            }

            if ( buttonID == NextPageButtonID )
            {
                m_Options.ReopenHandler( m_From, m_Page + 1 );
                return;
            }

            if ( buttonID >= FavoritePetButtonBase && buttonID < FavoritePetButtonBase + m_List.Count )
            {
                int index = buttonID - FavoritePetButtonBase;

                if ( index >= 0 && index < m_List.Count )
                    ToggleFavoritePet( m_List[index] );

                ReopenMainGump();
                return;
            }

            if ( buttonID >= AddToListButtonBase && buttonID < AddToListButtonBase + m_List.Count )
            {
                int index = buttonID - AddToListButtonBase;

                if ( index >= 0 && index < m_List.Count )
                {
                    BaseCreature pet = m_List[index];

                    if ( IsCurrentStabledPet( m_From, pet ) )
                    {
                        ReopenMainGump();
                        StableAddToCustomListGump.Show( m_Title, m_From, pet, m_Options, m_Page );
                    }
                    else
                    {
                        ReopenMainGump();
                    }
                }

                return;
            }

            if ( buttonID >= ComparePetButtonBase )
            {
                int index = buttonID - ComparePetButtonBase;

                if ( index >= 0 && index < m_List.Count )
                {
                    BaseCreature pet = m_List[index];

                    if ( pet != null )
                    {
                        List<BaseCreature> comparePets = new List<BaseCreature>( 1 );
                        comparePets.Add( pet );

                        StablePetComparisonGump.Show( m_Title, m_From, comparePets, m_DisplayFlags );
                    }
                }

                if ( m_Options != null && m_Options.ReopenHandler != null )
                    m_Options.ReopenHandler( m_From, m_Page );
                else
                    m_From.SendGump( new StableMasterGump( m_Title, m_From, m_List, m_Page, m_Options ) );

                return;
            }

            if ( buttonID >= PetButtonBase )
            {
                int index = buttonID - PetButtonBase;

                if ( index >= 0 && index < m_List.Count )
                {
                    m_Options.ClaimHandler( m_From, m_List[index] );
                    m_Options.ReopenHandler( m_From, m_Page );
                }
            }
        }

        private void AddExpansionSection( PlayerMobile pm )
        {
            if ( m_Options == null )
                return;

            int expansions = Math.Max( 0, Math.Min( pm.PurchasedStableExpansions, m_Options.MaxStableExpansions ) );
            int nextCapacity = m_Options.BaseStableSlots + (expansions + 1) * m_Options.StableExpansionAmount;
            if ( nextCapacity > m_Options.MaxStableSlots )
                nextCapacity = m_Options.MaxStableSlots;

            if ( expansions >= m_Options.MaxStableExpansions )
            {
                AddHtml( m_Layout.ExpansionX, m_Layout.ExpansionY, m_Layout.ExpansionWidth, m_Layout.ExpansionHeight, String.Format( "<BASEFONT COLOR=#C5FFAE>Stable expansions: {0} (maximum reached)</BASEFONT>", expansions ), false, false );
                return;
            }

            if ( m_Options.TryPurchaseExpansion == null || m_Options.GetExpansionCost == null )
            {
                AddHtml( m_Layout.ExpansionX, m_Layout.ExpansionY, m_Layout.ExpansionWidth, m_Layout.ExpansionHeight, String.Format( "<BASEFONT COLOR=#C5FFAE>Stable expansions: {0}<br>Next +{1} slots (total {2})</BASEFONT>", expansions, m_Options.StableExpansionAmount, nextCapacity ), false, false );
                return;
            }

            int cost = m_Options.GetExpansionCost( pm );

            AddHtml( m_Layout.ExpansionX, m_Layout.ExpansionY, m_Layout.ExpansionWidth, m_Layout.ExpansionHeight, String.Format( "<BASEFONT COLOR=#C5FFAE>Stable expansions: {0}<br>Next +{1} slots (total {2}) costs {3:N0} gold</BASEFONT>", expansions, m_Options.StableExpansionAmount, nextCapacity, cost ), false, false );
            AddButton( m_Layout.ExpansionX, m_Layout.ExpansionButtonY, 4005, 4007, PurchaseButtonID, GumpButtonType.Reply, 0 );
            AddLabel( m_Layout.ExpansionLabelX, m_Layout.ExpansionLabelY, 0x34, "Purchase upgrade" );
        }

        private void DrawPetEntry( BaseCreature pet, int petIndex, int entryY )
        {
            AddImageTiled( m_Layout.EntryX, entryY, m_Layout.EntryWidth, m_Layout.EntryHeight, 2624 );
            AddAlphaRegion( m_Layout.EntryX, entryY, m_Layout.EntryWidth, m_Layout.EntryHeight );

            AddLabel( m_Layout.DetailTextX, entryY + m_Layout.EntryTitleOffset, 0x2, GetCreatureLabel( pet ) );
            AddLabel( m_Layout.DetailTextX, entryY + m_Layout.EntryNameOffset, 0x58, GetDisplayName( pet ) );

            AddButton( m_Layout.CompareButtonX, entryY + m_Layout.EntryTitleOffset, 4005, 4007, ComparePetButtonBase + petIndex, GumpButtonType.Reply, 0 );
            AddLabel( m_Layout.CompareLabelX, entryY + m_Layout.EntryButtonLabelOffset, 0x34, "Compare" );

            AddButton( m_Layout.ClaimButtonX, entryY + m_Layout.EntryTitleOffset, 4005, 4007, PetButtonBase + petIndex, GumpButtonType.Reply, 0 );
            AddLabel( m_Layout.ClaimLabelX, entryY + m_Layout.EntryButtonLabelOffset, 0x34, "Claim" );

            PlayerMobile pm = m_From as PlayerMobile;

            if ( pm != null && pet != null )
            {
                int secondaryButtonY = entryY + m_Layout.EntryTitleOffset;
                int secondaryLabelY = entryY + m_Layout.EntryButtonLabelOffset;
                bool favorite = pm.StableListState.IsFavorite( pet.Serial );

                AddButton( m_Layout.FavoritePetButtonX, secondaryButtonY, 4005, 4007, FavoritePetButtonBase + petIndex, GumpButtonType.Reply, 0 );
                AddLabel( m_Layout.FavoritePetLabelX, secondaryLabelY, 0x34, favorite ? "Unfav" : "Favorite" );

                AddButton( m_Layout.AddListButtonX, secondaryButtonY, 4005, 4007, AddToListButtonBase + petIndex, GumpButtonType.Reply, 0 );
                AddLabel( m_Layout.AddListLabelX, secondaryLabelY, 0x34, "Add list" );
            }

            DrawPetPreview( pet, entryY );

            StringBuilder details = new StringBuilder();
            BuildPetDetails( pet, details, m_DisplayFlags );
            AddHtml( m_Layout.DetailTextX, entryY + m_Layout.DetailTopPadding, m_Layout.DetailTextWidth, m_Layout.EntryHeight - m_Layout.DetailTopPadding - m_Layout.DetailBottomPadding, details.ToString(), false, true );
        }

        private void DrawPetPreview( BaseCreature pet, int entryY )
        {
            if ( pet == null )
                return;

            int itemID = GetPetPreviewItemID( pet );

            if ( itemID <= 0 )
                return;

            int previewY = entryY + m_Layout.PetPreviewYOff;
            int previewAreaX = m_Layout.PetPreviewLeft;
            int previewAreaY = previewY - m_Layout.PetPreviewInnerPadding;
            int previewAreaHeight = m_Layout.PetPreviewHeight + (m_Layout.PetPreviewInnerPadding * 2);

            AddImageTiled( previewAreaX, previewAreaY, m_Layout.PetPreviewAreaWidth, previewAreaHeight, 2624 );
            AddAlphaRegion( previewAreaX, previewAreaY, m_Layout.PetPreviewAreaWidth, previewAreaHeight );

            Rectangle2D bounds = ItemBounds.Table[itemID & 0x3FFF];
            int centerX = previewAreaX + (m_Layout.PetPreviewAreaWidth / 2);
            int centerY = previewAreaY + (previewAreaHeight / 2);

            int width = bounds.Width;
            int height = bounds.Height;
            int offsetX = bounds.X;
            int offsetY = bounds.Y;

            if ( width <= 0 )
                width = 44;

            if ( height <= 0 )
                height = 44;

            int itemX = centerX - (width / 2) - offsetX;
            int itemY = centerY - (height / 2) - offsetY;

            int minX = previewAreaX - offsetX;
            int maxX = previewAreaX + m_Layout.PetPreviewAreaWidth - width - offsetX;
            if ( itemX < minX )
                itemX = minX;
            else if ( itemX > maxX )
                itemX = maxX;

            int minY = previewAreaY - offsetY;
            int maxY = previewAreaY + previewAreaHeight - height - offsetY;
            if ( itemY < minY )
                itemY = minY;
            else if ( itemY > maxY )
                itemY = maxY;

            AddItem( itemX, itemY, itemID, pet.Hue );
        }

        internal static string GetCreatureLabel( BaseCreature pet )
        {
            if ( pet == null )
                return String.Empty;

            string friendly = BaseCreature.FriendlyNameAttribute.GetFriendlyNameFor( pet.GetType() );
            if ( !String.IsNullOrEmpty( friendly ) )
                return friendly;

            string label = pet.GetType().Name;
            return label.Replace( '_', ' ' );
        }

        internal static string GetDisplayName( BaseCreature pet )
        {
            if ( pet == null )
                return String.Empty;

            if ( !String.IsNullOrEmpty( pet.Name ) )
                return pet.Name;

            return pet.GetType().Name.Replace( '_', ' ' );
        }

        private static int GetPetPreviewItemID( BaseCreature pet )
        {
            if ( pet == null )
                return ShrinkTable.DefaultItemID;

            if ( pet.Body.IsHuman )
                return ShrinkTable.DefaultItemID;

            return ShrinkTable.Lookup( pet, ShrinkTable.DefaultItemID );
        }

        internal static void BuildPetDetails( BaseCreature pet, StringBuilder sb, StableStatDisplayFlags displayFlags )
        {
            if ( pet == null || sb == null )
                return;

            TimeSpan age = DateTime.UtcNow - pet.CreationTime;
            if ( age < TimeSpan.Zero )
                age = TimeSpan.Zero;

            string ageText = FormatDurationDetailed( age );
            string stabledText = pet.LastStableTime == DateTime.MinValue ? "Not recorded" : FormatDurationDetailed( pet.StableDuration );

            List<string> details = new List<string>();

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.BasicInfo ) )
            {
                details.Add( String.Format( "<BASEFONT COLOR=#FFFFFF>Sex: {0} | Age: {1} | Level: {2} / Max level: {3} | Mating level: {4} | Taming Skill Required: {5:0.0}</BASEFONT>",
                    pet.SexString,
                    ageText,
                    pet.Level,
                    pet.MaxLevel,
                    pet.MatingLevel,
                    pet.MinTameSkill ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.ControlAndHue ) )
            {
                string hueRarity = GetHueRarity( pet.Hue );

                details.Add( String.Format( "<BASEFONT COLOR=#FFFFFF>Control slots: {0} | Stabled: {1} | Hue: {2} ({3})</BASEFONT>",
                    pet.ControlSlots,
                    stabledText,
                    pet.Hue,
                    hueRarity ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.BondingStatus ) )
            {
                string bondingStatus;
                string bondingTimer = GetBondingInformation( pet, out bondingStatus );
                string breedingStatus = GetBreedingStatus( pet );

                details.Add( String.Format( "<BASEFONT COLOR=#FFFFFF>Bonding: {0} | Time left to bond: {1} | Breeding cooldown: {2}</BASEFONT>",
                    bondingStatus,
                    bondingTimer,
                    breedingStatus ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.StatSummary ) )
            {
                string special = GetSpecialAbilityName( pet );
                string barding = GetBardingDifficulty( pet );

                details.Add( String.Format( "<BASEFONT COLOR=#FFFFFF>Stats: Str {0}, Dex {1}, Int {2} | Special: {3} | Barding: {4}</BASEFONT>",
                    pet.Str,
                    pet.Dex,
                    pet.Int,
                    special,
                    barding ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.DamageProfile ) )
            {
                string damageTypes = GetDamageTypes( pet );

                details.Add( String.Format( "<BASEFONT COLOR=#FFFFFF>Base damage: {0}-{1} | Damage Types: </BASEFONT>{2}",
                    pet.DamageMin,
                    pet.DamageMax,
                    damageTypes ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.SkillSummary ) )
            {
                string skillSummary = GetSkillSummary( pet );

                details.Add( String.Format( "<BASEFONT COLOR=#FFFFFF>Skills: {0}</BASEFONT>", skillSummary ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.Experience ) )
            {
                string packInstinct = GetPackInstinctSummary( pet );

                details.Add( String.Format( "<BASEFONT COLOR=#FFFFFF>Exp: {0:N0} | Exp Needed: {1:N0} | Pack instincts: {2}</BASEFONT>",
                    pet.Experience,
                    pet.ExpToNextLevel,
                    packInstinct ) );
            }

            double totalPercent = 0.0;
            int percentCount = 0;
            List<string> perfection = new List<string>();

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.PerfectionHits ) )
            {
                perfection.Add( BuildPerfectionLine( "Hits Max", DefaultStatLabelColor, pet.HitsMax, GetMaxAttribute( pet, JakoAttributesEnum.Hits ), ref totalPercent, ref percentCount, String.Format( "current {0}", pet.Hits ) ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.PerfectionStamina ) )
            {
                perfection.Add( BuildPerfectionLine( "Stamina Max", DefaultStatLabelColor, pet.StamMax, GetMaxAttribute( pet, JakoAttributesEnum.Stam ), ref totalPercent, ref percentCount, String.Format( "current {0}", pet.Stam ) ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.PerfectionMana ) )
            {
                perfection.Add( BuildPerfectionLine( "Mana Max", DefaultStatLabelColor, pet.ManaMax, GetMaxAttribute( pet, JakoAttributesEnum.Mana ), ref totalPercent, ref percentCount, String.Format( "current {0}", pet.Mana ) ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.PerfectionPhysicalResist ) )
            {
                perfection.Add( BuildPerfectionLine( "Physical Resist", PhysicalColor, pet.PhysicalResistance, GetMaxAttribute( pet, JakoAttributesEnum.BonusPhysResist ), ref totalPercent, ref percentCount, null ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.PerfectionFireResist ) )
            {
                perfection.Add( BuildPerfectionLine( "Fire Resist", FireColor, pet.FireResistance, GetMaxAttribute( pet, JakoAttributesEnum.BonusFireResist ), ref totalPercent, ref percentCount, null ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.PerfectionColdResist ) )
            {
                perfection.Add( BuildPerfectionLine( "Cold Resist", ColdColor, pet.ColdResistance, GetMaxAttribute( pet, JakoAttributesEnum.BonusColdResist ), ref totalPercent, ref percentCount, null ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.PerfectionPoisonResist ) )
            {
                perfection.Add( BuildPerfectionLine( "Poison Resist", PoisonColor, pet.PoisonResistance, GetMaxAttribute( pet, JakoAttributesEnum.BonusPoisResist ), ref totalPercent, ref percentCount, null ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.PerfectionEnergyResist ) )
            {
                perfection.Add( BuildPerfectionLine( "Energy Resist", EnergyColor, pet.EnergyResistance, GetMaxAttribute( pet, JakoAttributesEnum.BonusEnerResist ), ref totalPercent, ref percentCount, null ) );
            }

            if ( ShouldDisplay( displayFlags, StableStatDisplayFlags.OverallPerfection ) )
            {
                double overall = percentCount > 0 ? ( totalPercent / percentCount ) : 0.0;
                perfection.Add( String.Format( "<BASEFONT COLOR=#9DC6FF>Overall perfection: {0:0.0}%</BASEFONT>", overall * 100.0 ) );
            }

            bool appendedAny = false;

            AppendSection( sb, details, ref appendedAny );

            if ( perfection.Count > 0 )
                AppendSection( sb, perfection, ref appendedAny );

            if ( appendedAny )
                sb.Append( "<br>" );
            else
                sb.Append( "<br><br>" );
        }

        private static string BuildPerfectionLine( string label, string labelColor, int current, int max, ref double totalPercent, ref int count, string extra )
        {
            string formattedLabel = FormatStatLabel( label, labelColor );

            if ( max <= 0 )
            {
                if ( extra != null )
                    return String.Format( "{0}: <BASEFONT COLOR={1}>{2} ({3})</BASEFONT>", formattedLabel, DefaultStatValueColor, current, extra );

                return String.Format( "{0}: <BASEFONT COLOR={1}>{2}</BASEFONT>", formattedLabel, DefaultStatValueColor, current );
            }

            double percent = (double)current / max;

            if ( percent < 0.0 )
                percent = 0.0;
            else if ( percent > 1.0 )
                percent = 1.0;

            totalPercent += percent;
            count++;

            if ( extra != null )
            {
                return String.Format( "{0}: <BASEFONT COLOR={1}>{2}/{3} ({4:0.0}% perfect) - {5}</BASEFONT>",
                    formattedLabel,
                    DefaultStatValueColor,
                    current,
                    max,
                    percent * 100.0,
                    extra );
            }

            return String.Format( "{0}: <BASEFONT COLOR={1}>{2}/{3} ({4:0.0}% perfect)</BASEFONT>",
                formattedLabel,
                DefaultStatValueColor,
                current,
                max,
                percent * 100.0 );
        }

        private static void AppendSection( StringBuilder sb, IList<string> lines, ref bool appendedAny )
        {
            if ( sb == null || lines == null || lines.Count == 0 )
                return;

            if ( !appendedAny )
            {
                sb.Append( "<br>" );
                appendedAny = true;
            }
            else
            {
                sb.Append( "<br>" );
            }

            for ( int i = 0; i < lines.Count; ++i )
            {
                sb.Append( lines[i] );
                sb.Append( "<br>" );
            }
        }

        private static string FormatStatLabel( string label, string labelColor )
        {
            if ( String.IsNullOrEmpty( labelColor ) )
                labelColor = DefaultStatLabelColor;

            return String.Format( "<BASEFONT COLOR={0}>{1}</BASEFONT>", labelColor, label );
        }

        private static int GetMaxAttribute( BaseCreature pet, JakoAttributesEnum attribute )
        {
            if ( pet == null || pet.m_jakoAttributes == null )
                return 0;

            JakoBaseAttribute attr = pet.m_jakoAttributes.GetAttribute( attribute );
            if ( attr == null )
                return 0;

            return (int)attr.MaxBonus( pet );
        }

        private static bool ShouldDisplay( StableStatDisplayFlags flags, StableStatDisplayFlags flag )
        {
            return ( flags & flag ) != 0;
        }

        private void HandleSearchResponse( RelayInfo info )
        {
            string query = GetSearchQuery( info );

            SetSearchTerm( m_From, query );

            ReopenMainGump();

            if ( !String.IsNullOrEmpty( query ) )
                StablePetCollectionGump.ShowSearchResults( m_Title, m_From, m_Options, query, 0, StableGumpReturnContext.Main( m_Page ) );
        }

        private string GetSearchQuery( RelayInfo info )
        {
            if ( info == null )
                return String.Empty;

            TextRelay relay = info.GetTextEntry( SearchTextEntryID );

            if ( relay == null )
                return String.Empty;

            return StableMasterListState.NormalizeSearchTerm( relay.Text );
        }

        internal static bool MatchesSearchTerm( BaseCreature pet, string searchTerm )
        {
            if ( pet == null || String.IsNullOrEmpty( searchTerm ) )
                return false;

            if ( ContainsIgnoreCase( pet.Name, searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( GetDisplayName( pet ), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( GetCreatureLabel( pet ), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( pet.GetType().Name, searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( pet.SexString, searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( GetSpecialAbilityName( pet ), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( GetHueRarity( pet.Hue ), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( pet.Hue.ToString(), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( pet.Level.ToString(), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( pet.MaxLevel.ToString(), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( pet.MatingLevel.ToString(), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( pet.ControlSlots.ToString(), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( pet.Str.ToString(), searchTerm ) || ContainsIgnoreCase( pet.Dex.ToString(), searchTerm ) || ContainsIgnoreCase( pet.Int.ToString(), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( GetBardingDifficulty( pet ), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( pet.DamageMin.ToString(), searchTerm ) || ContainsIgnoreCase( pet.DamageMax.ToString(), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( GetDamageTypes( pet ), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( GetSkillSummary( pet ), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( GetPackInstinctSummary( pet ), searchTerm ) )
                return true;

            if ( ContainsIgnoreCase( pet.Serial.Value.ToString(), searchTerm ) )
                return true;

            StringBuilder detailBuilder = new StringBuilder();
            BuildPetDetails( pet, detailBuilder, StableStatDisplayFlags.All );

            return ContainsIgnoreCase( detailBuilder.ToString(), searchTerm );
        }

        private static bool ContainsIgnoreCase( string source, string term )
        {
            if ( String.IsNullOrEmpty( source ) || String.IsNullOrEmpty( term ) )
                return false;

            return source.IndexOf( term, StringComparison.OrdinalIgnoreCase ) >= 0;
        }

        private static string GetSearchEntryText( string term )
        {
            return String.IsNullOrEmpty( term ) ? StableMasterListState.SearchPlaceholder : term;
        }

        private void ReopenMainGump()
        {
            if ( m_Options != null && m_Options.ReopenHandler != null )
                m_Options.ReopenHandler( m_From, m_Page );
            else
                m_From.SendGump( new StableMasterGump( m_Title, m_From, m_List, m_Page, m_Options ) );
        }

        private void ToggleFavoritePet( BaseCreature pet )
        {
            PlayerMobile pm = m_From as PlayerMobile;

            if ( pm == null || !IsCurrentStabledPet( m_From, pet ) )
                return;

            bool favorite = pm.StableListState.ToggleFavorite( pet.Serial );
            m_From.SendMessage( favorite ? "That creature has been added to your stable favorites." : "That creature has been removed from your stable favorites." );
        }

        private static string GetSearchTerm( Mobile mobile )
        {
            PlayerMobile pm = mobile as PlayerMobile;

            if ( pm != null )
                return pm.StableSearchTerm;

            return String.Empty;
        }

        private static void SetSearchTerm( Mobile mobile, string value )
        {
            PlayerMobile pm = mobile as PlayerMobile;

            if ( pm != null )
                pm.StableSearchTerm = value;
        }

        internal static bool IsCurrentStabledPet( Mobile from, BaseCreature pet )
        {
            return from != null && pet != null && !pet.Deleted && from.Stabled != null && from.Stabled.Contains( pet );
        }

        internal static List<BaseCreature> BuildSortedStablePetList( Mobile from )
        {
            List<BaseCreature> list = new List<BaseCreature>();

            if ( from == null || from.Stabled == null )
                return list;

            for ( int i = 0; i < from.Stabled.Count; ++i )
            {
                BaseCreature pet = from.Stabled[i] as BaseCreature;

                if ( pet == null || pet.Deleted )
                    continue;

                list.Add( pet );
            }

            list.Sort( delegate ( BaseCreature a, BaseCreature b )
            {
                string nameA = GetDisplayName( a );
                string nameB = GetDisplayName( b );
                return String.Compare( nameA, nameB, StringComparison.OrdinalIgnoreCase );
            } );

            return list;
        }

        internal static List<Serial> BuildCurrentStableSerials( Mobile from )
        {
            List<Serial> serials = new List<Serial>();

            if ( from == null || from.Stabled == null )
                return serials;

            for ( int i = 0; i < from.Stabled.Count; ++i )
            {
                BaseCreature pet = from.Stabled[i] as BaseCreature;

                if ( pet != null && !pet.Deleted )
                    serials.Add( pet.Serial );
            }

            return serials;
        }

        internal static void PruneStableListState( PlayerMobile pm )
        {
            if ( pm == null )
                return;

            pm.StableListState.PruneInvalidPets( BuildCurrentStableSerials( pm ) );
        }

        internal static List<BaseCreature> BuildFavoritePetList( PlayerMobile pm )
        {
            List<BaseCreature> source = BuildSortedStablePetList( pm );
            List<BaseCreature> favorites = new List<BaseCreature>();

            if ( pm == null )
                return favorites;

            PruneStableListState( pm );

            for ( int i = 0; i < source.Count; ++i )
            {
                BaseCreature pet = source[i];

                if ( pet != null && pm.StableListState.IsFavorite( pet.Serial ) )
                    favorites.Add( pet );
            }

            return favorites;
        }

        internal static List<BaseCreature> BuildSearchResultList( Mobile from, string query )
        {
            List<BaseCreature> source = BuildSortedStablePetList( from );
            List<BaseCreature> results = new List<BaseCreature>();
            string normalized = StableMasterListState.NormalizeSearchTerm( query );

            if ( normalized.Length == 0 )
                return results;

            for ( int i = 0; i < source.Count; ++i )
            {
                BaseCreature pet = source[i];

                if ( MatchesSearchTerm( pet, normalized ) )
                    results.Add( pet );
            }

            return results;
        }

        internal static List<BaseCreature> BuildCustomListPets( PlayerMobile pm, int listIndex )
        {
            List<BaseCreature> source = BuildSortedStablePetList( pm );
            List<BaseCreature> results = new List<BaseCreature>();

            if ( pm == null )
                return results;

            PruneStableListState( pm );

            if ( listIndex < 0 || listIndex >= pm.StableListState.CustomLists.Count )
                return results;

            StableCustomPetList customList = pm.StableListState.CustomLists[listIndex];

            if ( customList == null )
                return results;

            for ( int i = 0; i < source.Count; ++i )
            {
                BaseCreature pet = source[i];

                if ( pet != null && customList.ContainsPet( pet.Serial ) )
                    results.Add( pet );
            }

            return results;
        }

        private static StableStatDisplayFlags GetDisplayFlags( Mobile mobile )
        {
            PlayerMobile pm = mobile as PlayerMobile;

            if ( pm != null )
                return pm.StableDisplayFlags;

            return StableStatDisplayFlags.All;
        }

        private static string GetBondingInformation( BaseCreature pet, out string status )
        {
            if ( pet.IsBonded )
            {
                status = "Bonded";
                return "Completed";
            }

            if ( pet.BondingBegin == DateTime.MinValue )
            {
                status = "Not bonding";
                return "N/A";
            }

            DateTime readyTime = pet.BondingBegin + pet.BondingDelay;
            TimeSpan remaining = readyTime - DateTime.UtcNow;

            if ( remaining <= TimeSpan.Zero )
            {
                status = "Ready";
                return "Ready now";
            }

            status = "In progress";
            return FormatDurationDetailed( remaining );
        }

        private static string GetBreedingStatus( BaseCreature pet )
        {
            if ( pet.Breeding )
                return "Currently breeding";

            if ( pet.RealLevel < pet.MatingLevel )
                return "Too young to mate";

            TimeSpan remaining = pet.NextMate - DateTime.UtcNow;
            if ( remaining <= TimeSpan.Zero )
                return "Ready now";

            return FormatDurationDetailed( remaining );
        }

        private static string GetSpecialAbilityName( BaseCreature pet )
        {
            switch ( pet.Special )
            {
                case 1: return "Ferocious";
                case 2: return "Regenerative";
                case 3: return "Mystical";
                case 4: return "Firebreather";
                case 5: return "Thor's touch";
                case 6: return "Tactical";
                case 7: return "Shamanic";
                case 8: return "Chaotic";
                case 9: return "Poisonous";
                default: return "None";
            }
        }

        private static string GetHueRarity( int hue )
        {
            if ( hue <= 0 )
                return "Common";

            if ( hue < 500 )
                return "Uncommon";

            if ( hue < 1000 )
                return "Rare";

            if ( hue < 2000 )
                return "Very rare";

            return "Legendary";
        }

        private static string GetBardingDifficulty( BaseCreature pet )
        {
            if ( pet == null )
                return "N/A";

            if ( pet.Uncalmable )
                return "Uncalmable";

            double difficulty = BaseInstrument.GetBaseDifficulty( pet );
            return String.Format( "{0:0.0}", difficulty );
        }

        private static string GetDamageTypes( BaseCreature pet )
        {
            if ( pet == null )
                return String.Format( "<BASEFONT COLOR={0}>Unknown</BASEFONT>", DefaultStatValueColor );

            return String.Format( "{0}, {1}, {2}, {3}, {4}",
                FormatDamageType( "Phys", PhysicalColor, pet.PhysicalDamage ),
                FormatDamageType( "Fire", FireColor, pet.FireDamage ),
                FormatDamageType( "Cold", ColdColor, pet.ColdDamage ),
                FormatDamageType( "Poison", PoisonColor, pet.PoisonDamage ),
                FormatDamageType( "Energy", EnergyColor, pet.EnergyDamage ) );
        }

        private static string FormatDamageType( string name, string color, int amount )
        {
            return String.Format( "<BASEFONT COLOR={0}>{1}</BASEFONT> <BASEFONT COLOR={2}>{3}%</BASEFONT>",
                color,
                name,
                DefaultStatValueColor,
                Math.Max( 0, amount ) );
        }

        private static string GetSkillSummary( BaseCreature pet )
        {
            if ( pet == null || pet.Skills == null )
                return "N/A";

            return String.Format(
                "Wrest {0}, Tactics {1}, Resist {2}, Magery {3}, Eval {4}, Med {5}, Poison {6}",
                FormatSkill( pet, SkillName.Wrestling ),
                FormatSkill( pet, SkillName.Tactics ),
                FormatSkill( pet, SkillName.MagicResist ),
                FormatSkill( pet, SkillName.Magery ),
                FormatSkill( pet, SkillName.EvalInt ),
                FormatSkill( pet, SkillName.Meditation ),
                FormatSkill( pet, SkillName.Poisoning ) );
        }

        private static string FormatSkill( BaseCreature pet, SkillName skill )
        {
            if ( pet == null || pet.Skills == null )
                return "0.0";

            Skill info = pet.Skills[skill];
            if ( info == null )
                return "0.0";

            return info.Value.ToString( "0.0" );
        }

        private static string GetPackInstinctSummary( BaseCreature pet )
        {
            if ( pet == null )
                return "No";

            PackInstinct instinct = pet.PackInstinct;
            if ( instinct == PackInstinct.None )
                return "No";

            return String.Format( "Yes ({0})", instinct );
        }

        private static string FormatDurationDetailed( TimeSpan span )
        {
            if ( span < TimeSpan.Zero )
                span = TimeSpan.Zero;

            if ( span.TotalMinutes < 1.0 )
                return "<1m";

            int days = span.Days;
            int hours = span.Hours;
            int minutes = span.Minutes;

            StringBuilder sb = new StringBuilder();

            if ( days > 0 )
                sb.AppendFormat( "{0}d ", days );

            if ( hours > 0 )
                sb.AppendFormat( "{0}h ", hours );

            if ( minutes > 0 )
                sb.AppendFormat( "{0}m", minutes );

            if ( sb.Length == 0 )
                sb.AppendFormat( "{0}s", span.Seconds );

            return sb.ToString().Trim();
        }
    }

    public class StablePetComparisonGump : Gump
    {
        private const int MaxPetsToCompare = 4;
        private const int ColumnWidthBase = 260 * 2;
        private const int ColumnSpacingBase = 20;
        private const int HeaderHeightBase = 80;
        private const int BackgroundPaddingBase = 20;

        private readonly StableMasterGump.StableMasterLayout m_Layout;
        private readonly int m_ColumnWidth;
        private readonly int m_ColumnSpacing;
        private readonly int m_HeaderHeight;
        private readonly int m_BackgroundPadding;
        private readonly int m_ColumnHeight;

        private readonly Serial[] m_PetSerials;

        public static void Show( string title, Mobile from, IList<BaseCreature> pets, StableStatDisplayFlags displayFlags )
        {
            if ( from == null || pets == null || pets.Count == 0 )
                return;

            NetState ns = from.NetState;

            if ( ns != null )
            {
                List<Gump> gumps = ns.Gumps;

                if ( gumps != null )
                {
                    for ( int i = 0; i < gumps.Count; ++i )
                    {
                        StablePetComparisonGump existing = gumps[i] as StablePetComparisonGump;

                        if ( existing != null && existing.RepresentsPets( pets ) )
                        {
                            ns.Send( new CloseGump( existing.TypeID, 0 ) );
                            ns.RemoveGump( i );
                            existing.OnServerClose( ns );
                            break;
                        }
                    }
                }
            }

            from.SendGump( new StablePetComparisonGump( title, from, pets, displayFlags ) );
        }

        public StablePetComparisonGump( string title, Mobile from, IList<BaseCreature> pets, StableStatDisplayFlags displayFlags ) : base( StableMasterGump.ScaleOffset( from, 50 ), StableMasterGump.ScaleOffset( from, 50 ) )
        {
            if ( from == null )
                throw new ArgumentNullException( "from" );
            if ( pets == null )
                throw new ArgumentNullException( "pets" );

            int petCount = pets.Count;

            if ( petCount > MaxPetsToCompare )
                petCount = MaxPetsToCompare;

            if ( petCount <= 0 )
                petCount = 1;

            m_Layout = StableMasterGump.CreateLayout( from );
            m_ColumnWidth = m_Layout.ScaleValue( ColumnWidthBase );
            m_ColumnSpacing = m_Layout.ScaleValue( ColumnSpacingBase );
            m_HeaderHeight = m_Layout.ScaleValue( HeaderHeightBase );
            m_BackgroundPadding = m_Layout.ScaleValue( BackgroundPaddingBase );
            m_ColumnHeight = m_Layout.EntryHeight;

            m_PetSerials = new Serial[petCount];

            for ( int i = 0; i < petCount; ++i )
            {
                BaseCreature pet = i < pets.Count ? pets[i] : null;
                m_PetSerials[i] = pet != null ? pet.Serial : Server.Serial.MinusOne;
            }

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage( 0 );

            int innerWidth = ( petCount * m_ColumnWidth ) + ( ( petCount - 1 ) * m_ColumnSpacing );
            int backgroundWidth = innerWidth + ( m_BackgroundPadding * 2 );
            int backgroundHeight = m_HeaderHeight + m_ColumnHeight + ( m_BackgroundPadding * 2 );

            AddBackground( 0, 0, backgroundWidth, backgroundHeight, 9270 );
            AddImageTiled( m_BackgroundPadding, m_BackgroundPadding, backgroundWidth - ( m_BackgroundPadding * 2 ), backgroundHeight - ( m_BackgroundPadding * 2 ), 2624 );
            AddAlphaRegion( m_BackgroundPadding, m_BackgroundPadding, backgroundWidth - ( m_BackgroundPadding * 2 ), backgroundHeight - ( m_BackgroundPadding * 2 ) );

            string header = pets.Count <= 1 ? "Pet Stat Sheet" : "Pet Comparison";
            string headerHtml = String.Format(
                "<CENTER><BASEFONT COLOR=#FFFFFF>{0}</BASEFONT></CENTER>",
                Utility.FixHtml( header ) );

            AddHtml( m_BackgroundPadding, m_BackgroundPadding + 8, backgroundWidth - ( m_BackgroundPadding * 2 ), 32, headerHtml, false, false );

            for ( int i = 0; i < petCount; ++i )
            {
                BaseCreature pet = i < pets.Count ? pets[i] : null;
                int columnX = m_BackgroundPadding + ( i * ( m_ColumnWidth + m_ColumnSpacing ) );
                int columnY = m_BackgroundPadding + m_Layout.ScaleValue( 60 );

                string creatureLabel = Utility.FixHtml( GetComparisonLabel( pet ) );
                string displayName = Utility.FixHtml( GetComparisonDisplayName( pet ) );

                AddLabel( columnX, columnY - 36, 0x2, creatureLabel );
                AddLabel( columnX, columnY - 16, 0x58, displayName );

                StringBuilder sb = new StringBuilder();
                StableMasterGump.BuildPetDetails( pet, sb, displayFlags );

                if ( sb.Length == 0 )
                    sb.Append( "<BASEFONT COLOR=#FFFFFF>No details available.</BASEFONT><br><br>" );

                AddHtml( columnX, columnY, m_ColumnWidth, m_ColumnHeight, sb.ToString(), false, true );
            }
        }

        private static string GetComparisonLabel( BaseCreature pet )
        {
            if ( pet == null )
                return String.Empty;

            string friendly = BaseCreature.FriendlyNameAttribute.GetFriendlyNameFor( pet.GetType() );

            if ( !String.IsNullOrEmpty( friendly ) )
                return friendly;

            return pet.GetType().Name.Replace( '_', ' ' );
        }

        private static string GetComparisonDisplayName( BaseCreature pet )
        {
            if ( pet == null )
                return String.Empty;

            if ( !String.IsNullOrEmpty( pet.Name ) )
                return pet.Name;

            return pet.GetType().Name.Replace( '_', ' ' );
        }

        internal bool RepresentsPets( IList<BaseCreature> pets )
        {
            if ( pets == null )
                return false;

            int candidateCount = pets.Count;

            if ( candidateCount > MaxPetsToCompare )
                candidateCount = MaxPetsToCompare;

            if ( candidateCount <= 0 )
                candidateCount = 1;

            if ( candidateCount != m_PetSerials.Length )
                return false;

            for ( int i = 0; i < candidateCount; ++i )
            {
                BaseCreature pet = i < pets.Count ? pets[i] : null;
                Serial serial = pet != null ? pet.Serial : Server.Serial.MinusOne;

                if ( serial != m_PetSerials[i] )
                    return false;
            }

            return true;
        }
    }

    public class StableMasterSettingsGump : Gump
    {
        private class SettingEntry
        {
            public readonly StableStatDisplayFlags Flag;
            public readonly string Label;

            public SettingEntry( StableStatDisplayFlags flag, string label )
            {
                Flag = flag;
                Label = label;
            }
        }

        private const int ApplyButtonID = 1;
        private const int ResetButtonID = 2;
        private const int CompactLayoutCheckID = 500;
        private const int BackgroundWidth = 520;
        private const int BackgroundHeight = 470;
        private const int ColumnWidth = 240;
        private const int RowHeight = 24;
        private const int ItemsPerColumn = 8;

        private static readonly SettingEntry[] Settings = new SettingEntry[]
        {
            new SettingEntry( StableStatDisplayFlags.BasicInfo, "Sex / Age / Level" ),
            new SettingEntry( StableStatDisplayFlags.ControlAndHue, "Control slots / Hue" ),
            new SettingEntry( StableStatDisplayFlags.BondingStatus, "Bonding & Breeding" ),
            new SettingEntry( StableStatDisplayFlags.StatSummary, "Stats & Special" ),
            new SettingEntry( StableStatDisplayFlags.DamageProfile, "Damage profile" ),
            new SettingEntry( StableStatDisplayFlags.SkillSummary, "Skill summary" ),
            new SettingEntry( StableStatDisplayFlags.Experience, "Experience & Pack" ),
            new SettingEntry( StableStatDisplayFlags.PerfectionHits, "Hits perfection" ),
            new SettingEntry( StableStatDisplayFlags.PerfectionStamina, "Stamina perfection" ),
            new SettingEntry( StableStatDisplayFlags.PerfectionMana, "Mana perfection" ),
            new SettingEntry( StableStatDisplayFlags.PerfectionPhysicalResist, "Physical resist perfection" ),
            new SettingEntry( StableStatDisplayFlags.PerfectionFireResist, "Fire resist perfection" ),
            new SettingEntry( StableStatDisplayFlags.PerfectionColdResist, "Cold resist perfection" ),
            new SettingEntry( StableStatDisplayFlags.PerfectionPoisonResist, "Poison resist perfection" ),
            new SettingEntry( StableStatDisplayFlags.PerfectionEnergyResist, "Energy resist perfection" ),
            new SettingEntry( StableStatDisplayFlags.OverallPerfection, "Overall perfection" )
        };

        private readonly PlayerMobile m_Player;
        private readonly StableMasterGumpOptions m_Options;
        private readonly int m_Page;
        private readonly string m_Title;

        public StableMasterSettingsGump( string title, PlayerMobile player, StableMasterGumpOptions options, int page ) : base( 60, 60 )
        {
            if ( player == null )
                throw new ArgumentNullException( "player" );

            player.CloseGump( typeof( StableMasterSettingsGump ) );

            m_Player = player;
            m_Options = options;
            m_Page = page;
            m_Title = String.IsNullOrEmpty( title ) ? "Stable Overview" : title;

            StableStatDisplayFlags current = player.StableDisplayFlags;

            AddPage( 0 );
            AddBackground( 0, 0, BackgroundWidth, BackgroundHeight, 9270 );
            AddImageTiled( 15, 15, BackgroundWidth - 30, BackgroundHeight - 30, 2624 );
            AddAlphaRegion( 15, 15, BackgroundWidth - 30, BackgroundHeight - 30 );

            AddLabel( 30, 30, 0x34, "Stable Display Settings" );
            AddHtml( 30, 56, BackgroundWidth - 60, 40, "<BASEFONT COLOR=#FFFFFF>Toggle each row to show or hide it inside the stable overview.</BASEFONT>", false, false );

            for ( int i = 0; i < Settings.Length; ++i )
            {
                int column = i / ItemsPerColumn;
                int row = i % ItemsPerColumn;
                int x = 40 + ( column * ColumnWidth );
                int y = 106 + ( row * RowHeight );

                bool enabled = ( current & Settings[i].Flag ) != 0;

                AddCheck( x, y, 9026, 9027, enabled, i );
                AddLabel( x + 35, y, 0x34, Settings[i].Label );
            }

            int layoutY = BackgroundHeight - 96;
            AddCheck( 40, layoutY, 9026, 9027, player.UseSmallStableMasterGump, CompactLayoutCheckID );
            AddLabel( 75, layoutY, 0x34, "Use compact stable view" );

            int buttonY = BackgroundHeight - 60;

            AddButton( 40, buttonY, 4005, 4007, ApplyButtonID, GumpButtonType.Reply, 0 );
            AddLabel( 78, buttonY + 2, 0x34, "Apply" );

            AddButton( 180, buttonY, 4005, 4007, ResetButtonID, GumpButtonType.Reply, 0 );
            AddLabel( 218, buttonY + 2, 0x34, "Show All" );
        }

        public override void OnResponse( NetState sender, RelayInfo info )
        {
            if ( sender == null || sender.Mobile != m_Player )
                return;

            int buttonID = info.ButtonID;

            if ( buttonID == 0 )
                return;

            if ( buttonID == ResetButtonID )
            {
                m_Player.StableDisplayFlags = StableStatDisplayFlags.All;
                m_Player.UseSmallStableMasterGump = false;

                if ( m_Options != null && m_Options.ReopenHandler != null )
                    m_Options.ReopenHandler( m_Player, m_Page );

                m_Player.SendGump( new StableMasterSettingsGump( m_Title, m_Player, m_Options, m_Page ) );
                return;
            }

            if ( buttonID == ApplyButtonID )
            {
                StableStatDisplayFlags flags = StableStatDisplayFlags.None;
                int[] switches = info.Switches;
                bool compactLayout = info.IsSwitched( CompactLayoutCheckID );

                if ( switches != null )
                {
                    for ( int i = 0; i < switches.Length; ++i )
                    {
                        int index = switches[i];

                        if ( index >= 0 && index < Settings.Length )
                            flags |= Settings[index].Flag;
                    }
                }

                m_Player.StableDisplayFlags = flags;
                m_Player.UseSmallStableMasterGump = compactLayout;

                if ( m_Options != null && m_Options.ReopenHandler != null )
                    m_Options.ReopenHandler( m_Player, m_Page );

                m_Player.SendGump( new StableMasterSettingsGump( m_Title, m_Player, m_Options, m_Page ) );
            }
        }

    }

    [Flags]
    public enum StableStatDisplayFlags
    {
        None = 0,
        BasicInfo = 1 << 0,
        ControlAndHue = 1 << 1,
        BondingStatus = 1 << 2,
        StatSummary = 1 << 3,
        DamageProfile = 1 << 4,
        SkillSummary = 1 << 5,
        Experience = 1 << 6,
        PerfectionHits = 1 << 7,
        PerfectionStamina = 1 << 8,
        PerfectionMana = 1 << 9,
        PerfectionPhysicalResist = 1 << 10,
        PerfectionFireResist = 1 << 11,
        PerfectionColdResist = 1 << 12,
        PerfectionPoisonResist = 1 << 13,
        PerfectionEnergyResist = 1 << 14,
        OverallPerfection = 1 << 15,
        All = BasicInfo | ControlAndHue | BondingStatus | StatSummary | DamageProfile | SkillSummary | Experience | PerfectionHits | PerfectionStamina | PerfectionMana | PerfectionPhysicalResist | PerfectionFireResist | PerfectionColdResist | PerfectionPoisonResist | PerfectionEnergyResist | OverallPerfection
    }

    public class StableMasterGumpOptions
    {
        public Func<Mobile, int> CapacityResolver { get; set; }
        public Action<Mobile, int> ReopenHandler { get; set; }
        public Action<Mobile, BaseCreature> ClaimHandler { get; set; }
        public Func<PlayerMobile, bool> TryPurchaseExpansion { get; set; }
        public Func<PlayerMobile, int> GetExpansionCost { get; set; }
        public int WeeklyCostPerPet { get; set; }
        public int BaseStableSlots { get; set; }
        public int StableExpansionAmount { get; set; }
        public int MaxStableSlots { get; set; }
        public int MaxStableExpansions { get; set; }
        private bool m_DisplayExpansionSection = true;

        public bool DisplayExpansionSection
        {
            get { return m_DisplayExpansionSection; }
            set { m_DisplayExpansionSection = value; }
        }
    }
}
