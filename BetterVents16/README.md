# BetterVents (1.6)

This mod makes the vanilla vent sit *inside* a wall. The wall stays in place as the cell's edifice, and the vent is drawn over it with new textures, including off-state textures.

It's a local update of the abandoned workshop mod BetterVents by Latta, Skullywag, Spdskatr and Antaios (Steam item 1430094853). Instead of replacing the vanilla `Vent` def, it patches it.

This is the twin of **BetterCoolers16** (`../BetterCoolers16`). Read that README for:

- how the C# works;
- the Harmony patches;
- the full list of XML field changes. The vent patch is the same patch with `defName="Vent"`.

## Status

| | |
|---|---|
| Name / author | BetterVents (1.6). Latta, Skullywag, Spdskatr, Antaios; 1.6 update by ellerain |
| packageId | `ellerain.bettervents16` |
| Supported versions | 1.6, 1.7 |
| Dependencies | Harmony (`brrainz.harmony`) |
| Incompatible with | `antaioz.bettervents` (the original workshop mod), `hanabishi.inwallcoolersandvents` |
| Local install | `~/.local/share/Steam/steamapps/common/RimWorld/Mods/BetterVents16`, enabled |

Works on 1.7. The 1.7 log is clean.

## Folder layout

`LoadFolders.xml` loads:

- 1.6: `Common` then `1.6`
- 1.7: `Common` then `1.7`

```
About/About.xml, About/Preview.png
Common/Patches/Vent.xml                           XML patch of the vanilla Vent ThingDef
Common/Languages/English/Keyed/PlaceWorkers.xml
Common/Textures/Things/Building/Misc/TempControl/VentB_{north,east,south}.png
Common/Textures/Things/Building/Misc/TempControl/VentB_Off_{north,east,south}.png
1.6/Assemblies/BetterCV.dll                       prebuilt net472
1.7/Assemblies/BetterCV.dll                       copied here by the BetterCoolers16 build
```

**This mod has no `Source/` folder.** Its `BetterCV.dll` is the same assembly as BetterCoolers16's, built from `../BetterCoolers16/Source/BetterCV.csproj`. A `CopyToBetterVents` target in that csproj copies the DLL here after each build.

## Build

```bash
dotnet build -c Release BetterCoolers16/Source/BetterCV.csproj
```

This one command updates both `BetterCoolers16/1.7/Assemblies/BetterCV.dll` and `BetterVents16/1.7/Assemblies/BetterCV.dll`. Commit both.

## Gotchas

- **Don't edit the DLL here by hand.** Never diverge it from BetterCoolers16's copy. If the vent ever needs code of its own, give it its own project and assembly name (see the BetterCoolers16 README on the shared Harmony id).
- **Off-state textures.** The vent's `CompFlickable` draws `<texPath>_Off` while the vent is closed, so the `VentB_Off_*` textures must stay next to `VentB_*`.

## Testing

1. Place a vent on a wall cell between two rooms.
2. The wall should stay in place.
3. Temperatures should equalize while the vent is open, and stop equalizing when it's closed. When closed, the vent should show the `_Off` texture.
