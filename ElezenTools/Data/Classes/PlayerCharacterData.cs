// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Dalamud.Game.Player;

namespace ElezenTools.Data.Classes;

/// <summary>
/// Helper record containing local player character data.
/// </summary>
/// <param name="GameObjectId">Game object ID of the local player, if the character object currently exists.</param>
/// <param name="EntityId">Entity ID of the local character.</param>
/// <param name="ContentId">Content ID of the local character.</param>
/// <param name="Address">Current memory address of the local player object, or zero if it is not spawned.</param>
/// <param name="Name">Character name.</param>
/// <param name="CurrentWorldId">Current world row ID.</param>
/// <param name="HomeWorldId">Home world row ID.</param>
/// <param name="ClassJobId">Current class or job row ID.</param>
/// <param name="RaceId">Race row ID.</param>
/// <param name="TribeId">Tribe row ID.</param>
/// <param name="Sex">Sex enum provided by Dalamud.</param>
/// <param name="Level">Current level on the active class or job.</param>
/// <param name="EffectiveLevel">Effective level after level sync is applied.</param>
/// <param name="IsLevelSynced">Whether the character is currently level synced.</param>
/// <param name="CurrentWorld">Resolved world data for the character's current world.</param>
/// <param name="HomeWorld">Resolved world data for the character's home world.</param>
/// <param name="ClassJob">Resolved job data for the character's active class or job.</param>
public readonly record struct PlayerCharacterData(
    ulong GameObjectId,
    uint EntityId,
    ulong ContentId,
    nint Address,
    string Name,
    uint CurrentWorldId,
    uint HomeWorldId,
    uint ClassJobId,
    uint RaceId,
    uint TribeId,
    Sex Sex,
    short Level,
    short EffectiveLevel,
    bool IsLevelSynced,
    WorldData? CurrentWorld = null,
    WorldData? HomeWorld = null,
    JobData? ClassJob = null);
