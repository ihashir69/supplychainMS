# ============================================================
# Dockerfile — recipe for packaging the app into a container
# ============================================================
#
# WHAT is a Dockerfile?
#   It's a step-by-step recipe that builds a "container" — a self-contained
#   box that has EVERYTHING the app needs to run (the .NET runtime, our
#   compiled code, etc.). Render.com reads this file and builds that box,
#   then runs it on the internet. It works the same on any computer.
#
# WHY "multi-stage"?
#   We use TWO images:
#     1. A big "sdk" image that can COMPILE C# (has all build tools).
#     2. A small "aspnet" image that just RUNS the finished app.
#   We compile in the big one, then copy ONLY the finished output into the
#   small one. Result: a much smaller, faster-to-start final container.

# ---- STAGE 1: "base" — the lightweight runtime the app finally runs on ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# The app will listen on port 8080 inside the container.
EXPOSE 8080

# ---- STAGE 2: "build" — compile the C# code ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy ONLY the project file first and restore packages.
# Docker caches this layer, so if only our code changes (not the packages),
# rebuilds are much faster — it skips re-downloading NuGet packages.
COPY ["src/SupplyChainMS/SupplyChainMS.csproj", "SupplyChainMS/"]
RUN dotnet restore "SupplyChainMS/SupplyChainMS.csproj"

# Now copy the rest of the source code and publish (compile + optimize).
COPY src/SupplyChainMS/ SupplyChainMS/
WORKDIR "/src/SupplyChainMS"
RUN dotnet publish -c Release -o /app/publish

# ---- STAGE 3: "final" — assemble the runnable container ----
FROM base AS final
WORKDIR /app
# Copy the compiled output from the build stage into this small image.
COPY --from=build /app/publish .
# Tell ASP.NET which URL/port to listen on inside the container.
ENV ASPNETCORE_URLS=http://+:8080
# The command that starts the app when the container boots.
ENTRYPOINT ["dotnet", "SupplyChainMS.dll"]
