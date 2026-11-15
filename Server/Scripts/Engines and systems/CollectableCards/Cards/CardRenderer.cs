using System;
using Server.Gumps;
using Server.Items;

namespace Server.Gumps
{
    public static class CardRenderer
    {
        /// <summary>
        /// Renders a collectable card at the specified coordinates
        /// </summary>
        public static void RenderCard(Gump gump, int x, int y, string cardName, string subtitle, CardRarity rarity, CardCategory category, int imageID, int customBorderGumpID = 0)
        {
            if (CardSystemConfig.UseWoodenBorderStyle)
            {
                RenderWoodenCard(gump, x, y, cardName, subtitle, rarity, category, imageID, customBorderGumpID);
            }
            else
            {
                RenderDefaultCard(gump, x, y, cardName, subtitle, rarity, category, imageID, customBorderGumpID);
            }
        }

        /// <summary>
        /// Renders a collectable card directly from a CollectableCard object
        /// </summary>
        public static void RenderCard(Gump gump, int x, int y, CollectableCard card)
        {
            RenderCard(gump, x, y, card.CardName, card.Subtitle, card.Rarity, card.Category, card.CardImageID, card.CardBorderGumpID);
        }

        /// <summary>
        /// Renders an empty card slot
        /// </summary>
        public static void RenderEmptyCard(Gump gump, int x, int y)
        {
            if (CardSystemConfig.UseWoodenBorderStyle)
            {
                RenderWoodenCardFrame(gump, x, y);
                gump.AddHtml(x + 30, y + 140, 150, 60, 
                        "<BASEFONT COLOR=#555555><CENTER>Empty Slot</CENTER></BASEFONT>", false, false);
            }
            else
            {
                int cardWidth = 210;
                int cardHeight = 320;

                gump.AddBackground(x, y, cardWidth, cardHeight, CardSystemConfig.CardBorderGumpArtID);
                gump.AddImageTiled(x + 8, y + 8, cardWidth - 16, cardHeight - 16, CardSystemConfig.CardBackgroundGumpArtID);
                gump.AddHtml(x + 30, y + 140, 150, 60, 
                        "<BASEFONT COLOR=#555555><CENTER>Empty Slot</CENTER></BASEFONT>", false, false);
            }
        }

        /// <summary>
        /// Renders an empty card slot with a custom message
        /// </summary>
        public static void RenderEmptyCardWithMessage(Gump gump, int x, int y, string message)
        {
            if (CardSystemConfig.UseWoodenBorderStyle)
            {
                RenderWoodenCardFrame(gump, x, y);
                gump.AddHtml(x + 20, y + 120, 170, 100, 
                        string.Format("<BASEFONT COLOR=#555555><CENTER>{0}</CENTER></BASEFONT>", message), 
                        false, false);
            }
            else
            {
                int cardWidth = 210;
                int cardHeight = 320;

                gump.AddBackground(x, y, cardWidth, cardHeight, CardSystemConfig.CardBorderGumpArtID);
                gump.AddImageTiled(x + 8, y + 8, cardWidth - 16, cardHeight - 16, CardSystemConfig.CardBackgroundGumpArtID);
                gump.AddHtml(x + 20, y + 120, 170, 100, 
                        string.Format("<BASEFONT COLOR=#555555><CENTER>{0}</CENTER></BASEFONT>", message), 
                        false, false);
            }
        }

	/// <summary>
	/// Original card rendering method
	/// </summary>
	private static void RenderDefaultCard(Gump gump, int x, int y, string cardName, string subtitle, 
			CardRarity rarity, CardCategory category, int imageID, int customBorderGumpID)
	{
		int cardWidth = 210;
		int cardHeight = 320;
		int borderThickness = 8;

		int borderGumpID = customBorderGumpID > 0 ? customBorderGumpID : CardSystemConfig.CardBorderGumpArtID;
		gump.AddBackground(x, y, cardWidth, cardHeight, borderGumpID);
		gump.AddImageTiled(x + borderThickness, y + borderThickness, 
				cardWidth - (borderThickness * 2), cardHeight - (borderThickness * 2), 
				CardSystemConfig.CardBackgroundGumpArtID);

		if (imageID > 0)
		{
			// Standard card art dimensions (most card images are 200x260)
			int standardArtWidth = 200;
			int standardArtHeight = 260;

			// Calculate available space
			int availableWidth = cardWidth - (borderThickness * 2);
			int availableHeight = cardHeight - 55;  // Reserve 55px for title/subtitle

			// Use the smaller of available space vs standard dimensions to prevent tiling
			int artWidth = Math.Min(availableWidth, standardArtWidth);
			int artHeight = Math.Min(availableHeight, standardArtHeight);

			// Center the art within the available space
			int artX = x + borderThickness + ((availableWidth - artWidth) / 2);
			int artY = y + borderThickness;

			gump.AddImageTiled(artX, artY, artWidth, artHeight, imageID);
		}

		int titleY = y + cardHeight - 55;
		string titleColor = GetTitleColor(rarity, category);
		string displayTitle = string.IsNullOrEmpty(cardName) ? "Card Title" : cardName;
		if (displayTitle.Length > 30)
			displayTitle = displayTitle.Substring(0, 28) + "...";

		gump.AddHtml(x + borderThickness, titleY + 8, 
				cardWidth - (borderThickness * 2), 22, 
				string.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", titleColor, displayTitle), 
				false, false);

		string displaySubtitle = string.IsNullOrEmpty(subtitle) ? "Subtitle" : subtitle;
		if (displaySubtitle.Length > 35)
			displaySubtitle = displaySubtitle.Substring(0, 33) + "...";

		gump.AddHtml(x + borderThickness, titleY + 26,
				cardWidth - (borderThickness * 2), 20, 
				string.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", CardSystemConfig.SubtitleColor, displaySubtitle), 
				false, false);
	}

