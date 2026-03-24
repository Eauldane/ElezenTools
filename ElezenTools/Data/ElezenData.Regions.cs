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

namespace ElezenTools.Data;

public static partial class ElezenData
{
    public static class Regions
    {
        /// <summary>
        /// Load all Region data.
        /// </summary>
        public static IReadOnlyDictionary<uint, RegionData> All => GetAll();

        /// <summary>
        /// Get all region data for a given language.
        /// </summary>
        /// <param name="language">Language to get. Defaults to client language.</param>
        /// <returns>Dictionary of RegionData objects mapped to their row ID.</returns>
        public static IReadOnlyDictionary<uint, RegionData> GetAll(ClientLanguage? language = null)
        {
            var regions = GetWorldDataSet(language).RegionItems;
            var result = new Dictionary<uint, RegionData>(regions.Length);
            foreach (var region in regions)
            {
                result[region.Id] = region;
            }

            return result;
        }

        /// <summary>
        ///  Get RegionData by its ID.
        /// </summary>
        /// <param name="id">ID of the region to get</param>
        /// <param name="language">Language to use, defaulting to client language.</param>
        /// <returns></returns>
        public static RegionData? GetById(uint id, ClientLanguage? language = null)
        {
            return TryGetById(id, out var region, language) ? region : null;
        }

        private static bool TryGetById(uint id, out RegionData region, ClientLanguage? language = null)
        {
            var regions = GetWorldDataSet(language).RegionItems;
            var index = Array.FindIndex(regions, item => item.Id == id);
            if (index >= 0)
            {
                region = regions[index];
                return true;
            }

            region = default;
            return false;
        }

        /// <summary>
        /// Get a region by its name. Returns null if the name is ambiguous.
        /// </summary>
        /// <param name="name">Name of the region to get.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>RegionData object if a single match is found, null otherwise.</returns>
        public static RegionData? GetByName(string name, ClientLanguage? language = null)
        {
            return TryGetByName(name, out var region, language) ? region : null;
        }

        /// <summary>
        /// Find all regions that share the same name.
        /// </summary>
        /// <param name="name">Name of the region to get.</param>
        /// <param name="language">Language to use. Defaults to client language.</param>
        /// <returns>All matching RegionData rows.</returns>
        public static IReadOnlyList<RegionData> FindByName(string name, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Array.Empty<RegionData>();
            }

            var regions = GetWorldDataSet(language).RegionItems;
            return Array.FindAll(regions, item => string.Equals(item.Name, name, StringComparison.Ordinal));
        }

        private static bool TryGetByName(string name, out RegionData region, ClientLanguage? language = null)
        {
            var matches = FindByName(name, language);
            if (matches.Count != 1)
            {
                region = default;
                return false;
            }

            region = matches[0];
            return true;
        }

        public static void Refresh()
        {
            WorldCache.Clear();
        }
    }
}
