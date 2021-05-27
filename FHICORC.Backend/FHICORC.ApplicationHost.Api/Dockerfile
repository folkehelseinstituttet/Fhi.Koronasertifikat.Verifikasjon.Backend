#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine3.13 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine3.13 AS build
WORKDIR /src
COPY ["FHICORC.ApplicationHost.Api/FHICORC.ApplicationHost.Api.csproj", "FHICORC.ApplicationHost.Api/"]
COPY ["FHICORC.Application.Common/FHICORC.Application.Common.csproj", "FHICORC.Application.Common/"]
COPY ["FHICORC.Application.Models/FHICORC.Application.Models.csproj", "FHICORC.Application.Models/"]
COPY ["FHICORC.Application.Repositories/FHICORC.Application.Repositories.csproj", "FHICORC.Application.Repositories/"]
COPY ["FHICORC.Integrations.Utilities/FHICORC.Integrations.Utilities.csproj", "FHICORC.Integrations.Utilities/"]
COPY ["FHICORC.Application.Validation/FHICORC.Application.Validation.csproj", "FHICORC.Application.Validation/"]
COPY ["FHICORC.Application.Services/FHICORC.Application.Services.csproj", "FHICORC.Application.Services/"]
RUN dotnet restore "FHICORC.ApplicationHost.Api/FHICORC.ApplicationHost.Api.csproj"
COPY . .
WORKDIR "/src/FHICORC.ApplicationHost.Api"
RUN dotnet build "FHICORC.ApplicationHost.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FHICORC.ApplicationHost.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FHICORC.ApplicationHost.Api.dll"]