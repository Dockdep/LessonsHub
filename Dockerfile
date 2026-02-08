# Stage 1: Build Angular
FROM node:20-alpine AS node-build
WORKDIR /src/ClientApp
COPY LessonsHub/ClientApp/package.json LessonsHub/ClientApp/package-lock.json ./
RUN npm ci
COPY LessonsHub/ClientApp/ ./
RUN npm run build

# Stage 2: Build .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build
WORKDIR /src

# Create a no-op npm shim so the csproj PublishRunWebpack target succeeds
# (Angular is already built in stage 1)
RUN echo '#!/bin/sh' > /usr/local/bin/npm && \
    echo 'exit 0' >> /usr/local/bin/npm && \
    chmod +x /usr/local/bin/npm

COPY LessonsHub/LessonsHub.csproj LessonsHub/
RUN dotnet restore LessonsHub/LessonsHub.csproj

COPY LessonsHub/ LessonsHub/

# Copy pre-built Angular output
COPY --from=node-build /src/ClientApp/dist LessonsHub/ClientApp/dist

RUN dotnet publish LessonsHub/LessonsHub.csproj -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

COPY --from=dotnet-build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "LessonsHub.dll"]
