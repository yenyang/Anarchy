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
        /// Tryies to add a tool base system into Anarchy's list of compatible tools.
        /// </summary>
        /// <param name="tool">Toolbase system for tool to add.</param>
        /// <returns>True if added. False if not.</returns>
        public static bool TryAddToolSystem(ToolBaseSystem tool)
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
