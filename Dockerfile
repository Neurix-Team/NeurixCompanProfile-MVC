# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY Neurix/Neurix.csproj Neurix/
COPY Neurix.BLL/Neurix.BLL.csproj Neurix.BLL/
COPY Neurix.DAL/Neurix.DAL.csproj Neurix.DAL/
RUN dotnet restore Neurix/Neurix.csproj

# Copy everything and build
COPY . .
RUN dotnet publish Neurix/Neurix.csproj \
    --configuration Release \
    --output /app/publish \
    --self-contained false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80

ENTRYPOINT ["dotnet", "Neurix.dll"]