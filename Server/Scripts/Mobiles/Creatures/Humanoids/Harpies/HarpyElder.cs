using System;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a harpy corpse" )]
        public class HarpyElder : BaseHarpy
        {
                [Constructable]
                public HarpyElder() : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
                {
			Name = "an elder harpy";
			Body = 153;
			BaseSoundID = 402;

			SetStr( 296, 325 );
			SetDex( 86, 105 );
			SetInt( 291, 385 );

			SetHits( 178, 195 );

			SetDamage( 8, 19 );

			SetDamageType( ResistanceType.Physical, 50 );
			SetDamageType( ResistanceType.Energy, 50 );

			SetResistance( ResistanceType.Physical, 45, 55 );
			SetResistance( ResistanceType.Fire, 60, 70 );
			SetResistance( ResistanceType.Cold, 40, 50 );
			SetResistance( ResistanceType.Poison, 40, 50 );
			SetResistance( ResistanceType.Energy, 40, 50 );

			SetSkill( SkillName.Anatomy, 62.0, 100.0 );
			SetSkill( SkillName.EvalInt, 90.1, 100.0 );
			SetSkill( SkillName.Magery, 90.1, 100.0 );
			SetSkill( SkillName.MagicResist, 115.1, 130.0 );
			SetSkill( SkillName.Tactics, 80.1, 100.0 );
			SetSkill( SkillName.Wrestling, 80.1, 100.0 );

			Fame = 12500;
			Karma = -12500;

			VirtualArmor = 50;

			if ( Utility.RandomMinMax( 1, 5 ) == 1 )
			{
				Item egg = new Eggs( Utility.RandomMinMax( 1, 10 ) );
				PackItem( egg );
			}
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Rich );
			AddLoot( LootPack.Average );
		}

		public override int GetAttackSound()
		{
			return 916;
		}

		public override int GetAngerSound()
		{
			return 916;
		}

		public override int GetDeathSound()
		{
			return 917;
		}

		public override int GetHurtSound()
		{
			return 919;
		}

		public override int GetIdleSound()
		{
                        return 918;
                }

		public override bool CanRummageCorpses{ get{ return true; } }
		public override int Meat{ get{ return 4; } }
		public override MeatType MeatType{ get{ return MeatType.Bird; } }
		public override int Feathers{ get{ return 50; } }

		public HarpyElder( Serial serial ) : base( serial )
		{
		}

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );
                        writer.Write( (int) 2 );
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );
                        int version = reader.ReadInt();

                        if ( version == 1 )
                        {
                                reader.ReadDateTime();
                                reader.ReadDateTime();
                                reader.ReadDateTime();
                                reader.ReadDateTime();
                                reader.ReadDateTime();
                        }
                }
	}
}
