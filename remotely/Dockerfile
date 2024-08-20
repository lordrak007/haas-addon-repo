# temporary container ad downloader
FROM alpine:3.15 as downloader

ARG REMOTELY_VERSION=2024.08.07.0819

RUN apk add unzip
WORKDIR /app/
# testing for local install
#COPY /r/remotely.zip /app/
#RUN unzip remotely.zip -d src && rm remotely.zip

RUN set -e \
    && wget https://github.com/immense/Remotely/releases/download/v${REMOTELY_VERSION}/Remotely_Server_Linux-x64.zip -O remotely.zip \
    && unzip remotely.zip -d src \
	&& rm remotely.zip


# build main image
#FROM mcr.microsoft.com/dotnet/aspnet:7.0-jammy # verze pred 2023 a starsi
FROM mcr.microsoft.com/dotnet/aspnet:8.0
SHELL ["/bin/bash", "-c"]
EXPOSE 5000
ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_URLS="http://*:5000"

RUN \
  apt-get -y clean && \
  apt-get -y update && \
  apt-get -y install curl

WORKDIR /app
# copy downloaded build from downloaded container to main container
COPY --from=downloader /app/src /app/
COPY DockerMain.sh /src/DockerMain.sh   


# není třeba zapisovat, protože základní nastavení je sqlite a nic jineho to nebude
#RUN \
#  sed -i 's/DataSource=Remotely.db/DataSource=\/remotely-data\/Remotely.db/' /app/appsettings.json
#RUN chmod +x "/src/DockerMain.sh"
#ENTRYPOINT ["/src/DockerMain.sh"]
ENTRYPOINT ["dotnet", "Remotely_Server.dll"]
# custom healtcheck interval - every one min
HEALTHCHECK --interval=1m --timeout=3s \
  CMD curl -f http://localhost:5000/api/healthcheck || exit 1