# syntax=docker/dockerfile:1

# Comments are provided throughout this file to help you get started.
# If you need more help, visit the Dockerfile reference guide at
# https://docs.docker.com/go/dockerfile-reference/

# Want to help us make this template better? Share your feedback here: https://forms.gle/ybq9Krt8jtBL3iCk7

################################################################################

# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md

# Create a stage for building the application.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

WORKDIR /source/Pizzeria

COPY . .

RUN dotnet publish -c Release -o /app

# If you need to enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
################################################################################
# Create a new stage for running the application that contains the minimal
# runtime dependencies for the application. This often uses a different base
# image from the build stage where the necessary files are copied from the build
# stage.
#
# The example below uses an aspnet alpine image as the foundation for running the app.
# It will also use whatever happens to be the most recent version of that tag when you
# build your Dockerfile. If reproducability is important, consider using a more specific
# version (e.g., aspnet:7.0.10-alpine-3.18),
# or SHA (e.g., mcr.microsoft.com/dotnet/aspnet@sha256:f3d99f54d504a21d38e4cc2f13ff47d67235efeeb85c109d3d1ff1808b38d034).

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS automate-test
WORKDIR /source/TestProject1

COPY ./TestProject1/*.csproj ./TestProject1/
RUN dotnet restore ./TestProject1

RUN dotnet build ./TestProject1 --configuration Debug

COPY . ./ 

RUN dotnet test --no-build --verbosity normal

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS development
COPY . /source
WORKDIR /source/Pizzeria
CMD ["dotnet", "run", "--no-launch-profile", "--urls", "http://*:8080"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Copy everything needed to run the app from the "build" stage.
COPY --from=build /app .

# Switch to a non-privileged user (defined in the base image) that the app will run under.
# See https://docs.docker.com/go/dockerfile-user-best-practices/
# and https://github.com/dotnet/dotnet-docker/discussions/4764
USER $APP_UID

ENTRYPOINT ["dotnet", "Pizzeria.dll"]
