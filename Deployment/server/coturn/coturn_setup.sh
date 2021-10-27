apt-get update
apt-get upgrade
apt install coturn


cp -f /etc/default/coturn coturn
systemctl start coturn
cp -f /etc/turnserver.conf turnserver.conf
service coturn restart