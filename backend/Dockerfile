FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Kupucha.Api/Kupucha.Api.csproj", "Kupucha.Api/"]
RUN dotnet restore "Kupucha.Api/Kupucha.Api.csproj"
COPY . .
WORKDIR "/src/Kupucha.Api"
RUN dotnet publish "Kupucha.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "Kupucha.Api.dll"]
