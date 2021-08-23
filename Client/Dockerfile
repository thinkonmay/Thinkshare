FROM arm64v8/nginx:alpine

WORKDIR /usr/share/nginx/html

COPY src/* ./

ENV PORT 80
ENV PATH_PREFIX /

CMD /bin/sh -c "sed -i -e 's/listen.*80;/listen '${PORT}';/g' -e 's|location /|location '${PATH_PREFIX}'|g' -e 's|root.*/usr/share/nginx/html.*|alias /usr/share/nginx/html/;|g' /etc/nginx/conf.d/default.conf && exec nginx -g 'daemon off;'"

# Patch index.html to fetch latest version of javascript source
RUN sed -i 's|script src="\(.*\)?ts=.*"|script src="\1?ts='$(date +%s)'"|g' index.html