// <copyright file="ToolbarUISystemApplyPatch.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Patches
{
    using System.Collections.Generic;
    using System.Linq;
    using Anarchy.Systems.Common;
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI.InGame;
    using HarmonyLib;
    using Unity.Entities;

    /// <summary>
    /// Patches ToolbarUISystemApplyPatch so that additionally selected prefabs can also show as selected.
    /// </summary>
    [HarmonyPatch(typeof(ToolbarUISystem), "Apply")]
    public class ToolbarUISystemApplyPatch
    {
        /// <summary>
        /// Patches ToolbarUISystemApplyPatch so that additionally selected prefabs can also show as selected.
        /// </summary>
        /// <param name="themes">list of themes</param>
        /// <param name="packs">list of pcaks</param>=
        /// <param name="assetMenuEntity">Not needed assetMenuEntity.</param>
        /// <param name="assetCategoryEntity">Not needed assetCategoryEntity.</param>
        /// <param name="assetEntity">Not needed assetEntity.</param>
        public static void Postfix(List<Entity> themes, List<Entity> packs, Entity assetMenuEntity, Entity assetCategoryEntity, Entity assetEntity)
        {
            ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            ObjectToolSystem objectToolSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ObjectToolSystem>();
            SelectMultiplePrefabsUISystem uISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<SelectMultiplePrefabsUISystem>();
            ToolbarUISystem toolbarUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolbarUISystem>();

            if (toolSystem.activeTool != objectToolSystem && toolSystem.activeTool.toolID != null && toolSystem.activeTool.toolID != "Line Tool")
            {
                return;
            }

            PrefabBase prefab = objectToolSystem.GetPrefab();
            if (prefab != null)
            {
                PrefabSystem prefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
                if (!prefabSystem.TryGetEntity(prefab, out Entity prefabEntity))
                {
                    return;
                }

                if (!prefabSystem.EntityManager.HasComponent<TreeData>(prefabEntity) &&
                    !prefabSystem.EntityManager.HasComponent<PlantData>(prefabEntity) &&
                     uISystem.ThemeEntities.Count != themes.Count)
                {
                    if (uISystem.ThemeEntities.Count != 0)
                    {
                        uISystem.UpdateSelectionSet = true;
                    }

                    uISystem.ThemeEntities = themes;
                    AnarchyMod.Instance.Log.Debug($"{nameof(ToolbarUISystemApplyPatch)}.{nameof(Postfix)} Setting UpdateSelectionSet to true while using {toolSystem.activeTool.toolID}.");
                }
            }
        }
    }
}
