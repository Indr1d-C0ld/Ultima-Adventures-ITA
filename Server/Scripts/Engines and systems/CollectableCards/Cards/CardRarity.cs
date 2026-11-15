using System;

namespace Server.Items
{
    public enum CardRarity
    {
        Common,     // White
        Uncommon,   // Blue
        Rare,       // Yellow/Gold
        Epic,       // Purple
        Legendary,  // Orange
        Special     // Light Green (GM-awarded only)
    }

    public static class CardRarityInfo
    {
        public static int GetPoints(CardRarity rarity)
        {
            return CardSystemConfig.GetPointsForRarity(rarity);
        }

        public static string GetName(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Common: return "Common";
                case CardRarity.Uncommon: return "Uncommon";
                case CardRarity.Rare: return "Rare";
                case CardRarity.Epic: return "Epic";
                case CardRarity.Legendary: return "Legendary";
                case CardRarity.Special: return "Special";
                default: return "Common";
            }
        }
    }
}
