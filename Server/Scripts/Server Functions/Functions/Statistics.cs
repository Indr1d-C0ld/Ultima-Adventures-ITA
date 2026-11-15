using System;
using System.Diagnostics;
using System.Collections.Generic;
using Server.Items;
using Server.Gumps;
using Server.Guilds;
using Server.Mobiles;
using Server.Network;
using Server.Commands;
using Server.Accounting;
using Server.Engines.PartySystem;
using System.IO;
using System.Text;
using Server.Diagnostics;

namespace Server
{
    class Statistics
    {
        public class Config
        {
            public static bool Enabled = true;                            // Is this system enabled?
            public static bool ConsoleReport = false;                     // Should we report statistics on console?
            public static int Interval = 1;                               // What's the statistics update interval, in minutes?
            public static AccessLevel CanSeeStats = AccessLevel.Player;   // What's the level required to see statistics in-game?
            public static AccessLevel CanUpdateStats = AccessLevel.Seer;  // What's the level required to update statistics in-game?
        }

        public static TimeSpan ShardAge { get { return m_ShardAge; } }
        public static TimeSpan Uptime { get { return m_Uptime; } }
        public static TimeSpan TotalGameTime { get { return m_TotalGameTime; } }
        public static DateTime LastRestart { get { return m_LastRestart; } }
        public static DateTime LastStatsUpdate { get { return m_LastStatsUpdate; } }
        public static int ActiveAccounts { get { return m_ActiveAccounts; } }
        public static int ActiveStaffMembers { get { return m_ActiveStaffMembers; } }
        public static int ActiveGuilds { get { return m_ActiveGuilds; } }
        public static int ActiveParties { get { return m_ActiveParties; } }
        public static int PlayersInParty { get { return m_PlayersInParty; } }
        public static int PlayerHouses { get { return m_PlayerHouses; } }
        public static int PlayerGold { get { return m_PlayerGold; } }
        public static int PlayersOnline { get { return m_PlayersOnline; } }
        public static int StaffOnline { get { return m_StaffOnline; } }

        private static TimeSpan m_ShardAge;
        private static TimeSpan m_Uptime;
        private static TimeSpan m_TotalGameTime;
        private static DateTime m_LastRestart;
        private static DateTime m_LastStatsUpdate;
        private static int m_ActiveAccounts;
        private static int m_ActiveStaffMembers;
        private static int m_ActiveGuilds;
        private static int m_ActiveParties;
        private static int m_PlayersInParty;
        private static int m_PlayerHouses;
        private static int m_PlayerGold;
        private static int m_PlayersOnline;
        private static int m_StaffOnline;

        private static List<String> StatsList = new List<String>();
        private const long SlowCollectStatsThreshold = 50;
        private static long m_CollectCount;
        private static long m_LastCollectElapsed;
        private static long m_MaxCollectElapsed;
        private static long m_TotalCollectElapsed;
        private static long m_LastHouseScanMs;
        private static long m_LastMobileScanMs;
        private static long m_LastBankCheckScanMs;
        private static long m_LastNetStateScanMs;
        private static long m_LastAccountScanMs;
        private static long m_LastFormatMs;
        private static long m_LastItemScanMs;
        private static long m_LastItemRegistryMs;
        private static int m_LastPhysicalItemScans;
        private static int m_LastPhysicalMobileScans;
        private static int m_LastItemsScanned;
        private static int m_LastItemsScannedForHouses;
        private static int m_LastMobilesScanned;
        private static int m_LastItemsScannedForChecks;
        private static int m_LastNetStatesScanned;
        private static int m_LastAccountsScanned;
        private static int m_LastBankChecksFound;
        private static int m_LastInvestmentChecksFound;
        private static int m_LastHouseRegistryCount;
        private static int m_LastBankCheckRegistryCount;
        private static int m_LastInvestmentCheckRegistryCount;
        private static int m_LastRegistryStaleRemoved;
        private static long m_RegistryRebuildCount;
        private static long m_LastRegistryRebuildMs;
        private static int m_LastRegistryRebuildItemsScanned;
        private static DateTime m_LastRegistryRebuildTime;
        private static bool m_RegistryReady;
        private static bool m_RegistryEventsHooked;
        private static readonly object m_RegistryLock = new object();
        private static Dictionary<Serial, Multis.BaseHouse> m_HouseRegistry = new Dictionary<Serial, Multis.BaseHouse>();
        private static Dictionary<Serial, BankCheck> m_BankCheckRegistry = new Dictionary<Serial, BankCheck>();
        private static Dictionary<Serial, InvestmentCheck> m_InvestmentCheckRegistry = new Dictionary<Serial, InvestmentCheck>();

