# SQL vector embeddings

Use SQL Server 2025 and Azure SQL vector capabilities to ingest and embed documents.

## SQL database

If you want to connect to the database with SSMS you can get the connection string in the Aspire portal and paste it into the connection dialog.

# Call function app

The function app expects an array called `uris` with an array of uris pointing to pdf files. 

There is a test http script in the functions folder that can be used to call it. Alternatively call from a command line using curl:
```
curl -X POST http://localhost:7034/api/ingest-uris/ -H "Content-Type: application/json" -d '{"uris": ["https://arxiv.org/pdf/1409.0473" ] }'
```

