using System;
using Server.Items;

namespace Server.Custom.VendorSearch
{
    internal static class VendorSearchPropertyKeys
    {
        public const string Material = "material";
        public const string Slayer = "slayer";
        public const string WeaponDamage = "weaponDamage";
        public const string HitChance = "hitChance";
        public const string DefenseChance = "defenseChance";
        public const string SwingSpeed = "swingSpeed";
        public const string SpellDamage = "spellDamage";
        public const string FasterCasting = "fasterCasting";
        public const string FasterCastRecovery = "fasterCastRecovery";
        public const string LowerManaCost = "lowerManaCost";
        public const string LowerReagentCost = "lowerReagentCost";
        public const string Luck = "luck";
        public const string BonusStrength = "bonusStrength";
        public const string BonusDexterity = "bonusDexterity";
        public const string BonusIntelligence = "bonusIntelligence";
        public const string BonusHits = "bonusHits";
        public const string BonusStamina = "bonusStamina";
        public const string EnhancePotions = "enhancePotions";
        public const string BonusMana = "bonusMana";
        public const string RegenHits = "regenHits";
        public const string RegenStamina = "regenStamina";
        public const string RegenMana = "regenMana";
        public const string PhysicalResist = "physicalResist";
        public const string FireResist = "fireResist";
        public const string ColdResist = "coldResist";
        public const string PoisonResist = "poisonResist";
        public const string EnergyResist = "energyResist";
        public const string SelfRepair = "selfRepair";
        public const string Durability = "durability";
        public const string LowerStatRequirement = "lowerStatRequirement";
        public const string SpellChanneling = "spellChanneling";
        public const string MageArmor = "mageArmor";
        public const string Exceptional = "exceptional";
        public const string Identified = "identified";
        public const string WeaponPhysicalDamage = "weaponPhysicalDamage";
        public const string WeaponFireDamage = "weaponFireDamage";
        public const string WeaponColdDamage = "weaponColdDamage";
        public const string WeaponPoisonDamage = "weaponPoisonDamage";
        public const string WeaponEnergyDamage = "weaponEnergyDamage";
        public const string WeaponChaosDamage = "weaponChaosDamage";
        public const string WeaponDirectDamage = "weaponDirectDamage";
        public const string HitMagicArrow = "hitMagicArrow";
        public const string HitHarm = "hitHarm";
        public const string HitFireball = "hitFireball";
        public const string HitLightning = "hitLightning";
        public const string HitDispel = "hitDispel";
        public const string HitPhysicalArea = "hitPhysicalArea";
        public const string HitFireArea = "hitFireArea";
        public const string HitColdArea = "hitColdArea";
        public const string HitPoisonArea = "hitPoisonArea";
        public const string HitEnergyArea = "hitEnergyArea";
        public const string HitLeechHits = "hitLeechHits";
        public const string HitLeechStamina = "hitLeechStamina";
        public const string HitLeechMana = "hitLeechMana";
        public const string HitLowerAttack = "hitLowerAttack";
        public const string HitLowerDefense = "hitLowerDefense";
    }

    internal sealed class VendorSearchNumericPropertyDefinition : VendorSearchPropertyDefinition
    {
        private readonly string m_Key;
        private readonly Func<Item, int?> m_Extractor;

        public VendorSearchNumericPropertyDefinition( string key, Func<Item, int?> extractor )
        {
            m_Key = key;
            m_Extractor = extractor;
        }

        public override string Key { get { return m_Key; } }

        public override string Extract( Item item )
        {
            if ( item == null || m_Extractor == null )
                return null;

            int? value = m_Extractor( item );

            if ( !value.HasValue || value.Value == 0 )
                return null;

            return value.Value.ToString( System.Globalization.CultureInfo.InvariantCulture );
        }

        public override bool Matches( string indexedValue, string requestedValue )
        {
            int indexed;
            int requested;

            return Int32.TryParse(
                    indexedValue,
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out indexed )
                && Int32.TryParse(
                    requestedValue,
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out requested )
                && indexed >= requested;
        }
    }

    internal sealed class VendorSearchTextPropertyDefinition : VendorSearchPropertyDefinition
    {
        private readonly string m_Key;
        private readonly Func<Item, string> m_Extractor;

        public VendorSearchTextPropertyDefinition( string key, Func<Item, string> extractor )
        {
            m_Key = key;
            m_Extractor = extractor;
        }

        public override string Key { get { return m_Key; } }

        public override string Extract( Item item )
        {
            return item == null || m_Extractor == null ? null : m_Extractor( item );
        }

