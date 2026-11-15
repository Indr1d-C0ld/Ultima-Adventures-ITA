using System.Linq;
using Server.Items;
using Server.Mobiles;

namespace Server.Commands
{
    public class InvasionCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("KillInvasion", AccessLevel.Administrator, new CommandEventHandler(args => KillInvasion(args.Mobile)));
            CommandSystem.Register("ResetInvasion", AccessLevel.Administrator, new CommandEventHandler(ResetInvasion));
        }

        public static void KillInvasion(Mobile from)
        {
            foreach (var creature in World.Mobiles.Values.Where(x =>
                            x is DeepDweller
                            || x is FailedExperiment
                            || x is OphidianGeneral
                            || x is WanderingConcubine
                            || x is WidowsConcubine
                            || x is wOphidianArchmage
                            || x is wOphidianKnight
                            || x is wOphidianMage
                            || x is wOphidianMatriarch
                            || x is wOphidianWarrior
                            || x is Zombiex
                        ).ToList())
            {
                creature.Kill();
            }

            if (from != null)
            {
                from.SendMessage("All invasion monsters have been killed");
            }
        }

        public static void ResetInvasion(CommandEventArgs args)
        {
            var oldValue = AetherGlobe.invasionstage;
            AetherGlobe.invasionstage = 0;
            args.Mobile.SendMessage("Invasion has been reduced from stage {0} to stage 0.", oldValue);

            KillInvasion(args.Mobile);
        }
    }
}