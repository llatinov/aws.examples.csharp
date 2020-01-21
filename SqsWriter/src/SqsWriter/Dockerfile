FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY SqsWriter.csproj /src/SqsWriter.csproj
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /pub

FROM base AS final
WORKDIR /app
COPY --from=build /pub .
ENTRYPOINT ["dotnet", "SqsWriter.dll"]