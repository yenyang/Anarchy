// <copyright file="UniqueAssetTrackingSystemOnCreatePatch.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Patches
{
    using HarmonyLib;
    using Unity.Entities;
    using Game.UI.InGame;

    /// <summary>
    /// Patches Unique Asset Tracking System OnCreate to disable the system.
    /// </summary>
    [HarmonyPatch(typeof(UniqueAssetTrackingSystem), "OnCreate")]
    public class UniqueAssetTrackingSystemOnCreatePatch
    {
        /// <summary>
        /// Patches Unique Asset Tracking System OnCreate to disable the system.
        /// </summary>
        public static void Postfix()
        {
            UniqueAssetTrackingSystem uniqueAssetTrackingSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<UniqueAssetTrackingSystem>();
            uniqueAssetTrackingSystem.Enabled = false;
        }
    }
}
