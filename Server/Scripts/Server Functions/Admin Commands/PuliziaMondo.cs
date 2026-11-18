using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Accounting;
using Server.Guilds;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

namespace Server.Misc
{
	/// <summary>
	/// Strumenti per un server personale ricavato dal salvataggio pubblico originale.
	///
	/// 1) PULIZIA DEL MONDO (una tantum): se esiste il file Data/pulizia-mondo.txt, all'avvio vengono eliminati
	///    TUTTI gli account con i loro personaggi, case, barche, animali e gilde; poi il mondo viene salvato
	///    e il file viene rinominato in pulizia-mondo.fatto.txt (così non si ripete).
	///    Restano mappe, mostri, PNG, mercanti e tutto ciò che appartiene al mondo.
	///
	/// 2) PRIMO PROPRIETARIO: se esiste Data/primo-owner.txt (una riga con il nome dell'account) e nel mondo
	///    non c'è nessun account Administrator o superiore, quando quell'account entra nel gioco riceve il
	///    livello Owner (account e personaggio). Dopo la promozione il file viene rinominato.
	/// </summary>
	public static class PuliziaMondo
	{
		private static string DataFile( string name )
		{
			return Path.Combine( Core.BaseDirectory, Path.Combine( "Data", name ) );
		}

		public static void Initialize()
		{
			EventSink.Login += new LoginEventHandler( OnLogin );

			if ( File.Exists( DataFile( "pulizia-mondo.txt" ) ) )
				Timer.DelayCall( TimeSpan.FromSeconds( 1.0 ), new TimerCallback( Pulisci ) );
		}

		private static void Pulisci()
		{
			Console.WriteLine( "PuliziaMondo: inizio della rimozione di tutti gli account dei giocatori..." );

			List<Account> accounts = new List<Account>();
			foreach ( IAccount ia in Accounts.GetAccounts() )
			{
				Account a = ia as Account;
				if ( a != null )
					accounts.Add( a );
			}

			HashSet<Mobile> giocatori = new HashSet<Mobile>();
			foreach ( Account a in accounts )
				for ( int i = 0; i < a.Length; ++i )
					if ( a[i] != null )
						giocatori.Add( a[i] );

			// barche dei giocatori
			int barche = 0;
			List<BaseBoat> boats = new List<BaseBoat>();
			foreach ( Item it in World.Items.Values )
			{
				BaseBoat b = it as BaseBoat;
				if ( b != null && b.Owner != null && giocatori.Contains( b.Owner ) )
					boats.Add( b );
			}
			foreach ( BaseBoat b in boats ) { b.Delete(); barche++; }

			// animali e seguaci dei giocatori (anche nelle stalle)
			int animali = 0;
			List<Mobile> pets = new List<Mobile>();
			foreach ( Mobile m in World.Mobiles.Values )
			{
				BaseCreature bc = m as BaseCreature;
				if ( bc == null )
					continue;
				if ( ( bc.ControlMaster != null && giocatori.Contains( bc.ControlMaster ) ) ||
				     ( bc.SummonMaster != null && giocatori.Contains( bc.SummonMaster ) ) ||
				     ( bc.IsStabled && bc.ControlMaster == null && bc.Map == Map.Internal ) )
					pets.Add( bc );
			}
			foreach ( Mobile m in pets ) { m.Delete(); animali++; }

			// gilde
			int gilde = 0;
			List<BaseGuild> guilds = new List<BaseGuild>( BaseGuild.List.Values );
			foreach ( BaseGuild g in guilds )
			{
				Guild gg = g as Guild;
				if ( gg != null && !gg.Disbanded ) { gg.Disband(); gilde++; }
			}

			// account, personaggi e case
			int pg = giocatori.Count;
			foreach ( Account a in accounts )
				a.Delete();

			Console.WriteLine( "PuliziaMondo: eliminati {0} account, {1} personaggi, {2} barche, {3} animali, {4} gilde.", accounts.Count, pg, barche, animali, gilde );

			try
			{
				string fatto = DataFile( "pulizia-mondo.fatto.txt" );
				if ( File.Exists( fatto ) ) File.Delete( fatto );
				File.Move( DataFile( "pulizia-mondo.txt" ), fatto );
			}
			catch ( Exception e ) { Console.WriteLine( "PuliziaMondo: impossibile rinominare il file di controllo: {0}", e.Message ); }

			World.Save();
			Console.WriteLine( "PuliziaMondo: mondo salvato." );
		}

		private static void OnLogin( LoginEventArgs e )
		{
			Mobile m = e.Mobile;
			Account acct = m == null ? null : m.Account as Account;
			string file = DataFile( "primo-owner.txt" );

			if ( acct == null || !File.Exists( file ) )
				return;

			string nome = File.ReadAllText( file ).Trim();
			if ( nome.Length == 0 || !nome.Equals( acct.Username, StringComparison.OrdinalIgnoreCase ) )
				return;

			// sicurezza: solo se il mondo non ha ancora un amministratore
			foreach ( IAccount ia in Accounts.GetAccounts() )
			{
				Account a = ia as Account;
				if ( a != null && a != acct && a.AccessLevel >= AccessLevel.Administrator )
				{
					Console.WriteLine( "PuliziaMondo: '{0}' non promosso, esiste già l'amministratore '{1}'.", acct.Username, a.Username );
					return;
				}
			}

			acct.AccessLevel = AccessLevel.Owner;
			m.AccessLevel = AccessLevel.Owner;
			m.SendMessage( 0x35, "Sei il proprietario di questo mondo (livello Owner). Usa i poteri con saggezza!" );
			Console.WriteLine( "PuliziaMondo: account '{0}' promosso a Owner.", acct.Username );

			try
			{
				string fatto = DataFile( "primo-owner.fatto.txt" );
				if ( File.Exists( fatto ) ) File.Delete( fatto );
				File.Move( file, fatto );
			}
			catch { }
		}
	}
}
