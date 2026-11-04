https://steamcommunity.com/sharedfiles/filedetails/?id=2877856030

## Developer handover notes

These notes cover the fork in `e71/RimworldMods` (forked from quicksilverfox/RimworldMods). The upstream readme above is only a workshop link. Note that it points to item 2877856030, while `About/PublishedFileId.txt` says 3236264529.

ResearchPowl replaces the vanilla research tab with an automatically laid-out research tree. Prerequisites are drawn as edges, nodes are coloured by tech level, and the tab has search, a content-pack (mod) filter and a research queue. Clicking a node replaces the queue with that project and its missing prerequisites. Shift+click appends, Alt+click prepends, clicking a queued node removes it, and you can reorder the queue by drag and drop, with undo/redo. In god mode, Ctrl+click finishes the project instantly. When a project finishes, the next queued project starts automatically. Shift+clicking the Research button opens the original vanilla tab. It is a fork of "ResearchPal - Forked", itself derived from Fluffy's Research Tree.

### Status

| | |
|---|---|
| Upstream authors | VinaLx, Owlchemist (About.xml); source files still carry Karel Kroeze (Fluffy) copyright headers. The 1.7 port and later changes in this repo are by Quicksilver Fox |
| packageId | `Owlchemist.ResearchPowl` |
| supportedVersions | 1.4, 1.5, 1.6, 1.7; `modVersion` 1.10.0 |
| Dependencies | `brrainz.harmony` (Harmony, workshop 2009463077) |
| loadAfter | `mlie.RemoveIndustrialStuff`; no loadBefore |
| Soft compatibility (by reflection, see `ModCompatibility.cs`) | `Ogam.Rimedieval`, `OskarPotocki.VanillaFactionsExpanded.Core` (storyteller tech limits), `OskarPotocki.VanillaVehiclesExpanded` (unstudied wrecks); any active packageId containing `randomresearch` switches the mod to read-only mode |
| Steam workshop id | 3236264529 (`About/PublishedFileId.txt`) |
| Harmony id | `Owlchemist.ResearchPowl` (`Content.PackageIdPlayerFacing`) |

1.7 history (`git log -- ResearchPowl`):
- `7163a0d` "1.7 ResearchPowl": net10 SDK csproj, and the IMGUI changes for 1.7. `GUIClip.Internal_Push/Pop` became `GUI.BeginClip/EndClip`, `FastGUI` now just calls `Graphics.DrawTexture`, and `Rect.m_*` field access became properties.
- `512c5c4` "Research Powl drag and drop fix": drag offset handling in `MainTabWindow_ResearchTree.StartDragging/HandleNodeDragging`. 1.7 changed the units of `Event.delta`, so the code avoids it.
- `5bd5c0f` "Experimental feature: random research node": `WildcardNode`, the wildcard handling in `Queue`, and `AnomalyKnowledge_HarmonyPatches.cs` (auto-assigns Anomaly knowledge projects).

The random research node is labelled experimental by its own commit message. Its strings, and the Anomaly auto-assign strings, exist only in English. The only TODO in the source is in `ResearchNode.DefAllowed`: a hard-coded rule for the `VFEM_Maynard` storyteller. `CompatibilityHooks.cs` is marked `/* Currently Does not work */`. The source builds cleanly for 1.7. I did not run it in-game as part of this handover.

### Folder layout

There is no `loadFolders.xml`. RimWorld loads the root folder, `Common/`, and the folder for the running version.

| Folder | Content |
|---|---|
| `About/` | About.xml, PublishedFileId.txt |
| `Common/Defs/MainTabDefs/MainTabWindow.xml` | Two `MainButtonDef`s, see below |
| `Common/Languages/` | Keyed strings, all with the `ResearchPal.` prefix. English is complete (about 92 keys). ChineseTraditional, French, German, Korean, Russian, Spanish and SpanishLatin are partial. ChineseTraditional also has DefInjected files that contain only commented-out entries |
| `Common/Textures/` | `Buttons/`, `Icons/`, `Lines/` used by `Assets.cs` |
| `1.4/`, `1.5/`, `1.6/` | `Assemblies/ResearchPowl.dll` only. **Prebuilt from older source. The current source is 1.7-only (net10, 1.7 IMGUI API) and cannot rebuild them** |
| `1.7/Assemblies/ResearchPowl.dll` | Built from `Source/` |
| `Source/` | `ResearchPowl.csproj`, `ResearchPowl.sln`, `Source.code-workspace` and the .cs files (the csproj sits next to the code and globs all .cs below it) |
| `LICENSE` | Upstream license |

