FROM mcr.microsoft.com/dotnet/sdk:latest
EXPOSE 80
EXPOSE 443

WORKDIR /app
COPY . /app

RUN dotnet dev-certs https --trust
ENTRYPOINT dotnet watch run --urls=http://*:80