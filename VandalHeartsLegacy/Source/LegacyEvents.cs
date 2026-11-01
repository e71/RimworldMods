using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using VandalHeartsChase;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;

namespace VandalHeartsLegacy
{
    /// <summary>Picks Legacy characters (the chase roster) who are out in the world and free to show up.</summary>
    public static class LegacyRoster
    {
        /// <param name="friendlyOnly">Only characters who belong to a faction at peace with the colony.</param>
        public static IEnumerable<Pawn> Available(bool friendlyOnly = false)
        {
            var tracker = ChaseTracker.Instance;
            if (tracker == null)
            {
                return Enumerable.Empty<Pawn>();
            }
            return tracker.chasePawns.Where(p =>
                p != null && !p.Dead && !p.Destroyed && !p.Spawned && !p.Downed
                && Find.WorldPawns.Contains(p)
                && Find.WorldPawns.GetSituation(p) == WorldPawnSituation.Free
                && !QuestUtility.IsReservedByQuestOrQuestBeingGenerated(p)
                && (!friendlyOnly || (p.Faction != null && !p.Faction.IsPlayer && !p.Faction.HostileTo(Faction.OfPlayer))));
        }

        public static bool AnyAvailable(bool friendlyOnly = false) => Available(friendlyOnly).Any();

        public static Pawn Pick(bool friendlyOnly = false) => Available(friendlyOnly).RandomElementWithFallback();
    }

    /// <summary>Stores a Legacy character in the slate, standing in for QuestNode_GeneratePawn.</summary>
    public class QuestNode_GetLegacyCharacter : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;

        public SlateRef<bool> friendlyOnly;

        // The test run stores a candidate too, so later nodes that need the pawn can test-run against it.
        protected override bool TestRunInt(Slate slate)
        {
            var pawn = LegacyRoster.Pick(friendlyOnly.GetValue(slate));
            if (pawn == null)
            {
                return false;
            }
            slate.Set(storeAs.GetValue(slate), pawn);
            return true;
        }

