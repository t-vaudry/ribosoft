# Ribosoft

Ribosoft is a web service to design different types of trans-acting conventional and allosteric ribozymes. Ribosoft uses template secondary structures that can be submitted by users to design ribozymes in accordance with parameters provided by the user. The generated designs specifically target a transcript (or, generally, an RNA sequence) given by the user.

URL : http://ribosoft2.fungalgenomics.ca

![Ribosoft2 FungalGenomics Uptime](https://github.com/t-vaudry/ribosoft/workflows/Ribosoft2%20FungalGenomics%20Uptime/badge.svg)
![Deployment to FungalGenomics](https://github.com/t-vaudry/ribosoft/workflows/Deployment%20to%20FungalGenomics/badge.svg?branch=master)
[![codecov](https://codecov.io/gh/t-vaudry/ribosoft/branch/develop/graph/badge.svg?token=nExWlRXew4)](https://codecov.io/gh/t-vaudry/ribosoft)
[![CodeFactor](https://www.codefactor.io/repository/github/t-vaudry/ribosoft/badge?s=f9a38174a3b592c768d1bfefc6316c85fb6925d3)](https://www.codefactor.io/repository/github/t-vaudry/ribosoft)
![Publish Docker image to GitHub Packages](https://github.com/t-vaudry/ribosoft/workflows/Publish%20Docker%20image%20to%20GitHub%20Packages/badge.svg)
![Publish RibosoftAlgo Nuget Package](https://github.com/t-vaudry/ribosoft/workflows/Publish%20RibosoftAlgo%20Nuget%20Package/badge.svg)
![Ribosoft .NET Core Builds](https://github.com/t-vaudry/ribosoft/workflows/Ribosoft%20.NET%20Core%20Builds/badge.svg)
![RibosoftAlgo CMake Builds](https://github.com/t-vaudry/ribosoft/workflows/RibosoftAlgo%20CMake%20Builds/badge.svg)
[![Robot Framework Testing](https://github.com/t-vaudry/ribosoft/actions/workflows/robot.yml/badge.svg)](https://github.com/t-vaudry/ribosoft/actions/workflows/robot.yml)

## Platform Support

- ✅ **Linux (Ubuntu)**: Primary development platform with C++23 modernization and automated builds
- 🚧 **macOS**: Disabled for modernization - will be rebuilt for Apple Silicon (arm64) natively  
- 🚧 **Windows**: Disabled for modernization - requires ViennaRNA dependency rebuild

> **Development Focus**: Currently focusing on Ubuntu/Linux builds with C++23 modernization. macOS and Windows support will be reintroduced with proper native architecture support (Apple Silicon arm64 for macOS) and updated dependencies as part of the modernization effort.

## Building

Prerequisites:

- mono (http://www.mono-project.com/download/)
- .NET 5.0 (https://www.microsoft.com/net/learn/get-started)
- cmake >=3.5 (https://cmake.org/download/)
- Visual Studio 2019 with .NET Core development enabled 

RibosoftAlgo uses CMake and can be built with any CMake-supported generator:

```sh
$ cd RibosoftAlgo/build
$ cmake .. -G "Visual Studio 16 2019" # set up the project with your favourite generator
$ msbuild # or make, or open the vcxproj, etc...
```

Ribosoft uses a Visual Studio .sln project and should be opened with Visual Studio 2017 or above.

```sh
$ dotnet build Ribosoft.sln # specifying Ribosoft.sln is optional
$ dotnet run # will run the actual Ribosoft project
```

## Unit tests

### RibosoftAlgo (Catch/C++)

Catch2 (https://github.com/catchorg/Catch2) has been integrated for unit testing the C++ library. Refer to its documentation for instructions on how to use it. 2 sample tests have been written under `test/main.cpp`.

A new target called `tests` was added which builds an executable that can run tests. It can be built with `make` or via whatever build system is being used.

```
$ RibosoftAlgo/build/bin/tests
===============================================================================
All tests passed (4 assertions in 2 test cases)
```

### Ribosoft (xUnit/C#)

The xUnit project is called Ribosoft.Tests. You can run it as follows:

```sh
$ dotnet test Ribosoft.Tests # Ribosoft.Tests is optional
```

## Usage

For more information on the usage of Ribosoft, please visit our Wiki for more setup.

## Docker Support

Ribosoft has been published as a docker application that can be used locally to run the service for larger jobs. This can help have control over the resources allocated to your service, and succeed in larger queries.

To achieve the proper configuration, Docker Compose is used.

### Docker Compose

Below is an example docker-compose.yml file.

```yaml
version: '3.8'

volumes:
  pgdata:

services:
  db:
     image: postgres:latest
     container_name: db
     restart: always
     ports:
       - 5432:5432
     environment:
       POSTGRES_USER: postgres
       POSTGRES_PASSWORD: postgres
       POSTGRES_DB: ribosoft
     volumes:
       - pgdata:/var/lib/postgresql/data

  ribosoft:
    image: tvaudryread/ribosoft:latest
    container_name: ribosoft
    ports:
      - 5001:80
    environment:
      ConnectionStrings__NpgsqlConnection: "Host=db;Port=5432;Username=postgres;Password=postgres;Database=ribosoft;Pooling=true;"
      DB_CONTEXT: NpgsqlDbContext
      EntityFrameworkProvider: Npgsql
      NODE_ENV: production
    depends_on:
      - "db"
```

The docker compose command to start the service is;

`docker-compose up`

The service will run at http://localhost:5001/

## License
[GNU General Public License v3.0](https://choosealicense.com/licenses/gpl-3.0/)
