// <copyright file="CheckTransformSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Anarchy.Systems.ObjectElevation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Anarchy;
    using Anarchy.Components;
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Objects;
    using Game.Tools;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// A system that prevents objects from being overriden that has a custom component.
    /// </summary>
    public partial class CheckTransformSystem : GameSystemBase
    {
        private const string MoveItToolID = "MoveItTool";
        private ILog m_Log;
        private EntityQuery m_TransformRecordQuery;
        private ToolSystem m_ToolSystem;
        private ToolBaseSystem m_MoveItTool;
        private ModificationBarrier1 m_ModificationBarrier1;
        private JobHandle m_writeDeps;
        private JobHandle m_readDeps;
        private NativeHashSet<Entity> m_MoveItSelectedEntities = new (0, Allocator.Persistent);
        private PropertyInfo m_MoveItSelectedEntitiesPropertyInfo;
        private HashSet<Entity> m_MoveItSelectedEntitiesHashSet = new HashSet<Entity>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTransformSystem"/> class.
        /// </summary>
        public CheckTransformSystem()
        {
        }

        /// <summary>
        /// Get data, can be used inside or outside of system
        /// </summary>
        /// <param name="readOnly">true.</param>
        /// <param name="deps">dependency.</param>
        /// <returns>MoveItSelectedEntities.</returns>
        public NativeHashSet<Entity> GetEntities(bool readOnly, out JobHandle deps)
        {
            deps = readOnly ? m_writeDeps : JobHandle.CombineDependencies(m_readDeps, m_writeDeps);
            return m_MoveItSelectedEntities;
        }

        /// <summary>
        /// Register jobhandle as read dependency.
        /// </summary>
        /// <param name="jobHandle">jobhandle to add.</param>
        public void AddEntitiesReader(JobHandle jobHandle)
        {
            m_readDeps = JobHandle.CombineDependencies(m_readDeps, jobHandle);
        }

        /// <summary>
        /// Registers jobhandle as write dependency.
        /// </summary>
        /// <param name="jobHandle">jobhandle to add.</param>
        public void AddEntitiesWriter(JobHandle jobHandle)
        {
            m_writeDeps = JobHandle.CombineDependencies(m_writeDeps, jobHandle);
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(CheckTransformSystem)} Created.");
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ModificationBarrier1 = World.GetOrCreateSystemManaged<ModificationBarrier1>();
            m_TransformRecordQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
               {
                    ComponentType.ReadOnly<Updated>(),
                    ComponentType.ReadWrite<TransformRecord>(),
                    ComponentType.ReadOnly<Game.Objects.Transform>(),
               },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<Deleted>(),
                },
            });
            RequireForUpdate(m_TransformRecordQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (World.GetOrCreateSystemManaged<ToolSystem>().tools.Find(x => x.toolID.Equals(MoveItToolID)) is ToolBaseSystem moveItTool)
            {
                // Found it
                m_Log.Info($"{nameof(ResetTransformSystem)}.{nameof(OnGameLoadingComplete)} found Move It.");
                PropertyInfo moveItSelectedEntities = moveItTool.GetType().GetProperty("SelectedEntities");
                if (moveItSelectedEntities is not null)
                {
                    m_MoveItTool = moveItTool;
                    m_MoveItSelectedEntitiesPropertyInfo = moveItSelectedEntities;
                    m_Log.Info($"{nameof(ResetTransformSystem)}.{nameof(OnGameLoadingComplete)} saved moveItTool");
                }
            }
            else
            {
                m_Log.Info($"{nameof(ResetTransformSystem)}.{nameof(OnGameLoadingComplete)} move it tool not found");
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.actionMode.IsEditor() && !AnarchyMod.Instance.Settings.PreventOverrideInEditor)
            {
                return;
            }

            if (m_ToolSystem.activeTool.toolID == MoveItToolID &&
                m_MoveItTool is not null &&
                m_MoveItSelectedEntitiesPropertyInfo is not null)
            {
                HashSet<Entity> moveItToolSelectedEntities = (HashSet<Entity>)m_MoveItSelectedEntitiesPropertyInfo.GetValue(m_MoveItTool);
                if (!moveItToolSelectedEntities.SetEquals(m_MoveItSelectedEntitiesHashSet))
                {
                    m_MoveItSelectedEntitiesHashSet = moveItToolSelectedEntities;
                    NativeArray<Entity> moveItSelectedEntities = new NativeArray<Entity>(moveItToolSelectedEntities.ToArray(), Allocator.TempJob);
                    WriteMoveItSelectedEntities writeMoveItSelectedEntities = new WriteMoveItSelectedEntities()
                    {
                        m_Entities = m_MoveItSelectedEntities,
                        m_MoveItToolSelectedEntities = moveItSelectedEntities,
                    };
                    JobHandle writerJobHandle = writeMoveItSelectedEntities.Schedule(Dependency);
                    AddEntitiesWriter(writerJobHandle);
                    moveItSelectedEntities.Dispose(writerJobHandle);
                    m_Log.Debug($"{nameof(CheckTransformSystem)}.{nameof(OnUpdate)} saved moveItTool selected entities");
                }

                UpdateTransformRecordJob updateTransformRecordJob = new UpdateTransformRecordJob()
                {
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_MoveItSelectedEntities = GetEntities(readOnly: true, out JobHandle readerDeps),
                    m_TransformRecordType = SystemAPI.GetComponentTypeHandle<TransformRecord>(),
                    m_TransformType = SystemAPI.GetComponentTypeHandle<Transform>(),
                    buffer = m_ModificationBarrier1.CreateCommandBuffer().AsParallelWriter(),
                };
                JobHandle jobHandle = updateTransformRecordJob.ScheduleParallel(m_TransformRecordQuery, JobHandle.CombineDependencies(Dependency, readerDeps));
                m_ModificationBarrier1.AddJobHandleForProducer(jobHandle);
                Dependency = jobHandle;
            }
            else if (EntityManager.TryGetComponent(m_ToolSystem.selected, out TransformRecord transformRecord) &&
                     EntityManager.TryGetComponent(m_ToolSystem.selected, out Game.Objects.Transform originalTransform) &&
                     EntityManager.HasComponent<Updated>(m_ToolSystem.selected))
            {
                if (!EntityManager.TryGetComponent(m_ToolSystem.selected, out Game.Common.Owner owner) ||
                    !EntityManager.TryGetComponent(owner.m_Owner, out Game.Objects.Transform ownerTransform))
                {
                    transformRecord.m_Position = originalTransform.m_Position;
                    transformRecord.m_Rotation = originalTransform.m_Rotation;
                }
                else
                {
                    transformRecord.m_Position = originalTransform.m_Position - ownerTransform.m_Position;
                    transformRecord.m_Rotation = originalTransform.m_Rotation.value - ownerTransform.m_Rotation.value;
                }

                EntityManager.SetComponentData(m_ToolSystem.selected, transformRecord);
            }
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_MoveItSelectedEntities.Dispose();
        }

        private void ProcessSubObject(Game.Objects.SubObject subObject)
        {
            if (EntityManager.TryGetComponent(subObject.m_SubObject, out TransformRecord transformRecord) &&
                EntityManager.TryGetComponent(subObject.m_SubObject, out Game.Objects.Transform originalTransform) &&
                EntityManager.HasComponent<Updated>(subObject.m_SubObject) &&
                EntityManager.TryGetComponent(subObject.m_SubObject, out Owner owner) &&
                EntityManager.TryGetComponent(owner.m_Owner, out Game.Objects.Transform ownerTransform))
            {
                transformRecord.m_Position = originalTransform.m_Position - ownerTransform.m_Position;
                transformRecord.m_Rotation = originalTransform.m_Rotation.value - ownerTransform.m_Rotation.value;
                EntityManager.SetComponentData(subObject.m_SubObject, transformRecord);
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct WriteMoveItSelectedEntities : IJob
        {
            public NativeHashSet<Entity> m_Entities;
            [ReadOnly]
            public NativeArray<Entity> m_MoveItToolSelectedEntities;

            public void Execute()
            {
                m_Entities.Clear();
                foreach (Entity entity in m_MoveItToolSelectedEntities)
                {
                    m_Entities.Add(entity);
                }
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct UpdateTransformRecordJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            public ComponentTypeHandle<TransformRecord> m_TransformRecordType;
            public ComponentTypeHandle<Transform> m_TransformType;
            public EntityCommandBuffer.ParallelWriter buffer;
            public NativeHashSet<Entity> m_MoveItSelectedEntities;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<TransformRecord> transformRecordNativeArray = chunk.GetNativeArray(ref m_TransformRecordType);
                NativeArray<Transform> transformNativeArray = chunk.GetNativeArray(ref m_TransformType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    Transform originalTransform = transformNativeArray[i];
                    TransformRecord transformRecord = transformRecordNativeArray[i];
                    if (m_MoveItSelectedEntities.Contains(currentEntity))
                    {
                        if (!Equals(transformRecord, originalTransform))
                        {
                            transformRecord.m_Position = originalTransform.m_Position;
                            transformRecord.m_Rotation = originalTransform.m_Rotation;
                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, transformRecord);
                        }
                    }
                }
            }

            private bool Equals(TransformRecord record, Transform original)
            {
                if (record.m_Position.x == original.m_Position.x && record.m_Position.y == original.m_Position.y && record.m_Position.z == original.m_Position.z)
                {
                    if (record.m_Rotation.value.x == original.m_Rotation.value.x && record.m_Rotation.value.y == original.m_Rotation.value.y && record.m_Rotation.value.z == original.m_Rotation.value.z && record.m_Rotation.value.w == original.m_Rotation.value.w)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
