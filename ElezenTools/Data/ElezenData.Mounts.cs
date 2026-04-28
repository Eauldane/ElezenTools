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
    public static class Mounts
    {
        private static readonly LanguageCache<CollectableDataSet<MountData, Mount>> Cache = new(BuildMounts);

        /// <summary>
        /// Get all mount data.
        /// </summary>
        public static IReadOnlyDictionary<uint, MountData> All => GetAll();

        /// <summary>
        /// Get all mount data.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Dictionary of MountData mapped to its Lumina row ID.</returns>
        public static IReadOnlyDictionary<uint, MountData> GetAll(ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById;
        }

        /// <summary>
        /// Get a mount by its row ID.
        /// </summary>
        /// <param name="id">Mount row ID.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>MountData if found, null otherwise.</returns>
        public static MountData? GetById(uint id, ClientLanguage? language = null)
        {
            return GetDataSet(language).ItemsById.TryGetValue(id, out var mount) ? mount : null;
        }

        /// <summary>
        /// Get a mount by name.
        /// </summary>
        /// <param name="name">Mount name to look for.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>MountData if found, null otherwise.</returns>
        public static MountData? GetByName(string name, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Array.Find(GetDataSet(language).Items, item => string.Equals(item.Name, name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Get all mounts currently unlocked on the local character.
        /// Returns an empty list if no character is logged in.
        /// </summary>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>Unlocked mount data for the local character.</returns>
        public static IReadOnlyList<MountData> GetUnlocked(ClientLanguage? language = null)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                return Array.Empty<MountData>();
            }

            var dataSet = GetDataSet(language);
            return Array.FindAll(dataSet.Items, item => IsUnlocked(item.Id, language));
        }

        /// <summary>
        /// Check whether a specific mount is unlocked on the local character.
        /// </summary>
        /// <param name="id">Mount row ID.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the mount is unlocked; otherwise false.</returns>
        public static bool IsUnlocked(uint id, ClientLanguage? language = null)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                return false;
            }

            var dataSet = GetDataSet(language);
            return dataSet.RowsById.TryGetValue(id, out var row) && Service.UnlockState.IsMountUnlocked(row);
        }

        /// <summary>
        /// Check whether a specific mount is unlocked on the local character.
        /// </summary>
        /// <param name="mount">MountData to check.</param>
        /// <param name="language">Language to use for row lookup. Defaults to client language.</param>
        /// <returns>True if the mount is unlocked; otherwise false.</returns>
        public static bool IsUnlocked(MountData mount, ClientLanguage? language = null)
        {
            return IsUnlocked(mount.Id, language);
        }

        public static void Refresh()
        {
            Cache.Clear();
        }

        private static CollectableDataSet<MountData, Mount> GetDataSet(ClientLanguage? language)
        {
            return Cache.Get(ResolveLanguage(language));
        }

        private static CollectableDataSet<MountData, Mount> BuildMounts(ClientLanguage language)
        {
            var mountSheet = Service.DataManager.GetExcelSheet<Mount>(language);
            if (mountSheet == null)
            {
                return new CollectableDataSet<MountData, Mount>(
                    Array.Empty<MountData>(),
                    new Dictionary<uint, MountData>(),
                    new Dictionary<uint, Mount>());
            }

            var transientDetails = LoadTransientDetails(language);
            var rowsById = new Dictionary<uint, Mount>();
            var items = new List<MountData>();
            foreach (var row in mountSheet)
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
                items.Add(new MountData(
                    row.RowId,
                    GetDisplayName(row.Singular, language),
                    description,
                    row.IsAirborne,
                    Math.Max(1, row.ExtraSeats + 1),
                    row.Order < 0 ? short.MaxValue : row.Order));
            }

            var orderedItems = items
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new CollectableDataSet<MountData, Mount>(
                orderedItems,
                orderedItems.ToDictionary(item => item.Id),
                rowsById);
        }

        private static Dictionary<uint, MountTransient> LoadTransientDetails(ClientLanguage language)
        {
            var transientSheet = Service.DataManager.GetExcelSheet<MountTransient>(language);
            if (transientSheet == null)
            {
                return new Dictionary<uint, MountTransient>();
            }

            return transientSheet
                .Where(row => row.RowId > 0)
                .ToDictionary(row => row.RowId, row => row);
        }
    }
}
