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
/// Helper record containing Triple Triad card data.
/// </summary>
/// <param name="Id">Lumina row ID for the card.</param>
/// <param name="Name">Card name.</param>
/// <param name="Description">Card description text.</param>
/// <param name="Rarity">Card rarity value.</param>
/// <param name="Top">Top stat.</param>
/// <param name="Right">Right stat.</param>
/// <param name="Bottom">Bottom stat.</param>
/// <param name="Left">Left stat.</param>
/// <param name="SortOrder">Card sort order.</param>
public readonly record struct TripleTriadCardData(
    uint Id,
    string Name,
    string Description,
    uint Rarity,
    int Top,
    int Right,
    int Bottom,
    int Left,
    uint SortOrder);
