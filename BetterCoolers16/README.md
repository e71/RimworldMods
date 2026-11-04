# BetterCoolers (1.6)

This mod makes the vanilla cooler sit *inside* a wall. The wall stays in place as the cell's edifice, and the cooler is drawn on top of it with new textures, so you no longer have to knock out a wall section to install a cooler.

It's a local update of the abandoned workshop mod BetterCoolers by Latta, Skullywag and Antaios (Steam item 1430093399). The original replaced the vanilla def. This version patches the vanilla `Cooler` def instead, so changes Ludeon makes to it in later versions are kept.

It has a twin, **BetterVents16** (`../BetterVents16`), which does the same for the vent. Both mods ship the same `BetterCV.dll`, and that DLL is built from **this** folder's `Source/` (see Build).

## Status

| | |
|---|---|
| Name / author | BetterCoolers (1.6). Latta, Skullywag, Antaios; 1.6 update by ellerain |
| packageId | `ellerain.bettercoolers16` |
| supportedVersions | 1.6, 1.7 |
| Dependencies | Harmony (`brrainz.harmony`) |
| incompatibleWith | `antaioz.bettercoolers` (the original workshop mod), `hanabishi.inwallcoolersandvents` |
| Local install | `~/.local/share/Steam/steamapps/common/RimWorld/Mods/BetterCoolers16`, enabled |

- **1.7:** works. A 1.7 net10 DLL is built, and the 1.7 log is clean for this mod.
- **1.6:** uses a prebuilt net472 DLL.
- **Name:** "(1.6)" in the name and folder is historical. The mod supports 1.7 as well.

## Folder layout

`LoadFolders.xml` loads:

- 1.6: `Common` then `1.6`
- 1.7: `Common` then `1.7`

```
About/About.xml, About/Preview.png
Common/Patches/Cooler.xml                         XML patch of the vanilla Cooler ThingDef
Common/Languages/English/Keyed/PlaceWorkers.xml   keyed strings
Common/Textures/Things/Building/Misc/TempControl/CoolerB_{north,east,south}.png
1.6/Assemblies/BetterCV.dll                       prebuilt net472
1.7/Assemblies/BetterCV.dll                       net10 build output
Source/                                           shared C# for BetterCoolers16 and BetterVents16
```

## How it works

### XML: `Common/Patches/Cooler.xml`

The patch changes these fields on `Defs/ThingDef[defName="Cooler"]`:

| Field | New value | Notes |
|---|---|---|
| `graphicData/texPath` | `Things/Building/Misc/TempControl/CoolerB` | |
| `altitudeLayer` | `PawnUnused` | |
| `passability` | `Standable` | |
| `fillPercent` | `0.99` | |
| `terrainAffordanceNeeded` | `Light` | |
| `blockWind`, `blockWeather`, `canOverlapZones` | removed | The wall underneath already blocks them. |
| `building/isAirtight` | removed | It's a config error on a building that doesn't fill its cell. |
| `building/isEdifice` | `false` (added) | |
| `placeWorkers` | `BetterCV.PlaceWorker_OnWall` (added) | |

Vanilla already has `building/canPlaceOverWall = true` on the cooler and vent, and the C# relies on that flag.

### C# (`Source/`, namespace `BetterCV`)

**`BetterCVUtil`**
- `isWall(def)`: the def links as a wall (`LinkFlags.Wall`) or is smoothed stone.
- `isOverWall(def)`: `building.canPlaceOverWall` is set and one of the def's place workers is `PlaceWorker_OnWall`.

**`PlaceWorker_OnWall`**
- Only allows placement where the cell's edifice is a wall or smoothed stone.
- Otherwise it returns the vanilla key `MessagePlacementOnSupport`.

**`ModWallPatch`** (StaticConstructorOnStartup)
- Runs `new Harmony("BetterCV").PatchAll()`.

**Harmony patches.** All three are postfixes that stop a wall and an over-wall building from treating each other as blockers:

| Class | Target | Effect |
|---|---|---|
| `BlocksConstruction` | `GenConstruct.BlocksConstruction` | `true` → `false` when one side is over-wall and the other is a wall. |
| `CanPlaceBlueprintOver` | `GenConstruct.CanPlaceBlueprintOver` | `false` → `true` for the same pairs, so you can place the blueprint over an existing wall. |
| `SpawningWipes` | `GenSpawn.SpawningWipes` | `true` → `false` for the same pairs, so spawning the cooler doesn't destroy the wall (and building a wall doesn't destroy the cooler). |

## Build

```bash
dotnet build -c Release BetterCoolers16/Source/BetterCV.csproj
```

- Targets net10.0 with `Krafs.Rimworld.Ref` 1.7.4977-alpha.1 and `Lib.Harmony` 3.0.0-rc.2, both reference-only.
- Output goes to `BetterCoolers16/1.7/Assemblies/BetterCV.dll`.
- A `CopyToBetterVents` target (AfterTargets=Build) then copies the same DLL to `BetterVents16/1.7/Assemblies/`.
- BetterVents16 has no `Source/` of its own. Commit both DLLs together.
- Rebuilding the 1.6 DLL would need a net472 project against 1.6 references. The current source should compile there unchanged, but that hasn't been tried since the move to SDK-style projects.

Install both mods:

```bash
for m in BetterCoolers16 BetterVents16; do rsync -a --delete --exclude Source --exclude .git $m/ ~/.local/share/Steam/steamapps/common/RimWorld/Mods/$m/; done
```

## Gotchas

- **Both mods ship an assembly called `BetterCV` with the same Harmony id.** With both enabled, the same postfixes apply once or twice depending on how the loader dedupes the assembly. Either way they are idempotent (they only flip `__result` in one direction), so this is harmless. Keep it that way if you add patches.
- **The patches are global.** They apply to *any* def with `canPlaceOverWall` plus `PlaceWorker_OnWall`. Another mod can add the place worker to its own building to get the same behavior.
- **Existing saves.** Vanilla coolers already built in a gap in the wall keep working. A new cooler needs a wall under it to be placed.
- **The textures are the original mod's art.** `CoolerB_*` must exist for every rotation the def uses. The vanilla cooler uses `Graphic_Multi`.

## Testing

1. Build a wall, then place a cooler on a wall cell. The blueprint should be accepted.
2. Check that the wall isn't deconstructed, and that the cooler moves heat between the two rooms.
3. Placing a cooler on open floor should be refused with "Must place on a supporting structure!" (the key `MessagePlacementOnSupport`, defined in `Common/Languages/English/Keyed/PlaceWorkers.xml`).
4. Check that the log has no `Cooler` config errors at startup.
