using System;
using System.Collections.Generic;
using System.Linq;
using Server.Gumps;

namespace Server
{
	public class DamageTypeProfile
	{
		public int Physical;
		public int Fire;
		public int Cold;
		public int Poison;
		public int Energy;

		public DamageTypeProfile( int physical, int fire, int cold, int poison, int energy )
		{
			Physical = Math.Max( 0, physical );
			Fire = Math.Max( 0, fire );
			Cold = Math.Max( 0, cold );
			Poison = Math.Max( 0, poison );
			Energy = Math.Max( 0, energy );
		}

		public string GetPrimaryDamageType()
		{
			int max = Physical;
			string type = "Physical";

			if( Fire > max )
			{
				max = Fire;
				type = "Fire";
			}

			if( Cold > max )
			{
				max = Cold;
				type = "Cold";
			}

			if( Poison > max )
			{
				max = Poison;
				type = "Poison";
			}

			if( Energy > max )
			{
				max = Energy;
				type = "Energy";
			}

			if( max <= 0 )
				type = "Unknown";

			return type;
		}
	}

	public class DeathDamageEntry
	{
		public DateTime Timestamp;
		public int Amount;
		public Mobile Source;
		public string SourceName;
		public string DamageType;
	}

	public class DeathReportData
	{
		public DeathDamageEntry KillerEntry;
		public int TotalDamage;
		public string PrimaryDamageType;
		public string PrimarySource;
		public TimeSpan Duration;
		public List<DeathDamageEntry> Entries;
	}

	public static class DeathReportSystem
	{
		private static readonly object m_SyncRoot = new object();
		private static readonly Dictionary<Mobile, DamageTypeProfile> m_PendingDamageTypes = new Dictionary<Mobile, DamageTypeProfile>();
		private static readonly Dictionary<int, List<DeathDamageEntry>> m_PlayerDamage = new Dictionary<int, List<DeathDamageEntry>>();
		private static readonly HashSet<int> m_EnabledPlayers = new HashSet<int>();

		private static readonly TimeSpan m_CaptureWindow = TimeSpan.FromSeconds( 5.0 );

		public static bool Toggle( Mobile pm )
		{
			if( pm == null || !pm.Player )
				return false;

			lock( m_SyncRoot )
			{
				if( m_EnabledPlayers.Contains( pm.Serial.Value ) )
				{
					m_EnabledPlayers.Remove( pm.Serial.Value );
					return false;
				}

				m_EnabledPlayers.Add( pm.Serial.Value );
				return true;
			}
		}

		public static bool IsEnabled( Mobile pm )
		{
			if( pm == null || !pm.Player )
				return false;

			lock( m_SyncRoot )
				return m_EnabledPlayers.Contains( pm.Serial.Value );
		}

		public static void RegisterPendingDamageType( Mobile target, DamageTypeProfile profile )
		{
			if( target == null || profile == null )
				return;

			lock( m_SyncRoot )
				m_PendingDamageTypes[target] = profile;
		}

		public static void RecordIncomingDamage( Mobile target, Mobile from, int amount )
		{
			if( target == null || !target.Player || amount <= 0 )
				return;

			DamageTypeProfile profile = null;

			lock( m_SyncRoot )
			{
				if( m_PendingDamageTypes.ContainsKey( target ) )
				{
					profile = m_PendingDamageTypes[target];
					m_PendingDamageTypes.Remove( target );
				}

				int key = target.Serial.Value;
				List<DeathDamageEntry> list;

				if( !m_PlayerDamage.TryGetValue( key, out list ) )
				{
					list = new List<DeathDamageEntry>();
					m_PlayerDamage[key] = list;
				}

				DateTime cutoff = DateTime.UtcNow - m_CaptureWindow;

				for( int i = list.Count - 1; i >= 0; --i )
				{
					if( list[i].Timestamp < cutoff )
						list.RemoveAt( i );
				}

				list.Add( new DeathDamageEntry
				{
					Timestamp = DateTime.UtcNow,
					Amount = amount,
					Source = from,
					SourceName = GetSourceName( from ),
					DamageType = (profile == null ? "Unknown" : profile.GetPrimaryDamageType())
				} );
			}
		}

		public static void TryDisplayDeathReport( Mobile pm, Mobile killer )
		{
			if( pm == null || !pm.Player || !IsEnabled( pm ) || pm.NetState == null )
				return;

			DeathReportData data = BuildReport( pm, killer );

			if( data == null )
				return;

			pm.CloseGump( typeof( DeathReportGump ) );
			pm.SendGump( new DeathReportGump( pm, data ) );
		}

		public static void SetEnabled( Mobile pm, bool enabled )
		{
			if( pm == null || !pm.Player )
				return;

			lock( m_SyncRoot )
			{
				if( enabled )
					m_EnabledPlayers.Add( pm.Serial.Value );
				else
					m_EnabledPlayers.Remove( pm.Serial.Value );
			}
		}

