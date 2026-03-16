// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

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
