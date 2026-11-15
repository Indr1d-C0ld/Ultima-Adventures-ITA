using System;
using System.Collections.Generic;
using Server;
using Server.Network;
using Server.Items;
using Server.Engines.Craft;
using Server.Mobiles;
using Server.Targeting;
using Server.Gumps;

namespace Server.Items
{
	[FlipableAttribute( 0xEC4, 0xEC5 )]
	public class SkinningKnife : BaseKnife, ITinkerRepairable
	{
		public override WeaponAbility PrimaryAbility{ get{ return WeaponAbility.ShadowStrike; } }
		public override WeaponAbility SecondaryAbility{ get{ return WeaponAbility.Disarm; } }
		public override WeaponAbility ThirdAbility{ get{ return WeaponAbility.Disrobe; } }
		public override WeaponAbility FourthAbility{ get{ return WeaponAbility.ZapStrStrike; } }
		public override WeaponAbility FifthAbility{ get{ return WeaponAbility.ForceOfNature; } }

		public override int AosStrengthReq{ get{ return 5; } }
		public override int AosMinDamage{ get{ return 9; } }
		public override int AosMaxDamage{ get{ return 11; } }
		public override int AosSpeed{ get{ return 49; } }
		public override float MlSpeed{ get{ return 2.25f; } }

		public override int OldStrengthReq{ get{ return 5; } }
		public override int OldMinDamage{ get{ return 1; } }
		public override int OldMaxDamage{ get{ return 10; } }
		public override int OldSpeed{ get{ return 40; } }

		public override int InitMinHits{ get{ return 31; } }
		public override int InitMaxHits{ get{ return 40; } }

		[Constructable]
		public SkinningKnife() : base( 0xEC4 )
		{
			Weight = 1.0;
		}

		public SkinningKnife( Serial serial ) : base( serial )
		{
		}

                private const int AutoSkinRange = 2;
                private static readonly TimeSpan AutoSkinRetryDelay = TimeSpan.FromSeconds( 5.0 );

                private static readonly Dictionary<Mobile, AutoSkinTimer> m_AutoSkinTimers = new Dictionary<Mobile, AutoSkinTimer>();

                private bool m_AutoSkinEnabled = true;
                private bool m_PickupMeat;
                private bool m_PickupFur;
                private bool m_PickupLeather;
                private bool m_PickupScales;
                private bool m_PickupFeathers;

                public virtual bool SupportsSkinningFeatures
                {
                        get { return GetType() == typeof( SkinningKnife ); }
                }

                [CommandProperty( AccessLevel.GameMaster )]
                public bool AutoSkinEnabled
                {
                        get
                        {
                                return m_AutoSkinEnabled;
                        }
                        set
                        {
                                if ( m_AutoSkinEnabled != value )
                                {
                                        m_AutoSkinEnabled = value;

                                        Mobile from = Parent as Mobile;

                                        if ( from != null && SupportsSkinningFeatures )
                                        {
                                                if ( m_AutoSkinEnabled )
                                                        StartAutoSkin( from );
                                                else
                                                        StopAutoSkin( from );
                                        }

                                        InvalidateProperties();
                                }
                        }
                }

                [CommandProperty( AccessLevel.GameMaster )]
                public bool AutoPickupMeat
                {
                        get
                        {
                                return m_PickupMeat;
                        }
                        set
                        {
                                if ( m_PickupMeat != value )
                                {
                                        m_PickupMeat = value;
                                        InvalidateProperties();
                                }
                        }
                }

                [CommandProperty( AccessLevel.GameMaster )]
                public bool AutoPickupFur
                {
                        get
                        {
                                return m_PickupFur;
                        }
                        set
                        {
                                if ( m_PickupFur != value )
                                {
                                        m_PickupFur = value;
                                        InvalidateProperties();
                                }
                        }
                }

                [CommandProperty( AccessLevel.GameMaster )]
                public bool AutoPickupLeather
                {
                        get
                        {
                                return m_PickupLeather;
                        }
                        set
                        {
                                if ( m_PickupLeather != value )
                                {
                                        m_PickupLeather = value;
                                        InvalidateProperties();
                                }
                        }
                }

                [CommandProperty( AccessLevel.GameMaster )]
                public bool AutoPickupScales
                {
                        get
                        {
                                return m_PickupScales;
                        }
                        set
                        {
                                if ( m_PickupScales != value )
                                {
                                        m_PickupScales = value;
                                        InvalidateProperties();
                                }
                        }
                }

