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
                .WithAll<Updated, Temp, Game.Net.Edge>()
                .WithNone<Deleted, Overridden, Game.Net.Upgraded>()
                .Build();

            RequireForUpdate(m_TempEdgeCurveQuery);
        }


        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Elevated) != NetworkAnarchyUISystem.Composition.Elevated
                && (m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Tunnel) != NetworkAnarchyUISystem.Composition.Tunnel
                && m_UISystem.LeftUpgrade == NetworkAnarchyUISystem.SideUpgrades.None
                && m_UISystem.RightUpgrade == NetworkAnarchyUISystem.SideUpgrades.None)
            {
                return;
            }

            NativeArray<Entity> entities = m_TempEdgeCurveQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                /*
                if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.ConstantSlope) == NetworkAnarchyUISystem.Composition.ConstantSlope
                    && EntityManager.TryGetComponent(entity, out Game.Net.Curve curve))
                {
                    float length = Vector2.Distance(new float2(curve.m_Bezier.a.x, curve.m_Bezier.a.z), new float2(curve.m_Bezier.d.x, curve.m_Bezier.d.z));
                    float slope = (curve.m_Bezier.d.y - curve.m_Bezier.a.y) / length;
                    curve.m_Bezier.b.y = curve.m_Bezier.a.y + (slope * Vector2.Distance(new float2(curve.m_Bezier.a.x, curve.m_Bezier.a.z), new float2(curve.m_Bezier.b.x, curve.m_Bezier.b.z)));
                    curve.m_Bezier.c.y = curve.m_Bezier.a.y + (slope * Vector2.Distance(new float2(curve.m_Bezier.a.x, curve.m_Bezier.a.z), new float2(curve.m_Bezier.c.x, curve.m_Bezier.c.z)));
                    EntityManager.SetComponentData(entity, curve);
                    // Need left and right terrain height.
                    if (EntityManager.TryGetComponent(entity, out Game.Net.Elevation elevation))
                    {
                        elevation.m_Elevation = new float2(5f, -5f);
                        EntityManager.SetComponentData(entity, elevation);
                    }
                }
                */
                CompositionFlags compositionFlags = default;
                if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Elevated) == NetworkAnarchyUISystem.Composition.Elevated)
                {
                    compositionFlags.m_General |= CompositionFlags.General.Elevated;
                }
                else if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Tunnel) == NetworkAnarchyUISystem.Composition.Tunnel)
                {
                    compositionFlags.m_General |= CompositionFlags.General.Tunnel;
                }

                if ((m_UISystem.LeftUpgrade & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay)
                {
                    compositionFlags.m_Left |= CompositionFlags.Side.Raised;
                }
                else if ((m_UISystem.LeftUpgrade & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall)
                {
                    compositionFlags.m_Left |= CompositionFlags.Side.Lowered;
                }

                if ((m_UISystem.RightUpgrade & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay)
                {
                    compositionFlags.m_Right |= CompositionFlags.Side.Raised;
                }
                else if ((m_UISystem.RightUpgrade & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall)
                {
                    compositionFlags.m_Right |= CompositionFlags.Side.Lowered;
                }

                if (compositionFlags.m_General == 0 && compositionFlags.m_Left == 0 && compositionFlags.m_Right == 0)
                {
                    continue;
                }

                Game.Net.Upgraded upgrades = new Game.Net.Upgraded()
                {
                    m_Flags = compositionFlags,
                };

                EntityManager.AddComponent<Game.Net.Upgraded>(entity);
                EntityManager.SetComponentData(entity, upgrades);
            }
        }

    }
}
