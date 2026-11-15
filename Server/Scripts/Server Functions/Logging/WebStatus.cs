using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Server;
using Server.Network;
using Server.Guilds;
using Server.Accounting;
using Server.Diagnostics;
using Server.Mobiles;

namespace Server.Misc
{
	public class StatusPage : Timer
	{
		public static bool Enabled = true;
		public static TimeSpan SlowLogThreshold = TimeSpan.FromMilliseconds( 50.0 );
		public static TimeSpan UpdateInterval = TimeSpan.FromSeconds( 60.0 );
		private const bool UseCachedAccountSnapshot = true;
		private static readonly TimeSpan AccountSnapshotMaxAge = TimeSpan.FromMinutes( 5.0 );
		private static readonly TimeSpan AccountSnapshotGrace = TimeSpan.FromSeconds( 5.0 );
		public static long LastElapsed;
		public static long MaxElapsed;
		public static int LastOnline;
		public static int LastAccountsChecked;
		public static int LastMurderersLogged;
		public static string LastOutputPath;
		public static bool LastWriteOccurred;
		public static long LastFileWriteMs;
		public static long LastAccountScanMs;
		public static long Count;
		public static long TotalElapsed;
		public static long TotalFileWriteMs;
		public static long TotalAccountScanMs;
		public static long MaxFileWriteMs;
		public static long MaxAccountScanMs;
		public static bool LastAccountsFromCache;
		public static long LastAccountCacheAgeMs;
		public static long AccountCacheRebuilds;
		public static long AccountCacheHits;
		public static long AccountCacheMisses;
		public static long CachedRunCount;
		public static long CachedRunTotalMs;
		public static long CachedRunMaxMs;
		public static long RebuildRunCount;
		public static long RebuildRunTotalMs;
		public static long RebuildRunMaxMs;

		private static DateTime m_AccountSnapshotTime = DateTime.MinValue;
		private static DateTime m_AccountSnapshotExpires = DateTime.MinValue;
		private static int m_CachedAccountsChecked;
		private static int m_CachedMurderersLogged;

		public static void Initialize()
		{
			if ( Enabled )
				new StatusPage().Start();
		}

		public StatusPage() : base( TimeSpan.FromSeconds( 5.0 ), UpdateInterval )
		{
			Priority = TimerPriority.FiveSeconds;
		}

		private static string Encode( string input )
		{
			StringBuilder sb = new StringBuilder( input );

			sb.Replace( "&", "&amp;" );
			sb.Replace( "<", "&lt;" );
			sb.Replace( ">", "&gt;" );
			sb.Replace( "\"", "&quot;" );
			sb.Replace( "'", "&apos;" );

			return sb.ToString();
		}

