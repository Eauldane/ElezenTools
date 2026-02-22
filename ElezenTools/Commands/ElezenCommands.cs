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
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace ElezenTools.Commands;

public static class ElezenCommands
{
    // There has to be a jollier way to do this
    public static IReadOnlySet<string> DangerList { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/say", "/shout", "/yell", "/tell", "/fc", "/p", "/a", "/em", "/party", "/s", "/sh", "/y", "/alliance",
            "/cwlinkshell1", "/cwlinkshell2", "/cwlinkshell3", "/cwlinkshell4", "/cwlinkshell5",
            "/cwlinkshell6", "/cwlinkshell7", "/cwlinkshell8",
            "/cwl1", "/cwl2", "/cwl3", "/cwl4", "/cwl5", "/cwl6", "/cwl7", "/cwl8",
            "/linkshell1", "/linkshell2", "/linkshell3", "/linkshell4", "/linkshell5",
            "/linkshell6", "/linkshell7", "/linkshell8",
            "/l1", "/l2", "/l3", "/l4", "/l5", "/l6", "/l7", "/l8",
            "/reply", "/shutdown",
            
        };
    /// <summary>
    /// Sends a command through the game shell command pipeline.
    /// This overload observes and logs the outcome internally and does not require awaiting.
    /// </summary>
    /// <param name="commandText">
    /// Command text to send. Supports with or without a leading slash.
    /// Example: "/echo hi" or "echo hi".
    /// </param>
    /// <param name="allowDangerous">Whether to allow commands that can output to chat or not. Exercise EXTREME caution when setting this true.</param>
    public static void Send(string commandText, bool allowDangerous = false)
    {
        ObserveSend(commandText, SendAsync(commandText, allowDangerous));
    }

    /// <summary>
    /// Sends a command through the game shell command pipeline.
    /// This overload observes and logs the outcome internally and does not require awaiting.
    /// </summary>
    /// <param name="commandName">Command name with or without leading slash.</param>
    /// <param name="arguments">Optional command arguments.</param>
    /// <param name="allowDangerous">Whether to allow commands that can output to chat or not. Exercise EXTREME caution when setting this true.</param>
    public static void Send(string commandName, string? arguments, bool allowDangerous = false)
    {
        ObserveSend(commandName, SendAsync(commandName, arguments, allowDangerous));
    }

    /// <summary>
    /// Sends a command through the game shell command pipeline.
    /// Use this overload when the caller needs to observe success/failure directly.
    /// </summary>
    /// <param name="commandText">
    /// Command text to send. Supports with or without a leading slash.
    /// Example: "/echo hi" or "echo hi".
    /// </param>
    /// <param name="allowDangerous">Whether to allow commands that can output to chat or not. Exercise EXTREME caution when setting this true.</param>
    /// <returns>True if the command was dispatched successfully.</returns>
    public static async Task<bool> SendAsync(string commandText, bool allowDangerous = false)
    {
        if (string.IsNullOrWhiteSpace(commandText))
        {
            return false;
        }

        var normalised = NormaliseCommand(commandText);
        if (!allowDangerous && IsDangerous(normalised))
        {
            Service.Log.Error($"Tried to run a dangerous command ({normalised}) without acknowledging the danger.");
            return false;
        }

        return await Service.UseFramework(() => SendOnFramework(normalised)).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a command through the game shell command pipeline.
    /// Use this overload when the caller needs to observe success/failure directly.
    /// </summary>
    /// <param name="commandName">Command name with or without leading slash.</param>
    /// <param name="arguments">Optional command arguments.</param>
    /// <param name="allowDangerous">Whether to allow commands that can output to chat or not. Exercise EXTREME caution when setting this true.</param>
    /// <returns>True if the command was dispatched successfully.</returns>
    public static Task<bool> SendAsync(string commandName, string? arguments, bool allowDangerous = false)
    {
        return SendInternal(commandName, arguments, allowDangerous);
    }

    /// <summary>
    /// Sends a command name with optional arguments.
    /// </summary>
    /// <param name="commandName">Command name with or without leading slash.</param>
    /// <param name="arguments">Optional command arguments.</param>
    /// <param name="allowDangerous">Whether to allow commands that can output to chat or not. Exercise EXTREME caution when setting this true.</param>
    /// <returns>True if the command was dispatched successfully.</returns>
    private static Task<bool> SendInternal(string commandName, string? arguments, bool allowDangerous)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            return Task.FromResult(false);
        }

        var normalisedName = NormaliseCommand(commandName);
        if (!allowDangerous && IsDangerous(normalisedName))
        {
            Service.Log.Error($"Tried to run a dangerous command ({normalisedName}) without acknowledging the danger.");
            return Task.FromResult(false);
        }

        if (string.IsNullOrWhiteSpace(arguments))
        {
            return SendAsync(normalisedName, allowDangerous: true);
        }

        return SendAsync($"{normalisedName} {arguments.Trim()}", allowDangerous: true);
    }

    private static bool IsDangerous(string normalisedCommandText)
    {
        if (string.IsNullOrWhiteSpace(normalisedCommandText))
        {
            return false;
        }

        var firstSpaceIndex = normalisedCommandText.IndexOf(' ');
        var commandName = firstSpaceIndex < 0
            ? normalisedCommandText
            : normalisedCommandText[..firstSpaceIndex];

        return DangerList.Contains(commandName);
    }

    private static void ObserveSend(string commandDescription, Task<bool> sendTask)
    {
        _ = sendTask.ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Service.Log.Error(task.Exception?.Flatten().ToString() ?? $"Command send failed for: {commandDescription}");
                return;
            }

            if (!task.IsCompletedSuccessfully || !task.Result)
            {
                Service.Log.Warning($"Command send was not successful: {commandDescription}");
            }
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    private static string NormaliseCommand(string commandText)
    {
        var trimmed = commandText.Trim();
        if (trimmed.StartsWith('/'))
        {
            return trimmed;
        }

        return $"/{trimmed}";
    }

    private static unsafe bool SendOnFramework(string commandText)
    {
        var uiModule = UIModule.Instance();
        if (uiModule == null)
        {
            return false;
        }

        var shellModule = uiModule->GetRaptureShellModule();
        if (shellModule == null)
        {
            return false;
        }

        using var utf8Command = new Utf8String();
        utf8Command.SetString(commandText);
        shellModule->ExecuteCommandInner(&utf8Command, uiModule);
        return true;
    }
}
