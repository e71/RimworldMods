# Animals Logic

Animals Logic is a set of independent animal tweaks, and most of them can be switched off in the mod settings. It adds:

- tags on animals in the trade and caravan screens (sex, number of trainings learned, wool growth)
- right-hand medical alerts for colony animals
- a configurable MTB for trained hauling
- configurable tameness-decay and training-decay thresholds
- a configurable chance that animals with toxic buildup rot when they die
- animals using nutrient paste dispensers
- predators that hunt your pawns are treated as hostile by turrets and colonists
- animals no longer eat random items
- wool from butchered shearable animals
- anesthetized explosive animals don't explode when killed
- younger animals are easier to tame and train
- obedient animals come to the handler for training
- a "play with animal" recreation
- a "hunting spree" major mental break
- info-card production stats for modded gatherable resources (venom, silk, VEF products)
- an option to show relations for unnamed animals

This is upstream code by Quicksilver Fox / Oblitus. All 1.7 work in this folder came from upstream (`origin` = quicksilverfox/RimworldMods). The user's fork (`e71`) has no AnimalsLogic-specific commits.

## Status

| | |
|---|---|
| Name / author | Animals Logic, `Oblitus` (git author: Quicksilver Fox) |
| packageId | `Oblitus.AnimalsLogic` |
| supportedVersions | 0.19, 1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7 |
| modDependencies | `brrainz.harmony` (Harmony, workshop 2009463077) |
| loadAfter | `brrainz.harmony` |
| incompatibleWith | `Daniledman.HardworkingAnimals` |
| Steam workshop id | `1098354593` (`About/PublishedFileId.txt`) |
| Harmony id | `net.quicksilverfox.rimworld.mod.animalslogic` |

The 1.7 port is done and in use on this machine:

- The committed `1.7/Assemblies/AnimalsLogic.dll` matches the current source. A Release rebuild gives the same size (68608 bytes) and identical metadata strings. The build is non-deterministic, so the bytes differ.
- The workshop copy at `~/.local/share/Steam/steamapps/workshop/content/294100/1098354593` is identical to this folder. `diff -rq` finds no differences, `Source/` included.
- That workshop copy is active in `ModsConfig.xml` (`oblitus.animalslogic`).
- The current `Player.log` (game 1.7.4995.309, Harmony library 3.0.0-rc.3) has no errors from this mod. Every transpiler logs an error when its anchor is missing, except the `TamenessCanDecay` one, which fails silently.
- The local `Mods/` folder has no copy of this mod.
- I checked the transpiler anchors against the 1.7.4995 `Assembly-CSharp.dll` IL (see "Harmony patches").
- The settings file `Config/Mod_1098354593_AnimalsLogic.xml` holds no values, so the user runs all defaults.

Features added or reworked during the 1.7 port (commits 68d6ffa through ede8ddd, Oct 2026):

- hunting spree: the think-tree job giver, the state worker, the break worker and the frequency setting
- the come-when-called rewrite
- modded production stats
- the 1.7 `FenceBlocked` change in the tameness detours

I don't know of any in-game test record beyond the clean startup log.

Code markers:

- `Patches/GetThemYoung.cs:15`: `TODO: make animals remember trainer and build up rapport with them.` This is a feature idea, not a bug.
- `Patches/ForgetMeNot.cs:52`: `This function is actually inlined and this patch is not working`. It refers to the body transpiler on `TrainableUtility.TamenessCanDecay`. The call-site redirects in the next point exist because of it.
- `Patches/ForgetMeNot.cs:70`: `this is workaround`. These are the call-site redirects in `TrainingTrackerTickRare` and `StatWorker_Wildness`.

`About.xml` `<description>` lists only the older features. It doesn't mention come-when-called, play with animals, hunting spree, production stats, young-animal taming, show relations, wool on butcher or no-boom slaughter.

## Folder layout

There is no `loadFolders.xml`, so the game uses its default. I checked this in `ModContentPack.InitLoadFolders`:

- it loads `1.7/` (or, if that folder is missing, the newest version folder that is not newer than the game)
- then `Common/` if present (there is none here)
- then the mod root

So on 1.7 the game loads the root (`About/`, `Languages/`) and `1.7/`.

