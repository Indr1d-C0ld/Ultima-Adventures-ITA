using System;
using System.Collections.Generic;
using System.IO;
using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Commands
{
    public static class GPSTagTracker
    {
        private static readonly Dictionary<Serial, TrackedItemState> m_Tracked = new Dictionary<Serial, TrackedItemState>();
        private static readonly string m_TrackedStatePath = Path.Combine(Core.BaseDirectory, "Saves", "GPSTagTrackedItems.txt");
        private static Timer m_PollTimer;

        public static void Initialize()
        {
            LoadTrackedState();
            StartPolling();
            CommandSystem.Register("gpstag", AccessLevel.Counselor, new CommandEventHandler(OnCommand));
            CommandSystem.Register("gpstaglog", AccessLevel.Counselor, new CommandEventHandler(OnLogCommand));
        }

        private static void StartPolling()
        {
            if (m_PollTimer != null)
                m_PollTimer.Stop();

            m_PollTimer = new TrackingPollTimer();
            m_PollTimer.Start();
        }


        private static void OnLogCommand(CommandEventArgs e)
        {
            e.Mobile.CloseGump(typeof(GPSTagLogGump));
            e.Mobile.SendGump(new GPSTagLogGump(e.Mobile, 0, "all"));
        }

        private static void OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Target an item to start or stop GPS tracking.");
            e.Mobile.Target = new GPSTagTarget();
        }

        private static string GetLogFilePath()
        {
            return Path.Combine(Core.BaseDirectory, "Logs", "GPSTag.log");
        }

        private static void WriteLog(string line)
        {
            try
            {
                string path = GetLogFilePath();
                string dir = Path.GetDirectoryName(path);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (StreamWriter writer = new StreamWriter(path, true))
                    writer.WriteLine(line);
            }
            catch
            {
            }
        }

        private static void SaveTrackedState()
        {
            try
            {
                string dir = Path.GetDirectoryName(m_TrackedStatePath);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (StreamWriter writer = new StreamWriter(m_TrackedStatePath, false))
                {
                    foreach (Serial serial in m_Tracked.Keys)
                        writer.WriteLine(serial.Value);
                }
            }
            catch
            {
            }
        }

        private static void LoadTrackedState()
        {
            try
            {
                if (!File.Exists(m_TrackedStatePath))
                    return;

                string[] lines = File.ReadAllLines(m_TrackedStatePath);

                for (int i = 0; i < lines.Length; i++)
                {
                    int value;
                    if (!Int32.TryParse(lines[i], out value))
                        continue;

                    Serial serial = (Serial)value;
                    Item item = World.FindItem(serial);

                    if (item == null || item.Deleted)
                        continue;

                    if (!m_Tracked.ContainsKey(serial))
                        m_Tracked[serial] = new TrackedItemState(item);
                }
            }
            catch
            {
            }
        }

        private static string DescribeContainer(Item item)
        {
            if (item == null)
                return "None";

            return String.Format("{0} ({1}) [0x{2:X}]", item.GetType().Name, item.Serial, item.ItemID);
        }

        private static string DescribeMobile(Mobile mobile)
        {
            if (mobile == null)
                return "None";

            return String.Format("{0} ({1})", mobile.Name, mobile.Serial);
        }

        private static string DescribeItem(Item item)
        {
            if (item == null)
                return "null-item";

            return String.Format("{0} ({1}) [0x{2:X}]", item.GetType().Name, item.Serial, item.ItemID);
        }

        private static string TimeStamp()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
        }

        public static bool IsTracked(Item item)
        {
            return item != null && m_Tracked.ContainsKey(item.Serial);
        }

        public static void ToggleTracking(Mobile from, Item item)
        {
            if (item == null)
                return;

            TrackedItemState state;

            if (m_Tracked.TryGetValue(item.Serial, out state))
            {
                m_Tracked.Remove(item.Serial);
                SaveTrackedState();
                from.SendMessage("Stopped GPS tracking for {0}.", item.GetType().Name);
                WriteLog(String.Format("[{0}] UNTAGGED {1} by {2}", TimeStamp(), DescribeItem(item), DescribeMobile(from)));
                return;
            }

            state = new TrackedItemState(item);
            m_Tracked[item.Serial] = state;
            SaveTrackedState();

            from.SendMessage("Now GPS tracking {0}.", item.GetType().Name);
            WriteLog(String.Format("[{0}] TAGGED {1} by {2}; InitialOwner={3}; Container={4}; Location={5} Map={6}",
                TimeStamp(), DescribeItem(item), DescribeMobile(from), DescribeMobile(state.LastHolder), DescribeContainer(state.LastContainer), state.LastWorldLocation, state.LastMap));
        }

        public static void OnItemAdded(Item item, IEntity newParent)
        {
            TrackedItemState state;

            if (item == null || !m_Tracked.TryGetValue(item.Serial, out state))
                return;

            Mobile newHolder = item.RootParent as Mobile;
            Item newContainer = item.Parent as Item;
            bool changedHolder = state.LastHolder != newHolder;
            bool changedContainer = state.LastContainer != newContainer;

            if (changedHolder || changedContainer)
            {
                string eventName = changedHolder ? "HANDOFF_OR_PICKUP" : "CONTAINER_MOVE";

                WriteLog(String.Format("[{0}] {1} {2}; Holder {3} -> {4}; Container {5} -> {6}; WorldLoc={7} Map={8}",
                    TimeStamp(), eventName, DescribeItem(item), DescribeMobile(state.LastHolder), DescribeMobile(newHolder), DescribeContainer(state.LastContainer), DescribeContainer(newContainer), item.GetWorldLocation(), item.Map));

                state.LastHolder = newHolder;
                state.LastContainer = newContainer;
                state.LastWorldLocation = item.GetWorldLocation();
                state.LastMap = item.Map;
            }
        }

        public static void OnItemRemoved(Item item, IEntity oldParent)
        {
            TrackedItemState state;

            if (item == null || !m_Tracked.TryGetValue(item.Serial, out state))
                return;

            WriteLog(String.Format("[{0}] REMOVED_FROM_PARENT {1}; Holder={2}; FromParent={3}; WorldLoc={4} Map={5}",
                TimeStamp(), DescribeItem(item), DescribeMobile(item.RootParent as Mobile), oldParent, item.GetWorldLocation(), item.Map));
        }

        public static void OnDroppedInto(Item item, Mobile from, Container target, Point3D p)
        {
            if (!IsTracked(item))
                return;

            WriteLog(String.Format("[{0}] DROPPED_INTO_CONTAINER {1}; By={2}; TargetContainer={3}; Point={4}; Map={5}",
                TimeStamp(), DescribeItem(item), DescribeMobile(from), DescribeContainer(target), p, from == null ? null : from.Map));
        }

        public static void OnDroppedToMobile(Item item, Mobile from, Mobile target)
        {
            if (!IsTracked(item))
                return;

            WriteLog(String.Format("[{0}] DROPPED_TO_MOBILE {1}; By={2}; ToMobile={3}; Map={4}",
                TimeStamp(), DescribeItem(item), DescribeMobile(from), DescribeMobile(target), from == null ? null : from.Map));
        }

        public static void OnDroppedOnto(Item item, Mobile from, Item target)
        {
            if (!IsTracked(item))
                return;

            WriteLog(String.Format("[{0}] DROPPED_ONTO_ITEM {1}; By={2}; Onto={3}; Map={4}",
                TimeStamp(), DescribeItem(item), DescribeMobile(from), DescribeContainer(target), from == null ? null : from.Map));
        }

        public static void OnDroppedToWorld(Item item, Mobile from, Point3D p)
        {
            TrackedItemState state;

            if (item == null || !m_Tracked.TryGetValue(item.Serial, out state))
                return;

            WriteLog(String.Format("[{0}] DROPPED_TO_GROUND {1}; By={2}; Coordinates={3}; Map={4}",
                TimeStamp(), DescribeItem(item), DescribeMobile(from), p, from.Map));

            state.LastHolder = null;
            state.LastContainer = null;
            state.LastWorldLocation = p;
            state.LastMap = from.Map;
        }


        public static string[] GetRecentLogLines(int maxLines)
        {
            string path = GetLogFilePath();

            if (!File.Exists(path))
                return new string[0];

            string[] all = File.ReadAllLines(path);

            if (all.Length <= maxLines)
                return all;

            string[] recent = new string[maxLines];
            Array.Copy(all, all.Length - maxLines, recent, 0, maxLines);
            return recent;
        }

        public static string[] GetTrackedItemSummaries()
        {
            List<string> list = new List<string>();

            foreach (KeyValuePair<Serial, TrackedItemState> kv in m_Tracked)
            {
                TrackedItemState state = kv.Value;
                Item item = World.FindItem(kv.Key);
                string itemText = item == null ? String.Format("Deleted ({0})", kv.Key) : DescribeItem(item);

                list.Add(String.Format("{0} | Holder={1} | Container={2} | Loc={3} Map={4}",
                    itemText, DescribeMobile(state.LastHolder), DescribeContainer(state.LastContainer), state.LastWorldLocation, state.LastMap));
            }

            return list.ToArray();
        }

        public static Serial[] GetTrackedSerials()
        {
            Serial[] serials = new Serial[m_Tracked.Count];
            int index = 0;

            foreach (KeyValuePair<Serial, TrackedItemState> kv in m_Tracked)
                serials[index++] = kv.Key;

            return serials;
        }

        public static string GetDetailForSerial(Serial serial, string mode)
        {
            TrackedItemState state;

            if (!m_Tracked.TryGetValue(serial, out state))
                return "This item is no longer actively tracked.";

            Item item = World.FindItem(serial);
            string itemName = item == null ? String.Format("Deleted Item ({0})", serial) : DescribeItem(item);
            string holder = DescribeMobile(state.LastHolder);
            string container = DescribeContainer(state.LastContainer);
            string location = String.Format("{0} ({1})", state.LastWorldLocation, state.LastMap);

            if (mode == "holder")
            {
                string[] lines = FilterItemHistory(serial, "HOLDER", "HANDOFF_OR_PICKUP", "DROPPED_TO_MOBILE", "POLL_HOLDER_CHANGE");
                return BuildHistorySection(itemName, holder, "Holder history", lines);
            }
            if (mode == "container")
            {
                string[] lines = FilterItemHistory(serial, "CONTAINER", "CONTAINER_MOVE", "DROPPED_INTO_CONTAINER", "DROPPED_ONTO_ITEM", "POLL_CONTAINER_CHANGE");
                return BuildHistorySection(itemName, container, "Container history", lines);
            }
            if (mode == "ground")
            {
                string[] lines = FilterItemHistory(serial, "GROUND", "DROPPED_TO_GROUND", "POLL_LOCATION_CHANGE");
                return BuildHistorySection(itemName, location, "Ground/location history", lines);
            }
            if (mode == "history")
            {
                string[] lines = GetItemLogLines(serial, 0);
                if (lines.Length == 0)
                    return "No history lines found for this item yet.";

                return String.Join("<br>", lines);
            }

            return String.Format("Item: {0}<br>Holder: {1}<br>Container: {2}<br>Last Location: {3}<br><br>Use the buttons to filter by specific tracking category.", itemName, holder, container, location);
        }

        private static string BuildHistorySection(string itemName, string currentValue, string label, string[] lines)
        {
            if (lines == null || lines.Length == 0)
                return String.Format("Item: {0}<br>Current: {1}<br><br>{2}: no entries found yet.", itemName, currentValue, label);

            return String.Format("Item: {0}<br>Current: {1}<br><br>{2}:<br>{3}", itemName, currentValue, label, String.Join("<br>", lines));
        }

        private static string[] FilterItemHistory(Serial serial, params string[] keywords)
        {
            string[] lines = GetItemLogLines(serial, 0);

            if (keywords == null || keywords.Length == 0)
                return lines;

            List<string> filtered = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < keywords.Length; j++)
                {
                    if (lines[i].IndexOf(keywords[j], StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        filtered.Add(lines[i]);
                        break;
                    }
                }
            }

            return filtered.ToArray();
        }

        public static string[] GetItemLogLines(Serial serial, int maxLines)
        {
            string path = GetLogFilePath();

            if (!File.Exists(path))
                return new string[0];

            string[] all = File.ReadAllLines(path);
            List<string> matches = new List<string>();
            string token = String.Format("({0})", serial);

            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].Contains(token))
                    matches.Add(all[i]);
            }

            if (maxLines > 0 && matches.Count > maxLines)
            {
                int start = matches.Count - maxLines;
                string[] clipped = new string[maxLines];
                matches.CopyTo(start, clipped, 0, maxLines);
                return clipped;
            }

            return matches.ToArray();
        }

        public static void OnItemDeleted(Item item)
        {
            if (item == null)
                return;

            if (m_Tracked.Remove(item.Serial))
            {
                SaveTrackedState();
                WriteLog(String.Format("[{0}] TRACKING_REMOVED_DELETED {1}", TimeStamp(), DescribeItem(item)));
            }
        }

        private static void PollTrackedItems()
        {
            List<Serial> removed = new List<Serial>();

            foreach (KeyValuePair<Serial, TrackedItemState> kv in m_Tracked)
            {
                Serial serial = kv.Key;
                TrackedItemState state = kv.Value;
                Item item = World.FindItem(serial);

                if (item == null || item.Deleted)
                {
                    removed.Add(serial);
                    continue;
                }

                Mobile newHolder = item.RootParent as Mobile;
                Item newContainer = item.Parent as Item;
                Point3D newLoc = item.GetWorldLocation();
                Map newMap = item.Map;

                if (state.LastHolder != newHolder)
                {
                    WriteLog(String.Format("[{0}] POLL_HOLDER_CHANGE {1}; Holder {2} -> {3}",
                        TimeStamp(), DescribeItem(item), DescribeMobile(state.LastHolder), DescribeMobile(newHolder)));
                }

                if (state.LastContainer != newContainer)
                {
                    WriteLog(String.Format("[{0}] POLL_CONTAINER_CHANGE {1}; Container {2} -> {3}",
                        TimeStamp(), DescribeItem(item), DescribeContainer(state.LastContainer), DescribeContainer(newContainer)));
                }

                if (state.LastWorldLocation != newLoc || state.LastMap != newMap)
                {
                    WriteLog(String.Format("[{0}] POLL_LOCATION_CHANGE {1}; Location {2}/{3} -> {4}/{5}",
                        TimeStamp(), DescribeItem(item), state.LastWorldLocation, state.LastMap, newLoc, newMap));
                }

                state.LastHolder = newHolder;
                state.LastContainer = newContainer;
                state.LastWorldLocation = newLoc;
                state.LastMap = newMap;
            }

            if (removed.Count > 0)
            {
                for (int i = 0; i < removed.Count; i++)
                    m_Tracked.Remove(removed[i]);

                SaveTrackedState();
            }
        }

        private class GPSTagTarget : Target
        {
            public GPSTagTarget() : base(15, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                Item item = targeted as Item;

                if (item == null)
                {
                    from.SendMessage("That is not an item.");
                    return;
                }

                ToggleTracking(from, item);
            }
        }

        private class TrackedItemState
        {
            public Mobile LastHolder;
            public Item LastContainer;
            public Point3D LastWorldLocation;
            public Map LastMap;

            public TrackedItemState(Item item)
            {
                LastHolder = item.RootParent as Mobile;
                LastContainer = item.Parent as Item;
                LastWorldLocation = item.GetWorldLocation();
                LastMap = item.Map;
            }
        }

        private class TrackingPollTimer : Timer
        {
            public TrackingPollTimer() : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
            {
                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                PollTrackedItems();
            }
        }
    }
}

