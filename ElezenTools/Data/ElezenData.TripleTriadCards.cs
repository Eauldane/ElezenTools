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
    public static class TripleTriadCards
    {
        private static readonly LanguageCache<CollectableDataSet<TripleTriadCardData, TripleTriadCard>> Cache = new(BuildCards);

        /// <summary>
        /// Get all Triple Triad card data.
        /// </summary>
        public static IReadOnlyDictionary<uint, TripleTriadCardData> All => GetAll();

        /// <summary>
        /// Get all Triple Triad card data.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Dictionary of TripleTriadCardData mapped to its Lumina row ID.</returns>
        public static IReadOnlyDictionary<uint, TripleTriadCardData> GetAll(ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById;
        }

        /// <summary>
        /// Get a Triple Triad card by its row ID.
        /// </summary>
        /// <param name="id">Card row ID.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>TripleTriadCardData if found, null otherwise.</returns>
        public static TripleTriadCardData? GetById(uint id, ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById.TryGetValue(id, out var card) ? card : null;
        }

        /// <summary>
        /// Get a Triple Triad card by name.
        /// </summary>
        /// <param name="name">Card name to look for.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>TripleTriadCardData if found, null otherwise.</returns>
        public static TripleTriadCardData? GetByName(string name, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Array.Find(GetDataSet(language).Items, item => string.Equals(item.Name, name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Get all Triple Triad cards currently unlocked on the local character.
        /// Returns an empty list if no character is logged in.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Unlocked card data for the local character.</returns>
        public static IReadOnlyList<TripleTriadCardData> GetUnlocked(ClientLanguage? language = null)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                return Array.Empty<TripleTriadCardData>();
            }

            var dataSet = GetDataSet(language);
            return Array.FindAll(dataSet.Items, item => IsUnlocked(item.Id, language));
        }

        /// <summary>
        /// Check whether a specific Triple Triad card is unlocked on the local character.
        /// </summary>
        /// <param name="id">Card row ID.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the card is unlocked; otherwise false.</returns>
        public static bool IsUnlocked(uint id, ClientLanguage? language = null)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                return false;
            }

            var dataSet = GetDataSet(language);
            return dataSet.RowsById.TryGetValue(id, out var row) && Service.UnlockState.IsTripleTriadCardUnlocked(row);
        }

        /// <summary>
        /// Check whether a specific Triple Triad card is unlocked on the local character.
        /// </summary>
        /// <param name="card">TripleTriadCardData to check.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the card is unlocked; otherwise false.</returns>
        public static bool IsUnlocked(TripleTriadCardData card, ClientLanguage? language = null)
        {
            return IsUnlocked(card.Id, language);
        }

        public static void Refresh()
        {
            Cache.Clear();
        }

        private static CollectableDataSet<TripleTriadCardData, TripleTriadCard> GetDataSet(ClientLanguage? language)
        {
            return Cache.Get(ResolveLanguage(language));
        }

        private static CollectableDataSet<TripleTriadCardData, TripleTriadCard> BuildCards(ClientLanguage language)
        {
            var cardSheet = Service.DataManager.GetExcelSheet<TripleTriadCard>(language);
            if (cardSheet == null)
            {
                return new CollectableDataSet<TripleTriadCardData, TripleTriadCard>(
                    Array.Empty<TripleTriadCardData>(),
                    new Dictionary<uint, TripleTriadCardData>(),
                    new Dictionary<uint, TripleTriadCard>());
            }

            var residentDetails = LoadResidents(language);
            var rowsById = new Dictionary<uint, TripleTriadCard>();
            var items = new List<TripleTriadCardData>();
            foreach (var row in cardSheet)
            {
                var name = ToText(row.Name);
                if (row.RowId == 0 || string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var hasResident = residentDetails.TryGetValue(row.RowId, out var resident);
                rowsById[row.RowId] = row;
                items.Add(new TripleTriadCardData(
                    row.RowId,
                    name,
                    ToText(row.Description),
                    hasResident ? resident.TripleTriadCardRarity.RowId : 0,
                    hasResident ? resident.Top : 0,
                    hasResident ? resident.Right : 0,
                    hasResident ? resident.Bottom : 0,
                    hasResident ? resident.Left : 0,
                    hasResident ? resident.Order : ushort.MaxValue));
            }

            var orderedItems = items
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new CollectableDataSet<TripleTriadCardData, TripleTriadCard>(
                orderedItems,
                orderedItems.ToDictionary(item => item.Id),
                rowsById);
        }

        private static Dictionary<uint, TripleTriadCardResident> LoadResidents(ClientLanguage language)
        {
            var residentSheet = Service.DataManager.GetExcelSheet<TripleTriadCardResident>(language);
            if (residentSheet == null)
            {
                return new Dictionary<uint, TripleTriadCardResident>();
            }

            return residentSheet
                .Where(row => row.RowId > 0)
                .ToDictionary(row => row.RowId, row => row);
        }
    }
}