```
About/About.xml, Preview.png, PublishedFileId.txt
Languages/<lang>/Keyed/Keys.xml   shared by every game version (English, Russian, ChineseSimplified, German, Spanish, SpanishLatin)
0.19/, 1.0/                       Assemblies/AnimalsLogic.dll + bundled 0Harmony.dll, Patches/patches.xml
1.1/ .. 1.5/                      Assemblies/AnimalsLogic.dll, Patches/patches.xml
1.6/                              Assemblies/AnimalsLogic.dll, Defs/ (job, joy, mental break/state), Patches/patches.xml
1.7/                              Assemblies/AnimalsLogic.dll (net10 build output), Defs/, Patches/patches.xml, Patches/ThinkTree_MentalState.xml
Source/AnimalsLogic.sln
Source/AnimalsLogic/              current C# source (1.7 only)
Source/MigrationBackup/           stale, see Build
```

- **0.19–1.6 hold prebuilt DLLs.** The current source doesn't build them. It uses 1.7-only APIs such as `Toil.AddPreTickIntervalAction(delta)`, `IsHashIntervalTick(..., delta)` and `Pawn.FenceBlocked`.
  - Target frameworks: .NET Framework 4.0 for 1.1–1.3, 4.8 for 1.4–1.6. The 0.19 and 1.0 DLLs carry no framework attribute.
  - 1.6 DLL: from commit 31b4658 (May 2026). It predates the hunting-spree fixes. Its MentalState drives the hunt itself, there is no `ThinkTree_MentalState.xml` in 1.6, and the 1.6 Defs have no `workerClass` entries. Later commits (5936f35, ede8ddd) still edited `1.6/Defs` text and commonality.
  - 1.4 and 1.5 DLLs: from a049870 (2024).
  - 1.3 DLL: from 18ba461 (2022).
  - 1.2 DLL: from dfd6234 (2021).
  - 1.1 DLL: from 5966793 (2020).
  - 0.19 and 1.0 DLLs: from 0ac59af (2020).
- **1.7 `Patches/patches.xml`** replaces the trained-Haul `ThinkNode_ChancePerHour_Constant` in `ThinkTreeDef[defName='Animal']` with `AnimalsLogic.ThinkNode_ChancePerHour_AnimalsHaulConfig`. It keeps the vanilla `ThinkNode_ConditionalRequireCapacities` (Manipulation) → `ThinkNode_ConditionalTrainableCompleted` (Haul) → `JobGiver_Haul` chain. The XPath selects the node by its `trainable` text, not by position. I checked that it matches `Data/Core/Defs/ThinkTreeDefs/Animal.xml` in 1.7.4995.
- **1.7 `Patches/ThinkTree_MentalState.xml`** adds a `ThinkNode_ConditionalMentalState` branch (`AL_HuntingWildAnimal`) to `MentalStateNonCritical/thinkRoot/subNodes`. The branch runs `JobGiver_HuntWildAnimal`, then eats or rests if starving or exhausted, then `JobGiver_WanderColony`.
- 0.19–1.5 use older positional XPaths for the haul node. Don't copy them forward.

## How it works

Entry point: `AnimalsLogic.cs`, class `AnimalsLogic : Mod`. The constructor does the following in order:

1. Creates the Harmony instance and runs `PatchAll()`, which applies the attribute patches.
2. Calls the manual patchers in this order: `AnimalsUseDispenser`, `NoBoomSlaughter`, `ShowAnimalRelations`, `NoToxicRot`, `HostilePredators`, `GetThemYoung`, `ForgetMeNot`.
3. Only then loads settings with `GetSettings<Settings>()`.

Every patch checks its `Settings.*` flag at run time. Toggling a setting never adds or removes a patch.

### Code map (`Source/AnimalsLogic/`)

