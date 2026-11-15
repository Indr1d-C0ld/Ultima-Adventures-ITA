using System;
using System.Collections.Generic;
using System.Globalization;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Craft
{
	public class CraftSkillListGump : Gump
	{
		private const int LabelHue = 0x480;
		private const int LabelColor = 0x7FFF;
		private const int FontColor = 0xFFFFFF;

		private const int BackButtonID = 6000;

		private readonly Mobile m_From;
		private readonly CraftSystem m_CraftSystem;
		private readonly BaseTool m_Tool;
		private readonly List<SkillListEntry> m_Entries;

		public CraftSkillListGump( Mobile from, CraftSystem craftSystem, BaseTool tool ) : base( 40, 40 )
		{
			m_From = from;
			m_CraftSystem = craftSystem;
			m_Tool = tool;
			m_Entries = BuildEntries();

			from.CloseGump( typeof( CraftSkillListGump ) );
			from.CloseGump( typeof( CraftGumpItem ) );
			from.CloseGump( typeof( CraftGump ) );

			AddPage( 0 );

			AddBackground( 0, 0, 530, 538, 5054 );
			AddImageTiled( 10, 10, 510, 22, 2624 );
			AddImageTiled( 10, 353, 150, 45, 2624 );
			AddImageTiled( 165, 353, 355, 45, 2624 );
			AddImageTiled( 10, 403, 510, 125, 2624 );
			AddImageTiled( 10, 37, 200, 311, 2624 );
			AddImageTiled( 215, 37, 305, 311, 2624 );
			AddAlphaRegion( 10, 10, 510, 518 );

			if ( craftSystem.GumpTitleNumber > 0 )
			AddHtmlLocalized( 10, 12, 510, 20, craftSystem.GumpTitleNumber, LabelColor, false, false );
			else
			AddHtml( 10, 12, 510, 20, craftSystem.GumpTitleString, false, false );

			AddHtmlLocalized( 10, 37, 200, 22, 1044011, LabelColor, false, false ); // <CENTER>SELECTIONS</CENTER>
                        AddHtml( 215, 37, 205, 22, String.Format( "<CENTER><BASEFONT COLOR=#{0:X6}>{1}</BASEFONT></CENTER>", FontColor, "CRAFT THESE ITEMS FOR SKILL GAINS" ), false, false );
                        AddLabel( 430, 37, LabelHue, "MAX SKILL" );
			AddHtml( 20, 70, 180, 260, String.Format( "<BASEFONT COLOR=#{0:X6}>Select an entry to craft it immediately or view its details. Use Back to return to the crafting menu.</BASEFONT>", FontColor ), false, true );

			AddButton( 15, 493, 4014, 4016, BackButtonID, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 50, 496, 150, 18, 1044150, LabelColor, false, false ); // BACK

			AddButton( 270, 493, 4017, 4019, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 305, 496, 150, 18, 1011441, LabelColor, false, false ); // EXIT

			RenderEntryList();
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( info.ButtonID == 0 )
			{
				m_From.SendGump( new CraftGump( m_From, m_CraftSystem, m_Tool, null ) );
				return;
			}

			if ( info.ButtonID == BackButtonID )
			{
				m_From.SendGump( new CraftGump( m_From, m_CraftSystem, m_Tool, null ) );
				return;
			}

			int craftCount = m_Entries.Count;
			int craftStart = 1;
			int detailStart = craftStart + craftCount;

			if ( info.ButtonID >= craftStart && info.ButtonID < detailStart )
			{
				int index = info.ButtonID - craftStart;

				if ( index >= 0 && index < craftCount )
				{
					CraftItem( m_Entries[index].Item );
				}

				return;
			}

			int detailIndex = info.ButtonID - detailStart;

			if ( detailIndex >= 0 && detailIndex < craftCount )
			{
				CraftItem item = m_Entries[detailIndex].Item;

				m_From.CloseGump( typeof( CraftSkillListGump ) );
				m_From.SendGump( new CraftGumpItem( m_From, m_CraftSystem, item, m_Tool ) );
			}
		}

		private void RenderEntryList()
		{
			if ( m_Entries.Count == 0 )
			{
				AddHtml( 220, 60, 300, 40, String.Format( "<BASEFONT COLOR=#{0:X6}>No craftable items currently grant skill gains.</BASEFONT>", FontColor ), false, false );
				return;
			}

			int craftButton = 1;
			int detailButton = 1 + m_Entries.Count;

			for ( int i = 0; i < m_Entries.Count; ++i )
			{
				int index = i % 10;
				SkillListEntry entry = m_Entries[i];

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

				int rowY = 60 + (index * 20);
				string skillText = entry.MaxSkill.ToString( "0.0", CultureInfo.InvariantCulture );

				AddButton( 220, rowY, 4005, 4007, craftButton + i, GumpButtonType.Reply, 0 );

				if ( entry.Item.NameNumber > 0 )
				{
					AddHtmlLocalized( 255, rowY + 3, 220, 18, entry.Item.NameNumber, LabelColor, false, false );
				}
				else
				{
					string name = entry.Item.NameString;

					if ( String.IsNullOrEmpty( name ) )
					name = entry.Item.ItemType.Name;

					AddLabel( 255, rowY, LabelHue, name );
				}

				AddLabel( 430, rowY, LabelHue, skillText );
				AddButton( 480, rowY, 4011, 4012, detailButton + i, GumpButtonType.Reply, 0 );
			}
		}

		private void CraftItem( CraftItem item )
		{
			int num = m_CraftSystem.CanCraft( m_From, m_Tool, item.ItemType );

			if ( num > 0 )
			{
				m_From.SendGump( new CraftGump( m_From, m_CraftSystem, m_Tool, num ) );
				return;
			}

			CraftContext context = m_CraftSystem.GetContext( m_From );
			Type type = ResolveResourceType( item, context );

			m_CraftSystem.CreateItem( m_From, item.ItemType, type, m_Tool, item );
		}

		private Type ResolveResourceType( CraftItem item, CraftContext context )
		{
			if ( context == null )
			return null;

			CraftSubResCol res = item.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes;
			int resIndex = item.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex;

			if ( resIndex >= 0 && resIndex < res.Count )
			return res.GetAt( resIndex ).ItemType;

			return null;
		}

		private List<SkillListEntry> BuildEntries()
		{
			List<SkillListEntry> results = new List<SkillListEntry>();
			CraftGroupCol groups = m_CraftSystem.CraftGroups;
			CraftContext context = m_CraftSystem.GetContext( m_From );
			double playerSkill = m_From.Skills[m_CraftSystem.MainSkill].Value;

			for ( int g = 0; g < groups.Count; ++g )
			{
				CraftGroup group = groups.GetAt( g );
				CraftItemCol items = group.CraftItems;

				for ( int i = 0; i < items.Count; ++i )
				{
					CraftItem item = items.GetAt( i );

					SkillListEntry entry;

					if ( TryCreateEntry( item, context, playerSkill, out entry ) )
					results.Add( entry );
				}
			}

			results.Sort( CompareEntries );
			return results;
		}

		private bool TryCreateEntry( CraftItem item, CraftContext context, double playerSkill, out SkillListEntry entry )
		{
			entry = null;

			double minSkill;
			double maxSkill;

			if ( !TryGetMainSkillRange( item, out minSkill, out maxSkill ) )
			return false;

			if ( playerSkill < minSkill || playerSkill >= maxSkill )
			return false;

			if ( item.RequiredExpansion != Expansion.None )
			{
				if ( m_From.NetState == null || !m_From.NetState.SupportsExpansion( item.RequiredExpansion ) )
				return false;
			}

			PlayerMobile pm = m_From as PlayerMobile;

			if ( item.Recipe != null && pm != null && !pm.HasRecipe( item.Recipe ) )
			return false;

			bool allRequiredSkills = true;
			item.GetSuccessChance( m_From, ResolveResourceType( item, context ), m_CraftSystem, false, ref allRequiredSkills );

			if ( !allRequiredSkills )
			return false;

			entry = new SkillListEntry( item, minSkill, maxSkill );
			return true;
		}

		private bool TryGetMainSkillRange( CraftItem item, out double minSkill, out double maxSkill )
		{
			CraftSkillCol skills = item.Skills;

			for ( int i = 0; i < skills.Count; ++i )
			{
				CraftSkill skill = skills.GetAt( i );

				if ( skill.SkillToMake == m_CraftSystem.MainSkill )
				{
					minSkill = skill.MinSkill;
					maxSkill = skill.MaxSkill;
					return true;
				}
			}

			minSkill = 0.0;
			maxSkill = 0.0;
			return false;
		}

		private int CompareEntries( SkillListEntry a, SkillListEntry b )
		{
			int comparison = a.MaxSkill.CompareTo( b.MaxSkill );

			if ( comparison != 0 )
			return comparison;

			return String.Compare( a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase );
		}

                private class SkillListEntry
                {
                        private readonly CraftItem m_Item;
                        private readonly double m_MinSkill;
                        private readonly double m_MaxSkill;

                        public CraftItem Item
                        {
                                get
                                {
                                        return m_Item;
                                }
                        }

                        public double MinSkill
                        {
                                get
                                {
                                        return m_MinSkill;
                                }
                        }

                        public double MaxSkill
                        {
                                get
                                {
                                        return m_MaxSkill;
                                }
                        }
			public string DisplayName
			{
				get
				{
					if ( Item.NameNumber > 0 )
					return String.Format( "#{0}", Item.NameNumber );

					return Item.NameString ?? Item.ItemType.Name;
				}
			}

                        public SkillListEntry( CraftItem item, double minSkill, double maxSkill )
                        {
                                m_Item = item;
                                m_MinSkill = minSkill;
                                m_MaxSkill = maxSkill;
                        }
                }
        }
}
