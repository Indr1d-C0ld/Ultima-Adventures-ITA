using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Server;
using Server.Mobiles;
using Server.Items;
using Server.Network;
using Server.HuePickers;
using Server.Multis;

namespace Server.Gumps
{
	public class PlayerVendorBuyGump : Gump
	{
		private PlayerVendor m_Vendor;
		private VendorItem m_VI;

		public PlayerVendorBuyGump( PlayerVendor vendor, VendorItem vi ) : base( 100, 200 )
		{
			m_Vendor = vendor;
			m_VI = vi;

			AddBackground( 100, 10, 300, 150, 5054 );

			AddHtmlLocalized( 125, 20, 250, 24, 1019070, false, false ); // You have agreed to purchase:

			if ( !String.IsNullOrEmpty( vi.Description ) )
				AddLabel( 125, 45, 0, vi.Description );
			else
				AddHtmlLocalized( 125, 45, 250, 24, 1019072, false, false ); // an item without a description

			AddHtmlLocalized( 125, 70, 250, 24, 1019071, false, false ); // for the amount of:
			AddLabel( 125, 95, 0, vi.Price.ToString() );

			AddButton( 250, 130, 4005, 4007, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 282, 130, 100, 24, 1011012, false, false ); // CANCEL

			AddButton( 120, 130, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 152, 130, 100, 24, 1011036, false, false ); // OKAY
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;

			if ( !m_Vendor.CanInteractWith( from, false ) )
				return;

			if ( m_Vendor.IsOwner( from ) )
			{
				m_Vendor.SayTo( from, 503212 ); // You own this shop, just take what you want.
				return;
			}

			if ( info.ButtonID == 1 )
			{
				m_Vendor.Say( from.Name );

				if ( !m_VI.Valid || !m_VI.Item.IsChildOf( m_Vendor.Backpack ) )
				{
					m_Vendor.SayTo( from, 503216 ); // You can't buy that.
					return;
				}

				int totalGold = 0;

				if ( from.Backpack != null )
					totalGold += from.Backpack.GetAmount( typeof( Gold ) );
				
				totalGold += Banker.GetBalance( from );

				if ( totalGold < m_VI.Price )
				{
					m_Vendor.SayTo( from, 503205 ); // You cannot afford this item.
				}
				else if ( !from.PlaceInBackpack( m_VI.Item ) )
				{
					m_Vendor.SayTo( from, 503204 ); // You do not have room in your backpack for this.
				}
				else
				{
					int leftPrice = m_VI.Price;

					if ( from.Backpack != null )
						leftPrice -= from.Backpack.ConsumeUpTo( typeof( Gold ), leftPrice );

					if ( leftPrice > 0 )
						Banker.Withdraw( from, leftPrice );

                                        int payout = m_Vendor.CalculateSalePayout( m_VI.Price );

                                        m_Vendor.HoldGold += payout;

                                        m_Vendor.RecordPurchase( from, m_VI.Item, payout, m_VI.Price );

                                        from.SendLocalizedMessage( 503201 ); // You take the item.
                                }
                        }
                        else
                        {
				from.SendLocalizedMessage( 503207 ); // Cancelled purchase.
			}
		}
	}

	public class PlayerVendorOwnerGump : Gump
	{
		private PlayerVendor m_Vendor;

