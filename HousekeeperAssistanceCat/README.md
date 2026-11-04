# HousekeeperAssistanceCat

This mod adds the **Housekeeper Cat**, a domesticated animal that is halfway to a person. Once a cat has Obedience trained, it runs the real work givers that colonists use (not animal stubs) for these work types:

- Firefighting
- Cleaning
- Hauling
- Plant cutting

Cats also shear for a small amount of quality wool (`Wool_HKCat`), and they wander in as a rare event (they sometimes self-tame).

There are three optional extras:

- **Pawnmorpher:** partial morphs and former humans.
- **Biotech:** a "nekomata" cat-person xenotype.
- **Odyssey:** a sentience catalyst used on a cat awakens it into a humanlike colonist.

## Status

| | |
|---|---|
| Author (About.xml) | Edge Crescent (original, 2017). This continuation was reworked by quicksilverfox ("Oblitus"). The 1.7 changes in this fork are by the user (e71). |
| packageId | `Oblitus.HousekeeperAssistanceCat` |
| supportedVersions | 0.19, 1.0 – 1.7 |
| Dependencies | Harmony on every version. On **1.7** also `e71.ImportantClean` (see `../e71ImportantClean`), declared in `modDependenciesByVersion/v1.7`. |
| loadAfter | brrainz.harmony, erdelf.HumanoidAlienRaces, PeteTimesSix.CompactHediffs, tachyonite.pawnmorpherpublic, SirMashedPotato.Ashlands, kentington.saveourship2, e71.ImportantClean |
| Workshop | upstream is Steam item 1541620640, which the user is also subscribed to |
| Local install | `~/.local/share/Steam/steamapps/common/RimWorld/Mods/HousekeeperAssistanceCat`, enabled |

**Two copies, same packageId.** When a local mod and a workshop mod share a packageId, RimWorld renames the workshop one with a `_steam` suffix, so the **local copy is the one that loads**. Keep the local install in sync with the repo.

**1.7 state.** These are the fork's changes in commit 021ffb6. The game hasn't been fully play-tested since.

- **Work priorities are fixed:**

  | Priority | Work type |
  |---|---|
  | 1 | Firefighter |
  | 2 | Cleaning |
  | 3 | Hauling |
  | 4 | PlantCutting |

  They are re-applied on every spawn, so cats from older saves pick them up too.
- **The care-work extras are turned off** (`extraEnabledWorkGivers` is empty). These extras are feeding patients and prisoners, rescue, warden and childcare jobs. Before, they ran ahead of the cats' chores, and the feeding jobs failed.
- **The 1.7 dependency on Important Clean.** The cats need no code for this: they pick up the new Cleaning work giver automatically, because they run every work giver of the Cleaning work type.

**1.6 still has the old behavior.** The `1.6/` defs still list the care-work extras, and `1.6/Assemblies/HousekeeperCat.dll` (a net48 build from commit d0a4699) predates the priority change.

## Folder layout

`loadFolders.xml` loads `/` and the version folder for each version. From 1.2 on, it adds `Pawnmorpher/<ver>` if `tachyonite.pawnmorpherpublic` is active. From 1.6 on, it adds `Biotech/<ver>` if Biotech is active.

```
About/                 About.xml, ModIcon.png, preview.png, PublishedFileId.txt (1541620640)
Languages/             English (DefInjected + Keyed: Incidents.xml, Social.xml), ChineseSimplified,
                       ChineseTraditional, Japanese (DefInjected only)
Textures/              shared art (cat, nekomata parts, items)
0.19 … 1.5/            frozen content + prebuilt DLLs for old game versions (0.19/1.0 also ship 0Harmony.dll;
                       0.19–1.2 DLL is HouseCatCleaningJob.dll, 1.3+ is HousekeeperCat.dll). Not built from current source.
1.6/                   Defs, Patches, Assemblies/HousekeeperCat.dll (net48, prebuilt)
1.7/                   Defs, Patches, Assemblies/HousekeeperCat.dll (net10, built from Source/)
Biotech/1.6, 1.7/      GeneDefs_Nekomata.xml, XenotypeDefs_Nekomata.xml
Pawnmorpher/1.2 … 1.7/ morph defs, former-human backstories, Pawnmorpher patches
Source/                HouseCatCleaningJob.sln + HouseCatCleaningJob/HousekeeperCat.csproj
```

### `1.7/` contents

