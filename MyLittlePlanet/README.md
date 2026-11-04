# My Little Planet

My Little Planet adds a "Planet size" slider to the Create World page. The slider sets how many times the planet's surface grid is subdivided, from 6 ("Tiny") to 10 ("Default", the vanilla value). Fewer subdivisions give a world with far fewer, larger tiles. World generation is faster and the world is smaller, for players who don't need a full-size planet. On 1.7 the mod also scales world-map camera rotation, keyboard and mouse drag, to the bigger tiles, so moving around a small planet doesn't feel sluggish.

This is upstream code by Quicksilver Fox / Oblitus. All 1.7 work came from upstream (`origin` = quicksilverfox/RimworldMods). The user's fork (`e71`) has no MyLittlePlanet-specific commits.

## Status

| | |
|---|---|
| Name / author | My Little Planet, `Oblitus` (git author: Quicksilver Fox) |
| packageId | `Oblitus.MyLittlePlanet` |
| supportedVersions | 0.19, 1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7 |
| modDependencies | `brrainz.harmony` (Harmony, workshop 2009463077) |
| loadAfter | `brrainz.harmony` |
| incompatibleWith | none declared |
| Steam workshop id | `1117406550` (`About/PublishedFileId.txt`) |
| Harmony id | `net.quicksilverfox.rimworld.mod.worldgenrules` |

The 1.7 port is upstream commit 7f0a4cc (2026-10-14). It did the following:

- switched to the SDK net10 csproj
- changed the transpiler to match the `ldstr` by value instead of by reference
- added an error log for a missing anchor
- added `CameraSpeed.cs`

Facts about the 1.7 DLL and test status:

- The committed `1.7/Assemblies/MyLittlePlanet.dll` matches the current source. A Release rebuild gives the same size (11776 bytes) and the same metadata strings, apart from the embedded pdb path. The committed DLL was built on Windows: `C:\Projects\...\obj\Release\MyLittlePlanet.pdb`.
- The mod is not installed here: no workshop folder, not in `Mods/`, not in `ModsConfig.xml`. The 1.7 build has never run on this machine.
- I checked the patch targets against the 1.7.4995 `Assembly-CSharp.dll`; see "Harmony patches".

There are no TODO, FIXME or HACK markers. `Settings.cs` is dead code, described under "Mod settings".

## Folder layout

There is no `loadFolders.xml`. The game's default (checked in `ModContentPack.InitLoadFolders`) loads, in this order:

- `1.7/`, or if that folder is missing, the newest version folder that is not newer than the game
- `Common/` if present (there is none here)
- the mod root

```
About/About.xml, Preview.png, PublishedFileId.txt
Languages/{English,French,Russian}/Keyed/Interface.xml   3 keys, shared by all versions
0.19/, 1.0/Assemblies/       MyLittlePlanet.dll + bundled 0Harmony.dll (pre-Harmony-mod era)
1.1/ .. 1.6/Assemblies/      MyLittlePlanet.dll, prebuilt
1.7/Assemblies/              MyLittlePlanet.dll, net10 build output of Source/
Source/MyLittlePlanet.sln
Source/MyLittlePlanet/       MyLittlePlanet.csproj and *.cs
```

The version folders contain only DLLs. No version folder has Defs or XML patches.

The 0.19–1.6 DLLs are prebuilt and the current source does not build them.

- Target frameworks: .NET Framework 4.0 for 1.1–1.3, 4.8 for 1.4–1.6.
- Commits:
  - 1.6: d865dfb (2025)
  - 1.5: 01dad75
  - 1.4: a049870
  - 1.3: 803f421
  - 1.2: 4412467
  - 1.1: 7a4a341
  - 0.19 and 1.0: 0ac59af
- Those versions did more than the 1.7 code. Up to 1.5, the DLLs also patch `PlanetShapeGenerator.DoGenerate` and the hills, rivers, roads and single-tile world layers, and the 1.3 DLL also has a storyteller-selection patch. The 1.6 DLL has only the slider, based on `PlanetLayerSettingsDef`.

## How it works

The code lives in namespace `WorldGenRules`, the mod's historical name.

