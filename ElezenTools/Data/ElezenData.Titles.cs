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
    public static class Titles
    {
        private static readonly LanguageCache<CollectableDataSet<TitleData, Title>> Cache = new(BuildTitles);

        /// <summary>
        /// Get all title data.
        /// </summary>
        public static IReadOnlyDictionary<uint, TitleData> All => GetAll();

        /// <summary>
        /// Whether the client has loaded the title list for the current character yet.
        /// Open the title selector first if this returns false.
        /// </summary>
        public static bool IsUnlockListLoaded => Service.ClientState.IsLoggedIn && Service.UnlockState.IsTitleListLoaded;

        /// <summary>
        /// Get all title data.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Dictionary of TitleData mapped to its Lumina row ID.</returns>
        public static IReadOnlyDictionary<uint, TitleData> GetAll(ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById;
        }

        /// <summary>
        /// Get a title by its row ID.
        /// </summary>
        /// <param name="id">Title row ID.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>TitleData if found, null otherwise.</returns>
        public static TitleData? GetById(uint id, ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById.TryGetValue(id, out var title) ? title : null;
        }

        /// <summary>
        /// Get a title by name.
        /// </summary>
        /// <param name="name">Title name to look for.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>TitleData if found, null otherwise.</returns>
        public static TitleData? GetByName(string name, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Array.Find(GetDataSet(language).Items, item => string.Equals(item.Name, name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Get all titles currently unlocked on the local character.
        /// Returns an empty list if the title list has not been loaded yet.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Unlocked title data for the local character.</returns>
        public static IReadOnlyList<TitleData> GetUnlocked(ClientLanguage? language = null)
        {
            if (!IsUnlockListLoaded)
            {
                return Array.Empty<TitleData>();
            }

            var dataSet = GetDataSet(language);
            return Array.FindAll(dataSet.Items, item => IsUnlocked(item.Id, language));
        }

        /// <summary>
        /// Check whether a specific title is unlocked on the local character.
        /// </summary>
        /// <param name="id">Title row ID.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the title is unlocked and the list is loaded; otherwise false.</returns>
        public static bool IsUnlocked(uint id, ClientLanguage? language = null)
        {
            if (!IsUnlockListLoaded)
            {
                return false;
            }

            var dataSet = GetDataSet(language);
            return dataSet.RowsById.TryGetValue(id, out var row) && Service.UnlockState.IsTitleUnlocked(row);
        }

        /// <summary>
        /// Check whether a specific title is unlocked on the local character.
        /// </summary>
        /// <param name="title">TitleData to check.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the title is unlocked and the list is loaded; otherwise false.</returns>
        public static bool IsUnlocked(TitleData title, ClientLanguage? language = null)
        {
            return IsUnlocked(title.Id, language);
        }

        public static void Refresh()
        {
            Cache.Clear();
        }

        private static CollectableDataSet<TitleData, Title> GetDataSet(ClientLanguage? language)
        {
            return Cache.Get(ResolveLanguage(language));
        }

        private static CollectableDataSet<TitleData, Title> BuildTitles(ClientLanguage language)
        {
            var titleSheet = Service.DataManager.GetExcelSheet<Title>(language);
            if (titleSheet == null)
            {
                return new CollectableDataSet<TitleData, Title>(
                    Array.Empty<TitleData>(),
                    new Dictionary<uint, TitleData>(),
                    new Dictionary<uint, Title>());
            }

            var rowsById = new Dictionary<uint, Title>();
            var items = new List<TitleData>();
            foreach (var row in titleSheet)
            {
                var masculine = ToText(row.Masculine);
                var feminine = ToText(row.Feminine);
                var name = BuildTitleName(masculine, feminine);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                rowsById[row.RowId] = row;
                items.Add(new TitleData(
                    row.RowId,
                    name,
                    masculine,
                    feminine,
                    row.IsPrefix,
                    row.Order));
            }

            var orderedItems = items
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new CollectableDataSet<TitleData, Title>(
                orderedItems,
                orderedItems.ToDictionary(item => item.Id),
                rowsById);
        }
    }
}
