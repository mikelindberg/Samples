FROM microsoft/dotnet:2.2-sdk AS build-env

WORKDIR /app

COPY ./util/*/*.csproj /src/csproj-files/
COPY ./SmartBuildingEdge/modules/SensorSimulatorApi/*.csproj /src/csproj-files/

COPY . ./

RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app/

COPY --from=build-env /app/out ./

RUN useradd -ms /bin/bash moduleuser
USER moduleuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080

ENTRYPOINT ["dotnet", "SensorSimulatorApi.dll"]