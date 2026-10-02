# PeePoo API + website. Build from src/PeePoo:  docker build -t peepoo .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish API/API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
# SQLite database and locally stored photos live on writable volumes owned by the non-root app user.
RUN mkdir -p /data /app/wwwroot/uploads && chown -R $APP_UID /data /app/wwwroot/uploads
ENV ASPNETCORE_URLS=http://+:8080 \
    ConnectionStrings__DefaultConnection="Data Source=/data/peepoo.db"
EXPOSE 8080
VOLUME ["/data", "/app/wwwroot/uploads"]
USER $APP_UID
ENTRYPOINT ["dotnet", "API.dll"]
