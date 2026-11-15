using System;
using Server.Gumps;
using Server.Network;
using Server.Items;

namespace Server.Items
{
    public class CollectableCardMaker : Item
    {
        [Constructable]
        public CollectableCardMaker() : base(CardSystemConfig.CardItemID)
        {
            Name = "Blank Card";
            Hue = 0;
            Weight = 0.1;
        }

        public override void OnDoubleClick(Mobile from)
        {
            // Only allow use if the blank card is in the player's backpack
            if (IsChildOf(from.Backpack))
            {
                if (from.AccessLevel >= AccessLevel.GameMaster)
                {
                    from.SendGump(new CreateCardGump(this));
                }
                else
                {
                    from.SendMessage("You cannot use this.");
                }
            }
            else
            {
                from.SendMessage("This must be in your backpack to use.");
            }
        }

        public CollectableCardMaker(Serial serial) : base(serial)
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

    public class CreateCardGump : Gump
    {
        private CollectableCardMaker m_Blank;
        private int m_SelectedRarity;
        private int m_SelectedCategory;
        private string m_CardName;
        private string m_Subtitle;
        private string m_ImageID;
        private string m_Description;
        private string m_BorderID;

        public CreateCardGump(CollectableCardMaker blank) : this(blank, 0, 0, "", "", "172", "", "0")
        {
        }

        public CreateCardGump(CollectableCardMaker blank, int selectedRarity, int selectedCategory, string cardName, string subtitle, string imageID, string description, string borderID) : base(50, 50)
        {
            m_Blank = blank;
            m_SelectedRarity = selectedRarity;
            m_SelectedCategory = selectedCategory;
            m_CardName = cardName;
            m_Subtitle = subtitle;
            m_ImageID = imageID;
            m_Description = description;
            m_BorderID = borderID;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            BuildGump();
        }

