using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Commands
{
    public class WagonAdminCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("Wagon", AccessLevel.Administrator, new CommandEventHandler(OnWagonCommand));
        }

        [Usage("Wagon")]
        [Description("Opens the wagon administration panel (administrators only).")]
        private static void OnWagonCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (from == null)
                return;

            if (from.AccessLevel < AccessLevel.Administrator)
            {
                from.SendMessage("Only administrators can configure wagon routes.");
                return;
            }

            from.CloseGump(typeof(WagonAdminGump));
            from.CloseGump(typeof(WagonDriverSetupGump));
            from.SendGump(new WagonAdminGump(from));
        }
    }
}

namespace Server.Gumps
{
    public class WagonAdminGump : Gump
    {
        private readonly List<WagonDriver> m_Drivers;
        private const int ConfigureButtonBase = 1;
        private const int TeleportButtonBase = 1001;
        private const int PrevFacetButton = 5001;
        private const int NextFacetButton = 5002;

        private readonly List<WagonDriver> m_PageDrivers;
        private readonly List<Map> m_Facets;
        private readonly int m_Page;

        public WagonAdminGump(Mobile admin, int page = 0) : base(50, 50)
        {
            m_Drivers = WagonDriver.GetWagonDrivers();
            m_Facets = BuildFacetList(m_Drivers);

            if (m_Facets.Count == 0)
                m_Facets.Add(null);

            m_Page = Math.Max(0, Math.Min(page, m_Facets.Count - 1));

            Map facet = m_Facets[m_Page];
            m_PageDrivers = m_Drivers.FindAll(d => WagonDriver.NormalizeFacet(d.Map) == facet);

            int height = 160 + (Math.Max(1, m_PageDrivers.Count) * 30);

            AddPage(0);
            AddBackground(0, 0, 680, height, 9270);
            AddAlphaRegion(10, 10, 660, height - 20);

            AddHtml(20, 20, 640, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>Wagon Administration</BASEFONT></CENTER>", false, false);
            AddLabel(20, 45, 0x481, "Review and configure wagon drivers.");

            string facetName = GetFacetName(facet);
            AddLabel(20, 70, 0x34, String.Format("Facet: {0} ({1}/{2})", facetName, m_Page + 1, m_Facets.Count));

            if (m_Facets.Count > 1)
            {
                AddButton(480, 70, 4014, 4016, PrevFacetButton, GumpButtonType.Reply, 0);
                AddLabel(514, 70, 0x34, "Prev");
                AddButton(560, 70, 4005, 4007, NextFacetButton, GumpButtonType.Reply, 0);
                AddLabel(594, 70, 0x34, "Next");
            }

            if (m_PageDrivers.Count == 0)
            {
                AddLabel(20, 100, 0x480, "No wagon drivers found for this facet.");
                return;
            }

            int y = 100;

            for (int i = 0; i < m_PageDrivers.Count; i++)
            {
                WagonDriver driver = m_PageDrivers[i];
                string stopName = string.IsNullOrWhiteSpace(driver.StopName) ? "(unnamed stop)" : driver.StopName;
                string mapName = driver.Map != null ? driver.Map.ToString() : "(no map)";

                AddButton(20, y, 4005, 4007, ConfigureButtonBase + i, GumpButtonType.Reply, 0);
                AddLabel(54, y, 0x480, String.Format("{0} - {1}", stopName, driver.Title));
                AddLabel(55, y + 15, 0x481, String.Format("{0} @ {1} [{2} {3}]", mapName, driver.Location, driver.X, driver.Y));
                AddLabel(500, y + 15, 0x34, String.Format("Fare: {0} gold", driver.TravelCost));

                AddButton(610, y, 4005, 4007, TeleportButtonBase + i, GumpButtonType.Reply, 0);
                AddLabel(644, y, 0x34, "Go");

                y += 30;
            }
        }

        private static List<Map> BuildFacetList(List<WagonDriver> drivers)
        {
            List<Map> facets = new List<Map>();

            foreach (WagonDriver driver in drivers)
            {
                Map facet = WagonDriver.NormalizeFacet(driver.Map);

                if (!facets.Contains(facet))
                    facets.Add(facet);
            }

            facets.Sort((a, b) => string.Compare(GetFacetName(a), GetFacetName(b), StringComparison.OrdinalIgnoreCase));
            return facets;
        }

        private static string GetFacetName(Map map)
        {
            return map != null ? map.ToString() : "(no facet)";
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (from == null)
                return;

            if (info.ButtonID == PrevFacetButton && m_Facets.Count > 1)
            {
                int prev = m_Page - 1;

                if (prev < 0)
                    prev = m_Facets.Count - 1;

                from.SendGump(new WagonAdminGump(from, prev));
                return;
            }

            if (info.ButtonID == NextFacetButton && m_Facets.Count > 1)
            {
                int next = m_Page + 1;

                if (next >= m_Facets.Count)
                    next = 0;

                from.SendGump(new WagonAdminGump(from, next));
                return;
            }

            if (info.ButtonID >= TeleportButtonBase)
            {
                int teleportIndex = info.ButtonID - TeleportButtonBase;

                if (teleportIndex < 0 || teleportIndex >= m_PageDrivers.Count)
                    return;

                WagonDriver driver = m_PageDrivers[teleportIndex];

                if (driver == null || driver.Deleted)
                {
                    from.SendMessage("That wagon driver no longer exists.");
                    from.SendGump(new WagonAdminGump(from, m_Page));
                    return;
                }

                if (driver.Map == null)
                {
                    from.SendMessage("That wagon driver does not have a valid location.");
                    from.SendGump(new WagonAdminGump(from, m_Page));
                    return;
                }

                from.MoveToWorld(driver.Location, driver.Map);
                from.SendMessage("You have been teleported to {0}.", driver.Name);
                from.SendGump(new WagonAdminGump(from, m_Page));
                return;
            }

            int index = info.ButtonID - 1;

            if (index < 0 || index >= m_PageDrivers.Count)
                return;

            WagonDriver setupDriver = m_PageDrivers[index];

            if (setupDriver == null || setupDriver.Deleted)
            {
                from.SendMessage("That wagon driver no longer exists.");
                from.SendGump(new WagonAdminGump(from, m_Page));
                return;
            }

            from.SendGump(new WagonDriverSetupGump(from, setupDriver, m_Page));
        }
    }

