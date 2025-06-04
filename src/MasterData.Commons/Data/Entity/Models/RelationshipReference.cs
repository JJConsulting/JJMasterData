namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Represents the equivalent table name of an element.
/// </summary>
/// <param name="ElementName"></param>
/// <param name="TableName"></param>
public record RelationshipReference(string ElementName, string TableName);