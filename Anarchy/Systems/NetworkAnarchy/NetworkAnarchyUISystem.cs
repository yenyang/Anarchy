// <copyright file="NetworkAnarchyUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// #define DUMP_PREFABS
namespace Anarchy.Systems.NetworkAnarchy
{
    using System.Collections.Generic;
    using Anarchy;
    using Anarchy.Components;
    using Anarchy.ExtendedRoadUpgrades;
    using Anarchy.Extensions;
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;
    using static Colossal.AssetPipeline.Diagnostic.Report;

    /// <summary>
    /// A UI System for CS:1 Network Anarchy type UI.
    /// </summary>
    public partial class NetworkAnarchyUISystem : ExtendedUISystemBase
    {
        private ILog m_Log;
        private ValueBindingHelper<SideUpgrades> m_LeftUpgrade;
        private ValueBindingHelper<SideUpgrades> m_RightUpgrade;
        private ValueBindingHelper<Composition> m_Composition;
        private ValueBindingHelper<SideUpgrades> m_LeftShowUpgrade;
        private ValueBindingHelper<SideUpgrades> m_RightShowUpgrade;
        private ValueBindingHelper<Composition> m_ShowComposition;
        private ValueBindingHelper<ButtonState> m_ReplaceLeftUpgrade;
        private ValueBindingHelper<ButtonState> m_ReplaceRightUpgrade;
        private ValueBindingHelper<ButtonState> m_ReplaceComposition;
        private ToolSystem m_ToolSystem;
        private PrefabSystem m_PrefabSystem;
        private NetToolSystem m_NetToolSystem;
        private TempNetworkSystem m_TempNetworkSystem;
        private NetToolSystem.Mode m_PreviousMode;
        private ValueBindingHelper<bool> m_ShowElevationStepSlider;
        private bool m_InfoviewsFixed = false;
        private EntityQuery m_HeightRangePlaceableNetQuery;

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

            /// <summary>
            /// Expands elevation range to large amounts.
            /// </summary>
            ExpanedElevationRange = 256,
        }

        /// <summary>
        /// An enum to handle whether a button is selected and/or hidden.
        /// </summary>
        public enum ButtonState
        {
            /// <summary>
            /// Not selected.
            /// </summary>
            Off = 0,

            /// <summary>
            /// Selected.
            /// </summary>
            On = 1,

            /// <summary>
            /// Not shown.
            /// </summary>
            Hidden = 2,
        }

        /// <summary>
        /// Gets the Left upgdrade.
        /// </summary>
        public SideUpgrades LeftUpgrade
        {
            get
            {
                return m_LeftUpgrade.Value & m_LeftShowUpgrade.Value;
            }
        }

        /// <summary>
        /// Gets the Right upgrade.
        /// </summary>
        public SideUpgrades RightUpgrade
        {
            get { return m_RightUpgrade.Value & m_RightShowUpgrade.Value; }
        }

