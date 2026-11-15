using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a ghostly corpse" )]
        public class Wraith : BaseCreature
        {
                private static readonly TimeSpan PhaseDuration = TimeSpan.FromSeconds( 8.0 );
                private static readonly TimeSpan PhaseDelay = TimeSpan.FromSeconds( 18.0 );

                private readonly List<ResistanceMod> m_PhaseMods = new List<ResistanceMod>();
                private DateTime m_NextPhase;
                private DateTime m_PhaseEnd;
                private bool m_Incorporeal;

                [Constructable]
                public Wraith() : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
                {
			Name = "a wraith";
			Body = 84;
			Hue = 0x9C2;
			BaseSoundID = 0x482;

			SetStr( 76, 100 );
			SetDex( 76, 95 );
			SetInt( 36, 60 );

			SetHits( 46, 60 );

			SetDamage( 7, 11 );

			SetDamageType( ResistanceType.Physical, 50 );
			SetDamageType( ResistanceType.Cold, 50 );

			SetResistance( ResistanceType.Physical, 25, 30 );
			SetResistance( ResistanceType.Cold, 15, 25 );
			SetResistance( ResistanceType.Poison, 10, 20 );

			SetSkill( SkillName.EvalInt, 55.1, 70.0 );
			SetSkill( SkillName.Magery, 55.1, 70.0 );
			SetSkill( SkillName.MagicResist, 55.1, 70.0 );
			SetSkill( SkillName.Tactics, 45.1, 60.0 );
			SetSkill( SkillName.Wrestling, 45.1, 55.0 );

			Fame = 4000;
			Karma = -4000;

			VirtualArmor = 28;

                        PackReg( 10 );

                        m_NextPhase = DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 6, 12 ) );
                }

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Meager );
		}

                public override bool OnBeforeDeath()
                {
                        this.Body = 13;
                        return base.OnBeforeDeath();
                }

                public override void OnThink()
                {
                        base.OnThink();

                        if ( m_Incorporeal )
                        {
                                if ( DateTime.Now >= m_PhaseEnd )
                                        EndPhase();
                        }
                        else if ( DateTime.Now >= m_NextPhase )
                        {
                                BeginPhase();
                        }
                }

                private void BeginPhase()
                {
                        if ( m_Incorporeal )
                                return;

                        m_Incorporeal = true;
                        m_PhaseEnd = DateTime.Now + PhaseDuration;
                        m_NextPhase = m_PhaseEnd + PhaseDelay;

                        ClearPhaseMods();
                        m_PhaseMods.Add( new ResistanceMod( ResistanceType.Physical, 30 ) );
                        m_PhaseMods.Add( new ResistanceMod( ResistanceType.Energy, -20 ) );
                        m_PhaseMods.Add( new ResistanceMod( ResistanceType.Fire, -10 ) );
                        m_PhaseMods.Add( new ResistanceMod( ResistanceType.Cold, -10 ) );

                        foreach ( ResistanceMod mod in m_PhaseMods )
                                AddResistanceMod( mod );

                        FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );
                        PlaySound( 0x64E );
                        PublicOverheadMessage( Network.MessageType.Regular, 0x3B2, false, "The wraith fades into the ether!" );
                }

                private void EndPhase()
                {
                        if ( !m_Incorporeal )
                                return;

                        m_Incorporeal = false;
                        ClearPhaseMods();
                        FixedParticles( 0x375A, 9, 20, 5041, EffectLayer.Waist );
                        PlaySound( 0x64E );
                }

                private void ClearPhaseMods()
                {
                        foreach ( ResistanceMod mod in m_PhaseMods )
                                RemoveResistanceMod( mod );

                        m_PhaseMods.Clear();
                }

                public override void OnGaveMeleeAttack( Mobile defender )
                {
                        base.OnGaveMeleeAttack( defender );

                        if ( defender == null || !defender.Alive )
                                return;

                        if ( 0.25 > Utility.RandomDouble() )
                        {
                                int drain = Utility.RandomMinMax( 10, 20 );
                                defender.Stam -= drain;
                                defender.SendMessage( "A chilling touch saps your stamina!" );
                                defender.FixedParticles( 0x374A, 10, 15, 5028, EffectLayer.Waist );
                        }
                }

		public override bool BleedImmune{ get{ return true; } }

		public override OppositionGroup OppositionGroup
		{
			get{ return OppositionGroup.FeyAndUndead; }
		}

		public override Poison PoisonImmune{ get{ return Poison.Deadly; } }

		public Wraith( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
                        writer.Write( (int) 1 );

                        writer.Write( (DateTime) m_NextPhase );
                        writer.Write( (DateTime) m_PhaseEnd );
                        writer.Write( (bool) m_Incorporeal );
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );
                        int version = reader.ReadInt();

                        if ( version >= 1 )
                        {
                                m_NextPhase = reader.ReadDateTime();
                                m_PhaseEnd = reader.ReadDateTime();
                                m_Incorporeal = reader.ReadBool();

                                if ( m_Incorporeal )
                                        BeginPhase();
                        }
                        else
                        {
                                m_NextPhase = DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 6, 12 ) );
                        }
                }
        }
}