# How to use mailcatcher for testing mail sending feature.

This is a demo of how to use mailcatcher for testing mail sending feature using a mailcatcher docker image and a simple mail sending sample program using node.js and nodemailer package.

## Basic usage

#### 1. run mailcatcher with docker

First, run mailcatcher with two public ports: 1080 for Web UI and 1025 for SMTP.

```bash:
$ docker run -d --rm -p 1080:1080 -p 1025:1025 --name mailcatcher schickling/mailcatcher
```

#### 2. send mail to mailcatcher server 

Then, run your program which is configured to send mail to 1025 port on the mailcatcher server.

```bash:
# one-time initialization
$ npm install
# run the sample program which sends a mail message to port 1025 on localhost and exits.
$ node index.js
```

