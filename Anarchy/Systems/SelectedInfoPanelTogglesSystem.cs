// <copyright file="SelectedInfoPanelTogglesSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>
namespace Anarchy.Systems
{
    using Colossal.UI.Binding;
    using Anarchy.Utils;
    using Anarchy.Components;

    /// <summary>
    /// Addes toggles to selected info panel for entites that can receive Anarchy mod components.
    /// </summary>
    public partial class SelectedInfoPanelTogglesSystem : ExtendedInfoSectionBase
    {
        /// <inheritdoc/>
        protected override string group => "Anarchy";

        private ValueBindingHelperInfo<bool> m_HasPreventOverride;

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
            m_HasPreventOverride = CreateBinding("HasPreventOverride", false);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();
            visible = true;
            if (m_HasPreventOverride.Value != EntityManager.HasComponent<PreventOverride>(selectedEntity))
            {
                m_HasPreventOverride.UpdateCallback(EntityManager.HasComponent<PreventOverride>(selectedEntity));
            }

            RequestUpdate();
        }

    }
}
