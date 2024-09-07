// <copyright file="NetToolSystem_InitializeRaycast.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// This code was originally part of Extended Road Upgrades by ST-Apps. It has been incorporated into this project with permission of ST-Apps.
namespace Anarchy.Patches
{
    using Game.Net;
    using Game.Tools;
    using HarmonyLib;

    /// <summary>
    ///     <para>
    ///         This patch just adds more flags to <see cref="NetToolSystem"/>'s <see cref="ToolRaycastSystem"/>.
    ///     </para>
    ///     <para>
    ///         The following flags are added:
    ///         <list type="bullet">
    ///             <ul>
    ///                 <see cref="Layer.Pathway"/> - enables upgrading pedestrian paths
    ///             </ul>
    ///             <ul>
    ///                 <see cref="Layer.TrainTrack"/> - enables upgrading train tracks paths
    ///             </ul>
    ///             <ul>
    ///                 <see cref="Layer.PublicTransportRoad"/> - enables upgrading bus roads
    ///             </ul>
    ///         </list>
    ///     </para>
    /// </summary>
    [HarmonyPatch(typeof(NetToolSystem), "InitializeRaycast")]
    internal class NetToolSystem_InitializeRaycast
    {
        static void Postfix(NetToolSystem __instance)
        {
            var logHeader = $"[{nameof(NetToolSystem_InitializeRaycast)}.{nameof(Postfix)}]";

            var toolRaycastSystem = Traverse.Create(__instance).Field<ToolRaycastSystem>("m_ToolRaycastSystem").Value;
            if (toolRaycastSystem == null)
            {
                AnarchyMod.Instance.Log.Error($"{logHeader} Failed retrieving ToolRaycastSystem instance, exiting.");
                return;
            }

            if (__instance.actualMode == NetToolSystem.Mode.Replace)
            {
                toolRaycastSystem.netLayerMask |= Layer.Pathway | Layer.TrainTrack | Layer.PublicTransportRoad | Layer.TramTrack | Layer.SubwayTrack;
            }
        }
    }
}
