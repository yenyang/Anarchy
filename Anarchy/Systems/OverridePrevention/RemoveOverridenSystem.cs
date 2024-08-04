// <copyright file="RemoveOverridenSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.OverridePrevention
{
    using System.Collections.Generic;
    using Anarchy;
    using Anarchy.Components;
    using Anarchy.Systems.Common;
    using Colossal.Logging;
    using Game;
    using Game.Buildings;
    using Game.Citizens;
    using Game.Common;
    using Game.Creatures;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Tools;
    using Game.Vehicles;
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
            { "Net Tool" },
        };

        private readonly List<string> m_AppropriateTools = new List<string>()
        {
            { "Bulldoze Tool" },
            { "Default Tool" },
        };

        private AnarchyUISystem m_AnarchyUISystem;
        private ILog m_Log;
        private ToolSystem m_ToolSystem;
        private NetToolSystem m_NetToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private EntityQuery m_OwnedAndOverridenQuery;
        private EntityQuery m_HasAnarchyAndUpdatedQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveOverridenSystem"/> class.
        /// </summary>
        public RemoveOverridenSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(RemoveOverridenSystem)} Created.");
            m_AnarchyUISystem = World.GetOrCreateSystemManaged<AnarchyUISystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
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
                    ComponentType.ReadOnly<Crane>(),
                    ComponentType.ReadOnly<Animal>(),
                    ComponentType.ReadOnly<Game.Creatures.Pet>(),
                    ComponentType.ReadOnly<Creature>(),
                    ComponentType.ReadOnly<Moving>(),
                    ComponentType.ReadOnly<Household>(),
                    ComponentType.ReadOnly<Vehicle>(),
                    ComponentType.ReadOnly<Event>(),
               },
            });

            m_HasAnarchyAndUpdatedQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
               {
                    ComponentType.ReadOnly<Updated>(),
                    ComponentType.ReadOnly<Overridden>(),
                    ComponentType.ReadOnly<PreventOverride>(),
               },
                None = new ComponentType[]
               {
                    ComponentType.ReadOnly<Owner>(),
                    ComponentType.ReadOnly<Temp>(),
                    ComponentType.ReadOnly<Building>(),
                    ComponentType.ReadOnly<Crane>(),
                    ComponentType.ReadOnly<Animal>(),
                    ComponentType.ReadOnly<Game.Creatures.Pet>(),
                    ComponentType.ReadOnly<Creature>(),
                    ComponentType.ReadOnly<Moving>(),
                    ComponentType.ReadOnly<Household>(),
                    ComponentType.ReadOnly<Vehicle>(),
                    ComponentType.ReadOnly<Event>(),
               },
            });

            RequireForUpdate(m_OwnedAndOverridenQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.activeTool.toolID == null || m_ToolSystem.actionMode.IsEditor() && !AnarchyMod.Instance.Settings.PreventOverrideInEditor || m_ToolSystem.activePrefab == null)
            {
                return;
            }

            if (m_NetToolSystem.TrySetPrefab(m_ToolSystem.activePrefab) && m_ToolSystem.activePrefab is not NetLaneGeometryPrefab && m_ToolSystem.activePrefab is not NetLanePrefab)
            {
                return;
            }

            if (m_AppropriateTools.Contains(m_ToolSystem.activeTool.toolID)
                || m_AppropriateToolsWithAnarchy.Contains(m_ToolSystem.activeTool.toolID) && m_AnarchyUISystem.AnarchyEnabled
                || !m_HasAnarchyAndUpdatedQuery.IsEmptyIgnoreFilter)
            {
                EntityManager.RemoveComponent(m_OwnedAndOverridenQuery, ComponentType.ReadWrite<Overridden>());
                m_Log.Debug($"{nameof(RemoveOverridenSystem)}.{nameof(OnUpdate)}");
            }
        }
    }
}
