Reproduces issue in EFCore when db command gets cancelled both due to command timeout and cancellation token and no exception is thrown.

`docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Abcd5678" -p 1433:1433 mcr.microsoft.com/mssql/server`
