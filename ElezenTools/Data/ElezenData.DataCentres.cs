using Dalamud.Game;
using ElezenTools.Data.Classes;

namespace ElezenTools.Data;

public static partial class ElezenData
{
    public static class DataCentres
    {
        public static IReadOnlyDictionary<uint, DataCentreData> All => GetAll();

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

        public static DataCentreData? GetById(uint id, ClientLanguage? language = null)
        {
            return TryGetById(id, out var dataCenter, language) ? dataCenter : null;
        }

        public static bool TryGetById(uint id, out DataCentreData dataCentre, ClientLanguage? language = null)
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

        public static DataCentreData? GetByName(string name, ClientLanguage? language = null)
        {
            return TryGetByName(name, out var dataCenter, language) ? dataCenter : null;
        }

        public static bool TryGetByName(string name, out DataCentreData dataCentre, ClientLanguage? language = null)
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
