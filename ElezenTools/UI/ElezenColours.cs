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

namespace ElezenTools.UI;

public static class ElezenColours
{
    public static Vector4 SnowcloakBlue { get; internal set; } = new Vector4(0.4275f, 0.6863f, 1f, 1f);
    public static Vector4 MissingTexture { get; internal set; } = new Vector4(1f, 0.0f, 1f, 1f);
    public static Vector4 SeafoamGreen { get; internal set; } = new Vector4(0.6275f, 0.8392f, 0.7059f, 1.0f);
    public static Vector4 EclipseRed { get; internal set; } = new Vector4(0.196f, 0.098f, 0.098f, 1.0f);
    public static Vector4 LemonYellow { get; internal set; } = new Vector4(0.98f, 0.98f, 0.2f, 1.0f);
    
}