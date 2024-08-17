// <copyright file="SetRetainingWallSegmentElevationSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Anarchy.Systems.NetworkAnarchy
{
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Net;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using UnityEngine;

    /// <summary>
    /// Adds elevation information to segments that have been upgraded with retaining walls.
    /// </summary>
    public partial class SetRetainingWallSegmentElevationSystem : GameSystemBase
    {
        private EntityQuery m_UpgradedAndUpdatedQuery;
        private ILog m_Log;
        private ModificationEndBarrier m_Barrier;
        private ToolSystem m_ToolSystem;
        private NetToolSystem m_NetToolSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetRetainingWallSegmentElevationSystem"/> class.
        /// </summary>
        public SetRetainingWallSegmentElevationSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_Barrier = World.GetOrCreateSystemManaged<ModificationEndBarrier>();
            m_Log.Info($"[{nameof(SetRetainingWallSegmentElevationSystem)}] {nameof(OnCreate)}");
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_ToolSystem.EventToolChanged += (ToolBaseSystem tool) => Enabled = tool == m_NetToolSystem;
            m_UpgradedAndUpdatedQuery = SystemAPI.QueryBuilder()
                            .WithAll<Applied, Game.Net.Upgraded, Game.Net.Edge>()
                            .WithNone<Deleted, Overridden, Temp>()
                            .Build();
            RequireForUpdate(m_UpgradedAndUpdatedQuery);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            SetSegmentElevationsJob setSegmentElevationsJob = new SetSegmentElevationsJob()
            {
                m_EdgeLookup = SystemAPI.GetComponentLookup<Game.Net.Edge>(isReadOnly: true),
                m_ElevationLookup = SystemAPI.GetComponentLookup<Game.Net.Elevation>(isReadOnly: true),
                m_EntityType = SystemAPI.GetEntityTypeHandle(),
                m_NodeLookup = SystemAPI.GetComponentLookup<Game.Net.Node>(isReadOnly: true),
                m_UpgradedType = SystemAPI.GetComponentTypeHandle<Game.Net.Upgraded>(isReadOnly: true),
                buffer = m_Barrier.CreateCommandBuffer(),
            };

            JobHandle segmentElevationsJobHandle = setSegmentElevationsJob.Schedule(m_UpgradedAndUpdatedQuery, Dependency);
            m_Barrier.AddJobHandleForProducer(segmentElevationsJobHandle);
            Dependency = segmentElevationsJobHandle;
        }

#if BURST
        [BurstCompile]
#endif
        private struct SetSegmentElevationsJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentTypeHandle<Game.Net.Upgraded> m_UpgradedType;
            public EntityCommandBuffer buffer;
            [ReadOnly]
            public ComponentLookup<Game.Net.Elevation> m_ElevationLookup;
            [ReadOnly]
            public ComponentLookup<Game.Net.Node> m_NodeLookup;
            [ReadOnly]
            public ComponentLookup<Game.Net.Edge> m_EdgeLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<Game.Net.Upgraded> upgradedNativeArray = chunk.GetNativeArray(ref m_UpgradedType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity entity = entityNativeArray[i];
                    Game.Net.Upgraded upgraded = upgradedNativeArray[i];
                    Elevation elevation = default;
                    if (!m_ElevationLookup.HasComponent(entity)
                        && ((upgraded.m_Flags.m_Left & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised
                        || (upgraded.m_Flags.m_Left & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered
                        || (upgraded.m_Flags.m_Right & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised
                        || (upgraded.m_Flags.m_Right & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered))
                    {
                        buffer.AddComponent<Game.Net.Elevation>(entity);
                    }
                    else if ((upgraded.m_Flags.m_Left & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised
                        || (upgraded.m_Flags.m_Left & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered
                        || (upgraded.m_Flags.m_Right & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised
                        || (upgraded.m_Flags.m_Right & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered)
                    {
                        m_ElevationLookup.TryGetComponent(entity, out elevation);
                    }
                    else
                    {
                        continue;
                    }

                    if (((upgraded.m_Flags.m_Left & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered && m_NodeLookup.HasComponent(entity))
                        || ((upgraded.m_Flags.m_Right & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered && m_EdgeLookup.HasComponent(entity)))
                    {
                        elevation.m_Elevation.y = Mathf.Min(elevation.m_Elevation.y, NetworkDefinitionSystem.RetainingWallThreshold);
                    }
                    else if (((upgraded.m_Flags.m_Left & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised && m_NodeLookup.HasComponent(entity))
                        || ((upgraded.m_Flags.m_Right & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised && m_EdgeLookup.HasComponent(entity)))
                    {
                        elevation.m_Elevation.y = Mathf.Max(elevation.m_Elevation.y, NetworkDefinitionSystem.QuayThreshold);
                    }

                    if (((upgraded.m_Flags.m_Right & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered && m_NodeLookup.HasComponent(entity))
                        || ((upgraded.m_Flags.m_Left & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered && m_EdgeLookup.HasComponent(entity)))
                    {
                        elevation.m_Elevation.x = Mathf.Min(elevation.m_Elevation.x, NetworkDefinitionSystem.RetainingWallThreshold);
                    }
                    else if (((upgraded.m_Flags.m_Right & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised && m_NodeLookup.HasComponent(entity))
                        || ((upgraded.m_Flags.m_Left & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised && m_EdgeLookup.HasComponent(entity)))
                    {
                        elevation.m_Elevation.x = Mathf.Max(elevation.m_Elevation.x, NetworkDefinitionSystem.QuayThreshold);
                    }

                    if (upgraded.m_Flags.m_Left == 0 && upgraded.m_Flags.m_Right == 0 && upgraded.m_Flags.m_General == 0)
                    {
                        buffer.RemoveComponent<Upgraded>(entity);
                    }
                    else
                    {
                        buffer.SetComponent(entity, upgraded);
                    }

                    buffer.SetComponent(entity, elevation);
                }
            }
        }
    }
}