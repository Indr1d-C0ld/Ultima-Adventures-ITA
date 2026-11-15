using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Network;

namespace Server.Multis
{
	public sealed class HousingSuiteCombineGump : HousingSuiteGumpBase
	{
		private const int SelectionRowsPerPage = 2;
		private const int NearbyRowsPerPage = 2;
		private const int ReviewRowsPerPage = 5;

		public const int PauseButton = 90;
		public const int DashboardButton = 1;
		public const int SelectionPreviousButton = 20;
		public const int SelectionNextPageButton = 21;
		public const int NearbyPreviousButton = 22;
		public const int NearbyNextButton = 23;
		public const int SelectedRemoveButtonBase = 1000;
		public const int NearbyAddButtonBase = 1100;
		public const int TargetAddButton = 2000;
		public const int ClearSelectionButton = 2001;
		public const int SelectionNextButton = 2002;
		public const int ValidationNextButton = 2003;
		public const int ReviewNextButton = 2004;
		public const int EditSelectionButton = 2005;
		public const int FinalConfirmButton = 2006;

		private readonly int m_Step;
		private readonly int m_SelectedPage;
		private readonly int m_NearbyPage;
		private readonly Serial[] m_SelectedPageSerials;
		private readonly Serial[] m_NearbySerials;
		private readonly HousingSuiteHouseFingerprint[] m_FinalFingerprints;

		public int Step { get { return m_Step; } }
		public int SelectedPage { get { return m_SelectedPage; } }

		public HousingSuiteCombineGump( Mobile actor, int step, int selectedPage, int nearbyPage, string message )
			: base( actor, HousingSuiteView.HouseSelection, "HousingSuite Combine Plots" )
		{
			m_Step = step < 1 || step > 4 ? 1 : step;
			m_SelectedPageSerials = new Serial[0];
			m_NearbySerials = new Serial[0];
			m_FinalFingerprints = new HousingSuiteHouseFingerprint[0];

			DrawStepNavigation();

			if ( m_Step == 1 )
			{
				Serial[] selectedRows;
				Serial[] nearbyRows;
				int normalizedSelectedPage;
				int normalizedNearbyPage;
				DrawSelection( actor, selectedPage, nearbyPage, message, out selectedRows, out nearbyRows, out normalizedSelectedPage, out normalizedNearbyPage );
				m_SelectedPageSerials = selectedRows;
				m_NearbySerials = nearbyRows;
				m_SelectedPage = normalizedSelectedPage;
				m_NearbyPage = normalizedNearbyPage;
			}
			else
			{
				m_SelectedPage = NormalizePage( selectedPage, RenderedSelectedSerials.Length, ReviewRowsPerPage );
				m_NearbyPage = 0;

				if ( m_Step == 2 )
					DrawValidation( actor, message );
				else if ( m_Step == 3 )
					DrawReview( actor, message );
				else
					m_FinalFingerprints = DrawFinalConfirmation( actor, message );
			}
		}

		public static void Show( Mobile actor, int step, int selectedPage, int nearbyPage, string message )
		{
			if ( actor == null || actor.Deleted || HousingSuiteSession.Find( actor ) == null )
				return;

			actor.CloseGump( typeof( HousingSuiteCombineGump ) );
			actor.SendGump( new HousingSuiteCombineGump( actor, step, selectedPage, nearbyPage, message ) );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile actor;
			HousingSuiteSession session;

			if ( info == null || !TryResolveResponse( sender, out actor, out session ) )
				return;

			int buttonID = info.ButtonID;

			if ( buttonID == 0 || buttonID == PauseButton )
			{
				PauseAndClose( session );
				return;
			}

			if ( buttonID == DashboardButton )
			{
				HousingSuiteGump.Show( actor, 0, String.Empty );
				return;
			}

			if ( m_Step > 1 && buttonID == SelectionPreviousButton && m_SelectedPage > 0 )
			{
				Show( actor, m_Step, m_SelectedPage - 1, 0, String.Empty );
				return;
			}

			if ( m_Step > 1 && buttonID == SelectionNextPageButton && ( m_SelectedPage + 1 ) * ReviewRowsPerPage < RenderedSelectedSerials.Length )
			{
				Show( actor, m_Step, m_SelectedPage + 1, 0, String.Empty );
				return;
			}

			if ( m_Step == 1 )
				HandleSelectionResponse( actor, session, buttonID );
			else if ( m_Step == 2 )
				HandleValidationResponse( actor, buttonID );
			else if ( m_Step == 3 )
				HandleReviewResponse( actor, buttonID );
			else
				HandleFinalResponse( actor, session, buttonID );
		}

