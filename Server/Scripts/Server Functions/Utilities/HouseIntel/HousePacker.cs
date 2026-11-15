using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Accounting;
using Server.Items;
using Server.Multis;
using Server.Commands;

namespace Server.Utilities
{
	public class HousePackResult
	{
		public int MovedCount;
		public int SkippedCount;
		public List<string> SkipReasons = new List<string>();
		public List<string> Failures = new List<string>();
		public Container TargetContainer;
		public MovingCrate Crate;
	}

	public class HousePackPreview
	{
		public int MoveRootCount;
		public int MoveItemCount;
		public int SkippedCount;
		public int ProtectedSkippedCount;
		public int AddonDeedCount;
		public Dictionary<string, int> TopItemCounts = new Dictionary<string, int>();
		public List<string> SkipReasons = new List<string>();
		public bool OwnerBankHasSpace;
		public bool EscrowBankHasSpace;
	}

	public static class HousePacker
	{
		private const int MaxTypeRows = 50;

		public static HousePackPreview BuildPreview( Mobile admin, BaseHouse house )
		{
			HousePackPreview preview = new HousePackPreview();

			if ( house == null )
				return preview;

			Dictionary<string, int> typeCounts = new Dictionary<string, int>();

			foreach ( Item root in HouseScanner.EnumerateRootItems( house ) )
			{
				if ( HouseScanner.IsExcludedItem( house, root ) )
					continue;

				string reason;
				if ( ShouldSkipItem( house, root, out reason ) )
				{
					preview.SkippedCount++;
					preview.SkipReasons.Add( FormatSkip( root, reason ) );
					continue;
				}

				if ( root is Container && ContainsProtectedItem( root ) )
				{
					preview.SkippedCount++;
					preview.ProtectedSkippedCount++;
					preview.SkipReasons.Add( FormatSkip( root, "container contains protected items" ) );
					continue;
				}

				preview.MoveRootCount++;
				CountItemsRecursive( root, typeCounts, ref preview.MoveItemCount );
			}

			preview.AddonDeedCount = PreviewAddonDeeds( house );

			preview.TopItemCounts = typeCounts
				.OrderByDescending( kvp => kvp.Value )
				.ThenBy( kvp => kvp.Key )
				.Take( MaxTypeRows )
				.ToDictionary( kvp => kvp.Key, kvp => kvp.Value );

			preview.OwnerBankHasSpace = EstimateBankSpace( GetOwnerBank( house ), admin );
			preview.EscrowBankHasSpace = EstimateBankSpace( GetEscrowContainer(), admin );

			return preview;
		}

		public static HousePackResult ExecutePack( Mobile admin, BaseHouse house, Container targetContainer )
		{
			HousePackResult result = new HousePackResult
			{
				TargetContainer = targetContainer
			};

			if ( house == null || house.Deleted )
			{
				result.Failures.Add( "House not found or deleted." );
				return result;
			}

			if ( targetContainer == null )
			{
				result.Failures.Add( "No valid target container was found." );
				return result;
			}

			MovingCrate crate = house.MovingCrate ?? new MovingCrate( house );
			crate.Movable = true;
			house.MovingCrate = crate;
			result.Crate = crate;

			PackAddons( admin, house, targetContainer, result );

			foreach ( Item root in HouseScanner.EnumerateRootItems( house ) )
			{
				if ( HouseScanner.IsExcludedItem( house, root ) )
				{
					result.SkippedCount++;
					result.SkipReasons.Add( FormatSkip( root, "excluded structural item" ) );
					continue;
				}

				string reason;
				if ( ShouldSkipItem( house, root, out reason ) )
				{
					result.SkippedCount++;
					result.SkipReasons.Add( FormatSkip( root, reason ) );
					continue;
				}

				if ( root is Container && ContainsProtectedItem( root ) )
				{
					result.SkippedCount++;
					result.SkipReasons.Add( FormatSkip( root, "container contains protected items" ) );
					continue;
				}

				try
				{
					house.DropToMovingCrate( root );
					result.MovedCount++;
				}
				catch ( Exception ex )
				{
					result.SkippedCount++;
					result.Failures.Add( FormatSkip( root, ex.Message ) );
				}
			}

			if ( !TryDrop( admin, targetContainer, crate ) )
			{
				result.Failures.Add( "Failed to move packing crate to target container." );
			}

			AuditPack( admin, house, result );
			return result;
		}

