
sudo kubectl apply -f ./deployment/systemdb.yaml

sudo kubectl apply -f ./configmap/dbmanagerconfig.yaml
sudo kubectl apply -f ./configmap/dbconfig.yaml

sudo kubectl apply -f ./ingress/cert-manager/issuer.yaml
sudo kubectl apply -f ./ingress/ingress.yaml

sudo kubectl apply -f ./volume/volume-class.yaml
sudo kubectl apply -f ./volume/volume.yaml
sudo kubectl apply -f ./volume/volume-claim.yaml


sudo kubectl apply -f ./deployment/dbmanager.yaml
sudo kubectl apply -f ./deployment/conductor.yaml
sudo kubectl apply -f ./deployment/resources.yaml
sudo kubectl apply -f ./deployment/signalling.yaml
sudo kubectl apply -f ./deployment/slavemanager.yaml
sudo kubectl apply -f ./deployment/document.yaml

sudo kubectl apply -f ./logging/namespace.yaml
sudo kubectl apply -f ./logging/elasticsearch.yaml
sudo kubectl apply -f ./logging/elastic_svc.yaml
sudo kubectl apply -f ./logging/fluentd.yaml
sudo kubectl apply -f ./logging/ingress.yaml
sudo kubectl apply -f ./logging/kibana.yaml