		private void HandleSelectionResponse( Mobile actor, HousingSuiteSession session, int buttonID )
		{
			if ( buttonID == TargetAddButton )
			{
				HousingSuiteCommand.BeginHouseSelection( actor, true );
				return;
			}

			if ( buttonID == ClearSelectionButton )
			{
				session.ClearSelectedHouses();
				Show( actor, 1, 0, m_NearbyPage, "The combine selection was cleared." );
				return;
			}

			if ( buttonID == SelectionPreviousButton && m_SelectedPage > 0 )
			{
				Show( actor, 1, m_SelectedPage - 1, m_NearbyPage, String.Empty );
				return;
			}

			if ( buttonID == SelectionNextPageButton && ( m_SelectedPage + 1 ) * SelectionRowsPerPage < session.SelectedHouseCount )
			{
				Show( actor, 1, m_SelectedPage + 1, m_NearbyPage, String.Empty );
				return;
			}

			if ( buttonID == NearbyPreviousButton && m_NearbyPage > 0 )
			{
				Show( actor, 1, m_SelectedPage, m_NearbyPage - 1, String.Empty );
				return;
			}

			if ( buttonID == NearbyNextButton )
			{
				int nearbyCount = HousingSuiteCommand.FindNearbyHouses( actor, session.NearbyRange ).Count;

				if ( ( m_NearbyPage + 1 ) * NearbyRowsPerPage < nearbyCount )
					Show( actor, 1, m_SelectedPage, m_NearbyPage + 1, String.Empty );

				return;
			}

			if ( buttonID >= SelectedRemoveButtonBase && buttonID < SelectedRemoveButtonBase + m_SelectedPageSerials.Length )
			{
				Serial serial = m_SelectedPageSerials[buttonID - SelectedRemoveButtonBase];

				if ( session.RemoveSelectedHouse( serial ) )
					Show( actor, 1, m_SelectedPage, m_NearbyPage, "Removed house " + serial + "." );

				return;
			}

			if ( buttonID >= NearbyAddButtonBase && buttonID < NearbyAddButtonBase + m_NearbySerials.Length )
			{
				Serial serial = m_NearbySerials[buttonID - NearbyAddButtonBase];
				HouseFoundation house = World.FindItem( serial ) as HouseFoundation;

				if ( !IsLiveHouse( house ) || house.Map != actor.Map || !IsWithinRange( actor.Location, house.Location, session.NearbyRange ) )
					return;

				if ( session.SelectedHouseCount == 0 )
				{
					Serial estateSerial;
					int revision;

					if ( !TryGetEstateContext( house, out estateSerial, out revision ) )
						return;

					session.SetSelectedEstate( estateSerial, revision );
				}

				if ( session.AddSelectedHouse( serial ) )
					Show( actor, 1, m_SelectedPage, m_NearbyPage, "Added house " + serial + "." );

				return;
			}

			if ( buttonID == SelectionNextButton )
			{
				List<HouseFoundation> members;
				string error;

				if ( !TryResolveFoundations( RenderedSelectedSerials, out members, out error ) )
				{
					Show( actor, 1, m_SelectedPage, m_NearbyPage, error );
					return;
				}

				HousingValidationResult validation = HousingEstateValidation.ValidateCombine( members );

				if ( !validation.IsValid )
				{
					Show( actor, 1, m_SelectedPage, m_NearbyPage, validation.Message );
					return;
				}

				Show( actor, 2, 0, 0, String.Empty );
			}
		}

