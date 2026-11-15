using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class CollectableCard : Item
    {
        private string m_CardName;
        private string m_Subtitle;
        private CardRarity m_Rarity;
        private CardCategory m_Category;
        private int m_CardImageID;
        private int m_DropSoundID;
        private string m_Description;
        private int m_CardBorderGumpID;

        [CommandProperty(AccessLevel.GameMaster)]
        public string CardName
        {
            get { return m_CardName; }
            set { m_CardName = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string Subtitle
        {
            get { return m_Subtitle; }
            set { m_Subtitle = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CardRarity Rarity
        {
            get { return m_Rarity; }
            set { m_Rarity = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CardCategory Category
        {
            get { return m_Category; }
            set { m_Category = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CardImageID
        {
            get { return m_CardImageID; }
            set { m_CardImageID = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DropSoundID
        {
            get { return m_DropSoundID; }
            set { m_DropSoundID = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CardBorderGumpID
        {
            get { return m_CardBorderGumpID; }
            set { m_CardBorderGumpID = value; InvalidateProperties(); }
        }

        public int AchievementPoints
        {
            get { return CardRarityInfo.GetPoints(m_Rarity); }
        }

        [Constructable]
        public CollectableCard() : this("Unknown Card", "", CardRarity.Common, CardCategory.Misc, 0, "")
        {
        }

        [Constructable]
        public CollectableCard(string cardName, string subtitle, CardRarity rarity, CardCategory category, int imageID, string description) 
            : this(cardName, subtitle, rarity, category, imageID, description, 0)
        {
        }

        [Constructable]
        public CollectableCard(string cardName, string subtitle, CardRarity rarity, CardCategory category, int imageID, string description, int borderGumpID) 
            : base(CardSystemConfig.CardItemID)
        {
            m_CardName = cardName;
            m_Subtitle = subtitle ?? "";
            m_Rarity = rarity;
            m_Category = category;
            m_CardImageID = imageID;
            m_Description = description ?? "";
            m_CardBorderGumpID = borderGumpID;

            Weight = 0.1;
            Hue = CardCategoryInfo.GetHue(category);
            Name = "Collectable Card";

            if (rarity >= CardRarity.Epic)
                Light = LightType.Circle150;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            // Card Title
            string titleColor = CardSystemConfig.GetTitleColor(this);
            list.Add(1070722, string.Format("<BASEFONT COLOR={0}>{1}</BASEFONT>", titleColor, m_CardName));

            // Subtitle
            if (!string.IsNullOrEmpty(m_Subtitle))
            {
                list.Add(1070722, string.Format("<BASEFONT COLOR={0}>{1}</BASEFONT>", CardSystemConfig.SubtitleColor, m_Subtitle));
            }

            // Rarity
            string rarityColorHex = CardSystemConfig.GetRarityColor(m_Rarity);
            string rarityName = CardRarityInfo.GetName(m_Rarity);
            list.Add(1070722, string.Format("Rarity: <BASEFONT COLOR={0}>{1}</BASEFONT>", rarityColorHex, rarityName));

            // Category
            string categoryName = CardCategoryInfo.GetName(m_Category);
            list.Add(1070722, string.Format("Category: {0}", categoryName));

            // Points (if enabled)
            if (CardSystemConfig.EnablePointsSystem)
            {
                list.Add(1070722, string.Format("{0} Points", AchievementPoints.ToString()));
            }

            // Description
            if (!string.IsNullOrEmpty(m_Description))
            {
                list.Add(1070722, string.Format("<BASEFONT COLOR={0}>{1}</BASEFONT>", CardSystemConfig.DescriptionColor, m_Description));
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(GetWorldLocation(), 3) || IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(CardViewGump));
                from.SendGump(new CardViewGump(this, from));
            }
            else
            {
                from.SendLocalizedMessage(500446);
            }
        }

        public CollectableCard(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)2);

            writer.Write(m_CardName);
            writer.Write(m_Subtitle);
            writer.Write((int)m_Rarity);
            writer.Write((int)m_Category);
            writer.Write(m_CardImageID);
            writer.Write(m_Description);
            writer.Write(m_CardBorderGumpID);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_CardName = reader.ReadString();
            m_Subtitle = reader.ReadString();
            m_Rarity = (CardRarity)reader.ReadInt();
            m_Category = (CardCategory)reader.ReadInt();
            m_CardImageID = reader.ReadInt();
            m_Description = reader.ReadString();

            // Only read border if version >= 2
            if (version >= 2)
            {
                m_CardBorderGumpID = reader.ReadInt();
            }
            else
            {
                // Default for old cards - no custom border
                m_CardBorderGumpID = 0;
            }

            Hue = CardCategoryInfo.GetHue(m_Category);
            Name = "Collectable Card";
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            list.Add(new ExamineCardMenuEntry(from, this));
            list.Add(new GuideCardMenuEntry(from, this));
        }

        private class ExamineCardMenuEntry : ContextMenuEntry
        {
            private Mobile m_From;
            private CollectableCard m_Card;

            public ExamineCardMenuEntry(Mobile from, CollectableCard card) : base(3006121)
            {
                m_From = from;
                m_Card = card;
            }

            public override void OnClick()
            {
                if (m_Card != null && !m_Card.Deleted)
                {
                    m_Card.OnDoubleClick(m_From);
                }
            }
        }

        private class GuideCardMenuEntry : ContextMenuEntry
        {
            private Mobile m_From;
            private CollectableCard m_Card;

            public GuideCardMenuEntry(Mobile from, CollectableCard card) : base(1075993)
            {
                m_From = from;
                m_Card = card;
            }

            public override void OnClick()
            {
                if (m_Card != null && !m_Card.Deleted)
                {
                    m_From.CloseGump(typeof(CardTomeGuideGump));
                    m_From.SendGump(new CardTomeGuideGump(m_From, null));
                }
            }
        }
    }
}
