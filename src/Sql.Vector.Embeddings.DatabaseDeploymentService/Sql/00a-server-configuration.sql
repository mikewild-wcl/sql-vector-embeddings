IF(NOT EXISTS(SELECT [value] FROM sys.configurations WHERE [name] = 'show advanced options' AND [value] = 1))
BEGIN
	PRINT 'Reconfiguring server to show advanced options'
	EXEC sp_configure 'show advanced options', 1;
	RECONFIGURE WITH OVERRIDE;
END
