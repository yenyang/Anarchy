# Anarchy Mod — Issues & Fixes Log

## Environment
- **Game**: Cities: Skylines II v1.5.5f1 (2026-03-12)
- **Mod**: Anarchy v1.7.22 → v1.7.23 (our patched build)
- **Source**: https://github.com/yenyang/Anarchy (cloned to ~/AnarchyModFix/src)

---

## Issue #1: NullReferenceException in PrefabSystem.RemovePrefab
- **Status**: FIXED
- **Discovered**: 2026-03-15
- **Error**: `NullReferenceException` at `PrefabID.Equals()` when removing `CarTruckTrailer01_LoadPlaceholder01`
- **Root cause**: Game v1.5.5f1 changed/removed truck trailer prefab, leaving null PrefabID fields
- **Source**: Base game code (`Game.Prefabs.PrefabSystem.RemovePrefab`), not Anarchy
- **Fix**: Added Harmony prefix patch `PrefabSystemRemovePrefabPatch.cs` that checks for null PrefabID before dictionary lookup
- **File**: `Anarchy/Patches/PrefabSystemRemovePrefabPatch.cs`

## Issue #2: InvalidOperationException — Collection modified during enumeration
- **Status**: ACCEPTED (not patched)
- **Discovered**: 2026-03-15
- **Error**: `InvalidOperationException` in `Colossal.UI.DefaultResourceHandler.RequestResourceAsync` (4x during load)
- **Root cause**: Base game thread safety bug — async coroutine iterates a List while another thread modifies it
- **Source**: Base game code (`Colossal.UI`), not Anarchy
- **Decision**: Not patching — transient during load, non-fatal, no functional impact. Patching via Harmony transpiler on a sealed internal coroutine would be extremely fragile.

## Issue #3: HTTP 404 for crowdin-icon-white.svg
- **Status**: NOT OURS
- **Discovered**: 2026-03-15
- **Error**: `[UI] [ERROR] ResourceHandler: HTTP/1.1 404 Not Found` for `coui://ui-mods/traffic-images/crowdin-icon-white.svg`
- **Source**: Traffic mod, not Anarchy
- **Decision**: Ignore — belongs to a different mod

## Issue #4: Assembly name mismatch — DLL named "Anarchy.Standalone"
- **Status**: FIXED
- **Discovered**: 2026-03-15
- **Root cause**: Standalone csproj defaulted assembly name to `Anarchy.Standalone` instead of `Anarchy`
- **Fix**: Set `<AssemblyName>Anarchy</AssemblyName>` in csproj
- **File**: `Anarchy.Standalone.csproj`

## Issue #5: Mod shows as "0 bytes" in game mod manager
- **Status**: INVESTIGATING
- **Discovered**: 2026-03-15
- **Symptom**: Mod visible in mod manager but displays "0 bytes"
- **Analysis**: Game registers mod in `ModsAndPlaysetCache.json` as `LocalType: "WorkInProgress"` with `Id: 0`. WIP mods lack Paradox metadata size field. Also initially missing UI files (.mjs, .css), 0Harmony.dll, and locales — all now built from source.
- **Attempted fixes**:
  1. Added `mod.json` with correct id/version ✓
  2. Built UI from source (webpack) ✓
  3. Copied 0Harmony.dll from NuGet build output ✓
  4. Also deployed to `UserDataPath/Mods/Anarchy/` (standard local mod path) — **testing now**
