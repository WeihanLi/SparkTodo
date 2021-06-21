FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build-env
WORKDIR /app

# install dotnet tool
RUN dotnet tool install --global dotnet-dump
RUN dotnet tool install --global dotnet-gcdump
RUN dotnet tool install --global dotnet-counters

COPY SparkTodo.Shared/SparkTodo.Shared.csproj SparkTodo.Shared/
COPY SparkTodo.API/SparkTodo.API.csproj SparkTodo.API/
RUN dotnet restore SparkTodo.API/SparkTodo.API.csproj

# copy everything and build
COPY . .

WORKDIR /app/SparkTodo.API
RUN dotnet publish -c Release -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
LABEL Maintainer="WeihanLi"
WORKDIR /app
COPY --from=build-env /app/SparkTodo.API/out .
COPY --from=build-env /root/.dotnet/tools /root/.dotnet/tools
ENV PATH="/root/.dotnet/tools:${PATH}"
EXPOSE 80
ENTRYPOINT ["dotnet", "SparkTodo.API.dll"]
