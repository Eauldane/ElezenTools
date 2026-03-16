// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Numerics;
using ElezenTools.Data.Enums;

namespace ElezenTools.Data.Classes;

/// <summary>
/// Helper record containing job data.
/// </summary>
/// <param name="Id">Lumina ID of the job.</param>
/// <param name="Name">Job name - e.g. White Mage.</param>
/// <param name="Abbreviation">Job abbreviation - e.g. WHM.</param>
/// <param name="Role">The specific role of the job, as used for party bonuses - melee DPS, barrier healer, etc.</param>
/// <param name="IsLimited">Whether the job is limited or not (e.g. Blue Mage, Beastmaster etc.)</param>
/// <param name="Category">Jog category data from Lumina.</param>
/// <param name="Parent">Parent class for the job - e.g. Conjurer for White Mage.</param>
/// <param name="IconId">Job icon ID.</param>
/// <param name="SortOrder">Sort order.</param>
/// <param name="ClassColour">Vector4 class colour object for use in UIs (e.g. green for healer, red for DPS)</param>
/// <param name="StartingTown">TownData object for the job's starting town.</param>
/// <param name="JobClass">Devolved class data - just Healer, DPS, Tank. Use Role if you need the specific subtype.</param>
public readonly record struct JobData(
    uint Id,
    string Name,
    string Abbreviation,
    ElezenTools.Data.Enums.JobRole Role,
    bool IsLimited,
    JobCategoryData? Category,
    JobParentData? Parent,
    uint IconId,
    int SortOrder,
    Vector4 ClassColour,
    TownData? StartingTown,
    JobClass JobClass
    );
