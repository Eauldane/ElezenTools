namespace ElezenTools.Data.Classes;

public readonly record struct RegionData(
    uint Id,
    string Name,
    IReadOnlyList<uint> DataCenterIds,
    IReadOnlyList<uint> WorldIds);
