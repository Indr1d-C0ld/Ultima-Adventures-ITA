using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;

namespace Server.Engines.Economy
{
    public enum EconomyTimeframe
    {
        Last24Hours,
        Last7Days,
        Last30Days,
        Last90Days,
        AllTime
    }

    public class EconomyFilterOptions
    {
        public EconomyTimeframe Timeframe { get; set; }
        public string VendorName { get; set; }
        public Serial? VendorSerial { get; set; }
        public string PlayerName { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public string Region { get; set; }
        public Map Map { get; set; }
        public string Counterparty { get; set; }
        public bool SuspiciousOnly { get; set; }
        public int? Days { get; set; }

        public DateTime? GetStartTime()
        {
            if (Days.HasValue && Days.Value > 0)
                return DateTime.UtcNow - TimeSpan.FromDays(Days.Value);

            switch (Timeframe)
            {
                case EconomyTimeframe.Last24Hours:
                    return DateTime.UtcNow - TimeSpan.FromHours(24);
                case EconomyTimeframe.Last7Days:
                    return DateTime.UtcNow - TimeSpan.FromDays(7);
                case EconomyTimeframe.Last30Days:
                    return DateTime.UtcNow - TimeSpan.FromDays(30);
                case EconomyTimeframe.Last90Days:
                    return DateTime.UtcNow - TimeSpan.FromDays(90);
                default:
                    return null;
            }
        }
    }

    public class EconomyTransaction
    {
        public string ItemName { get; set; }
        public string Category { get; set; }
        public Serial VendorSerial { get; set; }
        public string VendorOwner { get; set; }
        public string VendorName { get; set; }
        public string BuyerName { get; set; }
        public int SalePrice { get; set; }
        public int NetPrice { get; set; }
        public DateTime Timestamp { get; set; }
        public Map Map { get; set; }
        public Point3D Location { get; set; }
        public string Region { get; set; }
    }

    public class PriceMetrics
    {
        public int Count { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int Median { get; set; }
        public double Average { get; set; }
    }

    public class VendorSnapshot
    {
        public Serial Serial { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public Map Map { get; set; }
        public Point3D Location { get; set; }
        public string Region { get; set; }
        public int GoldHeld { get; set; }
        public int UnsoldCount { get; set; }
    }

    public class CategoryBreakdown
    {
        public string Category { get; set; }
        public int TotalQuantity { get; set; }
        public int TotalRevenue { get; set; }
        public PriceMetrics Metrics { get; set; }
        public string BestVendor { get; set; }
        public string WorstVendor { get; set; }
    }

    public class SuspiciousActivity
    {
        public string Description { get; set; }
        public EconomyTransaction Transaction { get; set; }
    }

    public class TrendRow
    {
        public string Label { get; set; }
        public int Revenue { get; set; }
        public int Volume { get; set; }
    }

    public static class EconomyHistoryRegistry
    {
        private static readonly List<EconomyTransaction> _transactions = new List<EconomyTransaction>();
        private static readonly Dictionary<Serial, List<EconomyTransaction>> _vendorIndex = new Dictionary<Serial, List<EconomyTransaction>>();
        private static readonly Dictionary<string, List<EconomyTransaction>> _playerIndex = new Dictionary<string, List<EconomyTransaction>>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _syncRoot = new object();
        private const int MaxTransactions = 500000;
        private const double SuspiciousFactor = 3.0;
        private const string SaveDirectory = "Saves/EconomyHistory";
        private const string SaveFileName = "transactions.bin";
        private static bool _persistenceRegistered;

        public static void Configure()
        {
            if (_persistenceRegistered)
            {
                return;
            }

            _persistenceRegistered = true;

            EventSink.WorldLoad += OnLoad;
            EventSink.WorldSave += OnSave;
        }

        public static void LogSale(PlayerVendor vendor, Item item, Mobile buyer, int salePrice, int netPrice, string buyerNameOverride = null)
        {
            if (vendor == null || item == null)
                return;

            Region vendorRegion = Region.Find(vendor.Location, vendor.Map);

            EconomyTransaction tx = new EconomyTransaction
            {
                ItemName = GetItemName(item),
                Category = ResolveCategory(item),
                VendorSerial = vendor.Serial,
                VendorOwner = vendor.Owner != null ? vendor.Owner.RawName : "Unknown",
                VendorName = !string.IsNullOrEmpty(vendor.ShopName) ? vendor.ShopName : vendor.Name,
                BuyerName = !string.IsNullOrEmpty(buyerNameOverride) ? buyerNameOverride : buyer != null ? buyer.RawName : "Unknown",
                SalePrice = salePrice,
                NetPrice = netPrice,
                Timestamp = DateTime.UtcNow,
                Map = vendor.Map,
                Location = vendor.Location,
                Region = vendorRegion != null ? vendorRegion.Name : "Unknown"
            };

            lock (_syncRoot)
            {
                _transactions.Add(tx);

                if (_transactions.Count > MaxTransactions)
                {
                    _transactions.RemoveRange(0, _transactions.Count - MaxTransactions);
                    RebuildIndexesUnlocked();
                }
                else
                {
                    AddToIndexes(tx);
                }
            }
        }

        private static void AddToIndexes(EconomyTransaction tx)
        {
            if (tx == null)
            {
                return;
            }

            List<EconomyTransaction> vendorList;
            if (!_vendorIndex.TryGetValue(tx.VendorSerial, out vendorList))
            {
                vendorList = new List<EconomyTransaction>();
                _vendorIndex[tx.VendorSerial] = vendorList;
            }

            vendorList.Add(tx);

            if (!string.IsNullOrEmpty(tx.BuyerName))
            {
                List<EconomyTransaction> buyerList;
                if (!_playerIndex.TryGetValue(tx.BuyerName, out buyerList))
                {
                    buyerList = new List<EconomyTransaction>();
                    _playerIndex[tx.BuyerName] = buyerList;
                }

                buyerList.Add(tx);
            }

            if (!string.IsNullOrEmpty(tx.VendorOwner))
            {
                List<EconomyTransaction> ownerList;
                if (!_playerIndex.TryGetValue(tx.VendorOwner, out ownerList))
                {
                    ownerList = new List<EconomyTransaction>();
                    _playerIndex[tx.VendorOwner] = ownerList;
                }

                ownerList.Add(tx);
            }
        }

        private static void RebuildIndexesUnlocked()
        {
            _vendorIndex.Clear();
            _playerIndex.Clear();

            foreach (EconomyTransaction tx in _transactions)
            {
                AddToIndexes(tx);
            }
        }

        private static void OnLoad()
        {
            Load();
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            Save();
        }