		public PlayerVendorOwnerGump( PlayerVendor vendor ) : base( 50, 200 )
		{
			m_Vendor = vendor;

			int perDay = m_Vendor.ChargePerDay;

			AddPage( 0 );
			AddBackground( 25, 10, 530, 140, 5054 );

			AddHtmlLocalized( 425, 25, 120, 20, 1019068, false, false ); // See goods
			AddButton( 390, 25, 4005, 4007, 1, GumpButtonType.Reply, 0 );
                        AddHtmlLocalized( 425, 48, 120, 20, 1019069, false, false ); // Customize
                        AddButton( 390, 48, 4005, 4007, 2, GumpButtonType.Reply, 0 );
                        AddHtml( 425, 72, 120, 20, "Purchase History", false, false );
                        AddButton( 390, 71, 4005, 4007, 3, GumpButtonType.Reply, 0 );
                        AddHtmlLocalized( 425, 96, 120, 20, 1011012, false, false ); // CANCEL
                        AddButton( 390, 95, 4005, 4007, 0, GumpButtonType.Reply, 0 );

			AddHtmlLocalized( 40, 72, 260, 20, 1038321, false, false ); // Gold held for you:
			AddLabel( 300, 72, 0, m_Vendor.HoldGold.ToString() );
			AddHtmlLocalized( 40, 96, 260, 20, 1038322, false, false ); // Gold held in my account:
			AddLabel( 300, 96, 0, m_Vendor.BankAccount.ToString() );

			//AddHtmlLocalized( 40, 120, 260, 20, 1038324, false, false ); // My charge per day is:
			// Localization has changed, we must use a string here
			AddHtml( 40, 120, 260, 20, "My charge per day is:", false, false );
			AddLabel( 300, 120, 0, perDay.ToString() );

			double days = (m_Vendor.HoldGold + m_Vendor.BankAccount) / ((double)perDay);

			AddHtmlLocalized( 40, 25, 260, 20, 1038318, false, false ); // Amount of days I can work: 
			AddLabel( 300, 25, 0, ((int)days).ToString() );
			AddHtmlLocalized( 40, 48, 260, 20, 1038319, false, false ); // Earth days: 
			AddLabel( 300, 48, 0, ((int)(days / 12.0)).ToString() );
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;

			if ( !m_Vendor.CanInteractWith( from, true ) )
				return;

                        switch ( info.ButtonID )
                        {
                                case 1:
                                {
                                        m_Vendor.OpenBackpack( from );

                                        break;
                                }
                                case 2:
                                {
                                        from.SendGump( new PlayerVendorCustomizeGump( m_Vendor, from ) );

                                        break;
                                }
                                case 3:
                                {
                                        from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, from ) );

                                        break;
                                }
                        }
		}
	}

	public class NewPlayerVendorOwnerGump : Gump
	{
		private PlayerVendor m_Vendor;

		public NewPlayerVendorOwnerGump( PlayerVendor vendor ) : base( 50, 200 )
		{
			m_Vendor = vendor;

			int perRealWorldDay = vendor.ChargePerRealWorldDay;
			int goldHeld = vendor.HoldGold;

                        AddBackground( 25, 10, 530, 200, 0x13BE );

                        AddImageTiled( 35, 20, 510, 180, 0xA40 );
                        AddAlphaRegion( 35, 20, 510, 180 );

                        AddImage( 10, 0, 0x28DC );
                        AddImage( 537, 195, 0x28DC );
                        AddImage( 10, 195, 0x28DC );
                        AddImage( 537, 0, 0x28DC );

			if ( goldHeld < perRealWorldDay )
			{
				int goldNeeded = perRealWorldDay - goldHeld;

				AddHtmlLocalized( 40, 35, 260, 20, 1038320, 0x7FFF, false, false ); // Gold needed for 1 day of vendor salary: 
				AddLabel( 300, 35, 0x1F, goldNeeded.ToString() );
			}
			else
			{
				int days = goldHeld / perRealWorldDay;

				AddHtmlLocalized( 40, 35, 260, 20, 1038318, 0x7FFF, false, false ); // # of days Vendor salary is paid for: 
				AddLabel( 300, 35, 0x480, days.ToString() );
			}

			AddHtmlLocalized( 40, 58, 260, 20, 1038324, 0x7FFF, false, false ); // My charge per real world day is: 
			AddLabel( 300, 58, 0x480, perRealWorldDay.ToString() );

			AddHtmlLocalized( 40, 82, 260, 20, 1038322, 0x7FFF, false, false ); // Gold held in my account: 
			AddLabel( 300, 82, 0x480, goldHeld.ToString() );

			AddHtmlLocalized( 40, 108, 260, 20, 1062509, 0x7FFF, false, false ); // Shop Name:
			AddLabel( 140, 106, 0x66D, vendor.ShopName );

			if ( vendor is RentedVendor )
			{
				int days, hours;
				((RentedVendor)vendor).ComputeRentalExpireDelay( out days, out hours );

				AddLabel( 38, 132, 0x480, String.Format( "Location rental will expire in {0} day{1} and {2} hour{3}.", days, days != 1 ? "s" : "", hours, hours != 1 ? "s" : "" ) );
			}

			AddButton( 390, 24, 0x15E1, 0x15E5, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 408, 21, 120, 20, 1019068, 0x7FFF, false, false ); // See goods

			AddButton( 390, 44, 0x15E1, 0x15E5, 2, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 408, 41, 120, 20, 1019069, 0x7FFF, false, false ); // Customize

			AddButton( 390, 64, 0x15E1, 0x15E5, 3, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 408, 61, 120, 20, 1062434, 0x7FFF, false, false ); // Rename Shop

			AddButton( 390, 84, 0x15E1, 0x15E5, 4, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 408, 81, 120, 20, 3006217, 0x7FFF, false, false ); // Rename Vendor

			AddButton( 390, 104, 0x15E1, 0x15E5, 5, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 408, 101, 120, 20, 3006123, 0x7FFF, false, false ); // Open Paperdoll

                        AddButton( 390, 124, 0x15E1, 0x15E5, 6, GumpButtonType.Reply, 0 );
                        AddLabel( 408, 121, 0x480, "Collect Gold" );

                        AddButton( 390, 144, 0x15E1, 0x15E5, 7, GumpButtonType.Reply, 0 );
                        AddLabel( 408, 141, 0x480, "Purchase History" );

                        AddButton( 390, 164, 0x15E1, 0x15E5, 8, GumpButtonType.Reply, 0 );
                        AddLabel( 408, 161, 0x480, "Dismiss Vendor" );

                        AddButton( 390, 184, 0x15E1, 0x15E5, 0, GumpButtonType.Reply, 0 );
                        AddHtmlLocalized( 408, 181, 120, 20, 1011012, 0x7FFF, false, false ); // CANCEL
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;

			if ( info.ButtonID == 1 || info.ButtonID == 2 ) // See goods or Customize
				m_Vendor.CheckTeleport( from );

			if ( !m_Vendor.CanInteractWith( from, true ) )
				return;

			switch ( info.ButtonID )
			{
				case 1: // See goods
				{
					m_Vendor.OpenBackpack( from );

					break;
				}
				case 2: // Customize
				{
					from.SendGump( new NewPlayerVendorCustomizeGump( m_Vendor ) );

					break;
				}
				case 3: // Rename Shop
				{
					m_Vendor.RenameShop( from );

					break;
				}
				case 4: // Rename Vendor
				{
					m_Vendor.Rename( from );

					break;
				}
				case 5: // Open Paperdoll
				{
					m_Vendor.DisplayPaperdollTo( from );

					break;
				}
                                case 6: // Collect Gold
                                {
                                        m_Vendor.CollectGold( from );

                                        break;
                                }
                                case 7: // Purchase History
                                {
                                        from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, from ) );

                                        break;
                                }
                                case 8: // Dismiss Vendor
                                {
                                        m_Vendor.Dismiss( from );

                                        break;
                                }
                        }
                }
        }

        public class PlayerVendorPurchaseHistoryGump : Gump
        {
private const int BtnLightDark = 1;
private const int BtnBackground = 2;
private const int BtnSort = 3;
private const int BtnLowStock = 4;
private const int BtnSize = 5;
private const int BtnAvg = 6;
private const int BtnToday = 7;
private const int Btn7d = 8;
private const int Btn30d = 9;
private const int BtnAll = 10;
private const int BtnPrev = 11;
private const int BtnNext = 12;
private const int BtnPresentation = 13;
private const int BtnExpandedPrev = 14;
private const int BtnExpandedNext = 15;
private const int BtnGroupBase = 1000;
private const int ModernInnerPadding = 20;
private const int ModernContentY = 135;
private const int ModernRowHeight = 104;
private const int ModernPurchaseRowHeight = 104;
private const int ModernItemSquareSize = 88;

                private PlayerVendor m_Vendor;
                private int m_Page;
                private bool m_Dark;
                private bool m_SortPrice;
private bool m_SortAsc;
private int m_Expanded;
private int m_Size;
private bool m_ShowAvg;
private int m_Filter;
private int m_Back;
                private int m_ExpandedPage;
                private Mobile m_Viewer;

                private static readonly int[] Widths = { 600, 795, 990, 1185 };
                private static readonly int[] Heights = { 480, 630, 780, 930 };
private static readonly string[] SizeNames = { "XS", "S", "L", "XL" };

public PlayerVendorPurchaseHistoryGump( PlayerVendor vendor ) : this( vendor, vendor != null ? vendor.Owner : null )
{
}

public PlayerVendorPurchaseHistoryGump( PlayerVendor vendor, Mobile viewer ) : this( vendor, 0, vendor.PurchaseHistoryDark, vendor.PurchaseHistoryBack, vendor.PurchaseHistorySortPrice, vendor.PurchaseHistorySortAsc, -1, vendor.PurchaseHistorySize, vendor.PurchaseHistoryShowAvg, vendor.PurchaseHistoryFilter, viewer )
{
}

public PlayerVendorPurchaseHistoryGump( PlayerVendor vendor, int page, bool dark, int back, bool sortPrice, bool sortAsc, int expanded, int size, bool showAvg, int filter ) : this( vendor, page, dark, back, sortPrice, sortAsc, expanded, size, showAvg, filter, vendor != null ? vendor.Owner : null )
{
}

public PlayerVendorPurchaseHistoryGump( PlayerVendor vendor, int page, bool dark, int back, bool sortPrice, bool sortAsc, int expanded, int size, bool showAvg, int filter, Mobile viewer ) : base( 50, 50 )
{
                        Initialize( vendor, page, dark, back, sortPrice, sortAsc, expanded, size, showAvg, filter, 0, viewer );
}

private PlayerVendorPurchaseHistoryGump( PlayerVendor vendor, int page, bool dark, int back, bool sortPrice, bool sortAsc, int expanded, int size, bool showAvg, int filter, int expandedPage, Mobile viewer ) : base( 50, 50 )
{
                        Initialize( vendor, page, dark, back, sortPrice, sortAsc, expanded, size, showAvg, filter, expandedPage, viewer );
}

private void Initialize( PlayerVendor vendor, int page, bool dark, int back, bool sortPrice, bool sortAsc, int expanded, int size, bool showAvg, int filter, int expandedPage, Mobile viewer )
{
m_Vendor = vendor;
m_Page = page;
m_Dark = dark;
m_Back = back >= 0 && back <= 2 ? back : 0;
m_SortPrice = sortPrice;
m_SortAsc = sortAsc;
m_Expanded = expanded;
m_Size = size >= 0 && size < Widths.Length ? size : 1;
m_ShowAvg = showAvg;
m_Filter = filter >= 0 && filter <= 3 ? filter : 2;
m_ExpandedPage = Math.Max( 0, expandedPage );
m_Viewer = viewer;

                        if ( m_Vendor == null || m_Vendor.Deleted )
                                return;

                        Closable = true;
                        Disposable = true;
                        Dragable = true;
                        Resizable = false;

                        int width = Widths[m_Size];
                        int height = Heights[m_Size];
                        int lineHeight = 40;

                        AddPage( 0 );

                        if ( !m_Vendor.PurchaseHistoryUseClassic )
                        {
                                List<List<PlayerVendor.PurchaseRecord>> modernGroups = PrepareGroups();
                                int modernPerPage = GetPerPage( height, false );
                                int modernMaxPage = Math.Max( 1, ( modernGroups.Count + modernPerPage - 1 ) / modernPerPage );

                                if ( m_Page < 0 )
                                        m_Page = 0;
                                else if ( m_Page >= modernMaxPage )
                                        m_Page = modernMaxPage - 1;

                                DrawModern( modernGroups, width, height, modernPerPage, modernMaxPage );
                                return;
                        }

                        DrawClassic( width, height );

                        List<List<PlayerVendor.PurchaseRecord>> groups = PrepareGroups();

                        int perPage = ( height - 160 ) / lineHeight;
                        int maxPage = ( groups.Count + perPage - 1 ) / perPage;
                        if ( maxPage < 1 )
                                maxPage = 1;
                        if ( m_Page >= maxPage )
                                m_Page = maxPage - 1;

                        int start = m_Page * perPage;
                        int end = Math.Min( start + perPage, groups.Count );

                        int y = 80;

                        for ( int i = start; i < end; i++ )
                        {
                                List<PlayerVendor.PurchaseRecord> grp = groups[i];
                                PlayerVendor.PurchaseRecord first = grp[0];
                                bool isExpanded = ( m_Expanded == i );

                                int arrow = grp.Count > 1 ? ( isExpanded ? 0x15E1 : 0x15E5 ) : 0;
                                int arrowPress = grp.Count > 1 ? ( isExpanded ? 0x15E5 : 0x15E1 ) : 0;

                                if ( grp.Count > 1 )
                                        AddButton( 40, y + 10, arrow, arrowPress, BtnGroupBase + i, GumpButtonType.Reply, 0 );

                                AddItem( 60, y + 10, first.ItemID );
                                AddTooltipFromPurchaseRecord( first );
                                string entry = FormatEntry( first );
                                if ( grp.Count > 1 && !isExpanded )
                                        entry += String.Format( " ×{0} listings.", grp.Count );

                                AddHtml( 100, y, width - 120, lineHeight, FormatText( entry ), false, false );

                                if ( isExpanded && grp.Count > 1 )
                                {
                                        for ( int j = 0; j < grp.Count; j++ )
                                        {
                                                PlayerVendor.PurchaseRecord pr = grp[j];
                                                y += lineHeight;
                                                AddItem( 80, y + 10, pr.ItemID );
                                                AddTooltipFromPurchaseRecord( pr );
                                                AddHtml( 120, y, width - 140, lineHeight, FormatText( FormatEntry( pr ) ), false, false );
                                        }
                                }

                                y += lineHeight;
                        }

                        int filterY = height - 60;
                        DrawDateFilters( filterY, false );

                        // pagination buttons
                        if ( m_Page > 0 )
                                AddButton( width - 90, height - 30, 0x15E3, 0x15E7, BtnPrev, GumpButtonType.Reply, 0 );
                        if ( m_Page < maxPage - 1 )
                                AddButton( width - 60, height - 30, 0x15E1, 0x15E5, BtnNext, GumpButtonType.Reply, 0 );

                        AddLabel( width - 120, height - 50, m_Dark ? 0x481 : 0, String.Format( "{0}/{1}", m_Page + 1, maxPage ) );
                }

                private List<List<PlayerVendor.PurchaseRecord>> PrepareGroups()
                {
                        List<PlayerVendor.PurchaseRecord> records = new List<PlayerVendor.PurchaseRecord>( m_Vendor.PurchaseHistory );

                        if ( m_Filter != 3 )
                        {
                                DateTime now = DateTime.UtcNow;
                                DateTime cutoff;

                                switch ( m_Filter )
                                {
                                        case 0: cutoff = now.AddDays( -1 ); break;
                                        case 1: cutoff = now.AddDays( -7 ); break;
                                        default: cutoff = now.AddDays( -30 ); break;
                                }

                                records = records.FindAll( delegate( PlayerVendor.PurchaseRecord record )
                                {
                                        return record != null && record.Time >= cutoff;
                                } );
                        }

                        if ( m_SortPrice )
                        {
                                if ( m_SortAsc )
                                        records.Sort( delegate( PlayerVendor.PurchaseRecord a, PlayerVendor.PurchaseRecord b ) { return a.Price.CompareTo( b.Price ); } );
                                else
                                        records.Sort( delegate( PlayerVendor.PurchaseRecord a, PlayerVendor.PurchaseRecord b ) { return b.Price.CompareTo( a.Price ); } );
                        }
                        else
                        {
                                if ( m_SortAsc )
                                        records.Sort( delegate( PlayerVendor.PurchaseRecord a, PlayerVendor.PurchaseRecord b ) { return a.Time.CompareTo( b.Time ); } );
                                else
                                        records.Sort( delegate( PlayerVendor.PurchaseRecord a, PlayerVendor.PurchaseRecord b ) { return b.Time.CompareTo( a.Time ); } );
                        }

                        return BuildGroups( records );
                }

                private int GetPerPage( int height, bool classic )
                {
                        int perPage = classic ? ( height - 160 ) / 40 : ( height - 205 ) / ModernRowHeight;
                        return Math.Max( 1, perPage );
                }

                private void DrawClassic( int width, int height )
                {
                        if ( m_Back == 0 )
                        {
                                string bg = m_Dark ? "#000000" : "#FAEBD7";
                                AddHtml( 25, 10, width, height, String.Format( "<BODY BGCOLOR={0}> </BODY>", bg ), false, false );
                        }
                        else if ( m_Back == 1 )
                        {
                                AddBackground( 25, 10, width, height, m_Dark ? 9270 : 5054 );
                        }
                        else
                        {
                                AddBackground( 25, 10, width, height, 3000 );
                        }

                        AddHtml( 40, 25, width - 80, 20, FormatText( "Purchase History" ), false, false );
                        DrawClassicControls();
                }

                private void DrawClassicControls()
                {
                        AddButton( 40, 50, 0x15E1, 0x15E5, BtnLightDark, GumpButtonType.Reply, 0 );
                        AddLabel( 60, 48, m_Dark ? 0x481 : 0, m_Dark ? "Light" : "Dark" );
                        AddButton( 120, 50, 0x15E1, 0x15E5, BtnBackground, GumpButtonType.Reply, 0 );
                        AddLabel( 140, 48, m_Dark ? 0x481 : 0, BackLabel() );
                        AddButton( 200, 50, 0x15E1, 0x15E5, BtnSort, GumpButtonType.Reply, 0 );
                        AddLabel( 220, 48, m_Dark ? 0x481 : 0, SortLabel() );
                        AddButton( 280, 50, 0x15E1, 0x15E5, BtnLowStock, GumpButtonType.Reply, 0 );
                        AddLabel( 300, 48, m_Dark ? 0x481 : 0, "Low Stock" );
                        AddButton( 360, 50, 0x15E1, 0x15E5, BtnSize, GumpButtonType.Reply, 0 );
                        AddLabel( 380, 48, m_Dark ? 0x481 : 0, SizeLabel() );
                        AddButton( 440, 50, 0x15E1, 0x15E5, BtnAvg, GumpButtonType.Reply, 0 );
                        AddLabel( 460, 48, m_Dark ? 0x481 : 0, m_ShowAvg ? "Avg On" : "Avg Off" );
                        AddButton( 520, 50, 0x15E1, 0x15E5, BtnPresentation, GumpButtonType.Reply, 0 );
                        AddLabel( 540, 48, m_Dark ? 0x481 : 0, "Modern" );
                }

                private void DrawModern( List<List<PlayerVendor.PurchaseRecord>> groups, int width, int height, int perPage, int maxPage )
                {
                        DrawModernBackground( width, height );

                        AddHtml(
                                25,
                                32,
                                width,
                                24,
                                String.Format( "<CENTER><BASEFONT COLOR={0}>Player Vendor Purchase History</BASEFONT></CENTER>", ModernPrimaryColor() ),
                                false,
                                false );

                        DrawModernControls( width );

                        if ( m_Expanded >= 0 && m_Expanded < groups.Count && groups[m_Expanded].Count > 1 )
                        {
                                List<PlayerVendor.PurchaseRecord> expandedGroup = groups[m_Expanded];
                                string expandedSummary = String.Format(
                                        "{0} individual sales | {1} | Click the selected group button to collapse",
                                        expandedGroup.Count,
                                        FilterLabel() );

                                AddHtml( 50, 108, width - 50, 20, "<BASEFONT COLOR=" + ModernSecondaryColor() + ">" + expandedSummary + "</BASEFONT>", false, false );
                                DrawModernExpandedGroup( expandedGroup, m_Expanded, width, height );
                                DrawDateFilters( height - 65, true );
                                return;
                        }

                        string summary = String.Format(
                                "{0} sale group{1} | {2} | Page {3} of {4}",
                                groups.Count,
                                groups.Count == 1 ? String.Empty : "s",
                                FilterLabel(),
                                m_Page + 1,
                                maxPage );

                        AddHtml( 50, 108, width - 50, 20, "<BASEFONT COLOR=" + ModernSecondaryColor() + ">" + summary + "</BASEFONT>", false, false );

                        int start = m_Page * perPage;
                        int end = Math.Min( start + perPage, groups.Count );
                        int y = ModernContentY;

                        for ( int i = start; i < end; i++ )
                        {
                                DrawModernGroupRow( groups[i], i, y, width );
                                y += ModernRowHeight;
                        }

                        if ( groups.Count == 0 )
                        {
                                AddHtml(
                                        50,
                                        ModernContentY + 24,
                                        width - 50,
                                        30,
                                        "<CENTER><BASEFONT COLOR=" + ModernPrimaryColor() + ">No purchases matched this date filter.</BASEFONT></CENTER>",
                                        false,
                                        false );
                        }

                        DrawDateFilters( height - 65, true );
                        DrawModernPager( width, height, maxPage );
                }

                private void DrawModernBackground( int width, int height )
                {
                        int innerX = 25 + ModernInnerPadding;
                        int innerY = 10 + ModernInnerPadding;
                        int innerWidth = width - ( ModernInnerPadding * 2 );
                        int innerHeight = height - ( ModernInnerPadding * 2 );

                        AddBackground( 25, 10, width, height, 9270 );
                        AddImageTiled( innerX, innerY, innerWidth, innerHeight, 2624 );
                        AddAlphaRegion( innerX, innerY, innerWidth, innerHeight );
                }

                private void DrawModernControls( int width )
                {
                        int left = 50;
                        int columnWidth = ( width - 80 ) / 5;

                        AddModernControl( left, 70, columnWidth, BtnSort, SortLabel() );
                        AddModernControl( left + columnWidth, 70, columnWidth, BtnLowStock, "Low Stock" );
                        AddModernControl( left + ( columnWidth * 2 ), 70, columnWidth, BtnSize, SizeLabel() );
                        AddModernControl( left + ( columnWidth * 3 ), 70, columnWidth, BtnAvg, m_ShowAvg ? "Avg On" : "Avg Off" );
                        AddModernControl( left + ( columnWidth * 4 ), 70, columnWidth, BtnPresentation, "Classic" );
                }

                private void AddModernControl( int x, int y, int width, int buttonID, string label )
                {
                        AddButton( x, y, 4005, 4007, buttonID, GumpButtonType.Reply, 0 );
                        AddHtml(
                                x + 34,
                                y + 3,
                                Math.Max( 20, width - 38 ),
                                20,
                                "<BASEFONT COLOR=" + ModernPrimaryColor() + ">" + Utility.FixHtml( label ) + "</BASEFONT>",
                                false,
                                false );
                }

                private void DrawModernGroupRow( List<PlayerVendor.PurchaseRecord> group, int groupIndex, int y, int width )
                {
                        int panelX = 50;
                        int panelWidth = width - 50;
                        int panelHeight = ModernRowHeight - 6;
                        PlayerVendor.PurchaseRecord first = group[0];
                        bool isExpanded = m_Expanded == groupIndex;

                        AddImageTiled( panelX, y, panelWidth, panelHeight, 2624 );
                        AddAlphaRegion( panelX, y, panelWidth, panelHeight );

                        if ( group.Count > 1 )
                                AddButton( panelX + 8, y + 38, isExpanded ? 4006 : 4005, isExpanded ? 4006 : 4007, BtnGroupBase + groupIndex, GumpButtonType.Reply, 0 );

                        AddCenteredPurchaseItem( first, panelX + 38, y + 5, ModernItemSquareSize );

                        int textX = panelX + 140;
                        int textWidth = panelWidth - 150;
                        string itemName = Utility.FixHtml( first.Item ?? String.Empty );
                        string countText = group.Count > 1 ? String.Format( " ({0} listings)", group.Count ) : String.Empty;

                        AddHtml(
                                textX,
                                y + 12,
                                textWidth,
                                19,
                                "<BASEFONT COLOR=" + ModernHeadingColor() + ">" + itemName + countText + "</BASEFONT>",
                                false,
                                false );

                        string buyer = Utility.FixHtml( first.Buyer ?? "Unknown" );
                        string detail = isExpanded
                                ? "Expanded sale details"
                                : String.Format( "{0} bought x{1} on {2}", buyer, first.Amount, first.Time.ToString( "g" ) );
                        string price = String.Format( "Net: {0} gold", FormatGold( first.Price ) );

                        if ( m_ShowAvg && first.Amount > 0 )
                                price += String.Format( " | {0} each", FormatGold( first.Price / first.Amount ) );

                        AddHtml( textX, y + 40, textWidth, 20, "<BASEFONT COLOR=" + ModernPrimaryColor() + ">" + detail + "</BASEFONT>", false, false );
                        AddHtml( textX, y + 68, textWidth, 20, "<BASEFONT COLOR=" + ModernGoldColor() + ">" + price + "</BASEFONT>", false, false );
                }

                private int GetExpandedPerPage( int height )
                {
                        int firstRowY = ModernContentY + ModernRowHeight;
                        int contentBottom = height - 75;
                        return Math.Max( 1, ( contentBottom - firstRowY ) / ModernPurchaseRowHeight );
                }

                private void DrawModernExpandedGroup( List<PlayerVendor.PurchaseRecord> group, int groupIndex, int width, int height )
                {
                        int perPage = GetExpandedPerPage( height );
                        int pageCount = Math.Max( 1, ( group.Count + perPage - 1 ) / perPage );

                        if ( m_ExpandedPage >= pageCount )
                                m_ExpandedPage = pageCount - 1;

                        DrawModernGroupRow( group, groupIndex, ModernContentY, width );

                        int start = m_ExpandedPage * perPage;
                        int end = Math.Min( start + perPage, group.Count );
                        int y = ModernContentY + ModernRowHeight;

                        for ( int i = start; i < end; i++ )
                        {
                                DrawModernPurchaseRow( group[i], i + 1, y, width );
                                y += ModernPurchaseRowHeight;
                        }

                        DrawModernExpandedPager( width, height, pageCount );
                }

                private void DrawModernPurchaseRow( PlayerVendor.PurchaseRecord record, int saleNumber, int y, int width )
                {
                        int panelX = 72;
                        int panelWidth = width - 72;
                        int panelHeight = ModernPurchaseRowHeight - 6;

                        AddImageTiled( panelX, y, panelWidth, panelHeight, 2624 );
                        AddAlphaRegion( panelX, y, panelWidth, panelHeight );
                        AddCenteredPurchaseItem( record, panelX + 10, y + 5, ModernItemSquareSize );

                        int textX = panelX + 110;
                        int textWidth = panelWidth - 120;
                        string buyer = Utility.FixHtml( record.Buyer ?? "Unknown" );
                        string detail = String.Format( "Sale {0}: {1} bought x{2} on {3}", saleNumber, buyer, record.Amount, record.Time.ToString( "g" ) );
                        string price = String.Format( "Net: {0} gold", FormatGold( record.Price ) );

                        if ( m_ShowAvg && record.Amount > 0 )
                                price += String.Format( " | {0} each", FormatGold( record.Price / record.Amount ) );

                        AddHtml( textX, y + 24, textWidth, 20, "<BASEFONT COLOR=" + ModernPrimaryColor() + ">" + detail + "</BASEFONT>", false, false );
                        AddHtml( textX, y + 56, textWidth, 20, "<BASEFONT COLOR=" + ModernGoldColor() + ">" + price + "</BASEFONT>", false, false );
                }

                private void AddCenteredPurchaseItem( PlayerVendor.PurchaseRecord record, int x, int y, int size )
                {
                        AddImageTiled( x, y, size, size, 9304 );

                        int itemID = record.ItemID;
                        int maskedID = itemID & TileData.MaxItemValue;
                        Rectangle2D bounds = new Rectangle2D( 0, 0, 44, 44 );

                        if ( ItemBounds.Table != null && maskedID >= 0 && maskedID < ItemBounds.Table.Length )
                                bounds = ItemBounds.Table[maskedID];

                        int artWidth = bounds.Width > 0 ? bounds.Width : 44;
                        int artHeight = bounds.Height > 0 ? bounds.Height : 44;
                        int itemX = x + ( size / 2 ) - ( artWidth / 2 ) - bounds.X;
                        int itemY = y + ( size / 2 ) - ( artHeight / 2 ) - bounds.Y;

                        itemX = Math.Max( x - bounds.X, Math.Min( itemX, x + size - artWidth - bounds.X ) );
                        itemY = Math.Max( y - bounds.Y, Math.Min( itemY, y + size - artHeight - bounds.Y ) );

                        int hue = 0;

                        if ( record.ItemSerial > 0 )
                        {
                                Item item = World.FindItem( (Serial) record.ItemSerial );

                                if ( item != null && !item.Deleted )
                                        hue = item.Hue;
                        }

                        AddItem( itemX, itemY, itemID, hue );
                        AddTooltipFromPurchaseRecord( record );
                }

                private void DrawModernExpandedPager( int width, int height, int pageCount )
                {
                        int y = height - 34;

                        AddHtml(
                                width - 330,
                                y + 3,
                                95,
                                20,
                                String.Format( "<BASEFONT COLOR={0}>Sales {1}/{2}</BASEFONT>", ModernSecondaryColor(), m_ExpandedPage + 1, pageCount ),
                                false,
                                false );

                        if ( m_ExpandedPage > 0 )
                        {
                                AddButton( width - 230, y, 4014, 4015, BtnExpandedPrev, GumpButtonType.Reply, 0 );
                                AddHtml( width - 196, y + 3, 70, 20, "<BASEFONT COLOR=" + ModernPrimaryColor() + ">Previous</BASEFONT>", false, false );
                        }

                        if ( m_ExpandedPage < pageCount - 1 )
                        {
                                AddButton( width - 120, y, 4005, 4007, BtnExpandedNext, GumpButtonType.Reply, 0 );
                                AddHtml( width - 86, y + 3, 55, 20, "<BASEFONT COLOR=" + ModernPrimaryColor() + ">Next</BASEFONT>", false, false );
                        }
                }

                private void DrawDateFilters( int y, bool modern )
                {
                        if ( modern )
                        {
                                DrawModernDateFilter( 50, y, BtnToday, "Today", m_Filter == 0 );
                                DrawModernDateFilter( 140, y, Btn7d, "7 Days", m_Filter == 1 );
                                DrawModernDateFilter( 230, y, Btn30d, "30 Days", m_Filter == 2 );
                                DrawModernDateFilter( 320, y, BtnAll, "All", m_Filter == 3 );
                                return;
                        }

                        AddButton( 40, y, 0x15E1, 0x15E5, BtnToday, GumpButtonType.Reply, 0 );
                        AddLabel( 60, y - 2, m_Filter == 0 ? 0x40 : ( m_Dark ? 0x481 : 0 ), "Today" );
                        AddButton( 120, y, 0x15E1, 0x15E5, Btn7d, GumpButtonType.Reply, 0 );
                        AddLabel( 140, y - 2, m_Filter == 1 ? 0x40 : ( m_Dark ? 0x481 : 0 ), "7d" );
                        AddButton( 180, y, 0x15E1, 0x15E5, Btn30d, GumpButtonType.Reply, 0 );
                        AddLabel( 200, y - 2, m_Filter == 2 ? 0x40 : ( m_Dark ? 0x481 : 0 ), "30d" );
                        AddButton( 240, y, 0x15E1, 0x15E5, BtnAll, GumpButtonType.Reply, 0 );
                        AddLabel( 260, y - 2, m_Filter == 3 ? 0x40 : ( m_Dark ? 0x481 : 0 ), "All" );
                }

                private void DrawModernDateFilter( int x, int y, int buttonID, string label, bool selected )
                {
                        AddButton( x, y, selected ? 4006 : 4005, selected ? 4006 : 4007, buttonID, GumpButtonType.Reply, 0 );
                        AddHtml(
                                x + 34,
                                y + 3,
                                55,
                                20,
                                "<BASEFONT COLOR=" + ( selected ? ModernHeadingColor() : ModernPrimaryColor() ) + ">" + label + "</BASEFONT>",
                                false,
                                false );
                }

                private void DrawModernPager( int width, int height, int maxPage )
                {
                        int y = height - 34;

                        AddHtml(
                                width - 330,
                                y + 3,
                                95,
                                20,
                                String.Format( "<BASEFONT COLOR={0}>{1}/{2}</BASEFONT>", ModernSecondaryColor(), m_Page + 1, maxPage ),
                                false,
                                false );

                        if ( m_Page > 0 )
                        {
                                AddButton( width - 230, y, 4014, 4015, BtnPrev, GumpButtonType.Reply, 0 );
                                AddHtml( width - 196, y + 3, 70, 20, "<BASEFONT COLOR=" + ModernPrimaryColor() + ">Previous</BASEFONT>", false, false );
                        }

                        if ( m_Page < maxPage - 1 )
                        {
                                AddButton( width - 120, y, 4005, 4007, BtnNext, GumpButtonType.Reply, 0 );
                                AddHtml( width - 86, y + 3, 55, 20, "<BASEFONT COLOR=" + ModernPrimaryColor() + ">Next</BASEFONT>", false, false );
                        }
                }

                private string ModernPrimaryColor()
                {
                        return "#FFFFFF";
                }

                private string ModernSecondaryColor()
                {
                        return "#9DC6FF";
                }

                private string ModernHeadingColor()
                {
                        return "#C5FFAE";
                }

                private string ModernGoldColor()
                {
                        return "#F7D774";
                }

                private string FilterLabel()
                {
                        switch ( m_Filter )
                        {
                                case 0: return "Today";
                                case 1: return "Last 7 days";
                                case 2: return "Last 30 days";
                                default: return "All time";
                        }
                }

private string FormatText( string text )
{
string color = m_Dark ? "#FFFFFF" : "#000000";
return String.Format( "<BASEFONT COLOR={0}>{1}</BASEFONT>", color, text );
}

                private string SortLabel()
                {
                        if ( m_SortPrice )
                                return m_SortAsc ? "Price ↑" : "Price ↓";
                        else
                                return m_SortAsc ? "Recent ↑" : "Recent ↓";
                }

private string SizeLabel()
{
return String.Format( "Size {0}", SizeNames[m_Size] );
}

                private string BackLabel()
                {
                        switch ( m_Back )
                        {
                                case 0: return "Solid";
                                case 1: return "Texture";
                                default: return "Other";
                        }
                }

                private string FormatGold( int amount )
                {
                        if ( amount >= 1000000 )
                                return String.Format( "{0:0.##}m", amount / 1000000.0 );
                        if ( amount >= 1000 )
                                return String.Format( "{0:0.##}k", amount / 1000.0 );
                        return amount.ToString();
                }

                private string FormatEntry( PlayerVendor.PurchaseRecord pr )
                {
                        string price = FormatGold( pr.Price );
                        string entry = String.Format( "{0}: {1} bought {2} x{3} for {4}", pr.Time.ToString( "g" ), pr.Buyer, pr.Item, pr.Amount, price );

                        if ( pr.Price > 0 )
                                entry += " (after 1% fee)";

                        if ( m_ShowAvg && pr.Amount > 0 )
                        {
                                string avg = FormatGold( pr.Price / pr.Amount );
                                entry += String.Format( " (<BASEFONT COLOR=#FF00FF>{0}</BASEFONT> ea)", avg );
                        }
                        return entry;
                }

                private void AddTooltipFromPurchaseRecord( PlayerVendor.PurchaseRecord record )
                {
                        if ( record == null || record.ItemSerial <= 0 )
                                return;

                        Item item = World.FindItem( (Serial) record.ItemSerial );

                        if ( item == null || item.Deleted )
                                return;

                        AddItemProperty( item.Serial );

                        Mobile viewer = m_Viewer;
                        if ( viewer != null && !viewer.Deleted )
                                item.SendPropertiesTo( viewer );
                }

                private List<List<PlayerVendor.PurchaseRecord>> BuildGroups( List<PlayerVendor.PurchaseRecord> records )
                {
                        Dictionary<string, List<PlayerVendor.PurchaseRecord>> dict = new Dictionary<string, List<PlayerVendor.PurchaseRecord>>();
                        List<List<PlayerVendor.PurchaseRecord>> list = new List<List<PlayerVendor.PurchaseRecord>>();

                        foreach ( PlayerVendor.PurchaseRecord pr in records )
                        {
                                string key = String.Format( "{0}|{1}|{2}|{3}", pr.Buyer, pr.Item, pr.Price, pr.ItemID );
                                List<PlayerVendor.PurchaseRecord> grp;
                                if ( !dict.TryGetValue( key, out grp ) )
                                {
                                        grp = new List<PlayerVendor.PurchaseRecord>();
                                        dict[key] = grp;
                                        list.Add( grp );
                                }
                                grp.Add( pr );
                        }

                        return list;
                }

                public override void OnResponse( NetState sender, RelayInfo info )
                {
                        Mobile from = sender.Mobile;

                        if ( m_Vendor == null || m_Vendor.Deleted || from == null || !m_Vendor.CanInteractWith( from, true ) )
                                return;

                        if ( !m_Vendor.PurchaseHistoryUseClassic
                                && ( info.ButtonID == BtnLightDark
                                        || info.ButtonID == BtnBackground ) )
                                return;

                        switch ( info.ButtonID )
                        {
case BtnLightDark:
m_Vendor.PurchaseHistoryDark = !m_Dark;
from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, m_Page, !m_Dark, m_Back, m_SortPrice, m_SortAsc, m_Expanded, m_Size, m_ShowAvg, m_Filter, from ) );
break;
case BtnBackground:
{
int newBack = ( m_Back + 1 ) % 3;
m_Vendor.PurchaseHistoryBack = newBack;
from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, m_Page, m_Dark, newBack, m_SortPrice, m_SortAsc, m_Expanded, m_Size, m_ShowAvg, m_Filter, from ) );
}
break;
                                case BtnSort:
                                {
                                        bool sortPrice = m_SortPrice;
                                        bool sortAsc = m_SortAsc;

                                        if ( !sortPrice && !sortAsc )
                                                sortAsc = true; // recent asc
                                        else if ( !sortPrice && sortAsc )
                                        {
                                                sortPrice = true; // price desc
                                                sortAsc = false;
                                        }
                                        else if ( sortPrice && !sortAsc )
                                                sortAsc = true; // price asc
                                        else
                                        {
                                                sortPrice = false; // recent desc
                                                sortAsc = false;
                                        }

                                        m_Vendor.PurchaseHistorySortPrice = sortPrice;
                                        m_Vendor.PurchaseHistorySortAsc = sortAsc;
                                        int expanded = m_Vendor.PurchaseHistoryUseClassic ? m_Expanded : -1;
from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, m_Page, m_Dark, m_Back, sortPrice, sortAsc, expanded, m_Size, m_ShowAvg, m_Filter, from ) );
                                        break;
                                }
                                case BtnLowStock:
