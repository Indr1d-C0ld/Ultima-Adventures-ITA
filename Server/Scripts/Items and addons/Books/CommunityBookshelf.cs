using System;
using System.Collections.Generic;
using Server;
using Server.Engines.BulkOrders;
using Server.Gumps;
using Server.Items;
using Server.Network;

namespace Server.Items
{
    [Flipable(0x0A97, 0x0A98, 0x0A9A, 0x0A9B)]
    public class CommunityBookshelf : BaseContainer
    {
        private static readonly string[] KeywordFilters = new string[]
        {
            "book",
            "scroll",
            "journal",
            "tome",
            "guide",
            "codex",
            "manual",
            "lore",
            "note"
        };

        private class BookTitleComparer : IComparer<Item>
        {
            public int Compare(Item x, Item y)
            {
                string a = CommunityBookshelf.FormatBookTitle(x);
                string b = CommunityBookshelf.FormatBookTitle(y);

                return Utility.InsensitiveCompare(a, b);
            }
        }

        private readonly List<string> m_Tabs = new List<string>();
        private readonly Dictionary<int, int> m_BookTabs = new Dictionary<int, int>();

        [Constructable]
        public CommunityBookshelf() : base(0x0A97)
        {
            Name = "community bookshelf";
            Weight = 25.0;
            Movable = true;
        }

        public override bool DisplaysContent
        {
            get { return false; }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return;
            }

            from.CloseGump(typeof(CommunityBookshelfGump));
            from.SendGump(new CommunityBookshelfGump(from, this));
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            return TryStore(from, dropped, null);
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            return TryStore(from, item, p);
        }

        private bool TryStore(Mobile from, Item dropped, Point3D? location)
        {
            if (!IsAllowedBook(dropped))
            {
                from.SendMessage("Only pre-written quest books, lore tomes and guide scrolls belong on this shelf.");
                return false;
            }

            if (ContainsDuplicateBook(dropped))
            {
                from.SendMessage("A copy of {0} is already resting on this shelf.", FormatBookTitle(dropped));
                return false;
            }

            bool stored;

            if (location.HasValue)
            {
                stored = base.OnDragDropInto(from, dropped, location.Value);
            }
            else
            {
                stored = base.OnDragDrop(from, dropped);
            }

            if (stored)
            {
                dropped.Movable = false;
                from.SendMessage("You carefully slide {0} onto the shelf.", FormatBookTitle(dropped));
                InvalidateProperties();
            }

            return stored;
        }

        public List<string> GetTabs()
        {
            return new List<string>(m_Tabs);
        }

        public int GetBookTab(Item item)
        {
            if (item == null || item.Deleted)
            {
                return -1;
            }

            int tabIndex;

            if (m_BookTabs.TryGetValue(item.Serial.Value, out tabIndex))
            {
                if (tabIndex >= 0 && tabIndex < m_Tabs.Count)
                {
                    return tabIndex;
                }

                m_BookTabs.Remove(item.Serial.Value);
            }

            return -1;
        }

        public void AssignBookToTab(Item item, int tabIndex)
        {
            if (item == null || item.Deleted)
            {
                return;
            }

            if (tabIndex < 0 || tabIndex >= m_Tabs.Count)
            {
                m_BookTabs.Remove(item.Serial.Value);
                return;
            }

            m_BookTabs[item.Serial.Value] = tabIndex;
        }

        public bool AddTab(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            name = name.Trim();

            if (name.Length == 0)
            {
                return false;
            }

            foreach (string tab in m_Tabs)
            {
                if (!string.IsNullOrEmpty(tab) && Utility.InsensitiveCompare(tab, name) == 0)
                {
                    return false;
                }
            }

            m_Tabs.Add(name);
            return true;
        }

        public void RenameTab(int index, string name)
        {
            if (index < 0 || index >= m_Tabs.Count)
            {
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            name = name.Trim();

            if (name.Length == 0)
            {
                return;
            }

            for (int i = 0; i < m_Tabs.Count; ++i)
            {
                if (i == index)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(m_Tabs[i]) && Utility.InsensitiveCompare(m_Tabs[i], name) == 0)
                {
                    return;
                }
            }

            m_Tabs[index] = name;
        }

