// <copyright file="PreventCullingSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Anarchy.Systems.OverridePrevention
{
    using Anarchy;
    using Anarchy.Components;
    using Colossal.Logging;
    using Game;
    using Game.Buildings;
    using Game.Citizens;
    using Game.Common;
    using Game.Creatures;
    using Game.Objects;
    using Game.Rendering;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;

    /// <summary>
    /// A system that prevents objects from being overriden that has a custom component.
    /// </summary>
    public partial class PreventCullingSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_CullingInfoQuery;
        private ToolOutputBarrier m_ToolOutputBarrier;
        private int m_FrameCount = 0;
        private float3 m_PrevCameraPosition;
        private CameraUpdateSystem m_CameraUpdateSystem;
        private float3 m_PrevCameraDirection;
        private float4 m_PrevLodParameters;
        private ToolSystem m_ToolSystem;
        private RenderingSystem m_RenderingSystem;
        private BatchDataSystem m_BatchDataSystem;
        private bool m_Loaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreventCullingSystem"/> class.
        /// </summary>
        public PreventCullingSystem()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether to trigger the system running now.
        /// </summary>
        public bool RunNow { get; set; }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(PreventCullingSystem)} Created.");
            m_ToolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_PrevCameraDirection = math.forward();
            m_PrevLodParameters = 1f;
            m_CameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();
            m_RenderingSystem = World.GetOrCreateSystemManaged<RenderingSystem>();
            m_BatchDataSystem = World.GetOrCreateSystemManaged<BatchDataSystem>();
            m_CullingInfoQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
               {
                    ComponentType.ReadOnly<PreventOverride>(),
                    ComponentType.ReadOnly<CullingInfo>(),
               },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<Temp>(),
                    ComponentType.ReadOnly<Building>(),
                    ComponentType.ReadOnly<Animal>(),
                    ComponentType.ReadOnly<Game.Creatures.Pet>(),
                    ComponentType.ReadOnly<Creature>(),
                    ComponentType.ReadOnly<Moving>(),
                    ComponentType.ReadOnly<Household>(),
                    ComponentType.ReadOnly<Vehicle>(),
                    ComponentType.ReadOnly<Event>(),
                    ComponentType.ReadOnly<Deleted>(),
                },
            });
            RequireForUpdate(m_CullingInfoQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            m_Loaded = true;
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.actionMode.IsEditor() && !AnarchyMod.Instance.Settings.PreventOverrideInEditor)
            {
                return;
            }

            if (m_FrameCount < AnarchyMod.Instance.Settings.PropRefreshFrequency && !RunNow)
            {
                m_FrameCount++;
                return;
            }

            m_FrameCount = 0;

            if (!AnarchyMod.Instance.Settings.PreventAccidentalPropCulling && !RunNow)
            {
                return;
            }

            if (!m_ToolSystem.actionMode.IsGameOrEditor() || !m_Loaded)
            {
                return;
            }

            float3 cameraPosition = m_PrevCameraPosition;
            float3 cameraDirection = m_PrevCameraDirection;
            float4 prevLodParameters = m_PrevLodParameters;

            if (m_CameraUpdateSystem.TryGetLODParameters(out var lodParameters))
            {
                cameraPosition = lodParameters.cameraPosition;
                IGameCameraController activeCameraController = m_CameraUpdateSystem.activeCameraController;
                prevLodParameters = RenderingUtils.CalculateLodParameters(m_BatchDataSystem.GetLevelOfDetail(m_RenderingSystem.frameLod, activeCameraController), lodParameters);
                cameraDirection = m_CameraUpdateSystem.activeViewer.forward;
            }

            RunNow = false;
            m_Log.Debug($"{nameof(PreventCullingSystem)}.{nameof(OnUpdate)}");

            VerifyVisibleJob verifyVisibleJob = new ()
            {
                m_CullingInfoType = SystemAPI.GetComponentTypeHandle<CullingInfo>(),
                m_EntityType = SystemAPI.GetEntityTypeHandle(),
                buffer = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_CameraDirection = cameraDirection,
                m_CameraPosition = cameraPosition,
                m_LodParameters = prevLodParameters,
            };
            JobHandle jobHandle = verifyVisibleJob.ScheduleParallel(m_CullingInfoQuery, Dependency);
            m_ToolOutputBarrier.AddJobHandleForProducer(jobHandle);
            Dependency = jobHandle;
        }

#if BURST
        [BurstCompile]
#endif
        private struct VerifyVisibleJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentTypeHandle<CullingInfo> m_CullingInfoType;
            [ReadOnly]
            public float4 m_LodParameters;
            [ReadOnly]
            public float3 m_CameraPosition;
            [ReadOnly]
            public float3 m_CameraDirection;
            public EntityCommandBuffer.ParallelWriter buffer;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<CullingInfo> cullingInfoNativeArray = chunk.GetNativeArray(ref m_CullingInfoType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    CullingInfo currentCullingInfo = cullingInfoNativeArray[i];
                    if (currentCullingInfo.m_PassedCulling == 0)
                    {
                        float num = RenderingUtils.CalculateMinDistance(currentCullingInfo.m_Bounds, m_CameraPosition, m_CameraDirection, m_LodParameters);
                        if (RenderingUtils.CalculateLod(num * num, m_LodParameters) > currentCullingInfo.m_MinLod)
                        {
                            buffer.AddComponent<Updated>(unfilteredChunkIndex, currentEntity);
                        }
                    }
                }
            }
        }

    }
}
