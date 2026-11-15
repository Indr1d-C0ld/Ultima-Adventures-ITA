using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Regions;

namespace Server
{
    public static class CardDropHelper
    {
        public static List<CardDropDefinition> CardDrops = new List<CardDropDefinition>();

        // Cache for faster lookups
        private static Dictionary<string, CardDropDefinition> _preferredCardCache = new Dictionary<string, CardDropDefinition>(StringComparer.OrdinalIgnoreCase);

        public struct RarityRange
        {
            public CardRarity Min;
            public CardRarity Max;

            public RarityRange(CardRarity min, CardRarity max)
            {
                Min = min;
                Max = max;
            }
        }

        public static void Initialize()
        {
            // ==========================================
            // BOSS CARDS
            // ==========================================

            RegisterCard(typeof(CollectableCardAncientWyrm), new CardDropDefinition
                    {
                    DropChance = 0.01,
                    SpecificMonster = "AncientWyrm",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    });

            RegisterCard(typeof(CollectableCardDarkFather), new CardDropDefinition
                    {
                    DropChance = 0.01,
                    SpecificMonster = "DemonKnight",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15
                    });

            RegisterCard(typeof(CollectableCardDracula), new CardDropDefinition
                    {
                    DropChance = 0.01,
                    Rarity = CardRarity.Legendary,
                    Category = CardCategory.Boss,
                    SpecificMonster = "Dracula",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15
                    });

            RegisterCard(typeof(CollectableCardExodus), new CardDropDefinition
                    {
                    DropChance = 0.01,
                    SpecificMonster = "Exodus",
                    Rarity = CardRarity.Legendary,
                    Category = CardCategory.Boss,
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    MinimumDropRarity = CardRarity.Legendary
                    });

            RegisterCard(typeof(CollectableCardTitanHydros), new CardDropDefinition
                    {
                    DropChance = 1.0,
                    SpecificMonster = "TitanHydros",
                    Rarity = CardRarity.Legendary,
                    Category = CardCategory.Boss,
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 1.0,
                    MinimumDropRarity = CardRarity.Legendary,
		    IgnoreDropRateMultiplier = true
                    });

            RegisterCard(typeof(CollectableCardTitanLithos), new CardDropDefinition
                    {
                    DropChance = 1.0,
                    SpecificMonster = "TitanLithos",
                    Rarity = CardRarity.Legendary,
                    Category = CardCategory.Boss,
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 1.0,
                    MinimumDropRarity = CardRarity.Legendary,
		    IgnoreDropRateMultiplier = true
                    });

            RegisterCard(typeof(CollectableCardTitanPyros), new CardDropDefinition
                    {
                    DropChance = 1.0,
                    SpecificMonster = "TitanPyros",
                    Rarity = CardRarity.Legendary,
                    Category = CardCategory.Boss,
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 1.0,
                    MinimumDropRarity = CardRarity.Legendary,
		    IgnoreDropRateMultiplier = true
                    });

            RegisterCard(typeof(CollectableCardTitanStratos), new CardDropDefinition
                    {
                    DropChance = 1.0,
                    SpecificMonster = "TitanStratos",
                    Rarity = CardRarity.Legendary,
                    Category = CardCategory.Boss,
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 1.0,
                    MinimumDropRarity = CardRarity.Legendary,
		    IgnoreDropRateMultiplier = true
                    });

            RegisterCard(typeof(CollectableCardWidow), new CardDropDefinition
                    {
                    DropChance = 0.01,
                    SpecificMonster = "Widow",
                    Rarity = CardRarity.Legendary,
                    Category = CardCategory.Boss,
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    MinimumDropRarity = CardRarity.Legendary
                    });

            RegisterCard(typeof(CollectableCardSlasherOfVoid), new CardDropDefinition
                    {
                    DropChance = 0.01,
                    SpecificMonster = "SlasherOfVoid",
                    Rarity = CardRarity.Legendary,
                    Category = CardCategory.Boss,
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    MinimumDropRarity = CardRarity.Legendary
                    });

            RegisterCard(typeof(CollectableCardMangarTheDark), new CardDropDefinition
                    {
                    DropChance = 1.0,
                    SpecificMonster = "Mangar",
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Boss,
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 1.0,
                    IgnoreDropRateMultiplier = true
                    });

            RegisterCard(typeof(CollectableCardJormungandr), new CardDropDefinition
                    {
                    DropChance = 0.01,
                    SpecificMonster = "Jormungandr",
                    Rarity = CardRarity.Legendary,
                    Category = CardCategory.Boss,
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    MinimumDropRarity = CardRarity.Legendary
                    });

            RegisterCard(typeof(CollectableCardSurtaz), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    SpecificMonster = "Surtaz",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    });

            RegisterCard(typeof(CollectableCardVulcrum), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    SpecificMonster = "Vulcrum",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    });

