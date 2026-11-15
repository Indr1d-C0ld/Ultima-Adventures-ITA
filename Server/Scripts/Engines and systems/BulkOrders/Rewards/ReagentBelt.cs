using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Gumps;
using Server.Network;
using Server.Items;

namespace Server.Items
{
	public class ReagentBelt : Container
	{
		private bool m_AutoLootBlackPearl = true;
		private bool m_AutoLootBloodmoss = true;
		private bool m_AutoLootGarlic = true;
		private bool m_AutoLootGinseng = true;
		private bool m_AutoLootMandrakeRoot = true;
		private bool m_AutoLootNightshade = true;
		private bool m_AutoLootSulfurousAsh = true;
		private bool m_AutoLootSpidersSilk = true;
		private bool m_AutoLootBrimstone = true;
		private bool m_AutoLootButterflyWings = true;
		private bool m_AutoLootEyeOfToad = true;
		private bool m_AutoLootFairyEgg = true;
		private bool m_AutoLootGargoyleEar = true;
		private bool m_AutoLootBeetleShell = true;
		private bool m_AutoLootMoonCrystal = true;
		private bool m_AutoLootPixieSkull = true;
		private bool m_AutoLootRedLotus = true;
		private bool m_AutoLootSeaSalt = true;
		private bool m_AutoLootSilverWidow = true;
		private bool m_AutoLootSwampBerries = true;
		private bool m_AutoLootBatWing = true;
		private bool m_AutoLootGraveDust = true;
		private bool m_AutoLootDaemonBlood = true;
		private bool m_AutoLootNoxCrystal = true;
		private bool m_AutoLootPigIron = true;

        [Constructable]
		public ReagentBelt() : this(32)      // create a 125 item bag by default
		{
		}
		
		[Constructable]
		public ReagentBelt(int maxitems) : base(0x2790)
		{
			Weight = 1.0;
			MaxItems = maxitems;
			Name = "a reagent belt";
			Layer = Layer.Waist;
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (!(dropped is BaseReagent))
			{
				from.SendMessage("It seems you can't place that item in this bag.");
				return false;
			}
			return base.OnDragDrop(from, dropped);
		}

		public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
		{
			if (!(item is BaseReagent))
			{
				from.SendMessage("It seems you can't place that item in this bag.");
				return false;
			}
			return base.OnDragDropInto(from, item, p);
		}

