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
/// Helper record containing mount data.
/// </summary>
/// <param name="Id">Lumina row ID for the mount.</param>
/// <param name="Name">Mount name.</param>
/// <param name="Description">Mount description text.</param>
/// <param name="IsAirborne">Whether the mount can fly.</param>
/// <param name="SeatCount">Total seat count for the mount.</param>
/// <param name="SortOrder">Mount sort order.</param>
public readonly record struct MountData(
    uint Id,
    string Name,
    string Description,
    bool IsAirborne,
    int SeatCount,
    int SortOrder);
