
# ParserWebApp

ParserWebApp is a web application designed to parse and display log entries from NASA access logs. The application provides functionalities to start and stop a log parser and view the parsed log entries in a paginated, sortable table.

## Features

- **Log Parsing**: Parse NASA access log files and extract relevant information.
- **Pagination and Sorting**: View log entries in a paginated and sortable table.
- **Start/Stop Parser**: Control the log parser directly from the web interface.
- **Database Integration**: Store parsed log entries in a SQL Server database.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Installation

1. **Clone the repository:**
    ```bash
    git clone https://github.com/your-username/ParserWebApp.git
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

    Open your web browser and go to `https://localhost:5001`.

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

- **View Logs:**

    Navigate to the home page to view parsed log entries. Use the sorting options to order the log entries by date or client, and navigate through pages using the pagination controls.

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

## Contributing

Contributions are welcome! Please fork the repository and create a pull request with your changes. Ensure your code follows the project's coding standards and includes appropriate tests.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Contact

For any inquiries or feedback, please contact [your-email@example.com](mailto:your-email@example.com).

---

Enjoy using ParserWebApp! If you find this project useful, please star the repository.