        public static void Initialize()
        {
            if (Config.Enabled)
            {
                CommandSystem.Register("Statistics", Config.CanSeeStats, new CommandEventHandler(SeeStatistics_OnCommand));
                CommandSystem.Register("UpdateStatistics", Config.CanUpdateStats, new CommandEventHandler(UpdateStatistics_OnCommand));
                HookRegistryEvents();
                Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromMinutes(Config.Interval), CollectStats);
            }
        }

        private static void HookRegistryEvents()
        {
            if (m_RegistryEventsHooked)
                return;

            World.ItemAdded += new World.ItemWorldEventHandler(OnWorldItemAdded);
            World.ItemRemoved += new World.ItemWorldEventHandler(OnWorldItemRemoved);
            m_RegistryEventsHooked = true;
        }

        private static void OnWorldItemAdded(Item item)
        {
            if (item == null)
                return;

            lock (m_RegistryLock)
                RegisterStatsItem(item);
        }

        private static void OnWorldItemRemoved(Item item)
        {
            if (item == null)
                return;

            lock (m_RegistryLock)
                UnregisterStatsItem(item);
        }

        private static void RegisterStatsItem(Item item)
        {
            Multis.BaseHouse house = item as Multis.BaseHouse;

            if (house != null)
                m_HouseRegistry[item.Serial] = house;

            BankCheck bankCheck = item as BankCheck;

            if (bankCheck != null)
                m_BankCheckRegistry[item.Serial] = bankCheck;

            InvestmentCheck investmentCheck = item as InvestmentCheck;

            if (investmentCheck != null)
                m_InvestmentCheckRegistry[item.Serial] = investmentCheck;
        }

        private static void UnregisterStatsItem(Item item)
        {
            m_HouseRegistry.Remove(item.Serial);
            m_BankCheckRegistry.Remove(item.Serial);
            m_InvestmentCheckRegistry.Remove(item.Serial);
        }

        private static void EnsureStatsRegistry()
        {
            HookRegistryEvents();

            if (!m_RegistryReady)
                RebuildStatsRegistry();
        }

        public static void RebuildStatsRegistry()
        {
            Stopwatch watch = Stopwatch.StartNew();
            int itemsScanned = 0;
            Dictionary<Serial, Multis.BaseHouse> houses = new Dictionary<Serial, Multis.BaseHouse>();
            Dictionary<Serial, BankCheck> bankChecks = new Dictionary<Serial, BankCheck>();
            Dictionary<Serial, InvestmentCheck> investmentChecks = new Dictionary<Serial, InvestmentCheck>();

            foreach (Item item in World.Items.Values)
            {
                ++itemsScanned;

                if (item == null || item.Deleted)
                    continue;

                Multis.BaseHouse house = item as Multis.BaseHouse;

                if (house != null)
                    houses[item.Serial] = house;

                BankCheck bankCheck = item as BankCheck;

                if (bankCheck != null)
                    bankChecks[item.Serial] = bankCheck;

                InvestmentCheck investmentCheck = item as InvestmentCheck;

                if (investmentCheck != null)
                    investmentChecks[item.Serial] = investmentCheck;
            }

            watch.Stop();

            lock (m_RegistryLock)
            {
                m_HouseRegistry = houses;
                m_BankCheckRegistry = bankChecks;
                m_InvestmentCheckRegistry = investmentChecks;
                m_RegistryReady = true;
                ++m_RegistryRebuildCount;
                m_LastRegistryRebuildMs = watch.ElapsedMilliseconds;
                m_LastRegistryRebuildItemsScanned = itemsScanned;
                m_LastRegistryRebuildTime = DateTime.UtcNow;
            }
        }

