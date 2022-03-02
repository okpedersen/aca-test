FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Dockerfile assumes src/ is used as context

# Copy csproj and restore as distinct layers
COPY aca-todo.API/aca-todo.API.csproj aca-todo.API/
RUN dotnet restore aca-todo.API/aca-todo.API.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish "aca-todo.API/aca-todo.API.csproj" -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "aca-todo.API.dll"]
EXPOSE 80

