// <copyright file="ErrorCheck.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Domain
{
    using Anarchy.Settings;
    using Game.Tools;

    /// <summary>
    /// A class to use for UI binding for error check handling.
    /// </summary>
    public class ErrorCheck
    {
        private int m_Index;
        private int m_ID;
        private string m_LocaleKey;
        private int m_DisabledState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorCheck"/> class.
        /// </summary>
        public ErrorCheck()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorCheck"/> class.
        /// </summary>
        /// <param name="errorType">Type of error.</param>
        /// <param name="disableState">When should error check be disabled.</param>
        /// <param name="index">Index in array of Error checks.</param>
        public ErrorCheck(ErrorType errorType, DisableState disableState, int index)
        {
            m_ID = (int)errorType;
            m_LocaleKey = LocaleEN.ErrorCheckKey(errorType);
            m_DisabledState = (int)disableState;
            m_Index = index;
        }

        /// <summary>
        /// An enum for when to disable an error check.
        /// </summary>
        public enum DisableState
        {
            /// <summary>
            /// Even anarchy does not disable this error.
            /// </summary>
            Never,

            /// <summary>
            /// Disable when Anarchy is Active.
            /// </summary>
            WithAnarchy,

            /// <summary>
            /// Always Disable regardless of Anarchy.
            /// </summary>
            Always,
        }

        /// <summary>
        /// Gets or sets a value for the ID.
        /// </summary>
        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        /// <summary>
        /// Gets or sets a value for Locale Key.
        /// </summary>
        public string LocaleKey
        {
            get { return m_LocaleKey; }
            set { m_LocaleKey = value; }
        }

        /// <summary>
        /// Gets or sets a value for Disabled state.
        /// </summary>
        public int DisabledState
        {
            get { return m_DisabledState; }
            set { m_DisabledState = value; }
        }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        public int Index
        {
            get { return m_Index; }
            set { m_Index = value; }
        }

        /// <summary>
        /// Gets the ErrorType from the ErrorCheck.
        /// </summary>
        /// <returns>ErrorType enum associated with this error check.</returns>
        public ErrorType GetErrorType()
        {
            return (ErrorType)m_ID;
        }

        /// <summary>
        /// Gets the Disable state from ErrorCheck.
        /// </summary>
        /// <returns>ErrorType enum associated with this error check.</returns>
        public DisableState GetDisableState()
        {
            return (DisableState)m_DisabledState;
        }
    }
}
