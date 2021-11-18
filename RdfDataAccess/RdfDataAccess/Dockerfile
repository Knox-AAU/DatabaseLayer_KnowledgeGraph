FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RdfDataAccess/RdfDataAccess.csproj", "RdfDataAccess/"]
RUN dotnet restore "RdfDataAccess/RdfDataAccess.csproj"
COPY . .
WORKDIR "/src/RdfDataAccess"
RUN dotnet build "RdfDataAccess.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RdfDataAccess.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RdfDataAccess.dll"]
