using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Server.Gumps;
using Server.Network;
using Server.Items;
using Server.Targeting;

namespace Server.Engines.Craft
{
        public enum CraftPage
        {
                None,
                PickResource,
                PickResource2
        }

        public class CraftGump : Gump
        {
                private Mobile m_From;
                private CraftSystem m_CraftSystem;
                private BaseTool m_Tool;

                private CraftPage m_Page;
                private readonly string m_SearchTerm;
                private readonly List<CraftItem> m_SearchResults;

                private const int LabelHue = 0x480;
                private const int LabelColor = 0x7FFF;
                private const int FontColor = 0xFFFFFF;
                private const int ButtonsPerIndex = 12;
                private const int SearchTextEntryID = 1;

		private const int LastTenGroupIndex = 501;
		private const int FavoritesGroupIndex = 502;

		/*public CraftGump( Mobile from, CraftSystem craftSystem, BaseTool tool ): this( from, craftSystem, -1, -1, tool, null )
		{
		}*/

		public CraftGump( Mobile from, CraftSystem craftSystem, BaseTool tool, object notice ) : this( from, craftSystem, tool, notice, CraftPage.None, null )
		{
		}

               public CraftGump( Mobile from, CraftSystem craftSystem, BaseTool tool, object notice, CraftPage page ) : this( from, craftSystem, tool, notice, page, null )
               {
               }

