# Carbon

XML-only RimWorld mod that adds three carbon-based materials made at the vanilla biofuel refinery from chemfuel: **c-weave** (a fabric), **c-composite** (carbon fiber reinforced plastic, usable as metal or wood) and **synthdiamond** (a very hard, very beautiful stony material). It also adds a "make chemfuel from hay" bill, a carbon tile floor, c-weave carpets, and three carbon power conduits (regular, waterproof, hidden). Everything except the hay bill is behind one research project, "carbon".

## Status

| | |
|---|---|
| Upstream author | Oblitus (`<author>` in About.xml); this repo is a fork of quicksilverfox/RimworldMods |
| packageId | `Oblitus.Carbon` |
| supportedVersions | 0.19, 1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7 |
| Dependencies | none (`modDependencies` not set); no loadAfter/loadBefore |
| Optional | Ideology (Techist hex carpet, guarded by `MayRequire` and `PatchOperationFindMod`) |
| Steam workshop id | 1376609823 (`About/PublishedFileId.txt`) |
| About url | https://ludeon.com/forums/index.php?topic=34628.0 |

1.7 port: done in commit `a696b35` ("1.7 Carbon. Comes with light rebalance and improvements"). It added the `1.7/` folder (a copy of 1.6 plus changes, see below) and `1.7` in supportedVersions. There are no TODO/FIXME/HACK markers in the XML. I checked every vanilla def, abstract parent and patch xpath target that 1.7 uses against the installed 1.7.4995 `Data/Core` and `Data/Ideology` XML, and they all exist. It has not been loaded in-game as part of this handover.

What 1.7 changed compared with 1.6:
- New c-weave carpet (`CarbonCarpet`) and Ideology-only c-weave hex carpet (`CarbonCarpetHex`), plus `Patches/Ideology/StyleCategory_Techist.xml`.
- `uiOrder` 2045 on the carbon tile.
- Conduits: `costList Inherit="False"` (the parent `PowerConduit` has its own cost, and without this the costs would merge), each now also costs 1 c-weave, the waterproof conduit costs 9 c-composite (was 3), and its `terrainAffordanceNeeded` is now `WaterproofConduitable` (was `Bridgeable`). The hidden conduit uses `shaderType Transparent`.
- Research tree position moved to x=8.00, y=3.40.

## Folder layout

There is no `loadFolders.xml`, so RimWorld's default applies: the root folder plus the folder for the running version. If no folder matches exactly, it uses the newest older one, so 1.3 and 1.4 load `1.2/`.

| Folder | Content |
|---|---|
| `About/` | About.xml, PublishedFileId.txt |
| `Textures/` | Shared by all versions: `Terrain/Surfaces/CarbonTileGrained.png`, `Things/Building/Linked/CarbonPowerConduit*_Atlas.png`, `Things/Item/Resource/{CarbonFiber,Diamond}.png` |
| `0.19/`, `1.0/`, `1.1/` | Old Defs and Patches, including `Patches/ArchitectSense.xml` (adds the carbon tile to an Architect Sense subcategory; the mod itself is long dead) |
| `1.2/` | Defs and Patches for 1.2 (also loaded on 1.3 and 1.4) |
| `1.5/`, `1.6/` | Defs and Patches for those versions |
| `1.7/` | Current version, the only one worth editing |
| `.gitignore` | Ignores `.claude/` and GIMP `*.xcf` sources |

Each version folder has the same structure: `Defs/{RecipeDefs,ResearchProjectDefs,TerrainDefs,ThingDefs_Buildings,ThingDefs_Items}` and `Patches/{ThingDefs_Buildings,ThingDefs_Misc}` (plus `Patches/Ideology` in 1.7). There is no C# and no Languages folder: all text is English, written directly in the defs.

## How it works (1.7)

### Research
- `CarbonResearch` ("carbon"): Industrial, base cost 600, prerequisite `BiofuelRefining`.

### Materials (`Defs/ThingDefs_Items/Items_Resource_Stuff.xml`)