        public static void Load()
        {
            lock (_syncRoot)
            {
                _transactions.Clear();
                _vendorIndex.Clear();
                _playerIndex.Clear();

                string filePath = Path.Combine(SaveDirectory, SaveFileName);

                if (!File.Exists(filePath))
                {
                    return;
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    GenericReader reader = new BinaryFileReader(br);

                    int version = reader.ReadInt();
                    int count = reader.ReadInt();

                    for (int i = 0; i < count; i++)
                    {
                        EconomyTransaction tx = new EconomyTransaction
                        {
                            ItemName = reader.ReadString(),
                            Category = reader.ReadString(),
                            VendorSerial = reader.ReadInt(),
                            VendorOwner = reader.ReadString(),
                            VendorName = reader.ReadString(),
                            BuyerName = reader.ReadString(),
                            SalePrice = reader.ReadInt(),
                            NetPrice = reader.ReadInt(),
                            Timestamp = reader.ReadDateTime(),
                            Map = reader.ReadMap(),
                            Location = reader.ReadPoint3D(),
                            Region = reader.ReadString()
                        };

                        _transactions.Add(tx);
                        AddToIndexes(tx);
                    }
                }
            }
        }

        public static void Save()
        {
            lock (_syncRoot)
            {
                if (!Directory.Exists(SaveDirectory))
                {
                    Directory.CreateDirectory(SaveDirectory);
                }

                string filePath = Path.Combine(SaveDirectory, SaveFileName);
                GenericWriter writer = new BinaryFileWriter(filePath, true);

                writer.Write(1); // version
                writer.Write(_transactions.Count);

                foreach (EconomyTransaction tx in _transactions)
                {
                    writer.Write(tx.ItemName ?? string.Empty);
                    writer.Write(tx.Category ?? string.Empty);
                    writer.Write(tx.VendorSerial);
                    writer.Write(tx.VendorOwner ?? string.Empty);
                    writer.Write(tx.VendorName ?? string.Empty);
                    writer.Write(tx.BuyerName ?? string.Empty);
                    writer.Write(tx.SalePrice);
                    writer.Write(tx.NetPrice);
                    writer.Write(tx.Timestamp);
                    writer.Write(tx.Map);
                    writer.Write(tx.Location);
                    writer.Write(tx.Region ?? string.Empty);
                }

                writer.Close();
            }
        }