    /// <summary>
    /// Renders a card using Outlands-style tiled wooden border
    /// </summary>
    private static void RenderWoodenCard(Gump gump, int x, int y, string cardName, string subtitle, 
            CardRarity rarity, CardCategory category, int imageID, int customBorderGumpID = 0)
    {
        // Draw the frame and background
        RenderWoodenCardFrame(gump, x, y);

        int cardWidth = 210;
        int cardHeight = 320;

        // Get the dark background position
        int bgX = x + 8;
        int bgY = y + 8;
        int bgWidth = cardWidth - 16;
        int bgHeight = cardHeight - 16;

        // === CARD ART LAYER ===
        if (imageID > 0)
        {
            // Standard card art dimensions
            int standardArtWidth = 200;
            int standardArtHeight = 260;

            // Available space within the dark background (with 3px inset on sides)
            int artInset = 3;
            int availableWidth = bgWidth - (artInset * 2);
            int availableHeight = bgHeight - (artInset * 2) - 45;  // Reserve 45px for title/subtitle

            // Use the smaller of available space vs standard dimensions to prevent tiling
            int artWidth = Math.Min(availableWidth, standardArtWidth);
            int artHeight = Math.Min(availableHeight, standardArtHeight);

            // Center the art within the available space
            int artX = bgX + artInset + ((availableWidth - artWidth) / 2);
            int artY = bgY + artInset;

            gump.AddImageTiled(artX, artY, artWidth, artHeight, imageID);
        }

        // === TITLE AND SUBTITLE ===
        int titleY = y + cardHeight - 50;
        string titleColor = GetTitleColor(rarity, category);
        string displayTitle = string.IsNullOrEmpty(cardName) ? "Card Title" : cardName;
        if (displayTitle.Length > 30)
            displayTitle = displayTitle.Substring(0, 28) + "...";

        int textX = bgX + 3;
        int textWidth = bgWidth - 6;

        gump.AddHtml(textX, titleY - 1, textWidth, 22, 
                string.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", titleColor, displayTitle), 
                false, false);

        string displaySubtitle = string.IsNullOrEmpty(subtitle) ? "Subtitle" : subtitle;
        if (displaySubtitle.Length > 35)
            displaySubtitle = displaySubtitle.Substring(0, 33) + "...";

        gump.AddHtml(textX, titleY + 18, textWidth, 20, 
                string.Format("<BASEFONT COLOR={0}><CENTER>{1}</CENTER></BASEFONT>", CardSystemConfig.SubtitleColor, displaySubtitle), 
                false, false);
    }

