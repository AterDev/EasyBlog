# dapr docker compose 配置示例

asp.net core + dapr docker compose配置示例，两个asp.net core 服务。


```yml
version: '3.4'

networks:
  dusi:

services:
  redis:
    image: redis:7-alpine
    restart: always
    networks:
      - dusi
    ports:
      - "6379:6379"

  api:
    image: ghcr.io/aterdev/dusi-api:latest
    container_name: dusi_api
    environment:
      - ConnectionStrings__Default=${ASPNETCORE_CONNECTIONSTRINGS}
      - Azure__BlobConnectionString=${AZURE_BLOB_CONNECTIONSTRING}
    restart: always
    networks:
      - dusi
    ports:
      - "50002:50001"
      - "5200:80"
      
  api-dapr:
    image: "daprio/daprd:edge"
    restart: always
    command:
      [
        "./daprd",
        "-app-id","api",
        "-app-port","80",
        "-components-path","/components"
      ]
    volumes:
      - "./components/:/components"
    depends_on:
      - redis
      - api
    network_mode: "service:api"

  task:
    image: ghcr.io/aterdev/dusi-task:latest
    container_name: dusi_task
    environment:
      - ConnectionStrings__Default=${ASPNETCORE_CONNECTIONSTRINGS}
      - Azure__BlobConnectionString=${AZURE_BLOB_CONNECTIONSTRING}
    restart: always
    networks:
      - dusi
    ports:
      - "50003:50001"
      - "5201:80"
      
  task-dapr:
    image: "daprio/daprd:edge"
    restart: always
    command:
      [
        "./daprd",
        "-app-id","task",
        "-app-port","80",
        "-components-path","/components"
      ]
    volumes:
      - "./components/:/components"
    depends_on:
      - redis
      - task
    network_mode: "service:task"

  watchtower:
    image: containrrr/watchtower
    container_name: watchtower
    restart: always
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - /root/.docker/config.json:/config.json
    command: 
      [
        'dusi_task','dusi_api',
        '--interval','30',
        '--cleanup'
      ]
```