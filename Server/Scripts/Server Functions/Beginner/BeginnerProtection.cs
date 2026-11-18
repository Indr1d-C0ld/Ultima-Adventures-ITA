using System;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Commands;

namespace Server.Misc
{
	/// <summary>
	/// Protezione del principiante: rende meno punitive le prime ore di gioco.
	/// Soglie in MyServerSettings (BeginnerProtectionHours / BeginnerProtectionSkillPoints).
	/// Stato salvato in PlayerMobile.BeginnerState (0 attiva, 1 rinunciata, 2 terminata).
	/// </summary>
	public static class BeginnerProtection
	{
		public const int StateActive = 0;
		public const int StateOptedOut = 1;
		public const int StateExpired = 2;

		public static void Initialize()
		{
			CommandSystem.Register( "principiante", AccessLevel.Player, new CommandEventHandler( Status_OnCommand ) );
			CommandSystem.Register( "novizio", AccessLevel.Player, new CommandEventHandler( Status_OnCommand ) );
			CommandSystem.Register( "beginner", AccessLevel.Player, new CommandEventHandler( Status_OnCommand ) );
			EventSink.Login += new LoginEventHandler( OnLogin );
		}

		/// <summary>True se il giocatore gode ancora della protezione. Aggiorna lo stato quando una soglia viene superata.</summary>
		public static bool IsActive( Mobile m )
		{
			PlayerMobile pm = m as PlayerMobile;

			if ( pm == null || pm.AccessLevel > AccessLevel.Player || !MyServerSettings.BeginnerProtectionEnabled() )
				return false;

			if ( pm.BeginnerState != StateActive )
				return false;

			if ( pm.SoulBound ) // modalità "anima legata" (hardcore): scelta consapevole, nessuna protezione
				return false;

			if ( ThresholdReached( pm ) )
			{
				pm.BeginnerState = StateExpired;
				Timer.DelayCall( TimeSpan.Zero, new TimerStateCallback( NotifyExpired ), pm );
				return false;
			}

			return true;
		}

		private static bool ThresholdReached( PlayerMobile pm )
		{
			if ( pm.GameTime.TotalHours >= MyServerSettings.BeginnerProtectionHours() )
				return true;

			if ( pm.SkillsTotal >= MyServerSettings.BeginnerProtectionSkillPoints() * 10 )
				return true;

			return false;
		}

		private static void NotifyExpired( object state )
		{
			Mobile m = state as Mobile;

			if ( m == null || m.Deleted || m.NetState == null )
				return;

			m.SendMessage( 0x35, "La tua protezione da principiante è terminata: ora sei un avventuriero a tutti gli effetti. Che la fortuna ti accompagni!" );
			m.SendMessage( 0x35, "Da adesso la morte, le trappole e i viaggi magici comportano i rischi normali. Digita [guida per rileggere i consigli." );
		}

		/// <summary>Trasforma un tipo di trappola del pavimento "distruttivo" in uno innocuo (solo danno/rivelazione).</summary>
		public static int SafeTrapType( int trapType )
		{
			switch ( trapType )
			{
				case 2:  // fa cadere lo zaino
				case 3:  // trasforma le monete in piombo
				case 4:  // distrugge un oggetto
				case 5:  // toglie una caratteristica
				case 8:  // distrugge gemme
				case 9:  // distrugge reagenti
				case 10: // lega i libri
				case 12: // toglie fama
				case 13: // maledice un oggetto
				case 20: // rompe le frecce
				case 21: // contamina il cibo
				case 22: // rompe le pozioni
				case 23: // fonde i gioielli
					return 1; // trappola rivelatrice: innocua
			}
			return trapType;
		}

		private static void OnLogin( LoginEventArgs e )
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if ( pm == null || !IsActive( pm ) )
				return;

			Timer.DelayCall( TimeSpan.FromSeconds( 8.0 ), new TimerStateCallback( LoginNotice ), pm );
		}

		private static void LoginNotice( object state )
		{
			PlayerMobile pm = state as PlayerMobile;

			if ( pm == null || pm.Deleted || pm.NetState == null || !IsActive( pm ) )
				return;

			pm.SendMessage( 0x59, "Sei sotto la protezione del principiante ({0}). Digita [principiante per i dettagli e [guida per la guida al gioco.", RemainingText( pm ) );
		}

		public static string RemainingText( PlayerMobile pm )
		{
			double hours = MyServerSettings.BeginnerProtectionHours() - pm.GameTime.TotalHours;
			if ( hours < 0 ) hours = 0;
			int points = MyServerSettings.BeginnerProtectionSkillPoints() - ( pm.SkillsTotal / 10 );
			if ( points < 0 ) points = 0;

			int h = (int)hours;
			int min = (int)( ( hours - h ) * 60 );

			return String.Format( "restano {0} h {1} min di gioco oppure {2} punti abilità", h, min, points );
		}

		[Usage( "principiante" )]
		[Description( "Mostra lo stato della protezione del principiante e permette di rinunciarvi." )]
		private static void Status_OnCommand( CommandEventArgs e )
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if ( pm == null )
				return;

