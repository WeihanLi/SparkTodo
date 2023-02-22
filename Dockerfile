FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview AS base
LABEL Maintainer="WeihanLi"
# use forward headers
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
# specific http port
ENV ASPNETCORE_HTTP_PORTS=80

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build-env
WORKDIR /app

ENV HUSKY=0

# install dotnet tool
RUN dotnet tool install --global dotnet-dump
RUN dotnet tool install --global dotnet-gcdump
RUN dotnet tool install --global dotnet-counters
RUN dotnet tool install --global dotnet-stack
RUN dotnet tool install --global dotnet-trace
RUN dotnet tool install --global dotnet-execute
RUN dotnet tool install --global dotnet-httpie
RUN dotnet tool install --global dotnet-runtimeinfo

COPY SparkTodo.Shared/SparkTodo.Shared.csproj SparkTodo.Shared/
COPY SparkTodo.API/SparkTodo.API.csproj SparkTodo.API/
COPY Directory.Build.props ./
RUN dotnet restore SparkTodo.API/SparkTodo.API.csproj

# copy everything and build
COPY . .

WORKDIR /app/SparkTodo.API
RUN dotnet publish -o out

# build runtime image
FROM base AS final
COPY --from=build-env /root/.dotnet/tools /dev/.dotnet/tools
ENV PATH="/dev/.dotnet/tools:${PATH}"
WORKDIR /app
# USER app
COPY --from=build-env /app/SparkTodo.API/out .
ENTRYPOINT ["dotnet", "SparkTodo.API.dll"]
