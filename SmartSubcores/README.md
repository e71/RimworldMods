# Smart Subcores (Oblitus.SubcoreAutomation) — developer handover

Smart Subcores lets the player install a mechanoid subcore (Biotech `SubcoreBasic` /
`SubcoreRegular` / `SubcoreHigh`) into vanilla buildings. A pawn does the installation
with Construction work, and the building then runs without an operator or gets extra
behaviour. Examples: ground-penetrating and long-range scanners, deep drills and the
hi-tech research bench run unmanned; generators switch on and off for backup power based
on battery levels; turrets get accuracy, aim-speed and friendly-fire bonuses; hydroponics
basins sow and harvest themselves and have a built-in sun lamp. Biotech, Ideology,
Royalty, Anomaly and Odyssey buildings also get features (growth vat, gene extractor,
biosculpter, mech charger/booster, band node, piloting console, proximity detector, and
more). Without Biotech, or with the "fallback" setting on, installation costs crafted
components instead of a subcore (see `SubcoreFallback`). The mod also adds two
"swappable" turret mounts that fire any handheld ranged weapon you install in them.

Everything below was checked against the files in this folder on 2026-11-01.

---

## 1. Status

| Item | Value |
|---|---|
| Name / author (About.xml) | Smart Subcores / **Oblitus** |
| Commits in this repo | Quicksilver Fox (`git log -- SmartSubcores`: `d0a4699`, `6be41ec` "Fallback mode for subcores", `cf15aec` "1.7 Smart Subcores. Needs testing.") |
| packageId | `Oblitus.SubcoreAutomation` |
| Harmony ID (main) | `quicksilverfox.SubcoreAutomation` (and about 20 more IDs, see §4.3) |
| supportedVersions | `1.6`, `1.7` |
| modDependencies | `Ludeon.RimWorld`, `brrainz.harmony` (workshop 2009463077) |
| loadAfter | `Ludeon.RimWorld`, `Ludeon.RimWorld.Biotech`, `brrainz.harmony`, `Dubwise.Rimefeller` |
| loadBefore | none |
| incompatibleWith | `kikohi.BetterGroundPenetratingScanner` |
| Steam workshop id | **none**: there is no `About/PublishedFileId.txt` |
| Textures | none of its own; it reuses vanilla texture paths (e.g. `Things/Building/Security/TurretMini_Base`) |
| Languages | English only (`Languages/English/Keyed/SubcoreAutomation.xml`, 188 keys). The mod-settings UI strings are hard-coded English in `SubcoreAutomationMod.cs`. |

Every `"Key".Translate()` call in `Source/` has a matching key in the Keyed file. The only
exceptions are 6 keys that belong to vanilla or Pawnmorpher (`AbilityNeedAllyToSkip`,
`CannotLaunchNotEnoughFuel`, `CannotLaunchOnCooldown`, `PMSelectAnimalToSequence`,
`SequencingProgress`, `SequencingComplete`).

### State of the 1.7 port

Commit `cf15aec` made these changes:
- copied `1.6/Defs`, `1.6/Patches` and `1.6/Mods` unchanged to `1.7/`. `diff -r 1.6 1.7` shows no differences except the DLL.
- added `<v1.7>` to `loadFolders.xml` and `1.7` to `supportedVersions`.
- switched the csproj from net48 / Krafs 1.6.* / Harmony 2.3.* to net10.0 / Krafs `1.7.4977-alpha.1` / Lib.Harmony `3.0.0-rc.2`. Output now goes to `..\1.7\Assemblies\`.
- made one API fix in `Patches/MechChargerPatches.cs`: `MechRepairUtility.RepairTick(mech, RepairDelta)` became `MechRepairUtility.RepairTick(mech)`, because 1.7 removed the delta parameter.
- committed `1.7/Assemblies/SubcoreAutomation.dll`.

What I verified:
- **Build**: clean, with 0 warnings and 0 errors (see §5). The rebuilt DLL is byte-identical to the committed `1.7/Assemblies/SubcoreAutomation.dll` (sha256 `e7ce7ed1…090b`), so the committed binary matches `Source/`.
- **Reflection and string-named Harmony targets**: about 120 members named by string (private fields, private methods, `AccessTools.TypeByName`, Odyssey types). I checked all of them against the installed game assembly (`~/.local/share/Steam/steamapps/common/RimWorld/Assembly-CSharp.dll`, `Version.txt` = **1.7.4995 rev309**) with a metadata reader, and every one exists. Some types are in a different namespace than you might guess: `Hediff_VatLearning` and `Hediff_BandNode` are in `Verse`, `MechanitorUtility` is in the global namespace, and `SteadyEnvironmentEffects` and `Toils_LayDown` are in `RimWorld`. The code handles this because it uses `typeof()` for those types, and a no-namespace fallback for `MechanitorUtility`. `CompPilotConsole.StartChoosingDestination_NewTemp(bool)` still exists in 1.7.4995.
- **Patches referenced with `nameof`/`typeof`**: these are checked by the compiler against Krafs ref 1.7.4977.

What I did **not** verify:
- **Nothing has been run in-game on 1.7.** The commit message says "Needs testing". The mod is not installed in the local `Mods` folder.
- Whether the XML patches still match 1.7 defs (e.g. that `GroundPenetratingScanner` still has a `PlaceWorker_NotUnderRoof`). Most operations are wrapped in `PatchOperationConditional` or `<success>Always</success>`, so a mismatch would quietly do nothing rather than error.
- Behavioural changes in 1.7 for code that compiles but whose semantics may have changed: tick model, `CompTick` vs `CompTickInterval`, power-net timing, mech charger internals, gravship launch flow.

TODO/FIXME/HACK markers: there are none in `Source/` or the XML. Unfinished or suspicious spots I found:
- `Core/SubcoreAutomationSettings.cs`: an orphaned doc comment, "When enabled, targeting a specific mineral in the scanner takes longer", sits directly above the `machineSettings` doc comment. A setting seems to have been started and then removed.
- `Patches/VanillaPatches.cs`: `CompFlickable_DoFlick_Patch` is a `[HarmonyPatch(typeof(CompFlickable), nameof(CompFlickable.DoFlick))]` class with no patch methods. Its comment says "nothing to do here". It is dead code and can be deleted.
- `Compat/RimefellerPatches.cs` uses the Harmony ID `"Pawnmorpher.SubcoreAutomation.Rimefeller"`, which looks like a copy-paste leftover.
- `CompUtilityAutomation`'s summary says it handles "TVs (3 tiers)", but no XML patch adds the comp to any television.
- `1.7/Defs/Example_ModCompatibility.xml.example` uses `<isTurret>true</isTurret>`. `isTurret` is a C# property, not a field, and RimWorld's XML loader only fills fields, so copying the example as-is will most likely fail. The shipped defs use `<category>Turret</category>` instead.
- **The 1.6 DLL is now stale with respect to `Source/`.** `1.6/Assemblies/SubcoreAutomation.dll` was last changed in `6be41ec`, when the csproj still targeted net48. Today the csproj only builds the 1.7 DLL, and the source uses the 1.7-only `RepairTick(mech)` signature. Any source fix you make reaches 1.7 only. To rebuild 1.6 you would need the old csproj (net48, `Krafs.Rimworld.Ref 1.6.*`, `Lib.Harmony 2.3.*`, OutputPath `..\1.6\Assemblies\`) and the `RepairTick(mech, 1)` call back.

---

## 2. Folder layout

```
SmartSubcores/
  About/About.xml            metadata (no PublishedFileId.txt, no Preview.png)
  loadFolders.xml
  Languages/English/Keyed/SubcoreAutomation.xml
  Source/                    C# project (SubcoreAutomation.csproj), 80 .cs files, 17,954 lines
  1.6/  Assemblies/SubcoreAutomation.dll (net48 build from 6be41ec), Defs/, Patches/, Mods/
  1.7/  Assemblies/SubcoreAutomation.dll (net10 build, matches Source/), Defs/, Patches/, Mods/
