﻿// <copyright file="PreventOverrideSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.OverridePrevention
{
    using Anarchy;
    using Anarchy.Components;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Tools;
    using Unity.Entities;

    /// <summary>
    /// A system that prevents objects from being overriden that has a custom component.
    /// </summary>
    public partial class PreventOverrideSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_NeedToPreventOverrideQuery;
        private ToolSystem m_ToolSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreventOverrideSystem"/> class.
        /// </summary>
        public PreventOverrideSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(PreventOverrideSystem)} Created.");
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_NeedToPreventOverrideQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
               {
                    ComponentType.ReadOnly<PreventOverride>(),
                    ComponentType.ReadWrite<Overridden>(),
               },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<Deleted>(),
                },
            });
            RequireForUpdate(m_NeedToPreventOverrideQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.actionMode.IsEditor() && !AnarchyMod.Instance.Settings.PreventOverrideInEditor)
            {
                return;
            }

            EntityManager.RemoveComponent(m_NeedToPreventOverrideQuery, ComponentType.ReadOnly<Overridden>());
            EntityManager.AddComponent(m_NeedToPreventOverrideQuery, ComponentType.ReadOnly<Updated>());
        }
    }
}