        private void BuildGump()
        {
            AddPage(0);

            AddBackground(0, 0, 650, 470, 9270);
            AddBackground(10, 10, 630, 450, 9300);

            AddHtml(240, 15, 200, 25, "<BASEFONT COLOR=#000000><CENTER><H3>Create Collectable Card</H3></CENTER></BASEFONT>", false, false);

            // --- Left panel ---
            int leftX = 20;
            int currentY = 50;
            int fieldWidth = 200;
            int labelWidth = 80;

            // Card Name
            AddHtml(leftX, currentY, labelWidth, 20, "<BASEFONT COLOR=#000000>Card Name:</BASEFONT>", false, false);
            AddTextEntry(leftX + labelWidth, currentY, fieldWidth, 20, 0, 1, m_CardName);
            currentY += 30;

            // Subtitle
            AddHtml(leftX, currentY, labelWidth, 20, "<BASEFONT COLOR=#000000>Subtitle:</BASEFONT>", false, false);
            AddTextEntry(leftX + labelWidth, currentY, fieldWidth, 20, 0, 2, m_Subtitle);
            currentY += 30;

            // Image ID
            AddHtml(leftX, currentY, labelWidth, 20, "<BASEFONT COLOR=#000000>Image ID:</BASEFONT>", false, false);
            AddTextEntry(leftX + labelWidth, currentY, 80, 20, 0, 3, m_ImageID);
            currentY += 30;

            // Border ID
            AddHtml(leftX, currentY, labelWidth, 40, "<BASEFONT COLOR=#000000>Border ID: (0=default)</BASEFONT>", false, false);
            AddTextEntry(leftX + labelWidth, currentY, 80, 20, 0, 5, m_BorderID);
            currentY += 55;

            // Description
            AddHtml(leftX, currentY, labelWidth, 20, "<BASEFONT COLOR=#000000>Description:</BASEFONT>", false, false);
            AddTextEntry(leftX + labelWidth, currentY, fieldWidth, 60, 0, 4, m_Description);
            currentY += 70;

            // Rarity
            int rarityStartY = currentY;
            AddHtml(leftX, currentY, 100, 20, "<BASEFONT COLOR=#000000>Rarity:</BASEFONT>", false, false);
            currentY += 22;

            AddRarityButton(leftX + 10, currentY, "Common", 100, m_SelectedRarity == 0);
            currentY += 22;
            AddRarityButton(leftX + 10, currentY, "Uncommon", 101, m_SelectedRarity == 1);
            currentY += 22;
            AddRarityButton(leftX + 10, currentY, "Rare", 102, m_SelectedRarity == 2);
            currentY += 22;
            AddRarityButton(leftX + 10, currentY, "Epic", 103, m_SelectedRarity == 3);
            currentY += 22;
            AddRarityButton(leftX + 10, currentY, "Legendary", 104, m_SelectedRarity == 4);
            currentY += 22;
            AddRarityButton(leftX + 10, currentY, "Special", 105, m_SelectedRarity == 5);

            // Category
            int categoryX = leftX + 155;
            int categoryY = rarityStartY;

            AddHtml(categoryX, categoryY, 100, 20, "<BASEFONT COLOR=#000000>Category:</BASEFONT>", false, false);
            categoryY += 22;

            AddCategoryButton(categoryX + 10, categoryY, "Boss", 200, m_SelectedCategory == 0);
            categoryY += 22;
            AddCategoryButton(categoryX + 10, categoryY, "Dungeon", 201, m_SelectedCategory == 1);
            categoryY += 22;
            AddCategoryButton(categoryX + 10, categoryY, "Legends", 202, m_SelectedCategory == 2);
            categoryY += 22;
            AddCategoryButton(categoryX + 10, categoryY, "History", 203, m_SelectedCategory == 3);
            categoryY += 22;
            AddCategoryButton(categoryX + 10, categoryY, "Server Event", 204, m_SelectedCategory == 4);
            categoryY += 22;
            AddCategoryButton(categoryX + 10, categoryY, "Town", 205, m_SelectedCategory == 5);
            categoryY += 22;
            AddCategoryButton(categoryX + 10, categoryY, "Miscellaneous", 206, m_SelectedCategory == 6);

            // --- Right panel ---
            int previewX = 400;
            int previewY = 50;

            AddHtml(previewX + 5, previewY, 200, 20, "<BASEFONT COLOR=#000000><CENTER>Card Preview</CENTER></BASEFONT>", false, false);
	    previewY += 20;

            // Parse image ID for preview
            int imageID = 172;
            int.TryParse(m_ImageID, out imageID);

            // Parse border ID for preview
            int borderID = 0;
            int.TryParse(m_BorderID, out borderID);

            CardRarity previewRarity = CardRarity.Common;
            switch (m_SelectedRarity)
            {
                case 1: previewRarity = CardRarity.Uncommon; break;
                case 2: previewRarity = CardRarity.Rare; break;
                case 3: previewRarity = CardRarity.Epic; break;
                case 4: previewRarity = CardRarity.Legendary; break;
                case 5: previewRarity = CardRarity.Special; break;
                default: previewRarity = CardRarity.Common; break;
            }

            CardCategory previewCategory = CardCategory.Misc;
            switch (m_SelectedCategory)
            {
                case 0: previewCategory = CardCategory.Boss; break;
                case 1: previewCategory = CardCategory.Dungeon; break;
                case 2: previewCategory = CardCategory.Legends; break;
                case 3: previewCategory = CardCategory.History; break;
                case 4: previewCategory = CardCategory.ServerEvent; break;
                case 5: previewCategory = CardCategory.Town; break;
                default: previewCategory = CardCategory.Misc; break;
            }

            // Build the preview card
            AddPreviewCard(previewX, previewY, m_CardName, m_Subtitle, previewRarity, imageID, previewCategory, borderID);

            int buttonY = previewY + 330;
            int buttonCenterX = previewX + 100;

            // Refresh button
            AddButton(buttonCenterX - 75, buttonY, 4005, 4007, 300, GumpButtonType.Reply, 0);
            AddHtml(buttonCenterX - 40, buttonY + 3, 60, 20, "<BASEFONT COLOR=#000000>Refresh</BASEFONT>", false, false);

            // Create button
            AddButton(buttonCenterX + 15, buttonY, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtml(buttonCenterX + 50, buttonY + 3, 60, 20, "<BASEFONT COLOR=#000000>Create</BASEFONT>", false, false);
        }

        private void AddRarityButton(int x, int y, string name, int buttonId, bool isSelected)
        {
            if (isSelected)
            {
                AddImage(x, y, 211);
                AddHtml(x + 22, y + 2, 80, 20, string.Format("<BASEFONT COLOR=#000000>{0}</BASEFONT>", name), false, false);
            }
            else
            {
                AddButton(x, y, 210, 211, buttonId, GumpButtonType.Reply, 0);
                AddHtml(x + 22, y + 2, 80, 20, string.Format("<BASEFONT COLOR=#000000>{0}</BASEFONT>", name), false, false);
            }
        }

        private void AddCategoryButton(int x, int y, string name, int buttonId, bool isSelected)
        {
            if (isSelected)
            {
                AddImage(x, y, 211);
                AddHtml(x + 22, y + 2, 90, 20, string.Format("<BASEFONT COLOR=#000000>{0}</BASEFONT>", name), false, false);
            }
            else
            {
                AddButton(x, y, 210, 211, buttonId, GumpButtonType.Reply, 0);
                AddHtml(x + 22, y + 2, 90, 20, string.Format("<BASEFONT COLOR=#000000>{0}</BASEFONT>", name), false, false);
            }
        }

        private void AddPreviewCard(int x, int y, string cardName, string subtitle, CardRarity rarity, int imageID, CardCategory category, int borderID)
        {
            CardRenderer.RenderCard(this, x, y, cardName, subtitle, rarity, category, imageID, borderID);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            // Get current values from text entries
            TextRelay nameEntry = info.GetTextEntry(1);
            TextRelay subtitleEntry = info.GetTextEntry(2);
            TextRelay imageEntry = info.GetTextEntry(3);
            TextRelay descEntry = info.GetTextEntry(4);
            TextRelay borderEntry = info.GetTextEntry(5);

            string currentCardName = nameEntry != null ? nameEntry.Text : m_CardName;
            string currentSubtitle = subtitleEntry != null ? subtitleEntry.Text : m_Subtitle;
            string currentImageID = imageEntry != null ? imageEntry.Text : m_ImageID;
            string currentDescription = descEntry != null ? descEntry.Text : m_Description;
            string currentBorderID = borderEntry != null ? borderEntry.Text : m_BorderID;

            // Refresh/Preview button (300)
            if (info.ButtonID == 300)
            {
                from.SendGump(new CreateCardGump(m_Blank, m_SelectedRarity, m_SelectedCategory, 
                            currentCardName, currentSubtitle, currentImageID, currentDescription, currentBorderID));
                return;
            }

            // Rarity selection buttons (100-105)
            if (info.ButtonID >= 100 && info.ButtonID <= 105)
            {
                int newRarity = info.ButtonID - 100;
                from.SendGump(new CreateCardGump(m_Blank, newRarity, m_SelectedCategory, 
                            currentCardName, currentSubtitle, currentImageID, currentDescription, currentBorderID));
                return;
            }

            // Category selection buttons (200-206)
            if (info.ButtonID >= 200 && info.ButtonID <= 206)
            {
                int newCategory = info.ButtonID - 200;
                from.SendGump(new CreateCardGump(m_Blank, m_SelectedRarity, newCategory, 
                            currentCardName, currentSubtitle, currentImageID, currentDescription, currentBorderID));
                return;
            }

            // Create button
            if (info.ButtonID == 1 && m_Blank != null && !m_Blank.Deleted)
            {
                int imageID = 172;
                if (!int.TryParse(currentImageID, out imageID))
                    imageID = 172;

                int borderID = 0;
                if (!int.TryParse(currentBorderID, out borderID))
                    borderID = 0;

                // Map selected rarity index to enum
                CardRarity rarity = CardRarity.Common;
                switch (m_SelectedRarity)
                {
                    case 1: rarity = CardRarity.Uncommon; break;
                    case 2: rarity = CardRarity.Rare; break;
                    case 3: rarity = CardRarity.Epic; break;
                    case 4: rarity = CardRarity.Legendary; break;
                    case 5: rarity = CardRarity.Special; break;
                    default: rarity = CardRarity.Common; break;
                }

                // Map selected category index to enum
                CardCategory category = CardCategory.Misc;
                switch (m_SelectedCategory)
                {
                    case 0: category = CardCategory.Boss; break;
                    case 1: category = CardCategory.Dungeon; break;
                    case 2: category = CardCategory.Legends; break;
                    case 3: category = CardCategory.History; break;
                    case 4: category = CardCategory.ServerEvent; break;
                    case 5: category = CardCategory.Town; break;
                    default: category = CardCategory.Misc; break;
                }

                CollectableCard card = new CollectableCard(currentCardName, currentSubtitle, rarity, category, imageID, currentDescription, borderID);
                from.AddToBackpack(card);
                from.SendMessage("Card created successfully!");

                m_Blank.Delete();
            }
        }
    }
}
