using System;
using Server.Misc;
using Server.Mobiles;
using Server.Spells;

namespace Server.Mobiles
{
        [CorpseName( "a bull corpse" )]
        public class Bull : BaseCreature
        {
                private DateTime m_NextChargeTime;
                private DateTime m_PrepareEndTime;
                private Point3D m_ChargeDestination;
                private Direction m_ChargeDirection;
                private bool m_PreparingCharge;
                private bool m_Charging;
                private double m_OriginalActiveSpeed;
                private double m_OriginalPassiveSpeed;

                [Constructable]
                public Bull() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
                {
                        if ( Utility.RandomDouble() > 0.85 )
                        {
                                Name = "a greater bull";
                                Body = Utility.RandomList( 0xE8, 0xE9 );
                                BaseSoundID = 0x64;

                                if ( 0.5 >= Utility.RandomDouble() )
                                        Hue = 0x901;

                                SetStr( 77, 211 );
                                SetDex( 56, 175 );
                                SetInt( 47, 75 );

                                SetHits( 50, 124 );
                                SetMana( 0 );

                                SetDamage( 4, 15 );

                                SetDamageType( ResistanceType.Physical, 100 );

                                SetResistance( ResistanceType.Physical, 25, 40 );
                                SetResistance( ResistanceType.Cold, 10, 15 );

                                SetSkill( SkillName.MagicResist, 17.6, 25.0 );
                                SetSkill( SkillName.Tactics, 67.6, 85.0 );
                                SetSkill( SkillName.Wrestling, 40.1, 57.5 );

                                Fame = 900;
                                Karma = 0;

                                VirtualArmor = 28;

                                Tamable = true;
                                ControlSlots = 1;
                                MinTameSkill = 79.1;
                        }
                        else
                        {
                                Name = "a bull";
                                Body = Utility.RandomList( 0xE8, 0xE9 );
                                BaseSoundID = 0x64;

                                if ( 0.5 >= Utility.RandomDouble() )
                                        Hue = 0x901;

                                SetStr( 77, 111 );
                                SetDex( 56, 75 );
                                SetInt( 47, 75 );

                                SetHits( 50, 64 );
                                SetMana( 0 );

                                SetDamage( 4, 9 );

                                SetDamageType( ResistanceType.Physical, 100 );

                                SetResistance( ResistanceType.Physical, 25, 30 );
                                SetResistance( ResistanceType.Cold, 10, 15 );

                                SetSkill( SkillName.MagicResist, 17.6, 25.0 );
                                SetSkill( SkillName.Tactics, 67.6, 85.0 );
                                SetSkill( SkillName.Wrestling, 40.1, 57.5 );

                                Fame = 600;
                                Karma = 0;

                                VirtualArmor = 28;

                                Tamable = true;
                                ControlSlots = 1;
                                MinTameSkill = 71.1;
                        }

                        m_OriginalActiveSpeed = ActiveSpeed;
                        m_OriginalPassiveSpeed = PassiveSpeed;
                        m_NextChargeTime = DateTime.UtcNow + TimeSpan.FromSeconds( Utility.RandomMinMax( 6, 10 ) );
                }

                public override int Meat{ get{ return 10; } }
                public override int Hides{ get{ return 15; } }
                public override int Furs{ get{ return Utility.RandomList( 0, 0, 0, 8 ); } }
                public override FurType FurType{ get{ return FurType.Regular; } }
                public override FoodType FavoriteFood{ get{ return FoodType.GrainsAndHay; } }
                public override PackInstinct PackInstinct{ get{ return PackInstinct.Bull; } }

                public override void OnThink()
                {
                        base.OnThink();

                        if ( Deleted )
                                return;

                        if ( m_Charging )
                        {
                                HandleCharge();
                                return;
                        }

                        if ( m_PreparingCharge )
                        {
                                if ( DateTime.UtcNow >= m_PrepareEndTime )
                                        BeginCharge();

                                return;
                        }

                        if ( DateTime.UtcNow < m_NextChargeTime )
                                return;

                        Mobile target = Combatant;

                        if ( target == null || target.Deleted || !Alive || !target.Alive || Map != target.Map )
                                return;

                        if ( !InRange( target, 8 ) || !CanSee( target ) || !CanBeHarmful( target ) )
                                return;

                        if ( !HasClearPath( target ) )
                        {
                                m_NextChargeTime = DateTime.UtcNow + TimeSpan.FromSeconds( Utility.RandomMinMax( 5, 8 ) );
                                return;
                        }

                        PrepareCharge( target );
                }

