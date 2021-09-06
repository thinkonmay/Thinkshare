FROM nginx AS build
RUN rmdir -rf /usr/share/nginx/html
RUN mkdir /usr/share/nginx/html
WORKDIR /usr/share/nginx/html

RUN mkdir Host
RUN mkdir Slave

COPY Host/html /usr/share/nginx/html/Host
COPY Slave/html /usr/share/nginx/html/Slave
