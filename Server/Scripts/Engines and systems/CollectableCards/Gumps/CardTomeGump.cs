using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Items;

namespace Server.Gumps
{
    public class CardTomeGump : Gump
    {
        private Mobile m_From;
        private CardTome m_Tome;
        private int m_Page;
        private CardCategory m_CurrentCategory;
        private List<CollectableCard> m_CurrentCards;
        private int m_CardsPerPage = 2;

        private const int CategoryPage = 0;

        public CardTomeGump(Mobile from, CardTome book) : this(from, book, CategoryPage, CardCategory.Boss)
        {
        }

        public CardTomeGump(Mobile from, CardTome book, int page) : this(from, book, page, CardCategory.Boss)
        {
        }

        public CardTomeGump(Mobile from, CardTome book, int page, CardCategory category) : base(50, 50)
        {
            m_From = from;
            m_Tome = book;
            m_Page = page;
            m_CurrentCategory = category;
            m_CurrentCards = m_Tome.GetCardsInCategory(category);

            BuildGump();
        }

        private void BuildGump()
        {
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            // Book background
            AddImage(0, 0, 1054);

            // Left side navigation (bookmarks)
            BuildNavigation();

            // Main content area (pages)
            AddPage(1);

            if (m_Page == CategoryPage)
            {
                BuildHomePage();
            }
            else
            {
                BuildCategoryPage();
            }
        }

        private void BuildNavigation()
        {
            int y = 86;

            // Home bookmark
            int homeButton = (m_Page == CategoryPage) ? 9780 : 9781;
            AddButton(23, y, homeButton, homeButton, 100, GumpButtonType.Reply, 0);
            AddOutlinedText(40, y + 4, 20, 20, "*", "#FFFFFF", true);
            y += 35;

            // Category bookmarks
            if (CardSystemConfig.EnableBossCategory)
            {
                string letter = GetBookmarkLetter(CardSystemConfig.BossDisplayName, "B");
                bool isActive = (m_CurrentCategory == CardCategory.Boss && m_Page != CategoryPage);
                AddCategoryBookmark(y, 201, letter, CardCategory.Boss, isActive);
                y += 35;
            }
            if (CardSystemConfig.EnableDungeonCategory)
            {
                string letter = GetBookmarkLetter(CardSystemConfig.DungeonDisplayName, "D");
                bool isActive = (m_CurrentCategory == CardCategory.Dungeon && m_Page != CategoryPage);
                AddCategoryBookmark(y, 202, letter, CardCategory.Dungeon, isActive);
                y += 35;
            }
            if (CardSystemConfig.EnableLegendsCategory)
            {
                string letter = GetBookmarkLetter(CardSystemConfig.LegendsDisplayName, "L");
                bool isActive = (m_CurrentCategory == CardCategory.Legends && m_Page != CategoryPage);
                AddCategoryBookmark(y, 203, letter, CardCategory.Legends, isActive);
                y += 35;
            }
            if (CardSystemConfig.EnableHistoryCategory)
            {
                string letter = GetBookmarkLetter(CardSystemConfig.HistoryDisplayName, "H");
                bool isActive = (m_CurrentCategory == CardCategory.History && m_Page != CategoryPage);
                AddCategoryBookmark(y, 204, letter, CardCategory.History, isActive);
                y += 35;
            }
            if (CardSystemConfig.EnableServerEventCategory)
            {
                string letter = GetBookmarkLetter(CardSystemConfig.ServerEventDisplayName, "E");
                bool isActive = (m_CurrentCategory == CardCategory.ServerEvent && m_Page != CategoryPage);
                AddCategoryBookmark(y, 205, letter, CardCategory.ServerEvent, isActive);
                y += 35;
            }
            if (CardSystemConfig.EnableTownCategory)
            {
                string letter = GetBookmarkLetter(CardSystemConfig.TownDisplayName, "T");
                bool isActive = (m_CurrentCategory == CardCategory.Town && m_Page != CategoryPage);
                AddCategoryBookmark(y, 206, letter, CardCategory.Town, isActive);
                y += 35;
            }
            if (CardSystemConfig.EnableMiscCategory)
            {
                string letter = GetBookmarkLetter(CardSystemConfig.MiscDisplayName, "M");
                bool isActive = (m_CurrentCategory == CardCategory.Misc && m_Page != CategoryPage);
                AddCategoryBookmark(y, 207, letter, CardCategory.Misc, isActive);
                y += 35;
            }
        }

        private string GetBookmarkLetter(string displayName, string defaultLetter)
        {
            if (string.IsNullOrEmpty(displayName))
                return defaultLetter;

            // Return the first character of the display name
            return displayName[0].ToString().ToUpper();
        }

