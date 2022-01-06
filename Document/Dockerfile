# use ubuntu as doxygen build image
FROM ubuntu AS build
RUN apt-get update && apt-get install -y doxygen



# make app 
RUN mkdir /app
RUN mkdir /build

WORKDIR /build

#copy doxyfile to build image
COPY . .

#install doxygen

# run doxygen to generate html document
RUN doxygen Document/doxyfile

#use nginx as final image
FROM nginx AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app /usr/share/nginx/html/document