using System;
using Server.Items;

namespace Server.Items
{
    public static class CardSystemConfig
    {
        // ==========================================
        // FEATURE TOGGLES
        // ==========================================
        // Set to false to hide achievement points from all displays
        public static bool EnablePointsSystem = true;
	
	// ==========================================
	// GUIDE GUMP CONFIGURATION
	// ==========================================
	// Guide image ID
	public static int GuideImageID = 503;

	// Guide gump dimensions
	public static int GuideGumpWidth = 993;
	public static int GuideGumpHeight = 614;

        // ==========================================
        // POINTS PER RARITY
        // ==========================================
        public static int CommonPoints = 5;
        public static int UncommonPoints = 10;
        public static int RarePoints = 25;
        public static int EpicPoints = 50;
        public static int LegendaryPoints = 100;
        public static int SpecialPoints = 250;

        // ==========================================
        // FAME TO RARITY THRESHOLDS (Min and Max)
        // ==========================================
        // Each fame tier defines both minimum and maximum rarity that can drop
        // Structure: (Fame Range) -> Min Rarity, Max Rarity

        // You can adjust these thresholds as needed
        public static int Tier1Fame = 400;      // 0-399: No drops (below threshold)
        public static int Tier2Fame = 1500;     // 400-1499: Common only
        public static int Tier3Fame = 3500;     // 1500-3499: Common to Uncommon
        public static int Tier4Fame = 8000;     // 3500-7999: Uncommon to Rare
        public static int Tier5Fame = 20000;    // 8000-19999: Rare to Epic
        public static int Tier6Fame = 100000;   // 20000-99999: Epic to Legendary
                                                // 100000+: Legendary to Special

                                                // Enable/disable fame-based filtering entirely
        public static bool EnableFameFiltering = true;

        // If true, monsters below the threshold cannot drop cards of that rarity at all
        // If false, they have a reduced chance instead of being blocked entirely
        public static bool StrictFameFiltering = true;

        // Reduced chance multiplier for monsters that don't meet fame requirements (when StrictFameFiltering = false)
        public static double BelowFameThresholdMultiplier = 0.25; // 25% of normal drop chance

        // ==========================================
        // DROP CONFIGURATION
        // ==========================================

        // Global multiplier for all card drop chances (0.0 to 10.0)
        // Use this to globally increase or decrease drop rates for testing or events
        // Examples:
        //   1.0 = normal rates (default)
        //   2.0 = double drop rates
        //   0.5 = half drop rates
        //   0.0 = no drops at all
        public static double DropRateMultiplier = 0.25;

        // Chance (0.0 to 1.0) that a general card drop is replaced with a monster-specific card
        // Set to 0 to disable monster flavor drops entirely
        public static double MonsterFlavorChance = 0.85;

        // Chance (0.0 to 1.0) that a general card drop is replaced with a region-specific card
        // Set to 0 to disable region flavor drops entirely
        public static double RegionFlavorChance = 0.50;

        // ==========================================
        // CHEST DROP CONFIGURATION
        // ==========================================
        // Maximum chance for cards from chests (0-100)
        public static double MaxChestCardChance = 25;

        // Base chance multiplier for chest cards (chest level * this value)
        public static double ChestBaseChanceMultiplier = 2;

	// Message format for mob drops
        public static string MonsterDropMessageFormat = "You notice {0} {1} collectable card on the corpse!";
	
	// Message format for named (specific) mob drops
        public static string NamedMonsterDropMessageFormat = "You notice {0} {1} collectable card from {2} on the corpse!";

        // Message format for dungeon chest drops
        public static string DungeonChestDropMessageFormat = "You notice {0} {1} collectable card in the chest!";

        // Message format for treasure map chest drops
        public static string TreasureChestDropMessageFormat = "You notice {0} {1} collectable card in the treasure chest!";

        // ==========================================
        // ITEM APPEARANCE
        // ==========================================
        // Item ID for the Collectable Card (default: 0x0E17, small card)
        public static int CardItemID = 0x4C29;