namespace Server.Commands
{
    public class GPSTagLogGump : Gump
    {
        private const int BtnRefresh = 1;
        private const int BtnPrevItem = 2;
        private const int BtnNextItem = 3;
        private const int BtnModeAll = 10;
        private const int BtnModeHolder = 11;
        private const int BtnModeContainer = 12;
        private const int BtnModeGround = 13;
        private const int BtnModeHistory = 14;
        private const int BtnTeleportToItem = 20;
        private const int BtnOpenParentContainer = 21;

        private readonly Mobile m_From;
        private readonly int m_ItemIndex;
        private readonly string m_Mode;

        public GPSTagLogGump(Mobile from, int itemIndex, string mode) : base(60, 40)
        {
            m_From = from;
            m_Mode = mode;

            Serial[] serials = GPSTagTracker.GetTrackedSerials();
            int count = serials.Length;

            if (count == 0)
                itemIndex = 0;
            else
            {
                if (itemIndex < 0)
                    itemIndex = count - 1;
                else if (itemIndex >= count)
                    itemIndex = 0;
            }

            m_ItemIndex = itemIndex;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 920, 700, 5054);
            AddAlphaRegion(10, 10, 900, 680);
            AddLabel(20, 20, 1152, "GPS Tag Tracker Control Panel");
            AddLabel(20, 45, 1152, "Tracked item selector");

