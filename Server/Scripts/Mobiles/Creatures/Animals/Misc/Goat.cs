using System;
using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
	[CorpseName( "a goat corpse" )]
        public class Goat : BaseCreature
        {
                private DateTime m_NextHeadbutt;

                [Constructable]
                public Goat() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
                {
			Name = "a goat";
			Body = 209;
			BaseSoundID = 0x99;

			SetStr( 19 );
			SetDex( 15 );
			SetInt( 5 );

			SetHits( 12 );
			SetMana( 0 );

			SetDamage( 3, 4 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 5, 15 );

			SetSkill( SkillName.MagicResist, 5.0 );
			SetSkill( SkillName.Tactics, 5.0 );
			SetSkill( SkillName.Wrestling, 5.0 );

			Fame = 150;
			Karma = 0;

			VirtualArmor = 10;

			Tamable = true;
			ControlSlots = 1;
                        MinTameSkill = 11.1;

                        m_NextHeadbutt = DateTime.UtcNow;
                }

		public override int Meat{ get{ return 2; } }
		public override int Hides{ get{ return 8; } }
		public override int Furs{ get{ return Utility.RandomList( 0, 0, 0, 4 ); } }
		public override FurType FurType{ get{ return FurType.White; } }
		public override FoodType FavoriteFood{ get{ return FoodType.GrainsAndHay | FoodType.FruitsAndVegies; } }

		public Goat(Serial serial) : base(serial)
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
                        Ability.SlideAway( defender, Location, 2 );

                        int damage = Utility.RandomMinMax( 4, 8 );

                        if ( collisionRisk )
                        {
                                damage += Utility.RandomMinMax( 4, 8 );
                                defender.SendMessage( "You are slammed into the nearby obstruction!" );
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