from.SendGump( new PlayerVendorLowStockGump( m_Vendor ) );
from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, m_Page, m_Dark, m_Back, m_SortPrice, m_SortAsc, m_Expanded, m_Size, m_ShowAvg, m_Filter, from ) );
                                        break;
                                case BtnPrev:
if ( !m_Vendor.PurchaseHistoryUseClassic && m_Expanded >= 0 )
break;
if ( m_Page > 0 )
m_Page--;
from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, m_Page, m_Dark, m_Back, m_SortPrice, m_SortAsc, m_Expanded, m_Size, m_ShowAvg, m_Filter, from ) );
                                        break;
                                case BtnNext:
                                {
                                        if ( !m_Vendor.PurchaseHistoryUseClassic && m_Expanded >= 0 )
                                                break;

                                        int height = Heights[m_Size];
                                        int perPage = GetPerPage( height, m_Vendor.PurchaseHistoryUseClassic );
                                        List<List<PlayerVendor.PurchaseRecord>> groups = PrepareGroups();
                                        int maxPage = ( groups.Count + perPage - 1 ) / perPage;
                                        if ( m_Page < maxPage - 1 )
                                                m_Page++;
from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, m_Page, m_Dark, m_Back, m_SortPrice, m_SortAsc, m_Expanded, m_Size, m_ShowAvg, m_Filter, from ) );
                                        break;
                                }
                                case BtnExpandedPrev:
                                {
                                        if ( m_Vendor.PurchaseHistoryUseClassic || m_Expanded < 0 )
                                                break;

                                        int expandedPage = Math.Max( 0, m_ExpandedPage - 1 );
                                        from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, m_Page, m_Dark, m_Back, m_SortPrice, m_SortAsc, m_Expanded, m_Size, m_ShowAvg, m_Filter, expandedPage, from ) );
                                        break;
                                }
                                case BtnExpandedNext:
                                {
                                        if ( m_Vendor.PurchaseHistoryUseClassic || m_Expanded < 0 )
                                                break;

                                        List<List<PlayerVendor.PurchaseRecord>> groups = PrepareGroups();

                                        if ( m_Expanded >= groups.Count )
                                                break;

                                        int perPage = GetExpandedPerPage( Heights[m_Size] );
                                        int pageCount = Math.Max( 1, ( groups[m_Expanded].Count + perPage - 1 ) / perPage );
                                        int expandedPage = Math.Min( pageCount - 1, m_ExpandedPage + 1 );
                                        from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, m_Page, m_Dark, m_Back, m_SortPrice, m_SortAsc, m_Expanded, m_Size, m_ShowAvg, m_Filter, expandedPage, from ) );
                                        break;
                                }
