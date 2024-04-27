// <copyright file="AnarchyUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems
{
    using System.Collections.Generic;
    using Anarchy.Utils;
    using Colossal.Logging;
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Simulation;
    using Game.Tools;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.InputSystem;

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

        /// <summary>
        /// A list of error types that Anarchy will disable.
        /// </summary>
        private readonly List<ErrorType> AllowableErrorTypes = new ()
        {
            { ErrorType.OverlapExisting },
            { ErrorType.InvalidShape },
            { ErrorType.LongDistance },
            { ErrorType.TightCurve },
            { ErrorType.AlreadyUpgraded },
            { ErrorType.InWater },
            { ErrorType.NoWater },
            { ErrorType.ExceedsCityLimits },
            { ErrorType.NotOnShoreline },
            { ErrorType.AlreadyExists },
            { ErrorType.ShortDistance },
            { ErrorType.LowElevation },
            { ErrorType.SmallArea },
            { ErrorType.SteepSlope },
            { ErrorType.NotOnBorder },
            { ErrorType.NoGroundWater },
            { ErrorType.OnFire },
            { ErrorType.ExceedsLotLimits },
        };

        private ToolSystem m_ToolSystem;
        private ILog m_Log;
        private bool m_DisableAnarchyWhenCompleted;
        private string m_LastTool;
        private BulldozeToolSystem m_BulldozeToolSystem;
        private NetToolSystem m_NetToolSystem;
        private ResetNetCompositionDataSystem m_ResetNetCompositionDataSystem;
        private ValueBinding<bool> m_AnarchyEnabled;
        private ValueBinding<bool> m_ShowToolIcon;
        private ValueBinding<bool> m_FlamingChirperOption;
        private ValueBindingHelper<float> m_ElevationValue;
        private ValueBindingHelper<float> m_ElevationStep;
        private ValueBindingHelper<int> m_ElevationScale;
        private ElevateObjectDefinitionSystem m_ObjectDefinitionSystem;
        private ValueBindingHelper<bool> m_LockElevation;
        private ElevateTempObjectSystem m_ElevateTempObjectSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private bool m_IsBrushing;
        private bool m_BeforeBrushingAnarchyEnabled;
        private int m_ButtonCooldown = 0;

        /// <summary>
        /// Gets a value indicating whether the flaming chirper option binding is on/off.
        /// </summary>
        public bool FlamingChirperOption { get => m_FlamingChirperOption.value; }

        /// <summary>
        /// Gets a value indicating whether the flaming chirper option binding is on/off.
        /// </summary>
        public bool AnarchyEnabled { get => m_AnarchyEnabled.value; }

        /// <summary>
        /// Gets a value indicating the elevation delta.
        /// </summary>
        public float ElevationDelta { get => m_ElevationValue.Value; }

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
            m_FlamingChirperOption.Update(value);
        }

        /// <summary>
        /// Checks the list of appropriate tools and returns true if Anarchy is appliable.
        /// </summary>
        /// <param name="toolID">A string representing a tool id.</param>
        /// <returns>True if anarchy is applicable to that toolID. False if not.</returns>
        public bool IsToolAppropriate(string toolID) => ToolIDs.Contains(toolID);

        /// <summary>
        /// Checks the list of error types that Anarchy disables.
        /// </summary>
        /// <param name="errorType">An Error type enum.</param>
        /// <returns>True if that error type should be disabled by anarchy. False if not.</returns>
        public bool IsErrorTypeAllowed(ErrorType errorType)
        {
            return AllowableErrorTypes.Contains(errorType);
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
            m_ObjectDefinitionSystem = World.GetOrCreateSystemManaged<ElevateObjectDefinitionSystem>();
            m_ElevateTempObjectSystem = World.GetOrCreateSystemManaged<ElevateTempObjectSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_ToolSystem.EventToolChanged += OnToolChanged;
            m_Log.Info($"{nameof(AnarchyUISystem)}.{nameof(OnCreate)}");
            InputAction hotKey = new ("Anarchy");
            hotKey.AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Keyboard>/ctrl").With("Button", "<Keyboard>/a");
            hotKey.performed += OnKeyPressed;
            hotKey.Enable();

            InputAction elevationUpHotKey = new ($"{AnarchyMod.Id}.ElevationUp");
            elevationUpHotKey.AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Keyboard>/shift").With("Button", "<Keyboard>/w");
            elevationUpHotKey.performed += OnElevationUpKeyPressed;
            elevationUpHotKey.Enable();

            InputAction elevationDownHotKey = new ($"{AnarchyMod.Id}.ElevationDown");
            elevationDownHotKey.AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Keyboard>/shift").With("Button", "<Keyboard>/s");
            elevationDownHotKey.performed += OnElevationDownKeyPressed;
            elevationDownHotKey.Enable();

            InputAction elevationResetHotKey = new ($"{AnarchyMod.Id}.ElevationReset");
            elevationResetHotKey.AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Keyboard>/shift").With("Button", "<Keyboard>/r");
            elevationResetHotKey.performed += OnElevationResetKeyPressed;
            elevationResetHotKey.Enable();

            InputAction elevationStepHotKey = new($"{AnarchyMod.Id}.ElevationStep");
            elevationStepHotKey.AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Keyboard>/shift").With("Button", "<Keyboard>/f");
            elevationStepHotKey.performed += OnElevationStepKeyPressed;
            elevationStepHotKey.Enable();

            // This binding communicates values between UI and C#
            AddBinding(m_AnarchyEnabled = new ValueBinding<bool>("Anarchy", "AnarchyEnabled", false));
            AddBinding(m_ShowToolIcon = new ValueBinding<bool>("Anarchy", "ShowToolIcon", false));
            AddBinding(m_FlamingChirperOption = new ValueBinding<bool>("Anarchy", "FlamingChirperOption", AnarchyMod.Instance.Settings.FlamingChirper));
            m_ElevationValue = CreateBinding("ElevationValue", 0f);
            m_ElevationStep = CreateBinding("ElevationStep", 10f);
            m_ElevationScale = CreateBinding("ElevationScale", 1);
            m_LockElevation = CreateBinding("LockElevation", false);

            // This binding listens for events triggered by the UI.
            AddBinding(new TriggerBinding("Anarchy", "AnarchyToggled", AnarchyToggled));
            CreateTrigger("IncreaseElevation", () => ChangeElevation(m_ElevationValue.Value + m_ElevationStep.Value, m_ElevationStep.Value));
            CreateTrigger("DecreaseElevation", () => ChangeElevation(m_ElevationValue.Value - m_ElevationStep.Value, -1f * m_ElevationStep.Value));
            CreateTrigger("LockElevationToggled", () => m_LockElevation.Value = !m_LockElevation.Value);
            CreateTrigger("ElevationStep", ElevationStepPressed);
            CreateTrigger("ResetElevationToggled", () => m_ElevationValue.Value = 0f);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (AnarchyMod.Instance.Settings.DisableAnarchyWhileBrushing && m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush && !m_IsBrushing)
            {
                m_IsBrushing = true;
                m_BeforeBrushingAnarchyEnabled = m_AnarchyEnabled.value;
                m_AnarchyEnabled.Update(false);
            }

            if ((m_IsBrushing && m_ToolSystem.activeTool != m_ObjectToolSystem) || (m_IsBrushing && m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush))
            {
                m_AnarchyEnabled.Update(m_BeforeBrushingAnarchyEnabled);
                m_IsBrushing = false;
            }

            base.OnUpdate();
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
                m_IsBrushing = false;
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

            if (IsToolAppropriate(tool.toolID) && AnarchyMod.Instance.Settings.ToolIcon)
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

            // Implements Anarchic Bulldozer when bulldoze tool is activated.
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

        private void OnKeyPressed(InputAction.CallbackContext context)
        {
            if (m_ToolSystem.activeTool.toolID != null)
            {
                if (ToolIDs.Contains(m_ToolSystem.activeTool.toolID))
                {
                    AnarchyToggled();
                }
            }
        }

        private void OnElevationUpKeyPressed(InputAction.CallbackContext context)
        {
            if (m_ToolSystem.activeTool.toolID != null)
            {
                if ((m_ToolSystem.activeTool == m_ObjectToolSystem || m_ToolSystem.activeTool.toolID == "Line Tool") && m_ToolSystem.activePrefab is not BuildingPrefab)
                {
                    ChangeElevation(m_ElevationValue.Value + m_ElevationStep.Value, m_ElevationStep.Value);
                }
            }
        }

        private void OnElevationDownKeyPressed(InputAction.CallbackContext context)
        {
            if (m_ToolSystem.activeTool.toolID != null)
            {
                if ((m_ToolSystem.activeTool == m_ObjectToolSystem || m_ToolSystem.activeTool.toolID == "Line Tool") && m_ToolSystem.activePrefab is not BuildingPrefab)
                {
                    ChangeElevation(m_ElevationValue.Value - m_ElevationStep.Value, -1f * m_ElevationStep.Value);
                }
            }
        }

        private void OnElevationResetKeyPressed(InputAction.CallbackContext context)
        {
            if (m_ToolSystem.activeTool.toolID != null)
            {
                if ((m_ToolSystem.activeTool == m_ObjectToolSystem || m_ToolSystem.activeTool.toolID == "Line Tool") && m_ToolSystem.activePrefab is not BuildingPrefab)
                {
                    ChangeElevation(0f, m_ElevationValue.Value * -1f);
                }
            }
        }

        private void OnElevationStepKeyPressed(InputAction.CallbackContext context)
        {
            if (m_ToolSystem.activeTool.toolID != null)
            {
                if ((m_ToolSystem.activeTool == m_ObjectToolSystem || m_ToolSystem.activeTool.toolID == "Line Tool") && m_ToolSystem.activePrefab is not BuildingPrefab)
                {
                    ElevationStepPressed();
                }
            }
        }

        private void ChangeElevation(float value, float difference)
        {
            m_ElevationValue.UpdateCallback(value);
            m_ElevateTempObjectSystem.ElevationChange = difference;
            m_ElevateTempObjectSystem.Enabled = true;
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

    }
}
