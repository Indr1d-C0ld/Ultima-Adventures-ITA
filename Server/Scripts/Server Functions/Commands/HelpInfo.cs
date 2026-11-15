using System;
using Server;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Commands.Generic;
using CommandInfo=Server.Commands.Docs.DocCommandEntry;
using CommandInfoSorter=Server.Commands.Docs.CommandEntrySorter;

namespace Server.Commands
{
	public class HelpInfo
	{
		private const int SearchUpdateButtonID = 3;
		private const int SearchTextEntryID = 4;

		private static Dictionary<string, CommandInfo> m_HelpInfos = new Dictionary<string, CommandInfo>();
		private static List<CommandInfo> m_SortedHelpInfo = new List<CommandInfo>();	//No need for SortedList cause it's only sorted once at creation...

		public static Dictionary<string, CommandInfo> HelpInfos{ get { return m_HelpInfos; } }
		public static List<CommandInfo> SortedHelpInfo { get { return m_SortedHelpInfo; } }

		[CallPriority( Int32.MaxValue )]
		public static void Initialize()
		{
			CommandSystem.Register( "HelpInfo", AccessLevel.Player, new CommandEventHandler( HelpInfo_OnCommand ) );

			FillTable();
		}

		[Usage( "HelpInfo [<command>|search <term>]" )]
		[Description( "Gives information on a specified command, searches commands by term, or when no argument specified, displays a gump containing all commands" )]
		private static void HelpInfo_OnCommand( CommandEventArgs e )
		{
			if( e.Length > 0 )
			{
				string arg = e.GetString( 0 ).ToLower();
				CommandInfo c;

				if( arg == "search" )
				{
					string term = e.Length > 1 ? e.GetString( 1 ) : String.Empty;
					e.Mobile.SendGump( new CommandListGump( 0, e.Mobile, BuildSearchList( e.Mobile, term ), term ) );

					return;
				}

				if( m_HelpInfos.TryGetValue( arg, out c ) )
				{
					Mobile m = e.Mobile;

					if( m.AccessLevel >= c.AccessLevel )
						m.SendGump( new CommandInfoGump( c ) );
					else
						m.SendMessage( "You don't have access to that command." );

					return;
				}

				e.Mobile.SendMessage( String.Format( "Command '{0}' not found! Use [HelpInfo search <term>] to search.", arg ) );
			}

			e.Mobile.SendGump( new CommandListGump( 0, e.Mobile, null ) );

		}

		private static List<CategorizedCommandInfo> BuildSearchList( Mobile from, string term )
		{
			List<CategorizedCommandInfo> filtered = new List<CategorizedCommandInfo>();
			string normalized = NormalizeSearchTerm( term );

			foreach( CommandInfo info in m_SortedHelpInfo )
			{
				if( from.AccessLevel < info.AccessLevel )
					continue;

				if( String.IsNullOrEmpty( normalized ) || IsSearchMatch( info, normalized ) )
					filtered.Add( new CategorizedCommandInfo( info, GetCategory( info ) ) );
			}

			return filtered.OrderBy( c => c.Category ).ThenBy( c => c.Info.AccessLevel ).ThenBy( c => c.Info.Name ).ToList();
		}

		private static string NormalizeSearchTerm( string term )
		{
			return term == null ? String.Empty : term.Trim().ToLower();
		}

		private static bool IsSearchMatch( CommandInfo info, string term )
		{
			if( String.IsNullOrEmpty( term ) )
				return true;

			string name = info.Name == null ? String.Empty : info.Name.ToLower();
			string usage = info.Usage == null ? String.Empty : info.Usage.ToLower();
			string desc = info.Description == null ? String.Empty : info.Description.ToLower();

			if( name.Contains( term ) || usage.Contains( term ) || desc.Contains( term ) )
				return true;

			int maxDistance = term.Length <= 4 ? 1 : 2;
			return LevenshteinDistance( name, term ) <= maxDistance;
		}