            AddButton(20, 70, 4014, 4016, BtnPrevItem, GumpButtonType.Reply, 0);
            AddLabel(55, 72, 1152, "Previous Item");
            AddButton(190, 70, 4005, 4007, BtnNextItem, GumpButtonType.Reply, 0);
            AddLabel(225, 72, 1152, "Next Item");
            AddButton(340, 70, 4011, 4012, BtnRefresh, GumpButtonType.Reply, 0);
            AddLabel(375, 72, 1152, "Refresh");

            string selectedHeader = count == 0 ? "No active tracked items" : String.Format("Selected {0}/{1}: {2}", m_ItemIndex + 1, count, serials[m_ItemIndex]);
            AddHtml(20, 100, 880, 40, "<BASEFONT COLOR=#FFFFFF>" + selectedHeader + "</BASEFONT>", true, true);

            AddLabel(20, 145, 1152, "Tracking detail filters");
            AddModeButton(20, 170, BtnModeAll, "All", mode == "all");
            AddModeButton(120, 170, BtnModeHolder, "Holder", mode == "holder");
            AddModeButton(240, 170, BtnModeContainer, "Container", mode == "container");
            AddModeButton(390, 170, BtnModeGround, "Ground", mode == "ground");
            AddModeButton(510, 170, BtnModeHistory, "History", mode == "history");
            AddButton(650, 168, 4011, 4012, BtnTeleportToItem, GumpButtonType.Reply, 0);
            AddLabel(685, 170, 1152, "Teleport To Item");
            AddButton(650, 192, 4011, 4012, BtnOpenParentContainer, GumpButtonType.Reply, 0);
            AddLabel(685, 194, 1152, "Open Item Container");

