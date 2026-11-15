using System;
using System.Collections.Generic;
using System.Text;
using Server.Items;
using Server.Misc;

namespace Server.Custom.VendorSearch
{
    public sealed class VendorSearchSubcategoryDefinition
    {
        public VendorSearchSubcategoryDefinition( string key, string name, VendorSearchSubcategoryFlags flag )
        {
            Key = key;
            Name = name;
            Flag = flag;
        }

        public string Key { get; private set; }
        public string Name { get; private set; }
        public VendorSearchSubcategoryFlags Flag { get; private set; }
    }

    public sealed class VendorSearchCategoryDefinition
    {
        private readonly List<VendorSearchSubcategoryDefinition> m_Subcategories;
        private readonly IList<VendorSearchSubcategoryDefinition> m_ReadOnlySubcategories;

        public VendorSearchCategoryDefinition( string key, string name, VendorSearchCategoryFlags flag )
        {
            Key = key;
            Name = name;
            Flag = flag;
            m_Subcategories = new List<VendorSearchSubcategoryDefinition>();
            m_ReadOnlySubcategories = m_Subcategories.AsReadOnly();
        }

        public string Key { get; private set; }
        public string Name { get; private set; }
        public VendorSearchCategoryFlags Flag { get; private set; }
        public IList<VendorSearchSubcategoryDefinition> Subcategories { get { return m_ReadOnlySubcategories; } }

        internal void Add( VendorSearchSubcategoryDefinition definition )
        {
            if ( definition != null )
                m_Subcategories.Add( definition );
        }
    }

    public static class VendorSearchCatalog
    {
        private static readonly string[] m_HandheldLightKeywords = new string[] { "torch", "lantern", "candle" };
        private static readonly string[] m_HandheldLightExclusions = new string[] { "candelab", "lamp post", "lamppost", "lamp-post", "lampost" };
        private static readonly List<VendorSearchCategoryDefinition> m_Categories = BuildCategories();
        private static readonly IList<VendorSearchCategoryDefinition> m_ReadOnlyCategories = m_Categories.AsReadOnly();
        private static readonly Dictionary<string, VendorSearchCategoryDefinition> m_CategoryLookup = BuildCategoryLookup();

        public static IList<VendorSearchCategoryDefinition> Categories
        {
            get { return m_ReadOnlyCategories; }
        }

        public static VendorSearchCategoryDefinition FindCategory( string key )
        {
            VendorSearchCategoryDefinition category;

            if ( String.IsNullOrEmpty( key ) || !m_CategoryLookup.TryGetValue( key, out category ) )
                return null;

            return category;
        }

        public static VendorSearchSubcategoryDefinition FindSubcategory( string categoryKey, string subcategoryKey )
        {
            VendorSearchCategoryDefinition category = FindCategory( categoryKey );

            if ( category == null || String.IsNullOrEmpty( subcategoryKey ) )
                return null;

            for ( int i = 0; i < category.Subcategories.Count; i++ )
            {
                VendorSearchSubcategoryDefinition subcategory = category.Subcategories[i];

                if ( String.Equals( subcategory.Key, subcategoryKey, StringComparison.OrdinalIgnoreCase ) )
                    return subcategory;
            }

            return null;
        }

        public static void NormalizeQuery( VendorSearchQuery query )
        {
            if ( query == null )
                return;

            query.SearchTerm = NormalizeText( query.SearchTerm, VendorSearchLimits.MaximumSearchTextLength );
            query.VendorName = NormalizeText( query.VendorName, VendorSearchLimits.MaximumFilterTextLength );
            query.RegionTerm = NormalizeText( query.RegionTerm, VendorSearchLimits.MaximumFilterTextLength );

            VendorSearchCategoryDefinition category = FindCategory( query.CategoryKey );

            if ( category == null )
                category = m_Categories[0];

            query.CategoryKey = category.Key;

            VendorSearchSubcategoryDefinition subcategory = FindSubcategory( category.Key, query.SubcategoryKey );

            if ( subcategory == null )
                subcategory = category.Subcategories[0];

            query.SubcategoryKey = subcategory.Key;

            query.MinPrice = ClampToZero( query.MinPrice );
            query.MaxPrice = ClampToZero( query.MaxPrice );
            query.MinPricePerItem = ClampToZero( query.MinPricePerItem );
            query.MaxPricePerItem = ClampToZero( query.MaxPricePerItem );
            query.MinStackAmount = ClampToZero( query.MinStackAmount );

            if ( query.CreatedAfter.HasValue && query.CreatedAfter.Value > DateTime.UtcNow )
                query.CreatedAfter = null;

            if ( query.Page < 0 )
                query.Page = 0;

            if ( query.AdvancedPage < VendorSearchAdvancedPage.General
                || query.AdvancedPage > VendorSearchAdvancedPage.WeaponEffects )
                query.AdvancedPage = VendorSearchAdvancedPage.General;
        }

