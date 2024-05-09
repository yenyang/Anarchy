// <copyright file="TransformRecord.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Components
{
    using Colossal.Serialization.Entities;
    using Game.Objects;
    using Unity.Entities;
    using Unity.Mathematics;

    /// <summary>
    /// A component used to record where an object was placed so that it doesn't drop by accident.
    /// </summary>
    public struct TransformRecord : IComponentData, IQueryTypeParameter, ISerializable
    {
        /// <summary>
        /// The position record from original transform.
        /// </summary>
        public float3 m_Position;

        /// <summary>
        /// The rotation record from orginal transform.
        /// </summary>
        public quaternion m_Rotation;

        /// <summary>
        /// Evaluates equualitiy between a transform record and a transform.
        /// </summary>
        /// <param name="other">A transform struct.</param>
        /// <returns>True if transform record matches the transform.</returns>
        public bool Equals(Transform other)
        {
            if (m_Position.Equals(other.m_Position))
            {
                return m_Rotation.Equals(other.m_Rotation);
            }

            return false;
        }
        
        /// <summary>
        /// Serializes the transform record.
        /// </summary>
        /// <typeparam name="TWriter">Part of serialization.</typeparam>
        /// <param name="writer">Part of serialization writing.</param>
        public void Serialize<TWriter>(TWriter writer)
            where TWriter : IWriter
        {
            writer.Write(m_Position);
            writer.Write(m_Rotation);
        }

        /// <summary>
        /// Deserializes the transform record.
        /// </summary>
        /// <typeparam name="TReader">Part of deserialization.</typeparam>
        /// <param name="reader">Reader for deserialization.</param>
        public void Deserialize<TReader>(TReader reader)
            where TReader : IReader
        {
            reader.Read(out m_Position);
            reader.Read(out m_Rotation);
            if (!math.all(m_Position >= -100000f) || !math.all(m_Position <= 100000f) || !math.all(math.isfinite(m_Rotation.value)) || math.all(m_Rotation.value == 0f))
            {
                m_Position = default(float3);
                m_Rotation = quaternion.identity;
            }
        }
    }
}