- **Current deploy**: Removed both `mods_workInProgress/Anarchy/` and `Mods/Anarchy/` — reverting to subscribed approach (see Issue #6)

## Issue #6: Game crashes on launch — "Value cannot be null. Parameter name: source"
- **Status**: FIXING
- **Discovered**: 2026-03-15
- **Error**: `ArgumentNullException` in `PdxSdkPlatform.CreateMod()` → `GetModsInActivePlayset()` → LINQ `.Where()` on null source
- **Root cause**: After unsubscribing from Anarchy (74604), the Paradox SDK server-side state is inconsistent. The SDK's `CreateMod()` returns null for the unsubscribed mod, which gets passed to LINQ `.Where()`, crashing with `ArgumentNullException`.
- **Stack trace**: `PdxSdkPlatform.GetModsInActivePlayset` → `ParadoxModsDataSource.OnActivePlaysetChanged` → `ParadoxModsDataSource.Populate` → FATAL
- **Fix**: Re-subscribe to Anarchy on Paradox Mods to restore consistent SDK state, then replace DLL in subscribed folder with our patched build. The `mods_workInProgress` approach doesn't work reliably.
- **Lesson**: Don't unsubscribe from Paradox mods to use local copies — it breaks the Paradox SDK's server sync. Instead, stay subscribed and overwrite the DLL in place.

## Issue #7: NullReferenceException in AnarchyUISystem.OnGameLoadingComplete
- **Status**: FIXED
- **Discovered**: 2026-03-15
- **Error**: `NullReferenceException` at `AnarchyUISystem.OnGameLoadingComplete` — system disabled itself on error
- **Root cause**: `Settings.GetAction()` returns null `ProxyAction` in v1.5.5f1 for keybinding actions (ToggleAnarchy, ResetElevation, ElevationStep, ElevationKey, ElevationMimicKeys). The mod accessed `.shouldBeEnabled` and `.WasPerformedThisFrame()` on null references.
- **Fix**: Added null guards (`!= null`) on all 5 `ProxyAction` field usages across `OnGameLoadingComplete`, `OnUpdate`, `OnToolChanged`, and `OnPrefabChanged`.
- **File**: `Anarchy/Systems/Common/AnarchyUISystem.cs`

## Issue #8: "No suitable code replacement generated" — ECS source generators missing
- **Status**: CRITICAL — REVERTED to original DLL
- **Discovered**: 2026-03-15
- **Error**: `InvalidOperationException: No suitable code replacement generated` in `ElevateObjectDefinitionSystem.OnCreate`, cascading to `AnarchyUISystem` and `AnarchyTooltipSystem`
- **Root cause**: Our standalone csproj compiles without Unity ECS source generators (SystemGenerator, JobEntityGenerator, etc.). These generators produce required runtime code for systems using `SystemAPI` and `IJobEntity`. Without them, the ECS framework can't initialize the systems.
- **Impact**: The entire mod fails to initialize — no Anarchy functionality at all
- **Resolution**: Reverted to original v1.7.22 DLL which has the source-generated code baked in. The original DLL works with v1.5.5f1 (APIs are compatible) — it just has the two non-fatal startup errors from Issues #1 and #2.
- **Lesson**: Cannot recompile CS2 ECS mods without the full Unity modding toolchain including source generators. A partial recompile breaks the generated system code.

## Issue #9: Hardcoded dev path for en-US.json
- **Status**: NON-FATAL (cosmetic, ignore)
- **Discovered**: 2026-03-15
- **Error**: `DirectoryNotFoundException: Could not find path "C:\Users\TJ\source\repos\Anarchy\...\en-US.json"`
- **Root cause**: The original author (yenyang, user "TJ") has a debug feature that writes locale files to their dev machine path. Fails harmlessly for everyone else.
- **Impact**: None — the mod continues loading normally after this error

---

## Backup Log

| # | Timestamp | Location | Contents |
|---|-----------|----------|----------|
| 1 | 2026-03-15 11:44 | `backup/full_mod_20260315_114438/` | Full mod v1.7.23 first deploy |
| 2 | 2026-03-15 11:49 | `backup/full_mod_20260315_114939/` | Full mod with UI built from source |
| — | 2026-03-15 11:11 | `backup/Anarchy.dll.original` | Original v1.7.22 DLL from Paradox |
| — | 2026-03-15 11:35 | `backup/Anarchy.dll.fixed-v1.7.23` | Our patched DLL only |
