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
- **Current deploy**: Two locations: `mods_workInProgress/Anarchy/` AND `Mods/Anarchy/` (under CS2 user data root)

---

## Backup Log

| # | Timestamp | Location | Contents |
|---|-----------|----------|----------|
| 1 | 2026-03-15 11:44 | `backup/full_mod_20260315_114438/` | Full mod v1.7.23 first deploy |
| 2 | 2026-03-15 11:49 | `backup/full_mod_20260315_114939/` | Full mod with UI built from source |
| — | 2026-03-15 11:11 | `backup/Anarchy.dll.original` | Original v1.7.22 DLL from Paradox |
| — | 2026-03-15 11:35 | `backup/Anarchy.dll.fixed-v1.7.23` | Our patched DLL only |
