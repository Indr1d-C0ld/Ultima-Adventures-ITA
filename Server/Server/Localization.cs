/***************************************************************************
 *  Localization.cs - traduzione a runtime dei testi inviati al client
 *
 *  Il server continua a "pensare" in inglese: nomi, confronti e salvataggi
 *  restano invariati. Solo i testi che escono verso il client (messaggi,
 *  gump, proprieta', libri, menu, nomi) passano da Translate().
 *
 *  Dati: Data/Localization/<lingua>/*.tsv, codifica UTF-8, una voce per riga:
 *      testo inglese <TAB> traduzione
 *  Sequenze di escape: \n \t \\ .  Righe vuote o che iniziano con # ignorate.
 *  Le chiavi che contengono segnaposto {0}, {1}... sono "modelli": il testo
 *  in uscita viene confrontato col modello, i valori catturati sono tradotti
 *  a loro volta (es. nomi di oggetti) e reinseriti nella traduzione, che puo'
 *  riordinarli liberamente.
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Server
{
	public static class Localization
	{
		private static bool m_Enabled;
		private static string m_Language = "it";

		private static Dictionary<string, string> m_Exact = new Dictionary<string, string>( StringComparer.Ordinal );
		private static Dictionary<string, string> m_ExactLower = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

		// modelli indicizzati per i primi caratteri del prefisso letterale
		private const int IndexLen = 4;
		private static Dictionary<string, List<Template>> m_ByPrefix = new Dictionary<string, List<Template>>( StringComparer.Ordinal );
		private static Dictionary<string, List<Template>> m_BySuffix = new Dictionary<string, List<Template>>( StringComparer.Ordinal );
		private static List<Template> m_Loose = new List<Template>();

		private static Dictionary<string, string> m_Cache = new Dictionary<string, string>( StringComparer.Ordinal );
		private static object m_Lock = new object();
		private const int CacheMax = 200000;

		private static bool m_LogMisses;
		private static HashSet<string> m_Misses = new HashSet<string>( StringComparer.Ordinal );

		[ThreadStatic]
		private static int m_SuppressDepth;

		[ThreadStatic]
		private static int m_Depth;

		public static bool Enabled { get { return m_Enabled && m_SuppressDepth == 0; } }
		public static string Language { get { return m_Language; } }
		public static int ExactCount { get { return m_Exact.Count; } }
		public static int TemplateCount { get; private set; }

		/// <summary>Disattiva la traduzione sul thread corrente (es. parlato dei giocatori).</summary>
		public static void PushSuppress() { m_SuppressDepth++; }
		public static void PopSuppress() { if ( m_SuppressDepth > 0 ) m_SuppressDepth--; }

		private sealed class Template
		{
			public string Key;
			public string Value;
			public string Prefix;
			public string Suffix;
			public Regex Pattern;
			public int[] Order; // indice del segnaposto per ogni gruppo catturato
			public HashSet<int> SuffixSlots; // segnaposto attaccati a una parola inglese (es. "ingot{1}" = suffisso plurale)
			public int Literal; // lunghezza del testo letterale (per preferire i modelli piu' specifici)
		}

		private static readonly Regex m_EnglishSuffix = new Regex( @"^(?:s|es|ies|y|'s|')?([\s.,;:!?]*)$", RegexOptions.Compiled );

		private static readonly Regex m_Placeholder = new Regex( @"\{(\d+)\}", RegexOptions.Compiled );

		public static void Configure()
		{
			// Data/Localization/config.txt:  lingua=it  |  registra-mancanti=si
			string cfg = Path.Combine( Core.BaseDirectory, Path.Combine( "Data", Path.Combine( "Localization", "config.txt" ) ) );

			if ( File.Exists( cfg ) )
			{
				foreach ( string raw in File.ReadAllLines( cfg ) )
				{
					string l = raw.Trim();

					if ( l.Length == 0 || l[0] == '#' || l.IndexOf( '=' ) < 0 )
						continue;

					string k = l.Substring( 0, l.IndexOf( '=' ) ).Trim().ToLowerInvariant();
					string v = l.Substring( l.IndexOf( '=' ) + 1 ).Trim().ToLowerInvariant();

					if ( k == "lingua" || k == "language" )
						m_Language = v;
					else if ( k == "registra-mancanti" || k == "log-missing" )
						m_LogMisses = ( v == "si" || v == "sì" || v == "yes" || v == "true" || v == "1" );
				}
			}

			string lang = Environment.GetEnvironmentVariable( "UA_LANG" );

			if ( lang != null )
				m_Language = lang.Trim().ToLowerInvariant();

			if ( Environment.GetEnvironmentVariable( "UA_LANG_MISSES" ) == "1" )
				m_LogMisses = true;

			Load();
		}

		public static void Load()
		{
			lock ( m_Lock )
			{
				m_Exact.Clear();
				m_ExactLower.Clear();
				m_ByPrefix.Clear();
				m_BySuffix.Clear();
				m_Loose.Clear();
				m_Cache.Clear();
				m_SpeechKeys = null;
				TemplateCount = 0;
				m_Enabled = false;

				if ( m_Language == "en" || m_Language == "off" || m_Language == "" )
				{
					Console.WriteLine( "Localization: disattivata (inglese)" );
					return;
				}

				string dir = Path.Combine( Core.BaseDirectory, Path.Combine( "Data", Path.Combine( "Localization", m_Language ) ) );

				if ( !Directory.Exists( dir ) )
				{
					Console.WriteLine( "Localization: cartella {0} assente, testi in inglese", dir );
					return;
				}

				string[] files = Directory.GetFiles( dir, "*.tsv" );
				Array.Sort( files, StringComparer.Ordinal ); // i file successivi sovrascrivono i precedenti

				List<Template> templates = new List<Template>();
				Dictionary<string, Template> byKey = new Dictionary<string, Template>( StringComparer.Ordinal );

				foreach ( string file in files )
				{
					int line = 0;

					foreach ( string raw in File.ReadAllLines( file, Encoding.UTF8 ) )
					{
						line++;

						if ( raw.Length == 0 || raw[0] == '#' )
							continue;

						int tab = IndexOfSeparator( raw );

						if ( tab <= 0 )
							continue;

						string key = Unescape( raw.Substring( 0, tab ) );
						string val = Unescape( raw.Substring( tab + 1 ) );

						if ( val.Length == 0 || key == val )
							continue;

						if ( m_Placeholder.IsMatch( key ) )
						{
							Template t = BuildTemplate( key, val );

							if ( t == null )
							{
								Console.WriteLine( "Localization: modello non valido {0}:{1}", Path.GetFileName( file ), line );
								continue;
							}

							byKey[key] = t;
						}
						else
						{
							m_Exact[key] = val;
							m_ExactLower[key] = val;
						}
					}
				}

				foreach ( Template t in byKey.Values )
				{
					templates.Add( t );

					if ( t.Prefix.Length >= IndexLen )
						AddTo( m_ByPrefix, t.Prefix.Substring( 0, IndexLen ), t );
					else if ( t.Suffix.Length >= IndexLen )
						AddTo( m_BySuffix, t.Suffix.Substring( t.Suffix.Length - IndexLen ), t );
					else
						m_Loose.Add( t );
				}

				// i modelli con piu' testo letterale vincono sui piu' generici
				Comparison<Template> cmp = delegate( Template a, Template b ) { return b.Literal.CompareTo( a.Literal ); };

				foreach ( List<Template> l in m_ByPrefix.Values ) l.Sort( cmp );
				foreach ( List<Template> l in m_BySuffix.Values ) l.Sort( cmp );
				m_Loose.Sort( cmp );

				TemplateCount = templates.Count;
				m_Enabled = m_Exact.Count > 0 || templates.Count > 0;

				Console.WriteLine( "Localization: lingua '{0}', {1} testi, {2} modelli", m_Language, m_Exact.Count, templates.Count );
			}
		}

		private static void AddTo( Dictionary<string, List<Template>> dict, string k, Template t )
		{
			List<Template> list;

			if ( !dict.TryGetValue( k, out list ) )
				dict[k] = list = new List<Template>();

			list.Add( t );
		}

		private static int IndexOfSeparator( string raw )
		{
			for ( int i = 0; i < raw.Length; i++ )
			{
				if ( raw[i] == '\\' ) { i++; continue; }
				if ( raw[i] == '\t' ) return i;
			}

			return -1;
		}

		private static string Unescape( string s )
		{
			if ( s.IndexOf( '\\' ) < 0 )
				return s;

			StringBuilder sb = new StringBuilder( s.Length );

			for ( int i = 0; i < s.Length; i++ )
			{
				char c = s[i];

				if ( c == '\\' && i + 1 < s.Length )
				{
					char n = s[++i];

					switch ( n )
					{
						case 'n': sb.Append( '\n' ); break;
						case 'r': sb.Append( '\r' ); break;
						case 't': sb.Append( '\t' ); break;
						default: sb.Append( n ); break;
					}
				}
				else
				{
					sb.Append( c );
				}
			}

			return sb.ToString();
		}

		public static string Escape( string s )
		{
			return s.Replace( "\\", "\\\\" ).Replace( "\t", "\\t" ).Replace( "\r", "\\r" ).Replace( "\n", "\\n" );
		}

		private static Template BuildTemplate( string key, string val )
		{
			MatchCollection mc = m_Placeholder.Matches( key );

			if ( mc.Count == 0 )
				return null;

			StringBuilder rx = new StringBuilder( "^" );
			List<int> order = new List<int>();
			HashSet<int> suffixSlots = new HashSet<int>();
			int pos = 0, literal = 0;

			foreach ( Match m in mc )
			{
				string lit = key.Substring( pos, m.Index - pos );
				rx.Append( Regex.Escape( lit ) );
				literal += lit.Length;

				int idx = int.Parse( m.Groups[1].Value );

				if ( m.Index > 0 && Char.IsLetter( key[m.Index - 1] ) )
					suffixSlots.Add( idx );

				if ( order.Contains( idx ) )
				{
					rx.Append( @"\" + ( order.IndexOf( idx ) + 1 ) ); // stesso valore ripetuto
				}
				else
				{
					rx.Append( "(.*?)" );
					order.Add( idx );
				}

				pos = m.Index + m.Length;
			}

			string tail = key.Substring( pos );
			rx.Append( Regex.Escape( tail ) );
			rx.Append( "$" );
			literal += tail.Length;

			if ( literal < 2 )
				return null; // "{0}" o "{0} {1}" catturerebbero qualsiasi cosa

			// la traduzione non puo' usare segnaposto assenti nell'originale
			foreach ( Match m in m_Placeholder.Matches( val ) )
				if ( !order.Contains( int.Parse( m.Groups[1].Value ) ) )
					return null;

			Template t = new Template();
			t.Key = key;
			t.Value = val;
			t.Prefix = key.Substring( 0, mc[0].Index );
			t.Suffix = tail;
			t.Order = order.ToArray();
			t.SuffixSlots = suffixSlots;
			t.Literal = literal;
			t.Pattern = new Regex( rx.ToString(), RegexOptions.Singleline | RegexOptions.CultureInvariant );
			return t;
		}

		/// <summary>Traduce un testo destinato al client. Restituisce l'originale se non c'e' traduzione.</summary>
		// ---- parole chiave del parlato (Data/Localization/<lingua>/parole-chiave.txt) ----
		private static List<KeyValuePair<Regex, int>> m_SpeechKeys;

		private static void LoadSpeechKeys()
		{
			List<KeyValuePair<Regex, int>> list = new List<KeyValuePair<Regex, int>>();
			string file = Path.Combine( Core.BaseDirectory, Path.Combine( "Data", Path.Combine( "Localization", Path.Combine( m_Language, "parole-chiave.txt" ) ) ) );

			if ( File.Exists( file ) )
			{
				foreach ( string raw in File.ReadAllLines( file, Encoding.UTF8 ) )
				{
					if ( raw.Length == 0 || raw[0] == '#' )
						continue;

					string[] c = raw.Split( '\t' );
					if ( c.Length < 2 )
						continue;

					try
					{
						int id = Convert.ToInt32( c[1].Trim(), 16 );
						string[] parts = c[0].Trim().Split( '*' );
						for ( int i = 0; i < parts.Length; ++i )
							parts[i] = Regex.Escape( parts[i] );
						list.Add( new KeyValuePair<Regex, int>( new Regex( "^" + String.Join( ".*", parts ) + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled ), id ) );
					}
					catch { }
				}
			}

			m_SpeechKeys = list;
		}

		/// <summary>Aggiunge ai codici inviati dal client quelli delle parole chiave dette nella lingua del server.</summary>
		public static int[] AddSpeechKeywords( string text, int[] keywords )
		{
			if ( !m_Enabled || text == null )
				return keywords;

			if ( m_SpeechKeys == null )
				LoadSpeechKeys();

			if ( m_SpeechKeys.Count == 0 )
				return keywords;

			List<int> result = null;

			foreach ( KeyValuePair<Regex, int> kv in m_SpeechKeys )
			{
				if ( !kv.Key.IsMatch( text ) )
					continue;

				if ( result == null )
					result = new List<int>( keywords );

				if ( !result.Contains( kv.Value ) )
					result.Add( kv.Value );
			}

			return result == null ? keywords : result.ToArray();
		}

		public static string Translate( string text )
		{
			if ( !Enabled || string.IsNullOrEmpty( text ) )
				return text;

			string result;

			lock ( m_Lock )
			{
				if ( m_Cache.TryGetValue( text, out result ) )
					return result;
			}

			if ( m_Depth > 4 )
				return text;

			m_Depth++;

			try
			{
				result = Lookup( text );
			}
			finally
			{
				m_Depth--;
			}

			lock ( m_Lock )
			{
				if ( m_Cache.Count >= CacheMax )
					m_Cache.Clear();

				m_Cache[text] = result;

				if ( m_LogMisses && (object) result == (object) text && LooksLikeText( text ) && m_Misses.Add( text ) )
					LogMiss( text );
			}

			return result;
		}

		private static string Lookup( string text )
		{
			string val;

			if ( m_Exact.TryGetValue( text, out val ) )
				return val;

			// spazi ai bordi: traduci il contenuto e conserva gli spazi
			string trimmed = text.Trim();

			if ( trimmed.Length != text.Length && trimmed.Length > 0 )
			{
				string inner = Lookup( trimmed );

				if ( (object) inner != (object) trimmed )
				{
					int lead = text.IndexOf( trimmed, StringComparison.Ordinal );
					return text.Substring( 0, lead ) + inner + text.Substring( lead + trimmed.Length );
				}
			}

			string tv = MatchTemplates( text );

			if ( tv != null )
				return tv;

			// stessa frase con maiuscole diverse ("A dagger" / "a dagger")
			if ( m_ExactLower.TryGetValue( text, out val ) )
				return MatchCase( text, val );

			// HTML dei gump: traduci i tratti di testo fra un tag e l'altro
			if ( text.IndexOf( '<' ) >= 0 && text.IndexOf( '>' ) > 0 )
			{
				string r = TranslateHtmlSegments( text );

				if ( r != null )
					return r;
			}

			// testi su piu' righe composti da frasi note
			if ( text.IndexOf( '\n' ) >= 0 )
			{
				string[] parts = text.Split( '\n' );
				bool any = false;

				for ( int i = 0; i < parts.Length; i++ )
				{
					string p = Translate( parts[i] );

					if ( (object) p != (object) parts[i] )
					{
						parts[i] = p;
						any = true;
					}
				}

				if ( any )
					return string.Join( "\n", parts );
			}

			return text;
		}

		private static readonly Regex m_Tag = new Regex( @"<[^<>]*>", RegexOptions.Compiled );

		private static string TranslateHtmlSegments( string text )
		{
			MatchCollection tags = m_Tag.Matches( text );

			if ( tags.Count == 0 )
				return null;

			StringBuilder sb = new StringBuilder( text.Length + 16 );
			bool any = false;
			int pos = 0;

			foreach ( Match t in tags )
			{
				any |= AppendSegment( sb, text.Substring( pos, t.Index - pos ) );
				sb.Append( t.Value );
				pos = t.Index + t.Length;
			}

			any |= AppendSegment( sb, text.Substring( pos ) );

			return any ? sb.ToString() : null;
		}

		private static bool AppendSegment( StringBuilder sb, string seg )
		{
			if ( seg.Length == 0 )
				return false;

			string tr = Translate( seg );
			sb.Append( tr );
			return (object) tr != (object) seg;
		}

		private static string MatchCase( string original, string translated )
		{
			if ( original.Length > 0 && translated.Length > 0 && char.IsUpper( original[0] ) && char.IsLower( translated[0] ) )
				return char.ToUpper( translated[0] ) + translated.Substring( 1 );

			return translated;
		}

		private static string MatchTemplates( string text )
		{
			List<Template> list;

			if ( text.Length >= IndexLen )
			{
				if ( m_ByPrefix.TryGetValue( text.Substring( 0, IndexLen ), out list ) )
				{
					string r = TryList( list, text );
					if ( r != null ) return r;
				}

				if ( m_BySuffix.TryGetValue( text.Substring( text.Length - IndexLen ), out list ) )
				{
					string r = TryList( list, text );
					if ( r != null ) return r;
				}
			}

			return TryList( m_Loose, text );
		}

		private static string TryList( List<Template> list, string text )
		{
			for ( int i = 0; i < list.Count; i++ )
			{
				Template t = list[i];

				if ( !text.StartsWith( t.Prefix, StringComparison.Ordinal ) || !text.EndsWith( t.Suffix, StringComparison.Ordinal ) )
					continue;

				if ( text.Length < t.Literal )
					continue;

				Match m = t.Pattern.Match( text );

				if ( !m.Success )
					continue;

				string[] values = new string[ MaxIndex( t.Order ) + 1 ];

				for ( int g = 0; g < t.Order.Length; g++ )
				{
					string v = m.Groups[g + 1].Value;

					// suffisso plurale inglese incollato a una parola ("ingot" + "s."): in italiano resta solo la punteggiatura
					if ( t.SuffixSlots.Contains( t.Order[g] ) )
					{
						Match sm = m_EnglishSuffix.Match( v );
						if ( sm.Success )
						{
							values[ t.Order[g] ] = sm.Groups[1].Value;
							continue;
						}
					}

					values[ t.Order[g] ] = Translate( v );
				}

				return m_Placeholder.Replace( t.Value, delegate( Match pm )
				{
					int idx = int.Parse( pm.Groups[1].Value );
					return idx < values.Length && values[idx] != null ? values[idx] : pm.Value;
				} );
			}

			return null;
		}

		private static int MaxIndex( int[] a )
		{
			int max = 0;
			for ( int i = 0; i < a.Length; i++ ) if ( a[i] > max ) max = a[i];
			return max;
		}

		/// <summary>Argomenti di cliloc separati da TAB: traduce ogni parte che non sia un riferimento #numero.</summary>
		public static string TranslateArgs( string args )
		{
			if ( !Enabled || string.IsNullOrEmpty( args ) )
				return args;

			if ( args.IndexOf( '\t' ) < 0 )
				return args[0] == '#' ? args : Translate( args );

			string[] parts = args.Split( '\t' );

			for ( int i = 0; i < parts.Length; i++ )
				if ( parts[i].Length > 0 && parts[i][0] != '#' )
					parts[i] = Translate( parts[i] );

			return string.Join( "\t", parts );
		}

		/// <summary>Per i campi che il protocollo vuole ASCII: toglie gli accenti invece di mostrare '?'.</summary>
		public static string ToAscii( string s )
		{
			if ( s == null )
				return s;

			bool ascii = true;

			for ( int i = 0; i < s.Length && ascii; i++ )
				if ( s[i] > 127 ) ascii = false;

			if ( ascii )
				return s;

			string d = s.Normalize( NormalizationForm.FormD );
			StringBuilder sb = new StringBuilder( d.Length );

			foreach ( char c in d )
			{
				if ( c < 128 )
					sb.Append( c );
				else if ( System.Globalization.CharUnicodeInfo.GetUnicodeCategory( c ) != System.Globalization.UnicodeCategory.NonSpacingMark )
					sb.Append( c == '’' ? '\'' : '?' );
			}

			return sb.ToString();
		}

		public static bool IsAscii( string s )
		{
			if ( s == null )
				return true;

			for ( int i = 0; i < s.Length; i++ )
				if ( s[i] > 127 )
					return false;

			return true;
		}

		private static bool LooksLikeText( string s )
		{
			int letters = 0;

			foreach ( char c in s )
				if ( char.IsLetter( c ) ) letters++;

			return letters >= 3;
		}

		private static void LogMiss( string text )
		{
			try
			{
				string path = Path.Combine( Core.BaseDirectory, Path.Combine( "Logs", "localization-missing.tsv" ) );
				File.AppendAllText( path, Escape( text ) + "\t\n", Encoding.UTF8 );
			}
			catch
			{
			}
		}
	}
}
