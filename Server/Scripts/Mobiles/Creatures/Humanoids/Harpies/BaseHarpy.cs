using System;
using System.Collections.Generic;
using Server.Items;
using Server.Network;
using Server.Targeting;
using Server.Spells;

namespace Server.Mobiles
{
        public abstract class BaseHarpy : BaseCreature
        {
                private DateTime m_NextPickup;
                private DateTime m_NextScreech;
                private DateTime m_WingBuffetEnd;
                private DateTime m_NextWingBuffet;
                private DateTime m_NextPeace;

                private const double ScreechChance = 0.5;
                private static readonly TimeSpan ScreechCooldown = TimeSpan.FromSeconds( 25.0 );
                private static readonly TimeSpan ScreechDuration = TimeSpan.FromSeconds( 6.0 );
                private static readonly TimeSpan WingBuffetDuration = TimeSpan.FromSeconds( 5.0 );
                private static readonly TimeSpan WingBuffetCooldown = TimeSpan.FromSeconds( 15.0 );

                private static readonly SkillName[] ScreechSkills = new SkillName[]
                {
                        SkillName.Magery,
                        SkillName.EvalInt,
                        SkillName.Necromancy,
                        SkillName.Spellweaving,
                        SkillName.Mysticism,
                        SkillName.SpiritSpeak,
                        SkillName.Focus,
                        SkillName.Archery,
                        SkillName.Throwing
                };

                private static readonly Dictionary<Mobile, Timer> m_SilencedCasters = new Dictionary<Mobile, Timer>();

                public BaseHarpy( AIType ai, FightMode fightMode, int rangePerception, int rangeFight, double activeSpeed, double passiveSpeed ) : base( ai, fightMode, rangePerception, rangeFight, activeSpeed, passiveSpeed )
                {
                }

                public BaseHarpy( Serial serial ) : base( serial )
                {
                }

                public override void OnThink()
                {
                        base.OnThink();

                        Screech();
                }

                private bool WingBuffetActive
                {
                        get{ return ( DateTime.UtcNow < m_WingBuffetEnd ); }
                }

                private void TriggerWingBuffet( Mobile attacker )
                {
                        if ( WingBuffetActive || DateTime.UtcNow < m_NextWingBuffet )
                                return;

                        m_WingBuffetEnd = DateTime.UtcNow + WingBuffetDuration;
                        m_NextWingBuffet = DateTime.UtcNow + WingBuffetCooldown;

                        PublicOverheadMessage( MessageType.Emote, 0x3B2, false, "*batters the air with a defensive wing buffet*" );
                        FixedParticles( 0x3728, 10, 15, 5042, 0 );
                        PlaySound( 916 );

                        if ( attacker != null && attacker.Alive )
                                attacker.SendMessage( "Your projectile is thrown off by the gusting wings!" );
                }

                private bool IsRangedAttacker( Mobile from )
                {
                        BaseWeapon weapon = from != null ? from.Weapon as BaseWeapon : null;
                        return ( weapon is BaseRanged );
                }

                public override void AlterMeleeDamageFrom( Mobile from, ref int damage )
                {
                        base.AlterMeleeDamageFrom( from, ref damage );

                        if ( from != null && IsRangedAttacker( from ) )
                        {
                                if ( !WingBuffetActive )
                                        TriggerWingBuffet( from );

                                if ( WingBuffetActive )
                                        damage = 0;
                        }
                }

                private void Screech()
                {
                        if ( Combatant == null || DateTime.UtcNow < m_NextScreech || Utility.RandomDouble() > ScreechChance )
                                return;

                        List<Mobile> targets = new List<Mobile>();

                        foreach ( Mobile mob in GetMobilesInRange( 6 ) )
                        {
                                if ( mob == this || !mob.Alive || mob.Blessed || mob.Hidden || !CanBeHarmful( mob ) )
                                        continue;

                                if ( !IsRangedOrCaster( mob ) )
                                        continue;

                                if ( mob.Player || (mob is BaseCreature && (((BaseCreature)mob).Controlled || ((BaseCreature)mob).Summoned)) )
                                        targets.Add( mob );
                        }

                        if ( targets.Count == 0 )
                                return;

                        m_NextScreech = DateTime.UtcNow + ScreechCooldown;

                        PublicOverheadMessage( MessageType.Regular, 0x3B2, false, "*lets out a shattering screech*" );
                        PlaySound( 916 );

                        foreach ( Mobile target in targets )
                        {
                                ApplyScreechDebuff( target );
                        }
                }

