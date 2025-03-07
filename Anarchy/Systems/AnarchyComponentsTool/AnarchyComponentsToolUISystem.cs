// <copyright file="AnarchyComponentsToolUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.AnarchyComponentsTool
{
    using Anarchy;
    using Anarchy.Extensions;
    using Colossal.Logging;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.Tools;
    using Game.UI.InGame;
    using System;
    using Unity.Entities;

    /// <summary>
    /// A UI System for the Anarchy Components Tool.
    /// </summary>
    public partial class AnarchyComponentsToolUISystem : ExtendedUISystemBase
    {
        private ILog m_Log;
        private ToolSystem m_ToolSystem;
        private AnarchyComponentsToolSystem m_AnarchyComponentsTool;
        private ValueBindingHelper<AnarchyComponentType> m_AnarchyComponentType;
        private ValueBindingHelper<SelectionMode> m_SelectionMode;
        private PrefabSystem m_PrefabSystem;
        private ValueBindingHelper<int> m_SelectionRadius;
        private RenderingSystem m_RenderingSystem;

        // private ToolBaseSystem m_PreviousToolSystem;
        // private PrefabBase m_PreviousPrefab;
        // private bool m_SwitchToPreviousToolSystem;
        private DefaultToolSystem m_DefaultToolSystem;
        private ToolbarUISystem m_ToolbarUISystem;

        /// <summary>
        /// Enum for different component types the tool can add or remove.
        /// </summary>
        public enum AnarchyComponentType
        {
            /// <summary>
            /// Prevents overridable static objects from being overriden.
            /// </summary>
            PreventOverride = 1,

            /// <summary>
            /// Prevents game systems from moving overrisable static objects.
            /// </summary>
            TransformRecord = 2,
        }

        /// <summary>
        /// An enum for tools selection mode.
        /// </summary>
        public enum SelectionMode
        {
            /// <summary>
            /// Single selection.
            /// </summary>
            Single,

            /// <summary>
            /// Radius Selection.
            /// </summary>
            Radius,
        }

        /// <summary>
        /// Gets the value of current anarchy component type for the tool.
        /// </summary>
        public AnarchyComponentType CurrentComponentType
        {
            get { return m_AnarchyComponentType.Value; }
        }

        /// <summary>
        /// Gets the value of the current selection mode for the tool.
        /// </summary>
        public SelectionMode CurrentSelectionMode
        {
            get { return m_SelectionMode.Value; }
        }

        /// <summary>
        /// Gets the value of the selection radius.
        /// </summary>
        public int SelectionRadius
        {
            get { return m_SelectionRadius.Value; }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(AnarchyComponentsToolUISystem)}.{nameof(OnCreate)}");
            m_AnarchyComponentsTool = World.GetOrCreateSystemManaged<AnarchyComponentsToolSystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_DefaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ToolbarUISystem = World.GetOrCreateSystemManaged<ToolbarUISystem>();
            m_ToolSystem.EventToolChanged += (ToolBaseSystem tool) => Enabled = tool == m_DefaultToolSystem;
            m_RenderingSystem = World.GetOrCreateSystemManaged<RenderingSystem>();
            m_AnarchyComponentType = CreateBinding("AnarchyComponentType", AnarchyComponentType.PreventOverride);
            m_SelectionMode = CreateBinding("SelectionMode", SelectionMode.Radius);
            m_SelectionRadius = CreateBinding("SelectionRadius", 10);

            // Creates triggers for C# methods based on UI events.
            CreateTrigger("ActivateAnarchyComponentsTool", () =>
            {
                m_AnarchyComponentsTool.MustStartRunning = true;
                m_ToolSystem.activeTool = m_AnarchyComponentsTool;
            });

            CreateTrigger("SelectionMode", (int mode) =>
            {
                m_SelectionMode.Value = (SelectionMode)mode;
                if ((m_AnarchyComponentType & AnarchyComponentType.PreventOverride) == AnarchyComponentType.PreventOverride
                    && m_SelectionMode == SelectionMode.Radius)
                {
                    m_RenderingSystem.markersVisible = true;
                }
                else
                {
                    m_RenderingSystem.markersVisible = m_AnarchyComponentsTool.PreviousShowMarkers;
                }
            });

            CreateTrigger("AnarchyComponentType", (int type) =>
            {
                AnarchyComponentType anarchyComponentType = (AnarchyComponentType)type;
                if ((m_AnarchyComponentType & anarchyComponentType) == anarchyComponentType)
                {
                    m_AnarchyComponentType.Value &= ~anarchyComponentType;
                    if (m_AnarchyComponentType.Value == 0 && anarchyComponentType == AnarchyComponentType.TransformRecord)
                    {
                        m_AnarchyComponentType.Value |= AnarchyComponentType.PreventOverride;
                    }
                    else if (m_AnarchyComponentType.Value == 0 && anarchyComponentType == AnarchyComponentType.PreventOverride)
                    {
                        m_AnarchyComponentType.Value |= AnarchyComponentType.TransformRecord;
                    }
                }
                else
                {
                    m_AnarchyComponentType.Value |= anarchyComponentType;
                }

                if ((m_AnarchyComponentType & AnarchyComponentType.PreventOverride) == AnarchyComponentType.PreventOverride
                    && m_SelectionMode == SelectionMode.Radius)
                {
                    m_RenderingSystem.markersVisible = true;
                }
                else
                {
                    m_RenderingSystem.markersVisible = m_AnarchyComponentsTool.PreviousShowMarkers;
                }
            });

            CreateTrigger("IncreaseRadius", () => m_SelectionRadius.Value = Math.Min(m_SelectionRadius.Value + 10, 100));
            CreateTrigger("DecreaseRadius", () => m_SelectionRadius.Value = Math.Max(m_SelectionRadius.Value - 10, 10));
            Enabled = false;
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_AnarchyComponentsTool.MustStartRunning &&
                m_ToolSystem.activeTool != m_AnarchyComponentsTool)
            {
                m_ToolSystem.activeTool = m_AnarchyComponentsTool;
            }

            Enabled = false;
        }
    }
}
