using Server.Network;

namespace Server.Items
{
	public enum DyePreviewDisplayMode
	{
		Invalid,
		Equipped,
		ContainerLegacy,
		ContainerGrid
	}

	public sealed class DyePreviewDisplay
	{
		private readonly DyePreviewDisplayMode m_Mode;
		private readonly int m_Hue;
		private readonly Mobile m_Recipient;

		public DyePreviewDisplayMode Mode
		{
			get { return m_Mode; }
		}

		public int Hue
		{
			get { return m_Hue; }
		}

		public Mobile Recipient
		{
			get { return m_Recipient; }
		}

		public DyePreviewDisplay( DyePreviewDisplayMode mode, int hue, Mobile recipient )
		{
			m_Mode = mode;
			m_Hue = hue;
			m_Recipient = recipient;
		}
	}

	public static class DyePreviewPackets
	{
		public static DyePreviewDisplay Describe( Mobile from, Item item, int hue, bool containerGridLines )
		{
			if ( DyePreviewUtility.IsEquippedBy( from, item ) )
				return new DyePreviewDisplay( DyePreviewDisplayMode.Equipped, hue, from );

			if ( DyePreviewUtility.IsInBackpack( from, item ) && item.Parent is Item )
			{
				return new DyePreviewDisplay(
					containerGridLines ? DyePreviewDisplayMode.ContainerGrid : DyePreviewDisplayMode.ContainerLegacy,
					hue,
					from );
			}

			return new DyePreviewDisplay( DyePreviewDisplayMode.Invalid, hue, from );
		}

		public static bool SendPreview( Mobile from, Item item, int hue )
		{
			if ( from == null || item == null || item.Deleted )
				return false;

			// Send only through the requesting player's NetState; Item.Hue is never changed here.
			NetState state = from.NetState;

			if ( state == null )
				return DyePreviewUtility.IsOwnedGear( from, item );

			DyePreviewDisplay display = Describe( from, item, hue, state.ContainerGridLines );

			switch ( display.Mode )
			{
				case DyePreviewDisplayMode.Equipped:
					state.Send( new DyePreviewEquipUpdate( item, from, hue ) );
					return true;
				case DyePreviewDisplayMode.ContainerGrid:
					state.Send( new DyePreviewContainerUpdate6017( item, hue ) );
					return true;
				case DyePreviewDisplayMode.ContainerLegacy:
					state.Send( new DyePreviewContainerUpdate( item, hue ) );
					return true;
			}

			return false;
		}

		public static void Restore( Mobile from, Item item )
		{
			if ( from == null || item == null )
				return;

			NetState state = from.NetState;

			if ( state == null )
				return;

			state.Send( new RemoveItem( item ) );

			if ( item.Deleted )
				return;

			if ( DyePreviewUtility.IsEquippedBy( from, item ) )
			{
				state.Send( new EquipUpdate( item ) );
			}
			else if ( DyePreviewUtility.IsInBackpack( from, item ) && item.Parent is Item )
			{
				if ( state.ContainerGridLines )
					state.Send( new ContainerContentUpdate6017( item ) );
				else
					state.Send( new ContainerContentUpdate( item ) );
			}
			else if ( item.Parent is Mobile && from.CanSee( item ) )
			{
				state.Send( new EquipUpdate( item ) );
			}
			else if ( item.Parent is Item && from.CanSee( item ) )
			{
				if ( state.ContainerGridLines )
					state.Send( new ContainerContentUpdate6017( item ) );
				else
					state.Send( new ContainerContentUpdate( item ) );
			}
			else if ( item.Parent == null && item.Map == from.Map && from.CanSee( item ) )
			{
				state.Send( new WorldItem( item ) );
			}
		}

		private sealed class DyePreviewEquipUpdate : Packet
		{
			public DyePreviewEquipUpdate( Item item, Mobile owner, int hue ) : base( 0x2E, 15 )
			{
				m_Stream.Write( (int)item.Serial );
				m_Stream.Write( (short)item.ItemID );
				m_Stream.Write( (byte)0 );
				m_Stream.Write( (byte)item.Layer );
				m_Stream.Write( (int)owner.Serial );
				m_Stream.Write( (short)hue );
			}
		}

		private sealed class DyePreviewContainerUpdate : Packet
		{
			public DyePreviewContainerUpdate( Item item, int hue ) : base( 0x25, 20 )
			{
				Item parent = item.Parent as Item;

				m_Stream.Write( (int)item.Serial );
				m_Stream.Write( (ushort)item.ItemID );
				m_Stream.Write( (byte)0 );
				m_Stream.Write( (ushort)item.Amount );
				m_Stream.Write( (short)item.X );
				m_Stream.Write( (short)item.Y );
				m_Stream.Write( (int)( parent != null ? parent.Serial : Serial.Zero ) );
				m_Stream.Write( (ushort)hue );
			}
		}

		private sealed class DyePreviewContainerUpdate6017 : Packet
		{
			public DyePreviewContainerUpdate6017( Item item, int hue ) : base( 0x25, 21 )
			{
				Item parent = item.Parent as Item;

				m_Stream.Write( (int)item.Serial );
				m_Stream.Write( (ushort)item.ItemID );
				m_Stream.Write( (byte)0 );
				m_Stream.Write( (ushort)item.Amount );
				m_Stream.Write( (short)item.X );
				m_Stream.Write( (short)item.Y );
				m_Stream.Write( (byte)0 );
				m_Stream.Write( (int)( parent != null ? parent.Serial : Serial.Zero ) );
				m_Stream.Write( (ushort)hue );
			}
		}
	}
}