### How it works

Code map (`Source/`):

| Area | Files | Notes |
|---|---|---|
| Mod entry, settings | `ResearchPowl.cs` | `ResearchPowl : Mod` applies Harmony `PatchAll()` and loads `ModSettings_ResearchPowl`. All settings are **static** fields on that class, used everywhere through `using Settings = ResearchPowl.ModSettings_ResearchPowl`. The settings window is a two-column `Listing_Standard`, and some options only show in dev mode (reset layout, hidden prerequisites, verbose logging). `WriteSettings()` calls `Tree.ResetLayout()`. A `[StaticConstructorOnStartup] Setup` turns on `readOnlyMode` when a `randomresearch` mod is active, unless `readOnlyModeSkip` is set |
| Main tab | `MainTabWindow_ResearchTree.cs`, `MainButtonWorker_ResearchPowl.cs` | Full-screen tab window. It handles zoom (its own matrix scaling and clip juggling), scrolling, the search box, the content-pack filter float menu (`MenuOfPacks`; Anomaly is redirected to the vanilla tab), and node dragging. The button worker draws the queue count on the Research button and opens the vanilla tab on Shift |
| Tree layout and drawing | `Graph/Tree.cs`, `Graph/NodeLayers.cs`, `Graph/Node.cs`, `Graph/DummyNode.cs`, `Graph/Edge.cs` | `Tree` builds nodes and edges from all `ResearchProjectDef`s (respecting the pack filter, hidden prerequisites and tech-level separation). `NodeLayers` does layered graph layout: barycenter ordering, brute-force swapping, splitting by mod or tab, alignment. `DummyNode`s carry long edges. The layout is built lazily and synchronously on first `PreOpen` (`Tree.WaitForInitialization`), and rebuilt by `Tree.ResetLayout()` |
| Research nodes | `Graph/ResearchNode.cs`, `Graph/WildcardNode.cs`, `Graph/Highlighting.cs` | Drawing, tooltips, unlock icons, search matching, availability (`GetAvailable`, `DefAllowed`: tech level cap, read-only mode, Rimedieval/VFE/VVE hooks, bench/techprint/mechanitor/analysis requirements), clicks and dragging. `WildcardNode` is the "Random research" pseudo node pinned at grid (1,1). It has no def and resolves to a random available project while it is at the head of the queue |
| Queue | `Queue/Queue.cs`, `Queue/Undo.cs`, `Queue/Queue_HarmonyPatches.cs` | `Queue : WorldComponent` (singleton `Queue._instance`) holds `List<ResearchNode>` and is saved in the world as defs (`Queue`), plus `wildcardIndex`, `wildcardTarget` and `wildcardPack`. It sets `Find.ResearchManager.currentProj` directly, draws the queue bar, handles drag and drop in the queue, keeps undo/redo state, runs `SanityCheck()` on tab open, and retries the wildcard about every 2500 ticks |
| Anomaly | `AnomalyKnowledge_HarmonyPatches.cs` | Auto-assigns an Anomaly knowledge project, see patches |
| Mod compat | `ModCompatibility.cs`, `CompatibilityHooks.cs` | Reflection lookups for Rimedieval, VFE storytellers and VVE. `CompatibilityHooks` holds empty hook methods meant for other mods to Harmony-patch (`IsHidden`, `PassCustomUnlockRequirements`, `CustomUnlockRequirementPrompts`). `IsHidden` is not called anywhere in the source |
| Misc | `Assets.cs`, `ResourceBank.cs`, `Constants.cs`, `FastGUI.cs`, `FloatMenu_Fixed.cs`, `Log.cs`, `Extensions/*` | Textures and tech-level colour tables (HSV by tech level index), translated strings (`ResearchPal.` keys), layout constants, texture drawing, a float menu at a fixed position, a `[ResearchPowl]`-prefixed logger (`Log.Debug` only prints with the dev-mode "verbose logging" setting), and def extension helpers (ancestors, unlocks, icons) |

