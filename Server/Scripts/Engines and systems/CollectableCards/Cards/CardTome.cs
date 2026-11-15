using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.ContextMenus;

namespace Server.Items
{
    public class CardTome : Item, ISecurable
    {
        private List<CollectableCard> m_Cards;
        private SecureLevel m_Level;
        private string m_OwnerName;

        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string OwnerName
        {
            get { return m_OwnerName; }
            set { m_OwnerName = value; InvalidateProperties(); }
        }

        public List<CollectableCard> Cards
        {
            get { return m_Cards; }
        }

        public int TotalAchievementPoints
        {
            get
            {
                int total = 0;
                foreach (CollectableCard card in m_Cards)
                    total += card.AchievementPoints;
                return total;
            }
        }

        public int CardCount
        {
            get { return m_Cards.Count; }
        }

        public CollectableCard FeaturedCard
        {
            get
            {
                if (m_Cards.Count == 0)
                    return null;
                return m_Cards[m_Cards.Count - 1];
            }
        }

        [Constructable]
        public CardTome() : base(CardSystemConfig.TomeItemID)
        {
            Weight = 1.0;
            Hue = CardSystemConfig.TomeHue;
            m_Cards = new List<CollectableCard>();
            m_OwnerName = "Unknown";
            m_Level = SecureLevel.CoOwners;
            Name = "Collectable Card Tome";
        }

        public void SetOwner(Mobile from)
        {
            if (from != null && !string.IsNullOrEmpty(from.Name))
            {
                m_OwnerName = from.Name;
                Name = string.Format("{0}'s Card Collection", m_OwnerName);
                InvalidateProperties();
            }
        }

        public int GetCardCountInCategory(CardCategory category)
        {
            int count = 0;
            foreach (CollectableCard card in m_Cards)
            {
                if (card.Category == category)
                    count++;
            }
            return count;
        }

        public List<CollectableCard> GetCardsInCategory(CardCategory category)
        {
            List<CollectableCard> categoryCards = new List<CollectableCard>();
            foreach (CollectableCard card in m_Cards)
            {
                if (card.Category == category)
                    categoryCards.Add(card);
            }
            return categoryCards;
        }

        public bool AddCard(CollectableCard card)
        {
            if (card == null)
                return false;

            foreach (CollectableCard existing in m_Cards)
            {
                if (existing.CardName == card.CardName && existing.Category == card.Category)
                    return false;
            }

            m_Cards.Add(card);
            card.Internalize();
            InvalidateProperties();
            return true;
        }

        public bool RemoveCard(CollectableCard card)
        {
            if (m_Cards.Contains(card))
            {
                m_Cards.Remove(card);
                InvalidateProperties();
                return true;
            }
            return false;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!string.IsNullOrEmpty(m_OwnerName) && m_OwnerName != "Unknown")
                list.Add(1060658, "Owner\t{0}", m_OwnerName);

            list.Add(1060659, "Cards\t{0}", CardCount.ToString());

            // Only show points if enabled
            if (CardSystemConfig.EnablePointsSystem)
            {
                list.Add(1070722, string.Format("Total Points: {0}", TotalAchievementPoints));
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(GetWorldLocation(), 3) || IsChildOf(from.Backpack) || RootParent is BankBox)
            {
                if (CheckAccess(from))
                {
                    // Check if this tome is inside a player vendor - if so, don't bind
                    bool isInVendor = false;
                    Item parent = RootParent as Item;
                    while (parent != null)
                    {
                        if (parent is PlayerVendor)
                        {
                            isInVendor = true;
                            break;
                        }
                        parent = parent.RootParent as Item;
                    }

                    // Only bind if not in a vendor and in personal inventory
                    bool shouldBind = !isInVendor && (IsChildOf(from.Backpack) || RootParent is BankBox);

                    if (shouldBind && (string.IsNullOrEmpty(m_OwnerName) || m_OwnerName == "Unknown"))
                    {
                        SetOwner(from);
                        from.SendMessage("This tome is now bound to you.");
                    }

                    from.PlaySound(CardSystemConfig.TomeOpenSound);
                    from.CloseGump(typeof(CardTomeGump));
                    from.SendGump(new CardTomeGump(from, this));
                }
                else
                {
                    from.SendMessage("You do not have permission to use this tome.");
                }
            }
            else
            {
                from.SendLocalizedMessage(500446);
            }
        }

