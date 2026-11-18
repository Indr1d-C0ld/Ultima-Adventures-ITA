using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Misc
{
	/// <summary>
	/// Guida in gioco: [guida (alias [aiuto-gioco, [guide).
	/// Gli argomenti sono file di testo in Data/Guida/: il nome del file stabilisce l'ordine,
	/// la prima riga è il titolo, il resto è il testo (HTML dei gump: &lt;BR&gt;, &lt;B&gt;, &lt;BASEFONT COLOR=#...&gt;).
	/// Le righe vuote diventano a-capo. Si possono aggiungere o modificare argomenti senza ricompilare: [guida ricarica (solo staff).
	/// </summary>
	public static class GuideSystem
	{
		public class Topic
		{
			public string Title;
			public string Body;
		}

		private static List<Topic> m_Topics;

		public static List<Topic> Topics
		{
			get
			{
				if ( m_Topics == null )
					Load();
				return m_Topics;
			}
		}

		public static void Initialize()
		{
			CommandSystem.Register( "guida", AccessLevel.Player, new CommandEventHandler( Guide_OnCommand ) );
			CommandSystem.Register( "guide", AccessLevel.Player, new CommandEventHandler( Guide_OnCommand ) );
			CommandSystem.Register( "manuale", AccessLevel.Player, new CommandEventHandler( Guide_OnCommand ) );
			EventSink.Login += new LoginEventHandler( OnLogin );
		}

		public static void Load()
		{
			List<Topic> list = new List<Topic>();
			string dir = Path.Combine( Core.BaseDirectory, Path.Combine( "Data", "Guida" ) );

			if ( Directory.Exists( dir ) )
			{
				string[] files = Directory.GetFiles( dir, "*.txt" );
				Array.Sort( files, StringComparer.Ordinal );

				foreach ( string file in files )
				{
					string[] lines = File.ReadAllLines( file, Encoding.UTF8 );
					if ( lines.Length == 0 )
						continue;

					StringBuilder sb = new StringBuilder();
					for ( int i = 1; i < lines.Length; ++i )
					{
						string l = lines[i].TrimEnd();
						if ( l.Length == 0 )
							sb.Append( "<BR><BR>" );
						else
							sb.Append( l ).Append( ' ' );
					}

					Topic t = new Topic();
					t.Title = lines[0].Trim();
					t.Body = sb.ToString();
					list.Add( t );
				}
			}

			if ( list.Count == 0 )
			{
				Topic t = new Topic();
				t.Title = "Guida non disponibile";
				t.Body = "La cartella Data/Guida non contiene argomenti.";
				list.Add( t );
			}

			m_Topics = list;
		}

		[Usage( "guida" )]
		[Description( "Apre la guida al gioco in italiano." )]
		private static void Guide_OnCommand( CommandEventArgs e )
		{
			if ( e.Mobile.AccessLevel >= AccessLevel.GameMaster && e.ArgString.Trim().ToLower() == "ricarica" )
			{
				Load();
				e.Mobile.SendMessage( "Guida ricaricata: {0} argomenti.", Topics.Count );
				return;
			}

			e.Mobile.CloseGump( typeof( GuideGump ) );
			e.Mobile.SendGump( new GuideGump( 0 ) );
		}

		private static void OnLogin( LoginEventArgs e )
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			// ai nuovi personaggi, al primo accesso dopo l'introduzione, ricorda la guida
			if ( pm != null && BeginnerProtection.IsActive( pm ) && pm.GameTime < TimeSpan.FromHours( 2.0 ) )
				Timer.DelayCall( TimeSpan.FromSeconds( 15.0 ), new TimerStateCallback( Remind ), pm );
		}

		private static void Remind( object state )
		{
			Mobile m = state as Mobile;
			if ( m != null && m.NetState != null )
				m.SendMessage( 0x59, "Sei nuovo da queste parti? Digita [guida per aprire la guida al gioco." );
		}
	}

	public class GuideGump : Gump
	{
		private const int Width = 640;
		private const int Height = 480;
		private const int PerPage = 16;

		private int m_Topic;

		public GuideGump( int topic ) : base( 40, 40 )
		{
			List<GuideSystem.Topic> topics = GuideSystem.Topics;

			if ( topic < 0 || topic >= topics.Count )
				topic = 0;

			m_Topic = topic;

			AddPage( 0 );
			AddBackground( 0, 0, Width, Height, 9270 );
			AddAlphaRegion( 12, 12, Width - 24, Height - 24 );

			AddHtml( 20, 18, Width - 40, 22, "<BASEFONT COLOR=#FFD700><CENTER>GUIDA DI ULTIMA ADVENTURES</CENTER></BASEFONT>", false, false );

			// indice a sinistra
			int y = 50;
			for ( int i = 0; i < topics.Count && i < 26; ++i )
			{
				string color = ( i == topic ) ? "#FFD700" : "#FFFFFF";
				AddButton( 20, y, ( i == topic ) ? 4006 : 4005, 4007, i + 1, GumpButtonType.Reply, 0 );
				AddHtml( 55, y + 2, 150, 20, String.Format( "<BASEFONT COLOR={0}>{1}</BASEFONT>", color, topics[i].Title ), false, false );
				y += 26;
			}

			// testo a destra
			GuideSystem.Topic t = topics[topic];
			AddHtml( 215, 50, Width - 235, 22, String.Format( "<BASEFONT COLOR=#FFD700><B>{0}</B></BASEFONT>", t.Title ), false, false );
			AddHtml( 215, 75, Width - 235, Height - 135, "<BASEFONT COLOR=#FFFFFF>" + t.Body + "</BASEFONT>", false, true );

			AddHtml( 215, Height - 50, Width - 235, 20, "<BASEFONT COLOR=#AAAAAA>Il manuale completo è nella cartella Documentazione del gioco.</BASEFONT>", false, false );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( info.ButtonID <= 0 )
				return;

			sender.Mobile.SendGump( new GuideGump( info.ButtonID - 1 ) );
		}
	}
}
