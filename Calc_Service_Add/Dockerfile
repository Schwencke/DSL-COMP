#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Calc_Service_Add/Calc_Service_Add.csproj", "Calc_Service_Add/"]
COPY ["Events/Events.csproj", "Events/"]
COPY ["Helpers/Helpers.csproj", "Helpers/"]
RUN dotnet restore "Calc_Service_Add/Calc_Service_Add.csproj"
COPY . .
WORKDIR "/src/Calc_Service_Add"
RUN dotnet build "Calc_Service_Add.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Calc_Service_Add.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Calc_Service_Add.dll"]