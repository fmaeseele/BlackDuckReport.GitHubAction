# Set the base image as the .NET 9.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
COPY . ./
RUN dotnet publish ./src/BlackDuckReport.GitHubAction.csproj -c Release -o out --no-self-contained

# Label the container
LABEL maintainer="François Maeseele <francois.maeseele@gmail.com>"
LABEL repository="https://github.com/fmaeseele/BlackDuckReport.GitHubAction"
LABEL homepage="https://github.com/fmaeseele/BlackDuckReport.GitHubAction"

# Label as GitHub action
LABEL com.github.actions.name="BlackDuck Security Scan report generator"
LABEL com.github.actions.description="A Github action that generate the BlackDuck Security Scan report"
LABEL com.github.actions.icon="sliders"
LABEL com.github.actions.color="purple"

# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/sdk:9.0
COPY --from=build-env /out .
ENTRYPOINT [ "dotnet", "/BlackDuckReport.GitHubAction.dll" ]
