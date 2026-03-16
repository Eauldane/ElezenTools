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
using Dalamud.Game;
using Dalamud.Interface.Style;
using ElezenTools.Data.Classes;
using ElezenTools.Data.Enums;
using ElezenTools.Data.Internal;
using ElezenTools.Services;
using Lumina.Excel.Sheets;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace ElezenTools.Data;

public static partial class ElezenData
{
    public static class Jobs
    {
        private static readonly LanguageCache<JobData[]> Cache = new(BuildJobs);

        /// <summary>
        /// Get all job data.
        /// </summary>
        public static IReadOnlyDictionary<uint, JobData> All => GetAll();

        /// <summary>
        /// Get all job data.
        /// </summary>
        /// <param name="language">Client language to use. Defaults to player's language if not set.</param>
        /// <returns></returns>
        public static IReadOnlyDictionary<uint, JobData> GetAll(ClientLanguage? language = null)
        {
            var jobs = GetJobs(language);
            var result = new Dictionary<uint, JobData>(jobs.Length);
            foreach (var job in jobs)
            {
                result[job.Id] = job;
            }

            return result;
        }

        /// <summary>
        /// Get a job by its ID.
        /// </summary>
        /// <param name="id">Job ID to get.</param>
        /// <param name="language">Client target language. Defaults to player's language.</param>
        /// <returns>JobData entity if the job was found; or null otherwise.</returns>
        public static JobData? GetById(uint id, ClientLanguage? language = null)
        {
            return TryGetById(id, out var job, language) ? job : null;
        }

        private static bool TryGetById(uint id, out JobData job, ClientLanguage? language = null)
        {
            var jobs = GetJobs(language);
            var index = Array.FindIndex(jobs, item => item.Id == id);
            if (index >= 0)
            {
                job = jobs[index];
                return true;
            }

            job = default;
            return false;
        }

        /// <summary>
        ///  Get job by name. Potentally risky if language isn't specified! 
        /// </summary>
        /// <param name="name">Job name to get.</param>
        /// <param name="language">Language to use. Default's to players language - manually specify if you can!</param>
        /// <returns>JobData if found, null otherwise.</returns>
        public static JobData? GetByName(string name, ClientLanguage? language = null)
        {
            return TryGetByName(name, out var job, language) ? job : null;
        }

        private static bool TryGetByName(string name, out JobData job, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                job = default;
                return false;
            }

            var jobs = GetJobs(language);
            var index = Array.FindIndex(jobs, item => string.Equals(item.Name, name, StringComparison.Ordinal));
            if (index >= 0)
            {
                job = jobs[index];
                return true;
            }

            job = default;
            return false;
        }

        /// <summary>
        /// Get a job by its abbreivation.
        /// </summary>
        /// <param name="abbreviation">Abbreviation to look for.</param>
        /// <param name="language">Client language to check. Defaults to client language, set this if you can!</param>
        /// <returns>JobData if found, null otherwise.</returns>
        public static JobData? GetByAbbreviation(string abbreviation, ClientLanguage? language = null)
        {
            return TryGetByAbbreviation(abbreviation, out var job, language) ? job : null;
        }

        private static bool TryGetByAbbreviation(string abbreviation, out JobData job, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(abbreviation))
            {
                job = default;
                return false;
            }

            var jobs = GetJobs(language);
            var index = Array.FindIndex(jobs, item => string.Equals(item.Abbreviation, abbreviation, StringComparison.Ordinal));
            if (index >= 0)
            {
                job = jobs[index];
                return true;
            }