        public bool CheckAccess(Mobile m)
        {
            // GMs always have access
            if (m.AccessLevel >= AccessLevel.GameMaster)
                return true;

            // In player's backpack - always accessible
            if (IsChildOf(m.Backpack))
                return true;

            // In bank box - always accessible
            if (RootParent is BankBox)
            {
                BankBox bankBox = (BankBox)RootParent;
                if (bankBox.Owner == m)
                    return true;
            }

            // Allow viewing in a player vendor inventory
            if (RootParent is PlayerVendor)
                return true;

            if (RootParent is VendorBackpack)
            {
                VendorBackpack backpack = (VendorBackpack)RootParent;
                if (backpack.RootParent is PlayerVendor)
                    return true;
            }

            // On the ground (not locked down) - accessible to anyone
            if (!IsLockedDown && RootParent == null)
                return true;

            // Locked down in a house - check house permissions
            if (IsLockedDown)
            {
                BaseHouse house = BaseHouse.FindHouseAt(this);
                if (house != null)
                {
                    if (house.IsOwner(m))
                        return true;
                    return house.HasSecureAccess(m, m_Level);
                }
            }

            return false;
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is CollectableCard)
            {
                CollectableCard card = (CollectableCard)dropped;

                if (AddCard(card))
                {
                    from.SendMessage("You add the card to your tome.");
                    from.PlaySound(CardSystemConfig.CardAddSound);
                    return true;
                }
                else
                {
                    from.SendMessage("You already have this card in your tome.");
                    return false;
                }
            }

            return false;
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            SetSecureLevelEntry.AddTo(from, this, list);

            list.Add(new OpenTomeMenuEntry(from, this));
            list.Add(new GuideMenuEntry(from, this));
        }

        public CardTome(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);

            writer.Write(m_OwnerName);
            writer.Write((int)m_Level);
            writer.Write(m_Cards.Count);

            foreach (CollectableCard card in m_Cards)
                writer.Write(card);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_OwnerName = reader.ReadString();
            m_Level = (SecureLevel)reader.ReadInt();

            int count = reader.ReadInt();
            m_Cards = new List<CollectableCard>(count);

            for (int i = 0; i < count; i++)
            {
                CollectableCard card = reader.ReadItem() as CollectableCard;
                if (card != null)
                    m_Cards.Add(card);
            }

            if (!string.IsNullOrEmpty(m_OwnerName) && m_OwnerName != "Unknown")
            {
                Name = string.Format("{0}'s Card Collection", m_OwnerName);
            }
        }

        private class GuideMenuEntry : ContextMenuEntry
        {
            private Mobile m_From;
            private CardTome m_Tome;

            public GuideMenuEntry(Mobile from, CardTome tome) : base(1075993)
            {
                m_From = from;
                m_Tome = tome;
            }

            public override void OnClick()
            {
                if (m_Tome != null && !m_Tome.Deleted)
                {
                    m_From.CloseGump(typeof(CardTomeGuideGump));
                    m_From.SendGump(new CardTomeGuideGump(m_From, m_Tome));
                }
            }
        }

        private class OpenTomeMenuEntry : ContextMenuEntry
        {
            private Mobile m_From;
            private CardTome m_Tome;

            public OpenTomeMenuEntry(Mobile from, CardTome tome) : base(3006122)
            {
                m_From = from;
                m_Tome = tome;
            }

            public override void OnClick()
            {
                if (m_Tome != null && !m_Tome.Deleted)
                {
                    m_Tome.OnDoubleClick(m_From);
                }
            }
        }
    }
}
