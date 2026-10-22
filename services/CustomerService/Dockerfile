FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all source files
COPY shared/ ./shared/
COPY services/CustomerService/ ./services/CustomerService/

# Restore dependencies
RUN dotnet restore services/CustomerService/src/CustomerService.Api/CustomerService.Api.csproj

# Build and publish
RUN dotnet publish services/CustomerService/src/CustomerService.Api/CustomerService.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CustomerService.Api.dll"]
