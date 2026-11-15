using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Network;

namespace Server.Multis
{
	internal static class HousingSuiteGumpOperations
	{
		public static int NormalizePage( int page, int count, int rowsPerPage )
		{
			int totalPages = Math.Max( 1, ( count + rowsPerPage - 1 ) / rowsPerPage );
			return Math.Max( 0, Math.Min( page, totalPages - 1 ) );
		}

		public static bool TryResolveEstate( Serial serial, int revision, out HousingEstateController estate, out string error )
		{
			estate = World.FindItem( serial ) as HousingEstateController;
			error = String.Empty;

			if ( estate == null || estate.Deleted || World.FindItem( serial ) != estate )
			{
				error = "The selected estate controller is missing or deleted.";
				return false;
			}

			if ( estate.CurrentRevision != revision )
			{
				error = "The selected estate revision changed.";
				return false;
			}

			return true;
		}

		public static bool TryResolveHealthyEstate( Serial serial, int revision, out HousingEstateController estate, out string error )
		{
			if ( !TryResolveEstate( serial, revision, out estate, out error ) )
				return false;

			if ( estate.Health != HousingEstateHealth.Healthy )
			{
				error = String.IsNullOrEmpty( estate.DegradedReason ) ? "The selected estate is degraded." : estate.DegradedReason;
				return false;
			}

			HousingValidationResult validation = estate.ValidateMembers();

			if ( !validation.IsValid )
			{
				error = validation.Message;
				return false;
			}

			return true;
		}

		public static bool TryResolveAnchor( Serial[] selected, out HouseFoundation anchor, out string error )
		{
			anchor = null;
			error = String.Empty;

			if ( selected == null || selected.Length == 0 )
			{
				error = "Select a house before opening this operation.";
				return false;
			}

			anchor = World.FindItem( selected[0] ) as HouseFoundation;

			if ( anchor == null || anchor.Deleted || World.FindItem( selected[0] ) != anchor || !HousingSuiteCommand.IsValidWorldMap( anchor.Map ) )
			{
				anchor = null;
				error = "The selected anchor is missing, deleted, or not a custom foundation.";
				return false;
			}

			return true;
		}

		public static bool TryResolveHouse( Serial[] selected, out BaseHouse house, out string error )
		{
			house = null;
			error = String.Empty;

			if ( selected == null || selected.Length == 0 )
			{
				error = "Select a house before opening this operation.";
				return false;
			}

			house = World.FindItem( selected[0] ) as BaseHouse;

			if ( house == null || house.Deleted || World.FindItem( selected[0] ) != house || !HousingSuiteCommand.IsValidWorldMap( house.Map ) )
			{
				house = null;
				error = "The selected house is missing, deleted, or on an invalid map.";
				return false;
			}

			return true;
		}

		public static List<BaseHouse> CopyBaseHouses( IList<HouseFoundation> foundations )
		{
			List<BaseHouse> houses = new List<BaseHouse>( foundations == null ? 0 : foundations.Count );

			if ( foundations != null )
			{
				for ( int i = 0; i < foundations.Count; ++i )
					houses.Add( foundations[i] );
			}

			return houses;
		}

		public static HousingValidationResult ValidateRestore( HousingEstateController estate, int revision, out HousingEstateSnapshot snapshot )
		{
			snapshot = null;

			if ( estate == null || estate.Deleted )
				return new HousingValidationResult( false, "The estate controller is missing or deleted." );

			if ( estate.Health != HousingEstateHealth.Healthy )
				return new HousingValidationResult( false, String.IsNullOrEmpty( estate.DegradedReason ) ? "The estate is degraded." : estate.DegradedReason );

			snapshot = estate.FindRevision( revision );

			if ( snapshot == null )
				return new HousingValidationResult( false, "The requested revision is missing, pruned, or ambiguous." );

			HousingValidationResult validation = snapshot.ValidateRestoreReferences();

			if ( !validation.IsValid )
				return validation;

			List<HouseFoundation> members = estate.CopyMembers();

			if ( !snapshot.MatchesMembership( members ) )
				return new HousingValidationResult( false, "The requested revision has incompatible estate membership." );

			return snapshot.ValidateRestoreDestinations( members );
		}

		public static string CapacityLine( HouseFoundation house, out bool overCapacity )
		{
			overCapacity = false;

			if ( house == null || house.Deleted )
			{
				overCapacity = true;
				return "missing house";
			}

			HousingSuiteHouseFingerprint value = HousingSuiteHouseFingerprint.Capture( house );
			long storageOver = Math.Max( 0L, (long) value.CurrentStorage - value.MaxStorage );
			long lockdownOver = Math.Max( 0L, (long) value.CurrentLockdowns - value.MaxLockdowns );
			long vendorOver = Math.Max( 0L, (long) value.CurrentVendors - value.MaxVendors );
			overCapacity = storageOver > 0 || lockdownOver > 0 || vendorOver > 0;

			return String.Format( "storage {0}/{1}{2} | lockdowns {3}/{4}{5} | vendors {6}/{7}{8}",
				value.CurrentStorage, value.MaxStorage, storageOver > 0 ? " OVER +" + storageOver : String.Empty,
				value.CurrentLockdowns, value.MaxLockdowns, lockdownOver > 0 ? " OVER +" + lockdownOver : String.Empty,
				value.CurrentVendors, value.MaxVendors, vendorOver > 0 ? " OVER +" + vendorOver : String.Empty );
		}

		public static HousingValidationResult ValidatePredictedEstateConnectivity( HouseFoundation movedHouse, Serial expectedEstateSerial, int expectedRevision, Map destinationMap, int deltaX, int deltaY )
		{
			if ( movedHouse == null || movedHouse.Deleted )
				return new HousingValidationResult( false, "The selected estate member is missing or deleted." );

			HousingEstateController estate;

			if ( !HousingEstateRegistry.TryGetEstate( movedHouse, out estate ) )
				return expectedEstateSerial == Server.Serial.MinusOne
					? HousingValidationResult.Valid
					: new HousingValidationResult( false, "The selected house is no longer in the expected estate." );

			if ( estate.Serial != expectedEstateSerial || estate.CurrentRevision != expectedRevision )
				return new HousingValidationResult( false, "The selected estate changed before connectivity review." );

			string error;

			if ( !TryResolveHealthyEstate( expectedEstateSerial, expectedRevision, out estate, out error ) )
				return new HousingValidationResult( false, error );

			if ( !HousingSuiteCommand.IsValidWorldMap( destinationMap ) )
				return new HousingValidationResult( false, "The predicted destination map is invalid." );

			List<HouseFoundation> members = estate.CopyMembers();

			if ( members.Count == 0 )
				return new HousingValidationResult( false, "The expected estate has no live members." );

			Rectangle2D[] plots = new Rectangle2D[members.Count];
			bool foundMovedHouse = false;

			for ( int i = 0; i < members.Count; ++i )
			{
				HouseFoundation member = members[i];

				if ( member == null || member.Deleted || World.FindItem( member.Serial ) != member )
					return new HousingValidationResult( false, "The estate contains a missing member." );

				bool moved = member.Serial == movedHouse.Serial;
				Map predictedMap = moved ? destinationMap : member.Map;

				if ( moved )
					foundMovedHouse = true;

				if ( predictedMap != destinationMap )
					return new HousingValidationResult( false, "Moving this member would place the estate on different maps." );

				MultiComponentList components = member.Components;

				if ( components == null || components.Width <= 0 || components.Height <= 0 )
					return new HousingValidationResult( false, "Estate member " + member.Serial + " has invalid component dimensions." );

				long x = (long) member.X + ( moved ? deltaX : 0 ) + components.Min.X;
				long y = (long) member.Y + ( moved ? deltaY : 0 ) + components.Min.Y;
				long endX = x + components.Width;
				long endY = y + components.Height;

				if ( x < 0 || y < 0 || endX > destinationMap.Width || endY > destinationMap.Height || endX <= x || endY <= y )
					return new HousingValidationResult( false, "The predicted estate footprint is outside map bounds." );

				plots[i] = new Rectangle2D( (int) x, (int) y, components.Width, components.Height );
			}

			if ( !foundMovedHouse )
				return new HousingValidationResult( false, "The selected house is not a live member of the expected estate." );

			bool[] visited = new bool[plots.Length];
			int[] queue = new int[plots.Length];
			int queueStart = 0;
			int queueEnd = 0;
			int visitedCount = 1;

			visited[0] = true;
			queue[queueEnd++] = 0;

			while ( queueStart < queueEnd )
			{
				int current = queue[queueStart++];

				for ( int i = 0; i < plots.Length; ++i )
				{
					if ( visited[i] || !HousingEstateGeometry.AreConnected( plots[current], plots[i], 1 ) )
						continue;

					visited[i] = true;
					queue[queueEnd++] = i;
					++visitedCount;
				}
			}

			return visitedCount == plots.Length
				? HousingValidationResult.Valid
				: new HousingValidationResult( false, "Moving this member would disconnect the estate plot graph." );
		}
	}

