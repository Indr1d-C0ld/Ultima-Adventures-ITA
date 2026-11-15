using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Network;
using Server.Misc;
using Server.Prompts;
using System.Net;
using Server.Accounting;
using Server.Mobiles;
using Server.Commands;
using Server.Regions;
using Server.Spells;
using Server.Gumps;
using Server.Targeting;

namespace Server.Items
{
	[Flipable(0x1E5E, 0x1E5F)]
	public class GuildBoard : Item
	{
		[Constructable]
		public GuildBoard( ) : base( 0x1E5E )
		{
			Weight = 1.0;
			Name = "Local Guilds";
		}

		public override void OnDoubleClick( Mobile e )
		{
			if ( e.InRange( this.GetWorldLocation(), 4 ) )
			{
				e.CloseGump( typeof( GuildBoardGump ) );
				e.SendGump( new GuildBoardGump( e ) );
			}
			else
			{
				e.SendLocalizedMessage( 502138 ); // That is too far away for you to use
			}
		}

		public class GuildBoardGump : Gump
		{
			private const int ResignButtonId = 1;
			private const int SortAscendingButtonId = 2;
			private const int SortDescendingButtonId = 3;
			private const int AllButtonId = 100;
			private const int BackButtonId = 101;
			private const int ToggleColumnsButtonId = 102;
			private const int ToggleColumnsCloseButtonId = 103;
			private const int GuildFilterButtonBaseId = 200;

			private static readonly NpcGuild[] GuildFilters = new NpcGuild[]
			{
				NpcGuild.AlchemistsGuild,
				NpcGuild.ArchersGuild,
				NpcGuild.AssassinsGuild,
				NpcGuild.BardsGuild,
				NpcGuild.BlacksmithsGuild,
				NpcGuild.CarpentersGuild,
				NpcGuild.CartographersGuild,
				NpcGuild.CulinariansGuild,
				NpcGuild.DruidsGuild,
				NpcGuild.HealersGuild,
				NpcGuild.LibrariansGuild,
				NpcGuild.MagesGuild,
				NpcGuild.FishermensGuild,
				NpcGuild.MerchantsGuild,
				NpcGuild.MinersGuild,
				NpcGuild.NecromancersGuild,
				NpcGuild.RangersGuild,
				NpcGuild.TailorsGuild,
				NpcGuild.ThievesGuild,
				NpcGuild.TinkersGuild,
				NpcGuild.WarriorsGuild
			};

			public GuildBoardGump( Mobile from ) : this( from, NpcGuild.None, true, false )
			{
			}

			public GuildBoardGump( Mobile from, NpcGuild selectedGuild, bool ascending, bool filtersExpanded ) : base( 25, 25 )
			{
				string guildMasters = BuildGuildMasterList( selectedGuild, ascending );

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);
				AddImage(0, 0, 151);
				AddImage(300, 0, 151);
				AddImage(0, 300, 151);
				AddImage(300, 300, 151);
				AddImage(600, 0, 151);
				AddImage(600, 300, 151);
				AddImage(2, 2, 129);
				AddImage(302, 2, 129);
				AddImage(598, 2, 129);
				AddImage(2, 298, 129);
				AddImage(301, 298, 129);
				AddImage(598, 298, 129);
				AddImage(8, 11, 145);
				AddImage(267, 19, 141);
				AddImage(473, 19, 141);
				AddImage(698, 7, 146);
				AddImage(219, 14, 143);
				AddImage(249, 31, 159);
				AddImage(50, 260, 161);
				AddImage(853, 210, 161);
				AddImage(853, 257, 161);
				AddImage(854, 554, 157);
				AddImage(51, 554, 157);
				AddItem(179, 49, 7775);
				AddHtml( 234, 72, 428, 27, @"<BODY><BASEFONT Color=#FBFBFB><BIG>LOCAL GUILDS</BIG></BASEFONT></BODY>", (bool)false, (bool)false);

				PlayerMobile pm = (PlayerMobile)from;
				if ( pm.NpcGuild != NpcGuild.None && selectedGuild == NpcGuild.None )
				{
					AddButton( 400, 82, 4005, 4005, ResignButtonId, GumpButtonType.Reply, 0 );
					AddHtml( 435, 82, 300, 21, @"<BODY><BASEFONT Color=#FCFF00><BIG>Resign From My Local Guild</BIG></BASEFONT></BODY>", false, false );
				}

