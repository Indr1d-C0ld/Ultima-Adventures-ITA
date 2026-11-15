using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using Server.Movement;

namespace Server.Items
{
        public class EarthStrike : WeaponAbility
        {
                private const int MaxResistStacks = 3;
                private const int ResistPenaltyPerStack = -10;
                private static readonly TimeSpan ResistDuration = TimeSpan.FromSeconds( 10.0 );
                private static readonly Dictionary<Mobile, EarthStrikeResistDebuff> m_ResistDebuffs = new Dictionary<Mobile, EarthStrikeResistDebuff>();
                private static readonly Dictionary<Mobile, int> m_HitCounts = new Dictionary<Mobile, int>();
                private static readonly Dictionary<Mobile, Direction> m_LastFacing = new Dictionary<Mobile, Direction>();

                public EarthStrike()
                {
                }

                public override int BaseMana{ get{ return 20; } }

                public override void OnHit( Mobile attacker, Mobile defender, int damage )
                {
                        if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
                                return;

                        ClearCurrentAbility( attacker );
                        attacker.SendMessage( "You strike them with extreme physical force!" );
                        defender.SendMessage( "You were struck with extreme physical force!" );

                        BaseWeapon weapon = attacker.Weapon as BaseWeapon;
                        if ( weapon == null )
                                return;

                        Skill skill = attacker.Skills[weapon.Skill];
                        double reqSkill = GetRequiredSkill( attacker );
                        double skilltouse = 0.0;

                        if ( skill != null )
                                skilltouse = skill.Value;

                        if ( weapon.WeaponAttributes.UseBestSkill > 0 )
                        {
                                double skilltouse2 = 0.0;

                                if ( attacker.Skills[SkillName.Swords].Value >= reqSkill )
                                        skilltouse2 = attacker.Skills[SkillName.Swords].Value;

                                if ( attacker.Skills[SkillName.Macing].Value >= reqSkill && attacker.Skills[SkillName.Macing].Value > skilltouse2 )
                                        skilltouse2 = attacker.Skills[SkillName.Macing].Value;

                                if ( attacker.Skills[SkillName.Fencing].Value >= reqSkill && attacker.Skills[SkillName.Fencing].Value > skilltouse2 )
                                        skilltouse2 = attacker.Skills[SkillName.Fencing].Value;

                                if ( attacker.Skills[SkillName.Lumberjacking].Value >= reqSkill && attacker.Skills[SkillName.Lumberjacking].Value > skilltouse2 )
                                        skilltouse2 = attacker.Skills[SkillName.Lumberjacking].Value;

                                if ( skilltouse2 > skilltouse )
                                        skilltouse = skilltouse2;
                        }

                        int todam = (int)( skilltouse / 20 );
                        int baseDamage = Utility.RandomMinMax( 15, 35 ) + todam;
                        AOS.Damage( defender, attacker, baseDamage, true, 100, 0, 0, 0, 0 );

                        ApplyResistDebuff( attacker, defender );
                        HandleShockwave( attacker, defender, baseDamage );

                        base.OnHit( attacker, defender, damage );
                }

                private static void ApplyResistDebuff( Mobile attacker, Mobile defender )
                {
                        if ( defender == null || defender.Deleted )
                                return;

                        EarthStrikeResistDebuff info;

                        if ( !m_ResistDebuffs.TryGetValue( defender, out info ) )
                        {
                                info = new EarthStrikeResistDebuff( defender );
                                m_ResistDebuffs[defender] = info;
                        }

                        info.AddStack();
                        BuffInfo.AddBuff( defender, new BuffInfo( BuffIcon.EarthStrike, 1151476, 1151477, ResistDuration, defender, info.StackCount.ToString() ) );

                        if ( attacker != null )
                                attacker.SendMessage( "Their physical defenses crumble ({0}/3)!", info.StackCount );

                        defender.SendMessage( "Your physical defenses crumble ({0}/3)!", info.StackCount );
                }

                private static void HandleShockwave( Mobile attacker, Mobile defender, int baseDamage )
                {
                        if ( attacker == null )
                                return;

                        Direction facing;

                        if ( defender != null )
                        {
                                facing = attacker.GetDirectionTo( defender ) & Direction.Mask;
                                m_LastFacing[attacker] = facing;
                        }
                        else if ( !m_LastFacing.TryGetValue( attacker, out facing ) )
                        {
                                facing = attacker.Direction & Direction.Mask;
                                m_LastFacing[attacker] = facing;
                        }

                        int count;

                        if ( !m_HitCounts.TryGetValue( attacker, out count ) )
                                count = 0;

                        count++;

                        if ( count >= 3 )
                        {
                                count = 0;
                                Direction shockwaveFacing;

                                if ( !m_LastFacing.TryGetValue( attacker, out shockwaveFacing ) )
                                        shockwaveFacing = attacker.Direction & Direction.Mask;

                                EmitShockwave( attacker, defender, shockwaveFacing, baseDamage );
                        }

                        m_HitCounts[attacker] = count;
                }

