using System.Numerics;

namespace ElezenTools.Data.Classes;

public readonly record struct JobData(
    uint Id,
    string Name,
    string Abbreviation,
    ElezenTools.Data.Enums.JobRole Role,
    bool IsLimited,
    JobCategoryData? Category,
    JobParentData? Parent,
    uint IconId,
    int SortOrder,
    Vector4 ClassColour,
    TownData? StartingTown
    );