        private static Multis.BaseHouse[] GetHouseSnapshot()
        {
            lock (m_RegistryLock)
            {
                Multis.BaseHouse[] snapshot = new Multis.BaseHouse[m_HouseRegistry.Count];
                m_HouseRegistry.Values.CopyTo(snapshot, 0);
                return snapshot;
            }
        }

        private static BankCheck[] GetBankCheckSnapshot()
        {
            lock (m_RegistryLock)
            {
                BankCheck[] snapshot = new BankCheck[m_BankCheckRegistry.Count];
                m_BankCheckRegistry.Values.CopyTo(snapshot, 0);
                return snapshot;
            }
        }

        private static InvestmentCheck[] GetInvestmentCheckSnapshot()
        {
            lock (m_RegistryLock)
            {
                InvestmentCheck[] snapshot = new InvestmentCheck[m_InvestmentCheckRegistry.Count];
                m_InvestmentCheckRegistry.Values.CopyTo(snapshot, 0);
                return snapshot;
            }
        }

        private static int PruneStatsRegistry()
        {
            int removed = 0;

            lock (m_RegistryLock)
            {
                removed += PruneDeletedHouses();
                removed += PruneDeletedBankChecks();
                removed += PruneDeletedInvestmentChecks();
            }

            return removed;
        }

        private static int PruneDeletedHouses()
        {
            List<Serial> remove = null;

            foreach (KeyValuePair<Serial, Multis.BaseHouse> kvp in m_HouseRegistry)
            {
                if (kvp.Value == null || kvp.Value.Deleted)
                {
                    if (remove == null)
                        remove = new List<Serial>();

                    remove.Add(kvp.Key);
                }
            }

            if (remove == null)
                return 0;

            for (int i = 0; i < remove.Count; ++i)
                m_HouseRegistry.Remove(remove[i]);

            return remove.Count;
        }

        private static int PruneDeletedBankChecks()
        {
            List<Serial> remove = null;

            foreach (KeyValuePair<Serial, BankCheck> kvp in m_BankCheckRegistry)
            {
                if (kvp.Value == null || kvp.Value.Deleted)
                {
                    if (remove == null)
                        remove = new List<Serial>();

                    remove.Add(kvp.Key);
                }
            }

            if (remove == null)
                return 0;

            for (int i = 0; i < remove.Count; ++i)
                m_BankCheckRegistry.Remove(remove[i]);

            return remove.Count;
        }

        private static int PruneDeletedInvestmentChecks()
        {
            List<Serial> remove = null;

            foreach (KeyValuePair<Serial, InvestmentCheck> kvp in m_InvestmentCheckRegistry)
            {
                if (kvp.Value == null || kvp.Value.Deleted)
                {
                    if (remove == null)
                        remove = new List<Serial>();

                    remove.Add(kvp.Key);
                }
            }

            if (remove == null)
                return 0;

            for (int i = 0; i < remove.Count; ++i)
                m_InvestmentCheckRegistry.Remove(remove[i]);

            return remove.Count;
        }

