FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
WORKDIR /src
COPY ./PixelGraph.sln ./
COPY ./PixelGraph.Common/PixelGraph.Common.csproj ./PixelGraph.Common/
COPY ./PixelGraph.CLI/PixelGraph.CLI.csproj ./PixelGraph.CLI/
RUN dotnet restore ./PixelGraph.Common/PixelGraph.Common.csproj && \
	dotnet restore ./PixelGraph.CLI/PixelGraph.CLI.csproj
COPY ./PixelGraph.Common ./PixelGraph.Common/
COPY ./PixelGraph.CLI ./PixelGraph.CLI/
WORKDIR /src/PixelGraph.CLI
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=build /publish ./
ENTRYPOINT ["./PixelGraph"]
