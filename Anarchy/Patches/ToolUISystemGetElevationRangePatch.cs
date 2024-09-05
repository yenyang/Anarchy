// <copyright file="ToolUISystemGetElevationRangePatch.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Patches
{
    using Anarchy.Systems.NetworkAnarchy;
    using Colossal.Mathematics;
    using Game.Tools;
    using Game.UI.InGame;
    using HarmonyLib;
    using Unity.Entities;

    /// <summary>
    /// Patches ToolUISystem GetElevation range to exand elevation range if option selected.
    /// </summary>
    [HarmonyPatch(typeof(ToolUISystem), "GetElevationRange")]
    public class ToolUISystemGetElevationRangePatch
    {
        /// <summary>
        /// Patches Unique Asset Tracking System IsPlacedUniqueAsset to return false.
        /// </summary>
        /// <returns>True so that the original method runs.</returns>
        public static bool Prefix(ref Bounds1 __result)
        {
            NetworkAnarchyUISystem networkAnarchyUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<NetworkAnarchyUISystem>();
            ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            if ((networkAnarchyUISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.ExpanedElevationRange) == NetworkAnarchyUISystem.Composition.ExpanedElevationRange && toolSystem.activePrefab != null)
            {
                __result = new Bounds1(-1000f, 1000f);
                return false;
            }

            return true;
        }
    }
}
