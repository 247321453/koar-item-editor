﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace KoAR.Core
{
    public static class Amalur
    {
        static Amalur()
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture =
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonSnakeCaseNamingPolicy.Instance,
                Converters = { new JsonStringEnumConverter() }
            };
            using var zipStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(Amalur).Namespace}.Data.zip");
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
            using var buffsStream = archive.GetEntry("buffs.json").Open();
            Buffs = JsonSerializer.DeserializeAsync<Buff[]>(buffsStream, jsonOptions).Result.ToDictionary(buff => buff.Id);
            using var questItemsStream = archive.GetEntry("questItemDefinitions.json").Open();
            QuestItemDefinitions = JsonSerializer.DeserializeAsync<QuestItemDefinition[]>(questItemsStream, jsonOptions).Result.ToDictionary(def => def.Id);
            using var gemsStream = archive.GetEntry("gemDefinitions.csv").Open();
            GemDefinitions = GemDefinition.ParseFile(gemsStream).ToDictionary(def => def.TypeId);
            using var itemsStream = archive.GetEntry("definitions.csv").Open();
            ItemDefinitions = ItemDefinition.ParseFile(itemsStream).ToDictionary(def => def.TypeId);
        }

        public static IReadOnlyDictionary<uint, Buff> Buffs { get; }
        public static IReadOnlyDictionary<uint, GemDefinition> GemDefinitions { get; }
        public static IReadOnlyDictionary<uint, ItemDefinition> ItemDefinitions { get; }
        public static IReadOnlyDictionary<uint, QuestItemDefinition> QuestItemDefinitions { get; }
        public static ReadOnlySpan<uint> PlayerTypeIds => MemoryMarshal.Cast<byte, uint>(new byte[16]{
            0x6D, 0x38, 0x0A, 0x00, // playerHumanMale
            0x6E, 0x38, 0x0A, 0x00, // playerHumanFemale
            0x6F, 0x38, 0x0A, 0x00, // playerElfMale
            0x70, 0x38, 0x0A, 0x00,  // playerElfFemale
        });

        internal static char[] Separator { get; } = { ',' };

        public static Buff GetBuff(uint buffId) => Buffs.GetOrDefault(buffId, new Buff { Id = buffId, Name = "Unknown" });

        [return: MaybeNull, NotNullIfNotNull("defaultValue")]
        internal static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
            where TValue : class => dictionary.TryGetValue(key, out TValue res) ? res : defaultValue;

        public static string? FindSaveGameDirectory()
        {
            // GOG
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"..\LocalLow\THQNOnline\Kingdoms of Amalur Re-Reckoning\autocloud\save");
            if (Directory.Exists(directory))
            {
                return directory;
            }
            // Steam
            directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "userdata");
            return Directory.Exists(directory) && Directory.GetDirectories(directory) is string[] { Length: 1 } userDirs 
                    && (Directory.Exists(directory = Path.Combine(userDirs[0], @"1041720\remote\autocloud\save")) || Directory.Exists(directory = Path.Combine(userDirs[0], @"102500\remote")))
                ? directory
                : null;
        }
    }
}
