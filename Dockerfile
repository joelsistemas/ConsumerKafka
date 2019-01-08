FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["SimpeConsumerSMBKafka/SimpeConsumerSMBKafka.csproj", "SimpeConsumerSMBKafka/"]
RUN dotnet restore "SimpeConsumerSMBKafka/SimpeConsumerSMBKafka.csproj"
COPY . .
WORKDIR "/src/SimpeConsumerSMBKafka"
RUN dotnet build "SimpeConsumerSMBKafka.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SimpeConsumerSMBKafka.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SimpeConsumerSMBKafka.dll"]