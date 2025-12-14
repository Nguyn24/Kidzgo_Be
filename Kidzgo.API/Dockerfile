FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Kidzgo.Apis/Kidzgo.Apis.csproj", "Kidzgo.Apis/"]
COPY ["Kidzgo.Application/Kidzgo.Application.csproj", "Kidzgo.Application/"]
COPY ["Kidzgo.Domain/Kidzgo.Domain.csproj", "Kidzgo.Domain/"]
COPY ["Kidzgo.Infrastructure/Kidzgo.Infrastructure.csproj", "Kidzgo.Infrastructure/"]

RUN dotnet restore "Kidzgo.Apis/Kidzgo.Apis.csproj"

COPY . .

WORKDIR "/src/Kidzgo.Apis"
RUN dotnet publish "Kidzgo.Apis.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Kidzgo.Apis.dll"]
