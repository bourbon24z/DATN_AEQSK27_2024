
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DATN/*.csproj DATN/
COPY DATN.sln .

RUN dotnet restore DATN/DATN.csproj

COPY . .

WORKDIR /src/DATN
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .


EXPOSE 5062
ENV ASPNETCORE_URLS=http://+:5062

ENTRYPOINT ["dotnet", "DATN.dll"]
