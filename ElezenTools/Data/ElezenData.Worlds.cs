using Dalamud.Game;
using ElezenTools.Data.Classes;
using ElezenTools.Data.Internal;
using ElezenTools.Services;
using Lumina.Excel.Sheets;

namespace ElezenTools.Data;

public static partial class ElezenData
{
    private static readonly LanguageCache<WorldDataSet> WorldCache = new(BuildWorldDataSet);

    public static class Worlds
    {
        public static IReadOnlyDictionary<uint, WorldData> All => GetAll();

        public static IReadOnlyDictionary<uint, WorldData> GetAll(ClientLanguage? language = null)
        {
            var worlds = GetWorldDataSet(language).WorldItems;
            var result = new Dictionary<uint, WorldData>(worlds.Length);
            foreach (var world in worlds)
            {
                result[world.Id] = world;
            }

            return result;
        }

        public static WorldData? GetById(uint id, ClientLanguage? language = null)
        {
            return TryGetById(id, out var world, language) ? world : null;
        }

        public static bool TryGetById(uint id, out WorldData world, ClientLanguage? language = null)
        {
            var worlds = GetWorldDataSet(language).WorldItems;
            var index = Array.FindIndex(worlds, item => item.Id == id);
            if (index >= 0)
            {
                world = worlds[index];
                return true;
            }

            world = default;
            return false;
        }

        public static WorldData? GetByName(string name, ClientLanguage? language = null)
        {
            return TryGetByName(name, out var world, language) ? world : null;
        }

        public static bool TryGetByName(string name, out WorldData world, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                world = default;
                return false;
            }

            var worlds = GetWorldDataSet(language).WorldItems;
            var index = Array.FindIndex(worlds, item => string.Equals(item.Name, name, StringComparison.Ordinal));
            if (index >= 0)
            {
                world = worlds[index];
                return true;
            }

            world = default;
            return false;
        }

