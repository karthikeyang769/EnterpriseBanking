FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all source files
COPY shared/ ./shared/
COPY services/TransactionService/ ./services/TransactionService/

# Restore dependencies
RUN dotnet restore services/TransactionService/src/TransactionService.Api/TransactionService.Api.csproj

# Build and publish
RUN dotnet publish services/TransactionService/src/TransactionService.Api/TransactionService.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TransactionService.Api.dll"]
