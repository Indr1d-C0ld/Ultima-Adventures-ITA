using System; 
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Misc; 
using Server.Items; 
using Server.Mobiles;
using Server.Network;
using Server.Gumps;
using Server.Targeting;

namespace Server.Mobiles 
{ 
	public class BaseRed : BaseCreature 
	{ 

		private bool m_Bandaging;

		private bool m_Threwpot;

                private bool m_attacking;
                private bool m_revealing;

                private bool m_roleplayEnabled;
                private bool m_roleplayActive;
                private PlayerMobile m_roleplayTarget;
                private DemandCategory m_demandCategory;
                private int m_demandAmount;
                private int m_demandCollected;
                private DateTime m_roleplayDeadline;
                private DateTime m_nextCountdown;
                private int m_nextCountdownValue;
                private FightMode m_savedFightMode;
                private bool m_hasSavedFightMode;
                private ActionType m_savedAIAction;
                private bool m_hasSavedAIAction;
                private readonly List<BaseRed> m_backupAllies;
                private bool m_backupActive;
                private BaseRed m_backupLeader;
                private DateTime m_nextJeer;
                private DateTime m_nextSuppressionPulse;
                private bool m_backupStoredRoleplayEnabled;
                private bool m_hasBackupStoredRoleplayEnabled;
                private int m_backupSlotIndex;
                private bool m_recalling;
                private bool m_movementLocked;
                private bool m_roleplayDisallowAllMoves;
                private int m_roleplayDamageTaken;
                private bool m_courtesyPending;
                private PlayerMobile m_courtesyVictim;
                private DateTime m_courtesyResponseDeadline;
                private bool m_courtesyStoredCanHearGhosts;
                private bool m_hasCourtesyStoredCanHearGhosts;
                private ISpawner m_roleplayPackSpawner;

                private static readonly string[] BackupTaunts = new string[]
                {
                        "Run for it, I dare ya!",
                        "Go on, see how far you get!",
                        "Hope you've got a plan 'cause we sure do!",
                        "This is gonna be fun if you bolt!",
                        "Keep stalling, we love a good chase!"
                };

                private static readonly string[] FarewellShouts = new string[]
                {
                        "See ya around!",
                        "Until next time!",
                        "We'll be seeing you!",
                        "Catch you later!",
                        "Until we cross paths again!"
                };

                private static readonly string[] RoleplayBreakTaunts = new string[]
                {
                        "So it's a fight you want, {0}?",
                        "So that's how it's gonna be, {0}?",
                        "Alright then, {0}, let's do this the hard way!",
                        "Guess talking's over, {0}!",
                        "You sure about this, {0}?"
                };

                private static readonly string[] CourtesyAffirmatives = new string[]
                {
                        "yes",
                        "y",
                        "yeah",
                        "yup",
                        "yep",
                        "sure",
                        "ok",
                        "okay",
                        "please",
                        "affirmative",
                        "aye",
                        "yay"
                };

                private static readonly string[] CourtesyNegatives = new string[]
                {
                        "no",
                        "n",
                        "nope",
                        "nah",
                        "negative",
                        "nay"
                };

                private static readonly char[] CourtesyResponseSeparators = new char[]
                {
                        ' ', '.', '!', '?', ',', ';', ':'
                };

                private static readonly TimeSpan CourtesyResponseTimeout = TimeSpan.FromSeconds(30.0);
                private const double CourtesyResurrectionChance = 0.30;

                private const double RoleplayEncounterChance = 0.30;
                private const int RoleplayDamageBreakThreshold = 75;

                private sealed class ExtortionPackState
                {
                        public int ActiveCount;
                        public bool LastRollResult;
                        public readonly List<BaseRed> Members = new List<BaseRed>();
                }

                private static readonly Dictionary<ISpawner, ExtortionPackState> s_extortionPackStates = new Dictionary<ISpawner, ExtortionPackState>();
                private static readonly object s_extortionPackSync = new object();
                private static readonly Dictionary<PlayerMobile, BaseRed> s_activeExtortions = new Dictionary<PlayerMobile, BaseRed>();
                private static readonly object s_activeExtortionSync = new object();
                private static readonly TimeSpan ExtortionSuppressionPulseInterval = TimeSpan.FromSeconds(0.1);

                private static readonly Point2D[] BackupFormationOffsets = new Point2D[]
                {
                        new Point2D(-6, 0),
                        new Point2D(6, 0),
                        new Point2D(0, -6),
                        new Point2D(0, 6),
                        new Point2D(-6, -4),
                        new Point2D(6, 4),
                        new Point2D(-4, 6),
                        new Point2D(4, -6),
                        new Point2D(-8, 2),
                        new Point2D(8, -2),
                        new Point2D(-4, -8),
                        new Point2D(4, 8)
                };

                private const int RoleplayRangeBuffer = 30;
                private const int ExtortionSuppressionRadius = 30;
                public BaseRed(AIType ai, FightMode fm, int PR, int FR, double AS, double PS) : base( ai, fm, PR, FR, AS, PS )
                {
                        SpeechHue = Utility.RandomDyedHue();
			Hue = Utility.RandomSkinHue();
			RangePerception = BaseCreature.DefaultRangePerception;
			Criminal = true;
			AIFullSpeedActive = AIFullSpeedPassive = true; // Force full speed
			m_revealing = false;
			m_teleporting = false;
                        m_Threwpot = false;
                        m_freezing = false;
                        m_attacking = false;
                        m_roleplayEnabled = false;
                        m_roleplayActive = false;
                        m_hasSavedFightMode = false;
                        m_hasSavedAIAction = false;
                        m_backupAllies = new List<BaseRed>();
                        m_backupActive = false;
                        m_backupLeader = null;
                        m_nextJeer = DateTime.MinValue;
                        m_nextSuppressionPulse = DateTime.MinValue;
                        m_backupStoredRoleplayEnabled = false;
                        m_hasBackupStoredRoleplayEnabled = false;
                        m_backupSlotIndex = -1;
                        m_recalling = false;
                        m_movementLocked = false;
                        m_roleplayDisallowAllMoves = false;
                        m_courtesyPending = false;
                        m_courtesyVictim = null;
                        m_courtesyResponseDeadline = DateTime.MinValue;
                        m_courtesyStoredCanHearGhosts = false;
                        m_hasCourtesyStoredCanHearGhosts = false;
                        m_roleplayPackSpawner = null;
                        m_roleplayDamageTaken = 0;
                }

                private enum DemandCategory
                {
                        Gold,
                        Reagents,
                        Ore,
                        Wood,
                        Bandages,
                        Potions
                }

                public override void OnAfterSpawn()
                {
                        base.OnAfterSpawn();

                        InitializeRoleplayChanceForSpawnPack();
                }

                public override void OnDelete()
                {
                        UnregisterActiveExtortionTarget(m_roleplayTarget);

                        ReleaseRoleplayPack();

                        base.OnDelete();
                }

                private void InitializeRoleplayChanceForSpawnPack()
                {
                        ReleaseRoleplayPack();

                        m_roleplayEnabled = false;

                        ISpawner spawner = Spawner;

                        if (spawner == null)
                        {
                                m_roleplayEnabled = RollRoleplayEncounterChance();
                                m_roleplayPackSpawner = null;
                                return;
                        }

                        m_roleplayPackSpawner = spawner;

                        lock (s_extortionPackSync)
                        {
                                ExtortionPackState state;

                                if (!s_extortionPackStates.TryGetValue(spawner, out state) || state == null)
                                {
                                        state = new ExtortionPackState();
                                        s_extortionPackStates[spawner] = state;
                                }

                                PurgeExtortionPackMembers(state);

                                if (!state.Members.Contains(this))
                                        state.Members.Add(this);

                                if (state.ActiveCount <= 0)
                                        state.LastRollResult = RollRoleplayEncounterChance();

                                state.ActiveCount++;
                                m_roleplayEnabled = state.LastRollResult;
                        }
                }

                private void ReleaseRoleplayPack()
                {
                        if (m_roleplayPackSpawner == null)
                                return;

                        lock (s_extortionPackSync)
                        {
                                ExtortionPackState state;

                                if (s_extortionPackStates.TryGetValue(m_roleplayPackSpawner, out state) && state != null)
                                {
                                        if (state.ActiveCount > 0)
                                                state.ActiveCount--;

                                        if (state.Members != null)
                                                state.Members.Remove(this);

                                        PurgeExtortionPackMembers(state);

                                        if (state.ActiveCount <= 0 && (state.Members == null || state.Members.Count == 0))
                                                s_extortionPackStates.Remove(m_roleplayPackSpawner);
                                }
                        }

                        m_roleplayPackSpawner = null;
                }

                private static bool RollRoleplayEncounterChance()
                {
                        return Utility.RandomDouble() < RoleplayEncounterChance;
                }

                public override bool HandlesOnSpeech(Mobile from)
                {
                        if (m_courtesyPending && from != null && from == m_courtesyVictim)
                                return true;

                        return base.HandlesOnSpeech(from);
                }

                public override bool DisallowAllMoves
                {
                        get
                        {
                                return base.DisallowAllMoves || m_roleplayDisallowAllMoves;
                        }
                }

                public override bool ReacquireOnMovement{ get { return true; } }
		public override TimeSpan ReacquireDelay{ get { return TimeSpan.FromSeconds( 1 ); } }
		public override bool BardImmune{ get{ return false; } }
		public override bool AlwaysMurderer{ get{ return true; } }