		public ReagentBelt( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version
			writer.Write( m_AutoLootBlackPearl );
			writer.Write( m_AutoLootBloodmoss );
			writer.Write( m_AutoLootGarlic );
			writer.Write( m_AutoLootGinseng );
			writer.Write( m_AutoLootMandrakeRoot );
			writer.Write( m_AutoLootNightshade );
			writer.Write( m_AutoLootSulfurousAsh );
			writer.Write( m_AutoLootSpidersSilk );
			writer.Write( m_AutoLootBrimstone );
			writer.Write( m_AutoLootButterflyWings );
			writer.Write( m_AutoLootEyeOfToad );
			writer.Write( m_AutoLootFairyEgg );
			writer.Write( m_AutoLootGargoyleEar );
			writer.Write( m_AutoLootBeetleShell );
			writer.Write( m_AutoLootMoonCrystal );
			writer.Write( m_AutoLootPixieSkull );
			writer.Write( m_AutoLootRedLotus );
			writer.Write( m_AutoLootSeaSalt );
			writer.Write( m_AutoLootSilverWidow );
			writer.Write( m_AutoLootSwampBerries );
			writer.Write( m_AutoLootBatWing );
			writer.Write( m_AutoLootGraveDust );
			writer.Write( m_AutoLootDaemonBlood );
			writer.Write( m_AutoLootNoxCrystal );
			writer.Write( m_AutoLootPigIron );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if ( version >= 1 )
			{
				m_AutoLootBlackPearl = reader.ReadBool();
				m_AutoLootBloodmoss = reader.ReadBool();
				m_AutoLootGarlic = reader.ReadBool();
				m_AutoLootGinseng = reader.ReadBool();
				m_AutoLootMandrakeRoot = reader.ReadBool();
				m_AutoLootNightshade = reader.ReadBool();
				m_AutoLootSulfurousAsh = reader.ReadBool();
				m_AutoLootSpidersSilk = reader.ReadBool();
				m_AutoLootBrimstone = reader.ReadBool();
				m_AutoLootButterflyWings = reader.ReadBool();
				m_AutoLootEyeOfToad = reader.ReadBool();
				m_AutoLootFairyEgg = reader.ReadBool();
				m_AutoLootGargoyleEar = reader.ReadBool();
				m_AutoLootBeetleShell = reader.ReadBool();
				m_AutoLootMoonCrystal = reader.ReadBool();
				m_AutoLootPixieSkull = reader.ReadBool();
				m_AutoLootRedLotus = reader.ReadBool();
				m_AutoLootSeaSalt = reader.ReadBool();
				m_AutoLootSilverWidow = reader.ReadBool();
				m_AutoLootSwampBerries = reader.ReadBool();
				m_AutoLootBatWing = reader.ReadBool();
				m_AutoLootGraveDust = reader.ReadBool();
				m_AutoLootDaemonBlood = reader.ReadBool();
				m_AutoLootNoxCrystal = reader.ReadBool();
				m_AutoLootPigIron = reader.ReadBool();
			}
		}
                public override int GetTotal(TotalType type)
        {
            if (type != TotalType.Weight)
                return base.GetTotal(type);
            else
            {
                return (int)(TotalItemWeights() * (0.05));
            }
        } 
		      public override void UpdateTotal(Item sender, TotalType type, int delta)
        {
            if (type != TotalType.Weight)
                base.UpdateTotal(sender, type, delta);
            else
                base.UpdateTotal(sender, type, (int)(delta * (0.05)));
        }
		private double TotalItemWeights()
        {
            double weight = 0.0;

            foreach (Item item in Items)
                weight += (item.Weight * (double)(item.Amount));

            return weight;
        }

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list )
		{
			base.GetContextMenuEntries( from, list );

			if ( from != null && from.Alive )
				list.Add( new ConfigureAutoLootEntry( this ) );
		}

		public bool CanAutoLoot( BaseReagent reagent )
		{
			if ( reagent == null )
				return false;

			foreach ( ReagentAutoLootOption option in AutoLootOptions )
			{
				if ( option.Matches( reagent ) )
					return option.GetValue( this );
			}

			return false;
		}

		public void TryAutoLoot( Mobile from, BaseReagent reagent )
		{
			if ( from == null || reagent == null || reagent.Deleted )
				return;

			if ( reagent.Parent == this || reagent.IsChildOf( this ) )
				return;

			if ( !CanAutoLoot( reagent ) )
				return;

			if ( !IsAccessibleTo( from ) )
				return;

			OnDragDrop( from, reagent );
		}

		public void AutoLootFromContainer( Mobile from, Container source )
		{
			if ( from == null || source == null || source.Deleted )
				return;

			if ( !IsAccessibleTo( from ) )
				return;

			List<Item> contents = source.Items;

			if ( contents == null || contents.Count == 0 )
				return;

			List<BaseReagent> move = null;

			for ( int i = contents.Count - 1; i >= 0; --i )
			{
				BaseReagent reagent = contents[ i ] as BaseReagent;

				if ( reagent == null || reagent.Deleted )
					continue;

				if ( CanAutoLoot( reagent ) )
				{
					if ( move == null )
						move = new List<BaseReagent>();

					move.Add( reagent );
				}
			}

			if ( move == null || move.Count == 0 )
				return;

			for ( int i = 0; i < move.Count; i++ )
			{
				BaseReagent reagent = move[ i ];

				if ( reagent == null || reagent.Deleted )
					continue;

				TryAutoLoot( from, reagent );
			}
		}

		private bool IsAccessibleTo( Mobile from )
		{
			if ( from == null )
				return false;

			if ( Parent == from && Layer == Layer.Waist )
				return true;

			return from.Backpack != null && IsChildOf( from.Backpack );
		}

