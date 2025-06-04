DROP TABLE DataAccessTest;
CREATE TABLE [DataAccessTest] (
    [ID] Int IDENTITY ,
    [VarCharColumn] Varchar (50),
    [IntColumn] Int,
    CONSTRAINT [PK_DataAccessTest] PRIMARY KEY NONCLUSTERED ([ID] ASC )
    );
INSERT INTO DataAccessTest(VarCharColumn,IntColumn) VALUES ('Example',0);