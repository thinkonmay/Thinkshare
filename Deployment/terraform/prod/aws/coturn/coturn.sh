apt-get -y update
apt-get install coturn
echo "TURNSERVER_ENABLED=1" >> /etc/default/coturn
turnserver -a -o -v -n -u coturnuser:coturnpassword -p 3478 -r someRealm --no-dtls --no-tls