        /// <summary>
        /// Gets the Placement Mode.
        /// </summary>
        public Composition NetworkComposition
        {
            get { return m_Composition.Value & m_ShowComposition.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether gets replace left upgrade.
        /// </summary>
        public bool ReplaceLeftUpgrade
        {
            get { return (m_ReplaceLeftUpgrade & ButtonState.On) == ButtonState.On && AnarchyMod.Instance.Settings.ReplaceUpgradesBehavior; }
        }

        /// <summary>
        /// Gets a value indicating whether gets replace right upgrade.
        /// </summary>
        public bool ReplaceRightUpgrade
        {
            get { return (m_ReplaceRightUpgrade & ButtonState.On) == ButtonState.On && AnarchyMod.Instance.Settings.ReplaceUpgradesBehavior; }
        }

        /// <summary>
        /// Gets a value indicating whether gets replace composition.
        /// </summary>
        public bool ReplaceComposition
        {
            get { return (m_ReplaceComposition & ButtonState.On) == ButtonState.On && (AnarchyMod.Instance.Settings.ReplaceUpgradesBehavior || AnarchyMod.Instance.Settings.NetworkAnarchyToolOptions); }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(NetworkAnarchyUISystem)}.{nameof(OnCreate)}");
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ToolSystem.EventToolChanged += CheckPrefabAndUpdateButtonDisplay;
            m_ToolSystem.EventPrefabChanged += UpdateButtonDisplay;
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_TempNetworkSystem = World.GetOrCreateSystemManaged<TempNetworkSystem>();

            // Establishing bindings between UI and C#.
            m_LeftUpgrade = CreateBinding("LeftUpgrade", SideUpgrades.None);
            m_RightUpgrade = CreateBinding("RightUpgrade", SideUpgrades.None);
            m_Composition = CreateBinding("Composition", Composition.None);
            m_LeftShowUpgrade = CreateBinding("LeftShowUpgrade", SideUpgrades.None);
            m_RightShowUpgrade = CreateBinding("RightShowUpgrade", SideUpgrades.None);
            m_ShowComposition = CreateBinding("ShowComposition", Composition.None);
            if (!AnarchyMod.Instance.Settings.ReplaceUpgradesBehavior)
            {
                m_ReplaceRightUpgrade = CreateBinding("ReplaceRightUpgrade", ButtonState.Hidden | ButtonState.Off);
                m_ReplaceLeftUpgrade = CreateBinding("ReplaceLeftUpgrade", ButtonState.Hidden | ButtonState.Off);
                m_ReplaceComposition = CreateBinding("ReplaceComposition", ButtonState.Hidden | ButtonState.Off);
            }
            else
            {
                m_ReplaceRightUpgrade = CreateBinding("ReplaceRightUpgrade", ButtonState.Hidden | ButtonState.On);
                m_ReplaceLeftUpgrade = CreateBinding("ReplaceLeftUpgrade", ButtonState.Hidden | ButtonState.On);
                m_ReplaceComposition = CreateBinding("ReplaceComposition", ButtonState.Hidden | ButtonState.On);
            }

            m_ShowElevationStepSlider = CreateBinding("ShowElevationStepSlider", AnarchyMod.Instance.Settings.ElevationStepSlider);
            m_PreviousMode = m_NetToolSystem.actualMode;

            // Creates triggers for C# methods based on UI events.
            CreateTrigger<int>("LeftUpgrade", LeftUpgradeClicked);
            CreateTrigger<int>("RightUpgrade", RightUpgradeClicked);
            CreateTrigger<int>("Composition", CompositionModeClicked);
            CreateTrigger("ReplaceComposition", () => InvertButtonState(ref m_ReplaceComposition));
            CreateTrigger("ReplaceLeftUpgrade", () => InvertButtonState(ref m_ReplaceLeftUpgrade));
            CreateTrigger("ReplaceRightUpgrade", () => InvertButtonState(ref m_ReplaceRightUpgrade));

            m_HeightRangePlaceableNetQuery = SystemAPI.QueryBuilder()
                           .WithAll<HeightRangeRecord, PlaceableNetData>()
                           .Build();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_PreviousMode != m_NetToolSystem.actualMode)
            {
                UpdateButtonDisplay(m_ToolSystem.activePrefab);
                m_PreviousMode = m_NetToolSystem.actualMode;
            }
        }

        /// <inheritdoc/>
        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
            if (AnarchyMod.Instance.Settings.NetworkUpgradesPrefabs)
            {
                UpgradesManager.Install();
            }
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

#if DUMP_PREFABS && DEBUG
            EntityQuery prefabQuery = SystemAPI.QueryBuilder()
                          .WithAll<PrefabData>()
                          .Build();
            NativeArray<Entity> prefabEntities = prefabQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity e in prefabEntities)
            {
                if (!EntityManager.TryGetComponent(e, out PrefabData prefabData))
                {
                    return;
                }

                if (!m_PrefabSystem.TryGetPrefab(prefabData, out PrefabBase prefabBase))
                {
                    return;
                }

                if (prefabBase != null)
                {
                    m_Log.Info(prefabBase.GetPrefabID());
                }
            }

            prefabEntities.Dispose();
