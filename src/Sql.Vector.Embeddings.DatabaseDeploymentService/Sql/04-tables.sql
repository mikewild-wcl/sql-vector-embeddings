
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
GO

IF NOT EXISTS(SELECT * FROM sys.indexes where name = 'IX_Documents_Metadata' AND type_desc = 'JSON')
    CREATE JSON INDEX IX_Documents_Metadata ON dbo.Documents(Metadata) FOR ('$');
GO

IF NOT EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentChunks')
    CREATE TABLE dbo.DocumentChunks
    (
        [Id] INT IDENTITY CONSTRAINT PK_DocumentChunks primary key,
        DocumentId INT NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        PageNumber INT NULL,
        IndexOnPage INT NULL,
        CONSTRAINT FK_DocumentChunks_Documents 
            FOREIGN KEY (DocumentId) REFERENCES Documents(Id) 
            ON DELETE CASCADE 
            ON UPDATE CASCADE
    )
GO

PRINT FORMATMESSAGE('Creating embedding tables with %s vector dimensions', $EMBEDDING_DIMENSIONS$)

IF NOT EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentSummaryEmbeddings')
    EXEC('CREATE TABLE dbo.DocumentSummaryEmbeddings (
        Id INT NOT NULL,
        Embedding VECTOR($EMBEDDING_DIMENSIONS$) NOT NULL,
        CONSTRAINT FK_DocumentSummaryEmbeddings_Documents FOREIGN KEY (Id) REFERENCES Documents(Id)
    )')
GO
    
IF NOT EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentCommentEmbeddings')
    EXEC('CREATE TABLE dbo.DocumentCommentEmbeddings (
        Id INT NOT NULL,
        Embedding VECTOR($EMBEDDING_DIMENSIONS$) NOT NULL,
        CONSTRAINT FK_DocumentCommentEmbeddings_Documents FOREIGN KEY (Id) REFERENCES Documents(Id) 
    )')
GO

IF NOT EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentMetadataEmbeddings')
    EXEC('CREATE TABLE dbo.DocumentMetadataEmbeddings (
        Id INT NOT NULL,
        Embedding VECTOR($EMBEDDING_DIMENSIONS$) NOT NULL,
        CONSTRAINT FK_DocumentMetadataEmbeddings_Documents FOREIGN KEY (Id) REFERENCES Documents(Id) 
    )')
GO

IF NOT EXISTS (SELECT * FROM sys.tables t INNER JOIN sys.schemas s ON (t.schema_id = s.schema_id) WHERE s.name = 'dbo' AND t.name = 'DocumentChunkEmbeddings')
    EXEC('CREATE TABLE dbo.DocumentChunkEmbeddings (
        Id INT NOT NULL,
        ChunkIndex INT NOT NULL,
        Embedding VECTOR($EMBEDDING_DIMENSIONS$) NOT NULL,
        PRIMARY KEY (Id, ChunkIndex),
        CONSTRAINT FK_DocumentChunkEmbeddings_DocumentChunks FOREIGN KEY (Id) REFERENCES Documents(Id) 
    )')
GO
