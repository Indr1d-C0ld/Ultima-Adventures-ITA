using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Network;

namespace Server.Multis
{
	public abstract class HousingSuiteGumpBase : Gump
	{
		public const int FrameWidth = 900;
		public const int FrameHeight = 650;
		public const int NavigationColumnWidth = 220;
		protected const int NavigationX = 35;
		protected const int NavigationY = 75;
		protected const int NavigationWidth = NavigationColumnWidth;
		protected const int NavigationHeight = 550;
		protected const int ContentX = 270;
		protected const int ContentY = 75;
		protected const int ContentWidth = 595;
		protected const int ContentHeight = 550;
		protected const int ActionNormalID = 4005;
		protected const int ActionPressedID = 4007;
		protected const int EntryBorderTileID = 9107;

		private static int m_NextGeneration;

		private readonly Serial m_ActorSerial;
		private readonly int m_SessionToken;
		private readonly int m_StateVersion;
		private readonly int m_Generation;
		private readonly HousingSuiteView m_ExpectedView;
		private readonly Serial[] m_SelectedSerials;
		private readonly Serial m_AnchorSerial;
		private readonly Serial m_EstateSerial;
		private readonly int m_EstateRevision;
		private readonly bool m_ContextValid;

		protected HousingSuiteGumpBase( Mobile actor, HousingSuiteView view, string title )
			: base( 35, 35 )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );

			if ( session != null )
			{
				session.View = view;
				session.ConfirmationToken = NextGeneration();
			}

