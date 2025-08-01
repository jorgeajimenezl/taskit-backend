FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Taskit.AI.Orchestrator/Taskit.AI.Orchestrator.csproj src/Taskit.AI.Orchestrator/
COPY src/Taskit.Domain/Taskit.Domain.csproj src/Taskit.Domain/
COPY src/Taskit.Infrastructure/Taskit.Infrastructure.csproj src/Taskit.Infrastructure/
RUN dotnet restore src/Taskit.AI.Orchestrator/Taskit.AI.Orchestrator.csproj

COPY . .
WORKDIR /src/src/Taskit.AI.Orchestrator
RUN dotnet publish Taskit.AI.Orchestrator.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV DOTNET_ENVIRONMENT=Production
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Taskit.AI.Orchestrator.dll"]
