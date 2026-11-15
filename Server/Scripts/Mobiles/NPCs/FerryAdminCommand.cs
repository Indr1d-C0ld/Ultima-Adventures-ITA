using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Commands
{
    public class FerryAdminCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("Ferry", AccessLevel.Administrator, new CommandEventHandler(OnFerryCommand));
        }

        [Usage("Ferry")]
        [Description("Opens the ferry administration panel (administrators only).")]
        private static void OnFerryCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (from == null)
                return;

            if (from.AccessLevel < AccessLevel.Administrator)
            {
                from.SendMessage("Only administrators can configure ferry stops.");
                return;
            }

            from.CloseGump(typeof(FerryAdminGump));
            from.CloseGump(typeof(FerryCaptainSetupGump));
            from.SendGump(new FerryAdminGump(from));
        }
    }
}

namespace Server.Gumps
{
    public class FerryAdminGump : Gump
    {
        private readonly List<FerryCaptain> m_Captains;
        private const int ConfigureButtonBase = 1;
        private const int TeleportButtonBase = 1001;
        private const int PrevFacetButton = 5001;
        private const int NextFacetButton = 5002;

        private readonly List<FerryCaptain> m_PageCaptains;
        private readonly List<Map> m_Facets;
        private readonly int m_Page;

        public FerryAdminGump(Mobile admin, int page = 0) : base(50, 50)
        {
            m_Captains = FerryCaptain.GetFerryCaptains();
            m_Facets = BuildFacetList(m_Captains);

            if (m_Facets.Count == 0)
                m_Facets.Add(null);

            m_Page = Math.Max(0, Math.Min(page, m_Facets.Count - 1));

            Map facet = m_Facets[m_Page];
            m_PageCaptains = m_Captains.FindAll(c => FerryCaptain.NormalizeFacet(c.Map) == facet);

            int height = 160 + (Math.Max(1, m_PageCaptains.Count) * 30);

            AddPage(0);
            AddBackground(0, 0, 680, height, 9270);
            AddAlphaRegion(10, 10, 660, height - 20);

            AddHtml(20, 20, 640, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>Ferry Administration</BASEFONT></CENTER>", false, false);
            AddLabel(20, 45, 0x481, "Review and configure ferry captains.");

            string facetName = GetFacetName(facet);
            AddLabel(20, 70, 0x34, String.Format("Facet: {0} ({1}/{2})", facetName, m_Page + 1, m_Facets.Count));

            if (m_Facets.Count > 1)
            {
                AddButton(480, 70, 4014, 4016, PrevFacetButton, GumpButtonType.Reply, 0);
                AddLabel(514, 70, 0x34, "Prev");
                AddButton(560, 70, 4005, 4007, NextFacetButton, GumpButtonType.Reply, 0);
                AddLabel(594, 70, 0x34, "Next");
            }

            if (m_PageCaptains.Count == 0)
            {
                AddLabel(20, 100, 0x480, "No ferry captains found for this facet.");
                return;
            }

            int y = 100;

            for (int i = 0; i < m_PageCaptains.Count; i++)
            {
                FerryCaptain captain = m_PageCaptains[i];
                string stopName = string.IsNullOrWhiteSpace(captain.StopName) ? "(unnamed stop)" : captain.StopName;
                string mapName = captain.Map != null ? captain.Map.ToString() : "(no map)";

                AddButton(20, y, 4005, 4007, ConfigureButtonBase + i, GumpButtonType.Reply, 0);
                AddLabel(54, y, 0x480, String.Format("{0} - {1}", stopName, captain.Title));
                AddLabel(55, y + 15, 0x481, String.Format("{0} @ {1} [{2} {3}]", mapName, captain.Location, captain.X, captain.Y));
                AddLabel(500, y + 15, 0x34, String.Format("Fare: {0} gold", captain.TravelCost));

                AddButton(610, y, 4005, 4007, TeleportButtonBase + i, GumpButtonType.Reply, 0);
                AddLabel(644, y, 0x34, "Go");

                y += 30;
            }
        }

        private static List<Map> BuildFacetList(List<FerryCaptain> captains)
        {
            List<Map> facets = new List<Map>();

            foreach (FerryCaptain captain in captains)
            {
                Map facet = FerryCaptain.NormalizeFacet(captain.Map);

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

                from.SendGump(new FerryAdminGump(from, prev));
                return;
            }

            if (info.ButtonID == NextFacetButton && m_Facets.Count > 1)
            {
                int next = m_Page + 1;

                if (next >= m_Facets.Count)
                    next = 0;

                from.SendGump(new FerryAdminGump(from, next));
                return;
            }

            if (info.ButtonID >= TeleportButtonBase)
            {
                int teleportIndex = info.ButtonID - TeleportButtonBase;

                if (teleportIndex < 0 || teleportIndex >= m_PageCaptains.Count)
                    return;

                FerryCaptain captain = m_PageCaptains[teleportIndex];

                if (captain == null || captain.Deleted)
                {
                    from.SendMessage("That ferry captain no longer exists.");
                    from.SendGump(new FerryAdminGump(from, m_Page));
                    return;
                }

                if (captain.Map == null)
                {
                    from.SendMessage("That ferry captain does not have a valid location.");
                    from.SendGump(new FerryAdminGump(from, m_Page));
                    return;
                }

                from.MoveToWorld(captain.Location, captain.Map);
                from.SendMessage("You have been teleported to {0}.", captain.Name);
                from.SendGump(new FerryAdminGump(from, m_Page));
                return;
            }

            int index = info.ButtonID - 1;

            if (index < 0 || index >= m_PageCaptains.Count)
                return;

            FerryCaptain setupCaptain = m_PageCaptains[index];

            if (setupCaptain == null || setupCaptain.Deleted)
            {
                from.SendMessage("That ferry captain no longer exists.");
                from.SendGump(new FerryAdminGump(from, m_Page));
                return;
            }

            from.SendGump(new FerryCaptainSetupGump(from, setupCaptain, m_Page));
        }
    }