		public override bool IsEnemy( Mobile m )
		{

			Region reg = Region.Find( this.Location, this.Map );

			if ( reg.IsPartOf( "the Basement" ))
				return false;

			if (m is PlayerMobile && (m.Str + m.Int + m.Dex) < 125 && this.Map == Map.Trammel)
				return false;

		    if ( m is HarvesterEntity || m is BaseRed || m is Citizens || m is PlayerVendor || m is TownHerald || m is BaseVendor )
			return false;

		    if ( m is BaseCreature && ((BaseCreature)m).ControlMaster != null && m.Combatant != this)
		    {
				Mobile owner = ((BaseCreature)m).ControlMaster;
				if (owner is PlayerMobile && owner.Kills >= 2)
					return false;
		    } 
			
			//if ( (Server.Misc.Worlds.GetRegionName( m.Map, m.Location ) == "The Pit") && (m.Kills >= 5 && m.Criminal) )
			//	return true;
			
		    if ( m is PlayerMobile && m.Criminal || m.Kills >= 2 )
				return false;

		    return true;
		}

		public virtual bool HealsYoungPlayers{ get{ return false; } }

		public virtual bool CheckResurrect( Mobile m )
		{
			return true;
		}

		public DateTime m_NextResurrect;
		public static TimeSpan ResurrectDelay = TimeSpan.FromSeconds( 6.0 );

		public virtual void OfferResurrection( Mobile m )
		{
			
			if (m.Criminal || m.Kills >= 2)
			{
				Direction = GetDirectionTo( m );
				Say("An Corp");

				m.PlaySound(0x1F2);
				m.FixedEffect( 0x376A, 10, 16 );
				
				m.Resurrect();

			}
		}

		public virtual void OfferHeal( PlayerMobile m )
		{
			Direction = GetDirectionTo( m );

			//if ( m.CheckYoungHealTime() )
			if ( DateTime.UtcNow >= m_NextResurrect && m.Criminal && m.Kills >= 2 && !m.Hidden)
			{
				Say("Here's some help"); // You look like you need some healing my child.
				Say("In Vas Mani");

				m.PlaySound( 0x1F2 );
				m.FixedEffect( 0x376A, 9, 32 );

				m.Hits = (m.Hits + 50);
				m_NextResurrect = DateTime.UtcNow + ResurrectDelay;
			}
			else
			{
				Say("You should be good"); // I can do no more for you at this time.
			}

		}

                public override void OnMovement( Mobile m, Point3D oldLocation )
                {
                        PlayerMobile player = m as PlayerMobile;

                        int triggerRange = GetRoleplayTriggerRange();

                        if (m_roleplayEnabled && !m_roleplayActive && player != null && !player.Hidden && InRange(player, triggerRange) && !InRange(oldLocation, triggerRange))
                        {
                                TryStartRoleplay(player);
                        }

                        if ( !m.Frozen && DateTime.UtcNow >= m_NextResurrect && InRange( m, 4 ) && !InRange( oldLocation, 4 ) && InLOS( m ) )
                        {
                                if ( !m.Alive && (player != null && (player.Criminal || player.Kills >= 2)) )
                                {
                                        m_NextResurrect = DateTime.UtcNow + ResurrectDelay;

                                        if ( m.Map == null || !m.Map.CanFit( m.Location, 16, false, false ) )
                                        {
                                                m.SendLocalizedMessage( 502391 ); // Thou can not be resurrected there!
                                        }
                                        else if ( CheckResurrect( m ) )
                                        {
                                                OfferResurrection( m );
                                        }
                                }
                                else if ( player != null && m.Hits < m.HitsMax && (player.Criminal || player.Kills >= 5) )
                                {
                                        OfferHeal( player );
                                }
                        }
                }

                public override bool OnDragDrop( Mobile from, Item dropped )
                {
                        if (m_roleplayEnabled && m_roleplayActive && from == m_roleplayTarget && dropped != null)
                        {
                                if (!IsDemandItem(dropped))
                                {
                                        Say("That's not what I asked for!");
                                        return false;
                                }

                                int needed = m_demandAmount - m_demandCollected;

                                if (needed <= 0)
                                {
                                        Say("I've already taken what I wanted from you.");
                                        return false;
                                }

                                int offered = GetItemUnitCount(dropped);

                                if (offered <= 0)
                                {
                                        return false;
                                }

                                int taken = Math.Min(needed, offered);
                                if (taken <= 0)
                                {
                                        if (!from.AddToBackpack(dropped))
                                                dropped.MoveToWorld(from.Location, from.Map);
                                        return false;
                                }
                                string demandLabel = GetDemandLabel(m_demandCategory);

                                HandleDemandTransfer(from, dropped, offered, taken);

                                m_demandCollected += taken;

                                from.SendMessage(string.Format("You hand over {0} {1}.", taken, demandLabel));
                                Say(string.Format("Good. I've received {0} {1}.", m_demandCollected, demandLabel));

                                if (m_demandCollected >= m_demandAmount)
                                {
                                        FinishRoleplaySuccess();
                                }
                                else
                                {
                                        int remaining = m_demandAmount - m_demandCollected;
                                        Say(string.Format("I still need {0} more {1}, {2}.", remaining, demandLabel, m_roleplayTarget.Name));
                                }

                                return true;
                        }

                        return base.OnDragDrop(from, dropped);
                }

                private int GetItemUnitCount(Item item)
                {
                        if (item == null)
                                return 0;

                        return Math.Max(1, item.Amount);
                }

                private void HandleDemandTransfer(Mobile from, Item dropped, int offered, int taken)
                {
                        if (dropped == null || from == null)
                                return;

                        int remainder = offered - taken;

                        if (remainder <= 0)
                        {
                                dropped.Delete();
                                return;
                        }

                        if (dropped.Stackable)
                        {
                                dropped.Amount = remainder;
                                if (!from.AddToBackpack(dropped))
                                        dropped.MoveToWorld(from.Location, from.Map);
                                from.SendMessage(string.Format("The murderer only takes {0} {1} and lets you keep the rest.", taken, GetDemandLabel(m_demandCategory)));
                        }
                        else
                        {
                                // Non-stackable items should only be taken whole, but guard against odd cases.
                                if (!from.AddToBackpack(dropped))
                                        dropped.MoveToWorld(from.Location, from.Map);
                        }
                }

                private bool InRoleplayScene()
                {
                        return m_roleplayActive || m_backupActive;
                }

                public bool IsRacketeering
                {
                        get { return InRoleplayScene(); }
                }

                public void CancelRacketeeringFromGuard()
                {
                        if (!IsRacketeering)
                                return;

                        BaseRed leader = m_backupActive ? m_backupLeader : this;

                        if (leader != null && leader != this && leader.IsRacketeering)
                        {
                                leader.EndRoleplay(false);
                        }
                        else if (m_roleplayActive)
                        {
                                EndRoleplay(false);
                        }
                        else if (m_backupActive)
                        {
                                LeaveBackupRole(true);
                        }
                }

                private void ProcessRoleplayThink()
                {
                        if (m_courtesyPending)
                        {
                                MaintainGuardingPosture();
                                return;
                        }

                        if (m_backupActive)
                        {
                                ProcessBackupSupport();
                                return;
                        }

                        if (!m_roleplayEnabled || !m_roleplayActive)
                                return;

                        if (m_roleplayTarget == null || m_roleplayTarget.Deleted || !m_roleplayTarget.Alive)
                        {
                                EndRoleplay(false);
                                return;
                        }

                        if (!InRange(m_roleplayTarget, 12))
                        {
                                Say("Running only makes it worse!");
                                EndRoleplay(true);
                                return;
                        }

                        Combatant = null;
                        FocusMob = null;
                        Warmode = false;

                        MaintainRoleplayPosition();

                        if (DateTime.UtcNow >= m_roleplayDeadline)
                        {
                                Say("Time's up! You should have paid.");
                                EndRoleplay(true);
                                return;
                        }

                        if (m_nextCountdownValue > 0 && DateTime.UtcNow >= m_nextCountdown)
                        {
                                Say(string.Format("{0}, you have {1} seconds left to hand over the {2}!", m_roleplayTarget.Name, m_nextCountdownValue, GetDemandLabel(m_demandCategory)));
                                m_nextCountdownValue -= 10;
                                m_nextCountdown = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);
                        }
                }

                private void ProcessCourtesyThink()
                {
                        if (!m_courtesyPending)
                                return;

                        PlayerMobile victim = m_courtesyVictim;

                        if (victim == null || victim.Deleted)
                        {
                                HandleCourtesyDecline(null, true);
                                return;
                        }

                        MaintainGuardingPosture();
                        RemoveAggressionFor(victim);

                        Direction = GetDirectionTo(victim);

                        if (victim.NetState == null)
                        {
                                HandleCourtesyDecline(null, true);
                                return;
                        }

                        if (victim.Map != Map)
                        {
                                HandleCourtesyDecline(null, true);
                                return;
                        }

                        if (victim.Alive)
                        {
                                HandleCourtesyAlreadyAlive();
                                return;
                        }

                        if (DateTime.UtcNow >= m_courtesyResponseDeadline)
                                HandleCourtesyDecline(null, true);
                }