        private void AddCategoryBookmark(int y, int buttonId, string label, CardCategory category, bool isActive)
        {
            // Use different button graphics based on active state
            // 9781 = normal (gray/dark), 9780 = pressed/active (red)
            int buttonGump = isActive ? 9780 : 9781;

            AddButton(23, y, buttonGump, buttonGump, buttonId, GumpButtonType.Reply, 0);
            string colorHex = CardSystemConfig.GetCategoryColor(category);
            AddOutlinedText(40, y + 4, 20, 20, label, colorHex, true);
        }

        private void BuildHomePage()
        {
            string ownerName = m_Tome.OwnerName;

            // Left Page - Owner Info
            string ownerText = String.Format("{0}'s Card Collection", ownerName);
            if (ownerName == "Unknown")
                ownerText = "Card Collection";

            AddOutlinedText(100, 30, 200, 25, ownerText, CardSystemConfig.HeaderTextColor, true);

            // Count how many categories are enabled
            int enabledCategoryCount = 0;
            if (CardSystemConfig.EnableBossCategory) enabledCategoryCount++;
            if (CardSystemConfig.EnableDungeonCategory) enabledCategoryCount++;
            if (CardSystemConfig.EnableLegendsCategory) enabledCategoryCount++;
            if (CardSystemConfig.EnableHistoryCategory) enabledCategoryCount++;
            if (CardSystemConfig.EnableServerEventCategory) enabledCategoryCount++;
            if (CardSystemConfig.EnableTownCategory) enabledCategoryCount++;
            if (CardSystemConfig.EnableMiscCategory) enabledCategoryCount++;

            // Base Y positions
            int statsY1 = 48;
            int statsY2 = 65;
            int categoriesHeaderY = 90;
            int firstCategoryY = 120;
            int categorySpacing = 37;

            // Adjust for fewer categories
            if (enabledCategoryCount <= 6)
            {
                int reduction = (7 - enabledCategoryCount) * 2;
                statsY1 -= reduction;
                statsY2 -= reduction;
                categoriesHeaderY -= reduction;
                firstCategoryY -= reduction;
            }

            // Stats
            string cardsText = String.Format("{0} Cards Total", m_Tome.CardCount);
            AddOutlinedText(100, statsY1, 200, 20, cardsText, CardSystemConfig.StatTextColor, true);

            if (CardSystemConfig.EnablePointsSystem)
            {
                string pointsText = String.Format("{0} Points", m_Tome.TotalAchievementPoints);
                AddOutlinedText(100, statsY2, 200, 20, pointsText, CardSystemConfig.StatTextColor, true);
            }

            // Categories header
            AddOutlinedText(100, categoriesHeaderY, 200, 20, "Categories", CardSystemConfig.HeaderTextColor, true);

            int y = firstCategoryY;

            // Category buttons
            if (CardSystemConfig.EnableBossCategory)
            {
                AddCategorySelectionButton(100, y, CardCategory.Boss, CardSystemConfig.BossDisplayName);
                y += categorySpacing;
            }
            if (CardSystemConfig.EnableDungeonCategory)
            {
                AddCategorySelectionButton(100, y, CardCategory.Dungeon, CardSystemConfig.DungeonDisplayName);
                y += categorySpacing;
            }
            if (CardSystemConfig.EnableLegendsCategory)
            {
                AddCategorySelectionButton(100, y, CardCategory.Legends, CardSystemConfig.LegendsDisplayName);
                y += categorySpacing;
            }
            if (CardSystemConfig.EnableHistoryCategory)
            {
                AddCategorySelectionButton(100, y, CardCategory.History, CardSystemConfig.HistoryDisplayName);
                y += categorySpacing;
            }
            if (CardSystemConfig.EnableServerEventCategory)
            {
                AddCategorySelectionButton(100, y, CardCategory.ServerEvent, CardSystemConfig.ServerEventDisplayName);
                y += categorySpacing;
            }
            if (CardSystemConfig.EnableTownCategory)
            {
                AddCategorySelectionButton(100, y, CardCategory.Town, CardSystemConfig.TownDisplayName);
                y += categorySpacing;
            }
            if (CardSystemConfig.EnableMiscCategory)
            {
                AddCategorySelectionButton(100, y, CardCategory.Misc, CardSystemConfig.MiscDisplayName);
            }

            // Right Page - Featured Card header
            AddOutlinedText(400, 30, 210, 20, "Latest Card", CardSystemConfig.HeaderTextColor, true);

            CollectableCard featured = m_Tome.FeaturedCard;
            if (featured != null)
            {
                AddCardDisplay(400, 55, featured, -1);
            }
            else
            {
                AddEmptyCardDisplayWithMessage(400, 55, "No cards yet.\nDrag cards onto the tome\nin the inventory to add\nto thy collection.");
            }
        }

