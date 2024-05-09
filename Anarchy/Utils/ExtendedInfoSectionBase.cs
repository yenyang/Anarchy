// <copyright file="ExtendedUISystemBase.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Utils
{
    using System;
    using Colossal.UI.Binding;
    using Game.UI;
    using Game.UI.InGame;

    public abstract partial class ExtendedInfoSectionBase : InfoSectionBase
    {
        public ValueBindingHelperInfo<T> CreateBinding<T>(string key, T initialValue)
        {
            var helper = new ValueBindingHelperInfo<T>(new(AnarchyMod.Id, key, initialValue));

            AddBinding(helper.Binding);

            return helper;
        }

        public ValueBindingHelperInfo<T> CreateBinding<T>(string key, string setterKey, T initialValue, Action<T> updateCallBack = null)
        {
            var helper = new ValueBindingHelperInfo<T>(new(AnarchyMod.Id, key, initialValue), updateCallBack);
            var trigger = new TriggerBinding<T>(AnarchyMod.Id, setterKey, helper.UpdateCallback);

            AddBinding(helper.Binding);
            AddBinding(trigger);

            return helper;
        }

        public ValueBindingHelperInfo<T[]> CreateBinding<T>(string key, T[] initialValue) where T : IJsonWritable
        {
            var helper = new ValueBindingHelperInfo<T[]>(new(AnarchyMod.Id, key, initialValue, new ArrayWriter<T>(new ValueWriter<T>())));

            AddBinding(helper.Binding);

            return helper;
        }

        public ValueBindingHelperInfo<T[]> CreateBinding<T>(string key, string setterKey, T[] initialValue, Action<T[]> updateCallBack = null) where T : IJsonWritable
        {
            var helper = new ValueBindingHelperInfo<T[]>(new(AnarchyMod.Id, key, initialValue, new ArrayWriter<T>(new ValueWriter<T>())), updateCallBack);
            var trigger = new TriggerBinding<T[]>(AnarchyMod.Id, setterKey, helper.UpdateCallback);

            AddBinding(helper.Binding);
            AddBinding(trigger);

            return helper;
        }

        public GetterValueBinding<T> CreateBinding<T>(string key, Func<T> getterFunc)
        {
            var binding = new GetterValueBinding<T>(AnarchyMod.Id, key, getterFunc);

            AddBinding(binding);

            return binding;
        }

        public GetterValueBinding<T[]> CreateBinding<T>(string key, Func<T[]> getterFunc) where T : IJsonWritable
        {
            var binding = new GetterValueBinding<T[]>(AnarchyMod.Id, key, getterFunc, new ArrayWriter<T>(new ValueWriter<T>()));

            AddBinding(binding);

            return binding;
        }

        public TriggerBinding CreateTrigger(string key, Action action)
        {
            var binding = new TriggerBinding(AnarchyMod.Id, key, action);

            AddBinding(binding);

            return binding;
        }

        public TriggerBinding<T1> CreateTrigger<T1>(string key, Action<T1> action)
        {
            var binding = new TriggerBinding<T1>(AnarchyMod.Id, key, action);

            AddBinding(binding);

            return binding;
        }

        public TriggerBinding<T1, T2> CreateTrigger<T1, T2>(string key, Action<T1, T2> action)
        {
            var binding = new TriggerBinding<T1, T2>(AnarchyMod.Id, key, action);

            AddBinding(binding);

            return binding;
        }

        public TriggerBinding<T1, T2, T3> CreateTrigger<T1, T2, T3>(string key, Action<T1, T2, T3> action)
        {
            var binding = new TriggerBinding<T1, T2, T3>(AnarchyMod.Id, key, action);

            AddBinding(binding);

            return binding;
        }

        public TriggerBinding<T1, T2, T3, T4> CreateTrigger<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> action)
        {
            var binding = new TriggerBinding<T1, T2, T3, T4>(AnarchyMod.Id, key, action);

            AddBinding(binding);

            return binding;
        }
    }

    public class ValueBindingHelperInfo<T>
    {
        private readonly Action<T> _updateCallBack;

        public ValueBinding<T> Binding { get; }

        public T Value { get => Binding.value; set => Binding.Update(value); }

        public ValueBindingHelperInfo(ValueBinding<T> binding, Action<T> updateCallBack = null)
        {
            Binding = binding;
            _updateCallBack = updateCallBack;
        }

        public void UpdateCallback(T value)
        {
            Binding.Update(value);
            _updateCallBack?.Invoke(value);
        }

        public static implicit operator T(ValueBindingHelperInfo<T> helper) => helper.Binding.value;
    }
}
