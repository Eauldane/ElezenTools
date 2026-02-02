#nullable disable

using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ElezenTools.Services;

#pragma warning disable S1118
public class Service
#pragma warning restore S1118
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] public static IFramework Framework { get; private set; }
    [PluginService] public static IPlayerState PlayerState { get; private set; }
    [PluginService] public static IPluginLog Log { get; private set; }

    public static void Init(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
    }
}