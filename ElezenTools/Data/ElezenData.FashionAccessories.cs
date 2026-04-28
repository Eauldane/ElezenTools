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
    public static class FashionAccessories
    {
        private static readonly LanguageCache<CollectableDataSet<FashionAccessoryData, Ornament>> Cache = new(BuildFashionAccessories);

        /// <summary>
        /// Get all fashion accessory data.
        /// </summary>
        public static IReadOnlyDictionary<uint, FashionAccessoryData> All => GetAll();

        /// <summary>
        /// Get all fashion accessory data.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Dictionary of FashionAccessoryData mapped to its Lumina row ID.</returns>
        public static IReadOnlyDictionary<uint, FashionAccessoryData> GetAll(ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById;
        }

        /// <summary>
        /// Get a fashion accessory by its row ID.
        /// </summary>
        /// <param name="id">Fashion accessory row ID.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>FashionAccessoryData if found, null otherwise.</returns>
        public static FashionAccessoryData? GetById(uint id, ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById.TryGetValue(id, out var accessory) ? accessory : null;
        }

        /// <summary>
        /// Get a fashion accessory by name.
        /// </summary>
        /// <param name="name">Fashion accessory name to look for.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>FashionAccessoryData if found, null otherwise.</returns>
        public static FashionAccessoryData? GetByName(string name, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Array.Find(GetDataSet(language).Items, item => string.Equals(item.Name, name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Get all fashion accessories currently unlocked on the local character.
        /// Returns an empty list if no character is logged in.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Unlocked fashion accessory data for the local character.</returns>
        public static IReadOnlyList<FashionAccessoryData> GetUnlocked(ClientLanguage? language = null)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                return Array.Empty<FashionAccessoryData>();
            }

            var dataSet = GetDataSet(language);
            return Array.FindAll(dataSet.Items, item => IsUnlocked(item.Id, language));
        }

        /// <summary>
        /// Check whether a specific fashion accessory is unlocked on the local character.
        /// </summary>
        /// <param name="id">Fashion accessory row ID.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the fashion accessory is unlocked; otherwise false.</returns>
        public static bool IsUnlocked(uint id, ClientLanguage? language = null)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                return false;
            }

            var dataSet = GetDataSet(language);
            return dataSet.RowsById.TryGetValue(id, out var row) && Service.UnlockState.IsOrnamentUnlocked(row);
        }

        /// <summary>
        /// Check whether a specific fashion accessory is unlocked on the local character.
        /// </summary>
        /// <param name="accessory">FashionAccessoryData to check.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the fashion accessory is unlocked; otherwise false.</returns>
        public static bool IsUnlocked(FashionAccessoryData accessory, ClientLanguage? language = null)
        {
            return IsUnlocked(accessory.Id, language);
        }

        public static void Refresh()
        {
            Cache.Clear();
        }

        private static CollectableDataSet<FashionAccessoryData, Ornament> GetDataSet(ClientLanguage? language)
        {
            return Cache.Get(ResolveLanguage(language));
        }

        private static CollectableDataSet<FashionAccessoryData, Ornament> BuildFashionAccessories(ClientLanguage language)
        {
            var accessorySheet = Service.DataManager.GetExcelSheet<Ornament>(language);
            if (accessorySheet == null)
            {
                return new CollectableDataSet<FashionAccessoryData, Ornament>(
                    Array.Empty<FashionAccessoryData>(),
                    new Dictionary<uint, FashionAccessoryData>(),
                    new Dictionary<uint, Ornament>());
            }

            var transientDetails = LoadTransientDetails(language);
            var rowsById = new Dictionary<uint, Ornament>();
            var items = new List<FashionAccessoryData>();
            foreach (var row in accessorySheet)
            {
                var name = ToText(row.Singular);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var description = transientDetails.TryGetValue(row.Transient, out var transient)
                    ? ToText(transient.Text)
                    : string.Empty;

                rowsById[row.RowId] = row;
                items.Add(new FashionAccessoryData(
                    row.RowId,
                    name,
                    description,
                    row.Order < 0 ? short.MaxValue : row.Order));
            }

            var orderedItems = items
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new CollectableDataSet<FashionAccessoryData, Ornament>(
                orderedItems,
                orderedItems.ToDictionary(item => item.Id),
                rowsById);
        }

        private static Dictionary<uint, OrnamentTransient> LoadTransientDetails(ClientLanguage language)
        {
            var transientSheet = Service.DataManager.GetExcelSheet<OrnamentTransient>(language);
            if (transientSheet == null)
            {
                return new Dictionary<uint, OrnamentTransient>();
            }

            return transientSheet
                .Where(row => row.RowId > 0)
                .ToDictionary(row => row.RowId, row => row);
        }
    }
}