                private void HandleCourtesyAcceptance()
                {
                        if (!m_courtesyPending)
                                return;

                        PlayerMobile victim = m_courtesyVictim;

                        ClearCourtesyState();

                        MaintainGuardingPosture();

                        RemoveAggressionFor(victim);

                        if (victim == null || victim.Deleted)
                        {
                                Timer.DelayCall(TimeSpan.FromSeconds(1.0), () =>
                                {
                                        if (!Deleted)
                                                RecallNearbyReds(true);
                                });

                                return;
                        }

                        Say("Alright, let's get you back on your feet.");

                        if (victim != null && victim.NetState != null)
                                victim.SendMessage(0x35, string.Format("{0} prepares to resurrect you. Hold tight!", Name));

                        Timer.DelayCall(TimeSpan.FromSeconds(2.0), () =>
                        {
                                if (Deleted || !Alive || Map == null)
                                        return;

                                if (victim == null || victim.Deleted)
                                        return;

                                if (victim.NetState == null)
                                {
                                        RecallNearbyReds(true);
                                        return;
                                }

                                Direction = GetDirectionTo(victim);
                                Say("An Corp");

                                victim.PlaySound(0x1F2);
                                victim.FixedEffect(0x376A, 10, 16);

                                if (!victim.Alive)
                                        victim.Resurrect();

                                string farewell = GetRandomFarewellShout();
                                RecallNearbyReds(true, farewell);
                        });
                }

                private void HandleCourtesyDecline(string redSpeech, bool dueToTimeout)
                {
                        if (!m_courtesyPending)
                                return;

                        PlayerMobile victim = m_courtesyVictim;

                        ClearCourtesyState();

                        MaintainGuardingPosture();

                        RemoveAggressionFor(victim);

                        if (!string.IsNullOrEmpty(redSpeech))
                                Say(redSpeech);
                        else if (dueToTimeout)
                                Say("Guess that's a no, then.");

                        if (victim != null && !victim.Deleted && victim.NetState != null)
                        {
                                if (dueToTimeout)
                                        victim.SendMessage(0x22, string.Format("{0} takes your silence as a refusal.", Name));
                                else
                                        victim.SendMessage(0x22, string.Format("You decline {0}'s offer to resurrect you.", Name));
                        }

                        Timer.DelayCall(TimeSpan.FromSeconds(1.0), () =>
                        {
                                if (!Deleted)
                                        RecallNearbyReds(true);
                        });
                }

                private void HandleCourtesyAlreadyAlive()
                {
                        if (!m_courtesyPending)
                                return;

                        PlayerMobile victim = m_courtesyVictim;

                        ClearCourtesyState();

                        MaintainGuardingPosture();

                        RemoveAggressionFor(victim);

                        Say("Looks like you're already back up. We'll catch you later.");

                        if (victim != null && !victim.Deleted && victim.NetState != null)
                                victim.SendMessage(0x35, string.Format("{0} notices you're already alive and leaves you be.", Name));

                        Timer.DelayCall(TimeSpan.FromSeconds(1.0), () =>
                        {
                                if (!Deleted)
                                        RecallNearbyReds(true);
                        });
                }

                private void ClearCourtesyState()
                {
                        m_courtesyPending = false;
                        m_courtesyVictim = null;
                        m_courtesyResponseDeadline = DateTime.MinValue;

                        if (m_hasCourtesyStoredCanHearGhosts)
                        {
                                CanHearGhosts = m_courtesyStoredCanHearGhosts;
                                m_hasCourtesyStoredCanHearGhosts = false;
                        }
                }

                private bool IsCourtesyAffirmativeResponse(string speech)
                {
                        return MatchesCourtesyKeyword(speech, CourtesyAffirmatives);
                }

                private bool IsCourtesyNegativeResponse(string speech)
                {
                        return MatchesCourtesyKeyword(speech, CourtesyNegatives);
                }

                private bool MatchesCourtesyKeyword(string speech, string[] keywords)
                {
                        if (string.IsNullOrEmpty(speech) || keywords == null || keywords.Length == 0)
                                return false;

                        string trimmed = speech.Trim();

                        if (MatchesCourtesyCandidate(trimmed, keywords))
                                return true;

                        string normalized = NormalizeCourtesyToken(trimmed);

                        if (MatchesCourtesyCandidate(normalized, keywords))
                                return true;

                        string[] parts = trimmed.Split(CourtesyResponseSeparators, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < parts.Length; ++i)
                        {
                                string part = parts[i];

                                if (MatchesCourtesyCandidate(part, keywords))
                                        return true;

                                string normalizedPart = NormalizeCourtesyToken(part);

                                if (MatchesCourtesyCandidate(normalizedPart, keywords))
                                        return true;
                        }

                        return false;
                }

                private static bool MatchesCourtesyCandidate(string candidate, string[] keywords)
                {
                        if (string.IsNullOrEmpty(candidate) || keywords == null)
                                return false;

                        for (int i = 0; i < keywords.Length; ++i)
                        {
                                if (Insensitive.Equals(candidate, keywords[i]))
                                        return true;
                        }

                        return false;
                }

                private static string NormalizeCourtesyToken(string token)
                {
                        if (string.IsNullOrEmpty(token))
                                return token;

                        int start = 0;
                        int end = token.Length - 1;

                        while (start <= end && !char.IsLetterOrDigit(token[start]))
                                start++;

                        while (end >= start && !char.IsLetterOrDigit(token[end]))
                                end--;

                        if (start > end)
                                return string.Empty;

                        string slice = token.Substring(start, end - start + 1);

                        return string.IsNullOrEmpty(slice) ? slice : slice.ToLowerInvariant();
                }

