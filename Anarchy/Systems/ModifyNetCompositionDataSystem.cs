// <copyright file="ModifyNetCompositionDataSystem.cs" company="Yenyang's Mods. MIT License">
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
    using Colossal.Mathematics;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// A system zeros out net composition data height ranges if anarchy is enabled and using net tool.
    /// </summary>
    public partial class ModifyNetCompositionDataSystem : GameSystemBase
    {
        private ToolSystem m_ToolSystem;
        private EntityQuery m_NetCompositionDataQuery;
        private AnarchySystem m_AnarchySystem;
        private ILog m_Log;
        private NetToolSystem m_NetToolSystem;
        private ResetNetCompositionDataSystem m_ResetNetCompositionDataSystem;
        private PrefabSystem m_PrefabSystem;
        private bool m_FirstTime = true;
        private bool m_EnsureReset = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyNetCompositionDataSystem"/> class.
        /// </summary>
        public ModifyNetCompositionDataSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Logger;
            m_AnarchySystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<AnarchySystem>();
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_NetToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<NetToolSystem>();
            m_PrefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            m_ResetNetCompositionDataSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ResetNetCompositionDataSystem>();
            m_Log.Info($"{nameof(ModifyNetCompositionDataSystem)} Created.");
            m_NetCompositionDataQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadWrite<NetCompositionData>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Deleted>(),
                    },
                },
            });
            RequireForUpdate(m_NetCompositionDataQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.activeTool != m_NetToolSystem || !m_AnarchySystem.AnarchyEnabled)
            {
                if (m_EnsureReset)
                {
                    m_ResetNetCompositionDataSystem.Enabled = true;
                    m_EnsureReset = false;
                }

                return;
            }

            NativeArray<Entity> entities = m_NetCompositionDataQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity currentEntity in entities)
            {
                if (EntityManager.TryGetComponent(currentEntity, out NetCompositionData netCompositionData))
                {
                    if (!EntityManager.HasComponent<HeightRangeRecord>(currentEntity))
                    {
                        if (netCompositionData.m_HeightRange.min == 0 && netCompositionData.m_HeightRange.max == 0)
                        {
                            m_Log.Debug($"{nameof(ModifyNetCompositionDataSystem)}.{nameof(OnUpdate)} Recalculating m_HeightRange {netCompositionData.m_HeightRange.min}+{netCompositionData.m_HeightRange.max} for entity: {currentEntity.Index}.{currentEntity.Version}.");
                            netCompositionData.m_HeightRange = RecalculateHeightRange(currentEntity);
                            m_Log.Debug($"{nameof(ModifyNetCompositionDataSystem)}.{nameof(OnUpdate)} Recalculated m_HeightRange {netCompositionData.m_HeightRange.min}+{netCompositionData.m_HeightRange.max} for entity: {currentEntity.Index}.{currentEntity.Version}.");
                        }

                        HeightRangeRecord heightRangeRecord = new ()
                        {
                            min = netCompositionData.m_HeightRange.min,
                            max = netCompositionData.m_HeightRange.max,
                        };
                        EntityManager.AddComponent<HeightRangeRecord>(currentEntity);
                        EntityManager.SetComponentData(currentEntity, heightRangeRecord);

                        m_Log.Debug($"{nameof(ModifyNetCompositionDataSystem)}.{nameof(OnUpdate)} Recorded m_HeightRange {netCompositionData.m_HeightRange.min}+{netCompositionData.m_HeightRange.max} for entity: {currentEntity.Index}.{currentEntity.Version}.");
                    }

                    if (EntityManager.TryGetComponent(currentEntity, out PrefabRef prefabRef) && EntityManager.HasComponent<PowerLineData>(prefabRef.m_Prefab))
                    {
                        netCompositionData.m_HeightRange.min = (netCompositionData.m_HeightRange.min + netCompositionData.m_HeightRange.max) / 2f;
                        netCompositionData.m_HeightRange.max = netCompositionData.m_HeightRange.min;
                    }
                    else
                    {
                        netCompositionData.m_HeightRange.min = Mathf.Clamp(-1f * AnarchyMod.Settings.MinimumClearanceBelowElevatedNetworks, netCompositionData.m_HeightRange.min, netCompositionData.m_HeightRange.max);
                        netCompositionData.m_HeightRange.max = Mathf.Clamp(0, netCompositionData.m_HeightRange.min, netCompositionData.m_HeightRange.max);
                    }

                    if (m_FirstTime)
                    {
                        m_Log.Debug($"{nameof(ModifyNetCompositionDataSystem)}.{nameof(OnUpdate)} Setting m_HeightRange to {netCompositionData.m_HeightRange.min}:{netCompositionData.m_HeightRange.max} for entity: {currentEntity.Index}.{currentEntity.Version}.");
                    }

                    EntityManager.SetComponentData(currentEntity, netCompositionData);
                }
                else
                {
                    m_Log.Warn($"{nameof(ModifyNetCompositionDataSystem)}.{nameof(OnUpdate)} could not retrieve net composition data for Entity {currentEntity.Index}.{currentEntity.Version}.");
                }
            }

            m_EnsureReset = true;
            m_FirstTime = false;
            entities.Dispose();
        }

        private Bounds1 RecalculateHeightRange(Entity e)
        {
            Bounds1 heightRange = new (float.MaxValue, -float.MaxValue);
            if (EntityManager.TryGetBuffer(e, true, out DynamicBuffer<NetCompositionPiece> netCompositionPieceBuffer))
            {
                foreach (NetCompositionPiece netCompositionPiece in netCompositionPieceBuffer)
                {
                    if (EntityManager.TryGetComponent(netCompositionPiece.m_Piece, out NetPieceData netPieceData))
                    {
                        if (netPieceData.m_HeightRange.min + netCompositionPiece.m_Offset.y < heightRange.min)
                        {
                            heightRange.min = netPieceData.m_HeightRange.min + netCompositionPiece.m_Offset.y;
                        }

                        if (netPieceData.m_HeightRange.max + netCompositionPiece.m_Offset.y > heightRange.max)
                        {
                            heightRange.max = netPieceData.m_HeightRange.max + netCompositionPiece.m_Offset.y;
                        }
                    }
                    else
                    {
                        m_Log.Warn($"{nameof(ModifyNetCompositionDataSystem)}.{nameof(RecalculateHeightRange)} could not retrieve NetPieceData for Entity {netCompositionPiece.m_Piece.Index}.{netCompositionPiece.m_Piece.Version}");
                    }
                }
            }
            else
            {
                m_Log.Warn($"{nameof(ModifyNetCompositionDataSystem)}.{nameof(RecalculateHeightRange)} could not retrieve NetCompositionPiece buffer for Entity {e.Index}.{e.Version}");
            }

            return heightRange;
        }
    }
}