		public static ReagentBelt FindReagentBelt( Mobile from )
		{
			if ( from == null )
				return null;

			ReagentBelt belt = from.FindItemOnLayer( Layer.Waist ) as ReagentBelt;

			if ( belt != null )
				return belt;

			return from.Backpack != null ? from.Backpack.FindItemByType( typeof( ReagentBelt ) ) as ReagentBelt : null;
		}

		private bool CanConfigure( Mobile from )
		{
			return from != null && from.Alive && IsAccessibleTo( from );
		}

		private const int StatusEntryId = 3000132;

		private class ConfigureAutoLootEntry : ContextMenuEntry
		{
			private readonly ReagentBelt m_Belt;

			public ConfigureAutoLootEntry( ReagentBelt belt ) : base( StatusEntryId )
			{
				m_Belt = belt;
			}

			public override void OnClick()
			{
				if ( Owner == null || m_Belt == null || m_Belt.Deleted )
					return;

				Mobile from = Owner.From;

				if ( !m_Belt.CanConfigure( from ) )
					return;

				from.CloseGump( typeof( ReagentBeltOptionsGump ) );
				from.SendGump( new ReagentBeltOptionsGump( from, m_Belt ) );
			}
		}

		private class ReagentBeltOptionsGump : Gump
		{
			private const int ApplyButtonId = 1;
			private const int NextPageButtonId = 2;
			private const int PrevPageButtonId = 3;

			private readonly ReagentBelt m_Belt;

			public ReagentBeltOptionsGump( Mobile from, ReagentBelt belt ) : base( 150, 50 )
			{
				m_Belt = belt;

				for ( int pageIndex = 0; pageIndex < AutoLootGroups.Length; pageIndex++ )
				{
					ReagentAutoLootGroup group = AutoLootGroups[ pageIndex ];
					int pageNumber = pageIndex + 1;

					AddPage( pageNumber );
					AddBackground( 0, 0, 420, 430, 0x13BE );
					AddAlphaRegion( 10, 10, 400, 410 );

					AddLabel( 20, 20, 0x480, "Reagent Belt Auto Loot" );
					AddLabel( 20, 45, 0x34, group.Label );

					int startX = 20;
					int startY = 75;
					int rowHeight = 25;

					for ( int i = 0; i < group.Options.Length; i++ )
					{
						ReagentAutoLootOption option = group.Options[ i ];
						int x = startX;
						int y = startY + ( i * rowHeight );
						int switchId = option.SwitchId;

						AddCheck( x, y, 0x2342, 0x2343, option.GetValue( belt ), switchId );
						AddLabel( x + 25, y, 0x34, option.Label );
					}

					AddButton( 300, 385, 0xF7, 0xF8, ApplyButtonId, GumpButtonType.Reply, 0 );
					AddLabel( 320, 385, 0x34, "Apply" );

					AddLabel( 20, 385, 0x34, String.Format( "Page {0} of {1}", pageNumber, AutoLootGroups.Length ) );

					if ( pageNumber > 1 )
						AddButton( 230, 385, 0x15E3, 0x15E7, PrevPageButtonId, GumpButtonType.Page, pageNumber - 1 );

					if ( pageNumber < AutoLootGroups.Length )
						AddButton( 260, 385, 0x15E1, 0x15E5, NextPageButtonId, GumpButtonType.Page, pageNumber + 1 );
				}
			}

			public override void OnResponse( NetState sender, RelayInfo info )
			{
				Mobile from = sender.Mobile;

				if ( from == null || m_Belt == null || m_Belt.Deleted )
					return;

				if ( !m_Belt.CanConfigure( from ) )
					return;

				if ( info.ButtonID != ApplyButtonId )
					return;

				for ( int i = 0; i < AutoLootOptions.Length; i++ )
				{
					ReagentAutoLootOption option = AutoLootOptions[ i ];
					option.SetValue( m_Belt, info.IsSwitched( option.SwitchId ) );
				}

				from.SendMessage( 0x5A, "Your reagent belt auto-loot settings have been updated." );
				from.CloseGump( typeof( ReagentBeltOptionsGump ) );
				from.SendGump( new ReagentBeltOptionsGump( from, m_Belt ) );
			}
		}