```

What `loadFolders.xml` loads (1.6 and 1.7 are symmetric):

| Version | Always | Only if mod active |
|---|---|---|
| v1.6 | `/`, `1.6` | `1.6/Mods/Pawnmorpher` (`tachyonite.pawnmorpherpublic`), `1.6/Mods/VNPE` (`VanillaExpanded.VNutrientE`), `1.6/Mods/BiotechGravship` (`als.biotechgravship`) |
| v1.7 | `/`, `1.7` | `1.7/Mods/Pawnmorpher`, `1.7/Mods/VNPE`, `1.7/Mods/BiotechGravship` (same packageIds) |

`/` contributes `About/` and `Languages/`. `Source/` is ignored by the game.

The contents of `1.7/` (identical to `1.6/`):

- `Defs/AutomatableMachines.xml`: 38 `SubcoreAutomation.Core.AutomatableMachineDef` entries, which drive the settings UI and per-machine efficiency. Defs whose `targetDefName` isn't loaded are hidden.
- `Defs/JobDefs.xml`: `SubcoreAutomation_InstallSubcore`, `SubcoreAutomation_InstallFallback`, `SubcoreAutomation_RemoveSubcore`.
- `Defs/WorkGiverDefs.xml`: `SubcoreAutomation_InstallSubcore` (Construction, prio 80), `SubcoreAutomation_RemoveSubcore` (Construction, 79), `SubcoreAutomation_InstallTurretWeapon` (Hauling, 90).
- `Defs/Hediffs.xml`: `SubcoreAutomation_MechBoosterBoost`, a "tactical relay" with fixed stat offsets that disappears after 180 ticks.
- `Defs/ThoughtDefs.xml`: `SubcoreAutomation_LucidDreams`, +8 mood for 1 day (sleep accelerator).
- `Defs/SwappableTurrets.xml`: `SubcoreAutomation_TurretMountSmall` (cost includes `SubcoreBasic`; research BasicMechtech + GunTurrets) and `SubcoreAutomation_TurretMountLarge` (`SubcoreRegular`; StandardMechtech + HeavyTurrets). Both have `MayRequire="Ludeon.RimWorld.Biotech"` and `thingClass` `SubcoreAutomation.Buildings.Building_SwappableTurret`.
- `Defs/Example_ModCompatibility.xml.example`: documentation only, not loaded.
- `Patches/*.xml`: add `CompProperties_*Automation` comps (and `<tickerType>Normal</tickerType>` where missing) to vanilla and DLC ThingDefs:
  - `Core_Patches.xml`: research bench, multi-analyzer, GPS, long-range scanner, deep drill, NPD, cooler, heater, moisture pump, 4 vanilla turrets, power switch, growth vat, band node, gene extractor, softscanner, ripscanner, basic/standard recharger, mech booster, hydroponics. It also **removes `PlaceWorker_NotUnderRoof` from `GroundPenetratingScanner`**.
  - `Generators_Patches.xml`: wood-fired, chemfuel and toxifier generators.
  - `Doors_Patches.xml`: Autodoor and SecurityDoor.
  - `Biosculpter_Patches.xml`, `VitalsMonitor_Patches.xml`, `Anomaly_Patches.xml` (ProximityDetector), `Odyssey_Patches.xml` (PilotConsole), `CommsConsole_Automation.xml`, `OrbitalTradeBeacon_Automation.xml`, `SleepAccelerator_Automation.xml`.
  - `Rimefeller_Patches.xml`: ResourceConsole, FuelPowerplant, LargeFuelPowerplant, each conditional.
  - `Biotech_PoluxTree_Patches.xml`: adds `CompProperties_PoluxWastepackConsumer` to `Plant_TreePolux`.
  - `HaulMechToCharger_Priority.xml`: sets `WorkGiverDef HaulMechsToCharger` `priorityInType` to 200 **globally**.
- `Mods/BiotechGravship/`: comps for `BfG_BasicRecharger`, `BfG_StandardRecharger`, `BfG_MechBooster`, plus an `AutomatableMachineDef` for `BfG_MechBooster`.
- `Mods/Pawnmorpher/`: comp for `PM_MutationScanner`.
- `Mods/VNPE/`: comps for `VNPE_NutrientPasteGrinder` and `VNPE_NutrientPasteFeeder`.

The XML sets the subcore tier per building with `<tier>Basic|Regular|High</tier>`. It also sets
`automatedSpeedFactor`, `automatedPowerConsumption` and `installWorkAmount`.

---

## 3. How it works

### 3.1 Startup sequence
1. `Core/SubcoreAutomationMod` (`Mod` subclass) loads `SubcoreAutomationSettings` in its constructor and detects `CETeam.CombatExtended` and `Mlie.TurnOnOffRePowered` with `ModsConfig.IsActive`.
2. `HarmonyInit` (`[StaticConstructorOnStartup]`):
   - calls `harmony.PatchAll()` with ID `quicksilverfox.SubcoreAutomation`, which applies every attribute-based patch.
   - **adds comps to defs at runtime**: `CompProperties_PowerAutomation` (Basic tier, 1500 work) goes to every ThingDef that has CompPowerPlant, CompFlickable and CompRefuelable and doesn't already have the comp. This is unconditional.
   - if `fallbackAutomationEnabled` is on, adds a generic `CompProperties_SubcoreAutomation` to every powered, flickable, non-generator building.
   - if `fallbackTurretAutomationEnabled` is on, adds `CompProperties_DefenseAutomation` to every buildable, non-decoy `Building_TurretGun`. The tier depends on footprint and on whether the turret costs ComponentSpacer.
3. Each manual patch class (§4.3) is a `[StaticConstructorOnStartup]` static class that reads settings and mod presence, then calls `harmony.Patch(...)` with its own Harmony ID. The settings these classes read take effect **only after a game restart**. The settings UI says so.
4. `ReflectionManifest`, `SubcoreAutomationUtils`, `DrillHandler` and `TurretPatches` cache `FieldInfo`/`MethodInfo` objects. `ReflectionManifest` logs a warning listing any lookups that failed.

### 3.2 Code map

**Core state and comps (`Source/Core/`)**
- `CompSubcoreAutomationBase` (854 lines) is the abstract base `ThingComp` and the class to read first. It holds the saved state `subcoreInstalled`, `installationRequested`, `removalRequested` and `basePowerConsumption`, and does the following:
  - install/remove request and complete logic, with refunds via `MakeRemovalYield`.
  - returns the subcore (or the fallback materials) when the building is deconstructed, but not when it is destroyed.
  - draws the install/cancel/remove gizmos, which are gated on `BasicMechtech` research (`Microelectronics` in fallback mode, or always in dev mode).
  - provides the "DEV: Install/Remove Subcore" gizmos (dev mode + god mode).
  - auto-completes flicks (`TryAutoFlick`).
  - `CanOperate()`, which checks power, the flick switch and breakdowns.
  - power overrides: `UpdatePowerConsumption` sets `PowerOutput = -(base + automatedPowerConsumption + extra)`, with special cases when RePowered is loaded.
  - `CompTick` and `CompTickRare` call the `DoMachineSpecificTick*` hooks.
- `CompProperties_SubcoreAutomation` defines `tier`, `automatedSpeedFactor`, `automatedPowerConsumption` and `installWorkAmount`. It also resolves the `SubcoreDef` ThingDef by name and caches it; this is null without Biotech.
- `CompProperties_Subclasses.cs` holds one CompProperties class per comp: `_ClimateAutomation`, `_DefenseAutomation`, `_ProductionAutomation`, `_UtilityAutomation`, `_PowerAutomation`, `_MechAutomation`, `_BiotechAutomation`, `_ScannerAutomation`, `_MiscAutomation`, `_CompatAutomation`.
- The concrete comps decide what a building does by comparing `parent.def.defName` against hard-coded names in `MachineDefNames.cs`:
  - `CompSubcoreAutomation` is the generic, behaviour-less comp, used by fallback automation and Pawnmorpher.
  - `CompClimateAutomation` (heater, cooler), `CompDefenseAutomation` (turrets, proximity detector), `CompProductionAutomation` (deep drill, hydroponics, moisture pump, toxifier; also implements `IThingGlower` for the sun lamp), `CompUtilityAutomation` (doors, NPD, vitals monitor), `CompPowerAutomation` (generators, power switch; battery min/max thresholds), `CompMechAutomation` (charger, booster, band node), `CompBiotechAutomation`, `CompScannerAutomation` (GPS / long-range scanner, target mineral, mountain-scan mode), `CompMiscAutomation` (comms console, trade beacon, sleep accelerator, pilot console / ship and shuttle pilot seats, research bench, multi-analyzer), `CompCompatAutomation` (VNPE grinder/feeder, Rimefeller console).
- `MapComponent_PowerBroker` is the map-wide backup-power controller, based on Fluffy's BackupPower (MIT). Generators register and deregister through `CompPowerAutomation`. It has no `ExposeData`.
- `AutomatableMachineDef` is the custom `Def` described in §3.3.
- `SubcoreFallback` handles fallback mode, which is active when `!ModsConfig.BiotechActive` or the `noBiotechFallbackMode` setting is on:
  - Basic tier: 2 ComponentIndustrial + 50 steel.
  - Regular tier: 2 ComponentIndustrial + 1 ComponentSpacer + 50 steel.
  - High tier: 2 ComponentSpacer + 50 plasteel.
  - Research: Microelectronics.
- `SubcoreTier` is the enum Basic/Regular/High. `SubcoreAutomationDefOf` holds the DefOf references to the 3 JobDefs and the hediff. `ReflectionManifest` is the central reflection cache. `InspectStringBuilder` and `IProductionAutomation` are helpers.
- `SubcoreAutomationSettings` and `SubcoreAutomationMod` hold the settings and the settings UI (§3.4).

**Handlers (`Source/Handlers/`)** contain static, per-feature logic that the comps call.
- They cover `BiotechHandler`, `ClimateHandler`, `CommsConsoleHandler`, `DefenseHandler`, `DrillHandler`, `MechHandler`, `PilotConsoleHandler`, `PowerHandler`, `ProductionHandler`, `ScannerHandler` and `UtilityHandler`, plus `ResearchBenchHandler`, which lives in `Patches/VanillaPatches.cs`.
- Runtime state that isn't saved is kept in static dictionaries keyed by comp, thing ID or map ID: drill progress, generator state, mountain-scan state, proximity-detector tracking, dispense state, and comms-console bonus-trader ticks.

**Jobs (`Source/Jobs/`)**
- `WorkGiver_InstallSubcore` scans `ThingRequestGroup.BuildingArtificial`. In fallback mode it builds an `InstallFallback` job that queues the material hauls; otherwise it builds an `InstallSubcore` job.
- `WorkGiver_RemoveSubcore` creates remove jobs.
- `WorkGiver_InstallTurretWeapon` issues vanilla `HaulToContainer` jobs for the swappable turrets.
- `JobDriver_InstallSubcore`, `JobDriver_InstallFallback` and `JobDriver_RemoveSubcore` carry out those jobs.
- Mechanoids are allowed through `ShouldSkip` if they can do Construction work.

**Buildings (`Source/Buildings/`)**
- `Building_SwappableTurret` subclasses `Building_TurretGun` and implements `IThingHolder`. It stores the installed weapon in a `ThingOwner`, rebuilds gun verbs by calling the private `Building_TurretGun.UpdateGunVerbs` through reflection, and draws the weapon itself (`TurretTopMaterial` is `BaseContent.ClearMat`). It has a "DEV: Install random weapon" gizmo.

**UI (`Source/UI/`)**
- `Command_ActionOverlay` is a gizmo that shows an overlay badge or tiled icons.
- `Command_BatteryRange` is the min/max battery slider gizmo, based on Fluffy's BackupPower (MIT).

**Compat (`Source/Compat/`)**: Pawnmorpher, Polux tree (Biotech), Rimefeller, VNPE, and WVC Work Modes. Details are in §4.3 and §6.

### 3.3 Custom Defs and comps

| Kind | Class | Notes |
|---|---|---|
| Def | `SubcoreAutomation.Core.AutomatableMachineDef` | Fields: `targetDefName`, `category` (`WorkBuilding`/`Turret`), `defaultEfficiency`, `defaultAccuracyBonus`, `defaultWarmupReduction`, `subcoreDef`. `isTurret` is a property kept for backwards compatibility, so don't use it in XML. It only drives the settings UI; the behaviour comes from the comp patched onto the ThingDef. |
| CompProperties | `CompProperties_SubcoreAutomation` and its 10 subclasses (§3.2) | Put on target ThingDefs by XML patches and by `HarmonyInit`. |
| CompProperties | `SubcoreAutomation.Compat.CompProperties_PoluxWastepackConsumer` → `CompPoluxWastepackConsumer` | Fields: `consumeRadius` 7.9, `consumeIntervalTicks` 150000, `maxWastepackStack` 5. |
| MapComponent | `MapComponent_PowerBroker` | Instantiated automatically by the game for every map. |
| ThingClass | `Building_SwappableTurret` | |
| WorkGivers | `WorkGiver_InstallSubcore`, `WorkGiver_RemoveSubcore`, `WorkGiver_InstallTurretWeapon` | |
| JobDrivers | `JobDriver_InstallSubcore`, `JobDriver_InstallFallback`, `JobDriver_RemoveSubcore` | |
| Gizmos | `Command_ActionOverlay`, `Command_BatteryRange` | |
| HediffDef / ThoughtDef | `SubcoreAutomation_MechBoosterBoost`, `SubcoreAutomation_LucidDreams` | |

The mod defines no DefModExtensions.

### 3.4 Mod settings (`SubcoreAutomationSettings`, saved as `Mod_<id>_SubcoreAutomationSettings.xml`)

All keys are listed below with their defaults. Settings marked **(R)** are read in static constructors or `Prepare()`, so they need a restart.

- Global: `permanentSubcoreInstallation`=false, `fallbackAutomationEnabled`=false **(R)**, `fallbackTurretAutomationEnabled`=false **(R)**, `noBiotechFallbackMode`=false.
- Biotech: `mechChargerPatchesEnabled`=true **(R)**, `geneExtractorFeaturesEnabled`=true, `softscannerFeaturesEnabled`=true, `ripscannerFeaturesEnabled`=true **(R, Prepare)**, `ripscannerOrganCount`=1, `scannerUISortingEnabled`=true, `mechSharedUftCraftingEnabled`=true, `mechBoosterFeaturesEnabled`=true, `mechBoosterCombatBonus`=0.15.
- Production / misc: `advancedFilteringEnabled`=true, `npdAnyStorageInputEnabled`=true, `hydroponicsFeaturesEnabled`=true, `hydroponicsDefaultSunLamp`=true, `deepDrillFullDepositEnabled`=true, `deepDrillFullDepositAutomatedOnly`=true, `commsConsoleFeaturesEnabled`=true, `commsConsoleBonusTraderDays`=15, `orbitalTradeBeaconFeaturesEnabled`=true, `sleepAcceleratorFeaturesEnabled`=true, `pilotConsoleFeaturesEnabled`=true **(R)**, `pilotConsoleInterventionChance`=0.5, `vnpeGrinderFeaturesEnabled`=true, `vnpeFeederFeaturesEnabled`=true, `poluxTreeFeaturesEnabled`=true, `moisturePumpSpeedMultiplier`=2, `allowMultipleResearchBenches`=false.
- Turrets: `turretPatchesEnabled`=true **(R)**, `turretBasicAccuracy`=0.10, `turretBasicWarmup`=0.10, `turretRegularAccuracy`=0.15, `turretRegularWarmup`=0.10, `turretHighAccuracy`=0.20, `turretHighWarmup`=0.15, `turretFriendlyFirePrevention`=true. All turret features are hidden and skipped when Combat Extended is active.
- Power: `backupPowerEnabled`=true, `backupPowerUpdateInterval`=60, `backupPowerMinimumOnTime`=300, `toxifierWastepackEnabled`=true.
- High-risk toggles: `growthVatEmbryoPatchEnabled`=true **(R)**, `vitalsMonitorSurgeryPatchEnabled`=true **(R)**, `biosculpterTuningPatchEnabled`=true **(R)**, `coolerInverterPatchEnabled`=true **(R)**.
- Per machine: `machineSettings` is a `Dictionary<string defName, MachineSettings>`, where `MachineSettings` holds `enabled`, `efficiency` (-1 means use the XML default), `accuracyBonus`, `warmupReduction` and `friendlyFirePrevention`. Turret bonuses actually come from the tier settings above, through the comp's `Props.tier`. The per-turret `accuracyBonus`, `warmupReduction` and `friendlyFirePrevention` fields are saved but nothing in `Source/` reads them.

---

## 4. Harmony patches

The patches fall into three groups:
- attribute patches applied by `PatchAll` under ID `quicksilverfox.SubcoreAutomation` (§4.1, §4.2).
- manual patches with their own IDs, applied from static constructors (§4.3).
- compat patches (§4.3).

Unless noted otherwise, each patch first checks that the building has a `CompSubcoreAutomationBase` with `SubcoreInstalled && IsAutomationEnabled`, and does nothing if not.

### 4.1 Attribute patches: `Patches/VanillaPatches.cs`
| Class | Target | Kind | Why |
|---|---|---|---|
| `WorkGiver_OperateScanner_HasJobOnThing_Patch` | `WorkGiver_OperateScanner.HasJobOnThing` | prefix (skip) | Stops pawns operating automated scanners. **Blocks mechanoids from scanners always.** |
| `WorkGiver_DeepDrill_HasJobOnThing_Patch` | `WorkGiver_DeepDrill.HasJobOnThing` | prefix (skip) | Same for deep drills. Blocks mechanoids always. |
| `WorkGiver_Researcher_HasJobOnThing_Patch` | `WorkGiver_Researcher.HasJobOnThing` | prefix (skip) | Same for research benches. Blocks mechanoids always. |
| `CompScanner_CanUseNow_Patch` | `CompScanner.CanUseNow` (getter) | postfix | Reports an automated scanner as usable. |
| `FlickUtility_UpdateFlickDesignation_Patch` | `FlickUtility.UpdateFlickDesignation` | prefix (skip) | Automated buildings flick instantly, so no designation is needed. |
| `CompScanner_CompInspectStringExtra_Patch` | `CompScanner.CompInspectStringExtra` | postfix, `Priority.Last` | Nulls the vanilla text so the comp can show its own. |
| `CompDeepDrill_CompInspectStringExtra_Patch` | `CompDeepDrill.CompInspectStringExtra` | postfix, `Priority.Last` | Same. |
| `CompDeepDrill_TryProducePortion_Patch` | `CompDeepDrill.TryProducePortion` (private) | postfix | Un-forbids a drill that vanilla forbade when the resource ran out. |
| `CompPowerTrader_SetUpPowerVars_Patch` | `CompPowerTrader.SetUpPowerVars` | postfix | Re-applies the automated power draw after vanilla resets it. Runs for **every** CompPowerTrader. |
| `CompDeepScanner_DoFind_Patch` | `CompDeepScanner.DoFind` (private) | prefix + postfix | Sets and clears the `[ThreadStatic] ScannerTargetOverride.OverrideMineral`. |
| `CompDeepScanner_ChooseLumpThingDef_Patch` | `CompDeepScanner.ChooseLumpThingDef` (private) | prefix (skip) | Returns the player-chosen mineral. |
| `CompFlickable_DoFlick_Patch` | `CompFlickable.DoFlick` | (none) | Empty class with no patch methods. Dead code. |
| `CompFlickable_CompGetGizmosExtra_Patch` | `CompFlickable.CompGetGizmosExtra` | postfix (wraps the enumerator) | Wraps the toggle action so automated buildings flick immediately. |
| `Alert_CannotBeUsedRoofed_GetReport_Patch` | `Alert_CannotBeUsedRoofed.GetReport` | postfix | Removes automated scanners from the alert's culprits. |

### 4.2 Other attribute patches
| File / class | Target | Kind | Gate | Why |
|---|---|---|---|---|
| `DoorPatches` `Patch_Building_Door_PawnCanOpen` | `Building_Door.PawnCanOpen` | postfix | — | Door lockdown: blocks prisoners, quest lodgers, etc. Hot path during pathfinding. |
| `DeepDrillFullDepositPatches` | `CompDeepDrill.GetNextResource` (private) | postfix | setting at runtime | Flood-fills the whole connected deposit and mines it from the outside in. |
| `MechBoosterFactionPatch` | `CompCauseHediff_AoE.IsPawnAffected` (private) | postfix | — | **Global vanilla bug-fix**: a booster with `onlyTargetMechs` no longer buffs hostile mechs. Applies with or without a subcore. |
| `MechBoosterPatches` `Patch_CanControlMechs` | `Pawn_MechanitorTracker.CanControlMechs` (getter) | postfix | Prepare: Biotech | A boosted booster works as a command relay when the mechanitor is away. |
| `MechBoosterPatches` `Patch_CanCommandTo` | `Pawn_MechanitorTracker.CanCommandTo` | postfix | Biotech | Allows commands within boosted-booster range. |
| `MechBoosterPatches` (relay overlay) | `SelectionDrawer.DrawSelectionOverlays` | postfix | Biotech | Draws relay ranges every frame while a mech is selected. |
| `BandNodePatches` `Patch_HediffBandNode_RecacheBandNodes` | `Hediff_BandNode.RecacheBandNodes` | prefix (replaces original) | Prepare: Biotech | Automated band nodes give +1 bandwidth by writing to the vanilla saved field `cachedTunedBandNodesCount` (see the class comment for why). |
| `MechBandAutoRebuildPatches` | `CompUseEffect_DestroySelf.DoDestroy` (private) | prefix + postfix | Biotech | Auto-rebuilds mechband antenna/dish after use. Global behaviour. |
| `GeneExtractorPatches` (3 nested classes) | `Building_GeneExtractor.IsContentsSuspended` (getter); `GeneUtility.ExtractXenogerm`; `Building_GeneExtractor.Finish` (private) | postfix; prefix + postfix; prefix + postfix | Biotech + setting | Hibernates the occupant, uses the minimum regrow time, and prefers genes not yet banked. |
| `ToxifierPatches` | `CompToxifier.PolluteNextCell` (private); `CompPowerPlant.UpdateDesiredPowerOutput` | prefix (skip); postfix | Biotech + setting | Makes wastepacks instead of polluting and reduces output. The postfix runs for every power plant. |
| `SubcoreSoftscannerPatches` | `Building_SubcoreScanner.EjectContents` | prefix + postfix | Biotech | Scanning sickness lasts 1 day less. |
| `SubcoreRipscannerPatches` | `Building_SubcoreScanner.EjectContents` | prefix + postfix | Prepare: Biotech **and** `ripscannerFeaturesEnabled` | Harvests N organs before death. |
| `SleepAcceleratorPatches` | `Toils_LayDown.FinalizeLayingJob` (private) | postfix | setting at runtime | Lucid-dreams thought and recreation. |
| `OrbitalTradeBeaconPatches` | `TradeShip.GiveSoldThingToPlayer`; `Building_OrbitalTradeBeacon.TradeableCellsAround` | prefix; prefix | setting at runtime | Teleports bought items to the beacon instead of drop pods; double range that ignores walls. |
| `FarskipPatches` `CompAbilityEffect_Farskip_Apply_Patch` | `CompAbilityEffect_Farskip.Apply` | prefix + postfix | — | Farskip lands at an automated band node or mech booster. |
| `FarskipPatches` `..._ShouldEnterMap_Patch` / `..._AlliedPawnOnMap_Patch` | `CompAbilityEffect_Farskip.ShouldEnterMap` / `AlliedPawnOnMap` (private) | postfix / postfix | Prepare: Royalty | Same feature. |
| `Compat/PoluxTreePatches` `PoluxTreeHarmonyPatches` | `CompDissolution.CanDissolveNow` (getter); `SteadyEnvironmentEffects.TryDoDeteriorate` (private); `Thing.CanStackWith`; `Map.FinalizeInit` | postfix; prefix; postfix; postfix | setting at runtime | Wastepacks near a Polux tree are preserved, can't be stacked onto, and are consumed. `FinalizeInit` **adds the comp at runtime to existing trees**. `Thing.CanStackWith` is a hot, global path. |

### 4.3 Manual patches (`harmony.Patch` in static constructors)
| Class (Harmony ID suffix) | Target(s) | Kind | Applied when | Why |
|---|---|---|---|---|
| `TurretPatches` (`.TurretPatches`) | `ShotReport.HitReportFor(Thing,Verb,LocalTargetInfo)`; `Building_TurretGun.TryStartShootSomething`; `.BurstCooldownTime`; `Verb_LaunchProjectile.TryCastShot`; `Projectile.CanHit`; `Building_TurretGun.CanSetForcedTarget` getter; `.CanToggleHoldFire` getter | prefix + postfix (Low); postfix (Low); postfix (Low); postfix (Low); prefix (Low); postfix; postfix | not CE **and** `turretPatchesEnabled` | Accuracy, warmup and cooldown bonuses, friendly-fire avoidance, forced target and hold fire on automated turrets. |
| `MechChargerPatches` (`.MechChargerPatches`) | `Building_MechCharger.Tick`, `StopCharging`, `StartCharging`; `JobGiver_GetEnergy.ShouldAutoRecharge`; `JobGiver_GetEnergy_Charger.TryGiveJob` | prefix + postfix; prefix; prefix; postfix; postfix | Biotech **and** `mechChargerPatchesEnabled` | 2x charge speed, repairs while charging, keeps downed mechs at the charger. |
| `MechBoosterPatches` (`.MechBoosterPatches.InMechanitorCommandRange`) | `MechanitorUtility.InMechanitorCommandRange` (global namespace) | postfix | Biotech | Cross-map relay. |
| `BiosculpterPatches` (`.BiosculpterPatches`) | `CompBiosculpterPod.CanAcceptOnceCycleChosen`, `BiotunedSpeedFactor` getter, `CannotUseNowPawnReason`, `SetBiotuned` | postfix (Low) ×3, prefix | Ideology **and** `biosculpterTuningPatchEnabled` | No biotuning needed; tuned speed for anyone. |
| `CoolerPatches` (`.CoolerPatches`) | `Building_Cooler.TickRare` | prefix | `coolerInverterPatchEnabled` | Inverter mode: the cooler heats when the room is too cold. |
| `DispenserPatches` (`.DispenserPatches`) | `GenTemperature.GetTemperatureForCell(IntVec3,Map)`; `Building_NutrientPasteDispenser.TryDispenseFood`; `.FindFeedInAnyHopper`; `.HasEnoughFeedstockInHoppers` | prefix; postfix; prefix; prefix | always | Refrigerates hoppers, strips ingredients from meals, feeds from any adjacent storage. `GetTemperatureForCell` is a very hot path. |
| `GrowthVatPatches` (`.GrowthVatPatches`) | `Building_GrowthVat.EmbryoBirth` (only if `growthVatEmbryoPatchEnabled`); `Hediff_VatLearning.Learn`; `HediffComp_SeverityPerDay.SeverityChangePerDay` | prefix; prefix; postfix | Biotech | 85% birth quality; passion-focused learning at 4000 XP/day. The `SeverityChangePerDay` postfix runs for every hediff with that comp. |
| `HydroponicsPatches` (`.HydroponicsPatches`) | `WorkGiver_GrowerSow.JobOnCell`; `WorkGiver_GrowerHarvest.HasJobOnCell` | prefix; prefix | always | Keeps colonists off automated basins (looked up in a per-map cell cache). |
| `MoisturePumpPatches` (`.MoisturePumpPatches`) | `CompTerrainPumpDry.CompTickRare`; `CompTerrainPump.CompInspectStringExtra`; `.PostDrawExtraSelectionOverlays` | prefix ×3 | always | Pumps targeted cells, auto-deconstructs when done, shows the full radius. |
| `PilotConsolePatches` (`.PilotConsolePatches`) | `Verse.WorldComponent_GravshipController.LandingEnded` | prefix | `Ludeon.RimWorld.Odyssey` **and** `pilotConsoleFeaturesEnabled` | Chance to cancel a landing mishap. Odyssey types are looked up by name. `PilotConsoleHandler` calls `CompPilotConsole.StartChoosingDestination_NewTemp(bool)` through reflection, and falls back to `StartChoosingDestination()`. |
| `ProximityDetectorPatches` (`.ProximityDetectorPatches`) | `InvisibilityUtility.IsPsychologicallyInvisible`; `ShotReport.HitReportFor` | postfix; postfix | always | Tracked invisible pawns become targetable, with a ×0.3 accuracy factor. **Both postfixes iterate `allBuildingsColonist` on every call**, which is a perf risk. |
| `RePoweredCompatPatches` (`.RePoweredCompat`) | `ReservationManager.IsReservedByAnyoneOf(LocalTargetInfo,Faction)` | postfix | `Mlie.TurnOnOffRePowered` | Makes RePowered treat automated buildings as in use. |
| `VitalsMonitorPatches` (`.VitalsMonitorPatches`) | `SurgeryOutcomeSuccess.Apply` | prefix (Low) | `vitalsMonitorSurgeryPatchEnabled` | Surgery always succeeds at ≥98% quality with an automated monitor. |
| `SubcoreScannerUIPatches` (`.SubcoreScannerUI`) | every `FloatMenu` constructor whose first parameter is `List<FloatMenuOption>` | prefix | Biotech | Sorts and colours pawn options while a subcore scanner is selected. Runs for every float menu. |
| `MechSharedUftPatches` (`.MechSharedUft`) | `WorkGiver_DoBill.ClosestUnfinishedThingForBill`, `.FinishUftJob` (private) | postfix; prefix | always (setting checked at runtime) | Any mech can finish another mech's unfinished thing. Global behaviour; no subcore needed. |
| `FarskipPatches` (`.FarskipPatches`) | `CompAbilityEffect_Farskip.CanApplyOn(GlobalTargetInfo)`, `.Valid(GlobalTargetInfo,bool)`, `.WorldMapExtraLabel` | postfix ×3 | Royalty | Beacon targets. |
| `Compat/RimefellerPatches` (`Pawnmorpher.SubcoreAutomation.Rimefeller`) | `Rimefeller.Building_ResourceConsole.Manned` getter; `HasJobOnThing` on every Rimefeller `WorkGiver` whose type name contains "Console" or "Resource" | postfix; prefix | type `Rimefeller.Building_ResourceConsole` found | Unmanned resource console. The work-giver match is a name heuristic, and `AccessTools.Method` also finds inherited methods, so it could patch a base `HasJobOnThing`. |
| `Compat/WVCWorkModesPatches` (`.WVCWorkModesCompat`) | `Pawn.TickRare` | postfix | `wvc.sergkart.biotech.MoreMechanoidsWorkModes` + Biotech + `mechChargerPatchesEnabled` | Sends damaged mechs to automated chargers, because WVC replaces the vanilla job givers. |
| `Compat/VNPEPatches` (`.VNPEPatches`) | `GenTemperature.GetTemperatureForCell`; `VNPE.Building_NutrientGrinder.TryProducePaste`; `CompIngredients.RegisterIngredient` | prefix; prefix + postfix; prefix | `VanillaExpanded.VNutrientE` | Hopper refrigeration and ingredient stripping. Also reflects on `PipeSystem.CompResource.PipeNet`, the grinder's `cachedHoppers` field and the pipe net's `storages`. |
| `Compat/PawnmorpherPatches` (`Pawnmorpher.SubcoreAutomation.Compat`) | `Pawnmorph.ThingComps.MutationSequencerComp.CanUseNow` getter, `.Used`, `.CompInspectStringExtra`; **`Verse.ThingComp.CompTick` (base virtual)** | postfix; prefix; prefix; postfix | `tachyonite.pawnmorpherpublic` | Automated mutation sequencer. The `ThingComp.CompTick` postfix runs for every comp that doesn't override `CompTick`, which is a global perf cost. Also reflects on `ChamberDatabase`, `DatabaseUtilities.GetAllMutationsFrom` and `PawnmorpherSettings.SequencingMultiplier`. |

Hot paths, worth profiling if you see TPS drops:
- `GenTemperature.GetTemperatureForCell`: patched twice when VNPE is loaded.
- `Thing.CanStackWith`
- `Building_Door.PawnCanOpen`
- `ShotReport.HitReportFor`: patched twice.
- `InvisibilityUtility.IsPsychologicallyInvisible`
- `CompPowerTrader.SetUpPowerVars`
- `ThingComp.CompTick`: only with Pawnmorpher.
- every `FloatMenu` constructor.

---

## 5. Build

Toolchain: .NET SDK 10.0.113 at `/usr/share/dotnet`. The csproj targets `net10.0`, `LangVersion latest`, with `Nullable` and `ImplicitUsings` disabled and `AllowUnsafeBlocks`. Packages:

```xml
<PackageReference Include="Krafs.Rimworld.Ref" Version="1.7.4977-alpha.1" ExcludeAssets="runtime" />
<PackageReference Include="Lib.Harmony"        Version="3.0.0-rc.2"        ExcludeAssets="runtime" />
```

Neither assembly is copied to the output (`ExcludeAssets="runtime"`, `CopyLocalLockFileAssemblies=false`), because the game and the Harmony mod provide them at runtime. **The project does not use Krafs.Publicizer.** All private access goes through `AccessTools` reflection or Harmony string targets (§4), even though the package is in `~/.nuget/packages`.

From the repo root (`/home/ellerain/src/Rimworld/RimworldMods`):

```sh
dotnet build -c Release SmartSubcores/Source/SubcoreAutomation.csproj
```

The output is `SmartSubcores/1.7/Assemblies/SubcoreAutomation.dll`. There are no pdb, deps.json or ref assemblies because `DebugType none` and `GenerateDependencyFile false` are set. Intermediates go to `SmartSubcores/Source/obj/`, which is git-ignored by the repo's `.gitignore` (`[Oo]bj/`). **Building in place overwrites the committed DLL.** That's the intended way to ship a change, but don't rebuild by accident before you commit unrelated work.

Verified on 2026-11-01: I copied the mod folder to `/tmp/claude-1000/smartsub.9BXH/` and built it there.
- Plain build and `--no-incremental` both gave **Build succeeded, 0 Warning(s), 0 Error(s)**, in about 6 s.
- The output DLL is byte-identical to the committed `1.7/Assemblies/SubcoreAutomation.dll`.

There is no build target for 1.6 (see §1).

Runtime environment: RimWorld 1.7 on Linux at `~/.local/share/Steam/steamapps/common/RimWorld`. The installed `Version.txt` says `1.7.4995 rev309`.
- 1.7 runs on CoreCLR .NET 10, so mods ship net10 DLLs in `1.7/Assemblies`. The game's `Assembly-CSharp.dll` is in the game root.
- Harmony comes from workshop item 2009463077 (`.../workshop/content/294100/2009463077/1.7/Assemblies/0Harmony.dll`).

---

## 6. Gotchas and known issues

**Compatibility**
- **Combat Extended** (`CETeam.CombatExtended`): all turret patches are skipped and turret settings are hidden. The turret comps are still added, so install and remove still work, but they give no bonuses.
- **Turn It On and Off – RePowered** (`Mlie.TurnOnOffRePowered`): `UpdatePowerConsumption` stops writing `PowerOutput` in some states, and `IsReservedByAnyoneOf` is faked so RePowered draws full power.
- **Rimefeller**: detected by type name rather than packageId. It also appears in `loadAfter` (`Dubwise.Rimefeller`).
- **Better Ground-Penetrating Scanner** (`kikohi.BetterGroundPenetratingScanner`) is marked incompatible; its features are built in.
- Optional mods with XML content in `Mods/`: Pawnmorpher, VNPE, Biotech for Gravship (`als.biotechgravship`). WVC Work Modes is code-only.
- The XML adds `<tickerType>Normal</tickerType>` to defs that lacked one (research bench, scanners, deep drill, NPD, comms console, and others). This makes every instance of those buildings tick every frame, which also affects other mods that expect the original ticker type.
- Some changes apply globally, even without a subcore:
  - mechanoids are blocked from operating scanners, deep drills and research benches.
  - `MechBoosterFactionPatch`
  - `MechSharedUftPatches`
  - `MechBandAutoRebuildPatches`
  - `HaulMechsToCharger` priority set to 200.
  - the GPS can be built under a roof (`PlaceWorker_NotUnderRoof` removed).
  - deep-drill full-deposit mode, when `deepDrillFullDepositAutomatedOnly`=false.
  - backup-power comps on every refuelable, flickable power plant.

**Reflection on other mods.** `PawnmorpherPatches`, `VNPEPatches`, `RimefellerPatches`, `PilotConsolePatches` and `PilotConsoleHandler` look up types, private fields and private methods by string. When the other mod updates, these break **silently**, or at most produce a `Log.Error`/`Log.Warning` that starts with `[SubcoreAutomation]`. Check the log after updating any of those mods.

**Things a newcomer is likely to break**
- Behaviour is chosen by **hard-coded defName** (`MachineDefNames.cs` and `defName ==` checks inside the comps). If you add a comp to a modded ThingDef through XML, it gets only the generic install/remove/flick behaviour unless the code knows its defName. Turrets are the exception, because the turret patches key off the comp, not the defName.
- `AutomatableMachineDef` does nothing by itself; it only affects the settings UI. The ThingDef also needs a `CompProperties_*Automation` patch.
- In XML use `<category>Turret</category>`, not `<isTurret>`.
- Many settings only take effect after a restart, because the patches are applied once in static constructors. If you add a toggle, decide whether it is checked at runtime or at patch time, and say so in the UI.
- `CompPowerTrader_SetUpPowerVars_Patch` and `UpdatePowerConsumption` overwrite `PowerOutput`. Other mods that change power draw will conflict.
- There are about 20 separate Harmony IDs. If you want "unpatch everything with my ID", or to check Harmony logs by owner, look under all of them (§4.3).
- Static caches keyed by `map.uniqueID` are **never reset when another save is loaded**: `HydroponicsPatches._automatedCells`, `DispenserPatches._refrigeratedCells`, the `VNPEPatches` caches and `CommsConsoleHandler`. Map IDs repeat between saves (usually 0). After loading a second save in the same session, stale cells can block sowing or harvesting on plain basins, or keep hopper cells cold. Hydroponics cells are also not unregistered when the basin is minified, because they are only removed on destroy or subcore removal. This is a likely bug that I haven't reproduced.
- When you change `Source/`, rebuild so the 1.7 DLL is updated, and remember that 1.6 does **not** get the change.

**Savegame compatibility**
- Saved per-building state lives in the comps:
  - base: `subcoreInstalled`, `installationRequested`, `removalRequested`, `basePowerConsumption`.
  - `CompPowerAutomation`: `backupPowerBatteryMin`, `backupPowerBatteryMax`, `backupPowerRunOnBatteriesOnly`, `lastGeneratorOnTick`.
  - `CompScannerAutomation`: `lastScanTick`, `targetMineral`, `scannerMode`, `mountainTilesProcessed`.
  - `CompProductionAutomation`: `drillProgress`, `sunLampEnabled`.
  - `CompUtilityAutomation`: `lockForNonColony`, `continuousDispenseMode`, `lastDispenseAttemptTick`, `advancedFilteringEnabled`.
  - `CompCompatAutomation`: `advancedFilteringEnabled`.
  - `Building_SwappableTurret`: its container and weapons.
  - Polux comp: `ticksUntilConsume`, `boundWastepacks`.

  **Renaming any of these keys or classes breaks existing saves.**
- What a building has installed is saved only as a bool, not as "subcore vs. materials". If you toggle fallback mode, or enable or disable Biotech, between installing and removing, removal pays out the *current* mode's yield: a subcore can become components, or the reverse.
- Comps added by `HarmonyInit` (all fuel generators, plus the optional fallback flickables and turrets) exist only while that code and setting are active. If you turn the fallback settings off, the comps disappear from those defs on the next start. Their saved install state is then dropped without a refund.
- `BandNodePatches` folds the +1 bonus into vanilla's **saved** `Hediff_BandNode.cachedTunedBandNodesCount`. That value can stay in the save until vanilla recaches it.
- Removing the mod from a save produces load errors for `SubcoreAutomation.Core.MapComponent_PowerBroker` (saved with every map), the swappable-turret buildings (their thingClass disappears, along with any held weapons), the hediff and thought defs, and the comp data. These errors are mostly non-fatal, but players should be warned.
- `Map.FinalizeInit` adds the Polux comp at runtime to trees that existed before the mod was added.

---

## 7. Testing in-game

1. Build (§5), then make the mod visible to the game. It is **not** currently in `~/.local/share/Steam/steamapps/common/RimWorld/Mods`. For example:
   `ln -s /home/ellerain/src/Rimworld/RimworldMods/SmartSubcores ~/.local/share/Steam/steamapps/common/RimWorld/Mods/SmartSubcores`
   Load order: Harmony, Core, DLCs, (Rimefeller), Smart Subcores.
2. Start the game and read `Player.log` for lines with `[SubcoreAutomation]` or `[SmartSubcores]`. Expected messages are the "Scanner UI sorting patches applied (N constructors)", "Piloting Console patches applied (Odyssey DLC)" and RePowered info lines. Anything containing "BROKEN" or "ReflectionManifest: N lookups failed" is a port problem.
3. Normal play: research **Basic mechtech** (Biotech). Without Biotech or in fallback mode, research **Microelectronics** instead. Have a `SubcoreBasic`/`Regular`/`High` on the map (craft it at the subcore encoder), or the fallback materials. Select a supported building and use **Request installation**. A pawn with Construction enabled installs it, and the building's inspect string changes.
4. Dev shortcuts:
   - With dev mode on, the install gizmos appear without the research.
   - With dev mode + **god mode**, buildings have "DEV: Install Subcore" and "DEV: Remove Subcore", and swappable turrets have "DEV: Install random weapon".
   - With "show dev gizmos", Polux trees have "DEV: Consume wastepack now" and "DEV: Update bindings".
   - `PowerHandler` writes backup-power debug lines to the log every 250 ticks while dev mode is on.
   - The mod defines no `[DebugAction]` entries.
5. Suggested smoke test for the 1.7 port, covering the areas most likely to have changed:
   - backup power: chemfuel generator + battery, using the min/max gizmo.
   - automated GPS: mineral target and mountain mode.
   - deep drill.
   - hydroponics: sow, harvest, sun lamp.
   - turret bonuses and friendly fire.
   - mech charger: repairing a downed mech.
   - band node bandwidth after save and load.
   - NPD with "any adjacent storage".
   - Odyssey pilot console autopilot and landing.
   - swappable turret weapon install.
   - fallback mode: toggle `noBiotechFallbackMode`, restart, install with components.
   - save, then reload in the same session, to check the stale static caches described in §6.