		private static int LevenshteinDistance( string left, string right )
		{
			if( left == null )
				left = String.Empty;

			if( right == null )
				right = String.Empty;

			int[,] costs = new int[left.Length + 1, right.Length + 1];

			for( int i = 0; i <= left.Length; ++i )
				costs[i, 0] = i;

			for( int j = 0; j <= right.Length; ++j )
				costs[0, j] = j;

			for( int i = 1; i <= left.Length; ++i )
			{
				for( int j = 1; j <= right.Length; ++j )
				{
					int substitution = costs[i - 1, j - 1] + (left[i - 1] == right[j - 1] ? 0 : 1);
					int insertion = costs[i, j - 1] + 1;
					int deletion = costs[i - 1, j] + 1;

					costs[i, j] = Math.Min( substitution, Math.Min( insertion, deletion ) );
				}
			}

			return costs[left.Length, right.Length];
		}

		public static void FillTable()
		{
			List<CommandEntry> commands = new List<CommandEntry>( CommandSystem.Entries.Values );
			List<CommandInfo> list = new List<CommandInfo>();

			for( int i = 0; i < commands.Count; ++i )
			{
				CommandEntry e = commands[i];

				MethodInfo mi = e.Handler.Method;

				object[] attrs = mi.GetCustomAttributes( typeof( UsageAttribute ), false );
				UsageAttribute usage = ( attrs.Length == 0 ? null : attrs[0] as UsageAttribute );

				attrs = mi.GetCustomAttributes( typeof( DescriptionAttribute ), false );
				DescriptionAttribute desc = ( attrs.Length == 0 ? null : attrs[0] as DescriptionAttribute );

				attrs = mi.GetCustomAttributes( typeof( AliasesAttribute ), false );

				AliasesAttribute aliases = ( attrs.Length == 0 ? null : attrs[0] as AliasesAttribute );

				string usageString = ( usage == null || String.IsNullOrEmpty( usage.Usage ) ? String.Format( "[{0}]", e.Command ) : usage.Usage );
				string descString = ( desc == null || String.IsNullOrEmpty( desc.Description ) ? "No detailed help is available for this command." : desc.Description.Replace( "<", "(" ).Replace( ">", ")" ) );

				if( aliases == null )
					AddCommandInfo( list, e.AccessLevel, e.Command, null, usageString, descString, false );
				else
				{
					AddCommandInfo( list, e.AccessLevel, e.Command, aliases.Aliases, usageString, descString, false );

					for( int j = 0; j < aliases.Aliases.Length; j++ )
					{
						string[] newAliases = new string[aliases.Aliases.Length];

						aliases.Aliases.CopyTo( newAliases, 0 );

						newAliases[j] = e.Command;

						AddCommandInfo( list, e.AccessLevel, aliases.Aliases[j], newAliases, usageString, descString, false );
					}
				}
			}


                        for( int i = 0; i < TargetCommands.AllCommands.Count; ++i )
                        {
                                BaseCommand command = TargetCommands.AllCommands[i];

				string usage = command.Usage;
				string desc = command.Description;

				if( usage == null || desc == null )
					continue;

				string[] cmds = command.Commands;
				string cmd = cmds[0];
				string[] aliases = new string[cmds.Length - 1];

				for( int j = 0; j < aliases.Length; ++j )
					aliases[j] = cmds[j + 1];

				desc = desc.Replace( "<", "(" ).Replace( ">", ")" );

				if( command.Supports != CommandSupport.Single )
				{
					StringBuilder sb = new StringBuilder( 50 + desc.Length );

					sb.Append( "Modifiers: " );

					if( (command.Supports & CommandSupport.Global) != 0 )
						sb.Append( "<i><Global</i>, " );

					if( (command.Supports & CommandSupport.Online) != 0 )
						sb.Append( "<i>Online</i>, " );

					if( (command.Supports & CommandSupport.Region) != 0 )
						sb.Append( "<i>Region</i>, " );

					if( (command.Supports & CommandSupport.Contained) != 0 )
						sb.Append( "<i>Contained</i>, " );

					if( (command.Supports & CommandSupport.Multi) != 0 )
						sb.Append( "<i>Multi</i>, " );

					if( (command.Supports & CommandSupport.Area) != 0 )
						sb.Append( "<i>Area</i>, " );

					if( (command.Supports & CommandSupport.Self) != 0 )
						sb.Append( "<i>Self</i>, " );

					sb.Remove( sb.Length - 2, 2 );
					sb.Append( "<br>" );
					sb.Append( desc );

					desc = sb.ToString();
				}

				AddCommandInfo( list, command.AccessLevel, cmd, aliases, usage, desc, true );

				for( int j = 0; j < aliases.Length; j++ )
				{
					string[] newAliases = new string[aliases.Length];

					aliases.CopyTo( newAliases, 0 );

					newAliases[j] = cmd;

						AddCommandInfo( list, command.AccessLevel, aliases[j], newAliases, usage, desc, true );
                                }
                        }

			List<BaseCommandImplementor> commandImpls = BaseCommandImplementor.Implementors;

			for( int i = 0; i < commandImpls.Count; ++i )
			{
				BaseCommandImplementor command = commandImpls[i];

				string usage = command.Usage;
				string desc = command.Description;

				if( usage == null || desc == null )
					continue;

				string[] cmds = command.Accessors;
				string cmd = cmds[0];
				string[] aliases = new string[cmds.Length - 1];

				for( int j = 0; j < aliases.Length; ++j )
					aliases[j] = cmds[j + 1];

				desc = desc.Replace( "<", ")" ).Replace( ">", ")" );

				AddCommandInfo( list, command.AccessLevel, cmd, aliases, usage, desc, true );

				for( int j = 0; j < aliases.Length; j++ )
				{
					string[] newAliases = new string[aliases.Length];

					aliases.CopyTo( newAliases, 0 );

					newAliases[j] = cmd;

					AddCommandInfo( list, command.AccessLevel, aliases[j], newAliases, usage, desc, true );
				}
			}

			EnsureManualHelpEntries( list );

			list.Sort( new CommandInfoSorter() );

			m_SortedHelpInfo = list;

			foreach( CommandInfo c in m_SortedHelpInfo )
			{
                                if( !m_HelpInfos.ContainsKey( c.Name.ToLower() ) )
                                        m_HelpInfos.Add( c.Name.ToLower(), c );
                        }
                }