| defName | Label | Stuff categories | Notable values |
|---|---|---|---|
| `CarbonWeave` | c-weave | Fabric | MarketValue 4, Mass 0.033, StuffPower Armor Sharp 1.4 / Blunt 0.36 / Heat 3.0, Insulation Cold 22 / Heat 22, flammability 0, commonality 0.15. Stuff factors: MaxHitPoints x2.5, Beauty x2.5, WorkToMake x1.3, Flammability x0.05. Uses the vanilla cloth texture tinted (35,35,35) and has `CompColorable`. Category Textiles |
| `CarbonFiber` | c-composite | Metallic **and** Woody | MarketValue 4, Mass 0.25, StuffPower Armor Sharp 0.79 / Blunt 1.04 / Heat 1.2, Sharp dmg x0.4, Blunt dmg x0.9. Stuff factors: MaxHitPoints x2, Beauty x2, WorkToMake/Build x0.7, melee cooldown x0.7, DoorOpenSpeed x1.3, Mass x0.5, Flammability x0; Cleanliness +0.2. Wood-style sounds, `terrainAffordanceNeeded Light`. Category ResourcesRaw |
| `CarbonDiamond` | synthdiamond | Stony | MarketValue 40, Mass 0.5, no hit points, never deteriorates, Blunt dmg x1.5. Stuff factors: MaxHitPoints x10, WorkToMake x3.5, WorkToBuild x7, Beauty x2.5; offsets Beauty +10, Cleanliness +0.2. `terrainAffordanceNeeded Heavy`. Category ResourcesRaw |

All three are `smeltable false`, `burnableByRecipe true`, `isAirtight true`, and tradeable.

### Recipes (`Defs/RecipeDefs/Recipes_Production.xml`)
All of these are added to the vanilla `BiofuelRefinery` by a patch and use `GeneralLaborSpeed`:

| defName | Input → output | Work | Research |
|---|---|---|---|
| `Carbon_MakeChemfuelFromHaygrass` | 75 Hay → 30 Chemfuel | 2000 | none (only needs the refinery) |
| `Carbon_MakeCarbonWeave` | 50 Chemfuel → 20 CarbonWeave | 2000 | CarbonResearch |
| `Carbon_MakeCarbonFiber` | 50 Chemfuel → 20 CarbonFiber | 2000 | CarbonResearch |
| `Carbon_MakeDiamond` | 50 Chemfuel → 5 CarbonDiamond | 3500 | CarbonResearch |

### Floors (`Defs/TerrainDefs/Terrain_Floors.xml`)
- `CarbonTile`: 4 CarbonFiber, Beauty 3, Cleanliness 0.2, WorkToBuild 100, CleaningTimeFactor 0.6, tag FineFloor, uiOrder 2045.
- `CarbonCarpetBase` (abstract): 7 CarbonWeave, Beauty 2, WorkToBuild 800, flammability 0, fixed color (50,50,50). Needs `CarpetMaking` and `CarbonResearch`. The comment in the file says these carpets are deliberately separate buttons placed by `uiOrder` right after the vanilla carpet palette, because generated color carpets are always appended after XML defs. They are not paintable.
  - `CarbonCarpet`: reuses vanilla `Terrain/Surfaces/Carpet`, uiOrder 2101.
  - `CarbonCarpetHex` (`MayRequire="Ludeon.RimWorld.Ideology"`): reuses `Terrain/Surfaces/HexCarpet`, `canGenerateDefaultDesignator false`, `dominantStyleCategory Techist`, StyleDominance 1, uiOrder 2116. It only shows up for ideoligions with the Techist style, using the same gating as the vanilla hex tile.

