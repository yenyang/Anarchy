// <copyright file="NetworkGradeDefinitionSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Anarchy.Systems
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
                && (m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Tunnel) != NetworkAnarchyUISystem.Composition.Tunnel)
            {
                return;
            }

            NativeArray<Entity> entities = m_NetCourseQuery.ToEntityArray(Allocator.Temp);
            NetCourse startCourse = default;
            NetCourse endCourse = default;
            NativeArray<NetCourse> netCourses = new (entities.Length, Allocator.Temp);
            float totalLength = 0f;
            if (entities.Length < 2)
            {
                return;
            }

            m_Log.Debug($"{nameof(NetworkDefinitionSystem)}.{nameof(OnUpdate)} entities length = {entities.Length}.");
            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                if (!EntityManager.TryGetComponent(entity, out CreationDefinition currentCreationDefinition))
                {
                    continue;
                }

                if (!EntityManager.TryGetComponent(entity, out NetCourse netCourse))
                {
                    continue;
                }

                if (!m_PrefabSystem.TryGetPrefab(currentCreationDefinition.m_Prefab, out PrefabBase prefabBase))
                {
                    continue;
                }

                if ((netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsFirst) == CoursePosFlags.IsFirst && (netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsParallel) != CoursePosFlags.IsParallel)
                {
                    startCourse = netCourse;
#if VERBOSE
                    m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} startCourse is {entity.Index}:{entity.Version}.");
#endif
                }
                else if ((netCourse.m_EndPosition.m_Flags & CoursePosFlags.IsLast) == CoursePosFlags.IsLast && (netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsParallel) != CoursePosFlags.IsParallel)
                {
                    endCourse = netCourse;
#if VERBOSE
                    m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} endCourse is {entity.Index}:{entity.Version}.");
#endif
                }

                totalLength += netCourse.m_Length;
                netCourses[i] = netCourse;

#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} current course start position ({netCourse.m_StartPosition.m_Position.x}, {netCourse.m_StartPosition.m_Position.y}, {netCourse.m_StartPosition.m_Position.z})");
#endif
            }

#if VERBOSE
            m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} current end position ({netCourses[entities.Length - 1].m_EndPosition.m_Position.x}, {netCourses[entities.Length - 1].m_EndPosition.m_Position.y}, {netCourses[entities.Length - 1].m_EndPosition.m_Position.z})");
#endif
            float slope = (netCourses[entities.Length - 1].m_EndPosition.m_Position.y - netCourses[0].m_StartPosition.m_Position.y) / totalLength;
#if VERBOSE
            m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} slope {slope} or {slope * 100f}%");
#endif
            for (int i = 0; i < entities.Length - 1; i++)
            {
                NetCourse currentCourse = netCourses[i];
                NetCourse nextCourse = netCourses[i + 1];
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} current course end position ({currentCourse.m_EndPosition.m_Position.x}, {currentCourse.m_EndPosition.m_Position.y}, {currentCourse.m_EndPosition.m_Position.z})");
#endif
                if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.ConstantSlope) == NetworkAnarchyUISystem.Composition.ConstantSlope)
                {
                    currentCourse.m_EndPosition.m_Position.y = currentCourse.m_StartPosition.m_Position.y + (slope * currentCourse.m_Length);
                    currentCourse.m_Curve.d.y = currentCourse.m_EndPosition.m_Position.y;
                    currentCourse.m_Curve.b.y = currentCourse.m_Curve.a.y + (slope * Vector2.Distance(new float2(currentCourse.m_Curve.a.x, currentCourse.m_Curve.a.z), new float2(currentCourse.m_Curve.b.x, currentCourse.m_Curve.b.z)));
                    currentCourse.m_Curve.c.y = currentCourse.m_Curve.a.y + (slope * Vector2.Distance(new float2(currentCourse.m_Curve.a.x, currentCourse.m_Curve.a.z), new float2(currentCourse.m_Curve.c.x, currentCourse.m_Curve.c.z)));

#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} set y to {currentCourse.m_EndPosition.m_Position.y}.");
                m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} currentCourse.m_EndPosition elevation is {currentCourse.m_EndPosition.m_Elevation}.");
#endif
                    currentCourse.m_EndPosition.m_Elevation += currentCourse.m_EndPosition.m_Position.y - nextCourse.m_StartPosition.m_Position.y;
                    currentCourse.m_Elevation = (currentCourse.m_StartPosition.m_Elevation + currentCourse.m_EndPosition.m_Elevation) / 2f;
                    currentCourse.m_EndPosition.m_Flags |= CoursePosFlags.FreeHeight;
                }

                CheckAndSetElevations(ref currentCourse);
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} set currentCourse.m_EndPosition elevation to {currentCourse.m_EndPosition.m_Elevation}.");
                m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} set course elevation to {currentCourse.m_Elevation}.");
#endif
                if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.ConstantSlope) == NetworkAnarchyUISystem.Composition.ConstantSlope)
                {
                    nextCourse.m_StartPosition.m_Position.y = currentCourse.m_EndPosition.m_Position.y;
                    nextCourse.m_Curve.a.y = currentCourse.m_EndPosition.m_Position.y;
                    nextCourse.m_StartPosition.m_Elevation = currentCourse.m_EndPosition.m_Elevation;
                }

                netCourses[i] = currentCourse;
                EntityManager.SetComponentData(entities[i], netCourses[i]);
                netCourses[i + 1] = nextCourse;
            }

            NetCourse finalCourse = netCourses[entities.Length - 1];
            if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.ConstantSlope) == NetworkAnarchyUISystem.Composition.ConstantSlope)
            {
                finalCourse.m_Curve.b.y = finalCourse.m_Curve.a.y + (slope * Vector2.Distance(new float2(finalCourse.m_Curve.a.x, finalCourse.m_Curve.a.z), new float2(finalCourse.m_Curve.b.x, finalCourse.m_Curve.b.z)));
                finalCourse.m_Curve.c.y = finalCourse.m_Curve.a.y + (slope * Vector2.Distance(new float2(finalCourse.m_Curve.a.x, finalCourse.m_Curve.a.z), new float2(finalCourse.m_Curve.c.x, finalCourse.m_Curve.c.z)));
            }

            CheckAndSetElevations(ref finalCourse);

            EntityManager.SetComponentData(entities[entities.Length - 1], finalCourse);
        }

        private void CheckAndSetElevations(ref NetCourse netCourse)
        {
            if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Ground) == NetworkAnarchyUISystem.Composition.Ground)
            {
                netCourse.m_StartPosition.m_Elevation = new float2(0, 0);
                netCourse.m_EndPosition.m_Elevation = new float2(0, 0);
                netCourse.m_Elevation = new float2(0, 0);
            }
            else if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.Tunnel) == NetworkAnarchyUISystem.Composition.Tunnel)
            {
                netCourse.m_StartPosition.m_Elevation.x = Mathf.Min(netCourse.m_StartPosition.m_Elevation.x, -25f);
                netCourse.m_StartPosition.m_Elevation.y = Mathf.Min(netCourse.m_StartPosition.m_Elevation.y, -25f);
                netCourse.m_EndPosition.m_Elevation.x = Mathf.Min(netCourse.m_StartPosition.m_Elevation.x, -25f);
                netCourse.m_EndPosition.m_Elevation.y = Mathf.Min(netCourse.m_StartPosition.m_Elevation.y, -25f);
                netCourse.m_Elevation.x = Mathf.Min(netCourse.m_Elevation.x, -25f);
                netCourse.m_Elevation.y = Mathf.Min(netCourse.m_Elevation.y, -25f);
            }
        }
    }
}
