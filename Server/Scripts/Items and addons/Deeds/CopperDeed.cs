using System;
using Server.Targeting;
using Server.Network;

namespace Server.Items
{

	public class CopperDeed : Item
	{

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();


		}

		public CopperDeed( Item commodity ) : base( 0x14F0 )
		{
			Weight = 1.0;
			Hue = 0x47;
			Name = "A deed redeemable for 10000 copper.";

		}

		[Constructable]
		public CopperDeed() : this( null )
		{
		}

		public CopperDeed( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			int number;

			BankBox box = from.FindBankNoCreate();
			
				if ( box != null && IsChildOf( box ) )
				{
					number = 1047031; // The commodity has been redeemed.
					DDCopper dds = new DDCopper(10000);

					box.DropItem( dds );

					Delete();
				}
				else
				{
					if( Core.ML )
					{
						number = 1080526; // That must be in your bank box or commodity deed box to use it.
					}
					else
					{
						number = 1047024; // To claim the resources ....
					}
				}
			

		}

	}
}