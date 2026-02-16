// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Eauldane
//
// This file is part of ElezenTools.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

#nullable disable

using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ElezenTools.Services;

#pragma warning disable S1118
public class Service
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] public static IFramework Framework { get; private set; }
    [PluginService] public static IPlayerState PlayerState { get; private set; }
    [PluginService] public static IPluginLog Log { get; private set; }
    [PluginService] public static IObjectTable ObjectTable { get; private set; }
    [PluginService] public static IDataManager DataManager { get; private set; }
    [PluginService] public static IChatGui ChatGui { get; private set; }
    [PluginService] public static ICommandManager CommandManager { get; private set; }


    public static void Init(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
    }

    public static async Task UseFramework(System.Action action)
    {
        if (!Framework.IsInFrameworkUpdateThread)
        {
            await Framework.RunOnFrameworkThread(action).ContinueWith((_) => Task.CompletedTask).ConfigureAwait(false);
            while (Framework.IsInFrameworkUpdateThread) // yield the thread again, should technically never be triggered
            {
                await Task.Delay(1).ConfigureAwait(false);
            }
        }
        else
        {
            action();
        }
    }
    
    public static async Task<T> UseFramework<T>(Func<T> func)
    {
        if (!Framework.IsInFrameworkUpdateThread)
        {
            var result = await Framework.RunOnFrameworkThread(func).ContinueWith((task) => task.Result).ConfigureAwait(false);
            while (Framework.IsInFrameworkUpdateThread) // yield the thread again, should technically never be triggered
            {
                await Task.Delay(1).ConfigureAwait(false);
            }
            return result;
        }

        return func.Invoke();
    }

    /// <summary>
    /// Sends a chat command through the in-game command pipeline.
    /// Example: "/say Hello".
    /// </summary>
    /// <param name="commandText">The full command text.</param>
    /// <returns>True if the command was found and dispatched.</returns>
    public static async Task<bool> SendChatCommand(string commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText))
            return false;

        return await UseFramework(() => CommandManager.ProcessCommand(commandText.Trim())).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a chat message. If the message already starts with "/", it is sent as-is.
    /// Otherwise, it is prefixed with the provided chat mode (default: /say).
    /// </summary>
    /// <param name="messageText">The chat message or full command text.</param>
    /// <param name="chatMode">The slash command prefix used for plain text messages.</param>
    /// <returns>True if the command was found and dispatched.</returns>
    public static Task<bool> SendChatMessage(string messageText, string chatMode = "/say")
    {
        if (string.IsNullOrWhiteSpace(messageText))
            return Task.FromResult(false);

        var trimmedMessage = messageText.Trim();
        if (trimmedMessage.StartsWith('/'))
            return SendChatCommand(trimmedMessage);

        var effectiveChatMode = string.IsNullOrWhiteSpace(chatMode) ? "/say" : chatMode.Trim();
        if (!effectiveChatMode.StartsWith('/'))
            effectiveChatMode = $"/{effectiveChatMode}";

        return SendChatCommand($"{effectiveChatMode} {trimmedMessage}");
    }
}
