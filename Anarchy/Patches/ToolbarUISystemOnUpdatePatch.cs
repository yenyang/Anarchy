// <copyright file="ToolbarUISystemOnUpdatePatch.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Patches
{
    using Anarchy.Utils;
    using Game.UI.InGame;
    using HarmonyLib;
    using Unity.Entities;

    /// <summary>
    /// Patches Unique Asset Tracking System IsPlacedUniqueAsset to return false.
    /// </summary>
    [HarmonyPatch(typeof(ToolbarUISystem), "OnUpdate")]
    public class ToolbarUISystemOnUpdatePatch
    {
        /// <summary>
        /// Patches Unique Asset Tracking System IsPlacedUniqueAsset to return false.
        /// </summary>
        /// <returns>True so that the original method runs.</returns>
        public static bool Prefix()
        {
            if (AnarchyMod.Settings.AllowPlacingMultipleUniqueBuildings)
            {
                ToolbarUISystem toolbarUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolbarUISystem>();
                toolbarUISystem.SetMemberValue("m_UniqueAssetStatusChanged", false);
                UniqueAssetTrackingSystem uniqueAssetTrackingSystem1 = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<UniqueAssetTrackingSystem>();
                if (uniqueAssetTrackingSystem1.Enabled == true)
                {
                    uniqueAssetTrackingSystem1.Enabled = false;
                }

                return true;
            }

            UniqueAssetTrackingSystem uniqueAssetTrackingSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<UniqueAssetTrackingSystem>();
            if (uniqueAssetTrackingSystem.Enabled == false)
            {
                uniqueAssetTrackingSystem.Enabled = true;
            }

            return true;
        }
    }
}
