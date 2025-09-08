# ---------- Frontend Build ----------
FROM node:20-slim AS frontend-build
WORKDIR /app
COPY CoreAccess.Frontend/coreaccess-webui/ .
RUN npm install
RUN npm run build

# ---------- Backend Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Zuerst nur sln + csproj-Dateien (Caching optimieren)
COPY CoreAccess.sln ./
COPY CoreAccess.Models/CoreAccess.Models.csproj CoreAccess.Models/
COPY CoreAccess.DataLayer/CoreAccess.DataLayer.csproj CoreAccess.DataLayer/
COPY CoreAccess.BizLayer/CoreAccess.BizLayer.csproj CoreAccess.BizLayer/
COPY CoreAccess.Workers/CoreAccess.Workers.csproj CoreAccess.Workers/
COPY CoreAccess.WebAPI/CoreAccess.WebAPI.csproj CoreAccess.WebAPI/
COPY CoreAccess.Tests/CoreAccess.Tests.csproj CoreAccess.Tests/

# Restore über die Solution (alle Projekte werden berücksichtigt)
RUN dotnet restore CoreAccess.sln

# Jetzt gesamten Code kopieren
COPY . .

# Publish nur das WebAPI (das ist dein Einstiegspunkt)
WORKDIR /src/CoreAccess.WebAPI
RUN dotnet publish -c Release -o /app/publish

# ---------- Final Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Statische Dateien vom Frontend rein
COPY --from=frontend-build /app/dist ./wwwroot

# Publish-Output ins Runtime-Image
COPY --from=build /app/publish .

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "CoreAccess.WebAPI.dll"]
