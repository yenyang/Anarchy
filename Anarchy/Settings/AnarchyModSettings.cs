// <copyright file="AnarchyModSettings.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Settings
{
    using Anarchy.Systems;
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using Game.UI;
    using Unity.Entities;

    /// <summary>
    /// The mod settings for the Anarchy Mod.
    /// </summary>
    [FileLocation("Mods_Yenyang_Anarchy")]
    [SettingsUITabOrder(General, UI)]
    [SettingsUISection(UI, General)]
    [SettingsUIGroupOrder(Stable, Reset)]
    public class AnarchyModSettings : ModSetting
    {
        /// <summary>
        /// This is for UI Settings.
        /// </summary>
        public const string UI = "UI";

        /// <summary>
        /// This is for general settings.
        /// </summary>
        public const string General = "General";


        /// <summary>
        /// This is for general settings.
        /// </summary>
        public const string Keybinds = "Keybinds";

        /// <summary>
        /// This is for settings that are stable.
        /// </summary>
        public const string Stable = "Stable";

        /// <summary>
        /// This is for reseting settings button group.
        /// </summary>
        public const string Reset = "Reset";


        /// <summary>
        /// Initializes a new instance of the <see cref="AnarchyModSettings"/> class.
        /// </summary>
        /// <param name="mod">AnarchyMod.</param>
        public AnarchyModSettings(IMod mod)
            : base(mod)
        {
            SetDefaults();
        }

        /// <summary>
        /// Gets or sets a value indicating whether Anarchy should always be enabled when using Bulldozer tool.
        /// </summary>
        [SettingsUISection(UI, Stable)]
        public bool AnarchicBulldozer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the tooltip.
        /// </summary>
        [SettingsUISection(UI, Stable)]
        public bool ShowTooltip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to have chirper be on fire. This is currently hidden as it is not implemented and hidding it doesn't break people's existing settings.
        /// </summary>
        [SettingsUISection(UI, Stable)]
        [SettingsUISetter(typeof(AnarchyModSettings), nameof(SetFlamingChirper))]
        public bool FlamingChirper { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the tool icon.
        [SettingsUISection(UI, Stable)]
        public bool ToolIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show Elevation tool option.
        [SettingsUISection(UI, Stable)]
        [SettingsUISetter(typeof(AnarchyModSettings), nameof(SetShowElevationToolOption))]
        public bool ShowElevationToolOption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to reset elevation when changing prefab.
        [SettingsUISection(UI, Stable)]
        [SettingsUIHideByCondition(typeof(AnarchyModSettings), nameof(IsElevationToolOptionNotShown))]
        public bool ResetElevationWhenChangingPrefab { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable anarchy while brushing.
        /// </summary>
        [SettingsUISection(UI, Stable)]
        public bool DisableAnarchyWhileBrushing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the minimum clearance below elevated network.
        /// </summary>
        [SettingsUISection(General, Stable)]
        [SettingsUISlider(min = 0f, max = 1.75f, step = 0.25f, scalarMultiplier = 1, unit = Unit.kFloatTwoFractions)]
        public float MinimumClearanceBelowElevatedNetworks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to prevent prop culling.
        /// </summary>
        [SettingsUISection(General, Stable)]
        public bool PreventAccidentalPropCulling { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the frequency to update props.
        /// </summary>
        [SettingsUISection(General, Stable)]
        [SettingsUISlider(min = 1, max = 600, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUIHideByCondition(typeof(AnarchyModSettings), nameof(IsCullingNotBeingPrevented))]
        public int PropRefreshFrequency { get; set; }

        /// <summary>
        /// Sets a value indicating whether: to update props now.
        /// </summary>
        [SettingsUISection(General, Stable)]
        [SettingsUIButton]
        public bool RefreshProps
        {
            set
            {
                PreventCullingSystem preventCullingSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PreventCullingSystem>();
                preventCullingSystem.RunNow = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use allow placing multiple unique buildings.
        /// </summary>
        [SettingsUISection(General, Stable)]
        public bool AllowPlacingMultipleUniqueBuildings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to prevent override in editor.
        /// </summary>
        [SettingsUISection(General, Stable)]
        [SettingsUISetter(typeof(AnarchyModSettings), nameof(CheckForDisableElevationLock))]
        public bool PreventOverrideInEditor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the keybinding for Toggling Anarchy.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        [SettingsUIKeyboardBinding(UnityEngine.InputSystem.Key.A, actionName: "ToggleAnarchy", ctrl: true)]
        public ProxyBinding ToggleAnarchy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the keybinding for Reset Elevation.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        [SettingsUIKeyboardBinding(UnityEngine.InputSystem.Key.R, actionName: "ResetElevation", shift: true)]
        public ProxyBinding ResetElevation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the keybinding for Elevation Step.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        [SettingsUIKeyboardBinding(UnityEngine.InputSystem.Key.E, actionName: "ElevationStep", shift: true)]
        public ProxyBinding ElevationStep { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the keybinding for Increase Elevation.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        [SettingsUIKeyboardBinding(UnityEngine.InputSystem.Key.UpArrow, AxisComponent.Positive, actionName: "Elevation")]
        public ProxyBinding IncreaseElevation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the keybinding for Decrease Elevation.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        [SettingsUIKeyboardBinding(UnityEngine.InputSystem.Key.DownArrow, AxisComponent.Negative, actionName: "Elevation")]
        public ProxyBinding DecreaseElevation { get; set; }

        /// <summary>
        /// Sets a value indicating whether: a button for Resetting the settings for keybinds.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(Keybinds, Reset)]
        public bool ResetKeybindSettings
        {
            set
            {
                ResetKeyBindings();
            }
        }

        /// <summary>
        /// Sets a value indicating whether: a button for Resetting the general mod settings.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(General, Reset)]
        public bool ResetGeneralModSettings
        {
            set
            {
                PreventAccidentalPropCulling = true;
                PropRefreshFrequency = 30;
                AllowPlacingMultipleUniqueBuildings = false;
                MinimumClearanceBelowElevatedNetworks = 0f;
                PreventOverrideInEditor = false;
                ApplyAndSave();
            }
        }

        /// <summary>
        /// Sets a value indicating whether: a button for Resetting for the ui Mod settings.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(UI, Reset)]
        public bool ResetUIModSettings
        {
            set
            {
                AnarchicBulldozer = true;
                ShowTooltip = false;
                FlamingChirper = true;
                SetFlamingChirper(true);
                ToolIcon = true;
                DisableAnarchyWhileBrushing = false;
                ShowElevationToolOption = true;
                SetShowElevationToolOption(true);
                ResetElevationWhenChangingPrefab = true;
                ApplyAndSave();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the player has enabled elevation lock.
        /// </summary>
        [SettingsUIHidden]
        public bool ElevationLock { get; set; } = false;

        /// <summary>
        /// Checks if prevent accidental prop culling is off or on.
        /// </summary>
        /// <returns>Opposite of PreventAccidentalPropCulling.</returns>
        public bool IsCullingNotBeingPrevented() => !PreventAccidentalPropCulling;

        /// <summary>
        /// Checks if prevent accidental prop culling is off or on.
        /// </summary>
        /// <returns>Opposite of PreventAccidentalPropCulling.</returns>
        public bool IsElevationToolOptionNotShown() => !ShowElevationToolOption;

        /// <inheritdoc/>
        public override void SetDefaults()
        {
            AnarchicBulldozer = true;
            ShowTooltip = false;
            FlamingChirper = true;
            ToolIcon = true;
            PreventAccidentalPropCulling = true;
            PropRefreshFrequency = 30;
            AllowPlacingMultipleUniqueBuildings = false;
            MinimumClearanceBelowElevatedNetworks = 0f;
            PreventOverrideInEditor = false;
            DisableAnarchyWhileBrushing = false;
            ShowElevationToolOption = true;
            ResetElevationWhenChangingPrefab = true;
        }


        /// <summary>
        /// Triggers method in Anarchy UI System when ShowElevationToolOption is toggled.
        /// </summary>
        /// <param name="value">The value being set to.</param>
        public void SetShowElevationToolOption(bool value)
        {
            AnarchyUISystem anarchyUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<AnarchyUISystem>();
            anarchyUISystem.SetShowElevationSettingsOption(value);
        }

        /// <summary>
        /// Triggers method in Anarchy UI System for setting flaming chirper.
        /// </summary>
        /// <param name="value">The value being set to.</param>
        public void SetFlamingChirper(bool value)
        {
            AnarchyUISystem anarchyReactUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<AnarchyUISystem>();
            anarchyReactUISystem.SetFlamingChirperOption(value);
        }

        /// <summary>
        /// Triggers method in Anarchy UI System for setting prevent override.
        /// </summary>
        /// <param name="value">The value being set to.</param>
        public void CheckForDisableElevationLock(bool value)
        {
            AnarchyUISystem anarchyReactUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<AnarchyUISystem>();
            anarchyReactUISystem.SetDisableElevationLock(value);
        }
    }
}