                private static void EmitShockwave( Mobile attacker, Mobile primaryTarget, Direction facing, int baseDamage )
                {
                        Map map = attacker.Map;

                        if ( map == null )
                                return;

                        List<Mobile> targets = new List<Mobile>();

                        IPooledEnumerable<Mobile> eable = attacker.GetMobilesInRange( 7 );

                        try
                        {
                                foreach ( Mobile m in eable )
                                {
                                        if ( m == null || m == attacker || m.Deleted )
                                                continue;

                                        if ( !attacker.CanBeHarmful( m, false ) )
                                                continue;

                                        if ( !IsInFront( facing, attacker, m ) )
                                                continue;

                                        targets.Add( m );
                                }
                        }
                        finally
                        {
                                eable.Free();
                        }

                        attacker.SendMessage( "You slam the earth and unleash a shockwave!" );
                        Effects.PlaySound( attacker.Location, map, 0x2F3 );
                        Effects.SendLocationParticles( EffectItem.Create( attacker.Location, map, EffectItem.DefaultDuration ), 0x3728, 10, 15, 0, 0, 5029, 0 );

                        if ( targets.Count == 0 )
                                return;

                        int shockwaveDamage = Math.Max( 1, ( baseDamage + 1 ) / 2 );

                        foreach ( Mobile target in targets )
                        {
                                attacker.DoHarmful( target );

                                AOS.Damage( target, attacker, shockwaveDamage, true, 100, 0, 0, 0, 0 );

                                EarthStrikeShockwaveDebuff.Apply( target );

                                target.FixedParticles( 0x36B0, 1, 14, 0x26B8, 0, 0, EffectLayer.Head );
                                target.SendMessage( "Dirt and sand blast into your eyes!" );

                                if ( target != primaryTarget )
                                {
                                        int distance = Utility.RandomMinMax( 2, 3 ) + 1;
                                        TryPush( target, facing, distance );
                                }
                        }
                }

                private static bool IsInFront( Direction facing, Mobile attacker, Mobile target )
                {
                        int fx, fy;

                        if ( !TryGetDirectionVector( facing, out fx, out fy ) )
                                return false;

                        int tx = target.X - attacker.X;
                        int ty = target.Y - attacker.Y;

                        if ( tx == 0 && ty == 0 )
                                return true;

                        int dot = ( fx * tx ) + ( fy * ty );

                        if ( dot <= 0 )
                                return false;

                        int facingMagSq = ( fx * fx ) + ( fy * fy );
                        int targetMagSq = ( tx * tx ) + ( ty * ty );

                        if ( facingMagSq == 0 || targetMagSq == 0 )
                                return false;

                        // cos(theta)^2 >= cos(45°)^2 == 0.5 to stay within a 90° cone in front of the attacker.
                        // Multiply to avoid floating point operations: 2 * dot^2 >= facingMagSq * targetMagSq.
                        long lhs = 2L * dot * dot;
                        long rhs = (long)facingMagSq * targetMagSq;

                        return lhs >= rhs;
                }

                private static bool TryGetDirectionVector( Direction facing, out int x, out int y )
                {
                        switch ( facing & Direction.Mask )
                        {
                                case Direction.North:
                                        x = 0;
                                        y = -1;
                                        return true;
                                case Direction.South:
                                        x = 0;
                                        y = 1;
                                        return true;
                                case Direction.West:
                                        x = -1;
                                        y = 0;
                                        return true;
                                case Direction.East:
                                        x = 1;
                                        y = 0;
                                        return true;
                                case Direction.Right:
                                        x = 1;
                                        y = -1;
                                        return true;
                                case Direction.Left:
                                        x = -1;
                                        y = 1;
                                        return true;
                                case Direction.Down:
                                        x = 1;
                                        y = 1;
                                        return true;
                                case Direction.Up:
                                        x = -1;
                                        y = -1;
                                        return true;
                        }

                        x = 0;
                        y = 0;
                        return false;
                }

                private static void TryPush( Mobile target, Direction direction, int distance )
                {
                        Map map = target.Map;

                        if ( map == null || distance <= 0 )
                                return;

                        int originalX = target.X;
                        int originalY = target.Y;
                        int originalZ = target.Z;

                        int finalX = originalX;
                        int finalY = originalY;
                        int finalZ = originalZ;

                        for ( int i = 0; i < distance; ++i )
                        {
                                int testX = finalX;
                                int testY = finalY;

                                Movement.Movement.Offset( direction, ref testX, ref testY );

                                int testZ = map.GetAverageZ( testX, testY );

                                if ( map.CanFit( testX, testY, testZ, 16, false, false, true ) )
                                {
                                        finalX = testX;
                                        finalY = testY;
                                        finalZ = testZ;
                                }
                                else
                                {
                                        break;
                                }
                        }

                        if ( finalX != originalX || finalY != originalY || finalZ != originalZ )
                                target.MoveToWorld( new Point3D( finalX, finalY, finalZ ), map );
                }

