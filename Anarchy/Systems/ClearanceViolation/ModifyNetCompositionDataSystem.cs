// <copyright file="ModifyNetCompositionDataSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Anarchy.Systems.ClearanceViolation
{
    using Anarchy;
    using Anarchy.Components;
    using Anarchy.Systems.Common;
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
        private AnarchyUISystem m_AnarchyUISystem;
        private ILog m_Log;
        private NetToolSystem m_NetToolSystem;
        private ResetNetCompositionDataSystem m_ResetNetCompositionDataSystem;
        private PrefabSystem m_PrefabSystem;
        private bool m_FirstTime = true;
        private bool m_EnsureReset = false;
        private ModificationBarrier3 m_Barrier;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyNetCompositionDataSystem"/> class.
        /// </summary>
        public ModifyNetCompositionDataSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_AnarchyUISystem = World.GetOrCreateSystemManaged<AnarchyUISystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_Barrier = World.GetOrCreateSystemManaged<ModificationBarrier3>();
            m_ResetNetCompositionDataSystem = World.GetOrCreateSystemManaged<ResetNetCompositionDataSystem>();
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
            if (m_ToolSystem.activeTool != m_NetToolSystem || !m_AnarchyUISystem.AnarchyEnabled)
            {
                if (m_EnsureReset)
                {
                    m_ResetNetCompositionDataSystem.Enabled = true;
                    m_EnsureReset = false;
                }

                return;
            }

            EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer();

            NativeArray<Entity> entities = m_NetCompositionDataQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity currentEntity in entities)
            {
                if (EntityManager.TryGetComponent(currentEntity, out NetCompositionData netCompositionData) &&
                    EntityManager.TryGetBuffer(currentEntity, true, out DynamicBuffer<NetCompositionPiece> netCompositionPieceBuffer) &&
                    netCompositionPieceBuffer.Length > 0)
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
                        buffer.AddComponent<HeightRangeRecord>(currentEntity);
                        buffer.SetComponent(currentEntity, heightRangeRecord);

                        m_Log.Debug($"{nameof(ModifyNetCompositionDataSystem)}.{nameof(OnUpdate)} Recorded m_HeightRange {netCompositionData.m_HeightRange.min}+{netCompositionData.m_HeightRange.max} for entity: {currentEntity.Index}.{currentEntity.Version}.");
                    }

                    if (EntityManager.TryGetComponent(currentEntity, out PrefabRef prefabRef) && EntityManager.HasComponent<PowerLineData>(prefabRef.m_Prefab))
                    {
                        netCompositionData.m_HeightRange.min = (netCompositionData.m_HeightRange.min + netCompositionData.m_HeightRange.max) / 2f;
                        netCompositionData.m_HeightRange.max = netCompositionData.m_HeightRange.min;
                    }
                    else
                    {
                        netCompositionData.m_HeightRange.min = Mathf.Clamp(-1f * AnarchyMod.Instance.Settings.MinimumClearanceBelowElevatedNetworks, netCompositionData.m_HeightRange.min, netCompositionData.m_HeightRange.max);
                        netCompositionData.m_HeightRange.max = Mathf.Clamp(0, netCompositionData.m_HeightRange.min, netCompositionData.m_HeightRange.max);
                    }

                    if (m_FirstTime)
                    {
                        m_Log.Debug($"{nameof(ModifyNetCompositionDataSystem)}.{nameof(OnUpdate)} Setting m_HeightRange to {netCompositionData.m_HeightRange.min}:{netCompositionData.m_HeightRange.max} for entity: {currentEntity.Index}.{currentEntity.Version}.");
                    }

                    buffer.SetComponent(currentEntity, netCompositionData);
                }
                else
                {
                    m_Log.Debug($"{nameof(ModifyNetCompositionDataSystem)}.{nameof(OnUpdate)} could not retrieve net composition data for Entity {currentEntity.Index}.{currentEntity.Version}.");
                }
            }

            m_EnsureReset = true;
            m_FirstTime = false;
            entities.Dispose();
        }

        private Bounds1 RecalculateHeightRange(Entity e)
        {
            Bounds1 heightRange = new (float.MaxValue, -float.MaxValue);
            if (EntityManager.TryGetBuffer(e, true, out DynamicBuffer<NetCompositionPiece> netCompositionPieceBuffer) &&
                netCompositionPieceBuffer.Length > 0)
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
                m_Log.Debug($"{nameof(ModifyNetCompositionDataSystem)}.{nameof(RecalculateHeightRange)} could not retrieve NetCompositionPiece buffer for Entity {e.Index}.{e.Version}");
            }

            return heightRange;
        }
    }
}