        protected override void RunInt()
        {
            var pawn = LegacyRoster.Pick(friendlyOnly.GetValue(QuestGen.slate));
            if (pawn != null)
            {
                QuestGen.slate.Set(storeAs.GetValue(QuestGen.slate), pawn);
            }
        }
    }

    /// <summary>
    /// Puts the slate's Legacy character into a quest site, where a vanilla gen step spawns them when the map is made.
    /// The site keeps them until then; if it disappears first, they go back to the world.
    /// </summary>
    public static class LegacySiteUtility
    {
        public const string SlateKey = "legacyPawn";

        public static Pawn MoveIntoSite(SitePart part, Slate slate)
        {
            var pawn = slate.Get<Pawn>(SlateKey);
            if (Find.WorldPawns.Contains(pawn))
            {
                Find.WorldPawns.RemovePawn(pawn);
            }
            part.things = new ThingOwner<Pawn>(part, oneStackOnly: true);
            part.things.TryAdd(pawn);
            return pawn;
        }

        /// <summary>What SitePartWorker.Notify_GeneratedByQuestGen does; the vanilla subclasses can't be skipped past.</summary>
        public static void AddBaseRules(SitePart part, List<Rule> rules, Dictionary<string, string> constants)
        {
            rules.AddRange(GrammarUtility.RulesForDef("", part.def));
            constants.Add("sitePart", part.def.defName);
        }

        public static string RelationsInfo(Pawn pawn)
        {
            PawnRelationUtility.Notify_PawnsSeenByPlayer(Gen.YieldSingle(pawn), out var info, informEvenIfSeenBefore: true, writeSeenPawnsNames: false);
            return info.NullOrEmpty() ? "" : "\n\n" + "PawnHasTheseRelationshipsWithColonists".Translate(pawn.LabelShort, pawn) + "\n\n" + info;
        }
    }

    /// <summary>The vanilla prisoner camp, holding a Legacy character instead of a generated slave.</summary>
    public class SitePartWorker_LegacyPrisoner : SitePartWorker_PrisonerWillingToJoin
    {
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            LegacySiteUtility.AddBaseRules(part, outExtraDescriptionRules, outExtraDescriptionConstants);
            var pawn = LegacySiteUtility.MoveIntoSite(part, slate);
            pawn.guest.SetGuestStatus(part.site.Faction, GuestStatus.Prisoner);
            slate.Set("prisoner", pawn);
            outExtraDescriptionRules.Add(new Rule_String("prisonerFullRelationInfo", LegacySiteUtility.RelationsInfo(pawn)));
        }
    }

    /// <summary>The vanilla downed-refugee site with a hurt Legacy character. The base class heals them if nobody comes.</summary>
    public class SitePartWorker_LegacyDowned : SitePartWorker_DownedRefugee
    {
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            LegacySiteUtility.AddBaseRules(part, outExtraDescriptionRules, outExtraDescriptionConstants);
            var pawn = LegacySiteUtility.MoveIntoSite(part, slate);
            HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: false);
            HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, allowBleedingWounds: false);
            slate.Set("refugee", pawn);
            outExtraDescriptionRules.Add(new Rule_String("refugeeRelationInfo", LegacySiteUtility.RelationsInfo(pawn)));
        }
    }

    /// <summary>Adds a QuestPart_LegacyAlly: the character walks in to help against a raid, then joins once it's beaten.</summary>
    public class QuestNode_LegacyAlly : QuestNode
    {
        public SlateRef<Pawn> pawn;
        public SlateRef<Map> map;
        public SlateRef<Faction> enemyFaction;
        public SlateRef<IntVec3?> walkInSpot;

        [NoTranslate]
        public SlateRef<string> inSignalArrive;

        [NoTranslate]
        public SlateRef<string> inSignalRaidArrived;

        [NoTranslate]
        public SlateRef<string> outSignalJoined;

        [NoTranslate]
        public SlateRef<string> outSignalLost;

        protected override bool TestRunInt(Slate slate) => pawn.GetValue(slate) != null && map.GetValue(slate) != null;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            QuestGen.quest.AddPart(new QuestPart_LegacyAlly
            {
                pawn = pawn.GetValue(slate),
                mapParent = map.GetValue(slate).Parent,
                enemyFaction = enemyFaction.GetValue(slate),
                walkInSpot = walkInSpot.GetValue(slate) ?? IntVec3.Invalid,
                inSignalArrive = QuestGenUtility.HardcodedSignalWithQuestID(inSignalArrive.GetValue(slate)),
                inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalRaidArrived.GetValue(slate)),
                outSignalJoined = QuestGenUtility.HardcodedSignalWithQuestID(outSignalJoined.GetValue(slate)),
                outSignalLost = QuestGenUtility.HardcodedSignalWithQuestID(outSignalLost.GetValue(slate)),
            });
        }
    }

    public class QuestPart_LegacyAlly : QuestPartActivable
    {
        // Join anyway if the fight drags on; the assist-colony lord walks off after 25000 ticks.
        private const int MaxFightTicks = 20000;

        public Pawn pawn;
        public MapParent mapParent;
        public Faction enemyFaction;
        public IntVec3 walkInSpot = IntVec3.Invalid;
        public string inSignalArrive;
        public string outSignalJoined;
        public string outSignalLost;
        private bool arrived;
        private bool finished;

        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get
            {
                if (pawn != null && pawn.Spawned)
                {
                    yield return pawn;
                }
            }
        }

        public override bool QuestPartReserves(Pawn p) => p == pawn && !finished;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag == inSignalArrive && !arrived)
            {
                Arrive();
            }
        }

        private void Arrive()
        {
            arrived = true;
            var map = mapParent?.Map;
            if (map == null || pawn == null || pawn.Dead || pawn.Spawned || pawn.Faction == null || pawn.Faction.HostileTo(Faction.OfPlayer))
            {
                Finish(joined: false);
                return;
            }
            if (!walkInSpot.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out walkInSpot, map, CellFinder.EdgeRoadChance_Friendly))
            {
                walkInSpot = CellFinder.RandomEdgeCell(map);
            }
            if (Find.WorldPawns.Contains(pawn))
            {
                Find.WorldPawns.RemovePawn(pawn);
            }
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(walkInSpot, map, 4), map);
            if (!RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out var fallback))
            {
                fallback = pawn.Position;
            }
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_AssistColony(pawn.Faction, fallback), map, new List<Pawn> { pawn });
            Find.LetterStack.ReceiveLetter("Legacy: " + pawn.LabelShortCap + " arrives to fight",
                $"{pawn.NameFullColored} has come to stand with you against the coming attack. Keep {pawn.Possessive()} back covered: once the raiders are dealt with, {pawn.Possessive()} home will be here.",
                LetterDefOf.PositiveEvent, pawn, quest: quest);
        }

        public override void QuestPartTick()
        {
            base.QuestPartTick();
            if (finished || !arrived || Find.TickManager.TicksGame % 250 != 0)
            {
                return;
            }
            if (pawn == null || pawn.Dead || !pawn.Spawned)
            {
                Finish(joined: false);
                return;
            }
            int fightTicks = Find.TickManager.TicksGame - EnableTick;
            bool raidersLeft = pawn.Map.mapPawns.AllPawnsSpawned.Any(p =>
                !p.Dead && !p.Downed && !p.IsPrisoner && p.HostileTo(Faction.OfPlayer)
                && (enemyFaction != null ? p.Faction == enemyFaction : p.Faction != null));
            if ((fightTicks > 1000 && !raidersLeft) || fightTicks > MaxFightTicks)
            {
                Finish(joined: true);
            }
        }

        private void Finish(bool joined)
        {
            finished = true;
            if (joined)
            {
                pawn.SetFaction(Faction.OfPlayer);
                Find.LetterStack.ReceiveLetter("Legacy: " + pawn.LabelShortCap + " stays",
                    $"The fighting is over, and {pawn.NameFullColored} isn't leaving. {pawn.Possessive().CapitalizeFirst()} place is with you now.",
                    LetterDefOf.PositiveEvent, pawn, quest: quest);
            }
            Find.SignalManager.SendSignal(new Signal(joined ? outSignalJoined : outSignalLost));
            if (State == QuestPartState.Enabled)
            {
                Complete();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_References.Look(ref enemyFaction, "enemyFaction");
            Scribe_Values.Look(ref walkInSpot, "walkInSpot", IntVec3.Invalid);
            Scribe_Values.Look(ref inSignalArrive, "inSignalArrive");
            Scribe_Values.Look(ref outSignalJoined, "outSignalJoined");
            Scribe_Values.Look(ref outSignalLost, "outSignalLost");
            Scribe_Values.Look(ref arrived, "arrived");
            Scribe_Values.Look(ref finished, "finished");
        }
    }

    /// <summary>
    /// Puts Legacy characters back in shape after a rescue quest ends without them: no longer anyone's prisoner,
    /// no longer waiting to be rescued, and healed enough to travel with visitor groups again.
    /// </summary>
    public class LegacyAftercare : GameComponent
    {
        public LegacyAftercare(Game game)
        {
        }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay != 4242)
            {
                return;
            }
            var tracker = ChaseTracker.Instance;
            if (tracker == null)
            {
                return;
            }
            foreach (var pawn in tracker.chasePawns)
            {
                if (pawn == null || pawn.Dead || pawn.Spawned || !Find.WorldPawns.Contains(pawn)
                    || QuestUtility.IsReservedByQuestOrQuestBeingGenerated(pawn))
                {
                    continue;
                }
                if (pawn.guest != null && pawn.guest.HostFaction != null && !pawn.guest.HostFaction.IsPlayer)
                {
                    pawn.guest.SetGuestStatus(null);
                }
                if (pawn.mindState != null)
                {
                    pawn.mindState.WillJoinColonyIfRescued = false;
                }
                if (pawn.Downed)
                {
                    HealthUtility.HealNonPermanentInjuriesAndRestoreLegs(pawn);
                }
            }
        }
    }

    /// <summary>The vanilla transport pod crash, carrying a Legacy character with no faction to go home to.</summary>
    public class QuestNode_Root_LegacyPodCrash : QuestNode_Root_RefugeePodCrash
    {
        public override Pawn GeneratePawn_NewTemp(Map map)
        {
            var pawn = LegacyRoster.Pick();
            if (pawn.Faction != null)
            {
                pawn.SetFaction(null);
            }
            HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: false);
            pawn.guest.Recruitable = true;
            return pawn;
        }

        public override void SendLetter_NewTemp(Quest quest, Pawn pawn, Map map)
        {
            TaggedString title = "Legacy: " + pawn.LabelShortCap + " crash-lands";
            TaggedString text = $"A transport pod streaks out of the sky and slams into the ground nearby. Inside, badly hurt, is a face you know: {pawn.NameFullColored}.\n\n";
            text += "RefugeePodCrash_Factionless".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref title, pawn);
            Find.LetterStack.ReceiveLetter(title, text, LetterDefOf.NeutralEvent, new TargetInfo(pawn));
        }

        protected override bool TestRunInt(Slate slate) => LegacyRoster.AnyAvailable() && base.TestRunInt(slate);
    }

    /// <summary>The vanilla "wanderer wants to join" letter, from a Legacy character walking back in.</summary>
    public class QuestNode_Root_LegacyReturns : QuestNode_Root_WandererJoin_WalkIn
    {
        public override Pawn GeneratePawn_NewTemp(Map map) => LegacyRoster.Pick();

        protected override bool TestRunInt(Slate slate) => LegacyRoster.AnyAvailable() && base.TestRunInt(slate);
    }
}
