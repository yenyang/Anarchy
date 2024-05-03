// <copyright file="LocaleEN.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Settings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Colossal;
    using Colossal.IO.AssetDatabase.Internal;
    using Colossal.PSI.Common;

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
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.AnarchicBulldozer)), "Always enable Anarchy with Bulldoze Tool" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.AnarchicBulldozer)), "With this option enabled the Bulldoze Tool will always have anarchy enabled." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.FlamingChirper)), "Flaming Chirper" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.FlamingChirper)), "With this option enabled the Chirper will be on fire when Anarchy is active for appropriate tools. Image Credit: Bad Peanut." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ShowTooltip)), "Show Tooltip" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ShowTooltip)), "With this option enabled a tooltip with Ⓐ will be shown when Anarchy is active for appropriate tools." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ToolIcon)), "Tool Icon" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ToolIcon)), "With this option enabled a icon row with a single button for Anarchy will show up when using appropriate tools." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ResetGeneralModSettings)), "Reset Anarchy General Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ResetGeneralModSettings)), "Upon confirmation this will reset the general settings for Anarchy mod." },
                { m_Setting.GetOptionWarningLocaleID(nameof(AnarchyModSettings.ResetGeneralModSettings)), "Reset Anarchy General Settings?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ResetUIModSettings)), "Reset Anarchy UI Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ResetUIModSettings)), "Upon confirmation this will reset the UI settings for Anarchy mod." },
                { m_Setting.GetOptionWarningLocaleID(nameof(AnarchyModSettings.ResetUIModSettings)), "Reset Anarchy UI Settings?" },
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
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.PreventOverrideInEditor)), "Prevent Override In Editor" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.PreventOverrideInEditor)), "In the editor, with Anarchy and this option enabled, you can place vegetation and props overlapping or inside the boundaries of other objects and close together. The map may require Anarchy as a dependency. When users draw roads through these objects they will not be overriden." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.DisableAnarchyWhileBrushing)), "Disable Anarchy Toggle While Brushing Objects" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.DisableAnarchyWhileBrushing)), "Automatically disables the anarchy toggle while brushing objects such as trees. Toggle reverts back to previous state after you stop brushing objects." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ShowElevationToolOption)), "Show Elevation Option for Objects" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ShowElevationToolOption)), "Allows trees, plants, and props to be placed at different vertical elevations with Object Tool or Line Tool. Also shows a button during placement for locking elevation. Keybinds are: Up Arrow -> Elevation Up | Down Arrow -> Elevation Down | Shift + R -> Reset to 0 | Shift + E -> change Elevation step" },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ResetElevationWhenChangingPrefab)), "Reset Elevation When Selecting New Asset" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ResetElevationWhenChangingPrefab)), "Automatically resets object Elevation tool option when you change to a new asset selection." },
                { "YY_ANARCHY.Anarchy", "Anarchy" },
                { "YY_ANARCHY.AnarchyButton", "Anarchy" },
                { "YY_ANARCHY_DESCRIPTION.AnarchyButton", "Disables error checks for tools and does not display errors. When applicable, you can place vegetation and props (with DevUI 'Add Object' menu) overlapping or inside the boundaries of other objects and close together." },
                { TooltipDescriptionKey("PreventOverrideButton"), "Allows placement of vegetation and props overlapping or inside the boundaries of other objects and close together." },
                { TooltipTitleKey("PreventOverrideButton"), "Prevent Override" },
                { TooltipTitleKey("AnarchyModComponets"), "Anarchy Mod Components" },
                { TooltipDescriptionKey("IncreaseElevation"), "Increases the elevation relative to the placement surface. Keybind: Up Arrow." },
                { TooltipDescriptionKey("DecreaseElevation"), "Decreases the elevation relative to the placement surface. Keybind: Down Arrow." },
                { TooltipDescriptionKey("ElevationStep"),  "Changes the rate in which the elevation changes. Keybind: Shift + E." },
                { TooltipTitleKey("ElevationLock"),         "Elevation Lock" },
                { TooltipDescriptionKey("ElevationLock"),  "Prevents game systems from changing elevation. You can still change position with mods." },
                { TooltipDescriptionKey("ResetElevation"),  "Resets Elevation to 0. Keybind: Shift + R." },
            };
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