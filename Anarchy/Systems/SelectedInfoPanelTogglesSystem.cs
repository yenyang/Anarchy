// <copyright file="SelectedInfoPanelTogglesSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>
namespace Anarchy.Systems
{
    using Anarchy.Components;
    using Anarchy.Extensions;
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.UI.Binding;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Entities;

    /// <summary>
    /// Addes toggles to selected info panel for entites that can receive Anarchy mod components.
    /// </summary>
    public partial class SelectedInfoPanelTogglesSystem : ExtendedInfoSectionBase
    {
        private ILog m_Log;
        private ValueBindingHelper<bool> m_HasPreventOverride;
        private ValueBindingHelper<bool> m_HasTransformRecord;
        private ToolSystem m_ToolSystem;

        /// <inheritdoc/>
        protected override string group => "Anarchy";

        /// <inheritdoc/>
        public override void OnWriteProperties(IJsonWriter writer)
        {
        }

        /// <inheritdoc/>
        protected override void OnProcess()
        {
        }

        /// <inheritdoc/>
        protected override void Reset()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_InfoUISystem.AddMiddleSection(this);
            m_Log = AnarchyMod.Instance.Log;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();

            // These create bindings to the UI via Extension methods.
            m_HasPreventOverride = CreateBinding("HasPreventOverride", false);
            m_HasTransformRecord = CreateBinding("HasTransformRecord", false);

            // Thse create listeners for events from UI that trigger actions here.
            CreateTrigger("PreventOverrideButtonToggled", PreventOverrideButtonToggled);
            CreateTrigger("TransformRecordButtonToggled", TransformRecordButtonToggled);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();
            visible = ScreenSelectedEntity();
            if (visible)
            {
                if (m_HasPreventOverride.Value != EntityManager.HasComponent<PreventOverride>(selectedEntity))
                {
                    m_HasPreventOverride.UpdateCallback(EntityManager.HasComponent<PreventOverride>(selectedEntity));
                }

                if (m_HasTransformRecord.Value != EntityManager.HasComponent<TransformRecord>(selectedEntity))
                {
                    m_HasTransformRecord.UpdateCallback(EntityManager.HasComponent<TransformRecord>(selectedEntity));
                }
            }

            RequestUpdate();
        }

        /// <summary>
        /// Event for toggling prevent override component after clicking button in UI. Should only be possible if selected entity was already screened.
        /// </summary>
        private void PreventOverrideButtonToggled()
        {
            if (EntityManager.HasComponent<PreventOverride>(selectedEntity))
            {
                EntityManager.RemoveComponent<PreventOverride>(selectedEntity);
            }
            else
            {
                EntityManager.AddComponent<PreventOverride>(selectedEntity);
            }
        }

        /// <summary>
        /// Event for toggling prevent override component after clicking button in UI. Should only be possible if selected entity was already screened.
        /// </summary>
        private void TransformRecordButtonToggled()
        {
            if (EntityManager.HasComponent<TransformRecord>(selectedEntity))
            {
                EntityManager.RemoveComponent<TransformRecord>(selectedEntity);
            }
            else if (EntityManager.TryGetComponent(selectedEntity, out Game.Objects.Transform transform))
            {
                EntityManager.AddComponent<TransformRecord>(selectedEntity);
                TransformRecord transformRecord = new TransformRecord() { m_Position = transform.m_Position, m_Rotation = transform.m_Rotation };
                EntityManager.SetComponentData(selectedEntity, transformRecord);
            }
        }

        /// <summary>
        /// Validates whether selected entity should receive anarchy components.
        /// </summary>
        /// <returns>True if entity can receive anarchy components. False if not approved.</returns>
        private bool ScreenSelectedEntity()
        {
            PrefabBase prefabBase = null;
            if (EntityManager.TryGetComponent(selectedEntity, out PrefabRef prefabRef) && !EntityManager.HasComponent<Owner>(selectedEntity))
            {
                if (m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out prefabBase))
                {
                    if (prefabBase is StaticObjectPrefab && EntityManager.TryGetComponent(prefabRef.m_Prefab, out ObjectGeometryData objectGeometryData) && prefabBase is not BuildingPrefab)
                    {
                        if ((objectGeometryData.m_Flags & Game.Objects.GeometryFlags.Overridable) == Game.Objects.GeometryFlags.Overridable)
                        {
                            m_Log.Debug($"{nameof(SelectedInfoPanelTogglesSystem)}.{nameof(ScreenSelectedEntity)} Acceptable selected entity.");
                            return true;
                        }
                    }
                    else if (m_ToolSystem.actionMode.IsGame() && prefabBase.GetPrefabID().ToString() == "NetPrefab:Lane Editor Container" && EntityManager.TryGetBuffer(selectedEntity, isReadOnly: true, out DynamicBuffer<Game.Net.SubLane> subLaneBuffer))
                    {
                        m_Log.Debug($"{nameof(SelectedInfoPanelTogglesSystem)}.{nameof(ScreenSelectedEntity)} Acceptable selected entity.");
                        return true;
                    }
                }
            }

            if (prefabBase != null)
            {
                m_Log.Debug($"{nameof(SelectedInfoPanelTogglesSystem)}.{nameof(ScreenSelectedEntity)}  selected entity {prefabBase.name} is not acceptable for anarchy components.");
            }

            return false;
        }
    }
}
