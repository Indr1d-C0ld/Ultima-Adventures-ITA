using System;
using System.Collections.Generic;
using Server;
using Server.Engines.Harvest;
using Server.Misc;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public class CastingFishingNet : Item
    {
        private bool m_InUse;
        private int m_UsesRemaining;

        private const int DefaultUses = 50;

        public override bool BlocksFit
        {
            get { return false; }
        }

        [Constructable]
        public CastingFishingNet()
            : base(0x0DCA)
        {
            Name = "casting fishing net";
            Weight = 4.0;
            Hue = Utility.RandomList(m_Hues);
            m_UsesRemaining = DefaultUses;
        }

        private static readonly int[] m_Hues = new int[]
        {
            0x09B,
            0x0CD,
            0x0D3,
            0x14D,
            0x1DD,
            0x1E9,
            0x1F4,
            0x373,
            0x451,
            0x47F,
            0x489,
            0x492,
            0x4B5,
            0x8AA
        };

        public CastingFishingNet(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add(1070722, "Cast this from the shoreline or deck");
            list.Add(1049644, "Requires 50 Fishing");
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining
        {
            get { return m_UsesRemaining; }
            set
            {
                m_UsesRemaining = value;
                InvalidateProperties();
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add(1060584, m_UsesRemaining.ToString()); // uses remaining: ~1_val~
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)2); // version
            writer.Write(m_InUse);
            writer.Write(m_UsesRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                    m_InUse = reader.ReadBool();
                    m_UsesRemaining = reader.ReadInt();
                    break;
                case 1:
                    m_InUse = false;
                    m_UsesRemaining = reader.ReadInt();
                    break;
                case 0:
                    m_InUse = reader.ReadBool();
                    m_UsesRemaining = DefaultUses;
                    break;
                default:
                    m_InUse = false;
                    m_UsesRemaining = DefaultUses;
                    break;
            }

            if (m_InUse)
            {
                Delete();
                return;
            }

            if (m_UsesRemaining < 0)
            {
                m_UsesRemaining = 0;
            }

            Stackable = false;
            InvalidateProperties();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Deleted)
            {
                return;
            }

            if (m_InUse)
            {
                from.SendLocalizedMessage(1010483); // Someone is already using that net!
                return;
            }

            if (m_UsesRemaining <= 0)
            {
                from.SendMessage("This casting fishing net is too damaged to use.");
                return;
            }

            if (from.Mounted)
            {
                from.SendLocalizedMessage(1010097); // You cannot use this while mounted.
                return;
            }

            if (from.Skills[SkillName.Fishing].Value < 50.0)
            {
                from.SendMessage("You are not skilled enough at fishing to work this net.");
                return;
            }

            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return;
            }

            from.SendLocalizedMessage(1010484); // Where do you wish to use the net?
            from.BeginTarget(-1, true, TargetFlags.None, new TargetStateCallback(OnTarget), null);
        }

        private void OnTarget(Mobile from, object obj, object state)
        {
            OnTarget(from, obj);
        }

        private void OnTarget(Mobile from, object obj)
        {
            if (Deleted || m_InUse)
            {
                return;
            }

            IPoint3D p3D = obj as IPoint3D;

            if (p3D == null)
            {
                return;
            }

            if (from != null && from.Mounted)
            {
                from.SendLocalizedMessage(1010097); // You cannot use this while mounted.
                return;
            }

            Map map = from.Map;

            if (map == null || map == Map.Internal)
            {
                return;
            }

            int x = p3D.X, y = p3D.Y;

            LandTile landTile = map.Tiles.GetLandTile(x, y);
            StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y, true);

            bool hasWater = false;

            if (landTile.Z == p3D.Z && Worlds.IsWaterTile(landTile.ID, 0))
            {
                hasWater = true;
            }

            for (int i = 0; i < tiles.Length && !hasWater; ++i)
            {
                StaticTile tile = tiles[i];

                if (tile.Z == p3D.Z && Worlds.IsWaterTile(tile.ID, 0))
                {
                    hasWater = true;
                }
            }

            Point3D checkPoint = new Point3D(x, y, p3D.Z);

            if (!hasWater)
            {
                IPooledEnumerable eable = map.GetItemsInRange(checkPoint, 0);

                try
                {
                    foreach (Item item in eable)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        if (item.Z == p3D.Z && Worlds.IsWaterTile(item.ItemID & 0x3FFF, 0))
                        {
                            hasWater = true;
                            break;
                        }
                    }
                }
                finally
                {
                    eable.Free();
                }
            }

            if (!from.InRange(p3D, 5))
            {
                from.SendLocalizedMessage(500976); // You need to be closer to the water to fish!
                return;
            }

            if (!hasWater)
            {
                from.SendLocalizedMessage(1010485); // You can only use this net in deep water!
                return;
            }

            Point3D p = checkPoint;

            m_InUse = true;
            Movable = false;
            ItemID = 0x0DCA;

            MoveToWorld(p, map);

            from.Animate(12, 5, 1, true, false, 0);

            SendOverheadMessages(from);

            Timer.DelayCall(TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(1.0), 20, new TimerStateCallback(DoEffect), new object[] { p, 0, from });

            from.SendLocalizedMessage(1010487); // You plunge the net into the sea...
        }

        private static readonly MessageType m_MessageType = MessageType.Regular;

        private void SendOverheadMessages(Mobile from)
        {
            if (from == null)
            {
                return;
            }

            from.PublicOverheadMessage(m_MessageType, 0x3B2, false, "You gather up the net to throw it.");
            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () => from.PublicOverheadMessage(m_MessageType, 0x3B2, false, "You throw the net out over the water."));
            Timer.DelayCall(TimeSpan.FromSeconds(4.5), () => from.PublicOverheadMessage(m_MessageType, 0x3B2, false, "You are waiting for the net to sink."));
            Timer.DelayCall(TimeSpan.FromSeconds(8.5), () => from.PublicOverheadMessage(m_MessageType, 0x3B2, false, "You pull the cord to start raising the net."));
        }

        private void DoEffect(object state)
        {
            if (Deleted)
            {
                return;
            }

            object[] states = (object[])state;

            Point3D p = (Point3D)states[0];
            int index = (int)states[1];
            Mobile from = (Mobile)states[2];

            states[1] = ++index;

            if (index == 1)
            {
                Effects.SendLocationEffect(p, Map, 0x352D, 16, 4);
                Effects.PlaySound(p, Map, 0x364);
            }
            else if (index <= 10 || index == 20)
            {
                for (int i = 0; i < 3; ++i)
                {
                    int ox, oy;

                    switch (Utility.Random(8))
                    {
                        default:
                        case 0: ox = -1; oy = -1; break;
                        case 1: ox = -1; oy = 0; break;
                        case 2: ox = -1; oy = +1; break;
                        case 3: ox = 0; oy = -1; break;
                        case 4: ox = 0; oy = +1; break;
                        case 5: ox = +1; oy = -1; break;
                        case 6: ox = +1; oy = 0; break;
                        case 7: ox = +1; oy = +1; break;
                    }

                    Effects.SendLocationEffect(new Point3D(p.X + ox, p.Y + oy, p.Z), Map, 0x352D, 16, 4);
                }

                Effects.PlaySound(p, Map, 0x364);

                if (index != 20)
                {
                    Z -= 1;
                }
                else
                {
                    FinishEffect(p, Map, from);
                }
            }
        }

        protected virtual void FinishEffect(Point3D p, Map map, Mobile from)
        {
            if (Deleted)
            {
                return;
            }

            m_InUse = false;
            Movable = true;

            if (from != null)
            {
                from.RevealingAction();
            }

            if (from != null)
            {
                Fishing.FishingSkill(from, 10);

                bool depleted;
                List<Fishing.NetCatchResult> results = Fishing.System.CollectNetCatch(from, this, map, p, 10, true, out depleted);

                int fishCount = 0;
                int junkCount = 0;
                int awarded = 0;

                HarvestDefinition def = Fishing.System.Definition;

                foreach (Fishing.NetCatchResult result in results)
                {
                    if (result == null || result.Item == null)
                    {
                        continue;
                    }

                    bool placed = Fishing.System.Give(from, result.Item, true);

                    if (placed)
                    {
                        awarded++;

                        if (!string.IsNullOrEmpty(result.CustomMessage))
                        {
                            from.SendMessage(result.CustomMessage);
                        }
                        else if (result.Resource != null)
                        {
                            Fishing.System.SendSuccessTo(from, result.Item, result.Resource);
                        }
                        else if (result.Bonus != null)
                        {
                            result.Bonus.SendSuccessTo(from);
                        }

                        if (result.Item is Fish || result.Item is BigFish || result.Item is BaseMagicFish || result.Item is NewFish)
                        {
                            fishCount += result.Item.Amount;
                        }
                        else
                        {
                            junkCount += result.Item.Amount;
                        }
                    }
                    else
                    {
                        if (def != null)
                        {
                            def.SendMessageTo(from, def.PackFullMessage);
                        }
                        else
                        {
                            from.SendLocalizedMessage(503204);
                        }
                        result.Item.Delete();
                    }
                }

                if (awarded > 0)
                {
                    from.PublicOverheadMessage(m_MessageType, 0x3B2, false, "You drag the net back aboard.");
                    from.SendMessage("You haul in {0} fish and {1} pieces of junk with the net.", fishCount, junkCount);
                }
                else if (depleted)
                {
                    if (def != null)
                    {
                        def.SendMessageTo(from, def.NoResourcesMessage);
                    }
                    else
                    {
                        from.SendLocalizedMessage(503172); // The fish don't seem to be biting here.
                    }
                }
                else
                {
                    if (def != null)
                    {
                        def.SendMessageTo(from, def.FailMessage);
                    }
                    else
                    {
                        from.SendLocalizedMessage(503171); // You fish a while, but fail to catch anything.
                    }
                }
            }

            if (!ConsumeUse(from))
            {
                ReturnToOwner(from);
            }
        }

        private bool ConsumeUse(Mobile from)
        {
            if (m_UsesRemaining > 0)
            {
                m_UsesRemaining--;

                if (m_UsesRemaining <= 0)
                {
                    if (from != null)
                    {
                        from.SendMessage("The casting fishing net tears apart after so much use.");
                    }

                    Delete();
                    return true;
                }

                InvalidateProperties();
            }

            return false;
        }

        private void ReturnToOwner(Mobile from)
        {
            if (Deleted)
            {
                return;
            }

            ItemID = 0x0DCA;

            if (from != null && from.Backpack != null && from.Backpack.TryDropItem(from, this, false))
                return;

            if (from != null)
                MoveToWorld(from.Location, from.Map);
            else
                MoveToWorld(Location, Map);
        }
    }
}
