using System;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a harpy corpse" )]
        public class HarpyHen : BaseHarpy
        {

		[Constructable]
                public HarpyHen() : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a harpy hen";
			Body = 153;
			BaseSoundID = 402;
			Hue = 0x9C7;

			SetStr( 696, 725 );
			SetDex( 86, 105 );
			SetInt( 436, 475 );

			SetHits( 478, 495 );

			SetDamage( 14, 20 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 45, 60 );
			SetResistance( ResistanceType.Fire, 50, 60 );
			SetResistance( ResistanceType.Cold, 30, 40 );
			SetResistance( ResistanceType.Poison, 20, 30 );
			SetResistance( ResistanceType.Energy, 30, 40 );

			SetSkill( SkillName.EvalInt, 70.1, 80.0 );
			SetSkill( SkillName.Magery, 70.1, 80.0 );
			SetSkill( SkillName.MagicResist, 85.1, 95.0 );
			SetSkill( SkillName.Tactics, 70.1, 80.0 );
			SetSkill( SkillName.Wrestling, 60.1, 80.0 );

			Fame = 15000;
			Karma = -15000;

			VirtualArmor = 40;

			if ( Utility.RandomMinMax( 1, 5 ) == 1 )
			{
				Item egg = new Eggs( Utility.RandomMinMax( 1, 10 ) );
				PackItem( egg );
			}
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Rich );
			AddLoot( LootPack.Average, 2 );
			AddLoot( LootPack.MedScrolls, 2 );
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
		public override int Meat{ get{ return 6; } }
		public override MeatType MeatType{ get{ return MeatType.Bird; } }
		public override int Feathers{ get{ return 100; } }

		public HarpyHen( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}