		public static Container GetOwnerBank( BaseHouse house )
		{
			if ( house == null || house.Owner == null )
				return null;

			return house.Owner.BankBox;
		}

		public static Container GetEscrowBank()
		{
			if ( String.IsNullOrWhiteSpace( HouseIntelConfig.AdminEscrowAccount ) )
				return null;

			Account acct = Accounts.GetAccount( HouseIntelConfig.AdminEscrowAccount ) as Account;
			if ( acct == null )
				return null;

			for ( int i = 0; i < acct.Length; i++ )
			{
				Mobile m = acct[i];
				if ( m != null )
					return m.BankBox;
			}

			return null;
		}

		public static Container GetEscrowContainer()
		{
			Container bank = GetEscrowBank();
			if ( bank == null )
				return null;

			if ( String.IsNullOrWhiteSpace( HouseIntelConfig.AdminEscrowContainerName ) )
				return bank;

			Container existing = bank.FindItemByType<Container>( true, item => item != null && item.Name == HouseIntelConfig.AdminEscrowContainerName );
			if ( existing != null )
				return existing;

			Bag bag = new Bag
			{
				Name = HouseIntelConfig.AdminEscrowContainerName
			};

			Mobile owner = bank.RootParent as Mobile;
			if ( owner != null && bank.TryDropItem( owner, bag, false ) )
				return bag;

			bag.Delete();
			return bank;
		}

		public static bool EstimateBankSpace( Container bank, Mobile admin )
		{
			if ( bank == null )
				return false;

			DummyCrateItem dummy = new DummyCrateItem();
			bool ok = bank.CheckHold( admin, dummy, false );
			dummy.Delete();
			return ok;
		}

		public static bool TryDrop( Mobile admin, Container target, Item item )
		{
			if ( target == null || item == null )
				return false;

			return target.TryDropItem( admin, item, false );
		}

		private static bool ShouldSkipItem( BaseHouse house, Item item, out string reason )
		{
			reason = null;

			if ( item == null || item.Deleted )
			{
				reason = "deleted item";
				return true;
			}

			if ( IsProtectedItem( item ) )
			{
				reason = "protected/blessed item";
				return true;
			}

			bool movable = item.Movable || house.IsLockedDown( item ) || house.IsSecure( item );

			if ( !movable )
			{
				reason = "immovable item";
				return true;
			}

			return false;
		}

		private static bool IsProtectedItem( Item item )
		{
			if ( item == null )
				return true;

			return item.LootType == LootType.Blessed || item.LootType == LootType.Newbied || item.LootType == LootType.Ensouled;
		}

		private static bool ContainsProtectedItem( Item root )
		{
			Container container = root as Container;
			if ( container == null )
				return false;

			List<Item> list = new List<Item>();
			list.AddRange( container.Items );

			for ( int i = 0; i < list.Count; i++ )
			{
				Item item = list[i];

				if ( IsProtectedItem( item ) )
					return true;

				Container sub = item as Container;
				if ( sub != null )
					list.AddRange( sub.Items );
			}

			return false;
		}

		private static int PreviewAddonDeeds( BaseHouse house )
		{
			if ( house == null || house.Addons == null )
				return 0;

			int count = 0;

			foreach ( object entry in house.Addons )
			{
				Item item = entry as Item;
				if ( item == null || item.Deleted )
					continue;

				IAddon addon = item as IAddon;
				if ( addon != null && addon.Deed != null )
					count++;
			}

			return count;
		}

		private static void PackAddons( Mobile admin, BaseHouse house, Container targetContainer, HousePackResult result )
		{
			if ( house == null || house.Addons == null )
				return;

			ArrayList addons = new ArrayList( house.Addons );

			foreach ( object entry in addons )
			{
				Item item = entry as Item;
				if ( item == null || item.Deleted )
					continue;

				IAddon addon = item as IAddon;
				if ( addon == null )
					continue;

				if ( item is BaseAddonContainer )
				{
					if ( !MoveAddonContainerItems( house, (BaseAddonContainer)item, result ) )
						continue;
				}

				Item deed = addon.Deed;

				if ( deed == null )
				{
					result.SkippedCount++;
					result.SkipReasons.Add( FormatSkip( item, "addon has no deed" ) );
					continue;
				}

				ApplyAddonHue( item, deed );

				house.Addons.Remove( item );
				item.Delete();

				if ( !TryDrop( admin, targetContainer, deed ) )
				{
					result.Failures.Add( FormatSkip( deed, "failed to drop addon deed to target container" ) );
					house.DropToMovingCrate( deed );
				}
				else
				{
					result.MovedCount++;
				}
			}
		}

