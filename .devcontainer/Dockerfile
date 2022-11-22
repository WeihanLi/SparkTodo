FROM mcr.microsoft.com/dotnet/sdk:7.0
# install dependencies

# install dotnet tool
RUN dotnet tool install --global dotnet-dump
RUN dotnet tool install --global dotnet-gcdump
RUN dotnet tool install --global dotnet-counters
RUN dotnet tool install --global dotnet-stack
RUN dotnet tool install --global dotnet-trace
RUN dotnet tool install --global dotnet-execute
RUN dotnet tool install --global dotnet-httpie
ENV PATH="/root/.dotnet/tools:${PATH}"