		protected override void OnTick()
		{
			long start = Stopwatch.GetTimestamp();
			int online = NetState.Instances.Count;
			int accountsChecked = 0;
			int murderersLogged = 0;
			string outputPath = "Info/online.php";
			bool wrote = false;
			long phaseStart;
			bool accountsFromCache = false;
			long accountCacheAgeMs = 0;
			bool accountSnapshotUsed = false;
			bool accountSnapshotRebuilt = false;

			phaseStart = Stopwatch.GetTimestamp();

			using ( StreamWriter op = new StreamWriter( outputPath ) )
			{
				foreach ( NetState state in NetState.Instances )
				{
					Mobile m = state.Mobile;

					if ( m != null && ( m.AccessLevel < AccessLevel.GameMaster ) )
					{
						op.Write( "<br>" );
						op.Write( Encode( m.Name ) );
						op.Write( " the " );
						op.Write( GetPlayerInfo.GetSkillTitle( m ) );
					}
				}

				wrote = true;
			}

			long fileWriteMs = (Stopwatch.GetTimestamp() - phaseStart) * 1000 / Stopwatch.Frequency;
			long accountScanMs = 0;

			if ( LoggingFunctions.LoggingEvents() == true )
			{
				DateTime now = DateTime.UtcNow;
				TimeSpan cacheAge = now - m_AccountSnapshotTime;
				bool cacheFresh = ( UseCachedAccountSnapshot && m_AccountSnapshotTime != DateTime.MinValue && now <= m_AccountSnapshotExpires + AccountSnapshotGrace );

				if ( cacheFresh )
				{
					accountSnapshotUsed = true;
					accountsFromCache = true;
					++AccountCacheHits;
					accountsChecked = m_CachedAccountsChecked;
					murderersLogged = m_CachedMurderersLogged;
					accountCacheAgeMs = (long)cacheAge.TotalMilliseconds;
				}
				else
				{
					accountSnapshotUsed = true;
					accountSnapshotRebuilt = true;
					phaseStart = Stopwatch.GetTimestamp();
					LoggingFunctions.LogClear( "Logging Murderers" );

					// GET ALL OF THE MURDERERS ///////////////////////////////
					foreach ( Account a in Accounts.GetAccounts() )
					{
						if (a == null)
							break;

						++accountsChecked;

						for (int i = 0; i < a.Length; ++i)
						{
							Mobile m = a[i];

							if (m == null)
								continue;

							if ( ( m.Kills > 0 ) && (m.AccessLevel < AccessLevel.GameMaster) )
							{
								LoggingFunctions.LogKillers( m, m.Kills );
								++murderersLogged;
							}
						}
					}

					accountScanMs = (Stopwatch.GetTimestamp() - phaseStart) * 1000 / Stopwatch.Frequency;
					m_AccountSnapshotTime = now;
					m_AccountSnapshotExpires = now + AccountSnapshotMaxAge;
					m_CachedAccountsChecked = accountsChecked;
					m_CachedMurderersLogged = murderersLogged;
					accountCacheAgeMs = 0;
					++AccountCacheRebuilds;
					++AccountCacheMisses;
				}
			}

			long elapsed = (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
			LastElapsed = elapsed;
			LastOnline = online;
			LastAccountsChecked = accountsChecked;
			LastMurderersLogged = murderersLogged;
			LastOutputPath = outputPath;
			LastWriteOccurred = wrote;
			LastFileWriteMs = fileWriteMs;
			LastAccountScanMs = accountScanMs;
			LastAccountsFromCache = accountsFromCache;
			LastAccountCacheAgeMs = accountCacheAgeMs;
			++Count;
			TotalElapsed += elapsed;
			TotalFileWriteMs += fileWriteMs;
			TotalAccountScanMs += accountScanMs;

			if ( accountSnapshotUsed )
			{
				if ( accountsFromCache )
				{
					++CachedRunCount;
					CachedRunTotalMs += elapsed;

					if ( elapsed > CachedRunMaxMs )
						CachedRunMaxMs = elapsed;
				}
				else if ( accountSnapshotRebuilt )
				{
					++RebuildRunCount;
					RebuildRunTotalMs += elapsed;

					if ( elapsed > RebuildRunMaxMs )
						RebuildRunMaxMs = elapsed;
				}
			}

			if ( elapsed > MaxElapsed )
				MaxElapsed = elapsed;

			if ( fileWriteMs > MaxFileWriteMs )
				MaxFileWriteMs = fileWriteMs;

			if ( accountScanMs > MaxAccountScanMs )
				MaxAccountScanMs = accountScanMs;

			if (elapsed >= SlowLogThreshold.TotalMilliseconds)
				DiagnosticConsole.WriteParsedLine(String.Format("[Lag] StatusPage took {0}ms; write={1}ms accountScan={2}ms online={3}, accountsChecked={4}, murderers={5}, fromCache={6}, cacheAge={7}ms, wrote={8}", elapsed, fileWriteMs, accountScanMs, online, accountsChecked, murderersLogged, accountsFromCache, accountCacheAgeMs, wrote ? outputPath : "false"));
		}

		public static void WriteStats( TextWriter writer )
		{
			if ( writer == null )
				return;

			long average = Count > 0 ? TotalElapsed / Count : 0;
			long writeAverage = Count > 0 ? TotalFileWriteMs / Count : 0;
			long accountAverage = Count > 0 ? TotalAccountScanMs / Count : 0;
			long cachedAverage = CachedRunCount > 0 ? CachedRunTotalMs / CachedRunCount : 0;
			long rebuildAverage = RebuildRunCount > 0 ? RebuildRunTotalMs / RebuildRunCount : 0;

			writer.WriteLine( "StatusPage: enabled={0}, interval={1}, accountSnapshotCache={2}, snapshotMaxAge={3}, count={4}, last={5}ms, avg={6}ms, max={7}ms", Enabled, UpdateInterval, UseCachedAccountSnapshot, AccountSnapshotMaxAge, Count, LastElapsed, average, MaxElapsed );
			writer.WriteLine( "  phases: writeLast={0}ms, writeAvg={1}ms, writeMax={2}ms, accountScanLast={3}ms, accountScanAvg={4}ms, accountScanMax={5}ms", LastFileWriteMs, writeAverage, MaxFileWriteMs, LastAccountScanMs, accountAverage, MaxAccountScanMs );
			writer.WriteLine( "  accountSnapshot: accountsChecked={0}, murderers={1}, fromCache={2}, cacheAge={3}ms, cacheMaxAge={4}, cacheExpiresUtc={5}, cacheHits={6}, cacheMisses={7}, cacheRebuilds={8}, cachedRuns={9}, avgCachedRunMs={10}, maxCachedRunMs={11}, rebuildRuns={12}, avgRebuildRunMs={13}, maxRebuildRunMs={14}", LastAccountsChecked, LastMurderersLogged, LastAccountsFromCache, LastAccountCacheAgeMs, AccountSnapshotMaxAge, m_AccountSnapshotExpires, AccountCacheHits, AccountCacheMisses, AccountCacheRebuilds, CachedRunCount, cachedAverage, CachedRunMaxMs, RebuildRunCount, rebuildAverage, RebuildRunMaxMs );
			writer.WriteLine( "  output: online={0}, wrote={1}", LastOnline, LastWriteOccurred ? LastOutputPath : "false" );
		}
	}
}
