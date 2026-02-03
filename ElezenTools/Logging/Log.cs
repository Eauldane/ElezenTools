// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using ElezenTools.Services;

namespace ElezenTools.Logging;

public static class Log
{
    public static void Debug(string s)
    {
        Service.Log.Debug(s);
    }
    
    public static void Information(string s)
    {
        Service.Log.Information(s);
    }
    
    public static void Warning(string s)
    {
        Service.Log.Warning(s);
    }
    
    public static void Error(string s)
    {
        Service.Log.Error(s);
    }
    
    public static void Verbose(string s)
    {
        Service.Log.Verbose(s);
    }
}