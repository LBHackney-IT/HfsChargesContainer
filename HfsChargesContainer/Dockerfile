FROM mcr.microsoft.com/dotnet/sdk:6.0

WORKDIR /app

COPY . ./

RUN dotnet publish -c Release -o out

EXPOSE ${PORT:-3000}
CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/HfsChargesContainer.dll
