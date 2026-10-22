FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all source files
COPY shared/ ./shared/
COPY services/IdentityService/ ./services/IdentityService/

# Restore dependencies
RUN dotnet restore services/IdentityService/src/IdentityService.Api/IdentityService.Api.csproj

# Build and publish
RUN dotnet publish services/IdentityService/src/IdentityService.Api/IdentityService.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "IdentityService.Api.dll"]
