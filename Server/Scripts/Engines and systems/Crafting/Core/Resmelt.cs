using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Targeting;
using Server.Items;

namespace Server.Engines.Craft
{
	public enum SmeltResult
	{
		Success,
		Invalid,
		NoSkill
	}

	public class Resmelt
	{
		public Resmelt()
		{
		}

		public static void Do( Mobile from, CraftSystem craftSystem, BaseTool tool )
		{
			int num = craftSystem.CanCraft( from, tool, null );

			if ( num > 0 && num != 1044267 )
			{
				from.SendGump( new CraftGump( from, craftSystem, tool, num ) );
			}
			else
			{
				from.Target = new InternalTarget( craftSystem, tool );
				from.SendLocalizedMessage( 1044273 ); // Target an item to recycle.
			}
		}

		private class InternalTarget : Target
		{
			private CraftSystem m_CraftSystem;
			private BaseTool m_Tool;
			private static readonly TimeSpan SmeltAllInterval = TimeSpan.FromSeconds( 0.75 );

			private class SmeltAllState
			{
				public Mobile From;
				public List<Item> Items;
				public int Index;
				public int SuccessCount;
				public SmeltResult FirstFailure;
				public bool FirstFailureStoreBought;
				public bool Cancelled;
			}

			private class SmeltAllProgressGump : Gump
			{
				private readonly SmeltAllState m_State;

				public SmeltAllProgressGump( Mobile from, SmeltAllState state ) : base( 50, 50 )
				{
					m_State = state;

					Closable = true;
					Disposable = true;
					Dragable = true;
					Resizable = false;

					from.CloseGump( typeof( SmeltAllProgressGump ) );

					AddPage( 0 );
					AddBackground( 0, 0, 260, 120, 9270 );
					AddHtml( 20, 20, 220, 60, "<BASEFONT COLOR=#FFFFFF>Smelting all items...<BR>Close this gump to stop smelting.</BASEFONT>", true, true );
				}

				public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
				{
					if ( m_State != null )
						m_State.Cancelled = true;
				}
			}

			public InternalTarget( CraftSystem craftSystem, BaseTool tool ) :  base ( 2, false, TargetFlags.None )
			{
				m_CraftSystem = craftSystem;
				m_Tool = tool;
			}

			private SmeltResult Resmelt( Mobile from, Item item, CraftResource resource )
			{
				try
				{
					if ( CraftResources.GetType( resource ) != CraftResourceType.Metal )
						return SmeltResult.Invalid;

					CraftResourceInfo info = CraftResources.GetInfo( resource );

					if ( info == null || info.ResourceTypes.Length == 0 )
						return SmeltResult.Invalid;

					CraftItem craftItem = m_CraftSystem.CraftItems.SearchFor( item.GetType() );

					if ( craftItem == null || craftItem.Resources.Count == 0 )
						return SmeltResult.Invalid;

					CraftRes craftResource = craftItem.Resources.GetAt( 0 );

					if ( craftResource.Amount < 2 )
						return SmeltResult.Invalid; // Not enough metal to resmelt

					double difficulty = 0.0;

					switch ( resource )
					{
						case CraftResource.Mithril: difficulty = 110.0; break;
						case CraftResource.Xormite: difficulty = 115.0; break;
						case CraftResource.Dwarven: difficulty = 120.0; break;
						default: difficulty = CraftResources.GetMetalProcessDifficulty(resource); break;
					}

					if ( difficulty > from.Skills[ SkillName.Mining ].Value )
						return SmeltResult.NoSkill;

					Type resourceType = info.ResourceTypes[0];
					Item ingot = (Item)Activator.CreateInstance( resourceType );

					if ( item is DragonBardingDeed || (item is BaseArmor && ((BaseArmor)item).PlayerConstructed) || (item is BaseWeapon && ((BaseWeapon)item).PlayerConstructed) || (item is BaseClothing && ((BaseClothing)item).PlayerConstructed) )
						ingot.Amount = craftResource.Amount / 2;
					else
						ingot.Amount = 1;

					item.Delete();
					from.AddToBackpack( ingot );

					from.PlaySound( 0x2A );
					from.PlaySound( 0x240 );
					return SmeltResult.Success;
				}
				catch
				{
				}

				return SmeltResult.Invalid;
			}

			private bool CheckCanSmelt( Mobile from )
			{
				int num = m_CraftSystem.CanCraft( from, m_Tool, null );

				if ( num <= 0 )
					return true;

				if ( num == 1044267 )
				{
					bool anvil, forge;

					DefBlacksmithy.CheckAnvilAndForge( from, 2, out anvil, out forge );

					if ( !anvil )
						num = 1044266; // You must be near an anvil
					else if ( !forge )
						num = 1044265; // You must be near a forge.
				}

				from.SendGump( new CraftGump( from, m_CraftSystem, m_Tool, num ) );
				return false;
			}

			private SmeltResult TryResmeltItem( Mobile from, Item item, out bool isStoreBought )
			{
				isStoreBought = false;

				if ( item is BaseArmor )
				{
					BaseArmor armor = (BaseArmor)item;
					isStoreBought = !armor.PlayerConstructed;
					return Resmelt( from, armor, armor.Resource );
				}

				if ( item is BaseWeapon )
				{
					BaseWeapon weapon = (BaseWeapon)item;
					isStoreBought = !weapon.PlayerConstructed;
					return Resmelt( from, weapon, weapon.Resource );
				}

				if ( item is DragonBardingDeed )
				{
					DragonBardingDeed deed = (DragonBardingDeed)item;
					return Resmelt( from, deed, deed.Resource );
				}

				return SmeltResult.Invalid;
			}

