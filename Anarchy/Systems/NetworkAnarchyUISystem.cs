// <copyright file="NetworkAnarchyUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using Anarchy.Extensions;
    using Colossal.Logging;

    /// <summary>
    /// A UI System for CS:1 Network Anarchy type UI.
    /// </summary>
    public partial class NetworkAnarchyUISystem : ExtendedUISystemBase
    {
        private ILog m_Log;
        private ValueBindingHelper<SideUpgrades> m_LeftUpgrade;
        private ValueBindingHelper<SideUpgrades> m_RightUprade;
        private ValueBindingHelper<Composition> m_Composition;

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
            /// Remove pillars.
            /// </summary>
            NoPillars = 16,

            /// <summary>
            /// Remove Height Limits and clearances.
            /// </summary>
            NoHeightLimits = 32,
        }

        /// <summary>
        /// Gets the Left upgdrade.
        /// </summary>
        public SideUpgrades LeftUpgrade
        {
            get { return m_LeftUpgrade; }
        }

        /// <summary>
        /// Gets the Right upgrade.
        /// </summary>
        public SideUpgrades RightUpgrade
        {
            get { return m_RightUprade; }
        }

        /// <summary>
        /// Gets the Placement Mode.
        /// </summary>
        public Composition NetworkComposition
        {
            get { return m_Composition; }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(NetworkAnarchyUISystem)}.{nameof(OnCreate)}");
            m_LeftUpgrade = CreateBinding("LeftUpgrade", SideUpgrades.None);
            m_RightUprade = CreateBinding("RightUpgrade", SideUpgrades.None);
            m_Composition = CreateBinding("Composition", Composition.None);

            CreateTrigger<int>("LeftUpgrade", LeftUpgradeClicked);
            CreateTrigger<int>("RightUpgrade", RightUpgradeClicked);
            CreateTrigger<int>("Composition", CompositionModeClicked);
        }

        private void LeftUpgradeClicked(int mode)
        {
            if ((SideUpgrades)mode == m_LeftUpgrade)
            {
                m_LeftUpgrade.Value = SideUpgrades.None;
            }
            else
            {
                m_LeftUpgrade.Value = (SideUpgrades)mode;
            }
        }

        private void RightUpgradeClicked(int mode)
        {
            if ((SideUpgrades)mode == m_RightUprade.Value)
            {
                m_RightUprade.Value = SideUpgrades.None;
            }
            else
            {
                m_RightUprade.Value = (SideUpgrades)mode;
            }
        }

        private void CompositionModeClicked(int composition)
        {
            Composition newComposition = (Composition)composition;
            if ((m_Composition.Value & newComposition) == newComposition)
            {
                m_Composition.Value &= ~newComposition;
            }
            else
            {
                m_Composition.Value |= newComposition;
            }

            if (newComposition == Composition.Ground)
            {
                m_Composition.Value &= ~Composition.Tunnel;
                m_Composition.Value &= ~Composition.Elevated;
            }

            if (newComposition == Composition.Tunnel)
            {
                m_Composition.Value &= ~Composition.Ground;
                m_Composition.Value &= ~Composition.Elevated;
            }

            if (newComposition == Composition.Elevated)
            {
                m_Composition.Value &= ~Composition.Tunnel;
                m_Composition.Value &= ~Composition.Ground;
            }
        }
    }
}
