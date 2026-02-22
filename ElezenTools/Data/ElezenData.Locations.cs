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

        public static IReadOnlyDictionary<uint, LocationData> All => GetAll();

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

        public static LocationData? GetByTerritoryId(uint territoryId, ClientLanguage? language = null)
        {
            return TryGetByTerritoryId(territoryId, out var location, language) ? location : null;
        }

        public static bool TryGetByTerritoryId(uint territoryId, out LocationData location, ClientLanguage? language = null)
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

        public static LocationData? GetByName(string name, ClientLanguage? language = null)
        {
            return TryGetByName(name, out var location, language) ? location : null;
        }

        public static bool TryGetByName(string name, out LocationData location, ClientLanguage? language = null)
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
                var name = LuminaRowReader.GetRowName(row, "PlaceName");
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var areaName = LuminaRowReader.GetRowName(row, "PlaceNameRegion") ?? string.Empty;
                var placeNameId = LuminaRowReader.GetRowId(row, "PlaceName");
                var placeNameRegionId = LuminaRowReader.GetRowId(row, "PlaceNameRegion");
                var isPvpZone = LuminaRowReader.GetBool(row, "IsPvpZone");
                locations.Add(new LocationData(row.RowId, name, areaName, placeNameId, placeNameRegionId, isPvpZone));
            }

            return locations.ToArray();
        }
    }
}
