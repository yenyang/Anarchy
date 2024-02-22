// <copyright file="AnarchyReactUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Anarchy.Systems
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Anarchy.Tooltip;
    using Anarchy.Utils;
    using cohtml.Net;
    using Colossal.Logging;
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.SceneFlow;
    using Game.Tools;
    using Game.UI;
    using Unity.Entities;

    /// <summary>
    /// UI system for Object Tool while using tree prefabs.
    /// </summary>
    public partial class AnarchyReactUISystem : UISystemBase
    {
        private View m_UiView;
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
        private bool m_PrefabIsMarker = false;
        private NetToolSystem m_NetToolSystem;
        private bool m_LastShowMarkers = false;
        private ResetNetCompositionDataSystem m_ResetNetCompositionDataSystem;
        private bool m_RaycastingMarkers = false;
        private ValueBinding<bool> m_AnarchyEnabled;
        private ValueBinding<bool> m_ShowToolIcon;

        /// <summary>
        /// Gets a value indicating whether whether Anarchy is only on because of Anarchic Bulldozer setting.
        /// </summary>
        public bool DisableAnarchyWhenCompleted
        {
            get { return m_DisableAnarchyWhenCompleted; }
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
            m_Log = Mod.Instance.Log;
            m_Log.effectivenessLevel = Level.Info;
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_UiView = GameManager.instance.userInterface.view.View;
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
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_UiView == null)
            {
                return;
            }

            if (m_ToolSystem.activePrefab != null && m_PrefabSystem.TryGetEntity(m_ToolSystem.activePrefab, out Entity prefabEntity) && m_ToolSystem.activeTool != m_DefaultToolSystem)
            {
                if (EntityManager.HasComponent<MarkerNetData>(prefabEntity) || m_ToolSystem.activePrefab is MarkerObjectPrefab)
                {
                    if (!m_PrefabIsMarker && (m_LastTool != m_BulldozeToolSystem.toolID || !m_RaycastingMarkers))
                    {
                        m_LastShowMarkers = m_RenderingSystem.markersVisible;
                        m_Log.Debug($"{nameof(AnarchyReactUISystem)}.{nameof(OnUpdate)} m_LastShowMarkers = {m_LastShowMarkers}");
                    }

                    m_RenderingSystem.markersVisible = true;
                    m_PrefabIsMarker = true;
                }
                else if (m_PrefabIsMarker)
                {
                    m_PrefabIsMarker = false;
                    m_RenderingSystem.markersVisible = m_LastShowMarkers;
                }
            }
            else if (m_PrefabIsMarker)
            {
                m_PrefabIsMarker = false;
                m_RenderingSystem.markersVisible = m_LastShowMarkers;
            }

            if (m_ToolSystem.activeTool.toolID == null)
            {
                Enabled = false;
                return;
            }

            if (m_AnarchySystem.IsToolAppropriate(m_ToolSystem.activeTool.toolID) && Mod.Instance.Settings.ToolIcon)
            {
                m_ShowToolIcon.Update(true);
            }
            else
            {
                m_ShowToolIcon.Update(false);
            }

            base.OnUpdate();
        }

        /// <summary>
        /// Converts a C# bool to JS string.
        /// </summary>
        /// <param name="flag">a bool.</param>
        /// <returns>"true" or "false".</returns>
        private string BoolToString(bool flag)
        {
            if (flag)
            {
                return "true";
            }

            return "false";
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
            m_ShowToolIcon.Update(false);

            if (tool == null || tool.toolID == null)
            {
                return;
            }

            if (m_AnarchySystem.IsToolAppropriate(tool.toolID))
            {
                Enabled = true;
            }

            if (tool != m_BulldozeToolSystem && m_DisableAnarchyWhenCompleted)
            {
                m_AnarchySystem.AnarchyEnabled = false;
                m_DisableAnarchyWhenCompleted = false;
                ToggleAnarchyButton();
            }

            // Implements Anarchic Bulldozer when bulldoze tool is activated from inappropriate tool.
            if (Mod.Instance.Settings.AnarchicBulldozer && m_AnarchySystem.AnarchyEnabled == false && tool == m_BulldozeToolSystem)
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
