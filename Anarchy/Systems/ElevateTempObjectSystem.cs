// <copyright file="TempObjectSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Net;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Simulation;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Overrides vertical position of creation definition.
    /// </summary>
    public partial class ElevateTempObjectSystem : GameSystemBase
    {
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private AnarchyUISystem m_AnarchyUISystem;
        private TerrainSystem m_TerrainSystem;
        private EntityQuery m_TempObjectQuery;
        private RaycastSystem m_RaycastSystem;
        private ILog m_Log;
        private float m_ElevationChange = 0f;

        /// <summary>
        /// Sets the elevation change for the system;
        /// </summary>
        public float ElevationChange { set { m_ElevationChange = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElevateTempObjectSystem"/> class.
        /// </summary>
        public ElevateTempObjectSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_RaycastSystem = World.GetOrCreateSystemManaged<RaycastSystem>();
            m_TerrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();
            m_AnarchyUISystem = World.CreateSystemManaged<AnarchyUISystem>();
            m_Log.Info($"[{nameof(ElevateTempObjectSystem)}] {nameof(OnCreate)}");
            Enabled = false;
        }


        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            m_TempObjectQuery = SystemAPI.QueryBuilder()
                .WithAllRW<Game.Objects.Transform>()
                .WithAll<Temp, Game.Objects.Object, Game.Objects.Static, PrefabRef>()
                .WithNone<Deleted, Overridden>()
                .Build();

            RequireForUpdate(m_TempObjectQuery);

            if (m_ToolSystem.activeTool != m_ObjectToolSystem && m_ToolSystem.activeTool.toolID != "Line Tool")
            {
                return;
            }


            NativeArray<Entity> entities = m_TempObjectQuery.ToEntityArray(Allocator.Temp);

            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out Game.Objects.Transform currentTransform))
                {
                    continue;
                }

                if (!EntityManager.TryGetComponent(entity, out PrefabRef currentPrefabRef) || !m_PrefabSystem.TryGetPrefab(currentPrefabRef.m_Prefab, out PrefabBase prefabBase))
                {
                    continue;
                }



                if (prefabBase is not BuildingPrefab)
                {
                    currentTransform.m_Position.y += m_ElevationChange;
                    EntityManager.SetComponentData(entity, currentTransform);

                    TerrainHeightData terrainHeightData = m_TerrainSystem.GetHeightData();
                    float terrainHeight = TerrainUtils.SampleHeight(ref terrainHeightData, currentTransform.m_Position);
                    if (!EntityManager.HasComponent<Game.Objects.Elevation>(entity) && currentTransform.m_Position.y > terrainHeight)
                    {
                        Game.Objects.Elevation elevation = new Game.Objects.Elevation()
                        {
                            m_Elevation = m_ElevationChange,
                            m_Flags = 0,
                        };
                        EntityManager.AddComponent<Game.Objects.Elevation>(entity);
                        EntityManager.SetComponentData(entity, elevation);
                    }
                    else if (EntityManager.TryGetComponent<Game.Objects.Elevation>(entity, out Game.Objects.Elevation currentElevation) && currentTransform.m_Position.y > terrainHeight)
                    {
                        currentElevation.m_Elevation += m_ElevationChange;
                        EntityManager.SetComponentData(entity, currentElevation);
                    }
                    else if (currentTransform.m_Position.y <= terrainHeight && EntityManager.HasComponent<Game.Objects.Elevation>(entity))
                    {
                        EntityManager.RemoveComponent<Game.Objects.Elevation>(entity);
                    }

                    EntityManager.AddComponent<Updated>(entity);
                }

                Enabled = false;
            }

            entities.Dispose();
        }

    }
}
