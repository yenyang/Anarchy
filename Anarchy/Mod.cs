// <copyright file="Mod.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy
{
    using System;
    using System.IO;
    using System.Linq;
    using Anarchy.Settings;
    using Anarchy.Systems;
    using Anarchy.Tooltip;
    using Colossal.IO.AssetDatabase;
    using Colossal.Localization;
    using Colossal.Logging;
    using Game;
    using Game.Modding;
    using Game.SceneFlow;
    using HarmonyLib;

    /// <summary>
    /// Mod entry point.
    /// </summary>
    public class Mod : IMod
    {
        /// <summary>
        /// Gets the install folder for the mod.
        /// </summary>
        private static string m_modInstallFolder;

        private Harmony m_Harmony;

        /// <summary>
        /// Gets the static reference to the mod instance.
        /// </summary>
        public static Mod Instance
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
                    if (GameManager.instance.modManager.TryGetExecutableAsset(Instance, out var asset))
                    {
                        m_modInstallFolder = asset.path;
                        Instance.Log.Info($"{nameof(Mod)}.{nameof(ModInstallFolder)} Current mod asset at {asset.path}");
                    }
                    else
                    {
                        Instance.Log.Warn($"{nameof(Mod)}.{nameof(ModInstallFolder)} Could not find Executable asset path!");
                    }
                }

                return m_modInstallFolder;
            }
        }

        /// <summary>
        /// Gets or sets the mods settings.
        /// </summary>
        internal AnarchyModSettings Settings { get; set; }

        /// <summary>
        /// Gets ILog for mod.
        /// </summary>
        internal ILog Log { get; private set; }

        /// <inheritdoc/>
        public void OnLoad(UpdateSystem updateSystem)
        {
            Instance = this;
            Log = LogManager.GetLogger("Mods_Yenyang_Anarchy").SetShowsErrorsInUI(false);
            Log.Info(nameof(OnLoad));
#if DEBUG
            Log.effectivenessLevel = Level.Debug;
#elif VERBOSE
            Log.effectivenessLevel = Level.Verbose;
#else
            Log.effectivenessLevel = Level.Info;
#endif
            Log.Info($"{nameof(Mod)}.{nameof(OnLoad)} Handling settings");
            Settings = new (this);
            Settings.RegisterInOptionsUI();
            AssetDatabase.global.LoadSettings("AnarchyMod", Settings, new AnarchyModSettings(this));
            Settings.Contra = false;
            Log.Info($"{nameof(Mod)}.{nameof(OnLoad)} ModInstallFolder = " + ModInstallFolder);
            Log.Info($"{nameof(Mod)}.{nameof(OnLoad)} Loading localization");
            LoadLocales();
            Log.Info($"{nameof(Mod)}.{nameof(OnLoad)} Injecting Harmony Patches.");
            m_Harmony = new Harmony("Mods_Yenyang_Anarchy");
            m_Harmony.PatchAll();
            Log.Info($"{nameof(Mod)}.{nameof(OnLoad)} Injecting systems.");
            updateSystem.UpdateAfter<AnarchyTooltipSystem>(SystemUpdatePhase.UITooltip);
            updateSystem.UpdateAt<AnarchySystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateBefore<DisableToolErrorsSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAfter<EnableToolErrorsSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<AnarchyReactUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateBefore<AnarchyPlopSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateBefore<PreventOverrideSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateBefore<RemoveOverridenSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<PreventCullingSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateBefore<ModifyNetCompositionDataSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAfter<ResetNetCompositionDataSystem>(SystemUpdatePhase.ModificationEnd);
            Log.Info($"{nameof(Mod)}.{nameof(OnLoad)} Completed.");
        }

        /// <inheritdoc/>
        public void OnDispose()
        {
            Log.Info(nameof(OnDispose));
            m_Harmony.UnpatchAll();
            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }

        private void LoadLocales()
        {
            LocaleEN defaultLocale = new LocaleEN(Settings);

            // defaultLocale.ExportLocalizationCSV(ModInstallFolder, GameManager.instance.localizationManager.GetSupportedLocales());
            var file = Path.Combine(ModInstallFolder, "l10n", $"l10n.csv");
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
                        Log.Warn($"{nameof(Mod)}.{nameof(LoadLocales)} Encountered exception {ex} while trying to localize {lang}.");
                    }
                }
            }
            else
            {
                Log.Warn($"{nameof(Mod)}.{nameof(LoadLocales)} couldn't find localization file and loaded default for every language.");
                foreach (var lang in GameManager.instance.localizationManager.GetSupportedLocales())
                {
                    GameManager.instance.localizationManager.AddSource(lang, defaultLocale);
                }
            }
        }
    }
}
