# RimworldMods

This repo holds RimWorld mod projects. It's the user's (e71) fork of [quicksilverfox/RimworldMods](https://github.com/quicksilverfox/RimworldMods). Most of the mods are Quicksilver Fox's ("Oblitus"). Six were added in this fork: the two Vandal Hearts mods, the BetterCoolers/BetterVents 1.6+ ports and e71ImportantClean. The fork also changed HousekeeperAssistanceCat's 1.7 work behavior.

Every mod folder has its own `README.md` (`Readme.md` for ResearchPowl and StuffedFloors, which keep their upstream readme with a handover section added at the end). Each one has:

- status;
- folder layout;
- a code map with every Harmony patch;
- build and install commands;
- gotchas and test steps.

**Read the mod's README before changing that mod.**

> Upstream's published versions of these mods are on Steam:
> https://steamcommunity.com/profiles/76561198024949177/myworkshopfiles/?appid=294100

## The mods

| Folder | Mod | packageId | Versions | Origin | Code | Installed locally |
|---|---|---|---|---|---|---|
| [AnimalsLogic](AnimalsLogic/README.md) | Animals Logic: fixes and tweaks for animal behavior | `Oblitus.AnimalsLogic` | 0.19–1.7 | upstream | C# + Harmony | no; the workshop copy 1098354593 is enabled |
| [BetterCoolers16](BetterCoolers16/README.md) | Coolers placed inside walls | `ellerain.bettercoolers16` | 1.6, 1.7 | this fork (port of workshop 1430093399) | C# + Harmony, XML patch | yes |
| [BetterVents16](BetterVents16/README.md) | Vents placed inside walls | `ellerain.bettervents16` | 1.6, 1.7 | this fork (port of workshop 1430094853) | XML patch; DLL built from BetterCoolers16 | yes |
| [Carbon](Carbon/README.md) | Carbon fiber and diamond materials | `Oblitus.Carbon` | 0.19–1.7 | upstream | XML only | no |
| [e71ImportantClean](e71ImportantClean/README.md) | Clean the rooms where cleanliness matters first | `e71.ImportantClean` | 1.7 | this fork | C#, no Harmony | yes |
| [HousekeeperAssistanceCat](HousekeeperAssistanceCat/README.md) | Trainable cleaning and hauling cat (+ Pawnmorpher, Biotech, Odyssey extras) | `Oblitus.HousekeeperAssistanceCat` | 0.19–1.7 | upstream, with fork changes for 1.7 | C# + Harmony | yes (overrides workshop 1541620640) |
| [Leeani](Leeani/README.md) | Leeani fox-people race (Biotech xenotype) | `MikeRobe.LeeaniPlayableRace` | 1.5, 1.6 (no 1.7) | upstream (MikeRobe) | XML only; a leftover old-style csproj with no real code and no DLL | no |
| [MyLittlePlanet](MyLittlePlanet/README.md) | Smaller world sizes | `Oblitus.MyLittlePlanet` | 0.19–1.7 | upstream | C# + Harmony | no |
| [ResearchPowl](ResearchPowl/Readme.md) | Research tree UI (fork of ResearchPal) | `Owlchemist.ResearchPowl` | 1.4–1.7 | upstream (VinaLx, Owlchemist; maintained here) | C# + Harmony + Publicizer | no |
| [SmartSubcores](SmartSubcores/README.md) | Mechanoid subcores that automate buildings | `Oblitus.SubcoreAutomation` | 1.6, 1.7 | upstream | C# + Harmony (largest project, ~18k lines) | no |
| [StuffedFloors](StuffedFloors/Readme.md) | Floors made from any stuff (Fluffy's mod) | `Fluffy.StuffedFloors` | 1.5–1.7 | upstream | C# + Harmony | no |
| [SyncGrowth](SyncGrowth/README.md) | Plants in a field grow in sync | `Oblitus.SyncGrowth` | 1.2–1.7 | upstream | C# + Harmony | no |
| [VandalHeartsChase](VandalHeartsChase/README.md) | Prepare Carefully world pawns join factions and show up with visitors or traders | `ellerain.vandalheartschase` | 1.6, 1.7 | this fork | C# + Harmony | yes |
| [VandalHeartsLegacy](VandalHeartsLegacy/README.md) | Quests that bring those characters to you (needs Chase) | `ellerain.vandalheartslegacy` | 1.6, 1.7 | this fork | C# + quest XML | yes |

**Dependencies between mods in this repo:**

- VandalHeartsLegacy → VandalHeartsChase (About.xml dependency and a C# `ProjectReference`).
- HousekeeperAssistanceCat → e71ImportantClean (1.7 only, About.xml dependency, no code reference).
- BetterVents16's DLL is built by BetterCoolers16's project.

## Environment

The fork was last worked on with this setup:

| | |
|---|---|
| OS | Arch Linux |
| Game | RimWorld **1.7.4995** (Steam), at `~/.local/share/Steam/steamapps/common/RimWorld` |
| DLC installed | Core, Royalty, Ideology. **No Biotech, Anomaly or Odyssey**, so content gated on those (HousekeeperAssistanceCat's nekomata and sentience catalyst, Leeani) can't be tested on this machine. |
| Runtime | 1.7 runs on Ludeon's new Waylith engine on **CoreCLR / .NET 10**, not Unity or Mono. `Assembly-CSharp.dll` and `UnityStubs.dll` sit in the game root; `RimWorldLinux_Data/Managed` no longer exists. Mods can ship net10 DLLs (all of ours do) or net472 DLLs. |
| Mods folder (local mods) | `~/.local/share/Steam/steamapps/common/RimWorld/Mods`. Only the six fork mods are installed here, plus HousekeeperAssistanceCat. |
| Workshop mods | `~/.local/share/Steam/steamapps/workshop/content/294100/<id>` |
| Harmony | workshop item 2009463077 (`brrainz.harmony`); its `1.7/` folder is built for net10 |
| Config | `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config/` (ModsConfig.xml, `Mod_<id>_<Name>.xml` settings) |
| Log | `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Logs/Player.log` (previous run: `Player-prev.log`) |
| .NET SDK | `dotnet` 10.0.113. No decompiler is installed; `dotnet tool install -g ilspycmd` if you need to read game code. |

**Local mods take precedence.** When a local mod and a workshop mod have the same packageId, RimWorld gives the workshop one a `_steam` suffix and the local one wins. That's how the local HousekeeperAssistanceCat overrides the subscribed workshop copy.

**Don't edit while the game runs.** The game rewrites ModsConfig.xml on exit, so make changes only while it's closed, or let the user enable mods in-game.

## Repo conventions

### Mod folder layout

```
<Mod>/
  About/About.xml            name, packageId, supportedVersions, dependencies, load order
  LoadFolders.xml            (optional) which folders load per game version
  Common/                    (optional) content shared by several versions
  1.6/, 1.7/ …               per-version Defs/Patches/Assemblies
  Languages/, Textures/ …    shared assets
  Source/                    C# project(s); never installed into the game
  README.md
```

- Without `LoadFolders.xml`, RimWorld loads the root plus the folder matching the game version.
- If no entry matches the current version, LoadFolders falls back to the newest earlier entry.
- Old version folders (0.19–1.5, and 1.6 for most mods) hold **prebuilt DLLs that the current source does not rebuild**. Treat them as frozen releases.

### C# projects (all buildable ones)

Every SDK-style csproj follows the same pattern:

- **Framework:** `TargetFramework` is `net10.0`, with `LangVersion latest` and Nullable and ImplicitUsings off.
- **References:** `Krafs.Rimworld.Ref` **1.7.4977-alpha.1** and `Lib.Harmony` **3.0.0-rc.2**, both with `ExcludeAssets="runtime"` so neither is copied into the mod.
- **Output:** `OutputPath` is the mod's `1.7/Assemblies/`, with `AppendTargetFrameworkToOutputPath=false`, no deps.json, no reference assembly, no pdb and no copied lock-file assemblies. The Assemblies folder should only ever contain the mod's own DLL.
- **Stray pdb files:** MyLittlePlanet's Release config still uses `DebugType pdbonly`, and StuffedFloors' csproj doesn't set `DebugType none`, so they may drop a `.pdb` next to the DLL. Don't commit it (see those READMEs).
- **Ref version:** the game is at 1.7.4995, while the newest reference package used is 4977. When a newer `Krafs.Rimworld.Ref` 1.7 package exists, bump it everywhere at once.
- **Leeani** is the exception: its old .NET Framework 4.0 csproj has a Windows HintPath, points at deleted source files and doesn't build. The mod is XML-only. See its README.

**Built DLLs are committed.** After changing source, rebuild and commit the DLL in the same commit; players install the mod folder as-is.

### Build

There is no top-level solution. Build one project:

```bash
dotnet build -c Release <Mod>/Source/<Project>.csproj
```

Build all the SDK-style projects:

```bash
find . -name '*.csproj' -not -path '*/MigrationBackup/*' -not -path './Leeani/*' -print0 | xargs -0 -n1 dotnet build -c Release
```

`obj/` and `bin/` are gitignored. If you only want to check that something compiles without touching committed DLLs, copy the mod folder to a temp dir and build there.

### Install into the game (for the mods installed locally)

```bash
rsync -a --delete --exclude Source --exclude .git <Mod>/ ~/.local/share/Steam/steamapps/common/RimWorld/Mods/<Mod>/
```

Don't install Source, `obj` or the README. A mod that isn't installed locally yet also has to be enabled in the in-game mod menu, or in ModsConfig.xml while the game is closed.

### Text files

- Most XML uses LF. Some upstream files use **CRLF and a UTF-8 BOM**, for example `HousekeeperAssistanceCat/1.7/Defs/ThingDefs_Races/Races_Animal_HousekeeperCat.xml`, which also has no final newline.
- Keep a file's existing line endings when editing. Converting them turns the whole file into a diff.

### Shell safety

The agent sandbox blocked `rm -rf $VAR/...` with a bare variable. Use `"${VAR:?}"` or literal paths, or use `rsync --delete` instead of deleting.

## Git

| Remote | URL | Use |
|---|---|---|
| `e71` | `git@github-e71-rimworldmods:e71/RimworldMods.git` | **The user's fork. Push here.** |
| `origin` | `https://github.com/quicksilverfox/RimworldMods` | Upstream, read-only. Fetch to pick up upstream changes. |

- **Branch.** `master` is the only branch, and `master == e71/master`.
- **Push access** is through a GitHub **deploy key** with write access on e71/RimworldMods. The key is `~/.ssh/e71_rimworldmods` (ed25519). It is mapped by the `Host github-e71-rimworldmods` entry in `~/.ssh/config` (with `IdentitiesOnly yes`), so use that host alias, not `github.com`. The user's normal GitHub key belongs to a different account and can't push to e71.
- **Commit identity:** Viacheslav Rostovtsev <vrostovtsev@protonmail.com>.
- **Before you commit or push,** ask the user. They've asked for specific commits ("commit X, leave Y uncommitted") before.
- **Commit messages** follow upstream style: `<Mod>: <what changed>`, or `Add <Mod>: <summary>`.

Fork-specific history on top of upstream `ede8ddd`:

| Commit | What |
|---|---|
| a1a9a6d | Add e71ImportantClean |
| 021ffb6 | HousekeeperAssistanceCat: clean first, haul second, no care jobs; depends on e71ImportantClean |
| dbd0005 | Add VandalHeartsChase, VandalHeartsLegacy, BetterCoolers16, BetterVents16 (migrated from the game's Mods folder, with new SDK-style csprojs) |

## Testing in-game

- **Dev mode** (Options → Dev mode) gives the debug actions menu: spawn things and pawns, execute incidents, generate quests.
- **After every change,** check `Player.log` for exceptions and XML errors from the mod. Search for the mod's log prefix, for example `[Vandal Hearts Chase]` or `[HousekeeperCat]`.
- **Not tested in-game:**
  - e71ImportantClean and the HousekeeperAssistanceCat 1.7 priority changes. Both were built and installed, but not yet watched in play.
  - The upstream mods' 1.7 ports, as far as their commit messages say ("Needs testing" on SmartSubcores).

## Related files outside the repo

These are in the user's environment and aren't tracked here:

- **`VandalHearts.pcp`:** the Prepare Carefully preset that VandalHeartsChase is built around. The user's `.pcc` saved characters have psylinks capped at level 4.
- **Duplicate QualityBuilder in the mod list.** The last 1.7 log had one real error not caused by our mods: QualityBuilder was loaded twice. The fix is to unsubscribe from workshop item 754637870.
- **Mod backups:** a tarball of the fork's mods from before the 1.7 port is in a temporary agent scratch directory (`/tmp/claude-1000/…/scratchpad/ourmods.before-1.7.tgz`). It disappears on reboot; git history is the real backup.
