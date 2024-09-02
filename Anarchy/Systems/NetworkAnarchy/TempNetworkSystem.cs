﻿// <copyright file="TempNetworkSystem.cs" company="Yenyang's Mods. MIT License">
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
            { NetworkAnarchyUISystem.SideUpgrades.None, 0 },
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
            { NetworkAnarchyUISystem.Composition.None, 0 },
            { NetworkAnarchyUISystem.Composition.Elevated, CompositionFlags.General.Elevated },
            { NetworkAnarchyUISystem.Composition.Tunnel, CompositionFlags.General.Tunnel },
            { NetworkAnarchyUISystem.Composition.WideMedian, CompositionFlags.General.WideMedian },
            { NetworkAnarchyUISystem.Composition.Lighting, CompositionFlags.General.Lighting },
            { NetworkAnarchyUISystem.Composition.GrassStrip, CompositionFlags.General.PrimaryMiddleBeautification },
            { NetworkAnarchyUISystem.Composition.Trees, CompositionFlags.General.SecondaryMiddleBeautification },
            { NetworkAnarchyUISystem.Composition.Trees | NetworkAnarchyUISystem.Composition.GrassStrip, CompositionFlags.General.SecondaryMiddleBeautification | CompositionFlags.General.PrimaryMiddleBeautification },
        };

        private readonly List<PrefabID> UpgradeLookup = new List<PrefabID>()
        {
             new PrefabID("FencePrefab", "RetainingWall"),
             new PrefabID("FencePrefab", "RetainingWall01"),
             new PrefabID("FencePrefab", "Quay"),
             new PrefabID("FencePrefab", "Quay01"),
             new PrefabID("FencePrefab", "Elevated"),
             new PrefabID("FencePrefab", "Elevated01"),
             new PrefabID("FencePrefab", "Tunnel"),
             new PrefabID("FencePrefab", "Tunnel01"),
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

        /// <summary>
        /// Gets the list of upgrade prefab IDs.
        /// </summary>
        public List<PrefabID> UpgradePrefabIDs
        {
            get { return UpgradeLookup; }
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
                .WithNone<Deleted, Overridden, Owner>()
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

                if (((placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.IsUpgrade) == Game.Net.PlacementFlags.IsUpgrade
                    || (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.UpgradeOnly) == Game.Net.PlacementFlags.UpgradeOnly)
                    && !UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()))
                {
                    continue;
                }

                // This is a somewhat roundabout way to adapt Extended Road upgrades method of applying upgrades to the way Network Anarchy applies upgrades.
                NetworkAnarchyUISystem.SideUpgrades effectiveLeftUpgrades = m_UISystem.LeftUpgrade;
                NetworkAnarchyUISystem.SideUpgrades effectiveRightUpgrades = m_UISystem.RightUpgrade;
                NetworkAnarchyUISystem.Composition effectiveComposition = m_UISystem.NetworkComposition;                
                if (UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()) && EntityManager.TryGetComponent(entity, out Upgraded upgraded))
                {
                    if ((upgraded.m_Flags.m_Left & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised)
                    {
                        effectiveLeftUpgrades |= NetworkAnarchyUISystem.SideUpgrades.Quay;
                    }
                    else if ((upgraded.m_Flags.m_Left & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered)
                    {
                        effectiveLeftUpgrades |= NetworkAnarchyUISystem.SideUpgrades.RetainingWall;
                    }

                    if ((upgraded.m_Flags.m_Right & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised)
                    {
                        effectiveRightUpgrades |= NetworkAnarchyUISystem.SideUpgrades.Quay;
                    }
                    else if ((upgraded.m_Flags.m_Right & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered)
                    {
                        effectiveRightUpgrades |= NetworkAnarchyUISystem.SideUpgrades.RetainingWall;
                    }

                    if ((upgraded.m_Flags.m_General & CompositionFlags.General.Elevated) == CompositionFlags.General.Elevated)
                    {
                        effectiveComposition |= NetworkAnarchyUISystem.Composition.Elevated;
                    } else if ((upgraded.m_Flags.m_General & CompositionFlags.General.Tunnel) == CompositionFlags.General.Tunnel)
                    {
                        effectiveComposition |= NetworkAnarchyUISystem.Composition.Tunnel;
                    }
                }

                if (((effectiveLeftUpgrades & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall
                    || (effectiveLeftUpgrades & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay
                    || (effectiveRightUpgrades & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall
                    || (effectiveRightUpgrades & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay)
                    && (temp.m_Flags != TempFlags.IsLast))
                {
                    if (!EntityManager.TryGetComponent(entity, out Game.Net.Elevation elevation))
                    {
                        EntityManager.AddComponent<Elevation>(entity);
                    }

                    if ((effectiveRightUpgrades & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall)
                    {
                        elevation.m_Elevation.y = Mathf.Min(elevation.m_Elevation.y, m_NetToolSystem.elevation, NetworkDefinitionSystem.RetainingWallThreshold);
                    }
                    else if ((effectiveRightUpgrades & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay)
                    {
                        elevation.m_Elevation.y = Mathf.Max(elevation.m_Elevation.y, m_NetToolSystem.elevation, NetworkDefinitionSystem.QuayThreshold);
                    }
                    else if (elevation.m_Elevation.y == NetworkDefinitionSystem.QuayThreshold || elevation.m_Elevation.y == NetworkDefinitionSystem.RetainingWallThreshold)
                    {
                        elevation.m_Elevation.y = 0;
                    }

                    if ((effectiveLeftUpgrades & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall)
                    {
                        elevation.m_Elevation.x = Mathf.Min(elevation.m_Elevation.x, m_NetToolSystem.elevation, NetworkDefinitionSystem.RetainingWallThreshold);
                    }
                    else if ((effectiveLeftUpgrades & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay)
                    {
                        elevation.m_Elevation.x = Mathf.Max(elevation.m_Elevation.x, m_NetToolSystem.elevation, NetworkDefinitionSystem.QuayThreshold);
                    }
                    else if (elevation.m_Elevation.x == NetworkDefinitionSystem.QuayThreshold || elevation.m_Elevation.x == NetworkDefinitionSystem.RetainingWallThreshold)
                    {
                        elevation.m_Elevation.x = 0;
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

                    if ((effectiveComposition & NetworkAnarchyUISystem.Composition.Tunnel) != NetworkAnarchyUISystem.Composition.Tunnel && EntityManager.TryGetComponent(entity, out Edge edge))
                    {
                        EntityManager.RemoveComponent<Elevation>(edge.m_Start);
                        EntityManager.RemoveComponent<Elevation>(edge.m_End);
                    }
                }

                CompositionFlags compositionFlags = default;

                if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace && EntityManager.TryGetComponent(entity, out Upgraded currentUpgrades))
                {
                    compositionFlags = currentUpgrades.m_Flags;
                    m_Log.Debug($"{nameof(TempNetworkSystem)}.{nameof(OnUpdate)} Replace Upgraded General = {compositionFlags.m_General} Left = {compositionFlags.m_Left} Right = {compositionFlags.m_Right}");
                }

                if (m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace || m_UISystem.ReplaceComposition)
                {
                    compositionFlags.m_General = GetCompositionGeneralFlags(effectiveComposition);
                }

                if (SideUpgradeLookup.ContainsKey(effectiveLeftUpgrades) && (m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace || m_UISystem.ReplaceLeftUpgrade))
                {
                    compositionFlags.m_Left = SideUpgradeLookup[effectiveLeftUpgrades];
                    m_Log.Debug($"{nameof(TempNetworkSystem)}.{nameof(OnUpdate)} m_NetToolSystem.actualMode = {m_NetToolSystem.actualMode} m_UISystem.ReplaceLeftUpgrade {m_UISystem.ReplaceLeftUpgrade}");
                }

                if (SideUpgradeLookup.ContainsKey(effectiveRightUpgrades) && (m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace || m_UISystem.ReplaceRightUpgrade))
                {
                    compositionFlags.m_Right = SideUpgradeLookup[effectiveRightUpgrades];
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
        private CompositionFlags.General GetCompositionGeneralFlags(NetworkAnarchyUISystem.Composition composition)
        {
            composition &= ~NetworkAnarchyUISystem.Composition.ConstantSlope;
            composition &= ~NetworkAnarchyUISystem.Composition.Ground;

            if (GeneralCompositionLookup.ContainsKey(composition))
            {
                return GeneralCompositionLookup[composition];
            }

            return 0;
        }

        private bool IsElevationForced(Elevation elevation)
        {
            if ((elevation.m_Elevation.x == NetworkDefinitionSystem.ElevatedThreshold && elevation.m_Elevation.y == NetworkDefinitionSystem.ElevatedThreshold) ||
                elevation.m_Elevation.x == NetworkDefinitionSystem.RetainingWallThreshold || elevation.m_Elevation.y == NetworkDefinitionSystem.RetainingWallThreshold ||
                elevation.m_Elevation.x == NetworkDefinitionSystem.QuayThreshold || elevation.m_Elevation.y == NetworkDefinitionSystem.QuayThreshold ||
                (elevation.m_Elevation.x == NetworkDefinitionSystem.TunnelThreshold && elevation.m_Elevation.y == NetworkDefinitionSystem.TunnelThreshold))
            {
                return true;
            }

            return false;
        }
    }
}