- **`Defs/ThingDefs_Races/Races_Animal_HousekeeperCat.xml`**: the race, its PawnKindDef and the `HousekeeperCatExtension`.
  - `thingClass` is `HousekeeperCat.Pawn_HousekeeperCat`.
  - `mechEnabledWorkTypes` is Hauling, Cleaning, Firefighter and PlantCutting.
  - Trainability is Advanced.
  - The Odyssey trainables are Comfort, Dig, Forage and AttackTarget (`MayRequire`).
  - There are resistance stats for Biotech, Ashlands, SOS2 and Odyssey (`MayRequire`).
  - **File format:** the file has CRLF line endings, a UTF-8 BOM and no final newline. Keep them when editing, or the diff becomes the whole file.
- **`Defs/ThinkTreeDefs/HousekeeperCat.xml`**: inserted at `Animal_PreWander`. Faction plus pawnKind HousekeeperCat plus Obedience trained → `JobGiver_HousekeeperCat`.
- **`Defs/Storyteller/Incidents_Map_Misc.xml`**: `HousekeeperCatWanderIn` (baseChance 0.4).
- **`Defs/ThingDefs_Items/Items_HousekeeperCat.xml`**: `Leather_HKCat` (fur) and `Wool_HKCat`.
- **`Defs/BackstoryDefs/Backstories_Awakened.xml`**: `HKCat_AwakenedChildhood`. Its spawn category is `HKCat_Awakened`, which no PawnKind uses, so normal pawns never roll it.
- **`Defs/PawnRelationDefs/PawnRelations_Awakened.xml`**: `HKCat_Bonded`, a permanent mutual +50 opinion.
- **`Patches/ADogSaid.xml`**: adds the cat to A Dog Said's animal surgery recipes.
- **`Patches/MorrowRim.xml`**: a MorrowRim compatibility patch. Both patches use `PatchOperationFindMod`.

## How it works (`Source/HouseCatCleaningJob/`)

The namespace is `HousekeeperCat` (the csproj RootNamespace says `RimWorld`, but the files declare `HousekeeperCat`).

### `Patch.cs`

- `HousekeeperCat : Mod` runs `harmony.PatchAll` with the id `net.quicksilverfox.rimworld.mod.housekeepercat`.
- `Patch_ITab_Pawn_Character_IsVisible` is a postfix on `ITab_Pawn_Character.get_IsVisible`. It hides the Bio tab for cats, because they get a `story` tracker that the tab would otherwise show. Pawnmorpher former humans keep the tab.

### `Pawn_HousekeeperCat.cs`

This is the pawn subclass.

- **`SpawnSetup`**
  - Adds the trackers that animals don't have but the work system needs:
    - `skills`, every skill at level 6 (to keep pricing neutral);
    - `story` (thin body type);
    - `workSettings`.
  - Then refreshes the disabled work types.
  - Then sets the priorities listed under Status.
  - Former humans (`IsFormerHuman()`, meaning the story has a childhood or adulthood) are skipped entirely.
- **`GetDisabledWorkTypes`**
  - Uses `new` to hide the base method, so only direct calls on the subclass type reach it.
  - Disables every work type that isn't in `RaceProps.mechEnabledWorkTypes`, and caches the list.
- **`ExtraEnabledWorkGivers`**: read from the `HousekeeperCatExtension` DefModExtension (`extraEnabledWorkGivers`) on the race def.
- **`DefOfHousekeeperCatThingDef` / `DefOfHousekeeperCatPawnDef`**: `[DefOf]` holders for the cat's ThingDef and PawnKindDef.

### `JobGiver_HousekeeperCat.cs`

This is a copy of vanilla `JobGiver_Work.TryIssueJobPackage`. The main difference is that it lets a non-colonist animal use the work givers.

- **Priority:** `GetPriority` is a fixed 5.5. The cats treat every hour as "anything" time.
- **1.7 port changes:**
  - Skip the job while in labor (humanlike only, for former humans).
  - The "already carrying it" check is used when choosing a target: a carried thing is a valid target and is preferred.
- **`MergeExtraWorkGivers`**
  - Puts the extra-enabled work givers ahead of the normal list. They are sorted by work type `naturalPriority`, then `priorityInType`, and respect the emergency flag.
  - It doesn't mutate the cached list.
  - It does nothing on 1.7 now that the list is empty.
- **If you port it again:** diff against the decompiled vanilla `JobGiver_Work` of the new game version.

### `IncidentWorker_HousekeeperCatWander.cs`

- Spawns a wild cat at the map edge, walking toward the center of the map.
- If player pawns are present, there's a 25% chance it joins the player with Obedience already trained.
- Letter keys: `LetterLabelHousekeeperCatWandersIn`, `LetterHousekeeperCatWandersIn`, `LetterLabelHousekeeperCatJoin` and `LetterHousekeeperCatJoin` (in `Languages/English/Keyed/Incidents.xml`).

### `SentienceConversion.cs` (Odyssey)

