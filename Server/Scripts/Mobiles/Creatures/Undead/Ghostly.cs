using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a ghostly corpse" )]
        public class Ghostly : BaseCreature
        {
                private static readonly TimeSpan PhaseDuration = TimeSpan.FromSeconds( 8.0 );
                private static readonly TimeSpan PhaseDelay = TimeSpan.FromSeconds( 18.0 );

                private readonly List<ResistanceMod> m_PhaseMods = new List<ResistanceMod>();
                private DateTime m_NextPhase;
                private DateTime m_PhaseEnd;
                private bool m_Incorporeal;

                [Constructable]
                public Ghostly() : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
                {
			Name = Server.Misc.RandomThings.GetRandomWizardName();
			Title = "the ghost";
			Body = 0x3CA;
			Hue = 1150;
			BaseSoundID = 0x482;
			int bump = 0;

			switch( Utility.RandomMinMax( 0, 24 ) )
			{
				case 0: Title = "the ruby ghost"; Hue = 0x5B5; bump = 1; break;
				case 1: Title = "the azure ghost"; Hue = 0x5B6; bump = 2; break;
				case 2: Title = "the emerald ghost"; Hue = 0xB93; bump = 3; break;
				case 3: Title = "the golden ghost"; Hue = 0x8A5; bump = 4; break;
				case 4: Title = "the dark ghost"; Hue = 1175; bump = 5; break;
			}

			SetStr( 171, 200 );
			SetDex( 126, 145 );
			SetInt( 276, 305 );

			SetHits( 103, 120 );

			SetDamage( 24, 26 );

			SetDamageType( ResistanceType.Physical, 10 );
			SetDamageType( ResistanceType.Cold, 40 );
			SetDamageType( ResistanceType.Energy, 50 );

			SetResistance( ResistanceType.Physical, 40, 60 );
			SetResistance( ResistanceType.Fire, 20, 30 );
			SetResistance( ResistanceType.Cold, 50, 60 );
			SetResistance( ResistanceType.Poison, 55, 65 );
			SetResistance( ResistanceType.Energy, 40, 50 );

			SetSkill( SkillName.Necromancy, 89, 99.1 );
			SetSkill( SkillName.SpiritSpeak, 90.0, 99.0 );
			SetSkill( SkillName.EvalInt, 100.0 );
			SetSkill( SkillName.Magery, 70.1, 80.0 );
			SetSkill( SkillName.Meditation, 85.1, 95.0 );
			SetSkill( SkillName.MagicResist, 80.1, 100.0 );
			SetSkill( SkillName.Tactics, 70.1, 90.0 );

			Fame = 8000;
			Karma = -8000;

			VirtualArmor = 50;
			PackItem( new GraveDust( 10 ) );
			PackNecroReg( 17, 24 );

                        AddItem( new LightSource() );

                        BeefUp( (BaseCreature)this, bump );

                        m_NextPhase = DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 6, 12 ) );
                }

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Average );
			AddLoot( LootPack.MedScrolls, 1 );
		}

		public override OppositionGroup OppositionGroup
		{
			get{ return OppositionGroup.FeyAndUndead; }
		}

                public override void OnDeath( Container c )
                {
                        base.OnDeath( c );

			Mobile killer = this.LastKiller;
			if ( killer != null )
			{
				if ( killer is BaseCreature )
					killer = ((BaseCreature)killer).GetMaster();

				if ( killer is PlayerMobile )
				{
					if ( Server.Misc.GetPlayerInfo.LuckyKiller( killer.Luck, killer ) )
					{
						int Magic = 0;
						int Speed = 0;
						if ( this.Hue == 0x5B5 ){ Magic = 5; Speed = 0; }
						else if ( this.Hue == 0x5B6 ){ Magic = 10; Speed = 1; }
						else if ( this.Hue == 0xB93 ){ Magic = 15; Speed = 1; }
						else if ( this.Hue == 0x8A5 ){ Magic = 20; Speed = 2; }
						else if ( this.Hue == 1175 ){ Magic = 25; Speed = 2; }

						Robe robe = new Robe();
							robe.Name = "shroud of " + this.Title;
							robe.Hue = this.Hue;
							robe.Attributes.CastRecovery = Speed;
							robe.Attributes.CastSpeed = Speed;
							robe.Attributes.LowerManaCost = 5 + Magic;
							robe.Attributes.LowerRegCost = 5 + Magic;
							robe.Attributes.SpellDamage = 5 + Magic;
							c.DropItem( robe );
					}
				}
			}
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
                        PlaySound( 0x482 );
                        PublicOverheadMessage( Network.MessageType.Regular, 0x3B2, false, "The ghost phases into the spirit world!" );
                }

                private void EndPhase()
                {
                        if ( !m_Incorporeal )
                                return;

                        m_Incorporeal = false;
                        ClearPhaseMods();
                        FixedParticles( 0x375A, 9, 20, 5041, EffectLayer.Waist );
                        PlaySound( 0x482 );
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

		public override bool ShowFameTitle{ get{ return false; } }
		public override bool BleedImmune{ get{ return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Deadly; } }
		public override int TreasureMapLevel{ get{ return 3; } }
		public override bool AlwaysAttackable{ get{ return true; } }

		public Ghostly( Serial serial ) : base( serial )
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