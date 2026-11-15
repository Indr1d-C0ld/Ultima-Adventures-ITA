using System;
using Server.Gumps;
using Server.Network;
using Server.Items;

namespace Server.Gumps
{
    public class CardTomeGuideGump : Gump
    {
        private Mobile m_From;
        private CardTome m_Tome;

        public CardTomeGuideGump(Mobile from, CardTome tome) : base(50, 50)
        {
            m_From = from;
            m_Tome = tome;

            // Play the same open sound as the card tome
            if (m_From != null && !m_From.Deleted)
                m_From.PlaySound(CardSystemConfig.TomeOpenSound);

            BuildGump();
        }

        private void BuildGump()
        {
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            // Render the wooden frame (try various offsets depending on gump size, e.g. X=33, Y=26)
            RenderWoodenBorder(this, 0, 0, 23, 15, CardSystemConfig.GuideGumpWidth, CardSystemConfig.GuideGumpHeight,
                    CardSystemConfig.WoodBorderTileID,
                    CardSystemConfig.WoodBorderShadowHue,
                    CardSystemConfig.WoodBackgroundTileID,
                    CardSystemConfig.WoodBackgroundHue);

            // Render the guide image
            AddImage(GetFrameOffsetX(), GetFrameOffsetY(), CardSystemConfig.GuideImageID);

            // Render the title
            RenderWoodenTitleHtml(this, "Collectable Card Guide", CardSystemConfig.GuideGumpWidth, 2446, 2401, "FFFFFF");

            // Add close button in the top-right corner
            int closeX = CardSystemConfig.GuideGumpWidth - 40;
            int closeY = 10;
            AddButton(closeX, closeY, 4017, 4018, 0, GumpButtonType.Reply, 0);
        }

        /// <summary>
        /// Renders a title with perfect centering using HTML rendering
        /// </summary>
        public static void RenderWoodenTitleHtml(Gump gump, string title, int gumpWidth,
                int headerGumpID = 2446, int headerHue = 2401, string colorHex = "FFFFFF")
        {
            // Header dimensions
            int headerWidth = 180;
            int headerHeight = 23;

            // Calculate header position
            int headerX = (gumpWidth - headerWidth) / 2;
            int headerY = -(headerHeight / 2) + 2;

            // Add decorative header
            gump.AddImage(headerX, headerY, headerGumpID, headerHue);

            // Calculate text position (centered within header)
            int textX = headerX;
            int textY = headerY + (headerHeight / 2) - 7;
            int textWidth = headerWidth;
            int textHeight = 25;

            string outlineColor = "000000";

            // Build HTML with centering
            string htmlText = string.Format(
                    "<BASEFONT COLOR=#{0}><CENTER>{1}</CENTER></BASEFONT>", 
                    colorHex, title);

            string shadowText = string.Format(
                    "<BASEFONT COLOR=#{0}><CENTER>{1}</CENTER></BASEFONT>", 
                    outlineColor, title);

            // Add shadow/outline layers (offset by 1 pixel)
            gump.AddHtml(textX - 1, textY - 1, textWidth, textHeight, shadowText, false, false);
            gump.AddHtml(textX + 1, textY - 1, textWidth, textHeight, shadowText, false, false);
            gump.AddHtml(textX - 1, textY + 1, textWidth, textHeight, shadowText, false, false);
            gump.AddHtml(textX + 1, textY + 1, textWidth, textHeight, shadowText, false, false);

            // Add main text
            gump.AddHtml(textX, textY, textWidth, textHeight, htmlText, false, false);
        }

        /// <summary>
        /// Renders a title with perfect centering using AddLabel
        /// </summary>
        public static void RenderWoodenTitleSimple(Gump gump, string title, int gumpWidth,
                int headerGumpID = 2446, int headerHue = 2401, int textHue = 149)
        {
            // Header dimensions
            int headerWidth = 180;
            int headerHeight = 23;

            // Calculate header position
            int headerX = (gumpWidth - headerWidth) / 2;
            int headerY = -(headerHeight / 2) + 2;

            // Add decorative header
            gump.AddImage(headerX, headerY, headerGumpID, headerHue);

            // Calculate text position (centered within header)
            // Each character is approximately 6 pixels wide
            int textWidth = title.Length * 6;
            int textX = headerX + ((headerWidth - textWidth) / 2);
            int textY = headerY + (headerHeight / 2) - 7;

            // Add text with outline
            gump.AddLabel(textX - 1, textY - 1, 0, title);
            gump.AddLabel(textX + 1, textY - 1, 0, title);
            gump.AddLabel(textX - 1, textY + 1, 0, title);
            gump.AddLabel(textX + 1, textY + 1, 0, title);
            gump.AddLabel(textX, textY, textHue, title);
        }