                private void ProcessBackupSupport()
                {
                        if (!m_backupActive)
                                return;

                        BaseRed leader = m_backupLeader;
                        PlayerMobile target = (leader != null) ? leader.m_roleplayTarget : null;

                        if (leader == null || leader.Deleted || !leader.Alive || target == null || target.Deleted || !target.Alive || !leader.m_roleplayActive)
                        {
                                LeaveBackupRole(true);
                                return;
                        }

                        m_roleplayTarget = target;

                        Combatant = null;
                        FocusMob = null;
                        Warmode = false;

                        MaintainBackupPosition(target, leader);

                        if (DateTime.UtcNow >= m_nextJeer)
                        {
                                Say(BackupTaunts[Utility.Random(BackupTaunts.Length)]);
                                m_nextJeer = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(12, 20));
                        }
                }

                private void MaintainRoleplayPosition()
                {
                        if (m_roleplayTarget == null || Map == null)
                                return;

                        bool adjacent = InRange(m_roleplayTarget, 1);

                        if (!adjacent)
                        {
                                UnlockMovement();
                                StepToward(m_roleplayTarget.Location);
                                return;
                        }

                        Direction = GetDirectionTo(m_roleplayTarget);

                        Point3D preferred = GetPreferredStandLocation(m_roleplayTarget);

                        if (preferred == Point3D.Zero || preferred == m_roleplayTarget.Location)
                        {
                                LockMovement();
                                if (AIObject != null && m_movementLocked)
                                        AIObject.NextMove = DateTime.MaxValue;
                                return;
                        }

                        if (!Map.CanSpawnMobile(preferred.X, preferred.Y, preferred.Z))
                        {
                                LockMovement();
                                if (AIObject != null && m_movementLocked)
                                        AIObject.NextMove = DateTime.MaxValue;
                                return;
                        }

                        if (Location != preferred)
                        {
                                if (InRange(preferred, 1))
                                {
                                        LockMovement();
                                        if (AIObject != null && m_movementLocked)
                                                AIObject.NextMove = DateTime.MaxValue;
                                        return;
                                }

                                UnlockMovement();
                                StepToward(preferred);
                                return;
                        }

                        LockMovement();
                        if (AIObject != null && m_movementLocked)
                                AIObject.NextMove = DateTime.MaxValue;
                }

                private void MaintainBackupPosition(PlayerMobile target, BaseRed leader)
                {
                        if (target == null)
                                return;

                        Point3D desired = GetBackupStandLocation(target, leader);

                        if (desired != Point3D.Zero)
                        {
                                if (Location == desired)
                                {
                                        LockMovement();
                                }
                                else if (InRange(desired, 1))
                                {
                                        LockMovement();
                                        Direction = GetDirectionTo(target);
                                        if (AIObject != null && m_movementLocked)
                                                AIObject.NextMove = DateTime.MaxValue;
                                        return;
                                }
                                else
                                {
                                        UnlockMovement();
                                        StepToward(desired);
                                        Direction = GetDirectionTo(target);
                                        return;
                                }
                        }
                        else if (!InRange(target, 3))
                        {
                                UnlockMovement();
                                StepToward(target.Location);
                                Direction = GetDirectionTo(target);
                                return;
                        }
                        else
                        {
                                LockMovement();
                        }

                        if (AIObject != null && m_movementLocked)
                                AIObject.NextMove = DateTime.MaxValue;

                        Direction = GetDirectionTo(target);
                }

                private Point3D GetBackupStandLocation(PlayerMobile target, BaseRed leader)
                {
                        if (target == null || Map == null)
                                return Point3D.Zero;

                        int slot = m_backupSlotIndex;

                        if (slot < 0)
                                slot = 0;

                        Map map = Map;

                        for (int i = 0; i < BackupFormationOffsets.Length; ++i)
                        {
                                Point2D offset = BackupFormationOffsets[(slot + i) % BackupFormationOffsets.Length];
                                Point3D candidate = new Point3D(target.X + offset.X, target.Y + offset.Y, target.Z);

                                int averageZ = map.GetAverageZ(candidate.X, candidate.Y);
                                candidate = new Point3D(candidate.X, candidate.Y, averageZ);

                                if (!map.CanSpawnMobile(candidate.X, candidate.Y, candidate.Z))
                                        continue;

                                if (leader != null && leader.Location == candidate)
                                        continue;

                                if (leader != null)
                                {
                                        bool occupied = false;

                                        for (int j = 0; j < leader.m_backupAllies.Count; ++j)
                                        {
                                                BaseRed ally = leader.m_backupAllies[j];

                                                if (ally == null || ally == this || ally.Deleted)
                                                        continue;

                                                if (ally.Location == candidate)
                                                {
                                                        occupied = true;
                                                        break;
                                                }
                                        }

                                        if (occupied)
                                                continue;
                                }

                                return candidate;
                        }

                        return Point3D.Zero;
                }

                private void StepToward(Point3D goal)
                {
                        if (goal == Point3D.Zero)
                                return;

                        if (Deleted || CantWalk || Paralyzed || Frozen || DisallowAllMoves)
                                return;

                        Direction direction = GetDirectionTo(goal);

                        if (Direction != direction)
                                Direction = direction;

                        Move(direction);
                }

                private void LockMovement()
                {
                        if (m_movementLocked)
                                return;

                        m_movementLocked = true;
                        CantWalk = true;
                        m_roleplayDisallowAllMoves = true;

                        if (AIObject != null)
                                AIObject.NextMove = DateTime.MaxValue;
                }

                private void UnlockMovement()
                {
                        if (!m_movementLocked && !CantWalk && !m_roleplayDisallowAllMoves)
                                return;

                        m_movementLocked = false;
                        CantWalk = false;
                        m_roleplayDisallowAllMoves = false;

                        if (AIObject != null)
                                AIObject.NextMove = DateTime.UtcNow + TimeSpan.FromMilliseconds(250);
                }

                private int GetRoleplayTriggerRange()
                {
                        int perception = RangePerception;

                        if (perception <= 0)
                                perception = BaseCreature.DefaultRangePerception;

                        return Math.Max(perception + RoleplayRangeBuffer, 8);
                }

                private Point3D GetPreferredStandLocation(Mobile target)
                {
                        if (target == null)
                                return Point3D.Zero;

                        Direction facing = (Direction)((int)target.Direction & (int)Direction.Mask);

                        int offsetX = 0;
                        int offsetY = 0;

                        switch (facing)
                        {
                                case Direction.North:
                                        offsetX = 0;
                                        offsetY = -1;
                                        break;
                                case Direction.Right:
                                        offsetX = 1;
                                        offsetY = -1;
                                        break;
                                case Direction.East:
                                        offsetX = 1;
                                        offsetY = 0;
                                        break;
                                case Direction.Down:
                                        offsetX = 1;
                                        offsetY = 1;
                                        break;
                                case Direction.South:
                                        offsetX = 0;
                                        offsetY = 1;
                                        break;
                                case Direction.Left:
                                        offsetX = -1;
                                        offsetY = 1;
                                        break;
                                case Direction.West:
                                        offsetX = -1;
                                        offsetY = 0;
                                        break;
                                case Direction.Up:
                                        offsetX = -1;
                                        offsetY = -1;
                                        break;
                        }

                        Point3D preferred = new Point3D(target.X + offsetX, target.Y + offsetY, target.Z);

                        if (Map != null)
                        {
                                int averageZ = Map.GetAverageZ(preferred.X, preferred.Y);
                                preferred = new Point3D(preferred.X, preferred.Y, averageZ);
                        }

                        return preferred;
                }

                private void TryStartRoleplay(PlayerMobile player)
                {
                        if (player == null || player.Backpack == null)
                        {
                                m_roleplayEnabled = false;
                                return;
                        }

                        DemandCategory category;
                        int available;

                        if (!TryGetDemand(player.Backpack, out category, out available))
                        {
                                m_roleplayEnabled = false;
                                return;
                        }

                        SuppressCombatStance();
                        UnlockMovement();

                        m_roleplayActive = true;
                        m_nextSuppressionPulse = DateTime.MinValue;
                        m_roleplayTarget = player;
                        RegisterActiveExtortionTarget(player);
                        RemoveAggressionFor(player);
                        m_demandCategory = category;
                        m_demandCollected = 0;
                        m_demandAmount = DetermineDemandAmount(category, available);
                        m_roleplayDeadline = DateTime.UtcNow + TimeSpan.FromMinutes(1.0);
                        m_nextCountdown = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);
                        m_nextCountdownValue = 50;
                        m_roleplayDamageTaken = 0;

                        ClearAggressionRecords();

                        Say(string.Format("Hold it right there, {0}! Hand over {1} {2} and I might let you walk away.", player.Name, m_demandAmount, GetDemandLabel(category)));
                        player.SendMessage(string.Format("{0} wants {1} {2}. Hand them to the murderer before time runs out!", Name, m_demandAmount, GetDemandLabel(category)));

                        RallyBackupAllies();

                        MaintainRoleplayPosition();
                }

                private void ProcessExtortionSuppression()
                {
                        if (m_roleplayActive || m_backupActive)
                                return;

                        PlayerMobile target = Combatant as PlayerMobile;

                        if (target == null)
                                target = FocusMob as PlayerMobile;

                        if (target == null)
                                return;

                        BaseRed leader;

                        if (!TryGetActiveExtortionLeader(target, out leader) || leader == this)
                                return;

                        RemoveAggressionFor(target);

                        bool alreadyBackup = m_backupActive && m_backupLeader == leader;

                        if (alreadyBackup)
                                return;

                        if (CanJoinAsBackup(leader))
                        {
                                if (leader.m_backupAllies != null && !leader.m_backupAllies.Contains(this))
                                {
                                        int slotIndex = leader.m_backupAllies.Count;
                                        JoinAsBackup(leader, target, slotIndex);
                                        leader.m_backupAllies.Add(this);
                                        return;
                                }
                        }

                        SuppressCombatStance();
                }

                private void PulseExtortionSuppression()
                {
                        if (!m_roleplayActive)
                                return;

                        PlayerMobile target = m_roleplayTarget;

                        if (target == null || target.Deleted || !target.Alive)
                                return;

                        DateTime now = DateTime.UtcNow;

                        if (now < m_nextSuppressionPulse)
                                return;

                        m_nextSuppressionPulse = now + ExtortionSuppressionPulseInterval;

                        ClearAggressionRecords();
                        Warmode = false;
                        RemoveAggressionFor(target);

                        if (Combatant == target)
                                Combatant = null;

                        if (FocusMob == target)
                                FocusMob = null;

                        m_attacking = false;

                        HashSet<BaseRed> allies = CollectBackupCandidates();

                        if (allies == null || allies.Count == 0)
                                return;

                        foreach (BaseRed ally in allies)
                        {
                                if (ally == null || ally.Deleted)
                                        continue;

                                ally.ApplyExtortionSuppressionPulse(target, this);
                        }
                }

                private void ApplyExtortionSuppressionPulse(PlayerMobile target, BaseRed leader, bool allowBackupEnrollment = true)
                {
                        if (target == null)
                                return;

                        ClearAggressionRecords();
                        Warmode = false;

                        RemoveAggressionFor(target);

                        if (Combatant == target)
                                Combatant = null;

                        if (FocusMob == target)
                                FocusMob = null;

                        m_attacking = false;

                        if (!m_roleplayActive)
                        {
                                SuppressCombatStance();
                        }
                        else if (AIObject != null)
                        {
                                AIObject.Action = ActionType.Guard;
                                AIObject.NextMove = DateTime.UtcNow + TimeSpan.FromSeconds(1.0);
                        }

                        if (!allowBackupEnrollment)
                                return;

                        if (leader == null || leader == this || !leader.m_roleplayActive)
                                return;

                        if (m_backupActive && m_backupLeader == leader)
                                return;

                        if (!CanJoinAsBackup(leader))
                                return;

                        if (leader.m_backupAllies == null || leader.m_backupAllies.Contains(this))
                                return;

                        int slotIndex = leader.m_backupAllies.Count;

                        JoinAsBackup(leader, target, slotIndex);
                        leader.m_backupAllies.Add(this);
                }

                private void RallyBackupAllies()
                {
                        if (Map == null || m_roleplayTarget == null)
                                return;

                        ReleaseBackupAllies(true);

                        Say("You lot, stand down and watch my back!");

                        m_backupAllies.Clear();

                        HashSet<BaseRed> candidates = CollectBackupCandidates();
                        BroadcastImmediateExtortionSuppression(candidates, m_roleplayTarget);

                        List<BaseRed> orderedCandidates = new List<BaseRed>(candidates);

                        orderedCandidates.Sort(CompareBackupCandidatesByDistance);

                        foreach (BaseRed ally in orderedCandidates)
                        {
                                if (ally == null || ally == this || ally.Deleted || !ally.Alive)
                                        continue;

                                if (!ally.CanJoinAsBackup(this))
                                        continue;

                                ally.JoinAsBackup(this, m_roleplayTarget, m_backupAllies.Count);
                                m_backupAllies.Add(ally);
                        }
                }

                private HashSet<BaseRed> CollectBackupCandidates()
                {
                        HashSet<BaseRed> candidates = new HashSet<BaseRed>();

                        CollectNearbyBackupCandidates(candidates);
                        CollectPackBackupCandidates(candidates);

                        candidates.Add(this);

                        return candidates;
                }

                private void CollectNearbyBackupCandidates(HashSet<BaseRed> candidates)
                {
                        if (candidates == null)
                                return;

                        int callRange = Math.Max(GetRoleplayTriggerRange() + 2, ExtortionSuppressionRadius);

                        IPooledEnumerable eable = GetMobilesInRange(callRange);

                        if (eable != null)
                        {
                                foreach (Mobile mob in eable)
                                {
                                        if (mob == null)
                                                continue;

                                        BaseRed ally = mob as BaseRed;

                                        if (ally == null)
                                                continue;

                                        candidates.Add(ally);
                                }

                                eable.Free();
                        }
                }

                private void CollectPackBackupCandidates(HashSet<BaseRed> candidates)
                {
                        if (candidates == null)
                                return;

                        ISpawner spawner = m_roleplayPackSpawner;

                        if (spawner == null)
                                return;

                        lock (s_extortionPackSync)
                        {
                                ExtortionPackState state;

                                if (!s_extortionPackStates.TryGetValue(spawner, out state) || state == null)
                                        return;

                                PurgeExtortionPackMembers(state);

                                if (state.Members == null)
                                        return;

                                for (int i = 0; i < state.Members.Count; ++i)
                                {
                                        BaseRed ally = state.Members[i];

                                        if (ally == null)
                                                continue;

                                        candidates.Add(ally);
                                }
                        }
                }

                private void RegisterActiveExtortionTarget(PlayerMobile target)
                {
                        if (target == null)
                                return;

                        lock (s_activeExtortionSync)
                        {
                                s_activeExtortions[target] = this;
                        }
                }

                private void UnregisterActiveExtortionTarget(PlayerMobile target)
                {
                        UnregisterActiveExtortionTarget(target, this);
                }

                private static void UnregisterActiveExtortionTarget(PlayerMobile target, BaseRed leader)
                {
                        if (target == null || leader == null)
                                return;

                        lock (s_activeExtortionSync)
                        {
                                BaseRed current;

                                if (s_activeExtortions.TryGetValue(target, out current) && current == leader)
                                        s_activeExtortions.Remove(target);
                        }
                }

                private static bool TryGetActiveExtortionLeader(PlayerMobile target, out BaseRed leader)
                {
                        leader = null;

                        if (target == null)
                                return false;

                        lock (s_activeExtortionSync)
                        {
                                BaseRed candidate;

                                if (!s_activeExtortions.TryGetValue(target, out candidate))
                                        return false;

                                leader = candidate;
                        }

                        if (leader == null || leader.Deleted || !leader.Alive || !leader.m_roleplayActive || leader.m_roleplayTarget != target)
                        {
                                UnregisterActiveExtortionTarget(target, leader);
                                leader = null;
                                return false;
                        }

                        return true;
                }

                private void BroadcastImmediateExtortionSuppression(IEnumerable<BaseRed> allies, PlayerMobile target)
                {
                        if (allies == null || target == null)
                                return;

                        foreach (BaseRed ally in allies)
                        {
                                if (ally == null || ally.Deleted)
                                        continue;

                                ally.ApplyExtortionSuppressionPulse(target, this, false);
                        }
                }

                private static void PurgeExtortionPackMembers(ExtortionPackState state)
                {
                        if (state == null || state.Members == null || state.Members.Count == 0)
                                return;

                        for (int i = state.Members.Count - 1; i >= 0; --i)
                        {
                                BaseRed member = state.Members[i];

                                if (member == null || member.Deleted)
                                        state.Members.RemoveAt(i);
                        }
                }

                private int CompareBackupCandidatesByDistance(BaseRed x, BaseRed y)
                {
                        if (ReferenceEquals(x, y))
                                return 0;

                        if (x == null)
                                return 1;

                        if (x.Deleted)
                                return 1;

                        if (y == null)
                                return -1;

                        if (y.Deleted)
                                return -1;

                        PlayerMobile target = m_roleplayTarget;

                        if (target == null)
                                return 0;

                        double xDistance = x.GetDistanceToSqrt(target);
                        double yDistance = y.GetDistanceToSqrt(target);

                        return xDistance.CompareTo(yDistance);
                }

                private bool CanJoinAsBackup(BaseRed leader)
                {
                        if (Deleted || !Alive)
                                return false;

                        if (m_roleplayActive || m_backupActive)
                                return false;

                        if (leader == null || leader.Deleted)
                                return false;

                        return true;
                }

                private void JoinAsBackup(BaseRed leader, PlayerMobile target, int slotIndex)
                {
                        if (leader == null || target == null)
                                return;

                        m_backupLeader = leader;
                        m_backupActive = true;
                        m_roleplayTarget = target;
                        m_backupSlotIndex = (slotIndex < 0) ? 0 : slotIndex;
                        RemoveAggressionFor(target);
                        UnlockMovement();

                        if (!m_hasBackupStoredRoleplayEnabled)
                        {
                                m_backupStoredRoleplayEnabled = m_roleplayEnabled;
                                m_hasBackupStoredRoleplayEnabled = true;
                        }

                        m_roleplayEnabled = false;

                        SuppressCombatStance();

                        m_nextJeer = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(3, 8));

                        Say(BackupTaunts[Utility.Random(BackupTaunts.Length)]);
                }

                private void LeaveBackupRole(bool restoreCombat)
                {
                        if (!m_backupActive)
                                return;

                        UnlockMovement();

                        m_backupActive = false;

                        if (restoreCombat)
                                RestoreCombatStance();
                        else
                                MaintainGuardingPosture();

                        m_backupSlotIndex = -1;

                        if (m_hasBackupStoredRoleplayEnabled)
                        {
                                m_roleplayEnabled = m_backupStoredRoleplayEnabled;
                                m_hasBackupStoredRoleplayEnabled = false;
                                m_backupStoredRoleplayEnabled = false;
                        }

                        m_backupLeader = null;
                        m_roleplayTarget = null;
                        m_nextJeer = DateTime.MinValue;
                }

                private void ReleaseBackupAllies(bool restoreCombat = true)
                {
                        if (m_backupAllies == null)
                                return;

                        for (int i = m_backupAllies.Count - 1; i >= 0; --i)
                        {
                                BaseRed ally = m_backupAllies[i];

                                if (ally == null || ally.Deleted)
                                        continue;

                                ally.LeaveBackupRole(restoreCombat);
                        }

                        m_backupAllies.Clear();
                }

                private bool TryGetDemand(Container pack, out DemandCategory category, out int amount)
                {
                        List<DemandCategory> options = new List<DemandCategory>();
                        int value;

                        value = CountGold(pack);
                        if (value > 0)
                                options.Add(DemandCategory.Gold);

                        value = CountReagents(pack);
                        if (value > 0)
                                options.Add(DemandCategory.Reagents);

                        value = CountOre(pack);
                        if (value > 0)
                                options.Add(DemandCategory.Ore);

                        value = CountWood(pack);
                        if (value > 0)
                                options.Add(DemandCategory.Wood);

                        value = CountBandages(pack);
                        if (value > 0)
                                options.Add(DemandCategory.Bandages);

                        value = CountPotions(pack);
                        if (value > 0)
                                options.Add(DemandCategory.Potions);

                        if (options.Count == 0)
                        {
                                category = DemandCategory.Gold;
                                amount = 0;
                                return false;
                        }

                        category = options[Utility.Random(options.Count)];

                        switch (category)
                        {
                                case DemandCategory.Gold:
                                        amount = CountGold(pack);
                                        break;
                                case DemandCategory.Reagents:
                                        amount = CountReagents(pack);
                                        break;
                                case DemandCategory.Ore:
                                        amount = CountOre(pack);
                                        break;
                                case DemandCategory.Wood:
                                        amount = CountWood(pack);
                                        break;
                                case DemandCategory.Bandages:
                                        amount = CountBandages(pack);
                                        break;
                                case DemandCategory.Potions:
                                        amount = CountPotions(pack);
                                        break;
                                default:
                                        amount = 0;
                                        break;
                        }

                        return amount > 0;
                }

                private int DetermineDemandAmount(DemandCategory category, int available)
                {
                        switch (category)
                        {
                                case DemandCategory.Gold:
                                        return Utility.RandomMinMax(Math.Min(available, 100), Math.Min(available, 400));
                                case DemandCategory.Reagents:
                                        return Math.Max(1, Utility.RandomMinMax(Math.Min(available, 5), Math.Min(available, 25)));
                                case DemandCategory.Ore:
                                        return Math.Max(1, Utility.RandomMinMax(Math.Min(available, 5), Math.Min(available, 20)));
                                case DemandCategory.Wood:
                                        return Math.Max(1, Utility.RandomMinMax(Math.Min(available, 5), Math.Min(available, 40)));
                                case DemandCategory.Bandages:
                                        return Math.Max(1, Utility.RandomMinMax(Math.Min(available, 10), Math.Min(available, 40)));
                                case DemandCategory.Potions:
                                        return Math.Max(1, Utility.RandomMinMax(Math.Min(available, 2), Math.Min(available, 10)));
                                default:
                                        return Math.Max(1, available);
                        }
                }

                private string GetDemandLabel(DemandCategory category)
                {
                        switch (category)
                        {
                                case DemandCategory.Gold:
                                        return "gold coins";
                                case DemandCategory.Reagents:
                                        return "reagents";
                                case DemandCategory.Ore:
                                        return "ore";
                                case DemandCategory.Wood:
                                        return "logs";
                                case DemandCategory.Bandages:
                                        return "bandages";
                                case DemandCategory.Potions:
                                        return "potions";
                                default:
                                        return "goods";
                        }
                }

                private bool IsDemandItem(Item item)
                {
                        switch (m_demandCategory)
                        {
                                case DemandCategory.Gold:
                                        return item is Gold;
                                case DemandCategory.Reagents:
                                        return item is BaseReagent;
                                case DemandCategory.Ore:
                                        return item is BaseOre;
                                case DemandCategory.Wood:
                                        return item is BaseLog;
                                case DemandCategory.Bandages:
                                        return item is Bandage;
                                case DemandCategory.Potions:
                                        return item is BasePotion;
                                default:
                                        return false;
                        }
                }

                private void FinishRoleplaySuccess()
                {
                        Say("A smart choice. Enjoy breathing another day.");
                        RecallNearbyReds();
                }

                public void TryHandlePostKillCourtesy(PlayerMobile victim)
                {
                        if (victim == null || victim.Deleted)
                                return;

                        if (Deleted || !Alive || Map == null || m_recalling)
                                return;

                        if (m_courtesyPending)
                                return;

                        if (Utility.RandomDouble() >= CourtesyResurrectionChance)
                                return;

                        MaintainGuardingPosture();
                        Warmode = false;
                        Combatant = null;
                        FocusMob = null;
                        RemoveAggressionFor(victim);

                        m_courtesyPending = true;
                        m_courtesyVictim = victim;
                        m_courtesyResponseDeadline = DateTime.UtcNow + CourtesyResponseTimeout;
                        m_courtesyStoredCanHearGhosts = CanHearGhosts;
                        m_hasCourtesyStoredCanHearGhosts = true;
                        CanHearGhosts = true;

                        Direction = GetDirectionTo(victim);
                        Say("Good fight! Want me to bring you back?");

                        victim.SendMessage(0x35, string.Format("{0} offers to resurrect you. Reply yes or no to choose.", Name));
                        victim.SendMessage(0x35, "Responses like yay/nay or affirmative/negative also work.");
                }

                public override void OnSpeech(SpeechEventArgs e)
                {
                        base.OnSpeech(e);

                        if (!m_courtesyPending)
                                return;

                        PlayerMobile speaker = e.Mobile as PlayerMobile;

                        if (speaker == null || speaker != m_courtesyVictim)
                                return;

                        if (IsCourtesyAffirmativeResponse(e.Speech))
                        {
                                HandleCourtesyAcceptance();
                                return;
                        }

                        if (IsCourtesyNegativeResponse(e.Speech))
                                HandleCourtesyDecline("Suit yourself.", false);
                }

                private void RecallNearbyReds(bool includeFarewells = false, string leaderFarewell = null)
                {
                        HashSet<BaseRed> reds = new HashSet<BaseRed>();

                        reds.Add(this);

                        if (Map != null)
                        {
                                int range = Math.Max(GetRoleplayTriggerRange() + 2, ExtortionSuppressionRadius);
                                IPooledEnumerable eable = GetMobilesInRange(range);

                                foreach (Mobile mob in eable)
                                {
                                        BaseRed red = mob as BaseRed;

                                        if (red == null || red.Deleted || !red.Alive)
                                                continue;

                                        reds.Add(red);
                                }

                                eable.Free();
                        }

                        foreach (BaseRed red in reds)
                        {
                                if (red == null)
                                        continue;

                                if (includeFarewells)
                                {
                                        string shout = (red == this && !string.IsNullOrEmpty(leaderFarewell)) ? leaderFarewell : GetRandomFarewellShout();

                                        if (!string.IsNullOrEmpty(shout))
                                                red.Say(shout);
                                }

                                red.BeginRecallDeparture();
                        }
                }

                private static string GetRandomFarewellShout()
                {
                        if (FarewellShouts == null || FarewellShouts.Length == 0)
                                return null;

                        return FarewellShouts[Utility.Random(FarewellShouts.Length)];
                }

                private static string GetRoleplayBreakTaunt(PlayerMobile player)
                {
                        if (player == null || RoleplayBreakTaunts == null || RoleplayBreakTaunts.Length == 0)
                                return null;

                        return string.Format(RoleplayBreakTaunts[Utility.Random(RoleplayBreakTaunts.Length)], player.Name);
                }

                private void BeginRecallDeparture()
                {
                        if (Deleted || m_recalling)
                                return;

                        m_recalling = true;

                        UnlockMovement();

                        ReleaseBackupAllies(false);
                        MaintainGuardingPosture();

                        UnregisterActiveExtortionTarget(m_roleplayTarget);

                        ClearCourtesyState();

                        m_roleplayEnabled = false;
                        m_roleplayActive = false;
                        m_backupActive = false;
                        m_backupLeader = null;
                        m_roleplayTarget = null;
                        m_demandCollected = 0;
                        m_demandAmount = 0;
                        m_nextCountdownValue = 0;
                        m_roleplayDamageTaken = 0;
                        m_nextSuppressionPulse = DateTime.MinValue;
                        m_nextCountdown = DateTime.MinValue;
                        m_roleplayDeadline = DateTime.MinValue;
                        m_backupAllies.Clear();
                        m_backupStoredRoleplayEnabled = false;
                        m_hasBackupStoredRoleplayEnabled = false;

                        Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
                        PlaySound(0x1FC);
                        Timer.DelayCall(TimeSpan.FromSeconds(1.0), Delete);
                }

                private void EndRoleplay(bool engageCombat, bool restoreCombat = true)
                {
                        UnregisterActiveExtortionTarget(m_roleplayTarget);

                        ReleaseBackupAllies(restoreCombat);

                        UnlockMovement();

                        if (!m_roleplayEnabled)
                                return;

                        if (restoreCombat)
                        {
                                RestoreCombatStance();
                        }
                        else
                        {
                                MaintainGuardingPosture();
                        }

                        m_roleplayEnabled = false;
                        m_roleplayActive = false;

                        if (engageCombat && m_roleplayTarget != null && m_roleplayTarget.Alive)
                        {
                                Combatant = m_roleplayTarget;
                                FocusMob = m_roleplayTarget;
                                Warmode = true;
                                Say(string.Format("Now you die, {0}!", m_roleplayTarget.Name));
                        }

                        m_roleplayTarget = null;
                        m_demandCollected = 0;
                        m_demandAmount = 0;
                        m_nextCountdownValue = 0;
                        m_roleplayDamageTaken = 0;
                        m_nextSuppressionPulse = DateTime.MinValue;
                }

                private void SuppressCombatStance()
                {
                        if (!m_hasSavedFightMode)
                        {
                                m_savedFightMode = FightMode;
                                m_hasSavedFightMode = true;
                        }

                        FightMode = FightMode.None;
                        Combatant = null;
                        Warmode = false;
                        FocusMob = null;
                        m_attacking = false;
                        RemoveAggressionFor(m_roleplayTarget);

                        if (AIObject != null)
                        {
                                if (!m_hasSavedAIAction)
                                {
                                        m_savedAIAction = AIObject.Action;
                                        m_hasSavedAIAction = true;
                                }

                                AIObject.Action = ActionType.Guard;
                                AIObject.NextMove = DateTime.UtcNow + TimeSpan.FromSeconds(1.0);
                        }
                }

                private void RestoreCombatStance()
                {
                        if (m_hasSavedFightMode)
                        {
                                FightMode = m_savedFightMode;
                                m_hasSavedFightMode = false;
                        }

                        if (AIObject != null && m_hasSavedAIAction)
                        {
                                AIObject.Action = m_savedAIAction;
                                m_hasSavedAIAction = false;
                        }
                }

                private void MaintainGuardingPosture()
                {
                        FightMode = FightMode.None;
                        Combatant = null;
                        Warmode = false;
                        FocusMob = null;
                        m_attacking = false;
                        RemoveAggressionFor(m_roleplayTarget);

                        if (AIObject != null)
                        {
                                AIObject.Action = ActionType.Guard;
                                AIObject.NextMove = DateTime.UtcNow + TimeSpan.FromSeconds(1.0);
                        }

                        m_hasSavedFightMode = false;
                        m_hasSavedAIAction = false;
                }

                private void RemoveAggressionFor(Mobile mob)
                {
                        if (mob == null)
                                return;

                        List<AggressorInfo> aggressors = Aggressors;

                        for (int i = aggressors.Count - 1; i >= 0; --i)
                        {
                                AggressorInfo info = aggressors[i];

                                if (info != null && info.Attacker == mob)
                                        aggressors.RemoveAt(i);
                        }

                        List<AggressorInfo> aggressed = Aggressed;

                        for (int i = aggressed.Count - 1; i >= 0; --i)
                        {
                                AggressorInfo info = aggressed[i];

                                if (info != null && info.Defender == mob)
                                        aggressed.RemoveAt(i);
                        }
                }

                private void ClearAggressionRecords()
                {
                        Combatant = null;
                        FocusMob = null;
                        Warmode = false;
                        m_attacking = false;

                        List<AggressorInfo> aggressors = Aggressors;

                        for (int i = aggressors.Count - 1; i >= 0; --i)
                        {
                                if (i >= aggressors.Count)
                                        continue;

                                AggressorInfo info = aggressors[i];

                                if (info == null)
                                {
                                        aggressors.RemoveAt(i);
                                        continue;
                                }

                                Mobile attacker = info.Attacker;

                                if (attacker != null)
                                {
                                        if (!attacker.Deleted)
                                        {
                                                attacker.RemoveAggressed(this);

                                                if (attacker.Combatant == this)
                                                        attacker.Combatant = null;
                                        }

                                        RemoveAggressor(attacker);
                                }
                                else
                                {
                                        aggressors.RemoveAt(i);
                                }
                        }

                        List<AggressorInfo> aggressed = Aggressed;

                        for (int i = aggressed.Count - 1; i >= 0; --i)
                        {
                                if (i >= aggressed.Count)
                                        continue;

                                AggressorInfo info = aggressed[i];

                                if (info == null)
                                {
                                        aggressed.RemoveAt(i);
                                        continue;
                                }

                                Mobile defender = info.Defender;

                                if (defender != null)
                                {
                                        if (!defender.Deleted)
                                        {
                                                defender.RemoveAggressor(this);

                                                if (defender.Combatant == this)
                                                        defender.Combatant = null;
                                        }

                                        RemoveAggressed(defender);
                                }
                                else
                                {
                                        aggressed.RemoveAt(i);
                                }
                        }
                }

                private int CountGold(Container container)
                {
                        return CountItems(container, typeof(Gold));
                }

                private int CountReagents(Container container)
                {
                        return CountItems(container, typeof(BaseReagent));
                }

                private int CountOre(Container container)
                {
                        return CountItems(container, typeof(BaseOre));
                }

                private int CountWood(Container container)
                {
                        return CountItems(container, typeof(BaseLog));
                }

                private int CountBandages(Container container)
                {
                        return CountItems(container, typeof(Bandage));
                }

                private int CountPotions(Container container)
                {
                        return CountItems(container, typeof(BasePotion));
                }

                private int CountItems(Container container, Type type)
                {
                        int total = 0;

                        foreach (Item item in container.Items)
                        {
                                if (item == null)
                                        continue;

                                if (type.IsAssignableFrom(item.GetType()))
                                        total += Math.Max(1, item.Amount);

                                if (item is Container)
                                        total += CountItems((Container)item, type);
                        }

                        return total;
                }

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			if ( Utility.RandomDouble() < (this.Fame / 250000) && (this.AI == AIType.AI_Melee) ) //odds of doing a special
			{
				BleedThem(defender);
			}

			base.OnGaveMeleeAttack( defender );
		}

        private void BleedThem(Mobile m)
        {
            if (m == null) return;
            m.SendLocalizedMessage(1060160); // You are bleeding!
            m.PlaySound(0x133);
            m.FixedParticles(0x377A, 244, 25, 9950, 31, 0, EffectLayer.Waist);
            BleedAttack.BeginBleed(m, m);
        }

                public override void OnThink()
                {
                        ProcessExtortionSuppression();

                        base.OnThink();

                        ProcessCourtesyThink();

                        if (m_roleplayEnabled && !m_roleplayActive && !m_courtesyPending)
                        {
                                PlayerMobile candidate = Combatant as PlayerMobile;

                                if (candidate == null)
                                        candidate = FocusMob as PlayerMobile;

                                int triggerRange = GetRoleplayTriggerRange();

                                if (candidate != null && !candidate.Deleted && candidate.Alive && !candidate.Hidden && InRange(candidate, triggerRange))
                                {
                                        TryStartRoleplay(candidate);
                                }
                        }

                        if (!m_courtesyPending)
                                ProcessRoleplayThink();

                        if (m_roleplayActive)
                                PulseExtortionSuppression();

                        ProcessExtortionSuppression();

                        //speed change
                        if (!Mounted)
                        {
                                AIFullSpeedActive = AIFullSpeedPassive = false;
			}
			else 
				AIFullSpeedActive = AIFullSpeedPassive = true;


			// Chug pots
			if ( this.Poisoned )
			{
				GreaterCurePotion m_CPot = (GreaterCurePotion)this.Backpack.FindItemByType( typeof ( GreaterCurePotion ) );
				if ( m_CPot != null )
					m_CPot.Drink( this );
			}

			if ( this.Hits <= (this.HitsMax * .7) ) // Will try to use heal pots if he's at or below 70% health
			{
				GreaterHealPotion m_HPot = (GreaterHealPotion)this.Backpack.FindItemByType( typeof ( GreaterHealPotion ) );
				if ( m_HPot != null )
					m_HPot.Drink( this );
			}
			
			if ( this.Stam <= (this.StamMax * .25) ) // Will use a refresh pot if he's at or below 25% stam
			{
				TotalRefreshPotion m_RPot = (TotalRefreshPotion)this.Backpack.FindItemByType( typeof ( TotalRefreshPotion) );
				if ( m_RPot != null )
					m_RPot.Drink( this );
			}

				    // Use bandages
			if ( (this.IsHurt() || this.Poisoned) && m_Bandaging == false )
			{
				Bandage m_Band = (Bandage)this.Backpack.FindItemByType( typeof ( Bandage ) );

				if ( m_Band != null )
				{
					m_Bandaging = true;

					if ( BandageContext.BeginHeal( this, this ) != null )
					m_Band.Consume();
					BandageTimer bt = new BandageTimer( this );
					bt.Start();
				}
			}
			/*if (this.Combatant != null && this.Combatant is BaseCreature )
			{
				BaseCreature bc = (BaseCreature)this.Combatant;

					if (!m_freezing && !bc.Paralyzed && !bc.Frozen && Utility.RandomDouble() > 0.75)
					{
						m_freezing = true;
						this.Frozen = true;

						Say("An Ex Por");
						
						Timer.DelayCall( TimeSpan.FromSeconds( 2 ), new TimerStateCallback ( FreezeIt ), new object[]{ this.Combatant }  );
					}
					

				if (bc.Controlled && bc.ControlMaster != null && InLOS( bc.ControlMaster ) &&  (int)GetDistanceToSqrt( bc.ControlMaster ) < 10 )
				{
					this.Combatant = bc.ControlMaster;
				}
			}*/
			//situational awareness
			if (m_attacking == false && this.Combatant != null) // new target found, just started attacking
			{
				m_attacking = true;
				CallOthers( this.Combatant );
			}
			if (m_attacking && this.Combatant == null)
				m_attacking = false;

			if (this.Combatant != null && this.Combatant is PlayerMobile )
			{
				PlayerMobile pm = (PlayerMobile)this.Combatant;
				if (pm.Hidden && InRange( pm, 10 ) && !m_revealing )
				{
					if (Utility.RandomBool())
						Say("Hah! Nice try you noob.");
					else 
						Say("Playing Hide and go seek?  What do you think I am!");

					Say("Wis Quas");
					m_revealing = true;
					Timer.DelayCall( TimeSpan.FromSeconds( 2 ), new TimerStateCallback ( RevealHim ), new object[]{ pm }  );
				}
				else if ( DateTime.UtcNow >= m_NextResurrect && (this is SpawnRed || this is DreadLord  ) && (int)GetDistanceToSqrt( this.Combatant ) > 2 && (int)GetDistanceToSqrt( this.Combatant ) < 12 && InLOS( this.Combatant ) && !m_teleporting )
				{
					m_NextResurrect = DateTime.UtcNow + ResurrectDelay;
					m_teleporting = true;
					this.Frozen = true;

					Say("Rel Por");
						
					Timer.DelayCall( TimeSpan.FromSeconds( 1 ), new TimerStateCallback ( TeleportTo ), new object[]{ this.Combatant }  );

				}
			}
                        if (this.Combatant != null && this is RedPlayer && InLOS( this.Combatant ) && (int)GetDistanceToSqrt( this.Combatant ) > 2 && (int)GetDistanceToSqrt( this.Combatant ) < 15 && !m_Threwpot)
                        {
                                        ExplosionPotion m_Expot = (ExplosionPotion)this.Backpack.FindItemByType( typeof ( ExplosionPotion ) );

                                        if ( m_Expot != null && m_potTimer == null )
                                        {
                                                this.Frozen = true;
                                                m_Threwpot = true;
                                                Timer.DelayCall( TimeSpan.FromSeconds( 1 ), new TimerStateCallback ( Throwpot ), new object[]{ this } );
                                        }
                        }

                }

                public override void AggressiveAction(Mobile aggressor, bool criminal)
                {
                        if (aggressor != null && InRoleplayScene() && aggressor == m_roleplayTarget)
                        {
                                RemoveAggressionFor(aggressor);
                                MaintainGuardingPosture();
                                return;
                        }

                        base.AggressiveAction(aggressor, criminal);
                }

                public override void OnDamage(int amount, Mobile from, bool willKill)
                {
                        if (from != null && m_roleplayActive && from == m_roleplayTarget)
                        {
                                if (amount > 0)
                                {
                                        m_roleplayDamageTaken += amount;

                                        if (m_roleplayDamageTaken > RoleplayDamageBreakThreshold)
                                        {
                                                string taunt = GetRoleplayBreakTaunt(m_roleplayTarget);

                                                if (!string.IsNullOrEmpty(taunt))
                                                        Say(taunt);

                                                EndRoleplay(true);
                                        }
                                        else
                                        {
                                                RemoveAggressionFor(from);
                                                MaintainGuardingPosture();
                                        }
                                }
                                else
                                {
                                        RemoveAggressionFor(from);
                                        MaintainGuardingPosture();
                                }
                        }

                        base.OnDamage(amount, from, willKill);
                }

                public override void OnGotMeleeAttack( Mobile attacker )
                {
                        if (attacker != null && InRoleplayScene() && attacker == m_roleplayTarget)
                        {
                                RemoveAggressionFor(attacker);
                                MaintainGuardingPosture();
                                return;
                        }

                        if ( !(this is RedPlayer) && attacker is BaseCreature && !m_freezing && !attacker.Frozen && Utility.RandomDouble() > 0.75 && InLOS( attacker ) &&  (int)GetDistanceToSqrt( attacker ) < 10)
                        {
                                BaseCreature bc = (BaseCreature)attacker;

                                m_freezing = true;
                                this.Frozen = true;

                                Say("An Ex Por");

                                Timer.DelayCall( TimeSpan.FromSeconds( 2 ), new TimerStateCallback ( FreezeIt ), new object[]{ attacker }  );

                                if (bc.Controlled && bc.ControlMaster != null && InLOS( bc.ControlMaster ) &&  (int)GetDistanceToSqrt( bc.ControlMaster ) < 10 )
                                {
                                        this.Combatant = bc.ControlMaster;
                                }
                        }
                        base.OnGotMeleeAttack( attacker );
                }

                public override void OnDamagedBySpell(Mobile from)
                {
                        if (from != null && InRoleplayScene() && from == m_roleplayTarget)
                        {
                                RemoveAggressionFor(from);
                                MaintainGuardingPosture();
                                return;
                        }

                        base.OnDamagedBySpell(from);
                }

                public override void CallOthers( Mobile target )
                {
                        if (target == null)
                                return;

                        if (m_roleplayActive || m_backupActive)
                                return;

                        PlayerMobile playerTarget = target as PlayerMobile;
                        BaseRed extortionLeader = null;
                        bool targetUnderExtortion = playerTarget != null && TryGetActiveExtortionLeader(playerTarget, out extortionLeader);

                        bool callout = false;

                        foreach ( Mobile mob in this.GetMobilesInRange( 13 ) )
                        {
                                BaseRed ally = mob as BaseRed;

                                if ( ally == null )
                                        continue;

                                if ( !InLOS( ally ) )
                                        continue;

                                if ( ally.m_roleplayActive || ally.m_backupActive )
                                        continue;

                                if ( ally.m_roleplayEnabled && ally.m_roleplayTarget != null )
                                        continue;

                                if (targetUnderExtortion)
                                        continue;

                                if ( ally.Combatant != null )
                                        continue;

                                ally.Combatant = target;
                                ally.Warmode = true;

                                if ( !callout )
                                        callout = true;
                        }

                        if (callout)
                        {
                                if (Utility.RandomBool() )
                                        this.Say("Found a Noob!");
                                else
                                        this.Say("This Way!");
                        }
                }

		public void RevealHim( object state )
		{
			m_revealing = false;
			if (this.Deleted || this == null)
				return;
				
			object[] states = (object[])state;

			PlayerMobile from = (PlayerMobile)states[0];
			Map map = from.Map;

			if ( map == null || map == Map.Internal || from == null )
				return;

			if ( InLOS( from ) )
			{
				from.RevealingAction();
			}
			
		}

		private bool m_freezing;

		public void FreezeIt( object state )
		{

			this.m_freezing = false;
			this.Frozen = false;

			if (this.Deleted || this == null)
				return;

			object[] states = (object[])state;
			Mobile from = (Mobile)states[0];

			if (from == null )
				return;

			if ( InLOS( from ) )
			{
						from.Paralyze( TimeSpan.FromSeconds( Utility.RandomMinMax(3, 5) ) );

						from.PlaySound( 0x204 );
						from.FixedEffect( 0x376A, 10, 16, 0, 0 );
			}
		}

		private bool m_teleporting;

		public void TeleportTo( object state )
		{
			this.m_teleporting = false;
			this.Frozen = false;

			if (this.Deleted || this == null)
				return;

			object[] states = (object[])state;
			Mobile from = (Mobile)states[0];

			if (from == null || from.Map == null )
				return;

 			if ( (int)GetDistanceToSqrt( from ) > 2 && (int)GetDistanceToSqrt( from ) < 12 && InLOS( from ))
			{
				from.PlaySound( 0x1FE );
				this.MoveToWorld( from.Location, from.Map );
			}
		}

		private Timer m_potTimer;		
		public void Throwpot( object state )
		{
			m_Threwpot = false;

			if (this.Deleted || this == null )
				return;

			this.Frozen = false;
			object[] states = (object[])state;

			BaseRed from = (BaseRed)states[0];

			if (!InLOS( from ))
				return;

			if (from == null || from.Map == null || from.Combatant == null || from.Backpack == null)
				return;

			Map map = from.Map;
			Mobile target = from.Combatant;
			Map targetmap = target.Map;
			Point3D targetloc = target.Location;

			if (  target == null || targetmap == Map.Internal || targetmap == null )
				return;

		    BaseExplosionPotion explosion = (BaseExplosionPotion)from.Backpack.FindItemByType( typeof( BaseExplosionPotion ) );
		    if( explosion != null && explosion.Amount > 0)
		    {

				if( explosion.Amount > 1 )
				{
					Mobile.LiftItemDupe( explosion, 1 );
				}

				explosion.Internalize();

				Effects.SendMovingEffect( from, target, 0x2408, 7, 0, false, false, 0, 0 );

				Timer.DelayCall( TimeSpan.FromSeconds( 1.0 ), new TimerStateCallback( Reposition_OnTick ), new object[]{ explosion, targetloc, targetmap } );
				
			}
		}

		private void Reposition_OnTick( object state )
		{
			if ( Deleted )
				return;

			object[] states = (object[])state;
			Item pot = (Item)states[0];
			Point3D p = (Point3D)states[1];
			Map map = (Map)states[2];

			pot.MoveToWorld( p, map );

			m_potTimer = Timer.DelayCall( TimeSpan.FromSeconds( 1.0 ), TimeSpan.FromSeconds( 1.25 ), 5, new TimerStateCallback( Detonate_OnTick ), new object[]{ pot, p, map, 3 } ); // 3.6 seconds explosion delay


		}

		private void Detonate_OnTick( object state )
		{

			object[] states = (object[])state;
			BaseExplosionPotion pot = (BaseExplosionPotion)states[0];
			Point3D loc = (Point3D)states[1];
			Map map = (Map)states[2];
			int timer = (int)states[3];

			if (pot.Deleted)
				return;

			if ( timer == 0 )
			{
				if ( pot.Deleted )
					return;

				pot.Consume();

				if ( map == null )
					return;

				Effects.PlaySound(loc, map, 0x307);

				Effects.SendLocationEffect(loc, map, 0x36B0, 9, 10, 0, 0);

				IPooledEnumerable eable =  map.GetObjectsInRange( loc, 2 ) ;
				ArrayList toExplode = new ArrayList();

				int toDamage = 0;

				foreach ( object o in eable )
				{
					if ( o is Mobile && !(o is BaseRed) ) 
					{
						toExplode.Add( o );
						++toDamage;
					}
				}

				eable.Free();

				for ( int i = 0; i < toExplode.Count; ++i )
				{
					object o = toExplode[i];

					if ( o is Mobile )
					{
						Mobile m = (Mobile)o;

						int damage = Utility.RandomMinMax( 20, 80 );

						AOS.Damage( m, this, damage, 0, 100, 0, 0, 0 );
					}
					else if ( o is BaseExplosionPotion )
					{
						BaseExplosionPotion pots = (BaseExplosionPotion)o;

						pots.Explode( this, false, pots.GetWorldLocation(), pots.Map );
					}
				}

				m_potTimer = null;
			}
			else
			{
				pot.PublicOverheadMessage( MessageType.Regular, 0x22, false, timer.ToString() );
				states[3] = timer - 1;
			}
		}

		private class BandageTimer : Timer 
		{ 
			private BaseRed pk;

			public BandageTimer( BaseRed o ) : base( TimeSpan.FromSeconds( Utility.RandomMinMax(3,6) ) ) 
			{ 
			pk = o;
			Priority = TimerPriority.OneSecond; 
			} 

			protected override void OnTick() 
			{ 
			pk.m_Bandaging = false; 
			} 
		}

		public override bool OnBeforeDeath()
		{

		    Mobile murderer = this.LastKiller;

		    if ( murderer is PlayerMobile )
		    {
				if (Utility.RandomMinMax(1, 250) == 69 && (this is SpawnRedMage || this is SpawnRed || this is DreadLord) )
				{
					Container pack = Backpack;
					pack.DropItem( new EtherealHorse() );
				}
			}

			return base.OnBeforeDeath();
		}

		public BaseRed( Serial serial ) : base( serial ) 
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
                        AIFullSpeedActive = AIFullSpeedPassive = true; // Force full speed
                        m_movementLocked = false;
                        Frozen = false;
                        CantWalk = false;
                        m_roleplayDisallowAllMoves = false;
                        m_courtesyPending = false;
                        m_courtesyVictim = null;
                        m_courtesyResponseDeadline = DateTime.MinValue;
                        m_courtesyStoredCanHearGhosts = false;
                        m_hasCourtesyStoredCanHearGhosts = false;
                        m_roleplayPackSpawner = null;
                }
        }
}
