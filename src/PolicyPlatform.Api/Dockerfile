FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PolicyPlatform.slnx .
COPY src/PolicyPlatform.Domain/PolicyPlatform.Domain.csproj src/PolicyPlatform.Domain/
COPY src/PolicyPlatform.Application/PolicyPlatform.Application.csproj src/PolicyPlatform.Application/
COPY src/PolicyPlatform.Infrastructure/PolicyPlatform.Infrastructure.csproj src/PolicyPlatform.Infrastructure/
COPY src/PolicyPlatform.Api/PolicyPlatform.Api.csproj src/PolicyPlatform.Api/
RUN dotnet restore src/PolicyPlatform.Api/PolicyPlatform.Api.csproj

COPY src/ src/
RUN dotnet publish src/PolicyPlatform.Api/PolicyPlatform.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PolicyPlatform.Api.dll"]
