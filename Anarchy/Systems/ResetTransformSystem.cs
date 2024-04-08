// <copyright file="ResetTransformSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using Anarchy;
    using Anarchy.Components;
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Rendering;
    using Game.Tools;
    using System.Collections.Generic;
    using System.Reflection;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// A system that prevents objects from being overriden that has a custom component.
    /// </summary>
    public partial class ResetTransformSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_TransformRecordQuery;
        private ToolSystem m_ToolSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetTransformSystem"/> class.
        /// </summary>
        public ResetTransformSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(ResetTransformSystem)} Created.");
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_TransformRecordQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
               {
                    ComponentType.ReadOnly<Updated>(),
                    ComponentType.ReadOnly<TransformRecord>(),
                    ComponentType.ReadWrite<Game.Objects.Transform>(),
               },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<Deleted>(),
                },
            });
            RequireForUpdate(m_TransformRecordQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.actionMode.IsEditor() && !AnarchyMod.Instance.Settings.PreventOverrideInEditor)
            {
                return;
            }

            NativeArray<Entity> entities = m_TransformRecordQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out TransformRecord transformRecord) || !EntityManager.TryGetComponent(entity, out Game.Objects.Transform originalTransform))
                {
                    continue;
                }

                if (transformRecord.Equals(originalTransform))
                {
                    EntityManager.RemoveComponent<UpdateNextFrame>(entity);
                    continue;
                }

                originalTransform.m_Position = transformRecord.m_Position;
                originalTransform.m_Rotation = transformRecord.m_Rotation;
                EntityManager.SetComponentData(entity, originalTransform);
                if (EntityManager.HasComponent<UpdateNextFrame>(entity))
                {
                    EntityManager.RemoveComponent<UpdateNextFrame>(entity);
                }
                else
                {
                    EntityManager.AddComponent<UpdateNextFrame>(entity);
                }
            }

        }
    }
}
