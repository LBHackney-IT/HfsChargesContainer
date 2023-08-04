FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

WORKDIR /code
COPY . .

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine

WORKDIR /app
COPY --from=build /code/out/ .

CMD dotnet ./HfsChargesContainer.dll
