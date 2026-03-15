# Anarchy Mod - Error Documentation

## Environment
- **Game**: Cities: Skylines II v1.5.5f1 (released 2026-03-12)
- **Mod**: Anarchy v1.7.22 (Paradox Mods ID: 74604, version 37)
- **Last Mod Update**: 2026-01-18 (compiled against game v1.5.2f1)
- **Author**: yenyang
- **Source**: https://github.com/yenyang/Anarchy
- **Dependency**: Unified Icon Library (Paradox Mods ID: 74417)
- **Subscribers**: 884,613

---

## Error #1: NullReferenceException in PrefabSystem.RemovePrefab

**Severity**: ERROR (non-fatal, game continues)
**Count in log**: 1 occurrence
**Triggered by**: Game's playset change pipeline, not Anarchy code directly

### Stack Trace
```
NullReferenceException: Object reference not set to an instance of an object
  at Game.Prefabs.PrefabID.Equals(PrefabID other)
  at Dictionary`2.FindEntry(TKey key)
  at Dictionary`2.TryGetValue(TKey key, TValue& value)
  at Game.Prefabs.PrefabSystem.RemovePrefab(PrefabBase prefab)
  at Game.SceneFlow.GameManager.<<RegisterPdxSdk>g__OnEntryIsInActivePlaysetChanged|15>d.MoveNext()
```

### Analysis
- The game is trying to remove prefab `CarTruckTrailer01_LoadPlaceholder01` (a `StaticObjectPrefab`)
- The `PrefabID.Equals()` null reference means the prefab's type name or object name field is null
- This happens during `OnEntryIsInActivePlaysetChanged` — the game's mod activation pipeline
- **Root cause**: The 1.5.5f1 update likely renamed, restructured, or removed this truck trailer placeholder prefab. The PrefabSystem's internal dictionary has a stale entry with a null ID field. This is a **base game bug** triggered during mod loading, not caused by Anarchy's code.

### Fixability
- **Cannot be fixed in Anarchy** — the crash is inside `Game.Prefabs.PrefabSystem.RemovePrefab()`, which is game engine code
- Could potentially be **worked around** with a Harmony prefix patch on `PrefabSystem.RemovePrefab` that catches null PrefabIDs before the dictionary lookup

---

## Error #2: InvalidOperationException - Collection Modified During Enumeration

**Severity**: EXCEPTION (non-fatal, game continues)
**Count in log**: 4 identical occurrences (rapid succession)
**Triggered by**: Game's UI resource handler

### Stack Trace
```
InvalidOperationException: Collection was modified; enumeration operation may not execute.
  at List`1+Enumerator[T].MoveNextRare()
  at List`1+Enumerator[T].MoveNext()
  at Colossal.UI.DefaultResourceHandler+<RequestResourceAsync>d__36.MoveNext()
  at UnityEngine.SetupCoroutine.InvokeMoveNext()
```

### Analysis
- `Colossal.UI.DefaultResourceHandler.RequestResourceAsync` is iterating a `List<T>` while another thread/coroutine modifies it
- This is inside `Colossal.UI` (the game's UI framework), not Anarchy code
- Occurs during mod loading when the UI resource handler is fetching assets (CSS, JS, images) for multiple mods simultaneously
- **Root cause**: Race condition in the game's UI resource loading pipeline. When Anarchy registers its UI resources (`.mjs`, `.css`), the async resource handler's internal list is being modified concurrently. This is a **base game thread safety bug** exposed by mod loading timing.

### Fixability
- **Cannot be directly fixed in Anarchy** — the crash is inside `Colossal.UI.DefaultResourceHandler`
- The 4 rapid-fire occurrences suggest it's a timing issue during initial mod UI registration
- Anarchy's `.mjs` and `.css` files are loaded through this pipeline

---

## Error #3: HTTP 404 - Missing UI Resource

**Severity**: ERROR (non-fatal, cosmetic only)
**Count in log**: 2 occurrences
**Triggered by**: Another mod (Traffic mod)

### Details
```
[UI] [ERROR] ResourceHandler: HTTP/1.1 404 Not Found
  URL: coui://ui-mods/traffic-images/crowdin-icon-white.svg (Id: 1350)
  URL: coui://ui-mods/traffic-images/crowdin-icon-white.svg (Id: 1352)
```

### Analysis
- This is **NOT from Anarchy** — it's from a Traffic mod trying to load `crowdin-icon-white.svg`
- The SVG file is missing or the path changed in the Traffic mod
- Purely cosmetic — a missing icon

### Fixability
- **Not Anarchy-related** — ignore for this fix effort

---

## Summary

| # | Error | Source | Anarchy's Fault? | Fixable by Us? |
|---|-------|--------|------------------|----------------|
| 1 | NullRef in PrefabSystem.RemovePrefab | Game engine | No | Workaround possible (Harmony patch) |
| 2 | Collection modified during enumeration (x4) | Game UI framework | No | Workaround possible (Harmony patch) |
| 3 | HTTP 404 missing SVG (x2) | Traffic mod | No | No (different mod) |

**Key finding**: None of the errors are caused by Anarchy's own code. They are all base game bugs exposed by the v1.5.5f1 update. However, Anarchy was last compiled against v1.5.2f1 and may have **latent incompatibilities** in its Harmony patches and ECS system registrations that haven't surfaced as errors yet but could cause issues during gameplay (e.g., broken placement rules, missing UI elements, non-functional network upgrades).
