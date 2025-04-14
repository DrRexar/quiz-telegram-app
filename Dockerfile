FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5295
ENV ASPNETCORE_URLS=http://+:5295

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["QuizTelegramApp.csproj", "./"]
RUN dotnet restore "QuizTelegramApp.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "QuizTelegramApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QuizTelegramApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuizTelegramApp.dll"] 