using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Mobiles
{
        internal static class ElementalTrailHelper
        {
                private const double DefaultDropChance = 1.0; // Always drop when cooldown allows
                private static readonly TimeSpan DefaultCooldown = TimeSpan.FromSeconds( 1.5 );

                private struct ElementalTrailProfile
                {
                        public readonly string Description;
                        public readonly int Hue;
                        public readonly bool Glow;
                        public readonly double Chance;
                        public readonly TimeSpan Cooldown;

                        public ElementalTrailProfile( string description, int hue, bool glow = false, double chance = DefaultDropChance, TimeSpan? cooldown = null )
                        {
                                Description = description;
                                Hue = hue;
                                Glow = glow;
                                Chance = chance;
                                Cooldown = cooldown ?? DefaultCooldown;
                        }
                }

                private delegate ElementalTrailProfile? TrailResolver( BaseCreature creature );

                private static readonly Dictionary<Type, TrailResolver> TypeResolvers = new Dictionary<Type, TrailResolver>
                {
                        { typeof( BloodElemental ), ResolveBloodElemental },
                        { typeof( ToxicElemental ), c => new ElementalTrailProfile( "acidic slime", 1167 ) },
                        { typeof( PoisonElemental ), c => new ElementalTrailProfile( "poisonous slime", 1167 ) },
                        { typeof( WaterElemental ), c => new ElementalTrailProfile( "freezing water", 296 ) },
                        { typeof( DeepWaterElemental ), c => new ElementalTrailProfile( "deep water", 0x555 ) },
                        { typeof( SeaweedElemental ), c => new ElementalTrailProfile( "seaweed slime", 0x7D1 ) },
                        { typeof( KelpElemental ), c => new ElementalTrailProfile( "seaweed slime", 0x7D1 ) },
                        { typeof( WeedElemental ), c => new ElementalTrailProfile( "seaweed slime", 0x7D1 ) },
                        { typeof( SewageElemental ), c => new ElementalTrailProfile( "sewage sludge", 0x3F ) },
                        { typeof( MudElemental ), c => new ElementalTrailProfile( "swamp muck", 0x7D1 ) },
                        { typeof( MagmaElemental ), c => new ElementalTrailProfile( "hot magma", 0x489, true ) },
                        { typeof( LavaElemental ), c => new ElementalTrailProfile( "hot magma", 0x489, true ) },
                        { typeof( FireElemental ), c => new ElementalTrailProfile( "scorching ooze", 0x496 ) },
                        { typeof( CinderElemental ), c => new ElementalTrailProfile( "smoldering ash", 0x54F ) },
                        { typeof( SteamElemental ), c => new ElementalTrailProfile( "scalding steam", 0x489 ) },
                        { typeof( SnowElemental ), c => new ElementalTrailProfile( "icy slush", 296 ) },
                        { typeof( IceElemental ), c => new ElementalTrailProfile( "icy slush", 296 ) },
                        { typeof( IcebergElemental ), c => new ElementalTrailProfile( "icy slush", 296 ) },
                        { typeof( DriftwoodElemental ), c => new ElementalTrailProfile( "driftwood mulch", 0x75E ) },
                        { typeof( ForestElemental ), c => new ElementalTrailProfile( "forest sap", 0x7D1 ) },
                        { typeof( GraveDustElemental ), c => new ElementalTrailProfile( "grave dust", 0x835 ) },
                        { typeof( DustElemental ), c => new ElementalTrailProfile( "dusty residue", 0xB8F ) },
                        { typeof( LightningElemental ), c => new ElementalTrailProfile( "crackling energy", 0xB97, true ) },
                        { typeof( ElectricalElemental ), c => new ElementalTrailProfile( "crackling energy", 0xB97, true ) },
                        { typeof( MeteorElemental ), c => new ElementalTrailProfile( "stardust", 0x550, true ) },
                        { typeof( AirElemental ), c => new ElementalTrailProfile( "gust wake", 0x47E ) },
                        { typeof( AnyElemental ), ResolveGenericElemental },
                        { typeof( AnyGemElemental ), c => new ElementalTrailProfile( "gem dust", 0x482, true ) },
                        { typeof( GemElemental ), c => new ElementalTrailProfile( "gem dust", 0x482, true ) },
                        { typeof( CaddelliteElemental ), c => new ElementalTrailProfile( "caddellite shards", 0xB93, true ) },
                        { typeof( CrystalElemental ), c => new ElementalTrailProfile( "crystal shards", 0x47E, true ) },
                        { typeof( DilithiumElemental ), c => new ElementalTrailProfile( "dilithium shards", 0x481, true ) },
                        { typeof( GarnetElemental ), c => new ElementalTrailProfile( "garnet dust", 0x486, true ) },
                        { typeof( ObsidianElemental ), c => new ElementalTrailProfile( "obsidian shards", 0x455 ) },
                        { typeof( QuartzElemental ), c => new ElementalTrailProfile( "quartz shards", 0x47E, true ) },
                        { typeof( SapphireElemental ), c => new ElementalTrailProfile( "sapphire dust", 0x5B6, true ) },
                        { typeof( SilverElemental ), c => new ElementalTrailProfile( "liquid silver", 0xB37, true ) },
                        { typeof( SpinelElemental ), c => new ElementalTrailProfile( "spinel dust", 0x8A5, true ) },
                        { typeof( StarRubyElemental ), c => new ElementalTrailProfile( "ruby dust", 0x66D, true ) },
                        { typeof( TopazElemental ), c => new ElementalTrailProfile( "topaz dust", 0xB73, true ) },
                        { typeof( TrilithiumElemental ), c => new ElementalTrailProfile( "trilithium shards", 0x48E, true ) },
                        { typeof( XormiteElemental ), c => new ElementalTrailProfile( "xormite shards", 0xB93, true ) },
                        { typeof( Vulcrum ), c => new ElementalTrailProfile( "hot magma", 0x489, true, cooldown: TimeSpan.FromSeconds( 2.0 ) ) },
                };

                public static void TryLeaveElementalTrail( BaseCreature creature, ref DateTime nextAllowedTime )
                {
                        if ( creature == null || creature.Deleted )
                                return;

                        Map map = creature.Map;

                        if ( map == null || map == Map.Internal )
                                return;

                        ElementalTrailProfile? profile = ResolveProfile( creature );

                        if ( profile == null )
                                return;

                        DateTime now = DateTime.UtcNow;

                        if ( now < nextAllowedTime )
                                return;

                        ElementalTrailProfile info = profile.Value;

                        if ( info.Chance < 1.0 && Utility.RandomDouble() > info.Chance )
                        {
                                nextAllowedTime = now + TimeSpan.FromSeconds( 1.0 );
                                return;
                        }

                        if ( MonsterSplatter.TooMuchSplatter( creature ) )
                        {
                                nextAllowedTime = now + TimeSpan.FromSeconds( 2.0 );
                                return;
                        }

                        if ( HasExistingTrailAt( creature, info.Description ) )
                        {
                                nextAllowedTime = now + TimeSpan.FromSeconds( 1.0 );
                                return;
                        }

                        MonsterSplatter puddle = new MonsterSplatter( creature )
                        {
                                Name = info.Description,
                                Hue = info.Hue,
                                Weight = info.Glow ? 2.0 : 1.0
                        };

                        puddle.MoveToWorld( creature.Location, map );

                        if ( info.Glow )
                        {
                                StrangeGlow glow = new StrangeGlow
                                {
                                        Name = info.Description,
                                        Hue = info.Hue
                                };

                                glow.MoveToWorld( creature.Location, map );
                        }

                        nextAllowedTime = now + info.Cooldown;
                }

                private static ElementalTrailProfile? ResolveProfile( BaseCreature creature )
                {
                        Type current = creature.GetType();

                        while ( current != null && typeof( BaseCreature ).IsAssignableFrom( current ) )
                        {
                                TrailResolver resolver;

                                if ( TypeResolvers.TryGetValue( current, out resolver ) )
                                {
                                        ElementalTrailProfile? profile = resolver( creature );

                                        if ( profile != null )
                                                return profile;
                                }

                                current = current.BaseType;
                        }

                        if ( creature.GetType().Name.IndexOf( "Elemental", StringComparison.OrdinalIgnoreCase ) < 0 )
                                return null;

                        return ResolveByName( creature );
                }

                private static ElementalTrailProfile? ResolveBloodElemental( BaseCreature creature )
                {
                        string name = creature.Name ?? string.Empty;
                        string lowered = name.ToLower();

                        if ( lowered.Contains( "coolant" ) )
                                return new ElementalTrailProfile( "engine coolant", 0xB73 );

                        if ( lowered.Contains( "contaminated" ) )
                                return new ElementalTrailProfile( "contamination", 0xBAD );

                        return new ElementalTrailProfile( "thick blood", 0x485 );
                }

                private static ElementalTrailProfile? ResolveGenericElemental( BaseCreature creature )
                {
                        string name = creature.Name ?? string.Empty;

                        if ( name.IndexOf( "blood", StringComparison.OrdinalIgnoreCase ) >= 0 )
                                return new ElementalTrailProfile( "thick blood", 0x485 );

                        if ( name.IndexOf( "acid", StringComparison.OrdinalIgnoreCase ) >= 0 || name.IndexOf( "toxic", StringComparison.OrdinalIgnoreCase ) >= 0 )
                                return new ElementalTrailProfile( "acidic slime", 1167 );

                        if ( name.IndexOf( "poison", StringComparison.OrdinalIgnoreCase ) >= 0 )
                                return new ElementalTrailProfile( "poisonous slime", 1167 );

                        if ( name.IndexOf( "fire", StringComparison.OrdinalIgnoreCase ) >= 0 || name.IndexOf( "flame", StringComparison.OrdinalIgnoreCase ) >= 0 )
                                return new ElementalTrailProfile( "scorching ooze", 0x496 );

                        if ( name.IndexOf( "water", StringComparison.OrdinalIgnoreCase ) >= 0 )
                                return new ElementalTrailProfile( "freezing water", 296 );

                        if ( name.IndexOf( "ice", StringComparison.OrdinalIgnoreCase ) >= 0 )
                                return new ElementalTrailProfile( "icy slush", 296 );

                        if ( name.IndexOf( "mud", StringComparison.OrdinalIgnoreCase ) >= 0 || name.IndexOf( "earth", StringComparison.OrdinalIgnoreCase ) >= 0 )
                                return new ElementalTrailProfile( "swamp muck", 0x7D1 );

                        if ( name.IndexOf( "dust", StringComparison.OrdinalIgnoreCase ) >= 0 )
                                return new ElementalTrailProfile( "dusty residue", 0xB8F );

                        if ( name.IndexOf( "storm", StringComparison.OrdinalIgnoreCase ) >= 0 || name.IndexOf( "lightning", StringComparison.OrdinalIgnoreCase ) >= 0 )
                                return new ElementalTrailProfile( "crackling energy", 0xB97, true );

                        if ( name.IndexOf( "lava", StringComparison.OrdinalIgnoreCase ) >= 0 || name.IndexOf( "magma", StringComparison.OrdinalIgnoreCase ) >= 0 )
                                return new ElementalTrailProfile( "hot magma", 0x489, true );

                        return new ElementalTrailProfile( "elemental residue", creature.Hue != 0 ? creature.Hue : 0x497 );
                }

                private static ElementalTrailProfile? ResolveByName( BaseCreature creature )
                {
                        return ResolveGenericElemental( creature );
                }

                private static bool HasExistingTrailAt( BaseCreature creature, string description )
                {
                        IPooledEnumerable items = creature.Map.GetItemsInRange( creature.Location, 0 );

                        try
                        {
                                foreach ( Item item in items )
                                {
                                        MonsterSplatter splatter = item as MonsterSplatter;

                                        if ( splatter != null && splatter.Name == description )
                                                return true;
                                }
                        }
                        finally
                        {
                                items.Free();
                        }

                        return false;
                }
        }
}
