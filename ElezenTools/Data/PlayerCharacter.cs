// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace ElezenTools.Data;
[Obsolete("Temporary handler left in by mistake! Only here until I clean it.")]
public struct PlayerCharacter
{
    public uint ObjectId { get; set; }
    public string Name { get; set; }
    public uint HomeWorldId { get; set; }
    public nint Address { get; set; }
    public byte Level { get; set; }
    public byte ClassJob { get; set; }
    public byte Gender { get; set; }
    public byte Clan { get; set; }
};