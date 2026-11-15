using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Items;
using Server.Targeting;

namespace Server.Multis
{
	public static class HousingSuiteCommand
	{
		public const int DefaultNearbyRange = 30;
		public const int MinimumNearbyRange = 1;
		public const int MaximumNearbyRange = 100;

		private static readonly NearbyHouseEntryComparer m_NearbyComparer = new NearbyHouseEntryComparer();
		private static bool m_Initialized;

		public static void Initialize()
		{
			if ( m_Initialized )
				return;

			m_Initialized = true;
			CommandSystem.Register( "HousingSuite", AccessLevel.GameMaster, new CommandEventHandler( OnCommand ) );
			EventSink.Logout += new LogoutEventHandler( OnLogout );
		}

		private static void OnCommand( CommandEventArgs e )
		{
			if ( e != null )
				OpenDashboard( e.Mobile );
		}

		internal static void OnLogout( LogoutEventArgs e )
		{
			if ( e != null )
				HousingSuiteSession.Close( e.Mobile );
		}

		public static void OpenDashboard( Mobile actor )
		{
			HousingSuiteSession session = HousingSuiteSession.Get( actor );

			if ( session == null )
			{
				if ( actor != null && !actor.Deleted )
					actor.SendMessage( "HousingSuite requires Game Master access." );

				return;
			}

			HousingSuiteGump.Show( actor );
		}

		public static void BeginHouseSelection( Mobile actor )
		{
			BeginHouseSelection( actor, false );
		}

		public static void BeginHouseSelection( Mobile actor, bool returnToCombine )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );

			if ( session == null )
				return;

