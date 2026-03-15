// <copyright file="PrefabSystemRemovePrefabPatch.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Patches
{
    using Game.Prefabs;
    using HarmonyLib;

    /// <summary>
    /// Harmony prefix patch on PrefabSystem.RemovePrefab to prevent NullReferenceException
    /// when the game tries to remove a prefab with a null PrefabID (e.g., after game updates
    /// restructure or remove prefab assets like CarTruckTrailer01_LoadPlaceholder01).
    /// </summary>
    [HarmonyPatch(typeof(PrefabSystem), "RemovePrefab")]
    internal class PrefabSystemRemovePrefabPatch
    {
        /// <summary>
        /// Checks for null prefab or null PrefabID fields before the original RemovePrefab
        /// executes its dictionary lookup, which would throw NullReferenceException.
        /// </summary>
        /// <param name="prefab">The prefab being removed.</param>
        /// <returns>True to run original method, false to skip it.</returns>
        static bool Prefix(PrefabBase prefab)
        {
            if (prefab == null)
            {
                return false;
            }

            try
            {
                PrefabID id = prefab.GetPrefabID(default);
                string name = id.GetName();
                if (name == null)
                {
                    AnarchyMod.Instance?.Log?.Warn($"Skipping RemovePrefab for prefab with null PrefabID name: {prefab.name ?? "(null)"}");
                    return false;
                }
            }
            catch
            {
                AnarchyMod.Instance?.Log?.Warn($"Skipping RemovePrefab due to exception in PrefabID for: {prefab.name ?? "(null)"}");
                return false;
            }

            return true;
        }
    }
}
