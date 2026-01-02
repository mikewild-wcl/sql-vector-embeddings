IF(NOT EXISTS(SELECT [value] FROM sys.configurations WHERE [name] = 'external rest endpoint enabled' AND [value] = 1))
BEGIN
	PRINT 'Reconfiguring server to allow external rest endpoints'
	EXEC sp_configure 'external rest endpoint enabled', 1;
	RECONFIGURE WITH OVERRIDE;
END
