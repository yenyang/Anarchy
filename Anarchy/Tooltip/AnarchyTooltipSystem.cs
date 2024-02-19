// <copyright file="AnarchyTooltipSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Tooltip
{
    using Anarchy;
    using Colossal.Logging;
    using Game.Tools;
    using Game.UI.Tooltip;

    /// <summary>
    /// Applies a circle A tooltip when Anarchy is active.
    /// </summary>
    public partial class AnarchyTooltipSystem : TooltipSystemBase
    {
        private StringTooltip m_Tooltip;
        private ToolSystem m_ToolSystem;
        private AnarchySystem m_AnarchySystem;
        private ILog m_Log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnarchyTooltipSystem"/> class.
        /// </summary>
        public AnarchyTooltipSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Logger;
            m_AnarchySystem = World.GetOrCreateSystemManaged<AnarchySystem>();
            m_Tooltip = new StringTooltip()
            {
                icon = "coui://uil/Colored/Anarchy.svg",
            };
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_Log.Info($"{nameof(AnarchyTooltipSystem)} Created.");
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.activeTool.toolID != null && AnarchyMod.Settings.ShowTooltip)
            {
                if (m_AnarchySystem.IsToolAppropriate(m_ToolSystem.activeTool.toolID) && m_AnarchySystem.AnarchyEnabled)
                {
                    AddMouseTooltip(m_Tooltip);
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
