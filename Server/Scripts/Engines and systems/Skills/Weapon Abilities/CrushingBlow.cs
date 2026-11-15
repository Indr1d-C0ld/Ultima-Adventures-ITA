using System;
using System.Collections.Generic;
using Server;

namespace Server.Items
{
	/// <summary>
	/// Also known as the Haymaker, this attack dramatically increases the damage done by a weapon reaching its mark.
	/// </summary>
        public class CrushingBlow : WeaponAbility
        {
                private const int PhysicalResistanceReduction = 15;
                private static readonly TimeSpan ResistanceDebuffDuration = TimeSpan.FromSeconds( 8.0 );
                private static readonly Dictionary<Mobile, CrushingBlowDebuffInfo> m_Debuffs = new Dictionary<Mobile, CrushingBlowDebuffInfo>();

                public CrushingBlow()
                {
                }

                public override int BaseMana{ get{ return 25; } }
                public override double DamageScalar{ get{ return 1.5; } }


                public override void OnHit( Mobile attacker, Mobile defender, int damage )
                {
                        if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
                                return;

                        ClearCurrentAbility( attacker );

                        attacker.SendLocalizedMessage( 1060090 ); // You have delivered a crushing blow!
                        defender.SendLocalizedMessage( 1060091 ); // You take extra damage from the crushing attack!

                        defender.PlaySound( 0x1E1 );
                        defender.FixedParticles( 0, 1, 0, 9946, EffectLayer.Head );

                        Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( defender.X, defender.Y, defender.Z + 50 ), defender.Map ), new Entity( Serial.Zero, new Point3D( defender.X, defender.Y, defender.Z + 20 ), defender.Map ), 0xFB4, 1, 0, false, false, 0, 3, 9501, 1, 0, EffectLayer.Head, 0x100 );

                        ApplyPhysicalResistanceDebuff( defender );
                }

                private static void ApplyPhysicalResistanceDebuff( Mobile defender )
                {
                        if ( defender == null || defender.Deleted )
                                return;

                        CrushingBlowDebuffInfo info;

                        if ( !m_Debuffs.TryGetValue( defender, out info ) )
                        {
                                ResistanceMod mod = new ResistanceMod( ResistanceType.Physical, -PhysicalResistanceReduction );
                                defender.AddResistanceMod( mod );

                                info = new CrushingBlowDebuffInfo( defender, mod );
                                m_Debuffs[defender] = info;
                        }

                        if ( info.Mod != null )
                                info.Mod.Offset = -PhysicalResistanceReduction;

                        if ( info.Timer != null )
                        {
                                info.Timer.Stop();
                                info.Timer = null;
                        }

                        info.Timer = Timer.DelayCall( ResistanceDebuffDuration, new TimerStateCallback( ClearPhysicalResistanceDebuff ), info );
                        BuffInfo.AddBuff( defender, new BuffInfo( BuffIcon.CrushingBlowDebuff, 1151470, 1151471, ResistanceDebuffDuration, defender, PhysicalResistanceReduction.ToString() ) );
                }

                private static void ClearPhysicalResistanceDebuff( object state )
                {
                        CrushingBlowDebuffInfo info = state as CrushingBlowDebuffInfo;

                        if ( info == null )
                                return;

                        Mobile defender = info.Defender;

                        if ( defender != null && !defender.Deleted && info.Mod != null )
                                defender.RemoveResistanceMod( info.Mod );

                        if ( defender != null )
                        {
                                m_Debuffs.Remove( defender );
                                BuffInfo.RemoveBuff( defender, BuffIcon.CrushingBlowDebuff );
                        }

                        if ( info.Timer != null )
                        {
                                info.Timer.Stop();
                                info.Timer = null;
                        }
                }

                private class CrushingBlowDebuffInfo
                {
                        public CrushingBlowDebuffInfo( Mobile defender, ResistanceMod mod )
                        {
                                Defender = defender;
                                Mod = mod;
                        }

                        public Mobile Defender{ get; private set; }
                        public ResistanceMod Mod{ get; private set; }
                        public Timer Timer{ get; set; }
                }
        }
}