		private void HandleValidationResponse( Mobile actor, int buttonID )
		{
			if ( buttonID == EditSelectionButton )
			{
				Show( actor, 1, 0, 0, String.Empty );
				return;
			}

			if ( buttonID != ValidationNextButton )
				return;

			List<HouseFoundation> members;
			string error;

			if ( !TryResolveFoundations( RenderedSelectedSerials, out members, out error ) )
			{
				Show( actor, 2, m_SelectedPage, 0, error );
				return;
			}

			HousingValidationResult validation = HousingEstateValidation.ValidateCombine( members );

			if ( validation.IsValid )
				Show( actor, 3, 0, 0, String.Empty );
			else
				Show( actor, 2, m_SelectedPage, 0, validation.Message );
		}

		private void HandleReviewResponse( Mobile actor, int buttonID )
		{
			if ( buttonID == EditSelectionButton )
			{
				Show( actor, 1, 0, 0, String.Empty );
				return;
			}

			if ( buttonID != ReviewNextButton )
				return;

			List<HouseFoundation> members;
			string error;

			if ( !TryResolveFoundations( RenderedSelectedSerials, out members, out error ) )
			{
				Show( actor, 3, m_SelectedPage, 0, error );
				return;
			}

			HousingValidationResult validation = HousingEstateValidation.ValidateCombine( members );

			if ( validation.IsValid )
				Show( actor, 4, 0, 0, String.Empty );
			else
				Show( actor, 3, m_SelectedPage, 0, validation.Message );
		}

		private void HandleFinalResponse( Mobile actor, HousingSuiteSession session, int buttonID )
		{
			if ( buttonID == EditSelectionButton )
			{
				Show( actor, 1, 0, 0, String.Empty );
				return;
			}

			if ( buttonID != FinalConfirmButton )
				return;

			List<HouseFoundation> members;
			string error;

			if ( !TryResolveFoundations( RenderedSelectedSerials, out members, out error ) )
			{
				Show( actor, 4, 0, 0, error );
				return;
			}

			HousingValidationResult validation = HousingEstateValidation.ValidateCombine( members );

			if ( !validation.IsValid )
			{
				Show( actor, 4, 0, 0, validation.Message );
				return;
			}

			if ( !HousingSuiteHouseFingerprint.MatchesAll( members, m_FinalFingerprints ) )
			{
				Show( actor, 4, 0, 0, "House capacity, ownership, map, location, or decay state changed after this warning was rendered." );
				return;
			}

			HousingEstateController estate;
			List<Serial> serials = new List<Serial>( RenderedSelectedSerials );

			if ( !HousingEstateService.TryCombine( actor, serials, out estate, out error ) )
			{
				Show( actor, 4, 0, 0, error );
				return;
			}

			session.SetSelectedEstate( estate.Serial, estate.CurrentRevision );
			HousingSuiteCommand.Complete( actor );
			actor.SendMessage( "Combined {0} plots into estate {1} at revision {2}.", serials.Count, estate.Serial, estate.CurrentRevision );
		}

		private void DrawStepNavigation()
		{
			AddSectionHeader( NavigationX + 15, NavigationY + 15, NavigationWidth - 30, "Four-Step Combine" );
			AddHtml( NavigationX + 15, NavigationY + 52, NavigationWidth - 30, 26, m_Step == 1 ? Orange( "1. Select houses" ) : White( "1. Select houses" ), false, false );
			AddHtml( NavigationX + 15, NavigationY + 84, NavigationWidth - 30, 26, m_Step == 2 ? Orange( "2. Check validation" ) : White( "2. Check validation" ), false, false );
			AddHtml( NavigationX + 15, NavigationY + 116, NavigationWidth - 30, 26, m_Step == 3 ? Orange( "3. Review before / after" ) : White( "3. Review before / after" ), false, false );
			AddHtml( NavigationX + 15, NavigationY + 148, NavigationWidth - 30, 26, m_Step == 4 ? Orange( "4. Final confirmation" ) : White( "4. Final confirmation" ), false, false );
			AddWrappedText( NavigationX + 15, NavigationY + 205, NavigationWidth - 30, 145, "Pause and Close keeps the serial-only draft. Edit Selection returns safely without changing any houses. No service transaction occurs until the separate final confirmation response." );
			AddAction( NavigationX + 15, NavigationY + 445, PauseButton, "Pause and Close" );
			AddAction( NavigationX + 15, NavigationY + 485, DashboardButton, "Dashboard" );
		}