        public static IEnumerable<EconomyTransaction> Query(EconomyFilterOptions filter)
        {
            lock (_syncRoot)
            {
                IEnumerable<EconomyTransaction> query = _transactions.ToList();

                DateTime? start = filter != null ? filter.GetStartTime() : null;

                if (start.HasValue)
                    query = query.Where(t => t.Timestamp >= start.Value);

                if (filter != null)
                {
                    if (filter.VendorSerial.HasValue)
                        query = query.Where(t => t.VendorSerial == filter.VendorSerial.Value);

                    if (!string.IsNullOrEmpty(filter.VendorName))
                        query = query.Where(t => !string.IsNullOrEmpty(t.VendorName) && t.VendorName.IndexOf(filter.VendorName, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (!string.IsNullOrEmpty(filter.PlayerName))
                        query = query.Where(t => string.Equals(t.BuyerName, filter.PlayerName, StringComparison.OrdinalIgnoreCase) || string.Equals(t.VendorOwner, filter.PlayerName, StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrEmpty(filter.ItemName))
                        query = query.Where(t => !string.IsNullOrEmpty(t.ItemName) && t.ItemName.IndexOf(filter.ItemName, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (!string.IsNullOrEmpty(filter.Category))
                        query = query.Where(t => string.Equals(t.Category, filter.Category, StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrEmpty(filter.Region))
                        query = query.Where(t => !string.IsNullOrEmpty(t.Region) && t.Region.IndexOf(filter.Region, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (filter.Map != null)
                        query = query.Where(t => t.Map == filter.Map);

                    if (filter.MinPrice.HasValue)
                        query = query.Where(t => t.SalePrice >= filter.MinPrice.Value);

                    if (filter.MaxPrice.HasValue)
                        query = query.Where(t => t.SalePrice <= filter.MaxPrice.Value);

                    if (!string.IsNullOrEmpty(filter.Counterparty))
                        query = query.Where(t => string.Equals(t.BuyerName, filter.Counterparty, StringComparison.OrdinalIgnoreCase) || string.Equals(t.VendorOwner, filter.Counterparty, StringComparison.OrdinalIgnoreCase));

                    if (filter.SuspiciousOnly)
                        query = query.Where(IsSuspicious);
                }

                return query.ToList();
            }
        }

        public static int Count(EconomyFilterOptions filter)
        {
            return Query(filter ?? new EconomyFilterOptions { Timeframe = EconomyTimeframe.AllTime }).Count();
        }

        public static PriceMetrics GetPriceMetrics(IEnumerable<int> prices)
        {
            List<int> list = prices.Where(p => p > 0).OrderBy(p => p).ToList();
            if (list.Count == 0)
                return new PriceMetrics { Count = 0, Min = 0, Max = 0, Median = 0, Average = 0 };

            int median = list[list.Count / 2];
            return new PriceMetrics
            {
                Count = list.Count,
                Min = list.First(),
                Max = list.Last(),
                Median = median,
                Average = list.Average()
            };
        }

        public static List<CategoryBreakdown> BuildCategoryBreakdown(EconomyFilterOptions filter)
        {
            var tx = Query(filter ?? new EconomyFilterOptions { Timeframe = EconomyTimeframe.AllTime });
            List<CategoryBreakdown> list = new List<CategoryBreakdown>();

            foreach (var grp in tx.GroupBy(t => string.IsNullOrEmpty(t.Category) ? "Uncategorized" : t.Category))
            {
                List<int> prices = grp.Select(g => g.SalePrice).ToList();
                PriceMetrics metrics = GetPriceMetrics(prices);

                string bestVendor = grp.GroupBy(g => g.VendorName).OrderByDescending(g => g.Sum(x => x.SalePrice)).Select(g => FormatVendor(g.Key, g.First().VendorOwner)).FirstOrDefault();
                string worstVendor = grp.GroupBy(g => g.VendorName).OrderBy(g => g.Sum(x => x.SalePrice)).Select(g => FormatVendor(g.Key, g.First().VendorOwner)).FirstOrDefault();

                list.Add(new CategoryBreakdown
                {
                    Category = grp.Key,
                    TotalQuantity = grp.Count(),
                    TotalRevenue = grp.Sum(g => g.SalePrice),
                    Metrics = metrics,
                    BestVendor = bestVendor ?? "-",
                    WorstVendor = worstVendor ?? "-"
                });
            }

            return list.OrderByDescending(c => c.TotalRevenue).ToList();
        }

        public static List<SuspiciousActivity> DetectSuspicious(EconomyFilterOptions filter)
        {
            var tx = Query(filter ?? new EconomyFilterOptions { Timeframe = EconomyTimeframe.Last7Days }).ToList();
            List<SuspiciousActivity> list = new List<SuspiciousActivity>();

            var medianByCategory = tx.GroupBy(t => t.Category).ToDictionary(g => g.Key, g => GetPriceMetrics(g.Select(x => x.SalePrice)));

            foreach (var t in tx)
            {
                PriceMetrics metrics;
                if (medianByCategory.TryGetValue(t.Category, out metrics) && metrics.Median > 0)
                {
                    if (t.SalePrice > metrics.Median * SuspiciousFactor || t.SalePrice < metrics.Median / SuspiciousFactor)
                        list.Add(new SuspiciousActivity { Transaction = t, Description = "Price far outside category median" });
                }
            }

            var rapidVendors = tx.GroupBy(t => t.VendorSerial).Where(g => g.Count(t => (DateTime.UtcNow - t.Timestamp).TotalHours <= 1) >= 20);
            foreach (var g in rapidVendors)
            {
                EconomyTransaction latest = g.OrderByDescending(t => t.Timestamp).First();
                list.Add(new SuspiciousActivity { Transaction = latest, Description = "High frequency microtransactions" });
            }

            var profitBursts = tx.GroupBy(t => t.VendorSerial).Where(g => g.Where(t => (DateTime.UtcNow - t.Timestamp).TotalHours <= 24).Sum(t => t.NetPrice) >= 250000);
            foreach (var g in profitBursts)
            {
                EconomyTransaction latest = g.OrderByDescending(t => t.Timestamp).First();
                list.Add(new SuspiciousActivity { Transaction = latest, Description = "Unusually high profit in short timeframe" });
            }

            var flips = tx.GroupBy(t => new { t.BuyerName, t.VendorOwner, t.Category }).Where(g => g.Count() >= 5);
            foreach (var g in flips)
            {
                list.Add(new SuspiciousActivity { Transaction = g.OrderByDescending(t => t.Timestamp).First(), Description = "Repeated flips between same players" });
            }

            return list.Take(200).ToList();
        }

        public static List<TrendRow> BuildTrends(EconomyTimeframe timeframe)
        {
            EconomyFilterOptions filter = new EconomyFilterOptions { Timeframe = timeframe };
            var tx = Query(filter).ToList();
            List<TrendRow> rows = new List<TrendRow>();

            if (timeframe == EconomyTimeframe.AllTime || tx.Count == 0)
                return rows;

            TimeSpan span = (DateTime.UtcNow - (filter.GetStartTime() ?? DateTime.UtcNow)).Duration();
            string intervalLabel = span.TotalDays <= 7 ? "Daily" : span.TotalDays <= 90 ? "Weekly" : "Monthly";

            IEnumerable<IGrouping<string, EconomyTransaction>> grouping;

            if (intervalLabel == "Daily")
                grouping = tx.GroupBy(t => t.Timestamp.ToLocalTime().Date.ToShortDateString());
            else if (intervalLabel == "Weekly")
                grouping = tx.GroupBy(t => string.Format("Week of {0:d}", t.Timestamp.ToLocalTime().Date.AddDays(-(int)t.Timestamp.DayOfWeek)));
            else
                grouping = tx.GroupBy(t => t.Timestamp.ToLocalTime().ToString("yyyy-MM"));

            foreach (var g in grouping.OrderBy(g => g.First().Timestamp))
            {
                rows.Add(new TrendRow
                {
                    Label = g.Key,
                    Revenue = g.Sum(x => x.SalePrice),
                    Volume = g.Count()
                });
            }

            return rows;
        }

        public static VendorSnapshot CaptureVendor(PlayerVendor vendor)
        {
            if (vendor == null)
                return null;

            Region vendorRegion = Region.Find(vendor.Location, vendor.Map);

            VendorSnapshot snap = new VendorSnapshot
            {
                Serial = vendor.Serial,
                Name = string.IsNullOrEmpty(vendor.ShopName) ? vendor.Name : vendor.ShopName,
                Owner = vendor.Owner != null ? vendor.Owner.RawName : "Unknown",
                Map = vendor.Map,
                Location = vendor.Location,
                Region = vendorRegion != null ? vendorRegion.Name : "Unknown",
                GoldHeld = vendor.HoldGold,
                UnsoldCount = vendor.EnumerateVendorItems().Count(vi => vi.IsForSale)
            };

            return snap;
        }

        public static List<EconomyTransaction> GetVendorTransactions(Serial vendor, EconomyFilterOptions filter)
        {
            filter = filter ?? new EconomyFilterOptions { Timeframe = EconomyTimeframe.AllTime };
            filter.VendorSerial = vendor;
            return Query(filter).ToList();
        }

        public static List<EconomyTransaction> GetPlayerTransactions(string player, EconomyFilterOptions filter)
        {
            filter = filter ?? new EconomyFilterOptions { Timeframe = EconomyTimeframe.AllTime };
            filter.PlayerName = player;
            return Query(filter).ToList();
        }

        public static string ResolveCategory(Item item)
        {
            if (item == null)
                return "Unknown";

            if (item is BaseWeapon)
                return "Weapons";
            if (item is BaseArmor)
                return "Armor";
            if (item is BaseClothing)
                return "Clothing";
            if (item is BaseJewel)
                return "Jewelry";
            if (item is BaseReagent)
                return "Reagents";
            if (item is Arrow || item is Bolt)
                return "Ammunition";
            if (item is BaseIngot)
                return "Ingots";
            if (item is BaseBeverage || item is Food)
                return "Consumables";
            if (item is RecallRune || item is Runebook)
                return "Runes";
            if (item is Container)
                return "Containers";
            if (item is BaseTool)
                return "Tools";

            return item.GetType().Name;
        }

        public static string GetItemName(Item item)
        {
            if (item == null)
                return "Unknown";

            string name = item.Name;

            if (string.IsNullOrEmpty(name))
                name = item.GetType().Name;

            return name;
        }

        public static bool IsSuspicious(EconomyTransaction tx)
        {
            if (tx == null)
                return false;

            List<EconomyTransaction> categoryTransactions;
            lock (_syncRoot)
            {
                categoryTransactions = _transactions.Where(t => string.Equals(t.Category, tx.Category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            PriceMetrics metrics = GetPriceMetrics(categoryTransactions.Select(t => t.SalePrice));

            if (metrics.Median == 0)
                return false;

            return tx.SalePrice > metrics.Median * SuspiciousFactor || tx.SalePrice < metrics.Median / SuspiciousFactor;
        }

        public static string FormatVendor(string vendorName, string owner)
        {
            return string.Format("{0} (Owner: {1})", string.IsNullOrEmpty(vendorName) ? "Unknown Vendor" : vendorName, string.IsNullOrEmpty(owner) ? "Unknown" : owner);
        }
    }

    public class EconomyHistoryCommand
    {
        private static bool _registered;

        [CallPriority(int.MinValue)]
        public static void Configure()
        {
            EnsureRegistered();
        }

        public static void Initialize()
        {
            EnsureRegistered();
        }

        private static void EnsureRegistered()
        {
            if (_registered)
            {
                return;
            }

            _registered = true;

            EconomyHistoryRegistry.Configure();

            RegisterAlias("EconomyHistory");
            RegisterAlias("EconomyHist");
            RegisterAlias("Economy");
            RegisterAlias("EcoHistory");
            RegisterAlias("EH");
        }

        private static void RegisterAlias(string name)
        {
            CommandSystem.Register(name, AccessLevel.Administrator, OnCommand);
        }

        [Usage("EconomyHistory [Player <name>|Item <name>|Category <name>|<vendor serial>]")]
        [Description("Displays administrator vendor economy analytics.")]
        public static void OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            EconomyPage page = EconomyPage.Overview;
            EconomyTimeframe timeframe = EconomyTimeframe.Last30Days;
            Serial? vendorSerial = null;
            string player = null;
            string item = null;
            string category = null;

            if (e.Arguments.Length > 0)
            {
                if (e.Arguments.Length >= 2 && e.Arguments[0].Equals("Player", StringComparison.OrdinalIgnoreCase))
                {
                    page = EconomyPage.PlayerDetail;
                    player = string.Join(" ", e.Arguments.Skip(1)).Trim();
                }
                else if (e.Arguments.Length >= 2 && e.Arguments[0].Equals("Item", StringComparison.OrdinalIgnoreCase))
                {
                    page = EconomyPage.ItemDetail;
                    item = string.Join(" ", e.Arguments.Skip(1)).Trim();
                }
                else if (e.Arguments.Length >= 2 && e.Arguments[0].Equals("Category", StringComparison.OrdinalIgnoreCase))
                {
                    page = EconomyPage.CategoryDetail;
                    category = string.Join(" ", e.Arguments.Skip(1)).Trim();
                }
                else
                {
                    int serial;
                    if (int.TryParse(e.Arguments[0], out serial))
                    {
                        Mobile mob = World.FindMobile(serial);
                        if (mob is PlayerVendor)
                        {
                            page = EconomyPage.VendorDetail;
                            vendorSerial = mob.Serial;
                        }
                    }
                    else
                    {
                        string target = string.Join(" ", e.Arguments).Trim();
                        var match = EconomyHistoryRegistry.Query(new EconomyFilterOptions { Timeframe = EconomyTimeframe.AllTime }).FirstOrDefault(t => !string.IsNullOrEmpty(t.VendorName) && t.VendorName.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0);
                        if (match != null)
                        {
                            page = EconomyPage.VendorDetail;
                            vendorSerial = match.VendorSerial;
                        }
                    }
                }
            }

            from.CloseGump(typeof(EconomyHistoryGump));
            from.SendGump(new EconomyHistoryGump(from, page, timeframe, vendorSerial, player, item, category));
        }
    }

    public enum EconomyPage
    {
        Overview,
        Vendors,
        Players,
        Categories,
        Items,
        Suspicious,
        VendorDetail,
        ItemDetail,
        PlayerDetail,
        CategoryDetail
    }

    public class EconomyHistoryGump : Gump
    {
        private const int PageSize = 10;
        private const int ContentStartY = 170;
        private const int RowSpacing = 24;
        private const int PaginationY = 900;
        private const int NavButtonBase = 2000;
        private const int GumpWidth = 1125;
        private const int GumpHeight = 975;
        private const int AlphaPadding = 10;
        private const int LeftColumnWidth = 500;
        private const int RightColumnX = 540;
        private const int RightColumnWidth = 520;
        public enum PlayerSortMode
        {
            Mixed,
            Earned,
            Spent
        }
        private readonly EconomyPage _page;
        private readonly EconomyTimeframe _timeframe;
        private readonly Serial? _vendorSerial;
        private readonly string _player;
        private readonly string _item;
        private readonly string _category;
        private readonly int _pageIndex;
        private readonly PlayerSortMode _playerSortMode;

        private class VendorListEntry
        {
            public Serial Serial;
            public string Name;
            public string Owner;
            public int Revenue;
        }

        private class ItemListEntry
        {
            public string Name;
            public int Revenue;
            public int Count;
        }

        private class CategoryListEntry
        {
            public string Name;
            public int Revenue;
            public int Count;
        }

        private class PlayerListEntry
        {
            public string Name;
            public int Earned;
            public int Spent;
        }

        private List<VendorListEntry> _vendorList;
        private List<ItemListEntry> _itemList;
        private List<CategoryListEntry> _categoryList;
        private List<PlayerListEntry> _playerList;
        private List<SuspiciousActivity> _suspicious;

        public EconomyHistoryGump(Mobile from, EconomyPage page, EconomyTimeframe timeframe, Serial? vendorSerial, string player, string item, string category, int pageIndex = 0, PlayerSortMode playerSort = PlayerSortMode.Mixed) : base(30, 30)
        {
            _page = page;
            _timeframe = timeframe;
            _vendorSerial = vendorSerial;
            _player = player;
            _item = item;
            _category = category;
            _pageIndex = pageIndex;
            _playerSortMode = playerSort;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, GumpWidth, GumpHeight, 9270);
            AddAlphaRegion(AlphaPadding, AlphaPadding, GumpWidth - (AlphaPadding * 2), GumpHeight - (AlphaPadding * 2));
            AddLabel(20, 20, 1152, "Economy History & Vendor Analytics");

            AddNavBar(from);
            AddTimeframeButtons(from);

            switch (page)
            {
                case EconomyPage.Overview:
                    RenderOverview();
                    break;
                case EconomyPage.Vendors:
                    RenderVendors();
                    break;
                case EconomyPage.Players:
                    RenderPlayers();
                    break;
                case EconomyPage.Categories:
                    RenderCategories();
                    break;
                case EconomyPage.Items:
                    RenderItems();
                    break;
                case EconomyPage.Suspicious:
                    RenderSuspicious();
                    break;
                case EconomyPage.VendorDetail:
                    RenderVendorDetail();
                    break;
                case EconomyPage.ItemDetail:
                    RenderItemDetail();
                    break;
                case EconomyPage.PlayerDetail:
                    RenderPlayerDetail();
                    break;
                case EconomyPage.CategoryDetail:
                    RenderCategoryDetail();
                    break;
            }
        }

        private EconomyFilterOptions BuildFilter()
        {
            return new EconomyFilterOptions
            {
                Timeframe = _timeframe,
                VendorSerial = _vendorSerial,
                PlayerName = _player,
                ItemName = _item,
                Category = _category
            };
        }

        private void AddNavBar(Mobile from)
        {
            int x = 20;
            int y = 60;
            int spacing = 120;

            AddButton(x, y, _page == EconomyPage.Overview ? 4006 : 4005, 4007, NavButtonBase + (int)EconomyPage.Overview, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Overview");
            x += spacing;

            AddButton(x, y, _page == EconomyPage.Vendors ? 4006 : 4005, 4007, NavButtonBase + (int)EconomyPage.Vendors, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Vendors");
            x += spacing;

            AddButton(x, y, _page == EconomyPage.Players ? 4006 : 4005, 4007, NavButtonBase + (int)EconomyPage.Players, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Players");
            x += spacing;

            AddButton(x, y, _page == EconomyPage.Categories ? 4006 : 4005, 4007, NavButtonBase + (int)EconomyPage.Categories, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Categories");
            x += spacing;

            AddButton(x, y, _page == EconomyPage.Items ? 4006 : 4005, 4007, NavButtonBase + (int)EconomyPage.Items, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Items");
            x += spacing;

            AddButton(x, y, _page == EconomyPage.Suspicious ? 4006 : 4005, 4007, NavButtonBase + (int)EconomyPage.Suspicious, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Suspicious");
        }

        private void AddTimeframeButtons(Mobile from)
        {
            int x = 20;
            int y = 100;
            int spacing = 180;

            AddButton(x, y, _timeframe == EconomyTimeframe.Last24Hours ? 4006 : 4005, 4007, 1000, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Last 24 Hours");
            x += spacing;

            AddButton(x, y, _timeframe == EconomyTimeframe.Last7Days ? 4006 : 4005, 4007, 1001, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Last 7 Days");
            x += spacing;

            AddButton(x, y, _timeframe == EconomyTimeframe.Last30Days ? 4006 : 4005, 4007, 1002, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Last 30 Days");
            x += spacing;

            AddButton(x, y, _timeframe == EconomyTimeframe.Last90Days ? 4006 : 4005, 4007, 1003, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Last 90 Days");
            x += spacing - 30;

            AddButton(x, y, _timeframe == EconomyTimeframe.AllTime ? 4006 : 4005, 4007, 1004, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "All Time");
        }

        private void AddPlayerSortButtons()
        {
            int x = 20;
            int y = 140;
            int spacing = 140;

            AddLabel(x, y, 1152, "Player Sort:");
            x += 120;

            AddButton(x, y, _playerSortMode == PlayerSortMode.Mixed ? 4006 : 4005, 4007, 1200, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Mix");
            x += spacing;

            AddButton(x, y, _playerSortMode == PlayerSortMode.Earned ? 4006 : 4005, 4007, 1201, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Earned");
            x += spacing;

            AddButton(x, y, _playerSortMode == PlayerSortMode.Spent ? 4006 : 4005, 4007, 1202, GumpButtonType.Reply, 0);
            AddLabel(x + 33, y, 1152, "Spent");
        }

        private void RenderOverview()
        {
            EconomyFilterOptions filter = BuildFilter();
            var tx = EconomyHistoryRegistry.Query(filter).ToList();
            int totalGold = tx.Sum(t => t.SalePrice);
            int totalTaxedGold = tx.Sum(t => Math.Max(0, t.SalePrice - t.NetPrice));
            int totalItems = tx.Count;
            DateTime referenceStart = filter.GetStartTime() ?? tx.Select(t => t.Timestamp).DefaultIfEmpty(DateTime.UtcNow).Min();
            double days = Math.Max(1, (DateTime.UtcNow - referenceStart).TotalDays);
            double goldVelocity = totalGold / days;
            var categories = tx.GroupBy(t => string.IsNullOrEmpty(t.Category) ? "Uncategorized" : t.Category).OrderByDescending(g => g.Sum(x => x.SalePrice)).Take(5).ToList();
            var topItems = tx.GroupBy(t => t.ItemName).OrderByDescending(g => g.Sum(x => x.SalePrice)).Take(10).ToList();
            var topVendors = tx.GroupBy(t => t.VendorSerial).Select(g => new { Serial = g.Key, Revenue = g.Sum(x => x.SalePrice), Name = g.First().VendorName, Owner = g.First().VendorOwner }).OrderByDescending(g => g.Revenue).Take(10).ToList();

            int baseY = ContentStartY;
            AddLabel(20, baseY, 1152, string.Format("Total Gold Spent: {0:N0}", totalGold));
            AddLabel(20, baseY + 22, 1152, string.Format("Total Items Sold: {0:N0}", totalItems));
            AddLabel(20, baseY + 44, 1152, string.Format("Total Vendor Tax Collected: {0:N0}", totalTaxedGold));
            AddLabel(20, baseY + 66, 1152, string.Format("Gold Velocity: {0:N0} gold/day", goldVelocity));

            int y = baseY + 92;
            AddLabel(20, y, 1152, "Top Categories (Revenue)");
            y += 22;
            foreach (var c in categories)
            {
                AddLabelCropped(35, y, LeftColumnWidth, RowSpacing - 4, 1152, string.Format("{0}: {1:N0} gold ({2} sales)", c.Key, c.Sum(x => x.SalePrice), c.Count()));
                y += RowSpacing - 4;
            }

            y += 15;
            AddLabel(20, y, 1152, "Top 10 Items");
            y += 22;
            int index = 0;
            foreach (var item in topItems)
            {
                AddLabelCropped(35, y, LeftColumnWidth, RowSpacing - 4, 1152, string.Format("{0}. {1} - {2:N0} gold / {3} sales", ++index, item.Key, item.Sum(x => x.SalePrice), item.Count()));
                y += RowSpacing - 6;
            }

            int statsY = y + 20;
            AddLabel(20, statsY, 1152, "Global Price Statistics (per category)");
            statsY += 22;
            var stats = EconomyHistoryRegistry.BuildCategoryBreakdown(filter).Take(6).ToList();
            foreach (var s in stats)
            {
                AddLabelCropped(35, statsY, LeftColumnWidth, RowSpacing - 4, 1152, string.Format("{0}: Min {1:N0}, Max {2:N0}, Median {3:N0}, Avg {4:N0}", s.Category, s.Metrics.Min, s.Metrics.Max, s.Metrics.Median, s.Metrics.Average));
                statsY += RowSpacing - 6;
            }

            int rightColumnY = baseY + 30;
            AddLabel(RightColumnX, rightColumnY, 1152, "Top Vendors by Revenue");
            rightColumnY += 22;
            int idx = 0;
            _vendorList = topVendors.Select(tv => new VendorListEntry
            {
                Serial = tv.Serial,
                Name = tv.Name,
                Owner = tv.Owner,
                Revenue = tv.Revenue
            }).ToList();
            foreach (var vendor in _vendorList)
            {
                AddButton(RightColumnX, rightColumnY, 4005, 4007, 3000 + idx, GumpButtonType.Reply, 0);
                AddLabelCropped(RightColumnX + 38, rightColumnY, RightColumnWidth, RowSpacing - 4, 1152, string.Format("{0}. {1} ({2}) - {3:N0}", ++idx, vendor.Name, vendor.Owner, vendor.Revenue));
                rightColumnY += RowSpacing - 6;
            }

            int trendsY = Math.Max(baseY + 250, rightColumnY + 20);
            AddLabel(RightColumnX, trendsY, 1152, "Trend Tables");
            trendsY += 22;
            foreach (var t in EconomyHistoryRegistry.BuildTrends(_timeframe).Take(8))
            {
                AddLabelCropped(RightColumnX + 20, trendsY, RightColumnWidth, RowSpacing - 4, 1152, string.Format("{0}: {1:N0} gold / {2} sales", t.Label, t.Revenue, t.Volume));
                trendsY += RowSpacing - 6;
            }
        }

        private void RenderVendors()
        {
            EconomyFilterOptions filter = BuildFilter();
            var tx = EconomyHistoryRegistry.Query(filter);
            var vendors = tx.GroupBy(t => t.VendorSerial).Select(g => new { Serial = g.Key, Revenue = g.Sum(x => x.SalePrice), Count = g.Count(), Name = g.First().VendorName, Owner = g.First().VendorOwner }).OrderByDescending(g => g.Revenue).ToList();

            _vendorList = vendors.Select(v => new VendorListEntry
            {
                Serial = v.Serial,
                Name = v.Name,
                Owner = v.Owner,
                Revenue = v.Revenue
            }).ToList();
            DrawPagedList(_vendorList.Count, (i, y) =>
            {
                var v = _vendorList[i];
                AddButton(20, y, 4005, 4007, 3000 + i, GumpButtonType.Reply, 0);
                AddLabel(58, y, 1152, string.Format("{0}. {1} ({2}) - {3:N0} gold", i + 1 + _pageIndex * PageSize, v.Name, v.Owner, v.Revenue));
            });
        }

        private void RenderPlayers()
        {
            AddPlayerSortButtons();

            EconomyFilterOptions filter = BuildFilter();
            var tx = EconomyHistoryRegistry.Query(filter);
            var earned = tx.GroupBy(t => t.VendorOwner).ToDictionary(g => g.Key ?? "Unknown", g => g.Sum(x => x.NetPrice));
            var spent = tx.GroupBy(t => t.BuyerName).ToDictionary(g => g.Key ?? "Unknown", g => g.Sum(x => x.SalePrice));

            var allPlayers = earned.Keys.Union(spent.Keys).Distinct(StringComparer.OrdinalIgnoreCase).Select(name => new
            {
                Name = name,
                Earned = earned.ContainsKey(name) ? earned[name] : 0,
                Spent = spent.ContainsKey(name) ? spent[name] : 0
            }).ToList();

            switch (_playerSortMode)
            {
                case PlayerSortMode.Earned:
                    allPlayers = allPlayers.OrderByDescending(p => p.Earned).ThenByDescending(p => p.Spent).ToList();
                    break;
                case PlayerSortMode.Spent:
                    allPlayers = allPlayers.OrderByDescending(p => p.Spent).ThenByDescending(p => p.Earned).ToList();
                    break;
                default:
                    allPlayers = allPlayers.OrderByDescending(p => p.Earned + p.Spent).ToList();
                    break;
            }

            _playerList = allPlayers.Select(p => new PlayerListEntry
            {
                Name = p.Name,
                Earned = p.Earned,
                Spent = p.Spent
            }).ToList();
            DrawPagedList(_playerList.Count, (i, y) =>
            {
                var p = _playerList[i];
                AddButton(20, y, 4005, 4007, 5000 + i, GumpButtonType.Reply, 0);
                AddLabel(58, y, 1152, string.Format("{0}. {1} - Earned {2:N0}, Spent {3:N0}", i + 1 + _pageIndex * PageSize, p.Name, p.Earned, p.Spent));
            });
        }

        private void RenderCategories()
        {
            EconomyFilterOptions filter = BuildFilter();
            var breakdown = EconomyHistoryRegistry.BuildCategoryBreakdown(filter);
            _categoryList = breakdown.Select(c => new CategoryListEntry
            {
                Name = c.Category,
                Revenue = c.TotalRevenue,
                Count = c.TotalQuantity
            }).ToList();

            DrawPagedList(_categoryList.Count, (i, y) =>
            {
                var c = _categoryList[i];
                AddButton(20, y, 4005, 4007, 6000 + i, GumpButtonType.Reply, 0);
                AddLabel(58, y, 1152, string.Format("{0}. {1} - {2:N0} gold / {3} sales", i + 1 + _pageIndex * PageSize, c.Name, c.Revenue, c.Count));
            });
        }

        private void RenderItems()
        {
            EconomyFilterOptions filter = BuildFilter();
            var tx = EconomyHistoryRegistry.Query(filter);
            var items = tx.GroupBy(t => t.ItemName).Select(g => new { Name = g.Key, Revenue = g.Sum(x => x.SalePrice), Count = g.Count() }).OrderByDescending(g => g.Revenue).ToList();

            _itemList = items.Select(it => new ItemListEntry
            {
                Name = it.Name,
                Revenue = it.Revenue,
                Count = it.Count
            }).ToList();

            DrawPagedList(_itemList.Count, (i, y) =>
            {
                var it = _itemList[i];
                AddButton(20, y, 4005, 4007, 7000 + i, GumpButtonType.Reply, 0);
                AddLabel(58, y, 1152, string.Format("{0}. {1} - {2:N0} gold / {3} sales", i + 1 + _pageIndex * PageSize, it.Name, it.Revenue, it.Count));
            });
        }

        private void RenderSuspicious()
        {
            _suspicious = EconomyHistoryRegistry.DetectSuspicious(BuildFilter());
            DrawPagedList(_suspicious.Count, (i, y) =>
            {
                var s = _suspicious[i];
                AddLabel(20, y, 33, string.Format("{0}. {1}: {2} sold for {3:N0} (Vendor {4}, Buyer {5})", i + 1 + _pageIndex * PageSize, s.Description, s.Transaction.ItemName, s.Transaction.SalePrice, s.Transaction.VendorOwner, s.Transaction.BuyerName));
            });
        }

        private void RenderVendorDetail()
        {
            if (!_vendorSerial.HasValue)
            {
                AddLabel(20, ContentStartY, 33, "No vendor selected.");
                return;
            }

            EconomyFilterOptions filter = BuildFilter();
            var tx = EconomyHistoryRegistry.GetVendorTransactions(_vendorSerial.Value, filter);
            if (tx.Count == 0)
            {
                AddLabel(20, ContentStartY, 33, "No sales for this vendor in the selected timeframe.");
                return;
            }

            int revenue = tx.Sum(t => t.SalePrice);
            int units = tx.Count;
            var topItems = tx.GroupBy(t => t.ItemName).OrderByDescending(g => g.Sum(x => x.SalePrice)).Take(5).ToList();
            DateTime lastSale = tx.Max(t => t.Timestamp);
            double idleDays = (DateTime.UtcNow - lastSale).TotalDays;
            var priceHistory = tx.OrderByDescending(t => t.Timestamp).Take(10).ToList();
            var priceHistoryByItem = tx.GroupBy(t => t.ItemName).Select(g => new { Name = g.Key, Metrics = EconomyHistoryRegistry.GetPriceMetrics(g.Select(x => x.SalePrice)) }).OrderByDescending(g => g.Metrics.Count).Take(5).ToList();

            PlayerVendor vendor = World.FindMobile(_vendorSerial.Value) as PlayerVendor;
            VendorSnapshot snapshot = EconomyHistoryRegistry.CaptureVendor(vendor);

            int listed = snapshot != null ? snapshot.UnsoldCount : 0;
            double sellThrough = units / (double)Math.Max(units + listed, 1) * 100.0;

            int shardAverage = (int)EconomyHistoryRegistry.Query(new EconomyFilterOptions { Timeframe = _timeframe }).GroupBy(t => t.VendorSerial).Select(g => g.Sum(x => x.SalePrice)).DefaultIfEmpty(0).Average();

            int y = ContentStartY;
            AddLabel(20, y, 1152, string.Format("Vendor Revenue: {0:N0}", revenue));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Units Sold: {0}", units));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Idle Time: {0:F1} days", idleDays));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Unsold Stock: {0}", listed));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Sell-through Rate: {0:F1}%", sellThrough));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Gold Stored: {0:N0}", snapshot != null ? snapshot.GoldHeld : 0));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Owner: {0}", snapshot != null ? snapshot.Owner : tx.First().VendorOwner));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Location: {0} [{1}, {2}] ({3})", snapshot != null ? snapshot.Map : tx.First().Map, snapshot != null ? snapshot.Location.X : tx.First().Location.X, snapshot != null ? snapshot.Location.Y : tx.First().Location.Y, snapshot != null ? snapshot.Region : tx.First().Region));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Revenue vs Average: {0:N0}", revenue - shardAverage));

            int rightY = ContentStartY;
            AddLabel(RightColumnX, rightY, 1152, "Top Items");
            rightY += RowSpacing;
            foreach (var item in topItems)
            {
                AddLabelCropped(RightColumnX + 20, rightY, RightColumnWidth, RowSpacing - 4, 1152, string.Format("{0}: {1:N0} / {2} sales", item.Key, item.Sum(x => x.SalePrice), item.Count()));
                rightY += RowSpacing - 6;
            }

            int perItemY = rightY + 20;
            AddLabel(RightColumnX, perItemY, 1152, "Per Item Price Metrics");
            perItemY += RowSpacing;
            foreach (var p in priceHistoryByItem)
            {
                AddLabelCropped(RightColumnX + 20, perItemY, RightColumnWidth, RowSpacing - 4, 1152, string.Format("{0}: Min {1:N0}, Max {2:N0}, Median {3:N0}", p.Name, p.Metrics.Min, p.Metrics.Max, p.Metrics.Median));
                perItemY += RowSpacing - 6;
            }

            int historyY = y + RowSpacing + 20;
            AddLabel(20, historyY, 1152, "Price History (most recent)");
            historyY += RowSpacing;
            foreach (var h in priceHistory)
            {
                AddLabelCropped(35, historyY, LeftColumnWidth, RowSpacing - 4, 1152, string.Format("{0:g}: {1} sold for {2:N0} to {3}", h.Timestamp.ToLocalTime(), h.ItemName, h.SalePrice, h.BuyerName));
                historyY += RowSpacing - 6;
            }
        }

        private void RenderItemDetail()
        {
            if (string.IsNullOrEmpty(_item))
            {
                AddLabel(20, ContentStartY, 33, "No item selected.");
                return;
            }

            EconomyFilterOptions filter = BuildFilter();
            var tx = EconomyHistoryRegistry.Query(filter).Where(t => string.Equals(t.ItemName, _item, StringComparison.OrdinalIgnoreCase)).ToList();
            if (tx.Count == 0)
            {
                AddLabel(20, ContentStartY, 33, "No sales recorded for this item.");
                return;
            }

            int revenue = tx.Sum(t => t.SalePrice);
            PriceMetrics metrics = EconomyHistoryRegistry.GetPriceMetrics(tx.Select(t => t.SalePrice));
            var vendors = tx.GroupBy(t => t.VendorSerial).OrderByDescending(g => g.Sum(x => x.SalePrice)).Take(5).ToList();
            var timeline = tx.OrderByDescending(t => t.Timestamp).Take(10).ToList();

            int y = ContentStartY;
            AddLabel(20, y, 1152, string.Format("Item: {0}", _item));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Total Revenue: {0:N0}", revenue));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Min {0:N0}, Max {1:N0}, Median {2:N0}, Avg {3:N0}", metrics.Min, metrics.Max, metrics.Median, metrics.Average));

            y += RowSpacing;
            AddLabel(20, y, 1152, "Top Vendors");
            y += RowSpacing;
            foreach (var v in vendors)
            {
                AddLabel(35, y, 1152, string.Format("{0}: {1:N0} / {2} sales", EconomyHistoryRegistry.FormatVendor(v.First().VendorName, v.First().VendorOwner), v.Sum(x => x.SalePrice), v.Count()));
                y += RowSpacing - 6;
            }

            int rightY = ContentStartY + 80;
            AddLabel(540, rightY, 1152, "Recent Sales");
            rightY += RowSpacing;
            foreach (var s in timeline)
            {
                AddLabel(560, rightY, 1152, string.Format("{0:g}: {1:N0} by {2}", s.Timestamp.ToLocalTime(), s.SalePrice, s.BuyerName));
                rightY += RowSpacing - 6;
            }
        }

        private void RenderPlayerDetail()
        {
            if (string.IsNullOrEmpty(_player))
            {
                AddLabel(20, ContentStartY, 33, "No player selected.");
                return;
            }

            EconomyFilterOptions filter = BuildFilter();
            var tx = EconomyHistoryRegistry.Query(filter);
            var earned = tx.Where(t => string.Equals(t.VendorOwner, _player, StringComparison.OrdinalIgnoreCase)).ToList();
            var spent = tx.Where(t => string.Equals(t.BuyerName, _player, StringComparison.OrdinalIgnoreCase)).ToList();

            int earnedGold = earned.Sum(t => t.NetPrice);
            int spentGold = spent.Sum(t => t.SalePrice);
            var topBought = spent.GroupBy(t => t.Category).OrderByDescending(g => g.Count()).Take(5).ToList();
            var topSold = earned.GroupBy(t => t.Category).OrderByDescending(g => g.Count()).Take(5).ToList();

            int y = ContentStartY;
            AddLabel(20, y, 1152, string.Format("Player: {0}", _player));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Gold Earned (vendors): {0:N0}", earnedGold));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Gold Spent (vendors): {0:N0}", spentGold));

            y += RowSpacing;
            AddLabel(20, y, 1152, "Top Categories Purchased");
            y += RowSpacing;
            foreach (var g in topBought)
            {
                AddLabel(35, y, 1152, string.Format("{0}: {1} items", g.Key, g.Count()));
                y += RowSpacing - 6;
            }

            int rightY = ContentStartY + RowSpacing;
            AddLabel(540, rightY, 1152, "Top Categories Sold");
            rightY += RowSpacing;
            foreach (var g in topSold)
            {
                AddLabel(560, rightY, 1152, string.Format("{0}: {1} items", g.Key, g.Count()));
                rightY += RowSpacing - 6;
            }

            double avgSpend = spent.Count > 0 ? spent.Average(t => t.SalePrice) : 0;
            double categoryMedian = EconomyHistoryRegistry.GetPriceMetrics(spent.Select(t => t.SalePrice)).Median;
            string priceBehavior = avgSpend == 0 ? "No purchases" : avgSpend > categoryMedian * 1.5 ? "Premium" : avgSpend < categoryMedian * 0.75 ? "Cheap" : "Standard";
            AddLabel(20, ContentStartY + 260, 1152, string.Format("Price Behavior: {0}", priceBehavior));
        }

        private void RenderCategoryDetail()
        {
            if (string.IsNullOrEmpty(_category))
            {
                AddLabel(20, ContentStartY, 33, "No category selected.");
                return;
            }

            EconomyFilterOptions filter = BuildFilter();
            var tx = EconomyHistoryRegistry.Query(filter).Where(t => string.Equals(t.Category, _category, StringComparison.OrdinalIgnoreCase)).ToList();
            if (tx.Count == 0)
            {
                AddLabel(20, ContentStartY, 33, "No data for this category.");
                return;
            }

            int revenue = tx.Sum(t => t.SalePrice);
            int quantity = tx.Count;
            PriceMetrics metrics = EconomyHistoryRegistry.GetPriceMetrics(tx.Select(t => t.SalePrice));
            var breakdown = EconomyHistoryRegistry.BuildCategoryBreakdown(filter).FirstOrDefault(c => string.Equals(c.Category, _category, StringComparison.OrdinalIgnoreCase));
            var vendors = tx.GroupBy(t => t.VendorSerial).Select(g => new { g.Key, Total = g.Sum(x => x.SalePrice), Name = g.First().VendorName, Owner = g.First().VendorOwner }).OrderByDescending(g => g.Total).ToList();
            var items = tx.GroupBy(t => t.ItemName).Select(g => new { g.Key, Total = g.Sum(x => x.SalePrice), Count = g.Count() }).OrderByDescending(g => g.Total).Take(10).ToList();

            int y = ContentStartY;
            AddLabel(20, y, 1152, string.Format("Category: {0}", _category));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Revenue: {0:N0} | Quantity: {1}", revenue, quantity));
            y += RowSpacing;
            AddLabel(20, y, 1152, string.Format("Min {0:N0}, Max {1:N0}, Median {2:N0}, Avg {3:N0}", metrics.Min, metrics.Max, metrics.Median, metrics.Average));
            if (breakdown != null)
            {
                y += RowSpacing;
                AddLabel(20, y, 1152, string.Format("Top Vendor: {0}", breakdown.BestVendor));
                y += RowSpacing;
                AddLabel(20, y, 1152, string.Format("Lowest Vendor: {0}", breakdown.WorstVendor));
            }

            y += RowSpacing;
            AddLabel(20, y, 1152, "Top Vendors");
            y += RowSpacing;
            foreach (var v in vendors.Take(5))
            {
                AddLabel(35, y, 1152, string.Format("{0}: {1:N0}", EconomyHistoryRegistry.FormatVendor(v.Name, v.Owner), v.Total));
                y += RowSpacing - 6;
            }

            int rightY = ContentStartY + RowSpacing;
            AddLabel(540, rightY, 1152, "Top Items");
            rightY += RowSpacing;
            foreach (var i in items)
            {
                AddLabel(560, rightY, 1152, string.Format("{0}: {1:N0} / {2} sales", i.Key, i.Total, i.Count));
                rightY += RowSpacing - 6;
            }
        }

        private void DrawPagedList(int total, Action<int, int> renderRow)
        {
            int maxPageIndex = Math.Max(0, (int)Math.Ceiling(total / (double)PageSize) - 1);
            int currentPage = Math.Min(_pageIndex, maxPageIndex);
            int start = currentPage * PageSize;
            int end = Math.Min(start + PageSize, total);
            int y = ContentStartY;

            for (int i = start; i < end; i++)
            {
                renderRow(i, y);
                y += RowSpacing;
            }

            AddLabel(20, PaginationY, 1152, string.Format("Page {0}/{1}", currentPage + 1, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize))));
            AddButton(150, PaginationY, 4005, 4007, 9000, GumpButtonType.Reply, 0);
            AddLabel(188, PaginationY, 1152, "Prev");
            AddButton(230, PaginationY, 4005, 4007, 9001, GumpButtonType.Reply, 0);
            AddLabel(268, PaginationY, 1152, "Next");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            int bid = info.ButtonID;

            if (bid == 0)
                return;

            if (bid >= 1000 && bid <= 1004)
            {
                EconomyTimeframe tf = _timeframe;
                switch (bid)
                {
                    case 1000: tf = EconomyTimeframe.Last24Hours; break;
                    case 1001: tf = EconomyTimeframe.Last7Days; break;
                    case 1002: tf = EconomyTimeframe.Last30Days; break;
                    case 1003: tf = EconomyTimeframe.Last90Days; break;
                    case 1004: tf = EconomyTimeframe.AllTime; break;
                }

                from.SendGump(new EconomyHistoryGump(from, _page, tf, _vendorSerial, _player, _item, _category, _pageIndex, _playerSortMode));
                return;
            }

            if (bid >= 9000 && bid <= 9001)
            {
                int pageIndex = _pageIndex;
                if (bid == 9000)
                    pageIndex = Math.Max(0, _pageIndex - 1);
                else if (bid == 9001)
                    pageIndex = _pageIndex + 1;

                from.SendGump(new EconomyHistoryGump(from, _page, _timeframe, _vendorSerial, _player, _item, _category, pageIndex, _playerSortMode));
                return;
            }

            int navCount = Enum.GetValues(typeof(EconomyPage)).Length;

            if (bid >= NavButtonBase && bid < NavButtonBase + navCount)
            {
                EconomyPage target = (EconomyPage)(bid - NavButtonBase);
                from.SendGump(new EconomyHistoryGump(from, target, _timeframe, null, null, null, null, 0, _playerSortMode));
                return;
            }

            if (_page == EconomyPage.Players && bid >= 1200 && bid <= 1202)
            {
                PlayerSortMode mode = _playerSortMode;
                if (bid == 1200)
                    mode = PlayerSortMode.Mixed;
                else if (bid == 1201)
                    mode = PlayerSortMode.Earned;
                else if (bid == 1202)
                    mode = PlayerSortMode.Spent;

                from.SendGump(new EconomyHistoryGump(from, _page, _timeframe, _vendorSerial, _player, _item, _category, _pageIndex, mode));
                return;
            }

            if (_page == EconomyPage.Vendors || _page == EconomyPage.Overview)
            {
                int idx = bid - 3000;
                if (_vendorList != null && idx >= 0 && idx < _vendorList.Count)
                {
                    var v = _vendorList[idx];
                    from.SendGump(new EconomyHistoryGump(from, EconomyPage.VendorDetail, _timeframe, v.Serial, null, null, null, 0, _playerSortMode));
                    return;
                }
            }

            if (_page == EconomyPage.Players)
            {
                int idx = bid - 5000;
                if (_playerList != null && idx >= 0 && idx < _playerList.Count)
                {
                    var p = _playerList[idx];
                    from.SendGump(new EconomyHistoryGump(from, EconomyPage.PlayerDetail, _timeframe, null, p.Name, null, null, 0, _playerSortMode));
                    return;
                }
            }

            if (_page == EconomyPage.Categories)
            {
                int idx = bid - 6000;
                if (_categoryList != null && idx >= 0 && idx < _categoryList.Count)
                {
                    var c = _categoryList[idx];
                    from.SendGump(new EconomyHistoryGump(from, EconomyPage.CategoryDetail, _timeframe, null, null, null, c.Name, 0, _playerSortMode));
                    return;
                }
            }

            if (_page == EconomyPage.Items)
            {
                int idx = bid - 7000;
                if (_itemList != null && idx >= 0 && idx < _itemList.Count)
                {
                    var it = _itemList[idx];
                    from.SendGump(new EconomyHistoryGump(from, EconomyPage.ItemDetail, _timeframe, null, null, it.Name, null, 0, _playerSortMode));
                    return;
                }
            }
        }
    }
}
