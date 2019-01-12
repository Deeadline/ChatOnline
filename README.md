# Chat Online - student project

## Stack:
* .NET Core 2.1
* MySQL 8
* SignalR

### Required:
* MySQL 8 with your database server
* .NET Core 2.1

`Clone git repository` in terminal use `dotnet restore` to get all packages.
In appsettings write your database connection.
In terminal write `dotnet ef migrations add HERE_YOUR_MIGRATION_NAME` after that use `dotnet ef database update`
Finally use `dotnet run` to start application.
