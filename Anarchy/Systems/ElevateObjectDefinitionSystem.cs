// <copyright file="ObjectDefinitionSystem.cs" company="Yenyang's Mods. MIT License">
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
    using UnityEngine;

    /// <summary>
    /// Overrides vertical position of creation definition.
    /// </summary>
    public partial class ElevateObjectDefinitionSystem : GameSystemBase
    {
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private AnarchyUISystem m_AnarchyUISystem;
        private EntityQuery m_ObjectDefinitionQuery;
        private ILog m_Log;
        private ElevateTempObjectSystem m_ElevateTempObjectSystem;
        private float m_ElevationDelta;

        /// <summary>
        /// Sets the elevation delta.
        /// </summary>
        public float ElevationDelta
        {
            set { m_ElevationDelta = value; }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_ElevateTempObjectSystem = World.GetOrCreateSystemManaged<ElevateTempObjectSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_AnarchyUISystem = World.CreateSystemManaged<AnarchyUISystem>();
            m_Log.Info($"[{nameof(ElevateObjectDefinitionSystem)}] {nameof(OnCreate)}");
            m_ObjectDefinitionQuery = SystemAPI.QueryBuilder()
                .WithAllRW<Game.Tools.ObjectDefinition>()
                .WithAll<CreationDefinition, Updated>()
                .WithNone<Deleted, Overridden>()
                .Build();

            RequireForUpdate(m_ObjectDefinitionQuery);
        }


        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if ((m_ToolSystem.activeTool != m_ObjectToolSystem && m_ToolSystem.activeTool.toolID != "Line Tool") || !AnarchyMod.Instance.Settings.ShowElevationToolOption)
            {
                return;
            }

            if (m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Create && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush)
            {
                return;
            }

            NativeArray<Entity> entities = m_ObjectDefinitionQuery.ToEntityArray(Allocator.Temp);

            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out CreationDefinition currentCreationDefinition))
                {
                    continue;
                }

                if (!EntityManager.TryGetComponent(entity, out ObjectDefinition currentObjectDefinition))
                {
                    continue;
                }

                if (!m_PrefabSystem.TryGetPrefab(currentCreationDefinition.m_Prefab, out PrefabBase prefabBase))
                {
                    continue;
                }

                if (prefabBase is not BuildingPrefab)
                {
                    if (!EntityManager.HasComponent<StackData>(currentCreationDefinition.m_Prefab))
                    {
                        currentObjectDefinition.m_Elevation = Mathf.Max(m_ElevationDelta, 0);
                        currentObjectDefinition.m_Position.y += m_ElevationDelta;
                    }
                    else
                    {
                        currentObjectDefinition.m_Position.y += m_ElevationDelta;
                    }

                    EntityManager.SetComponentData(entity, currentObjectDefinition);
                }
            }

            entities.Dispose();

            m_ElevateTempObjectSystem.Enabled = false;
        }

    }
}
