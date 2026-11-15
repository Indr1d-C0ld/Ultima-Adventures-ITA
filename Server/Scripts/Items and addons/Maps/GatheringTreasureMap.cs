using System;
using System.Collections.Generic;
using Server;
using Server.ContextMenus;
using Server.Engines.Harvest;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public class GatheringTreasureMap : MapItem
    {
        private DateTime m_Found;
        private bool m_Completed;
        private Mobile m_CompletedBy;
        private Mobile m_Decoder;
        private Map m_Map;
        private Point2D m_Location;
        private SkillName m_Skill;
        private string m_ResourceTypeName;

        private int m_ChestLevel;

        [CommandProperty(AccessLevel.GameMaster)]
        public int ChestLevel
        {
            get { return m_ChestLevel; }
            set { m_ChestLevel = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime Found
        {
            get { return m_Found; }
            set { m_Found = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Completed
        {
            get { return m_Completed; }
            set { m_Completed = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile CompletedBy
        {
            get { return m_CompletedBy; }
            set { m_CompletedBy = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Decoder
        {
            get { return m_Decoder; }
            set { m_Decoder = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Map TargetMap
        {
            get { return m_Map; }
            set { m_Map = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point2D TargetLocation
        {
            get { return m_Location; }
            set { m_Location = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName GatheringSkill
        {
            get { return m_Skill; }
            set { m_Skill = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ResourceTypeName
        {
            get { return m_ResourceTypeName; }
            set { m_ResourceTypeName = value; InvalidateProperties(); }
        }

        public Type ResourceType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(m_ResourceTypeName))
                    return null;

                return ScriptCompiler.FindTypeByName(m_ResourceTypeName);
            }
        }

        [Constructable]
        public GatheringTreasureMap(SkillName skill, Mobile owner)
        {
            if (owner == null)
                return;

            Initialize(skill, owner.Map, owner.Location, owner.X, owner.Y, null);
        }

        public GatheringTreasureMap(SkillName skill, Map map, Point3D location, int x, int y, Mobile decoder)
        {
            Initialize(skill, map, location, x, y, decoder);
        }

        public GatheringTreasureMap(Serial serial) : base(serial)
        {
        }

        private void Initialize(SkillName skill, Map map, Point3D location, int x, int y, Mobile decoder)
        {
            if (map != Map.Felucca && map != Map.Trammel && map != Map.Ilshenar && map != Map.Malas && map != Map.Tokuno && map != Map.TerMur)
                map = Map.Trammel;

            if (skill != SkillName.Mining && skill != SkillName.Lumberjacking)
                skill = SkillName.Mining;

            LootType = LootType.Blessed;
            Name = "a gathering treasure map";
            m_Skill = skill;
            m_Decoder = decoder;
            m_Found = DateTime.UtcNow;

            string world = Worlds.GetMyWorld(map, location, x, y);
            if (world == "the Town of Skara Brae")
                world = "the Land of Sosaria";
            else if (world == "the Moon of Luna")
                world = "the Land of Sosaria";
            else if (world == "the Bottle World of Kuldar")
                world = "the Serpent Island";

            map = Worlds.GetMyDefaultMap(world);

            HarvestDefinition def = GetHarvestDefinition(skill);
            Point3D loc = FindHarvestLocation(def, map, world);

            TargetLocation = new Point2D(loc.X, loc.Y);
            DisplayMap = TargetMap = map;

            HarvestResource selectedResource = GetHarvestResource(def, map, loc);
            m_ResourceTypeName = GetResourceTypeName(selectedResource);
            m_ChestLevel = GetChestLevel(def, selectedResource);

            UpdateGatheringMap(this);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return;
            }

            if (!m_Completed && m_Decoder == null)
                Decode(from);
            else
                DisplayTo(from);
        }

        public override void DisplayTo(Mobile from)
        {
            if (m_Completed)
            {
                from.SendMessage("This gathering treasure map has already been completed.");
            }
            else if (m_Decoder != null && m_Decoder != from)
            {
                from.SendMessage("Only the person who decoded this map may complete it.");
                return;
            }
            else
            {
                from.SendMessage("The resource node is marked by the red pin. Grab a shovel and dig it up!");
            }

            from.PlaySound(0x249);
            base.DisplayTo(from);
        }

        private void Decode(Mobile from)
        {
            if (m_Completed || m_Decoder != null)
                return;

            m_Decoder = from;
            m_Found = DateTime.UtcNow;
            from.SendMessage("You successfully decode the gathering treasure map.");
            DisplayTo(from);
        }

        public void OnBeginDig(Mobile from)
        {
            if (m_Completed)
            {
                from.SendLocalizedMessage(503028); // The treasure for this map has already been found.
            }
            else if (m_Decoder != null && from != m_Decoder)
            {
                from.SendMessage("Only the person who decoded this map may dig up the treasure.");
            }
            else if (!from.CanBeginAction(typeof(GatheringTreasureMap)))
            {
                from.SendMessage("You are already digging up treasure.");
            }
            else if (from.Map != this.m_Map)
            {
                from.SendLocalizedMessage(1010479); // You seem to be in the right place, but may be on the wrong facet!
            }
            else
            {
                from.SendMessage("Where do you wish to dig?");
                from.Target = new DigTarget(this);
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            Point3D loc = new Point3D(m_Location.X, m_Location.Y, 0);
            string world = Worlds.GetMyWorld(m_Map, loc, m_Location.X, m_Location.Y);
            string mDesc = "Somewhere in " + world;

            list.Add(1053099, String.Format("<BASEFONT COLOR=#DDCC22>\t{0}<BASEFONT Color=#FBFBFB>", mDesc));

            string resourceName = GetResourceDisplayName();
            if (!string.IsNullOrWhiteSpace(resourceName))
                list.Add(String.Format("Resource: {0}", resourceName));

            if (m_Completed)
                list.Add(1041507, m_CompletedBy == null ? "someone" : m_CompletedBy.Name); // completed by ~1_val~
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (!m_Completed)
            {
                if (m_Decoder == null)
                {
                    list.Add(new DecodeMapEntry(this));
                }
                else
                {
                    bool digTool = TreasureMap.HasDiggingTool(from);
                    list.Add(new OpenMapEntry(this));
                    list.Add(new DigEntry(this, digTool));
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);
            writer.Write((Mobile)m_CompletedBy);
            writer.Write(m_Completed);
            writer.Write(m_Decoder);
            writer.Write(m_Map);
            writer.Write(m_Location);
            writer.Write((int)m_Skill);
            writer.Write(m_ResourceTypeName);
            writer.Write(m_ChestLevel);
            writer.Write((DateTime)m_Found);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                {
                    m_CompletedBy = reader.ReadMobile();
                    m_Completed = reader.ReadBool();
                    m_Decoder = reader.ReadMobile();
                    m_Map = reader.ReadMap();
                    m_Location = reader.ReadPoint2D();
                    m_Skill = (SkillName)reader.ReadInt();
                    m_ResourceTypeName = reader.ReadString();
                    m_ChestLevel = reader.ReadInt();
                    m_Found = reader.ReadDateTime();
                    break;
                }
                case 0:
                {
                    m_CompletedBy = reader.ReadMobile();
                    m_Completed = reader.ReadBool();
                    m_Decoder = reader.ReadMobile();
                    m_Map = reader.ReadMap();
                    m_Location = reader.ReadPoint2D();
                    m_Skill = (SkillName)reader.ReadInt();
                    m_ResourceTypeName = reader.ReadString();
                    m_Found = reader.ReadDateTime();
                    break;
                }
            }

            if (version == 0)
            {
                HarvestDefinition def = GetHarvestDefinition(m_Skill);
                HarvestResource resource = GetHarvestResource(def, m_Map, new Point3D(m_Location, 0));
                m_ChestLevel = GetChestLevel(def, resource);
            }

            if (DisplayMap != m_Map)
                DisplayMap = m_Map;
        }

        public static bool TryHarvestDig(Mobile from, IPoint3D targeted)
        {
            if (from == null || targeted == null)
                return false;

            Container pack = from.Backpack;
            if (pack == null)
                return false;

            Point3D targ3D = GetTargetPoint(targeted);
            List<GatheringTreasureMap> maps = pack.FindItemsByType<GatheringTreasureMap>();

            foreach (GatheringTreasureMap map in maps)
            {
                if (map == null || map.Deleted)
                    continue;

                Point3D chest3D0 = new Point3D(map.m_Location, 0);

                if (Utility.InRange(targ3D, chest3D0, 8))
                {
                    if (map.TryToDig(from, targeted, true))
                        return true;
                }
            }

            return false;
        }

        private void GiveReward(Container container)
        {
            if (container == null)
                return;

            ClearExistingLoot(container);

            Type rewardType = ResourceType;
            if (rewardType == null)
                return;

            Item reward = Activator.CreateInstance(rewardType) as Item;
            if (reward == null)
                return;

            int amount = GetRewardAmount();
            if (reward.Stackable)
                reward.Amount = amount;

            container.DropItem(reward);
        }

        private static void ClearExistingLoot(Container container)
        {
            List<Item> items = container.FindItemsByType<Item>(false);

            for (int i = items.Count - 1; i >= 0; i--)
            {
                Item item = items[i];

                if (item != null && !item.Deleted)
                    item.Delete();
            }
        }

        private int GetRewardAmount()
        {
            if (m_Skill == SkillName.Lumberjacking)
                return 300;

            return 150;
        }

        private string GetResourceDisplayName()
        {
            Type resourceType = ResourceType;
            if (resourceType == null)
                return string.Empty;

            return resourceType.Name;
        }

        private static string GetResourceTypeName(HarvestResource resource)
        {
            if (resource == null || resource.Types == null || resource.Types.Length == 0)
                return string.Empty;

            return resource.Types[0].Name;
        }

        private static HarvestDefinition GetHarvestDefinition(SkillName skill)
        {
            if (skill == SkillName.Mining)
                return Mining.System.OreAndStone;

            return Lumberjacking.System.Definition;
        }

        private static HarvestResource GetHarvestResource(HarvestDefinition def, Map map, Point3D loc)
        {
            if (def == null || map == null)
                return null;

            HarvestBank bank = def.GetBank(map, loc.X, loc.Y);
            HarvestVein vein = bank == null ? null : bank.Vein;
            HarvestResource resource = vein == null ? null : vein.PrimaryResource;

            if (resource != null)
                return resource;

            if (def.Resources != null && def.Resources.Length > 0)
                return def.Resources[0];

            return null;
        }

        private static int GetChestLevel(HarvestDefinition def, HarvestResource resource)
        {
            int level = 1;

            if (def != null && def.Resources != null && resource != null)
            {
                int resourceIndex = Array.IndexOf(def.Resources, resource);
                int maxIndex = def.Resources.Length - 1;

                if (maxIndex > 0 && resourceIndex >= 0)
                {
                    double normalized = resourceIndex / (double)maxIndex;

                    if (normalized >= 0.9)
                        level = 5;
                    else if (normalized >= 0.7)
                        level = 4;
                    else if (normalized >= 0.5)
                        level = 3;
                    else if (normalized >= 0.3)
                        level = 2;
                }
            }

            return level;
        }

        private static Point3D FindHarvestLocation(HarvestDefinition def, Map map, string world)
        {
            if (def == null || map == null)
                return Worlds.GetRandomLocation(world, "land");

            for (int i = 0; i < 200; i++)
            {
                Point3D candidate = Worlds.GetRandomLocation(world, "land");
                LandTile land = map.Tiles.GetLandTile(candidate.X, candidate.Y);

                if (def.Validate(land.ID))
                    return candidate;

                StaticTile[] statics = map.Tiles.GetStaticTiles(candidate.X, candidate.Y, true);

                for (int j = 0; j < statics.Length; j++)
                {
                    int tileID = (statics[j].ID & 0x3FFF) | 0x4000;

                    if (def.Validate(tileID))
                        return candidate;
                }
            }

            return Worlds.GetRandomLocation(world, "land");
        }

        private static Point3D GetTargetPoint(IPoint3D targeted)
        {
            Item item = targeted as Item;

            if (item != null)
                return item.GetWorldLocation();

            return new Point3D(targeted);
        }

        private bool TryToDig(Mobile from, IPoint3D targeted, bool requireProximity)
        {
            if (from == null || targeted == null || Deleted)
                return false;

            Map map = this.Map;
            Map chestMap = m_Map;

            Point3D targ3D = GetTargetPoint(targeted);
            Point3D chest3D0 = new Point3D(m_Location, 0);

            if (requireProximity && !Utility.InRange(targ3D, chest3D0, 8))
                return false;

            if (m_Completed)
            {
                from.SendLocalizedMessage(503028); // The treasure for this map has already been found.
                return true;
            }
            else if (m_Decoder != null && from != m_Decoder)
            {
                from.SendMessage("Only the person who decoded this map may dig up the treasure.");
                return true;
            }
            else if (!from.CanBeginAction(typeof(GatheringTreasureMap)))
            {
                from.SendMessage("You are already digging up treasure.");
                return true;
            }
            else if (from.Map != chestMap)
            {
                from.SendLocalizedMessage(1010479); // You seem to be in the right place, but may be on the wrong facet!
                return true;
            }
            else if (!TreasureMap.HasDiggingTool(from))
            {
                from.SendMessage("You must have a digging tool to dig for treasure.");
                return true;
            }
            else if (from.Map != map)
            {
                from.SendLocalizedMessage(1010479); // You seem to be in the right place, but may be on the wrong facet!
                return true;
            }

            int maxRange;
            double skillValue = from.Skills[SkillName.Mining].Value;

            if (skillValue >= 100.0)
                maxRange = 4;
            else if (skillValue >= 81.0)
                maxRange = 3;
            else if (skillValue >= 51.0)
                maxRange = 2;
            else
                maxRange = 1;

            Point2D loc = m_Location;
            int x = loc.X, y = loc.Y;

            if (Utility.InRange(targ3D, chest3D0, maxRange))
            {
                if (from.Location.X == x && from.Location.Y == y)
                {
                    from.SendLocalizedMessage(503030); // The chest can't be dug up because you are standing on top of it.
                }
                else if (chestMap != null)
                {
                    int z = chestMap.GetAverageZ(x, y);

                    if (!chestMap.CanFit(x, y, z, 16, true, true))
                    {
                        from.SendLocalizedMessage(503021); // You have found the treasure chest but something is keeping it from being dug up.
                    }
                    else if (from.BeginAction(typeof(GatheringTreasureMap)))
                    {
                        new DigTimer(from, this, new Point3D(x, y, z), chestMap).Start();
                    }
                    else
                    {
                        from.SendMessage("You are already digging up treasure.");
                    }
                }

                return true;
            }
            else
            {
                if (Utility.InRange(targ3D, chest3D0, 8))
                    from.SendLocalizedMessage(503032); // You dig and dig but no treasure seems to be here.
                else
                    from.SendLocalizedMessage(503035); // You dig and dig but fail to find any treasure.

                return true;
            }
        }

        private static void UpdateGatheringMap(GatheringTreasureMap map)
        {
            Map targetMap = map.TargetMap;
            Point2D loc = map.TargetLocation;
            Point3D location = new Point3D(loc.X, loc.Y, 0);
            string world = Worlds.GetMyWorld(targetMap, location, loc.X, loc.Y);

            map.Width = 300;
            map.Height = 300;

            int width = 600;
            int height = 600;

            int x1 = loc.X - Utility.RandomMinMax(width / 4, (width / 4) * 3);
            int y1 = loc.Y - Utility.RandomMinMax(height / 4, (height / 4) * 3);

            if (x1 < 0)
                x1 = 0;
            if (y1 < 0)
                y1 = 0;

            if (world == "the Land of Ambrosia") { if (x1 < 5122) { x1 = 5122; } if (y1 < 3036) { y1 = 3036; } }
            else if (world == "the Island of Umber Veil") { if (x1 < 699) { x1 = 699; } if (y1 < 3129) { y1 = 3129; } }
            else if (world == "the Bottle World of Kuldar") { if (x1 < 6127) { x1 = 6127; } if (y1 < 828) { y1 = 828; } }
            else if (world == "the Savaged Empire") { if (x1 < 136) { x1 = 136; } if (y1 < 8) { y1 = 8; } }

            int x2 = x1 + width;
            int y2 = y1 + height;

            if (x2 > Map.Maps[targetMap.MapID].Width)
                x2 = Map.Maps[targetMap.MapID].Width;

            if (y2 > Map.Maps[targetMap.MapID].Height)
                y2 = Map.Maps[targetMap.MapID].Height;

            if (world == "the Moon of Luna") { if (x2 >= 6125) { x2 = 6125; } if (y2 >= 3034) { y2 = 3034; } }
            else if (world == "the Land of Ambrosia") { if (x2 >= 6126) { x2 = 6126; } if (y2 >= 4095) { y2 = 4095; } }
            else if (world == "the Island of Umber Veil") { if (x2 >= 2272) { x2 = 2272; } if (y2 >= 4095) { y2 = 4095; } }
            else if (world == "the Bottle World of Kuldar") { if (x2 >= 7167) { x2 = 7167; } if (y2 >= 2742) { y2 = 2742; } }
            else if (world == "the Land of Lodoria") { if (x2 >= 5120) { x2 = 5119; } if (y2 >= 4096) { y2 = 4095; } }
            else if (world == "the Land of Sosaria") { if (x2 >= 5119) { x2 = 5118; } if (y2 >= 3127) { y2 = 3126; } }
            else if (world == "the Underworld") { if (x2 >= 1581) { x2 = 1581; } if (y2 >= 1599) { y2 = 1599; } }
            else if (world == "the Serpent Island") { if (x2 >= 1870) { x2 = 1869; } if (y2 >= 2047) { y2 = 2046; } }
            else if (world == "the Isles of Dread") { if (x2 >= 1447) { x2 = 1446; } if (y2 >= 1447) { y2 = 1446; } }
            else if (world == "the Savaged Empire") { if (x2 >= 1160) { x2 = 1159; } if (y2 >= 1792) { y2 = 1791; } }

            x1 = x2 - width;
            y1 = y2 - height;

            map.Bounds = new Rectangle2D(x1, y1, width, height);

            map.ClearPins();
            map.Protected = true;

            if (map.Pins.Count > 0)
                map.ChangePin(1, loc.X, loc.Y);
            else
                map.AddWorldPin(loc.X, loc.Y);
        }

        private class DecodeMapEntry : ContextMenuEntry
        {
            private readonly GatheringTreasureMap m_Map;

            public DecodeMapEntry(GatheringTreasureMap map) : base(6147, 2)
            {
                m_Map = map;
            }

            public override void OnClick()
            {
                if (!m_Map.Deleted)
                    m_Map.Decode(Owner.From);
            }
        }

        private class OpenMapEntry : ContextMenuEntry
        {
            private readonly GatheringTreasureMap m_Map;

            public OpenMapEntry(GatheringTreasureMap map) : base(6150, 2)
            {
                m_Map = map;
            }

            public override void OnClick()
            {
                if (!m_Map.Deleted)
                    m_Map.DisplayTo(Owner.From);
            }
        }

        private class DigEntry : ContextMenuEntry
        {
            private readonly GatheringTreasureMap m_Map;

            public DigEntry(GatheringTreasureMap map, bool enabled) : base(6148, 2)
            {
                m_Map = map;

                if (!enabled)
                    Flags |= CMEFlags.Disabled;
            }

            public override void OnClick()
            {
                if (m_Map.Deleted)
                    return;

                Mobile from = Owner.From;

                if (TreasureMap.HasDiggingTool(from))
                    m_Map.OnBeginDig(from);
                else
                    from.SendMessage("You must have a digging tool to dig for treasure.");
            }
        }

        private class DigTarget : Target
        {
            private readonly GatheringTreasureMap m_Map;

            public DigTarget(GatheringTreasureMap map) : base(6, true, TargetFlags.None)
            {
                m_Map = map;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Map.Deleted)
                    return;

                IPoint3D p = targeted as IPoint3D;

                if (p != null)
                    m_Map.TryToDig(from, p, false);
            }
        }

        private class DigTimer : Timer
        {
            private readonly Mobile m_From;
            private readonly GatheringTreasureMap m_TreasureMap;
            private readonly Point3D m_Location;
            private readonly Map m_Map;

            private TreasureChestDirt m_Dirt1;
            private TreasureChestDirt m_Dirt2;
            private TreasureMapChest m_Chest;

            private int m_Count;

            private readonly long m_NextSkillTime;
            private readonly long m_NextSpellTime;
            private readonly long m_NextActionTime;
            private readonly long m_LastMoveTime;

            public DigTimer(Mobile from, GatheringTreasureMap treasureMap, Point3D location, Map map) : base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0))
            {
                m_From = from;
                m_TreasureMap = treasureMap;
                m_Location = location;
                m_Map = map;

                m_NextSkillTime = from.NextSkillTime;
                m_NextSpellTime = from.NextSpellTime;
                m_NextActionTime = from.NextActionTime;
                m_LastMoveTime = from.LastMoveTime;

                Priority = TimerPriority.TenMS;
            }

            private void Terminate()
            {
                Stop();
                m_From.EndAction(typeof(GatheringTreasureMap));

                if (m_Chest != null)
                    m_Chest.Delete();

                if (m_Dirt1 != null)
                {
                    m_Dirt1.Delete();
                    m_Dirt2.Delete();
                }
            }

            protected override void OnTick()
            {
                if (m_NextSkillTime != m_From.NextSkillTime || m_NextSpellTime != m_From.NextSpellTime || m_NextActionTime != m_From.NextActionTime)
                {
                    Terminate();
                    return;
                }

                if (m_LastMoveTime != m_From.LastMoveTime)
                {
                    m_From.SendLocalizedMessage(503023); // You cannot move around while digging up treasure. You will need to start digging anew.
                    Terminate();
                    return;
                }

                int z = (m_Chest != null) ? m_Chest.Z + m_Chest.ItemData.Height : int.MinValue;
                int height = 16;

                if (z > m_Location.Z)
                    height -= (z - m_Location.Z);
                else
                    z = m_Location.Z;

                if (!m_Map.CanFit(m_Location.X, m_Location.Y, z, height, true, true, false))
                {
                    m_From.SendLocalizedMessage(503024); // You stop digging because something is directly on top of the treasure chest.
                    Terminate();
                    return;
                }

                m_Count++;

                m_From.RevealingAction();
                m_From.Direction = m_From.GetDirectionTo(m_Location);

                if (m_Count > 1 && m_Dirt1 == null)
                {
                    m_Dirt1 = new TreasureChestDirt();
                    m_Dirt1.MoveToWorld(m_Location, m_Map);

                    m_Dirt2 = new TreasureChestDirt();
                    m_Dirt2.MoveToWorld(new Point3D(m_Location.X, m_Location.Y - 1, m_Location.Z), m_Map);
                }

                if (m_Count == 5)
                {
                    m_Dirt1.Turn1();
                }
                else if (m_Count == 10)
                {
                    m_Dirt1.Turn2();
                    m_Dirt2.Turn2();
                }
                else if (m_Count > 10)
                {
                    if (m_Chest == null)
                    {
                        m_Chest = new TreasureMapChest(m_From, m_TreasureMap.ChestLevel, true);
                        m_TreasureMap.GiveReward(m_Chest);
                        m_Chest.MoveToWorld(new Point3D(m_Location.X, m_Location.Y, m_Location.Z - 15), m_Map);
                    }
                    else
                    {
                        m_Chest.Z++;
                    }

                    Effects.PlaySound(m_Chest, m_Map, 0x33B);
                }

                if (m_Chest != null && m_Chest.Location.Z >= m_Location.Z)
                {
                    Stop();
                    m_From.EndAction(typeof(GatheringTreasureMap));

                    m_Chest.Temporary = false;
                    m_TreasureMap.Completed = true;
                    m_TreasureMap.CompletedBy = m_From;
                    m_From.SendMessage("You have completed the gathering treasure map!");

                    int spawns = 3;
                    Type[] guardianTypes = m_TreasureMap.GetGuardianTypes();

                    for (int i = 0; i < spawns; ++i)
                    {
                        Type guardianType = guardianTypes[Utility.Random(guardianTypes.Length)];
                        BaseCreature bc = SpawnGuardian(guardianType, m_Chest.Location, m_Chest.Map);

                        if (bc != null)
                            m_Chest.Guardians.Add(bc);
                    }
                }
                else
                {
                    if (m_From.Body.IsHuman && !m_From.Mounted)
                        m_From.Animate(11, 5, 1, true, false, 0);

                    new SoundTimer(m_From, 0x125 + (m_Count % 2)).Start();
                }
            }

            private class SoundTimer : Timer
            {
                private readonly Mobile m_From;
                private readonly int m_SoundID;

                public SoundTimer(Mobile from, int soundID) : base(TimeSpan.FromSeconds(0.9))
                {
                    m_From = from;
                    m_SoundID = soundID;
                    Priority = TimerPriority.TenMS;
                }

                protected override void OnTick()
                {
                    m_From.PlaySound(m_SoundID);
                }
            }
        }

        private Type[] GetGuardianTypes()
        {
            if (m_Skill == SkillName.Lumberjacking)
            {
                if (m_ChestLevel >= 5)
                    return new Type[] { typeof(AncientEnt), typeof(EvilEnt) };

                if (m_ChestLevel >= 3)
                    return new Type[] { typeof(EvilEnt), typeof(Ent) };

                return new Type[] { typeof(Ent) };
            }

            string resourceName = m_ResourceTypeName ?? string.Empty;

            if (resourceName.Contains("DullCopper"))
                return new Type[] { typeof(DullCopperElemental) };
            if (resourceName.Contains("ShadowIron"))
                return new Type[] { typeof(ShadowIronElemental) };
            if (resourceName.Contains("Copper"))
                return new Type[] { typeof(CopperElemental) };
            if (resourceName.Contains("Bronze"))
                return new Type[] { typeof(BronzeElemental) };
            if (resourceName.Contains("Gold"))
                return new Type[] { typeof(GoldenElemental) };
            if (resourceName.Contains("Agapite"))
                return new Type[] { typeof(AgapiteElemental) };
            if (resourceName.Contains("Verite"))
                return new Type[] { typeof(VeriteElemental) };
            if (resourceName.Contains("Valorite"))
                return new Type[] { typeof(ValoriteElemental) };

            return new Type[] { typeof(EarthElemental), typeof(StoneElemental) };
        }

        private static BaseCreature SpawnGuardian(Type type, Point3D p, Map map)
        {
            if (map == null || type == null)
                return null;

            BaseCreature bc;

            try
            {
                bc = (BaseCreature)Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }

            bc.Home = p;
            bc.RangeHome = 5;
            bc.ControlSlots = 666;
            bc.Title += "[Guardian]";

            bool spawned = false;

            for (int i = 0; !spawned && i < 10; ++i)
            {
                int x = p.X - 3 + Utility.Random(7);
                int y = p.Y - 3 + Utility.Random(7);

                if (map.CanSpawnMobile(x, y, p.Z))
                {
                    bc.MoveToWorld(new Point3D(x, y, p.Z), map);
                    spawned = true;
                }
                else
                {
                    int z = map.GetAverageZ(x, y);

                    if (map.CanSpawnMobile(x, y, z))
                    {
                        bc.MoveToWorld(new Point3D(x, y, z), map);
                        spawned = true;
                    }
                }
            }

            if (!spawned)
            {
                bc.Delete();
                return null;
            }

            return bc;
        }
    }
}
