using System.Numerics;
using Dalamud.Game;
using Dalamud.Interface.Style;
using ElezenTools.Data.Classes;
using ElezenTools.Data.Enums;
using ElezenTools.Data.Internal;
using ElezenTools.Services;
using Lumina.Excel.Sheets;
using Dalamud.Interface.Colors;

namespace ElezenTools.Data;

public static partial class ElezenData
{
    public static class Jobs
    {
        private static readonly LanguageCache<JobData[]> Cache = new(BuildJobs);

        public static IReadOnlyDictionary<uint, JobData> All => GetAll();

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

        public static JobData? GetById(uint id, ClientLanguage? language = null)
        {
            return TryGetById(id, out var job, language) ? job : null;
        }

        public static bool TryGetById(uint id, out JobData job, ClientLanguage? language = null)
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

        public static JobData? GetByName(string name, ClientLanguage? language = null)
        {
            return TryGetByName(name, out var job, language) ? job : null;
        }

        public static bool TryGetByName(string name, out JobData job, ClientLanguage? language = null)
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

        public static JobData? GetByAbbreviation(string abbreviation, ClientLanguage? language = null)
        {
            return TryGetByAbbreviation(abbreviation, out var job, language) ? job : null;
        }

        public static bool TryGetByAbbreviation(string abbreviation, out JobData job, ClientLanguage? language = null)
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
                    startingTown));
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

        private static JobRole MapJobRole(uint jobType)
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

        private static JobRole ResolveJobRole(object row)
        {
            var jobType = LuminaRowReader.GetUInt32(row, "JobType");
            var role = MapJobRole(jobType);
            if (role != JobRole.Unknown)
            {
                return role;
            }

            var roleType = LuminaRowReader.GetUInt32(row, "Role");
            if (roleType != 0)
            {
                var mapped = MapJobRole(roleType);
                if (mapped != JobRole.Unknown)
                {
                    return mapped;
                }
            }

            return JobRole.Unknown;
        }

        private static Vector4 ResolveClassColour(JobRole role)
        {
            switch (role)
            {
                case JobRole.BarrierHealer:
                case JobRole.PureHealer:
                    return ImGuiColors.HealerGreen;
                case JobRole.Tank:
                    return ImGuiColors.TankBlue;
                case JobRole.MeleeDps:
                case JobRole.MagicalRanged:
                case JobRole.PhysicalRanged:
                    return ImGuiColors.DPSRed;
                default:
                    return ImGuiColors.DalamudGrey;
            }
        }
    }
}
