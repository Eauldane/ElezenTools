namespace ElezenTools.Data.Classes;

public readonly record struct LocationData(
    uint TerritoryId,
    string Name,
    string AreaName,
    uint PlaceNameId,
    uint PlaceNameRegionId,
    bool IsPvpZone,
    PlaceNameData? PlaceName = null,
    PlaceNameData? PlaceNameRegion = null);
