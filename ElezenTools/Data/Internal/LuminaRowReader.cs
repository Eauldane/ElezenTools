using System.Globalization;

namespace ElezenTools.Data.Internal;

internal static class LuminaRowReader
{
    public static string? GetString(object row, params string[] propertyNames)
    {
        var value = GetValue(row, propertyNames);
        return ConvertToString(value);
    }

    public static uint GetUInt32(object row, params string[] propertyNames)
    {
        var value = GetValue(row, propertyNames);
        return ConvertToUInt(value);
    }

    public static int GetInt32(object row, params string[] propertyNames)
    {
        var value = GetValue(row, propertyNames);
        return ConvertToInt(value);
    }

    public static bool GetBool(object row, params string[] propertyNames)
    {
        var value = GetValue(row, propertyNames);
        return ConvertToBool(value);
    }

    public static uint GetRowId(object row, params string[] propertyNames)
    {
        return GetUInt32(row, propertyNames);
    }

    public static string? GetRowName(object row, params string[] propertyNames)
    {
        var value = GetValue(row, propertyNames);
        if (value == null)
        {
            return null;
        }

        var rowValue = GetPropertyValue(value, "Value") ?? value;
        var nameValue = GetPropertyValue(rowValue, "Name");
        return ConvertToString(nameValue);
    }

    public static string? GetRowString(object row, string rowPropertyName, params string[] propertyNames)
    {
        if (propertyNames == null || propertyNames.Length == 0)
        {
            return null;
        }

        var value = GetValue(row, rowPropertyName);
        if (value == null)
        {
            return null;
        }

        var rowValue = GetPropertyValue(value, "Value") ?? value;
        var propertyValue = GetValue(rowValue, propertyNames);
        return ConvertToString(propertyValue);
    }

    private static object? GetValue(object row, params string[] propertyNames)
    {
        foreach (var name in propertyNames)
        {
            var value = GetPropertyValue(row, name);
            if (value != null)
            {
                return value;
            }
        }

        return null;
    }

    private static object? GetPropertyValue(object target, string name)
    {
        var property = target.GetType().GetProperty(name);
        return property?.GetValue(target);
    }

    private static string? ConvertToString(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is string text)
        {
            return text;
        }

        var rowValue = GetPropertyValue(value, "Value");
        if (rowValue != null && !ReferenceEquals(rowValue, value))
        {
            return ConvertToString(rowValue);
        }

        return value.ToString();
    }

    private static uint ConvertToUInt(object? value)
    {
        if (value == null)
        {
            return 0;
        }

        switch (value)
        {
            case uint uintValue:
                return uintValue;
            case int intValue:
                return intValue < 0 ? 0u : (uint)intValue;
            case ushort ushortValue:
                return ushortValue;
            case short shortValue:
                return shortValue < 0 ? 0u : (uint)shortValue;
            case byte byteValue:
                return byteValue;
            case sbyte sbyteValue:
                return sbyteValue < 0 ? 0u : (uint)sbyteValue;
            case long longValue:
                return longValue < 0 ? 0u : (uint)longValue;
            case ulong ulongValue:
                return ulongValue > uint.MaxValue ? uint.MaxValue : (uint)ulongValue;
            case Enum enumValue:
                return Convert.ToUInt32(enumValue, CultureInfo.InvariantCulture);
            case string text when uint.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed):
                return parsed;
        }

        var rowIdValue = GetPropertyValue(value, "RowId")
            ?? GetPropertyValue(value, "RowID")
            ?? GetPropertyValue(value, "Id")
            ?? GetPropertyValue(value, "Key");

        if (rowIdValue != null && !ReferenceEquals(rowIdValue, value))
        {
            return ConvertToUInt(rowIdValue);
        }

        var rowValue = GetPropertyValue(value, "Value");
        if (rowValue != null && !ReferenceEquals(rowValue, value))
        {
            var innerRowId = GetPropertyValue(rowValue, "RowId")
                ?? GetPropertyValue(rowValue, "RowID")
                ?? GetPropertyValue(rowValue, "Id")
                ?? GetPropertyValue(rowValue, "Key");

            if (innerRowId != null && !ReferenceEquals(innerRowId, rowValue))
            {
                return ConvertToUInt(innerRowId);
            }
        }

        return 0u;
    }

    private static int ConvertToInt(object? value)
    {
        if (value == null)
        {
            return 0;
        }

        return value switch
        {
            int intValue => intValue,
            uint uintValue => uintValue > int.MaxValue ? int.MaxValue : (int)uintValue,
            short shortValue => shortValue,
            ushort ushortValue => ushortValue,
            byte byteValue => byteValue,
            sbyte sbyteValue => sbyteValue,
            long longValue => longValue > int.MaxValue ? int.MaxValue : (int)longValue,
            ulong ulongValue => ulongValue > int.MaxValue ? int.MaxValue : (int)ulongValue,
            Enum enumValue => Convert.ToInt32(enumValue, CultureInfo.InvariantCulture),
            string text when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                => parsed,
            _ => (int)ConvertToUInt(value),
        };
    }

    private static bool ConvertToBool(object? value)
    {
        if (value == null)
        {
            return false;
        }

        return value switch
        {
            bool boolValue => boolValue,
            byte byteValue => byteValue != 0,
            sbyte sbyteValue => sbyteValue != 0,
            short shortValue => shortValue != 0,
            ushort ushortValue => ushortValue != 0,
            int intValue => intValue != 0,
            uint uintValue => uintValue != 0,
            long longValue => longValue != 0,
            ulong ulongValue => ulongValue != 0,
            Enum enumValue => Convert.ToUInt32(enumValue, CultureInfo.InvariantCulture) != 0,
            string text when bool.TryParse(text, out var parsed) => parsed,
            _ => ConvertToUInt(value) != 0,
        };
    }
}
