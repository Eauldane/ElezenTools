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
            return new Vector4(255f, 255f, 255f, 1f);
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
            return new Vector4(255f, 255f, 255f, 1f);
        }

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
}



