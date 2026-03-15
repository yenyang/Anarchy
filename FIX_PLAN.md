# Anarchy Mod Fix Plan — v1.5.5f1 Compatibility

## Approach: Source-Based Recompilation

Anarchy is **open source** (MIT license, GitHub: yenyang/Anarchy). Rather than patching a decompiled DLL (fragile, lossy, error-prone), we will:

1. Clone the source repository
2. Update it for game v1.5.5f1 API changes
3. Recompile against the new game assemblies
4. Replace the installed mod DLL with our fixed build

This is the correct approach because:
- Decompiled code loses comments, variable names, and structure
- The mod has 12,737 lines of decompiled C# — too complex to patch blindly
- The source repo has full build infrastructure, project files, and test context
- We need the game's v1.5.5f1 reference assemblies to compile against

---

## Phase 1: Setup & Source Acquisition

### Step 1.1 — Clone Anarchy Source
```
git clone https://github.com/yenyang/Anarchy.git ~/AnarchyModFix/src
```

### Step 1.2 — Identify Game Assembly Location
- Find CS2's managed assemblies directory (typically `Cities2_Data/Managed/`)
- These contain the game's .NET DLLs that the mod references
- Key assemblies: `Game.dll`, `Colossal.Core.dll`, `Colossal.UI.dll`, `Unity.Entities.dll`

### Step 1.3 — Analyze API Differences
- Compare the mod's expected API (v1.5.2f1) against current game assemblies (v1.5.5f1)
- Focus on: `PrefabSystem`, `PrefabID`, `NetToolSystem`, `ToolbarUISystem`, `ToolUISystem`
- Use ILSpy to decompile relevant game DLLs and diff method signatures

---

## Phase 2: Error Workarounds (Harmony Patches)

### Step 2.1 — Workaround for Error #1: PrefabSystem.RemovePrefab NullRef

Add a Harmony **prefix** patch on `Game.Prefabs.PrefabSystem.RemovePrefab` that:
- Checks if the prefab's `PrefabID` has null fields before the method executes
- If null, logs a warning and skips the removal (returns false to skip original)
- This prevents the NullReferenceException without affecting normal prefab removal

```csharp
[HarmonyPatch(typeof(PrefabSystem), "RemovePrefab")]
public static class PrefabSystem_RemovePrefab_Patch
{
    public static bool Prefix(PrefabBase prefab)
    {
        try
        {
            var id = prefab.GetPrefabID(default);
            // If GetPrefabID returns an ID with null type/name, skip
            if (string.IsNullOrEmpty(id.GetName()) || string.IsNullOrEmpty(id.GetType()))
                return false; // skip original method
        }
        catch
        {
            return false; // skip if PrefabID itself throws
        }
        return true; // proceed normally
    }
}
```

### Step 2.2 — Workaround for Error #2: DefaultResourceHandler Collection Modified

Add a Harmony **prefix/transpiler** patch on `Colossal.UI.DefaultResourceHandler.RequestResourceAsync` that:
- Wraps the list enumeration in a try-catch for `InvalidOperationException`
- OR replaces the `List<T>` iteration with a snapshot-based approach (`.ToArray()`)
- This is trickier since it's an async coroutine — may need a transpiler to inject `ToList()` before the foreach

Alternative approach: Since this error is transient (4 occurrences during load, then stops), we may choose to **suppress rather than fix** — add an exception filter that silences this specific stack trace during mod initialization. The errors are non-fatal and don't affect functionality.

---

## Phase 3: API Compatibility Fixes

### Step 3.1 — PrefabID Constructor Changes
The decompiled code shows `PrefabID` being constructed with a 3-argument constructor:
```csharp
new PrefabID("FencePrefab", "RetainingWall", default(Hash128))
```
If v1.5.5f1 changed the `PrefabID` struct (added/removed fields, changed `Equals()`), we need to update all ~30 PrefabID usages in the mod.

### Step 3.2 — Harmony Patch Target Verification
The mod has 6 Harmony patches targeting game methods:
1. `NetToolSystem.InitializeRaycast` — Postfix
2. `ToolbarUISystem.ActivatePrefabTool` — Postfix
3. `ToolbarUISystem.OnUpdate` — Prefix
4. `ToolUISystem.GetElevationRange` — Prefix
5. `UniqueAssetTrackingSystem.IsPlacedUniqueAsset` — Postfix
6. `UniqueAssetTrackingSystem.OnCreate` — Postfix

Each must be verified against v1.5.5f1:
- Method signatures may have changed (parameters added/removed)
- Methods may have been renamed or moved to different classes
- Internal field layouts accessed via reflection may have shifted

### Step 3.3 — ECS Component Compatibility
The mod registers 3 ECS systems:
- Various systems interacting with `PlaceableNetData`, `Upgraded`, `SubLane`, `PrefabRef`
- If any ECS component struct layouts changed in v1.5.5f1, the mod will silently produce wrong behavior

### Step 3.4 — UI Module (.mjs) Compatibility
- Check if `cs2/api` binding names changed
- Check if CSS class names or DOM structure changed
- The mod uses `Anarchy.css` and `Anarchy.mjs` — verify they still target valid UI hooks

---

## Phase 4: Build & Test

### Step 4.1 — Update Project References
- Point the `.csproj` at v1.5.5f1 game assemblies
- Update any NuGet package versions if needed
- Ensure `0Harmony.dll` version is compatible

### Step 4.2 — Compile
- Build the solution
- Fix any compilation errors from API changes
- Produce new `Anarchy.dll`

### Step 4.3 — Deploy & Test
- Back up original mod files
- Replace `Anarchy.dll` in `mods_subscribed/74604_37/`
- Launch game with Anarchy enabled
- Verify:
  - [ ] No errors on startup
  - [ ] Anarchy toggle works (Ctrl+A)
  - [ ] Object placement with Anarchy works
  - [ ] Network anarchy options appear
  - [ ] Error check panel opens (gear icon)
  - [ ] Transform lock works
  - [ ] No new errors in Player.log

---

## Phase 5: Ongoing Maintenance

### Step 5.1 — Document Changes
- Record all API changes found between v1.5.2f1 and v1.5.5f1
- Create a diff of our modifications

### Step 5.2 — Consider Contributing Upstream
- If fixes are clean, open a PR on yenyang's GitHub repo
- The mod has 884K subscribers — fixes benefit the community

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Game assemblies have breaking API changes | High | Build fails | Decompile game DLLs, find new signatures |
| Harmony patches target removed methods | Medium | Mod crashes | Verify each patch target, update signatures |
| Native DLL (Anarchy_win_x86_64.dll) incompatible | Low | Feature broken | May need to rebuild native component too |
| ECS component layouts changed | Medium | Silent bugs | Compare component structs between versions |
| Fix breaks save compatibility | Low | Data loss | Test with existing saves, keep backups |

## Files We'll Modify
- `Anarchy.dll` — Main managed mod assembly (recompiled from source)
- Possibly `Anarchy.mjs` / `Anarchy.css` — If UI bindings changed
- `Anarchy_win_x86_64.dll` — Only if native interop broke (unlikely)
