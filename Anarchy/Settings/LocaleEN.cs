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
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.AnarchicBulldozer)), "Always enable Anarchy with Bulldoze Tool" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.AnarchicBulldozer)), "With this option enabled the Bulldoze Tool will always have anarchy enabled." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.FlamingChirper)), "Flaming Chirper" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.FlamingChirper)), "With this option enabled the Chirper will be on fire when Anarchy is active for appropriate tools. Image Credit: Bad Peanut." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ShowTooltip)), "Show Tooltip" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ShowTooltip)), "With this option enabled a tooltip with Ⓐ will be shown when Anarchy is active for appropriate tools." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ToolIcon)), "Tool Icon" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ToolIcon)), "With this option enabled a icon row with a single button for Anarchy will show up when using appropriate tools." },
                { m_Setting.GetOptionLabelLocaleID(nameof(AnarchyModSettings.ResetModSettings)), "Reset Anarchy Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(AnarchyModSettings.ResetModSettings)), "Upon confirmation this will reset the settings for Anarchy mod." },
                { m_Setting.GetOptionWarningLocaleID(nameof(AnarchyModSettings.ResetModSettings)), "Reset Anarchy Settings?" },
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
                { "YY_ANARCHY.Anarchy", "Anarchy" },
                { "YY_ANARCHY.AnarchyButton", "Anarchy" },
                { "YY_ANARCHY_DESCRIPTION.AnarchyButton", "Disables error checks for tools and does not display errors. When applicable, you can place vegetation and props (with DevUI 'Add Object' menu) overlapping or inside the boundaries of other objects and close together." },
            };
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

        /// <summary>
        /// Exports a localization CSV template with this files dictionary as default entries.
        /// </summary>
        /// <param name="folderPath">the path of where the file should be created.</param>
        /// <param name="langCodes">the language codes to be included in the template file.</param>
        /// <returns>True if the file is created. False if not.</returns>
        public bool ExportLocalizationCSV(string folderPath, string[] langCodes)
        {
            System.IO.Directory.CreateDirectory(folderPath);
            string localizationFilePath = Path.Combine(folderPath, $"l10n.csv");
            if (!File.Exists(localizationFilePath))
            {
                try
                {
                    using (StreamWriter streamWriter = new(File.Create(localizationFilePath)))
                    {
                        StringBuilder topLine = new StringBuilder();
                        topLine.Append("key\t");
                        foreach (string langCode in langCodes)
                        {
                            topLine.Append(langCode);
                            topLine.Append("\t");
                        }

                        streamWriter.WriteLine(topLine.ToString());

                        foreach (KeyValuePair<string, string> kvp in m_Localization)
                        {
                            StringBuilder currentLine = new StringBuilder();
                            currentLine.Append(kvp.Key);
                            currentLine.Append("\t");
                            foreach (string langCode in langCodes)
                            {
                                currentLine.Append(kvp.Value);
                                currentLine.Append("\t");
                            }

                            streamWriter.WriteLine(currentLine.ToString());
                        }

                        return true;
                    }
                }
                catch (Exception e)
                {
                    AnarchyMod.Instance.Logger.Warn($"{typeof(LocaleEN)}.{nameof(ExportLocalizationCSV)} Encountered Exception {e} while trying to export localization csv.");
                    return false;
                }
            }

            return false;
        }
    }
}