using System;
using System.Collections;
using Server;
using Server.Accounting;
using Server.Commands;
using Server.Gumps;

namespace Server.Commands
{
    public static class FindAccountByCharCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("FindAccountByChar", AccessLevel.Administrator, new CommandEventHandler(FindAccountByChar_OnCommand));
            CommandSystem.Register("FindAcctByChar", AccessLevel.Administrator, new CommandEventHandler(FindAccountByChar_OnCommand));
        }

        [Usage("FindAccountByChar <characterName>")]
        [Aliases("FindAcctByChar")]
        [Description("Finds account(s) that contain a character matching the given name, including offline characters.")]
        private static void FindAccountByChar_OnCommand(CommandEventArgs e)
        {
            string search = e.ArgString == null ? String.Empty : e.ArgString.Trim();

            if (search.Length == 0)
            {
                e.Mobile.SendMessage("Format: [FindAccountByChar <characterName>");
                return;
            }

            string lowerSearch = search.ToLower();
            ArrayList matches = new ArrayList();
            ArrayList matchedAccounts = new ArrayList();

            foreach (Account account in Accounts.GetAccounts())
            {
                if (account == null)
                    continue;

                for (int i = 0; i < account.Length; ++i)
                {
                    Mobile m = account[i];

                    if (m == null || m.Deleted || m.Name == null)
                        continue;

                    if (m.Name.ToLower().IndexOf(lowerSearch) < 0)
                        continue;

                    bool online = (m.NetState != null);

                    string line = String.Format(
                        "Account: {0}, Character: {1}, Slot: {2}, Status: {3}",
                        account.Username,
                        m.Name,
                        i,
                        online ? "Online" : "Offline");

                    matches.Add(line);

                    if (!matchedAccounts.Contains(account))
                        matchedAccounts.Add(account);
                }
            }

            if (matches.Count == 0)
            {
                e.Mobile.SendMessage("No characters matched '{0}'.", search);
                return;
            }

            e.Mobile.SendMessage("Found {0} matching character{1} for '{2}':", matches.Count, matches.Count == 1 ? String.Empty : "s", search);

            for (int i = 0; i < matches.Count; ++i)
                e.Mobile.SendMessage((string)matches[i]);

            if (matchedAccounts.Count == 1)
            {
                e.Mobile.SendGump(new AdminGump(e.Mobile, AdminGumpPage.AccountDetails_Information, 0, null, "One account matched.", matchedAccounts[0]));
            }
            else if (matchedAccounts.Count > 1)
            {
                e.Mobile.SendGump(new AdminGump(e.Mobile, AdminGumpPage.Accounts, 0, matchedAccounts, String.Format("Found {0} account matches.", matchedAccounts.Count), new ArrayList()));
            }
        }
    }
}
