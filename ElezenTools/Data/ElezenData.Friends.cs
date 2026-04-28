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
using ElezenTools.Data.Classes;
using ElezenTools.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace ElezenTools.Data;

public static partial class ElezenData
{
    public static class Friends
    {
        private static readonly TimeSpan RefreshPollDelay = TimeSpan.FromMilliseconds(250);
        private const int RefreshPollAttempts = 12;

        /// <summary>
        /// Read the client's currently cached friend list data.
        /// This does not ask the client to refresh the list first.
        /// </summary>
        /// <param name="language">Language to use for resolved world names. Defaults to client language.</param>
        /// <returns>FriendEntryData items read from the current client cache.</returns>
        public static Task<IReadOnlyList<FriendEntryData>> GetCachedAsync(ClientLanguage? language = null)
        {
            var resolvedLanguage = ResolveLanguage(language);
            return Service.Framework.RunOnFrameworkThread(() => CaptureEntriesUnsafe(false, resolvedLanguage).Entries);
        }

        /// <summary>
        /// Refresh the friend list data.
        /// If the client's cached data is empty, this will ask the client to populate the list and poll briefly for results.
        /// </summary>
        /// <param name="language">Language to use for resolved world names. Defaults to client language.</param>
        /// <returns>FriendEntryData items after the refresh attempt completes.</returns>
        public static async Task<IReadOnlyList<FriendEntryData>> RefreshAsync(ClientLanguage? language = null)
        {
            var resolvedLanguage = ResolveLanguage(language);

            var cachedEntries = await Service.Framework
                .RunOnFrameworkThread(() => CaptureEntriesUnsafe(false, resolvedLanguage))
                .ConfigureAwait(false);

            if (cachedEntries.Entries.Count > 0)
            {
                return cachedEntries.Entries;
            }

            var requestedEntries = await Service.Framework
                .RunOnFrameworkThread(() => CaptureEntriesUnsafe(true, resolvedLanguage))
                .ConfigureAwait(false);

            if (requestedEntries.Entries.Count > 0 || !requestedEntries.RequestedData)
            {
                return requestedEntries.Entries;
            }

            for (var attempt = 0; attempt < RefreshPollAttempts; attempt++)
            {
                await Task.Delay(RefreshPollDelay).ConfigureAwait(false);

                var polledEntries = await Service.Framework
                    .RunOnFrameworkThread(() => CaptureEntriesUnsafe(false, resolvedLanguage))
                    .ConfigureAwait(false);

                if (polledEntries.Entries.Count > 0)
                {
                    return polledEntries.Entries;
                }
            }

            return requestedEntries.Entries;
        }

        private static unsafe FriendListCapture CaptureEntriesUnsafe(bool requestIfEmpty, ClientLanguage language)
        {
            if (!Service.ClientState.IsLoggedIn)
            {
                throw new InvalidOperationException("Log in on a character before reading the friend list.");
            }

            var infoProxy = InfoProxyFriendList.Instance();
            if (infoProxy == null)
            {
                throw new InvalidOperationException("The friend list info proxy is unavailable.");
            }

            var entryCount = (int)Math.Min(infoProxy->GetEntryCount(), int.MaxValue);
            if (entryCount <= 0)
            {
                var requested = requestIfEmpty && infoProxy->RequestData();
                return new FriendListCapture(Array.Empty<FriendEntryData>(), requested);
            }

            var entries = BuildEntries(infoProxy, entryCount, language);
            return new FriendListCapture(entries, false);
        }

        private static unsafe List<FriendEntryData> BuildEntries(InfoProxyFriendList* infoProxy, int entryCount, ClientLanguage language)
        {
            var worlds = Worlds.GetAll(language);
            var entries = new List<FriendEntryData>(entryCount);
            for (var i = 0; i < entryCount; i++)
            {
                var friendPtr = infoProxy->GetEntry((uint)i);
                if (friendPtr == null || string.IsNullOrWhiteSpace(friendPtr->NameString))
                {
                    continue;
                }

                var homeWorldId = friendPtr->HomeWorld;
                var currentWorldId = friendPtr->CurrentWorld;
                var homeWorld = worlds.TryGetValue(homeWorldId, out var resolvedHomeWorld) ? resolvedHomeWorld : (WorldData?)null;
                var currentWorld = worlds.TryGetValue(currentWorldId, out var resolvedCurrentWorld) ? resolvedCurrentWorld : (WorldData?)null;
                var homeWorldName = homeWorld?.Name ?? ResolveWorldName(homeWorldId);
                var currentWorldName = currentWorld?.Name ?? ResolveWorldName(currentWorldId);
                var isVisiting = currentWorldId != 0 && currentWorldId != homeWorldId;
                var worldLabel = isVisiting
                    ? $"{homeWorldName} (visiting {currentWorldName})"
                    : homeWorldName;

                entries.Add(new FriendEntryData(
                    friendPtr->NameString,
                    homeWorldId,
                    homeWorldName,
                    currentWorldId,
                    currentWorldName,
                    isVisiting,
                    worldLabel,
                    friendPtr->ContentId,
                    homeWorld,
                    currentWorld));
            }

            entries.Sort(static (left, right) =>
            {
                var byName = string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
                if (byName != 0)
                {
                    return byName;
                }

                var byWorld = string.Compare(left.WorldLabel, right.WorldLabel, StringComparison.OrdinalIgnoreCase);
                return byWorld != 0
                    ? byWorld
                    : left.ContentId.CompareTo(right.ContentId);
            });

            return entries;
        }

        private static string ResolveWorldName(uint worldId)
        {
            if (worldId == 0)
            {
                return "Unknown";
            }

            var world = Worlds.GetById(worldId);
            return world?.Name ?? worldId.ToString();
        }

        private readonly record struct FriendListCapture(
            IReadOnlyList<FriendEntryData> Entries,
            bool RequestedData);
    }
}