                private static void ClearResistDebuff( Mobile defender )
                {
                        if ( defender == null )
                                return;

                        EarthStrikeResistDebuff info;

                        if ( !m_ResistDebuffs.TryGetValue( defender, out info ) )
                                return;

                        info.Clear();
                        m_ResistDebuffs.Remove( defender );
                        BuffInfo.RemoveBuff( defender, BuffIcon.EarthStrike );
                        defender.SendMessage( "Your physical defenses recover." );
                }

                private class EarthStrikeResistDebuff
                {
                        private readonly Mobile m_Defender;
                        private ResistanceMod m_Mod;
                        private Timer m_Timer;

                        public int StackCount{ get; private set; }

                        public EarthStrikeResistDebuff( Mobile defender )
                        {
                                m_Defender = defender;
                        }

                        public void AddStack()
                        {
                                if ( StackCount < MaxResistStacks )
                                        ++StackCount;

                                Refresh();
                        }

                        private void Refresh()
                        {
                                if ( m_Defender == null || m_Defender.Deleted )
                                        return;

                                if ( m_Mod == null )
                                {
                                        m_Mod = new ResistanceMod( ResistanceType.Physical, ResistPenaltyPerStack * StackCount );
                                        m_Defender.AddResistanceMod( m_Mod );
                                }
                                else
                                {
                                        m_Mod.Offset = ResistPenaltyPerStack * StackCount;
                                }

                                if ( m_Timer == null )
                                {
                                        m_Timer = new InternalTimer( m_Defender );
                                        m_Timer.Start();
                                }
                                else
                                {
                                        m_Timer.Stop();
                                        m_Timer.Delay = ResistDuration;
                                        m_Timer.Start();
                                }
                        }

                        public void Clear()
                        {
                                if ( m_Timer != null )
                                {
                                        m_Timer.Stop();
                                        m_Timer = null;
                                }

                                if ( m_Mod != null )
                                {
                                        m_Defender.RemoveResistanceMod( m_Mod );
                                        m_Mod = null;
                                }
                        }

                        private class InternalTimer : Timer
                        {
                                private readonly Mobile m_Target;

                                public InternalTimer( Mobile defender ) : base( ResistDuration )
                                {
                                        m_Target = defender;
                                        Priority = TimerPriority.TwoFiftyMS;
                                }

                                protected override void OnTick()
                                {
                                        ClearResistDebuff( m_Target );
                                }
                        }
                }
        }

        public static class EarthStrikeShockwaveDebuff
        {
                private const int AttackPenalty = 20;
                private const int DefensePenalty = 20;
                private static readonly TimeSpan DebuffDuration = TimeSpan.FromSeconds( 10.0 );

                private static readonly Dictionary<Mobile, DebuffTimer> m_Table = new Dictionary<Mobile, DebuffTimer>();

                public static int AttackMalus{ get{ return AttackPenalty; } }
                public static int DefenseMalus{ get{ return DefensePenalty; } }

                public static bool IsUnderAttackEffect( Mobile m )
                {
                        return HasDebuff( m );
                }

                public static bool IsUnderDefenseEffect( Mobile m )
                {
                        return HasDebuff( m );
                }

                public static bool HasDebuff( Mobile m )
                {
                        return m != null && m_Table.ContainsKey( m );
                }

                public static void Apply( Mobile m )
                {
                        if ( m == null )
                                return;

                        DebuffTimer timer;

                        if ( m_Table.TryGetValue( m, out timer ) )
                        {
                                timer.Restart();
                        }
                        else
                        {
                                timer = new DebuffTimer( m );
                                m_Table[m] = timer;
                                timer.Start();
                                m.SendMessage( "Your vision is clouded by dirt and sand!" );
                        }
                }

                public static void Remove( Mobile m )
                {
                        if ( m == null )
                                return;

                        DebuffTimer timer;

                        if ( !m_Table.TryGetValue( m, out timer ) )
                                return;

                                timer.Stop();
                                m_Table.Remove( m );
                                BuffInfo.RemoveBuff( m, BuffIcon.EarthStrike );
                                m.SendMessage( "You blink the grit out of your eyes." );
                }

                private class DebuffTimer : Timer
                {
                        private readonly Mobile m_Target;

                        public DebuffTimer( Mobile target ) : base( DebuffDuration )
                        {
                                m_Target = target;
                                Priority = TimerPriority.TwoFiftyMS;
                        }

                        public void Restart()
                        {
                                Stop();
                                Delay = DebuffDuration;
                                Start();
                        }

                        protected override void OnTick()
                        {
                                Remove( m_Target );
                        }
                }
        }
}
