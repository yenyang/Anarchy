// <copyright file="AnarchyModSettings.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Settings
{
    using Anarchy.Systems.Common;
    using Anarchy.Systems.OverridePrevention;
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
    [SettingsUIGroupOrder(Toggle, Elevation, Networks, Stable, Reset, About)]
    [SettingsUIMouseAction(AnarchyMod.SecondaryMimicAction, "AnarchySecondaryApplyMimic")]
    [SettingsUIKeyboardAction(ToggleAnarchyActionName, ActionType.Button, usages: new string[] { Usages.kToolUsage })]
    [SettingsUIKeyboardAction(ElevationStepActionName, ActionType.Button, usages: new string[] { Usages.kToolUsage })]
    [SettingsUIKeyboardAction(ResetElevationActionName, ActionType.Button, usages: new string[] { Usages.kToolUsage })]
    [SettingsUIKeyboardAction(ElevationActionName, ActionType.Button, usages: new string[] { "Anarchy" })]
    [SettingsUIKeyboardAction(ElevationMimicActionName, ActionType.Button, usages: new string[] { "AnarchyMimic" })]
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
        /// This is for settings that are stable.
        /// </summary>
        public const string About = "About";

        /// <summary>
        /// This is for reseting settings button group.
        /// </summary>
        public const string Reset = "Reset";

        /// <summary>
        /// This is for settings that are stable.
        /// </summary>
        public const string Toggle = "Anarchy Toggle";

        /// <summary>
        /// This is for settings that are stable.
        /// </summary>
        public const string Elevation = "Object Elevation";

        /// <summary>
        /// This is for reseting settings button group.
        /// </summary>
        public const string Networks = "Network Anarchy and Upgrades Overhaul";

        /// <summary>
        /// The action name for toggle anarchy keybind.
        /// </summary>
        public const string ToggleAnarchyActionName = "ToggleAnarchy";

        /// <summary>
        /// The action name for reset elevation keybind.
        /// </summary>
        public const string ResetElevationActionName = "ResetElevation";

        /// <summary>
        /// The action name for Elevation Step key bind.
        /// </summary>
        public const string ElevationStepActionName = "ElevationStep";

        /// <summary>
        /// The action name for Elevation Mimic key bind.
        /// </summary>
        public const string ElevationMimicActionName = "ElevationMimic";

        /// <summary>
        /// The action name for Elevation keybind.
        /// </summary>
        public const string ElevationActionName = "Elevation";

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
        [SettingsUISection(UI, Toggle)]
        public bool AnarchicBulldozer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the tooltip.
        /// </summary>
        [SettingsUISection(UI, Toggle)]
        public bool ShowTooltip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to have chirper be on fire. This is currently hidden as it is not implemented and hidding it doesn't break people's existing settings.
        /// </summary>
        [SettingsUISection(UI, Toggle)]
        [SettingsUISetter(typeof(AnarchyModSettings), nameof(SetFlamingChirper))]
        public bool FlamingChirper { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the tool icon.
        [SettingsUISection(UI, Toggle)]
        public bool ToolIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show Elevation tool option.
        [SettingsUISection(UI, Elevation)]
        [SettingsUISetter(typeof(AnarchyModSettings), nameof(SetShowElevationToolOption))]
        public bool ShowElevationToolOption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to reset elevation when changing prefab.
        [SettingsUISection(UI, Elevation)]
        [SettingsUIHideByCondition(typeof(AnarchyModSettings), nameof(IsElevationToolOptionNotShown))]
        public bool ResetElevationWhenChangingPrefab { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable anarchy while brushing.
        /// </summary>
        [SettingsUISection(UI, Toggle)]
        public bool DisableAnarchyWhileBrushing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show network anarchy in tool options menu.
        /// </summary>
        [SettingsUISection(UI, Networks)]
        public bool NetworkAnarchyToolOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show network upgrades in tool options menu.
        /// </summary>
        [SettingsUISection(UI, Networks)]
        public bool NetworkUpgradesToolOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show elevation step slider.
        /// </summary>
        [SettingsUISection(UI, Networks)]
        public bool ElevationStepSlider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show network upgrade prefabs.
        /// </summary>
        [SettingsUISection(UI, Networks)]
        public bool NetworkUpgradesPrefabs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to replace upgrades or not.
        /// </summary>
        [SettingsUISection(UI, Networks)]
        public bool ReplaceUpgradesBehavior { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to reset network tool options when changing prefab.
        /// </summary>
        [SettingsUISection(UI, Networks)]
        public bool ResetNetworkToolOptionsWhenChangingPrefab { get; set; }

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
        /// Gets or sets hidden keybinding for secondary apply action.
        /// </summary>
        [SettingsUIMouseBinding(AnarchyMod.SecondaryMimicAction)]
        [SettingsUISection(General, Keybinds)]
        [SettingsUIBindingMimic(InputManager.kToolMap, "Secondary Apply")]
        [SettingsUIHidden]
        public ProxyBinding SecondaryApplyMimic { get; set; }

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
        [SettingsUISetter(typeof(AnarchyModSettings), nameof(SetMultipleUniques))]
        public bool AllowPlacingMultipleUniqueBuildings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to prevent override in editor.
        /// </summary>
        [SettingsUISection(General, Stable)]
        [SettingsUISetter(typeof(AnarchyModSettings), nameof(CheckForDisableElevationLock))]
        public bool PreventOverrideInEditor { get; set; }


        /// <summary>
        /// Gets a value indicating the version.
        /// </summary>
        [SettingsUISection(General, About)]
        public string Version => AnarchyMod.Instance.Version;

        /// <summary>
        /// Gets or sets a value indicating the keybinding for Toggling Anarchy.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        [SettingsUIKeyboardBinding(BindingKeyboard.A, actionName: ToggleAnarchyActionName, ctrl: true)]
        public ProxyBinding ToggleAnarchy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the keybinding for Reset Elevation.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        [SettingsUIKeyboardBinding(BindingKeyboard.R, actionName: ResetElevationActionName, alt: true)]
        public ProxyBinding ResetElevation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the keybinding for Elevation Step.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        [SettingsUIKeyboardBinding(BindingKeyboard.E, actionName: ElevationStepActionName, alt: true)]
        public ProxyBinding ElevationStep { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the keybinding for Increase Elevation mimic.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        [SettingsUIKeyboardBinding(AxisComponent.Positive, actionName: ElevationMimicActionName)]
        [SettingsUIBindingMimic(InputManager.kShortcutsMap, "Change Elevation")]
        [SettingsUIHidden]
        public ProxyBinding IncreaseElevationMimic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the keybinding for Decrease Elevation mimic.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        [SettingsUIKeyboardBinding(AxisComponent.Negative, actionName: ElevationMimicActionName)]
        [SettingsUIBindingMimic(InputManager.kShortcutsMap, "Change Elevation")]
        [SettingsUIHidden]
        public ProxyBinding DecreaseElevationMimic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the elevation mimics.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        public bool UseElevationMimics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the keybinding for Increase Elevation.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        [SettingsUIKeyboardBinding(BindingKeyboard.PageUp, AxisComponent.Positive, actionName: ElevationActionName)]
        [SettingsUIDisableByCondition(typeof(AnarchyModSettings), nameof(UseElevationMimics))]
        public ProxyBinding IncreaseElevation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the keybinding for Decrease Elevation.
        /// </summary>
        [SettingsUISection(Keybinds, Stable)]
        [SettingsUIKeyboardBinding(BindingKeyboard.PageDown, AxisComponent.Negative, actionName: ElevationActionName)]
        [SettingsUIDisableByCondition(typeof(AnarchyModSettings), nameof(UseElevationMimics))]
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
                UseElevationMimics = true;
                ResetKeyBindings();
                ApplyAndSave();
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
                NetworkAnarchyToolOptions = true;
                NetworkUpgradesToolOptions = true;
                ElevationStepSlider = true;
                NetworkUpgradesPrefabs = true;
                ReplaceUpgradesBehavior = true;
                ResetNetworkToolOptionsWhenChangingPrefab = false;
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
            NetworkAnarchyToolOptions = true;
            NetworkUpgradesToolOptions = true;
            NetworkUpgradesPrefabs = true;
            ElevationStepSlider = true;
            ReplaceUpgradesBehavior = true;
            ResetNetworkToolOptionsWhenChangingPrefab = false;
            UseElevationMimics = true;
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
            anarchyReactUISystem.FlamingChirperOption = value;
        }

        /// <summary>
        /// Triggers method in Anarchy UI System for showing elevation lock.
        /// </summary>
        /// <param name="value">The value being set to.</param>
        public void CheckForDisableElevationLock(bool value)
        {
            AnarchyUISystem anarchyReactUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<AnarchyUISystem>();
            anarchyReactUISystem.SetDisableElevationLock(value);
        }

        /// <summary>
        /// Triggers method in Anarchy UI System for allowing multiple uniques.
        /// </summary>
        /// <param name="value">The value being set to.</param>
        public void SetMultipleUniques(bool value)
        {
            AnarchyUISystem anarchyReactUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<AnarchyUISystem>();
            anarchyReactUISystem.SetMultipleUniques(value);
        }
    }
}
