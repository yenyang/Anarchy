// <copyright file="AnarchyMod.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

#define DEBUG
namespace Anarchy
{
    using Anarchy.Settings;
    using Anarchy.Systems;
    using Anarchy.Tooltip;
    using Colossal.IO.AssetDatabase;
    using Colossal.Localization;
    using Colossal.Logging;
    using Game;
    using Game.Modding;
    using Game.SceneFlow;
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Mod entry point.
    /// </summary>
    public class AnarchyMod : IMod
    {
        /// <summary>
        /// Gets the install folder for the mod.
        /// </summary>
        private static string m_modInstallFolder;

        /// <summary>
        ///  Gets or sets the static version of the Anarchy Mod Settings.
        /// </summary>
        public static AnarchyModSettings Settings { get; set; }

        /// <summary>
        /// Gets the static reference to the mod instance.
        /// </summary>
        public static AnarchyMod Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Install Folder for the mod as a string.
        /// </summary>
        public static string ModInstallFolder
        {
            get
            {
                if (m_modInstallFolder is null)
                {
                    m_modInstallFolder = Path.GetDirectoryName(typeof(AnarchyPlugin).Assembly.Location);
                }

                return m_modInstallFolder;
            }
        }

        /// <summary>
        /// Gets ILog for mod.
        /// </summary>
        internal ILog Logger { get; private set; }

        /// <inheritdoc/>
        public void OnLoad()
        {
            Instance = this;
            Logger = LogManager.GetLogger("Mods_Yenyang_Anarchy", false);
            Logger.effectivenessLevel = Level.Info;
            Logger.Info("Loading. . .");
        }

        /// <inheritdoc/>
        public void OnCreateWorld(UpdateSystem updateSystem)
        {
            Logger.effectivenessLevel = Level.Info;
            Logger.Info("Initializing Settings.");
            Settings = new (this);
            Settings.RegisterInOptionsUI();
            AssetDatabase.global.LoadSettings(nameof(AnarchyMod), Settings, new AnarchyModSettings(this));
            Settings.Contra = false;
            Logger.Info("Handling create world");
            Logger.Info("ModInstallFolder = " + ModInstallFolder);
            LoadLocales();
            updateSystem.UpdateAfter<AnarchyTooltipSystem>(SystemUpdatePhase.UITooltip);
            updateSystem.UpdateAt<AnarchySystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateBefore<DisableToolErrorsSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAfter<EnableToolErrorsSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<AnarchyUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateBefore<AnarchyPlopSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateBefore<PreventOverrideSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateBefore<RemoveOverridenSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<PreventCullingSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateBefore<ModifyNetCompositionDataSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAfter<ResetNetCompositionDataSystem>(SystemUpdatePhase.ModificationEnd);
        }

        /// <inheritdoc/>
        public void OnDispose()
        {
            Logger.Info("Disposing..");
        }

        private void LoadLocales()
        {
            LocaleEN defaultLocale = new LocaleEN(Settings);

            // defaultLocale.ExportLocalizationCSV(ModInstallFolder, GameManager.instance.localizationManager.GetSupportedLocales());
            var file = Path.Combine(ModInstallFolder, $"l10n.csv");
            if (File.Exists(file))
            {
                var fileLines = File.ReadAllLines(file).Select(x => x.Split('\t'));
                var enColumn = Array.IndexOf(fileLines.First(), "en-US");
                var enMemoryFile = new MemorySource(fileLines.Skip(1).ToDictionary(x => x[0], x => x.ElementAtOrDefault(enColumn)));
                foreach (var lang in GameManager.instance.localizationManager.GetSupportedLocales())
                {
                    try
                    {
                        GameManager.instance.localizationManager.AddSource(lang, enMemoryFile);
                        if (lang != "en-US")
                        {
                            var valueColumn = Array.IndexOf(fileLines.First(), lang);
                            if (valueColumn > 0)
                            {
                                var i18nFile = new MemorySource(fileLines.Skip(1).ToDictionary(x => x[0], x => x.ElementAtOrDefault(valueColumn)));
                                GameManager.instance.localizationManager.AddSource(lang, i18nFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"{nameof(AnarchyMod)}.{nameof(LoadLocales)} Encountered exception {ex} while trying to localize {lang}.");
                    }
                }
            }
            else
            {
                Logger.Warn($"{nameof(AnarchyMod)}.{nameof(LoadLocales)} couldn't find localization file and loaded default for every language.");
                foreach (var lang in GameManager.instance.localizationManager.GetSupportedLocales())
                {
                    GameManager.instance.localizationManager.AddSource(lang, defaultLocale);
                }
            }
        }

    }
}
