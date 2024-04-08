// <copyright file="CheckTransformSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using System.Collections.Generic;
    using System.Reflection;
    using Anarchy;
    using Anarchy.Components;
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// A system that prevents objects from being overriden that has a custom component.
    /// </summary>
    public partial class CheckTransformSystem : GameSystemBase
    {
        private const string MoveItToolID = "MoveItTool";
        private ILog m_Log;
        private EntityQuery m_TransformRecordQuery;
        private ToolSystem m_ToolSystem;
        private ToolBaseSystem m_MoveItTool;


        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTransformSystem"/> class.
        /// </summary>
        public CheckTransformSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(CheckTransformSystem)} Created.");
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_TransformRecordQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
               {
                    ComponentType.ReadOnly<Updated>(),
                    ComponentType.ReadOnly<TransformRecord>(),
                    ComponentType.ReadOnly<Game.Objects.Transform>(),
               },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<Deleted>(),
                },
            });
            RequireForUpdate(m_TransformRecordQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (World.GetOrCreateSystemManaged<ToolSystem>().tools.Find(x => x.toolID.Equals(MoveItToolID)) is ToolBaseSystem moveItTool)
            {
                // Found it
                m_Log.Info($"{nameof(ResetTransformSystem)}.{nameof(OnGameLoadingComplete)} found Move It.");
                PropertyInfo moveItSelectedEntities = moveItTool.GetType().GetProperty("SelectedEntities");
                if (moveItSelectedEntities is not null)
                {
                    m_MoveItTool = moveItTool;
                    m_Log.Info($"{nameof(ResetTransformSystem)}.{nameof(OnGameLoadingComplete)} saved moveItTool");
                }
            }
            else
            {
                m_Log.Info($"{nameof(ResetTransformSystem)}.{nameof(OnGameLoadingComplete)} move it tool not found");
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.actionMode.IsEditor() && !AnarchyMod.Instance.Settings.PreventOverrideInEditor)
            {
                return;
            }

            HashSet<Entity> moveItToolSelectedEntities = new HashSet<Entity>();
            if (m_ToolSystem.activeTool.toolID == MoveItToolID && m_MoveItTool is not null)
            {
                PropertyInfo moveItSelectedEntities = m_MoveItTool.GetType().GetProperty("SelectedEntities");
                if (moveItSelectedEntities is not null)
                {
                    moveItToolSelectedEntities = (HashSet<Entity>)moveItSelectedEntities.GetValue(m_MoveItTool);
                    m_Log.Debug($"{nameof(CheckTransformSystem)}.{nameof(OnUpdate)} saved moveItTool selected entities");
                }
            }

            NativeArray<Entity> entities = m_TransformRecordQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out TransformRecord transformRecord) || !EntityManager.TryGetComponent(entity, out Game.Objects.Transform originalTransform))
                {
                    continue;
                }

                if (transformRecord.Equals(originalTransform))
                {
                    continue;
                }

                if (m_ToolSystem.selected == entity || moveItToolSelectedEntities.Contains(entity))
                {
                    transformRecord.m_Position = originalTransform.m_Position;
                    transformRecord.m_Rotation = originalTransform.m_Rotation;
                    EntityManager.SetComponentData(entity, transformRecord);
                }


            }

        }
    }
}
