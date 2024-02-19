// <copyright file="AnarchyPlopSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using System.Collections.Generic;
    using Anarchy;
    using Anarchy.Components;
    using Anarchy.Tooltip;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Buildings;
    using Game.Citizens;
    using Game.Common;
    using Game.Creatures;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// A system that prevents objects from being overriden when placed on each other.
    /// </summary>
    public partial class AnarchyPlopSystem : GameSystemBase
    {
        private readonly List<string> m_AppropriateTools = new List<string>()
        {
            { "Object Tool" },
            { "Line Tool" },
        };

        private AnarchySystem m_AnarchySystem;
        private ILog m_Log;
        private ToolSystem m_ToolSystem;
        private NetToolSystem m_NetToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private EntityQuery m_CreatedQuery;
        private EntityQuery m_PreventOverrideQuery;
        private EntityQuery m_OwnedAndOverridenQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnarchyPlopSystem"/> class.
        /// </summary>
        public AnarchyPlopSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Logger;
            m_Log.effectivenessLevel = Level.Info;
            m_Log.Info($"{nameof(AnarchyPlopSystem)} Created.");
            m_AnarchySystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<AnarchySystem>();
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_NetToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<NetToolSystem>();
            m_ObjectToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_PrefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            m_CreatedQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<Created>(),
                    ComponentType.ReadOnly<Updated>(),
                    ComponentType.ReadOnly<Static>(),
                    ComponentType.ReadOnly<Object>(),
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<Temp>(),
                    ComponentType.ReadOnly<Owner>(),
                    ComponentType.ReadOnly<Building>(),
                    ComponentType.ReadOnly<Animal>(),
                    ComponentType.ReadOnly<Game.Creatures.Pet>(),
                    ComponentType.ReadOnly<Creature>(),
                    ComponentType.ReadOnly<Moving>(),
                    ComponentType.ReadOnly<Household>(),
                    ComponentType.ReadOnly<Vehicle>(),
                    ComponentType.ReadOnly<Event>(),
                },
            });
            m_OwnedAndOverridenQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<Updated>(),
                    ComponentType.ReadOnly<Owner>(),
                    ComponentType.ReadOnly<Static>(),
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
                    ComponentType.ReadOnly<Event>(),
                },
            });
            m_PreventOverrideQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadWrite<PreventOverride>(),
                },
            });

            RequireForUpdate(m_CreatedQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            NativeArray<Entity> entitiesWithComponent = m_PreventOverrideQuery.ToEntityArray(Allocator.Temp);

            // Cycle through all entities with Prevent Override component and look for any that shouldn't have been added. Remove component if it is not Overridable Static Object.
            foreach (Entity entity in entitiesWithComponent)
            {
                PrefabBase prefabBase = null;
                if (EntityManager.TryGetComponent(entity, out PrefabRef prefabRef))
                {
                    if (m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out prefabBase) && EntityManager.HasComponent<Static>(entity) && !EntityManager.HasComponent<Building>(entity) && !EntityManager.HasComponent<Owner>(entity) && !EntityManager.HasComponent<Crane>(entity))
                    {
                        if (prefabBase is StaticObjectPrefab && EntityManager.TryGetComponent(prefabRef.m_Prefab, out ObjectGeometryData objectGeometryData))
                        {
                            if ((objectGeometryData.m_Flags & GeometryFlags.Overridable) == GeometryFlags.Overridable)
                            {
                                continue;
                            }
                        }
                    }
                }

                if (prefabBase != null)
                {
                    m_Log.Debug($"{nameof(AnarchyPlopSystem)}.{nameof(OnGameLoadingComplete)} Removed PreventOverride from {prefabBase.name}");
                }

                EntityManager.RemoveComponent<PreventOverride>(entity);
            }

            entitiesWithComponent.Dispose();

            base.OnGameLoadingComplete(purpose, mode);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.activeTool.toolID == null || m_ToolSystem.actionMode.IsEditor())
            {
                return;
            }

            if (m_ToolSystem.activePrefab != null)
            {
                Entity prefabEntity = m_PrefabSystem.GetEntity(m_ToolSystem.activePrefab);
                if (EntityManager.HasComponent<BuildingData>(prefabEntity))
                {
                    return;
                }

                if (EntityManager.TryGetComponent(prefabEntity, out ObjectGeometryData objectGeometryData))
                {
                    if ((objectGeometryData.m_Flags & GeometryFlags.Overridable) != GeometryFlags.Overridable)
                    {
                        m_Log.Debug($"{nameof(AnarchyPlopSystem)}.{nameof(OnUpdate)} Active prefab is not overridable.");
                    }
                }
            }

            if (m_AnarchySystem.AnarchyEnabled && m_AppropriateTools.Contains(m_ToolSystem.activeTool.toolID) && !m_NetToolSystem.TrySetPrefab(m_ToolSystem.activePrefab))
            {
                EntityManager.RemoveComponent(m_CreatedQuery, ComponentType.ReadWrite<Overridden>());
                EntityManager.RemoveComponent(m_OwnedAndOverridenQuery, ComponentType.ReadWrite<Overridden>());

                NativeArray<Entity> createdEntities = m_CreatedQuery.ToEntityArray(Allocator.Temp);

                foreach (Entity entity in createdEntities)
                {
                    PrefabBase prefabBase = null;
                    if (EntityManager.TryGetComponent(entity, out PrefabRef prefabRef))
                    {
                        if (m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out prefabBase))
                        {
                            if (prefabBase is StaticObjectPrefab && EntityManager.TryGetComponent(prefabRef.m_Prefab, out ObjectGeometryData objectGeometryData))
                            {
                                if ((objectGeometryData.m_Flags & GeometryFlags.Overridable) == GeometryFlags.Overridable)
                                {
                                    m_Log.Debug($"{nameof(AnarchyPlopSystem)}.{nameof(OnUpdate)} Added PreventOverride to {prefabBase.name}");
                                    EntityManager.AddComponent<PreventOverride>(entity);
                                    continue;
                                }
                            }
                        }
                    }

                    if (prefabBase != null)
                    {
                        m_Log.Debug($"{nameof(AnarchyPlopSystem)}.{nameof(OnUpdate)} Would not add PreventOverride to {prefabBase.name}");
                    }
                }

                createdEntities.Dispose();
            }
        }
    }
}
