using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Targeting;

namespace Server.Items
{
	public sealed class DyePreviewSession
	{
		private static readonly Dictionary<Mobile, DyePreviewSession> m_Sessions = new Dictionary<Mobile, DyePreviewSession>();
		private static ValidationTimer m_ValidationTimer;
		private static int m_NextToken;
		private static bool m_Initialized;

		private Mobile m_Player;
		private IDyePreviewSource m_Source;
		private Item m_Target;
		private int m_PreviewHue;
		private int m_Token;
		private bool m_Active;
		private bool m_Applying;
		private Point3D m_StartingLocation;
		private Map m_StartingMap;
		private DateTime m_StartedAt;
		private object m_TargetParent;
		private Layer m_TargetLayer;
		private Point3D m_TargetLocation;
		private DateTime m_TargetLastMoved;
		private object m_SourceParent;
		private Point3D m_SourceLocation;
		private DateTime m_SourceLastMoved;

		public Mobile Player
		{
			get { return m_Player; }
		}

		public IDyePreviewSource Source
		{
			get { return m_Source; }
		}

		public Item TargetItem
		{
			get { return m_Target; }
		}

		public int PreviewHue
		{
			get { return m_PreviewHue; }
		}

		public int Token
		{
			get { return m_Token; }
		}

		public bool IsActive
		{
			get { return m_Active && IsCurrent; }
		}

		private bool IsCurrent
		{
			get
			{
				DyePreviewSession current;
				return m_Player != null && m_Sessions.TryGetValue( m_Player, out current ) && current == this;
			}
		}

		public static void Initialize()
		{
			if ( m_Initialized )
				return;

			m_Initialized = true;
			EventSink.Disconnected += new DisconnectedEventHandler( OnDisconnected );
			EventSink.PlayerDeath += new PlayerDeathEventHandler( OnPlayerDeath );
			EventSink.Movement += new MovementEventHandler( OnMovement );
		}

		private DyePreviewSession( Mobile player, IDyePreviewSource source )
		{
			m_Player = player;
			m_Source = source;
			m_PreviewHue = source != null ? source.PreviewHue : 0;
			m_Token = NextToken();
			m_Active = true;
			m_StartingLocation = player != null ? player.Location : Point3D.Zero;
			m_StartingMap = player != null ? player.Map : null;
			m_StartedAt = DateTime.UtcNow;

			Item sourceItem = source != null ? source.SourceItem : null;

			if ( sourceItem != null )
			{
				m_SourceParent = sourceItem.Parent;
				m_SourceLocation = sourceItem.Location;
				m_SourceLastMoved = sourceItem.LastMoved;
			}
		}

		public static DyePreviewSession GetCurrent( Mobile player )
		{
			DyePreviewSession session;

			if ( player != null && m_Sessions.TryGetValue( player, out session ) )
				return session;

			return null;
		}

		public static void BeginTargeting( Mobile from, IDyePreviewSource source )
		{
			string failureMessage;

			if ( !CanBegin( from, source, out failureMessage ) )
			{
				SendFailure( from, failureMessage );
				return;
			}

			Cancel( from, null );

			DyePreviewSession session = Register( from, source );
			from.SendMessage( "Target an equipped or backpack item to preview this dye." );
			from.Target = new DyePreviewTarget( session );
		}

		public static DyePreviewSession Begin( Mobile from, IDyePreviewSource source, Item target )
		{
			string failureMessage;

			if ( !CanBegin( from, source, out failureMessage ) )
			{
				SendFailure( from, failureMessage );
				return null;
			}

			Cancel( from, null );

			DyePreviewSession session = Register( from, source );

			if ( !session.PreviewTarget( target, out failureMessage ) )
			{
				session.Cancel( failureMessage );
				return null;
			}

			return session;
		}

		public static void Cancel( Mobile from, string message )
		{
			DyePreviewSession session = GetCurrent( from );

			if ( session != null )
				session.Cancel( message );
		}

		private static bool CanBegin( Mobile from, IDyePreviewSource source, out string failureMessage )
		{
			failureMessage = null;

			if ( from == null || from.Deleted || !from.Alive )
			{
				failureMessage = "You cannot begin a dye preview right now.";
				return false;
			}

			if ( source == null || source.SourceItem == null || source.SourceItem.Deleted )
			{
				failureMessage = "That dye source is no longer available.";
				return false;
			}

			return source.CanBeginPreview( from, out failureMessage );
		}

		private static DyePreviewSession Register( Mobile from, IDyePreviewSource source )
		{
			DyePreviewSession session = new DyePreviewSession( from, source );
			m_Sessions[from] = session;
			EnsureValidationTimer();
			return session;
		}