		private class ReagentAutoLootOption
		{
			public ReagentAutoLootOption( int switchId, Type type, string label, Func<ReagentBelt, bool> getter, Action<ReagentBelt, bool> setter )
			{
				SwitchId = switchId;
				Type = type;
				Label = label;
				GetValue = getter;
				SetValue = setter;
			}

			public int SwitchId { get; private set; }
			public Type Type { get; private set; }
			public string Label { get; private set; }
			public Func<ReagentBelt, bool> GetValue { get; private set; }
			public Action<ReagentBelt, bool> SetValue { get; private set; }

			public bool Matches( BaseReagent reagent )
			{
				return Type.IsInstanceOfType( reagent );
			}
		}

		private class ReagentAutoLootGroup
		{
			public ReagentAutoLootGroup( string label, ReagentAutoLootOption[] options )
			{
				Label = label;
				Options = options;
			}

			public string Label { get; private set; }
			public ReagentAutoLootOption[] Options { get; private set; }
		}

		private static readonly ReagentAutoLootOption[] AutoLootOptions =
		{
			new ReagentAutoLootOption( 1, typeof( BlackPearl ), "Black Pearl", belt => belt.m_AutoLootBlackPearl, ( belt, value ) => belt.m_AutoLootBlackPearl = value ),
			new ReagentAutoLootOption( 2, typeof( Bloodmoss ), "Bloodmoss", belt => belt.m_AutoLootBloodmoss, ( belt, value ) => belt.m_AutoLootBloodmoss = value ),
			new ReagentAutoLootOption( 3, typeof( Garlic ), "Garlic", belt => belt.m_AutoLootGarlic, ( belt, value ) => belt.m_AutoLootGarlic = value ),
			new ReagentAutoLootOption( 4, typeof( Ginseng ), "Ginseng", belt => belt.m_AutoLootGinseng, ( belt, value ) => belt.m_AutoLootGinseng = value ),
			new ReagentAutoLootOption( 5, typeof( MandrakeRoot ), "Mandrake Root", belt => belt.m_AutoLootMandrakeRoot, ( belt, value ) => belt.m_AutoLootMandrakeRoot = value ),
			new ReagentAutoLootOption( 6, typeof( Nightshade ), "Nightshade", belt => belt.m_AutoLootNightshade, ( belt, value ) => belt.m_AutoLootNightshade = value ),
			new ReagentAutoLootOption( 7, typeof( SulfurousAsh ), "Sulfurous Ash", belt => belt.m_AutoLootSulfurousAsh, ( belt, value ) => belt.m_AutoLootSulfurousAsh = value ),
			new ReagentAutoLootOption( 8, typeof( SpidersSilk ), "Spider's Silk", belt => belt.m_AutoLootSpidersSilk, ( belt, value ) => belt.m_AutoLootSpidersSilk = value ),
			new ReagentAutoLootOption( 9, typeof( BatWing ), "Bat Wing", belt => belt.m_AutoLootBatWing, ( belt, value ) => belt.m_AutoLootBatWing = value ),
			new ReagentAutoLootOption( 10, typeof( GraveDust ), "Grave Dust", belt => belt.m_AutoLootGraveDust, ( belt, value ) => belt.m_AutoLootGraveDust = value ),
			new ReagentAutoLootOption( 11, typeof( DaemonBlood ), "Daemon Blood", belt => belt.m_AutoLootDaemonBlood, ( belt, value ) => belt.m_AutoLootDaemonBlood = value ),
			new ReagentAutoLootOption( 12, typeof( NoxCrystal ), "Nox Crystal", belt => belt.m_AutoLootNoxCrystal, ( belt, value ) => belt.m_AutoLootNoxCrystal = value ),
			new ReagentAutoLootOption( 13, typeof( PigIron ), "Pig Iron", belt => belt.m_AutoLootPigIron, ( belt, value ) => belt.m_AutoLootPigIron = value ),
			new ReagentAutoLootOption( 14, typeof( Brimstone ), "Brimstone", belt => belt.m_AutoLootBrimstone, ( belt, value ) => belt.m_AutoLootBrimstone = value ),
			new ReagentAutoLootOption( 15, typeof( ButterflyWings ), "Butterfly Wings", belt => belt.m_AutoLootButterflyWings, ( belt, value ) => belt.m_AutoLootButterflyWings = value ),
			new ReagentAutoLootOption( 16, typeof( EyeOfToad ), "Eye of Toad", belt => belt.m_AutoLootEyeOfToad, ( belt, value ) => belt.m_AutoLootEyeOfToad = value ),
			new ReagentAutoLootOption( 17, typeof( FairyEgg ), "Fairy Egg", belt => belt.m_AutoLootFairyEgg, ( belt, value ) => belt.m_AutoLootFairyEgg = value ),
			new ReagentAutoLootOption( 18, typeof( GargoyleEar ), "Gargoyle Ear", belt => belt.m_AutoLootGargoyleEar, ( belt, value ) => belt.m_AutoLootGargoyleEar = value ),
			new ReagentAutoLootOption( 19, typeof( BeetleShell ), "Beetle Shell", belt => belt.m_AutoLootBeetleShell, ( belt, value ) => belt.m_AutoLootBeetleShell = value ),
			new ReagentAutoLootOption( 20, typeof( MoonCrystal ), "Moon Crystal", belt => belt.m_AutoLootMoonCrystal, ( belt, value ) => belt.m_AutoLootMoonCrystal = value ),
			new ReagentAutoLootOption( 21, typeof( PixieSkull ), "Pixie Skull", belt => belt.m_AutoLootPixieSkull, ( belt, value ) => belt.m_AutoLootPixieSkull = value ),
			new ReagentAutoLootOption( 22, typeof( RedLotus ), "Red Lotus", belt => belt.m_AutoLootRedLotus, ( belt, value ) => belt.m_AutoLootRedLotus = value ),
			new ReagentAutoLootOption( 23, typeof( SeaSalt ), "Sea Salt", belt => belt.m_AutoLootSeaSalt, ( belt, value ) => belt.m_AutoLootSeaSalt = value ),
			new ReagentAutoLootOption( 24, typeof( SilverWidow ), "Silver Widow", belt => belt.m_AutoLootSilverWidow, ( belt, value ) => belt.m_AutoLootSilverWidow = value ),
			new ReagentAutoLootOption( 25, typeof( SwampBerries ), "Swamp Berries", belt => belt.m_AutoLootSwampBerries, ( belt, value ) => belt.m_AutoLootSwampBerries = value )
		};

