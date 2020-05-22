﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    /// <summary>
    /// A wrapper class for <see cref="ItemMemoryInfo"/> that implements <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    public sealed class ItemModel : NotifierBase
    {
        private List<EffectInfo>? _effects;
        private EffectInfo? _selectedEffect;

        public ItemModel(ItemMemoryInfo item)
        {
            this.Item = item;
        }

        public CoreEffectInfo? CoreEffect0 => this.CoreEffects.FirstOrDefault();

        public CoreEffectInfo? CoreEffect1 => this.CoreEffects.Skip(1).FirstOrDefault();

        public CoreEffectInfo? CoreEffect2 => this.CoreEffects.Skip(2).FirstOrDefault();

        public CoreEffectInfo? CoreEffect3 => this.CoreEffects.Skip(3).FirstOrDefault();

        public CoreEffectList CoreEffects => this.Item.CoreEffects;

        public float CurrentDurability
        {
            get => this.Item.CurrentDurability;
            set => this.SetItemValue(value, this.Item.CurrentDurability, value => this.Item.CurrentDurability = value);
        }

        public int EffectCount => this.Item.EffectCount;

        public List<EffectInfo> Effects => this._effects ??= this.Item.ReadEffects();

        public EquipmentType EquipmentType => this.Item.EquipmentType;

        public bool HasCustomName => this.Item.HasCustomName;

        public bool IsUnsellable
        {
            get => this.Item.IsUnsellable;
            set => this.SetItemValue(value, this.Item.IsUnsellable, value => this.Item.IsUnsellable = value);
        }

        public string ItemId => LittleEndianConverter.Convert(this.Item.ItemId);

        public int ItemIndex => this.Item.ItemIndex;

        public string ItemName
        {
            get => this.Item.ItemName;
            set => this.SetItemValue(value, this.Item.ItemName, value => this.Item.ItemName = value);
        }

        public float MaxDurability
        {
            get => this.Item.MaxDurability;
            set => this.SetItemValue(value, this.Item.MaxDurability, value => this.Item.MaxDurability = value);
        }

        public byte MysteryInteger => this.CoreEffects.MysteryInteger;

        public EffectInfo? SelectedEffect
        {
            get => this._selectedEffect ??= this.Effects.FirstOrDefault();
            set => this.SetValue(ref this._selectedEffect, value ?? this.Effects.FirstOrDefault());
        }

        internal ItemMemoryInfo Item
        {
            get;
        }

        public void AddEffect(EffectInfo info)
        {
            this.Effects.Add(info);
            this.Item.WriteEffects(this.Effects);
            Amalur.WriteEquipmentBytes(this.Item, out _);
        }

        public void DeleteEffect(EffectInfo info)
        {
            if (!this.Effects.Remove(info))
            {
                return;
            }
            this.Item.WriteEffects(this.Effects);
            Amalur.WriteEquipmentBytes(this.Item, out _);
        }

        private void SetItemValue<T>(T value, T currentValue, Action<T> setValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(value, currentValue))
            {
                return;
            }
            setValue(value);
            this.OnPropertyChanged(propertyName);
        }
    }
}
