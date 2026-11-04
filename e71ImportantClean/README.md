# Important Clean (e71)

With this mod, cleaners clean the rooms where cleanliness matters before the rest of the home area. It adds a "clean important rooms" work giver to the vanilla Cleaning work type, ranked ahead of vanilla filth cleaning and snow or sand clearing. Any pawn that does Cleaning uses it, including modded worker animals such as the Housekeeper Assistance Cat (`../HousekeeperAssistanceCat`), which depends on this mod on 1.7.

## Status

| | |
|---|---|
| Name | Important Clean (e71) |
| Author | e71 (the user's GitHub account) |
| packageId | `e71.ImportantClean` |
| Supported versions | 1.7 only |
| Dependencies | Core only. No Harmony and no patches. |
| URL | https://github.com/e71/RimworldMods |
| Workshop | not published |
| Local install | `~/.local/share/Steam/steamapps/common/RimWorld/Mods/e71ImportantClean`, enabled in ModsConfig |

New in this fork (commit a1a9a6d). It builds cleanly, but **it hasn't been verified in-game yet**: when it was written, the game was running, so it was enabled afterwards and still needs a play test.

## Folder layout

`loadFolders.xml` loads `/` and `1.7` on 1.7.

```
About/About.xml
loadFolders.xml
1.7/Defs/WorkGiverDefs.xml                 WorkGiverDef e71_CleanImportantRooms
1.7/Assemblies/e71ImportantClean.dll       net10 build output
Source/WorkGiver_CleanImportantRooms.cs    all code
Source/e71ImportantClean.csproj
```

## How it works

### The def: `1.7/Defs/WorkGiverDefs.xml`

`WorkGiverDef e71_CleanImportantRooms`:

| Field | Value |
|---|---|
| `giverClass` | `e71ImportantClean.WorkGiver_CleanImportantRooms` |
| `workType` | `Cleaning` |
| `priorityInType` | `15` |
| `requiredCapacities` | `Manipulation` (same as vanilla) |

In vanilla, `ClearSnowOrSand` has priorityInType 10 and `CleanFilth` has 5. Inside a work type, higher values are tried first. `JobGiver_Work` only falls through to a lower priorityInType when the higher one found no target, so filth in important rooms is cleaned first.

### The code: `Source/WorkGiver_CleanImportantRooms.cs`

The class extends vanilla `WorkGiver_CleanFilth`. Job creation is inherited unchanged, including how one cleaning job batches nearby filth. It overrides two methods:

- **`PotentialWorkThingsGlobal`** returns only the filth in `listerFilthInHomeArea.FilthInHomeArea` that is in an important room. It reuses one static list to avoid allocations.
- **`HasJobOnThing`** checks `InImportantRoom(t) && base.HasJobOnThing(...)`.

`InImportantRoom` is true when the room exists, isn't `PsychologicallyOutdoors`, and its `Role` has `IsStatRelated(RoomStatDefOf.Cleanliness)`. In vanilla, these roles use Cleanliness in their stat calculation:

- Bedroom, Barracks
- PrisonCell, PrisonBarracks
- DiningRoom, RecRoom
- Hospital, Laboratory, Kitchen

Modded room roles that use Cleanliness are picked up automatically.

## Build

```bash
dotnet build -c Release e71ImportantClean/Source/e71ImportantClean.csproj
```

- Targets net10.0 and references only `Krafs.Rimworld.Ref` 1.7.4977-alpha.1, as reference-only. There's no Harmony reference because nothing is patched.
- Output: `1.7/Assemblies/e71ImportantClean.dll`.
- `Source/obj/` is build cache. It's gitignored, so don't commit it.

Install:

```bash
rsync -a --delete --exclude Source --exclude .git e71ImportantClean/ ~/.local/share/Steam/steamapps/common/RimWorld/Mods/e71ImportantClean/
```

## Gotchas

- **The static list is shared.** `PotentialWorkThingsGlobal` returns a static list that it clears on each call. That's safe because the work scan is single-threaded and consumes the enumerable before the next call. Don't hold on to the result.
- **Safe to add to or remove from a save.** It's only a def and a class, with no saved data.
- **Vanilla behavior is unchanged.** Pawns still do the vanilla `CleanFilth` for everything else once the important rooms are clean. A home area with filth only outside important rooms behaves exactly as in vanilla.
- **1.6 isn't supported.** The code would probably compile against 1.6, but there's no 1.6 folder or DLL.

## Testing

1. Make filth both in a kitchen or hospital and in a hallway or storeroom (dev mode → spawn filth), inside the home area.
2. Give a pawn only Cleaning work.
3. Every important-room filth should be cleaned before the hallway, even when the hallway is closer.
4. In the work tab, hovering the Cleaning column should list "clean important rooms" above "clear snow" and "clean filth".
