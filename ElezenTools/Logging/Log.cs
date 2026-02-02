using ElezenTools.Services;
using Serilog.Events;
namespace ElezenTools.Logging;

public static class Log
{
    public static void Debug(string s)
    {
        Service.Log.Debug(s);
    }
    
    public static void Information(string s)
    {
        Service.Log.Information(s);
    }
    
    public static void Warning(string s)
    {
        Service.Log.Warning(s);
    }
    
    public static void Error(string s)
    {
        Service.Log.Error(s);
    }
    
    public static void Verbose(string s)
    {
        Service.Log.Verbose(s);
    }
}