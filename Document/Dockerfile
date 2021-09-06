# use ubuntu as doxygen build image
FROM ubuntu:latest AS build
# make app 
RUN mkdir /app
RUN mkdir /build

WORKDIR /build

#copy doxyfile to build image
COPY . .

#install doxygen
RUN apt install doxygen

# run doxygen to generate html document
RUN doxygen Document/slavedoxy
RUN doxygen Document/hostdoxy

#use nginx as final image
FROM nginx AS final
WORKDIR /usr/share/nginx/html

COPY --from=build /app .
