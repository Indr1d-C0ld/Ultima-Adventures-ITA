using System;
using System.Collections;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.BulkOrders
{
	public class BOBFilterGump : Gump
	{
		private PlayerMobile m_From;
		private BulkOrderBook m_Book;

		private const int LabelColor = 0x7FFF;
		private static int[,] m_MaterialFilters = new int[,]
			{
				{ 1044067,  1 }, // Blacksmithy
				{ 1044094,  2 }, // Tailoring
				{       0, 17 }, // Carpentry
				{       0, 18 }, // Fletching

				{ 1044286, 12 }, // Cloth
				{ 1062235, 13 }, // Leather
				{ 1062236, 14 }, // Spined
				{ 1062237, 15 }, // Horned
				{ 1062238, 16 }, // Barbed
				{       0, 34 }, // Necrotic
				{       0, 35 }, // Volcanic
				{       0, 36 }, // Frozen
				{       0, 37 }, // Goliath
				{       0, 38 }, // Draconic
				{       0, 39 }, // Hellish
				{       0, 40 }, // Dinosaur
				{       0, 41 }, // Alien

				{ 1072643, 19 }, // Regular
				{ 1095379, 20 }, // Ash
				{ 1095380, 21 }, // Cherry
				{ 1095381, 22 }, // Ebony
				{ 1095382, 23 }, // Golden Oak
				{ 1095383, 24 }, // Hickory
				{ 1095384, 25 }, // Mahogany
				{ 1095385, 26 }, // Oak
				{ 1095386, 27 }, // Pine
				{ 1095387, 28 }, // Rosewood
				{ 1095388, 29 }, // Walnut
				{ 1095409, 30 }, // Driftwood
				{ 1095511, 31 }, // Ghostwood
				{ 1095532, 32 }, // Petrified
				{ 1095535, 33 }, // Elven

				{ 1062226,  3 }, // Iron
				{ 1018332,  4 }, // Dull Copper
				{ 1018333,  5 }, // Shadow Iron
				{ 1018334,  6 }, // Copper
				{ 1018335,  7 }, // Bronze
				{ 1018336,  8 }, // Golden
				{ 1018337,  9 }, // Agapite
				{ 1018338, 10 }, // Verite
				{ 1018339, 11 }, // Valorite
				{       0, 42 }, // Nepturite
				{       0, 43 }, // Obsidian
				{       0, 44 }, // Steel
				{       0, 45 }, // Brass
				{       0, 46 }, // Mithril
				{       0, 47 }, // Xormite
				{       0, 48 }  // Dwarven
			};

		private static int[,] m_TypeFilters = new int[,]
			{
				{ 1062229, 0 }, // All
				{ 1062224, 1 }, // Small
				{ 1062225, 2 }  // Large
			};

		private static int[,] m_QualityFilters = new int[,]
			{
				{ 1062229, 0 }, // All
				{ 1011542, 1 }, // Normal
				{ 1060636, 2 }  // Exceptional
			};

		private static int[,] m_AmountFilters = new int[,]
			{
				{ 1062229, 0 }, // All
				{ 1049706, 1 }, // 10
				{ 1016007, 2 }, // 15
				{ 1062239, 3 }  // 20
			};

		private static int[][,] m_Filters = new int[][,]
			{
				m_TypeFilters,
				m_QualityFilters,
				m_MaterialFilters,
				m_AmountFilters
			};

		private static int[] m_XOffsets_Type = new int[]{ 0, 85, 190 };
		private static int[] m_XOffsets_Quality = new int[]{ 0, 85, 190 };
		private static int[] m_XOffsets_Amount = new int[]{ 0, 85, 190, 295 };
		private static int[] m_XOffsets_MaterialType = new int[]{ 0, 160, 320, 480 };
		private static int[] m_XOffsets_Material = new int[]{ 0, 120, 240, 360, 480, 600 };

		private static int[] m_XWidths_Small = new int[]{ 60, 60, 80, 60 };
		private static int[] m_XWidths_MaterialType = new int[]{ 120, 120, 120, 120 };
		private static int[] m_XWidths_Large = new int[]{ 100, 80, 80, 80, 80, 80 };

		private string GetCustomMaterialLabel( int filterValue )
		{
			switch ( filterValue )
			{
				case 17: return "Carpentry";
				case 18: return "Fletching";
				case 34: return "Necrotic";
				case 35: return "Volcanic";
				case 36: return "Frozen";
				case 37: return "Goliath";
				case 38: return "Draconic";
				case 39: return "Hellish";
				case 40: return "Dinosaur";
				case 41: return "Alien";
				case 19: return "Regular";
				case 20: return "Ash";
				case 21: return "Cherry";
				case 22: return "Ebony";
				case 23: return "Golden Oak";
				case 24: return "Hickory";
				case 25: return "Mahogany";
				case 26: return "Oak";
				case 27: return "Pine";
				case 28: return "Rosewood";
				case 29: return "Walnut";
				case 30: return "Driftwood";
				case 31: return "Ghostwood";
				case 32: return "Petrified";
				case 33: return "Elven";
				case 42: return "Nepturite";
				case 43: return "Obsidian";
				case 44: return "Steel";
				case 45: return "Brass";
				case 46: return "Mithril";
				case 47: return "Xormite";
				case 48: return "Dwarven";
				default: return null;
			}
		}

		private void AddFilterList( int x, int y, int[] xOffsets, int yOffset, int[,] filters, int[] xWidths, int filterValue, int filterIndex, int startIndex, int count )
		{
			for ( int i = 0; i < count; ++i )
			{
				int entryIndex = startIndex + i;
				int number = filters[entryIndex, 0];
				string customLabel = GetCustomMaterialLabel( filters[entryIndex, 1] );

				if ( number == 0 && customLabel == null )
					continue;

				bool isSelected = ( filters[entryIndex, 1] == filterValue );

				if ( !isSelected && (i % xOffsets.Length) == 0 )
					isSelected = ( filterValue == 0 );

				if ( customLabel != null )
				{
						string text = string.Format( "<BODY><BASEFONT Color=#FFFFFF><BIG>{0}</BIG></BASEFONT></BODY>", customLabel );
						AddHtml( x + 35 + xOffsets[i % xOffsets.Length], y + ((i / xOffsets.Length) * yOffset), xWidths[i % xOffsets.Length], 32, text, false, false );
				}
				else
				{
					AddHtmlLocalized( x + 35 + xOffsets[i % xOffsets.Length], y + ((i / xOffsets.Length) * yOffset), xWidths[i % xOffsets.Length], 32, number, LabelColor, false, false );
				}
				AddButton( x + xOffsets[i % xOffsets.Length], y + ((i / xOffsets.Length) * yOffset), isSelected ? 4006 : 4005, 4007, 4 + filterIndex + (entryIndex * 4), GumpButtonType.Reply, 0 );
			}
		}

		public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
		{
			BOBFilter f = ( m_From.UseOwnFilter ? m_From.BOBFilter : m_Book.Filter );

			int index = info.ButtonID;

			switch ( index )
			{
				case 0: // Apply
				{
					m_From.SendGump( new BOBGump( m_From, m_Book ) );

					break;
				}
				case 1: // Set Book Filter
				{
					m_From.UseOwnFilter = false;
					m_From.SendGump( new BOBFilterGump( m_From, m_Book ) );

					break;
				}
				case 2: // Set Your Filter
				{
					m_From.UseOwnFilter = true;
					m_From.SendGump( new BOBFilterGump( m_From, m_Book ) );

					break;
				}
				case 3: // Clear Filter
				{
					f.Clear();
					m_From.SendGump( new BOBFilterGump( m_From, m_Book ) );

					break;
				}
				default:
				{
					index -= 4;

					int type = index % 4;
					index /= 4;

					if ( type >= 0 && type < m_Filters.Length )
					{
						int[,] filters = m_Filters[type];

						if ( index >= 0 && index < filters.GetLength( 0 ) )
						{
							if ( filters[index, 0] == 0 && GetCustomMaterialLabel( filters[index, 1] ) == null )
								break;

							switch ( type )
							{
								case 0: f.Type = filters[index, 1]; break;
								case 1: f.Quality = filters[index, 1]; break;
								case 2: f.Material = filters[index, 1]; break;
								case 3: f.Quantity = filters[index, 1]; break;
							}

							m_From.SendGump( new BOBFilterGump( m_From, m_Book ) );
						}
					}

					break;
				}
			}
		}

		public BOBFilterGump( PlayerMobile from, BulkOrderBook book ) : base( 12, 24 )
		{
			from.CloseGump( typeof( BOBGump ) );
			from.CloseGump( typeof( BOBFilterGump ) );

			m_From = from;
			m_Book = book;

			BOBFilter f = ( from.UseOwnFilter ? from.BOBFilter : book.Filter );

			AddPage( 0 );

			AddBackground( 10, 10, 760, 880, 5054 );

			AddImageTiled( 18, 20, 743, 861, 2624 );
			AddAlphaRegion( 18, 20, 743, 861 );

			AddImage( 5, 5, 10460 );
			AddImage( 745, 5, 10460 );
			AddImage( 5, 865, 10460 );
			AddImage( 745, 865, 10460 );

			AddHtmlLocalized( 350, 32, 200, 32, 1062223, LabelColor, false, false ); // Filter Preference

			AddHtmlLocalized( 26, 64, 120, 32, 1062228, LabelColor, false, false ); // Bulk Order Type
			AddFilterList( 25, 96, m_XOffsets_Type, 40, m_TypeFilters, m_XWidths_Small, f.Type, 0, 0, m_TypeFilters.GetLength( 0 ) );

			AddHtmlLocalized( 400, 64, 50, 32, 1062215, LabelColor, false, false ); // Quality
			AddFilterList( 400, 96, m_XOffsets_Quality, 40, m_QualityFilters, m_XWidths_Small, f.Quality, 1, 0, m_QualityFilters.GetLength( 0 ) );

			AddHtmlLocalized( 26, 160, 120, 32, 1062232, LabelColor, false, false ); // Material Type
			AddHtml( 26, 188, 120, 32, "<BODY><BASEFONT Color=#FFFFFF><BIG>BOD Type</BIG></BASEFONT></BODY>", false, false );
			AddFilterList( 25, 216, m_XOffsets_MaterialType, 40, m_MaterialFilters, m_XWidths_MaterialType, f.Material, 2, 0, 4 );
			AddImageTiled( 25, 246, 720, 1, 9304 );
			AddHtml( 26, 252, 120, 32, "<BODY><BASEFONT Color=#FFFFFF><BIG>Material Type</BIG></BASEFONT></BODY>", false, false );
			AddHtml( 26, 276, 160, 32, "<BODY><BASEFONT Color=#FFFFFF><BIG>Cloth/Leather</BIG></BASEFONT></BODY>", false, false );
			AddFilterList( 25, 304, m_XOffsets_Material, 40, m_MaterialFilters, m_XWidths_Large, f.Material, 2, 4, 13 );
			AddImageTiled( 25, 424, 720, 1, 9304 );
			AddHtml( 26, 430, 120, 32, "<BODY><BASEFONT Color=#FFFFFF><BIG>Wood</BIG></BASEFONT></BODY>", false, false );
			AddFilterList( 25, 458, m_XOffsets_Material, 40, m_MaterialFilters, m_XWidths_Large, f.Material, 2, 17, 15 );
			AddImageTiled( 25, 578, 720, 1, 9304 );
			AddHtml( 26, 584, 120, 32, "<BODY><BASEFONT Color=#FFFFFF><BIG>Metals</BIG></BASEFONT></BODY>", false, false );
			AddFilterList( 25, 612, m_XOffsets_Material, 40, m_MaterialFilters, m_XWidths_Large, f.Material, 2, 32, 16 );
			AddImageTiled( 25, 732, 720, 1, 9304 );

			AddHtmlLocalized( 26, 740, 120, 32, 1062217, LabelColor, false, false ); // Amount
			AddFilterList( 25, 768, m_XOffsets_Amount, 40, m_AmountFilters, m_XWidths_Small, f.Quantity, 3, 0, m_AmountFilters.GetLength( 0 ) );

			AddHtmlLocalized( 75, 808, 120, 32, 1062477, LabelColor, false, false ); // Set Book Filter
			AddButton( 40, 808, 4005, 4007, 1, GumpButtonType.Reply, 0 );

			AddHtmlLocalized( 235, 808, 120, 32, 1062478, LabelColor, false, false ); // Set Your Filter
			AddButton( 200, 808, 4005, 4007, 2, GumpButtonType.Reply, 0 );

			AddHtmlLocalized( 485, 808, 120, 32, 1062231, LabelColor, false, false ); // Clear Filter
			AddButton( 450, 808, 4005, 4007, 3, GumpButtonType.Reply, 0 );

			AddHtmlLocalized( 620, 808, 50, 32, 1011046, LabelColor, false, false ); // APPLY
			AddButton( 585, 808, 4017, 4018, 0, GumpButtonType.Reply, 0 );
		}
	}
}
