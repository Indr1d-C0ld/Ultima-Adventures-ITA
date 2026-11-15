using System;
using Server;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Items;
using Server.Regions;
using Server.Spells;
using Server.Network;
using Server.Multis;
using System.Collections;

namespace Server.Items 
{
	public class TrapWand : Item
	{
		private const int BuffTitleCliloc = 1151528;
		private const int BuffDescriptionCliloc = 1151529;
		private static readonly TimeSpan BuffDuration = TimeSpan.FromMinutes( 30.0 );

		private DateTime m_ExpiresAt;
		private BuffInfo m_TrapFindingBuff;

		public int WandPower;

		[CommandProperty(AccessLevel.Owner)]
		public int Wand_Power { get { return WandPower; } set { WandPower = value; InvalidateProperties(); } }

		public Mobile owner;
		
		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner
		{
			get{ return owner; }
			set
			{
				if ( owner == value )
					return;

				RemoveTrapFindingBuff();
				owner = value;
				RenameWand();
				UpdateTrapFindingBuff();
			}
		}

		[Constructable]
		public TrapWand() : this( null )
		{
		}
		
		[Constructable]
		public TrapWand( Mobile from ) : base( 0x4FD6 )
		{
			this.owner = from;
			m_ExpiresAt = DateTime.UtcNow + BuffDuration;
			Weight = 1.0;
			LootType = LootType.Blessed;
			Hue = Server.Misc.RandomThings.GetRandomColor(0);
			Light = LightType.Circle150;
			RenameWand();
			ItemRemovalTimer thisTimer = new ItemRemovalTimer( this );
			thisTimer.Start();
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			list.Add( 1070722, WandPower + "% Avoiding Traps on the Walls or Floors");
			list.Add( 1049644, "Must be in backpack and lasts 30 minutes"); // PARENTHESIS
        }

		private void RenameWand()
		{
			if ( owner != null )
			{
				this.Name = "orb of trap finding for " + owner.Name;
			}
			else
			{
				this.Name = "orb of trap finding";
			}		
		}

		public TrapWand( Serial serial ) : base( serial )
		{ 
		} 

		private bool IsInOwnerBackpack()
		{
			PlayerMobile player = owner as PlayerMobile;

			return player != null && !player.Deleted && player.Backpack != null &&
				RootParent == player && IsChildOf( player.Backpack );
		}

		private bool IsTrapFindingActive()
		{
			return IsInOwnerBackpack() && m_ExpiresAt != DateTime.MinValue && DateTime.UtcNow < m_ExpiresAt;
		}

		private void UpdateTrapFindingBuff()
		{
			if ( m_TrapFindingBuff != null && !IsTrapFindingActive() )
			{
				RemoveTrapFindingBuff();
				return;
			}

			if ( m_TrapFindingBuff == null && IsTrapFindingActive() )
			{
				PlayerMobile player = owner as PlayerMobile;
				TimeSpan remaining = m_ExpiresAt - DateTime.UtcNow;

				if ( player != null && remaining > TimeSpan.Zero )
				{
					m_TrapFindingBuff = new BuffInfo( BuffIcon.TrapFinding, BuffTitleCliloc, BuffDescriptionCliloc, remaining, player );
					BuffInfo.AddBuff( player, m_TrapFindingBuff );
				}
			}
		}

		private void RemoveTrapFindingBuff()
		{
			if ( m_TrapFindingBuff == null )
				return;

			BuffInfo.RemoveBuff( owner, m_TrapFindingBuff );
			m_TrapFindingBuff = null;
		}

		internal void UpdateBuffState()
		{
			UpdateTrapFindingBuff();
		}

		public override void OnAdded( IEntity parent )
		{
			base.OnAdded( parent );
			UpdateTrapFindingBuff();
		}

		public override void OnRemoved( IEntity parent )
		{
			RemoveTrapFindingBuff();
			base.OnRemoved( parent );
		}

		public override void OnDelete()
		{
			RemoveTrapFindingBuff();
			base.OnDelete();
		}
		
		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 0 ); 
			writer.Write( (Mobile)owner);
            writer.Write( WandPower );
			RenameWand();
		} 
		
		public override void Deserialize(GenericReader reader) 
		{ 
			base.Deserialize( reader ); 
			int version = reader.ReadInt();
			owner = reader.ReadMobile();
            WandPower = reader.ReadInt();
			this.Delete(); // none when the world starts 
			RenameWand();
		}

		public class ItemRemovalTimer : Timer 
		{ 
			private Item i_item; 
			public ItemRemovalTimer( Item item ) : base( TimeSpan.FromMinutes( 30.0 ) ) 
			{ 
				Priority = TimerPriority.OneSecond; 
				i_item = item; 
			} 

			protected override void OnTick() 
			{ 
				if (( i_item != null ) && ( !i_item.Deleted ))
				{
					TrapWand wands = (TrapWand)i_item;
					Mobile from = wands.owner;

					if ( from != null )
					{
						from.LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, "Your trap finding orb has vanished.");
						from.PlaySound( 0x1F0 );
					}

					i_item.Delete();
				}
			} 
		} 
	}
}