	public sealed class HousingSuiteSplitGump : HousingSuiteGumpBase
	{
		private const int RowsPerPage = 5;

		public const int DashboardButton = 1;
		public const int PreviousPageButton = 20;
		public const int NextPageButton = 21;
		public const int ReviewButton = 2100;
		public const int ConfirmButton = 2101;
		public const int ForceDissolveButton = 2400;

		private readonly bool m_Confirming;
		private readonly int m_Page;

		public bool Confirming { get { return m_Confirming; } }
		public int Page { get { return m_Page; } }

		public HousingSuiteSplitGump( Mobile actor, bool confirming, string message )
			: this( actor, confirming, 0, message )
		{
		}

		public HousingSuiteSplitGump( Mobile actor, bool confirming, int page, string message )
			: base( actor, HousingSuiteView.Dashboard, "HousingSuite Split Estate" )
		{
			m_Confirming = confirming;
			HousingEstateController estate;
			string error;
			int count = HousingSuiteGumpOperations.TryResolveEstate( RenderedEstateSerial, RenderedEstateRevision, out estate, out error ) ? estate.MemberCount : 0;
			m_Page = HousingSuiteGumpOperations.NormalizePage( page, count, RowsPerPage );
			DrawNavigation();
			DrawContent( actor, message );
		}

		public static void Show( Mobile actor, bool confirming, string message )
		{
			Show( actor, confirming, 0, message );
		}

		public static void Show( Mobile actor, bool confirming, int page, string message )
		{
			if ( actor == null || actor.Deleted || HousingSuiteSession.Find( actor ) == null )
				return;

			actor.CloseGump( typeof( HousingSuiteSplitGump ) );
			actor.SendGump( new HousingSuiteSplitGump( actor, confirming, page, message ) );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile actor;
			HousingSuiteSession session;

			if ( info == null || !TryResolveResponse( sender, out actor, out session ) )
				return;

			if ( info.ButtonID == 0 )
			{
				PauseAndClose( session );
				return;
			}

			if ( info.ButtonID == DashboardButton )
			{
				HousingSuiteGump.Show( actor, 0, String.Empty );
				return;
			}

			HousingEstateController estate;
			string error;

			if ( !HousingSuiteGumpOperations.TryResolveEstate( RenderedEstateSerial, RenderedEstateRevision, out estate, out error ) )
				return;

			if ( info.ButtonID == PreviousPageButton && m_Page > 0 )
			{
				Show( actor, m_Confirming, m_Page - 1, String.Empty );
				return;
			}

			if ( info.ButtonID == NextPageButton && ( m_Page + 1 ) * RowsPerPage < estate.MemberCount )
			{
				Show( actor, m_Confirming, m_Page + 1, String.Empty );
				return;
			}

			if ( !m_Confirming && info.ButtonID == ForceDissolveButton )
			{
				if ( actor.AccessLevel < AccessLevel.Administrator || estate.Health != HousingEstateHealth.Degraded )
					return;

				session.Force = true;
				HousingSuiteForceDissolveGump.Show( actor, String.Empty );
				return;
			}

			if ( !m_Confirming && info.ButtonID == ReviewButton )
			{
				HousingValidationResult validation = HousingEstateValidation.ValidateSplit( estate );

				if ( validation.IsValid )
					Show( actor, true, String.Empty );
				else
					Show( actor, false, validation.Message );

				return;
			}

			if ( m_Confirming && info.ButtonID == ConfirmButton )
			{
				HousingValidationResult validation = HousingEstateValidation.ValidateSplit( estate );

				if ( !validation.IsValid )
				{
					Show( actor, true, validation.Message );
					return;
				}

				if ( !HousingEstateService.TrySplit( actor, estate.Serial, out error ) )
				{
					Show( actor, true, error );
					return;
				}

				HousingSuiteCommand.Complete( actor );
				actor.SendMessage( "Estate {0} was split into standalone houses.", estate.Serial );
			}
		}

		private void DrawNavigation()
		{
			AddSectionHeader( NavigationX + 15, NavigationY + 15, NavigationWidth - 30, "Split Workflow" );
			AddWrappedText( NavigationX + 15, NavigationY + 52, NavigationWidth - 30, 195, "Normal split is available only for a healthy estate whose every member fits its own local storage, lockdown, and vendor limits. Review and confirmation are separate responses." );
			AddAction( NavigationX + 15, NavigationY + 485, DashboardButton, "Dashboard" );
		}

		private void DrawContent( Mobile actor, string message )
		{
			HousingEstateController estate;
			string error;
			bool resolved = HousingSuiteGumpOperations.TryResolveEstate( RenderedEstateSerial, RenderedEstateRevision, out estate, out error );
			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, m_Confirming ? "Final Split Confirmation" : "Split Review" );

			if ( !resolved )
			{
				AddWrappedText( ContentX + 15, ContentY + 52, ContentWidth - 30, 70, error );
				return;
			}

			AddHtml( ContentX + 15, ContentY + 48, ContentWidth - 30, 28, Blue( String.Format( "Estate {0} | health {1} | revision {2} | members {3}", estate.Serial, estate.Health, estate.CurrentRevision, estate.MemberCount ) ), false, false );

			if ( estate.Health == HousingEstateHealth.Degraded )
				AddWrappedText( ContentX + 15, ContentY + 80, ContentWidth - 30, 58, "Normal split and restore are disabled because this estate is degraded: " + estate.DegradedReason );
			else if ( m_Confirming )
				AddWrappedText( ContentX + 15, ContentY + 80, ContentWidth - 30, 58, "This separate confirmation restores each member's original decay setting, unregisters all estate mappings, and deletes the controller. The member houses and their contents remain." );

			if ( !String.IsNullOrEmpty( message ) )
				AddHtml( ContentX + 15, ContentY + 142, ContentWidth - 30, 30, Orange( message ), false, false );

			int start = m_Page * RowsPerPage;
			int rows = Math.Min( RowsPerPage, Math.Max( 0, estate.MemberCount - start ) );

			for ( int i = 0; i < rows; ++i )
			{
				int index = start + i;
				HouseFoundation member = estate.GetMember( index );
				bool over;
				string capacity = HousingSuiteGumpOperations.CapacityLine( member, out over );
				int y = ContentY + 180 + ( i * 49 );
				AddPanel( ContentX + 15, y, ContentWidth - 30, 42 );
				AddHtml( ContentX + 25, y + 4, ContentWidth - 50, 18, White( member == null ? "Missing member " + estate.GetMemberSerial( index ) : member.GetType().Name + " " + member.Serial + " | owner " + DescribeMobile( member.Owner ) + " | " + DescribeMap( member.Map ) + " " + member.Location ), false, false );
				AddHtml( ContentX + 25, y + 22, ContentWidth - 50, 18, over ? Orange( capacity ) : Blue( capacity ), false, false );
			}

			DrawPager( estate.MemberCount );

			HousingValidationResult split = HousingEstateValidation.ValidateSplit( estate );

			if ( estate.Health == HousingEstateHealth.Healthy && split.IsValid )
				AddAction( ContentX + 390, ContentY + 497, m_Confirming ? ConfirmButton : ReviewButton, m_Confirming ? "Confirm Split" : "Continue" );
			else if ( estate.Health == HousingEstateHealth.Degraded && actor != null && actor.AccessLevel >= AccessLevel.Administrator && !m_Confirming )
				AddAction( ContentX + 360, ContentY + 497, ForceDissolveButton, "Force Warning" );
		}