		private bool PreviewTarget( Item target, out string failureMessage )
		{
			failureMessage = null;

			if ( !IsCurrent || !m_Active )
				return false;

			if ( !DyePreviewUtility.IsOwnedGear( m_Player, target ) )
			{
				failureMessage = "You may only preview dye on equipment you are wearing or carrying in your backpack.";
				return false;
			}

			if ( !m_Source.CanPreviewTarget( m_Player, target, m_PreviewHue, out failureMessage ) )
				return false;

			if ( !DyePreviewUtility.IsOwnedGear( m_Player, target ) )
			{
				failureMessage = "That item is no longer available.";
				return false;
			}

			m_Target = target;
			m_TargetParent = target.Parent;
			m_TargetLayer = target.Layer;
			m_TargetLocation = target.Location;
			m_TargetLastMoved = target.LastMoved;

			if ( !DyePreviewPackets.SendPreview( m_Player, target, m_PreviewHue ) )
			{
				failureMessage = "The dye preview could not be displayed.";
				return false;
			}

			if ( m_Player.NetState != null )
			{
				m_Player.CloseGump( typeof( DyePreviewGump ) );
				m_Player.SendGump( new DyePreviewGump( this ) );
			}

			return true;
		}

		public void Apply()
		{
			if ( !IsCurrent || !m_Active || m_Applying )
				return;

			string failureMessage;

			if ( !Validate( true, out failureMessage ) )
			{
				Cancel( failureMessage );
				return;
			}

			m_Applying = true;

			Mobile player = m_Player;
			IDyePreviewSource source = m_Source;
			Item target = m_Target;
			int hue = m_PreviewHue;

			Complete();

			bool applied = source.ApplyPermanentDye( player, target, hue );

			if ( applied )
				player.SendMessage( "The item has been permanently dyed." );
		}

		public void ChooseAnother()
		{
			if ( !IsCurrent || !m_Active )
				return;

			string failureMessage;

			if ( !Validate( true, out failureMessage ) )
			{
				Cancel( failureMessage );
				return;
			}

			// Retain the session token and captured hue so cycling sources cannot change the selection.
			DyePreviewPackets.Restore( m_Player, m_Target );

			m_Target = null;
			m_TargetParent = null;
			m_TargetLayer = Layer.Invalid;
			m_TargetLocation = Point3D.Zero;
			m_TargetLastMoved = DateTime.MinValue;

			m_Player.SendMessage( "Target an equipped or backpack item to preview this dye." );
			m_Player.Target = new DyePreviewTarget( this );
		}

		public void Cancel( string message )
		{
			if ( !IsCurrent || !m_Active )
				return;

			Mobile player = m_Player;

			Complete();

			SendFailure( player, message );
		}

		public bool Matches( Mobile from, int token )
		{
			return IsCurrent && m_Active && m_Player == from && m_Token == token;
		}

		public void ValidateNow()
		{
			string failureMessage;

			if ( IsCurrent && m_Active && !Validate( false, out failureMessage ) )
				Cancel( failureMessage );
		}

		private bool Validate( bool requireTarget, out string failureMessage )
		{
			failureMessage = null;

			if ( !IsCurrent || !m_Active || m_Player == null || m_Player.Deleted )
				return false;

			if ( !m_Player.Alive )
			{
				failureMessage = "The dye preview has been cancelled because you died.";
				return false;
			}

			if ( m_Player.Map != m_StartingMap )
			{
				failureMessage = "The dye preview has been cancelled because you changed maps.";
				return false;
			}

			if ( !Utility.InRange( m_Player.Location, m_StartingLocation, DyePreviewUtility.MovementAllowance ) )
			{
				failureMessage = "The dye preview has been cancelled because you moved too far away.";
				return false;
			}

			if ( DateTime.UtcNow - m_StartedAt >= DyePreviewUtility.SessionTimeout )
			{
				failureMessage = "The dye preview has timed out.";
				return false;
			}

			if ( m_Source == null || m_Source.SourceItem == null || m_Source.SourceItem.Deleted
				|| !m_Source.IsSourceStillValid( m_Player ) )
			{
				failureMessage = "That item or dye source is no longer available.";
				return false;
			}

			Item sourceItem = m_Source.SourceItem;

			if ( sourceItem.Parent != m_SourceParent || sourceItem.Location != m_SourceLocation || sourceItem.LastMoved != m_SourceLastMoved )
			{
				failureMessage = "The dye preview has been cancelled because the dye source was moved.";
				return false;
			}

			if ( !requireTarget && m_Target == null )
				return true;

			if ( m_Target == null || m_Target.Deleted || !DyePreviewUtility.IsOwnedGear( m_Player, m_Target ) )
			{
				failureMessage = "That item or dye source is no longer available.";
				return false;
			}

			if ( m_Target.Parent != m_TargetParent || m_Target.Layer != m_TargetLayer
				|| m_Target.Location != m_TargetLocation || m_Target.LastMoved != m_TargetLastMoved )
			{
				failureMessage = "The dye preview has been cancelled because the item was moved.";
				return false;
			}

			return m_Source.CanApply( m_Player, m_Target, m_PreviewHue, out failureMessage );
		}