                private bool IsRangedOrCaster( Mobile mob )
                {
                        BaseWeapon weapon = mob != null ? mob.Weapon as BaseWeapon : null;
                        bool isRanged = ( weapon is BaseRanged );

                        bool isCaster = false;

                        if ( mob != null )
                        {
                                isCaster = ( mob.Spell != null ||
                                        mob.Skills[SkillName.Magery].Value >= 50.0 ||
                                        mob.Skills[SkillName.Necromancy].Value >= 50.0 ||
                                        mob.Skills[SkillName.Mysticism].Value >= 50.0 ||
                                        mob.Skills[SkillName.Spellweaving].Value >= 50.0 );
                        }

                        return ( isRanged || isCaster );
                }

                private void ApplyScreechDebuff( Mobile target )
                {
                        if ( target == null || !target.Alive || m_SilencedCasters.ContainsKey( target ) || !target.BeginAction( typeof( BaseHarpy ) ) )
                                return;

                        Spell spell = target.Spell as Spell;

                        if ( spell != null && spell.IsCasting )
                                spell.Disturb( DisturbType.Hurt, false, true );
                        target.FixedParticles( 0x3709, 1, 30, 9944, 0x48E, 2, EffectLayer.Head );
                        target.SendMessage( "The harpy's screech deafens you, ruining your aim!" );

                        if ( target is PlayerMobile )
                                BuffInfo.AddBuff( target, new BuffInfo( BuffIcon.HarpyScreech, 1151508, 1151509, ScreechDuration, target ) );

                        foreach ( SkillName skill in ScreechSkills )
                        {
                                Skill skillObj = target.Skills[ skill ];
                                double offset = Math.Max( 15.0, skillObj.Base * 0.30 ) * -1;

                                target.AddSkillMod( new TimedSkillMod( skill, true, offset, ScreechDuration ) );
                        }

                        Timer timer = Timer.DelayCall( ScreechDuration, delegate
                        {
                                if ( target != null )
                                {
                                        BuffInfo.RemoveBuff( target, BuffIcon.HarpyScreech );
                                        target.EndAction( typeof( BaseHarpy ) );
                                        m_SilencedCasters.Remove( target );
                                }
                        } );

                        m_SilencedCasters[target] = timer;
                }

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );

                        writer.Write( (int)(-1) ); // marker to distinguish BaseHarpy data from legacy saves
                        writer.Write( (int) 1 );
                        writer.Write( m_NextPickup );
                        writer.Write( m_NextPeace );
                        writer.Write( m_NextScreech );
                        writer.Write( m_WingBuffetEnd );
                        writer.Write( m_NextWingBuffet );
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );

                        int peek = reader.PeekInt();

                        if ( peek == -1 )
                        {
                                reader.ReadInt(); // consume marker

                                int version = reader.ReadInt();

                                if ( version >= 1 )
                                {
                                        m_NextPickup = reader.ReadDateTime();
                                        m_NextPeace = reader.ReadDateTime();
                                        m_NextScreech = reader.ReadDateTime();
                                        m_WingBuffetEnd = reader.ReadDateTime();
                                        m_NextWingBuffet = reader.ReadDateTime();
                                }
                        }
                        else if ( peek >= 1 )
                        {
                                int version = reader.ReadInt();

                                if ( version >= 1 )
                                {
                                        m_NextPickup = reader.ReadDateTime();
                                        m_NextPeace = reader.ReadDateTime();
                                        m_NextScreech = reader.ReadDateTime();
                                        m_WingBuffetEnd = reader.ReadDateTime();
                                        m_NextWingBuffet = reader.ReadDateTime();
                                }
                        }
                        // legacy saves without BaseHarpy data fall through without consuming the next int,
                        // leaving the derived class version intact.
                }
        }
}