- **Trigger:** a postfix on `Pawn_HealthTracker.AddHediff`. When the SentienceCatalyst hediff lands on a spawned, non-former-human cat, the conversion is deferred to a safe point.
- **`SentienceConversion.Convert`**
  - Generates a fresh humanlike pawn: the nekomata xenotype `HKCat_Nekomata` with Biotech, or plain story fields without it. The pawn gets no gear.
  - Copies over the cat's name (if it had a real one), age (cat years 6–80 mapped onto human years 13–79), health conditions (body parts matched by BodyPartDef and index; the Monkey body maps closely onto the human one) and animal bonds (as `HKCat_Bonded`).
  - Sets fixed traits (nudist, fast learner, kind) and the backstory `HKCat_AwakenedChildhood` + `Colonist97`.
  - Starts every skill at 0 and gives 3 random passions.
  - Spawns the new pawn where the cat stood and only then vanishes the cat: no corpse and no letter.
  - With Biotech, adds a xenogerm-style recovery coma.
  - If any step fails before the cat is vanished, it's logged with the prefix `[HousekeeperCat] Sentience conversion aborted…`, the cat is left alone and the catalyst hediff is removed, so it can be retried.
- **Known limitations** (noted in the source):
  - Only spawned cats convert.
  - Relations other than the bond are dropped.
  - The result is always an adult.

### `AwakenedBond.cs`

Postfixes that make the `HKCat_Bonded` relation matter socially:

| Patch | Effect |
|---|---|
| `Pawn_RelationsTracker.CompatibilityWith` | Flat compatibility bonus for bonded pairs. |
| `InteractionWorker_RomanceAttempt.SuccessChance` | Raises a chance that's already above 0 to `RomanceAcceptChance`. Vanilla hard zeros stay at 0. |
| `InteractionWorker_RomanceAttempt.RomanceFactors` | Adds the tooltip line `HKCat_RomanceBondFactor` (in `Languages/English/Keyed/Social.xml`). |

### Other

`Properties/AssemblyInfo.cs` is hand-written, so the csproj sets `GenerateAssemblyInfo=false`.

## Build

```bash
dotnet build -c Release HousekeeperAssistanceCat/Source/HouseCatCleaningJob/HousekeeperCat.csproj
```

- Targets net10.0 with `Krafs.Rimworld.Ref` 1.7.4977-alpha.1 and `Lib.Harmony` 3.0.0-rc.2, both reference-only.
- Output goes to `HousekeeperAssistanceCat/1.7/Assemblies/HousekeeperCat.dll`.
- `Source/HouseCatCleaningJob/obj/` is gitignored.
- Commit the rebuilt DLL along with the source.

Install. The local copy overrides the workshop one, so this step is what actually changes the game:

```bash
rsync -a --delete --exclude Source --exclude .git HousekeeperAssistanceCat/ ~/.local/share/Steam/steamapps/common/RimWorld/Mods/HousekeeperAssistanceCat/
```

## Gotchas

- **The trackers are a deliberate hack.** Animals with `story`, `skills` and `workSettings` trackers work, but other mods that assume "has story ⇒ humanlike" can misbehave. The Bio-tab patch exists because of this. Expect similar issues with UI mods.
- **The `new` methods are not overrides.** `WorkTagIsDisabled`, `WorkTypeIsDisabled` and `GetDisabledWorkTypes` only take effect when the caller's static type is `Pawn_HousekeeperCat`. Vanilla calls through `Pawn` see the base behavior, which is driven by `mechEnabledWorkTypes`.
- **Priorities are hard-coded in `SpawnSetup`.** The player can't change them; animals have no work tab. To bring the care jobs back, move the commented entries in the race def back into `extraEnabledWorkGivers`. Those jobs will then run ahead of the chores again.
- **Important Clean is a soft dependency.** It's declared in About.xml, so the mod menu flags it as missing without Important Clean. There's no code reference to it, though. Without it the cats still work, with vanilla nearest-first cleaning.
- **Translations.** Only English has Keyed files. New keyed strings fall back to the key name in the other languages.
- **The old version folders are frozen.** Never edit them unless you're specifically fixing an old-version release.

## Testing

1. Dev mode: spawn a `HousekeeperCat` (tamed), or use "Execute incident" → `HousekeeperCatWanderIn`.
2. Train Obedience (dev: "Train" on the animal).
3. Put filth in a kitchen and a corridor, plus items to haul. The cat should do firefighting, then cleaning (important rooms first, from Important Clean), then hauling.
4. Check that it never tries to feed prisoners or patients.
5. Odyssey + Biotech (neither is installed on the user's machine, so this path is untested): use a sentience catalyst on a tamed cat. It should become a nekomata colonist, in a recovery coma, keeping its name and bond.
