# Vandal Hearts Chase Characters

Companion mod for the user's `VandalHearts.pcp` Prepare Carefully preset. Prepare Carefully hands its extra (non-starting) world pawns to the game as "starting pawns left behind", and the game never uses those pawns again. This mod moves them into friendly factions when the game starts or a save loads. After that, each of them can turn up with visitor groups (vanilla or Hospitality) and trade caravans from their faction, so the player gets a chance to recruit them.

Vandal Hearts Legacy (`../VandalHeartsLegacy`) builds on the roster this mod keeps.

## Status

| | |
|---|---|
| Name | Vandal Hearts Chase Characters |
| Author | ellerain (the user; it's their own mod, with no upstream) |
| packageId | `ellerain.vandalheartschase` |
| supportedVersions | 1.6, 1.7 |
| Dependencies | Harmony (`brrainz.harmony`, workshop 2009463077) |
| loadAfter | `brrainz.harmony`, `Orion.Hospitality`, `edb.preparecarefully` |
| Workshop | not published |
| Local install | `~/.local/share/Steam/steamapps/common/RimWorld/Mods/VandalHeartsChase`, enabled in ModsConfig |

- **1.7:** built and running. The 1.7 game log showed no errors from this mod.
- **1.6:** the 1.6 DLL is an older net472 build, kept for 1.6 players. It was built from the same `ChaseCharacters.cs`.

## Folder layout

There is no `LoadFolders.xml`, so RimWorld loads the root folder plus the folder that matches the game version.

```
About/About.xml                 metadata
1.6/Assemblies/VandalHeartsChase.dll   prebuilt net472 (1.6). Not rebuilt by the current csproj.
1.7/Assemblies/VandalHeartsChase.dll   net10 build output of Source/
Source/ChaseCharacters.cs       all code
Source/VandalHeartsChase.csproj
```

The mod has no Defs, Patches or Languages. Its UI strings (the settings labels and the letter text) are hard-coded in English.

## How it works

All the code is in `Source/ChaseCharacters.cs`, namespace `VandalHeartsChase`.

### `ChaseMod` (Mod) and `ChaseSettings` (ModSettings)

- Settings:
  - `visitorChance` (default 0.5): per-character chance to join a visitor group from their faction.
  - `traderChance` (default 0.35): per-character chance to join a trade caravan.
- Both are edited with sliders under Mod Settings → "Vandal Hearts Chase".
- The constructor runs `harmony.PatchAll` (Harmony id `ellerain.vandalheartschase`). It then calls `HospitalityPatch.TryPatch`, which patches Hospitality only when that mod is loaded.

### `ChaseTracker` (GameComponent)

This is the roster. It is saved as `chasePawns` (a list of pawn references), and `ChaseTracker.Instance` returns the current game's tracker.

- **`StartedNewGame` / `LoadedGame` → `Adopt()`**
  - Adds every humanlike world pawn with `wasLeftBehindStartingPawn` to the roster.
  - Then calls `Rehome(sendLetter: true)`, which sends the "Old companions scattered" letter.
- **Daily tick (`TicksGame % 60000 == 1717`) → `Rehome(sendLetter: false)`**
- **`NeedsHome(pawn)`** is true for a world pawn that is either:
  - still in the `StartingPawnLeftBehind` situation, or
  - `Free` with a null or defeated faction.

  Recruited, dead and spawned characters are left alone.
- **`HomeCandidates`** accepts a faction that is:
  - visible, non-player, not defeated, not temporary, humanlike and not a permanent enemy;
  - without royal titles (this keeps the Empire out);
  - able to make Peaceful or Trader groups.

  Friendly factions are tried first. If there are none, any qualifying faction is used.
- **Placement**
  - Each homeless pawn goes to the candidate faction that currently has the fewest roster members, with random tie-breaking.
  - The pawn gets `SetFaction` and, when available, `ChangeKind(faction.def.basicMemberKind)`, and its `wasLeftBehindStartingPawn` is cleared.
  - Placements are logged with the prefix `[Vandal Hearts Chase]`.
- **`PickJoiners(faction, chance, alreadyComing)`**
  - Considers roster pawns of that faction who are alive, unspawned, not downed, `Free` world pawns, not already in the group and not reserved by a quest.
  - Each one is kept if `Rand.Chance(chance)` passes.
  - Vandal Hearts Legacy uses the same eligibility rules.

### Harmony patches

| Patch class | Target | Kind | Purpose |
|---|---|---|---|
| `NeutralGroupSpawnPatch` | `IncidentWorker_NeutralGroup.SpawnPawns` (private, patched by name) | Prefix + Finalizer | Sets a static `active` flag for the duration of vanilla visitor, trader and traveler spawning, and clears it even if spawning throws. |
| `GeneratePawnsPatch` | `PawnGroupMakerUtility.GeneratePawns` | Postfix | While `active` is set and the group kind is Trader or Peaceful, appends `PickJoiners(...)` to the generated pawns. Other group kinds (raids and so on) are untouched. |
| `HospitalityPatch` | `Hospitality.IncidentWorker_VisitorGroup:GetPawnsToSpawn` (looked up by string with `AccessTools.Method`) | Postfix, applied manually | Hospitality builds its own visitor groups, so it gets the same appending. If the method isn't found, nothing is patched. When the patch is applied, the log says "Hooked into Hospitality visitor groups." |

## Build

```bash
dotnet build -c Release VandalHeartsChase/Source/VandalHeartsChase.csproj
```

- Targets net10.0 and references `Krafs.Rimworld.Ref` 1.7.4977-alpha.1 and `Lib.Harmony` 3.0.0-rc.2, both reference-only (`ExcludeAssets="runtime"`).
- The output is written straight to `1.7/Assemblies/VandalHeartsChase.dll`, with no pdb, deps.json or copied references.
- Building rewrites the committed DLL. Commit the rebuilt DLL along with the source change.
- Vandal Hearts Legacy compiles against this project through a `ProjectReference`. If a public signature changes here (`ChaseTracker.Instance`, `chasePawns` or `PickJoiners`), rebuild Legacy too.
- **Rebuilding the 1.6 DLL** needs a net472 project that references the 1.6 game assemblies. The original 1.6 csproj used HintPaths into `RimWorldLinux_Data/Managed`, which the 1.7 install no longer has. If you ever need it, use `Krafs.Rimworld.Ref` 1.6.x with `net472`.

### Install into the game

```bash
rsync -a --delete --exclude Source --exclude .git VandalHeartsChase/ ~/.local/share/Steam/steamapps/common/RimWorld/Mods/VandalHeartsChase/
```

## Gotchas

- **The roster is pawn references inside the save.** Removing the mod from a save drops the component, which is harmless, but the factions stay where the mod put them.
- **Rehome checks `situation == Free && (Faction == null || defeated)`.** A recruited character keeps the player faction and is left alone. A character who is banished and ends up as a factionless world pawn is re-homed to an NPC faction on the next daily tick.
- **`basicMemberKind` changes the pawn kind.** The pawn's gear and backstory stay as Prepare Carefully made them; only the kind changes.
- **The `SpawnPawns` patch is by name.** If Ludeon renames the method, Harmony will throw at startup with "Undefined target method". Check the log after game updates.
- **The `active` flag is static and not thread-safe.** That's fine, because RimWorld generates pawns on the main thread.

## Testing

1. Start a new game with the VandalHearts.pcp preset, or any Prepare Carefully setup with extra world pawns.
2. You should get the "Old companions scattered" letter, and a `[Vandal Hearts Chase] Placed chase characters` entry in the log.
3. In dev mode, run the `TraderCaravanArrival` or `VisitorGroup` incident for a faction that has a chase character. Setting the chances to 100% in Mod Settings makes it deterministic.
4. The character should be in the arriving group.
5. With Hospitality enabled, check the log for the "Hooked into Hospitality" line.
