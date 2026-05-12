# Production multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["src/AiEncyclopedia.API/AiEncyclopedia.API.csproj", "src/AiEncyclopedia.API/"]
COPY ["src/AiEncyclopedia.Application/AiEncyclopedia.Application.csproj", "src/AiEncyclopedia.Application/"]
COPY ["src/AiEncyclopedia.Domain/AiEncyclopedia.Domain.csproj", "src/AiEncyclopedia.Domain/"]
COPY ["src/AiEncyclopedia.Infrastructure/AiEncyclopedia.Infrastructure.csproj", "src/AiEncyclopedia.Infrastructure/"]

RUN dotnet restore "src/AiEncyclopedia.API/AiEncyclopedia.API.csproj"

COPY . .
RUN dotnet publish "src/AiEncyclopedia.API/AiEncyclopedia.API.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AiEncyclopedia.API.dll"]
