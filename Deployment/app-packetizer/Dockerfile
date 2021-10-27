#use nginx as final image
FROM nginx AS final
WORKDIR /usr/share/nginx/html
COPY /AdvancedInstaller/Projects/RemoteApp/RemoteApp-ThinkmayClientSetup-SetupFiles/ThinkmayClientSetup.msi  /usr/share/nginx/html
COPY /AdvancedInstaller/Projects/ThinkmayServer/ThinkmayServer-ThinkmayServer-SetupFiles/ThinkmayServer.msi  /usr/share/nginx/html