                private static void EnsureManualHelpEntries( List<CommandInfo> list )
                {
                        EnsureCommandInfo( list, "Roll", AccessLevel.Player, null, "[Roll]", "Rolls dice for the player." );
                        EnsureCommandInfo( list, "Ratings", AccessLevel.Player, null, "[Ratings]", "Opens the avatar leaderboard for Order and Chaos standings." );
                        EnsureCommandInfo( list, "Ferry", AccessLevel.Administrator, null, "[Ferry]", "Opens the ferry travel interface." );
                        EnsureCommandInfo( list, "Wagon", AccessLevel.Administrator, null, "[Wagon]", "Opens the wagon travel interface." );
                        EnsureCommandInfo( list, "VendorSearch", AccessLevel.Player, null, "[VendorSearch]", "Opens the vendor search interface." );
                        EnsureCommandInfo( list, "SetTime", AccessLevel.Administrator, null, "[SetTime <hour> <minute>]", "Sets the current world time." );
                        EnsureCommandInfo( list, "GlobalLight", AccessLevel.Administrator, null, "[GlobalLight <value>]", "Overrides the global light level." );
                        EnsureCommandInfo( list, "EconomyHistory", AccessLevel.Administrator, null, "[EconomyHistory [Player <name>|Item <name>|Category <name>|<vendor serial>]]", "Displays administrator vendor economy analytics." );
                }

                private static void EnsureCommandInfo( List<CommandInfo> list, string commandName, AccessLevel access, string[] aliases, string usage, string description )
                {
                        bool registered = CommandSystem.Entries.Values.Any( e => e.Command.Equals( commandName, StringComparison.OrdinalIgnoreCase ) );

                        if( registered )
                                AddCommandInfo( list, access, commandName, aliases, usage, description, true );
                }

		private static void AddCommandInfo( List<CommandInfo> list, AccessLevel access, string commandName, string[] aliases, string usage, string description, bool replaceExisting )
		{
			if( String.IsNullOrEmpty( commandName ) )
				return;

			for( int i = list.Count - 1; i >= 0; --i )
			{
				if( !list[i].Name.Equals( commandName, StringComparison.OrdinalIgnoreCase ) )
					continue;

				if( !replaceExisting )
					return;

				list.RemoveAt( i );
			}

			list.Add( new CommandInfo( access, commandName, aliases, usage, description ) );
		}


