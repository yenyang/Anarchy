// <copyright file="LocaleEN.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using Game.Tools;

    /// <summary>
    /// Localization for Anarchy Mod in English.
    /// </summary>
    public class LocaleEN : IDictionarySource
    {
        private readonly AnarchyModSettings m_Setting;

        private Dictionary<string, string> m_Localization;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocaleEN"/> class.
        /// </summary>
        /// <param name="setting">Settings class.</param>
        public LocaleEN(AnarchyModSettings setting)
        {
            m_Setting = setting;

            m_Localization = new Dictionary<string, string>()
            {
                { m_Setting.GetSettingsLocaleID(), "Anarchy" },
                { m_Setting.GetOptionTabLocaleID(nameof(AnarchyModSettings.General)), "General" },
                { m_Setting.GetOptionTabLocaleID(nameof(AnarchyModSettings.UI)), "UI" },
                { m_Setting.GetOptionTabLocaleID(nameof(AnarchyModSettings.Keybinds)), "Keybinds" },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.AnarchicBulldozer)), "Always enable Anarchy with Bulldoze Tool" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.AnarchicBulldozer)), "With this option enabled the Bulldoze Tool will always have anarchy enabled." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.FlamingChirper)), "Flaming Chirper" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.FlamingChirper)), "With this option enabled the Chirper will be on fire when Anarchy is active for appropriate tools. Image Credit: Bad Peanut." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ShowTooltip)), "Show Tooltip" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ShowTooltip)), "With this option enabled a tooltip with Ⓐ will be shown when Anarchy is active for appropriate tools." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ToolIcon)), "Tool Icon" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ToolIcon)), "With this option enabled a icon row with a button for Anarchy, Anarchy options menu, and Anarchy components tool, will show up when using appropriate tools." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ResetGeneralModSettings)), "Reset Anarchy General Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ResetGeneralModSettings)), "Upon confirmation this will reset the general settings for Anarchy mod." },
                { m_Setting.GetOptionWarningLocaleID(nameof(AnarchyModSettings.ResetGeneralModSettings)), "Reset Anarchy General Settings?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ResetUIModSettings)), "Reset Anarchy UI Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ResetUIModSettings)), "Upon confirmation this will reset the UI settings for Anarchy mod." },
                { m_Setting.GetOptionWarningLocaleID(nameof(AnarchyModSettings.ResetUIModSettings)), "Reset Anarchy UI Settings?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ResetKeybindSettings)), "Reset Anarchy Keybindings" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ResetKeybindSettings)), "Upon confirmation this will reset the keybindings for Anarchy mod." },
                { m_Setting.GetOptionWarningLocaleID(nameof(AnarchyModSettings.ResetKeybindSettings)), "Reset Anarchy Keybindings?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.PreventAccidentalPropCulling)), "Prevent Accidental Prop Culling" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.PreventAccidentalPropCulling)), "This will routinely trigger a graphical refresh to props placed with Anarchy that have been culled to prevent accidental culling of props. This affects performance." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.PropRefreshFrequency)), "Prop Refresh Frequency" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.PropRefreshFrequency)), "This is number of frames between graphical refreshes to props placed with Anarchy to prevent accidental culling. Higher numbers will have better performance, but longer possible time that props may be missing." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.RefreshProps)), "Refresh Props" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.RefreshProps)), "If props placed with Anarchy have been accidently culled, you can press this button to bring them back now. This doesn't negatively effect performance." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.AllowPlacingMultipleUniqueBuildings)), "Allow Placing Multiple Unique Buildings" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.AllowPlacingMultipleUniqueBuildings)), "This allows you to place multiple copies of unique buildings using the normal UI menu with or without Anarchy enabled. The effects of these buildings stack!" },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.MinimumClearanceBelowElevatedNetworks)), "Minimum Clearance Below Elevated Networks" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.MinimumClearanceBelowElevatedNetworks)), "With the net tool and Anarchy enabled you can violate the clearance of other networks. Zoning under low bridges can spawn buildings while doing this. This setting gives you some control over the minimum space below a low bridge. It would be better to just remove the zoning." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.PreventOverrideInEditor)), "Anarchy Components In Editor" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.PreventOverrideInEditor)), "In the editor, with Anarchy and this option enabled, you can place vegetation and props overlapping or inside the boundaries of other objects and close together. The map may require Anarchy as a dependency. When users draw roads through these objects they will not be overriden. Also allows you to Lock Elevation in the editor." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.DisableAnarchyWhileBrushing)), "Disable Anarchy Toggle While Brushing Objects" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.DisableAnarchyWhileBrushing)), "Automatically disables the anarchy toggle while brushing objects such as trees. Toggle reverts back to previous state after you stop brushing objects." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ShowElevationToolOption)), "Show Elevation Option for Objects" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ShowElevationToolOption)), "Allows trees, plants, and props to be placed at different vertical elevations with Object Tool or Line Tool. Also shows a button during placement for locking elevation. Keybinds are configurable in the Keybinds tab." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ResetElevationWhenChangingPrefab)), "Reset Elevation When Selecting New Asset" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ResetElevationWhenChangingPrefab)), "Automatically resets object Elevation tool option when you change to a new asset selection." },
                { "YY_ANARCHY.Anarchy", "Anarchy" },
                { "YY_ANARCHY.AnarchyButton", "Anarchy" },
                { "YY_ANARCHY_DESCRIPTION.AnarchyButton", "Disables error checks for tools and does not display errors. When applicable, you can place vegetation and props (with DevUI 'Add Object' menu) overlapping or inside the boundaries of other objects and close together." },
                { TooltipDescriptionKey("PreventOverrideButton"), "Allows placement of vegetation and props overlapping or inside the boundaries of other objects and close together." },
                { TooltipTitleKey("PreventOverrideButton"), "Prevent Override" },
                { TooltipTitleKey("AnarchyModComponets"), "Anarchy Mod Components" },
                { TooltipDescriptionKey("IncreaseElevation"), "Increases the elevation relative to the placement surface. Customize keybind in options menu." },
                { TooltipDescriptionKey("DecreaseElevation"), "Decreases the elevation relative to the placement surface. Customize keybind in options menu." },
                { TooltipDescriptionKey("ElevationStep"),  "Changes the rate in which the elevation changes. Customize keybind in options menu." },
                { TooltipTitleKey("ElevationLock"),         "Elevation Lock" },
                { TooltipDescriptionKey("ElevationLock"),  "Prevents game systems from changing elevation. You can still change position with mods." },
                { TooltipDescriptionKey("ResetElevation"),  "Resets Elevation to 0. Customize keybind in options menu." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ToggleAnarchy)), "Toggle Anarchy" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ToggleAnarchy)), "A keybind to switch the Anarchy toggle on or off." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ResetElevation)), "Reset Elevation" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ResetElevation)), "A keybind to reset the elevation value of objects during placement." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ElevationStep)), "Change Elevation Step" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ElevationStep)), "A keybind to change the rate in which the elevation value changes." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.IncreaseElevation)), "Increase Elevation" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.IncreaseElevation)), "A keybind to increase the elevation value of objects during placement." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.DecreaseElevation)), "Decrease Elevation" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.DecreaseElevation)), "A keybind to decrease the elevation value of objects during placement." },
                { m_Setting.GetBindingMapLocaleID(), "Anarchy" },
                { m_Setting.GetBindingKeyLocaleID(AnarchyModSettings.ToggleAnarchyActionName), "Press key" },
                { m_Setting.GetBindingKeyLocaleID(AnarchyModSettings.ResetElevationActionName), "Press key" },
                { m_Setting.GetBindingKeyLocaleID(AnarchyModSettings.ElevationStepActionName), "Press key" },
                { m_Setting.GetBindingKeyLocaleID(AnarchyModSettings.ElevationActionName, Game.Input.AxisComponent.Positive), "Increase key" },
                { m_Setting.GetBindingKeyLocaleID(AnarchyModSettings.ElevationActionName, Game.Input.AxisComponent.Negative), "Decrease key" },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyMod.Version)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyMod.Version)), $"Version number for the Anarchy mod installed." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.NetworkAnarchyToolOptions)), "Network Anarchy Tool Options" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.NetworkAnarchyToolOptions)), "With this option enabled and while drawing networks, options for ground, elevated, tunnel, and constant slope will appear when applicable in the tool options panel." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.NetworkUpgradesToolOptions)), "Network Upgrades Tool Options" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.NetworkUpgradesToolOptions)), "With this option enabled and while drawing networks, options for various network upgrades will appear when applicable in the tool options panel. (i.e. street trees, retaining walls, quays, etc.)" },
                { ErrorCheckKey(ErrorType.AlreadyExists), "Already Exists" },
                { ErrorCheckKey(ErrorType.AlreadyUpgraded), "Already Upgraded" },
                { ErrorCheckKey(ErrorType.ExceedsCityLimits), "Exceeds City Limits" },
                { ErrorCheckKey(ErrorType.ExceedsLotLimits), "Exceeds Lot Limits" },
                { ErrorCheckKey(ErrorType.InvalidShape), "Invalid Shape" },
                { ErrorCheckKey(ErrorType.InWater), "In Water" },
                { ErrorCheckKey(ErrorType.LongDistance), "Long Distance" },
                { ErrorCheckKey(ErrorType.LowElevation), "Low Elevation" },
                { ErrorCheckKey(ErrorType.NoCargoAccess), "No Cargo Access" },
                { ErrorCheckKey(ErrorType.NoGroundWater), "No Ground Water" },
                { ErrorCheckKey(ErrorType.NotOnBorder), "Not On Border" },
                { ErrorCheckKey(ErrorType.NoWater), "No Water" },
                { ErrorCheckKey(ErrorType.NotOnShoreline), "Not On Shoreline" },
                { ErrorCheckKey(ErrorType.OnFire), "On Fire" },
                { ErrorCheckKey(ErrorType.OverlapExisting), "Overlap Existing" },
                { ErrorCheckKey(ErrorType.PathfindFailed), "Pathfind Failed" },
                { ErrorCheckKey(ErrorType.SmallArea), "Small Area" },
                { ErrorCheckKey(ErrorType.ShortDistance), "Short Distance" },
                { ErrorCheckKey(ErrorType.SteepSlope), "Steep Slope" },
                { ErrorCheckKey(ErrorType.TightCurve), "Tight Curve" },
                { ErrorCheckKey(ErrorType.NoCarAccess), "No Car Access" },
                { ErrorCheckKey(ErrorType.NoPedestrianAccess), "No Pedestrian Access" },
                { ErrorCheckKey(ErrorType.NoRoadAccess), "No Road Access" },
                { ErrorCheckKey(ErrorType.NoTrackAccess), "No Track Access" },
                { ErrorCheckKey(ErrorType.NoTrainAccess), "No Train Access" },
                { SectionLabel("ErrorCheck"), "Error Check" },
                { SectionLabel("Disabled"), "Disabled?" },
                { SectionLabel("AnarchyOptions"), "Anarchy Options" },
                { TooltipDescriptionKey("AnarchyOptions"), "Opens a panel for controlling which error checks are never disabled, disabled with Anarchy, or always disabled." },
                { UIText("Never"), "Never" },
                { UIText("Always"), "Always" },
                { SectionLabel("Left"), "Left" },
                { SectionLabel("Right"), "Right" },
                { SectionLabel("General"), "General" },
                { SectionLabel("Radius"), "Radius" },
                { SectionLabel("Selection"), "Selection" },
                { SectionLabel("Components"), "Components" },
                { TooltipTitleKey("ConstantSlope"), "Constant Slope" },
                { TooltipDescriptionKey("ConstantSlope"), "Forces newly placed networks to have a constant slope or grade from starting point to end point (A -> B)." },
                { TooltipTitleKey("Ground"), "Ground" },
                { TooltipDescriptionKey("Ground"), "Forces terrain to follow newly placed network. Zoned roads should produce zones under normal circumstances." },
                { TooltipTitleKey("WideMedian"), "Wide Median" },
                { TooltipDescriptionKey("WideMedian"), "Applies Wide Sidewalk upgrade to the median to produce a wide concrete median." },
                { TooltipTitleKey("ReplaceUpgrade"), "Replace Upgrades" },
                { TooltipDescriptionKey("ReplaceUpgrade"), "When toggled, replace tool mode will add or remove network upgrades to match the selected upgrades. If not toggled, replace tool mode will preserve network upgrades, if possible." },
                { TooltipTitleKey("AnarchyComponentsTool"), "Anarchy Components Tool" },
                { TooltipDescriptionKey("AnarchyComponentsTool"), "Add or remove elevation lock or anarchy components using a tool with radius or single selection. Returns to previous tool when closed with Escape. Radius is recommended for Anarchy component since you can see overridden objects and interact with them. Single selection cannot select overriden objects and they are not visible." },
                { TooltipDescriptionKey("IncreaseRadius"), "Increase the radius." },
                { TooltipDescriptionKey("DecreaseRadius"), "Decrease the radius." },
                { TooltipTitleKey("SingleSelection"), "Single Selection" },
                { TooltipDescriptionKey("SingleSelection"), "Left Mouse Button to add selected component(s). Right Mouse Button to remove selected component(s). Single selection cannot select overriden objects and they are not visible." },
                { TooltipTitleKey("RadiusSelection"), "Radius Selection" },
                { TooltipDescriptionKey("RadiusSelection"), "Left Mouse Button to add selected component(s) within radius. Right Mouse Button to remove selected component(s) within radius. Radius is recommended for Anarchy component since you can see overridden objects and interact with them." },
            };
        }

        /// <summary>
        /// Gets a locale key for an error check.
        /// </summary>
        /// <param name="errorType">Error type enum.</param>
        /// <returns>Localization key.</returns>
        public static string ErrorCheckKey(ErrorType errorType)
        {
            return $"{AnarchyMod.Id}.ErrorType[{errorType}]";
        }

        private string TooltipDescriptionKey(string key)
        {
            return $"{AnarchyMod.Id}.TOOLTIP_DESCRIPTION[{key}]";
        }

        private string TooltipTitleKey(string key)
        {
            return $"{AnarchyMod.Id}.TOOLTIP_TITLE[{key}]";
        }

        private string SectionLabel(string key)
        {
            return $"{AnarchyMod.Id}.SECTION_TITLE[{key}]";
        }

        private string UIText(string key)
        {
            return $"{AnarchyMod.Id}.UI_TEXT[{key}]";
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return m_Localization;
        }

        /// <inheritdoc/>
        public void Unload()
        {
        }
    }
}