            RegisterCard(typeof(CollectableCardArachnar), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    SpecificMonster = "Arachnar",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    });

            RegisterCard(typeof(CollectableCardKhumashGor), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    SpecificMonster = "KhumashGor",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    });

            RegisterCard(typeof(CollectableCardXurtzar), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    SpecificMonster = "Xurtzar",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    });

            RegisterCard(typeof(CollectableCardShadowWyrm), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    SpecificMonster = "ShadowWyrm",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    });

            RegisterCard(typeof(CollectableCardLichKing), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    SpecificMonster = "LichKing",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    });

            RegisterCard(typeof(CollectableCardStygianDragon), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    SpecificMonster = "StygianDragon",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    });

            RegisterCard(typeof(CollectableCardThoroar), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    SpecificMonster = "Thoroar",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    });

            RegisterCard(typeof(CollectableCardAncientLich), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    SpecificMonster = "AncientLich",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    });

            RegisterCard(typeof(CollectableCardAncientWyvern), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    SpecificMonster = "AncientWyvern",
                    IsPreferredForMonster = true,
                    PreferredDropBonus = 0.15,
                    });

            // ==========================================
            // LEGEND CARDS
            // ==========================================

            RegisterCard(typeof(CollectableCardSoulbound), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardLordBritish), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    });

            RegisterCard(typeof(CollectableCardPeterGrimm), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    });

            RegisterCard(typeof(CollectableCardAmbrosia), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Legends,
                    AllowedMaps = new List<string> { "Trammel" }
                    });

            RegisterCard(typeof(CollectableCardEtherealVoid), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Legends,
                    AllowedMaps = new List<string> { "Malas" },
                    AllowedRegions = new List<string> { "the Ethereal Void" }
                    });

            RegisterCard(typeof(CollectableCardForgeOfFire), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Legends,
                    AllowedMaps = new List<string> { "Malas" },
                    });

            RegisterCard(typeof(CollectableCardSerpentPillars), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Legends,
                    AllowedMaps = new List<string> { "Felucca" },
                    });

            RegisterCard(typeof(CollectableCardLadyIllundai), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardBeejeemon), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardSilkySnow), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardAgetian), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardFinalTwist), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardGarthavan), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardSygun), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardTsai), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardPacem), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardDeides), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardDow), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardPug), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardBram), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardVexain), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    AllowedMaps = new List<string> { "Malas", "Ilshenar", "TerMur" },
                    });

            RegisterCard(typeof(CollectableCardTanaka), new CardDropDefinition
                    {
                    DropChance = 0.02
                    });

            RegisterCard(typeof(CollectableCardCodexOfWisdom), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    AllowedRegions = new List<string> { "the Stygian Abyss" }
                    });

            RegisterCard(typeof(CollectableCardXardoksCastle), new CardDropDefinition
                    {
                    DropChance = 0.01,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardCrashSite), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardSythHolocron), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    AllowedRegions = new List<string> { "the Fires of Hell" },
                    });

            RegisterCard(typeof(CollectableCardCoffee), new CardDropDefinition
                    {
                    DropChance = 0.02
                    });

            RegisterCard(typeof(CollectableCardMomonos), new CardDropDefinition
                    {
                    DropChance = 0.02
                    });

            RegisterCard(typeof(CollectableCardAsarco), new CardDropDefinition
                    {
                    DropChance = 0.02
                    });

            RegisterCard(typeof(CollectableCardNox), new CardDropDefinition
                    {
                    DropChance = 0.02
                    });

            RegisterCard(typeof(CollectableCardWarpGate), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    AllowedRegions = new List<string> { "the Ancient Pyramid" }
                    });

            RegisterCard(typeof(CollectableCardMoonsCore), new CardDropDefinition
                    {
                    DropChance = 0.02
                    });

            RegisterCard(typeof(CollectableCardSeaGiantsTreasure), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    AllowedRegions = new List<string> { "the Caverns of Poseidon" }
                    });

            RegisterCard(typeof(CollectableCardSacrifice), new CardDropDefinition
                    {
                    DropChance = 0.04
                    });

            // ==========================================
            // HISTORY CARDS
            // ==========================================

            RegisterCard(typeof(CollectableCardFoundingFathers), new CardDropDefinition
                    {
                    DropChance = 0.005,
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardChroniclesOfSkara), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardGuardiansChamber), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Felucca", "Malas" }
                    });

            RegisterCard(typeof(CollectableCardHallOfLegends), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.History,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardVirtualTrade), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardFinalTwistsHouse), new CardDropDefinition
                    {
                    DropChance = 0.01,
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardSigilOfTheBalance), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardPlayerHousing), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardRoadConstruction), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardHeltsDirtyDishes), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardPublicTransport), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    IsPreferredForMonster = false,
                    });

            // ==========================================
            // DUNGEON CARDS
            // ==========================================

            RegisterCard(typeof(CollectableCardMorinia), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    AllowedRegions = new List<string> { "the Mines of Morinia" },
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardFiresOfHell), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    AllowedRegions = new List<string> { "the Fires of Hell" },
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardColdhall), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    AllowedRegions = new List<string> { "Coldhall Depths" },
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardAncientPyramid), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    AllowedRegions = new List<string> { "the Ancient Pyramid" },
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardAccursedMaze), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    Rarity = CardRarity.Common,
                    Category = CardCategory.Dungeon,
                    FameRequirement = 0,
                    AllowedMaps = new List<string> { "Trammel" },
                    AllowedRegions = new List<string> { "the Accursed Maze" }
                    });

            RegisterCard(typeof(CollectableCardChamberOfBane), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    Rarity = CardRarity.Common,
                    Category = CardCategory.Dungeon,
                    FameRequirement = 0,
                    AllowedMaps = new List<string> { "Trammel" },
                    AllowedRegions = new List<string> { "the Chamber of Bane" }
                    });

            RegisterCard(typeof(CollectableCardClues), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    Rarity = CardRarity.Uncommon,
                    Category = CardCategory.Dungeon,
                    FameRequirement = 0,
                    AllowedMaps = new List<string> { "Trammel" },
                    AllowedRegions = new List<string> { "Dungeon Clues" }
                    });

            RegisterCard(typeof(CollectableCardDarkSanctum), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    Rarity = CardRarity.Common,
                    Category = CardCategory.Dungeon,
                    FameRequirement = 0,
                    AllowedMaps = new List<string> { "Trammel" },
                    AllowedRegions = new List<string> { "the Dark Sanctum" }
                    });

            RegisterCard(typeof(CollectableCardForgottenTombs), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    Rarity = CardRarity.Common,
                    Category = CardCategory.Dungeon,
                    FameRequirement = 0,
                    AllowedMaps = new List<string> { "Trammel" },
                    AllowedRegions = new List<string> { "the Forgotten Tombs" }
                    });

            RegisterCard(typeof(CollectableCardMagmaVaults), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    Rarity = CardRarity.Common,
                    Category = CardCategory.Dungeon,
                    FameRequirement = 0,
                    AllowedMaps = new List<string> { "Trammel" },
                    AllowedRegions = new List<string> { "the Magma Vaults" }
                    });

            RegisterCard(typeof(CollectableCardShroudedGrave), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    Rarity = CardRarity.Common,
                    Category = CardCategory.Dungeon,
                    FameRequirement = 0,
                    AllowedMaps = new List<string> { "Trammel" },
                    AllowedRegions = new List<string> { "the Shrouded Grave" }
                    });

            RegisterCard(typeof(CollectableCardTimeAwaits), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Trammel" },
                    FameRequirement = 0,
                    AllowedRegions = new List<string> { "the Dungeon of Time Awaits" }
                    });

            RegisterCard(typeof(CollectableCardAbandon), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    Rarity = CardRarity.Uncommon,
                    Category = CardCategory.Dungeon,
                    AllowedRegions = new List<string> { "Dungeon Abandon" }
                    });

            RegisterCard(typeof(CollectableCardCavernsOfPoseidon), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Trammel" },
                    AllowedRegions = new List<string> { "the Caverns of Poseidon" }
                    });

            RegisterCard(typeof(CollectableCardDragonsMaw), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Trammel" },
                    AllowedRegions = new List<string> { "the Dragon's Maw" }
                    });

            RegisterCard(typeof(CollectableCardZealanTombs), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Felucca" },
                    AllowedRegions = new List<string> { "the Zealan Tombs" }
                    });

            RegisterCard(typeof(CollectableCardCovetous), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Felucca" },
                    AllowedRegions = new List<string> { "Dungeon Covetous" }
                    });

            RegisterCard(typeof(CollectableCardDeceit), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Felucca" },
                    AllowedRegions = new List<string> { "Dungeon Deceit" }
                    });

            RegisterCard(typeof(CollectableCardDespise), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Felucca" },
                    AllowedRegions = new List<string> { "Dungeon Despise" }
                    });

            RegisterCard(typeof(CollectableCardDoom), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Ilshenar" },
                    AllowedRegions = new List<string> { "Doom" }
                    });

            RegisterCard(typeof(CollectableCardFrozenHells), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Felucca" },
                    AllowedRegions = new List<string> { "the Frozen Hells" }
                    });

            RegisterCard(typeof(CollectableCardHythloth), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Felucca" },
                    AllowedRegions = new List<string> { "Dungeon Hythloth" }
                    });

            RegisterCard(typeof(CollectableCardShame), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Felucca" },
                    AllowedRegions = new List<string> { "Dungeon Shame" }
                    });

            RegisterCard(typeof(CollectableCardStonegateCastle), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Felucca" },
                    });

            RegisterCard(typeof(CollectableCardUndermountain), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Felucca" },
                    AllowedRegions = new List<string> { "the Halls of Undermountain" }
                    });

            RegisterCard(typeof(CollectableCardVolcanicCaves), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Felucca" },
                    AllowedRegions = new List<string> { "the Volcanic Cave" }
                    });

            RegisterCard(typeof(CollectableCardWrong), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Felucca" },
                    AllowedRegions = new List<string> { "Dungeon Wrong" }
                    });

            RegisterCard(typeof(CollectableCardAnkh), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Malas" },
                    AllowedRegions = new List<string> { "Dungeon Ankh" }
                    });

            RegisterCard(typeof(CollectableCardBane), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Malas" },
                    AllowedRegions = new List<string> { "Dungeon Bane" }
                    });

            RegisterCard(typeof(CollectableCardFloodedTemple), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Malas" },
                    AllowedRegions = new List<string> { "the Flooded Temple" }
                    });

            RegisterCard(typeof(CollectableCardHate), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Malas" },
                    AllowedRegions = new List<string> { "Dungeon Hate" }
                    });

            RegisterCard(typeof(CollectableCardScorn), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Malas" },
                    AllowedRegions = new List<string> { "Dungeon Scorn" }
                    });

            RegisterCard(typeof(CollectableCardSerpentSanctum), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Malas" },
                    AllowedRegions = new List<string> { "the Serpent Sanctum" }
                    });

            RegisterCard(typeof(CollectableCardStygianAbyss), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Malas" },
                    AllowedRegions = new List<string> { "the Stygian Abyss" }
                    });

            RegisterCard(typeof(CollectableCardTorment), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Malas" },
                    AllowedRegions = new List<string> { "Dungeon Torment" }
                    });

            RegisterCard(typeof(CollectableCardVile), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Malas" },
                    AllowedRegions = new List<string> { "Dungeon Vile" }
                    });

            RegisterCard(typeof(CollectableCardWicked), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Malas" },
                    AllowedRegions = new List<string> { "Dungeon Wicked" }
                    });

            RegisterCard(typeof(CollectableCardWrath), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Malas" },
                    AllowedRegions = new List<string> { "Dungeon Wrath" }
                    });

            RegisterCard(typeof(CollectableCardWidowsKeep), new CardDropDefinition
                    {
                    DropChance = 0.012,
                    Rarity = CardRarity.Legendary,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Trammel" },
                    AllowedRegions = new List<string> { "the Widow's Keep" }
                    });

            RegisterCard(typeof(CollectableCardUnderworld), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Legendary,
                    Category = CardCategory.Dungeon,
                    AllowedMaps = new List<string> { "Felucca" }
                    });

            // ==========================================
            // TOWN CARDS
            // ==========================================

            RegisterCard(typeof(CollectableCardSarthAbbey), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    AllowedMaps = new List<string> { "Trammel" },
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardBritain), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    AllowedMaps = new List<string> { "Trammel" },
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardPublicBank), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardTavern), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardCasino), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardDawn), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardDeathGulch), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardDevilGuard), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardFawn), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardGlacialCoast), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardGray), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardLamutCountry), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardMontor), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardMountainCrest), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardUmbra), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardYew), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    IsPreferredForMonster = false,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardGlacialHills), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    Rarity = CardRarity.Uncommon,
                    Category = CardCategory.Town,
                    AllowedMaps = new List<string> { "Felucca" },
                    });

            RegisterCard(typeof(CollectableCardKrakenReefDocks), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Town,
                    AllowedMaps = new List<string> { "Felucca" },
                    });

            RegisterCard(typeof(CollectableCardLodoria), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    Rarity = CardRarity.Uncommon,
                    Category = CardCategory.Town,
                    AllowedMaps = new List<string> { "Felucca" },
                    });

            RegisterCard(typeof(CollectableCardRavendark), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Town,
                    AllowedMaps = new List<string> { "Felucca" },
                    });

            RegisterCard(typeof(CollectableCardSpringvale), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    Rarity = CardRarity.Uncommon,
                    Category = CardCategory.Town,
                    AllowedMaps = new List<string> { "Felucca" },
                    });

            RegisterCard(typeof(CollectableCardWhisper), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    Rarity = CardRarity.Uncommon,
                    Category = CardCategory.Town,
                    AllowedMaps = new List<string> { "Felucca" },
                    });


            RegisterCard(typeof(CollectableCardElidor), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    Rarity = CardRarity.Uncommon,
                    Category = CardCategory.Town,
                    AllowedMaps = new List<string> { "Felucca" },
                    });

            RegisterCard(typeof(CollectableCardIslegem), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    Rarity = CardRarity.Uncommon,
                    Category = CardCategory.Town,
                    AllowedMaps = new List<string> { "Felucca" },
                    });

            RegisterCard(typeof(CollectableCardPortshine), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    Rarity = CardRarity.Uncommon,
                    Category = CardCategory.Town,
                    AllowedMaps = new List<string> { "Felucca" },
                    });

            RegisterCard(typeof(CollectableCardStarguide), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    Rarity = CardRarity.Uncommon,
                    Category = CardCategory.Town,
                    AllowedMaps = new List<string> { "Felucca" },
                    });

            RegisterCard(typeof(CollectableCardAnchorRockDocks), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Town,
                    AllowedMaps = new List<string> { "Felucca" },
                    });

            RegisterCard(typeof(CollectableCardLostGlade), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Town
                    });

            RegisterCard(typeof(CollectableCardWizardsGuild), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Town
                    });

            RegisterCard(typeof(CollectableCardThievesGuild), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.Town
                    });

            RegisterCard(typeof(CollectableCardMangarsTower), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    AllowedRegions = new List<string> { "Mangar's Tower" },
                    });

            RegisterCard(typeof(CollectableCardSarthMarket), new CardDropDefinition
                    {
                    DropChance = 0.05,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardThePit), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardBasement), new CardDropDefinition
                    {
                    DropChance = 0.02,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardBritainMarket), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardWestGateCity), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            // ==========================================
            // SERVER EVENT CARDS
            // ==========================================

            RegisterCard(typeof(CollectableCard6thAnniversary), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardShardLaunch), new CardDropDefinition
                    {
                    DropChance = 0.01,
                    IsPreferredForMonster = false,
                    });

            RegisterCard(typeof(CollectableCardGauntlet), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.ServerEvent,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardLichTower), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.ServerEvent,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardChampionSpawn), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.ServerEvent,
                    AllowedMaps = new List<string> { "Trammel" },
                    });

            RegisterCard(typeof(CollectableCardHalloweenAdventures), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    Rarity = CardRarity.Uncommon,
                    Category = CardCategory.ServerEvent,
                    });

            RegisterCard(typeof(CollectableCardChristmasAdventures), new CardDropDefinition
                    {
                    DropChance = 0.04,
                    Rarity = CardRarity.Uncommon,
                    Category = CardCategory.ServerEvent,
                    });

            RegisterCard(typeof(CollectableCardZombieInvasion), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.ServerEvent,
                    });

            RegisterCard(typeof(CollectableCardMassacreBonyHand), new CardDropDefinition
                    {
                    DropChance = 0.015,
                    Rarity = CardRarity.Epic,
                    Category = CardCategory.ServerEvent,
                    });

            RegisterCard(typeof(CollectableCardHighSeasPiracy), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.ServerEvent,
                    });

            RegisterCard(typeof(CollectableCardTreasureHunting), new CardDropDefinition
                    {
                    DropChance = 0.03,
                    Rarity = CardRarity.Rare,
                    Category = CardCategory.ServerEvent,
                    });
        }

        /// <summary>
        /// Register a card type with drop definition
        /// </summary>
        public static void RegisterCard(Type cardType, CardDropDefinition definition)
        {
            if (!cardType.IsSubclassOf(typeof(CollectableCard)))
            {
                Console.WriteLine("Error: {0} is not a CollectableCard type", cardType.Name);
                return;
            }

            definition.CardType = cardType;

            // Try to extract default properties from the card class
            try
            {
                // Create a temporary instance to read properties
                CollectableCard tempCard = (CollectableCard)Activator.CreateInstance(cardType);
                if (definition.Rarity == CardRarity.Common) // Only set if not manually overridden
                    definition.Rarity = tempCard.Rarity;
                if (string.IsNullOrEmpty(definition.CardName))
                    definition.CardName = tempCard.CardName;
                if (string.IsNullOrEmpty(definition.Subtitle))
                    definition.Subtitle = tempCard.Subtitle;
                if (definition.Category == CardCategory.Misc && tempCard.Category != CardCategory.Misc)
                    definition.Category = tempCard.Category;
                if (definition.ImageID == 0)
                    definition.ImageID = tempCard.CardImageID;
                if (string.IsNullOrEmpty(definition.Description))
                    definition.Description = tempCard.Description;
            }
            catch
            {
                // If we can't create an instance, just use what's provided
                Console.WriteLine("Warning: Could not extract properties from {0}", cardType.Name);
            }

            CardDrops.Add(definition);

            // Clear cache when new cards are added
            _preferredCardCache.Clear();
        }

        /// <summary>
        /// Register using an existing card instance
        /// </summary>
        public static void RegisterCard(CollectableCard card, CardDropDefinition definition)
        {
            definition.CardType = card.GetType();
            definition.CardName = card.CardName;
            definition.Subtitle = card.Subtitle;
            definition.Rarity = card.Rarity;
            definition.Category = card.Category;
            definition.ImageID = card.CardImageID;
            definition.Description = card.Description;

            CardDrops.Add(definition);
            _preferredCardCache.Clear();
        }

        /// <summary>
        /// Gets the preferred card for a specific monster type
        /// </summary>
        public static CardDropDefinition GetPreferredCardForMonster(string monsterType)
        {
            if (string.IsNullOrEmpty(monsterType))
                return null;

            CardDropDefinition preferred;
            if (_preferredCardCache.TryGetValue(monsterType, out preferred))
                return preferred;

            // Build cache on first lookup
            foreach (CardDropDefinition def in CardDrops)
            {
                if (def.IsPreferredForMonster && 
                        !string.IsNullOrEmpty(def.SpecificMonster) && 
                        def.SpecificMonster.Equals(monsterType, StringComparison.OrdinalIgnoreCase))
                {
                    _preferredCardCache[monsterType] = def;
                    return def;
                }
            }

            _preferredCardCache[monsterType] = null;
            return null;
        }

        public static CollectableCard CreateCardFromDefinition(CardDropDefinition def)
        {
            if (def.CardType != null)
            {
                try
                {
                    CollectableCard card = (CollectableCard)Activator.CreateInstance(def.CardType);
                    if (def.CustomBorderGumpID > 0)
                        card.CardBorderGumpID = def.CustomBorderGumpID;
                    return card;
                }
                catch
                {
                    return new CollectableCard(
                            def.CardName ?? "Unknown Card",
                            def.Subtitle ?? "",
                            def.Rarity,
                            def.Category,
                            def.ImageID,
                            def.Description ?? ""
                            );
                }
            }
            else
            {
                return new CollectableCard(
                        def.CardName ?? "Unknown Card",
                        def.Subtitle ?? "",
                        def.Rarity,
                        def.Category,
                        def.ImageID,
                        def.Description ?? ""
                        );
            }
        }

        /// <summary>
        /// Converts fame to minimum card rarity based on configured thresholds
        /// </summary>
        private static CardRarity GetMinimumRarityFromFame(int fame)
        {
            // Thresholds:
            // 400     -> Common
            // 1500    -> Uncommon  
            // 3500    -> Rare
            // 8000    -> Epic
            // 20000   -> Legendary

            if (fame >= 20000)
                return CardRarity.Legendary;
            else if (fame >= 8000)
                return CardRarity.Epic;
            else if (fame >= 3500)
                return CardRarity.Rare;
            else if (fame >= 1500)
                return CardRarity.Uncommon;
            else if (fame >= 400)
                return CardRarity.Common;
            else
                return CardRarity.Common; // Below 400 fame, default to Common
        }

        /// <summary>
        /// Gets the minimum drop rarity for a monster, respecting custom override if specified
        /// </summary>
        private static CardRarity GetMinimumDropRarity(CardDropDefinition monsterDef, int fame)
        {
            // If the monster definition has a custom MinimumDropRarity set, use that
            if (monsterDef != null && monsterDef.MinimumDropRarity.HasValue)
                return monsterDef.MinimumDropRarity.Value;

            // Otherwise, calculate based on fame
            return GetMinimumRarityFromFame(fame);
        }

        public static CollectableCard GetRandomCardDrop(int minFame = 0, string monsterType = null,
                Region currentRegion = null, Map currentMap = null, CardRarity? forcedMinRarity = null, CardRarity? forcedMaxRarity = null)
        {
            // 1. Check for preferred card for this specific monster
            CardDropDefinition preferredDef = GetPreferredCardForMonster(monsterType);

            if (preferredDef != null)
            {
                if (minFame >= preferredDef.FameRequirement &&
                        (preferredDef.AllowedRegions == null || MatchesRegion(currentRegion, preferredDef.AllowedRegions)) &&
                        (preferredDef.AllowedMaps == null || MatchesMap(currentMap, preferredDef.AllowedMaps)))
                {
                    double preferredChance = preferredDef.DropChance + preferredDef.PreferredDropBonus;
                    //preferredChance = Math.Min(preferredChance, 0.50);

                    // Check if this preferred card is allowed by fame filtering
                    if (CardSystemConfig.CanDropRarity(minFame, preferredDef.Rarity))
                    {
                        if (Utility.RandomDouble() < preferredChance)
                        {
                            return CreateCardFromDefinition(preferredDef);
                        }
                    }
                }
            }

            // 2. Check all other eligible cards
            List<CardDropDefinition> eligible = new List<CardDropDefinition>();
            List<double> adjustedChances = new List<double>();

            // Determine allowed rarity range
            CardRarity minAllowedRarity, maxAllowedRarity;
            if (forcedMinRarity.HasValue && forcedMaxRarity.HasValue)
            {
                minAllowedRarity = forcedMinRarity.Value;
                maxAllowedRarity = forcedMaxRarity.Value;
            }
            else if (preferredDef != null && preferredDef.MinimumDropRarity.HasValue && preferredDef.MaximumDropRarity.HasValue)
            {
                minAllowedRarity = preferredDef.MinimumDropRarity.Value;
                maxAllowedRarity = preferredDef.MaximumDropRarity.Value;
            }
            else
            {
                CardSystemConfig.GetRarityRangeForFame(minFame, out minAllowedRarity, out maxAllowedRarity);
            }

            foreach (CardDropDefinition def in CardDrops)
            {
                // Skip the preferred card since we already tried it
                if (preferredDef != null && def == preferredDef)
                    continue;

                // Skip cards that require a specific monster (unless we're that monster)
                if (!string.IsNullOrEmpty(def.SpecificMonster) && def.SpecificMonster != monsterType)
                    continue;

                // Check fame requirement
                if (minFame < def.FameRequirement)
                    continue;

                // Check region requirement
                if (!MatchesRegion(currentRegion, def.AllowedRegions))
                    continue;

                // Check map requirement
                if (!MatchesMap(currentMap, def.AllowedMaps))
                    continue;

                // Check if this card's rarity is within allowed range
                if (def.Rarity < minAllowedRarity || def.Rarity > maxAllowedRarity)
                    continue;

                eligible.Add(def);
                adjustedChances.Add(def.DropChance);
            }

            if (eligible.Count == 0)
                return null;

            double totalChance = 0;
            foreach (double chance in adjustedChances)
                totalChance += chance;

            double roll = Utility.RandomDouble();
            double cumulative = 0;

            for (int i = 0; i < eligible.Count; i++)
            {
                cumulative += adjustedChances[i] / totalChance;
                if (roll <= cumulative)
                {
                    return CreateCardFromDefinition(eligible[i]);
                }
            }

            return null;
        }

        private static List<CardDropDefinition> GetMonsterFlavorCards(string monsterType, int minFame, 
                Region currentRegion, Map currentMap, CardRarity minAllowed, CardRarity maxAllowed)
        {
            List<CardDropDefinition> monsterFlavorCards = new List<CardDropDefinition>();

            foreach (CardDropDefinition def in CardDrops)
            {
                if (!string.IsNullOrEmpty(def.SpecificMonster) && 
                        def.SpecificMonster == monsterType)
                {
                    if (def.Rarity >= minAllowed && def.Rarity <= maxAllowed &&
                            minFame >= def.FameRequirement &&
                            MatchesRegion(currentRegion, def.AllowedRegions) &&
                            MatchesMap(currentMap, def.AllowedMaps))
                    {
                        monsterFlavorCards.Add(def);
                    }
                }
            }

            return monsterFlavorCards;
        }

        private static List<CardDropDefinition> GetRegionCards(Region currentRegion, Map currentMap, 
                int minFame, CardRarity minAllowed, CardRarity maxAllowed)
        {
            List<CardDropDefinition> regionCards = new List<CardDropDefinition>();

            foreach (CardDropDefinition def in CardDrops)
            {
                // Skip monster-specific cards
                if (!string.IsNullOrEmpty(def.SpecificMonster))
                    continue;

                // Must have region restrictions and match current region
                if (def.AllowedRegions != null && def.AllowedRegions.Count > 0 &&
                        MatchesRegion(currentRegion, def.AllowedRegions))
                {
                    if (minFame >= def.FameRequirement &&
                            MatchesMap(currentMap, def.AllowedMaps))
                    {
                        regionCards.Add(def);
                    }
                }
            }

            return regionCards;
        }

        public static bool TryDropCard(Mobile victim, Mobile killer)
        {
            if (victim == null || killer == null)
                return false;

            // Get actual player (handle pets, summons, provoked)
            Mobile actualKiller = GetActualKiller(killer);
            if (actualKiller == null || !actualKiller.Player)
                return false;

            if (victim is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)victim;
                if (bc.Controlled || bc.Summoned)
                    return false;
            }

            string monsterType = victim.GetType().Name;
            int minFame = victim is BaseCreature ? ((BaseCreature)victim).Fame : 0;
            Region currentRegion = Region.Find(victim.Location, victim.Map);
            Map currentMap = victim.Map;

            CardRarity minAllowed, maxAllowed;
            CardSystemConfig.GetRarityRangeForFame(minFame, out minAllowed, out maxAllowed);

            // ==========================================
            // TIER 1: Monster-specific drops (highest priority)
            // ==========================================
            CardDropDefinition monsterSpecificDef = GetPreferredCardForMonster(monsterType);

            if (monsterSpecificDef != null)
            {
                bool qualifies = minFame >= monsterSpecificDef.FameRequirement &&
                    MatchesRegion(currentRegion, monsterSpecificDef.AllowedRegions) &&
                    MatchesMap(currentMap, monsterSpecificDef.AllowedMaps) &&
                    monsterSpecificDef.Rarity >= minAllowed && monsterSpecificDef.Rarity <= maxAllowed;

                if (qualifies)
                {
                    double chance = monsterSpecificDef.DropChance + monsterSpecificDef.PreferredDropBonus;

                    if (!monsterSpecificDef.IgnoreDropRateMultiplier)
                        chance = chance * CardSystemConfig.DropRateMultiplier;

                    //chance = Math.Min(chance, 0.95);
                    if (Utility.RandomDouble() < chance)
                    {
                        DeliverCard(actualKiller, CreateCardFromDefinition(monsterSpecificDef), victim.Name, victim);
                        return true;
                    }
                }
            }

            // ==========================================
            // TIER 2: General card pool (single random selection)
            // ==========================================

            // Build the eligible cards list
            List<CardDropDefinition> eligibleCards = new List<CardDropDefinition>();

            foreach (CardDropDefinition def in CardDrops)
            {
                // Skip monster-specific cards (already handled above)
                if (!string.IsNullOrEmpty(def.SpecificMonster))
                    continue;

                // Check if this card is eligible to drop
                bool qualifiesByRarity = (def.Rarity >= minAllowed && def.Rarity <= maxAllowed);
                bool qualifiesByRegion = (def.AllowedRegions != null && def.AllowedRegions.Count > 0);

                if ((qualifiesByRarity || qualifiesByRegion) &&
                        minFame >= def.FameRequirement &&
                        MatchesMap(currentMap, def.AllowedMaps))
                {
                    // Region cards are only eligible if they match current region
                    if (def.AllowedRegions != null && def.AllowedRegions.Count > 0)
                    {
                        if (MatchesRegion(currentRegion, def.AllowedRegions))
                            eligibleCards.Add(def);
                    }
                    else if (qualifiesByRarity)  // Non-region cards just need to pass rarity check
                    {
                        eligibleCards.Add(def);
                    }
                }
            }

            if (eligibleCards.Count == 0)
                return false;

            // STEP 1: Randomly select a card from the eligible pool (uniform selection)
            CardDropDefinition selectedDef = eligibleCards[Utility.Random(eligibleCards.Count)];

            // STEP 2: Check if that specific card drops based on its chance (modified by DropRateMultiplier)
            double dropChance = selectedDef.DropChance;
            if (!selectedDef.IgnoreDropRateMultiplier)
                dropChance *= CardSystemConfig.DropRateMultiplier;
            
	    //dropChance = Math.Min(dropChance, 0.95);

            if (Utility.RandomDouble() >= dropChance)
                return false;  // The selected card failed its chance roll - no drop

            // STEP 3: Apply flavor replacements (monster or region specific)
            bool replaced = false;

            // Monster flavor chance - replace with a monster-specific card if available
            if (!replaced && CardSystemConfig.MonsterFlavorChance > 0 && 
                    Utility.RandomDouble() < CardSystemConfig.MonsterFlavorChance)
            {
                List<CardDropDefinition> monsterFlavorCards = GetMonsterFlavorCards(
                        monsterType, minFame, currentRegion, currentMap, minAllowed, maxAllowed);

                if (monsterFlavorCards.Count > 0)
                {
                    selectedDef = monsterFlavorCards[Utility.Random(monsterFlavorCards.Count)];
                    replaced = true;
                }
            }

            // Region flavor chance - replace with a region-specific card
            if (!replaced && CardSystemConfig.RegionFlavorChance > 0 && 
                    Utility.RandomDouble() < CardSystemConfig.RegionFlavorChance)
            {
                List<CardDropDefinition> regionCards = GetRegionCards(
                        currentRegion, currentMap, minFame, minAllowed, maxAllowed);

                if (regionCards.Count > 0)
                {
                    selectedDef = regionCards[Utility.Random(regionCards.Count)];
                    replaced = true;
                }
            }

            // Deliver the final selected card
            DeliverCard(actualKiller, CreateCardFromDefinition(selectedDef), null, victim);
            return true;
        }

        // Helper method to select a card by weighted DropChance
        private static CardDropDefinition SelectCardByWeight(List<CardDropDefinition> cards)
        {
            if (cards == null || cards.Count == 0)
                return null;

            double totalWeight = 0;
            foreach (CardDropDefinition def in cards)
                totalWeight += def.DropChance;

            double roll = Utility.RandomDouble() * totalWeight;
            double cumulative = 0;

            foreach (CardDropDefinition def in cards)
            {
                cumulative += def.DropChance;
                if (roll <= cumulative)
                    return def;
            }

            return cards[0];
        }

        private static void DeliverCard(Mobile player, CollectableCard card, string monsterName, Mobile victim)
        {
            if (card == null || player == null || victim == null)
                return;

            // Add the card to the victim's corpse
            if (victim.Corpse != null && !victim.Corpse.Deleted)
            {
                victim.Corpse.DropItem(card);
            }
            else
            {
                // Fallback: if no corpse exists, put in backpack
                player.AddToBackpack(card);
            }

            string message = FormatDropMessage(card.Rarity, monsterName, card.AchievementPoints);
            int messageHue = CardSystemConfig.GetMessageHueForRarity(card.Rarity);

            player.SendMessage(messageHue, message);
            player.PlaySound(CardSystemConfig.CardDropSound);
        }

        private static Mobile GetActualKiller(Mobile killer)
        {
            if (killer == null) return null;

            if (killer is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)killer;
                if (bc.Summoned && bc.SummonMaster != null)
                    return bc.SummonMaster;
                if (bc.Controlled && bc.ControlMaster != null)
                    return bc.ControlMaster;
                if (bc.BardProvoked && bc.BardMaster != null)
                    return bc.BardMaster;
            }

            return killer;
        }

        public static CollectableCard GetRandomCardDrop(int minFame = 0, string monsterType = null,
                Region currentRegion = null, Map currentMap = null, CardRarity minRarity = CardRarity.Common, 
                CardRarity maxRarity = CardRarity.Special, CardDropDefinition excludeDef = null)
        {
            List<CardDropDefinition> eligible = new List<CardDropDefinition>();

            foreach (CardDropDefinition def in CardDrops)
            {
                // Skip excluded card
                if (excludeDef != null && def == excludeDef)
                    continue;

                // Check rarity range
                if (def.Rarity < minRarity || def.Rarity > maxRarity)
                    continue;

                // Skip cards that require a specific monster (unless we're that monster)
                if (!string.IsNullOrEmpty(def.SpecificMonster) && def.SpecificMonster != monsterType)
                    continue;

                // Check fame requirement
                if (minFame < def.FameRequirement)
                    continue;

                // Check region requirement
                if (!MatchesRegion(currentRegion, def.AllowedRegions))
                    continue;

                // Check map requirement
                if (!MatchesMap(currentMap, def.AllowedMaps))
                    continue;

                eligible.Add(def);
            }

            if (eligible.Count == 0)
                return null;

            double totalChance = 0;
            foreach (CardDropDefinition def in eligible)
                totalChance += def.DropChance;

            double roll = Utility.RandomDouble();
            double cumulative = 0;

            foreach (CardDropDefinition def in eligible)
            {
                cumulative += def.DropChance / totalChance;
                if (roll <= cumulative)
                {
                    return CreateCardFromDefinition(def);
                }
            }

            return null;
        }

        private static bool MatchesRegion(Region currentRegion, List<string> allowedRegions)
        {
            if (allowedRegions == null || allowedRegions.Count == 0)
                return true;

            if (currentRegion == null || string.IsNullOrEmpty(currentRegion.Name))
                return false;

            string currentRegionName = currentRegion.Name;

            foreach (string allowedRegion in allowedRegions)
            {
                if (currentRegionName.IndexOf(allowedRegion, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static bool MatchesMap(Map currentMap, List<string> allowedMaps)
        {
            if (allowedMaps == null || allowedMaps.Count == 0)
                return true;

            if (currentMap == null || string.IsNullOrEmpty(currentMap.Name))
                return false;

            return allowedMaps.Contains(currentMap.Name);
        }

        /// <summary>
        /// Gets the correct indefinite article (a/an) for a word
        /// </summary>
        public static string GetIndefiniteArticle(string word)
        {
            if (string.IsNullOrEmpty(word))
                return "a";

            char firstChar = char.ToLower(word[0]);
            if (firstChar == 'a' || firstChar == 'e' || firstChar == 'i' || firstChar == 'o' || firstChar == 'u')
                return "an";
            return "a";
        }

        /// <summary>
        /// Formats a drop message with proper grammar
        /// </summary>
        public static string FormatDropMessage(CardRarity rarity, string monsterName = null, int points = 0)
        {
            string rarityName = CardRarityInfo.GetName(rarity).ToLower();
            string article = GetIndefiniteArticle(rarityName);

            string message;
            if (string.IsNullOrEmpty(monsterName))
                message = string.Format(CardSystemConfig.MonsterDropMessageFormat, article, rarityName);
            else
                message = string.Format(CardSystemConfig.NamedMonsterDropMessageFormat, article, rarityName, monsterName);

            if (CardSystemConfig.EnablePointsSystem && CardSystemConfig.ShowPointsInDropMessage && points > 0)
                message += string.Format(CardSystemConfig.PointsSuffixFormat, points);

            return message;
        }

        /// <summary>
        /// Gets a random card of the specified rarity (for chests, quest rewards, etc.)
        /// </summary>
        public static CollectableCard GetRandomCardByRarity(CardRarity rarity)
        {
            List<CardDropDefinition> eligibleCards = new List<CardDropDefinition>();

            foreach (CardDropDefinition def in CardDrops)
            {
                if (def.Rarity == rarity)
                {
                    eligibleCards.Add(def);
                }
            }

            if (eligibleCards.Count == 0)
                return null;

            CardDropDefinition selectedDef = eligibleCards[Utility.Random(eligibleCards.Count)];
            return CreateCardFromDefinition(selectedDef);
        }

        /// <summary>
        /// Gets a random card from a specific category
        /// </summary>
        public static CollectableCard GetRandomCardByCategory(CardCategory category)
        {
            List<CardDropDefinition> eligibleCards = new List<CardDropDefinition>();

            foreach (CardDropDefinition def in CardDrops)
            {
                if (def.Category == category)
                {
                    eligibleCards.Add(def);
                }
            }

            if (eligibleCards.Count == 0)
                return null;

            CardDropDefinition selectedDef = eligibleCards[Utility.Random(eligibleCards.Count)];
            return CreateCardFromDefinition(selectedDef);
        }

        // Chests and tresure chests

        /// <summary>
        /// Attempts to add a card to a chest based on chest level and region
        /// </summary>
        public static bool TryAddCardToChest(Container chest, int chestLevel, Mobile opener, Region currentRegion, Map currentMap, bool sendNotification = true)
        {
            if (chest == null || opener == null)
                return false;

            // Calculate base chance as a percentage (0-100)
            double cardChance = chestLevel * CardSystemConfig.ChestBaseChanceMultiplier;

            // Apply DropRateMultiplier (which could be >1 for events or <1 for reduced rates)
            cardChance = cardChance * CardSystemConfig.DropRateMultiplier;

            // Luck bonuses (additive percentage)
            if (opener.Luck >= 2500)
                cardChance += 1.5;
            else if (opener.Luck >= 2000)
                cardChance += 1.25;
            else if (opener.Luck >= 1500)
                cardChance += 1.0;
            else if (opener.Luck >= 1000)
                cardChance += 0.5;
            else if (opener.Luck >= 500)
                cardChance += 0.25;
            else if (opener.Luck >= 250)
                cardChance += 0.12;

            // Apply configured maximum cap
            cardChance = Math.Min(cardChance, CardSystemConfig.MaxChestCardChance);

            // Roll for card drop (cardChance is now a percentage 0-100)
            int roll = Utility.Random(100);
            if (roll >= cardChance)
                return false;

            // Determine rarity range based on chest level
            RarityRange range = GetChestRarityRange(chestLevel);

            // Try to get a region-specific card first
            CollectableCard card = GetRandomCardDrop(0, null, currentRegion, currentMap, range.Min, range.Max);

            // If no region card found, try without region restrictions
            if (card == null)
            {
                card = GetRandomCardDrop(0, null, null, null, range.Min, range.Max);
            }

            if (card == null)
                return false;

            // Add card to chest
            chest.DropItem(card);

            // Send notification if requested
            if (sendNotification && opener != null)
            {
                string rarityName = CardRarityInfo.GetName(card.Rarity).ToLower();
                string article = GetIndefiniteArticle(rarityName);
                string message = string.Format(CardSystemConfig.DungeonChestDropMessageFormat, article, rarityName);

                if (CardSystemConfig.EnablePointsSystem && CardSystemConfig.ShowPointsInDropMessage)
                {
                    message += string.Format(CardSystemConfig.PointsSuffixFormat, card.AchievementPoints);
                }

                int messageHue = CardSystemConfig.GetMessageHueForRarity(card.Rarity);
                opener.SendMessage(messageHue, message);
                opener.PlaySound(CardSystemConfig.CardDropSound);
            }

            return true;
        }

        /// <summary>
        /// Gets the rarity range for a chest based on its level
        /// </summary>
        public static RarityRange GetChestRarityRange(int chestLevel)
        {
            switch (chestLevel)
            {
                case 10: // Level 9-10 - Epic to Legendary
                case 9:
                    return new RarityRange(CardRarity.Epic, CardRarity.Legendary);
                case 8:  // Level 8-7 - Rare to Legendary
                case 7:
                    return new RarityRange(CardRarity.Rare, CardRarity.Legendary);
                case 6:  // Level 5-6 - Rare to Epic
                case 5:
                    return new RarityRange(CardRarity.Rare, CardRarity.Epic);
                case 4:  //  Level 3-4 - Uncommon to Rare
                case 3:
                    return new RarityRange(CardRarity.Uncommon, CardRarity.Rare);
                case 2:  //  Level 1-2 - Common to Uncommon
                case 1:
                default:
                    return new RarityRange(CardRarity.Common, CardRarity.Uncommon);
            }
        }

        /// <summary>
        /// Attempts to add a card to a treasure map chest based on map level
        /// </summary>
        public static bool TryAddCardToTreasureChest(Container chest, int mapLevel, Mobile opener, Region currentRegion, Map currentMap, bool sendNotification = true)
        {
            if (chest == null || opener == null)
                return false;

            // Base chance: 2% per map level (0-14%)
            double cardChance = mapLevel * 2.0;

            // Apply DropRateMultiplier
            cardChance = cardChance * CardSystemConfig.DropRateMultiplier;

            // Luck bonuses (additive percentage)
            if (opener.Luck >= 2500)
                cardChance += 1.5;
            else if (opener.Luck >= 2000)
                cardChance += 1.25;
            else if (opener.Luck >= 1500)
                cardChance += 1.0;
            else if (opener.Luck >= 1000)
                cardChance += 0.5;
            else if (opener.Luck >= 500)
                cardChance += 0.25;
            else if (opener.Luck >= 250)
                cardChance += 0.12;

            // Apply configured maximum cap
            cardChance = Math.Min(cardChance, CardSystemConfig.MaxChestCardChance);

            // Roll for card drop
            int roll = Utility.Random(100);
            if (roll >= cardChance)
                return false;

            // Get rarity range based on treasure map level
            RarityRange range = GetTreasureMapChestRarityRange(mapLevel);

            // Try to get a region-specific card first
            CollectableCard card = GetRandomCardDrop(0, null, currentRegion, currentMap, range.Min, range.Max);

            // If no region card found, try without region restrictions
            if (card == null)
            {
                card = GetRandomCardDrop(0, null, null, null, range.Min, range.Max);
            }

            if (card == null)
                return false;

            // Add card to chest
            chest.DropItem(card);

            // Send notification if requested
            if (sendNotification && opener != null)
            {
                string rarityName = CardRarityInfo.GetName(card.Rarity).ToLower();
                string article = GetIndefiniteArticle(rarityName);
                string message = string.Format(CardSystemConfig.TreasureChestDropMessageFormat, article, rarityName);

                if (CardSystemConfig.EnablePointsSystem && CardSystemConfig.ShowPointsInDropMessage)
                {
                    message += string.Format(CardSystemConfig.PointsSuffixFormat, card.AchievementPoints);
                }

                int messageHue = CardSystemConfig.GetMessageHueForRarity(card.Rarity);
                opener.SendMessage(messageHue, message);
                opener.PlaySound(CardSystemConfig.CardDropSound);
            }

            return true;
        }

        /// <summary>
        /// Gets the rarity range for a treasure map chest based on its level
        /// </summary>
        public static RarityRange GetTreasureMapChestRarityRange(int mapLevel)
        {
            switch (mapLevel)
            {
                case 7:  // Level 7 - Epic to Legendary
                    return new RarityRange(CardRarity.Epic, CardRarity.Legendary);
                case 6:  // Level 5-6 - Rare, Epic, or Legendary
                case 5:
                    return new RarityRange(CardRarity.Rare, CardRarity.Legendary);
                case 4:  // Level 4 - Rare or Epic
                    return new RarityRange(CardRarity.Rare, CardRarity.Epic);
                case 3:  // Level 2-3 - Uncommon or Rare
                case 2:
                    return new RarityRange(CardRarity.Uncommon, CardRarity.Rare);
                case 1:  // Level 1 - Uncommon
                default:
                    return new RarityRange(CardRarity.Uncommon, CardRarity.Uncommon);
            }
        }

        /// <summary>
        /// Gets the player vendor bot market value for an individual card
        /// </summary>
        public static int GetCardMarketValue(CollectableCard card)
        {
            if (card == null)
                return 0;

            // Base value by rarity
            int baseValue = 0;
            switch (card.Rarity)
            {
                case CardRarity.Common: baseValue = 500; break;
                case CardRarity.Uncommon: baseValue = 1500; break;
                case CardRarity.Rare: baseValue = 5000; break;
                case CardRarity.Epic: baseValue = 8500; break;
                case CardRarity.Legendary: baseValue = 15000; break;
                case CardRarity.Special: baseValue = 25000; break;
                default: baseValue = 25; break;
            }

            // Small random fluctuation (±10%)
            double fluctuation = 0.9 + (Utility.RandomDouble() * 0.2);
            int finalValue = (int)(baseValue * fluctuation);

            return Math.Max(1, finalValue);
        }

        /// <summary>
        /// Gets the player vendor bot market value for a card tome
        /// </summary>
        public static int GetCardTomeMarketValue(CardTome tome)
        {
            if (tome == null)
                return 500;

            int baseTomeValue = 500;
            int cardsTotalValue = 0;

            foreach (CollectableCard card in tome.Cards)
            {
                cardsTotalValue += GetCardMarketValue(card);
            }

            // Fluctuation
            double fluctuation = 0.85 + (Utility.RandomDouble() * 0.3);

            int totalValue = (int)((baseTomeValue + cardsTotalValue) * fluctuation);

            return Math.Max(100, totalValue);
        }

        public class CardDropDefinition
        {
            // Card properties (used for generic card creation if type not specified)
            public string CardName { get; set; }
            public string Subtitle { get; set; }
            public CardRarity Rarity { get; set; }
            public CardCategory Category { get; set; }
            public int ImageID { get; set; }
            public string Description { get; set; }

            // The actual card type to instantiate (preferred)
            public Type CardType { get; set; }

            // Drop conditions
            public double DropChance { get; set; }
            public int FameRequirement { get; set; }
            public string SpecificMonster { get; set; }
            public List<string> AllowedRegions { get; set; }
            public List<string> AllowedMaps { get; set; }

            // Priority system properties (using properties instead of fields)
            public bool IsPreferredForMonster { get; set; }
            public double PreferredDropBonus { get; set; }

            // Override the fame-based rarity range for this specific monster
            public CardRarity? MinimumDropRarity { get; set; }
            public CardRarity? MaximumDropRarity { get; set; }

            // Ignore the drop rate multiplier, e.g. for guaranteed drops
            public bool IgnoreDropRateMultiplier { get; set; }

            // Optional custom border gump ID (0 = use system default)
            public int CustomBorderGumpID { get; set; }
        }
    }
}