		private void DrawSelection( Mobile actor, int selectedPage, int nearbyPage, string message, out Serial[] selectedRows, out Serial[] nearbyRows, out int normalizedSelectedPage, out int normalizedNearbyPage )
		{
			Serial[] selected = RenderedSelectedSerials;
			normalizedSelectedPage = NormalizePage( selectedPage, selected.Length, SelectionRowsPerPage );
			int selectedStart = normalizedSelectedPage * SelectionRowsPerPage;
			selectedRows = Slice( selected, selectedStart, SelectionRowsPerPage );

			HousingSuiteSession session = HousingSuiteSession.Find( actor );
			int nearbyRange = session == null ? HousingSuiteCommand.DefaultNearbyRange : session.NearbyRange;
			IList<BaseHouse> nearby = HousingSuiteCommand.FindNearbyHouses( actor, nearbyRange );
			normalizedNearbyPage = NormalizePage( nearbyPage, nearby.Count, NearbyRowsPerPage );
			int nearbyStart = normalizedNearbyPage * NearbyRowsPerPage;
			int nearbyCount = Math.Min( NearbyRowsPerPage, Math.Max( 0, nearby.Count - nearbyStart ) );
			nearbyRows = new Serial[nearbyCount];

			for ( int i = 0; i < nearbyCount; ++i )
				nearbyRows[i] = nearby[nearbyStart + i].Serial;

			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, "Step 1 — Select Houses" );
			AddWrappedText( ContentX + 15, ContentY + 42, ContentWidth - 30, 54, "Add adjacent custom foundations from the bounded nearby rows or use a target. Remove individual serials, clear the draft, or continue only when at least two eligible connected plots are selected." );

			if ( !String.IsNullOrEmpty( message ) )
				AddHtml( ContentX + 15, ContentY + 98, ContentWidth - 30, 24, Orange( message ), false, false );

			AddSectionHeader( ContentX + 15, ContentY + 125, ContentWidth - 30, "Selected Serial Draft" );

			for ( int i = 0; i < selectedRows.Length; ++i )
			{
				int y = ContentY + 153 + ( i * 55 );
				BaseHouse house = World.FindItem( selectedRows[i] ) as BaseHouse;
				AddPanel( ContentX + 15, y, ContentWidth - 30, 47 );
				AddHtml( ContentX + 25, y + 12, ContentWidth - 105, 22, White( DescribeHouse( selectedRows[i], house ) ), false, false );
				AddButton( ContentX + ContentWidth - 55, y + 11, ActionNormalID, ActionPressedID, SelectedRemoveButtonBase + i, GumpButtonType.Reply, 0 );
			}

			DrawPager( ContentY + 266, normalizedSelectedPage, selected.Length, SelectionRowsPerPage, SelectionPreviousButton, SelectionNextPageButton );
			AddSectionHeader( ContentX + 15, ContentY + 300, ContentWidth - 30, "Nearby Custom Foundations" );

			for ( int i = 0; i < nearbyRows.Length; ++i )
			{
				int y = ContentY + 328 + ( i * 62 );
				BaseHouse house = nearby[nearbyStart + i];
				AddPanel( ContentX + 15, y, ContentWidth - 30, 54 );
				AddHtml( ContentX + 25, y + 7, ContentWidth - 105, 20, White( DescribeHouse( house.Serial, house ) ), false, false );
				AddHtml( ContentX + 25, y + 29, ContentWidth - 105, 20, Blue( DescribeMap( house.Map ) + " " + house.Location + " | owner " + DescribeMobile( house.Owner ) ), false, false );
				AddButton( ContentX + ContentWidth - 55, y + 16, ActionNormalID, ActionPressedID, NearbyAddButtonBase + i, GumpButtonType.Reply, 0 );
			}

