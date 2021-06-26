FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

LABEL author="Arthur Safiullin"

WORKDIR /app
# EXPOSE 80 # comment this out because I expose in docker-compose for consistancy

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src

COPY ./websocketschat.Core/websocketschat.Core.csproj/ ./websocketschat.Core/
COPY ./websocketschat.Database.Models/websocketschat.Database.Models.csproj/ ./websocketschat.Database.Models/
COPY ./websocketschat.Database.Repositories/websocketschat.Database.Repositories.csproj/ ./websocketschat.Database.Repositories/
COPY ./websocketschat.Database.Context/websocketschat.Database.Context.csproj/ ./websocketschat.Database.Context/
COPY ./websocketschat.Datatransfer.HttpDto/websocketschat.Datatransfer.HttpDto.csproj/ ./websocketschat.Datatransfer.HttpDto/
COPY ./websocketschat.Web/websocketschat.Web.csproj/ ./websocketschat.Web/
COPY ./websocketschat.sln ./

RUN dotnet restore
COPY ./ ./

WORKDIR /src/websocketschat.Web
RUN dotnet build "websocketschat.Web.csproj" -o app/build/

FROM build AS publish
RUN dotnet publish "websocketschat.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "websocketschat.Web.dll"]