        // Item ID for the Card Tome (default: 0x0F3D, runebook)
        public static int TomeItemID = 0x0F3D;

        // Hue for the Card Tome (default: 368)
        public static int TomeHue = 368;

        // ==========================================
        // CARD VISUAL APPEARANCE
        // ==========================================
        // Card border background (composite gump that uses 9 sequential images)
        // Default: 9200 (gray stony border)
        public static int CardBorderGumpArtID = 9200;

        // Card inner background (tiled solid fill)
        // Default: 2624 (dark slate) or 163 (monotonous #101010 dark gray, almost black)
        public static int CardBackgroundGumpArtID = 163;

        // Border rendering mode ("true" for Outlands style wooden borders with optional hue)
        public static bool UseWoodenBorderStyle = true;

        // Outlands-style wooden border component
        public static int WoodBorderTileID = 103;      // 143x101 wooden box
        public static int WoodBorderShadowHue = 0;     // Shadowed variant for corners (2401 for gray)
        public static int WoodBackgroundTileID = 3604; // Interior background tile
        public static int WoodBackgroundHue = 2052;    // Dark background hue

        // ==========================================
        // SOUND CONFIGURATION
        // ==========================================
        // Sound when opening a card (default: 0x55 = book open)
        public static int CardOpenSound = 0x55;

        // Sound when closing a card (default: 0x55 = book open)
        public static int CardCloseSound = 0x55;

        // Sound when opening the tome (default: 0x55 = book open; alternatively, 0x249 might work)
        public static int TomeOpenSound = 0x55;

        // Sound when closing the tome (default: 0x55 = book open)
        public static int TomeCloseSound = 0x55;

        // Sound when turning a page or clicking a bookmark (default: 0x55 = book/page sound)
        public static int TomeNavigationSound = 0x55;

        // Sound when adding a card to the tome (default: 0x55 = book/page sound)
        public static int CardAddSound = 0x55;

        // Sound when removing a card from the tome (default: 0x42 = drop to inventory sound; 0x59 might sound alright too)
        public static int CardRemoveSound = 0x55;

        // Sound when a card drops as loot (default: 0x5B6 = quest update sound)
        public static int CardDropSound = 0x5B6;

        // ==========================================
        // CATEGORY VISIBILITY TOGGLES
        // ==========================================
        public static bool EnableBossCategory = true;
        public static bool EnableDungeonCategory = true;
        public static bool EnableLegendsCategory = true;
        public static bool EnableHistoryCategory = true;
        public static bool EnableServerEventCategory = true;
        public static bool EnableTownCategory = true;
        public static bool EnableMiscCategory = false;  // Disabled by default

        // ==========================================
        // CATEGORY DISPLAY NAMES (Customizable)
        // ==========================================
        public static string BossDisplayName = "Boss";
        public static string DungeonDisplayName = "Dungeon";
        public static string LegendsDisplayName = "Legends";
        public static string HistoryDisplayName = "History";
        public static string ServerEventDisplayName = "Server Event";
        public static string TownDisplayName = "Town";
        public static string MiscDisplayName = "Miscellaneous";

        // ==========================================
        // TITLE COLOR MODE
        // ==========================================
        // Set to true to color card titles by Category, false to color by Rarity
        public static bool TitleColorByCategory = true;

        // ==========================================
        // TOME APPEARANCE
        // ==========================================

        // ==========================================
        // CATEGORY COLORS
        // ==========================================
        public static string BossColor = "#EE5333"; // Outlands color: #EE5333
        public static string DungeonColor = "#A2988E"; // Outlands color: #958D7A or #A18A6F
        public static string LegendsColor = "#F3F1AC"; // Outlands color: #F3F1AC
        public static string HistoryColor = "#F7ADF7"; // Outlands color: #F7ADF7
        public static string ServerEventColor = "#E6BC6B"; // Outlands color: #E6BC6B
        public static string TownColor = "#CFF7AD"; // Outlands color: #CFF7AD
        public static string MiscColor = "#FFFFFF";

