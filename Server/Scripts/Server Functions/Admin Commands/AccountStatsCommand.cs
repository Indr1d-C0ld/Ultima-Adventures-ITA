using System;
using Server;
using Server.Accounting;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Commands
{
	public class AccountStatsCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register("AccountStats", AccessLevel.GameMaster, new CommandEventHandler(OnCommand));
		}

		[Usage("AccountStats [accountName]")]
		[Description("Shows account and character statistics for the targeted player or specified account.")]
		private static void OnCommand(CommandEventArgs e)
		{
			Mobile from = e.Mobile;

			if (e.Length > 0)
			{
				string accountName = e.GetString(0);
				IAccount account = Accounts.GetAccount(accountName);
				if (account == null)
				{
					from.SendMessage("Account '{0}' was not found.", accountName);
					return;
				}

				ShowGump(from, account as Account);
				return;
			}

			from.Target = new AccountStatsTarget();
			from.SendMessage("Target a player to view account statistics.");
		}

		private static void ShowGump(Mobile from, Account account)
		{
			if (account == null)
			{
				from.SendMessage("Unable to resolve account information.");
				return;
			}

			from.CloseGump(typeof(AccountStatsGump));
			from.SendGump(new AccountStatsGump(from, account));
		}

		private class AccountStatsTarget : Target
		{
			public AccountStatsTarget() : base(12, false, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				Mobile mobile = targeted as Mobile;
				if (mobile == null)
				{
					from.SendMessage("You must target a player.");
					return;
				}

				Account account = mobile.Account as Account;
				if (account == null)
				{
					from.SendMessage("That target does not have a valid account.");
					return;
				}

				ShowGump(from, account);
			}
		}
	}
}