case BtnSize:
int newSize = ( m_Size + 1 ) % Widths.Length;
m_Vendor.PurchaseHistorySize = newSize;
from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, m_Page, m_Dark, m_Back, m_SortPrice, m_SortAsc, m_Expanded, newSize, m_ShowAvg, m_Filter, from ) );
break;
                                case BtnAvg:
{
bool newShowAvg = !m_ShowAvg;
m_Vendor.PurchaseHistoryShowAvg = newShowAvg;
from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, m_Page, m_Dark, m_Back, m_SortPrice, m_SortAsc, m_Expanded, m_Size, newShowAvg, m_Filter, from ) );
}
                                        break;
                                case BtnToday:
                                m_Vendor.PurchaseHistoryFilter = 0;
from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, 0, m_Dark, m_Back, m_SortPrice, m_SortAsc, m_Vendor.PurchaseHistoryUseClassic ? m_Expanded : -1, m_Size, m_ShowAvg, 0, from ) );
                                        break;
                                case Btn7d:
                                m_Vendor.PurchaseHistoryFilter = 1;
from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, 0, m_Dark, m_Back, m_SortPrice, m_SortAsc, m_Vendor.PurchaseHistoryUseClassic ? m_Expanded : -1, m_Size, m_ShowAvg, 1, from ) );
                                        break;
                                case Btn30d:
                                m_Vendor.PurchaseHistoryFilter = 2;
