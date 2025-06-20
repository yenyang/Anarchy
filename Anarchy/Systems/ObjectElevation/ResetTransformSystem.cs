// <copyright file="ResetTransformSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.ObjectElevation
{
    using Anarchy;
    using Anarchy.Components;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Objects;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// A system that prevents objects from being overriden that has a custom component.
    /// </summary>
    public partial class ResetTransformSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_TransformRecordQuery;
        private ToolSystem m_ToolSystem;
        private ModificationEndBarrier m_Barrier;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetTransformSystem"/> class.
        /// </summary>
        public ResetTransformSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(ResetTransformSystem)} Created.");
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_Barrier = World.GetOrCreateSystemManaged<ModificationEndBarrier>();
            m_TransformRecordQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
               {
                    ComponentType.ReadOnly<Updated>(),
                    ComponentType.ReadOnly<TransformRecord>(),
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
        protected override void OnUpdate()
        {
            if (m_ToolSystem.actionMode.IsEditor() && !AnarchyMod.Instance.Settings.PreventOverrideInEditor)
            {
                return;
            }

            NativeArray<Entity> entities = m_TransformRecordQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out TransformRecord transformRecord) || !EntityManager.TryGetComponent(entity, out Game.Objects.Transform originalTransform))
                {
                    continue;
                }

                EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer();

                if (!EntityManager.TryGetComponent(entity, out Game.Common.Owner owner))
                {
                    if (transformRecord.Equals(originalTransform))
                    {
                        buffer.RemoveComponent<UpdateNextFrame>(entity);
                        continue;
                    }

                    originalTransform.m_Position = transformRecord.m_Position;
                    originalTransform.m_Rotation = transformRecord.m_Rotation;
                    buffer.SetComponent(entity, originalTransform);
                }
                else if (EntityManager.TryGetComponent(owner.m_Owner, out Game.Objects.Transform ownerTransform))
                {
                    Game.Objects.Transform inverseParentTransform = ObjectUtils.InverseTransform(ownerTransform);
                    Game.Objects.Transform localTransform = ObjectUtils.WorldToLocal(inverseParentTransform, originalTransform);

                    if (transformRecord.Equals(localTransform))
                    {
                        buffer.RemoveComponent<UpdateNextFrame>(entity);
                        continue;
                    }

                    Game.Objects.Transform worldTransform = ObjectUtils.LocalToWorld(ownerTransform, transformRecord.Transform);

                    originalTransform.m_Position = worldTransform.m_Position;
                    originalTransform.m_Rotation = worldTransform.m_Rotation;
                    buffer.SetComponent(entity, originalTransform);
                }

                if (EntityManager.HasComponent<UpdateNextFrame>(entity))
                {
                    buffer.RemoveComponent<UpdateNextFrame>(entity);
                }
                else
                {
                    buffer.AddComponent<UpdateNextFrame>(entity);
                }

                if (m_ToolSystem.actionMode.IsEditor() && EntityManager.TryGetBuffer(entity, isReadOnly: true, out DynamicBuffer<SubObject> subObjects))
                {
                    foreach (SubObject subObject in subObjects)
                    {
                        if (EntityManager.TryGetComponent(subObject.m_SubObject, out Game.Objects.Transform subObjectTransform)
                            && EntityManager.TryGetComponent(subObject.m_SubObject, out LocalTransformCache localTransformCache))
                        {
                            subObjectTransform.m_Position = transformRecord.m_Position + localTransformCache.m_Position;

                            // I doubt this is valid and probably only worked with trees because of 0 rotation.
                            subObjectTransform.m_Rotation.value = transformRecord.m_Rotation.value + localTransformCache.m_Rotation.value;
                            buffer.SetComponent(subObject.m_SubObject, subObjectTransform);
                            if (EntityManager.HasComponent<UpdateNextFrame>(entity))
                            {
                                buffer.RemoveComponent<UpdateNextFrame>(entity);
                            }
                            else
                            {
                                buffer.AddComponent<UpdateNextFrame>(entity);
                            }
                        }
                    }
                }
            }

        }
    }
}
