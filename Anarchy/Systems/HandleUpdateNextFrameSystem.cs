// <copyright file="HandleUpdateNextFrameSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using Anarchy;
    using Anarchy.Components;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Rendering;
    using Unity.Entities;

    /// <summary>
    /// A system that prevents objects from being overriden when placed on each other.
    /// </summary>
    public partial class HandleUpdateNextFrameSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_UpdateNextFrameQuery;

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
                    ComponentType.ReadOnly<TransformAndCullingBoundsRecord>(),
                    ComponentType.ReadWrite<Game.Objects.Transform>(),
                    ComponentType.ReadWrite<CullingInfo>(),
               },
                None = new ComponentType[]
               {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Updated>(),
               },
            });
            RequireForUpdate(m_UpdateNextFrameQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            EntityManager.AddComponent<Updated>(m_UpdateNextFrameQuery);
        }
    }
}