               public CraftGump( Mobile from, CraftSystem craftSystem, BaseTool tool, object notice, CraftPage page, string searchTerm ) : base( 40, 40 )
               {
                       m_From = from;
                       m_CraftSystem = craftSystem;
                       m_Tool = tool;
                       m_Page = page;
			m_SearchTerm = String.IsNullOrWhiteSpace( searchTerm ) ? null : searchTerm.Trim();

			CraftContext context = craftSystem.GetContext( from );
			m_SearchResults = ( m_SearchTerm == null ? null : BuildSearchResults( m_SearchTerm ) );

			from.CloseGump( typeof( CraftGump ) );
			from.CloseGump( typeof( CraftGumpItem ) );

			AddPage( 0 );

                       AddBackground(0, 0, 530, 615, 5054);
                       AddImageTiled(10, 10, 510, 75, 2624);
                       AddImageTiled(10, 90, 200, 313, 2624);
                       AddImageTiled(215, 90, 305, 313, 2624);
                       AddImageTiled(10, 408, 150, 60, 2624);
                       AddImageTiled(165, 408, 355, 60, 2624);
                       AddImageTiled(10, 473, 510, 140, 2624);
                       AddAlphaRegion(10, 10, 510, 603);

                        if ( craftSystem.GumpTitleNumber > 0 )
                        AddHtmlLocalized( 10, 14, 510, 20, craftSystem.GumpTitleNumber, LabelColor, false, false );
                        else
                        AddHtml( 10, 14, 510, 20, craftSystem.GumpTitleString, false, false );

                        AddLabel( 220, 34, LabelHue, "SEARCH:" );
                        AddTextEntry( 275, 31, 170, 20, LabelHue, SearchTextEntryID, m_SearchTerm ?? String.Empty );
                        AddButton( 450, 31, 4005, 4007, GetButtonID( 9, 0 ), GumpButtonType.Reply, 0 );
                        AddLabel( 485, 34, LabelHue, "GO" );

                       AddHtmlLocalized( 10, 90, 200, 22, 1044010, LabelColor, false, false ); // <CENTER>CATEGORIES</CENTER>
                       AddHtmlLocalized( 215, 90, 305, 22, 1044011, LabelColor, false, false ); // <CENTER>SELECTIONS</CENTER>
                       AddHtmlLocalized( 10, 430, 150, 25, 1044012, LabelColor, false, false ); // <CENTER>NOTICES</CENTER>

                        const int buttonBottomY = 580;
			const int buttonSpacing = 24;
			const int leftButtonX = 15;
			const int leftLabelX = 50;
			const int rightButtonX = 270;
			const int rightLabelX = 305;

			var leftButtons = new List<Action<int>>();
			var rightButtons = new List<Action<int>>();

			bool isBlacksmith = craftSystem is DefBlacksmithy;

			Action<Action<int>> queueLeft = action => leftButtons.Add(action);
			Action<Action<int>> queueRight = action => rightButtons.Add(action);

			// Mark option
			if ( craftSystem.MarkOption )
			{
				queueRight( y =>
				{
					AddButton( rightButtonX, y, 4005, 4007, GetButtonID( 6, 6 ), GumpButtonType.Reply, 0 );
					AddHtmlLocalized( rightLabelX, y + 3, 150, 18, 1044017 + (context == null ? 0 : (int)context.MarkOption), LabelColor, false, false ); // MARK ITEM
				} );
			}
			// ****************************************

			// Resmelt option
			if ( craftSystem.Resmelt )
			{
				Action<Action<int>> resmeltQueue = isBlacksmith ? queueRight : queueLeft;

				resmeltQueue( y =>
				{
					int buttonX = isBlacksmith ? rightButtonX : leftButtonX;
					int labelX = isBlacksmith ? rightLabelX : leftLabelX;

					AddButton( buttonX, y, 4005, 4007, GetButtonID( 6, 1 ), GumpButtonType.Reply, 0 );
					AddHtmlLocalized( labelX, y + 3, 150, 18, 1044259, LabelColor, false, false ); // SMELT ITEM
				} );
			}
			// ****************************************

			// Repair option
			if ( craftSystem.Repair )
			{
				queueRight( y =>
				{
					AddButton( rightButtonX, y, 4005, 4007, GetButtonID( 6, 5 ), GumpButtonType.Reply, 0 );
					AddHtmlLocalized( rightLabelX, y + 3, 150, 18, 1044260, LabelColor, false, false ); // REPAIR ITEM
				} );
			}
			// ****************************************

			// Enhance option
			if ( craftSystem.CanEnhance )
			{
				queueRight( y =>
				{
					AddButton( rightButtonX, y, 4005, 4007, GetButtonID( 6, 8 ), GumpButtonType.Reply, 0 );
					AddHtmlLocalized( rightLabelX, y + 3, 150, 18, 1061001, LabelColor, false, false ); // ENHANCE ITEM
				} );
			}
			// ****************************************

			queueLeft( y =>
			{
				AddButton( leftButtonX, y, 4005, 4007, GetButtonID( 6, 9 ), GumpButtonType.Reply, 0 );
				AddLabel( leftLabelX, y + 3, LabelHue, "SKILL CRAFT LIST" );
			} );

			queueLeft( y =>
			{
				AddButton( leftButtonX, y, 4005, 4007, GetButtonID( 6, 10 ), GumpButtonType.Reply, 0 );
				AddLabel( leftLabelX, y + 3, LabelHue, "SET OUTPUT" );
			} );

                        if ( notice is int && (int)notice > 0 )
                        AddHtmlLocalized( 170, 411, 350, 40, (int)notice, LabelColor, false, false );
                        else if ( notice is string )
                        AddHtml( 170, 411, 350, 40, String.Format( "<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", FontColor, notice ), false, false );

			// If the system has more than one resource
			if ( craftSystem.CraftSubRes.Init )
			{
				string nameString = craftSystem.CraftSubRes.NameString;
				int nameNumber = craftSystem.CraftSubRes.NameNumber;

				int resIndex = ( context == null ? -1 : context.LastResourceIndex );

				Type resourceType = craftSystem.CraftSubRes.ResType;

				if ( resIndex > -1 )
				{
					CraftSubRes subResource = craftSystem.CraftSubRes.GetAt( resIndex );

					nameString = subResource.NameString;
					nameNumber = subResource.NameNumber;
					resourceType = subResource.ItemType;
				}

				int resourceCount = 0;

				if ( from.Backpack != null )
				{
					Item[] items = from.Backpack.FindItemsByType( resourceType, true );

					for ( int i = 0; i < items.Length; ++i )
					resourceCount += items[i].Amount;
				}

				int localNameNumber = nameNumber;
				string localNameString = nameString;
				int localResourceCount = resourceCount;

				queueLeft( y =>
				{
					AddButton( leftButtonX, y, 4005, 4007, GetButtonID( 6, 0 ), GumpButtonType.Reply, 0 );

					if ( localNameNumber > 0 )
					AddHtmlLocalized( leftLabelX, y + 3, 250, 18, localNameNumber, localResourceCount.ToString(), LabelColor, false, false );
					else
					AddLabel( leftLabelX, y, LabelHue, String.Format( "{0} ({1} Available)", localNameString, localResourceCount ) );
				} );
			}
			// ****************************************

			// For dragon scales
			if ( craftSystem.CraftSubRes2.Init )
			{
				string nameString = craftSystem.CraftSubRes2.NameString;
				int nameNumber = craftSystem.CraftSubRes2.NameNumber;

				int resIndex = ( context == null ? -1 : context.LastResourceIndex2 );

				Type resourceType = craftSystem.CraftSubRes2.ResType;

				if ( resIndex > -1 )
				{
					CraftSubRes subResource = craftSystem.CraftSubRes2.GetAt( resIndex );

					nameString = subResource.NameString;
					nameNumber = subResource.NameNumber;
					resourceType = subResource.ItemType;
				}

				int resourceCount = 0;

				if ( from.Backpack != null )
				{
					Item[] items = from.Backpack.FindItemsByType( resourceType, true );

					for ( int i = 0; i < items.Length; ++i )
					resourceCount += items[i].Amount;
				}

				int localNameNumber2 = nameNumber;
				string localNameString2 = nameString;
				int localResourceCount2 = resourceCount;

				queueLeft( y =>
				{
					AddButton( leftButtonX, y, 4005, 4007, GetButtonID( 6, 7 ), GumpButtonType.Reply, 0 );

					if ( localNameNumber2 > 0 )
					AddHtmlLocalized( leftLabelX, y + 3, 250, 18, localNameNumber2, localResourceCount2.ToString(), LabelColor, false, false );
					else
					AddLabel( leftLabelX, y, LabelHue, String.Format( "{0} ({1} Available)", localNameString2, localResourceCount2 ) );
				} );
			}
			// ****************************************

			for ( int i = 0; i < leftButtons.Count; ++i )
			{
				int y = buttonBottomY - (buttonSpacing * (leftButtons.Count - i));
				leftButtons[i]( y );
			}

			for ( int i = 0; i < rightButtons.Count; ++i )
			{
				int y = buttonBottomY - (buttonSpacing * (rightButtons.Count - i));
				rightButtons[i]( y );
			}

			AddButton( leftButtonX, buttonBottomY, 4017, 4019, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( leftLabelX, buttonBottomY + 3, 150, 18, 1011441, LabelColor, false, false ); // EXIT

			AddButton( rightButtonX, buttonBottomY, 4005, 4007, GetButtonID( 6, 2 ), GumpButtonType.Reply, 0 );
			AddHtmlLocalized( rightLabelX, buttonBottomY + 3, 150, 18, 1044013, LabelColor, false, false ); // MAKE LAST

			CreateGroupList();

                       if ( page == CraftPage.PickResource )
                       CreateResList( false, from );
                       else if ( page == CraftPage.PickResource2 )
                       CreateResList( true, from );
                       else if ( m_SearchResults != null )
                       CreateItemList( context != null ? context.LastGroupIndex : -1 );
                       else if ( context != null && context.LastGroupIndex > -1 )
                       CreateItemList( context.LastGroupIndex );
		}

		public void CreateResList( bool opt, Mobile from )
		{
			CraftSubResCol res = ( opt ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes );

			for ( int i = 0; i < res.Count; ++i )
			{
				int index = i % 10;

				CraftSubRes subResource = res.GetAt( i );

				if ( index == 0 )
				{
					if ( i > 0 )
					AddButton( 485, 321, 4005, 4007, 0, GumpButtonType.Page, (i / 10) + 1 );

					AddPage( (i / 10) + 1 );

					if ( i > 0 )
					AddButton( 455, 321, 4014, 4015, 0, GumpButtonType.Page, i / 10 );

					CraftContext context = m_CraftSystem.GetContext( m_From );

					AddButton( 220, 321, 4005, 4007, GetButtonID( 6, 4 ), GumpButtonType.Reply, 0 );
					AddHtmlLocalized( 255, 324, 200, 18, (context == null || !context.DoNotColor) ? 1061591 : 1061590, LabelColor, false, false );
				}

				int resourceCount = 0;

				if ( from.Backpack != null )
				{
					Item[] items = from.Backpack.FindItemsByType( subResource.ItemType, true );

					for ( int j = 0; j < items.Length; ++j )
					resourceCount += items[j].Amount;
				}

                                int rowY = 114 + (index * 20);
				string skillValue = subResource.RequiredSkill.ToString( "0.0", CultureInfo.InvariantCulture );
				string skillText = String.Format( "[{0}]", skillValue );

				AddButton( 220, rowY, 4005, 4007, GetButtonID( 5, i ), GumpButtonType.Reply, 0 );

				if ( subResource.NameNumber > 0 )
				AddHtmlLocalized( 255, rowY + 3, 160, 18, subResource.NameNumber, resourceCount.ToString(), LabelColor, false, false );
				else
				{
					string name = subResource.NameString;

					if ( String.IsNullOrEmpty( name ) )
					name = subResource.ItemType != null ? subResource.ItemType.Name : String.Empty;

					AddLabel( 255, rowY, LabelHue, String.Format( "{0} ({1})", name, resourceCount ) );
				}

                                AddLabel( 430, rowY, LabelHue, skillText );
                        }
                }


		public void CreateMakeLastList()
		{
			CraftContext context = m_CraftSystem.GetContext( m_From );

			if ( context == null )
			return;

			List<CraftItem> items = context.Items;

			if ( items.Count > 0 )
			{
				for ( int i = 0; i < items.Count; ++i )
				{
					int index = i % 10;

					CraftItem craftItem = items[i];

					if ( index == 0 )
					{
						if ( i > 0 )
						{
							AddButton( 370, 321, 4005, 4007, 0, GumpButtonType.Page, (i / 10) + 1 );
							AddHtmlLocalized( 405, 324, 100, 18, 1044045, LabelColor, false, false ); // NEXT PAGE
						}

						AddPage( (i / 10) + 1 );

						if ( i > 0 )
						{
							AddButton( 220, 321, 4014, 4015, 0, GumpButtonType.Page, i / 10 );
							AddHtmlLocalized( 255, 324, 100, 18, 1044044, LabelColor, false, false ); // PREV PAGE
						}
					}

                                        AddButton( 220, 114 + (index * 20), 4005, 4007, GetButtonID( 3, i ), GumpButtonType.Reply, 0 );

                                        if ( craftItem.NameNumber > 0 )
                                        AddHtmlLocalized( 255, 117 + (index * 20), 220, 18, craftItem.NameNumber, LabelColor, false, false );
                                        else
                                        AddLabel( 255, 114 + (index * 20), LabelHue, craftItem.NameString );

                                        AddButton( 480, 114 + (index * 20), 4011, 4012, GetButtonID( 4, i ), GumpButtonType.Reply, 0 );
                                }
                        }
                        else
                        {
                                // NOTE: This is not as OSI; it is an intentional difference

                                AddHtmlLocalized( 230, 116, 200, 22, 1044165, LabelColor, false, false ); // You haven't made anything yet.
                        }
                }

		public void CreateItemList( int selectedGroup )
		{
			CraftContext context = m_CraftSystem.GetContext( m_From );

			if ( m_SearchResults != null )
			{
				CreateSearchList();
				return;
			}

			if ( selectedGroup == LastTenGroupIndex ) // 501 : Last 10
			{
				CreateMakeLastList();
				return;
			}

			if ( selectedGroup == FavoritesGroupIndex )
			{
				CreateFavoritesList();
				return;
			}

			CraftGroupCol craftGroupCol = m_CraftSystem.CraftGroups;
			CraftGroup craftGroup = craftGroupCol.GetAt( selectedGroup );
			CraftItemCol craftItemCol = craftGroup.CraftItems;

			for ( int i = 0; i < craftItemCol.Count; ++i )
			{
				int index = i % 10;

				CraftItem craftItem = craftItemCol.GetAt( i );

				if ( index == 0 )
				{
					if ( i > 0 )
					{
						AddButton( 370, 321, 4005, 4007, 0, GumpButtonType.Page, (i / 10) + 1 );
						AddHtmlLocalized( 405, 324, 100, 18, 1044045, LabelColor, false, false ); // NEXT PAGE
					}

					AddPage( (i / 10) + 1 );

					if ( i > 0 )
					{
						AddButton( 220, 321, 4014, 4015, 0, GumpButtonType.Page, i / 10 );
						AddHtmlLocalized( 255, 324, 100, 18, 1044044, LabelColor, false, false ); // PREV PAGE
					}
				}

                                AddButton( 220, 114 + (index * 20), 4005, 4007, GetButtonID( 1, i ), GumpButtonType.Reply, 0 );

                                if ( craftItem.NameNumber > 0 )
                                AddHtmlLocalized( 255, 117 + (index * 20), 220, 18, craftItem.NameNumber, LabelColor, false, false );
                                else
                                AddLabel( 255, 114 + (index * 20), LabelHue, craftItem.NameString );

                                AddButton( 480, 114 + (index * 20), 4011, 4012, GetButtonID( 2, i ), GumpButtonType.Reply, 0 );
                        }
                }

		public void CreateFavoritesList()
		{
			CraftContext context = m_CraftSystem.GetContext( m_From );

			if ( context == null || context.Favorites.Count == 0 )
			{
                                AddLabel( 230, 116, LabelHue, "No favorites yet." );
				return;
			}

			List<CraftItem> favorites = context.Favorites;

			for ( int i = 0; i < favorites.Count; ++i )
			{
				int index = i % 10;

				CraftItem craftItem = favorites[i];

				if ( craftItem == null )
				continue;

				if ( index == 0 )
				{
					if ( i > 0 )
					{
						AddButton( 370, 321, 4005, 4007, 0, GumpButtonType.Page, (i / 10) + 1 );
						AddHtmlLocalized( 405, 324, 100, 18, 1044045, LabelColor, false, false ); // NEXT PAGE
					}

					AddPage( (i / 10) + 1 );

					if ( i > 0 )
					{
						AddButton( 220, 321, 4014, 4015, 0, GumpButtonType.Page, i / 10 );
						AddHtmlLocalized( 255, 324, 100, 18, 1044044, LabelColor, false, false ); // PREV PAGE
					}
				}

                                AddButton( 220, 114 + (index * 20), 4005, 4007, GetButtonID( 7, i ), GumpButtonType.Reply, 0 );

                                if ( craftItem.NameNumber > 0 )
                                AddHtmlLocalized( 255, 117 + (index * 20), 220, 18, craftItem.NameNumber, LabelColor, false, false );
                                else
                                AddLabel( 255, 114 + (index * 20), LabelHue, craftItem.NameString );

                                AddButton( 480, 114 + (index * 20), 4011, 4012, GetButtonID( 8, i ), GumpButtonType.Reply, 0 );
                        }
                }


		private List<CraftItem> BuildSearchResults( string term )
		{
			List<CraftItem> results = new List<CraftItem>();
			HashSet<CraftItem> seen = new HashSet<CraftItem>();

			if ( String.IsNullOrEmpty( term ) )
			return results;

			CraftGroupCol craftGroupCol = m_CraftSystem.CraftGroups;

			for ( int i = 0; i < craftGroupCol.Count; i++ )
			{
				CraftGroup craftGroup = craftGroupCol.GetAt( i );

				for ( int j = 0; j < craftGroup.CraftItems.Count; j++ )
				{
					CraftItem craftItem = craftGroup.CraftItems.GetAt( j );

					if ( craftItem == null || !seen.Add( craftItem ) )
					continue;

					if ( MatchesSearch( craftItem, term ) )
					results.Add( craftItem );
				}
			}

			return results;
		}

		private bool MatchesSearch( CraftItem craftItem, string term )
		{
			if ( craftItem == null || String.IsNullOrEmpty( term ) )
			return false;

			string name = craftItem.NameString;

			if ( String.IsNullOrEmpty( name ) && craftItem.ItemType != null )
			name = craftItem.ItemType.Name;

			return !String.IsNullOrEmpty( name ) && name.IndexOf( term, StringComparison.OrdinalIgnoreCase ) >= 0;
		}

		public void CreateSearchList()
		{
			if ( m_SearchResults == null )
			return;

                       AddLabel( 220, 48, LabelHue, String.Format( "Results for \"{0}\" ({1})", m_SearchTerm, m_SearchResults.Count ) );

			if ( m_SearchResults.Count == 0 )
			{
                                AddLabel( 230, 116, LabelHue, "No matching recipes." );
				return;
			}

			for ( int i = 0; i < m_SearchResults.Count; ++i )
			{
				int index = i % 10;

				CraftItem craftItem = m_SearchResults[i];

				if ( index == 0 )
				{
					if ( i > 0 )
					{
						AddButton( 370, 321, 4005, 4007, 0, GumpButtonType.Page, (i / 10) + 1 );
						AddHtmlLocalized( 405, 324, 100, 18, 1044045, LabelColor, false, false ); // NEXT PAGE
					}

					AddPage( (i / 10) + 1 );

					if ( i > 0 )
					{
						AddButton( 220, 321, 4014, 4015, 0, GumpButtonType.Page, i / 10 );
						AddHtmlLocalized( 255, 324, 100, 18, 1044044, LabelColor, false, false ); // PREV PAGE
					}
				}

                                AddButton( 220, 114 + (index * 20), 4005, 4007, GetButtonID( 10, i ), GumpButtonType.Reply, 0 );

                                if ( craftItem.NameNumber > 0 )
                                AddHtmlLocalized( 255, 117 + (index * 20), 220, 18, craftItem.NameNumber, LabelColor, false, false );
                                else
                                AddLabel( 255, 114 + (index * 20), LabelHue, craftItem.NameString );

                                AddButton( 480, 114 + (index * 20), 4011, 4012, GetButtonID( 11, i ), GumpButtonType.Reply, 0 );
                        }
                }

		public int CreateGroupList()
		{
			CraftGroupCol craftGroupCol = m_CraftSystem.CraftGroups;

                        const int categoryListY = 114;

                        AddButton( 15, categoryListY, 4005, 4007, GetButtonID( 6, 3 ), GumpButtonType.Reply, 0 );
                        AddHtmlLocalized( 50, categoryListY + 3, 150, 18, 1044014, LabelColor, false, false ); // LAST TEN

                        AddButton( 15, categoryListY + 20, 4005, 4007, GetButtonID( 6, 11 ), GumpButtonType.Reply, 0 );
                        AddLabel( 50, categoryListY + 23, LabelHue, "FAVORITES" );

                        int startY = categoryListY + 40;

                        for ( int i = 0; i < craftGroupCol.Count; i++ )
                        {
                                CraftGroup craftGroup = craftGroupCol.GetAt( i );

                                AddButton( 15, startY + (i * 20), 4005, 4007, GetButtonID( 0, i ), GumpButtonType.Reply, 0 );

                                if ( craftGroup.NameNumber > 0 )
                                AddHtmlLocalized( 50, startY + 3 + (i * 20), 150, 18, craftGroup.NameNumber, LabelColor, false, false );
                                else
                                AddLabel( 50, startY + 3 + (i * 20), LabelHue, craftGroup.NameString );
                        }

			return craftGroupCol.Count;
		}


		public static int GetButtonID( int type, int index )
		{
			return 1 + type + (index * ButtonsPerIndex);
		}

		public void CraftItem( CraftItem item )
		{
			int num = m_CraftSystem.CanCraft( m_From, m_Tool, item.ItemType );

			if ( num > 0 )
			{
				m_From.SendGump( new CraftGump( m_From, m_CraftSystem, m_Tool, num, m_Page, m_SearchTerm ) );
			}
			else
			{
				Type type = null;

				CraftContext context = m_CraftSystem.GetContext( m_From );

				if ( context != null )
				{
					CraftSubResCol res = ( item.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes );
					int resIndex = ( item.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex );

					if ( resIndex >= 0 && resIndex < res.Count )
					type = res.GetAt( resIndex ).ItemType;
				}

				m_CraftSystem.CreateItem( m_From, item.ItemType, type, m_Tool, item );
			}
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( info.ButtonID <= 0 )
			return; // Canceled

			int buttonID = info.ButtonID - 1;
			int type = buttonID % ButtonsPerIndex;
			int index = buttonID / ButtonsPerIndex;

			CraftSystem system = m_CraftSystem;
			CraftGroupCol groups = system.CraftGroups;
			CraftContext context = system.GetContext( m_From );

			switch ( type )
			{
				case 0: // Show group
				{
					if ( context == null )
					break;

					if ( index >= 0 && index < groups.Count )
					{
						context.LastGroupIndex = index;
						m_From.SendGump( new CraftGump( m_From, system, m_Tool, null ) );
					}

					break;
				}
				case 1: // Create item
				{
					if ( context == null )
					break;

					int groupIndex = context.LastGroupIndex;

					if ( groupIndex >= 0 && groupIndex < groups.Count )
					{
						CraftGroup group = groups.GetAt( groupIndex );

						if ( index >= 0 && index < group.CraftItems.Count )
						CraftItem( group.CraftItems.GetAt( index ) );
					}

					break;
				}
				case 2: // Item details
				{
					if ( context == null )
					break;

					int groupIndex = context.LastGroupIndex;

					if ( groupIndex >= 0 && groupIndex < groups.Count )
					{
						CraftGroup group = groups.GetAt( groupIndex );

						if ( index >= 0 && index < group.CraftItems.Count )
						m_From.SendGump( new CraftGumpItem( m_From, system, group.CraftItems.GetAt( index ), m_Tool, m_SearchTerm ) );
					}

					break;
				}
				case 3: // Create item (last 10)
				{
					if ( context == null )
					break;

					List<CraftItem> lastTen = context.Items;

					if ( index >= 0 && index < lastTen.Count )
					CraftItem( lastTen[index] );

					break;
				}
				case 4: // Item details (last 10)
				{
					if ( context == null )
					break;

					List<CraftItem> lastTen = context.Items;

					if ( index >= 0 && index < lastTen.Count )
					m_From.SendGump( new CraftGumpItem( m_From, system, lastTen[index], m_Tool, m_SearchTerm ) );

					break;
				}
				case 5: // Resource selected
				{
					if ( m_Page == CraftPage.PickResource && index >= 0 && index < system.CraftSubRes.Count )
					{
						int groupIndex = ( context == null ? -1 : context.LastGroupIndex );

						CraftSubRes res = system.CraftSubRes.GetAt( index );

						if ( m_From.Skills[system.MainSkill].Value < res.RequiredSkill )
						{
							m_From.SendGump( new CraftGump( m_From, system, m_Tool, res.Message, CraftPage.None, m_SearchTerm ) );
						}
						else
						{
							if ( context != null )
							context.LastResourceIndex = index;

							m_From.SendGump( new CraftGump( m_From, system, m_Tool, null, CraftPage.None, m_SearchTerm ) );
						}
					}
					else if ( m_Page == CraftPage.PickResource2 && index >= 0 && index < system.CraftSubRes2.Count )
					{
						int groupIndex = ( context == null ? -1 : context.LastGroupIndex );

						CraftSubRes res = system.CraftSubRes2.GetAt( index );

						if ( m_From.Skills[system.MainSkill].Value < res.RequiredSkill )
						{
							m_From.SendGump( new CraftGump( m_From, system, m_Tool, res.Message, CraftPage.None, m_SearchTerm ) );
						}
						else
						{
							if ( context != null )
							context.LastResourceIndex2 = index;

							m_From.SendGump( new CraftGump( m_From, system, m_Tool, null, CraftPage.None, m_SearchTerm ) );
						}
					}

					break;
				}
				case 6: // Misc. buttons
				{
					switch ( index )
					{
						case 0: // Resource selection
						{
							if ( system.CraftSubRes.Init )
							m_From.SendGump( new CraftGump( m_From, system, m_Tool, null, CraftPage.PickResource, m_SearchTerm ) );

							break;
						}
						case 1: // Smelt item
						{
							if ( system.Resmelt )
							Resmelt.Do( m_From, system, m_Tool );

							break;
						}
						case 2: // Make last
						{
							if ( context == null )
							break;

							CraftItem item = context.LastMade;

							if ( item != null )
							CraftItem( item );
							else
							m_From.SendGump( new CraftGump( m_From, m_CraftSystem, m_Tool, 1044165, m_Page, m_SearchTerm ) ); // You haven't made anything yet.

							break;
						}
						case 3: // Last 10
						{
							if ( context == null )
							break;

							context.LastGroupIndex = LastTenGroupIndex;
							m_From.SendGump( new CraftGump( m_From, system, m_Tool, null ) );

							break;
						}
						case 4: // Toggle use resource hue
						{
							if ( context == null )
							break;

							context.DoNotColor = !context.DoNotColor;

							m_From.SendGump( new CraftGump( m_From, m_CraftSystem, m_Tool, null, m_Page, m_SearchTerm ) );

							break;
						}
						case 5: // Repair item
						{
							if ( system.Repair )
							Repair.Do( m_From, system, m_Tool );

							break;
						}
						case 6: // Toggle mark option
						{
							if ( context == null || !system.MarkOption )
							break;

							switch ( context.MarkOption )
							{
								case CraftMarkOption.MarkItem: context.MarkOption = CraftMarkOption.DoNotMark; break;
								case CraftMarkOption.DoNotMark: context.MarkOption = CraftMarkOption.PromptForMark; break;
								case CraftMarkOption.PromptForMark: context.MarkOption = CraftMarkOption.MarkItem; break;
							}

							m_From.SendGump( new CraftGump( m_From, m_CraftSystem, m_Tool, null, m_Page, m_SearchTerm ) );

							break;
						}
						case 7: // Resource selection 2
						{
							if ( system.CraftSubRes2.Init )
							m_From.SendGump( new CraftGump( m_From, system, m_Tool, null, CraftPage.PickResource2, m_SearchTerm ) );

							break;
						}
						case 8: // Enhance item
						{
							if ( system.CanEnhance )
							Enhance.BeginTarget( m_From, system, m_Tool );

							break;
						}
						case 9: // Skill craft list
						{
							m_From.SendGump( new CraftSkillListGump( m_From, system, m_Tool ) );

							break;
						}
						case 10: // Set output container
						{
							BeginSetOutputTarget();

							break;
						}
						case 11: // Favorites list
						{
							if ( context == null )
							break;

							context.LastGroupIndex = FavoritesGroupIndex;
							m_From.SendGump( new CraftGump( m_From, system, m_Tool, null ) );

							break;
						}
					}

					break;
				}
				case 7: // Create item (favorites)
				{
					if ( context == null )
					break;

					List<CraftItem> favorites = context.Favorites;

					if ( index >= 0 && index < favorites.Count )
					CraftItem( favorites[index] );

					break;
				}
				case 8: // Item details (favorites)
				{
					if ( context == null )
					break;

					List<CraftItem> favorites = context.Favorites;

					if ( index >= 0 && index < favorites.Count )
					m_From.SendGump( new CraftGumpItem( m_From, system, favorites[index], m_Tool, m_SearchTerm ) );

					break;
				}
				case 9: // Search submit
				{
					TextRelay searchEntry = info.GetTextEntry( SearchTextEntryID );
					string searchTerm = ( searchEntry == null ? null : searchEntry.Text );

					if ( searchTerm != null )
					searchTerm = searchTerm.Trim();

					if ( String.IsNullOrEmpty( searchTerm ) )
					searchTerm = null;

					m_From.SendGump( new CraftGump( m_From, system, m_Tool, null, m_Page, searchTerm ) );

					break;
				}
				case 10: // Create item (search results)
				{
					if ( m_SearchResults == null )
					break;

					if ( index >= 0 && index < m_SearchResults.Count )
					CraftItem( m_SearchResults[index] );

					break;
				}
				case 11: // Item details (search results)
				{
					if ( m_SearchResults == null )
					break;

					if ( index >= 0 && index < m_SearchResults.Count )
					m_From.SendGump( new CraftGumpItem( m_From, system, m_SearchResults[index], m_Tool, m_SearchTerm ) );

					break;
				}
			}

		}

		private void BeginSetOutputTarget()
		{
			CraftContext context = m_CraftSystem.GetContext( m_From );

			if ( context == null )
			return;

			m_From.SendMessage( "Target the container to receive crafted items (or target yourself to use your backpack)." );
			m_From.Target = new OutputTarget( this );
		}

		private void FinishSetOutputTarget( Mobile from, object targeted )
		{
			CraftContext context = m_CraftSystem.GetContext( from );

			if ( context == null )
			{
				Refresh( from );
				return;
			}

			if ( targeted == from )
			{
				context.OutputContainer = null;
				from.SendMessage( "Crafted items will now be placed in your backpack." );
				Refresh( from );
				return;
			}

			Container container = targeted as Container;

			if ( container == null )
			{
				from.SendMessage( "That is not a valid container." );
			}
			else if ( container.Deleted )
			{
				from.SendMessage( "That container is no longer available." );
			}
			else
			{
				bool inBackpack = ( from.Backpack != null && container.IsChildOf( from.Backpack ) );
				Container bank = from.FindBankNoCreate();
				bool inBank = ( bank != null && container.IsChildOf( bank ) );
				bool onMobile = ( container.RootParent == from );
				bool inRange = ( inBackpack || inBank || onMobile || from.InRange( container.GetWorldLocation(), 2 ) );

				if ( !inRange )
				{
					from.SendLocalizedMessage( 500446 ); // That is too far away.
				}
				else
				{
					if ( !container.IsAccessibleTo( from ) && !inBackpack && !inBank && !onMobile )
					{
						from.SendMessage( "You cannot access that container." );
					}
					else
					{
						context.OutputContainer = container;
						from.SendMessage( "Crafted items will now be placed in that container." );
					}
				}
			}

			Refresh( from );
		}

		private void Refresh( Mobile from )
		{
			from.SendGump( new CraftGump( from, m_CraftSystem, m_Tool, null, m_Page, m_SearchTerm ) );
		}

private class OutputTarget : Target
{
private readonly CraftGump m_Gump;

			public OutputTarget( CraftGump gump ) : base( -1, false, TargetFlags.None )
			{
				m_Gump = gump;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				m_Gump.FinishSetOutputTarget( from, targeted );
			}

			protected override void OnTargetCancel( Mobile from, TargetCancelType cancelType )
			{
				m_Gump.Refresh( from );
			}
		}
	}
}