Harmony patches (both use attributes and are applied by `PatchAll()`):

| Class | Target | Kind | Why |
|---|---|---|---|
| `DoCompletionDialog` (`Queue/Queue_HarmonyPatches.cs`) | `ResearchManager.FinishProject` | Prefix | `doCompletionDialog &= useVanillaResearchFinishedMessage`, so the vanilla "research finished" dialog only shows when that setting is on (it is on by default). The comment in the code ("we never want to show it") is outdated |
| same | `ResearchManager.FinishProject` | Postfix | If the project is finished, `Queue.TryStartNext(proj)`: removes it from the queue, moves the wildcard to the back if its pick was finished, sets the next current project, and sends its own "research finished / next in queue" letter when the vanilla dialog is off |
| `Patch_AutoAssignAnomalyResearch` (`AnomalyKnowledge_HarmonyPatches.cs`) | `ResearchManager.ApplyKnowledge(KnowledgeCategoryDef, float)` | Prefix | With Anomaly active and `autoAssignAnomalyResearch` on (default), when knowledge arrives for a category with no selected project, it selects one (preferring already-started projects, otherwise random) so the points are not lost |

Defs (`Common/Defs/MainTabDefs/MainTabWindow.xml`):
- `MainButtonDef Research` has the **same defName as the vanilla Research button, so it replaces it**. Its `workerClass` is `ResearchPowl.MainButtonWorker_ResearchTree` and its `tabWindowClass` is `ResearchPowl.MainTabWindow_ResearchTree`, with hotkey F6.
- `MainButtonDef ResearchOriginal` is the vanilla `MainTabWindow_Research` with `buttonVisible false`. It is reached through Shift+click, the Anomaly pack filter entry, and `Assets.MainButtonDefOf.ResearchOriginal` (`[DefOf]`).

Settings (Scribe keys in `ModSettings_ResearchPowl.ExposeData`) include tech-level separation, grouping (nothing / tabs / large mods with `MinimumSeparateModTech`), align to ancestors, search by description, pause and reset on open, progress display, queue index, shortcut tooltip, swap zoom mode, scroll/zoom speed, drag display delay, max allowed tech level and hiding disallowed tech, read-only mode, vanilla finished message, and Anomaly auto-assign.

### Build

