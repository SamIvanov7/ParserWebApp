
# ParserWebApp

ParserWebApp is a web application designed to parse and display log entries from NASA access logs. 

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Installation

1. **Clone the repository:**
    ```bash
    git clone https://github.com/SamIvanov7/ParserWebApp.git
    cd ParserWebApp
    ```

2. **Restore NuGet packages:**
    ```bash
    dotnet restore
    ```

3. **Update the connection string:**

    Open `appsettings.json` and update the `DefaultConnection` string to point to your SQL Server instance.

4. **Apply migrations to set up the database:**
    ```bash
    dotnet ef database update
    ```

5. **Build and run the application:**
    ```bash
    dotnet run --project ParserWebApp
    ```

6. **Navigate to the application:**

    Open your web browser and go to `https://localhost:port`.

### Configuration

Ensure your `appsettings.json` file is configured correctly:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=ParserDb;User Id=your_user;Password=your_password;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### Usage

- **Start Parser:**

    Click the "Start Parser" button to begin parsing the log file. Ensure the path to the parser executable is correctly set in the `StartParser` method in `LogController.cs`.

- **Stop Parser:**

    Click the "Stop Parser" button to stop the parser.

## Project Structure

- **ParserConsoleApp**: Contains the console application that parses the log files.
- **ParserWebApp**: Contains the web application to manage and view log entries.
- **ParserWebApp.Data**: Contains the Entity Framework Core DbContext and database models.

## Dependencies

### ParserConsoleApp

```xml
<ItemGroup>
    <PackageReference Include="EFCore.BulkExtensions" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.3" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
</ItemGroup>
```

### ParserWebApp

```xml
<ItemGroup>
    <PackageReference Include="EFCore.BulkExtensions" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.3" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="X.PagedList" Version="9.1.2" />
    <PackageReference Include="X.PagedList.Mvc.Core" Version="9.1.2" />
</ItemGroup>
```

