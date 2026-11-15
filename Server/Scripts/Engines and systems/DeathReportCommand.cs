using Server;
using Server.Accounting;
using Server.Commands;
using Server.Mobiles;

namespace Server.Commands
{
	public class DeathReportCommand
	{
		public const string DeathReportTag = "DeathReportEnabled";

		public static void Initialize()
		{
			CommandSystem.Register( "DeathReport", AccessLevel.Player, new CommandEventHandler( ToggleDeathReport_OnCommand ) );
			CommandSystem.Register( "DR", AccessLevel.Player, new CommandEventHandler( ToggleDeathReport_OnCommand ) );
		}

		[Usage( "DeathReport" )]
		[Description( "Toggles the 5-second death report window." )]
		private static void ToggleDeathReport_OnCommand( CommandEventArgs e )
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if( pm == null )
				return;

			bool enabled = DeathReportSystem.Toggle( pm );
			Account acct = pm.Account as Account;

			if( acct != null )
				acct.SetTag( DeathReportTag, enabled ? "true" : "false" );

			pm.SendMessage( 1153, enabled ? "Death report is now ON." : "Death report is now OFF." );
		}
	}
}