        public static void AddCategoryCounts( VendorSearchRecord record, IDictionary<string, int> counts )
        {
            if ( record == null || counts == null )
                return;

            Increment( counts, "all" );

            for ( int i = 1; i < m_Categories.Count; i++ )
            {
                VendorSearchCategoryDefinition category = m_Categories[i];

                if ( ( record.CategoryFlags & category.Flag ) != 0 )
                    Increment( counts, category.Key );
            }
        }

        public static string ResolveItemName( Item item )
        {
            if ( item == null )
                return String.Empty;

            string name = item.Name;

            if ( String.IsNullOrEmpty( name ) )
                name = item.ItemData.Name;

            if ( String.IsNullOrEmpty( name ) )
                name = FormatTypeName( item.GetType() );

            return name ?? String.Empty;
        }

        public static string BuildSearchText( Item item, string displayName, string description, string vendorName, string shopName, string regionName, string facetName )
        {
            StringBuilder text = new StringBuilder( 256 );

            AppendSearchPart( text, displayName );
            AppendSearchPart( text, description );
            AppendSearchPart( text, vendorName );
            AppendSearchPart( text, shopName );
            AppendSearchPart( text, regionName );
            AppendSearchPart( text, facetName );

            if ( item != null )
            {
                AppendSearchPart( text, item.GetType().Name );
                AppendSearchPart( text, FormatTypeName( item.GetType() ) );

                foreach ( string value in EnumerateItemSpecificText( item ) )
                    AppendSearchPart( text, value );
            }

            string result = text.ToString().Trim().ToLowerInvariant();
            return result.Length <= 512 ? result : result.Substring( 0, 512 );
        }

        public static void Classify( Item item, out VendorSearchCategoryFlags categories, out VendorSearchSubcategoryFlags subcategories )
        {
            categories = VendorSearchCategoryFlags.None;
            subcategories = VendorSearchSubcategoryFlags.None;

            if ( item == null || item.Deleted )
                return;

            ClassifyWeapons( item, ref categories, ref subcategories );
            ClassifyArmor( item, ref categories, ref subcategories );
            ClassifyMagic( item, ref categories, ref subcategories );
            ClassifyClothing( item, ref categories, ref subcategories );
            ClassifyJewelry( item, ref categories, ref subcategories );
            ClassifyResources( item, ref categories, ref subcategories );
            ClassifyConsumables( item, ref categories, ref subcategories );

            if ( categories == VendorSearchCategoryFlags.None )
                categories = VendorSearchCategoryFlags.Misc;
        }

