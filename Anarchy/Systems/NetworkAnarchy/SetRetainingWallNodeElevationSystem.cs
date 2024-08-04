// <copyright file="SetRetainingWallNodeElevationSystem.cs" company="Yenyang's Mods. MIT License">
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
    using Game.Tools;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using UnityEngine;

    /// <summary>
    /// Fixes issues with forced retaining wall nodes.
    /// </summary>
    public partial class SetRetainingWallNodeElevationSystem : GameSystemBase
    {
        private EntityQuery m_NodeQuery;
        private ILog m_Log;
        private ModificationBarrier1 m_Barrier;
        private ToolSystem m_ToolSystem;
        private NetToolSystem m_NetToolSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetRetainingWallNodeElevationSystem"/> class.
        /// </summary>
        public SetRetainingWallNodeElevationSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_Barrier = World.GetOrCreateSystemManaged<ModificationBarrier1>();
            m_Log.Info($"[{nameof(SetRetainingWallNodeElevationSystem)}] {nameof(OnCreate)}");

            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_ToolSystem.EventToolChanged += (ToolBaseSystem tool) => Enabled = tool == m_NetToolSystem;

            m_NodeQuery = SystemAPI.QueryBuilder()
                            .WithAll<Game.Net.ConnectedEdge, UpdateNextFrame>()
                            .WithNone<Deleted, Overridden, Temp>()
                            .Build();
            RequireForUpdate(m_NodeQuery);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            SetNodeElevationsJob setNodeElevationsJob = new SetNodeElevationsJob()
            {
                m_ElevationLookup = SystemAPI.GetComponentLookup<Game.Net.Elevation>(isReadOnly: true),
                m_EntityType = SystemAPI.GetEntityTypeHandle(),
                buffer = m_Barrier.CreateCommandBuffer(),
                m_ConnectedEdgeType = SystemAPI.GetBufferTypeHandle<Game.Net.ConnectedEdge>(isReadOnly: true),
            };

            JobHandle nodeElevationsJobHandle = setNodeElevationsJob.Schedule(m_NodeQuery, Dependency);
            m_Barrier.AddJobHandleForProducer(nodeElevationsJobHandle);
            Dependency = nodeElevationsJobHandle;
        }

#if BURST
        [BurstCompile]
#endif
        private struct SetNodeElevationsJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            public EntityCommandBuffer buffer;
            [ReadOnly]
            public BufferTypeHandle<ConnectedEdge> m_ConnectedEdgeType;
            [ReadOnly]
            public ComponentLookup<Game.Net.Elevation> m_ElevationLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                BufferAccessor<Game.Net.ConnectedEdge> connectedEdgeBufferAccessor = chunk.GetBufferAccessor(ref m_ConnectedEdgeType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity entity = entityNativeArray[i];
                    DynamicBuffer<ConnectedEdge> connectedEdgeBuffer = connectedEdgeBufferAccessor[i];
                    ProcessElevation(ref entity, ref connectedEdgeBuffer);
                    ProcessElevation(ref entity, ref connectedEdgeBuffer);
                }
            }

            private void ProcessElevation(ref Entity nodeEntity, ref DynamicBuffer<ConnectedEdge> segments)
            {
                if (segments.Length <= 1)
                {
                    return;
                }

                Elevation elevation = new ();
                if (!m_ElevationLookup.HasComponent(nodeEntity))
                {
                    buffer.AddComponent<Game.Net.Elevation>(nodeEntity);
                }
                else
                {
                    m_ElevationLookup.TryGetComponent(nodeEntity, out elevation);
                }

                foreach (ConnectedEdge segment in segments)
                {
                    if (m_ElevationLookup.TryGetComponent(segment.m_Edge, out Elevation segmentElevation))
                    {
                        if (Mathf.Abs(segmentElevation.m_Elevation.x) > Mathf.Abs(elevation.m_Elevation.y))
                        {
                            elevation.m_Elevation.y = segmentElevation.m_Elevation.x;
                        }

                        if (Mathf.Abs(segmentElevation.m_Elevation.y) > Mathf.Abs(elevation.m_Elevation.x))
                        {
                            elevation.m_Elevation.x = segmentElevation.m_Elevation.y;
                        }
                    }

                    buffer.AddComponent<Updated>(segment.m_Edge);
                }

                buffer.SetComponent(nodeEntity, elevation);
                buffer.AddComponent<Updated>(nodeEntity);
                buffer.RemoveComponent<UpdateNextFrame>(nodeEntity);
            }
        }
    }
}