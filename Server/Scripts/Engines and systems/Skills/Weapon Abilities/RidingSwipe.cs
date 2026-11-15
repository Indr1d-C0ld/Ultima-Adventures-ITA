using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Movement;
using Server.Spells;
using Server.Network;

namespace Server.Items
{
        /// <summary>
        /// A devastating strike that deals increased damage after charging forward and cleaves
        /// enemies positioned behind the primary target.
        /// </summary>
	public class RidingSwipe : WeaponAbility
	{
		private const int ChargeDistanceRequirement = 5;
		private const double ChargeDamageBonus = 1.5;
                private const int CleaveRange = 5;
                private const double CleaveCosineThreshold = 0.5; // ~120 degree cone behind the target
                private const int CleaveShockwaveEffect = 0x37C4; // subtler dust ring
                private const int CleaveShockwaveHue = 0x835;
                private const int CleaveShockwaveSound = 0x11C;

                private class ChargeInfo
                {
                        public Point3D Start { get; private set; }
                        public Point3D LastLocation { get; private set; }
                        public Map Map { get; private set; }
                        public int TilesMoved { get; private set; }
                        public int LastDeltaX { get; private set; }
                        public int LastDeltaY { get; private set; }
                        public bool ChargeReadyAnnounced { get; set; }

                        public ChargeInfo( Point3D location, Map map )
                        {
                                Reset( location, map );
                        }

                        public void Reset( Point3D location, Map map )
                        {
                                Start = location;
                                LastLocation = location;
                                Map = map;
                                TilesMoved = 0;
                                LastDeltaX = 0;
                                LastDeltaY = 0;
                                ChargeReadyAnnounced = false;
                        }

                        public void RecordMovement( Point3D previousLocation, Point3D newLocation, Map map )
                        {
                                int deltaX = newLocation.X - previousLocation.X;
                                int deltaY = newLocation.Y - previousLocation.Y;

                                if ( deltaX == 0 && deltaY == 0 )
                                        return;

                                TilesMoved += Math.Max( Math.Abs( deltaX ), Math.Abs( deltaY ) );
                                LastLocation = newLocation;
                                Map = map;
                                LastDeltaX = deltaX;
                                LastDeltaY = deltaY;
                        }
                }

                private static readonly Dictionary<Mobile, ChargeInfo> m_ChargeStates = new Dictionary<Mobile, ChargeInfo>();

                static RidingSwipe()
                {
                        EventSink.Movement += EventSink_Movement;
                }

		public RidingSwipe()
		{
		}

		public override int BaseMana { get { return 30; } }

		public override bool RequiresSE { get { return true; } }

		public override bool CheckSkills( Mobile from )
		{
			return base.CheckSkills( from );
		}

                public override bool Validate( Mobile from )
                {
                        bool valid = base.Validate( from );

                        if ( !valid )
                        {
                                ResetCharge( from );
                                return false;
                        }

                        if ( from != null )
                        {
                                WeaponAbility current = WeaponAbility.Table[from] as WeaponAbility;

                                if ( current != this || !m_ChargeStates.ContainsKey( from ) )
                                {
                                        Map map = from.Map;

                                        if ( map == null )
                                        {
                                                m_ChargeStates.Remove( from );
                                        }
                                        else
                                        {
                                                m_ChargeStates[from] = new ChargeInfo( from.Location, map );
                                        }
                                }
                        }

                        return true;
                }

                public override void OnHit( Mobile attacker, Mobile defender, int damage )
                {
                        if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
                        {
                                ResetCharge( attacker );
                                return;
                        }

			ClearCurrentAbility( attacker );

                        bool movedEnough = HasMetChargeRequirement( attacker );

                        int amount = GetBaseDamage( attacker );
                        int finalDamage = GetChargedDamage( amount, movedEnough );

                        Map defenderMap = defender != null ? defender.Map : null;
                        Point3D defenderLocation = defender != null ? defender.Location : new Point3D();
                        int defenderZ = defender != null ? defender.Z : 0;
                        Serial defenderSerial = defender != null ? defender.Serial : Serial.MinusOne;
                        Point3D attackerLocation = attacker != null ? attacker.Location : new Point3D();

                        double directionX;
                        double directionY;
                        bool hasDirection = TryGetChargeDirection( attacker, defender, attackerLocation, defenderLocation, out directionX, out directionY );

                        attacker.DoHarmful( defender );
                        AOS.Damage( defender, attacker, finalDamage, 100, 0, 0, 0, 0 );

                        if ( movedEnough && hasDirection )
                        {
                                Mobile cleaveAttacker = attacker;
                                Serial cleaveDefenderSerial = defenderSerial;
                                Map cleaveMap = defenderMap;
                                Point3D cleaveLocation = defenderLocation;
                                int cleaveZ = defenderZ;
                                double cleaveDirectionX = directionX;
                                double cleaveDirectionY = directionY;
                                int cleaveDamage = finalDamage;

                                Timer.DelayCall( TimeSpan.Zero, () =>
                                {
                                        PerformCleave( cleaveAttacker, cleaveDefenderSerial, cleaveMap, cleaveLocation, cleaveZ, cleaveDirectionX, cleaveDirectionY, cleaveDamage );
                                } );
                        }

                        ResetCharge( attacker );
                }

