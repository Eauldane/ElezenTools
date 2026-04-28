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
    public static class OrchestrionRolls
    {
        private static readonly LanguageCache<CollectableDataSet<OrchestrionRollData, Orchestrion>> Cache = new(BuildRolls);

        /// <summary>
        /// Get all orchestrion roll data.
        /// </summary>
        public static IReadOnlyDictionary<uint, OrchestrionRollData> All => GetAll();

        /// <summary>
        /// Get all orchestrion roll data.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Dictionary of OrchestrionRollData mapped to its Lumina row ID.</returns>
        public static IReadOnlyDictionary<uint, OrchestrionRollData> GetAll(ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById;
        }

        /// <summary>
        /// Get an orchestrion roll by its row ID.
        /// </summary>
        /// <param name="id">Orchestrion roll row ID.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>OrchestrionRollData if found, null otherwise.</returns>
        public static OrchestrionRollData? GetById(uint id, ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById.TryGetValue(id, out var roll) ? roll : null;
        }

        /// <summary>
        /// Get an orchestrion roll by name.
        /// </summary>
        /// <param name="name">Orchestrion roll name to look for.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>OrchestrionRollData if found, null otherwise.</returns>
        public static OrchestrionRollData? GetByName(string name, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Array.Find(GetDataSet(language).Items, item => string.Equals(item.Name, name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Get all orchestrion rolls currently unlocked on the local character.
        /// Returns an empty list if no character is logged in.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Unlocked orchestrion roll data for the local character.</returns>
        public static IReadOnlyList<OrchestrionRollData> GetUnlocked(ClientLanguage? language = null)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                return Array.Empty<OrchestrionRollData>();
            }

            var dataSet = GetDataSet(language);
            return Array.FindAll(dataSet.Items, item => IsUnlocked(item.Id, language));
        }

        /// <summary>
        /// Check whether a specific orchestrion roll is unlocked on the local character.
        /// </summary>
        /// <param name="id">Orchestrion roll row ID.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the orchestrion roll is unlocked; otherwise false.</returns>
        public static bool IsUnlocked(uint id, ClientLanguage? language = null)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                return false;
            }

            var dataSet = GetDataSet(language);
            return dataSet.RowsById.TryGetValue(id, out var row) && Service.UnlockState.IsOrchestrionUnlocked(row);
        }

        /// <summary>
        /// Check whether a specific orchestrion roll is unlocked on the local character.
        /// </summary>
        /// <param name="roll">OrchestrionRollData to check.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the orchestrion roll is unlocked; otherwise false.</returns>
        public static bool IsUnlocked(OrchestrionRollData roll, ClientLanguage? language = null)
        {
            return IsUnlocked(roll.Id, language);
        }

        public static void Refresh()
        {
            Cache.Clear();
        }

        private static CollectableDataSet<OrchestrionRollData, Orchestrion> GetDataSet(ClientLanguage? language)
        {
            return Cache.Get(ResolveLanguage(language));
        }

        private static CollectableDataSet<OrchestrionRollData, Orchestrion> BuildRolls(ClientLanguage language)
        {
            var rollSheet = Service.DataManager.GetExcelSheet<Orchestrion>(language);
            if (rollSheet == null)
            {
                return new CollectableDataSet<OrchestrionRollData, Orchestrion>(
                    Array.Empty<OrchestrionRollData>(),
                    new Dictionary<uint, OrchestrionRollData>(),
                    new Dictionary<uint, Orchestrion>());
            }

            var uiParams = LoadUiParams(language);
            var categories = LoadCategories(language);
            var rowsById = new Dictionary<uint, Orchestrion>();
            var items = new List<OrchestrionRollData>();
            foreach (var row in rollSheet)
            {
                var name = ToText(row.Name);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var hasUiParam = uiParams.TryGetValue(row.RowId, out var uiParam);
                var categoryId = hasUiParam ? uiParam.OrchestrionCategory.RowId : 0;
                var categoryName = categoryId != 0 && categories.TryGetValue(categoryId, out var resolvedCategory)
                    ? resolvedCategory
                    : string.Empty;

                rowsById[row.RowId] = row;
                items.Add(new OrchestrionRollData(
                    row.RowId,
                    name,
                    ToText(row.Description),
                    categoryId,
                    categoryName,
                    hasUiParam ? uiParam.Order : ushort.MaxValue));
            }

            var orderedItems = items
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new CollectableDataSet<OrchestrionRollData, Orchestrion>(
                orderedItems,
                orderedItems.ToDictionary(item => item.Id),
                rowsById);
        }

        private static Dictionary<uint, OrchestrionUiparam> LoadUiParams(ClientLanguage language)
        {
            var uiParamSheet = Service.DataManager.GetExcelSheet<OrchestrionUiparam>(language);
            if (uiParamSheet == null)
            {
                return new Dictionary<uint, OrchestrionUiparam>();
            }

            return uiParamSheet
                .Where(row => row.RowId > 0)
                .ToDictionary(row => row.RowId, row => row);
        }

        private static Dictionary<uint, string> LoadCategories(ClientLanguage language)
        {
            var categorySheet = Service.DataManager.GetExcelSheet<OrchestrionCategory>(language);
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
