FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Taskit.Web/Taskit.Web.csproj src/Taskit.Web/
COPY src/Taskit.Application/Taskit.Application.csproj src/Taskit.Application/
COPY src/Taskit.Infrastructure/Taskit.Infrastructure.csproj src/Taskit.Infrastructure/
COPY src/Taskit.Domain/Taskit.Domain.csproj src/Taskit.Domain/
RUN dotnet restore src/Taskit.Web/Taskit.Web.csproj

COPY . .
WORKDIR /src/src/Taskit.Web
RUN dotnet publish Taskit.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV DOTNET_ENVIRONMENT=Production
COPY --from=build /app/publish .

EXPOSE 5152
ENTRYPOINT ["dotnet", "Taskit.Web.dll"]