        private static List<VendorSearchCategoryDefinition> BuildCategories()
        {
            List<VendorSearchCategoryDefinition> categories = new List<VendorSearchCategoryDefinition>();

            categories.Add( CreateCategory( "all", "All", VendorSearchCategoryFlags.All,
                Subcategory( "all", "All", VendorSearchSubcategoryFlags.None ) ) );

            categories.Add( CreateCategory( "weapons", "Weapons", VendorSearchCategoryFlags.Weapons,
                Subcategory( "all", "All Weapons", VendorSearchSubcategoryFlags.None ),
                Subcategory( "swords", "Swords", VendorSearchSubcategoryFlags.Swords ),
                Subcategory( "axes", "Axes", VendorSearchSubcategoryFlags.Axes ),
                Subcategory( "maces", "Maces", VendorSearchSubcategoryFlags.Maces ),
                Subcategory( "polearms", "Polearms", VendorSearchSubcategoryFlags.Polearms ),
                Subcategory( "daggers", "Daggers", VendorSearchSubcategoryFlags.Daggers ),
                Subcategory( "ranged", "Ranged", VendorSearchSubcategoryFlags.Ranged ),
                Subcategory( "offhand", "Off-Hand", VendorSearchSubcategoryFlags.OffHand ) ) );

            categories.Add( CreateCategory( "armor", "Armor", VendorSearchCategoryFlags.Armor,
                Subcategory( "all", "All Armor", VendorSearchSubcategoryFlags.None ),
                Subcategory( "plate", "Plate", VendorSearchSubcategoryFlags.Plate ),
                Subcategory( "chain", "Chain", VendorSearchSubcategoryFlags.Chain ),
                Subcategory( "leather", "Leather", VendorSearchSubcategoryFlags.LeatherArmor ),
                Subcategory( "bone", "Bone", VendorSearchSubcategoryFlags.Bone ),
                Subcategory( "shields", "Shields", VendorSearchSubcategoryFlags.Shields ) ) );

            categories.Add( CreateCategory( "magic", "Magic", VendorSearchCategoryFlags.Magic,
                Subcategory( "all", "All Magic", VendorSearchSubcategoryFlags.None ),
                Subcategory( "spellbooks", "Spellbooks", VendorSearchSubcategoryFlags.Spellbooks ),
                Subcategory( "scrolls", "Scrolls", VendorSearchSubcategoryFlags.Scrolls ),
                Subcategory( "wands-staves", "Wands & Staves", VendorSearchSubcategoryFlags.WandsAndStaves ),
                Subcategory( "reagents", "Reagents", VendorSearchSubcategoryFlags.MagicReagents ),
                Subcategory( "runebooks", "Runebooks", VendorSearchSubcategoryFlags.Runebooks ) ) );

            categories.Add( CreateCategory( "clothing", "Clothing", VendorSearchCategoryFlags.Clothing,
                Subcategory( "all", "All Clothing", VendorSearchSubcategoryFlags.None ),
                Subcategory( "hats", "Hats", VendorSearchSubcategoryFlags.Hats ),
                Subcategory( "robes", "Robes", VendorSearchSubcategoryFlags.Robes ),
                Subcategory( "shirts", "Shirts", VendorSearchSubcategoryFlags.Shirts ),
                Subcategory( "pants", "Pants", VendorSearchSubcategoryFlags.Pants ),
                Subcategory( "footwear", "Footwear", VendorSearchSubcategoryFlags.Footwear ),
                Subcategory( "cloaks", "Cloaks", VendorSearchSubcategoryFlags.Cloaks ) ) );

            categories.Add( CreateCategory( "jewelry", "Jewelry", VendorSearchCategoryFlags.Jewelry,
                Subcategory( "all", "All Jewelry", VendorSearchSubcategoryFlags.None ),
                Subcategory( "rings", "Rings", VendorSearchSubcategoryFlags.Rings ),
                Subcategory( "bracelets", "Bracelets", VendorSearchSubcategoryFlags.Bracelets ),
                Subcategory( "necklaces", "Necklaces", VendorSearchSubcategoryFlags.Necklaces ),
                Subcategory( "earrings", "Earrings", VendorSearchSubcategoryFlags.Earrings ),
                Subcategory( "trinkets", "Trinkets", VendorSearchSubcategoryFlags.Trinkets ) ) );

            categories.Add( CreateCategory( "resources", "Resources", VendorSearchCategoryFlags.Resources,
                Subcategory( "all", "All Resources", VendorSearchSubcategoryFlags.None ),
                Subcategory( "metals", "Metals", VendorSearchSubcategoryFlags.Metals ),
                Subcategory( "wood", "Wood", VendorSearchSubcategoryFlags.Wood ),
                Subcategory( "leather", "Leather", VendorSearchSubcategoryFlags.LeatherResources ),
                Subcategory( "scales", "Scales", VendorSearchSubcategoryFlags.Scales ),
                Subcategory( "reagents", "Reagents", VendorSearchSubcategoryFlags.ResourceReagents ) ) );

            categories.Add( CreateCategory( "consumables", "Consumables", VendorSearchCategoryFlags.Consumables,
                Subcategory( "all", "All Consumables", VendorSearchSubcategoryFlags.None ),
                Subcategory( "potions", "Potions", VendorSearchSubcategoryFlags.Potions ),
                Subcategory( "food-drink", "Food & Drink", VendorSearchSubcategoryFlags.FoodAndDrink ),
                Subcategory( "bandages", "Bandages", VendorSearchSubcategoryFlags.Bandages ) ) );

            categories.Add( CreateCategory( "misc", "Misc", VendorSearchCategoryFlags.Misc,
                Subcategory( "all", "Everything Else", VendorSearchSubcategoryFlags.None ) ) );

            return categories;
        }