| Area | Files |
|---|---|
| Entry, settings, DefOfs | `AnimalsLogic.cs`, `Settings.cs`, `ALDefOf.cs` (`ALJobDefOf.AL_PlayWithAnimal`, `ALJoyKindDefOf.AL_AnimalInteraction`, `ALMentalStateDefOf.AL_HuntingWildAnimal`), `GlobalSuppressions.cs`, `Properties/AssemblyInfo.cs` |
| Trade/caravan tags, relations | `Patches/AnimalsAreDifferent.cs` |
| Dispenser | `Patches/AnimalsUseDispenser.cs` |
| Come when called | `Patches/ComeWhenCalled.cs` (also used by the play job) |
| No random eating | `Patches/DoNotEatRandomly.cs` |
| Haul MTB think node | `Patches/FetchIt.cs` (`ThinkNode_ChancePerHour_AnimalsHaulConfig`, used from XML) |
| Tameness/training decay | `Patches/ForgetMeNot.cs` |
| Young animals easier | `Patches/GetThemYoung.cs` |
| Hostile predators | `Patches/HostilePredators.cs` |
| Modded production stats | `Patches/ModdedAnimalProduction.cs` |
| No-boom slaughter | `Patches/NoBoomSlaughter.cs` |
| Chicken meat, wool on butcher, toxic rot | `Patches/TastesLikeChicken.cs` (classes `TastesLikeChicken`, `NoToxicRot`) |
| Medical alerts | `MedicalAlerts/Alert_AnimalNeedsTend.cs`, `Alert_AnimalNeedsRescuing.cs`, `Alert_LifeThreateningHediffAnimal.cs`, `AnimalAlertsUtility.cs` |
| Hunting spree | `MentalBreaks/MentalBreakWorker_HuntingSpree.cs`, `MentalStateWorker_HuntWildAnimal.cs`, `MentalState_HuntWildAnimal.cs`, `JobGiver_HuntWildAnimal.cs` |
| Play with animal | `Recreation/JoyGiver_PlayWithAnimal.cs`, `Recreation/JobDriver_PlayWithAnimal.cs` |

### Harmony patches

"Attr" means applied by `PatchAll`. "Manual" means applied by a `Patch()` method from the constructor.