        public static void Refresh()
        {
            WorldCache.Clear();
        }
    }

    private static WorldDataSet GetWorldDataSet(ClientLanguage? language)
    {
        return WorldCache.Get(ResolveLanguage(language));
    }

    private static WorldDataSet BuildWorldDataSet(ClientLanguage language)
    {
        var sheet = Service.DataManager.GetExcelSheet<World>(language);
        if (sheet == null)
        {
            return new WorldDataSet(Array.Empty<WorldData>(), Array.Empty<DataCentreData>(), Array.Empty<RegionData>());
        }

        var dcInfoById = BuildDataCenterInfo(Service.DataManager.GetExcelSheet<WorldDCGroupType>(language));
        var worlds = new List<WorldData>();
        var worldsByDataCenter = new Dictionary<uint, List<WorldData>>();
        var worldsByRegion = new Dictionary<uint, List<WorldData>>();

        foreach (var row in sheet)
        {
            if (!TryBuildWorldData(row, dcInfoById, out var world))
            {
                continue;
            }

            worlds.Add(world);
            AddWorldToGrouping(worldsByDataCenter, world.DataCenterId, world);
            AddWorldToGrouping(worldsByRegion, world.RegionId, world);
        }

        var dataCenters = BuildDataCenters(worldsByDataCenter, dcInfoById);
        var regions = BuildRegions(worldsByRegion, dataCenters);

        return new WorldDataSet(worlds.ToArray(), dataCenters, regions);
    }

    private static Dictionary<uint, DataCenterInfo> BuildDataCenterInfo(IEnumerable<WorldDCGroupType>? dcSheet)
    {
        var dcInfoById = new Dictionary<uint, DataCenterInfo>();
        if (dcSheet == null)
        {
            return dcInfoById;
        }

        foreach (var row in dcSheet)
        {
            var name = LuminaRowReader.GetString(row, "Name");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var regionId = LuminaRowReader.GetRowId(row, "Region");
            if (regionId == 0)
            {
                regionId = LuminaRowReader.GetUInt32(row, "Region");
            }

            dcInfoById[row.RowId] = new DataCenterInfo(row.RowId, name, regionId);
        }

        return dcInfoById;
    }

    private static bool TryBuildWorldData(
        World row,
        IReadOnlyDictionary<uint, DataCenterInfo> dcInfoById,
        out WorldData world)
    {
        world = default;
        var name = LuminaRowReader.GetString(row, "Name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var dataCenterId = LuminaRowReader.GetRowId(row, "DataCenter");
        if (dataCenterId == 0)
        {
            dataCenterId = LuminaRowReader.GetUInt32(row, "DataCenter");
        }

        var dataCenterName = LuminaRowReader.GetRowName(row, "DataCenter");
        if (string.IsNullOrWhiteSpace(dataCenterName) && dcInfoById.TryGetValue(dataCenterId, out var dcInfo))
        {
            dataCenterName = dcInfo.Name;
        }

        var regionId = LuminaRowReader.GetRowId(row, "Region");
        if (regionId == 0)
        {
            regionId = LuminaRowReader.GetUInt32(row, "Region");
        }

        if (regionId == 0 && dcInfoById.TryGetValue(dataCenterId, out var dcInfoForRegion))
        {
            regionId = dcInfoForRegion.RegionId;
        }

        var regionName = LuminaRowReader.GetRowName(row, "Region");
        regionName = ResolveRegionName(regionId, regionName);

        world = new WorldData(
            row.RowId,
            name,
            dataCenterId,
            dataCenterName ?? string.Empty,
            regionId,
            regionName,
            LuminaRowReader.GetBool(row, "IsPublic"),
            LuminaRowReader.GetBool(row, "IsCloud"));

        return true;
    }

    private static void AddWorldToGrouping(
        IDictionary<uint, List<WorldData>> grouping,
        uint key,
        WorldData world)
    {
        if (key == 0)
        {
            return;
        }

        if (!grouping.TryGetValue(key, out var worldIds))
        {
            worldIds = new List<WorldData>();
            grouping.Add(key, worldIds);
        }

        worldIds.Add(world);
    }

    private static DataCentreData[] BuildDataCenters(
        IReadOnlyDictionary<uint, List<WorldData>> worldsByDataCenter,
        IReadOnlyDictionary<uint, DataCenterInfo> dcInfoById)
    {
        var dataCenters = new List<DataCentreData>(worldsByDataCenter.Count);
        foreach (var (dataCenterId, worldGroup) in worldsByDataCenter)
        {
            dcInfoById.TryGetValue(dataCenterId, out var info);
            var name = info?.Name ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = worldGroup[0].DataCenterName;
            }

            var regionId = info?.RegionId ?? 0;
            if (regionId == 0)
            {
                regionId = worldGroup[0].RegionId;
            }

            var regionName = ResolveRegionName(regionId);
            var worldIds = worldGroup.Select(world => world.Id).ToArray();
            dataCenters.Add(new DataCentreData(dataCenterId, name, regionId, regionName, worldIds));
        }

        return dataCenters.ToArray();
    }

    private static RegionData[] BuildRegions(
        IReadOnlyDictionary<uint, List<WorldData>> worldsByRegion,
        IReadOnlyList<DataCentreData> dataCenters)
    {
        var regions = new List<RegionData>(worldsByRegion.Count);
        foreach (var (regionId, worldGroup) in worldsByRegion)
        {
            var regionName = ResolveRegionName(regionId);
            var worldIds = worldGroup.Select(world => world.Id).ToArray();
            var dataCenterIds = dataCenters
                .Where(dataCenter => dataCenter.RegionId == regionId)
                .Select(dataCenter => dataCenter.Id)
                .ToArray();

            regions.Add(new RegionData(regionId, regionName, dataCenterIds, worldIds));
        }

        return regions.ToArray();
    }

    private sealed record DataCenterInfo(uint Id, string Name, uint RegionId);

    private sealed class WorldDataSet
    {
        public WorldDataSet(WorldData[] worlds, DataCentreData[] dataCenters, RegionData[] regions)
        {
            WorldItems = worlds;
            DataCenterItems = dataCenters;
            RegionItems = regions;
        }

        public WorldData[] WorldItems { get; }
        public DataCentreData[] DataCenterItems { get; }
        public RegionData[] RegionItems { get; }
    }
}
