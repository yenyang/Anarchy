// <copyright file="NetworkDefinitionSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Anarchy.Systems.NetworkAnarchy
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Net;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    /// <summary>
    /// Overrides vertical position of creation definition.
    /// </summary>
    public partial class NetworkDefinitionSystem : GameSystemBase
    {
        public const float QuayThreshold = 4f;
        public const float RetainingWallThreshold = -4f;
        public const float TunnelThreshold = -25f;
        public const float ElevatedThreshold = 10f;
        public const float ForceGroundElevation = 0f;

        private ToolSystem m_ToolSystem;
        private NetToolSystem m_NetToolSystem;
        private PrefabSystem m_PrefabSystem;
        private NetworkAnarchyUISystem m_UISystem;
        private EntityQuery m_NetCourseQuery;
        private ILog m_Log;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkDefinitionSystem"/> class.
        /// </summary>
        public NetworkDefinitionSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_UISystem = World.GetOrCreateSystemManaged<NetworkAnarchyUISystem>();
            m_ToolSystem.EventToolChanged += (ToolBaseSystem tool) => Enabled = tool == m_NetToolSystem;
            m_Log.Info($"[{nameof(NetworkDefinitionSystem)}] {nameof(OnCreate)}");

            m_NetCourseQuery = SystemAPI.QueryBuilder()
                            .WithAllRW<NetCourse>()
                            .WithAll<CreationDefinition, Updated>()
                            .WithNone<Deleted, Overridden>()
                            .Build();
            RequireForUpdate(m_NetCourseQuery);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.ConstantSlope) != NetworkAnarchyUISystem.Composition.ConstantSlope
                && (m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Ground) != NetworkAnarchyUISystem.Composition.Ground
                && (m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Tunnel) != NetworkAnarchyUISystem.Composition.Tunnel
                && (m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Elevated) != NetworkAnarchyUISystem.Composition.Elevated)
            {
                return;
            }

            NativeArray<Entity> entities = m_NetCourseQuery.ToEntityArray(Allocator.Temp);

            // If not constant slope only check and set elevations for network composition mode.
            if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.ConstantSlope) != NetworkAnarchyUISystem.Composition.ConstantSlope
                || m_NetToolSystem.actualMode == NetToolSystem.Mode.Grid)
            {
                foreach (Entity entity in entities)
                {
                    if (EntityManager.TryGetComponent(entity, out NetCourse netCourse))
                    {
                        CheckAndSetElevations(ref netCourse);
                        EntityManager.SetComponentData(entity, netCourse);
                    }
                }

                return;
            }

            NativeHashMap<Entity, NetCourse> netCourses;
            NativeHashMap<Entity, NetCourse> parallelCourses;
            if (m_NetToolSystem.actualParallelCount <= 1)
            {
                netCourses = new (entities.Length, Allocator.Temp);
                parallelCourses = new (entities.Length, Allocator.Temp);
            }
            else
            {
                netCourses = new (entities.Length / 2, Allocator.Temp);
                parallelCourses = new (entities.Length / 2, Allocator.Temp);
            }

            m_Log.Debug($"{nameof(NetworkDefinitionSystem)}.{nameof(OnUpdate)} entities length = {entities.Length}.");

            CalculateSlope(entities, out float slope, out float parallelSlope, ref netCourses, ref parallelCourses);

            OrderNetCourses(netCourses, out NativeArray<Entity> netCourseEntities, out NativeArray<NetCourse> netCoursesArray);
            ProcessNetCourses(netCourseEntities, netCoursesArray, slope);
            if (parallelCourses.Count > 0)
            {
                OrderParallelNetCourses(parallelCourses, out NativeArray<Entity> parallelEntities, out NativeArray<NetCourse> parallelNetCourses);
                ProcessParallelNetCourses(parallelEntities, parallelNetCourses, parallelSlope);
            }
        }

        private void CalculateSlope(NativeArray<Entity> entities, out float slope, out float parallelSlope, ref NativeHashMap<Entity, NetCourse> netCourses, ref NativeHashMap<Entity, NetCourse> parallelCourses)
        {
            float totalLength = 0;
            float parallelLength = 0;
            NetCourse startCourse = default;
            NetCourse endCourse = default;
            NetCourse parallelStartCourse = default;
            NetCourse parallelEndCourse = default;
            slope = 0;
            parallelSlope = 0;

            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];

                if (!EntityManager.TryGetComponent(entity, out NetCourse netCourse))
                {
                    continue;
                }

                if ((netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsFirst) == CoursePosFlags.IsFirst && (netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsParallel) != CoursePosFlags.IsParallel)
                {
                    startCourse = netCourse;
#if VERBOSE
                    m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(CalculateSlope)} startCourse is {entity.Index}:{entity.Version}.");
