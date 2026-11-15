using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;

namespace Server.Gumps
{
    [Flags]
    public enum ReagentGroup
    {
        None = 0,
        Magery = 1,
        Necromancy = 2,
        Alchemy = 4,
        Unique = 8,
        Special = 16,
        All = Magery | Necromancy | Alchemy | Unique | Special
    }

    public class ReagentDefinition
    {
        public string Key;
        public string Name;
        public Type Type;
        public int ItemID;
        public int Hue;
        public ReagentGroup Groups;

        public ReagentDefinition(string key, string name, Type type, int itemID, int hue, ReagentGroup groups)
        {
            Key = key;
            Name = name;
            Type = type;
            ItemID = itemID;
            Hue = hue;
            Groups = groups;
        }
    }

    public class ReagentBarSettings
    {
        public bool Vertical;
        public bool ShowNames;
        public int NameLayout; // 0 = alternating, 1 = top, 2 = bottom
        public List<string> Keys;

        public ReagentBarSettings()
        {
            Vertical = false;
            ShowNames = true;
            NameLayout = 0;
            Keys = new List<string>();
        }
    }

    public class ReagentToolbarState
    {
        public List<ReagentBarSettings> Bars;

        public ReagentToolbarState()
        {
            Bars = new List<ReagentBarSettings>();
        }
    }

    public static class ReagentToolbarSystem
    {
        private static readonly List<ReagentDefinition> m_Definitions = new List<ReagentDefinition>();
        private static readonly Dictionary<string, ReagentDefinition> m_ByKey = new Dictionary<string, ReagentDefinition>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<Serial, ReagentToolbarState> m_State = new Dictionary<Serial, ReagentToolbarState>();

        public static List<ReagentDefinition> Definitions { get { return m_Definitions; } }

