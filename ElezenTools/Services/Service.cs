#nullable disable

using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ElezenTools.Services;

public sealed class Service
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] public static IFramework Framework { get; private set; }
    [PluginService] public static IPlayerState PlayerState { get; private set; }
    [PluginService] public static IPluginLog Log { get; private set; }

    private void Init()
    {
        
    }
    public static void Init(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        
    }
}