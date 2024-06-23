// <copyright file="AnarchyMod.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Anarchy
{
    using System;
    using System.IO;
    using Anarchy.Settings;
    using Anarchy.Systems;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Game;
    using Game.Modding;
    using Game.SceneFlow;
    using HarmonyLib;

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