        public void RemoveTab(int index)
        {
            if (index < 0 || index >= m_Tabs.Count)
            {
                return;
            }

            m_Tabs.RemoveAt(index);

            if (m_BookTabs.Count == 0)
            {
                return;
            }

            List<int> keys = new List<int>(m_BookTabs.Keys);

            foreach (int key in keys)
            {
                int tabIndex;

                if (!m_BookTabs.TryGetValue(key, out tabIndex))
                {
                    continue;
                }

                if (tabIndex == index)
                {
                    m_BookTabs.Remove(key);
                }
                else if (tabIndex > index)
                {
                    m_BookTabs[key] = tabIndex - 1;
                }
            }
        }

        public void ReadBook(Mobile from, Item book)
        {
            if (from == null || Deleted)
            {
                return;
            }

            if (book == null || book.Deleted || book.Parent != this)
            {
                from.SendMessage("That text is no longer available.");
                return;
            }

            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
                return;
            }

            bool requiresBackpack = RequiresBackpackHandling(book);
            Container pack = from.Backpack;

            if (requiresBackpack && pack == null)
            {
                from.SendMessage("You need a backpack to flip through that volume.");
                return;
            }

            Point3D location = book.Location;
            bool moved = false;

            try
            {
                if (requiresBackpack)
                {
                    RemoveItem(book);
                    pack.AddItem(book);
                    moved = true;
                }

                book.OnDoubleClick(from);
            }
            finally
            {
                if (moved && !Deleted && book != null && !book.Deleted)
                {
                    if (book.Parent == pack)
                    {
                        pack.RemoveItem(book);
                    }

                    AddItem(book);
                    book.Location = location;
                    book.Movable = false;
                }
            }
        }

        public List<Item> GetReadableBooks(int tabIndex)
        {
            List<Item> list = new List<Item>();
            CleanupBookTabs();

            foreach (Item item in Items)
            {
                if (!IsAllowedBook(item))
                {
                    continue;
                }

                if (tabIndex >= 0 && GetBookTab(item) != tabIndex)
                {
                    continue;
                }

                list.Add(item);
            }

            list.Sort(new BookTitleComparer());
            return list;
        }

        public List<Item> GetReadableBooks()
        {
            return GetReadableBooks(-1);
        }

        private void CleanupBookTabs()
        {
            if (m_BookTabs.Count == 0)
            {
                return;
            }

            List<int> keys = new List<int>(m_BookTabs.Keys);

            foreach (int key in keys)
            {
                int tabIndex;

                if (!m_BookTabs.TryGetValue(key, out tabIndex))
                {
                    continue;
                }

                if (tabIndex < 0 || tabIndex >= m_Tabs.Count)
                {
                    m_BookTabs.Remove(key);
                    continue;
                }

                bool found = false;

                foreach (Item item in Items)
                {
                    if (item != null && !item.Deleted && item.Serial.Value == key)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    m_BookTabs.Remove(key);
                }
            }
        }

        public static bool IsAllowedBook(Item item)
        {
            if (item == null || item.Deleted)
            {
                return false;
            }

            if (item.Stackable)
            {
                return false;
            }

            if (item is Spellbook || item is Runebook || item is BulkOrderBook)
            {
                return false;
            }

            BaseBook baseBook = item as BaseBook;

            if (baseBook != null)
            {
                return !baseBook.Writable;
            }

            if (item is DynamicBook || item is SwordsAndShackles)
            {
                return true;
            }

            string typeName = item.GetType().Name;
            string name = item.Name;

            foreach (string keyword in KeywordFilters)
            {
                if (!string.IsNullOrEmpty(typeName) && typeName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(name) && name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool RequiresBackpackHandling(Item item)
        {
            return !(item is BaseBook) && !(item is DynamicBook);
        }

        private bool ContainsDuplicateBook(Item dropped)
        {
            string newTitle = FormatBookTitle(dropped);

            foreach (Item item in Items)
            {
                if (item == null || item.Deleted || item == dropped)
                {
                    continue;
                }

                if (!IsAllowedBook(item))
                {
                    continue;
                }

                if (Utility.InsensitiveCompare(FormatBookTitle(item), newTitle) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static string FormatBookTitle(Item item)
        {
            if (item == null)
            {
                return "a book";
            }

            BaseBook baseBook = item as BaseBook;

            if (baseBook != null)
            {
                if (!string.IsNullOrEmpty(baseBook.Title))
                {
                    return baseBook.Title;
                }
            }
            else
            {
                DynamicBook dynBook = item as DynamicBook;

                if (dynBook != null)
                {
                    if (!string.IsNullOrEmpty(dynBook.BookTitle))
                    {
                        return dynBook.BookTitle;
                    }
                }
            }

            if (!string.IsNullOrEmpty(item.Name))
            {
                return item.Name;
            }

            if (item.LabelNumber > 0)
            {
                return String.Format("#{0}", item.LabelNumber);
            }

            return item.GetType().Name;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("Stored texts: {0}", GetReadableBooks().Count);
        }

        public CommunityBookshelf(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);

            writer.Write(m_Tabs.Count);

            for (int i = 0; i < m_Tabs.Count; ++i)
            {
                writer.Write(m_Tabs[i]);
            }

            writer.Write(m_BookTabs.Count);

            foreach (KeyValuePair<int, int> entry in m_BookTabs)
            {
                writer.Write(entry.Key);
                writer.Write(entry.Value);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version >= 1)
            {
                int tabCount = reader.ReadInt();

                for (int i = 0; i < tabCount; ++i)
                {
                    m_Tabs.Add(reader.ReadString());
                }

                int mappingCount = reader.ReadInt();

                for (int i = 0; i < mappingCount; ++i)
                {
                    int serial = reader.ReadInt();
                    int tabIndex = reader.ReadInt();
                    m_BookTabs[serial] = tabIndex;
                }
            }
        }
    }
}

namespace Server.Gumps
{
    public class CommunityBookshelfGump : Gump
    {
        private const int EntriesPerPage = 10;
        private const int PrevButtonID = 1;
        private const int NextButtonID = 2;
        private const int OptionsButtonID = 3;
        private const int BookButtonBase = 1000;
        private const int RemoveButtonBase = 2000;
        private const int MoveButtonBase = 2500;
        private const int TabButtonBase = 3000;

        private readonly CommunityBookshelf m_Shelf;
        private readonly List<Item> m_Books;
        private readonly int m_Page;
        private readonly bool m_CanAdminRemove;
        private readonly int m_TabIndex;
        private readonly List<string> m_Tabs;

        public CommunityBookshelfGump(Mobile from, CommunityBookshelf shelf, int page, int tabIndex) : base(50, 50)
        {
            m_Shelf = shelf;
            m_Page = Math.Max(0, page);
            m_TabIndex = tabIndex;
            m_CanAdminRemove = from != null && from.AccessLevel >= AccessLevel.Administrator;

            if (shelf != null)
            {
                m_Tabs = shelf.GetTabs();
                m_Books = shelf.GetReadableBooks(m_TabIndex);
            }
            else
            {
                m_Tabs = new List<string>();
                m_Books = new List<Item>();
            }

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddBackground(0, 0, 440, 460, 9270);
            AddAlphaRegion(10, 10, 420, 440);

            string title = "bookshelf";

            if (shelf != null && !String.IsNullOrEmpty(shelf.Name))
            {
                title = shelf.Name;
            }

            AddHtml(20, 20, 400, 20, String.Format("<BASEFONT COLOR=#F0E68C><CENTER>{0}</CENTER></BASEFONT>", Utility.FixHtml(title)), false, false);
            AddHtml(25, 50, 390, 40, "<BASEFONT COLOR=#DDDDDD>Select a title to read. Drag pre-made lore books and guide scrolls onto the shelf to share them with others.</BASEFONT>", false, false);

            int tabY = 95;
            int tabX = 25;
            int tabButtonWidth = 90;

            AddButton(tabX, tabY, 4005, 4007, TabButtonBase, GumpButtonType.Reply, 0);
            AddHtml(tabX + 33, tabY, tabButtonWidth, 20, String.Format("<BASEFONT COLOR=#F0E68C>{0}</BASEFONT>", m_TabIndex < 0 ? "[All]" : "All"), false, false);
            tabX += tabButtonWidth + 5;

            for (int i = 0; i < m_Tabs.Count; ++i)
            {
                if (tabX + tabButtonWidth > 390)
                {
                    tabX = 25;
                    tabY += 22;
                }

                string label = Utility.FixHtml(m_Tabs[i]);
                bool selected = (m_TabIndex == i);

                AddButton(tabX, tabY, 4005, 4007, TabButtonBase + 1 + i, GumpButtonType.Reply, 0);
                AddHtml(tabX + 33, tabY, tabButtonWidth, 20, String.Format("<BASEFONT COLOR=#{0}>{1}</BASEFONT>", selected ? "F0E68C" : "FFFFFF", selected ? "[" + label + "]" : label), false, false);
                tabX += tabButtonWidth + 5;
            }

            if (m_CanAdminRemove)
            {
                AddButton(340, 71, 4011, 4013, OptionsButtonID, GumpButtonType.Reply, 0);
                AddHtml(373, 71, 70, 20, "<BASEFONT COLOR=#FFDD88>Options</BASEFONT>", false, false);
            }

            if (m_Books.Count == 0)
            {
                AddHtml(25, tabY + 35, 370, 200, "<BASEFONT COLOR=#AAAAAA>No books have been stored yet.</BASEFONT>", false, false);
                return;
            }

            int start = m_Page * EntriesPerPage;
            int end = Math.Min(start + EntriesPerPage, m_Books.Count);
            int y = tabY + 35;
            int textWidth = m_CanAdminRemove ? 235 : 340;

            for (int i = start; i < end; ++i)
            {
                Item book = m_Books[i];
                string bookName = CommunityBookshelf.FormatBookTitle(book);

                AddButton(30, y, 4005, 4007, BookButtonBase + i, GumpButtonType.Reply, 0);
                AddHtml(65, y, textWidth, 20, String.Format("<BASEFONT COLOR=#FFFFFF>{0}</BASEFONT>", Utility.FixHtml(bookName)), false, false);

                if (m_CanAdminRemove)
                {
                    AddButton(300, y, 4011, 4013, MoveButtonBase + i, GumpButtonType.Reply, 0);
                    AddHtml(330, y, 48, 20, "<BASEFONT COLOR=#AADDEE>Move</BASEFONT>", false, false);

                    AddButton(370, y, 4011, 4013, RemoveButtonBase + i, GumpButtonType.Reply, 0);
                    AddHtml(400, y, 40, 20, "<BASEFONT COLOR=#FFAAAA>Take</BASEFONT>", false, false);
                }

                y += 25;
            }

            int totalPages = (m_Books.Count + EntriesPerPage - 1) / EntriesPerPage;
            AddHtml(170, 410, 120, 20, String.Format("<BASEFONT COLOR=#F0E68C><CENTER>Page {0}/{1}</CENTER></BASEFONT>", m_Page + 1, Math.Max(1, totalPages)), false, false);

            if (m_Page > 0)
            {
                AddButton(130, 410, 4014, 4016, PrevButtonID, GumpButtonType.Reply, 0);
                AddHtml(165, 412, 60, 20, "<BASEFONT COLOR=#FFFFFF>Prev</BASEFONT>", false, false);
            }

            if (end < m_Books.Count)
            {
                AddButton(260, 410, 4005, 4007, NextButtonID, GumpButtonType.Reply, 0);
                AddHtml(295, 412, 60, 20, "<BASEFONT COLOR=#FFFFFF>Next</BASEFONT>", false, false);
            }
        }

        public CommunityBookshelfGump(Mobile from, CommunityBookshelf shelf, int page) : this(from, shelf, page, -1)
        {
        }

        public CommunityBookshelfGump(Mobile from, CommunityBookshelf shelf) : this(from, shelf, 0, -1)
        {
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            if (m_Shelf == null || m_Shelf.Deleted || from == null)
            {
                return;
            }

            if (info.ButtonID == OptionsButtonID && m_CanAdminRemove)
            {
                from.SendGump(new CommunityBookshelfOptionsGump(from, m_Shelf, m_Page, m_TabIndex));
                return;
            }

            if (info.ButtonID == PrevButtonID)
            {
                if (m_Page > 0)
                {
                    from.SendGump(new CommunityBookshelfGump(from, m_Shelf, m_Page - 1, m_TabIndex));
                }
                return;
            }

            if (info.ButtonID == NextButtonID)
            {
                int totalPages = (m_Books.Count + EntriesPerPage - 1) / EntriesPerPage;

                if (m_Page + 1 < totalPages)
                {
                    from.SendGump(new CommunityBookshelfGump(from, m_Shelf, m_Page + 1, m_TabIndex));
                }
                return;
            }

            if (info.ButtonID >= TabButtonBase && info.ButtonID < TabButtonBase + 1 + m_Tabs.Count)
            {
                int tabIndex = info.ButtonID - TabButtonBase - 1;

                if (info.ButtonID == TabButtonBase)
                {
                    tabIndex = -1;
                }

                from.SendGump(new CommunityBookshelfGump(from, m_Shelf, 0, tabIndex));
                return;
            }

            if (m_CanAdminRemove && info.ButtonID >= MoveButtonBase && info.ButtonID < MoveButtonBase + m_Books.Count)
            {
                int moveIndex = info.ButtonID - MoveButtonBase;

                if (moveIndex >= 0 && moveIndex < m_Books.Count)
                {
                    Item toMove = m_Books[moveIndex];

                    if (toMove != null && !toMove.Deleted && toMove.Parent == m_Shelf)
                    {
                        from.SendGump(new CommunityBookshelfAssignTabGump(from, m_Shelf, toMove, m_Page, m_TabIndex));
                        return;
                    }
                }

                from.SendGump(new CommunityBookshelfGump(from, m_Shelf, m_Page, m_TabIndex));
                return;
            }

            if (m_CanAdminRemove && info.ButtonID >= RemoveButtonBase && info.ButtonID < RemoveButtonBase + m_Books.Count)
            {
                int removeIndex = info.ButtonID - RemoveButtonBase;
                Item toRemove = null;

                if (removeIndex >= 0 && removeIndex < m_Books.Count)
                {
                    toRemove = m_Books[removeIndex];

                    if (toRemove != null && !toRemove.Deleted && toRemove.Parent == m_Shelf)
                    {
                        Container pack = from.Backpack;
                        m_Shelf.RemoveItem(toRemove);
                        toRemove.Movable = true;

                        if (pack != null)
                        {
                            pack.AddItem(toRemove);
                        }
                        else
                        {
                            toRemove.MoveToWorld(from.Location, from.Map);
                        }

                        from.SendMessage("You remove {0} from the shelf.", CommunityBookshelf.FormatBookTitle(toRemove));
                    }
                }

                if (toRemove != null)
                {
                    m_Shelf.AssignBookToTab(toRemove, -1);
                }

                from.SendGump(new CommunityBookshelfGump(from, m_Shelf, m_Page, m_TabIndex));
                return;
            }

            if (info.ButtonID >= BookButtonBase)
            {
                int index = info.ButtonID - BookButtonBase;

                if (index >= 0 && index < m_Books.Count)
                {
                    Item book = m_Books[index];
                    m_Shelf.ReadBook(from, book);
                }

                from.SendGump(new CommunityBookshelfGump(from, m_Shelf, m_Page, m_TabIndex));
            }
        }
    }

    public class CommunityBookshelfOptionsGump : Gump
    {
        private const int AddTabButtonID = 1;
        private const int TabNameEntryBase = 100;
        private const int UpdateTabButtonBase = 2000;
        private const int RemoveTabButtonBase = 3000;

        private readonly CommunityBookshelf m_Shelf;
        private readonly int m_Page;
        private readonly int m_TabIndex;

        public CommunityBookshelfOptionsGump(Mobile from, CommunityBookshelf shelf, int page, int tabIndex) : base(80, 80)
        {
            m_Shelf = shelf;
            m_Page = page;
            m_TabIndex = tabIndex;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddBackground(0, 0, 420, 360, 9270);
            AddAlphaRegion(10, 10, 400, 340);

            AddHtml(20, 20, 380, 20, "<BASEFONT COLOR=#F0E68C><CENTER>Bookshelf Tabs</CENTER></BASEFONT>", false, false);
            AddHtml(25, 50, 370, 20, "<BASEFONT COLOR=#DDDDDD>Rename, add, or remove tabs to organize the shelf.</BASEFONT>", false, false);

            List<string> tabs = shelf != null ? shelf.GetTabs() : new List<string>();
            int y = 85;

            for (int i = 0; i < tabs.Count; ++i)
            {
                AddTextEntry(30, y, 220, 20, 1152, TabNameEntryBase + i, tabs[i]);
                AddButton(260, y, 4005, 4007, UpdateTabButtonBase + i, GumpButtonType.Reply, 0);
                AddHtml(285, y, 50, 20, "<BASEFONT COLOR=#AADDEE>Save</BASEFONT>", false, false);
                AddButton(330, y, 4014, 4016, RemoveTabButtonBase + i, GumpButtonType.Reply, 0);
                AddHtml(355, y, 50, 20, "<BASEFONT COLOR=#FFAAAA>Del</BASEFONT>", false, false);
                y += 25;
            }

            AddHtml(25, 280, 120, 20, "<BASEFONT COLOR=#F0E68C>Add new tab:</BASEFONT>", false, false);
            AddTextEntry(130, 278, 170, 20, 1152, AddTabButtonID, String.Empty);
            AddButton(310, 278, 4005, 4007, AddTabButtonID, GumpButtonType.Reply, 0);
            AddHtml(335, 278, 50, 20, "<BASEFONT COLOR=#AADDEE>Add</BASEFONT>", false, false);
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            if (m_Shelf == null || m_Shelf.Deleted || from == null)
            {
                return;
            }

            if (info.ButtonID == AddTabButtonID)
            {
                TextRelay entry = info.GetTextEntry(AddTabButtonID);
                string name = entry != null ? entry.Text : String.Empty;

                if (!m_Shelf.AddTab(name))
                {
                    from.SendMessage("That tab name is already in use or invalid.");
                }

                from.SendGump(new CommunityBookshelfOptionsGump(from, m_Shelf, m_Page, m_TabIndex));
                return;
            }

            if (info.ButtonID >= UpdateTabButtonBase && info.ButtonID < UpdateTabButtonBase + m_Shelf.GetTabs().Count)
            {
                int index = info.ButtonID - UpdateTabButtonBase;
                TextRelay entry = info.GetTextEntry(TabNameEntryBase + index);
                string name = entry != null ? entry.Text : String.Empty;
                m_Shelf.RenameTab(index, name);
                from.SendGump(new CommunityBookshelfOptionsGump(from, m_Shelf, m_Page, m_TabIndex));
                return;
            }

            if (info.ButtonID >= RemoveTabButtonBase && info.ButtonID < RemoveTabButtonBase + m_Shelf.GetTabs().Count)
            {
                int index = info.ButtonID - RemoveTabButtonBase;
                m_Shelf.RemoveTab(index);
                from.SendGump(new CommunityBookshelfOptionsGump(from, m_Shelf, m_Page, m_TabIndex));
                return;
            }

            from.SendGump(new CommunityBookshelfGump(from, m_Shelf, m_Page, m_TabIndex));
        }
    }

    public class CommunityBookshelfAssignTabGump : Gump
    {
        private const int TabAssignBase = 4000;

        private readonly CommunityBookshelf m_Shelf;
        private readonly Item m_Book;
        private readonly int m_Page;
        private readonly int m_TabIndex;

        public CommunityBookshelfAssignTabGump(Mobile from, CommunityBookshelf shelf, Item book, int page, int tabIndex) : base(90, 90)
        {
            m_Shelf = shelf;
            m_Book = book;
            m_Page = page;
            m_TabIndex = tabIndex;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddBackground(0, 0, 300, 300, 9270);
            AddAlphaRegion(10, 10, 280, 280);

            AddHtml(20, 20, 260, 20, "<BASEFONT COLOR=#F0E68C><CENTER>Assign Tab</CENTER></BASEFONT>", false, false);

            string title = m_Book != null ? CommunityBookshelf.FormatBookTitle(m_Book) : "book";
            AddHtml(20, 45, 260, 20, String.Format("<BASEFONT COLOR=#DDDDDD>{0}</BASEFONT>", Utility.FixHtml(title)), false, false);

            List<string> tabs = shelf != null ? shelf.GetTabs() : new List<string>();
            int y = 80;

            AddButton(20, y, 4005, 4007, TabAssignBase, GumpButtonType.Reply, 0);
            AddHtml(45, y, 200, 20, "<BASEFONT COLOR=#FFFFFF>No Tab</BASEFONT>", false, false);
            y += 25;

            for (int i = 0; i < tabs.Count; ++i)
            {
                AddButton(20, y, 4005, 4007, TabAssignBase + 1 + i, GumpButtonType.Reply, 0);
                AddHtml(45, y, 200, 20, String.Format("<BASEFONT COLOR=#FFFFFF>{0}</BASEFONT>", Utility.FixHtml(tabs[i])), false, false);
                y += 25;
            }
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            if (m_Shelf == null || m_Shelf.Deleted || from == null)
            {
                return;
            }

            if (m_Book == null || m_Book.Deleted || m_Book.Parent != m_Shelf)
            {
                from.SendMessage("That book is no longer on the shelf.");
                from.SendGump(new CommunityBookshelfGump(from, m_Shelf, m_Page, m_TabIndex));
                return;
            }

            if (info.ButtonID >= TabAssignBase)
            {
                int index = info.ButtonID - TabAssignBase - 1;

                if (info.ButtonID == TabAssignBase)
                {
                    index = -1;
                }

                m_Shelf.AssignBookToTab(m_Book, index);
            }

            from.SendGump(new CommunityBookshelfGump(from, m_Shelf, m_Page, m_TabIndex));
        }
    }
}
