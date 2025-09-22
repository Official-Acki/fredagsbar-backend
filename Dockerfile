FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
# Publish with trimming and single file options for minimal size
RUN dotnet publish "fredagsbar-backend.csproj" -c Release -o /app/publish -r linux-musl-x64 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true

# Final minimal runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV HTTP_PORTS=8080

ENV DB_HOST=localhost,
ENV DB_PORT=5432,
ENV DB_USER=localhost,
ENV DB_PASSWORD=localhost,
ENV DB_NAME=fredagsbar-backend,
ENV INVITE_CODE=1234

ENTRYPOINT [ "./fredagsbar-backend" ]