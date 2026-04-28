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
using ElezenTools.Data.Classes;
using ElezenTools.Data.Internal;
using ElezenTools.Services;

namespace ElezenTools.Data;

public static partial class ElezenData
{
    public static class Players
    {
        /// <summary>
        /// Get data for the local player character.
        /// </summary>
        /// <param name="language">Language to use for related world and job data. Defaults to client language.</param>
        /// <returns>PlayerCharacterData for the local player, or null if character data is not loaded.</returns>
        public static PlayerCharacterData? GetLocalPlayer(ClientLanguage? language = null)
        {
            var playerState = Service.PlayerState;
            if (!playerState.IsLoaded)
            {
                return null;
            }

            var currentWorldId = LuminaRowReader.GetRowId(playerState, "CurrentWorld");
            var homeWorldId = LuminaRowReader.GetRowId(playerState, "HomeWorld");
            var classJobId = LuminaRowReader.GetRowId(playerState, "ClassJob");
            var raceId = LuminaRowReader.GetRowId(playerState, "Race");
            var tribeId = LuminaRowReader.GetRowId(playerState, "Tribe");
            var localPlayer = Service.ObjectTable.LocalPlayer;

            return new PlayerCharacterData(
                localPlayer?.GameObjectId ?? 0,
                playerState.EntityId,
                playerState.ContentId,
                localPlayer?.Address ?? nint.Zero,
                playerState.CharacterName ?? string.Empty,
                currentWorldId,
                homeWorldId,
                classJobId,
                raceId,
                tribeId,
                playerState.Sex,
                playerState.Level,
                playerState.EffectiveLevel,
                playerState.IsLevelSynced,
                Worlds.GetById(currentWorldId, language),
                Worlds.GetById(homeWorldId, language),
                Jobs.GetById(classJobId, language));
        }
    }
}