        private void AddOutlinedText(int x, int y, int width, int height, string text, string colorHex, bool centered = false)
        {
            string outlineColor = "#000000";
            string alignment = centered ? "<CENTER>" : "";
            string closeAlignment = centered ? "</CENTER>" : "";

            // Full 8-direction outline
            // Top row
            AddHtml(x - 1, y - 1, width, height, 
                    String.Format("<BASEFONT COLOR={0}>{1}{2}{3}</BASEFONT>", outlineColor, alignment, text, closeAlignment), 
                    false, false);
            AddHtml(x, y - 1, width, height, 
                    String.Format("<BASEFONT COLOR={0}>{1}{2}{3}</BASEFONT>", outlineColor, alignment, text, closeAlignment), 
                    false, false);
            AddHtml(x + 1, y - 1, width, height, 
                    String.Format("<BASEFONT COLOR={0}>{1}{2}{3}</BASEFONT>", outlineColor, alignment, text, closeAlignment), 
                    false, false);

            // Middle row
            AddHtml(x - 1, y, width, height, 
                    String.Format("<BASEFONT COLOR={0}>{1}{2}{3}</BASEFONT>", outlineColor, alignment, text, closeAlignment), 
                    false, false);
            AddHtml(x + 1, y, width, height, 
                    String.Format("<BASEFONT COLOR={0}>{1}{2}{3}</BASEFONT>", outlineColor, alignment, text, closeAlignment), 
                    false, false);

            // Bottom row
            AddHtml(x - 1, y + 1, width, height, 
                    String.Format("<BASEFONT COLOR={0}>{1}{2}{3}</BASEFONT>", outlineColor, alignment, text, closeAlignment), 
                    false, false);
            AddHtml(x, y + 1, width, height, 
                    String.Format("<BASEFONT COLOR={0}>{1}{2}{3}</BASEFONT>", outlineColor, alignment, text, closeAlignment), 
                    false, false);
            AddHtml(x + 1, y + 1, width, height, 
                    String.Format("<BASEFONT COLOR={0}>{1}{2}{3}</BASEFONT>", outlineColor, alignment, text, closeAlignment), 
                    false, false);

            // Main colored text
            AddHtml(x, y, width, height, 
                    String.Format("<BASEFONT COLOR={0}>{1}{2}{3}</BASEFONT>", colorHex, alignment, text, closeAlignment), 
                    false, false);
        }

        private void AddCategorySelectionButton(int x, int y, CardCategory category, string name)
        {
            int count = m_Tome.GetCardCountInCategory(category);
            string colorHex = CardSystemConfig.GetCategoryColor(category);

            // Category name button
            AddButton(x, y, 30008, 30008, 500 + (int)category, GumpButtonType.Reply, 0);

            // Category name with outline
            AddOutlinedText(x + 28, y - 2, 100, 20, name, colorHex, false);

            // Count text
            string countText = String.Format("{0} Collected", count);
            string countColor = "#BDBDC7";

            // Outline for count text
            string outlineColor = "#000000";

            // Count text outline
            AddHtml(x + 28, y + 15, 100, 15, 
                    String.Format("<BASEFONT COLOR={0}>{1}</BASEFONT>", outlineColor, countText), false, false);
            AddHtml(x + 28, y + 17, 100, 15, 
                    String.Format("<BASEFONT COLOR={0}>{1}</BASEFONT>", outlineColor, countText), false, false);
            AddHtml(x + 27, y + 16, 100, 15, 
                    String.Format("<BASEFONT COLOR={0}>{1}</BASEFONT>", outlineColor, countText), false, false);
            AddHtml(x + 29, y + 16, 100, 15, 
                    String.Format("<BASEFONT COLOR={0}>{1}</BASEFONT>", outlineColor, countText), false, false);

            // Count text main
            AddHtml(x + 28, y + 16, 100, 15, 
                    String.Format("<BASEFONT COLOR={0}>{1}</BASEFONT>", countColor, countText), false, false);
        }

