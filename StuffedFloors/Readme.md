Allows building floors out of stuff*.

![Important](https://headers.karel-kroeze.nl/title/Important.png)  

You can safely add this mod to you save games, but *you can not remove it* from save games. (If you really must do this, you'll have to remove all built stuffed floors added by this mod first).

![Features](https://headers.karel-kroeze.nl/title/Features.png)  

This mod does two things. First, using Architect Sense it provides a framework for modders to define their own stuffed floor types. Second, it uses this framework to create a number of new floors that are stuffed.

**For players**
Adds several new floors, and existing floors from Telkir's [More Floors](http://steamcommunity.com/sharedfiles/filedetails/?id=725623521), CuproPanda's [Extra Floors](https://ludeon.com/forums/index.php?topic=13400#msg135940) and Pravus' [Fences and Floors](http://steamcommunity.com/sharedfiles/filedetails/?id=784370602) in all types of stone, metal, wood.

Works great with other mods that add more resources, e.g. [Minerals and Materials](http://steamcommunity.com/sharedfiles/filedetails/?id=728233992) [Kura's Extra Minerals](http://steamcommunity.com/sharedfiles/filedetails/?id=852103845), [Extended Woodworking](http://steamcommunity.com/sharedfiles/filedetails/?id=836912371), [Vegetable Garden](http://steamcommunity.com/sharedfiles/filedetails/?id=822470192) and [GlitterTech](http://steamcommunity.com/sharedfiles/filedetails/?id=725576127).

This mod also organizes and where needed, removes non-stuffed versions of the floors added by RimWorld, [More Floors](http://steamcommunity.com/sharedfiles/filedetails/?id=725623521), [Extended Woodworking](http://steamcommunity.com/sharedfiles/filedetails/?id=836912371), [Minerals and Materials](http://steamcommunity.com/sharedfiles/filedetails/?id=728233992), [GlitterTech](http://steamcommunity.com/sharedfiles/filedetails/?id=725576127), [More Furniture](http://steamcommunity.com/sharedfiles/filedetails/?id=739089840), [Vegetable Garden](http://steamcommunity.com/sharedfiles/filedetails/?id=822470192), and [Floored](http://steamcommunity.com/sharedfiles/filedetails/?id=801544922).

Other mods can easily add more floors using existing materials, or completely new types of materials.

**For modders**
Adds a custom FloorTypeDef that derives from TerrainDef, and allows modders to create floortypes by setting a texture and a list of stuffCategories to generate terrain defs for. Removing now obsolete terrainDefs and/or architect categories is also easily handled. See [GitHub](https://github.com/fluffy-mods/StuffedFloors/wiki/For-Modders) for a guide on using Stuffed Floors in other mods.

![Known issues](https://headers.karel-kroeze.nl/title/Known%20issues.png)  

While this mod will happily generate more floors for any mods that add materials to the metallic, stony and/or woody stuff types, it only cleans up the floors added by mods that are explicitly supported. Any other mods that add floor types may appear as duplicates. If you encounter such an issue, please let the author(s) of said mod(s) know so that they can correctly set up their designator groups!

![Powered by Harmony](https://headers.karel-kroeze.nl/title/Powered%20by%20Harmony.png)  

![Powered by Harmony](https://raw.githubusercontent.com/pardeike/Harmony/master/HarmonyLogo.png)

![Think you found a bug?](https://headers.karel-kroeze.nl/title/Think%20you%20found%20a%20bug%3F.png)  

Please read [this guide](http://steamcommunity.com/sharedfiles/filedetails/?id=725234314) before creating a bug report,
and then create a bug report [here](https://github.com/fluffy-mods/StuffedFloors/issues)

![Older versions](https://headers.karel-kroeze.nl/title/Older%20versions.png)  

All current and past versions of this mod can be downloaded from [GitHub](https://github.com/fluffy-mods/StuffedFloors/releases).

![License](https://headers.karel-kroeze.nl/title/License.png)  

All original code in this mod is licensed under the [MIT license](https://opensource.org/licenses/MIT). Do what you want, but give me credit.
All original content (e.g. text, imagery, sounds) in this mod is licensed under the [CC-BY-SA 4.0 license](http://creativecommons.org/licenses/by-sa/4.0/).

Parts of the code in this mod, and some of the content may be licensed by their original authors. If this is the case, the original author & license will either be given in the source code, or be in a LICENSE file next to the content. Please do not decompile my mods, but use the original source code available on [GitHub](https://github.com/fluffy-mods/StuffedFloors/), so license information in the source code is preserved.



![Are you enjoying my mods?](https://headers.karel-kroeze.nl/title/Are%20you%20enjoying%20my%20mods%3F.png)  

Normally, this is where I ask you to show you appreciation by buying me a coffee.

These are not normal times. Ukraine is being invaded by Russia, at the whim of a ruthless dictator. Innocent people are loosing their lives, and fighting for their continued freedom.

This is not a matter of politics. This is not a debate. Putins' bloody campaign in Ukraine is illegal, and he will stop at nothing to get what he wants, when he wants it, no matter the cost. The Russian army is invading a country without provocation, bombing civilians and murdering innocents.

The prospect of waking up to see my country at war is alien to me, as it must have seemed to most Ukrainians. I can do little to influence the outcome of current affairs, but I will do whatever I can.

**I ask you to join me in supporting the people of Ukraine**

![Humanitarian Aid](https://headers.karel-kroeze.nl/title/Humanitarian%20Aid.png)  

If you can, donate to the various charities providing humanitarian aid. If you don't know where to donate, Global Citizen maintains a list of charitable organizations active in the region.

<https://www.globalcitizen.org/en/content/ways-to-help-ukraine-conflict/>

![Speak up](https://headers.karel-kroeze.nl/title/Speak%20up.png)  

Leaders around the world are deciding on how to respond to Putins' aggression. They are balancing their conscience, and their desire to be re-elected. Many are afraid to impose heavy sanctions because the economic repercussions might loose them votes. The Dutch government, my government, has hinted that they are unwilling to accept Ukranian refugees, and has played a part in allowing Russia to have continued access to international finance. I am ashamed of these actions, taken in my name.

Whereever your live, please let your government know you care more about supporting the people of Ukraine than you do about the price of gas. Join a protest, write letters, call your representatives, and show them that you care about justice!

## Developer handover notes

These notes cover the fork in `e71/RimworldMods` (forked from quicksilverfox/RimworldMods). The text above is Fluffy's upstream readme and describes what the mod does for players.

### Status

| | |
|---|---|
| Upstream author | Fluffy (Karel Kroeze), https://github.com/fluffy-mods/StuffedFloors. The 1.5-1.7 updates in this repo are by Quicksilver Fox |
| packageId | `Fluffy.StuffedFloors` |
| supportedVersions | 1.5, 1.6, 1.7 |
| Dependencies | `brrainz.harmony` (Harmony, workshop 2009463077) |
| loadAfter | `brrainz.harmony`; no loadBefore |
| Optional integrations | Royalty, Ideology, Biotech (pollution fields), Odyssey (vacstone obsoletes), Vanilla Furniture Expanded - Architect (`VanillaExpanded.VFEArchitect`) |
| Steam workshop id | 3245941356 (`About/PublishedFileId.txt`) |
| Harmony id | `Fluffy.StuffedFloors` |

`modinfo.json` (version 3.7.126) and `About/Manifest.xml` (2.13.92, pointing at Fluffy's 1.2 branch) are upstream leftovers, and nothing in this repo reads them.

1.7 port: commit `99f5a9d` ("1.7 update for Stuffed Floors. Comes with some bugfixes. Now saves key hashes used for floors it adds instead of just hoping that there would be no collisions."). It added `1.7/` (Defs, Patches, DLL), changed the csproj to the net10 SDK style, copied more TerrainDef fields in `FloorTypeDef.GetStuffedTerrainDef`, made the wealth patch count the foundation grid, and added `TerrainShortHashes.cs` with `GameComponent_StuffedFloors.cs`. There are no TODO/FIXME/HACK markers in the source. The current source builds cleanly against the 1.7 reference assemblies. As part of this handover I did not run it in-game, so the short-hash save compatibility code in particular is unverified here.

1.7 XML changes compared with 1.6: `Base.xml` adds the `Walkable` affordance, the `Floor` tag, Biotech pollution overlay fields and `canEverTerraform false`. `MetalFloors.xml` gives metal floors CleaningTimeFactor 0.6. `StoneFloors.xml` adds the Odyssey vacstone tiles/flagstones to `obsoletes`.

### Folder layout

There is no `loadFolders.xml`, so RimWorld loads the root folder plus the folder for the running version.

| Folder | Content |
|---|---|
| `About/` | About.xml, PublishedFileId.txt, Manifest.xml (stale) |
| `Languages/` | DefInjected translations (ChineseSimplified/Traditional, German, Russian, Spanish, SpanishLatin) for `StuffedFloors.FloorTypeDef` labels, plus some `ArchitectSense.DesignationSubCategoryDef` files for a mod that is no longer used. There is no English folder because English text is in the defs |
| `Textures/Floors/` | Floor textures used by the FloorTypeDefs |
| `1.5/`, `1.6/` | `Assemblies/StuffedFloors.dll` (**prebuilt from older source; the current csproj only targets 1.7 and cannot rebuild these**), `Defs/Floors/*.xml`, `Patches/*.xml` |
| `1.7/` | `Assemblies/StuffedFloors.dll` (built from `Source/`), `Defs/Floors/*.xml`, `Patches/*.xml` |
| `Source/` | `StuffedFloors.sln`, `StuffedFloors/StuffedFloors.csproj` and the .cs files, `description.md` and `preview.svg` (workshop text and preview sources) |

### How it works

Code map (`Source/StuffedFloors/`):

| File | Responsibility |
|---|---|
| `Controller.cs` | `Mod` subclass. The constructor runs `new Harmony("Fluffy.StuffedFloors").PatchAll()`. Nested `[StaticConstructorOnStartup] Init` calls `Initialize()` after all defs load: it hides every terrain listed in any FloorTypeDef's `obsoletes` and merges the More Floors `MoreFloors` designation category into `Floors`. It also holds `GeneratedTerrains` (every terrain this mod generated) and a cache of stuff defs per `StuffCategoryDef` |
| `FloorTypeDef.cs` | `public class FloorTypeDef : TerrainDef`, the custom def type. Extra fields: `stuffCost`, `obsoletes` (defName strings, so they can name defs of inactive mods), `statsAffectedByStuff`, runtime `terrains`. `GetStuffedTerrainDef(stuff)` builds one TerrainDef per stuff by copying a long list of TerrainDef fields (keep this list in sync when Ludeon adds TerrainDef fields). It applies stuff color and effects, adds `ceil(stuffCost / VolumePerUnit)` of the stuff to the cost, and applies stuff offsets and factors to `statsAffectedByStuff`, with Beauty damped (offset x1/3, factor sqrt). It adds the `FineFloor` tag when beauty is over 4. A generated def is named `<stuffDefName>_<floorTypeDefName>`, labelled with `ThingMadeOfStuffLabel`, placed in the `Floors` designation category, and grouped by a `DesignatorDropdownGroupDef` named after the floor type |
| `HarmonyPatch_GenerateImpliedDefs_PreResolve.cs` | Generates the stuffed terrains |
| `HarmonyPatch_WealthWatcher_CalculateWealthFloors.cs` | Replaces the floor wealth calculation |
| `TerrainShortHashes.cs` | Gives this mod's terrains short hashes that cannot steal hashes from vanilla or other mods, and restores the hash layout a save was written with when it is loaded |
| `GameComponent_StuffedFloors.cs` | `GameComponent` (created automatically by the game) that writes `terrainHashScheme` and a `terrainHashes` table (defName → short hash of every TerrainDef) into the save, and frees the table after loading |
| `DesignatorUtility.cs` | Removes designators (sets `designationCategory` and `designatorDropdown` to null and re-resolves the category), merges designation categories, and calls the Architect tab's `CacheDesPanels` through `Traverse` |
| `Extensions.cs` | Deep-copy helpers for `StatModifier`, cost lists and def lists |
| `WealthDebugger.cs` | Dead code, see Gotchas |

Harmony patches:

| Patch class | Target | Kind | Why |
|---|---|---|---|
| `HarmonyPatch_GenerateImpliedDefs_PreResolve` | `DefGenerator.GenerateImpliedDefs_PreResolve(bool hotReload)` | Prefix | For every FloorTypeDef and every stuff in its `stuffCategories`, creates the terrain and registers it with `DefGenerator.AddImpliedDef` (vanilla implied-def path), records it in `GeneratedTerrains` and the floor type's `terrains` |
| `HarmonyPatch_WealthWatcher_CalculateWealthFloors` | `WealthWatcher.CalculateWealthFloors` (private) | Prefix, returns false (replaces vanilla) | Vanilla indexes a cached market-value array by `def.index`, and per upstream comments that goes out of sync with the generated defs. The replacement counts each TerrainDef on the non-fogged top grid plus the foundation grid and multiplies by `MarketValue` |
| `HarmonyPatch_ShortHashGiver_GiveShortHash` | `ShortHashGiver.GiveShortHash` (private) | Prefix, skips | Skips FloorTypeDefs and generated terrains during the normal hash pass, unless `TerrainShortHashes.assigningDeferred` is set |
| `HarmonyPatch_ShortHashGiver_GiveAllShortHashes` | `ShortHashGiver.GiveAllShortHashes` | Postfix | `TerrainShortHashes.AssignDeferred()`: hashes the skipped defs afterwards (sorted by defName) with the remaining free values, rebuilds the TerrainDef short-hash dictionary and drops the stale entry under hash 0 |
| `HarmonyPatch_TerrainGrid_ExposeData` | `TerrainGrid.ExposeData` | Prefix and Finalizer | While a save's terrain grids load, temporarily sets every TerrainDef's `shortHash` to the value that save used, then restores it in the finalizer. The layout comes from the save's table (scheme 2). For saves written by an older version of this mod with no marker, it falls back to a reconstruction of the old single-pass hashing (scheme 0). If the mod was not active when the save was written, or the save is scheme 1, live hashes are used. It logs `[StuffedFloors] Reading terrain with ...` whenever it applies a layout |

The hashing scheme is explained at the top of `TerrainShortHashes.cs`. The bug it fixes: generated names like `AB_BlocksMudstone_StuffedStoneRoughCobblestones` sort before vanilla names and used to take their hashes. For example, adding the mod to an existing save turned gravel cells into a stuffed floor.

Defs (`1.7/Defs/Floors/`):
- `Base.xml`: abstract `StuffedFloorsBase` (affordances, `Floor` tag, `statsAffectedByStuff`: WorkToBuild, Beauty, Cleanliness, MarketValue, Flammability).
- `WoodFloors.xml`, `StoneFloors.xml`, `MetalFloors.xml`: abstract bases per material and tier (Basic/Pretty/Royal), with `stuffCategories` Woody/Stony/Metallic and `stuffCost`, plus the concrete floor types and their `obsoletes` (vanilla and More Floors floors they replace). One stone floor type has `MayRequire="Ludeon.RimWorld.Royalty"`.
- `Ideology.xml`: `MayRequire="Ludeon.RimWorld.Ideology"` style floors (Morbid, Spikecore, Totemic, Techist hex tile) with `dominantStyleCategory` and `canGenerateDefaultDesignator false`.
- `VFE_Architect.xml`: `MayRequire="VanillaExpanded.VFEArchitect"` floors replacing VFE Architect ones.

There are about 47 concrete FloorTypeDefs, and each yields one terrain per matching stuff, which adds up to hundreds of generated terrains.

Patches (`1.7/Patches/`):
- `IdeologyPatch.xml` (inside `PatchOperationFindMod` "Ideology"): removes vanilla `Tile_Transhumanist`, `Plates_Spikecore` and `Boards_Totemic` from the `addDesignators` of the Transhumanist meme / Spikecore / Totemic styles (conditionally), adds the stuffed designator groups to `addDesignatorGroups`, and replaces the vanilla Morbid/Spikecore/Totemic stone floor groups with the stuffed ones. I checked all targets against 1.7 `Data/Ideology`.
- `VFEArchiPatch.xml` (inside `PatchOperationFindMod` "Vanilla Furniture Expanded - Architect"): adds VFE wood plank floors to `StuffedWoodFloorVanilla`'s `obsoletes`.

There are no mod settings.

### Build

The project is `Source/StuffedFloors/StuffedFloors.csproj`: SDK style, `net10.0`, `Krafs.Rimworld.Ref` 1.7.4977-alpha.1 and `Lib.Harmony` 3.0.0-rc.2 (both `ExcludeAssets="runtime"`). `OutputPath` is `..\..\1.7\Assemblies\` with no TFM subfolder, no deps.json and no copied references. Run from the mod folder:

```
dotnet build Source/StuffedFloors/StuffedFloors.csproj -c Release
```

I verified this in a scratch copy (`/tmp/claude-1000/build.*`): it restored, built with 0 warnings and 0 errors, and produced `1.7/Assemblies/StuffedFloors.dll` (24064 bytes, the same size as the committed one) plus `StuffedFloors.pdb` (`*.pdb` is gitignored).

- Building inside the repo overwrites the committed `1.7/Assemblies/StuffedFloors.dll`. Build in a copy unless you mean to update it.
- **Always pass `-c Release`.** `dotnet build` defaults to Debug, which defines `DEBUG_IMPLIED_DEFS`, `DEBUG_COSTLIST` and `DEBUG_STUFFING` (top of `Controller.cs` and `FloorTypeDef.cs`) and logs a message for every generated terrain.
- The 1.5 and 1.6 DLLs cannot be rebuilt from this project: it only targets `net10.0` against the 1.7 reference assemblies, and no older build setup is in the repo. Treat them as frozen binaries. Rebuilding them would need separate net472 projects against those game versions.

### Gotchas and known issues

- **The mod cannot be removed from a save** (see the upstream text above). Built stuffed floors are generated defs, and their cells are lost on load without the mod.
- Adding or removing a mod that adds stuffs changes the set of generated terrains. Since 1.7 the save stores a full terrain hash table, so terrain still loads correctly. Saves from older versions of this mod rely on the reconstructed legacy layout in `TerrainShortHashes.LegacyLayout`, which mirrors vanilla's `GiveShortHash` loop. If vanilla changes that algorithm, update the reconstruction.
- `TerrainShortHashes` uses private members of `ShortHashGiver` (`takenHashesPerDeftype`, `GiveShortHash`) and `DefDatabase<TerrainDef>.defsByShortHash` through `AccessTools`. Renaming any of them in a game update breaks hashing at startup.
- `FloorTypeDef.GetStuffedTerrainDef` copies TerrainDef fields one by one. A new TerrainDef field in a game update is silently left at its default on generated floors until you add it to that list.
- The generated terrains are always put in the `Floors` designation category. A FloorTypeDef that sets `designationCategory` gets a config error.
- Only floors listed in some `obsoletes` are hidden. Floors from unsupported mods show up next to the stuffed versions (upstream known issue).
- The Ideology patch removes `Tile_Transhumanist` from the Transhumanist meme only. Vanilla 1.7 also lists it in the Techist `StyleCategoryDef` `addDesignators`, which the patch does not touch. It is in `StuffedTechistMetalTile`'s `obsoletes` (so its `designationCategory` is nulled), but I have not checked in-game whether Techist ideoligions still get the vanilla hex tile button.
- `WealthDebugger.cs` is guarded by `#if DEGUG` (typo), so `DEBUG_WEALTH` is never defined and the class never compiles. It also has no `using` lines, so fixing the typo alone would break the build.
- `Languages/*/DefInjected/ArchitectSense.DesignationSubCategoryDef/` targets a def type from Architect Sense, which this mod no longer uses. Expect them to be ignored, or to log translation warnings in those languages.
- `MayRequire="VanillaExpanded.VFEArchitect,Ludeon.Rimworld.Royalty"` in `VFE_Architect.xml` uses a lowercase "w". RimWorld compares packageIds case-insensitively, so this works.
- This mod generates stuffed floors from every Metallic, Woody and Stony stuff, including Carbon's c-composite (Metallic and Woody) and synthdiamond (Stony) from this repo.

### Testing

Royalty and Ideology are installed locally. Biotech and Odyssey are not, so their bits can't be checked here.
1. Copy the mod folder (with a freshly built Release DLL if you changed code) into `~/.local/share/Steam/steamapps/common/RimWorld/Mods/`. It is not installed there now. Enable Harmony (workshop 2009463077) and load Stuffed Floors after it.
2. Start with dev mode on and check the log: no Stuffed Floors exceptions, no XML errors for `StuffedFloors.FloorTypeDef`, no patch failures from `IdeologyPatch.xml`.
3. Architect > Floors: each floor type should be a dropdown with one entry per stone, wood or metal, labelled "<stuff> <floor label>" (via `ThingMadeOfStuffLabel`). Vanilla floors listed in `obsoletes` (e.g. `WoodPlankFloor`, stone tiles) should be gone from the menu.
4. Build a few floors, check cost (base cost plus stuff) and stats (beauty, cleanliness, work). Then check colony wealth in the History tab or dev wealth readout, to confirm the floor wealth patch gives sensible numbers.
5. Save and reload. With dev mode, the log should show `[StuffedFloors] Reading terrain with the terrain hash table stored in the save.` and the floors should still be there. Also add the mod to a save made without it: gravel and other vanilla terrain must stay unchanged.
6. With an Ideology ideoligion using Morbid, Spikecore, Totemic or Techist styles, check that the stuffed style floors replace the vanilla style floors.
