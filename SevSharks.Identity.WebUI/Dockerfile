FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 44350

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Identity.WebUI/Identity.WebUI.csproj Identity.WebUI/
COPY Identity.DataAccess/Identity.DataAccess.csproj Identity.DataAccess/
COPY Identity.BusinessLogic/Identity.BusinessLogic.csproj Identity.BusinessLogic/
COPY ./NuGet.Config .
RUN dotnet restore Identity.WebUI/Identity.WebUI.csproj
COPY . .
WORKDIR /src/Identity.WebUI
RUN dotnet build Identity.WebUI.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Identity.WebUI.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Identity.WebUI.dll"]