		private void DrawPager( int count )
		{
			int totalPages = Math.Max( 1, ( count + RowsPerPage - 1 ) / RowsPerPage );
			int y = ContentY + 464;
			AddHtml( ContentX + 260, y + 1, 120, 22, WhiteCentered( String.Format( "Page {0} of {1}", m_Page + 1, totalPages ) ), false, false );

			if ( m_Page > 0 )
				AddAction( ContentX + 25, y, PreviousPageButton, "Previous" );

			if ( m_Page + 1 < totalPages )
				AddAction( ContentX + ContentWidth - 190, y, NextPageButton, "Next" );
		}
	}

	public sealed class HousingSuiteForceDissolveGump : HousingSuiteGumpBase
	{
		private const int RowsPerPage = 6;

		public const int ConfirmForceButton = 2401;
		public const int BackButton = 1;
		public const int PreviousPageButton = 20;
		public const int NextPageButton = 21;

		private readonly int m_Page;

		public int Page { get { return m_Page; } }

		public HousingSuiteForceDissolveGump( Mobile actor, string message )
			: this( actor, 0, message )
		{
		}

		public HousingSuiteForceDissolveGump( Mobile actor, int page, string message )
			: base( actor, HousingSuiteView.Dashboard, "HousingSuite Administrator Force Dissolve" )
		{
			HousingEstateController estate;
			string error;
			int count = HousingSuiteGumpOperations.TryResolveEstate( RenderedEstateSerial, RenderedEstateRevision, out estate, out error ) ? estate.MemberCount : 0;
			m_Page = HousingSuiteGumpOperations.NormalizePage( page, count, RowsPerPage );
			AddSectionHeader( NavigationX + 15, NavigationY + 15, NavigationWidth - 30, "Administrator Only" );
			AddWrappedText( NavigationX + 15, NavigationY + 52, NavigationWidth - 30, 220, "This is the required second response. Missing members cannot be recreated. Surviving mappings are removed and decay restoration is attempted from the immutable original snapshot." );
			AddAction( NavigationX + 15, NavigationY + 485, BackButton, "Back" );
			DrawContent( message );
		}

		public static void Show( Mobile actor, string message )
		{
			Show( actor, 0, message );
		}

		public static void Show( Mobile actor, int page, string message )
		{
			if ( actor == null || actor.Deleted || actor.AccessLevel < AccessLevel.Administrator || HousingSuiteSession.Find( actor ) == null )
				return;

			actor.CloseGump( typeof( HousingSuiteForceDissolveGump ) );
			actor.SendGump( new HousingSuiteForceDissolveGump( actor, page, message ) );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile actor;
			HousingSuiteSession session;

			if ( info == null || !TryResolveResponse( sender, out actor, out session ) || actor.AccessLevel < AccessLevel.Administrator || !session.Force )
				return;

			if ( info.ButtonID == 0 )
			{
				CloseForceWarning( session );
				return;
			}

			if ( info.ButtonID == BackButton )
			{
				session.Force = false;
				HousingSuiteSplitGump.Show( actor, false, String.Empty );
				return;
			}

			HousingEstateController estate;
			string error;

			if ( !HousingSuiteGumpOperations.TryResolveEstate( RenderedEstateSerial, RenderedEstateRevision, out estate, out error ) || estate.Health != HousingEstateHealth.Degraded )
				return;

			if ( info.ButtonID == PreviousPageButton && m_Page > 0 )
			{
				Show( actor, m_Page - 1, String.Empty );
				return;
			}

			if ( info.ButtonID == NextPageButton && ( m_Page + 1 ) * RowsPerPage < estate.MemberCount )
			{
				Show( actor, m_Page + 1, String.Empty );
				return;
			}

			if ( info.ButtonID != ConfirmForceButton )
				return;

			if ( !HousingEstateService.TryForceDissolve( actor, estate.Serial, out error ) )
			{
				Show( actor, error );
				return;
			}

			HousingSuiteCommand.Complete( actor );
			actor.SendMessage( "Degraded estate {0} was force dissolved.", estate.Serial );
		}

		private void DrawContent( string message )
		{
			HousingEstateController estate;
			string error;
			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, "Second and Final Force Warning" );

			if ( !HousingSuiteGumpOperations.TryResolveEstate( RenderedEstateSerial, RenderedEstateRevision, out estate, out error ) )
			{
				AddWrappedText( ContentX + 15, ContentY + 52, ContentWidth - 30, 70, error );
				return;
			}

			AddWrappedText( ContentX + 15, ContentY + 48, ContentWidth - 30, 78, "Force dissolve permanently removes this degraded controller. It does not recreate missing houses, owners, items, mobiles, or designs. Review every missing member below before confirming." );

			if ( !String.IsNullOrEmpty( message ) )
				AddHtml( ContentX + 15, ContentY + 130, ContentWidth - 30, 28, Orange( message ), false, false );

			int start = m_Page * RowsPerPage;
			int count = Math.Min( RowsPerPage, Math.Max( 0, estate.MemberCount - start ) );

			for ( int i = 0; i < count; ++i )
			{
				Serial serial = estate.GetMemberSerial( start + i );
				HouseFoundation member = World.FindItem( serial ) as HouseFoundation;
				AddHtml( ContentX + 25, ContentY + 175 + ( i * 29 ), ContentWidth - 50, 24, member == null || member.Deleted ? Orange( "MISSING member " + serial ) : White( "Surviving member " + serial + " " + member.Location ), false, false );
			}

			DrawPager( estate.MemberCount );

			if ( estate.Health == HousingEstateHealth.Degraded )
				AddAction( ContentX + 360, ContentY + 497, ConfirmForceButton, "Force Dissolve" );
		}

		private void DrawPager( int count )
		{
			int totalPages = Math.Max( 1, ( count + RowsPerPage - 1 ) / RowsPerPage );
			int y = ContentY + 430;
			AddHtml( ContentX + 260, y + 1, 120, 22, WhiteCentered( String.Format( "Page {0} of {1}", m_Page + 1, totalPages ) ), false, false );

			if ( m_Page > 0 )
				AddAction( ContentX + 25, y, PreviousPageButton, "Previous" );

			if ( m_Page + 1 < totalPages )
				AddAction( ContentX + ContentWidth - 190, y, NextPageButton, "Next" );
		}
	}

	public sealed class HousingSuiteMoveGump : HousingSuiteGumpBase
	{
		private const int RowsPerPage = 6;
		private static readonly int[] ElevationDeltas = new int[] { -10, -5, -1, 1, 5, 10 };

		public const int DashboardButton = 1;
		public const int PreviousPageButton = 20;
		public const int NextPageButton = 21;
		public const int TargetHouseButton = 2200;
		public const int TargetEstateButton = 2201;
		public const int ElevateHouseButton = 2202;
		public const int ElevateEstateButton = 2203;
		public const int ElevationButtonBase = 2210;
		public const int ElevationPlusOneButton = 2213;
		public const int ReviewContinueButton = 2220;
		public const int ConfirmMoveButton = 2221;
		public const int PreviewAgainButton = 2222;
		public const int CustomElevationButton = 2230;
		public const int CustomElevationEntry = 2231;
		public const int ForceMoveButton = 2400;

		private readonly int m_Stage;
		private readonly bool m_MoveEstate;
		private readonly int m_ElevationDelta;
		private readonly int m_Page;
		private readonly HousingSuiteHouseFingerprint[] m_Fingerprints;
		private readonly bool m_PredictedConnectivityValid;
		private readonly string m_PredictedConnectivityMessage;

		public int Stage { get { return m_Stage; } }
		public int Page { get { return m_Page; } }
		public bool PredictedConnectivityValid { get { return m_PredictedConnectivityValid; } }

		public HousingSuiteMoveGump( Mobile actor, int stage, bool moveEstate, int elevationDelta, string message )
			: this( actor, stage, moveEstate, elevationDelta, 0, message )
		{
		}

		public HousingSuiteMoveGump( Mobile actor, int stage, bool moveEstate, int elevationDelta, int page, string message )
			: base( actor, HousingSuiteView.MoveReview, "HousingSuite Move / Elevate" )
		{
			m_Stage = stage < 1 || stage > 4 ? 1 : stage;
			m_MoveEstate = moveEstate;
			m_ElevationDelta = elevationDelta;
			m_Fingerprints = CaptureAffected();
			m_Page = HousingSuiteGumpOperations.NormalizePage( page, m_Fingerprints.Length, RowsPerPage );
			HousingValidationResult predictedConnectivity = EvaluatePredictedConnectivity( actor );
			m_PredictedConnectivityValid = predictedConnectivity.IsValid;
			m_PredictedConnectivityMessage = predictedConnectivity.Message;
			DrawNavigation();

			if ( m_Stage == 1 )
				DrawChoice( message );
			else if ( m_Stage == 2 )
				DrawElevationChoice( message );
			else
				DrawMoveReview( actor, message, m_Stage == 4 );
		}

		public static void Show( Mobile actor, int stage, bool moveEstate, int elevationDelta, string message )
		{
			Show( actor, stage, moveEstate, elevationDelta, 0, message );
		}

		public static void Show( Mobile actor, int stage, bool moveEstate, int elevationDelta, int page, string message )
		{
			if ( actor == null || actor.Deleted || HousingSuiteSession.Find( actor ) == null )
				return;

			actor.CloseGump( typeof( HousingSuiteMoveGump ) );
			actor.SendGump( new HousingSuiteMoveGump( actor, stage, moveEstate, elevationDelta, page, message ) );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile actor;
			HousingSuiteSession session;

			if ( info == null || !TryResolveResponse( sender, out actor, out session ) )
				return;

			if ( info.ButtonID == 0 )
			{
				PauseAndClose( session );
				return;
			}

			if ( info.ButtonID == DashboardButton )
			{
				HousingSuiteGump.Show( actor, 0, String.Empty );
				return;
			}

			if ( m_Stage >= 3 && info.ButtonID == PreviousPageButton && m_Page > 0 )
			{
				Show( actor, m_Stage, m_MoveEstate, m_ElevationDelta, m_Page - 1, String.Empty );
				return;
			}

			if ( m_Stage >= 3 && info.ButtonID == NextPageButton && ( m_Page + 1 ) * RowsPerPage < m_Fingerprints.Length )
			{
				Show( actor, m_Stage, m_MoveEstate, m_ElevationDelta, m_Page + 1, String.Empty );
				return;
			}

			if ( m_Stage == 1 )
			{
				if ( info.ButtonID == TargetHouseButton )
				{
					HousingSuiteCommand.BeginMoveHouseSelection( actor );
					return;
				}

				if ( info.ButtonID == TargetEstateButton )
				{
					HousingSuiteCommand.BeginDestinationSelection( actor, true );
					return;
				}

				if ( info.ButtonID == ElevateHouseButton )
				{
					HousingSuiteCommand.BeginElevateHouseSelection( actor );
					return;
				}

				if ( info.ButtonID == ElevateEstateButton )
				{
					session.ClearPendingDestination();
					Show( actor, 2, true, 0, String.Empty );
				}

				return;
			}

			if ( m_Stage == 2 )
			{
				int delta;

				if ( info.ButtonID == CustomElevationButton )
				{
					TextRelay entry = info.GetTextEntry( CustomElevationEntry );

					if ( entry == null || !Int32.TryParse( entry.Text, out delta ) || !HousingEstateValidation.IsAllowedElevationDelta( delta ) )
					{
						session.ClearPendingDestination();
						Show( actor, 2, m_MoveEstate, 0, "Enter a nonzero elevation from -100 to +100." );
						return;
					}
				}
				else
				{
					if ( info.ButtonID < ElevationButtonBase || info.ButtonID >= ElevationButtonBase + ElevationDeltas.Length )
						return;

					delta = ElevationDeltas[info.ButtonID - ElevationButtonBase];
				}

				if ( !TryPrepareElevation( actor, session, delta ) )
					return;
				return;
			}

			if ( ( m_Stage == 3 || m_Stage == 4 ) && info.ButtonID == PreviewAgainButton && !m_MoveEstate && m_ElevationDelta == 0 )
			{
				HousingSuiteCommand.BeginDestinationPreviewAgain( actor, m_Stage );
				return;
			}

			if ( m_Stage == 3 && info.ButtonID == ForceMoveButton )
			{
				List<BaseHouse> affected;
				Map map;
				Point3D destination;
				string error;

				if ( actor.AccessLevel < AccessLevel.GameMaster || !TryResolveMove( actor, session, out affected, out map, out destination, out error ) )
					return;

				HousingValidationResult connectivity = EvaluatePredictedConnectivity( actor );

				if ( !connectivity.IsValid )
				{
					Show( actor, 3, m_MoveEstate, m_ElevationDelta, m_Page, connectivity.Message );
					return;
				}

				HousingValidationResult normal = HousingEstateValidation.ValidateMove( actor, affected, map, session.PendingDeltaX, session.PendingDeltaY, session.PendingDeltaZ, false );
				HousingValidationResult forced = HousingEstateValidation.ValidateMove( actor, affected, map, session.PendingDeltaX, session.PendingDeltaY, session.PendingDeltaZ, true );

				if ( normal.IsValid || !forced.IsValid )
					return;

				session.Force = true;
				HousingSuiteMoveForceGump.Show( actor, m_MoveEstate, m_ElevationDelta, m_Page, normal.Message );
				return;
			}

			if ( m_Stage == 3 && info.ButtonID == ReviewContinueButton )
			{
				List<BaseHouse> affected;
				Map map;
				Point3D destination;
				string error;

				if ( TryResolveMove( actor, session, out affected, out map, out destination, out error ) )
				{
					HousingValidationResult validation = HousingEstateValidation.ValidateMove( actor, affected, map, session.PendingDeltaX, session.PendingDeltaY, session.PendingDeltaZ, false );
					HousingValidationResult connectivity = EvaluatePredictedConnectivity( actor );

					if ( validation.IsValid && connectivity.IsValid )
						Show( actor, 4, m_MoveEstate, m_ElevationDelta, m_Page, String.Empty );
					else
						Show( actor, 3, m_MoveEstate, m_ElevationDelta, m_Page, validation.IsValid ? connectivity.Message : validation.Message );
				}

				return;
			}

			if ( m_Stage == 4 && info.ButtonID == ConfirmMoveButton )
				ConfirmMove( actor, session, false );
		}

		internal void ConfirmMove( Mobile actor, HousingSuiteSession session, bool force )
		{
			List<BaseHouse> affected;
			Map map;
			Point3D destination;
			string error;

			if ( !TryResolveMove( actor, session, out affected, out map, out destination, out error ) )
				return;

			HousingValidationResult validation = HousingEstateValidation.ValidateMove( actor, affected, map, session.PendingDeltaX, session.PendingDeltaY, session.PendingDeltaZ, force );
			HousingValidationResult connectivity = EvaluatePredictedConnectivity( actor );

			if ( !validation.IsValid || !connectivity.IsValid || !HousingSuiteHouseFingerprint.MatchesAll( affected, m_Fingerprints ) )
			{
				string reason = !validation.IsValid ? validation.Message : ( !connectivity.IsValid ? connectivity.Message : "An affected house changed after review." );
				Show( actor, m_Stage, m_MoveEstate, m_ElevationDelta, m_Page, reason );
				return;
			}

			Map sourceMap = affected[0].Map;
			Point3D sourceAnchor = affected[0].Location;
			Serial estateSerial = RenderedEstateSerial;
			int deltaX = session.PendingDeltaX;
			int deltaY = session.PendingDeltaY;
			int deltaZ = session.PendingDeltaZ;
			int sourceEstateRevision = estateSerial == Server.Serial.MinusOne ? 0 : RenderedEstateRevision;
			HousingSuiteMovementUndoOperation operation = m_ElevationDelta != 0
				? ( m_MoveEstate ? HousingSuiteMovementUndoOperation.ElevateEstate : HousingSuiteMovementUndoOperation.ElevateHouse )
				: ( m_MoveEstate ? HousingSuiteMovementUndoOperation.MoveEstate : HousingSuiteMovementUndoOperation.MoveHouse );
			bool success;

			if ( m_MoveEstate )
				success = HousingEstateService.TryMoveEstate( actor, RenderedEstateSerial, map, destination, force, out error );
			else
				success = HousingEstateService.TryMoveHouse( actor, affected[0].Serial, map, destination, force, out error );

			if ( !success )
			{
				Show( actor, m_Stage, m_MoveEstate, m_ElevationDelta, m_Page, error );
				return;
			}

			session.ClearPendingMovementUndo();
			HousingSuiteMovementUndoRecord record;

			if ( !HousingSuiteMovementUndo.TryCreateAfterMovement( session, operation, m_MoveEstate, affected, estateSerial, sourceEstateRevision, sourceMap, sourceAnchor, map, destination, deltaX, deltaY, deltaZ, force, out record, out error ) || !session.SetPendingMovementUndo( record ) )
			{
				session.ClearPendingDestination();
				session.Force = false;
				session.ConfirmationToken = 0;
				session.View = HousingSuiteView.Dashboard;
				HousingSuiteGump.Show( actor, 0, String.IsNullOrEmpty( error ) ? "Movement completed, but its undo record could not be created." : error );
				return;
			}

			if ( estateSerial != Server.Serial.MinusOne )
				session.SetSelectedEstate( estateSerial, record.EstateRevision );

			session.ClearPendingDestination();
			session.Force = false;
			session.ConfirmationToken = 0;
			session.View = HousingSuiteView.Dashboard;
			actor.SendMessage( "HousingSuite movement completed{0}.", force ? " with Placement Override" : String.Empty );
			HousingSuiteMovementUndoGump.Show( actor, record );
		}

		private bool TryResolveMove( Mobile actor, HousingSuiteSession session, out List<BaseHouse> affected, out Map map, out Point3D destination, out string error )
		{
			affected = null;
			map = null;
			destination = Point3D.Zero;
			error = String.Empty;

			if ( session == null || !session.HasPendingDestination )
			{
				error = "No reviewed destination is pending.";
				return false;
			}

			map = HousingSuiteCommand.ResolveWorldMap( session.PendingDestinationMapIndex );
			destination = session.PendingDestination;

			if ( map == null )
			{
				error = "The pending destination map is invalid.";
				return false;
			}

			if ( m_MoveEstate )
			{
				HousingEstateController estate;

				if ( !HousingSuiteGumpOperations.TryResolveHealthyEstate( RenderedEstateSerial, RenderedEstateRevision, out estate, out error ) )
					return false;

				affected = HousingSuiteGumpOperations.CopyBaseHouses( estate.CopyMembers() );
			}
			else
			{
				BaseHouse anchor;

				if ( !HousingSuiteGumpOperations.TryResolveHouse( RenderedSelectedSerials, out anchor, out error ) )
					return false;

				affected = new List<BaseHouse>( 1 );
				affected.Add( anchor );
			}

			return true;
		}

		private HousingSuiteHouseFingerprint[] CaptureAffected()
		{
			string error;
			BaseHouse anchor;

			if ( m_MoveEstate )
			{
				HousingEstateController estate;

				if ( HousingSuiteGumpOperations.TryResolveHealthyEstate( RenderedEstateSerial, RenderedEstateRevision, out estate, out error ) )
					return HousingSuiteHouseFingerprint.CaptureAll( estate.CopyMembers() );
			}
			else if ( HousingSuiteGumpOperations.TryResolveHouse( RenderedSelectedSerials, out anchor, out error ) )
			{
				List<BaseHouse> one = new List<BaseHouse>( 1 );
				one.Add( anchor );
				return HousingSuiteHouseFingerprint.CaptureAll( one );
			}

			return new HousingSuiteHouseFingerprint[0];
		}

		private HousingValidationResult EvaluatePredictedConnectivity( Mobile actor )
		{
			if ( m_Stage < 3 || m_MoveEstate )
				return HousingValidationResult.Valid;

			HousingSuiteSession session = HousingSuiteSession.Find( actor );

			if ( session == null || !session.HasPendingDestination )
				return new HousingValidationResult( false, "No reviewed destination is pending for connectivity preview." );

			BaseHouse selectedHouse;
			string error;

			if ( !HousingSuiteGumpOperations.TryResolveHouse( RenderedSelectedSerials, out selectedHouse, out error ) )
				return new HousingValidationResult( false, error );

			HouseFoundation anchor = selectedHouse as HouseFoundation;

			if ( anchor == null )
				return RenderedEstateSerial == Server.Serial.MinusOne ? HousingValidationResult.Valid : new HousingValidationResult( false, "The selected classic house cannot be an estate member." );

			Map map = HousingSuiteCommand.ResolveWorldMap( session.PendingDestinationMapIndex );
			return HousingSuiteGumpOperations.ValidatePredictedEstateConnectivity( anchor, RenderedEstateSerial, RenderedEstateRevision, map, session.PendingDeltaX, session.PendingDeltaY );
		}

		private void DrawNavigation()
		{
			AddSectionHeader( NavigationX + 15, NavigationY + 15, NavigationWidth - 30, "Movement Workflow" );
			AddWrappedText( NavigationX + 15, NavigationY + 52, NavigationWidth - 30, 220, "Choose one selected house or the whole selected estate. Targeting stores a destination for review and never moves anything. Elevation accepts any nonzero signed value from -100 through +100." );
			AddAction( NavigationX + 15, NavigationY + 485, DashboardButton, "Dashboard" );
		}

		private void DrawChoice( string message )
		{
			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, "Choose Scope and Operation" );
			AddWrappedText( ContentX + 15, ContentY + 50, ContentWidth - 30, 70, "Destination movement and elevation both use the existing transactional move service. Whole-estate movement preserves member offsets; one-house movement must still leave a healthy estate connected." );
			AddAction( ContentX + 55, ContentY + 155, TargetHouseButton, "Move One House" );
			AddAction( ContentX + 330, ContentY + 155, TargetEstateButton, "Move Whole Estate" );
			AddAction( ContentX + 55, ContentY + 230, ElevateHouseButton, "Elevate One House" );
			AddAction( ContentX + 330, ContentY + 230, ElevateEstateButton, "Elevate Whole Estate" );

			if ( !String.IsNullOrEmpty( message ) )
				AddHtml( ContentX + 25, ContentY + 320, ContentWidth - 50, 60, Orange( message ), false, false );
		}

		private void DrawElevationChoice( string message )
		{
			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, m_MoveEstate ? "Choose Whole-Estate Elevation" : "Choose House Elevation" );
			AddWrappedText( ContentX + 15, ContentY + 50, ContentWidth - 30, 65, "Choose a preset or enter a signed elevation from -100 to +100. The server records a zero XY delta and validates destination Z, collisions, region rules, and estate connectivity before confirmation." );

			AddHtml( ContentX + 35, ContentY + 128, 155, 22, WhiteCentered( "Lower" ), false, false );
			AddHtml( ContentX + 220, ContentY + 128, 155, 22, WhiteCentered( "Custom signed Z" ), false, false );
			AddHtml( ContentX + 405, ContentY + 128, 155, 22, WhiteCentered( "Raise" ), false, false );

			for ( int i = 0; i < 3; ++i )
			{
				int y = ContentY + 165 + ( i * 55 );
				AddAction( ContentX + 35, y, ElevationButtonBase + i, ElevationDeltas[i] + " Z" );
				AddAction( ContentX + 405, y, ElevationButtonBase + i + 3, "+" + ElevationDeltas[i + 3] + " Z" );
			}

			AddEntryTray( ContentX + 245, ContentY + 176, 105, 22, CustomElevationEntry, String.Empty );
			AddAction( ContentX + 220, ContentY + 220, CustomElevationButton, "Apply Custom" );

			if ( !String.IsNullOrEmpty( message ) )
				AddHtml( ContentX + 25, ContentY + 330, ContentWidth - 50, 50, Orange( message ), false, false );
		}

		private bool TryPrepareElevation( Mobile actor, HousingSuiteSession session, int delta )
		{
			if ( !HousingEstateValidation.IsAllowedElevationDelta( delta ) )
				return false;

			BaseHouse anchor;
			string error;

			if ( !HousingSuiteGumpOperations.TryResolveHouse( RenderedSelectedSerials, out anchor, out error ) )
				return false;

			int destinationZ;

			try
			{
				destinationZ = checked( anchor.Z + delta );
			}
			catch ( OverflowException )
			{
				Show( actor, 2, m_MoveEstate, 0, "The requested elevation is outside the supported coordinate range." );
				return false;
			}

			Point3D destination = new Point3D( anchor.X, anchor.Y, destinationZ );
			session.SetPendingDestination( destination, anchor.Map.MapIndex, 0, 0, delta );
			Show( actor, 3, m_MoveEstate, delta, String.Empty );
			return true;
		}

		private void DrawMoveReview( Mobile actor, string message, bool confirming )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );
			List<BaseHouse> affected;
			Map map;
			Point3D destination;
			string error;
			bool resolved = TryResolveMove( actor, session, out affected, out map, out destination, out error );
			HousingValidationResult normal = resolved ? HousingEstateValidation.ValidateMove( actor, affected, map, session.PendingDeltaX, session.PendingDeltaY, session.PendingDeltaZ, false ) : new HousingValidationResult( false, error );
			HousingValidationResult forced = resolved && actor.AccessLevel >= AccessLevel.GameMaster ? HousingEstateValidation.ValidateMove( actor, affected, map, session.PendingDeltaX, session.PendingDeltaY, session.PendingDeltaZ, true ) : new HousingValidationResult( false, String.Empty );

			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, confirming ? "Final Movement Confirmation" : "Movement Review" );
			AddWrappedText( ContentX + 15, ContentY + 48, ContentWidth - 30, 62, confirming ? "This is the separate normal confirmation response. Current session state, exact affected serials, estate revision, source fingerprint, destination, and validation are checked again before the service is called." : "Review source, destination, delta, affected serials, collision and region validation, connectivity impact, and Placement Override availability. No movement has occurred." );

			if ( resolved )
			{
				BaseHouse anchor = affected[0];
				AddHtml( ContentX + 15, ContentY + 115, ContentWidth - 30, 24, Blue( String.Format( "Source {0} {1} | Destination {2} {3}", DescribeMap( anchor.Map ), anchor.Location, DescribeMap( map ), destination ) ), false, false );
				AddHtml( ContentX + 15, ContentY + 143, ContentWidth - 30, 24, Blue( String.Format( "Delta {0}, {1}, {2} | affected houses {3}", session.PendingDeltaX, session.PendingDeltaY, session.PendingDeltaZ, affected.Count ) ), false, false );
			}

			AddHtml( ContentX + 15, ContentY + 174, ContentWidth - 30, 34, normal.IsValid ? Blue( "Normal placement validation: valid." ) : Orange( "Normal placement validation: " + normal.Message ), false, false );
			AddHtml( ContentX + 15, ContentY + 210, ContentWidth - 30, 36, m_PredictedConnectivityValid ? Blue( "Predicted post-move estate connectivity: VALID." ) : Orange( "Predicted post-move estate connectivity: BLOCKED - " + m_PredictedConnectivityMessage ), false, false );

			if ( !String.IsNullOrEmpty( message ) )
				AddHtml( ContentX + 15, ContentY + 248, ContentWidth - 30, 30, Orange( message ), false, false );

			if ( affected != null )
			{
				int start = m_Page * RowsPerPage;
				int count = Math.Min( RowsPerPage, Math.Max( 0, affected.Count - start ) );

				for ( int i = 0; i < count; ++i )
				{
					BaseHouse house = affected[start + i];
					AddHtml( ContentX + 25, ContentY + 285 + ( i * 30 ), ContentWidth - 50, 22, White( house.GetType().Name + " " + house.Serial + " | " + DescribeMap( house.Map ) + " " + house.Location ), false, false );
				}
			}

			DrawPager( affected == null ? 0 : affected.Count );

			if ( !m_MoveEstate && m_ElevationDelta == 0 )
				AddAction( ContentX + 65, ContentY + 497, PreviewAgainButton, "Preview Again" );

			if ( normal.IsValid && m_PredictedConnectivityValid )
				AddAction( ContentX + 390, ContentY + 497, confirming ? ConfirmMoveButton : ReviewContinueButton, confirming ? "Confirm Movement" : "Continue" );
			else if ( !confirming && !normal.IsValid && m_PredictedConnectivityValid && actor.AccessLevel >= AccessLevel.GameMaster && forced.IsValid )
				AddAction( ContentX + 375, ContentY + 497, ForceMoveButton, "Override Placement" );
		}

		private void DrawPager( int count )
		{
			int totalPages = Math.Max( 1, ( count + RowsPerPage - 1 ) / RowsPerPage );
			int y = ContentY + 464;
			AddHtml( ContentX + 260, y + 1, 120, 22, WhiteCentered( String.Format( "Page {0} of {1}", m_Page + 1, totalPages ) ), false, false );

			if ( m_Page > 0 )
				AddAction( ContentX + 25, y, PreviousPageButton, "Previous" );

			if ( m_Page + 1 < totalPages )
				AddAction( ContentX + ContentWidth - 190, y, NextPageButton, "Next" );
		}
	}

	public sealed class HousingSuiteMoveForceGump : HousingSuiteGumpBase
	{
		private const int RowsPerPage = 6;

		public const int ConfirmForceButton = 2401;
		public const int BackButton = 1;
		public const int PreviousPageButton = 20;
		public const int NextPageButton = 21;

		private readonly bool m_MoveEstate;
		private readonly int m_ElevationDelta;
		private readonly int m_Page;
		private readonly HousingSuiteHouseFingerprint[] m_Fingerprints;

		public HousingSuiteMoveForceGump( Mobile actor, bool moveEstate, int elevationDelta, string reason )
			: this( actor, moveEstate, elevationDelta, 0, reason )
		{
		}

		public HousingSuiteMoveForceGump( Mobile actor, bool moveEstate, int elevationDelta, int page, string reason )
			: base( actor, HousingSuiteView.MoveReview, "HousingSuite Placement Override" )
		{
			m_MoveEstate = moveEstate;
			m_ElevationDelta = elevationDelta;
			m_Fingerprints = CaptureAffected();
			m_Page = HousingSuiteGumpOperations.NormalizePage( page, m_Fingerprints.Length, RowsPerPage );
			AddSectionHeader( NavigationX + 15, NavigationY + 15, NavigationWidth - 30, "Game Master and Higher" );
			AddWrappedText( NavigationX + 15, NavigationY + 52, NavigationWidth - 30, 230, "This second response may bypass placement results for terrain, statics, items, mobiles, multis, occupancy, and housing-region failures. Map validity, bounds, Z, dimensions, customization, stale state, estate connectivity, transaction verification, and rollback remain enforced." );
			AddAction( NavigationX + 15, NavigationY + 485, BackButton, "Back" );
			DrawContent( actor, reason );
		}

		public static void Show( Mobile actor, bool moveEstate, int elevationDelta, string reason )
		{
			Show( actor, moveEstate, elevationDelta, 0, reason );
		}

		public static void Show( Mobile actor, bool moveEstate, int elevationDelta, int page, string reason )
		{
			if ( actor == null || actor.Deleted || actor.AccessLevel < AccessLevel.GameMaster || HousingSuiteSession.Find( actor ) == null )
				return;

			actor.CloseGump( typeof( HousingSuiteMoveForceGump ) );
			actor.SendGump( new HousingSuiteMoveForceGump( actor, moveEstate, elevationDelta, page, reason ) );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile actor;
			HousingSuiteSession session;

			if ( info == null || !TryResolveResponse( sender, out actor, out session ) || actor.AccessLevel < AccessLevel.GameMaster || !session.Force )
				return;

			if ( info.ButtonID == 0 )
			{
				CloseForceWarning( session );
				return;
			}

			if ( info.ButtonID == BackButton )
			{
				session.Force = false;
				HousingSuiteMoveGump.Show( actor, 3, m_MoveEstate, m_ElevationDelta, m_Page, String.Empty );
				return;
			}

			if ( info.ButtonID == PreviousPageButton && m_Page > 0 )
			{
				Show( actor, m_MoveEstate, m_ElevationDelta, m_Page - 1, String.Empty );
				return;
			}

			if ( info.ButtonID == NextPageButton && ( m_Page + 1 ) * RowsPerPage < m_Fingerprints.Length )
			{
				Show( actor, m_MoveEstate, m_ElevationDelta, m_Page + 1, String.Empty );
				return;
			}

			if ( info.ButtonID != ConfirmForceButton )
				return;

			List<BaseHouse> affected = ResolveAffected();

			if ( affected == null || !HousingSuiteHouseFingerprint.MatchesAll( affected, m_Fingerprints ) )
				return;

			Map map = HousingSuiteCommand.ResolveWorldMap( session.PendingDestinationMapIndex );

			if ( !session.HasPendingDestination || map == null || affected.Count == 0 )
				return;

			HouseFoundation movedFoundation = affected[0] as HouseFoundation;
			HousingValidationResult connectivity = m_MoveEstate || movedFoundation == null
				? HousingValidationResult.Valid
				: HousingSuiteGumpOperations.ValidatePredictedEstateConnectivity( movedFoundation, RenderedEstateSerial, RenderedEstateRevision, map, session.PendingDeltaX, session.PendingDeltaY );

			if ( !connectivity.IsValid )
			{
				Show( actor, m_MoveEstate, m_ElevationDelta, m_Page, connectivity.Message );
				return;
			}

			HousingValidationResult validation = HousingEstateValidation.ValidateMove( actor, affected, map, session.PendingDeltaX, session.PendingDeltaY, session.PendingDeltaZ, true );

			if ( !validation.IsValid )
			{
				Show( actor, m_MoveEstate, m_ElevationDelta, m_Page, validation.Message );
				return;
			}

			Map sourceMap = affected[0].Map;
			Point3D sourceAnchor = affected[0].Location;
			Point3D destination = session.PendingDestination;
			int deltaX = session.PendingDeltaX;
			int deltaY = session.PendingDeltaY;
			int deltaZ = session.PendingDeltaZ;
			Serial estateSerial = RenderedEstateSerial;
			int sourceEstateRevision = estateSerial == Server.Serial.MinusOne ? 0 : RenderedEstateRevision;
			HousingSuiteMovementUndoOperation operation = m_ElevationDelta != 0
				? ( m_MoveEstate ? HousingSuiteMovementUndoOperation.ElevateEstate : HousingSuiteMovementUndoOperation.ElevateHouse )
				: ( m_MoveEstate ? HousingSuiteMovementUndoOperation.MoveEstate : HousingSuiteMovementUndoOperation.MoveHouse );
			string error;
			bool success;

			if ( m_MoveEstate )
				success = HousingEstateService.TryMoveEstate( actor, RenderedEstateSerial, map, session.PendingDestination, true, out error );
			else
				success = HousingEstateService.TryMoveHouse( actor, affected[0].Serial, map, session.PendingDestination, true, out error );

			if ( !success )
			{
				Show( actor, m_MoveEstate, m_ElevationDelta, m_Page, error );
				return;
			}

			session.ClearPendingMovementUndo();
			HousingSuiteMovementUndoRecord record;

			if ( !HousingSuiteMovementUndo.TryCreateAfterMovement( session, operation, m_MoveEstate, affected, estateSerial, sourceEstateRevision, sourceMap, sourceAnchor, map, destination, deltaX, deltaY, deltaZ, true, out record, out error ) || !session.SetPendingMovementUndo( record ) )
			{
				session.ClearPendingDestination();
				session.Force = false;
				session.ConfirmationToken = 0;
				session.View = HousingSuiteView.Dashboard;
				HousingSuiteGump.Show( actor, 0, String.IsNullOrEmpty( error ) ? "Movement completed, but its undo record could not be created." : error );
				return;
			}

			if ( estateSerial != Server.Serial.MinusOne )
				session.SetSelectedEstate( estateSerial, record.EstateRevision );

			session.ClearPendingDestination();
			session.Force = false;
			session.ConfirmationToken = 0;
			session.View = HousingSuiteView.Dashboard;
			actor.SendMessage( "HousingSuite movement completed with Placement Override." );
			HousingSuiteMovementUndoGump.Show( actor, record );
		}

		private void DrawContent( Mobile actor, string reason )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );
			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, "Second and Final Placement Override" );
			AddWrappedText( ContentX + 15, ContentY + 48, ContentWidth - 30, 75, "Normal validation failed: " + reason + " The same destination and affected serials will be revalidated with the Placement Override flag before the transactional movement service is called. Map validity, bounds, Z, dimensions, customization, stale state, estate connectivity, transaction verification, and rollback remain enforced." );

			if ( session != null && session.HasPendingDestination )
				AddHtml( ContentX + 15, ContentY + 128, ContentWidth - 30, 28, Blue( String.Format( "Destination map {0} {1} | delta {2}, {3}, {4}", session.PendingDestinationMapIndex, session.PendingDestination, session.PendingDeltaX, session.PendingDeltaY, session.PendingDeltaZ ) ), false, false );

			int start = m_Page * RowsPerPage;
			int count = Math.Min( RowsPerPage, Math.Max( 0, m_Fingerprints.Length - start ) );

			for ( int i = 0; i < count; ++i )
			{
				HousingSuiteHouseFingerprint value = m_Fingerprints[start + i];
				AddHtml( ContentX + 25, ContentY + 175 + ( i * 36 ), ContentWidth - 50, 28, White( String.Format( "Affected {0} | map {1} {2}", value.Serial, value.MapIndex, value.Location ) ), false, false );
			}

			DrawPager();
			AddAction( ContentX + 375, ContentY + 497, ConfirmForceButton, "Confirm Override Move" );
		}

		private void DrawPager()
		{
			int totalPages = Math.Max( 1, ( m_Fingerprints.Length + RowsPerPage - 1 ) / RowsPerPage );
			int y = ContentY + 430;
			AddHtml( ContentX + 260, y + 1, 120, 22, WhiteCentered( String.Format( "Page {0} of {1}", m_Page + 1, totalPages ) ), false, false );

			if ( m_Page > 0 )
				AddAction( ContentX + 25, y, PreviousPageButton, "Previous" );

			if ( m_Page + 1 < totalPages )
				AddAction( ContentX + ContentWidth - 190, y, NextPageButton, "Next" );
		}

		private List<BaseHouse> ResolveAffected()
		{
			string error;

			if ( m_MoveEstate )
			{
				HousingEstateController estate;

				if ( HousingSuiteGumpOperations.TryResolveHealthyEstate( RenderedEstateSerial, RenderedEstateRevision, out estate, out error ) )
					return HousingSuiteGumpOperations.CopyBaseHouses( estate.CopyMembers() );
			}
			else
			{
				BaseHouse anchor;

				if ( HousingSuiteGumpOperations.TryResolveHouse( RenderedSelectedSerials, out anchor, out error ) )
				{
					List<BaseHouse> one = new List<BaseHouse>( 1 );
					one.Add( anchor );
					return one;
				}
			}

			return null;
		}

		private HousingSuiteHouseFingerprint[] CaptureAffected()
		{
			List<BaseHouse> affected = ResolveAffected();
			return affected == null ? new HousingSuiteHouseFingerprint[0] : HousingSuiteHouseFingerprint.CaptureAll( affected );
		}
	}

	public sealed class HousingSuiteRestoreGump : HousingSuiteGumpBase
	{
		private const int RowsPerPage = 5;
		private const int MemberRowsPerPage = 6;

		public const int DashboardButton = 1;
		public const int PreviousPageButton = 30;
		public const int NextPageButton = 31;
		public const int MemberPreviousPageButton = 32;
		public const int MemberNextPageButton = 33;
		public const int RevisionButtonBase = 2300;
		public const int ReviewContinueButton = 2350;
		public const int ConfirmRestoreButton = 2351;

		private readonly int m_Stage;
		private readonly int m_Page;
		private readonly int m_SelectedRevision;
		private readonly int[] m_RenderedRevisions;

		public int Stage { get { return m_Stage; } }
		public int Page { get { return m_Page; } }
		public int SelectedRevision { get { return m_SelectedRevision; } }
		public int RenderedRevisionCount { get { return m_RenderedRevisions.Length; } }

		public HousingSuiteRestoreGump( Mobile actor, int stage, int page, int selectedRevision, string message )
			: base( actor, HousingSuiteView.Dashboard, "HousingSuite Restore History" )
		{
			m_Stage = stage < 1 || stage > 3 ? 1 : stage;
			m_SelectedRevision = selectedRevision;
			m_RenderedRevisions = new int[0];
			DrawNavigation();

			HousingEstateController estate;
			string error;

			if ( !HousingSuiteGumpOperations.TryResolveEstate( RenderedEstateSerial, RenderedEstateRevision, out estate, out error ) )
			{
				m_Page = 0;
				DrawError( error );
				return;
			}

			if ( m_Stage == 1 )
			{
				List<HousingEstateSnapshot> history = BuildHistory( estate );
				int totalPages = Math.Max( 1, ( history.Count + RowsPerPage - 1 ) / RowsPerPage );
				m_Page = Math.Max( 0, Math.Min( page, totalPages - 1 ) );
				int start = m_Page * RowsPerPage;
				int count = Math.Min( RowsPerPage, Math.Max( 0, history.Count - start ) );
				m_RenderedRevisions = new int[count];

				for ( int i = 0; i < count; ++i )
					m_RenderedRevisions[i] = history[start + i].Revision;

				DrawHistory( estate, history, start, totalPages, message );
			}
			else
			{
				HousingEstateSnapshot snapshot = estate.FindRevision( m_SelectedRevision );
				m_Page = HousingSuiteGumpOperations.NormalizePage( page, snapshot == null ? 0 : snapshot.HouseCount, MemberRowsPerPage );
				DrawReview( estate, message, m_Stage == 3 );
			}
		}

		public static void Show( Mobile actor, int stage, int page, int selectedRevision, string message )
		{
			if ( actor == null || actor.Deleted || HousingSuiteSession.Find( actor ) == null )
				return;

			actor.CloseGump( typeof( HousingSuiteRestoreGump ) );
			actor.SendGump( new HousingSuiteRestoreGump( actor, stage, page, selectedRevision, message ) );
		}

		internal int GetRenderedRevision( int index )
		{
			if ( index < 0 || index >= m_RenderedRevisions.Length )
				throw new ArgumentOutOfRangeException( "index" );

			return m_RenderedRevisions[index];
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile actor;
			HousingSuiteSession session;

			if ( info == null || !TryResolveResponse( sender, out actor, out session ) )
				return;

			if ( info.ButtonID == 0 )
			{
				PauseAndClose( session );
				return;
			}

			if ( info.ButtonID == DashboardButton )
			{
				HousingSuiteGump.Show( actor, 0, String.Empty );
				return;
			}

			HousingEstateController estate;
			string error;

			if ( !HousingSuiteGumpOperations.TryResolveEstate( RenderedEstateSerial, RenderedEstateRevision, out estate, out error ) )
				return;

			if ( m_Stage == 1 )
			{
				if ( info.ButtonID == PreviousPageButton && m_Page > 0 )
				{
					Show( actor, 1, m_Page - 1, -1, String.Empty );
					return;
				}

				if ( info.ButtonID == NextPageButton && ( m_Page + 1 ) * RowsPerPage < BuildHistory( estate ).Count )
				{
					Show( actor, 1, m_Page + 1, -1, String.Empty );
					return;
				}

				if ( info.ButtonID >= RevisionButtonBase && info.ButtonID < RevisionButtonBase + m_RenderedRevisions.Length )
				{
					int revision = m_RenderedRevisions[info.ButtonID - RevisionButtonBase];

					if ( estate.FindRevision( revision ) != null && estate.Health == HousingEstateHealth.Healthy )
						Show( actor, 2, 0, revision, String.Empty );
				}

				return;
			}

			HousingEstateSnapshot selectedSnapshot = estate.FindRevision( m_SelectedRevision );

			if ( info.ButtonID == MemberPreviousPageButton && m_Page > 0 )
			{
				Show( actor, m_Stage, m_Page - 1, m_SelectedRevision, String.Empty );
				return;
			}

			if ( info.ButtonID == MemberNextPageButton && selectedSnapshot != null && ( m_Page + 1 ) * MemberRowsPerPage < selectedSnapshot.HouseCount )
			{
				Show( actor, m_Stage, m_Page + 1, m_SelectedRevision, String.Empty );
				return;
			}

			HousingEstateSnapshot snapshot;
			HousingValidationResult validation = HousingSuiteGumpOperations.ValidateRestore( estate, m_SelectedRevision, out snapshot );

			if ( m_Stage == 2 && info.ButtonID == ReviewContinueButton )
			{
				if ( validation.IsValid )
					Show( actor, 3, m_Page, m_SelectedRevision, String.Empty );
				else
					Show( actor, 2, m_Page, m_SelectedRevision, validation.Message );

				return;
			}

			if ( m_Stage == 3 && info.ButtonID == ConfirmRestoreButton )
			{
				if ( !validation.IsValid )
				{
					Show( actor, 3, m_Page, m_SelectedRevision, validation.Message );
					return;
				}

				if ( !HousingEstateService.TryRestore( actor, estate.Serial, m_SelectedRevision, out error ) )
				{
					Show( actor, 3, m_Page, m_SelectedRevision, error );
					return;
				}

				HousingSuiteCommand.Complete( actor );
				actor.SendMessage( "Estate {0} restored from revision {1}.", estate.Serial, m_SelectedRevision );
			}
		}

		private void DrawNavigation()
		{
			AddSectionHeader( NavigationX + 15, NavigationY + 15, NavigationWidth - 30, "Restore Workflow" );
			AddWrappedText( NavigationX + 15, NavigationY + 52, NavigationWidth - 30, 220, "History contains the immutable original and retained recent revisions. Selection maps a rendered button to its server-held revision number. Review and confirmation revalidate live references and destinations." );
			AddAction( NavigationX + 15, NavigationY + 485, DashboardButton, "Dashboard" );
		}

		private void DrawHistory( HousingEstateController estate, IList<HousingEstateSnapshot> history, int start, int totalPages, string message )
		{
			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, "Immutable Original and Retained Revisions" );
			AddHtml( ContentX + 15, ContentY + 45, ContentWidth - 30, 26, Blue( String.Format( "Estate {0} | health {1} | current revision {2}", estate.Serial, estate.Health, estate.CurrentRevision ) ), false, false );

			if ( estate.Health != HousingEstateHealth.Healthy )
				AddHtml( ContentX + 15, ContentY + 75, ContentWidth - 30, 42, Orange( "Restore disabled: " + estate.DegradedReason ), false, false );
			else if ( !String.IsNullOrEmpty( message ) )
				AddHtml( ContentX + 15, ContentY + 75, ContentWidth - 30, 42, Orange( message ), false, false );

			for ( int i = 0; i < m_RenderedRevisions.Length; ++i )
			{
				HousingEstateSnapshot snapshot = history[start + i];
				HousingHouseSnapshot first = snapshot.HouseCount == 0 ? null : snapshot.GetHouse( 0 );
				int y = ContentY + 125 + ( i * 76 );
				AddPanel( ContentX + 15, y, ContentWidth - 30, 67 );
				string actor = String.IsNullOrEmpty( snapshot.ActorName ) ? snapshot.ActorSerial.ToString() : snapshot.ActorName + " " + snapshot.ActorSerial;
				AddHtml( ContentX + 25, y + 6, ContentWidth - 105, 20, White( String.Format( "Revision {0} | {1} | actor {2} | {3:u}", snapshot.Revision, snapshot.Operation, actor, snapshot.CapturedAtUtc ) ), false, false );
				AddHtml( ContentX + 25, y + 28, ContentWidth - 105, 18, Blue( String.Format( "members {0} | {1}", snapshot.HouseCount, first == null ? "no location summary" : first.TypeName + " " + DescribeMap( first.Map ) + " " + first.Location ) ), false, false );
				AddHtml( ContentX + 40, y + 47, ContentWidth - 120, 17, Blue( first == null ? "no design summary" : first.GetDesignRevisionSummary() ), false, false );

				if ( estate.Health == HousingEstateHealth.Healthy )
					AddButton( ContentX + ContentWidth - 55, y + 21, ActionNormalID, ActionPressedID, RevisionButtonBase + i, GumpButtonType.Reply, 0 );
			}

			int pageY = ContentY + ContentHeight - 42;
			AddHtml( ContentX + 260, pageY + 1, 120, 22, WhiteCentered( String.Format( "Page {0} of {1}", m_Page + 1, totalPages ) ), false, false );

			if ( m_Page > 0 )
				AddAction( ContentX + 25, pageY, PreviousPageButton, "Previous" );

			if ( m_Page + 1 < totalPages )
				AddAction( ContentX + ContentWidth - 190, pageY, NextPageButton, "Next" );
		}

		private void DrawReview( HousingEstateController estate, string message, bool confirming )
		{
			HousingEstateSnapshot snapshot;
			HousingValidationResult validation = HousingSuiteGumpOperations.ValidateRestore( estate, m_SelectedRevision, out snapshot );
			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, confirming ? "Final Restore Confirmation" : "Restore Revision Review" );
			AddWrappedText( ContentX + 15, ContentY + 48, ContentWidth - 30, 68, confirming ? "This separate confirmation restores the selected retained state, verifies every live reference and destination again, then appends a new Restore revision. Missing or pruned history fails without mutation." : "Review the selected revision against current live member references, owner identity, map destinations, collisions, and membership. No state changes on this screen." );

			if ( snapshot != null )
			{
				AddHtml( ContentX + 15, ContentY + 122, ContentWidth - 30, 26, Blue( String.Format( "Revision {0} | {1} | actor {2} | {3:u} | members {4}", snapshot.Revision, snapshot.Operation, snapshot.ActorName, snapshot.CapturedAtUtc, snapshot.HouseCount ) ), false, false );
				int start = m_Page * MemberRowsPerPage;
				int count = Math.Min( MemberRowsPerPage, Math.Max( 0, snapshot.HouseCount - start ) );

				for ( int i = 0; i < count; ++i )
				{
					HousingHouseSnapshot house = snapshot.GetHouse( start + i );
					int y = ContentY + 158 + ( i * 40 );
					AddHtml( ContentX + 25, y, ContentWidth - 50, 18, White( house.TypeName + " " + house.FoundationSerial + " -> " + DescribeMap( house.Map ) + " " + house.Location ), false, false );
					AddHtml( ContentX + 40, y + 19, ContentWidth - 65, 18, Blue( house.GetDesignRevisionSummary() ), false, false );
				}

				DrawMemberPager( snapshot.HouseCount );
			}

			if ( !validation.IsValid )
				AddHtml( ContentX + 15, ContentY + 452, ContentWidth - 30, 38, Orange( validation.Message ), false, false );
			else if ( !String.IsNullOrEmpty( message ) )
				AddHtml( ContentX + 15, ContentY + 452, ContentWidth - 30, 38, Orange( message ), false, false );

			if ( validation.IsValid )
				AddAction( ContentX + 375, ContentY + 497, confirming ? ConfirmRestoreButton : ReviewContinueButton, confirming ? "Confirm Restore" : "Continue" );
		}

		private void DrawMemberPager( int count )
		{
			int totalPages = Math.Max( 1, ( count + MemberRowsPerPage - 1 ) / MemberRowsPerPage );
			int y = ContentY + 420;
			AddHtml( ContentX + 260, y + 1, 120, 22, WhiteCentered( String.Format( "Page {0} of {1}", m_Page + 1, totalPages ) ), false, false );

			if ( m_Page > 0 )
				AddAction( ContentX + 25, y, MemberPreviousPageButton, "Previous" );

			if ( m_Page + 1 < totalPages )
				AddAction( ContentX + ContentWidth - 190, y, MemberNextPageButton, "Next" );
		}

		private void DrawError( string error )
		{
			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, "Restore Unavailable" );
			AddWrappedText( ContentX + 15, ContentY + 52, ContentWidth - 30, 80, error );
		}

		private static List<HousingEstateSnapshot> BuildHistory( HousingEstateController estate )
		{
			List<HousingEstateSnapshot> history = new List<HousingEstateSnapshot>();

			if ( estate.OriginalSnapshot != null )
				history.Add( estate.OriginalSnapshot );

			for ( int i = estate.RecentRevisionCount - 1; i >= 0; --i )
			{
				HousingEstateSnapshot snapshot = estate.GetRecentRevision( i );

				if ( snapshot != null )
					history.Add( snapshot );
			}

			return history;
		}
	}
}
