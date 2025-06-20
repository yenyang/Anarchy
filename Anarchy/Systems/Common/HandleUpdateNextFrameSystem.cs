// <copyright file="HandleUpdateNextFrameSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.Common
{
    using Anarchy;
    using Anarchy.Components;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Adds updated to applicable entities that need a delayed update.
    /// </summary>
    public partial class HandleUpdateNextFrameSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_UpdateNextFrameQuery;
        private EntityQuery m_UpdateNextFrameAndNotTransformRecordQuery;
        private ModificationBarrier1 m_Barrier;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleUpdateNextFrameSystem"/> class.
        /// </summary>
        public HandleUpdateNextFrameSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(HandleUpdateNextFrameSystem)} Created.");
            m_UpdateNextFrameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
               {
                    ComponentType.ReadOnly<UpdateNextFrame>(),
                    ComponentType.ReadOnly<TransformRecord>(),
               },
                None = new ComponentType[]
               {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Updated>(),
               },
            });
            m_UpdateNextFrameAndNotTransformRecordQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
               {
                    ComponentType.ReadOnly<UpdateNextFrame>(),
               },
                None = new ComponentType[]
               {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Updated>(),
                    ComponentType.ReadOnly<TransformRecord>(),
                    ComponentType.ReadOnly<ClearUpdateNextFrame>(),
               },
            });
            m_Barrier = World.GetOrCreateSystemManaged<ModificationBarrier1>();
            RequireAnyForUpdate(m_UpdateNextFrameQuery, m_UpdateNextFrameAndNotTransformRecordQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer();
            NativeArray<Entity> updateNextFrameEntities = m_UpdateNextFrameQuery.ToEntityArray(Allocator.Temp);
            buffer.AddComponent<Updated>(updateNextFrameEntities);
            NativeArray<Entity> updateNextFrameAndNotTransformRecordEntities = m_UpdateNextFrameAndNotTransformRecordQuery.ToEntityArray(Allocator.Temp);
            buffer.AddComponent<ClearUpdateNextFrame>(updateNextFrameAndNotTransformRecordEntities);
            buffer.AddComponent<Updated>(updateNextFrameAndNotTransformRecordEntities);
        }
    }
}
