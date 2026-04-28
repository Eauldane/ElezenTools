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
/// Helper record containing parent class data for a job.
/// </summary>
/// <param name="Id">Lumina row ID for the parent class.</param>
/// <param name="Name">Name of the parent class - e.g. Conjurer.</param>
/// <param name="Abbreviation">Abbreviation of the parent class - e.g. CNJ.</param>
public readonly record struct JobParentData(
    uint Id,
    string Name,
    string Abbreviation);
