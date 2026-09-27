FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CleanArch.sln Directory.Build.props global.json ./
COPY src/CleanArch.Domain/*.csproj src/CleanArch.Domain/
COPY src/CleanArch.Application/*.csproj src/CleanArch.Application/
COPY src/CleanArch.Infrastructure/*.csproj src/CleanArch.Infrastructure/
COPY src/CleanArch.Api/*.csproj src/CleanArch.Api/
RUN dotnet restore src/CleanArch.Api/CleanArch.Api.csproj

COPY src/ src/
RUN dotnet publish src/CleanArch.Api/CleanArch.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CleanArch.Api.dll"]