    public class FerryCaptainSetupGump : Gump
    {
        private readonly FerryCaptain m_Captain;
        private readonly int m_ListPage;
        private const int StopEntryId = 1;
        private const int FareEntryId = 2;
        private const int IntervalEntryId = 3;
        private const int DockEntryId = 4;

        public FerryCaptainSetupGump(Mobile admin, FerryCaptain captain, int listPage) : base(100, 100)
        {
            m_Captain = captain;
            m_ListPage = listPage;

            AddPage(0);
            AddBackground(0, 0, 550, 320, 9270);
            AddAlphaRegion(10, 10, 530, 300);

            AddHtml(20, 20, 510, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>Configure Ferry Stop</BASEFONT></CENTER>", false, false);
            AddLabel(20, 50, 0x481, String.Format("Captain: {0}", captain.Name));
            AddLabel(20, 70, 0x481, String.Format("Location: {0} ({1}, {2}, {3})", captain.Map, captain.X, captain.Y, captain.Z));

            AddLabel(20, 100, 0x480, "Stop name:");
            AddBackground(120, 96, 360, 24, 9350);
            AddTextEntry(125, 98, 350, 20, 0x480, StopEntryId, captain.StopName);

            AddLabel(20, 130, 0x480, "Fare (gold):");
            AddBackground(120, 126, 100, 24, 9350);
            AddTextEntry(125, 128, 90, 20, 0x480, FareEntryId, captain.TravelCost.ToString());

            AddLabel(20, 160, 0x480, "Rotation (minutes):");
            AddBackground(160, 156, 120, 24, 9350);
            AddTextEntry(165, 158, 110, 20, 0x480, IntervalEntryId, captain.DepartureInterval.TotalMinutes.ToString("0.##"));

            AddLabel(20, 190, 0x480, "Dock time (seconds):");
            AddBackground(160, 186, 120, 24, 9350);
            AddTextEntry(165, 188, 110, 20, 0x480, DockEntryId, captain.DockedDuration.TotalSeconds.ToString("0.##"));

            AddButton(20, 230, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddLabel(54, 230, 0x34, "Save changes");

            AddButton(20, 260, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddLabel(54, 260, 0x34, "Refresh stop registration");

            AddButton(20, 290, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddLabel(54, 290, 0x34, "Return to ferry list");

            AddButton(240, 260, 4005, 4007, 4, GumpButtonType.Reply, 0);
            AddLabel(274, 260, 0x34, "Configure links");
            AddLabel(240, 290, 0x480, String.Format("Linked stops: {0}", captain.LinkedCaptains.Count));
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (from == null || m_Captain == null || m_Captain.Deleted)
                return;

            switch (info.ButtonID)
            {
                case 1:
                    ApplyChanges(from, info);
                    break;
                case 2:
                    m_Captain.RegisterStop();
                    from.SendMessage("Ferry stop registration refreshed for {0}.", m_Captain.Name);
                    from.SendGump(new FerryCaptainSetupGump(from, m_Captain, m_ListPage));
                    break;
                case 3:
                    from.SendGump(new FerryAdminGump(from, m_ListPage));
                    break;
                case 4:
                    from.SendGump(new FerryConnectionsGump(from, m_Captain, m_ListPage));
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
                from.SendGump(new FerryCaptainSetupGump(from, m_Captain, m_ListPage));
                return;
            }

            if (!Double.TryParse(intervalText, out minutes) || minutes <= 0)
            {
                from.SendMessage("Please enter a valid rotation time in minutes (greater than zero).");
                from.SendGump(new FerryCaptainSetupGump(from, m_Captain, m_ListPage));
                return;
            }

            if (!Double.TryParse(dockText, out dockSeconds) || dockSeconds < 0)
            {
                from.SendMessage("Please enter a valid docked time in seconds (zero or more).");
                from.SendGump(new FerryCaptainSetupGump(from, m_Captain, m_ListPage));
                return;
            }

            m_Captain.StopName = stopName;
            m_Captain.TravelCost = fare;
            m_Captain.DepartureInterval = TimeSpan.FromMinutes(minutes);
            m_Captain.DockedDuration = TimeSpan.FromSeconds(dockSeconds);
            m_Captain.RegisterStop();

            from.SendMessage("Updated ferry stop '{0}' with fare {1} gold.", String.IsNullOrWhiteSpace(stopName) ? "(unnamed)" : stopName, fare);
            from.SendGump(new FerryCaptainSetupGump(from, m_Captain, m_ListPage));
        }

        public override void OnServerClose(NetState owner)
        {
            base.OnServerClose(owner);

            if (owner != null && owner.Mobile != null)
                owner.Mobile.SendGump(new FerryAdminGump(owner.Mobile, m_ListPage));
        }
    }

    public class FerryConnectionsGump : Gump
    {
        private const int ToggleBase = 1;
        private const int ReturnButton = 5000;

        private readonly FerryCaptain m_Captain;
        private readonly int m_ListPage;
        private readonly List<FerryCaptain> m_Candidates;

        public FerryConnectionsGump(Mobile admin, FerryCaptain captain, int listPage) : base(120, 120)
        {
            m_Captain = captain;
            m_ListPage = listPage;
            m_Candidates = FerryCaptain.GetFerryCaptains().FindAll(c => c != null && !c.Deleted && c != captain && FerryCaptain.NormalizeFacet(c.Map) == FerryCaptain.NormalizeFacet(captain.Map));

            int height = 150 + (Math.Max(1, m_Candidates.Count) * 30);

            AddPage(0);
            AddBackground(0, 0, 570, height, 9270);
            AddAlphaRegion(10, 10, 550, height - 20);

            AddHtml(20, 20, 520, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>Linked Ferry Stops</BASEFONT></CENTER>", false, false);
            AddLabel(20, 45, 0x481, "Toggle destinations for this ferry captain.");
            AddLabel(20, 65, 0x34, String.Format("Facet: {0}", FerryCaptain.NormalizeFacet(captain.Map) ?? Map.Internal));

            int y = 95;

            if (m_Candidates.Count == 0)
            {
                AddLabel(20, y, 0x480, "No other ferry captains found on this facet.");
            }
            else
            {
                for (int i = 0; i < m_Candidates.Count; i++)
                {
                    FerryCaptain candidate = m_Candidates[i];
                    bool linked = captain.IsLinkedTo(candidate);

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

            if (from == null || m_Captain == null || m_Captain.Deleted)
                return;

            if (info.ButtonID == ReturnButton)
            {
                from.SendGump(new FerryCaptainSetupGump(from, m_Captain, m_ListPage));
                return;
            }

            int index = info.ButtonID - ToggleBase;

            if (index < 0 || index >= m_Candidates.Count)
                return;

            FerryCaptain target = m_Candidates[index];

            if (target == null || target.Deleted)
            {
                from.SendMessage("That ferry captain is no longer available.");
                from.SendGump(new FerryConnectionsGump(from, m_Captain, m_ListPage));
                return;
            }

            bool newState = !m_Captain.IsLinkedTo(target);
            m_Captain.SetLink(target, newState);

            from.SendMessage(newState ? "Linked to {0}." : "Unlinked from {0}.", target.StopName);
            from.SendGump(new FerryConnectionsGump(from, m_Captain, m_ListPage));
        }

        public override void OnServerClose(NetState owner)
        {
            base.OnServerClose(owner);

            if (owner != null && owner.Mobile != null)
                owner.Mobile.SendGump(new FerryCaptainSetupGump(owner.Mobile, m_Captain, m_ListPage));
        }
    }
}
