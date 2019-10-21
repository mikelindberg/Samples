FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app

COPY /util/ /util/
COPY /modules/Client/ ./

RUN dotnet restore
RUN dotnet publish -c Release -o /app/out/

FROM microsoft/dotnet:2.2-runtime-stretch-slim
WORKDIR /app/
COPY --from=build-env /app/out ./

RUN useradd -ms /bin/bash moduleuser
RUN chmod -R 777 /app/
USER moduleuser

ENTRYPOINT ["dotnet", "Client.dll"]