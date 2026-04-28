// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Dalamud.Game;
using ElezenTools.Data.Classes;
using ElezenTools.Data.Internal;
using ElezenTools.Services;
using Lumina.Excel.Sheets;

namespace ElezenTools.Data;

public static partial class ElezenData
{
    public static class Minions
    {
        private static readonly LanguageCache<CollectableDataSet<MinionData, Companion>> Cache = new(BuildMinions);

        /// <summary>
        /// Get all minion data.
        /// </summary>
        public static IReadOnlyDictionary<uint, MinionData> All => GetAll();

        /// <summary>
        /// Get all minion data.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Dictionary of MinionData mapped to its Lumina row ID.</returns>
        public static IReadOnlyDictionary<uint, MinionData> GetAll(ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById;
        }

        /// <summary>
        /// Get a minion by its row ID.
        /// </summary>
        /// <param name="id">Minion row ID.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>MinionData if found, null otherwise.</returns>
        public static MinionData? GetById(uint id, ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById.TryGetValue(id, out var minion) ? minion : null;
        }

        /// <summary>
        /// Get a minion by name.
        /// </summary>
        /// <param name="name">Minion name to look for.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>MinionData if found, null otherwise.</returns>
        public static MinionData? GetByName(string name, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Array.Find(GetDataSet(language).Items, item => string.Equals(item.Name, name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Get all minions currently unlocked on the local character.
        /// Returns an empty list if no character is logged in.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Unlocked minion data for the local character.</returns>
        public static IReadOnlyList<MinionData> GetUnlocked(ClientLanguage? language = null)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                return Array.Empty<MinionData>();
            }

            var dataSet = GetDataSet(language);
            return Array.FindAll(dataSet.Items, item => IsUnlocked(item.Id, language));
        }

        /// <summary>
        /// Check whether a specific minion is unlocked on the local character.
        /// </summary>
        /// <param name="id">Minion row ID.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the minion is unlocked; otherwise false.</returns>
        public static bool IsUnlocked(uint id, ClientLanguage? language = null)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                return false;
            }

            var dataSet = GetDataSet(language);
            return dataSet.RowsById.TryGetValue(id, out var row) && Service.UnlockState.IsCompanionUnlocked(row);
        }

        /// <summary>
        /// Check whether a specific minion is unlocked on the local character.
        /// </summary>
        /// <param name="minion">MinionData to check.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the minion is unlocked; otherwise false.</returns>
        public static bool IsUnlocked(MinionData minion, ClientLanguage? language = null)
        {
            return IsUnlocked(minion.Id, language);
        }

        public static void Refresh()
        {
            Cache.Clear();
        }

        private static CollectableDataSet<MinionData, Companion> GetDataSet(ClientLanguage? language)
        {
            return Cache.Get(ResolveLanguage(language));
        }

        private static CollectableDataSet<MinionData, Companion> BuildMinions(ClientLanguage language)
        {
            var companionSheet = Service.DataManager.GetExcelSheet<Companion>(language);
            if (companionSheet == null)
            {
                return new CollectableDataSet<MinionData, Companion>(
                    Array.Empty<MinionData>(),
                    new Dictionary<uint, MinionData>(),
                    new Dictionary<uint, Companion>());
            }

            var transientDetails = LoadTransientDetails(language);
            var rowsById = new Dictionary<uint, Companion>();
            var items = new List<MinionData>();
            foreach (var row in companionSheet)
            {
                var name = ToText(row.Singular);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var description = transientDetails.TryGetValue(row.RowId, out var transient)
                    ? FirstNonEmpty(ToText(transient.Tooltip), ToText(transient.Description), ToText(transient.DescriptionEnhanced))
                    : string.Empty;

                rowsById[row.RowId] = row;
                items.Add(new MinionData(
                    row.RowId,
                    GetDisplayName(row.Singular, language),
                    description,
                    (uint)row.Order));
            }

            var orderedItems = items
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new CollectableDataSet<MinionData, Companion>(
                orderedItems,
                orderedItems.ToDictionary(item => item.Id),
                rowsById);
        }

        private static Dictionary<uint, CompanionTransient> LoadTransientDetails(ClientLanguage language)
        {
            var transientSheet = Service.DataManager.GetExcelSheet<CompanionTransient>(language);
            if (transientSheet == null)
            {
                return new Dictionary<uint, CompanionTransient>();
            }

            return transientSheet
                .Where(row => row.RowId > 0)
                .ToDictionary(row => row.RowId, row => row);
        }
    }
}
