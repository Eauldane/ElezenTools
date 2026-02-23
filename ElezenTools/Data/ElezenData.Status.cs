using Dalamud.Game;
using ElezenTools.Data.Classes;
using ElezenTools.Data.Enums;
using ElezenTools.Data.Internal;
using ElezenTools.Services;
using Lumina.Excel.Sheets;

namespace ElezenTools.Data;

public static partial class ElezenData
{
    public static class Statuses
    {
        private static readonly LanguageCache<StatusData[]> Cache = new(BuildStatuses);

        public static IReadOnlyDictionary<uint, StatusData> All => GetAll();

        public static IReadOnlyDictionary<uint, StatusData> GetAll(ClientLanguage? language = null)
        {
            var statuses = GetStatuses(language);
            var result = new Dictionary<uint, StatusData>(statuses.Length);
            foreach (var status in statuses)
            {
                result[status.Id] = status;
            }

            return result;
        }

        public static StatusData? GetById(uint id, ClientLanguage? language = null)
        {
            return TryGetById(id, out var status, language) ? status : null;
        }

        public static bool TryGetById(uint id, out StatusData status, ClientLanguage? language = null)
        {
            var statuses = GetStatuses(language);
            var index = Array.FindIndex(statuses, item => item.Id == id);
            if (index >= 0)
            {
                status = statuses[index];
                return true;
            }

            status = default;
            return false;
        }

        public static StatusData? GetByName(string name, ClientLanguage? language = null)
        {
            return TryGetByName(name, out var status, language) ? status : null;
        }

        public static bool TryGetByName(string name, out StatusData status, ClientLanguage? language = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                status = default;
                return false;
            }

            var statuses = GetStatuses(language);
            var index = Array.FindIndex(statuses, item => string.Equals(item.Name, name, StringComparison.Ordinal));
            if (index >= 0)
            {
                status = statuses[index];
                return true;
            }

            status = default;
            return false;
        }

        public static void Refresh()
        {
            Cache.Clear();
        }

        private static StatusData[] GetStatuses(ClientLanguage? language)
        {
            return Cache.Get(ResolveLanguage(language));
        }

        private static StatusData[] BuildStatuses(ClientLanguage language)
        {
            var sheet = Service.DataManager.GetExcelSheet<Status>(language);
            if (sheet == null)
            {
                return Array.Empty<StatusData>();
            }

            var statuses = new List<StatusData>();
            foreach (var row in sheet)
            {
                var name = LuminaRowReader.GetString(row, "Name");
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var description = LuminaRowReader.GetString(row, "Description") ?? string.Empty;
                var type = ResolveStatusType(row);
                var canDispel = LuminaRowReader.GetBool(row, "CanDispel");
                var isFcBuff = LuminaRowReader.GetBool(row, "IsFcBuff");

                statuses.Add(new StatusData(
                    row.RowId,
                    name,
                    description,
                    type,
                    canDispel,
                    isFcBuff));
            }

            return statuses.ToArray();
        }

        private static StatusType ResolveStatusType(object row)
        {
            var category = LuminaRowReader.GetUInt32(row, "StatusCategory");
            return category switch
            {
                (uint)StatusType.Buff => StatusType.Buff,
                (uint)StatusType.Debuff => StatusType.Debuff,
                _ => StatusType.Unknown,
            };
        }
    }
}
