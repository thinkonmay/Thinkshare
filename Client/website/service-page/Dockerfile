FROM node:16.8.0
WORKDIR /app

COPY ["package.json", "package-lock.json*", "./"]

RUN npm install --production

COPY . .

ENV PORT=80

ENV REMOTE_URL=remote.thinkmay.net
ENV HOST_URL=host.thinkmay.net

RUN echo $REMOTE_URL > public/REMOTE.js
RUN echo $HOST_URL > public/API.js
CMD [ "npm", "start" ]