| Class | Target | Kind | Purpose |
|---|---|---|---|
| `Patch_Tradeable_Pawn_Label` (attr) | `Tradeable_Pawn.get_Label` | postfix | Prefixes animal labels in the trade screen with `[T<n>;W<pct>]`, i.e. number of learned trainables and wool fullness above 5%. Gated by `trade_tags`. |
| `Patch_TransferableOneWay_Label` (attr) | `TransferableOneWay.get_Label` | postfix | Same tags in the caravan and transport-pod screens, plus sex (`M`/`F`). |
| `ShowAnimalRelations` (manual) | `SocialCardUtility.ShouldShowPawnRelations` | postfix | Returns true for animals the player has seen that are unnamed or numerically named. Gated by `always_show_relations`. |
| `AnimalsUseDispenser.BestFoodSourcePatch` (manual) | `FoodUtility.BestFoodSourceOnMap` | transpiler | Replaces the first `RaceProperties.get_ToolUser` call with `ToolUser(prop) => prop.ToolUser \|\| use_dispenser`. It also rewrites `callvirt` to `call`. Logs `[AnimalsLogic] Unable to patch FoodUtility...` if the call is missing (present at IL 54 in 1.7.4995). |
| `AnimalsUseDispenser.PrepareToIngestToils_DispenserPrefix` (manual) | `JobDriver_Ingest.PrepareToIngestToils_Dispenser` (private, found by name) | prefix, skips original | For `WildManUtility.AnimalOrWildMan` pawns, replaces the toils: go to the dispenser, take a meal, walk to a nearby chew spot. Pawnmorpher former humans count here too. The description calls this a "detour". |
| `ComeWhenCalled.JobDriver_InteractAnimal_MakeNewToils_Patch` (attr) | `JobDriver_InteractAnimal.MakeNewToils` | postfix (wraps the enumerable) | Adds a pre-init action and a pre-tick-interval action to every toil of training and taming jobs. Each toil start (and every 200 ticks) summons an obedient animal with a `FollowClose` job. During toils with `debugName == "TalkToAnimal"` (string checked in 1.7.4995) the animal is held with `PawnUtility.ForceWait`. Gated by `come_when_called`, player humanlike handlers only. |
| `DoNotEatRandomly.JobGiver_EatRandom_TryGiveJob_Patch` (attr) | `JobGiver_EatRandom.TryGiveJob(Pawn)` | prefix, skips original | Returns no job for animals. Gated by `prevent_eating_stuff`. |
| `ForgetMeNot.TrainableUtility_TamenessCanDecay_Patch` (manual) | `TrainableUtility.TamenessCanDecay(ThingDef)` and `(Pawn)` | transpiler | Replaces the first `ldc.r4` (vanilla 0.101) with `ldsfld Settings.wildness_threshold_for_tameness_decay`. Logs nothing on failure. Its own comment says callers may inline it. |
| `ForgetMeNot.Pawn_TrainingTracker_TrainingTrackerTickRare_Patch` (attr) | `Pawn_TrainingTracker.TrainingTrackerTickRare` | transpiler | Workaround for the inlining. Redirects every call to either `TamenessCanDecay` overload to `TamenessCanDecay_Pawn_Detour` / `TamenessCanDecay_Thing_Detour`. These reproduce vanilla 1.7, including the `FenceBlocked` early-out, but use the setting. |
| `ForgetMeNot.StatWorker_Wildness_GetExplanationFinalizePart_Patch` (attr) | `StatWorker_Wildness.GetExplanationFinalizePart` | transpiler | Same redirect, so the wildness tooltip agrees. In 1.7.4995 this method calls both overloads. |
| `ForgetMeNot.TrainableUtility_DegradationPeriodTicks_Patch` (manual) | `TrainableUtility.DegradationPeriodTicks(ThingDef)` and `(Pawn)` | transpiler | Inserts `ldsfld training_decay_factor; div` before `Mathf.RoundToInt`. |
| `GetThemYoung.Interacted_Transpiler` (manual) | `InteractionWorker_RecruitAttempt.Interacted` | transpiler | After the first `<ldarg/ldloc animal>; ldsfld StatDefOf.Wildness; ldc.i4; ldc.i4; call GetStatValue`, pushes the animal again and calls `WildnessFactor(wildness, pawn)`. That multiplies by `min(age/matureAge, 1)^(4/3)`, where `matureAge` is the first life stage that is reproductive, milkable or shearable (else the last stage). Gated by `taming_age_factor`. |
| `GetThemYoung.TryTrain_Transpiler` (manual) | the compiler-generated lambda in `Toils_Interpersonal` that reads Wildness (found by scanning nested types) | transpiler | Same as above, for training. Logs a warning if the lambda can't be found. |
| `HostilePredators.HostileToThing_Postfix` (manual) | `GenHostility.HostileTo(Thing, Thing)` | postfix | If either side is a spawned predator pawn whose current `PredatorHunt` target belongs to the other thing's faction, or that faction `HasPredatorRecentlyAttackedAnyone(predator)`, the result becomes hostile. Works for turrets, not only pawns. Gated by `hostile_predators`. |
| `ModdedAnimalProduction` (attr) | `RaceProperties.SpecialDisplayStats` | postfix (concat) | For animals with a `CompHasGatherableBodyResource` subclass that is neither `CompMilkable` nor `CompShearable`, adds rows in the `AnimalProductivity` category starting at priority 9760. Rows: product type, amount, interval, per year, market value, value per year, extra items. See "Gotchas" for reflection. Gated by `extra_display_stats`. |
| `NoBoomSlaughter.Explosion_Prefix` (manual) | `PawnDied` on many `DeathActionWorker` types | prefix, skips original | Skips the death action if the corpse's pawn has `HediffDefOf.Anesthetic`. Which types get it is described in "Gotchas". |
| `TastesLikeChicken.Pawn_ButcherProducts_Patch` (attr) | `Pawn.ButcherProducts(Pawn, float)` | postfix | Always materializes the result to a list. With `tastes_like_chicken`, meat is swapped by flesh type: humanlike → `Meat_Human`, insectoid → `Meat_Megaspider`, normal → `Meat_Chicken`. `RawCHFood*` (Cosmic Horrors) is skipped. With `shear_corpses`, adds wool for shearable animals in a shearable life stage. Fullness is random 50–100% for non-player animals, the drop needs at least 10% fullness, and the amount is scaled by butcher efficiency. |
| `NoToxicRot.KillPatch` (manual) | `Pawn.Kill(DamageInfo?, Hediff)` | transpiler | Replaces the first `Hediff.get_Severity` call with `RotChance(h) = h.Severity * toxic_buildup_rot`. In 1.7.4995 that first call is the ToxicBuildup rot roll (`Rand.Value < toxic.Severity`). Logs `Animal Logic is unable to patch Pawn.Kill method.` on failure. |

