#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Calc_Service_API/Calc_Service_API.csproj", "Calc_Service_API/"]
RUN dotnet restore "Calc_Service_API/Calc_Service_API.csproj"
COPY . .
WORKDIR "/src/Calc_Service_API"
RUN dotnet build "Calc_Service_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Calc_Service_API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Calc_Service_API.dll"]