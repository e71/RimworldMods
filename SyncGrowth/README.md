# Sync Growth

Sync Growth evens out the growth of planted crops so that a field or hydroponics bed is ready to harvest at the same time. It doesn't set growth directly. Instead it groups neighbouring plants of the same crop and scales each plant's growth rate: plants behind the group average grow faster and plants ahead grow slower, so all of them reach full growth at about the same moment. The inspect pane shows the adjustment next to the vanilla growth rate, e.g. `Growth rate: 85% (+12%)`. The mod is loosely based on "WM Sync growth".

This is upstream code by Quicksilver Fox / Oblitus. All 1.7 work came from upstream (`origin` = quicksilverfox/RimworldMods), and the user's fork (`e71`) has no SyncGrowth-specific commits.

## Status

| | |
|---|---|
| Name / author | Sync Growth, `Oblitus` (git author: Quicksilver Fox) |
| packageId | `Oblitus.SyncGrowth` |
| supportedVersions | 1.2, 1.3, 1.4, 1.5, 1.6, 1.7 |
| modDependencies | `brrainz.harmony` (Harmony, workshop 2009463077) |
| loadAfter | `brrainz.harmony`, `dianawinters.Orassans` |
| incompatibleWith | `neceros.plantgrowthsync` |
| Steam workshop id | `2392562136` (`About/PublishedFileId.txt`) |
| Harmony id | `Oblitus.SyncGrowth` |

The 1.7 state comes from two upstream commits on 2026-10-14:

- d45eee8 "1.7 SyncGrowth": SDK-style net10 csproj; deleted the old MigrationBackup, `app.config` and `Properties/` resources.
- 926430c "SyncGrowth improvements": a rewrite. It removed `GroupsUtils.cs`, `MapInterface.cs`, `Patches/GrowthRate.cs` and `Patches/Root.cs`, and added `Patches/GrowthPerTick.cs`.

Facts about the 1.7 DLL and test status:

- The committed `1.7/Assemblies/SyncGrowth.dll` matches the current source. A rebuild gives the same size (18432 bytes) and the same metadata strings.
- The only difference is the SDK-generated InformationalVersion `1.0.0+73ae917...`, which records the git commit the DLL was built from.
- The mod is not installed here: no workshop folder, not in `Mods/`, not in `ModsConfig.xml`. The 1.7 build has never run on this machine.
- I checked the patch targets against the 1.7.4995 `Assembly-CSharp.dll`. `Plant.get_GrowthPerTick` exists and is called from `Plant.TickLong` and `Plant.get_TicksUntilFullyGrown`. `Plant.GetInspectString` builds the line as `"GrowthRate".Translate() + ": " + GrowthRate.ToStringPercent()`.

There are no TODO, FIXME or HACK markers in the source.

## Folder layout

There is no `loadFolders.xml`. The game's default (checked in `ModContentPack.InitLoadFolders`) loads, in this order:

- `1.7/`, or if that folder is missing, the newest version folder that is not newer than the game
- `Common/` if present (there is none here)
- the mod root

```
About/About.xml, Preview.png, PublishedFileId.txt
Languages/English/Keyed/Keys.xml       settings strings (English only)
1.2/ .. 1.6/Assemblies/SyncGrowth.dll  prebuilt for older game versions (.NET Framework 4.7.2 for 1.2-1.4, 4.8 for 1.5-1.6), not built from current source
1.2, 1.3, 1.5, 1.6/Assemblies/SyncGrowth.dll.config   leftover app.config binding redirects from those builds
1.7/Assemblies/SyncGrowth.dll          net10 build output of Source/
Source/SyncGrowth.sln, Source/SyncGrowth.csproj, Source/*.cs
```

- There are no Defs or XML patches in any version folder. Everything is C#.
- Where the old DLLs come from:
  - 1.2 and 1.3: cb3af71 (2021)
  - 1.4: a049870 (2024)
  - 1.5: 01dad75 (2025)
  - 1.6: 09d7cb7 (2025)
- All of them predate the 926430c rewrite, so they contain the old grouping code. Behaviour on those versions differs from what this README describes.
- The `.dll.config` files are build leftovers. RimWorld doesn't use them.

## How it works

The entry point is `ModController.cs`, class `SyncGrowth : Mod`. Its constructor loads the settings, calls `Harmony.PatchAll()`, and then calls `Compatibility.Patch()`.

### Code map (`Source/`)

| File | Responsibility |
|---|---|
| `ModController.cs` | Mod class, settings window hookup (category "Sync Growth"). |
| `Settings.cs` | `ModSettings` with static fields and the settings UI. |
| `MapCompGrowthSync.cs` | `MapComponent`. RimWorld creates it automatically on every map. It owns the groups and the plant → group lookup, rebuilds them on a timer and draws the overlay. |
| `GroupMaker.cs` | Builds groups: adjacency flood fill (default) or zone mode, plus a dev-mode flash visualisation. |
| `Group.cs` | An immutable set of plants with a growth multiplier per plant, and `Draw()` for the outline. |
| `Patches/GrowthPerTick.cs` | The patch that does the actual work. |
| `Patches/GetInspectString.cs` | Inspect-pane annotation. |
| `Patches/Compatibility.cs` | Extra inspect-string patches for modded `Plant` subclasses. |
| `GlobalSuppressions.cs` | IDE analyzer suppressions only. |

