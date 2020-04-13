# How to create a Node.js-based docker images

## Basics

* Base Image
  * Use node:lts-slim as the base image, not node:lts-alpine
  * reason: [DockerでNode.jsアプリをイイ感じに保つ4つの方法](https://www.creationline.com/lab/29422)
* Applicatio root
  * Make "/app" or "/<app name>" as your application root
* Data root
  * Make "/app/data" or "/<app name>/data" as your data root (which is mapped to a volume) 

## Express

```bash:
$ docker run -d -p 8080:8080 --rm --name app tmj/express-app
```

