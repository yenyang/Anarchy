// <copyright file="TempNetworkGradeSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Citizens;
    using Game.Common;
    using Game.Prefabs;
    using Game.Simulation;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    /// <summary>
    /// Overrides vertical position of creation definition.
    /// </summary>
    public partial class TempNetworkGradeSystem : GameSystemBase
    {
        private ToolSystem m_ToolSystem;
        private NetToolSystem m_NetToolSystem;
        private PrefabSystem m_PrefabSystem;
        private NetworkAnarchyUISystem m_UISystem;
        private EntityQuery m_TempEdgeCurveQuery;
        private TerrainSystem m_TerrainSystem;
        private ILog m_Log;

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_UISystem = World.GetOrCreateSystemManaged<NetworkAnarchyUISystem>();
            m_TerrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();
            m_ToolSystem.EventToolChanged += (ToolBaseSystem tool) => Enabled = tool == m_NetToolSystem;
            m_Log.Info($"[{nameof(TempNetworkGradeSystem)}] {nameof(OnCreate)}");
            m_TempEdgeCurveQuery = SystemAPI.QueryBuilder()
                .WithAllRW<Game.Net.Curve, Game.Net.Elevation>()
                .WithAll<Game.Net.Edge, Updated, Temp>()
                .WithNone<Deleted, Overridden>()
                .Build();

            RequireForUpdate(m_TempEdgeCurveQuery);
        }


        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            NativeArray<Entity> entities = m_TempEdgeCurveQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.ConstantSlope) == NetworkAnarchyUISystem.Composition.ConstantSlope 
                    && EntityManager.TryGetComponent(entity, out Game.Net.Curve curve))
                {
                    float length = Vector2.Distance(new float2(curve.m_Bezier.a.x, curve.m_Bezier.a.z), new float2(curve.m_Bezier.d.x, curve.m_Bezier.d.z));
                    float slope = (curve.m_Bezier.d.y - curve.m_Bezier.a.y) / length;
                    curve.m_Bezier.b.y = curve.m_Bezier.a.y + (slope * Vector2.Distance(new float2(curve.m_Bezier.a.x, curve.m_Bezier.a.z), new float2(curve.m_Bezier.b.x, curve.m_Bezier.b.z)));
                    curve.m_Bezier.c.y = curve.m_Bezier.a.y + (slope * Vector2.Distance(new float2(curve.m_Bezier.a.x, curve.m_Bezier.a.z), new float2(curve.m_Bezier.c.x, curve.m_Bezier.c.z)));
                    EntityManager.SetComponentData(entity, curve);

                    TerrainHeightData terrainHeightData = m_TerrainSystem.GetHeightData();
                    float terrainHeight = TerrainUtils.SampleHeight(ref terrainHeightData, (curve.m_Bezier.b + curve.m_Bezier.c) / 2f);
                    
                    // Need left and right terrain height.
                    if (EntityManager.TryGetComponent(entity, out Game.Net.Elevation elevation))
                    {
                        elevation.m_Elevation = new float2(5f, -5f);
                        EntityManager.SetComponentData(entity, elevation);
                    }
                }

                /*
                if (EntityManager.TryGetComponent(entity, out Game.Net.Elevation elevation))
                {
                    TerrainHeightData terrainHeightData = m_TerrainSystem.GetHeightData();
                    float terrainHeight = TerrainUtils.SampleHeight(ref terrainHeightData, currentTransform.m_Position);
                    elevation.m_Elevation = new float2(5f, -5f);
                    EntityManager.SetComponentData(entity, elevation);
                }

                if (EntityManager.tryget)*/
            }
        }

    }
}