| File | Responsibility |
|---|---|
| `MyLittlePlanet.cs` | `MyLittlePlanet : Mod`. Its constructor creates the Harmony instance and calls `PatchAll()`. Nothing else. |
| `TileSize.cs` | `WorldGenRules.subcount` (static int, default 10) and the Create World page transpiler that draws the slider. |
| `CameraSpeed.cs` | `[StaticConstructorOnStartup] static class CameraSpeed`: tile-size ratio math and the two world-camera patches. |
| `Settings.cs` | Unused `ModSettings` subclass with an empty settings window and an empty `ExposeData`. The Mod class doesn't override `SettingsCategory` or `DoSettingsWindowContents`, so no settings page exists. |
| `Properties/AssemblyInfo.cs` | Fixed version 1.0.0.0. |

### Harmony patches

**`Page_CreateWorldParams_DoWindowContents_Patch`** (`TileSize.cs`)
- Target: `Page_CreateWorldParams.DoWindowContents(Rect)`. Kind: transpiler.
- Anchor: `ldstr "PlanetCoverageTip"`, matched by value with `CodeInstruction.Is`.
- It inserts 2 instructions after the anchor, i.e. after the `TooltipHandler.TipRegionByKey` call. The inserted code does:
  1. `local7 += 40f` (moves the y offset down one row)
  2. `DrawPlanetSizeSlider(local7, local8)`
- Logs `[My Little Planet] Could not find the PlanetCoverageTip anchor ...` when the anchor is missing.
- Verified in 1.7.4995: local 7 is the `float` row y offset, local 8 is the `float` field width (`rect.width - 200`), and the anchor is at IL 183. The code that follows is vanilla `local7 += 40` before the rainfall row, so the slider becomes a new row between planet coverage and rainfall.

**`DrawPlanetSizeSlider`** (`TileSize.cs`)
- Draws the `MLPWorldPlanetSize` label and a `Widgets.HorizontalSlider`, range 6–10 with step 1, end labels `MLPWorldTiny` / `MLPWorldDefault`.
- Writes the value to `subcount` and then to `PlanetLayerSettingsDefOf.Surface.settings.subdivisions`. This happens every frame the page draws.
- Vanilla 1.7 sets that value to 10 in `Data/Core/Defs/Misc/WorldLayerSettingsDefs/PlanetLayerSettings.xml`.

**`WorldCameraDriver_CalculateCurInputDollyVect_Patch`** (`CameraSpeed.cs`)
- Target: `WorldCameraDriver.CalculateCurInputDollyVect()`, returns `Vector2`. Kind: postfix.
- Multiplies a non-zero keyboard or screen-edge dolly vector by `ratio^(1 - AltitudePercent)`. The effect is full tile-size compensation when zoomed in and vanilla speed when zoomed fully out.

**`WorldCameraDriver_WorldCameraDriverOnGUI_Patch`** (`CameraSpeed.cs`)
- Target: `WorldCameraDriver.WorldCameraDriverOnGUI()`. Kind: transpiler.
- After every `GenWorldUI.CurUITileSize()` call, it inserts a call to `CameraSpeed.DragUITileSize(size) = size / ratio`, so mouse drag keeps the surface under the cursor.
- Logs an error if there is no such call. In 1.7.4995 there is exactly one, at IL 88.

How the ratio is computed (`CameraSpeed.TileSizeRatio`):

- The layer is `Find.WorldSelector.SelectedLayer`, or failing that `Find.WorldGrid.Surface`.
- For anything that isn't the root surface (e.g. Odyssey orbit layers), the ratio is 1.
- Otherwise it reads the layer's private `subdivisions` field through `AccessTools.FieldRefAccess<PlanetLayer, int>("subdivisions")`.
- The result is `3^((default - subdivisions) / 2)`, where `default` is captured in the static constructor from `PlanetLayerSettingsDefOf.Surface.settings.subdivisions`, which is 10, after defs load and before the slider can change it.
- The code comment explains the base: each subdivision pass triples the tile count, so the tile edge shrinks by √3 per pass.

### Defs, Comps, settings

The mod has no custom Defs and no Comps, and no mod settings are saved. The slider value is runtime-only: the static `subcount` plus the mutated def.

Translation keys (in `Languages/*/Keyed/Interface.xml`):

- `MLPWorldPlanetSize`
- `MLPWorldTiny`
- `MLPWorldDefault`

English, French and Russian all have the full set.

## Build

```bash
dotnet build -c Release MyLittlePlanet/Source/MyLittlePlanet/MyLittlePlanet.csproj
```

(Run it from the repo root. `dotnet build MyLittlePlanet/Source/MyLittlePlanet.sln` also works and builds Debug by default.)

