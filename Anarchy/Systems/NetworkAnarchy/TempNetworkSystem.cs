﻿// <copyright file="TempNetworkSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.NetworkAnarchy
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Anarchy.Components;
    using Anarchy.Extensions;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Input;
    using Game.Net;
    using Game.Prefabs;
    using Game.Simulation;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Entities.UniversalDelegates;
    using Unity.Jobs;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using static Game.Rendering.OverlayRenderSystem;
    using static Game.Tools.NetToolSystem;
    using static Unity.Burst.Intrinsics.X86.Avx;

    /// <summary>
    /// Applies upgrades and/or elevation to Temp networks.
    /// </summary>
    public partial class TempNetworkSystem : GameSystemBase
    {
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

            // { NetworkAnarchyUISystem.Composition.Lighting, CompositionFlags.General.Lighting },
            { NetworkAnarchyUISystem.Composition.GrassStrip, CompositionFlags.General.PrimaryMiddleBeautification },
            { NetworkAnarchyUISystem.Composition.Trees, CompositionFlags.General.SecondaryMiddleBeautification },
            { NetworkAnarchyUISystem.Composition.Trees | NetworkAnarchyUISystem.Composition.GrassStrip, CompositionFlags.General.SecondaryMiddleBeautification | CompositionFlags.General.PrimaryMiddleBeautification },
            { NetworkAnarchyUISystem.Composition.Trees | NetworkAnarchyUISystem.Composition.WideMedian, CompositionFlags.General.SecondaryMiddleBeautification | CompositionFlags.General.WideMedian },
        };

        private ToolSystem m_ToolSystem;
        private NetToolSystem m_NetToolSystem;
        private PrefabSystem m_PrefabSystem;
        private NetworkAnarchyUISystem m_UISystem;
        private EntityQuery m_TempNetworksQuery;
        private TerrainSystem m_TerrainSystem;
        private ProxyAction m_SecondaryApplyMimic;
        private ToolRaycastSystem m_ToolRaycastSystem;
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
            m_ToolRaycastSystem = World.GetOrCreateSystemManaged<ToolRaycastSystem>();
            m_ToolSystem.EventToolChanged += (ToolBaseSystem tool) =>
            {
                Enabled = tool == m_NetToolSystem;
                m_SecondaryApplyMimic.shouldBeEnabled = tool == m_NetToolSystem;
            };
            m_Log.Info($"[{nameof(TempNetworkSystem)}] {nameof(OnCreate)}");
            m_TempNetworksQuery = SystemAPI.QueryBuilder()
                .WithAllRW<Temp>()
                .WithAll<Updated, Game.Net.Edge>()
                .WithNone<Deleted, Overridden, Owner>()
                .Build();

            RequireForUpdate(m_TempNetworksQuery);

            m_SecondaryApplyMimic = AnarchyMod.Instance.Settings.GetAction(AnarchyMod.SecondaryMimicAction);
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

            if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace
                && (!m_UISystem.ReplaceComposition || !AnarchyMod.Instance.Settings.NetworkAnarchyToolOptions)
                && (!m_UISystem.ReplaceLeftUpgrade || !AnarchyMod.Instance.Settings.NetworkUpgradesToolOptions)
                && (!m_UISystem.ReplaceRightUpgrade || !AnarchyMod.Instance.Settings.NetworkUpgradesToolOptions)
                && !UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()))
            {
                return;
            }

            if (m_ToolSystem.activePrefab == null ||
               !m_PrefabSystem.TryGetEntity(m_ToolSystem.activePrefab, out Entity prefabEntity) ||
               !EntityManager.TryGetComponent(prefabEntity, out PlaceableNetData placeableNetData) ||
               !EntityManager.TryGetComponent(prefabEntity, out NetData prefabNetData))
            {
                return;
            }

            if ((placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.IsUpgrade) == Game.Net.PlacementFlags.IsUpgrade
                && (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.UpgradeOnly) == Game.Net.PlacementFlags.UpgradeOnly
                && !UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()))
            {
                return;
            }


            if ((m_SecondaryApplyMimic.IsPressed() || m_SecondaryApplyMimic.WasPerformedThisFrame())
                && (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.IsUpgrade) != Game.Net.PlacementFlags.IsUpgrade)
            {
                m_Log.Debug($"{nameof(TempNetworkSystem)}.{nameof(OnUpdate)} secondary apply was pressed. return early.");
                return;
            }

            m_Log.Debug($"{nameof(TempNetworkSystem)}.{nameof(OnUpdate)} Creating Path.");
            ComponentLookup<Edge> edgeLookup = SystemAPI.GetComponentLookup<Edge>();
            ComponentLookup<Node> nodeLookup = SystemAPI.GetComponentLookup<Node>();
            ComponentLookup<Curve> curveLookup = SystemAPI.GetComponentLookup<Curve>();
            ComponentLookup<PrefabRef> prefabRefLookup = SystemAPI.GetComponentLookup<PrefabRef>();
            ComponentLookup<NetData> netDataLookup = SystemAPI.GetComponentLookup<NetData>();
            BufferLookup<ConnectedEdge> connectedEdgeLookup = SystemAPI.GetBufferLookup<ConnectedEdge>();
            NativeList<ControlPoint> controlPoints = m_NetToolSystem.GetControlPoints(out JobHandle dependencies);
            dependencies.Complete();
            if (controlPoints.Length == 0 && m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace)
            {
                return;
            }

            ControlPoint startPoint = controlPoints[0];
            ControlPoint endPoint = controlPoints[controlPoints.Length - 1];
            NativeList<PathEdge> path = new NativeList<PathEdge>(Allocator.Temp);
            CreatePath(startPoint, endPoint, path, prefabNetData, placeableNetData, ref edgeLookup, ref nodeLookup, ref curveLookup, ref prefabRefLookup, ref netDataLookup, ref connectedEdgeLookup);
            NativeList<Entity> pathEntities = new NativeList<Entity>(Allocator.Temp);
            for (int i = 0; i < path.Length; i++)
            {
                pathEntities.Add(path[i].m_Entity);
                m_Log.Debug($"{nameof(TempNetworkSystem)}.{nameof(OnUpdate)} {path[i].m_Entity.Index}:{path[i].m_Entity.Version}.");
            }

            m_Log.Debug($"{nameof(TempNetworkSystem)}.{nameof(OnUpdate)} path completed.");

            NativeArray<Entity> entities = m_TempNetworksQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                TempFlags originalTempFlags = 0;
                Entity entity = entities[i];
                if (EntityManager.TryGetComponent(entity, out Temp temp))
                {
                    originalTempFlags = temp.m_Flags;
                    if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace &&
                        temp.m_Original != Entity.Null &&
                       (pathEntities.Contains(entity) ||
                        pathEntities.Contains(temp.m_Original)) &&
                       (UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()) ||
                       (EntityManager.TryGetComponent(temp.m_Original, out PrefabRef prefabRef) &&
                        prefabRef == prefabEntity)))
                    {
                        temp.m_Flags |= TempFlags.Modify;
                        EntityManager.SetComponentData(entity, temp);
                    }

                    if (m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace &&
                       (temp.m_Original != Entity.Null ||
                       (temp.m_Flags & TempFlags.Create) != TempFlags.Create))
                    {
                        continue;
                    }

                    if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace &&
                        (temp.m_Flags & TempFlags.Modify) != TempFlags.Modify)
                    {
                        if (EntityManager.HasComponent<Game.Net.Elevation>(entity) &&
                           !EntityManager.HasComponent<Game.Net.Elevation>(temp.m_Original))
                        {
                            EntityManager.RemoveComponent<Game.Net.Elevation>(entity);
                            m_Log.Debug($"{nameof(TempNetworkSystem)}.{nameof(OnUpdate)} removed elevation component.");
                        }

                        continue;
                    }
                }
                else
                {
                    continue;
                }

                bool changedElevation = false;
                EntityManager.TryGetComponent(entity, out Elevation originalElevation);
                EntityManager.TryGetComponent(entity, out Upgraded originalUpgraded);

                // This is a somewhat roundabout way to adapt Extended Road upgrades method of applying upgrades to the way Network Anarchy applies upgrades.
                NetworkAnarchyUISystem.SideUpgrades effectiveLeftUpgrades = m_UISystem.LeftUpgrade;
                NetworkAnarchyUISystem.SideUpgrades effectiveRightUpgrades = m_UISystem.RightUpgrade;
                NetworkAnarchyUISystem.Composition effectiveComposition = m_UISystem.NetworkComposition;
                if (UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()) &&
                    EntityManager.TryGetComponent(entity, out Upgraded upgraded))
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
                        effectiveLeftUpgrades &= ~(NetworkAnarchyUISystem.SideUpgrades.RetainingWall | NetworkAnarchyUISystem.SideUpgrades.Quay);
                        effectiveRightUpgrades &= ~(NetworkAnarchyUISystem.SideUpgrades.RetainingWall | NetworkAnarchyUISystem.SideUpgrades.Quay);
                    }
                    else if ((upgraded.m_Flags.m_General & CompositionFlags.General.Tunnel) == CompositionFlags.General.Tunnel)
                    {
                        effectiveComposition |= NetworkAnarchyUISystem.Composition.Tunnel;
                    }
                }

                if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace
                    && EntityManager.TryGetComponent(entity, out Elevation segmentElevation)
                    && EvaluateCompositionUpgradesAndToolOptions(entity, segmentElevation))
                {
                    EntityManager.RemoveComponent<Elevation>(entity);
                    m_Log.Debug("Removed Elevation");
                    temp.m_Flags |= TempFlags.Replace;
                    EntityManager.SetComponentData(entity, temp);
                    m_Log.Debug($"{nameof(TempNetworkSystem)}{nameof(OnUpdate)} added replace to temp.");
                }

                if (EntityManager.TryGetComponent(entity, out Edge edge)
                    && (((m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace) &&
                        ((effectiveComposition & NetworkAnarchyUISystem.Composition.Ground) == NetworkAnarchyUISystem.Composition.Ground ||
                         (effectiveLeftUpgrades & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay ||
                         (effectiveRightUpgrades & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay))
                    || (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace &&
                        ((EntityManager.TryGetComponent(edge.m_Start, out Game.Net.Elevation startElevation) && EvaluateCompositionUpgradesAndToolOptions(entity, startElevation))
                        || (EntityManager.TryGetComponent(edge.m_End, out Game.Net.Elevation endElevation) && EvaluateCompositionUpgradesAndToolOptions(entity, endElevation))))))
                {
                    EntityManager.AddComponent<SetEndElevationsToZero>(entity);
                    m_Log.Debug("added custom component to segment");
                    temp.m_Flags |= TempFlags.Replace;
                    EntityManager.SetComponentData(entity, temp);
                    m_Log.Debug($"{nameof(TempNetworkSystem)}{nameof(OnUpdate)} added replace to temp. if not already.");
                    EntityManager.RemoveComponent<Elevation>(edge.m_Start);
                    EntityManager.RemoveComponent<Elevation>(edge.m_End);
                }

                if ((effectiveLeftUpgrades & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall
                    || (effectiveLeftUpgrades & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay
                    || (effectiveRightUpgrades & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall
                    || (effectiveRightUpgrades & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay
                    || (effectiveComposition & NetworkAnarchyUISystem.Composition.Tunnel) == NetworkAnarchyUISystem.Composition.Tunnel
                    || (effectiveComposition & NetworkAnarchyUISystem.Composition.Elevated) == NetworkAnarchyUISystem.Composition.Elevated)
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
                        elevation.m_Elevation.y = Mathf.Clamp(Mathf.Max(elevation.m_Elevation.y, m_NetToolSystem.elevation, NetworkDefinitionSystem.QuayThreshold), NetworkDefinitionSystem.QuayThreshold, NetworkDefinitionSystem.ElevatedThreshold - .01f);
                    }

                    if ((effectiveLeftUpgrades & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) == NetworkAnarchyUISystem.SideUpgrades.RetainingWall)
                    {
                        elevation.m_Elevation.x = Mathf.Min(elevation.m_Elevation.x, m_NetToolSystem.elevation, NetworkDefinitionSystem.RetainingWallThreshold);
                    }
                    else if ((effectiveLeftUpgrades & NetworkAnarchyUISystem.SideUpgrades.Quay) == NetworkAnarchyUISystem.SideUpgrades.Quay)
                    {
                        elevation.m_Elevation.x = Mathf.Clamp(Mathf.Max(elevation.m_Elevation.x, m_NetToolSystem.elevation, NetworkDefinitionSystem.QuayThreshold), NetworkDefinitionSystem.QuayThreshold, NetworkDefinitionSystem.ElevatedThreshold - .01f);
                    }

                    if ((effectiveComposition & NetworkAnarchyUISystem.Composition.Elevated) == NetworkAnarchyUISystem.Composition.Elevated)
                    {
                        elevation.m_Elevation.x = Mathf.Max(elevation.m_Elevation.x, NetworkDefinitionSystem.ElevatedThreshold);
                        elevation.m_Elevation.y = Mathf.Max(elevation.m_Elevation.y, NetworkDefinitionSystem.ElevatedThreshold);

                        if (EntityManager.TryGetBuffer(edge.m_Start, isReadOnly: true, out DynamicBuffer<ConnectedEdge> startConnectedEdges))
                        {
                            bool addPillar = true;
                            foreach (ConnectedEdge connectedEdge in startConnectedEdges)
                            {
                                if (!EntityManager.TryGetComponent(connectedEdge.m_Edge, out Upgraded upgraded1) || (upgraded1.m_Flags.m_General & CompositionFlags.General.Elevated) != CompositionFlags.General.Elevated)
                                {
                                    addPillar = false;
                                    break;
                                }
                            }

                            if (addPillar)
                            {
                                if (!EntityManager.HasComponent<Game.Net.Elevation>(edge.m_Start))
                                {
                                    EntityManager.AddComponent<Game.Net.Elevation>(edge.m_Start);
                                }

                                EntityManager.SetComponentData(edge.m_Start, elevation);
                            }
                        }

                        if (EntityManager.TryGetBuffer(edge.m_End, isReadOnly: true, out DynamicBuffer<ConnectedEdge> endConnectedEdges))
                        {
                            bool addPillar = true;
                            foreach (ConnectedEdge connectedEdge in endConnectedEdges)
                            {
                                if (!EntityManager.TryGetComponent(connectedEdge.m_Edge, out Upgraded upgraded1) || (upgraded1.m_Flags.m_General & CompositionFlags.General.Elevated) != CompositionFlags.General.Elevated)
                                {
                                    addPillar = false;
                                    break;
                                }
                            }

                            if (addPillar)
                            {
                                if (!EntityManager.HasComponent<Game.Net.Elevation>(edge.m_End))
                                {
                                    EntityManager.AddComponent<Game.Net.Elevation>(edge.m_End);
                                }

                                EntityManager.SetComponentData(edge.m_End, elevation);
                            }
                        }
                    }
                    else if ((effectiveComposition & NetworkAnarchyUISystem.Composition.Tunnel) == NetworkAnarchyUISystem.Composition.Tunnel)
                    {
                        elevation.m_Elevation.x = Mathf.Min(elevation.m_Elevation.x, NetworkDefinitionSystem.TunnelThreshold);
                        elevation.m_Elevation.y = Mathf.Min(elevation.m_Elevation.y, NetworkDefinitionSystem.TunnelThreshold);
                    }

                    EntityManager.SetComponentData(entity, elevation);
                }

                EntityManager.TryGetComponent(entity, out Elevation finalElevation);
                if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace &&
                    !m_SecondaryApplyMimic.IsPressed() &&
                    !m_SecondaryApplyMimic.WasPerformedThisFrame() &&
                   (finalElevation.m_Elevation.x != originalElevation.m_Elevation.x ||
                    finalElevation.m_Elevation.y != originalElevation.m_Elevation.y))
                {
                    changedElevation = true;
                }

                CompositionFlags compositionFlags = default;

                if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace && EntityManager.TryGetComponent(entity, out Upgraded currentUpgrades))
                {
                    compositionFlags = currentUpgrades.m_Flags;
                    m_Log.Debug($"{nameof(TempNetworkSystem)}.{nameof(OnUpdate)} Replace Upgraded General = {compositionFlags.m_General} Left = {compositionFlags.m_Left} Right = {compositionFlags.m_Right}");
                    if (!UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()) || effectiveLeftUpgrades != 0)
                    {
                        compositionFlags.m_Left &= ~(CompositionFlags.Side.WideSidewalk | CompositionFlags.Side.Lowered | CompositionFlags.Side.Raised | CompositionFlags.Side.PrimaryBeautification | CompositionFlags.Side.SecondaryBeautification | CompositionFlags.Side.SoundBarrier);
                    }

                    if (!UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()) || effectiveRightUpgrades != 0)
                    {
                        compositionFlags.m_Right &= ~(CompositionFlags.Side.WideSidewalk | CompositionFlags.Side.Lowered | CompositionFlags.Side.Raised | CompositionFlags.Side.PrimaryBeautification | CompositionFlags.Side.SecondaryBeautification | CompositionFlags.Side.SoundBarrier);
                    }
                }


                if ((m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace || m_UISystem.ReplaceComposition)
                    && (!UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()) || effectiveComposition != 0))
                {
                    if (effectiveComposition != 0
                        || (compositionFlags.m_General & CompositionFlags.General.Elevated) == CompositionFlags.General.Elevated
                        || (compositionFlags.m_General & CompositionFlags.General.Tunnel) == CompositionFlags.General.Tunnel
                        || ((placeableNetData.m_SetUpgradeFlags.m_Right & CompositionFlags.Side.PrimaryTrack) != CompositionFlags.Side.PrimaryTrack
                           && (placeableNetData.m_SetUpgradeFlags.m_Right & CompositionFlags.Side.PrimaryLane) != CompositionFlags.Side.PrimaryLane)
                        || m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace)
                    {
                        compositionFlags.m_General = GetCompositionGeneralFlags(effectiveComposition);

                        m_Log.Debug($"{nameof(TempNetworkSystem)}.{nameof(OnUpdate)} composition changed");
                    }
                }


                CompositionFlags.Side leftSideTracksAndLanes = 0;
                CompositionFlags.Side rightSideTracksAndLanes = 0;
                if (EntityManager.TryGetComponent(entity, out PrefabRef tempPrefabRef) && EntityManager.TryGetComponent(tempPrefabRef.m_Prefab, out NetData netData))
                {
                    if ((netData.m_SideFlagMask & CompositionFlags.Side.PrimaryTrack) == CompositionFlags.Side.PrimaryTrack)
                    {
                        if ((compositionFlags.m_Left & CompositionFlags.Side.PrimaryTrack) == CompositionFlags.Side.PrimaryTrack)
                        {
                            leftSideTracksAndLanes |= CompositionFlags.Side.PrimaryTrack;
                        }

                        if ((compositionFlags.m_Right & CompositionFlags.Side.PrimaryTrack) == CompositionFlags.Side.PrimaryTrack)
                        {
                            rightSideTracksAndLanes |= CompositionFlags.Side.PrimaryTrack;
                        }
                    }

                    if ((netData.m_SideFlagMask & CompositionFlags.Side.PrimaryLane) == CompositionFlags.Side.PrimaryLane)
                    {
                        if ((compositionFlags.m_Left & CompositionFlags.Side.PrimaryLane) == CompositionFlags.Side.PrimaryLane)
                        {
                            leftSideTracksAndLanes |= CompositionFlags.Side.PrimaryLane;
                        }

                        if ((compositionFlags.m_Right & CompositionFlags.Side.PrimaryLane) == CompositionFlags.Side.PrimaryLane)
                        {
                            rightSideTracksAndLanes |= CompositionFlags.Side.PrimaryLane;
                        }
                    }
                }

                if (SideUpgradeLookup.ContainsKey(effectiveLeftUpgrades)
                    && (m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace || m_UISystem.ReplaceLeftUpgrade || (UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()) && effectiveLeftUpgrades != 0))
                    && (!UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()) || effectiveLeftUpgrades != 0))
                {
                    if (effectiveLeftUpgrades != 0
                        || (compositionFlags.m_Left & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised
                        || (compositionFlags.m_Left & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered
                        || ((placeableNetData.m_SetUpgradeFlags.m_Right & CompositionFlags.Side.PrimaryTrack) != CompositionFlags.Side.PrimaryTrack
                           && (placeableNetData.m_SetUpgradeFlags.m_Right & CompositionFlags.Side.PrimaryLane) != CompositionFlags.Side.PrimaryLane)
                        || m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace)
                    {
                        compositionFlags.m_Left |= SideUpgradeLookup[effectiveLeftUpgrades];
                    }

                    if (leftSideTracksAndLanes != 0 && m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace && m_UISystem.ReplaceLeftUpgrade)
                    {
                        compositionFlags.m_Left |= leftSideTracksAndLanes;
                    }

                    m_Log.Debug($"{nameof(TempNetworkSystem)}.{nameof(OnUpdate)} m_NetToolSystem.actualMode = {m_NetToolSystem.actualMode} m_UISystem.ReplaceLeftUpgrade {m_UISystem.ReplaceLeftUpgrade} compositionFlags.m_Left {compositionFlags.m_Left}");
                }

                if (SideUpgradeLookup.ContainsKey(effectiveRightUpgrades)
                    && (m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace || m_UISystem.ReplaceRightUpgrade || (UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()) && effectiveRightUpgrades != 0))
                    && (!UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID()) || effectiveRightUpgrades != 0))
                {
                    if (effectiveRightUpgrades != 0
                        || (compositionFlags.m_Right & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised
                        || (compositionFlags.m_Right & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered
                        || ((placeableNetData.m_SetUpgradeFlags.m_Right & CompositionFlags.Side.PrimaryTrack) != CompositionFlags.Side.PrimaryTrack
                            && (placeableNetData.m_SetUpgradeFlags.m_Right & CompositionFlags.Side.PrimaryLane) != CompositionFlags.Side.PrimaryLane)
                        || m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace)
                    {
                        compositionFlags.m_Right |= SideUpgradeLookup[effectiveRightUpgrades];
                    }

                    if (rightSideTracksAndLanes != 0 && m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace && m_UISystem.ReplaceRightUpgrade)
                    {
                        compositionFlags.m_Right |= rightSideTracksAndLanes;
                    }

                    m_Log.Debug($"{nameof(TempNetworkSystem)}.{nameof(OnUpdate)} m_NetToolSystem.actualMode = {m_NetToolSystem.actualMode} m_UISystem.ReplaceRightUpgrade {m_UISystem.ReplaceRightUpgrade} compositionFlags.m_Right {compositionFlags.m_Right}");
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

                    if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace &&
                        originalUpgraded.m_Flags.m_General == 0 &&
                        originalUpgraded.m_Flags.m_Left == 0 &&
                        originalUpgraded.m_Flags.m_Right == 0 &&
                        !changedElevation)
                    {
                        temp.m_Flags = originalTempFlags;
                        EntityManager.SetComponentData(entity, temp);
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

                // This is to remove retaining wall and quay upgrades from networks that are also elevated.
                if ((upgrades.m_Flags.m_General & CompositionFlags.General.Elevated) == CompositionFlags.General.Elevated)
                {
                    upgrades.m_Flags.m_Left &= ~(CompositionFlags.Side.Raised | CompositionFlags.Side.Lowered);
                    upgrades.m_Flags.m_Right &= ~(CompositionFlags.Side.Raised | CompositionFlags.Side.Lowered);
                }


                if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace &&
                    originalUpgraded.m_Flags == upgrades.m_Flags &&
                   !changedElevation)
                {
                    temp.m_Flags = originalTempFlags;
                    EntityManager.SetComponentData(entity, temp);
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

        private bool EvaluateCompositionUpgradesAndToolOptions(Entity entity, Elevation elevation)
        {
            EntityManager.TryGetComponent(entity, out Upgraded upgraded);
            bool isTunnel = (upgraded.m_Flags.m_General & CompositionFlags.General.Tunnel) == CompositionFlags.General.Tunnel || (elevation.m_Elevation.x <= NetworkDefinitionSystem.TunnelThreshold && elevation.m_Elevation.y <= NetworkDefinitionSystem.TunnelThreshold);
            if ((m_UISystem.ReplaceComposition || UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID())) && (m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Tunnel) != NetworkAnarchyUISystem.Composition.Tunnel && isTunnel)
            {
                return true;
            }

            bool isElevated = (upgraded.m_Flags.m_General & CompositionFlags.General.Elevated) == CompositionFlags.General.Elevated || (elevation.m_Elevation.x >= NetworkDefinitionSystem.ElevatedThreshold && elevation.m_Elevation.y >= NetworkDefinitionSystem.ElevatedThreshold);
            if ((m_UISystem.ReplaceComposition || UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID())) && (m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Elevated) != NetworkAnarchyUISystem.Composition.Elevated && isElevated)
            {
                return true;
            }

            bool isLeftRetainingWall = (upgraded.m_Flags.m_Left & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered || (elevation.m_Elevation.x < 0 && elevation.m_Elevation.x > NetworkDefinitionSystem.TunnelThreshold);
            if ((m_UISystem.ReplaceLeftUpgrade || UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID())) && (m_UISystem.LeftUpgrade & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) != NetworkAnarchyUISystem.SideUpgrades.RetainingWall && isLeftRetainingWall)
            {
                return true;
            }

            bool isLeftQuayWall = (upgraded.m_Flags.m_Left & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised || (elevation.m_Elevation.x < NetworkDefinitionSystem.ElevatedThreshold && elevation.m_Elevation.x > 0f);
            if ((m_UISystem.ReplaceLeftUpgrade || UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID())) && (m_UISystem.LeftUpgrade & NetworkAnarchyUISystem.SideUpgrades.Quay) != NetworkAnarchyUISystem.SideUpgrades.Quay && isLeftQuayWall)
            {
                return true;
            }

            bool isRightRetainingWall = (upgraded.m_Flags.m_Right & CompositionFlags.Side.Lowered) == CompositionFlags.Side.Lowered || (elevation.m_Elevation.y < 0f && elevation.m_Elevation.y > NetworkDefinitionSystem.TunnelThreshold);
            if ((m_UISystem.ReplaceRightUpgrade || UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID())) && (m_UISystem.RightUpgrade & NetworkAnarchyUISystem.SideUpgrades.RetainingWall) != NetworkAnarchyUISystem.SideUpgrades.RetainingWall && isRightRetainingWall)
            {
                return true;
            }

            bool isRightQuayWall = (upgraded.m_Flags.m_Right & CompositionFlags.Side.Raised) == CompositionFlags.Side.Raised || (elevation.m_Elevation.y < NetworkDefinitionSystem.ElevatedThreshold && elevation.m_Elevation.y > 0f);
            if ((m_UISystem.ReplaceRightUpgrade || UpgradeLookup.Contains(m_ToolSystem.activePrefab.GetPrefabID())) && (m_UISystem.RightUpgrade & NetworkAnarchyUISystem.SideUpgrades.Quay) != NetworkAnarchyUISystem.SideUpgrades.Quay && isRightQuayWall)
            {
                return true;
            }

            if (m_UISystem.ReplaceComposition && (m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Ground) == NetworkAnarchyUISystem.Composition.Ground)
            {
                return true;
            }

            return false;
        }
    }
}