				if ( selectedGuild != NpcGuild.None )
				{
					AddHtml( 260, 112, 420, 21, String.Format( "<BODY><BASEFONT Color=#FCFF00><BIG>{0}</BIG></BASEFONT></BODY>", GetGuildLabel( selectedGuild ) ), false, false );
					AddButton( 225, 136, 4005, 4005, BackButtonId, GumpButtonType.Reply, 0 );
					AddHtml( 260, 136, 80, 21, "<BODY><BASEFONT Color=#FCFF00><BIG>Back</BIG></BASEFONT></BODY>", false, false );
					AddHtml( 100, 155, 737, 418, @"<BODY><BASEFONT Color=#FCFF00><BIG>" + guildMasters + "</BIG></BASEFONT></BODY>", false, true );
					return;
				}

				AddButton( 500, 112, 4005, 4005, AllButtonId, GumpButtonType.Reply, 0 );
				AddHtml( 535, 112, 120, 21, "<BODY><BASEFONT Color=#FCFF00><BIG>All</BIG></BASEFONT></BODY>", false, false );
				AddButton( 620, 112, 4005, 4005, SortAscendingButtonId, GumpButtonType.Reply, 0 );
				AddHtml( 655, 112, 120, 21, "<BODY><BASEFONT Color=#FCFF00><BIG>Asc</BIG></BASEFONT></BODY>", false, false );
				AddButton( 740, 112, 4005, 4005, SortDescendingButtonId, GumpButtonType.Reply, 0 );
				AddHtml( 775, 112, 120, 21, "<BODY><BASEFONT Color=#FCFF00><BIG>Desc</BIG></BASEFONT></BODY>", false, false );

				AddButton( 100, 162, 4005, 4005, filtersExpanded ? ToggleColumnsCloseButtonId : ToggleColumnsButtonId, GumpButtonType.Reply, 0 );
				AddHtml( 135, 162, 460, 21, String.Format( "<BODY><BASEFONT Color=#FCFF00><BIG>Filter by guild type [{0}]</BIG></BASEFONT></BODY>", filtersExpanded ? "-" : "+" ), false, false );

				for ( int i = 0; i < GuildFilters.Length; ++i )
				{
					int row = i / 2;
					int col = i % 2;
					int x = 100 + ( col * 260 );
					int y = 188 + ( row * 22 );

					if ( !filtersExpanded )
						continue;

					AddButton( x, y, 4005, 4005, GuildFilterButtonBaseId + i, GumpButtonType.Reply, 0 );
					AddHtml( x + 35, y, 220, 21, String.Format( "<BODY><BASEFONT Color=#FCFF00><BIG>{0}</BIG></BASEFONT></BODY>", GetGuildLabel( GuildFilters[i] ) ), false, false );
				}

				int textY = filtersExpanded ? 430 : 194;
				int textH = 573 - textY;
				AddHtml( 100, textY, 737, textH, @"<BODY><BASEFONT Color=#FCFF00><BIG>There are many groups in the land that have established guild houses and are often looking for members. These guilds are separate from the various adventurer guilds that may be established on their own, as they focus on a group of people with a certain skillset and trade. Below is a listing of guild houses looking for members.<br><br>- Alchemists Guild<br>- Archers Guild<br>- Assassins Guild<br>- Bard Guild<br>- Blacksmith Guild<br>- Carpenters Guild<br>- Cartographers Guild<br>- Culinary Guild<br>- Druids Guild<br>- Healer Guild<br>- Librarians Guild<br>- Mage Guild<br>- Mariners Guild<br>- Merchant Guild<br>- Miner Guild<br>- Necromancers Guild<br>- Ranger Guild<br>- Tailor Guild<br>- Thief Guild<br>- Tinker Guild<br>- Warrior Guild<br><br>The requirement for entry to any of these guilds (in addition to not being a member of another local guild) is 2,000 gold paid to the guildmaster. To join a guild, find the appropriate guildmaster and single click them to select 'Join'. They will then ask you for an amount of gold if you meet the qualifications. Just drop the exact amount of gold on them to join. You may resign from a guild by going back to your guildmaster, single clicking them, and selecting 'Resign' (or you can use this board to resign). Then you could join another guild. Be warned, each guild you join will have double the fee from the last. So when you join a guild for 2,000 gold, the next guild you join will require 4,000 gold. The guild joined after that will be 8,000 gold. One of the benefits of joining a local guild is the receiving of more gold for goods sold to other guild members. You will also receive a guild membership ring that will help you with skills that pertain to the guild, which would be yours and yours alone. If you lose your ring for any reason, give a guildmaster 400 gold to replace it. The skills aided by the ring are also the skills that you will gain quicker, being a member of the guild. You will also be able to purchase items from guildmasters, as they sell extra items to members of the guild.<br><br>In order to steal from other players, you must be a member of the Thieves Guild." + guildMasters + "</BIG></BASEFONT></BODY>", (bool)false, (bool)true);
			}