- Targets `net10.0` with `Krafs.Rimworld.Ref` 1.7.4977-alpha.1 and `Lib.Harmony` 3.0.0-rc.2, both `ExcludeAssets="runtime"`. At run time the game and the Harmony mod (workshop 2009463077) provide them. 1.7 runs on CoreCLR .NET 10.
- Output: `MyLittlePlanet/1.7/Assemblies/MyLittlePlanet.dll`.
- **The Release configuration also writes `MyLittlePlanet.pdb` into `1.7/Assemblies/`** (`DebugType pdbonly`). The other two mods use `none`. The committed folder has only the DLL, so delete the pdb after a Release build or don't commit it. Debug (`DebugType none`) writes no pdb.
- `GenerateAssemblyInfo=false`. The project uses `Properties/AssemblyInfo.cs` with a fixed `1.0.0.0`, so the default `Deterministic` setting is fine.
- **Building inside the repo overwrites the committed DLL.** To only check compilation, copy the mod folder to a scratch dir (`mktemp -d`) and build there. `obj/` is gitignored.
- Verified 2026-11-01 in a scratch copy, Release and the .sln Debug build: **0 warnings, 0 errors**.
- The `.sln` lists only `MyLittlePlanet\MyLittlePlanet.csproj`, with no stale entries. It reuses AnimalsLogic's project GUID `{674B538C-...}`, which is harmless. There is no MigrationBackup folder.

To try it in game, copy the folder (without `Source/`) to `~/.local/share/Steam/steamapps/common/RimWorld/Mods/MyLittlePlanet`, then enable it after Harmony. The user has no workshop copy.

## Gotchas and known issues

- **Hard-coded locals in the transpiler.** Locals 7 and 8 of `Page_CreateWorldParams.DoWindowContents` are hard-coded (`CodeInstruction.LoadLocal(7)` / `(8)`). This only works while the vanilla method keeps that local layout. The 1.6 code used the same indices, and I checked them against the 1.7.4995 IL.
  - After any game update, recheck them: dump the method's `GetMethodBody().LocalVariables` and its IL around `"PlanetCoverageTip"`.
  - If a local changes type, the patched method gets invalid IL. If it keeps type but changes meaning, the slider draws in the wrong place.
- **The slider mutates a Def.**
  - `PlanetLayerSettingsDefOf.Surface.settings.subdivisions` stays at the chosen value for the rest of the game session, including later world generations, and the static `subcount` remembers it too. Other mods reading that def see the changed value.
  - Restarting the game restores 10.
  - Existing saves are not affected: `PlanetLayer.ExposeData` saves each layer's own `subdivisions` (checked in the 1.7.4995 IL), and the camera code reads the layer's field, not the def.
- **Reflection.** `CameraSpeed` reads the private field `PlanetLayer.subdivisions` with `AccessTools.FieldRefAccess`. A rename in a game update throws in the static constructor at startup.
- **Range.** The slider only goes down: 6–10, and 10 is vanilla. Nothing allows bigger planets.
- **Savegames.** The mod has no saved state of its own. Removing it from a save made with a small planet should keep the small planet, because the size is stored in the save (not tested). Camera speed then goes back to vanilla.
- **Odyssey layers.** Only the root surface layer is scaled, by both the slider and the camera code.
- **Translations.** If you change a key, update all three `Languages/*/Keyed/Interface.xml` files.

## Testing

1. Enable the mod after Harmony. Check `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Logs/Player.log` for `[My Little Planet]` errors or Harmony errors mentioning `MyLittlePlanet` or `WorldGenRules`.
2. Start a new colony. On the Create World page, a "Planet size" row with a Tiny ↔ Default slider should appear directly below "Planet coverage" and above rainfall, and it should not overlap other rows.
3. Generate a world at 6 and one at 10. The size-6 world should have far fewer, much larger tiles. Generation at 6 is expected to be faster.
4. On the small world, check world-map camera movement with WASD or the screen edge, zoomed in and zoomed out, and with mouse drag. Drag should keep the surface under the cursor. Keyboard speed should feel tile-proportional zoomed in and vanilla-like zoomed out. On a size-10 world everything should be vanilla, because the ratio is 1.
5. Save, quit to the main menu, and reload. The planet should keep its size and the camera scaling should still apply. Then generate another world in the same session: the slider starts at the last value used.
6. With Odyssey, select an orbit layer. Its camera speed should be unchanged.
