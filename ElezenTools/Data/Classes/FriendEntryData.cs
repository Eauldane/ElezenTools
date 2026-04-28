// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace ElezenTools.Data.Classes;

/// <summary>
/// Helper record containing friend list entry data.
/// </summary>
/// <param name="Name">Character name shown in the friend list.</param>
/// <param name="HomeWorldId">Home world row ID for the friend.</param>
/// <param name="HomeWorldName">Home world name for the friend.</param>
/// <param name="CurrentWorldId">Current world row ID for the friend, if available.</param>
/// <param name="CurrentWorldName">Current world name for the friend, if available.</param>
/// <param name="IsVisiting">Whether the friend is currently visiting another world.</param>
/// <param name="WorldLabel">Convenience label combining home and current world names, for example "Shiva (visiting Twintania)".</param>
/// <param name="ContentId">Content ID read from the friend list entry.</param>
/// <param name="HomeWorld">Resolved WorldData for the friend's home world.</param>
/// <param name="CurrentWorld">Resolved WorldData for the friend's current world, if available.</param>
public readonly record struct FriendEntryData(
    string Name,
    uint HomeWorldId,
    string HomeWorldName,
    uint CurrentWorldId,
    string CurrentWorldName,
    bool IsVisiting,
    string WorldLabel,
    ulong ContentId,
    WorldData? HomeWorld = null,
    WorldData? CurrentWorld = null);
