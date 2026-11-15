using System;

namespace Server.Items
{
    public enum CardCategory
    {
        Boss,           // Boss cards
        Dungeon,        // Dungeon creature cards  
        Legends,        // Legendary figures
        History,        // Historical events/figures
        ServerEvent,    // Event-specific cards
        Town,           // Town/Location cards
        Misc            // Miscellaneous cards
    }

    public static class CardCategoryInfo
    {
        public static string GetName(CardCategory category)
        {
            return CardSystemConfig.GetCategoryDisplayName(category);
        }

        public static int GetHue(CardCategory category)
        {
            switch (category)
            {
                case CardCategory.Boss: return 133;        // Orange-Red
                case CardCategory.Dungeon: return 449;     // Light Brown/Tan
                case CardCategory.Legends: return 56;      // Light Yellow/Gold
                case CardCategory.History: return 26;      // Light Pink/Purple
                case CardCategory.ServerEvent: return 48;  // Light Gold
                case CardCategory.Town: return 264;        // Light Lime Green
                default: return 0;                         // White
            }
        }
    }
}
