// <copyright file="ToolbarUISystemActivatePrefabTool.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Patches
{
    using Anarchy.Utils;
    using Colossal.Entities;
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI.InGame;
    using HarmonyLib;
    using Unity.Entities;

    /// <summary>
    /// Patches Unique Asset Tracking System IsPlacedUniqueAsset to return false.
    /// </summary>
    [HarmonyPatch(typeof(ToolbarUISystem), "ActivatePrefabTool")]
    public class ToolbarUISystemActivatePrefabToolPatch
    {
        /// <summary>
        /// Patches Toolbar UISystem Activate Prefab Tool to allow for multiple uniques being placed.
        /// </summary>
        /// <param name="assetEntity">The prefab entity selected.</param>
        public static void Postfix(Entity assetEntity)
        {
            ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            PrefabSystem prefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();

            if (assetEntity != Entity.Null && !prefabSystem.EntityManager.HasEnabledComponent<Locked>(assetEntity) && AnarchyMod.Settings.AllowPlacingMultipleUniqueBuildings)
            {
                if (prefabSystem.TryGetPrefab(assetEntity, out PrefabBase prefab))
                {
                    toolSystem.ActivatePrefabTool(prefab);
                }
            }
        }
    }
}
