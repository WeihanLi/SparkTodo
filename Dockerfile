FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
# use forward headers
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
LABEL Maintainer="WeihanLi"
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# install dotnet tool
RUN dotnet tool install --global dotnet-dump
RUN dotnet tool install --global dotnet-gcdump
RUN dotnet tool install --global dotnet-counters
RUN dotnet tool install --global dotnet-stack
RUN dotnet tool install --global dotnet-trace

COPY SparkTodo.Shared/SparkTodo.Shared.csproj SparkTodo.Shared/
COPY SparkTodo.API/SparkTodo.API.csproj SparkTodo.API/
RUN dotnet restore SparkTodo.API/SparkTodo.API.csproj

# copy everything and build
COPY . .

WORKDIR /app/SparkTodo.API
RUN dotnet publish -c Release -o out

# build runtime image
FROM base AS final
COPY --from=build-env /root/.dotnet/tools /root/.dotnet/tools
ENV PATH="/root/.dotnet/tools:${PATH}"
WORKDIR /app
COPY --from=build-env /app/SparkTodo.API/out .
ENTRYPOINT ["dotnet", "SparkTodo.API.dll"]