		private static string GetCategory( CommandInfo info )
		{
			string text = String.Format( "{0} {1} {2}", info.Name, info.Usage, info.Description ).ToLower();

			if( ContainsAny( text, "spell", "spells", "magic", "magery", "necromancy", "chivalry", "weapon ability", "ability", "special move", "bard", "jedi", "combat", "attack", "cast", "casting", "mana", "reagent", "spellbook", "circle", "summon", "fireball", "lightning" ) )
				return "Combat";

			if( ContainsAny( text, "account", "password", "email", "profile", "character", "login", "logout", "delete", "newbie" ) )
				return "Account";

			if( ContainsAny( text, "ban", "jail", "mute", "kick", "warn", "page", "staff", "moder", "report", "suspend", "silence" ) )
				return "Moderation";

			if( ContainsAny( text, "travel", "gate", "moongate", "teleport", "ferry", "wagon", "go ", "recall", "mark" ) )
				return "Travel";

			if( ContainsAny( text, "vendor", "shop", "economy", "buy", "sell", "gold", "bank", "trade", "auction" ) )
				return "Economy";

			if( ContainsAny( text, "music", "quest", "party", "guild", "chat", "emote", "social", "ratings" ) )
				return "Social";

			if( ContainsAny( text, "spawn", "region", "map", "world", "light", "time", "event", "save", "reload", "shutdown", "server" ) )
				return "World/Admin";

			return "Utility";
		}

		private static string EncodeHtml( string value )
		{
			if( String.IsNullOrEmpty( value ) )
				return String.Empty;

			return value.Replace( "&", "&amp;" ).Replace( "\"", "&quot;" ).Replace( "<", "&lt;" ).Replace( ">", "&gt;" );
		}


		private static bool ContainsAny( string value, params string[] needles )
		{
			for( int i = 0; i < needles.Length; ++i )
			{
				if( value.Contains( needles[i] ) )
					return true;
			}

			return false;
		}

		public class CategorizedCommandInfo
		{
			public CommandInfo Info;
			public string Category;

			public CategorizedCommandInfo( CommandInfo info, string category )
			{
				Info = info;
				Category = category;
			}
		}

		public class CommandListGump : BaseGridGump
		{
			private const int EntriesPerPage = 30;
			private const int HeaderWidth = 30;
			private const int ContentWidth = 160;
			private const int DetailButtonBase = 10;

			private static Dictionary<Mobile, List<Item>> m_ActiveTooltipItems = new Dictionary<Mobile, List<Item>>();

			int m_Page;
			List<CategorizedCommandInfo> m_List;
			List<int> m_TooltipSerials;
			string m_SearchTerm;

			public CommandListGump( int page, Mobile from, List<CategorizedCommandInfo> list )
				: this( page, from, list, String.Empty )
			{
			}

