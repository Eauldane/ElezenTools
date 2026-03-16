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
using System.Runtime.InteropServices;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace ElezenTools.UI;

public static class ElezenStrings
{
    
    private const byte ColourTypeForeground = 0x13;
    private const byte ColourTypeGlow = 0x14;

    /// <summary>
    /// Build a coloured SeString instance, for use in Honorifics, chat box, nameplates, etc. 
    /// </summary>
    /// <param name="text">The text for the .</param>
    /// <param name="colours">The Colour object for the string .</param>

    /// <returns>An SeString with the defined colours.</returns>
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
        
    /// <summary>
    /// Build a payload denoting the start of a coloured SeString. You probably want BuildColouredSeString instead! 
    /// </summary>
    /// <param name="colours">The colour for the string.</param>
    /// <returns>An SeString payload with the defined colours.</returns>
    public static SeString BuildColourStartString(Colour colours)
    {
        var ssb = new SeStringBuilder();
        if (colours.Foreground != 0)
            ssb.Add(BuildColourStartPayload(ColourTypeForeground, colours.Foreground));
        if (colours.Glow != 0)
            ssb.Add(BuildColourStartPayload(ColourTypeGlow, colours.Glow));
        return ssb.Build();
    }

    /// <summary>
    /// Build a payload denoting the end of a coloured SeString. You probably want BuildColouredSeString instead! 
    /// </summary>
    /// <param name="colours">The colour for the string.</param>
    /// <returns>An SeString payload with the defined colours.</returns>
    public static SeString BuildColourEndString(Colour colours)
    {
        var ssb = new SeStringBuilder();
        if (colours.Glow != 0)
            ssb.Add(BuildColourEndPayload(ColourTypeGlow));
        if (colours.Foreground != 0)
            ssb.Add(BuildColourEndPayload(ColourTypeForeground));
        return ssb.Build();
    }
        
    private static RawPayload BuildColourStartPayload(byte colourType, uint colour)
        => new(unchecked([0x02, colourType, 0x05, 0xF6, byte.Max((byte)colour, 0x01), byte.Max((byte)(colour >> 8), 0x01), byte.Max((byte)(color >> 16), 0x01), 0x03]));

    private static RawPayload BuildColourEndPayload(byte colourType)
        => new([0x02, colourType, 0x02, 0xEC, 0x03]);

    /// <summary>
    /// Struct representing a full SeString colourset with both foreground and glow colours. Accepts both uint and Vector4 colours. 
    /// </summary>
    /// <param name="Foreground">The colour the main text should be.</param>
    /// <param name="Glow">The colour for the "background" of the text. Aesthetically, this
    ///                     works best if it's a darker version of the colour used for the foreground.</param>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Colour(uint Foreground = 0, uint Glow = 0)
    {
        public Colour(Vector4 foreground, Vector4? glow = null)
            : this(global::ElezenTools.UI.Colour.Vector4ToColour(foreground),
                glow.HasValue ? global::ElezenTools.UI.Colour.Vector4ToColour(glow.Value) : 0u)
        {
        }
    };
}