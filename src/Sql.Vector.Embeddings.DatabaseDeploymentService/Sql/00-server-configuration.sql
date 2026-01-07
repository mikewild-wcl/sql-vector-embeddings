IF(NOT EXISTS(SELECT [value] FROM sys.configurations WHERE [name] = 'show advanced options' AND [value] = 1))
BEGIN
	PRINT 'Reconfiguring server to show advanced options'
	EXEC sp_configure 'show advanced options', 1;
	RECONFIGURE WITH OVERRIDE;
END
GO

IF(NOT EXISTS(SELECT [value] FROM sys.configurations WHERE [name] = 'external rest endpoint enabled' AND [value] = 1))
BEGIN
	PRINT 'Reconfiguring server to allow external rest endpoints'
	EXEC sp_configure 'external rest endpoint enabled', 1;
	RECONFIGURE WITH OVERRIDE;
END
GO

IF(NOT EXISTS(SELECT [value] FROM sys.database_scoped_configurations WHERE [name] = 'PREVIEW_FEATURES' AND [value] = 1))
BEGIN
	PRINT 'Turning on preview features'
	ALTER DATABASE SCOPED CONFIGURATION
	SET PREVIEW_FEATURES = ON;
END
GO