                [CommandProperty( AccessLevel.GameMaster )]
                public bool AutoPickupFeathers
                {
                        get
                        {
                                return m_PickupFeathers;
                        }
                        set
                        {
                                if ( m_PickupFeathers != value )
                                {
                                        m_PickupFeathers = value;
                                        InvalidateProperties();
                                }
                        }
                }

                public override void AddNameProperties( ObjectPropertyList list )
                {
                        base.AddNameProperties( list );

                        if ( !SupportsSkinningFeatures )
                                return;

                        list.Add( m_AutoSkinEnabled ? "Automatically carves nearby corpses while equipped." : "Manual corpse carving while equipped." );
                        list.Add( "Use the knife on itself to set up skinning functions." );
                }

                public override bool OnEquip( Mobile from )
                {
                        if ( base.OnEquip( from ) )
                        {
                                if ( SupportsSkinningFeatures && m_AutoSkinEnabled )
                                        StartAutoSkin( from );
                                return true;
                        }

                        return false;
                }

		public override void OnAdded( IEntity parent )
		{
			base.OnAdded( parent );

			Mobile from = parent as Mobile;

                        if ( from != null && Parent == from && SupportsSkinningFeatures && m_AutoSkinEnabled )
                                StartAutoSkin( from );
                }

                public override void OnSingleClick( Mobile from )
                {
                        base.OnSingleClick( from );

                        if ( SupportsSkinningFeatures )
                                LabelAutoSkinState( from );
                }

                internal void LabelAutoSkinState( Mobile from )
                {
                        if ( from == null || !SupportsSkinningFeatures )
                                return;

                        LabelTo( from, AutoSkinEnabled ? "Automatic corpse skinning: On" : "Automatic corpse skinning: Off" );
                        LabelTo( from, String.Format( "Pickup meat: {0}, fur: {1}, leather: {2}, scales: {3}, feathers: {4}",
                                m_PickupMeat ? "On" : "Off",
                                m_PickupFur ? "On" : "Off",
                                m_PickupLeather ? "On" : "Off",
                                m_PickupScales ? "On" : "Off",
                                m_PickupFeathers ? "On" : "Off" ) );
                }

                internal void HandleSelfUse( Mobile from )
                {
                        if ( from == null || !SupportsSkinningFeatures )
                                return;

                        if ( !from.Player )
                        {
                                LabelAutoSkinState( from );
                                return;
                        }

                        if ( !CanConfigure( from ) )
                        {
                                LabelAutoSkinState( from );
                                return;
                        }

                        from.CloseGump( typeof( SkinningKnifeOptionsGump ) );
                        from.SendGump( new SkinningKnifeOptionsGump( from, this ) );
                }

                private bool CanConfigure( Mobile from )
                {
                        if ( from == null || Deleted || !SupportsSkinningFeatures )
                                return false;

                        Mobile owner = RootParent as Mobile;

                        if ( owner != from )
                                return false;

                        if ( Parent == from )
                                return true;

                        return ( from.Backpack != null && IsChildOf( from.Backpack ) );
                }

                public override void OnRemoved( IEntity parent )
                {
			Mobile from = parent as Mobile;

			if ( from != null && SupportsSkinningFeatures )
				StopAutoSkin( from );

			base.OnRemoved( parent );
		}

		public static SkinningKnife GetEquippedKnife( Mobile from )
		{
			if ( from == null )
				return null;

			SkinningKnife knife = from.Weapon as SkinningKnife;

			if ( knife != null && knife.SupportsSkinningFeatures && knife.Parent == from )
				return knife;

			Item oneHand = from.FindItemOnLayer( Layer.OneHanded );
			knife = oneHand as SkinningKnife;

			if ( knife != null && knife.SupportsSkinningFeatures && knife.Parent == from )
				return knife;

			Item twoHand = from.FindItemOnLayer( Layer.TwoHanded );
			knife = twoHand as SkinningKnife;

			if ( knife != null && knife.SupportsSkinningFeatures && knife.Parent == from )
				return knife;

                        return null;
                }

                private Container GetAutoLootContainer( Mobile from )
                {
                        PlayerMobile player = from as PlayerMobile;

                        if ( player == null || player.Backpack == null )
                                return null;

                        Serial serial = CharacterDatabase.GetSkinningAutolootContainer( player );

                        if ( serial == Serial.Zero )
                                return null;

                        return player.FindItemRecursive( player.Backpack, i => i.Serial == serial, true ) as Container;
                }

