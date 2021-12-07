sudo apt-get -y update
sudo apt-get install coturn
echo "TURNSERVER_ENABLED=1" >> sudo vi /etc/default/coturn
turnserver -a -o -v -n -u <username>:<password> -p 3478 -r someRealm -X <Public_IP>/<Private_IP> --no-dtls --no-tls