// <copyright file="AnarchyComponentsToolSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Anarchy.Systems.AnarchyComponentsTool
{
    using Anarchy.Components;
    using Anarchy.Systems.Common;
    using Colossal.Entities;
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
    using static Anarchy.Systems.AnarchyComponentsTool.AnarchyComponentsToolUISystem;
    using static Game.Input.UIBaseInputAction;

    /// <summary>
    /// Tool for controlling adding and removing Anarchy mod components.
    /// </summary>
    public partial class AnarchyComponentsToolSystem : ToolBaseSystem
    {
        private OverlayRenderSystem m_OverlayRenderSystem;
        private ToolOutputBarrier m_Barrier;
        private RenderingSystem m_RenderingSystem;
        private ILog m_Log;
        private bool m_PreviousShowMarkers;
        private EntityQuery m_OverridenQuery;
        private EntityQuery m_PreventOverrideQuery;
        private EntityQuery m_TransformLockedQuery;
        private EntityQuery m_PreventOverrideOnlyQuery;
        private EntityQuery m_TransformLockedOnlyQuery;
        private EntityQuery m_PreventOverrideAndTransformLockedQuery;
        private AnarchyComponentsToolUISystem m_UISystem;
        private SelectedInfoPanelTogglesSystem m_SelectedInfoPanelTogglesSystem;
        private EntityQuery m_NotPreventOverrideQuery;
        private EntityQuery m_NotTransformRecordQuery;
        private EntityQuery m_HighlightedQuery;
        private Entity m_PreviousRaycastedEntity = Entity.Null;
        private ToolBaseSystem m_PreviousToolSystem;
        private bool m_SetToolToPreviousTool;
        private bool m_MustStartRunning = false;

        /// <inheritdoc/>
        public override string toolID => "AnarchyComponentsTool";

        /// <summary>
        /// Gets a value indicating whether the previous show markers.
        /// </summary>
        public bool PreviousShowMarkers
        {
            get { return m_PreviousShowMarkers; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the tool must start running.
        /// </summary>
        public bool MustStartRunning
        {
            get { return m_MustStartRunning; }
            set { m_MustStartRunning = value; }
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
            if (m_UISystem.CurrentSelectionMode == SelectionMode.Radius)
            {
                m_ToolRaycastSystem.typeMask = TypeMask.Terrain;
            }
            else
            {
                m_ToolRaycastSystem.typeMask = TypeMask.StaticObjects;
                m_ToolRaycastSystem.raycastFlags |= RaycastFlags.Markers | RaycastFlags.Placeholders;
                if ((m_UISystem.CurrentTier & Tier.SubElements) == Tier.SubElements)
                {
                    m_ToolRaycastSystem.raycastFlags |= RaycastFlags.SubElements;
                }

                if ((m_UISystem.CurrentTier & Tier.MainElements) != Tier.MainElements)
                {
                    m_ToolRaycastSystem.raycastFlags |= RaycastFlags.NoMainElements;
                }
            }
        }

        /// <summary>
        /// For stopping the tool. Probably with esc key. No longer appears to work as expected.
        /// </summary>
        public void RequestDisable()
        {
            m_ToolSystem.activeTool = m_PreviousToolSystem;
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
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"[{nameof(AnarchyComponentsToolSystem)}] {nameof(OnCreate)}");
            m_Barrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_RenderingSystem = World.GetOrCreateSystemManaged<RenderingSystem>();
            m_OverlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
            m_SelectedInfoPanelTogglesSystem = World.GetOrCreateSystemManaged<SelectedInfoPanelTogglesSystem>();
            m_PreviousToolSystem = m_DefaultToolSystem;
            m_UISystem = World.GetOrCreateSystemManaged<AnarchyComponentsToolUISystem>();
            m_ToolSystem.EventToolChanged += (ToolBaseSystem tool) =>
            {
                if (tool != this)
                {
                    m_PreviousToolSystem = tool;
                }
            };
            m_OverridenQuery = SystemAPI.QueryBuilder()
                .WithAll<Overridden, Game.Objects.Object, Game.Objects.Static, Game.Objects.Transform, CullingInfo>()
                .WithNone<Deleted, Temp>()
                .Build();

            m_PreventOverrideQuery = SystemAPI.QueryBuilder()
                .WithAll<PreventOverride, Game.Objects.Object, Game.Objects.Static, Game.Objects.Transform>()
                .WithNone<Deleted, Temp>()
                .Build();

            m_TransformLockedQuery = SystemAPI.QueryBuilder()
                .WithAll<TransformRecord, Game.Objects.Object, Game.Objects.Static, Game.Objects.Transform>()
                .WithNone<Deleted, Temp>()
                .Build();

            m_PreventOverrideOnlyQuery = SystemAPI.QueryBuilder()
              .WithAll<PreventOverride, Game.Objects.Object, Game.Objects.Static, Game.Objects.Transform>()
              .WithNone<Deleted, Temp, TransformRecord>()
              .Build();

            m_TransformLockedOnlyQuery = SystemAPI.QueryBuilder()
                .WithAll<TransformRecord, Game.Objects.Object, Game.Objects.Static, Game.Objects.Transform>()
                .WithNone<Deleted, Temp, PreventOverride>()
                .Build();


            m_PreventOverrideAndTransformLockedQuery = SystemAPI.QueryBuilder()
                .WithAll<PreventOverride, TransformRecord, Game.Objects.Object, Game.Objects.Static, Game.Objects.Transform>()
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
                    ComponentType.ReadOnly<Game.Objects.NetObject>(),
                    ComponentType.ReadOnly<Game.Objects.UtilityObject>(),
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
                    ComponentType.ReadOnly<Stack>(),
                    ComponentType.ReadOnly<Building>(),
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>(),
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
                    ComponentType.ReadOnly<Game.Objects.NetObject>(),
                    ComponentType.ReadOnly<Game.Objects.UtilityObject>(),
                },
            });

            m_HighlightedQuery = SystemAPI.QueryBuilder()
                .WithAll<Highlighted>()
                .WithNone<Deleted, Temp>()
                .Build();

        }

        /// <inheritdoc/>
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            applyAction.enabled = true;
            secondaryApplyAction.enabled = true;
            m_MustStartRunning = false;
            m_Log.Debug($"{nameof(AnarchyComponentsToolSystem)}.{nameof(OnStartRunning)}");
            m_PreviousShowMarkers = m_RenderingSystem.markersVisible;
            if ((m_UISystem.CurrentComponentType & AnarchyComponentType.PreventOverride) == AnarchyComponentType.PreventOverride
                && m_UISystem.CurrentSelectionMode == SelectionMode.Radius)
            {
                m_RenderingSystem.markersVisible = true;
            }
        }

        /// <inheritdoc/>
        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            m_RenderingSystem.markersVisible = m_PreviousShowMarkers;
            if (!m_HighlightedQuery.IsEmptyIgnoreFilter)
            {
                EntityManager.AddComponent<BatchesUpdated>(m_HighlightedQuery);
                EntityManager.RemoveComponent<Highlighted>(m_HighlightedQuery);
            }
        }

        /// <inheritdoc/>
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = Dependency;
            bool raycastFlag = GetRaycastResult(out Entity currentRaycastEntity, out RaycastHit hit);

            if (!m_OverridenQuery.IsEmptyIgnoreFilter
                && (m_UISystem.CurrentComponentType & AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride) == AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride
                && m_UISystem.CurrentSelectionMode == SelectionMode.Radius)
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

            if (!m_PreventOverrideOnlyQuery.IsEmptyIgnoreFilter
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
                inputDeps = preventOverrideHoopRenderJob.Schedule(m_PreventOverrideOnlyQuery, JobHandle.CombineDependencies(inputDeps, outJobHandle));
                m_OverlayRenderSystem.AddBufferWriter(inputDeps);
            }

            if (!m_TransformLockedOnlyQuery.IsEmptyIgnoreFilter
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
                inputDeps = elevationLockedHoopRenderJob.Schedule(m_TransformLockedOnlyQuery, JobHandle.CombineDependencies(inputDeps, outJobHandle));
                m_OverlayRenderSystem.AddBufferWriter(inputDeps);
            }

            if (!m_PreventOverrideAndTransformLockedQuery.IsEmptyIgnoreFilter)
            {
                // Purple for both.
                UnityEngine.Color color = new UnityEngine.Color(0.5f, 0f, 0.5f);
                if (m_UISystem.CurrentComponentType == AnarchyComponentType.TransformRecord)
                {
                    color = UnityEngine.Color.blue;
                }
                else if (m_UISystem.CurrentComponentType == AnarchyComponentType.PreventOverride)
                {
                    color = UnityEngine.Color.red;
                }

                ObjectHoopRenderJob elevationLockedHoopRenderJob = new ObjectHoopRenderJob()
                {
                    m_Color = color,
                    m_CullingInfoType = SystemAPI.GetComponentTypeHandle<CullingInfo>(isReadOnly: true),
                    m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out JobHandle outJobHandle),
                    m_TransformType = SystemAPI.GetComponentTypeHandle<Game.Objects.Transform>(isReadOnly: true),
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_ObjectGeometryDataLookup = SystemAPI.GetComponentLookup<ObjectGeometryData>(isReadOnly: true),
                    m_PrefabRefLookup = SystemAPI.GetComponentLookup<PrefabRef>(isReadOnly: true),
                    m_TreeLookup = SystemAPI.GetComponentLookup<Game.Objects.Tree>(isReadOnly: true),
                };
                inputDeps = elevationLockedHoopRenderJob.Schedule(m_PreventOverrideAndTransformLockedQuery, JobHandle.CombineDependencies(inputDeps, outJobHandle));
                m_OverlayRenderSystem.AddBufferWriter(inputDeps);
            }

            float radius = m_UISystem.SelectionRadius;
            if (m_UISystem.CurrentSelectionMode == AnarchyComponentsToolUISystem.SelectionMode.Radius)
            {
                if (hit.m_HitPosition.x == 0 &&
                    hit.m_HitPosition.y == 0 &&
                    hit.m_HitPosition.z == 0)
                {
                    return inputDeps;
                }

                ToolRadiusJob toolRadiusJob = new ()
                {
                    m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out JobHandle outJobHandle),
                    m_Position = new Vector3(hit.m_HitPosition.x, hit.m_Position.y, hit.m_HitPosition.z),
                    m_Radius = radius,
                };
                inputDeps = IJobExtensions.Schedule(toolRadiusJob, JobHandle.CombineDependencies(inputDeps, outJobHandle));
                m_OverlayRenderSystem.AddBufferWriter(inputDeps);

                if (applyAction.IsPressed() &&
                   (m_UISystem.CurrentComponentType & AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride) == AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride)
                {
                    AddOrRemoveComponentWithinRadiusJob addPreventOverrideJob = new()
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
                        m_OwnerLookup = SystemAPI.GetComponentLookup<Owner>(),
                        m_TransformLookup = SystemAPI.GetComponentLookup<Game.Objects.Transform>(),
                        m_NodeLookup = SystemAPI.GetComponentLookup<Game.Net.Node>(),
                        m_Tier = m_UISystem.CurrentTier,
                    };
                    inputDeps = JobChunkExtensions.Schedule(addPreventOverrideJob, m_NotPreventOverrideQuery, inputDeps);
                    m_Barrier.AddJobHandleForProducer(inputDeps);
                }
                else if (secondaryApplyAction.IsPressed() &&
                        (m_UISystem.CurrentComponentType & AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride) == AnarchyComponentsToolUISystem.AnarchyComponentType.PreventOverride)
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
                        m_OwnerLookup = SystemAPI.GetComponentLookup<Owner>(),
                        m_TransformLookup = SystemAPI.GetComponentLookup<Game.Objects.Transform>(),
                        m_NodeLookup = SystemAPI.GetComponentLookup<Game.Net.Node>(),
                        m_Tier = m_UISystem.CurrentTier,
                    };
                    inputDeps = JobChunkExtensions.Schedule(removePreventOverrideJob, m_PreventOverrideQuery, inputDeps);
                    m_Barrier.AddJobHandleForProducer(inputDeps);
                }

                if (applyAction.IsPressed() &&
                   (m_UISystem.CurrentComponentType & AnarchyComponentsToolUISystem.AnarchyComponentType.TransformRecord) == AnarchyComponentsToolUISystem.AnarchyComponentType.TransformRecord)
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
                        m_OwnerLookup = SystemAPI.GetComponentLookup<Owner>(),
                        m_TransformLookup = SystemAPI.GetComponentLookup<Game.Objects.Transform>(),
                        m_NodeLookup = SystemAPI.GetComponentLookup<Game.Net.Node>(),
                        m_Tier = m_UISystem.CurrentTier,
                    };
                    inputDeps = JobChunkExtensions.Schedule(addTransformRecordJob, m_NotTransformRecordQuery, inputDeps);
                    m_Barrier.AddJobHandleForProducer(inputDeps);
                }
                else if (secondaryApplyAction.IsPressed() &&
                        (m_UISystem.CurrentComponentType & AnarchyComponentsToolUISystem.AnarchyComponentType.TransformRecord) == AnarchyComponentsToolUISystem.AnarchyComponentType.TransformRecord)
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
                        m_OwnerLookup = SystemAPI.GetComponentLookup<Owner>(),
                        m_TransformLookup = SystemAPI.GetComponentLookup<Game.Objects.Transform>(),
                        m_NodeLookup = SystemAPI.GetComponentLookup<Game.Net.Node>(),
                        m_Tier = m_UISystem.CurrentTier,
                    };
                    inputDeps = JobChunkExtensions.Schedule(removeTransformRecordJob, m_TransformLockedQuery, inputDeps);
                    m_Barrier.AddJobHandleForProducer(inputDeps);
                }

                return inputDeps;
            }

            EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer();
            if (!m_HighlightedQuery.IsEmptyIgnoreFilter &&
               (currentRaycastEntity != m_PreviousRaycastedEntity ||
               !ScreenEntity(currentRaycastEntity) ||
               (hit.m_HitPosition.x == 0 && hit.m_HitPosition.y == 0 && hit.m_HitPosition.z == 0)))
            {
                buffer.AddComponent<BatchesUpdated>(m_HighlightedQuery, EntityQueryCaptureMode.AtPlayback);
                buffer.RemoveComponent<Highlighted>(m_HighlightedQuery, EntityQueryCaptureMode.AtPlayback);
                m_PreviousRaycastedEntity = Entity.Null;
                return inputDeps;
            }

            if (raycastFlag)
            {
                if (!EntityManager.HasComponent<Highlighted>(currentRaycastEntity) &&
                    ScreenEntity(currentRaycastEntity) &&
                    m_PreviousRaycastedEntity == Entity.Null)
                {
                    buffer.AddComponent<Highlighted>(currentRaycastEntity);
                    buffer.AddComponent<BatchesUpdated>(currentRaycastEntity);
                    m_PreviousRaycastedEntity = currentRaycastEntity;
                }

                if (applyAction.WasReleasedThisFrame() &&
                    ScreenEntity(currentRaycastEntity))
                {
                    if (ScreenEntity(currentRaycastEntity, AnarchyComponentType.TransformRecord) &&
                       (m_UISystem.CurrentComponentType & AnarchyComponentType.TransformRecord) == AnarchyComponentType.TransformRecord &&
                       !EntityManager.HasComponent<TransformRecord>(currentRaycastEntity) &&
                        EntityManager.TryGetComponent(currentRaycastEntity, out Game.Objects.Transform transform))
                    {
                        buffer.AddComponent<TransformRecord>(currentRaycastEntity);
                        TransformRecord transformRecord = new ();

                        if (!EntityManager.TryGetComponent(currentRaycastEntity, out Game.Common.Owner owner) ||
                           (!EntityManager.HasComponent<Game.Objects.Transform>(owner.m_Owner) &&
                            !EntityManager.HasComponent<Game.Net.Node>(owner.m_Owner)))
                        {
                            transformRecord.m_Position = transform.m_Position;
                            transformRecord.m_Rotation = transform.m_Rotation;
                        }
                        else if (EntityManager.TryGetComponent(owner.m_Owner, out Game.Objects.Transform ownerTransform))
                        {
                            Game.Objects.Transform inverseParentTransform = ObjectUtils.InverseTransform(ownerTransform);
                            Game.Objects.Transform localTransform = ObjectUtils.WorldToLocal(inverseParentTransform, transform);
                            transformRecord.m_Position = localTransform.m_Position;
                            transformRecord.m_Rotation = localTransform.m_Rotation;
                        }
                        else if (EntityManager.TryGetComponent(owner.m_Owner, out Game.Net.Node node))
                        {
                            Game.Objects.Transform inverseParentTransform = ObjectUtils.InverseTransform(new Game.Objects.Transform(node.m_Position, node.m_Rotation));
                            Game.Objects.Transform localTransform = ObjectUtils.WorldToLocal(inverseParentTransform, transform);
                            transformRecord.m_Position = localTransform.m_Position;
                            transformRecord.m_Rotation = localTransform.m_Rotation;
                        }

                        buffer.SetComponent(currentRaycastEntity, transformRecord);
                    }
                    else
                    {
                        // Tooltip as to why can't add component.
                    }

                    if (ScreenEntity(currentRaycastEntity, AnarchyComponentType.PreventOverride) &&
                        (m_UISystem.CurrentComponentType & AnarchyComponentType.PreventOverride) == AnarchyComponentType.PreventOverride &&
                        !EntityManager.HasComponent<PreventOverride>(currentRaycastEntity))
                    {
                       buffer.AddComponent<PreventOverride>(currentRaycastEntity);
                    }
                    else
                    {
                       // tooltip as to why can't add component.
                    }
                }
                else if (secondaryApplyAction.WasReleasedThisFrame() && ScreenEntity(currentRaycastEntity))
                {
                    if ((m_UISystem.CurrentComponentType & AnarchyComponentType.TransformRecord) == AnarchyComponentType.TransformRecord &&
                         EntityManager.HasComponent<TransformRecord>(currentRaycastEntity))
                    {
                        buffer.RemoveComponent<TransformRecord>(currentRaycastEntity);
                    }
                    else
                    {
                        // Tooltip as to why can't remove component.
                    }

                    if ((m_UISystem.CurrentComponentType & AnarchyComponentType.PreventOverride) == AnarchyComponentType.PreventOverride &&
                         EntityManager.HasComponent<PreventOverride>(currentRaycastEntity))
                    {
                        buffer.RemoveComponent<PreventOverride>(currentRaycastEntity);
                    }
                    else
                    {
                        // tooltip as to why can't remove component.
                    }
                }
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

        /// <summary>
        /// Validates whether entity should receive anarchy components.
        /// </summary>
        /// <returns>True if entity can receive anarchy components. False if not approved.</returns>
        private bool ScreenEntity(Entity instanceEntity, AnarchyComponentsToolUISystem.AnarchyComponentType componentType)
        {
            if (componentType == (AnarchyComponentType.PreventOverride | AnarchyComponentType.TransformRecord))
            {
                return m_SelectedInfoPanelTogglesSystem.CheckOverridable(instanceEntity) ||
                       m_SelectedInfoPanelTogglesSystem.CheckDisturbable(instanceEntity);
            }

            if ((componentType & AnarchyComponentType.PreventOverride) == AnarchyComponentType.PreventOverride)
            {
                return m_SelectedInfoPanelTogglesSystem.CheckOverridable(instanceEntity);
            }

            if ((componentType & AnarchyComponentType.TransformRecord) == AnarchyComponentType.TransformRecord)
            {
                return m_SelectedInfoPanelTogglesSystem.CheckDisturbable(instanceEntity);
            }

            return false;
        }

        private bool ScreenEntity(Entity instanceEntity)
        {
            return ScreenEntity(instanceEntity, m_UISystem.CurrentComponentType);
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
            [ReadOnly]
            public ComponentLookup<Game.Common.Owner> m_OwnerLookup;
            [ReadOnly]
            public ComponentLookup<Game.Objects.Transform> m_TransformLookup;
            [ReadOnly]
            public ComponentLookup<Game.Net.Node> m_NodeLookup;
            public Tier m_Tier;

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

                    if ((m_Tier & Tier.MainElements) != Tier.MainElements &&
                       !m_OwnerLookup.HasComponent(entityNativeArray[i]))
                    {
                        continue;
                    }

                    if ((m_Tier & Tier.SubElements) != Tier.SubElements &&
                        m_OwnerLookup.HasComponent(entityNativeArray[i]))
                    {
                        continue;
                    }

                    if (m_Add)
                    {
                        buffer.AddComponent(entityNativeArray[i], m_ComponentType);

                        if (m_Add && m_ComponentType == ComponentType.ReadOnly<TransformRecord>())
                        {
                            TransformRecord transformRecord = new ();
                            if (!m_OwnerLookup.TryGetComponent(entityNativeArray[i], out Game.Common.Owner owner) ||
                               (!m_TransformLookup.HasComponent(owner.m_Owner) &&
                                !m_NodeLookup.HasComponent(owner.m_Owner)))
                            {
                                transformRecord.m_Position = transformNativeArray[i].m_Position;
                                transformRecord.m_Rotation = transformNativeArray[i].m_Rotation;
                            }
                            else if (m_TransformLookup.TryGetComponent(owner.m_Owner, out Game.Objects.Transform ownerTransform))
                            {
                                Game.Objects.Transform inverseParentTransform = ObjectUtils.InverseTransform(ownerTransform);
                                Game.Objects.Transform localTransform = ObjectUtils.WorldToLocal(inverseParentTransform, transformNativeArray[i]);
                                transformRecord.m_Position = localTransform.m_Position;
                                transformRecord.m_Rotation = localTransform.m_Rotation;
                            }
                            else if (m_NodeLookup.TryGetComponent(owner.m_Owner, out Game.Net.Node node))
                            {
                                Game.Objects.Transform inverseParentTransform = ObjectUtils.InverseTransform(new Game.Objects.Transform(node.m_Position, node.m_Rotation));
                                Game.Objects.Transform localTransform = ObjectUtils.WorldToLocal(inverseParentTransform, transformNativeArray[i]);
                                transformRecord.m_Position = localTransform.m_Position;
                                transformRecord.m_Rotation = localTransform.m_Rotation;
                            }

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
