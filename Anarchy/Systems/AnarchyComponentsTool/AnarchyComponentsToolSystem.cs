﻿// <copyright file="AnarchyComponentsToolSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Anarchy.Systems.AnarchyComponentsTool
{
    using System.Linq;
    using Anarchy.Components;
    using Colossal.Logging;
    using Colossal.Mathematics;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Buildings;
    using Game.Citizens;
    using Game.Common;
    using Game.Creatures;
    using Game.Input;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using static Anarchy.Systems.AnarchyComponentsTool.AnarchyComponentsToolUISystem;


    /// <summary>
    /// Tool for controlling adding and removing Anarchy mod components.
    /// </summary>
    public partial class AnarchyComponentsToolSystem : ToolBaseSystem
    {
        private ProxyAction m_ApplyAction;
        private ProxyAction m_SecondaryApplyMimic;
        private OverlayRenderSystem m_OverlayRenderSystem;
        private ToolOutputBarrier m_Barrier;
        private RenderingSystem m_RenderingSystem;
        private ILog m_Log;
        private bool m_PreviousShowMarkers;
        private EntityQuery m_OverridenQuery;
        private EntityQuery m_PreventOverrideQuery;
        private EntityQuery m_ElevationLockedQuery;
        private AnarchyComponentsToolUISystem m_UISystem;
        private EntityQuery m_NotPreventOverrideQuery;
        private EntityQuery m_NotTransformRecordQuery;

        /// <inheritdoc/>
        public override string toolID => "AnarchyComponentsTool";

        /// <summary>
        /// Gets a value indicating whether the previous show markers.
        /// </summary>
        public bool PreviousShowMarkers
        {
            get { return m_PreviousShowMarkers; }
        }

        /// <inheritdoc/>
        public override PrefabBase GetPrefab()
        {
            return null;
        }

        /// <inheritdoc/>
        public override bool TrySetPrefab(PrefabBase prefab)
        {
            return false;
        }

        /// <inheritdoc/>
        public override void InitializeRaycast()
        {
            base.InitializeRaycast();
            m_ToolRaycastSystem.typeMask = TypeMask.Terrain;
        }

        /// <summary>
        /// For stopping the tool. Probably with esc key.
        /// </summary>
        public void RequestDisable()
        {
            m_ToolSystem.activeTool = m_DefaultToolSystem;
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            Enabled = false;
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"[{nameof(AnarchyComponentsToolSystem)}] {nameof(OnCreate)}");
            m_Barrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_RenderingSystem = World.GetOrCreateSystemManaged<RenderingSystem>();
            m_OverlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
            m_UISystem = World.GetOrCreateSystemManaged<AnarchyComponentsToolUISystem>();
            base.OnCreate();

            m_OverridenQuery = SystemAPI.QueryBuilder()
                .WithAll<Overridden, Game.Objects.Object, Game.Objects.Static, Game.Objects.Transform, CullingInfo>()
                .WithNone<Deleted, Temp>()
                .Build();

            m_PreventOverrideQuery = SystemAPI.QueryBuilder()
                .WithAll<PreventOverride, Game.Objects.Object, Game.Objects.Static, Game.Objects.Transform>()
                .WithNone<Deleted, Temp>()
                .Build();

            m_ElevationLockedQuery = SystemAPI.QueryBuilder()
                .WithAll<TransformRecord, Game.Objects.Object, Game.Objects.Static, Game.Objects.Transform>()
                .WithNone<Deleted, Temp>()
                .Build();

            m_NotPreventOverrideQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<Static>(),
                    ComponentType.ReadOnly<Game.Objects.Object>(),
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<PreventOverride>(),
                    ComponentType.ReadOnly<Building>(),
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>(),
                    ComponentType.ReadOnly<Owner>(),
                    ComponentType.ReadOnly<Animal>(),
                    ComponentType.ReadOnly<Game.Creatures.Pet>(),
                    ComponentType.ReadOnly<Creature>(),
                    ComponentType.ReadOnly<Moving>(),
                    ComponentType.ReadOnly<Household>(),
                    ComponentType.ReadOnly<Vehicle>(),
                    ComponentType.ReadOnly<Game.Common.Event>(),
                    ComponentType.ReadOnly<Game.Routes.TransportStop>(),
                    ComponentType.ReadOnly<Game.Routes.TransportLine>(),
                    ComponentType.ReadOnly<Game.Routes.TramStop>(),
                    ComponentType.ReadOnly<Game.Routes.TrainStop>(),
                    ComponentType.ReadOnly<Game.Routes.AirplaneStop>(),
                    ComponentType.ReadOnly<Game.Routes.BusStop>(),
                    ComponentType.ReadOnly<Game.Routes.ShipStop>(),
                    ComponentType.ReadOnly<Game.Routes.TakeoffLocation>(),
                    ComponentType.ReadOnly<Game.Routes.TaxiStand>(),
                    ComponentType.ReadOnly<Game.Routes.Waypoint>(),
                    ComponentType.ReadOnly<Game.Routes.MailBox>(),
                    ComponentType.ReadOnly<Game.Routes.WaypointDefinition>(),
                },
            });

            m_NotTransformRecordQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<Static>(),
                    ComponentType.ReadOnly<Game.Objects.Object>(),
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<TransformRecord>(),
                    ComponentType.ReadOnly<Building>(),
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>(),
                    ComponentType.ReadOnly<Owner>(),
                    ComponentType.ReadOnly<Animal>(),
                    ComponentType.ReadOnly<Game.Creatures.Pet>(),
                    ComponentType.ReadOnly<Creature>(),
                    ComponentType.ReadOnly<Moving>(),
                    ComponentType.ReadOnly<Household>(),
                    ComponentType.ReadOnly<Vehicle>(),
                    ComponentType.ReadOnly<Game.Common.Event>(),
                    ComponentType.ReadOnly<Game.Routes.TransportStop>(),
                    ComponentType.ReadOnly<Game.Routes.TransportLine>(),
                    ComponentType.ReadOnly<Game.Routes.TramStop>(),
                    ComponentType.ReadOnly<Game.Routes.TrainStop>(),
                    ComponentType.ReadOnly<Game.Routes.AirplaneStop>(),
                    ComponentType.ReadOnly<Game.Routes.BusStop>(),
                    ComponentType.ReadOnly<Game.Routes.ShipStop>(),
                    ComponentType.ReadOnly<Game.Routes.TakeoffLocation>(),
                    ComponentType.ReadOnly<Game.Routes.TaxiStand>(),
                    ComponentType.ReadOnly<Game.Routes.Waypoint>(),
                    ComponentType.ReadOnly<Game.Routes.MailBox>(),
                    ComponentType.ReadOnly<Game.Routes.WaypointDefinition>(),
                },
            });

            m_ApplyAction = AnarchyMod.Instance.Settings.GetAction(AnarchyMod.ApplyMimicAction);
            var builtInApplyAction = InputManager.instance.FindAction(InputManager.kToolMap, "Apply");
            var mimicApplyBinding = m_ApplyAction.bindings.FirstOrDefault(b => b.group == nameof(Mouse));
            var builtInApplyBinding = builtInApplyAction.bindings.FirstOrDefault(b => b.group == nameof(Mouse));
            var applyWatcher = new ProxyBinding.Watcher(builtInApplyBinding, binding => SetMimic(mimicApplyBinding, binding));
            SetMimic(mimicApplyBinding, applyWatcher.binding);

            m_SecondaryApplyMimic = AnarchyMod.Instance.Settings.GetAction(AnarchyMod.SecondaryApplyMimicAction);
            var builtInSecondaryApplyAction = InputManager.instance.FindAction(InputManager.kToolMap, "Secondary Apply");
            var mimicSecondaryApplyBinding = m_SecondaryApplyMimic.bindings.FirstOrDefault(b => b.group == nameof(Mouse));
            var builtInSecondaryApplyBinding = builtInSecondaryApplyAction.bindings.FirstOrDefault(b => b.group == nameof(Mouse));
            var secondaryApplyWatcher = new ProxyBinding.Watcher(builtInSecondaryApplyBinding, binding => SetMimic(mimicSecondaryApplyBinding, binding));
            SetMimic(mimicSecondaryApplyBinding, secondaryApplyWatcher.binding);
        }

        /// <inheritdoc/>
        protected override void OnStartRunning()
        {
            m_ApplyAction.shouldBeEnabled = true;
            m_SecondaryApplyMimic.shouldBeEnabled = true;
            m_Log.Debug($"{nameof(AnarchyComponentsToolSystem)}.{nameof(OnStartRunning)}");
            m_PreviousShowMarkers = m_RenderingSystem.markersVisible;
            if ((m_UISystem.CurrentComponentType & AnarchyComponentType.PreventOverride) == AnarchyComponentType.PreventOverride)
            {
                m_RenderingSystem.markersVisible = true;
            }
        }

        /// <inheritdoc/>
        protected override void OnStopRunning()
        {
            m_ApplyAction.shouldBeEnabled = false;
            m_SecondaryApplyMimic.shouldBeEnabled = false;
            m_RenderingSystem.markersVisible = m_PreviousShowMarkers;
        }

        /// <inheritdoc/>
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = Dependency;
            bool raycastFlag = GetRaycastResult(out Entity e, out RaycastHit hit);

            if (!m_OverridenQuery.IsEmptyIgnoreFilter
                && (m_UISystem.CurrentComponentType & AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride) == AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride)
            {
                ObjectHoopRenderJob overridenHoopRenderJob = new ObjectHoopRenderJob()
                {
                    m_Color = UnityEngine.Color.yellow,
                    m_CullingInfoType = SystemAPI.GetComponentTypeHandle<CullingInfo>(isReadOnly: true),
                    m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out JobHandle outJobHandle),
                    m_TransformType = SystemAPI.GetComponentTypeHandle<Game.Objects.Transform>(isReadOnly: true),
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_ObjectGeometryDataLookup = SystemAPI.GetComponentLookup<ObjectGeometryData>(isReadOnly: true),
                    m_PrefabRefLookup = SystemAPI.GetComponentLookup<PrefabRef>(isReadOnly: true),
                    m_TreeLookup = SystemAPI.GetComponentLookup<Game.Objects.Tree>(isReadOnly: true),
                };
                inputDeps = overridenHoopRenderJob.Schedule(m_OverridenQuery, JobHandle.CombineDependencies(inputDeps, outJobHandle));
                m_OverlayRenderSystem.AddBufferWriter(inputDeps);
            }

            if (!m_PreventOverrideQuery.IsEmptyIgnoreFilter
                && (m_UISystem.CurrentComponentType & AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride) == AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride)
            {
                ObjectHoopRenderJob preventOverrideHoopRenderJob = new ObjectHoopRenderJob()
                {
                    m_Color = UnityEngine.Color.red,
                    m_CullingInfoType = SystemAPI.GetComponentTypeHandle<CullingInfo>(isReadOnly: true),
                    m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out JobHandle outJobHandle),
                    m_TransformType = SystemAPI.GetComponentTypeHandle<Game.Objects.Transform>(isReadOnly: true),
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_ObjectGeometryDataLookup = SystemAPI.GetComponentLookup<ObjectGeometryData>(isReadOnly: true),
                    m_PrefabRefLookup = SystemAPI.GetComponentLookup<PrefabRef>(isReadOnly: true),
                    m_TreeLookup = SystemAPI.GetComponentLookup<Game.Objects.Tree>(isReadOnly: true),
                };
                inputDeps = preventOverrideHoopRenderJob.Schedule(m_PreventOverrideQuery, JobHandle.CombineDependencies(inputDeps, outJobHandle));
                m_OverlayRenderSystem.AddBufferWriter(inputDeps);
            }

            if (!m_ElevationLockedQuery.IsEmptyIgnoreFilter
                && (m_UISystem.CurrentComponentType & AnarchyComponentsToolUISystem.AnarchyComponentType.TransformRecord) == AnarchyComponentsToolUISystem.AnarchyComponentType.TransformRecord)
            {
                ObjectHoopRenderJob elevationLockedHoopRenderJob = new ObjectHoopRenderJob()
                {
                    m_Color = UnityEngine.Color.blue,
                    m_CullingInfoType = SystemAPI.GetComponentTypeHandle<CullingInfo>(isReadOnly: true),
                    m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out JobHandle outJobHandle),
                    m_TransformType = SystemAPI.GetComponentTypeHandle<Game.Objects.Transform>(isReadOnly: true),
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_ObjectGeometryDataLookup = SystemAPI.GetComponentLookup<ObjectGeometryData>(isReadOnly: true),
                    m_PrefabRefLookup = SystemAPI.GetComponentLookup<PrefabRef>(isReadOnly: true),
                    m_TreeLookup = SystemAPI.GetComponentLookup<Game.Objects.Tree>(isReadOnly: true),
                };
                inputDeps = elevationLockedHoopRenderJob.Schedule(m_ElevationLockedQuery, JobHandle.CombineDependencies(inputDeps, outJobHandle));
                m_OverlayRenderSystem.AddBufferWriter(inputDeps);
            }

            if (hit.m_HitPosition.x == 0 && hit.m_HitPosition.y == 0 && hit.m_HitPosition.z == 0)
            {
                return inputDeps;
            }

            float radius = m_UISystem.SelectionRadius;
            if (m_UISystem.CurrentSelectionMode == AnarchyComponentsToolUISystem.SelectionMode.Radius)
            {
                ToolRadiusJob toolRadiusJob = new ()
                {
                    m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out JobHandle outJobHandle),
                    m_Position = new Vector3(hit.m_HitPosition.x, hit.m_Position.y, hit.m_HitPosition.z),
                    m_Radius = radius,
                };
                inputDeps = IJobExtensions.Schedule(toolRadiusJob, JobHandle.CombineDependencies(inputDeps, outJobHandle));
                m_OverlayRenderSystem.AddBufferWriter(inputDeps);
            }

            if (m_ApplyAction.IsPressed()
                && (m_UISystem.CurrentComponentType & AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride) == AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride)
            {
                AddOrRemoveComponentWithinRadiusJob addPreventOverrideJob = new ()
                {
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_Position = hit.m_HitPosition,
                    m_Radius = radius,
                    m_TransformType = SystemAPI.GetComponentTypeHandle<Game.Objects.Transform>(isReadOnly: true),
                    buffer = m_Barrier.CreateCommandBuffer(),
                    m_Add = true,
                    m_ComponentType = ComponentType.ReadOnly<PreventOverride>(),
                    m_ObjectGeometryDataLookup = SystemAPI.GetComponentLookup<ObjectGeometryData>(),
                    m_PrefabRefLookup = SystemAPI.GetComponentLookup<PrefabRef>(),
                };
                inputDeps = JobChunkExtensions.Schedule(addPreventOverrideJob, m_NotPreventOverrideQuery, inputDeps);
                m_Barrier.AddJobHandleForProducer(inputDeps);
            }
            else if (m_SecondaryApplyMimic.IsPressed()
                && (m_UISystem.CurrentComponentType & AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride) == AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride)
            {
                AddOrRemoveComponentWithinRadiusJob removePreventOverrideJob = new ()
                {
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_Position = hit.m_HitPosition,
                    m_Radius = radius,
                    m_TransformType = SystemAPI.GetComponentTypeHandle<Game.Objects.Transform>(isReadOnly: true),
                    buffer = m_Barrier.CreateCommandBuffer(),
                    m_Add = false,
                    m_ComponentType = ComponentType.ReadOnly<PreventOverride>(),
                    m_ObjectGeometryDataLookup = SystemAPI.GetComponentLookup<ObjectGeometryData>(),
                    m_PrefabRefLookup = SystemAPI.GetComponentLookup<PrefabRef>(),
                };
                inputDeps = JobChunkExtensions.Schedule(removePreventOverrideJob, m_PreventOverrideQuery, inputDeps);
                m_Barrier.AddJobHandleForProducer(inputDeps);
            }

            if (m_ApplyAction.IsPressed()
                && (m_UISystem.CurrentComponentType & AnarchyComponentsToolUISystem.AnarchyComponentType.TransformRecord) == AnarchyComponentsToolUISystem.AnarchyComponentType.TransformRecord)
            {
                AddOrRemoveComponentWithinRadiusJob addTransformRecordJob = new ()
                {
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_Position = hit.m_HitPosition,
                    m_Radius = radius,
                    m_TransformType = SystemAPI.GetComponentTypeHandle<Game.Objects.Transform>(isReadOnly: true),
                    buffer = m_Barrier.CreateCommandBuffer(),
                    m_Add = true,
                    m_ComponentType = ComponentType.ReadOnly<TransformRecord>(),
                    m_ObjectGeometryDataLookup = SystemAPI.GetComponentLookup<ObjectGeometryData>(),
                    m_PrefabRefLookup = SystemAPI.GetComponentLookup<PrefabRef>(),
                };
                inputDeps = JobChunkExtensions.Schedule(addTransformRecordJob, m_NotTransformRecordQuery, inputDeps);
                m_Barrier.AddJobHandleForProducer(inputDeps);
            }
            else if (m_SecondaryApplyMimic.IsPressed()
                && (m_UISystem.CurrentComponentType & AnarchyComponentsToolUISystem.AnarchyComponentType.TransformRecord) == AnarchyComponentsToolUISystem.AnarchyComponentType.TransformRecord)
            {
                AddOrRemoveComponentWithinRadiusJob removeTransformRecordJob = new ()
                {
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_Position = hit.m_HitPosition,
                    m_Radius = radius,
                    m_TransformType = SystemAPI.GetComponentTypeHandle<Game.Objects.Transform>(isReadOnly: true),
                    buffer = m_Barrier.CreateCommandBuffer(),
                    m_Add = false,
                    m_ComponentType = ComponentType.ReadOnly<TransformRecord>(),
                    m_ObjectGeometryDataLookup = SystemAPI.GetComponentLookup<ObjectGeometryData>(),
                    m_PrefabRefLookup = SystemAPI.GetComponentLookup<PrefabRef>(),
                };
                inputDeps = JobChunkExtensions.Schedule(removeTransformRecordJob, m_ElevationLockedQuery, inputDeps);
                m_Barrier.AddJobHandleForProducer(inputDeps);
            }

            return inputDeps;
        }

        /// <inheritdoc/>
        protected override void OnGameLoaded(Context serializationContext)
        {
            base.OnGameLoaded(serializationContext);
        }

        /// <inheritdoc/>
        protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void SetMimic(ProxyBinding mimic, ProxyBinding buildIn)
        {
            var newMimicBinding = mimic.Copy();
            newMimicBinding.path = buildIn.path;
            newMimicBinding.modifiers = buildIn.modifiers;
            InputManager.instance.SetBinding(newMimicBinding, out _);
        }


