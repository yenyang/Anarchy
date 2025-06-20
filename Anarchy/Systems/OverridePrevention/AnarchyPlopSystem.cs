// <copyright file="AnarchyPlopSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.OverridePrevention
{
    using System.Collections.Generic;
    using Anarchy;
    using Anarchy.Components;
    using Anarchy.Systems.Common;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Buildings;
    using Game.Citizens;
    using Game.Common;
    using Game.Creatures;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Simulation;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// A system that prevents objects from being overriden when placed on each other.
    /// </summary>
    public partial class AnarchyPlopSystem : GameSystemBase
    {
        private readonly List<string> m_AppropriateTools = new List<string>()
        {
            { "Object Tool" },
            { "Line Tool" },
            { "Net Tool" },
        };

        private AnarchyUISystem m_AnarchyUISystem;
        private ILog m_Log;
        private ToolSystem m_ToolSystem;
        private NetToolSystem m_NetToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private EntityQuery m_AppliedQuery;
        private EntityQuery m_AnarchyComponentsQuery;
        private EntityQuery m_OwnedAndOverridenQuery;
        private TerrainSystem m_TerrainSystem;
        private bool m_ElevationChangeIsNegative;

        /// <summary>
        /// Gets or sets a value indicating whether Elevation change is negative.
        /// </summary>
        public bool ElevationChangeIsNegative { get => m_ElevationChangeIsNegative; set => m_ElevationChangeIsNegative = value; }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(AnarchyPlopSystem)} Created.");
            m_AnarchyUISystem = World.GetOrCreateSystemManaged<AnarchyUISystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_TerrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();
            m_AppliedQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<Applied>(),
                },
                Any = new ComponentType[]
                {
                    ComponentType.ReadOnly<Static>(),
                    ComponentType.ReadOnly<Game.Objects.Object>(),
                    ComponentType.ReadOnly<Game.Tools.EditorContainer>(),
                },
                None = new ComponentType[]
                {
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
            m_OwnedAndOverridenQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<Updated>(),
                    ComponentType.ReadOnly<Owner>(),
                    ComponentType.ReadOnly<Overridden>(),
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
                    ComponentType.ReadOnly<Game.Events.Event>(),
                },
            });
            m_AnarchyComponentsQuery = GetEntityQuery(new EntityQueryDesc
            {
                Any = new ComponentType[]
                {
                    ComponentType.ReadWrite<PreventOverride>(),
                    ComponentType.ReadWrite<TransformRecord>(),
                },
            });

            RequireForUpdate(m_AppliedQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            NativeArray<Entity> entitiesWithComponent = m_AnarchyComponentsQuery.ToEntityArray(Allocator.Temp);

            // Cycle through all entities with Prevent Override and transform record component and look for any that shouldn't have been added. Remove component if it is not Overridable Static Object.
            foreach (Entity entity in entitiesWithComponent)
            {
                if (EntityManager.HasComponent(entity, ComponentType.ReadOnly<TransformRecord>()) && EntityManager.HasComponent<Game.Objects.UtilityObject>(entity))
                {
                    EntityManager.RemoveComponent<TransformRecord>(entity);
                }

                if (EntityManager.TryGetComponent(entity, out TransformRecord transformRecord) &&
                    EntityManager.TryGetComponent(entity, out Owner owner) &&
                    EntityManager.TryGetComponent(entity, out Game.Objects.Transform subobjectTransform) &&
                    owner.m_Owner != Entity.Null &&
                    EntityManager.TryGetComponent(owner.m_Owner, out Game.Objects.Transform ownerTransform))
                {
                    // This is to fix a mistake where the transform record stored the wrong values and wasn't setup correctly to handle subobjects.
                    Game.Objects.Transform relativeTransform = new Game.Objects.Transform()
                    {
                        m_Position = subobjectTransform.m_Position - ownerTransform.m_Position,
                        m_Rotation = subobjectTransform.m_Rotation.value - ownerTransform.m_Rotation.value,
                    };

                    if (Mathf.Approximately(transformRecord.m_Position.x, relativeTransform.m_Position.x) &&
                        Mathf.Approximately(transformRecord.m_Position.y, relativeTransform.m_Position.y) &&
                        Mathf.Approximately(transformRecord.m_Position.z, relativeTransform.m_Position.z) &&
                        Mathf.Approximately(transformRecord.m_Rotation.value.x, transformRecord.m_Rotation.value.x) &&
                        Mathf.Approximately(transformRecord.m_Rotation.value.y, transformRecord.m_Rotation.value.y) &&
                        Mathf.Approximately(transformRecord.m_Rotation.value.z, transformRecord.m_Rotation.value.z) &&
                        Mathf.Approximately(transformRecord.m_Rotation.value.w, transformRecord.m_Rotation.value.w))
                    {
                        Game.Objects.Transform inverseParentTransform = ObjectUtils.InverseTransform(ownerTransform);
                        Game.Objects.Transform localTransform = ObjectUtils.WorldToLocal(inverseParentTransform, subobjectTransform);
                        transformRecord.m_Position = localTransform.m_Position;
                        transformRecord.m_Rotation = localTransform.m_Rotation;
                        EntityManager.SetComponentData(entity, transformRecord);
                    }
                }

                PrefabBase prefabBase = null;
                if (EntityManager.TryGetComponent(entity, out PrefabRef prefabRef))
                {
                    if (m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out prefabBase) && EntityManager.HasComponent<Static>(entity) && !EntityManager.HasComponent<Building>(entity))
                    {
                        if (prefabBase is StaticObjectPrefab && EntityManager.TryGetComponent(prefabRef.m_Prefab, out ObjectGeometryData objectGeometryData))
                        {
                            if ((objectGeometryData.m_Flags & Game.Objects.GeometryFlags.Overridable) == Game.Objects.GeometryFlags.Overridable)
                            {
                                continue;
                            }
                        }
                    }
                    else if (m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out prefabBase) && (prefabBase is NetLanePrefab || prefabBase is NetLaneGeometryPrefab))
                    {
                        continue;
                    }
                }

                if (prefabBase != null)
                {
                    m_Log.Debug($"{nameof(AnarchyPlopSystem)}.{nameof(OnGameLoadingComplete)} Removed PreventOverride from {prefabBase.name}");
                }

                if (EntityManager.HasComponent(entity, ComponentType.ReadOnly<PreventOverride>()))
                {
                    EntityManager.RemoveComponent<PreventOverride>(entity);
                }

                if (EntityManager.HasComponent(entity, ComponentType.ReadOnly<TransformRecord>()) && EntityManager.HasComponent<Game.Objects.NetObject>(entity))
                {
                    EntityManager.AddComponent<Deleted>(entity);
                }
            }

            entitiesWithComponent.Dispose();

            base.OnGameLoadingComplete(purpose, mode);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.activeTool.toolID == null || (m_ToolSystem.actionMode.IsEditor() && !AnarchyMod.Instance.Settings.PreventOverrideInEditor) || m_ToolSystem.activePrefab == null)
            {
                return;
            }


            if (!m_NetToolSystem.TrySetPrefab(m_ToolSystem.activePrefab) || m_ToolSystem.activePrefab is NetLaneGeometryPrefab || m_ToolSystem.activePrefab is NetLanePrefab)
            {
                EntityCommandBuffer buffer =  new EntityCommandBuffer(Allocator.Temp);
                NativeArray<Entity> appliedEntities = m_AppliedQuery.ToEntityArray(Allocator.Temp);
                NativeArray<Entity> ownedAndOverridenEnities = m_OwnedAndOverridenQuery.ToEntityArray(Allocator.Temp);
                m_Log.Debug($"{nameof(AnarchyPlopSystem)}.{nameof(OnUpdate)}");
                if (m_AnarchyUISystem.AnarchyEnabled && m_AppropriateTools.Contains(m_ToolSystem.activeTool.toolID))
                {
                    buffer.RemoveComponent<Overridden>(appliedEntities);
                    buffer.RemoveComponent<Overridden>(ownedAndOverridenEnities);
                }

                foreach (Entity entity in appliedEntities)
                {
                    PrefabBase prefabBase = null;
                    if (EntityManager.TryGetComponent(entity, out PrefabRef prefabRef)) 
                    {
                        if (m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out prefabBase))
                        {
                            if (prefabBase is StaticObjectPrefab && EntityManager.TryGetComponent(prefabRef.m_Prefab, out ObjectGeometryData objectGeometryData) && prefabBase is not BuildingPrefab && ((objectGeometryData.m_Flags & Game.Objects.GeometryFlags.Overridable) == Game.Objects.GeometryFlags.Overridable || !EntityManager.HasComponent<Game.Objects.NetObject>(entity)))
                            {
                                // added for compatibility with EDT.
                                bool isInappropriate = false;
                                if (EntityManager.TryGetComponent(prefabRef.m_Prefab, out PlaceableObjectData placeableObjectData) &&
                                    ((placeableObjectData.m_Flags & PlacementFlags.RoadEdge) == PlacementFlags.RoadEdge
                                  || (placeableObjectData.m_Flags & PlacementFlags.RoadNode) == PlacementFlags.RoadNode
                                  || (placeableObjectData.m_Flags & PlacementFlags.RoadSide) == PlacementFlags.RoadSide))
                                {
                                    isInappropriate = true;
                                }

                                if (m_ToolSystem.actionMode.IsGame() && EntityManager.TryGetComponent(entity, out Attached attachedComponent) && !isInappropriate)
                                {
                                    if (EntityManager.TryGetBuffer(attachedComponent.m_Parent, isReadOnly: true, out DynamicBuffer<Game.Objects.SubObject> subObjectBuffer))
                                    {
                                        // Loop through all subobjecst started at last entry to try and quickly find applied entity.
                                        DynamicBuffer<Game.Objects.SubObject> subObjects = buffer.AddBuffer<Game.Objects.SubObject>(attachedComponent.m_Parent);
                                        for (int i = 0; i < subObjectBuffer.Length; i++)
                                        {
                                            Game.Objects.SubObject subObject = subObjectBuffer[i];
                                            if (subObject.m_SubObject != entity)
                                            {
                                                subObjects.Add(subObject);
                                            }
                                        }
                                    }

                                    buffer.RemoveComponent<Attached>(entity);
                                    if (EntityManager.TryGetComponent(entity, out Game.Objects.Transform originalTransform) && m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create && !EntityManager.HasComponent<TransformRecord>(entity))
                                    {
                                        buffer.AddComponent<TransformRecord>(entity);
                                        TransformRecord transformRecord = new () { m_Position = originalTransform.m_Position, m_Rotation = originalTransform.m_Rotation };
                                        buffer.SetComponent(entity, transformRecord);
                                    }
                                }

                                bool hasTransform = EntityManager.TryGetComponent(entity, out Game.Objects.Transform originalTransform2);
                                TerrainHeightData terrainHeightData = m_TerrainSystem.GetHeightData();
                                float terrainHeight = TerrainUtils.SampleHeight(ref terrainHeightData, originalTransform2.m_Position);

                                // added for compatibility with EDT.
                                if (hasTransform && !EntityManager.HasComponent<TransformRecord>(entity) &&
                                    (m_ToolSystem.activeTool == m_ObjectToolSystem || m_ToolSystem.activeTool.toolID == "Line Tool") &&
                                    (m_AnarchyUISystem.LockElevation || (originalTransform2.m_Position.y <= terrainHeight - 0.1f && m_ElevationChangeIsNegative)))
                                {
                                    buffer.AddComponent<TransformRecord>(entity);
                                    TransformRecord transformRecord = new () { m_Position = originalTransform2.m_Position, m_Rotation = originalTransform2.m_Rotation };
                                    buffer.SetComponent(entity, transformRecord);

                                    if (!EntityManager.HasComponent<Game.Objects.Elevation>(entity) && originalTransform2.m_Position.y <= terrainHeight - 0.1f && m_ElevationChangeIsNegative)
                                    {
                                        EntityManager.AddComponent<Game.Objects.Elevation>(entity);
                                        Game.Objects.Elevation elevation = new Game.Objects.Elevation()
                                        {
                                            m_Elevation = originalTransform2.m_Position.y - terrainHeight,
                                            m_Flags = ElevationFlags.Lowered,
                                        };
                                        buffer.SetComponent(entity, elevation);
                                    }
                                }

                                if (m_AnarchyUISystem.AnarchyEnabled && m_AppropriateTools.Contains(m_ToolSystem.activeTool.toolID) && (objectGeometryData.m_Flags & Game.Objects.GeometryFlags.Overridable) == Game.Objects.GeometryFlags.Overridable)
                                {
                                    m_Log.Debug($"{nameof(AnarchyPlopSystem)}.{nameof(OnUpdate)} Added PreventOverride to {prefabBase.name}");
                                    buffer.AddComponent<PreventOverride>(entity);

                                    continue;
                                }
                            }
                            else if (m_ToolSystem.actionMode.IsGame() && prefabBase.GetPrefabID().ToString() == "NetPrefab:Lane Editor Container" && EntityManager.TryGetBuffer(entity, isReadOnly: true, out DynamicBuffer<Game.Net.SubLane> subLaneBuffer) && m_AnarchyUISystem.AnarchyEnabled && m_AppropriateTools.Contains(m_ToolSystem.activeTool.toolID))
                            {
                                // Loop through all subobjects started at last entry to try and quickly find applied entity.
                                for (int i = 0; i < subLaneBuffer.Length; i++)
                                {
                                    Game.Net.SubLane subLane = subLaneBuffer[i];
                                    m_Log.Debug($"{nameof(AnarchyPlopSystem)}.{nameof(OnUpdate)} Added PreventOverride to {prefabBase.name}");
                                    buffer.AddComponent(subLane.m_SubLane, ComponentType.ReadOnly<PreventOverride>());
                                    m_Log.Debug($"{nameof(AnarchyPlopSystem)}.{nameof(OnUpdate)} Added DoNotForceUpdate to {prefabBase.name}");
                                    buffer.AddComponent(subLane.m_SubLane, ComponentType.ReadOnly<DoNotForceUpdate>());
                                }
                            }
                        }
                    }

                    if (prefabBase != null)
                    {
                        m_Log.Debug($"{nameof(AnarchyPlopSystem)}.{nameof(OnUpdate)} Would not add PreventOverride to {prefabBase.name}");
                    }
                }

                buffer.Playback(EntityManager);
                buffer.Dispose();
                appliedEntities.Dispose();
            }
        }
    }
}
