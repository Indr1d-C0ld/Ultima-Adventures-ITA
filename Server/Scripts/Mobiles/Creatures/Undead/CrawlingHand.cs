using System;
using Server.Items;
using Server.Spells;

namespace Server.Mobiles
{
	[CorpseName( "a severed hand" )]
	public class CrawlingHand : BaseCreature
	{
		private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds( 30.0 );
		private DateTime m_Expiry;

		public override bool AlwaysMurderer{ get{ return true; } }
		public override double DispelDifficulty{ get{ return 10.0; } }
		public override double DispelFocus{ get{ return 15.0; } }
		public override bool DeleteOnRelease{ get{ return true; } }
		public override int Meat{ get{ return 1; } }

		public CrawlingHand( BaseCreature master ) : base( AIType.AI_Melee, FightMode.Closest, 8, 1, 0.2, 0.4 )
		{
			Name = "a crawling hand";
                        Body = 50; // placeholder skeleton body until custom art is available
			BaseSoundID = 0x1F1;
			Hue = 0x835;

			SetStr( 25 );
			SetDex( 40 );
			SetInt( 5 );

			SetHits( 20 );
			SetDamage( 2, 4 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 10, 15 );
			SetResistance( ResistanceType.Fire, 5, 10 );
			SetResistance( ResistanceType.Cold, 10, 20 );
			SetResistance( ResistanceType.Poison, 10, 20 );

			SetSkill( SkillName.Wrestling, 35.0 );
			SetSkill( SkillName.Tactics, 30.0 );

			Fame = 450;
			Karma = -450;
			VirtualArmor = 8;

			if ( master != null )
			{
				Summoned = true;
				SummonMaster = master;
				ControlSlots = 0;
				m_Expiry = DateTime.UtcNow + Lifetime;
				SummonEnd = m_Expiry;
				new UnsummonTimer( master, this, Lifetime ).Start();
			}
		}

		public CrawlingHand() : this( null )
		{
		}

		public override void OnThink()
		{
			base.OnThink();

			if ( Deleted )
				return;

			Mobile master = SummonMaster;

			if ( master == null || master.Deleted || master.Map != Map || !InRange( master, 12 ) )
			{
				Delete();
				return;
			}

			if ( DateTime.UtcNow >= m_Expiry )
			{
				Delete();
			}
		}

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			base.OnGaveMeleeAttack( defender );

			if ( defender == null || !defender.Alive )
				return;

			if ( 0.35 > Utility.RandomDouble() )
				defender.ApplyPoison( this, Poison.Lesser );

			if ( 0.25 > Utility.RandomDouble() )
				BleedAttack.BeginBleed( defender, this );
		}

		public CrawlingHand( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 );
			writer.WriteDeltaTime( m_Expiry );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			if ( version > 0 )
				m_Expiry = reader.ReadDeltaTime();
		}
	}
}