		private void Complete()
		{
			if ( !IsCurrent || !m_Active )
				return;

			Mobile player = m_Player;
			Item target = m_Target;

			m_Active = false;
			m_Sessions.Remove( player );

			if ( player != null )
			{
				Target activeTarget = player.Target;

				if ( activeTarget is DyePreviewTarget )
				{
					DyePreviewTarget previewTarget = (DyePreviewTarget)activeTarget;

					if ( previewTarget.Token == m_Token )
						activeTarget.Cancel( player, TargetCancelType.Canceled );
				}

				player.CloseGump( typeof( DyePreviewGump ) );
			}

			DyePreviewPackets.Restore( player, target );
			StopValidationTimerIfIdle();

			m_Player = null;
			m_Source = null;
			m_Target = null;
			m_TargetParent = null;
			m_SourceParent = null;
		}

		private static void ValidateSessions()
		{
			if ( m_Sessions.Count == 0 )
			{
				StopValidationTimerIfIdle();
				return;
			}

			List<DyePreviewSession> sessions = new List<DyePreviewSession>( m_Sessions.Values );

			for ( int i = 0; i < sessions.Count; ++i )
			{
				DyePreviewSession session = sessions[i];

				if ( session != null )
					session.ValidateNow();
			}
		}

		private static int NextToken()
		{
			if ( ++m_NextToken <= 0 )
				m_NextToken = 1;

			return m_NextToken;
		}

		private static void EnsureValidationTimer()
		{
			if ( m_ValidationTimer != null )
				return;

			m_ValidationTimer = new ValidationTimer();
			m_ValidationTimer.Start();
		}

		private static void StopValidationTimerIfIdle()
		{
			if ( m_Sessions.Count != 0 || m_ValidationTimer == null )
				return;

			m_ValidationTimer.Stop();
			m_ValidationTimer = null;
		}

		private static void SendFailure( Mobile from, string message )
		{
			if ( from != null && !from.Deleted && !String.IsNullOrEmpty( message ) )
				from.SendMessage( message );
		}

		private static void OnDisconnected( DisconnectedEventArgs e )
		{
			if ( e != null )
				NotifyDisconnected( e.Mobile );
		}

		private static void OnPlayerDeath( PlayerDeathEventArgs e )
		{
			if ( e != null )
				NotifyDeath( e.Mobile );
		}

		internal static void NotifyDisconnected( Mobile mobile )
		{
			Cancel( mobile, null );
		}

		internal static void NotifyDeath( Mobile mobile )
		{
			Cancel( mobile, "The dye preview has been cancelled because you died." );
		}

		private static void OnMovement( MovementEventArgs e )
		{
			if ( e == null || GetCurrent( e.Mobile ) == null )
				return;

			int x = e.Mobile.X;
			int y = e.Mobile.Y;
			Server.Movement.Movement.Offset( e.Direction, ref x, ref y );

			DyePreviewSession session = GetCurrent( e.Mobile );

			if ( session != null
				&& !Utility.InRange( new Point3D( x, y, e.Mobile.Z ), session.m_StartingLocation, DyePreviewUtility.MovementAllowance ) )
			{
				Timer.DelayCall( TimeSpan.Zero, new TimerStateCallback( ValidateMovement ), e.Mobile );
			}
		}

		private static void ValidateMovement( object state )
		{
			Mobile mobile = state as Mobile;
			DyePreviewSession session = GetCurrent( mobile );

			if ( session != null )
				session.ValidateNow();
		}

		private sealed class ValidationTimer : Timer
		{
			public ValidationTimer() : base( TimeSpan.FromSeconds( 1.0 ), TimeSpan.FromSeconds( 1.0 ) )
			{
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				ValidateSessions();
			}
		}

		private sealed class DyePreviewTarget : Target
		{
			private readonly DyePreviewSession m_Session;
			private readonly int m_Token;

			public int Token
			{
				get { return m_Token; }
			}

			public DyePreviewTarget( DyePreviewSession session ) : base( 12, false, TargetFlags.None )
			{
				m_Session = session;
				m_Token = session != null ? session.Token : 0;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_Session == null || !m_Session.Matches( from, m_Token ) )
					return;

				Item item = targeted as Item;
				string failureMessage = null;

				if ( item == null || !m_Session.PreviewTarget( item, out failureMessage ) )
				m_Session.Cancel( !String.IsNullOrEmpty( failureMessage ) ? failureMessage : "That item cannot be dyed with this source." );
			}

			protected override void OnTargetCancel( Mobile from, TargetCancelType cancelType )
			{
				if ( m_Session != null && m_Session.Matches( from, m_Token ) )
					m_Session.Cancel( null );
			}
		}
	}
}
