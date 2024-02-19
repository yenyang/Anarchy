// <copyright file="AnarchyUISystem.cs" company="Yenyang's Mods. MIT License">
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
    using Game.Prefabs;
    using Game.Rendering;
    using Game.SceneFlow;
    using Game.Tools;
    using Game.UI;
    using Unity.Entities;

    /// <summary>
    /// UI system for Object Tool while using tree prefabs.
    /// </summary>
    public partial class AnarchyUISystem : UISystemBase
    {
        private View m_UiView;
        private ToolSystem m_ToolSystem;
        private string m_InjectedJS = string.Empty;
        private string m_AnarchyItemScript = string.Empty;
        private string m_AnarchyItemLineToolScript = string.Empty;
        private ILog m_Log;
        private AnarchySystem m_AnarchySystem;
        private RenderingSystem m_RenderingSystem;
        private PrefabSystem m_PrefabSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private DefaultToolSystem m_DefaultToolSystem;
        private bool m_AnarchyOptionShown;
        private bool m_DisableAnarchyWhenCompleted;
        private string m_LastTool;
        private List<BoundEventHandle> m_BoundEventHandles;
        private BulldozeToolSystem m_BulldozeToolSystem;
        private bool m_PrefabIsMarker = false;
        private bool m_FirstTimeLoadingJS = true;
        private NetToolSystem m_NetToolSystem;
        private bool m_LastShowMarkers = false;
        private ResetNetCompositionDataSystem m_ResetNetCompositionDataSystem;
        private bool m_RaycastingMarkers = false;

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
            if (m_UiView == null)
            {
                return;
            }

            if (m_AnarchySystem.AnarchyEnabled)
            {
                // This script finds sets Anarchy button selected if toggled using key board shortcut.
                m_UiView.ExecuteScript($"yyAnarchy.buttonElement = document.getElementById(\"YYA-Anarchy-Button\"); if (yyAnarchy.buttonElement != null) yyAnarchy.buttonElement.classList.add(\"selected\");");

                // This script finds sets Anarchy button colored if toggled using key board shortcut.
                m_UiView.ExecuteScript($"yyAnarchy.imageElement = document.getElementById(\"YYA-Anarchy-Image\"); if (yyAnarchy.imageElement != null) {{ yyAnarchy.imageElement.src = \"coui://uil/Colored/Anarchy.svg\"; }} ");
                if (AnarchyMod.Settings.FlamingChirper)
                {
                    // This script sets flaming chirper if toggled using key board shortcut.
                    m_UiView.ExecuteScript($"yyAnarchy.tagElements = document.getElementsByTagName(\"img\"); for (yyAnarchy.i = 0; yyAnarchy.i < yyAnarchy.tagElements.length; yyAnarchy.i++) {{ if (yyAnarchy.tagElements[yyAnarchy.i].src == \"coui://GameUI/Media/Game/Icons/Chirper.svg\" || yyAnarchy.tagElements[yyAnarchy.i].src == \"Media/Game/Icons/Chirper.svg\") yyAnarchy.tagElements[yyAnarchy.i].src = \"coui://uil/Colored/AnarchyChirper.svg\"; }}");
                }

                return;
            }

            // This script resets Anarchy button if toggled using key board shortcut.
            m_UiView.ExecuteScript($"yyAnarchy.buttonElement = document.getElementById(\"YYA-Anarchy-Button\"); if (yyAnarchy.buttonElement != null) yyAnarchy.buttonElement.classList.remove(\"selected\");");

            // This script  resets Anarchy button image if toggled using key board shortcut.
            m_UiView.ExecuteScript($"yyAnarchy.imageElement = document.getElementById(\"YYA-Anarchy-Image\"); if (yyAnarchy.imageElement != null) {{ yyAnarchy.imageElement.src = \"coui://uil/Standard/Anarchy.svg\"; }} ");

            // This script resets chirper if toggled using key board shortcut.
            m_UiView.ExecuteScript($"yyAnarchy.tagElements = document.getElementsByTagName(\"img\"); for (yyAnarchy.i = 0; yyAnarchy.i < yyAnarchy.tagElements.length; yyAnarchy.i++) {{ if (yyAnarchy.tagElements[yyAnarchy.i].src == \"coui://uil/Colored/AnarchyChirper.svg\") yyAnarchy.tagElements[yyAnarchy.i].src = \"coui://GameUI/Media/Game/Icons/Chirper.svg\"; }}");

            m_DisableAnarchyWhenCompleted = false;
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Logger;
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
            m_InjectedJS = UIFileUtils.ReadJS(Path.Combine(UIFileUtils.AssemblyPath, "ui.js"));
            m_AnarchyItemScript = UIFileUtils.ReadHTML(Path.Combine(UIFileUtils.AssemblyPath, "YYA-anarchy-tool-row.html"), "if (document.getElementById(\"YYA-anarchy-item\") == null) { yyAnarchy.div.className = \"item_bZY\"; yyAnarchy.div.id = \"YYA-anarchy-item\"; yyAnarchy.entities = document.getElementsByClassName(\"tool-options-panel_Se6\"); if (yyAnarchy.entities[0] != null) { yyAnarchy.entities[0].insertAdjacentElement('afterbegin', yyAnarchy.div); } }");
            m_AnarchyItemLineToolScript = UIFileUtils.ReadHTML(Path.Combine(UIFileUtils.AssemblyPath, "YYA-anarchy-tool-row.html"), "if (document.getElementById(\"YYA-anarchy-item\") == null) { yyAnarchy.div.className = \"item_bZY\"; yyAnarchy.div.id = \"YYA-anarchy-item\"; yyAnarchy.LineToolTitle = document.getElementById(\"line-tool-title\"); if (yyAnarchy.LineToolTitle != null) { yyAnarchy.LineToolTitle.parentElement.insertAdjacentElement('afterend', yyAnarchy.div); } }");

            if (m_UiView == null)
            {
                m_Log.Info($"{nameof(AnarchyUISystem)}.{nameof(OnCreate)} m_UiView == null");
            }

            base.OnCreate();
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
                        m_Log.Debug($"{nameof(AnarchyUISystem)}.{nameof(OnUpdate)} m_LastShowMarkers = {m_LastShowMarkers}");
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
                if (m_AnarchyOptionShown == true)
                {
                    UnshowAnarchyOption();
                }

                Enabled = false;
                return;
            }

            // This script creates the Anarchy object if it doesn't exist.
            UIFileUtils.ExecuteScript(m_UiView, "if (yyAnarchy == null) var yyAnarchy = {};");

            if (!m_AnarchySystem.IsToolAppropriate(m_ToolSystem.activeTool.toolID))
            {
                if (m_AnarchyOptionShown)
                {
                    UnshowAnarchyOption();
                }

                Enabled = false;
                return;
            }

            if (m_AnarchyOptionShown == false)
            {
                SendVariablesToJS();

                if (m_InjectedJS == string.Empty)
                {
                    m_Log.Warn($"{nameof(AnarchyUISystem)}.{nameof(OnUpdate)} m_InjectedJS is empty!!! Did you forget to include the ui.js file in the mod install folder?");
                    m_InjectedJS = UIFileUtils.ReadJS(Path.Combine(UIFileUtils.AssemblyPath, "ui.js"));
                    m_AnarchyItemScript = UIFileUtils.ReadHTML(Path.Combine(UIFileUtils.AssemblyPath, "YYA-anarchy-tool-row.html"), "if (document.getElementById(\"YYA-anarchy-item\") == null) { yyAnarchy.div.className = \"item_bZY\"; yyAnarchy.div.id = \"YYA-anarchy-item\"; yyAnarchy.entities = document.getElementsByClassName(\"tool-options-panel_Se6\"); if (yyAnarchy.entities[0] != null) { yyAnarchy.entities[0].insertAdjacentElement('afterbegin', yyAnarchy.div); } }");
                    m_AnarchyItemLineToolScript = UIFileUtils.ReadHTML(Path.Combine(UIFileUtils.AssemblyPath, "YYA-anarchy-tool-row.html"), "if (document.getElementById(\"YYA-anarchy-item\") == null) { yyAnarchy.div.className = \"item_bZY\"; yyAnarchy.div.id = \"YYA-anarchy-item\"; yyAnarchy.LineToolTitle = document.getElementById(\"line-tool-title\"); if (yyAnarchy.LineToolTitle != null) { yyAnarchy.LineToolTitle.parentElement.insertAdjacentElement('afterend', yyAnarchy.div); } }");
                }

                // This script defines the JS functions if they are not defined.
                UIFileUtils.ExecuteScript(m_UiView, m_InjectedJS);

                m_BoundEventHandles.Add(m_UiView.RegisterForEvent("YYA-log", (Action<string>)LogFromJS));

                if (m_FirstTimeLoadingJS)
                {
                    m_FirstTimeLoadingJS = false;
                    return;
                }

                if (AnarchyMod.Settings.ToolIcon)
                {
                    if (m_ToolSystem.activeTool.toolID != "Line Tool")
                    {
                        // This script creates the anarchy item and sets up the buttons.
                        UIFileUtils.ExecuteScript(m_UiView, $"{m_AnarchyItemScript}  yyAnarchy.setupAnarchyItem({BoolToString(m_AnarchySystem.AnarchyEnabled)}); ");
                    }
                    else
                    {
                        // This script creates the anarchy item and sets up the buttons specifically for line tool.
                        UIFileUtils.ExecuteScript(m_UiView, $"{m_AnarchyItemLineToolScript} yyAnarchy.setupAnarchyItem({BoolToString(m_AnarchySystem.AnarchyEnabled)}); ");
                    }

                    UIFileUtils.ExecuteScript(m_UiView, $"if (typeof yyAnarchy.applyLocalization == 'function') {{ yyAnarchy.AnarchyItem = document.getElementById(\"YYA-anarchy-item\"); if (yyAnarchy.AnarchyItem)  yyAnarchy.applyLocalization(yyAnarchy.AnarchyItem); }}");

                    m_BoundEventHandles.Add(m_UiView.RegisterForEvent("YYA-AnarchyToggled", (Action<bool>)AnarchyToggled));
                    m_BoundEventHandles.Add(m_UiView.RegisterForEvent("CheckForElement-YYA-anarchy-item", (Action<bool>)ElementCheck));
                }
                else if (AnarchyMod.Settings.FlamingChirper)
                {
                    ToggleAnarchyButton();
                }

                if (m_ToolSystem.activeTool == m_BulldozeToolSystem)
                {
                    m_BoundEventHandles.Add(m_UiView.RegisterForEvent("YYA-raycastingMarkers", (Action<bool>)RecordRaycastMarkers));
                }

                m_AnarchyOptionShown = true;
            }
            else
            {
                // This script checks to see if there is a tool options panel. If there isn't one then it adds one.
                UIFileUtils.ExecuteScript(m_UiView, $"yyAnarchy.entities = document.getElementsByClassName(\"tool-options-panel_Se6\"); if (yyAnarchy.entities[0] == null) {{ yyAnarchy.div = document.createElement(\"div\"); yyAnarchy.div.className = \"tool-options-panel_Se6\"; yyAnarchy.toolColumns = document.getElementsByClassName(\"tool-side-column_l9i\"); if (yyAnarchy.toolColumns[0] != null) yyAnarchy.toolColumns[0].appendChild(yyAnarchy.div); }}");

                // This script checks if multiple tool options panels exist. If anarchy is the only one in that tool panel then it removes whole panel.
                UIFileUtils.ExecuteScript(m_UiView, $"yyAnarchy.entities = document.getElementsByClassName(\"tool-options-panel_Se6\"); if (yyAnarchy.entities.length > 1) {{  yyAnarchy.itemElement = document.getElementById(\"YYA-anarchy-item\"); if (yyAnarchy.itemElement != null) {{ if (yyAnarchy.itemElement.parentElement.children.length == 1) {{   yyAnarchy.itemElement.parentElement.parentElement.removeChild(yyAnarchy.itemElement.parentElement); }} }} }}");

                // This script checks if anarchy item exists. If it doesn't it triggers anarchy item being recreated.
                UIFileUtils.ExecuteScript(m_UiView, $"if (document.getElementById(\"YYA-anarchy-item\") == null) engine.trigger('CheckForElement-YYA-anarchy-item', false);");

                if (m_ToolSystem.activeTool == m_BulldozeToolSystem)
                {
                    // This script checks if raycast markers from Better Bulldozer is selected if the active tool is bulldoze tool.
                    UIFileUtils.ExecuteScript(m_UiView, $"yyAnarchy.raycastMarkers = document.getElementById(\"YYBB-Raycast-Markers-Button\"); if (yyAnarchy.raycastMarkers) {{ engine.trigger(\'YYA-raycastingMarkers\',yyAnarchy.raycastMarkers.classList.contains(\"selected\")); }} ");
                }

                if (m_ToolSystem.activeTool.toolID != "Line Tool")
                {
                    // This script checks if anarchy is first child and if not removes it.
                    UIFileUtils.ExecuteScript(m_UiView, $"yyAnarchy.itemElement = document.getElementById(\"YYA-anarchy-item\"); if (yyAnarchy.itemElement != null) {{  if (yyAnarchy.itemElement.parentElement.firstChild != yyAnarchy.itemElement) {{  yyAnarchy.itemElement.parentElement.removeChild(yyAnarchy.itemElement);   }}  }}");
                }
            }

            base.OnUpdate();
        }

        /// <summary>
        /// Get a script for Destroing element by id if that element exists.
        /// </summary>
        /// <param name="id">The id from HTML or JS.</param>
        /// <returns>a script for Destroing element by id if that element exists.</returns>
        private string DestroyElementByID(string id)
        {
            return $"yyAnarchy.itemElement = document.getElementById(\"{id}\"); if (yyAnarchy.itemElement) yyAnarchy.itemElement.parentElement.removeChild(yyAnarchy.itemElement);";
        }

        /// <summary>
        /// Logs a string from JS.
        /// </summary>
        /// <param name="log">A string from JS to log.</param>
        private void LogFromJS(string log) => m_Log.Debug($"{nameof(AnarchyUISystem)}.{nameof(LogFromJS)} {log}");

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
        private void AnarchyToggled(bool flag)
        {
            if (flag)
            {
                m_AnarchySystem.AnarchyEnabled = true;
            }
            else
            {
                m_AnarchySystem.AnarchyEnabled = false;
                m_DisableAnarchyWhenCompleted = false;
                m_ResetNetCompositionDataSystem.Enabled = true;
            }
        }

        /// <summary>
        /// C# event handler for event callback from UI JavaScript. If element YYA-anarchy-item is found then set value to true.
        /// </summary>
        /// <param name="flag">A bool for whether to element was found.</param>
        private void ElementCheck(bool flag) => m_AnarchyOptionShown = flag;

        /// <summary>
        /// C# event handler for event callback from UI JavaScript. If element YYA-anarchy-item is found then set value to true.
        /// </summary>
        /// <param name="flag">A bool for whether to element was found.</param>
        private void RecordRaycastMarkers(bool flag) => m_RaycastingMarkers = flag;

        /// <summary>
        /// C# event handler for event callback from UI JavaScript. If element YYTC-tree-age-item is found then set value to true.
        /// </summary>
        private void SendVariablesToJS()
        {
            // This script passes whether Anarchy is Enabled to JS.
            UIFileUtils.ExecuteScript(m_UiView, $"yyAnarchy.enabled = {BoolToString(m_AnarchySystem.AnarchyEnabled)};");

            // This script passes the option to have flaming Chirper to JS.
            UIFileUtils.ExecuteScript(m_UiView, $"yyAnarchy.flamingChirper = {BoolToString(AnarchyMod.Settings.FlamingChirper)};");
        }

        /// <summary>
        /// Handles cleaning up after the icons are no longer needed.
        /// </summary>
        private void UnshowAnarchyOption()
        {
            if (m_UiView == null)
            {
                return;
            }

            // This script checks if tool options panel(s) exist. If anarchy is the only one in that tool panel then it removes whole panel.
            UIFileUtils.ExecuteScript(m_UiView, $"yyAnarchy.entities = document.getElementsByClassName(\"tool-options-panel_Se6\"); if (yyAnarchy.entities.length >= 1) {{  yyAnarchy.itemElement = document.getElementById(\"YYA-anarchy-item\"); if (yyAnarchy.itemElement != null) {{ if (yyAnarchy.itemElement.parentElement.children.length == 1) {{   yyAnarchy.itemElement.parentElement.parentElement.removeChild(yyAnarchy.itemElement.parentElement); }} }} }}");

            // This script destroys the anarchy item if it exists.
            UIFileUtils.ExecuteScript(m_UiView, DestroyElementByID("YYA-anarchy-item"));

            // This script resets chirper.
            m_UiView.ExecuteScript($"yyAnarchy.tagElements = document.getElementsByTagName(\"img\"); for (yyAnarchy.i = 0; yyAnarchy.i < yyAnarchy.tagElements.length; yyAnarchy.i++) {{ if (yyAnarchy.tagElements[yyAnarchy.i].src == \"coui://uil/Colored/AnarchyChirper.svg\") yyAnarchy.tagElements[yyAnarchy.i].src = \"coui://GameUI/Media/Game/Icons/Chirper.svg\"; }}");

            // This unregisters the events.
            foreach (BoundEventHandle eventHandle in m_BoundEventHandles)
            {
                m_UiView.UnregisterFromEvent(eventHandle);
            }

            m_BoundEventHandles.Clear();

            // This records that everything is cleaned up.
            m_AnarchyOptionShown = false;
        }

        private void OnToolChanged(ToolBaseSystem tool)
        {
            // This script creates the Anarchy object if it doesn't exist.
            UIFileUtils.ExecuteScript(m_UiView, "if (yyAnarchy == null) var yyAnarchy = {};");

            if (tool == null || tool.toolID == null)
            {
                return;
            }

            if (m_AnarchySystem.IsToolAppropriate(tool.toolID))
            {
                Enabled = true;
            }

            // Makes it so Anarchic Bulldozer will work next frame when bulldoze tool is activated from other appropriate tool.
            if (tool.toolID == "Bulldoze Tool" && m_LastTool != "Bulldoze Tool")
            {
                m_AnarchyOptionShown = false;
            }

            // Removes Anarchy item if activating line tool so that it can be recreated in the new location. 
            if (tool.toolID == "Line Tool")
            {
                UIFileUtils.ExecuteScript(m_UiView, DestroyElementByID("YYA-anarchy-item"));
                m_AnarchyOptionShown = false;
            }

            if (tool != m_BulldozeToolSystem && m_DisableAnarchyWhenCompleted)
            {
                m_AnarchySystem.AnarchyEnabled = false;
                m_DisableAnarchyWhenCompleted = false;
                ToggleAnarchyButton();
            }

            // Implements Anarchic Bulldozer when bulldoze tool is activated from inappropriate tool.
            if (AnarchyMod.Settings.AnarchicBulldozer && m_AnarchySystem.AnarchyEnabled == false && tool == m_BulldozeToolSystem)
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
