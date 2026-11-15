using Server;
using Server.Commands;

namespace Server.Commands.Generic
{
	public class LOSBypassCommands
	{
		public static void Initialize()
		{
			TargetCommands.Register(new ToggleLOSBypassCommand());
			TargetCommands.Register(new ClearLOSBypassCommand());
			TargetCommands.Register(new ClearAllLOSBypassCommand());
		}
	}

	public class ToggleLOSBypassCommand : BaseCommand
	{
		public ToggleLOSBypassCommand()
		{
			AccessLevel = AccessLevel.GameMaster;
			Supports = CommandSupport.All;
			Commands = new string[] { "ToggleLOSBypass" };
			ObjectTypes = ObjectTypes.All;
			Usage = "ToggleLOSBypass";
			Description = "Toggles GLOBAL LOS bypass for the targeted object/tile. Supports [area and [m.";
		}

		public override void Execute(CommandEventArgs e, object obj)
		{
			if (LOSBypassOverrides.ToggleTarget(e.Mobile, obj))
				AddResponse("LOS bypass toggled.");
			else
				LogFailure("Unsupported target for LOS bypass.");
		}
	}

	public class ClearLOSBypassCommand : BaseCommand
	{
		public ClearLOSBypassCommand()
		{
			AccessLevel = AccessLevel.GameMaster;
			Supports = CommandSupport.All;
			Commands = new string[] { "ClearLOSBypass" };
			ObjectTypes = ObjectTypes.All;
			Usage = "ClearLOSBypass";
			Description = "Clears GLOBAL LOS bypass for targeted object/tile. Supports [area and [m.";
		}

		public override void Execute(CommandEventArgs e, object obj)
		{
			if (LOSBypassOverrides.ClearTarget(e.Mobile, obj))
				AddResponse("LOS bypass cleared for target.");
			else
				LogFailure("No matching LOS bypass entry for target.");
		}
	}

	public class ClearAllLOSBypassCommand : BaseCommand
	{
		public ClearAllLOSBypassCommand()
		{
			AccessLevel = AccessLevel.GameMaster;
			Supports = CommandSupport.Simple;
			Commands = new string[] { "ClearAllLOSBypass" };
			ObjectTypes = ObjectTypes.All;
			Usage = "ClearAllLOSBypass";
			Description = "Clears all GLOBAL LOS bypass entries.";
		}

		public override void Execute(CommandEventArgs e, object obj)
		{
			LOSBypassOverrides.ClearAll(e.Mobile);
			AddResponse("All LOS bypass overrides cleared.");
		}
	}
}