### Defs (1.7)

- `JobDef AL_PlayWithAnimal`
  - driver: `AnimalsLogic.Recreation.JobDriver_PlayWithAnimal`
  - joy kind: `AL_AnimalInteraction`
  - one participant, casually interruptible
- `JoyKindDef AL_AnimalInteraction`
- `JoyGiverDef AL_PlayWithAnimal`
  - giver: `JoyGiver_PlayWithAnimal`
  - `baseChance` 2, `pctPawnsEverDo` 0.7
  - needs Moving and Manipulation
- `MentalBreakDef AL_HuntingSpree`
  - worker: `MentalBreakWorker_HuntingSpree`
  - Major, `baseCommonality` 0.2
- `MentalStateDef AL_HuntingWildAnimal`
  - state: `MentalState_HuntWildAnimal`; worker: `MentalStateWorker_HuntWildAnimal`
  - category Aggro, colonists only
  - recovery MTB 0.5 days, 5000–25000 ticks
  - letter `ThreatSmall`, `Catharsis` on recovery
  - Letter and label text is literal English in the Def. There are no `DefInjected` translations.

### Non-patch classes

**`ThinkNode_ChancePerHour_AnimalsHaulConfig`** (`FetchIt.cs`)
- MTB comes from `haul_mtb`. If it is 0 or less, it always runs its subnodes.
- Otherwise it rate-limits tries to `min(mtb*2500/2, 2500)` ticks, using `pawn.mindState.thinkData[UniqueSaveKey]`.
- It then rolls `Rand.MTBEventOccurs` with a per-hour seeded RNG.

**Medical alerts** (subclasses of `Alert`, found by the game automatically; all gated by `medical_alerts`)
- `Alert_AnimalNeedsTend` (High): colony animals with `HasHediffsNeedingTendByPlayer`, not in a medical bed, and not already covered by the rescue alert.
- `Alert_AnimalNeedsRescuing` (Critical): animals that pass `Alert_ColonistNeedsRescuing.NeedsRescue`.
- `Alert_LifeThreateningHediffAnimal` (Critical): animals with a life-threatening, non-immune hediff. It suggests amputation when the hediff is on a non-core part.
- Lists put bonded animals first.

**Hunting spree**
- `MentalBreakWorker_HuntingSpree`: blocks the break when `hunting_spree` is off and multiplies commonality by `hunting_spree_frequency`.
- `MentalStateWorker_HuntWildAnimal.StateCanOccur` requires:
  - the setting on
  - the pawn not incapable of violence
  - a valid target on the map
- A valid target (`MentalState_HuntWildAnimal.IsValidHuntTarget`) is:
  - a wild (no faction), spawned, non-forbidden, reachable animal
  - not in an aggro mental state
  - not `DeathActionWorker.DangerousInMelee` unless the hunter has a ranged weapon
  - `combatPower * (0.5 + manhunterOnDamageChance)` at or below 80 (ranged, `WorkGiver_HunterHunt.HasHuntingWeapon`), 45 (melee) or 25 (unarmed)
- The state keeps `targetAnimal` and retargets every 500 ticks while the target is invalid. It recovers when no target is left.
- `ForceHostileTo(Thing)` is true only for the target.
- `JobGiver_HuntWildAnimal` (from the think-tree XML) issues `JobDefOf.Hunt` with `ignoreDesignations = true` and `killIncappedTarget = true`.

**Play with animal**
- The joy giver needs:
  - a humanlike pawn, not incapable of Handling
  - candidate colony animals: not downed, not in a mental state, not forbidden, the handler's Animals skill at or above the animal's `MinimumHandlingSkill`, reachable with no danger
- Weighting:
  - bond ×5
  - obedient with this pawn as master ×4 (other obedient ×1.5)
  - `1 + 2*petness`
  - distance falloff
- With Animals skill 15 or higher, there is a 15% chance per call to also consider wild animals. They must be:
  - wildness 0.6 or less
  - skill at least `MinimumHandlingSkill + 2`
  - not a predator at half the pawn's body size or bigger
  - weighted ×0.3