			public CommandListGump( int page, Mobile from, List<CategorizedCommandInfo> list, string searchTerm )
				: base( 30, 30 )
			{
				m_Page = page;
				m_SearchTerm = searchTerm == null ? String.Empty : searchTerm;

				if( list == null )
				{
					m_List = new List<CategorizedCommandInfo>();

					foreach( CommandInfo c in m_SortedHelpInfo )
					{
						if( from.AccessLevel >= c.AccessLevel )
							m_List.Add( new CategorizedCommandInfo( c, GetCategory( c ) ) );
					}

					m_List = m_List.OrderBy( c => c.Category ).ThenBy( c => c.Info.AccessLevel ).ThenBy( c => c.Info.Name ).ToList();
				}
				else
					m_List = list;

				CleanupTooltipItems( from );

				m_TooltipSerials = new List<int>( m_List.Count );
				List<Item> createdTooltipItems = new List<Item>( m_List.Count );

				for( int i = 0; i < m_List.Count; ++i )
				{
					CommandInfo info = m_List[i].Info;
					CommandTooltipItem item = new CommandTooltipItem( info.Name, info.Description );
					createdTooltipItems.Add( item );
					m_TooltipSerials.Add( item.Serial );
					item.SendPropertiesTo( from );
				}

				m_ActiveTooltipItems[from] = createdTooltipItems;

				AddNewPage();

				if( m_Page > 0 )
					AddEntryButton( HeaderWidth, ArrowLeftID1, ArrowLeftID2, 1, ArrowLeftWidth, ArrowLeftHeight );
				else
					AddEntryHeader( HeaderWidth );

				AddEntryHtml( ContentWidth, Center( String.Format( "Page {0} of {1}", m_Page+1, (m_List.Count + EntriesPerPage - 1) / EntriesPerPage ) ) );

				if( (m_Page + 1) * EntriesPerPage < m_List.Count )
					AddEntryButton( HeaderWidth, ArrowRightID1, ArrowRightID2, 2, ArrowRightWidth, ArrowRightHeight );
				else
					AddEntryHeader( HeaderWidth );

				AddNewLine();
				AddEntryHtml( HeaderWidth, Color( "Find:", 0x000000 ) );
				AddEntryText( ContentWidth, SearchTextEntryID, m_SearchTerm );
				AddEntryButton( HeaderWidth, ArrowRightID1, ArrowRightID2, SearchUpdateButtonID, ArrowRightWidth, ArrowRightHeight );

				int last = (int)AccessLevel.Player - 1;
				string lastCategory = String.Empty;

				for( int i = m_Page * EntriesPerPage, line = 0; line < EntriesPerPage && i < m_List.Count; ++i, ++line )
				{
					CategorizedCommandInfo entry = m_List[i];
					CommandInfo c = entry.Info;

					if( from.AccessLevel >= c.AccessLevel )
					{
						if( !entry.Category.Equals( lastCategory, StringComparison.Ordinal ) )
						{
							AddNewLine();
							AddEntryHtml( HeaderWidth + OffsetSize + ContentWidth, Color( Center( entry.Category ), 0x000000 ) );
							AddEntryHeader( HeaderWidth );
							lastCategory = entry.Category;
							last = (int)AccessLevel.Player - 1;
							line++;
						}

						if( line >= EntriesPerPage )
							break;

						if( (int)c.AccessLevel != last )
						{
							AddNewLine();
							AddEntryHtml( HeaderWidth + OffsetSize + ContentWidth, Color( c.AccessLevel.ToString(), 0xFF0000 ) );
							AddEntryHeader( HeaderWidth );
							last = (int)c.AccessLevel;
							line++;
						}

						if( line >= EntriesPerPage )
							break;

						AddNewLine();
						AddEntryHtml( HeaderWidth + OffsetSize + ContentWidth, EncodeHtml( c.Name ) );
						AddItemProperty( m_TooltipSerials[i] );
						AddEntryButton( HeaderWidth, ArrowRightID1, ArrowRightID2, DetailButtonBase + i, ArrowRightWidth, ArrowRightHeight );
					}
				}

				FinishPage();
			}

			public override void OnResponse( NetState sender, RelayInfo info )
			{
				Mobile m = sender.Mobile;
					switch( info.ButtonID )
					{
					case 0:
						{
							m.CloseGump( typeof( CommandInfoGump ) );
							CleanupTooltipItems( m );
							break;
						}
						case 1:
						{
							if( m_Page > 0 )
								m.SendGump( new CommandListGump( m_Page - 1, m, m_List, m_SearchTerm ) );

							break;
						}
						case 2:
						{
							if( (m_Page + 1) * EntriesPerPage < m_List.Count )
								m.SendGump( new CommandListGump( m_Page + 1, m, m_List, m_SearchTerm ) );

							break;
						}
						case SearchUpdateButtonID:
						{
							TextRelay text = info.GetTextEntry( SearchTextEntryID );
							string term = text == null ? String.Empty : text.Text;
							m.SendGump( new CommandListGump( 0, m, BuildSearchList( m, term ), term ) );

							break;
						}
					default:
						{

							int v = info.ButtonID - DetailButtonBase;

							if( v >= 0 && v < m_List.Count )
							{
								CommandInfo c = m_List[v].Info;

								if( m.AccessLevel >= c.AccessLevel )
								{
									m.SendGump( new CommandDescriptionGump( c ) );
									m.SendGump( new CommandListGump( m_Page, m, m_List, m_SearchTerm ) );
								}
								else
								{
									m.SendMessage( "You no longer have access to that command." );
									m.SendGump( new CommandListGump( m_Page, m, null, m_SearchTerm ) );
								}
							}
							break;
						}
				}
			}