            string detail = "Use [gpstag on an item to start tracking.";

            if (count > 0)
                detail = GPSTagTracker.GetDetailForSerial(serials[m_ItemIndex], mode);

            AddLabel(20, 210, 1152, "Details");
            AddBackground(20, 235, 880, 230, 2620);
            AddHtml(30, 245, 860, 210, "<BASEFONT COLOR=#FFFFFF>" + detail + "</BASEFONT>", true, true);

            AddLabel(20, 475, 1152, "Recent global log entries");
            AddBackground(20, 500, 880, 165, 2620);
            string[] recent = GPSTagTracker.GetRecentLogLines(100);
            string recentHtml = recent.Length == 0 ? "No global log entries yet." : String.Join("<br>", recent);
            AddHtml(30, 510, 860, 145, "<BASEFONT COLOR=#FFFFFF>" + recentHtml + "</BASEFONT>", true, true);

            AddLabel(20, 670, 1152, "Commands: [gpstag  [gpstaglog");
        }

        private void AddModeButton(int x, int y, int buttonId, string label, bool selected)
        {
            AddButton(x, y, selected ? 211 : 210, selected ? 212 : 211, buttonId, GumpButtonType.Reply, 0);
            AddLabel(x + 35, y + 2, selected ? 65 : 1152, label);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_From == null || m_From.Deleted)
                return;

