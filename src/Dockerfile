#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Order.WebAPI/Order.WebAPI.csproj", "Order.WebAPI/"]
COPY ["Order.Service/Order.Service.csproj", "Order.Service/"]
COPY ["Order.Model/Order.Model.csproj", "Order.Model/"]
COPY ["Order.Data/Order.Data.csproj", "Order.Data/"]
RUN dotnet restore "Order.WebAPI/Order.WebAPI.csproj"
COPY . .
WORKDIR "/src/Order.WebAPI"
RUN dotnet build "Order.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Order.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.WebAPI.dll"]