        /// <summary>
        /// Renders just the wooden card frame and dark background (no content)
        /// </summary>
        private static void RenderWoodenCardFrame(Gump gump, int x, int y)
        {
            int cardWidth = 210;
            int cardHeight = 320;

            // === WOODEN BORDER FRAME ===

            // TOP CORNER
            gump.AddImage(x, y, CardSystemConfig.WoodBorderTileID, CardSystemConfig.WoodBorderShadowHue);
            gump.AddImage(x + cardWidth - 143, y, CardSystemConfig.WoodBorderTileID, CardSystemConfig.WoodBorderShadowHue);

            // LEFT EDGE
            gump.AddImage(x, y + 101, CardSystemConfig.WoodBorderTileID);
            gump.AddImage(x, y + 180, CardSystemConfig.WoodBorderTileID);

            // RIGHT EDGE
            gump.AddImage(x + cardWidth - 143, y + 101, CardSystemConfig.WoodBorderTileID);
            gump.AddImage(x + cardWidth - 143, y + 180, CardSystemConfig.WoodBorderTileID);

            // BOTTOM CORNER
            gump.AddImage(x, y + cardHeight - 101, CardSystemConfig.WoodBorderTileID, CardSystemConfig.WoodBorderShadowHue);
            gump.AddImage(x + cardWidth - 143, y + cardHeight - 101, CardSystemConfig.WoodBorderTileID, CardSystemConfig.WoodBorderShadowHue);

            // === DARK BACKGROUND ===
            int bgX = x + 8;
            int bgY = y + 8;
            int bgWidth = cardWidth - 16;
            int bgHeight = cardHeight - 16;

            int tileSize = 128;
            int tileHue = CardSystemConfig.WoodBackgroundHue;
            int tileID = CardSystemConfig.WoodBackgroundTileID;

            // Cover the dark background area
            gump.AddImage(bgX, bgY, tileID, tileHue);
            gump.AddImage(bgX + (bgWidth - tileSize), bgY, tileID, tileHue);
            gump.AddImage(bgX, bgY + 128, tileID, tileHue);
            gump.AddImage(bgX + (bgWidth - tileSize), bgY + 128, tileID, tileHue);
            gump.AddImage(bgX, bgY + (bgHeight - tileSize), tileID, tileHue);
            gump.AddImage(bgX + (bgWidth - tileSize), bgY + (bgHeight - tileSize), tileID, tileHue);
        }

        /// <summary>
        /// Renders an empty card using wooden style
        /// </summary>
        private static void RenderWoodenEmptyCard(Gump gump, int x, int y)
        {
            int cardWidth = 210;
            int cardHeight = 320;

            // === WOODEN BORDER FRAME ===

            // TOP CORNER
            gump.AddImage(x, y, CardSystemConfig.WoodBorderTileID, CardSystemConfig.WoodBorderShadowHue);
            gump.AddImage(x + cardWidth - 143, y, CardSystemConfig.WoodBorderTileID, CardSystemConfig.WoodBorderShadowHue);

            // LEFT EDGE
            gump.AddImage(x, y + 101, CardSystemConfig.WoodBorderTileID);
            gump.AddImage(x, y + 180, CardSystemConfig.WoodBorderTileID);

            // RIGHT EDGE
            gump.AddImage(x + cardWidth - 143, y + 101, CardSystemConfig.WoodBorderTileID);
            gump.AddImage(x + cardWidth - 143, y + 180, CardSystemConfig.WoodBorderTileID);

	    // BOTTOM CORNER
            gump.AddImage(x, y + cardHeight - 101, CardSystemConfig.WoodBorderTileID, CardSystemConfig.WoodBorderShadowHue);
            gump.AddImage(x + cardWidth - 143, y + cardHeight - 101, CardSystemConfig.WoodBorderTileID, CardSystemConfig.WoodBorderShadowHue);

            // === DARK BACKGROUND ===
            int darkBgX = x + 8;
            int darkBgY = y + 8;
            int darkBgWidth = cardWidth - 16;
            int darkBgHeight = cardHeight - 16;

            int tileSize = 128;
            int tileHue = CardSystemConfig.WoodBackgroundHue;
            int tileID = CardSystemConfig.WoodBackgroundTileID;

            // Cover the dark background area
            gump.AddImage(darkBgX, darkBgY, tileID, tileHue);
            gump.AddImage(darkBgX + (darkBgWidth - tileSize), darkBgY, tileID, tileHue);
            gump.AddImage(darkBgX, darkBgY + 128, tileID, tileHue);
            gump.AddImage(darkBgX + (darkBgWidth - tileSize), darkBgY + 128, tileID, tileHue);
            gump.AddImage(darkBgX, darkBgY + (darkBgHeight - tileSize), tileID, tileHue);
            gump.AddImage(darkBgX + (darkBgWidth - tileSize), darkBgY + (darkBgHeight - tileSize), tileID, tileHue);

            // === EMPTY SLOT MESSAGE ===
            gump.AddHtml(x + 30, y + 140, 150, 60, 
                    "<BASEFONT COLOR=#555555><CENTER>Empty Slot</CENTER></BASEFONT>", false, false);
        }

        /// <summary>
        /// Gets the title color based on CardSystemConfig.TitleColorByCategory setting
        /// </summary>
        private static string GetTitleColor(CardRarity rarity, CardCategory category)
        {
            if (CardSystemConfig.TitleColorByCategory)
                return CardSystemConfig.GetCategoryColor(category);
            else
                return CardSystemConfig.GetRarityColor(rarity);
        }
    }
}
