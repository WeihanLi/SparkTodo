FROM microsoft/dotnet:2.1-sdk-alpine AS build-env
WORKDIR /app

COPY SparkTodo.API/SparkTodo.API.csproj SparkTodo.API/
RUN dotnet restore SparkTodo.API/SparkTodo.API.csproj

# copy everything and build
COPY . .
RUN dotnet publish -c Release -o out

# build runtime image
FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine
LABEL Maintainer="WeihanLi"
WORKDIR /app
COPY --from=build-env /app/SparkTodo.API/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "SparkTodo.API.dll"]
