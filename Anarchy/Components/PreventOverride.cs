// <copyright file="PreventOverride.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Components
{
    using Colossal.Serialization.Entities;
    using Unity.Entities;

    /// <summary>
    /// A component used to filter out prevent overriding of entitiy in future from queries.
    /// </summary>
    public struct PreventOverride : IComponentData, IQueryTypeParameter, IEmptySerializable
    {
    }
}