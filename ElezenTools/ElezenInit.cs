// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

#nullable disable
using Serilog.Events;
using Dalamud.Plugin;
using ElezenTools.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Microsoft.VisualBasic;

namespace ElezenTools;

public static class ElezenInit
{
    public static bool Disposed { get; private set; } = false;
   
    public static void Init(IDalamudPluginInterface pluginInterface, IDalamudPlugin instance)
    {
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
    }
}