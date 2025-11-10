# Keepi

Keepi is a practice project first and foremost. It was initially developed to get some practice with [clean architecture](<https://en.wikipedia.org/wiki/Hexagonal_architecture_(software)>) and later for practice with the [SPA YARP proxy](https://github.com/berhir/AspNetCore.SpaYarp) and the TypeScript [neverthrow library](https://github.com/supermacro/neverthrow).

The localization currently is Dutch only.

## What can keepi do?

Keepi is a simple time tracking web application. It supports the user registering spent time on user specified categories by a per week format through a grid input. The user can customize the labels for the categories, customize the order of categories and change what categories are available.

## Repository structure

This repository roughly follows the [.NET example](https://devblogs.microsoft.com/ise/next-level-clean-architecture-boilerplate/) of clean architecture. Key to the concept of clean architecture is that dependencies flow inward and any implementation details are provided through so-called plug-ins. In practice this means that the `src/Keepi.Core` project represents the center towards which all projects "point" in terms of dependency. A project such as the `src/Keepi.Infrastructure.Data` provides a persistence plug-in to the core by implementing the persistence related interfaces declared by the core.

The following (non-testing) projects are part of this repository, followed by a short description:

| Name                      | Description                                                                                                                                         |
|---------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------|
| Keepi.Core                | The project containing all business logic without concerning itself with the exact implementation of input and output.                              |
| Keepi.Infrastructure.Data | A persistence layer implemented through EF Core and Sqlite3.                                                                                        |
| Keepi.Api                 | A JSON API serving as an input output wrapper around Keepi.Core                                                                                     |
| Keepi.Vue                 | A Vue 3 web client application which uses the Keepi.Api provided HTTP endpoints to allow the user to interact with the business logic of Keepi.Core |
| Keepi.Web                 | An ASP.Net Core project that combines the JSON API of Keepi.Api and the web client of Keepi.Vue to create a so called backend before frontend setup |

## Development

### Required software

- [bun](https://bun.sh/)
- [Csharpier](https://csharpier.com/)
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) including the ASP.Net runtime
- [SQLite3](https://sqlite.org/)

### Secrets

This project uses some secrets, which are required to be setup:

```bash
dotnet user-secrets set 'Authentication:GitHub:ClientId' 'YOUR_CLIENT_ID' --project 'src/Keepi.Web/'
dotnet user-secrets set 'Authentication:GitHub:ClientSecret' 'YOUR_CLIENT_SECRET' --project 'src/Keepi.Web/'
```

### Creating database migrations

```bash
dotnet ef migrations add 'InitialCreate' -p 'src/Keepi.Infrastructure.Data' -s 'src/Keepi.Web'
```

### Creating the database

```bash
dotnet ef database update -p 'src/Keepi.Infrastructure.Data' -s 'src/Keepi.Web'
```

### HTTPS certificate not trusted on Linux

The combination of Linux and Firefox seems to work after using [this third party tool](https://github.com/dotnet/aspnetcore/issues/32842#issuecomment-2206905474):

```bash
dotnet tool update -g linux-dev-certs
dotnet linux-dev-certs install
```

### EF Core exceptions

By default the EF core exceptions are not exactly developer friendly, hence a third party strongly typed exceptions [package](https://github.com/Giorgi/EntityFramework.Exceptions) is used to make it easier to implement specific exception behaviour.

### Telemetry

This project uses Open Telemetry. Locally you can use this by running the Aspire dashboard standalone:

```bash
docker run --rm -it -d \
    -p 18888:18888 \
    -p 4317:18889 \
    --name aspire-dashboard \
    mcr.microsoft.com/dotnet/aspire-dashboard:latest
```

Once the container is running, use the `docker logs` functionality or similar to find the API token:

```bash
user@DESKTOP$~ docker logs 413946f9acdf
...
info: Aspire.Dashboard.DashboardWebApplication[0]
      Login to the dashboard at http://localhost:18888/login?t=f9ed81434d7ec8903db5fb34b1be12e7 . The URL may need changes depending on how network access to the container is configured.
...
```
