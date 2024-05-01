namespace JJMasterData.Commons.Exceptions;

public enum DataAccessErrorKind
{
    Unhandled,
    DependencyCannotBeDeleted,
    RecordAlreadyRegistered,
    InvalidCharacter,
    Custom
}