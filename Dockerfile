# Multi-stage Dockerfile for LehengaVerse .NET 8 Web API
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["Rent_Lehenga_Project.csproj", "./"]
RUN dotnet restore "Rent_Lehenga_Project.csproj"

COPY . .
RUN dotnet publish "Rent_Lehenga_Project.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Rent_Lehenga_Project.dll"]
