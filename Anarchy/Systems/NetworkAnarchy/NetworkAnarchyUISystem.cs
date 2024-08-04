// <copyright file="NetworkAnarchyUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.NetworkAnarchy
{
    using System.Collections.Generic;
    using Anarchy;
    using Anarchy.Extensions;
    using Anarchy.Systems;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Entities;

    /// <summary>
    /// A UI System for CS:1 Network Anarchy type UI.
    /// </summary>
    public partial class NetworkAnarchyUISystem : ExtendedUISystemBase
    {
        private ILog m_Log;
        private ValueBindingHelper<SideUpgrades> m_LeftUpgrade;
        private ValueBindingHelper<SideUpgrades> m_RightUpgrade;
        private ValueBindingHelper<Composition> m_Composition;
        private ValueBindingHelper<SideUpgrades> m_ShowUpgrade;
        private ValueBindingHelper<Composition> m_ShowComposition;
        private ToolSystem m_ToolSystem;
        private PrefabSystem m_PrefabSystem;
        private NetToolSystem m_NetToolSystem;
        private TempNetworkSystem m_TempNetworkSystem;
        private NetToolSystem.Mode m_PreviousMode;

        /// <summary>
        /// An enum for network cross section modes.
        /// </summary>
        public enum SideUpgrades
        {
            /// <summary>
            /// Vanilla placement.
            /// </summary>
            None,

            /// <summary>
            /// Attempted Quay placement.
            /// </summary>
            Quay = 1,

            /// <summary>
            /// Attempted RetainingWall Placement.
            /// </summary>
            RetainingWall = 2,

            /// <summary>
            /// Adds street trees.
            /// </summary>
            Trees = 4,

            /// <summary>
            /// Adds Grass Strips.
            /// </summary>
            GrassStrip = 8,

            /// <summary>
            /// Adds Wide Sidewalk
            /// </summary>
            WideSidewalk = 16,

            /// <summary>
            /// Adds Sound Barrier.
            /// </summary>
            SoundBarrier = 32,
        }

        /// <summary>
        /// An enum for network composition.
        /// </summary>
        public enum Composition
        {
            /// <summary>
            /// Vanilla Placement,
            /// </summary>
            None,

            /// <summary>
            /// Forced ground placement.
            /// </summary>
            Ground = 1,

            /// <summary>
            /// Forced elevated placement.
            /// </summary>
            Elevated = 2,

            /// <summary>
            /// Forced tunnel placement.
            /// </summary>
            Tunnel = 4,

            /// <summary>
            /// Forced constant slope.
            /// </summary>
            ConstantSlope = 8,

            /// <summary>
            /// Adds a wide median.
            /// </summary>
            WideMedian = 16,

            /// <summary>
            /// Median Trees.
            /// </summary>
            Trees = 32,

            /// <summary>
            /// Median Grass Strip.
            /// </summary>
            GrassStrip = 64,

            /// <summary>
            /// Lighting
            /// </summary>
            Lighting = 128,
        }

        /// <summary>
        /// Gets the Left upgdrade.
        /// </summary>
        public SideUpgrades LeftUpgrade
        {
            get
            {
                return m_LeftUpgrade.Value & m_ShowUpgrade.Value;
            }
        }

        /// <summary>
        /// Gets the Right upgrade.
        /// </summary>
        public SideUpgrades RightUpgrade
        {
            get { return m_RightUpgrade.Value & m_ShowUpgrade.Value; }
        }

        /// <summary>
        /// Gets the Placement Mode.
        /// </summary>
        public Composition NetworkComposition
        {
            get { return m_Composition.Value & m_ShowComposition.Value; }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(NetworkAnarchyUISystem)}.{nameof(OnCreate)}");
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ToolSystem.EventToolChanged += OnToolChanged;
            m_ToolSystem.EventPrefabChanged += OnPrefabChanged;
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_TempNetworkSystem = World.GetOrCreateSystemManaged<TempNetworkSystem>();

            // Establishing bindings between UI and C#.
            m_LeftUpgrade = CreateBinding("LeftUpgrade", SideUpgrades.None);
            m_RightUpgrade = CreateBinding("RightUpgrade", SideUpgrades.None);
            m_Composition = CreateBinding("Composition", Composition.None);
            m_ShowUpgrade = CreateBinding("ShowUpgrade", SideUpgrades.None);
            m_ShowComposition = CreateBinding("ShowComposition", Composition.None);
            m_PreviousMode = m_NetToolSystem.actualMode;

            // Creates triggers for C# methods based on UI events.
            CreateTrigger<int>("LeftUpgrade", LeftUpgradeClicked);
            CreateTrigger<int>("RightUpgrade", RightUpgradeClicked);
            CreateTrigger<int>("Composition", CompositionModeClicked);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_PreviousMode != m_NetToolSystem.actualMode)
            {
                OnPrefabChanged(m_ToolSystem.activePrefab);
                m_PreviousMode = m_NetToolSystem.actualMode;
            }
        }

        private void LeftUpgradeClicked(int mode)
        {
            SideUpgrades sideUpgrade = (SideUpgrades)mode;
            if ((m_LeftUpgrade.Value & sideUpgrade) == sideUpgrade)
            {
                m_LeftUpgrade.Value &= ~sideUpgrade;
            }
            else if (sideUpgrade == SideUpgrades.Trees && (m_LeftUpgrade.Value == SideUpgrades.WideSidewalk || m_LeftUpgrade.Value == SideUpgrades.GrassStrip)
                || m_LeftUpgrade == SideUpgrades.Trees && (sideUpgrade == SideUpgrades.WideSidewalk || sideUpgrade == SideUpgrades.GrassStrip))
            {
                m_LeftUpgrade.Value |= sideUpgrade;
            }
            else if ((m_LeftUpgrade.Value & SideUpgrades.WideSidewalk) == SideUpgrades.WideSidewalk && sideUpgrade == SideUpgrades.GrassStrip)
            {
                m_LeftUpgrade.Value &= ~SideUpgrades.WideSidewalk;
                m_LeftUpgrade.Value |= sideUpgrade;
            }
            else if ((m_LeftUpgrade.Value & SideUpgrades.GrassStrip) == SideUpgrades.GrassStrip && sideUpgrade == SideUpgrades.WideSidewalk)
            {
                m_LeftUpgrade.Value &= ~SideUpgrades.GrassStrip;
                m_LeftUpgrade.Value |= sideUpgrade;
            }
            else
            {
                m_LeftUpgrade.Value = sideUpgrade;
            }
        }

        private void RightUpgradeClicked(int mode)
        {
            SideUpgrades sideUpgrade = (SideUpgrades)mode;
            if ((m_RightUpgrade.Value & sideUpgrade) == sideUpgrade)
            {
                m_RightUpgrade.Value &= ~sideUpgrade;
            }
            else if (sideUpgrade == SideUpgrades.Trees && (m_RightUpgrade.Value == SideUpgrades.WideSidewalk || m_RightUpgrade.Value == SideUpgrades.GrassStrip)
                || m_RightUpgrade == SideUpgrades.Trees && (sideUpgrade == SideUpgrades.WideSidewalk || sideUpgrade == SideUpgrades.GrassStrip))
            {
                m_RightUpgrade.Value |= sideUpgrade;
            }
            else if ((m_RightUpgrade.Value & SideUpgrades.WideSidewalk) == SideUpgrades.WideSidewalk && sideUpgrade == SideUpgrades.GrassStrip)
            {
                m_RightUpgrade.Value &= ~SideUpgrades.WideSidewalk;
                m_RightUpgrade.Value |= sideUpgrade;
            }
            else if ((m_RightUpgrade.Value & SideUpgrades.GrassStrip) == SideUpgrades.GrassStrip && sideUpgrade == SideUpgrades.WideSidewalk)
            {
                m_RightUpgrade.Value &= ~SideUpgrades.GrassStrip;
                m_RightUpgrade.Value |= sideUpgrade;
            }
            else
            {
                m_RightUpgrade.Value = sideUpgrade;
            }

            if (((SideUpgrades.WideSidewalk | SideUpgrades.Trees | SideUpgrades.GrassStrip) & sideUpgrade) == sideUpgrade)
            {
                m_Composition.Value &= ~(Composition.WideMedian | Composition.GrassStrip | Composition.Trees);
            }
        }

        private void CompositionModeClicked(int composition)
        {
            Composition oldComposition = m_Composition.Value;

            Composition newComposition = (Composition)composition;
            if (((Composition.Tunnel | Composition.Ground | Composition.Elevated) & newComposition) == newComposition)
            {
                m_Composition.Value &= ~(Composition.Tunnel | Composition.Ground | Composition.Elevated);
            }

            if (((Composition.GrassStrip | Composition.Trees) & newComposition) == newComposition)
            {
                m_Composition.Value &= ~Composition.WideMedian;
                m_LeftUpgrade.Value &= ~(SideUpgrades.GrassStrip | SideUpgrades.WideSidewalk | SideUpgrades.Trees);
                m_RightUpgrade.Value &= ~(SideUpgrades.GrassStrip | SideUpgrades.WideSidewalk | SideUpgrades.Trees);
            }

            if ((Composition.WideMedian & newComposition) == newComposition)
            {
                m_Composition.Value &= ~(Composition.GrassStrip | Composition.Trees);
                m_LeftUpgrade.Value &= ~(SideUpgrades.GrassStrip | SideUpgrades.WideSidewalk | SideUpgrades.Trees);
                m_RightUpgrade.Value &= ~(SideUpgrades.GrassStrip | SideUpgrades.WideSidewalk | SideUpgrades.Trees);
            }

            if ((oldComposition & newComposition) == newComposition)
            {
                m_Composition.Value &= ~newComposition;
            }
            else
            {
                m_Composition.Value |= newComposition;
            }

            OnPrefabChanged(m_ToolSystem.activePrefab);
        }

        private void OnToolChanged(ToolBaseSystem toolSystem)
        {
            m_ShowUpgrade.Value = SideUpgrades.None;
            PrefabBase prefabBase = toolSystem.GetPrefab();

            if (m_ToolSystem.activeTool != m_NetToolSystem || prefabBase is null)
            {
                Enabled = false;
                return;
            }

            Enabled = true;
            OnPrefabChanged(prefabBase);
        }

        private void OnPrefabChanged(PrefabBase prefabBase)
        {
            m_ShowUpgrade.Value = SideUpgrades.None;
            m_ShowComposition.Value = Composition.None;

            if (prefabBase is null || m_ToolSystem.activeTool != m_NetToolSystem)
            {
                return;
            }

            if (!m_PrefabSystem.TryGetEntity(prefabBase, out Entity prefabEntity))
            {
                return;
            }

            if (!EntityManager.TryGetComponent(prefabEntity, out NetData netData))
            {
                return;
            }

            foreach (KeyValuePair<SideUpgrades, CompositionFlags.Side> keyValuePair in m_TempNetworkSystem.SideUpgradesDictionary)
            {
                if ((netData.m_SideFlagMask & keyValuePair.Value) == keyValuePair.Value)
                {
                    m_ShowUpgrade.Value |= keyValuePair.Key;
                }
            }

            foreach (KeyValuePair<Composition, CompositionFlags.General> generalUpgradePairs in m_TempNetworkSystem.GeneralCompositionDictionary)
            {
                if ((netData.m_GeneralFlagMask & generalUpgradePairs.Value) == generalUpgradePairs.Value)
                {
                    m_ShowComposition.Value |= generalUpgradePairs.Key;
                }
            }

            if (!EntityManager.TryGetComponent(prefabEntity, out NetGeometryData netGeometryData))
            {
                return;
            }

            if (!EntityManager.TryGetComponent(prefabEntity, out PlaceableNetData placeableNetData))
            {
                return;
            }

            if ((netGeometryData.m_Flags & Game.Net.GeometryFlags.RequireElevated) == Game.Net.GeometryFlags.RequireElevated)
            {
                m_ShowComposition.Value &= ~(Composition.Elevated | Composition.Tunnel | Composition.Trees | Composition.GrassStrip);
                m_ShowUpgrade.Value &= ~(SideUpgrades.Trees | SideUpgrades.GrassStrip | SideUpgrades.SoundBarrier | SideUpgrades.WideSidewalk);
            }
            else if ((placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.IsUpgrade) != Game.Net.PlacementFlags.IsUpgrade
                && (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.UpgradeOnly) != Game.Net.PlacementFlags.UpgradeOnly)
            {
                m_ShowComposition.Value |= Composition.Ground;
            }
            else
            {
                m_ShowComposition.Value &= ~(Composition.Elevated | Composition.Tunnel);
            }

            if ((m_Composition & Composition.Tunnel) == Composition.Tunnel
                || (m_Composition & Composition.Elevated) == Composition.Elevated)
            {
                m_ShowComposition.Value &= ~(Composition.Trees | Composition.GrassStrip);
                m_ShowUpgrade.Value &= ~(SideUpgrades.Trees | SideUpgrades.GrassStrip | SideUpgrades.SoundBarrier | SideUpgrades.WideSidewalk);
            }

            if ((netGeometryData.m_Flags & Game.Net.GeometryFlags.SmoothSlopes) != Game.Net.GeometryFlags.SmoothSlopes
                && (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.IsUpgrade) != Game.Net.PlacementFlags.IsUpgrade
                && (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.UpgradeOnly) != Game.Net.PlacementFlags.UpgradeOnly
                && m_NetToolSystem.actualMode != NetToolSystem.Mode.Grid)
            {
                m_ShowComposition.Value |= Composition.ConstantSlope;
            }
        }
    }
}
