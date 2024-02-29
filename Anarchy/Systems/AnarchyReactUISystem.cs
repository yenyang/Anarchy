// <copyright file="AnarchyReactUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Anarchy.Systems
{
    using System;
    using System.Collections.Generic;
    using Anarchy.Tooltip;
    using cohtml.Net;
    using Colossal.Logging;
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Rendering;
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
        private RenderingSystem m_RenderingSystem;
        private PrefabSystem m_PrefabSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private DefaultToolSystem m_DefaultToolSystem;
        private bool m_DisableAnarchyWhenCompleted;
        private string m_LastTool;
        private List<BoundEventHandle> m_BoundEventHandles;
        private BulldozeToolSystem m_BulldozeToolSystem;
        private NetToolSystem m_NetToolSystem;
        private ResetNetCompositionDataSystem m_ResetNetCompositionDataSystem;
        private ValueBinding<bool> m_AnarchyEnabled;
        private ValueBinding<bool> m_ShowToolIcon;
        private ValueBinding<bool> m_FlamingChirperOption;

        /// <summary>
        /// Gets a value indicating whether whether Anarchy is only on because of Anarchic Bulldozer setting.
        /// </summary>
        public bool DisableAnarchyWhenCompleted
        {
            get { return m_DisableAnarchyWhenCompleted; }
        }

        /// <summary>
        /// Gets a value indicating whether the flaming chirper option binding is on/off.
        /// </summary>
        public bool FlamingChirperOption { get => m_FlamingChirperOption.value; }

        /// <summary>
        /// Sets the flaming chirper option binding to value.
        /// </summary>
        /// <param name="value">True for option enabled. false if not.</param>
        public void SetFlamingChirperOption(bool value)
        {
            m_FlamingChirperOption.Update(value);
        }

        /// <summary>
        /// So Anarchy System can toggle the button selection with Keybind.
        /// </summary>
        public void ToggleAnarchyButton()
        {
            m_AnarchyEnabled.Update(m_AnarchySystem.AnarchyEnabled);
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_Log.effectivenessLevel = Level.Info;
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_AnarchySystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<AnarchySystem>();
            m_BulldozeToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<BulldozeToolSystem>();
            m_RenderingSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<RenderingSystem>();
            m_ResetNetCompositionDataSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ResetNetCompositionDataSystem>();
            m_ObjectToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_DefaultToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<DefaultToolSystem>();
            m_PrefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            ToolSystem toolSystem = m_ToolSystem; // I don't know why vanilla game did this.
            m_ToolSystem.EventToolChanged = (Action<ToolBaseSystem>)Delegate.Combine(toolSystem.EventToolChanged, new Action<ToolBaseSystem>(OnToolChanged));
            m_BoundEventHandles = new ();
            m_NetToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<NetToolSystem>();
            m_Log.Info($"{nameof(AnarchyReactUISystem)}.{nameof(OnCreate)}");
            AddBinding(m_AnarchyEnabled = new ValueBinding<bool>("Anarchy", "AnarchyEnabled", m_AnarchySystem.AnarchyEnabled));
            AddBinding(m_ShowToolIcon = new ValueBinding<bool>("Anarchy", "ShowToolIcon", false));
            AddBinding(new TriggerBinding("Anarchy", "AnarchyToggled", AnarchyToggled));
            AddBinding(m_FlamingChirperOption = new ValueBinding<bool>("Anarchy", "FlamingChirperOption", AnarchyMod.Instance.Settings.FlamingChirper));
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        /// <summary>
        /// An event to Toggle Anarchy.
        /// </summary>
        /// <param name="flag">A bool for whether it's enabled or not.</param>
        private void AnarchyToggled()
        {
            m_AnarchySystem.AnarchyEnabled = !m_AnarchySystem.AnarchyEnabled;
            m_AnarchyEnabled.Update(m_AnarchySystem.AnarchyEnabled);
            if (!m_AnarchySystem.AnarchyEnabled)
            {
                m_DisableAnarchyWhenCompleted = false;
                m_ResetNetCompositionDataSystem.Enabled = true;
            }
        }

        private void OnToolChanged(ToolBaseSystem tool)
        {
            if (tool == null || tool.toolID == null)
            {
                m_ShowToolIcon.Update(false);
                return;
            }

            if (m_AnarchySystem.IsToolAppropriate(tool.toolID))
            {
                Enabled = true;
                m_ShowToolIcon.Update(true);
            }
            else
            {
                m_ShowToolIcon.Update(false);
            }

            if (tool != m_BulldozeToolSystem && m_DisableAnarchyWhenCompleted)
            {
                m_AnarchySystem.AnarchyEnabled = false;
                m_DisableAnarchyWhenCompleted = false;
                ToggleAnarchyButton();
            }

            // Implements Anarchic Bulldozer when bulldoze tool is activated from inappropriate tool.
            if (AnarchyMod.Instance.Settings.AnarchicBulldozer && m_AnarchySystem.AnarchyEnabled == false && tool == m_BulldozeToolSystem)
            {
                m_AnarchySystem.AnarchyEnabled = true;
                m_DisableAnarchyWhenCompleted = true;
                ToggleAnarchyButton();
            }

            if (tool != m_NetToolSystem && m_LastTool == m_NetToolSystem.toolID)
            {
                m_ResetNetCompositionDataSystem.Enabled = true;
            }

            m_LastTool = tool.toolID;
        }

    }
}
