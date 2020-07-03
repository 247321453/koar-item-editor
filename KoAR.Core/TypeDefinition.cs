﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace KoAR.Core
{
    public class TypeDefinition
    {
        private static bool TryParseEffectList(string value, [NotNullWhen(true)] out uint[]? effects)
        {
            effects = null;
            if (value.Length % 6 != 0)
            {
                return false;
            }
            var results = value.Length == 0 ? Array.Empty<uint>() : new uint[value.Length / 6];
            for (int i = 0; i < results.Length; i++)
            {
                if (!uint.TryParse(value.Substring(i * 6, 6), NumberStyles.HexNumber, null, out uint effect))
                {
                    return false;
                }
                results[i] = effect;
            }
            effects = results;
            return true;
        }

        private static bool TryLoadFromRow(string[] entries, [NotNullWhen(true)] out TypeDefinition? definition)
        {
            //Category,TypeId,Level,Name,
            //Internal_Name,Durability,Rarity,Sockets
            //Element,ArmorType,CoreEffects,Effects

            definition = null;
            if (entries.Length != 14
                || !Enum.TryParse(entries[0], true, out EquipmentCategory category)
                || !uint.TryParse(entries[1], NumberStyles.HexNumber, null, out uint typeId)
                || !byte.TryParse(entries[2], out byte level)
                || !float.TryParse(entries[5], out float maxDurability)
                || !Enum.TryParse(entries[6], out Rarity rarity)
                || !Enum.TryParse(entries[8], out Element element)
                || !Enum.TryParse(entries[9], out ArmorType armorType)
                || !uint.TryParse(entries[10], NumberStyles.HexNumber, null, out uint prefix)
                || !uint.TryParse(entries[11], NumberStyles.HexNumber, null, out uint suffix)
                || !TryParseEffectList(entries[12], out uint[]? coreEffects)
                || !TryParseEffectList(entries[13], out uint[]? effects))
            {
                return false;
            }
            definition = new TypeDefinition(category, typeId, level, entries[3], entries[4], maxDurability, rarity, entries[7], element, armorType, prefix, suffix, coreEffects, effects);
            return true;
        }

        internal static IEnumerable<TypeDefinition> ParseFile(string path)
        {
            foreach(var line in File.ReadLines(path).Skip(1))
            {
                if(TryLoadFromRow(line.Split(','), out var definition))
                {
                    yield return definition;
                }
            }
        }

        internal TypeDefinition(EquipmentCategory category, uint typeId, byte level, string name, string internalName, float maxDurability, Rarity rarity,
            string sockets, Element element, ArmorType armorType, uint prefix, uint suffix, uint[] coreEffects, uint[] effects)
        {
            Category = category;
            TypeId = typeId;
            Level = level;
            Name = name;
            InternalName = internalName;
            MaxDurability = maxDurability;
            Rarity = rarity;
            Sockets = sockets;
            ArmorType = ArmorType == 0 ? default(ArmorType?) : armorType;
            Element = element == 0 ? default(Element?) : element;
            CoreEffects = coreEffects;
            Effects = effects;
            Prefix = prefix;
            Suffix = suffix;
            // merchant search is case sensitive to avoid affixing the Merchant's hat.
            AffixableName = internalName.IndexOf("common", StringComparison.OrdinalIgnoreCase) != -1 || InternalName.IndexOf("merchant") != -1;
        }

        public EquipmentCategory Category { get; }
        public uint TypeId { get; }
        public byte Level { get; }
        public string Name { get; }
        public string InternalName { get; }
        public float MaxDurability { get; }
        public Rarity Rarity { get; }
        public string Sockets { get; }
        public Element? Element { get; }
        public ArmorType? ArmorType { get; }
        public uint[] CoreEffects { get; }
        public uint[] Effects { get; }
        public uint Prefix { get; }
        public uint Suffix { get; }
        public bool AffixableName { get; } 
    }
}
