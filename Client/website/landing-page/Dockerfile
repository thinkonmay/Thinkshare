FROM nginx AS final

#clear default nginx static file
RUN rm -rf /usr/share/nginx/html
RUN mkdir /usr/share/nginx/html

#set static folder as workdir
WORKDIR /usr/share/nginx/html

# copy all neccessary folder and subfolder
COPY . .
