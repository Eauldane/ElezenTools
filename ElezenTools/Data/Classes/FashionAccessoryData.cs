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
/// Helper record containing fashion accessory data.
/// </summary>
/// <param name="Id">Lumina row ID for the fashion accessory.</param>
/// <param name="Name">Fashion accessory name.</param>
/// <param name="Description">Fashion accessory description text.</param>
/// <param name="SortOrder">Fashion accessory sort order.</param>
public readonly record struct FashionAccessoryData(
    uint Id,
    string Name,
    string Description,
    int SortOrder);
