# SQL vector embeddings

Use SQL Server 2025 and Azure SQL vector capabilities to ingest and embed documents.

## SQL database

If you want to connect to the database with SSMS you can get the connection string in the Aspire portal and paste it into the connection dialog.

Optionally you can set the SQL Server port and sa password by adding these parameters to your secrets. If the parameters aren't set then default values will be generated.
```
  "Parameters": {
   "SqlServerPassword": "<password>",
   "SqlServerPort": 14331
   }
```

## Call function app

The function app expects an array called `uris` with an array of uris pointing to pdf files. 

There is a test http script in the functions folder that can be used to call it. Alternatively call from a command line using curl:
```
curl -X POST http://localhost:7034/api/ingest-uris/ -H "Content-Type: application/json" -d '{"uris": ["https://arxiv.org/pdf/1409.0473" ] }'
```

## Aspire hosting and deployment

 - Function integration - https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-aspire-integration


## AI model clients

If using Ollama and have a GPU, include a parameter `OllamaGpuVendor` in the AppHost appsettings or secrets. The value can be `Nvidia` or `AMD` (or any future values from `Aspire.Hosting.OllamaGpuVendor`).
This is added via an extension `WithGPUSupportIfVendorParameterProvided` and should match your system.
```
  "Parameters": {
   "OllamaGpuVendor": "Nvidia"
   }
```

