/*
 * HouseIntel - Admin-only house intelligence and safe packing tool.
 *
 * Usage:
 * [HouseIntel
 * [HouseIntel <accountName>
 * [HouseIntel <ipAddress>
 * [HouseIntel <serial>
 * [HouseIntel target
 *
 * Safety:
 * - Read-only by default; packing requires explicit confirm from a preview gump.
 * - Protected items (blessed/newbied/ensouled), structural items, and immovable items are skipped.
 * - Full audit log in Logs/HouseIntel.log plus command logs.
 * - No house decay logic is used or referenced.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Server;
using Server.Accounting;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Targeting;
using Server.Utilities;

namespace Server.Commands
{
	public class HouseIntel
	{
		public static void Initialize()
		{
			CommandSystem.Register( "HouseIntel", HouseIntelConfig.RequiredAccessLevel, new CommandEventHandler( OnCommand ) );
			CommandSystem.Register( "HouseIntelTestPack", AccessLevel.Owner, new CommandEventHandler( OnTestPackCommand ) );
		}

		[Usage( "HouseIntel [accountName|ipAddress|serial|target]" )]
		[Description( "Opens the HouseIntel admin gump for house investigations and safe packing tools." )]
		private static void OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;

			if ( from.AccessLevel < HouseIntelConfig.RequiredAccessLevel )
			{
				from.SendMessage( "You do not have access to this command." );
				return;
			}

			if ( e.Length == 0 )
			{
				ShowGump( from, HouseIntelView.AllHouses, null );
				return;
			}

			string term = e.GetString( 0 );

			if ( term.Equals( "target", StringComparison.OrdinalIgnoreCase ) )
			{
				from.Target = new HouseIntelTarget();
				from.SendMessage( "Target a player to open HouseIntel cluster view." );
				return;
			}

			IPAddress ip;
			if ( IPAddress.TryParse( term, out ip ) )
			{
				ShowGump( from, HouseIntelView.Cluster, term );
				return;
			}

			int serialValue;
			if ( Int32.TryParse( term, out serialValue ) )
			{
				Serial serial = (Serial)serialValue;
				Mobile m = World.FindMobile( serial );
				if ( m != null )
				{
					ShowGump( from, HouseIntelView.Cluster, m.Account != null ? m.Account.Username : m.Name );
					return;
				}
			}

			ShowGump( from, HouseIntelView.Cluster, term );
		}

		[Usage( "HouseIntelTestPack" )]
		[Description( "Targets a house to run a pack preview. (Owner access; use for validation.)" )]
		private static void OnTestPackCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;

			from.Target = new HouseIntelTestTarget();
			from.SendMessage( "Target a house or house sign to run a pack preview." );
		}

		private static void ShowGump( Mobile from, HouseIntelView view, string searchTerm )
		{
			from.CloseGump( typeof( HouseIntelGump ) );
			from.SendGump( new HouseIntelGump( from, view, searchTerm ) );
		}

		private class HouseIntelTarget : Target
		{
			public HouseIntelTarget() : base( 12, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Mobile mobile = targeted as Mobile;
				if ( mobile == null )
				{
					from.SendMessage( "You must target a player." );
					return;
				}

				Account account = mobile.Account as Account;
				if ( account == null )
				{
					from.SendMessage( "That player has no account." );
					return;
				}

				ShowGump( from, HouseIntelView.Cluster, account.Username );
			}
		}

		private class HouseIntelTestTarget : Target
		{
			public HouseIntelTestTarget() : base( 12, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				BaseHouse house = null;

				if ( targeted is BaseHouse )
					house = (BaseHouse)targeted;
				else if ( targeted is HouseSign )
					house = ((HouseSign)targeted).Owner;
				else if ( targeted is Item )
					house = BaseHouse.FindHouseAt( (Item)targeted );
				else if ( targeted is Mobile )
					house = BaseHouse.FindHouseAt( (Mobile)targeted );

				if ( house == null )
				{
					from.SendMessage( "That is not a house." );
					return;
				}

				from.SendGump( new HouseIntelPackGump( from, house, HousePackTarget.OwnerBank ) );
			}
		}
	}

	public enum HouseIntelView
	{
		Cluster,
		AllHouses
	}

	public enum HouseIntelSort
	{
		LastLogin,
		ItemCount,
		Location
	}

	public enum HousePackTarget
	{
		OwnerBank,
		AdminEscrow
	}
}

namespace Server.Gumps
{
	public class HouseIntelGump : Gump
	{
		private const int EntriesPerPage = 20;
		private readonly Mobile m_From;
		private readonly HouseIntelView m_View;
		private readonly string m_SearchTerm;
		private readonly int m_Page;
		private readonly HouseIntelSort m_Sort;
		private readonly bool m_SortAsc;
		private readonly int m_InactiveDays;
		private readonly int m_MinItemCount;
		private readonly int m_MinAccountAge;
		private readonly int m_MaxAccountAge;
		private readonly bool m_HasVendors;
		private readonly bool m_OwnerOnly;
		private readonly bool m_IncludeAssociated;
		private readonly bool m_SubnetMatch;
		private readonly Map m_Facet;

		public HouseIntelGump( Mobile from, HouseIntelView view, string searchTerm )
			: this( from, view, searchTerm, 0, HouseIntelSort.LastLogin, false, HouseIntelConfig.DefaultInactiveDays, 0, 0, 0, false, false, HouseIntelConfig.IncludeAssociatedActivity, HouseIntelConfig.AllowSubnetMatching, null )
		{
		}

		private HouseIntelGump(
			Mobile from,
			HouseIntelView view,
			string searchTerm,
			int page,
			HouseIntelSort sort,
			bool sortAsc,
			int inactiveDays,
			int minItemCount,
			int minAccountAge,
			int maxAccountAge,
			bool hasVendors,
			bool ownerOnly,
			bool includeAssociated,
			bool subnetMatch,
			Map facet )
			: base( 50, 50 )
		{
			m_From = from;
			m_View = view;
			m_SearchTerm = searchTerm;
			m_Page = page;
			m_Sort = sort;
			m_SortAsc = sortAsc;
			m_InactiveDays = inactiveDays;
			m_MinItemCount = minItemCount;
			m_MinAccountAge = minAccountAge;
			m_MaxAccountAge = maxAccountAge;
			m_HasVendors = hasVendors;
			m_OwnerOnly = ownerOnly;
			m_IncludeAssociated = includeAssociated;
			m_SubnetMatch = subnetMatch;
			m_Facet = facet;

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 980, 700, 5054 );
			AddAlphaRegion( 10, 10, 960, 680 );

			AddLabel( 20, 20, 1152, "HouseIntel" );

			AddButton( 20, 50, view == HouseIntelView.Cluster ? 4005 : 4007, view == HouseIntelView.Cluster ? 4007 : 4005, 1, GumpButtonType.Reply, 0 );
			AddLabel( 55, 50, 1152, "Cluster View" );

			AddButton( 160, 50, view == HouseIntelView.AllHouses ? 4005 : 4007, view == HouseIntelView.AllHouses ? 4007 : 4005, 2, GumpButtonType.Reply, 0 );
			AddLabel( 195, 50, 1152, "All Houses View" );

			if ( view == HouseIntelView.Cluster )
				DrawClusterView();
			else
				DrawAllHousesView();
		}

		private void DrawClusterView()
		{
			AddLabel( 20, 90, 1152, "Account/IP:" );
			AddTextEntry( 110, 90, 200, 20, 0, 1, m_SearchTerm ?? String.Empty );
			AddButton( 320, 90, 4011, 4013, 3, GumpButtonType.Reply, 0 );
			AddLabel( 350, 90, 1152, "Search" );

			AddButton( 420, 90, m_SubnetMatch ? 4012 : 4011, m_SubnetMatch ? 4013 : 4012, 4, GumpButtonType.Reply, 0 );
			AddLabel( 450, 90, 1152, "Subnet match" );

			List<ClusterEntry> entries = BuildClusterEntries( m_SearchTerm, m_SubnetMatch );

			int totalPages = Math.Max( 1, (int)Math.Ceiling( entries.Count / (double)EntriesPerPage ) );
			int page = Math.Max( 0, Math.Min( m_Page, totalPages - 1 ) );

			int start = page * EntriesPerPage;
			int end = Math.Min( entries.Count, start + EntriesPerPage );

			int y = 130;
			AddLabel( 20, y, 1152, String.Format( "Results: {0}", entries.Count ) );
			y += 30;

			for ( int i = start; i < end; ++i )
			{
				ClusterEntry entry = entries[i];

				if ( entry.IsAccountHeader )
				{
					AddLabel( 20, y, 88, entry.DisplayText );
					y += 20;
				}
				else if ( entry.House != null )
				{
					AddLabel( 40, y, 1152, entry.DisplayText );
					AddButton( 360, y - 2, 4011, 4013, 1000 + i, GumpButtonType.Reply, 0 );
					AddLabel( 390, y, 1152, "Detail" );
					AddButton( 450, y - 2, 4011, 4013, 2000 + i, GumpButtonType.Reply, 0 );
					AddLabel( 480, y, 1152, "Teleport" );
					y += 20;
				}
			}

			AddPageControls( page, totalPages, 20, 650 );
		}

		private void DrawAllHousesView()
		{
			int y = 90;

			AddLabel( 20, y, 1152, "Filters" );
			y += 20;

			AddLabel( 20, y, 1152, "Inactive > days:" );
			AddTextEntry( 130, y, 50, 20, 0, 2, m_InactiveDays.ToString() );

			AddLabel( 200, y, 1152, "Item count >" );
			AddTextEntry( 300, y, 50, 20, 0, 3, m_MinItemCount.ToString() );

			AddLabel( 370, y, 1152, "Acct age min/max:" );
			AddTextEntry( 500, y, 50, 20, 0, 4, m_MinAccountAge.ToString() );
			AddTextEntry( 560, y, 50, 20, 0, 5, m_MaxAccountAge.ToString() );

			y += 30;

			AddButton( 20, y, m_HasVendors ? 4012 : 4011, m_HasVendors ? 4013 : 4012, 5, GumpButtonType.Reply, 0 );
			AddLabel( 50, y, 1152, "Has vendors" );

			AddButton( 160, y, m_OwnerOnly ? 4012 : 4011, m_OwnerOnly ? 4013 : 4012, 6, GumpButtonType.Reply, 0 );
			AddLabel( 190, y, 1152, "Owner houses only" );

			AddButton( 360, y, m_IncludeAssociated ? 4012 : 4011, m_IncludeAssociated ? 4013 : 4012, 7, GumpButtonType.Reply, 0 );
			AddLabel( 390, y, 1152, "Include co-owner/friend activity" );

			AddLabel( 650, y, 1152, "Facet:" );
			AddTextEntry( 700, y, 90, 20, 0, 6, m_Facet != null ? m_Facet.Name : String.Empty );
			AddButton( 800, y, 4011, 4013, 8, GumpButtonType.Reply, 0 );
			AddLabel( 830, y, 1152, "Apply" );

			y += 30;

			AddLabel( 20, y, 1152, "Sort:" );
			AddButton( 70, y, 4011, 4013, 9, GumpButtonType.Reply, 0 );
			AddLabel( 100, y, 1152, "Last Login" );

			AddButton( 200, y, 4011, 4013, 10, GumpButtonType.Reply, 0 );
			AddLabel( 230, y, 1152, "Item Count" );

			AddButton( 330, y, 4011, 4013, 11, GumpButtonType.Reply, 0 );
			AddLabel( 360, y, 1152, "Location" );

			AddButton( 470, y, m_SortAsc ? 4012 : 4011, m_SortAsc ? 4013 : 4012, 12, GumpButtonType.Reply, 0 );
			AddLabel( 500, y, 1152, m_SortAsc ? "Asc" : "Desc" );

			y += 30;

			List<HouseRow> rows = BuildHouseRows();
			int totalPages = Math.Max( 1, (int)Math.Ceiling( rows.Count / (double)EntriesPerPage ) );
			int page = Math.Max( 0, Math.Min( m_Page, totalPages - 1 ) );

			int start = page * EntriesPerPage;
			int end = Math.Min( rows.Count, start + EntriesPerPage );

			AddLabel( 20, y, 1152, String.Format( "Results: {0}", rows.Count ) );
			y += 20;

			AddLabel( 20, y, 1152, "Loc" );
			AddLabel( 130, y, 1152, "House" );
			AddLabel( 300, y, 1152, "Owner" );
			AddLabel( 420, y, 1152, "Owner Login" );
			AddLabel( 520, y, 1152, "Activity" );
			AddLabel( 620, y, 1152, "Items" );
			AddLabel( 690, y, 1152, "L/S/C/V" );
			AddLabel( 770, y, 1152, "Actions" );
			y += 20;

			for ( int i = start; i < end; ++i )
			{
				HouseRow row = rows[i];
				BaseHouse house = row.House;

				string loc = String.Format( "{0} {1},{2}", house.Map, house.X, house.Y );
				AddLabel( 20, y, 1152, loc );
				AddLabelCropped( 130, y, 160, 20, 1152, row.HouseName );
				AddLabelCropped( 300, y, 120, 20, 1152, row.OwnerName );
				AddLabelCropped( 420, y, 90, 20, 1152, row.OwnerLastLoginText );
				AddLabelCropped( 520, y, 90, 20, 1152, row.ActivityLastLoginText );
				AddLabel( 620, y, 1152, row.Stats.TotalItems.ToString() );
				AddLabel( 690, y, 1152, String.Format( "{0}/{1}/{2}/{3}", row.Stats.LockedDownCount, row.Stats.SecuredCount, row.Stats.ContainerCount, row.Stats.VendorCount ) );

				AddButton( 760, y - 2, 4011, 4013, 3000 + i, GumpButtonType.Reply, 0 );
				AddLabel( 790, y, 1152, "Detail" );
				AddButton( 850, y - 2, 4011, 4013, 4000 + i, GumpButtonType.Reply, 0 );
				AddLabel( 880, y, 1152, "TP" );
				AddButton( 910, y - 2, 4011, 4013, 5000 + i, GumpButtonType.Reply, 0 );
				AddLabel( 940, y, 1152, "Pack" );

				y += 20;
			}

			AddPageControls( page, totalPages, 20, 650 );
		}

		private void AddPageControls( int page, int totalPages, int x, int y )
		{
			AddLabel( x, y, 1152, String.Format( "Page {0}/{1}", page + 1, totalPages ) );

			if ( page > 0 )
				AddButton( x + 80, y, 4014, 4016, 90, GumpButtonType.Reply, 0 );

			if ( page < totalPages - 1 )
				AddButton( x + 110, y, 4005, 4007, 91, GumpButtonType.Reply, 0 );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( sender == null || sender.Mobile == null )
				return;

			Mobile from = sender.Mobile;

			switch ( info.ButtonID )
			{
				case 0:
					return;
				case 1:
					from.SendGump( new HouseIntelGump( from, HouseIntelView.Cluster, m_SearchTerm ) );
					return;
				case 2:
					from.SendGump( new HouseIntelGump( from, HouseIntelView.AllHouses, m_SearchTerm ) );
					return;
			}

			if ( m_View == HouseIntelView.Cluster )
			{
				HandleClusterResponse( from, info );
			}
			else
			{
				HandleAllHousesResponse( from, info );
			}
		}

		private void HandleClusterResponse( Mobile from, RelayInfo info )
		{
			if ( info.ButtonID == 3 )
			{
				string term = info.GetTextEntry( 1 ) != null ? info.GetTextEntry( 1 ).Text.Trim() : null;
				from.SendGump( new HouseIntelGump( from, HouseIntelView.Cluster, term, 0, m_Sort, m_SortAsc, m_InactiveDays, m_MinItemCount, m_MinAccountAge, m_MaxAccountAge, m_HasVendors, m_OwnerOnly, m_IncludeAssociated, m_SubnetMatch, m_Facet ) );
				return;
			}

			if ( info.ButtonID == 4 )
			{
				from.SendGump( new HouseIntelGump( from, HouseIntelView.Cluster, m_SearchTerm, m_Page, m_Sort, m_SortAsc, m_InactiveDays, m_MinItemCount, m_MinAccountAge, m_MaxAccountAge, m_HasVendors, m_OwnerOnly, m_IncludeAssociated, !m_SubnetMatch, m_Facet ) );
				return;
			}

			if ( info.ButtonID == 90 )
			{
				from.SendGump( new HouseIntelGump( from, HouseIntelView.Cluster, m_SearchTerm, m_Page - 1, m_Sort, m_SortAsc, m_InactiveDays, m_MinItemCount, m_MinAccountAge, m_MaxAccountAge, m_HasVendors, m_OwnerOnly, m_IncludeAssociated, m_SubnetMatch, m_Facet ) );
				return;
			}

			if ( info.ButtonID == 91 )
			{
				from.SendGump( new HouseIntelGump( from, HouseIntelView.Cluster, m_SearchTerm, m_Page + 1, m_Sort, m_SortAsc, m_InactiveDays, m_MinItemCount, m_MinAccountAge, m_MaxAccountAge, m_HasVendors, m_OwnerOnly, m_IncludeAssociated, m_SubnetMatch, m_Facet ) );
				return;
			}

			List<ClusterEntry> entries = BuildClusterEntries( m_SearchTerm, m_SubnetMatch );

			if ( info.ButtonID >= 1000 && info.ButtonID < 2000 )
			{
				int index = info.ButtonID - 1000;
				if ( index >= 0 && index < entries.Count )
				{
					BaseHouse house = entries[index].House;
					if ( house != null )
						from.SendGump( new HouseIntelDetailGump( from, house ) );
				}
				return;
			}

			if ( info.ButtonID >= 2000 && info.ButtonID < 3000 )
			{
				int index = info.ButtonID - 2000;
				if ( index >= 0 && index < entries.Count )
				{
					BaseHouse house = entries[index].House;
					if ( house != null )
						TeleportToHouse( from, house, false );
				}
				return;
			}
		}

		private void HandleAllHousesResponse( Mobile from, RelayInfo info )
		{
			string inactiveText = GetEntryText( info, 2, m_InactiveDays.ToString() );
			string minItemText = GetEntryText( info, 3, m_MinItemCount.ToString() );
			string minAgeText = GetEntryText( info, 4, m_MinAccountAge.ToString() );
			string maxAgeText = GetEntryText( info, 5, m_MaxAccountAge.ToString() );
			string facetText = GetEntryText( info, 6, m_Facet != null ? m_Facet.Name : String.Empty );

			int inactiveDays = ParseInt( inactiveText, m_InactiveDays );
			int minItemCount = ParseInt( minItemText, m_MinItemCount );
			int minAge = ParseInt( minAgeText, m_MinAccountAge );
			int maxAge = ParseInt( maxAgeText, m_MaxAccountAge );
			Map facet = ParseFacet( facetText );

			switch ( info.ButtonID )
			{
				case 5:
					from.SendGump( new HouseIntelGump( from, m_View, m_SearchTerm, 0, m_Sort, m_SortAsc, inactiveDays, minItemCount, minAge, maxAge, !m_HasVendors, m_OwnerOnly, m_IncludeAssociated, m_SubnetMatch, facet ) );
					return;
				case 6:
					from.SendGump( new HouseIntelGump( from, m_View, m_SearchTerm, 0, m_Sort, m_SortAsc, inactiveDays, minItemCount, minAge, maxAge, m_HasVendors, !m_OwnerOnly, m_IncludeAssociated, m_SubnetMatch, facet ) );
					return;
				case 7:
					from.SendGump( new HouseIntelGump( from, m_View, m_SearchTerm, 0, m_Sort, m_SortAsc, inactiveDays, minItemCount, minAge, maxAge, m_HasVendors, m_OwnerOnly, !m_IncludeAssociated, m_SubnetMatch, facet ) );
					return;
				case 8:
					from.SendGump( new HouseIntelGump( from, m_View, m_SearchTerm, 0, m_Sort, m_SortAsc, inactiveDays, minItemCount, minAge, maxAge, m_HasVendors, m_OwnerOnly, m_IncludeAssociated, m_SubnetMatch, facet ) );
					return;
				case 9:
					from.SendGump( new HouseIntelGump( from, m_View, m_SearchTerm, 0, HouseIntelSort.LastLogin, m_SortAsc, inactiveDays, minItemCount, minAge, maxAge, m_HasVendors, m_OwnerOnly, m_IncludeAssociated, m_SubnetMatch, facet ) );
					return;
				case 10:
					from.SendGump( new HouseIntelGump( from, m_View, m_SearchTerm, 0, HouseIntelSort.ItemCount, m_SortAsc, inactiveDays, minItemCount, minAge, maxAge, m_HasVendors, m_OwnerOnly, m_IncludeAssociated, m_SubnetMatch, facet ) );
					return;
				case 11:
					from.SendGump( new HouseIntelGump( from, m_View, m_SearchTerm, 0, HouseIntelSort.Location, m_SortAsc, inactiveDays, minItemCount, minAge, maxAge, m_HasVendors, m_OwnerOnly, m_IncludeAssociated, m_SubnetMatch, facet ) );
					return;
				case 12:
					from.SendGump( new HouseIntelGump( from, m_View, m_SearchTerm, m_Page, m_Sort, !m_SortAsc, inactiveDays, minItemCount, minAge, maxAge, m_HasVendors, m_OwnerOnly, m_IncludeAssociated, m_SubnetMatch, facet ) );
					return;
				case 90:
					from.SendGump( new HouseIntelGump( from, m_View, m_SearchTerm, m_Page - 1, m_Sort, m_SortAsc, inactiveDays, minItemCount, minAge, maxAge, m_HasVendors, m_OwnerOnly, m_IncludeAssociated, m_SubnetMatch, facet ) );
					return;
				case 91:
					from.SendGump( new HouseIntelGump( from, m_View, m_SearchTerm, m_Page + 1, m_Sort, m_SortAsc, inactiveDays, minItemCount, minAge, maxAge, m_HasVendors, m_OwnerOnly, m_IncludeAssociated, m_SubnetMatch, facet ) );
					return;
			}

			List<HouseRow> rows = BuildHouseRows();

			if ( info.ButtonID >= 3000 && info.ButtonID < 4000 )
			{
				int index = info.ButtonID - 3000;
				if ( index >= 0 && index < rows.Count )
					from.SendGump( new HouseIntelDetailGump( from, rows[index].House ) );
				return;
			}

			if ( info.ButtonID >= 4000 && info.ButtonID < 5000 )
			{
				int index = info.ButtonID - 4000;
				if ( index >= 0 && index < rows.Count )
					TeleportToHouse( from, rows[index].House, false );
				return;
			}

			if ( info.ButtonID >= 5000 && info.ButtonID < 6000 )
			{
				if ( !HouseIntelConfig.AllowPacking )
				{
					from.SendMessage( "Packing is disabled in HouseIntelConfig." );
					return;
				}

				int index = info.ButtonID - 5000;
				if ( index >= 0 && index < rows.Count )
					from.SendGump( new HouseIntelPackGump( from, rows[index].House, HousePackTarget.OwnerBank ) );
				return;
			}
		}

		private static string GetEntryText( RelayInfo info, int entryId, string fallback )
		{
			if ( info == null )
				return fallback;

			TextRelay relay = info.GetTextEntry( entryId );
			if ( relay == null || String.IsNullOrWhiteSpace( relay.Text ) )
				return fallback;

			return relay.Text.Trim();
		}

		private static int ParseInt( string value, int fallback )
		{
			int parsed;
			return Int32.TryParse( value, out parsed ) ? parsed : fallback;
		}

		private static Map ParseFacet( string value )
		{
			if ( String.IsNullOrWhiteSpace( value ) )
				return null;

			value = value.Trim();

			foreach ( Map map in Map.AllMaps )
			{
				if ( map != null && map.Name != null && map.Name.Equals( value, StringComparison.OrdinalIgnoreCase ) )
					return map;
			}

			return null;
		}

		private List<ClusterEntry> BuildClusterEntries( string term, bool subnetMatch )
		{
			List<ClusterEntry> entries = new List<ClusterEntry>();
			if ( String.IsNullOrWhiteSpace( term ) )
				return entries;

			Account seed = Accounts.GetAccount( term ) as Account;
			IPAddress ip;
			List<IPAddress> seedIPs = new List<IPAddress>();

			if ( seed != null )
			{
				foreach ( Account.LoginIPHistoryEntry entry in seed.LoginIPHistory )
				{
					if ( entry != null && entry.Address != null && !seedIPs.Contains( entry.Address ) )
						seedIPs.Add( entry.Address );
				}

				foreach ( IPAddress address in seed.LoginIPs )
				{
					if ( address != null && !seedIPs.Contains( address ) )
						seedIPs.Add( address );
				}
			}
			else if ( IPAddress.TryParse( term, out ip ) )
			{
				seedIPs.Add( ip );
			}

			if ( seedIPs.Count == 0 )
				return entries;

			List<Account> matchingAccounts = new List<Account>();

			foreach ( Account acct in Accounts.GetAccounts().OfType<Account>() )
			{
				if ( acct == null )
					continue;

				if ( AccountHasSharedIP( acct, seedIPs, subnetMatch ) )
					matchingAccounts.Add( acct );
			}

			matchingAccounts = matchingAccounts.OrderBy( a => a.Username ).ToList();

			foreach ( Account acct in matchingAccounts )
			{
				string header = String.Format( "{0} (Last login: {1})", acct.Username, acct.LastLogin.ToString( "g" ) );
				entries.Add( ClusterEntry.ForAccount( header ) );

				List<BaseHouse> houses = GetAccountHouses( acct );
				foreach ( BaseHouse house in houses )
				{
					string text = FormatHouseLabel( house );
					entries.Add( ClusterEntry.ForHouse( house, text ) );
				}
			}

			return entries;
		}

		private static bool AccountHasSharedIP( Account acct, List<IPAddress> seedIPs, bool subnetMatch )
		{
			if ( acct == null )
				return false;

			foreach ( Account.LoginIPHistoryEntry entry in acct.LoginIPHistory )
			{
				if ( entry == null || entry.Address == null )
					continue;

				if ( seedIPs.Any( ip => IpMatches( ip, entry.Address, subnetMatch ) ) )
					return true;
			}

			foreach ( IPAddress address in acct.LoginIPs )
			{
				if ( address == null )
					continue;

				if ( seedIPs.Any( ip => IpMatches( ip, address, subnetMatch ) ) )
					return true;
			}

			return false;
		}

		private static bool IpMatches( IPAddress left, IPAddress right, bool subnetMatch )
		{
			if ( left == null || right == null )
				return false;

			if ( !subnetMatch )
				return left.Equals( right );

			byte[] a = left.GetAddressBytes();
			byte[] b = right.GetAddressBytes();

			if ( a.Length != 4 || b.Length != 4 )
				return left.Equals( right );

			return a[0] == b[0] && a[1] == b[1] && a[2] == b[2];
		}

		private List<HouseRow> BuildHouseRows()
		{
			List<HouseRow> rows = new List<HouseRow>();

			foreach ( BaseHouse house in BaseHouse.AllHouses )
			{
				if ( house == null || house.Deleted )
					continue;

				if ( m_Facet != null && house.Map != m_Facet )
					continue;

				Mobile owner = house.Owner;
				Account ownerAccount = owner != null ? owner.Account as Account : null;

				if ( m_OwnerOnly && owner == null )
					continue;

				DateTime ownerLastLogin = ownerAccount != null ? ownerAccount.LastLogin : DateTime.MinValue;
				DateTime activityLastLogin = m_IncludeAssociated ? GetAssociatedLastLogin( house, ownerLastLogin ) : ownerLastLogin;

				double daysInactive = activityLastLogin == DateTime.MinValue ? Double.MaxValue : ( DateTime.UtcNow - activityLastLogin ).TotalDays;
				if ( daysInactive < m_InactiveDays )
					continue;

				if ( ownerAccount != null )
				{
					double accountAgeDays = ( DateTime.UtcNow - ownerAccount.Created ).TotalDays;
					if ( m_MinAccountAge > 0 && accountAgeDays < m_MinAccountAge )
						continue;
					if ( m_MaxAccountAge > 0 && accountAgeDays > m_MaxAccountAge )
						continue;
				}

				HouseScanResult stats = HouseScanner.GetStats( house, false );
				if ( stats.TotalItems < m_MinItemCount )
					continue;

				if ( m_HasVendors && stats.VendorCount <= 0 )
					continue;

				rows.Add( new HouseRow
				{
					House = house,
					Stats = stats,
					OwnerName = ownerAccount != null ? ownerAccount.Username : ( owner != null ? owner.Name : "None" ),
					OwnerLastLogin = ownerLastLogin,
					ActivityLastLogin = activityLastLogin
				} );
			}

			IEnumerable<HouseRow> ordered;
			switch ( m_Sort )
			{
				case HouseIntelSort.ItemCount:
					ordered = m_SortAsc ? rows.OrderBy( r => r.Stats.TotalItems ) : rows.OrderByDescending( r => r.Stats.TotalItems );
					break;
				case HouseIntelSort.Location:
					ordered = m_SortAsc ? rows.OrderBy( r => r.House.Map ).ThenBy( r => r.House.X ).ThenBy( r => r.House.Y ) : rows.OrderByDescending( r => r.House.Map ).ThenByDescending( r => r.House.X ).ThenByDescending( r => r.House.Y );
					break;
				default:
					ordered = m_SortAsc ? rows.OrderBy( r => r.ActivityLastLogin ) : rows.OrderByDescending( r => r.ActivityLastLogin );
					break;
			}

			return ordered.ToList();
		}

		private static DateTime GetAssociatedLastLogin( BaseHouse house, DateTime current )
		{
			DateTime newest = current;

			foreach ( object entry in house.CoOwners )
			{
				Mobile coOwner = entry as Mobile;
				Account acct = coOwner != null ? coOwner.Account as Account : null;
				if ( acct != null && acct.LastLogin > newest )
					newest = acct.LastLogin;
			}

			foreach ( object entry in house.Friends )
			{
				Mobile friend = entry as Mobile;
				Account acct = friend != null ? friend.Account as Account : null;
				if ( acct != null && acct.LastLogin > newest )
					newest = acct.LastLogin;
			}

			return newest;
		}

		private static List<BaseHouse> GetAccountHouses( Account account )
		{
			List<BaseHouse> houses = new List<BaseHouse>();
			if ( account == null )
				return houses;

			for ( int i = 0; i < account.Length; i++ )
			{
				Mobile m = account[i];
				if ( m == null )
					continue;

				houses.AddRange( BaseHouse.GetHouses( m ) );

				foreach ( BaseHouse house in BaseHouse.AllHouses )
				{
					if ( house != null && ( house.CoOwners.Contains( m ) || house.Friends.Contains( m ) ) )
						houses.Add( house );
				}
			}

			return houses.Distinct().ToList();
		}

		private static void TeleportToHouse( Mobile from, BaseHouse house, bool inside )
		{
			if ( from == null || house == null )
				return;

			Point3D dest = inside ? GetInsideLocation( house ) : house.BanLocation;
			Map map = house.Map;

			if ( map == null || map == Map.Internal )
				return;

			from.MoveToWorld( dest, map );
		}

		private static Point3D GetInsideLocation( BaseHouse house )
		{
			if ( house == null || house.Map == null )
				return house.BanLocation;

			if ( house.Region != null )
			{
				foreach ( Rectangle3D area in house.Region.Area )
				{
					Point3D center = new Point3D( area.Start.X + area.Width / 2, area.Start.Y + area.Height / 2, house.Z );
					if ( house.Map.CanFit( center, 16, false, false ) )
						return center;
				}
			}

			return house.Location;
		}

		private static string FormatHouseLabel( BaseHouse house )
		{
			if ( house == null )
				return "Unknown house";

			return String.Format( "{0} ({1} {2},{3})", house.GetType().Name, house.Map, house.X, house.Y );
		}

		private class HouseRow
		{
			public BaseHouse House;
			public HouseScanResult Stats;
			public string OwnerName;
			public DateTime OwnerLastLogin;
			public DateTime ActivityLastLogin;

			public string OwnerLastLoginText
			{
				get
				{
					if ( OwnerLastLogin == DateTime.MinValue )
						return "Never";

					TimeSpan span = DateTime.UtcNow - OwnerLastLogin;
					return String.Format( "{0}d", (int)Math.Floor( span.TotalDays ) );
				}
			}

			public string ActivityLastLoginText
			{
				get
				{
					if ( ActivityLastLogin == DateTime.MinValue )
						return "Never";

					TimeSpan span = DateTime.UtcNow - ActivityLastLogin;
					return String.Format( "{0}d", (int)Math.Floor( span.TotalDays ) );
				}
			}

			public string HouseName
			{
				get { return House != null ? House.GetType().Name : "Unknown"; }
			}
		}

		private class ClusterEntry
		{
			public bool IsAccountHeader;
			public string DisplayText;
			public BaseHouse House;

			public static ClusterEntry ForAccount( string text )
			{
				return new ClusterEntry { IsAccountHeader = true, DisplayText = text };
			}

			public static ClusterEntry ForHouse( BaseHouse house, string text )
			{
				return new ClusterEntry { IsAccountHeader = false, House = house, DisplayText = text };
			}
		}
	}

	public class HouseIntelDetailGump : Gump
	{
		private readonly Mobile m_From;
		private readonly BaseHouse m_House;

		public HouseIntelDetailGump( Mobile from, BaseHouse house )
			: base( 100, 100 )
		{
			m_From = from;
			m_House = house;

			AddPage( 0 );
			AddBackground( 0, 0, 520, 420, 5054 );
			AddAlphaRegion( 10, 10, 500, 400 );

			AddLabel( 20, 20, 1152, "House Detail" );

			if ( house == null )
			{
				AddLabel( 20, 50, 33, "House not found." );
				return;
			}

			HouseScanResult stats = HouseScanner.GetStats( house, false );

			int y = 50;

			AddLabel( 20, y, 1152, String.Format( "Type: {0}", house.GetType().Name ) ); y += 20;
			AddLabel( 20, y, 1152, String.Format( "Location: {0} {1},{2}", house.Map, house.X, house.Y ) ); y += 20;
			AddLabel( 20, y, 1152, String.Format( "Owner: {0}", house.Owner != null ? house.Owner.ToString() : "None" ) ); y += 20;
			AddLabel( 20, y, 1152, String.Format( "Co-Owners: {0}", house.CoOwners.Count ) ); y += 20;
			AddLabel( 20, y, 1152, String.Format( "Friends: {0}", house.Friends.Count ) ); y += 20;
			AddLabel( 20, y, 1152, String.Format( "Built: {0:g}", house.BuiltOn ) ); y += 20;
			AddLabel( 20, y, 1152, String.Format( "Items: {0}", stats.TotalItems ) ); y += 20;
			AddLabel( 20, y, 1152, String.Format( "Lockdowns: {0} Secures: {1} Containers: {2} Vendors: {3}", stats.LockedDownCount, stats.SecuredCount, stats.ContainerCount, stats.VendorCount ) ); y += 30;

			AddButton( 20, y, 4011, 4013, 1, GumpButtonType.Reply, 0 );
			AddLabel( 50, y, 1152, "Teleport Outside" );
			AddButton( 180, y, 4011, 4013, 2, GumpButtonType.Reply, 0 );
			AddLabel( 210, y, 1152, "Teleport Inside" );
			AddButton( 330, y, 4011, 4013, 3, GumpButtonType.Reply, 0 );
			AddLabel( 360, y, 1152, "Scan Items" );

			y += 30;

			if ( HouseIntelConfig.AllowPacking )
			{
				AddButton( 20, y, 4011, 4013, 4, GumpButtonType.Reply, 0 );
				AddLabel( 50, y, 1152, "Pack/Migrate" );
			}
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( m_House == null )
				return;

			switch ( info.ButtonID )
			{
				case 1:
					HouseIntelGumpTeleport( false );
					break;
				case 2:
					HouseIntelGumpTeleport( true );
					break;
				case 3:
					HouseScanner.GetStats( m_House, true );
					m_From.SendGump( new HouseIntelDetailGump( m_From, m_House ) );
					break;
				case 4:
					m_From.SendGump( new HouseIntelPackGump( m_From, m_House, HousePackTarget.OwnerBank ) );
					break;
			}
		}

		private void HouseIntelGumpTeleport( bool inside )
		{
			if ( m_From == null || m_House == null )
				return;

			Point3D dest = inside ? m_House.Location : m_House.BanLocation;
			m_From.MoveToWorld( dest, m_House.Map );
		}
	}

	public class HouseIntelPackGump : Gump
	{
		private readonly Mobile m_From;
		private readonly BaseHouse m_House;
		private readonly HousePackTarget m_Target;

		public HouseIntelPackGump( Mobile from, BaseHouse house, HousePackTarget target )
			: base( 120, 120 )
		{
			m_From = from;
			m_House = house;
			m_Target = target;

			AddPage( 0 );
			AddBackground( 0, 0, 560, 500, 5054 );
			AddAlphaRegion( 10, 10, 540, 480 );

			AddLabel( 20, 20, 1152, "House Pack Preview" );

			if ( house == null )
			{
				AddLabel( 20, 60, 33, "House not found." );
				return;
			}

			HousePackPreview preview = HousePacker.BuildPreview( from, house );

			int y = 60;
			AddLabel( 20, y, 1152, String.Format( "Roots to move: {0}", preview.MoveRootCount ) ); y += 20;
			AddLabel( 20, y, 1152, String.Format( "Total items to move: {0}", preview.MoveItemCount ) ); y += 20;
			AddLabel( 20, y, 1152, String.Format( "Skipped: {0} (protected: {1})", preview.SkippedCount, preview.ProtectedSkippedCount ) ); y += 20;
			AddLabel( 20, y, 1152, String.Format( "Addon deeds: {0}", preview.AddonDeedCount ) ); y += 20;

			string ownerBank = preview.OwnerBankHasSpace ? "OK" : "Full/Unknown";
			string escrowBank = preview.EscrowBankHasSpace ? "OK" : "Full/Unknown";
			AddLabel( 20, y, 1152, String.Format( "Owner bank capacity: {0}", ownerBank ) ); y += 20;
			AddLabel( 20, y, 1152, String.Format( "Escrow bank capacity: {0}", escrowBank ) ); y += 30;

			AddLabel( 20, y, 1152, "Top item types:" ); y += 20;

			foreach ( KeyValuePair<string, int> kvp in preview.TopItemCounts )
			{
				AddLabel( 40, y, 1152, String.Format( "{0}: {1}", kvp.Key, kvp.Value ) );
				y += 20;
				if ( y > 300 )
					break;
			}

			y = 320;
			AddLabel( 20, y, 1152, "Target:" );
			AddButton( 80, y, m_Target == HousePackTarget.OwnerBank ? 4005 : 4007, m_Target == HousePackTarget.OwnerBank ? 4007 : 4005, 1, GumpButtonType.Reply, 0 );
			AddLabel( 110, y, 1152, "Owner Bank" );
			AddButton( 220, y, m_Target == HousePackTarget.AdminEscrow ? 4005 : 4007, m_Target == HousePackTarget.AdminEscrow ? 4007 : 4005, 2, GumpButtonType.Reply, 0 );
			AddLabel( 250, y, 1152, "Admin Escrow" );

			AddButton( 20, 430, 4011, 4013, 3, GumpButtonType.Reply, 0 );
			AddLabel( 50, 430, 1152, "Confirm Pack" );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( m_House == null )
				return;

			switch ( info.ButtonID )
			{
				case 1:
					m_From.SendGump( new HouseIntelPackGump( m_From, m_House, HousePackTarget.OwnerBank ) );
					return;
				case 2:
					m_From.SendGump( new HouseIntelPackGump( m_From, m_House, HousePackTarget.AdminEscrow ) );
					return;
				case 3:
					RunPack();
					return;
			}
		}

		private void RunPack()
		{
			if ( !HouseIntelConfig.AllowPacking )
			{
				m_From.SendMessage( "Packing is disabled in HouseIntelConfig." );
				return;
			}

			Container target = ResolveTargetContainer();
			if ( target == null )
			{
				m_From.SendMessage( "No valid target container found for packing." );
				return;
			}

			HousePackResult result = HousePacker.ExecutePack( m_From, m_House, target );

			if ( m_Target == HousePackTarget.OwnerBank && result.Failures.Any( f => f.IndexOf( "Failed to move packing crate", StringComparison.OrdinalIgnoreCase ) >= 0 ) )
			{
				Container escrow = HousePacker.GetEscrowContainer();
				if ( escrow != null && result.Crate != null && HousePacker.TryDrop( m_From, escrow, result.Crate ) )
				{
					CommandLogging.WriteLine( m_From, "HousePack fallback to escrow container." );
					try
					{
						if ( !System.IO.Directory.Exists( "Logs" ) )
							System.IO.Directory.CreateDirectory( "Logs" );

						using ( System.IO.StreamWriter op = new System.IO.StreamWriter( "Logs/HouseIntel.log", true ) )
							op.WriteLine( "{0} HousePack fallback to escrow container.", DateTime.UtcNow );
					}
					catch
					{
					}
					m_From.SendMessage( "Owner bank full or unavailable; packing crate moved to escrow." );
				}
			}

			m_From.SendMessage( "Packed house. Moved: {0}, skipped: {1}, failures: {2}", result.MovedCount, result.SkippedCount, result.Failures.Count );
		}

		private Container ResolveTargetContainer()
		{
			if ( m_Target == HousePackTarget.AdminEscrow )
				return HousePacker.GetEscrowContainer();

			Container ownerBank = HousePacker.GetOwnerBank( m_House );

			if ( ownerBank != null )
				return ownerBank;

			return HousePacker.GetEscrowContainer();
		}
	}
}
