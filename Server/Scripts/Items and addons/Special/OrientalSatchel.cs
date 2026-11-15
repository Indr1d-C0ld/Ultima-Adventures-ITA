using System;
using Server; 
using System.Collections;
using Server.ContextMenus;
using System.Collections.Generic;
using Server.Misc;
using Server.Network;
using Server.Items;
using Server.Gumps;
using Server.Mobiles;
using Server.Commands;
using System.Globalization;
using Server.Regions;

namespace Server.Items
{
	public class OrientalSatchel : Item
	{
		[Constructable]
		public OrientalSatchel() : base( 0x27BE )
		{
			Weight = 1.0;
			Name = "oriental satchel";
			LootType = LootType.Blessed;
		}

		public override bool DisplayLootType{ get{ return false; } }

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.InRange( this.GetWorldLocation(), 4 ) )
			{
				from.CloseGump( typeof( OrientalSatchelGump ) );
				from.SendGump( new OrientalSatchelGump( from, this ) );
				from.PlaySound( 0x48 );
			}
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			bool openGump = false;

				if ( dropped != null && ( dropped is BaseArmor || dropped is BaseClothing || dropped is BaseJewel || dropped is BaseHat ) )
				{
					if ( dropped is BaseHat || dropped is MagicHat ){ openGump = true; }
					else if ( dropped is BaseArmor && dropped.Layer == Layer.Arms ){ openGump = true; }
					else if ( dropped.Layer == Layer.Gloves && dropped is BaseArmor )
					{
						BaseArmor gloves = (BaseArmor)dropped;
						if ( gloves.ItemID != 0x564E ){ ChangeItem( dropped, 0x564E, "guantlets", from ); }
						else if ( Server.Misc.MaterialInfo.IsAnyKindOfMetalItem( dropped ) ){ ChangeItem( dropped, 0x1414, "guantlets", from ); }
						else if ( Server.Misc.MaterialInfo.IsAnyKindOfWoodItem( dropped ) ){ ChangeItem( dropped, 0x1414, "guantlets", from ); }
						else { ChangeItem( dropped, 0x13C6, "gloves", from ); }
					}
					else if ( dropped.ItemID == 0x2790 || dropped.ItemID == 0x2B68 || dropped.ItemID == 0x153B )
					{
						ChangeItem( dropped, 0x2B68, "loin cloth", from );
					}
					else if ( dropped.ItemID == 0x2B68 )
					{
						ChangeItem( dropped, 0x55DB, "royal loin cloth", from );
					}
					else if ( dropped.Layer == Layer.Neck )
					{
						ChangeItem( dropped, 0x5650, "amulet", from );
					}
					else if ( dropped.ItemID == 0x1541 || dropped.ItemID == 0x1542 )
					{
						ChangeItem( dropped, 0x0409, "sash", from );
					}
					else if ( dropped.ItemID == 0x0409 )
					{
						ChangeItem( dropped, 0x1541, "sash", from );
					}
					else if ( dropped.Layer == Layer.OuterTorso )
					{
						openGump = true; 
					}
					else if ( dropped.Layer == Layer.Cloak && (
						dropped.ItemID == 0x1515 || 
						dropped.ItemID == 0x1530 || 
						dropped.ItemID == 0x2309 || 
						dropped.ItemID == 0x230A || 
						dropped.ItemID == 0x26AD || 
						dropped.ItemID == 0x2B04 || 
						dropped.ItemID == 0x2B05 || 
						dropped.ItemID == 0x2B76 || 
						dropped.ItemID == 0x316D || 
						dropped.ItemID == 0x5679 ) )
					{
						if ( dropped.ItemID != 0x5679 ){ ChangeItem( dropped, 0x5679, "fleece", from ); }
						else { ChangeItem( dropped, 0x1515, "cloak", from ); }
					}
					else if ( dropped.Layer == Layer.Helm && dropped is BaseArmor ){ openGump = true; }
					else if ( dropped is BaseShield ){ openGump = true; }
					else if ( ( dropped.Layer == Layer.Pants || dropped.Layer == Layer.InnerLegs ) && dropped is BaseArmor ){ openGump = true; }
					else if ( ( dropped.Layer == Layer.Shirt || dropped.Layer == Layer.InnerTorso ) && dropped is BaseArmor ){ openGump = true; }
					else if ( dropped.Layer == Layer.Shoes )
					{
						bool metalShoes = false;
						if ( dropped is BaseArmor )
						{
							BaseArmor boots = (BaseArmor)dropped;
							if ( Server.Misc.MaterialInfo.IsAnyKindOfMetalItem( boots ) ){ metalShoes = true; }
						}

						if ( dropped.ItemID == 0x0406 ){ ChangeItem( dropped, 0x170D, "sandals", from ); }

						else if ( dropped.ItemID == 0x170D )
						{
							if ( metalShoes ){ ChangeItem( dropped, 0x170B, "boots", from ); }

							else if ( dropped.ItemID != 0x2B67 ){ ChangeItem( dropped, 0x2B67, "boots", from ); }
						}
						else
						{
							ChangeItem( dropped, 0x0406, "boots", from );
						}
					}
				}

