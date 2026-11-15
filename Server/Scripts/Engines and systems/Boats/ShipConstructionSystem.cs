using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Multis;
using Server.Network;
using Server.Items;

// Admin/Dev Note:
// - Hull/part/milestone definitions live in ShipConstructionRegistry.BuildDefinitions().
// - Skill/material tuning lives in ShipConstructionRegistry skill + requirement entries.
// - Final boat/deed creation hook lives in ShipConstructionRegistry.CreateCompletedDeed().
// - This system is additive and does not alter existing boat craft/deed logic.

namespace Server.Items
{
    public enum ShipHullType
    {
        Skiff,
        SmallBoat,
        FishingBoat,
        MediumShip,
        LargeShip,
        WarshipGalleon
    }

    public enum ShipProjectStage
    {
        Blueprint,
        Keel,
        Frame,
        PartialHull,
        FullHull,
        Deck,
        Mast,
        Rigged
    }

    public enum ShipMaterialType
    {
        Boards,
        Cloth,
        Ingots,
        Rope,
        Pitch
    }

    public class ShipMaterialRequirement
    {
        public ShipMaterialType Material;
        public int Amount;

        public ShipMaterialRequirement(ShipMaterialType material, int amount)
        {
            Material = material;
            Amount = amount;
        }
    }

    public class ShipSkillRequirement
    {
        public SkillName Skill;
        public double Min;
        public double Max;

        public ShipSkillRequirement(SkillName skill, double min, double max)
        {
            Skill = skill;
            Min = min;
            Max = max;
        }
    }

    public class ShipPartDefinition
    {
        public string Key;
        public string Name;
        public int ItemID;
        public int NeededBase;
        public List<ShipMaterialRequirement> Materials;
        public List<ShipSkillRequirement> Skills;

        public ShipPartDefinition(string key, string name, int itemID, int neededBase)
        {
            Key = key;
            Name = name;
            ItemID = itemID;
            NeededBase = neededBase;
            Materials = new List<ShipMaterialRequirement>();
            Skills = new List<ShipSkillRequirement>();
        }
    }

    public class ShipMilestoneDefinition
    {
        public int Order;
        public string Key;
        public string Name;
        public string Description;
        public ShipProjectStage Stage;
        public List<ShipMaterialRequirement> Materials;
        public Dictionary<string, int> Parts;
        public List<ShipSkillRequirement> Skills;

        public ShipMilestoneDefinition(int order, string key, string name, string desc, ShipProjectStage stage)
        {
            Order = order;
            Key = key;
            Name = name;
            Description = desc;
            Stage = stage;
            Materials = new List<ShipMaterialRequirement>();
            Parts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            Skills = new List<ShipSkillRequirement>();
        }
    }

    public class ShipDefinition
    {
        public ShipHullType Hull;
        public string Name;
        public string Flavor;
        public int PreviewItemID;
        public double StartCarpentry;
        public int SizeScale;
        public List<ShipPartDefinition> Parts;
        public List<ShipMilestoneDefinition> Milestones;

        public ShipDefinition(ShipHullType hull, string name, string flavor, int previewItemID, double startCarpentry, int sizeScale)
        {
            Hull = hull;
            Name = name;
            Flavor = flavor;
            PreviewItemID = previewItemID;
            StartCarpentry = startCarpentry;
            SizeScale = sizeScale;
            Parts = new List<ShipPartDefinition>();
            Milestones = new List<ShipMilestoneDefinition>();
        }
    }

    public class ShipProjectState
    {
        public Mobile Builder;
        public ShipHullType HullType;
        public DateTime Started;
        public ShipProjectStage Stage;

        public Dictionary<ShipMaterialType, int> StoredMaterials;
        public Dictionary<string, int> FabricatedParts;
        public HashSet<string> CompletedMilestones;

        public ShipProjectState(Mobile builder, ShipHullType hull)
        {
            Builder = builder;
            HullType = hull;
            Started = DateTime.UtcNow;
            Stage = ShipProjectStage.Blueprint;
            StoredMaterials = new Dictionary<ShipMaterialType, int>();
            FabricatedParts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            CompletedMilestones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public ShipProjectState()
        {
            StoredMaterials = new Dictionary<ShipMaterialType, int>();
            FabricatedParts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            CompletedMilestones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public int GetMaterial(ShipMaterialType material)
        {
            int v;
            return StoredMaterials.TryGetValue(material, out v) ? v : 0;
        }

        public void AddMaterial(ShipMaterialType material, int amount)
        {
            if (amount <= 0)
                return;

            StoredMaterials[material] = GetMaterial(material) + amount;
        }

        public bool ConsumeMaterial(ShipMaterialType material, int amount)
        {
            int have = GetMaterial(material);

            if (have < amount)
                return false;

            have -= amount;

            if (have > 0)
                StoredMaterials[material] = have;
            else
                StoredMaterials.Remove(material);

            return true;
        }

        public int GetPartCount(string key)
        {
            int v;
            return FabricatedParts.TryGetValue(key, out v) ? v : 0;
        }

        public void AddPart(string key, int amount)
        {
            if (amount <= 0)
                return;

            FabricatedParts[key] = GetPartCount(key) + amount;
        }

        public bool ConsumePart(string key, int amount)
        {
            int have = GetPartCount(key);

            if (have < amount)
                return false;

            have -= amount;

            if (have > 0)
                FabricatedParts[key] = have;
            else
                FabricatedParts.Remove(key);

            return true;
        }

        public int CompletedCount
        {
            get { return CompletedMilestones.Count; }
        }

        public int ProgressPercent
        {
            get
            {
                ShipDefinition def = ShipConstructionRegistry.GetDefinition(HullType);
                if (def == null || def.Milestones.Count == 0)
                    return 0;

                return (CompletedCount * 100) / def.Milestones.Count;
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write((int)0);

            writer.Write(Builder);
            writer.Write((int)HullType);
            writer.Write(Started);
            writer.Write((int)Stage);

            writer.Write(StoredMaterials.Count);
            foreach (KeyValuePair<ShipMaterialType, int> kvp in StoredMaterials)
            {
                writer.Write((int)kvp.Key);
                writer.Write(kvp.Value);
            }

            writer.Write(FabricatedParts.Count);
            foreach (KeyValuePair<string, int> kvp in FabricatedParts)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }

            writer.Write(CompletedMilestones.Count);
            foreach (string key in CompletedMilestones)
                writer.Write(key);
        }

        public void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            Builder = reader.ReadMobile();
            HullType = (ShipHullType)reader.ReadInt();
            Started = reader.ReadDateTime();
            Stage = (ShipProjectStage)reader.ReadInt();

            int mCount = reader.ReadInt();
            for (int i = 0; i < mCount; i++)
            {
                ShipMaterialType material = (ShipMaterialType)reader.ReadInt();
                int amount = reader.ReadInt();
                StoredMaterials[material] = amount;
            }

            int pCount = reader.ReadInt();
            for (int i = 0; i < pCount; i++)
            {
                string key = reader.ReadString();
                int amount = reader.ReadInt();
                FabricatedParts[key] = amount;
            }

            int cCount = reader.ReadInt();
            for (int i = 0; i < cCount; i++)
                CompletedMilestones.Add(reader.ReadString());
        }
    }

