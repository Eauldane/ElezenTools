// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using ElezenTools.Data.Enums;

namespace ElezenTools.Data.Classes;

/// <summary>
/// Status data container.
/// </summary>
/// <param name="Id">Internal ID of the status.</param>
/// <param name="Name">Name of the status.</param>
/// <param name="Description">Description text of the status.</param>
/// <param name="StatusType">Whether it's a buff, debuff, or neutral.</param>
/// <param name="CanDispel">Whether the status can be removed.</param>
/// <param name="IsFcBuff">Whether the buff is granted by FC actions.</param>
public readonly record struct StatusData(
    uint Id,
    string Name,
    string Description,
    StatusType StatusType,
    bool CanDispel,
    bool IsFcBuff
    );