        static ReagentToolbarSystem()
        {
            Add(new ReagentDefinition("blackpearl", "Black Pearl", typeof(BlackPearl), 3962, 0, ReagentGroup.Magery));
            Add(new ReagentDefinition("bloodmoss", "Bloodmoss", typeof(Bloodmoss), 3963, 0, ReagentGroup.Magery));
            Add(new ReagentDefinition("garlic", "Garlic", typeof(Garlic), 3972, 0, ReagentGroup.Magery));
            Add(new ReagentDefinition("ginseng", "Ginseng", typeof(Ginseng), 3973, 0, ReagentGroup.Magery));
            Add(new ReagentDefinition("mandrakeroot", "Mandrake Root", typeof(MandrakeRoot), 3974, 0, ReagentGroup.Magery));
            Add(new ReagentDefinition("nightshade", "Nightshade", typeof(Nightshade), 3976, 0, ReagentGroup.Magery));
            Add(new ReagentDefinition("spiderssilk", "Spider's Silk", typeof(SpidersSilk), 3981, 0, ReagentGroup.Magery));
            Add(new ReagentDefinition("sulfurousash", "Sulfurous Ash", typeof(SulfurousAsh), 3980, 0, ReagentGroup.Magery));

            Add(new ReagentDefinition("batwing", "Bat Wing", typeof(BatWing), 3960, 0, ReagentGroup.Necromancy));
            Add(new ReagentDefinition("daemonblood", "Daemon Blood", typeof(DaemonBlood), 3965, 0, ReagentGroup.Necromancy));
            Add(new ReagentDefinition("gravedust", "Grave Dust", typeof(GraveDust), 3983, 0, ReagentGroup.Necromancy));
            Add(new ReagentDefinition("noxcrystal", "Nox Crystal", typeof(NoxCrystal), 3982, 0, ReagentGroup.Necromancy));
            Add(new ReagentDefinition("pigiron", "Pig Iron", typeof(PigIron), 3978, 0, ReagentGroup.Necromancy));

            Add(new ReagentDefinition("beetleshell", "Beetle Shell", typeof(BeetleShell), 12280, 0, ReagentGroup.Alchemy));
            Add(new ReagentDefinition("brimstone", "Brimstone", typeof(Brimstone), 12243, 0, ReagentGroup.Alchemy));
            Add(new ReagentDefinition("butterflywings", "Butterfly Wings", typeof(ButterflyWings), 12290, 0, ReagentGroup.Alchemy));
            Add(new ReagentDefinition("eyeoftoad", "Eye Of Toad", typeof(EyeOfToad), 12250, 0, ReagentGroup.Alchemy));
            Add(new ReagentDefinition("fairyegg", "Fairy Egg", typeof(FairyEgg), 12251, 0, ReagentGroup.Alchemy));
            Add(new ReagentDefinition("gargoyleear", "Gargoyle Ear", typeof(GargoyleEar), 12249, 0, ReagentGroup.Alchemy));
            Add(new ReagentDefinition("mooncrystal", "Moon Crystal", typeof(MoonCrystal), 12291, 0, ReagentGroup.Alchemy));
            Add(new ReagentDefinition("pixieskull", "Pixie Skull", typeof(PixieSkull), 12257, 0, ReagentGroup.Alchemy));
            Add(new ReagentDefinition("redlotus", "Red Lotus", typeof(RedLotus), 12264, 0, ReagentGroup.Alchemy));
            Add(new ReagentDefinition("seasalt", "Sea Salt", typeof(SeaSalt), 12265, 0, ReagentGroup.Alchemy));
            Add(new ReagentDefinition("silverwidow", "Silver Widow", typeof(SilverWidow), 12279, 0, ReagentGroup.Alchemy));
            Add(new ReagentDefinition("swampberries", "Swamp Berries", typeof(SwampBerries), 12256, 0, ReagentGroup.Alchemy));

            Add(new ReagentDefinition("unicornhorn", "Unicorn Horn", typeof(UnicornHorn), 0x2DB7, 0x47E, ReagentGroup.Unique));
            Add(new ReagentDefinition("goldenserpentvenom", "Golden Serpent Venom", typeof(GoldenSerpentVenom), 0x1FDD, 0x491, ReagentGroup.Unique));
            Add(new ReagentDefinition("silverserpentvenom", "Silver Serpent Venom", typeof(SilverSerpentVenom), 0x1FDD, 0x961, ReagentGroup.Unique));
            Add(new ReagentDefinition("pegasusfeather", "Pegasus Feather", typeof(PegasusFeather), 0x4CCD, 0xB5C, ReagentGroup.Unique));
            Add(new ReagentDefinition("phoenixfeather", "Phoenix Feather", typeof(PhoenixFeather), 0x4CCD, 0xB71, ReagentGroup.Unique));
            Add(new ReagentDefinition("lichdust", "Lich Dust", typeof(LichDust), 0xF8F, 0xB85, ReagentGroup.Unique));
            Add(new ReagentDefinition("demigodblood", "Demigod Blood", typeof(DemigodBlood), 0xF0E, 0x4AA, ReagentGroup.Unique));
            Add(new ReagentDefinition("enchantedseaweed", "Enchanted Seaweed", typeof(EnchantedSeaweed), 0x0A96, 0x84E, ReagentGroup.Unique));
            Add(new ReagentDefinition("dragontooth", "Dragon Tooth", typeof(DragonTooth), 0xF78, 0x481, ReagentGroup.Unique));
            Add(new ReagentDefinition("dragonblood", "Dragon Blood", typeof(DragonBlood), 0xF7D, 0x4AA, ReagentGroup.Unique));
            Add(new ReagentDefinition("demonclaw", "Demon Claw", typeof(DemonClaw), 0x2DB8, 0, ReagentGroup.Unique));

            Add(new ReagentDefinition("daemonbone", "Daemon Bone", typeof(DaemonBone), 0xF80, 0, ReagentGroup.Special));
            Add(new ReagentDefinition("deadwood", "Dead Wood", typeof(DeadWood), 0xF90, 0, ReagentGroup.Special));
            Add(new ReagentDefinition("hellshard", "Hell Shard", typeof(HellShard), 0x3003, 0x86C, ReagentGroup.Special));
            Add(new ReagentDefinition("karancrystal", "Karan Crystal", typeof(KaranCrystal), 0x3003, 0xB96, ReagentGroup.Special));
        }

        private static void Add(ReagentDefinition def)
        {
            m_Definitions.Add(def);
            m_ByKey[def.Key] = def;
        }

