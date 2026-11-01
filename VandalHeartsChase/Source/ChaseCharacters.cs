using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VandalHeartsChase
{
    public class ChaseSettings : ModSettings
    {
        public float visitorChance = 0.5f;
        public float traderChance = 0.35f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref visitorChance, "visitorChance", 0.5f);
            Scribe_Values.Look(ref traderChance, "traderChance", 0.35f);
        }
    }

    public class ChaseMod : Mod
    {
        public static ChaseSettings Settings;

        public ChaseMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ChaseSettings>();
            var harmony = new Harmony("ellerain.vandalheartschase");
            harmony.PatchAll(typeof(ChaseMod).Assembly);
            HospitalityPatch.TryPatch(harmony);
        }

        public override string SettingsCategory() => "Vandal Hearts Chase";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.Label("Each chase character rolls separately whenever a group from their faction arrives.");
            listing.Gap();
            listing.Label($"Chance to come with a visitor group: {Settings.visitorChance:P0}");
            Settings.visitorChance = listing.Slider(Settings.visitorChance, 0f, 1f);
            listing.Label($"Chance to come with a trade caravan: {Settings.traderChance:P0}");
            Settings.traderChance = listing.Slider(Settings.traderChance, 0f, 1f);
            listing.End();
        }
    }

    /// <summary>
    /// Prepare Carefully hands its world pawns to the game as "starting pawns left behind", which the game never
    /// reuses. This moves them into friendly factions so visitor and trader groups can bring them to the colony.
    /// </summary>
    public class ChaseTracker : GameComponent
    {
        public List<Pawn> chasePawns = new List<Pawn>();

        public ChaseTracker(Game game)
        {
        }

        public static ChaseTracker Instance => Current.Game?.GetComponent<ChaseTracker>();

        public override void StartedNewGame() => Adopt();

        public override void LoadedGame() => Adopt();

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 1717)
            {
                Rehome(sendLetter: false);
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref chasePawns, "chasePawns", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                chasePawns ??= new List<Pawn>();
                chasePawns.RemoveAll(p => p == null);
            }
        }

        private void Adopt()
        {
            foreach (var pawn in Find.WorldPawns.AllPawnsAlive)
            {
                if (pawn.wasLeftBehindStartingPawn && pawn.RaceProps.Humanlike && !chasePawns.Contains(pawn))
                {
                    chasePawns.Add(pawn);
                }
            }
            Rehome(sendLetter: true);
        }

        private static bool NeedsHome(Pawn pawn)
        {
            if (pawn.Dead || pawn.Destroyed || pawn.Spawned || !Find.WorldPawns.Contains(pawn))
            {
                return false;
            }
            var situation = Find.WorldPawns.GetSituation(pawn);
            if (situation == WorldPawnSituation.StartingPawnLeftBehind)
            {
                return true;
            }
            // Recruited chase characters are left alone; only re-home the ones whose faction disappeared.
            return situation == WorldPawnSituation.Free && (pawn.Faction == null || pawn.Faction.defeated);
        }

        private static IEnumerable<Faction> HomeCandidates(bool mustBeFriendly)
        {
            return Find.FactionManager.AllFactionsVisible.Where(f =>
                !f.IsPlayer && !f.defeated && !f.temporary && f.def.humanlikeFaction && !f.def.permanentEnemy
                && !f.def.HasRoyalTitles
                && f.def.pawnGroupMakers != null
                && f.def.pawnGroupMakers.Any(g => g.kindDef == PawnGroupKindDefOf.Peaceful || g.kindDef == PawnGroupKindDefOf.Trader)
                && (!mustBeFriendly || !f.HostileTo(Faction.OfPlayer)));
        }

        private void Rehome(bool sendLetter)
        {
            var homeless = chasePawns.Where(NeedsHome).ToList();
            if (homeless.Count == 0)
            {
                return;
            }
            var candidates = HomeCandidates(mustBeFriendly: true).ToList();
            if (candidates.Count == 0)
            {
                candidates = HomeCandidates(mustBeFriendly: false).ToList();
            }
            if (candidates.Count == 0)
            {
                return;
            }

            var report = new StringBuilder();
            foreach (var pawn in homeless.InRandomOrder())
            {
                // Spread the cast out: join whichever faction currently has the fewest of them.
                var faction = candidates
                    .OrderBy(f => chasePawns.Count(p => p != pawn && !p.Dead && p.Faction == f))
                    .ThenBy(_ => Rand.Value)
                    .First();
                pawn.wasLeftBehindStartingPawn = false;
                pawn.SetFaction(faction);
                if (faction.def.basicMemberKind != null)
                {
                    pawn.ChangeKind(faction.def.basicMemberKind);
                }
                report.AppendLine($"  {pawn.LabelShortCap}: {faction.Name}");
            }

            Log.Message("[Vandal Hearts Chase] Placed chase characters:\n" + report);
            if (sendLetter)
            {
                Find.LetterStack.ReceiveLetter("Old companions scattered",
                    "Your old companions have gone their separate ways. Watch for them among visitors and caravans:\n\n" + report,
                    LetterDefOf.NeutralEvent);
            }
        }

        /// <summary>Chase characters from this faction who are free to travel, each kept with the given chance.</summary>
        public static List<Pawn> PickJoiners(Faction faction, float chance, ICollection<Pawn> alreadyComing)
        {
            var result = new List<Pawn>();
            var tracker = Instance;
            if (tracker == null || faction == null || faction.IsPlayer || chance <= 0f)
            {
                return result;
            }
            foreach (var pawn in tracker.chasePawns)
            {
                if (pawn.Faction != faction || pawn.Dead || pawn.Destroyed || pawn.Spawned || pawn.Downed
                    || alreadyComing.Contains(pawn)
                    || !Find.WorldPawns.Contains(pawn)
                    || Find.WorldPawns.GetSituation(pawn) != WorldPawnSituation.Free
                    || QuestUtility.IsReservedByQuestOrQuestBeingGenerated(pawn))
                {
                    continue;
                }
                if (Rand.Chance(chance))
                {
                    result.Add(pawn);
                }
            }
            return result;
        }
    }

    /// <summary>Marks the window in which vanilla visitor/trader/traveler incidents generate the pawns they spawn.</summary>
    [HarmonyPatch(typeof(IncidentWorker_NeutralGroup), "SpawnPawns")]
    public static class NeutralGroupSpawnPatch
    {
        public static bool active;

        public static void Prefix() => active = true;

        public static Exception Finalizer(Exception __exception)
        {
            active = false;
            return __exception;
        }
    }

    [HarmonyPatch(typeof(PawnGroupMakerUtility), nameof(PawnGroupMakerUtility.GeneratePawns))]
    public static class GeneratePawnsPatch
    {
        public static void Postfix(PawnGroupMakerParms parms, ref IEnumerable<Pawn> __result)
        {
            if (!NeutralGroupSpawnPatch.active || parms?.faction == null)
            {
                return;
            }
            float chance;
            if (parms.groupKind == PawnGroupKindDefOf.Trader)
            {
                chance = ChaseMod.Settings.traderChance;
            }
            else if (parms.groupKind == PawnGroupKindDefOf.Peaceful)
            {
                chance = ChaseMod.Settings.visitorChance;
            }
            else
            {
                return;
            }
            var pawns = __result.ToList();
            pawns.AddRange(ChaseTracker.PickJoiners(parms.faction, chance, pawns));
            __result = pawns;
        }
    }

    /// <summary>Hospitality builds its visitor groups itself, so its group picker gets the same treatment.</summary>
    public static class HospitalityPatch
    {
        public static void TryPatch(Harmony harmony)
        {
            var method = AccessTools.Method("Hospitality.IncidentWorker_VisitorGroup:GetPawnsToSpawn");
            if (method == null)
            {
                return;
            }
            harmony.Patch(method, postfix: new HarmonyMethod(typeof(HospitalityPatch), nameof(Postfix)));
            Log.Message("[Vandal Hearts Chase] Hooked into Hospitality visitor groups.");
        }

        public static void Postfix(IncidentParms parms, ref IEnumerable<Pawn> __result)
        {
            if (parms?.faction == null)
            {
                return;
            }
            var pawns = __result.ToList();
            pawns.AddRange(ChaseTracker.PickJoiners(parms.faction, ChaseMod.Settings.visitorChance, pawns));
            __result = pawns;
        }
    }
}
