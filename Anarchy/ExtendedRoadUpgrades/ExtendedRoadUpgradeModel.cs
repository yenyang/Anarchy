// <copyright file="ExtendedRoadUpgradeModel.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// This code was originally part of Extended Road Upgrades by ST-Apps. It has been incorporated into this project with permission of ST-Apps.
namespace Anarchy.ExtendedRoadUpgrades
{
    using System.Collections.Generic;
    using Game.Prefabs;

    /// <summary>
    ///     Just a basic model containing all the information needed to provide the additional
    ///     upgrade modes, alongside their UI name and description.
    /// </summary>
    internal class ExtendedRoadUpgradeModel
    {
        /// <summary>
        ///     Gets or sets the internal id for the current upgrade mode.
        ///     This is used as Prefab id as well as to determine which icon to use.
        /// </summary>
        public string Id
        {
            get; set;
        }

        /// <summary>
        ///     Gets or sets the internal id for the current upgrade mode.
        ///     This is used as Prefab id as well as to determine which icon to use.
        /// </summary>
        public string ObsoleteId
        {
            get; set;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the current upgrade mode supports Underground also.
        /// </summary>
        public bool IsUnderground
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the <see cref="CompositionFlags"/> that must be added during the upgrade phase.
        /// </summary>
        public CompositionFlags m_SetUpgradeFlags
        {
            get; set;
        }

        /// <summary>
        ///     Gets or sets the <see cref="CompositionFlags"/> that must be removed during the upgrade phase.
        /// </summary>
        public CompositionFlags m_UnsetUpgradeFlags
        {
            get; set;
        }

        public IEnumerable<NetPieceRequirements> m_SetState
        {
            get; set;
        }

        public IEnumerable<NetPieceRequirements> m_UnsetState
        {
            get; set;
        }
    }
}
