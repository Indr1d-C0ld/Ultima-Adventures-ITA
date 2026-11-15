using System;
using Server.Mobiles;

namespace Server.Items
{
	public class MilkBarrel : BaseBeverage
	{
		public override int MaxQuantity { get { return 100; } }

		public override int ComputeItemID()
		{
			return 0x0FAE;
		}

		[Constructable]
		public MilkBarrel()
		{
			Name = "milk barrel";
			Weight = 25.0;
			Content = BeverageType.Milk;
			Quantity = 0;
		}

		public MilkBarrel( Serial serial )
			: base( serial )
		{
		}

		public override void Fill_OnTarget( Mobile from, object targ )
		{
			if( !Fillable || !ValidateUse( from, false ) )
				return;

			if( targ is BaseBeverage )
			{
				BaseBeverage bev = (BaseBeverage)targ;

				if( bev.IsEmpty || bev.Content != BeverageType.Milk || !bev.ValidateUse( from, true ) )
					return;

				if( IsFull )
				{
					from.SendLocalizedMessage( 500848 ); // Couldn't pour it there.  It was already full.
					return;
				}

				int transfer = Math.Min( MaxQuantity - Quantity, bev.Quantity );

				if( transfer > 0 )
				{
					Content = BeverageType.Milk;
					Quantity += transfer;
					bev.Quantity -= transfer;
					from.PlaySound( 0x4E );
				}

				return;
			}

			if( targ is Cow )
			{
				Cow cow = (Cow)targ;

				if( cow.TryMilk( from ) )
				{
					Content = BeverageType.Milk;
					Quantity = MaxQuantity;
					from.SendLocalizedMessage( 1080197 ); // You fill the container with milk.
				}

				return;
			}

			from.SendLocalizedMessage( 500846 ); // Can't pour it there.
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if( Content != BeverageType.Milk )
				Content = BeverageType.Milk;
		}
	}
}
