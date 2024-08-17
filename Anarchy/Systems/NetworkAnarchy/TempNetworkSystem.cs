// <copyright file="TempNetworkSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.NetworkAnarchy
{
    using System.Collections.Generic;
    using Anarchy.Systems.NetworkAnarchy;
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
    using UnityEngine;

    /// <summary>
    /// Applies upgrades and/or elevation to Temp networks.
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
            { NetworkAnarchyUISystem.Composition.Lighting, CompositionFlags.General.Lighting },
            { NetworkAnarchyUISystem.Composition.GrassStrip, CompositionFlags.General.PrimaryMiddleBeautification },
            { NetworkAnarchyUISystem.Composition.Trees, CompositionFlags.General.SecondaryMiddleBeautification },
            { NetworkAnarchyUISystem.Composition.Trees | NetworkAnarchyUISystem.Composition.GrassStrip, CompositionFlags.General.SecondaryMiddleBeautification | CompositionFlags.General.PrimaryMiddleBeautification },
        };

        private ToolSystem m_ToolSystem;
        private NetToolSystem m_NetToolSystem;
        private PrefabSystem m_PrefabSystem;
        private NetworkAnarchyUISystem m_UISystem;
        private EntityQuery m_TempNetworksQuery;
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
            m_TempNetworksQuery = SystemAPI.QueryBuilder()
                .WithAll<Updated, Temp, Game.Net.Edge>()
                .WithNone<Deleted, Overridden>()
                .Build();

            RequireForUpdate(m_TempNetworksQuery);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_UISystem.NetworkComposition == NetworkAnarchyUISystem.Composition.None
                && m_UISystem.LeftUpgrade == NetworkAnarchyUISystem.SideUpgrades.None
                && m_UISystem.RightUpgrade == NetworkAnarchyUISystem.SideUpgrades.None
                && m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace)
            {
                return;
            }

            NativeArray<Entity> entities = m_TempNetworksQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (EntityManager.TryGetComponent(entity, out Temp temp))
                {
                    if ((m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace && (temp.m_Original != Entity.Null || (temp.m_Flags & TempFlags.Create) != TempFlags.Create))
                        || (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace && (temp.m_Flags & TempFlags.Essential) != TempFlags.Essential))
                    {
                        continue;
                    }
                }

                if (m_ToolSystem.activePrefab == null || !m_PrefabSystem.TryGetEntity(m_ToolSystem.activePrefab, out Entity prefabEntity) || !EntityManager.TryGetComponent(prefabEntity, out PlaceableNetData placeableNetData))
                {
                    continue;
                }

                if ((placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.IsUpgrade) == Game.Net.PlacementFlags.IsUpgrade
                    || (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.UpgradeOnly) == Game.Net.PlacementFlags.UpgradeOnly)
                {
                    continue;
                }

                if (((m_UISystem.LeftUpgrade & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall
                    || (m_UISystem.LeftUpgrade & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay
                    || (m_UISystem.RightUpgrade & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall
                    || (m_UISystem.RightUpgrade & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay)
                    && (temp.m_Flags != TempFlags.IsLast))
                {
                    if (!EntityManager.TryGetComponent(entity, out Game.Net.Elevation elevation))
                    {
                        EntityManager.AddComponent<Elevation>(entity);
                    }

                    if ((m_UISystem.RightUpgrade & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall && EntityManager.HasComponent<Game.Net.Edge>(entity))
                    {
                        elevation.m_Elevation.y = Mathf.Min(elevation.m_Elevation.y, m_NetToolSystem.elevation, NetworkDefinitionSystem.RetainingWallThreshold);
                    }
                    else if ((m_UISystem.RightUpgrade & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay && EntityManager.HasComponent<Game.Net.Edge>(entity))
                    {
                        elevation.m_Elevation.y = Mathf.Max(elevation.m_Elevation.y, m_NetToolSystem.elevation, NetworkDefinitionSystem.QuayThreshold);
                    }

                    if ((m_UISystem.LeftUpgrade & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall && EntityManager.HasComponent<Game.Net.Edge>(entity))
                    {
                        elevation.m_Elevation.x = Mathf.Min(elevation.m_Elevation.x, m_NetToolSystem.elevation, NetworkDefinitionSystem.RetainingWallThreshold);
                    }
                    else if ((m_UISystem.LeftUpgrade & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay && EntityManager.HasComponent<Game.Net.Edge>(entity))
                    {
                        elevation.m_Elevation.x = Mathf.Max(elevation.m_Elevation.x, m_NetToolSystem.elevation, NetworkDefinitionSystem.QuayThreshold);
                    }

                    EntityManager.SetComponentData(entity, elevation);
                }
                else if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace && EntityManager.HasComponent<Elevation>(entity))
                {
                    EntityManager.RemoveComponent<Elevation>(entity);
                    m_Log.Debug("Removed Elevation");
                    temp.m_Flags |= TempFlags.Replace;
                    EntityManager.SetComponentData(entity, temp);
                    m_Log.Debug($"{nameof(TempNetworkSystem)}{nameof(OnUpdate)} added replace to temp.");
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

                if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace)
                {
                    temp.m_Flags |= TempFlags.Upgrade | TempFlags.Parent;
                    EntityManager.SetComponentData(entity, temp);

                    m_Log.Debug($"{nameof(TempNetworkSystem)}{nameof(OnUpdate)} modified temp.");
                }


                if (compositionFlags.m_General == 0 && compositionFlags.m_Left == 0 && compositionFlags.m_Right == 0)
                {
                    if (EntityManager.HasComponent<Game.Net.Upgraded>(entity))
                    {
                        EntityManager.RemoveComponent<Game.Net.Upgraded>(entity);
                        m_Log.Debug($"{nameof(TempNetworkSystem)}{nameof(OnUpdate)} removed.");
                    }

                    continue;
                }

                Game.Net.Upgraded upgrades = new Game.Net.Upgraded()
                {
                    m_Flags = compositionFlags,
                };

                if (!EntityManager.HasComponent<Game.Net.Upgraded>(entity))
                {
                    EntityManager.AddComponent<Game.Net.Upgraded>(entity);
                }

                EntityManager.SetComponentData(entity, upgrades);
                m_Log.Debug($"{nameof(TempNetworkSystem)}{nameof(OnUpdate)} upgraded.");
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