        public static ReagentToolbarState GetState(Mobile from)
        {
            ReagentToolbarState state;

            if (!m_State.TryGetValue(from.Serial, out state))
            {
                state = CreateDefaultState();
                m_State[from.Serial] = state;
            }

            return state;
        }

        private static ReagentToolbarState CreateDefaultState()
        {
            ReagentToolbarState state = new ReagentToolbarState();

            ReagentBarSettings magery = new ReagentBarSettings();
            ApplyGroup(magery, ReagentGroup.Magery);

            ReagentBarSettings necro = new ReagentBarSettings();
            ApplyGroup(necro, ReagentGroup.Necromancy);
            necro.Vertical = true;

            ReagentBarSettings alchemy = new ReagentBarSettings();
            ApplyGroup(alchemy, ReagentGroup.Alchemy);

            ReagentBarSettings unique = new ReagentBarSettings();
            ApplyGroup(unique, ReagentGroup.Unique);

            state.Bars.Add(magery);
            state.Bars.Add(necro);
            state.Bars.Add(alchemy);
            state.Bars.Add(unique);

            return state;
        }

        public static void ApplyGroup(ReagentBarSettings bar, ReagentGroup group)
        {
            bar.Keys.Clear();
            for (int i = 0; i < m_Definitions.Count; i++)
            {
                ReagentDefinition def = m_Definitions[i];
                if ((def.Groups & group) != 0)
                    bar.Keys.Add(def.Key);
            }
        }

        public static ReagentDefinition Find(string key)
        {
            ReagentDefinition def;
            if (m_ByKey.TryGetValue(key, out def))
                return def;
            return null;
        }

        public static void OpenBar(Mobile from, int slot)
        {
            OpenBar(from, slot, true);
        }

        public static void OpenBar(Mobile from, int slot, bool remember)
        {
            ReagentToolbarState state = GetState(from);
            if (slot < 0 || slot >= state.Bars.Count)
                return;

            if (remember)
                ToolBarUpdates.RememberToolbar(from, "reagentbar" + (slot + 1));

            if (slot == 0)
            {
                from.CloseGump(typeof(ReagentDisplayGump1));
                from.SendGump(new ReagentDisplayGump1(from));
            }
            else if (slot == 1)
            {
                from.CloseGump(typeof(ReagentDisplayGump2));
                from.SendGump(new ReagentDisplayGump2(from));
            }
            else if (slot == 2)
            {
                from.CloseGump(typeof(ReagentDisplayGump3));
                from.SendGump(new ReagentDisplayGump3(from));
            }
            else if (slot == 3)
            {
                from.CloseGump(typeof(ReagentDisplayGump4));
                from.SendGump(new ReagentDisplayGump4(from));
            }
        }
    }

