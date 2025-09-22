FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
# Publish with trimming and single file options for minimal size
RUN dotnet publish "fredagsbar-backend.csproj" -c Release -o /app/publish -r linux-musl-x64 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true

# Final minimal runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .

# ENTRYPOINT ["dotnet", "fredagsbar-backend.dll"]
ENTRYPOINT [ "/bin/sh" ]