// <copyright file="TempNetworkSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using System.Collections.Generic;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Net;
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
    public partial class TempNetworkSystem : GameSystemBase
    {
        private readonly Dictionary<NetworkAnarchyUISystem.SideUpgrades, CompositionFlags.Side> SideUpgradeLookup = new Dictionary<NetworkAnarchyUISystem.SideUpgrades, CompositionFlags.Side>()
        {
            { NetworkAnarchyUISystem.SideUpgrades.Quay, CompositionFlags.Side.Raised },
            { NetworkAnarchyUISystem.SideUpgrades.RetainingWall, CompositionFlags.Side.Lowered },
            { NetworkAnarchyUISystem.SideUpgrades.Trees, CompositionFlags.Side.SecondaryBeautification },
            { NetworkAnarchyUISystem.SideUpgrades.GrassStrip, CompositionFlags.Side.PrimaryBeautification },
            { NetworkAnarchyUISystem.SideUpgrades.WideSidewalk, CompositionFlags.Side.WideSidewalk },
            { NetworkAnarchyUISystem.SideUpgrades.SoundBarrier, CompositionFlags.Side.SoundBarrier },
            { NetworkAnarchyUISystem.SideUpgrades.Trees | NetworkAnarchyUISystem.SideUpgrades.GrassStrip, CompositionFlags.Side.PrimaryBeautification | CompositionFlags.Side.SecondaryBeautification },
            { NetworkAnarchyUISystem.SideUpgrades.WideSidewalk | NetworkAnarchyUISystem.SideUpgrades.Trees, CompositionFlags.Side.WideSidewalk | CompositionFlags.Side.SecondaryBeautification },
        };

        private readonly Dictionary<NetworkAnarchyUISystem.Composition, CompositionFlags.General> GeneralCompositionLookup = new Dictionary<NetworkAnarchyUISystem.Composition, CompositionFlags.General>()
        {
            { NetworkAnarchyUISystem.Composition.Elevated, CompositionFlags.General.Elevated },
            { NetworkAnarchyUISystem.Composition.Tunnel, CompositionFlags.General.Tunnel },
            { NetworkAnarchyUISystem.Composition.WideMedian, CompositionFlags.General.WideMedian },
            { NetworkAnarchyUISystem.Composition.Lighting, CompositionFlags.General.Lighting | CompositionFlags.General.WideMedian},
            { NetworkAnarchyUISystem.Composition.GrassStrip, CompositionFlags.General.PrimaryMiddleBeautification },
            { NetworkAnarchyUISystem.Composition.Trees, CompositionFlags.General.SecondaryMiddleBeautification },
            { NetworkAnarchyUISystem.Composition.Trees | NetworkAnarchyUISystem.Composition.GrassStrip, CompositionFlags.General.SecondaryMiddleBeautification | CompositionFlags.General.PrimaryMiddleBeautification },
        };

        private ToolSystem m_ToolSystem;
        private NetToolSystem m_NetToolSystem;
        private PrefabSystem m_PrefabSystem;
        private NetworkAnarchyUISystem m_UISystem;
        private EntityQuery m_TempEdgeCurveQuery;
        private TerrainSystem m_TerrainSystem;
        private ILog m_Log;

        /// <summary>
        /// Gets the sideUpgradeLookup.
        /// </summary>
        public Dictionary<NetworkAnarchyUISystem.SideUpgrades, CompositionFlags.Side> SideUpgradesDictionary
        {
            get { return SideUpgradeLookup; }
        }

        /// <summary>
        /// Gets the sideUpgradeLookup.
        /// </summary>
        public Dictionary<NetworkAnarchyUISystem.Composition, CompositionFlags.General> GeneralCompositionDictionary
        {
            get { return GeneralCompositionLookup; }
        }


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
            m_Log.Info($"[{nameof(TempNetworkSystem)}] {nameof(OnCreate)}");
            m_TempEdgeCurveQuery = SystemAPI.QueryBuilder()
                .WithAll<Updated, Temp>()
                .WithAny<Game.Net.Edge, Game.Net.Node>()
                .WithNone<Deleted, Overridden, Game.Net.Upgraded>()
                .Build();

            RequireForUpdate(m_TempEdgeCurveQuery);
        }


        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_UISystem.NetworkComposition == NetworkAnarchyUISystem.Composition.None
                && m_UISystem.LeftUpgrade == NetworkAnarchyUISystem.SideUpgrades.None
                && m_UISystem.RightUpgrade == NetworkAnarchyUISystem.SideUpgrades.None)
            {
                return;
            }

            NativeArray<Entity> entities = m_TempEdgeCurveQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (((m_UISystem.LeftUpgrade & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall
                    || (m_UISystem.LeftUpgrade & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay
                    || (m_UISystem.RightUpgrade & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall
                    || (m_UISystem.RightUpgrade & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay)
                    && EntityManager.HasComponent<Game.Net.Node>(entity))
                {
                    if (!EntityManager.TryGetComponent(entity, out Game.Net.Elevation elevation))
                    {
                        EntityManager.AddComponent<Elevation>(entity);
                    }

                    if ((m_UISystem.LeftUpgrade & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall)
                    {
                        elevation.m_Elevation.x = Mathf.Min(elevation.m_Elevation.x, m_NetToolSystem.elevation, NetworkDefinitionSystem.RetainingWallThreshold);
                    }
                    else if ((m_UISystem.LeftUpgrade & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay)
                    {
                        elevation.m_Elevation.x = Mathf.Max(elevation.m_Elevation.x, m_NetToolSystem.elevation, NetworkDefinitionSystem.QuayThreshold);
                    }

                    if ((m_UISystem.RightUpgrade & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall)
                    {
                        elevation.m_Elevation.y = Mathf.Min(elevation.m_Elevation.y, m_NetToolSystem.elevation, NetworkDefinitionSystem.RetainingWallThreshold);
                    }
                    else if ((m_UISystem.RightUpgrade & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay)
                    {
                        elevation.m_Elevation.y = Mathf.Max(elevation.m_Elevation.y, m_NetToolSystem.elevation, NetworkDefinitionSystem.QuayThreshold);
                    }

                    EntityManager.SetComponentData(entity, elevation);
                }

                CompositionFlags compositionFlags = default;
                compositionFlags.m_General = GetCompositionGeneralFlags();

                if (SideUpgradeLookup.ContainsKey(m_UISystem.LeftUpgrade))
                {
                    compositionFlags.m_Left = SideUpgradeLookup[m_UISystem.LeftUpgrade];
                }

                if (SideUpgradeLookup.ContainsKey(m_UISystem.RightUpgrade))
                {
                    compositionFlags.m_Right = SideUpgradeLookup[m_UISystem.RightUpgrade];
                }


                Game.Net.Upgraded upgrades = new Game.Net.Upgraded()
                {
                    m_Flags = compositionFlags,
                };

                EntityManager.AddComponent<Game.Net.Upgraded>(entity);
                EntityManager.SetComponentData(entity, upgrades);
            }
        }

        /// <summary>
        /// Gets the composition general flags.
        /// </summary>
        /// <returns>Compsoition General flags.</returns>
        private CompositionFlags.General GetCompositionGeneralFlags()
        {
            NetworkAnarchyUISystem.Composition composition = m_UISystem.NetworkComposition;
            composition &= ~NetworkAnarchyUISystem.Composition.ConstantSlope;
            composition &= ~NetworkAnarchyUISystem.Composition.Ground;

            if (GeneralCompositionLookup.ContainsKey(composition))
            {
                return GeneralCompositionLookup[composition];
            }

            return 0;
        }
    }
}