        private static Dictionary<string, VendorSearchCategoryDefinition> BuildCategoryLookup()
        {
            Dictionary<string, VendorSearchCategoryDefinition> lookup =
                new Dictionary<string, VendorSearchCategoryDefinition>( StringComparer.OrdinalIgnoreCase );

            for ( int i = 0; i < m_Categories.Count; i++ )
                lookup[m_Categories[i].Key] = m_Categories[i];

            return lookup;
        }

        private static VendorSearchCategoryDefinition CreateCategory( string key, string name, VendorSearchCategoryFlags flag, params VendorSearchSubcategoryDefinition[] subcategories )
        {
            VendorSearchCategoryDefinition category = new VendorSearchCategoryDefinition( key, name, flag );

            for ( int i = 0; i < subcategories.Length; i++ )
                category.Add( subcategories[i] );

            return category;
        }

        private static VendorSearchSubcategoryDefinition Subcategory( string key, string name, VendorSearchSubcategoryFlags flag )
        {
            return new VendorSearchSubcategoryDefinition( key, name, flag );
        }

        private static void ClassifyWeapons( Item item, ref VendorSearchCategoryFlags categories, ref VendorSearchSubcategoryFlags subcategories )
        {
            if ( item is BaseWeapon )
            {
                categories |= VendorSearchCategoryFlags.Weapons;

                if ( item is BaseSword )
                    subcategories |= VendorSearchSubcategoryFlags.Swords;
                if ( item is BaseAxe )
                    subcategories |= VendorSearchSubcategoryFlags.Axes;
                if ( item is BaseBashing )
                    subcategories |= VendorSearchSubcategoryFlags.Maces;
                if ( item is BasePoleArm )
                    subcategories |= VendorSearchSubcategoryFlags.Polearms;
                if ( item is BaseKnife )
                    subcategories |= VendorSearchSubcategoryFlags.Daggers;
                if ( item is BaseRanged )
                    subcategories |= VendorSearchSubcategoryFlags.Ranged;
            }
            else if ( IsOffHand( item ) )
            {
                categories |= VendorSearchCategoryFlags.Weapons;
                subcategories |= VendorSearchSubcategoryFlags.OffHand;
            }
        }

        private static void ClassifyArmor( Item item, ref VendorSearchCategoryFlags categories, ref VendorSearchSubcategoryFlags subcategories )
        {
            BaseArmor armor = item as BaseArmor;

            if ( armor != null )
            {
                categories |= VendorSearchCategoryFlags.Armor;

                switch ( armor.MaterialType )
                {
                    case ArmorMaterialType.Plate:
                        subcategories |= VendorSearchSubcategoryFlags.Plate;
                        break;
                    case ArmorMaterialType.Chainmail:
                        subcategories |= VendorSearchSubcategoryFlags.Chain;
                        break;
                    case ArmorMaterialType.Leather:
                    case ArmorMaterialType.Studded:
                    case ArmorMaterialType.Spined:
                    case ArmorMaterialType.Horned:
                    case ArmorMaterialType.Barbed:
                        subcategories |= VendorSearchSubcategoryFlags.LeatherArmor;
                        break;
                    case ArmorMaterialType.Bone:
                        subcategories |= VendorSearchSubcategoryFlags.Bone;
                        break;
                }
            }

            if ( item is BaseShield )
            {
                categories |= VendorSearchCategoryFlags.Armor;
                subcategories |= VendorSearchSubcategoryFlags.Shields;
            }
        }

