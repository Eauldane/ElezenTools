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
/// Helper record containing job category data.
/// </summary>
/// <param name="Id">Lumina row ID for the job category.</param>
/// <param name="Name">Name of the job category.</param>
public readonly record struct JobCategoryData(
    uint Id,
    string Name);
