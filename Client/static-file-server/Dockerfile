FROM nginx AS build

RUN rm /etc/nginx/nginx.conf /etc/nginx/conf.d/default.conf

COPY ./nginx/ /etc/nginx/

FROM build AS run

RUN mkdir /wwwroot/
WORKDIR /wwwroot/

COPY ./wwwroot .