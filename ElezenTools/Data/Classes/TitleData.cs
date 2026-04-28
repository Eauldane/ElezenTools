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
/// Helper record containing title data.
/// </summary>
/// <param name="Id">Lumina row ID for the title.</param>
/// <param name="Name">Combined title name, using both masculine and feminine forms when they differ.</param>
/// <param name="MasculineName">Masculine title form.</param>
/// <param name="FeminineName">Feminine title form.</param>
/// <param name="IsPrefix">Whether the title is shown as a prefix.</param>
/// <param name="SortOrder">Title sort order.</param>
public readonly record struct TitleData(
    uint Id,
    string Name,
    string MasculineName,
    string FeminineName,
    bool IsPrefix,
    uint SortOrder);
