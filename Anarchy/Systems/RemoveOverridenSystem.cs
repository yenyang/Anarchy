// <copyright file="RemoveOverridenSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using System.Collections.Generic;
    using Anarchy;
    using Anarchy.Tooltip;
    using Colossal.Logging;
    using Game;
    using Game.Buildings;
    using Game.Common;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Entities;

    /// <summary>
    /// A system that prevents objects from being overriden when placed on each other.
    /// </summary>
    public partial class RemoveOverridenSystem : GameSystemBase
    {
        private readonly List<string> m_AppropriateToolsWithAnarchy = new List<string>()
        {
            { "Object Tool" },
            { "Line Tool" },
        };

        private readonly List<string> m_AppropriateTools = new List<string>()
        {
            { "Bulldoze Tool" },
            { "Default Tool" },
        };

        private AnarchySystem m_AnarchySystem;
        private ILog m_Log;
        private ToolSystem m_ToolSystem;
        private NetToolSystem m_NetToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private EntityQuery m_OwnedAndOverridenQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveOverridenSystem"/> class.
        /// </summary>
        public RemoveOverridenSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Logger;
            m_Log.Info($"{nameof(RemoveOverridenSystem)} Created.");
            m_AnarchySystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<AnarchySystem>();
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_NetToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<NetToolSystem>();
            m_ObjectToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_PrefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            m_OwnedAndOverridenQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
               {
                    ComponentType.ReadOnly<Updated>(),
                    ComponentType.ReadOnly<Overridden>(),
                    ComponentType.ReadOnly<Owner>(),
               },
                None = new ComponentType[]
               {
                    ComponentType.ReadOnly<Temp>(),
                    ComponentType.ReadOnly<Building>(),
                    ComponentType.ReadOnly<Game.Objects.Crane>(),
               },
            });
            RequireForUpdate(m_OwnedAndOverridenQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.activeTool.toolID == null || m_ToolSystem.actionMode.IsEditor())
            {
                return;
            }

            if (m_ToolSystem.activePrefab != null)
            {
                Entity prefabEntity = m_PrefabSystem.GetEntity(m_ToolSystem.activePrefab);
                if (EntityManager.HasComponent<BuildingData>(prefabEntity))
                {
                    return;
                }
            }

            if (m_NetToolSystem.TrySetPrefab(m_ToolSystem.activePrefab))
            {
                return;
            }

            if (m_AppropriateTools.Contains(m_ToolSystem.activeTool.toolID) || (m_AppropriateToolsWithAnarchy.Contains(m_ToolSystem.activeTool.toolID) && m_AnarchySystem.AnarchyEnabled))
            {
                EntityManager.RemoveComponent(m_OwnedAndOverridenQuery, ComponentType.ReadWrite<Overridden>());
            }
        }
    }
}