			session.View = HousingSuiteView.HouseSelection;
			actor.Target = new HousingSuiteHouseTarget( actor, returnToCombine );
			actor.SendMessage( "Target a house, house sign, or an object inside a house." );
		}

		public static void BeginDestinationSelection( Mobile actor )
		{
			BeginDestinationSelection( actor, false );
		}

		public static void BeginMoveHouseSelection( Mobile actor )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );

			if ( session == null )
				return;

			session.ClearSelectedHouses();
			session.Force = false;
			session.ConfirmationToken = 0;
			session.View = HousingSuiteView.MoveSourceSelection;
			actor.Target = new HousingSuiteMoveHouseTarget( actor );
			actor.SendMessage( "Target the house to move, its sign, or an object inside it." );
		}

		public static void BeginElevateHouseSelection( Mobile actor )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );

			if ( session == null )
				return;

			session.ClearSelectedHouses();
			session.Force = false;
			session.ConfirmationToken = 0;
			session.View = HousingSuiteView.MoveSourceSelection;
			actor.Target = new HousingSuiteElevateHouseTarget( actor );
			actor.SendMessage( "Target the house to elevate, its sign, or an object inside it." );
		}

		public static void BeginDestinationSelection( Mobile actor, bool moveEstate )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );

			if ( session == null || session.SelectedHouseCount == 0 )
			{
				if ( actor != null && !actor.Deleted )
					actor.SendMessage( "Select an anchor house before choosing a destination." );

				return;
			}

			session.View = HousingSuiteView.DestinationSelection;

			if ( moveEstate )
			{
				actor.Target = new HousingSuiteEstateDestinationTarget( actor );
			}
			else
			{
				HousingSuiteDestinationTarget target;
				string error;

				if ( !HousingSuiteDestinationTarget.TryCreate( actor, out target, out error ) )
				{
					session.View = HousingSuiteView.MoveReview;
					actor.SendMessage( String.IsNullOrEmpty( error ) ? "The house preview could not be prepared." : error );
					HousingSuiteMoveGump.Show( actor, 1, false, 0, error );
					return;
				}

				actor.Target = target;
			}

			actor.SendMessage( "Target the destination point for review." );
		}

		public static void BeginDestinationPreviewAgain( Mobile actor, int returnStage )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );

			if ( !HousingSuiteSession.IsCurrentStaffActor( actor ) || session == null )
			{
				HousingSuiteSession.Close( actor );
				return;
			}

			if ( ( returnStage != 3 && returnStage != 4 )
				|| session.View != HousingSuiteView.MoveReview
				|| session.SelectedHouseCount != 1
				|| !session.HasPendingDestination
				|| ( session.PendingDeltaX == 0 && session.PendingDeltaY == 0 ) )
				return;

			session.View = HousingSuiteView.DestinationSelection;
			HousingSuiteDestinationTarget target;
			string error;

			if ( !HousingSuiteDestinationTarget.TryCreate( actor, true, returnStage, out target, out error ) )
			{
				session.View = HousingSuiteView.MoveReview;
				actor.SendMessage( String.IsNullOrEmpty( error ) ? "The house preview could not be prepared." : error );
				HousingSuiteMoveGump.Show( actor, returnStage, false, 0, error );
				return;
			}

			actor.Target = target;
			actor.SendMessage( "Target a replacement destination point, or cancel to keep the reviewed destination." );
		}

		public static void Pause( Mobile actor )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );

			if ( session != null )
				session.View = HousingSuiteView.Paused;
		}

		public static void Cancel( Mobile actor )
		{
			HousingSuiteSession.Close( actor );
		}

		public static void Complete( Mobile actor )
		{
			HousingSuiteSession.Close( actor );
		}

		public static IList<BaseHouse> FindNearbyHouses( Mobile from, int range )
		{
			List<BaseHouse> houses = new List<BaseHouse>();

			if ( !HousingSuiteSession.IsCurrentStaffActor( from ) || range < 0 || !IsValidWorldMap( from.Map ) )
				return houses;

			int operationalRange = range > MaximumNearbyRange ? MaximumNearbyRange : range;
			long maximumDistance = (long) operationalRange * operationalRange;
			Dictionary<Serial, bool> seen = new Dictionary<Serial, bool>();
			List<NearbyHouseEntry> entries = new List<NearbyHouseEntry>();
			IPooledEnumerable objects = null;

			try
			{
				objects = from.Map.GetObjectsInRange( from.Location, operationalRange );

				foreach ( object value in objects )
				{
					BaseHouse house = ResolveHouse( value );

					if ( house == null || house.Deleted || house.Map != from.Map || World.FindItem( house.Serial ) != house || seen.ContainsKey( house.Serial ) )
						continue;

					long distance;

					if ( !TryGetSquaredDistance( from.Location, house.Location, out distance ) || distance > maximumDistance )
						continue;

					seen.Add( house.Serial, true );
					entries.Add( new NearbyHouseEntry( house, distance ) );
				}
			}
			finally
			{
				if ( objects != null )
					objects.Free();
			}

			entries.Sort( m_NearbyComparer );

			for ( int i = 0; i < entries.Count; ++i )
				houses.Add( entries[i].House );

			return houses;
		}

		internal static bool IsValidWorldMap( Map map )
		{
			if ( map == null || map == Map.Internal )
				return false;

			int index = map.MapIndex;
			return index >= 0 && index < Map.Maps.Length && Map.Maps[index] == map;
		}

		internal static Map ResolveWorldMap( int mapIndex )
		{
			if ( mapIndex < 0 || mapIndex >= Map.Maps.Length )
				return null;

			Map map = Map.Maps[mapIndex];
			return IsValidWorldMap( map ) ? map : null;
		}

		internal static BaseHouse ResolveHouse( object targeted )
		{
			BaseHouse house = targeted as BaseHouse;

			if ( house != null )
				return house;

			HouseSign sign = targeted as HouseSign;

			if ( sign != null )
				return sign.Deleted ? null : sign.Owner;

			Item item = targeted as Item;

			if ( item != null )
				return BaseHouse.FindHouseAt( item );

			Mobile mobile = targeted as Mobile;
			return mobile == null ? null : BaseHouse.FindHouseAt( mobile );
		}

		public static BaseHouse ResolveHouse( Mobile actor, object targeted )
		{
			BaseHouse house = ResolveHouse( targeted );

			if ( house != null )
				return house;

			if ( targeted is Item || targeted is Mobile || actor == null || !IsValidWorldMap( actor.Map ) )
				return null;

			IPoint3D point = targeted as IPoint3D;
			return point == null ? null : BaseHouse.FindHouseAt( new Point3D( point ), actor.Map, 16 );
		}

		private static bool TryGetSquaredDistance( Point3D from, Point3D to, out long distance )
		{
			try
			{
				long deltaX = checked( (long) to.X - from.X );
				long deltaY = checked( (long) to.Y - from.Y );
				distance = checked( checked( deltaX * deltaX ) + checked( deltaY * deltaY ) );
				return true;
			}
			catch ( OverflowException )
			{
				distance = Int64.MaxValue;
				return false;
			}
		}

		private struct NearbyHouseEntry
		{
			public readonly BaseHouse House;
			public readonly long Distance;

			public NearbyHouseEntry( BaseHouse house, long distance )
			{
				House = house;
				Distance = distance;
			}
		}

		private sealed class NearbyHouseEntryComparer : IComparer<NearbyHouseEntry>
		{
			public int Compare( NearbyHouseEntry left, NearbyHouseEntry right )
			{
				if ( left.Distance < right.Distance )
					return -1;

				if ( left.Distance > right.Distance )
					return 1;

				int leftSerial = (int) left.House.Serial;
				int rightSerial = (int) right.House.Serial;
				return leftSerial < rightSerial ? -1 : ( leftSerial > rightSerial ? 1 : 0 );
			}
		}
	}

	internal static class HousingSuiteTargetContext
	{
		public static bool TryCapture( HousingSuiteSession session, bool allowEmpty, out Serial anchorSerial, out Serial estateSerial, out int revision )
		{
			anchorSerial = Serial.MinusOne;
			estateSerial = Serial.MinusOne;
			revision = 0;

			if ( session == null )
				return false;

			if ( session.SelectedHouseCount == 0 )
				return allowEmpty && session.SelectedEstateSerial == Serial.MinusOne && session.ExpectedEstateRevision == 0;

			anchorSerial = session.GetSelectedHouseSerial( 0 );
			BaseHouse anchor = World.FindItem( anchorSerial ) as BaseHouse;

			if ( !TryGetAuthoritativeContext( anchor, out estateSerial, out revision ) )
				return false;

			return session.SelectedEstateSerial == estateSerial && session.ExpectedEstateRevision == revision;
		}

		public static bool TryResolve( Serial anchorSerial, Serial estateSerial, int revision, out BaseHouse anchor )
		{
			anchor = World.FindItem( anchorSerial ) as BaseHouse;
			Serial authoritativeEstate;
			int authoritativeRevision;

			return TryGetAuthoritativeContext( anchor, out authoritativeEstate, out authoritativeRevision )
				&& authoritativeEstate == estateSerial
				&& authoritativeRevision == revision;
		}

		public static bool TryGetAuthoritativeContext( BaseHouse anchor, out Serial estateSerial, out int revision )
		{
			estateSerial = Serial.MinusOne;
			revision = 0;

			if ( anchor == null || anchor.Deleted || World.FindItem( anchor.Serial ) != anchor || !HousingSuiteCommand.IsValidWorldMap( anchor.Map ) )
				return false;

			HouseFoundation foundation = anchor as HouseFoundation;
			HousingEstateController estate;

			if ( foundation == null || !HousingEstateRegistry.TryGetEstate( foundation, out estate ) )
				return true;

			if ( estate == null || estate.Deleted || World.FindItem( estate.Serial ) != estate )
				return false;

			estateSerial = estate.Serial;
			revision = estate.CurrentRevision;
			return true;
		}
	}

	public sealed class HousingSuiteHouseTarget : Target
	{
		private readonly Serial m_ActorSerial;
		private readonly int m_SessionToken;
		private readonly int m_StateVersion;
		private readonly HousingSuiteView m_ExpectedView;
		private readonly Serial m_AnchorSerial;
		private readonly Serial m_EstateSerial;
		private readonly int m_ExpectedEstateRevision;
		private readonly bool m_ContextValid;
		private readonly bool m_ReturnToCombine;

		public HousingSuiteHouseTarget( Mobile actor )
			: this( actor, false )
		{
		}

		public HousingSuiteHouseTarget( Mobile actor, bool returnToCombine )
			: base( -1, false, TargetFlags.None )
		{
			m_ReturnToCombine = returnToCombine;
			m_ActorSerial = actor == null ? Serial.MinusOne : actor.Serial;
			HousingSuiteSession session = HousingSuiteSession.Find( actor );
			m_SessionToken = session == null ? 0 : session.SessionToken;
			m_StateVersion = session == null ? 0 : session.StateVersion;
			m_ExpectedView = HousingSuiteView.HouseSelection;
			m_ContextValid = HousingSuiteTargetContext.TryCapture( session, true, out m_AnchorSerial, out m_EstateSerial, out m_ExpectedEstateRevision );
		}

		protected override void OnTarget( Mobile from, object targeted )
		{
			HousingSuiteSession session;

			if ( !TryResolveSession( from, out session ) || !ValidateContext( from ) )
				return;

			if ( !HousingSuiteCommand.IsValidWorldMap( from.Map ) )
			{
				from.SendMessage( "The current map is invalid for house targeting." );
				return;
			}

			BaseHouse house = HousingSuiteCommand.ResolveHouse( from, targeted );

			if ( house == null )
			{
				from.SendMessage( "That target is not inside a house." );
				return;
			}

			if ( house.Deleted || World.FindItem( house.Serial ) != house || !HousingSuiteCommand.IsValidWorldMap( house.Map ) || house.Map != from.Map )
			{
				from.SendMessage( "That house is deleted or no longer on your current map." );
				return;
			}

			Serial selectedEstateSerial = Serial.MinusOne;
			int selectedEstateRevision = 0;

			if ( m_AnchorSerial == Serial.MinusOne && !HousingSuiteTargetContext.TryGetAuthoritativeContext( house, out selectedEstateSerial, out selectedEstateRevision ) )
			{
				from.SendMessage( "That house's estate state is no longer available." );
				return;
			}

			if ( !session.AddSelectedHouse( house.Serial ) )
			{
				from.SendMessage( "That house is already selected or the selection limit has been reached." );
				return;
			}

			if ( m_AnchorSerial == Serial.MinusOne )
				session.SetSelectedEstate( selectedEstateSerial, selectedEstateRevision );

			from.SendMessage( "Selected house {0}.", house.Serial );

			if ( m_ReturnToCombine )
				HousingSuiteCombineGump.Show( from, 1, 0, 0, String.Empty );
			else
				HousingSuiteGump.Show( from, 0, String.Empty );
		}

		protected override void OnTargetNotAccessible( Mobile from, object targeted )
		{
			SelectRejectedSign( from, targeted );
		}

		protected override void OnTargetUntargetable( Mobile from, object targeted )
		{
			SelectRejectedSign( from, targeted );
		}

		private void SelectRejectedSign( Mobile from, object targeted )
		{
			HouseSign sign = targeted as HouseSign;

			if ( sign == null )
			{
				from.SendMessage( "That inaccessible target is not a house sign." );
				return;
			}

			OnTarget( from, sign );
		}

		protected override void OnTargetCancel( Mobile from, TargetCancelType cancelType )
		{
			HousingSuiteSession session;

			if ( TryResolveSession( from, out session ) && ValidateContext( from ) )
			{
				if ( m_ReturnToCombine )
					HousingSuiteCombineGump.Show( from, 1, 0, 0, "House targeting was cancelled." );
				else
					HousingSuiteGump.Show( from, 0, "House targeting was cancelled." );
			}
		}

		protected override void OnTargetDeleted( Mobile from, object targeted )
		{
			HousingSuiteSession session;

			if ( TryResolveSession( from, out session ) && ValidateContext( from ) )
				from.SendMessage( "That target was deleted before it could be selected." );
		}

		private bool TryResolveSession( Mobile from, out HousingSuiteSession session )
		{
			session = null;
			Mobile actor = World.FindMobile( m_ActorSerial );

			if ( actor == null || actor != from || !HousingSuiteSession.IsCurrentStaffActor( actor ) )
			{
				HousingSuiteSession.Close( m_ActorSerial, m_SessionToken );
				return false;
			}

			session = HousingSuiteSession.Find( actor );

			if ( session == null || session.SessionToken != m_SessionToken )
			{
				HousingSuiteSession.Close( m_ActorSerial, m_SessionToken );
				actor.SendMessage( "That HousingSuite target belongs to an expired session." );
				return false;
			}

			if ( session.View != m_ExpectedView || session.StateVersion != m_StateVersion )
			{
				actor.SendMessage( "That HousingSuite target is no longer active for the current session state." );
				return false;
			}

			return true;
		}

		private bool ValidateContext( Mobile from )
		{
			BaseHouse ignored;

			if ( m_ContextValid && ( m_AnchorSerial == Serial.MinusOne || HousingSuiteTargetContext.TryResolve( m_AnchorSerial, m_EstateSerial, m_ExpectedEstateRevision, out ignored ) ) )
				return true;

			from.SendMessage( "The selected estate changed while targeting. Reopen the selection." );
			return false;
		}
	}

	public sealed class HousingSuiteDestinationTarget : MultiTarget
	{
		private readonly HousingSuiteDestinationTargetContext m_Context;

		private HousingSuiteDestinationTarget( HousingSuiteDestinationTargetContext context )
			: base( context.PreviewMultiID, context.PreviewOffset, -1, true, TargetFlags.None )
		{
			m_Context = context;
		}

		public static bool TryCreate( Mobile actor, out HousingSuiteDestinationTarget target, out string error )
		{
			return TryCreate( actor, false, 1, out target, out error );
		}

		public static bool TryCreate( Mobile actor, bool preservePendingOnCancel, int cancelReturnStage, out HousingSuiteDestinationTarget target, out string error )
		{
			target = null;
			HousingSuiteDestinationTargetContext context;

			if ( preservePendingOnCancel && cancelReturnStage != 3 && cancelReturnStage != 4 )
			{
				error = "The preview return stage is invalid.";
				return false;
			}

			if ( !HousingSuiteDestinationTargetContext.TryCapturePreview( actor, preservePendingOnCancel, cancelReturnStage, out context, out error ) )
				return false;

			target = new HousingSuiteDestinationTarget( context );
			return true;
		}

		protected override void OnTarget( Mobile from, object targeted )
		{
			m_Context.TryRecordDestination( from, targeted, false );
		}

		protected override void OnTargetCancel( Mobile from, TargetCancelType cancelType )
		{
			m_Context.TryCancelDestination( from, false );
		}
	}

	public sealed class HousingSuiteEstateDestinationTarget : Target
	{
		private readonly HousingSuiteDestinationTargetContext m_Context;

		public HousingSuiteEstateDestinationTarget( Mobile actor )
			: base( -1, true, TargetFlags.None )
		{
			m_Context = HousingSuiteDestinationTargetContext.Capture( actor );
		}

		protected override void OnTarget( Mobile from, object targeted )
		{
			m_Context.TryRecordDestination( from, targeted, true );
		}

		protected override void OnTargetCancel( Mobile from, TargetCancelType cancelType )
		{
			m_Context.TryCancelDestination( from, true );
		}
	}

	internal sealed class HousingSuiteDestinationTargetContext
	{
		private readonly Serial m_ActorSerial;
		private readonly int m_SessionToken;
		private readonly int m_StateVersion;
		private readonly HousingSuiteView m_ExpectedView;
		private readonly Serial m_AnchorSerial;
		private readonly Serial m_EstateSerial;
		private readonly int m_ExpectedEstateRevision;
		private readonly bool m_ContextValid;
		private readonly int m_PreviewMultiID;
		private readonly Point3D m_PreviewOffset;
		private readonly HousingSuiteHouseFingerprint m_AnchorFingerprint;
		private readonly bool m_PreservePendingOnCancel;
		private readonly int m_CancelReturnStage;

		public int PreviewMultiID { get { return m_PreviewMultiID; } }
		public Point3D PreviewOffset { get { return m_PreviewOffset; } }

		private HousingSuiteDestinationTargetContext( Mobile actor, HousingSuiteSession session, Serial anchorSerial, Serial estateSerial, int expectedEstateRevision, bool contextValid, int previewMultiID, Point3D previewOffset, HousingSuiteHouseFingerprint anchorFingerprint, bool preservePendingOnCancel, int cancelReturnStage )
		{
			m_ActorSerial = actor == null ? Serial.MinusOne : actor.Serial;
			m_SessionToken = session == null ? 0 : session.SessionToken;
			m_StateVersion = session == null ? 0 : session.StateVersion;
			m_ExpectedView = HousingSuiteView.DestinationSelection;
			m_AnchorSerial = anchorSerial;
			m_EstateSerial = estateSerial;
			m_ExpectedEstateRevision = expectedEstateRevision;
			m_ContextValid = contextValid;
			m_PreviewMultiID = previewMultiID;
			m_PreviewOffset = previewOffset;
			m_AnchorFingerprint = anchorFingerprint;
			m_PreservePendingOnCancel = preservePendingOnCancel;
			m_CancelReturnStage = cancelReturnStage;
		}

		public static HousingSuiteDestinationTargetContext Capture( Mobile actor )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );
			Serial anchorSerial;
			Serial estateSerial;
			int expectedEstateRevision;
			bool contextValid = HousingSuiteTargetContext.TryCapture( session, false, out anchorSerial, out estateSerial, out expectedEstateRevision );
			int previewMultiID = -1;
			BaseHouse anchor;

			if ( contextValid )
			{
				if ( HousingSuiteTargetContext.TryResolve( anchorSerial, estateSerial, expectedEstateRevision, out anchor ) )
					previewMultiID = anchor.ItemID & 0x3FFF;
				else
					contextValid = false;
			}

			return new HousingSuiteDestinationTargetContext( actor, session, anchorSerial, estateSerial, expectedEstateRevision, contextValid, previewMultiID, Point3D.Zero, new HousingSuiteHouseFingerprint(), false, 1 );
		}

		public static bool TryCapturePreview( Mobile actor, bool preservePendingOnCancel, int cancelReturnStage, out HousingSuiteDestinationTargetContext context, out string error )
		{
			context = null;
			error = null;
			HousingSuiteSession session = HousingSuiteSession.Find( actor );
			Serial anchorSerial;
			Serial estateSerial;
			int expectedEstateRevision;

			if ( !HousingSuiteTargetContext.TryCapture( session, false, out anchorSerial, out estateSerial, out expectedEstateRevision ) )
			{
				error = "The selected anchor or estate is no longer valid.";
				return false;
			}

			BaseHouse anchor;

			if ( !HousingSuiteTargetContext.TryResolve( anchorSerial, estateSerial, expectedEstateRevision, out anchor ) )
			{
				error = "The selected anchor or estate changed while preparing the preview.";
				return false;
			}

			Point3D previewOffset;

			if ( !TryGetPreviewOffset( anchor, out previewOffset ) )
			{
				error = "The selected house does not have a valid exterior preview hotspot.";
				return false;
			}

			HousingSuiteHouseFingerprint anchorFingerprint = preservePendingOnCancel ? HousingSuiteHouseFingerprint.Capture( anchor ) : new HousingSuiteHouseFingerprint();
			context = new HousingSuiteDestinationTargetContext( actor, session, anchorSerial, estateSerial, expectedEstateRevision, true, anchor.ItemID & 0x3FFF, previewOffset, anchorFingerprint, preservePendingOnCancel, cancelReturnStage );
			return true;
		}

		public static bool TryGetPreviewOffset( BaseHouse anchor, out Point3D offset )
		{
			offset = Point3D.Zero;

			if ( anchor == null || anchor.Deleted )
				return false;

			HousePlacementEntry entry = HousePlacementEntry.Find( anchor );

			if ( entry != null )
			{
				offset = entry.Offset;
				return IsPacketOffset( offset );
			}

			MultiComponentList components = anchor.Components;

			if ( components == null || components.Width <= 0 || components.Height <= 0 )
				return false;

			try
			{
				offset = new Point3D(
					checked( components.Min.X + ( ( components.Width - 1 ) / 2 ) ),
					checked( components.Min.Y + components.Height ),
					0 );
			}
			catch ( OverflowException )
			{
				offset = Point3D.Zero;
				return false;
			}

			if ( !IsPacketOffset( offset ) )
				return false;

			if ( offset == Point3D.Zero
				&& offset.X >= components.Min.X && offset.X <= components.Max.X
				&& offset.Y >= components.Min.Y && offset.Y <= components.Max.Y )
				return false;

			return true;
		}

		private static bool IsPacketOffset( Point3D offset )
		{
			return offset.X >= Int16.MinValue && offset.X <= Int16.MaxValue
				&& offset.Y >= Int16.MinValue && offset.Y <= Int16.MaxValue
				&& offset.Z >= Int16.MinValue && offset.Z <= Int16.MaxValue;
		}

		public bool TryRecordDestination( Mobile from, object targeted, bool moveEstate )
		{
			HousingSuiteSession session;
			BaseHouse anchor;

			if ( !TryResolveSession( from, out session ) || !TryResolveLiveContext( from, out anchor ) )
				return false;

			if ( !HousingSuiteCommand.IsValidWorldMap( from.Map ) )
			{
				from.SendMessage( "The destination map is invalid." );
				return false;
			}

			Point3D hotspot;

			if ( !TryGetWorldPoint( from, targeted, out hotspot ) )
			{
				from.SendMessage( "Target a current world point or entity on your map." );
				return false;
			}

			Point3D destination;

			try
			{
				destination = moveEstate
					? hotspot
					: new Point3D(
						checked( hotspot.X - m_PreviewOffset.X ),
						checked( hotspot.Y - m_PreviewOffset.Y ),
						checked( hotspot.Z - m_PreviewOffset.Z ) );
			}
			catch ( OverflowException )
			{
				from.SendMessage( "The preview hotspot is outside the supported coordinate range." );
				return false;
			}

			if ( destination.X < 0 || destination.Y < 0 || destination.X >= from.Map.Width || destination.Y >= from.Map.Height )
			{
				from.SendMessage( "The destination point is outside the map bounds." );
				return false;
			}

			int deltaX;
			int deltaY;
			int deltaZ;

			try
			{
				deltaX = checked( destination.X - anchor.X );
				deltaY = checked( destination.Y - anchor.Y );
				deltaZ = checked( destination.Z - anchor.Z );
			}
			catch ( OverflowException )
			{
				from.SendMessage( "The destination delta is outside the supported coordinate range." );
				return false;
			}

			session.SetPendingDestination( destination, from.Map.MapIndex, deltaX, deltaY, deltaZ );
			session.Force = false;
			session.ConfirmationToken = 0;
			session.View = HousingSuiteView.MoveReview;
			from.SendMessage( "Destination recorded for review: delta {0}, {1}, {2}.", deltaX, deltaY, deltaZ );
			HousingSuiteMoveGump.Show( from, 3, moveEstate, 0, String.Empty );
			return true;
		}

		public bool TryCancelDestination( Mobile from, bool moveEstate )
		{
			HousingSuiteSession session;
			BaseHouse anchor;

			if ( !TryResolveSession( from, out session ) || !TryResolveLiveContext( from, out anchor ) )
				return false;

			if ( !m_PreservePendingOnCancel )
				session.ClearPendingDestination();

			session.View = HousingSuiteView.MoveReview;
			from.SendMessage( "Destination targeting was cancelled." );
			HousingSuiteMoveGump.Show( from, m_PreservePendingOnCancel ? m_CancelReturnStage : 1, moveEstate, 0, "Destination targeting was cancelled." );
			return true;
		}

		private bool TryResolveSession( Mobile from, out HousingSuiteSession session )
		{
			session = null;
			Mobile actor = World.FindMobile( m_ActorSerial );

			if ( actor == null || actor != from || !HousingSuiteSession.IsCurrentStaffActor( actor ) )
			{
				HousingSuiteSession.Close( m_ActorSerial, m_SessionToken );
				return false;
			}

			session = HousingSuiteSession.Find( actor );

			if ( session == null || session.SessionToken != m_SessionToken )
			{
				HousingSuiteSession.Close( m_ActorSerial, m_SessionToken );
				actor.SendMessage( "That HousingSuite target belongs to an expired session." );
				return false;
			}

			if ( session.View != m_ExpectedView || session.StateVersion != m_StateVersion )
			{
				actor.SendMessage( "That HousingSuite target is no longer active for the current session state." );
				return false;
			}

			return true;
		}

		private bool TryResolveLiveContext( Mobile from, out BaseHouse anchor )
		{
			if ( !m_ContextValid || !HousingSuiteTargetContext.TryResolve( m_AnchorSerial, m_EstateSerial, m_ExpectedEstateRevision, out anchor ) )
			{
				anchor = null;
				from.SendMessage( "The selected anchor or estate changed while targeting. Reopen the review." );
				return false;
			}

			if ( m_PreservePendingOnCancel && !HousingSuiteHouseFingerprint.Matches( anchor, m_AnchorFingerprint ) )
			{
				from.SendMessage( "The selected house changed while targeting. Reopen the review." );
				return false;
			}

			return true;
		}

		private static bool TryGetWorldPoint( Mobile actor, object targeted, out Point3D point )
		{
			point = Point3D.Zero;

			if ( actor == null || !HousingSuiteCommand.IsValidWorldMap( actor.Map ) )
				return false;

			Item item = targeted as Item;

			if ( item != null )
			{
				Item current = World.FindItem( item.Serial );

				if ( current != item || item.Deleted || item.Map != actor.Map )
					return false;

				point = item.GetWorldLocation();
				return true;
			}

			Mobile mobile = targeted as Mobile;

			if ( mobile != null )
			{
				Mobile current = World.FindMobile( mobile.Serial );

				if ( current != mobile || mobile.Deleted || mobile.Map != actor.Map )
					return false;

				point = mobile.Location;
				return true;
			}

			IPoint3D value = targeted as IPoint3D;

			if ( value == null )
				return false;

			point = new Point3D( value.X, value.Y, value.Z );
			return true;
		}
	}
}