            int itemIndex = m_ItemIndex;
            string mode = m_Mode;

            switch (info.ButtonID)
            {
                case BtnPrevItem: itemIndex--; break;
                case BtnNextItem: itemIndex++; break;
                case BtnModeAll: mode = "all"; break;
                case BtnModeHolder: mode = "holder"; break;
                case BtnModeContainer: mode = "container"; break;
                case BtnModeGround: mode = "ground"; break;
                case BtnModeHistory: mode = "history"; break;
                case BtnTeleportToItem:
                    TeleportToSelectedItem();
                    break;
                case BtnOpenParentContainer:
                    OpenSelectedItemContainer();
                    break;
            }

            if (info.ButtonID > 0)
                m_From.SendGump(new GPSTagLogGump(m_From, itemIndex, mode));
        }

        private void TeleportToSelectedItem()
        {
            if (m_From == null || m_From.Deleted)
                return;

            Serial[] serials = GPSTagTracker.GetTrackedSerials();
            if (serials.Length == 0 || m_ItemIndex < 0 || m_ItemIndex >= serials.Length)
            {
                m_From.SendMessage("No tracked item is selected.");
                return;
            }

            Item item = World.FindItem(serials[m_ItemIndex]);
            if (item == null || item.Deleted || item.Map == null || item.Map == Map.Internal)
            {
                m_From.SendMessage("That item is missing or not currently in the world.");
                return;
            }

            Point3D worldLoc = item.GetWorldLocation();
            m_From.MoveToWorld(worldLoc, item.Map);
            m_From.SendMessage("Teleported to {0}.", item.GetType().Name);
        }

        private void OpenSelectedItemContainer()
        {
            if (m_From == null || m_From.Deleted)
                return;

            Serial[] serials = GPSTagTracker.GetTrackedSerials();
            if (serials.Length == 0 || m_ItemIndex < 0 || m_ItemIndex >= serials.Length)
            {
                m_From.SendMessage("No tracked item is selected.");
                return;
            }

            Item item = World.FindItem(serials[m_ItemIndex]);
            if (item == null || item.Deleted)
            {
                m_From.SendMessage("That item no longer exists.");
                return;
            }

            Container parent = item.Parent as Container;
            if (parent == null)
            {
                m_From.SendMessage("This item is not currently inside a container.");
                return;
            }

            parent.DisplayTo(m_From);
            m_From.SendMessage("Opened container for selected item.");
        }
    }
}
