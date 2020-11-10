FROM mcr.microsoft.com/dotnet/core/sdk:3.1-focal AS build
EXPOSE 5432
RUN apt-get update && apt-get -y install nodejs npm

RUN apt-get -y install dirmngr gnupg apt-transport-https ca-certificates software-properties-common
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
RUN apt-add-repository 'deb https://download.mono-project.com/repo/ubuntu stable-focal main'
RUN apt-get -y install mono-complete

WORKDIR /src
COPY . .
COPY ./.docker/appsettings.json /src/Ribosoft/appsettings.json

WORKDIR /src/Ribosoft
RUN npm install
RUN node node_modules/webpack/bin/webpack.js --config webpack.config.vendor.js
RUN node node_modules/webpack/bin/webpack.js

WORKDIR "/src/Ribosoft"
RUN dotnet build "Ribosoft.csproj" -c Release
EXPOSE 80
COPY ./.docker/migrations.sh .
RUN chmod +x ./migrations.sh
CMD /bin/bash ./migrations.sh