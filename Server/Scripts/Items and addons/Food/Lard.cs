using System;

namespace Server.Items
{
	public class Lard : Item
	{
		[Constructable]
		public Lard() : this( 1 )
		{
		}

		[Constructable]
		public Lard( int amount ) : base( 0x1E8A )
		{
			Name = "lard";
			Stackable = true;
			Amount = amount;
			Weight = 1.0;
		}

		public Lard( Serial serial ) : base( serial )
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