    public class WagonDriverSetupGump : Gump
    {
        private readonly WagonDriver m_Driver;
        private readonly int m_ListPage;
        private const int StopEntryId = 1;
        private const int FareEntryId = 2;
        private const int IntervalEntryId = 3;
        private const int DockEntryId = 4;

        public WagonDriverSetupGump(Mobile admin, WagonDriver driver, int listPage) : base(100, 100)
        {
            m_Driver = driver;
            m_ListPage = listPage;

            AddPage(0);
            AddBackground(0, 0, 550, 320, 9270);
            AddAlphaRegion(10, 10, 530, 300);

            AddHtml(20, 20, 510, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>Configure Wagon Stop</BASEFONT></CENTER>", false, false);
            AddLabel(20, 50, 0x481, String.Format("Driver: {0}", driver.Name));
            AddLabel(20, 70, 0x481, String.Format("Location: {0} ({1}, {2}, {3})", driver.Map, driver.X, driver.Y, driver.Z));

            AddLabel(20, 100, 0x480, "Stop name:");
            AddBackground(120, 96, 360, 24, 9350);
            AddTextEntry(125, 98, 350, 20, 0x480, StopEntryId, driver.StopName);

            AddLabel(20, 130, 0x480, "Fare (gold):");
            AddBackground(120, 126, 100, 24, 9350);
            AddTextEntry(125, 128, 90, 20, 0x480, FareEntryId, driver.TravelCost.ToString());

            AddLabel(20, 160, 0x480, "Rotation (minutes):");
            AddBackground(160, 156, 120, 24, 9350);
            AddTextEntry(165, 158, 110, 20, 0x480, IntervalEntryId, driver.DepartureInterval.TotalMinutes.ToString("0.##"));

            AddLabel(20, 190, 0x480, "Dock time (seconds):");
            AddBackground(160, 186, 120, 24, 9350);
            AddTextEntry(165, 188, 110, 20, 0x480, DockEntryId, driver.DockedDuration.TotalSeconds.ToString("0.##"));

            AddButton(20, 230, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddLabel(54, 230, 0x34, "Save changes");

            AddButton(20, 260, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddLabel(54, 260, 0x34, "Refresh stop registration");

            AddButton(20, 290, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddLabel(54, 290, 0x34, "Return to wagon list");

            AddButton(240, 260, 4005, 4007, 4, GumpButtonType.Reply, 0);
            AddLabel(274, 260, 0x34, "Configure links");
            AddLabel(240, 290, 0x480, String.Format("Linked stops: {0}", driver.LinkedDrivers.Count));
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (from == null || m_Driver == null || m_Driver.Deleted)
                return;

            switch (info.ButtonID)
            {
                case 1:
                    ApplyChanges(from, info);
                    break;
                case 2:
                    m_Driver.RegisterStop();
                    from.SendMessage("Wagon stop registration refreshed for {0}.", m_Driver.Name);
                    from.SendGump(new WagonDriverSetupGump(from, m_Driver, m_ListPage));
                    break;
                case 3:
                    from.SendGump(new WagonAdminGump(from, m_ListPage));
                    break;
                case 4:
                    from.SendGump(new WagonConnectionsGump(from, m_Driver, m_ListPage));
                    break;
            }
        }

        private void ApplyChanges(Mobile from, RelayInfo info)
        {
            TextRelay stopEntry = info.GetTextEntry(StopEntryId);
            TextRelay fareEntry = info.GetTextEntry(FareEntryId);
            TextRelay intervalEntry = info.GetTextEntry(IntervalEntryId);
            TextRelay dockEntry = info.GetTextEntry(DockEntryId);

            string stopName = stopEntry != null ? stopEntry.Text.Trim() : string.Empty;
            string fareText = fareEntry != null ? fareEntry.Text.Trim() : string.Empty;
            string intervalText = intervalEntry != null ? intervalEntry.Text.Trim() : string.Empty;
            string dockText = dockEntry != null ? dockEntry.Text.Trim() : string.Empty;

            int fare;
            double minutes;
            double dockSeconds;

            if (!Int32.TryParse(fareText, out fare) || fare < 0)
            {
                from.SendMessage("Please enter a valid non-negative fare amount.");
                from.SendGump(new WagonDriverSetupGump(from, m_Driver, m_ListPage));
                return;
            }

            if (!Double.TryParse(intervalText, out minutes) || minutes <= 0)
            {
                from.SendMessage("Please enter a valid rotation time in minutes (greater than zero).");
                from.SendGump(new WagonDriverSetupGump(from, m_Driver, m_ListPage));
                return;
            }

            if (!Double.TryParse(dockText, out dockSeconds) || dockSeconds < 0)
            {
                from.SendMessage("Please enter a valid docked time in seconds (zero or more).");
                from.SendGump(new WagonDriverSetupGump(from, m_Driver, m_ListPage));
                return;
            }

            m_Driver.StopName = stopName;
            m_Driver.TravelCost = fare;
            m_Driver.DepartureInterval = TimeSpan.FromMinutes(minutes);
            m_Driver.DockedDuration = TimeSpan.FromSeconds(dockSeconds);
            m_Driver.RegisterStop();

            from.SendMessage("Updated wagon stop '{0}' with fare {1} gold.", String.IsNullOrWhiteSpace(stopName) ? "(unnamed)" : stopName, fare);
            from.SendGump(new WagonDriverSetupGump(from, m_Driver, m_ListPage));
        }

        public override void OnServerClose(NetState owner)
        {
            base.OnServerClose(owner);

            if (owner != null && owner.Mobile != null)
                owner.Mobile.SendGump(new WagonAdminGump(owner.Mobile, m_ListPage));
        }
    }

    public class WagonConnectionsGump : Gump
    {
        private const int ToggleBase = 1;
        private const int ReturnButton = 5000;

        private readonly WagonDriver m_Driver;
        private readonly int m_ListPage;
        private readonly List<WagonDriver> m_Candidates;

        public WagonConnectionsGump(Mobile admin, WagonDriver driver, int listPage) : base(120, 120)
        {
            m_Driver = driver;
            m_ListPage = listPage;
            m_Candidates = WagonDriver.GetWagonDrivers().FindAll(d => d != null && !d.Deleted && d != driver && WagonDriver.NormalizeFacet(d.Map) == WagonDriver.NormalizeFacet(driver.Map));

            int height = 150 + (Math.Max(1, m_Candidates.Count) * 30);

            AddPage(0);
            AddBackground(0, 0, 570, height, 9270);
            AddAlphaRegion(10, 10, 550, height - 20);

            AddHtml(20, 20, 520, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>Linked Wagon Stops</BASEFONT></CENTER>", false, false);
            AddLabel(20, 45, 0x481, "Toggle destinations for this wagon driver.");
            AddLabel(20, 65, 0x34, String.Format("Facet: {0}", WagonDriver.NormalizeFacet(driver.Map) ?? Map.Internal));

            int y = 95;

            if (m_Candidates.Count == 0)
            {
                AddLabel(20, y, 0x480, "No other wagon drivers found on this facet.");
            }
            else
            {
                for (int i = 0; i < m_Candidates.Count; i++)
                {
                    WagonDriver candidate = m_Candidates[i];
                    bool linked = driver.IsLinkedTo(candidate);

                    AddButton(20, y, linked ? 4018 : 4005, linked ? 4019 : 4007, ToggleBase + i, GumpButtonType.Reply, 0);
                    AddLabel(54, y, 0x480, candidate.StopName);
                    AddLabel(240, y, 0x481, String.Format("{0} [{1}, {2}]", candidate.Map, candidate.X, candidate.Y));
                    AddLabel(440, y, linked ? 0x44 : 0x480, linked ? "Linked" : "Not linked");

                    y += 30;
                }
            }

            AddButton(20, height - 40, 4005, 4007, ReturnButton, GumpButtonType.Reply, 0);
            AddLabel(54, height - 40, 0x34, "Return to setup");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (from == null || m_Driver == null || m_Driver.Deleted)
                return;

            if (info.ButtonID == ReturnButton)
            {
                from.SendGump(new WagonDriverSetupGump(from, m_Driver, m_ListPage));
                return;
            }

            int index = info.ButtonID - ToggleBase;

            if (index < 0 || index >= m_Candidates.Count)
                return;

            WagonDriver target = m_Candidates[index];

            if (target == null || target.Deleted)
            {
                from.SendMessage("That wagon driver is no longer available.");
                from.SendGump(new WagonConnectionsGump(from, m_Driver, m_ListPage));
                return;
            }

            bool newState = !m_Driver.IsLinkedTo(target);
            m_Driver.SetLink(target, newState);

            from.SendMessage(newState ? "Linked to {0}." : "Unlinked from {0}.", target.StopName);
            from.SendGump(new WagonConnectionsGump(from, m_Driver, m_ListPage));
        }

        public override void OnServerClose(NetState owner)
        {
            base.OnServerClose(owner);

            if (owner != null && owner.Mobile != null)
                owner.Mobile.SendGump(new WagonDriverSetupGump(owner.Mobile, m_Driver, m_ListPage));
        }
    }
}
