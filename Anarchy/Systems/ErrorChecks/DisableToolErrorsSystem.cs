// <copyright file="DisableToolErrorsSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Anarchy.Systems.ErrorChecks
{
    using Anarchy.Systems.Common;
    using System.Collections.Generic;
    using Anarchy;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;
    using Game.Common;

    /// <summary>
    /// A system the queries for toolErrorPrefabs and then disables relevent tool errors in game if active tool is applicable.
    /// </summary>
    public partial class DisableToolErrorsSystem : GameSystemBase
    {
        private ToolSystem m_ToolSystem;
        private EntityQuery m_ToolErrorPrefabQuery;
        private AnarchyUISystem m_AnarchyUISystem;
        private EnableToolErrorsSystem m_EnableToolErrorsSystem;
        private ILog m_Log;
        private PrefabSystem m_PrefabSystem;
        private ModificationBarrier5 m_Barrier;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableToolErrorsSystem"/> class.
        /// </summary>
        public DisableToolErrorsSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_EnableToolErrorsSystem = World.GetOrCreateSystemManaged<EnableToolErrorsSystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_AnarchyUISystem = World.GetOrCreateSystemManaged<AnarchyUISystem>();
            m_Barrier = World.GetOrCreateSystemManaged<ModificationBarrier5>();
            m_Log.Info($"{nameof(DisableToolErrorsSystem)} Created.");
            m_ToolErrorPrefabQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<ToolErrorData>(),
                        ComponentType.ReadOnly<NotificationIconData>(),
                    },
                },
            });
            RequireForUpdate(m_ToolErrorPrefabQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.activeTool.toolID == null)
            {
                return;
            }

            EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer();

            if (AnarchyMod.Instance.Settings.AllowPlacingMultipleUniqueBuildings)
            {
                PrefabID prefabID = new ("NotificationIconPrefab", "Already Exists");
                if (m_PrefabSystem.TryGetPrefab(prefabID, out PrefabBase prefabBase))
                {
                    if (m_PrefabSystem.TryGetEntity(prefabBase, out Entity entity))
                    {
                        if (EntityManager.TryGetComponent(entity, out ToolErrorData toolErrorData) &&
                          ((toolErrorData.m_Flags & ToolErrorFlags.DisableInEditor) != ToolErrorFlags.DisableInEditor ||
                           (toolErrorData.m_Flags & ToolErrorFlags.DisableInGame) != ToolErrorFlags.DisableInGame))
                        {
                            toolErrorData.m_Flags |= ToolErrorFlags.DisableInGame;
                            toolErrorData.m_Flags |= ToolErrorFlags.DisableInEditor;
                            buffer.SetComponent(entity, toolErrorData);
                        }
                    }
                }
            }

            List<ErrorType> errorTypesToDisable = m_AnarchyUISystem.GetAllowableErrorTypes();
            NativeArray <Entity> toolErrorPrefabs = m_ToolErrorPrefabQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity currentEntity in toolErrorPrefabs)
            {
                if (EntityManager.TryGetComponent(currentEntity, out ToolErrorData toolErrorData))
                {
#if VERBOSE
                    m_Log.Verbose("DisableToolErrorsSystem.OnUpdate currentEntity.index = " + currentEntity.Index + " currentEntity.version = " + currentEntity.Version + " ErrorType = " + toolErrorData.m_Error.ToString());
                    m_Log.Verbose("DisableToolErrorsSystem.OnUpdate toolErrorData.m_Flags = " + toolErrorData.m_Flags.ToString());
#endif
                    if (errorTypesToDisable.Contains(toolErrorData.m_Error) &&
                       ((toolErrorData.m_Flags & ToolErrorFlags.DisableInEditor) != ToolErrorFlags.DisableInEditor ||
                       (toolErrorData.m_Flags & ToolErrorFlags.DisableInGame) != ToolErrorFlags.DisableInGame))
                    {
                        toolErrorData.m_Flags |= ToolErrorFlags.DisableInGame;
                        toolErrorData.m_Flags |= ToolErrorFlags.DisableInEditor;
                        buffer.SetComponent(currentEntity, toolErrorData);
                    }
                }
            }

            toolErrorPrefabs.Dispose();
            m_EnableToolErrorsSystem.Enabled = true;
        }
    }
}
