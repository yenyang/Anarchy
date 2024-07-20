// <copyright file="AnarchyUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using Anarchy.Domain;
    using Anarchy.Settings;
    using Anarchy.Utils;
    using Colossal.Logging;
    using Colossal.PSI.Environment;
    using Colossal.Serialization.Entities;
    using Colossal.UI.Binding;
    using Game;
    using Game.Input;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// UI system for Anarchy.
    /// </summary>
    public partial class AnarchyUISystem : ExtendedUISystemBase
    {
        /// <summary>
        /// A list of tools ids that Anarchy is applicable to.
        /// </summary>
        private readonly List<string> ToolIDs = new ()
        {
            { "Object Tool" },
            { "Net Tool" },
            { "Area Tool" },
            { "Bulldoze Tool" },
            { "Terrain Tool" },
            { "Upgrade Tool" },
            { "Line Tool" },
        };

        private readonly ErrorCheck[] DefaultErrorChecks = new ErrorCheck[]
        {
            new (ErrorType.AlreadyExists, ErrorCheck.DisableState.WithAnarchy, 0),
            new (ErrorType.AlreadyUpgraded, ErrorCheck.DisableState.WithAnarchy, 1),
            new (ErrorType.ExceedsCityLimits, ErrorCheck.DisableState.WithAnarchy, 2),
            new (ErrorType.ExceedsLotLimits, ErrorCheck.DisableState.WithAnarchy, 3),
            new (ErrorType.InvalidShape, ErrorCheck.DisableState.WithAnarchy, 4),
            new (ErrorType.InWater, ErrorCheck.DisableState.WithAnarchy, 5),
            new (ErrorType.LongDistance, ErrorCheck.DisableState.WithAnarchy, 6),
            new (ErrorType.LowElevation, ErrorCheck.DisableState.WithAnarchy, 7),
            new (ErrorType.NoCarAccess, ErrorCheck.DisableState.Never, 8),
            new (ErrorType.NoCargoAccess, ErrorCheck.DisableState.Never, 9),
            new (ErrorType.NoGroundWater, ErrorCheck.DisableState.WithAnarchy, 10),
            new (ErrorType.NoPedestrianAccess, ErrorCheck.DisableState.Never, 11),
            new (ErrorType.NoRoadAccess, ErrorCheck.DisableState.Never, 12),
            new (ErrorType.NotOnBorder, ErrorCheck.DisableState.WithAnarchy, 13),
            new (ErrorType.NotOnShoreline, ErrorCheck.DisableState.WithAnarchy, 14),
            new (ErrorType.NoTrackAccess, ErrorCheck.DisableState.Never, 15),
            new (ErrorType.NoTrainAccess, ErrorCheck.DisableState.Never, 16),
            new (ErrorType.NoWater, ErrorCheck.DisableState.WithAnarchy, 17),
            new (ErrorType.OnFire, ErrorCheck.DisableState.WithAnarchy, 18),
            new (ErrorType.OverlapExisting, ErrorCheck.DisableState.WithAnarchy, 19),
            new (ErrorType.PathfindFailed, ErrorCheck.DisableState.Never, 20),
            new (ErrorType.ShortDistance, ErrorCheck.DisableState.WithAnarchy, 21),
            new (ErrorType.SmallArea, ErrorCheck.DisableState.WithAnarchy, 22),
            new (ErrorType.SteepSlope, ErrorCheck.DisableState.WithAnarchy, 23),
            new (ErrorType.TightCurve, ErrorCheck.DisableState.WithAnarchy, 24),
        };

        private ToolSystem m_ToolSystem;
        private ILog m_Log;
        private bool m_DisableAnarchyWhenCompleted;
        private string m_LastTool;
        private BulldozeToolSystem m_BulldozeToolSystem;
        private NetToolSystem m_NetToolSystem;
        private ResetNetCompositionDataSystem m_ResetNetCompositionDataSystem;
        private EnableToolErrorsSystem m_EnableToolErrorsSystem;
        private AnarchyPlopSystem m_AnarchyPlopSystem;
        private bool m_AnarchyEnabled;
        private ValueBindingHelper<bool> m_AnarchyBinding;
        private ValueBindingHelper<bool> m_ShowToolIcon;
        private ValueBindingHelper<bool> m_FlamingChirperOption;
        private ValueBindingHelper<float> m_ElevationValue;
        private ValueBindingHelper<float> m_ElevationStep;
        private string m_ContentFolder;
        private ValueBindingHelper<int> m_ElevationScale;
        private ValueBindingHelper<bool> m_IsBuildingPrefab;
        private ValueBindingHelper<bool> m_ShowElevationSettingsOption;
        private ValueBindingHelper<bool> m_ObjectToolCreateOrBrushMode;
        private ValueBindingHelper<bool> m_DisableElevationLock;
        private ValueBindingHelper<bool> m_MultipleUniques;
        private ElevateObjectDefinitionSystem m_ElevateObjectDefinitionSystem;
        private ValueBindingHelper<bool> m_LockElevation;
        private ElevateTempObjectSystem m_ElevateTempObjectSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private bool m_IsBrushing;
        private bool m_BeforeBrushingAnarchyEnabled;
        private PrefabBase m_PreviousPrefab;
        private ProxyAction m_ToggleAnarchy;
        private ProxyAction m_ResetElevation;
        private ProxyAction m_ElevationStepToggle;
        private ProxyAction m_ElevationKey;
        private ValueBindingHelper<ErrorCheck[]> m_ErrorChecksBinding;
        private bool m_UpdateErrorChecks;
        private ValueBindingHelper<bool> m_ShowAnarchyToggleOptionsPanel;
        private Dictionary<ErrorType, ErrorCheck.DisableState> m_DefaultErrorChecks;

        /// <summary>
        /// Gets a value indicating whether the flaming chirper option binding is on/off.
        /// </summary>
        public bool FlamingChirperOption { get => m_FlamingChirperOption.Value; }

        /// <summary>
        /// Gets a value indicating whether Anarchy is enabled or not.
        /// </summary>
        public bool AnarchyEnabled { get => m_AnarchyEnabled; }

        /// <summary>
        /// Gets a value indicating whether the elevation should be locked.
        /// </summary>
        public bool LockElevation { get => m_LockElevation.Value; }

        /// <summary>
        /// Sets the flaming chirper option binding to value.
        /// </summary>
        /// <param name="value">True for option enabled. false if not.</param>
        public void SetFlamingChirperOption(bool value)
        {
            // This updates the flaming chirper option binding. It is triggered in the settings by overriding Apply.
            m_FlamingChirperOption.Value = value;
        }

        /// <summary>
        /// Sets the prevent Override option binding to value.
        /// </summary>
        /// <param name="value">True for option enabled. false if not.</param>
        public void SetDisableElevationLock(bool value)
        {
            if (m_ToolSystem.actionMode.IsEditor() && value == false)
            {
                m_DisableElevationLock.Value = true;
                return;
            }

            m_DisableElevationLock.Value = false;
        }

        /// <summary>
        /// Sets the binding for multiple uniques.
        /// </summary>
        /// <param name="value">Toggle of setting.</param>
        public void SetMultipleUniques(bool value)
        {
            m_MultipleUniques.Value = value;
        }

        /// <summary>
        /// Sets the Show Elevation Settings option binding to value.
        /// </summary>
        /// <param name="value">True for option enabled. false if not.</param>
        public void SetShowElevationSettingsOption(bool value)
        {
            m_ShowElevationSettingsOption.Value = value;
            m_ElevateObjectDefinitionSystem.Enabled = value;
            m_ElevateTempObjectSystem.Enabled = false;
            m_ElevationValue.Value = 0f;
        }

        /// <summary>
        /// Checks the list of appropriate tools and returns true if Anarchy is appliable.
        /// </summary>
        /// <param name="toolID">A string representing a tool id.</param>
        /// <returns>True if anarchy is applicable to that toolID. False if not.</returns>
        public bool IsToolAppropriate(string toolID) => ToolIDs.Contains(toolID);

        /// <summary>
        /// Gets a list of error types that should be disabled.
        /// </summary>
        /// <returns>List or error types.</returns>
        public List<ErrorType> GetAllowableErrorTypes()
        {
            List<ErrorType> allowableErrors = new List<ErrorType>();

            foreach (ErrorCheck check in m_ErrorChecksBinding.Value)
            {
                if (check.DisabledState == 0)
                {
                    continue;
                }
                else if (check.DisabledState == 1 && m_AnarchyEnabled)
                {
                    allowableErrors.Add((ErrorType)check.ID);
                    continue;
                }
                else if (check.DisabledState == 2)
                {
                    allowableErrors.Add((ErrorType)check.ID);
                }
            }

            return allowableErrors;
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_BulldozeToolSystem = World.GetOrCreateSystemManaged<BulldozeToolSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_ResetNetCompositionDataSystem = World.GetOrCreateSystemManaged<ResetNetCompositionDataSystem>();
            m_ElevateObjectDefinitionSystem = World.GetOrCreateSystemManaged<ElevateObjectDefinitionSystem>();
            m_ElevateTempObjectSystem = World.GetOrCreateSystemManaged<ElevateTempObjectSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_AnarchyPlopSystem = World.GetOrCreateSystemManaged<AnarchyPlopSystem>();
            m_EnableToolErrorsSystem = World.GetOrCreateSystemManaged<EnableToolErrorsSystem>();
            m_ToolSystem.EventToolChanged += OnToolChanged;
            m_ToolSystem.EventPrefabChanged += OnPrefabChanged;
            m_Log.Info($"{nameof(AnarchyUISystem)}.{nameof(OnCreate)}");
            m_ContentFolder = Path.Combine(EnvPath.kUserDataPath, "ModsData", AnarchyMod.Id, "ErrorChecks");
            System.IO.Directory.CreateDirectory(m_ContentFolder);
            PopulateErrorCheckDictionary();

            // This binding communicates values between UI and C#
            m_AnarchyBinding = CreateBinding("AnarchyEnabled", false);
            m_ShowToolIcon = CreateBinding("ShowToolIcon", false);
            m_FlamingChirperOption = CreateBinding("FlamingChirperOption", AnarchyMod.Instance.Settings.FlamingChirper);
            m_ElevationValue = CreateBinding("ElevationValue", 0f);
            m_ElevationStep = CreateBinding("ElevationStep", 10f);
            m_ElevationScale = CreateBinding("ElevationScale", 1);
            m_DisableElevationLock = CreateBinding("DisableElevationLock", false);
            m_LockElevation = CreateBinding("LockElevation", AnarchyMod.Instance.Settings.ElevationLock);
            m_IsBuildingPrefab = CreateBinding("IsBuilding", false);
            m_MultipleUniques = CreateBinding("MultipleUniques", AnarchyMod.Instance.Settings.AllowPlacingMultipleUniqueBuildings);
            m_ShowElevationSettingsOption = CreateBinding("ShowElevationSettingsOption", AnarchyMod.Instance.Settings.ShowElevationToolOption);
            m_ObjectToolCreateOrBrushMode = CreateBinding("ObjectToolCreateOrBrushMode", m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create || m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush);
            ErrorCheck[] errorChecks = DefaultErrorChecks;
            for (int i = 0; i < errorChecks.Length; i++)
            {
                TryLoadErrorCheck(ref errorChecks[i]);
            }

            m_ErrorChecksBinding = CreateBinding("ErrorChecks", errorChecks);
            m_ShowAnarchyToggleOptionsPanel = CreateBinding("ShowAnarchyToggleOptionsPanel", false);

            // This binding listens for events triggered by the UI.
            AddBinding(new TriggerBinding("Anarchy", "AnarchyToggled", AnarchyToggled));
            CreateTrigger("IncreaseElevation", () => ChangeElevation(m_ElevationStep.Value));
            CreateTrigger("DecreaseElevation", () => ChangeElevation(-1f * m_ElevationStep.Value));
            CreateTrigger("LockElevationToggled", () =>
            {
                m_LockElevation.Value = !m_LockElevation.Value;
                AnarchyMod.Instance.Settings.ElevationLock = m_LockElevation.Value;
            });
            CreateTrigger("ElevationStep", ElevationStepPressed);
            CreateTrigger("ResetElevationToggled", () => ChangeElevation(-1f * m_ElevationValue.Value));

            m_ToggleAnarchy = AnarchyMod.Instance.Settings.GetAction(AnarchyModSettings.ToggleAnarchyActionName);
            m_ResetElevation = AnarchyMod.Instance.Settings.GetAction(AnarchyModSettings.ResetElevationActionName);
            m_ElevationStepToggle = AnarchyMod.Instance.Settings.GetAction(AnarchyModSettings.ElevationStepActionName);
            m_ElevationKey = AnarchyMod.Instance.Settings.GetAction(AnarchyModSettings.ElevationActionName);
            CreateTrigger("ResetElevationToggled", () => ChangeElevation(-1f * m_ElevationValue.Value));
            CreateTrigger<int, int>("ChangeDisabledState", ChangeDisabledState);
            CreateTrigger("ToggleAnarchyOptionsPanel", () => m_ShowAnarchyToggleOptionsPanel.Value = !m_ShowAnarchyToggleOptionsPanel.Value);
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            m_ToggleAnarchy.shouldBeEnabled = mode.IsGameOrEditor();

            if (mode.IsEditor() && !AnarchyMod.Instance.Settings.PreventOverrideInEditor)
            {
                m_DisableElevationLock.Value = true;
                return;
            }

            m_DisableElevationLock.Value = false;
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (AnarchyMod.Instance.Settings.DisableAnarchyWhileBrushing && m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush && !m_IsBrushing)
            {
                m_IsBrushing = true;
                m_BeforeBrushingAnarchyEnabled = m_AnarchyEnabled;
                m_AnarchyEnabled = false;
            }

            if ((m_IsBrushing && m_ToolSystem.activeTool != m_ObjectToolSystem) || (m_IsBrushing && m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush))
            {
                if (!m_AnarchyEnabled && m_BeforeBrushingAnarchyEnabled)
                {
                    m_AnarchyEnabled = true;
                }

                m_IsBrushing = false;
            }

            if (m_ObjectToolCreateOrBrushMode.Value != (m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create || m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush))
            {
                m_ObjectToolCreateOrBrushMode.Value = m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create || m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush;
            }

            if (m_ToggleAnarchy.WasPerformedThisFrame())
            {
                AnarchyToggled();
            }

            if (m_ToolSystem.activeTool.toolID != null && (m_ToolSystem.activeTool == m_ObjectToolSystem || m_ToolSystem.activeTool.toolID == "Line Tool") && m_ToolSystem.activePrefab is not BuildingPrefab)
            {
                if (m_ResetElevation.WasPerformedThisFrame())
                {
                    ChangeElevation(m_ElevationValue.Value * -1f);
                }

                if (m_ElevationStepToggle.WasPerformedThisFrame())
                {
                    ElevationStepPressed();
                }

                if (m_ElevationKey.WasPerformedThisFrame())
                {
                    ChangeElevation(m_ElevationStep.Value * m_ElevationKey.ReadValue<float>());
                }
            }

            if (m_AnarchyEnabled != m_AnarchyBinding.Value)
            {
                m_AnarchyBinding.Value = m_AnarchyEnabled;
            }

            base.OnUpdate();
        }

        /// <summary>
        /// An event to Toggle Anarchy.
        /// </summary>
        /// <param name="flag">A bool for whether it's enabled or not.</param>
        private void AnarchyToggled()
        {
            // This updates the field that holds the acutal value. The binding will be updated on the next update. This ensures the binding actually has the right value as trying to change it off update has been causing issues.
            m_AnarchyEnabled = !m_AnarchyEnabled;
            if (!m_AnarchyEnabled)
            {
                m_DisableAnarchyWhenCompleted = false;
                m_ResetNetCompositionDataSystem.Enabled = true;
                m_IsBrushing = false;
            }
        }

        private void OnToolChanged(ToolBaseSystem tool)
        {
            if (tool == null || tool.toolID == null)
            {
                // This updates the Show Tool Icon binding to not show the tool icon.
                m_ShowToolIcon.Value = false;
                return;
            }

            if (IsToolAppropriate(tool.toolID) && AnarchyMod.Instance.Settings.ToolIcon)
            {
                // This updates the Show Tool Icon binding to show the tool icon.
                m_ShowToolIcon.Value = true;
            }
            else
            {
                // This updates the Show Tool Icon binding to not show the tool icon.
                m_ShowToolIcon.Value = false;
            }

            if (tool != m_BulldozeToolSystem && m_DisableAnarchyWhenCompleted)
            {
                m_DisableAnarchyWhenCompleted = false;

                // This updates the Anarchy Enabled binding to its inverse.
                AnarchyToggled();
            }

            // Implements Anarchic Bulldozer when bulldoze tool is activated.
            if (AnarchyMod.Instance.Settings.AnarchicBulldozer && m_AnarchyEnabled == false && tool == m_BulldozeToolSystem)
            {
                m_DisableAnarchyWhenCompleted = true;

                // This updates the Anarchy Enabled binding to its inverse.
                AnarchyToggled();
            }

            if (tool != m_NetToolSystem && m_LastTool == m_NetToolSystem.toolID)
            {
                m_ResetNetCompositionDataSystem.Enabled = true;
            }

            if (m_ToolSystem.activePrefab is BuildingPrefab && m_IsBuildingPrefab.Value == false)
            {
                m_IsBuildingPrefab.Value = true;
            }
            else if (m_IsBuildingPrefab.Value == true && m_ToolSystem.activePrefab is not BuildingPrefab)
            {
                m_IsBuildingPrefab.Value = false;
            }

            if (tool.toolID != null && AnarchyMod.Instance.Settings.ResetElevationWhenChangingPrefab)
            {
                if ((tool == m_ObjectToolSystem || tool.toolID == "Line Tool") && m_ToolSystem.activePrefab is not BuildingPrefab && m_ToolSystem.activePrefab != m_PreviousPrefab)
                {
                    ChangeElevation(m_ElevationValue.Value * -1f);
                    m_PreviousPrefab = m_ToolSystem.activePrefab;
                }
            }

            if ((tool == m_ObjectToolSystem || tool.toolID == "Line Tool") && m_ToolSystem.activePrefab is not BuildingPrefab)
            {
                m_ResetElevation.shouldBeEnabled = true;
                m_ElevationStepToggle.shouldBeEnabled = true;
                m_ElevationKey.shouldBeEnabled = true;
            }
            else
            {
                m_ResetElevation.shouldBeEnabled = false;
                m_ElevationStepToggle.shouldBeEnabled = false;
                m_ElevationKey.shouldBeEnabled = false;
            }

            m_EnableToolErrorsSystem.Enabled = true;
            m_LastTool = tool.toolID;
        }

        private void OnPrefabChanged(PrefabBase prefabBase)
        {
            if (prefabBase is BuildingPrefab && m_IsBuildingPrefab.Value == false)
            {
                m_IsBuildingPrefab.Value = true;
            }
            else if (m_IsBuildingPrefab.Value == true && prefabBase is not BuildingPrefab)
            {
                m_IsBuildingPrefab.Value = false;
            }

            if (m_ToolSystem.activeTool.toolID != null && AnarchyMod.Instance.Settings.ResetElevationWhenChangingPrefab)
            {
                if ((m_ToolSystem.activeTool == m_ObjectToolSystem || m_ToolSystem.activeTool.toolID == "Line Tool") && m_ToolSystem.activePrefab is not BuildingPrefab && prefabBase != m_PreviousPrefab)
                {
                    ChangeElevation(m_ElevationValue.Value * -1f);
                    m_PreviousPrefab = prefabBase;
                }
            }

            if ((m_ToolSystem.activeTool == m_ObjectToolSystem || m_ToolSystem.activeTool.toolID == "Line Tool") && prefabBase is not BuildingPrefab)
            {
                m_ResetElevation.shouldBeEnabled = true;
                m_ElevationStepToggle.shouldBeEnabled = true;
                m_ElevationKey.shouldBeEnabled = true;
            }
            else
            {
                m_ResetElevation.shouldBeEnabled = false;
                m_ElevationStepToggle.shouldBeEnabled = false;
                m_ElevationKey.shouldBeEnabled = false;
            }
        }

        private void ChangeElevation(float difference)
        {
            if (AnarchyMod.Instance.Settings.ShowElevationToolOption)
            {
                m_ElevationValue.Value += difference;

                // I don't know why this is necessary. There seems to be a disconnect that forms in the binding value between C# and UI when the value is changed during onUpdate.
                m_ElevateObjectDefinitionSystem.ElevationDelta = m_ElevationValue.Value;
                m_ElevateTempObjectSystem.ElevationChange = difference;
                m_AnarchyPlopSystem.ElevationChangeIsNegative = m_ElevationValue < 0f;

                // This prevents a crash in the editor but doesn't actually provide the positive effect of the system.
                if (m_ToolSystem.actionMode.IsGame())
                {
                    m_ElevateTempObjectSystem.Enabled = true;
                }
            }
        }

        private void ElevationStepPressed()
        {
            float tempValue = m_ElevationStep.Value;
            if (Mathf.Approximately(tempValue, 10f))
            {
                tempValue = 2.5f;
            }
            else if (Mathf.Approximately(tempValue, 2.5f))
            {
                tempValue = 1.0f;
            }
            else if (Mathf.Approximately(tempValue, 1.0f))
            {
                tempValue = 0.1f;
            }
            else
            {
                tempValue = 10f;
            }

            m_ElevationStep.Value = tempValue;
        }

        private void ChangeDisabledState(int index, int disabledState)
        {
            if (m_ErrorChecksBinding.Value.Length > index)
            {
                m_ErrorChecksBinding.Value[index].DisabledState = disabledState;
                TrySaveErrorCheck(m_ErrorChecksBinding.Value[index]);
            }
        }

        private void PopulateErrorCheckDictionary()
        {
            m_DefaultErrorChecks = new Dictionary<ErrorType, ErrorCheck.DisableState>();
            for (int i = 0; i < DefaultErrorChecks.Length; i++)
            {
                m_DefaultErrorChecks.Add(DefaultErrorChecks[i].GetErrorType(), DefaultErrorChecks[i].GetDisableState());
            }
        }

        /// <summary>
        /// Tries to save a custom color set.
        /// </summary>
        /// <param name="errorCheck"> Error type, disabled state, etc.</param>
        /// <returns>True if saved, false if error occured.</returns>
        private bool TrySaveErrorCheck(ErrorCheck errorCheck)
        {
            ErrorType errorType = (ErrorType)errorCheck.ID;
            string filePath = Path.Combine(m_ContentFolder, errorType.ToString()+ ".xml");

            if (m_DefaultErrorChecks.ContainsKey((ErrorType)errorCheck.ID) && m_DefaultErrorChecks[errorType] == errorCheck.GetDisableState())
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        m_Log.Warn($"{nameof(AnarchyUISystem)}.{nameof(TrySaveErrorCheck)} Could not delete file for Set {errorType}. Encountered exception {ex}");
                    }
                }

                return false;
            }

            try
            {
                XmlSerializer serTool = new XmlSerializer(typeof(ErrorCheck)); // Create serializer
                using (System.IO.FileStream file = System.IO.File.Create(filePath)) // Create file
                {
                    serTool.Serialize(file, errorCheck); // Serialize whole properties
                }

                m_Log.Debug($"{nameof(AnarchyUISystem)}.{nameof(TrySaveErrorCheck)} saved color set for {errorType}.");
                return true;
            }
            catch (Exception ex)
            {
                m_Log.Warn($"{nameof(AnarchyUISystem)}.{nameof(TrySaveErrorCheck)} Could not save values for {errorType}. Encountered exception {ex}");
                return false;
            }
        }

        private bool TryLoadErrorCheck(ref ErrorCheck errorCheck)
        {
            string filePath = Path.Combine(m_ContentFolder, ((ErrorType)errorCheck.ID).ToString() + ".xml");
            if (File.Exists(filePath))
            {
                try
                {
                    XmlSerializer serTool = new XmlSerializer(typeof(ErrorCheck)); // Create serializer
                    using System.IO.FileStream readStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open); // Open file
                    errorCheck = (ErrorCheck)serTool.Deserialize(readStream); // Des-serialize to new Properties

                    // m_Log.Debug($"{nameof(SelectedInfoPanelColorFieldsSystem)}.{nameof(TryLoadCustomColorSet)} loaded color set for {assetSeasonIdentifier.m_PrefabID}.");
                    return true;
                }
                catch (Exception ex)
                {
                    m_Log.Warn($"{nameof(AnarchyUISystem)}.{nameof(TryLoadErrorCheck)} Could not get default values for Set {(ErrorType)errorCheck.ID}. Encountered exception {ex}");
                    return false;
                }
            }

            return false;
        }

    }
}