                private string GetAutoLootContainerLabel( Mobile from )
                {
                        Container container = GetAutoLootContainer( from );

                        if ( container == null )
                                return "Auto loot container: Backpack";

                        string name = container.Name;

                        if ( String.IsNullOrEmpty( name ) )
                                name = container.ItemData.Name;

                        return String.Format( "Auto loot container: {0}", name );
                }

		private static void StartAutoSkin( Mobile from )
		{
			if ( from == null )
				return;

			if ( !from.Player && from.AccessLevel <= AccessLevel.Player )
				return;

			StopAutoSkin( from );

                        SkinningKnife knife = GetEquippedKnife( from );

                        if ( knife == null || knife.Deleted || !knife.AutoSkinEnabled )
                                return;

                        AutoSkinTimer timer = new AutoSkinTimer( from );
			m_AutoSkinTimers[from] = timer;
			timer.Start();
		}

		private static void StopAutoSkin( Mobile from )
		{
			AutoSkinTimer timer;

			if ( from != null && m_AutoSkinTimers.TryGetValue( from, out timer ) )
			{
				timer.Stop();
				m_AutoSkinTimers.Remove( from );
			}
		}

                private static void HandleAutoSkin( Mobile from, Dictionary<Corpse, DateTime> skipUntil )
                {
                        if ( from == null || from.Deleted || !from.Alive )
                        {
                                StopAutoSkin( from );
                                return;
			}

                        SkinningKnife knife = GetEquippedKnife( from );

                        if ( knife == null || knife.Deleted || !knife.AutoSkinEnabled )
                        {
                                StopAutoSkin( from );
                                return;
                        }

                        Map map = from.Map;

                        if ( map == null || map == Map.Internal )
                                return;

                        DateTime now = DateTime.UtcNow;

                        if ( skipUntil != null && skipUntil.Count > 0 )
                        {
                                List<Corpse> expired = null;

                                foreach ( KeyValuePair<Corpse, DateTime> entry in skipUntil )
                                {
                                        Corpse tracked = entry.Key;

                                        if ( tracked == null || tracked.Deleted || entry.Value <= now )
                                        {
                                                if ( expired == null )
                                                        expired = new List<Corpse>();

                                                expired.Add( tracked );
                                        }
                                }

                                if ( expired != null )
                                {
                                        for ( int i = 0; i < expired.Count; i++ )
                                                skipUntil.Remove( expired[i] );
                                }
                        }

                        IPooledEnumerable eable = from.GetItemsInRange( AutoSkinRange );

                        try
                        {
                                foreach ( Item item in eable )
				{
					Corpse corpse = item as Corpse;

					if ( corpse == null || corpse.Deleted )
						continue;

                                        if ( corpse.Carved )
                                                continue;

                                        if ( corpse.ItemID != 0x2006 )
                                                continue;

                                        Mobile owner = corpse.Owner;

                                        if ( owner == null || !(owner is BaseCreature) )
                                                continue;

                                        if ( skipUntil != null )
                                        {
                                                DateTime until;

                                                if ( skipUntil.TryGetValue( corpse, out until ) )
                                                {
                                                        if ( until > now )
                                                                continue;

                                                        skipUntil.Remove( corpse );
                                                }
                                        }

                                        if ( !corpse.CanLoot( from, null ) )
                                        {
                                                ScheduleAutoSkinRetry( skipUntil, corpse, now );
                                                continue;
                                        }

                                        bool carvedBefore = corpse.Carved;

                                        corpse.Carve( from, knife, true );

                                        if ( !corpse.Deleted )
                                                knife.HandleAutoPickup( from, corpse );

                                        if ( corpse.Deleted || corpse.Carved || corpse.ItemID != 0x2006 )
                                        {
                                                if ( skipUntil != null )
                                                        skipUntil.Remove( corpse );
                                        }
                                        else if ( !carvedBefore )
                                        {
                                                ScheduleAutoSkinRetry( skipUntil, corpse, now );
                                        }
                                }
                        }
                        finally
                        {
                                eable.Free();
			}
		}

		private class AutoSkinTimer : Timer
		{
                        private readonly Mobile m_From;
                        private readonly Dictionary<Corpse, DateTime> m_SkipUntil;

