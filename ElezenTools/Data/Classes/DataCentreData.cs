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
/// Helper class representing facts about a given data centre.
/// </summary>
/// <param name="Id">Lumina Row ID for the data centre.</param>
/// <param name="Name">The name of the data centre - e.g. Crystal, Aether, etc.</param>
/// <param name="RegionId">The region ID the data centre is part of. Prefer using the Region object.</param>
/// <param name="RegionName">The name of the region. Prefer using the Region object.</param>
/// <param name="WorldIds">List of row IDs for worlds contained in the data centre - for example, Light would contain Shiva, Twintania, etc. </param>
/// <param name="Region">A RegionData object for the region the DC is part of.</param>
public readonly record struct DataCentreData(
    uint Id,
    string Name,
    uint RegionId,
    string RegionName,
    IReadOnlyList<uint> WorldIds,
    RegionData? Region = null);
