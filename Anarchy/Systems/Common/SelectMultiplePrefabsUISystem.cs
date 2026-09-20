// <copyright file="SelectMultiplePrefabsUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.Common
{
    using Colossal.Annotations;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game.Prefabs;
    using Game.SceneFlow;
    using Game.Tools;
    using Game.UI;
    using System.Collections.Generic;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Hacky solution for selecing multiple prefabs. Limited to objects, on same tab, no trees or plants.
    /// </summary>
    public partial class SelectMultiplePrefabsUISystem : UISystemBase
    {
        private cohtml.Net.View m_UiView;
        private ToolSystem m_ToolSystem;
        private PrefabSystem m_PrefabSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private ILog m_Log;
        private List<Entity> m_ThemeEntities;
        private NativeList<Entity> m_SelectedPrefabEntities;
        private Entity m_RecentlyRemovedPrefabEntity;
        private bool m_UpdateSelectionSet;
        private Entity m_UIGroup = Entity.Null;
        private int m_FrameCount = 0;
        [CanBeNull]
        private PrefabBase m_TrySetPrefabNextFrame;
        [CanBeNull]
        private PrefabBase m_LastActivePrefab;

        /// <summary>
        /// Gets or sets a value indicating whether the selection set of buttons on the Toolbar UI needs to be updated.
        /// </summary>
        public bool UpdateSelectionSet
        {
            get => m_UpdateSelectionSet;
            set => m_UpdateSelectionSet = value;
        }

        /// <summary>
        /// Gets or sets a value indicating the list of theme entities selected.
        /// </summary>
        public List<Entity> ThemeEntities
        {
            get => m_ThemeEntities;
            set => m_ThemeEntities = value;
        }

        /// <summary>
        /// Gets a value indicating whether there are multiple prefab selected.
        /// </summary>
        public bool MultiplePrefabsSelected
        {
            get { return m_SelectedPrefabEntities.Length > 1; }
        }

        /// <summary>
        /// Gets a prefab entity from the selected tree prefabs given a random parameter.
        /// </summary>
        /// <param name="random">A source of randomness.</param>
        /// <returns>A random prefab entity from selected or Enity.null.</returns>
        public Entity GetNextPrefabEntity(ref Unity.Mathematics.Random random)
        {
            if (m_SelectedPrefabEntities.Length > 0)
            {
                int iterations = random.NextInt(10);
                for (int i = 0; i < iterations; i++)
                {
                    random.NextInt();
                }

                Entity result = m_SelectedPrefabEntities[random.NextInt(m_SelectedPrefabEntities.Length)];

                return result;
            }

            return Entity.Null;
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_UiView = GameManager.instance.userInterface.view.View;
            m_ThemeEntities = new List<Entity>();
            m_ToolSystem.EventToolChanged += OnToolChanged;
            m_ToolSystem.EventPrefabChanged += OnPrefabChanged;
            m_SelectedPrefabEntities = new NativeList<Entity>(Allocator.Persistent);

            Enabled = false;
            m_Log.Info($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(OnCreate)}");
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (m_TrySetPrefabNextFrame != null &&
                m_ToolSystem.activePrefab != m_TrySetPrefabNextFrame)
            {
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(OnUpdate)} ActivatedPrefabTool {m_TrySetPrefabNextFrame.name}.");
                m_ToolSystem.ActivatePrefabTool(m_TrySetPrefabNextFrame);
                return;
            }
            else if (m_TrySetPrefabNextFrame != null &&
                     m_ToolSystem.activePrefab == m_TrySetPrefabNextFrame)
            {
                m_TrySetPrefabNextFrame = null;
            }

            if (m_UiView is null)
            {
                m_Log.Info($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(OnUpdate)} m_UiView is null. Tried to reset it.");
                m_UiView = GameManager.instance.userInterface.view.View;
            }

            if (m_ToolSystem.activePrefab != m_LastActivePrefab)
            {
                if (m_ToolSystem.activeTool.toolID != null && m_ToolSystem.activeTool.toolID == "Line Tool")
                {
                    m_ToolSystem.EventPrefabChanged.Invoke(m_ToolSystem.activePrefab);
                }

                m_LastActivePrefab = m_ToolSystem.activePrefab;
            }

            if (((m_ToolSystem.activeTool.toolID is not null && m_ToolSystem.activeTool.toolID == "Line Tool") ||
                (m_ToolSystem.activeTool == m_ObjectToolSystem &&
                (m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush
                || m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Line
                || m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create
                || m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Curve))) &&
                m_UiView != null &&
                m_ToolSystem.activePrefab != null &&
                m_PrefabSystem.TryGetEntity(m_ToolSystem.activePrefab, out Entity prefabEntity) &&
                ReviewPrefab(m_ToolSystem.activePrefab))
            {
                // This script creates the Tree Controller object if it doesn't exist.
                m_UiView.ExecuteScript("if (yyAnarchy == null) var yyAnarchy = {};");

                if (m_UpdateSelectionSet && m_FrameCount <= 5)
                {
                    if (m_FrameCount < 5)
                    {
                        UnselectAllPrefabs();
                    }

                    foreach (Entity entity in m_SelectedPrefabEntities)
                    {
                        TrySelectOrUnselectPrefab(entity);
                    }

                    if (m_FrameCount == 5)
                    {
                        m_UpdateSelectionSet = false;
                        m_RecentlyRemovedPrefabEntity = Entity.Null;
                        m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(OnUpdate)} finished frame set. selectedPrefabs.Count = {m_SelectedPrefabEntities.Length}");
                        m_FrameCount = 6;
                    }
                    else
                    {
                        m_FrameCount++;
                    }
                }
                else if (m_UpdateSelectionSet)
                {
                    if (m_FrameCount == 6)
                    {
                        m_FrameCount = 0;
                    }

                    m_FrameCount++;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_SelectedPrefabEntities.Dispose();
        }

        /// <summary>
        /// Method implemented by event triggered by tool changing.
        /// </summary>
        /// <param name="tool">The new tool.</param>
        private void OnToolChanged(ToolBaseSystem tool)
        {
            m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(OnToolChanged)}");
            HandleToolOrPrefabChange(tool, tool.GetPrefab(), true);
        }

        /// <summary>
        /// Method implemented by event triggered by prefab changing.
        /// </summary>
        /// <param name="prefab">The new prefab.</param>
        private void OnPrefabChanged(PrefabBase prefab)
        {
            m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(OnPrefabChanged)}");
            HandleToolOrPrefabChange(m_ToolSystem.activeTool, prefab, false);
        }

        private void HandleToolOrPrefabChange(ToolBaseSystem tool, PrefabBase prefab, bool toolChange)
        {
            if (tool.toolID is null)
            {
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(HandleToolOrPrefabChange)} toolID is null.");
                Enabled = false;
                UnselectAllPrefabs(clearPrefabs: true);
                return;
            }

            if (prefab is null)
            {
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(HandleToolOrPrefabChange)} prefab is null.");
                Enabled = false;
                UnselectAllPrefabs(clearPrefabs: true);
                return;
            }

            m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(HandleToolOrPrefabChange)} tool.toolID: {tool.toolID} prefab {prefab.name} toolChange: {toolChange}.");

            if (prefab != null &&
                ((tool.toolID is not null && tool.toolID == "Line Tool") ||
                (tool == m_ObjectToolSystem &&
                (m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create
                || m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush
                || m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Line
                || m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Curve
                || m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Upgrade))) &&
                m_PrefabSystem.TryGetEntity(prefab, out Entity prefabEntity) &&
                ReviewPrefab(prefab, logging: true))
            {
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(HandleToolOrPrefabChange)} Passed Filter");
                if (!toolChange &&
                    !Keyboard.current.leftCtrlKey.isPressed &&
                    !Keyboard.current.rightCtrlKey.isPressed)
                {
                    UnselectAllPrefabs(clearPrefabs: true);
                }

                Enabled = true;
                if (m_TrySetPrefabNextFrame is null &&
                    (!toolChange ||
                     prefab != m_ToolSystem.activePrefab))
                {
                    TrySelectOrUnselectPrefab(prefab, selectionStateOnly: false);
                }
                else if (m_TrySetPrefabNextFrame is not null &&
                         prefab is not null)
                {
                    m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(HandleToolOrPrefabChange)} m_TrySetPrefabNextFrame: {m_TrySetPrefabNextFrame.name} Removing selected from {prefab.name}.");
                    m_TrySetPrefabNextFrame = null;

                    // This script creates the Anarchy object if it doesn't exist.
                    m_UiView.ExecuteScript("if (yyAnarchy == null) var yyAnarchy = {};");

                    // This script searches through all img and removes selected if the src of that image contains the name of the prefab and is not the active prefab.
                    m_UiView.ExecuteScript($"yyAnarchy.tagElements = document.getElementsByTagName(\"img\"); for (yyAnarchy.i = 0; yyAnarchy.i < yyAnarchy.tagElements.length; yyAnarchy.i++) {{ if (yyAnarchy.tagElements[yyAnarchy.i].src.includes(\"{ImageSystem.GetThumbnail(prefab)}\")) {{ yyAnarchy.tagElements[yyAnarchy.i].parentNode.classList.remove(\"selected\"); yyAnarchy.tagElements[yyAnarchy.i].parentNode.parentNode.classList.remove(\"selected\");   }} }} ");
                }
            }
            else
            {
                if (m_ToolSystem.activeTool.toolID is not null &&
                    m_ToolSystem.activeTool.toolID != "Line Tool")
                {
                    Enabled = false;
                }

                UnselectAllPrefabs(clearPrefabs: true);
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(HandleToolOrPrefabChange)} Neither");
            }
        }

        /// <summary>
        /// Clears all selected vegetation prefabs.
        /// </summary>
        private void UnselectAllPrefabs(bool clearPrefabs = false)
        {
            m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(UnselectAllPrefabs)} clearPrefabs: {clearPrefabs}.");
            foreach (Entity e in m_SelectedPrefabEntities)
            {
                if (m_PrefabSystem.TryGetPrefab(e, out PrefabBase prefab))
                {
                    // This script creates the Anarchy object if it doesn't exist.
                    m_UiView.ExecuteScript("if (yyAnarchy == null) var yyAnarchy = {};");

                    // This script searches through all img and adds removes selected if the src of that image contains the name of the prefab and is not the active prefab.
                    m_UiView.ExecuteScript($"yyAnarchy.tagElements = document.getElementsByTagName(\"img\"); for (yyAnarchy.i = 0; yyAnarchy.i < yyAnarchy.tagElements.length; yyAnarchy.i++) {{ if (yyAnarchy.tagElements[yyAnarchy.i].src.includes(\"{ImageSystem.GetThumbnail(prefab)}\")) {{ yyAnarchy.tagElements[yyAnarchy.i].parentNode.classList.remove(\"selected\"); yyAnarchy.tagElements[yyAnarchy.i].parentNode.parentNode.classList.remove(\"selected\");   }} }} ");
                }
            }

            if (m_RecentlyRemovedPrefabEntity != Entity.Null &&
                m_PrefabSystem.TryGetPrefab(m_RecentlyRemovedPrefabEntity, out PrefabBase prefab1))
            {
                // This script creates the Anarchy object if it doesn't exist.
                m_UiView.ExecuteScript("if (yyAnarchy == null) var yyAnarchy = {};");

                // This script searches through all img and adds removes selected if the src of that image contains the name of the prefab and is not the active prefab.
                m_UiView.ExecuteScript($"yyAnarchy.tagElements = document.getElementsByTagName(\"img\"); for (yyAnarchy.i = 0; yyAnarchy.i < yyAnarchy.tagElements.length; yyAnarchy.i++) {{ if (yyAnarchy.tagElements[yyAnarchy.i].src.includes(\"{ImageSystem.GetThumbnail(prefab1)}\")) {{ yyAnarchy.tagElements[yyAnarchy.i].parentNode.classList.remove(\"selected\"); yyAnarchy.tagElements[yyAnarchy.i].parentNode.parentNode.classList.remove(\"selected\");   }} }} ");
            }

            if (clearPrefabs)
            {
                m_SelectedPrefabEntities.Clear();
            }

            m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(UnselectAllPrefabs)}");
        }

        private bool ReviewPrefab(PrefabBase prefabBase, bool logging = false)
        {
            if (logging)
            {
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(ReviewPrefab)}");
            }

            if (prefabBase is null)
            {
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(ReviewPrefab)} prefabBase is null.");
                return false;
            }

            if (!m_PrefabSystem.TryGetEntity(prefabBase, out Entity prefabEntity))
            {
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(ReviewPrefab)} couldn't get prefab entity for {prefabBase.name}.");
                return false;
            }

            if (logging)
            {
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(ReviewPrefab)} {prefabBase.name}.");
            }

            if (EntityManager.HasComponent<Game.Prefabs.ObjectData>(prefabEntity) &&
                EntityManager.TryGetComponent(prefabEntity, out Game.Prefabs.UIObjectData uIObjectData) &&
               !EntityManager.HasComponent<Game.Prefabs.TreeData>(prefabEntity) &&
               !EntityManager.HasComponent<Game.Prefabs.PlantData>(prefabEntity) &&
               !EntityManager.HasComponent<Game.Prefabs.PlaceholderObjectData>(prefabEntity) &&
               !EntityManager.HasComponent<Game.Prefabs.SubArea>(prefabEntity) &&
               !EntityManager.HasComponent<Game.Prefabs.SubNet>(prefabEntity))
            {
                if (uIObjectData.m_Group != m_UIGroup)
                {
                    m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(ReviewPrefab)} uIObjectData.m_Group: {uIObjectData.m_Group} != m_UIGroup: {m_UIGroup}.");
                    m_UIGroup = uIObjectData.m_Group;
                    UnselectAllPrefabs(clearPrefabs: true);
                }

                if (logging)
                {
                    m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(ReviewPrefab)} valid prefab.");
                }

                return true;
            }

            if (logging)
            {
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(ReviewPrefab)} invalid prefab.");
            }

            return false;
        }

        /// <summary>
        /// Adds selected to the selected prefab.
        /// </summary>
        /// <param name="prefab">The selected prefab.</param>
        private void TrySelectOrUnselectPrefab(PrefabBase prefab, bool selectionStateOnly = true)
        {
            m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(TrySelectOrUnselectPrefab)}");
            if (prefab == null)
            {
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(TrySelectOrUnselectPrefab)} prefabBase is null.");
                return;
            }

            if (!m_PrefabSystem.TryGetEntity(prefab, out Entity prefabEntity) ||
                !ReviewPrefab(prefab))
            {
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(TrySelectOrUnselectPrefab)} couldn't get prefab entity for {prefab.name} or didn't pass review.");
                return;
            }

            if (selectionStateOnly)
            {
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(TrySelectOrUnselectPrefab)} Selecting {prefab.name} only.");

                // This script creates the Anarchy object if it doesn't exist.
                m_UiView.ExecuteScript("if (yyAnarchy == null) var yyAnarchy = {};");

                // This script searches through all img and adds selected if the src of that image contains the name of the prefab.
                m_UiView.ExecuteScript($"yyAnarchy.tagElements = document.getElementsByTagName(\"img\"); for (yyAnarchy.i = 0; yyAnarchy.i < yyAnarchy.tagElements.length; yyAnarchy.i++) {{ if (yyAnarchy.tagElements[yyAnarchy.i].src.includes(\"{ImageSystem.GetThumbnail(prefab)}\")) {{ yyAnarchy.tagElements[yyAnarchy.i].parentNode.classList.add(\"selected\"); yyAnarchy.tagElements[yyAnarchy.i].parentNode.parentNode.classList.add(\"selected\");  }} }} ");
                return;
            }

            if (!m_SelectedPrefabEntities.Contains(prefabEntity))
            {
                m_SelectedPrefabEntities.Add(prefabEntity);
                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(TrySelectOrUnselectPrefab)} Selecting {prefab.name} and adding to set. New length: {m_SelectedPrefabEntities.Length}.");
                m_UpdateSelectionSet = true;

                // This script creates the Anarchy object if it doesn't exist.
                m_UiView.ExecuteScript("if (yyAnarchy == null) var yyAnarchy = {};");

                // This script searches through all img and adds selected if the src of that image contains the name of the prefab.
                m_UiView.ExecuteScript($"yyAnarchy.tagElements = document.getElementsByTagName(\"img\"); for (yyAnarchy.i = 0; yyAnarchy.i < yyAnarchy.tagElements.length; yyAnarchy.i++) {{ if (yyAnarchy.tagElements[yyAnarchy.i].src.includes(\"{ImageSystem.GetThumbnail(prefab)}\")) {{ yyAnarchy.tagElements[yyAnarchy.i].parentNode.classList.add(\"selected\"); yyAnarchy.tagElements[yyAnarchy.i].parentNode.parentNode.classList.add(\"selected\");  }} }} ");
            }
            else
            {
                m_RecentlyRemovedPrefabEntity = prefabEntity;
                m_SelectedPrefabEntities.RemoveAt(m_SelectedPrefabEntities.IndexOf(prefabEntity));

                m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(TrySelectOrUnselectPrefab)} Unselecting {prefab.name} and removing from set. new length: {m_SelectedPrefabEntities.Length}.");

                foreach (Entity e in m_SelectedPrefabEntities)
                {
                    if (m_PrefabSystem.TryGetPrefab(e, out PrefabBase nextPrefab) &&
                        ReviewPrefab(nextPrefab))
                    {
                        m_TrySetPrefabNextFrame = nextPrefab;
                        m_Log.Debug($"{nameof(SelectMultiplePrefabsUISystem)}.{nameof(TrySelectOrUnselectPrefab)} trying to set prefab to {nextPrefab.name}");
                        break;
                    }
                }

                m_UpdateSelectionSet = true;

                // This script creates the Anarchy object if it doesn't exist.
                m_UiView.ExecuteScript("if (yyAnarchy == null) var yyAnarchy = {};");

                // This script searches through all img and adds removes selected if the src of that image contains the name of the prefab and is not the active prefab.
                m_UiView.ExecuteScript($"yyAnarchy.tagElements = document.getElementsByTagName(\"img\"); for (yyAnarchy.i = 0; yyAnarchy.i < yyAnarchy.tagElements.length; yyAnarchy.i++) {{ if (yyAnarchy.tagElements[yyAnarchy.i].src.includes(\"{ImageSystem.GetThumbnail(prefab)}\")) {{ yyAnarchy.tagElements[yyAnarchy.i].parentNode.classList.remove(\"selected\"); yyAnarchy.tagElements[yyAnarchy.i].parentNode.parentNode.classList.remove(\"selected\");   }} }} ");
            }
        }

        /// <summary>
        /// Tries to select a prefab using a prefab entity.
        /// </summary>
        /// <param name="prefabEntity">Entity for a prefab.</param>
        private void TrySelectOrUnselectPrefab(Entity prefabEntity)
        {
            if (m_PrefabSystem.TryGetPrefab(prefabEntity, out PrefabBase prefabBase) &&
                prefabBase is not null)
            {
                TrySelectOrUnselectPrefab(prefabBase);
            }
        }
    }
}