			pm.CloseGump( typeof( BeginnerProtectionGump ) );
			pm.SendGump( new BeginnerProtectionGump( pm ) );
		}
	}

	public class BeginnerProtectionGump : Gump
	{
		private const int Width = 460;
		private const int Height = 400;

		public BeginnerProtectionGump( PlayerMobile pm ) : base( 60, 60 )
		{
			bool active = BeginnerProtection.IsActive( pm );

			AddPage( 0 );
			AddBackground( 0, 0, Width, Height, 9270 );
			AddAlphaRegion( 12, 12, Width - 24, Height - 24 );

			AddHtml( 20, 20, Width - 40, 22, "<BASEFONT COLOR=#FFD700><CENTER>PROTEZIONE DEL PRINCIPIANTE</CENTER></BASEFONT>", false, false );

			string text;

			if ( active )
			{
				text = "<BASEFONT COLOR=#FFFFFF>Sei ancora un <B>principiante</B>: il mondo è duro, ma per ora ti risparmia il peggio.<BR><BR>"
					+ "Finché la protezione è attiva:<BR>"
					+ "• quando muori <B>non perdi oggetti</B> (restano nel tuo zaino), né abilità, caratteristiche, fama o karma;<BR>"
					+ "• le trappole che rubano o distruggono oggetti non hanno effetto su di te;<BR>"
					+ "• i viaggi magici (Richiamo, Portale) non ti portano maledizioni;<BR>"
					+ "• fame e sete calano più lentamente.<BR><BR>"
					+ "La protezione termina da sola dopo " + MyServerSettings.BeginnerProtectionHours() + " ore di gioco o al raggiungimento di "
					+ MyServerSettings.BeginnerProtectionSkillPoints() + " punti abilità totali.<BR>"
					+ "<BASEFONT COLOR=#88FF88>Ora: " + BeginnerProtection.RemainingText( pm ) + ".</BASEFONT><BR><BR>"
					+ "<BASEFONT COLOR=#FFFFFF>Se ti senti pronto, puoi rinunciarvi subito. <BASEFONT COLOR=#FF8888>La scelta è definitiva.</BASEFONT>";
			}
			else if ( pm.BeginnerState == BeginnerProtection.StateOptedOut )
			{
				text = "<BASEFONT COLOR=#FFFFFF>Hai rinunciato alla protezione del principiante. Il mondo ti tratta ormai come un avventuriero esperto.<BR><BR>Digita [guida per consultare la guida al gioco.";
			}
			else
			{
				text = "<BASEFONT COLOR=#FFFFFF>La tua protezione del principiante è terminata: la morte, le trappole e i viaggi magici comportano i rischi normali.<BR><BR>Digita [guida per consultare la guida al gioco.";
			}

			AddHtml( 25, 50, Width - 50, Height - 110, text, false, true );

			if ( active )
			{
				AddButton( 25, Height - 45, 4005, 4007, 1, GumpButtonType.Reply, 0 );
				AddHtml( 60, Height - 43, 200, 20, "<BASEFONT COLOR=#FF8888>Rinuncia alla protezione</BASEFONT>", false, false );
			}

			AddButton( Width - 120, Height - 45, 4017, 4019, 0, GumpButtonType.Reply, 0 );
			AddHtml( Width - 85, Height - 43, 60, 20, "<BASEFONT COLOR=#FFFFFF>Chiudi</BASEFONT>", false, false );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			PlayerMobile pm = sender.Mobile as PlayerMobile;

			if ( pm == null || info.ButtonID != 1 || !BeginnerProtection.IsActive( pm ) )
				return;

			pm.SendGump( new BeginnerOptOutConfirmGump() );
		}
	}

	public class BeginnerOptOutConfirmGump : Gump
	{
		public BeginnerOptOutConfirmGump() : base( 120, 120 )
		{
			AddPage( 0 );
			AddBackground( 0, 0, 340, 150, 9270 );
			AddAlphaRegion( 12, 12, 316, 126 );
			AddHtml( 25, 25, 290, 70, "<BASEFONT COLOR=#FFFFFF>Vuoi davvero rinunciare per sempre alla protezione del principiante?</BASEFONT>", false, false );
			AddButton( 25, 105, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtml( 60, 107, 100, 20, "<BASEFONT COLOR=#FF8888>Sì, rinuncio</BASEFONT>", false, false );
			AddButton( 200, 105, 4017, 4019, 0, GumpButtonType.Reply, 0 );
			AddHtml( 235, 107, 80, 20, "<BASEFONT COLOR=#FFFFFF>No</BASEFONT>", false, false );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			PlayerMobile pm = sender.Mobile as PlayerMobile;

			if ( pm == null || info.ButtonID != 1 || !BeginnerProtection.IsActive( pm ) )
				return;

			pm.BeginnerState = BeginnerProtection.StateOptedOut;
			pm.SendMessage( 0x35, "Hai rinunciato alla protezione del principiante. Buona fortuna, avventuriero!" );
		}
	}
}