    public class ReagentCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("reagents", AccessLevel.Player, new CommandEventHandler(Reagents_OnCommand));
            CommandSystem.Register("reagentbar1", AccessLevel.Player, new CommandEventHandler(ReagentBar1_OnCommand));
            CommandSystem.Register("reagentbar2", AccessLevel.Player, new CommandEventHandler(ReagentBar2_OnCommand));
            CommandSystem.Register("reagentbar3", AccessLevel.Player, new CommandEventHandler(ReagentBar3_OnCommand));
            CommandSystem.Register("reagentbar4", AccessLevel.Player, new CommandEventHandler(ReagentBar4_OnCommand));
            CommandSystem.Register("mreagents", AccessLevel.Player, new CommandEventHandler(MageryLegacy));
            CommandSystem.Register("nreagents", AccessLevel.Player, new CommandEventHandler(NecroLegacy));
            CommandSystem.Register("areagents", AccessLevel.Player, new CommandEventHandler(AlchemyLegacy));
            CommandSystem.Register("creagents", AccessLevel.Player, new CommandEventHandler(CloseAll));
        }

        private static void Reagents_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            ToolBarUpdates.ForgetToolbar(from, "reagents");
            from.CloseGump(typeof(ReagentManagerGump));
            from.SendGump(new ReagentManagerGump(from));
        }

        private static void ReagentBar1_OnCommand(CommandEventArgs e) { ReagentToolbarSystem.OpenBar(e.Mobile, 0, false); }
        private static void ReagentBar2_OnCommand(CommandEventArgs e) { ReagentToolbarSystem.OpenBar(e.Mobile, 1, false); }
        private static void ReagentBar3_OnCommand(CommandEventArgs e) { ReagentToolbarSystem.OpenBar(e.Mobile, 2, false); }
        private static void ReagentBar4_OnCommand(CommandEventArgs e) { ReagentToolbarSystem.OpenBar(e.Mobile, 3, false); }

        private static void MageryLegacy(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            ToolBarUpdates.RememberToolbar(from, "reagents");
            ReagentToolbarState state = ReagentToolbarSystem.GetState(from);
            ReagentToolbarSystem.ApplyGroup(state.Bars[0], ReagentGroup.Magery);
            ReagentToolbarSystem.OpenBar(from, 0);
        }

        private static void NecroLegacy(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            ToolBarUpdates.RememberToolbar(from, "reagents");
            ReagentToolbarState state = ReagentToolbarSystem.GetState(from);
            ReagentToolbarSystem.ApplyGroup(state.Bars[1], ReagentGroup.Necromancy);
            ReagentToolbarSystem.OpenBar(from, 1);
        }

        private static void AlchemyLegacy(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            ToolBarUpdates.RememberToolbar(from, "reagents");
            ReagentToolbarState state = ReagentToolbarSystem.GetState(from);
            ReagentToolbarSystem.ApplyGroup(state.Bars[2], ReagentGroup.Alchemy);
            ReagentToolbarSystem.OpenBar(from, 2);
        }

        private static void CloseAll(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            ToolBarUpdates.ForgetToolbar(from, "reagents");
            ToolBarUpdates.ForgetToolbar(from, "reagentbar1");
            ToolBarUpdates.ForgetToolbar(from, "reagentbar2");
            ToolBarUpdates.ForgetToolbar(from, "reagentbar3");
            ToolBarUpdates.ForgetToolbar(from, "reagentbar4");
            ToolBarUpdates.ForgetToolbar(from, "mreagents");
            ToolBarUpdates.ForgetToolbar(from, "nreagents");
            ToolBarUpdates.ForgetToolbar(from, "areagents");
            from.CloseGump(typeof(ReagentDisplayGump1));
            from.CloseGump(typeof(ReagentDisplayGump2));
            from.CloseGump(typeof(ReagentDisplayGump3));
            from.CloseGump(typeof(ReagentDisplayGump4));
            from.CloseGump(typeof(ReagentManagerGump));
            from.CloseGump(typeof(ReagentSelectionGump));
        }
    }

    public class ReagentManagerGump : Gump
    {
        private readonly Mobile m_From;

        public ReagentManagerGump(Mobile from) : base(50, 50)
        {
            m_From = from;
            ReagentToolbarState state = ReagentToolbarSystem.GetState(from);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 840, 390, 9270);
            AddImageTiled(20, 20, 800, 350, 2624);
            AddAlphaRegion(20, 20, 800, 350);
            AddLabel(320, 25, 0x34, "Reagent Toolbar Manager");
            AddLabel(30, 44, 0x58, "Use one command: [reagents]");

            for (int i = 0; i < state.Bars.Count; i++)
            {
                int y = 80 + (i * 56);
                int buttonY = y + 12;
                ReagentBarSettings bar = state.Bars[i];

                AddImageTiled(30, y, 780, 44, 2624);
                AddAlphaRegion(30, y, 780, 44);

                AddLabel(40, y + 3, 0x2, "Bar " + (i + 1));
                AddLabel(95, y + 3, 0x58, bar.Vertical ? "Vertical" : "Horizontal");
                AddLabel(190, y + 3, 0x58, bar.Keys.Count + " reagents");
                AddLabel(40, y + 21, 0x58, bar.ShowNames ? "Names: On" : "Names: Off");
                string layout = "Alt";
                if (bar.NameLayout == 1)
                    layout = "Top";
                else if (bar.NameLayout == 2)
                    layout = "Bottom";
                AddLabel(145, y + 21, 0x58, "Text: " + layout);

                AddButton(360, buttonY, 4005, 4007, 1000 + i, GumpButtonType.Reply, 0);
                AddLabel(394, buttonY, 0x34, "Open");

                AddButton(440, buttonY, 4005, 4007, 1100 + i, GumpButtonType.Reply, 0);
                AddLabel(474, buttonY, 0x34, "Orient");

                AddButton(530, buttonY, 4005, 4007, 1200 + i, GumpButtonType.Reply, 0);
                AddLabel(564, buttonY, 0x34, "Customize");

                AddButton(650, buttonY, 4005, 4007, 1300 + i, GumpButtonType.Reply, 0);
                AddLabel(684, buttonY, 0x34, "Names");

                AddButton(735, buttonY, 4005, 4007, 1400 + i, GumpButtonType.Reply, 0);
                AddLabel(769, buttonY, 0x34, "Text");
            }

            AddButton(40, 332, 4017, 4019, 2001, GumpButtonType.Reply, 0);
            AddLabel(75, 332, 0x34, "Set all = ALL reagents");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            ReagentToolbarState state = ReagentToolbarSystem.GetState(from);
            int id = info.ButtonID;

            if (id == 0)
                return;

            if (id >= 1000 && id < 1100)
            {
                ReagentToolbarSystem.OpenBar(from, id - 1000);
            }
            else if (id >= 1100 && id < 1200)
            {
                int slot = id - 1100;
                if (slot >= 0 && slot < state.Bars.Count)
                {
                    state.Bars[slot].Vertical = !state.Bars[slot].Vertical;
                }
            }
            else if (id >= 1200 && id < 1300)
            {
                int slot = id - 1200;
                from.CloseGump(typeof(ReagentManagerGump));
                from.CloseGump(typeof(ReagentSelectionGump));
                from.SendGump(new ReagentSelectionGump(from, slot, ReagentGroup.All));
                return;
            }
            else if (id >= 1300 && id < 1400)
            {
                int slot = id - 1300;
                if (slot >= 0 && slot < state.Bars.Count)
                    state.Bars[slot].ShowNames = !state.Bars[slot].ShowNames;
            }
            else if (id >= 1400 && id < 1500)
            {
                int slot = id - 1400;
                if (slot >= 0 && slot < state.Bars.Count)
                {
                    state.Bars[slot].NameLayout++;
                    if (state.Bars[slot].NameLayout > 2)
                        state.Bars[slot].NameLayout = 0;
                }
            }
            else if (id == 2001)
            {
                for (int i = 0; i < state.Bars.Count; i++)
                {
                    ReagentToolbarSystem.ApplyGroup(state.Bars[i], ReagentGroup.All);
                }
            }

            from.CloseGump(typeof(ReagentManagerGump));
            from.SendGump(new ReagentManagerGump(from));
        }
    }

    public class ReagentSelectionGump : Gump
    {
        private readonly int m_Slot;
        private readonly ReagentGroup m_Filter;

        public ReagentSelectionGump(Mobile from, int slot, ReagentGroup filter) : base(70, 70)
        {
            m_Slot = slot;
            m_Filter = filter;
            ReagentToolbarState state = ReagentToolbarSystem.GetState(from);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 610, 380, 9270);
            AddLabel(170, 15, 1152, "Customize Reagent Bar " + (slot + 1));
            AddLabel(390, 15, 2101, "Filter:");
            AddLabel(440, 15, 88, filter.ToString());
            AddButton(20, 15, 4005, 4007, 2110, GumpButtonType.Reply, 0);
            AddLabel(50, 15, 88, "Clear Checks");

            const int maxRows = 12;
            const int colWidth = 145;
            int displayed = 0;

            for (int i = 0; i < ReagentToolbarSystem.Definitions.Count; i++)
            {
                ReagentDefinition def = ReagentToolbarSystem.Definitions[i];

                if (filter != ReagentGroup.All && (def.Groups & filter) == 0)
                    continue;

                int row = displayed % maxRows;
                int col = displayed / maxRows;
                int x = 20 + (col * colWidth);
                int y = 45 + (row * 20);
                bool selected = state.Bars[slot].Keys.Contains(def.Key);

                AddButton(x, y, selected ? 211 : 210, selected ? 210 : 211, 3000 + i, GumpButtonType.Reply, 0);
                AddLabel(x + 25, y, 2101, def.Name);
                displayed++;
            }

            AddButton(20, 335, 4005, 4007, 2101, GumpButtonType.Reply, 0);
            AddLabel(50, 335, 88, "Magery");
            AddButton(105, 335, 4005, 4007, 2102, GumpButtonType.Reply, 0);
            AddLabel(135, 335, 88, "Necro");
            AddButton(190, 335, 4005, 4007, 2103, GumpButtonType.Reply, 0);
            AddLabel(220, 335, 88, "Alchemy");
            AddButton(295, 335, 4005, 4007, 2104, GumpButtonType.Reply, 0);
            AddLabel(325, 335, 88, "Unique");
            AddButton(385, 335, 4005, 4007, 2105, GumpButtonType.Reply, 0);
            AddLabel(415, 335, 88, "Special");
            AddButton(475, 335, 4005, 4007, 2106, GumpButtonType.Reply, 0);
            AddLabel(505, 335, 88, "All");

            AddButton(20, 355, 4017, 4019, 2999, GumpButtonType.Reply, 0);
            AddLabel(55, 355, 1152, "Back");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            ReagentToolbarState state = ReagentToolbarSystem.GetState(from);
            ReagentBarSettings bar = state.Bars[m_Slot];
            int id = info.ButtonID;

            if (id >= 3000 && id < 4000)
            {
                int index = id - 3000;
                if (index >= 0 && index < ReagentToolbarSystem.Definitions.Count)
                {
                    string key = ReagentToolbarSystem.Definitions[index].Key;
                    if (bar.Keys.Contains(key))
                        bar.Keys.Remove(key);
                    else
                        bar.Keys.Add(key);
                }
            }
            else if (id == 2101) { from.CloseGump(typeof(ReagentSelectionGump)); from.SendGump(new ReagentSelectionGump(from, m_Slot, ReagentGroup.Magery)); return; }
            else if (id == 2102) { from.CloseGump(typeof(ReagentSelectionGump)); from.SendGump(new ReagentSelectionGump(from, m_Slot, ReagentGroup.Necromancy)); return; }
            else if (id == 2103) { from.CloseGump(typeof(ReagentSelectionGump)); from.SendGump(new ReagentSelectionGump(from, m_Slot, ReagentGroup.Alchemy)); return; }
            else if (id == 2104) { from.CloseGump(typeof(ReagentSelectionGump)); from.SendGump(new ReagentSelectionGump(from, m_Slot, ReagentGroup.Unique)); return; }
            else if (id == 2105) { from.CloseGump(typeof(ReagentSelectionGump)); from.SendGump(new ReagentSelectionGump(from, m_Slot, ReagentGroup.Special)); return; }
            else if (id == 2106) { from.CloseGump(typeof(ReagentSelectionGump)); from.SendGump(new ReagentSelectionGump(from, m_Slot, ReagentGroup.All)); return; }
            else if (id == 2110)
            {
                bar.Keys.Clear();
                from.CloseGump(typeof(ReagentSelectionGump));
                from.SendGump(new ReagentSelectionGump(from, m_Slot, m_Filter));
                return;
            }
            else if (id == 2999)
            {
                from.CloseGump(typeof(ReagentSelectionGump));
                from.SendGump(new ReagentManagerGump(from));
                return;
            }
            else if (id == 0)
            {
                from.CloseGump(typeof(ReagentSelectionGump));
                from.CloseGump(typeof(ReagentManagerGump));
                from.SendGump(new ReagentManagerGump(from));
                return;
            }

            from.CloseGump(typeof(ReagentSelectionGump));
            from.SendGump(new ReagentSelectionGump(from, m_Slot, m_Filter));
        }
    }

    public class ReagentDisplayGump : Gump
    {
        private readonly int m_Slot;

        public ReagentDisplayGump(Mobile from, int slot) : base(25 + (slot * 35), 25 + (slot * 35))
        {
            m_Slot = slot;
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            ReagentToolbarState state = ReagentToolbarSystem.GetState(from);
            ReagentBarSettings bar = state.Bars[slot];
            List<ReagentDefinition> defs = new List<ReagentDefinition>();

            for (int i = 0; i < bar.Keys.Count; i++)
            {
                ReagentDefinition def = ReagentToolbarSystem.Find(bar.Keys[i]);
                if (def != null)
                    defs.Add(def);
            }

            if (defs.Count == 0)
            {
                AddBackground(0, 0, 190, 70, 9270);
                AddLabel(15, 15, 33, "No reagents selected for bar " + (slot + 1));
                AddLabel(15, 35, 1152, "Use [reagents to customize.");
                return;
            }

            int width = bar.Vertical ? 190 : (defs.Count * 110) + 40;
            int height = bar.Vertical ? (defs.Count * 42) + 20 : 85;

            if (bar.Vertical)
            {
                for (int y = 0; y <= height; y += 30)
                {
                    AddImage(25, y, 156);
                }
            }
            else
            {
                for (int x = 0; x <= width; x += 30)
                {
                    AddImage(x, 25, 156);
                }
            }

            for (int i = 0; i < defs.Count; i++)
            {
                ReagentDefinition def = defs[i];
                int x = bar.Vertical ? 18 : 10 + (i * 110);
                int y = bar.Vertical ? 5 + (i * 42) : 30;

                int amount = from.Backpack == null ? 0 : from.Backpack.GetAmount(def.Type, true);
                string color = amount < 10 ? "#FFA200" : "#FCFF00";

                AddItem(x, y, def.ItemID, def.Hue);
                AddHtml(x + 38, y + 2, 56, 21, "<BASEFONT COLOR=" + color + "><BIG>" + amount + "</BIG></BASEFONT>", false, false);

                if (!bar.Vertical && bar.ShowNames)
                {
                    bool topName = ((i % 2) == 0);
                    if (bar.NameLayout == 1)
                        topName = true;
                    else if (bar.NameLayout == 2)
                        topName = false;

                    int nameY = topName ? 2 : 60;
                    int lineY = topName ? 18 : 50;
                    AddLabel(x + 20, lineY, 1152, "|");
                    AddLabel(x - 5, nameY, 0x481, def.Name);
                }
                else if (bar.Vertical && bar.ShowNames)
                {
                    AddLabel(x + 58, y + 10, 1152, "-");
                    AddLabel(x + 70, y + 10, 0x481, def.Name);
                }
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0)
            {
                Mobile from = sender.Mobile;
                ToolBarUpdates.ForgetToolbar(from, "reagentbar" + (m_Slot + 1));
                return;
            }
        }
    }

    public class ReagentDisplayGump1 : ReagentDisplayGump { public ReagentDisplayGump1(Mobile from) : base(from, 0) { } }
    public class ReagentDisplayGump2 : ReagentDisplayGump { public ReagentDisplayGump2(Mobile from) : base(from, 1) { } }
    public class ReagentDisplayGump3 : ReagentDisplayGump { public ReagentDisplayGump3(Mobile from) : base(from, 2) { } }
    public class ReagentDisplayGump4 : ReagentDisplayGump { public ReagentDisplayGump4(Mobile from) : base(from, 3) { } }

    // Backward-compatible shim used by crafting/spell systems to refresh open reagent bars.
    public class MReagentGump : Gump
    {
        public MReagentGump(Mobile from) : base(0, 0)
        {
        }

        private static bool IsBarRemembered(Mobile from, int slot)
        {
            CharacterDatabase db = CharacterDatabase.GetDB(from);

            if (db == null || String.IsNullOrEmpty(db.OpenToolbars))
                return false;

            string key = "reagentbar" + (slot + 1);
            string[] bars = db.OpenToolbars.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < bars.Length; i++)
            {
                if (String.Equals(bars[i], key, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static void XReagentGump(Mobile from)
        {
            if (from == null || !(from is PlayerMobile))
                return;

            ReagentToolbarState state = ReagentToolbarSystem.GetState(from);
            for (int i = 0; i < state.Bars.Count; i++)
            {
                if (IsBarRemembered(from, i))
                    ReagentToolbarSystem.OpenBar(from, i, false);
            }
        }
    }

}
