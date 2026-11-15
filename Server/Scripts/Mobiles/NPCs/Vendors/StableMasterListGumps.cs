using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
    internal enum StablePetCollectionMode
    {
        SearchResults,
        Favorites,
        CustomList
    }

    internal enum StableGumpReturnTarget
    {
        Main,
        CustomLists,
        SearchResults,
        Favorites,
        CustomList,
        AddToCustomList
    }

    internal sealed class StableGumpReturnContext
    {
        private readonly StableGumpReturnTarget m_Target;
        private readonly int m_Page;
        private readonly string m_Query;
        private readonly int m_ListIndex;
        private readonly Serial m_PetSerial;
        private readonly StableGumpReturnContext m_BackContext;

        private StableGumpReturnContext( StableGumpReturnTarget target, int page, string query, int listIndex, Serial petSerial, StableGumpReturnContext backContext )
        {
            m_Target = target;
            m_Page = Math.Max( 0, page );
            m_Query = query == null ? String.Empty : query;
            m_ListIndex = listIndex;
            m_PetSerial = petSerial;
            m_BackContext = backContext;
        }

        public static StableGumpReturnContext Main( int page )
        {
            return new StableGumpReturnContext( StableGumpReturnTarget.Main, page, String.Empty, -1, Serial.Zero, null );
        }

        public static StableGumpReturnContext CustomLists( int page, StableGumpReturnContext backContext )
        {
            return new StableGumpReturnContext( StableGumpReturnTarget.CustomLists, page, String.Empty, -1, Serial.Zero, backContext );
        }

        public static StableGumpReturnContext SearchResults( string query, int page, StableGumpReturnContext backContext )
        {
            return new StableGumpReturnContext( StableGumpReturnTarget.SearchResults, page, query, -1, Serial.Zero, backContext );
        }

        public static StableGumpReturnContext Favorites( int page, StableGumpReturnContext backContext )
        {
            return new StableGumpReturnContext( StableGumpReturnTarget.Favorites, page, String.Empty, -1, Serial.Zero, backContext );
        }

        public static StableGumpReturnContext CustomList( int listIndex, int page, StableGumpReturnContext backContext )
        {
            return new StableGumpReturnContext( StableGumpReturnTarget.CustomList, page, String.Empty, listIndex, Serial.Zero, backContext );
        }

        public static StableGumpReturnContext AddToCustomList( Serial petSerial, int page, StableGumpReturnContext backContext )
        {
            return new StableGumpReturnContext( StableGumpReturnTarget.AddToCustomList, page, String.Empty, -1, petSerial, backContext );
        }

        public void Send( string title, Mobile from, StableMasterGumpOptions options )
        {
            if ( from == null )
                return;

            switch ( m_Target )
            {
                case StableGumpReturnTarget.CustomLists:
                    StableCustomListsGump.Show( title, from, options, m_Page, m_BackContext );
                    break;
                case StableGumpReturnTarget.SearchResults:
                    StablePetCollectionGump.ShowSearchResults( title, from, options, m_Query, m_Page, m_BackContext );
                    break;
                case StableGumpReturnTarget.Favorites:
                    StablePetCollectionGump.ShowFavorites( title, from, options, m_Page, m_BackContext );
                    break;
                case StableGumpReturnTarget.CustomList:
                    StablePetCollectionGump.ShowCustomList( title, from, options, m_ListIndex, m_Page, m_BackContext );
                    break;
                case StableGumpReturnTarget.AddToCustomList:
                    SendAddToCustomList( title, from, options );
                    break;
                default:
                    SendMain( from, options );
                    break;
            }
        }

        private void SendMain( Mobile from, StableMasterGumpOptions options )
        {
            if ( options != null && options.ReopenHandler != null )
                options.ReopenHandler( from, m_Page );
        }

        private void SendAddToCustomList( string title, Mobile from, StableMasterGumpOptions options )
        {
            PlayerMobile pm = from as PlayerMobile;
            BaseCreature pet = FindPet( pm, m_PetSerial );

            if ( pet != null )
                StableAddToCustomListGump.Show( title, from, pet, options, m_Page, m_BackContext );
            else if ( m_BackContext != null )
                m_BackContext.Send( title, from, options );
            else
                SendMain( from, options );
        }

        private static BaseCreature FindPet( PlayerMobile pm, Serial petSerial )
        {
            if ( pm == null || !StableMasterListState.IsUsableSerial( petSerial ) )
                return null;

            List<BaseCreature> pets = StableMasterGump.BuildSortedStablePetList( pm );

            for ( int i = 0; i < pets.Count; ++i )
            {
                BaseCreature pet = pets[i];

                if ( pet != null && pet.Serial == petSerial )
                    return pet;
            }

            return null;
        }
    }

    public class StablePetCollectionGump : Gump
    {
        private const int PetsPerPage = 5;
        private const int BackButtonID = 1;
        private const int PrevPageButtonID = 2;
        private const int NextPageButtonID = 3;
        private const int CompareButtonBase = 1000;
        private const int ClaimButtonBase = 2000;
        private const int FavoriteButtonBase = 3000;
        private const int ListButtonBase = 4000;

        private const int Width = 720;
        private const int Height = 550;
        private const int Padding = 20;
        private const int RowX = 25;
        private const int RowWidth = 670;
        private const int RowHeight = 74;
        private const int RowSpacing = 8;
        private const int RowStartY = 112;
        private const int FirstActionButtonX = RowX + 410;
        private const int FirstActionLabelX = RowX + 448;
        private const int SecondActionButtonX = RowX + 548;
        private const int SecondActionLabelX = RowX + 586;

        private readonly string m_Title;
        private readonly Mobile m_From;
        private readonly StableMasterGumpOptions m_Options;
        private readonly StablePetCollectionMode m_Mode;
        private readonly string m_Query;
        private readonly int m_ListIndex;
        private readonly int m_Page;
        private readonly IList<BaseCreature> m_List;
        private readonly StableGumpReturnContext m_BackContext;

        public static void ShowSearchResults( string title, Mobile from, StableMasterGumpOptions options, string query, int page )
        {
            ShowSearchResults( title, from, options, query, page, StableGumpReturnContext.Main( 0 ) );
        }

        internal static void ShowSearchResults( string title, Mobile from, StableMasterGumpOptions options, string query, int page, StableGumpReturnContext backContext )
        {
            if ( from == null )
                return;

            string normalized = StableMasterListState.NormalizeSearchTerm( query );
            List<BaseCreature> results = StableMasterGump.BuildSearchResultList( from, normalized );
            CloseSecondaryGumps( from );
            from.SendGump( new StablePetCollectionGump( title, from, options, StablePetCollectionMode.SearchResults, normalized, -1, results, page, backContext ) );
        }

        public static void ShowFavorites( string title, Mobile from, StableMasterGumpOptions options, int page )
        {
            ShowFavorites( title, from, options, page, StableGumpReturnContext.Main( 0 ) );
        }

        internal static void ShowFavorites( string title, Mobile from, StableMasterGumpOptions options, int page, StableGumpReturnContext backContext )
        {
            PlayerMobile pm = from as PlayerMobile;

            if ( pm == null )
                return;

            List<BaseCreature> results = StableMasterGump.BuildFavoritePetList( pm );
            CloseSecondaryGumps( from );
            from.SendGump( new StablePetCollectionGump( title, from, options, StablePetCollectionMode.Favorites, String.Empty, -1, results, page, backContext ) );
        }

        public static void ShowCustomList( string title, Mobile from, StableMasterGumpOptions options, int listIndex, int page )
        {
            ShowCustomList( title, from, options, listIndex, page, StableGumpReturnContext.Main( 0 ) );
        }

        internal static void ShowCustomList( string title, Mobile from, StableMasterGumpOptions options, int listIndex, int page, StableGumpReturnContext backContext )
        {
            PlayerMobile pm = from as PlayerMobile;

            if ( pm == null )
                return;

            List<BaseCreature> results = StableMasterGump.BuildCustomListPets( pm, listIndex );
            CloseSecondaryGumps( from );
            from.SendGump( new StablePetCollectionGump( title, from, options, StablePetCollectionMode.CustomList, String.Empty, listIndex, results, page, backContext ) );
        }

        internal static void CloseSecondaryGumps( Mobile from )
        {
            if ( from == null )
                return;

            from.CloseGump( typeof( StablePetCollectionGump ) );
            from.CloseGump( typeof( StableCustomListsGump ) );
            from.CloseGump( typeof( StableAddToCustomListGump ) );
        }

        private StablePetCollectionGump( string title, Mobile from, StableMasterGumpOptions options, StablePetCollectionMode mode, string query, int listIndex, IList<BaseCreature> list, int page, StableGumpReturnContext backContext ) : base( 70, 70 )
        {
            m_Title = String.IsNullOrEmpty( title ) ? "Stable Overview" : title;
            m_From = from;
            m_Options = options;
            m_Mode = mode;
            m_Query = query == null ? String.Empty : query;
            m_ListIndex = listIndex;
            m_List = list == null ? new List<BaseCreature>() : list;
            m_BackContext = backContext == null ? StableGumpReturnContext.Main( 0 ) : backContext;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            int totalPages = Math.Max( 1, (int)Math.Ceiling( m_List.Count / (double)PetsPerPage ) );
            m_Page = Math.Max( 0, Math.Min( page, totalPages - 1 ) );

            AddPage( 0 );
            AddBackground( 0, 0, Width, Height, 9270 );
            AddImageTiled( 10, 10, Width - 20, Height - 20, 2624 );
            AddAlphaRegion( 10, 10, Width - 20, Height - 20 );

            AddHtml( Padding, 24, Width - ( Padding * 2 ), 28, String.Format( "<CENTER><BASEFONT COLOR=#FFFFFF>{0}</BASEFONT></CENTER>", Utility.FixHtml( GetHeading() ) ), false, false );
            AddHtml( Padding, 54, Width - ( Padding * 2 ), 24, String.Format( "<BASEFONT COLOR=#9DC6FF>{0}</BASEFONT>", Utility.FixHtml( GetSubheading() ) ), false, false );

            AddButton( Width - 120, 78, 4014, 4016, BackButtonID, GumpButtonType.Reply, 0 );
            AddLabel( Width - 84, 80, 0x34, "Back" );

            AddLabel( Padding, 82, 0x34, String.Format( "Page {0} of {1}", m_Page + 1, totalPages ) );

            if ( m_Page > 0 )
                AddButton( 130, 80, 4014, 4016, PrevPageButtonID, GumpButtonType.Reply, 0 );

            if ( m_Page + 1 < totalPages )
                AddButton( 180, 80, 4005, 4007, NextPageButtonID, GumpButtonType.Reply, 0 );

            if ( m_List.Count == 0 )
            {
                AddHtml( Padding, RowStartY, Width - ( Padding * 2 ), 80, "<BASEFONT COLOR=#FFFFFF>No creatures are in this list.</BASEFONT>", false, false );
                return;
            }

            int startIndex = m_Page * PetsPerPage;

            for ( int i = 0; i < PetsPerPage; ++i )
            {
                int petIndex = startIndex + i;

                if ( petIndex >= m_List.Count )
                    break;

                DrawPetRow( m_List[petIndex], petIndex, RowStartY + ( i * ( RowHeight + RowSpacing ) ) );
            }
        }

        public override void OnResponse( NetState sender, RelayInfo info )
        {
            if ( sender == null || sender.Mobile != m_From || info == null )
                return;

            int buttonID = info.ButtonID;

            if ( buttonID == 0 )
            {
                ReturnToPreviousGump();
                return;
            }

            if ( buttonID == BackButtonID )
            {
                ReturnToPreviousGump();
                return;
            }

            if ( buttonID == PrevPageButtonID )
            {
                Reopen( Math.Max( m_Page - 1, 0 ) );
                return;
            }

            if ( buttonID == NextPageButtonID )
            {
                Reopen( m_Page + 1 );
                return;
            }

            if ( buttonID >= CompareButtonBase && buttonID < CompareButtonBase + m_List.Count )
            {
                int index = buttonID - CompareButtonBase;
                BaseCreature pet = GetPet( index );

                if ( pet != null )
                {
                    List<BaseCreature> compare = new List<BaseCreature>( 1 );
                    compare.Add( pet );
                    StablePetComparisonGump.Show( m_Title, m_From, compare, GetDisplayFlags() );
                }

                Reopen( m_Page );
                return;
            }

            if ( buttonID >= ClaimButtonBase && buttonID < ClaimButtonBase + m_List.Count )
            {
                int index = buttonID - ClaimButtonBase;
                BaseCreature pet = GetPet( index );

                if ( pet != null && m_Options != null && m_Options.ClaimHandler != null )
                    m_Options.ClaimHandler( m_From, pet );

                Reopen( m_Page );
                return;
            }

            if ( buttonID >= FavoriteButtonBase && buttonID < FavoriteButtonBase + m_List.Count )
            {
                int index = buttonID - FavoriteButtonBase;
                BaseCreature pet = GetPet( index );
                PlayerMobile pm = m_From as PlayerMobile;

                if ( pm != null && pet != null )
                    pm.StableListState.ToggleFavorite( pet.Serial );

                Reopen( m_Page );
                return;
            }

            if ( buttonID >= ListButtonBase && buttonID < ListButtonBase + m_List.Count )
            {
                int index = buttonID - ListButtonBase;
                BaseCreature pet = GetPet( index );

                if ( pet == null )
                {
                    Reopen( m_Page );
                    return;
                }

                if ( m_Mode == StablePetCollectionMode.CustomList )
                {
                    PlayerMobile pm = m_From as PlayerMobile;

                    if ( pm != null && m_ListIndex >= 0 && m_ListIndex < pm.StableListState.CustomLists.Count )
                        pm.StableListState.CustomLists[m_ListIndex].RemovePet( pet.Serial );

                    Reopen( m_Page );
                }
                else
                {
                    StableAddToCustomListGump.Show( m_Title, m_From, pet, m_Options, 0, GetCurrentContext() );
                }
            }
        }

        private void DrawPetRow( BaseCreature pet, int petIndex, int rowY )
        {
            AddImageTiled( RowX, rowY, RowWidth, RowHeight, 2624 );
            AddAlphaRegion( RowX, rowY, RowWidth, RowHeight );

            AddLabel( RowX + 15, rowY + 8, 0x2, StableMasterGump.GetCreatureLabel( pet ) );
            AddLabel( RowX + 15, rowY + 28, 0x58, StableMasterGump.GetDisplayName( pet ) );

            string summary = GetPetSummary( pet );
            AddHtml( RowX + 15, rowY + 50, 360, 18, String.Format( "<BASEFONT COLOR=#FFFFFF>{0}</BASEFONT>", Utility.FixHtml( summary ) ), false, false );

            AddButton( FirstActionButtonX, rowY + 8, 4005, 4007, CompareButtonBase + petIndex, GumpButtonType.Reply, 0 );
            AddLabel( FirstActionLabelX, rowY + 10, 0x34, "Compare" );

            AddButton( SecondActionButtonX, rowY + 8, 4005, 4007, ClaimButtonBase + petIndex, GumpButtonType.Reply, 0 );
            AddLabel( SecondActionLabelX, rowY + 10, 0x34, "Claim" );

            PlayerMobile pm = m_From as PlayerMobile;
            bool favorite = pm != null && pet != null && pm.StableListState.IsFavorite( pet.Serial );

            AddButton( FirstActionButtonX, rowY + 34, 4005, 4007, FavoriteButtonBase + petIndex, GumpButtonType.Reply, 0 );
            AddLabel( FirstActionLabelX, rowY + 36, 0x34, favorite ? "Unfavorite" : "Favorite" );

            AddButton( SecondActionButtonX, rowY + 34, 4005, 4007, ListButtonBase + petIndex, GumpButtonType.Reply, 0 );
            AddLabel( SecondActionLabelX, rowY + 36, 0x34, m_Mode == StablePetCollectionMode.CustomList ? "Remove" : "Lists" );
        }

        private BaseCreature GetPet( int index )
        {
            if ( index < 0 || index >= m_List.Count )
                return null;

            BaseCreature pet = m_List[index];

            if ( !StableMasterGump.IsCurrentStabledPet( m_From, pet ) )
                return null;

            return pet;
        }

        private string GetHeading()
        {
            if ( m_Mode == StablePetCollectionMode.SearchResults )
                return "Stable Search Results";

            if ( m_Mode == StablePetCollectionMode.Favorites )
                return "Stable Favorites";

            PlayerMobile pm = m_From as PlayerMobile;

            if ( pm != null && m_ListIndex >= 0 && m_ListIndex < pm.StableListState.CustomLists.Count )
                return pm.StableListState.CustomLists[m_ListIndex].Name;

            return "Stable Listing";
        }

        private string GetSubheading()
        {
            if ( m_Mode == StablePetCollectionMode.SearchResults )
                return String.Format( "{0} matching creature{1} for \"{2}\"", m_List.Count, m_List.Count == 1 ? String.Empty : "s", m_Query );

            if ( m_Mode == StablePetCollectionMode.Favorites )
                return String.Format( "{0} favorite creature{1}", m_List.Count, m_List.Count == 1 ? String.Empty : "s" );

            return String.Format( "{0} creature{1} in this listing", m_List.Count, m_List.Count == 1 ? String.Empty : "s" );
        }

        private string GetPetSummary( BaseCreature pet )
        {
            if ( pet == null )
                return String.Empty;

            return String.Format( "Level {0}/{1} | Slots {2} | Hue {3} | Serial {4}", pet.Level, pet.MaxLevel, pet.ControlSlots, pet.Hue, pet.Serial.Value );
        }

        private StableStatDisplayFlags GetDisplayFlags()
        {
            PlayerMobile pm = m_From as PlayerMobile;

            if ( pm != null )
                return pm.StableDisplayFlags;

            return StableStatDisplayFlags.All;
        }

        private void Reopen( int page )
        {
            if ( m_Mode == StablePetCollectionMode.SearchResults )
                ShowSearchResults( m_Title, m_From, m_Options, m_Query, page, m_BackContext );
            else if ( m_Mode == StablePetCollectionMode.Favorites )
                ShowFavorites( m_Title, m_From, m_Options, page, m_BackContext );
            else
                ShowCustomList( m_Title, m_From, m_Options, m_ListIndex, page, m_BackContext );
        }

        private StableGumpReturnContext GetCurrentContext()
        {
            if ( m_Mode == StablePetCollectionMode.SearchResults )
                return StableGumpReturnContext.SearchResults( m_Query, m_Page, m_BackContext );

            if ( m_Mode == StablePetCollectionMode.Favorites )
                return StableGumpReturnContext.Favorites( m_Page, m_BackContext );

            return StableGumpReturnContext.CustomList( m_ListIndex, m_Page, m_BackContext );
        }

        private void ReturnToPreviousGump()
        {
            m_BackContext.Send( m_Title, m_From, m_Options );
        }
    }

    public class StableCustomListsGump : Gump
    {
        private const int CreateButtonID = 1;
        private const int PrevPageButtonID = 2;
        private const int NextPageButtonID = 3;
        private const int BackButtonID = 4;
        private const int NameEntryID = 1;
        private const int OpenButtonBase = 1000;
        private const int DeleteButtonBase = 2000;

        private const int ListsPerPage = 6;
        private const int Width = 560;
        private const int Height = 450;
        private const int RowStartY = 126;
        private const int RowHeight = 42;

        private readonly string m_Title;
        private readonly PlayerMobile m_Player;
        private readonly StableMasterGumpOptions m_Options;
        private readonly int m_Page;
        private readonly StableGumpReturnContext m_BackContext;

        public static void Show( string title, Mobile from, StableMasterGumpOptions options, int page )
        {
            Show( title, from, options, page, StableGumpReturnContext.Main( 0 ) );
        }

        internal static void Show( string title, Mobile from, StableMasterGumpOptions options, int page, StableGumpReturnContext backContext )
        {
            PlayerMobile pm = from as PlayerMobile;

            if ( pm == null )
                return;

            StablePetCollectionGump.CloseSecondaryGumps( pm );
            pm.SendGump( new StableCustomListsGump( title, pm, options, page, backContext ) );
        }

        private StableCustomListsGump( string title, PlayerMobile player, StableMasterGumpOptions options, int page, StableGumpReturnContext backContext ) : base( 90, 90 )
        {
            m_Title = String.IsNullOrEmpty( title ) ? "Stable Overview" : title;
            m_Player = player;
            m_Options = options;
            m_BackContext = backContext == null ? StableGumpReturnContext.Main( 0 ) : backContext;

            StableMasterGump.PruneStableListState( player );
            player.CloseGump( typeof( StableCustomListsGump ) );

            int totalPages = Math.Max( 1, (int)Math.Ceiling( player.StableListState.CustomLists.Count / (double)ListsPerPage ) );
            m_Page = Math.Max( 0, Math.Min( page, totalPages - 1 ) );

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage( 0 );
            AddBackground( 0, 0, Width, Height, 9270 );
            AddImageTiled( 10, 10, Width - 20, Height - 20, 2624 );
            AddAlphaRegion( 10, 10, Width - 20, Height - 20 );

            AddHtml( 20, 24, Width - 40, 28, "<CENTER><BASEFONT COLOR=#FFFFFF>Stable Listings</BASEFONT></CENTER>", false, false );
            AddHtml( 20, 54, Width - 40, 24, String.Format( "<BASEFONT COLOR=#9DC6FF>{0} of {1} listings used</BASEFONT>", player.StableListState.CustomLists.Count, StableMasterListState.MaxCustomLists ), false, false );

            AddLabel( 20, 84, 0x34, "New listing" );
            AddTextEntry( 130, 82, 210, 22, 0x34, NameEntryID, String.Empty );
            AddButton( 350, 82, 4005, 4007, CreateButtonID, GumpButtonType.Reply, 0 );
            AddLabel( 388, 84, 0x34, "Create" );

            AddButton( Width - 108, 82, 4014, 4016, BackButtonID, GumpButtonType.Reply, 0 );
            AddLabel( Width - 72, 84, 0x34, "Back" );

            AddLabel( 20, Height - 40, 0x34, String.Format( "Page {0} of {1}", m_Page + 1, totalPages ) );

            if ( m_Page > 0 )
                AddButton( 130, Height - 42, 4014, 4016, PrevPageButtonID, GumpButtonType.Reply, 0 );

            if ( m_Page + 1 < totalPages )
                AddButton( 180, Height - 42, 4005, 4007, NextPageButtonID, GumpButtonType.Reply, 0 );

            DrawLists();
        }

        public override void OnResponse( NetState sender, RelayInfo info )
        {
            if ( sender == null || sender.Mobile != m_Player || info == null )
                return;

            int buttonID = info.ButtonID;

            if ( buttonID == 0 )
            {
                ReturnToPreviousGump();
                return;
            }

            if ( buttonID == BackButtonID )
            {
                ReturnToPreviousGump();
                return;
            }

            if ( buttonID == PrevPageButtonID )
            {
                Show( m_Title, m_Player, m_Options, Math.Max( m_Page - 1, 0 ), m_BackContext );
                return;
            }

            if ( buttonID == NextPageButtonID )
            {
                Show( m_Title, m_Player, m_Options, m_Page + 1, m_BackContext );
                return;
            }

            if ( buttonID == CreateButtonID )
            {
                TextRelay relay = info.GetTextEntry( NameEntryID );
                string name = relay == null ? String.Empty : relay.Text;
                StableCustomPetList created = m_Player.StableListState.GetOrCreateList( name );

                if ( created == null )
                    m_Player.SendMessage( "Enter a unique listing name. You may have up to {0} listings.", StableMasterListState.MaxCustomLists );
                else
                    m_Player.SendMessage( "Stable listing \"{0}\" is ready.", created.Name );

                Show( m_Title, m_Player, m_Options, m_Page, m_BackContext );
                return;
            }

            if ( buttonID >= OpenButtonBase && buttonID < OpenButtonBase + m_Player.StableListState.CustomLists.Count )
            {
                int index = buttonID - OpenButtonBase;
                StablePetCollectionGump.ShowCustomList( m_Title, m_Player, m_Options, index, 0, GetCurrentContext() );
                return;
            }

            if ( buttonID >= DeleteButtonBase && buttonID < DeleteButtonBase + m_Player.StableListState.CustomLists.Count )
            {
                int index = buttonID - DeleteButtonBase;
                m_Player.StableListState.RemoveListAt( index );
                Show( m_Title, m_Player, m_Options, m_Page, m_BackContext );
            }
        }

        private void DrawLists()
        {
            int start = m_Page * ListsPerPage;

            if ( m_Player.StableListState.CustomLists.Count == 0 )
            {
                AddHtml( 20, RowStartY, Width - 40, 40, "<BASEFONT COLOR=#FFFFFF>No custom listings have been created.</BASEFONT>", false, false );
                return;
            }

            for ( int i = 0; i < ListsPerPage; ++i )
            {
                int listIndex = start + i;

                if ( listIndex >= m_Player.StableListState.CustomLists.Count )
                    break;

                StableCustomPetList list = m_Player.StableListState.CustomLists[listIndex];
                int y = RowStartY + ( i * RowHeight );

                AddImageTiled( 20, y, Width - 40, RowHeight - 6, 2624 );
                AddAlphaRegion( 20, y, Width - 40, RowHeight - 6 );

                AddButton( 34, y + 7, 4005, 4007, OpenButtonBase + listIndex, GumpButtonType.Reply, 0 );
                AddLabel( 72, y + 9, 0x34, "Open" );

                AddHtml( 145, y + 8, 220, 22, String.Format( "<BASEFONT COLOR=#FFFFFF>{0}</BASEFONT>", Utility.FixHtml( list.Name ) ), false, false );
                AddLabel( 380, y + 9, 0x58, String.Format( "{0} pets", list.PetSerials.Count ) );

                AddButton( 452, y + 7, 4017, 4019, DeleteButtonBase + listIndex, GumpButtonType.Reply, 0 );
                AddLabel( 488, y + 9, 0x34, "Delete" );
            }
        }

        private StableGumpReturnContext GetCurrentContext()
        {
            return StableGumpReturnContext.CustomLists( m_Page, m_BackContext );
        }

        private void ReturnToPreviousGump()
        {
            m_BackContext.Send( m_Title, m_Player, m_Options );
        }
    }

    public class StableAddToCustomListGump : Gump
    {
        private const int ManageButtonID = 1;
        private const int CloseButtonID = 2;
        private const int PrevPageButtonID = 3;
        private const int NextPageButtonID = 4;
        private const int ToggleButtonBase = 1000;

        private const int ListsPerPage = 8;
        private const int Width = 540;
        private const int Height = 430;
        private const int RowStartY = 118;
        private const int RowHeight = 34;

        private readonly string m_Title;
        private readonly PlayerMobile m_Player;
        private readonly StableMasterGumpOptions m_Options;
        private readonly Serial m_PetSerial;
        private readonly int m_Page;
        private readonly StableGumpReturnContext m_BackContext;

        public static void Show( string title, Mobile from, BaseCreature pet, StableMasterGumpOptions options, int page )
        {
            Show( title, from, pet, options, 0, StableGumpReturnContext.Main( page ) );
        }

        internal static void Show( string title, Mobile from, BaseCreature pet, StableMasterGumpOptions options, int page, StableGumpReturnContext backContext )
        {
            PlayerMobile pm = from as PlayerMobile;

            if ( pm == null || pet == null || !StableMasterGump.IsCurrentStabledPet( from, pet ) )
                return;

            StablePetCollectionGump.CloseSecondaryGumps( pm );
            pm.SendGump( new StableAddToCustomListGump( title, pm, pet.Serial, options, page, backContext ) );
        }

        private StableAddToCustomListGump( string title, PlayerMobile player, Serial petSerial, StableMasterGumpOptions options, int page, StableGumpReturnContext backContext ) : base( 110, 110 )
        {
            m_Title = String.IsNullOrEmpty( title ) ? "Stable Overview" : title;
            m_Player = player;
            m_PetSerial = petSerial;
            m_Options = options;
            m_BackContext = backContext == null ? StableGumpReturnContext.Main( 0 ) : backContext;

            StableMasterGump.PruneStableListState( player );
            player.CloseGump( typeof( StableAddToCustomListGump ) );
            int totalPages = Math.Max( 1, (int)Math.Ceiling( player.StableListState.CustomLists.Count / (double)ListsPerPage ) );
            m_Page = Math.Max( 0, Math.Min( page, totalPages - 1 ) );

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage( 0 );
            AddBackground( 0, 0, Width, Height, 9270 );
            AddImageTiled( 10, 10, Width - 20, Height - 20, 2624 );
            AddAlphaRegion( 10, 10, Width - 20, Height - 20 );

            BaseCreature pet = FindPet();
            string petName = pet == null ? "Unknown creature" : StableMasterGump.GetDisplayName( pet );

            AddHtml( 20, 24, Width - 40, 28, "<CENTER><BASEFONT COLOR=#FFFFFF>Add To Stable Listing</BASEFONT></CENTER>", false, false );
            AddHtml( 20, 54, Width - 40, 24, String.Format( "<BASEFONT COLOR=#9DC6FF>{0}</BASEFONT>", Utility.FixHtml( petName ) ), false, false );

            AddButton( 20, 82, 4005, 4007, ManageButtonID, GumpButtonType.Reply, 0 );
            AddLabel( 58, 84, 0x34, "Manage listings" );

            AddButton( Width - 108, 82, 4014, 4016, CloseButtonID, GumpButtonType.Reply, 0 );
            AddLabel( Width - 72, 84, 0x34, "Close" );

            DrawLists();
        }

        public override void OnResponse( NetState sender, RelayInfo info )
        {
            if ( sender == null || sender.Mobile != m_Player || info == null )
                return;

            int buttonID = info.ButtonID;

            if ( buttonID == 0 )
            {
                ReturnToPreviousGump();
                return;
            }

            if ( buttonID == CloseButtonID )
                return;

            if ( buttonID == ManageButtonID )
            {
                StableCustomListsGump.Show( m_Title, m_Player, m_Options, 0, GetCurrentContext() );
                return;
            }

            if ( buttonID == PrevPageButtonID )
            {
                Reopen( Math.Max( m_Page - 1, 0 ) );
                return;
            }

            if ( buttonID == NextPageButtonID )
            {
                Reopen( m_Page + 1 );
                return;
            }

            if ( buttonID >= ToggleButtonBase && buttonID < ToggleButtonBase + m_Player.StableListState.CustomLists.Count )
            {
                BaseCreature pet = FindPet();

                if ( pet == null )
                    return;

                int index = buttonID - ToggleButtonBase;
                StableCustomPetList list = m_Player.StableListState.CustomLists[index];

                if ( list.ContainsPet( m_PetSerial ) )
                {
                    list.RemovePet( m_PetSerial );
                    m_Player.SendMessage( "Removed from \"{0}\".", list.Name );
                }
                else if ( list.AddPet( m_PetSerial ) )
                {
                    m_Player.SendMessage( "Added to \"{0}\".", list.Name );
                }
                else
                {
                    m_Player.SendMessage( "That listing is full." );
                }

                Reopen( m_Page );
            }
        }

        private void DrawLists()
        {
            if ( m_Player.StableListState.CustomLists.Count == 0 )
            {
                AddHtml( 20, RowStartY, Width - 40, 60, "<BASEFONT COLOR=#FFFFFF>Create a stable listing first, then return here to add this creature.</BASEFONT>", false, false );
                return;
            }

            int totalPages = Math.Max( 1, (int)Math.Ceiling( m_Player.StableListState.CustomLists.Count / (double)ListsPerPage ) );
            int start = m_Page * ListsPerPage;
            int maxRows = Math.Min( ListsPerPage, m_Player.StableListState.CustomLists.Count - start );

            for ( int i = 0; i < maxRows; ++i )
            {
                int listIndex = start + i;
                StableCustomPetList list = m_Player.StableListState.CustomLists[listIndex];
                int y = RowStartY + ( i * RowHeight );
                bool contains = list.ContainsPet( m_PetSerial );

                AddImageTiled( 20, y, Width - 40, RowHeight - 4, 2624 );
                AddAlphaRegion( 20, y, Width - 40, RowHeight - 4 );

                AddButton( 34, y + 5, contains ? 4017 : 4005, contains ? 4019 : 4007, ToggleButtonBase + listIndex, GumpButtonType.Reply, 0 );
                AddLabel( 72, y + 7, 0x34, contains ? "Remove" : "Add" );
                AddHtml( 150, y + 7, 250, 20, String.Format( "<BASEFONT COLOR=#FFFFFF>{0}</BASEFONT>", Utility.FixHtml( list.Name ) ), false, false );
                AddLabel( 410, y + 7, 0x58, String.Format( "{0} pets", list.PetSerials.Count ) );
            }

            if ( totalPages > 1 )
            {
                AddLabel( 20, Height - 34, 0x34, String.Format( "Page {0} of {1}", m_Page + 1, totalPages ) );

                if ( m_Page > 0 )
                    AddButton( 130, Height - 36, 4014, 4016, PrevPageButtonID, GumpButtonType.Reply, 0 );

                if ( m_Page + 1 < totalPages )
                    AddButton( 180, Height - 36, 4005, 4007, NextPageButtonID, GumpButtonType.Reply, 0 );
            }
        }

        private BaseCreature FindPet()
        {
            List<BaseCreature> pets = StableMasterGump.BuildSortedStablePetList( m_Player );

            for ( int i = 0; i < pets.Count; ++i )
            {
                BaseCreature pet = pets[i];

                if ( pet != null && pet.Serial == m_PetSerial )
                    return pet;
            }

            return null;
        }

        private StableGumpReturnContext GetCurrentContext()
        {
            return StableGumpReturnContext.AddToCustomList( m_PetSerial, m_Page, m_BackContext );
        }

        private void Reopen( int page )
        {
            BaseCreature pet = FindPet();

            if ( pet != null )
                Show( m_Title, m_Player, pet, m_Options, page, m_BackContext );
            else
                ReturnToPreviousGump();
        }

        private void ReturnToPreviousGump()
        {
            m_BackContext.Send( m_Title, m_Player, m_Options );
        }
    }
}