			private void HandleSmeltResult( Mobile from, SmeltResult result, bool isStoreBought )
			{
				int message = GetSmeltMessage( result, isStoreBought );

				if ( result == SmeltResult.Success )
				{
					from.SendLocalizedMessage( message );
					from.Target = new InternalTarget( m_CraftSystem, m_Tool );
					from.SendLocalizedMessage( 1044273 ); // Target an item to recycle.
				}
				else
				{
					from.SendGump( new CraftGump( from, m_CraftSystem, m_Tool, message ) );
				}
			}

			private int GetSmeltMessage( SmeltResult result, bool isStoreBought )
			{
				switch ( result )
				{
					default:
					case SmeltResult.Invalid: return 1044272; // You can't melt that down into ingots.
					case SmeltResult.NoSkill: return 1044269; // You have no idea how to work this metal.
					case SmeltResult.Success: return isStoreBought ? 500418 : 1044270; // You melt the item down into ingots.
				}
			}

			private void SmeltMatchingItems( Mobile from, Type targetType )
			{
				if ( from == null || from.Deleted || targetType == null )
					return;

				if ( !CheckCanSmelt( from ) )
					return;

				Container pack = from.Backpack;

				if ( pack == null )
				{
					HandleSmeltResult( from, SmeltResult.Invalid, false );
					return;
				}

				Item[] items = pack.FindItemsByType( targetType, true );
				List<Item> queue = new List<Item>( items.Length );

				for ( int i = 0; i < items.Length; ++i )
				{
					if ( items[i] != null && !items[i].Deleted )
						queue.Add( items[i] );
				}

				if ( queue.Count == 0 )
				{
					HandleSmeltResult( from, SmeltResult.Invalid, false );
					return;
				}

				SmeltAllState state = new SmeltAllState
				{
					From = from,
					Items = queue,
					Index = 0,
					SuccessCount = 0,
					FirstFailure = SmeltResult.Success,
					FirstFailureStoreBought = false,
					Cancelled = false
				};

				from.SendGump( new SmeltAllProgressGump( from, state ) );
				Timer.DelayCall( SmeltAllInterval, new TimerStateCallback( ProcessSmeltMatchingItems ), state );
			}

			private void ProcessSmeltMatchingItems( object stateObj )
			{
				SmeltAllState state = stateObj as SmeltAllState;

				if ( state == null || state.From == null || state.From.Deleted )
					return;

				Mobile from = state.From;

				if ( state.Cancelled )
				{
					from.SendMessage( "Smelt all cancelled." );
					from.CloseGump( typeof( SmeltAllProgressGump ) );
					return;
				}

				if ( !CheckCanSmelt( from ) )
				{
					from.CloseGump( typeof( SmeltAllProgressGump ) );
					return;
				}

				if ( state.Index >= state.Items.Count )
				{
					FinishSmeltMatchingItems( state );
					return;
				}

				Item item = state.Items[state.Index++];

				if ( item != null && !item.Deleted )
				{
					bool isStoreBought;
					SmeltResult result = TryResmeltItem( from, item, out isStoreBought );

					if ( result == SmeltResult.Success )
					{
						state.SuccessCount++;
						from.SendLocalizedMessage( GetSmeltMessage( result, isStoreBought ) );
					}
					else if ( state.FirstFailure == SmeltResult.Success )
					{
						state.FirstFailure = result;
						state.FirstFailureStoreBought = isStoreBought;
					}
				}

				if ( state.Index >= state.Items.Count )
				{
					FinishSmeltMatchingItems( state );
				}
				else
				{
					Timer.DelayCall( SmeltAllInterval, new TimerStateCallback( ProcessSmeltMatchingItems ), state );
				}
			}

			private void FinishSmeltMatchingItems( SmeltAllState state )
			{
				if ( state == null || state.From == null || state.From.Deleted )
					return;

				state.From.CloseGump( typeof( SmeltAllProgressGump ) );

				if ( state.SuccessCount > 0 )
				{
					state.From.Target = new InternalTarget( m_CraftSystem, m_Tool );
					state.From.SendLocalizedMessage( 1044273 ); // Target an item to recycle.
				}
				else
				{
					HandleSmeltResult( state.From, state.FirstFailure, state.FirstFailureStoreBought );
				}
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( !CheckCanSmelt( from ) )
					return;

				Item item = targeted as Item;

				if ( item == null || ( !(item is BaseArmor) && !(item is BaseWeapon) && !(item is DragonBardingDeed) ) )
				{
					HandleSmeltResult( from, SmeltResult.Invalid, false );
					return;
				}

				bool isStoreBought;
				SmeltResult result = TryResmeltItem( from, item, out isStoreBought );
				HandleSmeltResult( from, result, isStoreBought );

				if ( result != SmeltResult.Success )
					return;

				from.SendGump( new ConfirmationGump(
					from,
					"Smelt all matching items?",
					"Do you want to smelt all items of this type?",
					delegate { SmeltMatchingItems( from, item.GetType() ); } ) );
			}
		}
	}
}
