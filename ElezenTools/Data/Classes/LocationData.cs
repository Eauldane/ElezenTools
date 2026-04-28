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
/// Helper record containing location data for a territory.
/// </summary>
/// <param name="TerritoryId">Territory row ID.</param>
/// <param name="Name">Name of the location - e.g. Old Gridania.</param>
/// <param name="AreaName">Name of the wider area or region, if available.</param>
/// <param name="PlaceNameId">PlaceName row ID for the location's name.</param>
/// <param name="PlaceNameRegionId">PlaceName row ID for the wider area or region name.</param>
/// <param name="IsPvpZone">Whether the territory is flagged as a PvP zone.</param>
/// <param name="PlaceName">Resolved PlaceNameData object for the location name.</param>
/// <param name="PlaceNameRegion">Resolved PlaceNameData object for the wider area or region name.</param>
public readonly record struct LocationData(
    uint TerritoryId,
    string Name,
    string AreaName,
    uint PlaceNameId,
    uint PlaceNameRegionId,
    bool IsPvpZone,
    PlaceNameData? PlaceName = null,
    PlaceNameData? PlaceNameRegion = null);
