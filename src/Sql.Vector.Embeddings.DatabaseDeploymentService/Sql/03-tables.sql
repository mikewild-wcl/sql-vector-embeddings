IF NOT EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'Documents')
    CREATE TABLE dbo.Documents
    (
        [Id] INT IDENTITY CONSTRAINT PK_Documents primary key,
        [Title] nvarchar(300) NOT NULL,
        [Summary] nvarchar(max) NULL,
        [Comments] NVARCHAR(max) NULL,
        [ArxivId] NVARCHAR(50) NULL,
        [Doi] NVARCHAR(50) NULL,
        [Metadata] JSON NULL,
        [Url] NVARCHAR(1000) NOT NULL,
        [Published] DATETIME2(0) NOT NULL,
        [Updated] DATETIME2(7) NULL,
        [CreatedOn] DATETIME2(7) NOT NULL CONSTRAINT DF_Documents_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        [LastUpdatedOn] datetime2(0) NULL
    )

IF NOT EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'Testing')
    CREATE TABLE dbo.Testing
    (
        [Id] INT IDENTITY CONSTRAINT PK_Testing PRIMARY KEY,
        [Description] NVARCHAR(1000) NULL
    )
