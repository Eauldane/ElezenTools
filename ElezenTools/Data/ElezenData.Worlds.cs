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
        var regionsById = BuildRegionsById(regions);
        dataCenters = LinkDataCenterRegions(dataCenters, regionsById);
        var dataCentersById = BuildDataCentersById(dataCenters);
        var resolvedWorlds = LinkWorldReferences(worlds, dataCentersById, regionsById);

        return new WorldDataSet(resolvedWorlds, dataCenters, regions);
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

        var dataCenter = ReadWorldDataCenter(row, dcInfoById);
        var region = ReadWorldRegion(row, dataCenter, dcInfoById);
        var dataCenterId = dataCenter?.Id ?? 0;
        var dataCenterName = dataCenter?.Name ?? string.Empty;
        var regionId = region?.Id ?? 0;
        var regionName = region?.Name ?? ResolveRegionName(regionId);

        world = new WorldData(
            row.RowId,
            name,
            dataCenterId,
            dataCenterName,
            regionId,
            regionName,
            LuminaRowReader.GetBool(row, "IsPublic"),
            LuminaRowReader.GetBool(row, "IsCloud"),
            dataCenter,
            region);

        return true;
    }

    private static DataCentreData? ReadWorldDataCenter(
        object row,
        IReadOnlyDictionary<uint, DataCenterInfo> dcInfoById)
    {
        var dataCenterId = LuminaRowReader.GetRowId(row, "DataCenter");
        if (dataCenterId == 0)
        {
            dataCenterId = LuminaRowReader.GetUInt32(row, "DataCenter");
        }

        if (dataCenterId == 0)
        {
            return null;
        }

        var dataCenterName = LuminaRowReader.GetRowName(row, "DataCenter");
        var regionId = 0u;

        if (dcInfoById.TryGetValue(dataCenterId, out var dcInfo))
        {
            if (string.IsNullOrWhiteSpace(dataCenterName))
            {
                dataCenterName = dcInfo.Name;
            }

            regionId = dcInfo.RegionId;
        }

        var region = ReadWorldRegion(row, dataCenterId, regionId);
        if (region != null)
        {
            regionId = region.Value.Id;
        }

        return new DataCentreData(
            dataCenterId,
            dataCenterName ?? string.Empty,
            regionId,
            region?.Name ?? ResolveRegionName(regionId),
            Array.Empty<uint>(),
            region);
    }

    private static RegionData? ReadWorldRegion(
        object row,
        DataCentreData? dataCenter,
        IReadOnlyDictionary<uint, DataCenterInfo> dcInfoById)
    {
        var dataCenterId = dataCenter?.Id ?? 0;
        var fallbackRegionId = dataCenter?.RegionId ?? 0;
        if (fallbackRegionId == 0 && dataCenterId != 0 && dcInfoById.TryGetValue(dataCenterId, out var dcInfo))
        {
            fallbackRegionId = dcInfo.RegionId;
        }

        return ReadWorldRegion(row, dataCenterId, fallbackRegionId);
    }

    private static RegionData? ReadWorldRegion(
        object row,
        uint dataCenterId,
        uint fallbackRegionId)
    {
        var regionId = LuminaRowReader.GetRowId(row, "Region");
        if (regionId == 0)
        {
            regionId = LuminaRowReader.GetUInt32(row, "Region");
        }

        if (regionId == 0)
        {
            regionId = fallbackRegionId;
        }

        if (regionId == 0)
        {
            return null;
        }

        var regionName = LuminaRowReader.GetRowName(row, "Region");
        regionName = ResolveRegionName(regionId, regionName);

        var linkedDataCenterIds = dataCenterId == 0
            ? Array.Empty<uint>()
            : new[] { dataCenterId };

        return new RegionData(regionId, regionName, linkedDataCenterIds, Array.Empty<uint>());
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
            RegionData? linkedRegion = regionId == 0
                ? null
                : new RegionData(regionId, regionName, new[] { dataCenterId }, worldIds);
            dataCenters.Add(new DataCentreData(dataCenterId, name, regionId, regionName, worldIds, linkedRegion));
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

    private static Dictionary<uint, DataCentreData> BuildDataCentersById(IEnumerable<DataCentreData> dataCenters)
    {
        var result = new Dictionary<uint, DataCentreData>();
        foreach (var dataCenter in dataCenters)
        {
            result[dataCenter.Id] = dataCenter;
        }

        return result;
    }

    private static Dictionary<uint, RegionData> BuildRegionsById(IEnumerable<RegionData> regions)
    {
        var result = new Dictionary<uint, RegionData>();
        foreach (var region in regions)
        {
            result[region.Id] = region;
        }

        return result;
    }

    private static DataCentreData[] LinkDataCenterRegions(
        IReadOnlyList<DataCentreData> dataCenters,
        IReadOnlyDictionary<uint, RegionData> regionsById)
    {
        var resolvedDataCenters = new DataCentreData[dataCenters.Count];
        for (var index = 0; index < dataCenters.Count; index++)
        {
            var dataCenter = dataCenters[index];
            RegionData? region = dataCenter.Region;
            if (regionsById.TryGetValue(dataCenter.RegionId, out var resolvedRegion))
            {
                region = resolvedRegion;
            }

            resolvedDataCenters[index] = dataCenter with { Region = region };
        }

        return resolvedDataCenters;
    }

    private static WorldData[] LinkWorldReferences(
        IReadOnlyList<WorldData> worlds,
        IReadOnlyDictionary<uint, DataCentreData> dataCentersById,
        IReadOnlyDictionary<uint, RegionData> regionsById)
    {
        var resolvedWorlds = new WorldData[worlds.Count];
        for (var index = 0; index < worlds.Count; index++)
        {
            var world = worlds[index];
            DataCentreData? dataCenter = world.DataCenter;
            if (dataCentersById.TryGetValue(world.DataCenterId, out var resolvedDataCenter))
            {
                dataCenter = resolvedDataCenter;
            }

            RegionData? region = world.Region;
            if (regionsById.TryGetValue(world.RegionId, out var resolvedRegion))
            {
                region = resolvedRegion;
            }

            resolvedWorlds[index] = world with
            {
                DataCenter = dataCenter,
                Region = region,
            };
        }

        return resolvedWorlds;
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
