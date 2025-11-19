# netDNS

![LOGO](./docs/assets/logo/128.png)

[![Apache licensed][9]][10]
[![Docker][3]][4] 

[3]: https://img.shields.io/docker/image-size/aicrosoft/netdns/latest
[4]: https://hub.docker.com/r/aicrosoft/netdns
[9]: https://img.shields.io/badge/license-Apache-blue.svg
[10]: LICENSE

A plugin for JobAgent that implements DDNS functionality.

# How To Use

## Windows or Linux
unzip the plugin and then copy it to the JobAgent's Plugins path.

## Docker

Available Docker registries:
- <https://hub.docker.com/r/aicrosoft/netdns>
- <https://github.com/neatFactory/netDNS/pkgs/container/netdns>
> Visit <https://hub.docker.com/r/aicrosoft/netdns> to get the latest Docker image.


### DEBUG Creation
```shell
sudo docker run -d \
  --name ddns-de \
  aicrosoft/netdns:debug
```

### PRODUCTION Creation
```shell
## Create an empty directory on the host machine and assign permissions.
sudo mkdir -p /apps/ddns/logs /apps/ddns/plugins /apps/ddns/states
sudo chmod -R 777 /apps/ddns
sudo chown -R 65532:65532 /apps/ddns

## Ensure the parent directory exists.
mkdir -p /apps/ddns/plugins/netDNS
## Create an empty file (or copy the JSON file you have prepared).
touch /apps/ddns/plugins/netDNS/netDNS.json
## Modify your configuration file

## Create a container
sudo docker run -d --name ddns \
  -v /apps/ddns/logs:/app/logs:rw \
  -v /apps/ddns/states:/app/states:rw \
  -v /apps/ddns/plugins/netDNS/netDNS.json:/app/Plugins/netDNS/netDNS.json:ro \
  aicrosoft/netdns:latest
```

#### Docker参数说明
- 如果不用查看日志，不用映射/app/logs。只会保留30天内的日志。
- 如果不用查看或修改Job的状态，不用映射/app/states。
- 映射了/app/Plugins就必须把插件内容放到宿主机上对应的路径。
- 可以映射/app/appsettings.json到宿主机上对应的配置上。
- 必须映射/app/Plugins/netDNS/netDNS.json到宿主机上对应的配置上。

