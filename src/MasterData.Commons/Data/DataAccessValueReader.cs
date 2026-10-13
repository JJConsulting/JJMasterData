#nullable enable
using System;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace JJMasterData.Commons.Data;

/// <summary>Strict, culture-independent conversions used by generated mappers.</summary>
public static class DataAccessValueReader
{
    /// <summary>Resolves one column once per query; missing columns return -1, ambiguous columns fail.</summary>
    public static int FindOrdinal(DbDataReader reader, string column)
    {
        var ordinal = -1;
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (!string.Equals(reader.GetName(i), column, StringComparison.OrdinalIgnoreCase)) continue;
            if (ordinal >= 0) throw new DataException($"Column '{column}' occurs more than once in the result set.");
            ordinal = i;
        }
        return ordinal;
    }

    public static DataException NullValue(string column, string expectedType) => 
        new($"Column '{column}' contains NULL; expected non-nullable {expectedType}.");

    private static bool IsNumeric(object value) => value is byte
        or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;

    private static object Numeric(object value, bool integral)
    {
        if (!IsNumeric(value)) throw new InvalidCastException("Expected a numeric provider value.");

        if (integral && (value is float f && (float.IsNaN(f) || !MathF.Truncate(f).Equals(f)) ||
                         value is double d && (double.IsNaN(d) || !Math.Truncate(d).Equals(d)) ||
                         value is decimal m && decimal.Truncate(m) != m))
            throw new InvalidCastException("Fractional values cannot be mapped to integral properties.");
        return value;
    }

    private static T Read<T>(DbDataReader reader, int ordinal, string expectedType, Func<object, T> convert)
    {
        var value = reader.GetValue(ordinal);
        
        if (value is T exact) 
            return exact;
        try
        {
            return convert(value);
        }
        catch (Exception ex) when (ex is InvalidCastException or OverflowException or FormatException or ArgumentException)
        {
            throw new DataException($"Column '{reader.GetName(ordinal)}' cannot be mapped to {expectedType}.", ex);
        }
    }

    public static byte ReadByte(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.Byte", static value => Convert.ToByte(Numeric(value, true), CultureInfo.InvariantCulture));

    public static sbyte ReadSByte(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.SByte", static value => Convert.ToSByte(Numeric(value, true), CultureInfo.InvariantCulture));

    public static short ReadInt16(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.Int16", static value => Convert.ToInt16(Numeric(value, true), CultureInfo.InvariantCulture));

    public static ushort ReadUInt16(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.UInt16", static value => Convert.ToUInt16(Numeric(value, true), CultureInfo.InvariantCulture));

    public static int ReadInt32(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.Int32", static value => Convert.ToInt32(Numeric(value, true), CultureInfo.InvariantCulture));

    public static uint ReadUInt32(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.UInt32", static value => Convert.ToUInt32(Numeric(value, true), CultureInfo.InvariantCulture));

    public static long ReadInt64(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.Int64", static value => Convert.ToInt64(Numeric(value, true), CultureInfo.InvariantCulture));

    public static ulong ReadUInt64(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.UInt64", static value => Convert.ToUInt64(Numeric(value, true), CultureInfo.InvariantCulture));

    public static float ReadSingle(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.Single", static value =>
        {
            var number = Convert.ToSingle(Numeric(value, false), CultureInfo.InvariantCulture);
            if (float.IsInfinity(number) && value is double d && double.IsFinite(d))
                throw new OverflowException("Numeric value exceeds Single range.");
            return number;
        });

    public static double ReadDouble(DbDataReader reader, int ordinal)
    {
        return Read(reader, ordinal, "System.Double",
            static value => Convert.ToDouble(Numeric(value, false), CultureInfo.InvariantCulture));
    }

    public static decimal ReadDecimal(DbDataReader reader, int ordinal)
    {
        return Read(reader, ordinal, "System.Decimal", static value =>
        {
            if (value is string text)
            {
                return decimal.Parse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            }
            
            return Convert.ToDecimal(Numeric(value, false), CultureInfo.InvariantCulture);
        });
    }

    public static string ReadString(DbDataReader reader, int ordinal)
    {
        return Read<string>(reader, ordinal, "System.String",
            static _ => throw new InvalidCastException("Expected a string provider value."));
    }

    public static char ReadChar(DbDataReader reader, int ordinal)
    {
        return Read(reader, ordinal, "System.Char",
            static value => value is string { Length: 1 } text
                ? text[0]
                : throw new InvalidCastException("Expected a char or single-character string."));
    }

    public static bool ReadBoolean(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.Boolean", static value => Numeric(value, true) switch
        {
            var number when Convert.ToDecimal(number, CultureInfo.InvariantCulture) == 0 => false,
            var number when Convert.ToDecimal(number, CultureInfo.InvariantCulture) == 1 => true,
            _ => throw new InvalidCastException("Expected a boolean or numeric 0/1.")
        });

    public static Guid ReadGuid(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.Guid", static value => value is string text ? Guid.ParseExact(text, "D") : throw new InvalidCastException("Expected a Guid or canonical Guid string."));

    public static DateTime ReadDateTime(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.DateTime", static value =>
        {
            if (value is string text)
                return DateTime.ParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            
            throw new InvalidCastException("Expected a DateTime or ISO date string.");
        });

    public static DateTimeOffset ReadDateTimeOffset(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.DateTimeOffset", static value =>
        {
            if (value is string text)
                return DateTimeOffset.ParseExact(text, OffsetFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None);
            
            throw new InvalidCastException("Expected a DateTimeOffset or ISO date string with offset.");
        });

    public static TimeSpan ReadTimeSpan(DbDataReader reader, int ordinal) =>
        Read(reader, ordinal, "System.TimeSpan", static value =>
        {
            if (value is string text)
                return TimeSpan.ParseExact(text, "c", CultureInfo.InvariantCulture);
            throw new InvalidCastException("Expected a TimeSpan or constant-format duration string.");
        });

    //Você deve estar preocupado achando que ele lê somente essas datas, mas são formato ISO. Por volta da linha 46, caso seja data é retornado imediatamente.
    private static readonly string[] DateFormats = ["O", "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.FFFFFFF", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK"];
    private static readonly string[] OffsetFormats = ["O", "yyyy-MM-dd HH:mm:ss.FFFFFFFzzz", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz"];
}
