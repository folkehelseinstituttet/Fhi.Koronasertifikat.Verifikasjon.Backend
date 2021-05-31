#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine3.13 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS="http://*:5000"

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine3.13 AS build
WORKDIR /src
COPY ["FHICORC.ApplicationHost.Hangfire/FHICORC.ApplicationHost.Hangfire.csproj", "FHICORC.ApplicationHost.Hangfire/"]
COPY ["FHICORC.Application.Common/FHICORC.Application.Common.csproj", "FHICORC.Application.Common/"]
COPY ["FHICORC.Application.Models/FHICORC.Application.Models.csproj", "FHICORC.Application.Models/"]
COPY ["FHICORC.Application.Repositories/FHICORC.Application.Repositories.csproj", "FHICORC.Application.Repositories/"]
COPY ["FHICORC.Infrastructure.Database/FHICORC.Infrastructure.Database.csproj", "FHICORC.Infrastructure.Database/"]
COPY ["FHICORC.Domain.Models/FHICORC.Domain.Models.csproj", "FHICORC.Domain.Models/"]
COPY ["FHICORC.Application.Validation/FHICORC.Application.Validation.csproj", "FHICORC.Application.Validation/"]
COPY ["FHICORC.Integrations.DGCGateway/FHICORC.Integrations.DGCGateway.csproj", "FHICORC.Integrations.DGCGateway/"]
RUN dotnet restore "FHICORC.ApplicationHost.Hangfire/FHICORC.ApplicationHost.Hangfire.csproj"
COPY . .
WORKDIR "/src/FHICORC.ApplicationHost.Hangfire"
RUN dotnet build "FHICORC.ApplicationHost.Hangfire.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FHICORC.ApplicationHost.Hangfire.csproj" -c Release -o /app/publish

FROM base AS final
RUN addgroup -S app && adduser -H -S -G app app
WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "FHICORC.ApplicationHost.Hangfire.dll"]