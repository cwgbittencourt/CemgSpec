FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SpecAudit/SpecAudit.csproj SpecAudit/
COPY SpecAudit/packages.lock.json SpecAudit/
RUN dotnet restore SpecAudit/SpecAudit.csproj
COPY SpecAudit/ SpecAudit/
RUN dotnet publish SpecAudit/SpecAudit.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
RUN apt-get update && apt-get install -y --no-install-recommends python3 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://0.0.0.0:5187
EXPOSE 5187
ENTRYPOINT ["dotnet", "SpecAudit.dll"]
