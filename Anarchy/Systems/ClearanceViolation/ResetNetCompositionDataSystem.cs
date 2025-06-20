﻿// <copyright file="ResetNetCompositionDataSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Anarchy.Systems.ClearanceViolation
{
    using Anarchy;
    using Anarchy.Components;
    using Anarchy.Systems;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// A system resets net composition data height ranges after <see cref="ModifyNetCompositionDataSystem"/> zeroed them out.
    /// </summary>
    public partial class ResetNetCompositionDataSystem : GameSystemBase
    {
        private ToolSystem m_ToolSystem;
        private EntityQuery m_NetCompositionDataQuery;
        private ILog m_Log;
        private NetToolSystem m_NetToolSystem;
        private PrefabSystem m_PrefabSystem;
        private ModificationEndBarrier m_Barrier;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetNetCompositionDataSystem"/> class.
        /// </summary>
        public ResetNetCompositionDataSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_Barrier = World.GetOrCreateSystemManaged<ModificationEndBarrier>();
            m_Log.Info($"{nameof(ResetNetCompositionDataSystem)} Created.");
            m_NetCompositionDataQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<HeightRangeRecord>(),
                        ComponentType.ReadOnly<NetCompositionData>(),
                    },
                },
            });
            RequireForUpdate(m_NetCompositionDataQuery);
            Enabled = false;
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer();
            NativeArray<Entity> entities = m_NetCompositionDataQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity currentEntity in entities)
            {
                if (EntityManager.TryGetComponent(currentEntity, out NetCompositionData netCompositionData))
                {
                    if (EntityManager.TryGetComponent(currentEntity, out HeightRangeRecord heightRangeRecord))
                    {
                        netCompositionData.m_HeightRange.min = heightRangeRecord.min;
                        netCompositionData.m_HeightRange.max = heightRangeRecord.max;

                        // m_Log.Debug($"{nameof(ResetNetCompositionDataSystem)}.{nameof(OnUpdate)} Reset m_HeightRange to {netCompositionData.m_HeightRange.min}+{netCompositionData.m_HeightRange.max} for entity: {currentEntity.Index}.{currentEntity.Version}.");
                        buffer.SetComponent(currentEntity, netCompositionData);
                    }
                    else
                    {
                        m_Log.Warn($"{nameof(ResetNetCompositionDataSystem)}.{nameof(OnUpdate)} could not retrieve height range record for Entity {currentEntity.Index}.{currentEntity.Version}.");
                    }
                }
                else
                {
                    m_Log.Warn($"{nameof(ResetNetCompositionDataSystem)}.{nameof(OnUpdate)} could not retrieve net composition or net geometry data for Entity {currentEntity.Index}.{currentEntity.Version}.");
                }
            }

            entities.Dispose();
            Enabled = false;
        }
    }
}