			public override void OnResponse( NetState state, RelayInfo info )
			{
				Mobile from = state.Mobile;
				PlayerMobile pm = (PlayerMobile)from;

				if ( info.ButtonID == ResignButtonId )
				{
					pm.NpcGuild = NpcGuild.None;
					from.SendMessage(0X22, "You have resigned from the local guild.");
					BaseGuildmaster.SetNewGuildCost(pm);

					ArrayList targets = new ArrayList();
					foreach ( Item item in World.Items.Values )
					if ( item is GuildRings )
					{
						GuildRings guildring = (GuildRings)item;
						if ( guildring.RingOwner == from )
						{
							targets.Add( item );
						}
					}
					for ( int i = 0; i < targets.Count; ++i )
					{
						Item item = ( Item )targets[ i ];
						item.Delete();
					}
					return;
				}

				if ( info.ButtonID == SortAscendingButtonId )
				{
					from.SendGump( new GuildBoardGump( from, NpcGuild.None, true, false ) );
					return;
				}

				if ( info.ButtonID == SortDescendingButtonId )
				{
					from.SendGump( new GuildBoardGump( from, NpcGuild.None, false, false ) );
					return;
				}

				if ( info.ButtonID == AllButtonId || info.ButtonID == BackButtonId )
				{
					from.SendGump( new GuildBoardGump( from, NpcGuild.None, true, false ) );
					return;
				}

				if ( info.ButtonID == ToggleColumnsButtonId )
				{
					from.SendGump( new GuildBoardGump( from, NpcGuild.None, true, true ) );
					return;
				}

				if ( info.ButtonID == ToggleColumnsCloseButtonId )
				{
					from.SendGump( new GuildBoardGump( from, NpcGuild.None, true, false ) );
					return;
				}

				if ( info.ButtonID >= GuildFilterButtonBaseId && info.ButtonID < GuildFilterButtonBaseId + GuildFilters.Length )
				{
					NpcGuild filter = GuildFilters[info.ButtonID - GuildFilterButtonBaseId];
					from.SendGump( new GuildBoardGump( from, filter, true, false ) );
				}
			}

			private static string BuildGuildMasterList( NpcGuild filter, bool ascending )
			{
				string guildMasters = "<br><br>";
				List<BaseGuildmaster> list = new List<BaseGuildmaster>();

				foreach ( Mobile target in World.Mobiles.Values )
				{
					BaseGuildmaster guildMaster = target as BaseGuildmaster;
					if ( guildMaster == null )
						continue;

					if ( filter != NpcGuild.None && guildMaster.NpcGuild != filter )
						continue;

					list.Add( guildMaster );
				}

				if ( filter == NpcGuild.None )
				{
					list = ascending
						? list.OrderBy( e => e.Title.Replace( "elf ", "" ) ).ThenBy( e => e.Name ).ToList()
						: list.OrderByDescending( e => e.Title.Replace( "elf ", "" ) ).ThenBy( e => e.Name ).ToList();
				}
				else
				{
					list = list.OrderBy( e => e.Name ).ToList();
				}

				foreach ( Mobile m in list )
					guildMasters += m.Name + "<br>" + m.Title + "<br>" + Server.Misc.Worlds.GetRegionName( m.Map, m.Location ) + "<br><br>";

				return guildMasters;
			}

			private static string GetGuildLabel( NpcGuild guild )
			{
				if ( guild == NpcGuild.FishermensGuild )
					return "Mariners Guild";

				return guild.ToString().Replace( "Guild", " Guild" );
			}
		}

		public GuildBoard(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
