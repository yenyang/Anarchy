// <copyright file="EnableToolErrorsSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.ErrorChecks
{
    using System.Collections.Generic;
    using Anarchy;
    using Anarchy.Systems.Common;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    ///  A system the queries for toolErrorPrefabs and then re-enables relevent tool errors in game to restore them after they no longer need to be disabled.
    /// </summary>
    public partial class EnableToolErrorsSystem : GameSystemBase
    {
        private ModificationEndBarrier m_Barrier; // System runs on SystemUpdatePhasel.ModificationEnd therefore use ModificationEndBarrier. Using a barrier in the wrong phase will produce an error.
        private EntityQuery m_ToolErrorPrefabQuery;
        private AnarchyUISystem m_AnarchyUISystem;
        private ILog m_Log;

        // These errors typically have DisableInEditor flat by default.
        private List<Game.Tools.ErrorType> m_DoNotReEnableForEditor = new ()
        {
            Game.Tools.ErrorType.AlreadyExists,
            Game.Tools.ErrorType.AlreadyUpgraded,
            Game.Tools.ErrorType.ExceedsCityLimits,
            Game.Tools.ErrorType.NoWater,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableToolErrorsSystem"/> class.
        /// </summary>
        public EnableToolErrorsSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(EnableToolErrorsSystem)} Created.");

            // System References.
            m_AnarchyUISystem = World.GetOrCreateSystemManaged<AnarchyUISystem>();
            m_Barrier = World.GetOrCreateSystemManaged<ModificationEndBarrier>(); // Get an System reference to the barrier with the right timing.

            // Disable System by default. This system will enabled whenever any error checks are disabled by DisableToolErrorsSystem.
            Enabled = false;

            // Consider using SystemAPI but this is a valid way to setup a Query too. This setup worked before we could use SystemAPI.
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
            EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer(); // Create the command buffer that we will schedule structural changes too.
            NativeArray<Entity> toolErrorPrefabs = m_ToolErrorPrefabQuery.ToEntityArray(Allocator.Temp); // Important to use Allocator.Temp. You do not need to dispose of a Temp allocator. Forgetting to dispose a TempJob allocator will produce a memory leak.
            foreach (Entity currentEntity in toolErrorPrefabs)
            {
                if (EntityManager.TryGetComponent<ToolErrorData>(currentEntity, out ToolErrorData toolErrorData))
                {
                    bool flagChanged = false;
                    if (toolErrorData.m_Error != Game.Tools.ErrorType.ExceedsLotLimits &&
                       (toolErrorData.m_Flags & ToolErrorFlags.DisableInGame) == ToolErrorFlags.DisableInGame)
                    {
                        toolErrorData.m_Flags &= ~ToolErrorFlags.DisableInGame;
                        flagChanged = true;
                    }

                    if (!m_DoNotReEnableForEditor.Contains(toolErrorData.m_Error) &&
                       (toolErrorData.m_Flags & ToolErrorFlags.DisableInEditor) == ToolErrorFlags.DisableInEditor)
                    {
                        toolErrorData.m_Flags &= ~ToolErrorFlags.DisableInEditor;
                        flagChanged = true;
                    }

                    if (flagChanged)
                    {
                        buffer.SetComponent(currentEntity, toolErrorData); // Queue ups all structural changes to be played back automatically with ModificationEndBarrier. When using a barrier you should not manually playback the ECB, nor  should you dispose of the ECB. All handled by the barrier.
                    }
#if VERBOSE
                    AnarchyIMod.Logger.Verbose(("DisableToolErrorsSystem.OnUpdate currentEntity.index = " + currentEntity.Index + " currentEntity.version = " + currentEntity.Version + " ErrorType = " + toolErrorData.m_Error.ToString());
                    AnarchyIMod.Logger.Verbose("DisableToolErrorsSystem.OnUpdate toolErrorData.m_Flags = " + toolErrorData.m_Flags.ToString());
#endif
                }
            }

            toolErrorPrefabs.Dispose();   // Not necessary, but doesn't actually hurt to include it.
            Enabled = false;          // Disable the system afterwards.
        }
    }
}
