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
    using Unity.Entities;

    /// <summary>
    /// A system that prevents objects from being overriden when placed on each other.
    /// </summary>
    public partial class HandleUpdateNextFrameSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_UpdateNextFrameQuery;
        private EntityQuery m_UpdateNextFrameAndNotTransformRecordQuery;

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
               },
            });
            RequireForUpdate(m_UpdateNextFrameQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            EntityManager.AddComponent<Updated>(m_UpdateNextFrameQuery);
            EntityManager.AddComponent<Updated>(m_UpdateNextFrameAndNotTransformRecordQuery);
            EntityManager.RemoveComponent<UpdateNextFrame>(m_UpdateNextFrameAndNotTransformRecordQuery);
        }
    }
}
