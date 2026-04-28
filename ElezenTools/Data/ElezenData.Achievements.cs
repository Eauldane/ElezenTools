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
    public static class Achievements
    {
        private static readonly LanguageCache<CollectableDataSet<AchievementData, Achievement>> Cache = new(BuildAchievements);

        /// <summary>
        /// Get all achievement data.
        /// </summary>
        public static IReadOnlyDictionary<uint, AchievementData> All => GetAll();

        /// <summary>
        /// Whether the client has loaded the achievement list for the current character yet.
        /// Open the achievements window first if this returns false.
        /// </summary>
        public static bool IsUnlockListLoaded => Service.ClientState.IsLoggedIn && Service.UnlockState.IsAchievementListLoaded;

        /// <summary>
        /// Get all achievement data.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Dictionary of AchievementData mapped to its Lumina row ID.</returns>
        public static IReadOnlyDictionary<uint, AchievementData> GetAll(ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById;
        }

        /// <summary>
        /// Get an achievement by its row ID.
        /// </summary>
        /// <param name="id">Achievement row ID.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>AchievementData if found, null otherwise.</returns>
        public static AchievementData? GetById(uint id, ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById.TryGetValue(id, out var achievement) ? achievement : null;
        }

        /// <summary>
        /// Get an achievement by name.
        /// </summary>
        /// <param name="name">Achievement name to look for.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>AchievementData if found, null otherwise.</returns>
        public static AchievementData? GetByName(string name, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Array.Find(GetDataSet(language).Items, item => string.Equals(item.Name, name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Get all achievements currently marked complete on the local character.
        /// Returns an empty list if the achievement list has not been loaded yet.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Unlocked achievement data for the local character.</returns>
        public static IReadOnlyList<AchievementData> GetUnlocked(ClientLanguage? language = null)
        {
            if (!IsUnlockListLoaded)
            {
                return Array.Empty<AchievementData>();
            }

            var dataSet = GetDataSet(language);
            return Array.FindAll(dataSet.Items, item => IsUnlocked(item.Id, language));
        }

        /// <summary>
        /// Check whether a specific achievement is complete on the local character.
        /// </summary>
        /// <param name="id">Achievement row ID.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the achievement is complete and the list is loaded; otherwise false.</returns>
        public static bool IsUnlocked(uint id, ClientLanguage? language = null)
        {
            if (!IsUnlockListLoaded)
            {
                return false;
            }

            var dataSet = GetDataSet(language);
            return dataSet.RowsById.TryGetValue(id, out var row) && Service.UnlockState.IsAchievementComplete(row);
        }

        /// <summary>
        /// Check whether a specific achievement is complete on the local character.
        /// </summary>
        /// <param name="achievement">AchievementData to check.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the achievement is complete and the list is loaded; otherwise false.</returns>
        public static bool IsUnlocked(AchievementData achievement, ClientLanguage? language = null)
        {
            return IsUnlocked(achievement.Id, language);
        }

        public static void Refresh()
        {
            Cache.Clear();
        }

        private static CollectableDataSet<AchievementData, Achievement> GetDataSet(ClientLanguage? language)
        {
            return Cache.Get(ResolveLanguage(language));
        }

        private static CollectableDataSet<AchievementData, Achievement> BuildAchievements(ClientLanguage language)
        {
            var achievementSheet = Service.DataManager.GetExcelSheet<Achievement>(language);
            if (achievementSheet == null)
            {
                return new CollectableDataSet<AchievementData, Achievement>(
                    Array.Empty<AchievementData>(),
                    new Dictionary<uint, AchievementData>(),
                    new Dictionary<uint, Achievement>());
            }

            var categories = LoadCategories(language);
            var rowsById = new Dictionary<uint, Achievement>();
            var items = new List<AchievementData>();
            foreach (var row in achievementSheet)
            {
                var name = ToText(row.Name);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var categoryId = row.AchievementCategory.RowId;
                var categoryName = categories.TryGetValue(categoryId, out var resolvedCategory)
                    ? resolvedCategory
                    : string.Empty;

                rowsById[row.RowId] = row;
                items.Add(new AchievementData(
                    row.RowId,
                    name,
                    ToText(row.Description),
                    categoryId,
                    categoryName,
                    row.Order));
            }

            var orderedItems = items
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id)
                .ToArray();

            return new CollectableDataSet<AchievementData, Achievement>(
                orderedItems,
                orderedItems.ToDictionary(item => item.Id),
                rowsById);
        }

        private static Dictionary<uint, string> LoadCategories(ClientLanguage language)
        {
            var categorySheet = Service.DataManager.GetExcelSheet<AchievementCategory>(language);
            if (categorySheet == null)
            {
                return new Dictionary<uint, string>();
            }

            return categorySheet
                .Where(row => row.RowId > 0 && !string.IsNullOrWhiteSpace(ToText(row.Name)))
                .ToDictionary(row => row.RowId, row => ToText(row.Name));
        }
    }
}
