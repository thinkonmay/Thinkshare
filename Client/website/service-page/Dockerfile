FROM node:16.8.0
ENV NODE_ENV=production

WORKDIR /app

COPY ["package.json", "package-lock.json*", "./"]

RUN npm install --production

COPY . .

ENV PORT=80

ENV HOST_URL=host.thinkmay.net
RUN echo $HOST_URL > public/API.js
CMD [ "npm", "start" ]
