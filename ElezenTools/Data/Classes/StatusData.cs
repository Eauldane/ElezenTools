using ElezenTools.Data.Enums;

namespace ElezenTools.Data.Classes;

public readonly record struct StatusData(
    uint Id,
    string Name,
    string Description,
    StatusType StatusType,
    bool CanDispel,
    bool IsFcBuff
    );