        /// <summary>
        /// Renders a wooden border frame with Outlands-style wooden box overlapping technique
        /// </summary>
        /// <param name="gump">The gump to render on</param>
        /// <param name="x">Top-left X coordinate</param>
        /// <param name="y">Top-left Y coordinate</param>
        /// <param name="width">Total width of the gump</param>
        /// <param name="height">Total height of the gump</param>
        /// <param name="borderTileID">Wood border tile ID (default: 103)</param>
        /// <param name="borderShadowHue">Shadow hue for corners (default: 0)</param>
        /// <param name="bgTileID">Background tile ID (default: 3604)</param>
        /// <param name="bgHue">Background hue (default: 2052)</param>
        public static void RenderWoodenBorder(Gump gump, int x, int y, int overlapX, int overlapY, int width, int height, 
                int borderTileID = 103, int borderShadowHue = 0, 
                int bgTileID = 3604, int bgHue = 2052)
        {
            int tileWidth = 143;
            int tileHeight = 101;

            int stepX = tileWidth - overlapX;
            int stepY = tileHeight - overlapY;

            // Calculate the right and bottom boundaries
            int rightBoundary = x + width - tileWidth;
            int bottomBoundary = y + height - tileHeight;

            // === TOP EDGE ===
            // Top-left corner (with shadow)
            gump.AddImage(x, y, borderTileID, borderShadowHue);

            // Top middle tiles - start with overlap from the corner
            int currentX = x + stepX;
            while (currentX <= rightBoundary)
            {
                gump.AddImage(currentX, y, borderTileID);
                currentX += stepX;
            }

            // Top-right corner (with shadow) - only if not already placed
            if (currentX - stepX != rightBoundary)
            {
                gump.AddImage(rightBoundary, y, borderTileID, borderShadowHue);
            }

            // === BOTTOM EDGE ===
            // Bottom-left corner (with shadow)
            gump.AddImage(x, bottomBoundary, borderTileID, borderShadowHue);

            // Bottom middle tiles
            currentX = x + stepX;
            while (currentX <= rightBoundary)
            {
                gump.AddImage(currentX, bottomBoundary, borderTileID);
                currentX += stepX;
            }

            // Bottom-right corner (with shadow)
            if (currentX - stepX != rightBoundary)
            {
                gump.AddImage(rightBoundary, bottomBoundary, borderTileID, borderShadowHue);
            }

            // === LEFT EDGE ===
            int currentY = y + stepY;
            while (currentY <= bottomBoundary)
            {
                gump.AddImage(x, currentY, borderTileID);
                currentY += stepY;
            }

            // === RIGHT EDGE ===
            currentY = y + stepY;
            while (currentY <= bottomBoundary)
            {
                gump.AddImage(rightBoundary, currentY, borderTileID);
                currentY += stepY;
            }

            // === DARK BACKGROUND ===
            int bgX = x + 10;
            int bgY = y + 10;
            int bgWidth = width - 20;
            int bgHeight = height - 20;

            // Solid dark background
            gump.AddImageTiled(bgX, bgY, bgWidth, bgHeight, 163);

            // Overlay wood texture
            int woodTileSize = 128;
            for (int tx = bgX; tx < bgX + bgWidth; tx += woodTileSize)
            {
                for (int ty = bgY; ty < bgY + bgHeight; ty += woodTileSize)
                {
                    int drawWidth = Math.Min(woodTileSize, bgX + bgWidth - tx);
                    int drawHeight = Math.Min(woodTileSize, bgY + bgHeight - ty);
                    if (drawWidth == woodTileSize && drawHeight == woodTileSize)
                        gump.AddImage(tx, ty, bgTileID, bgHue);
                    else
                        gump.AddImageTiled(tx, ty, drawWidth, drawHeight, bgTileID);
                }
            }
        }

        private int GetFrameOffsetX()
        {
            // Return the X offset where the image should start (inside the frame)
            return 10; // Adjust based on your frame border thickness
        }

        private int GetFrameOffsetY()
        {
            // Return the Y offset where the image should start (inside the frame)
            return 10; // Adjust based on your frame border thickness
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_From != null && !m_From.Deleted)
            {
                if (info.ButtonID == 0) // Close button
                {
                    m_From.PlaySound(CardSystemConfig.TomeCloseSound);
                }
            }
        }
    }
}
