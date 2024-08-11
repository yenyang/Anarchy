// <copyright file="AnarchyComponentsToolUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.AnarchyComponentsTool
{
    using Anarchy;
    using Anarchy.Extensions;
    using Colossal.Logging;
    using Game.Tools;
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

        /// <summary>
        /// Enum for different component types the tool can add or remove.
        /// </summary>
        public enum AnarchyComponentType
        {
            /// <summary>
            /// Prevents overridable static objects from being overriden.
            /// </summary>
            PreventOverride,

            /// <summary>
            /// Prevents game systems from moving overrisable static objects.
            /// </summary>
            TransformRecord,
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

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(AnarchyComponentsToolUISystem)}.{nameof(OnCreate)}");
            m_AnarchyComponentsTool = World.GetOrCreateSystemManaged<AnarchyComponentsToolSystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ToolSystem.EventToolChanged += (ToolBaseSystem tool) => Enabled = tool == m_AnarchyComponentsTool;

            m_AnarchyComponentType = CreateBinding("AnarchyComponentType", AnarchyComponentType.TransformRecord);
            m_SelectionMode = CreateBinding("SelectionMode", SelectionMode.Radius);

            // Creates triggers for C# methods based on UI events.
            CreateTrigger("ActivateAnarchyComponentsTool", () => m_ToolSystem.activeTool = m_AnarchyComponentsTool);
        }
    }
}
