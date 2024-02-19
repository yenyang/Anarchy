// <copyright file="HeightRangeRecord.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Components
{
    using Unity.Entities;

    /// <summary>
    /// A component used to record the height range from net composition data.
    /// </summary>
    public struct HeightRangeRecord : IComponentData, IQueryTypeParameter
    {
        /// <summary>
        /// Records the min height.
        /// </summary>
        public float min;

        /// <summary>
        /// Records the max height.
        /// </summary>
        public float max;
    }
}