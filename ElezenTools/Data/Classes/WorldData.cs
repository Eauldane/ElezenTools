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
/// Helper record containing world data.
/// </summary>
/// <param name="Id">Lumina row ID for the world.</param>
/// <param name="Name">Name of the world - e.g. Shiva.</param>
/// <param name="DataCenterId">Row ID of the data centre the world belongs to. Prefer using the DataCenter object.</param>
/// <param name="DataCenterName">Name of the data centre the world belongs to. Prefer using the DataCenter object.</param>
/// <param name="RegionId">Row ID of the region the world belongs to. Prefer using the Region object.</param>
/// <param name="RegionName">Name of the region the world belongs to. Prefer using the Region object.</param>
/// <param name="IsPublic">Whether the world is marked as public in Lumina.</param>
/// <param name="IsCloud">Whether the world is marked as a cloud world.</param>
/// <param name="DataCenter">A DataCentreData object for the world's data centre.</param>
/// <param name="Region">A RegionData object for the world's region.</param>
public readonly record struct WorldData(
    uint Id,
    string Name,
    uint DataCenterId,
    string DataCenterName,
    uint RegionId,
    string RegionName,
    bool IsPublic,
    bool IsCloud,
    DataCentreData? DataCenter = null,
    RegionData? Region = null);