- The driver:
  - summons obedient colony animals while the handler walks over
  - holds the animal with `ForceWait` for 2500 ticks
  - gives Animals XP per tick of `0.02 * (1 + minSkill/10 + wildness)`
  - every 240 ticks tries an `AnimalChat` interaction; for non-wild animals that also rolls a 0.4% bond chance
  - every 300 ticks shows a heart fleck
  - joy factor ×1.5 for bonded animals, × `(1 + 0.5*petness)`
- At the end it releases the animal's wait job. For a wild animal it also tries a `TameAttempt` interaction unless the animal was tamed too recently.

### Mod settings (`Settings.cs`; all `public static` fields, saved with `Scribe_Values`)

| Key | Default | Effect |
|---|---|---|
| `prevent_eating_stuff` | true | Animals get no `JobGiver_EatRandom` job. |
| `hostile_predators` | true | Hostile-predator postfix. |
| `tastes_like_chicken` | false | Meat swap on butcher. |
| `shear_corpses` | true | Wool on butcher. |
| `medical_alerts` | true | The three animal alerts. |
| `trade_tags` | true | Trade and caravan label tags. |
| `use_dispenser` | true | Non-tool-users may use dispensers. The dispenser toil override applies to animals regardless of this flag. |
| `always_show_relations` | false | Relations for unnamed animals. |
| `taming_age_factor` | true | Wildness scaled by age for taming and training. |
| `come_when_called` | true | Summon and hold during training, taming and play. |
| `play_with_animals` | true | Joy giver active. |
| `extra_display_stats` | true | Modded production rows on info cards. |
| `hunting_spree` | true | Break and state allowed. |
| `hunting_spree_frequency` | 1.0 (slider 0.1–3) | Commonality multiplier. Shown only while `hunting_spree` is on. |
| `wildness_threshold_for_tameness_decay` | 0.101 (0–1) | Tameness decays only above this wildness. 1 disables decay. |
| `training_decay_factor` | 1.0 (0.01–2) | Divides the decay period. Lower values mean slower decay. |
| `haul_mtb` | 1.5 h (0–3) | MTB of trained-haul think node. 0 means always. |
| `toxic_buildup_rot` | 1.0 (0–1) | Multiplier on the vanilla toxic-buildup rot chance. |

The settings file is `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config/Mod_<mod folder name>_AnimalsLogic.xml`. For the workshop copy that is `Mod_1098354593_AnimalsLogic.xml`. A local copy in `Mods/AnimalsLogic` would use a separate file, so settings don't carry over.

## Build

```bash
dotnet build -c Release AnimalsLogic/Source/AnimalsLogic/AnimalsLogic.csproj
```

(Run it from the repo root. `dotnet build AnimalsLogic/Source/AnimalsLogic.sln` also works and builds Debug by default.)

- Targets `net10.0` with `Krafs.Rimworld.Ref` 1.7.4977-alpha.1 and `Lib.Harmony` 3.0.0-rc.2, both `ExcludeAssets="runtime"`. At run time the game and the Harmony mod (workshop 2009463077) provide those assemblies. 1.7 runs on CoreCLR .NET 10.
- Output: `AnimalsLogic/1.7/Assemblies/AnimalsLogic.dll` and nothing else. There is no pdb in either configuration (`DebugType none`), no deps.json and no Harmony copy.
- `GenerateAssemblyInfo=false`. The project uses `Properties/AssemblyInfo.cs`, which has `AssemblyVersion("1.0.*")`, so `Deterministic=false` is required.
- The SDK-style project compiles every `*.cs` under `Source/AnimalsLogic/`. `Source/MigrationBackup/` is a sibling folder, so it is not compiled.
- **Building inside the repo overwrites the committed DLL.** To only check that it compiles, copy the mod folder to a scratch dir (`mktemp -d`) and build there. `obj/` is gitignored.
- Verified 2026-11-01 in a scratch copy, Release and the .sln Debug build: **0 warnings, 0 errors**.

`Source/MigrationBackup/50b815fb/AnimalsLogic/` is tracked in git (added in 31b4658). It is the backup Visual Studio's NuGet "packages.config → PackageReference" migration wrote:
- the old net48 `AnimalsLogic.csproj`, which output to `1.6/Assemblies` with HintPaths into a Windows Steam library
- `packages.config` (Lib.Harmony 2.3.1.1, Unity 5.11.10)
- `NuGetUpgradeLog.html`

