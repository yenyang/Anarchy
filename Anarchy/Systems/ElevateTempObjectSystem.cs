// <copyright file="TempObjectSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Simulation;
    using Game.Tools;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Overrides vertical position of temp objects.
    /// </summary>
    public partial class ElevateTempObjectSystem : GameSystemBase
    {
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private AnarchyUISystem m_AnarchyUISystem;
        private TerrainSystem m_TerrainSystem;
        private EntityQuery m_TempObjectQuery;
        private ModificationBarrier1 m_ModificationBarrier;
        private RaycastSystem m_RaycastSystem;
        private ILog m_Log;
        private float m_ElevationChange = 0f;

        /// <summary>
        /// Sets the elevation change for the system.
        /// </summary>
        public float ElevationChange { set { m_ElevationChange = value; } }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_RaycastSystem = World.GetOrCreateSystemManaged<RaycastSystem>();
            m_ModificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier1>();
            m_TerrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();
            m_AnarchyUISystem = World.CreateSystemManaged<AnarchyUISystem>();
            m_Log.Info($"[{nameof(ElevateTempObjectSystem)}] {nameof(OnCreate)}");
            Enabled = false;
            m_TempObjectQuery = SystemAPI.QueryBuilder()
                .WithAllRW<Game.Objects.Transform>()
                .WithAll<Temp, Game.Objects.Object, Game.Objects.Static, PrefabRef>()
                .WithNone<Deleted, Overridden, Updated>()
                .Build();

            RequireForUpdate(m_TempObjectQuery);
        }


        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.actionMode.IsEditor())
            {
                Enabled = false;
                return;
            }

            if ((m_ToolSystem.activeTool != m_ObjectToolSystem && m_ToolSystem.activeTool.toolID != "Line Tool") || !AnarchyMod.Instance.Settings.ShowElevationToolOption)
            {
                return;
            }

            if (m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Create && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush)
            {
                return;
            }

            NativeArray<Entity> entities = m_TempObjectQuery.ToEntityArray(Allocator.Temp);
            EntityCommandBuffer buffer = m_ModificationBarrier.CreateCommandBuffer();


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

                if (prefabBase is not BuildingPrefab && !EntityManager.HasComponent<StackData>(currentPrefabRef.m_Prefab))
                {
                    currentTransform.m_Position.y += m_ElevationChange;
                    buffer.SetComponent(entity, currentTransform);

                    TerrainHeightData terrainHeightData = m_TerrainSystem.GetHeightData();
                    float terrainHeight = TerrainUtils.SampleHeight(ref terrainHeightData, currentTransform.m_Position);
                    if (!EntityManager.HasComponent<Game.Objects.Elevation>(entity) && currentTransform.m_Position.y > terrainHeight && m_ElevationChange > 0f)
                    {
                        Game.Objects.Elevation elevation = new Game.Objects.Elevation()
                        {
                            m_Elevation = m_ElevationChange,
                            m_Flags = 0,
                        };
                        buffer.AddComponent<Game.Objects.Elevation>(entity);
                        buffer.SetComponent(entity, elevation);
                    }
                    else if (EntityManager.TryGetComponent<Game.Objects.Elevation>(entity, out Game.Objects.Elevation currentElevation) && currentTransform.m_Position.y > terrainHeight && currentElevation.m_Elevation + m_ElevationChange > 0f)
                    {
                        currentElevation.m_Elevation += m_ElevationChange;
                        buffer.SetComponent(entity, currentElevation);
                    }
                    else if ((currentElevation.m_Elevation + m_ElevationChange <= 0f || currentTransform.m_Position.y <= terrainHeight) && EntityManager.HasComponent<Game.Objects.Elevation>(entity))
                    {
                        buffer.RemoveComponent<Game.Objects.Elevation>(entity);
                    }

                    buffer.AddComponent<Updated>(entity);
                }

                Enabled = false;
            }

            entities.Dispose();
        }
    }
}