		private static readonly ReagentAutoLootGroup[] AutoLootGroups =
		{
			new ReagentAutoLootGroup(
				"Magery Reagents",
				new[]
				{
					AutoLootOptions[ 0 ],
					AutoLootOptions[ 1 ],
					AutoLootOptions[ 2 ],
					AutoLootOptions[ 3 ],
					AutoLootOptions[ 4 ],
					AutoLootOptions[ 5 ],
					AutoLootOptions[ 6 ],
					AutoLootOptions[ 7 ]
				} ),
			new ReagentAutoLootGroup(
				"Necromancy Reagents",
				new[]
				{
					AutoLootOptions[ 8 ],
					AutoLootOptions[ 9 ],
					AutoLootOptions[ 10 ],
					AutoLootOptions[ 11 ],
					AutoLootOptions[ 12 ]
				} ),
			new ReagentAutoLootGroup(
				"Alchemic Reagents",
				new[]
				{
					AutoLootOptions[ 13 ],
					AutoLootOptions[ 14 ],
					AutoLootOptions[ 15 ],
					AutoLootOptions[ 16 ],
					AutoLootOptions[ 17 ],
					AutoLootOptions[ 18 ],
					AutoLootOptions[ 19 ],
					AutoLootOptions[ 20 ],
					AutoLootOptions[ 21 ],
					AutoLootOptions[ 22 ],
					AutoLootOptions[ 23 ],
					AutoLootOptions[ 24 ]
				} )
		};
	}
}