            job = default;
            return false;
        }

        public static void Refresh()
        {
            Cache.Clear();
        }

        private static JobData[] GetJobs(ClientLanguage? language)
        {
            return Cache.Get(ResolveLanguage(language));
        }

        private static JobData[] BuildJobs(ClientLanguage language)
        {
            var sheet = Service.DataManager.GetExcelSheet<ClassJob>(language);
            if (sheet == null)
            {
                return Array.Empty<JobData>();
            }

            var jobs = new List<JobData>();
            foreach (var row in sheet)
            {
                var name = LuminaRowReader.GetString(row, "Name");
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var abbreviation = LuminaRowReader.GetString(row, "Abbreviation") ?? string.Empty;
                var role = ResolveJobRole(row);
                var isLimited = LuminaRowReader.GetBool(row, "IsLimitedJob");
                var category = ReadJobCategory(row);
                var parent = ReadJobParent(row);
                var iconId = LuminaRowReader.GetUInt32(row, "Icon");
                var sortOrder = LuminaRowReader.GetInt32(row, "SortOrder");
                var classColour = ResolveClassColour(role);
                var startingTown = ReadStartingTown(row);
                var jobClass = GetJobClass(role);

                jobs.Add(new JobData(
                    row.RowId,
                    name,
                    abbreviation,
                    role,
                    isLimited,
                    category,
                    parent,
                    iconId,
                    sortOrder,
                    classColour,
                    startingTown,
                    jobClass));
            }

            return jobs.ToArray();
        }

        private static JobCategoryData? ReadJobCategory(object row)
        {
            var categoryId = LuminaRowReader.GetRowId(row, "ClassJobCategory");
            if (categoryId == 0)
            {
                return null;
            }

            var categoryName = LuminaRowReader.GetRowName(row, "ClassJobCategory") ?? string.Empty;
            return new JobCategoryData(categoryId, categoryName);
        }

        private static JobClass GetJobClass(JobRole role)
        {
            switch (role)
            {
                case JobRole.BarrierHealer:
                case JobRole.PureHealer:
                    return JobClass.Healer;
                case JobRole.MagicalRanged:
                case JobRole.PhysicalRanged:
                case JobRole.MeleeDps:
                    return JobClass.Dps;
                case JobRole.Tank:
                    return JobClass.Tank;
                case JobRole.Unknown:
                    return JobClass.Unknown;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role));
            }
        }

        private static JobParentData? ReadJobParent(object row)
        {
            var parentId = LuminaRowReader.GetRowId(row, "ClassJobParent");
            if (parentId == 0)
            {
                return null;
            }

            var parentName = LuminaRowReader.GetRowName(row, "ClassJobParent") ?? string.Empty;
            var parentAbbreviation = LuminaRowReader.GetRowString(row, "ClassJobParent", "Abbreviation") ?? string.Empty;
            return new JobParentData(parentId, parentName, parentAbbreviation);
        }

        private static TownData? ReadStartingTown(object row)
        {
            var townId = LuminaRowReader.GetRowId(row, "StartingTown");
            if (townId == 0)
            {
                return null;
            }

            var townName = LuminaRowReader.GetRowName(row, "StartingTown") ?? string.Empty;
            return new TownData(townId, townName);
        }

        private static JobRole MapJobTypeRole(uint jobType)
        {
            return jobType switch
            {
                1 => JobRole.Tank,
                2 => JobRole.PureHealer,
                3 => JobRole.MeleeDps,
                4 => JobRole.PhysicalRanged,
                5 => JobRole.MagicalRanged,
                6 => JobRole.BarrierHealer,
                _ => JobRole.Unknown,
            };
        }

        private static JobRole MapClassRole(uint roleType, object row)
        {
            return roleType switch
            {
                1 => JobRole.Tank,
                2 => JobRole.MeleeDps,
                3 => ResolveRangedClassRole(row),
                4 => JobRole.PureHealer,
                _ => JobRole.Unknown,
            };
        }

        private static JobRole ResolveRangedClassRole(object row)
        {
            var primaryStat = LuminaRowReader.GetUInt32(row, "PrimaryStat");
            return primaryStat switch
            {
                2 => JobRole.PhysicalRanged,
                4 => JobRole.MagicalRanged,
                _ => JobRole.Unknown,
            };
        }

        private static JobRole ResolveJobRole(object row)
        {
            var jobType = LuminaRowReader.GetUInt32(row, "JobType");
            var role = MapJobTypeRole(jobType);
            if (role != JobRole.Unknown)
            {
                return role;
            }

            var roleType = LuminaRowReader.GetUInt32(row, "Role");
            if (roleType != 0)
            {
                var mapped = MapClassRole(roleType, row);
                if (mapped != JobRole.Unknown)
                {
                    return mapped;
                }
            }

            return JobRole.Unknown;
        }

        private static Vector4 ResolveClassColour(JobRole role)
        {
            return role switch
            {
                JobRole.BarrierHealer or JobRole.PureHealer => ImGuiColors.HealerGreen,
                JobRole.Tank => ImGuiColors.TankBlue,
                JobRole.MeleeDps or JobRole.MagicalRanged or JobRole.PhysicalRanged => ImGuiColors.DPSRed,
                _ => ImGuiColors.DalamudGrey
            };
        }
    }
}
