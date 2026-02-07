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

using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ElezenTools.Services;

#pragma warning disable S1118
public class Service
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] public static IFramework Framework { get; private set; }
    [PluginService] public static IPlayerState PlayerState { get; private set; }
    [PluginService] public static IPluginLog Log { get; private set; }
    [PluginService] public static IObjectTable ObjectTable { get; private set; }

    public static void Init(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
    }

    public static async Task UseFramework(System.Action action)
    {
        Log.Information("Framework Event {0}", action.GetType().Name);

        if (!Framework.IsInFrameworkUpdateThread)
        {
            await Framework.RunOnFrameworkThread(action).ContinueWith((_) => Task.CompletedTask).ConfigureAwait(false);
            while (Framework.IsInFrameworkUpdateThread) // yield the thread again, should technically never be triggered
            {
                await Task.Delay(1).ConfigureAwait(false);
            }
        }
        else
        {
            action();
        }
    }
    
    public static async Task<T> UseFramework<T>(Func<T> func)
    {
        if (!Framework.IsInFrameworkUpdateThread)
        {
            Log.Information("Framework Event {0}", typeof(T).Name);
            var result = await Framework.RunOnFrameworkThread(func).ContinueWith((task) => task.Result).ConfigureAwait(false);
            while (Framework.IsInFrameworkUpdateThread) // yield the thread again, should technically never be triggered
            {
                await Task.Delay(1).ConfigureAwait(false);
            }
            return result;
        }
        Log.Information("Framework Event - off thread {0}", typeof(T).Name);

        return func.Invoke();
    }
}