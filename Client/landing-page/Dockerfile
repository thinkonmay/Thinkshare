FROM nginx AS final

#clear default nginx static file
RUN rm -rf /usr/share/nginx/html
RUN mkdir /usr/share/nginx/html

#set static folder as workdir
WORKDIR /usr/share/nginx/html
RUN mkdir assets
RUN mkdir remote-page-asset

# copy all neccessary folder and subfolder
COPY assets assets
COPY remote-page-asset remote-page-asset
COPY index.html .