                private void PrepareCharge( Mobile target )
                {
                        m_PreparingCharge = true;
                        m_ChargeDestination = target.Location;
                        m_ChargeDirection = GetDirectionTo( m_ChargeDestination );
                        m_PrepareEndTime = DateTime.UtcNow + TimeSpan.FromSeconds( Utility.RandomDouble() + 1.0 );

                        Animate( 5, 5, 1, true, false, 0 );
                        PlaySound( 0x64 );
                        PublicOverheadMessage( Network.MessageType.Regular, 0x3B2, false, "The bull paws at the ground and prepares to charge!" );
                }

                private void BeginCharge()
                {
                        m_PreparingCharge = false;
                        m_Charging = true;

                        ActiveSpeed = Math.Max( 0.1, m_OriginalActiveSpeed * 0.5 );
                        PassiveSpeed = Math.Max( 0.1, m_OriginalPassiveSpeed * 0.5 );

                        RevealingAction();
                        Warmode = true;
                }

                private void StopCharge()
                {
                        m_Charging = false;
                        m_PreparingCharge = false;
                        ActiveSpeed = m_OriginalActiveSpeed;
                        PassiveSpeed = m_OriginalPassiveSpeed;
                        m_NextChargeTime = DateTime.UtcNow + TimeSpan.FromSeconds( Utility.RandomMinMax( 10, 16 ) );
                }

                private void HandleCharge()
                {
                        Mobile target = Combatant;

                        if ( target == null || target.Deleted || !Alive || !target.Alive || Map != target.Map )
                        {
                                StopCharge();
                                return;
                        }

                        if ( Location == m_ChargeDestination )
                        {
                                StopCharge();
                                return;
                        }

                        if ( !Move( m_ChargeDirection ) )
                        {
                                StopCharge();
                                return;
                        }

                        if ( InRange( target, 1 ) )
                        {
                                ChargeHit( target );
                                StopCharge();
                        }
                }

                private void ChargeHit( Mobile target )
                {
                        int baseDamage = Utility.RandomMinMax( 8, 12 ) + (int)(RawStr * 0.25);
                        int bonusDamage = Utility.RandomMinMax( 6, 10 );

                        AOS.Damage( target, this, baseDamage + bonusDamage, 100, 0, 0, 0, 0 );
                        target.FixedParticles( 0x3728, 10, 15, 9909, EffectLayer.Waist );
                        target.PlaySound( target.Female ? 0x32D : 0x437 );

                        Ability.SlideAway( target, Location, 2 );
                        target.Paralyze( TimeSpan.FromSeconds( Utility.RandomDouble() * 1.5 + 1.5 ) );

                        PublicOverheadMessage( Network.MessageType.Regular, 0x3B2, false, "The bull slams into its target!" );
                }

                private bool HasClearPath( Mobile target )
                {
                        Map map = Map;

                        if ( map == null )
                                return false;

                        Direction direction = GetDirectionTo( target );
                        int steps = Math.Min( 10, (int)GetDistanceToSqrt( target ) + 1 );

                        Point3D current = Location;

                        for ( int i = 0; i < steps; i++ )
                        {
                                int x = current.X + GetDirectionOffsetX( direction );
                                int y = current.Y + GetDirectionOffsetY( direction );
                                int z = map.GetAverageZ( x, y );

                                if ( !map.CanSpawnMobile( x, y, z ) || SpellHelper.CheckMulti( new Point3D( x, y, z ), map ) )
                                        return false;

                                current = new Point3D( x, y, z );

                                if ( current == target.Location )
                                        break;
                        }

                        return InLOS( target );
                }

                private static int GetDirectionOffsetX( Direction d )
                {
                        switch ( d )
                        {
                                case Direction.North:
                                case Direction.Up:
                                        return 0;
                                case Direction.Right:
                                case Direction.East:
                                case Direction.Down:
                                        return 1;
                                case Direction.Left:
                                case Direction.West:
                                        return -1;
                                default:
                                        return 0;
                        }
                }

                private static int GetDirectionOffsetY( Direction d )
                {
                        switch ( d )
                        {
                                case Direction.North:
                                case Direction.Up:
                                        return -1;
                                case Direction.Down:
                                case Direction.Left:
                                case Direction.South:
                                        return 1;
                                default:
                                        return 0;
                        }
                }

                public Bull(Serial serial) : base(serial)
                {
                }

                public override void Serialize(GenericWriter writer)
                {
                        base.Serialize(writer);

                        writer.Write((int) 0);
                }

                public override void Deserialize(GenericReader reader)
                {
                        base.Deserialize(reader);

                        int version = reader.ReadInt();
                }
        }
}
