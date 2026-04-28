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
/// Helper record containing region data.
/// </summary>
/// <param name="Id">Lumina row ID for the region.</param>
/// <param name="Name">Name of the region - e.g. Europe.</param>
/// <param name="DataCenterIds">List of data centre row IDs contained in the region.</param>
/// <param name="WorldIds">List of world row IDs contained in the region.</param>
public readonly record struct RegionData(
    uint Id,
    string Name,
    IReadOnlyList<uint> DataCenterIds,
    IReadOnlyList<uint> WorldIds);