        // ==========================================
        // RARITY COLORS
        // ==========================================
        public static string CommonColor = "#FFFFFF";
        public static string UncommonColor = "#4C9FFF";
        public static string RareColor = "#FFD700";
        public static string EpicColor = "#9B30FF";
        public static string LegendaryColor = "#FF6600";
        public static string SpecialColor = "#66FF66";

        // ==========================================
        // DROP MESSAGE HUES
        // ==========================================
        public static int CommonMessageHue = 0x47E;     // Gray
        public static int UncommonMessageHue = 0x63;    // Blue
        public static int RareMessageHue = 0x35;        // Yellow/Gold
        public static int EpicMessageHue = 0x0A;        // Purple
        public static int LegendaryMessageHue = 0x26;   // Orange
        public static int SpecialMessageHue = 0x69;     // Bright Green
        public static int DefaultMessageHue = 0x47;     // Default (off-white)

        // Suffix for points in drops
        public static bool ShowPointsInDropMessage = false;
        public static string PointsSuffixFormat = " ({0} points)";

        // ==========================================
        // OTHER TEXT COLORS
        // ==========================================
        public static string SubtitleColor = "#E4D4AD";
        public static string DescriptionColor = "#FFD700";
        public static string StatTextColor = "#FFD700";
        public static string HeaderTextColor = "#E8C076";
        public static string CountTextColor = "#BDBDC7";

        // ==========================================
        // HELPER METHODS
        // ==========================================

        /// <summary>
        /// Checks if a category is enabled
        /// </summary>
        public static bool IsCategoryEnabled(CardCategory category)
        {
            switch (category)
            {
                case CardCategory.Boss: return EnableBossCategory;
                case CardCategory.Dungeon: return EnableDungeonCategory;
                case CardCategory.Legends: return EnableLegendsCategory;
                case CardCategory.History: return EnableHistoryCategory;
                case CardCategory.ServerEvent: return EnableServerEventCategory;
                case CardCategory.Town: return EnableTownCategory;
                case CardCategory.Misc: return EnableMiscCategory;
                default: return true;
            }
        }

        /// <summary>
        /// Gets the display name for a card category
        /// </summary>
        public static string GetCategoryDisplayName(CardCategory category)
        {
            switch (category)
            {
                case CardCategory.Boss: return BossDisplayName;
                case CardCategory.Dungeon: return DungeonDisplayName;
                case CardCategory.Legends: return LegendsDisplayName;
                case CardCategory.History: return HistoryDisplayName;
                case CardCategory.ServerEvent: return ServerEventDisplayName;
                case CardCategory.Town: return TownDisplayName;
                default: return MiscDisplayName;
            }
        }

        /// <summary>
        /// Gets the hex color for a card title (based on TitleColorByCategory setting)
        /// </summary>
        public static string GetTitleColor(CollectableCard card)
        {
            if (TitleColorByCategory)
                return GetCategoryColor(card.Category);
            else
                return GetRarityColor(card.Rarity);
        }

        /// <summary>
        /// Gets the hex color for a card category
        /// </summary>
        public static string GetCategoryColor(CardCategory category)
        {
            switch (category)
            {
                case CardCategory.Boss: return BossColor;
                case CardCategory.Dungeon: return DungeonColor;
                case CardCategory.Legends: return LegendsColor;
                case CardCategory.History: return HistoryColor;
                case CardCategory.ServerEvent: return ServerEventColor;
                case CardCategory.Town: return TownColor;
                default: return MiscColor;
            }
        }

        /// <summary>
        /// Gets the hex color for a card rarity
        /// </summary>
        public static string GetRarityColor(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Common: return CommonColor;
                case CardRarity.Uncommon: return UncommonColor;
                case CardRarity.Rare: return RareColor;
                case CardRarity.Epic: return EpicColor;
                case CardRarity.Legendary: return LegendaryColor;
                case CardRarity.Special: return SpecialColor;
                default: return CommonColor;
            }
        }

