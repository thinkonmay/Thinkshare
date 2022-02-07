sudo apt -y update
sudo apt -y upgrade
sudo apt search -y golang-go
sudo apt search -y gccgo-go
sudo apt install -y golang-go
sudo apt install -y git

git clone https://github.com/pion/turn
cd turn/examples/turn-server/log
while true; do
./log -public-ip $PUBLIC_IP -users $TURN_USERNAME=$TURN_PASSWORD
done