		private static DeathReportData BuildReport( Mobile pm, Mobile killer )
		{
			lock( m_SyncRoot )
			{
				int key = pm.Serial.Value;
				List<DeathDamageEntry> list;

				if( !m_PlayerDamage.TryGetValue( key, out list ) || list.Count == 0 )
					return null;

				DateTime cutoff = DateTime.UtcNow - m_CaptureWindow;

				List<DeathDamageEntry> relevant = list.Where( e => e.Timestamp >= cutoff ).ToList();

				if( relevant.Count == 0 )
					relevant = new List<DeathDamageEntry>( list );

				if( relevant.Count == 0 )
					return null;

				DeathDamageEntry killerEntry = relevant[relevant.Count - 1];

				if( killer != null )
				{
					for( int i = relevant.Count - 1; i >= 0; --i )
					{
						if( relevant[i].Source == killer )
						{
							killerEntry = relevant[i];
							break;
						}
					}
				}

				int totalDamage = 0;
				Dictionary<string, int> typeTotals = new Dictionary<string, int>( StringComparer.OrdinalIgnoreCase );
				Dictionary<string, int> sourceTotals = new Dictionary<string, int>( StringComparer.OrdinalIgnoreCase );

				for( int i = 0; i < relevant.Count; ++i )
				{
					DeathDamageEntry entry = relevant[i];
					totalDamage += entry.Amount;

					if( !typeTotals.ContainsKey( entry.DamageType ) )
						typeTotals[entry.DamageType] = 0;

					typeTotals[entry.DamageType] += entry.Amount;

					if( !sourceTotals.ContainsKey( entry.SourceName ) )
						sourceTotals[entry.SourceName] = 0;

					sourceTotals[entry.SourceName] += entry.Amount;
				}

				string primaryDamageType = "Unknown";
				int maxTypeDamage = -1;

				foreach( KeyValuePair<string, int> kvp in typeTotals )
				{
					if( kvp.Value > maxTypeDamage )
					{
						maxTypeDamage = kvp.Value;
						primaryDamageType = kvp.Key;
					}
				}

				string primarySource = killerEntry.SourceName;

				if( sourceTotals.Count > 0 )
				{
					int maxSourceDamage = -1;

					foreach( KeyValuePair<string, int> kvp in sourceTotals )
					{
						if( kvp.Value > maxSourceDamage )
						{
							maxSourceDamage = kvp.Value;
							primarySource = kvp.Key;
						}
					}
				}

				TimeSpan duration = relevant[relevant.Count - 1].Timestamp - relevant[0].Timestamp;

				if( duration < TimeSpan.Zero )
					duration = TimeSpan.Zero;

				return new DeathReportData
				{
					KillerEntry = killerEntry,
					TotalDamage = totalDamage,
					PrimaryDamageType = primaryDamageType,
					PrimarySource = primarySource,
					Duration = duration,
					Entries = relevant
				};
			}
		}

		private static string GetSourceName( Mobile source )
		{
			if( source == null )
				return "Environment";

			if( !String.IsNullOrEmpty( source.Name ) )
				return source.Name;

			return source.GetType().Name;
		}
	}

	public class DeathReportGump : Gump
	{
		public DeathReportGump( Mobile pm, DeathReportData data ) : base( 120, 90 )
		{
			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 520, 420, 9270 );
			AddAlphaRegion( 15, 15, 490, 390 );

			AddLabel( 175, 18, 1161, "Death Report (5s window)" );

			AddLabel( 25, 55, 1152, "Killed by:" );
			AddHtml( 135, 55, 345, 20, Colorize( Safe( data.KillerEntry.SourceName ) ), false, false );

			AddLabel( 25, 82, 1152, "Killing blow:" );
			AddHtml( 135, 82, 345, 20, Colorize( data.KillerEntry.Amount.ToString() + " damage" ), false, false );

			AddLabel( 25, 109, 1152, "Total damage:" );
			AddHtml( 135, 109, 345, 20, Colorize( data.TotalDamage.ToString() ), false, false );

			AddLabel( 25, 136, 1152, "Primary type:" );
			AddHtml( 135, 136, 345, 20, Colorize( Safe( data.PrimaryDamageType ) ), false, false );

			AddLabel( 25, 163, 1152, "Main source:" );
			AddHtml( 135, 163, 345, 20, Colorize( Safe( data.PrimarySource ) ), false, false );

			double seconds = Math.Max( data.Duration.TotalSeconds, 0.1 );
			double dps = data.TotalDamage / seconds;

			AddLabel( 25, 190, 1152, "Time to death:" );
			AddHtml( 135, 190, 345, 20, Colorize( String.Format( "{0:0.00}s ({1:0.0} dps)", seconds, dps ) ), false, false );

			AddLabel( 25, 220, 1152, "Damage events (all in last 5 seconds):" );
			AddHtml( 25, 245, 470, 155, BuildEntriesHtml( data ), true, true );
		}

		private static string Safe( string text )
		{
			if( String.IsNullOrEmpty( text ) )
				return "Unknown";

			return text;
		}

		private static string BuildEntriesHtml( DeathReportData data )
		{
			if( data == null || data.Entries == null || data.Entries.Count == 0 )
				return "<BASEFONT COLOR=#FFFFCC>No damage events found.</BASEFONT>";

			List<DeathDamageEntry> entries = new List<DeathDamageEntry>( data.Entries );
			entries.Sort( (a, b) => a.Timestamp.CompareTo( b.Timestamp ) );

			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append( "<BASEFONT COLOR=#FFFFCC>" );

			for( int i = 0; i < entries.Count; ++i )
			{
				DeathDamageEntry e = entries[i];
				string source = HtmlSafe( Safe( e.SourceName ) );
				string type = HtmlSafe( Safe( e.DamageType ) );

				sb.AppendFormat( "{0}. {1} dmg | {2} | {3}<BR>", (i + 1), e.Amount, type, source );
			}

			sb.Append( "</BASEFONT>" );
			return sb.ToString();
		}

		private static string HtmlSafe( string text )
		{
			if( String.IsNullOrEmpty( text ) )
				return String.Empty;

			return text.Replace( "&", "&amp;" ).Replace( "<", "&lt;" ).Replace( ">", "&gt;" );
		}

		private static string Colorize( string text )
		{
			return String.Format( "<BASEFONT COLOR=#FFFFCC>{0}</BASEFONT>", HtmlSafe( text ) );
		}
	}
}
