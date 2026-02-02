#nullable disable
using Serilog.Events;
using Dalamud.Plugin;
using ElezenTools.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Microsoft.VisualBasic;

namespace ElezenTools;

public static class ElezenInit
{
    public static IDalamudPlugin Instance = null;
    public static bool Disposed { get; private set; } = false;
   
    public static void Init(IDalamudPluginInterface pluginInterface, IDalamudPlugin instance)
    {
        Instance = instance;
        try
        {
            Service.Init(pluginInterface);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        Service.Log.Info($"ElezenTools initialised! We were loaded by {Service.PluginInterface.InternalName} version {instance.GetType().Assembly.GetName().Version}.");
        Service.Log.MinimumLogLevel = LogEventLevel.Information;
    }

    public static void Dispose()
    {
        Disposed = true;
        Instance = null;
    }
}