No project or solution references it, and the build doesn't use it. It is safe to delete if you want, but the upstream author has never removed it. SyncGrowth's equivalent was deleted in d45eee8. `AnimalsLogic.sln` lists only the current project and has no stale entries. It uses the same project GUID `{674B538C-...}` as MyLittlePlanet's solution, which is harmless.

To test a local build in game, copy the mod (without `Source/`) into `~/.local/share/Steam/steamapps/common/RimWorld/Mods/`. The workshop copy (1098354593) is subscribed and active, and it has the same packageId. Disable or unsubscribe it so the game doesn't see a duplicate `Oblitus.AnimalsLogic`. Check the mod list or `Player.log` to see which copy actually loaded.

## Gotchas and known issues

- **The constructor patch order matters.** The manual `Patch()` calls are not wrapped in try/catch. If one throws, every later manual patch is skipped, and so is `GetSettings`, so saved settings would not load and defaults would apply. For example: Harmony failing on a modded `DeathActionWorker`, or `Array.Find` returning null for the private dispenser method.
- **Transpiler anchors.** All anchors were checked against the 1.7.4995 IL. After a game update, check `Player.log` for these messages:
  - `[AnimalsLogic] ...`
  - `AnimalsLogic: Failed to patch ...`
  - `Animals Logic: ...`
  - `Animal Logic is unable to patch Pawn.Kill method.`
  - the `TryTrain` warning

  Fragile points:
  - `NoToxicRot` takes the first `get_Severity` in `Pawn.Kill`.
  - `TamenessCanDecay` takes the first `ldc.r4` and fails silently.
  - `GetThemYoung` expects exactly `load; ldsfld Wildness; ldc.i4; ldc.i4; call GetStatValue`.
  - `GetThemYoung` also finds the training lambda by scanning `Toils_Interpersonal` nested types for Wildness + `GetStatValue`.
  - These transpilers compare with `CodeInstruction.Calls(...)` on purpose: on Harmony 3 / CoreCLR, comparing operands by reference is unreliable. Keep it that way.
- **`NoBoomSlaughter` patching scope.** It patches a hard-coded list:
  - vanilla `DeathActionWorker_BigExplosion`, `_SmallExplosion`, `_ToxCloud`, and `RimWorld.DeathActionWorker_AntigrainExplosion` if present
  - GeneticRim, MorrowRim and AlphaBehavioursAndEvents worker names, resolved with `AccessTools.TypeByName`, so missing mods are skipped

  It then scans every loaded assembly for non-abstract `DeathActionWorker` subclasses. It patches those whose type name contains `"Explosion"`. The other half of the check, `method.ToString().Contains("GenExplosion")`, tests the method signature and can never match. `GetMethod("PawnDied")` without `DeclaredOnly` can return an inherited method, so a base method may get the prefix more than once. The prefix binds the parameter by name (`corpse`), so a modded override with a different parameter name makes Harmony throw.
- **`ModdedAnimalProduction` uses reflection.**
  - It reads the protected `ResourceDef`, `ResourceAmount` and `GatherResourcesIntervalDays` getters of `CompHasGatherableBodyResource`.
  - For def-only info cards it instantiates a detached comp with `Activator.CreateInstance`.
  - VEF's `VEF.AnimalBehaviours.CompProperties_AnimalProduct` / `CompAnimalProduct` are resolved by name. Fields read: `resourceDef`, `resourceAmount`, `gatheringIntervalDays`, `customResourceString`, `isRandom`, `randomItems`, `seasonalItems`, `hasAditional` (sic), `additionalItemsProb`, `additionalItemsNumber`, `additionalItems`, `hideDisplayOnWildAnimals`, `seasonalItemIndex`.
  - If VEF renames these fields, the code silently falls back or shows nothing, because exceptions inside `Read` are swallowed.
