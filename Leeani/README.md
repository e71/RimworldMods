# Leeani Playable Race

Adds the Leeani, cheerful fox-like humanoids, as a **Biotech xenotype** of baseline humans (not a separate race). Their gene set mixes custom, vanilla and Alpha Genes genes. The mod also adds two NPC factions (medieval "Leeani hold" and spacer "Leeani starwalkers") and two player factions, pawn kinds, backstories, names, a "Leeani Loords" start scenario, staff weapons, a smart turret, furniture (throne, banquet table and chair, stone brazier, bookshelf), a grape plant, Soul Wine and Soul Spirit drugs, traders, and a Leeani research tab. With Processor Framework installed it also adds juice, cider, wine and kegs. Earlier versions were a Humanoid Alien Races (HAR) race with their own C# DLLs. The Biotech conversion (late 2025) removed both.

## Status

| | |
|---|---|
| Upstream author | MikeRobe (About.xml). The Biotech conversion in this repo is by Quicksilver Fox (commits `d4df63c` "WIP conversion to Biotech." and `35ba612` "Wip", both 2025-12-24) |
| packageId | `MikeRobe.LeeaniPlayableRace` |
| supportedVersions | 1.5, 1.6 (no 1.7) |
| modDependencies | `ludeon.rimworld.biotech` (Biotech DLC), `sarg.alphagenes` (Alpha Genes) |
| loadAfter | `ludeon.rimworld`, `ludeon.rimworld.royalty`, `ludeon.rimworld.biotech`, `ludeon.rimworld.odyssey`, `OskarPotocki.VanillaFactionsExpanded.Core`, `sarg.alphagenes`, `vat.epoeforked`; no loadBefore |
| Optional | Processor Framework (`syrchalis.processor.framework` or `zal.processorframework`, via LoadFolders), Royalty, Ideology (`MayRequire` in factions/traders), Vanilla Psycasts Expanded (`MayRequire="VanillaExpanded.VPsycastsE"`), EPOE, Mechanoids Extraordinaire, EPOE Vulpine Race Pack Patch (`PatchOperationFindMod`) |
| Steam workshop id | 1542414306 (`About/PublishedFileId.txt`) |
| About url | https://github.com/quicksilverfox/RimworldMods/releases/latest |

The Biotech conversion is unfinished: both commits are labelled WIP, and nothing has touched Leeani's content since (`d0a4699` only deleted a `Thumbs.db`).

**1.7: no 1.7 version exists.** I checked: there is no `1.7/` folder, no `v1.7` entry in `LoadFolders.xml`, no 1.7 in supportedVersions, and nothing for Leeani 1.7 on `origin/master` or `e71/master`. The workshop item is not downloaded locally either.

**DLL: the mod currently has no DLL and no C# code.** The only file in `Source/` that compiles is `Properties/AssemblyInfo.cs`, which is 35 lines of assembly attributes (title "Leeani", version 1.0.0.0, a COM GUID) and contains no logic. The real sources (`Building_FermentingVat`, `ExtraThingDef`, `VatProperties`, `WorkGiverDynamicDef`, fill/empty vat job drivers and work givers, a `HarmonyPatches` stub using Harmony 1.x `HarmonyInstance`, and the separate `Source/Vats` AutomatedVat project) were deleted in `35ba612`. The prebuilt `Leeani.dll`, `AutomatedVat.dll` and `0Harmony.dll` in `0.19/`-`1.3/Assemblies` were deleted with those folders in `d4df63c`. No `Assemblies` folder exists anywhere in the mod now. The csproj's Debug `OutputPath` is `..\..\..\Assemblies\`, which is `Leeani/Assemblies/` at the mod root (unversioned, so loaded by every `<li>/</li>` entry). Release builds would go to `bin\Release\`. The fermenting kegs those classes powered are now provided by Processor Framework (`1.6/Compat/ProcFrame`).

## Folder layout

`LoadFolders.xml` drives loading:

| Game version | Folders loaded |
|---|---|
| 1.4 | `/`, `1.4`, `1.4/Compat/ProcFrame` if `syrchalis.processor.framework` is active. **`1.4/` does not exist** |
| 1.5 | `/`, `1.5`, and `1.5/Compat/ProcFrame` with either Processor Framework packageId. **`1.5/` does not exist**, so on 1.5 only the root content loads, despite 1.5 being listed in supportedVersions |
| 1.6 | `/`, `1.6`, and `1.6/Compat/ProcFrame` with `syrchalis.processor.framework` or `zal.processorframework` |
| 1.7 | No entry. As far as I know, RimWorld then falls back to the newest lower version entry (`v1.6`). I have not verified this, and About.xml does not list 1.7, so the game flags the mod as incompatible |