        [Usage("Statistics")]
        [Description("Shows shard statistics.")]
        private static void SeeStatistics_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;
            m.CloseGump(typeof(StatisticsGump));
            m.SendGump(new StatisticsGump( 0 ));
        }

        [Usage("UpdateStatistics")]
        [Description("Updates shard statistics.")]
        private static void UpdateStatistics_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;
            Console.WriteLine("Statistics: {0} ({1}) has updated statistics in-game.", m.RawName, m.AccessLevel.ToString());
            CollectStats();
            m.SendMessage(68, "Statistics updated successful.");
            m.CloseGump(typeof(StatisticsGump));
            m.SendGump(new StatisticsGump( 0 ));
        }

        private static void CollectStats()
        {
            Stopwatch watch = Stopwatch.StartNew();
            long phaseStart;
            int itemsScanned = 0;
            int registryEntriesScanned = 0;
            int mobilesScanned = 0;
            int netStatesScanned = 0;
            int accountsScanned = 0;
            int bankChecksFound = 0;
            int investmentChecksFound = 0;
            int staleRegistryEntries = 0;
            int staleRemoved = 0;
            int houseRegistryCount = 0;
            int bankCheckRegistryCount = 0;
            int investmentCheckRegistryCount = 0;

            m_StaffOnline = 0;
            m_PlayersOnline = 0;
            m_PlayersInParty = 0;
            m_PlayerHouses = 0;
            m_PlayerGold = 0;
            m_ActiveStaffMembers = 0;
            m_TotalGameTime = TimeSpan.Zero;
            StatsList.Clear();

            List<Party> parties = new List<Party>();
            DateTime shardCreation = DateTime.UtcNow;

            EnsureStatsRegistry();

            phaseStart = Stopwatch.GetTimestamp();
            Multis.BaseHouse[] houses = GetHouseSnapshot();
            BankCheck[] bankChecks = GetBankCheckSnapshot();
            InvestmentCheck[] investmentChecks = GetInvestmentCheckSnapshot();

            houseRegistryCount = houses.Length;
            bankCheckRegistryCount = bankChecks.Length;
            investmentCheckRegistryCount = investmentChecks.Length;

            for (int i = 0; i < houses.Length; ++i)
            {
                Multis.BaseHouse house = houses[i];
                ++registryEntriesScanned;

                if (house == null || house.Deleted)
                {
                    ++staleRegistryEntries;
                    continue;
                }

                m_PlayerHouses++;
            }

            for (int i = 0; i < bankChecks.Length; ++i)
            {
                BankCheck bankCheck = bankChecks[i];
                ++registryEntriesScanned;

                if (bankCheck == null || bankCheck.Deleted)
                {
                    ++staleRegistryEntries;
                    continue;
                }

                if (bankCheck != null && !(bankCheck.ParentEntity is BankBox) && !(bankCheck.RootParentEntity is PlayerMobile))
                {
                    m_PlayerGold += bankCheck.Amount;
                    ++bankChecksFound;
                }
            }

            for (int i = 0; i < investmentChecks.Length; ++i)
            {
                InvestmentCheck investmentCheck = investmentChecks[i];
                ++registryEntriesScanned;

                if (investmentCheck == null || investmentCheck.Deleted)
                {
                    ++staleRegistryEntries;
                    continue;
                }

                if (investmentCheck != null)
                {
                    m_PlayerGold += investmentCheck.Amount;
                    ++investmentChecksFound;
                }
            }
            long itemRegistryMs = (Stopwatch.GetTimestamp() - phaseStart) * 1000 / Stopwatch.Frequency;
            long itemScanMs = 0;

            if (staleRegistryEntries > 0)
                staleRemoved = PruneStatsRegistry();

            phaseStart = Stopwatch.GetTimestamp();
            foreach (Mobile m in World.Mobiles.Values)
            {
                ++mobilesScanned;

                if (m is PlayerMobile)
                {
                    if (m.AccessLevel == AccessLevel.Player)
                    {
                        m_PlayerGold += m.TotalGold + Banker.GetBalance( m );
                    }
                    else
                    {
                        m_ActiveStaffMembers++;
                    }
                }
            }
            long mobileScanMs = (Stopwatch.GetTimestamp() - phaseStart) * 1000 / Stopwatch.Frequency;
            long bankCheckScanMs = 0;
            long houseScanMs = itemRegistryMs;

            phaseStart = Stopwatch.GetTimestamp();
            foreach (NetState ns in NetState.Instances)
            {
                ++netStatesScanned;

                Mobile m = ns.Mobile;

                if (m != null)
                {
                    if (m.AccessLevel == AccessLevel.Player)
                    {
                        m_PlayersOnline++;
                    }
                    else
                    {
                        m_StaffOnline++;
                    }

                    Party p = Party.Get(m);

                    if (p != null)
                    {
                        m_PlayersInParty++;

                        if (!parties.Contains(p))
                            parties.Add(p);
                    }
                }
            }
            long netStateScanMs = (Stopwatch.GetTimestamp() - phaseStart) * 1000 / Stopwatch.Frequency;

            phaseStart = Stopwatch.GetTimestamp();
            foreach (Account a in Accounts.GetAccounts())
            {
                ++accountsScanned;

                m_TotalGameTime += a.TotalGameTime;

                if (a.Created < shardCreation)
                    shardCreation = a.Created;
            }
            long accountScanMs = (Stopwatch.GetTimestamp() - phaseStart) * 1000 / Stopwatch.Frequency;

            phaseStart = Stopwatch.GetTimestamp();
            m_ShardAge = DateTime.UtcNow - shardCreation;
            m_Uptime = DateTime.UtcNow - Clock.ServerStart;
            m_LastRestart = Clock.ServerStart;
            m_LastStatsUpdate = DateTime.UtcNow;
            m_ActiveAccounts = Accounts.Count;
            m_ActiveGuilds = Guild.List.Count;
            m_ActiveParties = parties.Count;

            StatsList.Add(String.Format("World Age: {0:n0} days, {1:n0} hours and {2:n0} minutes", m_ShardAge.Days, m_ShardAge.Hours, m_ShardAge.Minutes));
            StatsList.Add(String.Format("Total Game Time: {0:n0} hours and {1:n0} minutes", m_TotalGameTime.TotalHours, m_TotalGameTime.Minutes));
            StatsList.Add(String.Format("Last Restart: {0}", m_LastRestart));
            StatsList.Add(String.Format("Uptime: {0:n0} days, {1:n0} hours and {2:n0} minutes", m_Uptime.Days, m_Uptime.Hours, m_Uptime.Minutes));
            StatsList.Add(String.Format("Active Accounts: {0:n0} [{1:n0} Players Online]", m_ActiveAccounts, m_PlayersOnline));
            StatsList.Add(String.Format("Active Staff Members: {0:n0} [{1:n0} Online]", m_ActiveStaffMembers, m_StaffOnline));
            StatsList.Add(String.Format("Active Parties: {0:n0} [{1:n0} Players]", m_ActiveParties, m_PlayersInParty));
            StatsList.Add(String.Format("Active Guilds: {0:n0}", m_ActiveGuilds));
            StatsList.Add(String.Format("Player Houses: {0:n0}", m_PlayerHouses));
            StatsList.Add(String.Format("Player Gold: {0:n0}", m_PlayerGold));
            long formatMs = (Stopwatch.GetTimestamp() - phaseStart) * 1000 / Stopwatch.Frequency;

            watch.Stop();
            long elapsed = watch.ElapsedMilliseconds;
            ++m_CollectCount;
            m_LastCollectElapsed = elapsed;
            m_TotalCollectElapsed += elapsed;

            if (elapsed > m_MaxCollectElapsed)
                m_MaxCollectElapsed = elapsed;

            m_LastHouseScanMs = houseScanMs;
            m_LastItemScanMs = itemScanMs;
            m_LastItemRegistryMs = itemRegistryMs;
            m_LastMobileScanMs = mobileScanMs;
            m_LastBankCheckScanMs = bankCheckScanMs;
            m_LastNetStateScanMs = netStateScanMs;
            m_LastAccountScanMs = accountScanMs;
            m_LastFormatMs = formatMs;
            m_LastPhysicalItemScans = 0;
            m_LastPhysicalMobileScans = 1;
            m_LastItemsScanned = itemsScanned;
            m_LastItemsScannedForHouses = houses.Length;
            m_LastMobilesScanned = mobilesScanned;
            m_LastItemsScannedForChecks = bankChecks.Length + investmentChecks.Length;
            m_LastNetStatesScanned = netStatesScanned;
            m_LastAccountsScanned = accountsScanned;
            m_LastBankChecksFound = bankChecksFound;
            m_LastInvestmentChecksFound = investmentChecksFound;
            m_LastHouseRegistryCount = houseRegistryCount;
            m_LastBankCheckRegistryCount = bankCheckRegistryCount;
            m_LastInvestmentCheckRegistryCount = investmentCheckRegistryCount;
            m_LastRegistryStaleRemoved = staleRemoved;

            if (elapsed >= SlowCollectStatsThreshold)
                DiagnosticConsole.WriteParsedLine(String.Format("[Lag] CollectStats took {0}ms; itemRegistry={1}ms entries={2} houses={3} bankChecks={4} investmentChecks={5}, staleRemoved={6}, mobileScan={7}ms mobiles={8}, netStates={9}ms count={10}, accounts={11}ms count={12}, format={13}ms", elapsed, itemRegistryMs, registryEntriesScanned, m_PlayerHouses, bankChecksFound, investmentChecksFound, staleRemoved, mobileScanMs, mobilesScanned, netStateScanMs, netStatesScanned, accountScanMs, accountsScanned, formatMs));

            if (Config.ConsoleReport)
            {
                Console.WriteLine("");
                Console.WriteLine("===========================================");
                Console.WriteLine("| Statistics Report | {0} |", m_LastStatsUpdate);
                Console.WriteLine("===========================================");
                Console.WriteLine("");
                foreach (String stat in StatsList)
                {
                    Console.WriteLine("* {0}", stat);
                }
                Console.WriteLine("");
                Console.WriteLine("=============================");
                Console.WriteLine("| Generated in {0:F2} seconds |", watch.Elapsed.TotalSeconds);
                Console.WriteLine("=============================");
                Console.WriteLine("");

            }
        }

        public static void WriteStats(TextWriter writer)
        {
            if (writer == null)
                return;

            long average = m_CollectCount > 0 ? m_TotalCollectElapsed / m_CollectCount : 0;

            writer.WriteLine("CollectStats: count={0}, last={1}ms, avg={2}ms, max={3}ms", m_CollectCount, m_LastCollectElapsed, average, m_MaxCollectElapsed);
            writer.WriteLine("  registry: ready={0}, rebuilds={1}, lastRebuild={2}ms itemsScanned={3} utc={4}, houseCount={5}, bankCheckCount={6}, investmentCheckCount={7}, staleRemoved={8}",
                m_RegistryReady,
                m_RegistryRebuildCount,
                m_LastRegistryRebuildMs,
                m_LastRegistryRebuildItemsScanned,
                m_LastRegistryRebuildTime,
                m_LastHouseRegistryCount,
                m_LastBankCheckRegistryCount,
                m_LastInvestmentCheckRegistryCount,
                m_LastRegistryStaleRemoved);
            writer.WriteLine("  physicalScans: itemScans={0}, mobileScans={1}, items={2}, mobiles={3}", m_LastPhysicalItemScans, m_LastPhysicalMobileScans, m_LastItemsScanned, m_LastMobilesScanned);
            writer.WriteLine("  phases: itemRegistry={0}ms, itemScan={1}ms, houseScan(logical)={2}ms items={3}, mobileScan={4}ms mobiles={5}, bankCheckScan(logical)={6}ms items={7}, netStates={8}ms count={9}, accounts={10}ms count={11}, format={12}ms",
                m_LastItemRegistryMs,
                m_LastItemScanMs,
                m_LastHouseScanMs,
                m_LastItemsScannedForHouses,
                m_LastMobileScanMs,
                m_LastMobilesScanned,
                m_LastBankCheckScanMs,
                m_LastItemsScannedForChecks,
                m_LastNetStateScanMs,
                m_LastNetStatesScanned,
                m_LastAccountScanMs,
                m_LastAccountsScanned,
                m_LastFormatMs);
            writer.WriteLine("  counters: houses={0}, bankChecks={1}, investmentChecks={2}, playerGold={3}", m_PlayerHouses, m_LastBankChecksFound, m_LastInvestmentChecksFound, m_PlayerGold);
        }

        public static void GetPerformanceTestSummary(out int physicalItemScans, out long lastCollectMs)
        {
            physicalItemScans = m_LastPhysicalItemScans;
            lastCollectMs = m_LastCollectElapsed;
        }

        public static void WriteRegistryStats(TextWriter writer)
        {
            if (writer == null)
                return;

            int houses;
            int bankChecks;
            int investmentChecks;

            lock (m_RegistryLock)
            {
                houses = m_HouseRegistry.Count;
                bankChecks = m_BankCheckRegistry.Count;
                investmentChecks = m_InvestmentCheckRegistry.Count;
            }

            writer.WriteLine("StatisticsRegistry: ready={0}, rebuilds={1}, lastRebuild={2}ms itemsScanned={3} utc={4}", m_RegistryReady, m_RegistryRebuildCount, m_LastRegistryRebuildMs, m_LastRegistryRebuildItemsScanned, m_LastRegistryRebuildTime);
            writer.WriteLine("  current: houseCount={0}, bankCheckCount={1}, investmentCheckCount={2}", houses, bankChecks, investmentChecks);
            writer.WriteLine("  lastCollect: houseCount={0}, bankCheckCount={1}, investmentCheckCount={2}, staleRemoved={3}", m_LastHouseRegistryCount, m_LastBankCheckRegistryCount, m_LastInvestmentCheckRegistryCount, m_LastRegistryStaleRemoved);
        }

        public class StatisticsGump : Gump
        {
			public int m_Origin;

            public StatisticsGump( int origin ) : base(25, 25)
            {
				m_Origin = origin;

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);
				AddImage(0, 0, 154);
				AddImage(300, 0, 154);
				AddImage(0, 300, 154);
				AddImage(300, 300, 154);
				AddImage(2, 2, 129);
				AddImage(298, 2, 129);
				AddImage(2, 298, 129);
				AddImage(298, 298, 129);
				AddImage(232, 46, 132);
				AddImage(380, 7, 134);
				AddImage(134, 534, 130);
				AddImage(552, 100, 160);
				AddImage(55, 256, 160);
				AddImage(188, 363, 136);
				AddImage(7, 7, 133);
				AddImage(105, 529, 159);
				AddImage(51, 544, 157);
				AddImage(548, 80, 158);
				AddHtml( 100, 84, 435, 25, @"<BODY><BASEFONT Color=#FBFBFB><BIG>" + Misc.ServerList.ServerName + " - Updated: " + m_LastStatsUpdate + "</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 100, 120, 435, 352, @"<BODY><BASEFONT Color=#FCFF00><BIG>" + String.Join("<br><br>", StatsList.ToArray()) + "</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
            }

			public override void OnResponse( NetState sender, RelayInfo info )
			{
				Mobile from = sender.Mobile;
				if ( m_Origin > 0 ){ from.SendSound( 0x4A ); from.SendGump( new Server.Engines.Help.HelpGump( from, 1 ) ); }
			}
        }
    }
}
