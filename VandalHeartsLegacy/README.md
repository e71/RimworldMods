# Vandal Hearts Legacy Arrivals

These are storyteller events and quests that bring the "Legacy" characters to the colony. Legacy characters are the chase roster kept by Vandal Hearts Chase Characters (`../VandalHeartsChase`). The events only fire while at least one roster character is out in the world, free, and not reserved by another quest.

## Status

| | |
|---|---|
| Author | ellerain (the user's own mod, no upstream) |
| packageId | `ellerain.vandalheartslegacy` |
| Supported versions | 1.6, 1.7 |
| Dependencies | `ellerain.vandalheartschase` (it must be loaded, and this mod loads after it). Harmony isn't needed: this mod has no patches. |
| Workshop | not published |
| Local install | `~/.local/share/Steam/steamapps/common/RimWorld/Mods/VandalHeartsLegacy`, enabled in ModsConfig |

**1.7:** builds and loads. The 1.7 log showed no errors or def config errors. The four newer quests in `Legacy_MoreQuests.xml` (prisoner rescue, distress call, in hiding, fights beside you) have had less in-game play than the first three.

**1.6:** the 1.6 DLL is an older net472 build of the same source.

## The seven events

| Event | Def(s) | How it fires | What happens |
|---|---|---|---|
| Legacy pod crash | IncidentDef `Legacy_PodCrash` → QuestScriptDef `Legacy_PodCrash` (isRootSpecial) | Misc incident, baseChance 0.8, minRefireDays 8, needs colonists present | A vanilla refugee pod crash, with a roster character inside. They are made factionless, downed and recruitable. |
| Legacy returns | IncidentDef `Legacy_WandererReturns` → QuestScriptDef `Legacy_WandererReturns` (isRootSpecial) | Misc incident, baseChance 0.4, minRefireDays 12 | The vanilla "wanderer joins" walk-in, using a roster character. |
| Legacy on the run | QuestScriptDef `Legacy_ThreatReward_Raid_Joiner` (+ sub-scripts `Legacy_Util_JoinerThreat_Joiner`, `Legacy_Util_JoinerWalkIn`) | Random quest, rootSelectionWeight 0.8, expires in 0.3 days | A threat-reward quest: the character is chased by raiders, and joins if you accept and survive. |
| Legacy prisoner rescue | `Legacy_PrisonerRescue` + SitePartDef/GenStepDef `Legacy_PrisonerCamp` | Random quest, weight 0.8, autoAccept | A vanilla prisoner-camp site that holds the character. Break them out and they join. |
| Legacy distress call | `Legacy_DistressCall` + SitePartDef/GenStepDef `Legacy_DownedCompanion` | Random quest, weight 0.8, autoAccept | A vanilla downed-refugee site with the character hurt. Reach them and they join. |
| Legacy in hiding | `Legacy_InHiding` | Random quest, weight 0.8, expires in 2–4 days | The character lodges with you (still in their old faction) while raids hit. If you shelter them for `$hideTicks`, they stay. |
| Legacy fights beside you | `Legacy_FightsBesideYou` | Random quest, weight 0.8, expires in 0.3 days | The character walks in to help against a raid, and joins once it's beaten. |

"Random quest" means the storyteller's random quest chooser can pick the script, because `rootSelectionWeight` is above 0. Every script gates itself in the test run through `QuestNode_GetLegacyCharacter`: with nobody available, the quest can't generate.

## Folder layout

`LoadFolders.xml` loads:

- 1.6: `Common` then `1.6`
- 1.7: `Common` then `1.7`

```
About/About.xml
Common/Defs/Legacy_Incidents.xml     the two IncidentDefs
Common/Defs/Legacy_Quests.xml        pod crash, returns, on the run (+ two util sub-scripts)
Common/Defs/Legacy_MoreQuests.xml    prisoner rescue, distress call, in hiding, fights beside you
Common/Defs/Legacy_Sites.xml         Legacy_PrisonerCamp, Legacy_DownedCompanion (SitePartDef + GenStepDef each)
1.6/Assemblies/VandalHeartsLegacy.dll  prebuilt net472
1.7/Assemblies/VandalHeartsLegacy.dll  net10 build output of Source/
Source/LegacyEvents.cs               all code
Source/VandalHeartsLegacy.csproj
```

The Defs used to live in a version folder; they were moved to `Common/` during the 1.7 port, so both versions share them. Letter and quest text is in English, inline in the defs and the C#. There's no Languages folder.

## How it works

All the code is in `Source/LegacyEvents.cs`, namespace `VandalHeartsLegacy`. It has no Harmony patches; everything plugs in through def classes.

### Roster and quest nodes

- **`LegacyRoster`**
  - `Available(friendlyOnly)` reads `ChaseTracker.Instance.chasePawns` from the Chase mod. It returns pawns that are alive, unspawned, not downed, `Free` world pawns and not quest-reserved. With `friendlyOnly`, it also requires a non-player faction that isn't hostile to you.
  - `Pick()` returns a random one of those.
- **`QuestNode_GetLegacyCharacter`**
  - Stores a roster pawn in the slate under `storeAs`. It is the stand-in for `QuestNode_GeneratePawn`.
  - The test run also stores a candidate, so later nodes can test against it.

### Sites

- **`LegacySiteUtility`**
  - `MoveIntoSite` takes the pawn from the slate key `legacyPawn`, removes it from world pawns and puts it into `SitePart.things`. The vanilla gen step spawns it when the site map is made.
  - `AddBaseRules` copies what `SitePartWorker.Notify_GeneratedByQuestGen` does. The base implementation can't be called past the vanilla subclass override.
  - `RelationsInfo` builds the "has these relationships with colonists" text.
- **`SitePartWorker_LegacyPrisoner`** extends `SitePartWorker_PrisonerWillingToJoin`. It makes the pawn a prisoner of the site faction and sets the `prisoner` slate key.
- **`SitePartWorker_LegacyDowned`** extends `SitePartWorker_DownedRefugee`. It damages the pawn until downed and unable to walk (no bleeding wounds) and sets the `refugee` slate key. If nobody comes, the base class heals them.

### Fights beside you

- **`QuestNode_LegacyAlly`** adds a `QuestPart_LegacyAlly`. Its slate inputs are `pawn`, `map`, `enemyFaction`, `walkInSpot` and four signal names.
- **`QuestPart_LegacyAlly`** (a `QuestPartActivable`):
  - **On `inSignalArrive`:** spawns the pawn at the walk-in spot under a `LordJob_AssistColony` and sends the "arrives to fight" letter.
  - **Every 250 ticks after it's enabled:** once more than 1000 ticks have passed and no hostile pawns of `enemyFaction` are left standing, or after `MaxFightTicks` = 20000, the pawn joins the player and the part sends `outSignalJoined`.
  - **If the pawn dies or despawns:** it sends `outSignalLost` instead.
  - **Why 20000:** the assist-colony lord leaves after 25000 ticks, so the cap must come first.
  - **Saved fields:** pawn, mapParent, enemyFaction, walkInSpot, signals, `arrived`, `finished`.

### Vanilla quest roots

- **`QuestNode_Root_LegacyPodCrash`** (extends `QuestNode_Root_RefugeePodCrash`) overrides `GeneratePawn_NewTemp` and `SendLetter_NewTemp`.
- **`QuestNode_Root_LegacyReturns`** (extends `QuestNode_Root_WandererJoin_WalkIn`) overrides `GeneratePawn_NewTemp`.
- **These method names change between game versions.** Vanilla has renamed `*_NewTemp` methods before. After a game update, check that both overrides still compile.

### `LegacyAftercare` (GameComponent)

This runs once a day, at `TicksGame % 60000 == 4242`. For every roster pawn back in the world and not quest-reserved, it:

- clears a non-player guest or prisoner status;
- clears `WillJoinColonyIfRescued`;
- heals them if they are downed.

That way, a character from a failed or expired rescue quest becomes eligible again for visitors and Legacy events.

## Build

```bash
dotnet build -c Release VandalHeartsLegacy/Source/VandalHeartsLegacy.csproj
```

- Targets net10.0 with `Krafs.Rimworld.Ref` 1.7.4977-alpha.1 and `Lib.Harmony` 3.0.0-rc.2, both reference-only.
- Has `<ProjectReference Include="..\..\VandalHeartsChase\Source\VandalHeartsChase.csproj" Private="false" />`. Building Legacy therefore also builds Chase into `VandalHeartsChase/1.7/Assemblies/`. `Private="false"` keeps a copy of the Chase DLL out of Legacy's Assemblies folder, which must contain only `VandalHeartsLegacy.dll`.
- Output: `1.7/Assemblies/VandalHeartsLegacy.dll`. Commit the rebuilt DLL.
- Rebuilding the 1.6 DLL needs a net472 project against 1.6 references (see the Chase README).

Install:

```bash
rsync -a --delete --exclude Source --exclude .git VandalHeartsLegacy/ ~/.local/share/Steam/steamapps/common/RimWorld/Mods/VandalHeartsLegacy/
```

## Gotchas

- **The C# classes are named in the XML.** Classes are referenced by `Class="VandalHeartsLegacy.…"` and `<workerClass>`. Renaming a class or a public `SlateRef` field breaks the defs without a compile error. The game log then shows `Could not find type` or `XML error` lines at startup.
- **Slate keys are shared between XML and code.** `legacyPawn`, `prisoner` and `refugee` are read by the vanilla gen steps and description rules.
- **Quests hold pawn references.** Removing the mod mid-quest leaves broken quest parts in the save. Let the quests finish, or cancel them first.
- **`QuestNode_Root_LegacyPodCrash.GeneratePawn_NewTemp` assumes `Pick()` returns a pawn.** This holds because `TestRunInt` checks `AnyAvailable()` first. Keep that check if you refactor.

## Testing

1. Use a game where Vandal Hearts Chase has placed characters; there must be at least one roster pawn out in the world.
2. Dev mode, the incidents:
   - Debug actions → "Execute incident" → `Legacy_PodCrash` or `Legacy_WandererReturns`.
3. Dev mode, the quests:
   - Debug actions → "Generate quest" (Quests category) → pick a `Legacy_*` QuestScriptDef.
   - If it isn't listed or fails to generate, nobody on the roster is available.
4. For the site quests (`Legacy_PrisonerRescue`, `Legacy_DistressCall`), check:
   - the site appears on the world map;
   - the character is on the map when the site is entered;
   - after the quest expires, `LegacyAftercare` frees and heals the character within a day.
