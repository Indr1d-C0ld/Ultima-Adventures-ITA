using System;
using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
	[CorpseName( "a mountain goat corpse" )]
        public class MountainGoat : BaseCreature
        {
                private DateTime m_NextHeadbutt;

                [Constructable]
                public MountainGoat() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
                {
			Name = "a mountain goat";
			Body = 209;
			BaseSoundID = 0x99;

			SetStr( 22, 64 );
			SetDex( 56, 75 );
			SetInt( 16, 30 );

			SetHits( 20, 33 );
			SetMana( 0 );

			SetDamage( 3, 7 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 10, 20 );
			SetResistance( ResistanceType.Fire, 5, 10 );
			SetResistance( ResistanceType.Cold, 10, 20 );
			SetResistance( ResistanceType.Poison, 10, 15 );
			SetResistance( ResistanceType.Energy, 10, 15 );

			SetSkill( SkillName.MagicResist, 25.1, 30.0 );
			SetSkill( SkillName.Tactics, 29.3, 44.0 );
			SetSkill( SkillName.Wrestling, 29.3, 44.0 );

			Fame = 300;
			Karma = 0;

                        VirtualArmor = 10;

                        Tamable = true;
                        ControlSlots = 1;
                        MinTameSkill = -0.9;

                        m_NextHeadbutt = DateTime.UtcNow;
                }

		public override int Meat{ get{ return 2; } }
		public override int Hides{ get{ return 12; } }
		public override int Furs{ get{ return Utility.RandomList( 0, 0, 0, 6 ); } }
		public override FurType FurType{ get{ return FurType.White; } }
		public override FoodType FavoriteFood{ get{ return FoodType.GrainsAndHay | FoodType.FruitsAndVegies; } }

		public MountainGoat(Serial serial) : base(serial)
		{
		}

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			base.OnGaveMeleeAttack( defender );

			Headbutt( defender );
		}

                private void Headbutt( Mobile defender )
                {
                        if ( defender == null || !defender.Player || Map == null )
                                return;

                        if ( DateTime.UtcNow < m_NextHeadbutt )
                                return;

                        m_NextHeadbutt = DateTime.UtcNow + TimeSpan.FromSeconds( 4 );

                        Direction dir = GetDirectionTo( defender );
                        Point3D collisionCheck = defender.Location;
                        Ability.IncreaseByDirection( ref collisionCheck, dir );

			bool collisionRisk = !Map.CanFit( collisionCheck.X, collisionCheck.Y, Map.GetAverageZ( collisionCheck.X, collisionCheck.Y ), 16, false, false );

			PublicOverheadMessage( MessageType.Regular, 0x3B2, false, "*headbutts*" );
			Ability.SlideAway( defender, Location, 3 );

			int damage = Utility.RandomMinMax( 6, 10 );

			if ( collisionRisk )
			{
				damage += Utility.RandomMinMax( 6, 10 );
				defender.SendMessage( "The impact drives you into the nearby terrain!" );
			}

			AOS.Damage( defender, this, damage, 100, 0, 0, 0, 0 );
		}

                public override void Serialize(GenericWriter writer)
                {
                        base.Serialize(writer);

                        writer.Write((int) 1);
                        writer.Write( m_NextHeadbutt );
                }

                public override void Deserialize(GenericReader reader)
                {
                        base.Deserialize(reader);

                        int version = reader.ReadInt();

                        if ( version >= 1 )
                                m_NextHeadbutt = reader.ReadDateTime();
                        else
                                m_NextHeadbutt = DateTime.UtcNow;
                }
        }
}
