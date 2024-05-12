// <copyright file="NetworkGradeDefinitionSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Entities.UniversalDelegates;
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
        private AnarchyUISystem m_AnarchyUISystem;
        private EntityQuery m_NetCourseQuery;
        private ILog m_Log;
        private ElevateTempObjectSystem m_ElevateTempObjectSystem;

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
            m_ElevateTempObjectSystem = World.GetOrCreateSystemManaged<ElevateTempObjectSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_AnarchyUISystem = World.CreateSystemManaged<AnarchyUISystem>();
            m_Log.Info($"[{nameof(ElevateObjectDefinitionSystem)}] {nameof(OnCreate)}");
        }


        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            m_NetCourseQuery = SystemAPI.QueryBuilder()
                .WithAllRW<NetCourse>()
                .WithAll<CreationDefinition, Updated>()
                .WithNone<Deleted, Overridden>()
                .Build();

            RequireForUpdate(m_NetCourseQuery);

            if (m_ToolSystem.activeTool != m_NetToolSystem)
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

                if (m_NetToolSystem.actualMode == NetToolSystem.Mode.Straight)
                {
                    if ((netCourse.m_StartPosition.m_Flags & CoursePosFlags.IsFirst) == CoursePosFlags.IsFirst)
                    {
                        startCourse = netCourse;
                        m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} startCourse is {entity.Index}:{entity.Version}.");
                    }
                    else if ((netCourse.m_EndPosition.m_Flags & CoursePosFlags.IsLast) == CoursePosFlags.IsLast)
                    {
                        endCourse = netCourse;
                        m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} endCourse is {entity.Index}:{entity.Version}.");
                    }
                    else
                    {
                        // middleCoursesList.Add(netCourse);
                        m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} middle course added {entity.Index}:{entity.Version}.");
                    }

                    totalLength += netCourse.m_Length;
                    netCourses[i] = netCourse;
                    m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} current course start position ({netCourse.m_StartPosition.m_Position.x}, {netCourse.m_StartPosition.m_Position.y}, {netCourse.m_StartPosition.m_Position.z})");
                }
            }

            m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} current end position ({netCourses[entities.Length - 1].m_EndPosition.m_Position.x}, {netCourses[entities.Length - 1].m_EndPosition.m_Position.y}, {netCourses[entities.Length - 1].m_EndPosition.m_Position.z})");

            /*
            m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} courses identified. middleCoursesList.Length {middleCoursesList.Length}");

            float3 currentPosition = startCourse.m_EndPosition.m_Position;
            for (int i = 0; i < middleCoursesList.Length; i++)
            {
                for (int j = 0; j < middleCoursesList.Length; j++)
                {
                    NetCourse currentCourse = middleCoursesList.ElementAt(j);
                    if (currentCourse.m_StartPosition.m_Position.Equals(currentPosition))
                    {
                        orderedNetCourses.Add(currentCourse);
                        currentPosition = currentCourse.m_EndPosition.m_Position;
                        m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} found nextCourse.");
                        break;
                    }
                }
            }
            orderedNetCourses.Add(endCourse);
            m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} courses ordered {orderedNetCourses.Length}.");
            */

            float slope = (netCourses[entities.Length - 1].m_EndPosition.m_Position.y - netCourses[0].m_StartPosition.m_Position.y) / totalLength;
            m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} slope {slope} or {slope * 100f}%");
            for (int i = 0; i < entities.Length - 1; i++)
            {
                NetCourse currentCourse = netCourses[i];
                m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} current course end position ({currentCourse.m_EndPosition.m_Position.x}, {currentCourse.m_EndPosition.m_Position.y}, {currentCourse.m_EndPosition.m_Position.z})");
                currentCourse.m_EndPosition.m_Position.y = currentCourse.m_StartPosition.m_Position.y + (slope * currentCourse.m_Length);
                m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} set y to {currentCourse.m_EndPosition.m_Position.y}.");
                NetCourse nextCourse = netCourses[i + 1];
                m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} currentCourse.m_EndPosition elevation is {currentCourse.m_EndPosition.m_Elevation}.");
                currentCourse.m_EndPosition.m_Elevation -= currentCourse.m_EndPosition.m_Position.y - nextCourse.m_StartPosition.m_Position.y;
                m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} set currentCourse.m_EndPosition elevation to {currentCourse.m_EndPosition.m_Elevation}.");
                nextCourse.m_StartPosition.m_Position.y = currentCourse.m_EndPosition.m_Position.y;
                nextCourse.m_StartPosition.m_Elevation = currentCourse.m_EndPosition.m_Elevation;
                currentCourse.m_Elevation = (currentCourse.m_StartPosition.m_Elevation + currentCourse.m_EndPosition.m_Elevation) / 2f;
                currentCourse.m_EndPosition.m_Flags |= CoursePosFlags.FreeHeight;
                m_Log.Debug($"{nameof(NetworkGradeDefinitionSystem)}.{nameof(OnUpdate)} set course elevation to {currentCourse.m_EndPosition.m_Elevation}.");
                netCourses[i] = currentCourse;

                EntityManager.SetComponentData(entities[i], netCourses[i]);
                netCourses[i + 1] = nextCourse;
            }

            EntityManager.SetComponentData(entities[entities.Length - 1], netCourses[entities.Length - 1]);
        }

    }
}