| Folder | Content |
|---|---|
| `About/` | About.xml, PublishedFileId.txt |
| `Languages/English/` | `Strings/Leeani_Names/*.txt` (name banks used by `RulePacks_NameMakers_Leeani.xml`) and `Keyed/Misc_BigKeg.xml` and `Misc_CiderKeg.xml`. The keyed strings belonged to the deleted C# vat and keg classes and are now unused |
| `Sounds/Guns/` | `Leeanigun.ogg`, `XennGunOLD.ogg` |
| `Textures/` | `Gear/`, `Things/`, `World/` |
| `1.6/Defs/` | All current defs, see below. Also three disabled `.txt` files that RimWorld ignores: `Misc/Hairs_Leeani.txt`, `ThingDefs_Buildings/Buildings_Leeani_Factory.txt` and `Buildings_Leeani_Production.txt`. The last two are the old C#-based keg and vat defs (`Leeani.ExtraThingDef`, `Leeani.Building_FermentingVat`, `AutomatedVat.Building_AutomatedVat`) |
| `1.6/Patches/` | XML patches, see below |
| `1.6/Compat/ProcFrame/` | Processor Framework content: juices, cider/wine drugs, `ProcessorFramework.ProcessDef`s (cider, wine, soul wine, normal and fast), wine press and cider/wine kegs, industrial cider keg and wine vat, brewing research, juice recipes, trader and work giver patches |
| `Source/Leeani/` | `Leeani.sln`, `Leeani/Leeani.csproj` (old-style, see Build), `packages.config` (Unity container 5.11.7, unrelated to the Unity engine), `Properties/AssemblyInfo.cs` |

## How it works (1.6 content)

There is no code. It is XML defs and patches only.

- **Xenotype** `Leeani` (`XenotypeDefs/Xenotype_Leeani.xml`), inheritable, name maker `NamerPersonLeeani`. Its genes:
  - Custom (`GeneDefs/GeneDef_Leeani.xml`): `LeeaniTail`, `LeeaniHead`, `Skin_LeeaniFurOrange`, `Leeani_FurLeather`, `Leeani_QuickAndAgile` (+0.4 MoveSpeed, x0.8 MiningSpeed), `LeeaniPsychicSensitivity` (x1.25), `Leeani_CheerfulNature` (-0.02 mental break threshold; suppresses NaturalMood -1/-2; 50% NaturalMood +2, 50% Kind, 30% each Beauty +1/+2), `Leeani_Pacifist` (70% chance of trait `LeeaniPacifist`).
  - Vanilla Biotech, as labelled in the file (Biotech data is not installed here, so these defNames are unverified): `Body_Thin`, `Eyes_Red`, `DarkVision`, `Robust`, `MinTemp_SmallDecrease`, `Beauty_Pretty`, `AddictionResistant_Alcohol`, `NakedSpeed`, `Libido_High`, `Hair_Grayless`, `Sleepy`.
  - Alpha Genes: `AG_Digitigrade`, `AG_ReducedFertile`, `AG_SlowGestation`, `AG_Filthy`, `AG_HeatSlowness`, `AG_ToxResist_Vulnerability`, `AG_UVSensitivity_Minor`.
