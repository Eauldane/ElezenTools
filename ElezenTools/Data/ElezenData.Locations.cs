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
    public static class Locations
    {
        private static readonly LanguageCache<LocationData[]> Cache = new(BuildLocations);

        /// <summary>
        /// Get all location data.
        /// </summary>
        public static IReadOnlyDictionary<uint, LocationData> All => GetAll();

        /// <summary>
        /// Get all location data for a given language.
        /// </summary>
        /// <param name="language">Client language to use. Defaults to player's language if not set.</param>
        /// <returns>Dictionary of all location data, mapped to the territory ID.</returns>
        public static IReadOnlyDictionary<uint, LocationData> GetAll(ClientLanguage? language = null)
        {
            var locations = GetLocations(language);
            var result = new Dictionary<uint, LocationData>(locations.Length);
            foreach (var location in locations)
            {
                result[location.TerritoryId] = location;
            }

            return result;
        }

        /// <summary>
        /// Get territory data by ID. 
        /// </summary>
        /// <param name="territoryId">ID of territory to get.</param>
        /// <param name="language">Language to use, defaults to client language.</param>
        /// <returns>LocationData object for the territory; or null if not found.</returns>
        public static LocationData? GetByTerritoryId(uint territoryId, ClientLanguage? language = null)
        {
            return TryGetByTerritoryId(territoryId, out var location, language) ? location : null;
        }

        private static bool TryGetByTerritoryId(uint territoryId, out LocationData location, ClientLanguage? language = null)
        {
            var locations = GetLocations(language);
            var index = Array.FindIndex(locations, item => item.TerritoryId == territoryId);
            if (index >= 0)
            {
                location = locations[index];
                return true;
            }

            location = default;
            return false;
        }

        /// <summary>
        /// Get territory data by name. Risky if you don't set a client language! 
        /// </summary>
        /// <param name="name">Name of the territory to look for.</param>
        /// <param name="language">Language to look for it in; defaulting to the client language.</param>
        /// <returns>LocationData object if found, null otherwise.</returns>
        public static LocationData? GetByName(string name, ClientLanguage? language = null)
        {
            return TryGetByName(name, out var location, language) ? location : null;
        }

        private static bool TryGetByName(string name, out LocationData location, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                location = default;
                return false;
            }

            var locations = GetLocations(language);
            var index = Array.FindIndex(locations, item => string.Equals(item.Name, name, StringComparison.Ordinal));
            if (index >= 0)
            {
                location = locations[index];
                return true;
            }

            location = default;
            return false;
        }

        public static void Refresh()
        {
            Cache.Clear();
        }

        private static LocationData[] GetLocations(ClientLanguage? language)
        {
            return Cache.Get(ResolveLanguage(language));
        }

        private static LocationData[] BuildLocations(ClientLanguage language)
        {
            var sheet = Service.DataManager.GetExcelSheet<TerritoryType>(language);
            if (sheet == null)
            {
                return Array.Empty<LocationData>();
            }

            var locations = new List<LocationData>();
            foreach (var row in sheet)
            {
                var placeName = ReadPlaceName(row);
                if (placeName == null || string.IsNullOrWhiteSpace(placeName.Value.Name))
                {
                    continue;
                }

                var placeNameRegion = ReadPlaceNameRegion(row);
                var isPvpZone = LuminaRowReader.GetBool(row, "IsPvpZone");
                locations.Add(new LocationData(
                    row.RowId,
                    placeName.Value.Name,
                    placeNameRegion?.Name ?? string.Empty,
                    placeName.Value.Id,
                    placeNameRegion?.Id ?? 0,
                    isPvpZone,
                    placeName,
                    placeNameRegion));
            }

            return locations.ToArray();
        }

        private static PlaceNameData? ReadPlaceName(object row)
        {
            var placeNameId = LuminaRowReader.GetRowId(row, "PlaceName");
            if (placeNameId == 0)
            {
                return null;
            }

            var placeName = LuminaRowReader.GetRowName(row, "PlaceName") ?? string.Empty;
            return new PlaceNameData(placeNameId, placeName);
        }

        private static PlaceNameData? ReadPlaceNameRegion(object row)
        {
            var placeNameRegionId = LuminaRowReader.GetRowId(row, "PlaceNameRegion");
            if (placeNameRegionId == 0)
            {
                return null;
            }

            var placeNameRegion = LuminaRowReader.GetRowName(row, "PlaceNameRegion") ?? string.Empty;
            return new PlaceNameData(placeNameRegionId, placeNameRegion);
        }

        public static LocationData? GetCurrentLocation()
        {
            return GetByTerritoryId(Service.ClientState.TerritoryType);
        }

        
    }
}