			m_ActorSerial = actor == null ? Server.Serial.MinusOne : actor.Serial;
			m_SessionToken = session == null ? 0 : session.SessionToken;
			m_StateVersion = session == null ? 0 : session.StateVersion;
			m_Generation = session == null ? 0 : session.ConfirmationToken;
			m_ExpectedView = view;
			m_SelectedSerials = session == null ? new Serial[0] : CopySerials( session.SelectedHouseSerials );
			m_ContextValid = HousingSuiteTargetContext.TryCapture( session, true, out m_AnchorSerial, out m_EstateSerial, out m_EstateRevision );

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage( 0 );
			AddBackground( 0, 0, FrameWidth, FrameHeight, 9270 );
			AddImageTiled( 20, 20, FrameWidth - 40, FrameHeight - 40, 2624 );
			AddAlphaRegion( 20, 20, FrameWidth - 40, FrameHeight - 40 );
			AddHtml( 20, 30, FrameWidth - 40, 32, WhiteCentered( title ), false, false );
			AddPanel( NavigationX, NavigationY, NavigationWidth, NavigationHeight );
			AddPanel( ContentX, ContentY, ContentWidth, ContentHeight );
		}

		protected Serial ActorSerial { get { return m_ActorSerial; } }
		protected Serial[] RenderedSelectedSerials { get { return CopySerials( m_SelectedSerials ); } }
		protected Serial RenderedEstateSerial { get { return m_EstateSerial; } }
		protected int RenderedEstateRevision { get { return m_EstateRevision; } }

		protected bool TryResolveResponse( NetState sender, out Mobile actor, out HousingSuiteSession session )
		{
			actor = null;
			session = null;

			if ( sender == null )
				return false;

			Mobile current = World.FindMobile( m_ActorSerial );

			if ( current == null || current.Deleted )
			{
				HousingSuiteSession.Close( m_ActorSerial, m_SessionToken );
				return false;
			}

			if ( sender.Mobile != current )
				return false;

			if ( !HousingSuiteSession.IsCurrentStaffActor( current ) )
			{
				HousingSuiteSession.Close( m_ActorSerial, m_SessionToken );
				return false;
			}

			HousingSuiteSession found = HousingSuiteSession.Find( current );

			if ( found == null
				|| found.SessionToken != m_SessionToken
				|| found.StateVersion != m_StateVersion
				|| found.ConfirmationToken != m_Generation
				|| found.View != m_ExpectedView
				|| !SelectedSerialsMatch( found ) )
				return false;

			if ( !m_ContextValid )
				return false;

			if ( m_AnchorSerial != Server.Serial.MinusOne )
			{
				BaseHouse anchor;

				if ( !HousingSuiteTargetContext.TryResolve( m_AnchorSerial, m_EstateSerial, m_EstateRevision, out anchor ) )
					return false;
			}

			if ( found.SelectedEstateSerial != m_EstateSerial || found.ExpectedEstateRevision != m_EstateRevision )
				return false;

			actor = current;
			session = found;
			return true;
		}

		protected void AddPanel( int x, int y, int width, int height )
		{
			AddImageTiled( x, y, width, height, 2624 );
			AddAlphaRegion( x, y, width, height );
		}

		protected void AddEntryTray( int x, int y, int width, int height, int entryID, string value )
		{
			AddHtml( x, y - 22, width, 20, White( "Entry here" ), false, false );
			AddPanel( x, y, width, height );
			AddTextEntry( x + 4, y + 2, width - 8, height - 4, 1152, entryID, value );
			AddImageTiled( x, y, width, 1, EntryBorderTileID );
			AddImageTiled( x, y + height - 1, width, 1, EntryBorderTileID );
			AddImageTiled( x, y, 1, height, EntryBorderTileID );
			AddImageTiled( x + width - 1, y, 1, height, EntryBorderTileID );
		}

		protected void AddSectionHeader( int x, int y, int width, string text )
		{
			AddHtml( x, y, width, 24, Blue( text ), false, false );
		}

		protected void AddWrappedText( int x, int y, int width, int height, string text )
		{
			AddHtml( x, y, width, height, White( text ), false, false );
		}

		protected void AddAction( int x, int y, int buttonID, string label )
		{
			AddButton( x, y, ActionNormalID, ActionPressedID, buttonID, GumpButtonType.Reply, 0 );
			AddHtml( x + 38, y + 1, 140, 22, Orange( label ), false, false );
		}

		protected static string White( string value )
		{
			return "<BASEFONT COLOR=#FFFFFF>" + Fix( value ) + "</BASEFONT>";
		}

		protected static string Blue( string value )
		{
			return "<BASEFONT COLOR=#9DC6FF>" + Fix( value ) + "</BASEFONT>";
		}

		protected static string Orange( string value )
		{
			return "<BASEFONT COLOR=#FFB347>" + Fix( value ) + "</BASEFONT>";
		}

		protected static string WhiteCentered( string value )
		{
			return "<CENTER>" + White( value ) + "</CENTER>";
		}

		protected static string Fix( string value )
		{
			return Utility.FixHtml( value ?? String.Empty );
		}

		protected static string DescribeMap( Map map )
		{
			return map == null ? "(none)" : map.Name;
		}

		protected static string DescribeMobile( Mobile mobile )
		{
			if ( mobile == null || mobile.Deleted )
				return "(missing)";

			return String.IsNullOrEmpty( mobile.Name ) ? mobile.Serial.ToString() : mobile.Name + " " + mobile.Serial;
		}

		protected static bool IsLiveHouse( BaseHouse house )
		{
			return house != null
				&& !house.Deleted
				&& World.FindItem( house.Serial ) == house
				&& HousingSuiteCommand.IsValidWorldMap( house.Map );
		}

		protected static bool IsWithinRange( Point3D from, Point3D to, int range )
		{
			long deltaX = (long) to.X - from.X;
			long deltaY = (long) to.Y - from.Y;
			long maximum = (long) range * range;
			return ( deltaX * deltaX ) + ( deltaY * deltaY ) <= maximum;
		}

		protected static bool TryGetEstateContext( BaseHouse house, out Serial estateSerial, out int revision )
		{
			return HousingSuiteTargetContext.TryGetAuthoritativeContext( house, out estateSerial, out revision );
		}

		protected void PauseAndClose( HousingSuiteSession session )
		{
			if ( session != null )
				session.View = HousingSuiteView.Paused;
		}

		protected void CloseForceWarning( HousingSuiteSession session )
		{
			if ( session == null )
				return;

			session.Force = false;
			session.View = HousingSuiteView.Paused;
			session.ConfirmationToken = session.ConfirmationToken == Int32.MaxValue ? 1 : session.ConfirmationToken + 1;
		}

		private bool SelectedSerialsMatch( HousingSuiteSession session )
		{
			if ( session == null || session.SelectedHouseCount != m_SelectedSerials.Length )
				return false;

			for ( int i = 0; i < m_SelectedSerials.Length; ++i )
			{
				if ( session.GetSelectedHouseSerial( i ) != m_SelectedSerials[i] )
					return false;
			}

			return true;
		}

		private static Serial[] CopySerials( IList<Serial> values )
		{
			if ( values == null || values.Count == 0 )
				return new Serial[0];

			Serial[] copy = new Serial[values.Count];

			for ( int i = 0; i < values.Count; ++i )
				copy[i] = values[i];

			return copy;
		}

		private static Serial[] CopySerials( Serial[] values )
		{
			if ( values == null || values.Length == 0 )
				return new Serial[0];

			Serial[] copy = new Serial[values.Length];
			Array.Copy( values, copy, values.Length );
			return copy;
		}

		private static int NextGeneration()
		{
			if ( m_NextGeneration == Int32.MaxValue )
				m_NextGeneration = 0;

			return ++m_NextGeneration;
		}
	}

	public sealed class HousingSuiteGump : HousingSuiteGumpBase
	{
		private const int RowsPerPage = 4;

		public const int SelectTargetButton = 1;
		public const int CombineButton = 2;
		public const int SplitButton = 3;
		public const int MoveButton = 4;
		public const int RestoreButton = 5;
		public const int HousePropertiesButton = 6;
		public const int PreviousPageButton = 10;
		public const int NextPageButton = 11;
		public const int ApplyNearbyRangeButton = 12;
		public const int PauseButton = 90;
		public const int CancelButton = 91;
		public const int NearbyButtonBase = 1000;
		public const int NearbyRangeTextEntry = 20;

		private readonly int m_Page;
		private readonly Serial[] m_NearbySerials;

		public int RenderedNearbyCount { get { return m_NearbySerials.Length; } }

		public HousingSuiteGump( Mobile actor, int page, string message )
			: base( actor, HousingSuiteView.Dashboard, "HousingSuite Estate Dashboard" )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );
			int nearbyRange = session == null ? HousingSuiteCommand.DefaultNearbyRange : session.NearbyRange;
			IList<BaseHouse> nearby = HousingSuiteCommand.FindNearbyHouses( actor, nearbyRange );
			int totalPages = Math.Max( 1, ( nearby.Count + RowsPerPage - 1 ) / RowsPerPage );
			m_Page = Math.Max( 0, Math.Min( page, totalPages - 1 ) );
			int start = m_Page * RowsPerPage;
			int count = Math.Min( RowsPerPage, nearby.Count - start );

			if ( count < 0 )
				count = 0;

			m_NearbySerials = new Serial[count];

			for ( int i = 0; i < count; ++i )
				m_NearbySerials[i] = nearby[start + i].Serial;

			DrawNavigation( session );
			DrawOverview( actor, message );
			DrawNearbyRows( actor, nearby, start, totalPages );
		}

		public static void Show( Mobile actor )
		{
			Show( actor, 0, String.Empty );
		}

		public static void Show( Mobile actor, int page, string message )
		{
			if ( actor == null || actor.Deleted || HousingSuiteSession.Find( actor ) == null )
				return;

			actor.CloseGump( typeof( HousingSuiteGump ) );
			actor.SendGump( new HousingSuiteGump( actor, page, message ) );
		}

		internal Serial GetRenderedNearbySerial( int index )
		{
			if ( index < 0 || index >= m_NearbySerials.Length )
				throw new ArgumentOutOfRangeException( "index" );

			return m_NearbySerials[index];
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

			if ( buttonID == CancelButton )
			{
				HousingSuiteCommand.Cancel( actor );
				return;
			}

			if ( buttonID == ApplyNearbyRangeButton )
			{
				TextRelay entry = info.GetTextEntry( NearbyRangeTextEntry );
				int range;

				if ( entry == null || !Int32.TryParse( entry.Text, out range ) || !session.TrySetNearbyRange( range ) )
					Show( actor, m_Page, "Enter a nearby range from 1 to 100." );
				else
					Show( actor, m_Page, "Nearby range set to " + range + " tiles." );

				return;
			}

			if ( buttonID == PreviousPageButton && m_Page > 0 )
			{
				Show( actor, m_Page - 1, String.Empty );
				return;
			}

			if ( buttonID == NextPageButton )
			{
				int total = HousingSuiteCommand.FindNearbyHouses( actor, session.NearbyRange ).Count;

				if ( ( m_Page + 1 ) * RowsPerPage < total )
					Show( actor, m_Page + 1, String.Empty );

				return;
			}

			if ( buttonID == SelectTargetButton )
			{
				HousingSuiteCommand.BeginHouseSelection( actor );
				return;
			}

			if ( buttonID == CombineButton )
			{
				HousingSuiteCombineGump.Show( actor, 1, 0, 0, String.Empty );
				return;
			}

			if ( buttonID == SplitButton )
			{
				HousingSuiteSplitGump.Show( actor, false, String.Empty );
				return;
			}

			if ( buttonID == MoveButton )
			{
				HousingSuiteMoveGump.Show( actor, 1, false, 0, String.Empty );
				return;
			}

			if ( buttonID == RestoreButton )
			{
				HousingSuiteRestoreGump.Show( actor, 1, 0, -1, String.Empty );
				return;
			}

			if ( buttonID == HousePropertiesButton )
			{
				BaseHouse house;

				if ( !TryResolveSingleSelectedHouse( session, out house ) )
				{
					Show( actor, m_Page, "Select exactly one live house to open its properties." );
					return;
				}

				Show( actor, m_Page, "Opened properties for house " + house.Serial + "." );
				actor.CloseGump( typeof( PropertiesGump ) );
				actor.SendGump( new PropertiesGump( actor, house ) );
				return;
			}

			if ( buttonID >= NearbyButtonBase && buttonID < NearbyButtonBase + m_NearbySerials.Length )
			{
				int index = buttonID - NearbyButtonBase;
				BaseHouse house = World.FindItem( m_NearbySerials[index] ) as BaseHouse;

				if ( !IsLiveHouse( house ) || house.Map != actor.Map || !IsWithinRange( actor.Location, house.Location, session.NearbyRange ) )
					return;

				Serial estateSerial;
				int revision;

				if ( !TryGetEstateContext( house, out estateSerial, out revision ) )
					return;

				session.ClearSelectedHouses();

				if ( !session.AddSelectedHouse( house.Serial ) )
					return;

				session.SetSelectedEstate( estateSerial, revision );
				Show( actor, m_Page, "Selected house " + house.Serial + "." );
			}
		}

		internal static bool TryResolveSingleSelectedHouse( HousingSuiteSession session, out BaseHouse house )
		{
			house = null;

			if ( session == null || session.SelectedHouseCount != 1 )
				return false;

			house = World.FindItem( session.GetSelectedHouseSerial( 0 ) ) as BaseHouse;

			if ( !IsLiveHouse( house ) )
			{
				house = null;
				return false;
			}

			return true;
		}

		private void DrawNavigation( HousingSuiteSession session )
		{
			BaseHouse selectedHouse;
			AddSectionHeader( NavigationX + 15, NavigationY + 15, NavigationWidth - 30, "Operations" );
			AddAction( NavigationX + 15, NavigationY + 52, SelectTargetButton, "Select House" );
			AddAction( NavigationX + 15, NavigationY + 92, CombineButton, "Combine Plots" );
			AddAction( NavigationX + 15, NavigationY + 132, SplitButton, "Split Estate" );
			AddAction( NavigationX + 15, NavigationY + 172, MoveButton, "Move / Elevate" );
			AddAction( NavigationX + 15, NavigationY + 212, RestoreButton, "Restore History" );

			if ( TryResolveSingleSelectedHouse( session, out selectedHouse ) )
				AddAction( NavigationX + 15, NavigationY + 252, HousePropertiesButton, "House Properties" );

			AddWrappedText( NavigationX + 15, NavigationY + 310, NavigationWidth - 30, 100, "Closing or pausing retains the current serial-only draft. Cancel Session discards it." );
			AddAction( NavigationX + 15, NavigationY + 445, PauseButton, "Pause and Close" );
			AddAction( NavigationX + 15, NavigationY + 485, CancelButton, "Cancel Session" );
		}

		private void DrawOverview( Mobile actor, string message )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );
			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, "Overview and Current Draft" );
			AddWrappedText( ContentX + 15, ContentY + 42, ContentWidth - 30, 58, "Use the nearby list to focus a live house or estate, then choose a guided operation. Every confirmation is revalidated against the current session, serials, map, estate revision, and server state." );

			string summary = session == null
				? "No live HousingSuite session."
				: String.Format( "Selected houses: {0} | Estate: {1} | Expected revision: {2}", session.SelectedHouseCount, session.SelectedEstateSerial, session.ExpectedEstateRevision );

			AddHtml( ContentX + 15, ContentY + 105, ContentWidth - 30, 24, Blue( summary ), false, false );

			if ( !String.IsNullOrEmpty( message ) )
				AddHtml( ContentX + 15, ContentY + 130, ContentWidth - 30, 24, Orange( message ), false, false );

			int nearbyRange = session == null ? HousingSuiteCommand.DefaultNearbyRange : session.NearbyRange;
			AddSectionHeader( ContentX + 15, ContentY + 145, ContentWidth - 30, "Nearby Houses (bounded to " + nearbyRange + " tiles)" );
			AddHtml( ContentX + 15, ContentY + 174, 120, 22, White( "Range (1-100):" ), false, false );
			AddEntryTray( ContentX + 135, ContentY + 193, 75, 22, NearbyRangeTextEntry, nearbyRange.ToString() );
			AddAction( ContentX + 220, ContentY + 190, ApplyNearbyRangeButton, "Apply Range" );
		}

		private void DrawNearbyRows( Mobile actor, IList<BaseHouse> nearby, int start, int totalPages )
		{
			for ( int i = 0; i < m_NearbySerials.Length; ++i )
			{
				BaseHouse house = nearby[start + i];
				int y = ContentY + 220 + ( i * 70 );
				AddPanel( ContentX + 15, y, ContentWidth - 30, 65 );

				HousingEstateController estate;
				HouseFoundation foundation = house as HouseFoundation;
				string estateStatus = foundation != null && HousingEstateRegistry.TryGetEstate( foundation, out estate )
					? "Estate " + estate.Serial + " " + estate.Health + " r" + estate.CurrentRevision
					: "Standalone";
				long deltaX = (long) house.X - actor.X;
				long deltaY = (long) house.Y - actor.Y;
				long distance = ( deltaX * deltaX ) + ( deltaY * deltaY );
				string firstLine = String.Format( "{0} {1} | owner {2}", house.GetType().Name, house.Serial, DescribeMobile( house.Owner ) );
				string secondLine = String.Format( "{0} {1} | distance² {2} | {3}", DescribeMap( house.Map ), house.Location, distance, estateStatus );

				AddHtml( ContentX + 25, y + 8, ContentWidth - 100, 22, White( firstLine ), false, false );
				AddHtml( ContentX + 25, y + 32, ContentWidth - 100, 22, Blue( secondLine ), false, false );
				AddButton( ContentX + ContentWidth - 55, y + 21, ActionNormalID, ActionPressedID, NearbyButtonBase + i, GumpButtonType.Reply, 0 );
			}

			int pageY = ContentY + ContentHeight - 42;
			AddHtml( ContentX + 260, pageY + 2, 120, 22, WhiteCentered( String.Format( "Page {0} of {1}", m_Page + 1, totalPages ) ), false, false );

			if ( m_Page > 0 )
				AddAction( ContentX + 25, pageY, PreviousPageButton, "Previous" );

			if ( m_Page + 1 < totalPages )
				AddAction( ContentX + ContentWidth - 190, pageY, NextPageButton, "Next" );
		}
	}
}