    public static class ShipConstructionRegistry
    {
        private static readonly List<ShipDefinition> m_Definitions;

        static ShipConstructionRegistry()
        {
            m_Definitions = new List<ShipDefinition>();
            BuildDefinitions();
        }

        public static List<ShipDefinition> Definitions
        {
            get { return m_Definitions; }
        }

        public static ShipDefinition GetDefinition(ShipHullType hull)
        {
            for (int i = 0; i < m_Definitions.Count; i++)
            {
                if (m_Definitions[i].Hull == hull)
                    return m_Definitions[i];
            }

            return null;
        }

        public static int GetStagePreview(ShipProjectStage stage)
        {
            switch (stage)
            {
                case ShipProjectStage.Keel: return 0x3E96;
                case ShipProjectStage.Frame: return 0x3E93;
                case ShipProjectStage.PartialHull: return 0x3E90;
                case ShipProjectStage.FullHull: return 0x3E8D;
                case ShipProjectStage.Deck: return 0x3E8A;
                case ShipProjectStage.Mast: return 0x3E87;
                case ShipProjectStage.Rigged: return 0x3E84;
                default: return 0x14F2;
            }
        }

        public static Item CreateCompletedDeed(ShipHullType hull)
        {
            switch (hull)
            {
                case ShipHullType.Skiff: return new LargeBoatDeed();
                case ShipHullType.SmallBoat: return new SmallBoatDeed();
                case ShipHullType.FishingBoat: return new SmallDragonBoatDeed();
                case ShipHullType.MediumShip: return new MediumBoatDeed();
                case ShipHullType.LargeShip: return new MediumDragonBoatDeed();
                case ShipHullType.WarshipGalleon: return new LargeDragonBoatDeed();
                default: return new SmallBoatDeed();
            }
        }

        private static void BuildDefinitions()
        {
            m_Definitions.Clear();

            m_Definitions.Add(CreateDefinition(ShipHullType.SmallBoat, DockedBoatDisplayName.SmallBoat, "The shortest practical ship model for local waters.", 0x14F4, 28.0, 1));
            m_Definitions.Add(CreateDefinition(ShipHullType.FishingBoat, DockedBoatDisplayName.SmallDragonShip, "A short dragon-prow model with added structure.", 0x14F4, 39.0, 2));
            m_Definitions.Add(CreateDefinition(ShipHullType.MediumShip, DockedBoatDisplayName.MediumBoat, "A balanced mid-length ship model for open waters.", 0x14F4, 53.0, 3));
            m_Definitions.Add(CreateDefinition(ShipHullType.LargeShip, DockedBoatDisplayName.MediumDragonShip, "A longer dragon ship model with reinforced frame.", 0x14F4, 63.0, 4));
            m_Definitions.Add(CreateDefinition(ShipHullType.Skiff, DockedBoatDisplayName.LargeBoat, "A full-length standard long ship model requiring advanced work.", 0x14F4, 72.0, 5));
            m_Definitions.Add(CreateDefinition(ShipHullType.WarshipGalleon, DockedBoatDisplayName.LargeDragonShip, "The longest and heaviest dragon ship model.", 0x14F4, 80.0, 6));
        }

