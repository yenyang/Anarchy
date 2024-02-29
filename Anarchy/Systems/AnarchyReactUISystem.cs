// <copyright file="AnarchyReactUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using System;
    using Anarchy.Tooltip;
    using Colossal.Logging;
    using Colossal.UI.Binding;
    using Game.Tools;
    using Game.UI;
    using Unity.Entities;

    /// <summary>
    /// UI system for Object Tool while using tree prefabs.
    /// </summary>
    public partial class AnarchyReactUISystem : UISystemBase
    {
        private ToolSystem m_ToolSystem;
        private ILog m_Log;
        private AnarchySystem m_AnarchySystem;
        private bool m_DisableAnarchyWhenCompleted;
        private string m_LastTool;
        private BulldozeToolSystem m_BulldozeToolSystem;
        private NetToolSystem m_NetToolSystem;
        private ResetNetCompositionDataSystem m_ResetNetCompositionDataSystem;
        private ValueBinding<bool> m_AnarchyEnabled;
        private ValueBinding<bool> m_ShowToolIcon;
        private ValueBinding<bool> m_FlamingChirperOption;

        /// <summary>
        /// Gets a value indicating whether the flaming chirper option binding is on/off.
        /// </summary>
        public bool FlamingChirperOption { get => m_FlamingChirperOption.value; }

        /// <summary>
        /// Gets a value indicating whether the flaming chirper option binding is on/off.
        /// </summary>
        public bool AnarchyEnabled { get => m_AnarchyEnabled.value; }

        /// <summary>
        /// Sets the flaming chirper option binding to value.
        /// </summary>
        /// <param name="value">True for option enabled. false if not.</param>
        public void SetFlamingChirperOption(bool value)
        {
            // This updates the flaming chirper option binding. It is triggered in the settings by overriding Apply.
            m_FlamingChirperOption.Update(value);
        }

        /// <summary>
        /// So Anarchy System can toggle the button selection with Keybind.
        /// </summary>
        public void ToggleAnarchyButton()
        {
            // This updates the Anarchy Enabled binding to its inverse.
            m_AnarchyEnabled.Update(!m_AnarchyEnabled.value);
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_AnarchySystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<AnarchySystem>();
            m_BulldozeToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<BulldozeToolSystem>();
            m_NetToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<NetToolSystem>();
            m_ResetNetCompositionDataSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ResetNetCompositionDataSystem>();
            ToolSystem toolSystem = m_ToolSystem; // I don't know why vanilla game did this.
            m_ToolSystem.EventToolChanged = (Action<ToolBaseSystem>)Delegate.Combine(toolSystem.EventToolChanged, new Action<ToolBaseSystem>(OnToolChanged));
            m_Log.Info($"{nameof(AnarchyReactUISystem)}.{nameof(OnCreate)}");

            // This binding communicates whether Anarchy toggle is enabled or disabled.
            AddBinding(m_AnarchyEnabled = new ValueBinding<bool>("Anarchy", "AnarchyEnabled", false));

            // This binding communicates whether to show the Anarchy tool icon.
            AddBinding(m_ShowToolIcon = new ValueBinding<bool>("Anarchy", "ShowToolIcon", false));

            // This binding listens for whether the Anarchy tool icon has been toggled.
            AddBinding(new TriggerBinding("Anarchy", "AnarchyToggled", AnarchyToggled));

            // This binding communicated whether the option for using Flaming chirper is enabled.
            AddBinding(m_FlamingChirperOption = new ValueBinding<bool>("Anarchy", "FlamingChirperOption", AnarchyMod.Instance.Settings.FlamingChirper));
        }

        /// <summary>
        /// An event to Toggle Anarchy.
        /// </summary>
        /// <param name="flag">A bool for whether it's enabled or not.</param>
        private void AnarchyToggled()
        {
            // This updates the Anarchy Enabled binding to its inverse.
            m_AnarchyEnabled.Update(!m_AnarchyEnabled.value);
            if (!m_AnarchyEnabled.value)
            {
                m_DisableAnarchyWhenCompleted = false;
                m_ResetNetCompositionDataSystem.Enabled = true;
            }
        }

        private void OnToolChanged(ToolBaseSystem tool)
        {
            if (tool == null || tool.toolID == null)
            {
                // This updates the Show Tool Icon binding to not show the tool icon.
                m_ShowToolIcon.Update(false);
                return;
            }

            if (m_AnarchySystem.IsToolAppropriate(tool.toolID) && AnarchyMod.Instance.Settings.ToolIcon)
            {
                // This updates the Show Tool Icon binding to show the tool icon.
                m_ShowToolIcon.Update(true);
            }
            else
            {
                // This updates the Show Tool Icon binding to not show the tool icon.
                m_ShowToolIcon.Update(false);
            }

            if (tool != m_BulldozeToolSystem && m_DisableAnarchyWhenCompleted)
            {
                m_DisableAnarchyWhenCompleted = false;

                // This updates the Anarchy Enabled binding to its inverse.
                m_AnarchyEnabled.Update(!m_AnarchyEnabled.value);
            }

            // Implements Anarchic Bulldozer when bulldoze tool is activated from inappropriate tool.
            if (AnarchyMod.Instance.Settings.AnarchicBulldozer && m_AnarchyEnabled.value == false && tool == m_BulldozeToolSystem)
            {
                m_DisableAnarchyWhenCompleted = true;

                // This updates the Anarchy Enabled binding to its inverse.
                m_AnarchyEnabled.Update(!m_AnarchyEnabled.value);
            }

            if (tool != m_NetToolSystem && m_LastTool == m_NetToolSystem.toolID)
            {
                m_ResetNetCompositionDataSystem.Enabled = true;
            }

            m_LastTool = tool.toolID;
        }

    }
}
