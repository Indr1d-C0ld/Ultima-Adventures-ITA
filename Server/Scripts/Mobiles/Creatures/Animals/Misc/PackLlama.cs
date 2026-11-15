using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Items;
using Server.ContextMenus;

namespace Server.Mobiles
{
        [CorpseName("a llama corpse")]
        public class PackLlama : BaseCreature
        {
                private static readonly TimeSpan SpitCooldown = TimeSpan.FromSeconds( 15.0 );
                private DateTime m_NextSpit;

                [Constructable]
                public PackLlama() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
                {
                        Name = "a pack llama";
                        Body = 292;
                        BaseSoundID = 0x3F3;

                        SetStr( 52, 80 );
                        SetDex( 36, 55 );
                        SetInt( 16, 30 );

                        SetHits( 50 );
                        SetStam( 86, 105 );
                        SetMana( 0 );

                        SetDamage( 2, 6 );

                        SetDamageType( ResistanceType.Physical, 100 );

                        SetResistance( ResistanceType.Physical, 25, 35 );
                        SetResistance( ResistanceType.Fire, 10, 15 );
                        SetResistance( ResistanceType.Cold, 10, 15 );
                        SetResistance( ResistanceType.Poison, 10, 15 );
                        SetResistance( ResistanceType.Energy, 10, 15 );

                        SetSkill( SkillName.MagicResist, 15.1, 20.0 );
                        SetSkill( SkillName.Tactics, 19.2, 29.0 );
                        SetSkill( SkillName.Wrestling, 19.2, 29.0 );

                        Fame = 0;
                        Karma = 200;

                        VirtualArmor = 16;

                        Tamable = true;
                        ControlSlots = 1;
                        MinTameSkill = 11.1;

                        Container pack = Backpack;

                        if ( pack != null )
                                pack.Delete();

                        pack = new StrongBackpack();
                        pack.Movable = false;

                        AddItem( pack );

                        m_NextSpit = DateTime.UtcNow;
                }

                public override int Meat{ get{ return 1; } }
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

                public PackLlama( Serial serial ) : base( serial )
                {
                }

                #region Pack Animal Methods
                public override bool OnBeforeDeath()
                {
                        if ( !base.OnBeforeDeath() )
                                return false;

                        PackAnimal.CombineBackpacks( this );

                        return true;
                }

                public override DeathMoveResult GetInventoryMoveResultFor( Item item )
                {
                        return DeathMoveResult.MoveToCorpse;
                }

                public override bool IsSnoop( Mobile from )
                {
                        if ( PackAnimal.CheckAccess( this, from ) )
                                return false;

                        return base.IsSnoop( from );
                }

                public override bool OnDragDrop( Mobile from, Item item )
                {
                        if ( CheckFeed( from, item ) )
                                return true;

                        if ( PackAnimal.CheckAccess( this, from ) )
                        {
                                AddToBackpack( item );
                                return true;
                        }

                        return base.OnDragDrop( from, item );
                }

                public override bool CheckNonlocalDrop( Mobile from, Item item, Item target )
                {
                        return PackAnimal.CheckAccess( this, from );
                }

                public override bool CheckNonlocalLift( Mobile from, Item item )
                {
                        return PackAnimal.CheckAccess( this, from );
                }

                public override void OnDoubleClick( Mobile from )
                {
                        PackAnimal.TryPackOpen( this, from );
                }

                public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list )
                {
                        base.GetContextMenuEntries( from, list );

                        PackAnimal.GetContextMenuEntries( this, from, list );
                }
                #endregion

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );

                        writer.Write( (int) 1 );
                        writer.Write( m_NextSpit );
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );

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
