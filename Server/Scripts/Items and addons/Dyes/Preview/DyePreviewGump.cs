using System;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
	public sealed class DyePreviewGump : Gump
	{
		private const int ButtonApply = 1;
		private const int ButtonChooseAnother = 2;
		private const int ButtonCancel = 3;

		private readonly DyePreviewSession m_Session;
		private readonly int m_Token;

		public DyePreviewGump( DyePreviewSession session ) : base( 180, 120 )
		{
			m_Session = session;
			m_Token = session != null ? session.Token : 0;

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 460, 285, 9270 );
			AddAlphaRegion( 16, 16, 428, 253 );
			AddHtml( 28, 24, 404, 28, "<basefont color=#FFCC66><center><big>Dye Preview</big></center>", false, false );

			string itemName = session != null ? DyePreviewUtility.GetItemDisplayName( session.TargetItem ) : "item";
			string sourceName = session != null && session.Source != null ? session.Source.GetSourceDisplayName() : "dye source";
			string consumption = session != null && session.Source != null ? session.Source.GetConsumptionMessage() : null;
			string details = "<basefont color=#FFFFFF>The current color is temporary and visible only to you.<br>"
				+ "<br>Item: <basefont color=#FFCC66>" + DyePreviewUtility.EncodeHtml( itemName ) + "</basefont>"
				+ "<br>Source: <basefont color=#FFCC66>" + DyePreviewUtility.EncodeHtml( sourceName ) + "</basefont>"
				+ "<br>Hue: <basefont color=#FFCC66>" + ( session != null ? session.PreviewHue.ToString() : "0" ) + "</basefont>";

			if ( !String.IsNullOrEmpty( consumption ) )
				details += "<br><basefont color=#FFCC66>" + DyePreviewUtility.EncodeHtml( consumption ) + "</basefont>";

			details += "</basefont>";

			AddHtml( 30, 62, 400, 122, details, false, false );

			AddButton( 34, 218, 4005, 4007, ButtonApply, GumpButtonType.Reply, 0 );
			AddLabel( 68, 220, 68, "Apply Dye" );

			AddButton( 168, 218, 4005, 4007, ButtonChooseAnother, GumpButtonType.Reply, 0 );
			AddLabel( 202, 220, 1153, "Choose Another" );

			AddButton( 345, 218, 4005, 4007, ButtonCancel, GumpButtonType.Reply, 0 );
			AddLabel( 379, 220, 1152, "Cancel" );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender != null ? sender.Mobile : null;

			if ( m_Session == null || !m_Session.Matches( from, m_Token ) )
				return;

			switch ( info.ButtonID )
			{
				case ButtonApply:
					m_Session.Apply();
					break;
				case ButtonChooseAnother:
					m_Session.ChooseAnother();
					break;
				default:
					m_Session.Cancel( null );
					break;
			}
		}

		public override void OnServerClose( NetState owner )
		{
			base.OnServerClose( owner );

			Mobile from = owner != null ? owner.Mobile : null;

			if ( m_Session != null && m_Session.Matches( from, m_Token ) )
				m_Session.Cancel( null );
		}
	}
}
