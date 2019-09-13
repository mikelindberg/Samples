FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY . ./
RUN dotnet restore

# copy and build everything else
RUN dotnet publish -c Release -o /app/out/

FROM mcr.microsoft.com/dotnet/core/runtime:2.2.7-stretch-slim
WORKDIR /app/
COPY --from=build-env /app/out ./

ENTRYPOINT ["dotnet", "telemetryReader.dll"]