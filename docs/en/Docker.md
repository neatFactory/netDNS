# netDNS

![LOGO](https://raw.githubusercontent.com/neatFactory/netDNS/refs/heads/main/docs/assets/logo/64.png)

[![Apache licensed][9]][10]
[![Docker][3]][4] 

[3]: https://img.shields.io/docker/image-size/aicrosoft/netdns/latest
[4]: https://hub.docker.com/r/aicrosoft/netdns
[9]: https://img.shields.io/badge/license-Apache-blue.svg
[10]: LICENSE

A plugin for JobAgent that implements DDNS functionality.

Cloudflare's functionality is now fully operational.
> Currently, only the Cloudflare provider has been implemented.

## There are 2 ways
- Query the local IP every 5 minutes. If it is different, update it.
- The router sends a new public IP update event. If it is different, update it.


# How To Use

## Windows or Linux
unzip the plugin and then copy it to the JobAgent's Plugins path.

## Docker

Available Docker registries:
- <https://hub.docker.com/r/aicrosoft/netdns>
- <https://github.com/neatFactory/netDNS/pkgs/container/netdns>
> Visit <https://hub.docker.com/r/aicrosoft/netdns> to get the latest Docker image.

### Available Docker Versions
| Version | Type            | Description                                         |
| :------ | :-------------- | :-------------------------------------------------- |
| 1.2.3.4 | Standard        | Root privileges + No shell series                   |
| latest  | Latest Standard | Root privileges + No shell, latest                  |
| secure  | Secure          | Minimum privileges + No shell, latest               |
| debug   | Debug           | Root privileges + Shell + JobSamples plugin, latest |


### DEBUG Creation
```shell
sudo docker run -d --name ddns-de \
  -p 600:600/udp \
  -v /apps/ddns/logs:/app/logs:rw \
  -v /apps/ddns/states:/app/states:rw \
  -v /apps/ddns/plugins/netDNS/netDNS.json:/app/Plugins/netDNS/netDNS.json:ro \
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
  -p 600:600/udp \
  -v /apps/ddns/logs:/app/logs:rw \
  -v /apps/ddns/states:/app/states:rw \
  -v /apps/ddns/plugins/JobSampples:/app/Plugins/JobSampples:ro \
  -v /apps/ddns/plugins/netDNS/netDNS.json:/app/Plugins/netDNS/netDNS.json:ro \
  aicrosoft/netdns:latest

## Create a container mapping ipv4 and ipv6
sudo docker run -d --name ddns \
  -p 0.0.0.0:600:600/udp \
  -p [::]:600:600/udp \
  -v /apps/ddns/logs:/app/logs:rw \
  -v /apps/ddns/states:/app/states:rw \
  -v /apps/ddns/plugins/netDNS/netDNS.json:/app/Plugins/netDNS/netDNS.json:ro \
  aicrosoft/netdns:latest

```

### Docker Parameter Description
- The mapping of port 600 is the event notification sent by your router. It can be modified in the configuration.
- If you do not need to view logs, do not map `/app/logs`. Logs will only be retained for 30 days.
- If you do not need to view or modify the status of Jobs, do not map `/app/states`.
- If you map `/app/Plugins`, you must place the plugin content in the corresponding path on the host machine.
- You can map `/app/appsettings.json` to the corresponding configuration on the host machine.
- You must map `/app/Plugins/netDNS/netDNS.json` to the corresponding configuration on the host machine.

### netDNS.json Configuration Example
```json
{
  "DDNS": {
    "enableWatchRouteIp": true,
    "enableIntervalDetectIp": true,
    "provider": "Cloudflare",
    "interval": 300, //300  Every five minutes.
    "loginToken": "{INPUT_YOUR_LOGIN_TOKNE}",
    //"resolver": "8.8.8.8",//not support now.
    "ipUrls": [
      "https://ipecho.net/plain",
      "https://api.ipify.org",
      "https://myip.biturl.top",
      "https://api-ipv4.ip.sb/ip",
      "https://ip.3322.net/", //mainland
      "https://4.ipw.cn", //mainland
      "https://ip.qaros.com/" //mainland
    ],
    "ipStyle": "IPv4",
    "routeEventSetting": {
      "updServicePort": 600,
      "hitRegPartten": "^pppd\\[+\\d+\\]:.*local\\s+IP\\s+address",
      "ipV4RegPartten": "(?<=\\blocal\\s+IP\\s+address\\s+)(\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3})\\b",
      "ipV6RegPartten": ""
    },

    "domains": [
      {
        "name": "cnemcc.com",
        "subNames": [ "dev", "test", "debug" ]
      }
    ]
  }
}
```
- If you are in the Mainland, delete the first four `ipUrls` configurations.
- You can configure the logs on your router to be sent to the corresponding port on Docker.


# Contributing
Contributions are welcome! Feel free to submit a Pull Request.