        private static void ClassifyMagic( Item item, ref VendorSearchCategoryFlags categories, ref VendorSearchSubcategoryFlags subcategories )
        {
            if ( item is Spellbook )
            {
                categories |= VendorSearchCategoryFlags.Magic;
                subcategories |= VendorSearchSubcategoryFlags.Spellbooks;
            }

            if ( item is SpellScroll )
            {
                categories |= VendorSearchCategoryFlags.Magic;
                subcategories |= VendorSearchSubcategoryFlags.Scrolls;
            }

            if ( item is BaseMagicStaff )
            {
                categories |= VendorSearchCategoryFlags.Magic;
                subcategories |= VendorSearchSubcategoryFlags.WandsAndStaves;
            }

            if ( item is BaseReagent )
            {
                categories |= VendorSearchCategoryFlags.Magic;
                subcategories |= VendorSearchSubcategoryFlags.MagicReagents;
            }

            if ( item is Runebook )
            {
                categories |= VendorSearchCategoryFlags.Magic;
                subcategories |= VendorSearchSubcategoryFlags.Runebooks;
            }
        }

        private static void ClassifyClothing( Item item, ref VendorSearchCategoryFlags categories, ref VendorSearchSubcategoryFlags subcategories )
        {
            if ( item is BaseArmor && !( item is BaseClothing ) )
                return;

            if ( !( item is BaseClothing ) && !( item is IClothingStub ) && !IsClothingLayer( item.Layer ) )
                return;

            categories |= VendorSearchCategoryFlags.Clothing;

            if ( item.Layer == Layer.Helm )
                subcategories |= VendorSearchSubcategoryFlags.Hats;
            if ( item.Layer == Layer.OuterTorso )
                subcategories |= VendorSearchSubcategoryFlags.Robes;
            if ( item.Layer == Layer.Shirt )
                subcategories |= VendorSearchSubcategoryFlags.Shirts;
            if ( item.Layer == Layer.Pants )
                subcategories |= VendorSearchSubcategoryFlags.Pants;
            if ( item.Layer == Layer.Shoes )
                subcategories |= VendorSearchSubcategoryFlags.Footwear;
            if ( item.Layer == Layer.Cloak )
                subcategories |= VendorSearchSubcategoryFlags.Cloaks;
        }

        private static void ClassifyJewelry( Item item, ref VendorSearchCategoryFlags categories, ref VendorSearchSubcategoryFlags subcategories )
        {
            BaseJewel jewel = item as BaseJewel;

            if ( jewel == null || !IsJewelryLayer( jewel.Layer ) )
                return;

            categories |= VendorSearchCategoryFlags.Jewelry;

            switch ( jewel.Layer )
            {
                case Layer.Ring:
                    subcategories |= VendorSearchSubcategoryFlags.Rings;
                    break;
                case Layer.Bracelet:
                    subcategories |= VendorSearchSubcategoryFlags.Bracelets;
                    break;
                case Layer.Neck:
                    subcategories |= VendorSearchSubcategoryFlags.Necklaces;
                    break;
                case Layer.Earrings:
                    subcategories |= VendorSearchSubcategoryFlags.Earrings;
                    break;
                case Layer.Talisman:
                    subcategories |= VendorSearchSubcategoryFlags.Trinkets;
                    break;
            }
        }

        private static void ClassifyResources( Item item, ref VendorSearchCategoryFlags categories, ref VendorSearchSubcategoryFlags subcategories )
        {
            if ( item is BaseOre || item is BaseIngot )
            {
                categories |= VendorSearchCategoryFlags.Resources;
                subcategories |= VendorSearchSubcategoryFlags.Metals;
            }

            if ( item is BaseLog || item is Board )
            {
                categories |= VendorSearchCategoryFlags.Resources;
                subcategories |= VendorSearchSubcategoryFlags.Wood;
            }

            if ( item is BaseLeather )
            {
                categories |= VendorSearchCategoryFlags.Resources;
                subcategories |= VendorSearchSubcategoryFlags.LeatherResources;
            }

            if ( item is BaseScales )
            {
                categories |= VendorSearchCategoryFlags.Resources;
                subcategories |= VendorSearchSubcategoryFlags.Scales;
            }

            if ( item is BaseReagent )
            {
                categories |= VendorSearchCategoryFlags.Resources;
                subcategories |= VendorSearchSubcategoryFlags.ResourceReagents;
            }
        }

