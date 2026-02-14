namespace ElezenTools.Data.Classes;

public readonly record struct DataCentreData(
    uint Id,
    string Name,
    uint RegionId,
    string RegionName,
    IReadOnlyList<uint> WorldIds);
