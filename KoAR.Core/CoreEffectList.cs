﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace KoAR.Core
{
    public class CoreEffectList : IList<CoreEffectInfo>, IReadOnlyList<CoreEffectInfo>
    {
        private static class Offsets
        {
            public const int MysteryInteger = 13;
            public const int EffectCount = MysteryInteger + 4;
            public const int FirstEffect = EffectCount + 4;

        }

        private static ReadOnlySpan<uint> Prefixes => new[] { 0x57_8E_73u, 0x58_6E_AAu, 0x4B_03_f9u, 0x4b_43_f4u };
        private readonly List<CoreEffectInfo> _list = new List<CoreEffectInfo>();

        public CoreEffectInfo this[int index]
        {
            get => _list[index];
            set
            {
                _list[index] = value;// this'll throw an exception if out of bound so who cares.
                Serialize();
            }
        }

        public CoreEffectList(int itemIndex, ReadOnlySpan<byte> span)
        {
            ItemIndex = itemIndex;
            var count = (int)span[Offsets.EffectCount];
            DataLength = Offsets.EffectCount + (count * 24) + 8;
            Bytes = span.Slice(0, DataLength).ToArray();
            var firstDisplayEffect = Offsets.FirstEffect + (count * 16) + 8;
            for (int i = 0; i < count; i++)
            {
                _list.Add(new CoreEffectInfo
                {
                    Code = MemoryUtilities.Read<uint>(span, firstDisplayEffect + i * 8).ToString("X6")
                });
            }
        }

        public byte[] Bytes { get; private set; }
        public int ItemIndex { get; }
        public int DataLength { get; set; }
        public byte MysteryInteger
        {
            get => Bytes[Offsets.MysteryInteger];
        }

        public int Count
        {
            get => _list.Count;
        }

        public bool IsReadOnly => false;

        public void Add(CoreEffectInfo item)
        {
            if (_list.Count == 4)
            {
                throw new InvalidOperationException("List can only have four elements");
            }
            _list.Add(item);
            Serialize();
        }

        public void Clear()
        {
            _list.Clear();
            Serialize();
        }

        public void Insert(int index, CoreEffectInfo item)
        {
            _list.Remove(item);
            Serialize();
        }

        public bool Remove(CoreEffectInfo item)
        {
            if (_list.Remove(item))
            {
                Serialize();
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index); // this'll throw if you did something stupid.

            Serialize();
        }

        private void Serialize()
        {
            byte currentCount = Bytes[Offsets.EffectCount];
            var currentLength = currentCount * 24 + 8;
            var newCount = _list.Count;
            Span<ulong> effectData = stackalloc ulong[newCount * 3 + 1];
            for (int i = 0; i < newCount; i++)
            {
                ulong effect = uint.Parse(_list[i].Code, NumberStyles.HexNumber);
                effectData[i * 2] = Prefixes[i] | effect << 32;
                effectData[(i * 2) + 1] = ulong.MaxValue;
                effectData[(newCount * 2) + i] = effect | (ulong)uint.MaxValue << 32;
            }
            Bytes = MemoryUtilities.ReplaceBytes(Bytes, Offsets.FirstEffect, currentLength, MemoryMarshal.AsBytes(effectData));
            Bytes[Offsets.EffectCount + 4 + (16 * newCount)] = Bytes[Offsets.EffectCount] = (byte)newCount;
        }

        public bool Contains(CoreEffectInfo item) => _list.Contains(item);
        public int IndexOf(CoreEffectInfo item) => _list.IndexOf(item);
        public void CopyTo(CoreEffectInfo[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        public IEnumerator<CoreEffectInfo> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}