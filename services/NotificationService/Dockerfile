FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy source files
COPY shared/ ./shared/
COPY services/NotificationService/ ./services/NotificationService/

# Restore dependencies
RUN dotnet restore services/NotificationService/NotificationService.csproj

# Build and publish
RUN dotnet publish services/NotificationService/NotificationService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "NotificationService.dll"]
