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
    public static class DataCentres
    {
        /// <summary>
        /// Get all DataCentre rows from Lumina. 
        /// </summary>
        public static IReadOnlyDictionary<uint, DataCentreData> All => GetAll();

        /// <summary>
        /// /// Get all DataCentre rows from Lumina. 
        /// </summary>
        /// <param name="language">ClientLanguage. Will default to the client's current language if one isn't set..</param>
        /// <returns></returns>
        public static IReadOnlyDictionary<uint, DataCentreData> GetAll(ClientLanguage? language = null)
        {
            var dataCenters = GetWorldDataSet(language).DataCenterItems;
            var result = new Dictionary<uint, DataCentreData>(dataCenters.Length);
            foreach (var dataCenter in dataCenters)
            {
                result[dataCenter.Id] = dataCenter;
            }

            return result;
        }
        
        /// <summary>
        /// Gets a single data centre by ID.
        /// </summary>
        /// <param name="id">The DC ID to retrieve.</param>
        /// <param name="language">Language to retrieve; will default to client language if not passed.</param>
        /// <returns>Out var</returns>
        public static DataCentreData? GetById(uint id, ClientLanguage? language = null)
        {
            return TryGetById(id, out var dataCenter, language) ? dataCenter : null;
        }

        private static bool TryGetById(uint id, out DataCentreData dataCentre, ClientLanguage? language = null)
        {
            var dataCenters = GetWorldDataSet(language).DataCenterItems;
            var index = Array.FindIndex(dataCenters, item => item.Id == id);
            if (index >= 0)
            {
                dataCentre = dataCenters[index];
                return true;
            }

            dataCentre = default;
            return false;
        }

        /// <summary>
        /// Try and get a datacentre by name. This may cause issues if a language isn't passed in! 
        /// </summary>
        /// <param name="name">The name to get.</param>
        /// <param name="language">The language to use. Technically optional, but strongly recommended!</param>
        /// <returns></returns>
        public static DataCentreData? GetByName(string name, ClientLanguage? language = null)
        {
            return TryGetByName(name, out var dataCenter, language) ? dataCenter : null;
        }

        private static bool TryGetByName(string name, out DataCentreData dataCentre, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                dataCentre = default;
                return false;
            }

            var dataCenters = GetWorldDataSet(language).DataCenterItems;
            var index = Array.FindIndex(dataCenters, item => string.Equals(item.Name, name, StringComparison.Ordinal));
            if (index >= 0)
            {
                dataCentre = dataCenters[index];
                return true;
            }

            dataCentre = default;
            return false;
        }

        public static void Refresh()
        {
            WorldCache.Clear();
        }
    }
}
