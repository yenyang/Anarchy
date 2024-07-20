// <copyright file="TempNetworkGradeSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Prefabs;
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
        private AnarchyUISystem m_AnarchyUISystem;
        private EntityQuery m_TempEdgeCurveQuery;
        private ILog m_Log;

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_AnarchyUISystem = World.CreateSystemManaged<AnarchyUISystem>();
            m_Log.Info($"[{nameof(TempNetworkGradeSystem)}] {nameof(OnCreate)}");
        }


        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            m_TempEdgeCurveQuery = SystemAPI.QueryBuilder()
                .WithAllRW<Game.Net.Curve, Game.Net.Elevation>()
                .WithAll<Game.Net.Edge, Updated, Temp>()
                .WithNone<Deleted, Overridden>()
                .Build();

            RequireForUpdate(m_TempEdgeCurveQuery);

            if (m_ToolSystem.activeTool != m_NetToolSystem)
            {
                return;
            }

            NativeArray<Entity> entities = m_TempEdgeCurveQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (EntityManager.TryGetComponent(entity, out Game.Net.Curve curve))
                {
                    float length = Vector2.Distance(new float2(curve.m_Bezier.a.x, curve.m_Bezier.a.z), new float2(curve.m_Bezier.d.x, curve.m_Bezier.d.z));
                    float slope = (curve.m_Bezier.d.y - curve.m_Bezier.a.y) / length;
                    curve.m_Bezier.b.y = curve.m_Bezier.a.y + (slope * Vector2.Distance(new float2(curve.m_Bezier.a.x, curve.m_Bezier.a.z), new float2(curve.m_Bezier.b.x, curve.m_Bezier.b.z)));
                    curve.m_Bezier.c.y = curve.m_Bezier.a.y + (slope * Vector2.Distance(new float2(curve.m_Bezier.a.x, curve.m_Bezier.a.z), new float2(curve.m_Bezier.c.x, curve.m_Bezier.c.z)));
                    EntityManager.SetComponentData(entity, curve);
                }

                if (EntityManager.TryGetComponent(entity, out Game.Net.Elevation elevation))
                {
                    elevation.m_Elevation = new float2(5f, -5f);
                    EntityManager.SetComponentData(entity, elevation);
                }
            }
        }

    }
}
