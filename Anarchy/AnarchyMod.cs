﻿// <copyright file="AnarchyMod.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
// #define DUMP_VANILLA_LOCALIZATION
namespace Anarchy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Anarchy.Settings;
    using Anarchy.Systems;
    using Colossal;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Game;
    using Game.Modding;
    using Game.SceneFlow;
    using HarmonyLib;
    using Newtonsoft.Json;
    using UnityEngine;

    /// <summary>
    /// Mod entry point.
    /// </summary>
    public class AnarchyMod : IMod
    {
        /// <summary>
        /// An id used for bindings between UI and C#.
        /// </summary>
        public static readonly string Id = "Anarchy";

        /// <summary>
        /// Gets the install folder for the mod.
        /// </summary>
        private static string m_modInstallFolder;

        private Harmony m_Harmony;

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
                    var thisFullName = Instance.GetType().Assembly.FullName;
                    ExecutableAsset thisInfo = AssetDatabase.global.GetAsset(SearchFilter<ExecutableAsset>.ByCondition(x => x.definition?.FullName == thisFullName)) ?? throw new Exception("This mod info was not found!!!!");
                    m_modInstallFolder = Path.GetDirectoryName(thisInfo.GetMeta().path);
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

        /// <summary>
        /// Gets the version of the mod.
        /// </summary>
        internal string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);


        /// <inheritdoc/>
        public void OnLoad(UpdateSystem updateSystem)
        {
            Instance = this;
            Log = LogManager.GetLogger("Mods_Yenyang_Anarchy").SetShowsErrorsInUI(false);
            Log.Info(nameof(OnLoad));
#if VERBOSE
            Log.effectivenessLevel = Level.Verbose;
#elif DEBUG
            Log.effectivenessLevel = Level.Debug;
#else
            Log.effectivenessLevel = Level.Info;
#endif
            Log.Info($"{nameof(AnarchyMod)}.{nameof(OnLoad)} ModInstallFolder = " + ModInstallFolder);
            Log.Info($"{nameof(AnarchyMod)}.{nameof(OnLoad)} Initializing settings");
            Settings = new (this);
            Log.Info($"{nameof(AnarchyMod)}.{nameof(OnLoad)} Loading localization");
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Settings));

#if DEBUG
            Log.Info($"{nameof(AnarchyMod)}.{nameof(OnLoad)} Exporting localization");
            var localeDict = new LocaleEN(Settings).ReadEntries(new List<IDictionaryEntryError>(), new Dictionary<string, int>()).ToDictionary(pair => pair.Key, pair => pair.Value);
            var str = JsonConvert.SerializeObject(localeDict, Formatting.Indented);
            try
            {
                File.WriteAllText("C:\\Users\\TJ\\source\\repos\\Anarchy\\Anarchy\\UI\\src\\lang\\en-US.json", str);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
#endif
#if DUMP_VANILLA_LOCALIZATION
            var strings = GameManager.instance.localizationManager.activeDictionary.entries
                .OrderBy(kv => kv.Key)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var json = Colossal.Json.JSON.Dump(strings);

            var filePath = Path.Combine(Application.persistentDataPath, "locale-dictionary.json");

            File.WriteAllText(filePath, json);
#endif
            Log.Info($"{nameof(AnarchyMod)}.{nameof(OnLoad)} Registering settings");
            Settings.RegisterInOptionsUI();
            Log.Info($"{nameof(AnarchyMod)}.{nameof(OnLoad)} Loading settings");
            AssetDatabase.global.LoadSettings("AnarchyMod", Settings, new AnarchyModSettings(this));
            Settings.RegisterKeyBindings();
            Log.Info($"{nameof(AnarchyMod)}.{nameof(OnLoad)} Injecting Harmony Patches.");
            m_Harmony = new Harmony("Mods_Yenyang_Anarchy");
            m_Harmony.PatchAll();
            Log.Info($"{nameof(AnarchyMod)}.{nameof(OnLoad)} Injecting systems.");
            updateSystem.UpdateAfter<AnarchyTooltipSystem>(SystemUpdatePhase.UITooltip);
            updateSystem.UpdateBefore<DisableToolErrorsSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAfter<EnableToolErrorsSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<AnarchyUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateBefore<AnarchyPlopSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateBefore<PreventOverrideSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateBefore<RemoveOverridenSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<PreventCullingSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateBefore<ModifyNetCompositionDataSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAfter<ResetNetCompositionDataSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<ResetTransformSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<CheckTransformSystem>(SystemUpdatePhase.Modification1);
            updateSystem.UpdateBefore<HandleUpdateNextFrameSystem>(SystemUpdatePhase.Modification1);
            updateSystem.UpdateAt<SelectedInfoPanelTogglesSystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateBefore<ElevateObjectDefinitionSystem>(SystemUpdatePhase.Modification1);
            updateSystem.UpdateAt<ElevateTempObjectSystem>(SystemUpdatePhase.Modification1);
            updateSystem.UpdateBefore<NetworkGradeDefinitionSystem>(SystemUpdatePhase.Modification1);
            updateSystem.UpdateBefore<TempNetworkGradeSystem>(SystemUpdatePhase.Modification3);
            Log.Info($"{nameof(AnarchyMod)}.{nameof(OnLoad)} Completed.");
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
    }
}