        public override bool Matches( string indexedValue, string requestedValue )
        {
            return !String.IsNullOrEmpty( indexedValue )
                && !String.IsNullOrEmpty( requestedValue )
                && indexedValue.IndexOf( requestedValue.Trim(), StringComparison.OrdinalIgnoreCase ) >= 0;
        }
    }

    internal static class VendorSearchGearPropertyDefinitions
    {
        public static void RegisterAll()
        {
            RegisterText( VendorSearchPropertyKeys.Material, GetMaterial );
            RegisterText( VendorSearchPropertyKeys.Slayer, GetSlayer );

            RegisterAttribute( VendorSearchPropertyKeys.WeaponDamage, AosAttribute.WeaponDamage );
            RegisterAttribute( VendorSearchPropertyKeys.HitChance, AosAttribute.AttackChance );
            RegisterAttribute( VendorSearchPropertyKeys.DefenseChance, AosAttribute.DefendChance );
            RegisterAttribute( VendorSearchPropertyKeys.SwingSpeed, AosAttribute.WeaponSpeed );
            RegisterAttribute( VendorSearchPropertyKeys.SpellDamage, AosAttribute.SpellDamage );
            RegisterAttribute( VendorSearchPropertyKeys.FasterCasting, AosAttribute.CastSpeed );
            RegisterAttribute( VendorSearchPropertyKeys.FasterCastRecovery, AosAttribute.CastRecovery );
            RegisterAttribute( VendorSearchPropertyKeys.LowerManaCost, AosAttribute.LowerManaCost );
            RegisterAttribute( VendorSearchPropertyKeys.LowerReagentCost, AosAttribute.LowerRegCost );
            RegisterAttribute( VendorSearchPropertyKeys.Luck, AosAttribute.Luck );
            RegisterAttribute( VendorSearchPropertyKeys.BonusStrength, AosAttribute.BonusStr );
            RegisterAttribute( VendorSearchPropertyKeys.BonusDexterity, AosAttribute.BonusDex );
            RegisterAttribute( VendorSearchPropertyKeys.BonusIntelligence, AosAttribute.BonusInt );
            RegisterAttribute( VendorSearchPropertyKeys.BonusHits, AosAttribute.BonusHits );
            RegisterAttribute( VendorSearchPropertyKeys.BonusStamina, AosAttribute.BonusStam );
            RegisterAttribute( VendorSearchPropertyKeys.EnhancePotions, AosAttribute.EnhancePotions );
            RegisterAttribute( VendorSearchPropertyKeys.BonusMana, AosAttribute.BonusMana );
            RegisterAttribute( VendorSearchPropertyKeys.RegenHits, AosAttribute.RegenHits );
            RegisterAttribute( VendorSearchPropertyKeys.RegenStamina, AosAttribute.RegenStam );
            RegisterAttribute( VendorSearchPropertyKeys.RegenMana, AosAttribute.RegenMana );
            RegisterAttribute( VendorSearchPropertyKeys.SpellChanneling, AosAttribute.SpellChanneling );

            RegisterNumeric( VendorSearchPropertyKeys.PhysicalResist, delegate( Item item ) { return item.PhysicalResistance; } );
            RegisterNumeric( VendorSearchPropertyKeys.FireResist, delegate( Item item ) { return item.FireResistance; } );
            RegisterNumeric( VendorSearchPropertyKeys.ColdResist, delegate( Item item ) { return item.ColdResistance; } );
            RegisterNumeric( VendorSearchPropertyKeys.PoisonResist, delegate( Item item ) { return item.PoisonResistance; } );
            RegisterNumeric( VendorSearchPropertyKeys.EnergyResist, delegate( Item item ) { return item.EnergyResistance; } );
            RegisterNumeric( VendorSearchPropertyKeys.SelfRepair, GetSelfRepair );
            RegisterNumeric( VendorSearchPropertyKeys.Durability, GetDurability );
            RegisterNumeric( VendorSearchPropertyKeys.LowerStatRequirement, GetLowerStatRequirement );
            RegisterNumeric( VendorSearchPropertyKeys.MageArmor, GetMageArmor );
            RegisterNumeric( VendorSearchPropertyKeys.Exceptional, GetExceptional );
            RegisterNumeric( VendorSearchPropertyKeys.Identified, GetIdentified );

            RegisterWeaponDamage( VendorSearchPropertyKeys.WeaponPhysicalDamage, AosElementAttribute.Physical );
            RegisterWeaponDamage( VendorSearchPropertyKeys.WeaponFireDamage, AosElementAttribute.Fire );
            RegisterWeaponDamage( VendorSearchPropertyKeys.WeaponColdDamage, AosElementAttribute.Cold );
            RegisterWeaponDamage( VendorSearchPropertyKeys.WeaponPoisonDamage, AosElementAttribute.Poison );
            RegisterWeaponDamage( VendorSearchPropertyKeys.WeaponEnergyDamage, AosElementAttribute.Energy );
            RegisterWeaponDamage( VendorSearchPropertyKeys.WeaponChaosDamage, AosElementAttribute.Chaos );
            RegisterWeaponDamage( VendorSearchPropertyKeys.WeaponDirectDamage, AosElementAttribute.Direct );

            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitMagicArrow, AosWeaponAttribute.HitMagicArrow );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitHarm, AosWeaponAttribute.HitHarm );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitFireball, AosWeaponAttribute.HitFireball );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitLightning, AosWeaponAttribute.HitLightning );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitDispel, AosWeaponAttribute.HitDispel );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitPhysicalArea, AosWeaponAttribute.HitPhysicalArea );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitFireArea, AosWeaponAttribute.HitFireArea );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitColdArea, AosWeaponAttribute.HitColdArea );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitPoisonArea, AosWeaponAttribute.HitPoisonArea );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitEnergyArea, AosWeaponAttribute.HitEnergyArea );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitLeechHits, AosWeaponAttribute.HitLeechHits );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitLeechStamina, AosWeaponAttribute.HitLeechStam );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitLeechMana, AosWeaponAttribute.HitLeechMana );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitLowerAttack, AosWeaponAttribute.HitLowerAttack );
            RegisterWeaponAttribute( VendorSearchPropertyKeys.HitLowerDefense, AosWeaponAttribute.HitLowerDefend );
        }

        private static void RegisterAttribute( string key, AosAttribute attribute )
        {
            RegisterNumeric(
                key,
                delegate( Item item )
                {
                    AosAttributes attributes = GetAttributes( item );
                    return attributes == null ? (int?)null : attributes[attribute];
                } );
        }

        private static void RegisterNumeric( string key, Func<Item, int?> extractor )
        {
            VendorSearchPropertyRegistry.Register(
                new VendorSearchNumericPropertyDefinition( key, extractor ) );
        }

        private static void RegisterWeaponAttribute( string key, AosWeaponAttribute attribute )
        {
            RegisterNumeric(
                key,
                delegate( Item item )
                {
                    AosWeaponAttributes attributes = GetWeaponAttributes( item );
                    return attributes == null ? (int?)null : attributes[attribute];
                } );
        }

        private static void RegisterWeaponDamage( string key, AosElementAttribute element )
        {
            RegisterNumeric(
                key,
                delegate( Item item )
                {
                    BaseWeapon weapon = item as BaseWeapon;

                    if ( weapon == null )
                        return null;

                    int physical;
                    int fire;
                    int cold;
                    int poison;
                    int energy;
                    int chaos;
                    int direct;

                    weapon.GetDamageTypes(
                        null,
                        out physical,
                        out fire,
                        out cold,
                        out poison,
                        out energy,
                        out chaos,
                        out direct );

                    switch ( element )
                    {
                        case AosElementAttribute.Fire: return fire;
                        case AosElementAttribute.Cold: return cold;
                        case AosElementAttribute.Poison: return poison;
                        case AosElementAttribute.Energy: return energy;
                        case AosElementAttribute.Chaos: return chaos;
                        case AosElementAttribute.Direct: return direct;
                        default: return physical;
                    }
                } );
        }

        private static void RegisterText( string key, Func<Item, string> extractor )
        {
            VendorSearchPropertyRegistry.Register(
                new VendorSearchTextPropertyDefinition( key, extractor ) );
        }

        private static AosAttributes GetAttributes( Item item )
        {
            BaseWeapon weapon = item as BaseWeapon;
            if ( weapon != null )
                return weapon.Attributes;

            BaseArmor armor = item as BaseArmor;
            if ( armor != null )
                return armor.Attributes;

            BaseClothing clothing = item as BaseClothing;
            if ( clothing != null )
                return clothing.Attributes;

            BaseJewel jewel = item as BaseJewel;
            if ( jewel != null )
                return jewel.Attributes;

            Spellbook spellbook = item as Spellbook;
            if ( spellbook != null )
                return spellbook.Attributes;

            BaseInstrument instrument = item as BaseInstrument;
            if ( instrument != null )
                return instrument.Attributes;

            BaseQuiver quiver = item as BaseQuiver;
            return quiver == null ? null : quiver.Attributes;
        }

        private static AosArmorAttributes GetArmorAttributes( Item item )
        {
            BaseArmor armor = item as BaseArmor;
            if ( armor != null )
                return armor.ArmorAttributes;

            BaseClothing clothing = item as BaseClothing;
            return clothing == null ? null : clothing.ClothingAttributes;
        }

        private static AosWeaponAttributes GetWeaponAttributes( Item item )
        {
            BaseWeapon weapon = item as BaseWeapon;
            return weapon == null ? null : weapon.WeaponAttributes;
        }

        private static int? GetSelfRepair( Item item )
        {
            AosWeaponAttributes weapon = GetWeaponAttributes( item );
            AosArmorAttributes armor = GetArmorAttributes( item );
            int value = 0;

            if ( weapon != null )
                value = weapon.SelfRepair;

            if ( armor != null && armor.SelfRepair > value )
                value = armor.SelfRepair;

            return value;
        }

        private static int? GetLowerStatRequirement( Item item )
        {
            AosWeaponAttributes weapon = GetWeaponAttributes( item );
            AosArmorAttributes armor = GetArmorAttributes( item );
            int value = 0;

            if ( weapon != null )
                value = weapon.LowerStatReq;

            if ( armor != null && armor.LowerStatReq > value )
                value = armor.LowerStatReq;

            return value;
        }

        private static int? GetMageArmor( Item item )
        {
            AosArmorAttributes armor = GetArmorAttributes( item );
            return armor == null ? (int?)null : armor.MageArmor;
        }

        private static int? GetDurability( Item item )
        {
            BaseWeapon weapon = item as BaseWeapon;
            if ( weapon != null )
                return weapon.MaxHitPoints;

            BaseArmor armor = item as BaseArmor;
            if ( armor != null )
                return armor.MaxHitPoints;

            BaseJewel jewel = item as BaseJewel;
            if ( jewel != null )
                return jewel.MaxHitPoints;

            BaseInstrument instrument = item as BaseInstrument;
            return instrument == null ? (int?)null : instrument.MaxHitPoints;
        }

        private static string GetMaterial( Item item )
        {
            CraftResource resource = CraftResource.None;

            BaseWeapon weapon = item as BaseWeapon;
            if ( weapon != null )
                resource = weapon.Resource;

            BaseArmor armor = item as BaseArmor;
            if ( armor != null )
                resource = armor.Resource;

            BaseClothing clothing = item as BaseClothing;
            if ( clothing != null )
                resource = clothing.Resource;

            BaseJewel jewel = item as BaseJewel;
            if ( jewel != null )
                resource = jewel.Resource;

            BaseInstrument instrument = item as BaseInstrument;
            if ( instrument != null )
                resource = instrument.Resource;

            return resource == CraftResource.None ? null : CraftResources.GetName( resource );
        }

        private static string GetSlayer( Item item )
        {
            ISlayer slayer = item as ISlayer;

            if ( slayer == null )
                return null;

            string first = slayer.Slayer == SlayerName.None ? String.Empty : slayer.Slayer.ToString();
            string second = slayer.Slayer2 == SlayerName.None ? String.Empty : slayer.Slayer2.ToString();

            if ( first.Length == 0 )
                return second;

            return second.Length == 0 ? first : first + " " + second;
        }

        private static int? GetExceptional( Item item )
        {
            BaseWeapon weapon = item as BaseWeapon;
            if ( weapon != null )
                return weapon.Quality == WeaponQuality.Exceptional ? 1 : 0;

            BaseArmor armor = item as BaseArmor;
            if ( armor != null )
                return armor.Quality == ArmorQuality.Exceptional ? 1 : 0;

            BaseClothing clothing = item as BaseClothing;
            if ( clothing != null )
                return clothing.Quality == ClothingQuality.Exceptional ? 1 : 0;

            BaseJewel jewel = item as BaseJewel;
            if ( jewel != null )
                return jewel.Quality == ArmorQuality.Exceptional ? 1 : 0;

            BaseInstrument instrument = item as BaseInstrument;
            if ( instrument != null )
                return instrument.Quality == InstrumentQuality.Exceptional ? 1 : 0;

            BaseQuiver quiver = item as BaseQuiver;
            return quiver == null ? (int?)null : ( quiver.Quality == ClothingQuality.Exceptional ? 1 : 0 );
        }

        private static int? GetIdentified( Item item )
        {
            BaseWeapon weapon = item as BaseWeapon;
            if ( weapon != null )
                return weapon.Identified ? 1 : 0;

            BaseArmor armor = item as BaseArmor;
            return armor == null ? (int?)null : ( armor.Identified ? 1 : 0 );
        }
    }
}
