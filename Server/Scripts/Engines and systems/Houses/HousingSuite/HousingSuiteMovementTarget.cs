using System;
using Server.Targeting;

namespace Server.Multis
{
	public abstract class HousingSuiteHouseSourceTarget : Target
	{
		private readonly Serial m_ActorSerial;
		private readonly int m_SessionToken;
		private readonly int m_StateVersion;
		private readonly HousingSuiteView m_ExpectedView;

		protected HousingSuiteHouseSourceTarget( Mobile actor )
			: base( -1, false, TargetFlags.None )
		{
			m_ActorSerial = actor == null ? Serial.MinusOne : actor.Serial;
			HousingSuiteSession session = HousingSuiteSession.Find( actor );
			m_SessionToken = session == null ? 0 : session.SessionToken;
			m_StateVersion = session == null ? 0 : session.StateVersion;
			m_ExpectedView = HousingSuiteView.MoveSourceSelection;
		}

		protected abstract void OnHouseSelected( Mobile from, HousingSuiteSession session, BaseHouse house );

		protected sealed override void OnTarget( Mobile from, object targeted )
		{
			SelectAndContinue( from, targeted );
		}

		protected override void OnTargetNotAccessible( Mobile from, object targeted )
		{
			SelectRejectedSign( from, targeted );
		}

		protected override void OnTargetUntargetable( Mobile from, object targeted )
		{
			SelectRejectedSign( from, targeted );
		}

		protected bool TrySelectHouse( Mobile from, object targeted, out HousingSuiteSession session, out BaseHouse house )
		{
			session = null;
			house = null;

			if ( !TryResolveSession( from, out session ) )
				return false;

			if ( !HousingSuiteCommand.IsValidWorldMap( from.Map ) )
			{
				from.SendMessage( "The current map is invalid for house targeting." );
				return false;
			}

			house = HousingSuiteCommand.ResolveHouse( from, targeted );

			if ( house == null )
			{
				from.SendMessage( "That target is not inside a house." );
				return false;
			}

			if ( house.Deleted || World.FindItem( house.Serial ) != house || !HousingSuiteCommand.IsValidWorldMap( house.Map ) || house.Map != from.Map )
			{
				from.SendMessage( "That house is deleted or no longer on your current map." );
				return false;
			}

			Serial estateSerial;
			int estateRevision;

			if ( !HousingSuiteTargetContext.TryGetAuthoritativeContext( house, out estateSerial, out estateRevision ) )
			{
				from.SendMessage( "That house's estate state is no longer available." );
				return false;
			}

			session.ClearSelectedHouses();

			if ( !session.AddSelectedHouse( house.Serial ) )
			{
				from.SendMessage( "That house could not be selected for movement." );
				house = null;
				return false;
			}

			session.SetSelectedEstate( estateSerial, estateRevision );
			return true;
		}

		protected void ReturnToMovementChoice( Mobile from, string message )
		{
			HousingSuiteSession session;

			if ( !TryResolveSession( from, out session ) )
				return;

			session.ClearSelectedHouses();
			session.Force = false;
			session.ConfirmationToken = 0;
			session.View = HousingSuiteView.MoveReview;
			from.SendMessage( message );
			HousingSuiteMoveGump.Show( from, 1, false, 0, "House targeting was cancelled." );
		}

		protected void ReportDeletedTarget( Mobile from )
		{
			HousingSuiteSession session;

			if ( TryResolveSession( from, out session ) )
				from.SendMessage( "That target was deleted before it could be selected." );
		}

		private void SelectAndContinue( Mobile from, object targeted )
		{
			HousingSuiteSession session;
			BaseHouse house;

			if ( TrySelectHouse( from, targeted, out session, out house ) )
				OnHouseSelected( from, session, house );
		}

		private void SelectRejectedSign( Mobile from, object targeted )
		{
			HouseSign sign = targeted as HouseSign;

			if ( sign == null )
			{
				from.SendMessage( "That inaccessible target is not a house sign." );
				return;
			}

			SelectAndContinue( from, sign );
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
	}

	public sealed class HousingSuiteMoveHouseTarget : HousingSuiteHouseSourceTarget
	{
		public HousingSuiteMoveHouseTarget( Mobile actor )
			: base( actor )
		{
		}

		protected override void OnHouseSelected( Mobile from, HousingSuiteSession session, BaseHouse house )
		{
			from.SendMessage( "Selected house {0}. Target its destination.", house.Serial );
			HousingSuiteCommand.BeginDestinationSelection( from, false );
		}

		protected override void OnTargetCancel( Mobile from, TargetCancelType cancelType )
		{
			ReturnToMovementChoice( from, "House movement targeting was cancelled." );
		}

		protected override void OnTargetDeleted( Mobile from, object targeted )
		{
			ReportDeletedTarget( from );
		}
	}

	public sealed class HousingSuiteElevateHouseTarget : HousingSuiteHouseSourceTarget
	{
		public HousingSuiteElevateHouseTarget( Mobile actor )
			: base( actor )
		{
		}

		protected override void OnHouseSelected( Mobile from, HousingSuiteSession session, BaseHouse house )
		{
			from.SendMessage( "Selected house {0}. Choose its elevation.", house.Serial );
			HousingSuiteMoveGump.Show( from, 2, false, 0, String.Empty );
		}

		protected override void OnTargetCancel( Mobile from, TargetCancelType cancelType )
		{
			ReturnToMovementChoice( from, "House elevation targeting was cancelled." );
		}

		protected override void OnTargetDeleted( Mobile from, object targeted )
		{
			ReportDeletedTarget( from );
		}
	}
}