#if BURST
        [BurstCompile]
#endif
        private struct AddOrRemoveComponentWithinRadiusJob : IJobChunk
        {
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentTypeHandle<Game.Objects.Transform> m_TransformType;
            public EntityCommandBuffer buffer;
            public float m_Radius;
            public float3 m_Position;
            public ComponentType m_ComponentType;
            public bool m_Add;
            [ReadOnly]
            public ComponentLookup<PrefabRef> m_PrefabRefLookup;
            [ReadOnly]
            public ComponentLookup<Game.Prefabs.ObjectGeometryData> m_ObjectGeometryDataLookup;

            /// <summary>
            /// Executes job which will change state or prefab for trees within a radius.
            /// </summary>
            /// <param name="chunk">ArchteypeChunk of IJobChunk.</param>
            /// <param name="unfilteredChunkIndex">Use for EntityCommandBuffer.ParralelWriter.</param>
            /// <param name="useEnabledMask">Part of IJobChunk. Unsure what it does.</param>
            /// <param name="chunkEnabledMask">Part of IJobChunk. Not sure what it does.</param>
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<Game.Objects.Transform> transformNativeArray = chunk.GetNativeArray(ref m_TransformType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    if (!CheckForWithinRadius(m_Position, transformNativeArray[i].m_Position, m_Radius))
                    {
                        continue;
                    }

                    if (m_Add && m_ComponentType == ComponentType.ReadOnly<PreventOverride>()
                        && m_PrefabRefLookup.TryGetComponent(entityNativeArray[i], out PrefabRef prefabRef)
                        && m_ObjectGeometryDataLookup.TryGetComponent(prefabRef.m_Prefab, out ObjectGeometryData objectGeometryData)
                        && (objectGeometryData.m_Flags & Game.Objects.GeometryFlags.Overridable) != Game.Objects.GeometryFlags.Overridable)
                    {
                        continue;
                    }

                    if (m_Add)
                    {
                        buffer.AddComponent(entityNativeArray[i], m_ComponentType);

                        if (m_Add && m_ComponentType == ComponentType.ReadOnly<TransformRecord>())
                        {
                            TransformRecord transformRecord = new TransformRecord()
                            {
                                m_Position = transformNativeArray[i].m_Position,
                                m_Rotation = transformNativeArray[i].m_Rotation,
                            };
                            buffer.SetComponent(entityNativeArray[i], transformRecord);
                        }
                    }
                    else
                    {
                        buffer.RemoveComponent(entityNativeArray[i], m_ComponentType);
                    }
                }
            }

            /// <summary>
            /// Checks the radius and position and returns true if tree is there.
            /// </summary>
            /// <param name="cursorPosition">Float3 from Raycast.</param>
            /// <param name="position">Float3 position from InterploatedTransform.</param>
            /// <param name="radius">Radius usually passed from settings.</param>
            /// <returns>True if tree position is within radius of position. False if not.</returns>
            private bool CheckForWithinRadius(float3 cursorPosition, float3 position, float radius)
            {
                float minRadius = 10f;
                radius = Mathf.Max(radius, minRadius);
                position.y = cursorPosition.y;
                if (Unity.Mathematics.math.distance(cursorPosition, position) < radius)
                {
                    return true;
                }

                return false;
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct ToolRadiusJob : IJob
        {
            public OverlayRenderSystem.Buffer m_OverlayBuffer;
            public float3 m_Position;
            public float m_Radius;

            /// <summary>
            /// Draws tool radius.
            /// </summary>
            public void Execute()
            {
                m_OverlayBuffer.DrawCircle(new UnityEngine.Color(.52f, .80f, .86f, 1f), default, m_Radius / 20f, 0, new float2(0, 1), m_Position, m_Radius * 2f);
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct ObjectHoopRenderJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentTypeHandle<Game.Objects.Transform> m_TransformType;
            [ReadOnly]
            public ComponentTypeHandle<CullingInfo> m_CullingInfoType;
            public OverlayRenderSystem.Buffer m_OverlayBuffer;
            public UnityEngine.Color m_Color;
            [ReadOnly]
            public ComponentLookup<PrefabRef> m_PrefabRefLookup;
            [ReadOnly]
            public ComponentLookup<Game.Prefabs.ObjectGeometryData> m_ObjectGeometryDataLookup;
            [ReadOnly]
            public ComponentLookup<Game.Objects.Tree> m_TreeLookup;

            /// <summary>
            /// Executes job which will change state or prefab for trees within a radius.
            /// </summary>
            /// <param name="chunk">ArchteypeChunk of IJobChunk.</param>
            /// <param name="unfilteredChunkIndex">Use for EntityCommandBuffer.ParralelWriter.</param>
            /// <param name="useEnabledMask">Part of IJobChunk. Unsure what it does.</param>
            /// <param name="chunkEnabledMask">Part of IJobChunk. Not sure what it does.</param>
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Game.Objects.Transform> transformNativeArray = chunk.GetNativeArray(ref m_TransformType);
                NativeArray<CullingInfo> cullingInfoNativeArray = chunk.GetNativeArray(ref m_CullingInfoType);
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    if (cullingInfoNativeArray[i].m_PassedCulling == 1)
                    {
                        bool circular = true;
                        float radius = 0f;
                        float3 position = transformNativeArray[i].m_Position;
                        Quaternion rotation = transformNativeArray[i].m_Rotation;
                        float3 size = default;

                        if (m_PrefabRefLookup.TryGetComponent(entityNativeArray[i], out PrefabRef prefabRef) && m_ObjectGeometryDataLookup.TryGetComponent(prefabRef.m_Prefab, out ObjectGeometryData objectGeometryData))
                        {
                            if (!m_TreeLookup.HasComponent(entityNativeArray[i]))
                            {
                                position.y += objectGeometryData.m_Size.y / 2f;
                            }

                            circular = (objectGeometryData.m_Flags & Game.Objects.GeometryFlags.Circular) == Game.Objects.GeometryFlags.Circular;
                            if (circular)
                            {
                                radius = objectGeometryData.m_Size.x + 2f;
                            }
                            else
                            {
                                size = objectGeometryData.m_Size;
                            }
                        }

                        if (circular)
                        {
                            m_OverlayBuffer.DrawCircle(m_Color, default, 0.25f, 0, new float2(0, 1), position, radius);
                        }
                        else
                        {
                            DrawRectangle(size, position, rotation);
                        }
                    }
                }
            }

            private void DrawRectangle(float3 size, float3 position, Quaternion rotation)
            {
                float3 eulerAngles = rotation.eulerAngles;
                float3 xDirection = new float3(Mathf.Sin(eulerAngles.y * Mathf.PI / 180f), 0, Mathf.Cos(eulerAngles.y * Mathf.PI / 180f));
                float3 zDirection = new float3(Mathf.Sin((eulerAngles.y + 90) * Mathf.PI / 180f), 0, Mathf.Cos((eulerAngles.y + 90) * Mathf.PI / 180f));
                m_OverlayBuffer.DrawLine(m_Color, new Line3.Segment(new float3(position + (xDirection * size.z / 2f) + (zDirection * size.x / 2f)), new float3(position - (xDirection * size.z / 2f) + (zDirection * size.x / 2f))), 0.125f);
                m_OverlayBuffer.DrawLine(m_Color, new Line3.Segment(new float3(position + (xDirection * size.z / 2f) + (zDirection * size.x / 2f)), new float3(position + (xDirection * size.z / 2f) - (zDirection * size.x / 2f))), 0.125f);
                m_OverlayBuffer.DrawLine(m_Color, new Line3.Segment(new float3(position - (xDirection * size.z / 2f) - (zDirection * size.x / 2f)), new float3(position + (xDirection * size.z / 2f) - (zDirection * size.x / 2f))), 0.125f);
                m_OverlayBuffer.DrawLine(m_Color, new Line3.Segment(new float3(position - (xDirection * size.z / 2f) - (zDirection * size.x / 2f)), new float3(position - (xDirection * size.z / 2f) + (zDirection * size.x / 2f))), 0.125f);
            }
        }

    }
}