			DrawPager( ContentY + 458, normalizedNearbyPage, nearby.Count, NearbyRowsPerPage, NearbyPreviousButton, NearbyNextButton );
			AddAction( ContentX + 25, ContentY + 497, TargetAddButton, "Add by Target" );
			AddAction( ContentX + 220, ContentY + 497, ClearSelectionButton, "Clear Selection" );
			AddAction( ContentX + 415, ContentY + 497, SelectionNextButton, "Check Selection" );
		}

		private void DrawValidation( Mobile actor, string message )
		{
			List<HouseFoundation> members;
			string error;
			HousingValidationResult validation = TryResolveFoundations( RenderedSelectedSerials, out members, out error )
				? HousingEstateValidation.ValidateCombine( members )
				: new HousingValidationResult( false, error );

			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, "Step 2 — Fresh Information and Validation" );
			AddHtml( ContentX + 15, ContentY + 44, ContentWidth - 30, 42, validation.IsValid ? Blue( "VALID: same owner, same map, connected plots, no active customizer, and no current estate membership." ) : Orange( "BLOCKED: " + validation.Message ), false, false );

			if ( !String.IsNullOrEmpty( message ) )
				AddHtml( ContentX + 15, ContentY + 88, ContentWidth - 30, 24, Orange( message ), false, false );

			DrawSelectedDetailRows( members, false );
			AddAction( ContentX + 25, ContentY + 497, EditSelectionButton, "Edit Selection" );

