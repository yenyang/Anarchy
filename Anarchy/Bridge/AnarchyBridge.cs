// <copyright file="AnarchyBridge.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Bridge
{
    using Anarchy.Systems.Common;
    using Game.Tools;
    using Unity.Entities;

    /// <summary>
    /// A bridge class for other mods to tie into Anarchy.
    /// </summary>
    public static class AnarchyBridge
    {
        /// <summary>
        /// Tries to add a tool base system into Anarchy's list of compatible tools.
        /// </summary>
        /// <param name="tool">Toolbase system for tool to add.</param>
        /// <param name="alwaysDisableErrorChecks">Default is false where Anarchy is a toggle with Keybind or UI  on Tool Options Panel. True, whether Anarchy is enabled or not, all error checks will be disabled while that tool is active.</param>
        /// <returns>True if added. False if not.</returns>
        public static bool TryAddToolSystem(ToolBaseSystem tool, bool alwaysDisableErrorChecks = false)
        {
            AnarchyUISystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<AnarchyUISystem>();
            if (uiSystem is null ||
                tool is null ||
                tool.toolID is null)
            {
                return false;
            }

            return uiSystem.TryAddTool(tool.toolID);
        }
    }
}
