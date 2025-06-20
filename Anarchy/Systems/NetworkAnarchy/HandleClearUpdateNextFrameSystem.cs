// <copyright file="HandleClearUpdateNextFrameSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.NetworkAnarchy
{
    using Anarchy;
    using Anarchy.Components;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Removes UpdateNextFrame and ClearUpdateNextFrame components from applicable entities.
    /// </summary>
    public partial class HandleClearUpdateNextFrameSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_ClearUpdateNextFrameQuery;
        private ModificationEndBarrier m_Barrier;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleClearUpdateNextFrameSystem"/> class.
        /// </summary>
        public HandleClearUpdateNextFrameSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(HandleClearUpdateNextFrameSystem)} Created.");
            m_Barrier = World.GetOrCreateSystemManaged<ModificationEndBarrier>();
            m_ClearUpdateNextFrameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
               {
                    ComponentType.ReadOnly<UpdateNextFrame>(),
                    ComponentType.ReadOnly<ClearUpdateNextFrame>(),
               },
                None = new ComponentType[]
               {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Updated>(),
               },
            });
            RequireForUpdate(m_ClearUpdateNextFrameQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer();
            NativeArray<Entity> clearUpdateNextFrameEntities = m_ClearUpdateNextFrameQuery.ToEntityArray(Allocator.Temp);
            buffer.RemoveComponent<ClearUpdateNextFrame>(clearUpdateNextFrameEntities);
            buffer.RemoveComponent<UpdateNextFrame>(clearUpdateNextFrameEntities);
        }
    }
}