		private static bool MoveAddonContainerItems( BaseHouse house, BaseAddonContainer container, HousePackResult result )
		{
			if ( container == null )
				return false;

			bool canDeed = true;

			for ( int i = container.Items.Count - 1; i >= 0; i-- )
			{
				Item item = container.Items[i];
				if ( item == null )
					continue;

				string reason;
				if ( ShouldSkipItem( house, item, out reason ) )
				{
					result.SkippedCount++;
					result.SkipReasons.Add( FormatSkip( item, reason ) );
					canDeed = false;
					continue;
				}

				house.DropToMovingCrate( item );
			}

			if ( !canDeed )
				result.SkipReasons.Add( FormatSkip( container, "addon container contains skipped items; addon not deeded" ) );

			return canDeed;
		}

		private static void ApplyAddonHue( Item addonItem, Item deed )
		{
			BaseAddon baseAddon = addonItem as BaseAddon;
			BaseAddonContainer baseAddonContainer = addonItem as BaseAddonContainer;

			if ( baseAddon != null )
			{
				BaseAddonDeed addonDeed = deed as BaseAddonDeed;
				if ( addonDeed != null && baseAddon.RetainDeedHue )
					addonDeed.Hue = GetAddonHue( baseAddon.Components );
			}
			else if ( baseAddonContainer != null )
			{
				BaseAddonContainerDeed addonDeed = deed as BaseAddonContainerDeed;
				if ( addonDeed != null )
				{
					addonDeed.Resource = baseAddonContainer.Resource;
					if ( baseAddonContainer.RetainDeedHue )
						addonDeed.Hue = GetAddonHue( baseAddonContainer.Components );
				}
			}
		}

		private static int GetAddonHue<T>( List<T> components ) where T : Item
		{
			if ( components == null )
				return 0;

			for ( int i = 0; i < components.Count; i++ )
			{
				Item component = components[i];
				if ( component != null && component.Hue != 0 )
					return component.Hue;
			}

			return 0;
		}

		private static void CountItemsRecursive( Item item, Dictionary<string, int> typeCounts, ref int totalCount )
		{
			if ( item == null || item.Deleted )
				return;

			totalCount++;

			string key = item.GetType().Name;
			if ( typeCounts.ContainsKey( key ) )
				typeCounts[key]++;
			else
				typeCounts[key] = 1;

			List<Item> items = item.Items;
			for ( int i = 0; i < items.Count; ++i )
				CountItemsRecursive( items[i], typeCounts, ref totalCount );
		}

		private static string FormatSkip( Item item, string reason )
		{
			string name = item != null ? item.GetType().Name : "UnknownItem";
			string serial = item != null ? item.Serial.ToString() : "0x0";
			return String.Format( "{0} ({1}): {2}", name, serial, reason );
		}

		private static void AuditPack( Mobile admin, BaseHouse house, HousePackResult result )
		{
			string message = String.Format(
				"HousePack admin={0} house=0x{1:X} loc={2} owner={3} target={4} moved={5} skipped={6} failures={7}",
				admin,
				house.Serial.Value,
				house.Location,
					house.Owner != null ? house.Owner.ToString() : "none",
				result.TargetContainer != null ? result.TargetContainer.GetType().Name : "none",
				result.MovedCount,
				result.SkippedCount,
				result.Failures.Count );

			CommandLogging.WriteLine( admin, message );

			try
			{
				if ( !Directory.Exists( "Logs" ) )
					Directory.CreateDirectory( "Logs" );

				using ( StreamWriter op = new StreamWriter( "Logs/HouseIntel.log", true ) )
				{
					op.WriteLine( "{0} {1}", DateTime.UtcNow, message );

					foreach ( string skip in result.SkipReasons )
						op.WriteLine( "  SKIP: {0}", skip );

					foreach ( string fail in result.Failures )
						op.WriteLine( "  FAIL: {0}", fail );
				}
			}
			catch
			{
			}
		}

		private class DummyCrateItem : Item
		{
			public DummyCrateItem() : base( 0xE3D )
			{
			}

			public DummyCrateItem( Serial serial ) : base( serial )
			{
			}

			public override void Serialize( GenericWriter writer )
			{
				base.Serialize( writer );

				writer.Write( (int)0 );
			}

			public override void Deserialize( GenericReader reader )
			{
				base.Deserialize( reader );

				reader.ReadInt();
			}
		}
	}
}