        private static void ClassifyConsumables( Item item, ref VendorSearchCategoryFlags categories, ref VendorSearchSubcategoryFlags subcategories )
        {
            if ( item is BasePotion )
            {
                categories |= VendorSearchCategoryFlags.Consumables;
                subcategories |= VendorSearchSubcategoryFlags.Potions;
            }

            if ( item is Food || item is BaseBeverage )
            {
                categories |= VendorSearchCategoryFlags.Consumables;
                subcategories |= VendorSearchSubcategoryFlags.FoodAndDrink;
            }

            if ( item is Bandage )
            {
                categories |= VendorSearchCategoryFlags.Consumables;
                subcategories |= VendorSearchSubcategoryFlags.Bandages;
            }
        }

        private static bool IsOffHand( Item item )
        {
            if ( item == null || item is BaseWeapon || item is BaseShield || item is BaseTool || item is BaseHarvestTool )
                return false;

            return IsHandheldLightItem( item );
        }

        private static bool IsHandheldLightItem( Item item )
        {
            if ( item is BaseEquipableLight )
                return true;

            if ( LooksLikeHandheldLightName( item.Name )
                || LooksLikeHandheldLightName( item.ItemData.Name )
                || LooksLikeHandheldLightName( item.GetType().Name ) )
                return true;

            ItemData data = item.ItemData;

            if ( ( data.Flags & TileFlag.LightSource ) != 0
                && ( ( data.Flags & TileFlag.Wearable ) != 0 && data.Weight <= 10
                    || LooksLikeHandheldLightName( data.Name ) ) )
                return true;

            return ( item.Layer == Layer.OneHanded || item.Layer == Layer.TwoHanded ) && data.Weight <= 10;
        }

        private static bool LooksLikeHandheldLightName( string name )
        {
            if ( String.IsNullOrEmpty( name ) )
                return false;

            string lowered = name.ToLowerInvariant();

            for ( int i = 0; i < m_HandheldLightExclusions.Length; i++ )
            {
                if ( lowered.IndexOf( m_HandheldLightExclusions[i], StringComparison.Ordinal ) >= 0 )
                    return false;
            }

            for ( int i = 0; i < m_HandheldLightKeywords.Length; i++ )
            {
                if ( lowered.IndexOf( m_HandheldLightKeywords[i], StringComparison.Ordinal ) >= 0 )
                    return true;
            }

            return false;
        }

        private static bool IsJewelryLayer( Layer layer )
        {
            return layer == Layer.Ring
                || layer == Layer.Bracelet
                || layer == Layer.Neck
                || layer == Layer.Earrings
                || layer == Layer.Talisman;
        }

        private static bool IsClothingLayer( Layer layer )
        {
            switch ( layer )
            {
                case Layer.Arms:
                case Layer.Cloak:
                case Layer.Helm:
                case Layer.Gloves:
                case Layer.InnerLegs:
                case Layer.InnerTorso:
                case Layer.MiddleTorso:
                case Layer.OuterLegs:
                case Layer.OuterTorso:
                case Layer.Pants:
                case Layer.Shirt:
                case Layer.Shoes:
                case Layer.Waist:
                    return true;
            }

            return false;
        }

        private static IEnumerable<string> EnumerateItemSpecificText( Item item )
        {
            if ( item == null )
                yield break;

            PowerScroll powerScroll = item as PowerScroll;

            if ( powerScroll != null )
            {
                SkillInfo[] skills = SkillInfo.Table;
                int skillIndex = (int)powerScroll.Skill;
                string skillName = skillIndex >= 0 && skillIndex < skills.Length ? skills[skillIndex].Name : null;

                if ( !String.IsNullOrEmpty( skillName ) )
                    yield return skillName;

                yield return powerScroll.Skill.ToString();
                yield return String.Format( "{0} {1}", skillName, (int)powerScroll.Value );
                yield return powerScroll.Value.ToString( "F0" );
            }

            CommodityDeed deed = item as CommodityDeed;

            if ( deed != null )
            {
                Item commodity = deed.Commodity;

                if ( commodity != null )
                {
                    yield return ResolveItemName( commodity );
                    yield return FormatTypeName( commodity.GetType() );

                    if ( commodity.Amount > 0 )
                        yield return commodity.Amount.ToString();
                }
            }
        }

