// <copyright file="AnarchyModSettings.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Settings
{
    using Anarchy.Systems;
    using Colossal.IO.AssetDatabase;
    using Game.Modding;
    using Game.Settings;
    using Game.UI;
    using Unity.Entities;

    /// <summary>
    /// The mod settings for the Anarchy Mod.
    /// </summary>
    [FileLocation("Mods_Yenyang_Anarchy")]
    public class AnarchyModSettings : ModSetting
    {
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
        public bool AnarchicBulldozer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the tooltip.
        /// </summary>
        public bool ShowTooltip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to have chirper be on fire.
        /// </summary>
        public bool FlamingChirper { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the tool icon.
        public bool ToolIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to prevent prop culling.
        /// </summary>
        public bool PreventAccidentalPropCulling { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the frequency to update props.
        /// </summary>
        [SettingsUISlider(min = 1, max = 600, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        [SettingsUIHideByCondition(typeof(AnarchyModSettings), nameof(IsCullingNotBeingPrevented))]
        public int PropRefreshFrequency { get; set; }

        /// <summary>
        /// Sets a value indicating whether: to update props now.
        /// </summary>
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
        public bool AllowPlacingMultipleUniqueBuildings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the minimum clearance below elevated network.
        /// </summary>
        [SettingsUISlider(min = 0f, max = 1.75f, step = 0.25f, scalarMultiplier = 1, unit = Unit.kFloatTwoFractions)]
        public float MinimumClearanceBelowElevatedNetworks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether: Used to force saving of Modsettings if settings would result in empty Json.
        /// </summary>
        [SettingsUIHidden]
        public bool Contra { get; set; }

        /// <summary>
        /// Sets a value indicating whether: a button for Resetting the settings for the Mod.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        public bool ResetModSettings
        {
            set
            {
                SetDefaults();
                Contra = false;
                ApplyAndSave();
            }
        }

        /// <summary>
        /// Checks if prevent accidental prop culling is off or on.
        /// </summary>
        /// <returns>Opposite of PreventAccidentalPropCulling.</returns>
        public bool IsCullingNotBeingPrevented() => !PreventAccidentalPropCulling;

        /// <inheritdoc/>
        public override void SetDefaults()
        {
            AnarchicBulldozer = true;
            ShowTooltip = false;
            FlamingChirper = true;
            Contra = true;
            ToolIcon = true;
            PreventAccidentalPropCulling = true;
            PropRefreshFrequency = 30;
            AllowPlacingMultipleUniqueBuildings = false;
            MinimumClearanceBelowElevatedNetworks = 0f;
        }
    }
}