- **Head types** `LMAverage_Normal` and `LFAverage_Normal` (`HeadTypeDefs.xml`).
- **Factions**: `LeeaniHold` (Medieval), `LeeaniStarwalkers` (Spacer, with a raid loot `ThingSetMakerDef`), and player factions `LeeaniPlayerColony` and `LeeaniPlayerColonyTribal` ("No Research"), both Medieval. Ideology styles and memes use `MayRequire`.
- **Pawn kinds**: 12 hold kinds, 5 starwalker kinds and 2 player kinds (`LeeaniColonist`, `LeeaniSpacerColonist`), not counting one abstract base per file. They draw backstories from Leeani categories (`LeeaniBackstory`, `LeeaniSoldierBackstory`, `LeeaniScoutBackstory`, etc.) and vanilla `Offworld`.
- **Backstories**: 31 `AlienRace.AlienBackstoryDef` in `BackstoryDefs/BackstoryDef.xml`, with spawn categories `LeeaniBackstory` (30 of them), `LeeaniWorkerBackstory`, `LeeaniSoldierBackstory`, `LeeaniScoutBackstory`, and others.
- **Other defs**: scenario `Leeani Loords`, research tab `LeeaniResearchTab` plus `LeeaniSpacerTech`, trait `LeeaniPacifist`, joy giver and job for an archive, weapons (9 staff and gun defs in `Weapons_Leeani.xml`), apparel `LeeaniBycocket`, turret `Turret_LeeaniSmart`, furniture, grape plant `PlantLeeaniGrape` / `RawGrapes`, drugs `LeeaniSoulWine` / `LeeaniSoulSpirit` and recipe `Make_SoulSpirit`, traders (`Base_Leeani_Standard`, three caravans), tail items and hediffs and tail surgeries, a `BodyDef Leeani`, a `BodyPartDef LeeaniTail`, and filth.
- **Patches** (`1.6/Patches/`):
  - `Leeani_Patches.xml`: links `LeeaniBookshelf` as a facility to `HiTechResearchBench` and `SimpleResearchBench` (building the comps if missing), adds `PlaceWorker_ShowFacilitiesConnections` to the simple bench, adds the apparel tag `LeeaniPlayer` to six vanilla apparel defs, adds `LeeaniStoneBrazier` to the campfire bill work givers, and adds Leeani pawn kinds to `OutlanderFactionBase` combat, peaceful, trader-guard and settlement groups.
  - `Leeani_Royalty.xml`: adds `LeeaniPlayerColony` to the Empire's `permanentEnemyToEveryoneExcept`.
  - `Royalty_ThroneRoom_Patches.xml`: lets `LeeaniStoneBrazier` count as a brazier in throne room requirements.
  - `patch_EPOE.xml`, `patch_MechanoidsExtraordinaire.xml`, `patch_VulpineRacePack.xml`: tail surgery compatibility.

I checked the vanilla targets of `Leeani_Patches.xml` and the Royalty patches (`OutlanderFactionBase` group kinds, `DoBillsCookCampfire`/`DoBillsHaulCampfire`, Empire `permanentEnemyToEveryoneExcept`, throne room brazier requirements) against the installed 1.7 `Data/Core` and `Data/Royalty`, and they exist.

## Build

There is nothing to build for the current content. The mod is XML only, and no XML references a class in a Leeani assembly except the disabled `.txt` files.

`Source/Leeani/Leeani/Leeani.csproj` cannot be built, here or anywhere, and should not be "fixed" in place:
- It is an old-style (non-SDK) .NET Framework **v4.0** project. Running `dotnet build` on a scratch copy fails immediately with `MSB3644: The reference assemblies for .NETFramework,Version=v4.0 were not found`. This Linux machine has only the .NET 10 SDK and no Framework targeting packs.
- Its `Assembly-CSharp` and `UnityEngine.CoreModule` references are Windows HintPaths (`..\..\..\..\..\..\..\Games\SteamLibrary\steamapps\common\RimWorld\RimWorldWin64_Data\Managed\...`). Unity container references point at a `..\packages` folder that does not exist.
- It lists 8 `Compile` items that were deleted in `35ba612`. Only `Properties\AssemblyInfo.cs` exists.
- RimWorld 1.7 runs on CoreCLR .NET 10 and needs a modern project anyway.

If C# is ever needed again, create a new SDK-style csproj modelled on the other mods in this repo (`net10.0`, `Krafs.Rimworld.Ref` 1.7.4977-alpha.1 and `Lib.Harmony` 3.0.0-rc.2 with `ExcludeAssets="runtime"`, `OutputPath` `..\..\..\1.7\Assemblies\` or similar, `AppendTargetFrameworkToOutputPath` false). Delete the old project instead of converting it.

## Porting to 1.7 (not done)

Since there is no code, a port is XML work, but the 1.6 XML already uses things that don't exist in vanilla. I checked the 1.6 defs against the 1.7 reference assembly (`Krafs.Rimworld.Ref` 1.7.4977-alpha.1, read with `MetadataLoadContext`). I did not check them against 1.6.

