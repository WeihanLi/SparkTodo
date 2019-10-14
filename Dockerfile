FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build-env
WORKDIR /app

COPY SparkTodo.API/SparkTodo.API.csproj SparkTodo.API/
RUN dotnet restore SparkTodo.API/SparkTodo.API.csproj

# copy everything and build
COPY . .
RUN dotnet publish -c Release -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine
LABEL Maintainer="WeihanLi"
WORKDIR /app
COPY --from=build-env /app/SparkTodo.API/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "SparkTodo.API.dll"]
