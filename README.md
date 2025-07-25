# TaskIt Backend

TaskIt is a backend service for managing projects and tasks. It is built with **.NET 9** and uses **RabbitMQ** for message processing. A background worker handles notifications alongside the main HTTP API.

## Features

- Project and task management with tagging and comments
- JWT based authentication
- RabbitMQ integration for sending notifications
- Docker support for running the API and worker
- Unit test suite for application services

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/) if running locally without Docker
- [Docker](https://docs.docker.com/) and Docker Compose for containerised setup

## Running with Docker

1. Build and start the containers:
   ```bash
   docker-compose up --build
   ```
2. The API will be available on [http://localhost:5152](http://localhost:5152).
3. RabbitMQ management UI runs on [http://localhost:15672](http://localhost:15672) using the default `guest/guest` credentials.

## Running locally with the .NET CLI

1. Restore dependencies and build the solution:
   ```bash
   dotnet restore src/Taskit.sln
   dotnet build src/Taskit.sln
   ```
2. Start the web project:
   ```bash
   dotnet run --project src/Taskit.Web
   ```
3. The API listens on port `5152` by default. A SQLite database file named `taskit.db` will be created in the web project folder.

## Running tests

Execute the unit tests with:

```bash
dotnet test src/Taskit.sln
```

## Author

Created by **Jorge Jimenez** (<jorgeajimenezl17@gmail.com>).

