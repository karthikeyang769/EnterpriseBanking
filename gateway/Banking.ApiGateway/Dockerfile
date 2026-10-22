FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy gateway source
COPY gateway/Banking.ApiGateway/ ./gateway/Banking.ApiGateway/

# Restore dependencies
RUN dotnet restore gateway/Banking.ApiGateway/Banking.ApiGateway.csproj

# Build and publish
RUN dotnet publish gateway/Banking.ApiGateway/Banking.ApiGateway.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Banking.ApiGateway.dll"]
