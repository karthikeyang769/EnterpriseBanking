FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy source files
COPY shared/ ./shared/
COPY services/BeneficiaryService/ ./services/BeneficiaryService/

# Restore dependencies
RUN dotnet restore services/BeneficiaryService/BeneficiaryService.csproj

# Build and publish
RUN dotnet publish services/BeneficiaryService/BeneficiaryService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BeneficiaryService.dll"]
