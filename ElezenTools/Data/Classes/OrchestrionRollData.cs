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
/// Helper record containing orchestrion roll data.
/// </summary>
/// <param name="Id">Lumina row ID for the orchestrion roll.</param>
/// <param name="Name">Orchestrion roll name.</param>
/// <param name="Description">Orchestrion roll description text.</param>
/// <param name="CategoryId">Orchestrion category row ID.</param>
/// <param name="CategoryName">Orchestrion category name.</param>
/// <param name="SortOrder">Orchestrion roll sort order.</param>
public readonly record struct OrchestrionRollData(
    uint Id,
    string Name,
    string Description,
    uint CategoryId,
    string CategoryName,
    uint SortOrder);
