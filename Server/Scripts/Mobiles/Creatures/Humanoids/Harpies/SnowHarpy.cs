using System;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a snow harpy corpse" )]
        public class SnowHarpy : BaseHarpy
        {

		[Constructable]
                public SnowHarpy() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a snow harpy";
			Body = 30;
			BaseSoundID = 402;
			Hue = 1150;

			SetStr( 296, 320 );
			SetDex( 86, 110 );
			SetInt( 51, 75 );

			SetHits( 178, 192 );
			SetMana( 0 );

			SetDamage( 8, 16 );

			SetDamageType( ResistanceType.Physical, 75 );
			SetDamageType( ResistanceType.Cold, 25 );

			SetResistance( ResistanceType.Physical, 45, 55 );
			SetResistance( ResistanceType.Fire, 5, 10 );
			SetResistance( ResistanceType.Cold, 50, 70 );
			SetResistance( ResistanceType.Poison, 30, 40 );
			SetResistance( ResistanceType.Energy, 30, 40 );

			SetSkill( SkillName.MagicResist, 50.1, 65.0 );
			SetSkill( SkillName.Tactics, 70.1, 100.0 );
			SetSkill( SkillName.Wrestling, 70.1, 100.0 );

			Fame = 4500;
			Karma = -4500;

			VirtualArmor = 50;
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Average, 2 );
			AddLoot( LootPack.Gems, 2 );
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

		public SnowHarpy( Serial serial ) : base( serial )
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