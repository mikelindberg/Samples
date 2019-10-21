FROM microsoft/dotnet:2.2-sdk AS build-env

WORKDIR /app

COPY /util/ /util/
COPY /modules/SensorSimulatorApi/ ./

RUN dotnet restore
RUN dotnet publish -c Release -o /app/out/

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app/

COPY --from=build-env /app/out ./

RUN useradd -ms /bin/bash moduleuser
USER moduleuser

ENV ASPNETCORE_URLS=http://*:5001/

ENTRYPOINT ["dotnet", "SensorSimulatorApi.dll"]