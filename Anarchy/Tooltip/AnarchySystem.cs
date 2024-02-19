// <copyright file="AnarchySystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Tooltip
{
    using System.Collections.Generic;
    using Anarchy;
    using Anarchy.Systems;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Entities;
    using Unity.Jobs;
    using UnityEngine.InputSystem;

    /// <summary>
    /// A tool system for checking the keybind.
    /// </summary>
    public partial class AnarchySystem : ToolBaseSystem
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

        private ILog m_Log;
        private AnarchyUISystem m_AnarchyUISystem;
        private ResetNetCompositionDataSystem m_ResetNetCompositionDataSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnarchySystem"/> class.
        /// </summary>
        public AnarchySystem()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether Anarchy is enabled.
        /// </summary>
        public bool AnarchyEnabled { get; set; } = false;

        /// <inheritdoc/>
        public override string toolID => "Anarchy Tool";

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
        public override PrefabBase GetPrefab()
        {
            return null;
        }

        /// <inheritdoc/>
        public override bool TrySetPrefab(PrefabBase prefab)
        {
            return false;
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Logger;
            m_AnarchyUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<AnarchyUISystem>();
            m_ResetNetCompositionDataSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ResetNetCompositionDataSystem>();
            m_Log.Info($"{nameof(AnarchySystem)} System Created.");
            AnarchyEnabled = false;
            InputAction hotKey = new ("Anarchy");
            hotKey.AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Keyboard>/ctrl").With("Button", "<Keyboard>/a");
            hotKey.performed += this.OnKeyPressed;
            hotKey.Enable();
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return inputDeps;
        }

        /// <inheritdoc/>
        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            m_Log.Info($"{nameof(AnarchySystem)} System OnGamePreload.");
            base.OnGamePreload(purpose, mode);
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void OnKeyPressed(InputAction.CallbackContext context)
        {
            if (this.m_ToolSystem.activeTool.toolID != null)
            {
                if (ToolIDs.Contains(this.m_ToolSystem.activeTool.toolID))
                {
                    AnarchyEnabled = !AnarchyEnabled;
                    m_AnarchyUISystem.ToggleAnarchyButton();
                    if (AnarchyEnabled)
                    {
                        m_Log.Debug("Anarchy Enabled.");
                    }
                    else
                    {
                        m_Log.Debug("Anarchy Disabled.");
                        m_ResetNetCompositionDataSystem.Enabled = true;
                    }
                }
            }
        }
    }
}
