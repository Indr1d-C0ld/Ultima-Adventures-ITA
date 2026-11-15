using System;

namespace Server.Items
{
	public class Wasabi : Item
	{
		[Constructable]
		public Wasabi() : base( 0x24E8 )
		{
			Weight = 1.0;
		}

		public Wasabi( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class WasabiClumps : Food
	{
		[Constructable]
		public WasabiClumps() : base( 0x24EB )
		{
			Stackable = false;
			Weight = 1.0;
			FillFactor = 2;
		}

		public WasabiClumps( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class EmptyBentoBox : Item
	{
		[Constructable]
		public EmptyBentoBox() : base( 0x2834 )
		{
			Weight = 5.0;
		}

		public EmptyBentoBox( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class BentoBox : Food
	{
		[Constructable]
		public BentoBox() : base( 0x2836 )
		{
			Stackable = false;
			Weight = 5.0;
			FillFactor = 2;
		}

		public override bool Eat( Mobile from )
		{
			if ( !base.Eat( from ) )
				return false;

			from.AddToBackpack( new EmptyBentoBox() );
			return true;
		}

		public BentoBox( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class SushiRolls : Food
	{
		[Constructable]
		public SushiRolls() : base( 0x283E )
		{
			Stackable = false;
			Weight = 3.0;
			FillFactor = 2;
		}

		public SushiRolls( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class SushiPlatter : Food
	{
		[Constructable]
		public SushiPlatter() : base( 0x2840 )
		{
            Stackable = Core.ML;
			Weight = 3.0;
			FillFactor = 2;
		}

		public SushiPlatter( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class GreenTeaBasket : Item
	{
	[Constructable]
	public GreenTeaBasket() : base( 0x284B )
	{
		Stackable = true;
		Weight = 3.0;
	}

		public GreenTeaBasket( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class GreenTea : Food
	{
		[Constructable]
		public GreenTea() : base( 0x284C )
		{
			Stackable = true;
			Weight = 4.0;
			FillFactor = 2;
		}

		public override bool Eat( Mobile from )
		{
			if ( from.Thirst < 20 )
			{
				from.Thirst += 6;

				int iThirst = from.Thirst;
				if ( iThirst < 5 )
					from.SendMessage( "You drink the tea but are still extremely thirsty" );
				else if ( iThirst < 10 )
					from.SendMessage( "You drink the tea and feel less thirsty" );
				else if ( iThirst < 15 )
					from.SendMessage( "You drink the tea and feel much less thirsty" );
				else
					from.SendMessage( "You drink the tea and are no longer thirsty" );

				Server.Items.DrinkingFunctions.DrinkBenefits( from );
			}
			else
			{
				from.Thirst = 20;
			}

			if ( Benefit > 0 && from.Thirst < 40 )
				from.Thirst += Benefit;

			from.PlaySound( Utility.RandomList( 0x30, 0x2D6 ) );

			if ( Poison != null )
				from.ApplyPoison( Poisoner, Poison );

			Consume();
			return true;
		}

		public GreenTea( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class MisoSoup : Food
	{
		[Constructable]
		public MisoSoup() : base( 0x284D )
		{
			Stackable = false;
			Weight = 4.0;
			FillFactor = 2;
		}

		public MisoSoup( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class WhiteMisoSoup : Food
	{
		[Constructable]
		public WhiteMisoSoup() : base( 0x284E )
		{
			Stackable = false;
			Weight = 4.0;
			FillFactor = 2;
		}

		public WhiteMisoSoup( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class RedMisoSoup : Food
	{
		[Constructable]
		public RedMisoSoup() : base( 0x284F )
		{
			Stackable = false;
			Weight = 4.0;
			FillFactor = 2;
		}

		public RedMisoSoup( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class AwaseMisoSoup : Food
	{
		[Constructable]
		public AwaseMisoSoup() : base( 0x2850 )
		{
			Stackable = false;
			Weight = 4.0;
			FillFactor = 2;
		}

		public AwaseMisoSoup( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}
