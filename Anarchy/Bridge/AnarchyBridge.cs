// <copyright file="AnarchyBridge.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Bridge
{
    using Anarchy.Components;
    using Anarchy.Systems.Common;
    using Colossal.Entities;
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

            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.Temp);
            SelectedInfoPanelTogglesSystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            for (int i = 0; i <= entities.Length; i++)
            {
                if (uiSystem.CheckOverridable(entities[i]))
                {
                    buffer.AddComponent<PreventOverride>(entities[i]);
                }
            }

            buffer.Playback(uiSystem.EntityManager);
            buffer.Dispose();
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

            AddAnarchyComponent(entityQuery.ToEntityArray(Allocator.Temp));
        }

        /// <summary>
        /// Tries to add Transform Lock component to a entities in a query.
        /// </summary>
        /// <param name="entityQuery">Entity query you want to try to add component to.</param>
        public static void AddTransformLockComponent(EntityQuery entityQuery)
        {
            if (entityQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            AddTransformLockComponent(entityQuery.ToEntityArray(Allocator.Temp));
        }

        /// <summary>
        /// Tries to update the transform lock component on an Instance Entity.
        /// </summary>
        /// <param name="instanceEntity">Instance Entity to update component value.</param>
        /// <param name="transform">New Position and Rotation.</param>
        /// <returns>True is value updated. False if not.</returns>
        public static bool TryUpdateTransformLockComponent(Entity instanceEntity, Game.Objects.Transform transform)
        {
            SelectedInfoPanelTogglesSystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            if (uiSystem.CheckDisturbable(instanceEntity) &&
                uiSystem.EntityManager.HasComponent<TransformRecord>(instanceEntity))
            {
                uiSystem.EntityManager.SetComponentData(instanceEntity, new TransformRecord() { m_Position = transform.m_Position, m_Rotation = transform.m_Rotation, });
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to add Transform Lock component to entities in a native array.
        /// </summary>c
        /// <param name="entities">Native array of entities to try and add component to.</param>
        public static void AddTransformLockComponent(NativeArray<Entity> entities)
        {
            SelectedInfoPanelTogglesSystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                if (uiSystem.CheckDisturbable(entities[i]) &&
                   !uiSystem.EntityManager.HasComponent<TransformRecord>(entities[i]) &&
                    uiSystem.EntityManager.TryGetComponent(entities[i], out Game.Objects.Transform transform))
                {
                    TransformRecord transformRecord = new TransformRecord()
                    {
                        m_Position = transform.m_Position,
                        m_Rotation = transform.m_Rotation,
                    };
                    buffer.AddComponent<TransformRecord>(entities[i]);
                    buffer.SetComponent(entities[i], transformRecord);
                }
            }

            buffer.Playback(uiSystem.EntityManager);
            buffer.Dispose();
        }

        /// <summary>
        /// Removes Anarchy component from an entity if it has it.
        /// </summary>
        /// <param name="entity">Entity to check and remove component.</param>
        public static void RemoveAnarchyComponent(Entity entity)
        {
            SelectedInfoPanelTogglesSystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            if (entity != Entity.Null &&
                uiSystem.EntityManager.HasComponent<PreventOverride>(entity))
            {
                uiSystem.EntityManager.RemoveComponent<PreventOverride>(entity);
            }
        }

        /// <summary>
        /// Removes Transform Lock component from an entity if it has it.
        /// </summary>
        /// <param name="entity">Entity to check and remove component.</param>
        public static void RemoveTransformLockComponent(Entity entity)
        {
            SelectedInfoPanelTogglesSystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            if (entity != Entity.Null &&
                uiSystem.EntityManager.HasComponent<TransformRecord>(entity))
            {
                uiSystem.EntityManager.RemoveComponent<TransformRecord>(entity);
            }
        }

        /// <summary>
        /// Removes Anarchy component from an array of entities if they have it.
        /// </summary>
        /// <param name="entities">Entities to check and remove component.</param>
        public static void RemoveAnarchyComponent(NativeArray<Entity> entities)
        {
            SelectedInfoPanelTogglesSystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                if (entities[i] != Entity.Null &&
                    uiSystem.EntityManager.HasComponent<PreventOverride>(entities[i]))
                {
                    buffer.RemoveComponent<PreventOverride>(entities[i]);
                }
            }

            buffer.Playback(uiSystem.EntityManager);
            buffer.Dispose();
        }

        /// <summary>
        /// Removes Transform Lock component from an array of entities if they have it.
        /// </summary>
        /// <param name="entities">Entities to check and remove component.</param>
        public static void RemoveTransformLockComponent(NativeArray<Entity> entities)
        {
            SelectedInfoPanelTogglesSystem uiSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                if (entities[i] != Entity.Null &&
                    uiSystem.EntityManager.HasComponent<TransformRecord>(entities[i]))
                {
                    buffer.RemoveComponent<TransformRecord>(entities[i]);
                }
            }

            buffer.Playback(uiSystem.EntityManager);
            buffer.Dispose();
        }

        /// <summary>
        /// Removes Anarchy component from an query of entities if they have it.
        /// </summary>
        /// <param name="entityQuery">Query for entities to check and remove component.</param>
        public static void RemoveAnarchyComponent(EntityQuery entityQuery)
        {
            if (entityQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            RemoveAnarchyComponent(entityQuery.ToEntityArray(Allocator.Temp));
        }

        /// <summary>
        /// Removes Transform Lock component from an query of entities if they have it.
        /// </summary>
        /// <param name="entityQuery">Query for entities to check and remove component.</param>
        public static void RemoveTransformLockComponent(EntityQuery entityQuery)
        {
            if (entityQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            RemoveTransformLockComponent(entityQuery.ToEntityArray(Allocator.Temp));
        }

        /// <summary>
        /// Gets the component type for Anarchy Component.
        /// </summary>
        /// <returns>ComponentType for PreventOverride Component.</returns>
        public static ComponentType GetAnarchyComponentType()
        {
            return ComponentType.ReadWrite<PreventOverride>();
        }

        /// <summary>
        /// Gets the component type for Transform Lock.
        /// </summary>
        /// <returns>ComponentType for TransformRecord component.</returns>
        public static ComponentType GetTransformLockComponentType()
        {
            return ComponentType.ReadWrite<TransformRecord>();
        }
    }
}