The project is `Source/ResearchPowl.csproj`: SDK style, `net10.0`, `Krafs.Rimworld.Ref` 1.7.4977-alpha.1 and `Lib.Harmony` 3.0.0-rc.2 (both `ExcludeAssets="runtime"`), plus `Krafs.Publicizer` 2.3.0 (`PrivateAssets all`). `OutputPath` is `..\1.7\Assemblies\`, with `DebugType none` (no pdb), no deps.json and no copied references. Run from the mod folder:

```
dotnet build Source/ResearchPowl.csproj -c Release
```

I verified this in a scratch copy (`/tmp/claude-1000/build.*`): 0 warnings, 0 errors, output `1.7/Assemblies/ResearchPowl.dll` (118272 bytes, the same size as the committed one).

**Publicizer:** `<Publicize Include="Assembly-CSharp" />` publicizes the whole game assembly (`Assembly-CSharp`) at compile time. Only Assembly-CSharp is publicized, not Unity or Harmony. To find out what the code relies on, I removed that line in a scratch copy. The build then failed on:
- `FloatMenu_Fixed.SetInitialSizeAndPosition()` is declared `public override` of a member that is `protected` in vanilla. This is CS0507, and it stops compilation before any other errors are reported.
- Private game members: `Text.anchorInt`, `Text.wordWrapInt`, `Current.gameInt`, `ModsConfig.activeModsHashSet`, `ResearchManager.currentProj`, `FloatMenuOption.labelInt`, and `Pair<,>.first/.second`.

Both the committed and the freshly built DLL contain an `IgnoresAccessChecksToAttribute`, which the publicizer emits so the runtime allows these private accesses. If a game update renames any of these members, the build fails, and an already-built DLL fails when the affected code runs.

- Building inside the repo overwrites the committed `1.7/Assemblies/ResearchPowl.dll`. Build in a copy unless you mean to update it.
- The csproj sets `<Deterministic>false</Deterministic>`, so every rebuild produces a different DLL even without source changes, and git will show it as modified.
- `obj/` is created under `Source/` (gitignored).

### Gotchas and known issues

- **Replaces the vanilla `Research` MainButtonDef.** Any other mod that also defines `MainButtonDef Research` conflicts, and whichever loads last wins. The locally installed workshop `notfood.ResearchPal` (946390822) defines the same `MainButtonDef Research` and also uses `ResearchPal.*` translation keys. Don't run them together.
- The queue lives in a `WorldComponent` saved with the game. Removing the mod from a save drops the queue.
- `Queue._instance` is assumed to exist (e.g. `MainButtonWorker_ResearchTree.DoButton`, `PreOpen` calls `Queue._instance.SanityCheck()`). It is created with the world, so it only exists inside a game.
- The tree is laid out synchronously when the tab first opens. With many research mods, the first open can take a while (dev mode logs "Processed in ..."). `Tree.WaitForResearchNodes()` is a busy-wait loop left over from an older threaded design.
- `Tree.filteredOut` and edge fading use `ResearchProjectDef.index` stored as `ushort`.
- Read-only mode is turned on automatically when an active packageId contains `randomresearch`. Turning it off in settings sets `readOnlyModeSkip`, which stops the automatic switch.
- Anomaly knowledge projects (`knowledgeCategory != null`) are excluded from the tree. The pack filter's "Anomaly" entry opens the vanilla tab on the Anomaly research tab.
- `CompatibilityHooks` methods are no-ops and `IsHidden` is never called, so patching it has no effect. The VFE Maynard storyteller is a hard-coded special case.
- The random research and Anomaly auto-assign options have English strings only, so other languages show the English text for them.
- The 1.4-1.6 DLLs are frozen binaries (see Folder layout). Fixes in `Source/` reach 1.7 only.
- Anomaly, Biotech and Odyssey are not installed locally (only Core, Royalty, Ideology), so the Anomaly auto-assign patch and the mechanitor/analysis requirements can't be tested on this machine.

### Testing

1. Copy the mod folder into `~/.local/share/Steam/steamapps/common/RimWorld/Mods/` (it is not installed there now). Enable Harmony (workshop 2009463077), then ResearchPowl. Do not enable `notfood.ResearchPal` at the same time.
2. Start a dev-mode colony and check the log for exceptions with `[ResearchPowl]` or `ResearchPowl.` in the trace.
3. Press F6 or click Research. The tree should open full screen with tech-level bands. Check zoom (mouse wheel; Shift/Alt+wheel scrolls instead, swapped by the "swap zoom mode" setting), panning (middle-mouse drag, Shift+left drag, or the map camera keys), search, and the content-pack filter button.
4. Click an unresearched node deep in the tree: it and its missing prerequisites should be queued at the top bar. Try Shift+click (append) and Alt+click (prepend), drag nodes within the queue and from the tree into the queue, and undo/redo.
5. Finish the current project with dev tools (or god mode). The next queued project should become current. With "use vanilla research finished message" off in the mod settings, you should get the mod's own letter that names the next project.
6. Queue the "Random research" node (top-left). It should show "Now: <project>" and research it, then move to the end of the queue when that project finishes. Save and reload: the queue, including the wildcard position and pick, should be restored.
7. Shift+click the Research button: the vanilla research tab should open.
8. Open mod settings (category "ResearchPowl", translation key `ResearchPal`) and change grouping or tech-level separation. The layout should rebuild without errors.
