// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Runtime.InteropServices;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace ElezenTools.UI;

public static class ElezenStrings
{
    
    private const byte ColourTypeForeground = 0x13;
    private const byte ColourTypeGlow = 0x14;

    public static SeString BuildColouredString(string text, Colour colours)
    {
        var ssb = new SeStringBuilder();
        if (colours.Foreground != 0)
            ssb.Add(BuildColourStartPayload(ColourTypeForeground, colours.Foreground));
        if (colours.Glow != 0)
            ssb.Add(BuildColourStartPayload(ColourTypeGlow, colours.Glow));
        ssb.AddText(text);
        if (colours.Glow != 0)
            ssb.Add(BuildColourEndPayload(ColourTypeGlow));
        if (colours.Foreground != 0)
            ssb.Add(BuildColourEndPayload(ColourTypeForeground));
        return ssb.Build();
    }
        
    public static SeString BuildColourStartString(Colour colours)
    {
        var ssb = new SeStringBuilder();
        if (colours.Foreground != 0)
            ssb.Add(BuildColourStartPayload(ColourTypeForeground, colours.Foreground));
        if (colours.Glow != 0)
            ssb.Add(BuildColourStartPayload(ColourTypeGlow, colours.Glow));
        return ssb.Build();
    }

    public static SeString BuildColourEndString(Colour colors)
    {
        var ssb = new SeStringBuilder();
        if (colors.Glow != 0)
            ssb.Add(BuildColourEndPayload(ColourTypeGlow));
        if (colors.Foreground != 0)
            ssb.Add(BuildColourEndPayload(ColourTypeForeground));
        return ssb.Build();
    }
        
    private static RawPayload BuildColourStartPayload(byte colorType, uint color)
        => new(unchecked([0x02, colorType, 0x05, 0xF6, byte.Max((byte)color, 0x01), byte.Max((byte)(color >> 8), 0x01), byte.Max((byte)(color >> 16), 0x01), 0x03]));

    private static RawPayload BuildColourEndPayload(byte colorType)
        => new([0x02, colorType, 0x02, 0xEC, 0x03]);

    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Colour(uint Foreground = 0, uint Glow = 0);
}