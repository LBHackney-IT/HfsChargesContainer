FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /code
COPY . .

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app
COPY --from=build /code/out/ .

CMD dotnet ./HfsChargesContainer.dll
