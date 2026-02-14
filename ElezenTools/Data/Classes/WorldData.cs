namespace ElezenTools.Data.Classes;

public readonly record struct WorldData(
    uint Id,
    string Name,
    uint DataCenterId,
    string DataCenterName,
    uint RegionId,
    string RegionName,
    bool IsPublic,
    bool IsCloud);