        private void BuildCategoryPage()
        {
            int cardsCount = m_CurrentCards.Count;
            int startIndex = (m_Page - 1) * m_CardsPerPage;
            int totalPages = Math.Max(1, (int)Math.Ceiling(cardsCount / (double)m_CardsPerPage));

            // Left header: Category Name (centered over left card)
            string categoryName = CardCategoryInfo.GetName(m_CurrentCategory);

            // Right header: Page X of Y (centered over right card)
            string pageText = String.Format("Page {0} of {1}", m_Page, totalPages);

            // Determine header color based on TitleColorByCategory setting
            string headerColorHex;
            if (CardSystemConfig.TitleColorByCategory)
            {
                // Titles use category colors, so headers use neutral color
                headerColorHex = CardSystemConfig.HeaderTextColor;
            }
            else
            {
                // Titles use rarity colors, so headers use category color
                headerColorHex = CardSystemConfig.GetCategoryColor(m_CurrentCategory);
            }

            // Position over left card
            int leftHeaderX = 80;
            int rightHeaderX = 400;
            int headerWidth = 210;
            string outlineColor = "#000000";

            // Left header outline (centered)
            AddHtml(leftHeaderX - 1, 29, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, categoryName), false, false);
            AddHtml(leftHeaderX, 29, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, categoryName), false, false);
            AddHtml(leftHeaderX + 1, 29, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, categoryName), false, false);
            AddHtml(leftHeaderX - 1, 30, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, categoryName), false, false);
            AddHtml(leftHeaderX + 1, 30, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, categoryName), false, false);
            AddHtml(leftHeaderX - 1, 31, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, categoryName), false, false);
            AddHtml(leftHeaderX, 31, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, categoryName), false, false);
            AddHtml(leftHeaderX + 1, 31, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, categoryName), false, false);

            // Left header main text
            AddHtml(leftHeaderX, 30, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", headerColorHex, categoryName), false, false);

            // Right header outline
            AddHtml(rightHeaderX - 1, 29, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, pageText), false, false);
            AddHtml(rightHeaderX, 29, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, pageText), false, false);
            AddHtml(rightHeaderX + 1, 29, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, pageText), false, false);
            AddHtml(rightHeaderX - 1, 30, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, pageText), false, false);
            AddHtml(rightHeaderX + 1, 30, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, pageText), false, false);
            AddHtml(rightHeaderX - 1, 31, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, pageText), false, false);
            AddHtml(rightHeaderX, 31, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, pageText), false, false);
            AddHtml(rightHeaderX + 1, 31, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", outlineColor, pageText), false, false);

            // Right header main text
            AddHtml(rightHeaderX, 30, headerWidth, 25, 
                    String.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", headerColorHex, pageText), false, false);

            // Page navigation buttons
            if (m_Page > 1)
            {
                AddButton(53, 16, 1055, 1055, 400, GumpButtonType.Reply, 0);
            }

            if (m_Page < totalPages)
            {
                AddButton(586, 16, 1056, 1056, 401, GumpButtonType.Reply, 0);
            }

            // Get cards for this spread
            CollectableCard leftCard = (startIndex < cardsCount) ? m_CurrentCards[startIndex] : null;
            CollectableCard rightCard = (startIndex + 1 < cardsCount) ? m_CurrentCards[startIndex + 1] : null;

            // Left card
            if (leftCard != null)
            {
                AddCardDisplay(80, 55, leftCard, startIndex);
            }
            else
            {
                AddEmptyCardDisplay(80, 55);
            }

            // Right card
            if (rightCard != null)
            {
                AddCardDisplay(400, 55, rightCard, startIndex + 1);
            }
            else
            {
                AddEmptyCardDisplay(400, 55);
            }
        }

        private void AddEmptyCardDisplay(int x, int y)
        {
            CardRenderer.RenderEmptyCard(this, x, y);
        }

        private void AddEmptyCardDisplayWithMessage(int x, int y, string message)
        {
            CardRenderer.RenderEmptyCardWithMessage(this, x, y, message);
        }

        private void AddCardDisplay(int x, int y, CollectableCard card, int cardIndex)
        {
            if (card == null) return;

            CardRenderer.RenderCard(this, x, y, card.CardName, card.Subtitle, card.Rarity, card.Category, card.CardImageID, card.CardBorderGumpID);

            // Remove button - only show if the user can modify the tome
            bool canModify = false;

            // GM can always modify
            if (m_From.AccessLevel >= AccessLevel.GameMaster)
                canModify = true;
            // Tome in player's backpack
            else if (m_Tome.IsChildOf(m_From.Backpack))
                canModify = true;
            // Tome in player's bank box
            else if (m_Tome.RootParent is BankBox)
            {
                BankBox bankBox = (BankBox)m_Tome.RootParent;
                if (bankBox.Owner == m_From)
                    canModify = true;
            }
            // Tome is bound to this player and on ground/locked down
            else if (!string.IsNullOrEmpty(m_Tome.OwnerName) && m_Tome.OwnerName == m_From.Name)
                canModify = true;

            // Do not show remove buttons if the tome is inside a player vendor
            Item parent = m_Tome.RootParent as Item;
            while (parent != null)
            {
                if (parent is PlayerVendor)
                {
                    canModify = false;
                    break;
                }
                parent = parent.RootParent as Item;
            }

            if (cardIndex >= 0 && canModify)
            {
                if (CardSystemConfig.UseWoodenBorderStyle)
                {
                    AddButton(x + 169, y + 11, 4017, 4018, 1000 + cardIndex, GumpButtonType.Reply, 0);
                }
                else
                {
                    AddButton(x + 172, y + 8, 4017, 4018, 1000 + cardIndex, GumpButtonType.Reply, 0);
                }
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Tome.Deleted)
                return;

            // Right-click close detection
            if (info.ButtonID == 0)
            {
                m_From.PlaySound(CardSystemConfig.TomeCloseSound);
                return;
            }

            // play navigation sound for all interactive buttons
            if (info.ButtonID == 100 ||
                    (info.ButtonID >= 201 && info.ButtonID <= 207) ||
                    (info.ButtonID >= 400 && info.ButtonID <= 401) ||
                    (info.ButtonID >= 500 && info.ButtonID <= 506))
            {
                m_From.PlaySound(CardSystemConfig.TomeNavigationSound);
            }

            // Home button
            if (info.ButtonID == 100)
            {
                m_From.SendGump(new CardTomeGump(m_From, m_Tome, CategoryPage, m_CurrentCategory));
                return;
            }

            // Category bookmark buttons (201-207)
            if (info.ButtonID >= 201 && info.ButtonID <= 207)
            {
                CardCategory newCategory = CardCategory.Boss;
                switch (info.ButtonID)
                {
                    case 201: newCategory = CardCategory.Boss; break;
                    case 202: newCategory = CardCategory.Dungeon; break;
                    case 203: newCategory = CardCategory.Legends; break;
                    case 204: newCategory = CardCategory.History; break;
                    case 205: newCategory = CardCategory.ServerEvent; break;
                    case 206: newCategory = CardCategory.Town; break;
                    case 207: newCategory = CardCategory.Misc; break;
                }

                m_From.SendGump(new CardTomeGump(m_From, m_Tome, 1, newCategory));
                return;
            }

            // Category selection buttons from home page (500-506)
            if (info.ButtonID >= 500 && info.ButtonID <= 506)
            {
                CardCategory newCategory = (CardCategory)(info.ButtonID - 500);
                m_From.SendGump(new CardTomeGump(m_From, m_Tome, 1, newCategory));
                return;
            }

            // Previous page
            if (info.ButtonID == 400)
            {
                if (m_Page > 1)
                    m_From.SendGump(new CardTomeGump(m_From, m_Tome, m_Page - 1, m_CurrentCategory));
                else
                    m_From.SendGump(new CardTomeGump(m_From, m_Tome, m_Page, m_CurrentCategory));
                return;
            }

            // Next page
            if (info.ButtonID == 401)
            {
                int totalPages = (int)Math.Ceiling(m_CurrentCards.Count / (double)m_CardsPerPage);
                if (m_Page < totalPages)
                    m_From.SendGump(new CardTomeGump(m_From, m_Tome, m_Page + 1, m_CurrentCategory));
                else
                    m_From.SendGump(new CardTomeGump(m_From, m_Tome, m_Page, m_CurrentCategory));
                return;
            }

            // Remove card button
            if (info.ButtonID >= 1000)
            {
                int cardIndex = info.ButtonID - 1000;
                if (cardIndex >= 0 && cardIndex < m_CurrentCards.Count)
                {
                    CollectableCard cardToRemove = m_CurrentCards[cardIndex];
                    if (cardToRemove != null && m_Tome.RemoveCard(cardToRemove))
                    {
                        m_From.AddToBackpack(cardToRemove);
                        m_From.SendMessage("You remove the card from your tome.");
                        m_From.PlaySound(CardSystemConfig.CardRemoveSound);

                        int totalPages = (int)Math.Ceiling(m_Tome.GetCardsInCategory(m_CurrentCategory).Count / (double)m_CardsPerPage);
                        int newPage = m_Page;
                        if (newPage > totalPages && totalPages > 0)
                            newPage = totalPages;
                        else if (totalPages == 0)
                            newPage = CategoryPage;

                        m_From.SendGump(new CardTomeGump(m_From, m_Tome, newPage, m_CurrentCategory));
                    }
                }
                return;
            }
        }
    }
}
