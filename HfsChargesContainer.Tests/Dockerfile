FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine

ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

COPY . .

RUN dotnet build ./HfsChargesContainer.Tests/HfsChargesContainer.Tests.csproj

CMD dotnet test