        /// <summary>
        /// Gets the point value for a card rarity
        /// </summary>
        public static int GetPointsForRarity(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Common: return CommonPoints;
                case CardRarity.Uncommon: return UncommonPoints;
                case CardRarity.Rare: return RarePoints;
                case CardRarity.Epic: return EpicPoints;
                case CardRarity.Legendary: return LegendaryPoints;
                case CardRarity.Special: return SpecialPoints;
                default: return CommonPoints;
            }
        }

        /// <summary>
        /// Gets the message hue for a card rarity
        /// </summary>
        public static int GetMessageHueForRarity(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Common: return CommonMessageHue;
                case CardRarity.Uncommon: return UncommonMessageHue;
                case CardRarity.Rare: return RareMessageHue;
                case CardRarity.Epic: return EpicMessageHue;
                case CardRarity.Legendary: return LegendaryMessageHue;
                case CardRarity.Special: return SpecialMessageHue;
                default: return DefaultMessageHue;
            }
        }

        // ==========================================
        // HELPER METHODS FOR FAME THRESHOLDS
        // ==========================================

        /// <summary>
        /// Gets the minimum and maximum card rarity allowed for a given fame level
        /// </summary>
        public static void GetRarityRangeForFame(int fame, out CardRarity minRarity, out CardRarity maxRarity)
        {
            if (!EnableFameFiltering)
            {
                minRarity = CardRarity.Common;
                maxRarity = CardRarity.Special;
                return;
            }

            // Fame 100000+ : Legendary to Special
            if (fame >= Tier6Fame)
            {
                minRarity = CardRarity.Legendary;
                maxRarity = CardRarity.Special;
            }
            // Fame 20000-99999 : Epic to Legendary
            else if (fame >= Tier5Fame)
            {
                minRarity = CardRarity.Epic;
                maxRarity = CardRarity.Legendary;
            }
            // Fame 8000-19999 : Rare to Epic
            else if (fame >= Tier4Fame)
            {
                minRarity = CardRarity.Rare;
                maxRarity = CardRarity.Epic;
            }
            // Fame 3500-7999 : Uncommon to Rare
            else if (fame >= Tier3Fame)
            {
                minRarity = CardRarity.Uncommon;
                maxRarity = CardRarity.Rare;
            }
            // Fame 1500-3499 : Common to Uncommon
            else if (fame >= Tier2Fame)
            {
                minRarity = CardRarity.Common;
                maxRarity = CardRarity.Uncommon;
            }
            // Fame 400-1499 : Common only
            else if (fame >= Tier1Fame)
            {
                minRarity = CardRarity.Common;
                maxRarity = CardRarity.Common;
            }
            // Below 400 fame: No drops
            else
            {
                minRarity = CardRarity.Special; // Special is highest, nothing will match by default (purely GM-awarded)
                maxRarity = CardRarity.Common;
            }
        }

        /// <summary>
        /// Checks if a monster with given fame can drop a card of specified rarity
        /// </summary>
        public static bool CanDropRarity(int fame, CardRarity rarity)
        {
            if (!EnableFameFiltering)
                return true;

            // Special exception: Special rarity is never dropped normally (GM only)
            if (rarity == CardRarity.Special)
                return false;

            CardRarity minRarity, maxRarity;
            GetRarityRangeForFame(fame, out minRarity, out maxRarity);

            return rarity >= minRarity && rarity <= maxRarity;
        }

        /// <summary>
        /// Gets the minimum card rarity allowed for a given fame level
        /// </summary>
        public static CardRarity GetMinimumRarityFromFame(int fame)
        {
            CardRarity minRarity, maxRarity;
            GetRarityRangeForFame(fame, out minRarity, out maxRarity);
            return minRarity;
        }

        /// <summary>
        /// Gets the maximum card rarity allowed for a given fame level
        /// </summary>
        public static CardRarity GetMaximumRarityFromFame(int fame)
        {
            CardRarity minRarity, maxRarity;
            GetRarityRangeForFame(fame, out minRarity, out maxRarity);
            return maxRarity;
        }
    }
}
