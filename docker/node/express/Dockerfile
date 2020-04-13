FROM node:lts-slim

# create app directory
WORKDIR /${APP:-app}

# install dependencies (use yarn)
COPY package.json yarn.lock ./
RUN yarn install 

# copy app source
COPY . .

# expose PORT
EXPOSE ${PORT:-8080}

# run node app
CMD ["node", "index.js"]
