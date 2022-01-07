sudo docker build ./Host -f ./Host/Signalling/Dockerfile.development   -t pigeatgarlic/signalling:development &
sudo docker build ./Host -f ./Host/Conductor/Dockerfile.development   -t pigeatgarlic/conductor:development &
sudo docker build ./Host -f ./Host/Authenticator/Dockerfile.development   -t pigeatgarlic/authenticator:development &
sudo docker build ./Host -f ./Host/MetricCollector/Dockerfile.development   -t pigeatgarlic/metric-collector:development &
sudo docker build ./Host -f ./Host/SystemHub/Dockerfile.development   -t pigeatgarlic/autoscaling:development &
sudo docker build ./Host -f ./Host/AutoScaling/Dockerfile.development   -t pigeatgarlic/systemhub:development &

sleep 1m

sudo docker push  pigeatgarlic/signalling:development
sudo docker push  pigeatgarlic/conductor:development
sudo docker push  pigeatgarlic/authenticator:development
sudo docker push  pigeatgarlic/metric-collector:development
sudo docker push  pigeatgarlic/autoscaling:development
sudo docker push  pigeatgarlic/systemhub:development