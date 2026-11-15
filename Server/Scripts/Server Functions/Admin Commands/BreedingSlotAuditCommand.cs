using System;
using System.IO;
using Server.Mobiles;

namespace Server.Commands
{
    public static class BreedingSlotAuditCommand
    {
        private const string AuditLogPath = "Logs/BreedingSlotAudit.log";

        public static void Initialize()
        {
            CommandSystem.Register("AuditBreedingSlots", AccessLevel.Administrator, new CommandEventHandler(AuditBreedingSlots_OnCommand));
            CommandSystem.Register("RepairBreedingSlots", AccessLevel.Administrator, new CommandEventHandler(RepairBreedingSlots_OnCommand));
        }

        public static int GetExpectedControlSlots(BaseCreature creature)
        {
            if (creature == null)
                return 1;

            return creature.GetExpectedControlSlots();
        }

        [Usage("AuditBreedingSlots")]
        [Description("Audits control slots for all tamed, stabled, hitched, or breeding creatures without changing them.")]
        private static void AuditBreedingSlots_OnCommand(CommandEventArgs e)
        {
            RunAudit(e.Mobile, false);
        }

        [Usage("RepairBreedingSlots confirm")]
        [Description("Repairs control slots for all tamed, stabled, hitched, or breeding creatures after an audit.")]
        private static void RepairBreedingSlots_OnCommand(CommandEventArgs e)
        {
            if (e.Length != 1 || !Insensitive.Equals(e.GetString(0), "confirm"))
            {
                SendMessage(e.Mobile, "This changes saved pet data. Run AuditBreedingSlots first, then use RepairBreedingSlots confirm.");
                return;
            }

            RunAudit(e.Mobile, true);
        }

        private static void RunAudit(Mobile from, bool repair)
        {
            if (World.Loading || World.Saving)
            {
                SendMessage(from, "The breeding slot audit cannot run while the world is loading or saving.");
                return;
            }

            int inspected = 0;
            int mismatches = 0;
            int repaired = 0;
            int followerDelta = 0;
            int errors = 0;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(AuditLogPath));

                using (StreamWriter log = new StreamWriter(AuditLogPath, true))
                {
                    log.WriteLine("[{0:u}] {1} breeding slot audit started by {2}.", DateTime.UtcNow, repair ? "Repair" : "Read-only", GetSourceName(from));

                    foreach (Mobile mobile in World.Mobiles.Values)
                    {
                        try
                        {
                            BaseCreature creature = mobile as BaseCreature;
                            if (!IsAuditablePet(creature))
                                continue;

                            ++inspected;

                            int currentSlots = creature.ControlSlots;
                            int expectedSlots = GetExpectedControlSlots(creature);
                            if (currentSlots == expectedSlots)
                                continue;

                            ++mismatches;

                            Mobile owner = creature.ControlMaster;
                            int delta = expectedSlots - currentSlots;
                            log.WriteLine("{0} serial={1} type={2} name={3} current={4} expected={5} level={6} realLevel={7} status={8} owner={9}",
                                repair ? "REPAIR" : "MISMATCH",
                                creature.Serial,
                                creature.GetType().FullName,
                                GetCreatureName(creature),
                                currentSlots,
                                expectedSlots,
                                creature.Level,
                                creature.RealLevel,
                                GetPetStatus(creature),
                                GetOwnerName(owner));

                            if (!repair)
                                continue;

                            followerDelta += ApplyExpectedControlSlots(creature, expectedSlots);
                            ++repaired;
                        }
                        catch (Exception ex)
                        {
                            ++errors;
                            log.WriteLine("ERROR serial={0} message={1}", mobile == null ? Serial.Zero : mobile.Serial, ex.Message);
                        }
                    }

                    log.WriteLine("[{0:u}] inspected={1} mismatches={2} repaired={3} errors={4} followerDelta={5}.", DateTime.UtcNow, inspected, mismatches, repaired, errors, followerDelta);
                    log.WriteLine();
                }
            }
            catch (Exception ex)
            {
                SendMessage(from, "The breeding slot audit could not write its log: {0}", ex.Message);
                Console.WriteLine("Breeding slot audit failed: {0}", ex);
                return;
            }

            Console.WriteLine("Breeding slot audit: inspected={0}, mismatches={1}, repaired={2}, errors={3}, followerDelta={4}.", inspected, mismatches, repaired, errors, followerDelta);
            SendMessage(from, "Breeding slot audit complete: inspected {0}, mismatches {1}, repaired {2}, errors {3}.", inspected, mismatches, repaired, errors);
            if (repair)
                SendMessage(from, "Controlled-pet follower total adjusted by {0}. Save the world after reviewing the log.", followerDelta);
            else if (mismatches > 0)
                SendMessage(from, "Review {0} before running RepairBreedingSlots confirm.", AuditLogPath);
        }

        private static int ApplyExpectedControlSlots(BaseCreature creature, int expectedSlots)
        {
            if (creature == null || creature.Deleted || expectedSlots < 1 || creature.ControlSlots == expectedSlots)
                return 0;

            int followerDelta = expectedSlots - creature.ControlSlots;
            Mobile owner = creature.ControlMaster;

            if (owner != null && !owner.Deleted)
            {
                int followers = owner.Followers + followerDelta;
                owner.Followers = followers < 0 ? 0 : followers;
            }

            creature.ControlSlots = expectedSlots;
            creature.InvalidateProperties();
            return owner == null || owner.Deleted ? 0 : followerDelta;
        }

        private static bool IsAuditablePet(BaseCreature creature)
        {
            if (creature == null || creature.Deleted || creature.Summoned || !creature.Tamable)
                return false;

            return creature.Controlled || creature.ControlMaster != null || creature.IsStabled || creature.IsHitchStabled || creature.Breeding || creature.LastOwner != null;
        }

        private static string GetPetStatus(BaseCreature creature)
        {
            string status = String.Empty;

            if (creature.Controlled || creature.ControlMaster != null)
                status = "controlled";
            if (creature.IsStabled)
                status = AppendStatus(status, "stabled");
            if (creature.IsHitchStabled)
                status = AppendStatus(status, "hitching-post");
            if (creature.Breeding)
                status = AppendStatus(status, "breeding");

            return status.Length == 0 ? "tamed" : status;
        }

        private static string AppendStatus(string current, string value)
        {
            return current.Length == 0 ? value : current + "," + value;
        }

        private static string GetCreatureName(BaseCreature creature)
        {
            return String.IsNullOrEmpty(creature.Name) ? creature.GetType().Name : creature.Name;
        }

        private static string GetOwnerName(Mobile owner)
        {
            if (owner == null)
                return "(none)";

            return String.IsNullOrEmpty(owner.Name) ? owner.GetType().Name : owner.Name;
        }

        private static string GetSourceName(Mobile from)
        {
            return from == null ? "console" : GetOwnerName(from);
        }

        private static void SendMessage(Mobile from, string format, params object[] args)
        {
            if (from != null)
                from.SendMessage(format, args);
            else
                Console.WriteLine(format, args);
        }
    }
}
