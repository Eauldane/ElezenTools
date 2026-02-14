using Dalamud.Game;
using ElezenTools.Data.Classes;

namespace ElezenTools.Data;

public static partial class ElezenData
{
    public static class Regions
    {
        public static IReadOnlyDictionary<uint, RegionData> All => GetAll();

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

        public static RegionData? GetById(uint id, ClientLanguage? language = null)
        {
            return TryGetById(id, out var region, language) ? region : null;
        }

        public static bool TryGetById(uint id, out RegionData region, ClientLanguage? language = null)
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

        public static RegionData? GetByName(string name, ClientLanguage? language = null)
        {
            return TryGetByName(name, out var region, language) ? region : null;
        }

        public static bool TryGetByName(string name, out RegionData region, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                region = default;
                return false;
            }

            var regions = GetWorldDataSet(language).RegionItems;
            var index = Array.FindIndex(regions, item => string.Equals(item.Name, name, StringComparison.Ordinal));
            if (index >= 0)
            {
                region = regions[index];
                return true;
            }

            region = default;
            return false;
        }

        public static void Refresh()
        {
            WorldCache.Clear();
        }
    }
}
