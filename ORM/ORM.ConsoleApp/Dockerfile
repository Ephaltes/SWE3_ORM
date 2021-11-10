FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ORM_New/ORM_New.csproj", "ORM_New/"]
RUN dotnet restore "ORM_New/ORM_New.csproj"
COPY . .
WORKDIR "/src/ORM_New"
RUN dotnet build "ORM_New.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ORM_New.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ORM_New.dll"]
