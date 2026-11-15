using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Gumps
{
    public class SkillListingGump : Gump
    {
        private const int PrevPageButtonId = 1;
        private const int NextPageButtonId = 2;
        private const int ResetSessionButtonId = 3;

        private class SkillEntry
        {
            public SkillName SkillName;
            public string DisplayName;

            public SkillEntry(SkillName skillName, string displayName)
            {
                SkillName = skillName;
                DisplayName = displayName;
            }
        }

        private static readonly SkillEntry[] m_SkillEntries = new SkillEntry[]
        {
            new SkillEntry(SkillName.Alchemy, "Alchemy"),
            new SkillEntry(SkillName.Anatomy, "Anatomy"),
            new SkillEntry(SkillName.AnimalLore, "Animal Lore"),
            new SkillEntry(SkillName.AnimalTaming, "Animal Taming"),
            new SkillEntry(SkillName.Archery, "Archery"),
            new SkillEntry(SkillName.ArmsLore, "Arms Lore"),
            new SkillEntry(SkillName.Begging, "Begging"),
            new SkillEntry(SkillName.Blacksmith, "Blacksmithing"),
            new SkillEntry(SkillName.Bushido, "Bushido"),
            new SkillEntry(SkillName.Camping, "Camping"),
            new SkillEntry(SkillName.Carpentry, "Carpentry"),
            new SkillEntry(SkillName.Cartography, "Cartography"),
            new SkillEntry(SkillName.Chivalry, "Chivalry"),
            new SkillEntry(SkillName.Cooking, "Cooking"),
            new SkillEntry(SkillName.DetectHidden, "Detect Hidden"),
            new SkillEntry(SkillName.Discordance, "Discordance"),
            new SkillEntry(SkillName.EvalInt, "Eval Intelligence"),
            new SkillEntry(SkillName.Fencing, "Fencing"),
            new SkillEntry(SkillName.Fishing, "Fishing"),
            new SkillEntry(SkillName.Fletching, "Fletching"),
            new SkillEntry(SkillName.Focus, "Focus"),
            new SkillEntry(SkillName.Forensics, "Forensics"),
            new SkillEntry(SkillName.Healing, "Healing"),
            new SkillEntry(SkillName.Herding, "Herding"),
            new SkillEntry(SkillName.Hiding, "Hiding"),
            new SkillEntry(SkillName.Inscribe, "Inscription"),
            new SkillEntry(SkillName.ItemID, "Item ID"),
            new SkillEntry(SkillName.Lockpicking, "Lockpicking"),
            new SkillEntry(SkillName.Lumberjacking, "Lumberjacking"),
            new SkillEntry(SkillName.Macing, "Mace Fighting"),
            new SkillEntry(SkillName.Magery, "Magery"),
            new SkillEntry(SkillName.MagicResist, "Magic Resist"),
            new SkillEntry(SkillName.Meditation, "Meditation"),
            new SkillEntry(SkillName.Mining, "Mining"),
            new SkillEntry(SkillName.Musicianship, "Musicianship"),
            new SkillEntry(SkillName.Necromancy, "Necromancy"),
            new SkillEntry(SkillName.Ninjitsu, "Ninjitsu"),
            new SkillEntry(SkillName.Parry, "Parrying"),
            new SkillEntry(SkillName.Peacemaking, "Peacemaking"),
            new SkillEntry(SkillName.Poisoning, "Poisoning"),
            new SkillEntry(SkillName.Provocation, "Provocation"),
            new SkillEntry(SkillName.RemoveTrap, "Remove Trap"),
            new SkillEntry(SkillName.Snooping, "Snooping"),
            new SkillEntry(SkillName.SpiritSpeak, "Spirit Speak"),
            new SkillEntry(SkillName.Stealing, "Stealing"),
            new SkillEntry(SkillName.Stealth, "Stealth"),
            new SkillEntry(SkillName.Swords, "Swordsmanship"),
            new SkillEntry(SkillName.Tactics, "Tactics"),
            new SkillEntry(SkillName.Tailoring, "Tailoring"),
            new SkillEntry(SkillName.TasteID, "Taste ID"),
            new SkillEntry(SkillName.Tinkering, "Tinkering"),
            new SkillEntry(SkillName.Tracking, "Tracking"),
            new SkillEntry(SkillName.Veterinary, "Veterinary"),
            new SkillEntry(SkillName.Wrestling, "Wrestling")
        };

        private readonly int m_PageIndex;

        public static void RefreshSkillList(Mobile from)
        {
            if (from is PlayerMobile && from.HasGump(typeof(SkillListingGump)))
            {
                from.CloseGump(typeof(SkillListingGump));
                from.SendGump(new SkillListingGump(from, 0));
            }
        }

        public static void OpenSkillList(Mobile from)
        {
            if (from is PlayerMobile)
            {
                from.CloseGump(typeof(SkillListingGump));
                from.SendGump(new SkillListingGump(from, 0));
            }
        }

        public static void Initialize()
        {
            CommandSystem.Register("skilllist", AccessLevel.Player, new CommandEventHandler(NewSkillsGump_OnCommand));
            CommandSystem.Register("skillslist", AccessLevel.Player, new CommandEventHandler(NewSkillsGump_OnCommand));
        }

        [Usage("skilllist")]
        [Description("Shows the player the skills they want to watch.")]
        public static void NewSkillsGump_OnCommand(CommandEventArgs e)
        {
            OpenSkillList(e.Mobile);
        }

        public SkillListingGump(Mobile from, int pageIndex) : base(25, 25)
        {
            m_PageIndex = pageIndex;

            CharacterDatabase db = CharacterDatabase.GetDB(from);
            int skillDisplay = db.SkillDisplay;
            PlayerMobile pm = from as PlayerMobile;

            if (pm != null)
                pm.CaptureSkillSessionBaseline();

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            List<SkillEntry> watchedSkills = new List<SkillEntry>();

            for (int i = 0; i < m_SkillEntries.Length; ++i)
            {
                Skill skill = from.Skills[m_SkillEntries[i].SkillName];

                if (skill != null && (skill.Lock == SkillLock.Up || (skill.Lock == SkillLock.Locked && skillDisplay > 0)))
                    watchedSkills.Add(m_SkillEntries[i]);
            }

            int skillsPerPage = Math.Max(1, (watchedSkills.Count + 1) / 2);
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)watchedSkills.Count / skillsPerPage));
            int page = Math.Max(0, Math.Min(pageIndex, totalPages - 1));

            int start = page * skillsPerPage;
            int count = Math.Min(skillsPerPage, watchedSkills.Count - start);
            int rows = count + 3; // header + control/footer space

            AddPage(0);

            AddImage(0, 0, 164);
            AddImage(86, 0, 164);
            AddImage(172, 0, 164);
            AddImage(258, 0, 164);

            if (rows > 6)
            {
                int fillRows = rows - 6;
                int u = 0;
                int o = 2;

                while (fillRows > 0)
                {
                    u += 24;
                    AddImage(0, u, 164);
                    AddImage(86, u, 164);
                    AddImage(172, u, 164);
                    AddImage(258, u, 164);
                    fillRows--;
                }

                int trimRows = rows - 6;

                while (trimRows > 0)
                {
                    o += 24;
                    AddImage(2, o, 165);
                    AddImage(88, o, 165);
                    AddImage(174, o, 165);
                    AddImage(260, o, 165);
                    trimRows--;
                }
            }

            AddImage(2, 2, 165);
            AddImage(88, 2, 165);
            AddImage(174, 2, 165);
            AddImage(260, 2, 165);

            AddLabel(8, 8, 1152, "Skill");
            AddLabel(140, 8, 1152, "Real");
            AddLabel(198, 8, 1152, "Current");
            AddLabel(266, 8, 1152, "Session");
            AddLabel(340, 8, 1152, "Last");

            int navY = 52 + (count * 24);
            AddLabel(8, navY, 1153, String.Format("Page {0}/{1}", page + 1, totalPages));

            if (page > 0)
                AddButton(120, navY, 4014, 4016, PrevPageButtonId, GumpButtonType.Reply, 0);

            if (page < totalPages - 1)
                AddButton(150, navY, 4005, 4007, NextPageButtonId, GumpButtonType.Reply, 0);

            AddButton(220, navY, 4005, 4007, ResetSessionButtonId, GumpButtonType.Reply, 0);
            AddLabel(252, navY, 1153, "Reset Session");

            int y = 32;

            for (int i = 0; i < count; ++i)
            {
                SkillEntry entry = watchedSkills[start + i];
                Skill skill = from.Skills[entry.SkillName];
                int rowColor = skill.Lock == SkillLock.Locked ? 0x31 : 1153;

                double gain = 0.0;
                double lastGain = 0.0;

                if (pm != null)
                {
                    gain = pm.GetSkillSessionGain(entry.SkillName);
                    lastGain = pm.GetLastLoginSkillGain(entry.SkillName);
                }

                int gainColor = gain > 0.0 ? 68 : (gain < 0.0 ? 33 : 1153);
                int lastGainColor = lastGain > 0.0 ? 68 : (lastGain < 0.0 ? 33 : 1153);

                AddLabel(8, y, rowColor, entry.DisplayName);
                AddLabel(140, y, rowColor, skill.Base.ToString("0.0"));
                AddLabel(198, y, rowColor, skill.Value.ToString("0.0"));
                AddLabel(266, y, gainColor, gain.ToString("+0.0;-0.0;0.0"));
                AddLabel(340, y, lastGainColor, lastGain.ToString("+0.0;-0.0;0.0"));

                y += 24;
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (info.ButtonID == PrevPageButtonId)
            {
                from.SendGump(new SkillListingGump(from, m_PageIndex - 1));
                return;
            }

            if (info.ButtonID == NextPageButtonId)
            {
                from.SendGump(new SkillListingGump(from, m_PageIndex + 1));
                return;
            }

            if (info.ButtonID == ResetSessionButtonId)
            {
                PlayerMobile pm = from as PlayerMobile;

                if (pm != null)
                {
                    pm.ResetSkillSessionBaseline();
                    pm.CaptureSkillSessionBaseline();
                }

                from.SendGump(new SkillListingGump(from, m_PageIndex));
                return;
            }

            from.CloseGump(typeof(SkillListingGump));
        }
    }
}
