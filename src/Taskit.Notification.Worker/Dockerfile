FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Taskit.Notification.Worker/Taskit.Notification.Worker.csproj src/Taskit.Notification.Worker/
COPY src/Taskit.Domain/Taskit.Domain.csproj src/Taskit.Domain/
COPY src/Taskit.Infrastructure/Taskit.Infrastructure.csproj src/Taskit.Infrastructure/
RUN dotnet restore src/Taskit.Notification.Worker/Taskit.Notification.Worker.csproj

COPY . .
WORKDIR /src/src/Taskit.Notification.Worker
RUN dotnet publish Taskit.Notification.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app
ENV DOTNET_ENVIRONMENT=Production
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Taskit.Notification.Worker.dll"]