### Grouping and growth math

- `MapCompGrowthSync.MapComponentTick`:
  - If the mod is disabled, it clears all groups and stops.
  - Otherwise it rebuilds every 2000 ticks (`RebuildInterval`).
  - The first rebuild after load runs at `now + (map.uniqueID % 16) * 60` ticks, so maps don't all rebuild on the same tick.
  - Each rebuild clears the groups and starts from scratch.
- **Adjacency mode** (default):
  - Seeds are every plant in a `Zone_Growing` cell, plus every plant on a colonist building that implements `IPlantToGrowSettable` (hydroponics and similar). Wild plants are never visited.
  - From each seed not already grouped, a breadth-first flood fill runs over 4-way neighbours. A neighbour joins when:
    - it has the same `def`
    - it has equal `GrowthRateFactor_Light` and `GrowthRateFactor_Fertility` (`Mathf.Approximately`)
    - its cell's `IPlantToGrowSettable` is set to grow that def
    - adding it keeps the whole group's growth spread (max − min) at or below `max_gap`
- **Zone mode:**
  - Each `Zone_Growing` is one group: plants of the zone's chosen def that are in the `Growing` life stage.
  - Plants more than `max_gap` behind the most-grown plant are dropped from the group.
  - Plant-grower buildings such as hydroponics are not covered.
- **Multiplier** (`Group.CalculateMultiplier`):
  - For a plant in the `Growing` stage: `(1 − growth) / (1 − averageGrowth)`, where the average is over the group at rebuild time.
  - For any other plant, or when the average growth is already 1: `1`.
  - So every growing plant needs the same number of ticks to finish as the average plant would.

### Harmony patches

| Class | Target | Kind | Purpose |
|---|---|---|---|
| `Plant_GrowthPerTick_Patch` (`[HarmonyPriority(VeryLow)]`) | `Plant.GrowthPerTick` getter | postfix | When the mod is enabled and the result is above 0, multiplies the result by `MapCompGrowthSync.GrowthMultiplierOf(plant)`. A plant that isn't in a group gets multiplier 1. Resting, vacuum and other zero-growth states are left alone. The getter is non-virtual, so the patch covers every `Plant` subclass. Modded plants that override the virtual `GrowthRate` still go through it. |
| `Plant_GetInspectString_Patch` (`[HarmonyPriority(VeryLow)]`) | `Plant.GetInspectString` | postfix | Finds the first `"<GrowthRate label>: <digits>%"` with a regex built from `"GrowthRate".Translate()`. The regex is cached and rebuilt when the language changes. It appends ` (+N%)` / ` (-N%)` when the multiplier differs from 1 by at least 0.5%. A negative lookahead prevents a second annotation when a subclass override calls the patched base method. |
| `Compatibility.Patch()` (manual) | `GetInspectString` declared on the modded `Plant` subclasses listed below, when loaded | postfix (same method as above) | These subclasses override `GetInspectString` and may not call the base method, so they wouldn't show the annotation otherwise. Types are resolved with `AccessTools.TypeByName`, and missing mods are skipped. Growth needs no per-mod patch. |

The modded subclasses handled by `Compatibility.Patch()`:

- `Orassans.FrostPlant`
- `VanillaPlantsExpanded.Plant_ChecksRiver`
- `VanillaPlantsExpanded.Plant_HighTempAffected`
- `VanillaPlantsExpanded.Plant_RainAffected`
- `VanillaPlantsExpanded.Plant_StopsGrowingInToxins`
- `VanillaPlantsExpanded.Plant_WinterBlooming`
- `VanillaPlantsExpanded.Plant_WinterResistant`
- `Caveworld_Flora_Unleashed.FruitingBody`
- `BMT.BiomesPlant`
- `Pawnmorph.Things.ExpandedPlant`

### Overlay and dev tool

`MapCompGrowthSync.MapComponentUpdate` draws the outline only when all of these hold:

- mod enabled
- zone mode off
- `draw_overlay` on
- the map is visible
- exactly one plant is selected

It then outlines that plant's group with `GenDraw.DrawFieldEdges`. The colour is always red, because `Draw(0)` is called with index 0. In dev mode, pressing the `Misc1` key binding re-runs the flood fill from the selected plant and flashes each visited cell, with its index, and cells rejected for "max gap".

### Mod settings (`Settings.cs`; static fields, `Scribe_Values`)

| Key | Default | Effect |
|---|---|---|
| `mod_enabled` | true | Master switch. When off, groups are cleared and both postfixes do nothing. |
| `zone_mode` | false | Use growing zones as groups instead of the adjacency flood fill. Labelled "experimental". |
| `draw_overlay` | true | Outline the selected plant's group. Adjacency mode only. |
| `max_gap` | 0.20 (slider 0.01–1) | Maximum growth spread inside a group. In zone mode it is the distance below the most-grown plant. |

No custom Defs and no Comps. The only custom type the game instantiates on its own is `MapCompGrowthSync`.

