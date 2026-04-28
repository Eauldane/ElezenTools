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
    public static class Facewear
    {
        private static readonly LanguageCache<CollectableDataSet<FacewearData, Glasses>> Cache = new(BuildFacewear);

        /// <summary>
        /// Get all facewear data.
        /// </summary>
        public static IReadOnlyDictionary<uint, FacewearData> All => GetAll();

        /// <summary>
        /// Get all facewear data.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Dictionary of FacewearData mapped to its Lumina row ID.</returns>
        public static IReadOnlyDictionary<uint, FacewearData> GetAll(ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById;
        }

        /// <summary>
        /// Get a facewear item by its row ID.
        /// </summary>
        /// <param name="id">Facewear row ID.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>FacewearData if found, null otherwise.</returns>
        public static FacewearData? GetById(uint id, ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById.TryGetValue(id, out var item) ? item : null;
        }

        /// <summary>
        /// Get a facewear item by name.
        /// </summary>
        /// <param name="name">Facewear name to look for.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>FacewearData if found, null otherwise.</returns>
        public static FacewearData? GetByName(string name, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Array.Find(GetDataSet(language).Items, item => string.Equals(item.Name, name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Get all facewear items currently unlocked on the local character.
        /// Returns an empty list if no character is logged in.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Unlocked facewear data for the local character.</returns>
        public static IReadOnlyList<FacewearData> GetUnlocked(ClientLanguage? language = null)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                return Array.Empty<FacewearData>();
            }

            var dataSet = GetDataSet(language);
            return Array.FindAll(dataSet.Items, item => IsUnlocked(item.Id, language));
        }

        /// <summary>
        /// Check whether a specific facewear item is unlocked on the local character.
        /// </summary>
        /// <param name="id">Facewear row ID.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the facewear item is unlocked; otherwise false.</returns>
        public static bool IsUnlocked(uint id, ClientLanguage? language = null)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                return false;
            }

            var dataSet = GetDataSet(language);
            return dataSet.RowsById.TryGetValue(id, out var row) && Service.UnlockState.IsGlassesUnlocked(row);
        }

        /// <summary>
        /// Check whether a specific facewear item is unlocked on the local character.
        /// </summary>
        /// <param name="item">FacewearData to check.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the facewear item is unlocked; otherwise false.</returns>
        public static bool IsUnlocked(FacewearData item, ClientLanguage? language = null)
        {
            return IsUnlocked(item.Id, language);
        }

        public static void Refresh()
        {
            Cache.Clear();
        }

        private static CollectableDataSet<FacewearData, Glasses> GetDataSet(ClientLanguage? language)
        {
            return Cache.Get(ResolveLanguage(language));
        }

        private static CollectableDataSet<FacewearData, Glasses> BuildFacewear(ClientLanguage language)
        {
            var facewearSheet = Service.DataManager.GetExcelSheet<Glasses>(language);
            if (facewearSheet == null)
            {
                return new CollectableDataSet<FacewearData, Glasses>(
                    Array.Empty<FacewearData>(),
                    new Dictionary<uint, FacewearData>(),
                    new Dictionary<uint, Glasses>());
            }

            var styles = LoadStyles(language);
            var rowsById = new Dictionary<uint, Glasses>();
            var items = new List<FacewearData>();
            foreach (var row in facewearSheet)
            {
                var name = FirstNonEmpty(ToText(row.Name), ToText(row.Singular));
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var hasStyle = styles.TryGetValue(row.Style.RowId, out var style);
                var styleName = hasStyle
                    ? FirstNonEmpty(ToText(style.Name), ToText(style.Singular))
                    : string.Empty;

                rowsById[row.RowId] = row;
                items.Add(new FacewearData(
                    row.RowId,
                    name,
                    ToText(row.Description),
                    row.Style.RowId,
                    styleName,
                    hasStyle ? style.Order : ushort.MaxValue));
            }

            var orderedItems = items
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new CollectableDataSet<FacewearData, Glasses>(
                orderedItems,
                orderedItems.ToDictionary(item => item.Id),
                rowsById);
        }

        private static Dictionary<uint, GlassesStyle> LoadStyles(ClientLanguage language)
        {
            var styleSheet = Service.DataManager.GetExcelSheet<GlassesStyle>(language);
            if (styleSheet == null)
            {
                return new Dictionary<uint, GlassesStyle>();
            }

            return styleSheet
                .Where(row => row.RowId > 0)
                .ToDictionary(row => row.RowId, row => row);
        }
    }
}
