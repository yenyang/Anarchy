// <copyright file="UniqueAssetTrackingSystemIsPlacedUniqueAssetPatch.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Patches
{
    using HarmonyLib;
    using Unity.Entities;
    using Game.UI.InGame;

    /// <summary>
    /// Patches Unique Asset Tracking System IsPlacedUniqueAsset to return false.
    /// </summary>
    [HarmonyPatch(typeof(UniqueAssetTrackingSystem), "IsPlacedUniqueAsset")]
    public class UniqueAssetTrackingSystemIsPlacedUniqueAssetPatch
    {
        /// <summary>
        /// Patches Unique Asset Tracking System IsPlacedUniqueAsset to return false.
        /// </summary>
        /// <param name="entity">From original method.</param>
        /// <param name="__result">Result from original method.</param>
        public static void Postfix(Entity entity, ref bool __result)
        {
            if (AnarchyMod.Settings.AllowPlacingMultipleUniqueBuildings)
            {
                __result = false;
            }
        }
    }
}
