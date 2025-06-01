# ---------- Frontend Build ----------
FROM node:20-slim AS frontend-build
WORKDIR /app
COPY CoreAccess.Frontend/coreaccess-webui/ .         
RUN npm install
RUN npm run build

# ---------- Backend Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY CoreAccess.WebAPI/ ./           
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# ---------- Final Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Statische Dateien vom Frontend rein
COPY --from=frontend-build /app/dist ./wwwroot

# Backend Publish-Output rein
COPY --from=build /app/publish .

# Expose Port 80
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "CoreAccess.WebAPI.dll"]
