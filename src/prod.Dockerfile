FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build-env
WORKDIR /app
# EXPOSE 80 # comment this out because I expose in docker-compose for consistancy

# Copy csproj and restore as distinct layers
WORKDIR /src
COPY . .

RUN dotnet restore
WORKDIR /src/websocketschat.Web
RUN dotnet publish "websocketschat.Web.csproj" -c Release -o /app

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
WORKDIR /app
COPY --from=build-env /app .
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "websocketschat.Web.dll"]