                        public AutoSkinTimer( Mobile from ) : base( TimeSpan.FromSeconds( 0.5 ), TimeSpan.FromSeconds( 0.5 ) )
                        {
                                m_From = from;
                                m_SkipUntil = new Dictionary<Corpse, DateTime>();
                                Priority = TimerPriority.TwoFiftyMS;
                        }

                        protected override void OnTick()
                        {
                                HandleAutoSkin( m_From, m_SkipUntil );
                        }
                }

                private void HandleAutoPickup( Mobile from, Corpse corpse )
                {
                        if ( from == null || corpse == null || from.Backpack == null )
                                return;

                        if ( !m_PickupMeat && !m_PickupFur && !m_PickupLeather && !m_PickupScales && !m_PickupFeathers )
                                return;

                        List<Item> contents = corpse.Items;

                        if ( contents == null || contents.Count == 0 )
                                return;

                        List<Item> move = null;

                        for ( int i = contents.Count - 1; i >= 0; --i )
                        {
                                Item item = contents[i];

                                if ( item == null || item.Deleted )
                                        continue;

                                if ( ShouldAutoPickup( item ) )
                                {
                                        if ( move == null )
                                                move = new List<Item>();

                                        move.Add( item );
                                }
                        }

                        if ( move == null || move.Count == 0 )
                                return;

                        Container autolootContainer = GetAutoLootContainer( from );

                        for ( int i = 0; i < move.Count; ++i )
                        {
                                Item item = move[i];

                                if ( item == null || item.Deleted )
                                        continue;

                                if ( autolootContainer != null )
                                        MoveToAutoLootContainer( from, autolootContainer, item );
                                else
                                        from.PlaceInBackpack( item );
                        }

                        if ( autolootContainer != null )
                                MoveBackpackLootToAutoLootContainer( from, autolootContainer );
                }

                private bool ShouldAutoPickup( Item item )
                {
                        if ( item == null )
                                return false;

                        if ( m_PickupMeat && IsMeat( item ) )
                                return true;

                        if ( m_PickupFur && IsFur( item ) )
                                return true;

                        if ( m_PickupLeather && IsLeather( item ) )
                                return true;

                        if ( m_PickupScales && IsScales( item ) )
                                return true;

                        if ( m_PickupFeathers && IsFeathers( item ) )
                                return true;

                        return false;
                }

                private static bool IsMeat( Item item )
                {
                        return item is RawRibs || item is RawBird || item is RawLambLeg || item is RawFishSteak || item is RawChickenLeg;
                }

                private static bool IsFur( Item item )
                {
                        return item is Furs || item is FursWhite;
                }

                private static bool IsLeather( Item item )
                {
                        return item is BaseHides || item is BaseLeather;
                }

                private static bool IsScales( Item item )
                {
                        return item is BaseScales;
                }

                private static bool IsFeathers( Item item )
                {
                        return item is Feather;
                }

                private void MoveBackpackLootToAutoLootContainer( Mobile from, Container autolootContainer )
                {
                        if ( from == null || autolootContainer == null || from.Backpack == null )
                                return;

                        List<Item> backpackItems = from.Backpack.Items;

                        if ( backpackItems == null || backpackItems.Count == 0 )
                                return;

                        for ( int i = backpackItems.Count - 1; i >= 0; --i )
                        {
                                Item item = backpackItems[i];

                                if ( item == null || item.Deleted || item == autolootContainer || item.IsChildOf( autolootContainer ) )
                                        continue;

                                if ( ShouldAutoPickup( item ) )
                                        MoveToAutoLootContainer( from, autolootContainer, item );
                        }
                }

                private void MoveToAutoLootContainer( Mobile from, Container autolootContainer, Item item )
                {
                        if ( from == null || autolootContainer == null || item == null || item.Deleted )
                                return;

                        if ( item.Parent == autolootContainer || item.IsChildOf( autolootContainer ) )
                                return;

                        autolootContainer.OnDragDrop( from, item );
                }

                private static void ScheduleAutoSkinRetry( Dictionary<Corpse, DateTime> skipUntil, Corpse corpse, DateTime now )
                {
                        if ( skipUntil == null || corpse == null )
                                return;

                        skipUntil[corpse] = now + AutoSkinRetryDelay;
                }

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );

