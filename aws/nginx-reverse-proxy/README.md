# How to set up Nginx as an SSL terminator and reverse proxy server on Amazon Linux 2

This document explains how to setup Nginx and maildev. Nginx acts as the reverse proxy
server for maildev and terminates SSL.

## 1. Setup procedure

### (1) Setup Amazon Linux 2 instance

* create a new Amazon Linux 2 instance on AWS.
* install docker
* install docker-compose
* create and apply security group (open 22 port for SSH and 443 for SSL)

### (2) Create an SSL server certificate and a private key

* use certgen.sh to create a certificate and a private key
* name them server_crt.pem and server_key.pem respectively

### (3) Create a docker-compose file

* create docker-compose.yml as shown below
* run it (`"docker-compose up -d"`)

```yml:docker-compose.yml
version: '3.7'

services:
  maildev:
    image: maildev/maildev:1.1.0
    ports:
      - "1025:25"
  nginx:
    image: nginx:1.18.0-alpine
    ports:
      - "443:443"
    volumes:
#      - ./nginx/conf.d/default.conf:/etc/nginx/conf.d/default.conf
#      - ./nginx/certs/:/etc/nginx/cert
```

### (4) Configure Nginx

* create ./nginx/certs directory and copy the certificate and private key files into it.
* copy `/etc/nginx/conf.d/default.conf` in Nginx container to `./nginx/conf.d` direcctory on local PC.

The resulting directory structure is like this:

```
├── docker-compose.yml
├── key2.pem
└── nginx
    ├── certs
    │   ├── server_crt.pem
    │   └── server_key.pem
    └── conf.d
        └── default.conf
```

Then, edit `default.conf` as shown below:

```
    listen 443 ssl;
    ssl_certificate /etc/nginx/certs/server_crt.pem;
    ssl_certificate_key /etc/nginx/certs/server_key.pem;

    location / {
        proxy_set_header        Host $host;
        proxy_set_header        X-Real-IP $remote_addr;
        proxy_set_header        X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header        X-Forwarded-Proto $scheme;
    
        # Fix the “It appears that your reverse proxy set up is broken" error.
        proxy_pass          http://maildev:80;
        proxy_read_timeout  90;
    
        proxy_redirect      http:// https://;
    }
```

Finally, uncomment the last two lines in `docker-compose.yml` to map volumes.

### (4) Confirm it

* run `docker-compose down`, and then `docker-compose up -d`
* check if you can open maildev window by opening your browser and accessing via `https://...`
* install mailx and send test mail to maildev as shown below:

```bash
$ sudo yum install mailx
$ vi ~/.mailrc
set smtp=smtp://localhost:1025
$ echo test | mail -s testmail xxx@yyy.com
```

## 2. Trouble shooting

* use `docker logs <container_id>` to check if Nginx runs correctly or not.
* use `netstat -plan` to check if the host linux properly listen on 443.
* check the security group if 443 port is opened or not.