				if ( from is PlayerMobile && openGump )
				{
					from.CloseGump( typeof( OrientalSatchelGump ) );
					from.CloseGump( typeof( OrientalAlterGump ) );
					from.SendGump( new OrientalAlterGump( from, dropped ) );
					from.PlaySound( 0x48 );
				}

			return false;
		}

		public OrientalSatchel( Serial serial ) : base( serial )
		{
		}

		public Mobile owner;
		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner { get{ return owner; } set{ owner = value; } }

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)1 ); // version
			writer.Write( (Mobile)owner);
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			owner = reader.ReadMobile();
		}

		public static void GetRidOf( Mobile from )
		{
			ArrayList targets = new ArrayList();
			foreach ( Item item in World.Items.Values )
			if ( item is OrientalSatchel )
			{
				if ( ((OrientalSatchel)item).owner == from )
					targets.Add( item );
			}
			for ( int i = 0; i < targets.Count; ++i )
			{
				Item item = ( Item )targets[ i ];
				item.Delete();
			}
		}

		public static void GivePack( Mobile from )
		{
			Server.Items.OrientalSatchel.GetRidOf( from );
			OrientalSatchel pack = new OrientalSatchel();
			pack.owner = from;
			from.AddToBackpack( pack );
			from.SendMessage( "A oriental satchel has been added to your pack." );
		}

		public class OrientalSatchelGump : Gump
		{
			public OrientalSatchelGump( Mobile from, Item satchel ): base( 25, 25 )
			{
				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				string sText = "Walk the path of Tokuno with armor and garments styled for a samurai court, ronin wanderer, or imperial guard. This satchel cannot store items; instead, it reshapes the appearance of compatible equipment while preserving all existing properties. Place shields, hats, helms, sleeves, tunics, leggings, boots, necklaces, cloaks, and robes onto the satchel to receive oriental-themed visuals.<br><br>Some converted pieces are intended as style overlays and may not all display on your paperdoll in the same way as standard gear. Robes still layer over tunics and sleeves, and converting a mantle can create lighter variants for a more wandering ronin look. Keep experimenting to find the look you want.<br><br><br><br><br><br><br><br><br><br><br><br>";

				if ( satchel.Weight > 0 )
				{
					sText = "This play style grant has placed this satchel in your main pack. It is a style tool, not a container. Drop compatible gear onto it to transform visuals into Tokuno-inspired appearances while preserving all item stats and magical properties.<br><br>This satchel is bound to your character and only one is intended at a time. If it is lost, reacquire it through your play-style flow. Robes still cover tunics/sleeves, and style conversions are reversible through other style satchels if desired.<br><br>Oriental style examples include jingasa helms, hiro sode sleeves, do chest armor, haidate leg armor, and suneate greaves.<br><br><br><br><br><br><br><br><br><br><br><br>";
				}

				AddPage(0);
				AddImage(0, 0, 155);
				AddImage(300, 0, 155);
				AddImage(600, 0, 155);
				AddImage(0, 300, 155);
				AddImage(300, 300, 155);
				AddImage(600, 300, 155);
				AddImage(2, 2, 129);
				AddImage(301, 2, 129);
				AddImage(598, 2, 129);
				AddImage(2, 298, 129);
				AddImage(301, 298, 129);
				AddImage(598, 298, 129);
				AddImage(580, 20, 155);
				AddImage(574, 20, 155);
				AddImage(580, 86, 155);
				AddImage(574, 86, 155);
				AddImage(577, 23, 10862);
				AddImage(7, 7, 133);
				AddImage(235, 46, 132);
				AddImage(274, 46, 132);
				AddHtml( 111, 85, 439, 21, @"<BODY><BASEFONT Color=#FBFBFB><BIG><CENTER>ORIENTAL SATCHEL</CENTER></BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 111, 121, 439, 458, @"<BODY><BASEFONT Color=#FCFF00><BIG>" + sText + "</BIG></BASEFONT></BODY>", (bool)false, (bool)true);

				AddItem(565, 392, 0x1F03);
				AddItem(581, 445, 0x2780);
				AddItem(586, 484, 0x2777);
				AddItem(588, 524, 0x278F);
				AddItem(570, 558, 0x2794);

				AddItem(730, 393, 0x278E);
				AddItem(729, 427, 0x278D);
				AddItem(728, 467, 0x277D);
				AddItem(728, 515, 0x5650);
				AddItem(719, 544, 0x1515);

				AddHtml( 624, 408, 99, 21, @"<BODY><BASEFONT Color=#FFA200><BIG>Robe</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 624, 451, 99, 21, @"<BODY><BASEFONT Color=#FFA200><BIG>Sleeves</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 624, 488, 99, 21, @"<BODY><BASEFONT Color=#FFA200><BIG>Helm</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 624, 522, 99, 21, @"<BODY><BASEFONT Color=#FFA200><BIG>Hat</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 624, 556, 99, 21, @"<BODY><BASEFONT Color=#FFA200><BIG>Sash</BIG></BASEFONT></BODY>", (bool)false, (bool)false);

				AddHtml( 776, 399, 99, 21, @"<BODY><BASEFONT Color=#FFA200><BIG>Gloves</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 776, 434, 99, 21, @"<BODY><BASEFONT Color=#FFA200><BIG>Leggings</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 776, 475, 99, 21, @"<BODY><BASEFONT Color=#FFA200><BIG>Tunic</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 776, 516, 99, 21, @"<BODY><BASEFONT Color=#FFA200><BIG>Amulet</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 776, 558, 99, 21, @"<BODY><BASEFONT Color=#FFA200><BIG>Cloak</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				AddItem(9, 391, 12219);
				AddItem(54, 393, 7035);
				AddItem(46, 424, 4650);
				AddItem(-8, 414, 11560);
				AddItem(10, 473, 19126);
			}

			public override void OnResponse( NetState state, RelayInfo info ) 
			{
				Mobile from = state.Mobile; 
				from.PlaySound( 0x48 );
			}
		}

		public class OrientalAlterGump : Gump
		{
			private Item m_Item;

			public OrientalAlterGump( Mobile from, Item item ): base( 25, 25 )
			{
				m_Item = item; 

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);
				AddHtml( 10, 7, 434, 20, @"<BODY><BASEFONT Color=#FBFBFB><BIG>What do you want to change the item into?</BIG></BASEFONT></BODY>", (bool)false, (bool)false);

				if ( item is BaseHat || item is MagicHat || ( Server.Misc.MaterialInfo.IsAnyKindOfClothItem( item ) && item.Layer == Layer.Helm ) )
				{
					AddImage(0, 0, 164);
					AddImage(150, 0, 164);
					AddImage(300, 0, 164);
					AddImage(2, 2, 165);
					AddImage(144, 2, 165);
					AddImage(302, 2, 165);
					AddImage(220, 2, 165);

					AddItem(32, 58, 0x278F);
					AddItem(78, 59, 0x2777);
					AddItem(136, 60, 0x2781);
					AddItem(195, 53, 0x278F);
					AddItem(256, 53, 0x2777);
					AddItem(322, 58, 0x2781);
					AddItem(384, 63, 0x278F);

					AddButton(25, 100, 4018, 4018, 1, GumpButtonType.Reply, 0);
					AddButton(84, 100, 4005, 4005, 2, GumpButtonType.Reply, 0);
					AddButton(147, 100, 4005, 4005, 3, GumpButtonType.Reply, 0);
					AddButton(207, 100, 4005, 4005, 4, GumpButtonType.Reply, 0);
					AddButton(267, 100, 4005, 4005, 5, GumpButtonType.Reply, 0);
					AddButton(327, 100, 4005, 4005, 6, GumpButtonType.Reply, 0);
					AddButton(388, 100, 4005, 4005, 7, GumpButtonType.Reply, 0);
				}
				else if ( item is BaseArmor && item.Layer == Layer.Arms )
				{
					AddImage(0, 0, 164);
					AddImage(150, 0, 164);
					AddImage(220, 0, 164);
					AddImage(2, 2, 165);
					AddImage(144, 2, 165);
					AddImage(222, 2, 165);

					AddItem(89, 61, 0x2780);
					AddItem(232, 61, 0x277E);

					AddButton(89, 106, 4018, 4018, 8, GumpButtonType.Reply, 0);
					AddButton(242, 106, 4005, 4005, 9, GumpButtonType.Reply, 0);
				}
				else if ( item is BaseArmor && item.Layer == Layer.Helm )
				{
					AddImage(150, 0, 164);
					AddImage(300, 0, 164);
					AddImage(2, 2, 165);
					AddImage(144, 2, 165);
					AddImage(302, 2, 165);
					AddImage(220, 2, 165);

					AddItem(46, 58, 0x2777);
					AddItem(105, 59, 0x2777);
					AddItem(169, 58, 0x2781);
					AddItem(245, 60, 0x278F);
					AddItem(305, 54, 0x2777);
					AddItem(372, 60, 0x2781);

					AddButton(42, 100, 4018, 4018, 10, GumpButtonType.Reply, 0);
					AddButton(110, 100, 4005, 4005, 11, GumpButtonType.Reply, 0);
					AddButton(181, 100, 4005, 4005, 12, GumpButtonType.Reply, 0);
					AddButton(249, 100, 4005, 4005, 13, GumpButtonType.Reply, 0);
					AddButton(313, 100, 4005, 4005, 14, GumpButtonType.Reply, 0);
					AddButton(380, 100, 4005, 4005, 15, GumpButtonType.Reply, 0);
				}
				else if ( item.Layer == Layer.OuterTorso )
				{
					AddImage(0, 0, 164);
					AddImage(150, 0, 164);
					AddImage(232, 0, 164);
					AddImage(2, 2, 165);
					AddImage(139, 2, 165);
					AddImage(234, 2, 165);

					AddButton(22, 106, 4018, 4018, 30, GumpButtonType.Reply, 0);
					AddButton(172, 106, 4005, 4005, 31, GumpButtonType.Reply, 0);
					AddButton(325, 106, 4005, 4005, 32, GumpButtonType.Reply, 0);

					AddItem(14, 44, 0x1F03);
					AddItem(164, 44, 0x1515);
					AddItem(310, 52, 0x1F04);

					AddHtml( 154, 129, 69, 20, @"<BODY><BASEFONT Color=#FFA200><BIG><CENTER>Scant</CENTER></BIG></BASEFONT></BODY>", (bool)false, (bool)false);
				}
				else if ( item is BaseShield )
				{
					AddImage(0, 0, 164);
					AddImage(150, 0, 164);
					AddImage(220, 0, 164);
					AddImage(2, 2, 165);
					AddImage(144, 2, 165);
					AddImage(222, 2, 165);

					AddItem(32, 61, 0x2B01);
					AddItem(114, 58, 0x2B02);
					AddItem(211, 63, 0x2B03);
					AddItem(288, 60, 0x2B04);

					AddButton(39, 108, 4005, 4005, 16, GumpButtonType.Reply, 0);
					AddButton(129, 108, 4005, 4005, 17, GumpButtonType.Reply, 0);
					AddButton(218, 108, 4005, 4005, 18, GumpButtonType.Reply, 0);
					AddButton(298, 108, 4005, 4005, 19, GumpButtonType.Reply, 0);
				}
				else if ( ( item.Layer == Layer.Pants || item.Layer == Layer.InnerLegs ) && item is BaseArmor )
				{
					AddImage(0, 0, 164);
					AddImage(150, 0, 164);
					AddImage(220, 0, 164);
					AddImage(2, 2, 165);
					AddImage(144, 2, 165);
					AddImage(222, 2, 165);

					AddItem(20, 47, 0x278D);
					AddItem(110, 55, 0x2788);
					AddItem(206, 56, 0x279A);
					AddItem(307, 59, 0x279B);

					AddButton(22, 106, 4018, 4018, 20, GumpButtonType.Reply, 0);
					AddButton(115, 104, 4005, 4005, 21, GumpButtonType.Reply, 0);
					AddButton(215, 106, 4005, 4005, 22, GumpButtonType.Reply, 0);
					AddButton(312, 107, 4005, 4005, 23, GumpButtonType.Reply, 0);
				}
				else if ( ( item.Layer == Layer.Shirt || item.Layer == Layer.InnerTorso ) && item is BaseArmor )
				{
					AddImage(0, 0, 164);
					AddImage(150, 0, 164);
					AddImage(220, 0, 164);
					AddImage(2, 2, 165);
					AddImage(144, 2, 165);
					AddImage(222, 2, 165);

					AddItem(21, 50, 0x277D);
					if ( from.Female ){ AddItem(66, 60, 0x2794); }
					if ( from.Female ){ AddItem(123, 60, 0x2794); }
					if ( from.Female ){ AddItem(186, 58, 0x2794); }
					if ( from.Female ){ AddItem(251, 54, 0x2794); }
					AddItem(304, 49, 0x277D);

					AddButton(22, 106, 4018, 4018, 24, GumpButtonType.Reply, 0);
					if ( from.Female ){ AddButton(79, 106, 4005, 4005, 25, GumpButtonType.Reply, 0); }
					if ( from.Female ){ AddButton(135, 105, 4005, 4005, 26, GumpButtonType.Reply, 0); }
					if ( from.Female ){ AddButton(195, 105, 4005, 4005, 27, GumpButtonType.Reply, 0); }
					if ( from.Female ){ AddButton(253, 105, 4005, 4005, 28, GumpButtonType.Reply, 0); }
					AddButton(316, 106, 4005, 4005, 29, GumpButtonType.Reply, 0);
				}
			}

			public override void OnResponse( NetState state, RelayInfo info ) 
			{
				Mobile from = state.Mobile; 
				string NewName = "";
				int itemID = 0;

				if ( info.ButtonID == 1 ){ 			NewName = "cap";				itemID = 0x278F; }
				else if ( info.ButtonID == 2 ){ 	NewName = "circlet";			itemID = 0x2777; }
				else if ( info.ButtonID == 3 ){ 	NewName = "headband";			itemID = 0x2781; }
				else if ( info.ButtonID == 4 ){ 	NewName = "hood";				itemID = 0x278F; }
				else if ( info.ButtonID == 5 ){ 	NewName = "cowl";				itemID = 0x2777; }
				else if ( info.ButtonID == 6 ){ 	NewName = "mask";				itemID = 0x2781; }
				else if ( info.ButtonID == 7 ){ 	NewName = "mask";				itemID = 0x278F; }

				else if ( info.ButtonID == 8 ){ 	NewName = "bracers";			itemID = 0x2780; }
				else if ( info.ButtonID == 9 ){ 	NewName = "bracers";			itemID = 0x277E; }

				else if ( info.ButtonID == 10 ){ 	NewName = "helm";				itemID = 0x2777; }
				else if ( info.ButtonID == 11 ){ 	NewName = "circlet";			itemID = 0x2777; }
				else if ( info.ButtonID == 12 ){ 	NewName = "horned helm";		itemID = 0x2781; }
				else if ( info.ButtonID == 13 ){ 	NewName = "skull helm";			itemID = 0x278F; }
				else if ( info.ButtonID == 14 ){ 	NewName = "helmet";				itemID = 0x2777; }
				else if ( info.ButtonID == 15 ){ 	NewName = "helmet";				itemID = 0x2781; }

				else if ( info.ButtonID == 16 ){ 	NewName = "shield";				itemID = 0x2B01; }
				else if ( info.ButtonID == 17 ){ 	NewName = "shield";				itemID = 0x2B02; }
				else if ( info.ButtonID == 18 ){ 	NewName = "shield";				itemID = 0x2B03; }
				else if ( info.ButtonID == 19 ){ 	NewName = "shield";				itemID = 0x2B04; }

				else if ( info.ButtonID == 20 ){ 	NewName = "breeches";			itemID = 0x278D; }
				else if ( info.ButtonID == 21 ){ 	NewName = "greaves";			itemID = 0x2788; }
				else if ( info.ButtonID == 22 ){ 	NewName = "skirt";				itemID = 0x279A; }
				else if ( info.ButtonID == 23 ){ 	NewName = "shorts";				itemID = 0x279B; }

				else if ( info.ButtonID == 24 ){ 	NewName = "armor";				itemID = 0x277D; }
				else if ( info.ButtonID == 25 ){ 	NewName = "bustier";			itemID = 0x2794; }
				else if ( info.ButtonID == 26 ){ 	NewName = "bustier";			itemID = 0x2794; }
				else if ( info.ButtonID == 27 ){ 	NewName = "bustier";			itemID = 0x2794; }
				else if ( info.ButtonID == 28 ){ 	NewName = "bustier";			itemID = 0x2794; }
				else if ( info.ButtonID == 29 ){ 	NewName = "armor";				itemID = 0x277D; }

				else if ( info.ButtonID == 30 )
				{
													NewName = "mantle";				itemID = 0x1F03;
					if ( from.Female ){ 			NewName = "mantle";				itemID = 0x1F04; }
				}
				else if ( info.ButtonID == 31 ){ 	NewName = "scant mantle";		itemID = 0x1515; }
				else if ( info.ButtonID == 32 ){ 	NewName = "robe";				itemID = 0x1F03; }

				if ( itemID > 0 && NewName != "" )
				{
					ChangeItem( m_Item, itemID, NewName, from );
				}
			}
		}

		public static void OrientalRobe( Item item, Mobile from )
		{
			if ( from.Female && item.ItemID == 0x5652 ){ item.ItemID = 0x563E; }
			else if ( !from.Female && item.ItemID == 0x563E ){ item.ItemID = 0x5652; }
		}

		public static void ChangeItem( Item item, int itemID, string NewName, Mobile from )
		{
			string material = Server.Misc.MaterialInfo.GetSpecialMaterialName( item );

			if ( material == "" )
			{
				if ( Server.Misc.MaterialInfo.IsAnyKindOfMetalItem( item ) ){ material = "metal "; }
				else if ( Server.Misc.MaterialInfo.IsAnyKindOfWoodItem( item ) ){ material = "wooden "; }
				else if ( Server.Misc.MaterialInfo.IsLeatherItem( item ) ){ material = "leather "; }
			}

			if ( item is BaseArmor )
			{
				if ( item.ItemID == 7026 && material == "wooden" ){ item.Hue = 0xAC0; }
				else if ( item.ItemID == 7035 && material == "wooden" ){ item.Hue = 0xABF; }
				else if ( ((BaseArmor)item).Resource == CraftResource.Iron && item.Hue == 0 ){ item.Hue = 0xB70; }
				else if ( ((BaseArmor)item).Resource == CraftResource.RegularLeather && item.Hue == 0 ){ item.Hue = 0xABE; }
				else if ( ((BaseArmor)item).Resource == CraftResource.RegularWood && item.Hue == 0 ){ item.Hue = 0xABE; }
			}

			NewName = material + NewName;

			item.Name = GetRandomOriental() + " " + NewName;
				if ( Utility.RandomBool() ){ item.Name = NewName + " " + GetRandomSamurai(); }

			item.ItemID = itemID;

			from.SendSound( 0x55 );
			from.SendMessage( "The item has been changed." );
		}

		public static string GetRandomOriental()
		{
			string name = "oriental";

			switch( Utility.RandomMinMax( 0, 10 ) )
			{
				case 0: name = "oriental"; break;
				case 1: name = "samurai"; break;
				case 2: name = "tokuno"; break;
				case 3: name = "shogun"; break;
				case 4: name = "ronin"; break;
				case 5: name = "bushido"; break;
				case 6: name = "imperial"; break;
				case 7: name = "jade"; break;
				case 8: name = "dragon"; break;
				case 9: name = "lotus"; break;
				case 10: name = "crane"; break;
			}

			return name;
		}

		public static string GetRandomSamurai()
		{
			string name = "of the samurai";

			switch( Utility.RandomMinMax( 0, 10 ) )
			{
				case 0: name = "of the samurai"; break;
				case 1: name = "of tokuno"; break;
				case 2: name = "of the shogun"; break;
				case 3: name = "of the ronin"; break;
				case 4: name = "of bushido"; break;
				case 5: name = "of the empire"; break;
				case 6: name = "of the jade court"; break;
				case 7: name = "of the dragon"; break;
				case 8: name = "of the lotus"; break;
				case 9: name = "of the crane"; break;
				case 10: name = "of the dojo"; break;
			}

			return name;
		}

	}
}