                        writer.Write( (int) 4 ); // version
                        writer.Write( m_AutoSkinEnabled );
                        writer.Write( m_PickupMeat );
                        writer.Write( m_PickupFur );
                        writer.Write( m_PickupLeather );
                        writer.Write( m_PickupScales );
                        writer.Write( m_PickupFeathers );
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );

                        int version = reader.ReadInt();

                        switch ( version )
                        {
                                case 4:
                                        m_AutoSkinEnabled = reader.ReadBool();
                                        m_PickupMeat = reader.ReadBool();
                                        m_PickupFur = reader.ReadBool();
                                        m_PickupLeather = reader.ReadBool();
                                        m_PickupScales = reader.ReadBool();
                                        m_PickupFeathers = reader.ReadBool();
                                        break;
                                case 3:
                                        m_AutoSkinEnabled = reader.ReadBool();
                                        m_PickupMeat = reader.ReadBool();
                                        m_PickupFur = reader.ReadBool();
                                        m_PickupScales = reader.ReadBool();
                                        m_PickupFeathers = reader.ReadBool();
                                        break;
                                case 2:
                                        m_AutoSkinEnabled = reader.ReadBool();
                                        m_PickupMeat = reader.ReadBool();
                                        m_PickupFur = reader.ReadBool();
                                        m_PickupScales = reader.ReadBool();
                                        break;
                                case 1:
                                        m_AutoSkinEnabled = reader.ReadBool();
                                        break;
                                case 0:
                                        m_AutoSkinEnabled = true;
                                        break;
                        }

                        Mobile from = Parent as Mobile;

                        if ( from != null && SupportsSkinningFeatures && m_AutoSkinEnabled )
                                StartAutoSkin( from );
                }

                private class SkinningKnifeOptionsGump : Gump
                {
                        private const int ToggleAutoSkinButtonID = 1;
                        private const int ToggleMeatButtonID = 2;
                        private const int ToggleFurButtonID = 3;
                        private const int ToggleLeatherButtonID = 4;
                        private const int ToggleScalesButtonID = 5;
                        private const int ToggleFeathersButtonID = 6;
                        private const int SetContainerButtonID = 7;

                        private readonly SkinningKnife m_Knife;

                        public SkinningKnifeOptionsGump( Mobile from, SkinningKnife knife ) : base( 150, 50 )
                        {
                                m_Knife = knife;

                                AddPage( 0 );

                                AddBackground( 0, 0, 320, 345, 0x13BE );
                                AddAlphaRegion( 10, 10, 300, 325 );

                                AddLabel( 20, 25, 0x480, "Skinning Knife Options" );

                                AddButton( 20, 60, 0x15E1, 0x15E5, ToggleAutoSkinButtonID, GumpButtonType.Reply, 0 );
                                AddLabel( 55, 60, 0x34, String.Format( "Auto-skinning: {0}", knife.AutoSkinEnabled ? "On" : "Off" ) );

                                AddButton( 20, 100, 0x15E1, 0x15E5, ToggleMeatButtonID, GumpButtonType.Reply, 0 );
                                AddLabel( 55, 100, 0x34, String.Format( "Pickup meat: {0}", knife.AutoPickupMeat ? "Yes" : "No" ) );

                                AddButton( 20, 135, 0x15E1, 0x15E5, ToggleFurButtonID, GumpButtonType.Reply, 0 );
                                AddLabel( 55, 135, 0x34, String.Format( "Pickup fur: {0}", knife.AutoPickupFur ? "Yes" : "No" ) );

                                AddButton( 20, 170, 0x15E1, 0x15E5, ToggleLeatherButtonID, GumpButtonType.Reply, 0 );
                                AddLabel( 55, 170, 0x34, String.Format( "Pickup leather: {0}", knife.AutoPickupLeather ? "Yes" : "No" ) );

                                AddButton( 20, 205, 0x15E1, 0x15E5, ToggleScalesButtonID, GumpButtonType.Reply, 0 );
                                AddLabel( 55, 205, 0x34, String.Format( "Pickup scales: {0}", knife.AutoPickupScales ? "Yes" : "No" ) );

                                AddButton( 20, 240, 0x15E1, 0x15E5, ToggleFeathersButtonID, GumpButtonType.Reply, 0 );
                                AddLabel( 55, 240, 0x34, String.Format( "Pickup feathers: {0}", knife.AutoPickupFeathers ? "Yes" : "No" ) );

                                AddButton( 20, 275, 0x15E1, 0x15E5, SetContainerButtonID, GumpButtonType.Reply, 0 );
                                AddLabel( 55, 275, 0x34, knife.GetAutoLootContainerLabel( from ) );

                                AddButton( 250, 305, 0xF7, 0xF8, 0, GumpButtonType.Reply, 0 );
                                AddLabel( 270, 305, 0x34, "Close" );
                        }

                        public override void OnResponse( NetState sender, RelayInfo info )
                        {
                                Mobile from = sender.Mobile;

                                if ( from == null || m_Knife == null || m_Knife.Deleted )
                                        return;

                                if ( !m_Knife.CanConfigure( from ) )
                                        return;

                                switch ( info.ButtonID )
                                {
                                        case ToggleAutoSkinButtonID:
                                        {
                                                m_Knife.AutoSkinEnabled = !m_Knife.AutoSkinEnabled;
                                                from.SendMessage( m_Knife.AutoSkinEnabled ? 0x5A : 0x22, m_Knife.AutoSkinEnabled ? "Automatic corpse skinning enabled." : "Automatic corpse skinning disabled." );
                                                break;
                                        }
                                        case ToggleMeatButtonID:
                                        {
                                                m_Knife.AutoPickupMeat = !m_Knife.AutoPickupMeat;
                                                from.SendMessage( m_Knife.AutoPickupMeat ? 0x5A : 0x22, m_Knife.AutoPickupMeat ? "Automatically picking up carved meat." : "Leaving carved meat on the corpse." );
                                                break;
                                        }
                                        case ToggleFurButtonID:
                                        {
                                                m_Knife.AutoPickupFur = !m_Knife.AutoPickupFur;
                                                from.SendMessage( m_Knife.AutoPickupFur ? 0x5A : 0x22, m_Knife.AutoPickupFur ? "Automatically picking up carved fur." : "Leaving carved fur on the corpse." );
                                                break;
                                        }
                                        case ToggleLeatherButtonID:
                                        {
                                                m_Knife.AutoPickupLeather = !m_Knife.AutoPickupLeather;
                                                from.SendMessage( m_Knife.AutoPickupLeather ? 0x5A : 0x22, m_Knife.AutoPickupLeather ? "Automatically picking up carved leather." : "Leaving carved leather on the corpse." );
                                                break;
                                        }
                                        case ToggleScalesButtonID:
                                        {
                                                m_Knife.AutoPickupScales = !m_Knife.AutoPickupScales;
                                                from.SendMessage( m_Knife.AutoPickupScales ? 0x5A : 0x22, m_Knife.AutoPickupScales ? "Automatically picking up carved scales." : "Leaving carved scales on the corpse." );
                                                break;
                                        }
                                        case ToggleFeathersButtonID:
                                        {
                                                m_Knife.AutoPickupFeathers = !m_Knife.AutoPickupFeathers;
                                                from.SendMessage( m_Knife.AutoPickupFeathers ? 0x5A : 0x22, m_Knife.AutoPickupFeathers ? "Automatically picking up carved feathers." : "Leaving carved feathers on the corpse." );
                                                break;
                                        }
                                        case SetContainerButtonID:
                                        {
                                                from.Target = new AutoLootContainerTarget( m_Knife );
                                                from.SendMessage( "Target a container in your backpack to store auto-looted items." );
                                                break;
                                        }
                                }

                                if ( info.ButtonID != 0 )
                                {
                                        from.CloseGump( typeof( SkinningKnifeOptionsGump ) );
                                        from.SendGump( new SkinningKnifeOptionsGump( from, m_Knife ) );
                                }
                        }
                }

                private class AutoLootContainerTarget : Target
                {
                        private readonly SkinningKnife m_Knife;

                        public AutoLootContainerTarget( SkinningKnife knife ) : base( 12, false, TargetFlags.None )
                        {
                                m_Knife = knife;
                        }

                        protected override void OnTarget( Mobile from, object targeted )
                        {
                                if ( from == null || m_Knife == null || m_Knife.Deleted )
                                        return;

                                if ( !m_Knife.CanConfigure( from ) )
                                        return;

                                Container container = targeted as Container;

                                if ( container == null )
                                {
                                        from.SendMessage( 0x22, "You must target a container." );
                                        return;
                                }

                                if ( container.Deleted || from.Backpack == null || !container.IsChildOf( from.Backpack ) )
                                {
                                        from.SendMessage( 0x22, "The container must be in your backpack." );
                                        return;
                                }

                                CharacterDatabase.SetAutolootContainer( from, container.Serial );
                                from.SendMessage( 0x5A, "Auto loot container set to {0}.", String.IsNullOrEmpty( container.Name ) ? container.ItemData.Name : container.Name );
                        }
                }
        }
}
