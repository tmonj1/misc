# How to create Node.js-based docker images

## Basics

* Base image
  * use node:lts-slim as the base image, not node:lts-alpine
  * reason: [DockerでNode.jsアプリをイイ感じに保つ4つの方法](https://www.creationline.com/lab/29422)
* Application root
  * make "/app" or "/<app name>" as your application root
* Data root
  * make "/app/data" or "/<app name>/data" as your data root (which is mapped to a volume) 

## Express

```bash:
$ cd express
$ docker build . -t tmj/express-app
$ docker run -d -p 8080:8080 --rm --name app tmj/express-app
```