        private static ShipDefinition CreateDefinition(ShipHullType hull, string name, string flavor, int previewID, double startSkill, int scale)
        {
            ShipDefinition def = new ShipDefinition(hull, name, flavor, previewID, startSkill, scale);

            ShipPartDefinition keel = new ShipPartDefinition("keel_beam", "Keel Beam", 0x1BDD, 1 * scale);
            keel.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Boards, 40 * scale));
            keel.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Ingots, 8 * scale));
            keel.Skills.Add(new ShipSkillRequirement(SkillName.Carpentry, Math.Max(20, startSkill - 10), startSkill + 20));
            def.Parts.Add(keel);

            ShipPartDefinition ribs = new ShipPartDefinition("hull_rib", "Hull Rib", 0x1BDD, 5 * scale);
            ribs.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Boards, 12 * scale));
            ribs.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Ingots, 2 * scale));
            ribs.Skills.Add(new ShipSkillRequirement(SkillName.Carpentry, Math.Max(20, startSkill - 5), startSkill + 25));
            def.Parts.Add(ribs);

            ShipPartDefinition planks = new ShipPartDefinition("plank_bundle", "Plank Bundle", 0x1BD7, 6 * scale);
            planks.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Boards, 20 * scale));
            planks.Skills.Add(new ShipSkillRequirement(SkillName.Carpentry, Math.Max(20, startSkill - 5), startSkill + 25));
            def.Parts.Add(planks);

            ShipPartDefinition deck = new ShipPartDefinition("deck_section", "Deck Section", 0x1BD7, 3 * scale);
            deck.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Boards, 22 * scale));
            deck.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Ingots, 3 * scale));
            deck.Skills.Add(new ShipSkillRequirement(SkillName.Carpentry, startSkill + 5, startSkill + 30));
            def.Parts.Add(deck);

            ShipPartDefinition mast = new ShipPartDefinition("mast", "Mast", 0x1BDD, 1 * scale);
            mast.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Boards, 30 * scale));
            mast.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Rope, 5 * scale));
            mast.Skills.Add(new ShipSkillRequirement(SkillName.Carpentry, startSkill + 10, startSkill + 35));
            def.Parts.Add(mast);

            ShipPartDefinition sails = new ShipPartDefinition("sail_bundle", "Sail Bundle", 0x1766, 2 * scale);
            sails.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Cloth, 40 * scale));
            sails.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Rope, 8 * scale));
            sails.Skills.Add(new ShipSkillRequirement(SkillName.Tailoring, Math.Max(20, startSkill - 15), startSkill + 20));
            def.Parts.Add(sails);

            ShipPartDefinition rudder = new ShipPartDefinition("rudder_assembly", "Rudder Assembly", 0x3EBE, 1 * scale);
            rudder.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Boards, 10 * scale));
            rudder.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Ingots, 8 * scale));
            rudder.Skills.Add(new ShipSkillRequirement(SkillName.Tinkering, Math.Max(20, startSkill - 10), startSkill + 25));
            def.Parts.Add(rudder);

            ShipPartDefinition fittings = new ShipPartDefinition("iron_fittings", "Iron Fittings", 0x1053, 3 * scale);
            fittings.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Ingots, 25 * scale));
            fittings.Skills.Add(new ShipSkillRequirement(SkillName.Blacksmith, Math.Max(20, startSkill - 5), startSkill + 30));
            def.Parts.Add(fittings);

            ShipPartDefinition fasteners = new ShipPartDefinition("fastening_kit", "Fastening Kit", 0x1EB8, 3 * scale);
            fasteners.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Ingots, 10 * scale));
            fasteners.Materials.Add(new ShipMaterialRequirement(ShipMaterialType.Rope, 4 * scale));
            fasteners.Skills.Add(new ShipSkillRequirement(SkillName.Tinkering, Math.Max(20, startSkill - 10), startSkill + 25));
            def.Parts.Add(fasteners);

            AddMilestone(def, 1, "draft_plans", "Draft Plans", "Blueprints are drafted, measured, and approved.", ShipProjectStage.Blueprint,
                new ShipSkillRequirement(SkillName.Carpentry, startSkill, startSkill + 25));

            AddMilestone(def, 2, "cut_keel", "Cut the Keel", "The keel timbers are shaped and locked in place.", ShipProjectStage.Keel,
                new ShipSkillRequirement(SkillName.Carpentry, startSkill + 5, startSkill + 30),
                new ShipPartReq("keel_beam", 1 * scale), new ShipMatReq(ShipMaterialType.Pitch, 5 * scale));

            AddMilestone(def, 3, "raise_frame", "Raise the Frame", "Ribs are raised and joined to define the hull form.", ShipProjectStage.Frame,
                new ShipSkillRequirement(SkillName.Carpentry, startSkill + 10, startSkill + 35),
                new ShipPartReq("hull_rib", 5 * scale), new ShipPartReq("fastening_kit", 1 * scale));

            AddMilestone(def, 4, "hull_planking", "Fasten Hull Planking", "Outer planks are fitted and fastened to the frame.", ShipProjectStage.PartialHull,
                new ShipSkillRequirement(SkillName.Carpentry, startSkill + 15, startSkill + 40),
                new ShipPartReq("plank_bundle", 6 * scale), new ShipPartReq("iron_fittings", 1 * scale));

            AddMilestone(def, 5, "seal_hull", "Seal the Hull", "Pitch and cloth caulking seal the hull seams.", ShipProjectStage.FullHull,
                new ShipSkillRequirement(SkillName.Carpentry, startSkill + 20, startSkill + 45),
                new ShipMatReq(ShipMaterialType.Pitch, 20 * scale), new ShipMatReq(ShipMaterialType.Cloth, 15 * scale));

            AddMilestone(def, 6, "fit_deck", "Fit the Deck", "Deck sections are laid and secured.", ShipProjectStage.Deck,
                new ShipSkillRequirement(SkillName.Carpentry, startSkill + 22, startSkill + 47),
                new ShipPartReq("deck_section", 3 * scale));

            AddMilestone(def, 7, "step_mast", "Step the Mast", "The mast is raised and seated.", ShipProjectStage.Mast,
                new ShipSkillRequirement(SkillName.Carpentry, startSkill + 25, startSkill + 50),
                new ShipPartReq("mast", 1 * scale), new ShipMatReq(ShipMaterialType.Rope, 10 * scale));

            AddMilestone(def, 8, "rig_sails", "Rig the Sails", "Sails and lines are rigged to the mast.", ShipProjectStage.Rigged,
                new ShipSkillRequirement(SkillName.Tailoring, Math.Max(30, startSkill - 10), startSkill + 35),
                new ShipPartReq("sail_bundle", 2 * scale), new ShipMatReq(ShipMaterialType.Rope, 15 * scale));

            AddMilestone(def, 9, "mount_rudder", "Mount the Rudder", "Rudder and steering linkage are installed.", ShipProjectStage.Rigged,
                new ShipSkillRequirement(SkillName.Tinkering, Math.Max(30, startSkill - 5), startSkill + 35),
                new ShipPartReq("rudder_assembly", 1 * scale));

            AddMilestone(def, 10, "final_hardware", "Fit Final Hardware", "Ironwork and hardware are mounted and tested.", ShipProjectStage.Rigged,
                new ShipSkillRequirement(SkillName.Blacksmith, Math.Max(30, startSkill), startSkill + 40),
                new ShipPartReq("iron_fittings", 2 * scale), new ShipPartReq("fastening_kit", 1 * scale), new ShipMatReq(ShipMaterialType.Ingots, 20 * scale));

            AddMilestone(def, 11, "launch_prep", "Prepare for Launch", "Final inspection and launch-ready finishing.", ShipProjectStage.Rigged,
                new ShipSkillRequirement(SkillName.Carpentry, startSkill + 30, startSkill + 55),
                new ShipMatReq(ShipMaterialType.Pitch, 10 * scale), new ShipMatReq(ShipMaterialType.Cloth, 10 * scale));

            ApplySkillTuning(def);
            return def;
        }

        private static void ApplySkillTuning(ShipDefinition def)
        {
            for (int i = 0; i < def.Parts.Count; i++)
            {
                ShipPartDefinition part = def.Parts[i];
                for (int s = 0; s < part.Skills.Count; s++)
                {
                    ShipSkillRequirement req = part.Skills[s];
                    req.Min = Math.Min(120.0, req.Min * 0.7);
                    req.Max = Math.Min(120.0, req.Max * 0.7);
                }
            }

            for (int i = 0; i < def.Milestones.Count; i++)
            {
                ShipMilestoneDefinition ms = def.Milestones[i];
                for (int s = 0; s < ms.Skills.Count; s++)
                {
                    ShipSkillRequirement req = ms.Skills[s];
                    req.Min = Math.Min(120.0, req.Min * 0.7);
                    req.Max = Math.Min(120.0, req.Max * 0.7);
                }
            }
        }

        private class ShipPartReq
        {
            public string Key;
            public int Amount;
            public ShipPartReq(string key, int amount) { Key = key; Amount = amount; }
        }

        private class ShipMatReq
        {
            public ShipMaterialType Material;
            public int Amount;
            public ShipMatReq(ShipMaterialType material, int amount) { Material = material; Amount = amount; }
        }

        private static void AddMilestone(ShipDefinition def, int order, string key, string name, string desc, ShipProjectStage stage, ShipSkillRequirement skill, params object[] reqs)
        {
            ShipMilestoneDefinition ms = new ShipMilestoneDefinition(order, key, name, desc, stage);
            ms.Skills.Add(skill);

            for (int i = 0; i < reqs.Length; i++)
            {
                ShipPartReq part = reqs[i] as ShipPartReq;
                ShipMatReq mat = reqs[i] as ShipMatReq;

                if (part != null)
                    ms.Parts[part.Key] = part.Amount;
                else if (mat != null)
                    ms.Materials.Add(new ShipMaterialRequirement(mat.Material, mat.Amount));
            }

            def.Milestones.Add(ms);
        }
    }

    public class MasterShipSchematic : Item
    {
        private Item m_ActiveProject;

        [CommandProperty(AccessLevel.GameMaster)]
        public Item ActiveProject
        {
            get { return m_ActiveProject; }
            set { m_ActiveProject = value; }
        }

        [Constructable]
        public MasterShipSchematic()
            : base(0x14F1)
        {
            Name = "Shipwright's Master Schematic";
            Hue = 0x8AB;
            Weight = 1.0;
        }

        public MasterShipSchematic(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001);
                return;
            }

            from.CloseGump(typeof(ShipConstructionGump));
            from.SendGump(new ShipConstructionGump(from, this, 0));
        }

        public bool CanCreateProject(Mobile from)
        {
            if (m_ActiveProject == null || m_ActiveProject.Deleted)
                return true;

            ShipProjectSchematic existing = m_ActiveProject as ShipProjectSchematic;

            if (existing != null && existing.Owner == from)
                return false;

            return true;
        }

        public void AssignProject(Item project)
        {
            m_ActiveProject = project;
            InvalidateProperties();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write(m_ActiveProject);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_ActiveProject = reader.ReadItem();
        }
    }

    public class ShipProjectSchematic : Item
    {
        private Mobile m_Owner;
        private MasterShipSchematic m_Master;
        private ShipProjectState m_State;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owner { get { return m_Owner; } }

        public MasterShipSchematic Master { get { return m_Master; } }

        public ShipProjectState State { get { return m_State; } }

        [Constructable]
        public ShipProjectSchematic(Mobile owner, MasterShipSchematic master, ShipHullType hull)
            : base(0x14F2)
        {
            m_Owner = owner;
            m_Master = master;
            m_State = new ShipProjectState(owner, hull);

            Weight = 1.0;
            Hue = 0x59B;
            Name = "Ship Project Schematic - " + ShipConstructionRegistry.GetDefinition(hull).Name;
            LootType = LootType.Blessed;
        }

        public ShipProjectSchematic(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001);
                return;
            }

            if (from != m_Owner)
            {
                from.SendMessage("This ship project is bound to another builder.");
                return;
            }

            from.CloseGump(typeof(ShipProjectGump));
            from.SendGump(new ShipProjectGump(from, this, 1, 0));
        }

        public override void OnDelete()
        {
            if (m_Master != null && !m_Master.Deleted)
            {
                if (m_Master.ActiveProject == this)
                    m_Master.AssignProject(null);

                m_Master.Delete();
            }

            base.OnDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);

            writer.Write(m_Owner);
            writer.Write(m_Master);
            m_State.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_Owner = reader.ReadMobile();
            m_Master = reader.ReadItem() as MasterShipSchematic;

            m_State = new ShipProjectState();
            m_State.Deserialize(reader);

            if (Name == null)
            {
                ShipDefinition def = ShipConstructionRegistry.GetDefinition(m_State.HullType);
                Name = "Ship Project Schematic - " + (def != null ? def.Name : "Unknown");
            }
        }
    }

    public class RopeBundle : Item
    {
        [Constructable]
        public RopeBundle() : this(1) { }

        [Constructable]
        public RopeBundle(int amount) : base(0x14F8)
        {
            Stackable = true;
            Amount = amount;
            Weight = 0.1;
            Name = "rope bundle";
        }

        public RopeBundle(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class PineResin : Item
    {
        [Constructable]
        public PineResin() : this(1) { }

        [Constructable]
        public PineResin(int amount) : base(0x1422)
        {
            Stackable = true;
            Amount = amount;
            Weight = 0.1;
            Name = "pine resin";
            Hue = 0x59A;
        }

        public PineResin(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class Tar : Item
    {
        [Constructable]
        public Tar() : this(1) { }

        [Constructable]
        public Tar(int amount) : base(0x1BF2)
        {
            Stackable = true;
            Amount = amount;
            Weight = 0.1;
            Name = "tar";
            Hue = 0x497;
        }

        public Tar(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class PitchBucket : Item
    {
        [Constructable]
        public PitchBucket() : this(1) { }

        [Constructable]
        public PitchBucket(int amount) : base(0x1BF2)
        {
            Stackable = true;
            Amount = amount;
            Weight = 0.1;
            Name = "pitch";
            Hue = 0x455;
        }

        public PitchBucket(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}

namespace Server.Gumps
{
    using System.Collections.Generic;
    using Server.Items;

    public class ShipConstructionGump : Gump
    {
        private readonly Mobile m_From;
        private readonly MasterShipSchematic m_Master;
        private readonly int m_Selected;

        public ShipConstructionGump(Mobile from, MasterShipSchematic master, int selected)
            : base(40, 30)
        {
            m_From = from;
            m_Master = master;
            m_Selected = selected;

            Closable = true;
            Disposable = true;
            Dragable = true;

            AddPage(0);
            AddBackground(0, 0, 600, 470, 5054);
            AddLabel(20, 15, 1152, "Shipwright's Construction Schematic");
            AddLabel(20, 35, 2100, "Select a vessel to begin construction");

            AddImageTiled(15, 60, 240, 345, 2624);
            AddAlphaRegion(15, 60, 240, 345);
            AddImageTiled(265, 60, 320, 345, 2624);
            AddAlphaRegion(265, 60, 320, 345);

            List<ShipDefinition> defs = ShipConstructionRegistry.Definitions;

            for (int i = 0; i < defs.Count; i++)
            {
                ShipDefinition def = defs[i];
                int y = 72 + (i * 53);
                bool available = from.Skills[SkillName.Carpentry].Value >= def.StartCarpentry;
                int hue = available ? 1152 : 2401;

                AddItem(28, y + 2, def.PreviewItemID);
                AddLabel(64, y, hue, def.Name);
                AddLabel(64, y + 20, hue, "Start Carpentry: " + def.StartCarpentry.ToString("F1"));
                AddButton(220, y + 10, 4005, 4007, 100 + i, GumpButtonType.Reply, 0);
            }

            if (m_Selected >= 0 && m_Selected < defs.Count)
            {
                ShipDefinition selectedDef = defs[m_Selected];
                AddItem(390, 95, selectedDef.PreviewItemID);
                AddLabel(300, 80, 1258, selectedDef.Name);
                AddLabel(300, 165, 1152, "Start Skill: Carpentry " + selectedDef.StartCarpentry.ToString("F1"));
                AddHtml(300, 185, 275, 38, "<BASEFONT COLOR=#FFFFFF>Secondary Skills: Tailoring / Tinkering / Blacksmithing</BASEFONT>", false, false);
                AddLabel(300, 228, 1152, "Milestones: " + selectedDef.Milestones.Count);

                int boards = 0;
                int cloth = 0;
                int ingots = 0;
                int rope = 0;
                int pitch = 0;

                for (int p = 0; p < selectedDef.Parts.Count; p++)
                {
                    ShipPartDefinition part = selectedDef.Parts[p];
                    for (int r = 0; r < part.Materials.Count; r++)
                    {
                        ShipMaterialRequirement req = part.Materials[r];
                        if (req.Material == ShipMaterialType.Boards) boards += req.Amount * part.NeededBase;
                        else if (req.Material == ShipMaterialType.Cloth) cloth += req.Amount * part.NeededBase;
                        else if (req.Material == ShipMaterialType.Ingots) ingots += req.Amount * part.NeededBase;
                        else if (req.Material == ShipMaterialType.Rope) rope += req.Amount * part.NeededBase;
                        else if (req.Material == ShipMaterialType.Pitch) pitch += req.Amount * part.NeededBase;
                    }
                }

                for (int m = 0; m < selectedDef.Milestones.Count; m++)
                {
                    ShipMilestoneDefinition ms = selectedDef.Milestones[m];

                    for (int r = 0; r < ms.Materials.Count; r++)
                    {
                        ShipMaterialRequirement req = ms.Materials[r];
                        if (req.Material == ShipMaterialType.Boards) boards += req.Amount;
                        else if (req.Material == ShipMaterialType.Cloth) cloth += req.Amount;
                        else if (req.Material == ShipMaterialType.Ingots) ingots += req.Amount;
                        else if (req.Material == ShipMaterialType.Rope) rope += req.Amount;
                        else if (req.Material == ShipMaterialType.Pitch) pitch += req.Amount;
                    }
                }

                AddHtml(300, 255, 275, 70,
                    string.Format("<BASEFONT COLOR=#FFFFFF>Estimated materials:<BR>Boards {0}, Cloth {1}, Ingots {2}, Rope {3}, Pitch {4}</BASEFONT>", boards, cloth, ingots, rope, pitch),
                    false, false);
                AddHtml(300, 330, 275, 70,
                    "<BASEFONT COLOR=#CCCCCC>" + selectedDef.Flavor + "</BASEFONT>",
                    false, true);
            }

            AddButton(320, 425, 4023, 4025, 1, GumpButtonType.Reply, 0);
            AddLabel(355, 425, 67, "Create Project");

            AddButton(490, 425, 4017, 4019, 2, GumpButtonType.Reply, 0);
            AddLabel(525, 425, 1152, "Close");
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (m_Master == null || m_Master.Deleted)
                return;

            if (!m_Master.IsChildOf(m_From.Backpack))
                return;

            int buttonID = info.ButtonID;
            if (buttonID == 0)
                return;

            if (buttonID >= 100)
            {
                int selected = buttonID - 100;
                m_From.SendGump(new ShipConstructionGump(m_From, m_Master, selected));
                return;
            }

            if (buttonID == 1)
            {
                if (m_Selected < 0 || m_Selected >= ShipConstructionRegistry.Definitions.Count)
                {
                    m_From.SendMessage("Select a hull first.");
                    m_From.SendGump(new ShipConstructionGump(m_From, m_Master, m_Selected));
                    return;
                }

                ShipDefinition def = ShipConstructionRegistry.Definitions[m_Selected];
                if (m_From.Skills[SkillName.Carpentry].Value < def.StartCarpentry)
                {
                    m_From.SendMessage("You lack the Carpentry skill to begin this hull.");
                    m_From.SendGump(new ShipConstructionGump(m_From, m_Master, m_Selected));
                    return;
                }

                if (!m_Master.CanCreateProject(m_From))
                {
                    m_From.SendMessage("This master schematic already has an active bound project.");
                    return;
                }

                ShipProjectSchematic project = new ShipProjectSchematic(m_From, m_Master, def.Hull);
                m_Master.AssignProject(project);
                m_From.AddToBackpack(project);
                m_Master.Delete();
                m_From.SendMessage("You draft and bind a new ship project schematic.");
                m_From.CloseGump(typeof(ShipProjectGump));
                m_From.SendGump(new ShipProjectGump(m_From, project, 1, 0));
            }
        }
    }

    public class ShipProjectGump : Gump
    {
        private readonly Mobile m_From;
        private readonly ShipProjectSchematic m_Project;
        private readonly ShipDefinition m_Def;
        private readonly int m_Page;
        private readonly int m_SelectedIndex;
        private readonly int m_AssembleListPage;

        public ShipProjectGump(Mobile from, ShipProjectSchematic project, int page, int selectedIndex)
            : this(from, project, page, selectedIndex, 0)
        {
        }

        public ShipProjectGump(Mobile from, ShipProjectSchematic project, int page, int selectedIndex, int assembleListPage)
            : base(30, 20)
        {
            m_From = from;
            m_Project = project;
            m_Def = project != null ? ShipConstructionRegistry.GetDefinition(project.State.HullType) : null;
            m_Page = page;
            m_SelectedIndex = selectedIndex;
            m_AssembleListPage = assembleListPage;

            Closable = true;
            Disposable = true;
            Dragable = true;

            AddPage(0);
            AddBackground(0, 0, 640, 520, 5054);

            if (m_Def == null)
            {
                AddLabel(20, 20, 33, "Invalid ship project.");
                return;
            }

            AddLabel(20, 12, 1152, m_Def.Name + " Construction");
            AddLabel(20, 32, 1152, "Builder: " + (project.Owner != null ? project.Owner.Name : "Unknown"));
            AddLabel(260, 32, 1152, "Started: " + project.State.Started.ToString("yyyy-MM-dd"));
            AddLabel(20, 52, 1152, "Progress: " + project.State.ProgressPercent + "%");
            AddLabel(180, 52, 1152, "Current Stage: " + project.State.Stage);
            AddItem(540, 18, ShipConstructionRegistry.GetStagePreview(project.State.Stage));

            AddButton(20, 82, 4005, 4007, 6001, GumpButtonType.Reply, 0); AddLabel(50, 82, 1152, "Overview");
            AddButton(160, 82, 4005, 4007, 6002, GumpButtonType.Reply, 0); AddLabel(190, 82, 1152, "Materials");
            AddButton(300, 82, 4005, 4007, 6003, GumpButtonType.Reply, 0); AddLabel(330, 82, 1152, "Fabricate Parts");
            AddButton(470, 82, 4005, 4007, 6004, GumpButtonType.Reply, 0); AddLabel(500, 82, 1152, "Assemble");

            if (m_Page == 2)
                BuildMaterialsPage();
            else if (m_Page == 3)
                BuildFabricationPage();
            else if (m_Page == 4)
                BuildAssemblePage();
            else
                BuildOverviewPage();
        }

        private void BuildOverviewPage()
        {
            AddPage(0);

            AddImageTiled(15, 110, 610, 390, 2624);
            AddAlphaRegion(15, 110, 610, 390);

            AddLabel(28, 120, 1152, "Hull: " + m_Def.Name);
            AddLabel(28, 140, 1152, "Primary: Carpentry");
            AddLabel(28, 160, 1152, "Secondary: Tailoring / Tinkering / Blacksmithing");
            AddLabel(28, 180, 1152, "Completed Milestones: " + m_Project.State.CompletedCount + "/" + m_Def.Milestones.Count);
            AddLabel(28, 200, 1152, "Fabricated Part Types: " + m_Project.State.FabricatedParts.Count);

            AddLabel(28, 230, 1258, "Milestone Checklist");
            int y = 255;

            for (int i = 0; i < m_Def.Milestones.Count; i++)
            {
                ShipMilestoneDefinition ms = m_Def.Milestones[i];
                string state = "Locked";
                int hue = 2401;

                if (m_Project.State.CompletedMilestones.Contains(ms.Key))
                {
                    state = "Complete";
                    hue = 68;
                }
                else if (i == 0 || m_Project.State.CompletedMilestones.Contains(m_Def.Milestones[i - 1].Key))
                {
                    state = "Ready";
                    hue = 1152;
                }

                AddLabel(28, y, hue, string.Format("[{0}] {1}", state, ms.Name));
                if (ms.Skills.Count > 0)
                    AddLabel(305, y, 1152, ms.Skills[0].Skill + " " + ms.Skills[0].Min.ToString("F1"));
                AddButton(570, y, 4011, 4013, 5000 + i, GumpButtonType.Reply, 0);

                y += 21;
                if (y > 470)
                    break;
            }

            ShipMilestoneDefinition next = GetNextMilestone();
            if (next != null)
            {
                AddImageTiled(350, 120, 265, 120, 3604);
                AddAlphaRegion(350, 120, 265, 120);
                AddLabel(360, 126, 1258, "Next Step");
                AddLabel(360, 146, 1152, next.Name);

                if (next.Skills.Count > 0)
                {
                    ShipSkillRequirement s = next.Skills[0];
                    double skill = m_From.Skills[s.Skill].Value;
                    string diff = skill >= s.Min ? "Within range" : "Difficult";
                    AddLabel(360, 166, 1152, "Required: " + s.Skill + " " + s.Min.ToString("F1"));
                    AddLabel(360, 186, 1152, "Current: " + skill.ToString("F1") + " (" + diff + ")");
                }
            }
        }

        private void BuildMaterialsPage()
        {
            AddPage(0);
            AddImageTiled(15, 110, 610, 390, 2624);
            AddAlphaRegion(15, 110, 610, 390);

            AddLabel(28, 125, 1258, "Stored Materials");
            AddLabel(28, 150, 1152, "Boards: " + m_Project.State.GetMaterial(ShipMaterialType.Boards));
            AddLabel(28, 175, 1152, "Cloth: " + m_Project.State.GetMaterial(ShipMaterialType.Cloth));
            AddLabel(28, 200, 1152, "Iron Ingots: " + m_Project.State.GetMaterial(ShipMaterialType.Ingots));
            AddLabel(28, 225, 1152, "Harpoon Rope: " + m_Project.State.GetMaterial(ShipMaterialType.Rope));
            AddLabel(28, 250, 1152, "Pitch: " + m_Project.State.GetMaterial(ShipMaterialType.Pitch));

            int totalBoards = 0;
            int totalCloth = 0;
            int totalIngots = 0;
            int totalRope = 0;
            int totalPitch = 0;

            for (int i = 0; i < m_Def.Parts.Count; i++)
            {
                ShipPartDefinition part = m_Def.Parts[i];

                for (int r = 0; r < part.Materials.Count; r++)
                {
                    ShipMaterialRequirement req = part.Materials[r];
                    int amt = req.Amount * part.NeededBase;

                    if (req.Material == ShipMaterialType.Boards) totalBoards += amt;
                    else if (req.Material == ShipMaterialType.Cloth) totalCloth += amt;
                    else if (req.Material == ShipMaterialType.Ingots) totalIngots += amt;
                    else if (req.Material == ShipMaterialType.Rope) totalRope += amt;
                    else if (req.Material == ShipMaterialType.Pitch) totalPitch += amt;
                }
            }

            for (int i = 0; i < m_Def.Milestones.Count; i++)
            {
                ShipMilestoneDefinition ms = m_Def.Milestones[i];

                for (int r = 0; r < ms.Materials.Count; r++)
                {
                    ShipMaterialRequirement req = ms.Materials[r];

                    if (req.Material == ShipMaterialType.Boards) totalBoards += req.Amount;
                    else if (req.Material == ShipMaterialType.Cloth) totalCloth += req.Amount;
                    else if (req.Material == ShipMaterialType.Ingots) totalIngots += req.Amount;
                    else if (req.Material == ShipMaterialType.Rope) totalRope += req.Amount;
                    else if (req.Material == ShipMaterialType.Pitch) totalPitch += req.Amount;
                }
            }

            AddLabel(28, 282, 1258, "Total Needed For Full Ship");
            AddLabel(28, 304, 1152, "Boards: " + totalBoards);
            AddLabel(28, 324, 1152, "Cloth: " + totalCloth);
            AddLabel(28, 344, 1152, "Iron Ingots: " + totalIngots);
            AddLabel(28, 364, 1152, "Harpoon Rope: " + totalRope);
            AddLabel(28, 384, 1152, "Pitch: " + totalPitch);

            AddButton(310, 150, 4023, 4025, 2001, GumpButtonType.Reply, 0); AddLabel(345, 150, 1152, "Add Boards");
            AddButton(310, 175, 4023, 4025, 2002, GumpButtonType.Reply, 0); AddLabel(345, 175, 1152, "Add Cloth");
            AddButton(310, 200, 4023, 4025, 2003, GumpButtonType.Reply, 0); AddLabel(345, 200, 1152, "Add Ingots");
            AddButton(310, 225, 4023, 4025, 2004, GumpButtonType.Reply, 0); AddLabel(345, 225, 1152, "Add Harpoon Rope");
            AddButton(310, 250, 4023, 4025, 2005, GumpButtonType.Reply, 0); AddLabel(345, 250, 1152, "Add Pitch");
            AddButton(310, 290, 4005, 4007, 2010, GumpButtonType.Reply, 0); AddLabel(345, 290, 68, "Add All Valid Materials");
            AddButton(310, 325, 4014, 4016, 2020, GumpButtonType.Reply, 0); AddLabel(345, 325, 1152, "Return Stored Materials");
        }

        private void BuildFabricationPage()
        {
            AddPage(0);
            AddImageTiled(15, 110, 610, 390, 2624);
            AddAlphaRegion(15, 110, 610, 390);

            AddLabel(25, 118, 1258, "Fabricate Parts");

            ShipMilestoneDefinition nextMilestone = GetNextMilestone();
            int y = 145;
            int rowHeight = 40;

            for (int i = 0; i < m_Def.Parts.Count; i++)
            {
                ShipPartDefinition part = m_Def.Parts[i];
                int made = m_Project.State.GetPartCount(part.Key);
                int needed = part.NeededBase;
                int rowHue = 1152;

                if (nextMilestone != null && nextMilestone.Parts.ContainsKey(part.Key))
                {
                    int needForMilestone = nextMilestone.Parts[part.Key];
                    int missing = needForMilestone - made;

                    if (missing > 0)
                        rowHue = 68;
                }

                AddItem(22, y, part.ItemID);
                AddLabel(60, y, rowHue, part.Name);
                AddLabel(220, y, rowHue, made + " / " + needed);

                string skillText = part.Skills.Count > 0 ? (part.Skills[0].Skill + " " + part.Skills[0].Min.ToString("F1")) : "";
                AddLabel(300, y, rowHue, skillText);

                AddButton(510, y, 4023, 4025, 3000 + i, GumpButtonType.Reply, 0);
                AddLabel(545, y, 1152, "Make One");

                y += rowHeight;
                if (y > 470)
                    break;
            }
        }

        private void BuildAssemblePage()
        {
            AddPage(0);
            AddImageTiled(15, 110, 610, 390, 2624);
            AddAlphaRegion(15, 110, 610, 390);

            AddLabel(22, 118, 1258, "Assembly Milestones");
            int y = 145;
            int rowsPerPage = 9;
            int maxPage = (m_Def.Milestones.Count - 1) / rowsPerPage;
            int page = m_AssembleListPage;

            if (page < 0)
                page = 0;
            else if (page > maxPage)
                page = maxPage;

            int start = page * rowsPerPage;
            int end = Math.Min(start + rowsPerPage, m_Def.Milestones.Count);

            AddLabel(430, 118, 1152, string.Format("Page {0}/{1}", page + 1, maxPage + 1));

            if (page > 0)
                AddButton(520, 118, 4014, 4016, 5601, GumpButtonType.Reply, 0);

            if (page < maxPage)
                AddButton(550, 118, 4005, 4007, 5602, GumpButtonType.Reply, 0);

            for (int i = start; i < end; i++)
            {
                ShipMilestoneDefinition ms = m_Def.Milestones[i];
                bool complete = m_Project.State.CompletedMilestones.Contains(ms.Key);
                bool ready = !complete && (i == 0 || m_Project.State.CompletedMilestones.Contains(m_Def.Milestones[i - 1].Key));

                int hue = complete ? 68 : (ready ? 1152 : 2401);
                string state = complete ? "Complete" : (ready ? "Ready" : "Locked");
                AddLabel(22, y, hue, ms.Name + " [" + state + "]");

                if (ready)
                    AddButton(330, y, 4005, 4007, 4000 + i, GumpButtonType.Reply, 0);

                AddButton(365, y, 4011, 4013, 4500 + i, GumpButtonType.Reply, 0);
                y += 24;
                if (y > 360)
                    break;
            }

            if (m_SelectedIndex >= 0 && m_SelectedIndex < m_Def.Milestones.Count)
            {
                ShipMilestoneDefinition sel = m_Def.Milestones[m_SelectedIndex];
                AddImageTiled(20, 372, 600, 130, 3604);
                AddAlphaRegion(20, 372, 600, 130);
                AddLabel(28, 378, 1258, sel.Name);
                AddHtml(28, 398, 360, 44, "<BASEFONT COLOR=#FFFFFF>" + sel.Description + "</BASEFONT>", false, true);

                int yy = 446;
                if (sel.Skills.Count > 0)
                {
                    ShipSkillRequirement req = sel.Skills[0];
                    AddLabel(28, yy, 1152, "Skill: " + req.Skill + " " + req.Min.ToString("F1") + " (you " + m_From.Skills[req.Skill].Value.ToString("F1") + ")");
                }

                AddLabel(390, 398, 1152, "Required parts/materials:");
                int reqY = 418;
                foreach (KeyValuePair<string, int> kv in sel.Parts)
                {
                    AddLabel(390, reqY, 1152, kv.Key + " " + m_Project.State.GetPartCount(kv.Key) + "/" + kv.Value);
                    reqY += 18;
                }
                for (int i = 0; i < sel.Materials.Count; i++)
                {
                    ShipMaterialRequirement req = sel.Materials[i];
                    AddLabel(390, reqY, 1152, req.Material + " " + m_Project.State.GetMaterial(req.Material) + "/" + req.Amount);
                    reqY += 18;
                }
            }
        }

        private ShipMilestoneDefinition GetNextMilestone()
        {
            for (int i = 0; i < m_Def.Milestones.Count; i++)
            {
                ShipMilestoneDefinition ms = m_Def.Milestones[i];
                if (!m_Project.State.CompletedMilestones.Contains(ms.Key))
                    return ms;
            }

            return null;
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (m_Project == null || m_Project.Deleted || m_From != m_Project.Owner)
                return;

            if (!m_Project.IsChildOf(m_From.Backpack))
                return;

            int id = info.ButtonID;
            if (id == 0)
                return;

            if (id == 6001 || id == 6002 || id == 6003 || id == 6004)
            {
                m_From.SendGump(new ShipProjectGump(m_From, m_Project, id - 6000, m_SelectedIndex, m_AssembleListPage));
                return;
            }

            if (id >= 2001 && id <= 2005)
            {
                ShipMaterialType material = (ShipMaterialType)(id - 2001);
                AddMaterialFromBackpack(material);
                m_From.SendGump(new ShipProjectGump(m_From, m_Project, 2, m_SelectedIndex, m_AssembleListPage));
                return;
            }

            if (id == 2010)
            {
                AddMaterialFromBackpack(ShipMaterialType.Boards);
                AddMaterialFromBackpack(ShipMaterialType.Cloth);
                AddMaterialFromBackpack(ShipMaterialType.Ingots);
                AddMaterialFromBackpack(ShipMaterialType.Rope);
                AddMaterialFromBackpack(ShipMaterialType.Pitch);
                m_From.SendGump(new ShipProjectGump(m_From, m_Project, 2, m_SelectedIndex, m_AssembleListPage));
                return;
            }

            if (id == 2020)
            {
                ReturnStoredMaterials();
                m_From.SendGump(new ShipProjectGump(m_From, m_Project, 2, m_SelectedIndex, m_AssembleListPage));
                return;
            }

            if (id >= 3000 && id < 4000)
            {
                int idx = id - 3000;
                if (idx >= 0 && idx < m_Def.Parts.Count)
                    TryFabricate(m_Def.Parts[idx]);

                m_From.SendGump(new ShipProjectGump(m_From, m_Project, 3, m_SelectedIndex, m_AssembleListPage));
                return;
            }

            if (id >= 4000 && id < 4500)
            {
                int idx = id - 4000;
                if (idx >= 0 && idx < m_Def.Milestones.Count)
                    TryAssemble(m_Def.Milestones[idx]);

                m_From.SendGump(new ShipProjectGump(m_From, m_Project, 4, idx, idx / 9));
                return;
            }

            if (id >= 4500 && id < 5000)
            {
                int idx = id - 4500;
                m_From.SendGump(new ShipProjectGump(m_From, m_Project, 4, idx, idx / 9));
                return;
            }

            if (id >= 5000 && id < 5500)
            {
                int idx = id - 5000;
                m_From.SendGump(new ShipProjectGump(m_From, m_Project, 4, idx, idx / 9));
                return;
            }

            if (id == 5601)
            {
                m_From.SendGump(new ShipProjectGump(m_From, m_Project, 4, m_SelectedIndex, m_AssembleListPage - 1));
                return;
            }

            if (id == 5602)
            {
                m_From.SendGump(new ShipProjectGump(m_From, m_Project, 4, m_SelectedIndex, m_AssembleListPage + 1));
                return;
            }

            m_From.SendGump(new ShipProjectGump(m_From, m_Project, m_Page, m_SelectedIndex, m_AssembleListPage));
        }

        private void AddMaterialFromBackpack(ShipMaterialType material)
        {
            Container pack = m_From.Backpack;
            if (pack == null)
                return;

            int amount = 0;

            if (material == ShipMaterialType.Boards)
            {
                amount = pack.GetAmount(typeof(Board));
                if (amount > 0)
                    pack.ConsumeTotal(typeof(Board), amount);
            }
            else if (material == ShipMaterialType.Cloth)
            {
                int cloth = pack.GetAmount(typeof(Cloth));
                int uncut = pack.GetAmount(typeof(UncutCloth));
                int bolts = pack.GetAmount(typeof(BoltOfCloth));

                amount = cloth + uncut + (bolts * 50);

                if (cloth > 0) pack.ConsumeTotal(typeof(Cloth), cloth);
                if (uncut > 0) pack.ConsumeTotal(typeof(UncutCloth), uncut);
                if (bolts > 0) pack.ConsumeTotal(typeof(BoltOfCloth), bolts);
            }
            else if (material == ShipMaterialType.Ingots)
            {
                amount = pack.GetAmount(typeof(IronIngot));
                if (amount > 0)
                    pack.ConsumeTotal(typeof(IronIngot), amount);
            }
            else if (material == ShipMaterialType.Rope)
            {
                amount = pack.GetAmount(typeof(HarpoonRope));
                if (amount > 0)
                    pack.ConsumeTotal(typeof(HarpoonRope), amount);
            }
            else if (material == ShipMaterialType.Pitch)
            {
                amount = pack.GetAmount(typeof(PitchBucket));
                if (amount > 0)
                    pack.ConsumeTotal(typeof(PitchBucket), amount);
            }

            if (amount > 0)
            {
                m_Project.State.AddMaterial(material, amount);
                m_From.SendMessage("Stored {0} {1} in the project.", amount, material.ToString().ToLower());
            }
            else
            {
                m_From.SendMessage("No valid {0} found in your backpack.", material.ToString().ToLower());
            }
        }

        private void ReturnStoredMaterials()
        {
            int boards = m_Project.State.GetMaterial(ShipMaterialType.Boards);
            int cloth = m_Project.State.GetMaterial(ShipMaterialType.Cloth);
            int ingots = m_Project.State.GetMaterial(ShipMaterialType.Ingots);
            int rope = m_Project.State.GetMaterial(ShipMaterialType.Rope);
            int pitch = m_Project.State.GetMaterial(ShipMaterialType.Pitch);

            if (boards <= 0 && cloth <= 0 && ingots <= 0 && rope <= 0 && pitch <= 0)
            {
                m_From.SendMessage("There are no stored materials to return.");
                return;
            }

            if (boards > 0)
            {
                m_Project.State.ConsumeMaterial(ShipMaterialType.Boards, boards);
                m_From.AddToBackpack(new Board(boards));
            }

            if (cloth > 0)
            {
                m_Project.State.ConsumeMaterial(ShipMaterialType.Cloth, cloth);
                m_From.AddToBackpack(new Cloth(cloth));
            }

            if (ingots > 0)
            {
                m_Project.State.ConsumeMaterial(ShipMaterialType.Ingots, ingots);
                m_From.AddToBackpack(new IronIngot(ingots));
            }

            if (rope > 0)
            {
                m_Project.State.ConsumeMaterial(ShipMaterialType.Rope, rope);
                m_From.AddToBackpack(new HarpoonRope(rope));
            }

            if (pitch > 0)
            {
                m_Project.State.ConsumeMaterial(ShipMaterialType.Pitch, pitch);
                m_From.AddToBackpack(new PitchBucket(pitch));
            }

            m_From.SendMessage("Stored materials have been returned to your backpack.");
        }

        private bool CheckAllSkills(List<ShipSkillRequirement> reqs)
        {
            for (int i = 0; i < reqs.Count; i++)
            {
                ShipSkillRequirement req = reqs[i];
                if (m_From.Skills[req.Skill].Value < req.Min)
                    return false;
            }

            return true;
        }

        private bool RollSkillCheck(List<ShipSkillRequirement> reqs)
        {
            bool success = true;

            for (int i = 0; i < reqs.Count; i++)
            {
                ShipSkillRequirement req = reqs[i];
                if (!m_From.CheckSkill(req.Skill, req.Min, req.Max))
                    success = false;
            }

            return success;
        }

        private void TryFabricate(ShipPartDefinition part)
        {
            for (int i = 0; i < part.Materials.Count; i++)
            {
                ShipMaterialRequirement req = part.Materials[i];
                if (m_Project.State.GetMaterial(req.Material) < req.Amount)
                {
                    m_From.SendMessage("You need more stored {0} to fabricate {1}.", req.Material.ToString().ToLower(), part.Name);
                    return;
                }
            }

            if (!CheckAllSkills(part.Skills))
            {
                m_From.SendMessage("You do not meet the skill requirements to fabricate this part yet.");
                return;
            }

            bool success = RollSkillCheck(part.Skills);

            if (success)
            {
                for (int i = 0; i < part.Materials.Count; i++)
                {
                    ShipMaterialRequirement req = part.Materials[i];
                    m_Project.State.ConsumeMaterial(req.Material, req.Amount);
                }

                m_Project.State.AddPart(part.Key, 1);
                m_From.PlaySound(0x23D);
                m_From.SendMessage("You successfully fabricate a {0}.", part.Name);
            }
            else
            {
                for (int i = 0; i < part.Materials.Count; i++)
                {
                    ShipMaterialRequirement req = part.Materials[i];
                    int loss = Math.Max(1, req.Amount / 2);
                    m_Project.State.ConsumeMaterial(req.Material, loss);
                }

                m_From.PlaySound(0x21);
                m_From.SendMessage("You fail to fabricate the part and lose some materials.");
            }
        }

        private void TryAssemble(ShipMilestoneDefinition milestone)
        {
            if (m_Project.State.CompletedMilestones.Contains(milestone.Key))
            {
                m_From.SendMessage("That milestone is already complete.");
                return;
            }

            int index = m_Def.Milestones.IndexOf(milestone);
            if (index > 0)
            {
                ShipMilestoneDefinition prev = m_Def.Milestones[index - 1];
                if (!m_Project.State.CompletedMilestones.Contains(prev.Key))
                {
                    m_From.SendMessage("You must complete earlier milestones first.");
                    return;
                }
            }

            if (!CheckAllSkills(milestone.Skills))
            {
                m_From.SendMessage("You are not yet skilled enough for this milestone.");
                return;
            }

            foreach (KeyValuePair<string, int> kv in milestone.Parts)
            {
                if (m_Project.State.GetPartCount(kv.Key) < kv.Value)
                {
                    m_From.SendMessage("Missing fabricated part: {0}", kv.Key);
                    return;
                }
            }

            for (int i = 0; i < milestone.Materials.Count; i++)
            {
                ShipMaterialRequirement req = milestone.Materials[i];
                if (m_Project.State.GetMaterial(req.Material) < req.Amount)
                {
                    m_From.SendMessage("Missing stored material: {0}", req.Material.ToString().ToLower());
                    return;
                }
            }

            bool success = RollSkillCheck(milestone.Skills);

            if (success)
            {
                foreach (KeyValuePair<string, int> kv in milestone.Parts)
                    m_Project.State.ConsumePart(kv.Key, kv.Value);

                for (int i = 0; i < milestone.Materials.Count; i++)
                    m_Project.State.ConsumeMaterial(milestone.Materials[i].Material, milestone.Materials[i].Amount);

                m_Project.State.CompletedMilestones.Add(milestone.Key);
                m_Project.State.Stage = milestone.Stage;
                m_From.SendMessage("Milestone complete: {0}", milestone.Name);

                if (m_Project.State.CompletedCount >= m_Def.Milestones.Count)
                {
                    Item deed = ShipConstructionRegistry.CreateCompletedDeed(m_Project.State.HullType);
                    m_From.AddToBackpack(deed);
                    ReturnStoredMaterials();
                    m_From.SendMessage("Your ship project is complete! A deed has been issued.");

                    if (m_Project.Master != null && !m_Project.Master.Deleted)
                        m_Project.Master.AssignProject(null);

                    m_Project.Delete();
                }
            }
            else
            {
                foreach (KeyValuePair<string, int> kv in milestone.Parts)
                    m_Project.State.ConsumePart(kv.Key, Math.Max(1, kv.Value / 3));

                for (int i = 0; i < milestone.Materials.Count; i++)
                {
                    ShipMaterialRequirement req = milestone.Materials[i];
                    m_Project.State.ConsumeMaterial(req.Material, Math.Max(1, req.Amount / 3));
                }

                m_From.SendMessage("Assembly attempt failed. Some parts and materials were lost.");
            }
        }
    }
}

namespace Server.Multis
{
    using Server.Items;

    public class TinyBoatDeed : BaseBoatDeed
    {
        public override BaseBoat Boat { get { return new TinyBoat(); } }

        [Constructable]
        public TinyBoatDeed() : base(0x3C, Point3D.Zero)
        {
            Name = "skiff deed";
        }

        public TinyBoatDeed(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
