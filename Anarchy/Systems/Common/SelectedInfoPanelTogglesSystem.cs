﻿// <copyright file="SelectedInfoPanelTogglesSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>
namespace Anarchy.Systems.Common
{
    using Anarchy.Components;
    using Anarchy.Extensions;
    using Anarchy.Systems.NetworkAnarchy;
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.UI.Binding;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// Adds toggles to selected info panel for entites that can receive Anarchy mod components.
    /// </summary>
    public partial class SelectedInfoPanelTogglesSystem : ExtendedInfoSectionBase
    {
        private ILog m_Log;
        private ValueBindingHelper<NetworkAnarchyUISystem.ButtonState> m_HasPreventOverride;
        private ValueBindingHelper<NetworkAnarchyUISystem.ButtonState> m_HasTransformRecord;
        private ToolSystem m_ToolSystem;
        private Game.Objects.Transform m_RecentTransform;
        private Entity m_PreviouslySelectedEntity;

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
            m_HasPreventOverride = CreateBinding("HasPreventOverride", NetworkAnarchyUISystem.ButtonState.Off);
            m_HasTransformRecord = CreateBinding("HasTransformRecord", NetworkAnarchyUISystem.ButtonState.Off);

            // Thse create listeners for events from UI that trigger actions here.
            CreateTrigger("PreventOverrideButtonToggled", PreventOverrideButtonToggled);
            CreateTrigger("TransformRecordButtonToggled", TransformRecordButtonToggled);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();
            bool overridable = CheckOverridable(selectedEntity);
            bool disturbable = CheckDisturbable(selectedEntity);
            visible = overridable || disturbable;
            if (!visible)
            {
                return;
            }

            if (m_PreviouslySelectedEntity != selectedEntity)
            {
                m_PreviouslySelectedEntity = selectedEntity;
                EntityManager.TryGetComponent(selectedEntity, out m_RecentTransform);
            }

            if (overridable &&
                EntityManager.HasComponent<PreventOverride>(selectedEntity) &&
                m_HasPreventOverride.Value != NetworkAnarchyUISystem.ButtonState.On)
            {
                m_HasPreventOverride.Value = NetworkAnarchyUISystem.ButtonState.On;
            }
            else if (overridable &&
                    !EntityManager.HasComponent<PreventOverride>(selectedEntity) &&
                     m_HasPreventOverride.Value != NetworkAnarchyUISystem.ButtonState.Off)
            {
                m_HasPreventOverride.Value = NetworkAnarchyUISystem.ButtonState.Off;
            }
            else if (!overridable)
            {
                m_HasPreventOverride.Value = NetworkAnarchyUISystem.ButtonState.Hidden;
            }

            if (disturbable &&
                EntityManager.HasComponent<TransformRecord>(selectedEntity) &&
                m_HasTransformRecord.Value != NetworkAnarchyUISystem.ButtonState.On)
            {
                m_HasTransformRecord.Value = NetworkAnarchyUISystem.ButtonState.On;
            }
            else if (disturbable &&
                    !EntityManager.HasComponent<TransformRecord>(selectedEntity) &&
                     m_HasTransformRecord.Value != NetworkAnarchyUISystem.ButtonState.Off)
            {
                m_HasTransformRecord.Value = NetworkAnarchyUISystem.ButtonState.Off;
            }
            else if (!disturbable)
            {
                m_HasTransformRecord.Value = NetworkAnarchyUISystem.ButtonState.Hidden;
            }

            if (!EntityManager.HasComponent<Game.Common.Owner>(selectedEntity) ||
                 EntityManager.HasComponent<TransformRecord>(selectedEntity))
            {
                return;
            }

            if (EntityManager.TryGetComponent(selectedEntity, out Game.Objects.Transform transform) &&
               !m_RecentTransform.Equals(transform))
            {
                EntityManager.AddComponent<TransformRecord>(selectedEntity);
                TransformRecord transformRecord = new TransformRecord()
                {
                    m_Position = transform.m_Position,
                    m_Rotation = transform.m_Rotation,
                };
                EntityManager.SetComponentData(selectedEntity, transformRecord);
                EntityManager.AddComponent<PreventOverride>(selectedEntity);
                RequestUpdate();
            }
        }

        private bool ApproximateTransforms(Game.Objects.Transform original, Game.Objects.Transform comparision)
        {
            if (Mathf.Approximately(original.m_Position.x, comparision.m_Position.x) &&
                Mathf.Approximately(original.m_Position.y, comparision.m_Position.y) &&
                Mathf.Approximately(original.m_Position.z, comparision.m_Position.z) &&
                Mathf.Approximately(original.m_Rotation.value.x, comparision.m_Rotation.value.x) &&
                Mathf.Approximately(original.m_Rotation.value.y, comparision.m_Rotation.value.y) &&
                Mathf.Approximately(original.m_Rotation.value.z, comparision.m_Rotation.value.z) &&
                Mathf.Approximately(original.m_Rotation.value.w, comparision.m_Rotation.value.w))
            {
                return true;
            }

            return false;
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

            RequestUpdate();
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

            RequestUpdate();
        }

        /// <summary>
        /// Validates whether selected entity can receive prevent override component.
        /// </summary>
        /// <returns>True if entity can receive anarchy components. False if not approved.</returns>
        private bool CheckOverridable(Entity instanceEntity)
        {
            PrefabBase prefabBase = null;
            if (EntityManager.TryGetComponent(instanceEntity, out PrefabRef prefabRef) &&
                m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out prefabBase) &&
              ((prefabBase is StaticObjectPrefab &&
                EntityManager.TryGetComponent(prefabRef.m_Prefab, out ObjectGeometryData objectGeometryData) &&
                prefabBase is not BuildingPrefab &&
               (objectGeometryData.m_Flags & Game.Objects.GeometryFlags.Overridable) == Game.Objects.GeometryFlags.Overridable) ||
               (m_ToolSystem.actionMode.IsGame() &&
                prefabBase.GetPrefabID().ToString() == "NetPrefab:Lane Editor Container" &&
                EntityManager.HasBuffer<Game.Net.SubLane>(instanceEntity))))
            {
                m_Log.Debug($"{nameof(SelectedInfoPanelTogglesSystem)}.{nameof(CheckOverridable)} Acceptable selected entity.");
                return true;
            }

            if (prefabBase != null)
            {
                m_Log.Debug($"{nameof(SelectedInfoPanelTogglesSystem)}.{nameof(CheckOverridable)}  selected entity {prefabBase.name} is not acceptable for anarchy components.");
            }

            return false;
        }

        private bool CheckDisturbable(Entity instanceEntity)
        {
            if (CheckOverridable(instanceEntity))
            {
                return true;
            }

            if (EntityManager.HasComponent<Owner>(instanceEntity) &&
                EntityManager.HasComponent<Game.Objects.Transform>(instanceEntity) &&
                EntityManager.TryGetComponent(instanceEntity, out PrefabRef prefabRef) &&
                m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out PrefabBase prefabBase) &&
               (prefabBase is StaticObjectPrefab ||
                prefabBase is MarkerObjectPrefab) &&
                prefabBase is not BuildingPrefab)
            {
                return true;
            }

            return false;
        }
    }
}
