using System;
using System.Collections;
using Server.Items;
using Server.Targeting;
using Server.Spells;

namespace Server.Mobiles
{
	[CorpseName( "a skeletal corpse" )]
	public class Skeleton : BaseCreature
	{
		private static readonly TimeSpan HandShedCooldown = TimeSpan.FromSeconds( 20.0 );
		private DateTime m_NextHandShed;

		[Constructable]
		public Skeleton() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a skeleton";
			Body = Utility.RandomList( 50, 56, 167, 168, 170 );
			BaseSoundID = 0x48D;

			SetStr( 56, 80 );
			SetDex( 56, 75 );
			SetInt( 16, 40 );

			SetHits( 34, 48 );

			SetDamage( 3, 7 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 15, 20 );
			SetResistance( ResistanceType.Fire, 5, 10 );
			SetResistance( ResistanceType.Cold, 25, 40 );
			SetResistance( ResistanceType.Poison, 25, 35 );
			SetResistance( ResistanceType.Energy, 5, 15 );

			SetSkill( SkillName.MagicResist, 45.1, 60.0 );
			SetSkill( SkillName.Tactics, 45.1, 60.0 );
			SetSkill( SkillName.Wrestling, 45.1, 55.0 );

			Fame = 450;
			Karma = -450;

			VirtualArmor = 16;

			switch ( Utility.Random( 6 ))
			{
				case 0: PackItem( new BoneArms() ); break;
				case 1: PackItem( new BoneChest() ); break;
				case 2: PackItem( new BoneGloves() ); break;
				case 3: PackItem( new BoneLegs() ); break;
				case 4: PackItem( new BoneHelm() ); break;
				case 5: PackItem( new BoneSkirt() ); break;
			}

			int[] list = new int[]
			{
				0x1B11, 0x1B12, 0x1B13, 0x1B14, 0x1B15, 0x1B16, 0x1B19, 0x1B1A, // bone parts
				0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3, 0x1AE4, // skulls
				0x1B17, 0x1B18, 0x1B1B, 0x1B1C, // ribs and spines
				0x1B09, 0x1B0A, 0x1B0B, 0x1B0C, 0x1B0D, 0x1B0E, 0x1B0F, 0x1B10, // bone piles
				0xECA, 0xECB, 0xECC, 0xECD, 0xECE, 0xECF, 0xED0, 0xED1, 0xED2 // bones
			};

			PackItem( new BodyPart( Utility.RandomList( list ) ) );
		}

		public override void OnThink()
		{
			base.OnThink();

			if ( DateTime.UtcNow < m_NextHandShed || Map == null || Map == Map.Internal || Combatant == null )
			return;

			bool lowHealth = Hits < ( HitsMax / 2 );
			bool outnumbered = CountNearbyAttackers( 8 ) > 1;

			if ( ( lowHealth || outnumbered ) && Utility.RandomDouble() < 0.25 && CountExistingHands() < 2 )
			{
				ShedHand();
				m_NextHandShed = DateTime.UtcNow + HandShedCooldown;
			}
		}

		private int CountNearbyAttackers( int range )
		{
			int count = 0;

			foreach ( AggressorInfo info in Aggressors )
			{
				if ( info.Attacker != null && !info.Attacker.Deleted && info.Attacker.Alive && info.Attacker.Map == Map && InRange( info.Attacker, range ) )
				count++;
			}

			return count;
		}

		private int CountExistingHands()
		{
			int count = 0;

			foreach ( Mobile mob in GetMobilesInRange( 10 ) )
			{
				if ( mob is CrawlingHand && ( (CrawlingHand)mob ).SummonMaster == this )
				count++;
			}

			return count;
		}

		private void ShedHand()
		{
			CrawlingHand hand = new CrawlingHand( this );
			hand.Team = Team;
			hand.Combatant = Combatant;
			hand.MoveToWorld( GetSpawnPosition(), Map );
			Effects.PlaySound( hand.Location, hand.Map, 0x1F1 );
		}

		private Point3D GetSpawnPosition()
		{
			Point3D loc = Location;

			for ( int i = 0; i < 4; i++ )
			{
				int x = X + Utility.RandomMinMax( -1, 1 );
				int y = Y + Utility.RandomMinMax( -1, 1 );
				int z = Map.GetAverageZ( x, y );
				Point3D p = new Point3D( x, y, z );

				if ( Map.CanSpawnMobile( p ) )
				return p;
			}

			return loc;
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Poor );
		}

		public override bool BleedImmune{ get{ return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Lesser; } }

		public Skeleton( Serial serial ) : base( serial )
		{
		}

		public override OppositionGroup OppositionGroup
		{
			get{ return OppositionGroup.FeyAndUndead; }
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 );
			writer.WriteDeltaTime( m_NextHandShed );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			if ( version > 0 )
			m_NextHandShed = reader.ReadDeltaTime();
		}
	}
}