#endif
                }
                else if ((netCourse.m_EndPosition.m_Flags & CoursePosFlags.IsLast) == CoursePosFlags.IsLast && (netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsParallel) != CoursePosFlags.IsParallel)
                {
                    endCourse = netCourse;
#if VERBOSE
                    m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(CalculateSlope)} endCourse is {entity.Index}:{entity.Version}.");
#endif
                }

                if ((netCourse.m_EndPosition.m_Flags & CoursePosFlags.IsFirst) == CoursePosFlags.IsFirst && (netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsParallel) == CoursePosFlags.IsParallel)
                {
                    parallelStartCourse = netCourse;
#if VERBOSE
                    m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(CalculateSlope)} parallelStartCourse is {entity.Index}:{entity.Version}.");
#endif
                }
                else if ((netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsLast) == CoursePosFlags.IsLast && (netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsParallel) == CoursePosFlags.IsParallel)
                {
                    parallelEndCourse = netCourse;
#if VERBOSE
                    m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(CalculateSlope)} parallelEndCourse is {entity.Index}:{entity.Version}.");
#endif
                }

                if ((netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsParallel) != CoursePosFlags.IsParallel)
                {
                    totalLength += netCourse.m_Length;
                }
                else
                {
                    parallelLength += netCourse.m_Length;
                }

                if ((netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsParallel) != CoursePosFlags.IsParallel)
                {
                    netCourses.Add(entity, netCourse);
                }
                else
                {
                    parallelCourses.Add(entity, netCourse);
                }
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(CalculateSlope)} current course start position ({netCourse.m_StartPosition.m_Position.x}, {netCourse.m_StartPosition.m_Position.y}, {netCourse.m_StartPosition.m_Position.z})");
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(CalculateSlope)} current course end position ({netCourse.m_EndPosition.m_Position.x}, {netCourse.m_EndPosition.m_Position.y}, {netCourse.m_EndPosition.m_Position.z})");
#endif
                if (totalLength > 0)
                {
                    slope = (endCourse.m_EndPosition.m_Position.y - startCourse.m_StartPosition.m_Position.y) / totalLength;
                }

                if (parallelLength > 0)
                {
                    parallelSlope = (parallelEndCourse.m_StartPosition.m_Position.y - parallelStartCourse.m_EndPosition.m_Position.y) / parallelLength;
                }
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(CalculateSlope)} slope {slope} or {slope * 100f}%");
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(CalculateSlope)} parallelSlope {parallelSlope} or {parallelSlope * 100f}%");
#endif
            }
        }

        private void OrderNetCourses(NativeHashMap<Entity, NetCourse> kVPairs, out NativeArray<Entity> entities, out NativeArray<NetCourse> netCourses)
        {
            entities = new NativeArray<Entity>(kVPairs.Count, Allocator.Temp);
            netCourses = new NativeArray<NetCourse>(kVPairs.Count, Allocator.Temp);

            foreach (KVPair<Entity, NetCourse> kVPair in kVPairs)
            {
                if ((kVPair.Value.m_StartPosition.m_Flags & CoursePosFlags.IsFirst) == CoursePosFlags.IsFirst )
                {
                    entities[0] = kVPair.Key;
                    netCourses[0] = kVPair.Value;
                    break;
                }
            }

            for (int i = 0; i < kVPairs.Count - 1; i++)
            {
                foreach (KVPair<Entity, NetCourse> kVPair in kVPairs)
                {
                    if (kVPair.Value.m_StartPosition.m_Position.x == netCourses[i].m_EndPosition.m_Position.x
                        && kVPair.Value.m_StartPosition.m_Position.y == netCourses[i].m_EndPosition.m_Position.y
                        && kVPair.Value.m_StartPosition.m_Position.z == netCourses[i].m_EndPosition.m_Position.z)
                    {
                        entities[i + 1] = kVPair.Key;
                        netCourses[i + 1] = kVPair.Value;
                        break;
                    }
                }
            }
        }

        private void OrderParallelNetCourses(NativeHashMap<Entity, NetCourse> kVPairs, out NativeArray<Entity> entities, out NativeArray<NetCourse> netCourses)
        {
            entities = new NativeArray<Entity>(kVPairs.Count, Allocator.Temp);
            netCourses = new NativeArray<NetCourse>(kVPairs.Count, Allocator.Temp);

            foreach (KVPair<Entity, NetCourse> kVPair in kVPairs)
            {
                if ((kVPair.Value.m_EndPosition.m_Flags & CoursePosFlags.IsFirst) == CoursePosFlags.IsFirst)
                {
                    entities[0] = kVPair.Key;
                    netCourses[0] = kVPair.Value;
                    break;
                }
            }

            for (int i = 0; i < kVPairs.Count - 1; i++)
            {
                foreach (KVPair<Entity, NetCourse> kVPair in kVPairs)
                {
                    if (kVPair.Value.m_EndPosition.m_Position.x == netCourses[i].m_StartPosition.m_Position.x
                        && kVPair.Value.m_EndPosition.m_Position.y == netCourses[i].m_StartPosition.m_Position.y
                        && kVPair.Value.m_EndPosition.m_Position.z == netCourses[i].m_StartPosition.m_Position.z)
                    {
                        entities[i + 1] = kVPair.Key;
                        netCourses[i + 1] = kVPair.Value;
                        break;
                    }
                }
            }
        }

        private void CheckAndSetElevations(ref NetCourse netCourse)
        {
            if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Ground) == NetworkAnarchyUISystem.Composition.Ground)
            {
                netCourse.m_StartPosition.m_Elevation = new float2(ForceGroundElevation, ForceGroundElevation);
                netCourse.m_EndPosition.m_Elevation = new float2(ForceGroundElevation, ForceGroundElevation);
                netCourse.m_Elevation = new float2(ForceGroundElevation, ForceGroundElevation);
            }
            else if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Tunnel) == NetworkAnarchyUISystem.Composition.Tunnel)
            {
                netCourse.m_StartPosition.m_Elevation.x = Mathf.Min(netCourse.m_StartPosition.m_Elevation.x, TunnelThreshold);
                netCourse.m_StartPosition.m_Elevation.y = Mathf.Min(netCourse.m_StartPosition.m_Elevation.y, TunnelThreshold);
                netCourse.m_EndPosition.m_Elevation.x = Mathf.Min(netCourse.m_StartPosition.m_Elevation.x, TunnelThreshold);
                netCourse.m_EndPosition.m_Elevation.y = Mathf.Min(netCourse.m_StartPosition.m_Elevation.y, TunnelThreshold);
                netCourse.m_Elevation.x = Mathf.Min(netCourse.m_Elevation.x, TunnelThreshold);
                netCourse.m_Elevation.y = Mathf.Min(netCourse.m_Elevation.y, TunnelThreshold);
            }
            else if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Elevated) == NetworkAnarchyUISystem.Composition.Elevated)
            {
                netCourse.m_StartPosition.m_Elevation.x = Mathf.Max(netCourse.m_StartPosition.m_Elevation.x, ElevatedThreshold);
                netCourse.m_StartPosition.m_Elevation.y = Mathf.Max(netCourse.m_StartPosition.m_Elevation.y, ElevatedThreshold);
                netCourse.m_EndPosition.m_Elevation.x = Mathf.Max(netCourse.m_StartPosition.m_Elevation.x, ElevatedThreshold);
                netCourse.m_EndPosition.m_Elevation.y = Mathf.Max(netCourse.m_StartPosition.m_Elevation.y, ElevatedThreshold);
                netCourse.m_Elevation.x = Mathf.Max(netCourse.m_Elevation.x, ElevatedThreshold);
                netCourse.m_Elevation.y = Mathf.Max(netCourse.m_Elevation.y, ElevatedThreshold);
            }
        }

        private void ProcessNetCourses(NativeArray<Entity> entities, NativeArray<NetCourse> netCourses, float slope)
        {
            for (int i = 0; i < netCourses.Length - 1; i++)
            {
                NetCourse currentCourse = netCourses[i];
                NetCourse nextCourse = netCourses[i + 1];
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(ProcessNetCourses)} current course end position ({currentCourse.m_EndPosition.m_Position.x}, {currentCourse.m_EndPosition.m_Position.y}, {currentCourse.m_EndPosition.m_Position.z})");
                m_Log.Verbose($"currentCourse.m_StartPosition.m_Flags = {currentCourse.m_StartPosition.m_Flags} currentCourse.m_EndPosition.m_Flags = {currentCourse.m_EndPosition.m_Flags} nextCourse.m_StartPosition.m_Flags = {nextCourse.m_StartPosition.m_Flags} nextCourse.m_EndPosition.m_Flags = {nextCourse.m_EndPosition.m_Flags}");
#endif
                currentCourse.m_EndPosition.m_Position.y = currentCourse.m_StartPosition.m_Position.y + (slope * currentCourse.m_Length);
                currentCourse.m_Curve.d.y = currentCourse.m_EndPosition.m_Position.y;
                currentCourse.m_Curve.b.y = currentCourse.m_Curve.a.y + (slope * Vector2.Distance(new float2(currentCourse.m_Curve.a.x, currentCourse.m_Curve.a.z), new float2(currentCourse.m_Curve.b.x, currentCourse.m_Curve.b.z)));
                currentCourse.m_Curve.c.y = currentCourse.m_Curve.a.y + (slope * Vector2.Distance(new float2(currentCourse.m_Curve.a.x, currentCourse.m_Curve.a.z), new float2(currentCourse.m_Curve.c.x, currentCourse.m_Curve.c.z)));

#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(ProcessNetCourses)} set y to {currentCourse.m_EndPosition.m_Position.y}.");
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(ProcessNetCourses)} currentCourse.m_EndPosition elevation is {currentCourse.m_EndPosition.m_Elevation}.");
#endif
                currentCourse.m_EndPosition.m_Elevation += currentCourse.m_EndPosition.m_Position.y - nextCourse.m_StartPosition.m_Position.y;
                currentCourse.m_Elevation = (currentCourse.m_StartPosition.m_Elevation + currentCourse.m_EndPosition.m_Elevation) / 2f;
                currentCourse.m_EndPosition.m_Flags |= CoursePosFlags.FreeHeight;

                CheckAndSetElevations(ref currentCourse);
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(ProcessNetCourses)} set currentCourse.m_EndPosition elevation to {currentCourse.m_EndPosition.m_Elevation}.");
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(ProcessNetCourses)} set course elevation to {currentCourse.m_Elevation}.");
#endif
                nextCourse.m_StartPosition.m_Position.y = currentCourse.m_EndPosition.m_Position.y;
                nextCourse.m_Curve.a.y = currentCourse.m_EndPosition.m_Position.y;
                nextCourse.m_StartPosition.m_Elevation = currentCourse.m_EndPosition.m_Elevation;

                netCourses[i] = currentCourse;
                EntityManager.SetComponentData(entities[i], netCourses[i]);
                netCourses[i + 1] = nextCourse;

                if ((nextCourse.m_EndPosition.m_Flags & CoursePosFlags.IsLast) == CoursePosFlags.IsLast)
                {
                    if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.ConstantSlope) == NetworkAnarchyUISystem.Composition.ConstantSlope)
                    {
                        nextCourse.m_Curve.b.y = nextCourse.m_Curve.a.y + (slope * Vector2.Distance(new float2(nextCourse.m_Curve.a.x, nextCourse.m_Curve.a.z), new float2(nextCourse.m_Curve.b.x, nextCourse.m_Curve.b.z)));
                        nextCourse.m_Curve.c.y = nextCourse.m_Curve.a.y + (slope * Vector2.Distance(new float2(nextCourse.m_Curve.a.x, nextCourse.m_Curve.a.z), new float2(nextCourse.m_Curve.c.x, nextCourse.m_Curve.c.z)));
                    }

                    CheckAndSetElevations(ref nextCourse);

                    EntityManager.SetComponentData(entities[i + 1], nextCourse);
                }
            }
        }

        private void ProcessParallelNetCourses(NativeArray<Entity> entities, NativeArray<NetCourse> netCourses, float slope)
        {
            for (int i = 0; i < netCourses.Length - 1; i++)
            {
                NetCourse currentCourse = netCourses[i];
                NetCourse nextCourse = netCourses[i + 1];
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(ProcessNetCourses)} current course end position ({currentCourse.m_EndPosition.m_Position.x}, {currentCourse.m_EndPosition.m_Position.y}, {currentCourse.m_EndPosition.m_Position.z})");
                m_Log.Verbose($"currentCourse.m_StartPosition.m_Flags = {currentCourse.m_StartPosition.m_Flags} currentCourse.m_EndPosition.m_Flags = {currentCourse.m_EndPosition.m_Flags} nextCourse.m_StartPosition.m_Flags = {nextCourse.m_StartPosition.m_Flags} nextCourse.m_EndPosition.m_Flags = {nextCourse.m_EndPosition.m_Flags}");
#endif
                currentCourse.m_StartPosition.m_Position.y = currentCourse.m_EndPosition.m_Position.y + (slope * currentCourse.m_Length);
                currentCourse.m_Curve.a.y = currentCourse.m_StartPosition.m_Position.y;
                currentCourse.m_Curve.b.y = currentCourse.m_Curve.d.y + (slope * Vector2.Distance(new float2(currentCourse.m_Curve.d.x, currentCourse.m_Curve.d.z), new float2(currentCourse.m_Curve.b.x, currentCourse.m_Curve.b.z)));
                currentCourse.m_Curve.c.y = currentCourse.m_Curve.d.y + (slope * Vector2.Distance(new float2(currentCourse.m_Curve.d.x, currentCourse.m_Curve.d.z), new float2(currentCourse.m_Curve.c.x, currentCourse.m_Curve.c.z)));

#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(ProcessNetCourses)} set y to {currentCourse.m_StartPosition.m_Position.y}.");
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(ProcessNetCourses)} currentCourse.m_EndPosition elevation is {currentCourse.m_StartPosition.m_Elevation}.");
#endif
                currentCourse.m_StartPosition.m_Elevation += currentCourse.m_StartPosition.m_Position.y - nextCourse.m_EndPosition.m_Position.y;
                currentCourse.m_Elevation = (currentCourse.m_StartPosition.m_Elevation + currentCourse.m_EndPosition.m_Elevation) / 2f;
                currentCourse.m_StartPosition.m_Flags |= CoursePosFlags.FreeHeight;

                CheckAndSetElevations(ref currentCourse);
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(ProcessNetCourses)} set currentCourse.m_EndPosition elevation to {currentCourse.m_StartPosition.m_Elevation}.");
                m_Log.Verbose($"{nameof(NetworkDefinitionSystem)}.{nameof(ProcessNetCourses)} set course elevation to {currentCourse.m_Elevation}.");
