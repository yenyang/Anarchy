// <copyright file="PreventCullingSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
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
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// A system that prevents objects from being overriden that has a custom component.
    /// </summary>
    public partial class PreventCullingSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_CullingInfoQuery;
        private TypeHandle __TypeHandle;
        private ToolOutputBarrier m_ToolOutputBarrier;
        private int m_FrameCount = 0;
        private ToolSystem m_ToolSystem;

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
            m_Log = AnarchyMod.Instance.Logger;
            m_Log.Info($"{nameof(PreventCullingSystem)} Created.");
            m_ToolOutputBarrier = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
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
                    ComponentType.ReadOnly<Crane>(),
                    ComponentType.ReadOnly<Animal>(),
                    ComponentType.ReadOnly<Game.Creatures.Pet>(),
                    ComponentType.ReadOnly<Creature>(),
                    ComponentType.ReadOnly<Moving>(),
                    ComponentType.ReadOnly<Household>(),
                    ComponentType.ReadOnly<Vehicle>(),
                    ComponentType.ReadOnly<Event>(),
                },
            });
            RequireForUpdate(m_CullingInfoQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.actionMode.IsEditor())
            {
                return;
            }

            if (m_FrameCount < AnarchyMod.Settings.PropRefreshFrequency && !RunNow)
            {
                m_FrameCount++;
                return;
            }

            m_FrameCount = 0;

            if (!AnarchyMod.Settings.PreventAccidentalPropCulling && !RunNow)
            {
                return;
            }

            RunNow = false;

            __TypeHandle.__CullingInfo_RO_ComponentTypeHandle.Update(ref CheckedStateRef);
            __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref CheckedStateRef);

            PreventCullingJob preventCullingJob = new ()
            {
                m_CullingInfoType = __TypeHandle.__CullingInfo_RO_ComponentTypeHandle,
                m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle,
                buffer = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
            };
            JobHandle jobHandle = preventCullingJob.ScheduleParallel(m_CullingInfoQuery, Dependency);
            m_ToolOutputBarrier.AddJobHandleForProducer(jobHandle);
            Dependency = jobHandle;
        }

        /// <inheritdoc/>
        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            __TypeHandle.__AssignHandles(ref CheckedStateRef);
        }

        private struct TypeHandle
        {
            [ReadOnly]
            public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<CullingInfo> __CullingInfo_RO_ComponentTypeHandle;

            public void __AssignHandles(ref SystemState state)
            {
                __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                __CullingInfo_RO_ComponentTypeHandle = state.GetComponentTypeHandle<CullingInfo>();
            }
        }

        private struct PreventCullingJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentTypeHandle<CullingInfo> m_CullingInfoType;
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
                        buffer.AddComponent<Updated>(unfilteredChunkIndex, currentEntity);
                    }
                }
            }
        }
    }
}