### Power (`Defs/ThingDefs_Buildings/Buildings_Power.xml`)
All three use `ParentName="PowerConduit"` (vanilla), need `CarbonResearch`, and have flammability 0:
- `CarbonPowerConduit`: HP 250, work 70, cost 1 CarbonFiber + 1 CarbonWeave, own atlas texture.
- `UnderwaterCarbonPowerConduit`: HP 250, work 70, cost 9 CarbonFiber + 1 CarbonWeave, `WaterproofConduitable`, cannot be targeted or damaged by attacks.
- `CarbonPowerConduitUnderground`: HP 750, work 280, cost 1 CarbonFiber + 1 CarbonWeave, invisible (color alpha 0, Transparent shader), standable, cannot be targeted or damaged by attacks.

### PatchOperations (`1.7/Patches`)
- `ThingDefs_Buildings/Buildings_Production.xml`: `PatchOperationAdd` of the four recipes to `ThingDef[defName="BiofuelRefinery"]/recipes`.
- `ThingDefs_Misc/Apparel_Various.xml`: adds `CarbonDiamond` to `disallowedThingDefs` of the default ingredient filter of `ApparelMakeableBase`, `ArmorSmithableBase` and `ArmorMachineableBase`, so synthdiamond is not offered for apparel by default.
- `ThingDefs_Misc/Weapons_Melee.xml`: adds `CarbonFiber` and `CarbonDiamond` to the `disallowedThingDefs` of `BaseMeleeWeapon_Sharp`.
- `Ideology/StyleCategory_Techist.xml`: inside `PatchOperationFindMod` (mod name "Ideology"), adds `CarbonCarpetHex` to `StyleCategoryDef[defName="Techist"]/addDesignators`.

All of these patches are unconditional `PatchOperationAdd` (except the Ideology one), so if a target disappears in a future game version you get a patch error in the log.

## Build

There is nothing to build: the mod is XML and textures only. To use it, copy or symlink the `Carbon/` folder into `~/.local/share/Steam/steamapps/common/RimWorld/Mods/` (it is not installed there now).

## Gotchas and known issues

- **c-composite counts as both Metallic and Woody stuff**, so every recipe or building that accepts metal or wood accepts it (walls, doors, furniture, and the generated floors of the Stuffed Floors mod in this repo). Synthdiamond is Stony, so it shows up wherever stone blocks do. The only restrictions are the two disallow patches (apparel/armor for synthdiamond, sharp melee for both).
- The old version folders are mostly copies of each other. Edit only `1.7/`, and don't backport unless asked.
- 1.3 and 1.4 are listed in supportedVersions but have no folder, so they fall back to `1.2/`.
- `1.7/Defs/TerrainDefs/Terrain_Floors.xml` and `1.7/Defs/ThingDefs_Buildings/Buildings_Power.xml` start with a UTF-8 BOM. Keep the encoding when editing.
- The carpets and the waterproof/hidden conduit icons use vanilla texture paths (`Terrain/Surfaces/Carpet`, `Terrain/Surfaces/HexCarpet`, `Things/Building/Linked/WaterproofConduit_MenuIcon`). If Ludeon renames them, the carpets will render pink.
- There is no Languages folder, so the mod can't be translated without adding DefInjected files.

## Testing

Ideology is installed locally (`Data/Ideology`), so you can check the hex carpet. Biotech is not installed.
1. Enable the mod (after Core/Ideology) and start a dev-mode game. Check the log for XML or patch errors that mention Carbon defs.
2. Use dev tools to finish `BiofuelRefining` and `CarbonResearch`, or check the research tab: "carbon" should sit after biofuel refining.
3. Build a biofuel refinery. It should list "make chemfuel from hay" (before the research too), "make carbon weave", "make carbon composite" and "make synthdiamond".
4. Architect > Floors: carbon tile, and c-weave carpet right after the vanilla carpets (also needs CarpetMaking). With an ideoligion using the Techist style, the c-weave hex carpet appears next to the hex carpets.
5. Architect > Power: the three carbon conduits. The waterproof one should only be placeable where vanilla waterproof conduits are. The hidden one should be invisible once built.
6. Spawn c-composite and synthdiamond and check stuff options: c-composite should be offered for wood and metal items, both should be missing from sharp melee weapon bills by default, and synthdiamond should be missing from apparel bills by default.
