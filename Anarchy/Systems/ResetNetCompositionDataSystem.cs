// <copyright file="ResetNetCompositionDataSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Anarchy.Systems
{
    using Anarchy;
    using Anarchy.Components;
    using Anarchy.Tooltip;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
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
        private AnarchySystem m_AnarchySystem;
        private ILog m_Log;
        private NetToolSystem m_NetToolSystem;
        private PrefabSystem m_PrefabSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetNetCompositionDataSystem"/> class.
        /// </summary>
        public ResetNetCompositionDataSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Logger;
            m_NetToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<NetToolSystem>();
            m_PrefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            m_Log.Info($"{nameof(ResetNetCompositionDataSystem)} Created.");
            m_NetCompositionDataQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadWrite<HeightRangeRecord>(),
                    },
                    Any = new ComponentType[]
                    {
                        ComponentType.ReadWrite<NetCompositionData>(),
                        ComponentType.ReadWrite<NetGeometryData>(),
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
                        EntityManager.SetComponentData(currentEntity, netCompositionData);
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
