using System;
using Server;
using Server.Engines.Craft;

namespace Server.Items
{
        [TypeAlias( "Server.Items.WaterTubAddon" )]
        public class StaticWaterTubAddon : BaseAddon, IWaterSource
        {
                public override BaseAddonDeed Deed{ get{ return new StaticWaterTubDeed(); } }

                public int Quantity
                {
                        get{ return 50; }
                        set{ }
                }

                [Constructable]
                public StaticWaterTubAddon()
                {
                        AddComponent( new AddonComponent( 0xE7B ), 0, 0, 0 );
                }

                public StaticWaterTubAddon( Serial serial ) : base( serial )
                {
                }

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );

                        writer.Write( (int) 0 ); // version
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );

                        int version = reader.ReadInt();
                }
        }

        [TypeAlias( "Server.Items.WaterTubDeed" )]
        public class StaticWaterTubDeed : BaseAddonDeed
        {
                public override BaseAddon Addon{ get{ return new StaticWaterTubAddon(); } }

                [Constructable]
                public StaticWaterTubDeed()
                {
                        Name = "water tub deed";
                        ItemID = 0x14F0;
                }

                public StaticWaterTubDeed( Serial serial ) : base( serial )
                {
                }

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );

                        writer.Write( (int) 0 ); // version
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );

                        int version = reader.ReadInt();
                }
        }

        [TypeAlias( "Server.Items.Tub" )]
        [Obsolete( "Use StaticWaterTubDeed" )]
        public class WaterTubContainer : BaseWaterContainer, ICraftable
        {
                public override int voidItem_ID{ get{ return 0xE83; } }
                public override int fullItem_ID{ get{ return 0xE7B; } }
                public override int MaxQuantity{ get{ return 50; } }

                public WaterTubContainer()
                        : this( false )
                {
                }

                public WaterTubContainer( bool filled )
                        : base( filled ? 0xE7B : 0xE83, filled )
                {
                }

                public WaterTubContainer( Serial serial ) : base( serial )
                {
                }

                public override void Serialize( GenericWriter writer )
                {
                        base.Serialize( writer );

                        writer.Write( (int) 0 ); // version
                }

                public override void Deserialize( GenericReader reader )
                {
                        base.Deserialize( reader );

                        int version = reader.ReadInt();

                        Timer.DelayCall( TimeSpan.Zero, new TimerCallback( ConvertToDeed ) );
                }

                private void ConvertToDeed()
                {
                        if ( Deleted )
                                return;

                        Item deed = new StaticWaterTubDeed();
                        object parent = Parent;

                        if ( parent is Mobile )
                        {
                                ( (Mobile) parent ).AddToBackpack( deed );
                        }
                        else if ( parent is Container )
                        {
                                ( (Container) parent ).DropItem( deed );
                        }
                        else if ( parent is Item )
                        {
                                Item item = (Item) parent;
                                deed.MoveToWorld( item.GetWorldLocation(), item.Map );
                        }
                        else
                        {
                                deed.MoveToWorld( Location, Map );
                        }

                        Delete();
                }

                public int OnCraft( int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue )
                {
                        Item deed = new StaticWaterTubDeed();

                        if ( from != null )
                        {
                                from.AddToBackpack( deed );
                        }
                        else if ( Parent is Container )
                        {
                                ( (Container) Parent ).DropItem( deed );
                        }
                        else if ( Parent is Item )
                        {
                                Item item = (Item) Parent;
                                deed.MoveToWorld( item.GetWorldLocation(), item.Map );
                        }
                        else
                        {
                                deed.MoveToWorld( Location, Map );
                        }

                        Delete();

                        return quality;
                }
        }
}
