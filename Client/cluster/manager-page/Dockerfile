FROM node:16.8.0
ENV NODE_ENV=production

WORKDIR /app

ENV CLUSTER_MANAGER_URL=http://localhost:5000



COPY ["package.json", "package-lock.json*", "./"]

RUN npm install --production

COPY . .

CMD [ "npm", "start" ]