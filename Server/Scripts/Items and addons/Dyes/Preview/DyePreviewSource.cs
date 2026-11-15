using System;
using Server.Mobiles;

namespace Server.Items
{
	public interface IDyePreviewSource
	{
		Item SourceItem { get; }
		int PreviewHue { get; }

		bool CanBeginPreview( Mobile from, out string failureMessage );
		bool CanPreviewTarget( Mobile from, Item target, int selectedHue, out string failureMessage );
		bool CanApply( Mobile from, Item target, int selectedHue, out string failureMessage );
		bool ApplyPermanentDye( Mobile from, Item target, int selectedHue );
		bool IsSourceStillValid( Mobile from );

		string GetSourceDisplayName();
		string GetConsumptionMessage();
		void BeginTargeting( Mobile from );
	}

	public static class DyePreviewUtility
	{
		public const int MovementAllowance = 2;
		public static readonly TimeSpan SessionTimeout = TimeSpan.FromSeconds( 45.0 );

		public static bool IsWearableGear( Item item )
		{
			if ( item == null || item.Deleted )
				return false;

			Layer layer = item.Layer;

			if ( layer < Layer.FirstValid || layer > Layer.LastUserValid )
				return false;

			return layer != Layer.Hair
				&& layer != Layer.FacialHair
				&& layer != Layer.Backpack;
		}

		public static bool IsOwnedGear( Mobile from, Item item )
		{
			if ( from == null || from.Deleted || item == null || item.Deleted )
				return false;

			if ( !IsWearableGear( item ) || item.InSecureTrade )
				return false;

			if ( item.Parent == from )
				return true;

			Container backpack = from.Backpack;

			return backpack != null && !backpack.Deleted && item.IsChildOf( backpack );
		}

		public static bool IsEquippedBy( Mobile from, Item item )
		{
			return from != null && item != null && !item.Deleted
				&& item.Parent == from && IsWearableGear( item );
		}

		public static bool IsInBackpack( Mobile from, Item item )
		{
			if ( from == null || item == null || item.Deleted )
				return false;

			Container backpack = from.Backpack;

			return backpack != null && !backpack.Deleted && item.IsChildOf( backpack );
		}

		public static string GetItemDisplayName( Item item )
		{
			if ( item == null )
				return "item";

			if ( !String.IsNullOrEmpty( item.Name ) )
				return item.Name;

			return item.GetType().Name;
		}

		public static string EncodeHtml( string text )
		{
			if ( String.IsNullOrEmpty( text ) )
				return String.Empty;

			return text.Replace( "&", "&amp;" ).Replace( "<", "&lt;" ).Replace( ">", "&gt;" );
		}
	}
}
