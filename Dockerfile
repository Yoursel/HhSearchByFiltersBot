FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY HhBot.sln ./
COPY HhBot/HhBot.csproj HhBot/
COPY HhBot.Application/HhBot.Application.csproj HhBot.Application/
COPY HhBot.Domain/HhBot.Domain.csproj HhBot.Domain/
COPY HhBot.Infrastructure/HhBot.Infrastructure.csproj HhBot.Infrastructure/

RUN dotnet restore HhBot/HhBot.csproj

COPY . .
RUN dotnet publish HhBot/HhBot.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HhBot.dll"]
