using System;
using Server;
using Server.Mobiles;

namespace Server.Mobiles
{
        [CorpseName( "a llama corpse" )]
        public class Llama : BaseCreature
        {
                private static readonly TimeSpan SpitCooldown = TimeSpan.FromSeconds( 15.0 );
                private DateTime m_NextSpit;

                [Constructable]
                public Llama() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
                {
                        Name = "a llama";
                        Body = 0xDC;
                        BaseSoundID = 0x3F3;

                        SetStr( 21, 49 );
                        SetDex( 36, 55 );
                        SetInt( 16, 30 );

                        SetHits( 15, 27 );
                        SetMana( 0 );

                        SetDamage( 3, 5 );

                        SetDamageType( ResistanceType.Physical, 100 );

                        SetResistance( ResistanceType.Physical, 15, 20 );

                        SetSkill( SkillName.MagicResist, 15.1, 20.0 );
                        SetSkill( SkillName.Tactics, 19.2, 29.0 );
                        SetSkill( SkillName.Wrestling, 19.2, 29.0 );

                        Fame = 300;
                        Karma = 0;

                        VirtualArmor = 16;

                        Tamable = true;
                        ControlSlots = 1;
                        MinTameSkill = 35.1;

                        m_NextSpit = DateTime.UtcNow;
                }

                public override int Meat{ get{ return 1; } }
                public override int Hides{ get{ return 12; } }
                public override int Furs{ get{ return Utility.RandomList( 0, 0, 0, 6 ); } }
                public override FurType FurType{ get{ return FurType.Regular; } }
                public override FoodType FavoriteFood{ get{ return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

                public override void OnActionCombat()
                {
                        base.OnActionCombat();

                        Mobile combatant = Combatant;

                        if ( combatant == null || combatant.Deleted || combatant.Map != Map || !InRange( combatant, 8 ) || !CanBeHarmful( combatant ) || !InLOS( combatant ) )
                                return;

                        if ( DateTime.UtcNow < m_NextSpit )
                                return;

                        if ( Utility.RandomDouble() <= 0.25 )
                        {
                                SpitAt( combatant );
                                m_NextSpit = DateTime.UtcNow + SpitCooldown;
                        }
                }

                private void SpitAt( Mobile target )
                {
                        DoHarmful( target );

                        Say( "*spits*" );
                        Animate( 3, 5, 1, true, false, 0 );
                        MovingParticles( target, 0x36D4, 1, 0, false, false, 0x3F, 0, 0x3E, 1, 0, (EffectLayer)255, 0x100 );
                        target.PlaySound( Utility.RandomBool() ? 0x334 : 0x446 );
                        target.SendMessage( "You are hit by a glob of llama spit!" );

                        AOS.Damage( target, this, Utility.RandomMinMax( 2, 5 ), 100, 0, 0, 0, 0 );
                        target.Stam = Math.Max( 0, target.Stam - Utility.RandomMinMax( 4, 8 ) );
                }

                public Llama(Serial serial) : base(serial)
                {
                }

                public override void Serialize(GenericWriter writer)
                {
                        base.Serialize(writer);

                        writer.Write((int) 1);
                        writer.Write( m_NextSpit );
                }

                public override void Deserialize(GenericReader reader)
                {
                        base.Deserialize(reader);

                        int version = reader.ReadInt();

                        switch ( version )
                        {
                                case 1:
                                {
                                        m_NextSpit = reader.ReadDateTime();
                                        break;
                                }
                                case 0:
                                {
                                        m_NextSpit = DateTime.UtcNow;
                                        break;
                                }
                        }
                }
        }
}