			if ( validation.IsValid )
				AddAction( ContentX + 410, ContentY + 497, ValidationNextButton, "Review Changes" );
		}

		private void DrawReview( Mobile actor, string message )
		{
			List<HouseFoundation> members;
			string error;
			bool resolved = TryResolveFoundations( RenderedSelectedSerials, out members, out error );
			HousingValidationResult validation = resolved ? HousingEstateValidation.ValidateCombine( members ) : new HousingValidationResult( false, error );
			HousingEstateCapacityTotals totals = resolved ? SumCapacities( members ) : new HousingEstateCapacityTotals();

			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, "Step 3 — Before / After Review" );
			AddWrappedText( ContentX + 15, ContentY + 42, ContentWidth - 30, 63, "Each custom foundation remains a separately editable client plot because of the client plot-size limit. Combining changes logical ownership, decay protection, pooled storage, pooled lockdowns, and pooled vendor capacity; it does not merge foundation design canvases." );
			AddHtml( ContentX + 15, ContentY + 108, ContentWidth - 30, 38, Blue( String.Format( "After combine totals — storage {0}/{1}, lockdowns {2}/{3}, vendors {4}/{5}", totals.CurrentStorage, totals.MaxStorage, totals.CurrentLockdowns, totals.MaxLockdowns, totals.CurrentVendors, totals.MaxVendors ) ), false, false );

			if ( !validation.IsValid )
				AddHtml( ContentX + 15, ContentY + 145, ContentWidth - 30, 34, Orange( validation.Message ), false, false );

			DrawCapacityRows( members, ContentY + 184 );
			DrawPager( ContentY + 464, m_SelectedPage, RenderedSelectedSerials.Length, ReviewRowsPerPage, SelectionPreviousButton, SelectionNextPageButton );
			AddAction( ContentX + 25, ContentY + 497, EditSelectionButton, "Edit Selection" );

			if ( validation.IsValid )
				AddAction( ContentX + 395, ContentY + 497, ReviewNextButton, "Final Confirmation" );
		}

		private HousingSuiteHouseFingerprint[] DrawFinalConfirmation( Mobile actor, string message )
		{
			List<HouseFoundation> members;
			string error;
			bool resolved = TryResolveFoundations( RenderedSelectedSerials, out members, out error );
			HousingValidationResult validation = resolved ? HousingEstateValidation.ValidateCombine( members ) : new HousingValidationResult( false, error );
			HousingEstateCapacityTotals totals = resolved ? SumCapacities( members ) : new HousingEstateCapacityTotals();

			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, "Step 4 — Final Warning" );
			AddWrappedText( ContentX + 15, ContentY + 44, ContentWidth - 30, 90, "Confirming creates an immutable original snapshot and a new estate controller, records the first retained revision, and restricts member decay. Later split and restore operations remain available while the estate is healthy. This response will fail safely if any rendered serial, capacity, owner, map, location, estate revision, or session state changes." );
			AddHtml( ContentX + 15, ContentY + 140, ContentWidth - 30, 45, Blue( String.Format( "{0} plots | storage {1}/{2} | lockdowns {3}/{4} | vendors {5}/{6}", RenderedSelectedSerials.Length, totals.CurrentStorage, totals.MaxStorage, totals.CurrentLockdowns, totals.MaxLockdowns, totals.CurrentVendors, totals.MaxVendors ) ), false, false );

			if ( !String.IsNullOrEmpty( message ) )
				AddHtml( ContentX + 15, ContentY + 188, ContentWidth - 30, 34, Orange( message ), false, false );

			DrawSerialSummary( ContentY + 230 );
			DrawPager( ContentY + 430, m_SelectedPage, RenderedSelectedSerials.Length, ReviewRowsPerPage, SelectionPreviousButton, SelectionNextPageButton );
			AddAction( ContentX + 25, ContentY + 497, EditSelectionButton, "Edit Selection" );

			if ( validation.IsValid )
				AddAction( ContentX + 400, ContentY + 497, FinalConfirmButton, "Confirm Combine" );

			return resolved ? HousingSuiteHouseFingerprint.CaptureAll( members ) : new HousingSuiteHouseFingerprint[0];
		}

		private void DrawSelectedDetailRows( IList<HouseFoundation> members, bool capacities )
		{
			Serial[] selected = RenderedSelectedSerials;
			int start = m_SelectedPage * ReviewRowsPerPage;
			int count = Math.Min( ReviewRowsPerPage, Math.Max( 0, selected.Length - start ) );

			for ( int i = 0; i < count; ++i )
			{
				int index = start + i;
				HouseFoundation house = members != null && index < members.Count ? members[index] : null;
				int y = ContentY + 125 + ( i * 68 );
				AddPanel( ContentX + 15, y, ContentWidth - 30, 60 );
				AddHtml( ContentX + 25, y + 7, ContentWidth - 50, 21, White( DescribeHouse( selected[index], house ) ), false, false );
				string detail = house == null ? "Missing live foundation." : String.Format( "owner {0} | {1} {2} | active {3} | customizer {4} | estate eligible {5}", DescribeMobile( house.Owner ), DescribeMap( house.Map ), house.Location, house.IsActive, house.Customizer == null ? "none" : DescribeMobile( house.Customizer ), IsStandalone( house ) ? "yes" : "no" );
				AddHtml( ContentX + 25, y + 31, ContentWidth - 50, 22, Blue( detail ), false, false );
			}

			DrawPager( ContentY + 464, m_SelectedPage, selected.Length, ReviewRowsPerPage, SelectionPreviousButton, SelectionNextPageButton );
		}

		private void DrawCapacityRows( IList<HouseFoundation> members, int firstY )
		{
			int start = m_SelectedPage * ReviewRowsPerPage;
			int count = members == null ? 0 : Math.Min( ReviewRowsPerPage, Math.Max( 0, members.Count - start ) );

			for ( int i = 0; i < count; ++i )
			{
				HouseFoundation house = members[start + i];
				HousingSuiteHouseFingerprint values = HousingSuiteHouseFingerprint.Capture( house );
				int y = firstY + ( i * 55 );
				AddPanel( ContentX + 15, y, ContentWidth - 30, 47 );
				AddHtml( ContentX + 25, y + 7, ContentWidth - 50, 20, White( house.Serial + " " + house.GetType().Name + " | " + DescribeMobile( house.Owner ) + " | decay restricted " + house.RestrictDecay ), false, false );
				AddHtml( ContentX + 25, y + 27, ContentWidth - 50, 18, Blue( String.Format( "local storage {0}/{1} | lockdowns {2}/{3} | vendors {4}/{5}", values.CurrentStorage, values.MaxStorage, values.CurrentLockdowns, values.MaxLockdowns, values.CurrentVendors, values.MaxVendors ) ), false, false );
			}
		}

		private void DrawSerialSummary( int firstY )
		{
			Serial[] selected = RenderedSelectedSerials;
			int start = m_SelectedPage * ReviewRowsPerPage;
			int count = Math.Min( ReviewRowsPerPage, Math.Max( 0, selected.Length - start ) );

			for ( int i = 0; i < count; ++i )
				AddHtml( ContentX + 25, firstY + ( i * 32 ), ContentWidth - 50, 24, White( "Plot " + ( start + i + 1 ) + ": " + selected[start + i] ), false, false );
		}

		private void DrawPager( int y, int page, int count, int rowsPerPage, int previousButton, int nextButton )
		{
			int totalPages = Math.Max( 1, ( count + rowsPerPage - 1 ) / rowsPerPage );
			AddHtml( ContentX + 260, y + 1, 120, 22, WhiteCentered( String.Format( "Page {0} of {1}", page + 1, totalPages ) ), false, false );

			if ( page > 0 )
				AddAction( ContentX + 25, y, previousButton, "Previous" );

			if ( page + 1 < totalPages )
				AddAction( ContentX + ContentWidth - 190, y, nextButton, "Next" );
		}

		private static bool TryResolveFoundations( Serial[] serials, out List<HouseFoundation> members, out string error )
		{
			members = new List<HouseFoundation>( serials == null ? 0 : serials.Length );
			error = String.Empty;

			if ( serials == null )
			{
				error = "No selected serials were rendered.";
				return false;
			}

			for ( int i = 0; i < serials.Length; ++i )
			{
				HouseFoundation house = World.FindItem( serials[i] ) as HouseFoundation;

				if ( !IsLiveHouse( house ) )
				{
					error = "Selected house " + serials[i] + " is missing, deleted, or not a live custom foundation.";
					return false;
				}

				for ( int j = 0; j < i; ++j )
				{
					if ( serials[j] == serials[i] )
					{
						error = "Selected house " + serials[i] + " is duplicated.";
						return false;
					}
				}

				members.Add( house );
			}

			return true;
		}

		private static bool IsStandalone( HouseFoundation house )
		{
			HousingEstateController estate;
			return house != null && !HousingEstateRegistry.TryGetEstate( house, out estate );
		}

		private static string DescribeHouse( Serial serial, BaseHouse house )
		{
			return house == null ? "Missing house " + serial : house.GetType().Name + " " + serial + " | " + DescribeMap( house.Map ) + " " + house.Location;
		}

		private static int NormalizePage( int page, int count, int rowsPerPage )
		{
			int totalPages = Math.Max( 1, ( count + rowsPerPage - 1 ) / rowsPerPage );
			return Math.Max( 0, Math.Min( page, totalPages - 1 ) );
		}

		private static Serial[] Slice( Serial[] values, int start, int maximum )
		{
			int count = Math.Min( maximum, Math.Max( 0, values.Length - start ) );
			Serial[] result = new Serial[count];

			for ( int i = 0; i < count; ++i )
				result[i] = values[start + i];

			return result;
		}

		private static HousingEstateCapacityTotals SumCapacities( IList<HouseFoundation> members )
		{
			long maxStorage = 0;
			long currentStorage = 0;
			long maxLockdowns = 0;
			long currentLockdowns = 0;
			long maxVendors = 0;
			long currentVendors = 0;

			if ( members != null )
			{
				for ( int i = 0; i < members.Count; ++i )
				{
					HousingSuiteHouseFingerprint value = HousingSuiteHouseFingerprint.Capture( members[i] );
					maxStorage += value.MaxStorage;
					currentStorage += value.CurrentStorage;
					maxLockdowns += value.MaxLockdowns;
					currentLockdowns += value.CurrentLockdowns;
					maxVendors += value.MaxVendors;
					currentVendors += value.CurrentVendors;
				}
			}

			HousingEstateCapacityTotals totals = new HousingEstateCapacityTotals();
			totals.MaxStorage = HousingEstateCapacityTotals.Clamp( maxStorage );
			totals.CurrentStorage = HousingEstateCapacityTotals.Clamp( currentStorage );
			totals.MaxLockdowns = HousingEstateCapacityTotals.Clamp( maxLockdowns );
			totals.CurrentLockdowns = HousingEstateCapacityTotals.Clamp( currentLockdowns );
			totals.MaxVendors = HousingEstateCapacityTotals.Clamp( maxVendors );
			totals.CurrentVendors = HousingEstateCapacityTotals.Clamp( currentVendors );
			return totals;
		}
	}

	internal struct HousingSuiteHouseFingerprint
	{
		public Serial Serial;
		public Serial OwnerSerial;
		public int MapIndex;
		public Point3D Location;
		public bool RestrictDecay;
		public int MaxStorage;
		public int CurrentStorage;
		public int MaxLockdowns;
		public int CurrentLockdowns;
		public int MaxVendors;
		public int CurrentVendors;

		public static HousingSuiteHouseFingerprint Capture( BaseHouse house )
		{
			HousingSuiteHouseFingerprint value = new HousingSuiteHouseFingerprint();

			if ( house == null )
				return value;

			int fromSecures;
			int fromVendors;
			int fromLockdowns;
			int fromMovingCrate;
			value.Serial = house.Serial;
			value.OwnerSerial = house.Owner == null ? Server.Serial.MinusOne : house.Owner.Serial;
			value.MapIndex = house.Map == null ? -1 : house.Map.MapIndex;
			value.Location = house.Location;
			value.RestrictDecay = house.RestrictDecay;
			value.MaxStorage = house.GetLocalAosMaxSecures();
			value.CurrentStorage = house.GetLocalAosCurSecures( out fromSecures, out fromVendors, out fromLockdowns, out fromMovingCrate );
			value.MaxLockdowns = house.GetLocalAosMaxLockdowns();
			value.CurrentLockdowns = house.GetLocalAosCurLockdowns();
			value.MaxVendors = house.GetLocalNewVendorSystemMaxVendors();
			value.CurrentVendors = house.GetLocalNewVendorSystemCurVendors();
			return value;
		}

		public static bool Matches( BaseHouse house, HousingSuiteHouseFingerprint expected )
		{
			HousingSuiteHouseFingerprint actual = Capture( house );

			return actual.Serial == expected.Serial
				&& actual.OwnerSerial == expected.OwnerSerial
				&& actual.MapIndex == expected.MapIndex
				&& actual.Location == expected.Location
				&& actual.RestrictDecay == expected.RestrictDecay
				&& actual.MaxStorage == expected.MaxStorage
				&& actual.CurrentStorage == expected.CurrentStorage
				&& actual.MaxLockdowns == expected.MaxLockdowns
				&& actual.CurrentLockdowns == expected.CurrentLockdowns
				&& actual.MaxVendors == expected.MaxVendors
				&& actual.CurrentVendors == expected.CurrentVendors;
		}

		public static HousingSuiteHouseFingerprint[] CaptureAll( IList<HouseFoundation> houses )
		{
			HousingSuiteHouseFingerprint[] values = new HousingSuiteHouseFingerprint[houses == null ? 0 : houses.Count];

			for ( int i = 0; i < values.Length; ++i )
				values[i] = Capture( houses[i] );

			return values;
		}

		public static HousingSuiteHouseFingerprint[] CaptureAll( IList<BaseHouse> houses )
		{
			HousingSuiteHouseFingerprint[] values = new HousingSuiteHouseFingerprint[houses == null ? 0 : houses.Count];

			for ( int i = 0; i < values.Length; ++i )
				values[i] = Capture( houses[i] );

			return values;
		}

		public static bool MatchesAll( IList<HouseFoundation> houses, HousingSuiteHouseFingerprint[] expected )
		{
			if ( houses == null || expected == null || houses.Count != expected.Length )
				return false;

			for ( int i = 0; i < houses.Count; ++i )
			{
				if ( !Matches( houses[i], expected[i] ) )
					return false;
			}

			return true;
		}

		public static bool MatchesAll( IList<BaseHouse> houses, HousingSuiteHouseFingerprint[] expected )
		{
			if ( houses == null || expected == null || houses.Count != expected.Length )
				return false;

			for ( int i = 0; i < houses.Count; ++i )
			{
				if ( !Matches( houses[i], expected[i] ) )
					return false;
			}

			return true;
		}
	}
}
