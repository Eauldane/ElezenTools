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
/// Helper record containing achievement data.
/// </summary>
/// <param name="Id">Lumina row ID for the achievement.</param>
/// <param name="Name">Achievement name.</param>
/// <param name="Description">Achievement description text.</param>
/// <param name="CategoryId">Achievement category row ID.</param>
/// <param name="CategoryName">Achievement category name.</param>
/// <param name="SortOrder">Achievement sort order.</param>
public readonly record struct AchievementData(
    uint Id,
    string Name,
    string Description,
    uint CategoryId,
    string CategoryName,
    uint SortOrder);