        private static string FormatTypeName( Type type )
        {
            if ( type == null )
                return String.Empty;

            string name = type.Name.Replace( '_', ' ' );

            if ( name.Length <= 1 )
                return name;

            StringBuilder text = new StringBuilder( name.Length * 2 );
            text.Append( name[0] );

            for ( int i = 1; i < name.Length; i++ )
            {
                char current = name[i];
                char previous = name[i - 1];

                if ( Char.IsUpper( current ) && !Char.IsWhiteSpace( previous ) && !Char.IsUpper( previous ) )
                    text.Append( ' ' );

                text.Append( current );
            }

            return text.ToString();
        }

        private static void AppendSearchPart( StringBuilder text, string value )
        {
            if ( text == null || String.IsNullOrEmpty( value ) )
                return;

            string trimmed = value.Trim();

            if ( trimmed.Length == 0 )
                return;

            if ( text.Length > 0 )
                text.Append( ' ' );

            text.Append( trimmed );
        }

        private static string NormalizeText( string value, int maximumLength )
        {
            if ( String.IsNullOrEmpty( value ) )
                return String.Empty;

            string trimmed = value.Trim();
            return trimmed.Length <= maximumLength ? trimmed : trimmed.Substring( 0, maximumLength );
        }

        private static int? ClampToZero( int? value )
        {
            return value.HasValue && value.Value < 0 ? 0 : value;
        }

        private static double? ClampToZero( double? value )
        {
            return value.HasValue && value.Value < 0.0 ? 0.0 : value;
        }

        private static void Increment( IDictionary<string, int> counts, string key )
        {
            int value;
            counts.TryGetValue( key, out value );
            counts[key] = value + 1;
        }
    }

    public abstract class VendorSearchPropertyDefinition
    {
        public abstract string Key { get; }
        public abstract string Extract( Item item );
        public abstract bool Matches( string indexedValue, string requestedValue );
    }

    public static class VendorSearchPropertyRegistry
    {
        private static readonly List<VendorSearchPropertyDefinition> m_Definitions =
            new List<VendorSearchPropertyDefinition>();

        static VendorSearchPropertyRegistry()
        {
            VendorSearchGearPropertyDefinitions.RegisterAll();
        }

        public static void Register( VendorSearchPropertyDefinition definition )
        {
            if ( definition == null || String.IsNullOrEmpty( definition.Key ) || Find( definition.Key ) != null )
                return;

            m_Definitions.Add( definition );
        }

        public static IDictionary<string, string> BuildValues( Item item )
        {
            if ( m_Definitions.Count == 0 )
                return null;

            Dictionary<string, string> values =
                new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            for ( int i = 0; i < m_Definitions.Count; i++ )
            {
                VendorSearchPropertyDefinition definition = m_Definitions[i];
                string value = definition.Extract( item );

                if ( !String.IsNullOrEmpty( value ) )
                    values[definition.Key] = value;
            }

            return values.Count == 0 ? null : values;
        }

        internal static bool Matches( VendorSearchRecord record, IList<VendorSearchPropertyFilter> filters )
        {
            if ( filters == null || filters.Count == 0 )
                return true;

            if ( record == null )
                return false;

            for ( int i = 0; i < filters.Count; i++ )
            {
                VendorSearchPropertyFilter filter = filters[i];

                if ( filter == null )
                    return false;

                VendorSearchPropertyDefinition definition = Find( filter.Key );
                string indexedValue;

                if ( definition == null
                    || !record.TryGetPropertyValue( definition.Key, out indexedValue )
                    || !definition.Matches( indexedValue, filter.Value ) )
                    return false;
            }

            return true;
        }

        private static VendorSearchPropertyDefinition Find( string key )
        {
            for ( int i = 0; i < m_Definitions.Count; i++ )
            {
                if ( String.Equals( m_Definitions[i].Key, key, StringComparison.OrdinalIgnoreCase ) )
                    return m_Definitions[i];
            }

            return null;
        }
    }
}
