// <copyright file="UpdateNextFrame.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Components
{
    using Unity.Entities;

    /// <summary>
    /// A component used to filter out prevent overriding of entitiy in future from queries.
    /// </summary>
    public struct UpdateNextFrame : IComponentData, IQueryTypeParameter
    {
    }
}