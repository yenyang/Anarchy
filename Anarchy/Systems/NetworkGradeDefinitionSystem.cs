// <copyright file="NetworkGradeDefinitionSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

#define VERBOSE
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
    public partial class NetworkGradeDefinitionSystem : GameSystemBase
    {
        private ToolSystem m_ToolSystem;
        private NetToolSystem m_NetToolSystem;
        private PrefabSystem m_PrefabSystem;
        private NetworkAnarchyUISystem m_UISystem;
        private EntityQuery m_NetCourseQuery;
        private ILog m_Log;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkGradeDefinitionSystem"/> class.
        /// </summary>
        public NetworkGradeDefinitionSystem()
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
            m_Log.Info($"[{nameof(NetworkGradeDefinitionSystem)}] {nameof(OnCreate)}");

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
            if ((m_UISystem.NetworkComposition & NetworkAnarchyUISystem.Composition.ConstantSlope) != NetworkAnarchyUISystem.Composition.ConstantSlope)
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

            m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} entities length = {entities.Length}.");
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

                if ((netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsFirst) == CoursePosFlags.IsFirst)
                {
                    startCourse = netCourse;
#if VERBOSE
                    m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} startCourse is {entity.Index}:{entity.Version}.");
#endif
                }
                else if ((netCourse.m_EndPosition.m_Flags & CoursePosFlags.IsLast) == CoursePosFlags.IsLast)
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
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} current course end position ({currentCourse.m_EndPosition.m_Position.x}, {currentCourse.m_EndPosition.m_Position.y}, {currentCourse.m_EndPosition.m_Position.z})");
#endif
                currentCourse.m_EndPosition.m_Position.y = currentCourse.m_StartPosition.m_Position.y + (slope * currentCourse.m_Length);
                currentCourse.m_Curve.d.y = currentCourse.m_EndPosition.m_Position.y;
                currentCourse.m_Curve.b.y = currentCourse.m_Curve.a.y + (slope * Vector2.Distance(new float2(currentCourse.m_Curve.a.x, currentCourse.m_Curve.a.z), new float2(currentCourse.m_Curve.b.x, currentCourse.m_Curve.b.z)));
                currentCourse.m_Curve.c.y = currentCourse.m_Curve.a.y + (slope * Vector2.Distance(new float2(currentCourse.m_Curve.a.x, currentCourse.m_Curve.a.z), new float2(currentCourse.m_Curve.c.x, currentCourse.m_Curve.c.z)));
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} set y to {currentCourse.m_EndPosition.m_Position.y}.");
#endif
                NetCourse nextCourse = netCourses[i + 1];
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} currentCourse.m_EndPosition elevation is {currentCourse.m_EndPosition.m_Elevation}.");
#endif
                currentCourse.m_EndPosition.m_Elevation += currentCourse.m_EndPosition.m_Position.y - nextCourse.m_StartPosition.m_Position.y;
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} set currentCourse.m_EndPosition elevation to {currentCourse.m_EndPosition.m_Elevation}.");
#endif
                nextCourse.m_StartPosition.m_Position.y = currentCourse.m_EndPosition.m_Position.y;
                nextCourse.m_Curve.a.y = currentCourse.m_EndPosition.m_Position.y;
                nextCourse.m_StartPosition.m_Elevation = currentCourse.m_EndPosition.m_Elevation;
                currentCourse.m_Elevation = (currentCourse.m_StartPosition.m_Elevation + currentCourse.m_EndPosition.m_Elevation) / 2f;
                currentCourse.m_EndPosition.m_Flags |= CoursePosFlags.FreeHeight;
#if VERBOSE
                m_Log.Verbose($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} set course elevation to {currentCourse.m_Elevation}.");
#endif
                netCourses[i] = currentCourse;

                EntityManager.SetComponentData(entities[i], netCourses[i]);
                netCourses[i + 1] = nextCourse;
            }

            NetCourse finalCourse = netCourses[entities.Length - 1];
            finalCourse.m_Curve.b.y = finalCourse.m_Curve.a.y + (slope * Vector2.Distance(new float2(finalCourse.m_Curve.a.x, finalCourse.m_Curve.a.z), new float2(finalCourse.m_Curve.b.x, finalCourse.m_Curve.b.z)));
            finalCourse.m_Curve.c.y = finalCourse.m_Curve.a.y + (slope * Vector2.Distance(new float2(finalCourse.m_Curve.a.x, finalCourse.m_Curve.a.z), new float2(finalCourse.m_Curve.c.x, finalCourse.m_Curve.c.z)));
            EntityManager.SetComponentData(entities[entities.Length - 1], finalCourse);
        }
    }
}
