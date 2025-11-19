# ==============================================================================
# Stage 1: Build（只做编译）
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["nuget.config", "./"]

# --- netDNS 项目 ---
# 1. 先 build 插件（输出到 /app/Plugins/netDNS    
COPY ["netDNS/netDNS.csproj", "netDNS/"]
RUN dotnet restore netDNS/netDNS.csproj --runtime linux-x64
COPY ["netDNS/", "netDNS/"]
# 发布到独立目录 /app/Plugins/netDNS
RUN dotnet build netDNS/netDNS.csproj \
    -c Release -o /app/Plugins/netDNS \
    -r linux-x64 \
    -p:PublishReadyToRun=false \
    -p:PublishSingleFile=false \
    -p:SelfContained=false 

# 在 build 容器里建好空目录并赋权
RUN mkdir -p /app/states /app/logs && \
    chmod -R 755 /app/states /app/logs && \
    chown -R 65532:65532 /app/states /app/logs

# ==============================================================================
# Stage A: jobagent
# ==============================================================================
FROM aicrosoft/jobagent:1.0.0-beta.2 as jobagent
WORKDIR /app

# 整目录复制，但 .dockerignore 里排除掉的不会进来
COPY --from=build --chown=65532:65532 /app .

USER 65532
ENTRYPOINT ["./JobAgent"]


# ==============================================================================
# Stage B: debug
# ==============================================================================
FROM aicrosoft/jobagent:1.0.0-beta.2-debug as debug
WORKDIR /app
# 整目录复制，但 .dockerignore 里排除掉的不会进来
COPY --from=build /app .
ENTRYPOINT ["./JobAgent"]
