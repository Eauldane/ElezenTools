// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Dalamud.Game;
using Dalamud.Utility;
using Lumina.Text.ReadOnly;
using System.Globalization;

namespace ElezenTools.Data;

public static partial class ElezenData
{
    private static string GetDisplayName(in ReadOnlySeString name, ClientLanguage language)
    {
        return string.Intern(name.ExtractText().ToUpper(true, true, false, language));
    }

    private static string FirstNonEmpty(params string[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }

    private static string BuildTitleName(string masculine, string feminine)
    {
        if (string.IsNullOrWhiteSpace(masculine))
        {
            return feminine;
        }

        if (string.IsNullOrWhiteSpace(feminine) || string.Equals(masculine, feminine, StringComparison.Ordinal))
        {
            return masculine;
        }

        return $"{masculine} / {feminine}";
    }

    private static string ToText(object? value)
    {
        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
