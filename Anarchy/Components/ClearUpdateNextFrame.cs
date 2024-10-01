﻿// <copyright file="ClearUNF.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Components
{
    using Unity.Entities;

    /// <summary>
    /// A component used to clear update next frame component.
    /// </summary>
    public struct ClearUpdateNextFrame : IComponentData, IQueryTypeParameter
    {
    }
}