from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, 0, m_Dark, m_Back, m_SortPrice, m_SortAsc, m_Vendor.PurchaseHistoryUseClassic ? m_Expanded : -1, m_Size, m_ShowAvg, 2, from ) );
                                        break;
                                case BtnAll:
                                m_Vendor.PurchaseHistoryFilter = 3;
from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, 0, m_Dark, m_Back, m_SortPrice, m_SortAsc, m_Vendor.PurchaseHistoryUseClassic ? m_Expanded : -1, m_Size, m_ShowAvg, 3, from ) );
                                        break;
                                case BtnPresentation:
                                        m_Vendor.PurchaseHistoryUseClassic = !m_Vendor.PurchaseHistoryUseClassic;
                                        from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, 0, m_Dark, m_Back, m_SortPrice, m_SortAsc, -1, m_Size, m_ShowAvg, m_Filter, from ) );
                                        break;
                                default:
                                {
                                        if ( info.ButtonID >= BtnGroupBase )
                                        {
                                                int idx = info.ButtonID - BtnGroupBase;
                                                List<List<PlayerVendor.PurchaseRecord>> groups = PrepareGroups();

                                                if ( idx >= 0 && idx < groups.Count )
                                                {
                                                        int newExpanded = m_Expanded == idx ? -1 : idx;
                                                        from.SendGump( new PlayerVendorPurchaseHistoryGump( m_Vendor, m_Page, m_Dark, m_Back, m_SortPrice, m_SortAsc, newExpanded, m_Size, m_ShowAvg, m_Filter, from ) );
                                                }
                                        }
                                        break;
                                }
                        }
                }
        }

        public class PlayerVendorLowStockGump : Gump
        {
                private PlayerVendor m_Vendor;
                private int m_Page;
                private const int Threshold = 5;
                private const int PerPage = 10;
                private const int BtnOpen = 1;
                private const int BtnPrev = 2;
                private const int BtnNext = 3;

                public PlayerVendorLowStockGump( PlayerVendor vendor ) : this( vendor, 0 )
                {
                }

                public PlayerVendorLowStockGump( PlayerVendor vendor, int page ) : base( 50, 50 )
                {
                        m_Vendor = vendor;
                        m_Page = page;

                        AddPage( 0 );
                        AddBackground( 25, 10, 300, 300, 5054 );
                        AddHtml( 40, 25, 260, 20, "Low Stock", false, false );

                        List<Item> items = GetLowStockItems();

                        int maxPage = ( items.Count + PerPage - 1 ) / PerPage;
                        if ( maxPage < 1 )
                                maxPage = 1;
                        if ( m_Page >= maxPage )
                                m_Page = maxPage - 1;

                        int start = m_Page * PerPage;
                        int end = Math.Min( start + PerPage, items.Count );

                        int y = 50;
                        for ( int i = start; i < end; i++ )
                        {
                                Item item = items[i];
                                AddLabel( 40, y, 0, String.Format( "{0} ({1})", item.Name != null ? item.Name : item.GetType().Name, item.Amount ) );
                                y += 20;
                        }

                        if ( m_Page > 0 )
                                AddButton( 220, 260, 0x15E3, 0x15E7, BtnPrev, GumpButtonType.Reply, 0 );
                        if ( m_Page < maxPage - 1 )
                                AddButton( 250, 260, 0x15E1, 0x15E5, BtnNext, GumpButtonType.Reply, 0 );
                        AddLabel( 230, 240, 0, String.Format( "{0}/{1}", m_Page + 1, maxPage ) );

                        AddButton( 40, 260, 0x15E1, 0x15E5, BtnOpen, GumpButtonType.Reply, 0 );
                        AddLabel( 60, 258, 0, "Open Stock" );
                }

                private List<Item> GetLowStockItems()
                {
                        List<Item> list = new List<Item>();
                        Container pack = m_Vendor.Backpack;
                        if ( pack != null )
                        {
                                foreach ( Item item in pack.Items )
                                {
                                        if ( item.Amount <= Threshold )
                                                list.Add( item );
                                }
                        }
                        return list;
                }

                public override void OnResponse( NetState sender, RelayInfo info )
                {
                        Mobile from = sender.Mobile;

                        switch ( info.ButtonID )
                        {
                                case BtnOpen:
                                        m_Vendor.OpenBackpack( from );
                                        break;
                                case BtnPrev:
                                        if ( m_Page > 0 )
                                                from.SendGump( new PlayerVendorLowStockGump( m_Vendor, m_Page - 1 ) );
                                        break;
                                case BtnNext:
                                        from.SendGump( new PlayerVendorLowStockGump( m_Vendor, m_Page + 1 ) );
                                        break;
                        }
                }
        }

        public class PlayerVendorCustomizeGump : Gump
	{
		private Mobile m_Vendor;

		private class CustomItem
		{
			private Type m_Type;
			private int m_ItemID;
			private int m_LocNum;
			private int m_ArtNum;
			private bool m_LongText;

			public CustomItem( int itemID, int loc ) : this( null, itemID, loc, 0, false )
			{
			}

			public CustomItem( int itemID, int loc, bool longText ) : this( null, itemID, loc, 0, longText )
			{
			}

			public CustomItem( Type type, int loc ) : this( type, loc, 0 )
			{
			}

			public CustomItem( Type type, int loc, int art ) : this( type, 0, loc, art, false )
			{
			}

			public CustomItem( Type type, int itemID, int loc, int art, bool longText )
			{
				m_Type = type;
				m_ItemID = itemID;
				m_LocNum = loc;
				m_ArtNum = art;
				m_LongText = longText;
			}

			public Item Create()
			{
				if ( m_Type == null )
					return null;

				Item i = null;
				
				try
				{
					ConstructorInfo ctor = m_Type.GetConstructor( new Type[0] );
					if ( ctor != null )
						i = ctor.Invoke( null ) as Item;
				}
				catch
				{
				}
				
				return i;
			}

			public Type Type{ get{ return m_Type; } }
			public int ItemID{ get{ return m_ItemID; } }
			public int LocNumber{ get{ return m_LocNum; } }
			public int ArtNumber{ get{ return m_ArtNum; } }
			public bool LongText{ get{ return m_LongText; } }
		}

		private class CustomCategory
		{
			private CustomItem[] m_Entries;
			private Layer m_Layer;
			private bool m_CanDye;
			private int m_LocNum;

			public CustomCategory( Layer layer, int loc, bool canDye, CustomItem[] items )
			{
				m_Entries = items;
				m_CanDye = canDye;
				m_Layer = layer;
				m_LocNum = loc;
			}

			public bool CanDye{ get{ return m_CanDye; } }
			public CustomItem[] Entries{ get{ return m_Entries; } }
			public Layer Layer{ get{ return m_Layer; } }
			public int LocNumber{ get{ return m_LocNum; } }
		}

		private static CustomCategory[] Categories = new CustomCategory[]{
			new CustomCategory( Layer.InnerTorso, 1011357, true, new CustomItem[]{// Upper Torso
				new CustomItem( typeof( Shirt ), 		1011359, 5399 ),
				new CustomItem( typeof( FancyShirt ),	1011360, 7933 ),
				new CustomItem( typeof( PlainDress ),	1011363, 7937 ),
				new CustomItem( typeof( FancyDress ),	1011364, 7935 ),
				new CustomItem( typeof( Robe ),			1011365, 7939 )
			} ),

			new CustomCategory( Layer.MiddleTorso, 1011371, true, new CustomItem[]{//Over chest
				new CustomItem( typeof( Doublet ), 		1011358, 8059 ),
				new CustomItem( typeof( Tunic ),		1011361, 8097 ),
				new CustomItem( typeof( JesterSuit ), 	1011366, 8095 ),
				new CustomItem( typeof( BodySash ),		1011372, 5441 ),
				new CustomItem( typeof( Surcoat ),		1011362, 8189 ),
				new CustomItem( typeof( HalfApron ),	1011373, 5435 ),
				new CustomItem( typeof( FullApron ),	1011374, 5437 ),
			} ),

			new CustomCategory( Layer.Shoes, 1011388, true, new CustomItem[]{//Footwear
				new CustomItem( typeof( Sandals ),		1011389, 5901 ),
				new CustomItem( typeof( Shoes ),		1011390, 5904 ),
				new CustomItem( typeof( Boots ),		1011391, 5899 ),
				new CustomItem( typeof( ThighBoots ),	1011392, 5906 ),
			} ),

			new CustomCategory( Layer.Helm, 1011375, true, new CustomItem[]{//Hats
				new CustomItem( typeof( SkullCap ),		1011376, 5444 ),
				new CustomItem( typeof( Bandana ), 		1011377, 5440 ),
				new CustomItem( typeof( FloppyHat ),	1011378, 5907 ),
				new CustomItem( typeof( WideBrimHat ),	1011379, 5908 ),
				new CustomItem( typeof( Cap ),			1011380, 5909 ),
				new CustomItem( typeof( TallStrawHat ),	1011382, 5910 )
			} ),

			new CustomCategory( Layer.Helm, 1015319, true, new CustomItem[]{//More Hats
			    new CustomItem( typeof( StrawHat ),		1011382, 5911 ),
				new CustomItem( typeof( WizardsHat ), 	1011383, 5912 ),
				new CustomItem( typeof( Bonnet ),		1011384, 5913 ),
				new CustomItem( typeof( FeatheredHat ),	1011385, 5914 ),
				new CustomItem( typeof( TricorneHat ),	1011386, 5915 ),
				new CustomItem( typeof( JesterHat ),	1011387, 5916 )
			} ),

			new CustomCategory( Layer.Pants, 1011367, true, new CustomItem[]{ //Lower Torso
				new CustomItem( typeof( LongPants ),	1011368, 5433 ),
				new CustomItem( typeof( Kilt ), 		1011369, 5431 ),
				new CustomItem( typeof( Skirt ),		1011370, 5398 ),
			} ),

			new CustomCategory( Layer.Cloak, 1011393, true, new CustomItem[]{ // Back
				new CustomItem( typeof( Cloak ),		1011394, 5397 )
			} ),

			new CustomCategory( Layer.Hair, 1011395, true, new CustomItem[]{ // Hair
				new CustomItem( 0x203B,		1011052 ),
				new CustomItem( 0x203C,		1011053 ),
				new CustomItem( 0x203D,		1011054 ),
				new CustomItem( 0x2044,		1011055 ),
				new CustomItem( 0x2045,		1011047 ),
				new CustomItem( 0x204A,		1011050 ),
				new CustomItem( 0x2047,		1011396 ),
				new CustomItem( 0x2048,		1011048 ),
				new CustomItem( 0x2049,		1011049 ),
			} ),

			new CustomCategory( Layer.FacialHair, 1015320, true, new CustomItem[]{//Facial Hair
				new CustomItem( 0x2041,		1011062 ),
				new CustomItem( 0x203F,		1011060 ),
				new CustomItem( 0x204B,		1015321, true ),
				new CustomItem( 0x203E,		1011061 ),
				new CustomItem( 0x204C,		1015322, true ),
				new CustomItem( 0x2040,		1015323 ),
				new CustomItem( 0x204D,		1011401 ),
			} ),

			new CustomCategory( Layer.FirstValid, 1011397, false, new CustomItem[]{//Held items
				new CustomItem( typeof( FishingPole ), 	1011406, 3520 ),
				new CustomItem( typeof( Pickaxe ),		1011407, 3717 ),
				new CustomItem( typeof( Pitchfork ),	1011408, 3720 ),
				new CustomItem( typeof( Cleaver ),		1015324, 3778 ),
				new CustomItem( typeof( Mace ),			1011409, 3933 ),
				new CustomItem( typeof( Torch ),		1011410, 3940 ),
				new CustomItem( typeof( Hammer ),		1011411, 4020 ),
				new CustomItem( typeof( Longsword ),	1011412, 3936 ),
				new CustomItem( typeof( GnarledStaff ), 1011413, 5113 )
			} ),

			new CustomCategory( Layer.FirstValid, 1015325, false, new CustomItem[]{//More held items
				new CustomItem( typeof( Crossbow ),		1011414, 3920 ),
				new CustomItem( typeof( WarMace ),		1011415, 5126 ),
				new CustomItem( typeof( TwoHandedAxe ),	1011416, 5186 ),
				new CustomItem( typeof( Spear ),		1011417, 3939 ),
				new CustomItem( typeof( Katana ),		1011418, 5118 ),
				new CustomItem( typeof( Spellbook ),	1011419, 3834 )
			} )
		};

		public PlayerVendorCustomizeGump( Mobile v, Mobile from ) : base( 30, 40 )
		{
			m_Vendor = v;
			int x,y;

			from.CloseGump( typeof( PlayerVendorCustomizeGump ) );

			AddPage( 0 );
			AddBackground( 0, 0, 585, 393, 5054 );
			AddBackground( 195, 36, 387, 275, 3000 );
			AddHtmlLocalized( 10, 10, 565, 18, 1011356, false, false ); // <center>VENDOR CUSTOMIZATION MENU</center>
			AddHtmlLocalized( 60, 355, 150, 18, 1011036, false, false ); // OKAY
			AddButton( 25, 355, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 320, 355, 150, 18, 1011012, false, false ); // CANCEL
			AddButton( 285, 355, 4005, 4007, 0, GumpButtonType.Reply, 0 );

			y = 35;
			for ( int i=0;i<Categories.Length;i++ )
			{
				CustomCategory cat = (CustomCategory)Categories[i];
				AddHtmlLocalized( 5, y, 150, 25, cat.LocNumber, true, false );
				AddButton( 155, y, 4005, 4007, 0, GumpButtonType.Page, i+1 );
				y += 25;
			}

			for ( int i=0;i<Categories.Length;i++ )
			{
				CustomCategory cat = (CustomCategory)Categories[i];
				AddPage( i+1 );

				for ( int c=0;c<cat.Entries.Length;c++ )
				{
					CustomItem entry = (CustomItem)cat.Entries[c];
					x = 198 + (c%3)*129;
					y = 38 + (c/3)*67;

					AddHtmlLocalized( x, y, 100, entry.LongText ? 36 : 18, entry.LocNumber, false, false );

					if ( entry.ArtNumber != 0 )
						AddItem( x+20, y+25, entry.ArtNumber );

					AddRadio( x, y + (entry.LongText ? 40 : 20), 210, 211, false, (c<<8) + i );
				}

				if ( cat.CanDye )
				{
					AddHtmlLocalized( 327, 239, 100, 18, 1011402, false, false ); // Color
					AddRadio( 327, 259, 210, 211, false, 100+i );
				}

				AddHtmlLocalized( 456, 239, 100, 18, 1011403, false, false ); // Remove
				AddRadio( 456, 259, 210, 211, false, 200+i );
			}
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			if ( m_Vendor.Deleted )
				return;

			Mobile from = state.Mobile;

			if ( m_Vendor is PlayerVendor && !((PlayerVendor)m_Vendor).CanInteractWith( from, true ) )
				return;

			if ( m_Vendor is PlayerBarkeeper && !((PlayerBarkeeper)m_Vendor).IsOwner( from ) )
				return;

			if ( info.ButtonID == 0 )
			{
				if ( m_Vendor is PlayerVendor ) // do nothing for barkeeps
				{
					m_Vendor.Direction = m_Vendor.GetDirectionTo( from );
					m_Vendor.Animate( 32, 5, 1, true, false, 0 );//bow
					m_Vendor.SayTo( from, 1043310 + Utility.Random( 12 ) ); // a little random speech
				}
			}
			else if ( info.ButtonID == 1 && info.Switches.Length > 0 )
			{
				int cnum = info.Switches[0];
				int cat = cnum%256;
				int ent = cnum>>8;

				if ( cat < Categories.Length && cat >= 0 )
				{
					if ( ent < Categories[cat].Entries.Length && ent >= 0 )
					{
						Item item = m_Vendor.FindItemOnLayer( Categories[cat].Layer );

						if ( item != null )
							item.Delete();

						List<Item> items = m_Vendor.Items;

						for ( int i = 0; item == null && i < items.Count; ++i )
						{
							Item checkitem = items[i];
							Type type = checkitem.GetType();

							for ( int j = 0; item == null && j < Categories[cat].Entries.Length; ++j )
							{
								if ( type == Categories[cat].Entries[j].Type )
									item = checkitem;
							}
						}

						if ( item != null )
							item.Delete();

						if ( Categories[cat].Layer == Layer.FacialHair )
						{
							if ( m_Vendor.Female )
							from.SendLocalizedMessage( 1010639 ); // You cannot place facial hair on a woman!
							else
								m_Vendor.FacialHairItemID = Categories[cat].Entries[ent].ItemID;
						}
						else if ( Categories[cat].Layer == Layer.Hair )
							m_Vendor.HairItemID = Categories[cat].Entries[ent].ItemID;

						else
						{
							item = Categories[cat].Entries[ent].Create();

							if ( item != null )
							{
								item.Layer = Categories[cat].Layer;

								if ( !m_Vendor.EquipItem( item ) )
									item.Delete();
							}
						}

						from.SendGump( new PlayerVendorCustomizeGump( m_Vendor, from ) );
					}
				}
				else
				{
					cat -= 100;

					if ( cat < 100 )
					{
						if ( cat < Categories.Length && cat >= 0 )
						{
							Item item = null;

							List<Item> items = m_Vendor.Items;

							for ( int i = 0; item == null && i < items.Count; ++i )
							{
								Item checkitem = items[i];
								Type type = checkitem.GetType();

								for ( int j = 0; item == null && j < Categories[cat].Entries.Length; ++j )
								{
									if ( type == Categories[cat].Entries[j].Type )
										item = checkitem;
								}
							}

							if ( item != null )
								new PVHuePicker( item, m_Vendor, from ).SendTo( state );
						}
					}
					else
					{
						cat -= 100;

						if ( cat < Categories.Length && cat >= 0 )
						{
							Item item = null;

							List<Item> items = m_Vendor.Items;

							for ( int i = 0; item == null && i < items.Count; ++i )
							{
								Item checkitem = items[i];
								Type type = checkitem.GetType();

								for ( int j = 0; item == null && j < Categories[cat].Entries.Length; ++j )
								{
									if ( type == Categories[cat].Entries[j].Type )
										item = checkitem;
								}
							}

							if ( item != null )
								item.Delete();

							from.SendGump( new PlayerVendorCustomizeGump( m_Vendor, from ) );
						}
					}
				}
			}
		}

		private class PVHuePicker : HuePicker
		{
			private Item m_Item;
			private Mobile m_Vendor;
			private Mobile m_Mob;

			public PVHuePicker( Item item, Mobile v, Mobile from ) : base( (item.Layer == Layer.Hair || item.Layer == Layer.FacialHair) ? 0xFAB : item.ItemID )
			{
				m_Vendor = v;
				m_Item = item;
				m_Mob = from;
			}

			public override void OnResponse( int hue )
			{
				if ( m_Item.Deleted )
					return;

				if ( m_Vendor is PlayerVendor && !((PlayerVendor)m_Vendor).CanInteractWith( m_Mob, true ) )
					return;

				if ( m_Vendor is PlayerBarkeeper && !((PlayerBarkeeper)m_Vendor).IsOwner( m_Mob ) )
					return;

				m_Item.Hue = hue;
				m_Mob.SendGump( new PlayerVendorCustomizeGump( m_Vendor, m_Mob ) );
			}
		}
	}

	public class NewPlayerVendorCustomizeGump : Gump
	{
		private PlayerVendor m_Vendor;

		private class HairOrBeard
		{
			private int m_ItemID;
			private int m_Name;

			public int ItemID{ get{ return m_ItemID; } }
			public int Name{ get{ return m_Name; } }

			public HairOrBeard( int itemID, int name )
			{
				m_ItemID = itemID;
				m_Name = name;
			}
		}

		private static HairOrBeard[] m_HairStyles = new HairOrBeard[]
			{
				new HairOrBeard( 0x203B,	1011052 ),	// Short
				new HairOrBeard( 0x203C,	1011053 ),	// Long
				new HairOrBeard( 0x203D,	1011054 ),	// Ponytail
				new HairOrBeard( 0x2044,	1011055 ),	// Mohawk
				new HairOrBeard( 0x2045,	1011047 ),	// Pageboy
				new HairOrBeard( 0x204A,	1011050 ),	// Topknot
				new HairOrBeard( 0x2047,	1011396 ),	// Curly
				new HairOrBeard( 0x2048,	1011048 ),	// Receding
				new HairOrBeard( 0x2049,	1011049 )	// 2-tails
			};

		private static HairOrBeard[] m_BeardStyles = new HairOrBeard[]
			{
				new HairOrBeard( 0x2041,	1011062 ),	// Mustache
				new HairOrBeard( 0x203F,	1011060 ),	// Short beard
				new HairOrBeard( 0x204B,	1015321 ),	// Short Beard & Moustache
				new HairOrBeard( 0x203E,	1011061 ),	// Long beard
				new HairOrBeard( 0x204C,	1015322 ),	// Long Beard & Moustache
				new HairOrBeard( 0x2040,	1015323 ),	// Goatee
				new HairOrBeard( 0x204D,	1011401 )	// Vandyke
			};

		public NewPlayerVendorCustomizeGump( PlayerVendor vendor ) : base( 50, 50 )
		{
			m_Vendor = vendor;

			AddBackground( 0, 0, 370, 370, 0x13BE );

			AddImageTiled( 10, 10, 350, 20, 0xA40 );
			AddImageTiled( 10, 40, 350, 20, 0xA40 );
			AddImageTiled( 10, 70, 350, 260, 0xA40 );
			AddImageTiled( 10, 340, 350, 20, 0xA40 );

			AddAlphaRegion( 10, 10, 350, 350 );

			AddHtmlLocalized( 10, 12, 350, 18, 1011356, 0x7FFF, false, false ); // <center>VENDOR CUSTOMIZATION MENU</center>

			AddHtmlLocalized( 10, 42, 150, 18, 1062459, 0x421F, false, false ); // <CENTER>HAIR</CENTER>

			for ( int i = 0; i < m_HairStyles.Length; i++ )
			{
				HairOrBeard hair = m_HairStyles[i];

				AddButton( 10, 70 + i * 20, 0xFA5, 0xFA7, 0x100 | i, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 45, 72 + i * 20, 110, 18, hair.Name, 0x7FFF, false, false );
			}

			AddButton( 10, 70 + m_HairStyles.Length * 20, 0xFB1, 0xFB3, 2, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 45, 72 + m_HairStyles.Length * 20, 110, 18, 1011403, 0x7FFF, false, false ); // Remove

			AddButton( 10, 70 + (m_HairStyles.Length + 1) * 20, 0xFA5, 0xFA7, 3, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 45, 72 + (m_HairStyles.Length + 1) * 20, 110, 18, 1011402, 0x7FFF, false, false ); // Color

			if ( vendor.Female )
			{
				AddButton( 160, 290, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 195, 292, 160, 18, 1015327, 0x7FFF, false, false ); // Male

				AddHtmlLocalized( 195, 312, 160, 18, 1015328, 0x421F, false, false ); // Female
			}
			else
			{
				AddHtmlLocalized( 160, 42, 210, 18, 1062460, 0x421F, false, false ); // <CENTER>BEARD</CENTER>

				for ( int i = 0; i < m_BeardStyles.Length; i++ )
				{
					HairOrBeard beard = m_BeardStyles[i];

					AddButton( 160, 70 + i * 20, 0xFA5, 0xFA7, 0x200 | i, GumpButtonType.Reply, 0 );
					AddHtmlLocalized( 195, 72 + i * 20, 160, 18, beard.Name, 0x7FFF, false, false );
				}

				AddButton( 160, 70 + m_BeardStyles.Length * 20, 0xFB1, 0xFB3, 4, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 195, 72 + m_BeardStyles.Length * 20, 160, 18, 1011403, 0x7FFF, false, false ); // Remove

				AddButton( 160, 70 + (m_BeardStyles.Length + 1) * 20, 0xFA5, 0xFA7, 5, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 195, 72 + (m_BeardStyles.Length + 1) * 20, 160, 18, 1011402, 0x7FFF, false, false ); // Color

				AddHtmlLocalized( 195, 292, 160, 18, 1015327, 0x421F, false, false ); // Male

				AddButton( 160, 310, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 195, 312, 160, 18, 1015328, 0x7FFF, false, false ); // Female
			}

			AddButton( 10, 340, 0xFA5, 0xFA7, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 45, 342, 305, 18, 1060675, 0x7FFF, false, false ); // CLOSE
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;

			if ( !m_Vendor.CanInteractWith( from, true ) )
				return;

			switch ( info.ButtonID )
			{
				case 0: // CLOSE
				{
					m_Vendor.Direction = m_Vendor.GetDirectionTo( from );
					m_Vendor.Animate( 32, 5, 1, true, false, 0 ); // bow
					m_Vendor.SayTo( from, 1043310 + Utility.Random( 12 ) ); // a little random speech

					break;
				}
				case 1: // Female/Male
				{
					if ( m_Vendor.Female )
					{
						m_Vendor.BodyValue = 400;
						m_Vendor.Female = false;
					}
					else
					{
						m_Vendor.BodyValue = 401;
						m_Vendor.Female = true;

						m_Vendor.FacialHairItemID = 0;
					}

					from.SendGump( new NewPlayerVendorCustomizeGump( m_Vendor ) );

					break;
				}
				case 2: // Remove hair
				{
					m_Vendor.HairItemID = 0;

					from.SendGump( new NewPlayerVendorCustomizeGump( m_Vendor ) );

					break;
				}
				case 3: // Color hair
				{
					if ( m_Vendor.HairItemID > 0 )
					{
						new PVHuePicker( m_Vendor, false, from ).SendTo( from.NetState );
					}
					else
					{
						from.SendGump( new NewPlayerVendorCustomizeGump( m_Vendor ) );
					}

					break;
				}
				case 4: // Remove beard
				{
					m_Vendor.FacialHairItemID = 0;

					from.SendGump( new NewPlayerVendorCustomizeGump( m_Vendor ) );

					break;
				}
				case 5: // Color beard
				{
					if ( m_Vendor.FacialHairItemID > 0 )
					{
						new PVHuePicker( m_Vendor, true, from ).SendTo( from.NetState );
					}
					else
					{
						from.SendGump( new NewPlayerVendorCustomizeGump( m_Vendor ) );
					}

					break;
				}
				default:
				{
					int hairhue = 0;

					if ( (info.ButtonID & 0x100) != 0 ) // Hair style selected
					{
						int index = info.ButtonID & 0xFF;

						if ( index >= m_HairStyles.Length )
							return;

						HairOrBeard hairStyle = m_HairStyles[index];

						hairhue = m_Vendor.HairHue;

						m_Vendor.HairItemID = 0;
						m_Vendor.ProcessDelta();

						m_Vendor.HairItemID = hairStyle.ItemID;

						m_Vendor.HairHue = hairhue;

						from.SendGump( new NewPlayerVendorCustomizeGump( m_Vendor ) );
					}
					else if ( (info.ButtonID & 0x200) != 0 ) // Beard style selected
					{
						if ( m_Vendor.Female )
							return;

						int index = info.ButtonID & 0xFF;

						if ( index >= m_BeardStyles.Length )
							return;

						HairOrBeard beardStyle = m_BeardStyles[index];

						hairhue = m_Vendor.FacialHairHue;

						m_Vendor.FacialHairItemID = 0;
						m_Vendor.ProcessDelta();

						m_Vendor.FacialHairItemID = beardStyle.ItemID;

						m_Vendor.FacialHairHue = hairhue;

						from.SendGump( new NewPlayerVendorCustomizeGump( m_Vendor ) );
					}

					break;
				}
			}
		}

		private class PVHuePicker : HuePicker
		{
			private PlayerVendor m_Vendor;
			private bool m_FacialHair;
			private Mobile m_From;

			public PVHuePicker( PlayerVendor vendor, bool facialHair, Mobile from ) : base( 0xFAB )
			{
				m_Vendor = vendor;
				m_FacialHair = facialHair;
				m_From = from;
			}

			public override void OnResponse( int hue )
			{
				if ( !m_Vendor.CanInteractWith( m_From, true ) )
					return;

				if ( m_FacialHair )
					m_Vendor.FacialHairHue = hue;
				else
					m_Vendor.HairHue = hue;

				m_From.SendGump( new NewPlayerVendorCustomizeGump( m_Vendor ) );
			}
		}
	}
}
