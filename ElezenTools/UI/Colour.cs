// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ElezenTools.UI;

public static class Colour
{
    /// <summary>
    /// Convert a hexadecimal string to a Vector4 color, suitable for using with ImGui.
    /// </summary>
    /// <param name="hex">The hex string - 3, 4, 6 or 8 characters, with or without leading #.</param>
    /// <returns>Vector4 colour value. Returns white if the string was invalid or a null value was passed.</returns>
    public static Vector4 HexToVector4(string? hex)
    {
        if (hex == null)
        {
            return new Vector4(1f, 1f, 1f, 1f);
        }

        hex = hex.Trim().TrimStart('#');
        // On the off-chance someone sends a short hex, we still need to be able to 
        // handle those. They ARE valid colour hexes, after all...
        if (hex.Length is 3 or 4)
        {
            var chars = new char[hex.Length * 2];
            for (var i = 0; i < hex.Length; i++)
            {
                chars[i * 2] = hex[i];
                chars[i * 2 + 1] = hex[i];
            }

            hex = new string(chars);
        }
        if (hex.Length != 6 && hex.Length != 8)
        {
            return new Vector4(1f, 1f, 1f, 1f);
        }

        try
        {
            var r = byte.Parse(hex[..2], NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            byte a = 255;


            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
            }
        
            return new Vector4(r / 255f, g / 255f, b / 255f, a /255f);
        }
        catch
        {
            return new Vector4(1f, 1f, 1f, 1f);
        }
    }

    /// <summary>
    /// Convert a hexadecimal string to a uint color, suitable for using with SeStrings and other Dalamud internals.
    /// </summary>
    /// <param name="hex">The hex string - 3, 4, 6 or 8 characters, with or without leading #.</param>
    /// <returns>uint colour value. Returns white if the string was invalid or a null value was passed.</returns>
    public static uint HexToColour(string? hex)
    {
        return Vector4ToColour(HexToVector4(hex));
    }
    
    public static uint RgbaToColour(byte r, byte g, byte b, byte a)
    { uint ret = a; ret <<= 8; ret += b; ret <<= 8; ret += g; ret <<= 8; ret += r; return ret; }

    /// <summary>
    /// Convert a Vector4 colour to a uint color, suitable for using with SeStrings and other Dalamud internals.
    /// </summary>
    /// <param name="colour">A Vector4 representing a colour.</param>
    /// <returns>uint colour value. Returns white if the string was invalid or a null value was passed.</returns>
    public static uint Vector4ToColour(Vector4 colour)
    {
        uint ret = (byte)(colour.W * 255);
        ret <<= 8;
        ret += (byte)(colour.Z * 255);
        ret <<= 8;
        ret += (byte)(colour.Y * 255);
        ret <<= 8;
        ret += (byte)(colour.X * 255);
        return ret;
    }
}



