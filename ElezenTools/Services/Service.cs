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