1. **Dependencies first.** It needs Biotech, plus a 1.7 build of Alpha Genes (`sarg.alphagenes`) that still has the seven `AG_*` genes used by the xenotype. Processor Framework, EPOE and the others are optional. **Biotech is not installed on this machine** (`Data/` has only Core, Royalty, Ideology), so the mod can't be loaded or tested here at all.
2. **Folders and metadata.** Copy `1.6/` to `1.7/`. Add a `<v1.7>` block to `LoadFolders.xml` (`/`, `1.7`, and the two `IfModActive` ProcFrame entries pointing at `1.7/Compat/ProcFrame`). Add `<li>1.7</li>` to supportedVersions, and consider dropping 1.5, which has no content.
3. **Fix defs that don't match the 1.7 API:**
   - `GeneDef_Leeani.xml`: 1.7 `Verse.GeneDef` has no `graphicData`, `bodyPartGroupDefs`, `forcedHairTag` or `leatherDef` fields, and there is no `GeneGraphicData` type. These produce XML errors and do nothing. 1.7 has `GeneDef.fur` (a `FurDef`), `forcedHeadTypes` (list of `HeadTypeDef`), `forcedHair`, `hairTagFilter`/`beardTagFilter`, and `renderNodeProperties`. The tail needs a `renderNodeProperties` entry (or a `FurDef`), the head types need `forcedHeadTypes`, and the leather needs another mechanism.
   - `LeeaniHead` uses `<li Class="PawnRenderNodeProperties_Head">`. That class does not exist in 1.7 `Assembly-CSharp`.
   - `TraderKinds_*_Leeani.xml` use `StockGenerator_BuyCategory` (4 entries, `MayRequire` Ideology), which does not exist in 1.7. The `MayRequire` only skips them when Ideology is missing.
   - `BackstoryDef.xml` uses `AlienRace.AlienBackstoryDef`, a Humanoid Alien Races type. HAR is not a dependency, so without it all 31 backstories fail to load, and every pawn kind that draws from `LeeaniBackstory`-style `backstoryCategories` loses its backstory pool. Convert them to vanilla `BackstoryDef`. Every field they use (`title`, `titleShort`, `description`, `slot`, `spawnCategories`, `skillGains`, `workDisables`, `requiredWorkTags`, `forcedTraits`, `bodyTypeGlobal/Male/Female`) exists on 1.7 `RimWorld.BackstoryDef`, so renaming the element is probably enough. Verify in-game.
   - The tail surgeries (`Recipies_Ltail_Surgery.xml`) have `recipeUsers` `Alien_Leeani`, the old HAR race ThingDef that no longer exists. They use `appliedOnFixedBodyParts LeeaniTail`, but Leeani are now baseline humans and nothing adds a `LeeaniTail` part to the Human body. `BodyDef Leeani` (`XLee_Body.xml`) is not referenced by any race. All of this is dead content: either remove it, or patch a tail part onto the Human body.
   - Scenario defName `Leeani Loords` contains a space. Def names are normally letters, digits, `_` and `-`, so expect a config error.
4. **Then load it in-game** with dev mode on and work through the log. The class and field checks above do not cover def references (vanilla Biotech or Alpha Genes defNames, textures).

## Gotchas and known issues

- About.xml advertises 1.5 support, but 1.5 loads no defs.
- The description says "incompatible with the old Humanoid Alien Races versions", yet the backstories still need a HAR type (see porting notes).
- `.txt` files in `Defs/` are kept on purpose as disabled content. RimWorld only reads `.xml`. Don't rename them back without porting them to Processor Framework or new C#.
- The Royalty patches use `PatchOperationFindMod` with mod name "Royalty". The compat patches match by mod display names ("Expanded Prosthetics and Organ Engineering", "Mechanoids Extraordinaire", "EPOE Vulpine Race Pack Patch"), so a renamed mod silently disables its patch.
- `Leeani_Patches.xml` adds pawn kinds to `OutlanderFactionBase` and tags to vanilla apparel unconditionally. If those vanilla defs change, these patches fail with errors.
- The `Languages/English/Keyed` vat and keg strings are unused leftovers.

## Testing

Not possible on this machine without the Biotech DLC and Alpha Genes. On a machine that has them (on 1.6, or on 1.7 after porting):
1. Enable Biotech, Alpha Genes, then Leeani (optionally Processor Framework). Start in dev mode and read the log for XML errors in Leeani defs. The issues listed under Porting are the expected ones.
2. Start the "Leeani Loords" scenario, or generate pawns of kind `LeeaniColonist`. Check xenotype Leeani, the genes, the head and tail graphics, and that backstories come from the Leeani set.
3. Check that the world has the Leeani hold and starwalker factions and that they generate pawns. Look for Leeani pawns in outlander groups and traders.
4. Build the bookshelf next to research benches (facility link), the stone brazier (campfire bills, throne room brazier with Royalty), and the turret. Craft Soul Spirit. With Processor Framework, check juice, wine and cider production.