## Build

```bash
dotnet build -c Release SyncGrowth/Source/SyncGrowth.csproj
```

(Run it from the repo root. `dotnet build SyncGrowth/Source/SyncGrowth.sln` also works and builds Debug by default.)

- Targets `net10.0` with `Krafs.Rimworld.Ref` 1.7.4977-alpha.1 and `Lib.Harmony` 3.0.0-rc.2, both `ExcludeAssets="runtime"`. At run time the game and the Harmony mod (workshop 2009463077) provide them. 1.7 runs on CoreCLR .NET 10.
- Output: `SyncGrowth/1.7/Assemblies/SyncGrowth.dll` only (`DebugType none`, no deps.json, no Harmony copy).
- The csproj sits directly in `Source/`, so the SDK compiles every `*.cs` under `Source/`. Keep stray files out of there. `Source/obj/` is gitignored.
- Assembly info is SDK-generated (there is no `Properties/AssemblyInfo.cs`). That is why the DLL embeds the git commit hash.
- **Building inside the repo overwrites the committed DLL.** To only check compilation, copy the mod folder to a scratch dir (`mktemp -d`) and build there.
- Verified 2026-11-01 in a scratch copy, Release and the .sln Debug build: **0 warnings, 0 errors**.
- The `.sln` lists only `SyncGrowth.csproj` and has no stale entries. Unlike AnimalsLogic, there is no MigrationBackup folder (deleted in d45eee8).

To try it in game, copy the folder (without `Source/`) to `~/.local/share/Steam/steamapps/common/RimWorld/Mods/SyncGrowth`, then enable it after Harmony. The user has no workshop copy, so there is no duplicate packageId to worry about.

## Gotchas and known issues

- **Inspect annotation is format-dependent.** The regex wants whole-number percents (`[0-9]+%`).
  - Vanilla `ToStringPercent` prints decimals under 10% (e.g. `5.5%`), so slow-growing plants may show no annotation.
  - A translation, or another mod that changes the growth-rate line, has the same effect.
  - Growth is still adjusted. Only the text is missing.
- **Grouping uses light and fertility factors only.** Two adjacent plants on different soil or under different light are never grouped. Temperature isn't compared, which is fine on one map.
- **The multipliers are a snapshot.** They are computed at each rebuild and held for up to 2000 ticks. Destroyed or harvested plants stay in the lookup dictionary until the next rebuild. The dictionary is keyed by `Plant` reference, which is harmless.
- **Performance.** Adjacency mode visits every plant in every growing zone and plant-grower building every 2000 ticks per map. The settings tooltip offers zone mode as the faster option for large farms.
- **Savegames.**
  - The game saves map components in `Map.ExposeComponents` (a deep list under `"components"`) and calls `Map.FillComponents()` on load. I checked both in the 1.7.4995 IL.
  - `MapCompGrowthSync` therefore appears in saves as an empty entry. It is added to existing saves automatically, because it has no `ExposeData` fields.
  - Removing the mod should only cost a missing-class error for that entry on load. Nothing else is persisted.
- **Settings binding.** Settings are serialized as `SyncGrowth.Settings`. Renaming the class or namespace resets users' settings.
- **Compatibility list.** When a new plant mod overrides `GetInspectString` without calling the base method, add its fully qualified type name to `Compatibility.PlantSubclasses`. Growth needs no entry. `loadAfter` lists Orassans only, and the other names are resolved by reflection after all assemblies are loaded, so order doesn't matter for them.
- **Translations.** English is the only language (`Languages/English/Keyed/Keys.xml`): enabled, zone mode, overlay, and max gap. `SyncGrowthMaxGapLabel` takes `{0}` (current) and `{1}` (default). If you rename a key, update the code and the XML together.
- **Incompatible mod.** `About.xml` declares `incompatibleWith neceros.plantgrowthsync`, another growth-sync mod.

## Testing

1. Enable the mod after Harmony in dev mode, and check `Player.log` (`~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Logs/`) for Harmony errors mentioning `SyncGrowth`.
2. Make a growing zone and sow it unevenly, e.g. sow half, wait a few days, then sow the rest, or use dev tools to set growth on some plants. Plants within the max gap (20% by default) of each other should be grouped.
3. Select a plant. A red outline should mark its group, and the inspect pane should show `Growth rate: NN% (+N%)` or `(-N%)`. Plants behind the average get a plus, plants ahead get a minus.
4. In dev mode, select a plant and press the Misc1 key binding. Cells should flash with their flood-fill index, and rejected neighbours show "max gap".
5. Let it run and check that the group reaches 100% at roughly the same tick. Also check that the harvest ETA (`TicksUntilFullyGrown`, which uses the patched `GrowthPerTick`) agrees across the group.
6. Switch to zone mode and confirm that whole zones group, that hydroponics basins are no longer synced, and that the overlay is gone.
7. Turn "Mod enabled" off. Annotations should disappear, and groups clear on the next tick.
8. Optional: with Vanilla Plants Expanded, Biomes!, Orassans, Caveworld Flora or Pawnmorpher, check that their special plants show the annotation once, not twice.