#endif

            // This fixes the placeable info view items defaulting to fire and rescue for these custom prefabs.
            if (m_InfoviewsFixed || (mode == GameMode.Game || mode == GameMode.Editor) == false)
            {
                return;
            }

            foreach (PrefabID prefabID in m_TempNetworkSystem.UpgradePrefabIDs)
            {
                if (m_PrefabSystem.TryGetPrefab(prefabID, out PrefabBase prefabBase)
                    && m_PrefabSystem.TryGetEntity(prefabBase, out Entity entity)
                    && EntityManager.TryGetBuffer(entity, isReadOnly: false, out DynamicBuffer<PlaceableInfoviewItem> placeableInfoViewItems)
                    && m_PrefabSystem.TryGetPrefab(new PrefabID("InfoviewPrefab", "None"), out PrefabBase noneInfoviewPrefabBase)
                    && m_PrefabSystem.TryGetEntity(noneInfoviewPrefabBase, out Entity noneInfoviewPrefabEntity))
                {
                    placeableInfoViewItems[0] = new PlaceableInfoviewItem() { m_Item = noneInfoviewPrefabEntity, m_Priority = 0 };
                    m_Log.Debug($"{nameof(NetworkAnarchyUISystem)}.{nameof(OnGameLoadingComplete)} fixed placeable Info view for {prefabID}.");
                }
                else
                {
                    m_Log.Debug($"{nameof(NetworkAnarchyUISystem)}.{nameof(OnGameLoadingComplete)} could not fix placeable Info view for {prefabID}.");
                }
            }

            if (mode == GameMode.Game || mode == GameMode.Editor)
            {
                m_InfoviewsFixed = true;
                m_Log.Debug($"{nameof(NetworkAnarchyUISystem)}.{nameof(OnGameLoadingComplete)} PlaceableInfo Views fixed.");
            }
        }

        private void LeftUpgradeClicked(int mode)
        {
            SideUpgrades sideUpgrade = (SideUpgrades)mode;
            if ((m_LeftUpgrade.Value & sideUpgrade) == sideUpgrade)
            {
                m_LeftUpgrade.Value &= ~sideUpgrade;
            }
            else if ((sideUpgrade == SideUpgrades.Trees && (m_LeftUpgrade.Value == SideUpgrades.WideSidewalk || m_LeftUpgrade.Value == SideUpgrades.GrassStrip))
                || (m_LeftUpgrade == SideUpgrades.Trees && (sideUpgrade == SideUpgrades.WideSidewalk || sideUpgrade == SideUpgrades.GrassStrip)))
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

            if (((SideUpgrades.WideSidewalk | SideUpgrades.Trees | SideUpgrades.GrassStrip) & sideUpgrade) == sideUpgrade)
            {
                m_Composition.Value &= ~(Composition.WideMedian | Composition.GrassStrip | Composition.Trees);
            }

            if (((SideUpgrades.Quay | SideUpgrades.RetainingWall) & sideUpgrade) == sideUpgrade &&
                ((m_RightUpgrade.Value & SideUpgrades.Quay) == SideUpgrades.Quay || (m_RightUpgrade.Value & SideUpgrades.RetainingWall) == SideUpgrades.RetainingWall))
            {
                m_Composition.Value &= ~Composition.Ground;
            }
        }

        private void RightUpgradeClicked(int mode)
        {
            SideUpgrades sideUpgrade = (SideUpgrades)mode;
            if ((m_RightUpgrade.Value & sideUpgrade) == sideUpgrade)
            {
                m_RightUpgrade.Value &= ~sideUpgrade;
            }
            else if ((sideUpgrade == SideUpgrades.Trees && (m_RightUpgrade.Value == SideUpgrades.WideSidewalk || m_RightUpgrade.Value == SideUpgrades.GrassStrip))
                || (m_RightUpgrade == SideUpgrades.Trees && (sideUpgrade == SideUpgrades.WideSidewalk || sideUpgrade == SideUpgrades.GrassStrip)))
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

            if (((SideUpgrades.Quay | SideUpgrades.RetainingWall) & sideUpgrade) == sideUpgrade &&
                ((m_LeftUpgrade.Value & SideUpgrades.Quay) == SideUpgrades.Quay || (m_LeftUpgrade.Value & SideUpgrades.RetainingWall) == SideUpgrades.RetainingWall))
            {
                m_Composition.Value &= ~Composition.Ground;
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

            if (((m_LeftUpgrade.Value & SideUpgrades.Quay) == SideUpgrades.Quay || (m_LeftUpgrade.Value & SideUpgrades.RetainingWall) == SideUpgrades.RetainingWall)
                && ((m_RightUpgrade.Value & SideUpgrades.Quay) == SideUpgrades.Quay || (m_RightUpgrade.Value & SideUpgrades.RetainingWall) == SideUpgrades.RetainingWall)
                && newComposition == Composition.Ground)
            {
                m_LeftUpgrade.Value &= ~(SideUpgrades.Quay | SideUpgrades.RetainingWall);
                m_RightUpgrade.Value &= ~(SideUpgrades.Quay | SideUpgrades.RetainingWall);
            }

            UpdateButtonDisplay(m_ToolSystem.activePrefab);
        }

        private void CheckPrefabAndUpdateButtonDisplay(ToolBaseSystem toolSystem)
        {
            PrefabBase prefabBase = toolSystem.GetPrefab();

            if (m_ToolSystem.activeTool != m_NetToolSystem || prefabBase is null)
            {
                Enabled = false;
                return;
            }

            Enabled = true;
            UpdateButtonDisplay(prefabBase);
        }

        private void InvertButtonState(ref ValueBindingHelper<ButtonState> button)
        {
            if ((button.Value &= ButtonState.On) == ButtonState.On)
            {
                button.Value &= ~ButtonState.On;
            }
            else
            {
                button.Value |= ButtonState.On;
            }

            CheckPrefabAndUpdateButtonDisplay(m_ToolSystem.activeTool);
        }

        private void ResetExpandedElevationRanges()
        {
            if (m_HeightRangePlaceableNetQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            NativeArray<Entity> entities = m_HeightRangePlaceableNetQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (EntityManager.TryGetComponent(entity, out PlaceableNetData placeableNetData) && EntityManager.TryGetComponent(entity, out HeightRangeRecord heightRangeRecord)) 
                {
                    placeableNetData.m_ElevationRange = new Colossal.Mathematics.Bounds1(heightRangeRecord.min, heightRangeRecord.max);
                    EntityManager.SetComponentData(entity, placeableNetData);
                    EntityManager.RemoveComponent<HeightRangeRecord>(entity);
#if DEBUG
                    if (m_PrefabSystem.TryGetPrefab(entity, out PrefabBase prefabBase))
                    {
                        m_Log.Debug($"{nameof(NetworkAnarchyUISystem)}.{nameof(ResetExpandedElevationRanges)} Reset {prefabBase.GetPrefabID().GetName()} elevation range back to {placeableNetData.m_ElevationRange.min} to {placeableNetData.m_ElevationRange.max}.");
                    }
#endif
                }
            }
        }

        private void ExpandPrefabElevationRange(Entity prefabEntity, PlaceableNetData placeableNetData, PrefabID prefabID)
        {
            if (prefabEntity != Entity.Null)
            {
                HeightRangeRecord heightRangeRecord = new HeightRangeRecord()
                {
                    min = placeableNetData.m_ElevationRange.min,
                    max = placeableNetData.m_ElevationRange.max,
                };
                EntityManager.AddComponent<HeightRangeRecord>(prefabEntity);
                EntityManager.SetComponentData(prefabEntity, heightRangeRecord);
                placeableNetData.m_ElevationRange = new Colossal.Mathematics.Bounds1(-1000f, 1000f);
                EntityManager.SetComponentData(prefabEntity, placeableNetData);
                m_Log.Debug($"{nameof(NetworkAnarchyUISystem)}.{nameof(ExpandPrefabElevationRange)} Expanded {prefabID.GetName()} elevation range to -1000 to 1000.");
            }
        }

        private void UpdateButtonDisplay(PrefabBase prefabBase)
        {
            m_LeftShowUpgrade.Value = SideUpgrades.None;
            m_RightShowUpgrade.Value = SideUpgrades.None;
            m_ShowComposition.Value = Composition.None;
            m_ReplaceComposition.Value |= ButtonState.Hidden;
            m_ReplaceLeftUpgrade.Value |= ButtonState.Hidden;
            m_ReplaceRightUpgrade.Value |= ButtonState.Hidden;
            m_ShowElevationStepSlider.Value = false;

            if (prefabBase is null || m_ToolSystem.activeTool != m_NetToolSystem)
            {
                return;
            }

            if (!m_PrefabSystem.TryGetEntity(prefabBase, out Entity prefabEntity))
            {
                return;
            }

            if (!EntityManager.TryGetComponent(prefabEntity, out PlaceableNetData placeableNetData))
            {
                return;
            }

            ResetExpandedElevationRanges();

            if (!EntityManager.TryGetComponent(prefabEntity, out NetData netData))
            {
                return;
            }

            if (!EntityManager.TryGetComponent(prefabEntity, out NetGeometryData netGeometryData))
            {
                return;
            }

            if (m_NetToolSystem.actualMode != NetToolSystem.Mode.Replace && (placeableNetData.m_ElevationRange.max != 0 || placeableNetData.m_ElevationRange.min != 0))
            {
                m_ShowElevationStepSlider.Value = AnarchyMod.Instance.Settings.ElevationStepSlider;
                if (AnarchyMod.Instance.Settings.NetworkAnarchyToolOptions)
                {
                    m_ShowComposition.Value |= Composition.ExpanedElevationRange;

                    if ((m_Composition.Value & Composition.ExpanedElevationRange) == Composition.ExpanedElevationRange)
                    {
                        ExpandPrefabElevationRange(prefabEntity, placeableNetData, prefabBase.GetPrefabID());
                    }
                }
            }

            if (!AnarchyMod.Instance.Settings.NetworkAnarchyToolOptions && !AnarchyMod.Instance.Settings.NetworkUpgradesToolOptions)
            {
                return;
            }


            if (AnarchyMod.Instance.Settings.NetworkUpgradesToolOptions)
            {
                foreach (KeyValuePair<SideUpgrades, CompositionFlags.Side> keyValuePair in m_TempNetworkSystem.SideUpgradesDictionary)
                {
                    if ((netData.m_SideFlagMask & keyValuePair.Value) == keyValuePair.Value)
                    {
                        m_LeftShowUpgrade.Value |= keyValuePair.Key;
                        m_RightShowUpgrade.Value |= keyValuePair.Key;
                    }
                }
            }

            foreach (KeyValuePair<Composition, CompositionFlags.General> generalUpgradePairs in m_TempNetworkSystem.GeneralCompositionDictionary)
            {
                if ((netData.m_GeneralFlagMask & generalUpgradePairs.Value) == generalUpgradePairs.Value)
                {
                    m_ShowComposition.Value |= generalUpgradePairs.Key;
                }
            }

            if ((netGeometryData.m_Flags & Game.Net.GeometryFlags.RequireElevated) == Game.Net.GeometryFlags.RequireElevated)
            {
                m_ShowComposition.Value &= ~(Composition.Elevated | Composition.Tunnel | Composition.Trees | Composition.GrassStrip);
                m_LeftShowUpgrade.Value &= ~(SideUpgrades.Trees | SideUpgrades.GrassStrip | SideUpgrades.SoundBarrier | SideUpgrades.WideSidewalk);
                m_RightShowUpgrade.Value &= ~(SideUpgrades.Trees | SideUpgrades.GrassStrip | SideUpgrades.SoundBarrier | SideUpgrades.WideSidewalk);
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

            if ((NetworkComposition & Composition.Tunnel) == Composition.Tunnel
                || (NetworkComposition & Composition.Elevated) == Composition.Elevated)
            {
                m_ShowComposition.Value &= ~(Composition.Trees | Composition.GrassStrip);
                m_LeftShowUpgrade.Value &= ~(SideUpgrades.Trees | SideUpgrades.GrassStrip | SideUpgrades.SoundBarrier | SideUpgrades.WideSidewalk);
                m_RightShowUpgrade.Value &= ~(SideUpgrades.Trees | SideUpgrades.GrassStrip | SideUpgrades.SoundBarrier | SideUpgrades.WideSidewalk);
            }

            if ((NetworkComposition & Composition.Tunnel) == Composition.Tunnel)
            {
                m_LeftShowUpgrade.Value &= ~(SideUpgrades.RetainingWall | SideUpgrades.Quay);
                m_RightShowUpgrade.Value &= ~(SideUpgrades.RetainingWall | SideUpgrades.Quay);
            }

            if ((netGeometryData.m_Flags & Game.Net.GeometryFlags.SmoothSlopes) != Game.Net.GeometryFlags.SmoothSlopes
                && (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.IsUpgrade) != Game.Net.PlacementFlags.IsUpgrade
                && (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.UpgradeOnly) != Game.Net.PlacementFlags.UpgradeOnly
                && m_NetToolSystem.actualMode != NetToolSystem.Mode.Grid)
            {
                m_ShowComposition.Value |= Composition.ConstantSlope;
            }

            if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Replace)
            {
                if (m_LeftShowUpgrade.Value != 0)
                {
                    m_ReplaceLeftUpgrade.Value &= ~ButtonState.Hidden;
                    m_ReplaceRightUpgrade.Value &= ~ButtonState.Hidden;

                    if ((m_ReplaceLeftUpgrade.Value &= ButtonState.On) != ButtonState.On)
                    {
                        m_LeftShowUpgrade.Value = 0;
                    }

                    if ((m_ReplaceRightUpgrade.Value &= ButtonState.On) != ButtonState.On)
                    {
                        m_RightShowUpgrade.Value = 0;
                    }
                }

                if (m_ShowComposition.Value != 0)
                {
                    m_ReplaceComposition.Value &= ~ButtonState.Hidden;

                    if ((m_ReplaceComposition.Value &= ButtonState.On) != ButtonState.On)
                    {
                        m_ShowComposition.Value = 0;
                    }
                }

                m_ShowComposition.Value &= ~(Composition.ConstantSlope | Composition.Ground);
            }

            if (EntityManager.HasComponent<Game.Prefabs.PipelineData>(prefabEntity))
            {
                m_ShowComposition.Value &= ~(Composition.ConstantSlope | Composition.Ground | Composition.ExpanedElevationRange);
            }

            if (EntityManager.HasComponent<Game.Prefabs.PowerLineData>(prefabEntity))
            {
                m_ShowComposition.Value &= ~(Composition.ConstantSlope | Composition.Ground);
            }

            if (!AnarchyMod.Instance.Settings.NetworkAnarchyToolOptions)
            {
                m_ShowComposition.Value &= ~(Composition.ConstantSlope | Composition.Tunnel | Composition.Ground | Composition.Elevated);
            }

            if (!AnarchyMod.Instance.Settings.NetworkUpgradesToolOptions)
            {
                m_LeftShowUpgrade.Value = SideUpgrades.None;
                m_RightShowUpgrade.Value = SideUpgrades.None;
                m_ReplaceLeftUpgrade.Value |= ButtonState.Hidden;
                m_ReplaceRightUpgrade.Value |= ButtonState.Hidden;
                m_ShowComposition.Value &= ~(Composition.WideMedian | Composition.Trees | Composition.GrassStrip | Composition.Lighting);
            }
        }
    }
}