			private static void CleanupTooltipItems( Mobile from )
			{
				List<Item> items;

				if( from == null || !m_ActiveTooltipItems.TryGetValue( from, out items ) )
					return;

				for( int i = 0; i < items.Count; ++i )
				{
					if( items[i] != null && !items[i].Deleted )
						items[i].Delete();
				}

				m_ActiveTooltipItems.Remove( from );
			}
		}

		private class CommandTooltipItem : Item
		{
			private string m_CommandName;
			private string m_CommandDescription;

			public override string DefaultName { get { return m_CommandName; } }

			public CommandTooltipItem( string commandName, string commandDescription )
				: base( 0x1F14 )
			{
				Visible = false;
				Movable = false;
				m_CommandName = commandName;
				m_CommandDescription = commandDescription;
			}

			public CommandTooltipItem( Serial serial )
				: base( serial )
			{
			}

			public override void GetProperties( ObjectPropertyList list )
			{
				base.GetProperties( list );
				list.Add( m_CommandDescription );
			}

			public override void AddNameProperties( ObjectPropertyList list )
			{
				base.AddNameProperties( list );
			}

			public override void Serialize( GenericWriter writer )
			{
				base.Serialize( writer );
				writer.Write( (int)0 );
				writer.Write( m_CommandName );
				writer.Write( m_CommandDescription );
			}

			public override void Deserialize( GenericReader reader )
			{
				base.Deserialize( reader );
				int version = reader.ReadInt();
				if( version >= 0 )
				{
					m_CommandName = reader.ReadString();
					m_CommandDescription = reader.ReadString();
				}
			}
		}

		public class CommandDescriptionGump : Gump
		{
			public CommandDescriptionGump( CommandInfo info )
				: base( 300, 50 )
			{
				AddPage( 0 );
				AddBackground( 0, 0, 420, 180, 5054 );
				AddHtml( 10, 10, 400, 22, String.Format( "<BASEFONT COLOR=#000000><CENTER>{0}</CENTER></BASEFONT>", info.Name ), false, false );
				AddHtml( 10, 36, 400, 134, info.Description, false, true );
			}
		}

		public class CommandInfoGump : Gump
		{
			public string Color( string text, int color )
			{
				return String.Format( "<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text );
			}

			public string Center( string text )
			{
				return String.Format( "<CENTER>{0}</CENTER>", text );
			}

			public CommandInfoGump( CommandInfo info )
				: this( info, 320, 200 )
			{
			}

			public CommandInfoGump( CommandInfo info, int width, int height )
				: base( 300, 50 )
			{
				AddPage( 0 );

				AddBackground( 0, 0, width, height, 5054 );

				//AddImageTiled( 10, 10, width - 20, 20, 2624 );
				//AddAlphaRegion( 10, 10, width - 20, 20 );
				//AddHtmlLocalized( 10, 10, width - 20, 20, header, headerColor, false, false );
				AddHtml( 10, 10, width - 20, 20, Color( Center( info.Name ), 0xFF0000 ), false, false );

				//AddImageTiled( 10, 40, width - 20, height - 80, 2624 );
				//AddAlphaRegion( 10, 40, width - 20, height - 80 );

				StringBuilder sb = new StringBuilder();

				sb.Append( "Usage: " );
				sb.Append( info.Usage.Replace( "<", "(" ).Replace( ">", ")" ) );
				sb.Append( "<BR>" );

				string[] aliases = info.Aliases;

				if( aliases != null && aliases.Length != 0 )
				{
					sb.Append( String.Format( "Alias{0}: ", aliases.Length == 1 ? "" : "es" ) );

					for( int i = 0; i < aliases.Length; ++i )
					{
						if( i != 0 )
							sb.Append( ", " );

						sb.Append( aliases[i] );
					}

					sb.Append( "<BR>" );
				}

				sb.Append( "AccessLevel: " );
				sb.Append( info.AccessLevel.ToString() );
				sb.Append( "<BR>" );
				sb.Append( "<BR>" );

				sb.Append( info.Description );

				AddHtml( 10, 40, width - 20, height - 80, sb.ToString(), false, true );

				//AddImageTiled( 10, height - 30, width - 20, 20, 2624 );
				//AddAlphaRegion( 10, height - 30, width - 20, 20 );

			}
		}
	}
}