		private static int GetBaseDamage( Mobile attacker )
		{
			return 10 + (int)(10.0 * (attacker.Skills[SkillName.Tactics].Value - 50.0) / 70.0 + 5);
		}

		private static int GetChargedDamage( int baseDamage, bool movedEnough )
		{
			if ( !movedEnough )
				return baseDamage;

			return (int)Math.Round( baseDamage * ChargeDamageBonus, MidpointRounding.AwayFromZero );
		}

		private static bool HasMetChargeRequirement( Mobile attacker )
		{
                        if ( attacker == null )
                                return false;

                        ChargeInfo info;

                        if ( !m_ChargeStates.TryGetValue( attacker, out info ) )
                                return false;

                        if ( info.Map == null || attacker.Map != info.Map )
                        {
                                if ( attacker.Map == null )
                                        return false;

                                info.Reset( attacker.Location, attacker.Map );
                                return false;
                        }

                        return info.TilesMoved >= ChargeDistanceRequirement;
                }

                private static void PerformCleave( Mobile attacker, Serial defenderSerial, Map map, Point3D defenderLocation, int defenderZ, double directionX, double directionY, int damage )
                {
                        if ( attacker == null || attacker.Deleted )
                                return;

                        if ( map == null )
                                map = attacker.Map;

                        if ( map == null )
                                return;

                        double magnitude = Math.Sqrt( (directionX * directionX) + (directionY * directionY) );

                        if ( magnitude <= 0 )
                                return;

                        double normalizedX = directionX / magnitude;
                        double normalizedY = directionY / magnitude;

                        List<Mobile> targets = new List<Mobile>();

                        IPooledEnumerable eable = map.GetMobilesInRange( defenderLocation, CleaveRange );

                        try
                        {
                                foreach ( Mobile m in eable )
                                {
                                        if ( m == null || m == attacker )
                                                continue;

                                        if ( defenderSerial != Serial.MinusOne && m.Serial == defenderSerial )
                                                continue;

                                        if ( !m.Alive || !SpellHelper.ValidIndirectTarget( attacker, m ) )
                                                continue;

                                        if ( !attacker.CanSee( m ) || !attacker.CanBeHarmful( m, false ) )
                                                continue;

                                        int offsetX = m.X - defenderLocation.X;
                                        int offsetY = m.Y - defenderLocation.Y;

                                        double projection = (offsetX * normalizedX) + (offsetY * normalizedY);

                                        if ( projection <= 0 )
                                                continue;

                                        double distance = Math.Sqrt( (offsetX * offsetX) + (offsetY * offsetY) );

                                        if ( distance > CleaveRange )
                                                continue;

                                        if ( distance <= 0 )
                                                continue;

                                        double normalizedOffsetX = offsetX / distance;
                                        double normalizedOffsetY = offsetY / distance;
                                        double directionalAlignment = (normalizedOffsetX * normalizedX) + (normalizedOffsetY * normalizedY);

                                        if ( directionalAlignment < CleaveCosineThreshold )
                                                continue;

                                        targets.Add( m );
                                }
                        }
                        finally
                        {
                                eable.Free();
                        }

                        PlayCleaveShockwave( map, defenderLocation, defenderZ, normalizedX, normalizedY );

                        if ( targets.Count == 0 )
                                return;

                        foreach ( Mobile target in targets )
                        {
                                attacker.DoHarmful( target );
                                AOS.Damage( target, attacker, damage, 100, 0, 0, 0, 0 );
                        }
                }

                private static void PlayCleaveShockwave( Map map, Point3D origin, int originZ, double directionX, double directionY )
                {
                        if ( map == null )
                                return;

                        Effects.PlaySound( origin, map, CleaveShockwaveSound );

                        double magnitude = Math.Sqrt( (directionX * directionX) + (directionY * directionY) );

                        if ( magnitude <= 0 )
                                return;

                        double normalizedX = directionX / magnitude;
                        double normalizedY = directionY / magnitude;

                        Point3D center = new Point3D( origin.X, origin.Y, originZ );
                        IPoint3D centerSurface = center;
                        SpellHelper.GetSurfaceTop( ref centerSurface );
                        center = new Point3D( centerSurface );

                        Effects.SendLocationParticles( EffectItem.Create( center, map, EffectItem.DefaultDuration ), CleaveShockwaveEffect, 7, 20, CleaveShockwaveHue );

                        for ( int dx = -CleaveRange; dx <= CleaveRange; ++dx )
                        {
                                for ( int dy = -CleaveRange; dy <= CleaveRange; ++dy )
                                {
                                        if ( dx == 0 && dy == 0 )
                                                continue;

                                        double offsetX = dx;
                                        double offsetY = dy;

                                        double projection = (offsetX * normalizedX) + (offsetY * normalizedY);

                                        if ( projection <= 0 )
                                                continue;

                                        double distance = Math.Sqrt( (offsetX * offsetX) + (offsetY * offsetY) );

                                        if ( distance > CleaveRange )
                                                continue;

                                        if ( distance <= 0 )
                                                continue;

                                        double normalizedOffsetX = offsetX / distance;
                                        double normalizedOffsetY = offsetY / distance;
                                        double directionalAlignment = (normalizedOffsetX * normalizedX) + (normalizedOffsetY * normalizedY);

                                        if ( directionalAlignment < CleaveCosineThreshold )
                                                continue;

                                        Point3D effectLocation = new Point3D( origin.X + dx, origin.Y + dy, originZ );
                                        IPoint3D surface = effectLocation;
                                        SpellHelper.GetSurfaceTop( ref surface );
                                        effectLocation = new Point3D( surface );

                                        Effects.SendLocationParticles( EffectItem.Create( effectLocation, map, EffectItem.DefaultDuration ), CleaveShockwaveEffect, 7, 20, CleaveShockwaveHue );
                                }
                        }
                }

