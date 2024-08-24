// <copyright file="SetRetainingWallSegmentElevationSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Anarchy.Systems.NetworkAnarchy
{
    using Anarchy.Components;
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
        private EntityQuery m_UpgradedAndAppliedQuery;
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
            m_UpgradedAndAppliedQuery = SystemAPI.QueryBuilder()
                            .WithAll<Applied, Game.Net.Upgraded, Game.Net.Edge>()
                            .WithNone<Deleted, Overridden, Temp>()
                            .Build();

            RequireAnyForUpdate(m_UpgradedAndAppliedQuery);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            SetSegmentElevationsJob setSegmentElevationsJob = new SetSegmentElevationsJob()
            {
                m_ElevationLookup = SystemAPI.GetComponentLookup<Game.Net.Elevation>(isReadOnly: true),
                m_EntityType = SystemAPI.GetEntityTypeHandle(),
                m_UpgradedType = SystemAPI.GetComponentTypeHandle<Game.Net.Upgraded>(isReadOnly: true),
                m_ConnectedEdgeLookup = SystemAPI.GetBufferLookup<Game.Net.ConnectedEdge>(isReadOnly: true),
                buffer = m_Barrier.CreateCommandBuffer(),
                m_ReplaceMode = m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace,
                m_EdgeType = SystemAPI.GetComponentTypeHandle<Game.Net.Edge>(isReadOnly: true),
            };

            JobHandle segmentElevationsJobHandle = setSegmentElevationsJob.Schedule(m_UpgradedAndAppliedQuery, Dependency);
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
            [ReadOnly]
            public ComponentTypeHandle<Game.Net.Edge> m_EdgeType;
            public EntityCommandBuffer buffer;
            [ReadOnly]
            public ComponentLookup<Game.Net.Elevation> m_ElevationLookup;
            [ReadOnly]
            public BufferLookup<Game.Net.ConnectedEdge> m_ConnectedEdgeLookup;
            public bool m_ReplaceMode;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<Game.Net.Upgraded> upgradedNativeArray = chunk.GetNativeArray(ref m_UpgradedType);
                NativeArray<Game.Net.Edge> edgeNativeArray = chunk.GetNativeArray(ref m_EdgeType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity entity = entityNativeArray[i];
                    Game.Net.Upgraded upgraded = upgradedNativeArray[i];
                    Game.Net.Edge edge = edgeNativeArray[i];
                    Elevation elevation = default;
                    if (!m_ElevationLookup.HasComponent(entity)
                        && ((upgraded.m_Flags.m_Left & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised
                        || (upgraded.m_Flags.m_Left & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered
                        || (upgraded.m_Flags.m_Right & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised
                        || (upgraded.m_Flags.m_Right & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered
                        || (upgraded.m_Flags.m_General & CompositionFlags.General.Elevated) == CompositionFlags.General.Elevated
                        || (upgraded.m_Flags.m_General & CompositionFlags.General.Tunnel) == CompositionFlags.General.Tunnel))
                    {
                        buffer.AddComponent<Game.Net.Elevation>(entity);
                    }
                    else if ((upgraded.m_Flags.m_Left & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised
                        || (upgraded.m_Flags.m_Left & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered
                        || (upgraded.m_Flags.m_Right & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised
                        || (upgraded.m_Flags.m_Right & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered
                        || (upgraded.m_Flags.m_General & CompositionFlags.General.Elevated) == CompositionFlags.General.Elevated
                        || (upgraded.m_Flags.m_General & CompositionFlags.General.Tunnel) == CompositionFlags.General.Tunnel)
                    {
                        m_ElevationLookup.TryGetComponent(entity, out elevation);
                    }
                    else
                    {
                        continue;
                    }

                    if ((upgraded.m_Flags.m_Right & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered)
                    {
                        elevation.m_Elevation.y = Mathf.Min(elevation.m_Elevation.y, NetworkDefinitionSystem.RetainingWallThreshold);
                    }
                    else if ((upgraded.m_Flags.m_Right & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised)
                    {
                        elevation.m_Elevation.y = Mathf.Max(elevation.m_Elevation.y, NetworkDefinitionSystem.QuayThreshold);
                    }
                    else if (elevation.m_Elevation.y == NetworkDefinitionSystem.QuayThreshold || elevation.m_Elevation.y == NetworkDefinitionSystem.RetainingWallThreshold)
                    {
                        elevation.m_Elevation.y = 0;
                    }

                    if ((upgraded.m_Flags.m_Left & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered)
                    {
                        elevation.m_Elevation.x = Mathf.Min(elevation.m_Elevation.x, NetworkDefinitionSystem.RetainingWallThreshold);
                    }
                    else if ((upgraded.m_Flags.m_Left & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised)
                    {
                        elevation.m_Elevation.x = Mathf.Max(elevation.m_Elevation.x, NetworkDefinitionSystem.QuayThreshold);
                    }
                    else if (elevation.m_Elevation.x == NetworkDefinitionSystem.QuayThreshold || elevation.m_Elevation.x == NetworkDefinitionSystem.RetainingWallThreshold)
                    {
                        elevation.m_Elevation.x = 0;
                    }

                    if ((upgraded.m_Flags.m_General & CompositionFlags.General.Elevated) == CompositionFlags.General.Elevated)
                    {
                        elevation.m_Elevation.x = Mathf.Max(elevation.m_Elevation.x, NetworkDefinitionSystem.ElevatedThreshold);
                        elevation.m_Elevation.y = Mathf.Max(elevation.m_Elevation.y, NetworkDefinitionSystem.ElevatedThreshold);
                    }
                    else if ((upgraded.m_Flags.m_General & CompositionFlags.General.Tunnel) == CompositionFlags.General.Tunnel)
                    {
                        elevation.m_Elevation.x = Mathf.Min(elevation.m_Elevation.x, NetworkDefinitionSystem.TunnelThreshold);
                        elevation.m_Elevation.y = Mathf.Min(elevation.m_Elevation.y, NetworkDefinitionSystem.TunnelThreshold);

                        if (m_ReplaceMode)
                        {
                            if (m_ElevationLookup.TryGetComponent(edge.m_End, out Elevation edgeEndElevation))
                            {
                                edgeEndElevation.m_Elevation.x = Mathf.Min(edgeEndElevation.m_Elevation.x, NetworkDefinitionSystem.TunnelThreshold);
                                edgeEndElevation.m_Elevation.y = Mathf.Min(edgeEndElevation.m_Elevation.y, NetworkDefinitionSystem.TunnelThreshold);
                            }
                            else
                            {
                                edgeEndElevation.m_Elevation.x = NetworkDefinitionSystem.TunnelThreshold;
                                edgeEndElevation.m_Elevation.y = NetworkDefinitionSystem.TunnelThreshold;
                                buffer.AddComponent<Elevation>(edge.m_End);
                            }

                            buffer.SetComponent(edge.m_End, edgeEndElevation);

                            if (m_ElevationLookup.TryGetComponent(edge.m_Start, out Elevation edgeStartElevation))
                            {
                                edgeStartElevation.m_Elevation.x = Mathf.Min(edgeStartElevation.m_Elevation.x, NetworkDefinitionSystem.TunnelThreshold);
                                edgeStartElevation.m_Elevation.y = Mathf.Min(edgeStartElevation.m_Elevation.y, NetworkDefinitionSystem.TunnelThreshold);
                            }
                            else
                            {
                                edgeStartElevation.m_Elevation.x = NetworkDefinitionSystem.TunnelThreshold;
                                edgeStartElevation.m_Elevation.y = NetworkDefinitionSystem.TunnelThreshold;
                                buffer.AddComponent<Elevation>(edge.m_Start);
                            }

                            buffer.SetComponent(edge.m_Start, edgeStartElevation);
                        }
                    }

                    if (upgraded.m_Flags.m_Left == 0 && upgraded.m_Flags.m_Right == 0 && upgraded.m_Flags.m_General == 0)
                    {
                        buffer.RemoveComponent<Upgraded>(entity);
                    }
                    else
                    {
                        buffer.SetComponent(entity, upgraded);
                    }

                    if (elevation.m_Elevation.x != 0 || elevation.m_Elevation.y != 0)
                    {
                        buffer.SetComponent(entity, elevation);
                    }
                    else
                    {
                        buffer.RemoveComponent<Elevation>(entity);
                    }

                    if (m_ReplaceMode)
                    {
                        if (m_ConnectedEdgeLookup.TryGetBuffer(edge.m_End, out DynamicBuffer<ConnectedEdge> endConnectedEdges))
                        {
                            buffer.AddComponent<UpdateNextFrame>(edge.m_End);
                            foreach (ConnectedEdge connectedEdge in endConnectedEdges)
                            {
                                buffer.AddComponent<UpdateNextFrame>(connectedEdge.m_Edge);
                            }
                        }

                        if (m_ConnectedEdgeLookup.TryGetBuffer(edge.m_Start, out DynamicBuffer<ConnectedEdge> startConnectedEdges))
                        {
                            buffer.AddComponent<UpdateNextFrame>(edge.m_Start);
                            foreach (ConnectedEdge connectedEdge in startConnectedEdges)
                            {
                                buffer.AddComponent<UpdateNextFrame>(connectedEdge.m_Edge);
                            }
                        }
                    }
                }
            }
        }
    }
}