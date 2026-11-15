using System;
using Server.Gumps;
using Server.Network;

namespace Server.Multis
{
	public sealed class HousingSuiteMovementUndoGump : HousingSuiteGumpBase
	{
		public const int UndoMovementButton = 1;
		public const int KeepChangeButton = 2;

		private readonly int m_UndoToken;

		public HousingSuiteMovementUndoGump( Mobile actor, HousingSuiteMovementUndoRecord record )
			: base( actor, HousingSuiteView.Dashboard, "HousingSuite Movement Completed" )
		{
			m_UndoToken = record == null ? 0 : record.Token;
			DrawContent( record );
		}

		public static void Show( Mobile actor, HousingSuiteMovementUndoRecord record )
		{
			HousingSuiteSession session = HousingSuiteSession.Find( actor );

			if ( actor == null || record == null || session == null || session.PendingMovementUndo != record || record.ActorSerial != actor.Serial || record.SessionToken != session.SessionToken )
				return;

			actor.CloseGump( typeof( HousingSuiteMovementUndoGump ) );
			actor.SendGump( new HousingSuiteMovementUndoGump( actor, record ) );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile actor;
			HousingSuiteSession session;

			if ( info == null || !TryResolveResponse( sender, out actor, out session ) )
				return;

			if ( info.ButtonID == 0 || info.ButtonID == KeepChangeButton )
			{
				if ( session.ClearPendingMovementUndo( m_UndoToken ) )
					HousingSuiteGump.Show( actor, 0, "Movement kept. The immediate undo record was cleared." );

				return;
			}

			if ( info.ButtonID != UndoMovementButton )
				return;

			string error;
			bool hasEstateHistory;
			bool success = HousingSuiteMovementUndo.TryUndo( actor, session, m_UndoToken, out error, out hasEstateHistory );
			string message;

			if ( success )
				message = "Movement was undone successfully.";
			else if ( hasEstateHistory )
				message = error + " Restore History may still be available for this estate.";
			else
				message = error;

			HousingSuiteGump.Show( actor, 0, message );
		}

		private void DrawContent( HousingSuiteMovementUndoRecord record )
		{
			AddSectionHeader( NavigationX + 15, NavigationY + 15, NavigationWidth - 30, "Movement Complete" );
			AddWrappedText( NavigationX + 15, NavigationY + 52, NavigationWidth - 30, 220, "Choose Undo Movement to request one exact transactional inverse move. Choose Keep Change, or close this gump, to discard the immediate undo record." );

			AddSectionHeader( ContentX + 15, ContentY + 15, ContentWidth - 30, "Review Completed Change" );

			if ( record == null )
			{
				AddWrappedText( ContentX + 15, ContentY + 52, ContentWidth - 30, 70, "The movement record is unavailable." );
				return;
			}

			string operation;

			switch ( record.Operation )
			{
				case HousingSuiteMovementUndoOperation.ElevateEstate: operation = "Whole-estate elevation"; break;
				case HousingSuiteMovementUndoOperation.ElevateHouse: operation = "One-house elevation"; break;
				case HousingSuiteMovementUndoOperation.MoveEstate: operation = "Whole-estate movement"; break;
				default: operation = "One-house movement"; break;
			}

			AddHtml( ContentX + 25, ContentY + 60, ContentWidth - 50, 26, Blue( String.Format( "{0} | affected houses {1}{2}", operation, record.AffectedHouseSerials.Length, record.Force ? " | Placement Override" : String.Empty ) ), false, false );
			AddWrappedText( ContentX + 25, ContentY + 100, ContentWidth - 50, 70, String.Format( "Source map {0} {1}\nDestination map {2} {3}", record.SourceMapIndex, record.SourceAnchor, record.DestinationMapIndex, record.DestinationAnchor ) );
			AddHtml( ContentX + 25, ContentY + 185, ContentWidth - 50, 26, White( String.Format( "Delta X {0} | Y {1} | Z {2}", record.DeltaX, record.DeltaY, record.DeltaZ ) ), false, false );
			if ( record.EstateSerial != Server.Serial.MinusOne )
				AddHtml( ContentX + 25, ContentY + 210, ContentWidth - 50, 24, White( String.Format( "Estate revision {0} -> {1}", record.SourceEstateRevision, record.ResultingEstateRevision ) ), false, false );
			AddWrappedText( ContentX + 25, ContentY + 245, ContentWidth - 50, 65, "Undo is allowed only while every affected house, owner, capacity value, map, location, and estate revision still exactly matches this completed movement." );
			AddAction( ContentX + 70, ContentY + 340, UndoMovementButton, "Undo Movement" );
			AddAction( ContentX + 360, ContentY + 340, KeepChangeButton, "Keep Change" );
		}
	}
}