                private static bool TryGetChargeDirection( Mobile attacker, Mobile defender, Point3D attackerLocation, Point3D defenderLocation, out double directionX, out double directionY )
                {
                        directionX = 0;
                        directionY = 0;

                        if ( attacker == null )
                                return false;

                        ChargeInfo info;

                        if ( m_ChargeStates.TryGetValue( attacker, out info ) && info.Map == attacker.Map )
                        {
                                if ( info.LastDeltaX != 0 || info.LastDeltaY != 0 )
                                {
                                        directionX = info.LastDeltaX;
                                        directionY = info.LastDeltaY;
                                }
                                else
                                {
                                        Point3D reference = defender != null ? defender.Location : defenderLocation;

                                        directionX = reference.X - info.Start.X;
                                        directionY = reference.Y - info.Start.Y;
                                }
                        }

                        if ( directionX == 0 && directionY == 0 )
                        {
                                directionX = defenderLocation.X - attackerLocation.X;
                                directionY = defenderLocation.Y - attackerLocation.Y;
                        }

                        if ( directionX == 0 && directionY == 0 )
                        {
                                Mobile target = defender;
                                Direction facing;

                                if ( target != null )
                                        facing = attacker.GetDirectionTo( target );
                                else
                                        facing = attacker.Direction;

                                int offsetX = 0;
                                int offsetY = 0;

                                Server.Movement.Movement.Offset( facing, ref offsetX, ref offsetY );

                                directionX = offsetX;
                                directionY = offsetY;
                        }

                        return directionX != 0 || directionY != 0;
                }

		private static void ResetCharge( Mobile attacker )
		{
                        if ( attacker != null )
                                m_ChargeStates.Remove( attacker );
                }

                private static void EventSink_Movement( MovementEventArgs e )
                {
                        if ( e == null )
                                return;

                        Mobile mobile = e.Mobile;

                        if ( mobile == null )
                                return;

                        ChargeInfo info;

                        if ( !m_ChargeStates.TryGetValue( mobile, out info ) )
                                return;

                        Point3D previousLocation = mobile.Location;
                        Map previousMap = mobile.Map;

                        Timer.DelayCall( TimeSpan.Zero, () => HandleMovementStep( mobile, previousLocation, previousMap ) );
                }

                private static void HandleMovementStep( Mobile mobile, Point3D previousLocation, Map previousMap )
                {
                        if ( mobile == null || mobile.Deleted )
                                return;

                        ChargeInfo info;

                        if ( !m_ChargeStates.TryGetValue( mobile, out info ) )
                                return;

                        Map map = mobile.Map;

                        if ( map == null )
                        {
                                m_ChargeStates.Remove( mobile );
                                return;
                        }

                        Point3D newLocation = mobile.Location;

                        if ( map != previousMap )
                        {
                                info.Reset( newLocation, map );
                                return;
                        }

                        if ( newLocation == previousLocation )
                                return;

                        int previousTilesMoved = info.TilesMoved;

                        info.RecordMovement( previousLocation, newLocation, map );

                        if ( !info.ChargeReadyAnnounced && previousTilesMoved < ChargeDistanceRequirement && info.TilesMoved >= ChargeDistanceRequirement )
                        {
                                info.ChargeReadyAnnounced = true;
                                mobile.PublicOverheadMessage( MessageType.Regular, 0x3B2, false, "Charge ready!" );
                                PlayChargeReadyEffect( mobile );
                        }
                }

                private static void PlayChargeReadyEffect( Mobile mobile )
                {
                        if ( mobile == null )
                                return;

                        Map map = mobile.Map;

                        if ( map == null )
                                return;

                        Effects.SendLocationParticles( EffectItem.Create( mobile.Location, map, EffectItem.DefaultDuration ), 0x3728, 10, 30, 5042 );
                        mobile.PlaySound( 0x2F3 );
                }
        }
}
