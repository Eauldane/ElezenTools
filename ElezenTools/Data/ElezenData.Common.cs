using Dalamud.Game;
using ElezenTools.Services;

namespace ElezenTools.Data;

public static partial class ElezenData
{
    private static readonly IReadOnlyDictionary<uint, string> RegionNames = new Dictionary<uint, string>
    {
        [0] = string.Empty,
        [1] = "Japan",
        [2] = "North America",
        [3] = "Europe",
        [4] = "Oceania",
        [5] = "China",
        [6] = "Korea",
    };

    private static ClientLanguage ResolveLanguage(ClientLanguage? language)
    {
        return language ?? Service.ClientState.ClientLanguage;
        
    }

    private static string ResolveRegionName(uint regionId, string? overrideName = null)
    {
        if (!string.IsNullOrWhiteSpace(overrideName))
        {
            return overrideName;
        }

        return RegionNames.TryGetValue(regionId, out var name) ? name : $"Region {regionId}";
    }
}
