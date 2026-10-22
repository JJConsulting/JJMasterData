using System.Data;
using JJMasterData.Commons.Data;

namespace JJMasterData.Commons.Test.Data;

public class DataAccessValueReaderTests
{
    [Fact]
    public void ResolvesReorderedMissingAndAmbiguousColumnsInOnePass()
    {
        var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = 0, ["Name"] = 1, ["Missing"] = 2
        };
        using var table = new DataTable();
        table.Columns.Add("NAME");
        table.Columns.Add("Extra");
        table.Columns.Add("id");
        using var reader = table.CreateDataReader();
        Assert.Equal([2, 0, -1], DataAccessValueReader.FindOrdinals(reader, columns));

        using var duplicate = new DataTable();
        duplicate.Columns.Add("Id");
        duplicate.Columns.Add("ID");
        using var duplicateReader = duplicate.CreateDataReader();
        Assert.Throws<DataException>(() => DataAccessValueReader.FindOrdinals(duplicateReader, columns));
    }

    private static DataTableReader Reader(object value)
    {
        var table = new DataTable();
        table.Columns.Add("Value", value.GetType());
        table.Rows.Add(value);
        var reader = table.CreateDataReader();
        reader.Read();
        return reader;
    }

    [Fact]
    public void ConvertsAllNumericTargetsAndExactTypes()
    {
        using var reader = Reader(42L);
        Assert.Equal((byte)42, DataAccessValueReader.ReadByte(reader, 0));
        Assert.Equal((sbyte)42, DataAccessValueReader.ReadSByte(reader, 0));
        Assert.Equal((short)42, DataAccessValueReader.ReadInt16(reader, 0));
        Assert.Equal((ushort)42, DataAccessValueReader.ReadUInt16(reader, 0));
        Assert.Equal(42, DataAccessValueReader.ReadInt32(reader, 0));
        Assert.Equal(42u, DataAccessValueReader.ReadUInt32(reader, 0));
        Assert.Equal(42L, DataAccessValueReader.ReadInt64(reader, 0));
        Assert.Equal(42UL, DataAccessValueReader.ReadUInt64(reader, 0));
        Assert.Equal(42f, DataAccessValueReader.ReadSingle(reader, 0));
        Assert.Equal(42d, DataAccessValueReader.ReadDouble(reader, 0));
        Assert.Equal(42m, DataAccessValueReader.ReadDecimal(reader, 0));
        using var exact = Reader(ulong.MaxValue);
        Assert.Equal(ulong.MaxValue, DataAccessValueReader.ReadUInt64(exact, 0));
        using var signed = Reader(long.MinValue);
        Assert.Equal(long.MinValue, DataAccessValueReader.ReadInt64(signed, 0));
        using var integerDouble = Reader(42d);
        Assert.Equal(42, DataAccessValueReader.ReadInt32(integerDouble, 0));
    }

    [Fact]
    public void RejectsOverflowFractionsAndUnrelatedProviderValues()
    {
        using var negative = Reader(-1);
        Assert.Throws<DataException>(() => DataAccessValueReader.ReadUInt64(negative, 0));
        using var overflow = Reader(256);
        Assert.Throws<DataException>(() => DataAccessValueReader.ReadByte(overflow, 0));
        using var singleOverflow = Reader(double.MaxValue);
        Assert.Throws<DataException>(() => DataAccessValueReader.ReadSingle(singleOverflow, 0));
        using var fractional = Reader(1.0000000000000002);
        Assert.Throws<DataException>(() => DataAccessValueReader.ReadInt32(fractional, 0));
        using var nan = Reader(double.NaN);
        Assert.Throws<DataException>(() => DataAccessValueReader.ReadInt64(nan, 0));
        using var number = Reader(1);
        Assert.Throws<DataException>(() => DataAccessValueReader.ReadString(number, 0));
        Assert.Throws<DataException>(() => DataAccessValueReader.ReadGuid(number, 0));
        using var invalidBoolean = Reader("true");
        Assert.Throws<DataException>(() => DataAccessValueReader.ReadBoolean(invalidBoolean, 0));
    }

    [Theory]
    [InlineData(1.0000001f)]
    [InlineData(0.99999994f)]
    [InlineData(float.Epsilon)]
    [InlineData(-float.Epsilon)]
    [InlineData(float.NaN)]
    [InlineData(1.0000000000000002d)]
    [InlineData(0.9999999999999999d)]
    [InlineData(double.Epsilon)]
    [InlineData(-double.Epsilon)]
    [InlineData(double.NaN)]
    public void RejectsFloatingPointFractionsAndNaNWithoutRounding(object value)
    {
        using var reader = Reader(value);
        var exception = Assert.Throws<DataException>(() => DataAccessValueReader.ReadInt32(reader, 0));
        Assert.IsType<InvalidCastException>(exception.InnerException);
        Assert.Throws<DataException>(() => DataAccessValueReader.ReadBoolean(reader, 0));
    }

    [Fact]
    public void PreservesExactDatesGuidsDurationsAndDecimals()
    {
        var guid = Guid.NewGuid();
        using var guidReader = Reader(guid);
        Assert.Equal(guid, DataAccessValueReader.ReadGuid(guidReader, 0));
        var date = new DateTime(2026, 10, 13, 10, 0, 0, DateTimeKind.Utc);
        using var dateReader = Reader(date);
        Assert.Equal(date, DataAccessValueReader.ReadDateTime(dateReader, 0));
        var offset = new DateTimeOffset(date);
        using var offsetReader = Reader(offset);
        Assert.Equal(offset, DataAccessValueReader.ReadDateTimeOffset(offsetReader, 0));
        using var spanReader = Reader(TimeSpan.FromMinutes(2));
        Assert.Equal(TimeSpan.FromMinutes(2), DataAccessValueReader.ReadTimeSpan(spanReader, 0));
        using var decimalReader = Reader(decimal.MaxValue);
        Assert.Equal(decimal.MaxValue, DataAccessValueReader.ReadDecimal(decimalReader, 0));
        using var textDecimal = Reader("12345678901234567890.123456789");
        Assert.Equal(12345678901234567890.123456789m, DataAccessValueReader.ReadDecimal(textDecimal, 0));
        using var invalidDate = Reader("10/13/2026");
        Assert.Throws<DataException>(() => DataAccessValueReader.ReadDateTime(invalidDate, 0));
        using var invalidOffset = Reader("2026-10-13T10:00:00");
        Assert.Throws<DataException>(() => DataAccessValueReader.ReadDateTimeOffset(invalidOffset, 0));
    }
}
