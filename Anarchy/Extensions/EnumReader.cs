// <copyright file="EnumReader.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Extensions
{
    using Colossal.UI.Binding;

    public class EnumReader<T> : IReader<T>
    {
        public void Read(IJsonReader reader, out T value)
        {
            reader.Read(out int value2);
            value = (T)(object)value2;
        }
    }
}
