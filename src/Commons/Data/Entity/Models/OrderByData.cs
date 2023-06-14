#nullable enable
namespace JJMasterData.Commons.Data.Entity;

public record OrderByData(
    string FieldName,
    OrderByDirection Direction = OrderByDirection.Asc
)
{
    public override string ToString()
    {
        return FieldName + " " + Direction.ToString().ToUpper();
    }
}