using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class BagOfHerbalismReagents : Bag
	{
		[Constructable]
		public BagOfHerbalismReagents()
		{
			Name = "Bag of herbalism reagents";
			Weight = 10.0;
		}

		public override void Open( Mobile from )
		{
			int amount = 50;

			if ( this.Weight > 2.0 )
			{
				DropItem( CreatePlant( new PlantHerbalism_Leaf(), amount, "dragon berry", 0x2FE0, 0x845 ) );
				DropItem( CreatePlant( new PlantHerbalism_Leaf(), amount, "winter berry", 0x2FE0, 0x47E ) );
				DropItem( CreatePlant( new PlantHerbalism_Leaf(), amount, "earth stem", 0xF88, 0x7D6 ) );
				DropItem( CreatePlant( new PlantHerbalism_Leaf(), amount, "tangle leaf", 0x2066, 0x8A4 ) );
				DropItem( CreatePlant( new PlantHerbalism_Leaf(), amount, "eldritch leaf", 0x2066, 0x48F ) );

				DropItem( CreatePlant( new PlantHerbalism_Flower(), amount, "lotus petal", 0x2FE8, 0x1A0 ) );
				DropItem( CreatePlant( new PlantHerbalism_Flower(), amount, "life root", 0xF85, 0x8A4 ) );
				DropItem( CreatePlant( new PlantHerbalism_Flower(), amount, "snake weed", 0xF88, 0x89F ) );

				DropItem( CreatePlant( new PlantHerbalism_Mushroom(), amount, "white mushroom", 0xB4D, 0x47E ) );
				DropItem( CreatePlant( new PlantHerbalism_Mushroom(), amount, "dark toadstool", 0xB4D, 0x96A ) );
				DropItem( CreatePlant( new PlantHerbalism_Mushroom(), amount, "purple fungus", 0xB4D, 0x13C ) );

				DropItem( CreatePlant( new PlantHerbalism_Lilly(), amount, "frog bed leaf", 0x2066, 0x89F ) );
				DropItem( CreatePlant( new PlantHerbalism_Lilly(), amount, "lilly flower petal", 0x2FE8, 0x47E ) );
				DropItem( CreatePlant( new PlantHerbalism_Lilly(), amount, "deep water stem", 0x0A96, 0x60 ) );

				DropItem( CreatePlant( new PlantHerbalism_Cactus(), amount, "desert root", 0xF86, 0x54E ) );
				DropItem( CreatePlant( new PlantHerbalism_Cactus(), amount, "cactus sponge", 0x2065, 0x89F ) );
				DropItem( CreatePlant( new PlantHerbalism_Cactus(), amount, "vampire thorn", 0x2069, 0x485 ) );

				DropItem( CreatePlant( new PlantHerbalism_Grass(), amount, "forest hair", 0x2064, 0x7D1 ) );
				DropItem( CreatePlant( new PlantHerbalism_Grass(), amount, "fey seed", 0x2067, 0x7D6 ) );
				DropItem( CreatePlant( new PlantHerbalism_Grass(), amount, "druidic blade", 0x0A96, 0x7D6 ) );

				this.Weight = 2.0;
			}

			base.Open( from );
		}

		private static Item CreatePlant( Item plant, int amount, string name, int itemID, int hue )
		{
			plant.Amount = amount;
			plant.Name = name;
			plant.ItemID = itemID;
			plant.Hue = hue;

			return plant;
		}

		public BagOfHerbalismReagents( Serial serial ) : base( serial )
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
