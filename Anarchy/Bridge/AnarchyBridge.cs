// <copyright file="AnarchyBridge.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Bridge
{
    using Anarchy.Components;
    using Anarchy.Systems.AnarchyComponentsTool;
    using Anarchy.Systems.Common;
    using Game.Tools;
    using Unity.Collections;
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

        /// <summary>
        /// Tries to add anarchy component to an instance entity.
        /// </summary>
        /// <param name="instanceEntity">Entity for an instance of an object. Must be overridable non-building static object.</param>
        /// <returns>True if added. False if not.</returns>
        public static bool TryAddAnarchyComponent(Entity instanceEntity)
        {
            SelectedInfoPanelTogglesSystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            if (uiSystem.CheckOverridable(instanceEntity))
            {
                uiSystem.EntityManager.AddComponent<PreventOverride>(instanceEntity);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to add anarchy component to an instance entity.
        /// </summary>
        /// <param name="instanceEntity">Entity for an instance of an object. Must be overridable non-building static object.</param>
        /// <param name="transform">Transform position and rotation to lock object.</param>
        /// <returns>True if added. False if not.</returns>
        public static bool TryAddTransformLockComponent(Entity instanceEntity, Game.Objects.Transform transform)
        {
            SelectedInfoPanelTogglesSystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            if (uiSystem.CheckDisturbable(instanceEntity) &&
               !uiSystem.EntityManager.HasComponent<TransformRecord>(instanceEntity))
            {
                TransformRecord transformRecord = new TransformRecord()
                {
                    m_Position = transform.m_Position,
                    m_Rotation = transform.m_Rotation,
                };
                uiSystem.EntityManager.AddComponent<TransformRecord>(instanceEntity);
                uiSystem.EntityManager.SetComponentData(instanceEntity, transformRecord);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Tries to add anarchy component to an instance entity in an entity query.
        /// </summary>
        /// <param name="entities">Entity query you want to try to add component to.</param>
        public static void AddAnarchyComponent(NativeArray<Entity> entities)
        {
            if (entities.Length == 0)
            {
                return;
            }

            SelectedInfoPanelTogglesSystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            for (int i = 0; i <= entities.Length; i++)
            {
                if (uiSystem.CheckOverridable(entities[i]))
                {
                    uiSystem.EntityManager.AddComponent<PreventOverride>(entities[i]);
                }
            }
        }

        /// <summary>
        /// Tries to add anarchy component to an instance entity in an entity query.
        /// </summary>
        /// <param name="entityQuery">Entity query you want to try to add component to.</param>
        public static void AddAnarchyComponent(EntityQuery entityQuery)
        {
            if (entityQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            NativeArray<Entity> entities = entityQuery.ToEntityArray
            SelectedInfoPanelTogglesSystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            if (uiSystem.CheckOverridable(instanceEntity))
            {
                uiSystem.EntityManager.AddComponent<PreventOverride>(instanceEntity);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to add anarchy component to an instance entity.
        /// </summary>
        /// <param name="instanceEntity">Entity for an instance of an object. Must be overridable non-building static object.</param>
        /// <param name="transform">Transform position and rotation to lock object.</param>
        /// <returns>True if added. False if not.</returns>
        public static bool TryAddTransformLockComponent(Entity instanceEntity, Game.Objects.Transform transform)
        {
            SelectedInfoPanelTogglesSystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            if (uiSystem.CheckDisturbable(instanceEntity) &&
               !uiSystem.EntityManager.HasComponent<TransformRecord>(instanceEntity))
            {
                TransformRecord transformRecord = new TransformRecord()
                {
                    m_Position = transform.m_Position,
                    m_Rotation = transform.m_Rotation,
                };
                uiSystem.EntityManager.AddComponent<TransformRecord>(instanceEntity);
                uiSystem.EntityManager.SetComponentData(instanceEntity, transformRecord);
                return true;
            }

            return false;
        }
    }
}