#endif
                nextCourse.m_EndPosition.m_Position.y = currentCourse.m_StartPosition.m_Position.y;
                nextCourse.m_Curve.d.y = currentCourse.m_StartPosition.m_Position.y;
                nextCourse.m_EndPosition.m_Elevation = currentCourse.m_StartPosition.m_Elevation;

                netCourses[i] = currentCourse;
                EntityManager.SetComponentData(entities[i], netCourses[i]);
                netCourses[i + 1] = nextCourse;

                if ((nextCourse.m_StartPosition.m_Flags & CoursePosFlags.IsLast) == CoursePosFlags.IsLast)
                {
                    if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.ConstantSlope) == NetworkAnarchyUISystem.Composition.ConstantSlope)
                    {
                        nextCourse.m_Curve.b.y = nextCourse.m_Curve.d.y + (slope * Vector2.Distance(new float2(nextCourse.m_Curve.d.x, nextCourse.m_Curve.d.z), new float2(nextCourse.m_Curve.b.x, nextCourse.m_Curve.b.z)));
                        nextCourse.m_Curve.c.y = nextCourse.m_Curve.d.y + (slope * Vector2.Distance(new float2(nextCourse.m_Curve.d.x, nextCourse.m_Curve.d.z), new float2(nextCourse.m_Curve.c.x, nextCourse.m_Curve.c.z)));
                    }

                    CheckAndSetElevations(ref nextCourse);

                    EntityManager.SetComponentData(entities[i + 1], nextCourse);
                }
            }
        }
    }
}