- **`TastesLikeChicken`** calls `DefDatabase<ThingDef>.GetNamed` for `Meat_Human`, `Meat_Megaspider` and `Meat_Chicken`. `GetNamed` logs an error if a def is missing.
- **Savegames.** These go into saves:
  - `AnimalsLogic.MentalBreaks.MentalState_HuntWildAnimal` (fields `targetAnimal`, `ticksUntilRetarget`) on pawns in the state
  - `AL_PlayWithAnimal` jobs
  - the `AL_*` defs referenced by them

  Removing the mod while one of these is active gives load errors for those pawns. `JobDriver_PlayWithAnimal.animalWaitJob` is not saved. After a reload mid-play, the animal finishes its `ForceWait` instead of being released early. The haul node keeps an int in `mindState.thinkData` under its `UniqueSaveKey`. Changing the Animal think tree changes that key, which is harmless.
- **Settings binding.** The settings class is serialized as `AnimalsLogic.Settings`. Renaming it, or moving it to another namespace, loses users' saved settings. The fields are `static`, so they are read directly from transpiled IL (`ldsfld`) and must stay `public static`.
- **Translations.** Keyed strings live in root `Languages/` and are shared by every version folder.
  - English and Russian are complete: 68 keys, the same set.
  - ChineseSimplified is missing 36 keys. German, Spanish and SpanishLatin are each missing 42.
  - When you add or rename a key, update `Languages/English/Keyed/Keys.xml` and `Languages/Russian/Keyed/Keys.xml` together.
  - Removing a key can break old version DLLs: the 1.5 DLL still uses `ALConfigEggConversionLabel/Tooltip`, `AL_ResourceAmount_Desc` and `AL_ResourceType_Desc`, which are no longer in English.
  - Def text (mental state letter, job report string, labels) has no `DefInjected` translations.
- **1.6 vs 1.7 Defs.** The 1.6 Defs must stay consistent with the old 1.6 DLL: no `workerClass` entries and no think-tree patch. Don't sync 1.7 Defs into 1.6 without a 1.6 build.
- `About.xml` declares `incompatibleWith Daniledman.HardworkingAnimals`.

## Testing

Use a dev-mode test colony with this mod and Harmony. After startup, grep `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Logs/Player.log` for the error prefixes listed in "Gotchas". Then:

- **Settings:** Options → Mod settings → AnimalsLogic. Every row listed in the settings table should be present. The frequency slider shows only while hunting spree is on.
- **Trade tags:** open a trade with a caravan, or form a caravan with trained or woolly animals. Labels should start with `[T2;W60%]`, or `[F;T2]` in the caravan screen.
- **Alerts:** injure a colony animal, then down one, then give one a life-threatening disease. Check the "Animal needs treatment", "Animals need rescue" and "Medical emergency (animals)" alerts.
- **Dispenser:** a dispenser with paste, and hungry animals whose allowed area includes it. Animals should walk to the interaction cell, take a meal and step away to eat.
- **Haul MTB:** set it to 0 with Haul-trained animals and items to haul. Hauling should be near-continuous.
- **Tameness / training decay:** the Wildness stat explanation on the info card should match the threshold setting. Watch training decay with the factor at 1% and at 200%.
- **Young animals:** tame a juvenile and an adult of the same species and compare success rates. The `Interacted` patch scales the wildness used for the tame chance.
- **Come when called:** start training an obedient animal standing well away from the handler. The animal should jog over and stay put during each "TalkToAnimal" step.
- **Play with animal:** a pawn with joy need and Handling enabled, plus a tame animal. Look for the "playing with ..." report string and heart flecks.
- **Hunting spree:** trigger the mental break with dev tools on a colonist with a ranged weapon, with deer or boar on the map. The pawn should hunt them, should not target bears or wolves, and should recover when nothing valid is left. With only a melee weapon or no weapon, the pawn should also ignore boomalopes and boomrats.
- **Hostile predators:** let a predator hunt a colonist near a turret. The turret should fire.
- **No-boom:** anesthetize a boomalope (e.g. with a surgery bill), then kill it. It should not explode.
- **Toxic rot:** set the multiplier to 0% and kill an animal with high toxic buildup. The corpse should not rot instantly.
- **Butcher:** with chicken meat on, butcher any normal-flesh animal. With wool on, butcher a sheep. Check the products.
- **Production stats:** needs a mod animal with a custom gatherable comp (e.g. a VEF/Alpha Animals animal). The info card's "Animal productivity" section should show the `Product type` and amount, interval and value rows.
- **Relations:** enable "always show relations" and open the social tab of an unnamed animal with parents or children.
