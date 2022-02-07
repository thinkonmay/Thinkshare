export DEBIAN_FRONTEND=noninteractive
apt -y update
apt -y upgrade

apt search -y  golang-go \
               gccgo-go

apt install -y golang-go \
               git

rm -rf turn
git clone https://github.com/pion/turn
cd turn/examples/turn-server/log

if [ -z $TURN_USERNAME ]; then
    export TURN_USERNAME=username
    echo "using default username"
fi

if [ -z $TURN_PASSWORD ]; then
    export TURN_PASSWORD=password
    echo "using default password"
fi

if [ -z $PUBLIC_IP ]; then
    export PUBLIC_IP=0.0.0.0
    echo "using default ip"
fi

go build

while true; do
echo "starting turn server in infinite loop"
./log -public-ip $PUBLIC_IP -users $TURN_USERNAME=$TURN_PASSWORD
echo "turn server terminated, restarting"
sleep 3
done &