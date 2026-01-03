FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Kidzgo.API/Kidzgo.API.csproj", "Kidzgo.API/"]
COPY ["Kidzgo.Application/Kidzgo.Application.csproj", "Kidzgo.Application/"]
COPY ["Kidzgo.Domain/Kidzgo.Domain.csproj", "Kidzgo.Domain/"]
COPY ["Kidzgo.Infrastructure/Kidzgo.Infrastructure.csproj", "Kidzgo.Infrastructure/"]

RUN dotnet restore "Kidzgo.API/Kidzgo.API.csproj"

COPY . .

WORKDIR "/src/Kidzgo.API"
RUN dotnet publish "Kidzgo.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Kidzgo.API.dll"]
